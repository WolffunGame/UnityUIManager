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
        public AsyncReactiveProperty<string> Message { get; }
        public AsyncReactiveProperty<bool> AfterHide { get; set; }
        private IUIViewGroup ViewGroup { get; } 

        public AsyncReactiveProperty<bool> CloseOnCancelClick { get;}
        
        public bool LockClose { get; private set; }

        private Tooltip(string message, IUIViewGroup viewGroup, bool closeOnCancelClick = false)
        {
            Message = new AsyncReactiveProperty<string>(message);
            ViewGroup = viewGroup;
            CloseOnCancelClick = new AsyncReactiveProperty<bool>(closeOnCancelClick);
            AfterHide = new AsyncReactiveProperty<bool>(false);
        }

        private Tooltip(IUIViewGroup viewGroup, bool closeOnCancelClick = false)
        {
            ViewGroup = viewGroup;
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
        private static IUIViewGroup GetCurrentViewGroup()
        {
            var container = ContainerLayerManager.GetTopVisibilityLayer();
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
            TipPosition tipPosition, RectTransform target, int offset, bool closeOnCancelClick = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = ViewName;
            }

            //Close all tooltips are open
            foreach (var tooltip in Tooltips)
            {
                Object.Destroy(tooltip.Key.gameObject);
            }

            Tooltips.Clear();
            //load tooltip view
            var tipAsset = await _assetsKeyLoader.LoadAssetAsync(key);
            if (tipAsset == null)
            {
                throw new Exception($"Can't find tooltip asset with key: {key}");
            }

            var content = Object.Instantiate(tipAsset);
            var view = content.GetComponent<TooltipView>();

            var viewGroup = GetCurrentViewGroup();

            var tip = new Tooltip(message, viewGroup, closeOnCancelClick);
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
        public static async UniTask<Tooltip> Show(string key, IUIView contentView,
            TipPosition tipPosition,
            RectTransform target, int offset, bool closeOnCancelClick = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = ViewName;
            }

            //Close all tooltips are open
            foreach (var tooltip in Tooltips)
            {
                Object.Destroy(tooltip.Key.gameObject);
            }

            Tooltips.Clear();
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

            var viewGroup = GetCurrentViewGroup();

            var tip = new Tooltip(viewGroup, closeOnCancelClick);
            tip.ViewGroup.AddView(view);
            view.SetPosition(tipPosition, target, offset);
            
            view.Tooltip = tip;

            await view.Show(view.GetCancellationTokenOnDestroy());
            Tooltips.Add(view, tip);
            return tip;
        }

        public static async UniTask<Tooltip> Show(string key, string message,
            TipPosition tipPosition, RectTransform target, IUIViewGroup container ,int offset, bool closeOnCancelClick = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = ViewName;
            }

            //Close all tooltips are open
            foreach (var tooltip in Tooltips)
            {
                Object.Destroy(tooltip.Key.gameObject);
            }

            Tooltips.Clear();
            //load tooltip view
            var tipAsset = await _assetsKeyLoader.LoadAssetAsync(key);
            if (tipAsset == null)
            {
                throw new Exception($"Can't find tooltip asset with key: {key}");
            }

            var content = Object.Instantiate(tipAsset);
            var view = content.GetComponent<TooltipView>();

            var viewGroup = container;

            var tip = new Tooltip(message, viewGroup, closeOnCancelClick);
            viewGroup.AddView(view);
            view.SetPosition(tipPosition, target, offset);
            
            view.Tooltip = tip;

            await view.Show(view.GetCancellationTokenOnDestroy());
            Tooltips.Add(view, tip);
            return tip;
        }
        #endregion
    }
}