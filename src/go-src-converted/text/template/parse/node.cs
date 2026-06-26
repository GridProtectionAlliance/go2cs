// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Parse nodes.
namespace go.text.template;

using fmt = fmt_package;
using strconv = strconv_package;
using strings = strings_package;

partial class parse_package {

internal static @string textFormat = "%s"u8; // Changed to "%q" in tests for better error messages.

// A Node is an element in the parse tree. The interface is trivial.
// The interface contains an unexported method so that only
// types local to this package can satisfy it.
[GoType] partial interface Node {
    NodeType Type();
    @string String();
    // Copy does a deep copy of the Node and all its components.
    // To avoid type assertions, some XxxNodes also have specialized
    // CopyXxx methods that return *XxxNode.
    Node Copy();
    Pos Position(); // byte position of start of node in full original input string
    // tree returns the containing *Tree.
    // It is unexported so all implementations of Node are in this package.
    ж<Tree> tree();
    // writeTo writes the String output to the builder.
    void writeTo(ж<strings.Builder> _);
}

[GoType("num:nint")] partial struct NodeType;

[GoType("num:nint")] partial struct Pos;

public static Pos Position(this Pos p) {
    return p;
}

// Type returns itself and provides an easy default implementation
// for embedding in a Node. Embedded in all non-trivial Nodes.
public static NodeType Type(this NodeType t) {
    return t;
}

public static readonly NodeType NodeText = /* iota */ 0;        // Plain text.
public static readonly NodeType NodeAction = 1;      // A non-control action such as a field evaluation.
public static readonly NodeType NodeBool = 2;        // A boolean constant.
public static readonly NodeType NodeChain = 3;       // A sequence of field accesses.
public static readonly NodeType NodeCommand = 4;     // An element of a pipeline.
public static readonly NodeType NodeDot = 5;         // The cursor, dot.
internal static readonly NodeType nodeElse = 6;      // An else action. Not added to tree.
internal static readonly NodeType nodeEnd = 7;       // An end action. Not added to tree.
public static readonly NodeType NodeField = 8;       // A field or method name.
public static readonly NodeType NodeIdentifier = 9;  // An identifier; always a function name.
public static readonly NodeType NodeIf = 10;          // An if action.
public static readonly NodeType NodeList = 11;        // A list of Nodes.
public static readonly NodeType NodeNil = 12;         // An untyped nil constant.
public static readonly NodeType NodeNumber = 13;      // A numerical constant.
public static readonly NodeType NodePipe = 14;        // A pipeline of commands.
public static readonly NodeType NodeRange = 15;       // A range action.
public static readonly NodeType NodeString = 16;      // A string constant.
public static readonly NodeType NodeTemplate = 17;    // A template invocation action.
public static readonly NodeType NodeVariable = 18;    // A $ variable.
public static readonly NodeType NodeWith = 19;        // A with action.
public static readonly NodeType NodeComment = 20;     // A comment.
public static readonly NodeType NodeBreak = 21;       // A break action.
public static readonly NodeType NodeContinue = 22;    // A continue action.

// Nodes.

// ListNode holds a sequence of nodes.
[GoType] partial struct ListNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public slice<Node> Nodes; // The element nodes in lexical order.
}

[GoRecv] internal static ж<ListNode> newList(this ref Tree t, Pos pos) {
    return Ꮡ(new ListNode(tr: t, NodeType: NodeList, Pos: pos));
}

[GoRecv] internal static void append(this ref ListNode l, Node n) {
    l.Nodes = append(l.Nodes, n);
}

[GoRecv] internal static ж<Tree> tree(this ref ListNode l) {
    return l.tr;
}

[GoRecv] public static @string String(this ref ListNode l) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    l.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref ListNode l, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    foreach (var (_, n) in l.Nodes) {
        n.writeTo(Ꮡsb);
    }
}

[GoRecv("capture")] public static ж<ListNode> CopyList(this ref ListNode l) {
    if (l == nil) {
        return CopyListꓸᏑl;
    }
    var n = l.tr.newList(l.Pos);
    foreach (var (_, elem) in l.Nodes) {
        n.append(elem.Copy());
    }
    return n;
}

[GoRecv] public static Node Copy(this ref ListNode l) {
    return ~l.CopyList();
}

// TextNode holds plain text.
[GoType] partial struct TextNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public slice<byte> Text; // The text; may span newlines.
}

[GoRecv] internal static ж<TextNode> newText(this ref Tree t, Pos pos, @string text) {
    return Ꮡ(new TextNode(tr: t, NodeType: NodeText, Pos: pos, Text: slice<byte>(text)));
}

