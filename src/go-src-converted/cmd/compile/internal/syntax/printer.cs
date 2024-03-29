// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements printing of syntax trees in source format.

// package syntax -- go2cs converted at 2022 March 13 06:27:03 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\syntax\printer.go
namespace go.cmd.compile.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using strings = strings_package;


// Form controls print formatting.

using System;
public static partial class syntax_package {

public partial struct Form { // : nuint
}

private static readonly Form _ = iota; // default
public static readonly var LineForm = 0; // use spaces instead of linebreaks where possible
public static readonly var ShortForm = 1; // like LineForm but print "…" for non-empty function or composite literal bodies

// Fprint prints node x to w in the specified form.
// It returns the number of bytes written, and whether there was an error.
public static (nint, error) Fprint(io.Writer w, Node x, Form form) => func((defer, _, _) => {
    nint n = default;
    error err = default!;

    printer p = new printer(output:w,form:form,linebreaks:form==0,);

    defer(() => {
        n = p.written;
        {
            var e = recover();

            if (e != null) {
                err = e._<writeError>().err; // re-panics if it's not a writeError
            }

        }
    }());

    p.print(x);
    p.flush(_EOF);

    return ;
});

// String is a convenience functions that prints n in ShortForm
// and returns the printed string.
public static @string String(Node n) {
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    var (_, err) = Fprint(_addr_buf, n, ShortForm);
    if (err != null) {
        fmt.Fprintf(_addr_buf, "<<< ERROR: %s", err);
    }
    return buf.String();
}

private partial struct ctrlSymbol { // : nint
}

private static readonly ctrlSymbol none = iota;
private static readonly var semi = 0;
private static readonly var blank = 1;
private static readonly var newline = 2;
private static readonly var indent = 3;
private static readonly var outdent = 4; 
// comment
// eolComment

private partial struct whitespace {
    public token last;
    public ctrlSymbol kind; //text string // comment text (possibly ""); valid if kind == comment
}

private partial struct printer {
    public io.Writer output;
    public nint written; // number of bytes written
    public Form form;
    public bool linebreaks; // print linebreaks instead of semis

    public nint indent; // current indentation level
    public nint nlcount; // number of consecutive newlines

    public slice<whitespace> pending; // pending whitespace
    public token lastTok; // last token (after any pending semi) processed by print
}

// write is a thin wrapper around p.output.Write
// that takes care of accounting and error handling.
private static void write(this ptr<printer> _addr_p, slice<byte> data) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;

    var (n, err) = p.output.Write(data);
    p.written += n;
    if (err != null) {
        panic(new writeError(err));
    }
});

private static slice<byte> tabBytes = (slice<byte>)"\t\t\t\t\t\t\t\t";private static slice<byte> newlineByte = (slice<byte>)"\n";private static slice<byte> blankByte = (slice<byte>)" ";

private static void writeBytes(this ptr<printer> _addr_p, slice<byte> data) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;

    if (len(data) == 0) {
        panic("expected non-empty []byte");
    }
    if (p.nlcount > 0 && p.indent > 0) { 
        // write indentation
        var n = p.indent;
        while (n > len(tabBytes)) {
            p.write(tabBytes);
            n -= len(tabBytes);
        }
        p.write(tabBytes[..(int)n]);
    }
    p.write(data);
    p.nlcount = 0;
});

private static void writeString(this ptr<printer> _addr_p, @string s) {
    ref printer p = ref _addr_p.val;

    p.writeBytes((slice<byte>)s);
}

// If impliesSemi returns true for a non-blank line's final token tok,
// a semicolon is automatically inserted. Vice versa, a semicolon may
// be omitted in those cases.
private static bool impliesSemi(token tok) {

    if (tok == _Name || tok == _Break || tok == _Continue || tok == _Fallthrough || tok == _Return || tok == _Rparen || tok == _Rbrack || tok == _Rbrace) // TODO(gri) fix this
        return true;
        return false;
}

