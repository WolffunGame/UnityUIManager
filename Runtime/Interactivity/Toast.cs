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
    public class Toast
    {
        private const string DEFAULT_VIEW_NAME = "UI/Toast";

        private static string _toastKey;
        private readonly Action _callback;
        private readonly UILayout _layout;
        private readonly IUIViewGroup _viewGroup;
        
        private static List<Toast> _toasts = new List<Toast>();
        protected Toast(ToastView view, IUIViewGroup viewGroup, string message, float duration) : this(view, viewGroup,
            message, duration, null, null)
        {
        }

        protected Toast(ToastView view, IUIViewGroup viewGroup, string message, float duration, UILayout layout) : this(
            view, viewGroup, message, duration, layout, null)
        {
        }

        protected Toast(ToastView view, IUIViewGroup viewGroup, string message, float duration, UILayout layout,
            Action callback)
        {
            View = view;
            _viewGroup = viewGroup;
            Message = message;
            Duration = duration;
            _layout = layout;
            _callback = callback;
        }

        public static string ToastKey
        {
            get => string.IsNullOrEmpty(_toastKey) ? DEFAULT_VIEW_NAME : _toastKey;
            set => _toastKey = value;
        }

        public float Duration { get; }

        public string Message { get; }

        public ToastView View { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IUIViewGroup GetCurrentViewGroup()
        {
            var container = ContainerLayerManager.GetTopVisibilityLayer();
            return container.Current;
        }

        public static UniTask<Toast> Show(string text, float duration = 3f)
        {
            return Show(ToastKey, null, text, duration, null, null);
        }

        public static UniTask<Toast> Show(string text, float duration, UILayout layout)
        {
            return Show(ToastKey, null, text, duration, layout, null);
        }

        public static UniTask<Toast> Show(string text, float duration, UILayout layout, Action callback)
        {
            return Show(ToastKey, null, text, duration, layout, callback);
        }

        public static UniTask<Toast> Show(IUIViewGroup viewGroup, string text, float duration = 3f)
        {
            return Show(ToastKey, viewGroup, text, duration, null, null);
        }

        public static UniTask<Toast> Show(IUIViewGroup viewGroup, string text, float duration, UILayout layout)
        {
            return Show(ToastKey, viewGroup, text, duration, layout, null);
        }

        public static UniTask<Toast> Show(IUIViewGroup viewGroup, string text, float duration, UILayout layout,
            Action callback)
        {
            return Show(ToastKey, viewGroup, text, duration, layout, callback);
        }

        public static async UniTask<Toast> Show(string viewName, IUIViewGroup viewGroup, string text, float duration,
            UILayout layout,
            Action callback)
        {
            //Cancel all existing toasts
            foreach (var t in _toasts)
            {
                t.View.Visibility = false;
                t.Cancel().Forget();
            }
            if (string.IsNullOrEmpty(viewName))
                viewName = ToastKey;
            
            var contentGo = await AddressablesManager.LoadAssetAsync<GameObject>(ToastKey);
            if (contentGo.Value == null)
                throw new Exception($"Toast view is not found. viewName: {viewName}");
            var viewGo = Object.Instantiate(contentGo.Value);
            var view = viewGo.GetComponent<ToastView>();
            if (viewGroup == null)
                viewGroup = GetCurrentViewGroup();

            var toast = new Toast(view, viewGroup, text, duration, layout, callback);
            _toasts.Add(toast);
            await toast.Show();
            return toast;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public async UniTask Cancel()
        {
            if (View == null || View.Owner == null)
                return;

            if (!View.Visibility)
            {
                Object.Destroy(View.Owner);
                return;
            }
            
            await View.PlayExitAnimation();
            Object.Destroy(View.Owner);
            DoCallback();
        }

        private async UniTask Show()
        {
            _viewGroup.AddView(View, _layout);
            View.SetMessage(Message);

            await View.PlayEnterAnimation();

            await DelayDismiss(Duration);
        }

        protected async UniTask DelayDismiss(float duration)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: View.GetCancellationTokenOnDestroy());
            await Cancel();
        }

        protected void DoCallback()
        {
            try
            {
                if (_callback != null)
                    _callback();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}