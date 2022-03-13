// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 13 06:27:35 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\sizes.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;

using types = cmd.compile.@internal.types_package;
using types2 = cmd.compile.@internal.types2_package;


// Code below based on go/types.StdSizes.
// Intentional differences are marked with "gc:".

public static partial class noder_package {

private partial struct gcSizes {
}

private static long Alignof(this ptr<gcSizes> _addr_s, types2.Type T) {
    ref gcSizes s = ref _addr_s.val;
 
    // For arrays and structs, alignment is defined in terms
    // of alignment of the elements and fields, respectively.
    switch (T.Underlying().type()) {
        case ptr<types2.Array> t:
            return s.Alignof(t.Elem());
            break;
        case ptr<types2.Struct> t:
            var max = int64(1);
            for (nint i = 0;
            var nf = t.NumFields(); i < nf; i++) {
                {
                    var a__prev1 = a;

                    var a = s.Alignof(t.Field(i).Type());

                    if (a > max) {
                        max = a;
                    }

                    a = a__prev1;

                }
            }

            return max;
            break;
        case ptr<types2.Slice> t:
            return int64(types.PtrSize);
            break;
        case ptr<types2.Interface> t:
            return int64(types.PtrSize);
            break;
        case ptr<types2.Basic> t:
            if (t.Info() & types2.IsString != 0) {
                return int64(types.PtrSize);
            }
            break;
    }
    a = s.Sizeof(T); // may be 0
    // spec: "For a variable x of any type: unsafe.Alignof(x) is at least 1."
    if (a < 1) {
        return 1;
    }
    if (isComplex(T)) {
        a /= 2;
    }
    if (a > int64(types.RegSize)) {
        return int64(types.RegSize);
    }
    return a;
}

private static bool isComplex(types2.Type T) {
    ptr<types2.Basic> (basic, ok) = T.Underlying()._<ptr<types2.Basic>>();
    return ok && basic.Info() & types2.IsComplex != 0;
}

private static slice<long> Offsetsof(this ptr<gcSizes> _addr_s, slice<ptr<types2.Var>> fields) {
    ref gcSizes s = ref _addr_s.val;

    var offsets = make_slice<long>(len(fields));
    long o = default;
    foreach (var (i, f) in fields) {
        var typ = f.Type();
        var a = s.Alignof(typ);
        o = types.Rnd(o, a);
        offsets[i] = o;
        o += s.Sizeof(typ);
    }    return offsets;
}

private static long Sizeof(this ptr<gcSizes> _addr_s, types2.Type T) => func((_, panic, _) => {
    ref gcSizes s = ref _addr_s.val;

    switch (T.Underlying().type()) {
        case ptr<types2.Basic> t:
            var k = t.Kind();
            if (int(k) < len(basicSizes)) {
                {
                    var s = basicSizes[k];

                    if (s > 0) {
                        return int64(s);
                    }

                }
            }

            if (k == types2.String) 
                return int64(types.PtrSize) * 2;
            else if (k == types2.Int || k == types2.Uint || k == types2.Uintptr || k == types2.UnsafePointer) 
                return int64(types.PtrSize);
                        panic(fmt.Sprintf("unimplemented basic: %v (kind %v)", T, k));
            break;
        case ptr<types2.Array> t:
            var n = t.Len();
            if (n <= 0) {
                return 0;
            } 
            // n > 0
            // gc: Size includes alignment padding.
            return s.Sizeof(t.Elem()) * n;
            break;
        case ptr<types2.Slice> t:
            return int64(types.PtrSize) * 3;
            break;
        case ptr<types2.Struct> t:
            n = t.NumFields();
            if (n == 0) {
                return 0;
            }
            var fields = make_slice<ptr<types2.Var>>(n);
            foreach (var (i) in fields) {
                fields[i] = t.Field(i);
            }
            var offsets = s.Offsetsof(fields); 

            // gc: The last field of a non-zero-sized struct is not allowed to
            // have size 0.
            var last = s.Sizeof(fields[n - 1].Type());
            if (last == 0 && offsets[n - 1] > 0) {
                last = 1;
            } 

            // gc: Size includes alignment padding.
            return types.Rnd(offsets[n - 1] + last, s.Alignof(t));
            break;
        case ptr<types2.Interface> t:
            return int64(types.PtrSize) * 2;
            break;
        case ptr<types2.Chan> t:
            return int64(types.PtrSize);
            break;
        case ptr<types2.Map> t:
            return int64(types.PtrSize);
            break;
        case ptr<types2.Pointer> t:
            return int64(types.PtrSize);
            break;
        case ptr<types2.Signature> t:
            return int64(types.PtrSize);
            break;
        default:
        {
            var t = T.Underlying().type();
            panic(fmt.Sprintf("unimplemented type: %T", t));
            break;
        }
    }
});

private static array<byte> basicSizes = new array<byte>(InitKeyedValues<byte>((types2.Bool, 1), (types2.Int8, 1), (types2.Int16, 2), (types2.Int32, 4), (types2.Int64, 8), (types2.Uint8, 1), (types2.Uint16, 2), (types2.Uint32, 4), (types2.Uint64, 8), (types2.Float32, 4), (types2.Float64, 8), (types2.Complex64, 8), (types2.Complex128, 16)));

} // end noder_package
