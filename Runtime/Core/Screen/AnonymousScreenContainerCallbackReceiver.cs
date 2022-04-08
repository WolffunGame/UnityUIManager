using System;

namespace UnityScreenNavigator.Runtime.Core.Screen
{
    public sealed class AnonymousScreenContainerCallbackReceiver : IScreenContainerCallbackReceiver
    {
        public AnonymousScreenContainerCallbackReceiver(
            Action<(Screen enterScreen, Screen exitScreen)> onBeforePush = null,
            Action<(Screen enterScreen, Screen exitScreen)> onAfterPush = null,
            Action<(Screen enterScreen, Screen exitScreen)> onBeforePop = null,
            Action<(Screen enterScreen, Screen exitScreen)> onAfterPop = null)
        {
            OnBeforePush = onBeforePush;
            OnAfterPush = onAfterPush;
            OnBeforePop = onBeforePop;
            OnAfterPop = onAfterPop;
        }

        public void BeforePush(Screen enterScreen, Screen exitScreen)
        {
            OnBeforePush?.Invoke((enterScreen, exitScreen));
        }

        public void AfterPush(Screen enterScreen, Screen exitScreen)
        {
            OnAfterPush?.Invoke((enterScreen, exitScreen));
        }

        public void BeforePop(Screen enterScreen, Screen exitScreen)
        {
            OnBeforePop?.Invoke((enterScreen, exitScreen));
        }

        public void AfterPop(Screen enterScreen, Screen exitScreen)
        {
            OnAfterPop?.Invoke((enterScreen, exitScreen));
        }

        public event Action<(Screen enterScreen, Screen exitScreen)> OnAfterPop;
        public event Action<(Screen enterScreen, Screen exitScreen)> OnAfterPush;
        public event Action<(Screen enterScreen, Screen exitScreen)> OnBeforePop;
        public event Action<(Screen enterScreen, Screen exitScreen)> OnBeforePush;
    }
}