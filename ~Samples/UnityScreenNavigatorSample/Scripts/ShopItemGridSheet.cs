using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Shared;
using UnityScreenNavigator.Runtime.Core.Shared.Animation;
using UnityScreenNavigator.Runtime.Core.Sheet;

namespace Demo.Scripts
{
    public class ShopItemGridSheet : Sheet
    {
        [SerializeField] private Image _thumbnailImage;
        [SerializeField] private Button _firstThumbButton;

        private int _characterId;

        public void Setup(int index, int characterId)
        {
            Identifier = $"{nameof(ShopItemGridSheet)}{index}";
            _characterId = characterId;
            SetupTransitionAnimations(index);
        }

        public override async UniTask Initialize()
        {
            var key = ResourceKey.CharacterThumbnailSprite(_characterId, 1);
            var operationResult = await AddressablesManager.LoadAssetAsync<Sprite>(key);
            _thumbnailImage.sprite = operationResult.Value;
            _firstThumbButton.onClick.AddListener(OnFirstThumbButtonClicked);
        }

        private void SetupTransitionAnimations(int index)
        {
            string beforeSheetIdentifierRegex;
            if (index == 0) beforeSheetIdentifierRegex = string.Empty;
            else if (index == 1) beforeSheetIdentifierRegex = $"{nameof(ShopItemGridSheet)}0";
            else beforeSheetIdentifierRegex = $"{nameof(ShopItemGridSheet)}[0-{index - 1}]";

            var afterSheetIdentifierRegex = $"{nameof(ShopItemGridSheet)}[{index + 1}-9]";
            var toLeftExitAnim = SimpleTransitionAnimationObject.CreateInstance(beforeAlignment: SheetAlignment.Center,
                afterAlignment: SheetAlignment.Left, beforeAlpha: 1.0f, afterAlpha: 0.0f);
            var toRightExitAnim = SimpleTransitionAnimationObject.CreateInstance(beforeAlignment: SheetAlignment.Center,
                afterAlignment: SheetAlignment.Right, beforeAlpha: 1.0f, afterAlpha: 0.0f);
            var fromRightEnterAnim = SimpleTransitionAnimationObject.CreateInstance(
                beforeAlignment: SheetAlignment.Right,
                afterAlignment: SheetAlignment.Center, beforeAlpha: 0.0f, afterAlpha: 1.0f);
            var fromLeftEnterAnim = SimpleTransitionAnimationObject.CreateInstance(beforeAlignment: SheetAlignment.Left,
                afterAlignment: SheetAlignment.Center, beforeAlpha: 0.0f, afterAlpha: 1.0f);

            if (!string.IsNullOrEmpty(beforeSheetIdentifierRegex))
            {
                var enterAnimation1 = new TransitionAnimation();
                enterAnimation1.PartnerIdentifierRegex = beforeSheetIdentifierRegex;
                enterAnimation1.AssetType = AnimationAssetType.ScriptableObject;
                enterAnimation1.AnimationObject = fromRightEnterAnim;
                AnimationContainer.EnterAnimations.Add(enterAnimation1);
            }

            var enterAnimation2 = new TransitionAnimation();
            enterAnimation2.PartnerIdentifierRegex = afterSheetIdentifierRegex;
            enterAnimation2.AssetType = AnimationAssetType.ScriptableObject;
            enterAnimation2.AnimationObject = fromLeftEnterAnim;
            AnimationContainer.EnterAnimations.Add(enterAnimation2);

            if (!string.IsNullOrEmpty(beforeSheetIdentifierRegex))
            {
                var exitAnimation1 = new TransitionAnimation();
                exitAnimation1.PartnerIdentifierRegex = beforeSheetIdentifierRegex;
                exitAnimation1.AssetType = AnimationAssetType.ScriptableObject;
                exitAnimation1.AnimationObject = toRightExitAnim;
                AnimationContainer.ExitAnimations.Add(exitAnimation1);
            }

            var exitAnimation2 = new TransitionAnimation();
            exitAnimation2.PartnerIdentifierRegex = afterSheetIdentifierRegex;
            exitAnimation2.AssetType = AnimationAssetType.ScriptableObject;
            exitAnimation2.AnimationObject = toLeftExitAnim;
            AnimationContainer.ExitAnimations.Add(exitAnimation2);
        }

        public override UniTask Cleanup()
        {
            _firstThumbButton.onClick.RemoveListener(OnFirstThumbButtonClicked);
            var key = ResourceKey.CharacterThumbnailSprite(_characterId, 1);
            AddressablesManager.ReleaseAsset(key);
            return UniTask.CompletedTask;
        }

        private void OnFirstThumbButtonClicked()
        {
            var modalContainer = ModalContainer.Find(ContainerKey.ModalContainerLayer);
            var pushOption = new WindowOption(ResourceKey.CharacterModalPrefab(), true, onWindowCreated: modal =>
            {
                var characterModal = (CharacterModal) modal;
                characterModal.Setup(_characterId);
            });
            modalContainer.Push(pushOption);
        }
    }
}