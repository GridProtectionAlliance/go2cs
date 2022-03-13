// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:01:10 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\decompose.go
namespace go.cmd.compile.@internal;

using types = cmd.compile.@internal.types_package;
using sort = sort_package;


// decompose converts phi ops on compound builtin types into phi
// ops on simple types, then invokes rewrite rules to decompose
// other ops on those types.

using System;
public static partial class ssa_package {

private static void decomposeBuiltIn(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // Decompose phis
    foreach (var (_, b) in f.Blocks) {
        {
            var v__prev2 = v;

            foreach (var (_, __v) in b.Values) {
                v = __v;
                if (v.Op != OpPhi) {
                    continue;
                }
                decomposeBuiltInPhi(_addr_v);
            }
            v = v__prev2;
        }
    }    applyRewrite(f, rewriteBlockdec, rewriteValuedec, leaveDeadValues);
    if (f.Config.RegSize == 4) {
        applyRewrite(f, rewriteBlockdec64, rewriteValuedec64, leaveDeadValues);
    }
    slice<namedVal> toDelete = default;
    slice<ptr<LocalSlot>> newNames = default;
    foreach (var (i, name) in f.Names) {
        var t = name.Type;

        if (t.IsInteger() && t.Size() > f.Config.RegSize) 
            var (hiName, loName) = f.SplitInt64(name);
            newNames = maybeAppend2(_addr_f, newNames, _addr_hiName, _addr_loName);
            {
                var j__prev2 = j;
                var v__prev2 = v;

                foreach (var (__j, __v) in f.NamedValues[name.val]) {
                    j = __j;
                    v = __v;
                    if (v.Op != OpInt64Make) {
                        continue;
                    }
                    f.NamedValues[hiName.val] = append(f.NamedValues[hiName.val], v.Args[0]);
                    f.NamedValues[loName.val] = append(f.NamedValues[loName.val], v.Args[1]);
                    toDelete = append(toDelete, new namedVal(i,j));
                }
                j = j__prev2;
                v = v__prev2;
            }
        else if (t.IsComplex()) 
            var (rName, iName) = f.SplitComplex(name);
            newNames = maybeAppend2(_addr_f, newNames, _addr_rName, _addr_iName);
            {
                var j__prev2 = j;
                var v__prev2 = v;

                foreach (var (__j, __v) in f.NamedValues[name.val]) {
                    j = __j;
                    v = __v;
                    if (v.Op != OpComplexMake) {
                        continue;
                    }
                    f.NamedValues[rName.val] = append(f.NamedValues[rName.val], v.Args[0]);
                    f.NamedValues[iName.val] = append(f.NamedValues[iName.val], v.Args[1]);
                    toDelete = append(toDelete, new namedVal(i,j));
                }
                j = j__prev2;
                v = v__prev2;
            }
        else if (t.IsString()) 
            var (ptrName, lenName) = f.SplitString(name);
            newNames = maybeAppend2(_addr_f, newNames, _addr_ptrName, _addr_lenName);
            {
                var j__prev2 = j;
                var v__prev2 = v;

                foreach (var (__j, __v) in f.NamedValues[name.val]) {
                    j = __j;
                    v = __v;
                    if (v.Op != OpStringMake) {
                        continue;
                    }
                    f.NamedValues[ptrName.val] = append(f.NamedValues[ptrName.val], v.Args[0]);
                    f.NamedValues[lenName.val] = append(f.NamedValues[lenName.val], v.Args[1]);
                    toDelete = append(toDelete, new namedVal(i,j));
                }
                j = j__prev2;
                v = v__prev2;
            }
        else if (t.IsSlice()) 
            var (ptrName, lenName, capName) = f.SplitSlice(name);
            newNames = maybeAppend2(_addr_f, newNames, _addr_ptrName, _addr_lenName);
            newNames = maybeAppend(_addr_f, newNames, _addr_capName);
            {
                var j__prev2 = j;
                var v__prev2 = v;

                foreach (var (__j, __v) in f.NamedValues[name.val]) {
                    j = __j;
                    v = __v;
                    if (v.Op != OpSliceMake) {
                        continue;
                    }
                    f.NamedValues[ptrName.val] = append(f.NamedValues[ptrName.val], v.Args[0]);
                    f.NamedValues[lenName.val] = append(f.NamedValues[lenName.val], v.Args[1]);
                    f.NamedValues[capName.val] = append(f.NamedValues[capName.val], v.Args[2]);
                    toDelete = append(toDelete, new namedVal(i,j));
                }
                j = j__prev2;
                v = v__prev2;
            }
        else if (t.IsInterface()) 
            var (typeName, dataName) = f.SplitInterface(name);
            newNames = maybeAppend2(_addr_f, newNames, _addr_typeName, _addr_dataName);
            {
                var j__prev2 = j;
                var v__prev2 = v;

                foreach (var (__j, __v) in f.NamedValues[name.val]) {
                    j = __j;
                    v = __v;
                    if (v.Op != OpIMake) {
                        continue;
                    }
                    f.NamedValues[typeName.val] = append(f.NamedValues[typeName.val], v.Args[0]);
                    f.NamedValues[dataName.val] = append(f.NamedValues[dataName.val], v.Args[1]);
                    toDelete = append(toDelete, new namedVal(i,j));
                }
                j = j__prev2;
                v = v__prev2;
            }
        else if (t.IsFloat())         else if (t.Size() > f.Config.RegSize) 
            f.Fatalf("undecomposed named type %s %v", name, t);
            }    deleteNamedVals(_addr_f, toDelete);
    f.Names = append(f.Names, newNames);
}

private static slice<ptr<LocalSlot>> maybeAppend(ptr<Func> _addr_f, slice<ptr<LocalSlot>> ss, ptr<LocalSlot> _addr_s) {
    ref Func f = ref _addr_f.val;
    ref LocalSlot s = ref _addr_s.val;

    {
        var (_, ok) = f.NamedValues[s];

        if (!ok) {
            f.NamedValues[s] = null;
            return append(ss, s);
        }
    }
    return ss;
}

private static slice<ptr<LocalSlot>> maybeAppend2(ptr<Func> _addr_f, slice<ptr<LocalSlot>> ss, ptr<LocalSlot> _addr_s1, ptr<LocalSlot> _addr_s2) {
    ref Func f = ref _addr_f.val;
    ref LocalSlot s1 = ref _addr_s1.val;
    ref LocalSlot s2 = ref _addr_s2.val;

    return maybeAppend(_addr_f, maybeAppend(_addr_f, ss, _addr_s1), _addr_s2);
}

private static void decomposeBuiltInPhi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Type.IsInteger() && v.Type.Size() > v.Block.Func.Config.RegSize) 
        decomposeInt64Phi(_addr_v);
    else if (v.Type.IsComplex()) 
        decomposeComplexPhi(_addr_v);
    else if (v.Type.IsString()) 
        decomposeStringPhi(_addr_v);
    else if (v.Type.IsSlice()) 
        decomposeSlicePhi(_addr_v);
    else if (v.Type.IsInterface()) 
        decomposeInterfacePhi(_addr_v);
    else if (v.Type.IsFloat())     else if (v.Type.Size() > v.Block.Func.Config.RegSize) 
        v.Fatalf("undecomposed type %s", v.Type);
    }

