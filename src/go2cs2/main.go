package main

import (
	"flag"
	"fmt"
	"go/ast"
	"go/importer"
	"go/parser"
	"go/printer"
	"go/token"
	"go/types"
	"io"
	"log"
	"os"
	"path/filepath"
	"strings"
	"sync"
	"unicode"
	"unicode/utf8"

	. "go2cs/hashset"
	. "go2cs/stack"
)

type Options struct {
	indentSpaces    int
	preferVarDecl   bool
	includeComments bool
	parseCgoTargets bool
	showParseTree   bool
}

type FileEntry struct {
	file     *ast.File
	filePath string
}

type Visitor struct {
	fset               *token.FileSet
	pkg                *types.Package
	info               *types.Info
	file               *token.File
	targetFile         *strings.Builder
	standAloneComments map[token.Pos]string
	sortedCommentPos   []token.Pos
	processedComments  HashSet[token.Pos]
	newline            string
	indentLevel        int
	usesUnsafeCode     bool
	options            Options
	globalIdentNames   map[*ast.Ident]string // Global identifiers to adjusted names map
	globalScope        map[string]*types.Var // Global variable scope

	// ImportSpec variables
	currentImportPath string
	packageImports    *strings.Builder
	importQueue       HashSet[string]
	requiredUsings    HashSet[string]

	// FuncDecl variables
	inFunction      bool
	currentFunction *types.Func
	hasDefer        bool
	hasPanic        bool
	hasRecover      bool
	tempVarCount    map[string]int

	// BlockStmt variables
	blocks                 Stack[*strings.Builder]
	firstStatementIsReturn bool
	identEscapesHeap       map[*ast.Ident]bool
	identNames             map[*ast.Ident]string   // Local identifiers to adjusted names map
	isReassigned           map[*ast.Ident]bool     // Local identifiers to reassignment status map
	scopeStack             []map[string]*types.Var // Stack of local variable scopes
}

const RootNamespace = "go"
const ClassSuffix = "_package"
const AddressPrefix = "Ꮡ"   // Ꮡ ꝸ ꞥ
const ShadowVarMarker = "Δ" // Δ Ʌ
const TempVarMarker = "Ʌ"   // Ʌ ꞥ
const ExprSwitchMarker = "ᐧ"

var keywords = NewHashSet[string]([]string{
	// The following are all valid C# keywords, if encountered in Go code they should be escaped
	"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
	"continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
	"false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
	"internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
	"params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
	"sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
	"uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
	"__argslist", "__makeref", "__reftype", "__refvalue",
	// The following C# type names are reserved by go2cs as they may be used during code conversion
	"GoType", "GoUntyped", "GoTag",
	// The following symbols are reserved by go2cs as they are publically defined in "golib"
	"WithOK", "WithErr", "WithVal", "InitKeyedValues", "GetGoTypeName", "CastCopy", "ConvertToType", ExprSwitchMarker,
})

/*
   Current expected C# global project aliases:

   <Using Include="go.builtin" Static="True" />
   <Using Include="System.Byte" Alias="uint8" />
   <Using Include="System.UInt16" Alias="uint16" />
   <Using Include="System.UInt32" Alias="uint32" />
   <Using Include="System.UInt64" Alias="uint64" />
   <Using Include="System.SByte" Alias="int8" />
   <Using Include="System.Int16" Alias="int16" />
   <Using Include="System.Int32" Alias="int32" />
   <Using Include="System.Int64" Alias="int64" />
   <Using Include="System.Single" Alias="float32" />
   <Using Include="System.Double" Alias="float64" />
   <Using Include="System.Numerics.Complex" Alias="complex128" />
   <Using Include="System.Int32" Alias="rune" />
   <Using Include="System.UIntPtr" Alias="uintptr" />
   <Using Include="System.Numerics.BigInteger" Alias="GoUntyped" />
   <Using Include="System.ComponentModel.DescriptionAttribute" Alias="GoTag" />

*/

