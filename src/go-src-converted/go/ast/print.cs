// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains printing support for ASTs.

// package ast -- go2cs converted at 2020 August 29 08:48:34 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Go\src\go\ast\print.go
using fmt = go.fmt_package;
using token = go.go.token_package;
using io = go.io_package;
using os = go.os_package;
using reflect = go.reflect_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class ast_package
    {
        // A FieldFilter may be provided to Fprint to control the output.
        public delegate  bool FieldFilter(@string,  reflect.Value);

        // NotNilFilter returns true for field values that are not nil;
        // it returns false otherwise.
        public static bool NotNilFilter(@string _, reflect.Value v)
        {

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
        public static error Fprint(io.Writer w, ref token.FileSet fset, object x, FieldFilter f)
        {
            return error.As(fprint(w, fset, x, f));
        }

        private static error fprint(io.Writer w, ref token.FileSet _fset, object x, FieldFilter f) => func(_fset, (ref token.FileSet fset, Defer defer, Panic _, Recover __) =>
        { 
            // setup printer
            printer p = new printer(output:w,fset:fset,filter:f,ptrmap:make(map[interface{}]int),last:'\n',); 

            // install error handler
            defer(() =>
            {
                {
                    var e = recover();

                    if (e != null)
                    {
                        err = e._<localError>().err; // re-panics if it's not a localError
                    }

                }
            }()); 

            // print x
            if (x == null)
            {
                p.printf("nil\n");
                return;
            }
            p.print(reflect.ValueOf(x));
            p.printf("\n");

            return;
        });

        // Print prints x to standard output, skipping nil fields.
        // Print(fset, x) is the same as Fprint(os.Stdout, fset, x, NotNilFilter).
        public static error Print(ref token.FileSet fset, object x)
        {
            return error.As(Fprint(os.Stdout, fset, x, NotNilFilter));
        }

        private partial struct printer
        {
            public io.Writer output;
            public ptr<token.FileSet> fset;
            public FieldFilter filter;
            public long indent; // current indentation level
            public byte last; // the last byte processed by Write
            public long line; // current line number
        }

        private static slice<byte> indent = (slice<byte>)".  ";

        private static (long, error) Write(this ref printer p, slice<byte> data)
        {
            long m = default;
            foreach (var (i, b) in data)
            { 
                // invariant: data[0:n] has been written
                if (b == '\n')
                {
                    m, err = p.output.Write(data[n..i + 1L]);
                    n += m;
                    if (err != null)
                    {
                        return;
                    }
                    p.line++;
                }
                else if (p.last == '\n')
                {
                    _, err = fmt.Fprintf(p.output, "%6d  ", p.line);
                    if (err != null)
                    {
                        return;
                    }
                    for (var j = p.indent; j > 0L; j--)
                    {
                        _, err = p.output.Write(indent);
                        if (err != null)
                        {
                            return;
                        }
                    }

                }
                p.last = b;
            }
            if (len(data) > n)
            {
                m, err = p.output.Write(data[n..]);
                n += m;
            }
            return;
        }

        // localError wraps locally caught errors so we can distinguish
        // them from genuine panics which we don't want to return as errors.
        private partial struct localError
        {
            public error err;
        }

        // printf is a convenience wrapper that takes care of print errors.
        private static void printf(this ref printer _p, @string format, params object[] args) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            {
                var (_, err) = fmt.Fprintf(p, format, args);

                if (err != null)
                {
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

        private static void print(this ref printer p, reflect.Value x)
        {
            if (!NotNilFilter("", x))
            {
                p.printf("nil");
                return;
            }

            if (x.Kind() == reflect.Interface) 
                p.print(x.Elem());
            else if (x.Kind() == reflect.Map) 
                p.printf("%s (len = %d) {", x.Type(), x.Len());
                if (x.Len() > 0L)
                {
                    p.indent++;
                    p.printf("\n");
                    foreach (var (_, key) in x.MapKeys())
                    {
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

                    if (exists)
                    {
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
                if (x.Len() > 0L)
                {
                    p.indent++;
                    p.printf("\n");
                    {
                        long i__prev1 = i;
                        var n__prev1 = n;

                        for (long i = 0L;
                        var n = x.Len(); i < n; i++)
                        {
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

                    if (ok)
                    {
                        p.printf("%#q", s);
                        return;
                    }

                }
                p.printf("%s (len = %d) {", x.Type(), x.Len());
                if (x.Len() > 0L)
                {
                    p.indent++;
                    p.printf("\n");
                    {
                        long i__prev1 = i;
                        var n__prev1 = n;

                        for (i = 0L;
                        n = x.Len(); i < n; i++)
                        {
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
                    long i__prev1 = i;
                    var n__prev1 = n;

                    for (i = 0L;
                    n = t.NumField(); i < n; i++)
                    { 
                        // exclude non-exported fields because their
                        // values cannot be accessed via reflection
                        {
                            var name = t.Field(i).Name;

                            if (IsExported(name))
                            {
                                var value = x.Field(i);
                                if (p.filter == null || p.filter(name, value))
                                {
                                    if (first)
                                    {
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
                switch (v.type())
                {
                    case @string v:
                        p.printf("%q", v);
                        return;
                        break;
                    case token.Pos v:
                        if (p.fset != null)
                        {
                            p.printf("%s", p.fset.Position(v));
                            return;
                        }
                        break; 
                    // default
                } 
                // default
                p.printf("%v", v);
                    }
    }
}}
