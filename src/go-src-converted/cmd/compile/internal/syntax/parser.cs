// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 August 29 09:26:16 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Go\src\cmd\compile\internal\syntax\parser.go
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class syntax_package
    {
        private static readonly var debug = false;

        private static readonly var trace = false;



        private partial struct parser
        {
            public ptr<src.PosBase> @base;
            public ErrorHandler errh;
            public FilenameHandler fileh;
            public Mode mode;
            public ref scanner scanner => ref scanner_val;
            public error first; // first error encountered
            public long errcnt; // number of errors encountered
            public Pragma pragma; // pragma flags

            public long fnest; // function nesting level (for error handling)
            public long xnest; // expression nesting level (for complit ambiguity resolution)
            public slice<byte> indent; // tracing support
        }

        private static void init(this ref parser p, ref src.PosBase @base, io.Reader r, ErrorHandler errh, PragmaHandler pragh, FilenameHandler fileh, Mode mode)
        {
            p.@base = base;
            p.errh = errh;
            p.fileh = fileh;
            p.mode = mode;
            p.scanner.init(r, (line, col, msg) =>
            {
                p.error_at(p.pos_at(line, col), msg);
            }, (line, col, text) =>
            {
                if (strings.HasPrefix(text, "line "))
                {
                    p.updateBase(line, col + 5L, text[5L..]);
                    return;
                }
                if (pragh != null)
                {
                    p.pragma |= pragh(p.pos_at(line, col), text);
                }
            });

            p.first = null;
            p.errcnt = 0L;
            p.pragma = 0L;

            p.fnest = 0L;
            p.xnest = 0L;
            p.indent = null;
        }

        private static readonly long lineMax = 1L << (int)(24L) - 1L; // TODO(gri) this limit is defined for src.Pos - fix

 // TODO(gri) this limit is defined for src.Pos - fix

        private static void updateBase(this ref parser p, ulong line, ulong col, @string text)
        { 
            // Want to use LastIndexByte below but it's not defined in Go1.4 and bootstrap fails.
            var i = strings.LastIndex(text, ":"); // look from right (Windows filenames may contain ':')
            if (i < 0L)
            {
                return; // ignore (not a line directive)
            }
            var nstr = text[i + 1L..];
            var (n, err) = strconv.Atoi(nstr);
            if (err != null || n <= 0L || n > lineMax)
            {
                p.error_at(p.pos_at(line, col + uint(i + 1L)), "invalid line number: " + nstr);
                return;
            }
            var filename = text[..i];
            var absFilename = filename;
            if (p.fileh != null)
            {
                absFilename = p.fileh(filename);
            }
            p.@base = src.NewLinePragmaBase(src.MakePos(p.@base.Pos().Base(), line, col), filename, absFilename, uint(n));
        }

        private static bool got(this ref parser p, token tok)
        {
            if (p.tok == tok)
            {
                p.next();
                return true;
            }
            return false;
        }

        private static void want(this ref parser p, token tok)
        {
            if (!p.got(tok))
            {
                p.syntax_error("expecting " + tokstring(tok));
                p.advance();
            }
        }

        // ----------------------------------------------------------------------------
        // Error handling

        // pos_at returns the Pos value for (line, col) and the current position base.
        private static src.Pos pos_at(this ref parser p, ulong line, ulong col)
        {
            return src.MakePos(p.@base, line, col);
        }

        // error reports an error at the given position.
        private static void error_at(this ref parser _p, src.Pos pos, @string msg) => func(_p, (ref parser p, Defer _, Panic panic, Recover __) =>
        {
            Error err = new Error(pos,msg);
            if (p.first == null)
            {
                p.first = err;
            }
            p.errcnt++;
            if (p.errh == null)
            {
                panic(p.first);
            }
            p.errh(err);
        });

        // syntax_error_at reports a syntax error at the given position.
        private static void syntax_error_at(this ref parser p, src.Pos pos, @string msg)
        {
            if (trace)
            {
                p.print("syntax error: " + msg);
            }
            if (p.tok == _EOF && p.first != null)
            {
                return; // avoid meaningless follow-up errors
            } 

            // add punctuation etc. as needed to msg

            if (msg == "")             else if (strings.HasPrefix(msg, "in") || strings.HasPrefix(msg, "at") || strings.HasPrefix(msg, "after")) 
                msg = " " + msg;
            else if (strings.HasPrefix(msg, "expecting")) 
                msg = ", " + msg;
            else 
                // plain error - we don't care about current token
                p.error_at(pos, "syntax error: " + msg);
                return;
            // determine token string
            @string tok = default;

            if (p.tok == _Name || p.tok == _Semi) 
                tok = p.lit;
            else if (p.tok == _Literal) 
                tok = "literal " + p.lit;
            else if (p.tok == _Operator) 
                tok = p.op.String();
            else if (p.tok == _AssignOp) 
                tok = p.op.String() + "=";
            else if (p.tok == _IncOp) 
                tok = p.op.String();
                tok += tok;
            else 
                tok = tokstring(p.tok);
                        p.error_at(pos, "syntax error: unexpected " + tok + msg);
        }

        // tokstring returns the English word for selected punctuation tokens
        // for more readable error messages.
        private static @string tokstring(token tok)
        {

            if (tok == _Comma) 
                return "comma";
            else if (tok == _Semi) 
                return "semicolon or newline";
                        return tok.String();
        }

        // Convenience methods using the current token position.
        private static src.Pos pos(this ref parser p)
        {
            return p.pos_at(p.line, p.col);
        }
        private static void error(this ref parser p, @string msg)
        {
            p.error_at(p.pos(), msg);

        }
        private static void syntax_error(this ref parser p, @string msg)
        {
            p.syntax_error_at(p.pos(), msg);

        }

        // The stopset contains keywords that start a statement.
        // They are good synchronization points in case of syntax
        // errors and (usually) shouldn't be skipped over.
        private static readonly ulong stopset = 1L << (int)(_Break) | 1L << (int)(_Const) | 1L << (int)(_Continue) | 1L << (int)(_Defer) | 1L << (int)(_Fallthrough) | 1L << (int)(_For) | 1L << (int)(_Go) | 1L << (int)(_Goto) | 1L << (int)(_If) | 1L << (int)(_Return) | 1L << (int)(_Select) | 1L << (int)(_Switch) | 1L << (int)(_Type) | 1L << (int)(_Var);

        // Advance consumes tokens until it finds a token of the stopset or followlist.
        // The stopset is only considered if we are inside a function (p.fnest > 0).
        // The followlist is the list of valid tokens that can follow a production;
        // if it is empty, exactly one (non-EOF) token is consumed to ensure progress.


        // Advance consumes tokens until it finds a token of the stopset or followlist.
        // The stopset is only considered if we are inside a function (p.fnest > 0).
        // The followlist is the list of valid tokens that can follow a production;
        // if it is empty, exactly one (non-EOF) token is consumed to ensure progress.
        private static void advance(this ref parser p, params token[] followlist)
        {
            if (trace)
            {
                p.print(fmt.Sprintf("advance %s", followlist));
            } 

            // compute follow set
            // (not speed critical, advance is only called in error situations)
            ulong followset = 1L << (int)(_EOF); // don't skip over EOF
            if (len(followlist) > 0L)
            {
                if (p.fnest > 0L)
                {
                    followset |= stopset;
                }
                foreach (var (_, tok) in followlist)
                {
                    followset |= 1L << (int)(tok);
                }
            }
            while (!contains(followset, p.tok))
            {
                if (trace)
                {
                    p.print("skip " + p.tok.String());
                }
                p.next();
                if (len(followlist) == 0L)
                {
                    break;
                }
            }


            if (trace)
            {
                p.print("next " + p.tok.String());
            }
        }

        // usage: defer p.trace(msg)()
        private static Action trace(this ref parser _p, @string msg) => func(_p, (ref parser p, Defer _, Panic panic, Recover __) =>
        {
            p.print(msg + " (");
            const @string tab = ". ";

            p.indent = append(p.indent, tab);
            return () =>
            {
                p.indent = p.indent[..len(p.indent) - len(tab)];
                {
                    var x = recover();

                    if (x != null)
                    {
                        panic(x); // skip print_trace
                    }

                }
                p.print(")");
            }
;
        });

        private static void print(this ref parser p, @string msg)
        {
            fmt.Printf("%5d: %s%s\n", p.line, p.indent, msg);
        }

        // ----------------------------------------------------------------------------
        // Package files
        //
        // Parse methods are annotated with matching Go productions as appropriate.
        // The annotations are intended as guidelines only since a single Go grammar
        // rule may be covered by multiple parse methods and vice versa.
        //
        // Excluding methods returning slices, parse methods named xOrNil may return
        // nil; all others are expected to return a valid non-nil node.

        // SourceFile = PackageClause ";" { ImportDecl ";" } { TopLevelDecl ";" } .
        private static ref File fileOrNil(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("file")());
            }
            ptr<File> f = @new<File>();
            f.pos = p.pos(); 

            // PackageClause
            if (!p.got(_Package))
            {
                p.syntax_error("package statement must be first");
                return null;
            }
            f.PkgName = p.name();
            p.want(_Semi); 

            // don't bother continuing if package clause has errors
            if (p.first != null)
            {
                return null;
            } 

            // { ImportDecl ";" }
            while (p.got(_Import))
            {
                f.DeclList = p.appendGroup(f.DeclList, p.importDecl);
                p.want(_Semi);
            } 

            // { TopLevelDecl ";" }
 

            // { TopLevelDecl ";" }
            while (p.tok != _EOF)
            {

                if (p.tok == _Const) 
                    p.next();
                    f.DeclList = p.appendGroup(f.DeclList, p.constDecl);
                else if (p.tok == _Type) 
                    p.next();
                    f.DeclList = p.appendGroup(f.DeclList, p.typeDecl);
                else if (p.tok == _Var) 
                    p.next();
                    f.DeclList = p.appendGroup(f.DeclList, p.varDecl);
                else if (p.tok == _Func) 
                    p.next();
                    {
                        var d = p.funcDeclOrNil();

                        if (d != null)
                        {
                            f.DeclList = append(f.DeclList, d);
                        }

                    }
                else 
                    if (p.tok == _Lbrace && len(f.DeclList) > 0L && isEmptyFuncDecl(f.DeclList[len(f.DeclList) - 1L]))
                    { 
                        // opening { of function declaration on next line
                        p.syntax_error("unexpected semicolon or newline before {");
                    }
                    else
                    {
                        p.syntax_error("non-declaration statement outside function body");
                    }
                    p.advance(_Const, _Type, _Var, _Func);
                    continue;
                // Reset p.pragma BEFORE advancing to the next token (consuming ';')
                // since comments before may set pragmas for the next function decl.
                p.pragma = 0L;

                if (p.tok != _EOF && !p.got(_Semi))
                {
                    p.syntax_error("after top level declaration");
                    p.advance(_Const, _Type, _Var, _Func);
                }
            } 
            // p.tok == _EOF
 
            // p.tok == _EOF

            f.Lines = p.source.line;

            return f;
        });

        private static bool isEmptyFuncDecl(Decl dcl)
        {
            ref FuncDecl (f, ok) = dcl._<ref FuncDecl>();
            return ok && f.Body == null;
        }

        // ----------------------------------------------------------------------------
        // Declarations

        // list parses a possibly empty, sep-separated list, optionally
        // followed by sep and enclosed by ( and ) or { and }. open is
        // one of _Lparen, or _Lbrace, sep is one of _Comma or _Semi,
        // and close is expected to be the (closing) opposite of open.
        // For each list element, f is called. After f returns true, no
        // more list elements are accepted. list returns the position
        // of the closing token.
        //
        // list = "(" { f sep } ")" |
        //        "{" { f sep } "}" . // sep is optional before ")" or "}"
        //
        private static src.Pos list(this ref parser p, token open, token sep, token close, Func<bool> f)
        {
            p.want(open);

            bool done = default;
            while (p.tok != _EOF && p.tok != close && !done)
            {
                done = f(); 
                // sep is optional before close
                if (!p.got(sep) && p.tok != close)
                {
                    p.syntax_error(fmt.Sprintf("expecting %s or %s", tokstring(sep), tokstring(close)));
                    p.advance(_Rparen, _Rbrack, _Rbrace);
                    if (p.tok != close)
                    { 
                        // position could be better but we had an error so we don't care
                        return p.pos();
                    }
                }
            }


            var pos = p.pos();
            p.want(close);
            return pos;
        }

        // appendGroup(f) = f | "(" { f ";" } ")" . // ";" is optional before ")"
        private static slice<Decl> appendGroup(this ref parser _p, slice<Decl> list, Func<ref Group, Decl> f) => func(_p, (ref parser p, Defer _, Panic panic, Recover __) =>
        {
            if (p.tok == _Lparen)
            {
                ptr<Group> g = @new<Group>();
                p.list(_Lparen, _Semi, _Rparen, () =>
                {
                    list = append(list, f(g));
                    return false;
                }
            else
);
            }            {
                list = append(list, f(null));
            }
            if (debug)
            {
                foreach (var (_, d) in list)
                {
                    if (d == null)
                    {
                        panic("nil list entry");
                    }
                }
            }
            return list;
        });

        // ImportSpec = [ "." | PackageName ] ImportPath .
        // ImportPath = string_lit .
        private static Decl importDecl(this ref parser _p, ref Group _group) => func(_p, _group, (ref parser p, ref Group group, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("importDecl")());
            }
            ptr<object> d = @new<ImportDecl>();
            d.pos = p.pos();


            if (p.tok == _Name) 
                d.LocalPkgName = p.name();
            else if (p.tok == _Dot) 
                d.LocalPkgName = p.newName(".");
                p.next();
                        d.Path = p.oliteral();
            if (d.Path == null)
            {
                p.syntax_error("missing import path");
                p.advance(_Semi, _Rparen);
                return null;
            }
            d.Group = group;

            return d;
        });

        // ConstSpec = IdentifierList [ [ Type ] "=" ExpressionList ] .
        private static Decl constDecl(this ref parser _p, ref Group _group) => func(_p, _group, (ref parser p, ref Group group, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("constDecl")());
            }
            ptr<object> d = @new<ConstDecl>();
            d.pos = p.pos();

            d.NameList = p.nameList(p.name());
            if (p.tok != _EOF && p.tok != _Semi && p.tok != _Rparen)
            {
                d.Type = p.typeOrNil();
                if (p.got(_Assign))
                {
                    d.Values = p.exprList();
                }
            }
            d.Group = group;

            return d;
        });

        // TypeSpec = identifier [ "=" ] Type .
        private static Decl typeDecl(this ref parser _p, ref Group _group) => func(_p, _group, (ref parser p, ref Group group, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("typeDecl")());
            }
            ptr<object> d = @new<TypeDecl>();
            d.pos = p.pos();

            d.Name = p.name();
            d.Alias = p.got(_Assign);
            d.Type = p.typeOrNil();
            if (d.Type == null)
            {
                d.Type = p.bad();
                p.syntax_error("in type declaration");
                p.advance(_Semi, _Rparen);
            }
            d.Group = group;
            d.Pragma = p.pragma;

            return d;
        });

        // VarSpec = IdentifierList ( Type [ "=" ExpressionList ] | "=" ExpressionList ) .
        private static Decl varDecl(this ref parser _p, ref Group _group) => func(_p, _group, (ref parser p, ref Group group, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("varDecl")());
            }
            ptr<object> d = @new<VarDecl>();
            d.pos = p.pos();

            d.NameList = p.nameList(p.name());
            if (p.got(_Assign))
            {
                d.Values = p.exprList();
            }
            else
            {
                d.Type = p.type_();
                if (p.got(_Assign))
                {
                    d.Values = p.exprList();
                }
            }
            d.Group = group;

            return d;
        });

        // FunctionDecl = "func" FunctionName ( Function | Signature ) .
        // FunctionName = identifier .
        // Function     = Signature FunctionBody .
        // MethodDecl   = "func" Receiver MethodName ( Function | Signature ) .
        // Receiver     = Parameters .
        private static ref FuncDecl funcDeclOrNil(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("funcDecl")());
            }
            ptr<FuncDecl> f = @new<FuncDecl>();
            f.pos = p.pos();

            if (p.tok == _Lparen)
            {
                var rcvr = p.paramList();
                switch (len(rcvr))
                {
                    case 0L: 
                        p.error("method has no receiver");
                        break;
                    case 1L: 
                        f.Recv = rcvr[0L];
                        break;
                    default: 
                        p.error("method has multiple receivers");
                        break;
                }
            }
            if (p.tok != _Name)
            {
                p.syntax_error("expecting name or (");
                p.advance(_Lbrace, _Semi);
                return null;
            }
            f.Name = p.name();
            f.Type = p.funcType();
            if (p.tok == _Lbrace)
            {
                f.Body = p.funcBody();
            }
            f.Pragma = p.pragma;

            return f;
        });

        private static ref BlockStmt funcBody(this ref parser p)
        {
            p.fnest++;
            var errcnt = p.errcnt;
            var body = p.blockStmt("");
            p.fnest--; 

            // Don't check branches if there were syntax errors in the function
            // as it may lead to spurious errors (e.g., see test/switch2.go) or
            // possibly crashes due to incomplete syntax trees.
            if (p.mode & CheckBranches != 0L && errcnt == p.errcnt)
            {
                checkBranches(body, p.errh);
            }
            return body;
        }

        // ----------------------------------------------------------------------------
        // Expressions

        private static Expr expr(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("expr")());
            }
            return p.binaryExpr(0L);
        });

        // Expression = UnaryExpr | Expression binary_op Expression .
        private static Expr binaryExpr(this ref parser p, long prec)
        { 
            // don't trace binaryExpr - only leads to overly nested trace output

            var x = p.unaryExpr();
            while ((p.tok == _Operator || p.tok == _Star) && p.prec > prec)
            {
                ptr<object> t = @new<Operation>();
                t.pos = p.pos();
                t.Op = p.op;
                t.X = x;
                var tprec = p.prec;
                p.next();
                t.Y = p.binaryExpr(tprec);
                x = t;
            }

            return x;
        }

        // UnaryExpr = PrimaryExpr | unary_op UnaryExpr .
        private static Expr unaryExpr(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("unaryExpr")());
            }

            if (p.tok == _Operator || p.tok == _Star) 

                if (p.op == Mul || p.op == Add || p.op == Sub || p.op == Not || p.op == Xor) 
                    ptr<object> x = @new<Operation>();
                    x.pos = p.pos();
                    x.Op = p.op;
                    p.next();
                    x.X = p.unaryExpr();
                    return x;
                else if (p.op == And) 
                    x = @new<Operation>();
                    x.pos = p.pos();
                    x.Op = And;
                    p.next(); 
                    // unaryExpr may have returned a parenthesized composite literal
                    // (see comment in operand) - remove parentheses if any
                    x.X = unparen(p.unaryExpr());
                    return x;
                            else if (p.tok == _Arrow) 
                // receive op (<-x) or receive-only channel (<-chan E)
                var pos = p.pos();
                p.next(); 

                // If the next token is _Chan we still don't know if it is
                // a channel (<-chan int) or a receive op (<-chan int(ch)).
                // We only know once we have found the end of the unaryExpr.

                x = p.unaryExpr(); 

                // There are two cases:
                //
                //   <-chan...  => <-x is a channel type
                //   <-x        => <-x is a receive operation
                //
                // In the first case, <- must be re-associated with
                // the channel type parsed already:
                //
                //   <-(chan E)   =>  (<-chan E)
                //   <-(chan<-E)  =>  (<-chan (<-E))

                {
                    ref ChanType (_, ok) = x._<ref ChanType>();

                    if (ok)
                    { 
                        // x is a channel type => re-associate <-
                        var dir = SendOnly;
                        var t = x;
                        while (dir == SendOnly)
                        {
                            ref ChanType (c, ok) = t._<ref ChanType>();
                            if (!ok)
                            {
                                break;
                            }
                            dir = c.Dir;
                            if (dir == RecvOnly)
                            { 
                                // t is type <-chan E but <-<-chan E is not permitted
                                // (report same error as for "type _ <-<-chan E")
                                p.syntax_error("unexpected <-, expecting chan"); 
                                // already progressed, no need to advance
                            }
                            c.Dir = RecvOnly;
                            t = c.Elem;
                        }

                        if (dir == SendOnly)
                        { 
                            // channel dir is <- but channel element E is not a channel
                            // (report same error as for "type _ <-chan<-E")
                            p.syntax_error(fmt.Sprintf("unexpected %s, expecting chan", String(t))); 
                            // already progressed, no need to advance
                        }
                        return x;
                    } 

                    // x is not a channel type => we have a receive op

                } 

                // x is not a channel type => we have a receive op
                ptr<object> o = @new<Operation>();
                o.pos = pos;
                o.Op = Recv;
                o.X = x;
                return o;
            // TODO(mdempsky): We need parens here so we can report an
            // error for "(x) := true". It should be possible to detect
            // and reject that more efficiently though.
            return p.pexpr(true);
        });

        // callStmt parses call-like statements that can be preceded by 'defer' and 'go'.
        private static ref CallStmt callStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("callStmt")());
            }
            ptr<CallStmt> s = @new<CallStmt>();
            s.pos = p.pos();
            s.Tok = p.tok; // _Defer or _Go
            p.next();

            var x = p.pexpr(p.tok == _Lparen); // keep_parens so we can report error below
            {
                var t = unparen(x);

                if (t != x)
                {
                    p.error(fmt.Sprintf("expression in %s must not be parenthesized", s.Tok)); 
                    // already progressed, no need to advance
                    x = t;
                }

            }

            ref CallExpr (cx, ok) = x._<ref CallExpr>();
            if (!ok)
            {
                p.error(fmt.Sprintf("expression in %s must be function call", s.Tok)); 
                // already progressed, no need to advance
                cx = @new<CallExpr>();
                cx.pos = x.Pos();
                cx.Fun = p.bad();
            }
            s.Call = cx;
            return s;
        });

        // Operand     = Literal | OperandName | MethodExpr | "(" Expression ")" .
        // Literal     = BasicLit | CompositeLit | FunctionLit .
        // BasicLit    = int_lit | float_lit | imaginary_lit | rune_lit | string_lit .
        // OperandName = identifier | QualifiedIdent.
        private static Expr operand(this ref parser _p, bool keep_parens) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("operand " + p.tok.String())());
            }

            if (p.tok == _Name) 
                return p.name();
            else if (p.tok == _Literal) 
                return p.oliteral();
            else if (p.tok == _Lparen) 
                var pos = p.pos();
                p.next();
                p.xnest++;
                var x = p.expr();
                p.xnest--;
                p.want(_Rparen); 

                // Optimization: Record presence of ()'s only where needed
                // for error reporting. Don't bother in other cases; it is
                // just a waste of memory and time.

                // Parentheses are not permitted on lhs of := .
                // switch x.Op {
                // case ONAME, ONONAME, OPACK, OTYPE, OLITERAL, OTYPESW:
                //     keep_parens = true
                // }

                // Parentheses are not permitted around T in a composite
                // literal T{}. If the next token is a {, assume x is a
                // composite literal type T (it may not be, { could be
                // the opening brace of a block, but we don't know yet).
                if (p.tok == _Lbrace)
                {
                    keep_parens = true;
                } 

                // Parentheses are also not permitted around the expression
                // in a go/defer statement. In that case, operand is called
                // with keep_parens set.
                if (keep_parens)
                {
                    ptr<object> px = @new<ParenExpr>();
                    px.pos = pos;
                    px.X = x;
                    x = px;
                }
                return x;
            else if (p.tok == _Func) 
                pos = p.pos();
                p.next();
                var t = p.funcType();
                if (p.tok == _Lbrace)
                {
                    p.xnest++;

                    ptr<object> f = @new<FuncLit>();
                    f.pos = pos;
                    f.Type = t;
                    f.Body = p.funcBody();

                    p.xnest--;
                    return f;
                }
                return t;
            else if (p.tok == _Lbrack || p.tok == _Chan || p.tok == _Map || p.tok == _Struct || p.tok == _Interface) 
                return p.type_(); // othertype
            else 
                x = p.bad();
                p.syntax_error("expecting expression");
                p.advance();
                return x;
            // Syntactically, composite literals are operands. Because a complit
            // type may be a qualified identifier which is handled by pexpr
            // (together with selector expressions), complits are parsed there
            // as well (operand is only called from pexpr).
        });

        // PrimaryExpr =
        //     Operand |
        //     Conversion |
        //     PrimaryExpr Selector |
        //     PrimaryExpr Index |
        //     PrimaryExpr Slice |
        //     PrimaryExpr TypeAssertion |
        //     PrimaryExpr Arguments .
        //
        // Selector       = "." identifier .
        // Index          = "[" Expression "]" .
        // Slice          = "[" ( [ Expression ] ":" [ Expression ] ) |
        //                      ( [ Expression ] ":" Expression ":" Expression )
        //                  "]" .
        // TypeAssertion  = "." "(" Type ")" .
        // Arguments      = "(" [ ( ExpressionList | Type [ "," ExpressionList ] ) [ "..." ] [ "," ] ] ")" .
        private static Expr pexpr(this ref parser _p, bool keep_parens) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("pexpr")());
            }
            var x = p.operand(keep_parens);

