// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package parser implements a parser for Go source files. Input may be
// provided in a variety of forms (see the various Parse* functions); the
// output is an abstract syntax tree (AST) representing the Go source. The
// parser is invoked through one of the Parse* functions.

// package parser -- go2cs converted at 2020 August 29 08:52:41 UTC
// import "go/printer.parser" ==> using parser = go.go.printer.parser_package
// Original source: C:\Go\src\go\printer\testdata\parser.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class parser_package
    {
        // The mode parameter to the Parse* functions is a set of flags (or 0).
        // They control the amount of source code parsed and other optional
        // parser functionality.
        //
        public static readonly ulong PackageClauseOnly = 1L << (int)(iota); // parsing stops after package clause
        public static readonly var ImportsOnly = 0; // parsing stops after import declarations
        public static readonly var ParseComments = 1; // parse comments and add them to AST
        public static readonly var Trace = 2; // print a trace of parsed productions
        public static readonly var DeclarationErrors = 3; // report declaration errors

        // The parser structure holds the parser's internal state.
        private partial struct parser
        {
            public ptr<token.File> file;
            public ref scanner.ErrorVector ErrorVector => ref ErrorVector_val;
            public scanner.Scanner scanner; // Tracing/debugging
            public ulong mode; // parsing mode
            public bool trace; // == (mode & Trace != 0)
            public ulong indent; // indentation used for tracing output

// Comments
            public slice<ref ast.CommentGroup> comments;
            public ptr<ast.CommentGroup> leadComment; // last lead comment
            public ptr<ast.CommentGroup> lineComment; // last line comment

// Next token
            public token.Pos pos; // token position
            public token.Token tok; // one token look-ahead
            public @string lit; // token literal

// Non-syntactic parser control
            public long exprLev; // < 0: in control clause, >= 0: in expression

// Ordinary identifier scopes
            public ptr<ast.Scope> pkgScope; // pkgScope.Outer == nil
            public ptr<ast.Scope> topScope; // top-most scope; may be pkgScope
            public slice<ref ast.Ident> unresolved; // unresolved identifiers
            public slice<ref ast.ImportSpec> imports; // list of imports

// Label scope
// (maintained by open/close LabelScope)
            public ptr<ast.Scope> labelScope; // label scope for current function
            public slice<slice<ref ast.Ident>> targetStack; // stack of unresolved labels
        }

        // scannerMode returns the scanner mode bits given the parser's mode bits.
        private static ulong scannerMode(ulong mode)
        {
            ulong m = scanner.InsertSemis;
            if (mode & ParseComments != 0L)
            {
                m |= scanner.ScanComments;
            }
            return m;
        }

        private static void init(this ref parser p, ref token.FileSet fset, @string filename, slice<byte> src, ulong mode)
        {
            p.file = fset.AddFile(filename, fset.Base(), len(src));
            p.scanner.Init(p.file, src, p, scannerMode(mode));

            p.mode = mode;
            p.trace = mode & Trace != 0L; // for convenience (p.trace is used frequently)

            p.next(); 

            // set up the pkgScope here (as opposed to in parseFile) because
            // there are other parser entry points (ParseExpr, etc.)
            p.openScope();
            p.pkgScope = p.topScope; 

            // for the same reason, set up a label scope
            p.openLabelScope();
        }

        // ----------------------------------------------------------------------------
        // Scoping support

        private static void openScope(this ref parser p)
        {
            p.topScope = ast.NewScope(p.topScope);
        }

        private static void closeScope(this ref parser p)
        {
            p.topScope = p.topScope.Outer;
        }

        private static void openLabelScope(this ref parser p)
        {
            p.labelScope = ast.NewScope(p.labelScope);
            p.targetStack = append(p.targetStack, null);
        }

        private static void closeLabelScope(this ref parser p)
        { 
            // resolve labels
            var n = len(p.targetStack) - 1L;
            var scope = p.labelScope;
            foreach (var (_, ident) in p.targetStack[n])
            {
                ident.Obj = scope.Lookup(ident.Name);
                if (ident.Obj == null && p.mode & DeclarationErrors != 0L)
                {
                    p.error(ident.Pos(), fmt.Sprintf("label %s undefined", ident.Name));
                }
            } 
            // pop label scope
            p.targetStack = p.targetStack[0L..n];
            p.labelScope = p.labelScope.Outer;
        }

        private static void declare(this ref parser p, object decl, ref ast.Scope scope, ast.ObjKind kind, params ptr<ast.Ident>[] idents)
        {
            foreach (var (_, ident) in idents)
            {
                assert(ident.Obj == null, "identifier already declared or resolved");
                if (ident.Name != "_")
                {
                    var obj = ast.NewObj(kind, ident.Name); 
                    // remember the corresponding declaration for redeclaration
                    // errors and global variable resolution/typechecking phase
                    obj.Decl = decl;
                    {
                        var alt = scope.Insert(obj);

                        if (alt != null && p.mode & DeclarationErrors != 0L)
                        {
                            @string prevDecl = "";
                            {
                                var pos = alt.Pos();

                                if (pos.IsValid())
                                {
                                    prevDecl = fmt.Sprintf("\n\tprevious declaration at %s", p.file.Position(pos));
                                }

                            }
                            p.error(ident.Pos(), fmt.Sprintf("%s redeclared in this block%s", ident.Name, prevDecl));
                        }

                    }
                    ident.Obj = obj;
                }
            }
        }

        private static void shortVarDecl(this ref parser p, slice<ref ast.Ident> idents)
        { 
            // Go spec: A short variable declaration may redeclare variables
            // provided they were originally declared in the same block with
            // the same type, and at least one of the non-blank variables is new.
            long n = 0L; // number of new variables
            foreach (var (_, ident) in idents)
            {
                assert(ident.Obj == null, "identifier already declared or resolved");
                if (ident.Name != "_")
                {
                    var obj = ast.NewObj(ast.Var, ident.Name); 
                    // short var declarations cannot have redeclaration errors
                    // and are not global => no need to remember the respective
                    // declaration
                    var alt = p.topScope.Insert(obj);
                    if (alt == null)
                    {
                        n++; // new declaration
                        alt = obj;
                    }
                    ident.Obj = alt;
                }
            }
            if (n == 0L && p.mode & DeclarationErrors != 0L)
            {
                p.error(idents[0L].Pos(), "no new variables on left side of :=");
            }
        }

        // The unresolved object is a sentinel to mark identifiers that have been added
        // to the list of unresolved identifiers. The sentinel is only used for verifying
        // internal consistency.
        private static ptr<object> unresolved = @new<ast.Object>();

        private static void resolve(this ref parser p, ast.Expr x)
        { 
            // nothing to do if x is not an identifier or the blank identifier
            ref ast.Ident (ident, _) = x._<ref ast.Ident>();
            if (ident == null)
            {
                return;
            }
            assert(ident.Obj == null, "identifier already declared or resolved");
            if (ident.Name == "_")
            {
                return;
            } 
            // try to resolve the identifier
            {
                var s = p.topScope;

                while (s != null)
                {
                    {
                        var obj = s.Lookup(ident.Name);

                        if (obj != null)
                        {
                            ident.Obj = obj;
                            return;
                    s = s.Outer;
                        }

                    }
                } 
                // all local scopes are known, so any unresolved identifier
                // must be found either in the file scope, package scope
                // (perhaps in another file), or universe scope --- collect
                // them so that they can be resolved later

            } 
            // all local scopes are known, so any unresolved identifier
            // must be found either in the file scope, package scope
            // (perhaps in another file), or universe scope --- collect
            // them so that they can be resolved later
            ident.Obj = unresolved;
            p.unresolved = append(p.unresolved, ident);
        }

        // ----------------------------------------------------------------------------
        // Parsing support

        private static void printTrace(this ref parser p, params object[] a)
        {
            const @string dots = ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . " + ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . ";

            const var n = uint(len(dots));

            var pos = p.file.Position(p.pos);
            fmt.Printf("%5d:%3d: ", pos.Line, pos.Column);
            long i = 2L * p.indent;
            while (i > n)
            {
                fmt.Print(dots);
                i -= n;
            }

            fmt.Print(dots[0L..i]);
            fmt.Println(a);
        }

        private static ref parser trace(ref parser p, @string msg)
        {
            p.printTrace(msg, "(");
            p.indent++;
            return p;
        }

        // Usage pattern: defer un(trace(p, "..."));
        private static void un(ref parser p)
        {
            p.indent--;
            p.printTrace(")");
        }

        // Advance to the next token.
        private static void next0(this ref parser p)
        { 
            // Because of one-token look-ahead, print the previous token
            // when tracing as it provides a more readable output. The
            // very first token (!p.pos.IsValid()) is not initialized
            // (it is token.ILLEGAL), so don't print it .
            if (p.trace && p.pos.IsValid())
            {
                var s = p.tok.String();

                if (p.tok.IsLiteral()) 
                    p.printTrace(s, p.lit);
                else if (p.tok.IsOperator() || p.tok.IsKeyword()) 
                    p.printTrace("\"" + s + "\"");
                else 
                    p.printTrace(s);
                            }
            p.pos, p.tok, p.lit = p.scanner.Scan();
        }

        // Consume a comment and return it and the line on which it ends.
        private static (ref ast.Comment, long) consumeComment(this ref parser p)
        { 
            // /*-style comments may end on a different line than where they start.
            // Scan the comment for '\n' chars and adjust endline accordingly.
            endline = p.file.Line(p.pos);
            if (p.lit[1L] == '*')
            { 
                // don't use range here - no need to decode Unicode code points
                for (long i = 0L; i < len(p.lit); i++)
                {
                    if (p.lit[i] == '\n')
                    {
                        endline++;
                    }
                }

            }
            comment = ref new ast.Comment(p.pos,p.lit);
            p.next0();

            return;
        }

        // Consume a group of adjacent comments, add it to the parser's
        // comments list, and return it together with the line at which
        // the last comment in the group ends. An empty line or non-comment
        // token terminates a comment group.
        //
        private static (ref ast.CommentGroup, long) consumeCommentGroup(this ref parser p)
        {
            slice<ref ast.Comment> list = default;
            endline = p.file.Line(p.pos);
            while (p.tok == token.COMMENT && endline + 1L >= p.file.Line(p.pos))
            {
                ref ast.Comment comment = default;
                comment, endline = p.consumeComment();
                list = append(list, comment);
            } 

            // add comment group to the comments list
 

            // add comment group to the comments list
            comments = ref new ast.CommentGroup(list);
            p.comments = append(p.comments, comments);

            return;
        }

        // Advance to the next non-comment token. In the process, collect
        // any comment groups encountered, and remember the last lead and
        // and line comments.
        //
        // A lead comment is a comment group that starts and ends in a
        // line without any other tokens and that is followed by a non-comment
        // token on the line immediately after the comment group.
        //
        // A line comment is a comment group that follows a non-comment
        // token on the same line, and that has no tokens after it on the line
        // where it ends.
        //
        // Lead and line comments may be considered documentation that is
        // stored in the AST.
        //
        private static void next(this ref parser p)
        {
            p.leadComment = null;
            p.lineComment = null;
            var line = p.file.Line(p.pos); // current line
            p.next0();

            if (p.tok == token.COMMENT)
            {
                ref ast.CommentGroup comment = default;
                long endline = default;

                if (p.file.Line(p.pos) == line)
                { 
                    // The comment is on same line as the previous token; it
                    // cannot be a lead comment but may be a line comment.
                    comment, endline = p.consumeCommentGroup();
                    if (p.file.Line(p.pos) != endline)
                    { 
                        // The next token is on a different line, thus
                        // the last comment group is a line comment.
                        p.lineComment = comment;
                    }
                } 

                // consume successor comments, if any
                endline = -1L;
                while (p.tok == token.COMMENT)
                {
                    comment, endline = p.consumeCommentGroup();
                }


                if (endline + 1L == p.file.Line(p.pos))
                { 
                    // The next token is following on the line immediately after the
                    // comment group, thus the last comment group is a lead comment.
                    p.leadComment = comment;
                }
            }
        }

        private static void error(this ref parser p, token.Pos pos, @string msg)
        {
            p.Error(p.file.Position(pos), msg);
        }

        private static void errorExpected(this ref parser p, token.Pos pos, @string msg)
        {
            msg = "expected " + msg;
            if (pos == p.pos)
            { 
                // the error happened at the current position;
                // make the error message more specific
                if (p.tok == token.SEMICOLON && p.lit[0L] == '\n')
                {
                    msg += ", found newline";
                }
                else
                {
                    msg += ", found '" + p.tok.String() + "'";
                    if (p.tok.IsLiteral())
                    {
                        msg += " " + p.lit;
                    }
                }
            }
            p.error(pos, msg);
        }

        private static token.Pos expect(this ref parser p, token.Token tok)
        {
            var pos = p.pos;
            if (p.tok != tok)
            {
                p.errorExpected(pos, "'" + tok.String() + "'");
            }
            p.next(); // make progress
            return pos;
        }

        private static void expectSemi(this ref parser p)
        {
            if (p.tok != token.RPAREN && p.tok != token.RBRACE)
            {
                p.expect(token.SEMICOLON);
            }
        }

        private static void assert(bool cond, @string msg) => func((_, panic, __) =>
        {
            if (!cond)
            {
                panic("go/parser internal error: " + msg);
            }
        });

        // ----------------------------------------------------------------------------
        // Identifiers

        private static ref ast.Ident parseIdent(this ref parser p)
        {
            var pos = p.pos;
            @string name = "_";
            if (p.tok == token.IDENT)
            {
                name = p.lit;
                p.next();
            }
            else
            {
                p.expect(token.IDENT); // use expect() error handling
            }
            return ref new ast.Ident(pos,name,nil);
        }

        private static slice<ref ast.Ident> parseIdentList(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "IdentList")));
            }
            list = append(list, p.parseIdent());
            while (p.tok == token.COMMA)
            {
                p.next();
                list = append(list, p.parseIdent());
            }


            return;
        });

        // ----------------------------------------------------------------------------
        // Common productions

        // If lhs is set, result list elements which are identifiers are not resolved.
        private static slice<ast.Expr> parseExprList(this ref parser _p, bool lhs) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "ExpressionList")));
            }
            list = append(list, p.parseExpr(lhs));
            while (p.tok == token.COMMA)
            {
                p.next();
                list = append(list, p.parseExpr(lhs));
            }


            return;
        });

        private static slice<ast.Expr> parseLhsList(this ref parser p)
        {
            var list = p.parseExprList(true);

            if (p.tok == token.DEFINE) 
                // lhs of a short variable declaration
                p.shortVarDecl(p.makeIdentList(list));
            else if (p.tok == token.COLON)             else 
                // identifiers must be declared elsewhere
                foreach (var (_, x) in list)
                {
                    p.resolve(x);
                }
                        return list;
        }

        private static slice<ast.Expr> parseRhsList(this ref parser p)
        {
            return p.parseExprList(false);
        }

        // ----------------------------------------------------------------------------
        // Types

        private static ast.Expr parseType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Type")));
            }
            var typ = p.tryType();

            if (typ == null)
            {
                var pos = p.pos;
                p.errorExpected(pos, "type");
                p.next(); // make progress
                return ref new ast.BadExpr(pos,p.pos);
            }
            return typ;
        });

        // If the result is an identifier, it is not resolved.
        private static ast.Expr parseTypeName(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "TypeName")));
            }
            var ident = p.parseIdent(); 
            // don't resolve ident yet - it may be a parameter or field name

            if (p.tok == token.PERIOD)
            { 
                // ident is a package name
                p.next();
                p.resolve(ident);
                var sel = p.parseIdent();
                return ref new ast.SelectorExpr(ident,sel);
            }
            return ident;
        });

        private static ast.Expr parseArrayType(this ref parser _p, bool ellipsisOk) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "ArrayType")));
            }
            var lbrack = p.expect(token.LBRACK);
            ast.Expr len = default;
            if (ellipsisOk && p.tok == token.ELLIPSIS)
            {
                len = ref new ast.Ellipsis(p.pos,nil);
                p.next();
            }
            else if (p.tok != token.RBRACK)
            {
                len = p.parseRhs();
            }
            p.expect(token.RBRACK);
            var elt = p.parseType();

            return ref new ast.ArrayType(lbrack,len,elt);
        });

        private static slice<ref ast.Ident> makeIdentList(this ref parser p, slice<ast.Expr> list)
        {
            var idents = make_slice<ref ast.Ident>(len(list));
            foreach (var (i, x) in list)
            {
                ref ast.Ident (ident, isIdent) = x._<ref ast.Ident>();
                if (!isIdent)
                {
                    ast.Expr pos = x._<ast.Expr>().Pos();
                    p.errorExpected(pos, "identifier");
                    ident = ref new ast.Ident(pos,"_",nil);
                }
                idents[i] = ident;
            }
            return idents;
        }

        private static ref ast.Field parseFieldDecl(this ref parser _p, ref ast.Scope _scope) => func(_p, _scope, (ref parser p, ref ast.Scope scope, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "FieldDecl")));
            }
            var doc = p.leadComment; 

            // fields
            var (list, typ) = p.parseVarList(false); 

            // optional tag
            ref ast.BasicLit tag = default;
            if (p.tok == token.STRING)
            {
                tag = ref new ast.BasicLit(p.pos,p.tok,p.lit);
                p.next();
            } 

            // analyze case
            slice<ref ast.Ident> idents = default;
            if (typ != null)
            { 
                // IdentifierList Type
                idents = p.makeIdentList(list);
            }
            else
            { 
                // ["*"] TypeName (AnonymousField)
                typ = list[0L]; // we always have at least one element
                p.resolve(typ);
                {
                    var n = len(list);

                    if (n > 1L || !isTypeName(deref(typ)))
                    {
                        var pos = typ.Pos();
                        p.errorExpected(pos, "anonymous field");
                        typ = ref new ast.BadExpr(pos,list[n-1].End());
                    }

                }
            }
            p.expectSemi(); // call before accessing p.linecomment

            ast.Field field = ref new ast.Field(doc,idents,typ,tag,p.lineComment);
            p.declare(field, scope, ast.Var, idents);

            return field;
        });

        private static ref ast.StructType parseStructType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "StructType")));
            }
            var pos = p.expect(token.STRUCT);
            var lbrace = p.expect(token.LBRACE);
            var scope = ast.NewScope(null); // struct scope
            slice<ref ast.Field> list = default;
            while (p.tok == token.IDENT || p.tok == token.MUL || p.tok == token.LPAREN)
            { 
                // a field declaration cannot start with a '(' but we accept
                // it here for more robust parsing and better error messages
                // (parseFieldDecl will check and complain if necessary)
                list = append(list, p.parseFieldDecl(scope));
            }

            var rbrace = p.expect(token.RBRACE); 

            // TODO(gri): store struct scope in AST
            return ref new ast.StructType(pos,&ast.FieldList{lbrace,list,rbrace},false);
        });

        private static ref ast.StarExpr parsePointerType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "PointerType")));
            }
            var star = p.expect(token.MUL);
            var @base = p.parseType();

            return ref new ast.StarExpr(star,base);
        });

        private static ast.Expr tryVarType(this ref parser p, bool isParam)
        {
            if (isParam && p.tok == token.ELLIPSIS)
            {
                var pos = p.pos;
                p.next();
                var typ = p.tryIdentOrType(isParam); // don't use parseType so we can provide better error message
                if (typ == null)
                {
                    p.error(pos, "'...' parameter is missing type");
                    typ = ref new ast.BadExpr(pos,p.pos);
                }
                if (p.tok != token.RPAREN)
                {
                    p.error(pos, "can use '...' with last parameter type only");
                }
                return ref new ast.Ellipsis(pos,typ);
            }
            return p.tryIdentOrType(false);
        }

        private static ast.Expr parseVarType(this ref parser p, bool isParam)
        {
            var typ = p.tryVarType(isParam);
            if (typ == null)
            {
                var pos = p.pos;
                p.errorExpected(pos, "type");
                p.next(); // make progress
                typ = ref new ast.BadExpr(pos,p.pos);
            }
            return typ;
        }

        private static (slice<ast.Expr>, ast.Expr) parseVarList(this ref parser _p, bool isParam) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "VarList")));
            } 

            // a list of identifiers looks like a list of type names
            while (true)
            { 
                // parseVarType accepts any type (including parenthesized ones)
                // even though the syntax does not permit them here: we
                // accept them all for more robust parsing and complain
                // afterwards
                list = append(list, p.parseVarType(isParam));
                if (p.tok != token.COMMA)
                {
                    break;
                }
                p.next();
            } 

            // if we had a list of identifiers, it must be followed by a type
 

            // if we had a list of identifiers, it must be followed by a type
            typ = p.tryVarType(isParam);
            if (typ != null)
            {
                p.resolve(typ);
            }
            return;
        });

        private static slice<ref ast.Field> parseParameterList(this ref parser _p, ref ast.Scope _scope, bool ellipsisOk) => func(_p, _scope, (ref parser p, ref ast.Scope scope, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "ParameterList")));
            }
            var (list, typ) = p.parseVarList(ellipsisOk);
            if (typ != null)
            { 
                // IdentifierList Type
                var idents = p.makeIdentList(list);
                ast.Field field = ref new ast.Field(nil,idents,typ,nil,nil);
                params = append(params, field); 
                // Go spec: The scope of an identifier denoting a function
                // parameter or result variable is the function body.
                p.declare(field, scope, ast.Var, idents);
                if (p.tok == token.COMMA)
                {
                    p.next();
                }
                while (p.tok != token.RPAREN && p.tok != token.EOF)
                {
                    idents = p.parseIdentList();
                    var typ = p.parseVarType(ellipsisOk);
                    field = ref new ast.Field(nil,idents,typ,nil,nil);
                    params = append(params, field); 
                    // Go spec: The scope of an identifier denoting a function
                    // parameter or result variable is the function body.
                    p.declare(field, scope, ast.Var, idents);
                    if (p.tok != token.COMMA)
                    {
                        break;
                    }
                    p.next();
                }
            else



            }            { 
                // Type { "," Type } (anonymous parameters)
                params = make_slice<ref ast.Field>(len(list));
                foreach (var (i, x) in list)
                {
                    p.resolve(x);
                    params[i] = ref new ast.Field(Type:x);
                }
            }
            return;
        });

        private static ref ast.FieldList parseParameters(this ref parser _p, ref ast.Scope _scope, bool ellipsisOk) => func(_p, _scope, (ref parser p, ref ast.Scope scope, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Parameters")));
            }
            slice<ref ast.Field> @params = default;
            var lparen = p.expect(token.LPAREN);
            if (p.tok != token.RPAREN)
            {
                params = p.parseParameterList(scope, ellipsisOk);
            }
            var rparen = p.expect(token.RPAREN);

            return ref new ast.FieldList(lparen,params,rparen);
        });

        private static ref ast.FieldList parseResult(this ref parser _p, ref ast.Scope _scope) => func(_p, _scope, (ref parser p, ref ast.Scope scope, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Result")));
            }
            if (p.tok == token.LPAREN)
            {
                return p.parseParameters(scope, false);
            }
            var typ = p.tryType();
            if (typ != null)
            {
                var list = make_slice<ref ast.Field>(1L);
                list[0L] = ref new ast.Field(Type:typ);
                return ref new ast.FieldList(List:list);
            }
            return null;
        });

        private static (ref ast.FieldList, ref ast.FieldList) parseSignature(this ref parser _p, ref ast.Scope _scope) => func(_p, _scope, (ref parser p, ref ast.Scope scope, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Signature")));
            }
            params = p.parseParameters(scope, true);
            results = p.parseResult(scope);

            return;
        });

        private static (ref ast.FuncType, ref ast.Scope) parseFuncType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "FuncType")));
            }
            var pos = p.expect(token.FUNC);
            var scope = ast.NewScope(p.topScope); // function scope
            var (params, results) = p.parseSignature(scope);

            return (ref new ast.FuncType(pos,params,results), scope);
        });

        private static ref ast.Field parseMethodSpec(this ref parser _p, ref ast.Scope _scope) => func(_p, _scope, (ref parser p, ref ast.Scope scope, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "MethodSpec")));
            }
            var doc = p.leadComment;
            slice<ref ast.Ident> idents = default;
            ast.Expr typ = default;
            var x = p.parseTypeName();
            {
                ref ast.Ident (ident, isIdent) = x._<ref ast.Ident>();

                if (isIdent && p.tok == token.LPAREN)
                { 
                    // method
                    idents = new slice<ref ast.Ident>(new ref ast.Ident[] { ident });
                    var scope = ast.NewScope(null); // method scope
                    var (params, results) = p.parseSignature(scope);
                    typ = ref new ast.FuncType(token.NoPos,params,results);
                }
                else
                { 
                    // embedded interface
                    typ = x;
                }

            }
            p.expectSemi(); // call before accessing p.linecomment

            ast.Field spec = ref new ast.Field(doc,idents,typ,nil,p.lineComment);
            p.declare(spec, scope, ast.Fun, idents);

            return spec;
        });

        private static ref ast.InterfaceType parseInterfaceType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "InterfaceType")));
            }
            var pos = p.expect(token.INTERFACE);
            var lbrace = p.expect(token.LBRACE);
            var scope = ast.NewScope(null); // interface scope
            slice<ref ast.Field> list = default;
            while (p.tok == token.IDENT)
            {
                list = append(list, p.parseMethodSpec(scope));
            }

            var rbrace = p.expect(token.RBRACE); 

            // TODO(gri): store interface scope in AST
            return ref new ast.InterfaceType(pos,&ast.FieldList{lbrace,list,rbrace},false);
        });

        private static ref ast.MapType parseMapType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "MapType")));
            }
            var pos = p.expect(token.MAP);
            p.expect(token.LBRACK);
            var key = p.parseType();
            p.expect(token.RBRACK);
            var value = p.parseType();

            return ref new ast.MapType(pos,key,value);
        });

        private static ref ast.ChanType parseChanType(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "ChanType")));
            }
            var pos = p.pos;
            var dir = ast.SEND | ast.RECV;
            if (p.tok == token.CHAN)
            {
                p.next();
                if (p.tok == token.ARROW)
                {
                    p.next();
                    dir = ast.SEND;
                }
            }
            else
            {
                p.expect(token.ARROW);
                p.expect(token.CHAN);
                dir = ast.RECV;
            }
            var value = p.parseType();

            return ref new ast.ChanType(pos,dir,value);
        });

        // If the result is an identifier, it is not resolved.
        private static ast.Expr tryIdentOrType(this ref parser p, bool ellipsisOk)
        {

            if (p.tok == token.IDENT) 
                return p.parseTypeName();
            else if (p.tok == token.LBRACK) 
                return p.parseArrayType(ellipsisOk);
            else if (p.tok == token.STRUCT) 
                return p.parseStructType();
            else if (p.tok == token.MUL) 
                return p.parsePointerType();
            else if (p.tok == token.FUNC) 
                var (typ, _) = p.parseFuncType();
                return typ;
            else if (p.tok == token.INTERFACE) 
                return p.parseInterfaceType();
            else if (p.tok == token.MAP) 
                return p.parseMapType();
            else if (p.tok == token.CHAN || p.tok == token.ARROW) 
                return p.parseChanType();
            else if (p.tok == token.LPAREN) 
                var lparen = p.pos;
                p.next();
                var typ = p.parseType();
                var rparen = p.expect(token.RPAREN);
                return ref new ast.ParenExpr(lparen,typ,rparen);
            // no type found
            return null;
        }

        private static ast.Expr tryType(this ref parser p)
        {
            var typ = p.tryIdentOrType(false);
            if (typ != null)
            {
                p.resolve(typ);
            }
            return typ;
        }

        // ----------------------------------------------------------------------------
        // Blocks

        private static slice<ast.Stmt> parseStmtList(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "StatementList")));
            }
            while (p.tok != token.CASE && p.tok != token.DEFAULT && p.tok != token.RBRACE && p.tok != token.EOF)
            {
                list = append(list, p.parseStmt());
            }


            return;
        });

        private static ref ast.BlockStmt parseBody(this ref parser _p, ref ast.Scope _scope) => func(_p, _scope, (ref parser p, ref ast.Scope scope, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Body")));
            }
            var lbrace = p.expect(token.LBRACE);
            p.topScope = scope; // open function scope
            p.openLabelScope();
            var list = p.parseStmtList();
            p.closeLabelScope();
            p.closeScope();
            var rbrace = p.expect(token.RBRACE);

            return ref new ast.BlockStmt(lbrace,list,rbrace);
        });

        private static ref ast.BlockStmt parseBlockStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "BlockStmt")));
            }
            var lbrace = p.expect(token.LBRACE);
            p.openScope();
            var list = p.parseStmtList();
            p.closeScope();
            var rbrace = p.expect(token.RBRACE);

            return ref new ast.BlockStmt(lbrace,list,rbrace);
        });

        // ----------------------------------------------------------------------------
        // Expressions

        private static ast.Expr parseFuncTypeOrLit(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "FuncTypeOrLit")));
            }
            var (typ, scope) = p.parseFuncType();
            if (p.tok != token.LBRACE)
            { 
                // function type only
                return typ;
            }
            p.exprLev++;
            var body = p.parseBody(scope);
            p.exprLev--;

            return ref new ast.FuncLit(typ,body);
        });

        // parseOperand may return an expression or a raw type (incl. array
        // types of the form [...]T. Callers must verify the result.
        // If lhs is set and the result is an identifier, it is not resolved.
        //
        private static ast.Expr parseOperand(this ref parser _p, bool lhs) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Operand")));
            }

            if (p.tok == token.IDENT) 
                var x = p.parseIdent();
                if (!lhs)
                {
                    p.resolve(x);
                }
                return x;
            else if (p.tok == token.INT || p.tok == token.FLOAT || p.tok == token.IMAG || p.tok == token.CHAR || p.tok == token.STRING) 
                x = ref new ast.BasicLit(p.pos,p.tok,p.lit);
                p.next();
                return x;
            else if (p.tok == token.LPAREN) 
                var lparen = p.pos;
                p.next();
                p.exprLev++;
                x = p.parseRhs();
                p.exprLev--;
                var rparen = p.expect(token.RPAREN);
                return ref new ast.ParenExpr(lparen,x,rparen);
            else if (p.tok == token.FUNC) 
                return p.parseFuncTypeOrLit();
            else 
                {
                    var typ = p.tryIdentOrType(true);

                    if (typ != null)
                    { 
                        // could be type for composite literal or conversion
                        ref ast.Ident (_, isIdent) = typ._<ref ast.Ident>();
                        assert(!isIdent, "type cannot be identifier");
                        return typ;
                    }

                }
                        var pos = p.pos;
            p.errorExpected(pos, "operand");
            p.next(); // make progress
            return ref new ast.BadExpr(pos,p.pos);
        });

        private static ast.Expr parseSelector(this ref parser _p, ast.Expr x) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Selector")));
            }
            var sel = p.parseIdent();

            return ref new ast.SelectorExpr(x,sel);
        });

        private static ast.Expr parseTypeAssertion(this ref parser _p, ast.Expr x) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "TypeAssertion")));
            }
            p.expect(token.LPAREN);
            ast.Expr typ = default;
            if (p.tok == token.TYPE)
            { 
                // type switch: typ == nil
                p.next();
            }
            else
            {
                typ = p.parseType();
            }
            p.expect(token.RPAREN);

            return ref new ast.TypeAssertExpr(x,typ);
        });

        private static ast.Expr parseIndexOrSlice(this ref parser _p, ast.Expr x) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "IndexOrSlice")));
            }
            var lbrack = p.expect(token.LBRACK);
            p.exprLev++;
            ast.Expr low = default;            ast.Expr high = default;

            var isSlice = false;
            if (p.tok != token.COLON)
            {
                low = p.parseRhs();
            }
            if (p.tok == token.COLON)
            {
                isSlice = true;
                p.next();
                if (p.tok != token.RBRACK)
                {
                    high = p.parseRhs();
                }
            }
            p.exprLev--;
            var rbrack = p.expect(token.RBRACK);

            if (isSlice)
            {
                return ref new ast.SliceExpr(x,lbrack,low,high,rbrack);
            }
            return ref new ast.IndexExpr(x,lbrack,low,rbrack);
        });

        private static ref ast.CallExpr parseCallOrConversion(this ref parser _p, ast.Expr fun) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "CallOrConversion")));
            }
            var lparen = p.expect(token.LPAREN);
            p.exprLev++;
            slice<ast.Expr> list = default;
            token.Pos ellipsis = default;
            while (p.tok != token.RPAREN && p.tok != token.EOF && !ellipsis.IsValid())
            {
                list = append(list, p.parseRhs());
                if (p.tok == token.ELLIPSIS)
                {
                    ellipsis = p.pos;
                    p.next();
                }
                if (p.tok != token.COMMA)
                {
                    break;
                }
                p.next();
            }

            p.exprLev--;
            var rparen = p.expect(token.RPAREN);

            return ref new ast.CallExpr(fun,lparen,list,ellipsis,rparen);
        });

        private static ast.Expr parseElement(this ref parser _p, bool keyOk) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Element")));
            }
            if (p.tok == token.LBRACE)
            {
                return p.parseLiteralValue(null);
            }
            var x = p.parseExpr(keyOk); // don't resolve if map key
            if (keyOk)
            {
                if (p.tok == token.COLON)
                {
                    var colon = p.pos;
                    p.next();
                    return ref new ast.KeyValueExpr(x,colon,p.parseElement(false));
                }
                p.resolve(x); // not a map key
            }
            return x;
        });

        private static slice<ast.Expr> parseElementList(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "ElementList")));
            }
            while (p.tok != token.RBRACE && p.tok != token.EOF)
            {
                list = append(list, p.parseElement(true));
                if (p.tok != token.COMMA)
                {
                    break;
                }
                p.next();
            }


            return;
        });

        private static ast.Expr parseLiteralValue(this ref parser _p, ast.Expr typ) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "LiteralValue")));
            }
            var lbrace = p.expect(token.LBRACE);
            slice<ast.Expr> elts = default;
            p.exprLev++;
            if (p.tok != token.RBRACE)
            {
                elts = p.parseElementList();
            }
            p.exprLev--;
            var rbrace = p.expect(token.RBRACE);
            return ref new ast.CompositeLit(typ,lbrace,elts,rbrace);
        });

        // checkExpr checks that x is an expression (and not a type).
        private static ast.Expr checkExpr(this ref parser _p, ast.Expr x) => func(_p, (ref parser p, Defer _, Panic panic, Recover __) =>
        {
            switch (unparen(x).type())
            {
                case ref ast.BadExpr t:
                    break;
                case ref ast.Ident t:
                    break;
                case ref ast.BasicLit t:
                    break;
                case ref ast.FuncLit t:
                    break;
                case ref ast.CompositeLit t:
                    break;
                case ref ast.ParenExpr t:
                    panic("unreachable");
                    break;
                case ref ast.SelectorExpr t:
                    break;
                case ref ast.IndexExpr t:
                    break;
                case ref ast.SliceExpr t:
                    break;
                case ref ast.TypeAssertExpr t:
                    if (t.Type == null)
                    { 
                        // the form X.(type) is only allowed in type switch expressions
                        p.errorExpected(x.Pos(), "expression");
                        x = ref new ast.BadExpr(x.Pos(),x.End());
                    }
                    break;
                case ref ast.CallExpr t:
                    break;
                case ref ast.StarExpr t:
                    break;
                case ref ast.UnaryExpr t:
                    if (t.Op == token.RANGE)
                    { 
                        // the range operator is only allowed at the top of a for statement
                        p.errorExpected(x.Pos(), "expression");
                        x = ref new ast.BadExpr(x.Pos(),x.End());
                    }
                    break;
                case ref ast.BinaryExpr t:
                    break;
                default:
                {
                    var t = unparen(x).type();
                    p.errorExpected(x.Pos(), "expression");
                    x = ref new ast.BadExpr(x.Pos(),x.End());
                    break;
                }
            }
            return x;
        });

        // isTypeName reports whether x is a (qualified) TypeName.
        private static bool isTypeName(ast.Expr x)
        {
            switch (x.type())
            {
                case ref ast.BadExpr t:
                    break;
                case ref ast.Ident t:
                    break;
                case ref ast.SelectorExpr t:
                    ref ast.Ident (_, isIdent) = t.X._<ref ast.Ident>();
                    return isIdent;
                    break;
                default:
                {
                    var t = x.type();
                    return false; // all other nodes are not type names
                    break;
                }
            }
            return true;
        }

        // isLiteralType reports whether x is a legal composite literal type.
        private static bool isLiteralType(ast.Expr x)
        {
            switch (x.type())
            {
                case ref ast.BadExpr t:
                    break;
                case ref ast.Ident t:
                    break;
                case ref ast.SelectorExpr t:
                    ref ast.Ident (_, isIdent) = t.X._<ref ast.Ident>();
                    return isIdent;
                    break;
                case ref ast.ArrayType t:
                    break;
                case ref ast.StructType t:
                    break;
                case ref ast.MapType t:
                    break;
                default:
                {
                    var t = x.type();
                    return false; // all other nodes are not legal composite literal types
                    break;
                }
            }
            return true;
        }

        // If x is of the form *T, deref returns T, otherwise it returns x.
        private static ast.Expr deref(ast.Expr x)
        {
            {
                ref ast.StarExpr (p, isPtr) = x._<ref ast.StarExpr>();

                if (isPtr)
                {
                    x = p.X;
                }

            }
            return x;
        }

        // If x is of the form (T), unparen returns unparen(T), otherwise it returns x.
        private static ast.Expr unparen(ast.Expr x)
        {
            {
                ref ast.ParenExpr (p, isParen) = x._<ref ast.ParenExpr>();

                if (isParen)
                {
                    x = unparen(p.X);
                }

            }
            return x;
        }

        // checkExprOrType checks that x is an expression or a type
        // (and not a raw type such as [...]T).
        //
        private static ast.Expr checkExprOrType(this ref parser _p, ast.Expr x) => func(_p, (ref parser p, Defer _, Panic panic, Recover __) =>
        {
            switch (unparen(x).type())
            {
                case ref ast.ParenExpr t:
                    panic("unreachable");
                    break;
                case ref ast.UnaryExpr t:
                    if (t.Op == token.RANGE)
                    { 
                        // the range operator is only allowed at the top of a for statement
                        p.errorExpected(x.Pos(), "expression");
                        x = ref new ast.BadExpr(x.Pos(),x.End());
                    }
                    break;
                case ref ast.ArrayType t:
                    {
                        ref ast.Ellipsis (len, isEllipsis) = t.Len._<ref ast.Ellipsis>();

                        if (isEllipsis)
                        {
                            p.error(len.Pos(), "expected array length, found '...'");
                            x = ref new ast.BadExpr(x.Pos(),x.End());
                        }

                    }
                    break; 

                // all other nodes are expressions or types
            } 

            // all other nodes are expressions or types
            return x;
        });

        // If lhs is set and the result is an identifier, it is not resolved.
        private static ast.Expr parsePrimaryExpr(this ref parser _p, bool lhs) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "PrimaryExpr")));
            }
            var x = p.parseOperand(lhs);
