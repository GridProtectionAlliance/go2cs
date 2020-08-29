// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 09:59:26 UTC
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
using strconv = go.strconv_package;

using edit = go.cmd.@internal.edit_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static readonly @string usageMessage = "" + @"Usage of 'go tool cover':
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

        private static Func<ref File, @string, @string> counterStmt = default;

        private static readonly @string atomicPackagePath = "sync/atomic";
        private static readonly @string atomicPackageName = "_cover_atomic_";

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
            if (mode != "".Value)
            {
                annotate(flag.Arg(0L));
                return;
            } 

            // Output HTML or function coverage information.
            if (htmlOut != "".Value)
            {
                err = htmlOutput(profile, output.Value);
            }
            else
            {
                err = funcOutput(profile, output.Value);
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
            profile = htmlOut.Value;
            if (funcOut != "".Value)
            {
                if (profile != "")
                {
                    return error.As(fmt.Errorf("too many options"));
                }
                profile = funcOut.Value;
            } 

            // Must either display a profile or rewrite Go source.
            if ((profile == "") == (mode == "".Value))
            {
                return error.As(fmt.Errorf("too many options"));
            }
            if (mode != "".Value)
            {
                switch (mode.Value)
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
                        return error.As(fmt.Errorf("unknown -mode %v", mode.Value));
                        break;
                }

                if (flag.NArg() == 0L)
                {
                    return error.As(fmt.Errorf("missing source file"));
                }
                else if (flag.NArg() == 1L)
                {
                    return error.As(null);
                }
            }
            else if (flag.NArg() == 0L)
            {
                return error.As(null);
            }
            return error.As(fmt.Errorf("too many arguments"));
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
        private static long findText(this ref File f, token.Pos pos, @string text)
        {
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
        private static ast.Visitor Visit(this ref File _f, ast.Node node) => func(_f, (ref File f, Defer _, Panic panic, Recover __) =>
        {
            switch (node.type())
            {
                case ref ast.BlockStmt n:
                    if (len(n.List) > 0L)
                    {
                        switch (n.List[0L].type())
                        {
                            case ref ast.CaseClause _:
                                {
                                    var n__prev1 = n;

                                    foreach (var (_, __n) in n.List)
                                    {
                                        n = __n;
                                        ref ast.CaseClause clause = n._<ref ast.CaseClause>();
                                        f.addCounters(clause.Colon + 1L, clause.Colon + 1L, clause.End(), clause.Body, false);
                                    }

                                    n = n__prev1;
                                }

                                return f;
                                break;
                            case ref ast.CommClause _:
                                {
                                    var n__prev1 = n;

                                    foreach (var (_, __n) in n.List)
                                    {
                                        n = __n;
                                        clause = n._<ref ast.CommClause>();
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
                case ref ast.IfStmt n:
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
                        case ref ast.IfStmt stmt:
                            ast.BlockStmt block = ref new ast.BlockStmt(Lbrace:pos,List:[]ast.Stmt{stmt},Rbrace:stmt.End(),);
                            n.Else = block;
                            break;
                        case ref ast.BlockStmt stmt:
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
                case ref ast.SelectStmt n:
                    if (n.Body == null || len(n.Body.List) == 0L)
                    {
                        return null;
                    }
                    break;
                case ref ast.SwitchStmt n:
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
                case ref ast.TypeSwitchStmt n:
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
            }
            return f;
        });

        // unquote returns the unquoted string.
        private static @string unquote(@string s)
        {
            var (t, err) = strconv.Unquote(s);
            if (err != null)
            {
                log.Fatalf("cover: improperly quoted string %q\n", s);
            }
            return t;
        }

        private static slice<byte> slashslash = (slice<byte>)"//";

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
            File file = ref new File(fset:fset,name:name,content:content,edit:edit.NewBuffer(content),astFile:parsedFile,);
            if (mode == "atomic".Value)
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
            if (output != "".Value)
            {
                error err = default;
                fd, err = os.Create(output.Value);
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
        private static @string setCounterStmt(ref File f, @string counter)
        {
            return fmt.Sprintf("%s = 1", counter);
        }

        // incCounterStmt returns the expression: __count[23]++.
        private static @string incCounterStmt(ref File f, @string counter)
        {
            return fmt.Sprintf("%s++", counter);
        }

        // atomicCounterStmt returns the expression: atomic.AddUint32(&__count[23], 1)
        private static @string atomicCounterStmt(ref File f, @string counter)
        {
            return fmt.Sprintf("%s.AddUint32(&%s, 1)", atomicPackageName, counter);
        }

        // newCounter creates a new counter expression of the appropriate form.
        private static @string newCounter(this ref File f, token.Pos start, token.Pos end, long numStmt)
        {
            var stmt = counterStmt(f, fmt.Sprintf("%s.Count[%d]", varVar.Value, len(f.blocks)));
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
        private static void addCounters(this ref File f, token.Pos pos, token.Pos insertPos, token.Pos blockEnd, slice<ast.Stmt> list, bool extendToClosingBrace)
        { 
            // Special case: make sure we add a counter to an empty block. Can't do this below
            // or we will add a counter to an empty statement list after, say, a return statement.
            if (len(list) == 0L)
            {
                f.edit.Insert(f.offset(insertPos), f.newCounter(insertPos, blockEnd, 0L) + ";");
                return;
            } 
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
                            ref ast.LabeledStmt (label, isLabel) = stmt._<ref ast.LabeledStmt>();

                            if (isLabel && !f.isControl(label.Stmt))
                            {
                                var newLabel = label.Value;
                                newLabel.Stmt = ref new ast.EmptyStmt(Semicolon:label.Stmt.Pos(),Implicit:true,);
                                end = label.Pos(); // Previous block ends before the label.
                                list[last] = ref newLabel; 
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
            if (n == null)
            {
                return (false, 0L);
            }
            funcLitFinder literal = default;
            ast.Walk(ref literal, n);
            return (literal.found(), token.Pos(literal));
        }

        // statementBoundary finds the location in s that terminates the current basic
        // block in the source.
        private static token.Pos statementBoundary(this ref File f, ast.Stmt s)
        { 
            // Control flow statements are easy.
            switch (s.type())
            {
                case ref ast.BlockStmt s:
                    return s.Lbrace;
                    break;
                case ref ast.IfStmt s:
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
                case ref ast.ForStmt s:
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
                case ref ast.LabeledStmt s:
                    return f.statementBoundary(s.Stmt);
                    break;
                case ref ast.RangeStmt s:
                    (found, pos) = hasFuncLiteral(s.X);
                    if (found)
                    {
                        return pos;
                    }
                    return s.Body.Lbrace;
                    break;
                case ref ast.SwitchStmt s:
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
                case ref ast.SelectStmt s:
                    return s.Body.Lbrace;
                    break;
                case ref ast.TypeSwitchStmt s:
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
        private static bool endsBasicSourceBlock(this ref File f, ast.Stmt s)
        {
            switch (s.type())
            {
                case ref ast.BlockStmt s:
                    return true;
                    break;
                case ref ast.BranchStmt s:
                    return true;
                    break;
                case ref ast.ForStmt s:
                    return true;
                    break;
                case ref ast.IfStmt s:
                    return true;
                    break;
                case ref ast.LabeledStmt s:
                    return true; // A goto may branch here, starting a new basic block.
                    break;
                case ref ast.RangeStmt s:
                    return true;
                    break;
                case ref ast.SwitchStmt s:
                    return true;
                    break;
                case ref ast.SelectStmt s:
                    return true;
                    break;
                case ref ast.TypeSwitchStmt s:
                    return true;
                    break;
                case ref ast.ExprStmt s:
                    {
                        ref ast.CallExpr (call, ok) = s.X._<ref ast.CallExpr>();

                        if (ok)
                        {
                            {
                                ref ast.Ident (ident, ok) = call.Fun._<ref ast.Ident>();

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
        private static bool isControl(this ref File f, ast.Stmt s)
        {
            switch (s.type())
            {
                case ref ast.ForStmt _:
                    return true;
                    break;
                case ref ast.RangeStmt _:
                    return true;
                    break;
                case ref ast.SwitchStmt _:
                    return true;
                    break;
                case ref ast.SelectStmt _:
                    return true;
                    break;
                case ref ast.TypeSwitchStmt _:
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

        private static ast.Visitor Visit(this ref funcLitFinder f, ast.Node node)
        {
            if (f.found())
            {
                return null; // Prune search.
            }
            switch (node.type())
            {
                case ref ast.FuncLit n:
                    f.Value = funcLitFinder(n.Body.Lbrace);
                    return null; // Prune search.
                    break;
            }
            return f;
        }

        private static bool found(this ref funcLitFinder f)
        {
            return token.Pos(f.Value) != token.NoPos;
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
        private static long offset(this ref File f, token.Pos pos)
        {
            return f.fset.Position(pos).Offset;
        }

        // addVariables adds to the end of the file the declarations to set up the counter and position variables.
        private static void addVariables(this ref File f, io.Writer w)
        { 
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
            fmt.Fprintf(w, "\nvar %s = struct {\n", varVar.Value);
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
            if (mode == "atomic".Value)
            {
                fmt.Fprintf(w, "var _ = %s.LoadUint32\n", atomicPackageName);
            }
        }
    }
}
