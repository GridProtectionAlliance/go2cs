// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 13 05:59:24 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\dcl.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using strconv = strconv_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;

public static partial class typecheck_package {

public static ir.Class DeclContext = ir.PEXTERN; // PEXTERN/PAUTO

public static ptr<ir.Func> DeclFunc(ptr<types.Sym> _addr_sym, ir.Ntype tfn) {
    ref types.Sym sym = ref _addr_sym.val;

    if (tfn.Op() != ir.OTFUNC) {
        @base.Fatalf("expected OTFUNC node, got %v", tfn);
    }
    var fn = ir.NewFunc(@base.Pos);
    fn.Nname = ir.NewNameAt(@base.Pos, sym);
    fn.Nname.Func = fn;
    fn.Nname.Defn = fn;
    fn.Nname.Ntype = tfn;
    ir.MarkFunc(fn.Nname);
    StartFuncBody(_addr_fn);
    fn.Nname.Ntype = typecheckNtype(fn.Nname.Ntype);
    return _addr_fn!;
}

// Declare records that Node n declares symbol n.Sym in the specified
// declaration context.
public static void Declare(ptr<ir.Name> _addr_n, ir.Class ctxt) {
    ref ir.Name n = ref _addr_n.val;

    if (ir.IsBlank(n)) {
        return ;
    }
    var s = n.Sym(); 

    // kludgy: TypecheckAllowed means we're past parsing. Eg reflectdata.methodWrapper may declare out of package names later.
    if (!inimport && !TypecheckAllowed && s.Pkg != types.LocalPkg) {
        @base.ErrorfAt(n.Pos(), "cannot declare name %v", s);
    }
    if (ctxt == ir.PEXTERN) {
        if (s.Name == "init") {
            @base.ErrorfAt(n.Pos(), "cannot declare init - must be func");
        }
        if (s.Name == "main" && s.Pkg.Name == "main") {
            @base.ErrorfAt(n.Pos(), "cannot declare main - must be func");
        }
        Target.Externs = append(Target.Externs, n);
    }
    else
 {
        if (ir.CurFunc == null && ctxt == ir.PAUTO) {
            @base.Pos = n.Pos();
            @base.Fatalf("automatic outside function");
        }
        if (ir.CurFunc != null && ctxt != ir.PFUNC && n.Op() == ir.ONAME) {
            ir.CurFunc.Dcl = append(ir.CurFunc.Dcl, n);
        }
        types.Pushdcl(s);
        n.Curfn = ir.CurFunc;
    }
    if (ctxt == ir.PAUTO) {
        n.SetFrameOffset(0);
    }
    if (s.Block == types.Block) { 
        // functype will print errors about duplicate function arguments.
        // Don't repeat the error here.
        if (ctxt != ir.PPARAM && ctxt != ir.PPARAMOUT) {
            Redeclared(n.Pos(), _addr_s, "in this block");
        }
    }
    s.Block = types.Block;
    s.Lastlineno = @base.Pos;
    s.Def = n;
    n.Class = ctxt;
    if (ctxt == ir.PFUNC) {
        n.Sym().SetFunc(true);
    }
    autoexport(_addr_n, ctxt);
}

// Export marks n for export (or reexport).
public static void Export(ptr<ir.Name> _addr_n) {
    ref ir.Name n = ref _addr_n.val;

    if (n.Sym().OnExportList()) {
        return ;
    }
    n.Sym().SetOnExportList(true);

    if (@base.Flag.E != 0) {
        fmt.Printf("export symbol %v\n", n.Sym());
    }
    Target.Exports = append(Target.Exports, n);
}

// Redeclared emits a diagnostic about symbol s being redeclared at pos.
public static void Redeclared(src.XPos pos, ptr<types.Sym> _addr_s, @string where) {
    ref types.Sym s = ref _addr_s.val;

    if (!s.Lastlineno.IsKnown()) {
        ptr<ir.PkgName> pkgName;
        if (s.Def == null) {
            foreach (var (id, pkg) in DotImportRefs) {
                if (id.Sym().Name == s.Name) {
                    pkgName = pkg;
                    break;
                }
            }
        else
        } {
            pkgName = DotImportRefs[s.Def._<ptr<ir.Ident>>()];
        }
    else
        @base.ErrorfAt(pos, "%v redeclared %s\n" + "\t%v: previous declaration during import %q", s, where, @base.FmtPos(pkgName.Pos()), pkgName.Pkg.Path);
    } {
        var prevPos = s.Lastlineno; 

        // When an import and a declaration collide in separate files,
        // present the import as the "redeclared", because the declaration
        // is visible where the import is, but not vice versa.
        // See issue 4510.
        if (s.Def == null) {
            (pos, prevPos) = (prevPos, pos);
        }
        @base.ErrorfAt(pos, "%v redeclared %s\n" + "\t%v: previous declaration", s, where, @base.FmtPos(prevPos));
    }
}

// declare the function proper
// and declare the arguments.
// called in extern-declaration context
// returns in auto-declaration context.
public static void StartFuncBody(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;
 
    // change the declaration context from extern to auto
    funcStack = append(funcStack, new funcStackEnt(ir.CurFunc,DeclContext));
    ir.CurFunc = fn;
    DeclContext = ir.PAUTO;

    types.Markdcl();

    if (fn.Nname.Ntype != null) {
        funcargs(fn.Nname.Ntype._<ptr<ir.FuncType>>());
    }
    else
 {
        funcargs2(_addr_fn.Type());
    }
}

// finish the body.
// called in auto-declaration context.
// returns in extern-declaration context.
public static void FinishFuncBody() { 
    // change the declaration context from auto to previous context
    types.Popdcl();
    funcStackEnt e = default;
    (funcStack, e) = (funcStack[..(int)len(funcStack) - 1], funcStack[len(funcStack) - 1]);    (ir.CurFunc, DeclContext) = (e.curfn, e.dclcontext);
}

public static void CheckFuncStack() {
    if (len(funcStack) != 0) {
        @base.Fatalf("funcStack is non-empty: %v", len(funcStack));
    }
}

// Add a method, declared as a function.
// - msym is the method symbol
// - t is function type (with receiver)
// Returns a pointer to the existing or added Field; or nil if there's an error.
private static ptr<types.Field> addmethod(ptr<ir.Func> _addr_n, ptr<types.Sym> _addr_msym, ptr<types.Type> _addr_t, bool local, bool nointerface) {
    ref ir.Func n = ref _addr_n.val;
    ref types.Sym msym = ref _addr_msym.val;
    ref types.Type t = ref _addr_t.val;

    if (msym == null) {
        @base.Fatalf("no method symbol");
    }
    var rf = t.Recv(); // ptr to this structure
    if (rf == null) {
        @base.Errorf("missing receiver");
        return _addr_null!;
    }
    var mt = types.ReceiverBaseType(rf.Type);
    if (mt == null || mt.Sym() == null) {
        var pa = rf.Type;
        var t = pa;
        if (t != null && t.IsPtr()) {
            if (t.Sym() != null) {
                @base.Errorf("invalid receiver type %v (%v is a pointer type)", pa, t);
                return _addr_null!;
            }
            t = t.Elem();
        }

        if (t == null || t.Broke())         else if (t.Sym() == null) 
            @base.Errorf("invalid receiver type %v (%v is not a defined type)", pa, t);
        else if (t.IsPtr()) 
            @base.Errorf("invalid receiver type %v (%v is a pointer type)", pa, t);
        else if (t.IsInterface()) 
            @base.Errorf("invalid receiver type %v (%v is an interface type)", pa, t);
        else 
            // Should have picked off all the reasons above,
            // but just in case, fall back to generic error.
            @base.Errorf("invalid receiver type %v (%L / %L)", pa, pa, t);
                return _addr_null!;
    }
    if (local && mt.Sym().Pkg != types.LocalPkg) {
        @base.Errorf("cannot define new methods on non-local type %v", mt);
        return _addr_null!;
    }
    if (msym.IsBlank()) {
        return _addr_null!;
    }
    if (mt.IsStruct()) {
        {
            var f__prev1 = f;

            foreach (var (_, __f) in mt.Fields().Slice()) {
                f = __f;
                if (f.Sym == msym) {
                    @base.Errorf("type %v has both field and method named %v", mt, msym);
                    f.SetBroke(true);
                    return _addr_null!;
                }
            }

            f = f__prev1;
        }
    }
    {
        var f__prev1 = f;

        foreach (var (_, __f) in mt.Methods().Slice()) {
            f = __f;
            if (msym.Name != f.Sym.Name) {
                continue;
            } 
            // types.Identical only checks that incoming and result parameters match,
            // so explicitly check that the receiver parameters match too.
            if (!types.Identical(t, f.Type) || !types.Identical(t.Recv().Type, f.Type.Recv().Type)) {
                @base.Errorf("method redeclared: %v.%v\n\t%v\n\t%v", mt, msym, f.Type, t);
            }
            return _addr_f!;
        }
        f = f__prev1;
    }

    var f = types.NewField(@base.Pos, msym, t);
    f.Nname = n.Nname;
    f.SetNointerface(nointerface);

    mt.Methods().Append(f);
    return _addr_f!;
}

private static void autoexport(ptr<ir.Name> _addr_n, ir.Class ctxt) {
    ref ir.Name n = ref _addr_n.val;

    if (n.Sym().Pkg != types.LocalPkg) {
        return ;
    }
    if ((ctxt != ir.PEXTERN && ctxt != ir.PFUNC) || DeclContext != ir.PEXTERN) {
        return ;
    }
    if (n.Type() != null && n.Type().IsKind(types.TFUNC) && ir.IsMethod(n)) {
        return ;
    }
    if (types.IsExported(n.Sym().Name) || n.Sym().Name == "init") {
        Export(_addr_n);
    }
    if (@base.Flag.AsmHdr != "" && !n.Sym().Asm()) {
        n.Sym().SetAsm(true);
        Target.Asms = append(Target.Asms, n);
    }
}

// checkdupfields emits errors for duplicately named fields or methods in
// a list of struct or interface types.
private static void checkdupfields(@string what, params slice<ptr<types.Field>>[] fss) {
    fss = fss.Clone();

    var seen = make_map<ptr<types.Sym>, bool>();
    foreach (var (_, fs) in fss) {
        foreach (var (_, f) in fs) {
            if (f.Sym == null || f.Sym.IsBlank()) {
                continue;
            }
            if (seen[f.Sym]) {
                @base.ErrorfAt(f.Pos, "duplicate %s %s", what, f.Sym.Name);
                continue;
            }
            seen[f.Sym] = true;
        }
    }
}

// structs, functions, and methods.
// they don't belong here, but where do they belong?
private static void checkembeddedtype(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t == null) {
        return ;
    }
    if (t.Sym() == null && t.IsPtr()) {
        t = t.Elem();
        if (t.IsInterface()) {
            @base.Errorf("embedded type cannot be a pointer to interface");
        }
    }
    if (t.IsPtr() || t.IsUnsafePtr()) {
        @base.Errorf("embedded type cannot be a pointer");
    }
    else if (t.Kind() == types.TFORW && !t.ForwardType().Embedlineno.IsKnown()) {
        t.ForwardType().Embedlineno = @base.Pos;
    }
}

