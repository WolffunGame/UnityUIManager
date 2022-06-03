using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation;

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

                DynamicWindow window = _dynamicWindows[_dynamicWindows.Count - 1];
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
        
        private readonly List<IDynamicWindowContainerCallbackReceiver> _callbackReceivers =
            new List<IDynamicWindowContainerCallbackReceiver>();
        
        private List<DynamicWindow> _dynamicWindows = new List<DynamicWindow>();

        private readonly List<CacheWindowItem> _windowItems = new List<CacheWindowItem>();

        private readonly List<string> _preloadAssetKeys = new List<string>();

        public IEnumerator<DynamicWindow> Visibles()
        {
            return new InternalVisibleEnumerator(_dynamicWindows);
        }

        public DynamicWindow Get(int index)
        {
            if (index < 0 || index > _dynamicWindows.Count - 1)
                throw new IndexOutOfRangeException();

            return _dynamicWindows[index];
        }

        public void Add(DynamicWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            if (_dynamicWindows.Contains(window))
                return;

            _dynamicWindows.Add(window);
            transform.AddChild(GetTransform(window));
        }

        public bool Remove(DynamicWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            transform.RemoveChild(GetTransform(window));
            return _dynamicWindows.Remove(window);
        }

        public DynamicWindow RemoveAt(int index)
        {
            if (index < 0 || index > _dynamicWindows.Count - 1)
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

            return _dynamicWindows.Contains(window);
        }

        public int IndexOf(DynamicWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            return _dynamicWindows.IndexOf(window);
        }

        public List<DynamicWindow> Find(bool visible)
        {
            var result = new List<DynamicWindow>();

            foreach (var window in _dynamicWindows)
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

            foreach (var window in _dynamicWindows)
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

        public UniTask Show(WindowOption option)
        {
            return ShowTask(option);
        }

        private async UniTask ShowTask(WindowOption option)
        {
            if (option.ResourcePath == null)
            {
                throw new ArgumentNullException(nameof(option.ResourcePath));
            }

            var assetLoadHandle = await AddressablesManager.LoadAssetAsync<GameObject>(option.ResourcePath);

            var instance = Instantiate(assetLoadHandle.Value);
            var enterWindow = instance.GetComponent<DynamicWindow>();
            if (enterWindow == null)
            {
                throw new InvalidOperationException(
                    $"Cannot transition because the \"{nameof(DynamicWindow)}\" component is not attached to the specified resource \"{option.ResourcePath}\".");
            }

            var dynamicWindowId = enterWindow.GetInstanceID();
            enterWindow.Identifier = string.Concat(gameObject.name, dynamicWindowId.ToString());
            _windowItems.Add(new CacheWindowItem(instance, option.ResourcePath));
            option.WindowCreated?.Invoke(enterWindow);

            MoveToIndex(enterWindow, _dynamicWindows.Count);

            await enterWindow.AfterLoad((RectTransform) transform);


            var exitModal = _dynamicWindows.Count == 0 ? null : _dynamicWindows[_dynamicWindows.Count - 1];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforeShow(enterWindow, exitModal);
            }


            await exitModal.BeforeExit(true, enterWindow);


            await enterWindow.BeforeEnter(true, exitModal);

            // Play Animation
            if (exitModal != null)
            {
                await exitModal.Exit(true, option.PlayAnimation, enterWindow);
            }

            await enterWindow.Enter(true, option.PlayAnimation, exitModal);

            // End Transition
            _dynamicWindows.Add(enterWindow);


            // Postprocess
            if (exitModal != null)
            {
                exitModal.AfterExit(true, enterWindow);
            }

            enterWindow.AfterEnter(true, exitModal);

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterShow(enterWindow, exitModal);
            }
        }

        public UniTask Hide(string identifier, bool playAnimation)
        {
            return HideTask(identifier, playAnimation);
        }

        public void HideAll(bool playAnimation)
        {
            foreach (var dynamicWindow in _dynamicWindows)
            {
                dynamicWindow.Exit(false, playAnimation, dynamicWindow);
            }
        }

        private async UniTask HideTask(string identifier, bool playAnimation)
        {
            if (_dynamicWindows.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot transition because there are no modals loaded on the stack.");
            }

            //var exitModal = _dynamicWindows[_dynamicWindows.Count - 1];
            var exitWindow = _dynamicWindows.Find(x => x.Identifier == identifier);
            var enterWindow = _dynamicWindows.Count == 1 ? null : _dynamicWindows[_dynamicWindows.Count - 2];

            // Preprocess
            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.BeforeHide(enterWindow, exitWindow);
            }


            await exitWindow.BeforeExit(false, enterWindow);

            if (enterWindow != null)
            {
                await enterWindow.BeforeEnter(false, exitWindow);
            }

            // Play Animation
            await exitWindow.Exit(false, playAnimation, enterWindow);

            if (enterWindow != null)
            {
                await enterWindow.Enter(false, playAnimation, exitWindow);
            }

            // End Transition
            _dynamicWindows.Remove(exitWindow);

            // Postprocess
            exitWindow.AfterExit(false, enterWindow);
            if (enterWindow != null)
            {
                enterWindow.AfterEnter(false, exitWindow);
            }

            foreach (var callbackReceiver in _callbackReceivers)
            {
                callbackReceiver.AfterHide(enterWindow, exitWindow);
            }

            // Unload Unused Screen
            var beforeReleaseHandle = exitWindow.BeforeRelease();
            await beforeReleaseHandle;
            
            AddressablesManager.ReleaseAsset(_windowItems[_windowItems.Count - 2].Key);
            Destroy(exitWindow.gameObject);
        }

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