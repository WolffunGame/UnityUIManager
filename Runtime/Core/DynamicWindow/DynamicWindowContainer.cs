using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    [RequireComponent(typeof(RectMask2D))]
    public class DynamicWindowContainer : ContainerLayer, IDynamicWindowManager
    {
        private IDynamicWindowManager _localDynamicWindowManager;

        private readonly List<IDynamicWindowContainerCallbackReceiver> _callbackReceivers =
            new List<IDynamicWindowContainerCallbackReceiver>();

        public override int VisibleElementInLayer
        {
            get => _localDynamicWindowManager.Count;
        }

        /// <summary>
        /// Create a new instance of <see cref="DynamicWindowContainer"/> as a layer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="layer"></param>
        /// <param name="layerType"></param>
        /// <returns></returns>
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

            container.CreateLayer(layerName, layer, layerType);
            return container;
        }

        protected override void OnCreate()
        {
            _localDynamicWindowManager = CreateWindowManager();
        }

        protected virtual IDynamicWindowManager CreateWindowManager()
        {
            return gameObject.AddComponent<DynamicWindowManager>();
        }


        public DynamicWindow Current
        {
            get => _localDynamicWindowManager.Current;
        }

        public int Count
        {
            get => _localDynamicWindowManager.Count;
        }

        public IEnumerator<DynamicWindow> Visibles()
        {
            return _localDynamicWindowManager.Visibles();
        }

        public DynamicWindow Get(int index)
        {
            return _localDynamicWindowManager.Get(index);
        }

        public void Add(DynamicWindow window)
        {
            _localDynamicWindowManager.Add(window);
        }

        public bool Remove(DynamicWindow window)
        {
            return _localDynamicWindowManager.Remove(window);
        }

        public DynamicWindow RemoveAt(int index)
        {
            return _localDynamicWindowManager.RemoveAt(index);
        }

        public bool Contains(DynamicWindow window)
        {
            return _localDynamicWindowManager.Contains(window);
        }

        public int IndexOf(DynamicWindow window)
        {
            return _localDynamicWindowManager.IndexOf(window);
        }

        public List<DynamicWindow> Find(bool visible)
        {
            return _localDynamicWindowManager.Find(visible);
        }

        public T Find<T>() where T : DynamicWindow
        {
            return _localDynamicWindowManager.Find<T>();
        }

        public T Find<T>(string windowName) where T : DynamicWindow
        {
            return _localDynamicWindowManager.Find<T>(windowName);
        }

        public List<T> FindAll<T>() where T : DynamicWindow
        {
            return _localDynamicWindowManager.FindAll<T>();
        }

        public void Clear()
        {
            _localDynamicWindowManager.Clear();
        }

        public AsyncProcessHandle Show(WindowOption option)
        {
            return _localDynamicWindowManager.Show(option);
        }

        public AsyncProcessHandle Hide(string identifier, bool playAnimation = true)
        {
            return _localDynamicWindowManager.Hide(identifier, playAnimation);
        }

        public void HideAll(bool playAnimation)
        {
            _localDynamicWindowManager.HideAll(playAnimation);
        }

        /// <summary>
        /// Add a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void AddCallbackReceiver(IDynamicWindowContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Add(callbackReceiver);
        }

        /// <summary>
        /// Remove a callback receiver.
        /// </summary>
        /// <param name="callbackReceiver"></param>
        public void RemoveCallbackReceiver(IDynamicWindowContainerCallbackReceiver callbackReceiver)
        {
            _callbackReceivers.Remove(callbackReceiver);
        }
    }
}