package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"strconv"
	"strings"
)

func (v *Visitor) visitValueSpec(valueSpec *ast.ValueSpec, doc *ast.CommentGroup, tok token.Token) {
	v.targetFile.WriteString(v.newline)
	v.writeDoc(doc, valueSpec.End())

	if tok == token.VAR {
		for i, ident := range valueSpec.Names {
			var isAnyType bool
			var isInterfaceType bool

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
					if isInterface, isEmpty := isInterface(declType); isInterface {
						isInterfaceType = true

						if isEmpty {
							isAnyType = true
						} else {
							// Get the concrete type from the RHS
							rhsType := v.info.TypeOf(valueSpec.Values[i])

							// Record the implementation
							if rhsType != nil {
								v.convertToInterfaceType(declType, rhsType, "")
							}
						}
					}
				}
			}

			goIDName := v.getIdentName(ident)
			csIDName := getSanitizedIdentifier(goIDName)

			context := DefaultBasicLitContext()
			context.u8StringOK = !isInterfaceType

			if len(valueSpec.Values) <= i {
				def := v.info.Defs[ident]

				if def != nil {
					if i > 0 {
						v.targetFile.WriteString(v.newline)
					}

					// Check if value spec type is a struct or a pointer to a struct
					valueSpecType := valueSpec.Type

					if subStructType, exprType := v.extractStructType(valueSpecType); subStructType != nil && !v.liftedTypeExists(subStructType) {
						v.visitStructType(subStructType, exprType, csIDName, valueSpec.Comment, true, nil)
					}

					// Check if value spec type is an interface or a pointer to an interface
					if subInterfaceType, exprType := v.extractInterfaceType(valueSpecType); subInterfaceType != nil && !v.liftedTypeExists(subInterfaceType) {
						v.visitInterfaceType(subInterfaceType, exprType, csIDName, valueSpec.Comment, true, nil)
					}

					goTypeName := v.getTypeName(def.Type(), false)
					csTypeName := convertToCSTypeName(goTypeName)

					typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName) + len(goIDName) + (len(csIDName) - len(goIDName)))

					if v.inFunction {
						heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

						if len(heapTypeDecl) > 0 {
							v.writeOutput(heapTypeDecl)
						} else {
							if arrayType, ok := valueSpecType.(*ast.ArrayType); ok && arrayType.Len != nil {
								// Handle array type
								var arrayLenValue string
								arrayLenExpr := v.convExpr(arrayType.Len, nil)

								// Check if length expression is in type information
								if tv, ok := v.info.Types[arrayType.Len]; ok {
									// Check if it's a constant
									if tv.Value != nil {
										length := tv.Value
										intLength, _ := constant.Int64Val(length)
										arrayLenValue = strconv.FormatInt(intLength, 10)
									}
								}

								if len(arrayLenValue) > 0 && arrayLenValue != arrayLenExpr {
									v.writeOutput("%s %s = new(%s); /* %s */", csTypeName, csIDName, arrayLenValue, arrayLenExpr)
								} else {
									v.writeOutput("%s %s = new(%s);", csTypeName, csIDName, arrayLenExpr)
								}
							} else {
								v.writeOutput("%s %s = default!;", csTypeName, csIDName)
							}
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
					if i > 0 {
						v.targetFile.WriteString(v.newline)
					}

					csTypeName := v.getCSTypeName(def.Type())
					typeLenDeviation := token.Pos(len(csTypeName) + (len(csIDName) - len(goIDName)))

					if v.inFunction {
						heapTypeDecl := v.convertToHeapTypeDecl(ident, true)

						if len(heapTypeDecl) > 0 {
							v.writeOutputLn(heapTypeDecl)
							v.targetFile.WriteString(v.newline)
							v.writeOutput("%s = %s;", csIDName, v.convExpr(valueSpec.Values[i], []ExprContext{context}))
						} else {
							// Following declarations must use explicit type, do not use `v.options.preferVarDecl` for these:
							v.writeOutput("%s %s = %s;", csTypeName, csIDName, v.convExpr(valueSpec.Values[i], []ExprContext{context}))
						}
					} else {
						access := getAccess(goIDName)
						typeLenDeviation -= token.Pos(len(access) + 9)
						v.writeOutput("%s static %s %s = %s;", access, csTypeName, csIDName, v.convExpr(valueSpec.Values[i], []ExprContext{context}))
					}

					v.writeComment(valueSpec.Comment, valueSpec.Values[i].End()-typeLenDeviation)
				}
				continue
			}

			if i > 0 {
				v.targetFile.WriteString(v.newline)
			}

			var csTypeName string

			if isAnyType {
				csTypeName = "any"
			} else {
				csTypeName = convertToCSTypeName(v.getTypeName(tv.Type, false))
			}

			goValue := tv.Value.ExactString()
			csValue := v.convExpr(valueSpec.Values[i], []ExprContext{context})
			typeLenDeviation := token.Pos(len(csTypeName) + len(csValue) + (len(csIDName) - len(goIDName)) + (len(csValue) - len(goValue)))

			if v.inFunction {
				headTypeDecl := v.convertToHeapTypeDecl(ident, true)

				if len(headTypeDecl) > 0 {
					v.writeOutput(headTypeDecl)

					if len(csValue) > 0 {
						v.targetFile.WriteString(v.newline)
						v.writeOutput("%s = %s;", csIDName, csValue)
					}
				} else {
					if len(csValue) > 0 {
						v.writeOutput("%s %s = %s;", csTypeName, csIDName, csValue)
					} else {
						v.writeOutput("%s %s;", csTypeName, csIDName)
					}
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
			goTypeName := v.getTypeName(c.Type(), false)
			csTypeName := convertToCSTypeName(goTypeName)
			access := getAccess(goIDName)
			typeLenDeviation := token.Pos(len(csTypeName) + len(access) + (len(csIDName) - len(goIDName)))

			// Check if the type is a named type (user-defined), not a basic type
			isNamedType := false

			if _, ok := c.Type().(*types.Named); ok {
				isNamedType = true
			} else if csTypeName == "UntypedInt" || csTypeName == "UntypedFloat" || csTypeName == "UntypedComplex" {
				isNamedType = true
			}

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
				if i > 0 {
					v.targetFile.WriteString(v.newline)
				}

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

			if c.Val().Kind() == constant.Complex {
				// Check if const complex value will exceed complex128 limits
				if _, err := strconv.ParseComplex(constVal, 128); err != nil {
					constVal = c.Val().ExactString()

					// TODO: Assignment of complex value to GoUntyped will need to be handled
					writeUntypedConst()
				}
			}

			if c.Val().Kind() == constant.String {
				if i > 0 {
					v.targetFile.WriteString(v.newline)
				}

				if v.inFunction {
					v.writeOutput("@string %s = %s;", csIDName, constVal)
				} else {
					v.writeOutput("%s static readonly @string %s = %s;", access, csIDName, constVal)
				}

				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation-1)
				constHandled = true
			}

			if !constHandled {
				if i > 0 {
					v.targetFile.WriteString(v.newline)
				}

				if srcVal == "iota" {
					constVal = "iota"
				}

				orgExpr := ""

				if len(valueSpec.Values) >= i+1 {
					orgExpr = strings.TrimSpace(v.getPrintedNode(valueSpec.Values[i]))
				}

				if constVal == orgExpr {
					orgExpr = ""
				} else {
					// Try parse both constVal and orgExpr as floating point numbers to see if they are same
					if constNum, err := strconv.ParseFloat(constVal, 64); err == nil {
						if orgNum, err := strconv.ParseFloat(orgExpr, 64); err == nil {
							if constNum == orgNum {
								orgExpr = ""
							}
						}
					}

					if len(orgExpr) > 0 {
						if strings.Contains(orgExpr, "unsafe.Sizeof") {
							v.showWarning("Go const converted to C# using 'unsafe.Sizeof' may not match run-time value - verify usage: const %s = %s", goIDName, orgExpr)
						}

						orgExpr = fmt.Sprintf(" /* %s */", orgExpr)
					}
				}

				var constExpr string

				if isNamedType {
					constExpr = "static readonly"
				} else {
					constExpr = "const"
				}

				if v.inFunction {
					v.writeOutput("%s %s %s =%s %s;", constExpr, csTypeName, csIDName, orgExpr, constVal)
				} else {
					v.writeOutput("%s %s %s %s =%s %s;", access, constExpr, csTypeName, csIDName, orgExpr, constVal)
				}

				v.writeComment(valueSpec.Comment, tokEnd+typeLenDeviation+1)
			}
		}
	} else {
		println(fmt.Sprintf("Unexpected ValueSpec token type: %s", tok))
	}
}
