using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using UnityScreenNavigator.Runtime.Foundation;
using UnityScreenNavigator.Runtime.Foundation.Animation;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;
using UnityScreenNavigator.Runtime.Foundation.PriorityCollection;
#if USN_USE_ASYNC_METHODS
#endif

namespace UnityScreenNavigator.Runtime.Core.Page
{
    [DisallowMultipleComponent]
    public class Page : ContainerBase, IPageLifecycleEvent
    {
        [SerializeField] private bool _usePrefabNameAsIdentifier = true;

        [SerializeField] [EnabledIf(nameof(_usePrefabNameAsIdentifier), false)]
        private string _identifier;

        [SerializeField] private int _renderingOrder;

        [SerializeField]
        private PageTransitionAnimationContainer _animationContainer = new PageTransitionAnimationContainer();

        //private CanvasGroup _canvasGroup;
        //private RectTransform _parentTransform;
        //private RectTransform _rectTransform;

        private readonly PriorityList<IPageLifecycleEvent> _lifecycleEvents = new PriorityList<IPageLifecycleEvent>();

        public override string Identifier
        {
            get => _identifier;
            set => _identifier = value;
        }

        public PageTransitionAnimationContainer AnimationContainer => _animationContainer;

        // public bool Interactable
        // {
        //     get => _canvasGroup.interactable;
        //     set => _canvasGroup.interactable = value;
        // }

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

        public void AddLifecycleEvent(IPageLifecycleEvent lifecycleEvent, int priority = 0)
        {
            _lifecycleEvents.Add(lifecycleEvent, priority);
        }

        public void RemoveLifecycleEvent(IPageLifecycleEvent lifecycleEvent)
        {
            _lifecycleEvents.Remove(lifecycleEvent);
        }

        internal AsyncProcessHandle AfterLoad(RectTransform rectTransform)
        {
            //_canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            _lifecycleEvents.Add(this, 0);
            _identifier = _usePrefabNameAsIdentifier ? gameObject.name.Replace("(Clone)", string.Empty) : _identifier;
            //_parentTransform = parentTransform;
            Parent = rectTransform;
            RectTransform.FillParent((RectTransform)Parent);

            // Set order of rendering.
            var siblingIndex = 0;
            for (var i = 0; i < Parent.childCount; i++)
            {
                var child = Parent.GetChild(i);
                var childPage = child.GetComponent<Page>();
                siblingIndex = i;
                if (_renderingOrder >= childPage._renderingOrder)
                {
                    continue;
                }

                break;
            }

            RectTransform.SetSiblingIndex(siblingIndex);

            //_canvasGroup.alpha = 0.0f;
            Alpha = 0.0f;

            return CoroutineManager.Instance.Run(CreateCoroutine(_lifecycleEvents.Select(x => x.Initialize())));
        }


        internal AsyncProcessHandle BeforeEnter(bool push, Page partnerPage)
        {
            return CoroutineManager.Instance.Run(BeforeEnterRoutine(push, partnerPage));
        }

        private IEnumerator BeforeEnterRoutine(bool push, Page partnerPage)
        {
            gameObject.SetActive(true);
            RectTransform.FillParent((RectTransform)Parent);
            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                //_canvasGroup.interactable = false;
                Interactable = false;
            }

            //_canvasGroup.alpha = 0.0f;
            Alpha = 0.0f;

            var routines = push
                ? _lifecycleEvents.Select(x => x.WillPushEnter())
                : _lifecycleEvents.Select(x => x.WillPopEnter());
            var handle = CoroutineManager.Instance.Run(CreateCoroutine(routines));

            while (!handle.IsTerminated)
            {
                yield return null;
            }
        }

        internal AsyncProcessHandle Enter(bool push, bool playAnimation, Page partnerPage)
        {
            return CoroutineManager.Instance.Run(EnterRoutine(push, playAnimation, partnerPage));
        }

