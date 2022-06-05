using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Modal
{
    [RequireComponent(typeof(RectMask2D))]
    public sealed class ModalContainer : ContainerLayer, IContainerManager
    {
        private static readonly Dictionary<int, ModalContainer> InstanceCacheByTransform =
            new Dictionary<int, ModalContainer>();

        private static readonly Dictionary<string, ModalContainer> InstanceCacheByName =
            new Dictionary<string, ModalContainer>();
        

        [SerializeField] private ModalBackdrop _overrideBackdropPrefab;
        
        private readonly List<ModalBackdrop> _backdrops = new List<ModalBackdrop>();

        private readonly List<IModalContainerCallbackReceiver> _callbackReceivers =
            new List<IModalContainerCallbackReceiver>();
        //Controls the visibility of the modals
        private readonly List<Modal> _modals = new List<Modal>();
        
        //controls load and unload of resources
        private readonly List<CacheWindowItem> _modalItems = new List<CacheWindowItem>();
        
        private readonly List<string> _preloadAssetKeys = new List<string>();
        
        private ModalBackdrop _backdropPrefab;
        
        /// <summary>
        ///     True if in transition.
        /// </summary>
        public bool IsInTransition { get; private set; }

        /// <summary>
        ///     Stacked modals.
        /// </summary>
        public IReadOnlyList<Modal> Modals => _modals;


        public Window Current
        {
            get { return _modals[_modals.Count - 1]; }
        }
        
        public override int VisibleElementInLayer
        {
            get => Modals.Count;
        }

        protected override void Awake()
        {
            _callbackReceivers.AddRange(GetComponents<IModalContainerCallbackReceiver>());

            _backdropPrefab = _overrideBackdropPrefab
                ? _overrideBackdropPrefab
                : UnityScreenNavigatorSettings.Instance.ModalBackdropPrefab;
        }

        protected override void OnDestroy()
        {
            foreach (var preloadAssetKey in _preloadAssetKeys)
            {
                AddressablesManager.ReleaseAsset(preloadAssetKey);
            }
            _preloadAssetKeys.Clear();
            
            foreach (var item in _modalItems)
            {
                AddressablesManager.ReleaseAsset(item.Key);
            }
            _modalItems.Clear();
            
            InstanceCacheByName.Remove(LayerName);
            var keysToRemove = new List<int>();
            foreach (var cache in InstanceCacheByTransform)
            {
                if (Equals(cache.Value))
                {
                    keysToRemove.Add(cache.Key);
                }
            }

            foreach (var keyToRemove in keysToRemove)
            {
                InstanceCacheByTransform.Remove(keyToRemove);
            }
        }

        /// <summary>
        ///     Get the <see cref="ModalContainer" /> that manages the modal to which <see cref="transform" /> belongs.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="transform" />.</param>
        /// <returns></returns>
        public static ModalContainer Of(Transform transform, bool useCache = true)
        {
            return Of((RectTransform) transform, useCache);
        }

        /// <summary>
        ///     Get the <see cref="ModalContainer" /> that manages the modal to which <see cref="rectTransform" /> belongs.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="rectTransform" />.</param>
        /// <returns></returns>
        public static ModalContainer Of(RectTransform rectTransform, bool useCache = true)
        {
            var id = rectTransform.GetInstanceID();
            if (useCache && InstanceCacheByTransform.TryGetValue(id, out var container))
            {
                return container;
            }

            container = rectTransform.GetComponentInParent<ModalContainer>();
            if (container != null)
            {
                InstanceCacheByTransform.Add(id, container);
                return container;
            }

            return null;
        }

        /// <summary>
        /// Find the <see cref="ModalContainer" /> of <see cref="containerName" />.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static ModalContainer Find(string containerName)
        {
            if (InstanceCacheByName.TryGetValue(containerName, out var instance))
            {
                return instance;
            }

            return null;
        }
        /// <summary>
        /// Create a new <see cref="ModalContainer" /> as a layer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="layer"></param>
        /// <param name="layerType"></param>
        /// <returns></returns>
        public static ModalContainer Create(string layerName, int layer, ContainerLayerType layerType)
        {
            GameObject root = new GameObject(layerName, typeof(CanvasGroup));
            RectTransform rectTransform = root.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector3.zero;
            
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var canvasScaler = root.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            
            root.AddComponent<GraphicRaycaster>();

            ModalContainer container = root.AddComponent<ModalContainer>();

            container.CreateLayer(layerName, layer, layerType);

            if (!string.IsNullOrWhiteSpace(layerName))
            {
                InstanceCacheByName.Add(layerName, container);
            }
            
            return container;
        }
        
        /// <summary>
        ///     Add a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void AddCallbackReceiver(IModalContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Add(callbackReceiver);
        }

        /// <summary>
        ///     Remove a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void RemoveCallbackReceiver(IModalContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Remove(callbackReceiver);
        }

        /// <summary>
        /// Push new modal.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public UniTask Push(WindowOption option)
        {
            return PushTask(option);
        }

        /// <summary>
        /// Pop current modal.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        public UniTask Pop(bool playAnimation)
        {
            return PopTask(playAnimation);
        }

