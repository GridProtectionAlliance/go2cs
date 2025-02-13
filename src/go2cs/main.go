package main

import (
	"bytes"
	"embed"
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
	"path"
	"path/filepath"
	"sort"
	"strings"
	"sync"
	"time"
	"unicode"
	"unicode/utf8"
)

type Options struct {
	indentSpaces        int
	preferVarDecl       bool
	useChannelOperators bool
	includeComments     bool
	parseCgoTargets     bool
	showParseTree       bool
}

type FileEntry struct {
	file             *ast.File
	filePath         string
	identEscapesHeap map[types.Object]bool
}

// CapturedVarInfo tracks information about captured variables
type CapturedVarInfo struct {
	origIdent *ast.Ident // Original identifier
	copyIdent *ast.Ident // Temporary copy identifier
	varType   types.Type // Type of the variable
	used      bool       // Whether the capture has been used
}

// LambdaCapture handles analysis and tracking of captured variables
type LambdaCapture struct {
	capturedVars    map[*ast.Ident]*CapturedVarInfo  // Map of original idents to their capture info
	stmtCaptures    map[ast.Node]map[*ast.Ident]bool // Track which vars are captured by which stmt
	pendingCaptures map[string]*CapturedVarInfo      // Variables that need declarations before lambda

	currentLambdaVars map[string]string // Original var name to capture name tracking within current lambda

	// Analysis phase tracking
	analysisInLambda  bool     // Currently analyzing a lambda
	currentLambda     ast.Node // Current lambda being analyzed
	detectingCaptures bool

	// Conversion phase tracking
	conversionInLambda bool     // Currently converting a lambda
	currentConversion  ast.Node // Current node being converted
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
	liftedTypeNames    HashSet[string]
	liftedTypeMap      map[types.Type]string
	subStructTypes     map[types.Type][]types.Type

	// ImportSpec variables
	currentImportPath     string
	packageImports        *strings.Builder
	importQueue           HashSet[string]
	requiredUsings        HashSet[string]
	typeAliasDeclarations *strings.Builder

	// FuncDecl variables
	inFunction        bool
	currentFuncDecl   *ast.FuncDecl
	currentFuncType   *types.Func
	currentFuncName   string
	currentFuncPrefix *strings.Builder
	paramNames        HashSet[string]
	varNames          map[*types.Var]string
	hasDefer          bool
	hasRecover        bool
	capturedVarCount  map[string]int
	tempVarCount      map[string]int

	// BlockStmt variables
	blocks                 Stack[*strings.Builder]
	firstStatementIsReturn bool
	lastStatementWasReturn bool
	identEscapesHeap       map[types.Object]bool
	identNames             map[*ast.Ident]string   // Local identifiers to adjusted names map
	isReassigned           map[*ast.Ident]bool     // Local identifiers to reassignment status map
	scopeStack             []map[string]*types.Var // Stack of local variable scopes
	lambdaCapture          *LambdaCapture          // Lambda capture tracking
}

const RootNamespace = "go"
const PackageSuffix = "_package"
const OutputTypeMarker = ">>MARKER:OUTPUT_TYPE<<"
const PackageInfoFileName = "package_info.cs"

// Extended Unicode characters are being used to help avoid conflicts with Go identifiers for
// symbols, markers, intermediate and temporary variables. These characters have to be valid
// C# identifiers, i.e., Unicode letter characters, decimal digit characters, connecting
// characters, combining characters, or formatting characters. Some character variants will
// be better suited to different fonts or display environments. Defaults have been chosen
// based on better appearance with the Visual Studio default code font "Cascadia Mono":

const PointerPrefix = "\u0436"               // Variants: ж Ж ǂ
const AddressPrefix = "\u13D1"               // Variants: Ꮡ ꝸ
const ShadowVarMarker = "\u0394"             // Variants: Δ Ʌ ꞥ
const CapturedVarMarker = "\u0297"           // Variants: ʗ ɔ ᴄ
const TempVarMarker = "\u1D1B"               // Variants: ᴛ Ŧ ᵀ
const TrueMarker = "\u1427"                  // Variants: ᐧ true
const OverloadDiscriminator = "\uA7F7"       // Variants: ꟷ false
const ElipsisOperator = "\uA4F8\uA4F8\uA4F8" // Variants: ꓸꓸꓸ ᐧᐧᐧ
const TypeAliasDot = "\uA4F8"                // Variants: ꓸ
const ChannelLeftOp = "\u1438\uA7F7"         // Example: `ch.ᐸꟷ(val)` for `ch <- val`
const ChannelRightOp = "\uA7F7\u1433"        // Example: `ch.ꟷᐳ(out var val)` for `val := <-ch`

var keywords = NewHashSet([]string{
	// The following are all valid C# keywords and types, when encountered in Go code they should be
	// escaped with an `@` prefix which allows them to be used as identifiers in C#:
	"abstract", "as", "base", "catch", "char", "checked", "class", "const", "decimal", "delegate", "do",
	"double", "enum", "event", "explicit", "extern", "finally", "fixed", "foreach", "float", "implicit",
	"in", "internal", "is", "lock", "long", "namespace", "null", "object", "operator", "out", "override",
	"params", "private", "protected", "public", "readonly", "ref", "sbyte", "sealed", "short", "sizeof",
	"stackalloc", "static", "this", "throw", "try", "typeof", "ulong", "unchecked", "unsafe", "ushort",
	"using", "virtual", "void", "volatile", "while", "__argslist", "__makeref", "__reftype", "__refvalue",

	// The following C# types overlap with Go types, however, Go unnamed fields in structs will use type
	// name as the field name, so these should also be escaped with an `@` when encountered:
	"bool", "byte", "int", "string", "uint",

	// The remaining C# keywords overlap with Go keywords, so they do not need detection:
	// "break", "case", "const", "continue", "default", "else", "false", "for" "goto", "if",
	// "interface", "new", "return", "select", "struct", "switch", "true", "var"
})

