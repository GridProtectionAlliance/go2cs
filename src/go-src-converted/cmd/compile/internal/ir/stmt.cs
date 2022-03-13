// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 13 06:00:38 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\stmt.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;


// A Decl is a declaration of a const, type, or var. (A declared func is a Func.)

public static partial class ir_package {

public partial struct Decl {
    public ref miniNode miniNode => ref miniNode_val;
    public ptr<Name> X; // the thing being declared
}

public static ptr<Decl> NewDecl(src.XPos pos, Op op, ptr<Name> _addr_x) => func((_, panic, _) => {
    ref Name x = ref _addr_x.val;

    ptr<Decl> n = addr(new Decl(X:x));
    n.pos = pos;

    if (op == ODCL || op == ODCLCONST || op == ODCLTYPE) 
        n.op = op;
    else 
        panic("invalid Decl op " + op.String());
        return _addr_n!;
});

private static void isStmt(this ptr<Decl> _addr__p0) {
    ref Decl _p0 = ref _addr__p0.val;

}

// A Stmt is a Node that can appear as a statement.
// This includes statement-like expressions such as f().
//
// (It's possible it should include <-c, but that would require
// splitting ORECV out of UnaryExpr, which hasn't yet been
// necessary. Maybe instead we will introduce ExprStmt at
// some point.)
public partial interface Stmt {
    void isStmt();
}

// A miniStmt is a miniNode with extra fields common to statements.
private partial struct miniStmt {
    public ref miniNode miniNode => ref miniNode_val;
    public Nodes init;
}

private static void isStmt(this ptr<miniStmt> _addr__p0) {
    ref miniStmt _p0 = ref _addr__p0.val;

}

private static Nodes Init(this ptr<miniStmt> _addr_n) {
    ref miniStmt n = ref _addr_n.val;

    return n.init;
}
private static void SetInit(this ptr<miniStmt> _addr_n, Nodes x) {
    ref miniStmt n = ref _addr_n.val;

    n.init = x;
}
private static ptr<Nodes> PtrInit(this ptr<miniStmt> _addr_n) {
    ref miniStmt n = ref _addr_n.val;

    return _addr__addr_n.init!;
}

// An AssignListStmt is an assignment statement with
// more than one item on at least one side: Lhs = Rhs.
// If Def is true, the assignment is a :=.
public partial struct AssignListStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public Nodes Lhs;
    public bool Def;
    public Nodes Rhs;
}

public static ptr<AssignListStmt> NewAssignListStmt(src.XPos pos, Op op, slice<Node> lhs, slice<Node> rhs) {
    ptr<AssignListStmt> n = addr(new AssignListStmt());
    n.pos = pos;
    n.SetOp(op);
    n.Lhs = lhs;
    n.Rhs = rhs;
    return _addr_n!;
}

private static void SetOp(this ptr<AssignListStmt> _addr_n, Op op) => func((_, panic, _) => {
    ref AssignListStmt n = ref _addr_n.val;


    if (op == OAS2 || op == OAS2DOTTYPE || op == OAS2FUNC || op == OAS2MAPR || op == OAS2RECV || op == OSELRECV2) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    });

// An AssignStmt is a simple assignment statement: X = Y.
// If Def is true, the assignment is a :=.
public partial struct AssignStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public Node X;
    public bool Def;
    public Node Y;
}

public static ptr<AssignStmt> NewAssignStmt(src.XPos pos, Node x, Node y) {
    ptr<AssignStmt> n = addr(new AssignStmt(X:x,Y:y));
    n.pos = pos;
    n.op = OAS;
    return _addr_n!;
}

private static void SetOp(this ptr<AssignStmt> _addr_n, Op op) => func((_, panic, _) => {
    ref AssignStmt n = ref _addr_n.val;


    if (op == OAS) 
        n.op = op;
    else 
        panic(n.no("SetOp " + op.String()));
    });

// An AssignOpStmt is an AsOp= assignment statement: X AsOp= Y.
public partial struct AssignOpStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public Node X;
    public Op AsOp; // OADD etc
    public Node Y;
    public bool IncDec; // actually ++ or --
}

public static ptr<AssignOpStmt> NewAssignOpStmt(src.XPos pos, Op asOp, Node x, Node y) {
    ptr<AssignOpStmt> n = addr(new AssignOpStmt(AsOp:asOp,X:x,Y:y));
    n.pos = pos;
    n.op = OASOP;
    return _addr_n!;
}

// A BlockStmt is a block: { List }.
public partial struct BlockStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public Nodes List;
}

public static ptr<BlockStmt> NewBlockStmt(src.XPos pos, slice<Node> list) {
    ptr<BlockStmt> n = addr(new BlockStmt());
    n.pos = pos;
    if (!pos.IsKnown()) {
        n.pos = @base.Pos;
        if (len(list) > 0) {
            n.pos = list[0].Pos();
        }
    }
    n.op = OBLOCK;
    n.List = list;
    return _addr_n!;
}