private static void decomposeStringPhi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var types = _addr_v.Block.Func.Config.Types;
    var ptrType = types.BytePtr;
    var lenType = types.Int;

    var ptr = v.Block.NewValue0(v.Pos, OpPhi, ptrType);
    var len = v.Block.NewValue0(v.Pos, OpPhi, lenType);
    foreach (var (_, a) in v.Args) {
        ptr.AddArg(a.Block.NewValue1(v.Pos, OpStringPtr, ptrType, a));
        len.AddArg(a.Block.NewValue1(v.Pos, OpStringLen, lenType, a));
    }    v.reset(OpStringMake);
    v.AddArg(ptr);
    v.AddArg(len);
}

private static void decomposeSlicePhi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var types = _addr_v.Block.Func.Config.Types;
    var ptrType = v.Type.Elem().PtrTo();
    var lenType = types.Int;

    var ptr = v.Block.NewValue0(v.Pos, OpPhi, ptrType);
    var len = v.Block.NewValue0(v.Pos, OpPhi, lenType);
    var cap = v.Block.NewValue0(v.Pos, OpPhi, lenType);
    foreach (var (_, a) in v.Args) {
        ptr.AddArg(a.Block.NewValue1(v.Pos, OpSlicePtr, ptrType, a));
        len.AddArg(a.Block.NewValue1(v.Pos, OpSliceLen, lenType, a));
        cap.AddArg(a.Block.NewValue1(v.Pos, OpSliceCap, lenType, a));
    }    v.reset(OpSliceMake);
    v.AddArg(ptr);
    v.AddArg(len);
    v.AddArg(cap);
}

