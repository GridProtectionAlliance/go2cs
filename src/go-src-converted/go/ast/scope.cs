// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements scopes and the objects they contain.

// package ast -- go2cs converted at 2020 August 29 08:48:35 UTC
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
            public map<@string, ref Object> Objects;
        }

        // NewScope creates a new scope nested in the outer scope.
        public static ref Scope NewScope(ref Scope outer)
        {
            const long n = 4L; // initial scope capacity
 // initial scope capacity
            return ref new Scope(outer,make(map[string]*Object,n));
        }

        // Lookup returns the object with the given name if it is
        // found in scope s, otherwise it returns nil. Outer scopes
        // are ignored.
        //
        private static ref Object Lookup(this ref Scope s, @string name)
        {
            return s.Objects[name];
        }

        // Insert attempts to insert a named object obj into the scope s.
        // If the scope already contains an object alt with the same name,
        // Insert leaves the scope unchanged and returns alt. Otherwise
        // it inserts obj and returns nil.
        //
        private static ref Object Insert(this ref Scope s, ref Object obj)
        {
            alt = s.Objects[obj.Name];

            if (alt == null)
            {
                s.Objects[obj.Name] = obj;
            }
            return;
        }

        // Debugging support
        private static @string String(this ref Scope s)
        {
            bytes.Buffer buf = default;
            fmt.Fprintf(ref buf, "scope %p {", s);
            if (s != null && len(s.Objects) > 0L)
            {
                fmt.Fprintln(ref buf);
                foreach (var (_, obj) in s.Objects)
                {
                    fmt.Fprintf(ref buf, "\t%s %s\n", obj.Kind, obj.Name);
                }
            }
            fmt.Fprintf(ref buf, "}\n");
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
        public static ref Object NewObj(ObjKind kind, @string name)
        {
            return ref new Object(Kind:kind,Name:name);
        }

        // Pos computes the source position of the declaration of an object name.
        // The result may be an invalid position if it cannot be computed
        // (obj.Decl may be nil or not correct).
        private static token.Pos Pos(this ref Object obj)
        {
            var name = obj.Name;
            switch (obj.Decl.type())
            {
                case ref Field d:
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
                case ref ImportSpec d:
                    if (d.Name != null && d.Name.Name == name)
                    {
                        return d.Name.Pos();
                    }
                    return d.Path.Pos();
                    break;
                case ref ValueSpec d:
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
                case ref TypeSpec d:
                    if (d.Name.Name == name)
                    {
                        return d.Name.Pos();
                    }
                    break;
                case ref FuncDecl d:
                    if (d.Name.Name == name)
                    {
                        return d.Name.Pos();
                    }
                    break;
                case ref LabeledStmt d:
                    if (d.Label.Name == name)
                    {
                        return d.Label.Pos();
                    }
                    break;
                case ref AssignStmt d:
                    foreach (var (_, x) in d.Lhs)
                    {
                        {
                            ref Ident (ident, isIdent) = x._<ref Ident>();

                            if (isIdent && ident.Name == name)
                            {
                                return ident.Pos();
                            }

                        }
                    }
                    break;
                case ref Scope d:
                    break;
            }
            return token.NoPos;
        }

        // ObjKind describes what an object represents.
        public partial struct ObjKind // : long
        {
        }

        // The list of possible Object kinds.
        public static readonly ObjKind Bad = iota; // for error handling
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
