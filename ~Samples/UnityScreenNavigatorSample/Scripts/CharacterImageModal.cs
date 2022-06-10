using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Demo.Scripts
{
    public class CharacterImageModal : Modal
    {
        [SerializeField] private Image _image;

        private int _characterId;
        private int _rank;

        public RectTransform ImageTransform => (RectTransform) _image.transform;

        public void Setup(int characterId, int rank)
        {
            _characterId = characterId;
            _rank = rank;
        }

        public override async UniTask WillPushEnter()
        {
            var resourceKey = ResourceKey.CharacterSprite(_characterId, _rank);
            var handle = await AddressablesManager.LoadAssetAsync<Sprite>(resourceKey);
            var sprite = handle.Value;
            _image.sprite = sprite;
        }
    }
}