[GoRecv] public static @string String(this ref TextNode t) {
    return fmt.Sprintf(textFormat, t.Text);
}

[GoRecv] public static void writeTo(this ref TextNode t, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString(t.String());
}

[GoRecv] internal static ж<Tree> tree(this ref TextNode t) {
    return t.tr;
}

[GoRecv] public static Node Copy(this ref TextNode t) {
    return new TextNode(tr: t.tr, NodeType: NodeText, Pos: t.Pos, Text: append(new byte[]{}.slice(), t.Text.ꓸꓸꓸ));
}

// CommentNode holds a comment.
[GoType] partial struct CommentNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public @string Text; // Comment text.
}

[GoRecv] internal static ж<CommentNode> newComment(this ref Tree t, Pos pos, @string text) {
    return Ꮡ(new CommentNode(tr: t, NodeType: NodeComment, Pos: pos, Text: text));
}

[GoRecv] public static @string String(this ref CommentNode c) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    c.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref CommentNode c, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString("{{"u8);
    sb.WriteString(c.Text);
    sb.WriteString("}}"u8);
}

[GoRecv] internal static ж<Tree> tree(this ref CommentNode c) {
    return c.tr;
}

[GoRecv] public static Node Copy(this ref CommentNode c) {
    return new CommentNode(tr: c.tr, NodeType: NodeComment, Pos: c.Pos, Text: c.Text);
}

// PipeNode holds a pipeline with optional declaration
[GoType] partial struct PipeNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public nint Line;            // The line number in the input. Deprecated: Kept for compatibility.
    public bool IsAssign;            // The variables are being assigned, not declared.
    public slice<ж<VariableNode>> Decl; // Variables in lexical order.
    public slice<ж<CommandNode>> Cmds; // The commands in lexical order.
}

[GoRecv] internal static ж<PipeNode> newPipeline(this ref Tree t, Pos pos, nint line, slice<ж<VariableNode>> vars) {
    return Ꮡ(new PipeNode(tr: t, NodeType: NodePipe, Pos: pos, Line: line, Decl: vars));
}

[GoRecv] public static void append(this ref PipeNode p, ж<CommandNode> Ꮡcommand) {
    ref var command = ref Ꮡcommand.val;

    p.Cmds = append(p.Cmds, Ꮡcommand);
}

[GoRecv] public static @string String(this ref PipeNode p) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    p.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref PipeNode p, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    if (len(p.Decl) > 0) {
        foreach (var (i, v) in p.Decl) {
            if (i > 0) {
                sb.WriteString(", "u8);
            }
            v.writeTo(Ꮡsb);
        }
        if (p.IsAssign){
            sb.WriteString(" = "u8);
        } else {
            sb.WriteString(" := "u8);
        }
    }
    foreach (var (i, c) in p.Cmds) {
        if (i > 0) {
            sb.WriteString(" | "u8);
        }
        c.writeTo(Ꮡsb);
    }
}

[GoRecv] internal static ж<Tree> tree(this ref PipeNode p) {
    return p.tr;
}

[GoRecv("capture")] public static ж<PipeNode> CopyPipe(this ref PipeNode p) {
    if (p == nil) {
        return CopyPipeꓸᏑp;
    }
    var vars = new slice<ж<VariableNode>>(len(p.Decl));
    foreach (var (i, d) in p.Decl) {
        vars[i] = d.Copy()._<VariableNode.val>();
    }
    var n = p.tr.newPipeline(p.Pos, p.Line, vars);
    n.val.IsAssign = p.IsAssign;
    foreach (var (_, c) in p.Cmds) {
        n.append(c.Copy()._<CommandNode.val>());
    }
    return n;
}

[GoRecv] public static Node Copy(this ref PipeNode p) {
    return ~p.CopyPipe();
}

// ActionNode holds an action (something bounded by delimiters).
// Control actions have their own nodes; ActionNode represents simple
// ones such as field evaluations and parenthesized pipelines.
[GoType] partial struct ActionNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public nint Line;      // The line number in the input. Deprecated: Kept for compatibility.
    public ж<PipeNode> Pipe; // The pipeline in the action.
}

[GoRecv] public static ж<ActionNode> newAction(this ref Tree t, Pos pos, nint line, ж<PipeNode> Ꮡpipe) {
    ref var pipe = ref Ꮡpipe.val;

    return Ꮡ(new ActionNode(tr: t, NodeType: NodeAction, Pos: pos, Line: line, Pipe: pipe));
}

