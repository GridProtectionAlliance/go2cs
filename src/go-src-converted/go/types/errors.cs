// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements various error reporters.

// package types -- go2cs converted at 2020 October 08 04:03:10 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\errors.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        private static void assert(bool p) => func((_, panic, __) =>
        {
            if (!p)
            {
                panic("assertion failed");
            }
        });

        private static void unreachable() => func((_, panic, __) =>
        {
            panic("unreachable");
        });

        private static @string qualifier(this ptr<Checker> _addr_check, ptr<Package> _addr_pkg)
        {
            ref Checker check = ref _addr_check.val;
            ref Package pkg = ref _addr_pkg.val;
 
            // Qualify the package unless it's the package being type-checked.
            if (pkg != check.pkg)
            { 
                // If the same package name was used by multiple packages, display the full path.
                if (check.pkgCnt[pkg.name] > 1L)
                {
                    return strconv.Quote(pkg.path);
                }

                return pkg.name;

            }

            return "";

        }

        private static @string sprintf(this ptr<Checker> _addr_check, @string format, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();
            ref Checker check = ref _addr_check.val;

            foreach (var (i, arg) in args)
            {
                switch (arg.type())
                {
                    case 
                        arg = "<nil>";
                        break;
                    case operand a:
                        panic("internal error: should always pass *operand");
                        break;
                    case ptr<operand> a:
                        arg = operandString(a, check.qualifier);
                        break;
                    case token.Pos a:
                        arg = check.fset.Position(a).String();
                        break;
                    case ast.Expr a:
                        arg = ExprString(a);
                        break;
                    case Object a:
                        arg = ObjectString(a, check.qualifier);
                        break;
                    case Type a:
                        arg = TypeString(a, check.qualifier);
                        break;
                }
                args[i] = arg;

            }
            return fmt.Sprintf(format, args);

        });

        private static void trace(this ptr<Checker> _addr_check, token.Pos pos, @string format, params object[] args)
        {
            args = args.Clone();
            ref Checker check = ref _addr_check.val;

            fmt.Printf("%s:\t%s%s\n", check.fset.Position(pos), strings.Repeat(".  ", check.indent), check.sprintf(format, args));
        }

        // dump is only needed for debugging
        private static void dump(this ptr<Checker> _addr_check, @string format, params object[] args)
        {
            args = args.Clone();
            ref Checker check = ref _addr_check.val;

            fmt.Println(check.sprintf(format, args));
        }

        private static void err(this ptr<Checker> _addr_check, token.Pos pos, @string msg, bool soft) => func((_, panic, __) =>
        {
            ref Checker check = ref _addr_check.val;
 
            // Cheap trick: Don't report errors with messages containing
            // "invalid operand" or "invalid type" as those tend to be
            // follow-on errors which don't add useful information. Only
            // exclude them if these strings are not at the beginning,
            // and only if we have at least one error already reported.
            if (check.firstErr != null && (strings.Index(msg, "invalid operand") > 0L || strings.Index(msg, "invalid type") > 0L))
            {
                return ;
            }

            Error err = new Error(check.fset,pos,msg,soft);
            if (check.firstErr == null)
            {
                check.firstErr = err;
            }

            if (trace)
            {
                check.trace(pos, "ERROR: %s", msg);
            }

            var f = check.conf.Error;
            if (f == null)
            {
                panic(new bailout()); // report only first error
            }

            f(err);

        });

        private static void error(this ptr<Checker> _addr_check, token.Pos pos, @string msg)
        {
            ref Checker check = ref _addr_check.val;

            check.err(pos, msg, false);
        }

        private static void errorf(this ptr<Checker> _addr_check, token.Pos pos, @string format, params object[] args)
        {
            args = args.Clone();
            ref Checker check = ref _addr_check.val;

            check.err(pos, check.sprintf(format, args), false);
        }

        private static void softErrorf(this ptr<Checker> _addr_check, token.Pos pos, @string format, params object[] args)
        {
            args = args.Clone();
            ref Checker check = ref _addr_check.val;

            check.err(pos, check.sprintf(format, args), true);
        }

        private static void invalidAST(this ptr<Checker> _addr_check, token.Pos pos, @string format, params object[] args)
        {
            args = args.Clone();
            ref Checker check = ref _addr_check.val;

            check.errorf(pos, "invalid AST: " + format, args);
        }

        private static void invalidArg(this ptr<Checker> _addr_check, token.Pos pos, @string format, params object[] args)
        {
            args = args.Clone();
            ref Checker check = ref _addr_check.val;

            check.errorf(pos, "invalid argument: " + format, args);
        }

        private static void invalidOp(this ptr<Checker> _addr_check, token.Pos pos, @string format, params object[] args)
        {
            args = args.Clone();
            ref Checker check = ref _addr_check.val;

            check.errorf(pos, "invalid operation: " + format, args);
        }
    }
}}
