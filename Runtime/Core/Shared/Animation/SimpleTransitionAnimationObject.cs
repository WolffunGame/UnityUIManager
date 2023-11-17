using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    [CreateAssetMenu(menuName = "Screen Navigator/Simple Transition Animation")]
    public sealed class SimpleTransitionAnimationObject : TransitionAnimationObject
    {
        [SerializeField] private float _delay;
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private Ease _easeType = Ease.OutQuart;
        [SerializeField] private SheetAlignment _beforeAlignment = SheetAlignment.Center;
        [SerializeField] private Vector3 _beforeScale = Vector3.one;
        [SerializeField] private float _beforeAlpha = 1.0f;
        [SerializeField] private SheetAlignment _afterAlignment = SheetAlignment.Center;
        [SerializeField] private Vector3 _afterScale = Vector3.one;
        [SerializeField] private float _afterAlpha = 1.0f;
        [SerializeField] private bool _canvasGroupInteractable = true;
        [SerializeField] private bool _canvasGroupBlockRaycast = true;

        [Space(12)]
        [Header("Custom Scale Curve")]
        [Tooltip("Enable if you want to use Animation curve for scale instead of Ease Type above")]
        [SerializeField] private bool _isUsingScaleCurve = false;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.Linear(0f, 0f, 0.3f, 1f);

        private Vector3 _afterPosition;
        private Vector3 _beforePosition;
        private CanvasGroup _canvasGroup;

        private Sequence _sequence;
#if UI_ANIMATION_TIMELINE_SUPPORT
        public override float Duration => _duration;
        public override bool IsCompleted => _sequence.IsComplete();
#endif

        private void Awake()
        {
            _sequence = DOTween.Sequence();
        }
#if UI_ANIMATION_TIMELINE_SUPPORT
        public override void SetTime(float time)
        {
            //throw new System.NotImplementedException();
        }
#endif
        public override async UniTask Play(CancellationToken cancellationToken)
        {
            await SetTime(cancellationToken);
        }

        public static SimpleTransitionAnimationObject CreateInstance(float? duration = null, Ease? easeType = null,
            SheetAlignment? beforeAlignment = null, Vector3? beforeScale = null, float? beforeAlpha = null,
            SheetAlignment? afterAlignment = null, Vector3? afterScale = null, float? afterAlpha = null)
        {
            var anim = CreateInstance<SimpleTransitionAnimationObject>();
            anim.SetParams(duration, easeType, beforeAlignment, beforeScale, beforeAlpha, afterAlignment, afterScale,
                afterAlpha);
            return anim;
        }

        public override void Setup()
        {
            _beforePosition = _beforeAlignment.ToPosition(RectTransform);
            _afterPosition = _afterAlignment.ToPosition(RectTransform);
            if (!RectTransform.gameObject.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup = RectTransform.gameObject.AddComponent<CanvasGroup>();
            }

            _canvasGroup = canvasGroup;
        }

        public async UniTask SetTime(CancellationToken cancellationToken)
        {
            // time = Mathf.Max(0, time - _delay);
            // var progress = _duration <= 0.0f ? 1.0f : Mathf.Clamp01(time / _duration);
            // progress = Easings.Interpolate(progress, _easeType);
            // var position = Vector3.Lerp(_beforePosition, _afterPosition, progress);
            // var scale = Vector3.Lerp(_beforeScale, _afterScale, progress);
            // var alpha = Mathf.Lerp(_beforeAlpha, _afterAlpha, progress);
            // RectTransform.anchoredPosition = position;
            // RectTransform.localScale = scale;
            // _canvasGroup.alpha = alpha;

            var anchorPosTweener = RectTransform.DOAnchorPos(_afterPosition, _duration).SetDelay(_delay)
                .SetEase(_easeType).From(_beforePosition);

            var scaleTweener = RectTransform.DOScale(_afterScale, _duration).SetDelay(_delay);
            if (_isUsingScaleCurve)
                scaleTweener.SetEase(_scaleCurve).From(_beforeScale);
            else
                scaleTweener.SetEase(_easeType).From(_beforeScale);

            var fadeTweener = _canvasGroup.DOFade(_afterAlpha, _duration).SetDelay(_delay).SetEase(_easeType)
                .From(_beforeAlpha);

             _ = _sequence.Join(anchorPosTweener);
             _ = _sequence.Join(scaleTweener);
             _ = _sequence.Join(fadeTweener);
            await _sequence.AwaitForComplete(cancellationToken: cancellationToken);

            _canvasGroup.interactable = _canvasGroupInteractable;
            _canvasGroup.blocksRaycasts = _canvasGroupBlockRaycast;
        }

        public void SetParams(float? duration = null, Ease? easeType = null, SheetAlignment? beforeAlignment = null,
            Vector3? beforeScale = null, float? beforeAlpha = null, SheetAlignment? afterAlignment = null,
            Vector3? afterScale = null, float? afterAlpha = null,
            bool? canvasGroupInteractable = null, bool? canvasGroupBlockRaycast = null,
            AnimationCurve scaleCurve = null, bool? isUsingScaleCurve = null)
        {
            if (duration.HasValue)
            {
                _duration = duration.Value;
            }

            if (easeType.HasValue)
            {
                _easeType = easeType.Value;
            }

            if (beforeAlignment.HasValue)
            {
                _beforeAlignment = beforeAlignment.Value;
            }

            if (beforeScale.HasValue)
            {
                _beforeScale = beforeScale.Value;
            }

            if (beforeAlpha.HasValue)
            {
                _beforeAlpha = beforeAlpha.Value;
            }

            if (afterAlignment.HasValue)
            {
                _afterAlignment = afterAlignment.Value;
            }

            if (afterScale.HasValue)
            {
                _afterScale = afterScale.Value;
            }

            if (afterAlpha.HasValue)
            {
                _afterAlpha = afterAlpha.Value;
            }

            if (canvasGroupInteractable.HasValue)
            {
                _canvasGroupInteractable = canvasGroupInteractable.Value;
            }

            if (canvasGroupBlockRaycast.HasValue)
            {
                _canvasGroupBlockRaycast = canvasGroupBlockRaycast.Value;
            }

            if (isUsingScaleCurve.HasValue)
            {
                _isUsingScaleCurve = isUsingScaleCurve.Value;
            }

            if (scaleCurve != null)
            {
                _scaleCurve = scaleCurve;
            }
        }
    }
}