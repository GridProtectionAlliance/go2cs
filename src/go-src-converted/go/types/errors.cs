// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements various error reporters.

// package types -- go2cs converted at 2022 March 13 05:52:57 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\errors.go
namespace go.go;

using errors = errors_package;
using fmt = fmt_package;
using ast = go.ast_package;
using token = go.token_package;
using strconv = strconv_package;
using strings = strings_package;

public static partial class types_package {

private static void assert(bool p) => func((_, panic, _) => {
    if (!p) {
        panic("assertion failed");
    }
});

private static void unreachable() => func((_, panic, _) => {
    panic("unreachable");
});

private static @string qualifier(this ptr<Checker> _addr_check, ptr<Package> _addr_pkg) {
    ref Checker check = ref _addr_check.val;
    ref Package pkg = ref _addr_pkg.val;
 
    // Qualify the package unless it's the package being type-checked.
    if (pkg != check.pkg) {
        if (check.pkgPathMap == null) {
            check.pkgPathMap = make_map<@string, map<@string, bool>>();
            check.seenPkgMap = make_map<ptr<Package>, bool>();
            check.markImports(check.pkg);
        }
        if (len(check.pkgPathMap[pkg.name]) > 1) {
            return strconv.Quote(pkg.path);
        }
        return pkg.name;
    }
    return "";
}

// markImports recursively walks pkg and its imports, to record unique import
// paths in pkgPathMap.
private static void markImports(this ptr<Checker> _addr_check, ptr<Package> _addr_pkg) {
    ref Checker check = ref _addr_check.val;
    ref Package pkg = ref _addr_pkg.val;

    if (check.seenPkgMap[pkg]) {
        return ;
    }
    check.seenPkgMap[pkg] = true;

    var (forName, ok) = check.pkgPathMap[pkg.name];
    if (!ok) {
        forName = make_map<@string, bool>();
        check.pkgPathMap[pkg.name] = forName;
    }
    forName[pkg.path] = true;

    foreach (var (_, imp) in pkg.imports) {
        check.markImports(imp);
    }
}

private static @string sprintf(this ptr<Checker> _addr_check, @string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    foreach (var (i, arg) in args) {
        switch (arg.type()) {
            case 
                arg = "<nil>";
                break;
            case operand a:
                panic("internal error: should always pass *operand");
                break;
            case ptr<operand> a:
                arg = operandString(a, check.qualifier);
                break;
            case token.Pos a:
                arg = check.fset.Position(a).String();
                break;
            case ast.Expr a:
                arg = ExprString(a);
                break;
            case Object a:
                arg = ObjectString(a, check.qualifier);
                break;
            case Type a:
                arg = TypeString(a, check.qualifier);
                break;
        }
        args[i] = arg;
    }    return fmt.Sprintf(format, args);
});

private static void trace(this ptr<Checker> _addr_check, token.Pos pos, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    fmt.Printf("%s:\t%s%s\n", check.fset.Position(pos), strings.Repeat(".  ", check.indent), check.sprintf(format, args));
}

// dump is only needed for debugging
private static void dump(this ptr<Checker> _addr_check, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    fmt.Println(check.sprintf(format, args));
}

private static void err(this ptr<Checker> _addr_check, error err) => func((_, panic, _) => {
    ref Checker check = ref _addr_check.val;

    if (err == null) {
        return ;
    }
    ref Error e = ref heap(out ptr<Error> _addr_e);
    var isInternal = errors.As(err, _addr_e); 
    // Cheap trick: Don't report errors with messages containing
    // "invalid operand" or "invalid type" as those tend to be
    // follow-on errors which don't add useful information. Only
    // exclude them if these strings are not at the beginning,
    // and only if we have at least one error already reported.
    var isInvalidErr = isInternal && (strings.Index(e.Msg, "invalid operand") > 0 || strings.Index(e.Msg, "invalid type") > 0);
    if (check.firstErr != null && isInvalidErr) {
        return ;
    }
    if (isInternal) {
        e.Msg = stripAnnotations(e.Msg);
        if (check.errpos != null) { 
            // If we have an internal error and the errpos override is set, use it to
            // augment our error positioning.
            // TODO(rFindley) we may also want to augment the error message and refer
            // to the position (pos) in the original expression.
            var span = spanOf(check.errpos);
            e.Pos = span.pos;
            e.go116start = span.start;
            e.go116end = span.end;
        }
        err = e;
    }
    if (check.firstErr == null) {
        check.firstErr = err;
    }
    if (trace) {
        var pos = e.Pos;
        var msg = e.Msg;
        if (!isInternal) {
            msg = err.Error();
            pos = token.NoPos;
        }
        check.trace(pos, "ERROR: %s", msg);
    }
    var f = check.conf.Error;
    if (f == null) {
        panic(new bailout()); // report only first error
    }
    f(err);
});

