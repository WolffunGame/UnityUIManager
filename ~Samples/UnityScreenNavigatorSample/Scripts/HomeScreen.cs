using System;
using _Samples.UnityScreenNavigatorSample.Scripts.Tooltips;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Screen;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Interactivity;
using Screen = UnityScreenNavigator.Runtime.Core.Screen.Screen;

namespace Demo.Scripts
{
    public class HomeScreen : Screen
    {
        [SerializeField] private Button _settingButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _showTooltip;
        [SerializeField] private Button _showToast;
        [SerializeField] private Button _showAlertDialog;

        public override async UniTask Initialize()
        {
            _settingButton.onClick.AddListener(OnSettingButtonClicked);
            _shopButton.onClick.AddListener(OnShopButtonClicked);
            _showTooltip.onClick.AddListener(OnShowTooltipClicked);
            _showToast.onClick.AddListener(OnShowToastClicked);
            _showAlertDialog.onClick.AddListener(OnShowAlertDialogClicked);

            // Preload the "Shop" page prefab.
            await ScreenContainer.Of(transform).Preload(ResourceKey.ShopPagePrefab());
            // Simulate loading time.
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }

        private async void OnShowAlertDialogClicked()
        {
            AlertDialog.DialogKey = "Prefabs/prefab_alert_dialog";
            var result = await DefaultDialogService.ShowDialog("Hello World", "This is the first dialog in the demo",
                "OK", "Cancel");
            
            if (result == AlertDialog.BUTTON_POSITIVE)
            {
                Debug.Log("Positive button clicked");
            }
            else if (result == AlertDialog.BUTTON_NEGATIVE)
            {
                Debug.Log("Negative button clicked");
            }
            else
            {
                Debug.Log("Neutral button clicked");
            }
        }

        private void OnShowToastClicked()
        {
            Toast.ToastKey = "Prefabs/DefaultToast";
            Toast.Show("This is the first toast in the demo", 2f).Forget();
        }


        public override UniTask Cleanup()
        {
            _settingButton.onClick.RemoveListener(OnSettingButtonClicked);
            _shopButton.onClick.RemoveListener(OnShopButtonClicked);
            ScreenContainer.Of(transform).ReleasePreloaded(ResourceKey.ShopPagePrefab());
            return UniTask.CompletedTask;
        }

        private void OnSettingButtonClicked()
        {
            var pushOption = new WindowOption(ResourceKey.SettingsModalPrefab(), true);
            ModalContainer.Find(ContainerKey.ModalContainerLayer).Push(pushOption);
        }

        private void OnShopButtonClicked()
        {
            var pushOption = new WindowOption(ResourceKey.ShopPagePrefab(), true);
            ScreenContainer.Of(transform).Push(pushOption);
        }

        Tooltip _buttonTooltip;
        // /// <summary>
        // /// Show tooltip without content.
        // /// </summary>
        // private async void OnShowTooltipClicked()
        // {
        //     //lorem ipsum string for 100 words
        //     var str = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
        //               "Donec euismod, nisl eget consectetur sagittis, nisl nunc " +
        //               "consectetur nisi, euismod aliquam nisi nisl euismod. ";
        //     _buttonTooltip = await Tooltip.Show(string.Empty, str, TipPosition.TopMiddle,
        //         _showTooltip.image.rectTransform, 50, false);
        // }

        private async void OnShowTooltipClicked()
        {
            //lorem ipsum string for 100 words
            var str = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                      "Donec euismod, nisl eget consectetur sagittis, nisl nunc " +
                      "consectetur nisi, euismod aliquam nisi nisl euismod. ";
            var result = await AddressablesManager.LoadAssetAsync<GameObject>("Prefabs/simple_tooltip_content");
            var go = Instantiate(result.Value);
            var view = go.GetComponent<SimpleTooltipContent>();
            _buttonTooltip = await Tooltip.Show(string.Empty, view, TipPosition.BottomMiddle,  _showTooltip.image.rectTransform, 50);
        }
        
        public void ChangeToolTipText()
        {
            _buttonTooltip.Message.Value = "This is the new tooltip text";
        }
    }
    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(HomeScreen))]
    public class HomeScreenEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var homeScreen = target as HomeScreen;
            if (GUILayout.Button("Change Tooltip Text"))
            {
                homeScreen.ChangeToolTipText();
            }
        }
    }
    #endif
}