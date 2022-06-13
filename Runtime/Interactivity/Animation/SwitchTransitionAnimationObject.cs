using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared;

namespace UnityScreenNavigator.Runtime.Interactivity.Animation
{
    [CreateAssetMenu(menuName = "Screen Navigator/Tooltip Transition Animation")]
    public class SwitchTransitionAnimationObject : TransitionAnimationObject
    {
        [Header("Animation controls fade in/out and scale")] [SerializeField]
        private float _delay;

        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private Ease _easeType = Ease.OutQuart;
        [SerializeField] private Vector3 _beforeScale = Vector3.one;
        [SerializeField] private float _beforeAlpha = 1.0f;
        [SerializeField] private Vector3 _afterScale = Vector3.one;
        [SerializeField] private float _afterAlpha = 1.0f;

        private CanvasGroup _canvasGroup;

        private Sequence _sequence;
#if UI_ANIMATION_TIMELINE_SUPPORT
        public override float Duration => _duration;
        public override bool IsCompleted => _sequence.IsComplete();

        public override void SetTime(float time)
        {
            //throw new System.NotImplementedException();
        }
#endif
        public override async UniTask Play()
        {
            await SetTime();
        }

        public static SwitchTransitionAnimationObject CreateInstance(float? duration = null, Ease? easeType = null,
            Vector3? beforeScale = null, float? beforeAlpha = null,
            Vector3? afterScale = null, float? afterAlpha = null)
        {
            var anim = CreateInstance<SwitchTransitionAnimationObject>();
            anim.SetParams(duration, easeType, beforeScale, beforeAlpha, afterScale,
                afterAlpha);
            return anim;
        }

        public override void Setup()
        {
            if (!RectTransform.gameObject.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup = RectTransform.gameObject.AddComponent<CanvasGroup>();
            }

            _canvasGroup = canvasGroup;
        }

        public async UniTask SetTime()
        {
            _sequence = DOTween.Sequence();
            var cancellationToken = RectTransform.GetCancellationTokenOnDestroy();
            var scaleTweener = RectTransform.DOScale(_afterScale, _duration).SetDelay(_delay).SetEase(_easeType)
                .From(_beforeScale);
            var fadeTweener = _canvasGroup.DOFade(_afterAlpha, _duration).SetDelay(_delay).SetEase(_easeType)
                .From(_beforeAlpha);
            _ = _sequence.Join(scaleTweener);
            _ = _sequence.Join(fadeTweener);
            await _sequence.AwaitForComplete(cancellationToken: cancellationToken);
        }

        public void SetParams(float? duration = null, Ease? easeType = null,
            Vector3? beforeScale = null, float? beforeAlpha = null,
            Vector3? afterScale = null, float? afterAlpha = null)
        {
            if (duration.HasValue)
            {
                _duration = duration.Value;
            }

            if (easeType.HasValue)
            {
                _easeType = easeType.Value;
            }

            if (beforeScale.HasValue)
            {
                _beforeScale = beforeScale.Value;
            }

            if (beforeAlpha.HasValue)
            {
                _beforeAlpha = beforeAlpha.Value;
            }

            if (afterScale.HasValue)
            {
                _afterScale = afterScale.Value;
            }

            if (afterAlpha.HasValue)
            {
                _afterAlpha = afterAlpha.Value;
            }
        }
    }
}