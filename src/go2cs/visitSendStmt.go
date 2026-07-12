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

	if elemIsNonEmptyIface {
		if _, isPtr := v.getType(sendStmt.Value, false).(*types.Pointer); isPtr {
			identContext := DefaultIdentContext()
			identContext.isPointer = true
			contexts = append(contexts, identContext)
		}
	}

	sendExpr := v.convExpr(sendStmt.Value, contexts)

	if elemIsNonEmptyIface {
		sendExpr = v.convertToInterfaceType(elemType, v.getExprType(sendStmt.Value), sendExpr)
	}

	// An untyped `int` constant sent into an EMPTY-interface element boxes through nint (the
	// numeric twin of the @string boxing above), so a later `x.(int)` / `case int:` on the received
	// value matches Go's boxed `int` dynamic type rather than a stray System.Int32.
	return v.boxUntypedIntAsNint(elemType, sendStmt.Value, sendExpr)
}