[GoRecv] public static @string String(this ref ActionNode a) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    a.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref ActionNode a, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString("{{"u8);
    a.Pipe.writeTo(Ꮡsb);
    sb.WriteString("}}"u8);
}

[GoRecv] internal static ж<Tree> tree(this ref ActionNode a) {
    return a.tr;
}

[GoRecv] public static Node Copy(this ref ActionNode a) {
    return ~a.tr.newAction(a.Pos, a.Line, a.Pipe.CopyPipe());
}

// CommandNode holds a command (a pipeline inside an evaluating action).
[GoType] partial struct CommandNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public slice<Node> Args; // Arguments in lexical order: Identifier, field, or constant.
}

[GoRecv] internal static ж<CommandNode> newCommand(this ref Tree t, Pos pos) {
    return Ꮡ(new CommandNode(tr: t, NodeType: NodeCommand, Pos: pos));
}

[GoRecv] internal static void append(this ref CommandNode c, Node arg) {
    c.Args = append(c.Args, arg);
}

[GoRecv] public static @string String(this ref CommandNode c) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    c.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref CommandNode c, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    foreach (var (i, arg) in c.Args) {
        if (i > 0) {
            sb.WriteByte((rune)' ');
        }
        {
            var (argΔ1, ok) = arg._<PipeNode.val>(ᐧ); if (ok) {
                sb.WriteByte((rune)'(');
                argΔ1.writeTo(Ꮡsb);
                sb.WriteByte((rune)')');
                continue;
            }
        }
        arg.writeTo(Ꮡsb);
    }
}

[GoRecv] internal static ж<Tree> tree(this ref CommandNode c) {
    return c.tr;
}

[GoRecv("capture")] public static Node Copy(this ref CommandNode c) {
    if (c == nil) {
        return ~c;
    }
    var n = c.tr.newCommand(c.Pos);
    foreach (var (_, cΔ1) in c.Args) {
        n.append(cΔ1.Copy());
    }
    return ~n;
}

// IdentifierNode holds an identifier.
[GoType] partial struct IdentifierNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public @string Ident; // The identifier's name.
}

// NewIdentifier returns a new [IdentifierNode] with the given identifier name.
public static ж<IdentifierNode> NewIdentifier(@string ident) {
    return Ꮡ(new IdentifierNode(NodeType: NodeIdentifier, Ident: ident));
}

// SetPos sets the position. [NewIdentifier] is a public method so we can't modify its signature.
// Chained for convenience.
// TODO: fix one day?
[GoRecv("capture")] public static ж<IdentifierNode> SetPos(this ref IdentifierNode i, Pos pos) {
    i.Pos = pos;
    return SetPosꓸᏑi;
}

// SetTree sets the parent tree for the node. [NewIdentifier] is a public method so we can't modify its signature.
// Chained for convenience.
// TODO: fix one day?
[GoRecv("capture")] public static ж<IdentifierNode> SetTree(this ref IdentifierNode i, ж<Tree> Ꮡt) {
    ref var t = ref Ꮡt.val;

    i.tr = t;
    return SetTreeꓸᏑi;
}

[GoRecv] public static @string String(this ref IdentifierNode i) {
    return i.Ident;
}

[GoRecv] public static void writeTo(this ref IdentifierNode i, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString(i.String());
}

[GoRecv] internal static ж<Tree> tree(this ref IdentifierNode i) {
    return i.tr;
}

[GoRecv] public static Node Copy(this ref IdentifierNode i) {
    return ~NewIdentifier(i.Ident).SetTree(i.tr).SetPos(i.Pos);
}

// VariableNode holds a list of variable names, possibly with chained field
// accesses. The dollar sign is part of the (first) name.
[GoType] partial struct VariableNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public slice<@string> Ident; // Variable name and fields in lexical order.
}

[GoRecv] internal static ж<VariableNode> newVariable(this ref Tree t, Pos pos, @string ident) {
    return Ꮡ(new VariableNode(tr: t, NodeType: NodeVariable, Pos: pos, Ident: strings.Split(ident, "."u8)));
}

[GoRecv] public static @string String(this ref VariableNode v) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    v.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref VariableNode v, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    foreach (var (i, id) in v.Ident) {
        if (i > 0) {
            sb.WriteByte((rune)'.');
        }
        sb.WriteString(id);
    }
}

[GoRecv] internal static ж<Tree> tree(this ref VariableNode v) {
    return v.tr;
}

