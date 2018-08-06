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
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Math;

#pragma warning disable CS0660, CS0661

namespace go
{
    public static class builtin
    {
        /// <summary>
        /// The built-in error interface type is the conventional interface for representing an
        /// error condition, with the nil value representing no error.
        /// </summary>
        public interface error : EmptyInterface
        {
            /// <summary>
            /// Get string that represents an error.
            /// </summary>
            string Error();
        }

        /// <summary>
        /// Predeclared identifier representing the untyped integer ordinal number of the current
        /// const specification in a (usually parenthesized) const declaration.
        /// It is zero-indexed.
        /// </summary>
        public const int iota = 0;

        /// <summary>
        /// nil is a predeclared identifier representing the zero value for a pointer, channel,
        /// func, interface, map, or slice type.
        /// </summary>
        public static readonly NilType nil = NilType.Default;

        /// <summary>
        /// Instructs a switch case extension call to transfer control to the first
        /// statement of the next case clause in an expression.
        /// </summary>
        public const bool fallthrough = true;

        /// <summary>
        /// Appends elements to the end of a slice. If it has sufficient capacity, the destination is
        /// resliced to accommodate the new elements. If it does not, a new underlying array will be
        /// allocated.
        /// </summary>
        /// <param name="slice">Destination slice.</param>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static slice<T> append<T>(slice<T> slice, params object[] elems) => append(ref slice, elems);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static slice<T> append<T>(ref slice<T> slice, params object[] elems) => slice<T>.Append(ref slice, elems);

        /// <summary>
        /// Gets the length of the <paramref name="array"/> (same as <see cref="len{T}(T[])"/>).
        /// </summary>
        /// <param name="array">Target array.</param>
        /// <returns>The length of the <paramref name="array"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int cap<T>(T[] array) => array?.Length ?? 0;

        /// <summary>
        /// Gets the maximum length the <paramref name="slice"/> can reach when resliced.
        /// </summary>
        /// <param name="slice">Target slice.</param>
        /// <returns>The capacity of the <paramref name="slice"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int cap<T>(slice<T> slice) => cap(ref slice);

        /// <summary>
        /// Gets the maximum length the <paramref name="slice"/> can reach when resliced.
        /// </summary>
        /// <param name="slice">Target slice pointer.</param>
        /// <returns>The capacity of the <paramref name="slice"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int cap<T>(ref slice<T> slice) => slice.Capacity;

        //public static void close<T>(Channel<T> c) = c.Close();

        /// <summary>
        /// Constructs a complex value from two floating-point values.
        /// </summary>
        /// <param name="realPart">Real-part of complex value.</param>
        /// <param name="imaginaryPart">Imaginary-part of complex value.</param>
        /// <returns>New complex value from specified <paramref name="realPart"/> and <paramref name="imaginaryPart"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static complex64 complex(float32 realPart, float32 imaginaryPart) => new complex64(realPart, imaginaryPart);

        /// <summary>
        /// Constructs a complex value from two floating-point values.
        /// </summary>
        /// <param name="realPart">Real-part of complex value.</param>
        /// <param name="imaginaryPart">Imaginary-part of complex value.</param>
        /// <returns>New complex value from specified <paramref name="realPart"/> and <paramref name="imaginaryPart"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static complex128 complex(float64 realPart, float64 imaginaryPart) => new complex128(realPart, imaginaryPart);

        /// <summary>
        /// Copies elements from a source slice into a destination slice.
        /// The source and destination may overlap.
        /// </summary>
        /// <param name="dst">Destination slice.</param>
        /// <param name="src">Source slice.</param>
        /// <returns>
        /// The number of elements copied, which will be the minimum of len(src) and len(dst).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int copy<T1, T2>(slice<T1> dst, slice<T2> src) => copy(ref dst, ref src);

        /// <summary>
        /// Copies elements from a source slice into a destination slice.
        /// The source and destination may overlap.
        /// </summary>
        /// <param name="dst">Destination slice pointer.</param>
        /// <param name="src">Source slice.</param>
        /// <returns>
        /// The number of elements copied, which will be the minimum of len(src) and len(dst).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int copy<T1, T2>(ref slice<T1> dst, slice<T2> src) => copy(ref dst, ref src);

        /// <summary>
        /// Copies elements from a source slice into a destination slice.
        /// The source and destination may overlap.
        /// </summary>
        /// <param name="dst">Destination slice.</param>
        /// <param name="src">Source slice pointer.</param>
        /// <returns>
        /// The number of elements copied, which will be the minimum of len(src) and len(dst).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int copy<T1, T2>(slice<T1> dst, ref slice<T2> src) => copy(ref dst, ref src);