// The following names are reserved by go2cs, if encountered in Go code they should be escaped with `Δ`
var reserved = NewHashSet([]string{
	"array", "channel", "ConvertToType", "GetGoTypeName", "GoFunc", "GoFuncRoot", "GoImplement",
	"GoImplementAttribute", "GoImplicitConv", "GoImplicitConvAttribute", "GoPackage", "GoPackageAttribute",
	"GoRecv", "GoRecvAttribute", "GoTestMatchingConsoleOutput", "GoTestMatchingConsoleOutputAttribute",
	"GoTag", "GoTagAttribute", "GoTypeAlias", "GoTypeAliasAttribute", "GoType", "GoTypeAttribute",
	"GoUntyped", "go\u01C3", "slice",
})

//go:embed csproj-template.xml
var csprojTemplate []byte

//go:embed package_info-template.txt
var packageInfoTemplate []byte

//go:embed go2cs.ico
var iconFileBytes []byte

//go:embed go2cs.png
var pngFileBytes []byte

//go:embed profiles/*
var publishProfiles embed.FS

// Define package level variables
var packageName string
var exportedTypeAliases = map[string]string{}
var interfaceImplementations = map[string]HashSet[string]{}
var promotedInterfaceImplementations = map[string]HashSet[string]{}
var interfaceInheritances = map[string]HashSet[string]{}
var implicitConversions = map[string]HashSet[string]{}
var initFuncCounter int
var packageLock = sync.Mutex{}

