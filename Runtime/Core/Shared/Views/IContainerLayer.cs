namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    /// <summary>
    /// Layer for each container
    /// </summary>
    public interface IContainerLayer
    {
        int Layer { get; set; }
        string LayerName { get; set; }
        int VisibleElementInLayer { get;}
        
        ContainerLayerType LayerType { get; set; }
        IContainerLayerManager ContainerLayerManager { get; set; }
        void CreateLayer(string layerName, int layer, ContainerLayerType layerType);
    }

    public enum ContainerLayerType
    {
        Modal, Screen, Dynamic
    }
}