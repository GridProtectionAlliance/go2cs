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

	// Optimization for basic types: skip detailed analysis for basic types
	// that are used in simple, non-escaping patterns
	if isBasicType(identObj.Type()) && isSimpleUsage(ident, parentBlock, v.info) {
		v.identEscapesHeap[identObj] = false
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
					// The address of the ident is taken, simple response
					// is to just assume it escapes. Future iterations
					// may be able to provide more nuance and use C# ref
					// structure operations to avoid heap allocation
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

// Helper function to determine if a type is a basic value type
func isBasicType(t types.Type) bool {
	if t == nil {
		return false
	}

	basic, ok := t.Underlying().(*types.Basic)
	if !ok {
		return false
	}

	// Check if it's a non-reference basic type
	kind := basic.Kind()
	return kind >= types.Bool && kind <= types.Float64
}

// Helper function to check if a variable has a simple usage pattern
// that doesn't require heap allocation (like an integer counter)
func isSimpleUsage(ident *ast.Ident, block *ast.BlockStmt, info *types.Info) bool {
	if block == nil {
		return false
	}

	// Get the variable object
	identObj := info.ObjectOf(ident)

	if identObj == nil {
		return false
	}

	// Track if the ident is used in simple operations
	addressTaken := false
	assignedTo := false
	usedInCondition := false
	usedInIncrement := false

	// Check usage patterns
	ast.Inspect(block, func(n ast.Node) bool {
		if addressTaken {
			return false // Stop inspection if address is taken
		}

		switch node := n.(type) {
		case *ast.UnaryExpr:
			// Check if ident's address is taken
			if node.Op == token.AND {
				if id := getIdentifier(node.X); id != nil {
					if obj := info.ObjectOf(id); obj == identObj {
						addressTaken = true
						return false
					}
				}
			}

			// Check for increment/decrement
			if node.Op == token.INC || node.Op == token.DEC {
				if id := getIdentifier(node.X); id != nil {
					if obj := info.ObjectOf(id); obj == identObj {
						usedInIncrement = true
					}
				}
			}

		case *ast.AssignStmt:
			// Check if ident is assigned to
			for _, lhs := range node.Lhs {
				if id := getIdentifier(lhs); id != nil {
					if obj := info.ObjectOf(id); obj == identObj {
						assignedTo = true
						break
					}
				}
			}

			// Check if ident is used in assignment
			if node.Tok == token.ADD_ASSIGN || node.Tok == token.SUB_ASSIGN {
				for _, lhs := range node.Lhs {
					if id := getIdentifier(lhs); id != nil {
						if obj := info.ObjectOf(id); obj == identObj {
							usedInIncrement = true
							break
						}
					}
				}
			}

		case *ast.BinaryExpr:
			// Check if ident is used in conditions
			if isComparisonOperator(node.Op) || isLogicalOperator(node.Op) {
				if id := getIdentifier(node.X); id != nil {
					if obj := info.ObjectOf(id); obj == identObj {
						usedInCondition = true
					}
				}

				if id := getIdentifier(node.Y); id != nil {
					if obj := info.ObjectOf(id); obj == identObj {
						usedInCondition = true
					}
				}
			}

		case *ast.FuncLit:
			// Check for presence in closures - simple values can still be captured
			// without taking addresses if they're not mutated
			ast.Inspect(node.Body, func(n ast.Node) bool {
				if id := getIdentifier(n); id != nil {
					if obj := info.ObjectOf(id); obj == identObj {
						// If it appears in an assignment, we need to do a further check
						// to see if this identifier is on the left-hand side
						if assign, ok := n.(*ast.AssignStmt); ok {
							// This is an assignment statement, but we need to check
							// other identifiers to see if our target is on the LHS
							for _, lhs := range assign.Lhs {
								if idLHS := getIdentifier(lhs); idLHS != nil {
									if objLHS := info.ObjectOf(idLHS); objLHS == identObj {
										addressTaken = true
										return false
									}
								}
							}
						}
					}
				}
				return true
			})
		}

		return true
	})

	// Simple usage pattern: a basic variable that's used only for counting or conditions
	// and never has its address taken
	return !addressTaken && (usedInIncrement || (assignedTo && usedInCondition))
}
