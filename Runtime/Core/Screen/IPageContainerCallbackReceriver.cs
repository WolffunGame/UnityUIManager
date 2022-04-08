namespace UnityScreenNavigator.Runtime.Core.Screen
{
    public interface IScreenContainerCallbackReceiver
    {
        void BeforePush(Screen enterScreen, Screen exitScreen);

        void AfterPush(Screen enterScreen, Screen exitScreen);

        void BeforePop(Screen enterScreen, Screen exitScreen);

        void AfterPop(Screen enterScreen, Screen exitScreen);
    }
}