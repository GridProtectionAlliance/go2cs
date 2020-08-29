// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements Selections.

// package types -- go2cs converted at 2020 August 29 08:47:54 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\selection.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // SelectionKind describes the kind of a selector expression x.f
        // (excluding qualified identifiers).
        public partial struct SelectionKind // : long
        {
        }

        public static readonly SelectionKind FieldVal = iota; // x.f is a struct field selector
        public static readonly var MethodVal = 0; // x.f is a method selector
        public static readonly var MethodExpr = 1; // x.f is a method expression

        // A Selection describes a selector expression x.f.
        // For the declarations:
        //
        //    type T struct{ x int; E }
        //    type E struct{}
        //    func (e E) m() {}
        //    var p *T
        //
        // the following relations exist:
        //
        //    Selector    Kind          Recv    Obj    Type               Index     Indirect
        //
        //    p.x         FieldVal      T       x      int                {0}       true
        //    p.m         MethodVal     *T      m      func (e *T) m()    {1, 0}    true
        //    T.m         MethodExpr    T       m      func m(_ T)        {1, 0}    false
        //
        public partial struct Selection
        {
            public SelectionKind kind;
            public Type recv; // type of x
            public Object obj; // object denoted by x.f
            public slice<long> index; // path from x to x.f
            public bool indirect; // set if there was any pointer indirection on the path
        }

        // Kind returns the selection kind.
        private static SelectionKind Kind(this ref Selection s)
        {
            return s.kind;
        }

        // Recv returns the type of x in x.f.
        private static Type Recv(this ref Selection s)
        {
            return s.recv;
        }

        // Obj returns the object denoted by x.f; a *Var for
        // a field selection, and a *Func in all other cases.
        private static Object Obj(this ref Selection s)
        {
            return s.obj;
        }

        // Type returns the type of x.f, which may be different from the type of f.
        // See Selection for more information.
        private static Type Type(this ref Selection s)
        {

            if (s.kind == MethodVal) 
                // The type of x.f is a method with its receiver type set
                // to the type of x.
                ref Signature sig = s.obj._<ref Func>().typ._<ref Signature>().Value;
                var recv = sig.recv.Value;
                recv.typ = s.recv;
                sig.recv = ref recv;
                return ref sig;
            else if (s.kind == MethodExpr) 
                // The type of x.f is a function (without receiver)
                // and an additional first argument with the same type as x.
                // TODO(gri) Similar code is already in call.go - factor!
                // TODO(gri) Compute this eagerly to avoid allocations.
                sig = s.obj._<ref Func>().typ._<ref Signature>().Value;
                var arg0 = sig.recv.Value;
                sig.recv = null;
                arg0.typ = s.recv;
                slice<ref Var> @params = default;
                if (sig.@params != null)
                {
                    params = sig.@params.vars;
                }
                sig.@params = NewTuple(append(new slice<ref Var>(new ref Var[] { &arg0 }), params));
                return ref sig;
            // In all other cases, the type of x.f is the type of x.
            return s.obj.Type();
        }

        // Index describes the path from x to f in x.f.
        // The last index entry is the field or method index of the type declaring f;
        // either:
        //
        //    1) the list of declared methods of a named type; or
        //    2) the list of methods of an interface type; or
        //    3) the list of fields of a struct type.
        //
        // The earlier index entries are the indices of the embedded fields implicitly
        // traversed to get from (the type of) x to f, starting at embedding depth 0.
        private static slice<long> Index(this ref Selection s)
        {
            return s.index;
        }

        // Indirect reports whether any pointer indirection was required to get from
        // x to f in x.f.
        private static bool Indirect(this ref Selection s)
        {
            return s.indirect;
        }

        private static @string String(this ref Selection s)
        {
            return SelectionString(s, null);
        }

        // SelectionString returns the string form of s.
        // The Qualifier controls the printing of
        // package-level objects, and may be nil.
        //
        // Examples:
        //    "field (T) f int"
        //    "method (T) f(X) Y"
        //    "method expr (T) f(X) Y"
        //
        public static @string SelectionString(ref Selection s, Qualifier qf)
        {
            @string k = default;

            if (s.kind == FieldVal) 
                k = "field ";
            else if (s.kind == MethodVal) 
                k = "method ";
            else if (s.kind == MethodExpr) 
                k = "method expr ";
            else 
                unreachable();
                        bytes.Buffer buf = default;
            buf.WriteString(k);
            buf.WriteByte('(');
            WriteType(ref buf, s.Recv(), qf);
            fmt.Fprintf(ref buf, ") %s", s.obj.Name());
            {
                var T = s.Type();

                if (s.kind == FieldVal)
                {
                    buf.WriteByte(' ');
                    WriteType(ref buf, T, qf);
                }
                else
                {
                    WriteSignature(ref buf, T._<ref Signature>(), qf);
                }

            }
            return buf.String();
        }
    }
}}
