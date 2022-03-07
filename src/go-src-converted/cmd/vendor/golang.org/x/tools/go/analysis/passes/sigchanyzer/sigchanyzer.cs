// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sigchanyzer defines an Analyzer that detects
// misuse of unbuffered signal as argument to signal.Notify.
// package sigchanyzer -- go2cs converted at 2022 March 06 23:34:46 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/sigchanyzer" ==> using sigchanyzer = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.sigchanyzer_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\sigchanyzer\sigchanyzer.go
using bytes = go.bytes_package;
using ast = go.go.ast_package;
using format = go.go.format_package;
using token = go.go.token_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

public static partial class sigchanyzer_package {

public static readonly @string Doc = "check for unbuffered channel of os.Signal\n\nThis checker reports call expression o" +
    "f the form signal.Notify(c <-chan os.Signal, sig ...os.Signal),\nwhere c is an un" +
    "buffered channel, which can be at risk of missing the signal.";

// Analyzer describes sigchanyzer analysis function detector.


// Analyzer describes sigchanyzer analysis function detector.
public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"sigchanyzer",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });
    inspect.Preorder(nodeFilter, n => {
        ptr<ast.CallExpr> call = n._<ptr<ast.CallExpr>>();
        if (!isSignalNotify(_addr_pass.TypesInfo, call)) {
            return ;
        }
        ptr<ast.CallExpr> chanDecl;
        switch (call.Args[0].type()) {
            case ptr<ast.Ident> arg:
                {
                    ptr<ast.CallExpr> (decl, ok) = findDecl(_addr_arg)._<ptr<ast.CallExpr>>();

                    if (ok) {
                        chanDecl = decl;
                    }

                }

                break;
            case ptr<ast.CallExpr> arg:
                if (isBuiltinMake(_addr_pass.TypesInfo, _addr_arg)) {
                    return ;
                }
                chanDecl = arg;
                break;
        }
        if (chanDecl == null || len(chanDecl.Args) != 1) {
            return ;
        }
        ptr<ast.CallExpr> chanDeclCopy = addr(new ast.CallExpr());
        chanDeclCopy.val = chanDecl.val;
        chanDeclCopy.Args = append((slice<ast.Expr>)null, chanDecl.Args);
        chanDeclCopy.Args = append(chanDeclCopy.Args, addr(new ast.BasicLit(Kind:token.INT,Value:"1",)));

        ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
        {
            var err = format.Node(_addr_buf, token.NewFileSet(), chanDeclCopy);

            if (err != null) {
                return ;
            }

        }

        pass.Report(new analysis.Diagnostic(Pos:call.Pos(),End:call.End(),Message:"misuse of unbuffered os.Signal channel as argument to signal.Notify",SuggestedFixes:[]analysis.SuggestedFix{{Message:"Change to buffer channel",TextEdits:[]analysis.TextEdit{{Pos:chanDecl.Pos(),End:chanDecl.End(),NewText:buf.Bytes(),}},}},));

    });
    return (null, error.As(null!)!);

}

private static bool isSignalNotify(ptr<types.Info> _addr_info, ptr<ast.CallExpr> _addr_call) {
    ref types.Info info = ref _addr_info.val;
    ref ast.CallExpr call = ref _addr_call.val;

    Func<ptr<ast.Ident>, bool> check = id => {
        var obj = info.ObjectOf(id);
        return obj.Name() == "Notify" && obj.Pkg().Path() == "os/signal";
    };
    switch (call.Fun.type()) {
        case ptr<ast.SelectorExpr> fun:
            return check(fun.Sel);
            break;
        case ptr<ast.Ident> fun:
            {
                var fun__prev1 = fun;

                ptr<ast.SelectorExpr> (fun, ok) = findDecl(_addr_fun)._<ptr<ast.SelectorExpr>>();

                if (ok) {
                    return check(fun.Sel);
                }

                fun = fun__prev1;

            }

            return false;
            break;
        default:
        {
            var fun = call.Fun.type();
            return false;
            break;
        }
    }

}

private static ast.Node findDecl(ptr<ast.Ident> _addr_arg) {
    ref ast.Ident arg = ref _addr_arg.val;

    if (arg.Obj == null) {
        return null;
    }
    switch (arg.Obj.Decl.type()) {
        case ptr<ast.AssignStmt> @as:
            if (len(@as.Lhs) != len(@as.Rhs)) {
                return null;
            }
            {
                var i__prev1 = i;

                foreach (var (__i, __lhs) in @as.Lhs) {
                    i = __i;
                    lhs = __lhs;
                    ptr<ast.Ident> (lid, ok) = lhs._<ptr<ast.Ident>>();
                    if (!ok) {
                        continue;
                    }
                    if (lid.Obj == arg.Obj) {
                        return @as.Rhs[i];
                    }
                }

                i = i__prev1;
            }
            break;
        case ptr<ast.ValueSpec> @as:
            if (len(@as.Names) != len(@as.Values)) {
                return null;
            }
            {
                var i__prev1 = i;

                foreach (var (__i, __name) in @as.Names) {
                    i = __i;
                    name = __name;
                    if (name.Obj == arg.Obj) {
                        return @as.Values[i];
                    }
                }

                i = i__prev1;
            }
            break;
    }
    return null;

}

private static bool isBuiltinMake(ptr<types.Info> _addr_info, ptr<ast.CallExpr> _addr_call) {
    ref types.Info info = ref _addr_info.val;
    ref ast.CallExpr call = ref _addr_call.val;

    var typVal = info.Types[call.Fun];
    if (!typVal.IsBuiltin()) {
        return false;
    }
    switch (call.Fun.type()) {
        case ptr<ast.Ident> fun:
            return info.ObjectOf(fun).Name() == "make";
            break;
        default:
        {
            var fun = call.Fun.type();
            return false;
            break;
        }
    }

}

} // end sigchanyzer_package