[GoRecv] public static Node Copy(this ref VariableNode v) {
    return new VariableNode(tr: v.tr, NodeType: NodeVariable, Pos: v.Pos, Ident: append(new @string[]{}.slice(), v.Ident.ꓸꓸꓸ));
}

// DotNode holds the special identifier '.'.
[GoType] partial struct DotNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
}

[GoRecv] internal static ж<DotNode> newDot(this ref Tree t, Pos pos) {
    return Ꮡ(new DotNode(tr: t, NodeType: NodeDot, Pos: pos));
}

[GoRecv] public static NodeType Type(this ref DotNode d) {
    // Override method on embedded NodeType for API compatibility.
    // TODO: Not really a problem; could change API without effect but
    // api tool complains.
    return NodeDot;
}

[GoRecv] public static @string String(this ref DotNode d) {
    return "."u8;
}

[GoRecv] public static void writeTo(this ref DotNode d, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString(d.String());
}

[GoRecv] internal static ж<Tree> tree(this ref DotNode d) {
    return d.tr;
}

[GoRecv] public static Node Copy(this ref DotNode d) {
    return ~d.tr.newDot(d.Pos);
}

// NilNode holds the special identifier 'nil' representing an untyped nil constant.
[GoType] partial struct NilNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
}

[GoRecv] internal static ж<NilNode> newNil(this ref Tree t, Pos pos) {
    return Ꮡ(new NilNode(tr: t, NodeType: NodeNil, Pos: pos));
}

[GoRecv] public static NodeType Type(this ref NilNode n) {
    // Override method on embedded NodeType for API compatibility.
    // TODO: Not really a problem; could change API without effect but
    // api tool complains.
    return NodeNil;
}

[GoRecv] public static @string String(this ref NilNode n) {
    return "nil"u8;
}

[GoRecv] public static void writeTo(this ref NilNode n, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString(n.String());
}

[GoRecv] internal static ж<Tree> tree(this ref NilNode n) {
    return n.tr;
}

[GoRecv] public static Node Copy(this ref NilNode n) {
    return ~n.tr.newNil(n.Pos);
}

// FieldNode holds a field (identifier starting with '.').
// The names may be chained ('.x.y').
// The period is dropped from each ident.
[GoType] partial struct FieldNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public slice<@string> Ident; // The identifiers in lexical order.
}

[GoRecv] internal static ж<FieldNode> newField(this ref Tree t, Pos pos, @string ident) {
    return Ꮡ(new FieldNode(tr: t, NodeType: NodeField, Pos: pos, Ident: strings.Split(ident[1..], "."u8)));
}

// [1:] to drop leading period
[GoRecv] public static @string String(this ref FieldNode f) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    f.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref FieldNode f, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    foreach (var (_, id) in f.Ident) {
        sb.WriteByte((rune)'.');
        sb.WriteString(id);
    }
}

[GoRecv] internal static ж<Tree> tree(this ref FieldNode f) {
    return f.tr;
}

[GoRecv] public static Node Copy(this ref FieldNode f) {
    return new FieldNode(tr: f.tr, NodeType: NodeField, Pos: f.Pos, Ident: append(new @string[]{}.slice(), f.Ident.ꓸꓸꓸ));
}

// ChainNode holds a term followed by a chain of field accesses (identifier starting with '.').
// The names may be chained ('.x.y').
// The periods are dropped from each ident.
[GoType] partial struct ChainNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public Node Node;
    public slice<@string> Field; // The identifiers in lexical order.
}

[GoRecv] internal static ж<ChainNode> newChain(this ref Tree t, Pos pos, Node node) {
    return Ꮡ(new ChainNode(tr: t, NodeType: NodeChain, Pos: pos, Node: node));
}

// Add adds the named field (which should start with a period) to the end of the chain.
[GoRecv] public static void Add(this ref ChainNode c, @string field) {
    if (len(field) == 0 || field[0] != (rune)'.') {
        throw panic("no dot in field");
    }
    field = field[1..];
    // Remove leading dot.
    if (field == ""u8) {
        throw panic("empty field");
    }
    c.Field = append(c.Field, field);
}

[GoRecv] public static @string String(this ref ChainNode c) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    c.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref ChainNode c, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    {
        var (_, ok) = c.Node._<PipeNode.val>(ᐧ); if (ok){
            sb.WriteByte((rune)'(');
            c.Node.writeTo(Ꮡsb);
            sb.WriteByte((rune)')');
        } else {
            c.Node.writeTo(Ꮡsb);
        }
    }
    foreach (var (_, field) in c.Field) {
        sb.WriteByte((rune)'.');
        sb.WriteString(field);
    }
}

