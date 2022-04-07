using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation.AssetLoader;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.UnorderedModal
{
    [DisallowMultipleComponent]
    public class UnorderedModalManager : MonoBehaviour, IWindowManager
    {
        public virtual IWindow Current
        {
            get
            {
                if (this._modals == null || this._modals.Count <= 0)
                    return null;

                IWindow window = this._modals[0];
                return window != null && window.Visibility ? window : null;
            }
        }

        private bool _activated = true;

        public bool Activated
        {
            get { return this._activated; }
            set
            {
                if (this._activated == value)
                    return;

                this._activated = value;
            }
        }

        public int Count
        {
            get { return this._modals.Count; }
        }

        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _preloadedResourceHandles =
            new Dictionary<string, AssetLoadHandle<GameObject>>();

        private readonly Dictionary<int, AssetLoadHandle<GameObject>> _assetLoadHandles
            = new Dictionary<int, AssetLoadHandle<GameObject>>();

        private readonly List<IUnorderedModalContainerCallbackReceiver> _callbackReceivers =
            new List<IUnorderedModalContainerCallbackReceiver>();


        private IAssetLoader AssetLoader => UnityScreenNavigatorSettings.Instance.AssetLoader;


        private List<UnorderedModal> _modals = new List<UnorderedModal>();

        public IEnumerator<IWindow> Visibles()
        {
            throw new System.NotImplementedException();
        }

        public IWindow Get(int index)
        {
            throw new System.NotImplementedException();
        }

        public void Add(IWindow window)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(IWindow window)
        {
            throw new System.NotImplementedException();
        }

        public IWindow RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(IWindow window)
        {
            throw new System.NotImplementedException();
        }

        public int IndexOf(IWindow window)
        {
            throw new System.NotImplementedException();
        }

        public List<IWindow> Find(bool visible)
        {
            throw new System.NotImplementedException();
        }

        public T Find<T>() where T : IWindow
        {
            throw new System.NotImplementedException();
        }

        public T Find<T>(string windowName) where T : IWindow
        {
            throw new System.NotImplementedException();
        }

        public List<T> FindAll<T>() where T : IWindow
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        protected virtual void MoveToIndex(UnorderedModal window, int index)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            int oldIndex = this.IndexOf(window);
            try
            {
                if (oldIndex < 0 || oldIndex == index)
                    return;

                this._modals.RemoveAt(oldIndex);
                this._modals.Insert(index, window);
            }
            finally
            {
                Transform t = GetTransform(window);
                if (t != null)
                {
                    if (index == 0)
                    {
                        t.SetAsLastSibling();
                    }
                    else
                    {
                        IWindow preWindow = this._modals[index - 1];
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
            Transform transform1 = this.transform;
            int count = transform1.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                if (transform1.GetChild(i).Equals(child))
                    return i;
            }

            return -1;
        }

        public AsyncProcessHandle Show(ShowWindowOption option)
        {
            return CoroutineManager.Instance.Run(ShowRoutine(option));
        }

        private IEnumerator ShowRoutine(ShowWindowOption option)
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
            var enterModal = instance.GetComponent<UnorderedModal>();
            if (enterModal == null)
            {
                throw new InvalidOperationException(
                    $"Cannot transition because the \"{nameof(UnorderedModal)}\" component is not attached to the specified resource \"{option.ResourcePath}\".");
            }

            var modalId = enterModal.GetInstanceID();
            _assetLoadHandles.Add(modalId, assetLoadHandle);
            option.OnWindowCreated?.Invoke(enterModal);

            MoveToIndex(enterModal, _modals.Count);

            var afterLoadHandle = enterModal.AfterLoad((RectTransform) transform);
            while (!afterLoadHandle.IsTerminated)
            {
                yield return null;
            }

            var exitModal = _modals.Count == 0 ? null : _modals[_modals.Count - 1];

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
            _modals.Add(enterModal);


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

        public AsyncProcessHandle Hide(bool playAnimation)
        {
            return CoroutineManager.Instance.Run(HideRoutine(playAnimation));
        }

        private IEnumerator HideRoutine(bool playAnimation)
        {
            if (_modals.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot transition because there are no modals loaded on the stack.");
            }

            var exitModal = _modals[_modals.Count - 1];
            var exitModalId = exitModal.GetInstanceID();
            var enterModal = _modals.Count == 1 ? null : _modals[_modals.Count - 2];

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
            _modals.RemoveAt(_modals.Count - 1);

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

            // Unload Unused Page
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

        class InternalVisibleEnumerator : IEnumerator<IWindow>
        {
            private List<IWindow> windows;
            private int index = -1;

            public InternalVisibleEnumerator(List<IWindow> list)
            {
                this.windows = list;
            }

            public IWindow Current
            {
                get { return this.index < 0 || this.index >= this.windows.Count ? null : this.windows[index]; }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public void Dispose()
            {
                this.index = -1;
                this.windows.Clear();
            }

            public bool MoveNext()
            {
                if (index >= this.windows.Count - 1)
                    return false;

                index++;
                for (; index < this.windows.Count; index++)
                {
                    IWindow window = this.windows[index];
                    if (window != null && window.Visibility)
                        return true;
                }

                return false;
            }

            public void Reset()
            {
                this.index = -1;
            }
        }



    }
}