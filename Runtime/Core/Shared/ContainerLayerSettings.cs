using System;
using System.Linq;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    [CreateAssetMenu(fileName = "Container Layer Setting New", menuName = "Screen Navigator/Container Layer Setting", order = 0)]
    public class ContainerLayerSettings : ScriptableObject
    {
        [SerializeField] private ContainerLayerConfiguration[] containerLayers;
        
        public ContainerLayerConfiguration[] GetContainerLayers()
        {
            return containerLayers;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            //sort containerLayers by layer
            containerLayers = containerLayers.OrderBy(x => x.layer).ToArray();
        }
#endif

    }
    

    [Serializable]
    public class ContainerLayerConfiguration
    {
        public string name;
        [Range(0, 10)] public int layer;
        public ContainerLayerType layerType;
    }
}