using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Interactivity.Views
{
    public class ToastView : UIElement
    {
        [SerializeField]
        private Text messageText;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMessage(string message)
        {
            messageText.text = message;
        }
    }
}