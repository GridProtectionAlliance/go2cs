package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

const StructPrefixMarker = ">>MARKER:STRUCT_%s_PREFIX<<"

// Handles struct types in the context of a TypeSpec, ValueSpec, or FieldList
func (v *Visitor) visitStructType(structType *ast.StructType, identType types.Type, name string, doc *ast.CommentGroup, lifted bool, target *strings.Builder) (structTypeName string) {
	var preLiftIndentLevel int
	var structPrefix *strings.Builder

	// Intra-function type declarations are not allowed in C#
	if lifted {
		if v.inFunction {
			if target == nil {
				target = &strings.Builder{}
			}

			if !strings.HasPrefix(name, v.currentFuncName+"_") {
				name = fmt.Sprintf("%s_%s", v.currentFuncName, name)
			}

			preLiftIndentLevel = v.indentLevel
			v.indentLevel = 0
		}

		structTypeName = v.getUniqueLiftedTypeName(name)
		v.liftedTypeMap[identType] = structTypeName
		structSignatureType := v.getType(structType, false)
		v.liftedTypeMap[structSignatureType] = structTypeName

		// Package-level lifted structs are shared across the package so other files
		// can resolve cross-file references to this anonymous type (function-local
		// lifts are file/function-scoped and stay out of the shared registry).
		if !v.inFunction && structSignatureType != nil {
			registerDynamicTypeName(structSignatureType.String(), structTypeName)
		}
	} else {
		structTypeName = name
	}

	if target == nil {
		target = v.targetFile

		if !v.inFunction {
			target.WriteString(v.newline)
		}
	}

	structTypeName = getSanitizedIdentifier(structTypeName)
	typeParams, constraints := v.getGenericDefinition(identType)

	if len(constraints) == 0 {
		constraints = " "
	} else {
		constraints = fmt.Sprintf("%s%s%s", constraints, v.newline, v.indent(v.indentLevel))
	}

	if !v.inFunction {
		structPrefix = &strings.Builder{}
	}

	structPrefixMarker := fmt.Sprintf(StructPrefixMarker, structTypeName)
	target.WriteString(structPrefixMarker)
	v.writeDocString(target, doc, structType.Pos())

	var dynamic string

	if lifted {
		dynamic = "(\"dyn\")"
	}

	// Consume any pending publicized-type access modifier (an unexported type used as an
	// exported field). Only the top-level type declaration carries it; nested/anonymous lifts do
	// not, so read and clear before visiting fields (which may recurse into this function).
	access := v.pendingTypeAccess
	v.pendingTypeAccess = ""

	v.writeStringLn(target, "[GoType%s] %spartial struct %s%s%s{", dynamic, access, structTypeName, typeParams, constraints)
	v.indentLevel++

	var prevNameDiscardedCount int

	for _, field := range structType.Fields.List {
		v.writeDocString(target, field.Doc, field.Pos())

		if field.Tag != nil {
			v.writeString(target, "[GoTag(")
			target.WriteString(v.convBasicLit(field.Tag, BasicLitContext{u8StringOK: false}))
			target.WriteString(")]")
			target.WriteString(v.newline)
		}

		var indentOffset int

		if v.inFunction {
			indentOffset = 1
		} else {
			indentOffset = -1
		}

		// Check if field is a struct or a pointer to a struct
		if ptrType, ok := field.Type.(*ast.StarExpr); ok {
			if subStructType, ok := ptrType.X.(*ast.StructType); ok && !v.liftedTypeExists(subStructType) {
				subStructIdentType := v.getExprType(ptrType.X)
				v.indentLevel += indentOffset
				v.visitStructType(subStructType, subStructIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true, structPrefix)
				v.indentLevel -= indentOffset

				if structPrefix != nil {
					structPrefix.WriteString(v.newline)
				}

				// Track sub-struct types
				subStructTypes := v.subStructTypes[identType]

				if subStructTypes == nil {
					subStructTypes = make([]types.Type, 0)
				}

				subStructTypes = append(subStructTypes, v.getExprType(ptrType))
				v.subStructTypes[identType] = subStructTypes
			} else if interfaceType, ok := ptrType.X.(*ast.InterfaceType); ok && !v.liftedTypeExists(interfaceType) {
				interfaceIdentType := v.getExprType(ptrType.X)
				v.indentLevel += indentOffset
				v.visitInterfaceType(interfaceType, interfaceIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true, structPrefix)
				v.indentLevel -= indentOffset

				if structPrefix != nil {
					structPrefix.WriteString(v.newline)
				}
			}
		} else if subStructType, ok := field.Type.(*ast.StructType); ok {
			subStructIdentType := v.getExprType(field.Type)
			v.indentLevel += indentOffset
			v.visitStructType(subStructType, subStructIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true, structPrefix)
			v.indentLevel -= indentOffset

			if structPrefix != nil {
				structPrefix.WriteString(v.newline)
			}

			// Track sub-struct types
			subStructTypes := v.subStructTypes[identType]

			if subStructTypes == nil {
				subStructTypes = make([]types.Type, 0)
			}

			subStructTypes = append(subStructTypes, subStructIdentType)
			v.subStructTypes[identType] = subStructTypes
		} else if interfaceType, ok := field.Type.(*ast.InterfaceType); ok {
			interfaceIdentType := v.getExprType(field.Type)
			v.indentLevel += indentOffset
			v.visitInterfaceType(interfaceType, interfaceIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true, structPrefix)
			v.indentLevel -= indentOffset

			if structPrefix != nil {
				structPrefix.WriteString(v.newline)
			}
		} else if arrayType, ok := field.Type.(*ast.ArrayType); ok && len(field.Names) > 0 {
			// An array/slice field whose element is an anonymous struct/interface (e.g. runtime's
			// `MemStats.BySize [61]struct{…}`): lift the element type so the field declaration
			// resolves to a named type (`array<BySizeᴛ1>`) instead of a raw, un-compilable
			// `struct{…}`. getTypeName resolves the array element through liftedTypeMap.
			if subStructType, ok := arrayType.Elt.(*ast.StructType); ok && !v.liftedTypeExists(subStructType) {
				subStructIdentType := v.getExprType(arrayType.Elt)
				v.indentLevel += indentOffset
				v.visitStructType(subStructType, subStructIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true, structPrefix)
				v.indentLevel -= indentOffset

				if structPrefix != nil {
					structPrefix.WriteString(v.newline)
				}
			} else if interfaceType, ok := arrayType.Elt.(*ast.InterfaceType); ok && !v.liftedTypeExists(interfaceType) {
				interfaceIdentType := v.getExprType(arrayType.Elt)
				v.indentLevel += indentOffset
				v.visitInterfaceType(interfaceType, interfaceIdentType, fmt.Sprintf("%s_%s", structTypeName, field.Names[0].Name), field.Comment, true, structPrefix)
				v.indentLevel -= indentOffset

				if structPrefix != nil {
					structPrefix.WriteString(v.newline)
				}
			}
		}

		fieldType := v.getType(field.Type, false)
		goTypeName := v.getTypeName(fieldType, false)
		goFullTypeName := v.getFullTypeName(fieldType, false)
		csFullTypeName := convertToCSTypeName(goFullTypeName)

		// For the actual NAMED-field declaration, prefer the readable file-local package alias
		// (`atomic.Int32` over `sync.atomic_package.Int32`) when this file imports the type's
		// package — keeping the emitted field visually close to the Go source. The fully-qualified
		// csFullTypeName is retained for promotion/interface registration below, which feeds
		// generator-consumed strings that live in alias-less files. (Embedded fields keep the full
		// form for their promoted accessors; only the named-field branch uses the display name.)
		goDisplayTypeName := v.getDisplayTypeName(fieldType, false)
		csDisplayTypeName := convertToCSTypeName(goDisplayTypeName)
		displayLenDeviation := token.Pos(len(csDisplayTypeName) - len(goDisplayTypeName))
		typeLenDeviation := token.Pos(len(csFullTypeName) - len(goFullTypeName))

		var arrayInitializer string

		if arrayType, ok := field.Type.(*ast.ArrayType); ok {
			if arrayType.Len != nil {
				arrayInitializer = fmt.Sprintf(" = new(%s)", v.convExpr(arrayType.Len, nil))
			}
		}

		if field.Names == nil {
			// Check for promoted fields
			var ident *ast.Ident
			var ok bool

			var isIdentFieldType bool
			var selectorType bool

			if ident, ok = field.Type.(*ast.Ident); ok {
				isIdentFieldType = true
			} else if ptrType, ok := field.Type.(*ast.StarExpr); ok {
				if ident, ok = ptrType.X.(*ast.Ident); ok {
					isIdentFieldType = true
				}
			}

			if !isIdentFieldType {
				if selectorExpr, ok := field.Type.(*ast.SelectorExpr); ok {
					if ident, ok = selectorExpr.X.(*ast.Ident); ok {
						isIdentFieldType = true
						selectorType = true
					}
				} else if ptrType, ok := field.Type.(*ast.StarExpr); ok {
					if selectorExpr, ok := ptrType.X.(*ast.SelectorExpr); ok {
						if ident, ok = selectorExpr.X.(*ast.Ident); ok {
							isIdentFieldType = true
							selectorType = true
						}
					}
				}
			}

			if !isIdentFieldType {
				continue
			}

			if selectorType {
				// Get index of last dot in go type name
				dotIndex := strings.LastIndex(goTypeName, ".")

				if dotIndex != -1 {
					// Get the name of the struct type
					goTypeName = goTypeName[dotIndex+1:]
				}
			}

			// Lookup identity to determine if it's an interface
			identObj := v.info.ObjectOf(ident)

			if identObj == nil {
				continue // Could not find the object of ident
			}

			identType := identObj.Type().Underlying()

			if _, ok := identType.(*types.Interface); ok {
				// Add to promoted interface implementations
				packageLock.Lock()

				if promotions, exists := promotedInterfaceImplementations[csFullTypeName]; exists {
					promotions.Add(structTypeName)
				} else {
					promotedInterfaceImplementations[csFullTypeName] = NewHashSet([]string{structTypeName})
				}

				packageLock.Unlock()

				v.writeString(target, "%s %s %s;", getAccess(goTypeName), csFullTypeName, getCoreSanitizedIdentifier(goTypeName))
			} else {
				var handled bool

				if _, ok := identObj.(*types.PkgName); !ok {
					if ptrType, ok := identType.(*types.Pointer); ok {
						if _, ok = ptrType.Elem().(*types.Named); !ok {
							v.writeString(target, "%s %s %s;", getAccess(goTypeName), csFullTypeName, getCoreSanitizedIdentifier(goTypeName))
							handled = true
						}
					} else if _, ok = identType.(*types.Struct); !ok {
						if _, ok := identObj.Type().(*types.Named); !ok {
							v.writeString(target, "%s %s %s;", getAccess(goTypeName), csFullTypeName, getCoreSanitizedIdentifier(goTypeName))
							handled = true
						}
					}
				}

				// Handle promoted struct implementations
				if !handled {
					v.writeString(target, "%s partial ref %s %s { get; }", getAccess(goTypeName), csFullTypeName, getCoreSanitizedIdentifier(goTypeName))
				}
			}

			v.writeCommentString(target, field.Comment, field.Type.End()+typeLenDeviation)
			target.WriteString(v.newline)
		} else {
			// Match the Go source's line grouping for readability: when a single Go field
			// declaration groups multiple names (`x, y int`), emit one combined C# line
			// (`internal nint x, y;`). This is only safe when every name shares the same
			// access modifier and emitted type and none needs per-name special handling —
			// blank `_` (renamed per occurrence), a name colliding with the struct type
			// (Δ-marker rename), or a per-field array initializer (` = new(N)`). The names in
			// one field group already share field.Type/Tag/Comment, so only access and the
			// per-name renames can diverge. When any apply, fall back to one line per name.
			canCombine := len(field.Names) > 1 && arrayInitializer == ""

			if canCombine {
				groupAccess := getAccess(field.Names[0].Name)

				for _, ident := range field.Names {
					fieldName := getCoreSanitizedIdentifier(ident.Name)

					if fieldName == "_" || fieldName == structTypeName || getAccess(ident.Name) != groupAccess {
						canCombine = false
						break
					}
				}
			}

			if canCombine {
				fieldNames := make([]string, len(field.Names))

				for i, ident := range field.Names {
					fieldNames[i] = getCoreSanitizedIdentifier(ident.Name)
				}

				v.writeString(target, "%s %s %s;", getAccess(field.Names[0].Name), csDisplayTypeName, strings.Join(fieldNames, ", "))
				v.writeCommentString(target, field.Comment, field.Type.End()+displayLenDeviation)
				target.WriteString(v.newline)
			} else {
				for _, ident := range field.Names {
					fieldName := getCoreSanitizedIdentifier(ident.Name)

					if fieldName == "_" {
						for range prevNameDiscardedCount {
							fieldName = fieldName + "_"
						}

						prevNameDiscardedCount++
					} else if fieldName == structTypeName {
						// C# forbids a member sharing its enclosing type's name (CS0542), so rename a
						// field whose name equals the struct type with the disambiguation marker. Field
						// accesses are renamed to match (see convSelectorExpr / convIdent).
						fieldName = typeCollidingFieldName(fieldName)
					}

					v.writeString(target, "%s %s %s%s;", getAccess(ident.Name), csDisplayTypeName, fieldName, arrayInitializer)
					v.writeCommentString(target, field.Comment, field.Type.End()+displayLenDeviation)
					target.WriteString(v.newline)
				}
			}
		}
	}

	v.indentLevel--
	v.writeStringLn(target, "}")

	if structPrefix == nil {
		v.replaceMarkerString(target, structPrefixMarker, "")
	} else {
		v.replaceMarkerString(target, structPrefixMarker, structPrefix.String())
	}

	if lifted && v.inFunction {
		if v.currentFuncPrefix.Len() > 0 {
			v.currentFuncPrefix.WriteString(v.newline)
		}

		v.currentFuncPrefix.WriteString(target.String())
		target.Reset()
		v.indentLevel = preLiftIndentLevel
	}

	return
}

