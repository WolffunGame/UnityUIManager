using Cysharp.Threading.Tasks;
using Demo.Scripts.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityScreenNavigator.Runtime.Core.Screen;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;

namespace Demo.Scripts
{
    public class Launcher : MonoBehaviour
    {
        private GlobalContainerLayerManager _globalContainerLayerManager;
        private ScreenContainer _screenContainer;

        private async UniTaskVoid Start()
        {
            await UniTask.Delay(200);
            _globalContainerLayerManager = GetComponent<GlobalContainerLayerManager>();
            _screenContainer = _globalContainerLayerManager.Find<ScreenContainer>(ContainerKey.MainContainerLayer);
            var option = new WindowOption(ResourceKey.TopPagePrefab(), true);
            _screenContainer.Push(option).Forget();
        }
        

        public void DownloadAddressables()
        {
            var handle = Addressables.DownloadDependenciesAsync("", Addressables.MergeMode.None, true);
        }
    }
}