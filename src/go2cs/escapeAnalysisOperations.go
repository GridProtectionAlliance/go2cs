package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"sync"
)

// The escape analysis function is used to determine if a variable escapes the current
// stack and thus needs to be heap allocated. This is important for C# code generation
// since Go allows variables to escape the current stack automatically, adding them to
// the heap, behind the scenes. C# does not have this feature, so we need to manually
// determine if a variable needs to be heap allocated. The map that is created as a
// result of this analysis is called `identEscapesHeap`.

// Implementation of the escape analysis is currently very basic and only covers cases
// within a single function considering options where a variable "may" escape. It does
// not consider cases where a variable is passed to another function and may not need
// to be heap allocated and could be handled by using C# ref structure operations.

// Future implementations could consider functions that (a) are within the same package
// and (b) have private scope, that could look ahead for parameter uses that could use
// a C# ref structure instead of using a heap allocation with a ж<T> (pointer type).

func performEscapeAnalysis(files []FileEntry, fset *token.FileSet, pkg *types.Package, info *types.Info) {
	var concurrentTasks sync.WaitGroup

	for _, fileEntry := range files {
		concurrentTasks.Add(1)

		go func(fileEntry FileEntry) {
			defer concurrentTasks.Done()

			visitor := &Visitor{
				fset:             fset,
				pkg:              pkg,
				info:             info,
				identEscapesHeap: fileEntry.identEscapesHeap,
			}

			ast.Inspect(fileEntry.file, func(n ast.Node) bool {
				switch node := n.(type) {
				case *ast.FuncDecl:
					if node.Body == nil {
						return true
					}

					ast.Inspect(node.Body, func(n ast.Node) bool {
						switch n := n.(type) {
						case *ast.AssignStmt:
							if n.Tok == token.DEFINE {
								for _, lhs := range n.Lhs {
									if ident := getIdentifier(lhs); ident != nil {
										visitor.performEscapeAnalysis(ident, node.Body)
									}
								}
							}
						case *ast.RangeStmt:
							if n.Tok == token.DEFINE {
								if key := getIdentifier(n.Key); key != nil {
									visitor.performEscapeAnalysis(key, node.Body)
								}
								if value := getIdentifier(n.Value); value != nil {
									visitor.performEscapeAnalysis(value, node.Body)
								}
							}
						case *ast.DeclStmt:
							if genDecl, ok := n.Decl.(*ast.GenDecl); ok {
								for _, spec := range genDecl.Specs {
									if valueSpec, ok := spec.(*ast.ValueSpec); ok {
										for _, ident := range valueSpec.Names {
											if !isDiscardedVar(ident.Name) {
												visitor.performEscapeAnalysis(ident, node.Body)
											}
										}
									}
								}
							}
						case *ast.ForStmt:
							if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
								for _, lhs := range init.Lhs {
									if ident := getIdentifier(lhs); ident != nil {
										visitor.performEscapeAnalysis(ident, node.Body)
									}
								}
							}
						case *ast.IfStmt:
							if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
								for _, lhs := range init.Lhs {
									if ident := getIdentifier(lhs); ident != nil {
										visitor.performEscapeAnalysis(ident, node.Body)
									}
								}
							}
						case *ast.SwitchStmt:
							if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
								for _, lhs := range init.Lhs {
									if ident := getIdentifier(lhs); ident != nil {
										visitor.performEscapeAnalysis(ident, node.Body)
									}
								}
							}
						case *ast.TypeSwitchStmt:
							if assign, ok := n.Assign.(*ast.AssignStmt); ok && assign.Tok == token.DEFINE {
								for _, lhs := range assign.Lhs {
									if ident := getIdentifier(lhs); ident != nil {
										visitor.performEscapeAnalysis(ident, node.Body)
									}
								}
							}
						}
						return true
					})
				}
				return true
			})
		}(fileEntry)
	}

	concurrentTasks.Wait()
}

