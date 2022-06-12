using UnityEngine;
using UnityEngine.Serialization;
using UnityScreenNavigator.Runtime.Core.Screen;
using UnityScreenNavigator.Runtime.Core.Shared;

namespace Demo.Scripts
{
    public class RootPageManager : MonoBehaviour
    {
        [FormerlySerializedAs("_pageContainer")] [SerializeField]
        private ScreenContainer screenContainer;

        private void Start()
        {
            var option = new WindowOption(ResourceKey.TopPagePrefab(), false, loadAsync: false);
            screenContainer.Push(option);
        }
    }
}