private static void decomposeInt64Phi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var cfgtypes = _addr_v.Block.Func.Config.Types;
    ptr<types.Type> partType;
    if (v.Type.IsSigned()) {
        partType = cfgtypes.Int32;
    }
    else
 {
        partType = cfgtypes.UInt32;
    }
    var hi = v.Block.NewValue0(v.Pos, OpPhi, partType);
    var lo = v.Block.NewValue0(v.Pos, OpPhi, cfgtypes.UInt32);
    foreach (var (_, a) in v.Args) {
        hi.AddArg(a.Block.NewValue1(v.Pos, OpInt64Hi, partType, a));
        lo.AddArg(a.Block.NewValue1(v.Pos, OpInt64Lo, cfgtypes.UInt32, a));
    }    v.reset(OpInt64Make);
    v.AddArg(hi);
    v.AddArg(lo);
}

private static void decomposeComplexPhi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var cfgtypes = _addr_v.Block.Func.Config.Types;
    ptr<types.Type> partType;
    {
        var z = v.Type.Size();

        switch (z) {
            case 8: 
                partType = cfgtypes.Float32;
                break;
            case 16: 
                partType = cfgtypes.Float64;
                break;
            default: 
                v.Fatalf("decomposeComplexPhi: bad complex size %d", z);
                break;
        }
    }

    var real = v.Block.NewValue0(v.Pos, OpPhi, partType);
    var imag = v.Block.NewValue0(v.Pos, OpPhi, partType);
    foreach (var (_, a) in v.Args) {
        real.AddArg(a.Block.NewValue1(v.Pos, OpComplexReal, partType, a));
        imag.AddArg(a.Block.NewValue1(v.Pos, OpComplexImag, partType, a));
    }    v.reset(OpComplexMake);
    v.AddArg(real);
    v.AddArg(imag);
}

private static void decomposeInterfacePhi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var uintptrType = v.Block.Func.Config.Types.Uintptr;
    var ptrType = v.Block.Func.Config.Types.BytePtr;

    var itab = v.Block.NewValue0(v.Pos, OpPhi, uintptrType);
    var data = v.Block.NewValue0(v.Pos, OpPhi, ptrType);
    foreach (var (_, a) in v.Args) {
        itab.AddArg(a.Block.NewValue1(v.Pos, OpITab, uintptrType, a));
        data.AddArg(a.Block.NewValue1(v.Pos, OpIData, ptrType, a));
    }    v.reset(OpIMake);
    v.AddArg(itab);
    v.AddArg(data);
}

private static void decomposeUser(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, v) in b.Values) {
            if (v.Op != OpPhi) {
                continue;
            }
            decomposeUserPhi(_addr_v);
        }
    }    nint i = 0;
    slice<ptr<LocalSlot>> newNames = default;
    foreach (var (_, name) in f.Names) {
        var t = name.Type;

        if (t.IsStruct()) 
            newNames = decomposeUserStructInto(_addr_f, _addr_name, newNames);
        else if (t.IsArray()) 
            newNames = decomposeUserArrayInto(_addr_f, _addr_name, newNames);
        else 
            f.Names[i] = name;
            i++;
            }    f.Names = f.Names[..(int)i];
    f.Names = append(f.Names, newNames);
}

