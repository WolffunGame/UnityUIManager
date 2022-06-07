using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

namespace UnityScreenNavigator.Runtime.Core.Screen
{
    [DisallowMultipleComponent]
    public class Screen : Window, IScreenLifecycleEvent
    {
        [SerializeField] private bool _usePrefabNameAsIdentifier = true;

        [SerializeField] [EnabledIf(nameof(_usePrefabNameAsIdentifier), false)]
        private string _identifier;

        [SerializeField] private int _renderingOrder;

        [SerializeField]
        private ScreenTransitionAnimationContainer _animationContainer = new ScreenTransitionAnimationContainer();

        private readonly PriorityList<IScreenLifecycleEvent> _lifecycleEvents = new PriorityList<IScreenLifecycleEvent>();

        public override string Identifier
        {
            get => _identifier;
            set => _identifier = value;
        }

        public ScreenTransitionAnimationContainer AnimationContainer => _animationContainer;


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

        public void AddLifecycleEvent(IScreenLifecycleEvent lifecycleEvent, int priority = 0)
        {
            _lifecycleEvents.Add(lifecycleEvent, priority);
        }

        public void RemoveLifecycleEvent(IScreenLifecycleEvent lifecycleEvent)
        {
            _lifecycleEvents.Remove(lifecycleEvent);
        }

        internal UniTask AfterLoad(RectTransform rectTransform)
        {
            _lifecycleEvents.Add(this, 0);
            _identifier = _usePrefabNameAsIdentifier ? gameObject.name.Replace("(Clone)", string.Empty) : _identifier;
            Parent = rectTransform;
            RectTransform.FillParent((RectTransform)Parent);

            // Set order of rendering.
            var siblingIndex = 0;
            for (var i = 0; i < Parent.childCount; i++)
            {
                var child = Parent.GetChild(i);
                var childScreen = child.GetComponent<Screen>();
                siblingIndex = i;
                if (_renderingOrder >= childScreen._renderingOrder)
                {
                    continue;
                }

                break;
            }

            RectTransform.SetSiblingIndex(siblingIndex);
            
            Alpha = 0.0f;
            var tasks = _lifecycleEvents.Select(x =>  x.Initialize());
            return UniTask.WhenAll(tasks);
        }


        internal UniTask BeforeEnter(bool push, Screen partnerScreen)
        {
            return BeforeEnterRoutine(push, partnerScreen);
        }

        private UniTask BeforeEnterRoutine(bool push, Screen partnerScreen)
        {
            gameObject.SetActive(true);
            RectTransform.FillParent((RectTransform)Parent);
            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                Interactable = false;
            }
            
            Alpha = 0.0f;

            var tasks = push
                ? _lifecycleEvents.Select(x => x.WillPushEnter())
                : _lifecycleEvents.Select(x => x.WillPopEnter());
            return UniTask.WhenAll(tasks);
        }

        internal UniTask Enter(bool push, bool playAnimation, Screen partnerScreen)
        {
            return EnterTask(push, playAnimation, partnerScreen);
        }

        private UniTask EnterTask(bool push, bool playAnimation, Screen partnerScreen)
        {
            Alpha = 1.0f;

            if (playAnimation)
            {
                var anim = _animationContainer.GetAnimation(push, true, partnerScreen?.Identifier);
                if (anim == null)
                {
                    anim = UnityScreenNavigatorSettings.Instance.GetDefaultScreenTransitionAnimation(push, true);
                }

                anim.SetPartner(partnerScreen?.transform as RectTransform);
                anim.Setup(RectTransform);
                return anim.CreatePlayRoutine();
            }

            RectTransform.FillParent((RectTransform)Parent);
            return UniTask.CompletedTask;
        }

        internal void AfterEnter(bool push, Screen partnerScreen)
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

        internal UniTask BeforeExit(bool push, Screen partnerScreen)
        {
            return BeforeExitTask(push, partnerScreen);
        }

        private UniTask BeforeExitTask(bool push, Screen partnerScreen)
        {
            gameObject.SetActive(true);
            RectTransform.FillParent((RectTransform)Parent);
            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                Interactable = false;
            }
            
            Alpha = 1.0f;

            var routines = push
                ? _lifecycleEvents.Select(x => x.WillPushExit())
                : _lifecycleEvents.Select(x => x.WillPopExit());
            
            return UniTask.WhenAll(routines);
        }

        internal UniTask Exit(bool push, bool playAnimation, Screen partnerScreen)
        {
            return ExitRoutine(push, playAnimation, partnerScreen);
        }

        private async UniTask ExitRoutine(bool push, bool playAnimation, Screen partnerScreen)
        {
            if (playAnimation)
            {
                var anim = _animationContainer.GetAnimation(push, false, partnerScreen?.Identifier);
                if (anim == null)
                {
                    anim = UnityScreenNavigatorSettings.Instance.GetDefaultScreenTransitionAnimation(push, false);
                }

                anim.SetPartner(partnerScreen?.transform as RectTransform);
                anim.Setup(RectTransform);
                
                await anim.CreatePlayRoutine();
            }
            
            Alpha = 0.0f;
            //return UniTask.CompletedTask;
        }

        internal void AfterExit(bool push, Screen partnerScreen)
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

            gameObject.SetActive(false);
        }

        internal UniTask BeforeRelease()
        {
            var tasks = _lifecycleEvents.Select(x => x.Cleanup());
            return UniTask.WhenAll(tasks);
        }
    }
}