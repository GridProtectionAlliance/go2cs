// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains printing support for ASTs.

// package ast -- go2cs converted at 2022 March 13 05:54:08 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Program Files\Go\src\go\ast\print.go
namespace go.go;

using fmt = fmt_package;
using token = go.token_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;


// A FieldFilter may be provided to Fprint to control the output.

using System;
public static partial class ast_package {

public delegate  bool FieldFilter(@string,  reflect.Value);

// NotNilFilter returns true for field values that are not nil;
// it returns false otherwise.
public static bool NotNilFilter(@string _, reflect.Value v) {

    if (v.Kind() == reflect.Chan || v.Kind() == reflect.Func || v.Kind() == reflect.Interface || v.Kind() == reflect.Map || v.Kind() == reflect.Ptr || v.Kind() == reflect.Slice) 
        return !v.IsNil();
        return true;
}

// Fprint prints the (sub-)tree starting at AST node x to w.
// If fset != nil, position information is interpreted relative
// to that file set. Otherwise positions are printed as integer
// values (file set specific offsets).
//
// A non-nil FieldFilter f may be provided to control the output:
// struct fields for which f(fieldname, fieldvalue) is true are
// printed; all others are filtered from the output. Unexported
// struct fields are never printed.
public static error Fprint(io.Writer w, ptr<token.FileSet> _addr_fset, object x, FieldFilter f) {
    ref token.FileSet fset = ref _addr_fset.val;

    return error.As(fprint(w, _addr_fset, x, f))!;
}

private static error fprint(io.Writer w, ptr<token.FileSet> _addr_fset, object x, FieldFilter f) => func((defer, _, _) => {
    error err = default!;
    ref token.FileSet fset = ref _addr_fset.val;
 
    // setup printer
    printer p = new printer(output:w,fset:fset,filter:f,ptrmap:make(map[interface{}]int),last:'\n',); 

    // install error handler
    defer(() => {
        {
            var e = recover();

            if (e != null) {
                err = e._<localError>().err; // re-panics if it's not a localError
            }

        }
    }()); 

    // print x
    if (x == null) {
        p.printf("nil\n");
        return ;
    }
    p.print(reflect.ValueOf(x));
    p.printf("\n");

    return ;
});

// Print prints x to standard output, skipping nil fields.
// Print(fset, x) is the same as Fprint(os.Stdout, fset, x, NotNilFilter).
public static error Print(ptr<token.FileSet> _addr_fset, object x) {
    ref token.FileSet fset = ref _addr_fset.val;

    return error.As(Fprint(os.Stdout, _addr_fset, x, NotNilFilter))!;
}

private partial struct printer {
    public io.Writer output;
    public ptr<token.FileSet> fset;
    public FieldFilter filter;
    public nint indent; // current indentation level
    public byte last; // the last byte processed by Write
    public nint line; // current line number
}

private static slice<byte> indent = (slice<byte>)".  ";

private static (nint, error) Write(this ptr<printer> _addr_p, slice<byte> data) {
    nint n = default;
    error err = default!;
    ref printer p = ref _addr_p.val;

    nint m = default;
    foreach (var (i, b) in data) { 
        // invariant: data[0:n] has been written
        if (b == '\n') {
            m, err = p.output.Write(data[(int)n..(int)i + 1]);
            n += m;
            if (err != null) {
                return ;
            }
            p.line++;
        }
        else if (p.last == '\n') {
            _, err = fmt.Fprintf(p.output, "%6d  ", p.line);
            if (err != null) {
                return ;
            }
            for (var j = p.indent; j > 0; j--) {
                _, err = p.output.Write(indent);
                if (err != null) {
                    return ;
                }
            }
        }
        p.last = b;
    }    if (len(data) > n) {
        m, err = p.output.Write(data[(int)n..]);
        n += m;
    }
    return ;
}

// localError wraps locally caught errors so we can distinguish
// them from genuine panics which we don't want to return as errors.
private partial struct localError {
    public error err;
}

// printf is a convenience wrapper that takes care of print errors.
private static void printf(this ptr<printer> _addr_p, @string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();
    ref printer p = ref _addr_p.val;

    {
        var (_, err) = fmt.Fprintf(p, format, args);

        if (err != null) {
            panic(new localError(err));
        }
    }
});

