// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

namespace go.@internal;

//using goarch = @internal.goarch_package;
//using unsafeheader = @internal.unsafeheader_package;
//using @unsafe = unsafe_package;

partial class reflectlite_package {

// Swapper returns a function that swaps the elements in the provided
// slice.
//
// Swapper panics if the provided interface is not a slice.
public static Action<nint, nint> Swapper(any Δslice) {
    var v = ValueOf(Δslice);

    if (v.Kind() != Slice) {
        throw panic(Ꮡ(new ValueError(Method: "Swapper"u8, Kind: v.Kind())));
    }

    // Fast path for slices of size 0 and 1. Nothing to swap.
    switch (v.Len()) {
    case 0: {
        return (nint i, nint j) => {
            throw panic("reflect: slice index out of range");
        };
    }
    case 1: {
        return (nint i, nint j) => {
            if (i != 0 || j != 0) {
                throw panic("reflect: slice index out of range");
            }
        };
    }}

    ISlice @is = (v.Target as ISlice)!;
    ISlice isʗ1 = @is;

    return (i, j) => {
        (isʗ1[i], isʗ1[j]) = (isʗ1[j], isʗ1[i]);
    };

    /*
    var typ = v.Type().Elem().common();
    var size = typ.Size();
    var hasPtr = typ.Pointers();
    // Some common & small cases, without using memmove:
    if (hasPtr){
        if (size == goarch.PtrSize) {
            var ps = ~(ж<slice<@unsafe.Pointer>>)(uintptr)(v.ptr);
            var psʗ1 = ps;
            return (nint i, nint j) => {
                (psʗ1[i], psʗ1[j]) = (psʗ1[j], psʗ1[i]);
            };
        }
        if (typ.Kind() == ΔString) {
            var ss = ~(ж<slice<@string>>)(uintptr)(v.ptr);
            var ssʗ1 = ss;
            return (nint i, nint j) => {
                (ssʗ1[i], ssʗ1[j]) = (ssʗ1[j], ssʗ1[i]);
            };
        }
    } else {
        switch (size) {
        case 8: {
            var @is = ~(ж<slice<int64>>)(uintptr)(v.ptr);
            var isʗ1 = @is;
            return (nint i, nint j) => {
                (isʗ1[i], isʗ1[j]) = (isʗ1[j], isʗ1[i]);
            };
        }
        case 4: {
            var @is = ~(ж<slice<int32>>)(uintptr)(v.ptr);
            var isʗ2 = @is;
            return (nint i, nint j) => {
                (isʗ2[i], isʗ2[j]) = (isʗ2[j], isʗ2[i]);
            };
        }
        case 2: {
            var @is = ~(ж<slice<int16>>)(uintptr)(v.ptr);
            var isʗ3 = @is;
            return (nint i, nint j) => {
                (isʗ3[i], isʗ3[j]) = (isʗ3[j], isʗ3[i]);
            };
        }
        case 1: {
            var @is = ~(ж<slice<int8>>)(uintptr)(v.ptr);
            var isʗ4 = @is;
            return (nint i, nint j) => {
                (isʗ4[i], isʗ4[j]) = (isʗ4[j], isʗ4[i]);
            };
        }}

    }
    var s = (ж<unsafeheader.Slice>)(uintptr)(v.ptr);
    var tmp = (uintptr)unsafe_New(typ);
    // swap scratch space
    var sʗ1 = s;
    var typʗ1 = typ;
    return (nint i, nint j) => {
        if (((nuint)i) >= ((nuint)(~sʗ1).Len) || ((nuint)j) >= ((nuint)(~sʗ1).Len)) {
            throw panic("reflect: slice index out of range");
        }
        var val1 = (uintptr)arrayAt((~sʗ1).Data, i, size, "i < s.Len"u8);
        var val2 = (uintptr)arrayAt((~sʗ1).Data, j, size, "j < s.Len"u8);
        typedmemmove(typʗ1, tmp, val1);
        typedmemmove(typʗ1, val1, val2);
        typedmemmove(typʗ1, val2, tmp);
    };
    */
}

} // end reflectlite_package
