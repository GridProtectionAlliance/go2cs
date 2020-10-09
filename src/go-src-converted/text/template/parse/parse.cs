// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package parse builds parse trees for templates as defined by text/template
// and html/template. Clients should use those packages to construct templates
// rather than this one, which provides shared internal data structures not
// intended for general use.
// package parse -- go2cs converted at 2020 October 09 04:59:22 UTC
// import "text/template/parse" ==> using parse = go.text.template.parse_package
// Original source: C:\Go\src\text\template\parse\parse.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace text {
namespace template
{
    public static partial class parse_package
    {
        // Tree is the representation of a single parsed template.
        public partial struct Tree
        {
            public @string Name; // name of the template represented by the tree.
            public @string ParseName; // name of the top-level template during parsing, for error messages.
            public ptr<ListNode> Root; // top-level root of the tree.
            public @string text; // text parsed to create the template (or its parent)
// Parsing only; cleared after parse.
            public slice<object> funcs;
            public ptr<lexer> lex;
            public array<item> token; // three-token lookahead for parser.
            public long peekCount;
            public slice<@string> vars; // variables defined at the moment.
            public map<@string, ptr<Tree>> treeSet;
        }

        // Copy returns a copy of the Tree. Any parsing state is discarded.
        private static ptr<Tree> Copy(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            if (t == null)
            {
                return _addr_null!;
            }

            return addr(new Tree(Name:t.Name,ParseName:t.ParseName,Root:t.Root.CopyList(),text:t.text,));

        }

        // Parse returns a map from template name to parse.Tree, created by parsing the
        // templates described in the argument string. The top-level template will be
        // given the specified name. If an error is encountered, parsing stops and an
        // empty map is returned with the error.
        public static (map<@string, ptr<Tree>>, error) Parse(@string name, @string text, @string leftDelim, @string rightDelim, params object[] funcs)
        {
            map<@string, ptr<Tree>> _p0 = default;
            error _p0 = default!;
            funcs = funcs.Clone();

            var treeSet = make_map<@string, ptr<Tree>>();
            var t = New(name);
            t.text = text;
            var (_, err) = t.Parse(text, leftDelim, rightDelim, treeSet, funcs);
            return (treeSet, error.As(err)!);
        }

        // next returns the next token.
        private static item next(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            if (t.peekCount > 0L)
            {
                t.peekCount--;
            }
            else
            {
                t.token[0L] = t.lex.nextItem();
            }

            return t.token[t.peekCount];

        }

        // backup backs the input stream up one token.
        private static void backup(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            t.peekCount++;
        }

        // backup2 backs the input stream up two tokens.
        // The zeroth token is already there.
        private static void backup2(this ptr<Tree> _addr_t, item t1)
        {
            ref Tree t = ref _addr_t.val;

            t.token[1L] = t1;
            t.peekCount = 2L;
        }

        // backup3 backs the input stream up three tokens
        // The zeroth token is already there.
        private static void backup3(this ptr<Tree> _addr_t, item t2, item t1)
        {
            ref Tree t = ref _addr_t.val;
 // Reverse order: we're pushing back.
            t.token[1L] = t1;
            t.token[2L] = t2;
            t.peekCount = 3L;

        }

        // peek returns but does not consume the next token.
        private static item peek(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            if (t.peekCount > 0L)
            {
                return t.token[t.peekCount - 1L];
            }

            t.peekCount = 1L;
            t.token[0L] = t.lex.nextItem();
            return t.token[0L];

        }

        // nextNonSpace returns the next non-space token.
        private static item nextNonSpace(this ptr<Tree> _addr_t)
        {
            item token = default;
            ref Tree t = ref _addr_t.val;

            while (true)
            {
                token = t.next();
                if (token.typ != itemSpace)
                {
                    break;
                }

            }

            return token;

        }

        // peekNonSpace returns but does not consume the next non-space token.
        private static item peekNonSpace(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            var token = t.nextNonSpace();
            t.backup();
            return token;
        }

        // Parsing.

