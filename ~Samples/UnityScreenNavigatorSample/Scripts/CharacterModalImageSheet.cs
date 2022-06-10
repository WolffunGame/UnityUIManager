using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Sheet;
#if USN_USE_ASYNC_METHODS
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
#endif

namespace Demo.Scripts
{
    public class CharacterModalImageSheet : Sheet
    {
        [SerializeField] private Image _image;

        private int _characterId;
        private int _rank;

        public void Setup(int characterId, int rank)
        {
            _characterId = characterId;
            _rank = rank;
        }

        public override async UniTask WillEnter()
        {
            var handle =
                await AddressablesManager.LoadAssetAsync<Sprite>(ResourceKey.CharacterSprite(_characterId, _rank));
            _image.sprite = handle.Value;
        }
    }
}