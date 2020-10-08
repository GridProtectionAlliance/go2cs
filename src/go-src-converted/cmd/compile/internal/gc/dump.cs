// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements textual dumping of arbitrary data structures
// for debugging purposes. The code is customized for Node graphs
// and may be used for an alternative view of the node structure.

// package gc -- go2cs converted at 2020 October 08 04:28:40 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\dump.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using reflect = go.reflect_package;
using regexp = go.regexp_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // dump is like fdump but prints to stderr.
        private static void dump(object root, @string filter, long depth)
        {
            fdump(os.Stderr, root, filter, depth);
        }

        // fdump prints the structure of a rooted data structure
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
        private static void fdump(io.Writer w, object root, @string filter, long depth)
        {
            if (root == null)
            {
                fmt.Fprintln(w, "nil");
                return ;
            }

            if (filter == "")
            {
                filter = ".*"; // default
            }

            dumper p = new dumper(output:w,fieldrx:regexp.MustCompile(filter),ptrmap:make(map[uintptr]int),last:'\n',);

            p.dump(reflect.ValueOf(root), depth);
            p.printf("\n");

        }

        private partial struct dumper
        {
            public io.Writer output;
            public ptr<regexp.Regexp> fieldrx; // field name filter
            public map<System.UIntPtr, long> ptrmap; // ptr -> dump line number
            public @string lastadr; // last address string printed (for shortening)

// output
            public long indent; // current indentation level
            public byte last; // last byte processed by Write
            public long line; // current line number
        }

        private static slice<byte> indentBytes = (slice<byte>)".  ";

        private static (long, error) Write(this ptr<dumper> _addr_p, slice<byte> data)
        {
            long n = default;
            error err = default!;
            ref dumper p = ref _addr_p.val;

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
                        return ;
                    }

                }
                else if (p.last == '\n')
                {
                    p.line++;
                    _, err = fmt.Fprintf(p.output, "%6d  ", p.line);
                    if (err != null)
                    {
                        return ;
                    }

                    for (var j = p.indent; j > 0L; j--)
                    {
                        _, err = p.output.Write(indentBytes);
                        if (err != null)
                        {
                            return ;
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

            return ;

        }

        // printf is a convenience wrapper.
        private static void printf(this ptr<dumper> _addr_p, @string format, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();
            ref dumper p = ref _addr_p.val;

            {
                var (_, err) = fmt.Fprintf(p, format, args);

                if (err != null)
                {
                    panic(err);
                }

            }

        });

        // addr returns the (hexadecimal) address string of the object
        // represented by x (or "?" if x is not addressable), with the
        // common prefix between this and the prior address replaced by
        // "0x…" to make it easier to visually match addresses.
        private static @string addr(this ptr<dumper> _addr_p, reflect.Value x)
        {
            ref dumper p = ref _addr_p.val;

            if (!x.CanAddr())
            {
                return "?";
            }

            var adr = fmt.Sprintf("%p", x.Addr().Interface());
            var s = adr;
            {
                var i = commonPrefixLen(p.lastadr, adr);

                if (i > 0L)
                {
                    s = "0x…" + adr[i..];
                }

            }

            p.lastadr = adr;
            return s;

        }

        // dump prints the contents of x.
        private static void dump(this ptr<dumper> _addr_p, reflect.Value x, long depth)
        {
            ref dumper p = ref _addr_p.val;

            if (depth == 0L)
            {
                p.printf("…");
                return ;
            } 

            // special cases
            switch (x.Interface().type())
            {
                case Nodes v:
                    x = reflect.ValueOf(v.Slice());
                    break;
                case src.XPos v:
                    p.printf("%s", linestr(v));
                    return ;
                    break;
                case ptr<types.Node> v:
                    x = reflect.ValueOf(asNode(v));
                    break;

            }


            if (x.Kind() == reflect.String) 
                p.printf("%q", x.Interface()); // print strings in quotes
            else if (x.Kind() == reflect.Interface) 
                if (x.IsNil())
                {
                    p.printf("nil");
                    return ;
                }

                p.dump(x.Elem(), depth - 1L);
            else if (x.Kind() == reflect.Ptr) 
                if (x.IsNil())
                {
                    p.printf("nil");
                    return ;
                }

                p.printf("*");
                var ptr = x.Pointer();
                {
                    var (line, exists) = p.ptrmap[ptr];

                    if (exists)
                    {
                        p.printf("(@%d)", line);
                        return ;
                    }

                }

                p.ptrmap[ptr] = p.line;
                p.dump(x.Elem(), depth); // don't count pointer indirection towards depth
            else if (x.Kind() == reflect.Slice) 
                if (x.IsNil())
                {
                    p.printf("nil");
                    return ;
                }

                p.printf("%s (%d entries) {", x.Type(), x.Len());
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
                            p.dump(x.Index(i), depth - 1L);
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

                    if (ok)
                    {
                        isNode = true;
                        p.printf("%s %s {", n.Op.String(), p.addr(x));
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
                    long i__prev1 = i;
                    var n__prev1 = n;

                    for (i = 0L;
                    n = typ.NumField(); i < n; i++)
                    { 
                        // Exclude non-exported fields because their
                        // values cannot be accessed via reflection.
                        {
                            var name = typ.Field(i).Name;

                            if (types.IsExported(name))
                            {
                                if (!p.fieldrx.MatchString(name))
                                {
                                    omitted = true;
                                    continue; // field name not selected by filter
                                } 

                                // special cases
                                if (isNode && name == "Op")
                                {
                                    omitted = true;
                                    continue; // Op field already printed for Nodes
                                }

                                var x = x.Field(i);
                                if (isZeroVal(x))
                                {
                                    omitted = true;
                                    continue; // exclude zero-valued fields
                                }

                                {
                                    var n__prev2 = n;

                                    (n, ok) = x.Interface()._<Nodes>();

                                    if (ok && n.Len() == 0L)
                                    {
                                        omitted = true;
                                        continue; // exclude empty Nodes slices
                                    }

                                    n = n__prev2;

                                }


                                if (first)
                                {
                                    p.printf("\n");
                                    first = false;
                                }

                                p.printf("%s: ", name);
                                p.dump(x, depth - 1L);
                                p.printf("\n");

                            }

                        }

                    }


                    i = i__prev1;
                    n = n__prev1;
                }
                if (omitted)
                {
                    p.printf("…\n");
                }

                p.indent--;
                p.printf("}");
            else 
                p.printf("%v", x.Interface());
            
        }

        private static bool isZeroVal(reflect.Value x)
        {

            if (x.Kind() == reflect.Bool) 
                return !x.Bool();
            else if (x.Kind() == reflect.Int || x.Kind() == reflect.Int8 || x.Kind() == reflect.Int16 || x.Kind() == reflect.Int32 || x.Kind() == reflect.Int64) 
                return x.Int() == 0L;
            else if (x.Kind() == reflect.Uint || x.Kind() == reflect.Uint8 || x.Kind() == reflect.Uint16 || x.Kind() == reflect.Uint32 || x.Kind() == reflect.Uint64 || x.Kind() == reflect.Uintptr) 
                return x.Uint() == 0L;
            else if (x.Kind() == reflect.String) 
                return x.String() == "";
            else if (x.Kind() == reflect.Interface || x.Kind() == reflect.Ptr || x.Kind() == reflect.Slice) 
                return x.IsNil();
                        return false;

        }

        private static long commonPrefixLen(@string a, @string b)
        {
            long i = default;

            while (i < len(a) && i < len(b) && a[i] == b[i])
            {
                i++;
            }

            return ;

        }
    }
}}}}