L:

            while (true)
            {

                if (p.tok == token.PERIOD) 
                    p.next();
                    if (lhs)
                    {
                        p.resolve(x);
                    }

                    if (p.tok == token.IDENT) 
                        x = p.parseSelector(p.checkExpr(x));
                    else if (p.tok == token.LPAREN) 
                        x = p.parseTypeAssertion(p.checkExpr(x));
                    else 
                        var pos = p.pos;
                        p.next(); // make progress
                        p.errorExpected(pos, "selector or type assertion");
                        x = ref new ast.BadExpr(pos,p.pos);
                                    else if (p.tok == token.LBRACK) 
                    if (lhs)
                    {
                        p.resolve(x);
                    }
                    x = p.parseIndexOrSlice(p.checkExpr(x));
                else if (p.tok == token.LPAREN) 
                    if (lhs)
                    {
                        p.resolve(x);
                    }
                    x = p.parseCallOrConversion(p.checkExprOrType(x));
                else if (p.tok == token.LBRACE) 
                    if (isLiteralType(x) && (p.exprLev >= 0L || !isTypeName(x)))
                    {
                        if (lhs)
                        {
                            p.resolve(x);
                        }
                        x = p.parseLiteralValue(x);
                    }
                    else
                    {
                        _breakL = true;
                        break;
                    }
                else 
                    _breakL = true;
                    break;
                                lhs = false; // no need to try to resolve again
            }

            return x;
        });

        // If lhs is set and the result is an identifier, it is not resolved.
        private static ast.Expr parseUnaryExpr(this ref parser _p, bool lhs) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "UnaryExpr")));
            }

            if (p.tok == token.ADD || p.tok == token.SUB || p.tok == token.NOT || p.tok == token.XOR || p.tok == token.AND || p.tok == token.RANGE) 
                var pos = p.pos;
                var op = p.tok;
                p.next();
                var x = p.parseUnaryExpr(false);
                return ref new ast.UnaryExpr(pos,op,p.checkExpr(x));
            else if (p.tok == token.ARROW) 
                // channel type or receive expression
                pos = p.pos;
                p.next();
                if (p.tok == token.CHAN)
                {
                    p.next();
                    var value = p.parseType();
                    return ref new ast.ChanType(pos,ast.RECV,value);
                }
                x = p.parseUnaryExpr(false);
                return ref new ast.UnaryExpr(pos,token.ARROW,p.checkExpr(x));
            else if (p.tok == token.MUL) 
                // pointer type or unary "*" expression
                pos = p.pos;
                p.next();
                x = p.parseUnaryExpr(false);
                return ref new ast.StarExpr(pos,p.checkExprOrType(x));
                        return p.parsePrimaryExpr(lhs);
        });

        // If lhs is set and the result is an identifier, it is not resolved.
        private static ast.Expr parseBinaryExpr(this ref parser _p, bool lhs, long prec1) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "BinaryExpr")));
            }
            var x = p.parseUnaryExpr(lhs);
            for (var prec = p.tok.Precedence(); prec >= prec1; prec--)
            {
                while (p.tok.Precedence() == prec)
                {
                    var pos = p.pos;
                    var op = p.tok;
                    p.next();
                    if (lhs)
                    {
                        p.resolve(x);
                        lhs = false;
                    }
                    var y = p.parseBinaryExpr(false, prec + 1L);
                    x = ref new ast.BinaryExpr(p.checkExpr(x),pos,op,p.checkExpr(y));
                }

            }


            return x;
        });

        // If lhs is set and the result is an identifier, it is not resolved.
        // TODO(gri): parseExpr may return a type or even a raw type ([..]int) -
        //            should reject when a type/raw type is obviously not allowed
        private static ast.Expr parseExpr(this ref parser _p, bool lhs) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Expression")));
            }
            return p.parseBinaryExpr(lhs, token.LowestPrec + 1L);
        });

        private static ast.Expr parseRhs(this ref parser p)
        {
            return p.parseExpr(false);
        }

        // ----------------------------------------------------------------------------
        // Statements

        private static ast.Stmt parseSimpleStmt(this ref parser _p, bool labelOk) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "SimpleStmt")));
            }
            var x = p.parseLhsList();


            if (p.tok == token.DEFINE || p.tok == token.ASSIGN || p.tok == token.ADD_ASSIGN || p.tok == token.SUB_ASSIGN || p.tok == token.MUL_ASSIGN || p.tok == token.QUO_ASSIGN || p.tok == token.REM_ASSIGN || p.tok == token.AND_ASSIGN || p.tok == token.OR_ASSIGN || p.tok == token.XOR_ASSIGN || p.tok == token.SHL_ASSIGN || p.tok == token.SHR_ASSIGN || p.tok == token.AND_NOT_ASSIGN) 
                // assignment statement
                var pos = p.pos;
                var tok = p.tok;
                p.next();
                var y = p.parseRhsList();
                return ref new ast.AssignStmt(x,pos,tok,y);
                        if (len(x) > 1L)
            {
                p.errorExpected(x[0L].Pos(), "1 expression"); 
                // continue with first expression
            }

            if (p.tok == token.COLON) 
                // labeled statement
                var colon = p.pos;
                p.next();
                {
                    ref ast.Ident (label, isIdent) = x[0L]._<ref ast.Ident>();

                    if (labelOk && isIdent)
                    { 
                        // Go spec: The scope of a label is the body of the function
                        // in which it is declared and excludes the body of any nested
                        // function.
                        ast.LabeledStmt stmt = ref new ast.LabeledStmt(label,colon,p.parseStmt());
                        p.declare(stmt, p.labelScope, ast.Lbl, label);
                        return stmt;
                    }

                }
                p.error(x[0L].Pos(), "illegal label declaration");
                return ref new ast.BadStmt(x[0].Pos(),colon+1);
            else if (p.tok == token.ARROW) 
                // send statement
                var arrow = p.pos;
                p.next(); // consume "<-"
                y = p.parseRhs();
                return ref new ast.SendStmt(x[0],arrow,y);
            else if (p.tok == token.INC || p.tok == token.DEC) 
                // increment or decrement
                ast.IncDecStmt s = ref new ast.IncDecStmt(x[0],p.pos,p.tok);
                p.next(); // consume "++" or "--"
                return s;
            // expression
            return ref new ast.ExprStmt(x[0]);
        });

        private static ref ast.CallExpr parseCallExpr(this ref parser p)
        {
            var x = p.parseRhs();
            {
                ref ast.CallExpr (call, isCall) = x._<ref ast.CallExpr>();

                if (isCall)
                {
                    return call;
                }

            }
            p.errorExpected(x.Pos(), "function/method call");
            return null;
        }

        private static ast.Stmt parseGoStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "GoStmt")));
            }
            var pos = p.expect(token.GO);
            var call = p.parseCallExpr();
            p.expectSemi();
            if (call == null)
            {
                return ref new ast.BadStmt(pos,pos+2); // len("go")
            }
            return ref new ast.GoStmt(pos,call);
        });

        private static ast.Stmt parseDeferStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "DeferStmt")));
            }
            var pos = p.expect(token.DEFER);
            var call = p.parseCallExpr();
            p.expectSemi();
            if (call == null)
            {
                return ref new ast.BadStmt(pos,pos+5); // len("defer")
            }
            return ref new ast.DeferStmt(pos,call);
        });

        private static ref ast.ReturnStmt parseReturnStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "ReturnStmt")));
            }
            var pos = p.pos;
            p.expect(token.RETURN);
            slice<ast.Expr> x = default;
            if (p.tok != token.SEMICOLON && p.tok != token.RBRACE)
            {
                x = p.parseRhsList();
            }
            p.expectSemi();

            return ref new ast.ReturnStmt(pos,x);
        });

        private static ref ast.BranchStmt parseBranchStmt(this ref parser _p, token.Token tok) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "BranchStmt")));
            }
            var pos = p.expect(tok);
            ref ast.Ident label = default;
            if (tok != token.FALLTHROUGH && p.tok == token.IDENT)
            {
                label = p.parseIdent(); 
                // add to list of unresolved targets
                var n = len(p.targetStack) - 1L;
                p.targetStack[n] = append(p.targetStack[n], label);
            }
            p.expectSemi();

            return ref new ast.BranchStmt(pos,tok,label);
        });

        private static ast.Expr makeExpr(this ref parser p, ast.Stmt s)
        {
            if (s == null)
            {
                return null;
            }
            {
                ref ast.ExprStmt (es, isExpr) = s._<ref ast.ExprStmt>();

                if (isExpr)
                {
                    return p.checkExpr(es.X);
                }

            }
            p.error(s.Pos(), "expected condition, found simple statement");
            return ref new ast.BadExpr(s.Pos(),s.End());
        }

        private static ref ast.IfStmt parseIfStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "IfStmt")));
            }
            var pos = p.expect(token.IF);
            p.openScope();
            defer(p.closeScope());

            ast.Stmt s = default;
            ast.Expr x = default;
            {
                var prevLev = p.exprLev;
                p.exprLev = -1L;
                if (p.tok == token.SEMICOLON)
                {
                    p.next();
                    x = p.parseRhs();
                }
                else
                {
                    s = p.parseSimpleStmt(false);
                    if (p.tok == token.SEMICOLON)
                    {
                        p.next();
                        x = p.parseRhs();
                    }
                    else
                    {
                        x = p.makeExpr(s);
                        s = null;
                    }
                }
                p.exprLev = prevLev;
            }
            var body = p.parseBlockStmt();
            ast.Stmt else_ = default;
            if (p.tok == token.ELSE)
            {
                p.next();
                else_ = p.parseStmt();
            }
            else
            {
                p.expectSemi();
            }
            return ref new ast.IfStmt(pos,s,x,body,else_);
        });

        private static slice<ast.Expr> parseTypeList(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "TypeList")));
            }
            list = append(list, p.parseType());
            while (p.tok == token.COMMA)
            {
                p.next();
                list = append(list, p.parseType());
            }


            return;
        });

        private static ref ast.CaseClause parseCaseClause(this ref parser _p, bool exprSwitch) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "CaseClause")));
            }
            var pos = p.pos;
            slice<ast.Expr> list = default;
            if (p.tok == token.CASE)
            {
                p.next();
                if (exprSwitch)
                {
                    list = p.parseRhsList();
                }
                else
                {
                    list = p.parseTypeList();
                }
            }
            else
            {
                p.expect(token.DEFAULT);
            }
            var colon = p.expect(token.COLON);
            p.openScope();
            var body = p.parseStmtList();
            p.closeScope();

            return ref new ast.CaseClause(pos,list,colon,body);
        });

        private static bool isExprSwitch(ast.Stmt s)
        {
            if (s == null)
            {
                return true;
            }
            {
                ref ast.ExprStmt (e, ok) = s._<ref ast.ExprStmt>();

                if (ok)
                {
                    {
                        ref ast.TypeAssertExpr (a, ok) = e.X._<ref ast.TypeAssertExpr>();

                        if (ok)
                        {
                            return a.Type != null; // regular type assertion
                        }

                    }
                    return true;
                }

            }
            return false;
        }

        private static ast.Stmt parseSwitchStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "SwitchStmt")));
            }
            var pos = p.expect(token.SWITCH);
            p.openScope();
            defer(p.closeScope());

            ast.Stmt s1 = default;            ast.Stmt s2 = default;

            if (p.tok != token.LBRACE)
            {
                var prevLev = p.exprLev;
                p.exprLev = -1L;
                if (p.tok != token.SEMICOLON)
                {
                    s2 = p.parseSimpleStmt(false);
                }
                if (p.tok == token.SEMICOLON)
                {
                    p.next();
                    s1 = s2;
                    s2 = null;
                    if (p.tok != token.LBRACE)
                    {
                        s2 = p.parseSimpleStmt(false);
                    }
                }
                p.exprLev = prevLev;
            }
            var exprSwitch = isExprSwitch(s2);
            var lbrace = p.expect(token.LBRACE);
            slice<ast.Stmt> list = default;
            while (p.tok == token.CASE || p.tok == token.DEFAULT)
            {
                list = append(list, p.parseCaseClause(exprSwitch));
            }

            var rbrace = p.expect(token.RBRACE);
            p.expectSemi();
            ast.BlockStmt body = ref new ast.BlockStmt(lbrace,list,rbrace);

            if (exprSwitch)
            {
                return ref new ast.SwitchStmt(pos,s1,p.makeExpr(s2),body);
            } 
            // type switch
            // TODO(gri): do all the checks!
            return ref new ast.TypeSwitchStmt(pos,s1,s2,body);
        });

        private static ref ast.CommClause parseCommClause(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "CommClause")));
            }
            p.openScope();
            var pos = p.pos;
            ast.Stmt comm = default;
            if (p.tok == token.CASE)
            {
                p.next();
                var lhs = p.parseLhsList();
                if (p.tok == token.ARROW)
                { 
                    // SendStmt
                    if (len(lhs) > 1L)
                    {
                        p.errorExpected(lhs[0L].Pos(), "1 expression"); 
                        // continue with first expression
                    }
                    var arrow = p.pos;
                    p.next();
                    var rhs = p.parseRhs();
                    comm = ref new ast.SendStmt(lhs[0],arrow,rhs);
                }
                else
                { 
                    // RecvStmt
                    pos = p.pos;
                    var tok = p.tok;
                    rhs = default;
                    if (tok == token.ASSIGN || tok == token.DEFINE)
                    { 
                        // RecvStmt with assignment
                        if (len(lhs) > 2L)
                        {
                            p.errorExpected(lhs[0L].Pos(), "1 or 2 expressions"); 
                            // continue with first two expressions
                            lhs = lhs[0L..2L];
                        }
                        p.next();
                        rhs = p.parseRhs();
                    }
                    else
                    { 
                        // rhs must be single receive operation
                        if (len(lhs) > 1L)
                        {
                            p.errorExpected(lhs[0L].Pos(), "1 expression"); 
                            // continue with first expression
                        }
                        rhs = lhs[0L];
                        lhs = null; // there is no lhs
                    }
                    {
                        ref ast.UnaryExpr (x, isUnary) = rhs._<ref ast.UnaryExpr>();

                        if (!isUnary || x.Op != token.ARROW)
                        {
                            p.errorExpected(rhs.Pos(), "send or receive operation");
                            rhs = ref new ast.BadExpr(rhs.Pos(),rhs.End());
                        }

                    }
                    if (lhs != null)
                    {
                        comm = ref new ast.AssignStmt(lhs,pos,tok,[]ast.Expr{rhs});
                    }
                    else
                    {
                        comm = ref new ast.ExprStmt(rhs);
                    }
                }
            }
            else
            {
                p.expect(token.DEFAULT);
            }
            var colon = p.expect(token.COLON);
            var body = p.parseStmtList();
            p.closeScope();

            return ref new ast.CommClause(pos,comm,colon,body);
        });

        private static ref ast.SelectStmt parseSelectStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "SelectStmt")));
            }
            var pos = p.expect(token.SELECT);
            var lbrace = p.expect(token.LBRACE);
            slice<ast.Stmt> list = default;
            while (p.tok == token.CASE || p.tok == token.DEFAULT)
            {
                list = append(list, p.parseCommClause());
            }

            var rbrace = p.expect(token.RBRACE);
            p.expectSemi();
            ast.BlockStmt body = ref new ast.BlockStmt(lbrace,list,rbrace);

            return ref new ast.SelectStmt(pos,body);
        });

        private static ast.Stmt parseForStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "ForStmt")));
            }
            var pos = p.expect(token.FOR);
            p.openScope();
            defer(p.closeScope());

            ast.Stmt s1 = default;            ast.Stmt s2 = default;            ast.Stmt s3 = default;

            if (p.tok != token.LBRACE)
            {
                var prevLev = p.exprLev;
                p.exprLev = -1L;
                if (p.tok != token.SEMICOLON)
                {
                    s2 = p.parseSimpleStmt(false);
                }
                if (p.tok == token.SEMICOLON)
                {
                    p.next();
                    s1 = s2;
                    s2 = null;
                    if (p.tok != token.SEMICOLON)
                    {
                        s2 = p.parseSimpleStmt(false);
                    }
                    p.expectSemi();
                    if (p.tok != token.LBRACE)
                    {
                        s3 = p.parseSimpleStmt(false);
                    }
                }
                p.exprLev = prevLev;
            }
            var body = p.parseBlockStmt();
            p.expectSemi();

            {
                ref ast.AssignStmt (as, isAssign) = s2._<ref ast.AssignStmt>();

                if (isAssign)
                { 
                    // possibly a for statement with a range clause; check assignment operator
                    if (@as.Tok != token.ASSIGN && @as.Tok != token.DEFINE)
                    {
                        p.errorExpected(@as.TokPos, "'=' or ':='");
                        return ref new ast.BadStmt(pos,body.End());
                    } 
                    // check lhs
                    ast.Expr key = default;                    ast.Expr value = default;

                    switch (len(@as.Lhs))
                    {
                        case 2L: 
                            key = @as.Lhs[0L];
                            value = @as.Lhs[1L];
                            break;
                        case 1L: 
                            key = @as.Lhs[0L];
                            break;
                        default: 
                            p.errorExpected(@as.Lhs[0L].Pos(), "1 or 2 expressions");
                            return ref new ast.BadStmt(pos,body.End());
                            break;
                    } 
                    // check rhs
                    if (len(@as.Rhs) != 1L)
                    {
                        p.errorExpected(@as.Rhs[0L].Pos(), "1 expression");
                        return ref new ast.BadStmt(pos,body.End());
                    }
                    {
                        ref ast.UnaryExpr (rhs, isUnary) = @as.Rhs[0L]._<ref ast.UnaryExpr>();

                        if (isUnary && rhs.Op == token.RANGE)
                        { 
                            // rhs is range expression
                            // (any short variable declaration was handled by parseSimpleStat above)
                            return ref new ast.RangeStmt(pos,key,value,as.TokPos,as.Tok,rhs.X,body);
                        }

                    }
                    p.errorExpected(s2.Pos(), "range clause");
                    return ref new ast.BadStmt(pos,body.End());
                } 

                // regular for statement

            } 

            // regular for statement
            return ref new ast.ForStmt(pos,s1,p.makeExpr(s2),s3,body);
        });

        private static ast.Stmt parseStmt(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Statement")));
            }

            if (p.tok == token.CONST || p.tok == token.TYPE || p.tok == token.VAR) 
                s = ref new ast.DeclStmt(p.parseDecl());
            else if (p.tok == token.IDENT || p.tok == token.INT || p.tok == token.FLOAT || p.tok == token.CHAR || p.tok == token.STRING || p.tok == token.FUNC || p.tok == token.LPAREN || p.tok == token.LBRACK || p.tok == token.STRUCT || p.tok == token.MUL || p.tok == token.AND || p.tok == token.ARROW || p.tok == token.ADD || p.tok == token.SUB || p.tok == token.XOR) // unary operators
                s = p.parseSimpleStmt(true); 
                // because of the required look-ahead, labeled statements are
                // parsed by parseSimpleStmt - don't expect a semicolon after
                // them
                {
                    ref ast.LabeledStmt (_, isLabeledStmt) = s._<ref ast.LabeledStmt>();

                    if (!isLabeledStmt)
                    {
                        p.expectSemi();
                    }

                }
            else if (p.tok == token.GO) 
                s = p.parseGoStmt();
            else if (p.tok == token.DEFER) 
                s = p.parseDeferStmt();
            else if (p.tok == token.RETURN) 
                s = p.parseReturnStmt();
            else if (p.tok == token.BREAK || p.tok == token.CONTINUE || p.tok == token.GOTO || p.tok == token.FALLTHROUGH) 
                s = p.parseBranchStmt(p.tok);
            else if (p.tok == token.LBRACE) 
                s = p.parseBlockStmt();
                p.expectSemi();
            else if (p.tok == token.IF) 
                s = p.parseIfStmt();
            else if (p.tok == token.SWITCH) 
                s = p.parseSwitchStmt();
            else if (p.tok == token.SELECT) 
                s = p.parseSelectStmt();
            else if (p.tok == token.FOR) 
                s = p.parseForStmt();
            else if (p.tok == token.SEMICOLON) 
                s = ref new ast.EmptyStmt(p.pos);
                p.next();
            else if (p.tok == token.RBRACE) 
                // a semicolon may be omitted before a closing "}"
                s = ref new ast.EmptyStmt(p.pos);
            else 
                // no statement found
                var pos = p.pos;
                p.errorExpected(pos, "statement");
                p.next(); // make progress
                s = ref new ast.BadStmt(pos,p.pos);
                        return;
        });

        // ----------------------------------------------------------------------------
        // Declarations

        public delegate  ast.Spec parseSpecFunction(ref parser,  ref ast.CommentGroup,  long);

        private static ast.Spec parseImportSpec(ref parser _p, ref ast.CommentGroup _doc, long _) => func(_p, _doc, (ref parser p, ref ast.CommentGroup doc, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "ImportSpec")));
            }
            ref ast.Ident ident = default;

            if (p.tok == token.PERIOD) 
                ident = ref new ast.Ident(p.pos,".",nil);
                p.next();
            else if (p.tok == token.IDENT) 
                ident = p.parseIdent();
                        ref ast.BasicLit path = default;
            if (p.tok == token.STRING)
            {
                path = ref new ast.BasicLit(p.pos,p.tok,p.lit);
                p.next();
            }
            else
            {
                p.expect(token.STRING); // use expect() error handling
            }
            p.expectSemi(); // call before accessing p.linecomment

            // collect imports
            ast.ImportSpec spec = ref new ast.ImportSpec(doc,ident,path,p.lineComment);
            p.imports = append(p.imports, spec);

            return spec;
        });

        private static ast.Spec parseConstSpec(ref parser _p, ref ast.CommentGroup _doc, long iota) => func(_p, _doc, (ref parser p, ref ast.CommentGroup doc, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "ConstSpec")));
            }
            var idents = p.parseIdentList();
            var typ = p.tryType();
            slice<ast.Expr> values = default;
            if (typ != null || p.tok == token.ASSIGN || iota == 0L)
            {
                p.expect(token.ASSIGN);
                values = p.parseRhsList();
            }
            p.expectSemi(); // call before accessing p.linecomment

            // Go spec: The scope of a constant or variable identifier declared inside
            // a function begins at the end of the ConstSpec or VarSpec and ends at
            // the end of the innermost containing block.
            // (Global identifiers are resolved in a separate phase after parsing.)
            ast.ValueSpec spec = ref new ast.ValueSpec(doc,idents,typ,values,p.lineComment);
            p.declare(spec, p.topScope, ast.Con, idents);

            return spec;
        });

        private static ast.Spec parseTypeSpec(ref parser _p, ref ast.CommentGroup _doc, long _) => func(_p, _doc, (ref parser p, ref ast.CommentGroup doc, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "TypeSpec")));
            }
            var ident = p.parseIdent(); 

            // Go spec: The scope of a type identifier declared inside a function begins
            // at the identifier in the TypeSpec and ends at the end of the innermost
            // containing block.
            // (Global identifiers are resolved in a separate phase after parsing.)
            ast.TypeSpec spec = ref new ast.TypeSpec(doc,ident,nil,nil);
            p.declare(spec, p.topScope, ast.Typ, ident);

            spec.Type = p.parseType();
            p.expectSemi(); // call before accessing p.linecomment
            spec.Comment = p.lineComment;

            return spec;
        });

        private static ast.Spec parseVarSpec(ref parser _p, ref ast.CommentGroup _doc, long _) => func(_p, _doc, (ref parser p, ref ast.CommentGroup doc, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "VarSpec")));
            }
            var idents = p.parseIdentList();
            var typ = p.tryType();
            slice<ast.Expr> values = default;
            if (typ == null || p.tok == token.ASSIGN)
            {
                p.expect(token.ASSIGN);
                values = p.parseRhsList();
            }
            p.expectSemi(); // call before accessing p.linecomment

            // Go spec: The scope of a constant or variable identifier declared inside
            // a function begins at the end of the ConstSpec or VarSpec and ends at
            // the end of the innermost containing block.
            // (Global identifiers are resolved in a separate phase after parsing.)
            ast.ValueSpec spec = ref new ast.ValueSpec(doc,idents,typ,values,p.lineComment);
            p.declare(spec, p.topScope, ast.Var, idents);

            return spec;
        });

        private static ref ast.GenDecl parseGenDecl(this ref parser _p, token.Token keyword, parseSpecFunction f) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "GenDecl(" + keyword.String() + ")")));
            }
            var doc = p.leadComment;
            var pos = p.expect(keyword);
            token.Pos lparen = default;            token.Pos rparen = default;

            slice<ast.Spec> list = default;
            if (p.tok == token.LPAREN)
            {
                lparen = p.pos;
                p.next();
                for (long iota = 0L; p.tok != token.RPAREN && p.tok != token.EOF; iota++)
                {
                    list = append(list, f(p, p.leadComment, iota));
                }
            else

                rparen = p.expect(token.RPAREN);
                p.expectSemi();
            }            {
                list = append(list, f(p, null, 0L));
            }
            return ref new ast.GenDecl(doc,pos,keyword,lparen,list,rparen);
        });

        private static ref ast.FieldList parseReceiver(this ref parser _p, ref ast.Scope _scope) => func(_p, _scope, (ref parser p, ref ast.Scope scope, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Receiver")));
            }
            var pos = p.pos;
            var par = p.parseParameters(scope, false); 

            // must have exactly one receiver
            if (par.NumFields() != 1L)
            {
                p.errorExpected(pos, "exactly one receiver"); 
                // TODO determine a better range for BadExpr below
                par.List = new slice<ref ast.Field>(new ref ast.Field[] { {Type:&ast.BadExpr{pos,pos}} });
                return par;
            } 

            // recv type must be of the form ["*"] identifier
            var recv = par.List[0L];
            var @base = deref(recv.Type);
            {
                ref ast.Ident (_, isIdent) = base._<ref ast.Ident>();

                if (!isIdent)
                {
                    p.errorExpected(@base.Pos(), "(unqualified) identifier");
                    par.List = new slice<ref ast.Field>(new ref ast.Field[] { {Type:&ast.BadExpr{recv.Pos(),recv.End()}} });
                }

            }

            return par;
        });

        private static ref ast.FuncDecl parseFuncDecl(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "FunctionDecl")));
            }
            var doc = p.leadComment;
            var pos = p.expect(token.FUNC);
            var scope = ast.NewScope(p.topScope); // function scope

            ref ast.FieldList recv = default;
            if (p.tok == token.LPAREN)
            {
                recv = p.parseReceiver(scope);
            }
            var ident = p.parseIdent();

            var (params, results) = p.parseSignature(scope);

            ref ast.BlockStmt body = default;
            if (p.tok == token.LBRACE)
            {
                body = p.parseBody(scope);
            }
            p.expectSemi();

            ast.FuncDecl decl = ref new ast.FuncDecl(doc,recv,ident,&ast.FuncType{pos,params,results},body);
            if (recv == null)
            { 
                // Go spec: The scope of an identifier denoting a constant, type,
                // variable, or function (but not method) declared at top level
                // (outside any function) is the package block.
                //
                // init() functions cannot be referred to and there may
                // be more than one - don't put them in the pkgScope
                if (ident.Name != "init")
                {
                    p.declare(decl, p.pkgScope, ast.Fun, ident);
                }
            }
            return decl;
        });

        private static ast.Decl parseDecl(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "Declaration")));
            }
            parseSpecFunction f = default;

            if (p.tok == token.CONST) 
                f = parseConstSpec;
            else if (p.tok == token.TYPE) 
                f = parseTypeSpec;
            else if (p.tok == token.VAR) 
                f = parseVarSpec;
            else if (p.tok == token.FUNC) 
                return p.parseFuncDecl();
            else 
                var pos = p.pos;
                p.errorExpected(pos, "declaration");
                p.next(); // make progress
                ast.BadDecl decl = ref new ast.BadDecl(pos,p.pos);
                return decl;
                        return p.parseGenDecl(p.tok, f);
        });

        private static slice<ast.Decl> parseDeclList(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "DeclList")));
            }
            while (p.tok != token.EOF)
            {
                list = append(list, p.parseDecl());
            }


            return;
        });

        // ----------------------------------------------------------------------------
        // Source files

        private static ref ast.File parseFile(this ref parser _p) => func(_p, (ref parser p, Defer defer, Panic _, Recover __) =>
        {
            if (p.trace)
            {
                defer(un(trace(p, "File")));
            } 

            // package clause
            var doc = p.leadComment;
            var pos = p.expect(token.PACKAGE); 
            // Go spec: The package clause is not a declaration;
            // the package name does not appear in any scope.
            var ident = p.parseIdent();
            if (ident.Name == "_")
            {
                p.error(p.pos, "invalid package name _");
            }
            p.expectSemi();

            slice<ast.Decl> decls = default; 

            // Don't bother parsing the rest if we had errors already.
            // Likely not a Go source file at all.

            if (p.ErrorCount() == 0L && p.mode & PackageClauseOnly == 0L)
            { 
                // import decls
                while (p.tok == token.IMPORT)
                {
                    decls = append(decls, p.parseGenDecl(token.IMPORT, parseImportSpec));
                }


                if (p.mode & ImportsOnly == 0L)
                { 
                    // rest of package body
                    while (p.tok != token.EOF)
                    {
                        decls = append(decls, p.parseDecl());
                    }

                }
            }
            assert(p.topScope == p.pkgScope, "imbalanced scopes"); 

            // resolve global identifiers within the same file
            long i = 0L;
            {
                var ident__prev1 = ident;

                foreach (var (_, __ident) in p.unresolved)
                {
                    ident = __ident; 
                    // i <= index for current ident
                    assert(ident.Obj == unresolved, "object already resolved");
                    ident.Obj = p.pkgScope.Lookup(ident.Name); // also removes unresolved sentinel
                    if (ident.Obj == null)
                    {
                        p.unresolved[i] = ident;
                        i++;
                    }
                } 

                // TODO(gri): store p.imports in AST

                ident = ident__prev1;
            }

            return ref new ast.File(doc,pos,ident,decls,p.pkgScope,p.imports,p.unresolved[0:i],p.comments);
        });
    }
}}
