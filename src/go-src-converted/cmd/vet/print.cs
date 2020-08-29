// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the printf-checker.

// package main -- go2cs converted at 2020 August 29 10:09:27 UTC
// Original source: C:\Go\src\cmd\vet\print.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static var printfuncs = flag.String("printfuncs", "", "comma-separated list of print function names to check");

        private static void init()
        {
            register("printf", "check printf-like invocations", checkFmtPrintfCall, funcDecl, callExpr);
        }

        private static void initPrintFlags()
        {
            if (printfuncs == "".Value)
            {
                return;
            }
            foreach (var (_, name) in strings.Split(printfuncs.Value, ","))
            {
                if (len(name) == 0L)
                {
                    flag.Usage();
                } 

                // Backwards compatibility: skip optional first argument
                // index after the colon.
                {
                    var colon = strings.LastIndex(name, ":");

                    if (colon > 0L)
                    {
                        name = name[..colon];
                    }

                }

                isPrint[strings.ToLower(name)] = true;
            }
        }

        // TODO(rsc): Incorporate user-defined printf wrappers again.
        // The general plan is to allow vet of one package P to output
        // additional information to supply to later vets of packages
        // importing P. Then vet of P can record a list of printf wrappers
        // and the later vet using P.Printf will find it in the list and check it.
        // That's not ready for Go 1.10.
        // When that does happen, uncomment the user-defined printf
        // wrapper tests in testdata/print.go.

        // isPrint records the print functions.
        // If a key ends in 'f' then it is assumed to be a formatted print.
        private static map isPrint = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"fmt.Errorf":true,"fmt.Fprint":true,"fmt.Fprintf":true,"fmt.Fprintln":true,"fmt.Print":true,"fmt.Printf":true,"fmt.Println":true,"fmt.Sprint":true,"fmt.Sprintf":true,"fmt.Sprintln":true,"log.Fatal":true,"log.Fatalf":true,"log.Fatalln":true,"log.Logger.Fatal":true,"log.Logger.Fatalf":true,"log.Logger.Fatalln":true,"log.Logger.Panic":true,"log.Logger.Panicf":true,"log.Logger.Panicln":true,"log.Logger.Printf":true,"log.Logger.Println":true,"log.Panic":true,"log.Panicf":true,"log.Panicln":true,"log.Print":true,"log.Printf":true,"log.Println":true,"testing.B.Error":true,"testing.B.Errorf":true,"testing.B.Fatal":true,"testing.B.Fatalf":true,"testing.B.Log":true,"testing.B.Logf":true,"testing.B.Skip":true,"testing.B.Skipf":true,"testing.T.Error":true,"testing.T.Errorf":true,"testing.T.Fatal":true,"testing.T.Fatalf":true,"testing.T.Log":true,"testing.T.Logf":true,"testing.T.Skip":true,"testing.T.Skipf":true,"testing.TB.Error":true,"testing.TB.Errorf":true,"testing.TB.Fatal":true,"testing.TB.Fatalf":true,"testing.TB.Log":true,"testing.TB.Logf":true,"testing.TB.Skip":true,"testing.TB.Skipf":true,};

        // formatString returns the format string argument and its index within
        // the given printf-like call expression.
        //
        // The last parameter before variadic arguments is assumed to be
        // a format string.
        //
        // The first string literal or string constant is assumed to be a format string
        // if the call's signature cannot be determined.
        //
        // If it cannot find any format string parameter, it returns ("", -1).
        private static (@string, long) formatString(ref File f, ref ast.CallExpr call)
        {
            var typ = f.pkg.types[call.Fun].Type;
            if (typ != null)
            {
                {
                    ref types.Signature (sig, ok) = typ._<ref types.Signature>();

                    if (ok)
                    {
                        if (!sig.Variadic())
                        { 
                            // Skip checking non-variadic functions.
                            return ("", -1L);
                        }
                        var idx = sig.Params().Len() - 2L;
                        if (idx < 0L)
                        { 
                            // Skip checking variadic functions without
                            // fixed arguments.
                            return ("", -1L);
                        }
                        var (s, ok) = stringConstantArg(f, call, idx);
                        if (!ok)
                        { 
                            // The last argument before variadic args isn't a string.
                            return ("", -1L);
                        }
                        return (s, idx);
                    }

                }
            } 

            // Cannot determine call's signature. Fall back to scanning for the first
            // string constant in the call.
            {
                var idx__prev1 = idx;

                foreach (var (__idx) in call.Args)
                {
                    idx = __idx;
                    {
                        var s__prev1 = s;

                        (s, ok) = stringConstantArg(f, call, idx);

                        if (ok)
                        {
                            return (s, idx);
                        }

                        s = s__prev1;

                    }
                    if (f.pkg.types[call.Args[idx]].Type == types.Typ[types.String])
                    { 
                        // Skip checking a call with a non-constant format
                        // string argument, since its contents are unavailable
                        // for validation.
                        return ("", -1L);
                    }
                }

                idx = idx__prev1;
            }

            return ("", -1L);
        }

        // stringConstantArg returns call's string constant argument at the index idx.
        //
        // ("", false) is returned if call's argument at the index idx isn't a string
        // constant.
        private static (@string, bool) stringConstantArg(ref File f, ref ast.CallExpr call, long idx)
        {
            if (idx >= len(call.Args))
            {
                return ("", false);
            }
            var arg = call.Args[idx];
            var lit = f.pkg.types[arg].Value;
            if (lit != null && lit.Kind() == constant.String)
            {
                return (constant.StringVal(lit), true);
            }
            return ("", false);
        }

        // checkCall triggers the print-specific checks if the call invokes a print function.
        private static void checkFmtPrintfCall(ref File f, ast.Node node)
        {
            if (f.pkg.typesPkg == null)
            { 
                // This check now requires type information.
                return;
            }
            {
                ref ast.FuncDecl (d, ok) = node._<ref ast.FuncDecl>();

                if (ok && isStringer(f, d))
                { 
                    // Remember we saw this.
                    if (f.stringerPtrs == null)
                    {
                        f.stringerPtrs = make_map<ref ast.Object, bool>();
                    }
                    {
                        var l = d.Recv.List;

                        if (len(l) == 1L)
                        {
                            {
                                var n = l[0L].Names;

                                if (len(n) == 1L)
                                {
                                    var typ = f.pkg.types[l[0L].Type];
                                    ref types.Pointer (_, ptrRecv) = typ.Type._<ref types.Pointer>();
                                    f.stringerPtrs[n[0L].Obj] = ptrRecv;
                                }

                            }
                        }

                    }
                    return;
                }

            }

            ref ast.CallExpr (call, ok) = node._<ref ast.CallExpr>();
            if (!ok)
            {
                return;
            } 

            // Construct name like pkg.Printf or pkg.Type.Printf for lookup.
            @string name = default;
            switch (call.Fun.type())
            {
                case ref ast.Ident x:
                    {
                        ref types.Func (fn, ok) = f.pkg.uses[x]._<ref types.Func>();

                        if (ok)
                        {
                            @string pkg = default;
                            if (fn.Pkg() == null || fn.Pkg() == f.pkg.typesPkg)
                            {
                                pkg = vcfg.ImportPath;
                            }
                            else
                            {
                                pkg = fn.Pkg().Path();
                            }
                            name = pkg + "." + x.Name;
                            break;
                        }

                    }
                    break;
                case ref ast.SelectorExpr x:
                    {
                        ref ast.Ident (id, ok) = x.X._<ref ast.Ident>();

                        if (ok)
                        {
                            {
                                ref types.PkgName (pkgName, ok) = f.pkg.uses[id]._<ref types.PkgName>();

                                if (ok)
                                {
                                    name = pkgName.Imported().Path() + "." + x.Sel.Name;
                                    break;
                                }

                            }
                        } 

                        // Check for t.Logf where t is a *testing.T.

                    } 

                    // Check for t.Logf where t is a *testing.T.
                    {
                        var sel = f.pkg.selectors[x];

                        if (sel != null)
                        {
                            var recv = sel.Recv();
                            {
                                ref types.Pointer (p, ok) = recv._<ref types.Pointer>();

                                if (ok)
                                {
                                    recv = p.Elem();
                                }

                            }
                            {
                                ref types.Named (named, ok) = recv._<ref types.Named>();

                                if (ok)
                                {
                                    var obj = named.Obj();
                                    pkg = default;
                                    if (obj.Pkg() == null || obj.Pkg() == f.pkg.typesPkg)
                                    {
                                        pkg = vcfg.ImportPath;
                                    }
                                    else
                                    {
                                        pkg = obj.Pkg().Path();
                                    }
                                    name = pkg + "." + obj.Name() + "." + x.Sel.Name;
                                    break;
                                }

                            }
                        }

                    }
                    break;
            }
            if (name == "")
            {
                return;
            }
            var shortName = name[strings.LastIndex(name, ".") + 1L..];

            _, ok = isPrint[name];
            if (!ok)
            { 
                // Next look up just "printf", for use with -printfuncs.
                _, ok = isPrint[strings.ToLower(shortName)];
            }
            if (ok)
            {
                if (strings.HasSuffix(name, "f"))
                {
                    f.checkPrintf(call, shortName);
                }
                else
                {
                    f.checkPrint(call, shortName);
                }
            }
        }

        // isStringer returns true if the provided declaration is a "String() string"
        // method, an implementation of fmt.Stringer.
        private static bool isStringer(ref File f, ref ast.FuncDecl d)
        {
            return d.Recv != null && d.Name.Name == "String" && d.Type.Results != null && len(d.Type.Params.List) == 0L && len(d.Type.Results.List) == 1L && f.pkg.types[d.Type.Results.List[0L].Type].Type == types.Typ[types.String];
        }

        // isFormatter reports whether t satisfies fmt.Formatter.
        // Unlike fmt.Stringer, it's impossible to satisfy fmt.Formatter without importing fmt.
        private static bool isFormatter(this ref File f, types.Type t)
        {
            return formatterType != null && types.Implements(t, formatterType);
        }

        // formatState holds the parsed representation of a printf directive such as "%3.*[4]d".
        // It is constructed by parsePrintfVerb.
        private partial struct formatState
        {
            public int verb; // the format verb: 'd' for "%d"
            public @string format; // the full format directive from % through verb, "%.3d".
            public @string name; // Printf, Sprintf etc.
            public slice<byte> flags; // the list of # + etc.
            public slice<long> argNums; // the successive argument numbers that are consumed, adjusted to refer to actual arg in call
            public long firstArg; // Index of first argument after the format in the Printf call.
// Used only during parse.
            public ptr<File> file;
            public ptr<ast.CallExpr> call;
            public long argNum; // Which argument we're expecting to format now.
            public bool hasIndex; // Whether the argument is indexed.
            public bool indexPending; // Whether we have an indexed argument that has not resolved.
            public long nbytes; // number of bytes of the format string consumed.
        }

        // checkPrintf checks a call to a formatted print routine such as Printf.
        private static void checkPrintf(this ref File f, ref ast.CallExpr call, @string name)
        {
            var (format, idx) = formatString(f, call);
            if (idx < 0L)
            {
                if (verbose.Value)
                {
                    f.Warn(call.Pos(), "can't check non-constant format in call to", name);
                }
                return;
            }
            var firstArg = idx + 1L; // Arguments are immediately after format string.
            if (!strings.Contains(format, "%"))
            {
                if (len(call.Args) > firstArg)
                {
                    f.Badf(call.Pos(), "%s call has arguments but no formatting directives", name);
                }
                return;
            } 
            // Hard part: check formats against args.
            var argNum = firstArg;
            var maxArgNum = firstArg;
            var anyIndex = false;
            {
                long i = 0L;
                long w = 0L;

                while (i < len(format))
                {
                    w = 1L;
                    if (format[i] != '%')
                    {
                        continue;
                    i += w;
                    }
                    var state = f.parsePrintfVerb(call, name, format[i..], firstArg, argNum);
                    if (state == null)
                    {
                        return;
                    }
                    w = len(state.format);
                    if (!f.okPrintfArg(call, state))
                    { // One error per format is enough.
                        return;
                    }
                    if (state.hasIndex)
                    {
                        anyIndex = true;
                    }
                    if (len(state.argNums) > 0L)
                    { 
                        // Continue with the next sequential argument.
                        argNum = state.argNums[len(state.argNums) - 1L] + 1L;
                    }
                    foreach (var (_, n) in state.argNums)
                    {
                        if (n >= maxArgNum)
                        {
                            maxArgNum = n + 1L;
                        }
                    }
                } 
                // Dotdotdot is hard.

            } 
            // Dotdotdot is hard.
            if (call.Ellipsis.IsValid() && maxArgNum >= len(call.Args) - 1L)
            {
                return;
            } 
            // If any formats are indexed, extra arguments are ignored.
            if (anyIndex)
            {
                return;
            } 
            // There should be no leftover arguments.
            if (maxArgNum != len(call.Args))
            {
                var expect = maxArgNum - firstArg;
                var numArgs = len(call.Args) - firstArg;
                f.Badf(call.Pos(), "%s call needs %v but has %v", name, count(expect, "arg"), count(numArgs, "arg"));
            }
        }

        // parseFlags accepts any printf flags.
        private static void parseFlags(this ref formatState s)
        {
            while (s.nbytes < len(s.format))
            {
                {
                    var c = s.format[s.nbytes];

                    switch (c)
                    {
                        case '#': 

                        case '0': 

                        case '+': 

                        case '-': 

                        case ' ': 
                            s.flags = append(s.flags, c);
                            s.nbytes++;
                            break;
                        default: 
                            return;
                            break;
                    }
                }
            }

        }

        // scanNum advances through a decimal number if present.
        private static void scanNum(this ref formatState s)
        {
            while (s.nbytes < len(s.format))
            {
                var c = s.format[s.nbytes];
                if (c < '0' || '9' < c)
                {
                    return;
                s.nbytes++;
                }
            }

        }

        // parseIndex scans an index expression. It returns false if there is a syntax error.
        private static bool parseIndex(this ref formatState s)
        {
            if (s.nbytes == len(s.format) || s.format[s.nbytes] != '[')
            {
                return true;
            } 
            // Argument index present.
            s.nbytes++; // skip '['
            var start = s.nbytes;
            s.scanNum();
            var ok = true;
            if (s.nbytes == len(s.format) || s.nbytes == start || s.format[s.nbytes] != ']')
            {
                ok = false;
                s.nbytes = strings.Index(s.format, "]");
                if (s.nbytes < 0L)
                {
                    s.file.Badf(s.call.Pos(), "%s format %s is missing closing ]", s.name, s.format);
                    return false;
                }
            }
            var (arg32, err) = strconv.ParseInt(s.format[start..s.nbytes], 10L, 32L);
            if (err != null || !ok || arg32 <= 0L || arg32 > int64(len(s.call.Args) - s.firstArg))
            {
                s.file.Badf(s.call.Pos(), "%s format has invalid argument index [%s]", s.name, s.format[start..s.nbytes]);
                return false;
            }
            s.nbytes++; // skip ']'
            var arg = int(arg32);
            arg += s.firstArg - 1L; // We want to zero-index the actual arguments.
            s.argNum = arg;
            s.hasIndex = true;
            s.indexPending = true;
            return true;
        }

        // parseNum scans a width or precision (or *). It returns false if there's a bad index expression.
        private static bool parseNum(this ref formatState s)
        {
            if (s.nbytes < len(s.format) && s.format[s.nbytes] == '*')
            {
                if (s.indexPending)
                { // Absorb it.
                    s.indexPending = false;
                }
                s.nbytes++;
                s.argNums = append(s.argNums, s.argNum);
                s.argNum++;
            }
            else
            {
                s.scanNum();
            }
            return true;
        }

        // parsePrecision scans for a precision. It returns false if there's a bad index expression.
        private static bool parsePrecision(this ref formatState s)
        { 
            // If there's a period, there may be a precision.
            if (s.nbytes < len(s.format) && s.format[s.nbytes] == '.')
            {
                s.flags = append(s.flags, '.'); // Treat precision as a flag.
                s.nbytes++;
                if (!s.parseIndex())
                {
                    return false;
                }
                if (!s.parseNum())
                {
                    return false;
                }
            }
            return true;
        }

        // parsePrintfVerb looks the formatting directive that begins the format string
        // and returns a formatState that encodes what the directive wants, without looking
        // at the actual arguments present in the call. The result is nil if there is an error.
        private static ref formatState parsePrintfVerb(this ref File f, ref ast.CallExpr call, @string name, @string format, long firstArg, long argNum)
        {
            formatState state = ref new formatState(format:format,name:name,flags:make([]byte,0,5),argNum:argNum,argNums:make([]int,0,1),nbytes:1,firstArg:firstArg,file:f,call:call,); 
            // There may be flags.
            state.parseFlags(); 
            // There may be an index.
            if (!state.parseIndex())
            {
                return null;
            } 
            // There may be a width.
            if (!state.parseNum())
            {
                return null;
            } 
            // There may be a precision.
            if (!state.parsePrecision())
            {
                return null;
            } 
            // Now a verb, possibly prefixed by an index (which we may already have).
            if (!state.indexPending && !state.parseIndex())
            {
                return null;
            }
            if (state.nbytes == len(state.format))
            {
                f.Badf(call.Pos(), "%s format %s is missing verb at end of string", name, state.format);
                return null;
            }
            var (verb, w) = utf8.DecodeRuneInString(state.format[state.nbytes..]);
            state.verb = verb;
            state.nbytes += w;
            if (verb != '%')
            {
                state.argNums = append(state.argNums, state.argNum);
            }
            state.format = state.format[..state.nbytes];
            return state;
        }

        // printfArgType encodes the types of expressions a printf verb accepts. It is a bitmask.
        private partial struct printfArgType // : long
        {
        }

        private static readonly printfArgType argBool = 1L << (int)(iota);
        private static readonly var argInt = 0;
        private static readonly var argRune = 1;
        private static readonly var argString = 2;
        private static readonly var argFloat = 3;
        private static readonly var argComplex = 4;
        private static readonly var argPointer = 5;
        private static readonly printfArgType anyType = ~0L;

        private partial struct printVerb
        {
            public int verb; // User may provide verb through Formatter; could be a rune.
            public @string flags; // known flags are all ASCII
            public printfArgType typ;
        }

        // Common flag sets for printf verbs.
        private static readonly @string noFlag = "";
        private static readonly @string numFlag = " -+.0";
        private static readonly @string sharpNumFlag = " -+.0#";
        private static readonly @string allFlags = " -+.0#";

        // printVerbs identifies which flags are known to printf for each verb.
        private static printVerb printVerbs = new slice<printVerb>(new printVerb[] { {'%',noFlag,0}, {'b',numFlag,argInt|argFloat|argComplex}, {'c',"-",argRune|argInt}, {'d',numFlag,argInt}, {'e',sharpNumFlag,argFloat|argComplex}, {'E',sharpNumFlag,argFloat|argComplex}, {'f',sharpNumFlag,argFloat|argComplex}, {'F',sharpNumFlag,argFloat|argComplex}, {'g',sharpNumFlag,argFloat|argComplex}, {'G',sharpNumFlag,argFloat|argComplex}, {'o',sharpNumFlag,argInt}, {'p',"-#",argPointer}, {'q'," -+.0#",argRune|argInt|argString}, {'s'," -+.0",argString}, {'t',"-",argBool}, {'T',"-",anyType}, {'U',"-#",argRune|argInt}, {'v',allFlags,anyType}, {'x',sharpNumFlag,argRune|argInt|argString}, {'X',sharpNumFlag,argRune|argInt|argString} });

        // okPrintfArg compares the formatState to the arguments actually present,
        // reporting any discrepancies it can discern. If the final argument is ellipsissed,
        // there's little it can do for that.
        private static bool okPrintfArg(this ref File f, ref ast.CallExpr call, ref formatState state)
        {
            printVerb v = default;
            var found = false; 
            // Linear scan is fast enough for a small list.
            foreach (var (_, __v) in printVerbs)
            {
                v = __v;
                if (v.verb == state.verb)
                {
                    found = true;
                    break;
                }
            } 

            // Does current arg implement fmt.Formatter?

            var formatter = false;
            if (state.argNum < len(call.Args))
            {
                {
                    var (tv, ok) = f.pkg.types[call.Args[state.argNum]];

                    if (ok)
                    {
                        formatter = f.isFormatter(tv.Type);
                    }

                }
            }
            if (!formatter)
            {
                if (!found)
                {
                    f.Badf(call.Pos(), "%s format %s has unknown verb %c", state.name, state.format, state.verb);
                    return false;
                }
                foreach (var (_, flag) in state.flags)
                { 
                    // TODO: Disable complaint about '0' for Go 1.10. To be fixed properly in 1.11.
                    // See issues 23598 and 23605.
                    if (flag == '0')
                    {
                        continue;
                    }
                    if (!strings.ContainsRune(v.flags, rune(flag)))
                    {
                        f.Badf(call.Pos(), "%s format %s has unrecognized flag %c", state.name, state.format, flag);
                        return false;
                    }
                }
            } 
            // Verb is good. If len(state.argNums)>trueArgs, we have something like %.*s and all
            // but the final arg must be an integer.
            long trueArgs = 1L;
            if (state.verb == '%')
            {
                trueArgs = 0L;
            }
            var nargs = len(state.argNums);
            for (long i = 0L; i < nargs - trueArgs; i++)
            {
                var argNum = state.argNums[i];
                if (!f.argCanBeChecked(call, i, state))
                {
                    return;
                }
                var arg = call.Args[argNum];
                if (!f.matchArgType(argInt, null, arg))
                {
                    f.Badf(call.Pos(), "%s format %s uses non-int %s as argument of *", state.name, state.format, f.gofmt(arg));
                    return false;
                }
            }

            if (state.verb == '%' || formatter)
            {
                return true;
            }
            argNum = state.argNums[len(state.argNums) - 1L];
            if (!f.argCanBeChecked(call, len(state.argNums) - 1L, state))
            {
                return false;
            }
            arg = call.Args[argNum];
            if (f.isFunctionValue(arg) && state.verb != 'p' && state.verb != 'T')
            {
                f.Badf(call.Pos(), "%s format %s arg %s is a func value, not called", state.name, state.format, f.gofmt(arg));
                return false;
            }
            if (!f.matchArgType(v.typ, null, arg))
            {
                @string typeString = "";
                {
                    var typ = f.pkg.types[arg].Type;

                    if (typ != null)
                    {
                        typeString = typ.String();
                    }

                }
                f.Badf(call.Pos(), "%s format %s has arg %s of wrong type %s", state.name, state.format, f.gofmt(arg), typeString);
                return false;
            }
            if (v.typ & argString != 0L && v.verb != 'T' && !bytes.Contains(state.flags, new slice<byte>(new byte[] { '#' })) && f.recursiveStringer(arg))
            {
                f.Badf(call.Pos(), "%s format %s with arg %s causes recursive String method call", state.name, state.format, f.gofmt(arg));
                return false;
            }
            return true;
        }

        // recursiveStringer reports whether the provided argument is r or &r for the
        // fmt.Stringer receiver identifier r.
        private static bool recursiveStringer(this ref File f, ast.Expr e)
        {
            if (len(f.stringerPtrs) == 0L)
            {
                return false;
            }
            var ptr = false;
            ref ast.Object obj = default;
            switch (e.type())
            {
                case ref ast.Ident e:
                    obj = e.Obj;
                    break;
                case ref ast.UnaryExpr e:
                    {
                        ref ast.Ident (id, ok) = e.X._<ref ast.Ident>();

                        if (ok && e.Op == token.AND)
                        {
                            obj = id.Obj;
                            ptr = true;
                        }

                    }
                    break; 

                // It's unlikely to be a recursive stringer if it has a Format method.
            } 

            // It's unlikely to be a recursive stringer if it has a Format method.
            {
                var typ = f.pkg.types[e].Type;

                if (typ != null)
                { 
                    // Not a perfect match; see issue 6259.
                    if (f.hasMethod(typ, "Format"))
                    {
                        return false;
                    }
                } 

                // We compare the underlying Object, which checks that the identifier
                // is the one we declared as the receiver for the String method in
                // which this printf appears.

            } 

            // We compare the underlying Object, which checks that the identifier
            // is the one we declared as the receiver for the String method in
            // which this printf appears.
            var (ptrRecv, exist) = f.stringerPtrs[obj];
            if (!exist)
            {
                return false;
            } 
            // We also need to check that using &t when we declared String
            // on (t *T) is ok; in such a case, the address is printed.
            if (ptr && ptrRecv)
            {
                return false;
            }
            return true;
        }

        // isFunctionValue reports whether the expression is a function as opposed to a function call.
        // It is almost always a mistake to print a function value.
        private static bool isFunctionValue(this ref File f, ast.Expr e)
        {
            {
                var typ = f.pkg.types[e].Type;

                if (typ != null)
                {
                    ref types.Signature (_, ok) = typ._<ref types.Signature>();
                    return ok;
                }

            }
            return false;
        }

        // argCanBeChecked reports whether the specified argument is statically present;
        // it may be beyond the list of arguments or in a terminal slice... argument, which
        // means we can't see it.
        private static bool argCanBeChecked(this ref File _f, ref ast.CallExpr _call, long formatArg, ref formatState _state) => func(_f, _call, _state, (ref File f, ref ast.CallExpr call, ref formatState state, Defer _, Panic panic, Recover __) =>
        {
            var argNum = state.argNums[formatArg];
            if (argNum <= 0L)
            { 
                // Shouldn't happen, so catch it with prejudice.
                panic("negative arg num");
            }
            if (argNum < len(call.Args) - 1L)
            {
                return true; // Always OK.
            }
            if (call.Ellipsis.IsValid())
            {
                return false; // We just can't tell; there could be many more arguments.
            }
            if (argNum < len(call.Args))
            {
                return true;
            } 
            // There are bad indexes in the format or there are fewer arguments than the format needs.
            // This is the argument number relative to the format: Printf("%s", "hi") will give 1 for the "hi".
            var arg = argNum - state.firstArg + 1L; // People think of arguments as 1-indexed.
            f.Badf(call.Pos(), "%s format %s reads arg #%d, but call has only %v", state.name, state.format, arg, count(len(call.Args) - state.firstArg, "arg"));
            return false;
        });

        // printFormatRE is the regexp we match and report as a possible format string
        // in the first argument to unformatted prints like fmt.Print.
        // We exclude the space flag, so that printing a string like "x % y" is not reported as a format.
        private static var printFormatRE = regexp.MustCompile("%" + flagsRE + numOptRE + "\\.?" + numOptRE + indexOptRE + verbRE);

        private static readonly @string flagsRE = "[+\\-#]*";
        private static readonly @string indexOptRE = "(\\[[0-9]+\\])?";
        private static readonly @string numOptRE = "([0-9]+|" + indexOptRE + "\\*)?";
        private static readonly @string verbRE = "[bcdefgopqstvxEFGUX]";

        // checkPrint checks a call to an unformatted print routine such as Println.
        private static void checkPrint(this ref File f, ref ast.CallExpr call, @string name)
        {
            long firstArg = 0L;
            var typ = f.pkg.types[call.Fun].Type;
            if (typ == null)
            { 
                // Skip checking functions with unknown type.
                return;
            }
            {
                ref types.Signature (sig, ok) = typ._<ref types.Signature>();

                if (ok)
                {
                    if (!sig.Variadic())
                    { 
                        // Skip checking non-variadic functions.
                        return;
                    }
                    var @params = sig.Params();
                    firstArg = @params.Len() - 1L;

                    typ = @params.At(firstArg).Type();
                    typ = typ._<ref types.Slice>().Elem();
                    ref types.Interface (it, ok) = typ._<ref types.Interface>();
                    if (!ok || !it.Empty())
                    { 
                        // Skip variadic functions accepting non-interface{} args.
                        return;
                    }
                }

            }
            var args = call.Args;
            if (len(args) <= firstArg)
            { 
                // Skip calls without variadic args.
                return;
            }
            args = args[firstArg..];

            if (firstArg == 0L)
            {
                {
                    ref ast.SelectorExpr (sel, ok) = call.Args[0L]._<ref ast.SelectorExpr>();

                    if (ok)
                    {
                        {
                            ref ast.Ident (x, ok) = sel.X._<ref ast.Ident>();

                            if (ok)
                            {
                                if (x.Name == "os" && strings.HasPrefix(sel.Sel.Name, "Std"))
                                {
                                    f.Badf(call.Pos(), "%s does not take io.Writer but has first arg %s", name, f.gofmt(call.Args[0L]));
                                }
                            }

                        }
                    }

                }
            }
            var arg = args[0L];
            {
                ref ast.BasicLit lit__prev1 = lit;

                ref ast.BasicLit (lit, ok) = arg._<ref ast.BasicLit>();

                if (ok && lit.Kind == token.STRING)
                { 
                    // Ignore trailing % character in lit.Value.
                    // The % in "abc 0.0%" couldn't be a formatting directive.
                    var s = strings.TrimSuffix(lit.Value, "%\"");
                    if (strings.Contains(s, "%"))
                    {
                        var m = printFormatRE.FindStringSubmatch(s);
                        if (m != null)
                        {
                            f.Badf(call.Pos(), "%s call has possible formatting directive %s", name, m[0L]);
                        }
                    }
                }

                lit = lit__prev1;

            }
            if (strings.HasSuffix(name, "ln"))
            { 
                // The last item, if a string, should not have a newline.
                arg = args[len(args) - 1L];
                {
                    ref ast.BasicLit lit__prev2 = lit;

                    (lit, ok) = arg._<ref ast.BasicLit>();

                    if (ok && lit.Kind == token.STRING)
                    {
                        var (str, _) = strconv.Unquote(lit.Value);
                        if (strings.HasSuffix(str, "\n"))
                        {
                            f.Badf(call.Pos(), "%s arg list ends with redundant newline", name);
                        }
                    }

                    lit = lit__prev2;

                }
            }
            {
                var arg__prev1 = arg;

                foreach (var (_, __arg) in args)
                {
                    arg = __arg;
                    if (f.isFunctionValue(arg))
                    {
                        f.Badf(call.Pos(), "%s arg %s is a func value, not called", name, f.gofmt(arg));
                    }
                    if (f.recursiveStringer(arg))
                    {
                        f.Badf(call.Pos(), "%s arg %s causes recursive call to String method", name, f.gofmt(arg));
                    }
                }

                arg = arg__prev1;
            }

        }

        // count(n, what) returns "1 what" or "N whats"
        // (assuming the plural of what is whats).
        private static @string count(long n, @string what)
        {
            if (n == 1L)
            {
                return "1 " + what;
            }
            return fmt.Sprintf("%d %ss", n, what);
        }
    }
}
