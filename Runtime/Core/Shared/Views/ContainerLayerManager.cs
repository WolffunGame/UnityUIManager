using System;
using System.Collections.Generic;
using UnityEngine;
using UnityScreenNavigator.Runtime.Foundation;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(Canvas))]
    public class ContainerLayerManager : UIView, IContainerLayerManager
    {
        private readonly List<IContainerLayer> _containerLayers = new List<IContainerLayer>();

        public IContainerLayer Current
        {
            get
            {
                if (_containerLayers == null || _containerLayers.Count <= 0)
                    return null;

                IContainerLayer layer = _containerLayers[_containerLayers.Count - 1];
                return layer != null && layer.VisibleElementInLayer > 0 ? layer : null;
            }
        }

        public bool Activated { get; set; }

        public int Count
        {
            get => _containerLayers.Count;
        }

        public IEnumerator<IContainerLayer> Visibles()
        {
            foreach (var layer in _containerLayers)
            {
                if (layer.VisibleElementInLayer > 0)
                    yield return layer;
            }
        }

        public IContainerLayer Get(int index)
        {
            if (index < 0 || index > _containerLayers.Count - 1)
                throw new IndexOutOfRangeException();

            return _containerLayers[index];
        }

        public void Add(IContainerLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            if (_containerLayers.Contains(layer))
                return;

            _containerLayers.Add(layer);
            transform.AddChild(GetTransform(layer));
        }

        public bool Remove(IContainerLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            return _containerLayers.Remove(layer);
        }

        public IContainerLayer RemoveAt(int index)
        {
            if (index < 0 || index > _containerLayers.Count - 1)
                throw new IndexOutOfRangeException();

            var layer = _containerLayers[index];

            transform.RemoveChild(GetTransform(layer));
            _containerLayers.RemoveAt(index);
            return layer;
        }


        public bool Contains(IContainerLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            return _containerLayers.Contains(layer);
        }

        public int IndexOf(IContainerLayer window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            return _containerLayers.IndexOf(window);
        }

        public List<IContainerLayer> Find(bool visible)
        {
            List<IContainerLayer> result = new List<IContainerLayer>();
            foreach (var layer in _containerLayers)
            {
                if (layer.VisibleElementInLayer > 0 == visible)
                    result.Add(layer);
            }

            return result;
        }

        public T Find<T>() where T : IContainerLayer
        {
            return (T) _containerLayers.Find(x => x is T);
        }

        public T Find<T>(string layerName) where T : IContainerLayer
        {
            return (T) _containerLayers.Find(x => x is T && x.LayerName == layerName);
        }

        public List<T> FindAll<T>() where T : IContainerLayer
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _containerLayers.Clear();
        }

        protected virtual Transform GetTransform(IContainerLayer layer)
        {
            try
            {
                if (layer == null)
                    return null;

                if (layer is UIView)
                    return (layer as UIView).RectTransform;

                var propertyInfo = layer.GetType().GetProperty("Transform");
                if (propertyInfo != null)
                    return (Transform) propertyInfo.GetGetMethod().Invoke(layer, null);

                if (layer is Component)
                    return (layer as Component).transform;
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
    }
}