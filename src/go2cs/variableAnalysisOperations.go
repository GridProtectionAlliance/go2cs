package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"sort"
	"strings"
)

type TrackerType string

const (
	BlockTracker      TrackerType = "block"
	RangeTracker      TrackerType = "range"
	ForTracker        TrackerType = "for"
	FuncTracker       TrackerType = "func"
	IfTracker         TrackerType = "if"
	SwitchTracker     TrackerType = "switch"
	TypeSwitchTracker TrackerType = "typeswitch"
	SelectTracker     TrackerType = "select"
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

// trackerRegistry manages all our nested trackers
type trackerRegistry struct {
	trackers map[TrackerType]*nestedVarTracker
}

func newTrackerRegistry() *trackerRegistry {
	reg := &trackerRegistry{
		trackers: make(map[TrackerType]*nestedVarTracker),
	}
	// Initialize all tracker types
	for _, t := range []TrackerType{
		BlockTracker,
		RangeTracker,
		ForTracker,
		FuncTracker,
		IfTracker,
		SwitchTracker,
		TypeSwitchTracker,
		SelectTracker,
	} {
		reg.trackers[t] = newNestedVarTracker(string(t))
	}
	return reg
}

func (r *trackerRegistry) get(t TrackerType) *nestedVarTracker {
	return r.trackers[t]
}

// nestedVarTracker handles tracking of variables across nested scopes
type nestedVarTracker struct {
	stmtType   string                            // type of statement being tracked
	level      int                               // current nesting level
	vars       map[int]map[*ast.Ident]*types.Var // vars at each nesting level
	processing bool                              // flag for when processing vars in this tracker
}

func newNestedVarTracker(stmtType string) *nestedVarTracker {
	return &nestedVarTracker{
		stmtType: stmtType,
		vars:     make(map[int]map[*ast.Ident]*types.Var),
	}
}

func (t *nestedVarTracker) enter() {
	t.level++
	t.vars[t.level] = make(map[*ast.Ident]*types.Var)
}

func (t *nestedVarTracker) exit() {
	delete(t.vars, t.level)
	t.level--
}

func (t *nestedVarTracker) isTrackedAtOtherLevel(ident *ast.Ident) bool {
	for level := 1; level < t.level; level++ {
		if _, exists := t.vars[level][ident]; exists {
			return true
		}
	}
	return false
}

func (t *nestedVarTracker) track(ident *ast.Ident, varObj *types.Var) {
	t.vars[t.level][ident] = varObj
}

// varProcessor handles variable declaration and assignment processing
type varProcessor struct {
	tracker            *nestedVarTracker
	functionLevelDecls map[string]*types.Var
	declaredPos        map[*types.Var]token.Pos
	info               *types.Info
	declareVar         func(string, *types.Var, *ast.Ident)
	reassignVar        func(string, *ast.Ident)
}

func newVarProcessor(
	tracker *nestedVarTracker,
	functionLevelDecls map[string]*types.Var,
	declaredPos map[*types.Var]token.Pos,
	info *types.Info,
	declareVar func(string, *types.Var, *ast.Ident),
	reassignVar func(string, *ast.Ident),
) *varProcessor {
	return &varProcessor{
		tracker:            tracker,
		functionLevelDecls: functionLevelDecls,
		declaredPos:        declaredPos,
		info:               info,
		declareVar:         declareVar,
		reassignVar:        reassignVar,
	}
}

func (p *varProcessor) processIdent(ident *ast.Ident, isDefine bool) {
	if ident == nil || isDiscardedVar(ident.Name) {
		return
	}

	if isDefine {
		if obj := p.info.Defs[ident]; obj != nil {
			if varObj, ok := obj.(*types.Var); ok {
				p.declareVar(ident.Name, varObj, ident)
				p.tracker.track(ident, varObj)
			}
		}
	} else {
		if obj := p.info.Uses[ident]; obj != nil {
			if varObj, ok := obj.(*types.Var); ok {
				if !p.tracker.isTrackedAtOtherLevel(ident) {
					p.functionLevelDecls[ident.Name] = varObj
					p.declaredPos[varObj] = ident.Pos()
				}
				p.tracker.track(ident, varObj)
				p.reassignVar(ident.Name, ident)
			}
		}
	}
}

func (p *varProcessor) processAssignStmt(stmt *ast.AssignStmt) {
	isDefine := stmt.Tok == token.DEFINE
	for _, expr := range stmt.Lhs {
		if ident, ok := expr.(*ast.Ident); ok {
			p.processIdent(ident, isDefine)
		}
	}
}

func newLambdaCapture() *LambdaCapture {
	return &LambdaCapture{
		capturedVars:      make(map[*ast.Ident]*CapturedVarInfo),
		stmtCaptures:      make(map[ast.Node]map[*ast.Ident]bool),
		pendingCaptures:   make(map[string]*CapturedVarInfo),
		currentLambdaVars: make(map[string]string),
		detectingCaptures: true,
	}
}

// Helper functions to manage conversion phase lambda context
func (v *Visitor) enterLambdaConversion(node ast.Node) {
	v.lambdaCapture.conversionInLambda = true
	v.lambdaCapture.currentConversion = node
	v.lambdaCapture.currentLambdaVars = make(map[string]string)
}

func (v *Visitor) exitLambdaConversion() {
	v.lambdaCapture.conversionInLambda = false
	v.lambdaCapture.currentConversion = nil
	v.lambdaCapture.currentLambdaVars = nil
}

// Perform variable analysis on the specified function block, handling shadowing and scope
func (v *Visitor) performVariableAnalysis(funcDecl *ast.FuncDecl, signature *types.Signature) {
	v.identNames = make(map[*ast.Ident]string)
	v.isReassigned = make(map[*ast.Ident]bool)
	v.scopeStack = []map[string]*types.Var{v.globalScope}
	v.hasDefer = false
	v.hasRecover = false

	// Reset all capture-related state at the start of each function
	v.lambdaCapture = newLambdaCapture()
	v.capturedVarCount = make(map[string]int)

	// Track all function-level declarations for proper shadowing
	functionLevelDecls := make(map[string]*types.Var)
	var nameCounts = make(map[string]int)            // Counts for generating unique names
	var declaredPos = make(map[*types.Var]token.Pos) // Track declaration positions

	// Initialize tracker registry for different statement types
	registry := newTrackerRegistry()

	// Initialize local variables names from globals
	for varName, obj := range v.globalScope {
		v.varNames[obj] = varName
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
									// For declarations, always record the original position
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
									// Only add if not already declared
									if _, exists := functionLevelDecls[ident.Name]; !exists {
										functionLevelDecls[ident.Name] = varObj
										declaredPos[varObj] = ident.Pos()
									}
								}
							}
						}
					}
				}
			}

		case *ast.RangeStmt:
			if isFunctionLevelNode(n, funcDecl.Body) && n.Tok == token.ASSIGN {
				// Handle Key
				if key, ok := n.Key.(*ast.Ident); ok {
					if obj := v.info.Uses[key]; obj != nil {
						if varObj, ok := obj.(*types.Var); ok {
							// Only add if not already declared
							if _, exists := functionLevelDecls[key.Name]; !exists {
								functionLevelDecls[key.Name] = varObj
								declaredPos[varObj] = key.Pos()
							}
						}
					}
				}
				// Handle Value
				if value, ok := n.Value.(*ast.Ident); ok && value != nil {
					if obj := v.info.Uses[value]; obj != nil {
						if varObj, ok := obj.(*types.Var); ok {
							// Only add if not already declared
							if _, exists := functionLevelDecls[value.Name]; !exists {
								functionLevelDecls[value.Name] = varObj
								declaredPos[varObj] = value.Pos()
							}
						}
					}
				}
			}

		case *ast.ForStmt:
			if isFunctionLevelNode(n, funcDecl.Body) {
				if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
					for _, expr := range init.Lhs {
						if ident, ok := expr.(*ast.Ident); ok {
							if obj := v.info.Defs[ident]; obj != nil {
								if varObj, ok := obj.(*types.Var); ok {
									// Only add if not already declared
									if _, exists := functionLevelDecls[ident.Name]; !exists {
										functionLevelDecls[ident.Name] = varObj
										declaredPos[varObj] = ident.Pos()
									}
								}
							}
						}
					}
				}
			}

		case *ast.TypeSwitchStmt:
			if isFunctionLevelNode(n, funcDecl.Body) {
				if assign, ok := n.Assign.(*ast.AssignStmt); ok && assign.Tok == token.DEFINE {
					for _, expr := range assign.Lhs {
						if ident, ok := expr.(*ast.Ident); ok {
							if obj := v.info.Defs[ident]; obj != nil {
								if varObj, ok := obj.(*types.Var); ok {
									// Only add if not already declared
									if _, exists := functionLevelDecls[ident.Name]; !exists {
										functionLevelDecls[ident.Name] = varObj
										declaredPos[varObj] = ident.Pos()
									}
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
	if funcDecl.Body != nil {
		ast.Inspect(funcDecl.Body, collectFunctionLevelDecls)
	}

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

	getShadowedVarName := func(varName string) string {
		nameCounts[varName]++
		return fmt.Sprintf("%s%s%d", varName, ShadowVarMarker, nameCounts[varName])
	}

	// Declare variable, checking if an identifier needs shadowing
	declareVar := func(varName string, varObj *types.Var, ident *ast.Ident) {
		var adjustedName string

		// First check if this variable name conflicts with a function name
		if v.isFunctionNameInScope(varName) {
			adjustedName = getShadowedVarName(varName)
		} else if funcLevelVar, exists := functionLevelDecls[varName]; exists {
			needsShadowing := false

			// Check if we're in any tracked statement that requires shadowing
			for _, tracker := range registry.trackers {
				if tracker.processing {
					// Only shadow if the variable is already declared in an outer scope
					if isDeclaredInOuterScopes(varName, true) {
						needsShadowing = true
						break
					}
				}
			}

			// If not in a tracked statement, fall back to position-based logic
			if !needsShadowing {
				needsShadowing = declaredPos[funcLevelVar] > ident.Pos()
			}

			if needsShadowing {
				adjustedName = getShadowedVarName(varName)
			} else {
				adjustedName = varName
			}
		} else if isDeclaredInOuterScopes(varName, false) {
			adjustedName = getShadowedVarName(varName)
		} else {
			adjustedName = varName
			nameCounts[varName] = 0
		}

		v.varNames[varObj] = adjustedName
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
		adjustedName := v.varNames[varObj]
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
			if node == nil {
				return
			}

			// Enter a new scope
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))

			if initialBlock {
				initialBlock = false
				// Add function parameters (including receiver and results) to the current scope
				addFunctionParams(funcDecl, signature)
			} else {
				// Only track non-initial blocks
				blockTracker := registry.get(BlockTracker)
				blockTracker.enter()
				blockTracker.processing = true
			}

			for _, stmt := range node.List {
				visitNode(stmt)
			}

			if !initialBlock {
				// Only cleanup tracking for non-initial blocks
				blockTracker := registry.get(BlockTracker)
				blockTracker.processing = false
				blockTracker.exit()
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

				// Ignore constants here
				if varObj, ok := obj.(*types.Var); ok {
					declareVar(varName, varObj, ident)
				}
			}

			// Visit values
			for _, value := range node.Values {
				visitNode(value)
			}

		case *ast.Ident:
			if obj := v.info.Uses[node]; obj != nil {
				if varObj, ok := obj.(*types.Var); ok {
					if adjustedName, ok := v.varNames[varObj]; ok {
						v.identNames[node] = adjustedName
					}
				}
			}

		case *ast.FuncLit:
			// Enter a new scope for the function literal
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
			tracker := registry.get(FuncTracker)
			tracker.enter()
			tracker.processing = true

			// Enter lambda capture analysis context
			v.lambdaCapture.analysisInLambda = true
			v.lambdaCapture.currentLambda = node

			// Get funcDecl for the function literal
			funcDecl := &ast.FuncDecl{Type: node.Type}

			// Get signature for the function literal
			signature := v.info.TypeOf(funcDecl.Type).(*types.Signature)

			// Add function parameters (including receiver and results) to the current scope
			addFunctionParams(funcDecl, signature)

			// First pass: analyze the function body for captured variables
			ast.Inspect(node.Body, func(n ast.Node) bool {
				if id, ok := n.(*ast.Ident); ok {
					v.processPotentialCapture(id)
				}
				return true
			})

			tracker.processing = false

			// Visit the function body (will handle both shadowing and nested captures)
			visitNode(node.Body)

			// Exit lambda capture analysis context
			v.lambdaCapture.analysisInLambda = false
			v.lambdaCapture.currentLambda = nil

			// Exit the function literal scope
			tracker.exit()
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.GoStmt:
			// Enter capture analysis context for goroutine
			v.lambdaCapture.analysisInLambda = true
			v.lambdaCapture.currentLambda = node

			// Create capture set for this statement
			v.lambdaCapture.stmtCaptures[node] = make(map[*ast.Ident]bool)

			// Process function expression
			ast.Inspect(node.Call.Fun, func(n ast.Node) bool {
				if id, ok := n.(*ast.Ident); ok {
					v.processPotentialCapture(id)
					// If it was captured, associate it with this statement
					if _, exists := v.lambdaCapture.capturedVars[id]; exists {
						v.lambdaCapture.stmtCaptures[node][id] = true
					}
				}
				return true
			})

			// Note: parameters are passed as arguments to C# implementation of go
			// statement, so we don't need to check arguments for needing capture:

			// // Process arguments
			// for _, arg := range node.Call.Args {
			// 	ast.Inspect(arg, func(n ast.Node) bool {
			// 		if id, ok := n.(*ast.Ident); ok {
			// 			v.processPotentialCapture(id)
			// 			// If it was captured, associate it with this statement
			// 			if _, exists := v.lambdaCapture.capturedVars[id]; exists {
			// 				v.lambdaCapture.stmtCaptures[node][id] = true
			// 			}
			// 		}
			// 		return true
			// 	})
			// }

			visitNode(node.Call)

			// Exit capture analysis context
			v.lambdaCapture.analysisInLambda = false
			v.lambdaCapture.currentLambda = nil

		// Check for defer and recover calls
		case *ast.DeferStmt:
			v.hasDefer = true

			// Enter capture analysis context for goroutine
			v.lambdaCapture.analysisInLambda = true
			v.lambdaCapture.currentLambda = node

			// Create capture set for this statement
			v.lambdaCapture.stmtCaptures[node] = make(map[*ast.Ident]bool)

			// Process function expression
			ast.Inspect(node.Call.Fun, func(n ast.Node) bool {
				if id, ok := n.(*ast.Ident); ok {
					v.processPotentialCapture(id)
					// If it was captured, associate it with this statement
					if _, exists := v.lambdaCapture.capturedVars[id]; exists {
						v.lambdaCapture.stmtCaptures[node][id] = true
					}
				}
				return true
			})

			// Note: parameters are passed as arguments to C# implementation of defer
			// statement, so we don't need to check arguments for needing capture:

			// // Process arguments
			// for _, arg := range node.Call.Args {
			// 	ast.Inspect(arg, func(n ast.Node) bool {
			// 		if id, ok := n.(*ast.Ident); ok {
			// 			v.processPotentialCapture(id)
			// 			// If it was captured, associate it with this statement
			// 			if _, exists := v.lambdaCapture.capturedVars[id]; exists {
			// 				v.lambdaCapture.stmtCaptures[node][id] = true
			// 			}
			// 		}
			// 		return true
			// 	})
			// }

			visitNode(node.Call)

			// Exit capture analysis context
			v.lambdaCapture.analysisInLambda = false
			v.lambdaCapture.currentLambda = nil

		case *ast.IfStmt:
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
			tracker := registry.get(IfTracker)
			tracker.enter()
			tracker.processing = true

			if node.Init != nil {
				processor := newVarProcessor(
					tracker,
					functionLevelDecls,
					declaredPos,
					v.info,
					declareVar,
					reassignVar,
				)

				switch init := node.Init.(type) {
				case *ast.AssignStmt:
					processor.processAssignStmt(init)
				}
			}

			tracker.processing = false

			if node.Init != nil {
				// Manually check for recover function in the init statement
				if assign, ok := node.Init.(*ast.AssignStmt); ok {
					if assign.Tok == token.DEFINE {
						if assign.Rhs != nil {
							if len(assign.Rhs) == 1 {
								if call, ok := assign.Rhs[0].(*ast.CallExpr); ok {
									visitNode(call)
								}
							}
						}
					}
				}
			}

			// Visit condition
			if node.Cond != nil {
				visitNode(node.Cond)
			}

			// Visit body
			visitNode(node.Body)

			// Visit else
			if node.Else != nil {
				visitNode(node.Else)
			}

			tracker.exit()
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.SwitchStmt:
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
			tracker := registry.get(SwitchTracker)
			tracker.enter()
			tracker.processing = true

			if node.Init != nil {
				processor := newVarProcessor(
					tracker,
					functionLevelDecls,
					declaredPos,
					v.info,
					declareVar,
					reassignVar,
				)

				switch init := node.Init.(type) {
				case *ast.AssignStmt:
					processor.processAssignStmt(init)
				}
			}

			tracker.processing = false

			// Visit tag and body
			if node.Tag != nil {
				visitNode(node.Tag)
			}

			visitNode(node.Body)

			for _, stmt := range node.Body.List {
				if caseClause, ok := stmt.(*ast.CaseClause); ok {
					// Enter a new scope
					v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))

					// Visit the case body treating it as an implicit block
					blockTracker := registry.get(BlockTracker)
					blockTracker.enter()
					blockTracker.processing = true

					for _, stmt := range caseClause.Body {
						visitNode(stmt)
					}

					blockTracker.processing = false
					blockTracker.exit()

					// Exit the current scope
					v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]
				}
			}

			tracker.exit()
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.TypeSwitchStmt:
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
			tracker := registry.get(TypeSwitchTracker)
			tracker.enter()
			tracker.processing = true

			if node.Init != nil {
				processor := newVarProcessor(
					tracker,
					functionLevelDecls,
					declaredPos,
					v.info,
					declareVar,
					reassignVar,
				)

				switch init := node.Init.(type) {
				case *ast.AssignStmt:
					processor.processAssignStmt(init)
				}
			}

			// Handle assign statement (type switch specific)
			if assign, ok := node.Assign.(*ast.AssignStmt); ok {
				processor := newVarProcessor(
					tracker,
					functionLevelDecls,
					declaredPos,
					v.info,
					declareVar,
					reassignVar,
				)
				processor.processAssignStmt(assign)
			}

			tracker.processing = false

			visitNode(node.Body)

			tracker.exit()
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.RangeStmt:
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
			tracker := registry.get(RangeTracker)
			tracker.enter()
			tracker.processing = true

			processor := newVarProcessor(
				tracker,
				functionLevelDecls,
				declaredPos,
				v.info,
				declareVar,
				reassignVar,
			)

			// Track the shadowed names of range variables
			rangeVars := make(map[string]string)

			// Process range variables and remember their shadowed names
			if node.Key != nil {
				if keyIdent := getIdentifier(node.Key); keyIdent != nil {
					processor.processIdent(keyIdent, node.Tok == token.DEFINE)
					// Store any shadowed name that was created
					if shadowName, ok := v.identNames[keyIdent]; ok {
						rangeVars[keyIdent.Name] = shadowName
					}
				}
			}
			if node.Value != nil {
				if valueIdent := getIdentifier(node.Value); valueIdent != nil {
					processor.processIdent(valueIdent, node.Tok == token.DEFINE)
					// Store any shadowed name that was created
					if shadowName, ok := v.identNames[valueIdent]; ok {
						rangeVars[valueIdent.Name] = shadowName
					}
				}
			}

			tracker.processing = false

			visitNode(node.X)

			// Process identifiers in the loop body to use shadowed names
			ast.Inspect(node.Body, func(n ast.Node) bool {
				if ident, ok := n.(*ast.Ident); ok {
					if shadowName, exists := rangeVars[ident.Name]; exists {
						if obj := v.info.Uses[ident]; obj != nil {
							v.identNames[ident] = shadowName
						}
					}
				}
				return true
			})

			visitNode(node.Body)

			tracker.exit()
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.ForStmt:
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
			tracker := registry.get(ForTracker)
			tracker.enter()
			tracker.processing = true

			processor := newVarProcessor(
				tracker,
				functionLevelDecls,
				declaredPos,
				v.info,
				declareVar,
				reassignVar,
			)

			if node.Init != nil {
				if init, ok := node.Init.(*ast.AssignStmt); ok {
					processor.processAssignStmt(init)
				}
			}

			tracker.processing = false

			if node.Cond != nil {
				visitNode(node.Cond)
			}
			if node.Post != nil {
				visitNode(node.Post)
			}
			visitNode(node.Body)

			tracker.exit()
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

		case *ast.SelectStmt:
			v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))
			tracker := registry.get(SelectTracker)
			tracker.enter()

			// Process each case's statements
			for _, s := range node.Body.List {
				if commClause, ok := s.(*ast.CommClause); ok {
					// Handle communication statement assignments
					if comm, ok := commClause.Comm.(*ast.AssignStmt); ok {
						tracker.processing = true
						processor := newVarProcessor(
							tracker,
							functionLevelDecls,
							declaredPos,
							v.info,
							declareVar,
							reassignVar,
						)
						processor.processAssignStmt(comm)
						tracker.processing = false
					}

					// Enter a new scope
					v.scopeStack = append(v.scopeStack, make(map[string]*types.Var))

					// Visit the case body treating it as an implicit block
					blockTracker := registry.get(BlockTracker)
					blockTracker.enter()
					blockTracker.processing = true

					for _, stmt := range commClause.Body {
						visitNode(stmt)
					}

					blockTracker.processing = false
					blockTracker.exit()

					// Exit the current scope
					v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]
				}
			}

			tracker.exit()
			v.scopeStack = v.scopeStack[:len(v.scopeStack)-1]

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

		case *ast.SelectorExpr:
			if v.requiresLambdaConversion(node) {
				v.lambdaCapture.analysisInLambda = true
				v.lambdaCapture.currentLambda = node

				// Process the receiver for method values
				if id, ok := node.X.(*ast.Ident); ok {
					v.processPotentialCapture(id)
				}

				v.lambdaCapture.analysisInLambda = false
				v.lambdaCapture.currentLambda = nil
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

func (v *Visitor) processPotentialCapture(ident *ast.Ident) {
	var escapesToHeap bool

	if obj := v.info.ObjectOf(ident); obj != nil {
		if escapes, exists := v.identEscapesHeap[obj]; exists {
			escapesToHeap = escapes
		}
	}

	if !v.lambdaCapture.analysisInLambda || isDiscardedVar(ident.Name) {
		return
	}

	// Skip if we've already recorded this capture
	if v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda] != nil {
		if _, exists := v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda][ident]; exists {
			return
		}
	}

	obj := v.info.Uses[ident]
	if obj == nil {
		return
	}

	varObj, ok := obj.(*types.Var)

	if !ok {
		return
	}

	if !v.shouldCapture(varObj, ident) {
		return
	}

	// Check if variable needs capture due to:
	// 1. Being a reference type that needs copying, OR
	// 2. Having escaped to heap (being a ref in C#)
	needsRef := v.capturedLambdaVarRequiresCopy(varObj.Type())
	needsCapture := needsRef || escapesToHeap

	if !needsCapture {
		return
	}

	// Record the capture
	if v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda] == nil {
		v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda] = make(map[*ast.Ident]bool)
	}

	v.lambdaCapture.stmtCaptures[v.lambdaCapture.currentLambda][ident] = true
}

