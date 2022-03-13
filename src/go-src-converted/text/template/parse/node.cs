// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parse nodes.

// package parse -- go2cs converted at 2022 March 13 05:38:58 UTC
// import "text/template/parse" ==> using parse = go.text.template.parse_package
// Original source: C:\Program Files\Go\src\text\template\parse\node.go
namespace go.text.template;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

public static partial class parse_package {

private static @string textFormat = "%s"; // Changed to "%q" in tests for better error messages.

// A Node is an element in the parse tree. The interface is trivial.
// The interface contains an unexported method so that only
// types local to this package can satisfy it.
public partial interface Node {
    ptr<Tree> Type();
    ptr<Tree> String(); // Copy does a deep copy of the Node and all its components.
// To avoid type assertions, some XxxNodes also have specialized
// CopyXxx methods that return *XxxNode.
    ptr<Tree> Copy();
    ptr<Tree> Position(); // byte position of start of node in full original input string
// tree returns the containing *Tree.
// It is unexported so all implementations of Node are in this package.
    ptr<Tree> tree(); // writeTo writes the String output to the builder.
    ptr<Tree> writeTo(ptr<strings.Builder> _p0);
}

// NodeType identifies the type of a parse tree node.
public partial struct NodeType { // : nint
}

// Pos represents a byte position in the original input text from which
// this template was parsed.
public partial struct Pos { // : nint
}

public static Pos Position(this Pos p) {
    return p;
}

// Type returns itself and provides an easy default implementation
// for embedding in a Node. Embedded in all non-trivial Nodes.
public static NodeType Type(this NodeType t) {
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
public static readonly var NodeComment = 19; // A comment.

// Nodes.

// ListNode holds a sequence of nodes.
public partial struct ListNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public slice<Node> Nodes; // The element nodes in lexical order.
}

private static ptr<ListNode> newList(this ptr<Tree> _addr_t, Pos pos) {
    ref Tree t = ref _addr_t.val;

    return addr(new ListNode(tr:t,NodeType:NodeList,Pos:pos));
}

private static void append(this ptr<ListNode> _addr_l, Node n) {
    ref ListNode l = ref _addr_l.val;

    l.Nodes = append(l.Nodes, n);
}

private static ptr<Tree> tree(this ptr<ListNode> _addr_l) {
    ref ListNode l = ref _addr_l.val;

    return _addr_l.tr!;
}

private static @string String(this ptr<ListNode> _addr_l) {
    ref ListNode l = ref _addr_l.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    l.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<ListNode> _addr_l, ptr<strings.Builder> _addr_sb) {
    ref ListNode l = ref _addr_l.val;
    ref strings.Builder sb = ref _addr_sb.val;

    foreach (var (_, n) in l.Nodes) {
        n.writeTo(sb);
    }
}

private static ptr<ListNode> CopyList(this ptr<ListNode> _addr_l) {
    ref ListNode l = ref _addr_l.val;

    if (l == null) {
        return _addr_l!;
    }
    var n = l.tr.newList(l.Pos);
    foreach (var (_, elem) in l.Nodes) {
        n.append(elem.Copy());
    }    return _addr_n!;
}

private static Node Copy(this ptr<ListNode> _addr_l) {
    ref ListNode l = ref _addr_l.val;

    return l.CopyList();
}

// TextNode holds plain text.
public partial struct TextNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public slice<byte> Text; // The text; may span newlines.
}

private static ptr<TextNode> newText(this ptr<Tree> _addr_t, Pos pos, @string text) {
    ref Tree t = ref _addr_t.val;

    return addr(new TextNode(tr:t,NodeType:NodeText,Pos:pos,Text:[]byte(text)));
}

private static @string String(this ptr<TextNode> _addr_t) {
    ref TextNode t = ref _addr_t.val;

    return fmt.Sprintf(textFormat, t.Text);
}

private static void writeTo(this ptr<TextNode> _addr_t, ptr<strings.Builder> _addr_sb) {
    ref TextNode t = ref _addr_t.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString(t.String());
}

private static ptr<Tree> tree(this ptr<TextNode> _addr_t) {
    ref TextNode t = ref _addr_t.val;

    return _addr_t.tr!;
}

private static Node Copy(this ptr<TextNode> _addr_t) {
    ref TextNode t = ref _addr_t.val;

    return addr(new TextNode(tr:t.tr,NodeType:NodeText,Pos:t.Pos,Text:append([]byte{},t.Text...)));
}

// CommentNode holds a comment.
public partial struct CommentNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public @string Text; // Comment text.
}

private static ptr<CommentNode> newComment(this ptr<Tree> _addr_t, Pos pos, @string text) {
    ref Tree t = ref _addr_t.val;

    return addr(new CommentNode(tr:t,NodeType:NodeComment,Pos:pos,Text:text));
}

