using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Interactivity.Animation;
using UnityScreenNavigator.Runtime.Interactivity.ViewModels;

namespace UnityScreenNavigator.Runtime.Interactivity.Views
{
    public class TooltipView : UIElement
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Text messageText;
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
                if (_contentView == value)
                    return;

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

        private TooltipViewModel _viewModel;

        public TooltipViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                OnChangeViewModel();
            }
        }
        
        protected virtual async UniTask Close()
        {
            try
            {
                _viewModel.OnClick();
            }
            finally
            {
                //play Exit animation
                var exitAnim = transitionAnimationContainer.GetAnimation(false);
                if (exitAnim == null)
                {
                    gameObject.SetActive(false);
                    exitAnim = TooltipTransitionAnimationObject.CreateInstance();
                }

                exitAnim.Setup(RectTransform);
                await exitAnim.Play();

                Tooltip.Remove(this);
                Destroy(gameObject);
                AddressablesManager.ReleaseAsset(ViewModel.TooltipKey);
            }
        }

        public async UniTask Show()
        {
            var enterAnim = TransitionAnimationContainer.GetAnimation(true);
            if (enterAnim == null)
            {
                gameObject.SetActive(false);
                enterAnim = TooltipTransitionAnimationObject.CreateInstance();
            }

            enterAnim.Setup(RectTransform);
            await enterAnim.Play();
        }

        protected void OnChangeViewModel()
        {
            if (messageText != null)
            {
                if (!string.IsNullOrEmpty(_viewModel.Message))
                {
                    messageText.gameObject.SetActive(true);
                    messageText.text = _viewModel.Message;
                    if (_contentView != null && _contentView.Visibility)
                        _contentView.Visibility = false;
                }
                else
                    messageText.gameObject.SetActive(false);
            }
            
            if (closeButton != null && _viewModel.CloseOnCancelClick)
            {
                closeButton.gameObject.SetActive(true);
                closeButton.interactable = true;
                closeButton.onClick.AddListener(() => Close().Forget());
            }
        }

        protected override void Start()
        {
            base.Start();

            UniTaskAsyncEnumerable.EveryUpdate().ForEachAsync(_ =>
                {
                    if(Input.GetMouseButtonDown(0))
                    {
                        if (!_viewModel.CloseOnCancelClick)
                        {
                            Close().Forget();
                        }
                    }
                },
                gameObject.GetCancellationTokenOnDestroy());
            
        }

    }
}