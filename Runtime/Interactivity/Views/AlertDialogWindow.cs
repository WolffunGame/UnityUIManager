using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.ViewModels;

namespace UnityScreenNavigator.Runtime.Interactivity.Views
{
    public class AlertDialogWindow : Modal
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private GameObject content;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button neutralButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button outsideButton;

        private IUIView _contentView;

        private AlertDialogViewModel _viewModel;

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

        public AlertDialogViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;
                OnChangeViewModel();
            }
        }

        protected virtual void Button_OnClick(int which)
        {
            try
            {
                _viewModel.OnClick(which);
            }
            finally
            {
                var dialogContainer = ModalContainer.Find(AlertDialog.DialogLayer);
                if (dialogContainer != null) dialogContainer.Pop(true);
            }
        }

        public virtual void Cancel()
        {
            Button_OnClick(AlertDialog.BUTTON_NEGATIVE);
        }

        protected void OnChangeViewModel()
        {
            if (messageText != null)
            {
                if (!string.IsNullOrEmpty(_viewModel.Message))
                {
                    messageText.gameObject.SetActive(true);
                    messageText.text = _viewModel.Message;
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
                if (!string.IsNullOrEmpty(_viewModel.Title))
                {
                    titleText.gameObject.SetActive(true);
                    titleText.text = _viewModel.Title;
                }
                else
                {
                    titleText.gameObject.SetActive(false);
                }
            }

            if (confirmButton != null)
            {
                if (!string.IsNullOrEmpty(_viewModel.ConfirmButtonText))
                {
                    confirmButton.gameObject.SetActive(true);
                    confirmButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.BUTTON_POSITIVE); });
                    var text = confirmButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = _viewModel.ConfirmButtonText;
                }
                else
                {
                    confirmButton.gameObject.SetActive(false);
                }
            }

            if (cancelButton != null)
            {
                if (!string.IsNullOrEmpty(_viewModel.CancelButtonText))
                {
                    cancelButton.gameObject.SetActive(true);
                    cancelButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.BUTTON_NEGATIVE); });
                    var text = cancelButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = _viewModel.CancelButtonText;
                }
                else
                {
                    cancelButton.gameObject.SetActive(false);
                }
            }

            if (neutralButton != null)
            {
                if (!string.IsNullOrEmpty(_viewModel.NeutralButtonText))
                {
                    neutralButton.gameObject.SetActive(true);
                    neutralButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.BUTTON_NEUTRAL); });
                    var text = neutralButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = _viewModel.NeutralButtonText;
                }
                else
                {
                    neutralButton.gameObject.SetActive(false);
                }
            }

            CanceledOnTouchOutside = _viewModel.CanceledOnTouchOutside;
            if (outsideButton != null && CanceledOnTouchOutside)
            {
                outsideButton.gameObject.SetActive(true);
                outsideButton.interactable = true;
                outsideButton.onClick.AddListener(() => { Button_OnClick(AlertDialog.BUTTON_NEGATIVE); });
            }
        }
        
    }
}