private static @string String(this ptr<CommentNode> _addr_c) {
    ref CommentNode c = ref _addr_c.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    c.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<CommentNode> _addr_c, ptr<strings.Builder> _addr_sb) {
    ref CommentNode c = ref _addr_c.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString("{{");
    sb.WriteString(c.Text);
    sb.WriteString("}}");
}

private static ptr<Tree> tree(this ptr<CommentNode> _addr_c) {
    ref CommentNode c = ref _addr_c.val;

    return _addr_c.tr!;
}

private static Node Copy(this ptr<CommentNode> _addr_c) {
    ref CommentNode c = ref _addr_c.val;

    return addr(new CommentNode(tr:c.tr,NodeType:NodeComment,Pos:c.Pos,Text:c.Text));
}

// PipeNode holds a pipeline with optional declaration
public partial struct PipeNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public nint Line; // The line number in the input. Deprecated: Kept for compatibility.
    public bool IsAssign; // The variables are being assigned, not declared.
    public slice<ptr<VariableNode>> Decl; // Variables in lexical order.
    public slice<ptr<CommandNode>> Cmds; // The commands in lexical order.
}

private static ptr<PipeNode> newPipeline(this ptr<Tree> _addr_t, Pos pos, nint line, slice<ptr<VariableNode>> vars) {
    ref Tree t = ref _addr_t.val;

    return addr(new PipeNode(tr:t,NodeType:NodePipe,Pos:pos,Line:line,Decl:vars));
}

private static void append(this ptr<PipeNode> _addr_p, ptr<CommandNode> _addr_command) {
    ref PipeNode p = ref _addr_p.val;
    ref CommandNode command = ref _addr_command.val;

    p.Cmds = append(p.Cmds, command);
}

private static @string String(this ptr<PipeNode> _addr_p) {
    ref PipeNode p = ref _addr_p.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    p.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<PipeNode> _addr_p, ptr<strings.Builder> _addr_sb) {
    ref PipeNode p = ref _addr_p.val;
    ref strings.Builder sb = ref _addr_sb.val;

    if (len(p.Decl) > 0) {
        {
            var i__prev1 = i;

            foreach (var (__i, __v) in p.Decl) {
                i = __i;
                v = __v;
                if (i > 0) {
                    sb.WriteString(", ");
                }
                v.writeTo(sb);
            }

            i = i__prev1;
        }

        sb.WriteString(" := ");
    }
    {
        var i__prev1 = i;

        foreach (var (__i, __c) in p.Cmds) {
            i = __i;
            c = __c;
            if (i > 0) {
                sb.WriteString(" | ");
            }
            c.writeTo(sb);
        }
        i = i__prev1;
    }
}

private static ptr<Tree> tree(this ptr<PipeNode> _addr_p) {
    ref PipeNode p = ref _addr_p.val;

    return _addr_p.tr!;
}

private static ptr<PipeNode> CopyPipe(this ptr<PipeNode> _addr_p) {
    ref PipeNode p = ref _addr_p.val;

    if (p == null) {
        return _addr_p!;
    }
    var vars = make_slice<ptr<VariableNode>>(len(p.Decl));
    foreach (var (i, d) in p.Decl) {
        vars[i] = d.Copy()._<ptr<VariableNode>>();
    }    var n = p.tr.newPipeline(p.Pos, p.Line, vars);
    n.IsAssign = p.IsAssign;
    foreach (var (_, c) in p.Cmds) {
        n.append(c.Copy()._<ptr<CommandNode>>());
    }    return _addr_n!;
}

private static Node Copy(this ptr<PipeNode> _addr_p) {
    ref PipeNode p = ref _addr_p.val;

    return p.CopyPipe();
}

// ActionNode holds an action (something bounded by delimiters).
// Control actions have their own nodes; ActionNode represents simple
// ones such as field evaluations and parenthesized pipelines.
public partial struct ActionNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public nint Line; // The line number in the input. Deprecated: Kept for compatibility.
    public ptr<PipeNode> Pipe; // The pipeline in the action.
}

private static ptr<ActionNode> newAction(this ptr<Tree> _addr_t, Pos pos, nint line, ptr<PipeNode> _addr_pipe) {
    ref Tree t = ref _addr_t.val;
    ref PipeNode pipe = ref _addr_pipe.val;

    return addr(new ActionNode(tr:t,NodeType:NodeAction,Pos:pos,Line:line,Pipe:pipe));
}

private static @string String(this ptr<ActionNode> _addr_a) {
    ref ActionNode a = ref _addr_a.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    a.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<ActionNode> _addr_a, ptr<strings.Builder> _addr_sb) {
    ref ActionNode a = ref _addr_a.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString("{{");
    a.Pipe.writeTo(sb);
    sb.WriteString("}}");
}