// decomposeUserArrayInto creates names for the element(s) of arrays referenced
// by name where possible, and appends those new names to slots, which is then
// returned.
private static slice<ptr<LocalSlot>> decomposeUserArrayInto(ptr<Func> _addr_f, ptr<LocalSlot> _addr_name, slice<ptr<LocalSlot>> slots) {
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;

    var t = name.Type;
    if (t.NumElem() == 0) { 
        // TODO(khr): Not sure what to do here.  Probably nothing.
        // Names for empty arrays aren't important.
        return slots;
    }
    if (t.NumElem() != 1) { 
        // shouldn't get here due to CanSSA
        f.Fatalf("array not of size 1");
    }
    var elemName = f.SplitArray(name);
    slice<ptr<Value>> keep = default;
    foreach (var (_, v) in f.NamedValues[name]) {
        if (v.Op != OpArrayMake1) {
            keep = append(keep, v);
            continue;
        }
        f.NamedValues[elemName.val] = append(f.NamedValues[elemName.val], v.Args[0]);
    }    if (len(keep) == 0) { 
        // delete the name for the array as a whole
        delete(f.NamedValues, name);
    }
    else
 {
        f.NamedValues[name] = keep;
    }
    if (t.Elem().IsArray()) {
        return decomposeUserArrayInto(_addr_f, _addr_elemName, slots);
    }
    else if (t.Elem().IsStruct()) {
        return decomposeUserStructInto(_addr_f, _addr_elemName, slots);
    }
    return append(slots, elemName);
}

// decomposeUserStructInto creates names for the fields(s) of structs referenced
// by name where possible, and appends those new names to slots, which is then
// returned.
private static slice<ptr<LocalSlot>> decomposeUserStructInto(ptr<Func> _addr_f, ptr<LocalSlot> _addr_name, slice<ptr<LocalSlot>> slots) {
    ref Func f = ref _addr_f.val;
    ref LocalSlot name = ref _addr_name.val;

    ptr<LocalSlot> fnames = new slice<ptr<LocalSlot>>(new ptr<LocalSlot>[] {  }); // slots for struct in name
    var t = name.Type;
    var n = t.NumFields();

    {
        nint i__prev1 = i;

        for (nint i = 0; i < n; i++) {
            var fs = f.SplitStruct(name, i);
            fnames = append(fnames, fs); 
            // arrays and structs will be decomposed further, so
            // there's no need to record a name
            if (!fs.Type.IsArray() && !fs.Type.IsStruct()) {
                slots = maybeAppend(_addr_f, slots, _addr_fs);
            }
        }

        i = i__prev1;
    }

    var makeOp = StructMakeOp(n);
    slice<ptr<Value>> keep = default; 
    // create named values for each struct field
    foreach (var (_, v) in f.NamedValues[name]) {
        if (v.Op != makeOp) {
            keep = append(keep, v);
            continue;
        }
        {
            nint i__prev2 = i;

            for (i = 0; i < len(fnames); i++) {
                f.NamedValues[fnames[i].val] = append(f.NamedValues[fnames[i].val], v.Args[i]);
            }


            i = i__prev2;
        }
    }    if (len(keep) == 0) { 
        // delete the name for the struct as a whole
        delete(f.NamedValues, name);
    }
    else
 {
        f.NamedValues[name] = keep;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < n; i++) {
            if (name.Type.FieldType(i).IsStruct()) {
                slots = decomposeUserStructInto(_addr_f, _addr_fnames[i], slots);
                delete(f.NamedValues, fnames[i].val);
            }
            else if (name.Type.FieldType(i).IsArray()) {
                slots = decomposeUserArrayInto(_addr_f, _addr_fnames[i], slots);
                delete(f.NamedValues, fnames[i].val);
            }
        }

        i = i__prev1;
    }
    return slots;
}
private static void decomposeUserPhi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Type.IsStruct()) 
        decomposeStructPhi(_addr_v);
    else if (v.Type.IsArray()) 
        decomposeArrayPhi(_addr_v);
    }

