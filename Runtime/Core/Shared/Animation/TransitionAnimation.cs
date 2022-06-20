using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityScreenNavigator.Runtime.Foundation;

namespace UnityScreenNavigator.Runtime.Core.Shared.Animation
{
     [Serializable]
        public sealed class TransitionAnimation
        {
            [FormerlySerializedAs("_partnerScreenIdentifierRegex")] [FormerlySerializedAs("_partnerPageIdentifierRegex")] [SerializeField] private string partnerIdentifierRegex;

            [SerializeField] private AnimationAssetType _assetType;

            [SerializeField] [EnabledIf(nameof(_assetType), (int)AnimationAssetType.MonoBehaviour)]
            private TransitionAnimationBehaviour _animationBehaviour;

            [SerializeField] [EnabledIf(nameof(_assetType), (int)AnimationAssetType.ScriptableObject)]
            private TransitionAnimationObject _animationObject;

            private Regex _partnerSheetIdentifierRegexCache;

            public string PartnerIdentifierRegex
            {
                get => partnerIdentifierRegex;
                set => partnerIdentifierRegex = value;
            }

            public AnimationAssetType AssetType
            {
                get => _assetType;
                set => _assetType = value;
            }

            public TransitionAnimationBehaviour AnimationBehaviour
            {
                get => _animationBehaviour;
                set => _animationBehaviour = value;
            }

            public TransitionAnimationObject AnimationObject
            {
                get => _animationObject;
                set => _animationObject = value;
            }

            public bool IsValid(string partnerScreenIdentifier)
            {
                if (GetAnimation() == null)
                {
                    return false;
                }
                
                // If the partner identifier is not registered, the animation is always valid.
                if (string.IsNullOrEmpty(partnerIdentifierRegex))
                {
                    return true;
                }
                
                if (string.IsNullOrEmpty(partnerScreenIdentifier))
                {
                    return false;
                }
                
                if (_partnerSheetIdentifierRegexCache == null)
                {
                    _partnerSheetIdentifierRegexCache = new Regex(partnerIdentifierRegex);
                }

                return _partnerSheetIdentifierRegexCache.IsMatch(partnerScreenIdentifier);
            }

            public ITransitionAnimation GetAnimation()
            {
                switch (_assetType)
                {
                    case AnimationAssetType.MonoBehaviour:
                        return _animationBehaviour;
                    case AnimationAssetType.ScriptableObject:
                        return UnityEngine.Object.Instantiate(_animationObject);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
}