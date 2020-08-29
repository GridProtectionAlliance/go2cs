// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 August 29 08:47:28 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\eval.go
using fmt = go.fmt_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // Eval returns the type and, if constant, the value for the
        // expression expr, evaluated at position pos of package pkg,
        // which must have been derived from type-checking an AST with
        // complete position information relative to the provided file
        // set.
        //
        // If the expression contains function literals, their bodies
        // are ignored (i.e., the bodies are not type-checked).
        //
        // If pkg == nil, the Universe scope is used and the provided
        // position pos is ignored. If pkg != nil, and pos is invalid,
        // the package scope is used. Otherwise, pos must belong to the
        // package.
        //
        // An error is returned if pos is not within the package or
        // if the node cannot be evaluated.
        //
        // Note: Eval should not be used instead of running Check to compute
        // types and values, but in addition to Check. Eval will re-evaluate
        // its argument each time, and it also does not know about the context
        // in which an expression is used (e.g., an assignment). Thus, top-
        // level untyped constants will return an untyped type rather then the
        // respective context-specific type.
        //
        public static (TypeAndValue, error) Eval(ref token.FileSet _fset, ref Package _pkg, token.Pos pos, @string expr) => func(_fset, _pkg, (ref token.FileSet fset, ref Package pkg, Defer defer, Panic _, Recover __) =>
        { 
            // determine scope
            ref Scope scope = default;
            if (pkg == null)
            {
                scope = Universe;
                pos = token.NoPos;
            }
            else if (!pos.IsValid())
            {
                scope = pkg.scope;
            }
            else
            { 
                // The package scope extent (position information) may be
                // incorrect (files spread across a wide range of fset
                // positions) - ignore it and just consider its children
                // (file scopes).
                foreach (var (_, fscope) in pkg.scope.children)
                {
                    scope = fscope.Innermost(pos);

                    if (scope != null)
                    {
                        break;
                    }
                }                if (scope == null || debug)
                {
                    var s = scope;
                    while (s != null && s != pkg.scope)
                    {
                        s = s.parent;
                    } 
                    // s == nil || s == pkg.scope
                    if (s == null)
                    {
                        return (new TypeAndValue(), fmt.Errorf("no position %s found in package %s", fset.Position(pos), pkg.name));
                    }
                }
            }
            var (node, err) = parser.ParseExprFrom(fset, "eval", expr, 0L);
            if (err != null)
            {
                return (new TypeAndValue(), err);
            }
            var check = NewChecker(null, fset, pkg, null);
            check.scope = scope;
            check.pos = pos;
            defer(check.handleBailout(ref err)); 

            // evaluate node
            operand x = default;
            check.rawExpr(ref x, node, null);
            return (new TypeAndValue(x.mode,x.typ,x.val), err);
        });
    }
}}
