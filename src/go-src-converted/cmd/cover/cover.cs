// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:44:21 UTC
// Original source: C:\Go\src\cmd\cover\cover.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using sort = go.sort_package;

using edit = go.cmd.@internal.edit_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static readonly @string usageMessage = (@string)"" + @"Usage of 'go tool cover':
Given a coverage profile produced by 'go test':
	go test -coverprofile=c.out

Open a web browser displaying annotated source code:
	go tool cover -html=c.out

Write out an HTML file instead of launching a web browser:
	go tool cover -html=c.out -o coverage.html

Display coverage percentages to stdout for each function:
	go tool cover -func=c.out

Finally, to generate modified source code with coverage annotations
(what go test -cover does):
	go tool cover -mode=set -var=CoverageVariableName program.go
";



        private static void usage()
        {
            fmt.Fprintln(os.Stderr, usageMessage);
            fmt.Fprintln(os.Stderr, "Flags:");
            flag.PrintDefaults();
            fmt.Fprintln(os.Stderr, "\n  Only one of -html, -func, or -mode may be set.");
            os.Exit(2L);
        }

        private static var mode = flag.String("mode", "", "coverage mode: set, count, atomic");        private static var varVar = flag.String("var", "GoCover", "name of coverage variable to generate");        private static var output = flag.String("o", "", "file for output; default: stdout");        private static var htmlOut = flag.String("html", "", "generate HTML representation of coverage profile");        private static var funcOut = flag.String("func", "", "output coverage profile information for each function");

        private static @string profile = default; // The profile to read; the value of -html or -func

        private static Func<ptr<File>, @string, @string> counterStmt = default;

        private static readonly @string atomicPackagePath = (@string)"sync/atomic";
        private static readonly @string atomicPackageName = (@string)"_cover_atomic_";


        private static void Main()
        {
            objabi.AddVersionFlag();
            flag.Usage = usage;
            flag.Parse(); 

            // Usage information when no arguments.
            if (flag.NFlag() == 0L && flag.NArg() == 0L)
            {
                flag.Usage();
            }

            var err = parseFlags();
            if (err != null)
            {
                fmt.Fprintln(os.Stderr, err);
                fmt.Fprintln(os.Stderr, "For usage information, run \"go tool cover -help\"");
                os.Exit(2L);
            } 

            // Generate coverage-annotated source.
            if (mode != "".val)
            {
                annotate(flag.Arg(0L));
                return ;
            } 

            // Output HTML or function coverage information.
            if (htmlOut != "".val)
            {
                err = htmlOutput(profile, output.val);
            }
            else
            {
                err = funcOutput(profile, output.val);
            }

            if (err != null)
            {
                fmt.Fprintf(os.Stderr, "cover: %v\n", err);
                os.Exit(2L);
            }

        }

        // parseFlags sets the profile and counterStmt globals and performs validations.
        private static error parseFlags()
        {
            profile = htmlOut.val;
            if (funcOut != "".val)
            {
                if (profile != "")
                {
                    return error.As(fmt.Errorf("too many options"))!;
                }

                profile = funcOut.val;

            } 

            // Must either display a profile or rewrite Go source.
            if ((profile == "") == (mode == "".val))
            {
                return error.As(fmt.Errorf("too many options"))!;
            }

            if (varVar != "" && !token.IsIdentifier(varVar.val).val)
            {
                return error.As(fmt.Errorf("-var: %q is not a valid identifier", varVar.val))!;
            }

            if (mode != "".val)
            {
                switch (mode.val)
                {
                    case "set": 
                        counterStmt = setCounterStmt;
                        break;
                    case "count": 
                        counterStmt = incCounterStmt;
                        break;
                    case "atomic": 
                        counterStmt = atomicCounterStmt;
                        break;
                    default: 
                        return error.As(fmt.Errorf("unknown -mode %v", mode.val))!;
                        break;
                }

                if (flag.NArg() == 0L)
                {
                    return error.As(fmt.Errorf("missing source file"))!;
                }
                else if (flag.NArg() == 1L)
                {
                    return error.As(null!)!;
                }

            }
            else if (flag.NArg() == 0L)
            {
                return error.As(null!)!;
            }

            return error.As(fmt.Errorf("too many arguments"))!;

        }

        // Block represents the information about a basic block to be recorded in the analysis.
        // Note: Our definition of basic block is based on control structures; we don't break
        // apart && and ||. We could but it doesn't seem important enough to bother.
        public partial struct Block
        {
            public token.Pos startByte;
            public token.Pos endByte;
            public long numStmt;
        }

        // File is a wrapper for the state of a file used in the parser.
        // The basic parse tree walker is a method of this type.
        public partial struct File
        {
            public ptr<token.FileSet> fset;
            public @string name; // Name of file.
            public ptr<ast.File> astFile;
            public slice<Block> blocks;
            public slice<byte> content;
            public ptr<edit.Buffer> edit;
        }

        // findText finds text in the original source, starting at pos.
        // It correctly skips over comments and assumes it need not
        // handle quoted strings.
        // It returns a byte offset within f.src.
        private static long findText(this ptr<File> _addr_f, token.Pos pos, @string text)
        {
            ref File f = ref _addr_f.val;

            slice<byte> b = (slice<byte>)text;
            var start = f.offset(pos);
            var i = start;
            var s = f.content;
            while (i < len(s))
            {
                if (bytes.HasPrefix(s[i..], b))
                {
                    return i;
                }

                if (i + 2L <= len(s) && s[i] == '/' && s[i + 1L] == '/')
                {
                    while (i < len(s) && s[i] != '\n')
                    {
                        i++;
                    }

                    continue;

                }

                if (i + 2L <= len(s) && s[i] == '/' && s[i + 1L] == '*')
                {
                    for (i += 2L; >>MARKER:FOREXPRESSION_LEVEL_2<<; i++)
                    {
                        if (i + 2L > len(s))
                        {
                            return 0L;
                        }

                        if (s[i] == '*' && s[i + 1L] == '/')
                        {
                            i += 2L;
                            break;
                        }

                    }

                    continue;

                }

                i++;

            }

            return -1L;

        }

        // Visit implements the ast.Visitor interface.
        private static ast.Visitor Visit(this ptr<File> _addr_f, ast.Node node) => func((_, panic, __) =>
        {
            ref File f = ref _addr_f.val;

            switch (node.type())
            {
                case ptr<ast.BlockStmt> n:
                    if (len(n.List) > 0L)
                    {
                        switch (n.List[0L].type())
                        {
                            case ptr<ast.CaseClause> _:
                                {
                                    var n__prev1 = n;

                                    foreach (var (_, __n) in n.List)
                                    {
                                        n = __n;
                                        ptr<ast.CaseClause> clause = n._<ptr<ast.CaseClause>>();
                                        f.addCounters(clause.Colon + 1L, clause.Colon + 1L, clause.End(), clause.Body, false);
                                    }

                                    n = n__prev1;
                                }

                                return f;
                                break;
                            case ptr<ast.CommClause> _:
                                {
                                    var n__prev1 = n;

                                    foreach (var (_, __n) in n.List)
                                    {
                                        n = __n;
                                        clause = n._<ptr<ast.CommClause>>();
                                        f.addCounters(clause.Colon + 1L, clause.Colon + 1L, clause.End(), clause.Body, false);
                                    }

                                    n = n__prev1;
                                }

                                return f;
                                break;
                        }

                    }

                    f.addCounters(n.Lbrace, n.Lbrace + 1L, n.Rbrace + 1L, n.List, true); // +1 to step past closing brace.
                    break;
                case ptr<ast.IfStmt> n:
                    if (n.Init != null)
                    {
                        ast.Walk(f, n.Init);
                    }

                    ast.Walk(f, n.Cond);
                    ast.Walk(f, n.Body);
                    if (n.Else == null)
                    {
                        return null;
                    } 
                    // The elses are special, because if we have
                    //    if x {
                    //    } else if y {
                    //    }
                    // we want to cover the "if y". To do this, we need a place to drop the counter,
                    // so we add a hidden block:
                    //    if x {
                    //    } else {
                    //        if y {
                    //        }
                    //    }
                    var elseOffset = f.findText(n.Body.End(), "else");
                    if (elseOffset < 0L)
                    {
                        panic("lost else");
                    }

                    f.edit.Insert(elseOffset + 4L, "{");
                    f.edit.Insert(f.offset(n.Else.End()), "}"); 

                    // We just created a block, now walk it.
                    // Adjust the position of the new block to start after
                    // the "else". That will cause it to follow the "{"
                    // we inserted above.
                    var pos = f.fset.File(n.Body.End()).Pos(elseOffset + 4L);
                    switch (n.Else.type())
                    {
                        case ptr<ast.IfStmt> stmt:
                            ptr<ast.BlockStmt> block = addr(new ast.BlockStmt(Lbrace:pos,List:[]ast.Stmt{stmt},Rbrace:stmt.End(),));
                            n.Else = block;
                            break;
                        case ptr<ast.BlockStmt> stmt:
                            stmt.Lbrace = pos;
                            break;
                        default:
                        {
                            var stmt = n.Else.type();
                            panic("unexpected node type in if");
                            break;
                        }
                    }
                    ast.Walk(f, n.Else);
                    return null;
                    break;
                case ptr<ast.SelectStmt> n:
                    if (n.Body == null || len(n.Body.List) == 0L)
                    {
                        return null;
                    }

                    break;
                case ptr<ast.SwitchStmt> n:
                    if (n.Body == null || len(n.Body.List) == 0L)
                    {
                        if (n.Init != null)
                        {
                            ast.Walk(f, n.Init);
                        }

                        if (n.Tag != null)
                        {
                            ast.Walk(f, n.Tag);
                        }

                        return null;

                    }

                    break;
                case ptr<ast.TypeSwitchStmt> n:
                    if (n.Body == null || len(n.Body.List) == 0L)
                    {
                        if (n.Init != null)
                        {
                            ast.Walk(f, n.Init);
                        }

                        ast.Walk(f, n.Assign);
                        return null;

                    }

                    break;
                case ptr<ast.FuncDecl> n:
                    if (n.Name.Name == "_")
                    {
                        return null;
                    }

                    break;
            }
            return f;

        });

        private static void annotate(@string name)
        {
            var fset = token.NewFileSet();
            var (content, err) = ioutil.ReadFile(name);
            if (err != null)
            {
                log.Fatalf("cover: %s: %s", name, err);
            }

            var (parsedFile, err) = parser.ParseFile(fset, name, content, parser.ParseComments);
            if (err != null)
            {
                log.Fatalf("cover: %s: %s", name, err);
            }

            ptr<File> file = addr(new File(fset:fset,name:name,content:content,edit:edit.NewBuffer(content),astFile:parsedFile,));
            if (mode == "atomic".val)
            { 
                // Add import of sync/atomic immediately after package clause.
                // We do this even if there is an existing import, because the
                // existing import may be shadowed at any given place we want
                // to refer to it, and our name (_cover_atomic_) is less likely to
                // be shadowed.
                file.edit.Insert(file.offset(file.astFile.Name.End()), fmt.Sprintf("; import %s %q", atomicPackageName, atomicPackagePath));

            }

            ast.Walk(file, file.astFile);
            var newContent = file.edit.Bytes();

            var fd = os.Stdout;
            if (output != "".val)
            {
                error err = default!;
                fd, err = os.Create(output.val);
                if (err != null)
                {
                    log.Fatalf("cover: %s", err);
                }

            }

            fmt.Fprintf(fd, "//line %s:1\n", name);
            fd.Write(newContent); 

            // After printing the source tree, add some declarations for the counters etc.
            // We could do this by adding to the tree, but it's easier just to print the text.
            file.addVariables(fd);

        }

        // setCounterStmt returns the expression: __count[23] = 1.
        private static @string setCounterStmt(ptr<File> _addr_f, @string counter)
        {
            ref File f = ref _addr_f.val;

            return fmt.Sprintf("%s = 1", counter);
        }

        // incCounterStmt returns the expression: __count[23]++.
        private static @string incCounterStmt(ptr<File> _addr_f, @string counter)
        {
            ref File f = ref _addr_f.val;

            return fmt.Sprintf("%s++", counter);
        }

        // atomicCounterStmt returns the expression: atomic.AddUint32(&__count[23], 1)
        private static @string atomicCounterStmt(ptr<File> _addr_f, @string counter)
        {
            ref File f = ref _addr_f.val;

            return fmt.Sprintf("%s.AddUint32(&%s, 1)", atomicPackageName, counter);
        }

        // newCounter creates a new counter expression of the appropriate form.
        private static @string newCounter(this ptr<File> _addr_f, token.Pos start, token.Pos end, long numStmt)
        {
            ref File f = ref _addr_f.val;

            var stmt = counterStmt(f, fmt.Sprintf("%s.Count[%d]", varVar.val, len(f.blocks)));
            f.blocks = append(f.blocks, new Block(start,end,numStmt));
            return stmt;
        }

        // addCounters takes a list of statements and adds counters to the beginning of
        // each basic block at the top level of that list. For instance, given
        //
        //    S1
        //    if cond {
        //        S2
        //     }
        //    S3
        //
        // counters will be added before S1 and before S3. The block containing S2
        // will be visited in a separate call.
        // TODO: Nested simple blocks get unnecessary (but correct) counters
        private static void addCounters(this ptr<File> _addr_f, token.Pos pos, token.Pos insertPos, token.Pos blockEnd, slice<ast.Stmt> list, bool extendToClosingBrace)
        {
            ref File f = ref _addr_f.val;
 
            // Special case: make sure we add a counter to an empty block. Can't do this below
            // or we will add a counter to an empty statement list after, say, a return statement.
            if (len(list) == 0L)
            {
                f.edit.Insert(f.offset(insertPos), f.newCounter(insertPos, blockEnd, 0L) + ";");
                return ;
            } 
            // Make a copy of the list, as we may mutate it and should leave the
            // existing list intact.
            list = append((slice<ast.Stmt>)null, list); 
            // We have a block (statement list), but it may have several basic blocks due to the
            // appearance of statements that affect the flow of control.
            while (true)
            { 
                // Find first statement that affects flow of control (break, continue, if, etc.).
                // It will be the last statement of this basic block.
                long last = default;
                var end = blockEnd;
                for (last = 0L; last < len(list); last++)
                {
                    var stmt = list[last];
                    end = f.statementBoundary(stmt);
                    if (f.endsBasicSourceBlock(stmt))
                    { 
                        // If it is a labeled statement, we need to place a counter between
                        // the label and its statement because it may be the target of a goto
                        // and thus start a basic block. That is, given
                        //    foo: stmt
                        // we need to create
                        //    foo: ; stmt
                        // and mark the label as a block-terminating statement.
                        // The result will then be
                        //    foo: COUNTER[n]++; stmt
                        // However, we can't do this if the labeled statement is already
                        // a control statement, such as a labeled for.
                        {
                            ptr<ast.LabeledStmt> (label, isLabel) = stmt._<ptr<ast.LabeledStmt>>();

                            if (isLabel && !f.isControl(label.Stmt))
                            {
                                ref var newLabel = ref heap(label.val, out ptr<var> _addr_newLabel);
                                newLabel.Stmt = addr(new ast.EmptyStmt(Semicolon:label.Stmt.Pos(),Implicit:true,));
                                end = label.Pos(); // Previous block ends before the label.
                                _addr_list[last] = _addr_newLabel;
                                list[last] = ref _addr_list[last].val; 
                                // Open a gap and drop in the old statement, now without a label.
                                list = append(list, null);
                                copy(list[last + 1L..], list[last..]);
                                list[last + 1L] = label.Stmt;

                            }

                        }

                        last++;
                        extendToClosingBrace = false; // Block is broken up now.
                        break;

                    }

                }

                if (extendToClosingBrace)
                {
                    end = blockEnd;
                }

                if (pos != end)
                { // Can have no source to cover if e.g. blocks abut.
                    f.edit.Insert(f.offset(insertPos), f.newCounter(pos, end, last) + ";");

                }

                list = list[last..];
                if (len(list) == 0L)
                {
                    break;
                }

                pos = list[0L].Pos();
                insertPos = pos;

            }


        }

        // hasFuncLiteral reports the existence and position of the first func literal
        // in the node, if any. If a func literal appears, it usually marks the termination
        // of a basic block because the function body is itself a block.
        // Therefore we draw a line at the start of the body of the first function literal we find.
        // TODO: what if there's more than one? Probably doesn't matter much.
        private static (bool, token.Pos) hasFuncLiteral(ast.Node n)
        {
            bool _p0 = default;
            token.Pos _p0 = default;

            if (n == null)
            {
                return (false, 0L);
            }

            ref funcLitFinder literal = ref heap(out ptr<funcLitFinder> _addr_literal);
            ast.Walk(_addr_literal, n);
            return (literal.found(), token.Pos(literal));

        }

        // statementBoundary finds the location in s that terminates the current basic
        // block in the source.
        private static token.Pos statementBoundary(this ptr<File> _addr_f, ast.Stmt s)
        {
            ref File f = ref _addr_f.val;
 
            // Control flow statements are easy.
            switch (s.type())
            {
                case ptr<ast.BlockStmt> s:
                    return s.Lbrace;
                    break;
                case ptr<ast.IfStmt> s:
                    var (found, pos) = hasFuncLiteral(s.Init);
                    if (found)
                    {
                        return pos;
                    }

                    found, pos = hasFuncLiteral(s.Cond);
                    if (found)
                    {
                        return pos;
                    }

                    return s.Body.Lbrace;
                    break;
                case ptr<ast.ForStmt> s:
                    (found, pos) = hasFuncLiteral(s.Init);
                    if (found)
                    {
                        return pos;
                    }

                    found, pos = hasFuncLiteral(s.Cond);
                    if (found)
                    {
                        return pos;
                    }

                    found, pos = hasFuncLiteral(s.Post);
                    if (found)
                    {
                        return pos;
                    }

                    return s.Body.Lbrace;
                    break;
                case ptr<ast.LabeledStmt> s:
                    return f.statementBoundary(s.Stmt);
                    break;
                case ptr<ast.RangeStmt> s:
                    (found, pos) = hasFuncLiteral(s.X);
                    if (found)
                    {
                        return pos;
                    }

                    return s.Body.Lbrace;
                    break;
                case ptr<ast.SwitchStmt> s:
                    (found, pos) = hasFuncLiteral(s.Init);
                    if (found)
                    {
                        return pos;
                    }

                    found, pos = hasFuncLiteral(s.Tag);
                    if (found)
                    {
                        return pos;
                    }

                    return s.Body.Lbrace;
                    break;
                case ptr<ast.SelectStmt> s:
                    return s.Body.Lbrace;
                    break;
                case ptr<ast.TypeSwitchStmt> s:
                    (found, pos) = hasFuncLiteral(s.Init);
                    if (found)
                    {
                        return pos;
                    }

                    return s.Body.Lbrace;
                    break; 
                // If not a control flow statement, it is a declaration, expression, call, etc. and it may have a function literal.
                // If it does, that's tricky because we want to exclude the body of the function from this block.
                // Draw a line at the start of the body of the first function literal we find.
                // TODO: what if there's more than one? Probably doesn't matter much.
            } 
            // If not a control flow statement, it is a declaration, expression, call, etc. and it may have a function literal.
            // If it does, that's tricky because we want to exclude the body of the function from this block.
            // Draw a line at the start of the body of the first function literal we find.
            // TODO: what if there's more than one? Probably doesn't matter much.
            (found, pos) = hasFuncLiteral(s);
            if (found)
            {
                return pos;
            }

            return s.End();

        }

        // endsBasicSourceBlock reports whether s changes the flow of control: break, if, etc.,
        // or if it's just problematic, for instance contains a function literal, which will complicate
        // accounting due to the block-within-an expression.
        private static bool endsBasicSourceBlock(this ptr<File> _addr_f, ast.Stmt s)
        {
            ref File f = ref _addr_f.val;

            switch (s.type())
            {
                case ptr<ast.BlockStmt> s:
                    return true;
                    break;
                case ptr<ast.BranchStmt> s:
                    return true;
                    break;
                case ptr<ast.ForStmt> s:
                    return true;
                    break;
                case ptr<ast.IfStmt> s:
                    return true;
                    break;
                case ptr<ast.LabeledStmt> s:
                    return true; // A goto may branch here, starting a new basic block.
                    break;
                case ptr<ast.RangeStmt> s:
                    return true;
                    break;
                case ptr<ast.SwitchStmt> s:
                    return true;
                    break;
                case ptr<ast.SelectStmt> s:
                    return true;
                    break;
                case ptr<ast.TypeSwitchStmt> s:
                    return true;
                    break;
                case ptr<ast.ExprStmt> s:
                    {
                        ptr<ast.CallExpr> (call, ok) = s.X._<ptr<ast.CallExpr>>();

                        if (ok)
                        {
                            {
                                ptr<ast.Ident> (ident, ok) = call.Fun._<ptr<ast.Ident>>();

                                if (ok && ident.Name == "panic" && len(call.Args) == 1L)
                                {
                                    return true;
                                }

                            }

                        }

                    }

                    break;
            }
            var (found, _) = hasFuncLiteral(s);
            return found;

        }

        // isControl reports whether s is a control statement that, if labeled, cannot be
        // separated from its label.
        private static bool isControl(this ptr<File> _addr_f, ast.Stmt s)
        {
            ref File f = ref _addr_f.val;

            switch (s.type())
            {
                case ptr<ast.ForStmt> _:
                    return true;
                    break;
                case ptr<ast.RangeStmt> _:
                    return true;
                    break;
                case ptr<ast.SwitchStmt> _:
                    return true;
                    break;
                case ptr<ast.SelectStmt> _:
                    return true;
                    break;
                case ptr<ast.TypeSwitchStmt> _:
                    return true;
                    break;
            }
            return false;

        }

        // funcLitFinder implements the ast.Visitor pattern to find the location of any
        // function literal in a subtree.
        private partial struct funcLitFinder // : token.Pos
        {
        }

        private static ast.Visitor Visit(this ptr<funcLitFinder> _addr_f, ast.Node node)
        {
            ast.Visitor w = default;
            ref funcLitFinder f = ref _addr_f.val;

            if (f.found())
            {
                return null; // Prune search.
            }

            switch (node.type())
            {
                case ptr<ast.FuncLit> n:
                    f.val = funcLitFinder(n.Body.Lbrace);
                    return null; // Prune search.
                    break;
            }
            return f;

        }

        private static bool found(this ptr<funcLitFinder> _addr_f)
        {
            ref funcLitFinder f = ref _addr_f.val;

            return token.Pos(f.val) != token.NoPos;
        }

        // Sort interface for []block1; used for self-check in addVariables.

        private partial struct block1
        {
            public ref Block Block => ref Block_val;
            public long index;
        }

        private partial struct blockSlice // : slice<block1>
        {
        }

        private static long Len(this blockSlice b)
        {
            return len(b);
        }
        private static bool Less(this blockSlice b, long i, long j)
        {
            return b[i].startByte < b[j].startByte;
        }
        private static void Swap(this blockSlice b, long i, long j)
        {
            b[i] = b[j];
            b[j] = b[i];
        }

        // offset translates a token position into a 0-indexed byte offset.
        private static long offset(this ptr<File> _addr_f, token.Pos pos)
        {
            ref File f = ref _addr_f.val;

            return f.fset.Position(pos).Offset;
        }

        // addVariables adds to the end of the file the declarations to set up the counter and position variables.
        private static void addVariables(this ptr<File> _addr_f, io.Writer w)
        {
            ref File f = ref _addr_f.val;
 
            // Self-check: Verify that the instrumented basic blocks are disjoint.
            var t = make_slice<block1>(len(f.blocks));
            {
                var i__prev1 = i;

                foreach (var (__i) in f.blocks)
                {
                    i = __i;
                    t[i].Block = f.blocks[i];
                    t[i].index = i;
                }

                i = i__prev1;
            }

            sort.Sort(blockSlice(t));
            {
                var i__prev1 = i;

                for (long i = 1L; i < len(t); i++)
                {
                    if (t[i - 1L].endByte > t[i].startByte)
                    {
                        fmt.Fprintf(os.Stderr, "cover: internal error: block %d overlaps block %d\n", t[i - 1L].index, t[i].index); 
                        // Note: error message is in byte positions, not token positions.
                        fmt.Fprintf(os.Stderr, "\t%s:#%d,#%d %s:#%d,#%d\n", f.name, f.offset(t[i - 1L].startByte), f.offset(t[i - 1L].endByte), f.name, f.offset(t[i].startByte), f.offset(t[i].endByte));

                    }

                } 

                // Declare the coverage struct as a package-level variable.


                i = i__prev1;
            } 

            // Declare the coverage struct as a package-level variable.
            fmt.Fprintf(w, "\nvar %s = struct {\n", varVar.val);
            fmt.Fprintf(w, "\tCount     [%d]uint32\n", len(f.blocks));
            fmt.Fprintf(w, "\tPos       [3 * %d]uint32\n", len(f.blocks));
            fmt.Fprintf(w, "\tNumStmt   [%d]uint16\n", len(f.blocks));
            fmt.Fprintf(w, "} {\n"); 

            // Initialize the position array field.
            fmt.Fprintf(w, "\tPos: [3 * %d]uint32{\n", len(f.blocks)); 

            // A nice long list of positions. Each position is encoded as follows to reduce size:
            // - 32-bit starting line number
            // - 32-bit ending line number
            // - (16 bit ending column number << 16) | (16-bit starting column number).
            {
                var i__prev1 = i;
                var block__prev1 = block;

                foreach (var (__i, __block) in f.blocks)
                {
                    i = __i;
                    block = __block;
                    var start = f.fset.Position(block.startByte);
                    var end = f.fset.Position(block.endByte);

                    start, end = dedup(start, end);

                    fmt.Fprintf(w, "\t\t%d, %d, %#x, // [%d]\n", start.Line, end.Line, (end.Column & 0xFFFFUL) << (int)(16L) | (start.Column & 0xFFFFUL), i);
                } 

                // Close the position array.

                i = i__prev1;
                block = block__prev1;
            }

            fmt.Fprintf(w, "\t},\n"); 

            // Initialize the position array field.
            fmt.Fprintf(w, "\tNumStmt: [%d]uint16{\n", len(f.blocks)); 

            // A nice long list of statements-per-block, so we can give a conventional
            // valuation of "percent covered". To save space, it's a 16-bit number, so we
            // clamp it if it overflows - won't matter in practice.
            {
                var i__prev1 = i;
                var block__prev1 = block;

                foreach (var (__i, __block) in f.blocks)
                {
                    i = __i;
                    block = __block;
                    var n = block.numStmt;
                    if (n > 1L << (int)(16L) - 1L)
                    {
                        n = 1L << (int)(16L) - 1L;
                    }

                    fmt.Fprintf(w, "\t\t%d, // %d\n", n, i);

                } 

                // Close the statements-per-block array.

                i = i__prev1;
                block = block__prev1;
            }

            fmt.Fprintf(w, "\t},\n"); 

            // Close the struct initialization.
            fmt.Fprintf(w, "}\n"); 

            // Emit a reference to the atomic package to avoid
            // import and not used error when there's no code in a file.
            if (mode == "atomic".val)
            {
                fmt.Fprintf(w, "var _ = %s.LoadUint32\n", atomicPackageName);
            }

        }

        // It is possible for positions to repeat when there is a line
        // directive that does not specify column information and the input
        // has not been passed through gofmt.
        // See issues #27530 and #30746.
        // Tests are TestHtmlUnformatted and TestLineDup.
        // We use a map to avoid duplicates.

        // pos2 is a pair of token.Position values, used as a map key type.
        private partial struct pos2
        {
            public token.Position p1;
            public token.Position p2;
        }

        // seenPos2 tracks whether we have seen a token.Position pair.
        private static var seenPos2 = make_map<pos2, bool>();

        // dedup takes a token.Position pair and returns a pair that does not
        // duplicate any existing pair. The returned pair will have the Offset
        // fields cleared.
        private static (token.Position, token.Position) dedup(token.Position p1, token.Position p2)
        {
            token.Position r1 = default;
            token.Position r2 = default;

            pos2 key = new pos2(p1:p1,p2:p2,); 

            // We want to ignore the Offset fields in the map,
            // since cover uses only file/line/column.
            key.p1.Offset = 0L;
            key.p2.Offset = 0L;

            while (seenPos2[key])
            {
                key.p2.Column++;
            }

            seenPos2[key] = true;

            return (key.p1, key.p2);

        }
    }
}
