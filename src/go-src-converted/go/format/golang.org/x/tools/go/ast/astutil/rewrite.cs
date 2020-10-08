// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package astutil -- go2cs converted at 2020 October 08 04:27:08 UTC
// import "golang.org/x/tools/go/ast/astutil" ==> using astutil = go.golang.org.x.tools.go.ast.astutil_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ast\astutil\rewrite.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace ast
{
    public static partial class astutil_package
    {
        // An ApplyFunc is invoked by Apply for each node n, even if n is nil,
        // before and/or after the node's children, using a Cursor describing
        // the current node and providing operations on it.
        //
        // The return value of ApplyFunc controls the syntax tree traversal.
        // See Apply for details.
        public delegate  bool ApplyFunc(ptr<Cursor>);

        // Apply traverses a syntax tree recursively, starting with root,
        // and calling pre and post for each node as described below.
        // Apply returns the syntax tree, possibly modified.
        //
        // If pre is not nil, it is called for each node before the node's
        // children are traversed (pre-order). If pre returns false, no
        // children are traversed, and post is not called for that node.
        //
        // If post is not nil, and a prior call of pre didn't return false,
        // post is called for each node after its children are traversed
        // (post-order). If post returns false, traversal is terminated and
        // Apply returns immediately.
        //
        // Only fields that refer to AST nodes are considered children;
        // i.e., token.Pos, Scopes, Objects, and fields of basic types
        // (strings, etc.) are ignored.
        //
        // Children are traversed in the order in which they appear in the
        // respective node's struct definition. A package's files are
        // traversed in the filenames' alphabetical order.
        //
        public static ast.Node Apply(ast.Node root, ApplyFunc pre, ApplyFunc post) => func((defer, panic, _) =>
        {
            ast.Node result = default;

            struct{ast.Node} parent = _addr_/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{ast.Node}{root};
            defer(() =>
            {
                {
                    var r = recover();

                    if (r != null && r != abort)
                    {
                        panic(r);
                    }

                }

                result = parent.Node;

            }());
            ptr<application> a = addr(new application(pre:pre,post:post));
            a.apply(parent, "Node", null, root);
            return ;

        });

        private static ptr<object> abort = @new<int>(); // singleton, to signal termination of Apply

        // A Cursor describes a node encountered during Apply.
        // Information about the node and its parent is available
        // from the Node, Parent, Name, and Index methods.
        //
        // If p is a variable of type and value of the current parent node
        // c.Parent(), and f is the field identifier with name c.Name(),
        // the following invariants hold:
        //
        //   p.f            == c.Node()  if c.Index() <  0
        //   p.f[c.Index()] == c.Node()  if c.Index() >= 0
        //
        // The methods Replace, Delete, InsertBefore, and InsertAfter
        // can be used to change the AST without disrupting Apply.
        public partial struct Cursor
        {
            public ast.Node parent;
            public @string name;
            public ptr<iterator> iter; // valid if non-nil
            public ast.Node node;
        }

        // Node returns the current Node.
        private static ast.Node Node(this ptr<Cursor> _addr_c)
        {
            ref Cursor c = ref _addr_c.val;

            return c.node;
        }

        // Parent returns the parent of the current Node.
        private static ast.Node Parent(this ptr<Cursor> _addr_c)
        {
            ref Cursor c = ref _addr_c.val;

            return c.parent;
        }

        // Name returns the name of the parent Node field that contains the current Node.
        // If the parent is a *ast.Package and the current Node is a *ast.File, Name returns
        // the filename for the current Node.
        private static @string Name(this ptr<Cursor> _addr_c)
        {
            ref Cursor c = ref _addr_c.val;

            return c.name;
        }

        // Index reports the index >= 0 of the current Node in the slice of Nodes that
        // contains it, or a value < 0 if the current Node is not part of a slice.
        // The index of the current node changes if InsertBefore is called while
        // processing the current node.
        private static long Index(this ptr<Cursor> _addr_c)
        {
            ref Cursor c = ref _addr_c.val;

            if (c.iter != null)
            {
                return c.iter.index;
            }

            return -1L;

        }

        // field returns the current node's parent field value.
        private static reflect.Value field(this ptr<Cursor> _addr_c)
        {
            ref Cursor c = ref _addr_c.val;

            return reflect.Indirect(reflect.ValueOf(c.parent)).FieldByName(c.name);
        }

        // Replace replaces the current Node with n.
        // The replacement node is not walked by Apply.
        private static void Replace(this ptr<Cursor> _addr_c, ast.Node n) => func((_, panic, __) =>
        {
            ref Cursor c = ref _addr_c.val;

            {
                ptr<ast.File> (_, ok) = c.node._<ptr<ast.File>>();

                if (ok)
                {
                    ptr<ast.File> (file, ok) = n._<ptr<ast.File>>();
                    if (!ok)
                    {
                        panic("attempt to replace *ast.File with non-*ast.File");
                    }

                    c.parent._<ptr<ast.Package>>().Files[c.name] = file;
                    return ;

                }

            }


            var v = c.field();
            {
                var i = c.Index();

                if (i >= 0L)
                {
                    v = v.Index(i);
                }

            }

            v.Set(reflect.ValueOf(n));

        });

        // Delete deletes the current Node from its containing slice.
        // If the current Node is not part of a slice, Delete panics.
        // As a special case, if the current node is a package file,
        // Delete removes it from the package's Files map.
        private static void Delete(this ptr<Cursor> _addr_c) => func((_, panic, __) =>
        {
            ref Cursor c = ref _addr_c.val;

            {
                ptr<ast.File> (_, ok) = c.node._<ptr<ast.File>>();

                if (ok)
                {
                    delete(c.parent._<ptr<ast.Package>>().Files, c.name);
                    return ;
                }

            }


            var i = c.Index();
            if (i < 0L)
            {
                panic("Delete node not contained in slice");
            }

            var v = c.field();
            var l = v.Len();
            reflect.Copy(v.Slice(i, l), v.Slice(i + 1L, l));
            v.Index(l - 1L).Set(reflect.Zero(v.Type().Elem()));
            v.SetLen(l - 1L);
            c.iter.step--;

        });

        // InsertAfter inserts n after the current Node in its containing slice.
        // If the current Node is not part of a slice, InsertAfter panics.
        // Apply does not walk n.
        private static void InsertAfter(this ptr<Cursor> _addr_c, ast.Node n) => func((_, panic, __) =>
        {
            ref Cursor c = ref _addr_c.val;

            var i = c.Index();
            if (i < 0L)
            {
                panic("InsertAfter node not contained in slice");
            }

            var v = c.field();
            v.Set(reflect.Append(v, reflect.Zero(v.Type().Elem())));
            var l = v.Len();
            reflect.Copy(v.Slice(i + 2L, l), v.Slice(i + 1L, l));
            v.Index(i + 1L).Set(reflect.ValueOf(n));
            c.iter.step++;

        });

        // InsertBefore inserts n before the current Node in its containing slice.
        // If the current Node is not part of a slice, InsertBefore panics.
        // Apply will not walk n.
        private static void InsertBefore(this ptr<Cursor> _addr_c, ast.Node n) => func((_, panic, __) =>
        {
            ref Cursor c = ref _addr_c.val;

            var i = c.Index();
            if (i < 0L)
            {
                panic("InsertBefore node not contained in slice");
            }

            var v = c.field();
            v.Set(reflect.Append(v, reflect.Zero(v.Type().Elem())));
            var l = v.Len();
            reflect.Copy(v.Slice(i + 1L, l), v.Slice(i, l));
            v.Index(i).Set(reflect.ValueOf(n));
            c.iter.index++;

        });

        // application carries all the shared data so we can pass it around cheaply.
        private partial struct application
        {
            public ApplyFunc pre;
            public ApplyFunc post;
            public Cursor cursor;
            public iterator iter;
        }

        private static void apply(this ptr<application> _addr_a, ast.Node parent, @string name, ptr<iterator> _addr_iter, ast.Node n) => func((_, panic, __) =>
        {
            ref application a = ref _addr_a.val;
            ref iterator iter = ref _addr_iter.val;
 
            // convert typed nil into untyped nil
            {
                var v = reflect.ValueOf(n);

                if (v.Kind() == reflect.Ptr && v.IsNil())
                {
                    n = null;
                } 

                // avoid heap-allocating a new cursor for each apply call; reuse a.cursor instead

            } 

            // avoid heap-allocating a new cursor for each apply call; reuse a.cursor instead
            var saved = a.cursor;
            a.cursor.parent = parent;
            a.cursor.name = name;
            a.cursor.iter = iter;
            a.cursor.node = n;

            if (a.pre != null && !a.pre(_addr_a.cursor))
            {
                a.cursor = saved;
                return ;
            } 

            // walk children
            // (the order of the cases matches the order of the corresponding node types in go/ast)
            switch (n.type())
            {
                case 
                    break;
                case ptr<ast.Comment> n:
                    break;
                case ptr<ast.CommentGroup> n:
                    if (n != null)
                    {
                        a.applyList(n, "List");
                    }

                    break;
                case ptr<ast.Field> n:
                    a.apply(n, "Doc", null, n.Doc);
                    a.applyList(n, "Names");
                    a.apply(n, "Type", null, n.Type);
                    a.apply(n, "Tag", null, n.Tag);
                    a.apply(n, "Comment", null, n.Comment);
                    break;
                case ptr<ast.FieldList> n:
                    a.applyList(n, "List"); 

                    // Expressions
                    break;
                case ptr<ast.BadExpr> n:
                    break;
                case ptr<ast.Ident> n:
                    break;
                case ptr<ast.BasicLit> n:
                    break;
                case ptr<ast.Ellipsis> n:
                    a.apply(n, "Elt", null, n.Elt);
                    break;
                case ptr<ast.FuncLit> n:
                    a.apply(n, "Type", null, n.Type);
                    a.apply(n, "Body", null, n.Body);
                    break;
                case ptr<ast.CompositeLit> n:
                    a.apply(n, "Type", null, n.Type);
                    a.applyList(n, "Elts");
                    break;
                case ptr<ast.ParenExpr> n:
                    a.apply(n, "X", null, n.X);
                    break;
                case ptr<ast.SelectorExpr> n:
                    a.apply(n, "X", null, n.X);
                    a.apply(n, "Sel", null, n.Sel);
                    break;
                case ptr<ast.IndexExpr> n:
                    a.apply(n, "X", null, n.X);
                    a.apply(n, "Index", null, n.Index);
                    break;
                case ptr<ast.SliceExpr> n:
                    a.apply(n, "X", null, n.X);
                    a.apply(n, "Low", null, n.Low);
                    a.apply(n, "High", null, n.High);
                    a.apply(n, "Max", null, n.Max);
                    break;
                case ptr<ast.TypeAssertExpr> n:
                    a.apply(n, "X", null, n.X);
                    a.apply(n, "Type", null, n.Type);
                    break;
                case ptr<ast.CallExpr> n:
                    a.apply(n, "Fun", null, n.Fun);
                    a.applyList(n, "Args");
                    break;
                case ptr<ast.StarExpr> n:
                    a.apply(n, "X", null, n.X);
                    break;
                case ptr<ast.UnaryExpr> n:
                    a.apply(n, "X", null, n.X);
                    break;
                case ptr<ast.BinaryExpr> n:
                    a.apply(n, "X", null, n.X);
                    a.apply(n, "Y", null, n.Y);
                    break;
                case ptr<ast.KeyValueExpr> n:
                    a.apply(n, "Key", null, n.Key);
                    a.apply(n, "Value", null, n.Value); 

                    // Types
                    break;
                case ptr<ast.ArrayType> n:
                    a.apply(n, "Len", null, n.Len);
                    a.apply(n, "Elt", null, n.Elt);
                    break;
                case ptr<ast.StructType> n:
                    a.apply(n, "Fields", null, n.Fields);
                    break;
                case ptr<ast.FuncType> n:
                    a.apply(n, "Params", null, n.Params);
                    a.apply(n, "Results", null, n.Results);
                    break;
                case ptr<ast.InterfaceType> n:
                    a.apply(n, "Methods", null, n.Methods);
                    break;
                case ptr<ast.MapType> n:
                    a.apply(n, "Key", null, n.Key);
                    a.apply(n, "Value", null, n.Value);
                    break;
                case ptr<ast.ChanType> n:
                    a.apply(n, "Value", null, n.Value); 

                    // Statements
                    break;
                case ptr<ast.BadStmt> n:
                    break;
                case ptr<ast.DeclStmt> n:
                    a.apply(n, "Decl", null, n.Decl);
                    break;
                case ptr<ast.EmptyStmt> n:
                    break;
                case ptr<ast.LabeledStmt> n:
                    a.apply(n, "Label", null, n.Label);
                    a.apply(n, "Stmt", null, n.Stmt);
                    break;
                case ptr<ast.ExprStmt> n:
                    a.apply(n, "X", null, n.X);
                    break;
                case ptr<ast.SendStmt> n:
                    a.apply(n, "Chan", null, n.Chan);
                    a.apply(n, "Value", null, n.Value);
                    break;
                case ptr<ast.IncDecStmt> n:
                    a.apply(n, "X", null, n.X);
                    break;
                case ptr<ast.AssignStmt> n:
                    a.applyList(n, "Lhs");
                    a.applyList(n, "Rhs");
                    break;
                case ptr<ast.GoStmt> n:
                    a.apply(n, "Call", null, n.Call);
                    break;
                case ptr<ast.DeferStmt> n:
                    a.apply(n, "Call", null, n.Call);
                    break;
                case ptr<ast.ReturnStmt> n:
                    a.applyList(n, "Results");
                    break;
                case ptr<ast.BranchStmt> n:
                    a.apply(n, "Label", null, n.Label);
                    break;
                case ptr<ast.BlockStmt> n:
                    a.applyList(n, "List");
                    break;
                case ptr<ast.IfStmt> n:
                    a.apply(n, "Init", null, n.Init);
                    a.apply(n, "Cond", null, n.Cond);
                    a.apply(n, "Body", null, n.Body);
                    a.apply(n, "Else", null, n.Else);
                    break;
                case ptr<ast.CaseClause> n:
                    a.applyList(n, "List");
                    a.applyList(n, "Body");
                    break;
                case ptr<ast.SwitchStmt> n:
                    a.apply(n, "Init", null, n.Init);
                    a.apply(n, "Tag", null, n.Tag);
                    a.apply(n, "Body", null, n.Body);
                    break;
                case ptr<ast.TypeSwitchStmt> n:
                    a.apply(n, "Init", null, n.Init);
                    a.apply(n, "Assign", null, n.Assign);
                    a.apply(n, "Body", null, n.Body);
                    break;
                case ptr<ast.CommClause> n:
                    a.apply(n, "Comm", null, n.Comm);
                    a.applyList(n, "Body");
                    break;
                case ptr<ast.SelectStmt> n:
                    a.apply(n, "Body", null, n.Body);
                    break;
                case ptr<ast.ForStmt> n:
                    a.apply(n, "Init", null, n.Init);
                    a.apply(n, "Cond", null, n.Cond);
                    a.apply(n, "Post", null, n.Post);
                    a.apply(n, "Body", null, n.Body);
                    break;
                case ptr<ast.RangeStmt> n:
                    a.apply(n, "Key", null, n.Key);
                    a.apply(n, "Value", null, n.Value);
                    a.apply(n, "X", null, n.X);
                    a.apply(n, "Body", null, n.Body); 

                    // Declarations
                    break;
                case ptr<ast.ImportSpec> n:
                    a.apply(n, "Doc", null, n.Doc);
                    a.apply(n, "Name", null, n.Name);
                    a.apply(n, "Path", null, n.Path);
                    a.apply(n, "Comment", null, n.Comment);
                    break;
                case ptr<ast.ValueSpec> n:
                    a.apply(n, "Doc", null, n.Doc);
                    a.applyList(n, "Names");
                    a.apply(n, "Type", null, n.Type);
                    a.applyList(n, "Values");
                    a.apply(n, "Comment", null, n.Comment);
                    break;
                case ptr<ast.TypeSpec> n:
                    a.apply(n, "Doc", null, n.Doc);
                    a.apply(n, "Name", null, n.Name);
                    a.apply(n, "Type", null, n.Type);
                    a.apply(n, "Comment", null, n.Comment);
                    break;
                case ptr<ast.BadDecl> n:
                    break;
                case ptr<ast.GenDecl> n:
                    a.apply(n, "Doc", null, n.Doc);
                    a.applyList(n, "Specs");
                    break;
                case ptr<ast.FuncDecl> n:
                    a.apply(n, "Doc", null, n.Doc);
                    a.apply(n, "Recv", null, n.Recv);
                    a.apply(n, "Name", null, n.Name);
                    a.apply(n, "Type", null, n.Type);
                    a.apply(n, "Body", null, n.Body); 

                    // Files and packages
                    break;
                case ptr<ast.File> n:
                    a.apply(n, "Doc", null, n.Doc);
                    a.apply(n, "Name", null, n.Name);
                    a.applyList(n, "Decls"); 
                    // Don't walk n.Comments; they have either been walked already if
                    // they are Doc comments, or they can be easily walked explicitly.
                    break;
                case ptr<ast.Package> n:
                    slice<@string> names = default;
                    {
                        var name__prev1 = name;

                        foreach (var (__name) in n.Files)
                        {
                            name = __name;
                            names = append(names, name);
                        }

                        name = name__prev1;
                    }

                    sort.Strings(names);
                    {
                        var name__prev1 = name;

                        foreach (var (_, __name) in names)
                        {
                            name = __name;
                            a.apply(n, name, null, n.Files[name]);
                        }

                        name = name__prev1;
                    }
                    break;
                default:
                {
                    var n = n.type();
                    panic(fmt.Sprintf("Apply: unexpected node type %T", n));
                    break;
                }

            }

            if (a.post != null && !a.post(_addr_a.cursor))
            {
                panic(abort);
            }

            a.cursor = saved;

        });

        // An iterator controls iteration over a slice of nodes.
        private partial struct iterator
        {
            public long index;
            public long step;
        }

        private static void applyList(this ptr<application> _addr_a, ast.Node parent, @string name)
        {
            ref application a = ref _addr_a.val;
 
            // avoid heap-allocating a new iterator for each applyList call; reuse a.iter instead
            var saved = a.iter;
            a.iter.index = 0L;
            while (true)
            { 
                // must reload parent.name each time, since cursor modifications might change it
                var v = reflect.Indirect(reflect.ValueOf(parent)).FieldByName(name);
                if (a.iter.index >= v.Len())
                {
                    break;
                } 

                // element x may be nil in a bad AST - be cautious
                ast.Node x = default;
                {
                    var e = v.Index(a.iter.index);

                    if (e.IsValid())
                    {
                        x = e.Interface()._<ast.Node>();
                    }

                }


                a.iter.step = 1L;
                a.apply(parent, name, _addr_a.iter, x);
                a.iter.index += a.iter.step;

            }

            a.iter = saved;

        }
    }
}}}}}}
