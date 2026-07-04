package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"slices"
	"strings"
)

const InterfaceTypeAttributeMarker = ">>MARKER:INTERFACE_TYPE_ATTRS<<"
const InterfacePostAtributeMarker = ">>MARKER:POST_INTERFACE_ATTRS<<"
const InterfaceInheritanceMarker = ">>MARKER:INHERITED_INTERFACES<<"

// For interface types with generic constraints, we will be adding a C# type parameter to the
// converted Go interface to handle operators. Since methods in interfaces can have their own
// type constraints, we mark the type so that it will not conflict with generic method types
const TypeT = ShadowVarMarker + "T"

// Handles interface types in context of a TypeSpec
func (v *Visitor) visitInterfaceType(interfaceType *ast.InterfaceType, identType types.Type, name string, doc *ast.CommentGroup, lifted bool, target *strings.Builder) (interfaceTypeName string) {
	for _, field := range interfaceType.Methods.List {
		// Check if this is an actual method (has a function type)
		if funcType, ok := field.Type.(*ast.FuncType); ok {
			var indentOffset int

			if v.inFunction {
				indentOffset = 1
			} else {
				indentOffset = -1
			}

			// Loop through function results to check if any are structs
			if funcType.Results != nil {
				for index, resultField := range funcType.Results.List {
					var fieldName string

					if resultField.Names == nil {
						fieldName = fmt.Sprintf("%sR%d", name, index)
					} else {
						fieldName = fmt.Sprintf("%s_%s", name, resultField.Names[0].Name)
					}

					// Check if the return type is a struct or pointer to a struct
					if structType, exprType := v.extractStructType(resultField.Type); structType != nil && !v.liftedTypeExists(structType) {
						v.indentLevel += indentOffset
						v.visitStructType(structType, exprType, fieldName, resultField.Comment, true, target)
						v.indentLevel -= indentOffset
					}

					// Check if the return type is an anonymous interface
					if interfaceType, exprType := v.extractInterfaceType(resultField.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
						v.indentLevel += indentOffset
						v.visitInterfaceType(interfaceType, exprType, fieldName, resultField.Comment, true, target)
						v.indentLevel -= indentOffset
					}
				}
			}

			// Loop through function parameters to check if any are structs
			if funcType.Params != nil {
				for _, paramField := range funcType.Params.List {
					for _, paramName := range paramField.Names {
						// Check if the parameter type is a struct or pointer to a struct
						if structType, exprType := v.extractStructType(paramField.Type); structType != nil && !v.liftedTypeExists(structType) {
							v.indentLevel += indentOffset
							v.visitStructType(structType, exprType, fmt.Sprintf("%s_%s", name, paramName.Name), paramField.Comment, true, target)
							v.indentLevel -= indentOffset
						}

						// Check if the parameter type is an anonymous interface
						if interfaceType, exprType := v.extractInterfaceType(paramField.Type); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
							v.indentLevel += indentOffset
							v.visitInterfaceType(interfaceType, exprType, fmt.Sprintf("%s_%s", name, paramName.Name), paramField.Comment, true, target)
							v.indentLevel -= indentOffset
						}
					}
				}
			}
		}
	}

	var preLiftIndentLevel int

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

		interfaceTypeName = v.getUniqueLiftedTypeName(name)
		v.liftedTypeMap[identType] = interfaceTypeName
		v.liftedTypeMap[v.getType(interfaceType, false)] = interfaceTypeName
	} else {
		interfaceTypeName = name
	}

	if target == nil {
		target = v.targetFile

		if !v.inFunction {
			target.WriteString(v.newline)
		}
	}

	v.writeDocString(target, doc, interfaceType.Pos())

	var structuralBases []string
	var canonicalStructuralBases []string
	structuralCovered := HashSet[string]{}

	// Structural (non-embedded) satisfaction of an imported interface is emitted as C#
	// interface inheritance: Go converts fs.File to io.Reader implicitly because the method
	// set suffices, but C# interfaces are nominal, so the declaration site must carry the
	// link (os's CopyFS passes an fs.File to io.Copy — CS1503). Inheritance is
	// identity-preserving (no adapter wrapper — the dynamic value flows through type asserts)
	// and free at every downstream conversion site. Skipped for lifted/dyn interfaces
	// (reflection-implemented) and for constraint interfaces (generic machinery).
	if !lifted && identType != nil {
		hasConstraint := false

		for _, method := range interfaceType.Methods.List {
			if len(method.Names) == 0 && method.Type != nil {
				if isConstraint, _ := v.isTypeConstraint(method.Type); isConstraint {
					hasConstraint = true
					break
				}
			}
		}

		if !hasConstraint {
			structuralBases, canonicalStructuralBases, structuralCovered = v.getStructuralInterfaceBases(interfaceType, identType)
		}
	}

	result := &strings.Builder{}
	inheritedInterfaces := []string{}
	canonicalInheritedInterfaces := []string{}
	typeConstraints := HashSet[ConstraintType]{}
	var operatorSets HashSet[OperatorSet]
	outerIndent := v.indent(v.indentLevel)

	result.WriteString(outerIndent)
	result.WriteString(fmt.Sprintf("[GoType%s]%spartial interface %s%s{", InterfaceTypeAttributeMarker, InterfacePostAtributeMarker, getSanitizedIdentifier(interfaceTypeName), InterfaceInheritanceMarker))
	result.WriteString(v.newline)

	v.indentLevel++
	innerIndent := v.indent(v.indentLevel)

	for _, method := range interfaceType.Methods.List {
		if len(method.Names) == 1 {
			// A declared member covered by a structural base is inherited, not re-declared —
			// redeclaring would HIDE the base member (distinct C# members, implementers would
			// need both). Its signature is guaranteed compatible: the base was only chosen
			// because types.Implements matched the full method set.
			if structuralCovered.Contains(method.Names[0].Name) {
				continue
			}

			v.writeDocString(result, method.Doc, method.Pos())

			goMethodName := method.Names[0].Name
			csMethodName := getSanitizedFunctionName(goMethodName)
			typeLenDeviation := token.Pos(len(csMethodName) - len(goMethodName))
			methodType := v.info.ObjectOf(method.Names[0]).(*types.Func)

			if methodType == nil {
				panic("@visitInterfaceType - Failed to find interface method \"" + goMethodName + "\" in the type info")
			}

			signature := methodType.Signature()
			resultSignature := v.generateResultSignature(signature)
			parameterSignature, _ := v.generateParametersSignature(signature, false)

			typeLenDeviation += token.Pos(len(parameterSignature) - v.getSourceParameterSignatureLen(signature))
			typeLenDeviation += token.Pos(len(resultSignature) - v.getSourceResultSignatureLen(signature))

			result.WriteString(fmt.Sprintf("%s%s %s(%s);", innerIndent, resultSignature, csMethodName, parameterSignature))
			v.writeCommentString(result, method.Comment, method.Type.End()+typeLenDeviation)
			result.WriteString(v.newline)
		} else if method.Type != nil {
			if isConstraint, methodCount := v.isTypeConstraint(method.Type); isConstraint {
				// Collapse multi-line constraint unions (e.g. "~int | ... | ~string" spanning
				// several source lines) onto a single comment line; otherwise the continuation
				// lines are emitted as raw, uncompilable C# inside the interface body.
				constraintText := strings.Join(strings.Fields(v.getPrintedNode(method.Type)), " ")
				result.WriteString(fmt.Sprintf("%s//  Type constraints: %s%s", innerIndent, constraintText, v.newline))
				typeConstraints.UnionWithSet(v.getConstraintTypeSetFromExpr(method.Type))
				operatorSets = getOperatorSet(typeConstraints)
				result.WriteString(fmt.Sprintf("%s// Derived operators: %s%s", innerIndent, getOperatorSetAsString(operatorSets), v.newline))

				// If type constraint constains any methods, add it to the inherited interfaces
				if methodCount > 0 {
					inheritedInterfaces = append(inheritedInterfaces, fmt.Sprintf("%s<%s>", v.convExpr(method.Type, nil), TypeT))
				}
			} else {
				isDynamicInterface := v.isDynamicInterface(method.Type)

				if isDynamicInterface {
					v.indentLevel--
					v.removeLastLineFeed(result)
					v.removeLastLineFeed(v.targetFile)
				}

				inheritedInterfaces = append(inheritedInterfaces, v.convExpr(method.Type, nil))

				// Track the CANONICAL (full-name) render too: the duplicate-implementation
				// prune keys interfaceImplementations by getFullTypeName (a FOREIGN embed
				// records `go.io.fs_package.FileInfo`), so the alias render alone
				// (`fs.FileInfo`) never matches and both the derived and base impls emit
				// the same explicit members (zip headerFileInfo : fileInfoDirEntry +
				// fs.FileInfo, CS8646 ×6/CS0111 ×2).
				if embedType := v.getType(method.Type, false); embedType != nil {
					canonicalName := convertToCSTypeName(v.getFullTypeName(embedType, false))

					if canonicalName != "" {
						canonicalInheritedInterfaces = append(canonicalInheritedInterfaces, canonicalName)
					}
				}

				if isDynamicInterface {
					v.indentLevel++
					v.targetFile.WriteString(v.newline)
				}
			}
		} else {
			panic("@visitInterfaceType - Unexpected method declaration in interface: %s" + v.getPrintedNode(method))
		}
	}

	v.indentLevel--
	result.WriteString(v.indent(v.indentLevel))
	result.WriteRune('}')
	result.WriteString(v.newline)

	// Structural bases follow declared embeds, deduplicated against them
	for _, base := range structuralBases {
		if !slices.Contains(inheritedInterfaces, base) {
			inheritedInterfaces = append(inheritedInterfaces, base)
		}
	}

	inheritedResult := ""

	if len(typeConstraints) > 0 {
		inheritedResult = fmt.Sprintf("%s<%s>", inheritedResult, TypeT)
	}

	interfaceAttrs := ""
	postAttrs := " "

	if lifted {
		// Add "dyn" implementation attribute to lifted types since
		// they cannot be directly implemented in C# code. For these
		// types, a reflection based type implementation is used when
		// type assertions and comparisons are needed.
		interfaceAttrs = "dyn"
	}

	if len(operatorSets) > 0 {
		if len(interfaceAttrs) > 0 {
			interfaceAttrs += "; "
		}

		interfaceAttrs += fmt.Sprintf("operators = %s", getOperatorSetAttributes(operatorSets))
		postAttrs = v.newline
	}

	if len(interfaceAttrs) > 0 {
		interfaceAttrs = fmt.Sprintf("(\"%s\")", interfaceAttrs)
	}

	if len(inheritedInterfaces) > 0 {
		inheritedResult += " :" + v.newline

		for i, inheritedInterface := range inheritedInterfaces {
			if i > 0 {
				inheritedResult += "," + v.newline
			}

			inheritedResult += innerIndent + inheritedInterface
		}

		inheritedResult += v.newline + outerIndent

		// Track which interfaces this interface inherits from so duplicate interface
		// implementations can be avoided. The set carries BOTH the alias render (emission
		// form) and the canonical full-name render — the prune's implementation-map keys
		// are canonical, so the alias form alone never matched a foreign base (zip's
		// headerFileInfo implemented fs.FileInfo twice, CS8646 x6).
		trackedInheritances := append([]string{}, inheritedInterfaces...)
		trackedInheritances = append(trackedInheritances, canonicalInheritedInterfaces...)
		trackedInheritances = append(trackedInheritances, canonicalStructuralBases...)

		packageLock.Lock()
		interfaceInheritances[interfaceTypeName] = NewHashSet(trackedInheritances)
		packageLock.Unlock()
	} else {
		inheritedResult += " "
	}

	target.WriteString(strings.ReplaceAll(strings.ReplaceAll(strings.ReplaceAll(result.String(),
		InterfaceTypeAttributeMarker, interfaceAttrs),
		InterfacePostAtributeMarker, postAttrs),
		InterfaceInheritanceMarker, inheritedResult))

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