// Helper to determine if an expression requires lambda conversion
func (v *Visitor) requiresLambdaConversion(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.FuncLit:
		return true

	case *ast.SelectorExpr:
		return v.isMethodValue(e, false)

	case *ast.Ident:
		// Check if identifier refers to a function value
		if obj := v.info.ObjectOf(e); obj != nil {
			_, isFunc := obj.(*types.Func)
			return isFunc
		}

	case *ast.CallExpr:
		// Check if this is a function call that returns a function
		if typ := v.info.TypeOf(e); typ != nil {
			_, isFunc := typ.Underlying().(*types.Signature)
			return isFunc
		}
	}
	return false
}

func (v *Visitor) isMethodValue(sel *ast.SelectorExpr, isCallExpr bool) bool {
	if sel.Sel == nil {
		return false
	}

	obj := v.info.ObjectOf(sel.Sel)
	if obj == nil {
		return false
	}

	_, isFunc := obj.(*types.Func)
	if !isFunc {
		return false
	}

	// Not a method value if it's being called
	return !isCallExpr
}

func (v *Visitor) isPackageIdentifier(ident *ast.Ident) bool {
	obj := v.info.Uses[ident]
	_, ok := obj.(*types.PkgName)
	return ok
}

// isValueType determines if a type is a value type
func isValueType(t types.Type) bool {
	if t == nil {
		return false
	}

	if _, ok := t.Underlying().(*types.Basic); ok {
		return true
	}
	return false
}

