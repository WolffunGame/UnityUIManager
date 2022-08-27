
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared.Layers
{
    /// <summary>
    /// Layer for each container
    /// </summary>
    public interface IContainerLayer
    {
        int SortOrder { get;}
        string LayerName { get;}
        /// <summary>
        /// Element count in layer
        /// </summary>
        int VisibleElementInLayer { get;}
        /// <summary>
        /// Container for this layer
        /// </summary>
        ContainerLayerType LayerType { get; set; }
        
        IContainerLayerManager ContainerLayerManager { get; set; }
        /// <summary>
        /// Current window on the top in this layer
        /// </summary>
        public abstract Window Current { get; }
        /// <summary>
        /// Create layer
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="layer"></param>
        /// <param name="layerType"></param>
        void CreateLayer(string layerName, int layer, ContainerLayerType layerType);
        /// <summary>
        /// Handle back key on some device
        /// </summary>
        void OnBackButtonPressed();
    }

    public enum ContainerLayerType
    {
        Modal, Screen
    }
}