// A BranchStmt is a break, continue, fallthrough, or goto statement.
public partial struct BranchStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public ptr<types.Sym> Label; // label if present
}

public static ptr<BranchStmt> NewBranchStmt(src.XPos pos, Op op, ptr<types.Sym> _addr_label) => func((_, panic, _) => {
    ref types.Sym label = ref _addr_label.val;


    if (op == OBREAK || op == OCONTINUE || op == OFALL || op == OGOTO)     else 
        panic("NewBranch " + op.String());
        ptr<BranchStmt> n = addr(new BranchStmt(Label:label));
    n.pos = pos;
    n.op = op;
    return _addr_n!;
});

private static ptr<types.Sym> Sym(this ptr<BranchStmt> _addr_n) {
    ref BranchStmt n = ref _addr_n.val;

    return _addr_n.Label!;
}

// A CaseClause is a case statement in a switch or select: case List: Body.
public partial struct CaseClause {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public ptr<Name> Var; // declared variable for this case in type switch
    public Nodes List; // list of expressions for switch, early select
    public Nodes Body;
}

public static ptr<CaseClause> NewCaseStmt(src.XPos pos, slice<Node> list, slice<Node> body) {
    ptr<CaseClause> n = addr(new CaseClause(List:list,Body:body));
    n.pos = pos;
    n.op = OCASE;
    return _addr_n!;
}

public partial struct CommClause {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public Node Comm; // communication case
    public Nodes Body;
}

public static ptr<CommClause> NewCommStmt(src.XPos pos, Node comm, slice<Node> body) {
    ptr<CommClause> n = addr(new CommClause(Comm:comm,Body:body));
    n.pos = pos;
    n.op = OCASE;
    return _addr_n!;
}

// A ForStmt is a non-range for loop: for Init; Cond; Post { Body }
// Op can be OFOR or OFORUNTIL (!Cond).
public partial struct ForStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public ptr<types.Sym> Label;
    public Node Cond;
    public Nodes Late;
    public Node Post;
    public Nodes Body;
    public bool HasBreak;
}

public static ptr<ForStmt> NewForStmt(src.XPos pos, Node init, Node cond, Node post, slice<Node> body) {
    ptr<ForStmt> n = addr(new ForStmt(Cond:cond,Post:post));
    n.pos = pos;
    n.op = OFOR;
    if (init != null) {
        n.init = new slice<Node>(new Node[] { init });
    }
    n.Body = body;
    return _addr_n!;
}

private static void SetOp(this ptr<ForStmt> _addr_n, Op op) => func((_, panic, _) => {
    ref ForStmt n = ref _addr_n.val;

    if (op != OFOR && op != OFORUNTIL) {
        panic(n.no("SetOp " + op.String()));
    }
    n.op = op;
});

// A GoDeferStmt is a go or defer statement: go Call / defer Call.
//
// The two opcodes use a single syntax because the implementations
// are very similar: both are concerned with saving Call and running it
// in a different context (a separate goroutine or a later time).
public partial struct GoDeferStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public Node Call;
}

public static ptr<GoDeferStmt> NewGoDeferStmt(src.XPos pos, Op op, Node call) => func((_, panic, _) => {
    ptr<GoDeferStmt> n = addr(new GoDeferStmt(Call:call));
    n.pos = pos;

    if (op == ODEFER || op == OGO) 
        n.op = op;
    else 
        panic("NewGoDeferStmt " + op.String());
        return _addr_n!;
});

// A IfStmt is a return statement: if Init; Cond { Then } else { Else }.
public partial struct IfStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public Node Cond;
    public Nodes Body;
    public Nodes Else;
    public bool Likely; // code layout hint
}

public static ptr<IfStmt> NewIfStmt(src.XPos pos, Node cond, slice<Node> body, slice<Node> els) {
    ptr<IfStmt> n = addr(new IfStmt(Cond:cond));
    n.pos = pos;
    n.op = OIF;
    n.Body = body;
    n.Else = els;
    return _addr_n!;
}

// An InlineMarkStmt is a marker placed just before an inlined body.
public partial struct InlineMarkStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public long Index;
}

public static ptr<InlineMarkStmt> NewInlineMarkStmt(src.XPos pos, long index) {
    ptr<InlineMarkStmt> n = addr(new InlineMarkStmt(Index:index));
    n.pos = pos;
    n.op = OINLMARK;
    return _addr_n!;
}

private static long Offset(this ptr<InlineMarkStmt> _addr_n) {
    ref InlineMarkStmt n = ref _addr_n.val;

    return n.Index;
}
private static void SetOffset(this ptr<InlineMarkStmt> _addr_n, long x) {
    ref InlineMarkStmt n = ref _addr_n.val;

    n.Index = x;
}

