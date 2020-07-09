//******************************************************************************************************
//  channel.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  10/03/2018 - Ritchie Carroll
//       Generated original version of source based on work by ChrisWue in the following discussion:
//          https://codereview.stackexchange.com/questions/32500/golang-channel-in-c
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go
{
    public interface IChannel : EmptyInterface
    {
        ICollection Buffer { get; }

        int Capacity { get; }

        int Length { get; }

        bool Send(object value);

        bool Receive(out object value);

        void Close();

        IEnumerable Range();
    }

    [Serializable]
    public readonly struct channel<T> : IChannel
    {
        // Because "select" statement can operate on any channel types, buffer is internally managed
        // as object so that BlockingCollection<T>.TakeFromAny will work for any types
        private readonly BlockingCollection<object> m_buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public channel(int size = 0)
        {
            m_buffer = size < 1 ? new BlockingCollection<object>() : new BlockingCollection<object>(size);
        }

        public ICollection Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_buffer;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int capacity = m_buffer?.BoundedCapacity ?? 0;

                if (capacity < 0 || capacity == int.MaxValue)
                    return 0;

                return capacity;
            }
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_buffer?.Count ?? 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Send(T value)
        {
            // Per spec, sending on a nil channel blocks forever
            if (m_buffer == null)
                Monitor.Wait(this); // Pulse never expected

            try
            {
                m_buffer.Add(value);
            }
            catch (InvalidOperationException)
            {
                // Thrown when the collection gets closed
                throw new PanicException("panic: send on closed channel");
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Receive(out T value)
        {
            // Per spec, receiving from a nil channel blocks forever
            if (m_buffer == null)
                Monitor.Wait(this); // Pulse never expected

            try
            {
                value = (T)m_buffer.Take();
            }
            catch (InvalidOperationException)
            {
                // Thrown when the collection is empty / closed
                value = default;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Close()
        {
            if (m_buffer == null)
                throw new PanicException(RuntimeErrorPanic.NilPointerDereference);

            if (m_buffer.IsAddingCompleted)
                throw new PanicException("panic: close of closed channel");

            m_buffer.CompleteAdding();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> Range()
        {
            while (Receive(out T value))
                yield return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => m_buffer.PrintPointer();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (object)m_buffer == null ? 0 : m_buffer.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => base.Equals(obj);

        #region [ Equality Operators ]

        // channel<T> to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(channel<T> channel, NilType nil) => channel.Length == 0 && channel.Capacity == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(channel<T> channel, NilType nil) => !(channel == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, channel<T> channel) => channel == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, channel<T> channel) => channel != nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator channel<T>(NilType nil) => default;

        #endregion

        #region [ Interface Implementations ]

        bool IChannel.Send(object value) => Send((T)value);

        bool IChannel.Receive(out object value)
        {
            bool result = Receive(out T typedValue);
            value = typedValue;
            return result;
        }

        IEnumerable IChannel.Range() => Range();

        #endregion

        // Typically "blockOnAllNil" should be true if there is no default case
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Select(IChannel[] channels, out object value, bool blockOnAllNil)
        {
            ICollection[] buffers = channels
                .Select(channel => channel.Buffer)
                .ToArray();

            // Per spec, selecting on all nil channels with no default case blocks forever
            if (blockOnAllNil && buffers.All(buffer => (object)buffer == null))
                Monitor.Wait(channels); // Pulse never expected

            BlockingCollection<object>[] collections = buffers
                .Where(buffer => (object)buffer != null)
                .Cast<BlockingCollection<object>>()
                .ToArray();

            if (buffers.Length != 0)
                return BlockingCollection<object>.TakeFromAny(collections, out value);

            value = null;
            return -1;
        }
    }
}
