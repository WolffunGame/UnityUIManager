using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.Animation;

namespace UnityScreenNavigator.Runtime.Interactivity.Views
{
    public class TooltipView : UIView
    {
        [SerializeField] private Button closeButton;
        #if USN_USE_TEXTMESHPRO
        [SerializeField] private TMPro.TextMeshProUGUI messageText;
        #else
        [SerializeField] private Text messageText;
        #endif

        [SerializeField] private GameObject content;

        [SerializeField] private InteractivityTransitionAnimationContainer transitionAnimationContainer =
            new InteractivityTransitionAnimationContainer();

        /// <summary>
        /// Manage animations for tooltip
        /// </summary>
        public InteractivityTransitionAnimationContainer TransitionAnimationContainer
        {
            get => transitionAnimationContainer;
        }


        private IUIView _contentView;

        /// <summary>
        /// Custom content view for the tooltip.
        /// </summary>
        public IUIView ContentView
        {
            get { return _contentView; }
            set
            {
                // if (_contentView == value)
                //     return;
                //
                if (_contentView != null)
                    Destroy(_contentView.Owner);

                _contentView = value;
                if (_contentView != null && _contentView.Owner != null && content != null)
                {
                    _contentView.Visibility = true;
                    _contentView.RectTransform.SetParent(content.transform, false);
                    if (messageText != null)
                        messageText.gameObject.SetActive(false);
                }
            }
        }

        private Tooltip _tooltip;

        public Tooltip Tooltip
        {
            get => _tooltip;
            set => _tooltip = value;
        }

        protected virtual async void Close()
        {
            //play Exit animation
            var exitAnim = transitionAnimationContainer.GetAnimation(false);
            if (exitAnim == null)
            {
                gameObject.SetActive(false);
                exitAnim = SwitchTransitionAnimationObject.CreateInstance();
            }

            exitAnim.Setup(RectTransform);
            await exitAnim.Play();

            Tooltip.AfterHide.Value = true;

            Tooltip.Remove(this);
            if (gameObject)
                Destroy(gameObject);
        }

        public async UniTask Show()
        {
            var enterAnim = TransitionAnimationContainer.GetAnimation(true);
            if (enterAnim == null)
            {
                gameObject.SetActive(false);
                enterAnim = SwitchTransitionAnimationObject.CreateInstance();
            }

            enterAnim.Setup(RectTransform);
            await enterAnim.Play();
        }

        protected override void Start()
        {
            base.Start();

            UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ =>
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!Tooltip.CloseOnCancelClick.Value)
                        {
                            Close();
                        }
                    }
                },
                gameObject.GetCancellationTokenOnDestroy());

            Tooltip.Message?.BindTo(messageText);
            
            Tooltip.CloseOnCancelClick.Subscribe(b => closeButton.gameObject.SetActive(b));
            closeButton.onClick.AddListener(Close);
        }
    }
}