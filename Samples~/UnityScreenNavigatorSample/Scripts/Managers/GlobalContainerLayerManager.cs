using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;

namespace Demo.Scripts.Managers
{
    [DisallowMultipleComponent]
    public class GlobalContainerLayerManager : ContainerLayerManager
    {
        public static GlobalContainerLayerManager Root;

        protected void Start()
        {
            Root = this;
        }

        protected void OnDestroy()
        {
            Root = null;
        }

        private void Update()
        {
#if UNITY_EDITOR && ENABLE_LEGACY_INPUT_MANAGER
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                var currentLayer = GetTopVisibilityLayer();
                if (currentLayer != null)
                {
                    if (currentLayer.Layer == 0 && currentLayer.VisibleElementInLayer == 1)
                    {
                        Debug.Log("Application.Quit()");
                        return;
                    }

                    currentLayer.OnBackButtonPressed();
                }
            }
#else
            var currentLayer = GetTopVisibilityLayer();
            if (currentLayer != null)
            {
                if (currentLayer.Layer == 0 && currentLayer.VisibleElementInLayer == 1)
                {
                    Debug.Log("Application.Quit()");
                    return;
                }
                currentLayer.OnBackButtonPressed();
            }
#endif
        }
    }
}