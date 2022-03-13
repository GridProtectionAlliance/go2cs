// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:59:10 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\size.go
namespace go.cmd.compile.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using sort = sort_package;

using @base = cmd.compile.@internal.@base_package;
using src = cmd.@internal.src_package;
using System;

public static partial class types_package {

public static nint PtrSize = default;

public static nint RegSize = default;

// Slices in the runtime are represented by three components:
//
// type slice struct {
//     ptr unsafe.Pointer
//     len int
//     cap int
// }
//
// Strings in the runtime are represented by two components:
//
// type string struct {
//     ptr unsafe.Pointer
//     len int
// }
//
// These variables are the offsets of fields and sizes of these structs.
public static long SlicePtrOffset = default;public static long SliceLenOffset = default;public static long SliceCapOffset = default;public static long SliceSize = default;public static long StringSize = default;

public static bool SkipSizeForTracing = default;

// typePos returns the position associated with t.
// This is where t was declared or where it appeared as a type expression.
private static src.XPos typePos(ptr<Type> _addr_t) => func((_, panic, _) => {
    ref Type t = ref _addr_t.val;

    {
        var pos = t.Pos();

        if (pos.IsKnown()) {
            return pos;
        }
    }
    @base.Fatalf("bad type: %v", t);
    panic("unreachable");
});

// MaxWidth is the maximum size of a value on the target architecture.
public static long MaxWidth = default;

// CalcSizeDisabled indicates whether it is safe
// to calculate Types' widths and alignments. See CalcSize.
public static bool CalcSizeDisabled = default;

// machine size and rounding alignment is dictated around
// the size of a pointer, set in betypeinit (see ../amd64/galign.go).
private static nint defercalc = default;

public static long Rnd(long o, long r) {
    if (r < 1 || r > 8 || r & (r - 1) != 0) {
        @base.Fatalf("rnd %d", r);
    }
    return (o + r - 1) & ~(r - 1);
}

// expandiface computes the method set for interface type t by
// expanding embedded interfaces.
private static void expandiface(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    var seen = make_map<ptr<Sym>, ptr<Field>>();
    slice<ptr<Field>> methods = default;

    Action<ptr<Field>, bool> addMethod = (m, @explicit) => {
        {
            var prev = seen[m.Sym];


            if (prev == null) 
                seen[m.Sym] = m;
            else if (AllowsGoVersion(t.Pkg(), 1, 14) && !explicit && Identical(m.Type, prev.Type)) 
                return ;
            else 
                @base.ErrorfAt(m.Pos, "duplicate method %s", m.Sym.Name);

        }
        methods = append(methods, m);
    };

    {
        var m__prev1 = m;

        foreach (var (_, __m) in t.Methods().Slice()) {
            m = __m;
            if (m.Sym == null) {
                continue;
            }
            CheckSize(_addr_m.Type);
            addMethod(m, true);
        }
        m = m__prev1;
    }

    {
        var m__prev1 = m;

        foreach (var (_, __m) in t.Methods().Slice()) {
            m = __m;
            if (m.Sym != null || m.Type == null) {
                continue;
            }
            if (!m.Type.IsInterface()) {
                @base.ErrorfAt(m.Pos, "interface contains embedded non-interface %v", m.Type);
                m.SetBroke(true);
                t.SetBroke(true); 
                // Add to fields so that error messages
                // include the broken embedded type when
                // printing t.
                // TODO(mdempsky): Revisit this.
                methods = append(methods, m);
                continue;
            } 

            // Embedded interface: duplicate all methods
            // (including broken ones, if any) and add to t's
            // method set.
            foreach (var (_, t1) in m.Type.AllMethods().Slice()) { 
                // Use m.Pos rather than t1.Pos to preserve embedding position.
                var f = NewField(m.Pos, t1.Sym, t1.Type);
                addMethod(f, false);
            }
        }
        m = m__prev1;
    }

    sort.Sort(MethodsByName(methods));

    if (int64(len(methods)) >= MaxWidth / int64(PtrSize)) {
        @base.ErrorfAt(typePos(_addr_t), "interface too large");
    }
    {
        var m__prev1 = m;

        foreach (var (__i, __m) in methods) {
            i = __i;
            m = __m;
            m.Offset = int64(i) * int64(PtrSize);
        }
        m = m__prev1;
    }

    t.SetAllMethods(methods);
}

private static long calcStructOffset(ptr<Type> _addr_errtype, ptr<Type> _addr_t, long o, nint flag) {
    ref Type errtype = ref _addr_errtype.val;
    ref Type t = ref _addr_t.val;
 
    // flag is 0 (receiver), 1 (actual struct), or RegSize (in/out parameters)
    var isStruct = flag == 1;
    var starto = o;
    var maxalign = int32(flag);
    if (maxalign < 1) {
        maxalign = 1;
    }
    var lastzero = int64(0);
    foreach (var (_, f) in t.Fields().Slice()) {
        if (f.Type == null) { 
            // broken field, just skip it so that other valid fields
            // get a width.
            continue;
        }
        CalcSize(_addr_f.Type);
        if (int32(f.Type.Align) > maxalign) {
            maxalign = int32(f.Type.Align);
        }
        if (f.Type.Align > 0) {
            o = Rnd(o, int64(f.Type.Align));
        }
        if (isStruct) { // For receiver/args/results, do not set, it depends on ABI
            f.Offset = o;
        }
        var w = f.Type.Width;
        if (w < 0) {
            @base.Fatalf("invalid width %d", f.Type.Width);
        }
        if (w == 0) {
            lastzero = o;
        }
        o += w;
        var maxwidth = MaxWidth; 
        // On 32-bit systems, reflect tables impose an additional constraint
        // that each field start offset must fit in 31 bits.
        if (maxwidth < 1 << 32) {
            maxwidth = 1 << 31 - 1;
        }
        if (o >= maxwidth) {
            @base.ErrorfAt(typePos(_addr_errtype), "type %L too large", errtype);
            o = 8; // small but nonzero
        }
    }    if (flag == 1 && o > starto && o == lastzero) {
        o++;
    }
    if (flag != 0) {
        o = Rnd(o, int64(maxalign));
    }
    t.Align = uint8(maxalign); 

    // type width only includes back to first field's offset
    t.Width = o - starto;

    return o;
}

// findTypeLoop searches for an invalid type declaration loop involving
// type t and reports whether one is found. If so, path contains the
// loop.
//
// path points to a slice used for tracking the sequence of types
// visited. Using a pointer to a slice allows the slice capacity to
// grow and limit reallocations.
private static bool findTypeLoop(ptr<Type> _addr_t, ptr<slice<ptr<Type>>> _addr_path) {
    ref Type t = ref _addr_t.val;
    ref slice<ptr<Type>> path = ref _addr_path.val;
 
    // We implement a simple DFS loop-finding algorithm. This
    // could be faster, but type cycles are rare.

    if (t.Sym() != null) { 
        // Declared type. Check for loops and otherwise
        // recurse on the type expression used in the type
        // declaration.

        // Type imported from package, so it can't be part of
        // a type loop (otherwise that package should have
        // failed to compile).
        if (t.Sym().Pkg != LocalPkg) {
            return false;
        }
        foreach (var (i, x) in path.val) {
            if (x == t) {
                path.val = (path.val)[(int)i..];
                return true;
            }
        }
    else
        path.val = append(path.val, t);
        if (findTypeLoop(_addr_t.Obj()._<TypeObject>().TypeDefn(), _addr_path)) {
            return true;
        }
        path.val = (path.val)[..(int)len(path.val) - 1];
    } { 
        // Anonymous type. Recurse on contained types.


        if (t.Kind() == TARRAY) 
            if (findTypeLoop(_addr_t.Elem(), _addr_path)) {
                return true;
            }
        else if (t.Kind() == TSTRUCT) 
            foreach (var (_, f) in t.Fields().Slice()) {
                if (findTypeLoop(_addr_f.Type, _addr_path)) {
                    return true;
                }
            }
        else if (t.Kind() == TINTER) 
            foreach (var (_, m) in t.Methods().Slice()) {
                if (m.Type.IsInterface()) { // embedded interface
                    if (findTypeLoop(_addr_m.Type, _addr_path)) {
                        return true;
                    }
                }
            }
            }
    return false;
}

private static void reportTypeLoop(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t.Broke()) {
        return ;
    }
    ref slice<ptr<Type>> l = ref heap(out ptr<slice<ptr<Type>>> _addr_l);
    if (!findTypeLoop(_addr_t, _addr_l)) {
        @base.Fatalf("failed to find type loop for: %v", t);
    }
    nint i = 0;
    {
        var t__prev1 = t;

        foreach (var (__j, __t) in l[(int)1..]) {
            j = __j;
            t = __t;
            if (typePos(_addr_t).Before(typePos(_addr_l[i]))) {
                i = j + 1;
            }
        }
        t = t__prev1;
    }