// getStructuralInterfaceBases finds EXPORTED method interfaces from directly imported packages
// whose method sets are STRICT subsets of the declared interface's — Go satisfies such
// conversions structurally, so the C# declaration inherits them and skips re-declaring the
// covered members. The strict-subset guard also rules out inheritance cycles (equal method
// sets can never inherit each other). Candidates already covered by a declared EMBED are
// skipped (the embed emission handles those, and a second differently-rendered base of the
// same type would be a duplicate-interface error). Returns the rendered base type names and
// the covered method names.
func (v *Visitor) getStructuralInterfaceBases(interfaceType *ast.InterfaceType, identType types.Type) ([]string, []string, HashSet[string]) {
	named, ok := identType.(*types.Named)

	if !ok {
		return nil, nil, nil
	}

	iface, ok := named.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 {
		return nil, nil, nil
	}

	var embeddedTypes []types.Type

	for _, method := range interfaceType.Methods.List {
		if len(method.Names) == 0 && method.Type != nil {
			if embedType := v.getType(method.Type, false); embedType != nil {
				if _, isIface := embedType.Underlying().(*types.Interface); isIface {
					embeddedTypes = append(embeddedTypes, embedType)
				}
			}
		}
	}

	var bases []*types.Named

	for _, imported := range v.pkg.Imports() {
		scope := imported.Scope()

		for _, name := range scope.Names() {
			typeName, ok := scope.Lookup(name).(*types.TypeName)

			if !ok || !typeName.Exported() || typeName.IsAlias() {
				continue
			}

			candidate, ok := typeName.Type().(*types.Named)

			if !ok || candidate.TypeParams().Len() > 0 {
				continue
			}

			candidateIface, ok := candidate.Underlying().(*types.Interface)

			if !ok || candidateIface.NumMethods() == 0 || candidateIface.NumMethods() >= iface.NumMethods() || !candidateIface.IsMethodSet() {
				continue
			}

			if !types.Implements(named, candidateIface) {
				continue
			}

			// A declared embed that implements the candidate already carries its members
			coveredByEmbed := false

			for _, embedType := range embeddedTypes {
				if types.Implements(embedType, candidateIface) {
					coveredByEmbed = true
					break
				}
			}

			if !coveredByEmbed {
				bases = append(bases, candidate)
			}
		}
	}

	if len(bases) == 0 {
		return nil, nil, nil
	}

	// Keep the minimal covering set: drop a base another chosen base already implements
	// (fs.File satisfies io.Reader, io.Closer AND io.ReadCloser — only ReadCloser is
	// listed; C# reaches the others through it). Equal-sized sets tie-break by index so
	// mutual implementers cannot drop each other both ways.
	baseNames := make([]string, 0, len(bases))
	canonicalBaseNames := make([]string, 0, len(bases))
	coveredCounts := map[string]int{}

	for i, candidate := range bases {
		candidateIface := candidate.Underlying().(*types.Interface)
		subsumed := false

		for j, other := range bases {
			if i == j {
				continue
			}

			otherIface := other.Underlying().(*types.Interface)

			if (otherIface.NumMethods() > candidateIface.NumMethods() ||
				(otherIface.NumMethods() == candidateIface.NumMethods() && j < i)) &&
				types.Implements(other, candidateIface) {
				subsumed = true
				break
			}
		}

		if subsumed {
			continue
		}

		// Reference through the file-local package ALIAS (`CrossPkgLib.Labeled`, user-ruled
		// style, mirroring the foreign-adapter references): getTypeName both yields the
		// aliased form and registers the file-local using — needed because the declaring
		// Go FILE may not import the candidate's package (fs.go declares File without
		// importing io).
		baseNames = append(baseNames, convertToCSTypeName(v.getTypeName(candidate, false)))

		// The CANONICAL (full-name) render feeds the duplicate-implementation prune,
		// which keys interfaceImplementations by getFullTypeName (see visit tracking).
		canonicalBaseNames = append(canonicalBaseNames, convertToCSTypeName(v.getFullTypeName(candidate, false)))

		// The base's FULL method set (embedded members included) is inherited
		for k := 0; k < candidateIface.NumMethods(); k++ {
			coveredCounts[candidateIface.Method(k).Name()]++
		}
	}

	// A member covered by exactly ONE listed base is inherited and skipped. A member covered
	// by TWO OR MORE bases (interfaces that share a method without subsuming each other) is
	// RE-DECLARED instead: the redeclaration hides both inherited slots, so a call through
	// this interface stays unambiguous (CS0121) — Go needs only one method to satisfy all.
	covered := HashSet[string]{}

	for name, count := range coveredCounts {
		if count == 1 {
			covered.Add(name)
		}
	}

	return baseNames, canonicalBaseNames, covered
}

func (v *Visitor) getSourceParameterSignatureLen(signature *types.Signature) int {
	parameters := signature.Params()

	if parameters == nil {
		return 0
	}

	result := 0

	for i := 0; i < parameters.Len(); i++ {
		param := parameters.At(i)

		if i > 0 {
			result += 2
		}

		result += len(v.getTypeName(param.Type(), false))

		if param.Name() != "" {
			result += 1 + len(param.Name())
		}
	}

	return result
}

func (v *Visitor) getSourceResultSignatureLen(signature *types.Signature) int {
	results := signature.Results()

	if results == nil {
		return 0
	}

	if results.Len() == 1 {
		return len(v.getTypeName(results.At(0).Type(), false))
	}

	result := 2

	for i := 0; i < results.Len(); i++ {
		if i > 0 {
			result += 2
		}

		result += len(v.getTypeName(results.At(i).Type(), false))
	}

	return result
}
