package main

import (
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
	constNames := make(map[string]bool)
	methodNames := make(map[string]bool)

	// Collect all const/var names and method names
	for _, file := range pkg.Syntax {
		ast.Inspect(file, func(n ast.Node) bool {
			switch node := n.(type) {
			case *ast.GenDecl:
				if node.Tok == token.CONST || node.Tok == token.VAR {
					for _, spec := range node.Specs {
						if valueSpec, ok := spec.(*ast.ValueSpec); ok {
							for _, name := range valueSpec.Names {
								constNames[name.Name] = true
							}
						}
					}
				}

			case *ast.FuncDecl:
				methodNames[node.Name.Name] = true
			}
			return true
		})
	}

	// Find collisions (names that appear in both sets)
	for name := range constNames {
		if methodNames[name] {
			// Found a collision
			nameCollisions[name] = true

			// Add collision avoidance name as a type aliases to package info,
			// this way original name can be referenced as normal when using
			// the name from referenced package. The name will not collide in
			// a remote package because the type will have the package prefix.
			if getAccess(name) == "public" {
				packageLock.Lock()
				exportedTypeAliases[getCoreSanitizedIdentifier(name)] = getCollisionAvoidanceIdentifier(name)
				packageLock.Unlock()
			}
		}
	}
}
