package main

import (
	"go/ast"
	"go/token"
	"go/types"
)

// The escape analysis function is used to determine if a variable escapes the current
// scope and thus needs to be heap allocated. This is important for C# code generation
// since Go allows variables to escape the current scope automatically, adding them to
// the heap, behind the scenes. C# does not have this feature, so we need to manually
// determine if a variable needs to be heap allocated.

func (v *Visitor) performEscapeAnalysis(ident *ast.Ident, parentBlock *ast.BlockStmt) {
	if parentBlock == nil {
		return
	}

	// If analysis has already been performed, return
	if _, found := v.identEscapesHeap[ident]; found {
		return
	}

	identObj := v.info.ObjectOf(ident)

	if identObj == nil {
		return // Could not find the object of ident
	}

	// Check if the type is inherently heap allocated
	if isInherentlyHeapAllocatedType(identObj.Type()) {
		v.identEscapesHeap[ident] = true
		return
	}

	escapes := false

	// Helper function to check if identObj occurs within an expression
	containsIdent := func(node ast.Node) bool {
		found := false

		ast.Inspect(node, func(n ast.Node) bool {
			if found {
				return false // Stop if already found
			}

			if id := getIdentifier(n); id != nil {
				obj := v.info.ObjectOf(id)

				if obj == identObj {
					found = true
					return false
				}
			}

			return true
		})

		return found
	}

	// Visitor function to traverse the AST
	inspectFunc := func(node ast.Node) bool {
		if escapes {
			return false // Stop traversal if escape is found
		}

		switch n := node.(type) {
		case *ast.UnaryExpr:
			// Check if ident is used in an address-of operation
			if n.Op == token.AND {
				if containsIdent(n.X) {
					// The address of the ident is taken
					escapes = true
					return false
				}
			}

		case *ast.CallExpr:
			// Check if ident is passed as an argument
			for i, arg := range n.Args {
				if containsIdent(arg) {
					// Get the function type
					funType := v.info.TypeOf(n.Fun)

					sig, ok := funType.Underlying().(*types.Signature)

					if !ok {
						continue
					}

					var paramType types.Type

					if paramType, ok = getParameterType(sig, i); !ok {
						continue
					}

					// Check if paramType is a pointer type
					if _, ok := paramType.Underlying().(*types.Pointer); ok {
						// Passed as a pointer; may cause escape
						escapes = true
						return false
					}

					// We do not currently consider interface types as causing an escape since
					// in C# value types are boxed as needed making value basically read-only,
					// thus matching Go semantics
				}
			}

		case *ast.FuncLit:
			// Check if ident is used inside a closure
			closureContainsIdent := false

			ast.Inspect(n.Body, func(n ast.Node) bool {
				if closureContainsIdent {
					return false
				}

				if id := getIdentifier(n); id != nil {
					obj := v.info.ObjectOf(id)

					if obj == identObj {
						closureContainsIdent = true
						return false
					}
				}

				return true
			})

			if closureContainsIdent {
				// For now, we assume that variables captured by closures might escape
				escapes = true
				return false
			}
		}

		return true // Continue traversing
	}

	ast.Inspect(parentBlock, inspectFunc)

	v.identEscapesHeap[ident] = escapes
}
