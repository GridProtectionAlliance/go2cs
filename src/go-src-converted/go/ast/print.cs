// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains printing support for ASTs.
namespace go.go;

using fmt = fmt_package;
using token = go.token_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using ꓸꓸꓸany = Span<any>;

partial class ast_package {

public delegate bool FieldFilter(@string name, reflectꓸValue value);

// NotNilFilter is a [FieldFilter] that returns true for field values
// that are not nil; it returns false otherwise.
public static bool NotNilFilter(@string _, reflectꓸValue v) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func || exprᴛ1 == reflect.ΔInterface || exprᴛ1 == reflect.Map || exprᴛ1 == reflect.ΔPointer || exprᴛ1 == reflect.ΔSlice) {
        return !v.IsNil();
    }

    return true;
}

// Fprint prints the (sub-)tree starting at AST node x to w.
// If fset != nil, position information is interpreted relative
// to that file set. Otherwise positions are printed as integer
// values (file set specific offsets).
//
// A non-nil [FieldFilter] f may be provided to control the output:
// struct fields for which f(fieldname, fieldvalue) is true are
// printed; all others are filtered from the output. Unexported
// struct fields are never printed.
public static error Fprint(io.Writer w, ж<token.FileSet> Ꮡfset, any x, FieldFilter f) {
    ref var fset = ref Ꮡfset.val;

    return fprint(w, Ꮡfset, x, f);
}

internal static error /*err*/ fprint(io.Writer w, ж<token.FileSet> Ꮡfset, any x, FieldFilter f) => func((defer, recover) => {
    error err = default!;

    ref var fset = ref Ꮡfset.val;
    // setup printer
    var p = new printer(
        output: w,
        fset: fset,
        filter: f,
        ptrmap: new map<any, nint>(),
        last: (rune)'\n'
    );
    // force printing of line number on first line
    // install error handler
    defer(() => {
        {
            var e = recover(); if (e != default!) {
                err = e._<localError>().err;
            }
        }
    });
    // re-panics if it's not a localError
    // print x
    if (x == default!) {
        p.printf("nil\n"u8);
        return err;
    }
    p.print(reflect.ValueOf(x));
    p.printf("\n"u8);
    return err;
});

// Print prints x to standard output, skipping nil fields.
// Print(fset, x) is the same as Fprint(os.Stdout, fset, x, NotNilFilter).
public static error Print(ж<token.FileSet> Ꮡfset, any x) {
    ref var fset = ref Ꮡfset.val;

    return Fprint(~os.Stdout, Ꮡfset, x, NotNilFilter);
}

[GoType] partial struct printer {
    internal io_package.Writer output;
    internal ж<go.token_package.FileSet> fset;
    internal FieldFilter filter;
    internal map<any, nint> ptrmap; // *T -> line number
    internal nint indent;        // current indentation level
    internal byte last;        // the last byte processed by Write
    internal nint line;        // current line number
}

internal static slice<byte> indent = slice<byte>(".  ");

[GoRecv] internal static (nint n, error err) Write(this ref printer p, slice<byte> data) {
    nint n = default!;
    error err = default!;

    nint m = default!;
    foreach (var (i, b) in data) {
        // invariant: data[0:n] has been written
        if (b == (rune)'\n'){
            (m, err) = p.output.Write(data[(int)(n)..(int)(i + 1)]);
            n += m;
            if (err != default!) {
                return (n, err);
            }
            p.line++;
        } else 
        if (p.last == (rune)'\n') {
            (_, err) = fmt.Fprintf(p.output, "%6d  "u8, p.line);
            if (err != default!) {
                return (n, err);
            }
            for (nint j = p.indent; j > 0; j--) {
                (_, err) = p.output.Write(indent);
                if (err != default!) {
                    return (n, err);
                }
            }
        }
        p.last = b;
    }
    if (len(data) > n) {
        (m, err) = p.output.Write(data[(int)(n)..]);
        n += m;
    }
    return (n, err);
}

// localError wraps locally caught errors so we can distinguish
// them from genuine panics which we don't want to return as errors.
[GoType] partial struct localError {
    internal error err;
}

