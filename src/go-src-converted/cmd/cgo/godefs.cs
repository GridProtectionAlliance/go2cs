// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:52:14 UTC
// Original source: C:\Go\src\cmd\cgo\godefs.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using printer = go.go.printer_package;
using token = go.go.token_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // godefs returns the output for -godefs mode.
        private static @string godefs(this ref Package p, ref File f, @string srcfile)
        {
            bytes.Buffer buf = default;

            fmt.Fprintf(ref buf, "// Created by cgo -godefs - DO NOT EDIT\n");
            fmt.Fprintf(ref buf, "// %s %s\n", filepath.Base(os.Args[0L]), strings.Join(os.Args[1L..], " "));
            fmt.Fprintf(ref buf, "\n");

            var @override = make_map<@string, @string>(); 

            // Allow source file to specify override mappings.
            // For example, the socket data structures refer
            // to in_addr and in_addr6 structs but we want to be
            // able to treat them as byte arrays, so the godefs
            // inputs in package syscall say
            //
            //    // +godefs map struct_in_addr [4]byte
            //    // +godefs map struct_in_addr6 [16]byte
            //
            foreach (var (_, g) in f.Comments)
            {
                foreach (var (_, c) in g.List)
                {
                    var i = strings.Index(c.Text, "+godefs map");
                    if (i < 0L)
                    {
                        continue;
                    }
                    var s = strings.TrimSpace(c.Text[i + len("+godefs map")..]);
                    i = strings.Index(s, " ");
                    if (i < 0L)
                    {
                        fmt.Fprintf(os.Stderr, "invalid +godefs map comment: %s\n", c.Text);
                        continue;
                    }
                    override["_Ctype_" + strings.TrimSpace(s[..i])] = strings.TrimSpace(s[i..]);
                }
            }            {
                var n__prev1 = n;

                foreach (var (_, __n) in f.Name)
                {
                    n = __n;
                    {
                        var s__prev1 = s;

                        s = override[n.Go];

                        if (s != "")
                        {
                            override[n.Mangle] = s;
                        }
                        s = s__prev1;

                    }
                }
                n = n__prev1;
            }

            var refName = make_map<ref ast.Expr, ref Name>();
            foreach (var (_, r) in f.Ref)
            {
                refName[r.Expr] = r.Name;
            }            {
                var d__prev1 = d;

                foreach (var (_, __d) in f.AST.Decls)
                {
                    d = __d;
                    ref ast.GenDecl (d, ok) = d._<ref ast.GenDecl>();
                    if (!ok || d.Tok != token.TYPE)
                    {
                        continue;
                    }
                    {
                        var s__prev2 = s;

                        foreach (var (_, __s) in d.Specs)
                        {
                            s = __s;
                            s = s._<ref ast.TypeSpec>();
                            var n = refName[ref s.Type];
                            if (n != null && n.Mangle != "")
                            {
                                override[n.Mangle] = s.Name.Name;
                            }
                        }
                        s = s__prev2;
                    }

                }
                d = d__prev1;
            }

            {
                var def__prev1 = def;

                foreach (var (__typ, __def) in typedef)
                {
                    typ = __typ;
                    def = __def;
                    {
                        var new__prev1 = new;

                        var @new = override[typ];

                        if (new != "")
                        {
                            {
                                ref ast.Ident id__prev2 = id;

                                ref ast.Ident (id, ok) = def.Go._<ref ast.Ident>();

                                if (ok)
                                {
                                    override[id.Name] = new;
                                }
                                id = id__prev2;

                            }
                        }
                        new = new__prev1;

                    }
                }
                def = def__prev1;
            }

            {
                var new__prev1 = new;

                foreach (var (__old, __new) in override)
                {
                    old = __old;
                    new = __new;
                    {
                        ref ast.Ident id__prev1 = id;

                        var id = goIdent[old];

                        if (id != null)
                        {
                            id.Name = new;
                        }
                        id = id__prev1;

                    }
                }
                new = new__prev1;
            }

            {
                ref ast.Ident id__prev1 = id;

                foreach (var (__name, __id) in goIdent)
                {
                    name = __name;
                    id = __id;
                    if (id.Name == name && strings.Contains(name, "_Ctype_union"))
                    {
                        {
                            var def__prev2 = def;

                            var def = typedef[name];

                            if (def != null)
                            {
                                id.Name = gofmt(def);
                            }
                            def = def__prev2;

                        }
                    }
                }
                id = id__prev1;
            }

            conf.Fprint(ref buf, fset, f.AST);

            return buf.String();
        }

        private static bytes.Buffer gofmtBuf = default;

        // gofmt returns the gofmt-formatted string for an AST node.
        private static @string gofmt(object n)
        {
            gofmtBuf.Reset();
            var err = printer.Fprint(ref gofmtBuf, fset, n);
            if (err != null)
            {
                return "<" + err.Error() + ">";
            }
            return gofmtBuf.String();
        }
    }
}
