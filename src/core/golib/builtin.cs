//******************************************************************************************************
//  builtin.cs - Gbtc
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
//  05/05/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable UseSymbolAlias

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using go.runtime;
using static System.Math;
using TypeExtensions = go.runtime.TypeExtensions;

namespace go;

public static class builtin
{
    public const int StackAllocThreshold = 1024; // 1KB

    private static readonly ThreadLocal<bool> s_fallthrough = new();
    private static readonly ConcurrentDictionary<(Type, Type), bool> s_implementsInterface = [];
    private static readonly Type[] s_asTParams = [Type.MakeGenericMethodParameter(0).MakeByRefType()];

    [ModuleInitializer]
    internal static void InitializeGoLib()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Console.OutputEncoding = Console.InputEncoding = Encoding.UTF8;
        return;

        [MethodImpl(MethodImplOptions.Synchronized)]
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception? ex = e.ExceptionObject as Exception;
            Console.WriteLine($"{(ex is PanicException ? "panic: " : "")}{ex?.Message ?? $"unhandled exception: {e.ExceptionObject}"}");
            Environment.Exit(0);
        }
    }

    private static class Zero<T>
    {
        public static T Default = default!;
    }

    /// <summary>
    /// Predeclared identifier representing the untyped integer ordinal number of the current
    /// const specification in a (usually parenthesized) const declaration.
    /// It is zero-indexed.
    /// </summary>
    public const nint iota = 0;

    /// <summary>
    /// nil is a predeclared identifier representing the zero value for a pointer, channel,
    /// func, interface, map, or slice type.
    /// </summary>
    public static readonly NilType nil = NilType.Default;

    /// <summary>
    /// Defines a true constant used as a discriminator for accessing overloads with
    /// different return types.
    /// </summary>
    /// <remarks>
    /// Common use is for expression-based switch values where focus is
    /// on <c>when</c> conditions and not the value itself.
    /// </remarks>
    public const bool ᐧ = true;

    /// <summary>
    /// Defines a false constant used as a discriminator for accessing overloads with
    /// different return types.
    /// </summary>
    /// <remarks>
    /// Often used with functions that return a value and a boolean success or error state
    /// as a tuple.
    /// </remarks>
    public const bool ꟷ = false;

    /// <summary>
    /// Defines a nil constant used as a discriminator for accessing overloads with
    /// different return types.
    /// </summary>
    /// <remarks>
    /// Often used with functions that have an operation that will be continued, e.g.:
    /// <c>switch (select(f.ᐸꟷ(x, ꓸꓸꓸ), ᐸꟷ(quit, ꓸꓸꓸ)))</c>
    /// The above channel operations return a wait handle that is triggered when the
    /// operation is complete.
    /// </remarks>
    public static readonly NilType ꓸꓸꓸ = nil;

    /// <summary>
    /// Instructs a switch case extension call to transfer control to the first
    /// statement of the next case clause in an expression.
    /// </summary>
    /// <remarks>
    /// Note that <c>fallthrough</c> is a reserved Go keyword, so it will not be
    /// used as a variable name.
    /// </remarks>
    public static bool fallthrough
    {
        get
        {
            if (!s_fallthrough.Value)
                return false;

            s_fallthrough.Value = false;
            return true;
        }
        set
        {
            s_fallthrough.Value = value;
        }
    }

    /// <summary>
    /// Raises a <see cref="PanicException"/> with the specified <paramref name="state"/>.
    /// </summary>
    /// <param name="state">State of panic exception.</param>
    /// <exception cref="PanicException">Panic exception with the specified <paramref name="state"/>.</exception>
    public static void panic(object state)
    {
        throw new PanicException(state);
    }

    /// <summary>
    /// Appends elements to the end of a slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<T> append<T>(in slice<T> slice, params T[] elems)
    {
        return go.slice<T>.Append(slice, elems);
    }

    /// <summary>
    /// Appends elements to the end of a slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <typeparam name="T">Type of slice.</typeparam>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<T> append<T>(ISlice slice, params T[] elems)
    {
        return (slice<T>)slice.Append(elems.Cast<object>().ToArray())!;
    }

    /// <summary>
    /// Appends elements to the end of a slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <typeparam name="T">Type of slice.</typeparam>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<T> append<T>(slice<T> slice, params T[] elems)
    {
        return go.slice<T>.Append(slice, elems);
    }

    /// <summary>
    /// Appends elements to the end of a slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <typeparam name="T">Type of slice.</typeparam>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<T> append<T>(slice<T> slice, params Span<T> elems)
    {
        return go.slice<T>.Append(slice, elems);
    }

    /// <summary>
    /// Appends runes to the end of a byte slice. If it has sufficient capacity, the destination is
    /// resliced to accommodate the new elements. If it does not, a new underlying array will be
    /// allocated.
    /// </summary>
    /// <param name="slice">Destination slice pointer.</param>
    /// <param name="elems">Elements to append.</param>
    /// <returns>New slice with specified values appended.</returns>
    /// <remarks>
    /// Append returns the updated slice. It is therefore necessary to store the result of append,
    /// often in the variable holding the slice itself:
    /// <code>
    /// slice = append(slice, elem1, elem2)
    /// slice = append(slice, anotherSlice...)
    /// </code>
    /// </remarks>
    public static slice<byte> append(slice<byte> slice, params ReadOnlySpan<rune> elems)
    {
        return go.slice<byte>.Append(slice, elems.ToUTF8Bytes());
    }

    /// <summary>
    /// Converts a span of runes to a UTF-8 byte array.
    /// </summary>
    /// <param name="runes">Runes to convert to bytes.</param>
    /// <returns>byte array representing UTF-8 encoding of runes.</returns>
    /// <exception cref="InvalidOperationException">Buffer too small for UTF-8 encoding.</exception>
    public static byte[] ToUTF8Bytes(this in ReadOnlySpan<rune> runes)
    {
        // Estimate buffer size (4 bytes per rune as worst case)
        int estimatedBytes = runes.Length * 4;
        
        Span<byte> buffer = estimatedBytes <= StackAllocThreshold ? 
            stackalloc byte[estimatedBytes] : 
            new byte[estimatedBytes];

        int bytesWritten = 0;

        foreach (Rune codePoint in runes)
        {
            // Encoding not expected to fail given 4x buffer
            if (!codePoint.TryEncodeToUtf8(buffer[bytesWritten..], out int runeBytes))
                throw new InvalidOperationException("Buffer too small for UTF-8 encoding");
            
            bytesWritten += runeBytes;
        }

        return buffer[..bytesWritten].ToArray();
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/> (same as len(array)).
    /// </summary>
    /// <param name="array">Target array pointer.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint cap<T>(in array<T> array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/> (same as len(array)).
    /// </summary>
    /// <param name="array">Target array pointer.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint cap(IArray array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the maximum length the <paramref name="slice"/> can reach when resliced.
    /// </summary>
    /// <param name="slice">Target slice pointer.</param>
    /// <returns>The capacity of the <paramref name="slice"/>.</returns>
    public static nint cap<T>(in slice<T> slice)
    {
        return slice.Capacity;
    }

    /// <summary>
    /// Gets the maximum length the <paramref name="slice"/> can reach when resliced.
    /// </summary>
    /// <param name="slice">Target slice pointer.</param>
    /// <returns>The capacity of the <paramref name="slice"/>.</returns>
    public static nint cap(ISlice slice)
    {
        return slice.Capacity;
    }

    /// <summary>
    /// Gets the maximum capacity of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The capacity of the <paramref name="channel"/>.</returns>
    public static nint cap<T>(in channel<T> channel)
    {
        return channel.Capacity;
    }

    /// <summary>
    /// Gets the maximum capacity of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The capacity of the <paramref name="channel"/>.</returns>
    public static nint cap(IChannel channel)
    {
        return channel.Capacity;
    }

    /// <summary>
    /// Closes the channel.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    public static void close<T>(in channel<T> channel)
    {
        // An "in" parameter works here because the close method operates on channel structure's
        // private class-based member references, not on value types
        // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
        channel.Close();
    }

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>Received value.</returns>
    /// <remarks>
    /// <para>
    /// If the channel is empty, method will block the current thread until a value is sent to the channel.
    /// </para>
    /// <para>
    /// Defines a Go style channel <see cref="channel{T}.Receive()"/>> operation.
    /// </para>
    /// </remarks>
    public static T ᐸꟷ<T>(channel<T> channel)
    {
        return channel.Receive();
    }

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <param name="channel">Target channel.</param>
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
    public static (T val, bool ok) ᐸꟷ<T>(channel<T> channel, bool _)
    {
        return channel.Receive(_);
    }

    /// <summary>
    /// Gets a wait handle that is set when data is ready to be received from the channel.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꓸꓸꓸ"/>.</param>
    /// <returns>Wait handle that is set when data is ready to be received from the channel.</returns>
    /// <remarks>
    /// Defines a Go style channel <see cref="channel{T}.Receiving"/> wait handle.
    /// </remarks>
    public static WaitHandle ᐸꟷ<T>(channel<T> channel, NilType _)
    {
        return channel.Receiving;
    }

    /// <summary>
    /// Waits for any of the specified <paramref name="handles"/> to be set.
    /// </summary>
    /// <param name="handles">Handles to wait for.</param>
    /// <returns>Index of the handle that satisfied the wait condition.</returns>
    public static int select(params WaitHandle[] handles)
    {
        if (handles.Length == 0)
            fatal(FatalError.DeadLock());

        return WaitHandle.WaitAny(handles);
    }

    /// <summary>
    /// Enumerates over a range of integers.
    /// </summary>
    /// <param name="n">Number of integers to enumerate.</param>
    /// <returns>Enumerable range of integers.</returns>
    public static IEnumerable<int> range(nint n)
    {
        return Enumerable.Range(0, (int)n);
    }

    /// <summary>
    /// Enumerates over a yield function.
    /// </summary>
    /// <param name="enumerator">Yield function.</param>
    /// <returns>Enumerable range of values.</returns>
    public static IEnumerable<object> range(Action<Func<bool>> enumerator)
    {
        return range<object>(e =>
        {
            // ReSharper disable once UnusedParameter.Local
            bool yielder(object _) => e(null!);
            enumerator(() => yielder(null!));
        });
    }

    /// <summary>
    /// Enumerates over a yield function.
    /// </summary>
    /// <typeparam name="T">Type of enumeration.</typeparam>
    /// <param name="enumerator">Yield function.</param>
    /// <returns>Enumerable range of values.</returns>
    public static IEnumerable<T> range<T>(Action<Func<T, bool>> enumerator)
    {
        return new YieldFunctionEnumerable<T>(enumerator);
    }

    /// <summary>
    /// Constructs a complex value from two floating-point values.
    /// </summary>
    /// <param name="realPart">Real-part of complex value.</param>
    /// <param name="imaginaryPart">Imaginary-part of complex value.</param>
    /// <returns>New complex value from specified <paramref name="realPart"/> and <paramref name="imaginaryPart"/>.</returns>
    public static complex64 complex(float32 realPart, float32 imaginaryPart)
    {
        return new complex64(realPart, imaginaryPart);
    }

    /// <summary>
    /// Constructs a complex value from two floating-point values.
    /// </summary>
    /// <param name="realPart">Real-part of complex value.</param>
    /// <param name="imaginaryPart">Imaginary-part of complex value.</param>
    /// <returns>New complex value from specified <paramref name="realPart"/> and <paramref name="imaginaryPart"/>.</returns>
    public static complex128 complex(float64 realPart, float64 imaginaryPart)
    {
        return new complex128(realPart, imaginaryPart);
    }

    /// <summary>
    /// Copies elements from a source slice into a destination slice.
    /// The source and destination may overlap.
    /// </summary>
    /// <param name="dst">Destination slice pointer.</param>
    /// <param name="src">Source slice pointer.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    public static nint copy<T1, T2>(in slice<T1> dst, in slice<T2> src)
    {
        if (dst == nil)
            throw new InvalidOperationException("Destination slice array reference is null.");

        if (src == nil)
            throw new InvalidOperationException("Source slice array reference is null.");

        nint min = Min(dst.Length, src.Length);

        if (min > 0)
        {
            if (typeof(T1).IsAssignableFrom(typeof(T2)))
            {
                Array.Copy(src.m_array, src.Low, dst.m_array, dst.Low, min);
            }
            else
            {
                for (nint i = 0; i < min; i++)
                    dst[dst.Low + i] = (T1)TypeExtensions.ConvertToType((IConvertible)src[src.Low + i]!);
            }
        }

        return min;
    }

    /// <summary>
    /// Copies elements from a source slice into a destination slice.
    /// The source and destination may overlap.
    /// </summary>
    /// <param name="dst">Destination slice pointer.</param>
    /// <param name="src">Source slice.</param>
    /// <returns>
    /// The number of elements copied, which will be the minimum of len(src) and len(dst).
    /// </returns>
    /// <remarks>
    /// As a special case, it also will copy bytes from a string to a slice of bytes.
    /// </remarks>
    public static nint copy(in slice<byte> dst, in @string src)
    {
        slice<byte> bytes = src;
        return copy(dst, bytes);
    }

    /// <summary>
    /// Deletes the element with the specified key from the map. If m is nil or there is no such element, delete is a no-op.
    /// </summary>
    /// <param name="map">Target map.</param>
    /// <param name="key">Key to remove.</param>
    public static void delete<TKey, TValue>(map<TKey, TValue> map, TKey key) where TKey : notnull
    {
        map.Remove(key);
    }

    /// <summary>
    /// Gets the imaginary part of the complex number <paramref name="c"/>.
    /// </summary>
    /// <param name="c">Complex number.</param>
    /// <returns>Imaginary part of the complex number <paramref name="c"/>.</returns>
    public static float imag(complex64 c)
    {
        return c.Imaginary;
    }

    /// <summary>
    /// Gets the imaginary part of the complex number <paramref name="c"/>.
    /// </summary>
    /// <param name="c">Complex number.</param>
    /// <returns>Imaginary part of the complex number <paramref name="c"/>.</returns>
    public static double imag(complex128 c)
    {
        return c.Imaginary;
    }

    /// <summary>
    /// Gets the real part of the complex number <paramref name="c"/>.
    /// </summary>
    /// <param name="c">Complex number.</param>
    /// <returns>Real part of the complex number <paramref name="c"/>.</returns>
    public static float real(complex64 c)
    {
        return c.Real;
    }

    /// <summary>
    /// Gets the real part of the complex number <paramref name="c"/>.
    /// </summary>
    /// <param name="c">Complex number.</param>
    /// <returns>Real part of the complex number <paramref name="c"/>.</returns>
    public static double real(complex128 c)
    {
        return c.Real;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/>.
    /// </summary>
    /// <param name="array">Target array.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint len<T>(in array<T> array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/>.
    /// </summary>
    /// <param name="array">Target array.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint len<T>(T[] array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/>.
    /// </summary>
    /// <param name="array">Target array.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint len(IArray array)
    {
        return array.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="array"/>.
    /// </summary>
    /// <param name="array">Target array pointer.</param>
    /// <returns>The length of the <paramref name="array"/>.</returns>
    public static nint len<T>(in ж<array<T>> array)
    {
        return array.val.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="slice"/>.
    /// </summary>
    /// <param name="slice">Target slice.</param>
    /// <returns>The length of the <paramref name="slice"/>.</returns>
    public static nint len<T>(in slice<T> slice)
    {
        return slice.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="slice"/>.
    /// </summary>
    /// <param name="slice">Target slice.</param>
    /// <returns>The length of the <paramref name="slice"/>.</returns>
    public static nint len(ISlice slice)
    {
        return slice.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="slice"/>.
    /// </summary>
    /// <param name="slice">Target slice pointer.</param>
    /// <returns>The length of the <paramref name="slice"/>.</returns>
    public static nint len<T>(in ж<slice<T>> slice)
    {
        return slice.val.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="str"/>.
    /// </summary>
    /// <param name="str">Target string.</param>
    /// <returns>The length of the <paramref name="str"/>.</returns>
    public static nint len(in @string str)
    {
        return str.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="str"/>.
    /// </summary>
    /// <param name="str">Target string.</param>
    /// <returns>The length of the <paramref name="str"/>.</returns>
    public static nint len(in ReadOnlySpan<byte> str)
    {
        return str.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="str"/>.
    /// </summary>
    /// <param name="str">Target string pointer.</param>
    /// <returns>The length of the <paramref name="str"/>.</returns>
    public static nint len(in ж<@string> str)
    {
        return str.val.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="str"/>.
    /// </summary>
    /// <param name="str">Target channel.</param>
    /// <returns>The length of the <paramref name="str"/>.</returns>
    public static nint len(string str)
    {
        return str.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="map"/>.
    /// </summary>
    /// <param name="map">Target map.</param>
    /// <returns>The length of the <paramref name="map"/>.</returns>
    public static nint len<TKey, TValue>(in map<TKey, TValue> map) where TKey : notnull
    {
        return map.Count;
    }

    /// <summary>
    /// Gets the length of the <paramref name="map"/>.
    /// </summary>
    /// <param name="map">Target map.</param>
    /// <returns>The length of the <paramref name="map"/>.</returns>
    public static nint len(IMap map)
    {
        return map.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="map"/>.
    /// </summary>
    /// <param name="map">Target map pointer.</param>
    /// <returns>The length of the <paramref name="map"/>.</returns>
    public static nint len<TKey, TValue>(in ж<map<TKey, TValue>> map) where TKey : notnull
    {
        return map.val.Count;
    }

    /// <summary>
    /// Gets the length of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The length of the <paramref name="channel"/>.</returns>
    public static nint len<T>(in channel<T> channel)
    {
        return channel.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The length of the <paramref name="channel"/>.</returns>
    public static nint len(IChannel channel)
    {
        return channel.Length;
    }

    /// <summary>
    /// Gets the length of the <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <returns>The length of the <paramref name="channel"/>.</returns>
    public static nint len<T>(in ж<channel<T>> channel)
    {
        return channel.val.Length;
    }

    /// <summary>
    /// Allocates and initializes a new object for known types, e.g., <see cref="ISlice{T}"/>,
    /// <see cref="IMap{TKey, TValue}"/>, and <see cref="IChannel"/>.
    /// </summary>
    /// <param name="p1">First integer parameter, commonly for size.</param>
    /// <param name="p2">Second integer parameter, commonly for capacity.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <returns>New object.</returns>
    public static T make<T>(nint p1 = 0, nint p2 = -1) where T : ISupportMake<T>, new()
    {
        if (p1 == 0 && p2 == -1)
            return new T();

        return T.Make(p1, p2);
    }

    /// <summary>
    /// Allocates and initializes a new object.
    /// </summary>
    /// <param name="p1">First integer parameter, commonly for size.</param>
    /// <param name="p2">Second integer parameter, commonly for capacity.</param>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <returns>New object.</returns>
    public static T makeǃ<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(nint p1 = 0, nint p2 = -1) where T : new()
    {
        if (p1 == 0 && p2 == -1)
            return new T();

        if (p2 != -1)
            return (T)Activator.CreateInstance(typeof(T), p1, p2)!;

        return (T)Activator.CreateInstance(typeof(T), p1)!;
    }

    /// <summary>
    /// Gets a reference to a zero value instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <returns>Reference to a zero value instance of type <typeparamref name="T"/>.</returns>
    public static ref T zero<T>()
    {
        return ref Zero<T>.Default;
    }

    /// <summary>
    /// Gets dereferenced value that <paramref name="source"/> points to.
    /// </summary>
    /// <typeparam name="T">Source type of reference.</typeparam>
    /// <param name="source">Source pointer.</param>
    /// <returns>Dereferenced value that <paramref name="source"/> points to.</returns>
    /// <remarks>
    /// This is equivalent to <c>~source</c> and <c>source.val</c>.
    /// </remarks>
    public static T ж<T>(in ж<T> source)
    {
        return source.val;
    }

    /// <summary>
    /// Gets a pointer to a new heap allocated copy of <paramref name="target"/> value.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target value.</param>
    /// <returns>Pointer to heap allocated copy of <paramref name="target"/> value.</returns>
    public static ж<T> Ꮡ<T>(in T target)
    {
        return new ж<T>(target);
    }

    /// <summary>
    /// Gets a pointer to slice or array element at <paramref name="index"/>.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target value.</param>
    /// <param name="index">Index of element.</param>
    /// <returns>Pointer to slice or array element at <paramref name="index"/>.</returns>
    public static ж<T> Ꮡ<T>(in IArray<T> target, int index)
    {
        return new ж<T>(target, index);
    }

    /// <summary>
    /// Creates a new heap allocated instance of the zero value for type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="pointer">Out reference to pointer to heap allocated zero value.</param>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <returns>Reference to heap allocated instance of the zero value for type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// This is a convenience function to allow default local struct ref and <see cref="ж{T}"/>
    /// to be created in a single call, e.g.:
    /// <code language="cs">
    ///     ref var v = ref heap(out ж&lt;Vertex&gt; Ꮡv);
    /// </code>
    /// </remarks>
    public static ref T heap<T>(out ж<T> pointer)
    {
        pointer = Ꮡ<T>(default!);
        return ref pointer.val;
    }

    /// <summary>
    /// Creates a new heap allocated copy of existing <paramref name="target"/> value.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="target">Target value.</param>
    /// <param name="pointer">Out reference to pointer to heap allocated copy of <paramref name="target"/> value.</param>
    /// <returns>Reference to heap allocated copy of <paramref name="target"/> value.</returns>
    /// <remarks>
    /// This is a convenience function to allow local struct ref and <see cref="ж{T}"/>
    /// to be created in a single call, e.g.:
    /// <code language="cs">
    ///     ref var v = ref heap(new Vertex(40.68433, -74.39967), out var Ꮡv);
    /// </code>
    /// </remarks>
    public static ref T heap<T>(in T target, out ж<T> pointer)
    {
        pointer = Ꮡ(target);
        return ref pointer.val;
    }

    /// <summary>
    /// Creates a heap allocated pointer reference to a new zero value instance of type.
    /// </summary>
    /// <returns>Pointer to heap allocated zero value of provided type.</returns>
    public static ж<T> @new<T>() where T : new()
    {
        return new ж<T>(new T());
    }

    /// <summary>
    /// Creates a new reference for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Target type of reference.</typeparam>
    /// <param name="inputs">Constructor parameters.</param>
    /// <returns>New reference for <typeparamref name="T"/>.</returns>
    public static ж<T> @new<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(params object[] inputs)
    {
        return new ж<T>((T)Activator.CreateInstance(typeof(T), inputs)!);
    }

    /// <summary>
    /// Formats arguments in an implementation-specific way and writes the result to standard-error.
    /// </summary>
    /// <param name="args">Arguments to display.</param>
    public static void print(params object[] args)
    {
        Console.Error.Write(string.Join(" ", args.Select(arg => arg.ToString())));
    }

    /// <summary>
    /// Formats arguments in an implementation-specific way and writes the result to standard-error along with a new line.
    /// </summary>
    /// <param name="args">Arguments to display.</param>
    public static void println(params object[] args)
    {
        Console.Error.WriteLine(string.Join(" ", args.Select(arg => arg.ToString())));
    }

    /// <summary>
    /// Exits application with a fatal error.
    /// </summary>
    /// <param name="message">Fatal error message.</param>
    /// <param name="code">Application exit code.</param>
    public static void fatal(string message, nint code = 1)
    {
        if (!string.IsNullOrEmpty(message))
            message = $"fatal error: {message}";

    #if DEBUG
        throw new InvalidOperationException($"{message} [{code}]");
    #else
        Console.Error.WriteLine(message);
        Environment.Exit((int)code);
    #endif
    }

    // ** Type Assertion Functions **

    /// <summary>
    /// Type asserts <paramref name="target"/> object as type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Desired type for <paramref name="target"/>.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <returns><paramref name="target"/> value cast as <typeparamref name="T"/>, if successful.</returns>
    public static T _<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(this object target)
    {
        try
        {
            switch (target)
            {
                case string str when typeof(T) == typeof(@string):
                    return (T)(object)new @string(str);
                case T typedTarget:
                    return typedTarget;
            }

            if (!typeof(T).IsInterface || !Implements<T>(target))
                return (T)target;

            // Handle conversion of anonymous dynamically declared interfaces - unfortunately, you can't
            // define an interface that describes an abstract method implemented by another interface,
            // so we are forced to use reflection to find the static interface conversion method...
            MethodInfo? method = typeof(T).GetMethod("As", 1, BindingFlags.Public | BindingFlags.Static, s_asTParams);

            // Ths following exception will not be captured by type assertion overload that returns a tuple
            // that includes a "success" boolean since missing method is considered a code conversion error
            if (method == null)
                throw new InvalidOperationException($"Interface '{typeof(T).Name}' does not implement 'As' runtime conversion method.");

        #pragma warning disable IL2060
            MethodInfo genericMethod = method.MakeGenericMethod(target.GetType());
        #pragma warning restore IL2060

            return (T)genericMethod.Invoke(null, [target])!;
        }
        catch (InvalidCastException ex)
        {
            throw new PanicException($"interface conversion: interface {{}} is {GetGoTypeName(target.GetType())}, not {GetGoTypeName(typeof(T))}", ex);
        }
    }

    /// <summary>
    /// Attempts type assert of <paramref name="target"/> object as type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Desired type for <paramref name="target"/>.</typeparam>
    /// <param name="target">Source value to type assert.</param>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꟷ"/>.</param>
    /// <returns>Tuple of <paramref name="target"/> value cast as <typeparamref name="T"/> and success boolean.</returns>
    public static (T, bool) _<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(this object target, bool _)
    {
        try
        {
            return (target._<T>(), true);
        }
        catch (PanicException)
        {
            return (default, false)!;
        }
    }

    /// <summary>
    /// Gets common Go type for given <paramref name="target"/>.
    /// </summary>
    /// <param name="target">Target value.</param>
    /// <returns>Common Go type for given <paramref name="target"/>.</returns>
    public static object type(this object target)
    {
        // Infer common go type as needed
        return target switch
        {
            string str => new @string(str),
            _ => target
        };
    }

    // ** Conversion Functions **

    /// <summary>
    /// Converts C# <paramref name="source"/> array to Go <see cref="go.slice{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of array.</typeparam>
    /// <param name="source">C# source array.</param>
    /// <returns>Go <see cref="go.slice{T}"/> wrapper for C# <paramref name="source"/> array.</returns>
    public static slice<T> slice<T>(T[] source)
    {
        return source;
    }

    /// <summary>
    /// Converts value to a complex64 imaginary number.
    /// </summary>
    /// <param name="imaginary">Value to convert to imaginary.</param>
    /// <returns>New complex number with specified <paramref name="imaginary"/> part and a zero value real part.</returns>
    public static complex64 i(float imaginary)
    {
        return new complex64(0.0F, imaginary);
    }

    /// <summary>
    /// Converts value to a complex128 imaginary number.
    /// </summary>
    /// <param name="imaginary">Value to convert to imaginary.</param>
    /// <returns>New complex number with specified <paramref name="imaginary"/> part and a zero value real part.</returns>
    public static complex128 i(double imaginary)
    {
        return new complex128(0.0D, imaginary);
    }

    /*
    
    /// <summary>
    /// Creates a new Go <see cref="go.array{T}"/> with specified <paramref name="length"/>.
    /// </summary>
    /// <typeparam name="T">Type of array.</typeparam>
    /// <param name="length">Target array length.</param>
    /// <returns>Go <see cref="go.array{T}"/> with specified <paramref name="length"/>.</returns>
    public static array<T> array<T>(nint length)
    {
        return new array<T>(length);
    }

    /// <summary>
    /// Converts C# <paramref name="source"/> array to Go <see cref="go.array{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of array.</typeparam>
    /// <param name="source">C# source array.</param>
    /// <returns>Go <see cref="go.array{T}"/> wrapper for C# <paramref name="source"/> array.</returns>
    public static array<T> array<T>(T[] source)
    {
        return source;
    }

    /// <summary>
    /// Converts Go <param name="source"/> string to Go slice of bytes.
    /// </summary>
    /// <param name="source">C# source string.</param>
    /// <returns>Slice of bytes from Go string</returns>
    public static slice<byte> slice(@string source)
    {
        return source.Slice(0, len(source));
    }

    /// <summary>
    /// Converts C# <paramref name="source"/> string array to Go <see cref="go.slice{@string}"/>.
    /// </summary>
    /// <param name="source">C# source array.</param>
    /// <returns>Go <see cref="go.slice{@string}"/> wrapper for C# <paramref name="source"/> string array.</returns>
    public static slice<@string> slice(IReadOnlyCollection<string> source)
    {
        return @string(source);
    }

    /// <summary>
    /// Converts C# <see cref="string"/> array into Go <see cref="go.@string"/> array.
    /// </summary>
    /// <param name="source">C# <see cref="string"/> array</param>
    /// <returns>Go <see cref="go.@string"/> array from C# <see cref="string"/> array <paramref name="source"/>.</returns>
    public static @string[] @string(IReadOnlyCollection<string> source)
    {
        return source.Select(value => new @string(value)).ToArray();
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a <see cref="byte"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a <see cref="byte"/>.</returns>
    public static byte @byte(byte value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a <see cref="byte"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a <see cref="byte"/>.</returns>
    public static byte @byte(object value)
    {
        return (byte)Convert.ChangeType(value, TypeCode.Byte);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a rune.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a rune.</returns>
    public static rune rune(int32 value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a rune.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a rune.</returns>
    public static rune rune(object value)
    {
        return rune((int32)Convert.ChangeType(value, TypeCode.Int32));
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint8.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint8.</returns>
    public static uint8 uint8(byte value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint8.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint8.</returns>
    public static uint8 uint8(object value)
    {
        return (byte)Convert.ChangeType(value, TypeCode.Byte);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint16.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint16.</returns>
    public static uint16 uint16(ushort value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint16.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint16.</returns>
    public static uint16 uint16(object value)
    {
        return (ushort)Convert.ChangeType(value, TypeCode.UInt16);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint32.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint32.</returns>
    public static uint32 uint32(uint value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint32.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint32.</returns>
    public static uint32 uint32(object value)
    {
        return (uint)Convert.ChangeType(value, TypeCode.UInt32);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint64.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint64.</returns>
    public static uint64 uint64(ulong value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint64.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint64.</returns>
    public static uint64 uint64(object value)
    {
        return (ulong)Convert.ChangeType(value, TypeCode.UInt64);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int8.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int8.</returns>
    public static int8 int8(sbyte value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int8.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int8.</returns>
    public static int8 int8(object value)
    {
        return (sbyte)Convert.ChangeType(value, TypeCode.SByte);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int16.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int16.</returns>
    public static int16 int16(short value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int16.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int16.</returns>
    public static int16 int16(object value)
    {
        return (short)Convert.ChangeType(value, TypeCode.Int16);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int32.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int32.</returns>
    public static int32 int32(int value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int32.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int32.</returns>
    public static int32 int32(nint value)
    {
        return (int32)value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int32.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int32.</returns>
    public static int32 int32(long value)
    {
        return (int32)value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int32.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int32.</returns>
    public static int32 int32(object value)
    {
        return (int)Convert.ChangeType(value, TypeCode.Int32);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int64.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int64.</returns>
    public static int64 int64(int value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int64.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int64.</returns>
    public static int64 int64(nint value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int64.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int64.</returns>
    public static int64 int64(long value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int64.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int64.</returns>
    public static int64 int64(object value)
    {
        return (int64)Convert.ChangeType(value, TypeCode.Int64);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a float32.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a float32.</returns>
    public static float32 float32(float value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a float32.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a float32.</returns>
    public static float32 float32(object value)
    {
        return (float)Convert.ChangeType(value, TypeCode.Single);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a float64.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a float64.</returns>
    public static float64 float64(double value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a float64.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a float64.</returns>
    public static float64 float64(object value)
    {
        return (double)Convert.ChangeType(value, TypeCode.Double);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a <see cref="go.complex64"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a <see cref="go.complex64"/>.</returns>
    public static complex64 complex64(object value)
    {
        if (value is complex128 dcomplex)
            return (complex64)dcomplex;

        if (value is not complex64 fcomplex)
            return (float)Convert.ChangeType(value, TypeCode.Single);

        return fcomplex;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a complex128.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a complex128.</returns>
    public static complex128 complex128(Complex value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a complex128.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a complex128.</returns>
    public static complex128 complex128(object value)
    {
        if (value is complex64 fcomplex)
            return fcomplex;

        if (value is not complex128 dcomplex)
            return (double)Convert.ChangeType(value, TypeCode.Double);

        return dcomplex;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint.</returns>
    public static ulong @uint(uint value)
    {
        return (ulong)Convert.ChangeType(value, TypeCode.UInt64);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint.</returns>
    public static ulong @uint(ulong value)
    {
        return (ulong)Convert.ChangeType(value, TypeCode.UInt64);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uint.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uint.</returns>
    public static ulong @uint(object value)
    {
        return (ulong)Convert.ChangeType(value, TypeCode.UInt64);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int.</returns>
    public static nint @int(int value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int.</returns>
    public static nint @int(nint value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an int.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an int.</returns>
    public static nint @int(object value)
    {
        return (nint)Convert.ChangeType(value, TypeCode.Int64);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uintptr.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uintptr.</returns>
    public static uintptr uintptr(UIntPtr value)
    {
        return value;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to an uintptr.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to an uintptr.</returns>
    public static uintptr uintptr(object value)
    {
        return (uintptr)Convert.ChangeType(value, TypeCode.UInt64);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a <see cref="@string"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a <see cref="@string"/>.</returns>
    public static @string @string(string value)
    {
        return new @string(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a <see cref="@string"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a <see cref="@string"/>.</returns>
    public static @string @string(ReadOnlySpan<byte> value)
    {
        return new @string(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a <see cref="@string"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a <see cref="@string"/>.</returns>
    public static @string @string(Span<rune> value)
    {
        return new @string(value);
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a <see cref="@string"/>.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><paramref name="value"/> converted to a <see cref="@string"/>.</returns>
    public static @string @string(object? value)
    {
        // Only reference types can be null, therefore "" is its default value
        if (value is null)
            return "";

        Type itemType = value.GetType();

        if (!itemType.IsValueType)
            return itemType.ToString();

        // Handle common types
        bool isIntValue = false;
        ulong intValue = 0UL;

        if (value is IConvertible convertible)
        {
            switch (convertible.GetTypeCode())
            {
                case TypeCode.Char:
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    intValue = (ulong)Convert.ChangeType(value, TypeCode.UInt64);
                    isIntValue = true;
                    break;
            }
        }

        if (isIntValue)
        {
            char charValue = '\uFFFD';

            if (intValue <= char.MaxValue)
                charValue = (char)intValue;

            return new string(charValue, 1);
        }

        if (itemType == typeof(array<byte>) || itemType == typeof(byte[]))
            return new @string((byte[])value);

        if (itemType == typeof(slice<byte>))
            return new @string((slice<byte>)value);

        if (itemType == typeof(array<byte>) || itemType == typeof(byte[]))
            return new @string((byte[])value);

        if (itemType == typeof(slice<byte>))
            return new @string((slice<byte>)value);

        if (itemType == typeof(array<char>) || itemType == typeof(char[]))
            return new @string((char[])value);

        if (itemType == typeof(slice<char>))
            return new @string((slice<char>)value);

        if (itemType == typeof(array<rune>) || itemType == typeof(rune[]))
            return new @string(new Span<rune>((rune[])value));

        if (itemType == typeof(slice<rune>))
            return new @string((slice<rune>)value);

        // Handle custom value types
        return value.ToString()!;
    }

    */

    /// <summary>
    /// Compares two objects for equality.
    /// </summary>
    /// <param name="left">Left object.</param>
    /// <param name="right">Right object.</param>
    /// <returns><c>true</c> if both objects are equal; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// When both objects being compared are interface references, this method will match Go's behavior for comparing
    /// interfaces , i.e., two interface values are considered equal if they have identical dynamic types and equal
    /// dynamic values or if both have value nil. Since the type being referenced by the interface is not known at
    /// compile time, this method uses reflection to get the equality operator for the type and calls it if available.
    /// If the equality operator is not found, the default <see cref="object.Equals(object, object)"/> method is used.
    /// </remarks>
    public static bool AreEqual(object? left, object? right)
    {
        // Check if both are null
        if (left is null && right is null)
            return true;

        // Check if one is null
        if (left is null || right is null)
            return false;

        Type leftType = left.GetType();

        // Check if both are the same type
        if (leftType != right.GetType())
            return false;

        // Get equality "==" operator for type using reflection,
        // lookup is cached for performance.
        MethodInfo? equalityOperator = leftType.GetEqualityOperator();

        // If equality operator is not found, use default object.Equals
        if (equalityOperator is null)
            return left.Equals(right);

        // Call equality operator
        return (bool)equalityOperator.Invoke(null, [left, right])!;
    }

    /// <summary>
    /// Checks if the specified <paramref name="value"/> implements the specified interface <typeparamref name="TInterface"/>.
    /// </summary>
    /// <typeparam name="TInterface">Interface type to check.</typeparam>
    /// <param name="value">Object to check for interface implementation.</param>
    /// <returns><c>true</c> if the specified <paramref name="value"/> implements the specified interface <typeparamref name="TInterface"/>; otherwise, <c>false</c>.</returns>
    public static bool Implements<TInterface>(object? value)
    {
        return value switch
        {
            TInterface => true,
            null => false,
            _ => implementsInterface()
        };

        // Fall back on run-time check. This may be encountered for dynamically defined interface
        // types or when a function is checking a private library interface internally. All type
        // results are cached for performance, but there is an initial cost for lookup creation.
        bool implementsInterface()
        {
            return s_implementsInterface.GetOrAdd((value.GetType(), typeof(TInterface)), entry =>
            {
                (Type valueType, Type interfaceType) = entry;
                ImmutableHashSet<string> interfaceMethodNames = interfaceType.GetInterfaceMethodNames();

                // All types implement an empty interface
                if (interfaceMethodNames.Count == 0)
                    return true;

                ImmutableHashSet<string> typeExtensionMethodNames = valueType.GetExtensionMethodNames();

                return interfaceMethodNames.Except(typeExtensionMethodNames).Count == 0;
            });
        }
    }

    /// <summary>
    /// Gets the common Go type name for the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to evaluate.</param>
    /// <returns>Common Go type name for the specified <paramref name="value"/>.</returns>
    public static string GetGoTypeName(object? value)
    {
        return GetGoTypeName(value?.GetType());
    }

    /// <summary>
    /// Gets the common Go type name for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Target type</param>
    /// <returns>Common Go type name for the specified <paramref name="type"/>.</returns>
    public static string GetGoTypeName(Type? type)
    {
        if (type is null)
            return "nil";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.String => "string",
            TypeCode.Char => "rune",
            TypeCode.Boolean => "bool",
            TypeCode.SByte => "int8",
            TypeCode.Int16 => "int16",
            TypeCode.Int32 => "int",
            TypeCode.Int64 => "int64",
            TypeCode.Byte => "byte",
            TypeCode.UInt16 => "uint16",
            TypeCode.UInt32 => "uint32",
            TypeCode.UInt64 => "uint64",
            TypeCode.Single => "float32",
            TypeCode.Double => "float64",
            _ => handleDefault()
        };

        string handleDefault()
        {
            string typeName = type.FullName ?? type.Name;

            return typeName switch
            {
                "go.string" => "string",
                "System.IntPtr" => "int",
                "System.UIntPtr" => "uintptr",
                "System.Numerics.Complex" => "complex128",
                "go.complex64" => "complex64",
                _ => type == typeof(object) ? "interface {}" : typeName
            };
        }
    }

    /// <summary>
    /// Gets the common Go type name for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Target type.</typeparam>
    /// <returns>Common Go type name for the specified type <typeparamref name="T"/>.</returns>
    public static string GetGoTypeName<T>()
    {
        return GetGoTypeName(typeof(T));
    }

    /*

    /// <summary>
    /// Copies length elements from <paramref name="source"/> array, starting at <paramref name="sourceIndex"/>,
    /// to <paramref name="dest"/> array, starting at <paramref name="destinationIndex"/> where each element is
    /// casted from <typeparamref name="TSource"/> to <typeparamref name="TDest"/> using the specified
    /// <paramref name="cast"/> function.
    /// </summary>
    /// <param name="source">Source array.</param>
    /// <param name="sourceIndex">Source array index.</param>
    /// <param name="dest">Destination array.</param>
    /// <param name="destinationIndex">Destination array index.</param>
    /// <param name="cast">Cast function.</param>
    /// <param name="length">Number of bytes to copy.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDest">Destination type.</typeparam>
    public static void CastCopy<TSource, TDest>(this TSource[] source, nint sourceIndex, TDest[] dest, nint destinationIndex, Func<TSource, TDest> cast, nint length)
    {
        for (nint i = 0; i < length; i++)
            dest[destinationIndex + i] = cast(source[sourceIndex + i]);
    }

    /// <summary>
    /// Copies length elements from <paramref name="source"/> array, starting at index 0,
    /// to <paramref name="dest"/> array, starting at index 0 where each element is
    /// casted from <typeparamref name="TSource"/> to <typeparamref name="TDest"/> using
    /// the specified <paramref name="cast"/> function.
    /// </summary>
    /// <param name="source">Source array.</param>
    /// <param name="dest">Destination array.</param>
    /// <param name="cast">Cast function.</param>
    /// <param name="length">Number of bytes to copy, defaults to <paramref name="source"/> array length.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDest">Destination type.</typeparam>
    public static void CastCopy<TSource, TDest>(this TSource[] source, TDest[] dest, Func<TSource, TDest> cast, nint length = -1)
    {
        if (length == -1)
            length = source.Length;

        source.CastCopy(0, dest, 0, cast, length);
    }

    /// <summary>
    /// Copies <paramref name="source"/> array casting each element from <typeparamref name="TSource"/>
    /// to <typeparamref name="TDest"/> using the specified <paramref name="cast"/> function.
    /// </summary>
    /// <param name="source">Source array.</param>
    /// <param name="cast">Cast function.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDest">Destination type.</typeparam>
    public static TDest[] CastCopy<TSource, TDest>(this TSource[] source, Func<TSource, TDest> cast)
    {
        TDest[] dest = new TDest[source.Length];
        source.CastCopy(dest, cast);
        return dest;
    }

    /// <summary>
    /// Converts imaginary literal value to a complex128 imaginary number.
    /// </summary>
    /// <param name="literal">Literal imaginary value with "i" suffix.</param>
    /// <returns>New complex number with parsed <paramref name="literal"/> as imaginary part and a zero value real part.</returns>
    public static complex128 i(string literal)
    {
        if (!literal.EndsWith("i"))
            throw new InvalidCastException($"Token \"{literal}\" is not an imaginary literal.");

        if (double.TryParse(literal[..^1], out double imaginary))
            return i(imaginary);

        throw new InvalidCastException($"Could not parse \"{literal}\" as an imaginary value.");
    }

    */

#if EXPERIMENTAL

    // When using stack allocated strings, you need a function to convert the stack string to a heap string
    // any time you need to hold the string in a heap allocated object:

    /// <summary>
    /// Converts UTF8 byte array to heap allocated <see cref="@string"/>.
    /// </summary>
    /// <param name="value">UTF8 byte array.</param>
    /// <returns>Heap allocated string.</returns>
    public static @string str(ReadOnlySpan<byte> value) => (@string)value;
    
    /// <summary>
    /// Converts stack allocated <see cref="sstring"/> to heap allocated <see cref="@string"/>.
    /// </summary>
    /// <param name="value">Stack allocated string.</param>
    /// <returns>Heap allocated string.</returns>
    public static @string str(sstring value) => value;

#endif

    /// <summary>
    /// Execute Go routine.
    /// </summary>
    /// <param name="action">Routine to execute.</param>
    // The following is basically "go!". We use the unicode bang-type character
    // as a valid C# identifier symbol, where the standard "!" is not. This is
    // to disambiguate the method name from the namespace.
    public static void goǃ(WaitCallback action)
    {
        ThreadPool.QueueUserWorkItem(action);
    }

    /// <summary>
    /// Execute Go routine.
    /// </summary>
    /// <param name="action">Routine to execute.</param>
    // The following is basically "go!". We use the unicode bang-type character
    // as a valid C# identifier symbol, where the standard "!" is not. This is
    // to disambiguate the method name from the namespace.
    public static void goǃ(Action action)
    {
        ThreadPool.QueueUserWorkItem(_ => action());
    }

    // ** Go Routine Handlers with Parameters **

    /// <summary>
    /// Executes a Go routine with one parameter.
    /// </summary>
    /// <typeparam name="T">First parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg">First parameter.</param>
    public static void goǃ<T>(Action<T> action, T arg)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg));
    }

    #region [ goǃ<T1, T2, ... T16> Implementations ]

    /// <summary>
    /// Executes a Go routine with two parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    public static void goǃ<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2));
    }

    /// <summary>
    /// Executes a Go routine with three parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    public static void goǃ<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3));
    }

    /// <summary>
    /// Executes a Go routine with four parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    public static void goǃ<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4));
    }

    /// <summary>
    /// Executes a Go routine with five parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5));
    }

    /// <summary>
    /// Executes a Go routine with six parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6));
    }

    /// <summary>
    /// Executes a Go routine with seven parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
    }

    /// <summary>
    /// Executes a Go routine with eight parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
    }

    /// <summary>
    /// Executes a Go routine with nine parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
    }

    /// <summary>
    /// Executes a Go routine with ten parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
    }

    /// <summary>
    /// Executes a Go routine with eleven parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
    }

    /// <summary>
    /// Executes a Go routine with twelve parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
    }

    /// <summary>
    /// Executes a Go routine with thirteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
    }

    /// <summary>
    /// Executes a Go routine with fourteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <typeparam name="T14">Fourteenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="arg14">Fourteenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
    }

    /// <summary>
    /// Executes a Go routine with fifteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <typeparam name="T14">Fourteenth parameter type.</typeparam>
    /// <typeparam name="T15">Fifteenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="arg14">Fourteenth parameter.</param>
    /// <param name="arg15">Fifteenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
    }

    /// <summary>
    /// Executes a Go routine with sixteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <typeparam name="T14">Fourteenth parameter type.</typeparam>
    /// <typeparam name="T15">Fifteenth parameter type.</typeparam>
    /// <typeparam name="T16">Sixteenth parameter type.</typeparam>
    /// <param name="action">Target action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="arg14">Fourteenth parameter.</param>
    /// <param name="arg15">Fifteenth parameter.</param>
    /// <param name="arg16">Sixteenth parameter.</param>
    public static void goǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
    {
        ThreadPool.QueueUserWorkItem(_ => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
    }

    #endregion

    // ** Go defer Handlers with Parameters **

    /// <summary>
    /// Executes a deferred action with one parameter.
    /// </summary>
    /// <typeparam name="T">First parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg">First parameter.</param>
    /// <param name="defer">Source defer function.</param>
    // The following is basically "defer!". We use the unicode bang-type character
    // as a valid C# identifier symbol, where the standard "!" is not. This is
    // to disambiguate the method name with parameters from the Defer delegate
    // passed into the function execution context with a fixed name of 'defer'.
    public static void deferǃ<T>(Action<T> action, T arg, Defer defer)
    {
        defer(() => action(arg));
    }

    #region [ deferǃ<T1, T2, ... T16> Implementations ]

    /// <summary>
    /// Executes a deferred action with two parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">First parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2, Defer defer)
    {
        defer(() => action(arg1, arg2));
    }

    /// <summary>
    /// Executes a deferred action with three parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3));
    }

    /// <summary>
    /// Executes a deferred action with four parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4));
    }

    /// <summary>
    /// Executes a deferred action with five parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5));
    }

    /// <summary>
    /// Executes a deferred action with six parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6));
    }

    /// <summary>
    /// Executes a deferred action with seven parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
    }

    /// <summary>
    /// Executes a deferred action with eight parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
    }

    /// <summary>
    /// Executes a deferred action with nine parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
    }

    /// <summary>
    /// Executes a deferred action with ten parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
    }

    /// <summary>
    /// Executes a deferred action with eleven parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
    }

    /// <summary>
    /// Executes a deferred action with twelve parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
    }

    /// <summary>
    /// Executes a deferred action with thirteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
    }

    /// <summary>
    /// Executes a deferred action with fourteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <typeparam name="T14">Fourteenth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="arg14">Fourteenth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
    }

    /// <summary>
    /// Executes a deferred action with fifteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <typeparam name="T14">Fourteenth parameter type.</typeparam>
    /// <typeparam name="T15">Fifteenth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="arg14">Fourteenth parameter.</param>
    /// <param name="arg15">Fifteenth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
    }

    /// <summary>
    /// Executes a deferred action with sixteen parameters.
    /// </summary>
    /// <typeparam name="T1">First parameter type.</typeparam>
    /// <typeparam name="T2">Second parameter type.</typeparam>
    /// <typeparam name="T3">Third parameter type.</typeparam>
    /// <typeparam name="T4">Fourth parameter type.</typeparam>
    /// <typeparam name="T5">Fifth parameter type.</typeparam>
    /// <typeparam name="T6">Sixth parameter type.</typeparam>
    /// <typeparam name="T7">Seventh parameter type.</typeparam>
    /// <typeparam name="T8">Eighth parameter type.</typeparam>
    /// <typeparam name="T9">Ninth parameter type.</typeparam>
    /// <typeparam name="T10">Tenth parameter type.</typeparam>
    /// <typeparam name="T11">Eleventh parameter type.</typeparam>
    /// <typeparam name="T12">Twelfth parameter type.</typeparam>
    /// <typeparam name="T13">Thirteenth parameter type.</typeparam>
    /// <typeparam name="T14">Fourteenth parameter type.</typeparam>
    /// <typeparam name="T15">Fifteenth parameter type.</typeparam>
    /// <typeparam name="T16">Sixteenth parameter type.</typeparam>
    /// <param name="action">Target defer action.</param>
    /// <param name="arg1">First parameter.</param>
    /// <param name="arg2">Second parameter.</param>
    /// <param name="arg3">Third parameter.</param>
    /// <param name="arg4">Fourth parameter.</param>
    /// <param name="arg5">Fifth parameter.</param>
    /// <param name="arg6">Sixth parameter.</param>
    /// <param name="arg7">Seventh parameter.</param>
    /// <param name="arg8">Eighth parameter.</param>
    /// <param name="arg9">Ninth parameter.</param>
    /// <param name="arg10">Tenth parameter.</param>
    /// <param name="arg11">Eleventh parameter.</param>
    /// <param name="arg12">Twelfth parameter.</param>
    /// <param name="arg13">Thirteenth parameter.</param>
    /// <param name="arg14">Fourteenth parameter.</param>
    /// <param name="arg15">Fifteenth parameter.</param>
    /// <param name="arg16">Sixteenth parameter.</param>
    /// <param name="defer">Source defer function.</param>
    public static void deferǃ<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, Defer defer)
    {
        defer(() => action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
    }

    #endregion

    // ** Go Function Execution Context Handlers **/

    /// <summary>
    /// Executes a Go function with no return value.
    /// </summary>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func(GoFunc<object>.GoAction action)
    {
        new GoFunc<object>(action).Execute();
    }

    /// <summary>
    /// Executes a Go function with a return value.
    /// </summary>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<T>(GoFunc<T>.GoFunction function)
    {
        return new GoFunc<T>(function).Execute();
    }

    /// <summary>
    /// Executes a Go function with 1 reference parameter and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1>(ref TRef1 ref1, GoFunc<TRef1, object>.GoRefAction action)
    {
        new GoFunc<TRef1, object>(action).Execute(ref ref1);
    }

    /// <summary>
    /// Executes a Go function with 1 reference parameter and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, T>(ref TRef1 ref1, GoFunc<TRef1, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, T>(function).Execute(ref ref1);
    }

    #region [ func<TRef1, TRef2, ... TRef16> Implementations ]

    /*  The following code was generated using the "GenGoFuncRefInstances" utility: */

    /// <summary>
    /// Executes a Go function with 2 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2>(ref TRef1 ref1, ref TRef2 ref2, GoFunc<TRef1, TRef2, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, object>(action).Execute(ref ref1, ref ref2);
    }

    /// <summary>
    /// Executes a Go function with 2 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, T>(ref TRef1 ref1, ref TRef2 ref2, GoFunc<TRef1, TRef2, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, T>(function).Execute(ref ref1, ref ref2);
    }

    /// <summary>
    /// Executes a Go function with 3 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, GoFunc<TRef1, TRef2, TRef3, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, object>(action).Execute(ref ref1, ref ref2, ref ref3);
    }

    /// <summary>
    /// Executes a Go function with 3 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, GoFunc<TRef1, TRef2, TRef3, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, TRef3, T>(function).Execute(ref ref1, ref ref2, ref ref3);
    }

    /// <summary>
    /// Executes a Go function with 4 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, GoFunc<TRef1, TRef2, TRef3, TRef4, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4);
    }

    /// <summary>
    /// Executes a Go function with 4 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, GoFunc<TRef1, TRef2, TRef3, TRef4, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, TRef3, TRef4, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4);
    }

    /// <summary>
    /// Executes a Go function with 5 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4,
            ref ref5);
    }

    /// <summary>
    /// Executes a Go function with 5 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, T>(function).Execute(ref ref1, ref ref2, ref ref3,
            ref ref4, ref ref5);
    }

    /// <summary>
    /// Executes a Go function with 6 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, object>(action).Execute(ref ref1, ref ref2, ref ref3,
            ref ref4, ref ref5, ref ref6);
    }

    /// <summary>
    /// Executes a Go function with 6 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, T>(function).Execute(ref ref1, ref ref2, ref ref3,
            ref ref4, ref ref5, ref ref6);
    }

    /// <summary>
    /// Executes a Go function with 7 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, object>(action).Execute(ref ref1, ref ref2,
            ref ref3, ref ref4, ref ref5, ref ref6, ref ref7);
    }

    /// <summary>
    /// Executes a Go function with 7 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, T>(function).Execute(ref ref1, ref ref2,
            ref ref3, ref ref4, ref ref5, ref ref6, ref ref7);
    }

    /// <summary>
    /// Executes a Go function with 8 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, object>(action).Execute(ref ref1, ref ref2,
            ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8);
    }

    /// <summary>
    /// Executes a Go function with 8 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, T>(function).Execute(ref ref1,
            ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8);
    }

    /// <summary>
    /// Executes a Go function with 9 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, object>(action).Execute(ref ref1,
            ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9);
    }

    /// <summary>
    /// Executes a Go function with 9 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, T>(function).Execute(ref ref1,
            ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9);
    }

    /// <summary>
    /// Executes a Go function with 10 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, object>(action).Execute(
            ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10);
    }

    /// <summary>
    /// Executes a Go function with 10 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, T>(function).Execute(
            ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10);
    }

    /// <summary>
    /// Executes a Go function with 11 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, object>(action)
            .Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9,
                ref ref10, ref ref11);
    }

    /// <summary>
    /// Executes a Go function with 11 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, T>.GoRefFunction function)
    {
        return new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, T>(function)
            .Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9,
                ref ref10, ref ref11);
    }

    /// <summary>
    /// Executes a Go function with 12 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, object>(
            action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9,
            ref ref10, ref ref11, ref ref12);
    }

    /// <summary>
    /// Executes a Go function with 12 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, T>.GoRefFunction function)
    {
        return
            new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, T>(
                function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8,
                ref ref9, ref ref10, ref ref11, ref ref12);
    }

    /// <summary>
    /// Executes a Go function with 13 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="ref13">Reference parameter 13.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13,
            object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8,
            ref ref9, ref ref10, ref ref11, ref ref12, ref ref13);
    }

    /// <summary>
    /// Executes a Go function with 13 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="ref13">Reference parameter 13.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, T>.GoRefFunction function)
    {
        return
            new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13,
                T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8,
                ref ref9, ref ref10, ref ref11, ref ref12, ref ref13);
    }

    /// <summary>
    /// Executes a Go function with 14 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="ref13">Reference parameter 13.</param>
    /// <param name="ref14">Reference parameter 14.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14
            , object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8,
            ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14);
    }

    /// <summary>
    /// Executes a Go function with 14 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="ref13">Reference parameter 13.</param>
    /// <param name="ref14">Reference parameter 14.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, T>.GoRefFunction function)
    {
        return
            new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13,
                TRef14, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7,
                ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14);
    }

    /// <summary>
    /// Executes a Go function with 15 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="ref13">Reference parameter 13.</param>
    /// <param name="ref14">Reference parameter 14.</param>
    /// <param name="ref15">Reference parameter 15.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14
            , TRef15, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7,
            ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15);
    }

    /// <summary>
    /// Executes a Go function with 15 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="ref13">Reference parameter 13.</param>
    /// <param name="ref14">Reference parameter 14.</param>
    /// <param name="ref15">Reference parameter 15.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, T>.GoRefFunction function)
    {
        return
            new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13,
                TRef14, TRef15, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6,
                ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15);
    }

    /// <summary>
    /// Executes a Go function with 16 reference parameters and no return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="ref13">Reference parameter 13.</param>
    /// <param name="ref14">Reference parameter 14.</param>
    /// <param name="ref15">Reference parameter 15.</param>
    /// <param name="ref16">Reference parameter 16.</param>
    /// <param name="action">Go function to execute called with defer and recover function references.</param>
    public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, ref TRef16 ref16, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16, object>.GoRefAction action)
    {
        new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14
            , TRef15, TRef16, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6,
            ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15, ref ref16);
    }

    /// <summary>
    /// Executes a Go function with 16 reference parameters and a return value.
    /// </summary>
    /// <param name="ref1">Reference parameter 1.</param>
    /// <param name="ref2">Reference parameter 2.</param>
    /// <param name="ref3">Reference parameter 3.</param>
    /// <param name="ref4">Reference parameter 4.</param>
    /// <param name="ref5">Reference parameter 5.</param>
    /// <param name="ref6">Reference parameter 6.</param>
    /// <param name="ref7">Reference parameter 7.</param>
    /// <param name="ref8">Reference parameter 8.</param>
    /// <param name="ref9">Reference parameter 9.</param>
    /// <param name="ref10">Reference parameter 10.</param>
    /// <param name="ref11">Reference parameter 11.</param>
    /// <param name="ref12">Reference parameter 12.</param>
    /// <param name="ref13">Reference parameter 13.</param>
    /// <param name="ref14">Reference parameter 14.</param>
    /// <param name="ref15">Reference parameter 15.</param>
    /// <param name="ref16">Reference parameter 16.</param>
    /// <param name="function">Go function to execute called with defer and recover function references.</param>
    public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, ref TRef16 ref16, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16, T>.GoRefFunction function)
    {
        return
            new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13,
                TRef14, TRef15, TRef16, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6,
                ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15,
                ref ref16);
    }

    #endregion
}