func main() {
	commandLine := flag.NewFlagSet(os.Args[0], flag.ContinueOnError)
	commandLine.SetOutput(io.Discard)

	// Define command line flags for options
	indentSpaces := commandLine.Int("indent", 4, "Number of spaces for indentation")
	preferVarDecl := commandLine.Bool("var", true, "Prefer \"var\" declarations")
	includeComments := commandLine.Bool("comments", false, "Include comments in output")
	parseCgoTargets := commandLine.Bool("cgo", false, "Parse cgo targets")
	showParseTree := commandLine.Bool("tree", false, "Show parse tree")

	err := commandLine.Parse(os.Args[1:])
	inputFilePath := strings.TrimSpace(commandLine.Arg(0))

	if err != nil || inputFilePath == "" {
		if err != nil {
			fmt.Fprintf(os.Stderr, "Error: %s\n", err)
		}

		fmt.Fprintln(os.Stderr, `
File usage: go2cs [options] <input.go> [output.cs]
 Dir usage: go2cs [options] <input_dir> [output_dir]
 
 Options:`)

		commandLine.SetOutput(nil)
		commandLine.PrintDefaults()

		fmt.Fprintln(os.Stderr, `
Examples:
  go2cs -indent 2 -var=false example.go conv/example.cs
  go2cs example.go
  go2cs -cgo=true input_dir output_dir
  go2cs package_dir
 `)
		os.Exit(1)
	}

	options := Options{
		indentSpaces:    *indentSpaces,
		preferVarDecl:   *preferVarDecl,
		includeComments: *includeComments,
		parseCgoTargets: *parseCgoTargets,
		showParseTree:   *showParseTree,
	}

	fset := token.NewFileSet()
	files := []FileEntry{}

	// Check if the input is a file or a directory
	fileInfo, err := os.Stat(inputFilePath)

	if err != nil {
		log.Fatalf("Failed to access input file path \"%s\": %s\n", inputFilePath, err)
	}

	var parseMode parser.Mode

	if options.includeComments {
		parseMode = parser.ParseComments | parser.SkipObjectResolution
	} else {
		parseMode = parser.SkipObjectResolution
	}

	if fileInfo.IsDir() {
		// If the input is a directory, parse all .go files in the directory
		err := filepath.Walk(inputFilePath, func(path string, info os.FileInfo, err error) error {
			if err != nil {
				return err
			}

			if !info.IsDir() && strings.HasSuffix(info.Name(), ".go") {
				file, err := parser.ParseFile(fset, path, nil, parseMode)

				if err != nil {
					return fmt.Errorf("failed to parse input source file \"%s\": %s", path, err)
				}

				files = append(files, FileEntry{file, path})
			}

			return nil
		})

		if err != nil {
			log.Fatalf("Failed to parse files in directory \"%s\": %s\n", inputFilePath, err)
		}
	} else {
		// If the input is a single file, parse it
		if !strings.HasSuffix(inputFilePath, ".go") {
			log.Fatalln("Invalid file extension for input source file: please provide a .go file as first argument")
		}

		file, err := parser.ParseFile(fset, inputFilePath, nil, parseMode)

		if err != nil {
			log.Fatalf("Failed to parse input source file \"%s\": %s\n", inputFilePath, err)
		}

		files = append(files, FileEntry{file, inputFilePath})
	}

	conf := types.Config{Importer: importer.Default()}

	info := &types.Info{
		Types: make(map[ast.Expr]types.TypeAndValue),
		Defs:  make(map[*ast.Ident]types.Object),
		Uses:  make(map[*ast.Ident]types.Object),
	}

	extractFiles := func(files []FileEntry) []*ast.File {
		result := make([]*ast.File, len(files))

		for i, fileEntry := range files {
			result[i] = fileEntry.file
		}

		return result
	}

	pkg, err := conf.Check(".", fset, extractFiles(files), info)

	if err != nil {
		log.Fatalf("Failed to parse types from input source files: %s\n", err)
	}

	outputFilePath := ""

	if commandLine.NArg() > 1 {
		// If the user has provided a second argument, we will use it as the output directory or file
		outputFilePath = strings.TrimSpace(commandLine.Arg(1))
	}

	globalIdentNames := make(map[*ast.Ident]string)
	globalScope := map[string]*types.Var{}

	// Pre-process all global variables in package
	for _, fileEntry := range files {
		performGlobalVariableAnalysis(fileEntry.file.Decls, info, globalIdentNames, globalScope)

		if options.showParseTree {
			ast.Fprint(os.Stdout, fset, fileEntry.file, nil)
		}
	}

	var concurrentTasks sync.WaitGroup

	for _, fileEntry := range files {
		concurrentTasks.Add(1)

		go func(fileEntry FileEntry) {
			defer concurrentTasks.Done()

			visitor := &Visitor{
				fset:               fset,
				pkg:                pkg,
				info:               info,
				targetFile:         &strings.Builder{},
				packageImports:     &strings.Builder{},
				requiredUsings:     HashSet[string]{},
				importQueue:        HashSet[string]{},
				standAloneComments: map[token.Pos]string{},
				sortedCommentPos:   []token.Pos{},
				processedComments:  HashSet[token.Pos]{},
				newline:            "\r\n",
				options:            options,
				globalIdentNames:   globalIdentNames,
				globalScope:        globalScope,
				blocks:             Stack[*strings.Builder]{},
				identEscapesHeap:   map[*ast.Ident]bool{},
			}

			visitor.visitFile(fileEntry.file)

			var outputFileName string

			if outputFilePath != "" {
				if fileInfo.IsDir() {
					outputFileName = filepath.Join(outputFilePath, strings.TrimSuffix(filepath.Base(fileEntry.filePath), ".go")+".cs")
				} else {
					outputFileName = outputFilePath
				}
			} else {
				outputFileName = strings.TrimSuffix(fileEntry.filePath, ".go") + ".cs"
			}

			if err := visitor.writeOutputFile(outputFileName); err != nil {
				log.Printf("%s\n", err)
			}
		}(fileEntry)
	}

	concurrentTasks.Wait()
}

