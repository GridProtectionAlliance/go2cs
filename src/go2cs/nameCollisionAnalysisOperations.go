package main

import (
	"fmt"
	"go/ast"
	"go/token"

	"golang.org/x/tools/go/packages"
)

// The `performNameCollisionAnalysis` function analyzes the package for name collisions
// between constants/variables and method names. Resulting collisions are stored in the
// global `nameCollisions` map. This function is called for each package during the
// conversion process to ensure that any potential name collisions are identified and
// handled appropriately. This is important to avoid naming conflicts that could lead
// to runtime errors or unexpected behavior in the generated C# code, which is more
// strict about unique naming of discrete types than Go is in this case.

func performNameCollisionAnalysis(pkg *packages.Package) {
	// Track names of various declarations
	namedElementNames := make(map[string]bool)
	methodNames := make(map[string]bool)

	// Collect all named element names and method names (top-level declarations only)
	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			switch node := decl.(type) {
			case *ast.GenDecl:
				// Handle constants and variables at package level (not inside functions)
				if node.Tok == token.CONST || node.Tok == token.VAR {
					for _, spec := range node.Specs {
						if valueSpec, ok := spec.(*ast.ValueSpec); ok {
							for _, name := range valueSpec.Names {
								namedElementNames[name.Name] = false
							}
						}
					}
				}

				// Handle type declarations (structs, interfaces, type aliases)
				if node.Tok == token.TYPE {
					for _, spec := range node.Specs {
						if typeSpec, ok := spec.(*ast.TypeSpec); ok {
							if !typeSpec.Assign.IsValid() {
								namedElementNames[typeSpec.Name.Name] = true
							}
						}
					}
				}

			case *ast.FuncDecl:
				methodNames[node.Name.Name] = true
			}
		}
	}

	// Find collisions (names that appear in both sets)
	for name, isType := range namedElementNames {
		if methodNames[name] {
			// Found a collision
			nameCollisions[name] = true

			// Add collision avoidance name as a type aliases to package info,
			// this way original name can be referenced as normal when using
			// the name from referenced package. The name will not collide in
			// a remote package because the type will have the package prefix.
			if getAccess(name) == "public" {
				var typePrefix string

				if !isType {
					typePrefix = "const:"
				}

				packageLock.Lock()
				exportedTypeAliases[getCoreSanitizedIdentifier(name)] = fmt.Sprintf("%s%s", typePrefix, getCollisionAvoidanceIdentifier(name))
				packageLock.Unlock()
			}
		}
	}
}
