//******************************************************************************************************
//  channel.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/02/2020 - J. Ritchie Carroll
//       Generated original version of source code inspired by Chan4Net source:
//          https://github.com/superopengl/Chan4Net
//          Copyright (c) 2016 Jun Shao
//
//******************************************************************************************************
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable StaticMemberInGenericType

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace go;

public interface IChannel : IEnumerable
{
    nint Capacity { get; }

    nint Length { get; }

    bool SendIsReady { get; }

    bool ReceiveIsReady { get; }

    void Send(object value);

    object Receive();

    bool Sent(object value);

    bool Received(out object value);

    void Close();
}

public static class channel
{
    public static bool Wait(CancellationToken token)
    {
        SemaphoreSlim semaphore = new(1, 1);
        bool tokenCanceled = false;

        try
        {
            semaphore.Wait(token);
        }
        catch (OperationCanceledException)
        {
            tokenCanceled = true;
        }
        finally
        {
            if (!tokenCanceled)
                semaphore.Release();
        }

        return true;
    }
}

/// <summary>
/// Represents a concurrency primitive that operates like a Go channel.
/// </summary>
/// <typeparam name="T">Target type for channel.</typeparam>
public struct channel<T> : IChannel, IEnumerable<T>
{
    private readonly ManualResetEventSlim m_canAddEvent;
    private readonly ManualResetEventSlim m_canTakeEvent;
    private readonly ManualResetEventSlim m_selectSendEvent;
    private readonly ConcurrentQueue<T> m_queue;
    private readonly CancellationTokenSource m_enumeratorTokenSource;

    // Following value type is heap allocated so read-only or struct copy calls can still update
    // original value, e.g., allowing "builtin.close" method without requiring a "ref" parameter.
    private readonly ptr<bool> m_isClosed;

    private static readonly WaitHandle s_signaled = new ManualResetEventSlim(true).WaitHandle;

    /// <summary>
    /// Creates a new channel.
    /// </summary>
    /// <param name="size">
    /// Value greater than one will create a buffered channel; otherwise,
    /// an unbuffered channel will be created.
    /// </param>
    public channel(nint size)
    {
        if (size < 1)
            throw new ArgumentOutOfRangeException(nameof(size));

        m_canAddEvent = new ManualResetEventSlim(false);
        m_canTakeEvent = new ManualResetEventSlim(false);
        m_selectSendEvent = new ManualResetEventSlim(false);
        m_queue = new ConcurrentQueue<T>();
        m_enumeratorTokenSource = new CancellationTokenSource();
        m_isClosed = new ptr<bool>(false);
        Capacity = size;
    }

    /// <summary>
    /// Gets the capacity of the channel.
    /// </summary>
    public nint Capacity { get; }

    /// <summary>
    /// Gets the count of items in the channel.
    /// </summary>
    public nint Length => IsUnbuffered ? 0 : m_queue?.Count ?? 0;

    /// <summary>
    /// Gets a flag that determines if the channel is unbuffered.
    /// </summary>
    public bool IsUnbuffered => Capacity == 1;

    /// <summary>
    /// Gets a flag that determines if the channel is closed.
    /// </summary>
    public bool IsClosed
    {
        get => m_isClosed.val;
        private set => m_isClosed.val = value;
    }

    /// <summary>
    /// Gets a flag that determines if the channel is ready to send.
    /// </summary>
    public bool SendIsReady => m_queue is not null && m_queue.Count < Capacity;

    /// <summary>
    /// Gets a flag that determines if the channel is ready to receive.
    /// </summary>
    public bool ReceiveIsReady => m_queue is not null && !m_queue.IsEmpty;

    /// <summary>
    /// Gets the wait handle that is set when data is being received from the channel.
    /// </summary>
    public WaitHandle Receiving => m_canTakeEvent.WaitHandle;

    /// <summary>
    /// Closes the channel.
    /// </summary>
    public void Close()
    {
        if (IsClosed)
            throw new PanicException("close of closed channel");

        IsClosed = true;

        m_enumeratorTokenSource.Cancel();
    }

    /// <summary>
    /// Attempt to send an item to channel.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <returns>
    /// <c>true</c> if value was sent; otherwise,
    /// <c>false</c> as send would have blocked
    /// </returns>
    /// <remarks>
    /// This method will not block.
    /// </remarks>
    public bool TrySend(in T value)
    {
        return TrySend(value, CancellationToken.None);
    }

