using System;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    public struct WindowOption
    {
        public WindowOption(string resourcePath, bool playAnimation, bool stack = true,
            Action<Window> onWindowCreated = null
            , bool loadAsync = true)
        {
            ResourcePath = resourcePath;
            LoadAsync = loadAsync;
            PlayAnimation = playAnimation;
            WindowCreated = onWindowCreated;
            Stack = stack;
        }

        public bool LoadAsync { get; }

        public bool PlayAnimation { get; }

        public Action<Window> WindowCreated { get; }

        public bool Stack { get; }

        public string ResourcePath { get; }
    }
}