func main() {
	commandLine := flag.NewFlagSet(os.Args[0], flag.ContinueOnError)
	commandLine.SetOutput(io.Discard)

	// Define command line flags for options
	indentSpaces := commandLine.Int("indent", 4, "Number of spaces for indentation")
	preferVarDecl := commandLine.Bool("var", true, "Prefer \"var\" declarations")
	useChannelOperators := commandLine.Bool("uco", true, fmt.Sprintf("Use channel operators: %s / %s", ChannelLeftOp, ChannelRightOp))
	includeComments := commandLine.Bool("comments", false, "Include comments in output")
	parseCgoTargets := commandLine.Bool("cgo", false, "Parse cgo targets")
	showParseTree := commandLine.Bool("tree", false, "Show parse tree")
	csprojFile := commandLine.String("csproj", "", "Path to custom .csproj template file")

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
		indentSpaces:        *indentSpaces,
		preferVarDecl:       *preferVarDecl,
		useChannelOperators: *useChannelOperators,
		includeComments:     *includeComments,
		parseCgoTargets:     *parseCgoTargets,
		showParseTree:       *showParseTree,
	}

	// Load custom .csproj template if specified
	if *csprojFile != "" {
		var err error
		csprojTemplate, err = os.ReadFile(*csprojFile)

		if err != nil {
			log.Fatalf("Failed to read custom .csproj template file \"%s\": %s\n", *csprojFile, err)
		}
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

	outputFilePath := ""

	// If the user has provided a second argument, we will use it as the output directory or file
	if commandLine.NArg() > 1 {
		outputFilePath = strings.TrimSpace(commandLine.Arg(1))
	} else {
		outputFilePath = inputFilePath
	}

	var projectFileName string

	if fileInfo.IsDir() {
		// If the input is a directory, write project files (if needed)
		if projectFileName, err = writeProjectFiles(filepath.Base(inputFilePath), outputFilePath); err != nil {
			log.Fatalf("Failed to write project files for directory \"%s\": %s\n", outputFilePath, err)
		} else {
			// Parse all .go files in the directory
			err := filepath.Walk(inputFilePath, func(path string, info os.FileInfo, err error) error {
				if err != nil {
					return err
				}

				if !info.IsDir() && strings.HasSuffix(info.Name(), ".go") {
					file, err := parser.ParseFile(fset, path, nil, parseMode)

					if err != nil {
						return fmt.Errorf("failed to parse input source file \"%s\": %s", path, err)
					}

					files = append(files, FileEntry{file, path, map[types.Object]bool{}})
				}

				return nil
			})

			if err != nil {
				log.Fatalf("Failed to parse files in directory \"%s\": %s\n", inputFilePath, err)
			}
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

		files = append(files, FileEntry{file, inputFilePath, map[types.Object]bool{}})
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

	// Once we have the package details, we can determine the assembly output type
	outputType := getAssemblyOutputType(pkg)

	// Update project file with correct output type
	if len(projectFileName) > 0 {
		projectContents, err := os.ReadFile(projectFileName)

		if err != nil {
			log.Fatalf("Failed to read project file %q: %s", projectFileName, err)
		}

		// Replace the output type marker with the actual output type
		newContents := []byte(strings.ReplaceAll(string(projectContents), OutputTypeMarker, outputType))

		// Rewrite project file atomically
		err = os.WriteFile(projectFileName, newContents, 0644)

		if err != nil {
			log.Fatalf("Failed to write project file %q: %s", projectFileName, err)
		}

		// For executable projects, write OS-specific publish profiles
		if outputType == "Exe" {
			err = writePublishProfiles(outputFilePath)

			if err != nil {
				log.Fatalf("Failed to write publish profiles for project \"%s\": %s\n", outputFilePath, err)
			}
		}

		// For library projects, write package files, like icon
		if outputType == "Library" {
			err = writePackageFiles(outputFilePath)

			if err != nil {
				log.Fatalf("Failed to write package files for project \"%s\": %s\n", outputFilePath, err)
			}
		}
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

	// Perform escape analysis for each file
	for i := range files {
		visitor := &Visitor{
			fset:             fset,
			pkg:              pkg,
			info:             info,
			identEscapesHeap: files[i].identEscapesHeap,
		}

		ast.Inspect(files[i].file, func(n ast.Node) bool {
			switch node := n.(type) {
			case *ast.FuncDecl:
				ast.Inspect(node.Body, func(n ast.Node) bool {
					switch n := n.(type) {
					case *ast.AssignStmt:
						if n.Tok == token.DEFINE {
							for _, lhs := range n.Lhs {
								if ident := getIdentifier(lhs); ident != nil {
									visitor.performEscapeAnalysis(ident, node.Body)
								}
							}
						}
					case *ast.RangeStmt:
						if n.Tok == token.DEFINE {
							if key := getIdentifier(n.Key); key != nil {
								visitor.performEscapeAnalysis(key, node.Body)
							}
							if value := getIdentifier(n.Value); value != nil {
								visitor.performEscapeAnalysis(value, node.Body)
							}
						}
					case *ast.DeclStmt:
						if genDecl, ok := n.Decl.(*ast.GenDecl); ok {
							for _, spec := range genDecl.Specs {
								if valueSpec, ok := spec.(*ast.ValueSpec); ok {
									for _, ident := range valueSpec.Names {
										if !isDiscardedVar(ident.Name) {
											visitor.performEscapeAnalysis(ident, node.Body)
										}
									}
								}
							}
						}
					case *ast.ForStmt:
						if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
							for _, lhs := range init.Lhs {
								if ident := getIdentifier(lhs); ident != nil {
									visitor.performEscapeAnalysis(ident, node.Body)
								}
							}
						}
					case *ast.IfStmt:
						if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
							for _, lhs := range init.Lhs {
								if ident := getIdentifier(lhs); ident != nil {
									visitor.performEscapeAnalysis(ident, node.Body)
								}
							}
						}
					case *ast.SwitchStmt:
						if init, ok := n.Init.(*ast.AssignStmt); ok && init.Tok == token.DEFINE {
							for _, lhs := range init.Lhs {
								if ident := getIdentifier(lhs); ident != nil {
									visitor.performEscapeAnalysis(ident, node.Body)
								}
							}
						}
					case *ast.TypeSwitchStmt:
						if assign, ok := n.Assign.(*ast.AssignStmt); ok && assign.Tok == token.DEFINE {
							for _, lhs := range assign.Lhs {
								if ident := getIdentifier(lhs); ident != nil {
									visitor.performEscapeAnalysis(ident, node.Body)
								}
							}
						}
					}
					return true
				})
			}
			return true
		})
	}

	var concurrentTasks sync.WaitGroup

	for _, fileEntry := range files {
		concurrentTasks.Add(1)

		go func(fileEntry FileEntry) {
			defer concurrentTasks.Done()

			visitor := &Visitor{
				fset:                  fset,
				pkg:                   pkg,
				info:                  info,
				targetFile:            &strings.Builder{},
				liftedTypeNames:       HashSet[string]{},
				liftedTypeMap:         map[types.Type]string{},
				subStructTypes:        map[types.Type][]types.Type{},
				packageImports:        &strings.Builder{},
				requiredUsings:        HashSet[string]{},
				importQueue:           HashSet[string]{},
				typeAliasDeclarations: &strings.Builder{},
				standAloneComments:    map[token.Pos]string{},
				sortedCommentPos:      []token.Pos{},
				processedComments:     HashSet[token.Pos]{},
				newline:               "\r\n",
				options:               options,
				globalIdentNames:      globalIdentNames,
				globalScope:           globalScope,
				blocks:                Stack[*strings.Builder]{},
				identEscapesHeap:      fileEntry.identEscapesHeap,
			}

			visitor.visitFile(fileEntry.file)

			var outputFileName string

			if fileInfo.IsDir() {
				outputFileName = filepath.Join(outputFilePath, strings.TrimSuffix(filepath.Base(fileEntry.filePath), ".go")+".cs")
			} else {
				outputFileName = strings.TrimSuffix(outputFilePath, ".go") + ".cs"
			}

			if err := visitor.writeOutputFile(outputFileName); err != nil {
				log.Printf("%s\n", err)
			}
		}(fileEntry)
	}

	concurrentTasks.Wait()

	var packageInfoFileName string

	// Handle package information file
	if fileInfo.IsDir() {
		packageInfoFileName = filepath.Join(outputFilePath, PackageInfoFileName)
	} else {
		packageInfoFileName = filepath.Join(filepath.Dir(outputFilePath), PackageInfoFileName)
	}

	var packageInfoLines []string

	if _, err := os.Stat(packageInfoFileName); err == nil {
		// Read all lines from existing package info file
		packageInfoBytes, err := os.ReadFile(packageInfoFileName)

		if err != nil {
			log.Fatalf("Failed to read existing package info file \"%s\": %s\n", packageInfoFileName, err)
		}

		packageInfoLines = strings.Split(string(packageInfoBytes), "\r\n")
	} else {
		// Generate new package info file from template
		templateFile := fmt.Sprintf(string(packageInfoTemplate), packageName, packageName, packageName)
		packageInfoLines = strings.Split(templateFile, "\r\n")
	}

	// Handle exported type aliases
	startLineIndex := -1
	endLineIndex := -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<ExportedTypeAliases>") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</ExportedTypeAliases>") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		// Read existing type aliases from package info file
		lines := HashSet[string]{}

		// If processing a single file, instead of all package files, merge type aliases
		if !fileInfo.IsDir() {
			for i := startLineIndex + 1; i < endLineIndex; i++ {
				line := packageInfoLines[i]
				lines.Add(strings.TrimSpace(line))
			}
		}

		// Add new type aliases to package info file (hashset ensures uniqueness)
		for alias, typeName := range exportedTypeAliases {
			lines.Add(fmt.Sprintf("[assembly: GoTypeAlias(\"%s\", \"%s\")]", alias, typeName))
		}

		// Sort lines
		sortedLines := lines.Keys()
		sort.Strings(sortedLines)

		// Insert exported type aliases into package info file
		packageInfoLines = append(packageInfoLines[:startLineIndex+1],
			append(sortedLines, packageInfoLines[endLineIndex:]...)...)
	} else {
		log.Fatalf("Failed to find '<ExportedTypeAliases>...</ExportedTypeAliases>' section for inserting exported type aliases into package info file \"%s\"\n", packageInfoFileName)
	}

	// Handle interface implementations
	startLineIndex = -1
	endLineIndex = -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<InterfaceImplementations>") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</InterfaceImplementations>") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		// Read existing interface lines from package info file
		lines := HashSet[string]{}

		// If processing a single file, instead of all package files, merge interface implementations
		if !fileInfo.IsDir() {
			for i := startLineIndex + 1; i < endLineIndex; i++ {
				line := packageInfoLines[i]
				lines.Add(strings.TrimSpace(line))
			}
		}

		// Drop lower level interface implementations where interface inheritances are already covered
		for interfaceName, inheritedInterfaces := range interfaceInheritances {
			for _, inheritedInterfaceName := range inheritedInterfaces.Keys() {
				// Check if the same type implements both interfaces
				if inheritedImplementations, ok := interfaceImplementations[inheritedInterfaceName]; ok {
					if baseImplementations, ok := interfaceImplementations[interfaceName]; ok {
						baseImplementations.IntersectWithSet(inheritedImplementations)
						for _, implementation := range baseImplementations.Keys() {
							implementedTypes := interfaceImplementations[inheritedInterfaceName]
							implementedTypes.Remove(implementation)
						}
					}
				}
			}
		}

		// Add new interface implementations to package info file (hashset ensures uniqueness)
		for interfaceName, implementations := range interfaceImplementations {
			for implementation := range implementations {
				lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s>]", implementation, interfaceName))
			}
		}

		// Add new promoted interface implementations to package info file (hashset ensures uniqueness)
		for interfaceName, implementations := range promotedInterfaceImplementations {
			for implementation := range implementations {
				lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s>(Promoted = true)]", implementation, interfaceName))
			}
		}

		// Sort lines
		sortedLines := lines.Keys()
		sort.Strings(sortedLines)

		// Insert interface implementations into package info file
		packageInfoLines = append(packageInfoLines[:startLineIndex+1],
			append(sortedLines, packageInfoLines[endLineIndex:]...)...)

	} else {
		log.Fatalf("Failed to find '<InterfaceImplementations>...</InterfaceImplementations>' section for inserting interface implementations into package info file \"%s\"\n", packageInfoFileName)
	}

	// Handle implicit conversions
	startLineIndex = -1
	endLineIndex = -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<ImplicitConversions>") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</ImplicitConversions>") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		// Read existing interface lines from package info file
		lines := HashSet[string]{}

		// If processing a single file, instead of all package files, merge implicit conversions
		if !fileInfo.IsDir() {
			for i := startLineIndex + 1; i < endLineIndex; i++ {
				line := packageInfoLines[i]
				lines.Add(strings.TrimSpace(line))
			}
		}

		// Add new implicit conversions to package info file (hashset ensures uniqueness)
		for sourceType, targetTypes := range implicitConversions {
			for targetType := range targetTypes {
				lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>]", sourceType, targetType))
			}
		}

		// Sort lines
		sortedLines := lines.Keys()
		sort.Strings(sortedLines)

		// Insert implicit conversions into package info file
		packageInfoLines = append(packageInfoLines[:startLineIndex+1],
			append(sortedLines, packageInfoLines[endLineIndex:]...)...)

	} else {
		log.Fatalf("Failed to find '<ImplicitConversions>...</ImplicitConversions>' section for inserting implicit conversions into package info file \"%s\"\n", packageInfoFileName)
	}

	// Remove trailing empty lines
	for i := len(packageInfoLines) - 1; i >= 0; i-- {
		if strings.TrimSpace(packageInfoLines[i]) == "" {
			packageInfoLines = packageInfoLines[:i]
		} else {
			break
		}
	}

	// Write updated package info file
	packageInfoFile, err := os.Create(packageInfoFileName)

	if err != nil {
		log.Fatalf("Failed to create package info file \"%s\": %s\n", packageInfoFileName, err)
	}

	defer packageInfoFile.Close()

	for _, line := range packageInfoLines {
		_, err = packageInfoFile.WriteString(line + "\r\n")

		if err != nil {
			log.Fatalf("Failed to write to package info file \"%s\": %s\n", packageInfoFileName, err)
		}
	}
}