        // New allocates a new parse tree with the given name.
        public static ptr<Tree> New(@string name, params object[] funcs)
        {
            funcs = funcs.Clone();

            return addr(new Tree(Name:name,funcs:funcs,));
        }

        // ErrorContext returns a textual representation of the location of the node in the input text.
        // The receiver is only used when the node does not have a pointer to the tree inside,
        // which can occur in old code.
        private static (@string, @string) ErrorContext(this ptr<Tree> _addr_t, Node n)
        {
            @string location = default;
            @string context = default;
            ref Tree t = ref _addr_t.val;

            var pos = int(n.Position());
            var tree = n.tree();
            if (tree == null)
            {
                tree = t;
            }

            var text = tree.text[..pos];
            var byteNum = strings.LastIndex(text, "\n");
            if (byteNum == -1L)
            {
                byteNum = pos; // On first line.
            }
            else
            {
                byteNum++; // After the newline.
                byteNum = pos - byteNum;

            }

            long lineNum = 1L + strings.Count(text, "\n");
            context = n.String();
            return (fmt.Sprintf("%s:%d:%d", tree.ParseName, lineNum, byteNum), context);

        }

        // errorf formats the error and terminates processing.
        private static void errorf(this ptr<Tree> _addr_t, @string format, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();
            ref Tree t = ref _addr_t.val;

            t.Root = null;
            format = fmt.Sprintf("template: %s:%d: %s", t.ParseName, t.token[0L].line, format);
            panic(fmt.Errorf(format, args));
        });

        // error terminates processing.
        private static void error(this ptr<Tree> _addr_t, error err)
        {
            ref Tree t = ref _addr_t.val;

            t.errorf("%s", err);
        }

        // expect consumes the next token and guarantees it has the required type.
        private static item expect(this ptr<Tree> _addr_t, itemType expected, @string context)
        {
            ref Tree t = ref _addr_t.val;

            var token = t.nextNonSpace();
            if (token.typ != expected)
            {
                t.unexpected(token, context);
            }

            return token;

        }

        // expectOneOf consumes the next token and guarantees it has one of the required types.
        private static item expectOneOf(this ptr<Tree> _addr_t, itemType expected1, itemType expected2, @string context)
        {
            ref Tree t = ref _addr_t.val;

            var token = t.nextNonSpace();
            if (token.typ != expected1 && token.typ != expected2)
            {
                t.unexpected(token, context);
            }

            return token;

        }

        // unexpected complains about the token and terminates processing.
        private static void unexpected(this ptr<Tree> _addr_t, item token, @string context)
        {
            ref Tree t = ref _addr_t.val;

            t.errorf("unexpected %s in %s", token, context);
        }

        // recover is the handler that turns panics into returns from the top level of Parse.
        private static void recover(this ptr<Tree> _addr_t, ptr<error> _addr_errp) => func((_, panic, __) =>
        {
            ref Tree t = ref _addr_t.val;
            ref error errp = ref _addr_errp.val;

            var e = recover();
            if (e != null)
            {
                {
                    runtime.Error (_, ok) = e._<runtime.Error>();

                    if (ok)
                    {
                        panic(e);
                    }

                }

                if (t != null)
                {
                    t.lex.drain();
                    t.stopParse();
                }

                errp = error.As(e._<error>())!;

            }

        });

        // startParse initializes the parser, using the lexer.
        private static void startParse(this ptr<Tree> _addr_t, slice<object> funcs, ptr<lexer> _addr_lex, map<@string, ptr<Tree>> treeSet)
        {
            ref Tree t = ref _addr_t.val;
            ref lexer lex = ref _addr_lex.val;

            t.Root = null;
            t.lex = lex;
            t.vars = new slice<@string>(new @string[] { "$" });
            t.funcs = funcs;
            t.treeSet = treeSet;
        }

        // stopParse terminates parsing.
        private static void stopParse(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            t.lex = null;
            t.vars = null;
            t.funcs = null;
            t.treeSet = null;
        }

