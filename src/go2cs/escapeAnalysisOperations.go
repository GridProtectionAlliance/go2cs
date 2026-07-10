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

					visitor.markCaptureModeBoxedParams(node.Type.Params, node.Body)

					// Mark FUNCTION LITERAL value params BEFORE the define walk below, same as
					// the declaration's own params above: a mixed `t, y := …` re-use of a literal
					// param would otherwise record the define-walk's escape verdict first, and
					// markCaptureModeBoxedParams skips already-analyzed objects.
					ast.Inspect(node.Body, func(n ast.Node) bool {
						if funcLit, ok := n.(*ast.FuncLit); ok {
							visitor.markCaptureModeBoxedParams(funcLit.Type.Params, funcLit.Body)
						}
						return true
					})

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

				case *ast.FuncLit:
					// A literal OUTSIDE any function declaration (a package-level var
					// initializer). Literals inside a FuncDecl were already marked above —
					// the already-analyzed guard makes this re-visit a no-op for them.
					visitor.markCaptureModeBoxedParams(node.Type.Params, node.Body)
				}
				return true
			})
		}(fileEntry)
	}

	concurrentTasks.Wait()
}

// markCaptureModeBoxedParams marks the function's VALUE parameters on which the body calls a
// capture-mode (direct-ж) method — go/format's `format(…, cfg printer.Config)` calling
// `cfg.Fprint(…)`, where (*Config).Fprint is emitted with only the `this ж<Config>` receiver
// (CS1929 on the raw value). Parameters are deliberately NOT fed through the full escape
// analysis (their address-of forms use the Ꮡ(value) copy-box; see convUnaryExpr), so this is
// the ONLY writer of a parameter into identEscapesHeap — visitFuncDecl reads such an entry as
// the entry-time-box trigger (see paramNeedsHeapBox): the incoming value arrives under the
// `ʗp` name and the parameter preamble declares `ref var cfg = ref heap(cfgʗp, out var Ꮡcfg);`,
// so body uses hit the boxed alias and convSelectorExpr routes the call through `Ꮡcfg`.
// Entry-time boxing (never a call-site copy-box, which compiles but silently drops the
// callee's writes through the receiver pointer) preserves Go's by-value parameter +
// auto-address semantics exactly. Serves both function DECLARATIONS and function LITERALS
// (whose prologue/rename convFuncLit emits — see funcLitHeapBoxParamIdents).
func (v *Visitor) markCaptureModeBoxedParams(params *ast.FieldList, body *ast.BlockStmt) {
	if params == nil {
		return
	}

	for _, field := range params.List {
		// A variadic parameter already re-declares its Go name in the prologue
		// (`var xs = xsʗp.slice();`), and its unnamed []T type carries no methods.
		if _, isVariadic := field.Type.(*ast.Ellipsis); isVariadic {
			continue
		}

		for _, ident := range field.Names {
			if isDiscardedVar(ident.Name) {
				continue
			}

			obj := v.info.ObjectOf(ident)

			if obj == nil {
				continue
			}

			// A pointer parameter already carries its box (`Ꮡp` IS the emitted parameter).
			if _, isPointer := obj.Type().(*types.Pointer); isPointer {
				continue
			}

			if _, found := v.identEscapesHeap[obj]; found {
				continue
			}

			if v.bodyCallsCaptureModeMethodOn(ident, body) {
				v.identEscapesHeap[obj] = true

				// An inherently-heap value (named slice/map/chan) is already a reference, so
				// identHasHeapBox boxes it only for a recorded capture-mode reason — same rule
				// as the local-var arm in performEscapeAnalysis.
				if packageCaptureModeBoxIdents != nil && isInherentlyHeapAllocatedType(obj.Type()) {
					packageCaptureModeBoxIdents[obj] = true
				}
			}
		}
	}
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

		// An inherently-heap value var is already a reference, so identHasHeapBox does NOT box it
		// by default. But a capture-mode pointer-receiver method called on it (`frontier.Push(…)`
		// with Push/Pop on `*orderEventList`, a NAMED SLICE) needs the ж overload's receiver box
		// (CS1929 without it). Record that reason so identHasHeapBox forces the box for exactly
		// these vars (a non-inherently-heap struct like atomic.Int32 is already boxed below).
		if packageCaptureModeBoxIdents != nil && v.bodyCallsCaptureModeMethodOn(ident, parentBlock) {
			packageCaptureModeBoxIdents[identObj] = true
		}

		return
	}

	escapes := false

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

				// Address of a struct-field chain rooted at the identifier: `&x.field`,
				// `&x.a.b`. Only the CallExpr arm below peeled selector roots — and only for
				// pointer ARGUMENTS — so an assignment/return/composite-position field address
				// left the local unboxed and the emitted `Ꮡ(x).of(T.Ꮡval)` boxed a COPY,
				// silently dropping writes made through the pointer (Go reads the write back
				// through `x`; C# did not).
				if selectorChainRootsAtIdent(n.X, identObj, v.info) {
					escapes = true
					return false
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
			// Check if ident's STORAGE is passed as a pointer argument. Only a literal
			// address-of whose peeled ROOT is the ident (`&i`, `&i.field`, `&i[k]`) — or the
			// bare ident itself — hands the callee a pointer into the ident's storage. An
			// ident that merely appears in a subexpression of a pointer arg computes a VALUE:
			// in `xs[i].link(&xs[i+1])` or `typesEqual(tin[i], vin[i], seen)` the element/
			// slice storage escapes (the peeled root `xs`/`tin`), but the INDEX `i` does not —
			// the old contains-anywhere check heap-boxed every such loop index (a spurious
			// allocation, and duplicate hoisted boxes for sibling loops).
			for i, arg := range n.Args {
				// Skip this arg if it's a nested call expression
				if _, isNestedCall := arg.(*ast.CallExpr); isNestedCall {
					continue
				}

				if argRootIsIdent(arg, identObj, v.info) {
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

	// A value var on which a capture-mode pointer-receiver method is called (e.g.
	// `var i atomic.Int32; i.Store(10)`, or `var frontier orderEventList; frontier.Push(…)`)
	// must be heap-boxed so the call can be routed through the ж overload — the only path that
	// sets up the receiver box the method needs for `&recv.field`.
	if !escapes && v.bodyCallsCaptureModeMethodOn(ident, parentBlock) {
		escapes = true
	}

	v.identEscapesHeap[identObj] = escapes
}

// argRootIsIdent reports whether passing arg to a pointer parameter hands the callee a
// pointer into identObj's own storage: arg is `&expr` whose storage root — peeled through
// parens, field selectors, index expressions (the CONTAINER, never the index), and derefs —
// is the ident, or arg is the bare ident itself (only possible when the ident is already
// pointer-typed). Anything else (the ident inside an index, an operand of arithmetic, a
// nested composite) contributes a value, not the ident's address; a literal `&ident` deeper
// inside such an expression is caught independently by the UnaryExpr arm.
func argRootIsIdent(arg ast.Expr, identObj types.Object, info *types.Info) bool {
	root := arg

	if unary, ok := arg.(*ast.UnaryExpr); ok && unary.Op == token.AND {
		root = unary.X

		for {
			switch expr := root.(type) {
			case *ast.ParenExpr:
				root = expr.X
				continue
			case *ast.SelectorExpr:
				root = expr.X
				continue
			case *ast.IndexExpr:
				root = expr.X
				continue
			case *ast.StarExpr:
				root = expr.X
				continue
			}

			break
		}
	}

	if id, ok := root.(*ast.Ident); ok {
		return info.ObjectOf(id) == identObj
	}

	return false
}

// selectorChainRootsAtIdent reports whether expr is a struct-field selector chain
// (`x.f1.…fn`, n>=1) whose peeled root is the ident under analysis, with every hop a
// direct VALUE field selection. Taking such a chain's address aliases the root local's
// OWN storage, so the local must be heap-boxed — the `Ꮡ(x).of(T.Ꮡval)` copy-box
// fallback otherwise orphans writes made through the pointer. A hop that crosses a
// pointer — an explicit `ptr.field` deref or a field promoted through an embedded
// pointer (both are Selection.Indirect()) — aliases the POINTEE's storage instead, so
// the root must NOT be boxed: the pointer value already routes through `.of(…)` (see
// convUnaryExpr). A missing Selections entry is a package qualifier, and a method
// value cannot stand under `&`, so both stop the walk.
func selectorChainRootsAtIdent(expr ast.Expr, identObj types.Object, info *types.Info) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	for {
		if selection, ok := info.Selections[sel]; !ok || selection.Kind() != types.FieldVal || selection.Indirect() {
			return false
		}

		base := sel.X

		for {
			if paren, ok := base.(*ast.ParenExpr); ok {
				base = paren.X
				continue
			}

			break
		}

		switch base := base.(type) {
		case *ast.SelectorExpr:
			sel = base
		case *ast.Ident:
			return info.ObjectOf(base) == identObj
		default:
			return false
		}
	}
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