[GoRecv] internal static ж<Tree> tree(this ref ChainNode c) {
    return c.tr;
}

[GoRecv] public static Node Copy(this ref ChainNode c) {
    return new ChainNode(tr: c.tr, NodeType: NodeChain, Pos: c.Pos, Node: c.Node, Field: append(new @string[]{}.slice(), c.Field.ꓸꓸꓸ));
}

// BoolNode holds a boolean constant.
[GoType] partial struct BoolNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public bool True; // The value of the boolean constant.
}

[GoRecv] internal static ж<BoolNode> newBool(this ref Tree t, Pos pos, bool true) {
    return Ꮡ(new BoolNode(tr: t, NodeType: NodeBool, Pos: pos, True: true));
}

[GoRecv] public static @string String(this ref BoolNode b) {
    if (b.True) {
        return "true"u8;
    }
    return "false"u8;
}

[GoRecv] public static void writeTo(this ref BoolNode b, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString(b.String());
}

[GoRecv] internal static ж<Tree> tree(this ref BoolNode b) {
    return b.tr;
}

[GoRecv] public static Node Copy(this ref BoolNode b) {
    return ~b.tr.newBool(b.Pos, b.True);
}

// NumberNode holds a number: signed or unsigned integer, float, or complex.
// The value is parsed and stored under all the types that can represent the value.
// This simulates in a small amount of code the behavior of Go's ideal constants.
[GoType] partial struct NumberNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public bool IsInt;       // Number has an integral value.
    public bool IsUint;       // Number has an unsigned integral value.
    public bool IsFloat;       // Number has a floating-point value.
    public bool IsComplex;       // Number is complex.
    public int64 Int64;      // The signed integer value.
    public uint64 Uint64;     // The unsigned integer value.
    public float64 Float64;    // The floating-point value.
    public complex128 Complex128; // The complex value.
    public @string Text;    // The original textual representation from the input.
}

[GoRecv] internal static (ж<NumberNode>, error) newNumber(this ref Tree t, Pos pos, @string text, itemType typ) {
    var n = Ꮡ(new NumberNode(tr: t, NodeType: NodeNumber, Pos: pos, Text: text));
    var exprᴛ1 = typ;
    if (exprᴛ1 == itemCharConstant) {
        var (rune, _, tail, errΔ3) = strconv.UnquoteChar(text[1..], text[0]);
        if (errΔ3 != default!) {
            return (default!, errΔ3);
        }
        if (tail != "'"u8) {
            return (default!, fmt.Errorf("malformed character constant: %s"u8, text));
        }
        n.val.Int64 = ((int64)rune);
        n.val.IsInt = true;
        n.val.Uint64 = ((uint64)rune);
        n.val.IsUint = true;
        n.val.Float64 = ((float64)rune);
        n.val.IsFloat = true;
        return (n, default!);
    }
    if (exprᴛ1 == itemComplex) {
        {
            var (_, errΔ4) = fmt.Sscan(text, // odd but those are the rules.
 // fmt.Sscan can parse the pair, so let it do the work.
 Ꮡ((~n).Complex128)); if (errΔ4 != default!) {
                return (default!, errΔ4);
            }
        }
        n.val.IsComplex = true;
        n.simplifyComplex();
        return (n, default!);
    }

    // Imaginary constants can only be complex unless they are zero.
    if (len(text) > 0 && text[len(text) - 1] == (rune)'i') {
        var (f, errΔ5) = strconv.ParseFloat(text[..(int)(len(text) - 1)], 64);
        if (errΔ5 == default!) {
            n.val.IsComplex = true;
            n.val.Complex128 = complex(0, f);
            n.simplifyComplex();
            return (n, default!);
        }
    }
    // Do integer test first so we get 0x123 etc.
    var (u, err) = strconv.ParseUint(text, 0, 64);
    // will fail for -0; fixed below.
    if (err == default!) {
        n.val.IsUint = true;
        n.val.Uint64 = u;
    }
    var (i, err) = strconv.ParseInt(text, 0, 64);
    if (err == default!) {
        n.val.IsInt = true;
        n.val.Int64 = i;
        if (i == 0) {
            n.val.IsUint = true;
            // in case of -0.
            n.val.Uint64 = u;
        }
    }
    // If an integer extraction succeeded, promote the float.
    if ((~n).IsInt){
        n.val.IsFloat = true;
        n.val.Float64 = ((float64)(~n).Int64);
    } else 
    if ((~n).IsUint){
        n.val.IsFloat = true;
        n.val.Float64 = ((float64)(~n).Uint64);
    } else {
        var (f, errΔ6) = strconv.ParseFloat(text, 64);
        if (errΔ6 == default!) {
            // If we parsed it as a float but it looks like an integer,
            // it's a huge number too large to fit in an int. Reject it.
            if (!strings.ContainsAny(text, ".eEpP"u8)) {
                return (default!, fmt.Errorf("integer overflow: %q"u8, text));
            }
            n.val.IsFloat = true;
            n.val.Float64 = f;
            // If a floating-point extraction succeeded, extract the int if needed.
            if (!(~n).IsInt && ((float64)((int64)f)) == f) {
                n.val.IsInt = true;
                n.val.Int64 = ((int64)f);
            }
            if (!(~n).IsUint && ((float64)((uint64)f)) == f) {
                n.val.IsUint = true;
                n.val.Uint64 = ((uint64)f);
            }
        }
    }
    if (!(~n).IsInt && !(~n).IsUint && !(~n).IsFloat) {
        return (default!, fmt.Errorf("illegal number syntax: %q"u8, text));
    }
    return (n, default!);
}