// TODO(mdempsky): Move to package types.
public static ptr<types.Field> FakeRecv() {
    return _addr_types.NewField(src.NoXPos, null, types.FakeRecvType())!;
}

private static var fakeRecvField = FakeRecv;

private static slice<funcStackEnt> funcStack = default; // stack of previous values of ir.CurFunc/DeclContext

private partial struct funcStackEnt {
    public ptr<ir.Func> curfn;
    public ir.Class dclcontext;
}

private static void funcarg(ptr<ir.Field> _addr_n, ir.Class ctxt) {
    ref ir.Field n = ref _addr_n.val;

    if (n.Sym == null) {
        return ;
    }
    var name = ir.NewNameAt(n.Pos, n.Sym);
    n.Decl = name;
    name.Ntype = n.Ntype;
    Declare(_addr_name, ctxt);
}

private static void funcarg2(ptr<types.Field> _addr_f, ir.Class ctxt) {
    ref types.Field f = ref _addr_f.val;

    if (f.Sym == null) {
        return ;
    }
    var n = ir.NewNameAt(f.Pos, f.Sym);
    f.Nname = n;
    n.SetType(f.Type);
    Declare(_addr_n, ctxt);
}

private static void funcargs(ptr<ir.FuncType> _addr_nt) {
    ref ir.FuncType nt = ref _addr_nt.val;

    if (nt.Op() != ir.OTFUNC) {
        @base.Fatalf("funcargs %v", nt.Op());
    }
    if (nt.Recv != null) {
        funcarg(_addr_nt.Recv, ir.PPARAM);
    }
    {
        var n__prev1 = n;

        foreach (var (_, __n) in nt.Params) {
            n = __n;
            funcarg(_addr_n, ir.PPARAM);
        }
        n = n__prev1;
    }

    var gen = len(nt.Params);
    {
        var n__prev1 = n;

        foreach (var (_, __n) in nt.Results) {
            n = __n;
            if (n.Sym == null) { 
                // Name so that escape analysis can track it. ~r stands for 'result'.
                n.Sym = LookupNum("~r", gen);
                gen++;
            }
            if (n.Sym.IsBlank()) { 
                // Give it a name so we can assign to it during return. ~b stands for 'blank'.
                // The name must be different from ~r above because if you have
                //    func f() (_ int)
                //    func g() int
                // f is allowed to use a plain 'return' with no arguments, while g is not.
                // So the two cases must be distinguished.
                n.Sym = LookupNum("~b", gen);
                gen++;
            }
            funcarg(_addr_n, ir.PPARAMOUT);
        }
        n = n__prev1;
    }
}

