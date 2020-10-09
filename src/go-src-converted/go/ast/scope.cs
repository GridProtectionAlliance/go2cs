// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements scopes and the objects they contain.

// package ast -- go2cs converted at 2020 October 09 05:20:06 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Go\src\go\ast\scope.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class ast_package
    {
        // A Scope maintains the set of named language entities declared
        // in the scope and a link to the immediately surrounding (outer)
        // scope.
        //
        public partial struct Scope
        {
            public ptr<Scope> Outer;
            public map<@string, ptr<Object>> Objects;
        }

        // NewScope creates a new scope nested in the outer scope.
        public static ptr<Scope> NewScope(ptr<Scope> _addr_outer)
        {
            ref Scope outer = ref _addr_outer.val;

            const long n = (long)4L; // initial scope capacity
 // initial scope capacity
            return addr(new Scope(outer,make(map[string]*Object,n)));

        }

        // Lookup returns the object with the given name if it is
        // found in scope s, otherwise it returns nil. Outer scopes
        // are ignored.
        //
        private static ptr<Object> Lookup(this ptr<Scope> _addr_s, @string name)
        {
            ref Scope s = ref _addr_s.val;

            return _addr_s.Objects[name]!;
        }

        // Insert attempts to insert a named object obj into the scope s.
        // If the scope already contains an object alt with the same name,
        // Insert leaves the scope unchanged and returns alt. Otherwise
        // it inserts obj and returns nil.
        //
        private static ptr<Object> Insert(this ptr<Scope> _addr_s, ptr<Object> _addr_obj)
        {
            ptr<Object> alt = default!;
            ref Scope s = ref _addr_s.val;
            ref Object obj = ref _addr_obj.val;

            alt = s.Objects[obj.Name];

            if (alt == null)
            {
                s.Objects[obj.Name] = obj;
            }

            return ;

        }

        // Debugging support
        private static @string String(this ptr<Scope> _addr_s)
        {
            ref Scope s = ref _addr_s.val;

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            fmt.Fprintf(_addr_buf, "scope %p {", s);
            if (s != null && len(s.Objects) > 0L)
            {
                fmt.Fprintln(_addr_buf);
                foreach (var (_, obj) in s.Objects)
                {
                    fmt.Fprintf(_addr_buf, "\t%s %s\n", obj.Kind, obj.Name);
                }

            }

            fmt.Fprintf(_addr_buf, "}\n");
            return buf.String();

        }

        // ----------------------------------------------------------------------------
        // Objects

        // An Object describes a named language entity such as a package,
        // constant, type, variable, function (incl. methods), or label.
        //
        // The Data fields contains object-specific data:
        //
        //    Kind    Data type         Data value
        //    Pkg     *Scope            package scope
        //    Con     int               iota for the respective declaration
        //
        public partial struct Object
        {
            public ObjKind Kind;
            public @string Name; // declared name
        }

        // NewObj creates a new object of a given kind and name.
        public static ptr<Object> NewObj(ObjKind kind, @string name)
        {
            return addr(new Object(Kind:kind,Name:name));
        }

        // Pos computes the source position of the declaration of an object name.
        // The result may be an invalid position if it cannot be computed
        // (obj.Decl may be nil or not correct).
        private static token.Pos Pos(this ptr<Object> _addr_obj)
        {
            ref Object obj = ref _addr_obj.val;

            var name = obj.Name;
            switch (obj.Decl.type())
            {
                case ptr<Field> d:
                    {
                        var n__prev1 = n;

                        foreach (var (_, __n) in d.Names)
                        {
                            n = __n;
                            if (n.Name == name)
                            {
                                return n.Pos();
                            }

                        }

                        n = n__prev1;
                    }
                    break;
                case ptr<ImportSpec> d:
                    if (d.Name != null && d.Name.Name == name)
                    {
                        return d.Name.Pos();
                    }

                    return d.Path.Pos();
                    break;
                case ptr<ValueSpec> d:
                    {
                        var n__prev1 = n;

                        foreach (var (_, __n) in d.Names)
                        {
                            n = __n;
                            if (n.Name == name)
                            {
                                return n.Pos();
                            }

                        }

                        n = n__prev1;
                    }
                    break;
                case ptr<TypeSpec> d:
                    if (d.Name.Name == name)
                    {
                        return d.Name.Pos();
                    }

                    break;
                case ptr<FuncDecl> d:
                    if (d.Name.Name == name)
                    {
                        return d.Name.Pos();
                    }

                    break;
                case ptr<LabeledStmt> d:
                    if (d.Label.Name == name)
                    {
                        return d.Label.Pos();
                    }

                    break;
                case ptr<AssignStmt> d:
                    foreach (var (_, x) in d.Lhs)
                    {
                        {
                            ptr<Ident> (ident, isIdent) = x._<ptr<Ident>>();

                            if (isIdent && ident.Name == name)
                            {
                                return ident.Pos();
                            }

                        }

                    }
                    break;
                case ptr<Scope> d:
                    break;
            }
            return token.NoPos;

        }

        // ObjKind describes what an object represents.
        public partial struct ObjKind // : long
        {
        }

        // The list of possible Object kinds.
        public static readonly ObjKind Bad = (ObjKind)iota; // for error handling
        public static readonly var Pkg = 0; // package
        public static readonly var Con = 1; // constant
        public static readonly var Typ = 2; // type
        public static readonly var Var = 3; // variable
        public static readonly var Fun = 4; // function or method
        public static readonly var Lbl = 5; // label

        private static array<@string> objKindStrings = new array<@string>(InitKeyedValues<@string>((Bad, "bad"), (Pkg, "package"), (Con, "const"), (Typ, "type"), (Var, "var"), (Fun, "func"), (Lbl, "label")));

        public static @string String(this ObjKind kind)
        {
            return objKindStrings[kind];
        }
    }
}}
