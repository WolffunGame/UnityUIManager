using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Interactivity;

namespace Demo.Scripts.Dialogs
{
    public class TestDialog : MonoBehaviour
    {
        [SerializeField] private Image tipObject;
        public async UniTaskVoid OpenDialog()
        {
            AlertDialog.DialogKey = "Prefabs/prefab_alert_dialog";
            var result = await DefaultDialogService.ShowDialog("Hello World", "This is the first dialog in the demo",
                "OK", "Cancel");
            if (result == AlertDialog.BUTTON_POSITIVE)
            {
                Debug.Log("Positive button clicked");
            }
            else if (result == AlertDialog.BUTTON_NEGATIVE)
            {
                Debug.Log("Negative button clicked");
            }
            else
            {
                Debug.Log("Neutral button clicked");
            }
        }

    }

    [CustomEditor(typeof(TestDialog))]
    public class TestDialogEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Open Dialog"))
            {
                ((TestDialog) target).OpenDialog().Forget();
            }
        }
    }
}