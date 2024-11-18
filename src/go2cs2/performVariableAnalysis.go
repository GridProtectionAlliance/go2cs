package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
)

// These variable analysis functions are used to analyze variable declarations and assignments
// in the Go source code. The analysis is performed on global variables and on a per-function
// basis. The goal is to identify reassignments and shadowed variables so these can be handled
// correctly during the conversion process since C# does not allow redeclarations or shadowing
// of variables with the same name, at least within the same function scope. Two derivative
// functions use the results of this analysis: `getIdentName` and `isReassignment`.

// Perform variable analysis on the global ValueSpec declarations
func performGlobalVariableAnalysis(decls []ast.Decl, info *types.Info, globalIdentNames map[*ast.Ident]string, globalScope map[string]*types.Var) {
	for _, decl := range decls {
		switch genDecl := decl.(type) {
		case *ast.GenDecl:
			for _, spec := range genDecl.Specs {
				switch spec := spec.(type) {
				case *ast.ValueSpec:
					for _, ident := range spec.Names {
						varName := ident.Name

						if isDiscardedVar(varName) {
							continue
						}

						obj := info.Defs[ident]

						if obj == nil {
							continue
						}

						var varObj *types.Var
						var ok bool

						if varObj, ok = obj.(*types.Var); !ok {
							continue
						}

						globalIdentNames[ident] = getSanitizedIdentifier(varName)
						globalScope[varName] = varObj
					}
				}
			}
		}
	}
}

