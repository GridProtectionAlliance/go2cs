package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
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
				sstringEligible:  fileEntry.sstringEligible,
				sstringConvExprs: fileEntry.sstringConvExprs,
			}

			// Unnamed `string(x)` temporaries consumed within a comparison against a literal
			// (`string(buf) == "…"`) never outlive the expression, so they are safe to emit as a
			// zero-copy sstring view unconditionally — no escape/mutation analysis required.
			visitor.markSStringComparisonConversions(fileEntry.file)

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

								// A single-value `s := string(x)` may be emittable as a stack-only
								// sstring; decide now that identEscapesHeap is populated for the LHS.
								if len(n.Lhs) == 1 && len(n.Rhs) == 1 {
									if ident := getIdentifier(n.Lhs[0]); ident != nil {
										visitor.markSStringEligible(ident, n.Rhs[0], node.Body)
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

// markSStringEligible records whether the string local bound by `ident := string(x)` may be emitted
// as a stack-only sstring — a zero-copy view over x's bytes — instead of the heap @string. The
// predicate is deliberately CONSERVATIVE (the MVP's safest idiom): it fires only for a plain-string
// local that does not escape, is never returned, is used only through safe reads (len/cap, byte
// index, or comparison against a string literal), and whose conversion source is never written for
// the lifetime of the view. Any uncertainty leaves the local as @string.
//
// sstring is a ref struct, so most missed escapes (storing into a field/slice/map, boxing to an
// interface, sending on a channel, capturing in a closure) become COMPILE errors rather than silent
// bugs. The two vectors that would be silently wrong — the local escaping via `return`, and mutation
// of the source buffer while the view is alive — are guarded explicitly below.
func (v *Visitor) markSStringEligible(ident *ast.Ident, rhs ast.Expr, body *ast.BlockStmt) {
	obj := v.info.ObjectOf(ident)

	if obj == nil {
		return
	}

	// Must be the built-in `string` type exactly — a named string type (`type S string`) would lose
	// its identity if emitted as sstring.
	if !types.Identical(obj.Type(), types.Typ[types.String]) {
		return
	}

	// Must not escape by any channel the escape analysis already detects.
	if v.identEscapesHeap[obj] {
		return
	}

	// Initializer must be a `string(x)` conversion whose source x is an unnamed []byte (a []rune must
	// UTF-8-encode — an allocation, no view; a named slice needs a two-hop cast C# will not chain).
	call := v.unnamedByteSliceStringConv(rhs)

	if call == nil {
		return
	}

	// The source's storage root must be an identifiable local/param so the mutation scan can track it.
	srcRoot := rootIdentObject(call.Args[0], v.info)

	if srcRoot == nil {
		return
	}

	// Every use of the local must be a safe read, and it must not be returned.
	if !v.sstringUsesAreSafe(obj, ident, body) {
		return
	}

	// The source must not be written anywhere in the function (strongest form of the guard for now).
	if v.objectIsWritten(srcRoot, body) {
		return
	}

	v.sstringEligible[obj] = true

	if os.Getenv("GO2CS_DEBUG_SSTRING") != "" {
		pos := v.fset.Position(ident.Pos())
		fmt.Fprintf(os.Stderr, "[sstring] eligible: %s at %s:%d:%d\n", ident.Name, pos.Filename, pos.Line, pos.Column)
	}
}

// sstringUsesAreSafe reports whether every use of the sstring-candidate local `obj` (other than its
// declaring occurrence `declIdent`) is a safe read: an argument to len/cap, the base of a byte index
// `s[i]`, or an operand of a comparison against a string literal (`s == "x"`). Any other use — passed
// to a function, stored, ranged, concatenated, converted, returned, reassigned — makes it ineligible.
func (v *Visitor) sstringUsesAreSafe(obj types.Object, declIdent *ast.Ident, body *ast.BlockStmt) bool {
	// Pass 1: collect the identifier nodes that sit in a safe-read slot.
	safeIdents := map[*ast.Ident]bool{}

	ast.Inspect(body, func(n ast.Node) bool {
		switch e := n.(type) {
		case *ast.CallExpr:
			if fn, ok := e.Fun.(*ast.Ident); ok && (fn.Name == "len" || fn.Name == "cap") {
				for _, arg := range e.Args {
					if id, ok := arg.(*ast.Ident); ok {
						safeIdents[id] = true
					}
				}
			}
		case *ast.IndexExpr:
			// `s[i]`: the indexed BASE is a safe byte read (an ident used as the index is separate).
			if id, ok := e.X.(*ast.Ident); ok {
				safeIdents[id] = true
			}
		case *ast.BinaryExpr:
			if isComparisonOp(e.Op) {
				// A comparison against a string literal (`s == "x"`) or against a plain-`string`
				// operand (variable/field, `s == want`): the stack string has zero-copy comparison
				// operators against a `u8` literal, another sstring, and — via the mixed
				// sstring/@string operators — a heap @string, so any such comparison compiles and
				// the local never escapes. (Because a safe local's source is proven never-written
				// for the whole function, evaluating the other operand cannot mutate the view.)
				if id, ok := e.X.(*ast.Ident); ok && v.isPlainStringOperand(e.Y) {
					safeIdents[id] = true
				}
				if id, ok := e.Y.(*ast.Ident); ok && v.isPlainStringOperand(e.X) {
					safeIdents[id] = true
				}
			}
		}

		return true
	})

	// Pass 2: every occurrence of the local must be the declaration or one of those safe slots.
	allSafe := true

	ast.Inspect(body, func(n ast.Node) bool {
		if !allSafe {
			return false
		}

		if id, ok := n.(*ast.Ident); ok && id != declIdent && v.info.ObjectOf(id) == obj {
			if !safeIdents[id] {
				allSafe = false
			}
		}

		return true
	})

	return allSafe
}

// objectIsWritten reports whether `root`'s storage is (potentially) written anywhere in the body:
// as an assignment / increment target, through an address-of, or by being passed to a call that is
// not a conversion or len/cap (a slice shares its backing array, so any such callee could mutate it).
// This is the conservative "no write to the source for the whole function" form of the mutation guard.
func (v *Visitor) objectIsWritten(root types.Object, body *ast.BlockStmt) bool {
	written := false

	ast.Inspect(body, func(n ast.Node) bool {
		if written {
			return false
		}

		switch node := n.(type) {
		case *ast.AssignStmt:
			for _, lhs := range node.Lhs {
				// Skip root's OWN declaration (`root := …`): that establishes the initial value
				// before the view exists, it is not a mutation of it. Any later reassignment
				// (`root = append(root, …)`, `root = …`) is a distinct occurrence and IS flagged.
				if id := rootIdent(lhs); id != nil && v.info.ObjectOf(id) == root && id.Pos() != root.Pos() {
					written = true
				}
			}
		case *ast.IncDecStmt:
			if rootIdentObject(node.X, v.info) == root {
				written = true
			}
		case *ast.UnaryExpr:
			if node.Op == token.AND && rootIdentObject(node.X, v.info) == root {
				written = true
			}
		case *ast.CallExpr:
			// A conversion (`string(root)`, `[]byte(root)`) reads its operand; len/cap read too.
			if tv, ok := v.info.Types[node.Fun]; ok && tv.IsType() {
				return true
			}
			if fn, ok := node.Fun.(*ast.Ident); ok && (fn.Name == "len" || fn.Name == "cap") {
				return true
			}
			// Any other call receiving root (or a sub-slice of it) may mutate the shared backing.
			for _, arg := range node.Args {
				if rootIdentObject(arg, v.info) == root {
					written = true
				}
			}
		}

		return true
	})

	return written
}

// rootIdent peels an expression through parens, indexes, slice expressions and derefs to its root
// identifier, or nil if the root is not a plain identifier (a call result, a composite literal, ...).
func rootIdent(expr ast.Expr) *ast.Ident {
	for {
		switch e := expr.(type) {
		case *ast.ParenExpr:
			expr = e.X
		case *ast.IndexExpr:
			expr = e.X
		case *ast.SliceExpr:
			expr = e.X
		case *ast.StarExpr:
			expr = e.X
		case *ast.Ident:
			return e
		default:
			return nil
		}
	}
}

// rootIdentObject peels an expression to the object of its root identifier, or nil if the root is
// not a plain identifier, in which case the storage cannot be tracked.
func rootIdentObject(expr ast.Expr, info *types.Info) types.Object {
	if id := rootIdent(expr); id != nil {
		return info.ObjectOf(id)
	}

	return nil
}

// markSStringComparisonConversions flags every `string(x)` conversion CallExpr that is an operand of
// a comparison (`string(buf) == "…"` / `string(buf) == want`, any of ==/!=/</<=/>/>=). Such a
// temporary is created and consumed within the single comparison expression, so it cannot escape;
// emitting it as a zero-copy sstring view is safe with NO escape analysis, provided the OTHER operand
// cannot mutate the source buffer before the view is read (see sstringOtherOperandSafe — a literal, a
// pure-read plain-`string` expression, or another `string(bytes)` conversion, none of which run code
// that could write x). Restricted to an unnamed []byte source, like the local case.
func (v *Visitor) markSStringComparisonConversions(file *ast.File) {
	ast.Inspect(file, func(n ast.Node) bool {
		binaryExpr, ok := n.(*ast.BinaryExpr)

		if !ok || !isComparisonOp(binaryExpr.Op) {
			return true
		}

		if call := v.unnamedByteSliceStringConv(binaryExpr.X); call != nil && v.sstringOtherOperandSafe(binaryExpr.Y) {
			v.sstringConvExprs[call] = true
		}

		if call := v.unnamedByteSliceStringConv(binaryExpr.Y); call != nil && v.sstringOtherOperandSafe(binaryExpr.X) {
			v.sstringConvExprs[call] = true
		}

		return true
	})
}

// sstringOtherOperandSafe reports whether `expr` — the operand ON THE OTHER SIDE of a comparison whose
// first operand is an unnamed `string(bytes)` conversion — is one that (a) the emitted stack-string
// comparison operators can handle and (b) cannot mutate the converted source before the comparison
// reads the zero-copy view. Three safe shapes: another `string(bytes)` conversion (also a view; the
// two compare as sstring == sstring); a string literal (`"…"u8`); or a PURE-READ plain-`string`
// expression — a variable, field, or index read, which executes no function call and so cannot write
// the buffer. A named string type is excluded (no operator against sstring). Anything with a call is
// rejected: `string(x) == f()` could see f mutate x between the (lazy) view and the compare.
func (v *Visitor) sstringOtherOperandSafe(expr ast.Expr) bool {
	if v.unnamedByteSliceStringConv(expr) != nil {
		return true
	}

	if isStringLiteralExpr(expr) {
		return true
	}

	if !isPureReadExpr(expr) {
		return false
	}

	t := v.info.TypeOf(expr)

	return t != nil && types.Identical(t, types.Typ[types.String])
}

// isPlainStringOperand reports whether `expr` is a string literal or an expression whose type is the
// built-in `string` exactly (not a named string type) — the operands an sstring can be compared
// against via its literal (u8), sstring, or mixed-@string comparison operators. Used for the
// named-local case, whose source is already proven never-written, so mutation ordering is moot and no
// purity check is needed.
func (v *Visitor) isPlainStringOperand(expr ast.Expr) bool {
	if isStringLiteralExpr(expr) {
		return true
	}

	t := v.info.TypeOf(expr)

	return t != nil && types.Identical(t, types.Typ[types.String])
}

// isPureReadExpr reports whether evaluating `expr` runs no function/method call, channel receive, or
// other side-effecting operation — so it cannot mutate anything, in particular the source buffer of a
// sibling `string(bytes)` view being compared against it. Conservative: only literals, identifiers,
// and reads composed of them (selector/index/slice/paren) qualify; a call or any other node fails.
func isPureReadExpr(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.BasicLit, *ast.Ident:
		return true
	case *ast.ParenExpr:
		return isPureReadExpr(e.X)
	case *ast.SelectorExpr:
		return isPureReadExpr(e.X)
	case *ast.IndexExpr:
		return isPureReadExpr(e.X) && isPureReadExpr(e.Index)
	case *ast.SliceExpr:
		return isPureReadExpr(e.X) &&
			(e.Low == nil || isPureReadExpr(e.Low)) &&
			(e.High == nil || isPureReadExpr(e.High)) &&
			(e.Max == nil || isPureReadExpr(e.Max))
	}

	return false
}

// unnamedByteSliceStringConv returns the CallExpr if expr is a `string(x)` conversion whose source x
// is an UNNAMED []byte — the form that can become a zero-copy sstring view — else nil. A []rune source
// must UTF-8-encode (an allocation), and a named []byte would need a two-hop cast C# will not chain.
func (v *Visitor) unnamedByteSliceStringConv(expr ast.Expr) *ast.CallExpr {
	call, ok := expr.(*ast.CallExpr)

	if !ok || len(call.Args) != 1 {
		return nil
	}

	if tv, ok := v.info.Types[call.Fun]; !ok || !tv.IsType() || !types.Identical(tv.Type, types.Typ[types.String]) {
		return nil
	}

	srcType := v.info.TypeOf(call.Args[0])

	if srcType == nil {
		return nil
	}

	if _, isNamed := types.Unalias(srcType).(*types.Named); isNamed {
		return nil
	}

	if slice, ok := srcType.Underlying().(*types.Slice); !ok {
		return nil
	} else if basic, ok := slice.Elem().Underlying().(*types.Basic); !ok || basic.Kind() != types.Uint8 {
		return nil
	}

	return call
}

func isStringLiteralExpr(expr ast.Expr) bool {
	lit, ok := expr.(*ast.BasicLit)

	return ok && lit.Kind == token.STRING
}

func isComparisonOp(op token.Token) bool {
	switch op {
	case token.EQL, token.NEQ, token.LSS, token.LEQ, token.GTR, token.GEQ:
		return true
	}

	return false
}
