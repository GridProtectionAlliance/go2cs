// visitArrayType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

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

// Handles array and slice types in context of a TypeSpec
func (v *Visitor) visitArrayType(arrayType *ast.ArrayType, identType types.Type, name string, comment *ast.CommentGroup) {
	// Resolve the element type's C# name. A simple identifier element (e.g. `type d [3]rune`)
	// keeps its written name so the GoType attribute reads `[3]rune`; a composite element — a
	// selector, a (generic) instantiation, a pointer, etc. (e.g. `[N]atomic.Pointer[entry[K, V]]`)
	// — must be resolved through the type system, since getIdentifier would collapse it to just
	// its leading identifier (`atomic`), mangling the GoType and the generated array element.
	var csTypeName, goTypeName string

	// An anonymous-struct (or -interface) element of a NAMED array/slice type — `type semTable
	// [N]struct{…}` (runtime/sema) — must be lifted to a named type, otherwise the GoType
	// attribute and any `&t[i].field` (which emits `.of(ElemType.ᏑField)`) reference a raw,
	// un-compilable `struct{…}`. Lift it (named after the array type) and use the lifted name.
	if structType, ok := arrayType.Elt.(*ast.StructType); ok && !isEmptyStruct(structType) {
		eltType := v.info.TypeOf(arrayType.Elt)

		if !v.liftedTypeExists(structType) {
			liftedName := v.visitStructType(structType, eltType, name, nil, true, nil)
			csTypeName = liftedName
			goTypeName = liftedName
		} else if ln, ok := v.liftedTypeMap[eltType]; ok {
			csTypeName = ln
			goTypeName = ln
		}
	} else if interfaceType, ok := arrayType.Elt.(*ast.InterfaceType); ok && !interfaceType.Incomplete && len(interfaceType.Methods.List) > 0 {
		eltType := v.info.TypeOf(arrayType.Elt)

		if !v.liftedTypeExists(interfaceType) {
			liftedName := v.visitInterfaceType(interfaceType, eltType, name, nil, true, nil)
			csTypeName = liftedName
			goTypeName = liftedName
		} else if ln, ok := v.liftedTypeMap[eltType]; ok {
			csTypeName = ln
			goTypeName = ln
		}
	}

	if csTypeName != "" {
		// element already resolved to a lifted name above
	} else if ident := getIdentifier(arrayType.Elt); ident != nil && isSimpleIdentExpr(arrayType.Elt) && !v.liftedTypeExists(arrayType.Elt) {
		goTypeName = ident.Name
		csTypeName = convertToCSTypeName(goTypeName)
	} else if eltType := v.info.TypeOf(arrayType.Elt); eltType != nil {
		// Use the fully-qualified name (getFullTypeName) rather than the package-aliased form: the
		// GoType attribute is consumed by the generated array-backed partial, which lives in a file
		// without this file's package-relative `using` aliases (e.g. `using atomic = ...`), so an
		// aliased element type would be unresolvable there (CS0246). The leading namespace segment
		// must be the CANONICAL qualifier, never a file-local Δ collision-rename (same rule as the
		// visitTypeSpec global-using target).
		csTypeName = canonicalizeQualifierRename(convertToCSTypeName(v.getFullTypeName(eltType, false)))
		goTypeName = csTypeName
	} else {
		typeName := v.getPrintedNode(arrayType.Elt)
		v.showWarning("@visitArrayType - Failed to resolve 'ast.ArrayType' element %s", typeName)
		v.writeOutputLn("// [...]%s", typeName)
		return
	}

	typeLenDeviation := token.Pos(len(csTypeName) - len(goTypeName))

	access := v.pendingTypeAccess
	v.pendingTypeAccess = ""

	// A slice/array type declared inside a function body (`type People []Person`, example_test.go)
	// cannot be a method-body statement in C#; hoist it to member level (see liftLocalTypeDecl). A
	// package-level declaration is unaffected — target is v.targetFile and finish() is a no-op.
	name, target, finish := v.liftLocalTypeDecl(name, identType)

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	if arrayType.Len == nil {
		// Handle slice type
		v.writeString(target, "[GoType(\"[]%s\")] ", csTypeName)
	} else {
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
			v.writeString(target, "[GoType(\"[%s]%s\")] /* [%s]%s */%s", arrayLenValue, csTypeName, arrayLenExpr, csTypeName, v.newline)
		} else {
			v.writeString(target, "[GoType(\"[%s]%s\")] ", arrayLenExpr, csTypeName)
		}
	}

	// Append generic type parameters and constraints (e.g. `<K, V> where K : new()`) for a generic
	// named array type so the forward declaration matches its uses, and the constraints propagate
	// type-wide to the generated array-backed partial (whose element type may require them).
	typeParams, constraints := v.getGenericDefinition(identType)

	v.writeString(target, "%spartial struct %s%s%s;", access, getSanitizedIdentifier(name), typeParams, constraints)
	v.writeCommentString(target, comment, arrayType.Elt.End()+typeLenDeviation)
	target.WriteString(v.newline)
	finish()
}

