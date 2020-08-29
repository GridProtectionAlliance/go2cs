// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse nodes.

// package parse -- go2cs converted at 2020 August 29 08:34:38 UTC
// import "text/template/parse" ==> using parse = go.text.template.parse_package
// Original source: C:\Go\src\text\template\parse\node.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace text {
namespace template
{
    public static partial class parse_package
    {
        private static @string textFormat = "%s"; // Changed to "%q" in tests for better error messages.

        // A Node is an element in the parse tree. The interface is trivial.
        // The interface contains an unexported method so that only
        // types local to this package can satisfy it.
        public partial interface Node
        {
            ref Tree Type();
            ref Tree String(); // Copy does a deep copy of the Node and all its components.
// To avoid type assertions, some XxxNodes also have specialized
// CopyXxx methods that return *XxxNode.
            ref Tree Copy();
            ref Tree Position(); // byte position of start of node in full original input string
// tree returns the containing *Tree.
// It is unexported so all implementations of Node are in this package.
            ref Tree tree();
        }

        // NodeType identifies the type of a parse tree node.
        public partial struct NodeType // : long
        {
        }

        // Pos represents a byte position in the original input text from which
        // this template was parsed.
        public partial struct Pos // : long
        {
        }

        public static Pos Position(this Pos p)
        {
            return p;
        }

        // Type returns itself and provides an easy default implementation
        // for embedding in a Node. Embedded in all non-trivial Nodes.
        public static NodeType Type(this NodeType t)
        {
            return t;
        }

        public static readonly NodeType NodeText = iota; // Plain text.
        public static readonly var NodeAction = 0; // A non-control action such as a field evaluation.
        public static readonly var NodeBool = 1; // A boolean constant.
        public static readonly var NodeChain = 2; // A sequence of field accesses.
        public static readonly var NodeCommand = 3; // An element of a pipeline.
        public static readonly var NodeDot = 4; // The cursor, dot.
        private static readonly var nodeElse = 5; // An else action. Not added to tree.
        private static readonly var nodeEnd = 6; // An end action. Not added to tree.
        public static readonly var NodeField = 7; // A field or method name.
        public static readonly var NodeIdentifier = 8; // An identifier; always a function name.
        public static readonly var NodeIf = 9; // An if action.
        public static readonly var NodeList = 10; // A list of Nodes.
        public static readonly var NodeNil = 11; // An untyped nil constant.
        public static readonly var NodeNumber = 12; // A numerical constant.
        public static readonly var NodePipe = 13; // A pipeline of commands.
        public static readonly var NodeRange = 14; // A range action.
        public static readonly var NodeString = 15; // A string constant.
        public static readonly var NodeTemplate = 16; // A template invocation action.
        public static readonly var NodeVariable = 17; // A $ variable.
        public static readonly var NodeWith = 18; // A with action.

        // Nodes.

        // ListNode holds a sequence of nodes.
        public partial struct ListNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public slice<Node> Nodes; // The element nodes in lexical order.
        }

        private static ref ListNode newList(this ref Tree t, Pos pos)
        {
            return ref new ListNode(tr:t,NodeType:NodeList,Pos:pos);
        }

        private static void append(this ref ListNode l, Node n)
        {
            l.Nodes = append(l.Nodes, n);
        }

        private static ref Tree tree(this ref ListNode l)
        {
            return l.tr;
        }

        private static @string String(this ref ListNode l)
        {
            ptr<object> b = @new<bytes.Buffer>();
            foreach (var (_, n) in l.Nodes)
            {
                fmt.Fprint(b, n);
            }
            return b.String();
        }

        private static ref ListNode CopyList(this ref ListNode l)
        {
            if (l == null)
            {
                return l;
            }
            var n = l.tr.newList(l.Pos);
            foreach (var (_, elem) in l.Nodes)
            {
                n.append(elem.Copy());
            }
            return n;
        }

        private static Node Copy(this ref ListNode l)
        {
            return l.CopyList();
        }

