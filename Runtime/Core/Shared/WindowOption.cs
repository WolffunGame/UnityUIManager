using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    public readonly struct WindowOption
    {
        public WindowOption(string resourcePath, bool playAnimation, int priority = 0, bool stack = true)
        {
            ResourcePath = resourcePath;
            PlayAnimation = playAnimation;
            Stack = stack;
            WindowCreated = new AsyncReactiveProperty<Window>(default);
            Priority = priority;
        }
        
        public bool PlayAnimation { get; }
        
        public int Priority { get;}
        
        public AsyncReactiveProperty<Window> WindowCreated { get; }

        public bool Stack { get; }

        public string ResourcePath { get; }
    }
}