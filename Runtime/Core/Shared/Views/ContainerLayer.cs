
namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    public abstract class ContainerLayer : WindowView, IContainerLayer
    {
        public int Layer { get; set; }
        public string LayerName { get; set; }
        public abstract int VisibleElementInLayer { get; }
        public ContainerLayerType LayerType { get; set; }
        private IContainerLayerManager _containerLayerManager;

        public IContainerLayerManager ContainerLayerManager
        {
            get
            {
                return _containerLayerManager ??= FindObjectOfType<GlobalContainerLayerManager>();
            }
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

        protected abstract void OnCreate();
    }
}