    l = append(l[(int)i..], l[..(int)i]);

    ref bytes.Buffer msg = ref heap(out ptr<bytes.Buffer> _addr_msg);
    fmt.Fprintf(_addr_msg, "invalid recursive type %v\n", l[0]);
    {
        var t__prev1 = t;

        foreach (var (_, __t) in l) {
            t = __t;
            fmt.Fprintf(_addr_msg, "\t%v: %v refers to\n", @base.FmtPos(typePos(_addr_t)), t);
            t.SetBroke(true);
        }
        t = t__prev1;
    }

    fmt.Fprintf(_addr_msg, "\t%v: %v", @base.FmtPos(typePos(_addr_l[0])), l[0]);
    @base.ErrorfAt(typePos(_addr_l[0]), msg.String());
}

// CalcSize calculates and stores the size and alignment for t.
// If CalcSizeDisabled is set, and the size/alignment
// have not already been calculated, it calls Fatal.
// This is used to prevent data races in the back end.
public static void CalcSize(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;
 
    // Calling CalcSize when typecheck tracing enabled is not safe.
    // See issue #33658.
    if (@base.EnableTrace && SkipSizeForTracing) {
        return ;
    }
    if (PtrSize == 0) { 
        // Assume this is a test.
        return ;
    }
    if (t == null) {
        return ;
    }
    if (t.Width == -2) {
        reportTypeLoop(_addr_t);
        t.Width = 0;
        t.Align = 1;
        return ;
    }
    if (t.WidthCalculated()) {
        return ;
    }
    if (CalcSizeDisabled) {
        if (t.Broke()) { 
            // break infinite recursion from Fatal call below
            return ;
        }
        t.SetBroke(true);
        @base.Fatalf("width not calculated: %v", t);
    }
    if (t.Broke() && t.Width == 0) {
        return ;
    }
    DeferCheckSize();

    var lno = @base.Pos;
    {
        var pos = t.Pos();

        if (pos.IsKnown()) {
            @base.Pos = pos;
        }
    }

    t.Width = -2;
    t.Align = 0; // 0 means use t.Width, below

    var et = t.Kind();

    if (et == TFUNC || et == TCHAN || et == TMAP || et == TSTRING) 
        break; 

        // SimType == 0 during bootstrap
    else 
        if (SimType[t.Kind()] != 0) {
            et = SimType[t.Kind()];
        }
        long w = default;

    if (et == TINT8 || et == TUINT8 || et == TBOOL) 
        // bool is int8
        w = 1;
    else if (et == TINT16 || et == TUINT16) 
        w = 2;
    else if (et == TINT32 || et == TUINT32 || et == TFLOAT32) 
        w = 4;
    else if (et == TINT64 || et == TUINT64 || et == TFLOAT64) 
        w = 8;
        t.Align = uint8(RegSize);
    else if (et == TCOMPLEX64) 
        w = 8;
        t.Align = 4;
    else if (et == TCOMPLEX128) 
        w = 16;
        t.Align = uint8(RegSize);
    else if (et == TPTR) 
        w = int64(PtrSize);
        CheckSize(_addr_t.Elem());
    else if (et == TUNSAFEPTR) 
        w = int64(PtrSize);
    else if (et == TINTER) // implemented as 2 pointers
        w = 2 * int64(PtrSize);
        t.Align = uint8(PtrSize);
        expandiface(_addr_t);
    else if (et == TCHAN) // implemented as pointer
        w = int64(PtrSize);

        CheckSize(_addr_t.Elem()); 

        // make fake type to check later to
        // trigger channel argument check.
        var t1 = NewChanArgs(t);
        CheckSize(_addr_t1);
    else if (et == TCHANARGS) 
        t1 = t.ChanArgs();
        CalcSize(_addr_t1); // just in case
        if (t1.Elem().Width >= 1 << 16) {
            @base.ErrorfAt(typePos(_addr_t1), "channel element type too large (>64kB)");
        }
        w = 1; // anything will do
    else if (et == TMAP) // implemented as pointer
        w = int64(PtrSize);
        CheckSize(_addr_t.Elem());
        CheckSize(_addr_t.Key());
    else if (et == TFORW) // should have been filled in
        reportTypeLoop(_addr_t);
        w = 1; // anything will do
    else if (et == TANY) 
        // not a real type; should be replaced before use.
        @base.Fatalf("CalcSize any");
    else if (et == TSTRING) 
        if (StringSize == 0) {
            @base.Fatalf("early CalcSize string");
        }
        w = StringSize;
        t.Align = uint8(PtrSize);
    else if (et == TARRAY) 
        if (t.Elem() == null) {
            break;
        }
        CalcSize(_addr_t.Elem());
        if (t.Elem().Width != 0) {
            var cap = (uint64(MaxWidth) - 1) / uint64(t.Elem().Width);
            if (uint64(t.NumElem()) > cap) {
                @base.ErrorfAt(typePos(_addr_t), "type %L larger than address space", t);
            }
        }
        w = t.NumElem() * t.Elem().Width;
        t.Align = t.Elem().Align;
    else if (et == TSLICE) 
        if (t.Elem() == null) {
            break;
        }
        w = SliceSize;
        CheckSize(_addr_t.Elem());
        t.Align = uint8(PtrSize);
    else if (et == TSTRUCT) 
        if (t.IsFuncArgStruct()) {
            @base.Fatalf("CalcSize fn struct %v", t);
        }
        w = calcStructOffset(_addr_t, _addr_t, 0, 1); 

        // make fake type to check later to
        // trigger function argument computation.
    else if (et == TFUNC) 
        t1 = NewFuncArgs(t);
        CheckSize(_addr_t1);
        w = int64(PtrSize); // width of func type is pointer

        // function is 3 cated structures;
        // compute their widths as side-effect.
    else if (et == TFUNCARGS) 
        t1 = t.FuncArgs();
        w = calcStructOffset(_addr_t1, _addr_t1.Recvs(), 0, 0);
        w = calcStructOffset(_addr_t1, _addr_t1.Params(), w, RegSize);
        w = calcStructOffset(_addr_t1, _addr_t1.Results(), w, RegSize);
        t1.Extra._<ptr<Func>>().Argwid = w;
        if (w % int64(RegSize) != 0) {
            @base.Warn("bad type %v %d\n", t1, w);
        }
        t.Align = 1;
    else if (et == TTYPEPARAM) 
        // TODO(danscales) - remove when we eliminate the need
        // to do CalcSize in noder2 (which shouldn't be needed in the noder)
        w = int64(PtrSize);
    else 
        @base.Fatalf("CalcSize: unknown type: %v", t); 

        // compiler-specific stuff
        if (PtrSize == 4 && w != int64(int32(w))) {
        @base.ErrorfAt(typePos(_addr_t), "type %v too large", t);
    }
    t.Width = w;
    if (t.Align == 0) {
        if (w == 0 || w > 8 || w & (w - 1) != 0) {
            @base.Fatalf("invalid alignment for %v", t);
        }
        t.Align = uint8(w);
    }
    @base.Pos = lno;

    ResumeCheckSize();
}

