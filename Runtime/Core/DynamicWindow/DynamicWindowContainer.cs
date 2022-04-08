using System.Collections.Generic;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    public class DynamicWindowContainer : ContainerLayer, IWindowManager
    {
        private IWindowManager _localWindowManager;
        
        private readonly List<IDynamicWindowContainerCallbackReceiver> _callbackReceivers =
            new List<IDynamicWindowContainerCallbackReceiver>();
        
        public override int VisibleElementInLayer
        {
            get => _localWindowManager.Count;
        }
        
        public static DynamicWindowContainer Create(string layerName, int layer, ContainerLayerType layerType)
        {
            GameObject root = new GameObject(layerName, typeof(CanvasGroup));
            RectTransform rectTransform = root.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector3.zero;

            DynamicWindowContainer container = root.GetOrAddComponent<DynamicWindowContainer>();
            //container.WindowManager = windowManager;
            container.CreateLayer(layerName, layer, layerType);
            return container;
        }
        protected override void OnCreate()
        {
           _localWindowManager = CreateWindowManager();
        }

        protected virtual IWindowManager CreateWindowManager()
        {
            return gameObject.AddComponent<DynamicWindowManager>();
        }


        public IWindow Current { get=>_localWindowManager.Current; }
        
        public int Count { get=>_localWindowManager.Count; }
        public IEnumerator<IWindow> Visibles()
        {
            return _localWindowManager.Visibles();
        }

        public IWindow Get(int index)
        {
            return _localWindowManager.Get(index);
        }

        public void Add(IWindow window)
        {
            _localWindowManager.Add(window);
        }

        public bool Remove(IWindow window)
        {
            return _localWindowManager.Remove(window);
        }

        public IWindow RemoveAt(int index)
        {
            return _localWindowManager.RemoveAt(index);
        }

        public bool Contains(IWindow window)
        {
            return _localWindowManager.Contains(window);
        }

        public int IndexOf(IWindow window)
        {
            return _localWindowManager.IndexOf(window);
        }

        public List<IWindow> Find(bool visible)
        {
            return _localWindowManager.Find(visible);
        }

        public T Find<T>() where T : IWindow
        {
            return _localWindowManager.Find<T>();
        }

        public T Find<T>(string windowName) where T : IWindow
        {
            return _localWindowManager.Find<T>(windowName);
        }

        public List<T> FindAll<T>() where T : IWindow
        {
            return _localWindowManager.FindAll<T>();
        }

        public void Clear()
        {
            _localWindowManager.Clear();
        }

        public AsyncProcessHandle Show(WindowOption option)
        {
            return _localWindowManager.Show(option);
        }

        public AsyncProcessHandle Hide(bool playAnimation = true)
        {
            return _localWindowManager.Hide(playAnimation);
        }
        
        /// <summary>
        ///     Add a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void AddCallbackReceiver(IDynamicWindowContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Add(callbackReceiver);
        }

        /// <summary>
        ///     Remove a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void RemoveCallbackReceiver(IDynamicWindowContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Remove(callbackReceiver);
        }
    }
}