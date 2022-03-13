// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 13 06:00:04 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\syms.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using src = cmd.@internal.src_package;

public static partial class typecheck_package {

public static ptr<ir.Name> LookupRuntime(@string name) {
    var s = ir.Pkgs.Runtime.Lookup(name);
    if (s == null || s.Def == null) {
        @base.Fatalf("LookupRuntime: can't find runtime.%s", name);
    }
    return ir.AsNode(s.Def)._<ptr<ir.Name>>();
}

// SubstArgTypes substitutes the given list of types for
// successive occurrences of the "any" placeholder in the
// type syntax expression n.Type.
// The result of SubstArgTypes MUST be assigned back to old, e.g.
//     n.Left = SubstArgTypes(n.Left, t1, t2)
public static ptr<ir.Name> SubstArgTypes(ptr<ir.Name> _addr_old, params ptr<ptr<types.Type>>[] _addr_types_) {
    types_ = types_.Clone();
    ref ir.Name old = ref _addr_old.val;
    ref types.Type types_ = ref _addr_types_.val;

    foreach (var (_, t) in types_) {
        types.CalcSize(t);
    }    var n = ir.NewNameAt(old.Pos(), old.Sym());
    n.Class = old.Class;
    n.SetType(types.SubstAny(old.Type(), _addr_types_));
    n.Func = old.Func;
    if (len(types_) > 0) {
        @base.Fatalf("SubstArgTypes: too many argument types");
    }
    return _addr_n!;
}

// AutoLabel generates a new Name node for use with
// an automatically generated label.
// prefix is a short mnemonic (e.g. ".s" for switch)
// to help with debugging.
// It should begin with "." to avoid conflicts with
// user labels.
public static ptr<types.Sym> AutoLabel(@string prefix) {
    if (prefix[0] != '.') {
        @base.Fatalf("autolabel prefix must start with '.', have %q", prefix);
    }
    var fn = ir.CurFunc;
    if (ir.CurFunc == null) {
        @base.Fatalf("autolabel outside function");
    }
    var n = fn.Label;
    fn.Label++;
    return _addr_LookupNum(prefix, int(n))!;
}

public static ptr<types.Sym> Lookup(@string name) {
    return _addr_types.LocalPkg.Lookup(name)!;
}

// InitRuntime loads the definitions for the low-level runtime functions,
// so that the compiler can generate calls to them,
// but does not make them visible to user code.
public static void InitRuntime() {
    @base.Timer.Start("fe", "loadsys");
    types.Block = 1;

    var typs = runtimeTypes();
    foreach (var (_, d) in _addr_runtimeDecls) {
        var sym = ir.Pkgs.Runtime.Lookup(d.name);
        var typ = typs[d.typ];

        if (d.tag == funcTag) 
            importfunc(ir.Pkgs.Runtime, src.NoXPos, sym, typ);
        else if (d.tag == varTag) 
            importvar(ir.Pkgs.Runtime, src.NoXPos, sym, typ);
        else 
            @base.Fatalf("unhandled declaration tag %v", d.tag);
            }
}

// LookupRuntimeFunc looks up Go function name in package runtime. This function
// must follow the internal calling convention.
public static ptr<obj.LSym> LookupRuntimeFunc(@string name) {
    return _addr_LookupRuntimeABI(name, obj.ABIInternal)!;
}

// LookupRuntimeVar looks up a variable (or assembly function) name in package
// runtime. If this is a function, it may have a special calling
// convention.
public static ptr<obj.LSym> LookupRuntimeVar(@string name) {
    return _addr_LookupRuntimeABI(name, obj.ABI0)!;
}

// LookupRuntimeABI looks up a name in package runtime using the given ABI.
public static ptr<obj.LSym> LookupRuntimeABI(@string name, obj.ABI abi) {
    return _addr_@base.PkgLinksym("runtime", name, abi)!;
}

} // end typecheck_package
