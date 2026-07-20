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
	var liftedIsPublicized bool

	// Intra-function type declarations are not allowed in C#
	if lifted {
		// A lift can arrive with an EMPTY name — an anonymous struct in a call-argument slot
		// whose parameter is unnamed (builtin `new(struct{ types.Type })`, go/internal/
		// gccgoimporter's reserved). An empty name would declare `partial struct  {` and
		// register "" for every reference to the type — a whole-package syntax cascade. Fall
		// back to the generic "type" the other anonymous-type call sites pass
		// (convStructType/convStarExpr).
		if name == "" {
			name = "type"
		}

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

		// A lifted anonymous struct referenced by a PUBLICIZED interface method (or an exported
		// method/func/delegate) signature must itself be emitted `public`, or it is less accessible
		// than the public member (CS0050/CS0051 — testing's `type corpusEntry = struct{…}` alias
		// lifts to `corpusEntryᴛ1`, referenced by the public `testDeps` fuzzing methods). The lift
		// has no *types.Object, so the publicize pre-pass records the anonymous type itself.
		liftedIsPublicized = isPublicizedLiftedType(identType) || isPublicizedLiftedType(structSignatureType)
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

	// A lifted anonymous type carries no pendingTypeAccess (only a top-level TypeSpec sets it), so a
	// lift reached through a public surface is publicized here instead (see liftedIsPublicized).
	if liftedIsPublicized && access == "" {
		access = "public "
	}

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
			} else if interfaceType, ok := ptrType.X.(*ast.InterfaceType); ok && !isEmptyInterface(interfaceType) && !v.liftedTypeExists(interfaceType) {
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
		} else if interfaceType, ok := field.Type.(*ast.InterfaceType); ok && !isEmptyInterface(interfaceType) {
			// An EMPTY interface field (`ptr interface{}`, e.g. encoding/json's slice-cycle memo
			// struct) is NOT lifted to a named `[GoType("dyn")]` marker interface — that empty
			// marker is implemented by nothing, so a concrete value assigned to the field (a
			// boxed `uintptr` from `v.UnsafePointer()`) fails to convert (CS1503). It maps to
			// `any` via the field-type conversion below, matching how extractInterfaceType (the
			// canonical lift gate) already excludes empty interfaces everywhere else.
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
			} else if interfaceType, ok := arrayType.Elt.(*ast.InterfaceType); ok && !isEmptyInterface(interfaceType) && !v.liftedTypeExists(interfaceType) {
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

		// The fully-qualified form for emission INTO this source file's body. csFullTypeName is a
		// RELATIVE dotted name (`io.fs_package.FS`); when its leading segment is also imported as a
		// package alias in this file (`using io = io_package;`) C# binds it to that TYPE alias, so the
		// name resolves to the nonexistent nested type `io_package.fs_package.FS` (CS0426). Root-qualify
		// (`go.io.fs_package.FS`) so the leading segment resolves as the child NAMESPACE it names. The
		// unqualified csFullTypeName is kept below as the promotedInterfaceImplementations map KEY, which
		// feeds generator-consumed strings that live in alias-less files (where the relative form
		// resolves and the key must stay stable).
		csEmitTypeName := rootQualifyIfAmbiguous(csFullTypeName)

		// For the actual NAMED-field declaration, prefer the readable file-local package alias
		// (`atomic.Int32` over `sync.atomic_package.Int32`) when this file imports the type's
		// package — keeping the emitted field visually close to the Go source. The fully-qualified
		// csFullTypeName is retained for promotion/interface registration below, which feeds
		// generator-consumed strings that live in alias-less files. (Embedded fields keep the full
		// form for their promoted accessors; only the named-field branch uses the display name.)
		goDisplayTypeName := v.getDisplayTypeName(fieldType, false)
		csDisplayTypeName := convertToCSTypeName(goDisplayTypeName)

		// A func-typed field whose signature names a type from a MULTI-SEGMENT import path
		// (`Values func([]reflect.Value, *rand.Rand)`, where `rand` is `math/rand`) must be
		// rendered structurally as an Action/Func delegate via getCSTypeName. The string-based
		// getTypeName/convertToCSTypeName path stringifies the signature as
		// `func([]reflect.Value, *math/rand.Rand)` and then feeds the slash-bearing import path to
		// convertImportPathToNamespace, which splits on '/' and emits the dotted `math.rand.Rand` —
		// but `math` aliases to `math_package`, so `math.rand` resolves to the non-existent
		// `math_package.rand` (CS0426). getCSTypeName recurses through the signature per element,
		// qualifying each named type by its package NAME (`rand.Rand`), the alias the file imports.
		//
		// A VARIADIC func-typed field reroutes too: the string path cannot render a variadic
		// signature at all — getTypeName's '..' strip reduces the ellipsis of
		// `JoinPath func(elem ...string) string` (go/build's Context) to `.string`, emitting the
		// unparseable `Func<.@string, @string>` (CS1031 + CS1003 ×2), and even unstripped it has
		// no variadic lowering. Structurally the field renders the golib variadic delegate family
		// (`Funcꓸꓸꓸ<@string, @string>` — see iifeDelegateType), which loose-arg, empty and spread
		// calls through the field all bind.
		//
		// Every other signature keeps the display path: a func field with no cross-package import —
		// `func(string) (importPath string, ok bool)` — preserves its named tuple elements
		// (structural rendering drops them). Compiling correctness for the broken cases is worth
		// the lost tuple names in the rare rerouted field.
		if sig, isSignature := fieldType.(*types.Signature); isSignature && (sig.Variadic() || strings.Contains(goDisplayTypeName, "/")) {
			csDisplayTypeName = v.getCSTypeName(fieldType)
		}

		displayLenDeviation := token.Pos(len(csDisplayTypeName) - len(goDisplayTypeName))
		typeLenDeviation := token.Pos(len(csFullTypeName) - len(goFullTypeName))

		var arrayInitializer string

		if arrayType, ok := field.Type.(*ast.ArrayType); ok {
			if arrayType.Len != nil {
				arrayInitializer = fmt.Sprintf(" = new(%s)", v.arrayZeroValueArgs(v.convExpr(arrayType.Len, nil), fieldType))
			}
		}

		if field.Names == nil {
			// Check for promoted fields
			var ident *ast.Ident
			var ok bool

			var isIdentFieldType bool
			var selectorType bool

			// A GENERIC embed (`node[K, V]` — internal/concurrent's entry) arrives as an
			// IndexExpr/IndexListExpr over the base type expression; unwrap it (in both the
			// plain and pointer forms below) so the embed emits — it was silently DROPPED
			// (the struct lost the field entirely, every promoted access CS0117).
			unwrapGeneric := func(expr ast.Expr) ast.Expr {
				switch index := expr.(type) {
				case *ast.IndexExpr:
					return index.X
				case *ast.IndexListExpr:
					return index.X
				}

				return expr
			}

			if ident, ok = unwrapGeneric(field.Type).(*ast.Ident); ok {
				isIdentFieldType = true
			} else if ptrType, ok := field.Type.(*ast.StarExpr); ok {
				if ident, ok = unwrapGeneric(ptrType.X).(*ast.Ident); ok {
					isIdentFieldType = true
				}
			}

			if !isIdentFieldType {
				if selectorExpr, ok := unwrapGeneric(field.Type).(*ast.SelectorExpr); ok {
					if ident, ok = selectorExpr.X.(*ast.Ident); ok {
						isIdentFieldType = true
						selectorType = true
					}
				} else if ptrType, ok := field.Type.(*ast.StarExpr); ok {
					if selectorExpr, ok := unwrapGeneric(ptrType.X).(*ast.SelectorExpr); ok {
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

			// A generic embed's MEMBER NAME is the base type name (Go promotes entry[K,V]'s
			// embedded node[K,V] through the selector `.node`), so strip the type arguments —
			// and do it BEFORE the selector dot-strip: the arguments may contain qualified
			// types whose dots otherwise win the LastIndex (uniqueMap's
			// `*concurrent.HashTrieMap[T, weak.Pointer[T]]` named its member `Pointer`).
			if bracketIndex := strings.Index(goTypeName, "["); bracketIndex != -1 {
				goTypeName = goTypeName[:bracketIndex]
			}

			// An embedded field's NAME is the UNQUALIFIED type name (Go spec), so strip any package
			// qualifier. A selector embed (`io.Writer`) carries it explicitly; a DOT-IMPORTED ident
			// embed does too once resolved — io_test's `import . "io"` + embedded `ReaderFrom` reaches
			// here as a bare *ast.Ident whose getTypeName still renders the (collision-renamed)
			// package qualifier `Δio.ReaderFrom`. Gating the strip on selectorType left that qualifier
			// in the field name (`Δio.ReaderFrom`), whose dot is a C# syntax error (CS1003/CS1026).
			// Strip whenever a qualifier survives, covering both forms; a same-package embed has no
			// dot, so this is a no-op there (byte-identical).
			if dotIndex := strings.LastIndex(goTypeName, "."); dotIndex != -1 {
				// Get the unqualified name of the embedded type
				goTypeName = goTypeName[dotIndex+1:]
			}

			// Lookup identity to determine if it's an interface — for a SELECTOR embed
			// (io.Writer) resolve the SEL, not the package ident: a cross-package
			// INTERFACE embed otherwise took the promoted-STRUCT property form, and the
			// generator tried to construct the interface (archive/tar's lifted
			// `struct{ io.Writer }`, CS0144 ×8 + CS1929 ×4).
			identObj := v.info.ObjectOf(ident)

			if selectorType {
				if selectorExpr, ok := unwrapGeneric(field.Type).(*ast.SelectorExpr); ok {
					identObj = v.info.ObjectOf(selectorExpr.Sel)
				} else if ptrType, ok := field.Type.(*ast.StarExpr); ok {
					if selectorExpr, ok := unwrapGeneric(ptrType.X).(*ast.SelectorExpr); ok {
						identObj = v.info.ObjectOf(selectorExpr.Sel)
					}
				}
			}

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

				v.writeString(target, "%s %s %s;", getAccess(goTypeName), csEmitTypeName, getCoreSanitizedIdentifier(goTypeName))
			} else {
				var handled bool

				if _, ok := identObj.(*types.PkgName); !ok {
					if ptrType, ok := identType.(*types.Pointer); ok {
						if _, ok = ptrType.Elem().(*types.Named); !ok {
							v.writeString(target, "%s %s %s;", getAccess(goTypeName), csEmitTypeName, getCoreSanitizedIdentifier(goTypeName))
							handled = true
						}
					} else if _, ok = identType.(*types.Struct); !ok {
						if _, ok := identObj.Type().(*types.Named); !ok {
							v.writeString(target, "%s %s %s;", getAccess(goTypeName), csEmitTypeName, getCoreSanitizedIdentifier(goTypeName))
							handled = true
						}
					}
				}

				// Handle promoted struct implementations
				if !handled {
					v.writeString(target, "%s partial ref %s %s { get; }", getAccess(goTypeName), csEmitTypeName, getCoreSanitizedIdentifier(goTypeName))
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
					} else if strings.TrimPrefix(fieldName, "@") == strings.TrimPrefix(strings.TrimPrefix(structTypeName, ShadowVarMarker), "@") {
						// C# forbids a member sharing its enclosing type's name (CS0542), so rename a
						// field whose name equals the struct type with the disambiguation marker. Field
						// accesses are renamed to match (see convSelectorExpr / convIdent). Both sides
						// compare RAW (escape/rename markers stripped): net parse.go's `type file
						// struct{ file *os.File }` renames the TYPE to Δfile (CS9056) and escapes the
						// FIELD to @file — the literal compare missed, declaring `@file` while every
						// access site emitted `Δfile` (CS1061 ×3).
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

// structZeroValueNeedsConstruction reports whether a struct type's zero value default(T) is
// BROKEN — it has a promoted-embed box (constructor-allocated) or a fixed-size array field
// (`= new(N)` field initializer that default(T) skips), directly or through a nested value-struct
// field — so `var z T` must run the generated parameterless constructor (`new()`) rather than
// emit `default!`. Mirrors go2cs-gen StructTypeTemplate.NeedsConstruction; a false result keeps the
// existing `default!`/bare emission. The top-level promoted-embed case is routed to `new(nil)` by
// the caller's earlier structHasPromotedEmbeds check — this predicate still recurses for it so a
// NESTED field whose own type carries a promoted embed (or array) also constructs.
func (v *Visitor) structZeroValueNeedsConstruction(t types.Type) bool {
	return v.structZeroValueNeedsConstructionRec(t, map[*types.Struct]bool{})
}

func (v *Visitor) structZeroValueNeedsConstructionRec(t types.Type, seen map[*types.Struct]bool) bool {
	if t == nil {
		return false
	}

	st, ok := t.Underlying().(*types.Struct)

	if !ok {
		return false
	}

	// Go forbids value-type embedding cycles (infinite size), so a cycle cannot actually occur —
	// the guard is purely defensive.
	if seen[st] {
		return false
	}

	seen[st] = true

	// Any promoted embed surfaces as a constructor-allocated `ж<T>` box — default leaves it null.
	if v.structHasPromotedEmbeds(t) {
		return true
	}

	for i := range st.NumFields() {
		field := st.Field(i)

		if field.Name() == "_" {
			continue
		}

		fieldType := field.Type()

		// A reference field keeps its nil zero value (correct — a nil pointer/slice/map/chan/func
		// matches Go), so it never forces construction; skipping it also stops the recursion from
		// descending through a self-referential pointer field.
		if isInherentlyHeapAllocatedType(fieldType) {
			continue
		}

		// A fixed-size array field (`[N]T` → golib array<T>) carries a `= new(N)` field initializer
		// that default(T) skips, leaving a null backing.
		if _, isArray := fieldType.Underlying().(*types.Array); isArray {
			return true
		}

		// A nested value-struct field whose own type needs construction.
		if v.structZeroValueNeedsConstructionRec(fieldType, seen) {
			return true
		}
	}

	return false
}
