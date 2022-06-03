using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation;
using UnityScreenNavigator.Runtime.Foundation.Animation;
using UnityScreenNavigator.Runtime.Foundation.PriorityCollection;
#if USN_USE_ASYNC_METHODS
using System;
using Cysharp.Threading.Tasks;
#endif

namespace UnityScreenNavigator.Runtime.Core.Modal
{
    [DisallowMultipleComponent]
    public class Modal : Window, IModalLifecycleEvent
    {
        [SerializeField] private bool _usePrefabNameAsIdentifier = true;

        [SerializeField] [EnabledIf(nameof(_usePrefabNameAsIdentifier), false)]
        private string _identifier;

        [SerializeField]
        private ModalTransitionAnimationContainer _animationContainer = new ModalTransitionAnimationContainer();

        private readonly PriorityList<IModalLifecycleEvent> _lifecycleEvents = new PriorityList<IModalLifecycleEvent>();

        public override string Identifier
        {
            get => _identifier;
            set => _identifier = value;
        }
        
        public ModalTransitionAnimationContainer AnimationContainer => _animationContainer;


#if USN_USE_ASYNC_METHODS
        public virtual UniTask Initialize()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator Initialize()
        {
            yield break;
        }
#endif

#if USN_USE_ASYNC_METHODS
        public virtual UniTask WillPushEnter()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator WillPushEnter()
        {
            yield break;
        }
#endif

        public virtual void DidPushEnter()
        {
        }

#if USN_USE_ASYNC_METHODS
        public virtual UniTask WillPushExit()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator WillPushExit()
        {
            yield break;
        }
#endif

        public virtual void DidPushExit()
        {
        }

#if USN_USE_ASYNC_METHODS
        public virtual UniTask WillPopEnter()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator WillPopEnter()
        {
            yield break;
        }
#endif

        public virtual void DidPopEnter()
        {
        }

#if USN_USE_ASYNC_METHODS
        public virtual UniTask WillPopExit()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator WillPopExit()
        {
            yield break;
        }
#endif

        public virtual void DidPopExit()
        {
        }

#if USN_USE_ASYNC_METHODS
        public virtual UniTask Cleanup()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator Cleanup()
        {
            yield break;
        }
#endif

        public void AddLifecycleEvent(IModalLifecycleEvent lifecycleEvent, int priority = 0)
        {
            _lifecycleEvents.Add(lifecycleEvent, priority);
        }

        public void RemoveLifecycleEvent(IModalLifecycleEvent lifecycleEvent)
        {
            _lifecycleEvents.Remove(lifecycleEvent);
        }

        internal UniTask AfterLoad(RectTransform parentTransform)
        {
            _lifecycleEvents.Add(this, 0);
            _identifier = _usePrefabNameAsIdentifier ? gameObject.name.Replace("(Clone)", string.Empty) : _identifier;
            Parent = parentTransform;
            RectTransform.FillParent((RectTransform)Parent);

            Alpha = 0.0f;

            var tasks = _lifecycleEvents.Select(x => x.Initialize());
            return UniTask.WhenAll(tasks);
        }

        internal UniTask BeforeEnter(bool push, Modal partnerModal)
        {
            return BeforeEnterTask(push, partnerModal);
        }

        private UniTask BeforeEnterTask(bool push, Modal partnerModal)
        {
            if (push)
            {
                gameObject.SetActive(true);
                RectTransform.FillParent((RectTransform)Parent);

                Alpha = 0.0f;
            }

            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                Interactable = false;
            }

            var routines = push
                ? _lifecycleEvents.Select(x => x.WillPushEnter())
                : _lifecycleEvents.Select(x => x.WillPopEnter());
            return UniTask.WhenAll(routines);
        }

        internal UniTask Enter(bool push, bool playAnimation, Modal partnerModal)
        {
            return EnterTask(push, playAnimation, partnerModal);
        }

        private UniTask EnterTask(bool push, bool playAnimation, Modal partnerModal)
        {
            if (push)
            {
                Alpha = 1.0f;

                if (playAnimation)
                {
                    var anim = _animationContainer.GetAnimation(true, partnerModal?.Identifier);
                    if (anim == null)
                    {
                        anim = UnityScreenNavigatorSettings.Instance.GetDefaultModalTransitionAnimation(true);
                    }

                    anim.SetPartner(partnerModal?.transform as RectTransform);
                    anim.Setup(RectTransform);
                    return anim.CreatePlayRoutine();
                }

                RectTransform.FillParent((RectTransform)Parent);
                return UniTask.CompletedTask;
            }
            return UniTask.CompletedTask;
        }

        internal void AfterEnter(bool push, Modal partnerModal)
        {
            if (push)
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidPushEnter();
                }
            }
            else
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidPopEnter();
                }
            }

            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                Interactable = true;
            }
        }

        internal UniTask BeforeExit(bool push, Modal partnerModal)
        {
            return BeforeExitTask(push, partnerModal);
        }

        private UniTask BeforeExitTask(bool push, Modal partnerModal)
        {
            if (!push)
            {
                gameObject.SetActive(true);
                RectTransform.FillParent((RectTransform)Parent);
                Alpha = 1.0f;
            }

            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                Interactable = false;
            }

            var tasks = push
                ? _lifecycleEvents.Select(x => x.WillPushExit())
                : _lifecycleEvents.Select(x => x.WillPopExit());
            return UniTask.WhenAll(tasks);
        }

        internal UniTask Exit(bool push, bool playAnimation, Modal partnerModal)
        {
            return ExitTask(push, playAnimation, partnerModal);
        }

        private UniTask ExitTask(bool push, bool playAnimation, Modal partnerModal)
        {
            if (!push)
            {
                if (playAnimation)
                {
                    var anim = _animationContainer.GetAnimation(false, partnerModal?._identifier);
                    if (anim == null)
                    {
                        anim = UnityScreenNavigatorSettings.Instance.GetDefaultModalTransitionAnimation(false);
                    }

                    anim.SetPartner(partnerModal?.transform as RectTransform);
                    anim.Setup(RectTransform);
                    return anim.CreatePlayRoutine();
                }
                
                Alpha = 0.0f;
            }
            return UniTask.CompletedTask;
        }

        internal void AfterExit(bool push, Modal partnerModal)
        {
            if (push)
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidPushExit();
                }
            }
            else
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidPopExit();
                }
            }
        }

        internal UniTask BeforeRelease()
        {
            var tasks= _lifecycleEvents.Select(x => x.Cleanup());
            return UniTask.WhenAll(tasks);
        }
    }
}