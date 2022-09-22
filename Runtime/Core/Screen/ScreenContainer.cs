using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Screen
{
    [RequireComponent(typeof(RectMask2D))]
    public sealed class ScreenContainer : ContainerLayer, IContainerManager<Screen>
    {
        private static readonly Dictionary<int, ScreenContainer> InstanceCacheByTransform =
            new Dictionary<int, ScreenContainer>(5);

        private static readonly Dictionary<string, ScreenContainer> InstanceCacheByName =
            new Dictionary<string, ScreenContainer>(5);

        private readonly List<IScreenContainerCallbackReceiver> _callbackReceivers =
            new List<IScreenContainerCallbackReceiver>();

        //controls load and unload of resources
        private readonly List<string> _screenItems = new List<string>(5);

        //Controls the visibility of the screens, the last one is always visible
        private readonly List<Screen> _screenList = new List<Screen>(5);

        private readonly List<string> _preloadAssetKeys = new List<string>(5);

        private bool _isActiveScreenStacked;
        private bool _isInTransition;

        /// <summary>
        ///     Stacked screens.
        /// </summary>
        public IReadOnlyList<string> Screens => _screenItems;


        private void Awake()
        {
            _callbackReceivers.AddRange(GetComponents<IScreenContainerCallbackReceiver>());
            PreSetting();
        }

        private void OnDestroy()
        {
            foreach (var preloadAssetKey in _preloadAssetKeys)
            {
                AddressablesManager.ReleaseAsset(preloadAssetKey);
            }

            _preloadAssetKeys.Clear();
            foreach (var item in _screenItems)
            {
                AddressablesManager.ReleaseAsset(item);
            }

            _screenItems.Clear();

            InstanceCacheByName.Remove(LayerName);
            var keysToRemove = new List<int>();
            foreach (var cache in InstanceCacheByTransform)
                if (Equals(cache.Value))
                    keysToRemove.Add(cache.Key);

            foreach (var keyToRemove in keysToRemove) InstanceCacheByTransform.Remove(keyToRemove);
        }

        public override Window Current => _screenList.Count > 0 ? _screenList[_screenList.Count - 1] : null;

        public override int VisibleElementInLayer => Screens.Count;

        /// <summary>
        ///     Push new screen.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public UniTask<Screen> Push(WindowOption option)
        {
            return PushTask(option);
        }

        /// <summary>
        ///     Pop current screen.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        public UniTask Pop(bool playAnimation)
        {
            return PopTask(playAnimation);
        }

        /// <summary>
        ///     Add a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void AddCallbackReceiver(IScreenContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Add(callbackReceiver);
        }

        /// <summary>
        ///     Remove a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void RemoveCallbackReceiver(IScreenContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Remove(callbackReceiver);
        }

        private async UniTask<Screen> PushTask(WindowOption option)
        {
            if (option.ResourcePath == null) throw new ArgumentNullException(nameof(option.ResourcePath));

            if (_isInTransition)
            {
                await UniTask.WaitUntil(() => !_isInTransition);
            }

            _isInTransition = true;

            // Setup
            var operationResult = await AddressablesManager.LoadAssetAsync<GameObject>(option.ResourcePath);

            var instance = Instantiate(operationResult.Value);
            var enterScreen = instance.GetComponent<Screen>();
            if (enterScreen == null)
                throw new InvalidOperationException(
                    $"Cannot transition because the \"{nameof(Screen)}\" component is not attached to the specified resource \"{option.ResourcePath}\".");
            _screenItems.Add(option.ResourcePath);

            option.WindowCreated.Value = enterScreen;

            var afterLoadTask = enterScreen.AfterLoad((RectTransform) transform);
            await afterLoadTask;

            var exitScreen = _screenList.Count == 0 ? null : _screenList[_screenList.Count - 1];
            var exitScreenId = exitScreen == null ? (int?) null : exitScreen.GetInstanceID();

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePush(enterScreen, exitScreen);
            }

            if (exitScreen != null)
            {
                await exitScreen.BeforeExit(true, enterScreen);
            }

            await enterScreen.BeforeEnter(true, exitScreen);

            // Play Animations
            var animationTasks = new List<UniTask>();
            if (exitScreen != null)
            {
                var exitAnimation = exitScreen.Exit(true, option.PlayAnimation, enterScreen);
                animationTasks.Add(exitAnimation);
            }

            var enterAnimation = enterScreen.Enter(true, option.PlayAnimation, exitScreen);
            animationTasks.Add(enterAnimation);
            await UniTask.WhenAll(animationTasks);


            // End Transition
            if (!_isActiveScreenStacked && exitScreenId.HasValue)
            {
                _screenList.RemoveAt(_screenList.Count - 1);
            }

            _screenList.Add(enterScreen);
            _isInTransition = false;

            // Postprocess
            if (exitScreen != null) exitScreen.AfterExit(true, enterScreen);

            enterScreen.AfterEnter(true, exitScreen);

            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.AfterPush(enterScreen, exitScreen);

            // Unload Unused Screen, if we don't want stack the screen
            if (!_isActiveScreenStacked && exitScreenId.HasValue)
            {
                var beforeReleaseHandle = exitScreen.BeforeRelease();
                await beforeReleaseHandle;

                AddressablesManager.ReleaseAsset(_screenItems[_screenItems.Count - 2]);
                _screenItems.RemoveAt(_screenItems.Count - 2);
                Destroy(exitScreen.gameObject);
            }

            _isActiveScreenStacked = option.Stack;

            return enterScreen;
        }

        private async UniTask PopTask(bool playAnimation)
        {
            if (_screenList.Count == 0)
                throw new InvalidOperationException(
                    "Cannot transition because there are no screens loaded on the stack.");

            _isInTransition = true;

            var exitScreen = _screenList[_screenList.Count - 1];
            var enterScreen = _screenList.Count == 1 ? null : _screenList[_screenList.Count - 2];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.BeforePop(enterScreen, exitScreen);

            await exitScreen.BeforeExit(false, enterScreen);
            if (enterScreen != null) await enterScreen.BeforeEnter(false, exitScreen);

            // Play Animations
            var animationTasks = new List<UniTask>();
            var exitAnimation = exitScreen.Exit(false, playAnimation, enterScreen);
            animationTasks.Add(exitAnimation);
            if (enterScreen != null)
            {
                var enterAnimation = enterScreen.Enter(false, playAnimation, exitScreen);
                animationTasks.Add(enterAnimation);
            }

            await UniTask.WhenAll(animationTasks);

            // End Transition
            _screenList.RemoveAt(_screenList.Count - 1);
            _isInTransition = false;

            // Postprocess
            exitScreen.AfterExit(false, enterScreen);
            if (enterScreen != null) enterScreen.AfterEnter(false, exitScreen);

            foreach (var callbackReceiver in _callbackReceivers) callbackReceiver.AfterPop(enterScreen, exitScreen);

            // Unload Unused Screen
            var beforeReleaseTask = exitScreen.BeforeRelease();
            await beforeReleaseTask;

            AddressablesManager.ReleaseAsset(_screenItems[_screenItems.Count - 1]);
            _screenItems.RemoveAt(_screenItems.Count - 1);
            Destroy(exitScreen.gameObject);

            _isActiveScreenStacked = true;
        }

        #region STATIC_METHODS

        /// <summary>
        ///     Get the <see cref="ScreenContainer" /> that manages the screen to which <see cref="transform" /> belongs.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="transform" />.</param>
        /// <returns></returns>
        public static ScreenContainer Of(Transform transform, bool useCache = true)
        {
            return Of((RectTransform) transform, useCache);
        }

        /// <summary>
        ///     Get the <see cref="ScreenContainer" /> that manages the screen to which <see cref="rectTransform" /> belongs.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="rectTransform" />.</param>
        /// <returns></returns>
        public static ScreenContainer Of(RectTransform rectTransform, bool useCache = true)
        {
            var id = rectTransform.GetInstanceID();
            if (useCache && InstanceCacheByTransform.TryGetValue(id, out var container)) return container;

            container = rectTransform.GetComponentInParent<ScreenContainer>();
            if (container != null)
            {
                InstanceCacheByTransform.Add(id, container);
                return container;
            }

            return null;
        }

        /// <summary>
        ///     Find the <see cref="ScreenContainer" /> of <see cref="containerName" />.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static ScreenContainer Find(string containerName)
        {
            if (InstanceCacheByName.TryGetValue(containerName, out var instance)) return instance;

            return null;
        }

        /// <summary>
        /// Create a new <see cref="ScreenContainer" /> as a layer.
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="layer"></param>
        /// <param name="layerType"></param>
        /// <returns></returns>
        public static ScreenContainer Create(string layerName, int layer, ContainerLayerType layerType)
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
            canvasScaler.referenceResolution = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            root.AddComponent<GraphicRaycaster>();

            var container = root.AddComponent<ScreenContainer>();

            container.CreateLayer(layerName, layer, layerType);

            if (!InstanceCacheByName.ContainsKey(layerName))
            {
                InstanceCacheByName.Add(layerName, container);
            }

            return container;
        }

        /// <summary>
        /// In this case the <see cref="ScreenContainer" /> is created manually in Hierarchy.
        /// </summary>
        private void PreSetting()
        {
            if (!InstanceCacheByName.ContainsKey(LayerName))
            {
                SortOrder = Canvas.sortingOrder;
                LayerType = ContainerLayerType.Screen;
                InstanceCacheByName.Add(LayerName, this);
                ContainerLayerManager.Add(this);
            }
        }

        #endregion

        #region PRELOAD

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

        #endregion

        public override async UniTask OnBackButtonPressed()
        {
            if (_isInTransition) return;
            if (_screenList.Count > 1)
            {
                await Pop(true);
            }
        }
    }
}