// Perform escape analysis on the given identifier within the specified block
func (v *Visitor) performEscapeAnalysis(ident *ast.Ident, parentBlock *ast.BlockStmt) {
	if parentBlock == nil {
		return
	}

	// If analysis has already been performed, return
	identObj := v.info.ObjectOf(ident)

	if identObj == nil {
		return // Could not find the object of ident
	}

	if _, found := v.identEscapesHeap[identObj]; found {
		return
	}

	// Check if the type is inherently heap allocated
	if isInherentlyHeapAllocatedType(identObj.Type()) {
		v.identEscapesHeap[identObj] = true
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
				// Direct address of identifier
				if id, ok := n.X.(*ast.Ident); ok {
					if obj := v.info.ObjectOf(id); obj == identObj {
						escapes = true
						return false
					}
				}

				// Address of array/slice element
				if indexExpr, ok := n.X.(*ast.IndexExpr); ok {
					if id, ok := indexExpr.X.(*ast.Ident); ok {
						if obj := v.info.ObjectOf(id); obj == identObj {
							escapes = true
							return false
						}
					}
				}

				// For composite literals, special case:
				// If the variable is used only as part of calculating a value for a field,
				// it doesn't escape to the heap
				if compLit, ok := n.X.(*ast.CompositeLit); ok {
					// First check if our identifier is used in the type part
					// (like array size) - if so, it doesn't escape
					if containsIdentInTypeExpr(compLit.Type, identObj, v.info) {
						return true
					}

					// Now check if our identifier is directly stored in a field
					// or just used in a calculation
					for _, elt := range compLit.Elts {
						// Check value expressions - by default assume safe unless
						// we find a direct assignment of our identifier
						if kv, ok := elt.(*ast.KeyValueExpr); ok {
							// Key doesn't matter, only value
							if id, ok := kv.Value.(*ast.Ident); ok {
								if obj := v.info.ObjectOf(id); obj == identObj {
									// Direct assignment of our identifier to a field
									// This is a gray area - in many cases it's safe,
									// but for simplicity assume escape
									escapes = true
									return false
								}
							}

							// If our identifier is used in a calculation but not
							// directly stored, it doesn't escape
							// e.g., &Struct{field: n+1} doesn't make n escape
							if containsIdentInValueCalc(kv.Value, identObj, v.info) {
								// Don't mark as escaping - continue checking other elements
								continue
							}
						} else if id, ok := elt.(*ast.Ident); ok {
							// Direct use of identifier as element
							if obj := v.info.ObjectOf(id); obj == identObj {
								escapes = true
								return false
							}
						}
					}

					// If we get here, the identifier is only used in calculations
					// for field values, not directly stored in the composite literal
					return true
				}
			}

		case *ast.CallExpr:
			// Check if ident is passed as an argument
			for i, arg := range n.Args {
				// Skip this arg if it's a nested call expression
				if _, isNestedCall := arg.(*ast.CallExpr); isNestedCall {
					continue
				}

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

		case *ast.GoStmt:
			// Check if ident is used inside a goroutine
			goStmtContainsIdent := false
			takesAddress := false
			usedAsRef := false

			ast.Inspect(n.Call, func(n ast.Node) bool {
				if id := getIdentifier(n); id != nil {
					obj := v.info.ObjectOf(id)
					if obj == identObj {
						goStmtContainsIdent = true

						// Check if it's a value type
						if _, ok := obj.Type().Underlying().(*types.Basic); ok {
							// Value types only escape if their address is taken
							return true // continue checking for address operations
						}
						// Reference types still need to escape
						return false
					}
				}

				// Check for address-of operations
				if unary, ok := n.(*ast.UnaryExpr); ok && unary.Op == token.AND {
					if id := getIdentifier(unary.X); id != nil {
						if obj := v.info.ObjectOf(id); obj == identObj {
							takesAddress = true
							return false
						}
					}
				}

				return true
			})

			// Only escape if:
			// 1. It's a reference type used in goroutine
			// 2. It's a value type whose address is taken
			// 3. It's passed by reference somewhere
			if goStmtContainsIdent && (!isValueType(identObj.Type()) || takesAddress || usedAsRef) {
				escapes = true
				return false
			}

		case *ast.DeferStmt:
			// Check if ident is used inside a deferred function
			deferStmtContainsIdent := false
			takesAddress := false
			usedAsRef := false

			ast.Inspect(n.Call, func(n ast.Node) bool {
				if id := getIdentifier(n); id != nil {
					obj := v.info.ObjectOf(id)
					if obj == identObj {
						deferStmtContainsIdent = true

						// Check if it's a value type
						if _, ok := obj.Type().Underlying().(*types.Basic); ok {
							// Value types only escape if their address is taken
							return true // continue checking
						}
						return false
					}
				}

				// Check for address-of operations
				if unary, ok := n.(*ast.UnaryExpr); ok && unary.Op == token.AND {
					if id := getIdentifier(unary.X); id != nil {
						if obj := v.info.ObjectOf(id); obj == identObj {
							takesAddress = true
							return false
						}
					}
				}

				return true
			})

			// Only escape if necessary
			if deferStmtContainsIdent && (!isValueType(identObj.Type()) || takesAddress || usedAsRef) {
				escapes = true
				return false
			}

		case *ast.FuncLit:
			// Check if ident is used inside a closure
			closureContainsIdent := false
			takesAddress := false
			usedAsRef := false

			ast.Inspect(n.Body, func(n ast.Node) bool {
				if id := getIdentifier(n); id != nil {
					obj := v.info.ObjectOf(id)
					if obj == identObj {
						closureContainsIdent = true

						// Check if it's a value type
						if _, ok := obj.Type().Underlying().(*types.Basic); ok {
							// Value types only escape if their address is taken
							return true // continue checking for address operations
						}
						// Reference types still need to escape
						return false
					}
				}

				// Check for address-of operations
				if unary, ok := n.(*ast.UnaryExpr); ok && unary.Op == token.AND {
					if id := getIdentifier(unary.X); id != nil {
						if obj := v.info.ObjectOf(id); obj == identObj {
							takesAddress = true
							return false
						}
					}
				}

				return true
			})

			// Only escape if:
			// 1. It's a reference type used in closure
			// 2. It's a value type whose address is taken
			// 3. It's passed by reference somewhere
			escapes = (closureContainsIdent && !isValueType(identObj.Type())) ||
				takesAddress ||
				usedAsRef
		}

		return true // Continue traversing
	}

	ast.Inspect(parentBlock, inspectFunc)

	v.identEscapesHeap[identObj] = escapes
}

