using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AddressableAssets.Loaders;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Layers;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.Views;
using Object = UnityEngine.Object;

namespace UnityScreenNavigator.Runtime.Interactivity
{
    public readonly partial struct Toast
    {
        private const string DEFAULT_VIEW_NAME = "DefaultToast";

        private static string _toastKey;
        private readonly Action _callback;
        private readonly UILayout _layout;
        private readonly IUIViewGroup _viewGroup;
        
        private static readonly List<Toast> Toasts = new List<Toast>();
        private static readonly IAssetsKeyLoader<GameObject> AssetsKeyLoader = new AssetsKeyLoader<GameObject>();
        private Toast(ToastView view, IUIViewGroup viewGroup, string message, float duration) : this(view, viewGroup,
            message, duration, null, null)
        {
        }

        private Toast(ToastView view, IUIViewGroup viewGroup, string message, float duration, UILayout layout,
            Action callback = null)
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

        public static UniTask<Toast> Show(string text, float duration = 3f, CancellationToken cancellationToken = default)
        {
            return Show(ToastKey, null, text, duration, null, null, cancellationToken);
        }

        public static UniTask<Toast> Show(string text, float duration, UILayout layout, CancellationToken cancellationToken = default)
        {
            return Show(ToastKey, null, text, duration, layout, null, cancellationToken);
        }

        public static UniTask<Toast> Show(string text, float duration, UILayout layout, Action callback, CancellationToken cancellationToken = default)
        {
            return Show(ToastKey, null, text, duration, layout, callback, cancellationToken);
        }

        public static UniTask<Toast> Show(IUIViewGroup viewGroup, string text, float duration = 3f, CancellationToken cancellationToken = default)
        {
            return Show(ToastKey, viewGroup, text, duration, null, null, cancellationToken);
        }

        public static UniTask<Toast> Show(IUIViewGroup viewGroup, string text, float duration, UILayout layout, CancellationToken cancellationToken = default)
        {
            return Show(ToastKey, viewGroup, text, duration, layout, null, cancellationToken);
        }

        public static UniTask<Toast> Show(IUIViewGroup viewGroup, string text, float duration, UILayout layout,
            Action callback, CancellationToken cancellationToken = default)
        {
            return Show(ToastKey, viewGroup, text, duration, layout, callback, cancellationToken);
        }
        
        public static UniTask<Toast> Show(string key, IUIViewGroup viewGroup, string text, float duration = 3f, CancellationToken cancellationToken = default)
        {
            return Show(key, viewGroup, text, duration, null, null, cancellationToken);
        }
        
        public static UniTask<Toast> Show(string key, string text, float duration, CancellationToken cancellationToken = default)
        {
            return Show(key, null, text, duration, null, null, cancellationToken);
        }

        //Return the created Toast
        //TODO: add another awaiter for finish showing
        private static async UniTask<Toast> Show(string viewName, IUIViewGroup viewGroup, string text, float duration,
            UILayout layout, Action callback, CancellationToken cancellationToken = default)
        {
            //Cancel all existing toasts
            foreach (var t in Toasts)
            {
                t.View.Visibility = false;
                await t.Cancel(default);
            }
            if (string.IsNullOrEmpty(viewName))
                viewName = ToastKey;
            
            var contentGo = await AssetsKeyLoader.LoadAssetAsync(viewName);
            if (contentGo == null)
                throw new Exception($"Toast view is not found. viewName: {viewName}");
            var viewGo = Object.Instantiate(contentGo);
            var view = viewGo.GetComponent<ToastView>();
            if (viewGroup == null)
                viewGroup = GetCurrentViewGroup();

            var toast = new Toast(view, viewGroup, text, duration, layout, callback);
            Toasts.Add(toast);

            var cancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, view.GetCancellationTokenOnDestroy());

            cancelToken.Token.Register(() => toast.Cancel());

            await toast.Show(cancelToken.Token);

            return toast;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public async UniTask Cancel(CancellationToken cancellationToken = default)
        {
            if (View == null || View.Owner == null)
                return;

            if (!View.Visibility)
            {
                Object.Destroy(View);
                return;
            }
            
            await View.PlayExitAnimation(cancellationToken);
            Object.Destroy(View);
            DoCallback();
        }

        private async UniTask Show(CancellationToken cancellationToken)
        {
            _viewGroup.AddView(View, _layout);
            View.SetMessage(Message);

            await View.PlayEnterAnimation(cancellationToken);

            await DelayDismiss(Duration, cancellationToken);
        }

        private async UniTask DelayDismiss(float duration, CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            await Cancel(token);
        }

        private void DoCallback()
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