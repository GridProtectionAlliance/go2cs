// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements helper functions for scope position computations.

// package syntax -- go2cs converted at 2022 March 06 23:13:33 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\syntax\positions.go


namespace go.cmd.compile.@internal;

public static partial class syntax_package {

    // StartPos returns the start position of n.
public static Pos StartPos(Node n) => func((_, panic, _) => { 
    // Cases for nodes which don't need a correction are commented out.
    {
        var m = n;

        while (>>MARKER:FOREXPRESSION_LEVEL_1<<) {
            switch (m.type()) {
                case 
                    panic("internal error: nil"); 

                    // packages
                    break;
                case ptr<File> n:
                    return MakePos(n.Pos().Base(), 1, 1); 

                    // declarations
                    // case *ImportDecl:
                    // case *ConstDecl:
                    // case *TypeDecl:
                    // case *VarDecl:
                    // case *FuncDecl:

                    // expressions
                    // case *BadExpr:
                    // case *Name:
                    // case *BasicLit:
                    break;
                case ptr<CompositeLit> n:
                    if (n.Type != null) {
                        m = n.Type;
                        continue;
                    }
                    return n.Pos(); 
                    // case *KeyValueExpr:
                    // case *FuncLit:
                    // case *ParenExpr:
                    break;
                case ptr<SelectorExpr> n:
                    m = n.X;
                    break;
                case ptr<IndexExpr> n:
                    m = n.X; 
                    // case *SliceExpr:
                    break;
                case ptr<AssertExpr> n:
                    m = n.X;
                    break;
                case ptr<TypeSwitchGuard> n:
                    if (n.Lhs != null) {
                        m = n.Lhs;
                        continue;
                    }
                    m = n.X;
                    break;
                case ptr<Operation> n:
                    if (n.Y != null) {
                        m = n.X;
                        continue;
                    }
                    return n.Pos();
                    break;
                case ptr<CallExpr> n:
                    m = n.Fun;
                    break;
                case ptr<ListExpr> n:
                    if (len(n.ElemList) > 0) {
                        m = n.ElemList[0];
                        continue;
                    }
                    return n.Pos(); 
                    // types
                    // case *ArrayType:
                    // case *SliceType:
                    // case *DotsType:
                    // case *StructType:
                    // case *Field:
                    // case *InterfaceType:
                    // case *FuncType:
                    // case *MapType:
                    // case *ChanType:

                    // statements
                    // case *EmptyStmt:
                    // case *LabeledStmt:
                    // case *BlockStmt:
                    // case *ExprStmt:
                    break;
                case ptr<SendStmt> n:
                    m = n.Chan; 
                    // case *DeclStmt:
                    break;
                case ptr<AssignStmt> n:
                    m = n.Lhs; 
                    // case *BranchStmt:
                    // case *CallStmt:
                    // case *ReturnStmt:
                    // case *IfStmt:
                    // case *ForStmt:
                    // case *SwitchStmt:
                    // case *SelectStmt:

                    // helper nodes
                    break;
                case ptr<RangeClause> n:
                    if (n.Lhs != null) {
                        m = n.Lhs;
                        continue;
                    }
                    m = n.X; 
                    // case *CaseClause:
                    // case *CommClause:
                    break;
                default:
                {
                    var n = m.type();
                    return n.Pos();
                    break;
                }
            }

        }
    }

});

// EndPos returns the approximate end position of n in the source.
// For some nodes (*Name, *BasicLit) it returns the position immediately
// following the node; for others (*BlockStmt, *SwitchStmt, etc.) it
// returns the position of the closing '}'; and for some (*ParenExpr)
// the returned position is the end position of the last enclosed
// expression.
// Thus, EndPos should not be used for exact demarcation of the
// end of a node in the source; it is mostly useful to determine
// scope ranges where there is some leeway.
public static Pos EndPos(Node n) => func((_, panic, _) => {
    {
        var m = n;

        while (>>MARKER:FOREXPRESSION_LEVEL_1<<) {
            switch (m.type()) {
                case 
                    panic("internal error: nil"); 

                    // packages
                    break;
                case ptr<File> n:
                    return n.EOF; 

                    // declarations
                    break;
                case ptr<ImportDecl> n:
                    m = n.Path;
                    break;
                case ptr<ConstDecl> n:
                    if (n.Values != null) {
                        m = n.Values;
                        continue;
                    }
                    if (n.Type != null) {
                        m = n.Type;
                        continue;
                    }
                    {
                        var l__prev1 = l;

                        var l = len(n.NameList);

                        if (l > 0) {
                            m = n.NameList[l - 1];
                            continue;
                        }

                        l = l__prev1;

                    }

                    return n.Pos();
                    break;
                case ptr<TypeDecl> n:
                    m = n.Type;
                    break;
                case ptr<VarDecl> n:
                    if (n.Values != null) {
                        m = n.Values;
                        continue;
                    }
                    if (n.Type != null) {
                        m = n.Type;
                        continue;
                    }
                    {
                        var l__prev1 = l;

                        l = len(n.NameList);

                        if (l > 0) {
                            m = n.NameList[l - 1];
                            continue;
                        }

                        l = l__prev1;

                    }

                    return n.Pos();
                    break;
                case ptr<FuncDecl> n:
                    if (n.Body != null) {
                        m = n.Body;
                        continue;
                    }
                    m = n.Type; 

                    // expressions
                    break;
                case ptr<BadExpr> n:
                    return n.Pos();
                    break;
                case ptr<Name> n:
                    var p = n.Pos();
                    return MakePos(p.Base(), p.Line(), p.Col() + uint(len(n.Value)));
                    break;
                case ptr<BasicLit> n:
                    p = n.Pos();
                    return MakePos(p.Base(), p.Line(), p.Col() + uint(len(n.Value)));
                    break;
                case ptr<CompositeLit> n:
                    return n.Rbrace;
                    break;
                case ptr<KeyValueExpr> n:
                    m = n.Value;
                    break;
                case ptr<FuncLit> n:
                    m = n.Body;
                    break;
                case ptr<ParenExpr> n:
                    m = n.X;
                    break;
                case ptr<SelectorExpr> n:
                    m = n.Sel;
                    break;
                case ptr<IndexExpr> n:
                    m = n.Index;
                    break;
                case ptr<SliceExpr> n:
                    for (var i = len(n.Index) - 1; i >= 0; i--) {
                        {
                            var x = n.Index[i];

                            if (x != null) {
                                m = x;
                                continue;
                            }

                        }

                    }

                    m = n.X;
                    break;
                case ptr<AssertExpr> n:
                    m = n.Type;
                    break;
                case ptr<TypeSwitchGuard> n:
                    m = n.X;
                    break;
                case ptr<Operation> n:
                    if (n.Y != null) {
                        m = n.Y;
                        continue;
                    }
                    m = n.X;
                    break;
                case ptr<CallExpr> n:
                    {
                        var l__prev1 = l;

                        l = lastExpr(n.ArgList);

                        if (l != null) {
                            m = l;
                            continue;
                        }

                        l = l__prev1;

                    }

                    m = n.Fun;
                    break;
                case ptr<ListExpr> n:
                    {
                        var l__prev1 = l;

                        l = lastExpr(n.ElemList);

                        if (l != null) {
                            m = l;
                            continue;
                        }

                        l = l__prev1;

                    }

                    return n.Pos(); 

                    // types
                    break;
                case ptr<ArrayType> n:
                    m = n.Elem;
                    break;
                case ptr<SliceType> n:
                    m = n.Elem;
                    break;
                case ptr<DotsType> n:
                    m = n.Elem;
                    break;
                case ptr<StructType> n:
                    {
                        var l__prev1 = l;

                        l = lastField(n.FieldList);

                        if (l != null) {
                            m = l;
                            continue;
                        }

                        l = l__prev1;

                    }

                    return n.Pos(); 
                    // TODO(gri) need to take TagList into account
                    break;
                case ptr<Field> n:
                    if (n.Type != null) {
                        m = n.Type;
                        continue;
                    }
                    m = n.Name;
                    break;
                case ptr<InterfaceType> n:
                    {
                        var l__prev1 = l;

                        l = lastField(n.MethodList);

                        if (l != null) {
                            m = l;
                            continue;
                        }

                        l = l__prev1;

                    }

                    return n.Pos();
                    break;
                case ptr<FuncType> n:
                    {
                        var l__prev1 = l;

                        l = lastField(n.ResultList);

                        if (l != null) {
                            m = l;
                            continue;
                        }

                        l = l__prev1;

                    }

                    {
                        var l__prev1 = l;

                        l = lastField(n.ParamList);

                        if (l != null) {
                            m = l;
                            continue;
                        }

                        l = l__prev1;

                    }

                    return n.Pos();
                    break;
                case ptr<MapType> n:
                    m = n.Value;
                    break;
                case ptr<ChanType> n:
                    m = n.Elem; 

                    // statements
                    break;
                case ptr<EmptyStmt> n:
                    return n.Pos();
                    break;
                case ptr<LabeledStmt> n:
                    m = n.Stmt;
                    break;
                case ptr<BlockStmt> n:
                    return n.Rbrace;
                    break;
                case ptr<ExprStmt> n:
                    m = n.X;
                    break;
                case ptr<SendStmt> n:
                    m = n.Value;
                    break;
                case ptr<DeclStmt> n:
                    {
                        var l__prev1 = l;

                        l = lastDecl(n.DeclList);

                        if (l != null) {
                            m = l;
                            continue;
                        }

                        l = l__prev1;

                    }

                    return n.Pos();
                    break;
                case ptr<AssignStmt> n:
                    m = n.Rhs;
                    if (m == null) {
                        p = EndPos(n.Lhs);
                        return MakePos(p.Base(), p.Line(), p.Col() + 2);
                    }
                    break;
                case ptr<BranchStmt> n:
                    if (n.Label != null) {
                        m = n.Label;
                        continue;
                    }
                    return n.Pos();
                    break;
                case ptr<CallStmt> n:
                    m = n.Call;
                    break;
                case ptr<ReturnStmt> n:
                    if (n.Results != null) {
                        m = n.Results;
                        continue;
                    }
                    return n.Pos();
                    break;
                case ptr<IfStmt> n:
                    if (n.Else != null) {
                        m = n.Else;
                        continue;
                    }
                    m = n.Then;
                    break;
                case ptr<ForStmt> n:
                    m = n.Body;
                    break;
                case ptr<SwitchStmt> n:
                    return n.Rbrace;
                    break;
                case ptr<SelectStmt> n:
                    return n.Rbrace; 

                    // helper nodes
                    break;
                case ptr<RangeClause> n:
                    m = n.X;
                    break;
                case ptr<CaseClause> n:
                    {
                        var l__prev1 = l;

                        l = lastStmt(n.Body);

                        if (l != null) {
                            m = l;
                            continue;
                        }

                        l = l__prev1;

                    }

                    return n.Colon;
                    break;
                case ptr<CommClause> n:
                    {
                        var l__prev1 = l;

                        l = lastStmt(n.Body);

                        if (l != null) {
                            m = l;
                            continue;
                        }

                        l = l__prev1;

                    }

                    return n.Colon;
                    break;
                default:
                {
                    var n = m.type();
                    return n.Pos();
                    break;
                }
            }

        }
    }

});

private static Decl lastDecl(slice<Decl> list) {
    {
        var l = len(list);

        if (l > 0) {
            return list[l - 1];
        }
    }

    return null;

}

private static Expr lastExpr(slice<Expr> list) {
    {
        var l = len(list);

        if (l > 0) {
            return list[l - 1];
        }
    }

    return null;

}

private static Stmt lastStmt(slice<Stmt> list) {
    {
        var l = len(list);

        if (l > 0) {
            return list[l - 1];
        }
    }

    return null;

}

private static ptr<Field> lastField(slice<ptr<Field>> list) {
    {
        var l = len(list);

        if (l > 0) {
            return _addr_list[l - 1]!;
        }
    }

    return _addr_null!;

}

} // end syntax_package
