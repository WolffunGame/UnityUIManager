using Demo.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Screen;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using Screen = UnityScreenNavigator.Runtime.Core.Screen.Screen;

namespace Demo.Scripts.Demo
{
    public class Screen4 : Screen
    {
        [SerializeField] private Button popButton;
        private ScreenContainer _screenContainer;

        protected override void Start()
        {
            base.Start();
            popButton.onClick.AddListener(Pop);

            //Get global layer manager
            var layerManager = GlobalContainerLayerManager.Root;
            _screenContainer = layerManager.Find<ScreenContainer>();
        }

        private void Pop()
        {
            _screenContainer.Pop(true);
        }
    }
}