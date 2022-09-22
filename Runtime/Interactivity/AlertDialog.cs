using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.Views;
using Object = UnityEngine.Object;

namespace UnityScreenNavigator.Runtime.Interactivity
{
    public struct AlertDialog
    {
        public const int ButtonPositive = -1;
        public const int ButtonNegative = -2;
        public const int ButtonNeutral = -3;

        private static string _defaultDialogContainer = "Dialog";
        private const string DefaultDialogKey = "Prefabs/prefab_alert_dialog";

        private static string _dialogKey;

        public static string DialogKey
        {
            get { return string.IsNullOrEmpty(_dialogKey) ? DefaultDialogKey : _dialogKey; }
            set { _dialogKey = value; }
        }

        public static string DialogLayer
        {
            get => _defaultDialogContainer;
            set => _defaultDialogContainer = value;
        }


        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <param name="priority"></param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(
            string message,
            string title, int priority = 0)
        {
            return ShowMessage(message, title, null, null, null, true, priority);
        }

        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <param name="buttonText">The text shown in the only button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="priority"></param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(
            string message,
            string title,
            string buttonText, int priority = 0)
        {
            return ShowMessage(message, title, buttonText, null, null, false, priority);
        }

        /// <summary>
        /// Displays information to the user.
        /// </summary>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <param name="confirmButtonText">The text shown in the "confirm" button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="cancelButtonText">The text shown in the "cancel" button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="priority"></param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(
            string message,
            string title,
            string confirmButtonText,
            string cancelButtonText, int priority = 0)
        {
            return ShowMessage(message, title, confirmButtonText, null, cancelButtonText, false, priority);
        }

        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <param name="confirmButtonText">The text shown in the "confirm" button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="neutralButtonText">The text shown in the "neutral" button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="cancelButtonText">The text shown in the "cancel" button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="canceledOnTouchOutside">Whether the dialog box is canceled when 
        /// touched outside the window's bounds. </param>
        /// <param name="priority"></param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(
            string message,
            string title,
            string confirmButtonText,
            string neutralButtonText,
            string cancelButtonText,
            bool canceledOnTouchOutside = false, int priority = 0)
        {
            var dialog = new AlertDialog(title, message, confirmButtonText, neutralButtonText, cancelButtonText, canceledOnTouchOutside);

            return ShowMessage(DialogKey, dialog, priority);
        }

        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="contentView">The custom content view to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <param name="confirmButtonText">The text shown in the "confirm" button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="neutralButtonText">The text shown in the "neutral" button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="cancelButtonText">The text shown in the "cancel" button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="canceledOnTouchOutside">Whether the dialog box is canceled when 
        /// touched outside the window's bounds. </param>
        /// <param name="priority"></param>
        /// <returns>A AlertDialog.</returns>
        public static async UniTask<AlertDialog> ShowMessage(
            IUIView contentView,
            string title,
            string confirmButtonText,
            string neutralButtonText,
            string cancelButtonText,
            bool canceledOnTouchOutside = false, int priority = 0)
        {
            var dialog = new AlertDialog
            {
                Title = title,
                ConfirmButtonText = confirmButtonText,
                NeutralButtonText = neutralButtonText,
                CancelButtonText = cancelButtonText,
                CanceledOnTouchOutside = canceledOnTouchOutside,
                UserClick = new AsyncReactiveProperty<int>(0)
            };

            var modalContainer = ModalContainer.Find(_defaultDialogContainer);
            if (modalContainer == null)
            {
                modalContainer = ModalContainer.Create(_defaultDialogContainer, 10, ContainerLayerType.Modal);
            }
            
            var option = new WindowOption(DialogKey, true, priority);
            var window = (AlertDialogWindow) await modalContainer.Push(option);
            //var createdWindow = await option.WindowCreated.WaitAsync();
            //var window = (AlertDialogWindow)createdWindow;
            window.DialogModel.Value = dialog;
            window.ContentView = contentView;
            
            return dialog;
        }

        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="viewModel">The view model of the dialog box</param>
        /// <param name="priority"></param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(AlertDialog viewModel, int priority = 0)
        {
            return ShowMessage(DialogKey, viewModel, priority);
        }

        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="key">The view name of the dialog box,if it is null, use the default view name</param>
        /// <param name="viewModel">The view model of the dialog box</param>
        /// <param name="priority"></param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(string key, AlertDialog viewModel, int priority = 0)
        {
            return ShowMessage(key, null, viewModel, priority);
        }
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="key">The view name of the dialog box,if it is null, use the default view name</param>
        /// <param name="contentViewName">The custom content view name to be shown to the user.</param>
        /// <param name="dialog">The view model of the dialog box</param>
        /// <param name="priority"></param>
        /// <returns>A AlertDialog.</returns>
        private static async UniTask<AlertDialog> ShowMessage(string key, string contentViewName, AlertDialog dialog, int priority = 0)
        {
            AlertDialogWindow window = null;
            IUIView contentView = null;
            try
            {
                if (string.IsNullOrEmpty(key))
                    key = DialogKey;
                
                var modalContainer = ModalContainer.Find(_defaultDialogContainer);
                if (modalContainer == null)
                {
                    modalContainer = ModalContainer.Create(_defaultDialogContainer, 10, ContainerLayerType.Modal);
                }
                if (!string.IsNullOrEmpty(contentViewName))
                {
                    var contentGo = await AddressablesManager.LoadAssetAsync<GameObject>(contentViewName);
                    var content = Object.Instantiate(contentGo.Value);
                    contentView = content.GetComponent<IUIView>();   
                }

                var option = new WindowOption(key, true, priority);
                window = (AlertDialogWindow) await modalContainer.Push(option);
                
                window.DialogModel.Value = dialog;
                window.ContentView = contentView;
                return dialog;
            }
            catch (Exception e)
            {
                if (window != null)
                {
                    window.Cancel();
                }

                if (contentView != null)
                {
                    GameObject.Destroy(contentView.Owner);
                    AddressablesManager.ReleaseAsset(contentViewName);
                }
                Debug.LogError(e);
            }
            
            return default;
        }

        public AsyncReactiveProperty<int> UserClick { get; private set; }
        
        public string Title;
        public string Message;
        public string ConfirmButtonText;
        public string NeutralButtonText;
        public string CancelButtonText;
        public bool CanceledOnTouchOutside;
        
        public AlertDialog(string title, string message, string confirmButtonText, string neutralButtonText, string cancelButtonText, bool canceledOnTouchOutside)
        {
            Title = title;
            Message = message;
            ConfirmButtonText = confirmButtonText;
            NeutralButtonText = neutralButtonText;
            CancelButtonText = cancelButtonText;
            CanceledOnTouchOutside = canceledOnTouchOutside;
            UserClick = new AsyncReactiveProperty<int>(0);
        }

    }
}