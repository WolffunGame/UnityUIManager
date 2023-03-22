using System.Threading;
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

        [SerializeField] private bool isDisableAutoClose;

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

        public Tooltip Tooltip { get; set; }

        public void QuickClose()
        {
            Close(this.GetCancellationTokenOnDestroy());
        }

        protected virtual async void Close(CancellationToken cancellationToken)
        {
            if(Tooltip.LockClose)
                return;
            
            await PlayExitAnim(cancellationToken);

            Tooltip.AfterHide.Value = true;

            Tooltip.Remove(this);
            
            if (gameObject)
                Destroy(gameObject);
        }

        private UniTask PlayExitAnim(CancellationToken cancellationToken) {
            var exitAnim = transitionAnimationContainer.GetAnimation(false);
            if (exitAnim == null)
            {
                gameObject.SetActive(false);
                exitAnim = SwitchTransitionAnimationObject.CreateInstance();
            }

            exitAnim.Setup(RectTransform);
            return exitAnim.Play(cancellationToken);
        }

        public async UniTask Show(CancellationToken cancellationToken)
        {
            var enterAnim = TransitionAnimationContainer.GetAnimation(true);
            if (enterAnim == null)
            {
                gameObject.SetActive(false);
                enterAnim = SwitchTransitionAnimationObject.CreateInstance();
            }

            enterAnim.Setup(RectTransform);

            await enterAnim.Play(cancellationToken);
        }

        protected override void Start()
        {
            base.Start();
            Tooltip.Message?.BindTo(messageText);
            
            Tooltip.CloseOnCancelClick?.Subscribe(b => closeButton.gameObject.SetActive(b));
            closeButton?.OnClickAsAsyncEnumerable().Subscribe(_ => Close(this.GetCancellationTokenOnDestroy()));

            if (!isDisableAutoClose)
            {
                UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ =>
                  {
                      if (Input.GetMouseButtonDown(0))
                      {
                          if (!Tooltip.CloseOnCancelClick.Value)
                          {
                              Close(this.GetCancellationTokenOnDestroy());
                          }
                      }
                  },
                  this.GetCancellationTokenOnDestroy());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Tooltip.Remove(this);
        }

        #region LAZY SET
        public TooltipView SetViewGroup(IUIViewGroup viewGroup) {
            
            if (viewGroup == null) 
                    throw new System.ArgumentNullException("Null View Group");

            try
            {                
                viewGroup.AddView(this);

            } catch (System.Exception e) {

                throw new System.Exception("UnityScreenNavigator: Error adding view");

            }

            return this;
        }


        
        #endregion
    }
}