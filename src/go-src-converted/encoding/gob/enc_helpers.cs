// Created by encgen --output enc_helpers.go; DO NOT EDIT

// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gob -- go2cs converted at 2020 August 29 08:35:37 UTC
// import "encoding/gob" ==> using gob = go.encoding.gob_package
// Original source: C:\Go\src\encoding\gob\enc_helpers.go
using reflect = go.reflect_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class gob_package
    {
        private static map encArrayHelper = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<reflect.Kind, encHelper>{reflect.Bool:encBoolArray,reflect.Complex64:encComplex64Array,reflect.Complex128:encComplex128Array,reflect.Float32:encFloat32Array,reflect.Float64:encFloat64Array,reflect.Int:encIntArray,reflect.Int16:encInt16Array,reflect.Int32:encInt32Array,reflect.Int64:encInt64Array,reflect.Int8:encInt8Array,reflect.String:encStringArray,reflect.Uint:encUintArray,reflect.Uint16:encUint16Array,reflect.Uint32:encUint32Array,reflect.Uint64:encUint64Array,reflect.Uintptr:encUintptrArray,};

        private static map encSliceHelper = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<reflect.Kind, encHelper>{reflect.Bool:encBoolSlice,reflect.Complex64:encComplex64Slice,reflect.Complex128:encComplex128Slice,reflect.Float32:encFloat32Slice,reflect.Float64:encFloat64Slice,reflect.Int:encIntSlice,reflect.Int16:encInt16Slice,reflect.Int32:encInt32Slice,reflect.Int64:encInt64Slice,reflect.Int8:encInt8Slice,reflect.String:encStringSlice,reflect.Uint:encUintSlice,reflect.Uint16:encUint16Slice,reflect.Uint32:encUint32Slice,reflect.Uint64:encUint64Slice,reflect.Uintptr:encUintptrSlice,};

        private static bool encBoolArray(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encBoolSlice(state, v.Slice(0L, v.Len()));
        }

        private static bool encBoolSlice(ref encoderState state, reflect.Value v)
        {
            slice<bool> (slice, ok) = v.Interface()._<slice<bool>>();
            if (!ok)
            { 
                // It is kind bool but not type bool. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != false || state.sendZero)
                {
                    if (x)
                    {
                        state.encodeUint(1L);
                    }
                    else
                    {
                        state.encodeUint(0L);
                    }
                }
            }
            return true;
        }

        private static bool encComplex64Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encComplex64Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encComplex64Slice(ref encoderState state, reflect.Value v)
        {
            slice<complex64> (slice, ok) = v.Interface()._<slice<complex64>>();
            if (!ok)
            { 
                // It is kind complex64 but not type complex64. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L + 0iUL || state.sendZero)
                {
                    var rpart = floatBits(float64(real(x)));
                    var ipart = floatBits(float64(imag(x)));
                    state.encodeUint(rpart);
                    state.encodeUint(ipart);
                }
            }
            return true;
        }

        private static bool encComplex128Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encComplex128Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encComplex128Slice(ref encoderState state, reflect.Value v)
        {
            slice<System.Numerics.Complex128> (slice, ok) = v.Interface()._<slice<System.Numerics.Complex128>>();
            if (!ok)
            { 
                // It is kind complex128 but not type complex128. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L + 0iUL || state.sendZero)
                {
                    var rpart = floatBits(real(x));
                    var ipart = floatBits(imag(x));
                    state.encodeUint(rpart);
                    state.encodeUint(ipart);
                }
            }
            return true;
        }

        private static bool encFloat32Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encFloat32Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encFloat32Slice(ref encoderState state, reflect.Value v)
        {
            slice<float> (slice, ok) = v.Interface()._<slice<float>>();
            if (!ok)
            { 
                // It is kind float32 but not type float32. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    var bits = floatBits(float64(x));
                    state.encodeUint(bits);
                }
            }
            return true;
        }

        private static bool encFloat64Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encFloat64Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encFloat64Slice(ref encoderState state, reflect.Value v)
        {
            slice<double> (slice, ok) = v.Interface()._<slice<double>>();
            if (!ok)
            { 
                // It is kind float64 but not type float64. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    var bits = floatBits(x);
                    state.encodeUint(bits);
                }
            }
            return true;
        }

        private static bool encIntArray(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encIntSlice(state, v.Slice(0L, v.Len()));
        }

        private static bool encIntSlice(ref encoderState state, reflect.Value v)
        {
            slice<long> (slice, ok) = v.Interface()._<slice<long>>();
            if (!ok)
            { 
                // It is kind int but not type int. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeInt(int64(x));
                }
            }
            return true;
        }

        private static bool encInt16Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encInt16Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encInt16Slice(ref encoderState state, reflect.Value v)
        {
            slice<short> (slice, ok) = v.Interface()._<slice<short>>();
            if (!ok)
            { 
                // It is kind int16 but not type int16. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeInt(int64(x));
                }
            }
            return true;
        }

        private static bool encInt32Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encInt32Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encInt32Slice(ref encoderState state, reflect.Value v)
        {
            slice<int> (slice, ok) = v.Interface()._<slice<int>>();
            if (!ok)
            { 
                // It is kind int32 but not type int32. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeInt(int64(x));
                }
            }
            return true;
        }

        private static bool encInt64Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encInt64Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encInt64Slice(ref encoderState state, reflect.Value v)
        {
            slice<long> (slice, ok) = v.Interface()._<slice<long>>();
            if (!ok)
            { 
                // It is kind int64 but not type int64. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeInt(x);
                }
            }
            return true;
        }

        private static bool encInt8Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encInt8Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encInt8Slice(ref encoderState state, reflect.Value v)
        {
            slice<sbyte> (slice, ok) = v.Interface()._<slice<sbyte>>();
            if (!ok)
            { 
                // It is kind int8 but not type int8. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeInt(int64(x));
                }
            }
            return true;
        }

        private static bool encStringArray(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encStringSlice(state, v.Slice(0L, v.Len()));
        }

        private static bool encStringSlice(ref encoderState state, reflect.Value v)
        {
            slice<@string> (slice, ok) = v.Interface()._<slice<@string>>();
            if (!ok)
            { 
                // It is kind string but not type string. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != "" || state.sendZero)
                {
                    state.encodeUint(uint64(len(x)));
                    state.b.WriteString(x);
                }
            }
            return true;
        }

        private static bool encUintArray(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encUintSlice(state, v.Slice(0L, v.Len()));
        }

        private static bool encUintSlice(ref encoderState state, reflect.Value v)
        {
            slice<ulong> (slice, ok) = v.Interface()._<slice<ulong>>();
            if (!ok)
            { 
                // It is kind uint but not type uint. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeUint(uint64(x));
                }
            }
            return true;
        }

        private static bool encUint16Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encUint16Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encUint16Slice(ref encoderState state, reflect.Value v)
        {
            slice<ushort> (slice, ok) = v.Interface()._<slice<ushort>>();
            if (!ok)
            { 
                // It is kind uint16 but not type uint16. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeUint(uint64(x));
                }
            }
            return true;
        }

        private static bool encUint32Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encUint32Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encUint32Slice(ref encoderState state, reflect.Value v)
        {
            slice<uint> (slice, ok) = v.Interface()._<slice<uint>>();
            if (!ok)
            { 
                // It is kind uint32 but not type uint32. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeUint(uint64(x));
                }
            }
            return true;
        }

        private static bool encUint64Array(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encUint64Slice(state, v.Slice(0L, v.Len()));
        }

        private static bool encUint64Slice(ref encoderState state, reflect.Value v)
        {
            slice<ulong> (slice, ok) = v.Interface()._<slice<ulong>>();
            if (!ok)
            { 
                // It is kind uint64 but not type uint64. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeUint(x);
                }
            }
            return true;
        }

        private static bool encUintptrArray(ref encoderState state, reflect.Value v)
        { 
            // Can only slice if it is addressable.
            if (!v.CanAddr())
            {
                return false;
            }
            return encUintptrSlice(state, v.Slice(0L, v.Len()));
        }

        private static bool encUintptrSlice(ref encoderState state, reflect.Value v)
        {
            slice<System.UIntPtr> (slice, ok) = v.Interface()._<slice<System.UIntPtr>>();
            if (!ok)
            { 
                // It is kind uintptr but not type uintptr. TODO: We can handle this unsafely.
                return false;
            }
            foreach (var (_, x) in slice)
            {
                if (x != 0L || state.sendZero)
                {
                    state.encodeUint(uint64(x));
                }
            }
            return true;
        }
    }
}}