// arrayZeroValueArgs renders the constructor arguments for a fixed-size array's zero value: the
// length, plus an element factory when `default(T)` is not usable storage for the element type.
//
// golib's `new array<T>(N)` fills its backing with `default(T)`, which is only the correct Go zero
// value when `default(T)` is itself well formed. It is NOT when the element is:
//
//   - another UNNAMED fixed-size array — `[2][4]byte` emits `array<array<byte>>`, and the inner
//     length lives only in the Go type, never in `array<T>`, so every element would keep a null
//     backing: `len(x[1])` reports 0 (Go says 4) and the first indexed write panics; or
//   - a struct whose own zero value needs construction — `default(T)` skips the generated
//     constructor that runs its fixed-array field initializers and allocates its embed boxes.
//
// A NAMED array element (`type row [4]byte`) needs no factory: its generated wrapper allocates its
// backing lazily from its own known size (go2cs-gen's `m_value ??= new row(4)`).
//
// Mirrors go2cs-gen's AppendZeroValueInitializers, which does the same for struct FIELDS. Every
// other element type renders the bare length, so only genuinely nested shapes change.
func (v *Visitor) arrayZeroValueArgs(lengthExpr string, arrayType types.Type) string {
	if arrayType == nil {
		return lengthExpr
	}

	array, ok := arrayType.Underlying().(*types.Array)

	if !ok {
		return lengthExpr
	}

	elemFactory := v.arrayElemFactory(array.Elem())

	if len(elemFactory) == 0 {
		return lengthExpr
	}

	return fmt.Sprintf("%s, () => %s", lengthExpr, elemFactory)
}

// arrayElemFactory renders the target-typed construction expression for one element of a
// fixed-size array, or "" when `default(T)` is already the correct zero value. See
// arrayZeroValueArgs for which element shapes need one.
func (v *Visitor) arrayElemFactory(elemType types.Type) string {
	if elemType == nil {
		return ""
	}

	// A NAMED element keeps its own zero-value handling — an array wrapper allocates its backing
	// lazily, and a struct routes through the constructor forms below — so only an unnamed nested
	// array needs its length threaded through here.
	if _, isNamed := types.Unalias(elemType).(*types.Named); !isNamed {
		if innerArray, isArray := elemType.Underlying().(*types.Array); isArray {
			return fmt.Sprintf("new(%s)", v.arrayZeroValueArgs(strconv.FormatInt(innerArray.Len(), 10), innerArray))
		}
	}

	// Mirrors the zero-value construction the local/global variable paths already emit for these
	// struct shapes (a promoted embed's readonly `ж<T>` box exists only when a constructor runs).
	if v.structHasPromotedEmbeds(elemType) {
		return "new(nil)"
	}

	if v.structZeroValueNeedsConstruction(elemType) {
		return "new()"
	}

	return ""
}

// isSimpleIdentExpr reports whether an expression is a bare identifier (not a selector, index,
// star, etc.), so a single-identifier array element keeps its written name in the GoType attribute.
func isSimpleIdentExpr(expr ast.Expr) bool {
	_, ok := expr.(*ast.Ident)
	return ok
}

// canonicalizeQualifierRename strips a file-local import collision-rename (Δ) from the LEADING
// namespace segment of a fully-qualified type name destined for a GoType descriptor: the
// generated partial that consumes the descriptor has no file-local using aliases, so the
// segment must be the CANONICAL package qualifier (`IoLike.FsLike_package.Info` roots through
// the go namespace), never the renamed alias (`ΔIoLike.…` resolves nowhere in the .g.cs).
// Mirrors the visitTypeSpec global-using-target un-rename; a Δ-renamed TYPE segment is left
// untouched — only an entry the import-rename map produced is reverted.
func canonicalizeQualifierRename(typeName string) string {
	if seg, rest, found := strings.Cut(typeName, "."); found {
		if canonical, wasRenamed := strings.CutPrefix(seg, ShadowVarMarker); wasRenamed && packageImportAliasRenames[canonical] == seg {
			return canonical + "." + rest
		}
	}

	return typeName
}
