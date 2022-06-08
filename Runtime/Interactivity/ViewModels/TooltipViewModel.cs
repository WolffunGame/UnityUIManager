using System;
using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Interactivity.ViewModels
{
    public class TooltipViewModel : AsyncReactiveProperty<TooltipViewModel>
    {
        protected string _message;
        protected bool _closeOnCancelClick;
        protected Action _click;
        protected bool _closed;
        protected bool _stayOpen;
        protected IUIViewGroup _viewGroup;
        protected string _tooltipKey;

        public TooltipViewModel(TooltipViewModel value) : base(value)
        {
            Value = this;
        }
        
        /// <summary>
        /// The message to display in the tooltip.
        /// </summary>
        public virtual string Message
        {
            get => _message;
            set => Value._message = value;
        }
        

        /// <summary>
        /// If true, the tooltip will be hidden when the user clicks on the cancel button.
        /// If false, the tooltip will be hidden automatically if user click anywhere on the screen.
        /// </summary>
        public virtual bool CloseOnCancelClick
        {
            get => _closeOnCancelClick;
            set => Value._closeOnCancelClick = value;
        }
        
        /// <summary>
        /// Stay open after the user clicks on the cancel button.
        /// Otherwise, the tooltip will be hidden automatically.
        /// </summary>
        public virtual bool StayOpen
        {
            get => _stayOpen;
            set => Value._stayOpen = value;
        }

        /// <summary>
        /// Callback when the tooltip is clicked.
        /// </summary>
        public virtual Action Click
        {
            get => _click;
            set => Value._click = value;
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool Closed
        {
            get => _closed;
            set => Value._closed = value;
        }
        
        /// <summary>
        /// The view group that contains the tooltip.
        /// </summary>
        public virtual IUIViewGroup ViewGroup
        {
            get => _viewGroup;
            set => Value._viewGroup = value;
        }
        
        /// <summary>
        /// Key to load the tooltip from the resources.
        /// </summary>
        public virtual string TooltipKey
        {
            get => _tooltipKey;
            set => Value._tooltipKey = value;
        }

        public virtual void OnClick()
        {
            try
            {
                Click?.Invoke();
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                Closed = true;
            }
        }
        
    }
}