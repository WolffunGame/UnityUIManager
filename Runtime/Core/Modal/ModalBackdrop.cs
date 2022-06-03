using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Foundation;
using UnityScreenNavigator.Runtime.Foundation.Animation;
namespace UnityScreenNavigator.Runtime.Core.Modal
{
    public sealed class ModalBackdrop : MonoBehaviour
    {
        [SerializeField] private ModalBackdropTransitionAnimationContainer _animationContainer;

        private CanvasGroup _canvasGroup;
        private RectTransform _parentTransform;
        private RectTransform _rectTransform;

        public ModalBackdropTransitionAnimationContainer AnimationContainer => _animationContainer;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }

        public void Setup(RectTransform parentTransform)
        {
            _parentTransform = parentTransform;
            _rectTransform.FillParent(_parentTransform);
            _canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }

        internal UniTask Enter(bool playAnimation)
        {
            return EnterRoutine(playAnimation);
        }

        private UniTask EnterRoutine(bool playAnimation)
        {
            gameObject.SetActive(true);
            _rectTransform.FillParent(_parentTransform);
            _canvasGroup.alpha = 1;

            if (playAnimation)
            {
                var anim = _animationContainer.GetAnimation(true);
                if (anim == null)
                {
                    anim = UnityScreenNavigatorSettings.Instance.ModalBackdropEnterAnimation;
                }

                anim.Setup(_rectTransform);
                return anim.CreatePlayRoutine();
            }

            _rectTransform.FillParent(_parentTransform);
            return UniTask.CompletedTask;
        }

        internal UniTask Exit(bool playAnimation)
        {
            return ExitRoutine(playAnimation);
        }

        private UniTask ExitRoutine(bool playAnimation)
        {
            gameObject.SetActive(true);
            _rectTransform.FillParent(_parentTransform);
            _canvasGroup.alpha = 1;

            if (playAnimation)
            {
                var anim = _animationContainer.GetAnimation(false);
                if (anim == null)
                {
                    anim = UnityScreenNavigatorSettings.Instance.ModalBackdropExitAnimation;
                }

                anim.Setup(_rectTransform);
                return anim.CreatePlayRoutine();
            }

            _canvasGroup.alpha = 0;
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
    }
}