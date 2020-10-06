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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using static System.Math;

using uint8 = System.Byte;
using uint16 = System.UInt16;
using uint32 = System.UInt32;
using uint64 = System.UInt64;
using int8 = System.SByte;
using int16 = System.Int16;
using int32 = System.Int32;
using int64 = System.Int64;
using float32 = System.Single;
using float64 = System.Double;
using complex128 = System.Numerics.Complex;
using rune = System.Int32;
using uintptr = System.UIntPtr;
using System.Threading;

namespace go
{
    public static class builtin
    {
        private static class Zero<T>
        {
            public static T Default = default!;
        }

        private static readonly ThreadLocal<bool> s_fallthrough = new ThreadLocal<bool>();

        /// <summary>
        /// Predeclared identifier representing the untyped integer ordinal number of the current
        /// const specification in a (usually parenthesized) const declaration.
        /// It is zero-indexed.
        /// </summary>
        public const long iota = 0;

        /// <summary>
        /// Defines a constant to return a tuple that includes a boolean success indicator.
        /// </summary>
        public const bool WithOK = false;

        /// <summary>
        /// Defines a constant to return a tuple that includes an error indicator.
        /// </summary>
        public const bool WithErr = false;

        /// <summary>
        /// Defines a constant to return a tuple that includes a value.
        /// </summary>
        public const bool WithVal = false;

        /// <summary>
        /// nil is a predeclared identifier representing the zero value for a pointer, channel,
        /// func, interface, map, or slice type.
        /// </summary>
        public static readonly NilType nil = NilType.Default;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static slice<T> append<T>(in slice<T> slice, params T[] elems) => go.slice<T>.Append(slice, elems);

        /// <summary>
        /// Gets the length of the <paramref name="array"/> (same as len(array)).
        /// </summary>
        /// <param name="array">Target array pointer.</param>
        /// <returns>The length of the <paramref name="array"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long cap<T>(in array<T> array) => array.Length;

        /// <summary>
        /// Gets the maximum length the <paramref name="slice"/> can reach when resliced.
        /// </summary>
        /// <param name="slice">Target slice pointer.</param>
        /// <returns>The capacity of the <paramref name="slice"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long cap<T>(in slice<T> slice) => slice.Capacity;

        /// <summary>
        /// Gets the maximum capacity of the <paramref name="channel"/>.
        /// </summary>
        /// <param name="channel">Target channel pointer.</param>
        /// <returns>The capacity of the <paramref name="channel"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long cap<T>(in channel<T> channel) => channel.Capacity;

        /// <summary>
        /// Closes the channel.
        /// </summary>
        /// <param name="channel">Target channel pointer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        // An "in" parameter works here because the close method operates on channel structure's
        // private class-based member references, not on value types
        // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
        public static void close<T>(in channel<T> channel) => channel.Close();

        /// <summary>
        /// Constructs a complex value from two floating-point values.
        /// </summary>
        /// <param name="realPart">Real-part of complex value.</param>
        /// <param name="imaginaryPart">Imaginary-part of complex value.</param>
        /// <returns>New complex value from specified <paramref name="realPart"/> and <paramref name="imaginaryPart"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static complex64 complex(float32 realPart, float32 imaginaryPart) => new complex64(realPart, imaginaryPart);

        /// <summary>
        /// Constructs a complex value from two floating-point values.
        /// </summary>
        /// <param name="realPart">Real-part of complex value.</param>
        /// <param name="imaginaryPart">Imaginary-part of complex value.</param>
        /// <returns>New complex value from specified <paramref name="realPart"/> and <paramref name="imaginaryPart"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static complex128 complex(float64 realPart, float64 imaginaryPart) => new complex128(realPart, imaginaryPart);

