// visitSendStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

func (v *Visitor) visitSendStmt(sendStmt *ast.SendStmt, format FormattingContext) {
	if format.useNewLine {
		v.targetFile.WriteString(v.newline)
	}

	if format.useIndent {
		v.targetFile.WriteString(v.indent(v.indentLevel))
	}

	var channelOperation string

	if v.options.useChannelOperators {
		channelOperation = ChannelLeftOp
	} else {
		channelOperation = "Send"
	}

	v.targetFile.WriteString(fmt.Sprintf("%s.%s(%s)", v.convExpr(sendStmt.Chan, nil), channelOperation, v.convSendValueExpr(sendStmt)))

	if format.includeSemiColon {
		v.targetFile.WriteRune(';')
	}
}

// convSendValueExpr converts the VALUE expression of a channel send (`ch <- v`) against the
// channel's ELEMENT type — both the statement form (visitSendStmt) and the select-case
// registration form (visitSelectStmt) route here. A string literal sent into an EMPTY-interface
// element boxes through @string (`(@string)"…"`): the default `"…"u8` ReadOnlySpan<byte> has no
// conversion to the channel's `in object` send parameter (CS1503), and the box preserves Go
// string identity for a later type assertion — the same rule assignment, return, and
// composite-literal positions apply. A NON-empty interface element wraps the value in its
// generated interface adapter (convertToInterfaceType, with a pointer value rendering as its
// box — parity with the argument-position rules in convExprList); the check this replaces
// tested the CHANNEL type itself, which is never an interface, so it never fired. A type-
// parameter element (`chan T` in generic code) keeps the bare emission.
func (v *Visitor) convSendValueExpr(sendStmt *ast.SendStmt) string {
	var elemType types.Type

	if chanExprType := v.getExprType(sendStmt.Chan); chanExprType != nil {
		if chanType, ok := chanExprType.Underlying().(*types.Chan); ok {
			elemType = chanType.Elem()
		}
	}

	elemIsNonEmptyIface := false

	if elemType != nil {
		if _, isTypeParam := types.Unalias(elemType).(*types.TypeParam); !isTypeParam {
			elemIsIface, elemIfaceEmpty := isInterface(elemType)
			elemIsNonEmptyIface = elemIsIface && !elemIfaceEmpty
		}
	}

	var contexts []ExprContext

	if isEmptyInterfaceTarget(elemType) && isStringBasicLit(sendStmt.Value) {
		basicLitContext := DefaultBasicLitContext()
		basicLitContext.u8StringOK = false
		basicLitContext.castToGoString = true
		contexts = append(contexts, basicLitContext)
	}

	// A POINTER element type (`chan *Call` is `channel<ж<Call>>`) takes the box, exactly as the
	// interface case does — the send parameter is `in ж<Call>`. Without the pointer context a
	// DEREF-ALIASED pointer (a pointer PARAMETER, or the method's own receiver — both emitted as
	// the pointed-to value with the box held separately) renders as that value and cannot bind the
	// box parameter: net/rpc's `func (call *Call) done()` sending `call.Done <- call` was CS1503.
	// A pointer LOCAL already holds its box and convIdent's pointer arm leaves it as-is, so this
	// widens the emission only where the value had no other way to name its pointer.
	elemIsPointer := false

	if elemType != nil {
		_, elemIsPointer = elemType.Underlying().(*types.Pointer)
	}

	if elemIsNonEmptyIface || elemIsPointer {
		if _, isPtr := v.getType(sendStmt.Value, false).(*types.Pointer); isPtr {
			identContext := DefaultIdentContext()
			identContext.isPointer = true
			contexts = append(contexts, identContext)
		}
	}

	sendExpr := v.convExpr(sendStmt.Value, contexts)

	// A Go array SENT into a channel is copied at the send (`ch <- arr` then `arr[0] = 9` must
	// not affect the buffered element); the emitted struct copy would alias its backing, so an
	// existing-storage array value clones here. The RECEIVE side needs no twin: a buffered
	// element is dequeued exactly once, so the received struct copy is already unaliased.
	// Applied before the interface wrap below so an interface-element channel boxes the clone.
	if v.exprReadsArrayValueFromStorage(sendStmt.Value) {
		sendExpr = appendArrayValueClone(sendExpr)
	}

	if elemIsNonEmptyIface {
		sendExpr = v.convertToInterfaceType(elemType, v.getExprType(sendStmt.Value), sendExpr)
	}

	// An untyped `int` constant sent into an EMPTY-interface element boxes through nint (the
	// numeric twin of the @string boxing above), so a later `x.(int)` / `case int:` on the received
	// value matches Go's boxed `int` dynamic type rather than a stray System.Int32.
	return v.boxUntypedIntAsNint(elemType, sendStmt.Value, sendExpr)
}
