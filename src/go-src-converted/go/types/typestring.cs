// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements printing of types.

// package types -- go2cs converted at 2020 August 29 08:48:04 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\typestring.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // A Qualifier controls how named package-level objects are printed in
        // calls to TypeString, ObjectString, and SelectionString.
        //
        // These three formatting routines call the Qualifier for each
        // package-level object O, and if the Qualifier returns a non-empty
        // string p, the object is printed in the form p.O.
        // If it returns an empty string, only the object name O is printed.
        //
        // Using a nil Qualifier is equivalent to using (*Package).Path: the
        // object is qualified by the import path, e.g., "encoding/json.Marshal".
        //
        public delegate  @string Qualifier(ref Package);

        // RelativeTo(pkg) returns a Qualifier that fully qualifies members of
        // all packages other than pkg.
        public static Qualifier RelativeTo(ref Package pkg)
        {
            if (pkg == null)
            {
                return null;
            }
            return other =>
            {
                if (pkg == other)
                {
                    return ""; // same package; unqualified
                }
                return other.Path();
            }
;
        }

        // If gcCompatibilityMode is set, printing of types is modified
        // to match the representation of some types in the gc compiler:
        //
        //    - byte and rune lose their alias name and simply stand for
        //      uint8 and int32 respectively
        //    - embedded interfaces get flattened (the embedding info is lost,
        //      and certain recursive interface types cannot be printed anymore)
        //
        // This makes it easier to compare packages computed with the type-
        // checker vs packages imported from gc export data.
        //
        // Caution: This flag affects all uses of WriteType, globally.
        // It is only provided for testing in conjunction with
        // gc-generated data.
        //
        // This flag is exported in the x/tools/go/types package. We don't
        // need it at the moment in the std repo and so we don't export it
        // anymore. We should eventually try to remove it altogether.
        // TODO(gri) remove this
        private static bool gcCompatibilityMode = default;

        // TypeString returns the string representation of typ.
        // The Qualifier controls the printing of
        // package-level objects, and may be nil.
        public static @string TypeString(Type typ, Qualifier qf)
        {
            bytes.Buffer buf = default;
            WriteType(ref buf, typ, qf);
            return buf.String();
        }

        // WriteType writes the string representation of typ to buf.
        // The Qualifier controls the printing of
        // package-level objects, and may be nil.
        public static void WriteType(ref bytes.Buffer buf, Type typ, Qualifier qf)
        {
            writeType(buf, typ, qf, make_slice<Type>(0L, 8L));
        }

        private static void writeType(ref bytes.Buffer _buf, Type typ, Qualifier qf, slice<Type> visited) => func(_buf, (ref bytes.Buffer buf, Defer _, Panic panic, Recover __) =>
        { 
            // Theoretically, this is a quadratic lookup algorithm, but in
            // practice deeply nested composite types with unnamed component
            // types are uncommon. This code is likely more efficient than
            // using a map.
            {
                var t__prev1 = t;

                foreach (var (_, __t) in visited)
                {
                    t = __t;
                    if (t == typ)
                    {
                        fmt.Fprintf(buf, "○%T", typ); // cycle to typ
                        return;
                    }
                }

                t = t__prev1;
            }

            visited = append(visited, typ);

            switch (typ.type())
            {
                case 
                    buf.WriteString("<nil>");
                    break;
                case ref Basic t:
                    if (t.kind == UnsafePointer)
                    {
                        buf.WriteString("unsafe.");
                    }
                    if (gcCompatibilityMode)
                    { 
                        // forget the alias names

                        if (t.kind == Byte) 
                            t = Typ[Uint8];
                        else if (t.kind == Rune) 
                            t = Typ[Int32];
                                            }
                    buf.WriteString(t.name);
                    break;
                case ref Array t:
                    fmt.Fprintf(buf, "[%d]", t.len);
                    writeType(buf, t.elem, qf, visited);
                    break;
                case ref Slice t:
                    buf.WriteString("[]");
                    writeType(buf, t.elem, qf, visited);
                    break;
                case ref Struct t:
                    buf.WriteString("struct{");
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __f) in t.fields)
                        {
                            i = __i;
                            f = __f;
                            if (i > 0L)
                            {
                                buf.WriteString("; ");
                            }
                            if (!f.anonymous)
                            {
                                buf.WriteString(f.name);
                                buf.WriteByte(' ');
                            }
                            writeType(buf, f.typ, qf, visited);
                            {
                                var tag = t.Tag(i);

                                if (tag != "")
                                {
                                    fmt.Fprintf(buf, " %q", tag);
                                }

                            }
                        }

                        i = i__prev1;
                    }

                    buf.WriteByte('}');
                    break;
                case ref Pointer t:
                    buf.WriteByte('*');
                    writeType(buf, t.@base, qf, visited);
                    break;
                case ref Tuple t:
                    writeTuple(buf, t, false, qf, visited);
                    break;
                case ref Signature t:
                    buf.WriteString("func");
                    writeSignature(buf, t, qf, visited);
                    break;
                case ref Interface t:
                    buf.WriteString("interface{");
                    var empty = true;
                    if (gcCompatibilityMode)
                    { 
                        // print flattened interface
                        // (useful to compare against gc-generated interfaces)
                        {
                            var i__prev1 = i;
                            var m__prev1 = m;

                            foreach (var (__i, __m) in t.allMethods)
                            {
                                i = __i;
                                m = __m;
                                if (i > 0L)
                                {
                                    buf.WriteString("; ");
                                }
                                buf.WriteString(m.name);
                                writeSignature(buf, m.typ._<ref Signature>(), qf, visited);
                                empty = false;
                            }
                    else

                            i = i__prev1;
                            m = m__prev1;
                        }

                    }                    { 
                        // print explicit interface methods and embedded types
                        {
                            var i__prev1 = i;
                            var m__prev1 = m;

                            foreach (var (__i, __m) in t.methods)
                            {
                                i = __i;
                                m = __m;
                                if (i > 0L)
                                {
                                    buf.WriteString("; ");
                                }
                                buf.WriteString(m.name);
                                writeSignature(buf, m.typ._<ref Signature>(), qf, visited);
                                empty = false;
                            }

                            i = i__prev1;
                            m = m__prev1;
                        }

                        {
                            var i__prev1 = i;

                            foreach (var (__i, __typ) in t.embeddeds)
                            {
                                i = __i;
                                typ = __typ;
                                if (i > 0L || len(t.methods) > 0L)
                                {
                                    buf.WriteString("; ");
                                }
                                writeType(buf, typ, qf, visited);
                                empty = false;
                            }

                            i = i__prev1;
                        }

                    }
                    if (t.allMethods == null || len(t.methods) > len(t.allMethods))
                    {
                        if (!empty)
                        {
                            buf.WriteByte(' ');
                        }
                        buf.WriteString("/* incomplete */");
                    }
                    buf.WriteByte('}');
                    break;
                case ref Map t:
                    buf.WriteString("map[");
                    writeType(buf, t.key, qf, visited);
                    buf.WriteByte(']');
                    writeType(buf, t.elem, qf, visited);
                    break;
                case ref Chan t:
                    @string s = default;
                    bool parens = default;

                    if (t.dir == SendRecv) 
                        s = "chan "; 
                        // chan (<-chan T) requires parentheses
                        {
                            ref Chan (c, _) = t.elem._<ref Chan>();

                            if (c != null && c.dir == RecvOnly)
                            {
                                parens = true;
                            }

                        }
                    else if (t.dir == SendOnly) 
                        s = "chan<- ";
                    else if (t.dir == RecvOnly) 
                        s = "<-chan ";
                    else 
                        panic("unreachable");
                                        buf.WriteString(s);
                    if (parens)
                    {
                        buf.WriteByte('(');
                    }
                    writeType(buf, t.elem, qf, visited);
                    if (parens)
                    {
                        buf.WriteByte(')');
                    }
                    break;
                case ref Named t:
                    s = "<Named w/o object>";
                    {
                        var obj = t.obj;

                        if (obj != null)
                        {
                            if (obj.pkg != null)
                            {
                                writePackage(buf, obj.pkg, qf);
                            } 
                            // TODO(gri): function-local named types should be displayed
                            // differently from named types at package level to avoid
                            // ambiguity.
                            s = obj.name;
                        }

                    }
                    buf.WriteString(s);
                    break;
                default:
                {
                    var t = typ.type();
                    buf.WriteString(t.String());
                    break;
                }
            }
        });

        private static void writeTuple(ref bytes.Buffer _buf, ref Tuple _tup, bool variadic, Qualifier qf, slice<Type> visited) => func(_buf, _tup, (ref bytes.Buffer buf, ref Tuple tup, Defer _, Panic panic, Recover __) =>
        {
            buf.WriteByte('(');
            if (tup != null)
            {
                foreach (var (i, v) in tup.vars)
                {
                    if (i > 0L)
                    {
                        buf.WriteString(", ");
                    }
                    if (v.name != "")
                    {
                        buf.WriteString(v.name);
                        buf.WriteByte(' ');
                    }
                    var typ = v.typ;
                    if (variadic && i == len(tup.vars) - 1L)
                    {
                        {
                            ref Slice (s, ok) = typ._<ref Slice>();

                            if (ok)
                            {
                                buf.WriteString("...");
                                typ = s.elem;
                            }
                            else
                            { 
                                // special case:
                                // append(s, "foo"...) leads to signature func([]byte, string...)
                                {
                                    ref Basic (t, ok) = typ.Underlying()._<ref Basic>();

                                    if (!ok || t.kind != String)
                                    {
                                        panic("internal error: string type expected");
                                    }

                                }
                                writeType(buf, typ, qf, visited);
                                buf.WriteString("...");
                                continue;
                            }

                        }
                    }
                    writeType(buf, typ, qf, visited);
                }
            }
            buf.WriteByte(')');
        });

        // WriteSignature writes the representation of the signature sig to buf,
        // without a leading "func" keyword.
        // The Qualifier controls the printing of
        // package-level objects, and may be nil.
        public static void WriteSignature(ref bytes.Buffer buf, ref Signature sig, Qualifier qf)
        {
            writeSignature(buf, sig, qf, make_slice<Type>(0L, 8L));
        }

        private static void writeSignature(ref bytes.Buffer buf, ref Signature sig, Qualifier qf, slice<Type> visited)
        {
            writeTuple(buf, sig.@params, sig.variadic, qf, visited);

            var n = sig.results.Len();
            if (n == 0L)
            { 
                // no result
                return;
            }
            buf.WriteByte(' ');
            if (n == 1L && sig.results.vars[0L].name == "")
            { 
                // single unnamed result
                writeType(buf, sig.results.vars[0L].typ, qf, visited);
                return;
            } 

            // multiple or named result(s)
            writeTuple(buf, sig.results, false, qf, visited);
        }
    }
}}