        /// <summary>
        /// Copies elements from a source slice into a destination slice.
        /// The source and destination may overlap.
        /// </summary>
        /// <param name="dst">Destination slice pointer.</param>
        /// <param name="src">Source slice pointer.</param>
        /// <returns>
        /// The number of elements copied, which will be the minimum of len(src) and len(dst).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int copy<T1, T2>(ref slice<T1> dst, ref slice<T2> src)
        {
            if (dst.Array == null)
                throw new InvalidOperationException("Destination slice array reference is null.");

            if (src.Array == null)
                throw new InvalidOperationException("Source slice array reference is null.");

            int min = Min(dst.Length, src.Length);

            if (min > 0)
                Array.Copy(src.Array, dst.Array, min);

            return min;
        }

        /// <summary>
        /// Copies elements from a source slice into a destination slice.
        /// The source and destination may overlap.
        /// </summary>
        /// <param name="dst">Destination slice.</param>
        /// <param name="src">Source slice.</param>
        /// <returns>
        /// The number of elements copied, which will be the minimum of len(src) and len(dst).
        /// </returns>
        /// <remarks>
        /// As a special case, it also will copy bytes from a string to a slice of bytes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int copy(slice<byte> dst, string src) => copy(ref dst, src);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int copy(ref slice<byte> dst, string src)
        {
            slice<byte> bytes = new slice<byte>(Encoding.UTF8.GetBytes(src));
            return copy(dst, bytes);
        }

        //public static void delete<TKey, TValue>(Map<TKey, TValue> m, TKey key) => m.Delete(key);

        /// <summary>
        /// Gets the imaginary part of the complex number <paramref name="c"/>.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Imaginary part of the complex number <paramref name="c"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static double imag(complex64 c) => c.Imaginary;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int len<T>(T[] array) => array?.Length ?? 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slice"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int len<T>(slice<T> slice) => len(ref slice);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slice"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int len<T>(ref slice<T> slice) => slice.Length;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static int len(string s) => s?.Length ?? 0;

        //public static int len<T>(Map<T> map) => map?.Length ?? 0;

        //public static int len<T>(Channel<T> channel) => channel?.Length ?? 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="length"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static slice<T> make<T>(slice<T> _, int length, int capacity = -1) => make(ref _, length, capacity);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_"></param>
        /// <param name="length"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static slice<T> make<T>(ref slice<T> _, int length, int capacity = -1) => new slice<T>(length, capacity);

        //public static Map<TKey, TValue> make<TKey, TValue>(Map<TKey, TValue> _, int initialCapacity = -1) => new Map<TKey, TValue>(initialCapacity);

        //public static Channel<T> make<T>(Channel<T> _, capacity = 0) => new Channel<T>(capacity);

        /// <summary>
        /// Creates a pointer to a new type instance.
        /// </summary>
        /// <returns>Reference to newly allocated zero value of provided type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static Ptr<T> @new<T>() where T: new() => new Ptr<T>(new T());

        /// <summary>
        /// Formats arguments in an implementation-specific way and writes the result to standard-error.
        /// </summary>
        /// <param name="args">Arguments to display.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static void print(params object[] args) => Console.Error.Write(string.Join(" ", args.Select(arg => arg.ToString())));

        /// <summary>
        /// Formats arguments in an implementation-specific way and writes the result to standard-error along with a new line.
        /// </summary>
        /// <param name="args">Arguments to display.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static void println(params object[] args) => Console.Error.WriteLine(string.Join(" ", args.Select(arg => arg.ToString())));