// Implementation note: Print is written for AST nodes but could be
// used to print arbitrary data structures; such a version should
// probably be in a different package.
//
// Note: This code detects (some) cycles created via pointers but
// not cycles that are created via slices or maps containing the
// same slice or map. Code for general data structures probably
// should catch those as well.

private static void print(this ptr<printer> _addr_p, reflect.Value x) {
    ref printer p = ref _addr_p.val;

    if (!NotNilFilter("", x)) {
        p.printf("nil");
        return ;
    }

    if (x.Kind() == reflect.Interface) 
        p.print(x.Elem());
    else if (x.Kind() == reflect.Map) 
        p.printf("%s (len = %d) {", x.Type(), x.Len());
        if (x.Len() > 0) {
            p.indent++;
            p.printf("\n");
            foreach (var (_, key) in x.MapKeys()) {
                p.print(key);
                p.printf(": ");
                p.print(x.MapIndex(key));
                p.printf("\n");
            }
            p.indent--;
        }
        p.printf("}");
    else if (x.Kind() == reflect.Ptr) 
        p.printf("*"); 
        // type-checked ASTs may contain cycles - use ptrmap
        // to keep track of objects that have been printed
        // already and print the respective line number instead
        var ptr = x.Interface();
        {
            var (line, exists) = p.ptrmap[ptr];

            if (exists) {
                p.printf("(obj @ %d)", line);
            }
            else
 {
                p.ptrmap[ptr] = p.line;
                p.print(x.Elem());
            }

        }
    else if (x.Kind() == reflect.Array) 
        p.printf("%s {", x.Type());
        if (x.Len() > 0) {
            p.indent++;
            p.printf("\n");
            {
                nint i__prev1 = i;
                var n__prev1 = n;

                for (nint i = 0;
                var n = x.Len(); i < n; i++) {
                    p.printf("%d: ", i);
                    p.print(x.Index(i));
                    p.printf("\n");
                }


                i = i__prev1;
                n = n__prev1;
            }
            p.indent--;
        }
        p.printf("}");
    else if (x.Kind() == reflect.Slice) 
        {
            slice<byte> (s, ok) = x.Interface()._<slice<byte>>();

            if (ok) {
                p.printf("%#q", s);
                return ;
            }

        }
        p.printf("%s (len = %d) {", x.Type(), x.Len());
        if (x.Len() > 0) {
            p.indent++;
            p.printf("\n");
            {
                nint i__prev1 = i;
                var n__prev1 = n;

                for (i = 0;
                n = x.Len(); i < n; i++) {
                    p.printf("%d: ", i);
                    p.print(x.Index(i));
                    p.printf("\n");
                }


                i = i__prev1;
                n = n__prev1;
            }
            p.indent--;
        }
        p.printf("}");
    else if (x.Kind() == reflect.Struct) 
        var t = x.Type();
        p.printf("%s {", t);
        p.indent++;
        var first = true;
        {
            nint i__prev1 = i;
            var n__prev1 = n;

            for (i = 0;
            n = t.NumField(); i < n; i++) { 
                // exclude non-exported fields because their
                // values cannot be accessed via reflection
                {
                    var name = t.Field(i).Name;

                    if (IsExported(name)) {
                        var value = x.Field(i);
                        if (p.filter == null || p.filter(name, value)) {
                            if (first) {
                                p.printf("\n");
                                first = false;
                            }
                            p.printf("%s: ", name);
                            p.print(value);
                            p.printf("\n");
                        }
                    }

                }
            }


            i = i__prev1;
            n = n__prev1;
        }
        p.indent--;
        p.printf("}");
    else 
        var v = x.Interface();
        switch (v.type()) {
            case @string v:
                p.printf("%q", v);
                return ;
                break;
            case token.Pos v:
                if (p.fset != null) {
                    p.printf("%s", p.fset.Position(v));
                    return ;
                }
                break; 
            // default
        } 
        // default
        p.printf("%v", v);
    }

} // end ast_package
