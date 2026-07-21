// visitBranchStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

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
			v.writeOutput("goto %s;", getBreakLabelName(branchStmt.Label.Name))
		}
	case token.CONTINUE:
		if branchStmt.Label == nil {
			// A C# `continue` transfers straight to the post clause, skipping the end-of-body
			// per-iteration copy-backs of a Go 1.22+ transformed loop — emit them here first
			// (see forClausePerIterVars). A labeled continue instead flows through the
			// `continue_<label>:` target, which the copy-backs already follow.
			if len(v.loopCopyBackStack) > 0 {
				for _, copyBack := range v.loopCopyBackStack[len(v.loopCopyBackStack)-1] {
					v.targetFile.WriteString(v.newline)
					v.writeOutput(copyBack)
				}
			}

			v.targetFile.WriteString(v.newline)
			v.writeOutput("continue;")
		} else {
			v.targetFile.WriteString(v.newline)
			v.writeOutput("goto %s;", getContinueLabelName(branchStmt.Label.Name))
		}
	case token.GOTO:
		v.targetFile.WriteString(v.newline)
		v.writeOutput("goto %s;", getSanitizedIdentifier(branchStmt.Label.Name))
	}
}