// Determine if a type needs to be copied when captured
func (v *Visitor) capturedLambdaVarRequiresCopy(t types.Type) bool {
	switch typ := t.Underlying().(type) {
	case *types.Array:
		return true
	case *types.Struct:
		return true
	case *types.Chan:
		return true
	case *types.Slice:
		return true
	case *types.Map:
		return true
	case *types.Named:
		return v.capturedLambdaVarRequiresCopy(typ.Underlying())
	}
	return false
}

func (v *Visitor) getCapturedVarName(varPrefix string) string {
	if v.capturedVarCount == nil {
		v.capturedVarCount = make(map[string]int)
	}

	// Get counter specific to this variable prefix
	count := v.capturedVarCount[varPrefix]

	// Only increment during name generation phase
	if !v.lambdaCapture.detectingCaptures {
		count++
		v.capturedVarCount[varPrefix] = count
	}

	// Return the capture name using the variable-specific counter
	return fmt.Sprintf("%s%s%d", varPrefix, CapturedVarMarker, count)
}

func (v *Visitor) prepareStmtCaptures(stmt ast.Node) {
	if captures, ok := v.lambdaCapture.stmtCaptures[stmt]; ok {
		v.lambdaCapture.detectingCaptures = false

		// Use a map to ensure unique variables
		uniqueVars := make(map[string]*ast.Ident)

		for ident := range captures {
			// Only keep one instance of each variable name
			uniqueVars[ident.Name] = ident
		}

		// Create sorted list from unique variables
		idents := make([]*ast.Ident, 0, len(uniqueVars))

		for _, ident := range uniqueVars {
			idents = append(idents, ident)
		}

		sort.Slice(idents, func(i, j int) bool {
			return idents[i].Name < idents[j].Name
		})

		// Process unique captures in a consistent order
		for _, ident := range idents {
			captureName := v.getCapturedVarName(ident.Name)

			copyIdent := &ast.Ident{
				Name: captureName,
				Obj:  ident.Obj,
			}

			info := &CapturedVarInfo{
				origIdent: ident,
				copyIdent: copyIdent,
				varType:   v.info.TypeOf(ident),
			}

			v.lambdaCapture.pendingCaptures[ident.Name] = info
			v.lambdaCapture.currentLambdaVars[ident.Name] = captureName
		}
	}
}

