using System;
using Cysharp.Threading.Tasks;

namespace UnityScreenNavigator.Runtime.Interactivity.ViewModels
{
    public class AlertDialogViewModel : AsyncReactiveProperty<AlertDialogViewModel>
    {
        protected string title;
        protected string message;
        protected string confirmButtonText;
        protected string neutralButtonText;
        protected string cancelButtonText;
        protected bool canceledOnTouchOutside;
        protected bool closed;
        protected int result;
        protected Action<int> click;

        public AlertDialogViewModel(AlertDialogViewModel value) : base(value)
        {
        }

        /// <summary>
        /// The title of the dialog box. This may be null.
        /// </summary>
        public virtual string Title
        {
            get { return this.title; }
            set { Value.title = value; }
        }

        /// <summary>
        /// The message to be shown to the user.
        /// </summary>
        public virtual string Message
        {
            get { return this.message; }
            set { Value.message = value; }
        }

        /// <summary>
        /// The text shown in the "confirm" button in the dialog box. 
        /// If left null, the button will be invisible.
        /// </summary>
        public virtual string ConfirmButtonText
        {
            get { return this.confirmButtonText; }
            set { Value.confirmButtonText = value; }
        }

        /// <summary>
        /// The text shown in the "neutral" button in the dialog box. 
        /// If left null, the button will be invisible.
        /// </summary>
        public virtual string NeutralButtonText
        {
            get { return this.neutralButtonText; }
            set { Value.neutralButtonText = value; }
        }

        /// <summary>
        /// The text shown in the "cancel" button in the dialog box. 
        /// If left null, the button will be invisible.
        /// </summary>
        public virtual string CancelButtonText
        {
            get { return this.cancelButtonText; }
            set { Value.cancelButtonText = value; }
        }

        /// <summary>
        /// Whether the dialog box is canceled when 
        /// touched outside the window's bounds. 
        /// </summary>
        public virtual bool CanceledOnTouchOutside
        {
            get { return this.canceledOnTouchOutside; }
            set { Value.canceledOnTouchOutside = value; }
        }

        /// <summary>
        /// A callback that should be executed after
        /// the dialog box is closed by the user. The callback method will get a boolean
        /// parameter indicating if the "confirm" button (true) or the "cancel" button
        /// (false) was pressed by the user.
        /// </summary>
        public virtual Action<int> Click
        {
            get { return this.click; }
            set { Value.click = value; }
        }

        /// <summary>
        /// The dialog box has been closed.
        /// </summary>
        public virtual bool Closed
        {
            get { return this.closed; }
            set { Value.closed = value; }
        }

        /// <summary>
        /// result
        /// </summary>
        public virtual int Result
        {
            get { return this.result; }
        }
        
        UniTaskCompletionSource<int> taskCompletion;
 
        // await until button clicked
        public UniTask<int> WaitUntilClicked => taskCompletion.Task;

        public virtual void OnClick(int which)
        {
            try
            {
                this.result = which;
                var action = this.Click;
                if (action != null)
                    action(which);
            }
            catch (Exception)
            {
            }
            finally
            {
                this.Closed = true;
            }
        }


    }
}