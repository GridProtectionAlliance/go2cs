// package inspector -- go2cs converted at 2020 October 08 04:56:29 UTC
// import "golang.org/x/tools/go/ast/inspector" ==> using inspector = go.golang.org.x.tools.go.ast.inspector_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ast\inspector\typeof.go
// This file defines func typeOf(ast.Node) uint64.
//
// The initial map-based implementation was too slow;
// see https://go-review.googlesource.com/c/tools/+/135655/1/go/ast/inspector/inspector.go#196

using ast = go.go.ast_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace ast
{
    public static partial class inspector_package
    {
        private static readonly var nArrayType = (var)iota;
        private static readonly var nAssignStmt = (var)0;
        private static readonly var nBadDecl = (var)1;
        private static readonly var nBadExpr = (var)2;
        private static readonly var nBadStmt = (var)3;
        private static readonly var nBasicLit = (var)4;
        private static readonly var nBinaryExpr = (var)5;
        private static readonly var nBlockStmt = (var)6;
        private static readonly var nBranchStmt = (var)7;
        private static readonly var nCallExpr = (var)8;
        private static readonly var nCaseClause = (var)9;
        private static readonly var nChanType = (var)10;
        private static readonly var nCommClause = (var)11;
        private static readonly var nComment = (var)12;
        private static readonly var nCommentGroup = (var)13;
        private static readonly var nCompositeLit = (var)14;
        private static readonly var nDeclStmt = (var)15;
        private static readonly var nDeferStmt = (var)16;
        private static readonly var nEllipsis = (var)17;
        private static readonly var nEmptyStmt = (var)18;
        private static readonly var nExprStmt = (var)19;
        private static readonly var nField = (var)20;
        private static readonly var nFieldList = (var)21;
        private static readonly var nFile = (var)22;
        private static readonly var nForStmt = (var)23;
        private static readonly var nFuncDecl = (var)24;
        private static readonly var nFuncLit = (var)25;
        private static readonly var nFuncType = (var)26;
        private static readonly var nGenDecl = (var)27;
        private static readonly var nGoStmt = (var)28;
        private static readonly var nIdent = (var)29;
        private static readonly var nIfStmt = (var)30;
        private static readonly var nImportSpec = (var)31;
        private static readonly var nIncDecStmt = (var)32;
        private static readonly var nIndexExpr = (var)33;
        private static readonly var nInterfaceType = (var)34;
        private static readonly var nKeyValueExpr = (var)35;
        private static readonly var nLabeledStmt = (var)36;
        private static readonly var nMapType = (var)37;
        private static readonly var nPackage = (var)38;
        private static readonly var nParenExpr = (var)39;
        private static readonly var nRangeStmt = (var)40;
        private static readonly var nReturnStmt = (var)41;
        private static readonly var nSelectStmt = (var)42;
        private static readonly var nSelectorExpr = (var)43;
        private static readonly var nSendStmt = (var)44;
        private static readonly var nSliceExpr = (var)45;
        private static readonly var nStarExpr = (var)46;
        private static readonly var nStructType = (var)47;
        private static readonly var nSwitchStmt = (var)48;
        private static readonly var nTypeAssertExpr = (var)49;
        private static readonly var nTypeSpec = (var)50;
        private static readonly var nTypeSwitchStmt = (var)51;
        private static readonly var nUnaryExpr = (var)52;
        private static readonly var nValueSpec = (var)53;


        // typeOf returns a distinct single-bit value that represents the type of n.
        //
        // Various implementations were benchmarked with BenchmarkNewInspector:
        //                                GOGC=off
        // - type switch                4.9-5.5ms    2.1ms
        // - binary search over a sorted list of types  5.5-5.9ms    2.5ms
        // - linear scan, frequency-ordered list     5.9-6.1ms    2.7ms
        // - linear scan, unordered list        6.4ms        2.7ms
        // - hash table                    6.5ms        3.1ms
        // A perfect hash seemed like overkill.
        //
        // The compiler's switch statement is the clear winner
        // as it produces a binary tree in code,
        // with constant conditions and good branch prediction.
        // (Sadly it is the most verbose in source code.)
        // Binary search suffered from poor branch prediction.
        //
        private static ulong typeOf(ast.Node n)
        { 
            // Fast path: nearly half of all nodes are identifiers.
            {
                ptr<ast.Ident> (_, ok) = n._<ptr<ast.Ident>>();

                if (ok)
                {
                    return 1L << (int)(nIdent);
                } 

                // These cases include all nodes encountered by ast.Inspect.

            } 

            // These cases include all nodes encountered by ast.Inspect.
            switch (n.type())
            {
                case ptr<ast.ArrayType> _:
                    return 1L << (int)(nArrayType);
                    break;
                case ptr<ast.AssignStmt> _:
                    return 1L << (int)(nAssignStmt);
                    break;
                case ptr<ast.BadDecl> _:
                    return 1L << (int)(nBadDecl);
                    break;
                case ptr<ast.BadExpr> _:
                    return 1L << (int)(nBadExpr);
                    break;
                case ptr<ast.BadStmt> _:
                    return 1L << (int)(nBadStmt);
                    break;
                case ptr<ast.BasicLit> _:
                    return 1L << (int)(nBasicLit);
                    break;
                case ptr<ast.BinaryExpr> _:
                    return 1L << (int)(nBinaryExpr);
                    break;
                case ptr<ast.BlockStmt> _:
                    return 1L << (int)(nBlockStmt);
                    break;
                case ptr<ast.BranchStmt> _:
                    return 1L << (int)(nBranchStmt);
                    break;
                case ptr<ast.CallExpr> _:
                    return 1L << (int)(nCallExpr);
                    break;
                case ptr<ast.CaseClause> _:
                    return 1L << (int)(nCaseClause);
                    break;
                case ptr<ast.ChanType> _:
                    return 1L << (int)(nChanType);
                    break;
                case ptr<ast.CommClause> _:
                    return 1L << (int)(nCommClause);
                    break;
                case ptr<ast.Comment> _:
                    return 1L << (int)(nComment);
                    break;
                case ptr<ast.CommentGroup> _:
                    return 1L << (int)(nCommentGroup);
                    break;
                case ptr<ast.CompositeLit> _:
                    return 1L << (int)(nCompositeLit);
                    break;
                case ptr<ast.DeclStmt> _:
                    return 1L << (int)(nDeclStmt);
                    break;
                case ptr<ast.DeferStmt> _:
                    return 1L << (int)(nDeferStmt);
                    break;
                case ptr<ast.Ellipsis> _:
                    return 1L << (int)(nEllipsis);
                    break;
                case ptr<ast.EmptyStmt> _:
                    return 1L << (int)(nEmptyStmt);
                    break;
                case ptr<ast.ExprStmt> _:
                    return 1L << (int)(nExprStmt);
                    break;
                case ptr<ast.Field> _:
                    return 1L << (int)(nField);
                    break;
                case ptr<ast.FieldList> _:
                    return 1L << (int)(nFieldList);
                    break;
                case ptr<ast.File> _:
                    return 1L << (int)(nFile);
                    break;
                case ptr<ast.ForStmt> _:
                    return 1L << (int)(nForStmt);
                    break;
                case ptr<ast.FuncDecl> _:
                    return 1L << (int)(nFuncDecl);
                    break;
                case ptr<ast.FuncLit> _:
                    return 1L << (int)(nFuncLit);
                    break;
                case ptr<ast.FuncType> _:
                    return 1L << (int)(nFuncType);
                    break;
                case ptr<ast.GenDecl> _:
                    return 1L << (int)(nGenDecl);
                    break;
                case ptr<ast.GoStmt> _:
                    return 1L << (int)(nGoStmt);
                    break;
                case ptr<ast.Ident> _:
                    return 1L << (int)(nIdent);
                    break;
                case ptr<ast.IfStmt> _:
                    return 1L << (int)(nIfStmt);
                    break;
                case ptr<ast.ImportSpec> _:
                    return 1L << (int)(nImportSpec);
                    break;
                case ptr<ast.IncDecStmt> _:
                    return 1L << (int)(nIncDecStmt);
                    break;
                case ptr<ast.IndexExpr> _:
                    return 1L << (int)(nIndexExpr);
                    break;
                case ptr<ast.InterfaceType> _:
                    return 1L << (int)(nInterfaceType);
                    break;
                case ptr<ast.KeyValueExpr> _:
                    return 1L << (int)(nKeyValueExpr);
                    break;
                case ptr<ast.LabeledStmt> _:
                    return 1L << (int)(nLabeledStmt);
                    break;
                case ptr<ast.MapType> _:
                    return 1L << (int)(nMapType);
                    break;
                case ptr<ast.Package> _:
                    return 1L << (int)(nPackage);
                    break;
                case ptr<ast.ParenExpr> _:
                    return 1L << (int)(nParenExpr);
                    break;
                case ptr<ast.RangeStmt> _:
                    return 1L << (int)(nRangeStmt);
                    break;
                case ptr<ast.ReturnStmt> _:
                    return 1L << (int)(nReturnStmt);
                    break;
                case ptr<ast.SelectStmt> _:
                    return 1L << (int)(nSelectStmt);
                    break;
                case ptr<ast.SelectorExpr> _:
                    return 1L << (int)(nSelectorExpr);
                    break;
                case ptr<ast.SendStmt> _:
                    return 1L << (int)(nSendStmt);
                    break;
                case ptr<ast.SliceExpr> _:
                    return 1L << (int)(nSliceExpr);
                    break;
                case ptr<ast.StarExpr> _:
                    return 1L << (int)(nStarExpr);
                    break;
                case ptr<ast.StructType> _:
                    return 1L << (int)(nStructType);
                    break;
                case ptr<ast.SwitchStmt> _:
                    return 1L << (int)(nSwitchStmt);
                    break;
                case ptr<ast.TypeAssertExpr> _:
                    return 1L << (int)(nTypeAssertExpr);
                    break;
                case ptr<ast.TypeSpec> _:
                    return 1L << (int)(nTypeSpec);
                    break;
                case ptr<ast.TypeSwitchStmt> _:
                    return 1L << (int)(nTypeSwitchStmt);
                    break;
                case ptr<ast.UnaryExpr> _:
                    return 1L << (int)(nUnaryExpr);
                    break;
                case ptr<ast.ValueSpec> _:
                    return 1L << (int)(nValueSpec);
                    break;
            }
            return 0L;

        }

        private static ulong maskOf(slice<ast.Node> nodes)
        {
            if (nodes == null)
            {
                return 1L << (int)(64L) - 1L; // match all node types
            }

            ulong mask = default;
            foreach (var (_, n) in nodes)
            {
                mask |= typeOf(n);
            }
            return mask;

        }
    }
}}}}}}