// Generate declarations for pending captures - written out before lambda expression
func (v *Visitor) generateCaptureDeclarations() string {
	if v.lambdaCapture == nil {
		return ""
	}

	pendingCaptures := v.lambdaCapture.pendingCaptures

	if len(pendingCaptures) == 0 {
		return ""
	}

	var decls strings.Builder

	// Sort captured names for consistent output
	names := make([]string, 0, len(pendingCaptures))

	for name, info := range pendingCaptures {
		if !info.used {
			names = append(names, name)
		}
	}

	sort.Strings(names)

	wasInLambda := false

	if v.lambdaCapture != nil {
		wasInLambda = v.lambdaCapture.conversionInLambda
	}

	for _, name := range names {
		info := pendingCaptures[name]

		decls.WriteString(v.newline)
		decls.WriteString(v.indent(v.indentLevel))

		// "var" vs. explicit type
		if v.options.preferVarDecl {
			decls.WriteString("var ")
		} else {
			decls.WriteString(v.getCSTypeName(info.varType))
			decls.WriteRune(' ')
		}

		// The left-hand side is the new capture name, e.g. "fʗ1"
		decls.WriteString(info.copyIdent.Name)
		decls.WriteString(" = ")

		// Temporarily turn off in-lambda logic so we don't map back to fʗ1
		if v.lambdaCapture != nil {
			v.lambdaCapture.conversionInLambda = false
		}

		// Lookup the *types.Var for the original ident:
		obj := v.info.Uses[info.origIdent]

		// Fallback to original .Name if anything is missing
		outerName := info.origIdent.Name

		// If it’s a Var, see if we have a final overshadowed name in varNames
		if varObj, ok := obj.(*types.Var); ok {
			if shadowedName, found := v.varNames[varObj]; found {
				outerName = shadowedName
			}
		}

		// Restore old lambda state
		if v.lambdaCapture != nil {
			v.lambdaCapture.conversionInLambda = wasInLambda
		}

		// Now print e.g. "fʗ1 = fΔ1;"
		decls.WriteString(getSanitizedIdentifier(outerName))
		decls.WriteString(";")

		info.used = true
	}

	decls.WriteString(v.newline)
	decls.WriteString(v.indent(v.indentLevel))

	return decls.String()
}

