// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using iter = iter_package;
using abi = @internal.abi_package;

partial class reflect_package {

internal static iter.Seq<ΔValue> rangeNum<T, N>(N v)
    where T : /* int8 | int16 | int32 | int64 | int | uint8 | uint16 | uint32 | uint64 | uint | uintptr */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
    where N : /* int64 | uint64 */ IAdditionOperators<N, N, N>, ISubtractionOperators<N, N, N>, IMultiplyOperators<N, N, N>, IDivisionOperators<N, N, N>, IIncrementOperators<N>, IDecrementOperators<N>, IModulusOperators<N, N, N>, IBitwiseOperators<N, N, N>, IShiftOperators<N, int, N>, IEqualityOperators<N, N, bool>, IComparisonOperators<N, N, bool>, new()
{
    return (Func<ΔValue, bool> yield) => {
        // cannot use range T(v) because no core type.
        for (var i = ConvertToType<T>(0); i < ConvertToType<T>(ConvertToUInt64<N>(v)); i++) {
            if (!yield(ValueOf(i))) {
                return;
            }
        }
    };
}

// Seq returns an iter.Seq[Value] that loops over the elements of v.
// If v's kind is Func, it must be a function that has no results and
// that takes a single argument of type func(T) bool for some type T.
// If v's kind is Pointer, the pointer element type must have kind Array.
// Otherwise v's kind must be Int, Int8, Int16, Int32, Int64,
// Uint, Uint8, Uint16, Uint32, Uint64, Uintptr,
// Array, Chan, Map, Slice, or String.
public static iter.Seq<ΔValue> Seq(this ΔValue v) {
    if (canRangeFunc(v.typ())) {
        return (Func<ΔValue, bool> yield) => {
            ref var rf = ref heap<ΔValue>(out var Ꮡrf);
            rf = MakeFunc(v.Type().In(0), (slice<ΔValue> @in) => new ΔValue[]{ValueOf(yield(@in[0]))}.slice());
            v.Call(new ΔValue[]{rf}.slice());
        };
    }
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == ΔInt) {
        return rangeNum<nint, int64>(v.Int());
    }
    if (exprᴛ1 == Int8) {
        return rangeNum<int8, int64>(v.Int());
    }
    if (exprᴛ1 == Int16) {
        return rangeNum<int16, int64>(v.Int());
    }
    if (exprᴛ1 == Int32) {
        return rangeNum<int32, int64>(v.Int());
    }
    if (exprᴛ1 == Int64) {
        return rangeNum<int64, int64>(v.Int());
    }
    if (exprᴛ1 == ΔUint) {
        return rangeNum<nuint, uint64>(v.Uint());
    }
    if (exprᴛ1 == Uint8) {
        return rangeNum<uint8, uint64>(v.Uint());
    }
    if (exprᴛ1 == Uint16) {
        return rangeNum<uint16, uint64>(v.Uint());
    }
    if (exprᴛ1 == Uint32) {
        return rangeNum<uint32, uint64>(v.Uint());
    }
    if (exprᴛ1 == Uint64) {
        return rangeNum<uint64, uint64>(v.Uint());
    }
    if (exprᴛ1 == Uintptr) {
        return rangeNum<uintptr, uint64>(v.Uint());
    }
    if (exprᴛ1 == ΔPointer) {
        do {
            if (v.Elem().kind() != Array) {
                break;
            }
            return (Func<ΔValue, bool> yield) => {
                v = v.Elem();
                foreach (var i in range(v.Len())) {
                    if (!yield(ValueOf(i))) {
                        return;
                    }
                }
            };
        } while (false);
    }
    if (exprᴛ1 == Array || exprᴛ1 == ΔSlice) {
        return (Func<ΔValue, bool> yield) => {
            foreach (var i in range(v.Len())) {
                if (!yield(ValueOf(i))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == ΔString) {
        return (Func<ΔValue, bool> yield) => {
            foreach (var (i, _) in v.String()) {
                if (!yield(ValueOf(i))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == Map) {
        return (Func<ΔValue, bool> yield) => {
            var i = v.MapRange();
            while (i.Next()) {
                if (!yield(i.Key())) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == Chan) {
        return (Func<ΔValue, bool> yield) => {
            for (var (valueᴛ1, ok) = v.Recv(); ok; (valueᴛ1, ok) = v.Recv()) {
                ref var value = ref heap<ΔValue>(out var Ꮡvalue);
                value = valueᴛ1;
                if (!yield(value)) {
                    return;
                }
                valueᴛ1 = value;
            }
        };
    }

    throw panic("reflect: " + v.Type().String() + " cannot produce iter.Seq[Value]");
}

// Seq2 returns an iter.Seq2[Value, Value] that loops over the elements of v.
// If v's kind is Func, it must be a function that has no results and
// that takes a single argument of type func(K, V) bool for some type K, V.
// If v's kind is Pointer, the pointer element type must have kind Array.
// Otherwise v's kind must be Array, Map, Slice, or String.
public static iter.Seq2<ΔValue, ΔValue> Seq2(this ΔValue v) {
    if (canRangeFunc2(v.typ())) {
        return (Func<ΔValue, ΔValue, bool> yield) => {
            ref var rf = ref heap<ΔValue>(out var Ꮡrf);
            rf = MakeFunc(v.Type().In(0), (slice<ΔValue> @in) => new ΔValue[]{ValueOf(yield(@in[0], @in[1]))}.slice());
            v.Call(new ΔValue[]{rf}.slice());
        };
    }
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == ΔPointer) {
        do {
            if (v.Elem().kind() != Array) {
                break;
            }
            return (Func<ΔValue, ΔValue, bool> yield) => {
                v = v.Elem();
                foreach (var i in range(v.Len())) {
                    if (!yield(ValueOf(i), v.Index(i))) {
                        return;
                    }
                }
            };
        } while (false);
    }
    if (exprᴛ1 == Array || exprᴛ1 == ΔSlice) {
        return (Func<ΔValue, ΔValue, bool> yield) => {
            foreach (var i in range(v.Len())) {
                if (!yield(ValueOf(i), v.Index(i))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == ΔString) {
        return (Func<ΔValue, ΔValue, bool> yield) => {
            foreach (var (i, vΔ2) in v.String()) {
                if (!yield(ValueOf(i), ValueOf(vΔ2))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == Map) {
        return (Func<ΔValue, ΔValue, bool> yield) => {
            var i = v.MapRange();
            while (i.Next()) {
                if (!yield(i.Key(), i.Value())) {
                    return;
                }
            }
        };
    }

    throw panic("reflect: " + v.Type().String() + " cannot produce iter.Seq2[Value, Value]");
}

} // end reflect_package
