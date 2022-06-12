using Demo.Scripts;
using UnityEditor;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Screen;
using UnityScreenNavigator.Runtime.Core.Shared;

public class TestUniTask : MonoBehaviour
{
    [SerializeField] private ScreenContainer screenContainer;

    public void Preload()
    {
        screenContainer.Preload(ResourceKey.ShopPagePrefab());
    }

    public void PushScreen()
    {
        var option = new WindowOption(ResourceKey.ShopPagePrefab(), true);
        screenContainer.Push(option);
    }

    public void PopScreen()
    {
        screenContainer.Pop(true);
    }
}

//custom editor TestUniTask
#if UNITY_EDITOR
[CustomEditor(typeof(TestUniTask))]
public class TestUniTaskEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var testUniTask = target as TestUniTask;

        if (GUILayout.Button("Preload")) testUniTask.Preload();
        if (GUILayout.Button("Push Screen")) testUniTask.PushScreen();

        if (GUILayout.Button("Pop Screen")) testUniTask.PopScreen();
    }
}
#endif