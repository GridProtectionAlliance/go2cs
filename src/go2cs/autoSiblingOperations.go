package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"strings"
)

// emitAutoConversionSiblings converts each manually-converted (marker-skipped) source file to a
// NON-COMPILED `<name>.cs.auto` sibling written beside the hand-owned `<name>.cs`, so upgrade-time
// review (a new Go version vs the hand-owned C#) has an auto-converted reference to diff against.
// Without this, a `[module: go.GoManualConversion]` whole-file rewrite leaves NO auto output at all
// (containsManualConversionMarker drops the source .go from the convert set entirely).
//
// Side-effect safety: this pass MUST run as the LAST step of a package's conversion — after the
// normal .cs set, the .csproj, package_info.cs, package_init.cs, and dynamic-marker resolution are
// all written. At that point every package-level global the isolated conversions below mutate is
// dead state (processConversion resets all of it at the top of the next package iteration), so the
// pass can reuse the normal analysis + visitor pipeline verbatim and still add ONLY `.cs.auto`
// files — no other emitted byte changes. The whole-package analyses (name collision, capture-mode
// methods, typespec RHS, moved init vars, publicized types) already include marked files' syntax
// during normal conversion, so the sibling output reflects the same package-level decisions.
//
// The `.cs.auto` extension is invisible to the generated projects: csproj-template.xml compiles
// `<Compile Include="*.cs" />` only, which cannot match a file name ending in `.auto`.
//
// Scope: package (directory) conversions only — in single-file mode the manual-conversion gate is
// not effective (the destination probe path never matches), so there is nothing to sibling. A
// marked file whose conversion panics is skipped with a warning (deterministically), matching the
// normal conversion path; a package the converter never queues (e.g. `unsafe`, compiler-intrinsic)
// gets no sibling because its files are never visited at all.
func emitAutoConversionSiblings(markedFiles []FileEntry, fset *token.FileSet, packageTypes *types.Package,
	info *types.Info, globalIdentNames map[*ast.Ident]string, globalScope map[string]*types.Var,
	packageOutputPath string, options Options) {
	if len(markedFiles) == 0 {
		return
	}

	// Mirror the per-file analysis pipeline the normal conversion runs over its convert set.
	// All package-level state these passes mutate is dead after the package's artifacts are
	// written (see the side-effect note above), so no snapshot/restore is needed.
	for _, fileEntry := range markedFiles {
		performGlobalVariableAnalysis(fileEntry.file.Decls, info, globalIdentNames, globalScope)
	}

	performEscapeAnalysis(markedFiles, fset, packageTypes, info)
	collectAddressedGlobals(markedFiles, packageTypes, info)
	computeImportAliasRenames(markedFiles, packageTypes, packageNamespace)
	preloadImportedTypeAliases(markedFiles, options)

	var autoFileNames []string

	// Convert sequentially in markedFiles (pkg.Syntax) order — deterministic, like the normal path.
	for _, fileEntry := range markedFiles {
		func(fileEntry FileEntry) {
			defer func() {
				if !options.debugMode {
					if r := recover(); r != nil {
						showWarning("visit file error: %v in \"%s\" (auto-conversion sibling skipped)", r, filepath.Base(fileEntry.filePath))
					}
				}
			}()

			visitor := &Visitor{
				fset:                      fset,
				pkg:                       packageTypes,
				info:                      info,
				targetFile:                &strings.Builder{},
				liftedTypeNames:           HashSet[string]{},
				liftedTypeMap:             map[types.Type]string{},
				subStructTypes:            map[types.Type][]types.Type{},
				packageImports:            &strings.Builder{},
				requiredUsings:            HashSet[string]{},
				importQueue:               HashSet[string]{},
				referencedForeignPackages: HashSet[string]{},
				canonicalAliasImported:    HashSet[string]{},
				importAliasesEmitted:      HashSet[string]{},
				importPathAliases:         map[string]string{},
				typeAliasDeclarations:     &strings.Builder{},
				standAloneComments:        map[token.Pos]string{},
				sortedCommentPos:          []token.Pos{},
				processedComments:         HashSet[token.Pos]{},
				newline:                   "\r\n",
				options:                   options,
				globalIdentNames:          globalIdentNames,
				globalScope:               globalScope,
				blocks:                    Stack[*strings.Builder]{},
				identEscapesHeap:          fileEntry.identEscapesHeap,
				sstringEligible:           fileEntry.sstringEligible,
				sstringConvExprs:          fileEntry.sstringConvExprs,
			}

			visitor.visitFile(fileEntry.file)

			baseName := strings.TrimSuffix(filepath.Base(fileEntry.filePath), ".go")
			autoFileName := filepath.Join(packageOutputPath, baseName+".cs.auto")

			if err := writeAutoConversionSibling(autoFileName, baseName, visitor.targetFile.String()); err != nil {
				showWarning("%s", err)
				return
			}

			autoFileNames = append(autoFileNames, autoFileName)
		}(fileEntry)
	}

	// Resolve any deferred dynamic (anonymous struct) type markers in the sibling output using
	// the package registry, which now also contains the siblings' own lifted names.
	resolveDynamicTypeMarkers(autoFileNames)
}

// writeAutoConversionSibling writes the `.cs.auto` content prefixed with a stable banner that
// identifies the file as regenerated, non-compiled reference output (no timestamps — the sibling
// must stay byte-deterministic across runs).
func writeAutoConversionSibling(autoFileName string, baseName string, content string) error {
	banner := "// <auto-generated>" + "\r\n" +
		"//     go2cs auto-conversion of " + baseName + ".go — REFERENCE ONLY. The compiled " + baseName + ".cs is a manual" + "\r\n" +
		"//     conversion (marked go.GoManualConversion); this sibling is NOT compiled (projects include only" + "\r\n" +
		"//     *.cs) and is regenerated on every conversion for upgrade-time diff review. Do not edit." + "\r\n" +
		"// </auto-generated>" + "\r\n" + "\r\n"

	autoFile, err := os.Create(autoFileName)

	if err != nil {
		return fmt.Errorf("failed to create auto-conversion sibling file \"%s\": %s", autoFileName, err)
	}

	defer autoFile.Close()

	if _, err = autoFile.WriteString(banner + content); err != nil {
		return fmt.Errorf("failed to write to auto-conversion sibling file \"%s\": %s", autoFileName, err)
	}

	return nil
}
