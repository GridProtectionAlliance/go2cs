// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typebits -- go2cs converted at 2022 March 06 23:09:20 UTC
// import "cmd/compile/internal/typebits" ==> using typebits = go.cmd.compile.@internal.typebits_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typebits\typebits.go
using @base = go.cmd.compile.@internal.@base_package;
using bitvec = go.cmd.compile.@internal.bitvec_package;
using types = go.cmd.compile.@internal.types_package;

namespace go.cmd.compile.@internal;

public static partial class typebits_package {

    // NOTE: The bitmap for a specific type t could be cached in t after
    // the first run and then simply copied into bv at the correct offset
    // on future calls with the same type t.
public static void Set(ptr<types.Type> _addr_t, long off, bitvec.BitVec bv) {
    ref types.Type t = ref _addr_t.val;

    if (t.Align > 0 && off & int64(t.Align - 1) != 0) {
        @base.Fatalf("typebits.Set: invalid initial alignment: type %v has alignment %d, but offset is %v", t, t.Align, off);
    }
    if (!t.HasPointers()) { 
        // Note: this case ensures that pointers to go:notinheap types
        // are not considered pointers by garbage collection and stack copying.
        return ;

    }

    if (t.Kind() == types.TPTR || t.Kind() == types.TUNSAFEPTR || t.Kind() == types.TFUNC || t.Kind() == types.TCHAN || t.Kind() == types.TMAP) 
        if (off & int64(types.PtrSize - 1) != 0) {
            @base.Fatalf("typebits.Set: invalid alignment, %v", t);
        }
        bv.Set(int32(off / int64(types.PtrSize))); // pointer
    else if (t.Kind() == types.TSTRING) 
        // struct { byte *str; intgo len; }
        if (off & int64(types.PtrSize - 1) != 0) {
            @base.Fatalf("typebits.Set: invalid alignment, %v", t);
        }
        bv.Set(int32(off / int64(types.PtrSize))); //pointer in first slot
    else if (t.Kind() == types.TINTER) 
        // struct { Itab *tab;    void *data; }
        // or, when isnilinter(t)==true:
        // struct { Type *type; void *data; }
        if (off & int64(types.PtrSize - 1) != 0) {
            @base.Fatalf("typebits.Set: invalid alignment, %v", t);
        }
        bv.Set(int32(off / int64(types.PtrSize) + 1)); // pointer in second slot
    else if (t.Kind() == types.TSLICE) 
        // struct { byte *array; uintgo len; uintgo cap; }
        if (off & int64(types.PtrSize - 1) != 0) {
            @base.Fatalf("typebits.Set: invalid TARRAY alignment, %v", t);
        }
        bv.Set(int32(off / int64(types.PtrSize))); // pointer in first slot (BitsPointer)
    else if (t.Kind() == types.TARRAY) 
        var elt = t.Elem();
        if (elt.Width == 0) { 
            // Short-circuit for #20739.
            break;

        }
        for (var i = int64(0); i < t.NumElem(); i++) {
            Set(_addr_elt, off, bv);
            off += elt.Width;
        }
    else if (t.Kind() == types.TSTRUCT) 
        foreach (var (_, f) in t.Fields().Slice()) {
            Set(_addr_f.Type, off + f.Offset, bv);
        }    else 
        @base.Fatalf("typebits.Set: unexpected type, %v", t);
    
}

} // end typebits_package
