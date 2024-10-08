// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:59:08 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\scope.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using src = cmd.@internal.src_package;


// Declaration stack & operations

public static partial class types_package {

private static int blockgen = 1; // max block number
public static int Block = 1; // current block number

// A dsym stores a symbol's shadowed declaration so that it can be
// restored once the block scope ends.
private partial struct dsym {
    public ptr<Sym> sym; // sym == nil indicates stack mark
    public Object def;
    public int block;
    public src.XPos lastlineno; // last declaration for diagnostic
}

// dclstack maintains a stack of shadowed symbol declarations so that
// Popdcl can restore their declarations when a block scope ends.
private static slice<dsym> dclstack = default;

// Pushdcl pushes the current declaration for symbol s (if any) so that
// it can be shadowed by a new declaration within a nested block scope.
public static void Pushdcl(ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    dclstack = append(dclstack, new dsym(sym:s,def:s.Def,block:s.Block,lastlineno:s.Lastlineno,));
}

// Popdcl pops the innermost block scope and restores all symbol declarations
// to their previous state.
public static void Popdcl() {
    for (var i = len(dclstack); i > 0; i--) {
        var d = _addr_dclstack[i - 1];
        var s = d.sym;
        if (s == null) { 
            // pop stack mark
            Block = d.block;
            dclstack = dclstack[..(int)i - 1];
            return ;
        }
        s.Def = d.def;
        s.Block = d.block;
        s.Lastlineno = d.lastlineno; 

        // Clear dead pointer fields.
        d.sym = null;
        d.def = null;
    }
    @base.Fatalf("popdcl: no stack mark");
}

// Markdcl records the start of a new block scope for declarations.
public static void Markdcl() {
    dclstack = append(dclstack, new dsym(sym:nil,block:Block,));
    blockgen++;
    Block = blockgen;
}

private static bool isDclstackValid() {
    foreach (var (_, d) in dclstack) {
        if (d.sym == null) {
            return false;
        }
    }    return true;
}

// PkgDef returns the definition associated with s at package scope.
private static Object PkgDef(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return s.pkgDefPtr().val;
}

// SetPkgDef sets the definition associated with s at package scope.
private static void SetPkgDef(this ptr<Sym> _addr_s, Object n) {
    ref Sym s = ref _addr_s.val;

    s.pkgDefPtr().val = n;
}

private static ptr<Object> pkgDefPtr(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;
 
    // Look for outermost saved declaration, which must be the
    // package scope definition, if present.
    foreach (var (i) in dclstack) {
        var d = _addr_dclstack[i];
        if (s == d.sym) {
            return _addr__addr_d.def!;
        }
    }    return _addr__addr_s.Def!;
}

public static void CheckDclstack() {
    if (!isDclstackValid()) {
        @base.Fatalf("mark left on the dclstack");
    }
}

} // end types_package