func (v *Visitor) writeOutputFile(outputFileName string) error {
	outputFile, err := os.Create(outputFileName)

	if err != nil {
		return fmt.Errorf("failed to create output source file \"%s\": %s", outputFileName, err)
	}

	defer outputFile.Close()

	_, err = outputFile.WriteString(v.targetFile.String())

	if err != nil {
		return fmt.Errorf("failed to write to output source file \"%s\": %s", outputFileName, err)
	}

	return nil
}

func (v *Visitor) addRequiredUsing(usingName string) {
	v.requiredUsings.Add(usingName)
}

func (v *Visitor) getPrintedNode(node ast.Node) string {
	result := &strings.Builder{}
	printer.Fprint(result, v.fset, node)
	return result.String()
}

func (v *Visitor) getStringLiteral(str string) (result string, isRawStr bool) {
	// Convert Go raw string literal to C# raw string literal
	if strings.HasPrefix(str, "`") {
		// Remove backticks from the start and end of the string
		str = strings.Trim(str, "`")

		// See if raw string literal is required (contains newline)
		if strings.Contains(str, "\n") {
			// C# raw string literals are enclosed in triple (or more) quotes
			prefix := `"""`
			suffix := `"""`

			// Keep adding quotes until the source string does not contain the
			// prefix to create a unique C# raw string literal token
			for while := strings.Contains(str, prefix); while; {
				prefix += `"`
				suffix += `"`
				while = strings.Contains(str, prefix)
			}

			// Ensure multiline C# raw string literals starts with newline
			if !strings.HasPrefix(str, "\n") {
				prefix += v.newline
			}

			// Ensure multiline C# raw string literals ends with newline
			if !strings.HasSuffix(str[:len(str)-1], "\n") {
				// Get index of last newline
				lastNewline := strings.LastIndex(str, "\n")

				// Check if any characters beyond the last newline are not whitespace
				if strings.TrimSpace(str[lastNewline:]) != "" {
					suffix = v.newline + suffix
				}
			}

			return prefix + str + suffix, true
		}

		// Use C# verbatim string literal for more simple raw strings
		return fmt.Sprintf("@\"%s\"", strings.ReplaceAll(str, "\"", "\"\"")), true
	}

	return str, false
}

func (v *Visitor) isNonCallValue(expr ast.Expr) bool {
	_, isCallExpr := expr.(*ast.CallExpr)

	return v.info.Types[expr].IsValue() && !isCallExpr
}

func getSanitizedIdentifier(identifier string) string {
	if strings.HasPrefix(identifier, "@") {
		return identifier // Already sanitized
	}

	if keywords.Contains(identifier) ||
		strings.HasPrefix(identifier, AddressPrefix) ||
		strings.HasSuffix(identifier, ClassSuffix) ||
		strings.Contains(identifier, ShadowVarMarker) {
		return "@" + identifier
	}

	// Handle special exceptions
	if identifier == "Main" {
		// This can be improved on by only escaping if it is a function
		// name and verifying that escaped name is unique
		return "_Main_"
	}

	return identifier
}