        // Parse parses the template definition string to construct a representation of
        // the template for execution. If either action delimiter string is empty, the
        // default ("{{" or "}}") is used. Embedded template definitions are added to
        // the treeSet map.
        private static (ptr<Tree>, error) Parse(this ptr<Tree> _addr_t, @string text, @string leftDelim, @string rightDelim, map<@string, ptr<Tree>> treeSet, params object[] funcs) => func((defer, _, __) =>
        {
            ptr<Tree> tree = default!;
            error err = default!;
            funcs = funcs.Clone();
            ref Tree t = ref _addr_t.val;

            defer(t.recover(_addr_err));
            t.ParseName = t.Name;
            t.startParse(funcs, lex(t.Name, text, leftDelim, rightDelim), treeSet);
            t.text = text;
            t.parse();
            t.add();
            t.stopParse();
            return (_addr_t!, error.As(null!)!);
        });

        // add adds tree to t.treeSet.
        private static void add(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            var tree = t.treeSet[t.Name];
            if (tree == null || IsEmptyTree(tree.Root))
            {
                t.treeSet[t.Name] = t;
                return ;
            }

            if (!IsEmptyTree(t.Root))
            {
                t.errorf("template: multiple definition of template %q", t.Name);
            }

        }

        // IsEmptyTree reports whether this tree (node) is empty of everything but space.
        public static bool IsEmptyTree(Node n) => func((_, panic, __) =>
        {
            switch (n.type())
            {
                case 
                    return true;
                    break;
                case ptr<ActionNode> n:
                    break;
                case ptr<IfNode> n:
                    break;
                case ptr<ListNode> n:
                    foreach (var (_, node) in n.Nodes)
                    {
                        if (!IsEmptyTree(node))
                        {
                            return false;
                        }

                    }
                    return true;
                    break;
                case ptr<RangeNode> n:
                    break;
                case ptr<TemplateNode> n:
                    break;
                case ptr<TextNode> n:
                    return len(bytes.TrimSpace(n.Text)) == 0L;
                    break;
                case ptr<WithNode> n:
                    break;
                default:
                {
                    var n = n.type();
                    panic("unknown node: " + n.String());
                    break;
                }
            }
            return false;

        });

        // parse is the top-level parser for a template, essentially the same
        // as itemList except it also parses {{define}} actions.
        // It runs to EOF.
        private static void parse(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            t.Root = t.newList(t.peek().pos);
            while (t.peek().typ != itemEOF)
            {
                if (t.peek().typ == itemLeftDelim)
                {
                    var delim = t.next();
                    if (t.nextNonSpace().typ == itemDefine)
                    {
                        var newT = New("definition"); // name will be updated once we know it.
                        newT.text = t.text;
                        newT.ParseName = t.ParseName;
                        newT.startParse(t.funcs, t.lex, t.treeSet);
                        newT.parseDefinition();
                        continue;

                    }

                    t.backup2(delim);

                }

                {
                    var n = t.textOrAction();


                    if (n.Type() == nodeEnd || n.Type() == nodeElse) 
                        t.errorf("unexpected %s", n);
                    else 
                        t.Root.append(n);

                }

            }


        }

        // parseDefinition parses a {{define}} ...  {{end}} template definition and
        // installs the definition in t.treeSet. The "define" keyword has already
        // been scanned.
        private static void parseDefinition(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            const @string context = (@string)"define clause";

            var name = t.expectOneOf(itemString, itemRawString, context);
            error err = default!;
            t.Name, err = strconv.Unquote(name.val);
            if (err != null)
            {
                t.error(err);
            }

            t.expect(itemRightDelim, context);
            Node end = default;
            t.Root, end = t.itemList();
            if (end.Type() != nodeEnd)
            {
                t.errorf("unexpected %s in %s", end, context);
            }

            t.add();
            t.stopParse();

        }

