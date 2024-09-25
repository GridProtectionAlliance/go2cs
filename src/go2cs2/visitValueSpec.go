package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
)

func (v *Visitor) visitValueSpec(valueSpec *ast.ValueSpec, tok token.Token) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(valueSpec.Doc, valueSpec.End())

	if tok == token.VAR {
		for i, name := range valueSpec.Names {
			if len(valueSpec.Values) <= i {
				def := v.info.Defs[name]

				if def != nil {
					goTypeName := getTypeName(def.Type())
					csTypeName := convertToCSTypeName(goTypeName)
					access := getAccess(name.Name)
					typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(access) + len(name.Name) - 3)

					v.writeOutput(fmt.Sprintf("%s static %s %s;", access, csTypeName, name.Name))

					v.writeComment(valueSpec.Comment, name.End()+typeLenDeviation-token.Pos(len(csTypeName)))
					v.targetFile.WriteString(v.newline)
				}
				continue
			}

			tv := v.info.Types[valueSpec.Values[i]]

			if tv.Value == nil {
				def := v.info.Defs[name]

				if def != nil {
					csTypeName := getCSTypeName(def.Type())
					access := getAccess(name.Name)
					typeLenDeviation := token.Pos(len(csTypeName) - len(access))

					v.writeOutput(fmt.Sprintf("%s static %s %s = %s;", access, csTypeName, name.Name, v.convExpr(valueSpec.Values[i])))
					v.writeComment(valueSpec.Comment, valueSpec.Values[i].End()-typeLenDeviation)
					v.targetFile.WriteString(v.newline)
				}
				continue
			}

			goTypeName := getTypeName(tv.Type)
			csTypeName := convertToCSTypeName(goTypeName)
			access := getAccess(name.Name)
			typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(access) - 1)

			v.writeOutput("%s static %s %s = %s;", access, csTypeName, name.Name, tv.Value.ExactString())

			v.writeComment(valueSpec.Comment, name.End()+typeLenDeviation)
			v.targetFile.WriteString(v.newline)
		}
	} else if tok == token.CONST {
		for i, name := range valueSpec.Names {
			c := v.info.ObjectOf(name).(*types.Const)
			goTypeName := getTypeName(c.Type())
			csTypeName := convertToCSTypeName(goTypeName)
			access := getAccess(name.Name)
			typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(access) + len(csTypeName))
			var tokEnd token.Pos
			var srcVal string

			var constVal string

			if c.Val().Kind() == constant.Float {
				constVal = c.Val().String()
			} else {
				constVal = c.Val().ExactString()
			}

			if len(valueSpec.Values) >= i+1 {
				tokEnd = valueSpec.Values[i].End()

				if ident, ok := valueSpec.Values[i].(*ast.Ident); ok {
					srcVal = ident.Name
				} else if lit, ok := valueSpec.Values[i].(*ast.BasicLit); ok {
					srcVal = v.convBasicLit(lit)
				}

				typeLenDeviation += token.Pos(len(constVal) - len(srcVal) - 5)
			} else {
				tokEnd = name.End()
			}

			constHandled := false

			writeUntypedConst := func() {
				v.writeOutput("%s static readonly GoUntyped %s = /* ", access, name.Name)

				if len(valueSpec.Values) >= i+1 {
					v.targetFile.WriteString(v.getPrintedNode(valueSpec.Values[i]))
				}

				v.targetFile.WriteString(" */")
				v.writeComment(valueSpec.Comment, tokEnd+token.Pos(len(access)-5))
				v.targetFile.WriteString(v.newline)

				v.writeOutput("%sGoUntyped.Parse(\"%s\");", v.indent(v.indentLevel+1), constVal)
				constHandled = true
			}

			if c.Val().Kind() == constant.Int {
				// Check if const integer value will exceed int64 limits
				if _, err := strconv.ParseInt(constVal, 0, 64); err != nil {
					writeUntypedConst()
				}
			}

			if c.Val().Kind() == constant.Float {
				// Check if const float value will exceed float64 limits
				if _, err := strconv.ParseFloat(constVal, 64); err != nil {
					constVal = c.Val().ExactString()
					writeUntypedConst()
				}
			}

			if c.Val().Kind() == constant.String {
				v.writeOutput("%s static readonly @string %s = %s;", access, name.Name, constVal)
				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation-1)
				constHandled = true
			}

			if !constHandled {
				if srcVal == "iota" {
					constVal = "iota"
					typeLenDeviation += 1
				}

				v.writeOutput("%s const %s %s = %s;", access, csTypeName, name.Name, constVal)
				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation+2)
			}

			v.targetFile.WriteString(v.newline)
		}
	} else {
		println(fmt.Sprintf("Unexpected ValueSpec token type: %s", tok))
	}
}