//string resourceKey, bool playAnimation, Action<Modal> onLoad = null,
//        bool loadAsync = true
        private async UniTask PushTask(WindowOption option)
        {
            if (option.ResourcePath == null)
            {
                throw new ArgumentNullException(nameof(option.ResourcePath));
            }

            if (IsInTransition)
            {
                throw new InvalidOperationException(
                    "Cannot transition because the screen is already in transition.");
            }

            IsInTransition = true;

            var operationResult = await AddressablesManager.LoadAssetAsync<GameObject>(option.ResourcePath);
            
            var backdrop = Instantiate(_backdropPrefab);
            backdrop.Setup((RectTransform) transform);
            _backdrops.Add(backdrop);

            var instance = Instantiate(operationResult.Value);
            var enterModal = instance.GetComponent<Modal>();
            if (enterModal == null)
            {
                throw new InvalidOperationException(
                    $"Cannot transition because the \"{nameof(Modal)}\" component is not attached to the specified resource \"{option.ResourcePath}\".");
            }

            _modalItems.Add(new CacheWindowItem(instance, option.ResourcePath));
            option.WindowCreated?.Invoke(enterModal);
            var afterLoadHandle = enterModal.AfterLoad((RectTransform) transform);
            await afterLoadHandle;

            var exitModal = _modals.Count == 0 ? null : _modals[_modals.Count - 1];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePush(enterModal, exitModal);
            }
            
            if (exitModal != null)
            {
               await  exitModal.BeforeExit(true, enterModal);
            }

            await enterModal.BeforeEnter(true, exitModal);

            // Play Animation

            await backdrop.Enter(option.PlayAnimation);

            if (exitModal != null)
            {
               await exitModal.Exit(true, option.PlayAnimation, enterModal);
            }

            await enterModal.Enter(true, option.PlayAnimation, exitModal);

            // End Transition
            _modals.Add(enterModal);
            IsInTransition = false;

            // Postprocess
            if (exitModal != null)
            {
                exitModal.AfterExit(true, enterModal);
            }

            enterModal.AfterEnter(true, exitModal);

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterPush(enterModal, exitModal);
            }
        }

        private async UniTask PopTask(bool playAnimation)
        {
            if (_modals.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot transition because there are no modals loaded on the stack.");
            }

            if (IsInTransition)
            {
                throw new InvalidOperationException(
                    "Cannot transition because the screen is already in transition.");
            }

            IsInTransition = true;

            var exitModal = _modals[_modals.Count - 1];
            var enterModal = _modals.Count == 1 ? null : _modals[_modals.Count - 2];
            var backdrop = _backdrops[_backdrops.Count - 1];
            _backdrops.RemoveAt(_backdrops.Count - 1);

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePop(enterModal, exitModal);
            }


            await exitModal.BeforeExit(false, enterModal);
            
            if (enterModal != null)
            {
                await enterModal.BeforeEnter(false, exitModal);
            }

            // Play Animation
            await exitModal.Exit(false, playAnimation, enterModal);
            if (enterModal != null)
            {
               await enterModal.Enter(false, playAnimation, exitModal);
            }

            await backdrop.Exit(playAnimation);

            // End Transition
            _modals.RemoveAt(_modals.Count - 1);
            IsInTransition = false;

            // Postprocess
            exitModal.AfterExit(false, enterModal);
            if (enterModal != null)
            {
                enterModal.AfterEnter(false, exitModal);
            }

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterPop(enterModal, exitModal);
            }

            // Unload Unused Screen
            var beforeReleaseHandle = exitModal.BeforeRelease();
            await beforeReleaseHandle;


            AddressablesManager.ReleaseAsset(_modalItems[_modalItems.Count - 1].Key);
            _modalItems.RemoveAt(_modalItems.Count - 1);
            Destroy(exitModal.gameObject);
            Destroy(backdrop.gameObject);
        }

        public UniTask Preload(string resourceKey)
        {
            _preloadAssetKeys.Add(resourceKey);
            return PreloadTask(resourceKey);
        }

        private UniTask PreloadTask(string resourceKey)
        {
            return AddressablesManager.LoadAssetAsync<GameObject>(resourceKey);
        }

        public void ReleasePreloaded(string resourceKey)
        {
            _preloadAssetKeys.Remove(resourceKey);
            AddressablesManager.ReleaseAsset(resourceKey);
        }

        public override void OnBackButtonPressed()
        {
            if (_modals.Count > 0)
            {
                Pop(true).Forget();
            }
        }

        protected override void OnCreate()
        {
        }
    }
}