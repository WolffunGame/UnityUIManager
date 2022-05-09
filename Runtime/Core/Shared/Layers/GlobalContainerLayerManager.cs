using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Layers
{
    [DisallowMultipleComponent]
    public class GlobalContainerLayerManager : ContainerLayerManager
    {
        public static GlobalContainerLayerManager Root;

        protected override void Start()
        {
            base.Start();
            Root = this;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Root = null;
        }
    }
}