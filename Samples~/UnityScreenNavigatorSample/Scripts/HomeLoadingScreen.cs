using UnityScreenNavigator.Runtime.Core.Screen;
using UnityScreenNavigator.Runtime.Core.Shared;

namespace Demo.Scripts
{
    public class HomeLoadingScreen : Screen
    {
        public override void DidPushEnter()
        {
            var pushOption = new WindowOption(ResourceKey.HomePagePrefab(), true);
            // Transition to "Home".
            ScreenContainer.Of(transform).Push(pushOption);
        }
    }
}