// Check if the identifier is used in a type expression (like array size)
func containsIdentInTypeExpr(node ast.Expr, targetObj types.Object, info *types.Info) bool {
	if node == nil {
		return false
	}

	found := false

	ast.Inspect(node, func(n ast.Node) bool {
		if found {
			return false
		}

		if id, ok := n.(*ast.Ident); ok {
			if obj := info.ObjectOf(id); obj == targetObj {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// Check if the identifier is used in a value calculation but not directly stored
func containsIdentInValueCalc(node ast.Expr, targetObj types.Object, info *types.Info) bool {
	// Direct assignment of identifier is handled separately
	if id, ok := node.(*ast.Ident); ok {
		if obj := info.ObjectOf(id); obj == targetObj {
			return false // This is direct assignment, not just calculation
		}
		return false // Some other identifier
	}

	// Check if identifier is used in a binary operation
	if binExpr, ok := node.(*ast.BinaryExpr); ok {
		return containsIdentInValueCalc(binExpr.X, targetObj, info) ||
			containsIdentInValueCalc(binExpr.Y, targetObj, info)
	}

	// Check if identifier is used in a function call argument
	if callExpr, ok := node.(*ast.CallExpr); ok {
		for _, arg := range callExpr.Args {
			if containsIdentInValueCalc(arg, targetObj, info) {
				return true
			}
		}

		return false
	}

	// For other expression types, do a general search
	found := false
	ast.Inspect(node, func(n ast.Node) bool {
		if found {
			return false
		}

		if id, ok := n.(*ast.Ident); ok {
			if obj := info.ObjectOf(id); obj == targetObj {
				found = true
				return false
			}
		}

		return true
	})

	return found
}
