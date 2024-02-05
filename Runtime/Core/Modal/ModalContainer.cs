using System;
using System.Collections.Generic;
using AddressableAssets.Loaders;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using ZBase.Foundation.Pooling.GameObjectItem.LazyPool.Extensions;

namespace UnityScreenNavigator.Runtime.Core.Modal
{
    [RequireComponent(typeof(RectMask2D))]
    public sealed class ModalContainer : ContainerLayer, IContainerManager<Modal>
    {
        private static readonly Dictionary<int, ModalContainer> InstanceCacheByTransform = new();

        private static readonly Dictionary<string, ModalContainer> InstanceCacheByName = new();

        [SerializeField] private ModalBackdrop _overrideBackdropPrefab;

        private readonly List<ModalBackdrop> _backdrops = new();

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly List<string> _preloadAssetKeys = new();

        private readonly List<IModalContainerCallbackReceiver> _callbackReceivers = new();

        //Controls the visibility of the modals
        private readonly List<Modal> _modals = new();

        //controls load and unload of resources
        private readonly List<string> _modalItems = new();
        private readonly IAssetsKeyLoader<GameObject> _assetsKeyLoader = new AssetsKeyLoader<GameObject>();

        private ModalBackdrop _backdropPrefab;

        private bool _isInTransition;

        /// <summary>
        ///     Stacked modals.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public IReadOnlyList<Modal> Modals => _modals;

        public override Window Current => _modals.Count > 0 ? _modals[^1] : null;

        public override int VisibleElementInLayer => Modals.Count;

        [FormerlySerializedAs("allowMultiple")] [SerializeField]
        private bool _allowMultiple = true;

        /// <summary>
        /// Allow multiple modals can be stacked in this container. If set to false, the container will close the current modal before opening the new one.
        /// </summary>
        public bool AllowMultiple => _allowMultiple;

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
            _modalItems.Clear();
            InstanceCacheByName.Remove(LayerName);
            var keysToRemove = CollectionPool<List<int>, int>.Get();
            foreach (var cache in InstanceCacheByTransform)
                if (Equals(cache.Value))
                    keysToRemove.Add(cache.Key);

            foreach (var keyToRemove in keysToRemove)
                InstanceCacheByTransform.Remove(keyToRemove);
            CollectionPool<List<int>, int>.Release(keysToRemove);
            ContainerLayerManager.Remove(this);
        }