// decomposeStructPhi replaces phi-of-struct with structmake(phi-for-each-field),
// and then recursively decomposes the phis for each field.
private static void decomposeStructPhi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var t = v.Type;
    var n = t.NumFields();
    array<ptr<Value>> fields = new array<ptr<Value>>(MaxStruct);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < n; i++) {
            fields[i] = v.Block.NewValue0(v.Pos, OpPhi, t.FieldType(i));
        }

        i = i__prev1;
    }
    foreach (var (_, a) in v.Args) {
        {
            nint i__prev2 = i;

            for (i = 0; i < n; i++) {
                fields[i].AddArg(a.Block.NewValue1I(v.Pos, OpStructSelect, t.FieldType(i), int64(i), a));
            }


            i = i__prev2;
        }
    }    v.reset(StructMakeOp(n));
    v.AddArgs(fields[..(int)n]); 

    // Recursively decompose phis for each field.
    foreach (var (_, f) in fields[..(int)n]) {
        decomposeUserPhi(_addr_f);
    }
}

// decomposeArrayPhi replaces phi-of-array with arraymake(phi-of-array-element),
// and then recursively decomposes the element phi.
private static void decomposeArrayPhi(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var t = v.Type;
    if (t.NumElem() == 0) {
        v.reset(OpArrayMake0);
        return ;
    }
    if (t.NumElem() != 1) {
        v.Fatalf("SSAable array must have no more than 1 element");
    }
    var elem = v.Block.NewValue0(v.Pos, OpPhi, t.Elem());
    foreach (var (_, a) in v.Args) {
        elem.AddArg(a.Block.NewValue1I(v.Pos, OpArraySelect, t.Elem(), 0, a));
    }    v.reset(OpArrayMake1);
    v.AddArg(elem); 

    // Recursively decompose elem phi.
    decomposeUserPhi(_addr_elem);
}

// MaxStruct is the maximum number of fields a struct
// can have and still be SSAable.
public static readonly nint MaxStruct = 4;

// StructMakeOp returns the opcode to construct a struct with the
// given number of fields.


// StructMakeOp returns the opcode to construct a struct with the
// given number of fields.
public static Op StructMakeOp(nint nf) => func((_, panic, _) => {
    switch (nf) {
        case 0: 
            return OpStructMake0;
            break;
        case 1: 
            return OpStructMake1;
            break;
        case 2: 
            return OpStructMake2;
            break;
        case 3: 
            return OpStructMake3;
            break;
        case 4: 
            return OpStructMake4;
            break;
    }
    panic("too many fields in an SSAable struct");
});

private partial struct namedVal {
    public nint locIndex; // f.NamedValues[f.Names[locIndex]][valIndex] = key
    public nint valIndex; // f.NamedValues[f.Names[locIndex]][valIndex] = key
}

// deleteNamedVals removes particular values with debugger names from f's naming data structures,
// removes all values with OpInvalid, and re-sorts the list of Names.
private static void deleteNamedVals(ptr<Func> _addr_f, slice<namedVal> toDelete) {
    ref Func f = ref _addr_f.val;
 
    // Arrange to delete from larger indices to smaller, to ensure swap-with-end deletion does not invalidate pending indices.
    sort.Slice(toDelete, (i, j) => {
        if (toDelete[i].locIndex != toDelete[j].locIndex) {
            return toDelete[i].locIndex > toDelete[j].locIndex;
        }
        return toDelete[i].valIndex > toDelete[j].valIndex;
    }); 

    // Get rid of obsolete names
    foreach (var (_, d) in toDelete) {
        var loc = f.Names[d.locIndex];
        var vals = f.NamedValues[loc.val];
        var l = len(vals) - 1;
        if (l > 0) {
            vals[d.valIndex] = vals[l];
        }
        vals[l] = null;
        f.NamedValues[loc.val] = vals[..(int)l];
    }    var end = len(f.Names);
    for (var i = len(f.Names) - 1; i >= 0; i--) {
        loc = f.Names[i];
        vals = f.NamedValues[loc.val];
        var last = len(vals);
        for (var j = len(vals) - 1; j >= 0; j--) {
            if (vals[j].Op == OpInvalid) {
                last--;
                vals[j] = vals[last];
                vals[last] = null;
            }
        }
        if (last < len(vals)) {
            f.NamedValues[loc.val] = vals[..(int)last];
        }
        if (len(vals) == 0) {
            delete(f.NamedValues, loc.val);
            end--;
            f.Names[i] = f.Names[end];
            f.Names[end] = null;
        }
    }
    f.Names = f.Names[..(int)end];
}

} // end ssa_package