// Same as funcargs, except run over an already constructed TFUNC.
// This happens during import, where the hidden_fndcl rule has
// used functype directly to parse the function's type.
private static void funcargs2(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.Kind() != types.TFUNC) {
        @base.Fatalf("funcargs2 %v", t);
    }
    {
        var f__prev1 = f;

        foreach (var (_, __f) in t.Recvs().Fields().Slice()) {
            f = __f;
            funcarg2(_addr_f, ir.PPARAM);
        }
        f = f__prev1;
    }

    {
        var f__prev1 = f;

        foreach (var (_, __f) in t.Params().Fields().Slice()) {
            f = __f;
            funcarg2(_addr_f, ir.PPARAM);
        }
        f = f__prev1;
    }

    {
        var f__prev1 = f;

        foreach (var (_, __f) in t.Results().Fields().Slice()) {
            f = __f;
            funcarg2(_addr_f, ir.PPARAMOUT);
        }
        f = f__prev1;
    }
}

public static ptr<ir.Name> Temp(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    return _addr_TempAt(@base.Pos, _addr_ir.CurFunc, _addr_t)!;
}

// make a new Node off the books
public static ptr<ir.Name> TempAt(src.XPos pos, ptr<ir.Func> _addr_curfn, ptr<types.Type> _addr_t) {
    ref ir.Func curfn = ref _addr_curfn.val;
    ref types.Type t = ref _addr_t.val;

    if (curfn == null) {
        @base.Fatalf("no curfn for TempAt");
    }
    if (curfn.Op() == ir.OCLOSURE) {
        ir.Dump("TempAt", curfn);
        @base.Fatalf("adding TempAt to wrong closure function");
    }
    if (t == null) {
        @base.Fatalf("TempAt called with nil type");
    }
    if (t.Kind() == types.TFUNC && t.Recv() != null) {
        @base.Fatalf("misuse of method type: %v", t);
    }
    ptr<types.Sym> s = addr(new types.Sym(Name:autotmpname(len(curfn.Dcl)),Pkg:types.LocalPkg,));
    var n = ir.NewNameAt(pos, s);
    s.Def = n;
    n.SetType(t);
    n.Class = ir.PAUTO;
    n.SetEsc(ir.EscNever);
    n.Curfn = curfn;
    n.SetUsed(true);
    n.SetAutoTemp(true);
    curfn.Dcl = append(curfn.Dcl, n);

    types.CalcSize(t);

    return _addr_n!;
}

