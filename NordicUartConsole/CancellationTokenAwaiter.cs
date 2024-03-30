namespace NordicUartConsole;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

public static class AsyncExtensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static CancellationTokenAwaiter GetAwaiter(this CancellationToken token)
    {
        return new CancellationTokenAwaiter { CancellationToken = token };
    }

    /// <summary>
    /// Implements an awaiter for CancellationToken objects.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct CancellationTokenAwaiter(CancellationToken token) : INotifyCompletion, ICriticalNotifyCompletion
    {
        internal CancellationToken CancellationToken = token;

		public readonly object GetResult()
        {
            if (IsCompleted)
            {
                throw new OperationCanceledException();
            }

            throw new InvalidOperationException("token has not yet been canceled");
        }

		public readonly bool IsCompleted => CancellationToken.IsCancellationRequested;

		public readonly void OnCompleted(Action continuation) => CancellationToken.Register(continuation);

		public readonly void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
    }
}