// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package stdmethods defines an Analyzer that checks for misspellings
// in the signatures of methods similar to well-known interfaces.
// package stdmethods -- go2cs converted at 2022 March 06 23:34:46 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/stdmethods" ==> using stdmethods = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.stdmethods_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\stdmethods\stdmethods.go
using ast = go.go.ast_package;
using types = go.go.types_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

public static partial class stdmethods_package {

public static readonly @string Doc = @"check signature of methods of well-known interfaces

Sometimes a type may be intended to satisfy an interface but may fail to
do so because of a mistake in its method signature.
For example, the result of this WriteTo method should be (int64, error),
not error, to satisfy io.WriterTo:

	type myWriterTo struct{...}
        func (myWriterTo) WriteTo(w io.Writer) error { ... }

This check ensures that each method whose name matches one of several
well-known interface methods from the standard library has the correct
signature for that interface.

Checked method names include:
	Format GobEncode GobDecode MarshalJSON MarshalXML
	Peek ReadByte ReadFrom ReadRune Scan Seek
	UnmarshalJSON UnreadByte UnreadRune WriteByte
	WriteTo
";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"stdmethods",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

// canonicalMethods lists the input and output types for Go methods
// that are checked using dynamic interface checks. Because the
// checks are dynamic, such methods would not cause a compile error
// if they have the wrong signature: instead the dynamic check would
// fail, sometimes mysteriously. If a method is found with a name listed
// here but not the input/output types listed here, vet complains.
//
// A few of the canonical methods have very common names.
// For example, a type might implement a Scan method that
// has nothing to do with fmt.Scanner, but we still want to check
// the methods that are intended to implement fmt.Scanner.
// To do that, the arguments that have a = prefix are treated as
// signals that the canonical meaning is intended: if a Scan
// method doesn't have a fmt.ScanState as its first argument,
// we let it go. But if it does have a fmt.ScanState, then the
// rest has to match.


private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.FuncDecl)(nil), (*ast.InterfaceType)(nil) });
    inspect.Preorder(nodeFilter, n => {
        switch (n.type()) {
            case ptr<ast.FuncDecl> n:
                if (n.Recv != null) {
                    canonicalMethod(_addr_pass, _addr_n.Name);
                }
                break;
            case ptr<ast.InterfaceType> n:
                foreach (var (_, field) in n.Methods.List) {
                    foreach (var (_, id) in field.Names) {
                        canonicalMethod(_addr_pass, _addr_id);
                    }
                }
                break;
        }

    });
    return (null, error.As(null!)!);

}

private static void canonicalMethod(ptr<analysis.Pass> _addr_pass, ptr<ast.Ident> _addr_id) {
    ref analysis.Pass pass = ref _addr_pass.val;
    ref ast.Ident id = ref _addr_id.val;
 
    // Expected input/output.
    var (expect, ok) = canonicalMethods[id.Name];
    if (!ok) {
        return ;
    }
    ptr<types.Signature> sign = pass.TypesInfo.Defs[id].Type()._<ptr<types.Signature>>();
    var args = sign.Params();
    var results = sign.Results(); 

    // Special case: WriteTo with more than one argument,
    // not trying at all to implement io.WriterTo,
    // comes up often enough to skip.
    if (id.Name == "WriteTo" && args.Len() > 1) {
        return ;
    }
    if (id.Name == "Is" || id.Name == "As" || id.Name == "Unwrap") {
        {
            var recv = sign.Recv();

            if (recv == null || !implementsError(recv.Type())) {
                return ;
            }

        }

    }
    if (!matchParams(_addr_pass, expect.args, _addr_args, "=") || !matchParams(_addr_pass, expect.results, _addr_results, "=")) {
        return ;
    }
    if (!matchParams(_addr_pass, expect.args, _addr_args, "") || !matchParams(_addr_pass, expect.results, _addr_results, "")) {
        var expectFmt = id.Name + "(" + argjoin(expect.args) + ")";
        if (len(expect.results) == 1) {
            expectFmt += " " + argjoin(expect.results);
        }
        else if (len(expect.results) > 1) {
            expectFmt += " (" + argjoin(expect.results) + ")";
        }
        var actual = typeString(sign);
        actual = strings.TrimPrefix(actual, "func");
        actual = id.Name + actual;

        pass.ReportRangef(id, "method %s should have signature %s", actual, expectFmt);

    }
}

private static @string typeString(types.Type typ) {
    return types.TypeString(typ, (types.Package.val).Name);
}

private static @string argjoin(slice<@string> x) {
    var y = make_slice<@string>(len(x));
    foreach (var (i, s) in x) {
        if (s[0] == '=') {
            s = s[(int)1..];
        }
        y[i] = s;

    }    return strings.Join(y, ", ");

}

// Does each type in expect with the given prefix match the corresponding type in actual?
private static bool matchParams(ptr<analysis.Pass> _addr_pass, slice<@string> expect, ptr<types.Tuple> _addr_actual, @string prefix) {
    ref analysis.Pass pass = ref _addr_pass.val;
    ref types.Tuple actual = ref _addr_actual.val;

    foreach (var (i, x) in expect) {
        if (!strings.HasPrefix(x, prefix)) {
            continue;
        }
        if (i >= actual.Len()) {
            return false;
        }
        if (!matchParamType(x, actual.At(i).Type())) {
            return false;
        }
    }    if (prefix == "" && actual.Len() > len(expect)) {
        return false;
    }
    return true;

}

// Does this one type match?
private static bool matchParamType(@string expect, types.Type actual) {
    expect = strings.TrimPrefix(expect, "="); 
    // Overkill but easy.
    return typeString(actual) == expect;

}

private static ptr<types.Interface> errorType = types.Universe.Lookup("error").Type().Underlying()._<ptr<types.Interface>>();

private static bool implementsError(types.Type actual) {
    return types.Implements(actual, errorType);
}

} // end stdmethods_package
