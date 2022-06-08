using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.ViewModels;
using UnityScreenNavigator.Runtime.Interactivity.Views;
using Object = UnityEngine.Object;

namespace UnityScreenNavigator.Runtime.Interactivity
{
    public class Tooltip
    {
        private readonly TooltipViewModel _tooltipViewModel;

        private readonly TooltipView _tooltipView;
        private readonly string _key;

        public TooltipView TooltipView => _tooltipView;

        public Tooltip(string key, TooltipViewModel tooltipViewModel, TooltipView tooltipView)
        {
            _key = key;
            _tooltipViewModel = tooltipViewModel;
            _tooltipView = tooltipView;
        }

        private async UniTask Show(TipPosition tipPosition, RectTransform target, int offset)
        {
            _tooltipViewModel.ViewGroup.AddView(_tooltipView);

            this.SetPosition(tipPosition, target, offset);
            await TooltipView.Show();
        }

        private void ForceClose()
        {
            if (_tooltipView == null || _tooltipView.Owner == null)
            {
                return;
            }

            if (!_tooltipView.Visibility)
            {
                Object.Destroy(_tooltipView.Owner);
                return;
            }

            Object.Destroy(_tooltipView);
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

        public static async UniTask<Tooltip> Show(string key, IUIView contentView, string message,
            TipPosition tipPosition,
            RectTransform target, int offset, Action afterHideCallback = null, bool closeOnCancelClick = true)
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

            var tipAsset = await AddressablesManager.LoadAssetAsync<GameObject>(key);
            if (tipAsset.Value == null)
            {
                throw new Exception($"Can't find tooltip asset with key: {key}");
            }

            var content = Object.Instantiate(tipAsset.Value);
            var view = content.GetComponent<TooltipView>();
            view.ContentView = contentView;

            var viewGroup = GetCurrentViewGroup();
            var viewModel = new TooltipViewModel(default);
            viewModel.Message = message;
            viewModel.Click = afterHideCallback;
            viewModel.ViewGroup = viewGroup;
            viewModel.CloseOnCancelClick = closeOnCancelClick;
            viewModel.TooltipKey = key;

            view.ViewModel = viewModel;

            var tip = new Tooltip(key, viewModel, view);
            await tip.Show(tipPosition, target, offset);
            _tooltips.Add(view, tip);
            return tip;
        }

        public static async UniTask<Tooltip> Show(string key, string message, TipPosition tipPosition,
            RectTransform target, int offset)
        {
            return await Show(key, null, message, tipPosition, target, offset);
        }

        public static async UniTask<Tooltip> Show(string key, IUIView content, TipPosition tipPosition,
            RectTransform target, int offset)
        {
            return await Show(key, content, null, tipPosition, target, offset);
        }
        
        public static async UniTask<Tooltip> Show(string key, string message, TipPosition tipPosition, RectTransform target, int offset, bool closeOnCancelClick = true)
        {
            return await Show(key, null, message, tipPosition, target, offset, null, closeOnCancelClick);
        }
        public static async UniTask<Tooltip> Show(string key, string message, TipPosition tipPosition, RectTransform target, int offset, bool closeOnCancelClick = true, Action afterHideCallback = null)
        {
            return await Show(key, null, message, tipPosition, target, offset, afterHideCallback, closeOnCancelClick);
        }

        #endregion
    }
}