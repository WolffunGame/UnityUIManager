using System;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    public class AnonymousDynamicContainerCallbackReceiver: IDynamicWindowContainerCallbackReceiver
    {
        public AnonymousDynamicContainerCallbackReceiver(
            Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> onBeforeShow = null,
            Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> onAfterShow = null,
            Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> onBeforeHide = null,
            Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> onAfterHide = null)
        {
            OnBeforeShow = onBeforeShow;
            OnAfterShow = onAfterShow;
            OnBeforeHide = onBeforeHide;
            OnAfterHide = onAfterHide;
        }

        public void BeforeShow(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)
        {
            OnBeforeShow?.Invoke((enterModal, exitModal));
        }

        public void AfterShow(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)
        {
            OnAfterShow?.Invoke((enterModal, exitModal));
        }

        public void BeforeHide(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)
        {
            OnBeforeHide?.Invoke((enterModal, exitModal));
        }

        public void AfterHide(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)
        {
            OnAfterHide?.Invoke((enterModal, exitModal));
        }
        public event Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> OnAfterHide;
        public event Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> OnAfterShow;
        public event Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> OnBeforeHide;
        public event Action<(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal)> OnBeforeShow;

    }
}