        // itemList:
        //    textOrAction*
        // Terminates at {{end}} or {{else}}, returned separately.
        private static (ptr<ListNode>, Node) itemList(this ptr<Tree> _addr_t)
        {
            ptr<ListNode> list = default!;
            Node next = default;
            ref Tree t = ref _addr_t.val;

            list = t.newList(t.peekNonSpace().pos);
            while (t.peekNonSpace().typ != itemEOF)
            {
                var n = t.textOrAction();

                if (n.Type() == nodeEnd || n.Type() == nodeElse) 
                    return (_addr_list!, n);
                                list.append(n);

            }

            t.errorf("unexpected EOF");
            return ;

        }

        // textOrAction:
        //    text | action
        private static Node textOrAction(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            {
                var token = t.nextNonSpace();


                if (token.typ == itemText) 
                    return t.newText(token.pos, token.val);
                else if (token.typ == itemLeftDelim) 
                    return t.action();
                else 
                    t.unexpected(token, "input");

            }
            return null;

        }

        // Action:
        //    control
        //    command ("|" command)*
        // Left delim is past. Now get actions.
        // First word could be a keyword such as range.
        private static Node action(this ptr<Tree> _addr_t)
        {
            Node n = default;
            ref Tree t = ref _addr_t.val;

            {
                var token__prev1 = token;

                var token = t.nextNonSpace();


                if (token.typ == itemBlock) 
                    return t.blockControl();
                else if (token.typ == itemElse) 
                    return t.elseControl();
                else if (token.typ == itemEnd) 
                    return t.endControl();
                else if (token.typ == itemIf) 
                    return t.ifControl();
                else if (token.typ == itemRange) 
                    return t.rangeControl();
                else if (token.typ == itemTemplate) 
                    return t.templateControl();
                else if (token.typ == itemWith) 
                    return t.withControl();


                token = token__prev1;
            }
            t.backup();
            token = t.peek(); 
            // Do not pop variables; they persist until "end".
            return t.newAction(token.pos, token.line, t.pipeline("command"));

        }

        // Pipeline:
        //    declarations? command ('|' command)*
        private static ptr<PipeNode> pipeline(this ptr<Tree> _addr_t, @string context)
        {
            ptr<PipeNode> pipe = default!;
            ref Tree t = ref _addr_t.val;

            var token = t.peekNonSpace();
            pipe = t.newPipeline(token.pos, token.line, null); 
            // Are there declarations or assignments?
decls:
            {
                var v = t.peekNonSpace();

                if (v.typ == itemVariable)
                {
                    t.next(); 
                    // Since space is a token, we need 3-token look-ahead here in the worst case:
                    // in "$x foo" we need to read "foo" (as opposed to ":=") to know that $x is an
                    // argument variable rather than a declaration. So remember the token
                    // adjacent to the variable so we can push it back if necessary.
                    var tokenAfterVariable = t.peek();
                    var next = t.peekNonSpace();

                    if (next.typ == itemAssign || next.typ == itemDeclare) 
                        pipe.IsAssign = next.typ == itemAssign;
                        t.nextNonSpace();
                        pipe.Decl = append(pipe.Decl, t.newVariable(v.pos, v.val));
                        t.vars = append(t.vars, v.val);
                    else if (next.typ == itemChar && next.val == ",") 
                        t.nextNonSpace();
                        pipe.Decl = append(pipe.Decl, t.newVariable(v.pos, v.val));
                        t.vars = append(t.vars, v.val);
                        if (context == "range" && len(pipe.Decl) < 2L)
                        {

                            if (t.peekNonSpace().typ == itemVariable || t.peekNonSpace().typ == itemRightDelim || t.peekNonSpace().typ == itemRightParen) 
                                // second initialized variable in a range pipeline
                                goto decls;
                            else 
                                t.errorf("range can only initialize variables");
                            
                        }

                        t.errorf("too many declarations in %s", context);
                    else if (tokenAfterVariable.typ == itemSpace) 
                        t.backup3(v, tokenAfterVariable);
                    else 
                        t.backup2(v);
                    
                }

            }

            while (true)
            {
                {
                    var token__prev1 = token;

                    token = t.nextNonSpace();


                    if (token.typ == itemRightDelim || token.typ == itemRightParen) 
                        // At this point, the pipeline is complete
                        t.checkPipeline(pipe, context);
                        if (token.typ == itemRightParen)
                        {
                            t.backup();
                        }

                        return ;
                    else if (token.typ == itemBool || token.typ == itemCharConstant || token.typ == itemComplex || token.typ == itemDot || token.typ == itemField || token.typ == itemIdentifier || token.typ == itemNumber || token.typ == itemNil || token.typ == itemRawString || token.typ == itemString || token.typ == itemVariable || token.typ == itemLeftParen) 
                        t.backup();
                        pipe.append(t.command());
                    else 
                        t.unexpected(token, context);


                    token = token__prev1;
                }

            }


        }

