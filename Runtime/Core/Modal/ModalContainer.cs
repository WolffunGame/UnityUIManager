using System;
using System.Collections.Generic;
using AddressableAssets.Loaders;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Modal
{
    [RequireComponent(typeof(RectMask2D))]
    public sealed class ModalContainer : ContainerLayer, IContainerManager<Modal>
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
        private readonly List<string> _modalItems = new List<string>();
        private readonly IAssetsKeyLoader<GameObject> _assetsKeyLoader = new AssetsKeyLoader<GameObject>();

        private readonly List<string> _preloadAssetKeys = new List<string>();

        private ModalBackdrop _backdropPrefab;

        private bool _isInTransition;
        
        /// <summary>
        ///     Stacked modals.
        /// </summary>
        public IReadOnlyList<Modal> Modals => _modals;

        public override Window Current => _modals.Count > 0 ? _modals[_modals.Count - 1] : null;

        public override int VisibleElementInLayer => Modals.Count;

        [SerializeField] private bool allowMultiple = true;

        /// <summary>
        /// Allow multiple modals can be stacked in this container. If set to false, the container will close the current modal before opening the new one.
        /// </summary>
        public bool AllowMultiple => allowMultiple;

        private void Awake()
        {
            PreSetting();
            _callbackReceivers.AddRange(GetComponents<IModalContainerCallbackReceiver>());

            _backdropPrefab = _overrideBackdropPrefab
                ? _overrideBackdropPrefab
                : UnityScreenNavigatorSettings.Instance.ModalBackdropPrefab;
        }

        private void OnDestroy()
        {
            _assetsKeyLoader.UnloadAllAssets();
            
            _preloadAssetKeys.Clear();
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
            canvas.sortingOrder = layer;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var canvasScaler = root.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(UnityEngine.Screen.currentResolution.height,
                UnityEngine.Screen.currentResolution.width);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            root.AddComponent<GraphicRaycaster>();

            ModalContainer container = root.AddComponent<ModalContainer>();

            container.CreateLayer(layerName, layer, layerType);

            if (!InstanceCacheByName.ContainsKey(layerName))
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
        public UniTask<Modal> Push(WindowOption option)
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
        
        // ReSharper disable Unity.PerformanceAnalysis
        private async UniTask<Modal> PushTask(WindowOption option)
        {
            if (string.IsNullOrEmpty(option.ResourcePath))
            {
                throw new ArgumentException("Path is null or empty.");
            }

            if (_isInTransition)
            {
                await UniTask.WaitUntil(() => !_isInTransition);
            }

            //Handle the single container
            if (!AllowMultiple)
            {
                if (Current != null)
                {
                    //if the modal has higher priority than the current modal, pop the current modal
                    if (Current.Priority < option.Priority)
                    {
                        if(_modals.Count > 0)
                        {
                            await PopTask(false);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            _isInTransition = true;

            var operationResult = await _assetsKeyLoader.LoadAssetAsync(option.ResourcePath);

            var backdrop = Instantiate(_backdropPrefab);
            backdrop.Setup((RectTransform) transform);
            _backdrops.Add(backdrop);

            var instance = Instantiate(operationResult);
            var enterModal = instance.GetComponent<Modal>();
            if (enterModal == null)
            {
                throw new InvalidOperationException(
                    $"Cannot transition because the \"{nameof(Modal)}\" component is not attached to the specified resource \"{option.ResourcePath}\".");
            }

            _modalItems.Add(option.ResourcePath);
            enterModal.Priority = option.Priority;

            option.WindowCreated.Value = enterModal;

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
                await exitModal.BeforeExit(true, enterModal);
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
            _isInTransition = false;

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

            return enterModal;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private async UniTask PopTask(bool playAnimation)
        {
            if (_modals.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot transition because there are no modals loaded on the stack.");
            }

            _isInTransition = true;

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
            _isInTransition = false;

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


            _assetsKeyLoader.UnloadAsset(_modalItems[^1]);
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
            return _assetsKeyLoader.LoadAssetAsync(resourceKey);
        }

        public void ReleasePreloaded(string resourceKey)
        {
            _preloadAssetKeys.Remove(resourceKey);
            _assetsKeyLoader.UnloadAsset(resourceKey);
        }

        public override UniTask OnBackButtonPressed()
        {
            if (_modals.Count > 0)
            {
                return Pop(true);
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// In this case the <see cref="ModalContainer" /> is created manually in Hierarchy.
        /// </summary>
        private void PreSetting()
        {
            if (!InstanceCacheByName.ContainsKey(LayerName))
            {
                SortOrder = Canvas.sortingOrder;
                LayerType = ContainerLayerType.Modal;
                InstanceCacheByName.Add(LayerName, this);
                ContainerLayerManager.Add(this);
            }
        }
    }
}