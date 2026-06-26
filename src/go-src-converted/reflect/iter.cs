// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using iter = iter_package;

partial class reflect_package {

internal static iter.Seq<ΔValue> rangeNum<T, N>(N v)
    where T : /* int8 | int16 | int32 | int64 | int | uint8 | uint16 | uint32 | uint64 | uint | uintptr */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
    where N : /* int64 | uint64 */ IAdditionOperators<N, N, N>, ISubtractionOperators<N, N, N>, IMultiplyOperators<N, N, N>, IDivisionOperators<N, N, N>, IModulusOperators<N, N, N>, IBitwiseOperators<N, N, N>, IShiftOperators<N, N, N>, IEqualityOperators<N, N, bool>, IComparisonOperators<N, N, bool>, new()
{
    return (Func<ΔValue, bool> yield) => {
        // cannot use range T(v) because no core type.
        for (var i = ((T)0); i < ((T)v); i++) {
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
        var vʗ1 = v;
        return (Func<ΔValue, bool> yield) => {
            ref var rf = ref heap<ΔValue>(out var Ꮡrf);
            rf = MakeFunc(vʗ1.Type().In(0), 
            (slice<ΔValue> @in) => new ΔValue[]{ValueOf(yield(@in[0])));
            v.Call(new ΔValue[]{rf}.slice());
        };
    }
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == ΔInt) {
        return rangeNum<nint><ΔValue>(v.Int());
    }
    if (exprᴛ1 == Int8) {
        return rangeNum<int8><ΔValue>(v.Int());
    }
    if (exprᴛ1 == Int16) {
        return rangeNum<int16><ΔValue>(v.Int());
    }
    if (exprᴛ1 == Int32) {
        return rangeNum<int32><ΔValue>(v.Int());
    }
    if (exprᴛ1 == Int64) {
        return rangeNum<int64><ΔValue>(v.Int());
    }
    if (exprᴛ1 == ΔUint) {
        return rangeNum<nuint><ΔValue>(v.Uint());
    }
    if (exprᴛ1 == Uint8) {
        return rangeNum<uint8><ΔValue>(v.Uint());
    }
    if (exprᴛ1 == Uint16) {
        return rangeNum<uint16><ΔValue>(v.Uint());
    }
    if (exprᴛ1 == Uint32) {
        return rangeNum<uint32><ΔValue>(v.Uint());
    }
    if (exprᴛ1 == Uint64) {
        return rangeNum<uint64><ΔValue>(v.Uint());
    }
    if (exprᴛ1 == Uintptr) {
        return rangeNum<uintptr><ΔValue>(v.Uint());
    }
    if (exprᴛ1 == ΔPointer) {
        if (v.Elem().kind() != Array) {
            break;
        }
        var vʗ2 = v;
        return (Func<ΔValue, bool> yield) => {
            vʗ2 = vʗ2.Elem();
            foreach (var i in range(vʗ2.Len())) {
                if (!yield(ValueOf(i))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == Array || exprᴛ1 == ΔSlice) {
        var vʗ3 = v;
        return (Func<ΔValue, bool> yield) => {
            foreach (var i in range(vʗ3.Len())) {
                if (!yield(ValueOf(i))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == ΔString) {
        var vʗ4 = v;
        return (Func<ΔValue, bool> yield) => {
            foreach (var (i, _) in vʗ4.String()) {
                if (!yield(ValueOf(i))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == Map) {
        var vʗ5 = v;
        return (Func<ΔValue, bool> yield) => {
            var i = vʗ5.MapRange();
            while (i.Next()) {
                if (!yield(i.Key())) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == Chan) {
        var vʗ6 = v;
        return (Func<ΔValue, bool> yield) => {
            for (var (value, ok) = vʗ6.Recv(); ok; (value, ok) = vʗ6.Recv()) {
                if (!yield(value)) {
                    return;
                }
            }
        };
    }

    throw panic("reflect: "u8 + v.Type().String() + " cannot produce iter.Seq[Value]"u8);
}

// Seq2 returns an iter.Seq2[Value, Value] that loops over the elements of v.
// If v's kind is Func, it must be a function that has no results and
// that takes a single argument of type func(K, V) bool for some type K, V.
// If v's kind is Pointer, the pointer element type must have kind Array.
// Otherwise v's kind must be Array, Map, Slice, or String.
public static iter.Seq2<ΔValue, reflect.Value> Seq2(this ΔValue v) {
    if (canRangeFunc2(v.typ())) {
        var vʗ1 = v;
        return (Func<ΔValue, reflect.Value, bool> yield) => {
            ref var rf = ref heap<ΔValue>(out var Ꮡrf);
            rf = MakeFunc(vʗ1.Type().In(0), 
            (slice<ΔValue> @in) => new ΔValue[]{ValueOf(yield(@in[0], @in[1])));
            v.Call(new ΔValue[]{rf}.slice());
        };
    }
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == ΔPointer) {
        if (v.Elem().kind() != Array) {
            break;
        }
        var vʗ2 = v;
        return (Func<ΔValue, reflect.Value, bool> yield) => {
            vʗ2 = vʗ2.Elem();
            foreach (var i in range(vʗ2.Len())) {
                if (!yield(ValueOf(i), vʗ2.Index(i))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == Array || exprᴛ1 == ΔSlice) {
        var vʗ3 = v;
        return (Func<ΔValue, reflect.Value, bool> yield) => {
            foreach (var i in range(vʗ3.Len())) {
                if (!yield(ValueOf(i), vʗ3.Index(i))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == ΔString) {
        var vʗ4 = v;
        return (Func<ΔValue, reflect.Value, bool> yield) => {
            foreach (var (i, vʗ4) in vʗ4.String()) {
                if (!yield(ValueOf(i), ValueOf(vʗ4))) {
                    return;
                }
            }
        };
    }
    if (exprᴛ1 == Map) {
        var vʗ5 = v;
        return (Func<ΔValue, reflect.Value, bool> yield) => {
            var i = vʗ5.MapRange();
            while (i.Next()) {
                if (!yield(i.Key(), i.Value())) {
                    return;
                }
            }
        };
    }

    throw panic("reflect: "u8 + v.Type().String() + " cannot produce iter.Seq2[Value, Value]"u8);
}

} // end reflect_package
