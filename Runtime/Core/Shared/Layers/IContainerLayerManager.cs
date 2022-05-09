using System.Collections.Generic;

namespace UnityScreenNavigator.Runtime.Core.Shared.Layers
{
    /// <summary>
    /// Manages layers of UI views.
    /// </summary>
    public interface IContainerLayerManager
    {
        IContainerLayer Current { get; }
        bool Activated { get; set; }

        int Count { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator<IContainerLayer> Visibles();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IContainerLayer Get(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        void Add(IContainerLayer layer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        bool Remove(IContainerLayer window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IContainerLayer RemoveAt(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        bool Contains(IContainerLayer window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        int IndexOf(IContainerLayer window);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="visible"></param>
        /// <returns></returns>
        List<IContainerLayer> Find(bool visible);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Find<T>() where T : IContainerLayer;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="layerName"></param>
        /// <returns></returns>
        T Find<T>(string layerName) where T : IContainerLayer;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> FindAll<T>() where T : IContainerLayer;

        /// <summary>
        /// 
        /// </summary>
        void Clear();
    }
}