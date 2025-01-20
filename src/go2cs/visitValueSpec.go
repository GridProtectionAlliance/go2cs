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
	v.writeDoc(valueSpec.Doc, valueSpec.End())

	if tok == token.VAR {
		for i, ident := range valueSpec.Names {
			// Check if this is an interface type being assigned a value
			if len(valueSpec.Values) > i {
				// Get the type - either from explicit type or from value's type
				var declType types.Type

				if valueSpec.Type != nil {
					declType = v.info.TypeOf(valueSpec.Type)
				} else {
					declType = v.info.TypeOf(ident)
				}

				if declType != nil {
					// Check if it's an interface type
					if iface, ok := declType.Underlying().(*types.Interface); ok && iface.NumMethods() > 0 {
						// Get the concrete type from the RHS
						rhsType := v.info.TypeOf(valueSpec.Values[i])

						if rhsType != nil {
							// Record the implementation
							interfaceName := convertToCSTypeName(getFullTypeName(declType))
							concreteTypeName := convertToCSTypeName(getFullTypeName(rhsType))

							packageLock.Lock()
							if implementations, exists := interfaceImplementations[interfaceName]; exists {
								implementations.Add(concreteTypeName)
							} else {
								interfaceImplementations[interfaceName] = NewHashSet([]string{concreteTypeName})
							}
							packageLock.Unlock()
						}
					}
				}
			}

			goIDName := v.getIdentName(ident)
			csIDName := getSanitizedIdentifier(goIDName)

			if len(valueSpec.Values) <= i {
				def := v.info.Defs[ident]

				if def != nil {
					v.targetFile.WriteString(v.newline)

					goTypeName := getTypeName(def.Type())
					csTypeName := convertToCSTypeName(goTypeName)

					typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(goIDName) + (len(csIDName) - len(goIDName)))

					if v.inFunction {
						heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

						if len(heapTypeDecl) > 0 {
							v.writeOutput(heapTypeDecl)
						} else {
							v.writeOutput("%s %s = default!;", csTypeName, csIDName)
						}
					} else {
						access := getAccess(goIDName)
						typeLenDeviation += token.Pos(len(access) + 6)
						v.writeOutput("%s static %s %s;", access, csTypeName, csIDName)
					}

					v.writeComment(valueSpec.Comment, ident.End()+typeLenDeviation-token.Pos(len(csTypeName)))
				}
				continue
			}

			tv := v.info.Types[valueSpec.Values[i]]

			if tv.Value == nil {
				def := v.info.Defs[ident]

				if def != nil {
					v.targetFile.WriteString(v.newline)

					csTypeName := getCSTypeName(def.Type())
					typeLenDeviation := token.Pos(len(csTypeName) + (len(csIDName) - len(goIDName)))

					if v.inFunction {
						headTypeDecl := v.convertToHeapTypeDecl(ident, true)

						if len(headTypeDecl) > 0 {
							v.writeOutputLn(headTypeDecl)
							v.writeOutput("%s = %s;", csIDName, v.convExpr(valueSpec.Values[i], nil))
						} else {
							v.writeOutput("%s %s = %s;", csTypeName, csIDName, v.convExpr(valueSpec.Values[i], nil))
						}
					} else {
						access := getAccess(goIDName)
						typeLenDeviation -= token.Pos(len(access) + 9)
						v.writeOutput("%s static %s %s = %s;", access, csTypeName, csIDName, v.convExpr(valueSpec.Values[i], nil))
					}

					v.writeComment(valueSpec.Comment, valueSpec.Values[i].End()-typeLenDeviation)
				}
				continue
			}

			v.targetFile.WriteString(v.newline)

			csTypeName := convertToCSTypeName(getTypeName(tv.Type))
			goValue := tv.Value.ExactString()
			csValue := v.convExpr(valueSpec.Values[i], nil)
			typeLenDeviation := token.Pos(len(csTypeName) + len(csValue) + (len(csIDName) - len(goIDName)) + (len(csValue) - len(goValue)))

			if v.inFunction {
				headTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(headTypeDecl) > 0 {
					v.writeOutput(headTypeDecl)
				} else {
					v.writeOutput("%s %s;", csTypeName, csIDName)
				}
			} else {
				access := getAccess(goIDName)
				typeLenDeviation += token.Pos(len(access) + 4)
				v.writeOutput("%s static %s %s = %s;", access, csTypeName, csIDName, csValue)
			}

			v.writeComment(valueSpec.Comment, ident.End()+typeLenDeviation)
		}
	} else if tok == token.CONST {
		for i, ident := range valueSpec.Names {
			goIDName := v.getIdentName(ident)
			csIDName := getSanitizedIdentifier(goIDName)

			c := v.info.ObjectOf(ident).(*types.Const)
			goTypeName := getTypeName(c.Type())
			csTypeName := convertToCSTypeName(goTypeName)
			access := getAccess(goIDName)
			typeLenDeviation := token.Pos(len(csTypeName) + len(access) + (len(csIDName) - len(goIDName)))

			var tokEnd token.Pos
			var srcVal string
			var constVal string

			if c.Val().Kind() == constant.String && len(valueSpec.Values) >= i+1 {
				if lit, ok := valueSpec.Values[i].(*ast.BasicLit); ok && lit.Kind == token.STRING {
					constVal = v.convBasicLit(lit, DefaultBasicLitContext())
				} else {
					constVal, _ = v.getStringLiteral(c.Val().ExactString())
				}
			} else if c.Val().Kind() == constant.Float {
				constVal = c.Val().String()
			} else {
				constVal = c.Val().ExactString()
			}

			if valueSpec.Type == nil && len(valueSpec.Values) >= i+1 {
				tokEnd = valueSpec.Values[i].End()

				if ident := getIdentifier(valueSpec.Values[i]); ident != nil {
					srcVal = ident.Name
				} else if lit, ok := valueSpec.Values[i].(*ast.BasicLit); ok {
					srcVal = lit.Value
				}

				typeLenDeviation += token.Pos(len(constVal) - len(srcVal) - 4)
			} else {
				tokEnd = ident.End()
			}

			constHandled := false

			writeUntypedConst := func() {
				v.targetFile.WriteString(v.newline)

				if v.inFunction {
					v.writeOutput("GoUntyped %s = /* ", csIDName)
				} else {
					v.writeOutput("%s static readonly GoUntyped %s = /* ", access, csIDName)
				}

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
				v.targetFile.WriteString(v.newline)

				if v.inFunction {
					v.writeOutput("@string %s = %s;", csIDName, constVal)
				} else {
					v.writeOutput("%s static readonly @string %s = %s;", access, csIDName, constVal)
				}

				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation-1)
				constHandled = true
			}

			if !constHandled {
				v.targetFile.WriteString(v.newline)

				if srcVal == "iota" {
					constVal = "iota"
				}

				if v.inFunction {
					v.writeOutput("const %s %s = %s;", csTypeName, csIDName, constVal)
				} else {
					v.writeOutput("%s const %s %s = %s;", access, csTypeName, csIDName, constVal)
				}

				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation+1)
			}
		}
	} else {
		println(fmt.Sprintf("Unexpected ValueSpec token type: %s", tok))
	}
}
