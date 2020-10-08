// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package demangle -- go2cs converted at 2020 October 08 04:43:51 UTC
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
            @string print(ptr<printState> _p0); // Traverse each element of an AST.  If the function returns
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
            ref printState ps = ref heap(new printState(tparams:tparams), out ptr<printState> _addr_ps);
            a.print(_addr_ps);
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
        private static void writeByte(this ptr<printState> _addr_ps, byte b)
        {
            ref printState ps = ref _addr_ps.val;

            ps.last = b;
            ps.buf.WriteByte(b);
        }

        // writeString adds a string to the string being printed.
        private static void writeString(this ptr<printState> _addr_ps, @string s)
        {
            ref printState ps = ref _addr_ps.val;

            if (len(s) > 0L)
            {
                ps.last = s[len(s) - 1L];
            }

            ps.buf.WriteString(s);

        }

        // Print an AST.
        private static void print(this ptr<printState> _addr_ps, AST a)
        {
            ref printState ps = ref _addr_ps.val;

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
                        return ;
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

        private static void print(this ptr<Name> _addr_n, ptr<printState> _addr_ps)
        {
            ref Name n = ref _addr_n.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString(n.Name);
        }

        private static bool Traverse(this ptr<Name> _addr_n, Func<AST, bool> fn)
        {
            ref Name n = ref _addr_n.val;

            fn(n);
        }

        private static AST Copy(this ptr<Name> _addr_n, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Name n = ref _addr_n.val;

            if (skip(n))
            {
                return null;
            }

            return fn(n);

        }

        private static @string GoString(this ptr<Name> _addr_n)
        {
            ref Name n = ref _addr_n.val;

            return n.goString(0L, "Name: ");
        }

        private static @string goString(this ptr<Name> _addr_n, long indent, @string field)
        {
            ref Name n = ref _addr_n.val;

            return fmt.Sprintf("%*s%s%s", indent, "", field, n.Name);
        }

        // Typed is a typed name.
        public partial struct Typed
        {
            public AST Name;
            public AST Type;
        }

        private static void print(this ptr<Typed> _addr_t, ptr<printState> _addr_ps) => func((defer, _, __) =>
        {
            ref Typed t = ref _addr_t.val;
            ref printState ps = ref _addr_ps.val;
 
            // We are printing a typed name, so ignore the current set of
            // inner names to print.  Pass down our name as the one to use.
            var holdInner = ps.inner;
            defer(() =>
            {
                ps.inner = holdInner;
            }());

            ps.inner = new slice<AST>(new AST[] { AST.As(t)! });
            ps.print(t.Type);
            if (len(ps.inner) > 0L)
            { 
                // The type did not print the name; print it now in
                // the default location.
                ps.writeByte(' ');
                ps.print(t.Name);

            }

        });

        private static void printInner(this ptr<Typed> _addr_t, ptr<printState> _addr_ps)
        {
            ref Typed t = ref _addr_t.val;
            ref printState ps = ref _addr_ps.val;

            ps.print(t.Name);
        }

        private static bool Traverse(this ptr<Typed> _addr_t, Func<AST, bool> fn)
        {
            ref Typed t = ref _addr_t.val;

            if (fn(t))
            {
                t.Name.Traverse(fn);
                t.Type.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Typed> _addr_t, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Typed t = ref _addr_t.val;

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

            t = addr(new Typed(Name:name,Type:typ));
            {
                var r = fn(t);

                if (r != null)
                {
                    return r;
                }

            }

            return t;

        }

        private static @string GoString(this ptr<Typed> _addr_t)
        {
            ref Typed t = ref _addr_t.val;

            return t.goString(0L, "");
        }

        private static @string goString(this ptr<Typed> _addr_t, long indent, @string field)
        {
            ref Typed t = ref _addr_t.val;

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

        private static void print(this ptr<Qualified> _addr_q, ptr<printState> _addr_ps)
        {
            ref Qualified q = ref _addr_q.val;
            ref printState ps = ref _addr_ps.val;

            ps.print(q.Scope);
            ps.writeString("::");
            ps.print(q.Name);
        }

        private static bool Traverse(this ptr<Qualified> _addr_q, Func<AST, bool> fn)
        {
            ref Qualified q = ref _addr_q.val;

            if (fn(q))
            {
                q.Scope.Traverse(fn);
                q.Name.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Qualified> _addr_q, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Qualified q = ref _addr_q.val;

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

            q = addr(new Qualified(Scope:scope,Name:name,LocalName:q.LocalName));
            {
                var r = fn(q);

                if (r != null)
                {
                    return r;
                }

            }

            return q;

        }

        private static @string GoString(this ptr<Qualified> _addr_q)
        {
            ref Qualified q = ref _addr_q.val;

            return q.goString(0L, "");
        }

        private static @string goString(this ptr<Qualified> _addr_q, long indent, @string field)
        {
            ref Qualified q = ref _addr_q.val;

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

        private static void print(this ptr<Template> _addr_t, ptr<printState> _addr_ps) => func((defer, _, __) =>
        {
            ref Template t = ref _addr_t.val;
            ref printState ps = ref _addr_ps.val;
 
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
                return ;

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

        private static bool Traverse(this ptr<Template> _addr_t, Func<AST, bool> fn)
        {
            ref Template t = ref _addr_t.val;

            if (fn(t))
            {
                t.Name.Traverse(fn);
                foreach (var (_, a) in t.Args)
                {
                    a.Traverse(fn);
                }

            }

        }

        private static AST Copy(this ptr<Template> _addr_t, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Template t = ref _addr_t.val;

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

            t = addr(new Template(Name:name,Args:args));
            {
                var r = fn(t);

                if (r != null)
                {
                    return r;
                }

            }

            return t;

        }

        private static @string GoString(this ptr<Template> _addr_t)
        {
            ref Template t = ref _addr_t.val;

            return t.goString(0L, "");
        }

        private static @string goString(this ptr<Template> _addr_t, long indent, @string field)
        {
            ref Template t = ref _addr_t.val;

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

        private static void print(this ptr<TemplateParam> _addr_tp, ptr<printState> _addr_ps) => func((_, panic, __) =>
        {
            ref TemplateParam tp = ref _addr_tp.val;
            ref printState ps = ref _addr_ps.val;

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

        private static bool Traverse(this ptr<TemplateParam> _addr_tp, Func<AST, bool> fn)
        {
            ref TemplateParam tp = ref _addr_tp.val;

            fn(tp); 
            // Don't traverse Template--it points elsewhere in the AST.
        }

        private static AST Copy(this ptr<TemplateParam> _addr_tp, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref TemplateParam tp = ref _addr_tp.val;

            if (skip(tp))
            {
                return null;
            }

            return fn(tp);

        }

        private static @string GoString(this ptr<TemplateParam> _addr_tp)
        {
            ref TemplateParam tp = ref _addr_tp.val;

            return tp.goString(0L, "");
        }

        private static @string goString(this ptr<TemplateParam> _addr_tp, long indent, @string field)
        {
            ref TemplateParam tp = ref _addr_tp.val;

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

        private static void print(this ptr<TypeWithQualifiers> _addr_twq, ptr<printState> _addr_ps)
        {
            ref TypeWithQualifiers twq = ref _addr_twq.val;
            ref printState ps = ref _addr_ps.val;
 
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
        private static void printInner(this ptr<TypeWithQualifiers> _addr_twq, ptr<printState> _addr_ps)
        {
            ref TypeWithQualifiers twq = ref _addr_twq.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeByte(' ');
            ps.writeString(strings.Join(twq.Qualifiers, " "));
        }

        private static bool Traverse(this ptr<TypeWithQualifiers> _addr_twq, Func<AST, bool> fn)
        {
            ref TypeWithQualifiers twq = ref _addr_twq.val;

            if (fn(twq))
            {
                twq.Base.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<TypeWithQualifiers> _addr_twq, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref TypeWithQualifiers twq = ref _addr_twq.val;

            if (skip(twq))
            {
                return null;
            }

            var @base = twq.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(twq);
            }

            twq = addr(new TypeWithQualifiers(Base:base,Qualifiers:twq.Qualifiers));
            {
                var r = fn(twq);

                if (r != null)
                {
                    return r;
                }

            }

            return twq;

        }

        private static @string GoString(this ptr<TypeWithQualifiers> _addr_twq)
        {
            ref TypeWithQualifiers twq = ref _addr_twq.val;

            return twq.goString(0L, "");
        }

        private static @string goString(this ptr<TypeWithQualifiers> _addr_twq, long indent, @string field)
        {
            ref TypeWithQualifiers twq = ref _addr_twq.val;

            return fmt.Sprintf("%*s%sTypeWithQualifiers: Qualifiers: %s\n%s", indent, "", field, twq.Qualifiers, twq.Base.goString(indent + 2L, "Base: "));
        }

        // MethodWithQualifiers is a method with qualifiers.
        public partial struct MethodWithQualifiers
        {
            public AST Method;
            public Qualifiers Qualifiers;
            public @string RefQualifier; // "" or "&" or "&&"
        }

        private static void print(this ptr<MethodWithQualifiers> _addr_mwq, ptr<printState> _addr_ps)
        {
            ref MethodWithQualifiers mwq = ref _addr_mwq.val;
            ref printState ps = ref _addr_ps.val;
 
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

        private static void printInner(this ptr<MethodWithQualifiers> _addr_mwq, ptr<printState> _addr_ps)
        {
            ref MethodWithQualifiers mwq = ref _addr_mwq.val;
            ref printState ps = ref _addr_ps.val;

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

        private static bool Traverse(this ptr<MethodWithQualifiers> _addr_mwq, Func<AST, bool> fn)
        {
            ref MethodWithQualifiers mwq = ref _addr_mwq.val;

            if (fn(mwq))
            {
                mwq.Method.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<MethodWithQualifiers> _addr_mwq, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref MethodWithQualifiers mwq = ref _addr_mwq.val;

            if (skip(mwq))
            {
                return null;
            }

            var method = mwq.Method.Copy(fn, skip);
            if (method == null)
            {
                return fn(mwq);
            }

            mwq = addr(new MethodWithQualifiers(Method:method,Qualifiers:mwq.Qualifiers,RefQualifier:mwq.RefQualifier));
            {
                var r = fn(mwq);

                if (r != null)
                {
                    return r;
                }

            }

            return mwq;

        }

        private static @string GoString(this ptr<MethodWithQualifiers> _addr_mwq)
        {
            ref MethodWithQualifiers mwq = ref _addr_mwq.val;

            return mwq.goString(0L, "");
        }

        private static @string goString(this ptr<MethodWithQualifiers> _addr_mwq, long indent, @string field)
        {
            ref MethodWithQualifiers mwq = ref _addr_mwq.val;

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

        private static void print(this ptr<BuiltinType> _addr_bt, ptr<printState> _addr_ps)
        {
            ref BuiltinType bt = ref _addr_bt.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString(bt.Name);
        }

        private static bool Traverse(this ptr<BuiltinType> _addr_bt, Func<AST, bool> fn)
        {
            ref BuiltinType bt = ref _addr_bt.val;

            fn(bt);
        }

        private static AST Copy(this ptr<BuiltinType> _addr_bt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref BuiltinType bt = ref _addr_bt.val;

            if (skip(bt))
            {
                return null;
            }

            return fn(bt);

        }

        private static @string GoString(this ptr<BuiltinType> _addr_bt)
        {
            ref BuiltinType bt = ref _addr_bt.val;

            return bt.goString(0L, "");
        }

        private static @string goString(this ptr<BuiltinType> _addr_bt, long indent, @string field)
        {
            ref BuiltinType bt = ref _addr_bt.val;

            return fmt.Sprintf("%*s%sBuiltinType: %s", indent, "", field, bt.Name);
        }

        // printBase is common print code for types that are printed with a
        // simple suffix.
        private static void printBase(ptr<printState> _addr_ps, AST qual, AST @base)
        {
            ref printState ps = ref _addr_ps.val;

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

        private static void print(this ptr<PointerType> _addr_pt, ptr<printState> _addr_ps)
        {
            ref PointerType pt = ref _addr_pt.val;
            ref printState ps = ref _addr_ps.val;

            printBase(_addr_ps, pt, pt.Base);
        }

        private static void printInner(this ptr<PointerType> _addr_pt, ptr<printState> _addr_ps)
        {
            ref PointerType pt = ref _addr_pt.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString("*");
        }

        private static bool Traverse(this ptr<PointerType> _addr_pt, Func<AST, bool> fn)
        {
            ref PointerType pt = ref _addr_pt.val;

            if (fn(pt))
            {
                pt.Base.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<PointerType> _addr_pt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref PointerType pt = ref _addr_pt.val;

            if (skip(pt))
            {
                return null;
            }

            var @base = pt.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(pt);
            }

            pt = addr(new PointerType(Base:base));
            {
                var r = fn(pt);

                if (r != null)
                {
                    return r;
                }

            }

            return pt;

        }

        private static @string GoString(this ptr<PointerType> _addr_pt)
        {
            ref PointerType pt = ref _addr_pt.val;

            return pt.goString(0L, "");
        }

        private static @string goString(this ptr<PointerType> _addr_pt, long indent, @string field)
        {
            ref PointerType pt = ref _addr_pt.val;

            return fmt.Sprintf("%*s%sPointerType:\n%s", indent, "", field, pt.Base.goString(indent + 2L, ""));
        }

        // ReferenceType is a reference type.
        public partial struct ReferenceType
        {
            public AST Base;
        }

        private static void print(this ptr<ReferenceType> _addr_rt, ptr<printState> _addr_ps)
        {
            ref ReferenceType rt = ref _addr_rt.val;
            ref printState ps = ref _addr_ps.val;

            printBase(_addr_ps, rt, rt.Base);
        }

        private static void printInner(this ptr<ReferenceType> _addr_rt, ptr<printState> _addr_ps)
        {
            ref ReferenceType rt = ref _addr_rt.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString("&");
        }

        private static bool Traverse(this ptr<ReferenceType> _addr_rt, Func<AST, bool> fn)
        {
            ref ReferenceType rt = ref _addr_rt.val;

            if (fn(rt))
            {
                rt.Base.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<ReferenceType> _addr_rt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref ReferenceType rt = ref _addr_rt.val;

            if (skip(rt))
            {
                return null;
            }

            var @base = rt.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(rt);
            }

            rt = addr(new ReferenceType(Base:base));
            {
                var r = fn(rt);

                if (r != null)
                {
                    return r;
                }

            }

            return rt;

        }

        private static @string GoString(this ptr<ReferenceType> _addr_rt)
        {
            ref ReferenceType rt = ref _addr_rt.val;

            return rt.goString(0L, "");
        }

        private static @string goString(this ptr<ReferenceType> _addr_rt, long indent, @string field)
        {
            ref ReferenceType rt = ref _addr_rt.val;

            return fmt.Sprintf("%*s%sReferenceType:\n%s", indent, "", field, rt.Base.goString(indent + 2L, ""));
        }

        // RvalueReferenceType is an rvalue reference type.
        public partial struct RvalueReferenceType
        {
            public AST Base;
        }

        private static void print(this ptr<RvalueReferenceType> _addr_rt, ptr<printState> _addr_ps)
        {
            ref RvalueReferenceType rt = ref _addr_rt.val;
            ref printState ps = ref _addr_ps.val;

            printBase(_addr_ps, rt, rt.Base);
        }

        private static void printInner(this ptr<RvalueReferenceType> _addr_rt, ptr<printState> _addr_ps)
        {
            ref RvalueReferenceType rt = ref _addr_rt.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString("&&");
        }

        private static bool Traverse(this ptr<RvalueReferenceType> _addr_rt, Func<AST, bool> fn)
        {
            ref RvalueReferenceType rt = ref _addr_rt.val;

            if (fn(rt))
            {
                rt.Base.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<RvalueReferenceType> _addr_rt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref RvalueReferenceType rt = ref _addr_rt.val;

            if (skip(rt))
            {
                return null;
            }

            var @base = rt.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(rt);
            }

            rt = addr(new RvalueReferenceType(Base:base));
            {
                var r = fn(rt);

                if (r != null)
                {
                    return r;
                }

            }

            return rt;

        }

        private static @string GoString(this ptr<RvalueReferenceType> _addr_rt)
        {
            ref RvalueReferenceType rt = ref _addr_rt.val;

            return rt.goString(0L, "");
        }

        private static @string goString(this ptr<RvalueReferenceType> _addr_rt, long indent, @string field)
        {
            ref RvalueReferenceType rt = ref _addr_rt.val;

            return fmt.Sprintf("%*s%sRvalueReferenceType:\n%s", indent, "", field, rt.Base.goString(indent + 2L, ""));
        }

        // ComplexType is a complex type.
        public partial struct ComplexType
        {
            public AST Base;
        }

        private static void print(this ptr<ComplexType> _addr_ct, ptr<printState> _addr_ps)
        {
            ref ComplexType ct = ref _addr_ct.val;
            ref printState ps = ref _addr_ps.val;

            printBase(_addr_ps, ct, ct.Base);
        }

        private static void printInner(this ptr<ComplexType> _addr_ct, ptr<printState> _addr_ps)
        {
            ref ComplexType ct = ref _addr_ct.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString(" _Complex");
        }

        private static bool Traverse(this ptr<ComplexType> _addr_ct, Func<AST, bool> fn)
        {
            ref ComplexType ct = ref _addr_ct.val;

            if (fn(ct))
            {
                ct.Base.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<ComplexType> _addr_ct, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref ComplexType ct = ref _addr_ct.val;

            if (skip(ct))
            {
                return null;
            }

            var @base = ct.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(ct);
            }

            ct = addr(new ComplexType(Base:base));
            {
                var r = fn(ct);

                if (r != null)
                {
                    return r;
                }

            }

            return ct;

        }

        private static @string GoString(this ptr<ComplexType> _addr_ct)
        {
            ref ComplexType ct = ref _addr_ct.val;

            return ct.goString(0L, "");
        }

        private static @string goString(this ptr<ComplexType> _addr_ct, long indent, @string field)
        {
            ref ComplexType ct = ref _addr_ct.val;

            return fmt.Sprintf("%*s%sComplexType:\n%s", indent, "", field, ct.Base.goString(indent + 2L, ""));
        }

        // ImaginaryType is an imaginary type.
        public partial struct ImaginaryType
        {
            public AST Base;
        }

        private static void print(this ptr<ImaginaryType> _addr_it, ptr<printState> _addr_ps)
        {
            ref ImaginaryType it = ref _addr_it.val;
            ref printState ps = ref _addr_ps.val;

            printBase(_addr_ps, it, it.Base);
        }

        private static void printInner(this ptr<ImaginaryType> _addr_it, ptr<printState> _addr_ps)
        {
            ref ImaginaryType it = ref _addr_it.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString(" _Imaginary");
        }

        private static bool Traverse(this ptr<ImaginaryType> _addr_it, Func<AST, bool> fn)
        {
            ref ImaginaryType it = ref _addr_it.val;

            if (fn(it))
            {
                it.Base.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<ImaginaryType> _addr_it, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref ImaginaryType it = ref _addr_it.val;

            if (skip(it))
            {
                return null;
            }

            var @base = it.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(it);
            }

            it = addr(new ImaginaryType(Base:base));
            {
                var r = fn(it);

                if (r != null)
                {
                    return r;
                }

            }

            return it;

        }

        private static @string GoString(this ptr<ImaginaryType> _addr_it)
        {
            ref ImaginaryType it = ref _addr_it.val;

            return it.goString(0L, "");
        }

        private static @string goString(this ptr<ImaginaryType> _addr_it, long indent, @string field)
        {
            ref ImaginaryType it = ref _addr_it.val;

            return fmt.Sprintf("%*s%sImaginaryType:\n%s", indent, "", field, it.Base.goString(indent + 2L, ""));
        }

        // VendorQualifier is a type qualified by a vendor-specific qualifier.
        public partial struct VendorQualifier
        {
            public AST Qualifier;
            public AST Type;
        }

        private static void print(this ptr<VendorQualifier> _addr_vq, ptr<printState> _addr_ps)
        {
            ref VendorQualifier vq = ref _addr_vq.val;
            ref printState ps = ref _addr_ps.val;

            ps.inner = append(ps.inner, vq);
            ps.print(vq.Type);
            if (len(ps.inner) > 0L)
            {
                ps.printOneInner(null);
            }

        }

        private static void printInner(this ptr<VendorQualifier> _addr_vq, ptr<printState> _addr_ps)
        {
            ref VendorQualifier vq = ref _addr_vq.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeByte(' ');
            ps.print(vq.Qualifier);
        }

        private static bool Traverse(this ptr<VendorQualifier> _addr_vq, Func<AST, bool> fn)
        {
            ref VendorQualifier vq = ref _addr_vq.val;

            if (fn(vq))
            {
                vq.Qualifier.Traverse(fn);
                vq.Type.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<VendorQualifier> _addr_vq, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref VendorQualifier vq = ref _addr_vq.val;

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

            vq = addr(new VendorQualifier(Qualifier:qualifier,Type:vq.Type));
            {
                var r = fn(vq);

                if (r != null)
                {
                    return r;
                }

            }

            return vq;

        }

        private static @string GoString(this ptr<VendorQualifier> _addr_vq)
        {
            ref VendorQualifier vq = ref _addr_vq.val;

            return vq.goString(0L, "");
        }

        private static @string goString(this ptr<VendorQualifier> _addr_vq, long indent, @string field)
        {
            ref VendorQualifier vq = ref _addr_vq.val;

            return fmt.Sprintf("%*s%sVendorQualifier:\n%s\n%s", indent, "", field, vq.Qualifier.goString(indent + 2L, "Qualifier: "), vq.Type.goString(indent + 2L, "Type: "));
        }

        // ArrayType is an array type.
        public partial struct ArrayType
        {
            public AST Dimension;
            public AST Element;
        }

        private static void print(this ptr<ArrayType> _addr_at, ptr<printState> _addr_ps)
        {
            ref ArrayType at = ref _addr_at.val;
            ref printState ps = ref _addr_ps.val;
 
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

        private static void printInner(this ptr<ArrayType> _addr_at, ptr<printState> _addr_ps)
        {
            ref ArrayType at = ref _addr_at.val;
            ref printState ps = ref _addr_ps.val;

            at.printDimension(ps);
        }

        // Print the array dimension.
        private static void printDimension(this ptr<ArrayType> _addr_at, ptr<printState> _addr_ps)
        {
            ref ArrayType at = ref _addr_at.val;
            ref printState ps = ref _addr_ps.val;

            @string space = " ";
            while (len(ps.inner) > 0L)
            { 
                // We haven't gotten to the real type yet.  Use
                // parentheses around that type, except that if it is
                // an array type we print it as a multi-dimensional
                // array
                var @in = ps.inner[len(ps.inner) - 1L];
                {
                    ptr<TypeWithQualifiers> (twq, ok) = in._<ptr<TypeWithQualifiers>>();

                    if (ok)
                    {
                        in = twq.Base;
                    }

                }

                {
                    ptr<ArrayType> (_, ok) = in._<ptr<ArrayType>>();

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

        private static bool Traverse(this ptr<ArrayType> _addr_at, Func<AST, bool> fn)
        {
            ref ArrayType at = ref _addr_at.val;

            if (fn(at))
            {
                at.Dimension.Traverse(fn);
                at.Element.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<ArrayType> _addr_at, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref ArrayType at = ref _addr_at.val;

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

            at = addr(new ArrayType(Dimension:dimension,Element:element));
            {
                var r = fn(at);

                if (r != null)
                {
                    return r;
                }

            }

            return at;

        }

        private static @string GoString(this ptr<ArrayType> _addr_at)
        {
            ref ArrayType at = ref _addr_at.val;

            return at.goString(0L, "");
        }

        private static @string goString(this ptr<ArrayType> _addr_at, long indent, @string field)
        {
            ref ArrayType at = ref _addr_at.val;

            return fmt.Sprintf("%*s%sArrayType:\n%s\n%s", indent, "", field, at.Dimension.goString(indent + 2L, "Dimension: "), at.Element.goString(indent + 2L, "Element: "));
        }

        // FunctionType is a function type.  The Return field may be nil for
        // cases where the return type is not part of the mangled name.
        public partial struct FunctionType
        {
            public AST Return;
            public slice<AST> Args;
        }

        private static void print(this ptr<FunctionType> _addr_ft, ptr<printState> _addr_ps)
        {
            ref FunctionType ft = ref _addr_ft.val;
            ref printState ps = ref _addr_ps.val;

            if (ft.Return != null)
            { 
                // Pass the return type as an inner type in order to
                // print the arguments in the right location.
                ps.inner = append(ps.inner, ft);
                ps.print(ft.Return);
                if (len(ps.inner) == 0L)
                { 
                    // Everything was printed.
                    return ;

                }

                ps.inner = ps.inner[..len(ps.inner) - 1L];
                ps.writeByte(' ');

            }

            ft.printArgs(ps);

        }

        private static void printInner(this ptr<FunctionType> _addr_ft, ptr<printState> _addr_ps)
        {
            ref FunctionType ft = ref _addr_ft.val;
            ref printState ps = ref _addr_ps.val;

            ft.printArgs(ps);
        }

        // printArgs prints the arguments of a function type.  It looks at the
        // inner types for spacing.
        private static void printArgs(this ptr<FunctionType> _addr_ft, ptr<printState> _addr_ps)
        {
            ref FunctionType ft = ref _addr_ft.val;
            ref printState ps = ref _addr_ps.val;

            var paren = false;
            var space = false;
            for (var i = len(ps.inner) - 1L; i >= 0L; i--)
            {
                switch (ps.inner[i].type())
                {
                    case ptr<PointerType> _:
                        paren = true;
                        break;
                    case ptr<ReferenceType> _:
                        paren = true;
                        break;
                    case ptr<RvalueReferenceType> _:
                        paren = true;
                        break;
                    case ptr<TypeWithQualifiers> _:
                        space = true;
                        paren = true;
                        break;
                    case ptr<ComplexType> _:
                        space = true;
                        paren = true;
                        break;
                    case ptr<ImaginaryType> _:
                        space = true;
                        paren = true;
                        break;
                    case ptr<PtrMem> _:
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

        private static bool Traverse(this ptr<FunctionType> _addr_ft, Func<AST, bool> fn)
        {
            ref FunctionType ft = ref _addr_ft.val;

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

        private static AST Copy(this ptr<FunctionType> _addr_ft, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref FunctionType ft = ref _addr_ft.val;

            if (skip(ft))
            {
                return null;
            }

            var changed = false;
            AST ret = default!;
            if (ft.Return != null)
            {
                ret = AST.As(ft.Return.Copy(fn, skip))!;
                if (ret == null)
                {
                    ret = AST.As(ft.Return)!;
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

            ft = addr(new FunctionType(Return:ret,Args:args));
            {
                var r = fn(ft);

                if (r != null)
                {
                    return r;
                }

            }

            return ft;

        }

        private static @string GoString(this ptr<FunctionType> _addr_ft)
        {
            ref FunctionType ft = ref _addr_ft.val;

            return ft.goString(0L, "");
        }

        private static @string goString(this ptr<FunctionType> _addr_ft, long indent, @string field)
        {
            ref FunctionType ft = ref _addr_ft.val;

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

        private static void print(this ptr<FunctionParam> _addr_fp, ptr<printState> _addr_ps)
        {
            ref FunctionParam fp = ref _addr_fp.val;
            ref printState ps = ref _addr_ps.val;

            if (fp.Index == 0L)
            {
                ps.writeString("this");
            }
            else
            {
                fmt.Fprintf(_addr_ps.buf, "{parm#%d}", fp.Index);
            }

        }

        private static bool Traverse(this ptr<FunctionParam> _addr_fp, Func<AST, bool> fn)
        {
            ref FunctionParam fp = ref _addr_fp.val;

            fn(fp);
        }

        private static AST Copy(this ptr<FunctionParam> _addr_fp, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref FunctionParam fp = ref _addr_fp.val;

            if (skip(fp))
            {
                return null;
            }

            return fn(fp);

        }

        private static @string GoString(this ptr<FunctionParam> _addr_fp)
        {
            ref FunctionParam fp = ref _addr_fp.val;

            return fp.goString(0L, "");
        }

        private static @string goString(this ptr<FunctionParam> _addr_fp, long indent, @string field)
        {
            ref FunctionParam fp = ref _addr_fp.val;

            return fmt.Sprintf("%*s%sFunctionParam: %d", indent, "", field, fp.Index);
        }

        // PtrMem is a pointer-to-member expression.
        public partial struct PtrMem
        {
            public AST Class;
            public AST Member;
        }

        private static void print(this ptr<PtrMem> _addr_pm, ptr<printState> _addr_ps)
        {
            ref PtrMem pm = ref _addr_pm.val;
            ref printState ps = ref _addr_ps.val;

            ps.inner = append(ps.inner, pm);
            ps.print(pm.Member);
            if (len(ps.inner) > 0L)
            {
                ps.printOneInner(null);
            }

        }

        private static void printInner(this ptr<PtrMem> _addr_pm, ptr<printState> _addr_ps)
        {
            ref PtrMem pm = ref _addr_pm.val;
            ref printState ps = ref _addr_ps.val;

            if (ps.last != '(')
            {
                ps.writeByte(' ');
            }

            ps.print(pm.Class);
            ps.writeString("::*");

        }

        private static bool Traverse(this ptr<PtrMem> _addr_pm, Func<AST, bool> fn)
        {
            ref PtrMem pm = ref _addr_pm.val;

            if (fn(pm))
            {
                pm.Class.Traverse(fn);
                pm.Member.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<PtrMem> _addr_pm, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref PtrMem pm = ref _addr_pm.val;

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

            pm = addr(new PtrMem(Class:class,Member:member));
            {
                var r = fn(pm);

                if (r != null)
                {
                    return r;
                }

            }

            return pm;

        }

        private static @string GoString(this ptr<PtrMem> _addr_pm)
        {
            ref PtrMem pm = ref _addr_pm.val;

            return pm.goString(0L, "");
        }

        private static @string goString(this ptr<PtrMem> _addr_pm, long indent, @string field)
        {
            ref PtrMem pm = ref _addr_pm.val;

            return fmt.Sprintf("%*s%sPtrMem:\n%s\n%s", indent, "", field, pm.Class.goString(indent + 2L, "Class: "), pm.Member.goString(indent + 2L, "Member: "));
        }

        // FixedType is a fixed numeric type of unknown size.
        public partial struct FixedType
        {
            public AST Base;
            public bool Accum;
            public bool Sat;
        }

        private static void print(this ptr<FixedType> _addr_ft, ptr<printState> _addr_ps)
        {
            ref FixedType ft = ref _addr_ft.val;
            ref printState ps = ref _addr_ps.val;

            if (ft.Sat)
            {
                ps.writeString("_Sat ");
            }

            {
                ptr<BuiltinType> (bt, ok) = ft.Base._<ptr<BuiltinType>>();

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

        private static bool Traverse(this ptr<FixedType> _addr_ft, Func<AST, bool> fn)
        {
            ref FixedType ft = ref _addr_ft.val;

            if (fn(ft))
            {
                ft.Base.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<FixedType> _addr_ft, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref FixedType ft = ref _addr_ft.val;

            if (skip(ft))
            {
                return null;
            }

            var @base = ft.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(ft);
            }

            ft = addr(new FixedType(Base:base,Accum:ft.Accum,Sat:ft.Sat));
            {
                var r = fn(ft);

                if (r != null)
                {
                    return r;
                }

            }

            return ft;

        }

        private static @string GoString(this ptr<FixedType> _addr_ft)
        {
            ref FixedType ft = ref _addr_ft.val;

            return ft.goString(0L, "");
        }

        private static @string goString(this ptr<FixedType> _addr_ft, long indent, @string field)
        {
            ref FixedType ft = ref _addr_ft.val;

            return fmt.Sprintf("%*s%sFixedType: Accum: %t; Sat: %t\n%s", indent, "", field, ft.Accum, ft.Sat, ft.Base.goString(indent + 2L, "Base: "));
        }

        // VectorType is a vector type.
        public partial struct VectorType
        {
            public AST Dimension;
            public AST Base;
        }

        private static void print(this ptr<VectorType> _addr_vt, ptr<printState> _addr_ps)
        {
            ref VectorType vt = ref _addr_vt.val;
            ref printState ps = ref _addr_ps.val;

            ps.inner = append(ps.inner, vt);
            ps.print(vt.Base);
            if (len(ps.inner) > 0L)
            {
                ps.printOneInner(null);
            }

        }

        private static void printInner(this ptr<VectorType> _addr_vt, ptr<printState> _addr_ps)
        {
            ref VectorType vt = ref _addr_vt.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString(" __vector(");
            ps.print(vt.Dimension);
            ps.writeByte(')');
        }

        private static bool Traverse(this ptr<VectorType> _addr_vt, Func<AST, bool> fn)
        {
            ref VectorType vt = ref _addr_vt.val;

            if (fn(vt))
            {
                vt.Dimension.Traverse(fn);
                vt.Base.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<VectorType> _addr_vt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref VectorType vt = ref _addr_vt.val;

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

            vt = addr(new VectorType(Dimension:dimension,Base:base));
            {
                var r = fn(vt);

                if (r != null)
                {
                    return r;
                }

            }

            return vt;

        }

        private static @string GoString(this ptr<VectorType> _addr_vt)
        {
            ref VectorType vt = ref _addr_vt.val;

            return vt.goString(0L, "");
        }

        private static @string goString(this ptr<VectorType> _addr_vt, long indent, @string field)
        {
            ref VectorType vt = ref _addr_vt.val;

            return fmt.Sprintf("%*s%sVectorType:\n%s\n%s", indent, "", field, vt.Dimension.goString(indent + 2L, "Dimension: "), vt.Base.goString(indent + 2L, "Base: "));
        }

        // Decltype is the decltype operator.
        public partial struct Decltype
        {
            public AST Expr;
        }

        private static void print(this ptr<Decltype> _addr_dt, ptr<printState> _addr_ps)
        {
            ref Decltype dt = ref _addr_dt.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString("decltype (");
            ps.print(dt.Expr);
            ps.writeByte(')');
        }

        private static bool Traverse(this ptr<Decltype> _addr_dt, Func<AST, bool> fn)
        {
            ref Decltype dt = ref _addr_dt.val;

            if (fn(dt))
            {
                dt.Expr.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Decltype> _addr_dt, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Decltype dt = ref _addr_dt.val;

            if (skip(dt))
            {
                return null;
            }

            var expr = dt.Expr.Copy(fn, skip);
            if (expr == null)
            {
                return fn(dt);
            }

            dt = addr(new Decltype(Expr:expr));
            {
                var r = fn(dt);

                if (r != null)
                {
                    return r;
                }

            }

            return dt;

        }

        private static @string GoString(this ptr<Decltype> _addr_dt)
        {
            ref Decltype dt = ref _addr_dt.val;

            return dt.goString(0L, "");
        }

        private static @string goString(this ptr<Decltype> _addr_dt, long indent, @string field)
        {
            ref Decltype dt = ref _addr_dt.val;

            return fmt.Sprintf("%*s%sDecltype:\n%s", indent, "", field, dt.Expr.goString(indent + 2L, "Expr: "));
        }

        // Operator is an operator.
        public partial struct Operator
        {
            public @string Name;
        }

        private static void print(this ptr<Operator> _addr_op, ptr<printState> _addr_ps)
        {
            ref Operator op = ref _addr_op.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString("operator");
            if (isLower(op.Name[0L]))
            {
                ps.writeByte(' ');
            }

            var n = op.Name;
            n = strings.TrimSuffix(n, " ");
            ps.writeString(n);

        }

        private static bool Traverse(this ptr<Operator> _addr_op, Func<AST, bool> fn)
        {
            ref Operator op = ref _addr_op.val;

            fn(op);
        }

        private static AST Copy(this ptr<Operator> _addr_op, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Operator op = ref _addr_op.val;

            if (skip(op))
            {
                return null;
            }

            return fn(op);

        }

        private static @string GoString(this ptr<Operator> _addr_op)
        {
            ref Operator op = ref _addr_op.val;

            return op.goString(0L, "");
        }

        private static @string goString(this ptr<Operator> _addr_op, long indent, @string field)
        {
            ref Operator op = ref _addr_op.val;

            return fmt.Sprintf("%*s%sOperator: %s", indent, "", field, op.Name);
        }

        // Constructor is a constructor.
        public partial struct Constructor
        {
            public AST Name;
        }

        private static void print(this ptr<Constructor> _addr_c, ptr<printState> _addr_ps)
        {
            ref Constructor c = ref _addr_c.val;
            ref printState ps = ref _addr_ps.val;

            ps.print(c.Name);
        }

        private static bool Traverse(this ptr<Constructor> _addr_c, Func<AST, bool> fn)
        {
            ref Constructor c = ref _addr_c.val;

            if (fn(c))
            {
                c.Name.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Constructor> _addr_c, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Constructor c = ref _addr_c.val;

            if (skip(c))
            {
                return null;
            }

            var name = c.Name.Copy(fn, skip);
            if (name == null)
            {
                return fn(c);
            }

            c = addr(new Constructor(Name:name));
            {
                var r = fn(c);

                if (r != null)
                {
                    return r;
                }

            }

            return c;

        }

        private static @string GoString(this ptr<Constructor> _addr_c)
        {
            ref Constructor c = ref _addr_c.val;

            return c.goString(0L, "");
        }

        private static @string goString(this ptr<Constructor> _addr_c, long indent, @string field)
        {
            ref Constructor c = ref _addr_c.val;

            return fmt.Sprintf("%*s%sConstructor:\n%s", indent, "", field, c.Name.goString(indent + 2L, "Name: "));
        }

        // Destructor is a destructor.
        public partial struct Destructor
        {
            public AST Name;
        }

        private static void print(this ptr<Destructor> _addr_d, ptr<printState> _addr_ps)
        {
            ref Destructor d = ref _addr_d.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeByte('~');
            ps.print(d.Name);
        }

        private static bool Traverse(this ptr<Destructor> _addr_d, Func<AST, bool> fn)
        {
            ref Destructor d = ref _addr_d.val;

            if (fn(d))
            {
                d.Name.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Destructor> _addr_d, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Destructor d = ref _addr_d.val;

            if (skip(d))
            {
                return null;
            }

            var name = d.Name.Copy(fn, skip);
            if (name == null)
            {
                return fn(d);
            }

            d = addr(new Destructor(Name:name));
            {
                var r = fn(d);

                if (r != null)
                {
                    return r;
                }

            }

            return d;

        }

        private static @string GoString(this ptr<Destructor> _addr_d)
        {
            ref Destructor d = ref _addr_d.val;

            return d.goString(0L, "");
        }

        private static @string goString(this ptr<Destructor> _addr_d, long indent, @string field)
        {
            ref Destructor d = ref _addr_d.val;

            return fmt.Sprintf("%*s%sDestructor:\n%s", indent, "", field, d.Name.goString(indent + 2L, "Name: "));
        }

        // GlobalCDtor is a global constructor or destructor.
        public partial struct GlobalCDtor
        {
            public bool Ctor;
            public AST Key;
        }

        private static void print(this ptr<GlobalCDtor> _addr_gcd, ptr<printState> _addr_ps)
        {
            ref GlobalCDtor gcd = ref _addr_gcd.val;
            ref printState ps = ref _addr_ps.val;

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

        private static bool Traverse(this ptr<GlobalCDtor> _addr_gcd, Func<AST, bool> fn)
        {
            ref GlobalCDtor gcd = ref _addr_gcd.val;

            if (fn(gcd))
            {
                gcd.Key.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<GlobalCDtor> _addr_gcd, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref GlobalCDtor gcd = ref _addr_gcd.val;

            if (skip(gcd))
            {
                return null;
            }

            var key = gcd.Key.Copy(fn, skip);
            if (key == null)
            {
                return fn(gcd);
            }

            gcd = addr(new GlobalCDtor(Ctor:gcd.Ctor,Key:key));
            {
                var r = fn(gcd);

                if (r != null)
                {
                    return r;
                }

            }

            return gcd;

        }

        private static @string GoString(this ptr<GlobalCDtor> _addr_gcd)
        {
            ref GlobalCDtor gcd = ref _addr_gcd.val;

            return gcd.goString(0L, "");
        }

        private static @string goString(this ptr<GlobalCDtor> _addr_gcd, long indent, @string field)
        {
            ref GlobalCDtor gcd = ref _addr_gcd.val;

            return fmt.Sprintf("%*s%sGlobalCDtor: Ctor: %t\n%s", indent, "", field, gcd.Ctor, gcd.Key.goString(indent + 2L, "Key: "));
        }

        // TaggedName is a name with an ABI tag.
        public partial struct TaggedName
        {
            public AST Name;
            public AST Tag;
        }

        private static void print(this ptr<TaggedName> _addr_t, ptr<printState> _addr_ps)
        {
            ref TaggedName t = ref _addr_t.val;
            ref printState ps = ref _addr_ps.val;

            ps.print(t.Name);
            ps.writeString("[abi:");
            ps.print(t.Tag);
            ps.writeByte(']');
        }

        private static bool Traverse(this ptr<TaggedName> _addr_t, Func<AST, bool> fn)
        {
            ref TaggedName t = ref _addr_t.val;

            if (fn(t))
            {
                t.Name.Traverse(fn);
                t.Tag.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<TaggedName> _addr_t, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref TaggedName t = ref _addr_t.val;

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

            t = addr(new TaggedName(Name:name,Tag:tag));
            {
                var r = fn(t);

                if (r != null)
                {
                    return r;
                }

            }

            return t;

        }

        private static @string GoString(this ptr<TaggedName> _addr_t)
        {
            ref TaggedName t = ref _addr_t.val;

            return t.goString(0L, "");
        }

        private static @string goString(this ptr<TaggedName> _addr_t, long indent, @string field)
        {
            ref TaggedName t = ref _addr_t.val;

            return fmt.Sprintf("%*s%sTaggedName:\n%s\n%s", indent, "", field, t.Name.goString(indent + 2L, "Name: "), t.Tag.goString(indent + 2L, "Tag: "));
        }

        // PackExpansion is a pack expansion.  The Pack field may be nil.
        public partial struct PackExpansion
        {
            public AST Base;
            public ptr<ArgumentPack> Pack;
        }

        private static void print(this ptr<PackExpansion> _addr_pe, ptr<printState> _addr_ps)
        {
            ref PackExpansion pe = ref _addr_pe.val;
            ref printState ps = ref _addr_ps.val;
 
            // We normally only get here if the simplify function was
            // unable to locate and expand the pack.
            if (pe.Pack == null)
            {
                parenthesize(_addr_ps, pe.Base);
                ps.writeString("...");
            }
            else
            {
                ps.print(pe.Base);
            }

        }

        private static bool Traverse(this ptr<PackExpansion> _addr_pe, Func<AST, bool> fn)
        {
            ref PackExpansion pe = ref _addr_pe.val;

            if (fn(pe))
            {
                pe.Base.Traverse(fn); 
                // Don't traverse Template--it points elsewhere in the AST.
            }

        }

        private static AST Copy(this ptr<PackExpansion> _addr_pe, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref PackExpansion pe = ref _addr_pe.val;

            if (skip(pe))
            {
                return null;
            }

            var @base = pe.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(pe);
            }

            pe = addr(new PackExpansion(Base:base,Pack:pe.Pack));
            {
                var r = fn(pe);

                if (r != null)
                {
                    return r;
                }

            }

            return pe;

        }

        private static @string GoString(this ptr<PackExpansion> _addr_pe)
        {
            ref PackExpansion pe = ref _addr_pe.val;

            return pe.goString(0L, "");
        }

        private static @string goString(this ptr<PackExpansion> _addr_pe, long indent, @string field)
        {
            ref PackExpansion pe = ref _addr_pe.val;

            return fmt.Sprintf("%*s%sPackExpansion: Pack: %p\n%s", indent, "", field, pe.Pack, pe.Base.goString(indent + 2L, "Base: "));
        }

        // ArgumentPack is an argument pack.
        public partial struct ArgumentPack
        {
            public slice<AST> Args;
        }

        private static void print(this ptr<ArgumentPack> _addr_ap, ptr<printState> _addr_ps)
        {
            ref ArgumentPack ap = ref _addr_ap.val;
            ref printState ps = ref _addr_ps.val;

            foreach (var (i, a) in ap.Args)
            {
                if (i > 0L)
                {
                    ps.writeString(", ");
                }

                ps.print(a);

            }

        }

        private static bool Traverse(this ptr<ArgumentPack> _addr_ap, Func<AST, bool> fn)
        {
            ref ArgumentPack ap = ref _addr_ap.val;

            if (fn(ap))
            {
                foreach (var (_, a) in ap.Args)
                {
                    a.Traverse(fn);
                }

            }

        }

        private static AST Copy(this ptr<ArgumentPack> _addr_ap, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref ArgumentPack ap = ref _addr_ap.val;

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

            ap = addr(new ArgumentPack(Args:args));
            {
                var r = fn(ap);

                if (r != null)
                {
                    return r;
                }

            }

            return ap;

        }

        private static @string GoString(this ptr<ArgumentPack> _addr_ap)
        {
            ref ArgumentPack ap = ref _addr_ap.val;

            return ap.goString(0L, "");
        }

        private static @string goString(this ptr<ArgumentPack> _addr_ap, long indent, @string field)
        {
            ref ArgumentPack ap = ref _addr_ap.val;

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

        private static void print(this ptr<SizeofPack> _addr_sp, ptr<printState> _addr_ps)
        {
            ref SizeofPack sp = ref _addr_sp.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString(fmt.Sprintf("%d", len(sp.Pack.Args)));
        }

        private static bool Traverse(this ptr<SizeofPack> _addr_sp, Func<AST, bool> fn)
        {
            ref SizeofPack sp = ref _addr_sp.val;

            fn(sp); 
            // Don't traverse the pack--it points elsewhere in the AST.
        }

        private static AST Copy(this ptr<SizeofPack> _addr_sp, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref SizeofPack sp = ref _addr_sp.val;

            if (skip(sp))
            {
                return null;
            }

            sp = addr(new SizeofPack(Pack:sp.Pack));
            {
                var r = fn(sp);

                if (r != null)
                {
                    return r;
                }

            }

            return sp;

        }

        private static @string GoString(this ptr<SizeofPack> _addr_sp)
        {
            ref SizeofPack sp = ref _addr_sp.val;

            return sp.goString(0L, "");
        }

        private static @string goString(this ptr<SizeofPack> _addr_sp, long indent, @string field)
        {
            ref SizeofPack sp = ref _addr_sp.val;

            return fmt.Sprintf("%*s%sSizeofPack: Pack: %p", indent, "", field, sp.Pack);
        }

        // SizeofArgs is the size of a captured template parameter pack from
        // an alias template.
        public partial struct SizeofArgs
        {
            public slice<AST> Args;
        }

        private static void print(this ptr<SizeofArgs> _addr_sa, ptr<printState> _addr_ps)
        {
            ref SizeofArgs sa = ref _addr_sa.val;
            ref printState ps = ref _addr_ps.val;

            long c = 0L;
            foreach (var (_, a) in sa.Args)
            {
                {
                    ptr<ArgumentPack> (ap, ok) = a._<ptr<ArgumentPack>>();

                    if (ok)
                    {
                        c += len(ap.Args);
                    }                    {
                        ptr<ExprList> (el, ok) = a._<ptr<ExprList>>();


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

        private static bool Traverse(this ptr<SizeofArgs> _addr_sa, Func<AST, bool> fn)
        {
            ref SizeofArgs sa = ref _addr_sa.val;

            if (fn(sa))
            {
                foreach (var (_, a) in sa.Args)
                {
                    a.Traverse(fn);
                }

            }

        }

        private static AST Copy(this ptr<SizeofArgs> _addr_sa, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref SizeofArgs sa = ref _addr_sa.val;

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

            sa = addr(new SizeofArgs(Args:args));
            {
                var r = fn(sa);

                if (r != null)
                {
                    return r;
                }

            }

            return sa;

        }

        private static @string GoString(this ptr<SizeofArgs> _addr_sa)
        {
            ref SizeofArgs sa = ref _addr_sa.val;

            return sa.goString(0L, "");
        }

        private static @string goString(this ptr<SizeofArgs> _addr_sa, long indent, @string field)
        {
            ref SizeofArgs sa = ref _addr_sa.val;

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

        private static void print(this ptr<Cast> _addr_c, ptr<printState> _addr_ps)
        {
            ref Cast c = ref _addr_c.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString("operator ");
            ps.print(c.To);
        }

        private static bool Traverse(this ptr<Cast> _addr_c, Func<AST, bool> fn)
        {
            ref Cast c = ref _addr_c.val;

            if (fn(c))
            {
                c.To.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Cast> _addr_c, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Cast c = ref _addr_c.val;

            if (skip(c))
            {
                return null;
            }

            var to = c.To.Copy(fn, skip);
            if (to == null)
            {
                return fn(c);
            }

            c = addr(new Cast(To:to));
            {
                var r = fn(c);

                if (r != null)
                {
                    return r;
                }

            }

            return c;

        }

        private static @string GoString(this ptr<Cast> _addr_c)
        {
            ref Cast c = ref _addr_c.val;

            return c.goString(0L, "");
        }

        private static @string goString(this ptr<Cast> _addr_c, long indent, @string field)
        {
            ref Cast c = ref _addr_c.val;

            return fmt.Sprintf("%*s%sCast\n%s", indent, "", field, c.To.goString(indent + 2L, "To: "));
        }

        // The parenthesize function prints the string for val, wrapped in
        // parentheses if necessary.
        private static void parenthesize(ptr<printState> _addr_ps, AST val)
        {
            ref printState ps = ref _addr_ps.val;

            var paren = false;
            switch (val.type())
            {
                case ptr<Name> v:
                    break;
                case ptr<InitializerList> v:
                    break;
                case ptr<FunctionParam> v:
                    break;
                case ptr<Qualified> v:
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

        private static void print(this ptr<Nullary> _addr_n, ptr<printState> _addr_ps)
        {
            ref Nullary n = ref _addr_n.val;
            ref printState ps = ref _addr_ps.val;

            {
                ptr<Operator> (op, ok) = n.Op._<ptr<Operator>>();

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

        private static bool Traverse(this ptr<Nullary> _addr_n, Func<AST, bool> fn)
        {
            ref Nullary n = ref _addr_n.val;

            if (fn(n))
            {
                n.Op.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Nullary> _addr_n, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Nullary n = ref _addr_n.val;

            if (skip(n))
            {
                return null;
            }

            var op = n.Op.Copy(fn, skip);
            if (op == null)
            {
                return fn(n);
            }

            n = addr(new Nullary(Op:op));
            {
                var r = fn(n);

                if (r != null)
                {
                    return r;
                }

            }

            return n;

        }

        private static @string GoString(this ptr<Nullary> _addr_n)
        {
            ref Nullary n = ref _addr_n.val;

            return n.goString(0L, "");
        }

        private static @string goString(this ptr<Nullary> _addr_n, long indent, @string field)
        {
            ref Nullary n = ref _addr_n.val;

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

        private static void print(this ptr<Unary> _addr_u, ptr<printState> _addr_ps)
        {
            ref Unary u = ref _addr_u.val;
            ref printState ps = ref _addr_ps.val;

            var expr = u.Expr; 

            // Don't print the argument list when taking the address of a
            // function.
            {
                ptr<Operator> op__prev1 = op;

                ptr<Operator> (op, ok) = u.Op._<ptr<Operator>>();

                if (ok && op.Name == "&")
                {
                    {
                        ptr<Typed> (t, ok) = expr._<ptr<Typed>>();

                        if (ok)
                        {
                            {
                                ptr<FunctionType> (_, ok) = t.Type._<ptr<FunctionType>>();

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
                parenthesize(_addr_ps, expr);
            }

            {
                ptr<Operator> op__prev1 = op;

                (op, ok) = u.Op._<ptr<Operator>>();

                if (ok)
                {
                    ps.writeString(op.Name);
                }                {
                    ptr<Cast> (c, ok) = u.Op._<ptr<Cast>>();


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
                    ptr<Operator> op__prev2 = op;

                    (op, ok) = u.Op._<ptr<Operator>>();

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
                        parenthesize(_addr_ps, expr);
                    }


                    op = op__prev2;

                }

            }

        }

        private static bool Traverse(this ptr<Unary> _addr_u, Func<AST, bool> fn)
        {
            ref Unary u = ref _addr_u.val;

            if (fn(u))
            {
                u.Op.Traverse(fn);
                u.Expr.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Unary> _addr_u, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Unary u = ref _addr_u.val;

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

            u = addr(new Unary(Op:op,Expr:expr,Suffix:u.Suffix,SizeofType:u.SizeofType));
            {
                var r = fn(u);

                if (r != null)
                {
                    return r;
                }

            }

            return u;

        }

        private static @string GoString(this ptr<Unary> _addr_u)
        {
            ref Unary u = ref _addr_u.val;

            return u.goString(0L, "");
        }

        private static @string goString(this ptr<Unary> _addr_u, long indent, @string field)
        {
            ref Unary u = ref _addr_u.val;

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

        private static void print(this ptr<Binary> _addr_b, ptr<printState> _addr_ps)
        {
            ref Binary b = ref _addr_b.val;
            ref printState ps = ref _addr_ps.val;

            ptr<Operator> (op, _) = b.Op._<ptr<Operator>>();

            if (op != null && strings.Contains(op.Name, "cast"))
            {
                ps.writeString(op.Name);
                ps.writeByte('<');
                ps.print(b.Left);
                ps.writeString(">(");
                ps.print(b.Right);
                ps.writeByte(')');
                return ;
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
                    ptr<Typed> (ty, ok) = b.Left._<ptr<Typed>>();

                    if (ok)
                    {
                        left = ty.Name;
                    }

                }

            }

            parenthesize(_addr_ps, left);

            if (op != null && op.Name == "[]")
            {
                ps.writeByte('[');
                ps.print(b.Right);
                ps.writeByte(']');
                return ;
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

            parenthesize(_addr_ps, b.Right);

            if (op != null && op.Name == ">")
            {
                ps.writeByte(')');
            }

        }

        private static bool Traverse(this ptr<Binary> _addr_b, Func<AST, bool> fn)
        {
            ref Binary b = ref _addr_b.val;

            if (fn(b))
            {
                b.Op.Traverse(fn);
                b.Left.Traverse(fn);
                b.Right.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Binary> _addr_b, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Binary b = ref _addr_b.val;

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

            b = addr(new Binary(Op:op,Left:left,Right:right));
            {
                var r = fn(b);

                if (r != null)
                {
                    return r;
                }

            }

            return b;

        }

        private static @string GoString(this ptr<Binary> _addr_b)
        {
            ref Binary b = ref _addr_b.val;

            return b.goString(0L, "");
        }

        private static @string goString(this ptr<Binary> _addr_b, long indent, @string field)
        {
            ref Binary b = ref _addr_b.val;

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

        private static void print(this ptr<Trinary> _addr_t, ptr<printState> _addr_ps)
        {
            ref Trinary t = ref _addr_t.val;
            ref printState ps = ref _addr_ps.val;

            parenthesize(_addr_ps, t.First);
            ps.writeByte('?');
            parenthesize(_addr_ps, t.Second);
            ps.writeString(" : ");
            parenthesize(_addr_ps, t.Third);
        }

        private static bool Traverse(this ptr<Trinary> _addr_t, Func<AST, bool> fn)
        {
            ref Trinary t = ref _addr_t.val;

            if (fn(t))
            {
                t.Op.Traverse(fn);
                t.First.Traverse(fn);
                t.Second.Traverse(fn);
                t.Third.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Trinary> _addr_t, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Trinary t = ref _addr_t.val;

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

            t = addr(new Trinary(Op:op,First:first,Second:second,Third:third));
            {
                var r = fn(t);

                if (r != null)
                {
                    return r;
                }

            }

            return t;

        }

        private static @string GoString(this ptr<Trinary> _addr_t)
        {
            ref Trinary t = ref _addr_t.val;

            return t.goString(0L, "");
        }

        private static @string goString(this ptr<Trinary> _addr_t, long indent, @string field)
        {
            ref Trinary t = ref _addr_t.val;

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

        private static void print(this ptr<Fold> _addr_f, ptr<printState> _addr_ps)
        {
            ref Fold f = ref _addr_f.val;
            ref printState ps = ref _addr_ps.val;

            ptr<Operator> (op, _) = f.Op._<ptr<Operator>>();
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
                    parenthesize(_addr_ps, f.Arg1);
                    ps.writeString(")");
                }
                else
                {
                    ps.writeString("(");
                    parenthesize(_addr_ps, f.Arg1);
                    printOp();
                    ps.writeString("...)");
                }

            }
            else
            {
                ps.writeString("(");
                parenthesize(_addr_ps, f.Arg1);
                printOp();
                ps.writeString("...");
                printOp();
                parenthesize(_addr_ps, f.Arg2);
                ps.writeString(")");
            }

        }

        private static bool Traverse(this ptr<Fold> _addr_f, Func<AST, bool> fn)
        {
            ref Fold f = ref _addr_f.val;

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

        private static AST Copy(this ptr<Fold> _addr_f, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Fold f = ref _addr_f.val;

            if (skip(f))
            {
                return null;
            }

            var op = f.Op.Copy(fn, skip);
            var arg1 = f.Arg1.Copy(fn, skip);
            AST arg2 = default!;
            if (f.Arg2 != null)
            {
                arg2 = AST.As(f.Arg2.Copy(fn, skip))!;
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
                arg2 = AST.As(f.Arg2)!;
            }

            f = addr(new Fold(Left:f.Left,Op:op,Arg1:arg1,Arg2:arg2));
            {
                var r = fn(f);

                if (r != null)
                {
                    return r;
                }

            }

            return f;

        }

        private static @string GoString(this ptr<Fold> _addr_f)
        {
            ref Fold f = ref _addr_f.val;

            return f.goString(0L, "");
        }

        private static @string goString(this ptr<Fold> _addr_f, long indent, @string field)
        {
            ref Fold f = ref _addr_f.val;

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

        private static void print(this ptr<New> _addr_n, ptr<printState> _addr_ps)
        {
            ref New n = ref _addr_n.val;
            ref printState ps = ref _addr_ps.val;
 
            // Op doesn't really matter for printing--we always print "new".
            ps.writeString("new ");
            if (n.Place != null)
            {
                parenthesize(_addr_ps, n.Place);
                ps.writeByte(' ');
            }

            ps.print(n.Type);
            if (n.Init != null)
            {
                parenthesize(_addr_ps, n.Init);
            }

        }

        private static bool Traverse(this ptr<New> _addr_n, Func<AST, bool> fn)
        {
            ref New n = ref _addr_n.val;

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

        private static AST Copy(this ptr<New> _addr_n, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref New n = ref _addr_n.val;

            if (skip(n))
            {
                return null;
            }

            var op = n.Op.Copy(fn, skip);
            AST place = default!;
            if (n.Place != null)
            {
                place = AST.As(n.Place.Copy(fn, skip))!;
            }

            var typ = n.Type.Copy(fn, skip);
            AST ini = default!;
            if (n.Init != null)
            {
                ini = AST.As(n.Init.Copy(fn, skip))!;
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
                place = AST.As(n.Place)!;
            }

            if (typ == null)
            {
                typ = n.Type;
            }

            if (ini == null)
            {
                ini = AST.As(n.Init)!;
            }

            n = addr(new New(Op:op,Place:place,Type:typ,Init:ini));
            {
                var r = fn(n);

                if (r != null)
                {
                    return r;
                }

            }

            return n;

        }

        private static @string GoString(this ptr<New> _addr_n)
        {
            ref New n = ref _addr_n.val;

            return n.goString(0L, "");
        }

        private static @string goString(this ptr<New> _addr_n, long indent, @string field)
        {
            ref New n = ref _addr_n.val;

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

        private static void print(this ptr<Literal> _addr_l, ptr<printState> _addr_ps)
        {
            ref Literal l = ref _addr_l.val;
            ref printState ps = ref _addr_ps.val;

            var isFloat = false;
            {
                ptr<BuiltinType> (b, ok) = l.Type._<ptr<BuiltinType>>();

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
                            return ;

                        }
                        else if (b.Name == "bool" && !l.Neg)
                        {
                            switch (l.Val)
                            {
                                case "0": 
                                    ps.writeString("false");
                                    return ;
                                    break;
                                case "1": 
                                    ps.writeString("true");
                                    return ;
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

        private static bool Traverse(this ptr<Literal> _addr_l, Func<AST, bool> fn)
        {
            ref Literal l = ref _addr_l.val;

            if (fn(l))
            {
                l.Type.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Literal> _addr_l, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Literal l = ref _addr_l.val;

            if (skip(l))
            {
                return null;
            }

            var typ = l.Type.Copy(fn, skip);
            if (typ == null)
            {
                return fn(l);
            }

            l = addr(new Literal(Type:typ,Val:l.Val,Neg:l.Neg));
            {
                var r = fn(l);

                if (r != null)
                {
                    return r;
                }

            }

            return l;

        }

        private static @string GoString(this ptr<Literal> _addr_l)
        {
            ref Literal l = ref _addr_l.val;

            return l.goString(0L, "");
        }

        private static @string goString(this ptr<Literal> _addr_l, long indent, @string field)
        {
            ref Literal l = ref _addr_l.val;

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

        private static void print(this ptr<ExprList> _addr_el, ptr<printState> _addr_ps)
        {
            ref ExprList el = ref _addr_el.val;
            ref printState ps = ref _addr_ps.val;

            foreach (var (i, e) in el.Exprs)
            {
                if (i > 0L)
                {
                    ps.writeString(", ");
                }

                ps.print(e);

            }

        }

        private static bool Traverse(this ptr<ExprList> _addr_el, Func<AST, bool> fn)
        {
            ref ExprList el = ref _addr_el.val;

            if (fn(el))
            {
                foreach (var (_, e) in el.Exprs)
                {
                    e.Traverse(fn);
                }

            }

        }

        private static AST Copy(this ptr<ExprList> _addr_el, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref ExprList el = ref _addr_el.val;

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

            el = addr(new ExprList(Exprs:exprs));
            {
                var r = fn(el);

                if (r != null)
                {
                    return r;
                }

            }

            return el;

        }

        private static @string GoString(this ptr<ExprList> _addr_el)
        {
            ref ExprList el = ref _addr_el.val;

            return el.goString(0L, "");
        }

        private static @string goString(this ptr<ExprList> _addr_el, long indent, @string field)
        {
            ref ExprList el = ref _addr_el.val;

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

        private static void print(this ptr<InitializerList> _addr_il, ptr<printState> _addr_ps)
        {
            ref InitializerList il = ref _addr_il.val;
            ref printState ps = ref _addr_ps.val;

            if (il.Type != null)
            {
                ps.print(il.Type);
            }

            ps.writeByte('{');
            ps.print(il.Exprs);
            ps.writeByte('}');

        }

        private static bool Traverse(this ptr<InitializerList> _addr_il, Func<AST, bool> fn)
        {
            ref InitializerList il = ref _addr_il.val;

            if (fn(il))
            {
                if (il.Type != null)
                {
                    il.Type.Traverse(fn);
                }

                il.Exprs.Traverse(fn);

            }

        }

        private static AST Copy(this ptr<InitializerList> _addr_il, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref InitializerList il = ref _addr_il.val;

            if (skip(il))
            {
                return null;
            }

            AST typ = default!;
            if (il.Type != null)
            {
                typ = AST.As(il.Type.Copy(fn, skip))!;
            }

            var exprs = il.Exprs.Copy(fn, skip);
            if (typ == null && exprs == null)
            {
                return fn(il);
            }

            if (typ == null)
            {
                typ = AST.As(il.Type)!;
            }

            if (exprs == null)
            {
                exprs = il.Exprs;
            }

            il = addr(new InitializerList(Type:typ,Exprs:exprs));
            {
                var r = fn(il);

                if (r != null)
                {
                    return r;
                }

            }

            return il;

        }

        private static @string GoString(this ptr<InitializerList> _addr_il)
        {
            ref InitializerList il = ref _addr_il.val;

            return il.goString(0L, "");
        }

        private static @string goString(this ptr<InitializerList> _addr_il, long indent, @string field)
        {
            ref InitializerList il = ref _addr_il.val;

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

        private static void print(this ptr<DefaultArg> _addr_da, ptr<printState> _addr_ps)
        {
            ref DefaultArg da = ref _addr_da.val;
            ref printState ps = ref _addr_ps.val;

            fmt.Fprintf(_addr_ps.buf, "{default arg#%d}::", da.Num + 1L);
            ps.print(da.Arg);
        }

        private static bool Traverse(this ptr<DefaultArg> _addr_da, Func<AST, bool> fn)
        {
            ref DefaultArg da = ref _addr_da.val;

            if (fn(da))
            {
                da.Arg.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<DefaultArg> _addr_da, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref DefaultArg da = ref _addr_da.val;

            if (skip(da))
            {
                return null;
            }

            var arg = da.Arg.Copy(fn, skip);
            if (arg == null)
            {
                return fn(da);
            }

            da = addr(new DefaultArg(Num:da.Num,Arg:arg));
            {
                var r = fn(da);

                if (r != null)
                {
                    return r;
                }

            }

            return da;

        }

        private static @string GoString(this ptr<DefaultArg> _addr_da)
        {
            ref DefaultArg da = ref _addr_da.val;

            return da.goString(0L, "");
        }

        private static @string goString(this ptr<DefaultArg> _addr_da, long indent, @string field)
        {
            ref DefaultArg da = ref _addr_da.val;

            return fmt.Sprintf("%*s%sDefaultArg: Num: %d\n%s", indent, "", field, da.Num, da.Arg.goString(indent + 2L, "Arg: "));
        }

        // Closure is a closure, or lambda expression.
        public partial struct Closure
        {
            public slice<AST> Types;
            public long Num;
        }

        private static void print(this ptr<Closure> _addr_cl, ptr<printState> _addr_ps)
        {
            ref Closure cl = ref _addr_cl.val;
            ref printState ps = ref _addr_ps.val;

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

        private static bool Traverse(this ptr<Closure> _addr_cl, Func<AST, bool> fn)
        {
            ref Closure cl = ref _addr_cl.val;

            if (fn(cl))
            {
                foreach (var (_, t) in cl.Types)
                {
                    t.Traverse(fn);
                }

            }

        }

        private static AST Copy(this ptr<Closure> _addr_cl, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Closure cl = ref _addr_cl.val;

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

            cl = addr(new Closure(Types:types,Num:cl.Num));
            {
                var r = fn(cl);

                if (r != null)
                {
                    return r;
                }

            }

            return cl;

        }

        private static @string GoString(this ptr<Closure> _addr_cl)
        {
            ref Closure cl = ref _addr_cl.val;

            return cl.goString(0L, "");
        }

        private static @string goString(this ptr<Closure> _addr_cl, long indent, @string field)
        {
            ref Closure cl = ref _addr_cl.val;

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

        private static void print(this ptr<UnnamedType> _addr_ut, ptr<printState> _addr_ps)
        {
            ref UnnamedType ut = ref _addr_ut.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString(fmt.Sprintf("{unnamed type#%d}", ut.Num + 1L));
        }

        private static bool Traverse(this ptr<UnnamedType> _addr_ut, Func<AST, bool> fn)
        {
            ref UnnamedType ut = ref _addr_ut.val;

            fn(ut);
        }

        private static AST Copy(this ptr<UnnamedType> _addr_ut, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref UnnamedType ut = ref _addr_ut.val;

            if (skip(ut))
            {
                return null;
            }

            return fn(ut);

        }

        private static @string GoString(this ptr<UnnamedType> _addr_ut)
        {
            ref UnnamedType ut = ref _addr_ut.val;

            return ut.goString(0L, "");
        }

        private static @string goString(this ptr<UnnamedType> _addr_ut, long indent, @string field)
        {
            ref UnnamedType ut = ref _addr_ut.val;

            return fmt.Sprintf("%*s%sUnnamedType: Num: %d", indent, "", field, ut.Num);
        }

        // Clone is a clone of a function, with a distinguishing suffix.
        public partial struct Clone
        {
            public AST Base;
            public @string Suffix;
        }

        private static void print(this ptr<Clone> _addr_c, ptr<printState> _addr_ps)
        {
            ref Clone c = ref _addr_c.val;
            ref printState ps = ref _addr_ps.val;

            ps.print(c.Base);
            ps.writeString(fmt.Sprintf(" [clone %s]", c.Suffix));
        }

        private static bool Traverse(this ptr<Clone> _addr_c, Func<AST, bool> fn)
        {
            ref Clone c = ref _addr_c.val;

            if (fn(c))
            {
                c.Base.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Clone> _addr_c, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Clone c = ref _addr_c.val;

            if (skip(c))
            {
                return null;
            }

            var @base = c.Base.Copy(fn, skip);
            if (base == null)
            {
                return fn(c);
            }

            c = addr(new Clone(Base:base,Suffix:c.Suffix));
            {
                var r = fn(c);

                if (r != null)
                {
                    return r;
                }

            }

            return c;

        }

        private static @string GoString(this ptr<Clone> _addr_c)
        {
            ref Clone c = ref _addr_c.val;

            return c.goString(0L, "");
        }

        private static @string goString(this ptr<Clone> _addr_c, long indent, @string field)
        {
            ref Clone c = ref _addr_c.val;

            return fmt.Sprintf("%*s%sClone: Suffix: %s\n%s", indent, "", field, c.Suffix, c.Base.goString(indent + 2L, "Base: "));
        }

        // Special is a special symbol, printed as a prefix plus another
        // value.
        public partial struct Special
        {
            public @string Prefix;
            public AST Val;
        }

        private static void print(this ptr<Special> _addr_s, ptr<printState> _addr_ps)
        {
            ref Special s = ref _addr_s.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString(s.Prefix);
            ps.print(s.Val);
        }

        private static bool Traverse(this ptr<Special> _addr_s, Func<AST, bool> fn)
        {
            ref Special s = ref _addr_s.val;

            if (fn(s))
            {
                s.Val.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Special> _addr_s, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Special s = ref _addr_s.val;

            if (skip(s))
            {
                return null;
            }

            var val = s.Val.Copy(fn, skip);
            if (val == null)
            {
                return fn(s);
            }

            s = addr(new Special(Prefix:s.Prefix,Val:val));
            {
                var r = fn(s);

                if (r != null)
                {
                    return r;
                }

            }

            return s;

        }

        private static @string GoString(this ptr<Special> _addr_s)
        {
            ref Special s = ref _addr_s.val;

            return s.goString(0L, "");
        }

        private static @string goString(this ptr<Special> _addr_s, long indent, @string field)
        {
            ref Special s = ref _addr_s.val;

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

        private static void print(this ptr<Special2> _addr_s, ptr<printState> _addr_ps)
        {
            ref Special2 s = ref _addr_s.val;
            ref printState ps = ref _addr_ps.val;

            ps.writeString(s.Prefix);
            ps.print(s.Val1);
            ps.writeString(s.Middle);
            ps.print(s.Val2);
        }

        private static bool Traverse(this ptr<Special2> _addr_s, Func<AST, bool> fn)
        {
            ref Special2 s = ref _addr_s.val;

            if (fn(s))
            {
                s.Val1.Traverse(fn);
                s.Val2.Traverse(fn);
            }

        }

        private static AST Copy(this ptr<Special2> _addr_s, Func<AST, AST> fn, Func<AST, bool> skip)
        {
            ref Special2 s = ref _addr_s.val;

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

            s = addr(new Special2(Prefix:s.Prefix,Val1:val1,Middle:s.Middle,Val2:val2));
            {
                var r = fn(s);

                if (r != null)
                {
                    return r;
                }

            }

            return s;

        }

        private static @string GoString(this ptr<Special2> _addr_s)
        {
            ref Special2 s = ref _addr_s.val;

            return s.goString(0L, "");
        }

        private static @string goString(this ptr<Special2> _addr_s, long indent, @string field)
        {
            ref Special2 s = ref _addr_s.val;

            return fmt.Sprintf("%*s%sSpecial2: Prefix: %s\n%s\n%*sMiddle: %s\n%s", indent, "", field, s.Prefix, s.Val1.goString(indent + 2L, "Val1: "), indent + 2L, "", s.Middle, s.Val2.goString(indent + 2L, "Val2: "));
        }

        // Print the inner types.
        private static slice<AST> printInner(this ptr<printState> _addr_ps, bool prefixOnly)
        {
            ref printState ps = ref _addr_ps.val;

            ref slice<AST> save = ref heap(out ptr<slice<AST>> _addr_save);
            ptr<slice<AST>> psave;
            if (prefixOnly)
            {
                psave = _addr_save;
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
            void printInner(ptr<printState> _p0);
        }

        // Print the most recent inner type.  If save is not nil, only print
        // prefixes.
        private static void printOneInner(this ptr<printState> _addr_ps, ptr<slice<AST>> _addr_save) => func((_, panic, __) =>
        {
            ref printState ps = ref _addr_ps.val;
            ref slice<AST> save = ref _addr_save.val;

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
                    ptr<MethodWithQualifiers> (_, ok) = a._<ptr<MethodWithQualifiers>>();

                    if (ok)
                    {
                        save = append(save, a);
                        return ;
                    }

                }

            }

            {
                innerPrinter (ip, ok) = innerPrinter.As(a._<innerPrinter>())!;

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
        private static bool isEmpty(this ptr<printState> _addr_ps, AST a)
        {
            ref printState ps = ref _addr_ps.val;

            switch (a.type())
            {
                case ptr<ArgumentPack> a:
                    return len(a.Args) == 0L;
                    break;
                case ptr<ExprList> a:
                    return len(a.Exprs) == 0L;
                    break;
                case ptr<PackExpansion> a:
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
