package main

import (
	"fmt"
	"go/ast"
	"go/printer"
	"go/token"
	"go/types"
	"strings"
)

func (v *Visitor) enterValueSpec(x *ast.ValueSpec, tok token.Token) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(x.Doc, x.End())

	if x.Type != nil {
		v.visitType(x.Type, x.Names[0].Name, x.Comment)
	}

	if tok == token.VAR {
		for i, name := range x.Names {
			if len(x.Values) <= i {
				def := v.info.Defs[name]

				if def != nil {
					goTypeName := getTypeName(def.Type())
					csTypeName := convertToCSTypeName(goTypeName)
					access := getAccess(name.Name)
					typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(access) + len(name.Name) - 3)

					v.writeOutput(fmt.Sprintf("%s static %s %s;", access, csTypeName, name.Name))

					v.writeComment(x.Comment, name.End()+typeLenDeviation-token.Pos(len(csTypeName)))
					v.targetFile.WriteString(v.newline)
				}
				continue
			}

			tv := v.info.Types[x.Values[i]]

			if tv.Value == nil {
				def := v.info.Defs[name]

				if def != nil {
					csTypeName := getCSTypeName(def.Type())
					access := getAccess(name.Name)
					typeLenDeviation := token.Pos(len(csTypeName) - len(access) - 1)

					// HACK: Will likely be necessary to walk expression tree here instead of using printer package
					varDecl := &strings.Builder{}
					varDecl.WriteString(fmt.Sprintf("%s static %s %s = ", access, csTypeName, name.Name))
					printer.Fprint(varDecl, v.fset, x.Values[i])
					varDecl.WriteString(";")
					v.writeOutput(varDecl.String())

					v.writeComment(x.Comment, x.Values[i].End()-typeLenDeviation)
					v.targetFile.WriteString(v.newline)
				}
				continue
			}

			goTypeName := getTypeName(tv.Type)
			csTypeName := convertToCSTypeName(goTypeName)
			access := getAccess(name.Name)
			typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(access))

			v.writeOutput("%s static %s %s = %s;", access, csTypeName, name.Name, tv.Value.ExactString())

			v.writeComment(x.Comment, name.End()+typeLenDeviation)
			v.targetFile.WriteString(v.newline)
		}
	} else if tok == token.CONST {
		for _, name := range x.Names {
			c := v.info.ObjectOf(name).(*types.Const)
			goTypeName := getTypeName(c.Type())
			csTypeName := convertToCSTypeName(goTypeName)
			access := getAccess(name.Name)
			typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(access) + 7)

			v.writeOutput("%s const %s %s = %s;", access, csTypeName, name.Name, c.Val().ExactString())
			v.writeComment(x.Comment, name.End()+typeLenDeviation)
			v.targetFile.WriteString(v.newline)
		}
	} else {
		println(fmt.Sprintf("Unexpected ValueSpec token type: %s", tok))
	}
}

func (v *Visitor) exitValueSpec(x *ast.ValueSpec) {
}
