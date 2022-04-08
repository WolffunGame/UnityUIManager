namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    public interface IDynamicWindowContainerCallbackReceiver
    {
        void BeforeShow(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal);

        void AfterShow(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal);

        void BeforeHide(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal);

        void AfterHide(DynamicDynamicWindow enterModal, DynamicDynamicWindow exitModal);
    }
}