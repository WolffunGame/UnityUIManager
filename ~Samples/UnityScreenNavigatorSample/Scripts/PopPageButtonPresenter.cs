using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Screen;

namespace Demo
{
    [RequireComponent(typeof(Button))]
    public class PopPageButtonPresenter : MonoBehaviour
    {
        [SerializeField] private string _containerName;
        [SerializeField] private bool _playAnimation = true;

        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            var pageContainer = string.IsNullOrEmpty(_containerName)
                ? ScreenContainer.Of(transform)
                : ScreenContainer.Find(_containerName);
            pageContainer.Pop(_playAnimation);
        }
    }
}