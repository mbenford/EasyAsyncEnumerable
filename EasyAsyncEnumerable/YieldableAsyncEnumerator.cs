using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyAsyncEnumerable
{
    class YieldableAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly Func<AsyncYielder<T>, CancellationToken, Task> producer;
        private readonly AsyncYielder<T> yielder;

        public YieldableAsyncEnumerator(Func<AsyncYielder<T>, CancellationToken, Task> producer)
        {
            this.producer = producer;
            yielder = new AsyncYielder<T>();
        }

        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return false;
            if (yielder.State == YielderState.Idle) await producer(yielder, cancellationToken);
            if (yielder.State == YielderState.Idle || yielder.State == YielderState.Stopped) return false;
            
            Current = yielder.GetNext();
            return true;
        }

        public void Dispose()
        {
        }

        public T Current { get; private set; }
    }
}