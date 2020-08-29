// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements various error reporters.

// package types -- go2cs converted at 2020 August 29 08:47:28 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\errors.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
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

        private static @string qualifier(this ref Checker check, ref Package pkg)
        {
            if (pkg != check.pkg)
            {
                return pkg.path;
            }
            return "";
        }

        private static @string sprintf(this ref Checker _check, @string format, params object[] args) => func(_check, (ref Checker check, Defer _, Panic panic, Recover __) =>
        {
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
                    case ref operand a:
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

        private static void trace(this ref Checker check, token.Pos pos, @string format, params object[] args)
        {
            fmt.Printf("%s:\t%s%s\n", check.fset.Position(pos), strings.Repeat(".  ", check.indent), check.sprintf(format, args));
        }

        // dump is only needed for debugging
        private static void dump(this ref Checker check, @string format, params object[] args)
        {
            fmt.Println(check.sprintf(format, args));
        }

        private static void err(this ref Checker _check, token.Pos pos, @string msg, bool soft) => func(_check, (ref Checker check, Defer _, Panic panic, Recover __) =>
        {
            Error err = new Error(check.fset,pos,msg,soft);
            if (check.firstErr == null)
            {
                check.firstErr = err;
            }
            var f = check.conf.Error;
            if (f == null)
            {
                panic(new bailout()); // report only first error
            }
            f(err);
        });

        private static void error(this ref Checker check, token.Pos pos, @string msg)
        {
            check.err(pos, msg, false);
        }

        private static void errorf(this ref Checker check, token.Pos pos, @string format, params object[] args)
        {
            check.err(pos, check.sprintf(format, args), false);
        }

        private static void softErrorf(this ref Checker check, token.Pos pos, @string format, params object[] args)
        {
            check.err(pos, check.sprintf(format, args), true);
        }

        private static void invalidAST(this ref Checker check, token.Pos pos, @string format, params object[] args)
        {
            check.errorf(pos, "invalid AST: " + format, args);
        }

        private static void invalidArg(this ref Checker check, token.Pos pos, @string format, params object[] args)
        {
            check.errorf(pos, "invalid argument: " + format, args);
        }

        private static void invalidOp(this ref Checker check, token.Pos pos, @string format, params object[] args)
        {
            check.errorf(pos, "invalid operation: " + format, args);
        }
    }
}}
