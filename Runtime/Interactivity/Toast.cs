using System;
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

        private static string _viewName;
        private readonly Action _callback;
        private readonly UILayout _layout;

        private readonly IUIViewGroup _viewGroup;

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

        public static string ViewName
        {
            get => string.IsNullOrEmpty(_viewName) ? DEFAULT_VIEW_NAME : _viewName;
            set => _viewName = value;
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
            return Show(ViewName, null, text, duration, null, null);
        }

        public static UniTask<Toast> Show(string text, float duration, UILayout layout)
        {
            return Show(ViewName, null, text, duration, layout, null);
        }

        public static UniTask<Toast> Show(string text, float duration, UILayout layout, Action callback)
        {
            return Show(ViewName, null, text, duration, layout, callback);
        }

        public static UniTask<Toast> Show(IUIViewGroup viewGroup, string text, float duration = 3f)
        {
            return Show(ViewName, viewGroup, text, duration, null, null);
        }

        public static UniTask<Toast> Show(IUIViewGroup viewGroup, string text, float duration, UILayout layout)
        {
            return Show(ViewName, viewGroup, text, duration, layout, null);
        }

        public static UniTask<Toast> Show(IUIViewGroup viewGroup, string text, float duration, UILayout layout,
            Action callback)
        {
            return Show(ViewName, viewGroup, text, duration, layout, callback);
        }

        public static async UniTask<Toast> Show(string viewName, IUIViewGroup viewGroup, string text, float duration,
            UILayout layout,
            Action callback)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ViewName;

            var contentGo = await AddressablesManager.LoadAssetAsync<GameObject>(ViewName);
            if (contentGo.Value == null)
                throw new Exception($"Toast view is not found. viewName: {viewName}");
            var view = contentGo.Value.GetComponent<ToastView>();
            if (viewGroup == null)
                viewGroup = GetCurrentViewGroup();

            var toast = new Toast(view, viewGroup, text, duration, layout, callback);
            await toast.Show();
            return toast;
        }

        public async UniTask Cancel()
        {
            if (View == null || View.Owner == null)
                return;

            if (!View.Visibility)
            {
                Object.Destroy(View.Owner);
                return;
            }

            if (View.ExitAnimation != null)
            {
                await View.ExitAnimation.Play();
                View.Visibility = false;
                _viewGroup.RemoveView(View);
                Object.Destroy(View.Owner);
                DoCallback();
            }
            else
            {
                View.Visibility = false;
                _viewGroup.RemoveView(View);
                Object.Destroy(View.Owner);
                DoCallback();
            }
        }

        private async UniTask Show()
        {
            if (View.Visibility)
                return;

            _viewGroup.AddView(View, _layout);
            View.Visibility = true;
            View.SetMessage(Message);

            if (View.EnterAnimation != null)
                await View.EnterAnimation.Play();

            await DelayDismiss(Duration);
        }

        protected async UniTask DelayDismiss(float duration)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration),
                cancellationToken: View.GetCancellationTokenOnDestroy());
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
                Show("", 3, _layout => { });
            }
        }
    }
}