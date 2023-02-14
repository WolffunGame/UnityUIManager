using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AddressableAssets.Loaders;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.Views;
using Object = UnityEngine.Object;

namespace UnityScreenNavigator.Runtime.Interactivity
{
    public class Tooltip
    {
        public AsyncReactiveProperty<string> Message { get; set;}
        public AsyncReactiveProperty<bool> AfterHide { get; set; }
        private IUIViewGroup ViewGroup { get; } 

        public AsyncReactiveProperty<bool> CloseOnCancelClick { get;}
        public AsyncReactiveProperty<bool> DoneSetUp { get; set;}
        public bool LockClose { get; private set; }

        #region LAZY SET 
        public Tooltip SetMessage(string message) {
            Message.Value = message;
            return this;
        }

        public Tooltip SetDoneSetUp() {
            DoneSetUp.Value = true;
            return this;
        }

        #endregion

        private Tooltip(string message, bool closeOnCancelClick = false)
        {
            Message = new AsyncReactiveProperty<string>(message);
            CloseOnCancelClick = new AsyncReactiveProperty<bool>(closeOnCancelClick);
            AfterHide = new AsyncReactiveProperty<bool>(false);
        }

        private Tooltip( bool closeOnCancelClick = false)
        {
            CloseOnCancelClick = new AsyncReactiveProperty<bool>(closeOnCancelClick);
            AfterHide = new AsyncReactiveProperty<bool>(false);
        }

        public void LockCloseForSeconds(float seconds)
        {
            LockClose = true;
            UniTask.Delay(TimeSpan.FromSeconds(seconds)).ContinueWith(() => LockClose = false);
        }

        #region Static

        private const string DefaultViewName = "DefaultTooltip";

        private static string _viewName;

        public static string ViewName
        {
            get => string.IsNullOrEmpty(_viewName) ? DefaultViewName : _viewName;
            set => _viewName = value;
        }

        private static readonly Dictionary<TooltipView, Tooltip> Tooltips = new Dictionary<TooltipView, Tooltip>();
        private static IAssetsKeyLoader<GameObject> _assetsKeyLoader = new AssetsKeyLoader<GameObject>();

        public static void Remove(TooltipView tooltipView)
        {
            if (Tooltips.ContainsKey(tooltipView))
                Tooltips.Remove(tooltipView);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IUIViewGroup GetCurrentViewGroup(IContainerLayer layer)
        {
            var container = layer;
            return container.Current;
        }

        /// <summary>
        /// Show tooltip with text only
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        /// <param name="tipPosition"></param>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <param name="closeOnCancelClick"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async UniTask<Tooltip> Show(string key, string message,
            TipPosition tipPosition,
            RectTransform target,
            int offset, 
            bool closeOnCancelClick = true
            )
        {
            if (string.IsNullOrEmpty(key))
            {
                key = ViewName;
            }

            CloseAllOpeningTooltips();

            //load tooltip view
            var tipAsset = await _assetsKeyLoader.LoadAssetAsync(key);
            if (tipAsset == null)
            {
                throw new Exception($"Can't find tooltip asset with key: {key}");
            }

            var content = Object.Instantiate(tipAsset);
            var view = content.GetComponent<TooltipView>();

            var viewGroup = GetCurrentViewGroup(ContainerLayerManager.GetTopVisibilityLayer());

            var tip = new Tooltip(message, closeOnCancelClick);
            viewGroup.AddView(view);
            view.SetPosition(tipPosition, target, offset);
            
            view.Tooltip = tip;

            await view.Show(view.GetCancellationTokenOnDestroy());
            Tooltips.Add(view, tip);
            return tip;
        }

        /// <summary>
        /// Show tooltip with custom view.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="contentView"></param>
        /// <param name="tipPosition"></param>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <param name="closeOnCancelClick"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async UniTask<Tooltip> Show(
            string key, IUIView contentView,
            TipPosition tipPosition,
            RectTransform target, int offset,
            bool closeOnCancelClick = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = ViewName;
            }

            CloseAllOpeningTooltips();