private static ptr<Tree> tree(this ptr<ActionNode> _addr_a) {
    ref ActionNode a = ref _addr_a.val;

    return _addr_a.tr!;
}

private static Node Copy(this ptr<ActionNode> _addr_a) {
    ref ActionNode a = ref _addr_a.val;

    return a.tr.newAction(a.Pos, a.Line, a.Pipe.CopyPipe());
}

// CommandNode holds a command (a pipeline inside an evaluating action).
public partial struct CommandNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public slice<Node> Args; // Arguments in lexical order: Identifier, field, or constant.
}

private static ptr<CommandNode> newCommand(this ptr<Tree> _addr_t, Pos pos) {
    ref Tree t = ref _addr_t.val;

    return addr(new CommandNode(tr:t,NodeType:NodeCommand,Pos:pos));
}

private static void append(this ptr<CommandNode> _addr_c, Node arg) {
    ref CommandNode c = ref _addr_c.val;

    c.Args = append(c.Args, arg);
}

private static @string String(this ptr<CommandNode> _addr_c) {
    ref CommandNode c = ref _addr_c.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    c.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<CommandNode> _addr_c, ptr<strings.Builder> _addr_sb) {
    ref CommandNode c = ref _addr_c.val;
    ref strings.Builder sb = ref _addr_sb.val;

    {
        var arg__prev1 = arg;

        foreach (var (__i, __arg) in c.Args) {
            i = __i;
            arg = __arg;
            if (i > 0) {
                sb.WriteByte(' ');
            }
            {
                var arg__prev1 = arg;

                ptr<PipeNode> (arg, ok) = arg._<ptr<PipeNode>>();

                if (ok) {
                    sb.WriteByte('(');
                    arg.writeTo(sb);
                    sb.WriteByte(')');
                    continue;
                }

                arg = arg__prev1;

            }
            arg.writeTo(sb);
        }
        arg = arg__prev1;
    }
}

private static ptr<Tree> tree(this ptr<CommandNode> _addr_c) {
    ref CommandNode c = ref _addr_c.val;

    return _addr_c.tr!;
}

private static Node Copy(this ptr<CommandNode> _addr_c) {
    ref CommandNode c = ref _addr_c.val;

    if (c == null) {
        return c;
    }
    var n = c.tr.newCommand(c.Pos);
    foreach (var (_, c) in c.Args) {
        n.append(c.Copy());
    }    return n;
}

// IdentifierNode holds an identifier.
public partial struct IdentifierNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public @string Ident; // The identifier's name.
}

// NewIdentifier returns a new IdentifierNode with the given identifier name.
public static ptr<IdentifierNode> NewIdentifier(@string ident) {
    return addr(new IdentifierNode(NodeType:NodeIdentifier,Ident:ident));
}

// SetPos sets the position. NewIdentifier is a public method so we can't modify its signature.
// Chained for convenience.
// TODO: fix one day?
private static ptr<IdentifierNode> SetPos(this ptr<IdentifierNode> _addr_i, Pos pos) {
    ref IdentifierNode i = ref _addr_i.val;

    i.Pos = pos;
    return _addr_i!;
}

// SetTree sets the parent tree for the node. NewIdentifier is a public method so we can't modify its signature.
// Chained for convenience.
// TODO: fix one day?
private static ptr<IdentifierNode> SetTree(this ptr<IdentifierNode> _addr_i, ptr<Tree> _addr_t) {
    ref IdentifierNode i = ref _addr_i.val;
    ref Tree t = ref _addr_t.val;

    i.tr = t;
    return _addr_i!;
}

private static @string String(this ptr<IdentifierNode> _addr_i) {
    ref IdentifierNode i = ref _addr_i.val;

    return i.Ident;
}

private static void writeTo(this ptr<IdentifierNode> _addr_i, ptr<strings.Builder> _addr_sb) {
    ref IdentifierNode i = ref _addr_i.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString(i.String());
}

private static ptr<Tree> tree(this ptr<IdentifierNode> _addr_i) {
    ref IdentifierNode i = ref _addr_i.val;

    return _addr_i.tr!;
}

private static Node Copy(this ptr<IdentifierNode> _addr_i) {
    ref IdentifierNode i = ref _addr_i.val;

    return NewIdentifier(i.Ident).SetTree(i.tr).SetPos(i.Pos);
}

// VariableNode holds a list of variable names, possibly with chained field
// accesses. The dollar sign is part of the (first) name.
public partial struct VariableNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public slice<@string> Ident; // Variable name and fields in lexical order.
}

