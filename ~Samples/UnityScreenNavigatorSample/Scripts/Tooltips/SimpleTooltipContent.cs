using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace _Samples.UnityScreenNavigatorSample.Scripts.Tooltips
{
    public class SimpleTooltipContent : UIView
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text title;

        protected override void Awake()
        {
            base.Awake();
            
        }
    }
}