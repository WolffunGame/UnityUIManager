using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Sheet;
#if USN_USE_ASYNC_METHODS
using Cysharp.Threading.Tasks;
#endif

namespace Demo.Scripts
{
    public class CharacterModal : Modal
    {
        private const int ImageCount = 3;
        [SerializeField] private SheetContainer _imageContainer;
        [SerializeField] private CharacterModalThumbnailGrid thumbnailGrid;
        [SerializeField] private Button _expandButton;

        private readonly (int sheetId, CharacterModalImageSheet sheet)[] _imageSheets =
            new (int sheetId, CharacterModalImageSheet sheet)[ImageCount];

        private int _characterId;
        private int _selectedRank;

        public RectTransform CharacterImageRectTransform => (RectTransform) _imageContainer.transform;

        public void Setup(int characterId)
        {
            _characterId = characterId;
        }

        public override async UniTask Initialize()
        {
            var imageSheetHandles = new List<UniTask>();
            for (var i = 0; i < ImageCount; i++)
            {
                var index = i;
                var handle = _imageContainer.Register(ResourceKey.CharacterModalImageSheetPrefab(),
                    x => { _imageSheets[index] = (x.sheetId, (CharacterModalImageSheet) x.instance); });
                imageSheetHandles.Add(handle);
            }

            foreach (var handle in imageSheetHandles) await handle;

            _expandButton.onClick.AddListener(OnExpandButtonClicked);
        }


        public override async UniTask WillPushEnter()
        {
            for (var i = 0; i < ImageCount; i++) _imageSheets[i].sheet.Setup(_characterId, i + 1);

            await _imageContainer.Show(_imageSheets[0].sheetId, false);
            _selectedRank = 1;

            await thumbnailGrid.Setup(_characterId);
            thumbnailGrid.ThumbnailClicked += x =>
            {
                if (_imageContainer.IsInTransition) return;

                var targetSheet = _imageSheets[x];
                if (_imageContainer.ActiveSheet.Equals(targetSheet.sheet)) return;

                var sheetId = targetSheet.sheetId;
                _imageContainer.Show(sheetId, true);
                _selectedRank = x + 1;
            };
        }


        public override UniTask Cleanup()
        {
            _expandButton.onClick.RemoveListener(OnExpandButtonClicked);
            return UniTask.CompletedTask;
        }

        private void OnExpandButtonClicked()
        {
            var pushOption = new WindowOption(ResourceKey.CharacterImageModalPrefab(), true,
                onWindowCreated: modal =>
                {
                    var characterImageModal = (CharacterImageModal) modal;
                    characterImageModal.Setup(_characterId, _selectedRank);
                });
            ModalContainer.Find(ContainerKey.ModalContainerLayer)
                .Push(pushOption);
        }
    }
}