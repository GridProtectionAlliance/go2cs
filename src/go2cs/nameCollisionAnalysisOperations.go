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

// emitterSpelledTypeNames are type names the converter's EMITTER spells independent of the Go
// source's own spelling — so a user-declared package-level TYPE of the same name must be
// package-scoped Δ-renamed (nameCollisions), never '@'-escaped or globally `reserved` (both
// would corrupt the emitter's legitimate spellings in every other package). Proven vectors:
// `any` — the `interface{}` rendering (`slice<any>` bound the user struct, CS0029 ×3);
// `rune` — the untyped rune-constant default (`c := 'x'` emits `rune c = 'x';`, CS0030);
// `nint`/`nuint` — the Go int/uint MAPPED spellings and C# native-int contextual keywords
// (`partial struct nint { internal nint d; }` is a CS0523 layout cycle, and '@' cannot fix a
// name-identity problem).
var emitterSpelledTypeNames = map[string]bool{
	"any": true, "rune": true, "nint": true, "nuint": true,
}

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
								// The blank identifier is a discard, never referenced, and the
								// value-spec visitor gives each blank a unique name — so a `_`
								// const/var must not be treated as colliding with a `func _()`
								// (a common stringer compile-time assertion). Otherwise every
								// `_` is Δ-prefixed to the same `Δ_` and they collide (CS0102).
								if name.Name == "_" {
									continue
								}

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

	// A package method/function whose name is a Go built-in (`func (b *pageBits) clear()`) shadows
	// the using-static `go.builtin.<name>` for an unqualified free call in C#, so such built-in calls
	// must be emitted qualified (`builtin.<name>(…)`). Record which built-ins this package shadows.
	packageBuiltinShadows = make(map[string]bool)

	for name := range methodNames {
		// `recover` is NEVER qualified to `builtin.recover`: golib has no such static — a Go
		// `recover()` builtin call is emitted as the func() execution-context lambda PARAMETER
		// `recover` (visitFuncDecl names it), which is always in scope wherever recover is legal
		// and correctly shadows the same-named package method's extension. Qualifying it would
		// bind to the nonexistent `builtin.recover` and fall back to the method (text/template/
		// parse's `func (t *Tree) recover(errp *error)`, CS0815/CS7036).
		if name == "recover" {
			continue
		}

		if goBuiltinNames[name] {
			packageBuiltinShadows[name] = true
		}
	}

	// A package-level TYPE named after a spelling the EMITTER produces on its own (`any`,
	// `rune`, `nint`, `nuint` — see emitterSpelledTypeNames) shadows that spelling inside the
	// package class, breaking the converter's own emissions (`slice<any>` bound the user
	// struct, CS0029 ×3; `internal nint d;` inside `partial struct nint` is a CS0523 cycle).
	// Δ-rename every ident named it in THIS package (nameCollisions is package-scoped),
	// keeping the bare name bound to its emitter meaning. These names must never go in the
	// string-based `reserved` set: that would corrupt the emitter's legitimate spellings in
	// every OTHER package (see the comment on that set).
	for name, isType := range namedElementNames {
		if isType && emitterSpelledTypeNames[name] {
			nameCollisions[name] = true
		}
	}

	// A method/function name can also shadow an IMPORTED PACKAGE's using-alias inside the
	// package class (`func (s *byLiteral) sort(…)` vs `import "sort"` — `sort.Sort(…)`
	// bound the method group, CS0119, compress/flate). Record every method/function name;
	// the package-ident emission qualifies through the _package class when shadowed.
	packageFuncMethodNames = make(map[string]bool)

	for name := range methodNames {
		packageFuncMethodNames[name] = true
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
