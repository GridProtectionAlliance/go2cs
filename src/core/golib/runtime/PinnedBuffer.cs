// PinnedBuffer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace go.golib;

/// <summary>
/// Represents a pinned buffer that can be used as an array pointer reference.
/// </summary>
/// <remarks>
/// Buffer is allocated with a pinned handle so it can be used in unmanaged code.
/// Because buffer is pinned, it will not be moved by the garbage collector.
/// Pinned buffer class is disposable and registered with the garbage collector
/// so allocated handle will be freed when class is no longer referenced.
/// </remarks>
internal class PinnedBuffer : IArray<byte>, IDisposable
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

    public unsafe byte* Pointer => (byte*)m_handle.AddrOfPinnedObject();

    // The pinned storage object itself (normally the byte[] the buffer wraps). PinnedBuffer is a
    // per-access VIEW — e.g. @string.buffer creates a fresh one per unsafe.StringData call — so
    // pointer identity (ж equality/hashing) must compare this canonical target, not the view.
    internal object? PinnedTarget => m_handle.IsAllocated ? m_handle.Target : null;

    public nint Length => m_len;

    public Span<byte> ꓸꓸꓸ => ToSpan();

    public byte[] Source => ToSpan().ToArray();

    Array IArray.Source => Source;

    public unsafe ref byte this[nint index]
    {
        get
        {
            if (index < 0 || index >= m_len)
                throw new IndexOutOfRangeException();

            return ref Pointer[index];
        }
    }

    // IArray implementation allows PinnedBuffer to be used as
    // an indexed array pointer reference, see ptr constructor:
    // ж(IArray array, int index)
    unsafe object? IArray.this[nint index]
    {
        get => this[index];
        set
        {
            if (index < 0 || index >= m_len)
                throw new IndexOutOfRangeException();

            Pointer[index] = value switch
            {
                byte val => val,
                NilType => 0,
                null => 0,
                _ => throw new ArgumentException("Invalid value type.")
            };
        }
    }

    public slice<byte> this[Range range] => ToSpan()[range];

    public slice<byte> Slice(int start, int length) => ToSpan().Slice(start, length);

    public slice<byte> Slice(nint start, nint length) => ToSpan().Slice((int)start, (int)length);

    public unsafe Span<byte> ToSpan()
    {
        return new Span<byte>(Pointer, m_len);
    }

    public IEnumerator<(nint, byte)> GetEnumerator()
    {
        byte[] array = Source;
        nint index = 0;

        foreach (byte item in array)
            yield return (index++, item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Source.GetEnumerator();
    }

    public object Clone()
    {
        return new PinnedBuffer(m_handle, m_len);
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
}
