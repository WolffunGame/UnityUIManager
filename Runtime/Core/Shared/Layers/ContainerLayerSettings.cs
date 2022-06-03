using System;
using System.Linq;
using UnityEditor;

#if ODIN_INSPECTOR_3 || ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

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
#if ODIN_INSPECTOR_3 || ODIN_INSPECTOR
        [Button]
#endif

        public void AutoSortLayers()
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
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(ContainerLayerSettings))]
    public class ContainerLayerSettingsEditor : Editor
    {
        private ContainerLayerSettings _containerLayerSettings;
        private void OnEnable()
        {
            _containerLayerSettings = (ContainerLayerSettings) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Auto Sort Layers"))
            {
                _containerLayerSettings.AutoSortLayers();
            }
        }
    }
#endif
}