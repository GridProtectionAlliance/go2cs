// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package copylock defines an Analyzer that checks for locks
// erroneously passed by value.
// package copylock -- go2cs converted at 2020 October 09 06:03:57 UTC
// import "golang.org/x/tools/go/analysis/passes/copylock" ==> using copylock = go.golang.org.x.tools.go.analysis.passes.copylock_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\copylock\copylock.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class copylock_package
    {
        public static readonly @string Doc = (@string)"check for locks erroneously passed by value\n\nInadvertently copying a value contai" +
    "ning a lock, such as sync.Mutex or\nsync.WaitGroup, may cause both copies to malf" +
    "unction. Generally such\nvalues should be referred to through a pointer.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"copylocks",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},RunDespiteErrors:true,Run:run,));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.AssignStmt)(nil), (*ast.CallExpr)(nil), (*ast.CompositeLit)(nil), (*ast.FuncDecl)(nil), (*ast.FuncLit)(nil), (*ast.GenDecl)(nil), (*ast.RangeStmt)(nil), (*ast.ReturnStmt)(nil) });
            inspect.Preorder(nodeFilter, node =>
            {
                switch (node.type())
                {
                    case ptr<ast.RangeStmt> node:
                        checkCopyLocksRange(_addr_pass, _addr_node);
                        break;
                    case ptr<ast.FuncDecl> node:
                        checkCopyLocksFunc(_addr_pass, node.Name.Name, _addr_node.Recv, _addr_node.Type);
                        break;
                    case ptr<ast.FuncLit> node:
                        checkCopyLocksFunc(_addr_pass, "func", _addr_null, _addr_node.Type);
                        break;
                    case ptr<ast.CallExpr> node:
                        checkCopyLocksCallExpr(_addr_pass, _addr_node);
                        break;
                    case ptr<ast.AssignStmt> node:
                        checkCopyLocksAssign(_addr_pass, _addr_node);
                        break;
                    case ptr<ast.GenDecl> node:
                        checkCopyLocksGenDecl(_addr_pass, _addr_node);
                        break;
                    case ptr<ast.CompositeLit> node:
                        checkCopyLocksCompositeLit(_addr_pass, _addr_node);
                        break;
                    case ptr<ast.ReturnStmt> node:
                        checkCopyLocksReturnStmt(_addr_pass, _addr_node);
                        break;
                }

            });
            return (null, error.As(null!)!);

        }

        // checkCopyLocksAssign checks whether an assignment
        // copies a lock.
        private static void checkCopyLocksAssign(ptr<analysis.Pass> _addr_pass, ptr<ast.AssignStmt> _addr_@as)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.AssignStmt @as = ref _addr_@as.val;

            foreach (var (i, x) in @as.Rhs)
            {
                {
                    var path = lockPathRhs(_addr_pass, x);

                    if (path != null)
                    {
                        pass.ReportRangef(x, "assignment copies lock value to %v: %v", analysisutil.Format(pass.Fset, @as.Lhs[i]), path);
                    }

                }

            }

        }

        // checkCopyLocksGenDecl checks whether lock is copied
        // in variable declaration.
        private static void checkCopyLocksGenDecl(ptr<analysis.Pass> _addr_pass, ptr<ast.GenDecl> _addr_gd)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.GenDecl gd = ref _addr_gd.val;

            if (gd.Tok != token.VAR)
            {
                return ;
            }

            foreach (var (_, spec) in gd.Specs)
            {
                ptr<ast.ValueSpec> valueSpec = spec._<ptr<ast.ValueSpec>>();
                foreach (var (i, x) in valueSpec.Values)
                {
                    {
                        var path = lockPathRhs(_addr_pass, x);

                        if (path != null)
                        {
                            pass.ReportRangef(x, "variable declaration copies lock value to %v: %v", valueSpec.Names[i].Name, path);
                        }

                    }

                }

            }

        }

        // checkCopyLocksCompositeLit detects lock copy inside a composite literal
        private static void checkCopyLocksCompositeLit(ptr<analysis.Pass> _addr_pass, ptr<ast.CompositeLit> _addr_cl)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CompositeLit cl = ref _addr_cl.val;

            foreach (var (_, x) in cl.Elts)
            {
                {
                    ptr<ast.KeyValueExpr> (node, ok) = x._<ptr<ast.KeyValueExpr>>();

                    if (ok)
                    {
                        x = node.Value;
                    }

                }

                {
                    var path = lockPathRhs(_addr_pass, x);

                    if (path != null)
                    {
                        pass.ReportRangef(x, "literal copies lock value from %v: %v", analysisutil.Format(pass.Fset, x), path);
                    }

                }

            }

        }

        // checkCopyLocksReturnStmt detects lock copy in return statement
        private static void checkCopyLocksReturnStmt(ptr<analysis.Pass> _addr_pass, ptr<ast.ReturnStmt> _addr_rs)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.ReturnStmt rs = ref _addr_rs.val;

            foreach (var (_, x) in rs.Results)
            {
                {
                    var path = lockPathRhs(_addr_pass, x);

                    if (path != null)
                    {
                        pass.ReportRangef(x, "return copies lock value: %v", path);
                    }

                }

            }

        }

        // checkCopyLocksCallExpr detects lock copy in the arguments to a function call
        private static void checkCopyLocksCallExpr(ptr<analysis.Pass> _addr_pass, ptr<ast.CallExpr> _addr_ce)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CallExpr ce = ref _addr_ce.val;

            ptr<ast.Ident> id;
            switch (ce.Fun.type())
            {
                case ptr<ast.Ident> fun:
                    id = fun;
                    break;
                case ptr<ast.SelectorExpr> fun:
                    id = fun.Sel;
                    break;
            }
            {
                var fun__prev1 = fun;

                ptr<types.Builtin> (fun, ok) = pass.TypesInfo.Uses[id]._<ptr<types.Builtin>>();

                if (ok)
                {
                    switch (fun.Name())
                    {
                        case "new": 

                        case "len": 

                        case "cap": 

                        case "Sizeof": 
                            return ;
                            break;
                    }

                }

                fun = fun__prev1;

            }

            foreach (var (_, x) in ce.Args)
            {
                {
                    var path = lockPathRhs(_addr_pass, x);

                    if (path != null)
                    {
                        pass.ReportRangef(x, "call of %s copies lock value: %v", analysisutil.Format(pass.Fset, ce.Fun), path);
                    }

                }

            }

        }

        // checkCopyLocksFunc checks whether a function might
        // inadvertently copy a lock, by checking whether
        // its receiver, parameters, or return values
        // are locks.
        private static void checkCopyLocksFunc(ptr<analysis.Pass> _addr_pass, @string name, ptr<ast.FieldList> _addr_recv, ptr<ast.FuncType> _addr_typ)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.FieldList recv = ref _addr_recv.val;
            ref ast.FuncType typ = ref _addr_typ.val;

            if (recv != null && len(recv.List) > 0L)
            {
                var expr = recv.List[0L].Type;
                {
                    var path__prev2 = path;

                    var path = lockPath(_addr_pass.Pkg, pass.TypesInfo.Types[expr].Type);

                    if (path != null)
                    {
                        pass.ReportRangef(expr, "%s passes lock by value: %v", name, path);
                    }

                    path = path__prev2;

                }

            }

            if (typ.Params != null)
            {
                foreach (var (_, field) in typ.Params.List)
                {
                    expr = field.Type;
                    {
                        var path__prev2 = path;

                        path = lockPath(_addr_pass.Pkg, pass.TypesInfo.Types[expr].Type);

                        if (path != null)
                        {
                            pass.ReportRangef(expr, "%s passes lock by value: %v", name, path);
                        }

                        path = path__prev2;

                    }

                }

            } 

            // Don't check typ.Results. If T has a Lock field it's OK to write
            //     return T{}
            // because that is returning the zero value. Leave result checking
            // to the return statement.
        }

        // checkCopyLocksRange checks whether a range statement
        // might inadvertently copy a lock by checking whether
        // any of the range variables are locks.
        private static void checkCopyLocksRange(ptr<analysis.Pass> _addr_pass, ptr<ast.RangeStmt> _addr_r)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.RangeStmt r = ref _addr_r.val;

            checkCopyLocksRangeVar(_addr_pass, r.Tok, r.Key);
            checkCopyLocksRangeVar(_addr_pass, r.Tok, r.Value);
        }

        private static void checkCopyLocksRangeVar(ptr<analysis.Pass> _addr_pass, token.Token rtok, ast.Expr e)
        {
            ref analysis.Pass pass = ref _addr_pass.val;

            if (e == null)
            {
                return ;
            }

            ptr<ast.Ident> (id, isId) = e._<ptr<ast.Ident>>();
            if (isId && id.Name == "_")
            {
                return ;
            }

            types.Type typ = default;
            if (rtok == token.DEFINE)
            {
                if (!isId)
                {
                    return ;
                }

                var obj = pass.TypesInfo.Defs[id];
                if (obj == null)
                {
                    return ;
                }

                typ = obj.Type();

            }
            else
            {
                typ = pass.TypesInfo.Types[e].Type;
            }

            if (typ == null)
            {
                return ;
            }

            {
                var path = lockPath(_addr_pass.Pkg, typ);

                if (path != null)
                {
                    pass.Reportf(e.Pos(), "range var %s copies lock: %v", analysisutil.Format(pass.Fset, e), path);
                }

            }

        }

        private partial struct typePath // : slice<types.Type>
        {
        }

        // String pretty-prints a typePath.
        private static @string String(this typePath path)
        {
            var n = len(path);
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            foreach (var (i) in path)
            {
                if (i > 0L)
                {
                    fmt.Fprint(_addr_buf, " contains ");
                } 
                // The human-readable path is in reverse order, outermost to innermost.
                fmt.Fprint(_addr_buf, path[n - i - 1L].String());

            }
            return buf.String();

        }

        private static typePath lockPathRhs(ptr<analysis.Pass> _addr_pass, ast.Expr x)
        {
            ref analysis.Pass pass = ref _addr_pass.val;

            {
                ptr<ast.CompositeLit> (_, ok) = x._<ptr<ast.CompositeLit>>();

                if (ok)
                {
                    return null;
                }

            }

            {
                (_, ok) = x._<ptr<ast.CallExpr>>();

                if (ok)
                { 
                    // A call may return a zero value.
                    return null;

                }

            }

            {
                ptr<ast.StarExpr> (star, ok) = x._<ptr<ast.StarExpr>>();

                if (ok)
                {
                    {
                        (_, ok) = star.X._<ptr<ast.CallExpr>>();

                        if (ok)
                        { 
                            // A call may return a pointer to a zero value.
                            return null;

                        }

                    }

                }

            }

            return lockPath(_addr_pass.Pkg, pass.TypesInfo.Types[x].Type);

        }

        // lockPath returns a typePath describing the location of a lock value
        // contained in typ. If there is no contained lock, it returns nil.
        private static typePath lockPath(ptr<types.Package> _addr_tpkg, types.Type typ)
        {
            ref types.Package tpkg = ref _addr_tpkg.val;

            if (typ == null)
            {
                return null;
            }

            while (true)
            {
                ptr<types.Array> (atyp, ok) = typ.Underlying()._<ptr<types.Array>>();
                if (!ok)
                {
                    break;
                }

                typ = atyp.Elem();

            } 

            // We're only interested in the case in which the underlying
            // type is a struct. (Interfaces and pointers are safe to copy.)
 

            // We're only interested in the case in which the underlying
            // type is a struct. (Interfaces and pointers are safe to copy.)
            ptr<types.Struct> (styp, ok) = typ.Underlying()._<ptr<types.Struct>>();
            if (!ok)
            {
                return null;
            } 

            // We're looking for cases in which a pointer to this type
            // is a sync.Locker, but a value is not. This differentiates
            // embedded interfaces from embedded values.
            if (types.Implements(types.NewPointer(typ), lockerType) && !types.Implements(typ, lockerType))
            {
                return new slice<types.Type>(new types.Type[] { typ });
            } 

            // In go1.10, sync.noCopy did not implement Locker.
            // (The Unlock method was added only in CL 121876.)
            // TODO(adonovan): remove workaround when we drop go1.10.
            {
                ptr<types.Named> (named, ok) = typ._<ptr<types.Named>>();

                if (ok && named.Obj().Name() == "noCopy" && named.Obj().Pkg().Path() == "sync")
                {
                    return new slice<types.Type>(new types.Type[] { typ });
                }

            }


            var nfields = styp.NumFields();
            for (long i = 0L; i < nfields; i++)
            {
                var ftyp = styp.Field(i).Type();
                var subpath = lockPath(_addr_tpkg, ftyp);
                if (subpath != null)
                {
                    return append(subpath, typ);
                }

            }


            return null;

        }

        private static ptr<types.Interface> lockerType;

        // Construct a sync.Locker interface type.
        private static void init()
        {
            var nullary = types.NewSignature(null, null, null, false); // func()
            ptr<types.Func> methods = new slice<ptr<types.Func>>(new ptr<types.Func>[] { types.NewFunc(token.NoPos,nil,"Lock",nullary), types.NewFunc(token.NoPos,nil,"Unlock",nullary) });
            lockerType = types.NewInterface(methods, null).Complete();

        }
    }
}}}}}}}
