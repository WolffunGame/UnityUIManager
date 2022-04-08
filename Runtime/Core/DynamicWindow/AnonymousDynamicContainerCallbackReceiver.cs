using System;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    public class AnonymousDynamicContainerCallbackReceiver: IDynamicWindowContainerCallbackReceiver
    {
        public AnonymousDynamicContainerCallbackReceiver(
            Action<(DynamicWindow enterModal, DynamicWindow exitModal)> onBeforeShow = null,
            Action<(DynamicWindow enterModal, DynamicWindow exitModal)> onAfterShow = null,
            Action<(DynamicWindow enterModal, DynamicWindow exitModal)> onBeforeHide = null,
            Action<(DynamicWindow enterModal, DynamicWindow exitModal)> onAfterHide = null)
        {
            OnBeforeShow = onBeforeShow;
            OnAfterShow = onAfterShow;
            OnBeforeHide = onBeforeHide;
            OnAfterHide = onAfterHide;
        }

        public void BeforeShow(DynamicWindow enterModal, DynamicWindow exitModal)
        {
            OnBeforeShow?.Invoke((enterModal, exitModal));
        }

        public void AfterShow(DynamicWindow enterModal, DynamicWindow exitModal)
        {
            OnAfterShow?.Invoke((enterModal, exitModal));
        }

        public void BeforeHide(DynamicWindow enterModal, DynamicWindow exitModal)
        {
            OnBeforeHide?.Invoke((enterModal, exitModal));
        }

        public void AfterHide(DynamicWindow enterModal, DynamicWindow exitModal)
        {
            OnAfterHide?.Invoke((enterModal, exitModal));
        }
        public event Action<(DynamicWindow enterModal, DynamicWindow exitModal)> OnAfterHide;
        public event Action<(DynamicWindow enterModal, DynamicWindow exitModal)> OnAfterShow;
        public event Action<(DynamicWindow enterModal, DynamicWindow exitModal)> OnBeforeHide;
        public event Action<(DynamicWindow enterModal, DynamicWindow exitModal)> OnBeforeShow;

    }
}