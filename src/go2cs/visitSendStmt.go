package main

import (
	"fmt"
	"go/ast"
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

	sendExpr := v.convExpr(sendStmt.Value, nil)
	chanType := v.getExprType(sendStmt.Chan)

	if needsInterfaceCast, isEmpty := isInterface(chanType); needsInterfaceCast && !isEmpty {
		sendExpr = convertToInterfaceType(chanType, v.getExprType(sendStmt.Value), sendExpr)
	}

	v.targetFile.WriteString(fmt.Sprintf("%s.%s(%s)", v.convExpr(sendStmt.Chan, nil), channelOperation, sendExpr))

	if format.includeSemiColon {
		v.targetFile.WriteRune(';')
	}
}