private static ptr<VariableNode> newVariable(this ptr<Tree> _addr_t, Pos pos, @string ident) {
    ref Tree t = ref _addr_t.val;

    return addr(new VariableNode(tr:t,NodeType:NodeVariable,Pos:pos,Ident:strings.Split(ident,".")));
}

private static @string String(this ptr<VariableNode> _addr_v) {
    ref VariableNode v = ref _addr_v.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    v.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<VariableNode> _addr_v, ptr<strings.Builder> _addr_sb) {
    ref VariableNode v = ref _addr_v.val;
    ref strings.Builder sb = ref _addr_sb.val;

    foreach (var (i, id) in v.Ident) {
        if (i > 0) {
            sb.WriteByte('.');
        }
        sb.WriteString(id);
    }
}

private static ptr<Tree> tree(this ptr<VariableNode> _addr_v) {
    ref VariableNode v = ref _addr_v.val;

    return _addr_v.tr!;
}

private static Node Copy(this ptr<VariableNode> _addr_v) {
    ref VariableNode v = ref _addr_v.val;

    return addr(new VariableNode(tr:v.tr,NodeType:NodeVariable,Pos:v.Pos,Ident:append([]string{},v.Ident...)));
}

// DotNode holds the special identifier '.'.
public partial struct DotNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
}

private static ptr<DotNode> newDot(this ptr<Tree> _addr_t, Pos pos) {
    ref Tree t = ref _addr_t.val;

    return addr(new DotNode(tr:t,NodeType:NodeDot,Pos:pos));
}

private static NodeType Type(this ptr<DotNode> _addr_d) {
    ref DotNode d = ref _addr_d.val;
 
    // Override method on embedded NodeType for API compatibility.
    // TODO: Not really a problem; could change API without effect but
    // api tool complains.
    return NodeDot;
}

private static @string String(this ptr<DotNode> _addr_d) {
    ref DotNode d = ref _addr_d.val;

    return ".";
}

private static void writeTo(this ptr<DotNode> _addr_d, ptr<strings.Builder> _addr_sb) {
    ref DotNode d = ref _addr_d.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString(d.String());
}

private static ptr<Tree> tree(this ptr<DotNode> _addr_d) {
    ref DotNode d = ref _addr_d.val;

    return _addr_d.tr!;
}

private static Node Copy(this ptr<DotNode> _addr_d) {
    ref DotNode d = ref _addr_d.val;

    return d.tr.newDot(d.Pos);
}

// NilNode holds the special identifier 'nil' representing an untyped nil constant.
public partial struct NilNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
}

private static ptr<NilNode> newNil(this ptr<Tree> _addr_t, Pos pos) {
    ref Tree t = ref _addr_t.val;

    return addr(new NilNode(tr:t,NodeType:NodeNil,Pos:pos));
}

private static NodeType Type(this ptr<NilNode> _addr_n) {
    ref NilNode n = ref _addr_n.val;
 
    // Override method on embedded NodeType for API compatibility.
    // TODO: Not really a problem; could change API without effect but
    // api tool complains.
    return NodeNil;
}

private static @string String(this ptr<NilNode> _addr_n) {
    ref NilNode n = ref _addr_n.val;

    return "nil";
}

private static void writeTo(this ptr<NilNode> _addr_n, ptr<strings.Builder> _addr_sb) {
    ref NilNode n = ref _addr_n.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString(n.String());
}

private static ptr<Tree> tree(this ptr<NilNode> _addr_n) {
    ref NilNode n = ref _addr_n.val;

    return _addr_n.tr!;
}

private static Node Copy(this ptr<NilNode> _addr_n) {
    ref NilNode n = ref _addr_n.val;

    return n.tr.newNil(n.Pos);
}

// FieldNode holds a field (identifier starting with '.').
// The names may be chained ('.x.y').
// The period is dropped from each ident.
public partial struct FieldNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public slice<@string> Ident; // The identifiers in lexical order.
}

private static ptr<FieldNode> newField(this ptr<Tree> _addr_t, Pos pos, @string ident) {
    ref Tree t = ref _addr_t.val;

    return addr(new FieldNode(tr:t,NodeType:NodeField,Pos:pos,Ident:strings.Split(ident[1:],"."))); // [1:] to drop leading period
}

private static @string String(this ptr<FieldNode> _addr_f) {
    ref FieldNode f = ref _addr_f.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    f.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<FieldNode> _addr_f, ptr<strings.Builder> _addr_sb) {
    ref FieldNode f = ref _addr_f.val;
    ref strings.Builder sb = ref _addr_sb.val;

    foreach (var (_, id) in f.Ident) {
        sb.WriteByte('.');
        sb.WriteString(id);
    }
}

private static ptr<Tree> tree(this ptr<FieldNode> _addr_f) {
    ref FieldNode f = ref _addr_f.val;

    return _addr_f.tr!;
}

