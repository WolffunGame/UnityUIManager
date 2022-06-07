using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.ViewModels;

namespace UnityScreenNavigator.Runtime.Interactivity.Views
{
    public class AlertDialogWindow : Modal
    {
        public Text Title;

        public Text Message;

        public GameObject Content;

        public Button ConfirmButton;

        public Button NeutralButton;

        public Button CancelButton;

        public Button OutsideButton;

        public bool CanceledOnTouchOutside { get; set; }

        private IUIView contentView;

        private AlertDialogViewModel viewModel;

        public IUIView ContentView
        {
            get { return this.contentView; }
            set
            {
                if (this.contentView == value)
                    return;

                if (this.contentView != null)
                    GameObject.Destroy(this.contentView.Owner);

                this.contentView = value;
                if (this.contentView != null && this.contentView.Owner != null && this.Content != null)
                {
                    this.contentView.Visibility = true;
                    this.contentView.RectTransform.SetParent(this.Content.transform, false);
                    if (this.Message != null)
                        this.Message.gameObject.SetActive(false);
                }
            }
        }

        public AlertDialogViewModel ViewModel
        {
            get { return this.viewModel; }
            set
            {
                this.viewModel = value;
                this.OnChangeViewModel();
            }
        }

        public override UniTask Initialize()
        {
            //OnChangeViewModel();
            return base.Initialize();
        }

        protected virtual void Button_OnClick(int which)
        {
            try
            {
                this.viewModel.OnClick(which);
            }
            catch (Exception) { }
            finally
            {
                //TODO: ModalContainer Pop this modal
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
            if (this.Message != null)
            {
                if (!string.IsNullOrEmpty(this.viewModel.Message))
                {
                    this.Message.gameObject.SetActive(true);
                    this.Message.text = this.viewModel.Message;
                    if (this.contentView != null && this.contentView.Visibility)
                        this.contentView.Visibility = false;
                }
                else
                    this.Message.gameObject.SetActive(false);
            }

            if (this.Title != null)
            {
                if (!string.IsNullOrEmpty(this.viewModel.Title))
                {
                    this.Title.gameObject.SetActive(true);
                    this.Title.text = this.viewModel.Title;
                }
                else
                    this.Title.gameObject.SetActive(false);
            }

            if (this.ConfirmButton != null)
            {
                if (!string.IsNullOrEmpty(this.viewModel.ConfirmButtonText))
                {
                    this.ConfirmButton.gameObject.SetActive(true);
                    this.ConfirmButton.onClick.AddListener(() => { this.Button_OnClick(AlertDialog.BUTTON_POSITIVE); });
                    Text text = this.ConfirmButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = this.viewModel.ConfirmButtonText;
                }
                else
                {
                    this.ConfirmButton.gameObject.SetActive(false);
                }
            }

            if (this.CancelButton != null)
            {
                if (!string.IsNullOrEmpty(this.viewModel.CancelButtonText))
                {
                    this.CancelButton.gameObject.SetActive(true);
                    this.CancelButton.onClick.AddListener(() => { this.Button_OnClick(AlertDialog.BUTTON_NEGATIVE); });
                    Text text = this.CancelButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = this.viewModel.CancelButtonText;
                }
                else
                {
                    this.CancelButton.gameObject.SetActive(false);
                }
            }

            if (this.NeutralButton != null)
            {
                if (!string.IsNullOrEmpty(this.viewModel.NeutralButtonText))
                {
                    this.NeutralButton.gameObject.SetActive(true);
                    this.NeutralButton.onClick.AddListener(() => { this.Button_OnClick(AlertDialog.BUTTON_NEUTRAL); });
                    Text text = this.NeutralButton.GetComponentInChildren<Text>();
                    if (text != null)
                        text.text = this.viewModel.NeutralButtonText;
                }
                else
                {
                    this.NeutralButton.gameObject.SetActive(false);
                }
            }

            this.CanceledOnTouchOutside = this.viewModel.CanceledOnTouchOutside;
            if (this.OutsideButton != null && this.CanceledOnTouchOutside)
            {
                this.OutsideButton.gameObject.SetActive(true);
                this.OutsideButton.interactable = true;
                this.OutsideButton.onClick.AddListener(() => { this.Button_OnClick(AlertDialog.BUTTON_NEGATIVE); });
            }
        }
    }
}