// ptr.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable UnusedParameter.Local

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace go.experimental
{
    /// <summary>
    /// Represents a heap allocated reference to an instance of type <typeparamref name="T"/>
    /// where <typeparamref name="T"/> is an unmanaged type, e.g., a struct.
    /// </summary>
    /// <typeparam name="T">Unmanaged type for heap based reference.</typeparam>
    /// <remarks>
    /// <para>
    /// A .NET class is always allocated on the heap and registered for garbage collection.
    /// The <see cref="ptr{T}"/> class is used to create a reference to a heap allocated instance
    /// of type <typeparamref name="T"/> so that the type can (1) have scope beyond the current
    /// stack, and (2) have the ability to create a pointer to the type.
    /// </para>
    /// <para>
    /// For the lifetime of this class, value will be pinned in memory to ensure pointer
    /// access is consistent.
    /// </para>
    /// <para>
    /// This class can just as easily be a blittable struct, however, struct implementations do
    /// not support destructors. Without a destructor manual reference counting for structs would
    /// be required so system would know when to free GCHandle. Note that not freeing the GCHandle
    /// causes a memory leak.
    ///
    /// When <see cref="ptr{T}"/> is a class this creates the following conundrum: when a structure
    /// contains a pointer field then creating a pointer to this structure that contains an instance
    /// of <see cref="ptr{T}"/> is impossible since <see cref="ptr{T}"/> is a class and a class is
    /// non-blittable.
    /// </para>
    /// </remarks>
    public sealed unsafe class ptr<T> : IDisposable where T : unmanaged
    {
        private GCHandle m_handle;
        private readonly T* m_value;

        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref *m_value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ptr(in T value)
        {
            m_handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            m_value = (T*)m_handle.AddrOfPinnedObject();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ptr(NilType _) : this() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ptr() : this(default(T)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ~ptr() => Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (!m_handle.IsAllocated)
                return;

            m_handle.Free();
            GC.SuppressFinalize(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"&{Value.ToString() ?? "nil"}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Equals(ptr<T> other) => m_handle.Equals(other.m_handle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is ptr<T> other && Equals(other);

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => m_handle.GetHashCode();

        // Enable implicit conversions between ptr<T> and related types
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(ptr<T> value) => value.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ptr<T>(T* value) => new ptr<T>(*value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator IntPtr(ptr<T> value) => (IntPtr)value.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UIntPtr(ptr<T> value) => (UIntPtr)value.m_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* operator ~(ptr<T> value) => value.m_value;

        // Enable comparisons between nil and ptr<T> interface instance
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ptr<T> value, NilType _) => value.m_value == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ptr<T> value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, ptr<T> value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, ptr<T> value) => value != nil;
    }
}