// printf is a convenience wrapper that takes care of print errors.
[GoRecv] internal static void printf(this ref printer p, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    {
        var (_, err) = fmt.Fprintf(~p, format, args.ꓸꓸꓸ); if (err != default!) {
            throw panic(new localError(err));
        }
    }
}

// Implementation note: Print is written for AST nodes but could be
// used to print arbitrary data structures; such a version should
// probably be in a different package.
//
// Note: This code detects (some) cycles created via pointers but
// not cycles that are created via slices or maps containing the
// same slice or map. Code for general data structures probably
// should catch those as well.
[GoRecv] internal static void print(this ref printer p, reflectꓸValue x) {
    if (!NotNilFilter(""u8, x)) {
        p.printf("nil"u8);
        return;
    }
    var exprᴛ1 = x.Kind();
    if (exprᴛ1 == reflect.ΔInterface) {
        p.print(x.Elem());
    }
    else if (exprᴛ1 == reflect.Map) {
        p.printf("%s (len = %d) {"u8, x.Type(), x.Len());
        if (x.Len() > 0) {
            p.indent++;
            p.printf("\n"u8);
            foreach (var (_, key) in x.MapKeys()) {
                p.print(key);
                p.printf(": "u8);
                p.print(x.MapIndex(key));
                p.printf("\n"u8);
            }
            p.indent--;
        }
        p.printf("}"u8);
    }
    else if (exprᴛ1 == reflect.ΔPointer) {
        p.printf("*"u8);
        var ptr = x.Interface();
        {
            nint line = p.ptrmap[ptr];
            var exists = p.ptrmap[ptr]; if (exists){
                // type-checked ASTs may contain cycles - use ptrmap
                // to keep track of objects that have been printed
                // already and print the respective line number instead
                p.printf("(obj @ %d)"u8, line);
            } else {
                p.ptrmap[ptr] = p.line;
                p.print(x.Elem());
            }
        }
    }
    else if (exprᴛ1 == reflect.Array) {
        p.printf("%s {"u8, x.Type());
        if (x.Len() > 0) {
            p.indent++;
            p.printf("\n"u8);
            for (nint i = 0;nint n = x.Len(); i < n; i++) {
                p.printf("%d: "u8, i);
                p.print(x.Index(i));
                p.printf("\n"u8);
            }
            p.indent--;
        }
        p.printf("}"u8);
    }
    else if (exprᴛ1 == reflect.ΔSlice) {
        {
            var (s, ok) = x.Interface()._<slice<byte>>(ᐧ); if (ok) {
                p.printf("%#q"u8, s);
                return;
            }
        }
        p.printf("%s (len = %d) {"u8, x.Type(), x.Len());
        if (x.Len() > 0) {
            p.indent++;
            p.printf("\n"u8);
            for (nint i = 0;nint n = x.Len(); i < n; i++) {
                p.printf("%d: "u8, i);
                p.print(x.Index(i));
                p.printf("\n"u8);
            }
            p.indent--;
        }
        p.printf("}"u8);
    }
    else if (exprᴛ1 == reflect.Struct) {
        var t = x.Type();
        p.printf("%s {"u8, t);
        p.indent++;
        var first = true;
        for (nint i = 0;nint n = t.NumField(); i < n; i++) {
            // exclude non-exported fields because their
            // values cannot be accessed via reflection
            {
                @string name = t.Field(i).Name; if (IsExported(name)) {
                    var value = x.Field(i);
                    if (p.filter == default! || p.filter(name, value)) {
                        if (first) {
                            p.printf("\n"u8);
                            first = false;
                        }
                        p.printf("%s: "u8, name);
                        p.print(value);
                        p.printf("\n"u8);
                    }
                }
            }
        }
        p.indent--;
        p.printf("}"u8);
    }
    else { /* default: */
        var v = x.Interface();
        switch (v.type()) {
        case @string v: {
            p.printf("%q"u8, // print strings in quotes
 v);
            return;
        }
        case tokenꓸPos v: {
            if (p.fset != nil) {
                // position values can be printed nicely if we have a file set
                p.printf("%s"u8, p.fset.Position(v));
                return;
            }
            break;
        }}
        p.printf("%v"u8, // default
 v);
    }

}

} // end ast_package
