using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Demo.Scripts
{
    public class CharacterModalThumbnailGrid : MonoBehaviour
    {
        [SerializeField] private CharacterModalThumbnail _firstThumb;
        [SerializeField] private CharacterModalThumbnail _secondThumb;
        [SerializeField] private CharacterModalThumbnail _thirdThumb;

        public event Action<int> ThumbnailClicked;

        public UniTask Setup(int characterId)
        {
            var task1 = _firstThumb.Setup(characterId, 1);
            _firstThumb.Clicked += () => ThumbnailClicked?.Invoke(0);
            var task2 = _secondThumb.Setup(characterId, 2);
            _secondThumb.Clicked += () => ThumbnailClicked?.Invoke(1);
            var task3 = _thirdThumb.Setup(characterId, 3);
            _thirdThumb.Clicked += () => ThumbnailClicked?.Invoke(2);
            return UniTask.WhenAll(task1, task2, task3);
        }
    }
}