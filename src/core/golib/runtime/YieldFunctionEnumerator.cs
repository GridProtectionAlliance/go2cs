using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace go.runtime;

internal class YieldFunctionEnumerable<T>(Action<Func<T, bool>> enumerator) : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator()
    {
        return new YieldFunctionEnumerator(enumerator);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class YieldFunctionEnumerator : IEnumerator<T>
    {
        private volatile bool m_hasValue;
        private volatile bool m_completed;
        private volatile bool m_started;

        private readonly ManualResetEventSlim m_processed;  // Consumer processed value event
        private readonly ManualResetEventSlim m_yielded;    // Producer yielded value event
        private readonly CancellationTokenSource m_cancellationTokenSource;

        private bool m_disposed;

        // This enumerator synchronizes value production and consumption
        public YieldFunctionEnumerator(Action<Func<T, bool>> enumerator)
        {
            m_processed = new ManualResetEventSlim(false);
            m_yielded = new ManualResetEventSlim(false);
            m_cancellationTokenSource = new CancellationTokenSource();

            Task.Run(processEnumerator, m_cancellationTokenSource.Token);
            return;

            // Producer task that calls user function to yield values
            void processEnumerator()
            {
                CancellationToken token = m_cancellationTokenSource.Token;

                try
                {
                    // 'enumerator' is user a function that enumerates over source values. The function
                    // is provided with a delegate that is called by the user while enumerating in order
                    // to yield values. The implementation of this yielding delegate is provided here:
                    enumerator(value =>
                    {
                        if (token.IsCancellationRequested)
                            return false;

                        // Store provided user value in the .NET enumerator's 'Current' property
                        Current = value;
                        m_hasValue = true;

                        try
                        {
                            // Signal consumer that producer yielded a value
                            m_yielded.Set();

                            // Wait for consumer to process the value
                            m_processed.Wait(token);
                            m_processed.Reset();

                            return !token.IsCancellationRequested;
                        }
                        catch (ObjectDisposedException)
                        {
                            return false;
                        }
                        catch (OperationCanceledException)
                        {
                            return false;
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    m_completed = true;
                    m_yielded.Set();
                }
            }
        }

        public T Current { get; private set; } = default!;

        object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            if (m_completed)
                return false;

            // Signal producer that consumer processed the value
            if (m_started)
                m_processed.Set();

            // Wait for producer to yield next value
            m_started = true;
            m_yielded.Wait(m_cancellationTokenSource.Token);
            m_yielded.Reset();

            return m_hasValue && !m_completed;
        }

        public void Reset()
        {
            throw new NotSupportedException("Reset is not supported for this enumerator");
        }

        public void Dispose()
        {
            if (m_disposed)
                return;

            try
            {
                m_cancellationTokenSource.Cancel();
                m_processed.Set();
                m_yielded.Set();
            }
            finally
            {
                m_disposed = true;
                m_cancellationTokenSource.Dispose();
                m_processed.Dispose();
                m_yielded.Dispose();
            }
        }
    }
}
