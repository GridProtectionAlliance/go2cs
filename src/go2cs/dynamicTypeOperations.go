// dynamicTypeOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"encoding/hex"
	"go/ast"
	"go/types"
	"os"
	"strings"
)

// Sentinels wrapping a deferred dynamic-type-name reference, resolved to the lifted
// C# type name after all files in the package have been visited (see
// resolveDynamicTypeMarkers). The guillemets do not occur in generated C#, so the
// marker is unambiguous.
const (
	dynamicTypeMarkerPrefix = "«DYNTYPE:"
	dynamicTypeMarkerSuffix = ":DYNTYPE»"
)

// dynamicTypeMarker returns the deferred marker for a signature. The payload is the
// HEX-ENCODED signature, not the raw text: a rendered type name flows through string
// transformation passes before it reaches the output file (convertToCSTypeName rewrites
// every `[`/`]` to `<`/`>`, getAliasedTypeName splits on `.`, …), and raw Go type text
// like `struct{a []byte; …}` would be corrupted in transit so the post-barrier
// resolution could never match it back to the registry. Hex digits pass through every
// transform untouched. The encoding is a pure function, so equal signatures yield the
// identical marker — string comparisons on rendered names behave exactly as signature
// comparisons.
func dynamicTypeMarker(signature string) string {
	return dynamicTypeMarkerPrefix + hex.EncodeToString([]byte(signature)) + dynamicTypeMarkerSuffix
}

// dynamicTypeMarkerSignature decodes a marker payload back to the structural signature.
func dynamicTypeMarkerSignature(payload string) (string, bool) {
	signature, err := hex.DecodeString(payload)

	if err != nil {
		return "", false
	}

	return string(signature), true
}

// deferredDynamicTypeName renders a NON-EMPTY anonymous struct/interface type that no
// per-file lifted name resolved: the shared package registry's lifted name when the
// declaring file has already been visited (file visits run in deterministic sorted-file
// order), else a deferred marker resolved after the file-visit barrier. Returns "" for
// any other type — including the EMPTY struct/interface, whose raw `struct{}`/`interface{}`
// signatures already map to `EmptyStruct`/`any` downstream.
func deferredDynamicTypeName(t types.Type) string {
	switch typ := t.(type) {
	case *types.Struct:
		if isEmptyStructType(typ) {
			return ""
		}
	case *types.Interface:
		if typ.Empty() {
			return ""
		}
	default:
		return ""
	}

	signature := t.String()

	if name := lookupDynamicTypeName(signature); name != "" {
		return name
	}

	return dynamicTypeMarker(signature)
}

// registerDynamicTypeName records the lifted C# name for a package-level
// anonymous struct/interface type, keyed by its structural signature, so other
// files in the same package can resolve cross-file references to it.
func registerDynamicTypeName(signature, csTypeName string) {
	packageLock.Lock()

	// Deterministic winner when one signature registers from multiple files/functions
	// (file visits are concurrent, so last-wins would vary run to run — the converter is
	// byte-deterministic): keep the lexically smallest name. Every registrant is a lifted
	// file-level type of the same package, so any winner resolves.
	if existing, ok := packageDynamicTypeNames[signature]; !ok || csTypeName < existing {
		packageDynamicTypeNames[signature] = csTypeName
	}

	packageLock.Unlock()
}

// lookupDynamicTypeName returns the lifted C# name registered for a signature, or
// "" if none is registered yet (the declaring file may not have been visited).
func lookupDynamicTypeName(signature string) string {
	packageLock.Lock()
	name := packageDynamicTypeNames[signature]
	packageLock.Unlock()
	return name
}

// dynamicStructTypeName resolves the C# type name of an expression whose type is
// a (possibly anonymous) struct, for use where a concrete type name is required —
// e.g. the `ж.of(StructType.ᏑField)` address-of-field form. It prefers this
// visitor's per-file lifted name, falls back to the shared package registry, and
// otherwise emits a marker resolved after the file-visit barrier.
func (v *Visitor) dynamicStructTypeName(expr ast.Expr) string {
	t := v.getType(expr, false)

	if t != nil {
		if name, ok := v.liftedTypeMap[t]; ok {
			return name
		}

		signature := t.String()

		if name := lookupDynamicTypeName(signature); name != "" {
			return name
		}

		// A non-empty anonymous struct lifted in another file of this package:
		// defer resolution until the shared registry is fully populated.
		if structType, ok := t.(*types.Struct); ok && !isEmptyStructType(structType) {
			return dynamicTypeMarker(signature)
		}
	}

	// Concrete/named or otherwise resolvable type: use the normal path.
	return v.getExprTypeName(expr, false)
}

// resolveDynamicTypeMarkers rewrites any deferred dynamic-type markers in the
// given output files using the now-complete package registry. Called once after
// the concurrent file-visit barrier. Unresolved markers (genuinely unknown types)
// are replaced with the raw signature and a warning, preserving prior behavior.
func resolveDynamicTypeMarkers(outputFileNames []string) {
	for _, fileName := range outputFileNames {
		contentBytes, err := os.ReadFile(fileName)

		if err != nil {
			continue
		}

		content := string(contentBytes)

		if !strings.Contains(content, dynamicTypeMarkerPrefix) {
			continue
		}

		for {
			start := strings.Index(content, dynamicTypeMarkerPrefix)

			if start == -1 {
				break
			}

			end := strings.Index(content[start:], dynamicTypeMarkerSuffix)

			if end == -1 {
				break
			}

			end += start
			signature, _ := dynamicTypeMarkerSignature(content[start+len(dynamicTypeMarkerPrefix) : end])
			marker := content[start : end+len(dynamicTypeMarkerSuffix)]

			replacement := lookupDynamicTypeName(signature)

			if replacement == "" {
				showWarning("Unresolved dynamic struct type: %s", signature)
				replacement = signature
			}

			content = strings.ReplaceAll(content, marker, replacement)
		}

		if err := os.WriteFile(fileName, []byte(content), 0644); err != nil {
			showWarning("Failed to resolve dynamic type markers in \"%s\": %s", fileName, err)
		}
	}
}
