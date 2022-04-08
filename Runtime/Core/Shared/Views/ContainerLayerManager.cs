using System;
using System.Collections.Generic;
using UnityEngine;

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
                if (this._containerLayers == null || this._containerLayers.Count <= 0)
                    return null;

                IContainerLayer layer = this._containerLayers[this._containerLayers.Count - 1];
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
            if (index < 0 || index > this._containerLayers.Count - 1)
                throw new IndexOutOfRangeException();

            return this._containerLayers[index];
        }

        public void Add(IContainerLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            if (this._containerLayers.Contains(layer))
                return;

            this._containerLayers.Add(layer);
            AddChild(GetTransform(layer));
        }

        public bool Remove(IContainerLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            return this._containerLayers.Remove(layer);
        }

        public IContainerLayer RemoveAt(int index)
        {
            if (index < 0 || index > this._containerLayers.Count - 1)
                throw new IndexOutOfRangeException();

            var layer = _containerLayers[index];

            this.RemoveChild(GetTransform(layer));
            this._containerLayers.RemoveAt(index);
            return layer;
        }


        public bool Contains(IContainerLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            return this._containerLayers.Contains(layer);
        }

        public int IndexOf(IContainerLayer window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            return this._containerLayers.IndexOf(window);
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
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            this._containerLayers.Clear();
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

        protected virtual void RemoveChild(Transform child, bool worldPositionStays = false)
        {
            if (child == null || !this.transform.Equals(child.parent))
                return;

            child.SetParent(null, worldPositionStays);
        }

        protected virtual void AddChild(Transform child, bool worldPositionStays = false)
        {
            if (child == null || this.transform.Equals(child.parent))
                return;

            child.gameObject.layer = this.gameObject.layer;
            child.SetParent(this.transform, worldPositionStays);
            child.SetAsLastSibling();
        }
    }
}