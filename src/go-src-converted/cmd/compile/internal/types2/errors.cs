// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements various error reporters.

// package types2 -- go2cs converted at 2022 March 06 23:12:31 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\errors.go
using bytes = go.bytes_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

namespace go.cmd.compile.@internal;

public static partial class types2_package {

private static void unimplemented() => func((_, panic, _) => {
    panic("unimplemented");
});

private static void assert(bool p) => func((_, panic, _) => {
    if (!p) {
        panic("assertion failed");
    }
});

private static void unreachable() => func((_, panic, _) => {
    panic("unreachable");
});

// An error_ represents a type-checking error.
// To report an error_, call Checker.report.
private partial struct error_ {
    public slice<errorDesc> desc;
    public bool soft; // TODO(gri) eventually determine this from an error code
}

// An errorDesc describes part of a type-checking error.
private partial struct errorDesc {
    public syntax.Pos pos;
    public @string format;
    public slice<object> args;
}

private static bool empty(this ptr<error_> _addr_err) {
    ref error_ err = ref _addr_err.val;

    return err.desc == null;
}

private static syntax.Pos pos(this ptr<error_> _addr_err) {
    ref error_ err = ref _addr_err.val;

    if (err.empty()) {
        return nopos;
    }
    return err.desc[0].pos;

}

private static @string msg(this ptr<error_> _addr_err, Qualifier qf) {
    ref error_ err = ref _addr_err.val;

    if (err.empty()) {
        return "no error";
    }
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    foreach (var (i) in err.desc) {
        var p = _addr_err.desc[i];
        if (i > 0) {
            fmt.Fprintf(_addr_buf, "\n\t%s: ", p.pos);
        }
        buf.WriteString(sprintf(qf, p.format, p.args));

    }    return buf.String();

}

// String is for testing.
private static @string String(this ptr<error_> _addr_err) {
    ref error_ err = ref _addr_err.val;

    if (err.empty()) {
        return "no error";
    }
    return fmt.Sprintf("%s: %s", err.pos(), err.msg(null));

}

// errorf adds formatted error information to err.
// It may be called multiple times to provide additional information.
private static void errorf(this ptr<error_> _addr_err, poser at, @string format, params object[] args) {
    args = args.Clone();
    ref error_ err = ref _addr_err.val;

    err.desc = append(err.desc, new errorDesc(posFor(at),format,args));
}

private static @string sprintf(Qualifier qf, @string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();

    foreach (var (i, arg) in args) {
        switch (arg.type()) {
            case 
                arg = "<nil>";
                break;
            case operand a:
                panic("internal error: should always pass *operand");
                break;
            case ptr<operand> a:
                arg = operandString(a, qf);
                break;
            case syntax.Pos a:
                arg = a.String();
                break;
            case syntax.Expr a:
                arg = syntax.String(a);
                break;
            case Object a:
                arg = ObjectString(a, qf);
                break;
            case Type a:
                arg = TypeString(a, qf);
                break;
        }
        args[i] = arg;

    }    return fmt.Sprintf(format, args);

});

private static @string qualifier(this ptr<Checker> _addr_check, ptr<Package> _addr_pkg) {
    ref Checker check = ref _addr_check.val;
    ref Package pkg = ref _addr_pkg.val;
 
    // Qualify the package unless it's the package being type-checked.
    if (pkg != check.pkg) {
        if (check.pkgPathMap == null) {
            check.pkgPathMap = make_map<@string, map<@string, bool>>();
            check.seenPkgMap = make_map<ptr<Package>, bool>();
            check.markImports(pkg);
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

private static @string sprintf(this ptr<Checker> _addr_check, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    return sprintf(check.qualifier, format, args);
}

private static void report(this ptr<Checker> _addr_check, ptr<error_> _addr_err) => func((_, panic, _) => {
    ref Checker check = ref _addr_check.val;
    ref error_ err = ref _addr_err.val;

    if (err.empty()) {
        panic("internal error: reporting no error");
    }
    check.err(err.pos(), err.msg(check.qualifier), err.soft);

});

private static void trace(this ptr<Checker> _addr_check, syntax.Pos pos, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    fmt.Printf("%s:\t%s%s\n", pos, strings.Repeat(".  ", check.indent), check.sprintf(format, args));
}

// dump is only needed for debugging
private static void dump(this ptr<Checker> _addr_check, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    fmt.Println(check.sprintf(format, args));
}

private static void err(this ptr<Checker> _addr_check, poser at, @string msg, bool soft) => func((_, panic, _) => {
    ref Checker check = ref _addr_check.val;
 
    // Cheap trick: Don't report errors with messages containing
    // "invalid operand" or "invalid type" as those tend to be
    // follow-on errors which don't add useful information. Only
    // exclude them if these strings are not at the beginning,
    // and only if we have at least one error already reported.
    if (check.firstErr != null && (strings.Index(msg, "invalid operand") > 0 || strings.Index(msg, "invalid type") > 0)) {
        return ;
    }
    var pos = posFor(at); 

    // If we are encountering an error while evaluating an inherited
    // constant initialization expression, pos is the position of in
    // the original expression, and not of the currently declared
    // constant identifier. Use the provided errpos instead.
    // TODO(gri) We may also want to augment the error message and
    // refer to the position (pos) in the original expression.
    if (check.errpos.IsKnown()) {
        assert(check.iota != null);
        pos = check.errpos;
    }
    Error err = new Error(pos,stripAnnotations(msg),msg,soft);
    if (check.firstErr == null) {
        check.firstErr = err;
    }
    if (check.conf.Trace) {
        check.trace(pos, "ERROR: %s", msg);
    }
    var f = check.conf.Error;
    if (f == null) {
        panic(new bailout()); // report only first error
    }
    f(err);

});

private static readonly @string invalidAST = "invalid AST: ";
private static readonly @string invalidArg = "invalid argument: ";
private static readonly @string invalidOp = "invalid operation: ";


private partial interface poser {
    syntax.Pos Pos();
}

private static void error(this ptr<Checker> _addr_check, poser at, @string msg) {
    ref Checker check = ref _addr_check.val;

    check.err(at, msg, false);
}

private static void errorf(this ptr<Checker> _addr_check, poser at, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    check.err(at, check.sprintf(format, args), false);
}

private static void softErrorf(this ptr<Checker> _addr_check, poser at, @string format, params object[] args) {
    args = args.Clone();
    ref Checker check = ref _addr_check.val;

    check.err(at, check.sprintf(format, args), true);
}

// posFor reports the left (= start) position of at.
private static syntax.Pos posFor(poser at) {
    switch (at.type()) {
        case ptr<operand> x:
            if (x.expr != null) {
                return syntax.StartPos(x.expr);
            }
            break;
        case syntax.Node x:
            return syntax.StartPos(x);
            break;
    }
    return at.Pos();

}

// stripAnnotations removes internal (type) annotations from s.
private static @string stripAnnotations(@string s) { 
    // Would like to use strings.Builder but it's not available in Go 1.4.
    bytes.Buffer b = default;
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

} // end types2_package
