package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

func (v *Visitor) visitTypeSpec(typeSpec *ast.TypeSpec, doc *ast.CommentGroup) {
	name := v.getIdentName(typeSpec.Name)
	identType := v.getIdentType(typeSpec.Name)

	// Handle type alias
	if typeSpec.Assign.IsValid() {
		// Get types.Type from typeSpec.Type expr
		typeSpecType := v.info.TypeOf(typeSpec.Type)

		if typeSpecType == nil {
			panic(fmt.Sprintf("@visitTypeSpec - Failed to get type for type alias %s", name))
		}

		var usePackagePrefix bool

		// Check if the aliased type is a struct or pointer to a struct
		if structType, exprType := v.extractStructType(typeSpec.Type); structType != nil && !v.liftedTypeExists(structType) {
			if v.inFunction {
				v.indentLevel++
			}

			v.visitStructType(structType, exprType, name, doc, true, nil)

			if v.inFunction {
				v.indentLevel--
			}

			usePackagePrefix = true
		}

		// Check if the aliased type is an anonymous interface
		if interfaceType, exprType := v.extractInterfaceType(typeSpec.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
			if v.inFunction {
				v.indentLevel++
			}

			v.visitInterfaceType(interfaceType, exprType, name, doc, true, nil)

			if v.inFunction {
				v.indentLevel--
			}

			usePackagePrefix = true
		}

		// A type alias to a SAME-PACKAGE named type (`type alias = Inner`) must qualify the target
		// with the package class — the `global using` sits at namespace scope, outside the class, so
		// a bare `go.Inner` does not resolve (Inner is `go.main_package.Inner`). Cross-package targets
		// already carry their own qualification, so only same-package named targets need this.
		if !usePackagePrefix {
			if named, ok := types.Unalias(typeSpecType).(*types.Named); ok {
				if obj := named.Obj(); obj != nil && obj.Pkg() == v.pkg {
					usePackagePrefix = true
				}
			}
		}

		var typeNamePrefix string

		if usePackagePrefix {
			typeNamePrefix = getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix)) + "/"
		}

		typeName := convertToCSFullTypeName(typeNamePrefix + v.getFullTypeName(typeSpecType, false))

		v.typeAliasDeclarations.WriteString(fmt.Sprintf("global using %s = %s;%s", name, typeName, v.newline))

		// Add exported type aliases to package info
		if getAccess(name) == "public" {
			packageLock.Lock()
			exportedTypeAliases[name] = typeName
			packageLock.Unlock()
		}

		return
	}

	// A manually-converted type (see manualTypeOperations.go) emits only a marker comment; the
	// package's *_impl.cs declares the type. Both its plain and collision-renamed forms are
	// recorded (a type-vs-method collision Δ-prefixes the TYPE — guintptr → Δguintptr) so the
	// GoImplicitConv attribute emission can skip conversions referencing either rendering.
	if v.isManualType(typeSpec.Name.Name) {
		packageLock.Lock()
		packageManualTypeNames[name] = true
		packageManualTypeNames[getSanitizedIdentifier(typeSpec.Name.Name)] = true
		packageLock.Unlock()

		if !v.inFunction {
			v.targetFile.WriteString(v.newline)
		}

		v.writeOutput("// type %s is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])", name)
		v.targetFile.WriteString(v.newline)
		return
	}

	// An unexported type used as an exported struct field must be emitted as public (CS0051/
	// CS0052). Set the access modifier for the type-kind emitter below to consume.
	if v.isPublicizedType(typeSpec.Name) {
		v.pendingTypeAccess = "public "
	}

	defer func() { v.pendingTypeAccess = "" }()

	switch typeSpecType := typeSpec.Type.(type) {
	case *ast.ArrayType:
		v.visitArrayType(typeSpecType, identType, name, typeSpec.Comment)
	case *ast.ChanType:
		v.visitChanType(typeSpecType)
	case *ast.FuncType:
		v.visitFuncType(typeSpecType, identType, name)
	case *ast.Ident:
		v.visitIdent(typeSpecType, identType, name, v.inFunction)
	case *ast.InterfaceType:
		v.visitInterfaceType(typeSpecType, v.info.Defs[typeSpec.Name].Type(), name, doc, v.inFunction, nil)
	case *ast.MapType:
		v.visitMapType(typeSpecType)
	case *ast.ParenExpr:
		v.targetFile.WriteString(v.convParenExpr(typeSpecType, DefaultLambdaContext()))
	case *ast.SelectorExpr:
		// A DEFINED type over a cross-package named type (`type stdFunction unsafe.Pointer`,
		// `type goroutineProfileStateHolder atomic.Uint32`). Emit an inherited `[GoType]` wrapper of
		// the NAMED type (go2cs-gen's InheritedTypeTemplate wraps a plain type-name definition);
		// writing the bare selector text alone is an orphan type reference (CS1585). Use the named
		// type, not its underlying (which may expose unexported cross-package fields → CS0246).
		if rhsType := v.info.TypeOf(typeSpecType); rhsType != nil {
			csName := convertToCSTypeName(v.getFullTypeName(rhsType, false))

			// The GoType attribute is consumed by the generated `<X>.g.cs`, which has no file-local
			// `using` aliases. unsafe.Pointer (a *types.Basic, a C# keyword) renders via the
			// `@unsafe` alias; rewrite it to the alias-free package class so it resolves there.
			csName = strings.ReplaceAll(csName, "@unsafe.", "unsafe_package.")

			access := v.pendingTypeAccess
			v.pendingTypeAccess = ""

			if !v.inFunction {
				v.targetFile.WriteString(v.newline)
			}

			v.writeOutput("[GoType(\"%s\")] %spartial struct %s;", csName, access, getSanitizedIdentifier(name))
			v.targetFile.WriteString(v.newline)
		} else {
			v.targetFile.WriteString(v.convSelectorExpr(typeSpecType, DefaultLambdaContext()))
		}
	case *ast.StarExpr:
		v.targetFile.WriteString(v.convStarExpr(typeSpecType, DefaultStarExprContext()))
	case *ast.StructType:
		v.visitStructType(typeSpecType, v.info.Defs[typeSpec.Name].Type(), name, doc, v.inFunction, nil)
	default:
		panic(fmt.Sprintf("@visitTypeSpec - Unexpected TypeSpec type: %#v", v.getPrintedNode(typeSpecType)))
	}
}
