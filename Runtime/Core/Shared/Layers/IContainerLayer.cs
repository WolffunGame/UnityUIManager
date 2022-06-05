
namespace UnityScreenNavigator.Runtime.Core.Shared.Layers
{
    /// <summary>
    /// Layer for each container
    /// </summary>
    public interface IContainerLayer
    {
        int Layer { get;}
        string LayerName { get;}
        int VisibleElementInLayer { get;}
        
        ContainerLayerType LayerType { get; set; }
        IContainerLayerManager ContainerLayerManager { get; set; }
        void CreateLayer(string layerName, int layer, ContainerLayerType layerType);
        
        void OnBackButtonPressed();
    }

    public enum ContainerLayerType
    {
        Modal, Screen
    }
}