private static Node Copy(this ptr<FieldNode> _addr_f) {
    ref FieldNode f = ref _addr_f.val;

    return addr(new FieldNode(tr:f.tr,NodeType:NodeField,Pos:f.Pos,Ident:append([]string{},f.Ident...)));
}

// ChainNode holds a term followed by a chain of field accesses (identifier starting with '.').
// The names may be chained ('.x.y').
// The periods are dropped from each ident.
public partial struct ChainNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public Node Node;
    public slice<@string> Field; // The identifiers in lexical order.
}

private static ptr<ChainNode> newChain(this ptr<Tree> _addr_t, Pos pos, Node node) {
    ref Tree t = ref _addr_t.val;

    return addr(new ChainNode(tr:t,NodeType:NodeChain,Pos:pos,Node:node));
}

// Add adds the named field (which should start with a period) to the end of the chain.
private static void Add(this ptr<ChainNode> _addr_c, @string field) => func((_, panic, _) => {
    ref ChainNode c = ref _addr_c.val;

    if (len(field) == 0 || field[0] != '.') {
        panic("no dot in field");
    }
    field = field[(int)1..]; // Remove leading dot.
    if (field == "") {
        panic("empty field");
    }
    c.Field = append(c.Field, field);
});

private static @string String(this ptr<ChainNode> _addr_c) {
    ref ChainNode c = ref _addr_c.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    c.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<ChainNode> _addr_c, ptr<strings.Builder> _addr_sb) {
    ref ChainNode c = ref _addr_c.val;
    ref strings.Builder sb = ref _addr_sb.val;

    {
        ptr<PipeNode> (_, ok) = c.Node._<ptr<PipeNode>>();

        if (ok) {
            sb.WriteByte('(');
            c.Node.writeTo(sb);
            sb.WriteByte(')');
        }
        else
 {
            c.Node.writeTo(sb);
        }
    }
    foreach (var (_, field) in c.Field) {
        sb.WriteByte('.');
        sb.WriteString(field);
    }
}

private static ptr<Tree> tree(this ptr<ChainNode> _addr_c) {
    ref ChainNode c = ref _addr_c.val;

    return _addr_c.tr!;
}

private static Node Copy(this ptr<ChainNode> _addr_c) {
    ref ChainNode c = ref _addr_c.val;

    return addr(new ChainNode(tr:c.tr,NodeType:NodeChain,Pos:c.Pos,Node:c.Node,Field:append([]string{},c.Field...)));
}

// BoolNode holds a boolean constant.
public partial struct BoolNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public bool True; // The value of the boolean constant.
}

private static ptr<BoolNode> newBool(this ptr<Tree> _addr_t, Pos pos, bool @true) {
    ref Tree t = ref _addr_t.val;

    return addr(new BoolNode(tr:t,NodeType:NodeBool,Pos:pos,True:true));
}

private static @string String(this ptr<BoolNode> _addr_b) {
    ref BoolNode b = ref _addr_b.val;

    if (b.True) {
        return "true";
    }
    return "false";
}

private static void writeTo(this ptr<BoolNode> _addr_b, ptr<strings.Builder> _addr_sb) {
    ref BoolNode b = ref _addr_b.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString(b.String());
}

private static ptr<Tree> tree(this ptr<BoolNode> _addr_b) {
    ref BoolNode b = ref _addr_b.val;

    return _addr_b.tr!;
}

private static Node Copy(this ptr<BoolNode> _addr_b) {
    ref BoolNode b = ref _addr_b.val;

    return b.tr.newBool(b.Pos, b.True);
}

