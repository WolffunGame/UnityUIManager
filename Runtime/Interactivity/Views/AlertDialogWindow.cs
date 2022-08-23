using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Interactivity.Views
{
    public sealed class AlertDialogWindow : Modal
    {
#if USN_USE_TEXTMESHPRO
        [SerializeField] private TMPro.TextMeshProUGUI titleText;
        [SerializeField] private TMPro.TextMeshProUGUI messageText;
#else
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
#endif

        [SerializeField] private GameObject content;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button neutralButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button outsideButton;

        private IUIView _contentView;
        
        public bool CanceledOnTouchOutside { get; set; }

        public IUIView ContentView
        {
            get => _contentView;
            set
            {
                if (_contentView == value)
                    return;

                if (_contentView != null)
                    Destroy(_contentView.Owner);

                _contentView = value;
                if (_contentView != null && _contentView.Owner != null && content != null)
                {
                    _contentView.Visibility = true;
                    _contentView.RectTransform.SetParent(content.transform, false);
                    if (messageText != null)
                        messageText.gameObject.SetActive(false);
                }
            }
        }
        
        public override UniTask Initialize()
        {
            DialogModel.Subscribe(OnModelChanged);
            return base.Initialize();
        }

        public AsyncReactiveProperty<AlertDialog> DialogModel { get; } =
            new AsyncReactiveProperty<AlertDialog>(default);

        private void Button_OnClick(int which)
        {
            try
            {
                DialogModel.Value.UserClick.Value = which;
            }
            finally
            {
                var dialogContainer = ModalContainer.Find(AlertDialog.DialogLayer);
                if (dialogContainer != null) dialogContainer.Pop(true);
            }
        }

        public void Cancel()
        {
            Button_OnClick(AlertDialog.ButtonNegative);
        }

        private void OnModelChanged(AlertDialog dialog)
        {
            if (messageText != null)
            {
                if (!string.IsNullOrEmpty(dialog.Message))
                {
                    messageText.gameObject.SetActive(true);
                    messageText.text = dialog.Message;
                    if (_contentView != null && _contentView.Visibility)
                        _contentView.Visibility = false;
                }
                else
                {
                    messageText.gameObject.SetActive(false);
                }
            }

            if (titleText != null)
            {
                if (!string.IsNullOrEmpty(dialog.Title))
                {
                    titleText.gameObject.SetActive(true);
                    titleText.text = dialog.Title;
                }
                else
                {
                    titleText.gameObject.SetActive(false);
                }
            }

            if (confirmButton != null)
            {
                if (!string.IsNullOrEmpty(dialog.ConfirmButtonText))
                {
                    confirmButton.gameObject.SetActive(true);
                    confirmButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.ButtonPositive); });
#if USN_USE_TEXTMESHPRO
                    var text = confirmButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
#else
                    var text = confirmButton.GetComponentInChildren<Text>();
#endif

                    if (text != null)
                        text.text = dialog.ConfirmButtonText;
                }
                else
                {
                    confirmButton.gameObject.SetActive(false);
                }
            }

            if (cancelButton != null)
            {
                if (!string.IsNullOrEmpty(dialog.CancelButtonText))
                {
                    cancelButton.gameObject.SetActive(true);
                    cancelButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.ButtonNegative); });
#if USN_USE_TEXTMESHPRO
                    var text = cancelButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
#else
                    var text = cancelButton.GetComponentInChildren<Text>();
#endif
                    if (text != null)
                        text.text = dialog.CancelButtonText;
                }
                else
                {
                    cancelButton.gameObject.SetActive(false);
                }
            }

            if (neutralButton != null)
            {
                if (!string.IsNullOrEmpty(dialog.NeutralButtonText))
                {
                    neutralButton.gameObject.SetActive(true);
                    neutralButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.ButtonNeutral); });
#if USN_USE_TEXTMESHPRO
                    var text = neutralButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
#else
                    var text = neutralButton.GetComponentInChildren<Text>();
#endif
                    if (text != null)
                        text.text = dialog.NeutralButtonText;
                }
                else
                {
                    neutralButton.gameObject.SetActive(false);
                }
            }

            CanceledOnTouchOutside = dialog.CanceledOnTouchOutside;
            if (outsideButton != null && CanceledOnTouchOutside)
            {
                outsideButton.gameObject.SetActive(true);
                outsideButton.interactable = true;
                outsideButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.ButtonNegative); });
            }
        }
    }
}