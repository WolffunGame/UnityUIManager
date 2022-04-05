using System;

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnityScreenNavigator.Runtime.Foundation.Coroutine
{
    internal interface IAsyncProcessHandleSetter
    {
        void Complete(object result);

        void Error(Exception ex);
    }

    public class AsyncProcessHandle : CustomYieldInstruction, IAsyncProcessHandleSetter
    {
        private readonly UniTaskCompletionSource<object> _tcs = new UniTaskCompletionSource<object>();

        public AsyncProcessHandle(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public object Result { get; private set; }

        public bool IsTerminated { get; private set; }

        public Exception Exception { get; private set; }

        public UniTask<object> UniTask => _tcs.Task;

        public bool HasError => Exception != null;

        public override bool keepWaiting => !IsTerminated;

        void IAsyncProcessHandleSetter.Complete(object result)
        {
            Result = result;
            IsTerminated = true;
            OnTerminate?.Invoke();
            _tcs.TrySetResult(result);
        }

        void IAsyncProcessHandleSetter.Error(Exception ex)
        {
            Exception = ex;
            IsTerminated = true;
            OnTerminate?.Invoke();
            _tcs.TrySetException(ex);
        }

        public event Action OnTerminate;
    }
}