loop:

            while (true)
            {
                var pos = p.pos();

                if (p.tok == _Dot) 
                    p.next();

                    if (p.tok == _Name) 
                        // pexpr '.' sym
                        ptr<object> t = @new<SelectorExpr>();
                        t.pos = pos;
                        t.X = x;
                        t.Sel = p.name();
                        x = t;
                    else if (p.tok == _Lparen) 
                        p.next();
                        if (p.got(_Type))
                        {
                            t = @new<TypeSwitchGuard>();
                            t.pos = pos;
                            t.X = x;
                            x = t;
                        }
                        else
                        {
                            t = @new<AssertExpr>();
                            t.pos = pos;
                            t.X = x;
                            t.Type = p.expr();
                            x = t;
                        }
                        p.want(_Rparen);
                    else 
                        p.syntax_error("expecting name or (");
                        p.advance(_Semi, _Rparen);
                                    else if (p.tok == _Lbrack) 
                    p.next();
                    p.xnest++;

                    Expr i = default;
                    if (p.tok != _Colon)
                    {
                        i = p.expr();
                        if (p.got(_Rbrack))
                        { 
                            // x[i]
                            t = @new<IndexExpr>();
                            t.pos = pos;
                            t.X = x;
                            t.Index = i;
                            x = t;
                            p.xnest--;
                            break;
                        }
                    } 

                    // x[i:...
                    t = @new<SliceExpr>();
                    t.pos = pos;
                    t.X = x;
                    t.Index[0L] = i;
                    p.want(_Colon);
                    if (p.tok != _Colon && p.tok != _Rbrack)
                    { 
                        // x[i:j...
                        t.Index[1L] = p.expr();
                    }
                    if (p.got(_Colon))
                    {
                        t.Full = true; 
                        // x[i:j:...]
                        if (t.Index[1L] == null)
                        {
                            p.error("middle index required in 3-index slice");
                        }
                        if (p.tok != _Rbrack)
                        { 
                            // x[i:j:k...
                            t.Index[2L] = p.expr();
                        }
                        else
                        {
                            p.error("final index required in 3-index slice");
                        }
                    }
                    p.want(_Rbrack);

                    x = t;
                    p.xnest--;
                else if (p.tok == _Lparen) 
                    t = @new<CallExpr>();
                    t.pos = pos;
                    t.Fun = x;
                    t.ArgList, t.HasDots = p.argList();
                    x = t;
                else if (p.tok == _Lbrace) 
                    // operand may have returned a parenthesized complit
                    // type; accept it but complain if we have a complit
                    t = unparen(x); 
                    // determine if '{' belongs to a composite literal or a block statement
                    var complit_ok = false;
                    switch (t.type())
                    {
                        case ref Name _:
                            if (p.xnest >= 0L)
                            { 
                                // x is considered a composite literal type
                                complit_ok = true;
                            }
                            break;
                        case ref SelectorExpr _:
                            if (p.xnest >= 0L)
                            { 
                                // x is considered a composite literal type
                                complit_ok = true;
                            }
                            break;
                        case ref ArrayType _:
                            complit_ok = true;
                            break;
                        case ref SliceType _:
                            complit_ok = true;
                            break;
                        case ref StructType _:
                            complit_ok = true;
                            break;
                        case ref MapType _:
                            complit_ok = true;
                            break;
                    }
                    if (!complit_ok)
                    {
                        _breakloop = true;
                        break;
                    }
                    if (t != x)
                    {
                        p.syntax_error("cannot parenthesize type in composite literal"); 
                        // already progressed, no need to advance
                    }
                    var n = p.complitexpr();
                    n.Type = x;
                    x = n;
                else 
                    _breakloop = true;
                    break;
                            }

            return x;
        });

        // Element = Expression | LiteralValue .
        private static Expr bare_complitexpr(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("bare_complitexpr")());
            }
            if (p.tok == _Lbrace)
            { 
                // '{' start_complit braced_keyval_list '}'
                return p.complitexpr();
            }
            return p.expr();
        });

        // LiteralValue = "{" [ ElementList [ "," ] ] "}" .
        private static ref CompositeLit complitexpr(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("complitexpr")());
            }
            ptr<CompositeLit> x = @new<CompositeLit>();
            x.pos = p.pos();

            p.xnest++;
            x.Rbrace = p.list(_Lbrace, _Comma, _Rbrace, () =>
            { 
                // value
                var e = p.bare_complitexpr();
                if (p.tok == _Colon)
                { 
                    // key ':' value
                    ptr<object> l = @new<KeyValueExpr>();
                    l.pos = p.pos();
                    p.next();
                    l.Key = e;
                    l.Value = p.bare_complitexpr();
                    e = l;
                    x.NKeys++;
                }
                x.ElemList = append(x.ElemList, e);
                return false;
            });
            p.xnest--;

            return x;
        });

        // ----------------------------------------------------------------------------
        // Types

        private static Expr type_(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("type_")());
            }
            var typ = p.typeOrNil();
            if (typ == null)
            {
                typ = p.bad();
                p.syntax_error("expecting type");
                p.advance();
            }
            return typ;
        });

        private static Expr newIndirect(src.Pos pos, Expr typ)
        {
            ptr<object> o = @new<Operation>();
            o.pos = pos;
            o.Op = Mul;
            o.X = typ;
            return o;
        }

        // typeOrNil is like type_ but it returns nil if there was no type
        // instead of reporting an error.
        //
        // Type     = TypeName | TypeLit | "(" Type ")" .
        // TypeName = identifier | QualifiedIdent .
        // TypeLit  = ArrayType | StructType | PointerType | FunctionType | InterfaceType |
        //           SliceType | MapType | Channel_Type .
        private static Expr typeOrNil(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("typeOrNil")());
            }
            var pos = p.pos();

            if (p.tok == _Star) 
                // ptrtype
                p.next();
                return newIndirect(pos, p.type_());
            else if (p.tok == _Arrow) 
                // recvchantype
                p.next();
                p.want(_Chan);
                ptr<ChanType> t = @new<ChanType>();
                t.pos = pos;
                t.Dir = RecvOnly;
                t.Elem = p.chanElem();
                return t;
            else if (p.tok == _Func) 
                // fntype
                p.next();
                return p.funcType();
            else if (p.tok == _Lbrack) 
                // '[' oexpr ']' ntype
                // '[' _DotDotDot ']' ntype
                p.next();
                p.xnest++;
                if (p.got(_Rbrack))
                { 
                    // []T
                    p.xnest--;
                    t = @new<SliceType>();
                    t.pos = pos;
                    t.Elem = p.type_();
                    return t;
                } 

                // [n]T
                t = @new<ArrayType>();
                t.pos = pos;
                if (!p.got(_DotDotDot))
                {
                    t.Len = p.expr();
                }
                p.want(_Rbrack);
                p.xnest--;
                t.Elem = p.type_();
                return t;
            else if (p.tok == _Chan) 
                // _Chan non_recvchantype
                // _Chan _Comm ntype
                p.next();
                t = @new<ChanType>();
                t.pos = pos;
                if (p.got(_Arrow))
                {
                    t.Dir = SendOnly;
                }
                t.Elem = p.chanElem();
                return t;
            else if (p.tok == _Map) 
                // _Map '[' ntype ']' ntype
                p.next();
                p.want(_Lbrack);
                t = @new<MapType>();
                t.pos = pos;
                t.Key = p.type_();
                p.want(_Rbrack);
                t.Value = p.type_();
                return t;
            else if (p.tok == _Struct) 
                return p.structType();
            else if (p.tok == _Interface) 
                return p.interfaceType();
            else if (p.tok == _Name) 
                return p.dotname(p.name());
            else if (p.tok == _Lparen) 
                p.next();
                t = p.type_();
                p.want(_Rparen);
                return t;
                        return null;
        });

        private static ref FuncType funcType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("funcType")());
            }
            ptr<FuncType> typ = @new<FuncType>();
            typ.pos = p.pos();
            typ.ParamList = p.paramList();
            typ.ResultList = p.funcResult();

            return typ;
        });

        private static Expr chanElem(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("chanElem")());
            }
            var typ = p.typeOrNil();
            if (typ == null)
            {
                typ = p.bad();
                p.syntax_error("missing channel element type"); 
                // assume element type is simply absent - don't advance
            }
            return typ;
        });

        private static Expr dotname(this ref parser _p, ref Name _name) => func(_p, _name, (ref parser p, ref Name name, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("dotname")());
            }
            if (p.tok == _Dot)
            {
                ptr<SelectorExpr> s = @new<SelectorExpr>();
                s.pos = p.pos();
                p.next();
                s.X = name;
                s.Sel = p.name();
                return s;
            }
            return name;
        });

        // StructType = "struct" "{" { FieldDecl ";" } "}" .
        private static ref StructType structType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("structType")());
            }
            ptr<StructType> typ = @new<StructType>();
            typ.pos = p.pos();

            p.want(_Struct);
            p.list(_Lbrace, _Semi, _Rbrace, () =>
            {
                p.fieldDecl(typ);
                return false;
            });

            return typ;
        });

        // InterfaceType = "interface" "{" { MethodSpec ";" } "}" .
        private static ref InterfaceType interfaceType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("interfaceType")());
            }
            ptr<InterfaceType> typ = @new<InterfaceType>();
            typ.pos = p.pos();

            p.want(_Interface);
            p.list(_Lbrace, _Semi, _Rbrace, () =>
            {
                {
                    var m = p.methodDecl();

                    if (m != null)
                    {
                        typ.MethodList = append(typ.MethodList, m);
                    }

                }
                return false;
            });

            return typ;
        });

        // Result = Parameters | Type .
        private static slice<ref Field> funcResult(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("funcResult")());
            }
            if (p.tok == _Lparen)
            {
                return p.paramList();
            }
            var pos = p.pos();
            {
                var typ = p.typeOrNil();

                if (typ != null)
                {
                    ptr<Field> f = @new<Field>();
                    f.pos = pos;
                    f.Type = typ;
                    return new slice<ref Field>(new ref Field[] { f });
                }

            }

            return null;
        });

        private static void addField(this ref parser _p, ref StructType _styp, src.Pos pos, ref Name _name, Expr typ, ref BasicLit _tag) => func(_p, _styp, _name, _tag, (ref parser p, ref StructType styp, ref Name name, ref BasicLit tag, Defer _, Panic panic, Recover __) =>
        {
            if (tag != null)
            {
                for (var i = len(styp.FieldList) - len(styp.TagList); i > 0L; i--)
                {
                    styp.TagList = append(styp.TagList, null);
                }

                styp.TagList = append(styp.TagList, tag);
            }
            ptr<Field> f = @new<Field>();
            f.pos = pos;
            f.Name = name;
            f.Type = typ;
            styp.FieldList = append(styp.FieldList, f);

            if (debug && tag != null && len(styp.FieldList) != len(styp.TagList))
            {
                panic("inconsistent struct field list");
            }
        });

        // FieldDecl      = (IdentifierList Type | AnonymousField) [ Tag ] .
        // AnonymousField = [ "*" ] TypeName .
        // Tag            = string_lit .
        private static void fieldDecl(this ref parser _p, ref StructType _styp) => func(_p, _styp, (ref parser p, ref StructType styp, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("fieldDecl")());
            }
            var pos = p.pos();

            if (p.tok == _Name) 
                var name = p.name();
                if (p.tok == _Dot || p.tok == _Literal || p.tok == _Semi || p.tok == _Rbrace)
                { 
                    // embed oliteral
                    var typ = p.qualifiedName(name);
                    var tag = p.oliteral();
                    p.addField(styp, pos, null, typ, tag);
                    return;
                } 

                // new_name_list ntype oliteral
                var names = p.nameList(name);
                typ = p.type_();
                tag = p.oliteral();

                {
                    var name__prev1 = name;

                    foreach (var (_, __name) in names)
                    {
                        name = __name;
                        p.addField(styp, name.Pos(), name, typ, tag);
                    }

                    name = name__prev1;
                }
            else if (p.tok == _Lparen) 
                p.next();
                if (p.tok == _Star)
                { 
                    // '(' '*' embed ')' oliteral
                    pos = p.pos();
                    p.next();
                    typ = newIndirect(pos, p.qualifiedName(null));
                    p.want(_Rparen);
                    tag = p.oliteral();
                    p.addField(styp, pos, null, typ, tag);
                    p.syntax_error("cannot parenthesize embedded type");

                }
                else
                { 
                    // '(' embed ')' oliteral
                    typ = p.qualifiedName(null);
                    p.want(_Rparen);
                    tag = p.oliteral();
                    p.addField(styp, pos, null, typ, tag);
                    p.syntax_error("cannot parenthesize embedded type");
                }
            else if (p.tok == _Star) 
                p.next();
                if (p.got(_Lparen))
                { 
                    // '*' '(' embed ')' oliteral
                    typ = newIndirect(pos, p.qualifiedName(null));
                    p.want(_Rparen);
                    tag = p.oliteral();
                    p.addField(styp, pos, null, typ, tag);
                    p.syntax_error("cannot parenthesize embedded type");

                }
                else
                { 
                    // '*' embed oliteral
                    typ = newIndirect(pos, p.qualifiedName(null));
                    tag = p.oliteral();
                    p.addField(styp, pos, null, typ, tag);
                }
            else 
                p.syntax_error("expecting field name or embedded type");
                p.advance(_Semi, _Rbrace);
                    });

        private static ref BasicLit oliteral(this ref parser p)
        {
            if (p.tok == _Literal)
            {
                ptr<BasicLit> b = @new<BasicLit>();
                b.pos = p.pos();
                b.Value = p.lit;
                b.Kind = p.kind;
                p.next();
                return b;
            }
            return null;
        }

        // MethodSpec        = MethodName Signature | InterfaceTypeName .
        // MethodName        = identifier .
        // InterfaceTypeName = TypeName .
        private static ref Field methodDecl(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("methodDecl")());
            }

            if (p.tok == _Name) 
                var name = p.name(); 

                // accept potential name list but complain
                var hasNameList = false;
                while (p.got(_Comma))
                {
                    p.name();
                    hasNameList = true;
                }

                if (hasNameList)
                {
                    p.syntax_error("name list not allowed in interface type"); 
                    // already progressed, no need to advance
                }
                ptr<Field> f = @new<Field>();
                f.pos = name.Pos();
                if (p.tok != _Lparen)
                { 
                    // packname
                    f.Type = p.qualifiedName(name);
                    return f;
                }
                f.Name = name;
                f.Type = p.funcType();
                return f;
            else if (p.tok == _Lparen) 
                p.syntax_error("cannot parenthesize embedded type");
                f = @new<Field>();
                f.pos = p.pos();
                p.next();
                f.Type = p.qualifiedName(null);
                p.want(_Rparen);
                return f;
            else 
                p.syntax_error("expecting method or interface name");
                p.advance(_Semi, _Rbrace);
                return null;
                    });

        // ParameterDecl = [ IdentifierList ] [ "..." ] Type .
        private static ref Field paramDeclOrNil(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("paramDecl")());
            }
            ptr<Field> f = @new<Field>();
            f.pos = p.pos();


            if (p.tok == _Name) 
                f.Name = p.name();

                if (p.tok == _Name || p.tok == _Star || p.tok == _Arrow || p.tok == _Func || p.tok == _Lbrack || p.tok == _Chan || p.tok == _Map || p.tok == _Struct || p.tok == _Interface || p.tok == _Lparen) 
                    // sym name_or_type
                    f.Type = p.type_();
                else if (p.tok == _DotDotDot) 
                    // sym dotdotdot
                    f.Type = p.dotsType();
                else if (p.tok == _Dot) 
                    // name_or_type
                    // from dotname
                    f.Type = p.dotname(f.Name);
                    f.Name = null;
                            else if (p.tok == _Arrow || p.tok == _Star || p.tok == _Func || p.tok == _Lbrack || p.tok == _Chan || p.tok == _Map || p.tok == _Struct || p.tok == _Interface || p.tok == _Lparen) 
                // name_or_type
                f.Type = p.type_();
            else if (p.tok == _DotDotDot) 
                // dotdotdot
                f.Type = p.dotsType();
            else 
                p.syntax_error("expecting )");
                p.advance(_Comma, _Rparen);
                return null;
                        return f;
        });

        // ...Type
        private static ref DotsType dotsType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("dotsType")());
            }
            ptr<DotsType> t = @new<DotsType>();
            t.pos = p.pos();

            p.want(_DotDotDot);
            t.Elem = p.typeOrNil();
            if (t.Elem == null)
            {
                t.Elem = p.bad();
                p.syntax_error("final argument in variadic function missing type");
            }
            return t;
        });

        // Parameters    = "(" [ ParameterList [ "," ] ] ")" .
        // ParameterList = ParameterDecl { "," ParameterDecl } .
        private static slice<ref Field> paramList(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic panic, Recover _) =>
        {
            if (trace)
            {
                defer(p.trace("paramList")());
            }
            var pos = p.pos();

            long named = default; // number of parameters that have an explicit name and type
            p.list(_Lparen, _Comma, _Rparen, () =>
            {
                {
                    var par__prev1 = par;

                    var par = p.paramDeclOrNil();

                    if (par != null)
                    {
                        if (debug && par.Name == null && par.Type == null)
                        {
                            panic("parameter without name or type");
                        }
                        if (par.Name != null && par.Type != null)
                        {
                            named++;
                        }
                        list = append(list, par);
                    }

                    par = par__prev1;

                }
                return false;
            }); 

            // distribute parameter types
            if (named == 0L)
            { 
                // all unnamed => found names are named types
                {
                    var par__prev1 = par;

                    foreach (var (_, __par) in list)
                    {
                        par = __par;
                        {
                            var typ__prev2 = typ;

                            var typ = par.Name;

                            if (typ != null)
                            {
                                par.Type = typ;
                                par.Name = null;
                            }

                            typ = typ__prev2;

                        }
                    }

                    par = par__prev1;
                }

            }
            else if (named != len(list))
            { 
                // some named => all must be named
                var ok = true;
                typ = default;
                for (var i = len(list) - 1L; i >= 0L; i--)
                {
                    {
                        var par__prev3 = par;

                        par = list[i];

                        if (par.Type != null)
                        {
                            typ = par.Type;
                            if (par.Name == null)
                            {
                                ok = false;
                                var n = p.newName("_");
                                n.pos = typ.Pos(); // correct position
                                par.Name = n;
                            }
                        }
                        else if (typ != null)
                        {
                            par.Type = typ;
                        }
                        else
                        { 
                            // par.Type == nil && typ == nil => we only have a par.Name
                            ok = false;
                            var t = p.bad();
                            t.pos = par.Name.Pos(); // correct position
                            par.Type = t;
                        }

                        par = par__prev3;

                    }
                }

                if (!ok)
                {
                    p.syntax_error_at(pos, "mixed named and unnamed function parameters");
                }
            }
            return;
        });

        private static ref BadExpr bad(this ref parser p)
        {
            ptr<BadExpr> b = @new<BadExpr>();
            b.pos = p.pos();
            return b;
        }

        // ----------------------------------------------------------------------------
        // Statements

        // We represent x++, x-- as assignments x += ImplicitOne, x -= ImplicitOne.
        // ImplicitOne should not be used elsewhere.
        public static BasicLit ImplicitOne = ref new BasicLit(Value:"1");

        // SimpleStmt = EmptyStmt | ExpressionStmt | SendStmt | IncDecStmt | Assignment | ShortVarDecl .
        private static SimpleStmt simpleStmt(this ref parser _p, Expr lhs, bool rangeOk) => func(_p, (ref parser p, Defer defer, Panic panic, Recover _) =>
        {
            if (trace)
            {
                defer(p.trace("simpleStmt")());
            }
            if (rangeOk && p.tok == _Range)
            { 
                // _Range expr
                if (debug && lhs != null)
                {
                    panic("invalid call of simpleStmt");
                }
                return p.newRangeClause(null, false);
            }
            if (lhs == null)
            {
                lhs = p.exprList();
            }
            {
                ref ListExpr (_, ok) = lhs._<ref ListExpr>();

                if (!ok && p.tok != _Assign && p.tok != _Define)
                { 
                    // expr
                    var pos = p.pos();

                    if (p.tok == _AssignOp) 
                        // lhs op= rhs
                        var op = p.op;
                        p.next();
                        return p.newAssignStmt(pos, op, lhs, p.expr());
                    else if (p.tok == _IncOp) 
                        // lhs++ or lhs--
                        op = p.op;
                        p.next();
                        return p.newAssignStmt(pos, op, lhs, ImplicitOne);
                    else if (p.tok == _Arrow) 
                        // lhs <- rhs
                        ptr<object> s = @new<SendStmt>();
                        s.pos = pos;
                        p.next();
                        s.Chan = lhs;
                        s.Value = p.expr();
                        return s;
                    else 
                        // expr
                        s = @new<ExprStmt>();
                        s.pos = lhs.Pos();
                        s.X = lhs;
                        return s;
                                    } 

                // expr_list

            } 

            // expr_list
            pos = p.pos();

            if (p.tok == _Assign) 
                p.next();

                if (rangeOk && p.tok == _Range)
                { 
                    // expr_list '=' _Range expr
                    return p.newRangeClause(lhs, false);
                } 

                // expr_list '=' expr_list
                return p.newAssignStmt(pos, 0L, lhs, p.exprList());
            else if (p.tok == _Define) 
                p.next();

                if (rangeOk && p.tok == _Range)
                { 
                    // expr_list ':=' range expr
                    return p.newRangeClause(lhs, true);
                } 

                // expr_list ':=' expr_list
                var rhs = p.exprList();

                {
                    ref TypeSwitchGuard x__prev1 = x;

                    ref TypeSwitchGuard (x, ok) = rhs._<ref TypeSwitchGuard>();

                    if (ok)
                    {
                        switch (lhs.type())
                        {
                            case ref Name lhs:
                                x.Lhs = lhs;
                                break;
                            case ref ListExpr lhs:
                                p.error_at(lhs.Pos(), fmt.Sprintf("cannot assign 1 value to %d variables", len(lhs.ElemList))); 
                                // make the best of what we have
                                {
                                    var lhs__prev2 = lhs;

                                    ref Name (lhs, ok) = lhs.ElemList[0L]._<ref Name>();

                                    if (ok)
                                    {
                                        x.Lhs = lhs;
                                    }

                                    lhs = lhs__prev2;

                                }
                                break;
                            default:
                            {
                                var lhs = lhs.type();
                                p.error_at(lhs.Pos(), fmt.Sprintf("invalid variable name %s in type switch", String(lhs)));
                                break;
                            }
                        }
                        s = @new<ExprStmt>();
                        s.pos = x.Pos();
                        s.X = x;
                        return s;
                    }

                    x = x__prev1;

                }

                var @as = p.newAssignStmt(pos, Def, lhs, rhs);
                return as;
            else 
                p.syntax_error("expecting := or = or comma");
                p.advance(_Semi, _Rbrace); 
                // make the best of what we have
                {
                    ref TypeSwitchGuard x__prev1 = x;

                    (x, ok) = lhs._<ref ListExpr>();

                    if (ok)
                    {
                        lhs = x.ElemList[0L];
                    }

                    x = x__prev1;

                }
                s = @new<ExprStmt>();
                s.pos = lhs.Pos();
                s.X = lhs;
                return s;
                    });

        private static ref RangeClause newRangeClause(this ref parser p, Expr lhs, bool def)
        {
            ptr<RangeClause> r = @new<RangeClause>();
            r.pos = p.pos();
            p.next(); // consume _Range
            r.Lhs = lhs;
            r.Def = def;
            r.X = p.expr();
            return r;
        }

        private static ref AssignStmt newAssignStmt(this ref parser p, src.Pos pos, Operator op, Expr lhs, Expr rhs)
        {
            ptr<AssignStmt> a = @new<AssignStmt>();
            a.pos = pos;
            a.Op = op;
            a.Lhs = lhs;
            a.Rhs = rhs;
            return a;
        }

        private static Stmt labeledStmtOrNil(this ref parser _p, ref Name _label) => func(_p, _label, (ref parser p, ref Name label, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("labeledStmt")());
            }
            ptr<object> s = @new<LabeledStmt>();
            s.pos = p.pos();
            s.Label = label;

            p.want(_Colon);

            if (p.tok == _Rbrace)
            { 
                // We expect a statement (incl. an empty statement), which must be
                // terminated by a semicolon. Because semicolons may be omitted before
                // an _Rbrace, seeing an _Rbrace implies an empty statement.
                ptr<object> e = @new<EmptyStmt>();
                e.pos = p.pos();
                s.Stmt = e;
                return s;
            }
            s.Stmt = p.stmtOrNil();
            if (s.Stmt != null)
            {
                return s;
            } 

            // report error at line of ':' token
            p.syntax_error_at(s.pos, "missing statement after label"); 
            // we are already at the end of the labeled statement - no need to advance
            return null; // avoids follow-on errors (see e.g., fixedbugs/bug274.go)
        });

        // context must be a non-empty string unless we know that p.tok == _Lbrace.
        private static ref BlockStmt blockStmt(this ref parser _p, @string context) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("blockStmt")());
            }
            ptr<BlockStmt> s = @new<BlockStmt>();
            s.pos = p.pos(); 

            // people coming from C may forget that braces are mandatory in Go
            if (!p.got(_Lbrace))
            {
                p.syntax_error("expecting { after " + context);
                p.advance(_Name, _Rbrace);
                s.Rbrace = p.pos(); // in case we found "}"
                if (p.got(_Rbrace))
                {
                    return s;
                }
            }
            s.List = p.stmtList();
            s.Rbrace = p.pos();
            p.want(_Rbrace);

            return s;
        });

        private static ref DeclStmt declStmt(this ref parser _p, Func<ref Group, Decl> f) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("declStmt")());
            }
            ptr<DeclStmt> s = @new<DeclStmt>();
            s.pos = p.pos();

            p.next(); // _Const, _Type, or _Var
            s.DeclList = p.appendGroup(null, f);

            return s;
        });

        private static Stmt forStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("forStmt")());
            }
            ptr<object> s = @new<ForStmt>();
            s.pos = p.pos();

            s.Init, s.Cond, s.Post = p.header(_For);
            s.Body = p.blockStmt("for clause");

            return s;
        });

        private static (SimpleStmt, Expr, SimpleStmt) header(this ref parser p, token keyword)
        {
            p.want(keyword);

            if (p.tok == _Lbrace)
            {
                if (keyword == _If)
                {
                    p.syntax_error("missing condition in if statement");
                }
                return;
            } 
            // p.tok != _Lbrace
            var outer = p.xnest;
            p.xnest = -1L;

            if (p.tok != _Semi)
            { 
                // accept potential varDecl but complain
                if (p.got(_Var))
                {
                    p.syntax_error(fmt.Sprintf("var declaration not allowed in %s initializer", keyword.String()));
                }
                init = p.simpleStmt(null, keyword == _For); 
                // If we have a range clause, we are done (can only happen for keyword == _For).
                {
                    ref RangeClause (_, ok) = init._<ref RangeClause>();

                    if (ok)
                    {
                        p.xnest = outer;
                        return;
                    }

                }
            }
            SimpleStmt condStmt = default;
            var semi = default;
            if (p.tok != _Lbrace)
            {
                if (p.tok == _Semi)
                {
                    semi.pos = p.pos();
                    semi.lit = p.lit;
                    p.next();
                }
                else
                {
                    p.want(_Semi);
                }
                if (keyword == _For)
                {
                    if (p.tok != _Semi)
                    {
                        if (p.tok == _Lbrace)
                        {
                            p.syntax_error("expecting for loop condition");
                            goto done;
                        }
                        condStmt = p.simpleStmt(null, false);
                    }
                    p.want(_Semi);
                    if (p.tok != _Lbrace)
                    {
                        post = p.simpleStmt(null, false);
                        {
                            ref AssignStmt (a, _) = post._<ref AssignStmt>();

                            if (a != null && a.Op == Def)
                            {
                                p.syntax_error_at(a.Pos(), "cannot declare in post statement of for loop");
                            }

                        }
                    }
                }
                else if (p.tok != _Lbrace)
                {
                    condStmt = p.simpleStmt(null, false);
                }
            }
            else
            {
                condStmt = init;
                init = null;
            }
done:

            switch (condStmt.type())
            {
                case 
                    if (keyword == _If && semi.pos.IsKnown())
                    {
                        if (semi.lit != "semicolon")
                        {
                            p.syntax_error_at(semi.pos, fmt.Sprintf("unexpected %s, expecting { after if clause", semi.lit));
                        }
                        else
                        {
                            p.syntax_error_at(semi.pos, "missing condition in if statement");
                        }
                    }
                    break;
                case ref ExprStmt s:
                    cond = s.X;
                    break;
                default:
                {
                    var s = condStmt.type();
                    p.syntax_error(fmt.Sprintf("%s used as value", String(s)));
                    break;
                }

            }
            p.xnest = outer;
            return;
        }

        private static ref IfStmt ifStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("ifStmt")());
            }
            ptr<IfStmt> s = @new<IfStmt>();
            s.pos = p.pos();

            s.Init, s.Cond, _ = p.header(_If);
            s.Then = p.blockStmt("if clause");

            if (p.got(_Else))
            {

                if (p.tok == _If) 
                    s.Else = p.ifStmt();
                else if (p.tok == _Lbrace) 
                    s.Else = p.blockStmt("");
                else 
                    p.syntax_error("else must be followed by if or statement block");
                    p.advance(_Name, _Rbrace);
                            }
            return s;
        });

        private static ref SwitchStmt switchStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("switchStmt")());
            }
            ptr<SwitchStmt> s = @new<SwitchStmt>();
            s.pos = p.pos();

            s.Init, s.Tag, _ = p.header(_Switch);

            if (!p.got(_Lbrace))
            {
                p.syntax_error("missing { after switch clause");
                p.advance(_Case, _Default, _Rbrace);
            }
            while (p.tok != _EOF && p.tok != _Rbrace)
            {
                s.Body = append(s.Body, p.caseClause());
            }

            s.Rbrace = p.pos();
            p.want(_Rbrace);

            return s;
        });

        private static ref SelectStmt selectStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("selectStmt")());
            }
            ptr<SelectStmt> s = @new<SelectStmt>();
            s.pos = p.pos();

            p.want(_Select);
            if (!p.got(_Lbrace))
            {
                p.syntax_error("missing { after select clause");
                p.advance(_Case, _Default, _Rbrace);
            }
            while (p.tok != _EOF && p.tok != _Rbrace)
            {
                s.Body = append(s.Body, p.commClause());
            }

            s.Rbrace = p.pos();
            p.want(_Rbrace);

            return s;
        });

        private static ref CaseClause caseClause(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("caseClause")());
            }
            ptr<CaseClause> c = @new<CaseClause>();
            c.pos = p.pos();


            if (p.tok == _Case) 
                p.next();
                c.Cases = p.exprList();
            else if (p.tok == _Default) 
                p.next();
            else 
                p.syntax_error("expecting case or default or }");
                p.advance(_Colon, _Case, _Default, _Rbrace);
                        c.Colon = p.pos();
            p.want(_Colon);
            c.Body = p.stmtList();

            return c;
        });

        private static ref CommClause commClause(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("commClause")());
            }
            ptr<CommClause> c = @new<CommClause>();
            c.pos = p.pos();


            if (p.tok == _Case) 
                p.next();
                c.Comm = p.simpleStmt(null, false); 

                // The syntax restricts the possible simple statements here to:
                //
                //     lhs <- x (send statement)
                //     <-x
                //     lhs = <-x
                //     lhs := <-x
                //
                // All these (and more) are recognized by simpleStmt and invalid
                // syntax trees are flagged later, during type checking.
                // TODO(gri) eventually may want to restrict valid syntax trees
                // here.
            else if (p.tok == _Default) 
                p.next();
            else 
                p.syntax_error("expecting case or default or }");
                p.advance(_Colon, _Case, _Default, _Rbrace);
                        c.Colon = p.pos();
            p.want(_Colon);
            c.Body = p.stmtList();

            return c;
        });

        // Statement =
        //     Declaration | LabeledStmt | SimpleStmt |
        //     GoStmt | ReturnStmt | BreakStmt | ContinueStmt | GotoStmt |
        //     FallthroughStmt | Block | IfStmt | SwitchStmt | SelectStmt | ForStmt |
        //     DeferStmt .
        private static Stmt stmtOrNil(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("stmt " + p.tok.String())());
            } 

            // Most statements (assignments) start with an identifier;
            // look for it first before doing anything more expensive.
            if (p.tok == _Name)
            {
                var lhs = p.exprList();
                {
                    ref Name (label, ok) = lhs._<ref Name>();

                    if (ok && p.tok == _Colon)
                    {
                        return p.labeledStmtOrNil(label);
                    }

                }
                return p.simpleStmt(lhs, false);
            }

            if (p.tok == _Lbrace) 
                return p.blockStmt("");
            else if (p.tok == _Var) 
                return p.declStmt(p.varDecl);
            else if (p.tok == _Const) 
                return p.declStmt(p.constDecl);
            else if (p.tok == _Type) 
                return p.declStmt(p.typeDecl);
            else if (p.tok == _Operator || p.tok == _Star) 

                if (p.op == Add || p.op == Sub || p.op == Mul || p.op == And || p.op == Xor || p.op == Not) 
                    return p.simpleStmt(null, false); // unary operators
                            else if (p.tok == _Literal || p.tok == _Func || p.tok == _Lparen || p.tok == _Lbrack || p.tok == _Struct || p.tok == _Map || p.tok == _Chan || p.tok == _Interface || p.tok == _Arrow) // receive operator
                return p.simpleStmt(null, false);
            else if (p.tok == _For) 
                return p.forStmt();
            else if (p.tok == _Switch) 
                return p.switchStmt();
            else if (p.tok == _Select) 
                return p.selectStmt();
            else if (p.tok == _If) 
                return p.ifStmt();
            else if (p.tok == _Fallthrough) 
                ptr<object> s = @new<BranchStmt>();
                s.pos = p.pos();
                p.next();
                s.Tok = _Fallthrough;
                return s;
            else if (p.tok == _Break || p.tok == _Continue) 
                s = @new<BranchStmt>();
                s.pos = p.pos();
                s.Tok = p.tok;
                p.next();
                if (p.tok == _Name)
                {
                    s.Label = p.name();
                }
                return s;
            else if (p.tok == _Go || p.tok == _Defer) 
                return p.callStmt();
            else if (p.tok == _Goto) 
                s = @new<BranchStmt>();
                s.pos = p.pos();
                s.Tok = _Goto;
                p.next();
                s.Label = p.name();
                return s;
            else if (p.tok == _Return) 
                s = @new<ReturnStmt>();
                s.pos = p.pos();
                p.next();
                if (p.tok != _Semi && p.tok != _Rbrace)
                {
                    s.Results = p.exprList();
                }
                return s;
            else if (p.tok == _Semi) 
                s = @new<EmptyStmt>();
                s.pos = p.pos();
                return s;
                        return null;
        });

        // StatementList = { Statement ";" } .
        private static slice<Stmt> stmtList(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("stmtList")());
            }
            while (p.tok != _EOF && p.tok != _Rbrace && p.tok != _Case && p.tok != _Default)
            {
                var s = p.stmtOrNil();
                if (s == null)
                {
                    break;
                }
                l = append(l, s); 
                // ";" is optional before "}"
                if (!p.got(_Semi) && p.tok != _Rbrace)
                {
                    p.syntax_error("at end of statement");
                    p.advance(_Semi, _Rbrace, _Case, _Default);
                    p.got(_Semi); // avoid spurious empty statement
                }
            }

            return;
        });

        // Arguments = "(" [ ( ExpressionList | Type [ "," ExpressionList ] ) [ "..." ] [ "," ] ] ")" .
        private static (slice<Expr>, bool) argList(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("argList")());
            }
            p.xnest++;
            p.list(_Lparen, _Comma, _Rparen, () =>
            {
                list = append(list, p.expr());
                hasDots = p.got(_DotDotDot);
                return hasDots;
            });
            p.xnest--;

            return;
        });

        // ----------------------------------------------------------------------------
        // Common productions

        private static ref Name newName(this ref parser p, @string value)
        {
            ptr<Name> n = @new<Name>();
            n.pos = p.pos();
            n.Value = value;
            return n;
        }

        private static ref Name name(this ref parser p)
        { 
            // no tracing to avoid overly verbose output

            if (p.tok == _Name)
            {
                var n = p.newName(p.lit);
                p.next();
                return n;
            }
            n = p.newName("_");
            p.syntax_error("expecting name");
            p.advance();
            return n;
        }

        // IdentifierList = identifier { "," identifier } .
        // The first name must be provided.
        private static slice<ref Name> nameList(this ref parser _p, ref Name _first) => func(_p, _first, (ref parser p, ref Name first, Defer defer, Panic panic, Recover _) =>
        {
            if (trace)
            {
                defer(p.trace("nameList")());
            }
            if (debug && first == null)
            {
                panic("first name not provided");
            }
            ref Name l = new slice<ref Name>(new ref Name[] { first });
            while (p.got(_Comma))
            {
                l = append(l, p.name());
            }


            return l;
        });

        // The first name may be provided, or nil.
        private static Expr qualifiedName(this ref parser _p, ref Name _name) => func(_p, _name, (ref parser p, ref Name name, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("qualifiedName")());
            }

            if (name != null)             else if (p.tok == _Name) 
                name = p.name();
            else 
                name = p.newName("_");
                p.syntax_error("expecting name");
                p.advance(_Dot, _Semi, _Rbrace);
                        return p.dotname(name);
        });

        // ExpressionList = Expression { "," Expression } .
        private static Expr exprList(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                defer(p.trace("exprList")());
            }
            var x = p.expr();
            if (p.got(_Comma))
            {
                Expr list = new slice<Expr>(new Expr[] { x, p.expr() });
                while (p.got(_Comma))
                {
                    list = append(list, p.expr());
                }

                ptr<ListExpr> t = @new<ListExpr>();
                t.pos = x.Pos();
                t.ElemList = list;
                x = t;
            }
            return x;
        });

        // unparen removes all parentheses around an expression.
        private static Expr unparen(Expr x)
        {
            while (true)
            {
                ref ParenExpr (p, ok) = x._<ref ParenExpr>();
                if (!ok)
                {
                    break;
                }
                x = p.X;
            }

            return x;
        }
    }
}}}}