        /// <summary>
        /// Copies elements from a source slice into a destination slice.
        /// The source and destination may overlap.
        /// </summary>
        /// <param name="dst">Destination slice pointer.</param>
        /// <param name="src">Source slice pointer.</param>
        /// <returns>
        /// The number of elements copied, which will be the minimum of len(src) and len(dst).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long copy<T1, T2>(in slice<T1> dst, in slice<T2> src)
        {
            if (dst == nil)
                throw new InvalidOperationException("Destination slice array reference is null.");

            if (src == nil)
                throw new InvalidOperationException("Source slice array reference is null.");

            long min = Min(dst.Length, src.Length);

            if (min > 0)
            {
                if (typeof(T1).IsAssignableFrom(typeof(T2)))
                {
                    Array.Copy(src.Array, src.Low, dst.Array, dst.Low, min);
                }
                else
                {
                    for (long i = 0; i < min; i++)
                        dst[dst.Low + i] = (T1)ConvertToType((IConvertible)src[src.Low + i]!);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long copy(in slice<byte> dst, in @string src)
        {
            slice<byte> bytes = src;
            return copy(dst, bytes);
        }

        /// <summary>
        /// Deletes the element with the specified key from the map. If m is nil or there is no such element, delete is a no-op.
        /// </summary>
        /// <param name="map">Target map.</param>
        /// <param name="key">Key to remove.</param>
        public static void delete<TKey, TValue>(map<TKey, TValue> map, TKey key) where TKey : notnull => map.Remove(key);

        /// <summary>
        /// Gets the imaginary part of the complex number <paramref name="c"/>.
        /// </summary>
        /// <param name="c">Complex number.</param>
        /// <returns>Imaginary part of the complex number <paramref name="c"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static float imag(complex64 c) => c.Imaginary;

        /// <summary>
        /// Gets the imaginary part of the complex number <paramref name="c"/>.
        /// </summary>
        /// <param name="c">Complex number.</param>
        /// <returns>Imaginary part of the complex number <paramref name="c"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static double imag(complex128 c) => c.Imaginary;

        /// <summary>
        /// Gets the real part of the complex number <paramref name="c"/>.
        /// </summary>
        /// <param name="c">Complex number.</param>
        /// <returns>Real part of the complex number <paramref name="c"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static float real(complex64 c) => c.Real;

        /// <summary>
        /// Gets the real part of the complex number <paramref name="c"/>.
        /// </summary>
        /// <param name="c">Complex number.</param>
        /// <returns>Real part of the complex number <paramref name="c"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static double real(complex128 c) => c.Real;

        /// <summary>
        /// Gets the length of the <paramref name="array"/>.
        /// </summary>
        /// <param name="array">Target array.</param>
        /// <returns>The length of the <paramref name="array"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len<T>(in array<T> array) => array.Length;

        /// <summary>
        /// Gets the length of the <paramref name="array"/>.
        /// </summary>
        /// <param name="array">Target array pointer.</param>
        /// <returns>The length of the <paramref name="array"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len<T>(in ptr<array<T>> array) => array.val.Length;

        /// <summary>
        /// Gets the length of the <paramref name="slice"/>.
        /// </summary>
        /// <param name="slice">Target slice.</param>
        /// <returns>The length of the <paramref name="slice"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len<T>(in slice<T> slice) => slice.Length;

        /// <summary>
        /// Gets the length of the <paramref name="slice"/>.
        /// </summary>
        /// <param name="slice">Target slice pointer.</param>
        /// <returns>The length of the <paramref name="slice"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len<T>(in ptr<slice<T>> slice) => slice.val.Length;

        /// <summary>
        /// Gets the length of the <paramref name="str"/>.
        /// </summary>
        /// <param name="str">Target string.</param>
        /// <returns>The length of the <paramref name="str"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len(in @string str) => str.Length;

        /// <summary>
        /// Gets the length of the <paramref name="str"/>.
        /// </summary>
        /// <param name="str">Target string pointer.</param>
        /// <returns>The length of the <paramref name="str"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len(in ptr<@string> str) => str.val.Length;

        /// <summary>
        /// Gets the length of the <paramref name="str"/>.
        /// </summary>
        /// <param name="str">Target channel pointer.</param>
        /// <returns>The length of the <paramref name="str"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len(string str) => str.Length;

        /// <summary>
        /// Gets the length of the <paramref name="map"/>.
        /// </summary>
        /// <param name="map">Target map.</param>
        /// <returns>The length of the <paramref name="map"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len<TKey, TValue>(in map<TKey, TValue> map) where TKey : notnull => map.Count;

        /// <summary>
        /// Gets the length of the <paramref name="map"/>.
        /// </summary>
        /// <param name="map">Target map pointer.</param>
        /// <returns>The length of the <paramref name="map"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len<TKey, TValue>(in ptr<map<TKey, TValue>> map) where TKey : notnull => map.val.Count;

        /// <summary>
        /// Gets the length of the <paramref name="channel"/>.
        /// </summary>
        /// <param name="channel">Target channel.</param>
        /// <returns>The length of the <paramref name="channel"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len<T>(in channel<T> channel) => channel.Length;

        /// <summary>
        /// Gets the length of the <paramref name="channel"/>.
        /// </summary>
        /// <param name="channel">Target channel pointer.</param>
        /// <returns>The length of the <paramref name="channel"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long len<T>(in ptr<channel<T>> channel) => channel.val.Length;

        /// <summary>
        /// Allocates and initializes a slice object.
        /// </summary>
        /// <param name="size">Specifies the slice length.</param>
        /// <param name="capacity">Specified slice capacity; must be no smaller than the length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static slice<T> make_slice<T>(long size, long capacity = 0) => new slice<T>((int)size, (int)capacity);

        /// <summary>
        /// Allocates and initializes a map object.
        /// </summary>
        // <param name="size">Specifies the number of map elements.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static map<TKey, TValue> make_map<TKey, TValue>(long size = 0) where TKey : notnull => new map<TKey, TValue>((int)size);

        /// <summary>
        /// Allocates and initializes a channel object.
        /// </summary>
        /// <param name="size">Specifies the buffer capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static channel<T> make_channel<T>(long size = 1) => new channel<T>((int)size);

        /// <summary>
        /// Allocates and initializes a new object.
        /// </summary>
        /// <param name="p1">Size parameter.</param>
        /// <param name="p2">Capacity parameter,</param>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <returns>New object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static T make<T>(long p1 = 0, long p2 = -1) where T : new()
        {
            if (p1 == 0 && p2 == 0)
                return new T();

            Type type = typeof(T);

            if (type == typeof(slice<>))
                return (T)Activator.CreateInstance(type, p1, p2, 0)!;

            if (type == typeof(channel<>) && p1 == 0)
                p1 = 1;

            return (T)Activator.CreateInstance(type, p1)!;
        }

        /// <summary>
        /// Gets a reference to a zero value instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Target type of reference.</typeparam>
        /// <returns>Reference to a zero value instance of type <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static ref T zero<T>() => ref Zero<T>.Default;

        /// <summary>
        /// Creates a new heap allocated copy of existing <paramref name="target"/> value.
        /// </summary>
        /// <typeparam name="T">Target type of reference.</typeparam>
        /// <param name="target">Target value.</param>
        /// <returns>Pointer to heap allocated copy of <paramref name="target"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static ptr<T> addr<T>(in T target) => new ptr<T>(target);

        /// <summary>
        /// Creates a new heap allocated instance of the zero value for type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="pointer">Out reference to pointer to heap allocated copy of <paramref name="target"/> value.</param>
        /// <typeparam name="T">Target type of reference.</typeparam>
        /// <returns>Reference to heap allocated instance of the zero value for type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// This is a convenience function to allow default local struct ref and <see cref="go.ptr{T}"/>
        /// to be created in a single call, e.g.:
        /// <code language="cs">
        ///     ref var v = ref heap(out ptr&lt;Vertex&gt; v_ptr);
        /// </code>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static ref T heap<T>(out ptr<T> pointer)
        {
            pointer = addr(default(T)!);
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
        /// This is a convenience function to allow local struct ref and <see cref="go.ptr{T}"/>
        /// to be created in a single call, e.g.:
        /// <code language="cs">
        ///     ref var v = ref heap(new Vertex(40.68433, -74.39967), out var v_ptr);
        /// </code>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static ref T heap<T>(in T target, out ptr<T> pointer)
        {
            pointer = addr(target);
            return ref pointer.val;
        }

        /// <summary>
        /// Creates a heap allocated pointer reference to a new zero value instance of type.
        /// </summary>
        /// <returns>Pointer to heap allocated zero value of provided type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static ptr<T> @new<T>() where T : new() => new ptr<T>(new T());

        /// <summary>
        /// Creates a new reference for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Target type of reference.</typeparam>
        /// <param name="inputs">Constructor parameters.</param>
        /// <returns>New reference for <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static ptr<T> @new<T>(params object[] inputs) => new ptr<T>((T)Activator.CreateInstance(typeof(T), inputs)!);

        /// <summary>
        /// Formats arguments in an implementation-specific way and writes the result to standard-error.
        /// </summary>
        /// <param name="args">Arguments to display.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static void print(params object[] args) => Console.Error.Write(string.Join(" ", args.Select(arg => arg.ToString())));

        /// <summary>
        /// Formats arguments in an implementation-specific way and writes the result to standard-error along with a new line.
        /// </summary>
        /// <param name="args">Arguments to display.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static void println(params object[] args) => Console.Error.WriteLine(string.Join(" ", args.Select(arg => arg.ToString())));

        /// <summary>
        /// Execute Go routine.
        /// </summary>
        /// <param name="action">Routine to execute.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static void go_(Action action) => ThreadPool.QueueUserWorkItem(_ => action());

        /// <summary>
        /// Exits application with a fatal error.
        /// </summary>
        /// <param name="message">Fatal error message.</param>
        /// <param name="code">Application exit code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary, DebuggerNonUserCode]
        public static void fatal(string message, long code = 1)
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

        /// <summary>
        /// Enumerates indexes of <see cref="go.array{T}"/> <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Array type.</typeparam>
        /// <param name="source">Source array.</param>
        /// <returns>Enumerable of indexes.</returns>
        public static IEnumerable<long> range<T>(in array<T> source) => source.Range;

        /// <summary>
        /// Enumerates indexes and values of <see cref="go.array{T}"/> <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Array type.</typeparam>
        /// <param name="source">Source array.</param>
        /// <param name="_">Overload marker, set to <see cref="WithVal"/>.</param>
        /// <returns>Enumerable of indexes and values.</returns>
        public static IEnumerable<(long, T)> range<T>(in array<T> source, bool _) => source;

        /// <summary>
        /// Enumerates indexes of <see cref="go.slice{T}"/> <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Slice type.</typeparam>
        /// <param name="source">Source slice.</param>
        /// <returns>Enumerable of indexes.</returns>
        public static IEnumerable<long> range<T>(in slice<T> source) => source.Range;

        /// <summary>
        /// Enumerates indexes and values of <see cref="go.slice{T}"/> <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Slice type.</typeparam>
        /// <param name="source">Source slice.</param>
        /// <param name="_">Overload marker, set to <see cref="WithVal"/>.</param>
        /// <returns>Enumerable of indexes and values.</returns>
        public static IEnumerable<(long, T)> range<T>(in slice<T> source, bool _) => source;

        // ** Type Assertion Functions **

        /// <summary>
        /// Type asserts <paramref name="target"/> object as type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Desired type for <paramref name="target"/>.</typeparam>
        /// <param name="target">Source value to type assert.</param>
        /// <returns><paramref name="target"/> value cast as <typeparamref name="T"/>, if successful.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */, DebuggerNonUserCode]
        public static T _<T>(this object target)
        {
            try
            {
                return (T)target;
            }
            catch (InvalidCastException ex)
            {
                throw new PanicException($"interface conversion: interface{{}} is {GetGoTypeName(target.GetType())}, not {GetGoTypeName(typeof(T))}", ex);
            }
        }

        /// <summary>
        /// Attempts type assert of <paramref name="target"/> object as type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Desired type for <paramref name="target"/>.</typeparam>
        /// <param name="target">Source value to type assert.</param>
        /// <param name="_"><see cref="WithOK"/> placeholder parameter used to overload return type.</param>
        /// <returns>Tuple of <paramref name="target"/> value cast as <typeparamref name="T"/> and success boolean.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */, DebuggerNonUserCode]
        public static (T, bool) _<T>(this object target, bool _)
        {
            try
            {
                return ((T)target, true);
            }
            catch (InvalidCastException)
            {
                return (default, false)!;
            }
        }

        /// <summary>
        /// Gets common Go type for given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">Target value.</param>
        /// <returns>Common Go type for given <paramref name="target"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */, DebuggerNonUserCode]
        public static object type(this object target)
        {
            // Infer common go type as needed
            return target switch
            {
                string str => new @string(str),
                _ => target
            };
        }

        /// <summary>
        /// Gets the common Go type name for the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Value to evaluate.</param>
        /// <returns>Common Go type name for the specified <paramref name="value"/>.</returns>
        public static string GetGoTypeName(object value) => GetGoTypeName(value.GetType());

        /// <summary>
        /// Gets the common Go type name for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Target type</param>
        /// <returns>Common Go type name for the specified <paramref name="type"/>.</returns>
        public static string GetGoTypeName(Type type)
        {
            if (type is null)
                return "nil";

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                    return "string";
                case TypeCode.Char:
                    return "rune";
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.SByte:
                    return "int8";
                case TypeCode.Int16:
                    return "int16";
                case TypeCode.Int32:
                    return "int32";
                case TypeCode.Int64:
                    return "int64";
                case TypeCode.Byte:
                    return "byte";
                case TypeCode.UInt16:
                    return "uint16";
                case TypeCode.UInt32:
                    return "uint32";
                case TypeCode.UInt64:
                    return "uint64";
                case TypeCode.Single:
                    return "float32";
                case TypeCode.Double:
                    return "float64";
                default:
                {
                    string typeName = type.FullName?? type.Name;

                    switch (typeName)
                    {
                        case "System.Numerics.Complex":
                            return "complex128";
                        case "go.complex64":
                            return "complex64";
                        default:
                            return type == typeof(object) ? "interface {}" : typeName;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the common Go type name for the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <returns>Common Go type name for the specified type <typeparamref name="T"/>.</returns>
        public static string GetGoTypeName<T>() => GetGoTypeName(typeof(T));

        // ** Conversion Functions **

        /// <summary>
        /// Creates a new Go <see cref="go.array{T}"/> with specified <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="T">Type of array.</typeparam>
        /// <param name="length">Target array length.</param>
        /// <returns>Go <see cref="go.array{T}"/> with specified <paramref name="length"/>.</returns>
        public static array<T> array<T>(long length) => new array<T>(length);

        /// <summary>
        /// Converts C# <paramref name="source"/> array to Go <see cref="go.array{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of array.</typeparam>
        /// <param name="source">C# source array.</param>
        /// <returns>Go <see cref="go.array{T}"/> wrapper for C# <paramref name="source"/> array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static array<T> array<T>(T[] source) => source;

        /// <summary>
        /// Converts C# <paramref name="source"/> array to Go <see cref="slice{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of array.</typeparam>
        /// <param name="source">C# source array.</param>
        /// <returns>Go <see cref="slice{T}"/> wrapper for C# <paramref name="source"/> array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static slice<T> slice<T>(T[] source) => source;

        /// <summary>
        /// Converts C# <paramref name="source"/> string array to Go <see cref="slice{@string}"/>.
        /// </summary>
        /// <param name="source">C# source array.</param>
        /// <returns>Go <see cref="slice{@string}"/> wrapper for C# <paramref name="source"/> string array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static slice<@string> slice(IReadOnlyCollection<string> source) => @string(source);

        /// <summary>
        /// Converts C# <see cref="string"/> array into Go <see cref="go.@string"/> array.
        /// </summary>
        /// <param name="source">C# <see cref="string"/> array</param>
        /// <returns>Go <see cref="go.@string"/> array from C# <see cref="string"/> array <paramref name="source"/>.</returns>
        public static @string[] @string(IReadOnlyCollection<string> source) => source.Select(value => new @string(value)).ToArray();

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="byte"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="byte"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static byte @byte(byte value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="byte"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="byte"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static byte @byte(object value) => (byte)Convert.ChangeType(value, TypeCode.Byte);

        /// <summary>
        /// Converts <paramref name="value"/> to a rune.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a rune.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static rune rune(int32 value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a rune.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a rune.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static rune rune(object value) => (int)Convert.ChangeType(value, TypeCode.Int32);

        /// <summary>
        /// Converts <paramref name="value"/> to a uint8.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint8.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uint8 uint8(byte value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a uint8.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint8.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uint8 uint8(object value) => (byte)Convert.ChangeType(value, TypeCode.Byte);

        /// <summary>
        /// Converts <paramref name="value"/> to a uint16.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint16.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uint16 uint16(ushort value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a uint16.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint16.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uint16 uint16(object value) => (ushort)Convert.ChangeType(value, TypeCode.UInt16);

        /// <summary>
        /// Converts <paramref name="value"/> to a uint32.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uint32 uint32(uint value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a uint32.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uint32 uint32(object value) => (uint)Convert.ChangeType(value, TypeCode.UInt32);

        /// <summary>
        /// Converts <paramref name="value"/> to a uint64.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uint64 uint64(ulong value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a uint64.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uint64 uint64(object value) => (ulong)Convert.ChangeType(value, TypeCode.UInt64);

        /// <summary>
        /// Converts <paramref name="value"/> to a int8.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int8.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static int8 int8(sbyte value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a int8.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int8.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static int8 int8(object value) => (sbyte)Convert.ChangeType(value, TypeCode.SByte);

        /// <summary>
        /// Converts <paramref name="value"/> to a int16.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int16.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static int16 int16(short value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a int16.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int16.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static int16 int16(object value) => (short)Convert.ChangeType(value, TypeCode.Int16);

        /// <summary>
        /// Converts <paramref name="value"/> to a int32.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static int32 int32(int value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a int32.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static int32 int32(long value) => (int32)value;

        /// <summary>
        /// Converts <paramref name="value"/> to a int32.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static int32 int32(object value) => (int)Convert.ChangeType(value, TypeCode.Int32);

        /// <summary>
        /// Converts <paramref name="value"/> to a int64.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static int64 int64(long value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a int64.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static int64 int64(object value) => (long)Convert.ChangeType(value, TypeCode.Int64);

        /// <summary>
        /// Converts <paramref name="value"/> to a float32.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a float32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static float32 float32(float value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a float32.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a float32.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static float32 float32(object value) => (float)Convert.ChangeType(value, TypeCode.Single);

        /// <summary>
        /// Converts <paramref name="value"/> to a float64.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a float64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static float64 float64(double value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a float64.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a float64.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static float64 float64(object value) => (double)Convert.ChangeType(value, TypeCode.Double);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="go.complex64"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="go.complex64"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static complex64 complex64(object value)
        {
            if (value is complex128 dcomplex)
                return (complex64)dcomplex;

            if (!(value is complex64 fcomplex))
                return (float)Convert.ChangeType(value, TypeCode.Single);

            return fcomplex;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to a complex128.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a complex128.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static complex128 complex128(Complex value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a complex128.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a complex128.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static complex128 complex128(object value)
        {
            if (value is complex64 fcomplex)
                return fcomplex;

            if (!(value is complex128 dcomplex))
                return (double)Convert.ChangeType(value, TypeCode.Double);

            return dcomplex;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to a uint.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static ulong @uint(uint value) => (ulong)Convert.ChangeType(value, TypeCode.UInt64);

        /// <summary>
        /// Converts <paramref name="value"/> to a uint.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static ulong @uint(ulong value) => (ulong)Convert.ChangeType(value, TypeCode.UInt64);

        /// <summary>
        /// Converts <paramref name="value"/> to a uint.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uint.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static ulong @uint(object value) => (ulong)Convert.ChangeType(value, TypeCode.UInt64);

        /// <summary>
        /// Converts <paramref name="value"/> to a int.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long @int(int value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a int.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long @int(long value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a int.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a int.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static long @int(object value) => (long)Convert.ChangeType(value, TypeCode.Int64);

        /// <summary>
        /// Converts <paramref name="value"/> to a uintptr.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uintptr.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uintptr uintptr(UIntPtr value) => value;

        /// <summary>
        /// Converts <paramref name="value"/> to a uintptr.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a uintptr.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static uintptr uintptr(object value) => (UIntPtr)Convert.ChangeType(value, TypeCode.UInt64);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="@string"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="@string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static @string @string(string value) => new @string(value);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="@string"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="@string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static @string @string(object value)
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

                if (intValue >= char.MinValue && intValue <= char.MaxValue)
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
                return new @string((rune[])value);

            if (itemType == typeof(slice<rune>))
                return new @string((slice<rune>)value);

            // Handle custom value types
            return value.ToString()!;
        }

        // ** Helper Functions **

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CastCopy<TSource, TDest>(this TSource[] source, long sourceIndex, TDest[] dest, long destinationIndex, Func<TSource, TDest> cast, long length)
        {
            for (long i = 0; i < length; i++)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CastCopy<TSource, TDest>(this TSource[] source, TDest[] dest, Func<TSource, TDest> cast, long length = -1)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static complex128 i(string literal)
        {
            if (!literal.EndsWith("i"))
                throw new InvalidCastException($"Token \"{literal}\" is not an imaginary literal.");

            if (double.TryParse(literal.Substring(0, literal.Length - 1), out double imaginary))
                return i(imaginary);

            throw new InvalidCastException($"Could not parse \"{literal}\" as an imaginary value.");
        }

        /// <summary>
        /// Converts value to a complex128 imaginary number.
        /// </summary>
        /// <param name="imaginary">Value to convert to imaginary.</param>
        /// <returns>New complex number with specified <paramref name="imaginary"/> part and a zero value real part.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining) /* , DebuggerStepperBoundary */]
        public static complex128 i(double imaginary) => new complex128(0.0D, imaginary);

        /// <summary>
        /// Returns a Go type equivalent to the specified value.
        /// </summary>
        /// <param name="value">An object that implements the <see cref="IConvertible" /> interface.</param>
        /// <returns>A Go type whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ConvertToType<T>(in T value) where T : IConvertible
        {
            if (value is null)
                return nil;

            return value.GetTypeCode() switch
            {
                TypeCode.Boolean => value.ToBoolean(null),
                TypeCode.Char => (rune)value.ToChar(null),
                TypeCode.SByte => value.ToSByte(null),
                TypeCode.Byte => value.ToByte(null),
                TypeCode.Int16 => value.ToInt16(null),
                TypeCode.UInt16 => value.ToUInt16(null),
                TypeCode.Int32 => value.ToInt32(null),
                TypeCode.UInt32 => value.ToUInt32(null),
                TypeCode.Int64 => value.ToInt64(null),
                TypeCode.UInt64 => value.ToUInt64(null),
                TypeCode.Single => value.ToSingle(null),
                TypeCode.Double => value.ToDouble(null),
                _ => (@string)value.ToString(null)
            };
        }

        /// <summary>
        /// Converts keyed value initializer into an array.
        /// </summary>
        /// <param name="length">Length of target array.</param>
        /// <param name="keyedValues">Keyed values.</param>
        /// <returns>Array from keyed value initializer.</returns>
        /// <typeparam name="T">Type of values.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] InitKeyedValues<T>(long length, params object[] keyedValues) where T : struct
        {
            T[] values = new T[length];

            foreach (object keyedValue in keyedValues)
            {
                switch (keyedValue)
                {
                    case T value:
                        values[values.Length] = value;

                        break;
                    case (var index, T indexValue):
                        {
                            if (!(index is null))
                            {
                                if (index.TryCastAsInteger(out ulong key))
                                    values[key] = indexValue;
                            }

                            break;
                        }
                }
            }

            return values;
        }

        /// <summary>
        /// Converts keyed value initializer into an array.
        /// </summary>
        /// <param name="keyedValues">Keyed values.</param>
        /// <returns>Array from keyed value initializer.</returns>
        /// <typeparam name="T">Type of values.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] InitKeyedValues<T>(params object[] keyedValues) where T : struct
        {
            List<T> values = new List<T>();

            foreach (object keyedValue in keyedValues)
            {
                switch (keyedValue)
                {
                    case T value:
                        values.Add(value);

                        break;
                    case (var index, T indexValue):
                        {
                            if (!(index is null))
                            {
                                if (index.TryCastAsInteger(out ulong key))
                                {
                                    for (ulong i = (ulong)values.Count; i < key; i++)
                                        values.Add(default);

                                    values.Add(indexValue);
                                }
                            }

                            break;
                        }
                }
            }

            return values.ToArray();
        }

        // ** Go Function Execution Context Handlers **/

        /// <summary>
        /// Executes a Go function with no return value.
        /// </summary>
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func(GoFunc<object>.GoAction action) => new GoFunc<object>(action).Execute();

        /// <summary>
        /// Executes a Go function with a return value.
        /// </summary>
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<T>(GoFunc<T>.GoFunction function) => new GoFunc<T>(function).Execute();

        /// <summary>
        /// Executes a Go function with 1 reference parameter and no return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1>(ref TRef1 ref1, GoFunc<TRef1, object>.GoRefAction action) => new GoFunc<TRef1, object>(action).Execute(ref ref1);

        /// <summary>
        /// Executes a Go function with 1 reference parameter and a return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, T>(ref TRef1 ref1, GoFunc<TRef1, T>.GoRefFunction function) => new GoFunc<TRef1, T>(function).Execute(ref ref1);

        #region [ func<TRef1, TRef2, ... TRef16> Implementations ]

        /*  The following code was generated using the "GenGoFuncRefInstances" utility: */

        /// <summary>
        /// Executes a Go function with 2 reference parameters and no return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2>(ref TRef1 ref1, ref TRef2 ref2, GoFunc<TRef1, TRef2, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, object>(action).Execute(ref ref1, ref ref2);

        /// <summary>
        /// Executes a Go function with 2 reference parameters and a return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, T>(ref TRef1 ref1, ref TRef2 ref2, GoFunc<TRef1, TRef2, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, T>(function).Execute(ref ref1, ref ref2);

        /// <summary>
        /// Executes a Go function with 3 reference parameters and no return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="ref3">Reference parameter 3.</param>
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, GoFunc<TRef1, TRef2, TRef3, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, object>(action).Execute(ref ref1, ref ref2, ref ref3);

        /// <summary>
        /// Executes a Go function with 3 reference parameters and a return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="ref3">Reference parameter 3.</param>
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, GoFunc<TRef1, TRef2, TRef3, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, T>(function).Execute(ref ref1, ref ref2, ref ref3);

        /// <summary>
        /// Executes a Go function with 4 reference parameters and no return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="ref3">Reference parameter 3.</param>
        /// <param name="ref4">Reference parameter 4.</param>
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, GoFunc<TRef1, TRef2, TRef3, TRef4, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4);

        /// <summary>
        /// Executes a Go function with 4 reference parameters and a return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="ref3">Reference parameter 3.</param>
        /// <param name="ref4">Reference parameter 4.</param>
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, GoFunc<TRef1, TRef2, TRef3, TRef4, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4);

        /// <summary>
        /// Executes a Go function with 5 reference parameters and no return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="ref3">Reference parameter 3.</param>
        /// <param name="ref4">Reference parameter 4.</param>
        /// <param name="ref5">Reference parameter 5.</param>
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5);

        /// <summary>
        /// Executes a Go function with 5 reference parameters and a return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="ref3">Reference parameter 3.</param>
        /// <param name="ref4">Reference parameter 4.</param>
        /// <param name="ref5">Reference parameter 5.</param>
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5);

        /// <summary>
        /// Executes a Go function with 6 reference parameters and no return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="ref3">Reference parameter 3.</param>
        /// <param name="ref4">Reference parameter 4.</param>
        /// <param name="ref5">Reference parameter 5.</param>
        /// <param name="ref6">Reference parameter 6.</param>
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6);

        /// <summary>
        /// Executes a Go function with 6 reference parameters and a return value.
        /// </summary>
        /// <param name="ref1">Reference parameter 1.</param>
        /// <param name="ref2">Reference parameter 2.</param>
        /// <param name="ref3">Reference parameter 3.</param>
        /// <param name="ref4">Reference parameter 4.</param>
        /// <param name="ref5">Reference parameter 5.</param>
        /// <param name="ref6">Reference parameter 6.</param>
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15);

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
        /// <param name="action">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static void func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, ref TRef16 ref16, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16, object>.GoRefAction action) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16, object>(action).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15, ref ref16);

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
        /// <param name="function">Go function to execute called with defer, panic and recover function references.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T func<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16, T>(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, ref TRef16 ref16, GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16, T>.GoRefFunction function) => new GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16, T>(function).Execute(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15, ref ref16);

        #endregion
    }
}