// CalcStructSize calculates the size of s,
// filling in s.Width and s.Align,
// even if size calculation is otherwise disabled.
public static void CalcStructSize(ptr<Type> _addr_s) {
    ref Type s = ref _addr_s.val;

    s.Width = calcStructOffset(_addr_s, _addr_s, 0, 1); // sets align
}

// when a type's width should be known, we call CheckSize
// to compute it.  during a declaration like
//
//    type T *struct { next T }
//
// it is necessary to defer the calculation of the struct width
// until after T has been initialized to be a pointer to that struct.
// similarly, during import processing structs may be used
// before their definition.  in those situations, calling
// DeferCheckSize() stops width calculations until
// ResumeCheckSize() is called, at which point all the
// CalcSizes that were deferred are executed.
// CalcSize should only be called when the type's size
// is needed immediately.  CheckSize makes sure the
// size is evaluated eventually.

private static slice<ptr<Type>> deferredTypeStack = default;

public static void CheckSize(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t == null) {
        return ;
    }
    if (t.IsFuncArgStruct()) {
        @base.Fatalf("CheckSize %v", t);
    }
    if (defercalc == 0) {
        CalcSize(_addr_t);
        return ;
    }
    if (!t.Deferwidth()) {
        t.SetDeferwidth(true);
        deferredTypeStack = append(deferredTypeStack, t);
    }
}

