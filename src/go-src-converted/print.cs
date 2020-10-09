// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 06:03:26 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\print.go
// This file implements the String() methods for all Value and
// Instruction types.

using bytes = go.bytes_package;
using fmt = go.fmt_package;
using types = go.go.types_package;
using io = go.io_package;
using reflect = go.reflect_package;
using sort = go.sort_package;

using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        // relName returns the name of v relative to i.
        // In most cases, this is identical to v.Name(), but references to
        // Functions (including methods) and Globals use RelString and
        // all types are displayed with relType, so that only cross-package
        // references are package-qualified.
        //
        private static @string relName(Value v, Instruction i)
        {
            ptr<types.Package> from;
            if (i != null)
            {
                from = i.Parent().pkg();
            }
            switch (v.type())
            {
                case Member v:
                    return v.RelString(from);
                    break;
                case ptr<Const> v:
                    return v.RelString(from);
                    break;
            }
            return v.Name();

        }

        private static @string relType(types.Type t, ptr<types.Package> _addr_from)
        {
            ref types.Package from = ref _addr_from.val;

            return types.TypeString(t, types.RelativeTo(from));
        }

        private static @string relString(Member m, ptr<types.Package> _addr_from)
        {
            ref types.Package from = ref _addr_from.val;
 
            // NB: not all globals have an Object (e.g. init$guard),
            // so use Package().Object not Object.Package().
            {
                var pkg = m.Package().Pkg;

                if (pkg != null && pkg != from)
                {
                    return fmt.Sprintf("%s.%s", pkg.Path(), m.Name());
                }

            }

            return m.Name();

        }

        // Value.String()
        //
        // This method is provided only for debugging.
        // It never appears in disassembly, which uses Value.Name().

        private static @string String(this ptr<Parameter> _addr_v)
        {
            ref Parameter v = ref _addr_v.val;

            var from = v.Parent().pkg();
            return fmt.Sprintf("parameter %s : %s", v.Name(), relType(v.Type(), _addr_from));
        }

        private static @string String(this ptr<FreeVar> _addr_v)
        {
            ref FreeVar v = ref _addr_v.val;

            var from = v.Parent().pkg();
            return fmt.Sprintf("freevar %s : %s", v.Name(), relType(v.Type(), _addr_from));
        }

        private static @string String(this ptr<Builtin> _addr_v)
        {
            ref Builtin v = ref _addr_v.val;

            return fmt.Sprintf("builtin %s", v.Name());
        }

        // Instruction.String()

        private static @string String(this ptr<Alloc> _addr_v)
        {
            ref Alloc v = ref _addr_v.val;

            @string op = "local";
            if (v.Heap)
            {
                op = "new";
            }

            var from = v.Parent().pkg();
            return fmt.Sprintf("%s %s (%s)", op, relType(deref(v.Type()), _addr_from), v.Comment);

        }

        private static @string String(this ptr<Phi> _addr_v)
        {
            ref Phi v = ref _addr_v.val;

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            b.WriteString("phi [");
            foreach (var (i, edge) in v.Edges)
            {
                if (i > 0L)
                {
                    b.WriteString(", ");
                } 
                // Be robust against malformed CFG.
                if (v.block == null)
                {
                    b.WriteString("??");
                    continue;
                }

                long block = -1L;
                if (i < len(v.block.Preds))
                {
                    block = v.block.Preds[i].Index;
                }

                fmt.Fprintf(_addr_b, "%d: ", block);
                @string edgeVal = "<nil>"; // be robust
                if (edge != null)
                {
                    edgeVal = relName(edge, v);
                }

                b.WriteString(edgeVal);

            }
            b.WriteString("]");
            if (v.Comment != "")
            {
                b.WriteString(" #");
                b.WriteString(v.Comment);
            }

            return b.String();

        }

        private static @string printCall(ptr<CallCommon> _addr_v, @string prefix, Instruction instr)
        {
            ref CallCommon v = ref _addr_v.val;

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            b.WriteString(prefix);
            if (!v.IsInvoke())
            {
                b.WriteString(relName(v.Value, instr));
            }
            else
            {
                fmt.Fprintf(_addr_b, "invoke %s.%s", relName(v.Value, instr), v.Method.Name());
            }

            b.WriteString("(");
            foreach (var (i, arg) in v.Args)
            {
                if (i > 0L)
                {
                    b.WriteString(", ");
                }

                b.WriteString(relName(arg, instr));

            }
            if (v.Signature().Variadic())
            {
                b.WriteString("...");
            }

            b.WriteString(")");
            return b.String();

        }

        private static @string String(this ptr<CallCommon> _addr_c)
        {
            ref CallCommon c = ref _addr_c.val;

            return printCall(_addr_c, "", null);
        }

        private static @string String(this ptr<Call> _addr_v)
        {
            ref Call v = ref _addr_v.val;

            return printCall(_addr_v.Call, "", v);
        }

        private static @string String(this ptr<BinOp> _addr_v)
        {
            ref BinOp v = ref _addr_v.val;

            return fmt.Sprintf("%s %s %s", relName(v.X, v), v.Op.String(), relName(v.Y, v));
        }

        private static @string String(this ptr<UnOp> _addr_v)
        {
            ref UnOp v = ref _addr_v.val;

            return fmt.Sprintf("%s%s%s", v.Op, relName(v.X, v), commaOk(v.CommaOk));
        }

        private static @string printConv(@string prefix, Value v, Value x)
        {
            var from = v.Parent().pkg();
            return fmt.Sprintf("%s %s <- %s (%s)", prefix, relType(v.Type(), _addr_from), relType(x.Type(), _addr_from), relName(x, v._<Instruction>()));
        }

        private static @string String(this ptr<ChangeType> _addr_v)
        {
            ref ChangeType v = ref _addr_v.val;

            return printConv("changetype", v, v.X);
        }
        private static @string String(this ptr<Convert> _addr_v)
        {
            ref Convert v = ref _addr_v.val;

            return printConv("convert", v, v.X);
        }
        private static @string String(this ptr<ChangeInterface> _addr_v)
        {
            ref ChangeInterface v = ref _addr_v.val;

            return printConv("change interface", v, v.X);
        }
        private static @string String(this ptr<MakeInterface> _addr_v)
        {
            ref MakeInterface v = ref _addr_v.val;

            return printConv("make", v, v.X);
        }

        private static @string String(this ptr<MakeClosure> _addr_v)
        {
            ref MakeClosure v = ref _addr_v.val;

            ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
            fmt.Fprintf(_addr_b, "make closure %s", relName(v.Fn, v));
            if (v.Bindings != null)
            {
                b.WriteString(" [");
                foreach (var (i, c) in v.Bindings)
                {
                    if (i > 0L)
                    {
                        b.WriteString(", ");
                    }

                    b.WriteString(relName(c, v));

                }
                b.WriteString("]");

            }

            return b.String();

        }

        private static @string String(this ptr<MakeSlice> _addr_v)
        {
            ref MakeSlice v = ref _addr_v.val;

            var from = v.Parent().pkg();
            return fmt.Sprintf("make %s %s %s", relType(v.Type(), _addr_from), relName(v.Len, v), relName(v.Cap, v));
        }

        private static @string String(this ptr<Slice> _addr_v)
        {
            ref Slice v = ref _addr_v.val;

            bytes.Buffer b = default;
            b.WriteString("slice ");
            b.WriteString(relName(v.X, v));
            b.WriteString("[");
            if (v.Low != null)
            {
                b.WriteString(relName(v.Low, v));
            }

            b.WriteString(":");
            if (v.High != null)
            {
                b.WriteString(relName(v.High, v));
            }

            if (v.Max != null)
            {
                b.WriteString(":");
                b.WriteString(relName(v.Max, v));
            }

            b.WriteString("]");
            return b.String();

        }

        private static @string String(this ptr<MakeMap> _addr_v)
        {
            ref MakeMap v = ref _addr_v.val;

            @string res = "";
            if (v.Reserve != null)
            {
                res = relName(v.Reserve, v);
            }

            var from = v.Parent().pkg();
            return fmt.Sprintf("make %s %s", relType(v.Type(), _addr_from), res);

        }

        private static @string String(this ptr<MakeChan> _addr_v)
        {
            ref MakeChan v = ref _addr_v.val;

            var from = v.Parent().pkg();
            return fmt.Sprintf("make %s %s", relType(v.Type(), _addr_from), relName(v.Size, v));
        }

        private static @string String(this ptr<FieldAddr> _addr_v)
        {
            ref FieldAddr v = ref _addr_v.val;

            ptr<types.Struct> st = deref(v.X.Type()).Underlying()._<ptr<types.Struct>>(); 
            // Be robust against a bad index.
            @string name = "?";
            if (0L <= v.Field && v.Field < st.NumFields())
            {
                name = st.Field(v.Field).Name();
            }

            return fmt.Sprintf("&%s.%s [#%d]", relName(v.X, v), name, v.Field);

        }

        private static @string String(this ptr<Field> _addr_v)
        {
            ref Field v = ref _addr_v.val;

            ptr<types.Struct> st = v.X.Type().Underlying()._<ptr<types.Struct>>(); 
            // Be robust against a bad index.
            @string name = "?";
            if (0L <= v.Field && v.Field < st.NumFields())
            {
                name = st.Field(v.Field).Name();
            }

            return fmt.Sprintf("%s.%s [#%d]", relName(v.X, v), name, v.Field);

        }

        private static @string String(this ptr<IndexAddr> _addr_v)
        {
            ref IndexAddr v = ref _addr_v.val;

            return fmt.Sprintf("&%s[%s]", relName(v.X, v), relName(v.Index, v));
        }

        private static @string String(this ptr<Index> _addr_v)
        {
            ref Index v = ref _addr_v.val;

            return fmt.Sprintf("%s[%s]", relName(v.X, v), relName(v.Index, v));
        }

        private static @string String(this ptr<Lookup> _addr_v)
        {
            ref Lookup v = ref _addr_v.val;

            return fmt.Sprintf("%s[%s]%s", relName(v.X, v), relName(v.Index, v), commaOk(v.CommaOk));
        }

        private static @string String(this ptr<Range> _addr_v)
        {
            ref Range v = ref _addr_v.val;

            return "range " + relName(v.X, v);
        }

        private static @string String(this ptr<Next> _addr_v)
        {
            ref Next v = ref _addr_v.val;

            return "next " + relName(v.Iter, v);
        }

        private static @string String(this ptr<TypeAssert> _addr_v)
        {
            ref TypeAssert v = ref _addr_v.val;

            var from = v.Parent().pkg();
            return fmt.Sprintf("typeassert%s %s.(%s)", commaOk(v.CommaOk), relName(v.X, v), relType(v.AssertedType, _addr_from));
        }

        private static @string String(this ptr<Extract> _addr_v)
        {
            ref Extract v = ref _addr_v.val;

            return fmt.Sprintf("extract %s #%d", relName(v.Tuple, v), v.Index);
        }

        private static @string String(this ptr<Jump> _addr_s)
        {
            ref Jump s = ref _addr_s.val;
 
            // Be robust against malformed CFG.
            long block = -1L;
            if (s.block != null && len(s.block.Succs) == 1L)
            {
                block = s.block.Succs[0L].Index;
            }

            return fmt.Sprintf("jump %d", block);

        }

        private static @string String(this ptr<If> _addr_s)
        {
            ref If s = ref _addr_s.val;
 
            // Be robust against malformed CFG.
            long tblock = -1L;
            long fblock = -1L;
            if (s.block != null && len(s.block.Succs) == 2L)
            {
                tblock = s.block.Succs[0L].Index;
                fblock = s.block.Succs[1L].Index;
            }

            return fmt.Sprintf("if %s goto %d else %d", relName(s.Cond, s), tblock, fblock);

        }

        private static @string String(this ptr<Go> _addr_s)
        {
            ref Go s = ref _addr_s.val;

            return printCall(_addr_s.Call, "go ", s);
        }

        private static @string String(this ptr<Panic> _addr_s)
        {
            ref Panic s = ref _addr_s.val;

            return "panic " + relName(s.X, s);
        }

        private static @string String(this ptr<Return> _addr_s)
        {
            ref Return s = ref _addr_s.val;

            bytes.Buffer b = default;
            b.WriteString("return");
            foreach (var (i, r) in s.Results)
            {
                if (i == 0L)
                {
                    b.WriteString(" ");
                }
                else
                {
                    b.WriteString(", ");
                }

                b.WriteString(relName(r, s));

            }
            return b.String();

        }

        private static @string String(this ptr<RunDefers> _addr__p0)
        {
            ref RunDefers _p0 = ref _addr__p0.val;

            return "rundefers";
        }

        private static @string String(this ptr<Send> _addr_s)
        {
            ref Send s = ref _addr_s.val;

            return fmt.Sprintf("send %s <- %s", relName(s.Chan, s), relName(s.X, s));
        }

        private static @string String(this ptr<Defer> _addr_s)
        {
            ref Defer s = ref _addr_s.val;

            return printCall(_addr_s.Call, "defer ", s);
        }

        private static @string String(this ptr<Select> _addr_s)
        {
            ref Select s = ref _addr_s.val;

            bytes.Buffer b = default;
            foreach (var (i, st) in s.States)
            {
                if (i > 0L)
                {
                    b.WriteString(", ");
                }

                if (st.Dir == types.RecvOnly)
                {
                    b.WriteString("<-");
                    b.WriteString(relName(st.Chan, s));
                }
                else
                {
                    b.WriteString(relName(st.Chan, s));
                    b.WriteString("<-");
                    b.WriteString(relName(st.Send, s));
                }

            }
            @string non = "";
            if (!s.Blocking)
            {
                non = "non";
            }

            return fmt.Sprintf("select %sblocking [%s]", non, b.String());

        }

        private static @string String(this ptr<Store> _addr_s)
        {
            ref Store s = ref _addr_s.val;

            return fmt.Sprintf("*%s = %s", relName(s.Addr, s), relName(s.Val, s));
        }

        private static @string String(this ptr<MapUpdate> _addr_s)
        {
            ref MapUpdate s = ref _addr_s.val;

            return fmt.Sprintf("%s[%s] = %s", relName(s.Map, s), relName(s.Key, s), relName(s.Value, s));
        }

        private static @string String(this ptr<DebugRef> _addr_s)
        {
            ref DebugRef s = ref _addr_s.val;

            var p = s.Parent().Prog.Fset.Position(s.Pos());
            var descr = default;
            if (s.@object != null)
            {
                descr = s.@object; // e.g. "var x int"
            }
            else
            {
                descr = reflect.TypeOf(s.Expr); // e.g. "*ast.CallExpr"
            }

            @string addr = default;
            if (s.IsAddr)
            {
                addr = "address of ";
            }

            return fmt.Sprintf("; %s%s @ %d:%d is %s", addr, descr, p.Line, p.Column, s.X.Name());

        }

        private static @string String(this ptr<Package> _addr_p)
        {
            ref Package p = ref _addr_p.val;

            return "package " + p.Pkg.Path();
        }

        private static io.WriterTo _ = (Package.val)(null); // *Package implements io.Writer

        private static (long, error) WriteTo(this ptr<Package> _addr_p, io.Writer w)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Package p = ref _addr_p.val;

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            WritePackage(_addr_buf, _addr_p);
            var (n, err) = w.Write(buf.Bytes());
            return (int64(n), error.As(err)!);
        }

        // WritePackage writes to buf a human-readable summary of p.
        public static void WritePackage(ptr<bytes.Buffer> _addr_buf, ptr<Package> _addr_p)
        {
            ref bytes.Buffer buf = ref _addr_buf.val;
            ref Package p = ref _addr_p.val;

            fmt.Fprintf(buf, "%s:\n", p);

            slice<@string> names = default;
            long maxname = 0L;
            {
                var name__prev1 = name;

                foreach (var (__name) in p.Members)
                {
                    name = __name;
                    {
                        var l = len(name);

                        if (l > maxname)
                        {
                            maxname = l;
                        }

                    }

                    names = append(names, name);

                }

                name = name__prev1;
            }

            var from = p.Pkg;
            sort.Strings(names);
            {
                var name__prev1 = name;

                foreach (var (_, __name) in names)
                {
                    name = __name;
                    switch (p.Members[name].type())
                    {
                        case ptr<NamedConst> mem:
                            fmt.Fprintf(buf, "  const %-*s %s = %s\n", maxname, name, mem.Name(), mem.Value.RelString(from));
                            break;
                        case ptr<Function> mem:
                            fmt.Fprintf(buf, "  func  %-*s %s\n", maxname, name, relType(mem.Type(), _addr_from));
                            break;
                        case ptr<Type> mem:
                            fmt.Fprintf(buf, "  type  %-*s %s\n", maxname, name, relType(mem.Type().Underlying(), _addr_from));
                            foreach (var (_, meth) in typeutil.IntuitiveMethodSet(mem.Type(), _addr_p.Prog.MethodSets))
                            {
                                fmt.Fprintf(buf, "    %s\n", types.SelectionString(meth, types.RelativeTo(from)));
                            }
                            break;
                        case ptr<Global> mem:
                            fmt.Fprintf(buf, "  var   %-*s %s\n", maxname, name, relType(mem.Type()._<ptr<types.Pointer>>().Elem(), _addr_from));
                            break;
                    }

                }

                name = name__prev1;
            }

            fmt.Fprintf(buf, "\n");

        }

        private static @string commaOk(bool x)
        {
            if (x)
            {
                return ",ok";
            }

            return "";

        }
    }
}}}}}