// NumberNode holds a number: signed or unsigned integer, float, or complex.
// The value is parsed and stored under all the types that can represent the value.
// This simulates in a small amount of code the behavior of Go's ideal constants.
public partial struct NumberNode {
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

private static (ptr<NumberNode>, error) newNumber(this ptr<Tree> _addr_t, Pos pos, @string text, itemType typ) {
    ptr<NumberNode> _p0 = default!;
    error _p0 = default!;
    ref Tree t = ref _addr_t.val;

    ptr<NumberNode> n = addr(new NumberNode(tr:t,NodeType:NodeNumber,Pos:pos,Text:text));

    if (typ == itemCharConstant) 
        var (rune, _, tail, err) = strconv.UnquoteChar(text[(int)1..], text[0]);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        if (tail != "'") {
            return (_addr_null!, error.As(fmt.Errorf("malformed character constant: %s", text))!);
        }
        n.Int64 = int64(rune);
        n.IsInt = true;
        n.Uint64 = uint64(rune);
        n.IsUint = true;
        n.Float64 = float64(rune); // odd but those are the rules.
        n.IsFloat = true;
        return (_addr_n!, error.As(null!)!);
    else if (typ == itemComplex) 
        // fmt.Sscan can parse the pair, so let it do the work.
        {
            var (_, err) = fmt.Sscan(text, _addr_n.Complex128);

            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }

        }
        n.IsComplex = true;
        n.simplifyComplex();
        return (_addr_n!, error.As(null!)!);
    // Imaginary constants can only be complex unless they are zero.
    if (len(text) > 0 && text[len(text) - 1] == 'i') {
        var (f, err) = strconv.ParseFloat(text[..(int)len(text) - 1], 64);
        if (err == null) {
            n.IsComplex = true;
            n.Complex128 = complex(0, f);
            n.simplifyComplex();
            return (_addr_n!, error.As(null!)!);
        }
    }
    var (u, err) = strconv.ParseUint(text, 0, 64); // will fail for -0; fixed below.
    if (err == null) {
        n.IsUint = true;
        n.Uint64 = u;
    }
    var (i, err) = strconv.ParseInt(text, 0, 64);
    if (err == null) {
        n.IsInt = true;
        n.Int64 = i;
        if (i == 0) {
            n.IsUint = true; // in case of -0.
            n.Uint64 = u;
        }
    }
    if (n.IsInt) {
        n.IsFloat = true;
        n.Float64 = float64(n.Int64);
    }
    else if (n.IsUint) {
        n.IsFloat = true;
        n.Float64 = float64(n.Uint64);
    }
    else
 {
        (f, err) = strconv.ParseFloat(text, 64);
        if (err == null) { 
            // If we parsed it as a float but it looks like an integer,
            // it's a huge number too large to fit in an int. Reject it.
            if (!strings.ContainsAny(text, ".eEpP")) {
                return (_addr_null!, error.As(fmt.Errorf("integer overflow: %q", text))!);
            }
            n.IsFloat = true;
            n.Float64 = f; 
            // If a floating-point extraction succeeded, extract the int if needed.
            if (!n.IsInt && float64(int64(f)) == f) {
                n.IsInt = true;
                n.Int64 = int64(f);
            }
            if (!n.IsUint && float64(uint64(f)) == f) {
                n.IsUint = true;
                n.Uint64 = uint64(f);
            }
        }
    }
    if (!n.IsInt && !n.IsUint && !n.IsFloat) {
        return (_addr_null!, error.As(fmt.Errorf("illegal number syntax: %q", text))!);
    }
    return (_addr_n!, error.As(null!)!);
}

// simplifyComplex pulls out any other types that are represented by the complex number.
// These all require that the imaginary part be zero.
private static void simplifyComplex(this ptr<NumberNode> _addr_n) {
    ref NumberNode n = ref _addr_n.val;

    n.IsFloat = imag(n.Complex128) == 0;
    if (n.IsFloat) {
        n.Float64 = real(n.Complex128);
        n.IsInt = float64(int64(n.Float64)) == n.Float64;
        if (n.IsInt) {
            n.Int64 = int64(n.Float64);
        }
        n.IsUint = float64(uint64(n.Float64)) == n.Float64;
        if (n.IsUint) {
            n.Uint64 = uint64(n.Float64);
        }
    }
}

private static @string String(this ptr<NumberNode> _addr_n) {
    ref NumberNode n = ref _addr_n.val;

    return n.Text;
}

private static void writeTo(this ptr<NumberNode> _addr_n, ptr<strings.Builder> _addr_sb) {
    ref NumberNode n = ref _addr_n.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString(n.String());
}

private static ptr<Tree> tree(this ptr<NumberNode> _addr_n) {
    ref NumberNode n = ref _addr_n.val;

    return _addr_n.tr!;
}

private static Node Copy(this ptr<NumberNode> _addr_n) {
    ref NumberNode n = ref _addr_n.val;

    ptr<NumberNode> nn = @new<NumberNode>();
    nn.val = n.val; // Easy, fast, correct.
    return nn;
}

// StringNode holds a string constant. The value has been "unquoted".
public partial struct StringNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public @string Quoted; // The original text of the string, with quotes.
    public @string Text; // The string, after quote processing.
}

private static ptr<StringNode> newString(this ptr<Tree> _addr_t, Pos pos, @string orig, @string text) {
    ref Tree t = ref _addr_t.val;

    return addr(new StringNode(tr:t,NodeType:NodeString,Pos:pos,Quoted:orig,Text:text));
}

private static @string String(this ptr<StringNode> _addr_s) {
    ref StringNode s = ref _addr_s.val;

    return s.Quoted;
}

private static void writeTo(this ptr<StringNode> _addr_s, ptr<strings.Builder> _addr_sb) {
    ref StringNode s = ref _addr_s.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString(s.String());
}

