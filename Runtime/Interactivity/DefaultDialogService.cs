using Cysharp.Threading.Tasks;

namespace UnityScreenNavigator.Runtime.Interactivity
{
    public static class DefaultDialogService
    {
        public static UniTask ShowDialog(string title, string message)
        {
            return ShowDialog(title, message, null, null, null, true);
        }

        public static UniTask<AlertDialog> ShowDialog(string title, string message, string buttonText)
        {
            return ShowDialog(title, message, buttonText, null, null, false);
        }

        public static UniTask<AlertDialog> ShowDialog(string title, string message, string confirmButtonText,
            string cancelButtonText)
        {
            return ShowDialog(title, message, confirmButtonText, cancelButtonText, null, false);
        }

        public static UniTask<AlertDialog> ShowDialog(string title, string message, string confirmButtonText,
            string cancelButtonText, string neutralButtonText)
        {
            return ShowDialog(title, message, confirmButtonText, cancelButtonText, neutralButtonText, false);
        }

        public static UniTask<AlertDialog> ShowDialog(string title, string message, string confirmButtonText,
            string cancelButtonText, string neutralButtonText, bool canceledOnTouchOutside)
        {
            return AlertDialog.ShowMessage(message, title, confirmButtonText, neutralButtonText, cancelButtonText,
                canceledOnTouchOutside);
        }
    }
}