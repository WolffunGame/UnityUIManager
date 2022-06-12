using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Foundation;
using UnityScreenNavigator.Runtime.Foundation.Animation;
using UnityScreenNavigator.Runtime.Foundation.PriorityCollection;
using Cysharp.Threading.Tasks;

namespace UnityScreenNavigator.Runtime.Core.Sheet
{
    [DisallowMultipleComponent]
    public class Sheet : MonoBehaviour, ISheetLifecycleEvent
    {
        [SerializeField] private string _identifier;

        [SerializeField] private int _renderingOrder;

        [SerializeField]
        private SheetTransitionAnimationContainer _animationContainer = new SheetTransitionAnimationContainer();

        private CanvasGroup _canvasGroup;
        private RectTransform _parentTransform;
        private RectTransform _rectTransform;

        private readonly PriorityList<ISheetLifecycleEvent> _lifecycleEvents = new PriorityList<ISheetLifecycleEvent>();

        public string Identifier
        {
            get => _identifier;
            set => _identifier = value;
        }

        public SheetTransitionAnimationContainer AnimationContainer => _animationContainer;

        public bool Interactable
        {
            get => _canvasGroup.interactable;
            set => _canvasGroup.interactable = value;
        }

        public virtual UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }


        public virtual UniTask WillEnter()
        {
            return UniTask.CompletedTask;
        }

        public virtual void DidEnter()
        {
        }

        public virtual UniTask WillExit()
        {
            return UniTask.CompletedTask;
        }

        public virtual void DidExit()
        {
        }

        public virtual UniTask Cleanup()
        {
            return UniTask.CompletedTask;
        }

        public void AddLifecycleEvent(ISheetLifecycleEvent lifecycleEvent, int priority = 0)
        {
            _lifecycleEvents.Add(lifecycleEvent, priority);
        }

        public void RemoveLifecycleEvent(ISheetLifecycleEvent lifecycleEvent)
        {
            _lifecycleEvents.Remove(lifecycleEvent);
        }

        internal UniTask AfterLoad(RectTransform parentTransform)
        {
            _rectTransform = (RectTransform)transform;
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            _lifecycleEvents.Add(this, 0);
            _parentTransform = parentTransform;
            _rectTransform.FillParent(_parentTransform);

            // Set order of rendering.
            var siblingIndex = 0;
            for (var i = 0; i < _parentTransform.childCount; i++)
            {
                var child = _parentTransform.GetChild(i);
                var childPage = child.GetComponent<Sheet>();
                siblingIndex = i;
                if (_renderingOrder >= childPage._renderingOrder)
                {
                    continue;
                }

                break;
            }

            _rectTransform.SetSiblingIndex(siblingIndex);

            gameObject.SetActive(false);

            var tasks = _lifecycleEvents.Select(x => x.Initialize());
            return UniTask.WhenAll(tasks);
        }

        internal UniTask BeforeEnter(Sheet partnerSheet)
        {
            return BeforeEnterTask(partnerSheet);
        }

        private UniTask BeforeEnterTask(Sheet partnerSheet)
        {
            gameObject.SetActive(true);
            _rectTransform.FillParent(_parentTransform);

            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                _canvasGroup.interactable = false;
            }

            _canvasGroup.alpha = 0.0f;

            var tasks = _lifecycleEvents.Select(x => x.WillEnter());
            return UniTask.WhenAll(tasks);
        }

        internal UniTask Enter(bool playAnimation, Sheet partnerSheet)
        {
            return EnterTask(playAnimation, partnerSheet);
        }

        private UniTask EnterTask(bool playAnimation, Sheet partnerSheet)
        {
            _canvasGroup.alpha = 1.0f;

            if (playAnimation)
            {
                var anim = _animationContainer.GetAnimation(true, partnerSheet?._identifier);
                if (anim == null)
                {
                    anim = UnityScreenNavigatorSettings.Instance.GetDefaultSheetTransitionAnimation(true);
                }

                anim.SetPartner(partnerSheet?.transform as RectTransform);
                anim.Setup(_rectTransform);
                return anim.CreatePlayRoutine();
            }

            _rectTransform.FillParent(_parentTransform);
            return UniTask.CompletedTask;
        }

        internal void AfterEnter(Sheet partnerSheet)
        {
            foreach (var lifecycleEvent in _lifecycleEvents)
            {
                lifecycleEvent.DidEnter();
            }

            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                _canvasGroup.interactable = true;
            }
        }

        internal UniTask BeforeExit(Sheet partnerSheet)
        {
            return BeforeExitTask(partnerSheet);
        }

        private UniTask BeforeExitTask(Sheet partnerSheet)
        {
            gameObject.SetActive(true);
            _rectTransform.FillParent(_parentTransform);
            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                _canvasGroup.interactable = false;
            }

            _canvasGroup.alpha = 1.0f;

            var tasks = _lifecycleEvents.Select(x => x.WillExit());
            return UniTask.WhenAll(tasks);
        }

        internal UniTask Exit(bool playAnimation, Sheet partnerSheet)
        {
            return ExitTask(playAnimation, partnerSheet);
        }

        private UniTask ExitTask(bool playAnimation, Sheet partnerSheet)
        {
            if (playAnimation)
            {
                var anim = _animationContainer.GetAnimation(false, partnerSheet?.Identifier);
                if (anim == null)
                {
                    anim = UnityScreenNavigatorSettings.Instance.GetDefaultSheetTransitionAnimation(false);
                }

                anim.SetPartner(partnerSheet?.transform as RectTransform);
                anim.Setup(_rectTransform);
                return anim.CreatePlayRoutine();
            }

            _canvasGroup.alpha = 0.0f;
            return UniTask.CompletedTask;
        }

        internal void AfterExit(Sheet partnerSheet)
        {
            foreach (var lifecycleEvent in _lifecycleEvents)
            {
                lifecycleEvent.DidExit();
            }

            gameObject.SetActive(false);
        }

        internal UniTask BeforeRelease()
        {
            var tasks = _lifecycleEvents.Select(x => x.Cleanup());
            return UniTask.WhenAll(tasks);
        }
    }
}