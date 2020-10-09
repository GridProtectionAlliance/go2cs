// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:43:18 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\subr.go
using types = go.cmd.compile.@internal.types_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using md5 = go.crypto.md5_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using os = go.os_package;
using debug = go.runtime.debug_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using unicode = go.unicode_package;
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
        public partial struct Error
        {
            public src.XPos pos;
            public @string msg;
        }

        private static slice<Error> errors = default;

        // largeStack is info about a function whose stack frame is too large (rare).
        private partial struct largeStack
        {
            public long locals;
            public long args;
            public long callee;
            public src.XPos pos;
        }

        private static sync.Mutex largeStackFramesMu = default;        private static slice<largeStack> largeStackFrames = default;

        private static void errorexit()
        {
            flusherrors();
            if (outfile != "")
            {
                os.Remove(outfile);
            }

            os.Exit(2L);

        }

        private static void adderrorname(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != ODOT)
            {
                return ;
            }

            var old = fmt.Sprintf("%v: undefined: %v\n", n.Line(), n.Left);
            if (len(errors) > 0L && errors[len(errors) - 1L].pos.Line() == n.Pos.Line() && errors[len(errors) - 1L].msg == old)
            {
                errors[len(errors) - 1L].msg = fmt.Sprintf("%v: undefined: %v in %v\n", n.Line(), n.Left, n);
            }

        }

        private static void adderr(src.XPos pos, @string format, params object[] args)
        {
            args = args.Clone();

            var msg = fmt.Sprintf(format, args); 
            // Only add the position if know the position.
            // See issue golang.org/issue/11361.
            if (pos.IsKnown())
            {
                msg = fmt.Sprintf("%v: %s", linestr(pos), msg);
            }

            errors = append(errors, new Error(pos:pos,msg:msg+"\n",));

        }

        // byPos sorts errors by source position.
        private partial struct byPos // : slice<Error>
        {
        }

        private static long Len(this byPos x)
        {
            return len(x);
        }
        private static bool Less(this byPos x, long i, long j)
        {
            return x[i].pos.Before(x[j].pos);
        }
        private static void Swap(this byPos x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];
        }

        // flusherrors sorts errors seen so far by line number, prints them to stdout,
        // and empties the errors array.
        private static void flusherrors()
        {
            Ctxt.Bso.Flush();
            if (len(errors) == 0L)
            {
                return ;
            }

            sort.Stable(byPos(errors));
            foreach (var (i, err) in errors)
            {
                if (i == 0L || err.msg != errors[i - 1L].msg)
                {
                    fmt.Printf("%s", err.msg);
                }

            }
            errors = errors[..0L];

        }

        private static void hcrash()
        {
            if (Debug['h'] != 0L)
            {
                flusherrors();
                if (outfile != "")
                {
                    os.Remove(outfile);
                }

                ptr<long> x;
                x.val = 0L;

            }

        }

        private static @string linestr(src.XPos pos)
        {
            return Ctxt.OutermostPos(pos).Format(Debug['C'] == 0L, Debug['L'] == 1L);
        }

        // lasterror keeps track of the most recently issued error.
        // It is used to avoid multiple error messages on the same
        // line.
        private static var lasterror = default;

        // sameline reports whether two positions a, b are on the same line.
        private static bool sameline(src.XPos a, src.XPos b)
        {
            var p = Ctxt.PosTable.Pos(a);
            var q = Ctxt.PosTable.Pos(b);
            return p.Base() == q.Base() && p.Line() == q.Line();
        }

        private static void yyerrorl(src.XPos pos, @string format, params object[] args)
        {
            args = args.Clone();

            var msg = fmt.Sprintf(format, args);

            if (strings.HasPrefix(msg, "syntax error"))
            {
                nsyntaxerrors++; 
                // only one syntax error per line, no matter what error
                if (sameline(lasterror.syntax, pos))
                {
                    return ;
                }

                lasterror.syntax = pos;

            }
            else
            { 
                // only one of multiple equal non-syntax errors per line
                // (flusherrors shows only one of them, so we filter them
                // here as best as we can (they may not appear in order)
                // so that we don't count them here and exit early, and
                // then have nothing to show for.)
                if (sameline(lasterror.other, pos) && lasterror.msg == msg)
                {
                    return ;
                }

                lasterror.other = pos;
                lasterror.msg = msg;

            }

            adderr(pos, "%s", msg);

            hcrash();
            nerrors++;
            if (nsavederrors + nerrors >= 10L && Debug['e'] == 0L)
            {
                flusherrors();
                fmt.Printf("%v: too many errors\n", linestr(pos));
                errorexit();
            }

        }

        private static void yyerrorv(@string lang, @string format, params object[] args)
        {
            args = args.Clone();

            var what = fmt.Sprintf(format, args);
            yyerrorl(lineno, "%s requires %s or later (-lang was set to %s; check go.mod)", what, lang, flag_lang);
        }

        private static void yyerror(@string format, params object[] args)
        {
            args = args.Clone();

            yyerrorl(lineno, format, args);
        }

        public static void Warn(@string fmt_, params object[] args)
        {
            args = args.Clone();

            Warnl(lineno, fmt_, args);
        }

        public static void Warnl(src.XPos line, @string fmt_, params object[] args)
        {
            args = args.Clone();

            adderr(line, fmt_, args);
            if (Debug['m'] != 0L)
            {
                flusherrors();
            }

        }

        public static void Fatalf(@string fmt_, params object[] args)
        {
            args = args.Clone();

            flusherrors();

            if (Debug_panic != 0L || nsavederrors + nerrors == 0L)
            {
                fmt.Printf("%v: internal compiler error: ", linestr(lineno));
                fmt.Printf(fmt_, args);
                fmt.Printf("\n"); 

                // If this is a released compiler version, ask for a bug report.
                if (strings.HasPrefix(objabi.Version, "go"))
                {
                    fmt.Printf("\n");
                    fmt.Printf("Please file a bug report including a short program that triggers the error.\n");
                    fmt.Printf("https://golang.org/issue/new\n");
                }
                else
                { 
                    // Not a release; dump a stack trace, too.
                    fmt.Println();
                    os.Stdout.Write(debug.Stack());
                    fmt.Println();

                }

            }

            hcrash();
            errorexit();

        }

        // hasUniquePos reports whether n has a unique position that can be
        // used for reporting error messages.
        //
        // It's primarily used to distinguish references to named objects,
        // whose Pos will point back to their declaration position rather than
        // their usage position.
        private static bool hasUniquePos(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;


            if (n.Op == ONAME || n.Op == OPACK) 
                return false;
            else if (n.Op == OLITERAL || n.Op == OTYPE) 
                if (n.Sym != null)
                {
                    return false;
                }

                        if (!n.Pos.IsKnown())
            {
                if (Debug['K'] != 0L)
                {
                    Warn("setlineno: unknown position (line 0)");
                }

                return false;

            }

            return true;

        }

        private static src.XPos setlineno(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var lno = lineno;
            if (n != null && hasUniquePos(_addr_n))
            {
                lineno = n.Pos;
            }

            return lno;

        }

        private static ptr<types.Sym> lookup(@string name)
        {
            return _addr_localpkg.Lookup(name)!;
        }

        // lookupN looks up the symbol starting with prefix and ending with
        // the decimal n. If prefix is too long, lookupN panics.
        private static ptr<types.Sym> lookupN(@string prefix, long n)
        {
            array<byte> buf = new array<byte>(20L); // plenty long enough for all current users
            copy(buf[..], prefix);
            var b = strconv.AppendInt(buf[..len(prefix)], int64(n), 10L);
            return _addr_localpkg.LookupBytes(b)!;

        }

        // autolabel generates a new Name node for use with
        // an automatically generated label.
        // prefix is a short mnemonic (e.g. ".s" for switch)
        // to help with debugging.
        // It should begin with "." to avoid conflicts with
        // user labels.
        private static ptr<types.Sym> autolabel(@string prefix)
        {
            if (prefix[0L] != '.')
            {
                Fatalf("autolabel prefix must start with '.', have %q", prefix);
            }

            var fn = Curfn;
            if (Curfn == null)
            {
                Fatalf("autolabel outside function");
            }

            var n = fn.Func.Label;
            fn.Func.Label++;
            return _addr_lookupN(prefix, int(n))!;

        }

        private static ptr<types.Sym> restrictlookup(@string name, ptr<types.Pkg> _addr_pkg)
        {
            ref types.Pkg pkg = ref _addr_pkg.val;

            if (!types.IsExported(name) && pkg != localpkg)
            {
                yyerror("cannot refer to unexported name %s.%s", pkg.Name, name);
            }

            return _addr_pkg.Lookup(name)!;

        }

        // find all the exported symbols in package opkg
        // and make them available in the current package
        private static void importdot(ptr<types.Pkg> _addr_opkg, ptr<Node> _addr_pack)
        {
            ref types.Pkg opkg = ref _addr_opkg.val;
            ref Node pack = ref _addr_pack.val;

            long n = 0L;
            foreach (var (_, s) in opkg.Syms)
            {
                if (s.Def == null)
                {
                    continue;
                }

                if (!types.IsExported(s.Name) || strings.ContainsRune(s.Name, 0xb7UL))
                { // 0xb7 = center dot
                    continue;

                }

                var s1 = lookup(s.Name);
                if (s1.Def != null)
                {
                    var pkgerror = fmt.Sprintf("during import %q", opkg.Path);
                    redeclare(lineno, s1, pkgerror);
                    continue;
                }

                s1.Def = s.Def;
                s1.Block = s.Block;
                if (asNode(s1.Def).Name == null)
                {
                    Dump("s1def", asNode(s1.Def));
                    Fatalf("missing Name");
                }

                asNode(s1.Def).Name.Pack;

                pack;
                s1.Origpkg = opkg;
                n++;

            }
            if (n == 0L)
            { 
                // can't possibly be used - there were no symbols
                yyerrorl(pack.Pos, "imported and not used: %q", opkg.Path);

            }

        }

        private static ptr<Node> nod(Op op, ptr<Node> _addr_nleft, ptr<Node> _addr_nright)
        {
            ref Node nleft = ref _addr_nleft.val;
            ref Node nright = ref _addr_nright.val;

            return _addr_nodl(lineno, op, _addr_nleft, _addr_nright)!;
        }

        private static ptr<Node> nodl(src.XPos pos, Op op, ptr<Node> _addr_nleft, ptr<Node> _addr_nright)
        {
            ref Node nleft = ref _addr_nleft.val;
            ref Node nright = ref _addr_nright.val;

            ptr<Node> n;

            if (op == OCLOSURE || op == ODCLFUNC) 
                var x = default;
                n = _addr_x.n;
                n.Func = _addr_x.f;
            else if (op == ONAME) 
                Fatalf("use newname instead");
            else if (op == OLABEL || op == OPACK) 
                x = default;
                n = _addr_x.n;
                n.Name = _addr_x.m;
            else 
                n = @new<Node>();
                        n.Op = op;
            n.Left = nleft;
            n.Right = nright;
            n.Pos = pos;
            n.Xoffset = BADWIDTH;
            n.Orig = n;
            return _addr_n!;

        }

        // newname returns a new ONAME Node associated with symbol s.
        private static ptr<Node> newname(ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;

            var n = newnamel(lineno, _addr_s);
            n.Name.Curfn = Curfn;
            return _addr_n!;
        }

        // newname returns a new ONAME Node associated with symbol s at position pos.
        // The caller is responsible for setting n.Name.Curfn.
        private static ptr<Node> newnamel(src.XPos pos, ptr<types.Sym> _addr_s)
        {
            ref types.Sym s = ref _addr_s.val;

            if (s == null)
            {
                Fatalf("newnamel nil");
            }

            var x = default;
            var n = _addr_x.n;
            n.Name = _addr_x.m;
            n.Name.Param = _addr_x.p;

            n.Op = ONAME;
            n.Pos = pos;
            n.Orig = n;

            n.Sym = s;
            return _addr_n!;

        }

        // nodSym makes a Node with Op op and with the Left field set to left
        // and the Sym field set to sym. This is for ODOT and friends.
        private static ptr<Node> nodSym(Op op, ptr<Node> _addr_left, ptr<types.Sym> _addr_sym)
        {
            ref Node left = ref _addr_left.val;
            ref types.Sym sym = ref _addr_sym.val;

            return _addr_nodlSym(lineno, op, _addr_left, _addr_sym)!;
        }

        // nodlSym makes a Node with position Pos, with Op op, and with the Left field set to left
        // and the Sym field set to sym. This is for ODOT and friends.
        private static ptr<Node> nodlSym(src.XPos pos, Op op, ptr<Node> _addr_left, ptr<types.Sym> _addr_sym)
        {
            ref Node left = ref _addr_left.val;
            ref types.Sym sym = ref _addr_sym.val;

            var n = nodl(pos, op, _addr_left, _addr_null);
            n.Sym = sym;
            return _addr_n!;
        }

        // rawcopy returns a shallow copy of n.
        // Note: copy or sepcopy (rather than rawcopy) is usually the
        //       correct choice (see comment with Node.copy, below).
        private static ptr<Node> rawcopy(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            ref var copy = ref heap(n.val, out ptr<var> _addr_copy);
            return _addr__addr_copy!;
        }

        // sepcopy returns a separate shallow copy of n, with the copy's
        // Orig pointing to itself.
        private static ptr<Node> sepcopy(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            ref var copy = ref heap(n.val, out ptr<var> _addr_copy);
            _addr_copy.Orig = _addr_copy;
            copy.Orig = ref _addr_copy.Orig.val;
            return _addr__addr_copy!;

        }

        // copy returns shallow copy of n and adjusts the copy's Orig if
        // necessary: In general, if n.Orig points to itself, the copy's
        // Orig should point to itself as well. Otherwise, if n is modified,
        // the copy's Orig node appears modified, too, and then doesn't
        // represent the original node anymore.
        // (This caused the wrong complit Op to be used when printing error
        // messages; see issues #26855, #27765).
        private static ptr<Node> copy(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            ref var copy = ref heap(n.val, out ptr<var> _addr_copy);
            if (n.Orig == n)
            {
                _addr_copy.Orig = _addr_copy;
                copy.Orig = ref _addr_copy.Orig.val;

            }

            return _addr__addr_copy!;

        }

        // methcmp sorts methods by symbol.
        private partial struct methcmp // : slice<ptr<types.Field>>
        {
        }

        private static long Len(this methcmp x)
        {
            return len(x);
        }
        private static void Swap(this methcmp x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];
        }
        private static bool Less(this methcmp x, long i, long j)
        {
            return x[i].Sym.Less(x[j].Sym);
        }

        private static ptr<Node> nodintconst(long v)
        {
            ptr<object> u = @new<Mpint>();
            u.SetInt64(v);
            return _addr_nodlit(new Val(u))!;
        }

        private static ptr<Node> nodnil()
        {
            return _addr_nodlit(new Val(new(NilVal)))!;
        }

        private static ptr<Node> nodbool(bool b)
        {
            return _addr_nodlit(new Val(b))!;
        }

        private static ptr<Node> nodstr(@string s)
        {
            return _addr_nodlit(new Val(s))!;
        }

        // treecopy recursively copies n, with the exception of
        // ONAME, OLITERAL, OTYPE, and ONONAME leaves.
        // If pos.IsKnown(), it sets the source position of newly
        // allocated nodes to pos.
        private static ptr<Node> treecopy(ptr<Node> _addr_n, src.XPos pos)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return _addr_null!;
            }


            if (n.Op == OPACK) 
            {
                // OPACK nodes are never valid in const value declarations,
                // but allow them like any other declared symbol to avoid
                // crashing (golang.org/issue/11361).
                fallthrough = true;

            }
            if (fallthrough || n.Op == ONAME || n.Op == ONONAME || n.Op == OLITERAL || n.Op == OTYPE)
            {
                return _addr_n!;
                goto __switch_break0;
            }
            // default: 
                var m = n.sepcopy();
                m.Left = treecopy(_addr_n.Left, pos);
                m.Right = treecopy(_addr_n.Right, pos);
                m.List.Set(listtreecopy(n.List.Slice(), pos));
                if (pos.IsKnown())
                {
                    m.Pos = pos;
                }

                if (m.Name != null && n.Op != ODCLFIELD)
                {
                    Dump("treecopy", n);
                    Fatalf("treecopy Name");
                }

                return _addr_m!;

            __switch_break0:;

        }

        // isNil reports whether n represents the universal untyped zero value "nil".
        private static bool isNil(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;
 
            // Check n.Orig because constant propagation may produce typed nil constants,
            // which don't exist in the Go spec.
            return Isconst(n.Orig, CTNIL);

        }

        private static bool isptrto(ptr<types.Type> _addr_t, types.EType et)
        {
            ref types.Type t = ref _addr_t.val;

            if (t == null)
            {
                return false;
            }

            if (!t.IsPtr())
            {
                return false;
            }

            t = t.Elem();
            if (t == null)
            {
                return false;
            }

            if (t.Etype != et)
            {
                return false;
            }

            return true;

        }

        private static bool isBlank(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return false;
            }

            return n.Sym.IsBlank();

        }

        // methtype returns the underlying type, if any,
        // that owns methods with receiver parameter t.
        // The result is either a named type or an anonymous struct.
        private static ptr<types.Type> methtype(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t == null)
            {
                return _addr_null!;
            } 

            // Strip away pointer if it's there.
            if (t.IsPtr())
            {
                if (t.Sym != null)
                {
                    return _addr_null!;
                }

                t = t.Elem();
                if (t == null)
                {
                    return _addr_null!;
                }

            } 

            // Must be a named type or anonymous struct.
            if (t.Sym == null && !t.IsStruct())
            {
                return _addr_null!;
            } 

            // Check types.
            if (issimple[t.Etype])
            {
                return _addr_t!;
            }


            if (t.Etype == TARRAY || t.Etype == TCHAN || t.Etype == TFUNC || t.Etype == TMAP || t.Etype == TSLICE || t.Etype == TSTRING || t.Etype == TSTRUCT) 
                return _addr_t!;
                        return _addr_null!;

        }

        // Is type src assignment compatible to type dst?
        // If so, return op code to use in conversion.
        // If not, return OXXX.
        private static Op assignop(ptr<types.Type> _addr_src, ptr<types.Type> _addr_dst, ptr<@string> _addr_why)
        {
            ref types.Type src = ref _addr_src.val;
            ref types.Type dst = ref _addr_dst.val;
            ref @string why = ref _addr_why.val;

            if (why != null)
            {
                why = "";
            }

            if (src == dst)
            {
                return OCONVNOP;
            }

            if (src == null || dst == null || src.Etype == TFORW || dst.Etype == TFORW || src.Orig == null || dst.Orig == null)
            {
                return OXXX;
            } 

            // 1. src type is identical to dst.
            if (types.Identical(src, dst))
            {
                return OCONVNOP;
            } 

            // 2. src and dst have identical underlying types
            // and either src or dst is not a named type or
            // both are empty interface types.
            // For assignable but different non-empty interface types,
            // we want to recompute the itab. Recomputing the itab ensures
            // that itabs are unique (thus an interface with a compile-time
            // type I has an itab with interface type I).
            if (types.Identical(src.Orig, dst.Orig))
            {
                if (src.IsEmptyInterface())
                { 
                    // Conversion between two empty interfaces
                    // requires no code.
                    return OCONVNOP;

                }

                if ((src.Sym == null || dst.Sym == null) && !src.IsInterface())
                { 
                    // Conversion between two types, at least one unnamed,
                    // needs no conversion. The exception is nonempty interfaces
                    // which need to have their itab updated.
                    return OCONVNOP;

                }

            } 

            // 3. dst is an interface type and src implements dst.
            if (dst.IsInterface() && src.Etype != TNIL)
            {
                ptr<types.Field> missing;                ptr<types.Field> have;

                ref long ptr = ref heap(out ptr<long> _addr_ptr);
                if (implements(_addr_src, _addr_dst, _addr_missing, _addr_have, _addr_ptr))
                {
                    return OCONVIFACE;
                } 

                // we'll have complained about this method anyway, suppress spurious messages.
                if (have != null && have.Sym == missing.Sym && (have.Type.Broke() || missing.Type.Broke()))
                {
                    return OCONVIFACE;
                }

                if (why != null)
                {
                    if (isptrto(_addr_src, TINTER))
                    {
                        why = fmt.Sprintf(":\n\t%v is pointer to interface, not interface", src);
                    }
                    else if (have != null && have.Sym == missing.Sym && have.Nointerface())
                    {
                        why = fmt.Sprintf(":\n\t%v does not implement %v (%v method is marked 'nointerface')", src, dst, missing.Sym);
                    }
                    else if (have != null && have.Sym == missing.Sym)
                    {
                        why = fmt.Sprintf(":\n\t%v does not implement %v (wrong type for %v method)\n" + "\t\thave %v%0S\n\t\twant %v%0S", src, dst, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
                    }
                    else if (ptr != 0L)
                    {
                        why = fmt.Sprintf(":\n\t%v does not implement %v (%v method has pointer receiver)", src, dst, missing.Sym);
                    }
                    else if (have != null)
                    {
                        why = fmt.Sprintf(":\n\t%v does not implement %v (missing %v method)\n" + "\t\thave %v%0S\n\t\twant %v%0S", src, dst, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
                    }
                    else
                    {
                        why = fmt.Sprintf(":\n\t%v does not implement %v (missing %v method)", src, dst, missing.Sym);
                    }

                }

                return OXXX;

            }

            if (isptrto(_addr_dst, TINTER))
            {
                if (why != null)
                {
                    why = fmt.Sprintf(":\n\t%v is pointer to interface, not interface", dst);
                }

                return OXXX;

            }

            if (src.IsInterface() && dst.Etype != TBLANK)
            {
                missing = ;                have = ;

                ptr = default;
                if (why != null && implements(_addr_dst, _addr_src, _addr_missing, _addr_have, _addr_ptr))
                {
                    why = ": need type assertion";
                }

                return OXXX;

            } 

            // 4. src is a bidirectional channel value, dst is a channel type,
            // src and dst have identical element types, and
            // either src or dst is not a named type.
            if (src.IsChan() && src.ChanDir() == types.Cboth && dst.IsChan())
            {
                if (types.Identical(src.Elem(), dst.Elem()) && (src.Sym == null || dst.Sym == null))
                {
                    return OCONVNOP;
                }

            } 

            // 5. src is the predeclared identifier nil and dst is a nillable type.
            if (src.Etype == TNIL)
            {

                if (dst.Etype == TPTR || dst.Etype == TFUNC || dst.Etype == TMAP || dst.Etype == TCHAN || dst.Etype == TINTER || dst.Etype == TSLICE) 
                    return OCONVNOP;
                
            } 

            // 6. rule about untyped constants - already converted by defaultlit.

            // 7. Any typed value can be assigned to the blank identifier.
            if (dst.Etype == TBLANK)
            {
                return OCONVNOP;
            }

            return OXXX;

        }

        // Can we convert a value of type src to a value of type dst?
        // If so, return op code to use in conversion (maybe OCONVNOP).
        // If not, return OXXX.
        // srcConstant indicates whether the value of type src is a constant.
        private static Op convertop(bool srcConstant, ptr<types.Type> _addr_src, ptr<types.Type> _addr_dst, ptr<@string> _addr_why)
        {
            ref types.Type src = ref _addr_src.val;
            ref types.Type dst = ref _addr_dst.val;
            ref @string why = ref _addr_why.val;

            if (why != null)
            {
                why = "";
            }

            if (src == dst)
            {
                return OCONVNOP;
            }

            if (src == null || dst == null)
            {
                return OXXX;
            } 

            // Conversions from regular to go:notinheap are not allowed
            // (unless it's unsafe.Pointer). These are runtime-specific
            // rules.
            // (a) Disallow (*T) to (*U) where T is go:notinheap but U isn't.
            if (src.IsPtr() && dst.IsPtr() && dst.Elem().NotInHeap() && !src.Elem().NotInHeap())
            {
                if (why != null)
                {
                    why = fmt.Sprintf(":\n\t%v is go:notinheap, but %v is not", dst.Elem(), src.Elem());
                }

                return OXXX;

            } 
            // (b) Disallow string to []T where T is go:notinheap.
            if (src.IsString() && dst.IsSlice() && dst.Elem().NotInHeap() && (dst.Elem().Etype == types.Bytetype.Etype || dst.Elem().Etype == types.Runetype.Etype))
            {
                if (why != null)
                {
                    why = fmt.Sprintf(":\n\t%v is go:notinheap", dst.Elem());
                }

                return OXXX;

            } 

            // 1. src can be assigned to dst.
            var op = assignop(_addr_src, _addr_dst, _addr_why);
            if (op != OXXX)
            {
                return op;
            } 

            // The rules for interfaces are no different in conversions
            // than assignments. If interfaces are involved, stop now
            // with the good message from assignop.
            // Otherwise clear the error.
            if (src.IsInterface() || dst.IsInterface())
            {
                return OXXX;
            }

            if (why != null)
            {
                why = "";
            } 

            // 2. Ignoring struct tags, src and dst have identical underlying types.
            if (types.IdenticalIgnoreTags(src.Orig, dst.Orig))
            {
                return OCONVNOP;
            } 

            // 3. src and dst are unnamed pointer types and, ignoring struct tags,
            // their base types have identical underlying types.
            if (src.IsPtr() && dst.IsPtr() && src.Sym == null && dst.Sym == null)
            {
                if (types.IdenticalIgnoreTags(src.Elem().Orig, dst.Elem().Orig))
                {
                    return OCONVNOP;
                }

            } 

            // 4. src and dst are both integer or floating point types.
            if ((src.IsInteger() || src.IsFloat()) && (dst.IsInteger() || dst.IsFloat()))
            {
                if (simtype[src.Etype] == simtype[dst.Etype])
                {
                    return OCONVNOP;
                }

                return OCONV;

            } 

            // 5. src and dst are both complex types.
            if (src.IsComplex() && dst.IsComplex())
            {
                if (simtype[src.Etype] == simtype[dst.Etype])
                {
                    return OCONVNOP;
                }

                return OCONV;

            } 

            // Special case for constant conversions: any numeric
            // conversion is potentially okay. We'll validate further
            // within evconst. See #38117.
            if (srcConstant && (src.IsInteger() || src.IsFloat() || src.IsComplex()) && (dst.IsInteger() || dst.IsFloat() || dst.IsComplex()))
            {
                return OCONV;
            } 

            // 6. src is an integer or has type []byte or []rune
            // and dst is a string type.
            if (src.IsInteger() && dst.IsString())
            {
                return ORUNESTR;
            }

            if (src.IsSlice() && dst.IsString())
            {
                if (src.Elem().Etype == types.Bytetype.Etype)
                {
                    return OBYTES2STR;
                }

                if (src.Elem().Etype == types.Runetype.Etype)
                {
                    return ORUNES2STR;
                }

            } 

            // 7. src is a string and dst is []byte or []rune.
            // String to slice.
            if (src.IsString() && dst.IsSlice())
            {
                if (dst.Elem().Etype == types.Bytetype.Etype)
                {
                    return OSTR2BYTES;
                }

                if (dst.Elem().Etype == types.Runetype.Etype)
                {
                    return OSTR2RUNES;
                }

            } 

            // 8. src is a pointer or uintptr and dst is unsafe.Pointer.
            if ((src.IsPtr() || src.Etype == TUINTPTR) && dst.Etype == TUNSAFEPTR)
            {
                return OCONVNOP;
            } 

            // 9. src is unsafe.Pointer and dst is a pointer or uintptr.
            if (src.Etype == TUNSAFEPTR && (dst.IsPtr() || dst.Etype == TUINTPTR))
            {
                return OCONVNOP;
            } 

            // src is map and dst is a pointer to corresponding hmap.
            // This rule is needed for the implementation detail that
            // go gc maps are implemented as a pointer to a hmap struct.
            if (src.Etype == TMAP && dst.IsPtr() && src.MapType().Hmap == dst.Elem())
            {
                return OCONVNOP;
            }

            return OXXX;

        }

        private static ptr<Node> assignconv(ptr<Node> _addr_n, ptr<types.Type> _addr_t, @string context)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_assignconvfn(_addr_n, _addr_t, () => _addr_context!)!;
        }

        // Convert node n for assignment to type t.
        private static ptr<Node> assignconvfn(ptr<Node> _addr_n, ptr<types.Type> _addr_t, Func<@string> context)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            if (n == null || n.Type == null || n.Type.Broke())
            {
                return _addr_n!;
            }

            if (t.Etype == TBLANK && n.Type.Etype == TNIL)
            {
                yyerror("use of untyped nil");
            }

            n = convlit1(n, t, false, context);
            if (n.Type == null)
            {
                return _addr_n!;
            }

            if (t.Etype == TBLANK)
            {
                return _addr_n!;
            } 

            // Convert ideal bool from comparison to plain bool
            // if the next step is non-bool (like interface{}).
            if (n.Type == types.Idealbool && !t.IsBoolean())
            {
                if (n.Op == ONAME || n.Op == OLITERAL)
                {
                    var r = nod(OCONVNOP, _addr_n, _addr_null);
                    r.Type = types.Types[TBOOL];
                    r.SetTypecheck(1L);
                    r.SetImplicit(true);
                    n = r;
                }

            }

            if (types.Identical(n.Type, t))
            {
                return _addr_n!;
            }

            ref @string why = ref heap(out ptr<@string> _addr_why);
            var op = assignop(_addr_n.Type, _addr_t, _addr_why);
            if (op == OXXX)
            {
                yyerror("cannot use %L as type %v in %s%s", n, t, context(), why);
                op = OCONV;
            }

            r = nod(op, _addr_n, _addr_null);
            r.Type = t;
            r.SetTypecheck(1L);
            r.SetImplicit(true);
            r.Orig = n.Orig;
            return _addr_r!;

        }

        // IsMethod reports whether n is a method.
        // n must be a function or a method.
        private static bool IsMethod(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Type.Recv() != null;
        }

        // SliceBounds returns n's slice bounds: low, high, and max in expr[low:high:max].
        // n must be a slice expression. max is nil if n is a simple slice expression.
        private static (ptr<Node>, ptr<Node>, ptr<Node>) SliceBounds(this ptr<Node> _addr_n)
        {
            ptr<Node> low = default!;
            ptr<Node> high = default!;
            ptr<Node> max = default!;
            ref Node n = ref _addr_n.val;

            if (n.List.Len() == 0L)
            {
                return (_addr_null!, _addr_null!, _addr_null!);
            }


            if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICESTR) 
                var s = n.List.Slice();
                return (_addr_s[0L]!, _addr_s[1L]!, _addr_null!);
            else if (n.Op == OSLICE3 || n.Op == OSLICE3ARR) 
                s = n.List.Slice();
                return (_addr_s[0L]!, _addr_s[1L]!, _addr_s[2L]!);
                        Fatalf("SliceBounds op %v: %v", n.Op, n);
            return (_addr_null!, _addr_null!, _addr_null!);

        }

        // SetSliceBounds sets n's slice bounds, where n is a slice expression.
        // n must be a slice expression. If max is non-nil, n must be a full slice expression.
        private static void SetSliceBounds(this ptr<Node> _addr_n, ptr<Node> _addr_low, ptr<Node> _addr_high, ptr<Node> _addr_max)
        {
            ref Node n = ref _addr_n.val;
            ref Node low = ref _addr_low.val;
            ref Node high = ref _addr_high.val;
            ref Node max = ref _addr_max.val;


            if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICESTR) 
                if (max != null)
                {
                    Fatalf("SetSliceBounds %v given three bounds", n.Op);
                }

                var s = n.List.Slice();
                if (s == null)
                {
                    if (low == null && high == null)
                    {
                        return ;
                    }

                    n.List.Set2(low, high);
                    return ;

                }

                s[0L] = low;
                s[1L] = high;
                return ;
            else if (n.Op == OSLICE3 || n.Op == OSLICE3ARR) 
                s = n.List.Slice();
                if (s == null)
                {
                    if (low == null && high == null && max == null)
                    {
                        return ;
                    }

                    n.List.Set3(low, high, max);
                    return ;

                }

                s[0L] = low;
                s[1L] = high;
                s[2L] = max;
                return ;
                        Fatalf("SetSliceBounds op %v: %v", n.Op, n);

        }

        // IsSlice3 reports whether o is a slice3 op (OSLICE3, OSLICE3ARR).
        // o must be a slicing op.
        public static bool IsSlice3(this Op o)
        {

            if (o == OSLICE || o == OSLICEARR || o == OSLICESTR) 
                return false;
            else if (o == OSLICE3 || o == OSLICE3ARR) 
                return true;
                        Fatalf("IsSlice3 op %v", o);
            return false;

        }

        // slicePtrLen extracts the pointer and length from a slice.
        // This constructs two nodes referring to n, so n must be a cheapexpr.
        private static (ptr<Node>, ptr<Node>) slicePtrLen(this ptr<Node> _addr_n)
        {
            ptr<Node> ptr = default!;
            ptr<Node> len = default!;
            ref Node n = ref _addr_n.val;

            ref Nodes init = ref heap(out ptr<Nodes> _addr_init);
            var c = cheapexpr(_addr_n, _addr_init);
            if (c != n || init.Len() != 0L)
            {
                Fatalf("slicePtrLen not cheap: %v", n);
            }

            ptr = nod(OSPTR, _addr_n, _addr_null);
            ptr.Type = n.Type.Elem().PtrTo();
            len = nod(OLEN, _addr_n, _addr_null);
            len.Type = types.Types[TINT];
            return (_addr_ptr!, _addr_len!);

        }

        // labeledControl returns the control flow Node (for, switch, select)
        // associated with the label n, if any.
        private static ptr<Node> labeledControl(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OLABEL)
            {
                Fatalf("labeledControl %v", n.Op);
            }

            var ctl = n.Name.Defn;
            if (ctl == null)
            {
                return _addr_null!;
            }


            if (ctl.Op == OFOR || ctl.Op == OFORUNTIL || ctl.Op == OSWITCH || ctl.Op == OSELECT) 
                return _addr_ctl!;
                        return _addr_null!;

        }

        private static ptr<Node> syslook(@string name)
        {
            var s = Runtimepkg.Lookup(name);
            if (s == null || s.Def == null)
            {
                Fatalf("syslook: can't find runtime.%s", name);
            }

            return _addr_asNode(s.Def)!;

        }

        // typehash computes a hash value for type t to use in type switch statements.
        private static uint typehash(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var p = t.LongString(); 

            // Using MD5 is overkill, but reduces accidental collisions.
            var h = md5.Sum((slice<byte>)p);
            return binary.LittleEndian.Uint32(h[..4L]);

        }

        // updateHasCall checks whether expression n contains any function
        // calls and sets the n.HasCall flag if so.
        private static void updateHasCall(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return ;
            }

            n.SetHasCall(calcHasCall(_addr_n));

        }

        private static bool calcHasCall(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Ninit.Len() != 0L)
            { 
                // TODO(mdempsky): This seems overly conservative.
                return true;

            }


            if (n.Op == OLITERAL || n.Op == ONAME || n.Op == OTYPE) 
                if (n.HasCall())
                {
                    Fatalf("OLITERAL/ONAME/OTYPE should never have calls: %+v", n);
                }

                return false;
            else if (n.Op == OCALL || n.Op == OCALLFUNC || n.Op == OCALLMETH || n.Op == OCALLINTER) 
                return true;
            else if (n.Op == OANDAND || n.Op == OOROR) 
                // hard with instrumented code
                if (instrumenting)
                {
                    return true;
                }

            else if (n.Op == OINDEX || n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICE3 || n.Op == OSLICE3ARR || n.Op == OSLICESTR || n.Op == ODEREF || n.Op == ODOTPTR || n.Op == ODOTTYPE || n.Op == ODIV || n.Op == OMOD) 
                // These ops might panic, make sure they are done
                // before we start marshaling args for a call. See issue 16760.
                return true; 

                // When using soft-float, these ops might be rewritten to function calls
                // so we ensure they are evaluated first.
            else if (n.Op == OADD || n.Op == OSUB || n.Op == ONEG || n.Op == OMUL) 
                if (thearch.SoftFloat && (isFloat[n.Type.Etype] || isComplex[n.Type.Etype]))
                {
                    return true;
                }

            else if (n.Op == OLT || n.Op == OEQ || n.Op == ONE || n.Op == OLE || n.Op == OGE || n.Op == OGT) 
                if (thearch.SoftFloat && (isFloat[n.Left.Type.Etype] || isComplex[n.Left.Type.Etype]))
                {
                    return true;
                }

            else if (n.Op == OCONV) 
                if (thearch.SoftFloat && ((isFloat[n.Type.Etype] || isComplex[n.Type.Etype]) || (isFloat[n.Left.Type.Etype] || isComplex[n.Left.Type.Etype])))
                {
                    return true;
                }

                        if (n.Left != null && n.Left.HasCall())
            {
                return true;
            }

            if (n.Right != null && n.Right.HasCall())
            {
                return true;
            }

            return false;

        }

        private static void badtype(Op op, ptr<types.Type> _addr_tl, ptr<types.Type> _addr_tr)
        {
            ref types.Type tl = ref _addr_tl.val;
            ref types.Type tr = ref _addr_tr.val;

            @string fmt_ = "";
            if (tl != null)
            {
                fmt_ += fmt.Sprintf("\n\t%v", tl);
            }

            if (tr != null)
            {
                fmt_ += fmt.Sprintf("\n\t%v", tr);
            } 

            // common mistake: *struct and *interface.
            if (tl != null && tr != null && tl.IsPtr() && tr.IsPtr())
            {
                if (tl.Elem().IsStruct() && tr.Elem().IsInterface())
                {
                    fmt_ += "\n\t(*struct vs *interface)";
                }
                else if (tl.Elem().IsInterface() && tr.Elem().IsStruct())
                {
                    fmt_ += "\n\t(*interface vs *struct)";
                }

            }

            var s = fmt_;
            yyerror("illegal types for operand: %v%s", op, s);

        }

        // brcom returns !(op).
        // For example, brcom(==) is !=.
        private static Op brcom(Op op)
        {

            if (op == OEQ) 
                return ONE;
            else if (op == ONE) 
                return OEQ;
            else if (op == OLT) 
                return OGE;
            else if (op == OGT) 
                return OLE;
            else if (op == OLE) 
                return OGT;
            else if (op == OGE) 
                return OLT;
                        Fatalf("brcom: no com for %v\n", op);
            return op;

        }

        // brrev returns reverse(op).
        // For example, Brrev(<) is >.
        private static Op brrev(Op op)
        {

            if (op == OEQ) 
                return OEQ;
            else if (op == ONE) 
                return ONE;
            else if (op == OLT) 
                return OGT;
            else if (op == OGT) 
                return OLT;
            else if (op == OLE) 
                return OGE;
            else if (op == OGE) 
                return OLE;
                        Fatalf("brrev: no rev for %v\n", op);
            return op;

        }

        // return side effect-free n, appending side effects to init.
        // result is assignable if n is.
        private static ptr<Node> safeexpr(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;

            if (n == null)
            {
                return _addr_null!;
            }

            if (n.Ninit.Len() != 0L)
            {
                walkstmtlist(n.Ninit.Slice());
                init.AppendNodes(_addr_n.Ninit);
            }


            if (n.Op == ONAME || n.Op == OLITERAL) 
                return _addr_n!;
            else if (n.Op == ODOT || n.Op == OLEN || n.Op == OCAP) 
                var l = safeexpr(_addr_n.Left, _addr_init);
                if (l == n.Left)
                {
                    return _addr_n!;
                }

                var r = n.copy();
                r.Left = l;
                r = typecheck(r, ctxExpr);
                r = walkexpr(r, init);
                return _addr_r!;
            else if (n.Op == ODOTPTR || n.Op == ODEREF) 
                l = safeexpr(_addr_n.Left, _addr_init);
                if (l == n.Left)
                {
                    return _addr_n!;
                }

                var a = n.copy();
                a.Left = l;
                a = walkexpr(a, init);
                return _addr_a!;
            else if (n.Op == OINDEX || n.Op == OINDEXMAP) 
                l = safeexpr(_addr_n.Left, _addr_init);
                r = safeexpr(_addr_n.Right, _addr_init);
                if (l == n.Left && r == n.Right)
                {
                    return _addr_n!;
                }

                a = n.copy();
                a.Left = l;
                a.Right = r;
                a = walkexpr(a, init);
                return _addr_a!;
            else if (n.Op == OSTRUCTLIT || n.Op == OARRAYLIT || n.Op == OSLICELIT) 
                if (isStaticCompositeLiteral(n))
                {
                    return _addr_n!;
                }

            // make a copy; must not be used as an lvalue
            if (islvalue(n))
            {
                Fatalf("missing lvalue case in safeexpr: %v", n);
            }

            return _addr_cheapexpr(_addr_n, _addr_init)!;

        }

        private static ptr<Node> copyexpr(ptr<Node> _addr_n, ptr<types.Type> _addr_t, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;
            ref Nodes init = ref _addr_init.val;

            var l = temp(t);
            var a = nod(OAS, _addr_l, _addr_n);
            a = typecheck(a, ctxStmt);
            a = walkexpr(a, init);
            init.Append(a);
            return _addr_l!;
        }

        // return side-effect free and cheap n, appending side effects to init.
        // result may not be assignable.
        private static ptr<Node> cheapexpr(ptr<Node> _addr_n, ptr<Nodes> _addr_init)
        {
            ref Node n = ref _addr_n.val;
            ref Nodes init = ref _addr_init.val;


            if (n.Op == ONAME || n.Op == OLITERAL) 
                return _addr_n!;
                        return _addr_copyexpr(_addr_n, _addr_n.Type, _addr_init)!;

        }

        // Code to resolve elided DOTs in embedded types.

        // A Dlist stores a pointer to a TFIELD Type embedded within
        // a TSTRUCT or TINTER Type.
        public partial struct Dlist
        {
            public ptr<types.Field> field;
        }

        // dotlist is used by adddot1 to record the path of embedded fields
        // used to access a target field or method.
        // Must be non-nil so that dotpath returns a non-nil slice even if d is zero.
        private static var dotlist = make_slice<Dlist>(10L);

        // lookdot0 returns the number of fields or methods named s associated
        // with Type t. If exactly one exists, it will be returned in *save
        // (if save is not nil).
        private static long lookdot0(ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, ptr<ptr<types.Field>> _addr_save, bool ignorecase)
        {
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ptr<types.Field> save = ref _addr_save.val;

            var u = t;
            if (u.IsPtr())
            {
                u = u.Elem();
            }

            long c = 0L;
            if (u.IsStruct() || u.IsInterface())
            {
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in u.Fields().Slice())
                    {
                        f = __f;
                        if (f.Sym == s || (ignorecase && f.IsMethod() && strings.EqualFold(f.Sym.Name, s.Name)))
                        {
                            if (save != null)
                            {
                                save.val = f;
                            }

                            c++;

                        }

                    }

                    f = f__prev1;
                }
            }

            u = t;
            if (t.Sym != null && t.IsPtr() && !t.Elem().IsPtr())
            { 
                // If t is a defined pointer type, then x.m is shorthand for (*x).m.
                u = t.Elem();

            }

            u = methtype(_addr_u);
            if (u != null)
            {
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in u.Methods().Slice())
                    {
                        f = __f;
                        if (f.Embedded == 0L && (f.Sym == s || (ignorecase && strings.EqualFold(f.Sym.Name, s.Name))))
                        {
                            if (save != null)
                            {
                                save.val = f;
                            }

                            c++;

                        }

                    }

                    f = f__prev1;
                }
            }

            return c;

        }

        // adddot1 returns the number of fields or methods named s at depth d in Type t.
        // If exactly one exists, it will be returned in *save (if save is not nil),
        // and dotlist will contain the path of embedded fields traversed to find it,
        // in reverse order. If none exist, more will indicate whether t contains any
        // embedded fields at depth d, so callers can decide whether to retry at
        // a greater depth.
        private static (long, bool) adddot1(ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, long d, ptr<ptr<types.Field>> _addr_save, bool ignorecase) => func((defer, _, __) =>
        {
            long c = default;
            bool more = default;
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ptr<types.Field> save = ref _addr_save.val;

            if (t.Recur())
            {
                return ;
            }

            t.SetRecur(true);
            defer(t.SetRecur(false));

            ptr<types.Type> u;
            d--;
            if (d < 0L)
            { 
                // We've reached our target depth. If t has any fields/methods
                // named s, then we're done. Otherwise, we still need to check
                // below for embedded fields.
                c = lookdot0(_addr_s, _addr_t, _addr_save, ignorecase);
                if (c != 0L)
                {
                    return (c, false);
                }

            }

            u = t;
            if (u.IsPtr())
            {
                u = u.Elem();
            }

            if (!u.IsStruct() && !u.IsInterface())
            {
                return (c, false);
            }

            foreach (var (_, f) in u.Fields().Slice())
            {
                if (f.Embedded == 0L || f.Sym == null)
                {
                    continue;
                }

                if (d < 0L)
                { 
                    // Found an embedded field at target depth.
                    return (c, true);

                }

                var (a, more1) = adddot1(_addr_s, _addr_f.Type, d, _addr_save, ignorecase);
                if (a != 0L && c == 0L)
                {
                    dotlist[d].field = f;
                }

                c += a;
                if (more1)
                {
                    more = true;
                }

            }
            return (c, more);

        });

        // dotpath computes the unique shortest explicit selector path to fully qualify
        // a selection expression x.f, where x is of type t and f is the symbol s.
        // If no such path exists, dotpath returns nil.
        // If there are multiple shortest paths to the same depth, ambig is true.
        private static (slice<Dlist>, bool) dotpath(ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, ptr<ptr<types.Field>> _addr_save, bool ignorecase)
        {
            slice<Dlist> path = default;
            bool ambig = default;
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;
            ref ptr<types.Field> save = ref _addr_save.val;
 
            // The embedding of types within structs imposes a tree structure onto
            // types: structs parent the types they embed, and types parent their
            // fields or methods. Our goal here is to find the shortest path to
            // a field or method named s in the subtree rooted at t. To accomplish
            // that, we iteratively perform depth-first searches of increasing depth
            // until we either find the named field/method or exhaust the tree.
            for (long d = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; d++)
            {
                if (d > len(dotlist))
                {
                    dotlist = append(dotlist, new Dlist());
                }

                {
                    var (c, more) = adddot1(_addr_s, _addr_t, d, _addr_save, ignorecase);

                    if (c == 1L)
                    {
                        return (dotlist[..d], false);
                    }
                    else if (c > 1L)
                    {
                        return (null, true);
                    }
                    else if (!more)
                    {
                        return (null, false);
                    }


                }

            }


        }

        // in T.field
        // find missing fields that
        // will give shortest unique addressing.
        // modify the tree with missing type names.
        private static ptr<Node> adddot(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            n.Left = typecheck(n.Left, ctxType | ctxExpr);
            if (n.Left.Diag())
            {
                n.SetDiag(true);
            }

            var t = n.Left.Type;
            if (t == null)
            {
                return _addr_n!;
            }

            if (n.Left.Op == OTYPE)
            {
                return _addr_n!;
            }

            var s = n.Sym;
            if (s == null)
            {
                return _addr_n!;
            }

            {
                var (path, ambig) = dotpath(_addr_s, _addr_t, _addr_null, false);


                if (path != null) 
                    // rebuild elided dots
                    for (var c = len(path) - 1L; c >= 0L; c--)
                    {
                        n.Left = nodSym(ODOT, _addr_n.Left, _addr_path[c].field.Sym);
                        n.Left.SetImplicit(true);
                    }
                else if (ambig) 
                    yyerror("ambiguous selector %v", n);
                    n.Left = null;

            }

            return _addr_n!;

        }

        // Code to help generate trampoline functions for methods on embedded
        // types. These are approx the same as the corresponding adddot
        // routines except that they expect to be called with unique tasks and
        // they return the actual methods.

        public partial struct Symlink
        {
            public ptr<types.Field> field;
        }

        private static slice<Symlink> slist = default;

        private static void expand0(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var u = t;
            if (u.IsPtr())
            {
                u = u.Elem();
            }

            if (u.IsInterface())
            {
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in u.Fields().Slice())
                    {
                        f = __f;
                        if (f.Sym.Uniq())
                        {
                            continue;
                        }

                        f.Sym.SetUniq(true);
                        slist = append(slist, new Symlink(field:f));

                    }

                    f = f__prev1;
                }

                return ;

            }

            u = methtype(_addr_t);
            if (u != null)
            {
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in u.Methods().Slice())
                    {
                        f = __f;
                        if (f.Sym.Uniq())
                        {
                            continue;
                        }

                        f.Sym.SetUniq(true);
                        slist = append(slist, new Symlink(field:f));

                    }

                    f = f__prev1;
                }
            }

        }

        private static void expand1(ptr<types.Type> _addr_t, bool top)
        {
            ref types.Type t = ref _addr_t.val;

            if (t.Recur())
            {
                return ;
            }

            t.SetRecur(true);

            if (!top)
            {
                expand0(_addr_t);
            }

            var u = t;
            if (u.IsPtr())
            {
                u = u.Elem();
            }

            if (u.IsStruct() || u.IsInterface())
            {
                foreach (var (_, f) in u.Fields().Slice())
                {
                    if (f.Embedded == 0L)
                    {
                        continue;
                    }

                    if (f.Sym == null)
                    {
                        continue;
                    }

                    expand1(_addr_f.Type, false);

                }

            }

            t.SetRecur(false);

        }

        private static void expandmeth(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t == null || t.AllMethods().Len() != 0L)
            {
                return ;
            } 

            // mark top-level method symbols
            // so that expand1 doesn't consider them.
            {
                var f__prev1 = f;

                foreach (var (_, __f) in t.Methods().Slice())
                {
                    f = __f;
                    f.Sym.SetUniq(true);
                } 

                // generate all reachable methods

                f = f__prev1;
            }

            slist = slist[..0L];
            expand1(_addr_t, true); 

            // check each method to be uniquely reachable
            slice<ptr<types.Field>> ms = default;
            foreach (var (i, sl) in slist)
            {
                slist[i].field = null;
                sl.field.Sym.SetUniq(false);

                ptr<types.Field> f;
                var (path, _) = dotpath(_addr_sl.field.Sym, _addr_t, _addr_f, false);
                if (path == null)
                {
                    continue;
                } 

                // dotpath may have dug out arbitrary fields, we only want methods.
                if (!f.IsMethod())
                {
                    continue;
                } 

                // add it to the base type method list
                f = f.Copy();
                f.Embedded = 1L; // needs a trampoline
                foreach (var (_, d) in path)
                {
                    if (d.field.Type.IsPtr())
                    {
                        f.Embedded = 2L;
                        break;
                    }

                }
                ms = append(ms, f);

            }
            {
                var f__prev1 = f;

                foreach (var (_, __f) in t.Methods().Slice())
                {
                    f = __f;
                    f.Sym.SetUniq(false);
                }

                f = f__prev1;
            }

            ms = append(ms, t.Methods().Slice());
            sort.Sort(methcmp(ms));
            t.AllMethods().Set(ms);

        }

        // Given funarg struct list, return list of ODCLFIELD Node fn args.
        private static slice<ptr<Node>> structargs(ptr<types.Type> _addr_tl, bool mustname)
        {
            ref types.Type tl = ref _addr_tl.val;

            slice<ptr<Node>> args = default;
            long gen = 0L;
            foreach (var (_, t) in tl.Fields().Slice())
            {
                var s = t.Sym;
                if (mustname && (s == null || s.Name == "_"))
                { 
                    // invent a name so that we can refer to it in the trampoline
                    s = lookupN(".anon", gen);
                    gen++;

                }

                var a = symfield(s, t.Type);
                a.Pos = t.Pos;
                a.SetIsDDD(t.IsDDD());
                args = append(args, a);

            }
            return args;

        }

        // Generate a wrapper function to convert from
        // a receiver of type T to a receiver of type U.
        // That is,
        //
        //    func (t T) M() {
        //        ...
        //    }
        //
        // already exists; this function generates
        //
        //    func (u U) M() {
        //        u.M()
        //    }
        //
        // where the types T and U are such that u.M() is valid
        // and calls the T.M method.
        // The resulting function is for use in method tables.
        //
        //    rcvr - U
        //    method - M func (t T)(), a TFIELD type struct
        //    newnam - the eventual mangled name of this function
        private static void genwrapper(ptr<types.Type> _addr_rcvr, ptr<types.Field> _addr_method, ptr<types.Sym> _addr_newnam)
        {
            ref types.Type rcvr = ref _addr_rcvr.val;
            ref types.Field method = ref _addr_method.val;
            ref types.Sym newnam = ref _addr_newnam.val;

            if (false && Debug['r'] != 0L)
            {
                fmt.Printf("genwrapper rcvrtype=%v method=%v newnam=%v\n", rcvr, method, newnam);
            } 

            // Only generate (*T).M wrappers for T.M in T's own package.
            if (rcvr.IsPtr() && rcvr.Elem() == method.Type.Recv().Type && rcvr.Elem().Sym != null && rcvr.Elem().Sym.Pkg != localpkg)
            {
                return ;
            } 

            // Only generate I.M wrappers for I in I's own package
            // but keep doing it for error.Error (was issue #29304).
            if (rcvr.IsInterface() && rcvr.Sym != null && rcvr.Sym.Pkg != localpkg && rcvr != types.Errortype)
            {
                return ;
            }

            lineno = autogeneratedPos;
            dclcontext = PEXTERN;

            var tfn = nod(OTFUNC, _addr_null, _addr_null);
            tfn.Left = namedfield(".this", rcvr);
            tfn.List.Set(structargs(_addr_method.Type.Params(), true));
            tfn.Rlist.Set(structargs(_addr_method.Type.Results(), false));

            disableExport(newnam);
            var fn = dclfunc(newnam, tfn);
            fn.Func.SetDupok(true);

            var nthis = asNode(tfn.Type.Recv().Nname);

            var methodrcvr = method.Type.Recv().Type; 

            // generate nil pointer check for better error
            if (rcvr.IsPtr() && rcvr.Elem() == methodrcvr)
            { 
                // generating wrapper from *T to T.
                var n = nod(OIF, _addr_null, _addr_null);
                n.Left = nod(OEQ, _addr_nthis, _addr_nodnil());
                var call = nod(OCALL, _addr_syslook("panicwrap"), _addr_null);
                n.Nbody.Set1(call);
                fn.Nbody.Append(n);

            }

            var dot = adddot(_addr_nodSym(OXDOT, _addr_nthis, _addr_method.Sym)); 

            // generate call
            // It's not possible to use a tail call when dynamic linking on ppc64le. The
            // bad scenario is when a local call is made to the wrapper: the wrapper will
            // call the implementation, which might be in a different module and so set
            // the TOC to the appropriate value for that module. But if it returns
            // directly to the wrapper's caller, nothing will reset it to the correct
            // value for that function.
            if (!instrumenting && rcvr.IsPtr() && methodrcvr.IsPtr() && method.Embedded != 0L && !isifacemethod(method.Type) && !(thearch.LinkArch.Name == "ppc64le" && Ctxt.Flag_dynlink))
            { 
                // generate tail call: adjust pointer receiver and jump to embedded method.
                dot = dot.Left; // skip final .M
                // TODO(mdempsky): Remove dependency on dotlist.
                if (!dotlist[0L].field.Type.IsPtr())
                {
                    dot = nod(OADDR, _addr_dot, _addr_null);
                }

                var @as = nod(OAS, _addr_nthis, _addr_convnop(dot, rcvr));
                fn.Nbody.Append(as);
                fn.Nbody.Append(nodSym(ORETJMP, _addr_null, _addr_methodSym(methodrcvr, method.Sym)));

            }
            else
            {
                fn.Func.SetWrapper(true); // ignore frame for panic+recover matching
                call = nod(OCALL, _addr_dot, _addr_null);
                call.List.Set(paramNnames(_addr_tfn.Type));
                call.SetIsDDD(tfn.Type.IsVariadic());
                if (method.Type.NumResults() > 0L)
                {
                    n = nod(ORETURN, _addr_null, _addr_null);
                    n.List.Set1(call);
                    call = n;
                }

                fn.Nbody.Append(call);

            }

            if (false && Debug['r'] != 0L)
            {
                dumplist("genwrapper body", fn.Nbody);
            }

            funcbody();
            if (debug_dclstack != 0L)
            {
                testdclstack();
            }

            fn = typecheck(fn, ctxStmt);

            Curfn = fn;
            typecheckslice(fn.Nbody.Slice(), ctxStmt); 

            // Inline calls within (*T).M wrappers. This is safe because we only
            // generate those wrappers within the same compilation unit as (T).M.
            // TODO(mdempsky): Investigate why we can't enable this more generally.
            if (rcvr.IsPtr() && rcvr.Elem() == method.Type.Recv().Type && rcvr.Elem().Sym != null)
            {
                inlcalls(fn);
            }

            escapeFuncs(new slice<ptr<Node>>(new ptr<Node>[] { fn }), false);

            Curfn = null;
            funccompile(fn);

        }

        private static slice<ptr<Node>> paramNnames(ptr<types.Type> _addr_ft)
        {
            ref types.Type ft = ref _addr_ft.val;

            var args = make_slice<ptr<Node>>(ft.NumParams());
            foreach (var (i, f) in ft.Params().FieldSlice())
            {
                args[i] = asNode(f.Nname);
            }
            return args;

        }

        private static ptr<Node> hashmem(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            var sym = Runtimepkg.Lookup("memhash");

            var n = newname(_addr_sym);
            n.SetClass(PFUNC);
            n.Sym.SetFunc(true);
            n.Type = functype(null, new slice<ptr<Node>>(new ptr<Node>[] { anonfield(types.NewPtr(t)), anonfield(types.Types[TUINTPTR]), anonfield(types.Types[TUINTPTR]) }), new slice<ptr<Node>>(new ptr<Node>[] { anonfield(types.Types[TUINTPTR]) }));
            return _addr_n!;
        }

        private static (ptr<types.Field>, bool) ifacelookdot(ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, bool ignorecase)
        {
            ptr<types.Field> m = default!;
            bool followptr = default;
            ref types.Sym s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            if (t == null)
            {
                return (_addr_null!, false);
            }

            var (path, ambig) = dotpath(_addr_s, _addr_t, _addr_m, ignorecase);
            if (path == null)
            {
                if (ambig)
                {
                    yyerror("%v.%v is ambiguous", t, s);
                }

                return (_addr_null!, false);

            }

            foreach (var (_, d) in path)
            {
                if (d.field.Type.IsPtr())
                {
                    followptr = true;
                    break;
                }

            }
            if (!m.IsMethod())
            {
                yyerror("%v.%v is a field, not a method", t, s);
                return (_addr_null!, followptr);
            }

            return (_addr_m!, followptr);

        }

        private static bool implements(ptr<types.Type> _addr_t, ptr<types.Type> _addr_iface, ptr<ptr<types.Field>> _addr_m, ptr<ptr<types.Field>> _addr_samename, ptr<long> _addr_ptr)
        {
            ref types.Type t = ref _addr_t.val;
            ref types.Type iface = ref _addr_iface.val;
            ref ptr<types.Field> m = ref _addr_m.val;
            ref ptr<types.Field> samename = ref _addr_samename.val;
            ref long ptr = ref _addr_ptr.val;

            var t0 = t;
            if (t == null)
            {
                return false;
            }

            if (t.IsInterface())
            {
                long i = 0L;
                var tms = t.Fields().Slice();
                {
                    var im__prev1 = im;

                    foreach (var (_, __im) in iface.Fields().Slice())
                    {
                        im = __im;
                        while (i < len(tms) && tms[i].Sym != im.Sym)
                        {
                            i++;
                        }

                        if (i == len(tms))
                        {
                            m.val = im;
                            samename.val = null;
                            ptr = 0L;
                            return false;
                        }

                        var tm = tms[i];
                        if (!types.Identical(tm.Type, im.Type))
                        {
                            m.val = im;
                            samename.val = tm;
                            ptr = 0L;
                            return false;
                        }

                    }

                    im = im__prev1;
                }

                return true;

            }

            t = methtype(_addr_t);
            tms = default;
            if (t != null)
            {
                expandmeth(_addr_t);
                tms = t.AllMethods().Slice();
            }

            i = 0L;
            {
                var im__prev1 = im;

                foreach (var (_, __im) in iface.Fields().Slice())
                {
                    im = __im;
                    if (im.Broke())
                    {
                        continue;
                    }

                    while (i < len(tms) && tms[i].Sym != im.Sym)
                    {
                        i++;
                    }

                    if (i == len(tms))
                    {
                        m.val = im;
                        samename.val, _ = ifacelookdot(_addr_im.Sym, _addr_t, true);
                        ptr = 0L;
                        return false;
                    }

                    tm = tms[i];
                    if (tm.Nointerface() || !types.Identical(tm.Type, im.Type))
                    {
                        m.val = im;
                        samename.val = tm;
                        ptr = 0L;
                        return false;
                    }

                    var followptr = tm.Embedded == 2L; 

                    // if pointer receiver in method,
                    // the method does not exist for value types.
                    var rcvr = tm.Type.Recv().Type;
                    if (rcvr.IsPtr() && !t0.IsPtr() && !followptr && !isifacemethod(tm.Type))
                    {
                        if (false && Debug['r'] != 0L)
                        {
                            yyerror("interface pointer mismatch");
                        }

                        m.val = im;
                        samename.val = null;
                        ptr = 1L;
                        return false;

                    }

                } 

                // We're going to emit an OCONVIFACE.
                // Call itabname so that (t, iface)
                // gets added to itabs early, which allows
                // us to de-virtualize calls through this
                // type/interface pair later. See peekitabs in reflect.go

                im = im__prev1;
            }

            if (isdirectiface(_addr_t0) && !iface.IsEmptyInterface())
            {
                itabname(t0, iface);
            }

            return true;

        }

        private static slice<ptr<Node>> listtreecopy(slice<ptr<Node>> l, src.XPos pos)
        {
            slice<ptr<Node>> @out = default;
            foreach (var (_, n) in l)
            {
                out = append(out, treecopy(_addr_n, pos));
            }
            return out;

        }

        private static ptr<Node> liststmt(slice<ptr<Node>> l)
        {
            var n = nod(OBLOCK, _addr_null, _addr_null);
            n.List.Set(l);
            if (len(l) != 0L)
            {
                n.Pos = l[0L].Pos;
            }

            return _addr_n!;

        }

        public static ptr<Node> asblock(this Nodes l)
        {
            var n = nod(OBLOCK, _addr_null, _addr_null);
            n.List = l;
            if (l.Len() != 0L)
            {
                n.Pos = l.First().Pos;
            }

            return _addr_n!;

        }

        private static ptr<types.Sym> ngotype(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Type != null)
            {
                return _addr_typenamesym(n.Type)!;
            }

            return _addr_null!;

        }

        // The result of addinit MUST be assigned back to n, e.g.
        //     n.Left = addinit(n.Left, init)
        private static ptr<Node> addinit(ptr<Node> _addr_n, slice<ptr<Node>> init)
        {
            ref Node n = ref _addr_n.val;

            if (len(init) == 0L)
            {
                return _addr_n!;
            }

            if (n.mayBeShared())
            { 
                // Introduce OCONVNOP to hold init list.
                n = nod(OCONVNOP, _addr_n, _addr_null);
                n.Type = n.Left.Type;
                n.SetTypecheck(1L);

            }

            n.Ninit.Prepend(init);
            n.SetHasCall(true);
            return _addr_n!;

        }

        // The linker uses the magic symbol prefixes "go." and "type."
        // Avoid potential confusion between import paths and symbols
        // by rejecting these reserved imports for now. Also, people
        // "can do weird things in GOPATH and we'd prefer they didn't
        // do _that_ weird thing" (per rsc). See also #4257.
        private static @string reservedimports = new slice<@string>(new @string[] { "go", "type" });

        private static bool isbadimport(@string path, bool allowSpace)
        {
            if (strings.Contains(path, "\x00"))
            {
                yyerror("import path contains NUL");
                return true;
            }

            foreach (var (_, ri) in reservedimports)
            {
                if (path == ri)
                {
                    yyerror("import path %q is reserved and cannot be used", path);
                    return true;
                }

            }
            foreach (var (_, r) in path)
            {
                if (r == utf8.RuneError)
                {
                    yyerror("import path contains invalid UTF-8 sequence: %q", path);
                    return true;
                }

                if (r < 0x20UL || r == 0x7fUL)
                {
                    yyerror("import path contains control character: %q", path);
                    return true;
                }

                if (r == '\\')
                {
                    yyerror("import path contains backslash; use slash: %q", path);
                    return true;
                }

                if (!allowSpace && unicode.IsSpace(r))
                {
                    yyerror("import path contains space character: %q", path);
                    return true;
                }

                if (strings.ContainsRune("!\"#$%&'()*,:;<=>?[]^`{|}", r))
                {
                    yyerror("import path contains invalid character '%c': %q", r, path);
                    return true;
                }

            }
            return false;

        }

        // Can this type be stored directly in an interface word?
        // Yes, if the representation is a single pointer.
        private static bool isdirectiface(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t.Broke())
            {
                return false;
            }


            if (t.Etype == TPTR || t.Etype == TCHAN || t.Etype == TMAP || t.Etype == TFUNC || t.Etype == TUNSAFEPTR) 
                return true;
            else if (t.Etype == TARRAY) 
                // Array of 1 direct iface type can be direct.
                return t.NumElem() == 1L && isdirectiface(_addr_t.Elem());
            else if (t.Etype == TSTRUCT) 
                // Struct with 1 field of direct iface type can be direct.
                return t.NumFields() == 1L && isdirectiface(_addr_t.Field(0L).Type);
                        return false;

        }

        // itabType loads the _type field from a runtime.itab struct.
        private static ptr<Node> itabType(ptr<Node> _addr_itab)
        {
            ref Node itab = ref _addr_itab.val;

            var typ = nodSym(ODOTPTR, _addr_itab, _addr_null);
            typ.Type = types.NewPtr(types.Types[TUINT8]);
            typ.SetTypecheck(1L);
            typ.Xoffset = int64(Widthptr); // offset of _type in runtime.itab
            typ.SetBounded(true); // guaranteed not to fault
            return _addr_typ!;

        }

        // ifaceData loads the data field from an interface.
        // The concrete type must be known to have type t.
        // It follows the pointer if !isdirectiface(t).
        private static ptr<Node> ifaceData(src.XPos pos, ptr<Node> _addr_n, ptr<types.Type> _addr_t)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            if (t.IsInterface())
            {
                Fatalf("ifaceData interface: %v", t);
            }

            var ptr = nodlSym(pos, OIDATA, _addr_n, _addr_null);
            if (isdirectiface(_addr_t))
            {
                ptr.Type = t;
                ptr.SetTypecheck(1L);
                return _addr_ptr!;
            }

            ptr.Type = types.NewPtr(t);
            ptr.SetTypecheck(1L);
            var ind = nodl(pos, ODEREF, _addr_ptr, _addr_null);
            ind.Type = t;
            ind.SetTypecheck(1L);
            ind.SetBounded(true);
            return _addr_ind!;

        }
    }
}}}}