    /// <summary>
    /// Attempt to send an item to channel.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// <c>true</c> if value was sent; otherwise,
    /// <c>false</c> as send would have blocked.
    /// </returns>
    /// <remarks>
    /// This method will not block.
    /// </remarks>
    public bool TrySend(in T value, CancellationToken cancellationToken)
    {
        AssertChannelIsOpenForSend();

        if (!SendIsReady)
            return false;

        cancellationToken.ThrowIfCancellationRequested();

        m_queue.Enqueue(value);
        m_canTakeEvent.Set();
        return true;
    }

    /// <summary>
    /// Sends an item to channel. 
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <remarks>
    /// For a buffered channel, method will block the current thread if channel is full.
    /// </remarks>
    public void Send(in T value)
    {
        Send(value, CancellationToken.None);
    }

    /// <summary>
    /// Sends an item to channel. 
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <remarks>
    /// <para>
    /// For a buffered channel, method will block the current thread if channel is full.
    /// </para>
    /// <para>
    /// Defines a Go style channel Send operation.
    /// </para>
    /// </remarks>
    public void ᐸꟷ(in T value)
    {
        Send(value, CancellationToken.None);
    }

    /// <summary>
    /// Gets a wait handle that is set when the send operation is complete.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <returns>Wait handle for send operation.</returns>
    public WaitHandle Sending(in T value)
    {
        // If channel is ready, send immediately on current thread
        if (TrySend(value))
            return s_signaled;

        // Otherwise, queue send operation for processing on separate thread
        m_selectSendEvent.Reset();
        ThreadPool.QueueUserWorkItem(ProcessSendQueue, value);
        return m_selectSendEvent.WaitHandle;
    }

    private void ProcessSendQueue(object? state)
    {
        Send((T)state!, m_enumeratorTokenSource.Token);
        m_selectSendEvent.Set();
    }

    /// <summary>
    /// Gets a wait handle that is set when the send operation is complete.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꓸꓸꓸ"/>.</param>
    /// <returns>Wait handle for send operation.</returns>
    /// <remarks>
    /// Defines a Go style channel <see cref="Sending"/>> wait handle.
    /// </remarks>
    public WaitHandle ᐸꟷ(in T value, NilType _)
    {
        return Sending(value);
    }

    /// <summary>
    /// Sends an item to channel. 
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// For a buffered channel, method will block the current thread
    /// if channel is full.
    /// </remarks>
    public void Send(in T value, CancellationToken cancellationToken)
    {
        // TODO: Verify behavior of Go deadlock handling
        //if (IsUnbuffered && m_waitingReceivers <= 0)
        //    fatal(FatalError.DeadLock());

        // Per spec, sending to a nil channel blocks forever
        if (m_queue is null && channel.Wait(cancellationToken))
            return;

        while (!SendIsReady)
        {
            AssertChannelIsOpenForSend();
            m_canAddEvent.Wait(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            m_canAddEvent.Reset();
        }

        AssertChannelIsOpenForSend();

        m_queue!.Enqueue(value);
        m_canTakeEvent.Set();
    }

    void IChannel.Send(object value)
    {
        Send((T)value);
    }

    /// <summary>
    /// Attempts to send a value to the channel.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <returns><c>true</c> if a value was sent; otherwise, <c>false</c>.</returns>
    public bool Sent(in T value)
    {
        if (!SendIsReady)
            return false;

        m_queue.Enqueue(value);
        m_canTakeEvent.Set();
        return true;
    }

    /// <summary>
    /// Attempts to remove an item from channel.
    /// </summary>
    /// <param name="value">Returned value.</param>
    /// <returns>
    /// <c>true</c> if value was received; otherwise,
    /// <c>false</c> as receive would have blocked.
    /// </returns>
    /// <remarks>
    /// This method will not block.
    /// </remarks>
    public bool TryReceive(out T value)
    {
        return TryReceive(out value, CancellationToken.None);
    }

    /// <summary>
    /// Attempts to remove an item from channel.
    /// </summary>
    /// <param name="value">Returned value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// <c>true</c> if value was received; otherwise,
    /// <c>false</c> as receive would have blocked.
    /// </returns>
    /// <remarks>
    /// This method will not block.
    /// </remarks>
    public bool TryReceive(out T value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (m_queue is null)
        {
            value = default!;
            return false;
        }

        if (!m_queue.TryDequeue(out value!))
            return false;

        m_canAddEvent.Set();
        return true;
    }

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <remarks>
    /// If the channel is empty, method will block the current thread until a value is sent to the channel.
    /// </remarks>
    /// <returns>Value received.</returns>
    public T Receive()
    {
        return Receive(CancellationToken.None);
    }

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// If the channel is empty, method will block the current thread until a value is sent to the channel.
    /// </remarks>
    /// <returns>Value received.</returns>
    public T Receive(CancellationToken cancellationToken)
    {
        // Per spec, receiving from a nil channel blocks forever
        if (m_queue is null && channel.Wait(cancellationToken))
            return default!;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            m_canTakeEvent.Reset();

            if (m_queue!.TryDequeue(out T? value))
            {
                m_canAddEvent.Set();
                return value;
            }

            if (m_queue.IsEmpty)
                AssertChannelIsOpenForReceive();

            m_canTakeEvent.Wait(cancellationToken);
        }
    }

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꟷ"/>.</param>
    /// <returns>
    /// Received value and boolean result reporting whether the communication succeeded which is
    /// <c>true</c> if the value received was delivered by a successful send operation ; otherwise,
    /// <c>false</c> if a zero value generated because the channel is closed and empty.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If the channel is empty, method will block the current thread until a value is sent to the channel.
    /// </para>
    /// <para>
    /// Defines a Go style channel <see cref="channel{T}.Receive()"/>> operation.
    /// </para>
    /// </remarks>
    public (T val, bool ok) Receive(bool _)
    {
        return IsClosed ?
            (zero<T>(), false) :
            (Receive(), true);
    }

