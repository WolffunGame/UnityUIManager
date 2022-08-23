using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    public struct WindowOption
    {
        public WindowOption(string resourcePath, bool playAnimation, bool stack = true, bool loadAsync = true)
        {
            ResourcePath = resourcePath;
            LoadAsync = loadAsync;
            PlayAnimation = playAnimation;
            Stack = stack;
            WindowCreated = new AsyncReactiveProperty<Window>(default);
        }

        public bool LoadAsync { get; }

        public bool PlayAnimation { get; }
        

        public AsyncReactiveProperty<Window> WindowCreated { get; }

        public bool Stack { get; }

        public string ResourcePath { get; }
    }
}