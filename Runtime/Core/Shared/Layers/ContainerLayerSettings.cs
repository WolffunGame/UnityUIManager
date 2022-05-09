using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Layers
{
    [CreateAssetMenu(fileName = "Container Layer Setting New", menuName = "Screen Navigator/Container Layer Setting", order = 0)]
    public class ContainerLayerSettings : ScriptableObject
    {
        [SerializeField] private ContainerLayerConfiguration[] containerLayers;
        
        public ContainerLayerConfiguration[] GetContainerLayers()
        {
            return containerLayers;
        }
        [Button]
        private void AutoSortLayers()
        {
            //sort containerLayers by layer
            containerLayers = containerLayers.OrderBy(x => x.layer).ToArray();
        }
    }
    

    [Serializable]
    public class ContainerLayerConfiguration
    {
        public string name;
        [Range(0, 10)] public int layer;
        public ContainerLayerType layerType;
    }
}