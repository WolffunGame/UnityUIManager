#if UI_ANIMATION_TIMELINE_SUPPORT
namespace UnityScreenNavigator.Runtime.Foundation.Animation
{
    internal interface IUpdatable
    {
        void Update(float deltaTime);
    }
}
#endif