        /// <summary>
        ///     Get the <see cref="ModalContainer" /> that manages the modal to which <see cref="transform" /> belongs.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="transform" />.</param>
        /// <returns></returns>
        public static ModalContainer Of(Transform transform, bool useCache = true) =>
            Of((RectTransform)transform, useCache);

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
                return container;
            container = rectTransform.GetComponentInParent<ModalContainer>();
            if (container == null) return null;
            InstanceCacheByTransform.Add(id, container);
            return container;
        }

        /// <summary>
        /// Find the <see cref="ModalContainer" /> of <see cref="containerName" />.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static ModalContainer Find(string containerName) => InstanceCacheByName.GetValueOrDefault(containerName);

        /// <summary>
        /// Create a new <see cref="ModalContainer" /> as a layer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="layer"></param>
        /// <param name="layerType"></param>
        /// <returns></returns>
        public static ModalContainer Create(string layerName, int layer, ContainerLayerType layerType)
        {
            var root = new GameObject(layerName, typeof(CanvasGroup));
            var rectTransform = root.AddComponent<RectTransform>();
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
            var container = root.AddComponent<ModalContainer>();
            container.CreateLayer(layerName, layer, layerType);
            InstanceCacheByName.TryAdd(layerName, container);
            return container;
        }

        /// <summary>
        ///     Add a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void AddCallbackReceiver(IModalContainerCallbackReceiver callbackReceiver)
            => _callbackReceivers.Add(callbackReceiver);

        /// <summary>
        ///     Remove a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void RemoveCallbackReceiver(IModalContainerCallbackReceiver callbackReceiver)
            => _callbackReceivers.Remove(callbackReceiver);

        /// <summary>
        /// Push new modal.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public UniTask<Modal> Push(WindowOption option) => PushTask(option);

        /// <summary>
        /// Pop current modal.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        public UniTask Pop(bool playAnimation) => PopTask(playAnimation);

        // ReSharper disable Unity.PerformanceAnalysis
        private async UniTask<Modal> PushTask(WindowOption option)
        {
            if (string.IsNullOrEmpty(option.ResourcePath))
                throw new ArgumentException("Path is null or empty.");

            if (_isInTransition)
                await UniTask.WaitUntil(() => !_isInTransition);

            //Handle the single container
            if (!AllowMultiple && Current != null)
            {
                //if the modal has higher priority than the current modal, pop the current modal
                if (Current.Priority < option.Priority)
                {
                    if (_modals.Count > 0)
                        await PopTask(false);
                }
                else
                    return null;
            }

            _isInTransition = true;
            ModalBackdrop backdrop;
            if (!option.IsPoolable)
                backdrop = Instantiate(_backdropPrefab);
            else
            {
                var instance = await LazyGameObjectPool.Rent(_backdropPrefab.gameObject);
                backdrop = instance.GetComponent<ModalBackdrop>();
            }
            if(!backdrop)
                throw new InvalidOperationException("Cannot transition because the \"ModalBackdrop\" component is not attached to the specified resource.");
            backdrop.IsPoolItem = option.IsPoolable;
            backdrop.Setup((RectTransform)transform);
            _backdrops.Add(backdrop);
            Modal enterModal;
            if (!option.IsPoolable)
            {
                var operationResult = await _assetsKeyLoader.LoadAssetAsync(option.ResourcePath);
                var instance = Instantiate(operationResult);
                enterModal = instance.GetComponent<Modal>();
            }
            else
            {
                var instance = await LazyAssetRefGameObjectPool.Rent(option.ResourcePath);
                enterModal = instance.GetComponent<Modal>();
            }

            if (enterModal == null)
            {
                throw new InvalidOperationException(
                    $"Cannot transition because the \"{nameof(Modal)}\" component is not attached to the specified resource \"{option.ResourcePath}\".");
            }

            _modalItems.Add(option.ResourcePath);
            enterModal.Priority = option.Priority;
            enterModal.IsPoolItem = option.IsPoolable;
            option.WindowCreated.Value = enterModal;

            var afterLoadHandle = enterModal.AfterLoad((RectTransform)transform);
            await afterLoadHandle;

            var exitModal = _modals.Count == 0 ? null : _modals[^1];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
                callbackReceiver.BeforePush(enterModal, exitModal);
            if (exitModal != null)
                await exitModal.BeforeExit(true, enterModal);

            await enterModal.BeforeEnter(true, exitModal);

            // Play Animation
            await backdrop.Enter(option.PlayAnimation);
            if (exitModal != null)
                await exitModal.Exit(true, option.PlayAnimation, enterModal);
            await enterModal.Enter(true, option.PlayAnimation, exitModal);

            // End Transition
            _modals.Add(enterModal);
            _isInTransition = false;

            // Postprocess
            if (exitModal != null)
                exitModal.AfterExit(true, enterModal);

            enterModal.AfterEnter(true, exitModal);

            foreach (var callbackReceiver in _callbackReceivers)
                callbackReceiver.AfterPush(enterModal, exitModal);

            return enterModal;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private async UniTask PopTask(bool playAnimation)
        {
            if (_modals.Count == 0)
            {
                Debug.LogError("Cannot transition because there are no modals loaded on the stack.");
                return;
            }

            _isInTransition = true;

            var exitModal = _modals[^1];
            var enterModal = _modals.Count == 1 ? null : _modals[^2];
            var backdrop = _backdrops[^1];
            _backdrops.RemoveAt(_backdrops.Count - 1);

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
                callbackReceiver.BeforePop(enterModal, exitModal);

            await exitModal.BeforeExit(false, enterModal);

            if (enterModal != null)
                await enterModal.BeforeEnter(false, exitModal);

            // Play Animation
            await exitModal.Exit(false, playAnimation, enterModal);
            if (enterModal != null)
                await enterModal.Enter(false, playAnimation, exitModal);

            await backdrop.Exit(playAnimation);

            // End Transition
            _modals.RemoveAt(_modals.Count - 1);
            _isInTransition = false;

            // Postprocess
            exitModal.AfterExit(false, enterModal);
            if (enterModal != null)
                enterModal.AfterEnter(false, exitModal);

            foreach (var callbackReceiver in _callbackReceivers)
                callbackReceiver.AfterPop(enterModal, exitModal);

            // Unload Unused Screen
            var beforeReleaseHandle = exitModal.BeforeRelease();
            await beforeReleaseHandle;


            _assetsKeyLoader.UnloadAsset(_modalItems[^1]);
            _modalItems.RemoveAt(_modalItems.Count - 1);
            if (exitModal.IsPoolItem)
                LazyAssetRefGameObjectPool.Return(exitModal.gameObject);
            else
                Destroy(exitModal.gameObject);
            if (backdrop.IsPoolItem)
                LazyGameObjectPool.Return(backdrop.gameObject);
            else
                Destroy(backdrop.gameObject);
        }

        public UniTask Preload(string resourceKey) => PreloadTask(resourceKey);

        private UniTask PreloadTask(string resourceKey) => _assetsKeyLoader.LoadAssetAsync(resourceKey);

        public void ReleasePreloaded(string resourceKey)
        {
            _preloadAssetKeys.Remove(resourceKey);
            _assetsKeyLoader.UnloadAsset(resourceKey);
        }

        public override UniTask OnBackButtonPressed() => _modals.Count > 0 ? Pop(true) : UniTask.CompletedTask;

        /// <summary>
        /// In this case the <see cref="ModalContainer" /> is created manually in Hierarchy.
        /// </summary>
        private void PreSetting()
        {
            if (InstanceCacheByName.ContainsKey(LayerName))
                return;
            SortOrder = Canvas.sortingOrder;
            LayerType = ContainerLayerType.Modal;
            InstanceCacheByName.Add(LayerName, this);
            ContainerLayerManager.Add(this);
        }
    }
}