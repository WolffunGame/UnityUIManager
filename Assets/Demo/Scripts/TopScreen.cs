using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Screen;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace Demo.Scripts
{
    public class TopScreen : UnityScreenNavigator.Runtime.Core.Screen.Screen
    {
        [SerializeField] private Button _button;

        protected override void Start()
        {
            _button.onClick.AddListener(OnClick);
        }

        protected override void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            var option = new WindowOption(ResourceKey.HomeLoadingPagePrefab(), true, false);
            ScreenContainer.Of(transform).Push(option);
        }
    }
}
