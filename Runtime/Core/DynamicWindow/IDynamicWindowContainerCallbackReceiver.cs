namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    public interface IDynamicWindowContainerCallbackReceiver
    {
        void BeforeShow(DynamicWindow enterModal, DynamicWindow exitModal);

        void AfterShow(DynamicWindow enterModal, DynamicWindow exitModal);

        void BeforeHide(DynamicWindow enterModal, DynamicWindow exitModal);

        void AfterHide(DynamicWindow enterModal, DynamicWindow exitModal);
    }
}