using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.Animation;

namespace UnityScreenNavigator.Runtime.Interactivity.Views
{
    public class ToastView : UIView
    {
        #if USN_USE_TEXTMESHPRO
        [SerializeField] private TMPro.TextMeshProUGUI messageText;
        #else
        [SerializeField] private Text messageText;
        #endif
        [SerializeField] private InteractivityTransitionAnimationContainer transitionAnimationContainer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMessage(string message)
        {
            messageText.text = message;
        }

        public UniTask PlayEnterAnimation()
        {
            var enterAnimation = transitionAnimationContainer.GetAnimation(true);
            if (enterAnimation == null)
            {
                gameObject.SetActive(false);
                enterAnimation = SwitchTransitionAnimationObject.CreateInstance();
            }

            enterAnimation.Setup(RectTransform);
            return enterAnimation.Play();
        }
        
        public UniTask PlayExitAnimation()
        {
            var exitAnimation = transitionAnimationContainer.GetAnimation(false);
            if (exitAnimation == null)
            {
                gameObject.SetActive(false);
                exitAnimation = SwitchTransitionAnimationObject.CreateInstance();
            }

            exitAnimation.Setup(RectTransform);
            return exitAnimation.Play();
        }
        
    }
}