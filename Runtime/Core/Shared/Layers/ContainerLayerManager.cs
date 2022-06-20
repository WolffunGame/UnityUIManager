using System;
using System.Collections.Generic;
using UnityEngine;
using UnityScreenNavigator.Runtime.Foundation;

namespace UnityScreenNavigator.Runtime.Core.Shared.Layers
{
    [DisallowMultipleComponent]
    public class ContainerLayerManager : MonoBehaviour, IContainerLayerManager
    {
        private static readonly List<IContainerLayer> ContainerLayers = new List<IContainerLayer>();
        
        private IContainerLayer _currentContainerLayer;

        public bool Activated { get; set; }

        public int Count
        {
            get => ContainerLayers.Count;
        }

        public static IContainerLayer GetTopVisibilityLayer()
        {
            if (ContainerLayers == null || ContainerLayers.Count <= 0)
                return null;
            //current layer is the highest layer which is visible (VisibleElementInLayer > 0)
            for (int i = ContainerLayers.Count - 1; i >= 0; i--)
            {
                if (ContainerLayers[i].VisibleElementInLayer > 0)
                    return ContainerLayers[i];
            }
            return null;
        }

        public IEnumerator<IContainerLayer> Visibles()
        {
            foreach (var layer in ContainerLayers)
            {
                if (layer.VisibleElementInLayer > 0)
                    yield return layer;
            }
        }

        public IContainerLayer Get(int index)
        {
            if (index < 0 || index > ContainerLayers.Count - 1)
                throw new IndexOutOfRangeException();

            return ContainerLayers[index];
        }

        public void Add(IContainerLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            if (ContainerLayers.Contains(layer))
                return;

            ContainerLayers.Add(layer);
            transform.AddChild(GetTransform(layer));
        }

        public bool Remove(IContainerLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            return ContainerLayers.Remove(layer);
        }

        public IContainerLayer RemoveAt(int index)
        {
            if (index < 0 || index > ContainerLayers.Count - 1)
                throw new IndexOutOfRangeException();

            var layer = ContainerLayers[index];

            transform.RemoveChild(GetTransform(layer));
            ContainerLayers.RemoveAt(index);
            return layer;
        }


        public bool Contains(IContainerLayer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            return ContainerLayers.Contains(layer);
        }

        public int IndexOf(IContainerLayer window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            return ContainerLayers.IndexOf(window);
        }

        public List<IContainerLayer> Find(bool visible)
        {
            List<IContainerLayer> result = new List<IContainerLayer>();
            foreach (var layer in ContainerLayers)
            {
                if (layer.VisibleElementInLayer > 0 == visible)
                    result.Add(layer);
            }

            return result;
        }

        public T Find<T>() where T : IContainerLayer
        {
            return (T) ContainerLayers.Find(x => x is T);
        }

        public T Find<T>(string layerName) where T : IContainerLayer
        {
            return (T) ContainerLayers.Find(x => x is T && x.LayerName == layerName);
        }

        public List<T> FindAll<T>() where T : IContainerLayer
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            ContainerLayers.Clear();
        }

        protected virtual Transform GetTransform(IContainerLayer layer)
        {
            try
            {
                if (layer == null)
                    return null;

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

        private void OnDestroy()
        {
            ContainerLayers.Clear();
        }
    }
}