func getAccess(name string) string {
	// If name starts with a lowercase letter, scope is "private"
	ch, _ := utf8.DecodeRuneInString(name)

	if unicode.IsLower(ch) {
		return "private"
	}

	// Otherwise, scope is "public"
	return "public"
}

func isDiscardedVar(varName string) bool {
	return len(varName) == 0 || varName == "_"
}

func isLogicalOperator(op token.Token) bool {
	switch op {
	case token.LAND, token.LOR:
		return true
	default:
		return false
	}
}

func isComparisonOperator(op token.Token) bool {
	switch op {
	case token.EQL, token.NEQ, token.LSS, token.LEQ, token.GTR, token.GEQ:
		return true
	default:
		return false
	}
}

func (v *Visitor) isInterface(ident *ast.Ident) bool {
	obj := v.info.ObjectOf(ident)

	if obj == nil {
		return false
	}

	return isInterface(obj.Type())
}

func isInterface(t types.Type) bool {
	exprType := t.Underlying()

	_, isInterface := exprType.(*types.Interface)

	return isInterface
}

func paramsAreInterfaces(paramTypes *types.Tuple) []bool {
	if paramTypes == nil {
		return nil
	}

	paramIsInterface := make([]bool, paramTypes.Len())

	for i := 0; i < paramTypes.Len(); i++ {
		param := paramTypes.At(i)
		paramIsInterface[i] = isInterface(param.Type())
	}

	return paramIsInterface
}

func (v *Visitor) convertToInterfaceType(interfaceExpr ast.Expr, targetExpr string) string {
	return convertToInterfaceType(v.getType(interfaceExpr, false), targetExpr)
}

func convertToInterfaceType(interfaceType types.Type, targetExpr string) string {
	result := &strings.Builder{}

	// Convert to interface type using Go converted interface ".As" method,
	// this handles duck typed Go interface implementations
	result.WriteString(convertToCSTypeName(getTypeName(interfaceType)))
	result.WriteString(".As(")
	result.WriteString(targetExpr)
	result.WriteRune(')')

	return result.String()
}

func getIdentifier(node ast.Node) *ast.Ident {
	var ident *ast.Ident

	if indexExpr, ok := node.(*ast.IndexExpr); ok {
		if identExpr, ok := indexExpr.X.(*ast.Ident); ok {
			ident = identExpr
		}
	} else if starExpr, ok := node.(*ast.StarExpr); ok {
		ident = getIdentifier(starExpr.X)
	} else if identExpr, ok := node.(*ast.Ident); ok {
		ident = identExpr
	}

	return ident
}

func (v *Visitor) getType(expr ast.Expr, underlying bool) types.Type {
	exprType := v.info.TypeOf(expr)

	if exprType == nil {
		return nil
	}

	if underlying {
		return exprType.Underlying()
	}

	return exprType
}

func (v *Visitor) getTypeName(expr ast.Expr, underlying bool) string {
	return getTypeName(v.getType(expr, underlying))
}

func getTypeName(t types.Type) string {
	if named, ok := t.(*types.Named); ok {
		return named.Obj().Name()
	}

	return t.String()
}

func getCSTypeName(t types.Type) string {
	return convertToCSTypeName(getTypeName(t))
}

func convertToCSTypeName(typeName string) string {
	fullTypeName := convertToCSFullTypeName(typeName)

	// If full type name starts with root namespace, remove it
	if strings.HasPrefix(fullTypeName, RootNamespace+".") {
		return fullTypeName[len(RootNamespace)+1:]
	}

	return fullTypeName
}

