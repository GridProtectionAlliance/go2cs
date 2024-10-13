package main

import (
	"go/ast"
	"go/token"
)

func (v *Visitor) visitBranchStmt(branchStmt *ast.BranchStmt) {
	// FALLTHROUGH is handled in visitSwitchStmt.go
	switch branchStmt.Tok {
	case token.BREAK:
		v.targetFile.WriteString(v.newline)
		if branchStmt.Label == nil {
			v.writeOutput("break;")
		} else {
			v.writeOutput("goto " + getSanitizedIdentifier(branchStmt.Label.Name) + "_break;")
		}
	case token.CONTINUE:
		v.targetFile.WriteString(v.newline)
		if branchStmt.Label == nil {
			v.writeOutput("continue;")
		} else {
			v.writeOutput("goto " + getSanitizedIdentifier(branchStmt.Label.Name) + "_continue;")
		}
	case token.GOTO:
		v.targetFile.WriteString(v.newline)
		v.writeOutput("goto " + getSanitizedIdentifier(branchStmt.Label.Name) + ";")
	}
}
