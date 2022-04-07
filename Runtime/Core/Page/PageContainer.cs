using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation.AssetLoader;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.Page
{
    [RequireComponent(typeof(RectMask2D))]
    public sealed class PageContainer : ContainerBase, IContainerManager
    {
        private static readonly Dictionary<int, PageContainer> InstanceCacheByTransform =
            new Dictionary<int, PageContainer>();

        private static readonly Dictionary<string, PageContainer> InstanceCacheByName =
            new Dictionary<string, PageContainer>();

        [SerializeField] private string _name;

        public override string Identifier => _name;

        private readonly Dictionary<int, AssetLoadHandle<GameObject>> _assetLoadHandles
            = new Dictionary<int, AssetLoadHandle<GameObject>>();

        private readonly List<IPageContainerCallbackReceiver> _callbackReceivers =
            new List<IPageContainerCallbackReceiver>();

        private readonly List<Page> _pages = new List<Page>();

        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _preloadedResourceHandles =
            new Dictionary<string, AssetLoadHandle<GameObject>>();

        private bool _isActivePageStacked;

        private IAssetLoader AssetLoader => UnityScreenNavigatorSettings.Instance.AssetLoader;

        /// <summary>
        /// True if in transition.
        /// </summary>
        public bool IsInTransition { get; private set; }

        /// <summary>
        /// Stacked pages.
        /// </summary>
        public IReadOnlyList<Page> Pages => _pages;

        public ContainerBase Current => _pages[_pages.Count - 1];
        public int Layer { get; set; }
        public string LayerName { get; set; }
        public ContainerLayerType LayerType { get; set; }
        private IContainerLayerManager _containerLayerManager;

        public IContainerLayerManager ContainerLayerManager
        {
            get
            {
                return this._containerLayerManager ?? (this._containerLayerManager =
                    GameObject.FindObjectOfType<GlobalContainerLayerManager>());
            }
            set { this._containerLayerManager = value; }
        }

        public int VisibleElementInLayer
        {
            get => Pages.Count;
        }

        protected override void Awake()
        {
            _callbackReceivers.AddRange(GetComponents<IPageContainerCallbackReceiver>());
            if (!string.IsNullOrWhiteSpace(_name))
            {
                InstanceCacheByName.Add(_name, this);
            }
        }

        protected override void OnDestroy()
        {
            foreach (var page in _pages)
            {
                var pageId = page.GetInstanceID();
                var assetLoadHandle = _assetLoadHandles[pageId];

                Destroy(page.gameObject);
                AssetLoader.Release(assetLoadHandle);
            }

            _assetLoadHandles.Clear();

            InstanceCacheByName.Remove(_name);
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
        ///     Get the <see cref="PageContainer" /> that manages the page to which <see cref="transform" /> belongs.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="transform" />.</param>
        /// <returns></returns>
        public static PageContainer Of(Transform transform, bool useCache = true)
        {
            return Of((RectTransform) transform, useCache);
        }

        /// <summary>
        ///     Get the <see cref="PageContainer" /> that manages the page to which <see cref="rectTransform" /> belongs.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="rectTransform" />.</param>
        /// <returns></returns>
        public static PageContainer Of(RectTransform rectTransform, bool useCache = true)
        {
            var id = rectTransform.GetInstanceID();
            if (useCache && InstanceCacheByTransform.TryGetValue(id, out var container))
            {
                return container;
            }

            container = rectTransform.GetComponentInParent<PageContainer>();
            if (container != null)
            {
                InstanceCacheByTransform.Add(id, container);
                return container;
            }

            return null;
        }

        /// <summary>
        ///     Find the <see cref="PageContainer" /> of <see cref="containerName" />.
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static PageContainer Find(string containerName)
        {
            if (InstanceCacheByName.TryGetValue(containerName, out var instance))
            {
                return instance;
            }

            return null;
        }

        public static PageContainer Create(string name, string path)
        {
            GameObject root = new GameObject(name, typeof(CanvasGroup));
            RectTransform rectTransform = root.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector3.zero;

            PageContainer container = root.AddComponent<PageContainer>();
            //container.WindowManager = windowManager;
            //container.Create();
            //TODO get layer from settings


            PushWindowOption option = new PushWindowOption(path, false);
            container.Push(option);
            return container;
        }

        #endregion

        /// <summary>
        ///     Add a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void AddCallbackReceiver(IPageContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Add(callbackReceiver);
        }

        /// <summary>
        ///     Remove a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void RemoveCallbackReceiver(IPageContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Remove(callbackReceiver);
        }

        /// <summary>
        ///     Push new page.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public AsyncProcessHandle Push(PushWindowOption option)
        {
            return CoroutineManager.Instance.Run(PushRoutine(option));
        }

        /// <summary>
        ///     Pop current page.
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        public AsyncProcessHandle Pop(bool playAnimation)
        {
            return CoroutineManager.Instance.Run(PopRoutine(playAnimation));
        }

        private IEnumerator PushRoutine(PushWindowOption option)
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
            var enterPage = instance.GetComponent<Page>();
            if (enterPage == null)
            {
                throw new InvalidOperationException(
                    $"Cannot transition because the \"{nameof(Page)}\" component is not attached to the specified resource \"{option.ResourcePath}\".");
            }

            var pageId = enterPage.GetInstanceID();
            _assetLoadHandles.Add(pageId, assetLoadHandle);
            option.OnWindowCreated?.Invoke(enterPage);
            var afterLoadHandle = enterPage.AfterLoad((RectTransform) transform);
            while (!afterLoadHandle.IsTerminated)
            {
                yield return null;
            }

            var exitPage = _pages.Count == 0 ? null : _pages[_pages.Count - 1];
            var exitPageId = exitPage == null ? (int?) null : exitPage.GetInstanceID();

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePush(enterPage, exitPage);
            }

            var preprocessHandles = new List<AsyncProcessHandle>();
            if (exitPage != null)
            {
                preprocessHandles.Add(exitPage.BeforeExit(true, enterPage));
            }

            preprocessHandles.Add(enterPage.BeforeEnter(true, exitPage));

            foreach (var coroutineHandle in preprocessHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // Play Animations
            var animationHandles = new List<AsyncProcessHandle>();
            if (exitPage != null)
            {
                animationHandles.Add(exitPage.Exit(true, option.PlayAnimation, enterPage));
            }

            animationHandles.Add(enterPage.Enter(true, option.PlayAnimation, exitPage));

            foreach (var coroutineHandle in animationHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // End Transition
            if (!_isActivePageStacked && exitPageId.HasValue)
            {
                _pages.RemoveAt(_pages.Count - 1);
            }

            _pages.Add(enterPage);
            IsInTransition = false;

            // Postprocess
            if (exitPage != null)
            {
                exitPage.AfterExit(true, enterPage);
            }

            enterPage.AfterEnter(true, exitPage);

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterPush(enterPage, exitPage);
            }

            // Unload Unused Page
            if (!_isActivePageStacked && exitPageId.HasValue)
            {
                var beforeReleaseHandle = exitPage.BeforeRelease();
                while (!beforeReleaseHandle.IsTerminated)
                {
                    yield return null;
                }

                var handle = _assetLoadHandles[exitPageId.Value];
                AssetLoader.Release(handle);

                Destroy(exitPage.gameObject);
                _assetLoadHandles.Remove(exitPageId.Value);
            }

            _isActivePageStacked = option.Stack;
        }

        private IEnumerator PopRoutine(bool playAnimation)
        {
            if (_pages.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot transition because there are no pages loaded on the stack.");
            }

            if (IsInTransition)
            {
                throw new InvalidOperationException(
                    "Cannot transition because the screen is already in transition.");
            }

            IsInTransition = true;

            var exitPage = _pages[_pages.Count - 1];
            var exitPageId = exitPage.GetInstanceID();
            var enterPage = _pages.Count == 1 ? null : _pages[_pages.Count - 2];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforePop(enterPage, exitPage);
            }

            var preprocessHandles = new List<AsyncProcessHandle>
            {
                exitPage.BeforeExit(false, enterPage)
            };
            if (enterPage != null)
            {
                preprocessHandles.Add(enterPage.BeforeEnter(false, exitPage));
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
                exitPage.Exit(false, playAnimation, enterPage)
            };
            if (enterPage != null)
            {
                animationHandles.Add(enterPage.Enter(false, playAnimation, exitPage));
            }

            foreach (var coroutineHandle in animationHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // End Transition
            _pages.RemoveAt(_pages.Count - 1);
            IsInTransition = false;

            // Postprocess
            exitPage.AfterExit(false, enterPage);
            if (enterPage != null)
            {
                enterPage.AfterEnter(false, exitPage);
            }

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterPop(enterPage, exitPage);
            }

            // Unload Unused Page
            var beforeReleaseHandle = exitPage.BeforeRelease();
            while (!beforeReleaseHandle.IsTerminated)
            {
                yield return null;
            }

            var loadHandle = _assetLoadHandles[exitPageId];
            Destroy(exitPage.gameObject);
            AssetLoader.Release(loadHandle);
            _assetLoadHandles.Remove(exitPageId);

            _isActivePageStacked = true;
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