func writeProjectFiles(projectName string, projectPath string) (string, error) {
	// Make sure project path ends with a directory separator
	projectPath = strings.TrimRight(projectPath, string(filepath.Separator)) + string(filepath.Separator)

	iconFileName := projectPath + "go2cs.ico"

	// Check if icon file needs to be written
	if needToWriteFile(iconFileName, iconFileBytes) {
		iconFile, err := os.Create(iconFileName)

		if err != nil {
			return "", fmt.Errorf("failed to create icon file \"%s\": %s", iconFileName, err)
		}

		defer iconFile.Close()

		_, err = iconFile.Write(iconFileBytes)

		if err != nil {
			return "", fmt.Errorf("failed to write to icon file \"%s\": %s", iconFileName, err)
		}
	}

	// TODO: Need to know which projects to reference based on package imports.
	// Original src path location to referenced project can be determined by using:
	//     go list -f '{{.Standard}}:{{.Dir}}' "import/path/package/name"
	// This returns `std:dir`, where `std` is `true` if package is in the standard
	// library and `dir` is the Go source code directory of the package.

	// Generate project file contents
	projectFileContents := fmt.Sprintf(string(csprojTemplate),
		OutputTypeMarker,
		projectName,
		time.Now().Year())

	projectFileName := projectPath + projectName + ".csproj"

	// Check if project file needs to be written
	if needToWriteFile(projectFileName, []byte(projectFileContents)) {
		projectFile, err := os.Create(projectFileName)

		if err != nil {
			return "", fmt.Errorf("failed to create project file \"%s\": %s", projectFileName, err)
		}

		_, err = projectFile.WriteString(projectFileContents)

		if err != nil {
			return "", fmt.Errorf("failed to write to project file \"%s\": %s", projectFileName, err)
		}

		defer projectFile.Close()
	}

	return projectFileName, nil
}