    /// <summary>
    /// Attempts to receive a value from the channel.
    /// </summary>
    /// <param name="value">Output parameter for received value.</param>
    /// <returns><c>true</c> if a value was received; otherwise, <c>false</c>.</returns>
    public bool Received(out T value)
    {
        if (ReceiveIsReady)
        {
            if (m_queue.TryDequeue(out value!))
            {
                m_canAddEvent.Set();
                m_canTakeEvent.Reset();
                return true;
            }
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Attempts to receive a value from the channel and boolean flag indicating if
    /// the channel is not closed.
    /// </summary>
    /// <param name="value">Output parameter for received value.</param>
    /// <param name="ok">Boolean flag indicating if the channel is not closed.</param>
    /// <returns>
    /// <c>true</c> if a value was received and channel was not closed;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Received(out T value, out bool ok)
    {
        ok = !IsClosed;

        if (ok)
            return Received(out value);

        value = zero<T>();
        return false;
    }

    /// <summary>
    /// Attempts to receive a value from the channel.
    /// </summary>
    /// <param name="value">Output parameter for received value.</param>
    /// <returns><c>true</c> if a value was received; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Defines a Go style channel Receive operation.
    /// </remarks>
    /// <seealso cref="ᐸꟷ{T}(channel{T})"/>.
    public bool ꟷᐳ(out T value)
    {
        return Received(out value);
    }

    /// <summary>
    /// Attempts to receive a value from the channel and boolean flag indicating if
    /// the channel is not closed.
    /// </summary>
    /// <param name="value">Output parameter for received value.</param>
    /// <param name="ok">Boolean flag indicating if the channel is not closed.</param>
    /// <returns>
    /// <c>true</c> if a value was received and channel was not closed;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Defines a Go style channel Receive operation.
    /// </remarks>
    /// <seealso cref="ᐸꟷ{T}(channel{T})"/>.
    public bool ꟷᐳ(out T value, out bool ok)
    {
        return Received(out value, out ok);
    }

    object IChannel.Receive()
    {
        return Receive()!;
    }

    bool IChannel.Sent(object value)
    {
        return Sent((T)value);
    }

    bool IChannel.Received(out object value)
    {
        if (Received(out T receivedValue))
        {
            value = receivedValue!;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator(CancellationToken cancellationToken)
    {
        // Per spec, receiving from a nil channel blocks forever
        if (m_queue is null && channel.Wait(cancellationToken))
            yield break;

        while (!IsClosed)
        {
            T value = default!;
            bool assigned;

            try
            {
                value = Receive(cancellationToken);
                assigned = true;
            }
            catch (OperationCanceledException)
            {
                assigned = false;
            }
            catch (PanicException)
            {
                assigned = false;
            }

            if (!assigned)
                break;

            yield return value;
        }

        if (IsClosed)
        {
            while (TryReceive(out T value))
                yield return value;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return GetEnumerator(m_enumeratorTokenSource.Token);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void AssertChannelIsOpenForSend()
    {
        if (IsClosed)
            throw new PanicException("send on closed channel");
    }

    private void AssertChannelIsOpenForReceive()
    {
        if (IsClosed)
            throw new PanicException("receive on closed channel");
    }

    // Stylistic support for channel send operation: "ch <- 12" becomes "ch <<= 12"
    // This option is not used by default since it incurs an unnecessary assignment
    // since "ch <<= 12" is equivalent to "ch = ch << 12"
    public static channel<T> operator <<(channel<T> source, T value)
    {
        source.Send(value);
        return source;
    }
}
