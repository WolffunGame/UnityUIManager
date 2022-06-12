using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.Views;
using Object = UnityEngine.Object;

namespace UnityScreenNavigator.Runtime.Interactivity
{
    public class Tooltip
    {
        public AsyncReactiveProperty<string> Message { get; }
        public TipPosition Position { get; }
        public Action AfterHideCallBack { get; }
        public AsyncReactiveProperty<IUIViewGroup> ViewGroup { get; }
        public AsyncReactiveProperty<IUIView> View { get; }
        
        public AsyncReactiveProperty<bool> CloseOnCancelClick { get; }

        public Tooltip(string message, TipPosition tipPosition, IUIViewGroup viewGroup, TooltipView tooltipView,
            Action afterHideCallBack, bool closeOnCancelClick = false)
        {
            Message = new AsyncReactiveProperty<string>(message);
            Position = tipPosition;
            ViewGroup = new AsyncReactiveProperty<IUIViewGroup>(viewGroup);
            View = new AsyncReactiveProperty<IUIView>(tooltipView);
            CloseOnCancelClick = new AsyncReactiveProperty<bool>(closeOnCancelClick);
        }

        public Tooltip(TipPosition tipPosition, IUIViewGroup viewGroup, TooltipView tooltipView,
            Action afterHideCallBack, bool closeOnCancelClick = false)
        {
            Position = tipPosition;
            ViewGroup = new AsyncReactiveProperty<IUIViewGroup>(viewGroup);
            View = new AsyncReactiveProperty<IUIView>(tooltipView);
            CloseOnCancelClick = new AsyncReactiveProperty<bool>(closeOnCancelClick);
            AfterHideCallBack = afterHideCallBack;
        }

        private void Setup(TipPosition tipPosition, RectTransform target, int offset)
        {
            ViewGroup.Value.AddView(View.Value);
            View.Value.SetPosition(tipPosition, target, offset);
        }

        #region Static

        private const string DEFAULT_VIEW_NAME = "Prefabs/DefaultTooltip";

        private static string _viewName;

        public static string ViewName
        {
            get => string.IsNullOrEmpty(_viewName) ? DEFAULT_VIEW_NAME : _viewName;
            set => _viewName = value;
        }

        private static readonly Dictionary<TooltipView, Tooltip> _tooltips = new Dictionary<TooltipView, Tooltip>();

        public static void Remove(TooltipView tooltipView)
        {
            if (_tooltips.ContainsKey(tooltipView))
                _tooltips.Remove(tooltipView);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IUIViewGroup GetCurrentViewGroup()
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
        /// <param name="afterHideCallback"></param>
        /// <param name="closeOnCancelClick"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async UniTask<Tooltip> Show(string key, string message,
            TipPosition tipPosition,
            RectTransform target, int offset, Action afterHideCallback, bool closeOnCancelClick = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = ViewName;
            }

            //Close all tooltips are open
            foreach (var tooltip in _tooltips)
            {
                Object.Destroy(tooltip.Key.gameObject);
            }

            _tooltips.Clear();
            //load tooltip view
            var tipAsset = await AddressablesManager.LoadAssetAsync<GameObject>(key);
            if (tipAsset.Value == null)
            {
                throw new Exception($"Can't find tooltip asset with key: {key}");
            }

            var content = Object.Instantiate(tipAsset.Value);
            var view = content.GetComponent<TooltipView>();

            var viewGroup = GetCurrentViewGroup();

            var tip = new Tooltip(message, tipPosition, viewGroup, view, afterHideCallback, closeOnCancelClick);

            tip.Setup(tipPosition, target, offset);
            view.Tooltip = tip;

            await view.Show();
            _tooltips.Add(view, tip);
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
        /// <param name="afterHideCallback"></param>
        /// <param name="closeOnCancelClick"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async UniTask<Tooltip> Show(string key, IUIView contentView,
            TipPosition tipPosition,
            RectTransform target, int offset, Action afterHideCallback, bool closeOnCancelClick = true)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = ViewName;
            }

            //Close all tooltips are open
            foreach (var tooltip in _tooltips)
            {
                Object.Destroy(tooltip.Key.gameObject);
            }

            _tooltips.Clear();
            //load tooltip view
            var tipAsset = await AddressablesManager.LoadAssetAsync<GameObject>(key);
            if (tipAsset.Value == null)
            {
                throw new Exception($"Can't find tooltip asset with key: {key}");
            }

            var content = Object.Instantiate(tipAsset.Value);
            var view = content.GetComponent<TooltipView>();
            //set tooltip view if any
            view.ContentView = contentView;

            var viewGroup = GetCurrentViewGroup();

            var tip = new Tooltip(tipPosition, viewGroup, view, afterHideCallback, closeOnCancelClick);

            tip.Setup(tipPosition, target, offset);
            view.Tooltip = tip;

            await view.Show();
            _tooltips.Add(view, tip);
            return tip;
        }

        public static async UniTask<Tooltip> Show(string key, string message, TipPosition tipPosition,
            RectTransform target, int offset)
        {
            return await Show(key, message, tipPosition, target, offset, null);
        }

        public static async UniTask<Tooltip> Show(string key, IUIView content, TipPosition tipPosition,
            RectTransform target, int offset)
        {
            return await Show(key, content, tipPosition, target, offset, null);
        }

        public static async UniTask<Tooltip> Show(string key, string message, TipPosition tipPosition,
            RectTransform target, int offset, bool closeOnCancelClick)
        {
            return await Show(key, message, tipPosition, target, offset, null, closeOnCancelClick);
        }

        #endregion
    }
}