private static ptr<Tree> tree(this ptr<StringNode> _addr_s) {
    ref StringNode s = ref _addr_s.val;

    return _addr_s.tr!;
}

private static Node Copy(this ptr<StringNode> _addr_s) {
    ref StringNode s = ref _addr_s.val;

    return s.tr.newString(s.Pos, s.Quoted, s.Text);
}

// endNode represents an {{end}} action.
// It does not appear in the final parse tree.
private partial struct endNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
}

private static ptr<endNode> newEnd(this ptr<Tree> _addr_t, Pos pos) {
    ref Tree t = ref _addr_t.val;

    return addr(new endNode(tr:t,NodeType:nodeEnd,Pos:pos));
}

private static @string String(this ptr<endNode> _addr_e) {
    ref endNode e = ref _addr_e.val;

    return "{{end}}";
}

private static void writeTo(this ptr<endNode> _addr_e, ptr<strings.Builder> _addr_sb) {
    ref endNode e = ref _addr_e.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString(e.String());
}

private static ptr<Tree> tree(this ptr<endNode> _addr_e) {
    ref endNode e = ref _addr_e.val;

    return _addr_e.tr!;
}

private static Node Copy(this ptr<endNode> _addr_e) {
    ref endNode e = ref _addr_e.val;

    return e.tr.newEnd(e.Pos);
}

// elseNode represents an {{else}} action. Does not appear in the final tree.
private partial struct elseNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public nint Line; // The line number in the input. Deprecated: Kept for compatibility.
}

private static ptr<elseNode> newElse(this ptr<Tree> _addr_t, Pos pos, nint line) {
    ref Tree t = ref _addr_t.val;

    return addr(new elseNode(tr:t,NodeType:nodeElse,Pos:pos,Line:line));
}

private static NodeType Type(this ptr<elseNode> _addr_e) {
    ref elseNode e = ref _addr_e.val;

    return nodeElse;
}

private static @string String(this ptr<elseNode> _addr_e) {
    ref elseNode e = ref _addr_e.val;

    return "{{else}}";
}

private static void writeTo(this ptr<elseNode> _addr_e, ptr<strings.Builder> _addr_sb) {
    ref elseNode e = ref _addr_e.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString(e.String());
}

private static ptr<Tree> tree(this ptr<elseNode> _addr_e) {
    ref elseNode e = ref _addr_e.val;

    return _addr_e.tr!;
}

private static Node Copy(this ptr<elseNode> _addr_e) {
    ref elseNode e = ref _addr_e.val;

    return e.tr.newElse(e.Pos, e.Line);
}

// BranchNode is the common representation of if, range, and with.
public partial struct BranchNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public nint Line; // The line number in the input. Deprecated: Kept for compatibility.
    public ptr<PipeNode> Pipe; // The pipeline to be evaluated.
    public ptr<ListNode> List; // What to execute if the value is non-empty.
    public ptr<ListNode> ElseList; // What to execute if the value is empty (nil if absent).
}

private static @string String(this ptr<BranchNode> _addr_b) {
    ref BranchNode b = ref _addr_b.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    b.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<BranchNode> _addr_b, ptr<strings.Builder> _addr_sb) => func((_, panic, _) => {
    ref BranchNode b = ref _addr_b.val;
    ref strings.Builder sb = ref _addr_sb.val;

    @string name = "";

    if (b.NodeType == NodeIf) 
        name = "if";
    else if (b.NodeType == NodeRange) 
        name = "range";
    else if (b.NodeType == NodeWith) 
        name = "with";
    else 
        panic("unknown branch type");
        sb.WriteString("{{");
    sb.WriteString(name);
    sb.WriteByte(' ');
    b.Pipe.writeTo(sb);
    sb.WriteString("}}");
    b.List.writeTo(sb);
    if (b.ElseList != null) {
        sb.WriteString("{{else}}");
        b.ElseList.writeTo(sb);
    }
    sb.WriteString("{{end}}");
});

private static ptr<Tree> tree(this ptr<BranchNode> _addr_b) {
    ref BranchNode b = ref _addr_b.val;

    return _addr_b.tr!;
}

