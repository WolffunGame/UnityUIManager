namespace UnityScreenNavigator.Runtime.Core.UnorderedModal
{
    public interface IUnorderedModalContainerCallbackReceiver
    {
        void BeforeShow(UnorderedModal enterModal, UnorderedModal exitModal);

        void AfterShow(UnorderedModal enterModal, UnorderedModal exitModal);

        void BeforeHide(UnorderedModal enterModal, UnorderedModal exitModal);

        void AfterHide(UnorderedModal enterModal, UnorderedModal exitModal);
    }
}