// A LabelStmt is a label statement (just the label, not including the statement it labels).
public partial struct LabelStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public ptr<types.Sym> Label; // "Label:"
}

public static ptr<LabelStmt> NewLabelStmt(src.XPos pos, ptr<types.Sym> _addr_label) {
    ref types.Sym label = ref _addr_label.val;

    ptr<LabelStmt> n = addr(new LabelStmt(Label:label));
    n.pos = pos;
    n.op = OLABEL;
    return _addr_n!;
}

private static ptr<types.Sym> Sym(this ptr<LabelStmt> _addr_n) {
    ref LabelStmt n = ref _addr_n.val;

    return _addr_n.Label!;
}

// A RangeStmt is a range loop: for Key, Value = range X { Body }
public partial struct RangeStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public ptr<types.Sym> Label;
    public bool Def;
    public Node X;
    public Node Key;
    public Node Value;
    public Nodes Body;
    public bool HasBreak;
    public ptr<Name> Prealloc;
}

public static ptr<RangeStmt> NewRangeStmt(src.XPos pos, Node key, Node value, Node x, slice<Node> body) {
    ptr<RangeStmt> n = addr(new RangeStmt(X:x,Key:key,Value:value));
    n.pos = pos;
    n.op = ORANGE;
    n.Body = body;
    return _addr_n!;
}

// A ReturnStmt is a return statement.
public partial struct ReturnStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public ref origNode origNode => ref origNode_val; // for typecheckargs rewrite
    public Nodes Results; // return list
}

public static ptr<ReturnStmt> NewReturnStmt(src.XPos pos, slice<Node> results) {
    ptr<ReturnStmt> n = addr(new ReturnStmt());
    n.pos = pos;
    n.op = ORETURN;
    n.orig = n;
    n.Results = results;
    return _addr_n!;
}

// A SelectStmt is a block: { Cases }.
public partial struct SelectStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public ptr<types.Sym> Label;
    public slice<ptr<CommClause>> Cases;
    public bool HasBreak; // TODO(rsc): Instead of recording here, replace with a block?
    public Nodes Compiled; // compiled form, after walkSwitch
}

public static ptr<SelectStmt> NewSelectStmt(src.XPos pos, slice<ptr<CommClause>> cases) {
    ptr<SelectStmt> n = addr(new SelectStmt(Cases:cases));
    n.pos = pos;
    n.op = OSELECT;
    return _addr_n!;
}

// A SendStmt is a send statement: X <- Y.
public partial struct SendStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public Node Chan;
    public Node Value;
}

public static ptr<SendStmt> NewSendStmt(src.XPos pos, Node ch, Node value) {
    ptr<SendStmt> n = addr(new SendStmt(Chan:ch,Value:value));
    n.pos = pos;
    n.op = OSEND;
    return _addr_n!;
}

// A SwitchStmt is a switch statement: switch Init; Expr { Cases }.
public partial struct SwitchStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public Node Tag;
    public slice<ptr<CaseClause>> Cases;
    public ptr<types.Sym> Label;
    public bool HasBreak; // TODO(rsc): Instead of recording here, replace with a block?
    public Nodes Compiled; // compiled form, after walkSwitch
}

public static ptr<SwitchStmt> NewSwitchStmt(src.XPos pos, Node tag, slice<ptr<CaseClause>> cases) {
    ptr<SwitchStmt> n = addr(new SwitchStmt(Tag:tag,Cases:cases));
    n.pos = pos;
    n.op = OSWITCH;
    return _addr_n!;
}

// A TailCallStmt is a tail call statement, which is used for back-end
// code generation to jump directly to another function entirely.
public partial struct TailCallStmt {
    public ref miniStmt miniStmt => ref miniStmt_val;
    public ptr<Name> Target;
}

public static ptr<TailCallStmt> NewTailCallStmt(src.XPos pos, ptr<Name> _addr_target) {
    ref Name target = ref _addr_target.val;

    if (target.Op() != ONAME || target.Class != PFUNC) {
        @base.FatalfAt(pos, "tail call to non-func %v", target);
    }
    ptr<TailCallStmt> n = addr(new TailCallStmt(Target:target));
    n.pos = pos;
    n.op = OTAILCALL;
    return _addr_n!;
}

// A TypeSwitchGuard is the [Name :=] X.(type) in a type switch.
public partial struct TypeSwitchGuard {
    public ref miniNode miniNode => ref miniNode_val;
    public ptr<Ident> Tag;
    public Node X;
    public bool Used;
}

public static ptr<TypeSwitchGuard> NewTypeSwitchGuard(src.XPos pos, ptr<Ident> _addr_tag, Node x) {
    ref Ident tag = ref _addr_tag.val;

    ptr<TypeSwitchGuard> n = addr(new TypeSwitchGuard(Tag:tag,X:x));
    n.pos = pos;
    n.op = OTYPESW;
    return _addr_n!;
}

} // end ir_package
