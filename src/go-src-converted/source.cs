// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:33:26 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\source.go
// This file defines utilities for working with source positions
// or source-level named entities ("objects").

// TODO(adonovan): test that {Value,Instruction}.Pos() positions match
// the originating syntax, as specified.

using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;

namespace go.golang.org.x.tools.go;

public static partial class ssa_package {

    // EnclosingFunction returns the function that contains the syntax
    // node denoted by path.
    //
    // Syntax associated with package-level variable specifications is
    // enclosed by the package's init() function.
    //
    // Returns nil if not found; reasons might include:
    //    - the node is not enclosed by any function.
    //    - the node is within an anonymous function (FuncLit) and
    //      its SSA function has not been created yet
    //      (pkg.Build() has not yet been called).
    //
public static ptr<Function> EnclosingFunction(ptr<Package> _addr_pkg, slice<ast.Node> path) {
    ref Package pkg = ref _addr_pkg.val;
 
    // Start with package-level function...
    var fn = findEnclosingPackageLevelFunction(_addr_pkg, path);
    if (fn == null) {
        return _addr_null!; // not in any function
    }
    var n = len(path);
outer:
    foreach (var (i) in path) {
        {
            ptr<ast.FuncLit> (lit, ok) = path[n - 1 - i]._<ptr<ast.FuncLit>>();

            if (ok) {
                foreach (var (_, anon) in fn.AnonFuncs) {
                    if (anon.Pos() == lit.Type.Func) {
                        fn = anon;
                        _continueouter = true;
                        break;
                    }
                }                return _addr_null!;

            }
        }

    }    return _addr_fn!;

}

// HasEnclosingFunction returns true if the AST node denoted by path
// is contained within the declaration of some function or
// package-level variable.
//
// Unlike EnclosingFunction, the behaviour of this function does not
// depend on whether SSA code for pkg has been built, so it can be
// used to quickly reject check inputs that will cause
// EnclosingFunction to fail, prior to SSA building.
//
public static bool HasEnclosingFunction(ptr<Package> _addr_pkg, slice<ast.Node> path) {
    ref Package pkg = ref _addr_pkg.val;

    return findEnclosingPackageLevelFunction(_addr_pkg, path) != null;
}

// findEnclosingPackageLevelFunction returns the Function
// corresponding to the package-level function enclosing path.
//
private static ptr<Function> findEnclosingPackageLevelFunction(ptr<Package> _addr_pkg, slice<ast.Node> path) {
    ref Package pkg = ref _addr_pkg.val;

    {
        var n = len(path);

        if (n >= 2) { // [... {Gen,Func}Decl File]
            switch (path[n - 2].type()) {
                case ptr<ast.GenDecl> decl:
                    if (decl.Tok == token.VAR && n >= 3) { 
                        // Package-level 'var' initializer.
                        return _addr_pkg.init!;

                    }

                    break;
                case ptr<ast.FuncDecl> decl:
                    if (decl.Recv == null && decl.Name.Name == "init") { 
                        // Explicit init() function.
                        foreach (var (_, b) in pkg.init.Blocks) {
                            {
                                var instr__prev2 = instr;

                                foreach (var (_, __instr) in b.Instrs) {
                                    instr = __instr;
                                    {
                                        var instr__prev3 = instr;

                                        ptr<Call> (instr, ok) = instr._<ptr<Call>>();

                                        if (ok) {
                                            {
                                                ptr<Function> (callee, ok) = instr.Call.Value._<ptr<Function>>();

                                                if (ok && callee.Pkg == pkg && callee.Pos() == decl.Name.NamePos) {
                                                    return _addr_callee!;
                                                }

                                            }

                                        }

                                        instr = instr__prev3;

                                    }

                                }

                                instr = instr__prev2;
                            }
                        } 
                        // Hack: return non-nil when SSA is not yet
                        // built so that HasEnclosingFunction works.
                        return _addr_pkg.init!;

                    } 
                    // Declared function/method.
                    return _addr_findNamedFunc(_addr_pkg, decl.Name.NamePos)!;
                    break;
            }

        }
    }

    return _addr_null!; // not in any function
}

// findNamedFunc returns the named function whose FuncDecl.Ident is at
// position pos.
//
private static ptr<Function> findNamedFunc(ptr<Package> _addr_pkg, token.Pos pos) {
    ref Package pkg = ref _addr_pkg.val;
 
    // Look at all package members and method sets of named types.
    // Not very efficient.
    {
        var mem__prev1 = mem;

        foreach (var (_, __mem) in pkg.Members) {
            mem = __mem;
            switch (mem.type()) {
                case ptr<Function> mem:
                    if (mem.Pos() == pos) {
                        return _addr_mem!;
                    }
                    break;
                case ptr<Type> mem:
                    var mset = pkg.Prog.MethodSets.MethodSet(types.NewPointer(mem.Type()));
                    for (nint i = 0;
                    var n = mset.Len(); i < n; i++) { 
                        // Don't call Program.Method: avoid creating wrappers.
                        ptr<types.Func> obj = mset.At(i).Obj()._<ptr<types.Func>>();
                        if (obj.Pos() == pos) {
                            return pkg.values[obj]._<ptr<Function>>();
                        }

                    }
                    break;
            }

        }
        mem = mem__prev1;
    }

    return _addr_null!;

}

// ValueForExpr returns the SSA Value that corresponds to non-constant
// expression e.
//
// It returns nil if no value was found, e.g.
//    - the expression is not lexically contained within f;
//    - f was not built with debug information; or
//    - e is a constant expression.  (For efficiency, no debug
//      information is stored for constants. Use
//      go/types.Info.Types[e].Value instead.)
//    - e is a reference to nil or a built-in function.
//    - the value was optimised away.
//
// If e is an addressable expression used in an lvalue context,
// value is the address denoted by e, and isAddr is true.
//
// The types of e (or &e, if isAddr) and the result are equal
// (modulo "untyped" bools resulting from comparisons).
//
// (Tip: to find the ssa.Value given a source position, use
// astutil.PathEnclosingInterval to locate the ast.Node, then
// EnclosingFunction to locate the Function, then ValueForExpr to find
// the ssa.Value.)
//
private static (Value, bool) ValueForExpr(this ptr<Function> _addr_f, ast.Expr e) {
    Value value = default;
    bool isAddr = default;
    ref Function f = ref _addr_f.val;

    if (f.debugInfo()) { // (opt)
        e = unparen(e);
        foreach (var (_, b) in f.Blocks) {
            foreach (var (_, instr) in b.Instrs) {
                {
                    ptr<DebugRef> (ref, ok) = instr._<ptr<DebugRef>>();

                    if (ok) {
                        if (@ref.Expr == e) {
                            return (@ref.X, @ref.IsAddr);
                        }
                    }

                }

            }

        }
    }
    return ;

}

// --- Lookup functions for source-level named entities (types.Objects) ---

// Package returns the SSA Package corresponding to the specified
// type-checker package object.
// It returns nil if no such SSA package has been created.
//
private static ptr<Package> Package(this ptr<Program> _addr_prog, ptr<types.Package> _addr_obj) {
    ref Program prog = ref _addr_prog.val;
    ref types.Package obj = ref _addr_obj.val;

    return _addr_prog.packages[obj]!;
}

// packageLevelValue returns the package-level value corresponding to
// the specified named object, which may be a package-level const
// (*Const), var (*Global) or func (*Function) of some package in
// prog.  It returns nil if the object is not found.
//
private static Value packageLevelValue(this ptr<Program> _addr_prog, types.Object obj) {
    ref Program prog = ref _addr_prog.val;

    {
        var (pkg, ok) = prog.packages[obj.Pkg()];

        if (ok) {
            return pkg.values[obj];
        }
    }

    return null;

}

// FuncValue returns the concrete Function denoted by the source-level
// named function obj, or nil if obj denotes an interface method.
//
// TODO(adonovan): check the invariant that obj.Type() matches the
// result's Signature, both in the params/results and in the receiver.
//
private static ptr<Function> FuncValue(this ptr<Program> _addr_prog, ptr<types.Func> _addr_obj) {
    ref Program prog = ref _addr_prog.val;
    ref types.Func obj = ref _addr_obj.val;

    ptr<Function> (fn, _) = prog.packageLevelValue(obj)._<ptr<Function>>();
    return _addr_fn!;
}

// ConstValue returns the SSA Value denoted by the source-level named
// constant obj.
//
private static ptr<Const> ConstValue(this ptr<Program> _addr_prog, ptr<types.Const> _addr_obj) {
    ref Program prog = ref _addr_prog.val;
    ref types.Const obj = ref _addr_obj.val;
 
    // TODO(adonovan): opt: share (don't reallocate)
    // Consts for const objects and constant ast.Exprs.

    // Universal constant? {true,false,nil}
    if (obj.Parent() == types.Universe) {
        return _addr_NewConst(obj.Val(), obj.Type())!;
    }
    {
        var v = prog.packageLevelValue(obj);

        if (v != null) {
            return v._<ptr<Const>>();
        }
    }

    return _addr_NewConst(obj.Val(), obj.Type())!;

}

// VarValue returns the SSA Value that corresponds to a specific
// identifier denoting the source-level named variable obj.
//
// VarValue returns nil if a local variable was not found, perhaps
// because its package was not built, the debug information was not
// requested during SSA construction, or the value was optimized away.
//
// ref is the path to an ast.Ident (e.g. from PathEnclosingInterval),
// and that ident must resolve to obj.
//
// pkg is the package enclosing the reference.  (A reference to a var
// always occurs within a function, so we need to know where to find it.)
//
// If the identifier is a field selector and its base expression is
// non-addressable, then VarValue returns the value of that field.
// For example:
//    func f() struct {x int}
//    f().x  // VarValue(x) returns a *Field instruction of type int
//
// All other identifiers denote addressable locations (variables).
// For them, VarValue may return either the variable's address or its
// value, even when the expression is evaluated only for its value; the
// situation is reported by isAddr, the second component of the result.
//
// If !isAddr, the returned value is the one associated with the
// specific identifier.  For example,
//       var x int    // VarValue(x) returns Const 0 here
//       x = 1        // VarValue(x) returns Const 1 here
//
// It is not specified whether the value or the address is returned in
// any particular case, as it may depend upon optimizations performed
// during SSA code generation, such as registerization, constant
// folding, avoidance of materialization of subexpressions, etc.
//
private static (Value, bool) VarValue(this ptr<Program> _addr_prog, ptr<types.Var> _addr_obj, ptr<Package> _addr_pkg, slice<ast.Node> @ref) {
    Value value = default;
    bool isAddr = default;
    ref Program prog = ref _addr_prog.val;
    ref types.Var obj = ref _addr_obj.val;
    ref Package pkg = ref _addr_pkg.val;
 
    // All references to a var are local to some function, possibly init.
    var fn = EnclosingFunction(_addr_pkg, ref);
    if (fn == null) {
        return ; // e.g. def of struct field; SSA not built?
    }
    ptr<ast.Ident> id = ref[0]._<ptr<ast.Ident>>(); 

    // Defining ident of a parameter?
    if (id.Pos() == obj.Pos()) {
        foreach (var (_, param) in fn.Params) {
            if (param.Object() == obj) {
                return (param, false);
            }
        }
    }
    foreach (var (_, b) in fn.Blocks) {
        foreach (var (_, instr) in b.Instrs) {
            {
                ptr<DebugRef> (dr, ok) = instr._<ptr<DebugRef>>();

                if (ok) {
                    if (dr.Pos() == id.Pos()) {
                        return (dr.X, dr.IsAddr);
                    }
                }

            }

        }
    }    {
        var v = prog.packageLevelValue(obj);

        if (v != null) {
            return (v._<ptr<Global>>(), true);
        }
    }


    return ; // e.g. debug info not requested, or var optimized away
}

} // end ssa_package