// simplifyComplex pulls out any other types that are represented by the complex number.
// These all require that the imaginary part be zero.
[GoRecv] internal static void simplifyComplex(this ref NumberNode n) {
    n.IsFloat = imag(n.Complex128) == 0;
    if (n.IsFloat) {
        n.Float64 = real(n.Complex128);
        n.IsInt = ((float64)((int64)n.Float64)) == n.Float64;
        if (n.IsInt) {
            n.Int64 = ((int64)n.Float64);
        }
        n.IsUint = ((float64)((uint64)n.Float64)) == n.Float64;
        if (n.IsUint) {
            n.Uint64 = ((uint64)n.Float64);
        }
    }
}

[GoRecv] public static @string String(this ref NumberNode n) {
    return n.Text;
}

[GoRecv] public static void writeTo(this ref NumberNode n, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString(n.String());
}

[GoRecv] internal static ж<Tree> tree(this ref NumberNode n) {
    return n.tr;
}

[GoRecv] public static Node Copy(this ref NumberNode n) {
    var nn = @new<NumberNode>();
    nn.val = n;
    // Easy, fast, correct.
    return ~nn;
}

// StringNode holds a string constant. The value has been "unquoted".
[GoType] partial struct StringNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public @string Quoted; // The original text of the string, with quotes.
    public @string Text; // The string, after quote processing.
}

[GoRecv] internal static ж<StringNode> newString(this ref Tree t, Pos pos, @string orig, @string text) {
    return Ꮡ(new StringNode(tr: t, NodeType: NodeString, Pos: pos, Quoted: orig, Text: text));
}

[GoRecv] public static @string String(this ref StringNode s) {
    return s.Quoted;
}

[GoRecv] public static void writeTo(this ref StringNode s, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString(s.String());
}

[GoRecv] internal static ж<Tree> tree(this ref StringNode s) {
    return s.tr;
}

[GoRecv] public static Node Copy(this ref StringNode s) {
    return ~s.tr.newString(s.Pos, s.Quoted, s.Text);
}

// endNode represents an {{end}} action.
// It does not appear in the final parse tree.
[GoType] partial struct endNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
}

[GoRecv] internal static ж<endNode> newEnd(this ref Tree t, Pos pos) {
    return Ꮡ(new endNode(tr: t, NodeType: nodeEnd, Pos: pos));
}

[GoRecv] internal static @string String(this ref endNode e) {
    return "{{end}}"u8;
}

[GoRecv] internal static void writeTo(this ref endNode e, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString(e.String());
}

[GoRecv] internal static ж<Tree> tree(this ref endNode e) {
    return e.tr;
}

[GoRecv] internal static Node Copy(this ref endNode e) {
    return ~e.tr.newEnd(e.Pos);
}

// elseNode represents an {{else}} action. Does not appear in the final tree.
[GoType] partial struct elseNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public nint Line; // The line number in the input. Deprecated: Kept for compatibility.
}

[GoRecv] internal static ж<elseNode> newElse(this ref Tree t, Pos pos, nint line) {
    return Ꮡ(new elseNode(tr: t, NodeType: nodeElse, Pos: pos, Line: line));
}

