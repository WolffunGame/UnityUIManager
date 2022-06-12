using System;
using UnityEditor;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Screen;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;

namespace Demo.Scripts
{
    public class LayerCreator : MonoBehaviour
    {
        [SerializeField] private ContainerLayerSettings containerLayerSettings;

        private void Awake()
        {
            if (transform.childCount > 0) throw new Exception("LayerCreator should not have children");
            InitializeLayers();
        }


        public void InitializeLayers()
        {
            var layers = containerLayerSettings.GetContainerLayers();
            for (var i = 0; i < layers.Length; i++)
                switch (layers[i].layerType)
                {
                    case ContainerLayerType.Modal:
                        var modalContainer =
                            ModalContainer.Create(layers[i].name, layers[i].layer, layers[i].layerType);
                        modalContainer.transform.SetParent(transform);
                        break;
                    case ContainerLayerType.Screen:
                        var screenContainer =
                            ScreenContainer.Create(layers[i].name, layers[i].layer, layers[i].layerType);
                        screenContainer.transform.SetParent(transform);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }

        private void RemoveLayers()
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(LayerCreator))]
    public class LayerCreatorEditor : Editor
    {
        private LayerCreator _layerCreator;

        private void OnEnable()
        {
            _layerCreator = (LayerCreator) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Create Layers"))
            {
                _layerCreator.InitializeLayers();
            }
        }
    }
#endif
}