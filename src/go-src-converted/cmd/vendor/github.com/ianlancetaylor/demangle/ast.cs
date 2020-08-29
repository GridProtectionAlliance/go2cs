// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package demangle -- go2cs converted at 2020 August 29 10:06:49 UTC
// import "cmd/vendor/github.com/ianlancetaylor/demangle" ==> using demangle = go.cmd.vendor.github.com.ianlancetaylor.demangle_package
// Original source: C:\Go\src\cmd\vendor\github.com\ianlancetaylor\demangle\ast.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace ianlancetaylor
{
    public static partial class demangle_package
    {
        // AST is an abstract syntax tree representing a C++ declaration.
        // This is sufficient for the demangler but is by no means a general C++ AST.
        public partial interface AST
        {
            @string print(ref printState _p0); // Traverse each element of an AST.  If the function returns
// false, traversal of children of that element is skipped.
            @string Traverse(Func<AST, bool> _p0); // Copy an AST with possible transformations.
// If the skip function returns true, no copy is required.
// If the copy function returns nil, no copy is required.
// Otherwise the AST returned by copy is used in a copy of the full AST.
// Copy itself returns either a copy or nil.
            @string Copy(Func<AST, AST> copy, Func<AST, bool> skip); // Implement the fmt.GoStringer interface.
            @string GoString();
            @string goString(long indent, @string field);
        }

        // ASTToString returns the demangled name of the AST.
        public static @string ASTToString(AST a, params Option[] options)
        {
            options = options.Clone();

            var tparams = true;
            foreach (var (_, o) in options)
            {

                if (o == NoTemplateParams) 
                    tparams = false;
                            }
            printState ps = new printState(tparams:tparams);
            a.print(ref ps);
            return ps.buf.String();
        }

        // The printState type holds information needed to print an AST.
        private partial struct printState
        {
            public bool tparams; // whether to print template parameters

            public bytes.Buffer buf;
            public byte last; // Last byte written to buffer.

// The inner field is a list of items to print for a type
// name.  This is used by types to implement the inside-out
// C++ declaration syntax.
            public slice<AST> inner; // The printing field is a list of items we are currently
// printing.  This avoids endless recursion if a substitution
// reference creates a cycle in the graph.
            public slice<AST> printing;
        }

        // writeByte adds a byte to the string being printed.
        private static void writeByte(this ref printState ps, byte b)
        {
            ps.last = b;
            ps.buf.WriteByte(b);
        }

        // writeString adds a string to the string being printed.
        private static void writeString(this ref printState ps, @string s)
        {
            if (len(s) > 0L)
            {
                ps.last = s[len(s) - 1L];
            }
            ps.buf.WriteString(s);
        }

        // Print an AST.
        private static void print(this ref printState ps, AST a)
        {
            long c = 0L;
            foreach (var (_, v) in ps.printing)
            {
                if (v == a)
                { 
                    // We permit the type to appear once, and
                    // return without printing anything if we see
                    // it twice.  This is for a case like
                    // _Z6outer2IsEPFilES1_, where the
                    // substitution is printed differently the
                    // second time because the set of inner types
                    // is different.
                    c++;
                    if (c > 1L)
                    {
                        return;
                    }
                }
            }
            ps.printing = append(ps.printing, a);

            a.print(ps);

            ps.printing = ps.printing[..len(ps.printing) - 1L];
        }

        // Name is an unqualified name.
        public partial struct Name
        {
            public @string Name;
        }

        private static void print(this ref Name n, ref printState ps)
        {
            ps.writeString(n.Name);
        }

        private static bool Traverse(this ref Name n, Func<AST, bool> fn)
        {
            fn(n);
        }

        private static AST Copy(this ref Name n, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(n))
            {
                return null;
            }
            return fn(n);
        }

        private static @string GoString(this ref Name n)
        {
            return n.goString(0L, "Name: ");
        }

        private static @string goString(this ref Name n, long indent, @string field)
        {
            return fmt.Sprintf("%*s%s%s", indent, "", field, n.Name);
        }

        // Typed is a typed name.
        public partial struct Typed
        {
            public AST Name;
            public AST Type;
        }

        private static void print(this ref Typed _t, ref printState _ps) => func(_t, _ps, (ref Typed t, ref printState ps, Defer defer, Panic _, Recover __) =>
        { 
            // We are printing a typed name, so ignore the current set of
            // inner names to print.  Pass down our name as the one to use.
            var holdInner = ps.inner;
            defer(() =>
            {
                ps.inner = holdInner;

            }());

            ps.inner = new slice<AST>(new AST[] { AST.As(t) });
            ps.print(t.Type);
            if (len(ps.inner) > 0L)
            { 
                // The type did not print the name; print it now in
                // the default location.
                ps.writeByte(' ');
                ps.print(t.Name);
            }
        });

        private static void printInner(this ref Typed t, ref printState ps)
        {
            ps.print(t.Name);
        }

        private static bool Traverse(this ref Typed t, Func<AST, bool> fn)
        {
            if (fn(t))
            {
                t.Name.Traverse(fn);
                t.Type.Traverse(fn);
            }
        }

        private static AST Copy(this ref Typed t, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(t))
            {
                return null;
            }
            var name = t.Name.Copy(fn, skip);
            var typ = t.Type.Copy(fn, skip);
            if (name == null && typ == null)
            {
                return fn(t);
            }
            if (name == null)
            {
                name = t.Name;
            }
            if (typ == null)
            {
                typ = t.Type;
            }
            t = ref new Typed(Name:name,Type:typ);
            {
                var r = fn(t);

                if (r != null)
                {
                    return r;
                }

            }
            return t;
        }

        private static @string GoString(this ref Typed t)
        {
            return t.goString(0L, "");
        }

        private static @string goString(this ref Typed t, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sTyped:\n%s\n%s", indent, "", field, t.Name.goString(indent + 2L, "Name: "), t.Type.goString(indent + 2L, "Type: "));
        }

        // Qualified is a name in a scope.
        public partial struct Qualified
        {
            public AST Scope;
            public AST Name; // The LocalName field is true if this is parsed as a
// <local-name>.  We shouldn't really need this, but in some
// cases (for the unary sizeof operator) the standard
// demangler prints a local name slightly differently.  We
// keep track of this for compatibility.
            public bool LocalName; // A full local name encoding
        }

        private static void print(this ref Qualified q, ref printState ps)
        {
            ps.print(q.Scope);
            ps.writeString("::");
            ps.print(q.Name);
        }

        private static bool Traverse(this ref Qualified q, Func<AST, bool> fn)
        {
            if (fn(q))
            {
                q.Scope.Traverse(fn);
                q.Name.Traverse(fn);
            }
        }

        private static AST Copy(this ref Qualified q, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(q))
            {
                return null;
            }
            var scope = q.Scope.Copy(fn, skip);
            var name = q.Name.Copy(fn, skip);
            if (scope == null && name == null)
            {
                return fn(q);
            }
            if (scope == null)
            {
                scope = q.Scope;
            }
            if (name == null)
            {
                name = q.Name;
            }
            q = ref new Qualified(Scope:scope,Name:name,LocalName:q.LocalName);
            {
                var r = fn(q);

                if (r != null)
                {
                    return r;
                }

            }
            return q;
        }

        private static @string GoString(this ref Qualified q)
        {
            return q.goString(0L, "");
        }

        private static @string goString(this ref Qualified q, long indent, @string field)
        {
            @string s = "";
            if (q.LocalName)
            {
                s = " LocalName: true";
            }
            return fmt.Sprintf("%*s%sQualified:%s\n%s\n%s", indent, "", field, s, q.Scope.goString(indent + 2L, "Scope: "), q.Name.goString(indent + 2L, "Name: "));
        }

        // Template is a template with arguments.
        public partial struct Template
        {
            public AST Name;
            public slice<AST> Args;
        }

        private static void print(this ref Template _t, ref printState _ps) => func(_t, _ps, (ref Template t, ref printState ps, Defer defer, Panic _, Recover __) =>
        { 
            // Inner types apply to the template as a whole, they don't
            // cross over into the template.
            var holdInner = ps.inner;
            defer(() =>
            {
                ps.inner = holdInner;

            }());

            ps.inner = null;
            ps.print(t.Name);

            if (!ps.tparams)
            { 
                // Do not print template parameters.
                return;
            } 
            // We need an extra space after operator<.
            if (ps.last == '<')
            {
                ps.writeByte(' ');
            }
            ps.writeByte('<');
            var first = true;
            foreach (var (_, a) in t.Args)
            {
                if (ps.isEmpty(a))
                {
                    continue;
                }
                if (!first)
                {
                    ps.writeString(", ");
                }
                ps.print(a);
                first = false;
            }
            if (ps.last == '>')
            { 
                // Avoid syntactic ambiguity in old versions of C++.
                ps.writeByte(' ');
            }
            ps.writeByte('>');
        });

        private static bool Traverse(this ref Template t, Func<AST, bool> fn)
        {
            if (fn(t))
            {
                t.Name.Traverse(fn);
                foreach (var (_, a) in t.Args)
                {
                    a.Traverse(fn);
                }
            }
        }

        private static AST Copy(this ref Template t, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(t))
            {
                return null;
            }
            var name = t.Name.Copy(fn, skip);
            var changed = name != null;
            var args = make_slice<AST>(len(t.Args));
            foreach (var (i, a) in t.Args)
            {
                var ac = a.Copy(fn, skip);
                if (ac == null)
                {
                    args[i] = a;
                }
                else
                {
                    args[i] = ac;
                    changed = true;
                }
            }
            if (!changed)
            {
                return fn(t);
            }
            if (name == null)
            {
                name = t.Name;
            }
            t = ref new Template(Name:name,Args:args);
            {
                var r = fn(t);

                if (r != null)
                {
                    return r;
                }

            }
            return t;
        }

        private static @string GoString(this ref Template t)
        {
            return t.goString(0L, "");
        }

        private static @string goString(this ref Template t, long indent, @string field)
        {
            @string args = default;
            if (len(t.Args) == 0L)
            {
                args = fmt.Sprintf("%*sArgs: nil", indent + 2L, "");
            }
            else
            {
                args = fmt.Sprintf("%*sArgs:", indent + 2L, "");
                foreach (var (i, a) in t.Args)
                {
                    args += "\n";
                    args += a.goString(indent + 4L, fmt.Sprintf("%d: ", i));
                }
            }
            return fmt.Sprintf("%*s%sTemplate (%p):\n%s\n%s", indent, "", field, t, t.Name.goString(indent + 2L, "Name: "), args);
        }

        // TemplateParam is a template parameter.  The Template field is
        // filled in while parsing the demangled string.  We don't normally
        // see these while printing--they are replaced by the simplify
        // function.
        public partial struct TemplateParam
        {
            public long Index;
            public ptr<Template> Template;
        }

        private static void print(this ref TemplateParam _tp, ref printState _ps) => func(_tp, _ps, (ref TemplateParam tp, ref printState ps, Defer _, Panic panic, Recover __) =>
        {
            if (tp.Template == null)
            {
                panic("TemplateParam Template field is nil");
            }
            if (tp.Index >= len(tp.Template.Args))
            {
                panic("TemplateParam Index out of bounds");
            }
            ps.print(tp.Template.Args[tp.Index]);
        });

        private static bool Traverse(this ref TemplateParam tp, Func<AST, bool> fn)
        {
            fn(tp); 
            // Don't traverse Template--it points elsewhere in the AST.
        }

        private static AST Copy(this ref TemplateParam tp, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(tp))
            {
                return null;
            }
            return fn(tp);
        }

        private static @string GoString(this ref TemplateParam tp)
        {
            return tp.goString(0L, "");
        }

        private static @string goString(this ref TemplateParam tp, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sTemplateParam: Template: %p; Index %d", indent, "", field, tp.Template, tp.Index);
        }

        // Qualifiers is an ordered list of type qualifiers.
        public partial struct Qualifiers // : slice<@string>
        {
        }

        // TypeWithQualifiers is a type with standard qualifiers.
        public partial struct TypeWithQualifiers
        {
            public AST Base;
            public Qualifiers Qualifiers;
        }

        private static void print(this ref TypeWithQualifiers twq, ref printState ps)
        { 
            // Give the base type a chance to print the inner types.
            ps.inner = append(ps.inner, twq);
            ps.print(twq.Base);
            if (len(ps.inner) > 0L)
            { 
                // The qualifier wasn't printed by Base.
                ps.writeByte(' ');
                ps.writeString(strings.Join(twq.Qualifiers, " "));
                ps.inner = ps.inner[..len(ps.inner) - 1L];
            }
        }

        // Print qualifiers as an inner type by just printing the qualifiers.
        private static void printInner(this ref TypeWithQualifiers twq, ref printState ps)
        {
            ps.writeByte(' ');
            ps.writeString(strings.Join(twq.Qualifiers, " "));
        }

        private static bool Traverse(this ref TypeWithQualifiers twq, Func<AST, bool> fn)
        {
            if (fn(twq))
            {
                twq.Base.Traverse(fn);
            }
        }

        private static AST Copy(this ref TypeWithQualifiers twq, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(twq))
            {
                return null;
            }
            var @base = twq.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(twq);
            }
            twq = ref new TypeWithQualifiers(Base:base,Qualifiers:twq.Qualifiers);
            {
                var r = fn(twq);

                if (r != null)
                {
                    return r;
                }

            }
            return twq;
        }

        private static @string GoString(this ref TypeWithQualifiers twq)
        {
            return twq.goString(0L, "");
        }

        private static @string goString(this ref TypeWithQualifiers twq, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sTypeWithQualifiers: Qualifiers: %s\n%s", indent, "", field, twq.Qualifiers, twq.Base.goString(indent + 2L, "Base: "));
        }

        // MethodWithQualifiers is a method with qualifiers.
        public partial struct MethodWithQualifiers
        {
            public AST Method;
            public Qualifiers Qualifiers;
            public @string RefQualifier; // "" or "&" or "&&"
        }

        private static void print(this ref MethodWithQualifiers mwq, ref printState ps)
        { 
            // Give the base type a chance to print the inner types.
            ps.inner = append(ps.inner, mwq);
            ps.print(mwq.Method);
            if (len(ps.inner) > 0L)
            {
                if (len(mwq.Qualifiers) > 0L)
                {
                    ps.writeByte(' ');
                    ps.writeString(strings.Join(mwq.Qualifiers, " "));
                }
                if (mwq.RefQualifier != "")
                {
                    ps.writeByte(' ');
                    ps.writeString(mwq.RefQualifier);
                }
                ps.inner = ps.inner[..len(ps.inner) - 1L];
            }
        }

        private static void printInner(this ref MethodWithQualifiers mwq, ref printState ps)
        {
            if (len(mwq.Qualifiers) > 0L)
            {
                ps.writeByte(' ');
                ps.writeString(strings.Join(mwq.Qualifiers, " "));
            }
            if (mwq.RefQualifier != "")
            {
                ps.writeByte(' ');
                ps.writeString(mwq.RefQualifier);
            }
        }

        private static bool Traverse(this ref MethodWithQualifiers mwq, Func<AST, bool> fn)
        {
            if (fn(mwq))
            {
                mwq.Method.Traverse(fn);
            }
        }

        private static AST Copy(this ref MethodWithQualifiers mwq, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(mwq))
            {
                return null;
            }
            var method = mwq.Method.Copy(fn, skip);
            if (method == null)
            {
                return fn(mwq);
            }
            mwq = ref new MethodWithQualifiers(Method:method,Qualifiers:mwq.Qualifiers,RefQualifier:mwq.RefQualifier);
            {
                var r = fn(mwq);

                if (r != null)
                {
                    return r;
                }

            }
            return mwq;
        }

        private static @string GoString(this ref MethodWithQualifiers mwq)
        {
            return mwq.goString(0L, "");
        }

        private static @string goString(this ref MethodWithQualifiers mwq, long indent, @string field)
        {
            @string q = default;
            if (len(mwq.Qualifiers) > 0L)
            {
                q += fmt.Sprintf(" Qualifiers: %v", mwq.Qualifiers);
            }
            if (mwq.RefQualifier != "")
            {
                if (q != "")
                {
                    q += ";";
                }
                q += " RefQualifier: " + mwq.RefQualifier;
            }
            return fmt.Sprintf("%*s%sMethodWithQualifiers:%s\n%s", indent, "", field, q, mwq.Method.goString(indent + 2L, "Method: "));
        }

        // BuiltinType is a builtin type, like "int".
        public partial struct BuiltinType
        {
            public @string Name;
        }

        private static void print(this ref BuiltinType bt, ref printState ps)
        {
            ps.writeString(bt.Name);
        }

        private static bool Traverse(this ref BuiltinType bt, Func<AST, bool> fn)
        {
            fn(bt);
        }

        private static AST Copy(this ref BuiltinType bt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(bt))
            {
                return null;
            }
            return fn(bt);
        }

        private static @string GoString(this ref BuiltinType bt)
        {
            return bt.goString(0L, "");
        }

        private static @string goString(this ref BuiltinType bt, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sBuiltinType: %s", indent, "", field, bt.Name);
        }

        // printBase is common print code for types that are printed with a
        // simple suffix.
        private static void printBase(ref printState ps, AST qual, AST @base)
        {
            ps.inner = append(ps.inner, qual);
            ps.print(base);
            if (len(ps.inner) > 0L)
            {
                qual._<innerPrinter>().printInner(ps);
                ps.inner = ps.inner[..len(ps.inner) - 1L];
            }
        }

        // PointerType is a pointer type.
        public partial struct PointerType
        {
            public AST Base;
        }

        private static void print(this ref PointerType pt, ref printState ps)
        {
            printBase(ps, pt, pt.Base);
        }

        private static void printInner(this ref PointerType pt, ref printState ps)
        {
            ps.writeString("*");
        }

        private static bool Traverse(this ref PointerType pt, Func<AST, bool> fn)
        {
            if (fn(pt))
            {
                pt.Base.Traverse(fn);
            }
        }

        private static AST Copy(this ref PointerType pt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(pt))
            {
                return null;
            }
            var @base = pt.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(pt);
            }
            pt = ref new PointerType(Base:base);
            {
                var r = fn(pt);

                if (r != null)
                {
                    return r;
                }

            }
            return pt;
        }

        private static @string GoString(this ref PointerType pt)
        {
            return pt.goString(0L, "");
        }

        private static @string goString(this ref PointerType pt, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sPointerType:\n%s", indent, "", field, pt.Base.goString(indent + 2L, ""));
        }

        // ReferenceType is a reference type.
        public partial struct ReferenceType
        {
            public AST Base;
        }

        private static void print(this ref ReferenceType rt, ref printState ps)
        {
            printBase(ps, rt, rt.Base);
        }

        private static void printInner(this ref ReferenceType rt, ref printState ps)
        {
            ps.writeString("&");
        }

        private static bool Traverse(this ref ReferenceType rt, Func<AST, bool> fn)
        {
            if (fn(rt))
            {
                rt.Base.Traverse(fn);
            }
        }

        private static AST Copy(this ref ReferenceType rt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(rt))
            {
                return null;
            }
            var @base = rt.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(rt);
            }
            rt = ref new ReferenceType(Base:base);
            {
                var r = fn(rt);

                if (r != null)
                {
                    return r;
                }

            }
            return rt;
        }

        private static @string GoString(this ref ReferenceType rt)
        {
            return rt.goString(0L, "");
        }

        private static @string goString(this ref ReferenceType rt, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sReferenceType:\n%s", indent, "", field, rt.Base.goString(indent + 2L, ""));
        }

        // RvalueReferenceType is an rvalue reference type.
        public partial struct RvalueReferenceType
        {
            public AST Base;
        }

        private static void print(this ref RvalueReferenceType rt, ref printState ps)
        {
            printBase(ps, rt, rt.Base);
        }

        private static void printInner(this ref RvalueReferenceType rt, ref printState ps)
        {
            ps.writeString("&&");
        }

        private static bool Traverse(this ref RvalueReferenceType rt, Func<AST, bool> fn)
        {
            if (fn(rt))
            {
                rt.Base.Traverse(fn);
            }
        }

        private static AST Copy(this ref RvalueReferenceType rt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(rt))
            {
                return null;
            }
            var @base = rt.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(rt);
            }
            rt = ref new RvalueReferenceType(Base:base);
            {
                var r = fn(rt);

                if (r != null)
                {
                    return r;
                }

            }
            return rt;
        }

        private static @string GoString(this ref RvalueReferenceType rt)
        {
            return rt.goString(0L, "");
        }

        private static @string goString(this ref RvalueReferenceType rt, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sRvalueReferenceType:\n%s", indent, "", field, rt.Base.goString(indent + 2L, ""));
        }

        // ComplexType is a complex type.
        public partial struct ComplexType
        {
            public AST Base;
        }

        private static void print(this ref ComplexType ct, ref printState ps)
        {
            printBase(ps, ct, ct.Base);
        }

        private static void printInner(this ref ComplexType ct, ref printState ps)
        {
            ps.writeString(" _Complex");
        }

        private static bool Traverse(this ref ComplexType ct, Func<AST, bool> fn)
        {
            if (fn(ct))
            {
                ct.Base.Traverse(fn);
            }
        }

        private static AST Copy(this ref ComplexType ct, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(ct))
            {
                return null;
            }
            var @base = ct.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(ct);
            }
            ct = ref new ComplexType(Base:base);
            {
                var r = fn(ct);

                if (r != null)
                {
                    return r;
                }

            }
            return ct;
        }

        private static @string GoString(this ref ComplexType ct)
        {
            return ct.goString(0L, "");
        }

        private static @string goString(this ref ComplexType ct, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sComplexType:\n%s", indent, "", field, ct.Base.goString(indent + 2L, ""));
        }

        // ImaginaryType is an imaginary type.
        public partial struct ImaginaryType
        {
            public AST Base;
        }

        private static void print(this ref ImaginaryType it, ref printState ps)
        {
            printBase(ps, it, it.Base);
        }

        private static void printInner(this ref ImaginaryType it, ref printState ps)
        {
            ps.writeString(" _Imaginary");
        }

        private static bool Traverse(this ref ImaginaryType it, Func<AST, bool> fn)
        {
            if (fn(it))
            {
                it.Base.Traverse(fn);
            }
        }

        private static AST Copy(this ref ImaginaryType it, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(it))
            {
                return null;
            }
            var @base = it.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(it);
            }
            it = ref new ImaginaryType(Base:base);
            {
                var r = fn(it);

                if (r != null)
                {
                    return r;
                }

            }
            return it;
        }

        private static @string GoString(this ref ImaginaryType it)
        {
            return it.goString(0L, "");
        }

        private static @string goString(this ref ImaginaryType it, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sImaginaryType:\n%s", indent, "", field, it.Base.goString(indent + 2L, ""));
        }

        // VendorQualifier is a type qualified by a vendor-specific qualifier.
        public partial struct VendorQualifier
        {
            public AST Qualifier;
            public AST Type;
        }

        private static void print(this ref VendorQualifier vq, ref printState ps)
        {
            ps.inner = append(ps.inner, vq);
            ps.print(vq.Type);
            if (len(ps.inner) > 0L)
            {
                ps.printOneInner(null);
            }
        }

        private static void printInner(this ref VendorQualifier vq, ref printState ps)
        {
            ps.writeByte(' ');
            ps.print(vq.Qualifier);
        }

        private static bool Traverse(this ref VendorQualifier vq, Func<AST, bool> fn)
        {
            if (fn(vq))
            {
                vq.Qualifier.Traverse(fn);
                vq.Type.Traverse(fn);
            }
        }

        private static AST Copy(this ref VendorQualifier vq, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(vq))
            {
                return null;
            }
            var qualifier = vq.Qualifier.Copy(fn, skip);
            var typ = vq.Type.Copy(fn, skip);
            if (qualifier == null && typ == null)
            {
                return fn(vq);
            }
            if (qualifier == null)
            {
                qualifier = vq.Qualifier;
            }
            if (typ == null)
            {
                typ = vq.Type;
            }
            vq = ref new VendorQualifier(Qualifier:qualifier,Type:vq.Type);
            {
                var r = fn(vq);

                if (r != null)
                {
                    return r;
                }

            }
            return vq;
        }

        private static @string GoString(this ref VendorQualifier vq)
        {
            return vq.goString(0L, "");
        }

        private static @string goString(this ref VendorQualifier vq, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sVendorQualifier:\n%s\n%s", indent, "", field, vq.Qualifier.goString(indent + 2L, "Qualifier: "), vq.Type.goString(indent + 2L, "Type: "));
        }

        // ArrayType is an array type.
        public partial struct ArrayType
        {
            public AST Dimension;
            public AST Element;
        }

        private static void print(this ref ArrayType at, ref printState ps)
        { 
            // Pass the array type down as an inner type so that we print
            // multi-dimensional arrays correctly.
            ps.inner = append(ps.inner, at);
            ps.print(at.Element);
            {
                var ln = len(ps.inner);

                if (ln > 0L)
                {
                    ps.inner = ps.inner[..ln - 1L];
                    at.printDimension(ps);
                }

            }
        }

        private static void printInner(this ref ArrayType at, ref printState ps)
        {
            at.printDimension(ps);
        }

        // Print the array dimension.
        private static void printDimension(this ref ArrayType at, ref printState ps)
        {
            @string space = " ";
            while (len(ps.inner) > 0L)
            { 
                // We haven't gotten to the real type yet.  Use
                // parentheses around that type, except that if it is
                // an array type we print it as a multi-dimensional
                // array
                var @in = ps.inner[len(ps.inner) - 1L];
                {
                    ref TypeWithQualifiers (twq, ok) = in._<ref TypeWithQualifiers>();

                    if (ok)
                    {
                        in = twq.Base;
                    }

                }
                {
                    ref ArrayType (_, ok) = in._<ref ArrayType>();

                    if (ok)
                    {
                        if (in == ps.inner[len(ps.inner) - 1L])
                        {
                            space = "";
                        }
                        ps.printOneInner(null);
                    }
                    else
                    {
                        ps.writeString(" (");
                        ps.printInner(false);
                        ps.writeByte(')');
                    }

                }
            }

            ps.writeString(space);
            ps.writeByte('[');
            ps.print(at.Dimension);
            ps.writeByte(']');
        }

        private static bool Traverse(this ref ArrayType at, Func<AST, bool> fn)
        {
            if (fn(at))
            {
                at.Dimension.Traverse(fn);
                at.Element.Traverse(fn);
            }
        }

        private static AST Copy(this ref ArrayType at, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(at))
            {
                return null;
            }
            var dimension = at.Dimension.Copy(fn, skip);
            var element = at.Element.Copy(fn, skip);
            if (dimension == null && element == null)
            {
                return fn(at);
            }
            if (dimension == null)
            {
                dimension = at.Dimension;
            }
            if (element == null)
            {
                element = at.Element;
            }
            at = ref new ArrayType(Dimension:dimension,Element:element);
            {
                var r = fn(at);

                if (r != null)
                {
                    return r;
                }

            }
            return at;
        }

        private static @string GoString(this ref ArrayType at)
        {
            return at.goString(0L, "");
        }

        private static @string goString(this ref ArrayType at, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sArrayType:\n%s\n%s", indent, "", field, at.Dimension.goString(indent + 2L, "Dimension: "), at.Element.goString(indent + 2L, "Element: "));
        }

        // FunctionType is a function type.  The Return field may be nil for
        // cases where the return type is not part of the mangled name.
        public partial struct FunctionType
        {
            public AST Return;
            public slice<AST> Args;
        }

        private static void print(this ref FunctionType ft, ref printState ps)
        {
            if (ft.Return != null)
            { 
                // Pass the return type as an inner type in order to
                // print the arguments in the right location.
                ps.inner = append(ps.inner, ft);
                ps.print(ft.Return);
                if (len(ps.inner) == 0L)
                { 
                    // Everything was printed.
                    return;
                }
                ps.inner = ps.inner[..len(ps.inner) - 1L];
                ps.writeByte(' ');
            }
            ft.printArgs(ps);
        }

        private static void printInner(this ref FunctionType ft, ref printState ps)
        {
            ft.printArgs(ps);
        }

        // printArgs prints the arguments of a function type.  It looks at the
        // inner types for spacing.
        private static void printArgs(this ref FunctionType ft, ref printState ps)
        {
            var paren = false;
            var space = false;
            for (var i = len(ps.inner) - 1L; i >= 0L; i--)
            {
                switch (ps.inner[i].type())
                {
                    case ref PointerType _:
                        paren = true;
                        break;
                    case ref ReferenceType _:
                        paren = true;
                        break;
                    case ref RvalueReferenceType _:
                        paren = true;
                        break;
                    case ref TypeWithQualifiers _:
                        space = true;
                        paren = true;
                        break;
                    case ref ComplexType _:
                        space = true;
                        paren = true;
                        break;
                    case ref ImaginaryType _:
                        space = true;
                        paren = true;
                        break;
                    case ref PtrMem _:
                        space = true;
                        paren = true;
                        break;
                }
                if (paren)
                {
                    break;
                }
            }


            if (paren)
            {
                if (!space && (ps.last != '(' && ps.last != '*'))
                {
                    space = true;
                }
                if (space && ps.last != ' ')
                {
                    ps.writeByte(' ');
                }
                ps.writeByte('(');
            }
            var save = ps.printInner(true);

            if (paren)
            {
                ps.writeByte(')');
            }
            ps.writeByte('(');
            var first = true;
            foreach (var (_, a) in ft.Args)
            {
                if (ps.isEmpty(a))
                {
                    continue;
                }
                if (!first)
                {
                    ps.writeString(", ");
                }
                ps.print(a);
                first = false;
            }
            ps.writeByte(')');

            ps.inner = save;
            ps.printInner(false);
        }

        private static bool Traverse(this ref FunctionType ft, Func<AST, bool> fn)
        {
            if (fn(ft))
            {
                if (ft.Return != null)
                {
                    ft.Return.Traverse(fn);
                }
                foreach (var (_, a) in ft.Args)
                {
                    a.Traverse(fn);
                }
            }
        }

        private static AST Copy(this ref FunctionType ft, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(ft))
            {
                return null;
            }
            var changed = false;
            AST ret = default;
            if (ft.Return != null)
            {
                ret = AST.As(ft.Return.Copy(fn, skip));
                if (ret == null)
                {
                    ret = AST.As(ft.Return);
                }
                else
                {
                    changed = true;
                }
            }
            var args = make_slice<AST>(len(ft.Args));
            foreach (var (i, a) in ft.Args)
            {
                var ac = a.Copy(fn, skip);
                if (ac == null)
                {
                    args[i] = a;
                }
                else
                {
                    args[i] = ac;
                    changed = true;
                }
            }
            if (!changed)
            {
                return fn(ft);
            }
            ft = ref new FunctionType(Return:ret,Args:args);
            {
                var r = fn(ft);

                if (r != null)
                {
                    return r;
                }

            }
            return ft;
        }

        private static @string GoString(this ref FunctionType ft)
        {
            return ft.goString(0L, "");
        }

        private static @string goString(this ref FunctionType ft, long indent, @string field)
        {
            @string r = default;
            if (ft.Return == null)
            {
                r = fmt.Sprintf("%*sReturn: nil", indent + 2L, "");
            }
            else
            {
                r = ft.Return.goString(indent + 2L, "Return: ");
            }
            @string args = default;
            if (len(ft.Args) == 0L)
            {
                args = fmt.Sprintf("%*sArgs: nil", indent + 2L, "");
            }
            else
            {
                args = fmt.Sprintf("%*sArgs:", indent + 2L, "");
                foreach (var (i, a) in ft.Args)
                {
                    args += "\n";
                    args += a.goString(indent + 4L, fmt.Sprintf("%d: ", i));
                }
            }
            return fmt.Sprintf("%*s%sFunctionType:\n%s\n%s", indent, "", field, r, args);
        }

        // FunctionParam is a parameter of a function, used for last-specified
        // return type in a closure.
        public partial struct FunctionParam
        {
            public long Index;
        }

        private static void print(this ref FunctionParam fp, ref printState ps)
        {
            if (fp.Index == 0L)
            {
                ps.writeString("this");
            }
            else
            {
                fmt.Fprintf(ref ps.buf, "{parm#%d}", fp.Index);
            }
        }

        private static bool Traverse(this ref FunctionParam fp, Func<AST, bool> fn)
        {
            fn(fp);
        }

        private static AST Copy(this ref FunctionParam fp, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(fp))
            {
                return null;
            }
            return fn(fp);
        }

        private static @string GoString(this ref FunctionParam fp)
        {
            return fp.goString(0L, "");
        }

        private static @string goString(this ref FunctionParam fp, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sFunctionParam: %d", indent, "", field, fp.Index);
        }

        // PtrMem is a pointer-to-member expression.
        public partial struct PtrMem
        {
            public AST Class;
            public AST Member;
        }

        private static void print(this ref PtrMem pm, ref printState ps)
        {
            ps.inner = append(ps.inner, pm);
            ps.print(pm.Member);
            if (len(ps.inner) > 0L)
            {
                ps.printOneInner(null);
            }
        }

        private static void printInner(this ref PtrMem pm, ref printState ps)
        {
            if (ps.last != '(')
            {
                ps.writeByte(' ');
            }
            ps.print(pm.Class);
            ps.writeString("::*");
        }

        private static bool Traverse(this ref PtrMem pm, Func<AST, bool> fn)
        {
            if (fn(pm))
            {
                pm.Class.Traverse(fn);
                pm.Member.Traverse(fn);
            }
        }

        private static AST Copy(this ref PtrMem pm, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(pm))
            {
                return null;
            }
            var @class = pm.Class.Copy(fn, skip);
            var member = pm.Member.Copy(fn, skip);
            if (class == null && member == null)
            {
                return fn(pm);
            }
            if (class == null)
            {
                class = pm.Class;
            }
            if (member == null)
            {
                member = pm.Member;
            }
            pm = ref new PtrMem(Class:class,Member:member);
            {
                var r = fn(pm);

                if (r != null)
                {
                    return r;
                }

            }
            return pm;
        }

        private static @string GoString(this ref PtrMem pm)
        {
            return pm.goString(0L, "");
        }

        private static @string goString(this ref PtrMem pm, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sPtrMem:\n%s\n%s", indent, "", field, pm.Class.goString(indent + 2L, "Class: "), pm.Member.goString(indent + 2L, "Member: "));
        }

        // FixedType is a fixed numeric type of unknown size.
        public partial struct FixedType
        {
            public AST Base;
            public bool Accum;
            public bool Sat;
        }

        private static void print(this ref FixedType ft, ref printState ps)
        {
            if (ft.Sat)
            {
                ps.writeString("_Sat ");
            }
            {
                ref BuiltinType (bt, ok) = ft.Base._<ref BuiltinType>();

                if (ok && bt.Name == "int")
                { 
                    // The standard demangler skips printing "int".
                }
                else
                {
                    ps.print(ft.Base);
                    ps.writeByte(' ');
                }

            }
            if (ft.Accum)
            {
                ps.writeString("_Accum");
            }
            else
            {
                ps.writeString("_Fract");
            }
        }

        private static bool Traverse(this ref FixedType ft, Func<AST, bool> fn)
        {
            if (fn(ft))
            {
                ft.Base.Traverse(fn);
            }
        }

        private static AST Copy(this ref FixedType ft, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(ft))
            {
                return null;
            }
            var @base = ft.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(ft);
            }
            ft = ref new FixedType(Base:base,Accum:ft.Accum,Sat:ft.Sat);
            {
                var r = fn(ft);

                if (r != null)
                {
                    return r;
                }

            }
            return ft;
        }

        private static @string GoString(this ref FixedType ft)
        {
            return ft.goString(0L, "");
        }

        private static @string goString(this ref FixedType ft, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sFixedType: Accum: %t; Sat: %t\n%s", indent, "", field, ft.Accum, ft.Sat, ft.Base.goString(indent + 2L, "Base: "));
        }

        // VectorType is a vector type.
        public partial struct VectorType
        {
            public AST Dimension;
            public AST Base;
        }

        private static void print(this ref VectorType vt, ref printState ps)
        {
            ps.inner = append(ps.inner, vt);
            ps.print(vt.Base);
            if (len(ps.inner) > 0L)
            {
                ps.printOneInner(null);
            }
        }

        private static void printInner(this ref VectorType vt, ref printState ps)
        {
            ps.writeString(" __vector(");
            ps.print(vt.Dimension);
            ps.writeByte(')');
        }

        private static bool Traverse(this ref VectorType vt, Func<AST, bool> fn)
        {
            if (fn(vt))
            {
                vt.Dimension.Traverse(fn);
                vt.Base.Traverse(fn);
            }
        }

        private static AST Copy(this ref VectorType vt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(vt))
            {
                return null;
            }
            var dimension = vt.Dimension.Copy(fn, skip);
            var @base = vt.Base.Copy(fn, skip);
            if (dimension == null && base == null)
            {
                return fn(vt);
            }
            if (dimension == null)
            {
                dimension = vt.Dimension;
            }
            if (base == null)
            {
                base = vt.Base;
            }
            vt = ref new VectorType(Dimension:dimension,Base:base);
            {
                var r = fn(vt);

                if (r != null)
                {
                    return r;
                }

            }
            return vt;
        }

        private static @string GoString(this ref VectorType vt)
        {
            return vt.goString(0L, "");
        }

        private static @string goString(this ref VectorType vt, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sVectorType:\n%s\n%s", indent, "", field, vt.Dimension.goString(indent + 2L, "Dimension: "), vt.Base.goString(indent + 2L, "Base: "));
        }

        // Decltype is the decltype operator.
        public partial struct Decltype
        {
            public AST Expr;
        }

        private static void print(this ref Decltype dt, ref printState ps)
        {
            ps.writeString("decltype (");
            ps.print(dt.Expr);
            ps.writeByte(')');
        }

        private static bool Traverse(this ref Decltype dt, Func<AST, bool> fn)
        {
            if (fn(dt))
            {
                dt.Expr.Traverse(fn);
            }
        }

        private static AST Copy(this ref Decltype dt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(dt))
            {
                return null;
            }
            var expr = dt.Expr.Copy(fn, skip);
            if (expr == null)
            {
                return fn(dt);
            }
            dt = ref new Decltype(Expr:expr);
            {
                var r = fn(dt);

                if (r != null)
                {
                    return r;
                }

            }
            return dt;
        }

        private static @string GoString(this ref Decltype dt)
        {
            return dt.goString(0L, "");
        }

        private static @string goString(this ref Decltype dt, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sDecltype:\n%s", indent, "", field, dt.Expr.goString(indent + 2L, "Expr: "));
        }

        // Operator is an operator.
        public partial struct Operator
        {
            public @string Name;
        }

        private static void print(this ref Operator op, ref printState ps)
        {
            ps.writeString("operator");
            if (isLower(op.Name[0L]))
            {
                ps.writeByte(' ');
            }
            var n = op.Name;
            n = strings.TrimSuffix(n, " ");
            ps.writeString(n);
        }

        private static bool Traverse(this ref Operator op, Func<AST, bool> fn)
        {
            fn(op);
        }

        private static AST Copy(this ref Operator op, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(op))
            {
                return null;
            }
            return fn(op);
        }

        private static @string GoString(this ref Operator op)
        {
            return op.goString(0L, "");
        }

        private static @string goString(this ref Operator op, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sOperator: %s", indent, "", field, op.Name);
        }

        // Constructor is a constructor.
        public partial struct Constructor
        {
            public AST Name;
        }

        private static void print(this ref Constructor c, ref printState ps)
        {
            ps.print(c.Name);
        }

        private static bool Traverse(this ref Constructor c, Func<AST, bool> fn)
        {
            if (fn(c))
            {
                c.Name.Traverse(fn);
            }
        }

        private static AST Copy(this ref Constructor c, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(c))
            {
                return null;
            }
            var name = c.Name.Copy(fn, skip);
            if (name == null)
            {
                return fn(c);
            }
            c = ref new Constructor(Name:name);
            {
                var r = fn(c);

                if (r != null)
                {
                    return r;
                }

            }
            return c;
        }

        private static @string GoString(this ref Constructor c)
        {
            return c.goString(0L, "");
        }

        private static @string goString(this ref Constructor c, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sConstructor:\n%s", indent, "", field, c.Name.goString(indent + 2L, "Name: "));
        }

        // Destructor is a destructor.
        public partial struct Destructor
        {
            public AST Name;
        }

        private static void print(this ref Destructor d, ref printState ps)
        {
            ps.writeByte('~');
            ps.print(d.Name);
        }

        private static bool Traverse(this ref Destructor d, Func<AST, bool> fn)
        {
            if (fn(d))
            {
                d.Name.Traverse(fn);
            }
        }

        private static AST Copy(this ref Destructor d, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(d))
            {
                return null;
            }
            var name = d.Name.Copy(fn, skip);
            if (name == null)
            {
                return fn(d);
            }
            d = ref new Destructor(Name:name);
            {
                var r = fn(d);

                if (r != null)
                {
                    return r;
                }

            }
            return d;
        }

        private static @string GoString(this ref Destructor d)
        {
            return d.goString(0L, "");
        }

        private static @string goString(this ref Destructor d, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sDestructor:\n%s", indent, "", field, d.Name.goString(indent + 2L, "Name: "));
        }

        // GlobalCDtor is a global constructor or destructor.
        public partial struct GlobalCDtor
        {
            public bool Ctor;
            public AST Key;
        }

        private static void print(this ref GlobalCDtor gcd, ref printState ps)
        {
            ps.writeString("global ");
            if (gcd.Ctor)
            {
                ps.writeString("constructors");
            }
            else
            {
                ps.writeString("destructors");
            }
            ps.writeString(" keyed to ");
            ps.print(gcd.Key);
        }

        private static bool Traverse(this ref GlobalCDtor gcd, Func<AST, bool> fn)
        {
            if (fn(gcd))
            {
                gcd.Key.Traverse(fn);
            }
        }

        private static AST Copy(this ref GlobalCDtor gcd, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(gcd))
            {
                return null;
            }
            var key = gcd.Key.Copy(fn, skip);
            if (key == null)
            {
                return fn(gcd);
            }
            gcd = ref new GlobalCDtor(Ctor:gcd.Ctor,Key:key);
            {
                var r = fn(gcd);

                if (r != null)
                {
                    return r;
                }

            }
            return gcd;
        }

        private static @string GoString(this ref GlobalCDtor gcd)
        {
            return gcd.goString(0L, "");
        }

        private static @string goString(this ref GlobalCDtor gcd, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sGlobalCDtor: Ctor: %t\n%s", indent, "", field, gcd.Ctor, gcd.Key.goString(indent + 2L, "Key: "));
        }

        // TaggedName is a name with an ABI tag.
        public partial struct TaggedName
        {
            public AST Name;
            public AST Tag;
        }

        private static void print(this ref TaggedName t, ref printState ps)
        {
            ps.print(t.Name);
            ps.writeString("[abi:");
            ps.print(t.Tag);
            ps.writeByte(']');
        }

        private static bool Traverse(this ref TaggedName t, Func<AST, bool> fn)
        {
            if (fn(t))
            {
                t.Name.Traverse(fn);
                t.Tag.Traverse(fn);
            }
        }

        private static AST Copy(this ref TaggedName t, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(t))
            {
                return null;
            }
            var name = t.Name.Copy(fn, skip);
            var tag = t.Tag.Copy(fn, skip);
            if (name == null && tag == null)
            {
                return fn(t);
            }
            if (name == null)
            {
                name = t.Name;
            }
            if (tag == null)
            {
                tag = t.Tag;
            }
            t = ref new TaggedName(Name:name,Tag:tag);
            {
                var r = fn(t);

                if (r != null)
                {
                    return r;
                }

            }
            return t;
        }

        private static @string GoString(this ref TaggedName t)
        {
            return t.goString(0L, "");
        }

        private static @string goString(this ref TaggedName t, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sTaggedName:\n%s\n%s", indent, "", field, t.Name.goString(indent + 2L, "Name: "), t.Tag.goString(indent + 2L, "Tag: "));
        }

        // PackExpansion is a pack expansion.  The Pack field may be nil.
        public partial struct PackExpansion
        {
            public AST Base;
            public ptr<ArgumentPack> Pack;
        }

        private static void print(this ref PackExpansion pe, ref printState ps)
        { 
            // We normally only get here if the simplify function was
            // unable to locate and expand the pack.
            if (pe.Pack == null)
            {
                parenthesize(ps, pe.Base);
                ps.writeString("...");
            }
            else
            {
                ps.print(pe.Base);
            }
        }

        private static bool Traverse(this ref PackExpansion pe, Func<AST, bool> fn)
        {
            if (fn(pe))
            {
                pe.Base.Traverse(fn); 
                // Don't traverse Template--it points elsewhere in the AST.
            }
        }

        private static AST Copy(this ref PackExpansion pe, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(pe))
            {
                return null;
            }
            var @base = pe.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(pe);
            }
            pe = ref new PackExpansion(Base:base,Pack:pe.Pack);
            {
                var r = fn(pe);

                if (r != null)
                {
                    return r;
                }

            }
            return pe;
        }

        private static @string GoString(this ref PackExpansion pe)
        {
            return pe.goString(0L, "");
        }

        private static @string goString(this ref PackExpansion pe, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sPackExpansion: Pack: %p\n%s", indent, "", field, pe.Pack, pe.Base.goString(indent + 2L, "Base: "));
        }

        // ArgumentPack is an argument pack.
        public partial struct ArgumentPack
        {
            public slice<AST> Args;
        }

        private static void print(this ref ArgumentPack ap, ref printState ps)
        {
            foreach (var (i, a) in ap.Args)
            {
                if (i > 0L)
                {
                    ps.writeString(", ");
                }
                ps.print(a);
            }
        }

        private static bool Traverse(this ref ArgumentPack ap, Func<AST, bool> fn)
        {
            if (fn(ap))
            {
                foreach (var (_, a) in ap.Args)
                {
                    a.Traverse(fn);
                }
            }
        }

        private static AST Copy(this ref ArgumentPack ap, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(ap))
            {
                return null;
            }
            var args = make_slice<AST>(len(ap.Args));
            var changed = false;
            foreach (var (i, a) in ap.Args)
            {
                var ac = a.Copy(fn, skip);
                if (ac == null)
                {
                    args[i] = a;
                }
                else
                {
                    args[i] = ac;
                    changed = true;
                }
            }
            if (!changed)
            {
                return fn(ap);
            }
            ap = ref new ArgumentPack(Args:args);
            {
                var r = fn(ap);

                if (r != null)
                {
                    return r;
                }

            }
            return ap;
        }

        private static @string GoString(this ref ArgumentPack ap)
        {
            return ap.goString(0L, "");
        }

        private static @string goString(this ref ArgumentPack ap, long indent, @string field)
        {
            if (len(ap.Args) == 0L)
            {
                return fmt.Sprintf("%*s%sArgumentPack: nil", indent, "", field);
            }
            var s = fmt.Sprintf("%*s%sArgumentPack:", indent, "", field);
            foreach (var (i, a) in ap.Args)
            {
                s += "\n";
                s += a.goString(indent + 2L, fmt.Sprintf("%d: ", i));
            }
            return s;
        }

        // SizeofPack is the sizeof operator applied to an argument pack.
        public partial struct SizeofPack
        {
            public ptr<ArgumentPack> Pack;
        }

        private static void print(this ref SizeofPack sp, ref printState ps)
        {
            ps.writeString(fmt.Sprintf("%d", len(sp.Pack.Args)));
        }

        private static bool Traverse(this ref SizeofPack sp, Func<AST, bool> fn)
        {
            fn(sp); 
            // Don't traverse the pack--it points elsewhere in the AST.
        }

        private static AST Copy(this ref SizeofPack sp, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(sp))
            {
                return null;
            }
            sp = ref new SizeofPack(Pack:sp.Pack);
            {
                var r = fn(sp);

                if (r != null)
                {
                    return r;
                }

            }
            return sp;
        }

        private static @string GoString(this ref SizeofPack sp)
        {
            return sp.goString(0L, "");
        }

        private static @string goString(this ref SizeofPack sp, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sSizeofPack: Pack: %p", indent, "", field, sp.Pack);
        }

        // SizeofArgs is the size of a captured template parameter pack from
        // an alias template.
        public partial struct SizeofArgs
        {
            public slice<AST> Args;
        }

        private static void print(this ref SizeofArgs sa, ref printState ps)
        {
            long c = 0L;
            foreach (var (_, a) in sa.Args)
            {
                {
                    ref ArgumentPack (ap, ok) = a._<ref ArgumentPack>();

                    if (ok)
                    {
                        c += len(ap.Args);
                    }                    {
                        ref ExprList (el, ok) = a._<ref ExprList>();


                        else if (ok)
                        {
                            c += len(el.Exprs);
                        }
                        else
                        {
                            c++;
                        }

                    }

                }
            }
            ps.writeString(fmt.Sprintf("%d", c));
        }

        private static bool Traverse(this ref SizeofArgs sa, Func<AST, bool> fn)
        {
            if (fn(sa))
            {
                foreach (var (_, a) in sa.Args)
                {
                    a.Traverse(fn);
                }
            }
        }

        private static AST Copy(this ref SizeofArgs sa, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(sa))
            {
                return null;
            }
            var changed = false;
            var args = make_slice<AST>(len(sa.Args));
            foreach (var (i, a) in sa.Args)
            {
                var ac = a.Copy(fn, skip);
                if (ac == null)
                {
                    args[i] = a;
                }
                else
                {
                    args[i] = ac;
                    changed = true;
                }
            }
            if (!changed)
            {
                return fn(sa);
            }
            sa = ref new SizeofArgs(Args:args);
            {
                var r = fn(sa);

                if (r != null)
                {
                    return r;
                }

            }
            return sa;
        }

        private static @string GoString(this ref SizeofArgs sa)
        {
            return sa.goString(0L, "");
        }

        private static @string goString(this ref SizeofArgs sa, long indent, @string field)
        {
            @string args = default;
            if (len(sa.Args) == 0L)
            {
                args = fmt.Sprintf("%*sArgs: nil", indent + 2L, "");
            }
            else
            {
                args = fmt.Sprintf("%*sArgs:", indent + 2L, "");
                foreach (var (i, a) in sa.Args)
                {
                    args += "\n";
                    args += a.goString(indent + 4L, fmt.Sprintf("%d: ", i));
                }
            }
            return fmt.Sprintf("%*s%sSizeofArgs:\n%s", indent, "", field, args);
        }

        // Cast is a type cast.
        public partial struct Cast
        {
            public AST To;
        }

        private static void print(this ref Cast c, ref printState ps)
        {
            ps.writeString("operator ");
            ps.print(c.To);
        }

        private static bool Traverse(this ref Cast c, Func<AST, bool> fn)
        {
            if (fn(c))
            {
                c.To.Traverse(fn);
            }
        }

        private static AST Copy(this ref Cast c, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(c))
            {
                return null;
            }
            var to = c.To.Copy(fn, skip);
            if (to == null)
            {
                return fn(c);
            }
            c = ref new Cast(To:to);
            {
                var r = fn(c);

                if (r != null)
                {
                    return r;
                }

            }
            return c;
        }

        private static @string GoString(this ref Cast c)
        {
            return c.goString(0L, "");
        }

        private static @string goString(this ref Cast c, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sCast\n%s", indent, "", field, c.To.goString(indent + 2L, "To: "));
        }

        // The parenthesize function prints the string for val, wrapped in
        // parentheses if necessary.
        private static void parenthesize(ref printState ps, AST val)
        {
            var paren = false;
            switch (val.type())
            {
                case ref Name v:
                    break;
                case ref InitializerList v:
                    break;
                case ref FunctionParam v:
                    break;
                case ref Qualified v:
                    if (v.LocalName)
                    {
                        paren = true;
                    }
                    break;
                default:
                {
                    var v = val.type();
                    paren = true;
                    break;
                }
            }
            if (paren)
            {
                ps.writeByte('(');
            }
            ps.print(val);
            if (paren)
            {
                ps.writeByte(')');
            }
        }

        // Nullary is an operator in an expression with no arguments, such as
        // throw.
        public partial struct Nullary
        {
            public AST Op;
        }

        private static void print(this ref Nullary n, ref printState ps)
        {
            {
                ref Operator (op, ok) = n.Op._<ref Operator>();

                if (ok)
                {
                    ps.writeString(op.Name);
                }
                else
                {
                    ps.print(n.Op);
                }

            }
        }

        private static bool Traverse(this ref Nullary n, Func<AST, bool> fn)
        {
            if (fn(n))
            {
                n.Op.Traverse(fn);
            }
        }

        private static AST Copy(this ref Nullary n, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(n))
            {
                return null;
            }
            var op = n.Op.Copy(fn, skip);
            if (op == null)
            {
                return fn(n);
            }
            n = ref new Nullary(Op:op);
            {
                var r = fn(n);

                if (r != null)
                {
                    return r;
                }

            }
            return n;
        }

        private static @string GoString(this ref Nullary n)
        {
            return n.goString(0L, "");
        }

        private static @string goString(this ref Nullary n, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sNullary:\n%s", indent, "", field, n.Op.goString(indent + 2L, "Op: "));
        }

        // Unary is a unary operation in an expression.
        public partial struct Unary
        {
            public AST Op;
            public AST Expr;
            public bool Suffix; // true for ++ -- when used as postfix
            public bool SizeofType; // true for sizeof (type)
        }

        private static void print(this ref Unary u, ref printState ps)
        {
            var expr = u.Expr; 

            // Don't print the argument list when taking the address of a
            // function.
            {
                ref Operator op__prev1 = op;

                ref Operator (op, ok) = u.Op._<ref Operator>();

                if (ok && op.Name == "&")
                {
                    {
                        ref Typed (t, ok) = expr._<ref Typed>();

                        if (ok)
                        {
                            {
                                ref FunctionType (_, ok) = t.Type._<ref FunctionType>();

                                if (ok)
                                {
                                    expr = t.Name;
                                }

                            }
                        }

                    }
                }

                op = op__prev1;

            }

            if (u.Suffix)
            {
                parenthesize(ps, expr);
            }
            {
                ref Operator op__prev1 = op;

                (op, ok) = u.Op._<ref Operator>();

                if (ok)
                {
                    ps.writeString(op.Name);
                }                {
                    ref Cast (c, ok) = u.Op._<ref Cast>();


                    else if (ok)
                    {
                        ps.writeByte('(');
                        ps.print(c.To);
                        ps.writeByte(')');
                    }
                    else
                    {
                        ps.print(u.Op);
                    }

                }


                op = op__prev1;

            }

            if (!u.Suffix)
            {
                {
                    ref Operator op__prev2 = op;

                    (op, ok) = u.Op._<ref Operator>();

                    if (ok && op.Name == "::")
                    { 
                        // Don't use parentheses after ::.
                        ps.print(expr);
                    }
                    else if (u.SizeofType)
                    { 
                        // Always use parentheses for sizeof argument.
                        ps.writeByte('(');
                        ps.print(expr);
                        ps.writeByte(')');
                    }
                    else
                    {
                        parenthesize(ps, expr);
                    }

                    op = op__prev2;

                }
            }
        }

        private static bool Traverse(this ref Unary u, Func<AST, bool> fn)
        {
            if (fn(u))
            {
                u.Op.Traverse(fn);
                u.Expr.Traverse(fn);
            }
        }

        private static AST Copy(this ref Unary u, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(u))
            {
                return null;
            }
            var op = u.Op.Copy(fn, skip);
            var expr = u.Expr.Copy(fn, skip);
            if (op == null && expr == null)
            {
                return fn(u);
            }
            if (op == null)
            {
                op = u.Op;
            }
            if (expr == null)
            {
                expr = u.Expr;
            }
            u = ref new Unary(Op:op,Expr:expr,Suffix:u.Suffix,SizeofType:u.SizeofType);
            {
                var r = fn(u);

                if (r != null)
                {
                    return r;
                }

            }
            return u;
        }

        private static @string GoString(this ref Unary u)
        {
            return u.goString(0L, "");
        }

        private static @string goString(this ref Unary u, long indent, @string field)
        {
            @string s = default;
            if (u.Suffix)
            {
                s = " Suffix: true";
            }
            if (u.SizeofType)
            {
                s += " SizeofType: true";
            }
            return fmt.Sprintf("%*s%sUnary:%s\n%s\n%s", indent, "", field, s, u.Op.goString(indent + 2L, "Op: "), u.Expr.goString(indent + 2L, "Expr: "));
        }

        // Binary is a binary operation in an expression.
        public partial struct Binary
        {
            public AST Op;
            public AST Left;
            public AST Right;
        }

        private static void print(this ref Binary b, ref printState ps)
        {
            ref Operator (op, _) = b.Op._<ref Operator>();

            if (op != null && strings.Contains(op.Name, "cast"))
            {
                ps.writeString(op.Name);
                ps.writeByte('<');
                ps.print(b.Left);
                ps.writeString(">(");
                ps.print(b.Right);
                ps.writeByte(')');
                return;
            } 

            // Use an extra set of parentheses around an expression that
            // uses the greater-than operator, so that it does not get
            // confused with the '>' that ends template parameters.
            if (op != null && op.Name == ">")
            {
                ps.writeByte('(');
            }
            var left = b.Left; 

            // A function call in an expression should not print the types
            // of the arguments.
            if (op != null && op.Name == "()")
            {
                {
                    ref Typed (ty, ok) = b.Left._<ref Typed>();

                    if (ok)
                    {
                        left = ty.Name;
                    }

                }
            }
            parenthesize(ps, left);

            if (op != null && op.Name == "[]")
            {
                ps.writeByte('[');
                ps.print(b.Right);
                ps.writeByte(']');
                return;
            }
            if (op != null)
            {
                if (op.Name != "()")
                {
                    ps.writeString(op.Name);
                }
            }
            else
            {
                ps.print(b.Op);
            }
            parenthesize(ps, b.Right);

            if (op != null && op.Name == ">")
            {
                ps.writeByte(')');
            }
        }

        private static bool Traverse(this ref Binary b, Func<AST, bool> fn)
        {
            if (fn(b))
            {
                b.Op.Traverse(fn);
                b.Left.Traverse(fn);
                b.Right.Traverse(fn);
            }
        }

        private static AST Copy(this ref Binary b, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(b))
            {
                return null;
            }
            var op = b.Op.Copy(fn, skip);
            var left = b.Left.Copy(fn, skip);
            var right = b.Right.Copy(fn, skip);
            if (op == null && left == null && right == null)
            {
                return fn(b);
            }
            if (op == null)
            {
                op = b.Op;
            }
            if (left == null)
            {
                left = b.Left;
            }
            if (right == null)
            {
                right = b.Right;
            }
            b = ref new Binary(Op:op,Left:left,Right:right);
            {
                var r = fn(b);

                if (r != null)
                {
                    return r;
                }

            }
            return b;
        }

        private static @string GoString(this ref Binary b)
        {
            return b.goString(0L, "");
        }

        private static @string goString(this ref Binary b, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sBinary:\n%s\n%s\n%s", indent, "", field, b.Op.goString(indent + 2L, "Op: "), b.Left.goString(indent + 2L, "Left: "), b.Right.goString(indent + 2L, "Right: "));
        }

        // Trinary is the ?: trinary operation in an expression.
        public partial struct Trinary
        {
            public AST Op;
            public AST First;
            public AST Second;
            public AST Third;
        }

        private static void print(this ref Trinary t, ref printState ps)
        {
            parenthesize(ps, t.First);
            ps.writeByte('?');
            parenthesize(ps, t.Second);
            ps.writeString(" : ");
            parenthesize(ps, t.Third);
        }

        private static bool Traverse(this ref Trinary t, Func<AST, bool> fn)
        {
            if (fn(t))
            {
                t.Op.Traverse(fn);
                t.First.Traverse(fn);
                t.Second.Traverse(fn);
                t.Third.Traverse(fn);
            }
        }

        private static AST Copy(this ref Trinary t, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(t))
            {
                return null;
            }
            var op = t.Op.Copy(fn, skip);
            var first = t.First.Copy(fn, skip);
            var second = t.Second.Copy(fn, skip);
            var third = t.Third.Copy(fn, skip);
            if (op == null && first == null && second == null && third == null)
            {
                return fn(t);
            }
            if (op == null)
            {
                op = t.Op;
            }
            if (first == null)
            {
                first = t.First;
            }
            if (second == null)
            {
                second = t.Second;
            }
            if (third == null)
            {
                third = t.Third;
            }
            t = ref new Trinary(Op:op,First:first,Second:second,Third:third);
            {
                var r = fn(t);

                if (r != null)
                {
                    return r;
                }

            }
            return t;
        }

        private static @string GoString(this ref Trinary t)
        {
            return t.goString(0L, "");
        }

        private static @string goString(this ref Trinary t, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sTrinary:\n%s\n%s\n%s\n%s", indent, "", field, t.Op.goString(indent + 2L, "Op: "), t.First.goString(indent + 2L, "First: "), t.Second.goString(indent + 2L, "Second: "), t.Third.goString(indent + 2L, "Third: "));
        }

        // Fold is a C++17 fold-expression.  Arg2 is nil for a unary operator.
        public partial struct Fold
        {
            public bool Left;
            public AST Op;
            public AST Arg1;
            public AST Arg2;
        }

        private static void print(this ref Fold f, ref printState ps)
        {
            ref Operator (op, _) = f.Op._<ref Operator>();
            Action printOp = () =>
            {
                if (op != null)
                {
                    ps.writeString(op.Name);
                }
                else
                {
                    ps.print(f.Op);
                }
            }
;

            if (f.Arg2 == null)
            {
                if (f.Left)
                {
                    ps.writeString("(...");
                    printOp();
                    parenthesize(ps, f.Arg1);
                    ps.writeString(")");
                }
                else
                {
                    ps.writeString("(");
                    parenthesize(ps, f.Arg1);
                    printOp();
                    ps.writeString("...)");
                }
            }
            else
            {
                ps.writeString("(");
                parenthesize(ps, f.Arg1);
                printOp();
                ps.writeString("...");
                printOp();
                parenthesize(ps, f.Arg2);
                ps.writeString(")");
            }
        }

        private static bool Traverse(this ref Fold f, Func<AST, bool> fn)
        {
            if (fn(f))
            {
                f.Op.Traverse(fn);
                f.Arg1.Traverse(fn);
                if (f.Arg2 != null)
                {
                    f.Arg2.Traverse(fn);
                }
            }
        }

        private static AST Copy(this ref Fold f, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(f))
            {
                return null;
            }
            var op = f.Op.Copy(fn, skip);
            var arg1 = f.Arg1.Copy(fn, skip);
            AST arg2 = default;
            if (f.Arg2 != null)
            {
                arg2 = AST.As(f.Arg2.Copy(fn, skip));
            }
            if (op == null && arg1 == null && arg2 == null)
            {
                return fn(f);
            }
            if (op == null)
            {
                op = f.Op;
            }
            if (arg1 == null)
            {
                arg1 = f.Arg1;
            }
            if (arg2 == null)
            {
                arg2 = AST.As(f.Arg2);
            }
            f = ref new Fold(Left:f.Left,Op:op,Arg1:arg1,Arg2:arg2);
            {
                var r = fn(f);

                if (r != null)
                {
                    return r;
                }

            }
            return f;
        }

        private static @string GoString(this ref Fold f)
        {
            return f.goString(0L, "");
        }

        private static @string goString(this ref Fold f, long indent, @string field)
        {
            if (f.Arg2 == null)
            {
                return fmt.Sprintf("%*s%sFold: Left: %t\n%s\n%s", indent, "", field, f.Left, f.Op.goString(indent + 2L, "Op: "), f.Arg1.goString(indent + 2L, "Arg1: "));
            }
            else
            {
                return fmt.Sprintf("%*s%sFold: Left: %t\n%s\n%s\n%s", indent, "", field, f.Left, f.Op.goString(indent + 2L, "Op: "), f.Arg1.goString(indent + 2L, "Arg1: "), f.Arg2.goString(indent + 2L, "Arg2: "));
            }
        }

        // New is a use of operator new in an expression.
        public partial struct New
        {
            public AST Op;
            public AST Place;
            public AST Type;
            public AST Init;
        }

        private static void print(this ref New n, ref printState ps)
        { 
            // Op doesn't really matter for printing--we always print "new".
            ps.writeString("new ");
            if (n.Place != null)
            {
                parenthesize(ps, n.Place);
                ps.writeByte(' ');
            }
            ps.print(n.Type);
            if (n.Init != null)
            {
                parenthesize(ps, n.Init);
            }
        }

        private static bool Traverse(this ref New n, Func<AST, bool> fn)
        {
            if (fn(n))
            {
                n.Op.Traverse(fn);
                if (n.Place != null)
                {
                    n.Place.Traverse(fn);
                }
                n.Type.Traverse(fn);
                if (n.Init != null)
                {
                    n.Init.Traverse(fn);
                }
            }
        }

        private static AST Copy(this ref New n, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(n))
            {
                return null;
            }
            var op = n.Op.Copy(fn, skip);
            AST place = default;
            if (n.Place != null)
            {
                place = AST.As(n.Place.Copy(fn, skip));
            }
            var typ = n.Type.Copy(fn, skip);
            AST ini = default;
            if (n.Init != null)
            {
                ini = AST.As(n.Init.Copy(fn, skip));
            }
            if (op == null && place == null && typ == null && ini == null)
            {
                return fn(n);
            }
            if (op == null)
            {
                op = n.Op;
            }
            if (place == null)
            {
                place = AST.As(n.Place);
            }
            if (typ == null)
            {
                typ = n.Type;
            }
            if (ini == null)
            {
                ini = AST.As(n.Init);
            }
            n = ref new New(Op:op,Place:place,Type:typ,Init:ini);
            {
                var r = fn(n);

                if (r != null)
                {
                    return r;
                }

            }
            return n;
        }

        private static @string GoString(this ref New n)
        {
            return n.goString(0L, "");
        }

        private static @string goString(this ref New n, long indent, @string field)
        {
            @string place = default;
            if (n.Place == null)
            {
                place = fmt.Sprintf("%*sPlace: nil", indent, "");
            }
            else
            {
                place = n.Place.goString(indent + 2L, "Place: ");
            }
            @string ini = default;
            if (n.Init == null)
            {
                ini = fmt.Sprintf("%*sInit: nil", indent, "");
            }
            else
            {
                ini = n.Init.goString(indent + 2L, "Init: ");
            }
            return fmt.Sprintf("%*s%sNew:\n%s\n%s\n%s\n%s", indent, "", field, n.Op.goString(indent + 2L, "Op: "), place, n.Type.goString(indent + 2L, "Type: "), ini);
        }

        // Literal is a literal in an expression.
        public partial struct Literal
        {
            public AST Type;
            public @string Val;
            public bool Neg;
        }

        // Suffixes to use for constants of the given integer type.
        private static map builtinTypeSuffix = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"int":"","unsigned int":"u","long":"l","unsigned long":"ul","long long":"ll","unsigned long long":"ull",};

        // Builtin float types.
        private static map builtinTypeFloat = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"double":true,"long double":true,"float":true,"__float128":true,"half":true,};

        private static void print(this ref Literal l, ref printState ps)
        {
            var isFloat = false;
            {
                ref BuiltinType (b, ok) = l.Type._<ref BuiltinType>();

                if (ok)
                {
                    {
                        var (suffix, ok) = builtinTypeSuffix[b.Name];

                        if (ok)
                        {
                            if (l.Neg)
                            {
                                ps.writeByte('-');
                            }
                            ps.writeString(l.Val);
                            ps.writeString(suffix);
                            return;
                        }
                        else if (b.Name == "bool" && !l.Neg)
                        {
                            switch (l.Val)
                            {
                                case "0": 
                                    ps.writeString("false");
                                    return;
                                    break;
                                case "1": 
                                    ps.writeString("true");
                                    return;
                                    break;
                            }
                        }
                        else
                        {
                            isFloat = builtinTypeFloat[b.Name];
                        }

                    }
                }

            }

            ps.writeByte('(');
            ps.print(l.Type);
            ps.writeByte(')');

            if (isFloat)
            {
                ps.writeByte('[');
            }
            if (l.Neg)
            {
                ps.writeByte('-');
            }
            ps.writeString(l.Val);
            if (isFloat)
            {
                ps.writeByte(']');
            }
        }

        private static bool Traverse(this ref Literal l, Func<AST, bool> fn)
        {
            if (fn(l))
            {
                l.Type.Traverse(fn);
            }
        }

        private static AST Copy(this ref Literal l, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(l))
            {
                return null;
            }
            var typ = l.Type.Copy(fn, skip);
            if (typ == null)
            {
                return fn(l);
            }
            l = ref new Literal(Type:typ,Val:l.Val,Neg:l.Neg);
            {
                var r = fn(l);

                if (r != null)
                {
                    return r;
                }

            }
            return l;
        }

        private static @string GoString(this ref Literal l)
        {
            return l.goString(0L, "");
        }

        private static @string goString(this ref Literal l, long indent, @string field)
        {
            @string neg = default;
            if (l.Neg)
            {
                neg = " Neg: true";
            }
            return fmt.Sprintf("%*s%sLiteral:%s\n%s\n%*sVal: %s", indent, "", field, neg, l.Type.goString(indent + 2L, "Type: "), indent + 2L, "", l.Val);
        }

        // ExprList is a list of expressions, typically arguments to a
        // function call in an expression.
        public partial struct ExprList
        {
            public slice<AST> Exprs;
        }

        private static void print(this ref ExprList el, ref printState ps)
        {
            foreach (var (i, e) in el.Exprs)
            {
                if (i > 0L)
                {
                    ps.writeString(", ");
                }
                ps.print(e);
            }
        }

        private static bool Traverse(this ref ExprList el, Func<AST, bool> fn)
        {
            if (fn(el))
            {
                foreach (var (_, e) in el.Exprs)
                {
                    e.Traverse(fn);
                }
            }
        }

        private static AST Copy(this ref ExprList el, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(el))
            {
                return null;
            }
            var exprs = make_slice<AST>(len(el.Exprs));
            var changed = false;
            foreach (var (i, e) in el.Exprs)
            {
                var ec = e.Copy(fn, skip);
                if (ec == null)
                {
                    exprs[i] = e;
                }
                else
                {
                    exprs[i] = ec;
                    changed = true;
                }
            }
            if (!changed)
            {
                return fn(el);
            }
            el = ref new ExprList(Exprs:exprs);
            {
                var r = fn(el);

                if (r != null)
                {
                    return r;
                }

            }
            return el;
        }

        private static @string GoString(this ref ExprList el)
        {
            return el.goString(0L, "");
        }

        private static @string goString(this ref ExprList el, long indent, @string field)
        {
            if (len(el.Exprs) == 0L)
            {
                return fmt.Sprintf("%*s%sExprList: nil", indent, "", field);
            }
            var s = fmt.Sprintf("%*s%sExprList:", indent, "", field);
            foreach (var (i, e) in el.Exprs)
            {
                s += "\n";
                s += e.goString(indent + 2L, fmt.Sprintf("%d: ", i));
            }
            return s;
        }

        // InitializerList is an initializer list: an optional type with a
        // list of expressions.
        public partial struct InitializerList
        {
            public AST Type;
            public AST Exprs;
        }

        private static void print(this ref InitializerList il, ref printState ps)
        {
            if (il.Type != null)
            {
                ps.print(il.Type);
            }
            ps.writeByte('{');
            ps.print(il.Exprs);
            ps.writeByte('}');
        }

        private static bool Traverse(this ref InitializerList il, Func<AST, bool> fn)
        {
            if (fn(il))
            {
                if (il.Type != null)
                {
                    il.Type.Traverse(fn);
                }
                il.Exprs.Traverse(fn);
            }
        }

        private static AST Copy(this ref InitializerList il, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(il))
            {
                return null;
            }
            AST typ = default;
            if (il.Type != null)
            {
                typ = AST.As(il.Type.Copy(fn, skip));
            }
            var exprs = il.Exprs.Copy(fn, skip);
            if (typ == null && exprs == null)
            {
                return fn(il);
            }
            if (typ == null)
            {
                typ = AST.As(il.Type);
            }
            if (exprs == null)
            {
                exprs = il.Exprs;
            }
            il = ref new InitializerList(Type:typ,Exprs:exprs);
            {
                var r = fn(il);

                if (r != null)
                {
                    return r;
                }

            }
            return il;
        }

        private static @string GoString(this ref InitializerList il)
        {
            return il.goString(0L, "");
        }

        private static @string goString(this ref InitializerList il, long indent, @string field)
        {
            @string t = default;
            if (il.Type == null)
            {
                t = fmt.Sprintf("%*sType: nil", indent + 2L, "");
            }
            else
            {
                t = il.Type.goString(indent + 2L, "Type: ");
            }
            return fmt.Sprintf("%*s%sInitializerList:\n%s\n%s", indent, "", field, t, il.Exprs.goString(indent + 2L, "Exprs: "));
        }

        // DefaultArg holds a default argument for a local name.
        public partial struct DefaultArg
        {
            public long Num;
            public AST Arg;
        }

        private static void print(this ref DefaultArg da, ref printState ps)
        {
            fmt.Fprintf(ref ps.buf, "{default arg#%d}::", da.Num + 1L);
            ps.print(da.Arg);
        }

        private static bool Traverse(this ref DefaultArg da, Func<AST, bool> fn)
        {
            if (fn(da))
            {
                da.Arg.Traverse(fn);
            }
        }

        private static AST Copy(this ref DefaultArg da, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(da))
            {
                return null;
            }
            var arg = da.Arg.Copy(fn, skip);
            if (arg == null)
            {
                return fn(da);
            }
            da = ref new DefaultArg(Num:da.Num,Arg:arg);
            {
                var r = fn(da);

                if (r != null)
                {
                    return r;
                }

            }
            return da;
        }

        private static @string GoString(this ref DefaultArg da)
        {
            return da.goString(0L, "");
        }

        private static @string goString(this ref DefaultArg da, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sDefaultArg: Num: %d\n%s", indent, "", field, da.Num, da.Arg.goString(indent + 2L, "Arg: "));
        }

        // Closure is a closure, or lambda expression.
        public partial struct Closure
        {
            public slice<AST> Types;
            public long Num;
        }

        private static void print(this ref Closure cl, ref printState ps)
        {
            ps.writeString("{lambda(");
            foreach (var (i, t) in cl.Types)
            {
                if (i > 0L)
                {
                    ps.writeString(", ");
                }
                ps.print(t);
            }
            ps.writeString(fmt.Sprintf(")#%d}", cl.Num + 1L));
        }

        private static bool Traverse(this ref Closure cl, Func<AST, bool> fn)
        {
            if (fn(cl))
            {
                foreach (var (_, t) in cl.Types)
                {
                    t.Traverse(fn);
                }
            }
        }

        private static AST Copy(this ref Closure cl, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(cl))
            {
                return null;
            }
            var types = make_slice<AST>(len(cl.Types));
            var changed = false;
            foreach (var (i, t) in cl.Types)
            {
                var tc = t.Copy(fn, skip);
                if (tc == null)
                {
                    types[i] = t;
                }
                else
                {
                    types[i] = tc;
                    changed = true;
                }
            }
            if (!changed)
            {
                return fn(cl);
            }
            cl = ref new Closure(Types:types,Num:cl.Num);
            {
                var r = fn(cl);

                if (r != null)
                {
                    return r;
                }

            }
            return cl;
        }

        private static @string GoString(this ref Closure cl)
        {
            return cl.goString(0L, "");
        }

        private static @string goString(this ref Closure cl, long indent, @string field)
        {
            @string types = default;
            if (len(cl.Types) == 0L)
            {
                types = fmt.Sprintf("%*sTypes: nil", indent + 2L, "");
            }
            else
            {
                types = fmt.Sprintf("%*sTypes:", indent + 2L, "");
                foreach (var (i, t) in cl.Types)
                {
                    types += "\n";
                    types += t.goString(indent + 4L, fmt.Sprintf("%d: ", i));
                }
            }
            return fmt.Sprintf("%*s%sClosure: Num: %d\n%s", indent, "", field, cl.Num, types);
        }

        // UnnamedType is an unnamed type, that just has an index.
        public partial struct UnnamedType
        {
            public long Num;
        }

        private static void print(this ref UnnamedType ut, ref printState ps)
        {
            ps.writeString(fmt.Sprintf("{unnamed type#%d}", ut.Num + 1L));
        }

        private static bool Traverse(this ref UnnamedType ut, Func<AST, bool> fn)
        {
            fn(ut);
        }

        private static AST Copy(this ref UnnamedType ut, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(ut))
            {
                return null;
            }
            return fn(ut);
        }

        private static @string GoString(this ref UnnamedType ut)
        {
            return ut.goString(0L, "");
        }

        private static @string goString(this ref UnnamedType ut, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sUnnamedType: Num: %d", indent, "", field, ut.Num);
        }

        // Clone is a clone of a function, with a distinguishing suffix.
        public partial struct Clone
        {
            public AST Base;
            public @string Suffix;
        }

        private static void print(this ref Clone c, ref printState ps)
        {
            ps.print(c.Base);
            ps.writeString(fmt.Sprintf(" [clone %s]", c.Suffix));
        }

        private static bool Traverse(this ref Clone c, Func<AST, bool> fn)
        {
            if (fn(c))
            {
                c.Base.Traverse(fn);
            }
        }

        private static AST Copy(this ref Clone c, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(c))
            {
                return null;
            }
            var @base = c.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(c);
            }
            c = ref new Clone(Base:base,Suffix:c.Suffix);
            {
                var r = fn(c);

                if (r != null)
                {
                    return r;
                }

            }
            return c;
        }

        private static @string GoString(this ref Clone c)
        {
            return c.goString(0L, "");
        }

        private static @string goString(this ref Clone c, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sClone: Suffix: %s\n%s", indent, "", field, c.Suffix, c.Base.goString(indent + 2L, "Base: "));
        }

        // Special is a special symbol, printed as a prefix plus another
        // value.
        public partial struct Special
        {
            public @string Prefix;
            public AST Val;
        }

        private static void print(this ref Special s, ref printState ps)
        {
            ps.writeString(s.Prefix);
            ps.print(s.Val);
        }

        private static bool Traverse(this ref Special s, Func<AST, bool> fn)
        {
            if (fn(s))
            {
                s.Val.Traverse(fn);
            }
        }

        private static AST Copy(this ref Special s, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(s))
            {
                return null;
            }
            var val = s.Val.Copy(fn, skip);
            if (val == null)
            {
                return fn(s);
            }
            s = ref new Special(Prefix:s.Prefix,Val:val);
            {
                var r = fn(s);

                if (r != null)
                {
                    return r;
                }

            }
            return s;
        }

        private static @string GoString(this ref Special s)
        {
            return s.goString(0L, "");
        }

        private static @string goString(this ref Special s, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sSpecial: Prefix: %s\n%s", indent, "", field, s.Prefix, s.Val.goString(indent + 2L, "Val: "));
        }

        // Special2 is like special, but uses two values.
        public partial struct Special2
        {
            public @string Prefix;
            public AST Val1;
            public @string Middle;
            public AST Val2;
        }

        private static void print(this ref Special2 s, ref printState ps)
        {
            ps.writeString(s.Prefix);
            ps.print(s.Val1);
            ps.writeString(s.Middle);
            ps.print(s.Val2);
        }

        private static bool Traverse(this ref Special2 s, Func<AST, bool> fn)
        {
            if (fn(s))
            {
                s.Val1.Traverse(fn);
                s.Val2.Traverse(fn);
            }
        }

        private static AST Copy(this ref Special2 s, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            if (skip(s))
            {
                return null;
            }
            var val1 = s.Val1.Copy(fn, skip);
            var val2 = s.Val2.Copy(fn, skip);
            if (val1 == null && val2 == null)
            {
                return fn(s);
            }
            if (val1 == null)
            {
                val1 = s.Val1;
            }
            if (val2 == null)
            {
                val2 = s.Val2;
            }
            s = ref new Special2(Prefix:s.Prefix,Val1:val1,Middle:s.Middle,Val2:val2);
            {
                var r = fn(s);

                if (r != null)
                {
                    return r;
                }

            }
            return s;
        }

        private static @string GoString(this ref Special2 s)
        {
            return s.goString(0L, "");
        }

        private static @string goString(this ref Special2 s, long indent, @string field)
        {
            return fmt.Sprintf("%*s%sSpecial2: Prefix: %s\n%s\n%*sMiddle: %s\n%s", indent, "", field, s.Prefix, s.Val1.goString(indent + 2L, "Val1: "), indent + 2L, "", s.Middle, s.Val2.goString(indent + 2L, "Val2: "));
        }

        // Print the inner types.
        private static slice<AST> printInner(this ref printState ps, bool prefixOnly)
        {
            slice<AST> save = default;
            ref slice<AST> psave = default;
            if (prefixOnly)
            {
                psave = ref save;
            }
            while (len(ps.inner) > 0L)
            {
                ps.printOneInner(psave);
            }

            return save;
        }

        // innerPrinter is an interface for types that can print themselves as
        // inner types.
        private partial interface innerPrinter
        {
            void printInner(ref printState _p0);
        }

        // Print the most recent inner type.  If save is not nil, only print
        // prefixes.
        private static void printOneInner(this ref printState _ps, ref slice<AST> _save) => func(_ps, _save, (ref printState ps, ref slice<AST> save, Defer _, Panic panic, Recover __) =>
        {
            if (len(ps.inner) == 0L)
            {
                panic("printOneInner called with no inner types");
            }
            var ln = len(ps.inner);
            var a = ps.inner[ln - 1L];
            ps.inner = ps.inner[..ln - 1L];

            if (save != null)
            {
                {
                    ref MethodWithQualifiers (_, ok) = a._<ref MethodWithQualifiers>();

                    if (ok)
                    {
                        save.Value = append(save.Value, a);
                        return;
                    }

                }
            }
            {
                innerPrinter (ip, ok) = a._<innerPrinter>();

                if (ok)
                {
                    ip.printInner(ps);
                }
                else
                {
                    ps.print(a);
                }

            }
        });

        // isEmpty returns whether printing a will not print anything.
        private static bool isEmpty(this ref printState ps, AST a)
        {
            switch (a.type())
            {
                case ref ArgumentPack a:
                    return len(a.Args) == 0L;
                    break;
                case ref ExprList a:
                    return len(a.Exprs) == 0L;
                    break;
                case ref PackExpansion a:
                    return a.Pack != null && ps.isEmpty(a.Base);
                    break;
                default:
                {
                    var a = a.type();
                    return false;
                    break;
                }
            }
        }
    }
}}}}}
