// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:41:29 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\fmt.go
using bytes = go.bytes_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // A FmtFlag value is a set of flags (or 0).
        // They control how the Xconv functions format their values.
        // See the respective function's documentation for details.
        public partial struct FmtFlag // : long
        {
        }

 //                                 fmt.Format flag/prec or verb
        public static readonly FmtFlag FmtLeft = (FmtFlag)1L << (int)(iota); // '-'
        public static readonly var FmtSharp = 0; // '#'
        public static readonly var FmtSign = 1; // '+'
        public static readonly var FmtUnsigned = 2; // internal use only (historic: u flag)
        public static readonly var FmtShort = 3; // verb == 'S'       (historic: h flag)
        public static readonly var FmtLong = 4; // verb == 'L'       (historic: l flag)
        public static readonly var FmtComma = 5; // '.' (== hasPrec)  (historic: , flag)
        public static readonly var FmtByte = 6; // '0'               (historic: hh flag)

        // fmtFlag computes the (internal) FmtFlag
        // value given the fmt.State and format verb.
        private static FmtFlag fmtFlag(fmt.State s, int verb)
        {
            FmtFlag flag = default;
            if (s.Flag('-'))
            {
                flag |= FmtLeft;
            }

            if (s.Flag('#'))
            {
                flag |= FmtSharp;
            }

            if (s.Flag('+'))
            {
                flag |= FmtSign;
            }

            if (s.Flag(' '))
            {
                Fatalf("FmtUnsigned in format string");
            }

            {
                var (_, ok) = s.Precision();

                if (ok)
                {
                    flag |= FmtComma;
                }

            }

            if (s.Flag('0'))
            {
                flag |= FmtByte;
            }

            switch (verb)
            {
                case 'S': 
                    flag |= FmtShort;
                    break;
                case 'L': 
                    flag |= FmtLong;
                    break;
            }
            return flag;

        }

        // Format conversions:
        // TODO(gri) verify these; eliminate those not used anymore
        //
        //    %v Op        Node opcodes
        //        Flags:  #: print Go syntax (automatic unless mode == FDbg)
        //
        //    %j *Node    Node details
        //        Flags:  0: suppresses things not relevant until walk
        //
        //    %v *Val        Constant values
        //
        //    %v *types.Sym        Symbols
        //    %S              unqualified identifier in any mode
        //        Flags:  +,- #: mode (see below)
        //            0: in export mode: unqualified identifier if exported, qualified if not
        //
        //    %v *types.Type    Types
        //    %S              omit "func" and receiver in function types
        //    %L              definition instead of name.
        //        Flags:  +,- #: mode (see below)
        //            ' ' (only in -/Sym mode) print type identifiers wit package name instead of prefix.
        //
        //    %v *Node    Nodes
        //    %S              (only in +/debug mode) suppress recursion
        //    %L              (only in Error mode) print "foo (type Bar)"
        //        Flags:  +,- #: mode (see below)
        //
        //    %v Nodes    Node lists
        //        Flags:  those of *Node
        //            .: separate items with ',' instead of ';'

        // *types.Sym, *types.Type, and *Node types use the flags below to set the format mode
        public static readonly fmtMode FErr = (fmtMode)iota;
        public static readonly var FDbg = 0;
        public static readonly var FTypeId = 1;
        public static readonly var FTypeIdName = 2; // same as FTypeId, but use package name instead of prefix

        // The mode flags '+', '-', and '#' are sticky; they persist through
        // recursions of *Node, *types.Type, and *types.Sym values. The ' ' flag is
        // sticky only on *types.Type recursions and only used in %-/*types.Sym mode.
        //
        // Example: given a *types.Sym: %+v %#v %-v print an identifier properly qualified for debug/export/internal mode

        // Useful format combinations:
        // TODO(gri): verify these
        //
        // *Node, Nodes:
        //   %+v    multiline recursive debug dump of *Node/Nodes
        //   %+S    non-recursive debug dump
        //
        // *Node:
        //   %#v    Go format
        //   %L     "foo (type Bar)" for error messages
        //
        // *types.Type:
        //   %#v    Go format
        //   %#L    type definition instead of name
        //   %#S    omit "func" and receiver in function signature
        //
        //   %-v    type identifiers
        //   %-S    type identifiers without "func" and arg names in type signatures (methodsym)
        //   %- v   type identifiers with package name instead of prefix (typesym, dcommontype, typehash)

        // update returns the results of applying f to mode.
        public static (FmtFlag, fmtMode) update(this FmtFlag f, fmtMode mode)
        {
            FmtFlag _p0 = default;
            fmtMode _p0 = default;


            if (f & FmtSign != 0L) 
                mode = FDbg;
            else if (f & FmtSharp != 0L)             else if (f & FmtUnsigned != 0L) 
                mode = FTypeIdName;
            else if (f & FmtLeft != 0L) 
                mode = FTypeId;
                        f &= FmtSharp | FmtLeft | FmtSign;
            return (f, mode);

        }

        private static @string goopnames = new slice<@string>(InitKeyedValues<@string>((OADDR, "&"), (OADD, "+"), (OADDSTR, "+"), (OALIGNOF, "unsafe.Alignof"), (OANDAND, "&&"), (OANDNOT, "&^"), (OAND, "&"), (OAPPEND, "append"), (OAS, "="), (OAS2, "="), (OBREAK, "break"), (OCALL, "function call"), (OCAP, "cap"), (OCASE, "case"), (OCLOSE, "close"), (OCOMPLEX, "complex"), (OBITNOT, "^"), (OCONTINUE, "continue"), (OCOPY, "copy"), (ODELETE, "delete"), (ODEFER, "defer"), (ODIV, "/"), (OEQ, "=="), (OFALL, "fallthrough"), (OFOR, "for"), (OFORUNTIL, "foruntil"), (OGE, ">="), (OGOTO, "goto"), (OGT, ">"), (OIF, "if"), (OIMAG, "imag"), (OINLMARK, "inlmark"), (ODEREF, "*"), (OLEN, "len"), (OLE, "<="), (OLSH, "<<"), (OLT, "<"), (OMAKE, "make"), (ONEG, "-"), (OMOD, "%"), (OMUL, "*"), (ONEW, "new"), (ONE, "!="), (ONOT, "!"), (OOFFSETOF, "unsafe.Offsetof"), (OOROR, "||"), (OOR, "|"), (OPANIC, "panic"), (OPLUS, "+"), (OPRINTN, "println"), (OPRINT, "print"), (ORANGE, "range"), (OREAL, "real"), (ORECV, "<-"), (ORECOVER, "recover"), (ORETURN, "return"), (ORSH, ">>"), (OSELECT, "select"), (OSEND, "<-"), (OSIZEOF, "unsafe.Sizeof"), (OSUB, "-"), (OSWITCH, "switch"), (OXOR, "^")));

        public static @string GoString(this Op o)
        {
            return fmt.Sprintf("%#v", o);
        }

        public static void format(this Op o, fmt.State s, int verb, fmtMode mode)
        {
            switch (verb)
            {
                case 'v': 
                    o.oconv(s, fmtFlag(s, verb), mode);
                    break;
                default: 
                    fmt.Fprintf(s, "%%!%c(Op=%d)", verb, int(o));
                    break;
            }

        }

        public static void oconv(this Op o, fmt.State s, FmtFlag flag, fmtMode mode)
        {
            if (flag & FmtSharp != 0L || mode != FDbg)
            {
                if (int(o) < len(goopnames) && goopnames[o] != "")
                {
                    fmt.Fprint(s, goopnames[o]);
                    return ;
                }

            } 

            // 'o.String()' instead of just 'o' to avoid infinite recursion
            fmt.Fprint(s, o.String());

        }

        private partial struct fmtMode // : long
        {
        }

        private partial struct fmtNodeErr // : Node
        {
        }
        private partial struct fmtNodeDbg // : Node
        {
        }
        private partial struct fmtNodeTypeId // : Node
        {
        }
        private partial struct fmtNodeTypeIdName // : Node
        {
        }

        private partial struct fmtOpErr // : Op
        {
        }
        private partial struct fmtOpDbg // : Op
        {
        }
        private partial struct fmtOpTypeId // : Op
        {
        }
        private partial struct fmtOpTypeIdName // : Op
        {
        }

        private partial struct fmtTypeErr // : types.Type
        {
        }
        private partial struct fmtTypeDbg // : types.Type
        {
        }
        private partial struct fmtTypeTypeId // : types.Type
        {
        }
        private partial struct fmtTypeTypeIdName // : types.Type
        {
        }

        private partial struct fmtSymErr // : types.Sym
        {
        }
        private partial struct fmtSymDbg // : types.Sym
        {
        }
        private partial struct fmtSymTypeId // : types.Sym
        {
        }
        private partial struct fmtSymTypeIdName // : types.Sym
        {
        }

        private partial struct fmtNodesErr // : Nodes
        {
        }
        private partial struct fmtNodesDbg // : Nodes
        {
        }
        private partial struct fmtNodesTypeId // : Nodes
        {
        }
        private partial struct fmtNodesTypeIdName // : Nodes
        {
        }
        private static void Format(this ptr<fmtNodeErr> _addr_n, fmt.State s, int verb)
        {
            ref fmtNodeErr n = ref _addr_n.val;

            (Node.val)(n).format(s, verb, FErr);
        }
        private static void Format(this ptr<fmtNodeDbg> _addr_n, fmt.State s, int verb)
        {
            ref fmtNodeDbg n = ref _addr_n.val;

            (Node.val)(n).format(s, verb, FDbg);
        }
        private static void Format(this ptr<fmtNodeTypeId> _addr_n, fmt.State s, int verb)
        {
            ref fmtNodeTypeId n = ref _addr_n.val;

            (Node.val)(n).format(s, verb, FTypeId);
        }
        private static void Format(this ptr<fmtNodeTypeIdName> _addr_n, fmt.State s, int verb)
        {
            ref fmtNodeTypeIdName n = ref _addr_n.val;

            (Node.val)(n).format(s, verb, FTypeIdName);
        }
        private static void Format(this ptr<Node> _addr_n, fmt.State s, int verb)
        {
            ref Node n = ref _addr_n.val;

            n.format(s, verb, FErr);
        }

        private static void Format(this fmtOpErr o, fmt.State s, int verb)
        {
            Op(o).format(s, verb, FErr);
        }
        private static void Format(this fmtOpDbg o, fmt.State s, int verb)
        {
            Op(o).format(s, verb, FDbg);
        }
        private static void Format(this fmtOpTypeId o, fmt.State s, int verb)
        {
            Op(o).format(s, verb, FTypeId);
        }
        private static void Format(this fmtOpTypeIdName o, fmt.State s, int verb)
        {
            Op(o).format(s, verb, FTypeIdName);
        }
        public static void Format(this Op o, fmt.State s, int verb)
        {
            o.format(s, verb, FErr);
        }

        private static void Format(this ptr<fmtTypeErr> _addr_t, fmt.State s, int verb)
        {
            ref fmtTypeErr t = ref _addr_t.val;

            typeFormat(_addr_(types.Type.val)(t), s, verb, FErr);
        }
        private static void Format(this ptr<fmtTypeDbg> _addr_t, fmt.State s, int verb)
        {
            ref fmtTypeDbg t = ref _addr_t.val;

            typeFormat(_addr_(types.Type.val)(t), s, verb, FDbg);
        }
        private static void Format(this ptr<fmtTypeTypeId> _addr_t, fmt.State s, int verb)
        {
            ref fmtTypeTypeId t = ref _addr_t.val;

            typeFormat(_addr_(types.Type.val)(t), s, verb, FTypeId);
        }
        private static void Format(this ptr<fmtTypeTypeIdName> _addr_t, fmt.State s, int verb)
        {
            ref fmtTypeTypeIdName t = ref _addr_t.val;

            typeFormat(_addr_(types.Type.val)(t), s, verb, FTypeIdName);
        }

        // func (t *types.Type) Format(s fmt.State, verb rune)     // in package types

        private static void Format(this ptr<fmtSymErr> _addr_y, fmt.State s, int verb)
        {
            ref fmtSymErr y = ref _addr_y.val;

            symFormat(_addr_(types.Sym.val)(y), s, verb, FErr);
        }
        private static void Format(this ptr<fmtSymDbg> _addr_y, fmt.State s, int verb)
        {
            ref fmtSymDbg y = ref _addr_y.val;

            symFormat(_addr_(types.Sym.val)(y), s, verb, FDbg);
        }
        private static void Format(this ptr<fmtSymTypeId> _addr_y, fmt.State s, int verb)
        {
            ref fmtSymTypeId y = ref _addr_y.val;

            symFormat(_addr_(types.Sym.val)(y), s, verb, FTypeId);
        }
        private static void Format(this ptr<fmtSymTypeIdName> _addr_y, fmt.State s, int verb)
        {
            ref fmtSymTypeIdName y = ref _addr_y.val;

            symFormat(_addr_(types.Sym.val)(y), s, verb, FTypeIdName);
        }

        // func (y *types.Sym) Format(s fmt.State, verb rune)            // in package types  { y.format(s, verb, FErr) }

        private static void Format(this fmtNodesErr n, fmt.State s, int verb)
        {
            (Nodes)(n).format(s, verb, FErr);
        }
        private static void Format(this fmtNodesDbg n, fmt.State s, int verb)
        {
            (Nodes)(n).format(s, verb, FDbg);
        }
        private static void Format(this fmtNodesTypeId n, fmt.State s, int verb)
        {
            (Nodes)(n).format(s, verb, FTypeId);
        }
        private static void Format(this fmtNodesTypeIdName n, fmt.State s, int verb)
        {
            (Nodes)(n).format(s, verb, FTypeIdName);
        }
        public static void Format(this Nodes n, fmt.State s, int verb)
        {
            n.format(s, verb, FErr);
        }

        private static void Fprintf(this fmtMode m, fmt.State s, @string format, params object[] args)
        {
            args = args.Clone();

            m.prepareArgs(args);
            fmt.Fprintf(s, format, args);
        }

        private static @string Sprintf(this fmtMode m, @string format, params object[] args)
        {
            args = args.Clone();

            m.prepareArgs(args);
            return fmt.Sprintf(format, args);
        }

        private static @string Sprint(this fmtMode m, params object[] args)
        {
            args = args.Clone();

            m.prepareArgs(args);
            return fmt.Sprint(args);
        }

        private static void prepareArgs(this fmtMode m, slice<object> args)
        {

            if (m == FErr) 
                {
                    var i__prev1 = i;
                    var arg__prev1 = arg;

                    foreach (var (__i, __arg) in args)
                    {
                        i = __i;
                        arg = __arg;
                        switch (arg.type())
                        {
                            case Op arg:
                                args[i] = fmtOpErr(arg);
                                break;
                            case ptr<Node> arg:
                                args[i] = (fmtNodeErr.val)(arg);
                                break;
                            case ptr<types.Type> arg:
                                args[i] = (fmtTypeErr.val)(arg);
                                break;
                            case ptr<types.Sym> arg:
                                args[i] = (fmtSymErr.val)(arg);
                                break;
                            case Nodes arg:
                                args[i] = fmtNodesErr(arg);
                                break;
                            case Val arg:
                                break;
                            case int arg:
                                break;
                            case long arg:
                                break;
                            case @string arg:
                                break;
                            case types.EType arg:
                                break;
                            default:
                            {
                                var arg = arg.type();
                                Fatalf("mode.prepareArgs type %T", arg);
                                break;
                            }
                        }

                    }

                    i = i__prev1;
                    arg = arg__prev1;
                }
            else if (m == FDbg) 
                {
                    var i__prev1 = i;
                    var arg__prev1 = arg;

                    foreach (var (__i, __arg) in args)
                    {
                        i = __i;
                        arg = __arg;
                        switch (arg.type())
                        {
                            case Op arg:
                                args[i] = fmtOpDbg(arg);
                                break;
                            case ptr<Node> arg:
                                args[i] = (fmtNodeDbg.val)(arg);
                                break;
                            case ptr<types.Type> arg:
                                args[i] = (fmtTypeDbg.val)(arg);
                                break;
                            case ptr<types.Sym> arg:
                                args[i] = (fmtSymDbg.val)(arg);
                                break;
                            case Nodes arg:
                                args[i] = fmtNodesDbg(arg);
                                break;
                            case Val arg:
                                break;
                            case int arg:
                                break;
                            case long arg:
                                break;
                            case @string arg:
                                break;
                            case types.EType arg:
                                break;
                            default:
                            {
                                var arg = arg.type();
                                Fatalf("mode.prepareArgs type %T", arg);
                                break;
                            }
                        }

                    }

                    i = i__prev1;
                    arg = arg__prev1;
                }
            else if (m == FTypeId) 
                {
                    var i__prev1 = i;
                    var arg__prev1 = arg;

                    foreach (var (__i, __arg) in args)
                    {
                        i = __i;
                        arg = __arg;
                        switch (arg.type())
                        {
                            case Op arg:
                                args[i] = fmtOpTypeId(arg);
                                break;
                            case ptr<Node> arg:
                                args[i] = (fmtNodeTypeId.val)(arg);
                                break;
                            case ptr<types.Type> arg:
                                args[i] = (fmtTypeTypeId.val)(arg);
                                break;
                            case ptr<types.Sym> arg:
                                args[i] = (fmtSymTypeId.val)(arg);
                                break;
                            case Nodes arg:
                                args[i] = fmtNodesTypeId(arg);
                                break;
                            case Val arg:
                                break;
                            case int arg:
                                break;
                            case long arg:
                                break;
                            case @string arg:
                                break;
                            case types.EType arg:
                                break;
                            default:
                            {
                                var arg = arg.type();
                                Fatalf("mode.prepareArgs type %T", arg);
                                break;
                            }
                        }

                    }

                    i = i__prev1;
                    arg = arg__prev1;
                }
            else if (m == FTypeIdName) 
                {
                    var i__prev1 = i;
                    var arg__prev1 = arg;

                    foreach (var (__i, __arg) in args)
                    {
                        i = __i;
                        arg = __arg;
                        switch (arg.type())
                        {
                            case Op arg:
                                args[i] = fmtOpTypeIdName(arg);
                                break;
                            case ptr<Node> arg:
                                args[i] = (fmtNodeTypeIdName.val)(arg);
                                break;
                            case ptr<types.Type> arg:
                                args[i] = (fmtTypeTypeIdName.val)(arg);
                                break;
                            case ptr<types.Sym> arg:
                                args[i] = (fmtSymTypeIdName.val)(arg);
                                break;
                            case Nodes arg:
                                args[i] = fmtNodesTypeIdName(arg);
                                break;
                            case Val arg:
                                break;
                            case int arg:
                                break;
                            case long arg:
                                break;
                            case @string arg:
                                break;
                            case types.EType arg:
                                break;
                            default:
                            {
                                var arg = arg.type();
                                Fatalf("mode.prepareArgs type %T", arg);
                                break;
                            }
                        }

                    }

                    i = i__prev1;
                    arg = arg__prev1;
                }
            else 
                Fatalf("mode.prepareArgs mode %d", m);
            
        }

        private static void format(this ptr<Node> _addr_n, fmt.State s, int verb, fmtMode mode)
        {
            ref Node n = ref _addr_n.val;

            switch (verb)
            {
                case 'v': 

                case 'S': 

                case 'L': 
                    n.nconv(s, fmtFlag(s, verb), mode);
                    break;
                case 'j': 
                    n.jconv(s, fmtFlag(s, verb));
                    break;
                default: 
                    fmt.Fprintf(s, "%%!%c(*Node=%p)", verb, n);
                    break;
            }

        }

        // *Node details
        private static void jconv(this ptr<Node> _addr_n, fmt.State s, FmtFlag flag)
        {
            ref Node n = ref _addr_n.val;

            var c = flag & FmtShort;

            if (c == 0L && n.Name != null && n.Name.Vargen != 0L)
            {
                fmt.Fprintf(s, " g(%d)", n.Name.Vargen);
            }

            if (n.Pos.IsKnown())
            {
                @string pfx = "";

                if (n.Pos.IsStmt() == src.PosNotStmt) 
                    pfx = "_"; // "-" would be confusing
                else if (n.Pos.IsStmt() == src.PosIsStmt) 
                    pfx = "+";
                                fmt.Fprintf(s, " l(%s%d)", pfx, n.Pos.Line());

            }

            if (c == 0L && n.Xoffset != BADWIDTH)
            {
                fmt.Fprintf(s, " x(%d)", n.Xoffset);
            }

            if (n.Class() != 0L)
            {
                fmt.Fprintf(s, " class(%v)", n.Class());
            }

            if (n.Colas())
            {
                fmt.Fprintf(s, " colas(%v)", n.Colas());
            }


            if (n.Esc == EscUnknown) 
                break;
            else if (n.Esc == EscHeap) 
                fmt.Fprint(s, " esc(h)");
            else if (n.Esc == EscNone) 
                fmt.Fprint(s, " esc(no)");
            else if (n.Esc == EscNever) 
                if (c == 0L)
                {
                    fmt.Fprint(s, " esc(N)");
                }

            else 
                fmt.Fprintf(s, " esc(%d)", n.Esc);
                        {
                ptr<EscLocation> (e, ok) = n.Opt()._<ptr<EscLocation>>();

                if (ok && e.loopDepth != 0L)
                {
                    fmt.Fprintf(s, " ld(%d)", e.loopDepth);
                }

            }


            if (c == 0L && n.Typecheck() != 0L)
            {
                fmt.Fprintf(s, " tc(%d)", n.Typecheck());
            }

            if (n.IsDDD())
            {
                fmt.Fprintf(s, " isddd(%v)", n.IsDDD());
            }

            if (n.Implicit())
            {
                fmt.Fprintf(s, " implicit(%v)", n.Implicit());
            }

            if (n.Embedded())
            {
                fmt.Fprintf(s, " embedded");
            }

            if (n.Op == ONAME)
            {
                if (n.Name.Addrtaken())
                {
                    fmt.Fprint(s, " addrtaken");
                }

                if (n.Name.Assigned())
                {
                    fmt.Fprint(s, " assigned");
                }

            }

            if (n.Bounded())
            {
                fmt.Fprint(s, " bounded");
            }

            if (n.NonNil())
            {
                fmt.Fprint(s, " nonnil");
            }

            if (c == 0L && n.HasCall())
            {
                fmt.Fprint(s, " hascall");
            }

            if (c == 0L && n.Name != null && n.Name.Used())
            {
                fmt.Fprint(s, " used");
            }

        }

        public static void Format(this Val v, fmt.State s, int verb)
        {
            switch (verb)
            {
                case 'v': 
                    v.vconv(s, fmtFlag(s, verb));
                    break;
                default: 
                    fmt.Fprintf(s, "%%!%c(Val=%T)", verb, v);
                    break;
            }

        }

        public static void vconv(this Val v, fmt.State s, FmtFlag flag)
        {
            switch (v.U.type())
            {
                case ptr<Mpint> u:
                    if (!u.Rune)
                    {
                        if (flag & FmtSharp != 0L)
                        {
                            fmt.Fprint(s, u.String());
                            return ;
                        }

                        fmt.Fprint(s, u.GoString());
                        return ;

                    }

                    {
                        var x = u.Int64();


                        if (' ' <= x && x < utf8.RuneSelf && x != '\\' && x != '\'') 
                            fmt.Fprintf(s, "'%c'", int(x));
                        else if (0L <= x && x < 1L << (int)(16L)) 
                            fmt.Fprintf(s, "'\\u%04x'", uint(int(x)));
                        else if (0L <= x && x <= utf8.MaxRune) 
                            fmt.Fprintf(s, "'\\U%08x'", uint64(x));
                        else 
                            fmt.Fprintf(s, "('\\x00' + %v)", u);

                    }
                    break;
                case ptr<Mpflt> u:
                    if (flag & FmtSharp != 0L)
                    {
                        fmt.Fprint(s, u.String());
                        return ;
                    }

                    fmt.Fprint(s, u.GoString());
                    return ;
                    break;
                case ptr<Mpcplx> u:
                    if (flag & FmtSharp != 0L)
                    {
                        fmt.Fprint(s, u.String());
                        return ;
                    }

                    fmt.Fprint(s, u.GoString());
                    return ;
                    break;
                case @string u:
                    fmt.Fprint(s, strconv.Quote(u));
                    break;
                case bool u:
                    fmt.Fprint(s, u);
                    break;
                case ptr<NilVal> u:
                    fmt.Fprint(s, "nil");
                    break;
                default:
                {
                    var u = v.U.type();
                    fmt.Fprintf(s, "<ctype=%d>", v.Ctype());
                    break;
                }
            }

        }

        /*
        s%,%,\n%g
        s%\n+%\n%g
        s%^[    ]*T%%g
        s%,.*%%g
        s%.+%    [T&]        = "&",%g
        s%^    ........*\]%&~%g
        s%~    %%g
        */

        private static void symfmt(ptr<bytes.Buffer> _addr_b, ptr<types.Sym> _addr_s, FmtFlag flag, fmtMode mode)
        {
            ref bytes.Buffer b = ref _addr_b.val;
            ref types.Sym s = ref _addr_s.val;

            if (flag & FmtShort == 0L)
            {

                if (mode == FErr) // This is for the user
                    if (s.Pkg == builtinpkg || s.Pkg == localpkg)
                    {
                        b.WriteString(s.Name);
                        return ;
                    } 

                    // If the name was used by multiple packages, display the full path,
                    if (s.Pkg.Name != "" && numImport[s.Pkg.Name] > 1L)
                    {
                        fmt.Fprintf(b, "%q.%s", s.Pkg.Path, s.Name);
                        return ;
                    }

                    b.WriteString(s.Pkg.Name);
                    b.WriteByte('.');
                    b.WriteString(s.Name);
                    return ;
                else if (mode == FDbg) 
                    b.WriteString(s.Pkg.Name);
                    b.WriteByte('.');
                    b.WriteString(s.Name);
                    return ;
                else if (mode == FTypeIdName) 
                    // dcommontype, typehash
                    b.WriteString(s.Pkg.Name);
                    b.WriteByte('.');
                    b.WriteString(s.Name);
                    return ;
                else if (mode == FTypeId) 
                    // (methodsym), typesym, weaksym
                    b.WriteString(s.Pkg.Prefix);
                    b.WriteByte('.');
                    b.WriteString(s.Name);
                    return ;
                
            }

            if (flag & FmtByte != 0L)
            { 
                // FmtByte (hh) implies FmtShort (h)
                // skip leading "type." in method name
                var name = s.Name;
                {
                    var i = strings.LastIndex(name, ".");

                    if (i >= 0L)
                    {
                        name = name[i + 1L..];
                    }

                }


                if (mode == FDbg)
                {
                    fmt.Fprintf(b, "@%q.%s", s.Pkg.Path, name);
                    return ;
                }

                b.WriteString(name);
                return ;

            }

            b.WriteString(s.Name);

        }

        private static @string basicnames = new slice<@string>(InitKeyedValues<@string>((TINT, "int"), (TUINT, "uint"), (TINT8, "int8"), (TUINT8, "uint8"), (TINT16, "int16"), (TUINT16, "uint16"), (TINT32, "int32"), (TUINT32, "uint32"), (TINT64, "int64"), (TUINT64, "uint64"), (TUINTPTR, "uintptr"), (TFLOAT32, "float32"), (TFLOAT64, "float64"), (TCOMPLEX64, "complex64"), (TCOMPLEX128, "complex128"), (TBOOL, "bool"), (TANY, "any"), (TSTRING, "string"), (TNIL, "nil"), (TIDEAL, "untyped number"), (TBLANK, "blank")));

        private static sync.Pool fmtBufferPool = new sync.Pool(New:func()interface{}{returnnew(bytes.Buffer)},);

        private static @string tconv(ptr<types.Type> _addr_t, FmtFlag flag, fmtMode mode) => func((defer, _, __) =>
        {
            ref types.Type t = ref _addr_t.val;

            ptr<bytes.Buffer> buf = fmtBufferPool.Get()._<ptr<bytes.Buffer>>();
            buf.Reset();
            defer(fmtBufferPool.Put(buf));

            tconv2(buf, _addr_t, flag, mode, null);
            return types.InternString(buf.Bytes());
        });

        // tconv2 writes a string representation of t to b.
        // flag and mode control exactly what is printed.
        // Any types x that are already in the visited map get printed as @%d where %d=visited[x].
        // See #16897 before changing the implementation of tconv.
        private static void tconv2(ptr<bytes.Buffer> _addr_b, ptr<types.Type> _addr_t, FmtFlag flag, fmtMode mode, map<ptr<types.Type>, long> visited) => func((defer, _, __) =>
        {
            ref bytes.Buffer b = ref _addr_b.val;
            ref types.Type t = ref _addr_t.val;

            {
                var (off, ok) = visited[t];

                if (ok)
                { 
                    // We've seen this type before, so we're trying to print it recursively.
                    // Print a reference to it instead.
                    fmt.Fprintf(b, "@%d", off);
                    return ;

                }

            }

            if (t == null)
            {
                b.WriteString("<T>");
                return ;
            }

            if (t.Etype == types.TSSA)
            {
                b.WriteString(t.Extra._<@string>());
                return ;
            }

            if (t.Etype == types.TTUPLE)
            {
                b.WriteString(t.FieldType(0L).String());
                b.WriteByte(',');
                b.WriteString(t.FieldType(1L).String());
                return ;
            }

            flag, mode = flag.update(mode);
            if (mode == FTypeIdName)
            {
                flag |= FmtUnsigned;
            }

            if (t == types.Bytetype || t == types.Runetype)
            { 
                // in %-T mode collapse rune and byte with their originals.

                if (mode == FTypeIdName || mode == FTypeId) 
                    t = types.Types[t.Etype];
                else 
                    sconv2(_addr_b, _addr_t.Sym, FmtShort, mode);
                    return ;
                
            }

            if (t == types.Errortype)
            {
                b.WriteString("error");
                return ;
            } 

            // Unless the 'L' flag was specified, if the type has a name, just print that name.
            if (flag & FmtLong == 0L && t.Sym != null && t != types.Types[t.Etype])
            {

                if (mode == FTypeId || mode == FTypeIdName) 
                    if (flag & FmtShort != 0L)
                    {
                        if (t.Vargen != 0L)
                        {
                            sconv2(_addr_b, _addr_t.Sym, FmtShort, mode);
                            fmt.Fprintf(b, "·%d", t.Vargen);
                            return ;
                        }

                        sconv2(_addr_b, _addr_t.Sym, FmtShort, mode);
                        return ;

                    }

                    if (mode == FTypeIdName)
                    {
                        sconv2(_addr_b, _addr_t.Sym, FmtUnsigned, mode);
                        return ;
                    }

                    if (t.Sym.Pkg == localpkg && t.Vargen != 0L)
                    {
                        b.WriteString(mode.Sprintf("%v·%d", t.Sym, t.Vargen));
                        return ;
                    }

                                sconv2(_addr_b, _addr_t.Sym, 0L, mode);
                return ;

            }

            if (int(t.Etype) < len(basicnames) && basicnames[t.Etype] != "")
            {
                @string name = default;

                if (t == types.Idealbool) 
                    name = "untyped bool";
                else if (t == types.Idealstring) 
                    name = "untyped string";
                else if (t == types.Idealint) 
                    name = "untyped int";
                else if (t == types.Idealrune) 
                    name = "untyped rune";
                else if (t == types.Idealfloat) 
                    name = "untyped float";
                else if (t == types.Idealcomplex) 
                    name = "untyped complex";
                else 
                    name = basicnames[t.Etype];
                                b.WriteString(name);
                return ;

            } 

            // At this point, we might call tconv2 recursively. Add the current type to the visited list so we don't
            // try to print it recursively.
            // We record the offset in the result buffer where the type's text starts. This offset serves as a reference
            // point for any later references to the same type.
            // Note that we remove the type from the visited map as soon as the recursive call is done.
            // This prevents encoding types like map[*int]*int as map[*int]@4. (That encoding would work,
            // but I'd like to use the @ notation only when strictly necessary.)
            if (visited == null)
            {
                visited = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<types.Type>, long>{};
            }

            visited[t] = b.Len();
            defer(delete(visited, t));

            if (mode == FDbg)
            {
                b.WriteString(t.Etype.String());
                b.WriteByte('-');
                tconv2(_addr_b, _addr_t, flag, FErr, visited);
                return ;
            }


            if (t.Etype == TPTR) 
                b.WriteByte('*');

                if (mode == FTypeId || mode == FTypeIdName) 
                    if (flag & FmtShort != 0L)
                    {
                        tconv2(_addr_b, _addr_t.Elem(), FmtShort, mode, visited);
                        return ;
                    }

                                tconv2(_addr_b, _addr_t.Elem(), 0L, mode, visited);
            else if (t.Etype == TARRAY) 
                b.WriteByte('[');
                b.WriteString(strconv.FormatInt(t.NumElem(), 10L));
                b.WriteByte(']');
                tconv2(_addr_b, _addr_t.Elem(), 0L, mode, visited);
            else if (t.Etype == TSLICE) 
                b.WriteString("[]");
                tconv2(_addr_b, _addr_t.Elem(), 0L, mode, visited);
            else if (t.Etype == TCHAN) 

                if (t.ChanDir() == types.Crecv) 
                    b.WriteString("<-chan ");
                    tconv2(_addr_b, _addr_t.Elem(), 0L, mode, visited);
                else if (t.ChanDir() == types.Csend) 
                    b.WriteString("chan<- ");
                    tconv2(_addr_b, _addr_t.Elem(), 0L, mode, visited);
                else 
                    b.WriteString("chan ");
                    if (t.Elem() != null && t.Elem().IsChan() && t.Elem().Sym == null && t.Elem().ChanDir() == types.Crecv)
                    {
                        b.WriteByte('(');
                        tconv2(_addr_b, _addr_t.Elem(), 0L, mode, visited);
                        b.WriteByte(')');
                    }
                    else
                    {
                        tconv2(_addr_b, _addr_t.Elem(), 0L, mode, visited);
                    }

                            else if (t.Etype == TMAP) 
                b.WriteString("map[");
                tconv2(_addr_b, _addr_t.Key(), 0L, mode, visited);
                b.WriteByte(']');
                tconv2(_addr_b, _addr_t.Elem(), 0L, mode, visited);
            else if (t.Etype == TINTER) 
                if (t.IsEmptyInterface())
                {
                    b.WriteString("interface {}");
                    break;
                }

                b.WriteString("interface {");
                {
                    var i__prev1 = i;
                    var f__prev1 = f;

                    foreach (var (__i, __f) in t.Fields().Slice())
                    {
                        i = __i;
                        f = __f;
                        if (i != 0L)
                        {
                            b.WriteByte(';');
                        }

                        b.WriteByte(' ');

                        if (f.Sym == null) 
                            // Check first that a symbol is defined for this type.
                            // Wrong interface definitions may have types lacking a symbol.
                            break;
                        else if (types.IsExported(f.Sym.Name)) 
                            sconv2(_addr_b, _addr_f.Sym, FmtShort, mode);
                        else 
                            var flag1 = FmtLeft;
                            if (flag & FmtUnsigned != 0L)
                            {
                                flag1 = FmtUnsigned;
                            }

                            sconv2(_addr_b, _addr_f.Sym, flag1, mode);
                                                tconv2(_addr_b, _addr_f.Type, FmtShort, mode, visited);

                    }

                    i = i__prev1;
                    f = f__prev1;
                }

                if (t.NumFields() != 0L)
                {
                    b.WriteByte(' ');
                }

                b.WriteByte('}');
            else if (t.Etype == TFUNC) 
                if (flag & FmtShort != 0L)
                { 
                    // no leading func
                }
                else
                {
                    if (t.Recv() != null)
                    {
                        b.WriteString("method");
                        tconv2(_addr_b, _addr_t.Recvs(), 0L, mode, visited);
                        b.WriteByte(' ');
                    }

                    b.WriteString("func");

                }

                tconv2(_addr_b, _addr_t.Params(), 0L, mode, visited);

                switch (t.NumResults())
                {
                    case 0L: 
                        break;
                    case 1L: 
                        b.WriteByte(' ');
                        tconv2(_addr_b, _addr_t.Results().Field(0L).Type, 0L, mode, visited); // struct->field->field's type
                        break;
                    default: 
                        b.WriteByte(' ');
                        tconv2(_addr_b, _addr_t.Results(), 0L, mode, visited);
                        break;
                }
            else if (t.Etype == TSTRUCT) 
                {
                    var m = t.StructType().Map;

                    if (m != null)
                    {
                        var mt = m.MapType(); 
                        // Format the bucket struct for map[x]y as map.bucket[x]y.
                        // This avoids a recursive print that generates very long names.

                        if (t == mt.Bucket) 
                            b.WriteString("map.bucket[");
                        else if (t == mt.Hmap) 
                            b.WriteString("map.hdr[");
                        else if (t == mt.Hiter) 
                            b.WriteString("map.iter[");
                        else 
                            Fatalf("unknown internal map type");
                                                tconv2(_addr_b, _addr_m.Key(), 0L, mode, visited);
                        b.WriteByte(']');
                        tconv2(_addr_b, _addr_m.Elem(), 0L, mode, visited);
                        break;

                    }

                }


                {
                    var funarg = t.StructType().Funarg;

                    if (funarg != types.FunargNone)
                    {
                        b.WriteByte('(');
                        flag1 = default;

                        if (mode == FTypeId || mode == FTypeIdName || mode == FErr) 
                            // no argument names on function signature, and no "noescape"/"nosplit" tags
                            flag1 = FmtShort;
                                                {
                            var i__prev1 = i;
                            var f__prev1 = f;

                            foreach (var (__i, __f) in t.Fields().Slice())
                            {
                                i = __i;
                                f = __f;
                                if (i != 0L)
                                {
                                    b.WriteString(", ");
                                }

                                fldconv(_addr_b, _addr_f, flag1, mode, visited, funarg);

                            }
                    else

                            i = i__prev1;
                            f = f__prev1;
                        }

                        b.WriteByte(')');

                    }                    {
                        b.WriteString("struct {");
                        {
                            var i__prev1 = i;
                            var f__prev1 = f;

                            foreach (var (__i, __f) in t.Fields().Slice())
                            {
                                i = __i;
                                f = __f;
                                if (i != 0L)
                                {
                                    b.WriteByte(';');
                                }

                                b.WriteByte(' ');
                                fldconv(_addr_b, _addr_f, FmtLong, mode, visited, funarg);

                            }

                            i = i__prev1;
                            f = f__prev1;
                        }

                        if (t.NumFields() != 0L)
                        {
                            b.WriteByte(' ');
                        }

                        b.WriteByte('}');

                    }

                }


            else if (t.Etype == TFORW) 
                b.WriteString("undefined");
                if (t.Sym != null)
                {
                    b.WriteByte(' ');
                    sconv2(_addr_b, _addr_t.Sym, 0L, mode);
                }

            else if (t.Etype == TUNSAFEPTR) 
                b.WriteString("unsafe.Pointer");
            else if (t.Etype == Txxx) 
                b.WriteString("Txxx");
            else 
                // Don't know how to handle - fall back to detailed prints.
                b.WriteString(mode.Sprintf("%v <%v>", t.Etype, t.Sym));
            
        });

        // Statements which may be rendered with a simplestmt as init.
        private static bool stmtwithinit(Op op)
        {

            if (op == OIF || op == OFOR || op == OFORUNTIL || op == OSWITCH) 
                return true;
                        return false;

        }

        private static void stmtfmt(this ptr<Node> _addr_n, fmt.State s, fmtMode mode)
        {
            ref Node n = ref _addr_n.val;
 
            // some statements allow for an init, but at most one,
            // but we may have an arbitrary number added, eg by typecheck
            // and inlining. If it doesn't fit the syntax, emit an enclosing
            // block starting with the init statements.

            // if we can just say "for" n->ninit; ... then do so
            var simpleinit = n.Ninit.Len() == 1L && n.Ninit.First().Ninit.Len() == 0L && stmtwithinit(n.Op); 

            // otherwise, print the inits as separate statements
            var complexinit = n.Ninit.Len() != 0L && !simpleinit && (mode != FErr); 

            // but if it was for if/for/switch, put in an extra surrounding block to limit the scope
            var extrablock = complexinit && stmtwithinit(n.Op);

            if (extrablock)
            {
                fmt.Fprint(s, "{");
            }

            if (complexinit)
            {
                mode.Fprintf(s, " %v; ", n.Ninit);
            }


            if (n.Op == ODCL)
            {
                mode.Fprintf(s, "var %v %v", n.Left.Sym, n.Left.Type);
                goto __switch_break0;
            }
            if (n.Op == ODCLFIELD)
            {
                if (n.Sym != null)
                {
                    mode.Fprintf(s, "%v %v", n.Sym, n.Left);
                }
                else
                {
                    mode.Fprintf(s, "%v", n.Left);
                } 

                // Don't export "v = <N>" initializing statements, hope they're always
                // preceded by the DCL which will be re-parsed and typechecked to reproduce
                // the "v = <N>" again.
                goto __switch_break0;
            }
            if (n.Op == OAS)
            {
                if (n.Colas() && !complexinit)
                {
                    mode.Fprintf(s, "%v := %v", n.Left, n.Right);
                }
                else
                {
                    mode.Fprintf(s, "%v = %v", n.Left, n.Right);
                }

                goto __switch_break0;
            }
            if (n.Op == OASOP)
            {
                if (n.Implicit())
                {
                    if (n.SubOp() == OADD)
                    {
                        mode.Fprintf(s, "%v++", n.Left);
                    }
                    else
                    {
                        mode.Fprintf(s, "%v--", n.Left);
                    }

                    break;

                }

                mode.Fprintf(s, "%v %#v= %v", n.Left, n.SubOp(), n.Right);
                goto __switch_break0;
            }
            if (n.Op == OAS2)
            {
                if (n.Colas() && !complexinit)
                {
                    mode.Fprintf(s, "%.v := %.v", n.List, n.Rlist);
                    break;
                }

                fallthrough = true;

            }
            if (fallthrough || n.Op == OAS2DOTTYPE || n.Op == OAS2FUNC || n.Op == OAS2MAPR || n.Op == OAS2RECV)
            {
                mode.Fprintf(s, "%.v = %v", n.List, n.Right);
                goto __switch_break0;
            }
            if (n.Op == ORETURN)
            {
                mode.Fprintf(s, "return %.v", n.List);
                goto __switch_break0;
            }
            if (n.Op == ORETJMP)
            {
                mode.Fprintf(s, "retjmp %v", n.Sym);
                goto __switch_break0;
            }
            if (n.Op == OINLMARK)
            {
                mode.Fprintf(s, "inlmark %d", n.Xoffset);
                goto __switch_break0;
            }
            if (n.Op == OGO)
            {
                mode.Fprintf(s, "go %v", n.Left);
                goto __switch_break0;
            }
            if (n.Op == ODEFER)
            {
                mode.Fprintf(s, "defer %v", n.Left);
                goto __switch_break0;
            }
            if (n.Op == OIF)
            {
                if (simpleinit)
                {
                    mode.Fprintf(s, "if %v; %v { %v }", n.Ninit.First(), n.Left, n.Nbody);
                }
                else
                {
                    mode.Fprintf(s, "if %v { %v }", n.Left, n.Nbody);
                }

                if (n.Rlist.Len() != 0L)
                {
                    mode.Fprintf(s, " else { %v }", n.Rlist);
                }

                goto __switch_break0;
            }
            if (n.Op == OFOR || n.Op == OFORUNTIL)
            {
                @string opname = "for";
                if (n.Op == OFORUNTIL)
                {
                    opname = "foruntil";
                }

                if (mode == FErr)
                { // TODO maybe only if FmtShort, same below
                    fmt.Fprintf(s, "%s loop", opname);
                    break;

                }

                fmt.Fprint(s, opname);
                if (simpleinit)
                {
                    mode.Fprintf(s, " %v;", n.Ninit.First());
                }
                else if (n.Right != null)
                {
                    fmt.Fprint(s, " ;");
                }

                if (n.Left != null)
                {
                    mode.Fprintf(s, " %v", n.Left);
                }

                if (n.Right != null)
                {
                    mode.Fprintf(s, "; %v", n.Right);
                }
                else if (simpleinit)
                {
                    fmt.Fprint(s, ";");
                }

                if (n.Op == OFORUNTIL && n.List.Len() != 0L)
                {
                    mode.Fprintf(s, "; %v", n.List);
                }

                mode.Fprintf(s, " { %v }", n.Nbody);
                goto __switch_break0;
            }
            if (n.Op == ORANGE)
            {
                if (mode == FErr)
                {
                    fmt.Fprint(s, "for loop");
                    break;
                }

                if (n.List.Len() == 0L)
                {
                    mode.Fprintf(s, "for range %v { %v }", n.Right, n.Nbody);
                    break;
                }

                mode.Fprintf(s, "for %.v = range %v { %v }", n.List, n.Right, n.Nbody);
                goto __switch_break0;
            }
            if (n.Op == OSELECT || n.Op == OSWITCH)
            {
                if (mode == FErr)
                {
                    mode.Fprintf(s, "%v statement", n.Op);
                    break;
                }

                mode.Fprintf(s, "%#v", n.Op);
                if (simpleinit)
                {
                    mode.Fprintf(s, " %v;", n.Ninit.First());
                }

                if (n.Left != null)
                {
                    mode.Fprintf(s, " %v ", n.Left);
                }

                mode.Fprintf(s, " { %v }", n.List);
                goto __switch_break0;
            }
            if (n.Op == OCASE)
            {
                if (n.List.Len() != 0L)
                {
                    mode.Fprintf(s, "case %.v", n.List);
                }
                else
                {
                    fmt.Fprint(s, "default");
                }

                mode.Fprintf(s, ": %v", n.Nbody);
                goto __switch_break0;
            }
            if (n.Op == OBREAK || n.Op == OCONTINUE || n.Op == OGOTO || n.Op == OFALL)
            {
                if (n.Sym != null)
                {
                    mode.Fprintf(s, "%#v %v", n.Op, n.Sym);
                }
                else
                {
                    mode.Fprintf(s, "%#v", n.Op);
                }

                goto __switch_break0;
            }
            if (n.Op == OEMPTY)
            {
                break;
                goto __switch_break0;
            }
            if (n.Op == OLABEL)
            {
                mode.Fprintf(s, "%v: ", n.Sym);
                goto __switch_break0;
            }

            __switch_break0:;

            if (extrablock)
            {
                fmt.Fprint(s, "}");
            }

        }

        private static long opprec = new slice<long>(InitKeyedValues<long>((OALIGNOF, 8), (OAPPEND, 8), (OBYTES2STR, 8), (OARRAYLIT, 8), (OSLICELIT, 8), (ORUNES2STR, 8), (OCALLFUNC, 8), (OCALLINTER, 8), (OCALLMETH, 8), (OCALL, 8), (OCAP, 8), (OCLOSE, 8), (OCONVIFACE, 8), (OCONVNOP, 8), (OCONV, 8), (OCOPY, 8), (ODELETE, 8), (OGETG, 8), (OLEN, 8), (OLITERAL, 8), (OMAKESLICE, 8), (OMAKESLICECOPY, 8), (OMAKE, 8), (OMAPLIT, 8), (ONAME, 8), (ONEW, 8), (ONONAME, 8), (OOFFSETOF, 8), (OPACK, 8), (OPANIC, 8), (OPAREN, 8), (OPRINTN, 8), (OPRINT, 8), (ORUNESTR, 8), (OSIZEOF, 8), (OSTR2BYTES, 8), (OSTR2RUNES, 8), (OSTRUCTLIT, 8), (OTARRAY, 8), (OTCHAN, 8), (OTFUNC, 8), (OTINTER, 8), (OTMAP, 8), (OTSTRUCT, 8), (OINDEXMAP, 8), (OINDEX, 8), (OSLICE, 8), (OSLICESTR, 8), (OSLICEARR, 8), (OSLICE3, 8), (OSLICE3ARR, 8), (OSLICEHEADER, 8), (ODOTINTER, 8), (ODOTMETH, 8), (ODOTPTR, 8), (ODOTTYPE2, 8), (ODOTTYPE, 8), (ODOT, 8), (OXDOT, 8), (OCALLPART, 8), (OPLUS, 7), (ONOT, 7), (OBITNOT, 7), (ONEG, 7), (OADDR, 7), (ODEREF, 7), (ORECV, 7), (OMUL, 6), (ODIV, 6), (OMOD, 6), (OLSH, 6), (ORSH, 6), (OAND, 6), (OANDNOT, 6), (OADD, 5), (OSUB, 5), (OOR, 5), (OXOR, 5), (OEQ, 4), (OLT, 4), (OLE, 4), (OGE, 4), (OGT, 4), (ONE, 4), (OSEND, 3), (OANDAND, 2), (OOROR, 1), (OAS, -1), (OAS2, -1), (OAS2DOTTYPE, -1), (OAS2FUNC, -1), (OAS2MAPR, -1), (OAS2RECV, -1), (OASOP, -1), (OBREAK, -1), (OCASE, -1), (OCONTINUE, -1), (ODCL, -1), (ODCLFIELD, -1), (ODEFER, -1), (OEMPTY, -1), (OFALL, -1), (OFOR, -1), (OFORUNTIL, -1), (OGOTO, -1), (OIF, -1), (OLABEL, -1), (OGO, -1), (ORANGE, -1), (ORETURN, -1), (OSELECT, -1), (OSWITCH, -1), (OEND, 0)));

        private static void exprfmt(this ptr<Node> _addr_n, fmt.State s, long prec, fmtMode mode)
        {
            ref Node n = ref _addr_n.val;

            while (n != null && n.Implicit() && (n.Op == ODEREF || n.Op == OADDR))
            {
                n = n.Left;
            }


            if (n == null)
            {
                fmt.Fprint(s, "<N>");
                return ;
            }

            var nprec = opprec[n.Op];
            if (n.Op == OTYPE && n.Sym != null)
            {
                nprec = 8L;
            }

            if (prec > nprec)
            {
                mode.Fprintf(s, "(%v)", n);
                return ;
            }


            if (n.Op == OPAREN)
            {
                mode.Fprintf(s, "(%v)", n.Left);
                goto __switch_break1;
            }
            if (n.Op == OLITERAL) // this is a bit of a mess
            {
                if (mode == FErr)
                {
                    if (n.Orig != null && n.Orig != n)
                    {
                        n.Orig.exprfmt(s, prec, mode);
                        return ;
                    }

                    if (n.Sym != null)
                    {
                        fmt.Fprint(s, smodeString(_addr_n.Sym, mode));
                        return ;
                    }

                }

                if (n.Val().Ctype() == CTNIL && n.Orig != null && n.Orig != n)
                {
                    n.Orig.exprfmt(s, prec, mode);
                    return ;
                }

                if (n.Type != null && n.Type.Etype != TIDEAL && n.Type.Etype != TNIL && n.Type != types.Idealbool && n.Type != types.Idealstring)
                { 
                    // Need parens when type begins with what might
                    // be misinterpreted as a unary operator: * or <-.
                    if (n.Type.IsPtr() || (n.Type.IsChan() && n.Type.ChanDir() == types.Crecv))
                    {
                        mode.Fprintf(s, "(%v)(%v)", n.Type, n.Val());
                        return ;
                    }
                    else
                    {
                        mode.Fprintf(s, "%v(%v)", n.Type, n.Val());
                        return ;
                    }

                }

                mode.Fprintf(s, "%v", n.Val()); 

                // Special case: name used as local variable in export.
                // _ becomes ~b%d internally; print as _ for export
                goto __switch_break1;
            }
            if (n.Op == ONAME)
            {
                if (mode == FErr && n.Sym != null && n.Sym.Name[0L] == '~' && n.Sym.Name[1L] == 'b')
                {
                    fmt.Fprint(s, "_");
                    return ;
                }

                fallthrough = true;
            }
            if (fallthrough || n.Op == OPACK || n.Op == ONONAME)
            {
                fmt.Fprint(s, smodeString(_addr_n.Sym, mode));
                goto __switch_break1;
            }
            if (n.Op == OTYPE)
            {
                if (n.Type == null && n.Sym != null)
                {
                    fmt.Fprint(s, smodeString(_addr_n.Sym, mode));
                    return ;
                }

                mode.Fprintf(s, "%v", n.Type);
                goto __switch_break1;
            }
            if (n.Op == OTARRAY)
            {
                if (n.Left != null)
                {
                    mode.Fprintf(s, "[%v]%v", n.Left, n.Right);
                    return ;
                }

                mode.Fprintf(s, "[]%v", n.Right); // happens before typecheck
                goto __switch_break1;
            }
            if (n.Op == OTMAP)
            {
                mode.Fprintf(s, "map[%v]%v", n.Left, n.Right);
                goto __switch_break1;
            }
            if (n.Op == OTCHAN)
            {

                if (n.TChanDir() == types.Crecv) 
                    mode.Fprintf(s, "<-chan %v", n.Left);
                else if (n.TChanDir() == types.Csend) 
                    mode.Fprintf(s, "chan<- %v", n.Left);
                else 
                    if (n.Left != null && n.Left.Op == OTCHAN && n.Left.Sym == null && n.Left.TChanDir() == types.Crecv)
                    {
                        mode.Fprintf(s, "chan (%v)", n.Left);
                    }
                    else
                    {
                        mode.Fprintf(s, "chan %v", n.Left);
                    }

                                goto __switch_break1;
            }
            if (n.Op == OTSTRUCT)
            {
                fmt.Fprint(s, "<struct>");
                goto __switch_break1;
            }
            if (n.Op == OTINTER)
            {
                fmt.Fprint(s, "<inter>");
                goto __switch_break1;
            }
            if (n.Op == OTFUNC)
            {
                fmt.Fprint(s, "<func>");
                goto __switch_break1;
            }
            if (n.Op == OCLOSURE)
            {
                if (mode == FErr)
                {
                    fmt.Fprint(s, "func literal");
                    return ;
                }

                if (n.Nbody.Len() != 0L)
                {
                    mode.Fprintf(s, "%v { %v }", n.Type, n.Nbody);
                    return ;
                }

                mode.Fprintf(s, "%v { %v }", n.Type, n.Func.Closure.Nbody);
                goto __switch_break1;
            }
            if (n.Op == OCOMPLIT)
            {
                if (mode == FErr)
                {
                    if (n.Implicit())
                    {
                        mode.Fprintf(s, "... argument");
                        return ;
                    }

                    if (n.Right != null)
                    {
                        mode.Fprintf(s, "%v literal", n.Right);
                        return ;
                    }

                    fmt.Fprint(s, "composite literal");
                    return ;

                }

                mode.Fprintf(s, "(%v{ %.v })", n.Right, n.List);
                goto __switch_break1;
            }
            if (n.Op == OPTRLIT)
            {
                mode.Fprintf(s, "&%v", n.Left);
                goto __switch_break1;
            }
            if (n.Op == OSTRUCTLIT || n.Op == OARRAYLIT || n.Op == OSLICELIT || n.Op == OMAPLIT)
            {
                if (mode == FErr)
                {
                    mode.Fprintf(s, "%v literal", n.Type);
                    return ;
                }

                mode.Fprintf(s, "(%v{ %.v })", n.Type, n.List);
                goto __switch_break1;
            }
            if (n.Op == OKEY)
            {
                if (n.Left != null && n.Right != null)
                {
                    mode.Fprintf(s, "%v:%v", n.Left, n.Right);
                    return ;
                }

                if (n.Left == null && n.Right != null)
                {
                    mode.Fprintf(s, ":%v", n.Right);
                    return ;
                }

                if (n.Left != null && n.Right == null)
                {
                    mode.Fprintf(s, "%v:", n.Left);
                    return ;
                }

                fmt.Fprint(s, ":");
                goto __switch_break1;
            }
            if (n.Op == OSTRUCTKEY)
            {
                mode.Fprintf(s, "%v:%v", n.Sym, n.Left);
                goto __switch_break1;
            }
            if (n.Op == OCALLPART)
            {
                n.Left.exprfmt(s, nprec, mode);
                if (n.Right == null || n.Right.Sym == null)
                {
                    fmt.Fprint(s, ".<nil>");
                    return ;
                }

                mode.Fprintf(s, ".%0S", n.Right.Sym);
                goto __switch_break1;
            }
            if (n.Op == OXDOT || n.Op == ODOT || n.Op == ODOTPTR || n.Op == ODOTINTER || n.Op == ODOTMETH)
            {
                n.Left.exprfmt(s, nprec, mode);
                if (n.Sym == null)
                {
                    fmt.Fprint(s, ".<nil>");
                    return ;
                }

                mode.Fprintf(s, ".%0S", n.Sym);
                goto __switch_break1;
            }
            if (n.Op == ODOTTYPE || n.Op == ODOTTYPE2)
            {
                n.Left.exprfmt(s, nprec, mode);
                if (n.Right != null)
                {
                    mode.Fprintf(s, ".(%v)", n.Right);
                    return ;
                }

                mode.Fprintf(s, ".(%v)", n.Type);
                goto __switch_break1;
            }
            if (n.Op == OINDEX || n.Op == OINDEXMAP)
            {
                n.Left.exprfmt(s, nprec, mode);
                mode.Fprintf(s, "[%v]", n.Right);
                goto __switch_break1;
            }
            if (n.Op == OSLICE || n.Op == OSLICESTR || n.Op == OSLICEARR || n.Op == OSLICE3 || n.Op == OSLICE3ARR)
            {
                n.Left.exprfmt(s, nprec, mode);
                fmt.Fprint(s, "[");
                var (low, high, max) = n.SliceBounds();
                if (low != null)
                {
                    fmt.Fprint(s, low.modeString(mode));
                }

                fmt.Fprint(s, ":");
                if (high != null)
                {
                    fmt.Fprint(s, high.modeString(mode));
                }

                if (n.Op.IsSlice3())
                {
                    fmt.Fprint(s, ":");
                    if (max != null)
                    {
                        fmt.Fprint(s, max.modeString(mode));
                    }

                }

                fmt.Fprint(s, "]");
                goto __switch_break1;
            }
            if (n.Op == OSLICEHEADER)
            {
                if (n.List.Len() != 2L)
                {
                    Fatalf("bad OSLICEHEADER list length %d", n.List.Len());
                }

                mode.Fprintf(s, "sliceheader{%v,%v,%v}", n.Left, n.List.First(), n.List.Second());
                goto __switch_break1;
            }
            if (n.Op == OCOMPLEX || n.Op == OCOPY)
            {
                if (n.Left != null)
                {
                    mode.Fprintf(s, "%#v(%v, %v)", n.Op, n.Left, n.Right);
                }
                else
                {
                    mode.Fprintf(s, "%#v(%.v)", n.Op, n.List);
                }

                goto __switch_break1;
            }
            if (n.Op == OCONV || n.Op == OCONVIFACE || n.Op == OCONVNOP || n.Op == OBYTES2STR || n.Op == ORUNES2STR || n.Op == OSTR2BYTES || n.Op == OSTR2RUNES || n.Op == ORUNESTR)
            {
                if (n.Type == null || n.Type.Sym == null)
                {
                    mode.Fprintf(s, "(%v)", n.Type);
                }
                else
                {
                    mode.Fprintf(s, "%v", n.Type);
                }

                if (n.Left != null)
                {
                    mode.Fprintf(s, "(%v)", n.Left);
                }
                else
                {
                    mode.Fprintf(s, "(%.v)", n.List);
                }

                goto __switch_break1;
            }
            if (n.Op == OREAL || n.Op == OIMAG || n.Op == OAPPEND || n.Op == OCAP || n.Op == OCLOSE || n.Op == ODELETE || n.Op == OLEN || n.Op == OMAKE || n.Op == ONEW || n.Op == OPANIC || n.Op == ORECOVER || n.Op == OALIGNOF || n.Op == OOFFSETOF || n.Op == OSIZEOF || n.Op == OPRINT || n.Op == OPRINTN)
            {
                if (n.Left != null)
                {
                    mode.Fprintf(s, "%#v(%v)", n.Op, n.Left);
                    return ;
                }

                if (n.IsDDD())
                {
                    mode.Fprintf(s, "%#v(%.v...)", n.Op, n.List);
                    return ;
                }

                mode.Fprintf(s, "%#v(%.v)", n.Op, n.List);
                goto __switch_break1;
            }
            if (n.Op == OCALL || n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OCALLMETH || n.Op == OGETG)
            {
                n.Left.exprfmt(s, nprec, mode);
                if (n.IsDDD())
                {
                    mode.Fprintf(s, "(%.v...)", n.List);
                    return ;
                }

                mode.Fprintf(s, "(%.v)", n.List);
                goto __switch_break1;
            }
            if (n.Op == OMAKEMAP || n.Op == OMAKECHAN || n.Op == OMAKESLICE)
            {
                if (n.List.Len() != 0L)
                { // pre-typecheck
                    mode.Fprintf(s, "make(%v, %.v)", n.Type, n.List);
                    return ;

                }

                if (n.Right != null)
                {
                    mode.Fprintf(s, "make(%v, %v, %v)", n.Type, n.Left, n.Right);
                    return ;
                }

                if (n.Left != null && (n.Op == OMAKESLICE || !n.Left.Type.IsUntyped()))
                {
                    mode.Fprintf(s, "make(%v, %v)", n.Type, n.Left);
                    return ;
                }

                mode.Fprintf(s, "make(%v)", n.Type);
                goto __switch_break1;
            }
            if (n.Op == OMAKESLICECOPY)
            {
                mode.Fprintf(s, "makeslicecopy(%v, %v, %v)", n.Type, n.Left, n.Right);
                goto __switch_break1;
            }
            if (n.Op == OPLUS || n.Op == ONEG || n.Op == OADDR || n.Op == OBITNOT || n.Op == ODEREF || n.Op == ONOT || n.Op == ORECV) 
            {
                // Unary
                mode.Fprintf(s, "%#v", n.Op);
                if (n.Left != null && n.Left.Op == n.Op)
                {
                    fmt.Fprint(s, " ");
                }

                n.Left.exprfmt(s, nprec + 1L, mode); 

                // Binary
                goto __switch_break1;
            }
            if (n.Op == OADD || n.Op == OAND || n.Op == OANDAND || n.Op == OANDNOT || n.Op == ODIV || n.Op == OEQ || n.Op == OGE || n.Op == OGT || n.Op == OLE || n.Op == OLT || n.Op == OLSH || n.Op == OMOD || n.Op == OMUL || n.Op == ONE || n.Op == OOR || n.Op == OOROR || n.Op == ORSH || n.Op == OSEND || n.Op == OSUB || n.Op == OXOR)
            {
                n.Left.exprfmt(s, nprec, mode);
                mode.Fprintf(s, " %#v ", n.Op);
                n.Right.exprfmt(s, nprec + 1L, mode);
                goto __switch_break1;
            }
            if (n.Op == OADDSTR)
            {
                foreach (var (i, n1) in n.List.Slice())
                {
                    if (i != 0L)
                    {
                        fmt.Fprint(s, " + ");
                    }

                    n1.exprfmt(s, nprec, mode);

                }
                goto __switch_break1;
            }
            // default: 
                mode.Fprintf(s, "<node %v>", n.Op);

            __switch_break1:;

        }

        private static void nodefmt(this ptr<Node> _addr_n, fmt.State s, FmtFlag flag, fmtMode mode)
        {
            ref Node n = ref _addr_n.val;

            var t = n.Type; 

            // We almost always want the original.
            // TODO(gri) Why the special case for OLITERAL?
            if (n.Op != OLITERAL && n.Orig != null)
            {
                n = n.Orig;
            }

            if (flag & FmtLong != 0L && t != null)
            {
                if (t.Etype == TNIL)
                {
                    fmt.Fprint(s, "nil");
                }
                else if (n.Op == ONAME && n.Name.AutoTemp())
                {
                    mode.Fprintf(s, "%v value", t);
                }
                else
                {
                    mode.Fprintf(s, "%v (type %v)", n, t);
                }

                return ;

            } 

            // TODO inlining produces expressions with ninits. we can't print these yet.
            if (opprec[n.Op] < 0L)
            {
                n.stmtfmt(s, mode);
                return ;
            }

            n.exprfmt(s, 0L, mode);

        }

        private static void nodedump(this ptr<Node> _addr_n, fmt.State s, FmtFlag flag, fmtMode mode)
        {
            ref Node n = ref _addr_n.val;

            var recur = flag & FmtShort == 0L;

            if (recur)
            {
                indent(s);
                if (dumpdepth > 40L)
                {
                    fmt.Fprint(s, "...");
                    return ;
                }

                if (n.Ninit.Len() != 0L)
                {
                    mode.Fprintf(s, "%v-init%v", n.Op, n.Ninit);
                    indent(s);
                }

            }


            if (n.Op == OLITERAL) 
                mode.Fprintf(s, "%v-%v%j", n.Op, n.Val(), n);
            else if (n.Op == ONAME || n.Op == ONONAME) 
                if (n.Sym != null)
                {
                    mode.Fprintf(s, "%v-%v%j", n.Op, n.Sym, n);
                }
                else
                {
                    mode.Fprintf(s, "%v%j", n.Op, n);
                }

                if (recur && n.Type == null && n.Name != null && n.Name.Param != null && n.Name.Param.Ntype != null)
                {
                    indent(s);
                    mode.Fprintf(s, "%v-ntype%v", n.Op, n.Name.Param.Ntype);
                }

            else if (n.Op == OASOP) 
                mode.Fprintf(s, "%v-%v%j", n.Op, n.SubOp(), n);
            else if (n.Op == OTYPE) 
                mode.Fprintf(s, "%v %v%j type=%v", n.Op, n.Sym, n, n.Type);
                if (recur && n.Type == null && n.Name != null && n.Name.Param != null && n.Name.Param.Ntype != null)
                {
                    indent(s);
                    mode.Fprintf(s, "%v-ntype%v", n.Op, n.Name.Param.Ntype);
                }

            else 
                mode.Fprintf(s, "%v%j", n.Op, n);
                        if (n.Sym != null && n.Op != ONAME)
            {
                mode.Fprintf(s, " %v", n.Sym);
            }

            if (n.Type != null)
            {
                mode.Fprintf(s, " %v", n.Type);
            }

            if (recur)
            {
                if (n.Left != null)
                {
                    mode.Fprintf(s, "%v", n.Left);
                }

                if (n.Right != null)
                {
                    mode.Fprintf(s, "%v", n.Right);
                }

                if (n.List.Len() != 0L)
                {
                    indent(s);
                    mode.Fprintf(s, "%v-list%v", n.Op, n.List);
                }

                if (n.Rlist.Len() != 0L)
                {
                    indent(s);
                    mode.Fprintf(s, "%v-rlist%v", n.Op, n.Rlist);
                }

                if (n.Nbody.Len() != 0L)
                {
                    indent(s);
                    mode.Fprintf(s, "%v-body%v", n.Op, n.Nbody);
                }

            }

        }

        // "%S" suppresses qualifying with package
        private static void symFormat(ptr<types.Sym> _addr_s, fmt.State f, int verb, fmtMode mode)
        {
            ref types.Sym s = ref _addr_s.val;

            switch (verb)
            {
                case 'v': 

                case 'S': 
                    fmt.Fprint(f, sconv(_addr_s, fmtFlag(f, verb), mode));
                    break;
                default: 
                    fmt.Fprintf(f, "%%!%c(*types.Sym=%p)", verb, s);
                    break;
            }

        }

        private static @string smodeString(ptr<types.Sym> _addr_s, fmtMode mode)
        {
            ref types.Sym s = ref _addr_s.val;

            return sconv(_addr_s, 0L, mode);
        }

        // See #16897 before changing the implementation of sconv.
        private static @string sconv(ptr<types.Sym> _addr_s, FmtFlag flag, fmtMode mode) => func((defer, panic, _) =>
        {
            ref types.Sym s = ref _addr_s.val;

            if (flag & FmtLong != 0L)
            {
                panic("linksymfmt");
            }

            if (s == null)
            {
                return "<S>";
            }

            if (s.Name == "_")
            {
                return "_";
            }

            ptr<bytes.Buffer> buf = fmtBufferPool.Get()._<ptr<bytes.Buffer>>();
            buf.Reset();
            defer(fmtBufferPool.Put(buf));

            flag, mode = flag.update(mode);
            symfmt(buf, _addr_s, flag, mode);
            return types.InternString(buf.Bytes());

        });

        private static void sconv2(ptr<bytes.Buffer> _addr_b, ptr<types.Sym> _addr_s, FmtFlag flag, fmtMode mode) => func((_, panic, __) =>
        {
            ref bytes.Buffer b = ref _addr_b.val;
            ref types.Sym s = ref _addr_s.val;

            if (flag & FmtLong != 0L)
            {
                panic("linksymfmt");
            }

            if (s == null)
            {
                b.WriteString("<S>");
                return ;
            }

            if (s.Name == "_")
            {
                b.WriteString("_");
                return ;
            }

            flag, mode = flag.update(mode);
            symfmt(_addr_b, _addr_s, flag, mode);

        });

        private static void fldconv(ptr<bytes.Buffer> _addr_b, ptr<types.Field> _addr_f, FmtFlag flag, fmtMode mode, map<ptr<types.Type>, long> visited, types.Funarg funarg)
        {
            ref bytes.Buffer b = ref _addr_b.val;
            ref types.Field f = ref _addr_f.val;

            if (f == null)
            {
                b.WriteString("<T>");
                return ;
            }

            flag, mode = flag.update(mode);
            if (mode == FTypeIdName)
            {
                flag |= FmtUnsigned;
            }

            @string name = default;
            if (flag & FmtShort == 0L)
            {
                var s = f.Sym; 

                // Take the name from the original.
                if (mode == FErr)
                {
                    s = origSym(s);
                }

                if (s != null && f.Embedded == 0L)
                {
                    if (funarg != types.FunargNone)
                    {
                        name = asNode(f.Nname).modeString(mode);
                    }
                    else if (flag & FmtLong != 0L)
                    {
                        name = mode.Sprintf("%0S", s);
                        if (!types.IsExported(name) && flag & FmtUnsigned == 0L)
                        {
                            name = smodeString(_addr_s, mode); // qualify non-exported names (used on structs, not on funarg)
                        }

                    }
                    else
                    {
                        name = smodeString(_addr_s, mode);
                    }

                }

            }

            if (name != "")
            {
                b.WriteString(name);
                b.WriteString(" ");
            }

            if (f.IsDDD())
            {
                ptr<types.Type> et;
                if (f.Type != null)
                {
                    et = f.Type.Elem();
                }

                b.WriteString("...");
                tconv2(_addr_b, et, 0L, mode, visited);

            }
            else
            {
                tconv2(_addr_b, _addr_f.Type, 0L, mode, visited);
            }

            if (flag & FmtShort == 0L && funarg == types.FunargNone && f.Note != "")
            {
                b.WriteString(" ");
                b.WriteString(strconv.Quote(f.Note));
            }

        }

        // "%L"  print definition, not name
        // "%S"  omit 'func' and receiver from function types, short type names
        private static void typeFormat(ptr<types.Type> _addr_t, fmt.State s, int verb, fmtMode mode)
        {
            ref types.Type t = ref _addr_t.val;

            switch (verb)
            {
                case 'v': 

                case 'S': 

                case 'L': 
                    fmt.Fprint(s, tconv(_addr_t, fmtFlag(s, verb), mode));
                    break;
                default: 
                    fmt.Fprintf(s, "%%!%c(*Type=%p)", verb, t);
                    break;
            }

        }

        private static @string String(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return fmt.Sprint(n);
        }
        private static @string modeString(this ptr<Node> _addr_n, fmtMode mode)
        {
            ref Node n = ref _addr_n.val;

            return mode.Sprint(n);
        }

        // "%L"  suffix with "(type %T)" where possible
        // "%+S" in debug mode, don't recurse, no multiline output
        private static void nconv(this ptr<Node> _addr_n, fmt.State s, FmtFlag flag, fmtMode mode)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                fmt.Fprint(s, "<N>");
                return ;
            }

            flag, mode = flag.update(mode);


            if (mode == FErr) 
                n.nodefmt(s, flag, mode);
            else if (mode == FDbg) 
                dumpdepth++;
                n.nodedump(s, flag, mode);
                dumpdepth--;
            else 
                Fatalf("unhandled %%N mode: %d", mode);
            
        }

        public static void format(this Nodes l, fmt.State s, int verb, fmtMode mode)
        {
            switch (verb)
            {
                case 'v': 
                    l.hconv(s, fmtFlag(s, verb), mode);
                    break;
                default: 
                    fmt.Fprintf(s, "%%!%c(Nodes)", verb);
                    break;
            }

        }

        public static @string String(this Nodes n)
        {
            return fmt.Sprint(n);
        }

        // Flags: all those of %N plus '.': separate with comma's instead of semicolons.
        public static void hconv(this Nodes l, fmt.State s, FmtFlag flag, fmtMode mode)
        {
            if (l.Len() == 0L && mode == FDbg)
            {
                fmt.Fprint(s, "<nil>");
                return ;
            }

            flag, mode = flag.update(mode);
            @string sep = "; ";
            if (mode == FDbg)
            {
                sep = "\n";
            }
            else if (flag & FmtComma != 0L)
            {
                sep = ", ";
            }

            foreach (var (i, n) in l.Slice())
            {
                fmt.Fprint(s, n.modeString(mode));
                if (i + 1L < l.Len())
                {
                    fmt.Fprint(s, sep);
                }

            }

        }

        private static void dumplist(@string s, Nodes l)
        {
            fmt.Printf("%s%+v\n", s, l);
        }

        private static void fdumplist(io.Writer w, @string s, Nodes l)
        {
            fmt.Fprintf(w, "%s%+v\n", s, l);
        }

        public static void Dump(@string s, ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            fmt.Printf("%s [%p]%+v\n", s, n, n);
        }

        // TODO(gri) make variable local somehow
        private static long dumpdepth = default;

        // indent prints indentation to s.
        private static void indent(fmt.State s)
        {
            fmt.Fprint(s, "\n");
            for (long i = 0L; i < dumpdepth; i++)
            {
                fmt.Fprint(s, ".   ");
            }


        }
    }
}}}}
