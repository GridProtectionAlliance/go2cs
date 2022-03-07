// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements printing of syntax tree structures.

// package syntax -- go2cs converted at 2022 March 06 23:13:10 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\syntax\dumper.go
using fmt = go.fmt_package;
using io = go.io_package;
using reflect = go.reflect_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class syntax_package {

    // Fdump dumps the structure of the syntax tree rooted at n to w.
    // It is intended for debugging purposes; no specific output format
    // is guaranteed.
public static error Fdump(io.Writer w, Node n) => func((defer, _, _) => {
    error err = default!;

    dumper p = new dumper(output:w,ptrmap:make(map[Node]int),last:'\n',);

    defer(() => {
        {
            var e = recover();

            if (e != null) {
                err = e._<writeError>().err; // re-panics if it's not a writeError
            }
        }

    }());

    if (n == null) {
        p.printf("nil\n");
        return ;
    }
    p.dump(reflect.ValueOf(n), n);
    p.printf("\n");

    return ;

});

private partial struct dumper {
    public io.Writer output;
    public map<Node, nint> ptrmap; // node -> dump line number
    public nint indent; // current indentation level
    public byte last; // last byte processed by Write
    public nint line; // current line number
}

private static slice<byte> indentBytes = (slice<byte>)".  ";

private static (nint, error) Write(this ptr<dumper> _addr_p, slice<byte> data) {
    nint n = default;
    error err = default!;
    ref dumper p = ref _addr_p.val;

    nint m = default;
    foreach (var (i, b) in data) { 
        // invariant: data[0:n] has been written
        if (b == '\n') {
            m, err = p.output.Write(data[(int)n..(int)i + 1]);
            n += m;
            if (err != null) {
                return ;
            }
        }
        else if (p.last == '\n') {
            p.line++;
            _, err = fmt.Fprintf(p.output, "%6d  ", p.line);
            if (err != null) {
                return ;
            }
            for (var j = p.indent; j > 0; j--) {
                _, err = p.output.Write(indentBytes);
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

// writeError wraps locally caught write errors so we can distinguish
// them from genuine panics which we don't want to return as errors.
private partial struct writeError {
    public error err;
}

// printf is a convenience wrapper that takes care of print errors.
private static void printf(this ptr<dumper> _addr_p, @string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();
    ref dumper p = ref _addr_p.val;

    {
        var (_, err) = fmt.Fprintf(p, format, args);

        if (err != null) {
            panic(new writeError(err));
        }
    }

});

// dump prints the contents of x.
// If x is the reflect.Value of a struct s, where &s
// implements Node, then &s should be passed for n -
// this permits printing of the unexported span and
// comments fields of the embedded isNode field by
// calling the Span() and Comment() instead of using
// reflection.
private static void dump(this ptr<dumper> _addr_p, reflect.Value x, Node n) {
    ref dumper p = ref _addr_p.val;


    if (x.Kind() == reflect.Interface) 
        if (x.IsNil()) {
            p.printf("nil");
            return ;
        }
        p.dump(x.Elem(), null);
    else if (x.Kind() == reflect.Ptr) 
        if (x.IsNil()) {
            p.printf("nil");
            return ;
        }
        {
            ptr<Name> x__prev1 = x;

            ptr<Name> (x, ok) = x.Interface()._<ptr<Name>>();

            if (ok) {
                p.printf("%s @ %v", x.Value, x.Pos());
                return ;
            }

            x = x__prev1;

        }


        p.printf("*"); 
        // Fields may share type expressions, and declarations
        // may share the same group - use ptrmap to keep track
        // of nodes that have been printed already.
        {
            Node (ptr, ok) = x.Interface()._<Node>();

            if (ok) {
                {
                    var (line, exists) = p.ptrmap[ptr];

                    if (exists) {
                        p.printf("(Node @ %d)", line);
                        return ;
                    }

                }

                p.ptrmap[ptr] = p.line;
                n = ptr;

            }

        }

        p.dump(x.Elem(), n);
    else if (x.Kind() == reflect.Slice) 
        if (x.IsNil()) {
            p.printf("nil");
            return ;
        }
        p.printf("%s (%d entries) {", x.Type(), x.Len());
        if (x.Len() > 0) {
            p.indent++;
            p.printf("\n");
            {
                nint i__prev1 = i;
                var n__prev1 = n;

                for (nint i = 0;
                var n = x.Len(); i < n; i++) {
                    p.printf("%d: ", i);
                    p.dump(x.Index(i), null);
                    p.printf("\n");
                }


                i = i__prev1;
                n = n__prev1;
            }
            p.indent--;

        }
        p.printf("}");
    else if (x.Kind() == reflect.Struct) 
        var typ = x.Type(); 

        // if span, ok := x.Interface().(lexical.Span); ok {
        //     p.printf("%s", &span)
        //     return
        // }

        p.printf("%s {", typ);
        p.indent++;

        var first = true;
        if (n != null) {
            p.printf("\n");
            first = false; 
            // p.printf("Span: %s\n", n.Span())
            // if c := *n.Comments(); c != nil {
            //     p.printf("Comments: ")
            //     p.dump(reflect.ValueOf(c), nil) // a Comment is not a Node
            //     p.printf("\n")
            // }
        }
        {
            nint i__prev1 = i;
            var n__prev1 = n;

            for (i = 0;
            n = typ.NumField(); i < n; i++) { 
                // Exclude non-exported fields because their
                // values cannot be accessed via reflection.
                {
                    var name = typ.Field(i).Name;

                    if (isExported(name)) {
                        if (first) {
                            p.printf("\n");
                            first = false;
                        }
                        p.printf("%s: ", name);
                        p.dump(x.Field(i), null);
                        p.printf("\n");
                    }

                }

            }


            i = i__prev1;
            n = n__prev1;
        }

        p.indent--;
        p.printf("}");
    else 
        switch (x.Interface().type()) {
            case @string x:
                p.printf("%q", x);
                break;
            default:
            {
                var x = x.Interface().type();
                p.printf("%v", x);
                break;
            }
        }
    
}

private static bool isExported(@string name) {
    var (ch, _) = utf8.DecodeRuneInString(name);
    return unicode.IsUpper(ch);
}

} // end syntax_package
