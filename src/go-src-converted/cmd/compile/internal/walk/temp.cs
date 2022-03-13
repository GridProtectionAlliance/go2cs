// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package walk -- go2cs converted at 2022 March 13 06:25:28 UTC
// import "cmd/compile/internal/walk" ==> using walk = go.cmd.compile.@internal.walk_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\walk\temp.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;


// initStackTemp appends statements to init to initialize the given
// temporary variable to val, and then returns the expression &tmp.

public static partial class walk_package {

private static ptr<ir.AddrExpr> initStackTemp(ptr<ir.Nodes> _addr_init, ptr<ir.Name> _addr_tmp, ir.Node val) {
    ref ir.Nodes init = ref _addr_init.val;
    ref ir.Name tmp = ref _addr_tmp.val;

    if (val != null && !types.Identical(tmp.Type(), val.Type())) {
        @base.Fatalf("bad initial value for %L: %L", tmp, val);
    }
    appendWalkStmt(init, ir.NewAssignStmt(@base.Pos, tmp, val));
    return typecheck.Expr(typecheck.NodAddr(tmp))._<ptr<ir.AddrExpr>>();
}

// stackTempAddr returns the expression &tmp, where tmp is a newly
// allocated temporary variable of the given type. Statements to
// zero-initialize tmp are appended to init.
private static ptr<ir.AddrExpr> stackTempAddr(ptr<ir.Nodes> _addr_init, ptr<types.Type> _addr_typ) {
    ref ir.Nodes init = ref _addr_init.val;
    ref types.Type typ = ref _addr_typ.val;

    return _addr_initStackTemp(_addr_init, _addr_typecheck.Temp(typ), null)!;
}

// stackBufAddr returns thte expression &tmp, where tmp is a newly
// allocated temporary variable of type [len]elem. This variable is
// initialized, and elem must not contain pointers.
private static ptr<ir.AddrExpr> stackBufAddr(long len, ptr<types.Type> _addr_elem) {
    ref types.Type elem = ref _addr_elem.val;

    if (elem.HasPointers()) {
        @base.FatalfAt(@base.Pos, "%v has pointers", elem);
    }
    var tmp = typecheck.Temp(types.NewArray(elem, len));
    return typecheck.Expr(typecheck.NodAddr(tmp))._<ptr<ir.AddrExpr>>();
}

} // end walk_package