// autotmpname returns the name for an autotmp variable numbered n.
private static @string autotmpname(nint n) { 
    // Give each tmp a different name so that they can be registerized.
    // Add a preceding . to avoid clashing with legal names.
    const @string prefix = ".autotmp_"; 
    // Start with a buffer big enough to hold a large n.
 
    // Start with a buffer big enough to hold a large n.
    slice<byte> b = (slice<byte>)prefix + "      "[..(int)len(prefix)];
    b = strconv.AppendInt(b, int64(n), 10);
    return types.InternString(b);
}

// f is method type, with receiver.
// return function type, receiver as first argument (or not).
public static ptr<types.Type> NewMethodType(ptr<types.Type> _addr_sig, ptr<types.Type> _addr_recv) {
    ref types.Type sig = ref _addr_sig.val;
    ref types.Type recv = ref _addr_recv.val;

    nint nrecvs = 0;
    if (recv != null) {
        nrecvs++;
    }
    var @params = make_slice<ptr<types.Field>>(nrecvs + sig.Params().Fields().Len());
    if (recv != null) {
        params[0] = types.NewField(@base.Pos, null, recv);
    }
    {
        var i__prev1 = i;

        foreach (var (__i, __param) in sig.Params().Fields().Slice()) {
            i = __i;
            param = __param;
            var d = types.NewField(@base.Pos, null, param.Type);
            d.SetIsDDD(param.IsDDD());
            params[nrecvs + i] = d;
        }
        i = i__prev1;
    }

    var results = make_slice<ptr<types.Field>>(sig.Results().Fields().Len());
    {
        var i__prev1 = i;

        foreach (var (__i, __t) in sig.Results().Fields().Slice()) {
            i = __i;
            t = __t;
            results[i] = types.NewField(@base.Pos, null, t.Type);
        }
        i = i__prev1;
    }

    return _addr_types.NewSignature(types.LocalPkg, null, null, params, results)!;
}

} // end typecheck_package
