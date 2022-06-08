using System;
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

        public bool CanceledOnTouchOutside { get; set; }

        private IUIView _contentView;

        private AlertDialogViewModel _viewModel;

        public IUIView ContentView
        {
            get { return this._contentView; }
            set
            {
                if (this._contentView == value)
                    return;

                if (this._contentView != null)
                    GameObject.Destroy(this._contentView.Owner);

                this._contentView = value;
                if (this._contentView != null && this._contentView.Owner != null && this.content != null)
                {
                    this._contentView.Visibility = true;
                    this._contentView.RectTransform.SetParent(this.content.transform, false);
                    if (this.messageText != null)
                        this.messageText.gameObject.SetActive(false);
                }
            }
        }

        public AlertDialogViewModel ViewModel
        {
            get { return this._viewModel; }
            set
            {
                this._viewModel = value;
                this.OnChangeViewModel();
            }
        }

        protected virtual void Button_OnClick(int which)
        {
            try
            {
                this._viewModel.OnClick(which);
            }
            finally
            {
                var dialogContainer = ModalContainer.Find(AlertDialog.DialogLayer);
                if (dialogContainer != null)
                {
                    dialogContainer.Pop(true);
                }
            }
        }

        public virtual void Cancel()
        {
            this.Button_OnClick(AlertDialog.BUTTON_NEGATIVE);
        }

        protected void OnChangeViewModel()
        {
            if (this.messageText != null)
            {
                if (!string.IsNullOrEmpty(this._viewModel.Message))
                {
                    this.messageText.gameObject.SetActive(true);
                    this.messageText.text = this._viewModel.Message;
                    if (this._contentView != null && this._contentView.Visibility)
                        this._contentView.Visibility = false;
                }
                else
                    this.messageText.gameObject.SetActive(false);
            }

            if (this.titleText != null)
            {
                if (!string.IsNullOrEmpty(this._viewModel.Title))
                {
                    this.titleText.gameObject.SetActive(true);
                    this.titleText.text = this._viewModel.Title;
                }
                else
                    this.titleText.gameObject.SetActive(false);
            }

            if (this.confirmButton != null)
            {
                if (!string.IsNullOrEmpty(this._viewModel.ConfirmButtonText))
                {
                    this.confirmButton.gameObject.SetActive(true);
                    this.confirmButton.onClick.AddListener(() => { this.Button_OnClick(AlertDialog.BUTTON_POSITIVE); });
                    Text text = this.confirmButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = this._viewModel.ConfirmButtonText;
                }
                else
                {
                    this.confirmButton.gameObject.SetActive(false);
                }
            }

            if (this.cancelButton != null)
            {
                if (!string.IsNullOrEmpty(this._viewModel.CancelButtonText))
                {
                    this.cancelButton.gameObject.SetActive(true);
                    this.cancelButton.onClick.AddListener(() => { this.Button_OnClick(AlertDialog.BUTTON_NEGATIVE); });
                    Text text = this.cancelButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = this._viewModel.CancelButtonText;
                }
                else
                {
                    this.cancelButton.gameObject.SetActive(false);
                }
            }

            if (this.neutralButton != null)
            {
                if (!string.IsNullOrEmpty(this._viewModel.NeutralButtonText))
                {
                    this.neutralButton.gameObject.SetActive(true);
                    this.neutralButton.onClick.AddListener(() => { this.Button_OnClick(AlertDialog.BUTTON_NEUTRAL); });
                    Text text = this.neutralButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = this._viewModel.NeutralButtonText;
                }
                else
                {
                    this.neutralButton.gameObject.SetActive(false);
                }
            }

            this.CanceledOnTouchOutside = this._viewModel.CanceledOnTouchOutside;
            if (this.outsideButton != null && this.CanceledOnTouchOutside)
            {
                this.outsideButton.gameObject.SetActive(true);
                this.outsideButton.interactable = true;
                this.outsideButton.onClick.AddListener(() => { this.Button_OnClick(AlertDialog.BUTTON_NEGATIVE); });
            }
        }
    }
}