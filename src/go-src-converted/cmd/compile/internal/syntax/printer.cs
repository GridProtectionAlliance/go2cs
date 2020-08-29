// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements printing of syntax trees in source format.

// package syntax -- go2cs converted at 2020 August 29 09:26:23 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Go\src\cmd\compile\internal\syntax\printer.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
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
        // TODO(gri) Consider removing the linebreaks flag from this signature.
        // Its likely rarely used in common cases.
        public static (long, error) Fprint(io.Writer w, Node x, bool linebreaks) => func((defer, _, __) =>
        {
            printer p = new printer(output:w,linebreaks:linebreaks,);

            defer(() =>
            {
                n = p.written;
                {
                    var e = recover();

                    if (e != null)
                    {
                        err = e._<localError>().err; // re-panics if it's not a localError
                    }
                }
            }());

            p.print(x);
            p.flush(_EOF);

            return;
        });

        public static @string String(Node n) => func((_, panic, __) =>
        {
            bytes.Buffer buf = default;
            var (_, err) = Fprint(ref buf, n, false);
            if (err != null)
            {
                panic(err); // TODO(gri) print something sensible into buf instead
            }
            return buf.String();
        });

        private partial struct ctrlSymbol // : long
        {
        }

        private static readonly ctrlSymbol none = iota;
        private static readonly var semi = 0;
        private static readonly var blank = 1;
        private static readonly var newline = 2;
        private static readonly var indent = 3;
        private static readonly var outdent = 4; 
        // comment
        // eolComment

        private partial struct whitespace
        {
            public token last;
            public ctrlSymbol kind; //text string // comment text (possibly ""); valid if kind == comment
        }

        private partial struct printer
        {
            public io.Writer output;
            public long written; // number of bytes written
            public bool linebreaks; // print linebreaks instead of semis

            public long indent; // current indentation level
            public long nlcount; // number of consecutive newlines

            public slice<whitespace> pending; // pending whitespace
            public token lastTok; // last token (after any pending semi) processed by print
        }

        // write is a thin wrapper around p.output.Write
        // that takes care of accounting and error handling.
        private static void write(this ref printer _p, slice<byte> data) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            var (n, err) = p.output.Write(data);
            p.written += n;
            if (err != null)
            {
                panic(new localError(err));
            }
        });

        private static slice<byte> tabBytes = (slice<byte>)"\t\t\t\t\t\t\t\t";        private static slice<byte> newlineByte = (slice<byte>)"\n";        private static slice<byte> blankByte = (slice<byte>)" ";

        private static void writeBytes(this ref printer _p, slice<byte> data) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            if (len(data) == 0L)
            {
                panic("expected non-empty []byte");
            }
            if (p.nlcount > 0L && p.indent > 0L)
            { 
                // write indentation
                var n = p.indent;
                while (n > len(tabBytes))
                {
                    p.write(tabBytes);
                    n -= len(tabBytes);
                }

                p.write(tabBytes[..n]);
            }
            p.write(data);
            p.nlcount = 0L;
        });

        private static void writeString(this ref printer p, @string s)
        {
            p.writeBytes((slice<byte>)s);
        }

        // If impliesSemi returns true for a non-blank line's final token tok,
        // a semicolon is automatically inserted. Vice versa, a semicolon may
        // be omitted in those cases.
        private static bool impliesSemi(token tok)
        {

            if (tok == _Name || tok == _Break || tok == _Continue || tok == _Fallthrough || tok == _Return || tok == _Rparen || tok == _Rbrack || tok == _Rbrace) // TODO(gri) fix this
                return true;
                        return false;
        }

        // TODO(gri) provide table of []byte values for all tokens to avoid repeated string conversion

        private static bool lineComment(@string text)
        {
            return strings.HasPrefix(text, "//");
        }

        private static void addWhitespace(this ref printer p, ctrlSymbol kind, @string text)
        {
            p.pending = append(p.pending, new whitespace(p.lastTok,kind));

            if (kind == semi) 
                p.lastTok = _Semi;
            else if (kind == newline) 
                p.lastTok = 0L; 
                // TODO(gri) do we need to handle /*-style comments containing newlines here?
                    }

        private static void flush(this ref printer _p, token next) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        { 
            // eliminate semis and redundant whitespace
            var sawNewline = next == _EOF;
            var sawParen = next == _Rparen || next == _Rbrace;
            {
                var i__prev1 = i;

                for (var i = len(p.pending) - 1L; i >= 0L; i--)
                {

                    if (p.pending[i].kind == semi) 
                        var k = semi;
                        if (sawParen)
                        {
                            sawParen = false;
                            k = none; // eliminate semi
                        }
                        else if (sawNewline && impliesSemi(p.pending[i].last))
                        {
                            sawNewline = false;
                            k = none; // eliminate semi
                        }
                        p.pending[i].kind = k;
                    else if (p.pending[i].kind == newline) 
                        sawNewline = true;
                    else if (p.pending[i].kind == blank || p.pending[i].kind == indent || p.pending[i].kind == outdent)                     else 
                        panic("unreachable");
                                    } 

                // print pending


                i = i__prev1;
            } 

            // print pending
            var prev = none;
            {
                var i__prev1 = i;

                foreach (var (__i) in p.pending)
                {
                    i = __i;

                    if (p.pending[i].kind == none)                     else if (p.pending[i].kind == semi) 
                        p.writeString(";");
                        p.nlcount = 0L;
                        prev = semi;
                    else if (p.pending[i].kind == blank) 
                        if (prev != blank)
                        { 
                            // at most one blank
                            p.writeBytes(blankByte);
                            p.nlcount = 0L;
                            prev = blank;
                        }
                    else if (p.pending[i].kind == newline) 
                        const long maxEmptyLines = 1L;

                        if (p.nlcount <= maxEmptyLines)
                        {
                            p.write(newlineByte);
                            p.nlcount++;
                            prev = newline;
                        }
                    else if (p.pending[i].kind == indent) 
                        p.indent++;
                    else if (p.pending[i].kind == outdent) 
                        p.indent--;
                        if (p.indent < 0L)
                        {
                            panic("negative indentation");
                        } 
                        // case comment:
                        //     if text := p.pending[i].text; text != "" {
                        //         p.writeString(text)
                        //         p.nlcount = 0
                        //         prev = comment
                        //     }
                        //     // TODO(gri) should check that line comments are always followed by newline
                    else 
                        panic("unreachable");
                                    }

                i = i__prev1;
            }

            p.pending = p.pending[..0L]; // re-use underlying array
        });

        private static bool mayCombine(token prev, byte next)
        {
            return; // for now
            // switch prev {
            // case lexical.Int:
            //     b = next == '.' // 1.
            // case lexical.Add:
            //     b = next == '+' // ++
            // case lexical.Sub:
            //     b = next == '-' // --
            // case lexical.Quo:
            //     b = next == '*' // /*
            // case lexical.Lss:
            //     b = next == '-' || next == '<' // <- or <<
            // case lexical.And:
            //     b = next == '&' || next == '^' // && or &^
            // }
            // return
        }

        private static void print(this ref printer _p, params object[] args) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            for (long i = 0L; i < len(args); i++)
            {
                switch (args[i].type())
                {
                    case 
                        break;
                    case Node x:
                        p.printNode(x);
                        break;
                    case token x:
                        @string s = default;
                        if (x == _Name)
                        {
                            i++;
                            if (i >= len(args))
                            {
                                panic("missing string argument after _Name");
                            }
                            s = args[i]._<@string>();
                        }
                        else
                        {
                            s = x.String();
                        } 

                        // TODO(gri) This check seems at the wrong place since it doesn't
                        //           take into account pending white space.
                        if (mayCombine(p.lastTok, s[0L]))
                        {
                            panic("adjacent tokens combine without whitespace");
                        }
                        if (x == _Semi)
                        { 
                            // delay printing of semi
                            p.addWhitespace(semi, "");
                        }
                        else
                        {
                            p.flush(x);
                            p.writeString(s);
                            p.nlcount = 0L;
                            p.lastTok = x;
                        }
                        break;
                    case Operator x:
                        if (x != 0L)
                        {
                            p.flush(_Operator);
                            p.writeString(x.String());
                        }
                        break;
                    case ctrlSymbol x:

                        if (x == none || x == semi) 
                            panic("unreachable");
                        else if (x == newline) 
                            // TODO(gri) need to handle mandatory newlines after a //-style comment
                            if (!p.linebreaks)
                            {
                                x = blank;
                            }
                                                p.addWhitespace(x, ""); 

                        // case *Comment: // comments are not Nodes
                        //     p.addWhitespace(comment, x.Text)
                        break;
                    default:
                    {
                        var x = args[i].type();
                        panic(fmt.Sprintf("unexpected argument %v (%T)", x, x));
                        break;
                    }
                }
            }

        });

        private static void printNode(this ref printer p, Node n)
        { 
            // ncom := *n.Comments()
            // if ncom != nil {
            //     // TODO(gri) in general we cannot make assumptions about whether
            //     // a comment is a /*- or a //-style comment since the syntax
            //     // tree may have been manipulated. Need to make sure the correct
            //     // whitespace is emitted.
            //     for _, c := range ncom.Alone {
            //         p.print(c, newline)
            //     }
            //     for _, c := range ncom.Before {
            //         if c.Text == "" || lineComment(c.Text) {
            //             panic("unexpected empty line or //-style 'before' comment")
            //         }
            //         p.print(c, blank)
            //     }
            // }

            p.printRawNode(n); 

            // if ncom != nil && len(ncom.After) > 0 {
            //     for i, c := range ncom.After {
            //         if i+1 < len(ncom.After) {
            //             if c.Text == "" || lineComment(c.Text) {
            //                 panic("unexpected empty line or //-style non-final 'after' comment")
            //             }
            //         }
            //         p.print(blank, c)
            //     }
            //     //p.print(newline)
            // }
        }

        private static void printRawNode(this ref printer _p, Node n) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            switch (n.type())
            {
                case 
                    break;
                case ref BadExpr n:
                    p.print(_Name, "<bad expr>");
                    break;
                case ref Name n:
                    p.print(_Name, n.Value); // _Name requires actual value following immediately
                    break;
                case ref BasicLit n:
                    p.print(_Name, n.Value); // _Name requires actual value following immediately
                    break;
                case ref FuncLit n:
                    p.print(n.Type, blank, n.Body);
                    break;
                case ref CompositeLit n:
                    if (n.Type != null)
                    {
                        p.print(n.Type);
                    }
                    p.print(_Lbrace);
                    if (n.NKeys > 0L && n.NKeys == len(n.ElemList))
                    {
                        p.printExprLines(n.ElemList);
                    }
                    else
                    {
                        p.printExprList(n.ElemList);
                    }
                    p.print(_Rbrace);
                    break;
                case ref ParenExpr n:
                    p.print(_Lparen, n.X, _Rparen);
                    break;
                case ref SelectorExpr n:
                    p.print(n.X, _Dot, n.Sel);
                    break;
                case ref IndexExpr n:
                    p.print(n.X, _Lbrack, n.Index, _Rbrack);
                    break;
                case ref SliceExpr n:
                    p.print(n.X, _Lbrack);
                    {
                        var i = n.Index[0L];

                        if (i != null)
                        {
                            p.printNode(i);
                        }

                    }
                    p.print(_Colon);
                    {
                        var j = n.Index[1L];

                        if (j != null)
                        {
                            p.printNode(j);
                        }

                    }
                    {
                        var k = n.Index[2L];

                        if (k != null)
                        {
                            p.print(_Colon, k);
                        }

                    }
                    p.print(_Rbrack);
                    break;
                case ref AssertExpr n:
                    p.print(n.X, _Dot, _Lparen);
                    if (n.Type != null)
                    {
                        p.printNode(n.Type);
                    }
                    else
                    {
                        p.print(_Type);
                    }
                    p.print(_Rparen);
                    break;
                case ref CallExpr n:
                    p.print(n.Fun, _Lparen);
                    p.printExprList(n.ArgList);
                    if (n.HasDots)
                    {
                        p.print(_DotDotDot);
                    }
                    p.print(_Rparen);
                    break;
                case ref Operation n:
                    if (n.Y == null)
                    { 
                        // unary expr
                        p.print(n.Op); 
                        // if n.Op == lexical.Range {
                        //     p.print(blank)
                        // }
                        p.print(n.X);
                    }
                    else
                    { 
                        // binary expr
                        // TODO(gri) eventually take precedence into account
                        // to control possibly missing parentheses
                        p.print(n.X, blank, n.Op, blank, n.Y);
                    }
                    break;
                case ref KeyValueExpr n:
                    p.print(n.Key, _Colon, blank, n.Value);
                    break;
                case ref ListExpr n:
                    p.printExprList(n.ElemList);
                    break;
                case ref ArrayType n:
                    var len = _DotDotDot;
                    if (n.Len != null)
                    {
                        len = n.Len;
                    }
                    p.print(_Lbrack, len, _Rbrack, n.Elem);
                    break;
                case ref SliceType n:
                    p.print(_Lbrack, _Rbrack, n.Elem);
                    break;
                case ref DotsType n:
                    p.print(_DotDotDot, n.Elem);
                    break;
                case ref StructType n:
                    p.print(_Struct);
                    if (len(n.FieldList) > 0L && p.linebreaks)
                    {
                        p.print(blank);
                    }
                    p.print(_Lbrace);
                    if (len(n.FieldList) > 0L)
                    {
                        p.print(newline, indent);
                        p.printFieldList(n.FieldList, n.TagList);
                        p.print(outdent, newline);
                    }
                    p.print(_Rbrace);
                    break;
                case ref FuncType n:
                    p.print(_Func);
                    p.printSignature(n);
                    break;
                case ref InterfaceType n:
                    p.print(_Interface);
                    if (len(n.MethodList) > 0L && p.linebreaks)
                    {
                        p.print(blank);
                    }
                    p.print(_Lbrace);
                    if (len(n.MethodList) > 0L)
                    {
                        p.print(newline, indent);
                        p.printMethodList(n.MethodList);
                        p.print(outdent, newline);
                    }
                    p.print(_Rbrace);
                    break;
                case ref MapType n:
                    p.print(_Map, _Lbrack, n.Key, _Rbrack, n.Value);
                    break;
                case ref ChanType n:
                    if (n.Dir == RecvOnly)
                    {
                        p.print(_Arrow);
                    }
                    p.print(_Chan);
                    if (n.Dir == SendOnly)
                    {
                        p.print(_Arrow);
                    }
                    p.print(blank, n.Elem); 

                    // statements
                    break;
                case ref DeclStmt n:
                    p.printDecl(n.DeclList);
                    break;
                case ref EmptyStmt n:
                    break;
                case ref LabeledStmt n:
                    p.print(outdent, n.Label, _Colon, indent, newline, n.Stmt);
                    break;
                case ref ExprStmt n:
                    p.print(n.X);
                    break;
                case ref SendStmt n:
                    p.print(n.Chan, blank, _Arrow, blank, n.Value);
                    break;
                case ref AssignStmt n:
                    p.print(n.Lhs);
                    if (n.Rhs == ImplicitOne)
                    { 
                        // TODO(gri) This is going to break the mayCombine
                        //           check once we enable that again.
                        p.print(n.Op, n.Op); // ++ or --
                    }
                    else
                    {
                        p.print(blank, n.Op, _Assign, blank);
                        p.print(n.Rhs);
                    }
                    break;
                case ref CallStmt n:
                    p.print(n.Tok, blank, n.Call);
                    break;
                case ref ReturnStmt n:
                    p.print(_Return);
                    if (n.Results != null)
                    {
                        p.print(blank, n.Results);
                    }
                    break;
                case ref BranchStmt n:
                    p.print(n.Tok);
                    if (n.Label != null)
                    {
                        p.print(blank, n.Label);
                    }
                    break;
                case ref BlockStmt n:
                    p.print(_Lbrace);
                    if (len(n.List) > 0L)
                    {
                        p.print(newline, indent);
                        p.printStmtList(n.List, true);
                        p.print(outdent, newline);
                    }
                    p.print(_Rbrace);
                    break;
                case ref IfStmt n:
                    p.print(_If, blank);
                    if (n.Init != null)
                    {
                        p.print(n.Init, _Semi, blank);
                    }
                    p.print(n.Cond, blank, n.Then);
                    if (n.Else != null)
                    {
                        p.print(blank, _Else, blank, n.Else);
                    }
                    break;
                case ref SwitchStmt n:
                    p.print(_Switch, blank);
                    if (n.Init != null)
                    {
                        p.print(n.Init, _Semi, blank);
                    }
                    if (n.Tag != null)
                    {
                        p.print(n.Tag, blank);
                    }
                    p.printSwitchBody(n.Body);
                    break;
                case ref TypeSwitchGuard n:
                    if (n.Lhs != null)
                    {
                        p.print(n.Lhs, blank, _Define, blank);
                    }
                    p.print(n.X, _Dot, _Lparen, _Type, _Rparen);
                    break;
                case ref SelectStmt n:
                    p.print(_Select, blank); // for now
                    p.printSelectBody(n.Body);
                    break;
                case ref RangeClause n:
                    if (n.Lhs != null)
                    {
                        var tok = _Assign;
                        if (n.Def)
                        {
                            tok = _Define;
                        }
                        p.print(n.Lhs, blank, tok, blank);
                    }
                    p.print(_Range, blank, n.X);
                    break;
                case ref ForStmt n:
                    p.print(_For, blank);
                    if (n.Init == null && n.Post == null)
                    {
                        if (n.Cond != null)
                        {
                            p.print(n.Cond, blank);
                        }
                    }
                    else
                    {
                        if (n.Init != null)
                        {
                            p.print(n.Init); 
                            // TODO(gri) clean this up
                            {
                                ref RangeClause (_, ok) = n.Init._<ref RangeClause>();

                                if (ok)
                                {
                                    p.print(blank, n.Body);
                                    break;
                                }

                            }
                        }
                        p.print(_Semi, blank);
                        if (n.Cond != null)
                        {
                            p.print(n.Cond);
                        }
                        p.print(_Semi, blank);
                        if (n.Post != null)
                        {
                            p.print(n.Post, blank);
                        }
                    }
                    p.print(n.Body);
                    break;
                case ref ImportDecl n:
                    if (n.Group == null)
                    {
                        p.print(_Import, blank);
                    }
                    if (n.LocalPkgName != null)
                    {
                        p.print(n.LocalPkgName, blank);
                    }
                    p.print(n.Path);
                    break;
                case ref ConstDecl n:
                    if (n.Group == null)
                    {
                        p.print(_Const, blank);
                    }
                    p.printNameList(n.NameList);
                    if (n.Type != null)
                    {
                        p.print(blank, n.Type);
                    }
                    if (n.Values != null)
                    {
                        p.print(blank, _Assign, blank, n.Values);
                    }
                    break;
                case ref TypeDecl n:
                    if (n.Group == null)
                    {
                        p.print(_Type, blank);
                    }
                    p.print(n.Name, blank);
                    if (n.Alias)
                    {
                        p.print(_Assign, blank);
                    }
                    p.print(n.Type);
                    break;
                case ref VarDecl n:
                    if (n.Group == null)
                    {
                        p.print(_Var, blank);
                    }
                    p.printNameList(n.NameList);
                    if (n.Type != null)
                    {
                        p.print(blank, n.Type);
                    }
                    if (n.Values != null)
                    {
                        p.print(blank, _Assign, blank, n.Values);
                    }
                    break;
                case ref FuncDecl n:
                    p.print(_Func, blank);
                    {
                        var r = n.Recv;

                        if (r != null)
                        {
                            p.print(_Lparen);
                            if (r.Name != null)
                            {
                                p.print(r.Name, blank);
                            }
                            p.printNode(r.Type);
                            p.print(_Rparen, blank);
                        }

                    }
                    p.print(n.Name);
                    p.printSignature(n.Type);
                    if (n.Body != null)
                    {
                        p.print(blank, n.Body);
                    }
                    break;
                case ref printGroup n:
                    p.print(n.Tok, blank, _Lparen);
                    if (len(n.Decls) > 0L)
                    {
                        p.print(newline, indent);
                        foreach (var (_, d) in n.Decls)
                        {
                            p.printNode(d);
                            p.print(_Semi, newline);
                        }
                        p.print(outdent);
                    }
                    p.print(_Rparen); 

                    // files
                    break;
                case ref File n:
                    p.print(_Package, blank, n.PkgName);
                    if (len(n.DeclList) > 0L)
                    {
                        p.print(_Semi, newline, newline);
                        p.printDeclList(n.DeclList);
                    }
                    break;
                default:
                {
                    var n = n.type();
                    panic(fmt.Sprintf("syntax.Iterate: unexpected node type %T", n));
                    break;
                }
            }
        });

        private static void printFields(this ref printer p, slice<ref Field> fields, slice<ref BasicLit> tags, long i, long j)
        {
            if (i + 1L == j && fields[i].Name == null)
            { 
                // anonymous field
                p.printNode(fields[i].Type);
            }
            else
            {
                foreach (var (k, f) in fields[i..j])
                {
                    if (k > 0L)
                    {
                        p.print(_Comma, blank);
                    }
                    p.printNode(f.Name);
                }
                p.print(blank);
                p.printNode(fields[i].Type);
            }
            if (i < len(tags) && tags[i] != null)
            {
                p.print(blank);
                p.printNode(tags[i]);
            }
        }

        private static void printFieldList(this ref printer p, slice<ref Field> fields, slice<ref BasicLit> tags)
        {
            long i0 = 0L;
            Expr typ = default;
            foreach (var (i, f) in fields)
            {
                if (f.Name == null || f.Type != typ)
                {
                    if (i0 < i)
                    {
                        p.printFields(fields, tags, i0, i);
                        p.print(_Semi, newline);
                        i0 = i;
                    }
                    typ = f.Type;
                }
            }
            p.printFields(fields, tags, i0, len(fields));
        }

        private static void printMethodList(this ref printer p, slice<ref Field> methods)
        {
            foreach (var (i, m) in methods)
            {
                if (i > 0L)
                {
                    p.print(_Semi, newline);
                }
                if (m.Name != null)
                {
                    p.printNode(m.Name);
                    p.printSignature(m.Type._<ref FuncType>());
                }
                else
                {
                    p.printNode(m.Type);
                }
            }
        }

        private static void printNameList(this ref printer p, slice<ref Name> list)
        {
            foreach (var (i, x) in list)
            {
                if (i > 0L)
                {
                    p.print(_Comma, blank);
                }
                p.printNode(x);
            }
        }

        private static void printExprList(this ref printer p, slice<Expr> list)
        {
            foreach (var (i, x) in list)
            {
                if (i > 0L)
                {
                    p.print(_Comma, blank);
                }
                p.printNode(x);
            }
        }

        private static void printExprLines(this ref printer p, slice<Expr> list)
        {
            if (len(list) > 0L)
            {
                p.print(newline, indent);
                foreach (var (_, x) in list)
                {
                    p.print(x, _Comma, newline);
                }
                p.print(outdent);
            }
        }

        private static (token, ref Group) groupFor(Decl d) => func((_, panic, __) =>
        {
            switch (d.type())
            {
                case ref ImportDecl d:
                    return (_Import, d.Group);
                    break;
                case ref ConstDecl d:
                    return (_Const, d.Group);
                    break;
                case ref TypeDecl d:
                    return (_Type, d.Group);
                    break;
                case ref VarDecl d:
                    return (_Var, d.Group);
                    break;
                case ref FuncDecl d:
                    return (_Func, null);
                    break;
                default:
                {
                    var d = d.type();
                    panic("unreachable");
                    break;
                }
            }
        });

        private partial struct printGroup
        {
            public ref node node => ref node_val;
            public token Tok;
            public slice<Decl> Decls;
        }

        private static void printDecl(this ref printer _p, slice<Decl> list) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            var (tok, group) = groupFor(list[0L]);

            if (group == null)
            {
                if (len(list) != 1L)
                {
                    panic("unreachable");
                }
                p.printNode(list[0L]);
                return;
            } 

            // if _, ok := list[0].(*EmptyDecl); ok {
            //     if len(list) != 1 {
            //         panic("unreachable")
            //     }
            //     // TODO(gri) if there are comments inside the empty
            //     // group, we may need to keep the list non-nil
            //     list = nil
            // }

            // printGroup is here for consistent comment handling
            // (this is not yet used)
            printGroup pg = default; 
            // *pg.Comments() = *group.Comments()
            pg.Tok = tok;
            pg.Decls = list;
            p.printNode(ref pg);
        });

        private static void printDeclList(this ref printer p, slice<Decl> list)
        {
            long i0 = 0L;
            token tok = default;
            ref Group group = default;
            foreach (var (i, x) in list)
            {
                {
                    var (s, g) = groupFor(x);

                    if (g == null || g != group)
                    {
                        if (i0 < i)
                        {
                            p.printDecl(list[i0..i]);
                            p.print(_Semi, newline); 
                            // print empty line between different declaration groups,
                            // different kinds of declarations, or between functions
                            if (g != group || s != tok || s == _Func)
                            {
                                p.print(newline);
                            }
                            i0 = i;
                        }
                        tok = s;
                        group = g;
                    }

                }
            }
            p.printDecl(list[i0..]);
        }

        private static void printSignature(this ref printer p, ref FuncType sig)
        {
            p.printParameterList(sig.ParamList);
            {
                var list = sig.ResultList;

                if (list != null)
                {
                    p.print(blank);
                    if (len(list) == 1L && list[0L].Name == null)
                    {
                        p.printNode(list[0L].Type);
                    }
                    else
                    {
                        p.printParameterList(list);
                    }
                }

            }
        }

        private static void printParameterList(this ref printer p, slice<ref Field> list)
        {
            p.print(_Lparen);
            if (len(list) > 0L)
            {
                foreach (var (i, f) in list)
                {
                    if (i > 0L)
                    {
                        p.print(_Comma, blank);
                    }
                    if (f.Name != null)
                    {
                        p.printNode(f.Name);
                        if (i + 1L < len(list))
                        {
                            var f1 = list[i + 1L];
                            if (f1.Name != null && f1.Type == f.Type)
                            {
                                continue; // no need to print type
                            }
                        }
                        p.print(blank);
                    }
                    p.printNode(f.Type);
                }
            }
            p.print(_Rparen);
        }

        private static void printStmtList(this ref printer p, slice<Stmt> list, bool braces)
        {
            foreach (var (i, x) in list)
            {
                p.print(x, _Semi);
                if (i + 1L < len(list))
                {
                    p.print(newline);
                }
                else if (braces)
                { 
                    // Print an extra semicolon if the last statement is
                    // an empty statement and we are in a braced block
                    // because one semicolon is automatically removed.
                    {
                        ref EmptyStmt (_, ok) = x._<ref EmptyStmt>();

                        if (ok)
                        {
                            p.print(x, _Semi);
                        }

                    }
                }
            }
        }

        private static void printSwitchBody(this ref printer p, slice<ref CaseClause> list)
        {
            p.print(_Lbrace);
            if (len(list) > 0L)
            {
                p.print(newline);
                foreach (var (i, c) in list)
                {
                    p.printCaseClause(c, i + 1L == len(list));
                    p.print(newline);
                }
            }
            p.print(_Rbrace);
        }

        private static void printSelectBody(this ref printer p, slice<ref CommClause> list)
        {
            p.print(_Lbrace);
            if (len(list) > 0L)
            {
                p.print(newline);
                foreach (var (i, c) in list)
                {
                    p.printCommClause(c, i + 1L == len(list));
                    p.print(newline);
                }
            }
            p.print(_Rbrace);
        }

        private static void printCaseClause(this ref printer p, ref CaseClause c, bool braces)
        {
            if (c.Cases != null)
            {
                p.print(_Case, blank, c.Cases);
            }
            else
            {
                p.print(_Default);
            }
            p.print(_Colon);
            if (len(c.Body) > 0L)
            {
                p.print(newline, indent);
                p.printStmtList(c.Body, braces);
                p.print(outdent);
            }
        }

        private static void printCommClause(this ref printer p, ref CommClause c, bool braces)
        {
            if (c.Comm != null)
            {
                p.print(_Case, blank);
                p.print(c.Comm);
            }
            else
            {
                p.print(_Default);
            }
            p.print(_Colon);
            if (len(c.Body) > 0L)
            {
                p.print(newline, indent);
                p.printStmtList(c.Body, braces);
                p.print(outdent);
            }
        }
    }
}}}}