func writePackageFiles(projectPath string) error {
	// Make sure project path ends with a directory separator
	projectPath = strings.TrimRight(projectPath, string(filepath.Separator)) + string(filepath.Separator)

	pngFileName := projectPath + "go2cs.png"

	// Check if icon file needs to be written
	if needToWriteFile(pngFileName, pngFileBytes) {
		iconFile, err := os.Create(pngFileName)

		if err != nil {
			return fmt.Errorf("failed to create package icon file \"%s\": %s", pngFileName, err)
		}

		defer iconFile.Close()

		_, err = iconFile.Write(pngFileBytes)

		if err != nil {
			return fmt.Errorf("failed to write to package icon file \"%s\": %s", pngFileName, err)
		}
	}

	return nil
}

func writePublishProfiles(projectPath string) error {
	// Make sure "Properties/PublishProfiles" directory exists
	publishProfilesDir := filepath.Join(projectPath, "Properties", "PublishProfiles")

	if err := os.MkdirAll(publishProfilesDir, 0755); err != nil {
		return fmt.Errorf("failed to create directory \"%s\": %s", publishProfilesDir, err)
	}

	// Get list of publish profiles
	profiles, err := publishProfiles.ReadDir("profiles")

	if err != nil {
		return fmt.Errorf("failed to read publish profiles: %s", err)
	}

	// Write each publish profile file
	for _, profile := range profiles {
		profileBytes, err := publishProfiles.ReadFile(path.Join("profiles", profile.Name()))

		if err != nil {
			return fmt.Errorf("failed to read publish profile \"%s\": %s", profile.Name(), err)
		}

		profileFileName := filepath.Join(publishProfilesDir, profile.Name())

		// Check if profile file already exists - user may change default parameters, so we don't overwrite
		if _, err := os.Stat(profileFileName); err == nil {
			continue
		}

		profileFile, err := os.Create(profileFileName)

		if err != nil {
			return fmt.Errorf("failed to create publish profile \"%s\": %s", profileFileName, err)
		}

		defer profileFile.Close()

		_, err = profileFile.Write(profileBytes)

		if err != nil {
			return fmt.Errorf("failed to write to publish profile \"%s\": %s", profileFileName, err)
		}
	}

	return nil
}

