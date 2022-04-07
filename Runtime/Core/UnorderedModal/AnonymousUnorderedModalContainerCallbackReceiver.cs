using System;

namespace UnityScreenNavigator.Runtime.Core.UnorderedModal
{
    public class AnonymousUnorderedModalContainerCallbackReceiver: IUnorderedModalContainerCallbackReceiver
    {
        public AnonymousUnorderedModalContainerCallbackReceiver(
            Action<(UnorderedModal enterModal, UnorderedModal exitModal)> onBeforeShow = null,
            Action<(UnorderedModal enterModal, UnorderedModal exitModal)> onAfterShow = null,
            Action<(UnorderedModal enterModal, UnorderedModal exitModal)> onBeforeHide = null,
            Action<(UnorderedModal enterModal, UnorderedModal exitModal)> onAfterHide = null)
        {
            OnBeforeShow = onBeforeShow;
            OnAfterShow = onAfterShow;
            OnBeforeHide = onBeforeHide;
            OnAfterHide = onAfterHide;
        }

        public void BeforeShow(UnorderedModal enterModal, UnorderedModal exitModal)
        {
            OnBeforeShow?.Invoke((enterModal, exitModal));
        }

        public void AfterShow(UnorderedModal enterModal, UnorderedModal exitModal)
        {
            OnAfterShow?.Invoke((enterModal, exitModal));
        }

        public void BeforeHide(UnorderedModal enterModal, UnorderedModal exitModal)
        {
            OnBeforeHide?.Invoke((enterModal, exitModal));
        }

        public void AfterHide(UnorderedModal enterModal, UnorderedModal exitModal)
        {
            OnAfterHide?.Invoke((enterModal, exitModal));
        }
        public event Action<(UnorderedModal enterModal, UnorderedModal exitModal)> OnAfterHide;
        public event Action<(UnorderedModal enterModal, UnorderedModal exitModal)> OnAfterShow;
        public event Action<(UnorderedModal enterModal, UnorderedModal exitModal)> OnBeforeHide;
        public event Action<(UnorderedModal enterModal, UnorderedModal exitModal)> OnBeforeShow;

    }
}