        private static void checkPipeline(this ptr<Tree> _addr_t, ptr<PipeNode> _addr_pipe, @string context)
        {
            ref Tree t = ref _addr_t.val;
            ref PipeNode pipe = ref _addr_pipe.val;
 
            // Reject empty pipelines
            if (len(pipe.Cmds) == 0L)
            {
                t.errorf("missing value for %s", context);
            } 
            // Only the first command of a pipeline can start with a non executable operand
            foreach (var (i, c) in pipe.Cmds[1L..])
            {

                if (c.Args[0L].Type() == NodeBool || c.Args[0L].Type() == NodeDot || c.Args[0L].Type() == NodeNil || c.Args[0L].Type() == NodeNumber || c.Args[0L].Type() == NodeString) 
                    // With A|B|C, pipeline stage 2 is B
                    t.errorf("non executable command in pipeline stage %d", i + 2L);
                
            }

        }

        private static (Pos, long, ptr<PipeNode>, ptr<ListNode>, ptr<ListNode>) parseControl(this ptr<Tree> _addr_t, bool allowElseIf, @string context) => func((defer, _, __) =>
        {
            Pos pos = default;
            long line = default;
            ptr<PipeNode> pipe = default!;
            ptr<ListNode> list = default!;
            ptr<ListNode> elseList = default!;
            ref Tree t = ref _addr_t.val;

            defer(t.popVars(len(t.vars)));
            pipe = t.pipeline(context);
            Node next = default;
            list, next = t.itemList();

            if (next.Type() == nodeEnd)             else if (next.Type() == nodeElse) 
                if (allowElseIf)
                { 
                    // Special case for "else if". If the "else" is followed immediately by an "if",
                    // the elseControl will have left the "if" token pending. Treat
                    //    {{if a}}_{{else if b}}_{{end}}
                    // as
                    //    {{if a}}_{{else}}{{if b}}_{{end}}{{end}}.
                    // To do this, parse the if as usual and stop at it {{end}}; the subsequent{{end}}
                    // is assumed. This technique works even for long if-else-if chains.
                    // TODO: Should we allow else-if in with and range?
                    if (t.peek().typ == itemIf)
                    {
                        t.next(); // Consume the "if" token.
                        elseList = t.newList(next.Position());
                        elseList.append(t.ifControl()); 
                        // Do not consume the next item - only one {{end}} required.
                        break;

                    }

                }

                elseList, next = t.itemList();
                if (next.Type() != nodeEnd)
                {
                    t.errorf("expected end; found %s", next);
                }

                        return (pipe.Position(), pipe.Line, _addr_pipe!, _addr_list!, _addr_elseList!);

        });

        // If:
        //    {{if pipeline}} itemList {{end}}
        //    {{if pipeline}} itemList {{else}} itemList {{end}}
        // If keyword is past.
        private static Node ifControl(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            return t.newIf(t.parseControl(true, "if"));
        }

        // Range:
        //    {{range pipeline}} itemList {{end}}
        //    {{range pipeline}} itemList {{else}} itemList {{end}}
        // Range keyword is past.
        private static Node rangeControl(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            return t.newRange(t.parseControl(false, "range"));
        }

        // With:
        //    {{with pipeline}} itemList {{end}}
        //    {{with pipeline}} itemList {{else}} itemList {{end}}
        // If keyword is past.
        private static Node withControl(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            return t.newWith(t.parseControl(false, "with"));
        }

