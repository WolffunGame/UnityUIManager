using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    public interface IUIView
    {
        RectTransform RectTransform { get; }

        float Alpha { get; set; }

        bool Interactable { get; set; }

        CanvasGroup CanvasGroup { get; }
        
        string Name { get; set; }

        Transform Parent { get; }

        GameObject Owner { get; }

        bool Visibility { get; set; }
        
        /// <summary>
        /// External extended attributes
        /// </summary>
        IAttributes ExtraAttributes { get; }
        
        //TODO add animations
    }
}