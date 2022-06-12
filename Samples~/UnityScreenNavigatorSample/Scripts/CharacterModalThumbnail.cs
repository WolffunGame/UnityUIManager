using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Demo.Scripts
{
    public class CharacterModalThumbnail : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Button _button;

        private void Awake()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        public event Action Clicked;

        public async UniTask Setup(int id, int rank)
        {
            var sprite =
                await AddressablesManager.LoadAssetAsync<Sprite>(ResourceKey.CharacterThumbnailSprite(id, rank));
            _image.sprite = sprite.Value;
        }

        private void OnClicked()
        {
            Clicked?.Invoke();
        }
    }
}