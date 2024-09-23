using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Screen;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    public static class Utils
    {
        // public static async UniTask ShowToastForLocStr(LocalizedString locStr, float duration = 3F, Action callback = null)
        // {
        //     if (locStr is null)
        //         return;
        //
        //     var str = await locStr.GetLocalizedStringAsync();
        //
        //     await Toast.Show(str, duration, null, callback);
        // }
        //
        // public static async UniTask<Toast> ShowToastForLocStrFormat<T>(
        //     LocalizedString locStr, 
        //     T formatParam, 
        //     float duration = 3F, 
        //     Action callback = null, 
        //     CancellationToken cancellationToken = default)
        // {
        //     var str = locStr is not null ? await locStr.GetLocalizedStringAsync() : string.Empty;
        //
        //     var toast = await Toast.Show(StringUtils.Format(str, formatParam), duration, null, callback, cancellationToken);
        //
        //     return toast;
        // }
    
        public static UniTask PopScreen(string container, bool playAnim = true)
        {
            var screenContainer = ScreenContainer.Find(container);
    
            UniTask res = UniTask.CompletedTask; 
            
            if (screenContainer.IsInTransition)
                res = UniTask.WaitUntil(() => !screenContainer.IsInTransition);
    
            return res.ContinueWith(() => screenContainer.Pop(playAnim));
    
        }
    
        public static UniTask PopModal(string containeStr, bool playAnim = true)
        {
            var container = ModalContainer.Find(containeStr);
    
            return container.Pop(playAnim);
    
        }
        public static UniTask<Modal.Modal> PushModal(string containeStr, string prefab, bool playAnim = true)
        {
            var container = ModalContainer.Find(containeStr);
    
            var modal = new WindowOption(prefab, playAnim);
    
            return container.Push(modal);
    
        }
    
        public static async UniTask<Window> PushModalTakeWindow(
            string containeStr,
            string prefab,
            bool playAnim = true)
        {
            var container = ModalContainer.Find(containeStr);
    
            var modal = new WindowOption(prefab, playAnim);
    
            container.Push(modal);
    
            if (modal.WindowCreated.Value != null)
            {
                return modal.WindowCreated.Value;
            }
    
            return await modal.WindowCreated.WaitAsync();
    
        }
        
        public static async UniTask<Window> PushModalTakeWindow(
            ModalContainer modalContainer,
            string prefab,
            bool playAnim = true)
        {
            var modal = new WindowOption(prefab, playAnim);
    
            modalContainer.Push(modal);
    
            if (modal.WindowCreated.Value != null)
            {
                return modal.WindowCreated.Value;
            }
    
            return await modal.WindowCreated.WaitAsync();
    
        }
    
    
        public static UniTask<Screen.Screen> PushScreen(string container, string prefab, bool playAnim = true, bool isPoolable = false)
        {
            var screenContainer = ScreenContainer.Find(container);
    
            var screen = new WindowOption(prefab, playAnim, IsPoolable: isPoolable);
    
            return screenContainer.Push(screen);
    
        }
        // public static async void ShowToastForLocStr(IUIViewGroup viewGroup, LocalizedString locStr, float duration = 3)
        // {
        //     if (locStr is null)
        //         return;
        //
        //     var str = await locStr.GetLocalizedStringAsync();
        //
        //     Toast.Show(viewGroup, str, duration);
        // }
        //
    }
    
}