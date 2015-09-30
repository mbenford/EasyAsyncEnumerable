using System;
using System.Collections.Generic;

namespace EasyAsyncEnumerable
{
    /// <summary>
    /// Controls the flow of the YieldableAsyncEnumerator class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AsyncYielder<T>
    {
        private readonly Queue<T> values = new Queue<T>();

        internal AsyncYielder()
        {
            State = YielderState.Idle;
        }

        /// <summary>
        /// Enqueues a value to be processed by the iterator.
        /// </summary>
        /// <param name="value">Value to be enqueued.</param>
        /// <returns>An instance of the <see cref="AsyncYielder{T}"/> class so further calls can be chained.</returns>
        public AsyncYielder<T> Return(T value)
        {
            EnsureIsNotStopped();
            values.Enqueue(value);
            State = YielderState.Running;
            return this;
        }

        /// <summary>
        /// Ends the iteration.
        /// </summary>
        public void Break()
        {
            EnsureIsNotStopped();
            State = YielderState.Stopped;
        }

        internal T GetNext()
        {
            EnsureIsNotStopped();
            T value = values.Dequeue();
            State = values.Count == 0 ? YielderState.Idle : YielderState.Running;
            return value;
        }

        internal YielderState State { get; private set; }

        private void EnsureIsNotStopped()
        {
            if (State == YielderState.Stopped) throw new InvalidOperationException("Yielder is stopped");
        }
    }

    enum YielderState
    {
        Idle,
        Running,
        Stopped
    }
}