            //load tooltip view
            var tipAsset = await _assetsKeyLoader.LoadAssetAsync(key);
            if (tipAsset == null)
            {
                throw new Exception($"Can't find tooltip asset with key: {key}");
            }

            var content = Object.Instantiate(tipAsset);
            var view = content.GetComponent<TooltipView>();
            //set tooltip view if any
            view.ContentView = contentView;

            var viewGroup = GetCurrentViewGroup(ContainerLayerManager.GetTopVisibilityLayer());

            var tip = new Tooltip(closeOnCancelClick);

            view.SetViewGroup(viewGroup);

            view.SetPosition(tipPosition, target, offset);
            
            view.Tooltip = tip;

            await view.Show(view.GetCancellationTokenOnDestroy());
            Tooltips.Add(view, tip);
            return tip;
        }

        public static void CloseAllOpeningTooltips(){

            foreach (var tooltip in Tooltips)
            {
                Object.Destroy(tooltip.Key.gameObject);
            }

            Tooltips.Clear();
        }

        public static async UniTask<Tooltip> Show(string key, string message,
            TipPosition tipPosition, RectTransform target, IUIViewGroup container ,int offset, bool closeOnCancelClick = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = ViewName;
            }

            CloseAllOpeningTooltips();

            //load tooltip view
            var tipAsset = await _assetsKeyLoader.LoadAssetAsync(key);
            if (tipAsset == null)
            {
                throw new Exception($"Can't find tooltip asset with key: {key}");
            }

            var content = Object.Instantiate(tipAsset);

            var view = content.GetComponent<TooltipView>();

            var viewGroup = container;

            var tip = new Tooltip(message, closeOnCancelClick);

            view.SetViewGroup(viewGroup);

            view.SetPosition(tipPosition, target, offset);
            
            view.Tooltip = tip;

            await view.Show(view.GetCancellationTokenOnDestroy());

            Tooltips.Add(view, tip);

            return tip;
        }


        /// <summary>
        /// Create a (Tooltip, TooltipView) pair in the specified ViewGroup without setting position, message, or animation
        /// </summary>
        /// <param name="key">The Addressable key of the tooltip prefab</param>
        /// <param name="container">The ViewGroup the tooltip will be added to</param>
        /// <param name="closeOnCancelClick"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        
        public static async UniTask<(Tooltip, TooltipView)> LazyShow(string key, IUIViewGroup container , bool closeOnCancelClick = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = ViewName;
            }

            //load tooltip view
            var tipAsset = await _assetsKeyLoader.LoadAssetAsync(key);
            
            if (tipAsset == null)
            {
                throw new Exception($"Can't find tooltip asset with key: {key}");
            }

            var content = Object.Instantiate(tipAsset);
            
            var view = content.GetComponent<TooltipView>();

            var viewGroup = container;

            var tip = new Tooltip("", closeOnCancelClick);

            view.SetViewGroup(viewGroup);

            view.Tooltip = tip;

          //  await tip.DoneSetUp.WaitAsync();

         //   await view.Show(view.GetCancellationTokenOnDestroy());

            Tooltips.Add(view, tip);

            return (tip, view);
        }

        /// <summary>
        /// LazyShow but with input tooltip GameObject
        /// </summary>
        /// <param name="tooltipObj"></param>
        /// <param name="container"></param>
        /// <param name="closeOnCancelClick"></param>
        /// <returns></returns>
         public static async UniTask<(Tooltip, TooltipView)> LazyShow(GameObject tooltipObj, IUIViewGroup container , bool closeOnCancelClick = true)
        {            
            var view = tooltipObj.GetComponent<TooltipView>();

            var viewGroup = container;

            var tip = new Tooltip("", closeOnCancelClick);

            view.SetViewGroup(viewGroup);

            view.Tooltip = tip;

          //  await tip.DoneSetUp.WaitAsync();

         //   await view.Show(view.GetCancellationTokenOnDestroy());

            Tooltips.Add(view, tip);

            return (tip, view);
        }
        #endregion
    }
}