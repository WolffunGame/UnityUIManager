using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation;
using UnityScreenNavigator.Runtime.Foundation.AssetLoader;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    [DisallowMultipleComponent]
    public class DynamicWindowManager : MonoBehaviour, IDynamicWindowManager
    {
        public virtual DynamicWindow Current
        {
            get
            {
                if (_dynamicWindows == null || _dynamicWindows.Count <= 0)
                    return null;

                DynamicWindow window = _dynamicWindows[0];
                return window != null && window.Visibility ? window : null;
            }
        }

        private bool _activated = true;

        public bool Activated
        {
            get { return _activated; }
            set
            {
                if (_activated == value)
                    return;

                _activated = value;
            }
        }

        public int Count
        {
            get { return _dynamicWindows.Count; }
        }

        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _preloadedResourceHandles =
            new Dictionary<string, AssetLoadHandle<GameObject>>();

        private readonly Dictionary<int, AssetLoadHandle<GameObject>> _assetLoadHandles
            = new Dictionary<int, AssetLoadHandle<GameObject>>();

        private readonly List<IDynamicWindowContainerCallbackReceiver> _callbackReceivers =
            new List<IDynamicWindowContainerCallbackReceiver>();


        private IAssetLoader AssetLoader => UnityScreenNavigatorSettings.Instance.AssetLoader;


        private List<DynamicWindow> _dynamicWindows = new List<DynamicWindow>();

        public IEnumerator<DynamicWindow> Visibles()
        {
            return new InternalVisibleEnumerator(_dynamicWindows);
        }

        public DynamicWindow Get(int index)
        {
            if (index < 0 || index > this._dynamicWindows.Count - 1)
                throw new IndexOutOfRangeException();

            return this._dynamicWindows[index];
        }

        public void Add(DynamicWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            if (this._dynamicWindows.Contains(window as DynamicWindow))
                return;

            this._dynamicWindows.Add(window as DynamicWindow);
            transform.AddChild(GetTransform(window));
        }

        public bool Remove(DynamicWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            transform.RemoveChild(GetTransform(window));
            return this._dynamicWindows.Remove(window as DynamicWindow);
        }

        public DynamicWindow RemoveAt(int index)
        {
            if (index < 0 || index > this._dynamicWindows.Count - 1)
                throw new IndexOutOfRangeException();

            var window = _dynamicWindows[index];

            transform.RemoveChild(GetTransform(window));
            _dynamicWindows.RemoveAt(index);
            return window;
        }

        public bool Contains(DynamicWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            return this._dynamicWindows.Contains(window as DynamicWindow);
        }

        public int IndexOf(DynamicWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            return this._dynamicWindows.IndexOf(window as DynamicWindow);
        }

        public List<DynamicWindow> Find(bool visible)
        {
            var result = new List<DynamicWindow>();

            foreach (var window in this._dynamicWindows)
            {
                if (window.Visibility == visible)
                    result.Add(window);
            }

            return result;
        }

        public T Find<T>() where T : DynamicWindow
        {
            return (T) _dynamicWindows.Find(x => x is T);
        }

        public T Find<T>(string windowName) where T : DynamicWindow
        {
            return (T) _dynamicWindows.Find(x => x is T && x.Name == windowName);
        }

        public List<T> FindAll<T>() where T : DynamicWindow
        {
            var result = new List<T>();

            foreach (var window in this._dynamicWindows)
            {
                if (window is T)
                    result.Add((T) window);
            }

            return result;
        }


        public void Clear()
        {
            //TODO dismiss animation, destroy gameobject
            _dynamicWindows.Clear();
        }

        protected virtual void MoveToIndex(DynamicWindow dynamicWindow, int index)
        {
            if (dynamicWindow == null)
                throw new ArgumentNullException("dynamicWindow");

            int oldIndex = IndexOf(dynamicWindow);
            try
            {
                if (oldIndex < 0 || oldIndex == index)
                    return;

                _dynamicWindows.RemoveAt(oldIndex);
                _dynamicWindows.Insert(index, dynamicWindow);
            }
            finally
            {
                Transform t = GetTransform(dynamicWindow);
                if (t != null)
                {
                    if (index == 0)
                    {
                        t.SetAsLastSibling();
                    }
                    else
                    {
                        IWindow preWindow = _dynamicWindows[index - 1];
                        int preWindowPosition = GetChildIndex(GetTransform(preWindow));
                        int currWindowPosition = oldIndex >= index ? preWindowPosition - 1 : preWindowPosition;
                        t.SetSiblingIndex(currWindowPosition);
                    }
                }
            }
        }

        protected virtual Transform GetTransform(IWindow window)
        {
            try
            {
                if (window == null)
                    return null;

                if (window is UIView)
                    return (window as UIView).RectTransform;

                var propertyInfo = window.GetType().GetProperty("Transform");
                if (propertyInfo != null)
                    return (Transform) propertyInfo.GetGetMethod().Invoke(window, null);

                if (window is Component)
                    return (window as Component).transform;
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected virtual int GetChildIndex(Transform child)
        {
            Transform transform1 = transform;
            int count = transform1.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                if (transform1.GetChild(i).Equals(child))
                    return i;
            }

            return -1;
        }

        public AsyncProcessHandle Show(WindowOption option)
        {
            return CoroutineManager.Instance.Run(ShowRoutine(option));
        }

        private IEnumerator ShowRoutine(WindowOption option)
        {
            if (option.ResourcePath == null)
            {
                throw new ArgumentNullException(nameof(option.ResourcePath));
            }

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
            var enterModal = instance.GetComponent<DynamicWindow>();
            if (enterModal == null)
            {
                throw new InvalidOperationException(
                    $"Cannot transition because the \"{nameof(DynamicWindow)}\" component is not attached to the specified resource \"{option.ResourcePath}\".");
            }
            
            var dynamicWindowId = enterModal.GetInstanceID();
            enterModal.Identifier = string.Concat(gameObject.name, dynamicWindowId.ToString());
            _assetLoadHandles.Add(dynamicWindowId, assetLoadHandle);
            
            option.WindowCreated?.Invoke(enterModal);

            MoveToIndex(enterModal, _dynamicWindows.Count);

            var afterLoadHandle = enterModal.AfterLoad((RectTransform) transform);
            while (!afterLoadHandle.IsTerminated)
            {
                yield return null;
            }

            var exitModal = _dynamicWindows.Count == 0 ? null : _dynamicWindows[_dynamicWindows.Count - 1];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforeShow(enterModal, exitModal);
            }

            var preprocessHandles = new List<AsyncProcessHandle>();
            if (exitModal != null)
            {
                preprocessHandles.Add(exitModal.BeforeExit(true, enterModal));
            }

            preprocessHandles.Add(enterModal.BeforeEnter(true, exitModal));

            foreach (var coroutineHandle in preprocessHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // Play Animation
            var animationHandles = new List<AsyncProcessHandle>();

            if (exitModal != null)
            {
                animationHandles.Add(exitModal.Exit(true, option.PlayAnimation, enterModal));
            }

            animationHandles.Add(enterModal.Enter(true, option.PlayAnimation, exitModal));

            foreach (var coroutineHandle in animationHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // End Transition
            _dynamicWindows.Add(enterModal);


            // Postprocess
            if (exitModal != null)
            {
                exitModal.AfterExit(true, enterModal);
            }

            enterModal.AfterEnter(true, exitModal);

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterShow(enterModal, exitModal);
            }
        }

        public AsyncProcessHandle Hide(string identifier, bool playAnimation)
        {
            return CoroutineManager.Instance.Run(HideRoutine(identifier, playAnimation));
        }

        public void HideAll(bool playAnimation)
        {
            foreach (var dynamicWindow in _dynamicWindows)
            {
                dynamicWindow.Exit(false, playAnimation, dynamicWindow);
            }
        }

        private IEnumerator HideRoutine(string identifier, bool playAnimation)
        {
            if (_dynamicWindows.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot transition because there are no modals loaded on the stack.");
            }

            //var exitModal = _dynamicWindows[_dynamicWindows.Count - 1];
            var exitModal = _dynamicWindows.Find(x => x.Identifier == identifier);
            var exitModalId = exitModal.GetInstanceID();
            var enterModal = _dynamicWindows.Count == 1 ? null : _dynamicWindows[_dynamicWindows.Count - 2];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforeHide(enterModal, exitModal);
            }

            var preprocessHandles = new List<AsyncProcessHandle>
            {
                exitModal.BeforeExit(false, enterModal)
            };
            if (enterModal != null)
            {
                preprocessHandles.Add(enterModal.BeforeEnter(false, exitModal));
            }

            foreach (var coroutineHandle in preprocessHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // Play Animation
            var animationHandles = new List<AsyncProcessHandle>
            {
                exitModal.Exit(false, playAnimation, enterModal)
            };
            if (enterModal != null)
            {
                animationHandles.Add(enterModal.Enter(false, playAnimation, exitModal));
            }

            foreach (var coroutineHandle in animationHandles)
            {
                while (!coroutineHandle.IsTerminated)
                {
                    yield return coroutineHandle;
                }
            }

            // End Transition
            _dynamicWindows.Remove(exitModal);

            // Postprocess
            exitModal.AfterExit(false, enterModal);
            if (enterModal != null)
            {
                enterModal.AfterEnter(false, exitModal);
            }

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterHide(enterModal, exitModal);
            }

            // Unload Unused Screen
            var beforeReleaseHandle = exitModal.BeforeRelease();
            while (!beforeReleaseHandle.IsTerminated)
            {
                yield return null;
            }

            var loadHandle = _assetLoadHandles[exitModalId];
            Destroy(exitModal.gameObject);
            AssetLoader.Release(loadHandle);
            _assetLoadHandles.Remove(exitModalId);
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

        class InternalVisibleEnumerator : IEnumerator<DynamicWindow>
        {
            private List<DynamicWindow> windows;
            private int index = -1;

            public InternalVisibleEnumerator(List<DynamicWindow> list)
            {
                windows = list;
            }

            public DynamicWindow Current
            {
                get { return index < 0 || index >= windows.Count ? null : windows[index]; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                index = -1;
                windows.Clear();
            }

            public bool MoveNext()
            {
                if (index >= windows.Count - 1)
                    return false;

                index++;
                for (; index < windows.Count; index++)
                {
                    DynamicWindow window = windows[index];
                    if (window != null && window.Visibility)
                        return true;
                }

                return false;
            }

            public void Reset()
            {
                index = -1;
            }
        }
    }
}