[GoRecv] internal static NodeType Type(this ref elseNode e) {
    return nodeElse;
}

[GoRecv] internal static @string String(this ref elseNode e) {
    return "{{else}}"u8;
}

[GoRecv] internal static void writeTo(this ref elseNode e, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString(e.String());
}

[GoRecv] internal static ж<Tree> tree(this ref elseNode e) {
    return e.tr;
}

[GoRecv] internal static Node Copy(this ref elseNode e) {
    return ~e.tr.newElse(e.Pos, e.Line);
}

// BranchNode is the common representation of if, range, and with.
[GoType] partial struct BranchNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public nint Line;      // The line number in the input. Deprecated: Kept for compatibility.
    public ж<PipeNode> Pipe; // The pipeline to be evaluated.
    public ж<ListNode> List; // What to execute if the value is non-empty.
    public ж<ListNode> ElseList; // What to execute if the value is empty (nil if absent).
}

[GoRecv] public static @string String(this ref BranchNode b) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    b.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref BranchNode b, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    @string name = ""u8;
    var exprᴛ1 = b.NodeType;
    if (exprᴛ1 == NodeIf) {
        name = "if"u8;
    }
    else if (exprᴛ1 == NodeRange) {
        name = "range"u8;
    }
    else if (exprᴛ1 == NodeWith) {
        name = "with"u8;
    }
    else { /* default: */
        throw panic("unknown branch type");
    }

    sb.WriteString("{{"u8);
    sb.WriteString(name);
    sb.WriteByte((rune)' ');
    b.Pipe.writeTo(Ꮡsb);
    sb.WriteString("}}"u8);
    b.List.writeTo(Ꮡsb);
    if (b.ElseList != nil) {
        sb.WriteString("{{else}}"u8);
        b.ElseList.writeTo(Ꮡsb);
    }
    sb.WriteString("{{end}}"u8);
}

[GoRecv] internal static ж<Tree> tree(this ref BranchNode b) {
    return b.tr;
}

[GoRecv] public static Node Copy(this ref BranchNode b) {
    var exprᴛ1 = b.NodeType;
    if (exprᴛ1 == NodeIf) {
        return ~b.tr.newIf(b.Pos, b.Line, b.Pipe, b.List, b.ElseList);
    }
    if (exprᴛ1 == NodeRange) {
        return ~b.tr.newRange(b.Pos, b.Line, b.Pipe, b.List, b.ElseList);
    }
    if (exprᴛ1 == NodeWith) {
        return ~b.tr.newWith(b.Pos, b.Line, b.Pipe, b.List, b.ElseList);
    }
    { /* default: */
        throw panic("unknown branch type");
    }

}

// IfNode represents an {{if}} action and its commands.
[GoType] partial struct IfNode {
    public partial ref BranchNode BranchNode { get; }
}

[GoRecv] public static ж<IfNode> newIf(this ref Tree t, Pos pos, nint line, ж<PipeNode> Ꮡpipe, ж<ListNode> Ꮡlist, ж<ListNode> ᏑelseList) {
    ref var pipe = ref Ꮡpipe.val;
    ref var list = ref Ꮡlist.val;
    ref var elseList = ref ᏑelseList.val;

    return Ꮡ(new IfNode(new BranchNode(tr: t, NodeType: NodeIf, Pos: pos, Line: line, Pipe: pipe, List: list, ElseList: elseList)));
}

[GoRecv] public static Node Copy(this ref IfNode i) {
    return ~i.tr.newIf(i.Pos, i.Line, i.Pipe.CopyPipe(), i.List.CopyList(), i.ElseList.CopyList());
}

// BreakNode represents a {{break}} action.
[GoType] partial struct BreakNode {
    internal ж<Tree> tr;
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    public nint Line;
}

[GoRecv] internal static ж<BreakNode> newBreak(this ref Tree t, Pos pos, nint line) {
    return Ꮡ(new BreakNode(tr: t, NodeType: NodeBreak, Pos: pos, Line: line));
}

[GoRecv] public static Node Copy(this ref BreakNode b) {
    return ~b.tr.newBreak(b.Pos, b.Line);
}

[GoRecv] public static @string String(this ref BreakNode b) {
    return "{{break}}"u8;
}

[GoRecv] internal static ж<Tree> tree(this ref BreakNode b) {
    return b.tr;
}

[GoRecv] public static void writeTo(this ref BreakNode b, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString("{{break}}"u8);
}

// ContinueNode represents a {{continue}} action.
[GoType] partial struct ContinueNode {
    internal ж<Tree> tr;
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    public nint Line;
}