public static void DeferCheckSize() {
    defercalc++;
}

public static void ResumeCheckSize() {
    if (defercalc == 1) {
        while (len(deferredTypeStack) > 0) {
            var t = deferredTypeStack[len(deferredTypeStack) - 1];
            deferredTypeStack = deferredTypeStack[..(int)len(deferredTypeStack) - 1];
            t.SetDeferwidth(false);
            CalcSize(_addr_t);
        }
    }
    defercalc--;
}

// PtrDataSize returns the length in bytes of the prefix of t
// containing pointer data. Anything after this offset is scalar data.
public static long PtrDataSize(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (!t.HasPointers()) {
        return 0;
    }

    if (t.Kind() == TPTR || t.Kind() == TUNSAFEPTR || t.Kind() == TFUNC || t.Kind() == TCHAN || t.Kind() == TMAP) 
        return int64(PtrSize);
    else if (t.Kind() == TSTRING) 
        // struct { byte *str; intgo len; }
        return int64(PtrSize);
    else if (t.Kind() == TINTER) 
        // struct { Itab *tab;    void *data; } or
        // struct { Type *type; void *data; }
        // Note: see comment in typebits.Set
        return 2 * int64(PtrSize);
    else if (t.Kind() == TSLICE) 
        // struct { byte *array; uintgo len; uintgo cap; }
        return int64(PtrSize);
    else if (t.Kind() == TARRAY) 
        // haspointers already eliminated t.NumElem() == 0.
        return (t.NumElem() - 1) * t.Elem().Width + PtrDataSize(_addr_t.Elem());
    else if (t.Kind() == TSTRUCT) 
        // Find the last field that has pointers.
        ptr<Field> lastPtrField;
        var fs = t.Fields().Slice();
        for (var i = len(fs) - 1; i >= 0; i--) {
            if (fs[i].Type.HasPointers()) {
                lastPtrField = fs[i];
                break;
            }
        }
        return lastPtrField.Offset + PtrDataSize(_addr_lastPtrField.Type);
    else 
        @base.Fatalf("PtrDataSize: unexpected type, %v", t);
        return 0;
    }

} // end types_package
