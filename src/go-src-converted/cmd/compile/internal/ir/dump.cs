// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements textual dumping of arbitrary data structures
// for debugging purposes. The code is customized for Node graphs
// and may be used for an alternative view of the node structure.

// package ir -- go2cs converted at 2022 March 13 06:00:19 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\dump.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using regexp = regexp_package;

using @base = cmd.compile.@internal.@base_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;


// DumpAny is like FDumpAny but prints to stderr.

public static partial class ir_package {

public static void DumpAny(object root, @string filter, nint depth) {
    FDumpAny(os.Stderr, root, filter, depth);
}

// FDumpAny prints the structure of a rooted data structure
// to w by depth-first traversal of the data structure.
//
// The filter parameter is a regular expression. If it is
// non-empty, only struct fields whose names match filter
// are printed.
//
// The depth parameter controls how deep traversal recurses
// before it returns (higher value means greater depth).
// If an empty field filter is given, a good depth default value
// is 4. A negative depth means no depth limit, which may be fine
// for small data structures or if there is a non-empty filter.
//
// In the output, Node structs are identified by their Op name
// rather than their type; struct fields with zero values or
// non-matching field names are omitted, and "…" means recursion
// depth has been reached or struct fields have been omitted.
public static void FDumpAny(io.Writer w, object root, @string filter, nint depth) {
    if (root == null) {
        fmt.Fprintln(w, "nil");
        return ;
    }
    if (filter == "") {
        filter = ".*"; // default
    }
    dumper p = new dumper(output:w,fieldrx:regexp.MustCompile(filter),ptrmap:make(map[uintptr]int),last:'\n',);

    p.dump(reflect.ValueOf(root), depth);
    p.printf("\n");
}

private partial struct dumper {
    public io.Writer output;
    public ptr<regexp.Regexp> fieldrx; // field name filter
    public map<System.UIntPtr, nint> ptrmap; // ptr -> dump line number
    public @string lastadr; // last address string printed (for shortening)

// output
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

// printf is a convenience wrapper.
private static void printf(this ptr<dumper> _addr_p, @string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();
    ref dumper p = ref _addr_p.val;

    {
        var (_, err) = fmt.Fprintf(p, format, args);

        if (err != null) {
            panic(err);
        }
    }
});

// addr returns the (hexadecimal) address string of the object
// represented by x (or "?" if x is not addressable), with the
// common prefix between this and the prior address replaced by
// "0x…" to make it easier to visually match addresses.
private static @string addr(this ptr<dumper> _addr_p, reflect.Value x) {
    ref dumper p = ref _addr_p.val;

    if (!x.CanAddr()) {
        return "?";
    }
    var adr = fmt.Sprintf("%p", x.Addr().Interface());
    var s = adr;
    {
        var i = commonPrefixLen(p.lastadr, adr);

        if (i > 0) {
            s = "0x…" + adr[(int)i..];
        }
    }
    p.lastadr = adr;
    return s;
}

// dump prints the contents of x.
private static void dump(this ptr<dumper> _addr_p, reflect.Value x, nint depth) {
    ref dumper p = ref _addr_p.val;

    if (depth == 0) {
        p.printf("…");
        return ;
    }
    {
        src.XPos (pos, ok) = x.Interface()._<src.XPos>();

        if (ok) {
            p.printf("%s", @base.FmtPos(pos));
            return ;
        }
    }


    if (x.Kind() == reflect.String) 
        p.printf("%q", x.Interface()); // print strings in quotes
    else if (x.Kind() == reflect.Interface) 
        if (x.IsNil()) {
            p.printf("nil");
            return ;
        }
        p.dump(x.Elem(), depth - 1);
    else if (x.Kind() == reflect.Ptr) 
        if (x.IsNil()) {
            p.printf("nil");
            return ;
        }
        p.printf("*");
        var ptr = x.Pointer();
        {
            var (line, exists) = p.ptrmap[ptr];

            if (exists) {
                p.printf("(@%d)", line);
                return ;
            }

        }
        p.ptrmap[ptr] = p.line;
        p.dump(x.Elem(), depth); // don't count pointer indirection towards depth
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
                    p.dump(x.Index(i), depth - 1);
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

        var isNode = false;
        {
            var n__prev1 = n;

            Node (n, ok) = x.Interface()._<Node>();

            if (ok) {
                isNode = true;
                p.printf("%s %s {", n.Op().String(), p.addr(x));
            }
            else
 {
                p.printf("%s {", typ);
            }

            n = n__prev1;

        }
        p.indent++;

        var first = true;
        var omitted = false;
        {
            nint i__prev1 = i;
            var n__prev1 = n;

            for (i = 0;
            n = typ.NumField(); i < n; i++) { 
                // Exclude non-exported fields because their
                // values cannot be accessed via reflection.
                {
                    var name = typ.Field(i).Name;

                    if (types.IsExported(name)) {
                        if (!p.fieldrx.MatchString(name)) {
                            omitted = true;
                            continue; // field name not selected by filter
                        } 

                        // special cases
                        if (isNode && name == "Op") {
                            omitted = true;
                            continue; // Op field already printed for Nodes
                        }
                        var x = x.Field(i);
                        if (isZeroVal(x)) {
                            omitted = true;
                            continue; // exclude zero-valued fields
                        }
                        {
                            var n__prev2 = n;

                            (n, ok) = x.Interface()._<Nodes>();

                            if (ok && len(n) == 0) {
                                omitted = true;
                                continue; // exclude empty Nodes slices
                            }

                            n = n__prev2;

                        }

                        if (first) {
                            p.printf("\n");
                            first = false;
                        }
                        p.printf("%s: ", name);
                        p.dump(x, depth - 1);
                        p.printf("\n");
                    }

                }
            }


            i = i__prev1;
            n = n__prev1;
        }
        if (omitted) {
            p.printf("…\n");
        }
        p.indent--;
        p.printf("}");
    else 
        p.printf("%v", x.Interface());
    }

private static bool isZeroVal(reflect.Value x) {

    if (x.Kind() == reflect.Bool) 
        return !x.Bool();
    else if (x.Kind() == reflect.Int || x.Kind() == reflect.Int8 || x.Kind() == reflect.Int16 || x.Kind() == reflect.Int32 || x.Kind() == reflect.Int64) 
        return x.Int() == 0;
    else if (x.Kind() == reflect.Uint || x.Kind() == reflect.Uint8 || x.Kind() == reflect.Uint16 || x.Kind() == reflect.Uint32 || x.Kind() == reflect.Uint64 || x.Kind() == reflect.Uintptr) 
        return x.Uint() == 0;
    else if (x.Kind() == reflect.String) 
        return x.String() == "";
    else if (x.Kind() == reflect.Interface || x.Kind() == reflect.Ptr || x.Kind() == reflect.Slice) 
        return x.IsNil();
        return false;
}

private static nint commonPrefixLen(@string a, @string b) {
    nint i = default;

    while (i < len(a) && i < len(b) && a[i] == b[i]) {
        i++;
    }
    return ;
}

} // end ir_package
