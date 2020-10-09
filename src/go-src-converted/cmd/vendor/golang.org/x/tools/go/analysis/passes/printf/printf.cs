// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package printf defines an Analyzer that checks consistency
// of Printf format strings and arguments.
// package printf -- go2cs converted at 2020 October 09 06:04:42 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/printf" ==> using printf = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.printf_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\printf\printf.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using reflect = go.reflect_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class printf_package
    {
        private static void init()
        {
            Analyzer.Flags.Var(isPrint, "funcs", "comma-separated list of print function names to check");
        }

        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"printf",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,ResultType:reflect.TypeOf((*Result)(nil)),FactTypes:[]analysis.Fact{new(isWrapper)},));

        public static readonly @string Doc = (@string)@"check consistency of Printf format strings and arguments

The check applies to known functions (for example, those in package fmt)
as well as any detected wrappers of known functions.

A function that wants to avail itself of printf checking but is not
found by this analyzer's heuristics (for example, due to use of
dynamic calls) can insert a bogus call:

	if false {
		_ = fmt.Sprintf(format, args...) // enable printf checking
	}

The -funcs flag specifies a comma-separated list of names of additional
known formatting functions or methods. If the name contains a period,
it must denote a specific function using one of the following forms:

	dir/pkg.Function
	dir/pkg.Type.Method
	(*dir/pkg.Type).Method