private static error newError(this ptr<Checker> _addr_check, positioner at, errorCode code, bool soft, @string msg) {
    ref Checker check = ref _addr_check.val;

    var span = spanOf(at);
    return error.As(new Error(Fset:check.fset,Pos:span.pos,Msg:msg,Soft:soft,go116code:code,go116start:span.start,go116end:span.end,))!;
}

// newErrorf creates a new Error, but does not handle it.
private static error newErrorf(this ptr<Checker> _addr_check, positioner at, errorCode code, bool soft, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    var msg = check.sprintf(format, args);
    return error.As(check.newError(at, code, soft, msg))!;
}

private static void error(this ptr<Checker> _addr_check, positioner at, errorCode code, @string msg) {
    ref Checker check = ref _addr_check.val;

    check.err(check.newError(at, code, false, msg));
}

private static void errorf(this ptr<Checker> _addr_check, positioner at, errorCode code, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    check.error(at, code, check.sprintf(format, args));
}

private static void softErrorf(this ptr<Checker> _addr_check, positioner at, errorCode code, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    check.err(check.newErrorf(at, code, true, format, args));
}

private static void invalidAST(this ptr<Checker> _addr_check, positioner at, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    check.errorf(at, 0, "invalid AST: " + format, args);
}

private static void invalidArg(this ptr<Checker> _addr_check, positioner at, errorCode code, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    check.errorf(at, code, "invalid argument: " + format, args);
}

private static void invalidOp(this ptr<Checker> _addr_check, positioner at, errorCode code, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    check.errorf(at, code, "invalid operation: " + format, args);
}

// The positioner interface is used to extract the position of type-checker
// errors.
private partial interface positioner {
    token.Pos Pos();
}

// posSpan holds a position range along with a highlighted position within that
// range. This is used for positioning errors, with pos by convention being the
// first position in the source where the error is known to exist, and start
// and end defining the full span of syntax being considered when the error was
// detected. Invariant: start <= pos < end || start == pos == end.
private partial struct posSpan {
    public token.Pos start;
    public token.Pos pos;
    public token.Pos end;
}

private static token.Pos Pos(this posSpan e) {
    return e.pos;
}

// inNode creates a posSpan for the given node.
// Invariant: node.Pos() <= pos < node.End() (node.End() is the position of the
// first byte after node within the source).
private static posSpan inNode(ast.Node node, token.Pos pos) {
    var start = node.Pos();
    var end = node.End();
    if (debug) {
        assert(start <= pos && pos < end);
    }
    return new posSpan(start,pos,end);
}

// atPos wraps a token.Pos to implement the positioner interface.
private partial struct atPos { // : token.Pos
}

private static token.Pos Pos(this atPos s) {
    return token.Pos(s);
}

// spanOf extracts an error span from the given positioner. By default this is
// the trivial span starting and ending at pos, but this span is expanded when
// the argument naturally corresponds to a span of source code.
private static posSpan spanOf(positioner at) => func((_, panic, _) => {
    switch (at.type()) {
        case 
            panic("internal error: nil");
            break;
        case posSpan x:
            return x;
            break;
        case ast.Node x:
            var pos = x.Pos();
            return new posSpan(pos,pos,x.End());
            break;
        case ptr<operand> x:
            if (x.expr != null) {
                pos = x.Pos();
                return new posSpan(pos,pos,x.expr.End());
            }
            return new posSpan(token.NoPos,token.NoPos,token.NoPos);
            break;
        default:
        {
            var x = at.type();
            pos = at.Pos();
            return new posSpan(pos,pos,pos);
            break;
        }
    }
});

// stripAnnotations removes internal (type) annotations from s.
private static @string stripAnnotations(@string s) {
    strings.Builder b = default;
    foreach (var (_, r) in s) { 
        // strip #'s and subscript digits
        if (r != instanceMarker && !('₀' <= r && r < '₀' + 10)) { // '₀' == U+2080
            b.WriteRune(r);
        }
    }    if (b.Len() < len(s)) {
        return b.String();
    }
    return s;
}

} // end types_package
