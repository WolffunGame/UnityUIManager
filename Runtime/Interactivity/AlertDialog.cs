using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.ViewModels;
using UnityScreenNavigator.Runtime.Interactivity.Views;

namespace UnityScreenNavigator.Runtime.Interactivity
{
    public class AlertDialog
    {
        public const int BUTTON_POSITIVE = -1;
        public const int BUTTON_NEGATIVE = -2;
        public const int BUTTON_NEUTRAL = -3;

        private const string DEFAULT_DIALOG_CONTAINER = "Dialog";
        private const string DEFAULT_VIEW_NAME = "Prefabs/prefab_alert_dialog";

        private static string viewName;

        public static string ViewName
        {
            get { return string.IsNullOrEmpty(viewName) ? DEFAULT_VIEW_NAME : viewName; }
            set { viewName = value; }
        }


        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(
            string message,
            string title)
        {
            return ShowMessage(message, title, null, null, null, true, null);
        }

        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <param name="buttonText">The text shown in the only button
        /// in the dialog box. If left null, the button will be invisible.</param>
        /// <param name="afterHideCallback">A callback that should be executed after
        /// the dialog box is closed by the user.</param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(
            string message,
            string title,
            string buttonText,
            Action<int> afterHideCallback)
        {
            return ShowMessage(message, title, buttonText, null, null, false, afterHideCallback);
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
        /// <param name="afterHideCallback">A callback that should be executed after
        /// the dialog box is closed by the user. The callback method will get a boolean
        /// parameter indicating if the "confirm" button (true) or the "cancel" button
        /// (false) was pressed by the user.</param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(
            string message,
            string title,
            string confirmButtonText,
            string cancelButtonText,
            Action<int> afterHideCallback)
        {
            return ShowMessage(message, title, confirmButtonText, null, cancelButtonText, false, afterHideCallback);
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
        /// <param name="afterHideCallback">A callback that should be executed after
        /// the dialog box is closed by the user. The callback method will get a boolean
        /// parameter indicating if the "confirm" button (true) or the "cancel" button
        /// (false) was pressed by the user.</param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(
            string message,
            string title,
            string confirmButtonText,
            string neutralButtonText,
            string cancelButtonText,
            bool canceledOnTouchOutside,
            Action<int> afterHideCallback)
        {
            AlertDialogViewModel viewModel = new AlertDialogViewModel(default);
            viewModel.Message = message;
            viewModel.Title = title;
            viewModel.ConfirmButtonText = confirmButtonText;
            viewModel.NeutralButtonText = neutralButtonText;
            viewModel.CancelButtonText = cancelButtonText;
            viewModel.CanceledOnTouchOutside = canceledOnTouchOutside;
            viewModel.Click = afterHideCallback;

            return ShowMessage(ViewName, viewModel);
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
        /// <param name="afterHideCallback">A callback that should be executed after
        /// the dialog box is closed by the user. The callback method will get a boolean
        /// parameter indicating if the "confirm" button (true) or the "cancel" button
        /// (false) was pressed by the user.</param>
        /// <returns>A AlertDialog.</returns>
        public static async UniTask<AlertDialog> ShowMessage(
            IUIView contentView,
            string title,
            string confirmButtonText,
            string neutralButtonText,
            string cancelButtonText,
            bool canceledOnTouchOutside,
            Action<int> afterHideCallback)
        {
            AlertDialogViewModel viewModel = new AlertDialogViewModel(default);
            viewModel.Title = title;
            viewModel.ConfirmButtonText = confirmButtonText;
            viewModel.NeutralButtonText = neutralButtonText;
            viewModel.CancelButtonText = cancelButtonText;
            viewModel.CanceledOnTouchOutside = canceledOnTouchOutside;
            viewModel.Click = afterHideCallback;

            var modalContainer = ModalContainer.Find(DEFAULT_DIALOG_CONTAINER);
            if (modalContainer == null)
            {
                modalContainer = ModalContainer.Create(DEFAULT_DIALOG_CONTAINER, 10, ContainerLayerType.Modal);
            }

            AlertDialogWindow window = null;
            var option = new WindowOption(ViewName, true);
            option.WindowCreated = (w) =>
            {
                window = (AlertDialogWindow)w;
                window.ViewModel = viewModel;
                window.ContentView = contentView;
            };
            window = (AlertDialogWindow) await modalContainer.Push(option);
            //TODO Load view here

            AlertDialog dialog = new AlertDialog(window, viewModel);
            return dialog;
        }

        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="viewModel">The view model of the dialog box</param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(AlertDialogViewModel viewModel)
        {
            return ShowMessage(ViewName, null, viewModel);
        }

        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="key">The view name of the dialog box,if it is null, use the default view name</param>
        /// <param name="viewModel">The view model of the dialog box</param>
        /// <returns>A AlertDialog.</returns>
        public static UniTask<AlertDialog> ShowMessage(string key, AlertDialogViewModel viewModel)
        {
            return ShowMessage(key, null, viewModel);
        }

        /// <summary>
        /// Displays information to the user. 
        /// </summary>
        /// <param name="key">The view name of the dialog box,if it is null, use the default view name</param>
        /// <param name="contentViewName">The custom content view name to be shown to the user.</param>
        /// <param name="viewModel">The view model of the dialog box</param>
        /// <returns>A AlertDialog.</returns>
        public static async UniTask<AlertDialog> ShowMessage(string key, string contentViewName,
            AlertDialogViewModel viewModel)
        {
            AlertDialogWindow window = null;
            IUIView contentView = null;
            try
            {
                if (string.IsNullOrEmpty(key))
                    key = ViewName;
                
                var modalContainer = ModalContainer.Find(DEFAULT_DIALOG_CONTAINER);
                if (modalContainer == null)
                {
                    modalContainer = ModalContainer.Create(DEFAULT_DIALOG_CONTAINER, 10, ContainerLayerType.Modal);
                }
                if (!string.IsNullOrEmpty(contentViewName))
                {
                    var contentGo = await AddressablesManager.LoadAssetAsync<GameObject>(contentViewName);
                    contentView = contentGo.Value.GetComponent<IUIView>();
                }

                var option = new WindowOption(key, true);
                option.WindowCreated = (w) =>
                {
                    window = (AlertDialogWindow)w;
                    window.ViewModel = viewModel;
                    window.ContentView = contentView;
                };
                window = (AlertDialogWindow) await modalContainer.Push(option);
                return new AlertDialog(window, viewModel);
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

                throw e;
            }
        }

        private AlertDialogWindow window;
        private AlertDialogViewModel viewModel;

        public AlertDialog(AlertDialogWindow window, AlertDialogViewModel viewModel)
        {
            this.window = window;
            this.viewModel = viewModel;
        }


        public UniTask WaitForClosed()
        {
            return UniTask.WaitUntil(() => !this.viewModel.Closed);
        }

        public void Cancel()
        {
            this.window.Cancel();
        }
    }
}