// Perform variable analysis on the specified function block, handling shadowing and scope
func (v *Visitor) performVariableAnalysis(funcDecl *ast.FuncDecl, signature *types.Signature) {
	v.identNames = make(map[*ast.Ident]string)
	v.isReassigned = make(map[*ast.Ident]bool)
	v.scopeStack = []map[string]*types.Var{v.globalScope}
	v.hasDefer = false
	v.hasRecover = false

	// Track all function-level declarations for proper shadowing
	functionLevelDecls := make(map[string]*types.Var)
	var varNames = make(map[*types.Var]string)       // Map from types.Var to adjusted names
	var nameCounts = make(map[string]int)            // Counts for generating unique names
	var declaredPos = make(map[*types.Var]token.Pos) // Track declaration positions

	// Initialize local variables names from globals
	for varName, obj := range v.globalScope {
		varNames[obj] = varName
	}

	// Helper function to determine if a node is directly in the function body
	// (not in a nested block or control structure)
	isFunctionLevelNode := func(node ast.Node, funcBody *ast.BlockStmt) bool {
		for _, stmt := range funcBody.List {
			if stmt == node {
				return true
			}
		}
		return false
	}

	// First pass: collect all function-level variable declarations
	collectFunctionLevelDecls := func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.FuncLit:
			// Skip the body of function literals (they have their own scope)
			return false

		case *ast.DeclStmt:
			if genDecl, ok := n.Decl.(*ast.GenDecl); ok {
				for _, spec := range genDecl.Specs {
					if valueSpec, ok := spec.(*ast.ValueSpec); ok {
						for _, ident := range valueSpec.Names {
							if obj := v.info.Defs[ident]; obj != nil {
								if varObj, ok := obj.(*types.Var); ok {
									functionLevelDecls[ident.Name] = varObj
									declaredPos[varObj] = ident.Pos()
								}
							}
						}
					}
				}
			}

		case *ast.AssignStmt:
			if n.Tok == token.DEFINE {
				// Check if this assignment is directly in the function body
				// (not in a nested block or control structure)
				if isFunctionLevelNode(n, funcDecl.Body) {
					for _, expr := range n.Lhs {
						if ident, ok := expr.(*ast.Ident); ok {
							if obj := v.info.Defs[ident]; obj != nil {
								if varObj, ok := obj.(*types.Var); ok {
									functionLevelDecls[ident.Name] = varObj
									declaredPos[varObj] = ident.Pos()
								}
							}
						}
					}
				}
			}

		case *ast.RangeStmt:
			// Only process range variables if they're using = (not :=)
			// and at function level
			if isFunctionLevelNode(n, funcDecl.Body) && n.Tok == token.ASSIGN {
				// For assignments (=), these are function-level variables
				if key, ok := n.Key.(*ast.Ident); ok {
					if obj := v.info.Uses[key]; obj != nil {
						if varObj, ok := obj.(*types.Var); ok {
							// Only add if it's a pre-existing variable
							if _, exists := functionLevelDecls[key.Name]; !exists {
								functionLevelDecls[key.Name] = varObj
								declaredPos[varObj] = key.Pos()
							}
						}
					}
				}
				if value, ok := n.Value.(*ast.Ident); ok && value != nil {
					if obj := v.info.Uses[value]; obj != nil {
						if varObj, ok := obj.(*types.Var); ok {
							// Only add if it's a pre-existing variable
							if _, exists := functionLevelDecls[value.Name]; !exists {
								functionLevelDecls[value.Name] = varObj
								declaredPos[varObj] = value.Pos()
							}
						}
					}
				}
			}
			// Note: We don't collect range variables when Tok == token.DEFINE
			// as these are block-scoped and will be handled during the main traversal

		case *ast.ForStmt:
			// Only process the init statement if it's at function level
			if isFunctionLevelNode(n, funcDecl.Body) {
				if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
					for _, expr := range init.Lhs {
						if ident, ok := expr.(*ast.Ident); ok {
							if obj := v.info.Defs[ident]; obj != nil {
								if varObj, ok := obj.(*types.Var); ok {
									functionLevelDecls[ident.Name] = varObj
									declaredPos[varObj] = ident.Pos()
								}
							}
						}
					}
				}
			}

		case *ast.TypeSwitchStmt:
			// Only process the assign statement if it's at function level
			if isFunctionLevelNode(n, funcDecl.Body) {
				if assign, ok := n.Assign.(*ast.AssignStmt); ok && assign.Tok == token.DEFINE {
					for _, expr := range assign.Lhs {
						if ident, ok := expr.(*ast.Ident); ok {
							if obj := v.info.Defs[ident]; obj != nil {
								if varObj, ok := obj.(*types.Var); ok {
									functionLevelDecls[ident.Name] = varObj
									declaredPos[varObj] = ident.Pos()
								}
							}
						}
					}
				}
			}
		}

		return true
	}

	// Collect all function-level declarations
	ast.Inspect(funcDecl.Body, collectFunctionLevelDecls)

	// Helper functions
	isDeclaredInCurrentScope := func(varName string) bool {
		scope := v.scopeStack[len(v.scopeStack)-1]
		_, exists := scope[varName]
		return exists
	}

	isDeclaredInOuterScopes := func(varName string, includeGlobal bool) bool {
		minScope := 1
		if includeGlobal {
			minScope = 0
		}
		for i := len(v.scopeStack) - 2; i >= minScope; i-- {
			if _, exists := v.scopeStack[i][varName]; exists {
				return true
			}
		}
		return false
	}

	lookupVar := func(varName string) *types.Var {
		// First check if it's a function-level declaration
		if varObj, exists := functionLevelDecls[varName]; exists {
			return varObj
		}
		// Then check scope stack
		for i := len(v.scopeStack) - 1; i >= 0; i-- {
			if varObj, exists := v.scopeStack[i][varName]; exists {
				return varObj
			}
		}
		return nil
	}

	getShadowedVarName := func(varName string, varObj *types.Var) string {
		nameCounts[varName]++
		return fmt.Sprintf("%s%s%d", varName, ShadowVarMarker, nameCounts[varName])
	}

	// Track if we're currently processing a range statement's variables
	inRangeVars := false

	// Helper to check if an identifier is a range loop variable
	isRangeLoopVariable := func(ident *ast.Ident) bool {
		// Check if we're currently processing range variables
		if inRangeVars {
			return true
		}

		// Alternative approach: check if the identifier is defined in a range statement
		// by walking up its parent nodes
		var isRange bool

		ast.Inspect(funcDecl, func(n ast.Node) bool {
			if n == nil {
				return false
			}

			if rs, ok := n.(*ast.RangeStmt); ok {
				// Check if this ident is the Key or Value of this range statement
				if key, ok := rs.Key.(*ast.Ident); ok && key == ident {
					isRange = true
					return false
				}
				if rs.Value != nil {
					if value, ok := rs.Value.(*ast.Ident); ok && value == ident {
						isRange = true
						return false
					}
				}
			}
			return true
		})

		return isRange
	}

	declareVar := func(varName string, varObj *types.Var, ident *ast.Ident) {
		var adjustedName string

		// Check if this is shadowing a function-level declaration that appears anywhere
		if funcLevelVar, exists := functionLevelDecls[varName]; exists {
			// For range variables with := we always want to shadow the outer variable
			// regardless of where it's declared
			if isRangeLoopVariable(ident) {
				adjustedName = getShadowedVarName(varName, varObj)
			} else if declaredPos[funcLevelVar] > ident.Pos() {
				// For non-range variables, maintain the original position-based logic
				adjustedName = getShadowedVarName(varName, varObj)
			} else {
				adjustedName = varName
			}
		} else if isDeclaredInOuterScopes(varName, false) {
			// Normal shadowing of an outer scope variable
			adjustedName = getShadowedVarName(varName, varObj)
		} else {
			adjustedName = varName
			nameCounts[varName] = 0
		}

		varNames[varObj] = adjustedName
		v.identNames[ident] = adjustedName
		v.isReassigned[ident] = false

		scope := v.scopeStack[len(v.scopeStack)-1]
		scope[varName] = varObj
	}

	reassignVar := func(varName string, ident *ast.Ident) {
		varObj := lookupVar(varName)
		if varObj == nil {
			return
		}
		adjustedName := varNames[varObj]
		v.identNames[ident] = getSanitizedIdentifier(adjustedName)
		v.isReassigned[ident] = true
	}

	addFunctionParams := func(funcDecl *ast.FuncDecl, signature *types.Signature) {
		// Add all function parameters to the current scope
		parameters := getParameters(signature, false)
		paramIndex := 0

		if len(funcDecl.Type.Params.List) > 0 {
			for _, field := range funcDecl.Type.Params.List {
				names := field.Names

				if len(names) == 0 {
					// Anonymous parameter (no name)
					paramIndex++
				} else {
					for _, ident := range names {
						name := ident.Name

						if isDiscardedVar(name) {
							paramIndex++
							continue
						}

						param := parameters.At(paramIndex)
						declareVar(name, param, ident)
						paramIndex++
					}
				}
			}
		}

		// Handle receiver if present
		if signature.Recv() != nil && funcDecl.Recv != nil && len(funcDecl.Recv.List) > 0 {
			field := funcDecl.Recv.List[0]
			names := field.Names

			if len(names) > 0 {
				ident := names[0]
				recv := signature.Recv()
				recvName := ident.Name

				if !isDiscardedVar(recvName) {
					declareVar(recvName, recv, ident)
				}
			}
		}

		// Add named result parameters to the current scope
		results := signature.Results()
		resultIndex := 0

		if funcDecl.Type.Results != nil && len(funcDecl.Type.Results.List) > 0 {
			for _, field := range funcDecl.Type.Results.List {
				names := field.Names

				if len(names) == 0 {
					// Anonymous result (no name)
					resultIndex++
				} else {
					for _, ident := range names {
						name := ident.Name

						if isDiscardedVar(name) {
							resultIndex++
							continue
						}

						result := results.At(resultIndex)
						declareVar(name, result, ident)
						resultIndex++
					}
				}
			}
		}
	}

	var visitNode func(n ast.Node)
	initialBlock := true

	visitNode = func(n ast.Node) {
		switch node := n.(type) {
		case *ast.BlockStmt:
			// Enter a new scope
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))

			if initialBlock {
				initialBlock = false

				// Add function parameters (including receiver and results) to the current scope
				addFunctionParams(funcDecl, signature)
			}

			for _, stmt := range node.List {
				visitNode(stmt)
			}

			// Exit the current scope
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.AssignStmt:
			if node.Tok == token.DEFINE {
				// Short variable declaration ':='
				for _, lhs := range node.Lhs {
					ident := getIdentifier(lhs)

					if ident == nil {
						continue
					}

					varName := ident.Name

					if isDiscardedVar(varName) {
						continue
					}

					if isDeclaredInCurrentScope(varName) {
						reassignVar(varName, ident)
					} else {
						// New variable declaration
						obj := v.info.Defs[ident]

						if obj == nil {
							continue
						}

						varObj := obj.(*types.Var)
						declareVar(varName, varObj, ident)
					}
				}
			} else {
				// Regular assignment '='
				for _, lhs := range node.Lhs {
					ident := getIdentifier(lhs)

					if ident == nil {
						continue
					}

					varName := ident.Name

					if isDiscardedVar(varName) {
						continue
					}

					reassignVar(varName, ident)
				}
			}

			// Visit RHS expressions
			for _, rhs := range node.Rhs {
				visitNode(rhs)
			}

		case *ast.ValueSpec:
			// Variable declaration using 'var'
			for _, ident := range node.Names {
				varName := ident.Name

				if isDiscardedVar(varName) {
					continue
				}

				obj := v.info.Defs[ident]

				if obj == nil {
					continue
				}

				varObj := obj.(*types.Var)
				declareVar(varName, varObj, ident)
			}

			// Visit values
			for _, value := range node.Values {
				visitNode(value)
			}

		case *ast.Ident:
			if obj := v.info.Uses[node]; obj != nil {
				if varObj, ok := obj.(*types.Var); ok {
					if adjustedName, ok := varNames[varObj]; ok {
						v.identNames[node] = adjustedName
					}
				}
			}

		case *ast.FuncLit:
			// Enter a new scope for the function literal
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))

			// Get funcDecl for the function literal
			funcDecl := &ast.FuncDecl{Type: node.Type}

			// Get signature for the function literal
			signature := v.info.TypeOf(funcDecl.Type).(*types.Signature)

			// Add function parameters (including receiver and results) to the current scope
			addFunctionParams(funcDecl, signature)

			// Visit the function body
			visitNode(node.Body)

			// Exit the function literal scope
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.IfStmt:
			// Enter a new scope if there's an Init statement
			if node.Init != nil {
				v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
				visitNode(node.Init)
			}

			// Visit Cond
			visitNode(node.Cond)

			// Visit Body
			visitNode(node.Body)

			// Visit Else
			if node.Else != nil {
				visitNode(node.Else)
			}

			// Exit the scope if it was created
			if node.Init != nil {
				v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]
			}

		case *ast.SwitchStmt:
			// Enter a new scope if there's an Init statement
			if node.Init != nil {
				v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
				visitNode(node.Init)
			}

			// Visit Tag
			visitNode(node.Tag)

			// Visit Body
			visitNode(node.Body)

			// Exit the scope if it was created
			if node.Init != nil {
				v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]
			}

		case *ast.TypeSwitchStmt:
			// Enter a new scope for the type switch statement
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))

			// Visit Init if present
			if node.Init != nil {
				visitNode(node.Init)
			}

			// Visit Assign
			if node.Assign != nil {
				visitNode(node.Assign)
			}

			// Visit Body
			visitNode(node.Body)

			// Exit the current scope
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.RangeStmt:
			// Enter a new scope for the range statement
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))

			// Set flag before processing range variables
			inRangeVars = true

			// Handle range variables
			if node.Key != nil {
				if key, ok := node.Key.(*ast.Ident); ok {
					if obj := v.info.Defs[key]; obj != nil {
						if varObj, ok := obj.(*types.Var); ok {
							declareVar(key.Name, varObj, key)
						}
					}
				}
			}

			if node.Value != nil {
				if value, ok := node.Value.(*ast.Ident); ok {
					if obj := v.info.Defs[value]; obj != nil {
						if varObj, ok := obj.(*types.Var); ok {
							declareVar(value.Name, varObj, value)
						}
					}
				}
			}

			// Reset flag after processing range variables
			inRangeVars = false

			// Visit the range expression
			visitNode(node.X)

			// Visit the loop body
			visitNode(node.Body)

			// Exit the current scope
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.ForStmt:
			// Enter a new scope for the loop
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))

			// Visit the Init statement (may declare new variables)
			if node.Init != nil {
				visitNode(node.Init)
			}

			// Visit the Condition expression
			if node.Cond != nil {
				visitNode(node.Cond)
			}

			// Visit the Post statement
			if node.Post != nil {
				visitNode(node.Post)
			}

			// Visit the loop body
			visitNode(node.Body)

			// Exit the current scope
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		// Check for defer and recover calls
		case *ast.DeferStmt:
			v.hasDefer = true

			// Visit the function call
			visitNode(node.Call)

		case *ast.CallExpr:
			if fun, ok := node.Fun.(*ast.Ident); ok {
				if fun.Name == "recover" {
					obj := v.info.Uses[fun]

					if obj != nil && obj.Parent() == types.Universe {
						v.hasRecover = true
					}
				}
			}

			// Visit function
			visitNode(node.Fun)

			// Visit call arguments
			for _, arg := range node.Args {
				visitNode(arg)
			}

		default:
			// Visit child nodes
			ast.Inspect(n, func(child ast.Node) bool {
				if child == nil {
					return false // Do not call visitNode with nil
				}

				if child == n {
					return true // Avoid processing the same node again
				}

				visitNode(child)

				// Return false to prevent ast.Inspect from recursing
				// into children, since visitNode will handle them
				return false
			})
		}
	}

	// Start traversal
	visitNode(funcDecl.Body)
}