private static Node Copy(this ptr<BranchNode> _addr_b) => func((_, panic, _) => {
    ref BranchNode b = ref _addr_b.val;


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
public partial struct IfNode {
    public ref BranchNode BranchNode => ref BranchNode_val;
}

private static ptr<IfNode> newIf(this ptr<Tree> _addr_t, Pos pos, nint line, ptr<PipeNode> _addr_pipe, ptr<ListNode> _addr_list, ptr<ListNode> _addr_elseList) {
    ref Tree t = ref _addr_t.val;
    ref PipeNode pipe = ref _addr_pipe.val;
    ref ListNode list = ref _addr_list.val;
    ref ListNode elseList = ref _addr_elseList.val;

    return addr(new IfNode(BranchNode{tr:t,NodeType:NodeIf,Pos:pos,Line:line,Pipe:pipe,List:list,ElseList:elseList}));
}

private static Node Copy(this ptr<IfNode> _addr_i) {
    ref IfNode i = ref _addr_i.val;

    return i.tr.newIf(i.Pos, i.Line, i.Pipe.CopyPipe(), i.List.CopyList(), i.ElseList.CopyList());
}

// RangeNode represents a {{range}} action and its commands.
public partial struct RangeNode {
    public ref BranchNode BranchNode => ref BranchNode_val;
}

private static ptr<RangeNode> newRange(this ptr<Tree> _addr_t, Pos pos, nint line, ptr<PipeNode> _addr_pipe, ptr<ListNode> _addr_list, ptr<ListNode> _addr_elseList) {
    ref Tree t = ref _addr_t.val;
    ref PipeNode pipe = ref _addr_pipe.val;
    ref ListNode list = ref _addr_list.val;
    ref ListNode elseList = ref _addr_elseList.val;

    return addr(new RangeNode(BranchNode{tr:t,NodeType:NodeRange,Pos:pos,Line:line,Pipe:pipe,List:list,ElseList:elseList}));
}

private static Node Copy(this ptr<RangeNode> _addr_r) {
    ref RangeNode r = ref _addr_r.val;

    return r.tr.newRange(r.Pos, r.Line, r.Pipe.CopyPipe(), r.List.CopyList(), r.ElseList.CopyList());
}

// WithNode represents a {{with}} action and its commands.
public partial struct WithNode {
    public ref BranchNode BranchNode => ref BranchNode_val;
}

private static ptr<WithNode> newWith(this ptr<Tree> _addr_t, Pos pos, nint line, ptr<PipeNode> _addr_pipe, ptr<ListNode> _addr_list, ptr<ListNode> _addr_elseList) {
    ref Tree t = ref _addr_t.val;
    ref PipeNode pipe = ref _addr_pipe.val;
    ref ListNode list = ref _addr_list.val;
    ref ListNode elseList = ref _addr_elseList.val;

    return addr(new WithNode(BranchNode{tr:t,NodeType:NodeWith,Pos:pos,Line:line,Pipe:pipe,List:list,ElseList:elseList}));
}

private static Node Copy(this ptr<WithNode> _addr_w) {
    ref WithNode w = ref _addr_w.val;

    return w.tr.newWith(w.Pos, w.Line, w.Pipe.CopyPipe(), w.List.CopyList(), w.ElseList.CopyList());
}

// TemplateNode represents a {{template}} action.
public partial struct TemplateNode {
    public ref NodeType NodeType => ref NodeType_val;
    public ref Pos Pos => ref Pos_val;
    public ptr<Tree> tr;
    public nint Line; // The line number in the input. Deprecated: Kept for compatibility.
    public @string Name; // The name of the template (unquoted).
    public ptr<PipeNode> Pipe; // The command to evaluate as dot for the template.
}

private static ptr<TemplateNode> newTemplate(this ptr<Tree> _addr_t, Pos pos, nint line, @string name, ptr<PipeNode> _addr_pipe) {
    ref Tree t = ref _addr_t.val;
    ref PipeNode pipe = ref _addr_pipe.val;

    return addr(new TemplateNode(tr:t,NodeType:NodeTemplate,Pos:pos,Line:line,Name:name,Pipe:pipe));
}

private static @string String(this ptr<TemplateNode> _addr_t) {
    ref TemplateNode t = ref _addr_t.val;

    ref strings.Builder sb = ref heap(out ptr<strings.Builder> _addr_sb);
    t.writeTo(_addr_sb);
    return sb.String();
}

private static void writeTo(this ptr<TemplateNode> _addr_t, ptr<strings.Builder> _addr_sb) {
    ref TemplateNode t = ref _addr_t.val;
    ref strings.Builder sb = ref _addr_sb.val;

    sb.WriteString("{{template ");
    sb.WriteString(strconv.Quote(t.Name));
    if (t.Pipe != null) {
        sb.WriteByte(' ');
        t.Pipe.writeTo(sb);
    }
    sb.WriteString("}}");
}

private static ptr<Tree> tree(this ptr<TemplateNode> _addr_t) {
    ref TemplateNode t = ref _addr_t.val;

    return _addr_t.tr!;
}

private static Node Copy(this ptr<TemplateNode> _addr_t) {
    ref TemplateNode t = ref _addr_t.val;

    return t.tr.newTemplate(t.Pos, t.Line, t.Name, t.Pipe.CopyPipe());
}

} // end parse_package