func needToWriteFile(fileName string, fileBytes []byte) bool {
	existingFileBytes, err := os.ReadFile(fileName)

	if err != nil {
		return true
	}

	return !bytes.Equal(existingFileBytes, fileBytes)
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

func getAssemblyOutputType(pkg *types.Package) string {
	if hasMainFunction(pkg) {
		return "Exe"
	}

	return "Library"
}

func hasMainFunction(pkg *types.Package) bool {
	// First check if this is a main package
	if pkg.Name() != "main" {
		return false
	}

	// Look through all objects in the package scope
	scope := pkg.Scope()
	mainObj := scope.Lookup("main")

	if mainObj == nil {
		return false
	}

	// Check if it's a function
	mainFunc, ok := mainObj.(*types.Func)

	if !ok {
		return false
	}

	// Get the function's type
	funcType, ok := mainFunc.Type().(*types.Signature)

	if !ok {
		return false
	}

	// main function should have no parameters and no return values
	return funcType.Params().Len() == 0 && funcType.Results().Len() == 0
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

			// Multiline C# raw string literals start and end with newlines
			prefix += v.newline
			suffix = v.newline + suffix

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
	if strings.HasPrefix(identifier, "@") || strings.HasPrefix(identifier, ShadowVarMarker) {
		return identifier // Already sanitized
	}

	// Remove pointer dereference operator if present
	identifier = strings.TrimPrefix(identifier, "*")

	if keywords.Contains(identifier) {
		return "@" + identifier
	}

	if reserved.Contains(identifier) || strings.HasSuffix(identifier, PackageSuffix) {
		return ShadowVarMarker + identifier
	}

	return identifier
}

func getSanitizedFunctionName(funcName string) string {
	funcName = getSanitizedIdentifier(funcName)

	// Handle special exceptions
	if funcName == "Main" {
		// C# "Main" method name is reserved, so we need to
		// shadow it if Go code has a function named "Main"
		return ShadowVarMarker + "Main"
	}

	return funcName
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

func (v *Visitor) isInterface(ident *ast.Ident) (result bool, empty bool) {
	obj := v.info.ObjectOf(ident)

	if obj == nil {
		return false, false
	}

	return isInterface(obj.Type())
}

func isInterface(t types.Type) (result bool, empty bool) {
	exprType := t.Underlying()

	if interfaceType, ok := exprType.(*types.Interface); ok {
		// Empty interface has zero methods
		return true, interfaceType.NumMethods() == 0
	}

	return false, false
}

func (v *Visitor) isPointer(ident *ast.Ident) bool {
	obj := v.info.ObjectOf(ident)

	if obj == nil {
		return false
	}

	return isPointer(obj.Type())
}

func isPointer(t types.Type) bool {
	exprType := t.Underlying()

	_, isPointer := exprType.(*types.Pointer)

	return isPointer
}

func paramsAreInterfaces(paramTypes *types.Tuple, andNotEmptyInterface bool) []bool {
	if paramTypes == nil {
		return nil
	}

	paramIsInterface := make([]bool, paramTypes.Len())

	for i := 0; i < paramTypes.Len(); i++ {
		param := paramTypes.At(i)
		paramType := param.Type()
		isInterface, isEmpty := isInterface(paramType)

		if andNotEmptyInterface {
			paramIsInterface[i] = isInterface && !isEmpty
		} else {
			paramIsInterface[i] = isInterface
		}
	}

	return paramIsInterface
}

func paramsArePointers(paramTypes *types.Tuple) []bool {
	if paramTypes == nil {
		return nil
	}

	paramIsPointer := make([]bool, paramTypes.Len())

	for i := 0; i < paramTypes.Len(); i++ {
		param := paramTypes.At(i)
		paramIsPointer[i] = isPointer(param.Type())
	}

	return paramIsPointer
}

func (v *Visitor) convertExprToInterfaceType(interfaceExpr ast.Expr, targetExpr ast.Expr, exprResult string) string {
	// Target selector or index expression source if this source of the interface expression
	if selectorExpr, ok := interfaceExpr.(*ast.SelectorExpr); ok {
		interfaceExpr = selectorExpr.Sel
	} else if indexExpr, ok := interfaceExpr.(*ast.IndexExpr); ok {
		interfaceExpr = indexExpr.X
	}

	return v.convertToInterfaceType(v.getType(interfaceExpr, false), v.getType(targetExpr, false), exprResult)
}

func (v *Visitor) convertToInterfaceType(interfaceType types.Type, targetType types.Type, exprResult string) string {
	// Track interface types that need to an implementation mapping
	// to properly handle duck typed Go interface implementations
	interfaceTypeName := convertToCSTypeName(v.getFullTypeName(interfaceType, false))
	targetTypeName := convertToCSTypeName(v.getFullTypeName(targetType, false))

	var prefix string

	if strings.HasPrefix(targetTypeName, PointerPrefix+"<") {
		targetTypeName = targetTypeName[3 : len(targetTypeName)-1]
		prefix = "~"
	}

	if interfaceTypeName != "" && interfaceTypeName != "nil" && targetTypeName != "" && targetTypeName != "nil" {
		packageLock.Lock()

		if implementations, exists := interfaceImplementations[interfaceTypeName]; exists {
			implementations.Add(targetTypeName)
		} else {
			interfaceImplementations[interfaceTypeName] = NewHashSet([]string{targetTypeName})
		}

		packageLock.Unlock()
	}

	return prefix + exprResult
}

// getUnderlyingType attempts to get the concrete type underneath an interface type
func (v *Visitor) getUnderlyingType(expr ast.Expr) types.Type {
	typ := v.info.TypeOf(expr)
	if typ == nil {
		return nil
	}

	// If it's already a concrete type, return it
	if _, isInterface := typ.Underlying().(*types.Interface); !isInterface {
		return typ
	}

	// Get the type and value information
	tv, ok := v.info.Types[expr]
	if !ok {
		return nil
	}

	// The concrete type is available in the type checker's type-and-value info
	if tv.IsValue() {
		return tv.Type
	}

	return nil
}

func getIdentifier(node ast.Node) *ast.Ident {
	var ident *ast.Ident

	if identExpr, ok := node.(*ast.Ident); ok {
		ident = identExpr
	} else if indexExpr, ok := node.(*ast.IndexExpr); ok {
		return getIdentifier(indexExpr.X)
	} else if starExpr, ok := node.(*ast.StarExpr); ok {
		ident = getIdentifier(starExpr.X)
	} else if chanExpr, ok := node.(*ast.ChanType); ok {
		ident = getIdentifier(chanExpr.Value)
	} else if arrayExpr, ok := node.(*ast.ArrayType); ok {
		ident = getIdentifier(arrayExpr.Elt)
	} else if mapExpr, ok := node.(*ast.MapType); ok {
		ident = getIdentifier(mapExpr.Key)
	}

	// TODO: Other types expected to have an identifier
	/*
		} else if funcExpr, ok := node.(*ast.FuncType); ok {
			ident = getIdentifier(funcExpr.Results)
		}
	*/

	return ident
}

func (v *Visitor) getIdentType(ident *ast.Ident) types.Type {
	// First check Types map
	if typeAndValue, exists := v.info.Types[ident]; exists {
		return typeAndValue.Type
	}

	// If not in Types, check Uses map
	if obj := v.info.Uses[ident]; obj != nil {
		return obj.Type()
	}

	return nil
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

func (v *Visitor) getExprTypeName(expr ast.Expr, underlying bool) string {
	return v.getTypeName(v.getType(expr, underlying), underlying)
}

func (v *Visitor) getTypeName(t types.Type, isUnderlying bool) string {
	if pointer, ok := t.(*types.Pointer); ok {
		return "*" + v.getTypeName(pointer.Elem(), isUnderlying)
	}

	if name, ok := v.liftedTypeMap[t]; ok {
		return name
	}

	if named, ok := t.(*types.Named); ok {
		return named.Obj().Name()
	}

	if !isUnderlying {
		if _, ok := t.(*types.Struct); ok {
			println(fmt.Sprintf("WARNING: Unresolved dynamic struct type: %s", t.String()))
		}
	}

	return strings.ReplaceAll(t.String(), "..", "")
}

func (v *Visitor) getFullTypeName(t types.Type, isUnderlying bool) string {
	if pointer, ok := t.(*types.Pointer); ok {
		if name, ok := v.liftedTypeMap[pointer.Elem()]; ok {
			return "*" + name
		}
	}

	if name, ok := v.liftedTypeMap[t]; ok {
		return name
	}

	if named, ok := t.(*types.Named); ok {
		obj := named.Obj()
		pkg := obj.Pkg()

		if pkg != nil && pkg.Name() != packageName {
			return pkg.Name() + PackageSuffix + "." + obj.Name()
		}

		return obj.Name()
	}

	if !isUnderlying {
		if _, ok := t.(*types.Struct); ok {
			println(fmt.Sprintf("WARNING: Unresolved dynamic struct type: %s", t.String()))
		}
	}

	return strings.ReplaceAll(t.String(), "..", "")
}

func (v *Visitor) getCSTypeName(t types.Type) string {
	return convertToCSTypeName(v.getTypeName(t, false))
}

func (v *Visitor) getRefParamTypeName(t types.Type) string {
	typeName := v.getTypeName(t, false)

	if strings.HasPrefix(typeName, "*") {
		return fmt.Sprintf("ref %s", convertToCSTypeName(typeName[1:]))
	}

	return convertToCSTypeName(typeName)
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
		return fmt.Sprintf("%s.slice<%s>", RootNamespace, convertToCSTypeName(typeName[2:]))
	}

	// Handle array types
	if strings.HasPrefix(typeName, "[") {
		return fmt.Sprintf("%s.array<%s>", RootNamespace, convertToCSTypeName(typeName[strings.Index(typeName, "]")+1:]))
	}

	if strings.HasPrefix(typeName, "map[") {
		keyValue := strings.Split(typeName[4:], "]")
		return fmt.Sprintf("%s.map<%s, %s>", RootNamespace, convertToCSTypeName(keyValue[0]), convertToCSTypeName(keyValue[1]))
	}

	if strings.HasPrefix(typeName, "chan ") {
		return fmt.Sprintf("%s.channel<%s>", RootNamespace, convertToCSTypeName(typeName[5:]))
	}

	if strings.HasPrefix(typeName, "chan<- ") {
		return fmt.Sprintf("%s.channel/*<-*/<%s>", RootNamespace, convertToCSTypeName(typeName[7:]))
	}

	if strings.HasPrefix(typeName, "<-chan ") {
		return fmt.Sprintf("%s./*<-*/channel<%s>", RootNamespace, convertToCSTypeName(typeName[7:]))
	}

	if typeName == "func()" {
		return "Action"
	}

	if strings.HasPrefix(typeName, "func(") {
		// Find the matching closing parenthesis for the parameter list
		depth := 0
		closingParenIndex := -1

		for i := 5; i < len(typeName); i++ {
			if typeName[i] == '(' {
				depth++
			} else if typeName[i] == ')' {
				depth--
				if depth == -1 {
					closingParenIndex = i
					break
				}
			}
		}

		if closingParenIndex == -1 {
			return "Action" // Malformed input (unexpected)
		}

		// Extract parameter types, handling nested functions
		paramString := typeName[5:closingParenIndex]
		paramTypes := extractTypes(paramString)

		// Convert parameter types to C#
		csTypeNames := make([]string, len(paramTypes))

		for i, pType := range paramTypes {
			csTypeNames[i] = convertToCSTypeName(pType)
		}

		// Check for return type after the closing parenthesis
		remainingType := strings.TrimSpace(typeName[closingParenIndex+1:])

		if len(remainingType) > 0 {
			// Has explicit return type
			csReturnType := convertToCSTypeName(remainingType)

			if len(csTypeNames) > 0 {
				return fmt.Sprintf("Func<%s, %s>", strings.Join(csTypeNames, ", "), csReturnType)
			}

			return fmt.Sprintf("Func<%s>", csReturnType)
		}

		// No return type, use Action
		if len(csTypeNames) > 0 {
			return fmt.Sprintf("Action<%s>", strings.Join(csTypeNames, ", "))
		}

		return "Action"
	}

	// Handle pointer types
	if strings.HasPrefix(typeName, "*") {
		return fmt.Sprintf("%s.%s<%s>", RootNamespace, PointerPrefix, convertToCSTypeName(typeName[1:]))
	}

	switch typeName {
	case "int":
		return "nint"
	case "uint":
		return "nuint"
	case "bool":
		return "bool"
	case "byte":
		return "byte"
	case "float":
		return "float64"
	case "complex64":
		return RootNamespace + ".complex64"
	case "string":
		return RootNamespace + ".@string"
	case "interface{}":
		return "object"
	default:
		return fmt.Sprintf("%s.%s", RootNamespace, getSanitizedIdentifier(typeName))
	}
}

func (v *Visitor) extractStructType(expr ast.Expr) (*ast.StructType, types.Type) {
	if starExpr, ok := expr.(*ast.StarExpr); ok {
		if structType, ok := starExpr.X.(*ast.StructType); ok {
			return structType, v.getType(starExpr.X, false)
		}
	} else if structType, ok := expr.(*ast.StructType); ok {
		return structType, v.getType(expr, false)
	}

	return nil, nil
}

func extractTypes(signature string) []string {
	// Remove any whitespace at the ends
	signature = strings.TrimSpace(signature)

	// Handle empty signature
	if signature == "" {
		return []string{}
	}

	// Split the signature into individual parameter declarations
	params := strings.Split(signature, ",")
	types := make([]string, 0, len(params))

	for _, param := range params {
		// Trim whitespace
		param = strings.TrimSpace(param)

		// Find the first space or end of string
		var typeStart int

		for i, char := range param {
			if unicode.IsSpace(char) {
				typeStart = i
				break
			}
		}

		// If no space found, the entire param is a type (e.g., "string")
		if typeStart == 0 {
			types = append(types, param)
		} else {
			// Extract everything after the space
			paramType := convertToCSTypeName(strings.TrimSpace(param[typeStart:]))
			types = append(types, paramType)
		}
	}

	return types
}

func (v *Visitor) convertToHeapTypeDecl(ident *ast.Ident, createNew bool) string {
	identType := v.info.TypeOf(ident)

	// Check both Defs and Uses maps
	obj := v.info.Defs[ident]

	if obj == nil {
		obj = v.info.Uses[ident]
	}

	if obj != nil {
		escapesHeap := v.identEscapesHeap[obj]

		if !escapesHeap || isInherentlyHeapAllocatedType(identType) {
			return ""
		}
	}

	goTypeName := v.getFullTypeName(identType, false)
	csIDName := v.getIdentName(ident)

	// Handle array types
	if strings.HasPrefix(goTypeName, "[") {
		arrayLen := strings.Split(goTypeName[1:], "]")[0]

		// Get array element type
		arrayType := convertToCSTypeName(goTypeName[strings.Index(goTypeName, "]")+1:])

		if v.options.preferVarDecl {
			if createNew {
				return fmt.Sprintf("ref var %s = ref heap(new array<%s>(%s), out var %s%s);", csIDName, arrayType, arrayLen, AddressPrefix, csIDName)
			}

			return fmt.Sprintf("ref var %s = ref heap<array<%s>>(out var %s%s);", csIDName, arrayType, AddressPrefix, csIDName)
		}

		if createNew {
			return fmt.Sprintf("ref array<%s> %s = ref heap(new array<%s>(%s), out %s<array<%s>> %s%s);", arrayType, csIDName, arrayType, arrayLen, PointerPrefix, arrayType, AddressPrefix, csIDName)
		}

		return fmt.Sprintf("ref array<%s> %s = ref heap<array<%s>>(out %s%s);", arrayType, csIDName, arrayType, AddressPrefix, csIDName)
	}

	csTypeName := convertToCSTypeName(goTypeName)

	if v.options.preferVarDecl {
		if createNew {
			return fmt.Sprintf("ref var %s = ref heap(new %s(), out var %s%s);", csIDName, csTypeName, AddressPrefix, csIDName)
		}

		return fmt.Sprintf("ref var %s = ref heap<%s>(out var %s%s);", csIDName, csTypeName, AddressPrefix, csIDName)
	}

	if createNew {
		return fmt.Sprintf("ref %s %s = ref heap(out %s<%s> %s%s);", csTypeName, csIDName, PointerPrefix, csTypeName, AddressPrefix, csIDName)
	}

	return fmt.Sprintf("ref %s %s = ref heap<%s>(out %s%s);", csTypeName, csIDName, csTypeName, AddressPrefix, csIDName)
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

func (v *Visitor) getExprType(expr ast.Expr) types.Type {
	return v.info.TypeOf(expr)
}

// Get the adjusted identifier name, considering captures and shadowing
func (v *Visitor) getIdentName(ident *ast.Ident) string {
	// Check if we're in a lambda conversion
	if v.lambdaCapture != nil && v.lambdaCapture.conversionInLambda {
		// First check if we already have a mapping for this variable in this lambda
		if captureName, ok := v.lambdaCapture.currentLambdaVars[ident.Name]; ok {
			return captureName
		}

		// Then check if it needs to be captured
		if captureInfo, ok := v.lambdaCapture.capturedVars[ident]; ok {
			captureInfo.used = true

			// Store the mapping for this lambda
			v.lambdaCapture.currentLambdaVars[ident.Name] = captureInfo.copyIdent.Name

			return captureInfo.copyIdent.Name
		}
	}

	// Fall back to existing shadowing logic
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