// Determine if a variable should be captured
func (v *Visitor) shouldCapture(varObj *types.Var, ident *ast.Ident) bool {
	// Don't capture variables declared inside the lambda
	if v.info.Defs[ident] != nil {
		return false
	}

	// Only capture if:
	// 1. Declared outside but used inside the lambda
	// 2. Is a reference type OR address is taken
	declaredOutside := false
	for _, scope := range v.scopeStack {
		if scope[ident.Name] == varObj {
			declaredOutside = true
			break
		}
	}

	if !declaredOutside {
		return false
	}

	// Don't capture value types unless they need to escape
	if _, ok := varObj.Type().Underlying().(*types.Basic); ok {
		if escapes, exists := v.identEscapesHeap[varObj]; exists {
			return escapes
		}
		return false
	}

	return true
}

// Check if a name conflicts with any function in scope
func (v *Visitor) isFunctionNameInScope(name string) bool {
	// Check package scope for the name
	obj := v.pkg.Scope().Lookup(name)
	if obj == nil {
		return false
	}

	// Only proceed if it's a function from our package
	funcObj, ok := obj.(*types.Func)
	if !ok || funcObj.Pkg() != v.pkg {
		return false
	}

	// Now we need to analyze the current function's scope to see if:
	// 1. The function is used before any variable declaration with the same name
	// 2. The function is used as a value (delegate) rather than called
	if v.currentFuncDecl == nil {
		return false // Not in a function scope
	}

	var foundUsageBeforeDecl bool
	var foundDelegateUsage bool
	var varDeclPos token.Pos

	// Find variable declaration position
	ast.Inspect(v.currentFuncDecl, func(n ast.Node) bool {
		switch node := n.(type) {
		case *ast.ValueSpec:
			for _, ident := range node.Names {
				if ident.Name == name {
					varDeclPos = ident.Pos()
					return false
				}
			}
		case *ast.AssignStmt:
			if node.Tok == token.DEFINE {
				for _, expr := range node.Lhs {
					if ident, ok := expr.(*ast.Ident); ok && ident.Name == name {
						varDeclPos = ident.Pos()
						return false
					}
				}
			}
		}
		return true
	})

	// Track parent node during traversal
	var parent ast.Node

	ast.Inspect(v.currentFuncDecl, func(n ast.Node) bool {
		if n == nil {
			return true
		}

		switch node := n.(type) {
		case *ast.CallExpr:
			// Save as parent for checking identifiers
			parent = node
		case *ast.Ident:
			if node.Name == name {
				// Check if this identifier refers to our function
				if obj, ok := v.info.Uses[node].(*types.Func); ok && obj == funcObj {
					// Check if it's part of a call expression
					if callExpr, ok := parent.(*ast.CallExpr); ok {
						// Only consider it a conflict if the call is before the variable declaration
						if varDeclPos == token.NoPos || callExpr.Pos() < varDeclPos {
							foundUsageBeforeDecl = true
						}
					} else {
						// Used as a value rather than called
						foundDelegateUsage = true
					}
				}
			}
		default:
			// Save as parent for next iteration
			parent = node
		}
		return true
	})

	return foundUsageBeforeDecl || foundDelegateUsage
}
