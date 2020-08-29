// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:29:19 UTC
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

        private static sync.Mutex largeStackFramesMu = default;        private static slice<src.XPos> largeStackFrames = default;

        private static void errorexit()
        {
            flusherrors();
            if (outfile != "")
            {
                os.Remove(outfile);
            }
            os.Exit(2L);
        }

        private static void adderrorname(ref Node n)
        {
            if (n.Op != ODOT)
            {
                return;
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

            errors = append(errors, new Error(pos:pos,msg:fmt.Sprintf("%v: %s\n",linestr(pos),fmt.Sprintf(format,args...)),));
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
                return;
            }
            sort.Stable(byPos(errors));
            for (long i = 0L; i < len(errors); i++)
            {
                if (i == 0L || errors[i].msg != errors[i - 1L].msg)
                {
                    fmt.Printf("%s", errors[i].msg);
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
                ref long x = default;
                x.Value = 0L;
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
                    return;
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
                    return;
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

        private static void yyerror(@string format, params object[] args)
        {
            args = args.Clone();

            yyerrorl(lineno, format, args);
        }

        public static void Warn(@string fmt_, params object[] args)
        {
            args = args.Clone();

            adderr(lineno, fmt_, args);

            hcrash();
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

        private static src.XPos setlineno(ref Node n)
        {
            var lno = lineno;
            if (n != null)
            {

                if (n.Op == ONAME || n.Op == OPACK)
                {
                    break;
                    goto __switch_break0;
                }
                if (n.Op == OLITERAL || n.Op == OTYPE)
                {
                    if (n.Sym != null)
                    {
                        break;
                    }
                }
                // default: 
                    lineno = n.Pos;
                    if (!lineno.IsKnown())
                    {
                        if (Debug['K'] != 0L)
                        {
                            Warn("setlineno: unknown position (line 0)");
                        }
                        lineno = lno;
                    }

                __switch_break0:;
            }
            return lno;
        }

        private static ref types.Sym lookup(@string name)
        {
            return localpkg.Lookup(name);
        }

        // lookupN looks up the symbol starting with prefix and ending with
        // the decimal n. If prefix is too long, lookupN panics.
        private static ref types.Sym lookupN(@string prefix, long n)
        {
            array<byte> buf = new array<byte>(20L); // plenty long enough for all current users
            copy(buf[..], prefix);
            var b = strconv.AppendInt(buf[..len(prefix)], int64(n), 10L);
            return localpkg.LookupBytes(b);
        }

        // autolabel generates a new Name node for use with
        // an automatically generated label.
        // prefix is a short mnemonic (e.g. ".s" for switch)
        // to help with debugging.
        // It should begin with "." to avoid conflicts with
        // user labels.
        private static ref Node autolabel(@string prefix)
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
            return newname(lookupN(prefix, int(n)));
        }

        private static ref types.Sym restrictlookup(@string name, ref types.Pkg pkg)
        {
            if (!exportname(name) && pkg != localpkg)
            {
                yyerror("cannot refer to unexported name %s.%s", pkg.Name, name);
            }
            return pkg.Lookup(name);
        }

        // find all the exported symbols in package opkg
        // and make them available in the current package
        private static void importdot(ref types.Pkg opkg, ref Node pack)
        {
            long n = 0L;
            foreach (var (_, s) in opkg.Syms)
            {
                if (s.Def == null)
                {
                    continue;
                }
                if (!exportname(s.Name) || strings.ContainsRune(s.Name, 0xb7UL))
                { // 0xb7 = center dot
                    continue;
                }
                var s1 = lookup(s.Name);
                if (s1.Def != null)
                {
                    var pkgerror = fmt.Sprintf("during import %q", opkg.Path);
                    redeclare(s1, pkgerror);
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

        private static ref Node nod(Op op, ref Node nleft, ref Node nright)
        {
            return nodl(lineno, op, nleft, nright);
        }

        private static ref Node nodl(src.XPos pos, Op op, ref Node nleft, ref Node nright)
        {
            ref Node n = default;

            if (op == OCLOSURE || op == ODCLFUNC) 
                var x = default;
                n = ref x.Node;
                n.Func = ref x.Func;
            else if (op == ONAME) 
                Fatalf("use newname instead");
            else if (op == OLABEL || op == OPACK) 
                x = default;
                n = ref x.Node;
                n.Name = ref x.Name;
            else 
                n = @new<Node>();
                        n.Op = op;
            n.Left = nleft;
            n.Right = nright;
            n.Pos = pos;
            n.Xoffset = BADWIDTH;
            n.Orig = n;
            return n;
        }

        // newname returns a new ONAME Node associated with symbol s.
        private static ref Node newname(ref types.Sym s)
        {
            var n = newnamel(lineno, s);
            n.Name.Curfn = Curfn;
            return n;
        }

        // newname returns a new ONAME Node associated with symbol s at position pos.
        // The caller is responsible for setting n.Name.Curfn.
        private static ref Node newnamel(src.XPos pos, ref types.Sym s)
        {
            if (s == null)
            {
                Fatalf("newnamel nil");
            }
            var x = default;
            var n = ref x.Node;
            n.Name = ref x.Name;
            n.Name.Param = ref x.Param;

            n.Op = ONAME;
            n.Pos = pos;
            n.Orig = n;

            n.Sym = s;
            n.SetAddable(true);
            return n;
        }

        // nodSym makes a Node with Op op and with the Left field set to left
        // and the Sym field set to sym. This is for ODOT and friends.
        private static ref Node nodSym(Op op, ref Node left, ref types.Sym sym)
        {
            var n = nod(op, left, null);
            n.Sym = sym;
            return n;
        }

        private static void saveorignode(ref Node n)
        {
            if (n.Orig != null)
            {
                return;
            }
            var norig = nod(n.Op, null, null);
            norig.Value = n.Value;
            n.Orig = norig;
        }

        // methcmp sorts by symbol, then by package path for unexported symbols.
        private partial struct methcmp // : slice<ref types.Field>
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
            var a = x[i];
            var b = x[j];
            if (a.Sym == null && b.Sym == null)
            {
                return false;
            }
            if (a.Sym == null)
            {
                return true;
            }
            if (b.Sym == null)
            {
                return false;
            }
            if (a.Sym.Name != b.Sym.Name)
            {
                return a.Sym.Name < b.Sym.Name;
            }
            if (!exportname(a.Sym.Name))
            {
                if (a.Sym.Pkg.Path != b.Sym.Pkg.Path)
                {
                    return a.Sym.Pkg.Path < b.Sym.Pkg.Path;
                }
            }
            return false;
        }

        private static ref Node nodintconst(long v)
        {
            var c = nod(OLITERAL, null, null);
            c.SetAddable(true);
            c.SetVal(new Val(new(Mpint)));
            c.Val().U._<ref Mpint>().SetInt64(v);
            c.Type = types.Types[TIDEAL];
            return c;
        }

        private static ref Node nodfltconst(ref Mpflt v)
        {
            var c = nod(OLITERAL, null, null);
            c.SetAddable(true);
            c.SetVal(new Val(newMpflt()));
            c.Val().U._<ref Mpflt>().Set(v);
            c.Type = types.Types[TIDEAL];
            return c;
        }

        private static void nodconst(ref Node n, ref types.Type t, long v)
        {
            n.Value = new Node();
            n.Op = OLITERAL;
            n.SetAddable(true);
            n.SetVal(new Val(new(Mpint)));
            n.Val().U._<ref Mpint>().SetInt64(v);
            n.Type = t;

            if (t.IsFloat())
            {
                Fatalf("nodconst: bad type %v", t);
            }
        }

        private static ref Node nodnil()
        {
            var c = nodintconst(0L);
            c.SetVal(new Val(new(NilVal)));
            c.Type = types.Types[TNIL];
            return c;
        }

        private static ref Node nodbool(bool b)
        {
            var c = nodintconst(0L);
            c.SetVal(new Val(b));
            c.Type = types.Idealbool;
            return c;
        }

        private static ref Node nodstr(@string s)
        {
            return nodlit(new Val(s));
        }

        // treecopy recursively copies n, with the exception of
        // ONAME, OLITERAL, OTYPE, and non-iota ONONAME leaves.
        // Copies of iota ONONAME nodes are assigned the current
        // value of iota_. If pos.IsKnown(), it sets the source
        // position of newly allocated nodes to pos.
        private static ref Node treecopy(ref Node n, src.XPos pos)
        {
            if (n == null)
            {
                return null;
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
                return n;
                goto __switch_break1;
            }
            // default: 
                var m = n.Value;
                m.Orig = ref m;
                m.Left = treecopy(n.Left, pos);
                m.Right = treecopy(n.Right, pos);
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
                return ref m;

            __switch_break1:;
        }

        // isnil reports whether n represents the universal untyped zero value "nil".
        private static bool isnil(ref Node n)
        { 
            // Check n.Orig because constant propagation may produce typed nil constants,
            // which don't exist in the Go spec.
            return Isconst(n.Orig, CTNIL);
        }

        private static bool isptrto(ref types.Type t, types.EType et)
        {
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

        private static bool isblank(ref Node n)
        {
            if (n == null)
            {
                return false;
            }
            return n.Sym.IsBlank();
        }

        // methtype returns the underlying type, if any,
        // that owns methods with receiver parameter t.
        // The result is either a named type or an anonymous struct.
        private static ref types.Type methtype(ref types.Type t)
        {
            if (t == null)
            {
                return null;
            } 

            // Strip away pointer if it's there.
            if (t.IsPtr())
            {
                if (t.Sym != null)
                {
                    return null;
                }
                t = t.Elem();
                if (t == null)
                {
                    return null;
                }
            } 

            // Must be a named type or anonymous struct.
            if (t.Sym == null && !t.IsStruct())
            {
                return null;
            } 

            // Check types.
            if (issimple[t.Etype])
            {
                return t;
            }

            if (t.Etype == TARRAY || t.Etype == TCHAN || t.Etype == TFUNC || t.Etype == TMAP || t.Etype == TSLICE || t.Etype == TSTRING || t.Etype == TSTRUCT) 
                return t;
                        return null;
        }

        // eqtype reports whether t1 and t2 are identical, following the spec rules.
        //
        // Any cyclic type must go through a named type, and if one is
        // named, it is only identical to the other if they are the same
        // pointer (t1 == t2), so there's no chance of chasing cycles
        // ad infinitum, so no need for a depth counter.
        private static bool eqtype(ref types.Type t1, ref types.Type t2)
        {
            return eqtype1(t1, t2, true, null);
        }

        // eqtypeIgnoreTags is like eqtype but it ignores struct tags for struct identity.
        private static bool eqtypeIgnoreTags(ref types.Type t1, ref types.Type t2)
        {
            return eqtype1(t1, t2, false, null);
        }

        private partial struct typePair
        {
            public ptr<types.Type> t1;
            public ptr<types.Type> t2;
        }

        private static bool eqtype1(ref types.Type t1, ref types.Type t2, bool cmpTags, object assumedEqual)
        {
            if (t1 == t2)
            {
                return true;
            }
            if (t1 == null || t2 == null || t1.Etype != t2.Etype || t1.Broke() || t2.Broke())
            {
                return false;
            }
            if (t1.Sym != null || t2.Sym != null)
            { 
                // Special case: we keep byte/uint8 and rune/int32
                // separate for error messages. Treat them as equal.

                if (t1.Etype == TUINT8) 
                    return (t1 == types.Types[TUINT8] || t1 == types.Bytetype) && (t2 == types.Types[TUINT8] || t2 == types.Bytetype);
                else if (t1.Etype == TINT32) 
                    return (t1 == types.Types[TINT32] || t1 == types.Runetype) && (t2 == types.Types[TINT32] || t2 == types.Runetype);
                else 
                    return false;
                            }
            if (assumedEqual == null)
            {
                assumedEqual = make();
            }            {
                var (_, ok) = assumedEqual[new typePair(t1,t2)];


                else if (ok)
                {
                    return true;
                }

            }
            assumedEqual[new typePair(t1,t2)] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};


            if (t1.Etype == TINTER) 
                if (t1.NumFields() != t2.NumFields())
                {
                    return false;
                }
                {
                    var i__prev1 = i;
                    var f1__prev1 = f1;

                    foreach (var (__i, __f1) in t1.FieldSlice())
                    {
                        i = __i;
                        f1 = __f1;
                        var f2 = t2.Field(i);
                        if (f1.Sym != f2.Sym || !eqtype1(f1.Type, f2.Type, cmpTags, assumedEqual))
                        {
                            return false;
                        }
                    }

                    i = i__prev1;
                    f1 = f1__prev1;
                }

                return true;
            else if (t1.Etype == TSTRUCT) 
                if (t1.NumFields() != t2.NumFields())
                {
                    return false;
                }
                {
                    var i__prev1 = i;
                    var f1__prev1 = f1;

                    foreach (var (__i, __f1) in t1.FieldSlice())
                    {
                        i = __i;
                        f1 = __f1;
                        f2 = t2.Field(i);
                        if (f1.Sym != f2.Sym || f1.Embedded != f2.Embedded || !eqtype1(f1.Type, f2.Type, cmpTags, assumedEqual))
                        {
                            return false;
                        }
                        if (cmpTags && f1.Note != f2.Note)
                        {
                            return false;
                        }
                    }

                    i = i__prev1;
                    f1 = f1__prev1;
                }

                return true;
            else if (t1.Etype == TFUNC) 
                // Check parameters and result parameters for type equality.
                // We intentionally ignore receiver parameters for type
                // equality, because they're never relevant.
                foreach (var (_, f) in types.ParamsResults)
                { 
                    // Loop over fields in structs, ignoring argument names.
                    var fs1 = f(t1).FieldSlice();
                    var fs2 = f(t2).FieldSlice();
                    if (len(fs1) != len(fs2))
                    {
                        return false;
                    }
                    {
                        var i__prev2 = i;
                        var f1__prev2 = f1;

                        foreach (var (__i, __f1) in fs1)
                        {
                            i = __i;
                            f1 = __f1;
                            f2 = fs2[i];
                            if (f1.Isddd() != f2.Isddd() || !eqtype1(f1.Type, f2.Type, cmpTags, assumedEqual))
                            {
                                return false;
                            }
                        }

                        i = i__prev2;
                        f1 = f1__prev2;
                    }

                }
                return true;
            else if (t1.Etype == TARRAY) 
                if (t1.NumElem() != t2.NumElem())
                {
                    return false;
                }
            else if (t1.Etype == TCHAN) 
                if (t1.ChanDir() != t2.ChanDir())
                {
                    return false;
                }
            else if (t1.Etype == TMAP) 
                if (!eqtype1(t1.Key(), t2.Key(), cmpTags, assumedEqual))
                {
                    return false;
                }
                return eqtype1(t1.Val(), t2.Val(), cmpTags, assumedEqual);
                        return eqtype1(t1.Elem(), t2.Elem(), cmpTags, assumedEqual);
        }

        // Are t1 and t2 equal struct types when field names are ignored?
        // For deciding whether the result struct from g can be copied
        // directly when compiling f(g()).
        private static bool eqtypenoname(ref types.Type t1, ref types.Type t2)
        {
            if (t1 == null || t2 == null || !t1.IsStruct() || !t2.IsStruct())
            {
                return false;
            }
            if (t1.NumFields() != t2.NumFields())
            {
                return false;
            }
            foreach (var (i, f1) in t1.FieldSlice())
            {
                var f2 = t2.Field(i);
                if (!eqtype(f1.Type, f2.Type))
                {
                    return false;
                }
            }
            return true;
        }

        // Is type src assignment compatible to type dst?
        // If so, return op code to use in conversion.
        // If not, return 0.
        private static Op assignop(ref types.Type src, ref types.Type dst, ref @string why)
        {
            if (why != null)
            {
                why.Value = "";
            } 

            // TODO(rsc,lvd): This behaves poorly in the presence of inlining.
            // https://golang.org/issue/2795
            if (safemode && !inimport && src != null && src.Etype == TUNSAFEPTR)
            {
                yyerror("cannot use unsafe.Pointer");
                errorexit();
            }
            if (src == dst)
            {
                return OCONVNOP;
            }
            if (src == null || dst == null || src.Etype == TFORW || dst.Etype == TFORW || src.Orig == null || dst.Orig == null)
            {
                return 0L;
            } 

            // 1. src type is identical to dst.
            if (eqtype(src, dst))
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
            if (eqtype(src.Orig, dst.Orig))
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
                ref types.Field missing = default;                ref types.Field have = default;

                long ptr = default;
                if (implements(src, dst, ref missing, ref have, ref ptr))
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
                    if (isptrto(src, TINTER))
                    {
                        why.Value = fmt.Sprintf(":\n\t%v is pointer to interface, not interface", src);
                    }
                    else if (have != null && have.Sym == missing.Sym && have.Nointerface())
                    {
                        why.Value = fmt.Sprintf(":\n\t%v does not implement %v (%v method is marked 'nointerface')", src, dst, missing.Sym);
                    }
                    else if (have != null && have.Sym == missing.Sym)
                    {
                        why.Value = fmt.Sprintf(":\n\t%v does not implement %v (wrong type for %v method)\n" + "\t\thave %v%0S\n\t\twant %v%0S", src, dst, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
                    }
                    else if (ptr != 0L)
                    {
                        why.Value = fmt.Sprintf(":\n\t%v does not implement %v (%v method has pointer receiver)", src, dst, missing.Sym);
                    }
                    else if (have != null)
                    {
                        why.Value = fmt.Sprintf(":\n\t%v does not implement %v (missing %v method)\n" + "\t\thave %v%0S\n\t\twant %v%0S", src, dst, missing.Sym, have.Sym, have.Type, missing.Sym, missing.Type);
                    }
                    else
                    {
                        why.Value = fmt.Sprintf(":\n\t%v does not implement %v (missing %v method)", src, dst, missing.Sym);
                    }
                }
                return 0L;
            }
            if (isptrto(dst, TINTER))
            {
                if (why != null)
                {
                    why.Value = fmt.Sprintf(":\n\t%v is pointer to interface, not interface", dst);
                }
                return 0L;
            }
            if (src.IsInterface() && dst.Etype != TBLANK)
            {
                missing = default;                have = default;

                ptr = default;
                if (why != null && implements(dst, src, ref missing, ref have, ref ptr))
                {
                    why.Value = ": need type assertion";
                }
                return 0L;
            } 

            // 4. src is a bidirectional channel value, dst is a channel type,
            // src and dst have identical element types, and
            // either src or dst is not a named type.
            if (src.IsChan() && src.ChanDir() == types.Cboth && dst.IsChan())
            {
                if (eqtype(src.Elem(), dst.Elem()) && (src.Sym == null || dst.Sym == null))
                {
                    return OCONVNOP;
                }
            } 

            // 5. src is the predeclared identifier nil and dst is a nillable type.
            if (src.Etype == TNIL)
            {

                if (dst.Etype == TPTR32 || dst.Etype == TPTR64 || dst.Etype == TFUNC || dst.Etype == TMAP || dst.Etype == TCHAN || dst.Etype == TINTER || dst.Etype == TSLICE) 
                    return OCONVNOP;
                            } 

            // 6. rule about untyped constants - already converted by defaultlit.

            // 7. Any typed value can be assigned to the blank identifier.
            if (dst.Etype == TBLANK)
            {
                return OCONVNOP;
            }
            return 0L;
        }

        // Can we convert a value of type src to a value of type dst?
        // If so, return op code to use in conversion (maybe OCONVNOP).
        // If not, return 0.
        private static Op convertop(ref types.Type src, ref types.Type dst, ref @string why)
        {
            if (why != null)
            {
                why.Value = "";
            }
            if (src == dst)
            {
                return OCONVNOP;
            }
            if (src == null || dst == null)
            {
                return 0L;
            } 

            // Conversions from regular to go:notinheap are not allowed
            // (unless it's unsafe.Pointer). This is a runtime-specific
            // rule.
            if (src.IsPtr() && dst.IsPtr() && dst.Elem().NotInHeap() && !src.Elem().NotInHeap())
            {
                if (why != null)
                {
                    why.Value = fmt.Sprintf(":\n\t%v is go:notinheap, but %v is not", dst.Elem(), src.Elem());
                }
                return 0L;
            } 

            // 1. src can be assigned to dst.
            var op = assignop(src, dst, why);
            if (op != 0L)
            {
                return op;
            } 

            // The rules for interfaces are no different in conversions
            // than assignments. If interfaces are involved, stop now
            // with the good message from assignop.
            // Otherwise clear the error.
            if (src.IsInterface() || dst.IsInterface())
            {
                return 0L;
            }
            if (why != null)
            {
                why.Value = "";
            } 

            // 2. Ignoring struct tags, src and dst have identical underlying types.
            if (eqtypeIgnoreTags(src.Orig, dst.Orig))
            {
                return OCONVNOP;
            } 

            // 3. src and dst are unnamed pointer types and, ignoring struct tags,
            // their base types have identical underlying types.
            if (src.IsPtr() && dst.IsPtr() && src.Sym == null && dst.Sym == null)
            {
                if (eqtypeIgnoreTags(src.Elem().Orig, dst.Elem().Orig))
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
                    return OARRAYBYTESTR;
                }
                if (src.Elem().Etype == types.Runetype.Etype)
                {
                    return OARRAYRUNESTR;
                }
            } 

            // 7. src is a string and dst is []byte or []rune.
            // String to slice.
            if (src.IsString() && dst.IsSlice())
            {
                if (dst.Elem().Etype == types.Bytetype.Etype)
                {
                    return OSTRARRAYBYTE;
                }
                if (dst.Elem().Etype == types.Runetype.Etype)
                {
                    return OSTRARRAYRUNE;
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
            return 0L;
        }

        private static ref Node assignconv(ref Node n, ref types.Type t, @string context)
        {
            return assignconvfn(n, t, () => context);
        }

        // Convert node n for assignment to type t.
        private static ref Node assignconvfn(ref Node n, ref types.Type t, Func<@string> context)
        {
            if (n == null || n.Type == null || n.Type.Broke())
            {
                return n;
            }
            if (t.Etype == TBLANK && n.Type.Etype == TNIL)
            {
                yyerror("use of untyped nil");
            }
            var old = n;
            var od = old.Diag();
            old.SetDiag(true); // silence errors about n; we'll issue one below
            n = defaultlit(n, t);
            old.SetDiag(od);
            if (t.Etype == TBLANK)
            {
                return n;
            } 

            // Convert ideal bool from comparison to plain bool
            // if the next step is non-bool (like interface{}).
            if (n.Type == types.Idealbool && !t.IsBoolean())
            {
                if (n.Op == ONAME || n.Op == OLITERAL)
                {
                    var r = nod(OCONVNOP, n, null);
                    r.Type = types.Types[TBOOL];
                    r.SetTypecheck(1L);
                    r.SetImplicit(true);
                    n = r;
                }
            }
            if (eqtype(n.Type, t))
            {
                return n;
            }
            @string why = default;
            var op = assignop(n.Type, t, ref why);
            if (op == 0L)
            {
                if (!old.Diag())
                {
                    yyerror("cannot use %L as type %v in %s%s", n, t, context(), why);
                }
                op = OCONV;
            }
            r = nod(op, n, null);
            r.Type = t;
            r.SetTypecheck(1L);
            r.SetImplicit(true);
            r.Orig = n.Orig;
            return r;
        }

        // IsMethod reports whether n is a method.
        // n must be a function or a method.
        private static bool IsMethod(this ref Node n)
        {
            return n.Type.Recv() != null;
        }

        // SliceBounds returns n's slice bounds: low, high, and max in expr[low:high:max].
        // n must be a slice expression. max is nil if n is a simple slice expression.
        private static (ref Node, ref Node, ref Node) SliceBounds(this ref Node n)
        {
            if (n.List.Len() == 0L)
            {
                return (null, null, null);
            }

            if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICESTR) 
                var s = n.List.Slice();
                return (s[0L], s[1L], null);
            else if (n.Op == OSLICE3 || n.Op == OSLICE3ARR) 
                s = n.List.Slice();
                return (s[0L], s[1L], s[2L]);
                        Fatalf("SliceBounds op %v: %v", n.Op, n);
            return (null, null, null);
        }

        // SetSliceBounds sets n's slice bounds, where n is a slice expression.
        // n must be a slice expression. If max is non-nil, n must be a full slice expression.
        private static void SetSliceBounds(this ref Node n, ref Node low, ref Node high, ref Node max)
        {

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
                        return;
                    }
                    n.List.Set2(low, high);
                    return;
                }
                s[0L] = low;
                s[1L] = high;
                return;
            else if (n.Op == OSLICE3 || n.Op == OSLICE3ARR) 
                s = n.List.Slice();
                if (s == null)
                {
                    if (low == null && high == null && max == null)
                    {
                        return;
                    }
                    n.List.Set3(low, high, max);
                    return;
                }
                s[0L] = low;
                s[1L] = high;
                s[2L] = max;
                return;
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

        // labeledControl returns the control flow Node (for, switch, select)
        // associated with the label n, if any.
        private static ref Node labeledControl(this ref Node n)
        {
            if (n.Op != OLABEL)
            {
                Fatalf("labeledControl %v", n.Op);
            }
            var ctl = n.Name.Defn;
            if (ctl == null)
            {
                return null;
            }

            if (ctl.Op == OFOR || ctl.Op == OFORUNTIL || ctl.Op == OSWITCH || ctl.Op == OSELECT) 
                return ctl;
                        return null;
        }

        private static ref Node syslook(@string name)
        {
            var s = Runtimepkg.Lookup(name);
            if (s == null || s.Def == null)
            {
                Fatalf("syslook: can't find runtime.%s", name);
            }
            return asNode(s.Def);
        }

        // typehash computes a hash value for type t to use in type switch statements.
        private static uint typehash(ref types.Type t)
        {
            var p = t.LongString(); 

            // Using MD5 is overkill, but reduces accidental collisions.
            var h = md5.Sum((slice<byte>)p);
            return binary.LittleEndian.Uint32(h[..4L]);
        }

        private static void frame(long context)
        {
            if (context != 0L)
            {
                fmt.Printf("--- external frame ---\n");
                foreach (var (_, n) in externdcl)
                {
                    printframenode(n);
                }
                return;
            }
            if (Curfn != null)
            {
                fmt.Printf("--- %v frame ---\n", Curfn.Func.Nname.Sym);
                foreach (var (_, ln) in Curfn.Func.Dcl)
                {
                    printframenode(ln);
                }
            }
        }

        private static void printframenode(ref Node n)
        {
            var w = int64(-1L);
            if (n.Type != null)
            {
                w = n.Type.Width;
            }

            if (n.Op == ONAME) 
                fmt.Printf("%v %v G%d %v width=%d\n", n.Op, n.Sym, n.Name.Vargen, n.Type, w);
            else if (n.Op == OTYPE) 
                fmt.Printf("%v %v width=%d\n", n.Op, n.Type, w);
                    }

        // updateHasCall checks whether expression n contains any function
        // calls and sets the n.HasCall flag if so.
        private static void updateHasCall(ref Node n)
        {
            if (n == null)
            {
                return;
            }
            n.SetHasCall(calcHasCall(n));
        }

        private static bool calcHasCall(ref Node n)
        {
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
            else if (n.Op == OINDEX || n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICE3 || n.Op == OSLICE3ARR || n.Op == OSLICESTR || n.Op == OIND || n.Op == ODOTPTR || n.Op == ODOTTYPE || n.Op == ODIV || n.Op == OMOD) 
                // These ops might panic, make sure they are done
                // before we start marshaling args for a call. See issue 16760.
                return true; 

                // When using soft-float, these ops might be rewritten to function calls
                // so we ensure they are evaluated first.
            else if (n.Op == OADD || n.Op == OSUB || n.Op == OMINUS) 
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

        private static void badtype(Op op, ref types.Type tl, ref types.Type tr)
        {
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
        private static ref Node safeexpr(ref Node n, ref Nodes init)
        {
            if (n == null)
            {
                return null;
            }
            if (n.Ninit.Len() != 0L)
            {
                walkstmtlist(n.Ninit.Slice());
                init.AppendNodes(ref n.Ninit);
            }

            if (n.Op == ONAME || n.Op == OLITERAL) 
                return n;
            else if (n.Op == ODOT || n.Op == OLEN || n.Op == OCAP) 
                var l = safeexpr(n.Left, init);
                if (l == n.Left)
                {
                    return n;
                }
                var r = nod(OXXX, null, null);
                r.Value = n.Value;
                r.Left = l;
                r = typecheck(r, Erv);
                r = walkexpr(r, init);
                return r;
            else if (n.Op == ODOTPTR || n.Op == OIND) 
                l = safeexpr(n.Left, init);
                if (l == n.Left)
                {
                    return n;
                }
                var a = nod(OXXX, null, null);
                a.Value = n.Value;
                a.Left = l;
                a = walkexpr(a, init);
                return a;
            else if (n.Op == OINDEX || n.Op == OINDEXMAP) 
                l = safeexpr(n.Left, init);
                r = safeexpr(n.Right, init);
                if (l == n.Left && r == n.Right)
                {
                    return n;
                }
                a = nod(OXXX, null, null);
                a.Value = n.Value;
                a.Left = l;
                a.Right = r;
                a = walkexpr(a, init);
                return a;
            else if (n.Op == OSTRUCTLIT || n.Op == OARRAYLIT || n.Op == OSLICELIT) 
                if (isStaticCompositeLiteral(n))
                {
                    return n;
                }
            // make a copy; must not be used as an lvalue
            if (islvalue(n))
            {
                Fatalf("missing lvalue case in safeexpr: %v", n);
            }
            return cheapexpr(n, init);
        }

        private static ref Node copyexpr(ref Node n, ref types.Type t, ref Nodes init)
        {
            var l = temp(t);
            var a = nod(OAS, l, n);
            a = typecheck(a, Etop);
            a = walkexpr(a, init);
            init.Append(a);
            return l;
        }

        // return side-effect free and cheap n, appending side effects to init.
        // result may not be assignable.
        private static ref Node cheapexpr(ref Node n, ref Nodes init)
        {

            if (n.Op == ONAME || n.Op == OLITERAL) 
                return n;
                        return copyexpr(n, n.Type, init);
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
        private static long lookdot0(ref types.Sym s, ref types.Type t, ptr<ptr<types.Field>> save, bool ignorecase)
        {
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
                        if (f.Sym == s || (ignorecase && f.Type.Etype == TFUNC && f.Type.Recv() != null && strings.EqualFold(f.Sym.Name, s.Name)))
                        {
                            if (save != null)
                            {
                                save.Value = f;
                            }
                            c++;
                        }
                    }

                    f = f__prev1;
                }

            }
            u = methtype(t);
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
                                save.Value = f;
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
        private static (long, bool) adddot1(ref types.Sym _s, ref types.Type _t, long d, ptr<ptr<types.Field>> save, bool ignorecase) => func(_s, _t, (ref types.Sym s, ref types.Type t, Defer defer, Panic _, Recover __) =>
        {
            if (t.Recur())
            {
                return;
            }
            t.SetRecur(true);
            defer(t.SetRecur(false));

            ref types.Type u = default;
            d--;
            if (d < 0L)
            { 
                // We've reached our target depth. If t has any fields/methods
                // named s, then we're done. Otherwise, we still need to check
                // below for embedded fields.
                c = lookdot0(s, t, save, ignorecase);
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
                var (a, more1) = adddot1(s, f.Type, d, save, ignorecase);
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
        private static (slice<Dlist>, bool) dotpath(ref types.Sym s, ref types.Type t, ptr<ptr<types.Field>> save, bool ignorecase)
        { 
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
                    var (c, more) = adddot1(s, t, d, save, ignorecase);

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
        private static ref Node adddot(ref Node n)
        {
            n.Left = typecheck(n.Left, Etype | Erv);
            if (n.Left.Diag())
            {
                n.SetDiag(true);
            }
            var t = n.Left.Type;
            if (t == null)
            {
                return n;
            }
            if (n.Left.Op == OTYPE)
            {
                return n;
            }
            var s = n.Sym;
            if (s == null)
            {
                return n;
            }
            {
                var (path, ambig) = dotpath(s, t, null, false);


                if (path != null) 
                    // rebuild elided dots
                    for (var c = len(path) - 1L; c >= 0L; c--)
                    {
                        n.Left = nodSym(ODOT, n.Left, path[c].field.Sym);
                        n.Left.SetImplicit(true);
                    }
                else if (ambig) 
                    yyerror("ambiguous selector %v", n);
                    n.Left = null;

            }

            return n;
        }

        // code to help generate trampoline
        // functions for methods on embedded
        // subtypes.
        // these are approx the same as
        // the corresponding adddot routines
        // except that they expect to be called
        // with unique tasks and they return
        // the actual methods.
        public partial struct Symlink
        {
            public ptr<types.Field> field;
            public bool followptr;
        }

        private static slice<Symlink> slist = default;

        private static void expand0(ref types.Type t, bool followptr)
        {
            var u = t;
            if (u.IsPtr())
            {
                followptr = true;
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
                        slist = append(slist, new Symlink(field:f,followptr:followptr));
                    }

                    f = f__prev1;
                }

                return;
            }
            u = methtype(t);
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
                        slist = append(slist, new Symlink(field:f,followptr:followptr));
                    }

                    f = f__prev1;
                }

            }
        }

        private static void expand1(ref types.Type t, bool top, bool followptr)
        {
            if (t.Recur())
            {
                return;
            }
            t.SetRecur(true);

            if (!top)
            {
                expand0(t, followptr);
            }
            var u = t;
            if (u.IsPtr())
            {
                followptr = true;
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
                    expand1(f.Type, false, followptr);
                }
            }
            t.SetRecur(false);
        }

        private static void expandmeth(ref types.Type t)
        {
            if (t == null || t.AllMethods().Len() != 0L)
            {
                return;
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
            expand1(t, true, false); 

            // check each method to be uniquely reachable
            slice<ref types.Field> ms = default;
            foreach (var (i, sl) in slist)
            {
                slist[i].field = null;
                sl.field.Sym.SetUniq(false);

                ref types.Field f = default;
                {
                    var (path, _) = dotpath(sl.field.Sym, t, ref f, false);

                    if (path == null)
                    {
                        continue;
                    } 

                    // dotpath may have dug out arbitrary fields, we only want methods.

                } 

                // dotpath may have dug out arbitrary fields, we only want methods.
                if (f.Type.Etype != TFUNC || f.Type.Recv() == null)
                {
                    continue;
                } 

                // add it to the base type method list
                f = f.Copy();
                f.Embedded = 1L; // needs a trampoline
                if (sl.followptr)
                {
                    f.Embedded = 2L;
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
            t.AllMethods().Set(ms);
        }

        // Given funarg struct list, return list of ODCLFIELD Node fn args.
        private static slice<ref Node> structargs(ref types.Type tl, bool mustname)
        {
            slice<ref Node> args = default;
            long gen = 0L;
            foreach (var (_, t) in tl.Fields().Slice())
            {
                ref Node n = default;
                if (mustname && (t.Sym == null || t.Sym.Name == "_"))
                { 
                    // invent a name so that we can refer to it in the trampoline
                    var buf = fmt.Sprintf(".anon%d", gen);
                    gen++;
                    n = newname(lookup(buf));
                }
                else if (t.Sym != null)
                {
                    n = newname(t.Sym);
                }
                var a = nod(ODCLFIELD, n, typenod(t.Type));
                a.SetIsddd(t.Isddd());
                if (n != null)
                {
                    n.SetIsddd(t.Isddd());
                }
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
        private static void genwrapper(ref types.Type rcvr, ref types.Field method, ref types.Sym newnam, bool iface)
        {
            if (false && Debug['r'] != 0L)
            {
                fmt.Printf("genwrapper rcvrtype=%v method=%v newnam=%v\n", rcvr, method, newnam);
            } 

            // Only generate (*T).M wrappers for T.M in T's own package.
            if (rcvr.IsPtr() && rcvr.Elem() == method.Type.Recv().Type && rcvr.Elem().Sym != null && rcvr.Elem().Sym.Pkg != localpkg)
            {
                return;
            }
            lineno = autogeneratedPos;

            dclcontext = PEXTERN;
            types.Markdcl();

            var @this = namedfield(".this", rcvr);
            @this.Left.Name.Param.Ntype = @this.Right;
            var @in = structargs(method.Type.Params(), true);
            var @out = structargs(method.Type.Results(), false);

            var t = nod(OTFUNC, null, null);
            ref Node l = new slice<ref Node>(new ref Node[] { this });
            if (iface && rcvr.Width < int64(Widthptr))
            { 
                // Building method for interface table and receiver
                // is smaller than the single pointer-sized word
                // that the interface call will pass in.
                // Add a dummy padding argument after the
                // receiver to make up the difference.
                var tpad = types.NewArray(types.Types[TUINT8], int64(Widthptr) - rcvr.Width);
                var pad = namedfield(".pad", tpad);
                l = append(l, pad);
            }
            t.List.Set(append(l, in));
            t.Rlist.Set(out);

            var fn = dclfunc(newnam, t);
            fn.Func.SetDupok(true);
            fn.Func.Nname.Sym.SetExported(true); // prevent export; see closure.go

            // arg list
            slice<ref Node> args = default;

            var isddd = false;
            {
                var n__prev1 = n;

                foreach (var (_, __n) in in)
                {
                    n = __n;
                    args = append(args, n.Left);
                    isddd = n.Left.Isddd();
                }

                n = n__prev1;
            }

            var methodrcvr = method.Type.Recv().Type; 

            // generate nil pointer check for better error
            if (rcvr.IsPtr() && rcvr.Elem() == methodrcvr)
            { 
                // generating wrapper from *T to T.
                var n = nod(OIF, null, null);
                n.Left = nod(OEQ, @this.Left, nodnil());
                var call = nod(OCALL, syslook("panicwrap"), null);
                n.Nbody.Set1(call);
                fn.Nbody.Append(n);
            }
            var dot = adddot(nodSym(OXDOT, @this.Left, method.Sym)); 

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
                    dot = nod(OADDR, dot, null);
                }
                var @as = nod(OAS, @this.Left, nod(OCONVNOP, dot, null));
                @as.Right.Type = rcvr;
                fn.Nbody.Append(as);
                fn.Nbody.Append(nodSym(ORETJMP, null, methodsym(method.Sym, methodrcvr, false))); 
                // When tail-calling, we can't use a frame pointer.
                fn.Func.SetNoFramePointer(true);
            }
            else
            {
                fn.Func.SetWrapper(true); // ignore frame for panic+recover matching
                call = nod(OCALL, dot, null);
                call.List.Set(args);
                call.SetIsddd(isddd);
                if (method.Type.NumResults() > 0L)
                {
                    n = nod(ORETURN, null, null);
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
            Curfn = fn;
            types.Popdcl();
            if (debug_dclstack != 0L)
            {
                testdclstack();
            } 

            // wrappers where T is anonymous (struct or interface) can be duplicated.
            if (rcvr.IsStruct() || rcvr.IsInterface() || rcvr.IsPtr() && rcvr.Elem().IsStruct())
            {
                fn.Func.SetDupok(true);
            }
            fn = typecheck(fn, Etop);
            typecheckslice(fn.Nbody.Slice(), Etop);

            inlcalls(fn);
            escAnalyze(new slice<ref Node>(new ref Node[] { fn }), false);

            Curfn = null;
            funccompile(fn);
        }

        private static ref Node hashmem(ref types.Type t)
        {
            var sym = Runtimepkg.Lookup("memhash");

            var n = newname(sym);
            n.SetClass(PFUNC);
            var tfn = nod(OTFUNC, null, null);
            tfn.List.Append(anonfield(types.NewPtr(t)));
            tfn.List.Append(anonfield(types.Types[TUINTPTR]));
            tfn.List.Append(anonfield(types.Types[TUINTPTR]));
            tfn.Rlist.Append(anonfield(types.Types[TUINTPTR]));
            tfn = typecheck(tfn, Etype);
            n.Type = tfn.Type;
            return n;
        }

        private static (ref types.Field, bool) ifacelookdot(ref types.Sym s, ref types.Type t, bool ignorecase)
        {
            if (t == null)
            {
                return (null, false);
            }
            var (path, ambig) = dotpath(s, t, ref m, ignorecase);
            if (path == null)
            {
                if (ambig)
                {
                    yyerror("%v.%v is ambiguous", t, s);
                }
                return (null, false);
            }
            foreach (var (_, d) in path)
            {
                if (d.field.Type.IsPtr())
                {
                    followptr = true;
                    break;
                }
            }
            if (m.Type.Etype != TFUNC || m.Type.Recv() == null)
            {
                yyerror("%v.%v is a field, not a method", t, s);
                return (null, followptr);
            }
            return (m, followptr);
        }

        private static bool implements(ref types.Type t, ref types.Type iface, ptr<ptr<types.Field>> m, ptr<ptr<types.Field>> samename, ref long ptr)
        {
            var t0 = t;
            if (t == null)
            {
                return false;
            } 

            // if this is too slow,
            // could sort these first
            // and then do one loop.
            if (t.IsInterface())
            {
Outer:

                {
                    var im__prev1 = im;

                    foreach (var (_, __im) in iface.Fields().Slice())
                    {
                        im = __im;
                        {
                            var tm__prev2 = tm;

                            foreach (var (_, __tm) in t.Fields().Slice())
                            {
                                tm = __tm;
                                if (tm.Sym == im.Sym)
                                {
                                    if (eqtype(tm.Type, im.Type))
                                    {
                                        _continueOuter = true;
                                        break;
                                    }
                                    m.Value = im;
                                    samename.Value = tm;
                                    ptr.Value = 0L;
                                    return false;
                                }
                            }

                            tm = tm__prev2;
                        }

                        m.Value = im;
                        samename.Value = null;
                        ptr.Value = 0L;
                        return false;
                    }

                    im = im__prev1;
                }
                return true;
            }
            t = methtype(t);
            if (t != null)
            {
                expandmeth(t);
            }
            {
                var im__prev1 = im;

                foreach (var (_, __im) in iface.Fields().Slice())
                {
                    im = __im;
                    if (im.Broke())
                    {
                        continue;
                    }
                    var (tm, followptr) = ifacelookdot(im.Sym, t, false);
                    if (tm == null || tm.Nointerface() || !eqtype(tm.Type, im.Type))
                    {
                        if (tm == null)
                        {
                            tm, followptr = ifacelookdot(im.Sym, t, true);
                        }
                        m.Value = im;
                        samename.Value = tm;
                        ptr.Value = 0L;
                        return false;
                    } 

                    // if pointer receiver in method,
                    // the method does not exist for value types.
                    var rcvr = tm.Type.Recv().Type;

                    if (rcvr.IsPtr() && !t0.IsPtr() && !followptr && !isifacemethod(tm.Type))
                    {
                        if (false && Debug['r'] != 0L)
                        {
                            yyerror("interface pointer mismatch");
                        }
                        m.Value = im;
                        samename.Value = null;
                        ptr.Value = 1L;
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

            if (isdirectiface(t0) && !iface.IsEmptyInterface())
            {
                itabname(t0, iface);
            }
            return true;
        }

        private static slice<ref Node> listtreecopy(slice<ref Node> l, src.XPos pos)
        {
            slice<ref Node> @out = default;
            foreach (var (_, n) in l)
            {
                out = append(out, treecopy(n, pos));
            }
            return out;
        }

        private static ref Node liststmt(slice<ref Node> l)
        {
            var n = nod(OBLOCK, null, null);
            n.List.Set(l);
            if (len(l) != 0L)
            {
                n.Pos = l[0L].Pos;
            }
            return n;
        }

        public static ref Node asblock(this Nodes l)
        {
            var n = nod(OBLOCK, null, null);
            n.List = l;
            if (l.Len() != 0L)
            {
                n.Pos = l.First().Pos;
            }
            return n;
        }

        private static ref types.Sym ngotype(ref Node n)
        {
            if (n.Type != null)
            {
                return typenamesym(n.Type);
            }
            return null;
        }

        // The result of addinit MUST be assigned back to n, e.g.
        //     n.Left = addinit(n.Left, init)
        private static ref Node addinit(ref Node n, slice<ref Node> init)
        {
            if (len(init) == 0L)
            {
                return n;
            }
            if (n.mayBeShared())
            { 
                // Introduce OCONVNOP to hold init list.
                n = nod(OCONVNOP, n, null);
                n.Type = n.Left.Type;
                n.SetTypecheck(1L);
            }
            n.Ninit.Prepend(init);
            n.SetHasCall(true);
            return n;
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

        private static void checknil(ref Node x, ref Nodes init)
        {
            x = walkexpr(x, null); // caller has not done this yet
            if (x.Type.IsInterface())
            {
                x = nod(OITAB, x, null);
                x = typecheck(x, Erv);
            }
            var n = nod(OCHECKNIL, x, null);
            n.SetTypecheck(1L);
            init.Append(n);
        }

        // Can this type be stored directly in an interface word?
        // Yes, if the representation is a single pointer.
        private static bool isdirectiface(ref types.Type t)
        {
            if (t.Broke())
            {
                return false;
            }

            if (t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TCHAN || t.Etype == TMAP || t.Etype == TFUNC || t.Etype == TUNSAFEPTR) 
                return true;
            else if (t.Etype == TARRAY) 
                // Array of 1 direct iface type can be direct.
                return t.NumElem() == 1L && isdirectiface(t.Elem());
            else if (t.Etype == TSTRUCT) 
                // Struct with 1 field of direct iface type can be direct.
                return t.NumFields() == 1L && isdirectiface(t.Field(0L).Type);
                        return false;
        }

        // itabType loads the _type field from a runtime.itab struct.
        private static ref Node itabType(ref Node itab)
        {
            var typ = nodSym(ODOTPTR, itab, null);
            typ.Type = types.NewPtr(types.Types[TUINT8]);
            typ.SetTypecheck(1L);
            typ.Xoffset = int64(Widthptr); // offset of _type in runtime.itab
            typ.SetBounded(true); // guaranteed not to fault
            return typ;
        }

        // ifaceData loads the data field from an interface.
        // The concrete type must be known to have type t.
        // It follows the pointer if !isdirectiface(t).
        private static ref Node ifaceData(ref Node n, ref types.Type t)
        {
            var ptr = nodSym(OIDATA, n, null);
            if (isdirectiface(t))
            {
                ptr.Type = t;
                ptr.SetTypecheck(1L);
                return ptr;
            }
            ptr.Type = types.NewPtr(t);
            ptr.SetBounded(true);
            ptr.SetTypecheck(1L);
            var ind = nod(OIND, ptr, null);
            ind.Type = t;
            ind.SetTypecheck(1L);
            return ind;
        }
    }
}}}}
