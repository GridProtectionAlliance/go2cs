//******************************************************************************************************
//  PinnedBuffer.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  03/09/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace go.runtime;

internal class PinnedBuffer : IArray, IDisposable
{
    private GCHandle m_handle;
    private readonly int m_len;
    private bool m_disposed;

    public PinnedBuffer(byte[] array)
    {
        m_handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        m_len = array.Length;
    }

    public PinnedBuffer(object? src, nint len) : this(src, (int)len) {}

    public PinnedBuffer(object? src, int len)
    {
        m_handle = src is null ? default : GCHandle.Alloc(src, GCHandleType.Pinned);
        m_len = len;
    }

    private PinnedBuffer(GCHandle handle, int len)
    {
        m_handle = handle;
        m_len = len;
    }

    public unsafe byte* Pointer => (byte*)m_handle.AddrOfPinnedObject();

    public unsafe Span<byte> ToSpan()
    {
        return new Span<byte>(Pointer, m_len);
    }

    ~PinnedBuffer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (m_disposed)
            return;
            
        if (m_handle.IsAllocated)
            m_handle.Free();

        m_disposed = true;
    }

    public static implicit operator PinnedBuffer(byte[] array)
    {
        return new PinnedBuffer(array);
    }

    public static implicit operator Span<byte>(PinnedBuffer buffer)
    {
        return buffer.ToSpan();
    }

    public static unsafe implicit operator byte*(PinnedBuffer buffer)
    {
        return buffer.Pointer;
    }

    public static implicit operator PinnedBuffer(@string src)
    {
        return new PinnedBuffer(src, src.Length);
    }

    object ICloneable.Clone() => new PinnedBuffer(m_handle, m_len);

    IEnumerator IEnumerable.GetEnumerator() => ToSpan().ToArray().GetEnumerator();

    Array IArray.Source => ToSpan().ToArray();

    nint IArray.Length => m_len;

    // IArray implementation allows PinnedBuffer to be used as
    // an indexed array pointer reference, see ptr constructor:
    // ж(IArray array, int index)
    unsafe object? IArray.this[nint index]
    {
        get => Pointer[index];
        set
        {
            Pointer[index] = value switch
            {
                byte val => val,
                null => 0,
                _ => throw new ArgumentException("Invalid value type.")
            };
        }
    }
}