        private IEnumerator EnterRoutine(bool push, bool playAnimation, Page partnerPage)
        {
            //_canvasGroup.alpha = 1.0f;
            Alpha = 1.0f;

            if (playAnimation)
            {
                var anim = _animationContainer.GetAnimation(push, true, partnerPage?.Identifier);
                if (anim == null)
                {
                    anim = UnityScreenNavigatorSettings.Instance.GetDefaultPageTransitionAnimation(push, true);
                }

                anim.SetPartner(partnerPage?.transform as RectTransform);
                anim.Setup(RectTransform);
                yield return CoroutineManager.Instance.Run(anim.CreatePlayRoutine());
            }

            RectTransform.FillParent((RectTransform)Parent);
        }

        internal void AfterEnter(bool push, Page partnerPage)
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
                //_canvasGroup.interactable = true;
                Interactable = true;
            }
        }

        internal AsyncProcessHandle BeforeExit(bool push, Page partnerPage)
        {
            return CoroutineManager.Instance.Run(BeforeExitRoutine(push, partnerPage));
        }

        private IEnumerator BeforeExitRoutine(bool push, Page partnerPage)
        {
            gameObject.SetActive(true);
            RectTransform.FillParent((RectTransform)Parent);
            if (!UnityScreenNavigatorSettings.Instance.EnableInteractionInTransition)
            {
                //_canvasGroup.interactable = false;
                Interactable = false;
            }

            //_canvasGroup.alpha = 1.0f;
            Alpha = 1.0f;

            var routines = push
                ? _lifecycleEvents.Select(x => x.WillPushExit())
                : _lifecycleEvents.Select(x => x.WillPopExit());
            var handle = CoroutineManager.Instance.Run(CreateCoroutine(routines));

            while (!handle.IsTerminated)
            {
                yield return null;
            }
        }

        internal AsyncProcessHandle Exit(bool push, bool playAnimation, Page partnerPage)
        {
            return CoroutineManager.Instance.Run(ExitRoutine(push, playAnimation, partnerPage));
        }

        private IEnumerator ExitRoutine(bool push, bool playAnimation, Page partnerPage)
        {
            if (playAnimation)
            {
                var anim = _animationContainer.GetAnimation(push, false, partnerPage?.Identifier);
                if (anim == null)
                {
                    anim = UnityScreenNavigatorSettings.Instance.GetDefaultPageTransitionAnimation(push, false);
                }

                anim.SetPartner(partnerPage?.transform as RectTransform);
                anim.Setup(RectTransform);
                yield return CoroutineManager.Instance.Run(anim.CreatePlayRoutine());
            }

            //_canvasGroup.alpha = 0.0f;
            Alpha = 0.0f;
        }

        internal void AfterExit(bool push, Page partnerPage)
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

        internal AsyncProcessHandle BeforeRelease()
        {
            return CoroutineManager.Instance.Run(CreateCoroutine(_lifecycleEvents.Select(x => x.Cleanup())));
        }

#if USN_USE_ASYNC_METHODS
        private IEnumerator CreateCoroutine(IEnumerable<UniTask> targets)
#else
        private IEnumerator CreateCoroutine(IEnumerable<IEnumerator> targets)
#endif
        {
            foreach (var target in targets)
            {
                var handle = CoroutineManager.Instance.Run(CreateCoroutine(target));
                if (!handle.IsTerminated)
                {
                    yield return handle;
                }
            }
        }

#if USN_USE_ASYNC_METHODS
        private IEnumerator CreateCoroutine(UniTask target)
#else
        private IEnumerator CreateCoroutine(IEnumerator target)
#endif
        {
#if USN_USE_ASYNC_METHODS
            async void WaitTaskAndCallback(UniTask task, Action callback)
            {
                await task;
                callback?.Invoke();
            }
            
            var isCompleted = false;
            WaitTaskAndCallback(target, () =>
            {
                isCompleted = true;
            });
            return new WaitUntil(() => isCompleted);
#else
            return target;
#endif
        }
    }
}