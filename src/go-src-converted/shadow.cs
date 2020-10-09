// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package shadow defines an Analyzer that checks for shadowed variables.
// package shadow -- go2cs converted at 2020 October 09 06:04:12 UTC
// import "golang.org/x/tools/go/analysis/passes/shadow" ==> using shadow = go.golang.org.x.tools.go.analysis.passes.shadow_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\shadow\shadow.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class shadow_package
    {
        // NOTE: Experimental. Not part of the vet suite.
        public static readonly @string Doc = (@string)@"check for possible unintended shadowing of variables

This analyzer check for shadowed variables.
A shadowed variable is a variable declared in an inner scope
with the same name and type as a variable in an outer scope,
and where the outer variable is mentioned after the inner one
is declared.

(This definition can be refined; the module generates too many
false positives and is not yet enabled by default.)

For example:

	func BadRead(f *os.File, buf []byte) error {
		var err error
		for {
			n, err := f.Read(buf) // shadows the function variable 'err'
			if err != nil {
				break // causes return of wrong value
			}
			foo(buf)
		}
		return err
	}
";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"shadow",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

        // flags
        private static var strict = false;

        private static void init()
        {
            Analyzer.Flags.BoolVar(_addr_strict, "strict", strict, "whether to be strict about shadowing; can be noisy");
        }

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

            var spans = make_map<types.Object, span>();
            {
                var id__prev1 = id;
                var obj__prev1 = obj;

                foreach (var (__id, __obj) in pass.TypesInfo.Defs)
                {
                    id = __id;
                    obj = __obj; 
                    // Ignore identifiers that don't denote objects
                    // (package names, symbolic variables such as t
                    // in t := x.(type) of type switch headers).
                    if (obj != null)
                    {
                        growSpan(spans, obj, id.Pos(), id.End());
                    }

                }

                id = id__prev1;
                obj = obj__prev1;
            }

            {
                var id__prev1 = id;
                var obj__prev1 = obj;

                foreach (var (__id, __obj) in pass.TypesInfo.Uses)
                {
                    id = __id;
                    obj = __obj;
                    growSpan(spans, obj, id.Pos(), id.End());
                }

                id = id__prev1;
                obj = obj__prev1;
            }

            {
                var obj__prev1 = obj;

                foreach (var (__node, __obj) in pass.TypesInfo.Implicits)
                {
                    node = __node;
                    obj = __obj; 
                    // A type switch with a short variable declaration
                    // such as t := x.(type) doesn't declare the symbolic
                    // variable (t in the example) at the switch header;
                    // instead a new variable t (with specific type) is
                    // declared implicitly for each case. Such variables
                    // are found in the types.Info.Implicits (not Defs)
                    // map. Add them here, assuming they are declared at
                    // the type cases' colon ":".
                    {
                        ptr<ast.CaseClause> (cc, ok) = node._<ptr<ast.CaseClause>>();

                        if (ok)
                        {
                            growSpan(spans, obj, cc.Colon, cc.Colon);
                        }

                    }

                }

                obj = obj__prev1;
            }

            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.AssignStmt)(nil), (*ast.GenDecl)(nil) });
            inspect.Preorder(nodeFilter, n =>
            {
                switch (n.type())
                {
                    case ptr<ast.AssignStmt> n:
                        checkShadowAssignment(_addr_pass, spans, _addr_n);
                        break;
                    case ptr<ast.GenDecl> n:
                        checkShadowDecl(_addr_pass, spans, _addr_n);
                        break;
                }

            });
            return (null, error.As(null!)!);

        }

        // A span stores the minimum range of byte positions in the file in which a
        // given variable (types.Object) is mentioned. It is lexically defined: it spans
        // from the beginning of its first mention to the end of its last mention.
        // A variable is considered shadowed (if strict is off) only if the
        // shadowing variable is declared within the span of the shadowed variable.
        // In other words, if a variable is shadowed but not used after the shadowed
        // variable is declared, it is inconsequential and not worth complaining about.
        // This simple check dramatically reduces the nuisance rate for the shadowing
        // check, at least until something cleverer comes along.
        //
        // One wrinkle: A "naked return" is a silent use of a variable that the Span
        // will not capture, but the compilers catch naked returns of shadowed
        // variables so we don't need to.
        //
        // Cases this gets wrong (TODO):
        // - If a for loop's continuation statement mentions a variable redeclared in
        // the block, we should complain about it but don't.
        // - A variable declared inside a function literal can falsely be identified
        // as shadowing a variable in the outer function.
        //
        private partial struct span
        {
            public token.Pos min;
            public token.Pos max;
        }

        // contains reports whether the position is inside the span.
        private static bool contains(this span s, token.Pos pos)
        {
            return s.min <= pos && pos < s.max;
        }

        // growSpan expands the span for the object to contain the source range [pos, end).
        private static void growSpan(map<types.Object, span> spans, types.Object obj, token.Pos pos, token.Pos end)
        {
            if (strict)
            {
                return ; // No need
            }

            var (s, ok) = spans[obj];
            if (ok)
            {
                if (s.min > pos)
                {
                    s.min = pos;
                }

                if (s.max < end)
                {
                    s.max = end;
                }

            }
            else
            {
                s = new span(pos,end);
            }

            spans[obj] = s;

        }

        // checkShadowAssignment checks for shadowing in a short variable declaration.
        private static void checkShadowAssignment(ptr<analysis.Pass> _addr_pass, map<types.Object, span> spans, ptr<ast.AssignStmt> _addr_a)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.AssignStmt a = ref _addr_a.val;

            if (a.Tok != token.DEFINE)
            {
                return ;
            }

            if (idiomaticShortRedecl(_addr_pass, _addr_a))
            {
                return ;
            }

            foreach (var (_, expr) in a.Lhs)
            {
                ptr<ast.Ident> (ident, ok) = expr._<ptr<ast.Ident>>();
                if (!ok)
                {
                    pass.ReportRangef(expr, "invalid AST: short variable declaration of non-identifier");
                    return ;
                }

                checkShadowing(_addr_pass, spans, ident);

            }

        }

        // idiomaticShortRedecl reports whether this short declaration can be ignored for
        // the purposes of shadowing, that is, that any redeclarations it contains are deliberate.
        private static bool idiomaticShortRedecl(ptr<analysis.Pass> _addr_pass, ptr<ast.AssignStmt> _addr_a)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.AssignStmt a = ref _addr_a.val;
 
            // Don't complain about deliberate redeclarations of the form
            //    i := i
            // Such constructs are idiomatic in range loops to create a new variable
            // for each iteration. Another example is
            //    switch n := n.(type)
            if (len(a.Rhs) != len(a.Lhs))
            {
                return false;
            } 
            // We know it's an assignment, so the LHS must be all identifiers. (We check anyway.)
            foreach (var (i, expr) in a.Lhs)
            {
                ptr<ast.Ident> (lhs, ok) = expr._<ptr<ast.Ident>>();
                if (!ok)
                {
                    pass.ReportRangef(expr, "invalid AST: short variable declaration of non-identifier");
                    return true; // Don't do any more processing.
                }

                switch (a.Rhs[i].type())
                {
                    case ptr<ast.Ident> rhs:
                        if (lhs.Name != rhs.Name)
                        {
                            return false;
                        }

                        break;
                    case ptr<ast.TypeAssertExpr> rhs:
                        {
                            ptr<ast.Ident> (id, ok) = rhs.X._<ptr<ast.Ident>>();

                            if (ok)
                            {
                                if (lhs.Name != id.Name)
                                {
                                    return false;
                                }

                            }

                        }

                        break;
                    default:
                    {
                        var rhs = a.Rhs[i].type();
                        return false;
                        break;
                    }
                }

            }
            return true;

        }

        // idiomaticRedecl reports whether this declaration spec can be ignored for
        // the purposes of shadowing, that is, that any redeclarations it contains are deliberate.
        private static bool idiomaticRedecl(ptr<ast.ValueSpec> _addr_d)
        {
            ref ast.ValueSpec d = ref _addr_d.val;
 
            // Don't complain about deliberate redeclarations of the form
            //    var i, j = i, j
            // Don't ignore redeclarations of the form
            //    var i = 3
            if (len(d.Names) != len(d.Values))
            {
                return false;
            }

            foreach (var (i, lhs) in d.Names)
            {
                ptr<ast.Ident> (rhs, ok) = d.Values[i]._<ptr<ast.Ident>>();
                if (!ok || lhs.Name != rhs.Name)
                {
                    return false;
                }

            }
            return true;

        }

        // checkShadowDecl checks for shadowing in a general variable declaration.
        private static void checkShadowDecl(ptr<analysis.Pass> _addr_pass, map<types.Object, span> spans, ptr<ast.GenDecl> _addr_d)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.GenDecl d = ref _addr_d.val;

            if (d.Tok != token.VAR)
            {
                return ;
            }

            foreach (var (_, spec) in d.Specs)
            {
                ptr<ast.ValueSpec> (valueSpec, ok) = spec._<ptr<ast.ValueSpec>>();
                if (!ok)
                {
                    pass.ReportRangef(spec, "invalid AST: var GenDecl not ValueSpec");
                    return ;
                } 
                // Don't complain about deliberate redeclarations of the form
                //    var i = i
                if (idiomaticRedecl(valueSpec))
                {
                    return ;
                }

                foreach (var (_, ident) in valueSpec.Names)
                {
                    checkShadowing(_addr_pass, spans, _addr_ident);
                }

            }

        }

        // checkShadowing checks whether the identifier shadows an identifier in an outer scope.
        private static void checkShadowing(ptr<analysis.Pass> _addr_pass, map<types.Object, span> spans, ptr<ast.Ident> _addr_ident)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.Ident ident = ref _addr_ident.val;

            if (ident.Name == "_")
            { 
                // Can't shadow the blank identifier.
                return ;

            }

            var obj = pass.TypesInfo.Defs[ident];
            if (obj == null)
            {
                return ;
            } 
            // obj.Parent.Parent is the surrounding scope. If we can find another declaration
            // starting from there, we have a shadowed identifier.
            var (_, shadowed) = obj.Parent().Parent().LookupParent(obj.Name(), obj.Pos());
            if (shadowed == null)
            {
                return ;
            } 
            // Don't complain if it's shadowing a universe-declared identifier; that's fine.
            if (shadowed.Parent() == types.Universe)
            {
                return ;
            }

            if (strict)
            { 
                // The shadowed identifier must appear before this one to be an instance of shadowing.
                if (shadowed.Pos() > ident.Pos())
                {
                    return ;
                }

            }
            else
            { 
                // Don't complain if the span of validity of the shadowed identifier doesn't include
                // the shadowing identifier.
                var (span, ok) = spans[shadowed];
                if (!ok)
                {
                    pass.ReportRangef(ident, "internal error: no range for %q", ident.Name);
                    return ;
                }

                if (!span.contains(ident.Pos()))
                {
                    return ;
                }

            } 
            // Don't complain if the types differ: that implies the programmer really wants two different things.
            if (types.Identical(obj.Type(), shadowed.Type()))
            {
                var line = pass.Fset.Position(shadowed.Pos()).Line;
                pass.ReportRangef(ident, "declaration of %q shadows declaration at line %d", obj.Name(), line);
            }

        }
    }
}}}}}}}