// TODO(gri) provide table of []byte values for all tokens to avoid repeated string conversion

private static bool lineComment(@string text) {
    return strings.HasPrefix(text, "//");
}

private static void addWhitespace(this ptr<printer> _addr_p, ctrlSymbol kind, @string text) {
    ref printer p = ref _addr_p.val;

    p.pending = append(p.pending, new whitespace(p.lastTok,kind));

    if (kind == semi) 
        p.lastTok = _Semi;
    else if (kind == newline) 
        p.lastTok = 0; 
        // TODO(gri) do we need to handle /*-style comments containing newlines here?
    }

private static void flush(this ptr<printer> _addr_p, token next) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;
 
    // eliminate semis and redundant whitespace
    var sawNewline = next == _EOF;
    var sawParen = next == _Rparen || next == _Rbrace;
    {
        var i__prev1 = i;

        for (var i = len(p.pending) - 1; i >= 0; i--) {

            if (p.pending[i].kind == semi) 
                var k = semi;
                if (sawParen) {
                    sawParen = false;
                    k = none; // eliminate semi
                }
                else if (sawNewline && impliesSemi(p.pending[i].last)) {
                    sawNewline = false;
                    k = none; // eliminate semi
                }
                p.pending[i].kind = k;
            else if (p.pending[i].kind == newline) 
                sawNewline = true;
            else if (p.pending[i].kind == blank || p.pending[i].kind == indent || p.pending[i].kind == outdent)             else 
                panic("unreachable");
                    }

        i = i__prev1;
    } 

    // print pending
    var prev = none;
    {
        var i__prev1 = i;

        foreach (var (__i) in p.pending) {
            i = __i;

            if (p.pending[i].kind == none)             else if (p.pending[i].kind == semi) 
                p.writeString(";");
                p.nlcount = 0;
                prev = semi;
            else if (p.pending[i].kind == blank) 
                if (prev != blank) { 
                    // at most one blank
                    p.writeBytes(blankByte);
                    p.nlcount = 0;
                    prev = blank;
                }
            else if (p.pending[i].kind == newline) 
                const nint maxEmptyLines = 1;

                if (p.nlcount <= maxEmptyLines) {
                    p.write(newlineByte);
                    p.nlcount++;
                    prev = newline;
                }
            else if (p.pending[i].kind == indent) 
                p.indent++;
            else if (p.pending[i].kind == outdent) 
                p.indent--;
                if (p.indent < 0) {
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

    p.pending = p.pending[..(int)0]; // re-use underlying array
});

private static bool mayCombine(token prev, byte next) {
    bool b = default;

    return ; // for now
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

private static void print(this ptr<printer> _addr_p, params object[] args) => func((_, panic, _) => {
    args = args.Clone();
    ref printer p = ref _addr_p.val;

    for (nint i = 0; i < len(args); i++) {
        switch (args[i].type()) {
            case 
                break;
            case Node x:
                p.printNode(x);
                break;
            case token x:
                @string s = default;
                if (x == _Name) {
                    i++;
                    if (i >= len(args)) {
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
                if (mayCombine(p.lastTok, s[0])) {
                    panic("adjacent tokens combine without whitespace");
                }
                if (x == _Semi) { 
                    // delay printing of semi
                    p.addWhitespace(semi, "");
                }
                else
 {
                    p.flush(x);
                    p.writeString(s);
                    p.nlcount = 0;
                    p.lastTok = x;
                }
                break;
            case Operator x:
                if (x != 0) {
                    p.flush(_Operator);
                    p.writeString(x.String());
                }
                break;
            case ctrlSymbol x:

                if (x == none || x == semi) 
                    panic("unreachable");
                else if (x == newline) 
                    // TODO(gri) need to handle mandatory newlines after a //-style comment
                    if (!p.linebreaks) {
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

private static void printNode(this ptr<printer> _addr_p, Node n) {
    ref printer p = ref _addr_p.val;
 
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

private static void printRawNode(this ptr<printer> _addr_p, Node n) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;

    switch (n.type()) {
        case 
            break;
        case ptr<BadExpr> n:
            p.print(_Name, "<bad expr>");
            break;
        case ptr<Name> n:
            p.print(_Name, n.Value); // _Name requires actual value following immediately
            break;
        case ptr<BasicLit> n:
            p.print(_Name, n.Value); // _Name requires actual value following immediately
            break;
        case ptr<FuncLit> n:
            p.print(n.Type, blank);
            if (n.Body != null) {
                if (p.form == ShortForm) {
                    p.print(_Lbrace);
                    if (len(n.Body.List) > 0) {
                        p.print(_Name, "…");
                    }
                    p.print(_Rbrace);
                }
                else
 {
                    p.print(n.Body);
                }
            }
            break;
        case ptr<CompositeLit> n:
            if (n.Type != null) {
                p.print(n.Type);
            }
            p.print(_Lbrace);
            if (p.form == ShortForm) {
                if (len(n.ElemList) > 0) {
                    p.print(_Name, "…");
                }
            }
            else
 {
                if (n.NKeys > 0 && n.NKeys == len(n.ElemList)) {
                    p.printExprLines(n.ElemList);
                }
                else
 {
                    p.printExprList(n.ElemList);
                }
            }
            p.print(_Rbrace);
            break;
        case ptr<ParenExpr> n:
            p.print(_Lparen, n.X, _Rparen);
            break;
        case ptr<SelectorExpr> n:
            p.print(n.X, _Dot, n.Sel);
            break;
        case ptr<IndexExpr> n:
            p.print(n.X, _Lbrack, n.Index, _Rbrack);
            break;
        case ptr<SliceExpr> n:
            p.print(n.X, _Lbrack);
            {
                var i = n.Index[0];

                if (i != null) {
                    p.printNode(i);
                }

            }
            p.print(_Colon);
            {
                var j = n.Index[1];

                if (j != null) {
                    p.printNode(j);
                }

            }
            {
                var k = n.Index[2];

                if (k != null) {
                    p.print(_Colon, k);
                }

            }
            p.print(_Rbrack);
            break;
        case ptr<AssertExpr> n:
            p.print(n.X, _Dot, _Lparen, n.Type, _Rparen);
            break;
        case ptr<TypeSwitchGuard> n:
            if (n.Lhs != null) {
                p.print(n.Lhs, blank, _Define, blank);
            }
            p.print(n.X, _Dot, _Lparen, _Type, _Rparen);
            break;
        case ptr<CallExpr> n:
            p.print(n.Fun, _Lparen);
            p.printExprList(n.ArgList);
            if (n.HasDots) {
                p.print(_DotDotDot);
            }
            p.print(_Rparen);
            break;
        case ptr<Operation> n:
            if (n.Y == null) { 
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
        case ptr<KeyValueExpr> n:
            p.print(n.Key, _Colon, blank, n.Value);
            break;
        case ptr<ListExpr> n:
            p.printExprList(n.ElemList);
            break;
        case ptr<ArrayType> n:
            var len = _DotDotDot;
            if (n.Len != null) {
                len = n.Len;
            }
            p.print(_Lbrack, len, _Rbrack, n.Elem);
            break;
        case ptr<SliceType> n:
            p.print(_Lbrack, _Rbrack, n.Elem);
            break;
        case ptr<DotsType> n:
            p.print(_DotDotDot, n.Elem);
            break;
        case ptr<StructType> n:
            p.print(_Struct);
            if (len(n.FieldList) > 0 && p.linebreaks) {
                p.print(blank);
            }
            p.print(_Lbrace);
            if (len(n.FieldList) > 0) {
                if (p.linebreaks) {
                    p.print(newline, indent);
                    p.printFieldList(n.FieldList, n.TagList, _Semi);
                    p.print(outdent, newline);
                }
                else
 {
                    p.printFieldList(n.FieldList, n.TagList, _Semi);
                }
            }
            p.print(_Rbrace);
            break;
        case ptr<FuncType> n:
            p.print(_Func);
            p.printSignature(n);
            break;
        case ptr<InterfaceType> n:
            slice<Expr> types = default;
            slice<ptr<Field>> methods = default;
            foreach (var (_, f) in n.MethodList) {
                if (f.Name != null && f.Name.Value == "type") {
                    types = append(types, f.Type);
                }
                else
 { 
                    // method or embedded interface
                    methods = append(methods, f);
                }
            }
            var multiLine = len(n.MethodList) > 0 && p.linebreaks;
            p.print(_Interface);
            if (multiLine) {
                p.print(blank);
            }
            p.print(_Lbrace);
            if (multiLine) {
                p.print(newline, indent);
            }
            if (len(types) > 0) {
                p.print(_Type, blank);
                p.printExprList(types);
                if (len(methods) > 0) {
                    p.print(_Semi, blank);
                }
            }
            if (len(methods) > 0) {
                p.printMethodList(methods);
            }
            if (multiLine) {
                p.print(outdent, newline);
            }
            p.print(_Rbrace);
            break;
        case ptr<MapType> n:
            p.print(_Map, _Lbrack, n.Key, _Rbrack, n.Value);
            break;
        case ptr<ChanType> n:
            if (n.Dir == RecvOnly) {
                p.print(_Arrow);
            }
            p.print(_Chan);
            if (n.Dir == SendOnly) {
                p.print(_Arrow);
            }
            p.print(blank);
            {
                ptr<ChanType> (e, _) = n.Elem._<ptr<ChanType>>();

                if (n.Dir == 0 && e != null && e.Dir == RecvOnly) { 
                    // don't print chan (<-chan T) as chan <-chan T
                    p.print(_Lparen);
                    p.print(n.Elem);
                    p.print(_Rparen);
                }
                else
 {
                    p.print(n.Elem);
                } 

                // statements

            } 

            // statements
            break;
        case ptr<DeclStmt> n:
            p.printDecl(n.DeclList);
            break;
        case ptr<EmptyStmt> n:
            break;
        case ptr<LabeledStmt> n:
            p.print(outdent, n.Label, _Colon, indent, newline, n.Stmt);
            break;
        case ptr<ExprStmt> n:
            p.print(n.X);
            break;
        case ptr<SendStmt> n:
            p.print(n.Chan, blank, _Arrow, blank, n.Value);
            break;
        case ptr<AssignStmt> n:
            p.print(n.Lhs);
            if (n.Rhs == null) { 
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
        case ptr<CallStmt> n:
            p.print(n.Tok, blank, n.Call);
            break;
        case ptr<ReturnStmt> n:
            p.print(_Return);
            if (n.Results != null) {
                p.print(blank, n.Results);
            }
            break;
        case ptr<BranchStmt> n:
            p.print(n.Tok);
            if (n.Label != null) {
                p.print(blank, n.Label);
            }
            break;
        case ptr<BlockStmt> n:
            p.print(_Lbrace);
            if (len(n.List) > 0) {
                p.print(newline, indent);
                p.printStmtList(n.List, true);
                p.print(outdent, newline);
            }
            p.print(_Rbrace);
            break;
        case ptr<IfStmt> n:
            p.print(_If, blank);
            if (n.Init != null) {
                p.print(n.Init, _Semi, blank);
            }
            p.print(n.Cond, blank, n.Then);
            if (n.Else != null) {
                p.print(blank, _Else, blank, n.Else);
            }
            break;
        case ptr<SwitchStmt> n:
            p.print(_Switch, blank);
            if (n.Init != null) {
                p.print(n.Init, _Semi, blank);
            }
            if (n.Tag != null) {
                p.print(n.Tag, blank);
            }
            p.printSwitchBody(n.Body);
            break;
        case ptr<SelectStmt> n:
            p.print(_Select, blank); // for now
            p.printSelectBody(n.Body);
            break;
        case ptr<RangeClause> n:
            if (n.Lhs != null) {
                var tok = _Assign;
                if (n.Def) {
                    tok = _Define;
                }
                p.print(n.Lhs, blank, tok, blank);
            }
            p.print(_Range, blank, n.X);
            break;
        case ptr<ForStmt> n:
            p.print(_For, blank);
            if (n.Init == null && n.Post == null) {
                if (n.Cond != null) {
                    p.print(n.Cond, blank);
                }
            }
            else
 {
                if (n.Init != null) {
                    p.print(n.Init); 
                    // TODO(gri) clean this up
                    {
                        ptr<RangeClause> (_, ok) = n.Init._<ptr<RangeClause>>();

                        if (ok) {
                            p.print(blank, n.Body);
                            break;
                        }

                    }
                }
                p.print(_Semi, blank);
                if (n.Cond != null) {
                    p.print(n.Cond);
                }
                p.print(_Semi, blank);
                if (n.Post != null) {
                    p.print(n.Post, blank);
                }
            }
            p.print(n.Body);
            break;
        case ptr<ImportDecl> n:
            if (n.Group == null) {
                p.print(_Import, blank);
            }
            if (n.LocalPkgName != null) {
                p.print(n.LocalPkgName, blank);
            }
            p.print(n.Path);
            break;
        case ptr<ConstDecl> n:
            if (n.Group == null) {
                p.print(_Const, blank);
            }
            p.printNameList(n.NameList);
            if (n.Type != null) {
                p.print(blank, n.Type);
            }
            if (n.Values != null) {
                p.print(blank, _Assign, blank, n.Values);
            }
            break;
        case ptr<TypeDecl> n:
            if (n.Group == null) {
                p.print(_Type, blank);
            }
            p.print(n.Name);
            if (n.TParamList != null) {
                p.print(_Lbrack);
                p.printFieldList(n.TParamList, null, _Comma);
                p.print(_Rbrack);
            }
            p.print(blank);
            if (n.Alias) {
                p.print(_Assign, blank);
            }
            p.print(n.Type);
            break;
        case ptr<VarDecl> n:
            if (n.Group == null) {
                p.print(_Var, blank);
            }
            p.printNameList(n.NameList);
            if (n.Type != null) {
                p.print(blank, n.Type);
            }
            if (n.Values != null) {
                p.print(blank, _Assign, blank, n.Values);
            }
            break;
        case ptr<FuncDecl> n:
            p.print(_Func, blank);
            {
                var r = n.Recv;

                if (r != null) {
                    p.print(_Lparen);
                    if (r.Name != null) {
                        p.print(r.Name, blank);
                    }
                    p.printNode(r.Type);
                    p.print(_Rparen, blank);
                }

            }
            p.print(n.Name);
            if (n.TParamList != null) {
                p.print(_Lbrack);
                p.printFieldList(n.TParamList, null, _Comma);
                p.print(_Rbrack);
            }
            p.printSignature(n.Type);
            if (n.Body != null) {
                p.print(blank, n.Body);
            }
            break;
        case ptr<printGroup> n:
            p.print(n.Tok, blank, _Lparen);
            if (len(n.Decls) > 0) {
                p.print(newline, indent);
                foreach (var (_, d) in n.Decls) {
                    p.printNode(d);
                    p.print(_Semi, newline);
                }
                p.print(outdent);
            }
            p.print(_Rparen); 

            // files
            break;
        case ptr<File> n:
            p.print(_Package, blank, n.PkgName);
            if (len(n.DeclList) > 0) {
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

private static void printFields(this ptr<printer> _addr_p, slice<ptr<Field>> fields, slice<ptr<BasicLit>> tags, nint i, nint j) {
    ref printer p = ref _addr_p.val;

    if (i + 1 == j && fields[i].Name == null) { 
        // anonymous field
        p.printNode(fields[i].Type);
    }
    else
 {
        foreach (var (k, f) in fields[(int)i..(int)j]) {
            if (k > 0) {
                p.print(_Comma, blank);
            }
            p.printNode(f.Name);
        }        p.print(blank);
        p.printNode(fields[i].Type);
    }
    if (i < len(tags) && tags[i] != null) {
        p.print(blank);
        p.printNode(tags[i]);
    }
}

private static void printFieldList(this ptr<printer> _addr_p, slice<ptr<Field>> fields, slice<ptr<BasicLit>> tags, token sep) {
    ref printer p = ref _addr_p.val;

    nint i0 = 0;
    Expr typ = default;
    foreach (var (i, f) in fields) {
        if (f.Name == null || f.Type != typ) {
            if (i0 < i) {
                p.printFields(fields, tags, i0, i);
                p.print(sep, newline);
                i0 = i;
            }
            typ = f.Type;
        }
    }    p.printFields(fields, tags, i0, len(fields));
}

private static void printMethodList(this ptr<printer> _addr_p, slice<ptr<Field>> methods) {
    ref printer p = ref _addr_p.val;

    foreach (var (i, m) in methods) {
        if (i > 0) {
            p.print(_Semi, newline);
        }
        if (m.Name != null) {
            p.printNode(m.Name);
            p.printSignature(m.Type._<ptr<FuncType>>());
        }
        else
 {
            p.printNode(m.Type);
        }
    }
}

private static void printNameList(this ptr<printer> _addr_p, slice<ptr<Name>> list) {
    ref printer p = ref _addr_p.val;

    foreach (var (i, x) in list) {
        if (i > 0) {
            p.print(_Comma, blank);
        }
        p.printNode(x);
    }
}

private static void printExprList(this ptr<printer> _addr_p, slice<Expr> list) {
    ref printer p = ref _addr_p.val;

    foreach (var (i, x) in list) {
        if (i > 0) {
            p.print(_Comma, blank);
        }
        p.printNode(x);
    }
}

private static void printExprLines(this ptr<printer> _addr_p, slice<Expr> list) {
    ref printer p = ref _addr_p.val;

    if (len(list) > 0) {
        p.print(newline, indent);
        foreach (var (_, x) in list) {
            p.print(x, _Comma, newline);
        }        p.print(outdent);
    }
}

private static (token, ptr<Group>) groupFor(Decl d) => func((_, panic, _) => {
    token _p0 = default;
    ptr<Group> _p0 = default!;

    switch (d.type()) {
        case ptr<ImportDecl> d:
            return (_Import, _addr_d.Group!);
            break;
        case ptr<ConstDecl> d:
            return (_Const, _addr_d.Group!);
            break;
        case ptr<TypeDecl> d:
            return (_Type, _addr_d.Group!);
            break;
        case ptr<VarDecl> d:
            return (_Var, _addr_d.Group!);
            break;
        case ptr<FuncDecl> d:
            return (_Func, _addr_null!);
            break;
        default:
        {
            var d = d.type();
            panic("unreachable");
            break;
        }
    }
});

private partial struct printGroup {
    public ref node node => ref node_val;
    public token Tok;
    public slice<Decl> Decls;
}

private static void printDecl(this ptr<printer> _addr_p, slice<Decl> list) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;

    var (tok, group) = groupFor(list[0]);

    if (group == null) {
        if (len(list) != 1) {
            panic("unreachable");
        }
        p.printNode(list[0]);
        return ;
    }
    ref printGroup pg = ref heap(out ptr<printGroup> _addr_pg); 
    // *pg.Comments() = *group.Comments()
    pg.Tok = tok;
    pg.Decls = list;
    p.printNode(_addr_pg);
});

private static void printDeclList(this ptr<printer> _addr_p, slice<Decl> list) {
    ref printer p = ref _addr_p.val;

    nint i0 = 0;
    token tok = default;
    ptr<Group> group;
    foreach (var (i, x) in list) {
        {
            var (s, g) = groupFor(x);

            if (g == null || g != group) {
                if (i0 < i) {
                    p.printDecl(list[(int)i0..(int)i]);
                    p.print(_Semi, newline); 
                    // print empty line between different declaration groups,
                    // different kinds of declarations, or between functions
                    if (g != group || s != tok || s == _Func) {
                        p.print(newline);
                    }
                    i0 = i;
                }
                (tok, group) = (s, g);
            }

        }
    }    p.printDecl(list[(int)i0..]);
}

private static void printSignature(this ptr<printer> _addr_p, ptr<FuncType> _addr_sig) {
    ref printer p = ref _addr_p.val;
    ref FuncType sig = ref _addr_sig.val;

    p.printParameterList(sig.ParamList);
    {
        var list = sig.ResultList;

        if (list != null) {
            p.print(blank);
            if (len(list) == 1 && list[0].Name == null) {
                p.printNode(list[0].Type);
            }
            else
 {
                p.printParameterList(list);
            }
        }
    }
}

private static void printParameterList(this ptr<printer> _addr_p, slice<ptr<Field>> list) {
    ref printer p = ref _addr_p.val;

    p.print(_Lparen);
    if (len(list) > 0) {
        foreach (var (i, f) in list) {
            if (i > 0) {
                p.print(_Comma, blank);
            }
            if (f.Name != null) {
                p.printNode(f.Name);
                if (i + 1 < len(list)) {
                    var f1 = list[i + 1];
                    if (f1.Name != null && f1.Type == f.Type) {
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

private static void printStmtList(this ptr<printer> _addr_p, slice<Stmt> list, bool braces) {
    ref printer p = ref _addr_p.val;

    foreach (var (i, x) in list) {
        p.print(x, _Semi);
        if (i + 1 < len(list)) {
            p.print(newline);
        }
        else if (braces) { 
            // Print an extra semicolon if the last statement is
            // an empty statement and we are in a braced block
            // because one semicolon is automatically removed.
            {
                ptr<EmptyStmt> (_, ok) = x._<ptr<EmptyStmt>>();

                if (ok) {
                    p.print(x, _Semi);
                }

            }
        }
    }
}

private static void printSwitchBody(this ptr<printer> _addr_p, slice<ptr<CaseClause>> list) {
    ref printer p = ref _addr_p.val;

    p.print(_Lbrace);
    if (len(list) > 0) {
        p.print(newline);
        foreach (var (i, c) in list) {
            p.printCaseClause(c, i + 1 == len(list));
            p.print(newline);
        }
    }
    p.print(_Rbrace);
}

private static void printSelectBody(this ptr<printer> _addr_p, slice<ptr<CommClause>> list) {
    ref printer p = ref _addr_p.val;

    p.print(_Lbrace);
    if (len(list) > 0) {
        p.print(newline);
        foreach (var (i, c) in list) {
            p.printCommClause(c, i + 1 == len(list));
            p.print(newline);
        }
    }
    p.print(_Rbrace);
}

private static void printCaseClause(this ptr<printer> _addr_p, ptr<CaseClause> _addr_c, bool braces) {
    ref printer p = ref _addr_p.val;
    ref CaseClause c = ref _addr_c.val;

    if (c.Cases != null) {
        p.print(_Case, blank, c.Cases);
    }
    else
 {
        p.print(_Default);
    }
    p.print(_Colon);
    if (len(c.Body) > 0) {
        p.print(newline, indent);
        p.printStmtList(c.Body, braces);
        p.print(outdent);
    }
}

private static void printCommClause(this ptr<printer> _addr_p, ptr<CommClause> _addr_c, bool braces) {
    ref printer p = ref _addr_p.val;
    ref CommClause c = ref _addr_c.val;

    if (c.Comm != null) {
        p.print(_Case, blank);
        p.print(c.Comm);
    }
    else
 {
        p.print(_Default);
    }
    p.print(_Colon);
    if (len(c.Body) > 0) {
        p.print(newline, indent);
        p.printStmtList(c.Body, braces);
        p.print(outdent);
    }
}

} // end syntax_package
