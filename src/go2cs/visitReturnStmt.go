package main

import (
	"go/ast"
	"strings"
)

const DeferredDeclsMarker = ">>MARKER:DEFERRED_DECLS<<"

func (v *Visitor) visitReturnStmt(returnStmt *ast.ReturnStmt) {
	recvIndex := -1
	var capturedRecvName string

	// Check if receiver is directly returned
	if ptrRecv, recvName := v.isPointerReceiver(); ptrRecv {
		for i, result := range returnStmt.Results {
			if ident, ok := result.(*ast.Ident); ok {
				if ident.Name == recvName {
					recvIndex = i

					// Direct-ж: the receiver box is the parameter `Ꮡrecv`, so return it
					// directly. A method that returns its receiver is marked direct-box by the
					// pre-pass (bodyReturnsReceiver → packageDirectBoxReceiverMethods), so it is
					// emitted with the `ж<T> Ꮡrecv` receiver. This replaces the old static
					// ThreadLocal capture, which raced across threads for distinct receivers.
					capturedRecvName = AddressPrefix + recvName

					break
				}
			}
		}
	}

	result := strings.Builder{}
	deferredDecls := strings.Builder{}

	// In namedReturnDeferMode a `return e1, e2` assigns the named result params and then returns
	// (via the void func() wrapper); the wrapper's caller side emits the actual `return <named>`
	// after the defers run. So here we emit `(named1, named2) = (e1, e2); return;` for an explicit
	// return, or a bare `return;` for a naked return (the result params are already set).
	namedDefer := v.namedReturnDeferMode

	// An explicit `return n1, n2` that returns exactly the named result identifiers (in order) is
	// equivalent to a naked return — emitting the assignment would be a `(n1, n2) = (n1, n2)`
	// self-assignment. Skip it. Only collapse when *every* result is a bare named-result ident:
	// `return foo(), msg` must keep the assignment because foo() may have side effects.
	explicitIsNamed := namedDefer && returnStmt.Results != nil && len(returnStmt.Results) == len(v.namedReturnNames)

	if explicitIsNamed {
		for i, expr := range returnStmt.Results {
			ident, ok := expr.(*ast.Ident)

			if !ok || getSanitizedIdentifier(v.getIdentName(ident)) != v.namedReturnNames[i] {
				explicitIsNamed = false
				break
			}
		}
	}

	// namedDeferAssign is true when we must emit the `(named) = (results); return;` form (an
	// explicit return whose results are not simply the named results themselves).
	namedDeferAssign := namedDefer && returnStmt.Results != nil && !explicitIsNamed

	// A namedReturnDefer return that is the last statement of the function body is terminal: the
	// void func() wrapper simply falls off its end, so the trailing `return;` is redundant. A
	// terminal naked/collapsed return contributes no statement at all; a terminal assignment
	// return emits just the assignment (no `return;`).
	isTerminalReturn := false

	if namedDefer && v.currentFuncDecl != nil && v.currentFuncDecl.Body != nil {
		list := v.currentFuncDecl.Body.List
		isTerminalReturn = len(list) > 0 && list[len(list)-1] == returnStmt
	}

	if isTerminalReturn && !namedDeferAssign {
		return
	}

	result.WriteString(DeferredDeclsMarker)
	result.WriteString(v.indent(v.indentLevel))

	if namedDeferAssign {
		if len(v.namedReturnNames) > 1 {
			result.WriteString("(" + strings.Join(v.namedReturnNames, ", ") + ") = ")
		} else if len(v.namedReturnNames) == 1 {
			result.WriteString(v.namedReturnNames[0] + " = ")
		}
	} else {
		result.WriteString("return")
	}

	signature := v.currentFuncSignature

	if returnStmt.Results == nil || explicitIsNamed {
		// In namedReturnDeferMode a naked return — or an explicit return of exactly the named
		// results — is just `return;` (the result params are already set). Otherwise:
		// In namedReturnDeferMode a naked return is just `return;` — the result params are
		// already assigned and are returned (post-defer) by the wrapper's caller side.
		if signature != nil && !namedDefer {
			// Check if result signature has named return values
			results := &strings.Builder{}

			for i := range signature.Results().Len() {
				if i > 0 && results.Len() > 0 {
					results.WriteString(", ")
				}

				if recvIndex != -1 && i == recvIndex {
					result.WriteString(capturedRecvName)
				} else {
					param := signature.Results().At(i)

					if param.Name() != "" {
						ident := v.getVarIdent(param)

						if ident != nil {
							results.WriteString(getSanitizedIdentifier(v.getIdentName(ident)))
						} else {
							results.WriteString(getSanitizedIdentifier(param.Name()))
						}
					}
				}
			}

			if results.Len() > 0 {
				result.WriteRune(' ')

				if signature.Results().Len() > 1 {
					result.WriteRune('(')
					result.WriteString(results.String())
					result.WriteRune(')')
				} else {
					result.WriteString(results.String())
				}
			}
		}
	} else {
		// In namedReturnDeferMode the LHS already ends with "= "; otherwise separate "return"
		// from its operand with a space.
		if !namedDefer {
			result.WriteRune(' ')
		}

		basicLitContext := DefaultBasicLitContext()

		if len(returnStmt.Results) > 1 {
			result.WriteRune('(')

			// u8 readonly spans are not supported in value tuple
			basicLitContext.u8StringOK = false
		}

		lambdaContext := DefaultLambdaContext()

		resultParams := signature.Results()
		resultParamIsInterface := paramsAreInterfaces(resultParams, true)
		resultParamIsPointer := paramsArePointers(resultParams)

		for i, expr := range returnStmt.Results {
			if i > 0 {
				result.WriteString(", ")
			}

			var replacementVal string

			if resultParams != nil && i < resultParams.Len() {
				argType := v.getType(expr, false)
				targetType := resultParams.At(i).Type()
				replacementVal = v.checkForDynamicStructs(argType, targetType)
			}

			lambdaContext.deferredDecls = &strings.Builder{}

			resultExpr := v.convExpr(expr, []ExprContext{basicLitContext, lambdaContext})

			if len(replacementVal) > 0 {
				resultExpr = strings.ReplaceAll(replacementVal, DynamicCastArgMarker, resultExpr)
			}

			if lambdaContext.deferredDecls.Len() > 0 {
				deferredDecls.WriteString(lambdaContext.deferredDecls.String())
			}

			if resultParamIsInterface != nil && i < len(resultParamIsInterface) && resultParamIsInterface[i] {
				resultParamType := resultParams.At(i).Type()
				result.WriteString(v.convertToInterfaceType(resultParamType, v.getType(expr, false), resultExpr))
			} else {
				if resultParamIsPointer != nil && i < len(resultParamIsPointer) && resultParamIsPointer[i] {
					// A deref-aliased pointer parameter returned WHOLE (`return p`) must yield its box
					// `Ꮡp` (the value alias cannot bind the pointer result). Use a DIRECT ident check,
					// not getIdentifier (which digs through a selector/index to the root): `return it.key`
					// — a field of a pointer param — returns the FIELD value, so prefixing `Ꮡ` would
					// wrongly emit `Ꮡit.key` (the box `Ꮡit` has no `key` → CS1061).
					if ident, isIdent := expr.(*ast.Ident); isIdent && v.identIsParameter(ident) {
						result.WriteString(AddressPrefix)
						resultExpr = strings.TrimPrefix(resultExpr, "@")
					}
				}

				var narrowCast string
				if resultParams != nil && i < resultParams.Len() {
					narrowCast = v.narrowArithmeticCastTypeFor(resultParams.At(i).Type(), expr, resultExpr)
				}

				if recvIndex != -1 && i == recvIndex {
					result.WriteString(capturedRecvName)
				} else if len(narrowCast) > 0 {
					// A narrow-integer (byte/int8/uint8/int16/uint16) arithmetic RESULT is promoted to
					// `int` by C#, so returning it from a narrow-typed function needs the cast back to
					// compile and to preserve Go's wrap (CS0266) — `func lowerASCII(c byte) byte { return
					// c + ('a'-'A') }`. The assignment / value-spec paths already narrow such a result;
					// the return path had omitted it. narrowArithmeticCastTypeFor gates on a binary/unary
					// arith RHS whose Go type matches the (narrow) result type, and skips a whole-expr
					// already-cast RHS — so a bare ident / call / already-narrowed return is untouched.
					result.WriteString("(" + narrowCast + ")(" + resultExpr + ")")
				} else {
					result.WriteString(resultExpr)
				}
			}
		}

		if len(returnStmt.Results) > 1 {
			result.WriteRune(')')
		}
	}

	result.WriteRune(';')

	// In namedReturnDeferMode a non-terminal explicit return assigned the named result params
	// above; follow it with a bare `return;` to exit the void func() wrapper. A terminal return
	// needs none (the wrapper falls off its end), and a naked/collapsed return already produced
	// just `return;`.
	if namedDeferAssign && !isTerminalReturn {
		result.WriteString(" return;")
	}

	if deferredDecls.Len() == 0 {
		deferredDecls.WriteString(v.newline)
	}

	v.targetFile.WriteString(strings.ReplaceAll(result.String(), DeferredDeclsMarker, deferredDecls.String()))
}
