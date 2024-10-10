package main

import (
	"fmt"
	"go/ast"
	"go/importer"
	"go/parser"
	"go/printer"
	"go/token"
	"go/types"
	"log"
	"os"
	"strings"
	"unicode"
	"unicode/utf8"

	. "go2cs/hashset"
	. "go2cs/stack"
)

type Options struct {
	indentSpaces    int
	preferVarDecl   bool
	parseCgoTargets bool
	showParseTree   bool
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

	// BlockStmt variables
	blocks                    Stack[*strings.Builder]
	blockInnerPrefixInjection Stack[string]
	blockInnerSuffixInjection Stack[string]
	blockOuterPrefixInjection Stack[string]
	blockOuterSuffixInjection Stack[string]
	firstStatementIsReturn    bool
	identEscapesHeap          map[*ast.Ident]bool
	identNames                map[*ast.Ident]string   // Local identifiers to adjusted names map
	isReassigned              map[*ast.Ident]bool     // Local identifiers to reassignment status map
	scopeStack                []map[string]*types.Var // Stack of local variable scopes

	// SwitchStmt variables
	switchIndentLevel int
	caseFallthrough   bool
}

const RootNamespace = "go"
const ClassSuffix = "_package"
const AddressPrefix = "Ꮡ" // Ꮡ ꝸ Ʌ ᥍ Ზ
const ShadowVarMarker = "ꞥ"

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
	"WithOK", "WithErr", "WithVal", "InitKeyedValues", "GetGoTypeName", "CastCopy", "ConvertToType",
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
	// TODO: Add option to process an entire package (all files in a directory)
	if len(os.Args) < 2 {
		log.Fatalln("Usage: go run main.go <input.go> [output.cs]")
	}

	inputFileName := strings.TrimSpace(os.Args[1])

	// Check if the file has a ".go" extension
	if len(inputFileName) < 3 || inputFileName[len(inputFileName)-3:] != ".go" {
		log.Fatalln("Invalid file extension for input source file: please provide a .go file as first argument")
	}

	// TODO: Load options from command line arguments
	options := Options{
		indentSpaces:    4,
		preferVarDecl:   true,
		parseCgoTargets: false,
		showParseTree:   true,
	}

	fset := token.NewFileSet()

	files := []*ast.File{}

	// TODO: Handle option to parse all files in the package (dir)
	file, err := parser.ParseFile(fset, inputFileName, nil, parser.ParseComments|parser.SkipObjectResolution)

	files = append(files, file)

	if err != nil {
		log.Fatalf("Failed to parse input source file \"%s\": %s\n", inputFileName, err)
	}

	if options.showParseTree {
		ast.Fprint(os.Stdout, fset, file, nil)
	}

	conf := types.Config{Importer: importer.Default()}

	info := &types.Info{
		Types: make(map[ast.Expr]types.TypeAndValue),
		Defs:  make(map[*ast.Ident]types.Object),
		Uses:  make(map[*ast.Ident]types.Object),
	}

	pkg, err := conf.Check(".", fset, files, info)

	if err != nil {
		log.Fatalf("Failed to parse types from input source file \"%s\": %s\n", inputFileName, err)
	}

	var outputFileName string

	if len(os.Args) > 2 {
		// If the user has provided a second argument, we will use it as the output file
		outputFileName = strings.TrimSpace(os.Args[2])
	} else {
		// Otherwise, output file will replace ".go" with ".cs"
		outputFileName = inputFileName[:len(inputFileName)-3] + ".cs"
	}

	outputFile, err := os.Create(outputFileName)

	if err != nil {
		log.Fatalf("Failed to create output source file \"%s\": %s\n", outputFileName, err)
	}

	defer outputFile.Close()

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

		// BlockStmt variable initializations
		blocks:                    Stack[*strings.Builder]{},
		blockInnerPrefixInjection: Stack[string]{},
		blockInnerSuffixInjection: Stack[string]{},
		blockOuterPrefixInjection: Stack[string]{},
		blockOuterSuffixInjection: Stack[string]{},
		identEscapesHeap:          map[*ast.Ident]bool{},
	}

	// TODO: To consider a package as a whole, all files in the package should
	// be parsed and processed. Since global variables could be defined in any
	// file in the package, we need to process `performGlobalVariableAnalysis`
	// for all files in the package before further processing files with the
	// `visitFile` function.

	visitor.visitFile(file)

	outputFile.WriteString(visitor.targetFile.String())
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

func getSanitizedIdentifier(identifier string) string {
	if keywords.Contains(identifier) ||
		strings.HasPrefix(identifier, AddressPrefix) ||
		strings.HasSuffix(identifier, ClassSuffix) ||
		strings.Contains(identifier, ShadowVarMarker) {
		return "@" + identifier
	}

	// Handle special exceptions
	if identifier == "Main" {
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

func (v *Visitor) getTypeName(expr ast.Expr, underlying bool) string {
	exprType := v.info.TypeOf(expr)

	if underlying {
		exprType = exprType.Underlying()
	}

	return getTypeName(exprType)
}

func getTypeName(t types.Type) string {
	if named, ok := t.(*types.Named); ok {
		return named.Obj().Name()
	}

	return t.String()
}

func getIdentifier(node ast.Node) *ast.Ident {
	var ident *ast.Ident

	if indexExpr, ok := node.(*ast.IndexExpr); ok {
		if identExpr, ok := indexExpr.X.(*ast.Ident); ok {
			ident = identExpr
		}
	} else if identExpr, ok := node.(*ast.Ident); ok {
		ident = identExpr
	}

	return ident
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
	case "float":
		return "float64"
	case "complex64":
		return "go.complex64"
	case "string":
		return "go.@string"
	case "interface{}":
		return "object"
	default:
		return typeName
	}
}

func (v *Visitor) convertToHeapTypeDecl(ident *ast.Ident) string {
	escapesHeap := v.identEscapesHeap[ident]
	identType := v.info.TypeOf(ident)

	if !escapesHeap || isInherentlyHeapAllocatedType(identType) {
		return ""
	}

	goTypeName := getTypeName(identType)
	csIDName := getSanitizedIdentifier(v.getIdentName(ident))

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
	case *types.Map, *types.Slice, *types.Chan, *types.Interface, *types.Signature:
		// Maps, slices, channels, interfaces, and functions are reference types
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
