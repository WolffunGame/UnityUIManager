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

        private Vector3 _afterPosition;
        private Vector3 _beforePosition;
        private CanvasGroup _canvasGroup;

        private Sequence _sequence;

        public override float Duration => _duration;
        public override bool IsCompleted => _sequence.IsComplete();

        private void Awake()
        {
            _sequence = DOTween.Sequence();
        }

        public override void SetTime(float time)
        {
            //throw new System.NotImplementedException();
        }

        public override async UniTask Play()
        {
            Debug.Log("playing animation object " + name);
            await SetTime();
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

        public async UniTask SetTime()
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
            var scaleTweener = RectTransform.DOScale(_afterScale, _duration).SetDelay(_delay).SetEase(_easeType)
                .From(_beforeScale);
            var fadeTweener = _canvasGroup.DOFade(_afterAlpha, _duration).SetDelay(_delay).SetEase(_easeType)
                .From(_beforeAlpha);
             _ = _sequence.Join(anchorPosTweener);
             _ = _sequence.Join(scaleTweener);
             _ = _sequence.Join(fadeTweener);
            await _sequence.AwaitForComplete();
            Debug.Log("Complete animation object " + name);
        }

        public void SetParams(float? duration = null, Ease? easeType = null, SheetAlignment? beforeAlignment = null,
            Vector3? beforeScale = null, float? beforeAlpha = null, SheetAlignment? afterAlignment = null,
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
        }
    }
}