        // End:
        //    {{end}}
        // End keyword is past.
        private static Node endControl(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            return t.newEnd(t.expect(itemRightDelim, "end").pos);
        }

        // Else:
        //    {{else}}
        // Else keyword is past.
        private static Node elseControl(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;
 
            // Special case for "else if".
            var peek = t.peekNonSpace();
            if (peek.typ == itemIf)
            { 
                // We see "{{else if ... " but in effect rewrite it to {{else}}{{if ... ".
                return t.newElse(peek.pos, peek.line);

            }

            var token = t.expect(itemRightDelim, "else");
            return t.newElse(token.pos, token.line);

        }

        // Block:
        //    {{block stringValue pipeline}}
        // Block keyword is past.
        // The name must be something that can evaluate to a string.
        // The pipeline is mandatory.
        private static Node blockControl(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            const @string context = (@string)"block clause";



            var token = t.nextNonSpace();
            var name = t.parseTemplateName(token, context);
            var pipe = t.pipeline(context);

            var block = New(name); // name will be updated once we know it.
            block.text = t.text;
            block.ParseName = t.ParseName;
            block.startParse(t.funcs, t.lex, t.treeSet);
            Node end = default;
            block.Root, end = block.itemList();
            if (end.Type() != nodeEnd)
            {
                t.errorf("unexpected %s in %s", end, context);
            }

            block.add();
            block.stopParse();

            return t.newTemplate(token.pos, token.line, name, pipe);

        }

        // Template:
        //    {{template stringValue pipeline}}
        // Template keyword is past. The name must be something that can evaluate
        // to a string.
        private static Node templateControl(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            const @string context = (@string)"template clause";

            var token = t.nextNonSpace();
            var name = t.parseTemplateName(token, context);
            ptr<PipeNode> pipe;
            if (t.nextNonSpace().typ != itemRightDelim)
            {
                t.backup(); 
                // Do not pop variables; they persist until "end".
                pipe = t.pipeline(context);

            }

            return t.newTemplate(token.pos, token.line, name, pipe);

        }

        private static @string parseTemplateName(this ptr<Tree> _addr_t, item token, @string context)
        {
            @string name = default;
            ref Tree t = ref _addr_t.val;


            if (token.typ == itemString || token.typ == itemRawString) 
                var (s, err) = strconv.Unquote(token.val);
                if (err != null)
                {
                    t.error(err);
                }

                name = s;
            else 
                t.unexpected(token, context);
                        return ;

        }

        // command:
        //    operand (space operand)*
        // space-separated arguments up to a pipeline character or right delimiter.
        // we consume the pipe character but leave the right delim to terminate the action.
        private static ptr<CommandNode> command(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            var cmd = t.newCommand(t.peekNonSpace().pos);
            while (true)
            {
                t.peekNonSpace(); // skip leading spaces.
                var operand = t.operand();
                if (operand != null)
                {
                    cmd.append(operand);
                }

                {
                    var token = t.next();


                    if (token.typ == itemSpace) 
                        continue;
                    else if (token.typ == itemError) 
                        t.errorf("%s", token.val);
                    else if (token.typ == itemRightDelim || token.typ == itemRightParen) 
                        t.backup();
                    else if (token.typ == itemPipe)                     else 
                        t.errorf("unexpected %s in operand", token);

                }
                break;

            }

            if (len(cmd.Args) == 0L)
            {
                t.errorf("empty command");
            }

            return _addr_cmd!;

        }

