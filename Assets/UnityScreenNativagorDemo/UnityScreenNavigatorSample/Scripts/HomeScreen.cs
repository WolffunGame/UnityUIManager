using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;
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

        // ReSharper disable Unity.PerformanceAnalysis
        private async void OnShowAlertDialogClicked()
        {
            AlertDialog.DialogKey = "prefab_alert_dialog_tmp";
            AlertDialog.DialogLayer = "Modal_Container";
            // Show the alert dialog.
            var result = await AlertDialog.ShowMessage("Hello World", "This is the first dialog in the demo",
                "OK", "Cancel");
            // Wait for user to click the button.
            result.UserClick.Subscribe(b =>
            {
                if (b == AlertDialog.ButtonPositive)
                {
                    Debug.Log("Positive button clicked");
                    AlertDialog2();
                }
                else if (b == AlertDialog.ButtonNegative)
                {
                    Debug.Log("Negative button clicked");
                }
                else
                {
                    Debug.Log("Neutral button clicked");
                }
            });
        }
        
        private async void AlertDialog2()
        {
            AlertDialog.DialogKey = "prefab_alert_dialog_tmp_2";
            AlertDialog.DialogLayer = "Modal_Container";
            // Show the alert dialog.
            var result = await AlertDialog.ShowMessage("New Dialog", "123 456 789",
                "OK", "Cancel");
            // Wait for user to click the button.
            result.UserClick.Subscribe(b =>
            {
                if (b == AlertDialog.ButtonPositive)
                {
                    Debug.Log("Positive button clicked");
                    OnShowAlertDialogClicked();
                }
                else if (b == AlertDialog.ButtonNegative)
                {
                    Debug.Log("Negative button clicked");
                }
                else
                {
                    Debug.Log("Neutral button clicked");
                }
            });
        }

        private void OnShowToastClicked()
        {
            Toast.ToastKey = "DefaultToast";
            Toast.Show("This is the first toast in the demo", 2f).Forget();
        }


        public override UniTask Cleanup()
        {
            _settingButton.onClick.RemoveListener(OnSettingButtonClicked);
            _shopButton.onClick.RemoveListener(OnShopButtonClicked);
            ScreenContainer.Of(transform).ReleasePreloaded(ResourceKey.ShopPagePrefab());
            return UniTask.CompletedTask;
        }

        private async void OnSettingButtonClicked()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            AlertDialog2();
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            OnShowAlertDialogClicked();
        }

        private void OnShopButtonClicked()
        {
            var pushOption = new WindowOption(ResourceKey.ShopPagePrefab(), true, IsPoolable:true);
            ScreenContainer.Of(transform).Push(pushOption);
        }

        Tooltip _buttonTooltip;
      

        private async void OnShowTooltipClicked()
        {
            //lorem ipsum string for 100 words
            var str = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                      "Donec euismod, nisl eget consectetur sagittis, nisl nunc " +
                      "consectetur nisi, euismod aliquam nisi nisl euismod. ";
            //var result = await AddressablesManager.LoadAssetAsync<GameObject>("simple_tooltip_content");
            //var go = Instantiate(result.Value);
            //var view = go.GetComponent<SimpleTooltipContent>();
            _buttonTooltip = await Tooltip.Show(string.Empty, "view", TipPosition.BottomMiddle,  _showTooltip.image.rectTransform, 50);
            //Do something after tooltip is shown.
            Debug.Log("Tooltip shown");
            var closed = await _buttonTooltip.AfterHide.WaitAsync();
            if (closed)
            {
                Debug.Log("Tooltip closed");
            }
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