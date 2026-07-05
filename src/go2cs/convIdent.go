package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

func (v *Visitor) convIdent(ident *ast.Ident, context IdentContext) string {
	// A package qualifier (`runtime.Goexit()`) renders its using alias, which is
	// collision-renamed when a same-named child namespace is visible from the import
	// closure (CS0576 — see importAliasOperations.go).
	if pkgName, isPkg := v.identifierIsPackageName(ident); isPkg {
		if renamed, ok := packageImportAliasRenames[pkgName]; ok {
			return getSanitizedImport(renamed)
		}
	}

	if ident.Name == "nil" {
		if context.isPointer {
			return "nil"
		}

		return "default!"
	}

	// `true`/`false` are C# KEYWORDS but Go PREDECLARED identifiers (universe scope). A VALUE
	// use resolves to that universe-scope const — emit the bare C# literal. A shadowing
	// VARIABLE/parameter named `true`/`false` (text/template/parse's `newBool(pos Pos, true
	// bool)`) has a package-scoped object and falls through to the escaped `@true`/`@false`
	// below (`true`/`false` are in the keyword-escape set).
	if ident.Name == "true" || ident.Name == "false" {
		if c, ok := v.info.ObjectOf(ident).(*types.Const); ok && c.Pkg() == nil {
			return ident.Name
		}
	}

	if context.isPointer {
		// Check if the identifier is an unsafe pointer
		if basic, ok := v.getIdentType(ident).(*types.Basic); ok && basic.Kind() == types.UnsafePointer {
			// Sanitize the name so a C# keyword identifier (e.g. an `unsafe.Pointer`
			// parameter named `new`, as in internal/runtime/atomic) is escaped to `@new`
			// rather than emitted bare, which C# parses as the `new` operator (CS1526).
			return fmt.Sprintf("%s.Value", getSanitizedIdentifier(v.getIdentName(ident)))
		}

		// A direct-ж method's receiver used as a bare pointer value (e.g. `recv.field = recv`):
		// the receiver box is the parameter `Ꮡrecv`, so emit that. A value-ref receiver
		// (`this ref T recv`) has no box and its deref'd value cannot stand in for the pointer
		// — this is why such methods are marked direct-ж (see packageDirectBoxReceiverMethods).
		if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && ident.Name == recvName && isDirectBoxReceiverMethod(v.currentFuncDecl, v.info) {
			return AddressPrefix + strings.TrimPrefix(v.getIdentName(ident), "@")
		}

		var identEscapesHeap bool
		obj := v.info.ObjectOf(ident)

		if obj != nil {
			identEscapesHeap = v.identEscapesHeap[obj]
		}

		identType := v.getIdentType(ident)

		// The current method's REF receiver has NO box — only a direct-ж method carries
		// Ꮡrecv (handled above), and receivers are never heap-decl'd. An ESCAPE-MARKED
		// receiver must not take the heap-box render below: flate init's
		// `d.fill = (*compressor).fillStore` emitted a nonexistent Ꮡd (CS0103 ×11).
		isCurrentRefReceiver := false

		if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && ident.Name == recvName {
			isCurrentRefReceiver = true
		}

		// Check if the identifier is not already a pointer type or is a parameter or escapes heap,
		// in these cases, we need to add the address operator to reference the pointer variable.
		// The box keeps the RAW Go name (`Ꮡp`), even when the value alias is collision-renamed
		// (`Δp`) — an escaping local is `ref var Δp = ref heap(new T(), out var Ꮡp)`, a deref'd
		// pointer param `ref var Δp = ref Ꮡp.Value` — so reference it by the raw name, not `ᏑΔp`
		// (not in scope → CS0103). boxBaseName is a no-op when nothing is shadow-renamed (no churn).
		// NOTE: this arm renders the ident's POINTER-form for a VALUE-type local (its box) — an
		// inherently heap-allocated local stays on the plain render even when it now owns an
		// address-taken box (identHasHeapBox): here the pointer VALUE is the ident itself
		// (`new Middle(Inner: inner)`), and `Ꮡinner` (the ж<ж<T>> box) is only what an explicit
		// `&inner` wants — that renders through convUnaryExpr.
		if !isCurrentRefReceiver {
			if _, ok := identType.(*types.Pointer); !ok || v.identIsParameter(ident) || (identEscapesHeap && !isInherentlyHeapAllocatedType(identType)) {
				return AddressPrefix + v.boxBaseName(ident)
			}
		}
	}

	if context.isType {
		// A reference to a function-local (lifted) named type must use the lifted C# name
		// (`makePoint_point`), not the bare Go name — e.g. a composite literal `point{…}` or a
		// cast `point(x)`. Only lifted/anonymous types are in liftedTypeMap, so package-level
		// types are unaffected.
		if identType := v.getIdentType(ident); identType != nil {
			if liftedName, ok := v.liftedTypeMap[identType]; ok {
				return liftedName
			}
		}

		return convertToCSTypeName(v.getIdentName(ident))
	}

	if context.isMethod {
		var name string

		// A FIELD selection must not take the function-name Main→ΔMain special (the C#
		// entry-point reservation applies to METHODS): runtime/debug's BuildInfo.Main
		// field access emitted `bi.ΔMain` against the raw `Main` declaration (CS1061 ×2).
		// Fields otherwise share the function-name path (core sanitize, no package-level
		// nameCollisions Δ — a field is struct-scoped and declared unrenamed).
		if context.isField {
			name = getCoreSanitizedIdentifier(v.getIdentName(ident))
		} else {
			name = getSanitizedFunctionName(v.getIdentName(ident))
		}

		// A field whose name equals its enclosing struct's type name is renamed with the
		// disambiguation marker (CS0542); match the renamed declaration at the access site.
		if context.fieldCollidesWithType {
			name = typeCollidingFieldName(name)
		}

		return name
	}

	// A heap-boxed local captured by-box inside a lambda is read through its box: the ref-local
	// alias `ref var m = ref Ꮡm.Value` can't be captured by the closure (CS8175), so a value use must
	// deref the box directly (`Ꮡm.Value`). The box `Ꮡm` is a capturable reference. Address uses
	// (`&m`, `&m.field`) are rendered from the box name in convUnaryExpr, bypassing this rewrite.
	if v.lambdaCapture != nil && v.lambdaCapture.conversionInLambda && v.isLambdaBoxRefVar(v.info.ObjectOf(ident)) {
		// The current method's REF receiver has NO box — a genuine closure capture would
		// have promoted the method direct-ж (bodyCapturesReceiverInClosure); reaching here
		// as a ref receiver means a PSEUDO-lambda conversion context (a method-value assign)
		// touched an escape-marked receiver, and the box render is a nonexistent name
		// (flate init's `d.fill = (*compressor).fillStore`, CS0103 ×11).
		isRefReceiver := false

		if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && ident.Name == recvName {
			isRefReceiver = !isDirectBoxReceiverMethod(v.currentFuncDecl, v.info)
		}

		if !isRefReceiver {
			// For a box-of-POINTER (or other inherently-heap) local — `Ꮡm` is a `ж<ж<T>>` — reading the box
			// is reading the HELD pointer value, not a dereference of it, so it must use `.ValueSlot` (no
			// nil-pointer-dereference check): in Go reading `*(&p)` for a nil `*T`/slice/map yields the nil
			// value, no panic. The box of a value-struct local (`ж<box>`) or a deref'd pointer PARAMETER
			// (`ж<pointed-to-T>`) is a genuine dereference, so it keeps the strict `.Value`.
			valAccessor := ".Value"
			if v.isBoxedPointerLocal(ident) {
				valAccessor = ".ValueSlot"
			}
			return AddressPrefix + v.boxBaseName(ident) + valAccessor
		}
	}

	// A same-package GLOBAL variable reference shadowed by a same-named function-level LOCAL: C# locals
	// are function-scoped, so the bare global name binds to the local everywhere in the function — a use
	// of the global BEFORE the local's declaration is CS0841 (and would read the wrong variable
	// regardless). Runtime `traceallocfree.traceSnapshotMemory` reads the global `trace.minPageHeapAddr`
	// before declaring the local `trace := traceAcquire()`; both are collision-renamed `Δtrace`. Qualify
	// the global with its package static class (`runtime_package.Δtrace`), which a local can never shadow.
	// Gated to an ident that resolves to a package-level var of THIS package with a same-named
	// function-level local — so an ordinary global (no shadowing local) and the local's own uses (which
	// resolve to the local, not the package scope) are untouched.
	if v.funcLevelDecls != nil {
		if _, shadowed := v.funcLevelDecls[ident.Name]; shadowed && v.pkg != nil {
			if vr, ok := v.info.ObjectOf(ident).(*types.Var); ok && vr.Parent() == v.pkg.Scope() {
				return getSanitizedImport(packageName+PackageSuffix) + "." + getSanitizedIdentifier(v.getIdentName(ident))
			}
		}
	}

	// A PACKAGE ident whose using-alias is shadowed by a same-package method/function name
	// (`func (s *byLiteral) sort(…)` vs `import "sort"` — the member lookup binds the method
	// group before the alias, CS0119, compress/flate) qualifies through the _package class.
	if packageFuncMethodNames != nil && packageFuncMethodNames[ident.Name] {
		if pkgName, ok := v.info.ObjectOf(ident).(*types.PkgName); ok {
			return rootQualifyIfAmbiguous(convertImportPathToNamespace(pkgName.Imported().Path(), PackageSuffix))
		}
	}

	return getSanitizedIdentifier(v.getIdentName(ident))
}