        // operand:
        //    term .Field*
        // An operand is a space-separated component of a command,
        // a term possibly followed by field accesses.
        // A nil return means the next item is not an operand.
        private static Node operand(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            var node = t.term();
            if (node == null)
            {
                return null;
            }

            if (t.peek().typ == itemField)
            {
                var chain = t.newChain(t.peek().pos, node);
                while (t.peek().typ == itemField)
                {
                    chain.Add(t.next().val);
                } 
                // Compatibility with original API: If the term is of type NodeField
                // or NodeVariable, just put more fields on the original.
                // Otherwise, keep the Chain node.
                // Obvious parsing errors involving literal values are detected here.
                // More complex error cases will have to be handled at execution time.
 
                // Compatibility with original API: If the term is of type NodeField
                // or NodeVariable, just put more fields on the original.
                // Otherwise, keep the Chain node.
                // Obvious parsing errors involving literal values are detected here.
                // More complex error cases will have to be handled at execution time.

                if (node.Type() == NodeField) 
                    node = t.newField(chain.Position(), chain.String());
                else if (node.Type() == NodeVariable) 
                    node = t.newVariable(chain.Position(), chain.String());
                else if (node.Type() == NodeBool || node.Type() == NodeString || node.Type() == NodeNumber || node.Type() == NodeNil || node.Type() == NodeDot) 
                    t.errorf("unexpected . after term %q", node.String());
                else 
                    node = chain;
                
            }

            return node;

        }

        // term:
        //    literal (number, string, nil, boolean)
        //    function (identifier)
        //    .
        //    .Field
        //    $
        //    '(' pipeline ')'
        // A term is a simple "expression".
        // A nil return means the next item is not a term.
        private static Node term(this ptr<Tree> _addr_t)
        {
            ref Tree t = ref _addr_t.val;

            {
                var token__prev1 = token;

                var token = t.nextNonSpace();


                if (token.typ == itemError) 
                    t.errorf("%s", token.val);
                else if (token.typ == itemIdentifier) 
                    if (!t.hasFunction(token.val))
                    {
                        t.errorf("function %q not defined", token.val);
                    }

                    return NewIdentifier(token.val).SetTree(t).SetPos(token.pos);
                else if (token.typ == itemDot) 
                    return t.newDot(token.pos);
                else if (token.typ == itemNil) 
                    return t.newNil(token.pos);
                else if (token.typ == itemVariable) 
                    return t.useVar(token.pos, token.val);
                else if (token.typ == itemField) 
                    return t.newField(token.pos, token.val);
                else if (token.typ == itemBool) 
                    return t.newBool(token.pos, token.val == "true");
                else if (token.typ == itemCharConstant || token.typ == itemComplex || token.typ == itemNumber) 
                    var (number, err) = t.newNumber(token.pos, token.val, token.typ);
                    if (err != null)
                    {
                        t.error(err);
                    }

                    return number;
                else if (token.typ == itemLeftParen) 
                    var pipe = t.pipeline("parenthesized pipeline");
                    {
                        var token__prev1 = token;

                        token = t.next();

                        if (token.typ != itemRightParen)
                        {
                            t.errorf("unclosed right paren: unexpected %s", token);
                        }

                        token = token__prev1;

                    }

                    return pipe;
                else if (token.typ == itemString || token.typ == itemRawString) 
                    var (s, err) = strconv.Unquote(token.val);
                    if (err != null)
                    {
                        t.error(err);
                    }

                    return t.newString(token.pos, token.val, s);


                token = token__prev1;
            }
            t.backup();
            return null;

        }

        // hasFunction reports if a function name exists in the Tree's maps.
        private static bool hasFunction(this ptr<Tree> _addr_t, @string name)
        {
            ref Tree t = ref _addr_t.val;

            foreach (var (_, funcMap) in t.funcs)
            {
                if (funcMap == null)
                {
                    continue;
                }

                if (funcMap[name] != null)
                {
                    return true;
                }

            }
            return false;

        }

        // popVars trims the variable list to the specified length
        private static void popVars(this ptr<Tree> _addr_t, long n)
        {
            ref Tree t = ref _addr_t.val;

            t.vars = t.vars[..n];
        }

        // useVar returns a node for a variable reference. It errors if the
        // variable is not defined.
        private static Node useVar(this ptr<Tree> _addr_t, Pos pos, @string name)
        {
            ref Tree t = ref _addr_t.val;

            var v = t.newVariable(pos, name);
            foreach (var (_, varName) in t.vars)
            {
                if (varName == v.Ident[0L])
                {
                    return v;
                }

            }
            t.errorf("undefined variable %q", v.Ident[0L]);
            return null;

        }
    }
}}}
