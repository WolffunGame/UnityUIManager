using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnityScreenNavigator.Runtime.Foundation.Animation
{
    internal static class AnimationExtensions
    {
        public static async UniTask CreatePlayRoutine(this IAnimation self)
        {
            var player = new AnimationPlayer(self);
            UpdateDispatcher.Instance.Register(player);
            player.Play();
            await UniTask.WaitUntil(() => player.IsFinished);
            UpdateDispatcher.Instance.Unregister(player);
        }
    }
}