using System;
using System.Collections.Generic;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    /// <summary>
    /// Manages the dynamic window.
    /// </summary>
    public interface IDynamicWindowManager
    {
        DynamicWindow Current { get; }

        int Count { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator<DynamicWindow> Visibles();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        DynamicWindow Get(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        void Add(DynamicWindow window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        bool Remove(DynamicWindow window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        DynamicWindow RemoveAt(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        bool Contains(DynamicWindow window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        int IndexOf(DynamicWindow window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visible"></param>
        /// <returns></returns>
        List<DynamicWindow> Find(bool visible);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Find<T>() where T : DynamicWindow;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="windowName"></param>
        /// <returns></returns>
        T Find<T>(string windowName) where T : DynamicWindow;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> FindAll<T>() where T : DynamicWindow;

        /// <summary>
        /// 
        /// </summary>
        void Clear();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        AsyncProcessHandle Show(WindowOption option);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        AsyncProcessHandle Hide(string identifier, bool playAnimation);
        
        void HideAll(bool playAnimation);
    }

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