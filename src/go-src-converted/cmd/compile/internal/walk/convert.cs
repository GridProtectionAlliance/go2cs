// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 06 23:11:45 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\convert.go
using binary = go.encoding.binary_package;
using constant = go.go.constant_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using reflectdata = go.cmd.compile.@internal.reflectdata_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using sys = go.cmd.@internal.sys_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class walk_package {

    // walkConv walks an OCONV or OCONVNOP (but not OCONVIFACE) node.
private static ir.Node walkConv(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    n.X = walkExpr(n.X, init);
    if (n.Op() == ir.OCONVNOP && n.Type() == n.X.Type()) {
        return n.X;
    }
    if (n.Op() == ir.OCONVNOP && ir.ShouldCheckPtr(ir.CurFunc, 1)) {
        if (n.Type().IsPtr() && n.X.Type().IsUnsafePtr()) { // unsafe.Pointer to *T
            return walkCheckPtrAlignment(_addr_n, _addr_init, null);

        }
        if (n.Type().IsUnsafePtr() && n.X.Type().IsUintptr()) { // uintptr to unsafe.Pointer
            return walkCheckPtrArithmetic(_addr_n, _addr_init);

        }
    }
    var (param, result) = rtconvfn(_addr_n.X.Type(), _addr_n.Type());
    if (param == types.Txxx) {
        return n;
    }
    var fn = types.BasicTypeNames[param] + "to" + types.BasicTypeNames[result];
    return typecheck.Conv(mkcall(fn, types.Types[result], init, typecheck.Conv(n.X, types.Types[param])), n.Type());

}

// walkConvInterface walks an OCONVIFACE node.
private static ir.Node walkConvInterface(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    n.X = walkExpr(n.X, init);

    var fromType = n.X.Type();
    var toType = n.Type();

    if (!fromType.IsInterface() && !ir.IsBlank(ir.CurFunc.Nname)) { // skip unnamed functions (func _())
        reflectdata.MarkTypeUsedInInterface(fromType, ir.CurFunc.LSym);

    }
    Func<ir.Node> typeword = () => {
        if (toType.IsEmptyInterface()) {
            return reflectdata.TypePtr(fromType);
        }
        return reflectdata.ITabAddr(fromType, toType);

    }; 

    // Optimize convT2E or convT2I as a two-word copy when T is pointer-shaped.
    if (types.IsDirectIface(fromType)) {
        var l = ir.NewBinaryExpr(@base.Pos, ir.OEFACE, typeword(), n.X);
        l.SetType(toType);
        l.SetTypecheck(n.Typecheck());
        return l;
    }
    ir.Node value = default;

    if (fromType.Size() == 0) 
        // n.Left is zero-sized. Use zerobase.
        cheapExpr(n.X, init); // Evaluate n.Left for side-effects. See issue 19246.
        value = ir.NewLinksymExpr(@base.Pos, ir.Syms.Zerobase, types.Types[types.TUINTPTR]);
    else if (fromType.IsBoolean() || (fromType.Size() == 1 && fromType.IsInteger())) 
        // n.Left is a bool/byte. Use staticuint64s[n.Left * 8] on little-endian
        // and staticuint64s[n.Left * 8 + 7] on big-endian.
        n.X = cheapExpr(n.X, init); 
        // byteindex widens n.Left so that the multiplication doesn't overflow.
        var index = ir.NewBinaryExpr(@base.Pos, ir.OLSH, byteindex(n.X), ir.NewInt(3));
        if (ssagen.Arch.LinkArch.ByteOrder == binary.BigEndian) {
            index = ir.NewBinaryExpr(@base.Pos, ir.OADD, index, ir.NewInt(7));
        }
        var staticuint64s = ir.NewLinksymExpr(@base.Pos, ir.Syms.Staticuint64s, types.NewArray(types.Types[types.TUINT8], 256 * 8));
        var xe = ir.NewIndexExpr(@base.Pos, staticuint64s, index);
        xe.SetBounded(true);
        value = xe;
    else if (n.X.Op() == ir.ONAME && n.X._<ptr<ir.Name>>().Class == ir.PEXTERN && n.X._<ptr<ir.Name>>().Readonly()) 
        // n.Left is a readonly global; use it directly.
        value = n.X;
    else if (!fromType.IsInterface() && n.Esc() == ir.EscNone && fromType.Width <= 1024) 
        // n.Left does not escape. Use a stack temporary initialized to n.Left.
        value = typecheck.Temp(fromType);
        init.Append(typecheck.Stmt(ir.NewAssignStmt(@base.Pos, value, n.X)));
        if (value != null) { 
        // Value is identical to n.Left.
        // Construct the interface directly: {type/itab, &value}.
        l = ir.NewBinaryExpr(@base.Pos, ir.OEFACE, typeword(), typecheck.Expr(typecheck.NodAddr(value)));
        l.SetType(toType);
        l.SetTypecheck(n.Typecheck());
        return l;

    }
    if (toType.IsEmptyInterface() && fromType.IsInterface() && !fromType.IsEmptyInterface()) { 
        // Evaluate the input interface.
        var c = typecheck.Temp(fromType);
        init.Append(ir.NewAssignStmt(@base.Pos, c, n.X)); 

        // Get the itab out of the interface.
        var tmp = typecheck.Temp(types.NewPtr(types.Types[types.TUINT8]));
        init.Append(ir.NewAssignStmt(@base.Pos, tmp, typecheck.Expr(ir.NewUnaryExpr(@base.Pos, ir.OITAB, c)))); 

        // Get the type out of the itab.
        var nif = ir.NewIfStmt(@base.Pos, typecheck.Expr(ir.NewBinaryExpr(@base.Pos, ir.ONE, tmp, typecheck.NodNil())), null, null);
        nif.Body = new slice<ir.Node>(new ir.Node[] { ir.NewAssignStmt(base.Pos,tmp,itabType(tmp)) });
        init.Append(nif); 

        // Build the result.
        var e = ir.NewBinaryExpr(@base.Pos, ir.OEFACE, tmp, ifaceData(n.Pos(), c, types.NewPtr(types.Types[types.TUINT8])));
        e.SetType(toType); // assign type manually, typecheck doesn't understand OEFACE.
        e.SetTypecheck(1);
        return e;

    }
    var (fnname, argType, needsaddr) = convFuncName(_addr_fromType, _addr_toType);

    if (!needsaddr && !fromType.IsInterface()) { 
        // Use a specialized conversion routine that only returns a data pointer.
        // ptr = convT2X(val)
        // e = iface{typ/tab, ptr}
        var fn = typecheck.LookupRuntime(fnname);
        types.CalcSize(fromType);

        var arg = n.X;

        if (fromType == argType)         else if (fromType.Kind() == argType.Kind() || fromType.IsPtrShaped() && argType.IsPtrShaped()) 
            // can directly convert (e.g. named type to underlying type, or one pointer to another)
            arg = ir.NewConvExpr(n.Pos(), ir.OCONVNOP, argType, arg);
        else if (fromType.IsInteger() && argType.IsInteger()) 
            // can directly convert (e.g. int32 to uint32)
            arg = ir.NewConvExpr(n.Pos(), ir.OCONV, argType, arg);
        else 
            // unsafe cast through memory
            arg = copyExpr(arg, arg.Type(), init);
            ir.Node addr = typecheck.NodAddr(arg);
            addr = ir.NewConvExpr(n.Pos(), ir.OCONVNOP, argType.PtrTo(), addr);
            arg = ir.NewStarExpr(n.Pos(), addr);
            arg.SetType(argType);
                var call = ir.NewCallExpr(@base.Pos, ir.OCALL, fn, null);
        call.Args = new slice<ir.Node>(new ir.Node[] { arg });
        e = ir.NewBinaryExpr(@base.Pos, ir.OEFACE, typeword(), safeExpr(walkExpr(typecheck.Expr(call), init), init));
        e.SetType(toType);
        e.SetTypecheck(1);
        return e;

    }
    ir.Node tab = default;
    if (fromType.IsInterface()) { 
        // convI2I
        tab = reflectdata.TypePtr(toType);

    }
    else
 { 
        // convT2x
        tab = typeword();

    }
    var v = n.X;
    if (needsaddr) { 
        // Types of large or unknown size are passed by reference.
        // Orderexpr arranged for n.Left to be a temporary for all
        // the conversions it could see. Comparison of an interface
        // with a non-interface, especially in a switch on interface value
        // with non-interface cases, is not visible to order.stmt, so we
        // have to fall back on allocating a temp here.
        if (!ir.IsAddressable(v)) {
            v = copyExpr(v, v.Type(), init);
        }
        v = typecheck.NodAddr(v);

    }
    types.CalcSize(fromType);
    fn = typecheck.LookupRuntime(fnname);
    fn = typecheck.SubstArgTypes(fn, fromType, toType);
    types.CalcSize(fn.Type());
    call = ir.NewCallExpr(@base.Pos, ir.OCALL, fn, null);
    call.Args = new slice<ir.Node>(new ir.Node[] { tab, v });
    return walkExpr(typecheck.Expr(call), init);

}

// walkBytesRunesToString walks an OBYTES2STR or ORUNES2STR node.
private static ir.Node walkBytesRunesToString(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var a = typecheck.NodNil();
    if (n.Esc() == ir.EscNone) { 
        // Create temporary buffer for string on stack.
        a = stackBufAddr(tmpstringbufsize, types.Types[types.TUINT8]);

    }
    if (n.Op() == ir.ORUNES2STR) { 
        // slicerunetostring(*[32]byte, []rune) string
        return mkcall("slicerunetostring", n.Type(), init, a, n.X);

    }
    n.X = cheapExpr(n.X, init);
    var (ptr, len) = backingArrayPtrLen(n.X);
    return mkcall("slicebytetostring", n.Type(), init, a, ptr, len);

}

// walkBytesToStringTemp walks an OBYTES2STRTMP node.
private static ir.Node walkBytesToStringTemp(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    n.X = walkExpr(n.X, init);
    if (!@base.Flag.Cfg.Instrumenting) { 
        // Let the backend handle OBYTES2STRTMP directly
        // to avoid a function call to slicebytetostringtmp.
        return n;

    }
    n.X = cheapExpr(n.X, init);
    var (ptr, len) = backingArrayPtrLen(n.X);
    return mkcall("slicebytetostringtmp", n.Type(), init, ptr, len);

}

// walkRuneToString walks an ORUNESTR node.
private static ir.Node walkRuneToString(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var a = typecheck.NodNil();
    if (n.Esc() == ir.EscNone) {
        a = stackBufAddr(4, types.Types[types.TUINT8]);
    }
    return mkcall("intstring", n.Type(), init, a, typecheck.Conv(n.X, types.Types[types.TINT64]));

}

// walkStringToBytes walks an OSTR2BYTES node.
private static ir.Node walkStringToBytes(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var s = n.X;
    if (ir.IsConst(s, constant.String)) {
        var sc = ir.StringVal(s); 

        // Allocate a [n]byte of the right size.
        var t = types.NewArray(types.Types[types.TUINT8], int64(len(sc)));
        ir.Node a = default;
        if (n.Esc() == ir.EscNone && len(sc) <= int(ir.MaxImplicitStackVarSize)) {
            a = stackBufAddr(t.NumElem(), t.Elem());
        }
        else
 {
            types.CalcSize(t);
            a = ir.NewUnaryExpr(@base.Pos, ir.ONEW, null);
            a.SetType(types.NewPtr(t));
            a.SetTypecheck(1);
            a.MarkNonNil();
        }
        var p = typecheck.Temp(t.PtrTo()); // *[n]byte
        init.Append(typecheck.Stmt(ir.NewAssignStmt(@base.Pos, p, a))); 

        // Copy from the static string data to the [n]byte.
        if (len(sc) > 0) {
            var @as = ir.NewAssignStmt(@base.Pos, ir.NewStarExpr(@base.Pos, p), ir.NewStarExpr(@base.Pos, typecheck.ConvNop(ir.NewUnaryExpr(@base.Pos, ir.OSPTR, s), t.PtrTo())));
            appendWalkStmt(init, as);
        }
        var slice = ir.NewSliceExpr(n.Pos(), ir.OSLICEARR, p, null, null, null);
        slice.SetType(n.Type());
        slice.SetTypecheck(1);
        return walkExpr(slice, init);

    }
    a = typecheck.NodNil();
    if (n.Esc() == ir.EscNone) { 
        // Create temporary buffer for slice on stack.
        a = stackBufAddr(tmpstringbufsize, types.Types[types.TUINT8]);

    }
    return mkcall("stringtoslicebyte", n.Type(), init, a, typecheck.Conv(s, types.Types[types.TSTRING]));

}

// walkStringToBytesTemp walks an OSTR2BYTESTMP node.
private static ir.Node walkStringToBytesTemp(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // []byte(string) conversion that creates a slice
    // referring to the actual string bytes.
    // This conversion is handled later by the backend and
    // is only for use by internal compiler optimizations
    // that know that the slice won't be mutated.
    // The only such case today is:
    // for i, c := range []byte(string)
    n.X = walkExpr(n.X, init);
    return n;

}

// walkStringToRunes walks an OSTR2RUNES node.
private static ir.Node walkStringToRunes(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init) {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    var a = typecheck.NodNil();
    if (n.Esc() == ir.EscNone) { 
        // Create temporary buffer for slice on stack.
        a = stackBufAddr(tmpstringbufsize, types.Types[types.TINT32]);

    }
    return mkcall("stringtoslicerune", n.Type(), init, a, typecheck.Conv(n.X, types.Types[types.TSTRING]));

}

// convFuncName builds the runtime function name for interface conversion.
// It also returns the argument type that the runtime function takes, and
// whether the function expects the data by address.
// Not all names are possible. For example, we never generate convE2E or convE2I.
private static (@string, ptr<types.Type>, bool) convFuncName(ptr<types.Type> _addr_from, ptr<types.Type> _addr_to) => func((_, panic, _) => {
    @string fnname = default;
    ptr<types.Type> argType = default!;
    bool needsaddr = default;
    ref types.Type from = ref _addr_from.val;
    ref types.Type to = ref _addr_to.val;

    var tkind = to.Tie();
    switch (from.Tie()) {
        case 'I': 
            if (tkind == 'I') {
                return ("convI2I", _addr_types.Types[types.TINTER]!, false);
            }
            break;
        case 'T': 

            if (from.Size() == 2 && from.Align == 2) 
                return ("convT16", _addr_types.Types[types.TUINT16]!, false);
            else if (from.Size() == 4 && from.Align == 4 && !from.HasPointers()) 
                return ("convT32", _addr_types.Types[types.TUINT32]!, false);
            else if (from.Size() == 8 && from.Align == types.Types[types.TUINT64].Align && !from.HasPointers()) 
                return ("convT64", _addr_types.Types[types.TUINT64]!, false);
                    {
                var sc = from.SoleComponent();

                if (sc != null) {

                    if (sc.IsString()) 
                        return ("convTstring", _addr_types.Types[types.TSTRING]!, false);
                    else if (sc.IsSlice()) 
                        return ("convTslice", _addr_types.NewSlice(types.Types[types.TUINT8])!, false); // the element type doesn't matter
                                }

            }


            switch (tkind) {
                case 'E': 
                    if (!from.HasPointers()) {
                        return ("convT2Enoptr", _addr_types.Types[types.TUNSAFEPTR]!, true);
                    }
                    return ("convT2E", _addr_types.Types[types.TUNSAFEPTR]!, true);
                    break;
                case 'I': 
                    if (!from.HasPointers()) {
                        return ("convT2Inoptr", _addr_types.Types[types.TUNSAFEPTR]!, true);
                    }
                    return ("convT2I", _addr_types.Types[types.TUNSAFEPTR]!, true);
                    break;
            }

            break;
    }
    @base.Fatalf("unknown conv func %c2%c", from.Tie(), to.Tie());
    panic("unreachable");

});

// rtconvfn returns the parameter and result types that will be used by a
// runtime function to convert from type src to type dst. The runtime function
// name can be derived from the names of the returned types.
//
// If no such function is necessary, it returns (Txxx, Txxx).
private static (types.Kind, types.Kind) rtconvfn(ptr<types.Type> _addr_src, ptr<types.Type> _addr_dst) {
    types.Kind param = default;
    types.Kind result = default;
    ref types.Type src = ref _addr_src.val;
    ref types.Type dst = ref _addr_dst.val;

    if (ssagen.Arch.SoftFloat) {
        return (types.Txxx, types.Txxx);
    }

    if (ssagen.Arch.LinkArch.Family == sys.ARM || ssagen.Arch.LinkArch.Family == sys.MIPS) 
        if (src.IsFloat()) {

            if (dst.Kind() == types.TINT64 || dst.Kind() == types.TUINT64) 
                return (types.TFLOAT64, dst.Kind());
            
        }
        if (dst.IsFloat()) {

            if (src.Kind() == types.TINT64 || src.Kind() == types.TUINT64) 
                return (src.Kind(), types.TFLOAT64);
            
        }
    else if (ssagen.Arch.LinkArch.Family == sys.I386) 
        if (src.IsFloat()) {

            if (dst.Kind() == types.TINT64 || dst.Kind() == types.TUINT64) 
                return (types.TFLOAT64, dst.Kind());
            else if (dst.Kind() == types.TUINT32 || dst.Kind() == types.TUINT || dst.Kind() == types.TUINTPTR) 
                return (types.TFLOAT64, types.TUINT32);
            
        }
        if (dst.IsFloat()) {

            if (src.Kind() == types.TINT64 || src.Kind() == types.TUINT64) 
                return (src.Kind(), types.TFLOAT64);
            else if (src.Kind() == types.TUINT32 || src.Kind() == types.TUINT || src.Kind() == types.TUINTPTR) 
                return (types.TUINT32, types.TFLOAT64);
            
        }
        return (types.Txxx, types.Txxx);

}

// byteindex converts n, which is byte-sized, to an int used to index into an array.
// We cannot use conv, because we allow converting bool to int here,
// which is forbidden in user code.
private static ir.Node byteindex(ir.Node n) { 
    // We cannot convert from bool to int directly.
    // While converting from int8 to int is possible, it would yield
    // the wrong result for negative values.
    // Reinterpreting the value as an unsigned byte solves both cases.
    if (!types.Identical(n.Type(), types.Types[types.TUINT8])) {
        n = ir.NewConvExpr(@base.Pos, ir.OCONV, null, n);
        n.SetType(types.Types[types.TUINT8]);
        n.SetTypecheck(1);
    }
    n = ir.NewConvExpr(@base.Pos, ir.OCONV, null, n);
    n.SetType(types.Types[types.TINT]);
    n.SetTypecheck(1);
    return n;

}

private static ir.Node walkCheckPtrAlignment(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init, ir.Node count) {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;

    if (!n.Type().IsPtr()) {
        @base.Fatalf("expected pointer type: %v", n.Type());
    }
    var elem = n.Type().Elem();
    if (count != null) {
        if (!elem.IsArray()) {
            @base.Fatalf("expected array type: %v", elem);
        }
        elem = elem.Elem();

    }
    var size = elem.Size();
    if (elem.Alignment() == 1 && (size == 0 || size == 1 && count == null)) {
        return n;
    }
    if (count == null) {
        count = ir.NewInt(1);
    }
    n.X = cheapExpr(n.X, init);
    init.Append(mkcall("checkptrAlignment", null, init, typecheck.ConvNop(n.X, types.Types[types.TUNSAFEPTR]), reflectdata.TypePtr(elem), typecheck.Conv(count, types.Types[types.TUINTPTR])));
    return n;

}

private static ir.Node walkCheckPtrArithmetic(ptr<ir.ConvExpr> _addr_n, ptr<ir.Nodes> _addr_init) => func((defer, _, _) => {
    ref ir.ConvExpr n = ref _addr_n.val;
    ref ir.Nodes init = ref _addr_init.val;
 
    // Calling cheapExpr(n, init) below leads to a recursive call to
    // walkExpr, which leads us back here again. Use n.Checkptr to
    // prevent infinite loops.
    if (n.CheckPtr()) {
        return n;
    }
    n.SetCheckPtr(true);
    defer(n.SetCheckPtr(false)); 

    // TODO(mdempsky): Make stricter. We only need to exempt
    // reflect.Value.Pointer and reflect.Value.UnsafeAddr.

    if (n.X.Op() == ir.OCALLFUNC || n.X.Op() == ir.OCALLMETH || n.X.Op() == ir.OCALLINTER) 
        return n;
        if (n.X.Op() == ir.ODOTPTR && ir.IsReflectHeaderDataField(n.X)) {
        return n;
    }
    slice<ir.Node> originals = default;
    Action<ir.Node> walk = default;
    walk = n => {

        if (n.Op() == ir.OADD) 
            ptr<ir.BinaryExpr> n = n._<ptr<ir.BinaryExpr>>();
            walk(n.X);
            walk(n.Y);
        else if (n.Op() == ir.OSUB || n.Op() == ir.OANDNOT) 
            n = n._<ptr<ir.BinaryExpr>>();
            walk(n.X);
        else if (n.Op() == ir.OCONVNOP) 
            n = n._<ptr<ir.ConvExpr>>();
            if (n.X.Type().IsUnsafePtr()) {
                n.X = cheapExpr(n.X, init);
                originals = append(originals, typecheck.ConvNop(n.X, types.Types[types.TUNSAFEPTR]));
            }
        
    };
    walk(n.X);

    var cheap = cheapExpr(n, init);

    var slice = typecheck.MakeDotArgs(types.NewSlice(types.Types[types.TUNSAFEPTR]), originals);
    slice.SetEsc(ir.EscNone);

    init.Append(mkcall("checkptrArithmetic", null, init, typecheck.ConvNop(cheap, types.Types[types.TUNSAFEPTR]), slice)); 
    // TODO(khr): Mark backing store of slice as dead. This will allow us to reuse
    // the backing store for multiple calls to checkptrArithmetic.

    return cheap;

});

} // end walk_package