        // TextNode holds plain text.
        public partial struct TextNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public slice<byte> Text; // The text; may span newlines.
        }

        private static ref TextNode newText(this ref Tree t, Pos pos, @string text)
        {
            return ref new TextNode(tr:t,NodeType:NodeText,Pos:pos,Text:[]byte(text));
        }

        private static @string String(this ref TextNode t)
        {
            return fmt.Sprintf(textFormat, t.Text);
        }

        private static ref Tree tree(this ref TextNode t)
        {
            return t.tr;
        }

        private static Node Copy(this ref TextNode t)
        {
            return ref new TextNode(tr:t.tr,NodeType:NodeText,Pos:t.Pos,Text:append([]byte{},t.Text...));
        }

        // PipeNode holds a pipeline with optional declaration
        public partial struct PipeNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public long Line; // The line number in the input. Deprecated: Kept for compatibility.
            public slice<ref VariableNode> Decl; // Variable declarations in lexical order.
            public slice<ref CommandNode> Cmds; // The commands in lexical order.
        }

        private static ref PipeNode newPipeline(this ref Tree t, Pos pos, long line, slice<ref VariableNode> decl)
        {
            return ref new PipeNode(tr:t,NodeType:NodePipe,Pos:pos,Line:line,Decl:decl);
        }

        private static void append(this ref PipeNode p, ref CommandNode command)
        {
            p.Cmds = append(p.Cmds, command);
        }

        private static @string String(this ref PipeNode p)
        {
            @string s = "";
            if (len(p.Decl) > 0L)
            {
                {
                    var i__prev1 = i;

                    foreach (var (__i, __v) in p.Decl)
                    {
                        i = __i;
                        v = __v;
                        if (i > 0L)
                        {
                            s += ", ";
                        }
                        s += v.String();
                    }

                    i = i__prev1;
                }

                s += " := ";
            }
            {
                var i__prev1 = i;

                foreach (var (__i, __c) in p.Cmds)
                {
                    i = __i;
                    c = __c;
                    if (i > 0L)
                    {
                        s += " | ";
                    }
                    s += c.String();
                }

                i = i__prev1;
            }

            return s;
        }

        private static ref Tree tree(this ref PipeNode p)
        {
            return p.tr;
        }

        private static ref PipeNode CopyPipe(this ref PipeNode p)
        {
            if (p == null)
            {
                return p;
            }
            slice<ref VariableNode> decl = default;
            foreach (var (_, d) in p.Decl)
            {
                decl = append(decl, d.Copy()._<ref VariableNode>());
            }
            var n = p.tr.newPipeline(p.Pos, p.Line, decl);
            foreach (var (_, c) in p.Cmds)
            {
                n.append(c.Copy()._<ref CommandNode>());
            }
            return n;
        }

        private static Node Copy(this ref PipeNode p)
        {
            return p.CopyPipe();
        }

        // ActionNode holds an action (something bounded by delimiters).
        // Control actions have their own nodes; ActionNode represents simple
        // ones such as field evaluations and parenthesized pipelines.
        public partial struct ActionNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public long Line; // The line number in the input. Deprecated: Kept for compatibility.
            public ptr<PipeNode> Pipe; // The pipeline in the action.
        }

        private static ref ActionNode newAction(this ref Tree t, Pos pos, long line, ref PipeNode pipe)
        {
            return ref new ActionNode(tr:t,NodeType:NodeAction,Pos:pos,Line:line,Pipe:pipe);
        }

        private static @string String(this ref ActionNode a)
        {
            return fmt.Sprintf("{{%s}}", a.Pipe);

        }

        private static ref Tree tree(this ref ActionNode a)
        {
            return a.tr;
        }

        private static Node Copy(this ref ActionNode a)
        {
            return a.tr.newAction(a.Pos, a.Line, a.Pipe.CopyPipe());

        }

        // CommandNode holds a command (a pipeline inside an evaluating action).
        public partial struct CommandNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public slice<Node> Args; // Arguments in lexical order: Identifier, field, or constant.
        }

        private static ref CommandNode newCommand(this ref Tree t, Pos pos)
        {
            return ref new CommandNode(tr:t,NodeType:NodeCommand,Pos:pos);
        }

        private static void append(this ref CommandNode c, Node arg)
        {
            c.Args = append(c.Args, arg);
        }

        private static @string String(this ref CommandNode c)
        {
            @string s = "";
            {
                var arg__prev1 = arg;

                foreach (var (__i, __arg) in c.Args)
                {
                    i = __i;
                    arg = __arg;
                    if (i > 0L)
                    {
                        s += " ";
                    }
                    {
                        var arg__prev1 = arg;

                        ref PipeNode (arg, ok) = arg._<ref PipeNode>();

                        if (ok)
                        {
                            s += "(" + arg.String() + ")";
                            continue;
                        }

                        arg = arg__prev1;

                    }
                    s += arg.String();
                }

                arg = arg__prev1;
            }

            return s;
        }

        private static ref Tree tree(this ref CommandNode c)
        {
            return c.tr;
        }

        private static Node Copy(this ref CommandNode c)
        {
            if (c == null)
            {
                return c;
            }
            var n = c.tr.newCommand(c.Pos);
            foreach (var (_, c) in c.Args)
            {
                n.append(c.Copy());
            }
            return n;
        }

        // IdentifierNode holds an identifier.
        public partial struct IdentifierNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public @string Ident; // The identifier's name.
        }

        // NewIdentifier returns a new IdentifierNode with the given identifier name.
        public static ref IdentifierNode NewIdentifier(@string ident)
        {
            return ref new IdentifierNode(NodeType:NodeIdentifier,Ident:ident);
        }

        // SetPos sets the position. NewIdentifier is a public method so we can't modify its signature.
        // Chained for convenience.
        // TODO: fix one day?
        private static ref IdentifierNode SetPos(this ref IdentifierNode i, Pos pos)
        {
            i.Pos = pos;
            return i;
        }

        // SetTree sets the parent tree for the node. NewIdentifier is a public method so we can't modify its signature.
        // Chained for convenience.
        // TODO: fix one day?
        private static ref IdentifierNode SetTree(this ref IdentifierNode i, ref Tree t)
        {
            i.tr = t;
            return i;
        }

        private static @string String(this ref IdentifierNode i)
        {
            return i.Ident;
        }

        private static ref Tree tree(this ref IdentifierNode i)
        {
            return i.tr;
        }

        private static Node Copy(this ref IdentifierNode i)
        {
            return NewIdentifier(i.Ident).SetTree(i.tr).SetPos(i.Pos);
        }

        // VariableNode holds a list of variable names, possibly with chained field
        // accesses. The dollar sign is part of the (first) name.
        public partial struct VariableNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public slice<@string> Ident; // Variable name and fields in lexical order.
        }

        private static ref VariableNode newVariable(this ref Tree t, Pos pos, @string ident)
        {
            return ref new VariableNode(tr:t,NodeType:NodeVariable,Pos:pos,Ident:strings.Split(ident,"."));
        }

        private static @string String(this ref VariableNode v)
        {
            @string s = "";
            foreach (var (i, id) in v.Ident)
            {
                if (i > 0L)
                {
                    s += ".";
                }
                s += id;
            }
            return s;
        }

        private static ref Tree tree(this ref VariableNode v)
        {
            return v.tr;
        }

        private static Node Copy(this ref VariableNode v)
        {
            return ref new VariableNode(tr:v.tr,NodeType:NodeVariable,Pos:v.Pos,Ident:append([]string{},v.Ident...));
        }

        // DotNode holds the special identifier '.'.
        public partial struct DotNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
        }

        private static ref DotNode newDot(this ref Tree t, Pos pos)
        {
            return ref new DotNode(tr:t,NodeType:NodeDot,Pos:pos);
        }

        private static NodeType Type(this ref DotNode d)
        { 
            // Override method on embedded NodeType for API compatibility.
            // TODO: Not really a problem; could change API without effect but
            // api tool complains.
            return NodeDot;
        }

        private static @string String(this ref DotNode d)
        {
            return ".";
        }

        private static ref Tree tree(this ref DotNode d)
        {
            return d.tr;
        }

        private static Node Copy(this ref DotNode d)
        {
            return d.tr.newDot(d.Pos);
        }

        // NilNode holds the special identifier 'nil' representing an untyped nil constant.
        public partial struct NilNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
        }

        private static ref NilNode newNil(this ref Tree t, Pos pos)
        {
            return ref new NilNode(tr:t,NodeType:NodeNil,Pos:pos);
        }

        private static NodeType Type(this ref NilNode n)
        { 
            // Override method on embedded NodeType for API compatibility.
            // TODO: Not really a problem; could change API without effect but
            // api tool complains.
            return NodeNil;
        }

        private static @string String(this ref NilNode n)
        {
            return "nil";
        }

        private static ref Tree tree(this ref NilNode n)
        {
            return n.tr;
        }

        private static Node Copy(this ref NilNode n)
        {
            return n.tr.newNil(n.Pos);
        }

        // FieldNode holds a field (identifier starting with '.').
        // The names may be chained ('.x.y').
        // The period is dropped from each ident.
        public partial struct FieldNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public slice<@string> Ident; // The identifiers in lexical order.
        }

        private static ref FieldNode newField(this ref Tree t, Pos pos, @string ident)
        {
            return ref new FieldNode(tr:t,NodeType:NodeField,Pos:pos,Ident:strings.Split(ident[1:],".")); // [1:] to drop leading period
        }

        private static @string String(this ref FieldNode f)
        {
            @string s = "";
            foreach (var (_, id) in f.Ident)
            {
                s += "." + id;
            }
            return s;
        }

        private static ref Tree tree(this ref FieldNode f)
        {
            return f.tr;
        }

        private static Node Copy(this ref FieldNode f)
        {
            return ref new FieldNode(tr:f.tr,NodeType:NodeField,Pos:f.Pos,Ident:append([]string{},f.Ident...));
        }

        // ChainNode holds a term followed by a chain of field accesses (identifier starting with '.').
        // The names may be chained ('.x.y').
        // The periods are dropped from each ident.
        public partial struct ChainNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public Node Node;
            public slice<@string> Field; // The identifiers in lexical order.
        }

        private static ref ChainNode newChain(this ref Tree t, Pos pos, Node node)
        {
            return ref new ChainNode(tr:t,NodeType:NodeChain,Pos:pos,Node:node);
        }

        // Add adds the named field (which should start with a period) to the end of the chain.
        private static void Add(this ref ChainNode _c, @string field) => func(_c, (ref ChainNode c, Defer _, Panic panic, Recover __) =>
        {
            if (len(field) == 0L || field[0L] != '.')
            {
                panic("no dot in field");
            }
            field = field[1L..]; // Remove leading dot.
            if (field == "")
            {
                panic("empty field");
            }
            c.Field = append(c.Field, field);
        });

        private static @string String(this ref ChainNode c)
        {
            var s = c.Node.String();
            {
                ref PipeNode (_, ok) = c.Node._<ref PipeNode>();

                if (ok)
                {
                    s = "(" + s + ")";
                }

            }
            foreach (var (_, field) in c.Field)
            {
                s += "." + field;
            }
            return s;
        }

        private static ref Tree tree(this ref ChainNode c)
        {
            return c.tr;
        }

        private static Node Copy(this ref ChainNode c)
        {
            return ref new ChainNode(tr:c.tr,NodeType:NodeChain,Pos:c.Pos,Node:c.Node,Field:append([]string{},c.Field...));
        }

        // BoolNode holds a boolean constant.
        public partial struct BoolNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public bool True; // The value of the boolean constant.
        }

        private static ref BoolNode newBool(this ref Tree t, Pos pos, bool @true)
        {
            return ref new BoolNode(tr:t,NodeType:NodeBool,Pos:pos,True:true);
        }

        private static @string String(this ref BoolNode b)
        {
            if (b.True)
            {
                return "true";
            }
            return "false";
        }

        private static ref Tree tree(this ref BoolNode b)
        {
            return b.tr;
        }

        private static Node Copy(this ref BoolNode b)
        {
            return b.tr.newBool(b.Pos, b.True);
        }

        // NumberNode holds a number: signed or unsigned integer, float, or complex.
        // The value is parsed and stored under all the types that can represent the value.
        // This simulates in a small amount of code the behavior of Go's ideal constants.
        public partial struct NumberNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public bool IsInt; // Number has an integral value.
            public bool IsUint; // Number has an unsigned integral value.
            public bool IsFloat; // Number has a floating-point value.
            public bool IsComplex; // Number is complex.
            public long Int64; // The signed integer value.
            public ulong Uint64; // The unsigned integer value.
            public double Float64; // The floating-point value.
            public System.Numerics.Complex128 Complex128; // The complex value.
            public @string Text; // The original textual representation from the input.
        }

        private static (ref NumberNode, error) newNumber(this ref Tree t, Pos pos, @string text, itemType typ)
        {
            NumberNode n = ref new NumberNode(tr:t,NodeType:NodeNumber,Pos:pos,Text:text);

            if (typ == itemCharConstant) 
                var (rune, _, tail, err) = strconv.UnquoteChar(text[1L..], text[0L]);
                if (err != null)
                {
                    return (null, err);
                }
                if (tail != "'")
                {
                    return (null, fmt.Errorf("malformed character constant: %s", text));
                }
                n.Int64 = int64(rune);
                n.IsInt = true;
                n.Uint64 = uint64(rune);
                n.IsUint = true;
                n.Float64 = float64(rune); // odd but those are the rules.
                n.IsFloat = true;
                return (n, null);
            else if (typ == itemComplex) 
                // fmt.Sscan can parse the pair, so let it do the work.
                {
                    var (_, err) = fmt.Sscan(text, ref n.Complex128);

                    if (err != null)
                    {
                        return (null, err);
                    }

                }
                n.IsComplex = true;
                n.simplifyComplex();
                return (n, null);
            // Imaginary constants can only be complex unless they are zero.
            if (len(text) > 0L && text[len(text) - 1L] == 'i')
            {
                var (f, err) = strconv.ParseFloat(text[..len(text) - 1L], 64L);
                if (err == null)
                {
                    n.IsComplex = true;
                    n.Complex128 = complex(0L, f);
                    n.simplifyComplex();
                    return (n, null);
                }
            } 
            // Do integer test first so we get 0x123 etc.
            var (u, err) = strconv.ParseUint(text, 0L, 64L); // will fail for -0; fixed below.
            if (err == null)
            {
                n.IsUint = true;
                n.Uint64 = u;
            }
            var (i, err) = strconv.ParseInt(text, 0L, 64L);
            if (err == null)
            {
                n.IsInt = true;
                n.Int64 = i;
                if (i == 0L)
                {
                    n.IsUint = true; // in case of -0.
                    n.Uint64 = u;
                }
            } 
            // If an integer extraction succeeded, promote the float.
            if (n.IsInt)
            {
                n.IsFloat = true;
                n.Float64 = float64(n.Int64);
            }
            else if (n.IsUint)
            {
                n.IsFloat = true;
                n.Float64 = float64(n.Uint64);
            }
            else
            {
                (f, err) = strconv.ParseFloat(text, 64L);
                if (err == null)
                { 
                    // If we parsed it as a float but it looks like an integer,
                    // it's a huge number too large to fit in an int. Reject it.
                    if (!strings.ContainsAny(text, ".eE"))
                    {
                        return (null, fmt.Errorf("integer overflow: %q", text));
                    }
                    n.IsFloat = true;
                    n.Float64 = f; 
                    // If a floating-point extraction succeeded, extract the int if needed.
                    if (!n.IsInt && float64(int64(f)) == f)
                    {
                        n.IsInt = true;
                        n.Int64 = int64(f);
                    }
                    if (!n.IsUint && float64(uint64(f)) == f)
                    {
                        n.IsUint = true;
                        n.Uint64 = uint64(f);
                    }
                }
            }
            if (!n.IsInt && !n.IsUint && !n.IsFloat)
            {
                return (null, fmt.Errorf("illegal number syntax: %q", text));
            }
            return (n, null);
        }

        // simplifyComplex pulls out any other types that are represented by the complex number.
        // These all require that the imaginary part be zero.
        private static void simplifyComplex(this ref NumberNode n)
        {
            n.IsFloat = imag(n.Complex128) == 0L;
            if (n.IsFloat)
            {
                n.Float64 = real(n.Complex128);
                n.IsInt = float64(int64(n.Float64)) == n.Float64;
                if (n.IsInt)
                {
                    n.Int64 = int64(n.Float64);
                }
                n.IsUint = float64(uint64(n.Float64)) == n.Float64;
                if (n.IsUint)
                {
                    n.Uint64 = uint64(n.Float64);
                }
            }
        }

        private static @string String(this ref NumberNode n)
        {
            return n.Text;
        }

        private static ref Tree tree(this ref NumberNode n)
        {
            return n.tr;
        }

        private static Node Copy(this ref NumberNode n)
        {
            ptr<NumberNode> nn = @new<NumberNode>();
            nn.Value = n.Value; // Easy, fast, correct.
            return nn;
        }

        // StringNode holds a string constant. The value has been "unquoted".
        public partial struct StringNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public @string Quoted; // The original text of the string, with quotes.
            public @string Text; // The string, after quote processing.
        }

        private static ref StringNode newString(this ref Tree t, Pos pos, @string orig, @string text)
        {
            return ref new StringNode(tr:t,NodeType:NodeString,Pos:pos,Quoted:orig,Text:text);
        }

        private static @string String(this ref StringNode s)
        {
            return s.Quoted;
        }

        private static ref Tree tree(this ref StringNode s)
        {
            return s.tr;
        }

        private static Node Copy(this ref StringNode s)
        {
            return s.tr.newString(s.Pos, s.Quoted, s.Text);
        }

        // endNode represents an {{end}} action.
        // It does not appear in the final parse tree.
        private partial struct endNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
        }

        private static ref endNode newEnd(this ref Tree t, Pos pos)
        {
            return ref new endNode(tr:t,NodeType:nodeEnd,Pos:pos);
        }

        private static @string String(this ref endNode e)
        {
            return "{{end}}";
        }

        private static ref Tree tree(this ref endNode e)
        {
            return e.tr;
        }

        private static Node Copy(this ref endNode e)
        {
            return e.tr.newEnd(e.Pos);
        }

        // elseNode represents an {{else}} action. Does not appear in the final tree.
        private partial struct elseNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public long Line; // The line number in the input. Deprecated: Kept for compatibility.
        }

        private static ref elseNode newElse(this ref Tree t, Pos pos, long line)
        {
            return ref new elseNode(tr:t,NodeType:nodeElse,Pos:pos,Line:line);
        }

        private static NodeType Type(this ref elseNode e)
        {
            return nodeElse;
        }

        private static @string String(this ref elseNode e)
        {
            return "{{else}}";
        }

        private static ref Tree tree(this ref elseNode e)
        {
            return e.tr;
        }

        private static Node Copy(this ref elseNode e)
        {
            return e.tr.newElse(e.Pos, e.Line);
        }

        // BranchNode is the common representation of if, range, and with.
        public partial struct BranchNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public long Line; // The line number in the input. Deprecated: Kept for compatibility.
            public ptr<PipeNode> Pipe; // The pipeline to be evaluated.
            public ptr<ListNode> List; // What to execute if the value is non-empty.
            public ptr<ListNode> ElseList; // What to execute if the value is empty (nil if absent).
        }

        private static @string String(this ref BranchNode _b) => func(_b, (ref BranchNode b, Defer _, Panic panic, Recover __) =>
        {
            @string name = "";

            if (b.NodeType == NodeIf) 
                name = "if";
            else if (b.NodeType == NodeRange) 
                name = "range";
            else if (b.NodeType == NodeWith) 
                name = "with";
            else 
                panic("unknown branch type");
                        if (b.ElseList != null)
            {
                return fmt.Sprintf("{{%s %s}}%s{{else}}%s{{end}}", name, b.Pipe, b.List, b.ElseList);
            }
            return fmt.Sprintf("{{%s %s}}%s{{end}}", name, b.Pipe, b.List);
        });

        private static ref Tree tree(this ref BranchNode b)
        {
            return b.tr;
        }

        private static Node Copy(this ref BranchNode _b) => func(_b, (ref BranchNode b, Defer _, Panic panic, Recover __) =>
        {

            if (b.NodeType == NodeIf) 
                return b.tr.newIf(b.Pos, b.Line, b.Pipe, b.List, b.ElseList);
            else if (b.NodeType == NodeRange) 
                return b.tr.newRange(b.Pos, b.Line, b.Pipe, b.List, b.ElseList);
            else if (b.NodeType == NodeWith) 
                return b.tr.newWith(b.Pos, b.Line, b.Pipe, b.List, b.ElseList);
            else 
                panic("unknown branch type");
                    });

        // IfNode represents an {{if}} action and its commands.
        public partial struct IfNode
        {
            public ref BranchNode BranchNode => ref BranchNode_val;
        }

        private static ref IfNode newIf(this ref Tree t, Pos pos, long line, ref PipeNode pipe, ref ListNode list, ref ListNode elseList)
        {
            return ref new IfNode(BranchNode{tr:t,NodeType:NodeIf,Pos:pos,Line:line,Pipe:pipe,List:list,ElseList:elseList});
        }

        private static Node Copy(this ref IfNode i)
        {
            return i.tr.newIf(i.Pos, i.Line, i.Pipe.CopyPipe(), i.List.CopyList(), i.ElseList.CopyList());
        }

        // RangeNode represents a {{range}} action and its commands.
        public partial struct RangeNode
        {
            public ref BranchNode BranchNode => ref BranchNode_val;
        }

        private static ref RangeNode newRange(this ref Tree t, Pos pos, long line, ref PipeNode pipe, ref ListNode list, ref ListNode elseList)
        {
            return ref new RangeNode(BranchNode{tr:t,NodeType:NodeRange,Pos:pos,Line:line,Pipe:pipe,List:list,ElseList:elseList});
        }

        private static Node Copy(this ref RangeNode r)
        {
            return r.tr.newRange(r.Pos, r.Line, r.Pipe.CopyPipe(), r.List.CopyList(), r.ElseList.CopyList());
        }

        // WithNode represents a {{with}} action and its commands.
        public partial struct WithNode
        {
            public ref BranchNode BranchNode => ref BranchNode_val;
        }

        private static ref WithNode newWith(this ref Tree t, Pos pos, long line, ref PipeNode pipe, ref ListNode list, ref ListNode elseList)
        {
            return ref new WithNode(BranchNode{tr:t,NodeType:NodeWith,Pos:pos,Line:line,Pipe:pipe,List:list,ElseList:elseList});
        }

        private static Node Copy(this ref WithNode w)
        {
            return w.tr.newWith(w.Pos, w.Line, w.Pipe.CopyPipe(), w.List.CopyList(), w.ElseList.CopyList());
        }

        // TemplateNode represents a {{template}} action.
        public partial struct TemplateNode
        {
            public ref NodeType NodeType => ref NodeType_val;
            public ref Pos Pos => ref Pos_val;
            public ptr<Tree> tr;
            public long Line; // The line number in the input. Deprecated: Kept for compatibility.
            public @string Name; // The name of the template (unquoted).
            public ptr<PipeNode> Pipe; // The command to evaluate as dot for the template.
        }

        private static ref TemplateNode newTemplate(this ref Tree t, Pos pos, long line, @string name, ref PipeNode pipe)
        {
            return ref new TemplateNode(tr:t,NodeType:NodeTemplate,Pos:pos,Line:line,Name:name,Pipe:pipe);
        }

        private static @string String(this ref TemplateNode t)
        {
            if (t.Pipe == null)
            {
                return fmt.Sprintf("{{template %q}}", t.Name);
            }
            return fmt.Sprintf("{{template %q %s}}", t.Name, t.Pipe);
        }

        private static ref Tree tree(this ref TemplateNode t)
        {
            return t.tr;
        }

        private static Node Copy(this ref TemplateNode t)
        {
            return t.tr.newTemplate(t.Pos, t.Line, t.Name, t.Pipe.CopyPipe());
        }
    }
}}}
