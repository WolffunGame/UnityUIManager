using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.DynamicWindow;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation.AssetLoader;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.Screen
{
    [RequireComponent(typeof(RectMask2D))]
    public sealed class ScreenContainer : ContainerLayer, IContainerManager
    {
        private static readonly Dictionary<int, ScreenContainer> InstanceCacheByTransform =
            new Dictionary<int, ScreenContainer>();

        private static readonly Dictionary<string, ScreenContainer> InstanceCacheByName =
            new Dictionary<string, ScreenContainer>();
        
        private readonly Dictionary<int, AssetLoadHandle<GameObject>> _assetLoadHandles
            = new Dictionary<int, AssetLoadHandle<GameObject>>();

        private readonly List<IScreenContainerCallbackReceiver> _callbackReceivers =
            new List<IScreenContainerCallbackReceiver>();

        private readonly List<Screen> _screens = new List<Screen>();

        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _preloadedResourceHandles =
            new Dictionary<string, AssetLoadHandle<GameObject>>();

        private bool _isActiveScreenStacked;

        private IAssetLoader AssetLoader => UnityScreenNavigatorSettings.Instance.AssetLoader;

        /// <summary>
        /// True if in transition.
        /// </summary>
        public bool IsInTransition { get; private set; }

        /// <summary>
        /// Stacked screens.
        /// </summary>
        public IReadOnlyList<Screen> Screens => _screens;

        public Window Current => _screens[_screens.Count - 1];

        public override int VisibleElementInLayer
        {
            get => Screens.Count;
        }

        protected override void Awake()
        {
            _callbackReceivers.AddRange(GetComponents<IScreenContainerCallbackReceiver>());

        }

        protected override void OnDestroy()
        {
            foreach (var screen in _screens)
            {
                var screenId = screen.GetInstanceID();
                var assetLoadHandle = _assetLoadHandles[screenId];

                Destroy(screen.gameObject);
                AssetLoader.Release(assetLoadHandle);
            }

            _assetLoadHandles.Clear();

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

        #region STATIC_METHODS

        /// <summary>
        /// Get the <see cref="ScreenContainer" /> that manages the screen to which <see cref="transform" /> belongs.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="transform" />.</param>
        /// <returns></returns>
        public static ScreenContainer Of(Transform transform, bool useCache = true)
        {
            return Of((RectTransform) transform, useCache);
        }

        /// <summary>
        /// Get the <see cref="ScreenContainer" /> that manages the screen to which <see cref="rectTransform" /> belongs.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="rectTransform" />.</param>
        /// <returns></returns>
        public static ScreenContainer Of(RectTransform rectTransform, bool useCache = true)
        {
            var id = rectTransform.GetInstanceID();
            if (useCache && InstanceCacheByTransform.TryGetValue(id, out var container))
            {
                return container;
            }

            container = rectTransform.GetComponentInParent<ScreenContainer>();
            if (container != null)
            {
                InstanceCacheByTransform.Add(id, container);
                return container;
            }

            return null;
        }

        /// <summary>
        /// Find the <see cref="ScreenContainer" /> of <see cref="containerName" />.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static ScreenContainer Find(string containerName)
        {
            if (InstanceCacheByName.TryGetValue(containerName, out var instance))
            {
                return instance;
            }

            return null;
        }
        /// <summary>
        /// Create a new <see cref="ScreenContainer"/> as a layer.
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="layer"></param>
        /// <param name="layerType"></param>
        /// <returns></returns>
        public static ScreenContainer Create(string layerName, int layer, ContainerLayerType layerType)
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

            ScreenContainer container = root.AddComponent<ScreenContainer>();

            container.CreateLayer(layerName, layer, layerType);
            
            if (!string.IsNullOrWhiteSpace(layerName))
            {
                InstanceCacheByName.Add(layerName, container);
            }
            return container;
        }

        #endregion

        protected override void OnCreate()
        {
            
        }

        /// <summary>
        /// Add a callback receiver.
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

        /// <summary>
        /// Push new screen.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public AsyncProcessHandle Push(WindowOption option)
        {
            return CoroutineManager.Instance.Run(PushRoutine(option));
        }

        /// <summary>
        /// Pop current screen.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        public AsyncProcessHandle Pop(bool playAnimation)
        {
            return CoroutineManager.Instance.Run(PopRoutine(playAnimation));
        }

        private IEnumerator PushRoutine(WindowOption option)
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

            // Setup
            var assetLoadHandle = option.LoadAsync
                ? AssetLoader.LoadAsync<GameObject>(option.ResourcePath)
                : AssetLoader.Load<GameObject>(option.ResourcePath);
            if (!assetLoadHandle.IsDone)
            {
                yield return new WaitUntil(() => assetLoadHandle.IsDone);
            }

            if (assetLoadHandle.Status == AssetLoadStatus.Failed)
            {
                throw assetLoadHandle.OperationException;
            }

            var instance = Instantiate(assetLoadHandle.Result);
            var enterScreen = instance.GetComponent<Screen>();
            if (enterScreen == null)
            {
                throw new InvalidOperationException(
                    $"Cannot transition because the \"{nameof(Screen)}\" component is not attached to the specified resource \"{option.ResourcePath}\".");
            }

            var screenId = enterScreen.GetInstanceID();
            _assetLoadHandles.Add(screenId, assetLoadHandle);
            option.WindowCreated?.Invoke(enterScreen);
            var afterLoadHandle = enterScreen.AfterLoad((RectTransform) transform);
            while (!afterLoadHandle.IsTerminated)
            {
                yield return null;
            }

            var exitScreen = _screens.Count == 0 ? null : _screens[_screens.Count - 1];
            var exitScreenId = exitScreen == null ? (int?) null : exitScreen.GetInstanceID();

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePush(enterScreen, exitScreen);
            }

            var preprocessHandles = new List<AsyncProcessHandle>();
            if (exitScreen != null)
            {
                preprocessHandles.Add(exitScreen.BeforeExit(true, enterScreen));
            }

            preprocessHandles.Add(enterScreen.BeforeEnter(true, exitScreen));

            foreach (var coroutineHandle in preprocessHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // Play Animations
            var animationHandles = new List<AsyncProcessHandle>();
            if (exitScreen != null)
            {
                animationHandles.Add(exitScreen.Exit(true, option.PlayAnimation, enterScreen));
            }

            animationHandles.Add(enterScreen.Enter(true, option.PlayAnimation, exitScreen));

            foreach (var coroutineHandle in animationHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // End Transition
            if (!_isActiveScreenStacked && exitScreenId.HasValue)
            {
                _screens.RemoveAt(_screens.Count - 1);
            }

            _screens.Add(enterScreen);
            IsInTransition = false;

            // Postprocess
            if (exitScreen != null)
            {
                exitScreen.AfterExit(true, enterScreen);
            }

            enterScreen.AfterEnter(true, exitScreen);

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterPush(enterScreen, exitScreen);
            }

            // Unload Unused Screen
            if (!_isActiveScreenStacked && exitScreenId.HasValue)
            {
                var beforeReleaseHandle = exitScreen.BeforeRelease();
                while (!beforeReleaseHandle.IsTerminated)
                {
                    yield return null;
                }

                var handle = _assetLoadHandles[exitScreenId.Value];
                AssetLoader.Release(handle);

                Destroy(exitScreen.gameObject);
                _assetLoadHandles.Remove(exitScreenId.Value);
            }

            _isActiveScreenStacked = option.Stack;
        }

        private IEnumerator PopRoutine(bool playAnimation)
        {
            if (_screens.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot transition because there are no screens loaded on the stack.");
            }

            if (IsInTransition)
            {
                throw new InvalidOperationException(
                    "Cannot transition because the screen is already in transition.");
            }

            IsInTransition = true;

            var exitScreen = _screens[_screens.Count - 1];
            var exitScreenId = exitScreen.GetInstanceID();
            var enterScreen = _screens.Count == 1 ? null : _screens[_screens.Count - 2];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePop(enterScreen, exitScreen);
            }

            var preprocessHandles = new List<AsyncProcessHandle>
            {
                exitScreen.BeforeExit(false, enterScreen)
            };
            if (enterScreen != null)
            {
                preprocessHandles.Add(enterScreen.BeforeEnter(false, exitScreen));
            }

            foreach (var coroutineHandle in preprocessHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // Play Animations
            var animationHandles = new List<AsyncProcessHandle>
            {
                exitScreen.Exit(false, playAnimation, enterScreen)
            };
            if (enterScreen != null)
            {
                animationHandles.Add(enterScreen.Enter(false, playAnimation, exitScreen));
            }

            foreach (var coroutineHandle in animationHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // End Transition
            _screens.RemoveAt(_screens.Count - 1);
            IsInTransition = false;

            // Postprocess
            exitScreen.AfterExit(false, enterScreen);
            if (enterScreen != null)
            {
                enterScreen.AfterEnter(false, exitScreen);
            }

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterPop(enterScreen, exitScreen);
            }

            // Unload Unused Screen
            var beforeReleaseHandle = exitScreen.BeforeRelease();
            while (!beforeReleaseHandle.IsTerminated)
            {
                yield return null;
            }

            var loadHandle = _assetLoadHandles[exitScreenId];
            Destroy(exitScreen.gameObject);
            AssetLoader.Release(loadHandle);
            _assetLoadHandles.Remove(exitScreenId);

            _isActiveScreenStacked = true;
        }

        public AsyncProcessHandle Preload(string resourceKey, bool loadAsync = true)
        {
            return CoroutineManager.Instance.Run(PreloadRoutine(resourceKey, loadAsync));
        }

        private IEnumerator PreloadRoutine(string resourceKey, bool loadAsync = true)
        {
            if (_preloadedResourceHandles.ContainsKey(resourceKey))
            {
                throw new InvalidOperationException(
                    $"The resource with key \"${resourceKey}\" has already been preloaded.");
            }

            var assetLoadHandle = loadAsync
                ? AssetLoader.LoadAsync<GameObject>(resourceKey)
                : AssetLoader.Load<GameObject>(resourceKey);
            _preloadedResourceHandles.Add(resourceKey, assetLoadHandle);

            if (!assetLoadHandle.IsDone)
            {
                yield return new WaitUntil(() => assetLoadHandle.IsDone);
            }

            if (assetLoadHandle.Status == AssetLoadStatus.Failed)
            {
                throw assetLoadHandle.OperationException;
            }
        }

        public bool IsPreloadRequested(string resourceKey)
        {
            return _preloadedResourceHandles.ContainsKey(resourceKey);
        }

        public bool IsPreloaded(string resourceKey)
        {
            if (!_preloadedResourceHandles.TryGetValue(resourceKey, out var handle))
            {
                return false;
            }

            return handle.Status == AssetLoadStatus.Success;
        }

        public void ReleasePreloaded(string resourceKey)
        {
            if (!_preloadedResourceHandles.ContainsKey(resourceKey))
            {
                throw new InvalidOperationException($"The resource with key \"${resourceKey}\" is not preloaded.");
            }

            var handle = _preloadedResourceHandles[resourceKey];
            _preloadedResourceHandles.Remove(resourceKey);
            AssetLoader.Release(handle);
        }
    }
}