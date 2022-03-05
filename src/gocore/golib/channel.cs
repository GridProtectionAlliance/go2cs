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

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Wait(CancellationToken token)
    {
        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
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
    private readonly ConcurrentQueue<T> m_queue;
    private readonly CancellationTokenSource m_enumeratorTokenSource;

    // Following value type is heap allocated so read-only or struct copy calls can still update
    // original value, e.g., allowing "builtin.close" method without requiring a "ref" parameter.
    private readonly ptr<bool> m_isClosed;

    /// <summary>
    /// Creates a new channel.
    /// </summary>
    /// <param name="size">
    /// Value greater than one will create a buffered channel; otherwise,
    /// an unbuffered channel will be created.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public channel(nint size)
    {
        if (size < 1)
            throw new ArgumentOutOfRangeException(nameof(size));

        m_canAddEvent = new(false);
        m_canTakeEvent = new(false);
        m_queue = new();
        m_enumeratorTokenSource = new();
        m_isClosed = new(false);

        Capacity = size;
    }

    /// <summary>
    /// Gets the capacity of the channel.
    /// </summary>
    public nint Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    /// <summary>
    /// Gets the count of items in the channel.
    /// </summary>
    public nint Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_queue?.Count ?? 0;
    }

    public bool IsUnbuffered
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Capacity == 1;
    }

    public bool IsClosed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_isClosed.val;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set => m_isClosed.val = value;
    }

    public bool SendIsReady
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_queue is not null && m_queue.Count != Capacity;
    }

    public bool ReceiveIsReady
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_queue is not null && !m_queue.IsEmpty;
    }

    /// <summary>
    /// Closes the channel.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySend(in T value) => TrySend(value, CancellationToken.None);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    /// For a buffered channel, method will block the current thread
    /// if channel is full.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Send(in T value) => Send(value, CancellationToken.None);

    /// <summary>
    /// Sends an item to channel. 
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// For a buffered channel, method will block the current thread
    /// if channel is full.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Send(in T value, CancellationToken cancellationToken)
    {
        // TODO: Need to think about how Go handles deadlock checks
        //if (IsUnbuffered && m_waitingReceivers <= 0)
        //    fatal("all goroutines are asleep - deadlock!", 2);

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

    void IChannel.Send(object value) => Send((T)value);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReceive(out T value) =>
        TryReceive(out value, CancellationToken.None);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Receive() => Receive(CancellationToken.None);

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// If the channel is empty, method will block the current thread until a value is sent to the channel.
    /// </remarks>
    /// <returns>Value received.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    public bool Sent(in T value)
    {
        if (SendIsReady)
        {
            m_queue.Enqueue(value);
            m_canTakeEvent.Set();
            return true;
        }

        return false;
    }

    public bool Received(out T value)
    {
        if (ReceiveIsReady)
        {
            if (m_queue.TryDequeue(out value!))
            {
                m_canAddEvent.Set();
                return true;
            }
        }

        value = default!;
        return false;
    }

    object IChannel.Receive() => Receive()!;

    bool IChannel.Sent(object value) => Sent((T)value);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator() => GetEnumerator(m_enumeratorTokenSource.Token);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertChannelIsOpenForSend()
    {
        if (IsClosed)
            throw new PanicException("send on closed channel");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertChannelIsOpenForReceive()
    {
        if (IsClosed)
            throw new PanicException("receive on closed channel");
    }
}