[GoRecv] internal static ж<ContinueNode> newContinue(this ref Tree t, Pos pos, nint line) {
    return Ꮡ(new ContinueNode(tr: t, NodeType: NodeContinue, Pos: pos, Line: line));
}

[GoRecv] public static Node Copy(this ref ContinueNode c) {
    return ~c.tr.newContinue(c.Pos, c.Line);
}

[GoRecv] public static @string String(this ref ContinueNode c) {
    return "{{continue}}"u8;
}

[GoRecv] internal static ж<Tree> tree(this ref ContinueNode c) {
    return c.tr;
}

[GoRecv] public static void writeTo(this ref ContinueNode c, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString("{{continue}}"u8);
}

// RangeNode represents a {{range}} action and its commands.
[GoType] partial struct RangeNode {
    public partial ref BranchNode BranchNode { get; }
}

[GoRecv] public static ж<RangeNode> newRange(this ref Tree t, Pos pos, nint line, ж<PipeNode> Ꮡpipe, ж<ListNode> Ꮡlist, ж<ListNode> ᏑelseList) {
    ref var pipe = ref Ꮡpipe.val;
    ref var list = ref Ꮡlist.val;
    ref var elseList = ref ᏑelseList.val;

    return Ꮡ(new RangeNode(new BranchNode(tr: t, NodeType: NodeRange, Pos: pos, Line: line, Pipe: pipe, List: list, ElseList: elseList)));
}

[GoRecv] public static Node Copy(this ref RangeNode r) {
    return ~r.tr.newRange(r.Pos, r.Line, r.Pipe.CopyPipe(), r.List.CopyList(), r.ElseList.CopyList());
}

// WithNode represents a {{with}} action and its commands.
[GoType] partial struct WithNode {
    public partial ref BranchNode BranchNode { get; }
}

[GoRecv] public static ж<WithNode> newWith(this ref Tree t, Pos pos, nint line, ж<PipeNode> Ꮡpipe, ж<ListNode> Ꮡlist, ж<ListNode> ᏑelseList) {
    ref var pipe = ref Ꮡpipe.val;
    ref var list = ref Ꮡlist.val;
    ref var elseList = ref ᏑelseList.val;

    return Ꮡ(new WithNode(new BranchNode(tr: t, NodeType: NodeWith, Pos: pos, Line: line, Pipe: pipe, List: list, ElseList: elseList)));
}

[GoRecv] public static Node Copy(this ref WithNode w) {
    return ~w.tr.newWith(w.Pos, w.Line, w.Pipe.CopyPipe(), w.List.CopyList(), w.ElseList.CopyList());
}

// TemplateNode represents a {{template}} action.
[GoType] partial struct TemplateNode {
    public partial ref NodeType NodeType { get; }
    public partial ref Pos Pos { get; }
    internal ж<Tree> tr;
    public nint Line;      // The line number in the input. Deprecated: Kept for compatibility.
    public @string Name;   // The name of the template (unquoted).
    public ж<PipeNode> Pipe; // The command to evaluate as dot for the template.
}

[GoRecv] public static ж<TemplateNode> newTemplate(this ref Tree t, Pos pos, nint line, @string name, ж<PipeNode> Ꮡpipe) {
    ref var pipe = ref Ꮡpipe.val;

    return Ꮡ(new TemplateNode(tr: t, NodeType: NodeTemplate, Pos: pos, Line: line, Name: name, Pipe: pipe));
}

[GoRecv] public static @string String(this ref TemplateNode t) {
    ref var sb = ref heap(new strings_package.Builder(), out var Ꮡsb);
    t.writeTo(Ꮡsb);
    return sb.String();
}

[GoRecv] public static void writeTo(this ref TemplateNode t, ж<strings.Builder> Ꮡsb) {
    ref var sb = ref Ꮡsb.val;

    sb.WriteString("{{template "u8);
    sb.WriteString(strconv.Quote(t.Name));
    if (t.Pipe != nil) {
        sb.WriteByte((rune)' ');
        t.Pipe.writeTo(Ꮡsb);
    }
    sb.WriteString("}}"u8);
}

[GoRecv] internal static ж<Tree> tree(this ref TemplateNode t) {
    return t.tr;
}

[GoRecv] public static Node Copy(this ref TemplateNode t) {
    return ~t.tr.newTemplate(t.Pos, t.Line, t.Name, t.Pipe.CopyPipe());
}

} // end parse_package
