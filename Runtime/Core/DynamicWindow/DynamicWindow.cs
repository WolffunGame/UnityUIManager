using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation;
using UnityScreenNavigator.Runtime.Foundation.Animation;
using UnityScreenNavigator.Runtime.Foundation.PriorityCollection;

namespace UnityScreenNavigator.Runtime.Core.DynamicWindow
{
    [DisallowMultipleComponent]
    public class DynamicWindow : Window, IDynamicWindowLifeCycleEvent
    {
        [SerializeField]
        private string _identifier;

        public override string Identifier
        {
            get
            {
                if (string.IsNullOrEmpty(_identifier))
                {
                    _identifier = gameObject.name;
                }

                return _identifier;
            }
            set
            {
                _identifier = value;
            }
        }

        private readonly PriorityList<IDynamicWindowLifeCycleEvent> _lifecycleEvents =
            new PriorityList<IDynamicWindowLifeCycleEvent>();

        [SerializeField]
        private ModalTransitionAnimationContainer _animationContainer = new ModalTransitionAnimationContainer();

        public ModalTransitionAnimationContainer AnimationContainer => _animationContainer;
        private IDynamicWindowManager _dynamicWindowManager;
        public virtual IDynamicWindowManager DynamicWindowManager
        {
            get
            {
                return _dynamicWindowManager ??= gameObject.AddComponent<DynamicWindowManager>();
            }
            set { _dynamicWindowManager = value; }
        }

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
        public virtual UniTask WillShowEnter()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator WillShowEnter()
        {
            yield break;
        }
#endif

        public virtual void DidShowEnter()
        {
        }
#if USN_USE_ASYNC_METHODS
        public virtual UniTask WillShowExit()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator WillShowExit()
        {
            yield break;
        }
#endif

        public virtual void DidShowExit()
        {
        }
#if USN_USE_ASYNC_METHODS
        public virtual UniTask WillHideEnter()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator WillHideEnter()
        {
            yield break;
        }
#endif
        public virtual void DidHideEnter()
        {
        }
#if USN_USE_ASYNC_METHODS
        public virtual UniTask WillHideExit()
        {
            return UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator WillHideExit()
        {
            yield break;
        }
#endif
        public virtual void DidHideExit()
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

        internal UniTask AfterLoad(RectTransform parentTransform)
        {
            _lifecycleEvents.Add(this, 0);
            Parent = parentTransform;
            RectTransform.FillParent((RectTransform) Parent);
            Alpha = 0.0f;

            var tasks = _lifecycleEvents.Select(x => x.Initialize());
            return UniTask.WhenAll(tasks);
        }
        internal UniTask BeforeEnter(bool push, DynamicWindow partnerModal)
        {
            return BeforeEnterRoutine(push, partnerModal);
        }

        private UniTask BeforeEnterRoutine(bool push, DynamicWindow partnerModal)
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

            var tasks = push
                ? _lifecycleEvents.Select(x => x.WillShowEnter())
                : _lifecycleEvents.Select(x => x.WillHideEnter());
            return UniTask.WhenAll(tasks);
        }
        internal UniTask Enter(bool push, bool playAnimation, DynamicWindow partnerModal)
        {
            return EnterRoutine(push, playAnimation, partnerModal);
        }

        private UniTask EnterRoutine(bool push, bool playAnimation, DynamicWindow partnerModal)
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
            }
            return UniTask.CompletedTask;
        }
        internal void AfterEnter(bool show, DynamicWindow partnerModal)
        {
            if (show)
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidShowEnter();
                }
            }
            else
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidHideEnter();
                }
            }

            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                Interactable = true;
            }
        }
        internal UniTask BeforeExit(bool push, DynamicWindow partnerModal)
        {
            return BeforeExitRoutine(push, partnerModal);
        }
        private UniTask BeforeExitRoutine(bool push, DynamicWindow partnerModal)
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
                ? _lifecycleEvents.Select(x => x.WillShowExit())
                : _lifecycleEvents.Select(x => x.WillHideExit());
            return UniTask.WhenAll(tasks);
        }
        internal UniTask Exit(bool push, bool playAnimation, DynamicWindow partnerModal)
        {
            return ExitRoutine(push, playAnimation, partnerModal);
        }

        private UniTask ExitRoutine(bool push, bool playAnimation, DynamicWindow partnerModal)
        {
            if (!push)
            {
                if (playAnimation)
                {
                    var anim = _animationContainer.GetAnimation(false, partnerModal?.Identifier);
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

        internal void AfterExit(bool push, DynamicWindow partnerModal)
        {
            if (push)
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidShowExit();
                }
            }
            else
            {
                foreach (var lifecycleEvent in _lifecycleEvents)
                {
                    lifecycleEvent.DidHideExit();
                }
            }
        }

        internal UniTask BeforeRelease()
        {
            var tasks = _lifecycleEvents.Select(x => x.Cleanup());
            return UniTask.WhenAll(tasks);
        }

    }
}