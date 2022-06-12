using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared.Layers
{
    public abstract class ContainerLayer : MonoBehaviour, IContainerLayer
    {
        public int Layer { get; protected set; }
        [SerializeField] private string layerName;

        public string LayerName
        {
            get
            {
                if (string.IsNullOrEmpty(layerName))
                {
                    layerName = gameObject.name;
                }

                return layerName;
            }
            protected set => layerName = value;
        }

        public abstract int VisibleElementInLayer { get; }
        public ContainerLayerType LayerType { get; set; }

        public abstract Window Current { get; }

        private IContainerLayerManager _containerLayerManager;

        public IContainerLayerManager ContainerLayerManager
        {
            get { return _containerLayerManager ??= FindObjectOfType<ContainerLayerManager>(); }
            set { _containerLayerManager = value; }
        }

        public void CreateLayer(string layerName, int layer, ContainerLayerType layerType)
        {
            LayerName = layerName;
            Layer = layer;
            LayerType = layerType;
            OnCreate();
            ContainerLayerManager.Add(this);
        }

        public abstract void OnBackButtonPressed();

        protected abstract void OnCreate();
    }
}