func convertToCSFullTypeName(typeName string) string {
	typeName = strings.TrimPrefix(typeName, "untyped ")

	if strings.HasPrefix(typeName, "[]") {
		return fmt.Sprintf("go.slice<%s>", convertToCSTypeName(typeName[2:]))
	}

	// Handle array types
	if strings.HasPrefix(typeName, "[") {
		return fmt.Sprintf("go.array<%s>", convertToCSTypeName(typeName[strings.Index(typeName, "]")+1:]))
	}

	if strings.HasPrefix(typeName, "map[") {
		keyValue := strings.Split(typeName[4:len(typeName)-1], "]")
		return fmt.Sprintf("go.map<%s, %s>", convertToCSTypeName(keyValue[0]), convertToCSTypeName(keyValue[1]))
	}

	if strings.HasPrefix(typeName, "chan ") {
		return fmt.Sprintf("go.chan<%s>", convertToCSTypeName(typeName[5:]))
	}

	if strings.HasPrefix(typeName, "func(") {
		return fmt.Sprintf("Func<%s>", convertToCSTypeName(typeName[5:len(typeName)-1]))
	}

	// Handle pointer types
	if strings.HasPrefix(typeName, "*") {
		return fmt.Sprintf("go.ptr<%s>", convertToCSTypeName(typeName[1:]))
	}

	switch typeName {
	case "int":
		return "nint"
	case "uint":
		return "nuint"
	case "bool":
		return "bool"
	case "float":
		return "float64"
	case "complex64":
		return "go.complex64"
	case "string":
		return "go.@string"
	case "interface{}":
		return "object"
	default:
		return getSanitizedIdentifier(typeName)
	}
}

func (v *Visitor) convertToHeapTypeDecl(ident *ast.Ident) string {
	escapesHeap := v.identEscapesHeap[ident]
	identType := v.info.TypeOf(ident)

	if !escapesHeap || isInherentlyHeapAllocatedType(identType) {
		return ""
	}

	goTypeName := getTypeName(identType)
	csIDName := v.getIdentName(ident)

	// Handle array types
	if strings.HasPrefix(goTypeName, "[") {
		arrayLen := strings.Split(goTypeName[1:], "]")[0]

		// Get array element type
		arrayType := convertToCSTypeName(goTypeName[strings.Index(goTypeName, "]")+1:])

		if v.options.preferVarDecl {
			return fmt.Sprintf("ref var %s = ref heap(new array<%s>(%s), out var %s%s);", csIDName, arrayType, arrayLen, AddressPrefix, csIDName)
		}

		return fmt.Sprintf("ref array<%s> %s = ref heap(new array<%s>(%s), out ptr<array<%s>> %s%s);", arrayType, csIDName, arrayType, arrayLen, arrayType, AddressPrefix, csIDName)
	}

	csTypeName := convertToCSTypeName(goTypeName)

	if v.options.preferVarDecl {
		return fmt.Sprintf("ref var %s = ref heap(new %s(), out var %s%s);", csIDName, csTypeName, AddressPrefix, csIDName)
	}

	return fmt.Sprintf("ref %s %s = ref heap(out ptr<%s> %s%s);", csTypeName, csIDName, csTypeName, AddressPrefix, csIDName)
}

func isInherentlyHeapAllocatedType(typ types.Type) bool {
	switch typ.Underlying().(type) {
	case *types.Map, *types.Slice, *types.Chan, *types.Interface, *types.Signature, *types.Pointer:
		// Maps, slices, channels, interfaces, functions and pointers are reference types
		return true
	default:
		return false
	}
}

func getParameterType(sig *types.Signature, i int) (types.Type, bool) {
	var paramType types.Type
	params := sig.Params()

	// Check variadic parameter type
	if sig.Variadic() && i >= params.Len()-1 {
		paramType = params.At(params.Len() - 1).Type()

		if sliceType, ok := paramType.(*types.Slice); ok {
			paramType = sliceType.Elem()
		}
	} else if i < params.Len() {
		paramType = params.At(i).Type()
	} else {
		return nil, false
	}

	return paramType, true
}

func (v *Visitor) getVarIdent(varType *types.Var) *ast.Ident {
	for ident, obj := range v.info.Defs {
		if obj == varType {
			return ident
		}
	}

	return nil
}

// Get the adjusted identifier name, considering shadowing
func (v *Visitor) getIdentName(ident *ast.Ident) string {
	if v.identNames != nil {
		if name, ok := v.identNames[ident]; ok {
			return name
		}
	}

	if v.globalIdentNames != nil {
		if name, ok := v.globalIdentNames[ident]; ok {
			return name
		}
	}

	return ident.Name
}

// Determine if the identifier represents a reassignment
func (v *Visitor) isReassignment(ident *ast.Ident) bool {
	return v.isReassigned[ident]
}