        // ** Conversion Functions **

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="@bool"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="@bool"/>.</returns>
        public static @bool @bool(object value) => (bool)Convert.ChangeType(value, TypeCode.Boolean);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="@byte"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="@byte"/>.</returns>
        public static @byte @byte(object value) => (byte)Convert.ChangeType(value, TypeCode.Byte);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="rune"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="rune"/>.</returns>
        public static rune rune(object value) => (int32)(int)Convert.ChangeType(value, TypeCode.Int32);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="uint8"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="uint8"/>.</returns>
        public static uint8 uint8(object value) => (byte)Convert.ChangeType(value, TypeCode.Byte);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="uint16"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="uint16"/>.</returns>
        public static uint16 uint16(object value) => (ushort)Convert.ChangeType(value, TypeCode.UInt16);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="uint32"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="uint32"/>.</returns>
        public static uint32 uint32(object value) => (uint)Convert.ChangeType(value, TypeCode.UInt32);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="uint64"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="uint64"/>.</returns>
        public static uint64 uint64(object value) => (ulong)Convert.ChangeType(value, TypeCode.UInt64);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="int8"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="int8"/>.</returns>
        public static int8 int8(object value) => (sbyte)Convert.ChangeType(value, TypeCode.SByte);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="int16"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="int16"/>.</returns>
        public static int16 int16(object value) => (short)Convert.ChangeType(value, TypeCode.Int16);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="int32"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="int32"/>.</returns>
        public static int32 int32(object value) => (int)Convert.ChangeType(value, TypeCode.Int32);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="int64"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="int64"/>.</returns>
        public static int64 int64(object value) => (long)Convert.ChangeType(value, TypeCode.Int64);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="float32"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="float32"/>.</returns>
        public static float32 float32(object value) => (float)Convert.ChangeType(value, TypeCode.Single);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="float64"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="float64"/>.</returns>
        public static float64 float64(object value) => (double)Convert.ChangeType(value, TypeCode.Double);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="complex64"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="complex64"/>.</returns>
        public static complex64 complex64(object value)
        {
            if (value is complex128 dcomplex)
                return (complex64)dcomplex;

            if (!(value is complex64 fcomplex))
                return (float)Convert.ChangeType(value, TypeCode.Single);

            return fcomplex;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="complex128"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="complex128"/>.</returns>
        public static complex128 complex128(object value)
        {
            if (value is complex64 fcomplex)
                return fcomplex;

            if (!(value is complex128 dcomplex))
                return (double)Convert.ChangeType(value, TypeCode.Double);

            return dcomplex;
        }

#if Target32Bit
        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="@uint"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="@uint"/>.</returns>
        public static @uint @uint(object value) => (uint)Convert.ChangeType(value, TypeCode.UInt32);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="@int"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="@int"/>.</returns>
        public static @int @int(object value) => (int)Convert.ChangeType(value, TypeCode.Int32);
#else
        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="@uint"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="@uint"/>.</returns>
        public static @uint @uint(object value) => (ulong)Convert.ChangeType(value, TypeCode.UInt64);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="@int"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="@int"/>.</returns>
        public static @int @int(object value) => (long)Convert.ChangeType(value, TypeCode.Int64);
#endif

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="uintptr"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="uintptr"/>.</returns>
        public static uintptr uintptr(object value) => (UIntPtr)Convert.ChangeType(value, TypeCode.UInt64);

        /// <summary>
        /// Converts <paramref name="value"/> to a <see cref="@string"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns><paramref name="value"/> converted to a <see cref="@string"/>.</returns>
        public static @string @string(object value)
        {
            // Only reference types can be null, therefore "" is its default value
            if (value == null)
                return "";

            Type itemType = value.GetType();

            if (!itemType.IsValueType)
                return itemType.ToString();

            // Handle common types
            IConvertible convertible = value as IConvertible;
            bool isIntValue = false;
            ulong intValue = 0UL;

            if ((object)convertible != null)
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

            if (itemType == typeof(@byte[]))
                return new @string((@byte[])value);

            if (itemType == typeof(slice<@byte>))
                return new @string((slice<@byte>)value);

            if (itemType == typeof(byte[]))
                return new @string((byte[])value);

            if (itemType == typeof(slice<byte>))
                return new @string((slice<byte>)value);

            if (itemType == typeof(char[]))
                return new @string((char[])value);

            if (itemType == typeof(slice<char>))
                return new @string((slice<char>)value);

            if (itemType == typeof(rune[]))
                return new @string((rune[])value);

            if (itemType == typeof(slice<rune>))
                return new @string((slice<rune>)value);

            // Handle custom value types
            return value.ToString();
        }

        // ** Helper Functions **

        /// <summary>
        /// Converts imaginary literal value to a <see cref="complex128"/> imaginary number.
        /// </summary>
        /// <param name="literal">Literal imaginary value with "i" suffix.</param>
        /// <returns>New complex number with parsed <paramref name="literal"/> as imaginary part and a zero value real part.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static complex128 i(string literal)
        {
            if (!literal.EndsWith("i"))
                throw new InvalidCastException($"Token \"{literal}\" is not an imaginary literal.");

            if (double.TryParse(literal.Substring(0, literal.Length - 1), out double imaginary))
                return i(imaginary);

            throw new InvalidCastException($"Could not parse \"{literal}\" as an imaginary value.");
        }

        /// <summary>
        /// Converts value to a <see cref="complex128"/> imaginary number.
        /// </summary>
        /// <param name="imaginary">Value to convert to imaginary.</param>
        /// <returns>New complex number with specified <paramref name="imaginary"/> part and a zero value real part.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public static complex128 i(double imaginary) => new complex128(0.0D, imaginary);

        /// <summary>
        /// Creates a new switch expression that behaves like a Go switch statement.
        /// </summary>
        /// <returns>New switch object that behaves like a Go switch statement.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchExpression<object> Switch() => new SwitchExpression<object>(null);

        /// <summary>
        /// Creates a new switch expression that behaves like a Go switch statement.
        /// </summary>
        /// <param name="value">Switch target value.</param>
        /// <returns>New switch object that behaves like a Go switch statement.</returns>
        /// <typeparam name="T">Target type of the switch statement.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SwitchExpression<T> Switch<T>(T value) => new SwitchExpression<T>(value);

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

#region [ error interface implementation ]

        public struct error<T> : error
        {
            private T m_target;

            public T Target => m_target;

            private delegate string ErrorByVal(T value);
            private delegate string ErrorByRef(ref T value);

            private static readonly ErrorByVal s_ErrorByVal;
            private static readonly ErrorByRef s_ErrorByRef;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public string Error() => s_ErrorByRef?.Invoke(ref m_target) ?? s_ErrorByVal(m_target);

            [DebuggerStepperBoundary]
            static error()
            {
                Type targetType = typeof(T);
                MethodInfo extensionMethod;

                extensionMethod = targetType.GetExtensionMethod("Error");

                if ((object)extensionMethod != null)
                {
                    s_ErrorByRef = extensionMethod.CreateStaticDelegate(typeof(ErrorByRef)) as ErrorByRef;

                    if ((object)s_ErrorByRef == null)
                        s_ErrorByVal = extensionMethod.CreateStaticDelegate(typeof(ErrorByVal)) as ErrorByVal;
                }

                if ((object)s_ErrorByRef == null && (object)s_ErrorByVal == null)
                    throw new NotImplementedException($"{targetType.Name} does not implement error.Error method", new Exception("Error"));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
            public static explicit operator error<T>(T target) => new error<T> { m_target = target };

            // Enable comparisons between nil and error<T> interface instance
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(error<T> value, NilType nil) => Activator.CreateInstance<error<T>>().Equals(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(error<T> value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, error<T> value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, error<T> value) => value != nil;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static error error_cast<T>(T target)
        {
            if (typeof(error).IsAssignableFrom(typeof(T)))
                return target as error;

            return (error<T>)target;
        }


#endregion
    }

#region [ error interface nil comparisons and type assertions ]

    public partial class NilType
    {
        // Enable comparisons between nil and error interface
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(builtin.error value, NilType nil) => (object)value == null || Activator.CreateInstance(value.GetType()).Equals(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(builtin.error value, NilType nil) => !(value == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, builtin.error value) => value == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, builtin.error value) => value != nil;
    }

    public static class builtin_errorExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new ConcurrentDictionary<Type, MethodInfo>();

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static T TypeAssert<T>(this builtin.error target)
        {
            try
            {
                return ((builtin.error<T>)target).Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"panic: interface conversion: {target.GetType().FullName} is not {typeof(T).FullName}: missing method {ex.InnerException?.Message}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool TryTypeAssert<T>(this builtin.error target, out T result)
        {
            try
            {
                result = target.TypeAssert<T>();
                return true;
            }
            catch (PanicException)
            {
                result = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static object TypeAssert(this builtin.error target, Type type)
        {
            try
            {
                MethodInfo conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(builtin.error<>).GetExplicitGenericConversionOperator(type));

                if ((object)conversionOperator == null)
                    throw new PanicException($"panic: interface conversion: failed to create converter for {target.GetType().FullName} to {type.FullName}");

                dynamic result = conversionOperator.Invoke(null, new object[] { target });
                return result.Target;
            }
            catch (NotImplementedException ex)
            {
                throw new PanicException($"panic: interface conversion: {target.GetType().FullName} is not {type.FullName}: missing method {ex.InnerException?.Message}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public static bool TryTypeAssert(this builtin.error target, Type type, out object result)
        {
            try
            {
                result = target.TypeAssert(type);
                return true;
            }
            catch (PanicException)
            {
                result = type.IsValueType ? Activator.CreateInstance(type) : null;
                return false;
            }
        }
    }

#endregion
}