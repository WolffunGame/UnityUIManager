using System;
using System.Collections.Generic;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    public interface IWindowManager 
    {
        IWindow Current { get; }

        int Count { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator<IWindow> Visibles();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IWindow Get(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        void Add(IWindow window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        bool Remove(IWindow window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IWindow RemoveAt(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        bool Contains(IWindow window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        int IndexOf(IWindow window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visible"></param>
        /// <returns></returns>
        List<IWindow> Find(bool visible);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Find<T>() where T : IWindow;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="windowName"></param>
        /// <returns></returns>
        T Find<T>(string windowName) where T : IWindow;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> FindAll<T>() where T : IWindow;

        /// <summary>
        /// 
        /// </summary>
        void Clear();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        AsyncProcessHandle Show(ShowWindowOption option);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playAnimation"></param>
        /// <returns></returns>
        AsyncProcessHandle Hide(bool playAnimation);
    }

    public struct PushWindowOption
    {
        private string resourcePath;
        private bool loadAsync;
        private bool playAnimation;
        private Action<ContainerBase> onWindowCreated;
        private bool stack;

        public PushWindowOption(string resourcePath, bool playAnimation, bool stack = true,
            Action<ContainerBase> onWindowCreated = null
            , bool loadAsync = true)
        {
            this.resourcePath = resourcePath;
            this.loadAsync = loadAsync;
            this.playAnimation = playAnimation;
            this.onWindowCreated = onWindowCreated;
            this.stack = stack;
        }

        public bool LoadAsync => loadAsync;
        public bool PlayAnimation => playAnimation;
        public Action<ContainerBase> OnWindowCreated => onWindowCreated;
        public bool Stack => stack;
        public string ResourcePath => resourcePath;
    }

    public struct ShowWindowOption
    {
        private string resourcePath;
        private bool loadAsync;
        private bool playAnimation;
        private Action<IWindow> onWindowCreated;
        
        public ShowWindowOption(string resourcePath, bool playAnimation,
            Action<IWindow> onWindowCreated = null, bool loadAsync = true)
        {
            this.resourcePath = resourcePath;
            this.loadAsync = loadAsync;
            this.playAnimation = playAnimation;
            this.onWindowCreated = onWindowCreated;
        }
        
        public bool LoadAsync => loadAsync;
        public bool PlayAnimation => playAnimation;
        public Action<IWindow> OnWindowCreated => onWindowCreated;
        public string ResourcePath => resourcePath;
    }
}