Otherwise the name is interpreted as a case-insensitive unqualified
identifier such as ""errorf"". Either way, if a listed name ends in f, the
function is assumed to be Printf-like, taking a format string before the
argument list. Otherwise it is assumed to be Print-like, taking a list
of arguments with no format string.
";

        // Kind is a kind of fmt function behavior.


        // Kind is a kind of fmt function behavior.
        public partial struct Kind // : long
        {
        }

        public static readonly Kind KindNone = (Kind)iota; // not a fmt wrapper function
        public static readonly var KindPrint = 0; // function behaves like fmt.Print
        public static readonly var KindPrintf = 1; // function behaves like fmt.Printf
        public static readonly var KindErrorf = 2; // function behaves like fmt.Errorf

        public static @string String(this Kind kind)
        {

            if (kind == KindPrint) 
                return "print";
            else if (kind == KindPrintf) 
                return "printf";
            else if (kind == KindErrorf) 
                return "errorf";
                        return "";

        }

        // Result is the printf analyzer's result type. Clients may query the result
        // to learn whether a function behaves like fmt.Print or fmt.Printf.
        public partial struct Result
        {
            public map<ptr<types.Func>, Kind> funcs;
        }

        // Kind reports whether fn behaves like fmt.Print or fmt.Printf.
        private static Kind Kind(this ptr<Result> _addr_r, ptr<types.Func> _addr_fn)
        {
            ref Result r = ref _addr_r.val;
            ref types.Func fn = ref _addr_fn.val;

            var (_, ok) = isPrint[fn.FullName()];
            if (!ok)
            { 
                // Next look up just "printf", for use with -printf.funcs.
                _, ok = isPrint[strings.ToLower(fn.Name())];

            }

            if (ok)
            {
                if (strings.HasSuffix(fn.Name(), "f"))
                {
                    return KindPrintf;
                }
                else
                {
                    return KindPrint;
                }

            }

            return r.funcs[fn];

        }

        // isWrapper is a fact indicating that a function is a print or printf wrapper.
        private partial struct isWrapper
        {
            public Kind Kind;
        }

        private static void AFact(this ptr<isWrapper> _addr_f)
        {
            ref isWrapper f = ref _addr_f.val;

        }

        private static @string String(this ptr<isWrapper> _addr_f)
        {
            ref isWrapper f = ref _addr_f.val;


            if (f.Kind == KindPrintf) 
                return "printfWrapper";
            else if (f.Kind == KindPrint) 
                return "printWrapper";
            else if (f.Kind == KindErrorf) 
                return "errorfWrapper";
            else 
                return "unknownWrapper";
            
        }

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<Result> res = addr(new Result(funcs:make(map[*types.Func]Kind),));
            findPrintfLike(_addr_pass, res);
            checkCall(_addr_pass);
            return (res, error.As(null!)!);
        }

        private partial struct printfWrapper
        {
            public ptr<types.Func> obj;
            public ptr<ast.FuncDecl> fdecl;
            public ptr<types.Var> format;
            public ptr<types.Var> args;
            public slice<printfCaller> callers;
            public bool failed; // if true, not a printf wrapper
        }

        private partial struct printfCaller
        {
            public ptr<printfWrapper> w;
            public ptr<ast.CallExpr> call;
        }

        // maybePrintfWrapper decides whether decl (a declared function) may be a wrapper
        // around a fmt.Printf or fmt.Print function. If so it returns a printfWrapper
        // function describing the declaration. Later processing will analyze the
        // graph of potential printf wrappers to pick out the ones that are true wrappers.
        // A function may be a Printf or Print wrapper if its last argument is ...interface{}.
        // If the next-to-last argument is a string, then this may be a Printf wrapper.
        // Otherwise it may be a Print wrapper.
        private static ptr<printfWrapper> maybePrintfWrapper(ptr<types.Info> _addr_info, ast.Decl decl)
        {
            ref types.Info info = ref _addr_info.val;
 
            // Look for functions with final argument type ...interface{}.
            ptr<ast.FuncDecl> (fdecl, ok) = decl._<ptr<ast.FuncDecl>>();
            if (!ok || fdecl.Body == null)
            {
                return _addr_null!;
            }

            ptr<types.Func> (fn, ok) = info.Defs[fdecl.Name]._<ptr<types.Func>>(); 
            // Type information may be incomplete.
            if (!ok)
            {
                return _addr_null!;
            }

            ptr<types.Signature> sig = fn.Type()._<ptr<types.Signature>>();
            if (!sig.Variadic())
            {
                return _addr_null!; // not variadic
            }

            var @params = sig.Params();
            var nparams = @params.Len(); // variadic => nonzero

            var args = @params.At(nparams - 1L);
            ptr<types.Interface> (iface, ok) = args.Type()._<ptr<types.Slice>>().Elem()._<ptr<types.Interface>>();
            if (!ok || !iface.Empty())
            {
                return _addr_null!; // final (args) param is not ...interface{}
            } 

            // Is second last param 'format string'?
            ptr<types.Var> format;
            if (nparams >= 2L)
            {
                {
                    var p = @params.At(nparams - 2L);

                    if (p.Type() == types.Typ[types.String])
                    {
                        format = p;
                    }

                }

            }

            return addr(new printfWrapper(obj:fn,fdecl:fdecl,format:format,args:args,));

        }

        // findPrintfLike scans the entire package to find printf-like functions.
        private static (object, error) findPrintfLike(ptr<analysis.Pass> _addr_pass, ptr<Result> _addr_res)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;
            ref Result res = ref _addr_res.val;
 
            // Gather potential wrappers and call graph between them.
            var byObj = make_map<ptr<types.Func>, ptr<printfWrapper>>();
            slice<ptr<printfWrapper>> wrappers = default;
            foreach (var (_, file) in pass.Files)
            {
                foreach (var (_, decl) in file.Decls)
                {
                    var w = maybePrintfWrapper(_addr_pass.TypesInfo, decl);
                    if (w == null)
                    {
                        continue;
                    }

                    byObj[w.obj] = w;
                    wrappers = append(wrappers, w);

                }

            } 

            // Walk the graph to figure out which are really printf wrappers.
            {
                var w__prev1 = w;

                foreach (var (_, __w) in wrappers)
                {
                    w = __w; 
                    // Scan function for calls that could be to other printf-like functions.
                    ast.Inspect(w.fdecl.Body, n =>
                    {
                        if (w.failed)
                        {
                            return false;
                        } 

                        // TODO: Relax these checks; issue 26555.
                        {
                            ptr<ast.AssignStmt> (assign, ok) = n._<ptr<ast.AssignStmt>>();

                            if (ok)
                            {
                                foreach (var (_, lhs) in assign.Lhs)
                                {
                                    if (match(_addr_pass.TypesInfo, lhs, _addr_w.format) || match(_addr_pass.TypesInfo, lhs, _addr_w.args))
                                    { 
                                        // Modifies the format
                                        // string or args in
                                        // some way, so not a
                                        // simple wrapper.
                                        w.failed = true;
                                        return false;

                                    }

                                }

                            }

                        }

                        {
                            ptr<ast.UnaryExpr> (un, ok) = n._<ptr<ast.UnaryExpr>>();

                            if (ok && un.Op == token.AND)
                            {
                                if (match(_addr_pass.TypesInfo, un.X, _addr_w.format) || match(_addr_pass.TypesInfo, un.X, _addr_w.args))
                                { 
                                    // Taking the address of the
                                    // format string or args,
                                    // so not a simple wrapper.
                                    w.failed = true;
                                    return false;

                                }

                            }

                        }


                        ptr<ast.CallExpr> (call, ok) = n._<ptr<ast.CallExpr>>();
                        if (!ok || len(call.Args) == 0L || !match(_addr_pass.TypesInfo, call.Args[len(call.Args) - 1L], _addr_w.args))
                        {
                            return true;
                        }

                        var (fn, kind) = printfNameAndKind(_addr_pass, call);
                        if (kind != 0L)
                        {
                            checkPrintfFwd(_addr_pass, _addr_w, call, kind, _addr_res);
                            return true;
                        } 

                        // If the call is to another function in this package,
                        // maybe we will find out it is printf-like later.
                        // Remember this call for later checking.
                        if (fn != null && fn.Pkg() == pass.Pkg && byObj[fn] != null)
                        {
                            var callee = byObj[fn];
                            callee.callers = append(callee.callers, new printfCaller(w,call));
                        }

                        return true;

                    });

                }

                w = w__prev1;
            }

            return (null, error.As(null!)!);

        }

        private static bool match(ptr<types.Info> _addr_info, ast.Expr arg, ptr<types.Var> _addr_param)
        {
            ref types.Info info = ref _addr_info.val;
            ref types.Var param = ref _addr_param.val;

            ptr<ast.Ident> (id, ok) = arg._<ptr<ast.Ident>>();
            return ok && info.ObjectOf(id) == param;
        }

        // checkPrintfFwd checks that a printf-forwarding wrapper is forwarding correctly.
        // It diagnoses writing fmt.Printf(format, args) instead of fmt.Printf(format, args...).
        private static void checkPrintfFwd(ptr<analysis.Pass> _addr_pass, ptr<printfWrapper> _addr_w, ptr<ast.CallExpr> _addr_call, Kind kind, ptr<Result> _addr_res)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref printfWrapper w = ref _addr_w.val;
            ref ast.CallExpr call = ref _addr_call.val;
            ref Result res = ref _addr_res.val;

            var matched = kind == KindPrint || kind != KindNone && len(call.Args) >= 2L && match(_addr_pass.TypesInfo, call.Args[len(call.Args) - 2L], _addr_w.format);
            if (!matched)
            {
                return ;
            }

            if (!call.Ellipsis.IsValid())
            {
                ptr<types.Signature> (typ, ok) = pass.TypesInfo.Types[call.Fun].Type._<ptr<types.Signature>>();
                if (!ok)
                {
                    return ;
                }

                if (len(call.Args) > typ.Params().Len())
                { 
                    // If we're passing more arguments than what the
                    // print/printf function can take, adding an ellipsis
                    // would break the program. For example:
                    //
                    //   func foo(arg1 string, arg2 ...interface{} {
                    //       fmt.Printf("%s %v", arg1, arg2)
                    //   }
                    return ;

                }

                @string desc = "printf";
                if (kind == KindPrint)
                {
                    desc = "print";
                }

                pass.ReportRangef(call, "missing ... in args forwarded to %s-like function", desc);
                return ;

            }

            var fn = w.obj;
            ref isWrapper fact = ref heap(out ptr<isWrapper> _addr_fact);
            if (!pass.ImportObjectFact(fn, _addr_fact))
            {
                fact.Kind = kind;
                pass.ExportObjectFact(fn, _addr_fact);
                res.funcs[fn] = kind;
                foreach (var (_, caller) in w.callers)
                {
                    checkPrintfFwd(_addr_pass, _addr_caller.w, _addr_caller.call, kind, _addr_res);
                }

            }

        }

        // isPrint records the print functions.
        // If a key ends in 'f' then it is assumed to be a formatted print.
        //
        // Keys are either values returned by (*types.Func).FullName,
        // or case-insensitive identifiers such as "errorf".
        //
        // The -funcs flag adds to this set.
        //
        // The set below includes facts for many important standard library
        // functions, even though the analysis is capable of deducing that, for
        // example, fmt.Printf forwards to fmt.Fprintf. We avoid relying on the
        // driver applying analyzers to standard packages because "go vet" does
        // not do so with gccgo, and nor do some other build systems.
        // TODO(adonovan): eliminate the redundant facts once this restriction
        // is lifted.
        //
        private static stringSet isPrint = new stringSet("fmt.Errorf":true,"fmt.Fprint":true,"fmt.Fprintf":true,"fmt.Fprintln":true,"fmt.Print":true,"fmt.Printf":true,"fmt.Println":true,"fmt.Sprint":true,"fmt.Sprintf":true,"fmt.Sprintln":true,"runtime/trace.Logf":true,"log.Print":true,"log.Printf":true,"log.Println":true,"log.Fatal":true,"log.Fatalf":true,"log.Fatalln":true,"log.Panic":true,"log.Panicf":true,"log.Panicln":true,"(*log.Logger).Fatal":true,"(*log.Logger).Fatalf":true,"(*log.Logger).Fatalln":true,"(*log.Logger).Panic":true,"(*log.Logger).Panicf":true,"(*log.Logger).Panicln":true,"(*log.Logger).Print":true,"(*log.Logger).Printf":true,"(*log.Logger).Println":true,"(*testing.common).Error":true,"(*testing.common).Errorf":true,"(*testing.common).Fatal":true,"(*testing.common).Fatalf":true,"(*testing.common).Log":true,"(*testing.common).Logf":true,"(*testing.common).Skip":true,"(*testing.common).Skipf":true,"(testing.TB).Error":true,"(testing.TB).Errorf":true,"(testing.TB).Fatal":true,"(testing.TB).Fatalf":true,"(testing.TB).Log":true,"(testing.TB).Logf":true,"(testing.TB).Skip":true,"(testing.TB).Skipf":true,);

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
        private static (@string, long) formatString(ptr<analysis.Pass> _addr_pass, ptr<ast.CallExpr> _addr_call)
        {
            @string format = default;
            long idx = default;
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CallExpr call = ref _addr_call.val;

            var typ = pass.TypesInfo.Types[call.Fun].Type;
            if (typ != null)
            {
                {
                    ptr<types.Signature> (sig, ok) = typ._<ptr<types.Signature>>();

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

                        var (s, ok) = stringConstantArg(_addr_pass, _addr_call, idx);
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

                        (s, ok) = stringConstantArg(_addr_pass, _addr_call, idx);

                        if (ok)
                        {
                            return (s, idx);
                        }

                        s = s__prev1;

                    }

                    if (pass.TypesInfo.Types[call.Args[idx]].Type == types.Typ[types.String])
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
        private static (@string, bool) stringConstantArg(ptr<analysis.Pass> _addr_pass, ptr<ast.CallExpr> _addr_call, long idx)
        {
            @string _p0 = default;
            bool _p0 = default;
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CallExpr call = ref _addr_call.val;

            if (idx >= len(call.Args))
            {
                return ("", false);
            }

            var arg = call.Args[idx];
            var lit = pass.TypesInfo.Types[arg].Value;
            if (lit != null && lit.Kind() == constant.String)
            {
                return (constant.StringVal(lit), true);
            }

            return ("", false);

        }

        // checkCall triggers the print-specific checks if the call invokes a print function.
        private static void checkCall(ptr<analysis.Pass> _addr_pass)
        {
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();
            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });
            inspect.Preorder(nodeFilter, n =>
            {
                ptr<ast.CallExpr> call = n._<ptr<ast.CallExpr>>();
                var (fn, kind) = printfNameAndKind(_addr_pass, call);

                if (kind == KindPrintf || kind == KindErrorf) 
                    checkPrintf(_addr_pass, kind, call, _addr_fn);
                else if (kind == KindPrint) 
                    checkPrint(_addr_pass, call, _addr_fn);
                
            });

        }

        private static (ptr<types.Func>, Kind) printfNameAndKind(ptr<analysis.Pass> _addr_pass, ptr<ast.CallExpr> _addr_call)
        {
            ptr<types.Func> fn = default!;
            Kind kind = default;
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CallExpr call = ref _addr_call.val;

            fn, _ = typeutil.Callee(pass.TypesInfo, call)._<ptr<types.Func>>();
            if (fn == null)
            {
                return (_addr_null!, 0L);
            }

            var (_, ok) = isPrint[fn.FullName()];
            if (!ok)
            { 
                // Next look up just "printf", for use with -printf.funcs.
                _, ok = isPrint[strings.ToLower(fn.Name())];

            }

            if (ok)
            {
                if (fn.Name() == "Errorf")
                {
                    kind = KindErrorf;
                }
                else if (strings.HasSuffix(fn.Name(), "f"))
                {
                    kind = KindPrintf;
                }
                else
                {
                    kind = KindPrint;
                }

                return (_addr_fn!, kind);

            }

            ref isWrapper fact = ref heap(out ptr<isWrapper> _addr_fact);
            if (pass.ImportObjectFact(fn, _addr_fact))
            {
                return (_addr_fn!, fact.Kind);
            }

            return (_addr_fn!, KindNone);

        }

        // isFormatter reports whether t could satisfy fmt.Formatter.
        // The only interface method to look for is "Format(State, rune)".
        private static bool isFormatter(types.Type typ)
        { 
            // If the type is an interface, the value it holds might satisfy fmt.Formatter.
            {
                ptr<types.Interface> (_, ok) = typ.Underlying()._<ptr<types.Interface>>();

                if (ok)
                {
                    return true;
                }

            }

            var (obj, _, _) = types.LookupFieldOrMethod(typ, false, null, "Format");
            ptr<types.Func> (fn, ok) = obj._<ptr<types.Func>>();
            if (!ok)
            {
                return false;
            }

            ptr<types.Signature> sig = fn.Type()._<ptr<types.Signature>>();
            return sig.Params().Len() == 2L && sig.Results().Len() == 0L && isNamed(sig.Params().At(0L).Type(), "fmt", "State") && types.Identical(sig.Params().At(1L).Type(), types.Typ[types.Rune]);

        }

        private static bool isNamed(types.Type T, @string pkgpath, @string name)
        {
            ptr<types.Named> (named, ok) = T._<ptr<types.Named>>();
            return ok && named.Obj().Pkg().Path() == pkgpath && named.Obj().Name() == name;
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
            public ptr<analysis.Pass> pass;
            public ptr<ast.CallExpr> call;
            public long argNum; // Which argument we're expecting to format now.
            public bool hasIndex; // Whether the argument is indexed.
            public bool indexPending; // Whether we have an indexed argument that has not resolved.
            public long nbytes; // number of bytes of the format string consumed.
        }

        // checkPrintf checks a call to a formatted print routine such as Printf.
        private static void checkPrintf(ptr<analysis.Pass> _addr_pass, Kind kind, ptr<ast.CallExpr> _addr_call, ptr<types.Func> _addr_fn)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CallExpr call = ref _addr_call.val;
            ref types.Func fn = ref _addr_fn.val;

            var (format, idx) = formatString(_addr_pass, _addr_call);
            if (idx < 0L)
            {
                if (false)
                {
                    pass.Reportf(call.Lparen, "can't check non-constant format in call to %s", fn.Name());
                }

                return ;

            }

            var firstArg = idx + 1L; // Arguments are immediately after format string.
            if (!strings.Contains(format, "%"))
            {
                if (len(call.Args) > firstArg)
                {
                    pass.Reportf(call.Lparen, "%s call has arguments but no formatting directives", fn.Name());
                }

                return ;

            } 
            // Hard part: check formats against args.
            var argNum = firstArg;
            var maxArgNum = firstArg;
            var anyIndex = false;
            var anyW = false;
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

                    var state = parsePrintfVerb(_addr_pass, _addr_call, fn.Name(), format[i..], firstArg, argNum);
                    if (state == null)
                    {
                        return ;
                    }

                    w = len(state.format);
                    if (!okPrintfArg(_addr_pass, _addr_call, _addr_state))
                    { // One error per format is enough.
                        return ;

                    }

                    if (state.hasIndex)
                    {
                        anyIndex = true;
                    }

                    if (state.verb == 'w')
                    {
                        if (kind != KindErrorf)
                        {
                            pass.Reportf(call.Pos(), "%s call has error-wrapping directive %%w", state.name);
                            return ;
                        }

                        if (anyW)
                        {
                            pass.Reportf(call.Pos(), "%s call has more than one error-wrapping directive %%w", state.name);
                            return ;
                        }

                        anyW = true;

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
                return ;
            } 
            // If any formats are indexed, extra arguments are ignored.
            if (anyIndex)
            {
                return ;
            } 
            // There should be no leftover arguments.
            if (maxArgNum != len(call.Args))
            {
                var expect = maxArgNum - firstArg;
                var numArgs = len(call.Args) - firstArg;
                pass.ReportRangef(call, "%s call needs %v but has %v", fn.Name(), count(expect, "arg"), count(numArgs, "arg"));
            }

        }

        // parseFlags accepts any printf flags.
        private static void parseFlags(this ptr<formatState> _addr_s)
        {
            ref formatState s = ref _addr_s.val;

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
                            return ;
                            break;
                    }
                }

            }


        }

        // scanNum advances through a decimal number if present.
        private static void scanNum(this ptr<formatState> _addr_s)
        {
            ref formatState s = ref _addr_s.val;

            while (s.nbytes < len(s.format))
            {
                var c = s.format[s.nbytes];
                if (c < '0' || '9' < c)
                {
                    return ;
                s.nbytes++;
                }

            }


        }

        // parseIndex scans an index expression. It returns false if there is a syntax error.
        private static bool parseIndex(this ptr<formatState> _addr_s)
        {
            ref formatState s = ref _addr_s.val;

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
                    s.pass.ReportRangef(s.call, "%s format %s is missing closing ]", s.name, s.format);
                    return false;
                }

            }

            var (arg32, err) = strconv.ParseInt(s.format[start..s.nbytes], 10L, 32L);
            if (err != null || !ok || arg32 <= 0L || arg32 > int64(len(s.call.Args) - s.firstArg))
            {
                s.pass.ReportRangef(s.call, "%s format has invalid argument index [%s]", s.name, s.format[start..s.nbytes]);
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
        private static bool parseNum(this ptr<formatState> _addr_s)
        {
            ref formatState s = ref _addr_s.val;

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
        private static bool parsePrecision(this ptr<formatState> _addr_s)
        {
            ref formatState s = ref _addr_s.val;
 
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
        private static ptr<formatState> parsePrintfVerb(ptr<analysis.Pass> _addr_pass, ptr<ast.CallExpr> _addr_call, @string name, @string format, long firstArg, long argNum)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CallExpr call = ref _addr_call.val;

            ptr<formatState> state = addr(new formatState(format:format,name:name,flags:make([]byte,0,5),argNum:argNum,argNums:make([]int,0,1),nbytes:1,firstArg:firstArg,pass:pass,call:call,)); 
            // There may be flags.
            state.parseFlags(); 
            // There may be an index.
            if (!state.parseIndex())
            {
                return _addr_null!;
            } 
            // There may be a width.
            if (!state.parseNum())
            {
                return _addr_null!;
            } 
            // There may be a precision.
            if (!state.parsePrecision())
            {
                return _addr_null!;
            } 
            // Now a verb, possibly prefixed by an index (which we may already have).
            if (!state.indexPending && !state.parseIndex())
            {
                return _addr_null!;
            }

            if (state.nbytes == len(state.format))
            {
                pass.ReportRangef(call.Fun, "%s format %s is missing verb at end of string", name, state.format);
                return _addr_null!;
            }

            var (verb, w) = utf8.DecodeRuneInString(state.format[state.nbytes..]);
            state.verb = verb;
            state.nbytes += w;
            if (verb != '%')
            {
                state.argNums = append(state.argNums, state.argNum);
            }

            state.format = state.format[..state.nbytes];
            return _addr_state!;

        }

        // printfArgType encodes the types of expressions a printf verb accepts. It is a bitmask.
        private partial struct printfArgType // : long
        {
        }

        private static readonly printfArgType argBool = (printfArgType)1L << (int)(iota);
        private static readonly var argInt = 0;
        private static readonly var argRune = 1;
        private static readonly var argString = 2;
        private static readonly var argFloat = 3;
        private static readonly var argComplex = 4;
        private static readonly var argPointer = 5;
        private static readonly var argError = 6;
        private static readonly printfArgType anyType = (printfArgType)~0L;


        private partial struct printVerb
        {
            public int verb; // User may provide verb through Formatter; could be a rune.
            public @string flags; // known flags are all ASCII
            public printfArgType typ;
        }

        // Common flag sets for printf verbs.
        private static readonly @string noFlag = (@string)"";
        private static readonly @string numFlag = (@string)" -+.0";
        private static readonly @string sharpNumFlag = (@string)" -+.0#";
        private static readonly @string allFlags = (@string)" -+.0#";


        // printVerbs identifies which flags are known to printf for each verb.
        private static printVerb printVerbs = new slice<printVerb>(new printVerb[] { {'%',noFlag,0}, {'b',sharpNumFlag,argInt|argFloat|argComplex|argPointer}, {'c',"-",argRune|argInt}, {'d',numFlag,argInt|argPointer}, {'e',sharpNumFlag,argFloat|argComplex}, {'E',sharpNumFlag,argFloat|argComplex}, {'f',sharpNumFlag,argFloat|argComplex}, {'F',sharpNumFlag,argFloat|argComplex}, {'g',sharpNumFlag,argFloat|argComplex}, {'G',sharpNumFlag,argFloat|argComplex}, {'o',sharpNumFlag,argInt|argPointer}, {'O',sharpNumFlag,argInt|argPointer}, {'p',"-#",argPointer}, {'q'," -+.0#",argRune|argInt|argString}, {'s'," -+.0",argString}, {'t',"-",argBool}, {'T',"-",anyType}, {'U',"-#",argRune|argInt}, {'v',allFlags,anyType}, {'w',allFlags,argError}, {'x',sharpNumFlag,argRune|argInt|argString|argPointer|argFloat|argComplex}, {'X',sharpNumFlag,argRune|argInt|argString|argPointer|argFloat|argComplex} });

        // okPrintfArg compares the formatState to the arguments actually present,
        // reporting any discrepancies it can discern. If the final argument is ellipsissed,
        // there's little it can do for that.
        private static bool okPrintfArg(ptr<analysis.Pass> _addr_pass, ptr<ast.CallExpr> _addr_call, ptr<formatState> _addr_state)
        {
            bool ok = default;
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CallExpr call = ref _addr_call.val;
            ref formatState state = ref _addr_state.val;

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

            // Could current arg implement fmt.Formatter?

            var formatter = false;
            if (state.argNum < len(call.Args))
            {
                {
                    var (tv, ok) = pass.TypesInfo.Types[call.Args[state.argNum]];

                    if (ok)
                    {
                        formatter = isFormatter(tv.Type);
                    }

                }

            }

            if (!formatter)
            {
                if (!found)
                {
                    pass.ReportRangef(call, "%s format %s has unknown verb %c", state.name, state.format, state.verb);
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
                        pass.ReportRangef(call, "%s format %s has unrecognized flag %c", state.name, state.format, flag);
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
                if (!argCanBeChecked(_addr_pass, _addr_call, i, _addr_state))
                {
                    return ;
                }

                var arg = call.Args[argNum];
                if (!matchArgType(pass, argInt, null, arg))
                {
                    pass.ReportRangef(call, "%s format %s uses non-int %s as argument of *", state.name, state.format, analysisutil.Format(pass.Fset, arg));
                    return false;
                }

            }


            if (state.verb == '%' || formatter)
            {
                return true;
            }

            argNum = state.argNums[len(state.argNums) - 1L];
            if (!argCanBeChecked(_addr_pass, _addr_call, len(state.argNums) - 1L, _addr_state))
            {
                return false;
            }

            arg = call.Args[argNum];
            if (isFunctionValue(_addr_pass, arg) && state.verb != 'p' && state.verb != 'T')
            {
                pass.ReportRangef(call, "%s format %s arg %s is a func value, not called", state.name, state.format, analysisutil.Format(pass.Fset, arg));
                return false;
            }

            if (!matchArgType(pass, v.typ, null, arg))
            {
                @string typeString = "";
                {
                    var typ = pass.TypesInfo.Types[arg].Type;

                    if (typ != null)
                    {
                        typeString = typ.String();
                    }

                }

                pass.ReportRangef(call, "%s format %s has arg %s of wrong type %s", state.name, state.format, analysisutil.Format(pass.Fset, arg), typeString);
                return false;

            }

            if (v.typ & argString != 0L && v.verb != 'T' && !bytes.Contains(state.flags, new slice<byte>(new byte[] { '#' })))
            {
                {
                    var (methodName, ok) = recursiveStringer(_addr_pass, arg);

                    if (ok)
                    {
                        pass.ReportRangef(call, "%s format %s with arg %s causes recursive %s method call", state.name, state.format, analysisutil.Format(pass.Fset, arg), methodName);
                        return false;
                    }

                }

            }

            return true;

        }

        // recursiveStringer reports whether the argument e is a potential
        // recursive call to stringer or is an error, such as t and &t in these examples:
        //
        //     func (t *T) String() string { printf("%s",  t) }
        //     func (t  T) Error() string { printf("%s",  t) }
        //     func (t  T) String() string { printf("%s", &t) }
        private static (@string, bool) recursiveStringer(ptr<analysis.Pass> _addr_pass, ast.Expr e)
        {
            @string _p0 = default;
            bool _p0 = default;
            ref analysis.Pass pass = ref _addr_pass.val;

            var typ = pass.TypesInfo.Types[e].Type; 

            // It's unlikely to be a recursive stringer if it has a Format method.
            if (isFormatter(typ))
            {
                return ("", false);
            } 

            // Does e allow e.String() or e.Error()?
            var (strObj, _, _) = types.LookupFieldOrMethod(typ, false, pass.Pkg, "String");
            ptr<types.Func> (strMethod, strOk) = strObj._<ptr<types.Func>>();
            var (errObj, _, _) = types.LookupFieldOrMethod(typ, false, pass.Pkg, "Error");
            ptr<types.Func> (errMethod, errOk) = errObj._<ptr<types.Func>>();
            if (!strOk && !errOk)
            {
                return ("", false);
            } 

            // Is the expression e within the body of that String or Error method?
            ptr<types.Func> method;
            if (strOk && strMethod.Pkg() == pass.Pkg && strMethod.Scope().Contains(e.Pos()))
            {
                method = strMethod;
            }
            else if (errOk && errMethod.Pkg() == pass.Pkg && errMethod.Scope().Contains(e.Pos()))
            {
                method = errMethod;
            }
            else
            {
                return ("", false);
            }

            ptr<types.Signature> sig = method.Type()._<ptr<types.Signature>>();
            if (!isStringer(sig))
            {
                return ("", false);
            } 

            // Is it the receiver r, or &r?
            {
                ptr<ast.UnaryExpr> (u, ok) = e._<ptr<ast.UnaryExpr>>();

                if (ok && u.Op == token.AND)
                {
                    e = u.X; // strip off & from &r
                }

            }

            {
                ptr<ast.Ident> (id, ok) = e._<ptr<ast.Ident>>();

                if (ok)
                {
                    if (pass.TypesInfo.Uses[id] == sig.Recv())
                    {
                        return (method.Name(), true);
                    }

                }

            }

            return ("", false);

        }

        // isStringer reports whether the method signature matches the String() definition in fmt.Stringer.
        private static bool isStringer(ptr<types.Signature> _addr_sig)
        {
            ref types.Signature sig = ref _addr_sig.val;

            return sig.Params().Len() == 0L && sig.Results().Len() == 1L && sig.Results().At(0L).Type() == types.Typ[types.String];
        }

        // isFunctionValue reports whether the expression is a function as opposed to a function call.
        // It is almost always a mistake to print a function value.
        private static bool isFunctionValue(ptr<analysis.Pass> _addr_pass, ast.Expr e)
        {
            ref analysis.Pass pass = ref _addr_pass.val;

            {
                var typ = pass.TypesInfo.Types[e].Type;

                if (typ != null)
                {
                    ptr<types.Signature> (_, ok) = typ._<ptr<types.Signature>>();
                    return ok;
                }

            }

            return false;

        }

        // argCanBeChecked reports whether the specified argument is statically present;
        // it may be beyond the list of arguments or in a terminal slice... argument, which
        // means we can't see it.
        private static bool argCanBeChecked(ptr<analysis.Pass> _addr_pass, ptr<ast.CallExpr> _addr_call, long formatArg, ptr<formatState> _addr_state) => func((_, panic, __) =>
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CallExpr call = ref _addr_call.val;
            ref formatState state = ref _addr_state.val;

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
            pass.ReportRangef(call, "%s format %s reads arg #%d, but call has %v", state.name, state.format, arg, count(len(call.Args) - state.firstArg, "arg"));
            return false;

        });

        // printFormatRE is the regexp we match and report as a possible format string
        // in the first argument to unformatted prints like fmt.Print.
        // We exclude the space flag, so that printing a string like "x % y" is not reported as a format.
        private static var printFormatRE = regexp.MustCompile("%" + flagsRE + numOptRE + "\\.?" + numOptRE + indexOptRE + verbRE);

        private static readonly @string flagsRE = (@string)"[+\\-#]*";
        private static readonly @string indexOptRE = (@string)"(\\[[0-9]+\\])?";
        private static readonly @string numOptRE = (@string)"([0-9]+|" + indexOptRE + "\\*)?";
        private static readonly @string verbRE = (@string)"[bcdefgopqstvxEFGTUX]";


        // checkPrint checks a call to an unformatted print routine such as Println.
        private static void checkPrint(ptr<analysis.Pass> _addr_pass, ptr<ast.CallExpr> _addr_call, ptr<types.Func> _addr_fn)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.CallExpr call = ref _addr_call.val;
            ref types.Func fn = ref _addr_fn.val;

            long firstArg = 0L;
            var typ = pass.TypesInfo.Types[call.Fun].Type;
            if (typ == null)
            { 
                // Skip checking functions with unknown type.
                return ;

            }

            {
                ptr<types.Signature> (sig, ok) = typ._<ptr<types.Signature>>();

                if (ok)
                {
                    if (!sig.Variadic())
                    { 
                        // Skip checking non-variadic functions.
                        return ;

                    }

                    var @params = sig.Params();
                    firstArg = @params.Len() - 1L;

                    typ = @params.At(firstArg).Type();
                    typ = typ._<ptr<types.Slice>>().Elem();
                    ptr<types.Interface> (it, ok) = typ._<ptr<types.Interface>>();
                    if (!ok || !it.Empty())
                    { 
                        // Skip variadic functions accepting non-interface{} args.
                        return ;

                    }

                }

            }

            var args = call.Args;
            if (len(args) <= firstArg)
            { 
                // Skip calls without variadic args.
                return ;

            }

            args = args[firstArg..];

            if (firstArg == 0L)
            {
                {
                    ptr<ast.SelectorExpr> (sel, ok) = call.Args[0L]._<ptr<ast.SelectorExpr>>();

                    if (ok)
                    {
                        {
                            ptr<ast.Ident> (x, ok) = sel.X._<ptr<ast.Ident>>();

                            if (ok)
                            {
                                if (x.Name == "os" && strings.HasPrefix(sel.Sel.Name, "Std"))
                                {
                                    pass.ReportRangef(call, "%s does not take io.Writer but has first arg %s", fn.Name(), analysisutil.Format(pass.Fset, call.Args[0L]));
                                }

                            }

                        }

                    }

                }

            }

            var arg = args[0L];
            {
                ptr<ast.BasicLit> lit__prev1 = lit;

                ptr<ast.BasicLit> (lit, ok) = arg._<ptr<ast.BasicLit>>();

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
                            pass.ReportRangef(call, "%s call has possible formatting directive %s", fn.Name(), m[0L]);
                        }

                    }

                }

                lit = lit__prev1;

            }

            if (strings.HasSuffix(fn.Name(), "ln"))
            { 
                // The last item, if a string, should not have a newline.
                arg = args[len(args) - 1L];
                {
                    ptr<ast.BasicLit> lit__prev2 = lit;

                    (lit, ok) = arg._<ptr<ast.BasicLit>>();

                    if (ok && lit.Kind == token.STRING)
                    {
                        var (str, _) = strconv.Unquote(lit.Value);
                        if (strings.HasSuffix(str, "\n"))
                        {
                            pass.ReportRangef(call, "%s arg list ends with redundant newline", fn.Name());
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
                    if (isFunctionValue(_addr_pass, arg))
                    {
                        pass.ReportRangef(call, "%s arg %s is a func value, not called", fn.Name(), analysisutil.Format(pass.Fset, arg));
                    }

                    {
                        var (methodName, ok) = recursiveStringer(_addr_pass, arg);

                        if (ok)
                        {
                            pass.ReportRangef(call, "%s arg %s causes recursive call to %s method", fn.Name(), analysisutil.Format(pass.Fset, arg), methodName);
                        }

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

        // stringSet is a set-of-nonempty-strings-valued flag.
        // Note: elements without a '.' get lower-cased.
        private partial struct stringSet // : map<@string, bool>
        {
        }

        private static @string String(this stringSet ss)
        {
            slice<@string> list = default;
            foreach (var (name) in ss)
            {
                list = append(list, name);
            }
            sort.Strings(list);
            return strings.Join(list, ",");

        }

        private static error Set(this stringSet ss, @string flag)
        {
            foreach (var (_, name) in strings.Split(flag, ","))
            {
                if (len(name) == 0L)
                {
                    return error.As(fmt.Errorf("empty string"))!;
                }

                if (!strings.Contains(name, "."))
                {
                    name = strings.ToLower(name);
                }

                ss[name] = true;

            }
            return error.As(null!)!;

        }
    }
}}}}}}}}}
