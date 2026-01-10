using System;
using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    public readonly struct WindowOption
    {
        public WindowOption(string resourcePath, bool playAnimation, int priority = 0, bool stack = true, bool IsPoolable = false)
        {
            ResourcePath = resourcePath;
            PlayAnimation = playAnimation;
            Stack = stack;
            WindowCreated = new AsyncReactiveProperty<Window>(default);
            Priority = priority;
            this.IsPoolable = IsPoolable;
        }
        
        public bool PlayAnimation { get; }
        
        public int Priority { get;}
        
        public AsyncReactiveProperty<Window> WindowCreated { get; }

        public bool Stack { get; }

        public bool IsPoolable { get; }
        
        public string ResourcePath { get; }
    }
}