// structHasPromotedEmbeds reports whether the type's underlying struct carries at least one
// embedded field that the generated C# stores in a constructor-initialized readonly `ж<T>` box
// (the StructTypeTemplate "Promoted Struct References"). A `default`-valued instance of such a
// struct has null boxes, so the first promoted-member access throws NullReferenceException —
// an uninitialized declaration must render `new T(nil)` instead of `default!`. The decision
// mirrors the embedded-field emission above: an embed renders as a `partial ref` promotion
// (and thus a box) unless it is a same-package interface, a builtin non-named embed (`int`),
// or a pointer to a non-named type; a CROSS-PACKAGE embed always takes the promotion path
// (the selector-type branch above bypasses every plain-field case, interfaces included).
func (v *Visitor) structHasPromotedEmbeds(t types.Type) bool {
	if t == nil {
		return false
	}

	st, ok := t.Underlying().(*types.Struct)

	if !ok {
		return false
	}

	for i := range st.NumFields() {
		field := st.Field(i)

		if !field.Anonymous() {
			continue
		}

		fieldType := field.Type()

		// Resolve the embed's named type, through one syntactic pointer (`*X`).
		named, _ := types.Unalias(fieldType).(*types.Named)

		if named == nil {
			if ptr, isPtr := fieldType.(*types.Pointer); isPtr {
				named, _ = types.Unalias(ptr.Elem()).(*types.Named)
			}
		}

		// A cross-package embed always renders as a promoted box.
		if named != nil && named.Obj().Pkg() != nil && named.Obj().Pkg() != v.pkg {
			return true
		}

		// Same-package `*X` embed: any named pointee promotes (struct underlying and named
		// non-struct both take the partial-ref path); `*int` (builtin pointee) stays plain.
		if ptr, isPtr := fieldType.(*types.Pointer); isPtr {
			if _, isNamed := types.Unalias(ptr.Elem()).(*types.Named); isNamed {
				return true
			}

			continue
		}

		underlying := fieldType.Underlying()

		// A same-package interface embed renders as a plain interface field — no box.
		if _, isInterface := underlying.(*types.Interface); isInterface {
			continue
		}

		// A named-pointer-type embed (`type P *T`) promotes only when the pointee is named.
		if ptr, isPtr := underlying.(*types.Pointer); isPtr {
			if _, isNamed := types.Unalias(ptr.Elem()).(*types.Named); isNamed {
				return true
			}

			continue
		}

		// A value embed promotes when its underlying is a struct or the embed itself is a
		// named type (`type RCode int` embeds as a partial-ref box despite the basic core).
		if _, isStruct := underlying.(*types.Struct); isStruct {
			return true
		}

		if named != nil {
			return true
		}
	}

	return false
}
