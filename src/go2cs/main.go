package main

import (
	"bytes"
	"embed"
	"flag"
	"fmt"
	"go/ast"
	"go/build"
	"go/constant"
	"go/printer"
	"go/token"
	"go/types"
	"io"
	"log"
	"os"
	"os/exec"
	"path"
	"path/filepath"
	"runtime"
	"sort"
	"strconv"
	"strings"
	"sync"
	"time"
	"unicode"
	"unicode/utf8"

	"golang.org/x/tools/go/packages"
)

type Options struct {
	goRoot              string
	goPath              string
	go2csPath           string
	convertStdLib       bool
	targetPlatform      string
	indentSpaces        int
	preferVarDecl       bool
	useChannelOperators bool
	includeComments     bool
	parseCgoTargets     bool
	showParseTree       bool
	debugMode           bool
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
	inFunction           bool
	currentFuncDecl      *ast.FuncDecl
	currentFuncSignature *types.Signature
	currentFuncName      string
	currentFuncPrefix    *strings.Builder
	paramNames           HashSet[string]
	varNames             map[*types.Var]string
	hasDefer             bool
	hasRecover           bool
	captureReceiver      bool
	useUnsafeFunc        bool
	capturedVarCount     map[string]int
	tempVarCount         map[string]int

	// BlockStmt variables
	blocks                 Stack[*strings.Builder]
	firstStatementIsReturn bool
	lastStatementWasReturn bool
	lastReturnIndentLevel  int
	identEscapesHeap       map[types.Object]bool
	identNames             map[*ast.Ident]string   // Local identifiers to adjusted names map
	isReassigned           map[*ast.Ident]bool     // Local identifiers to reassignment status map
	scopeStack             []map[string]*types.Var // Stack of local variable scopes
	lambdaCapture          *LambdaCapture          // Lambda capture tracking
}

const RootNamespace = "go"
const PackageSuffix = "_package"
const OutputTypeMarker = ">>MARKER:OUTPUT_TYPE<<"
const UnsafeMarker = ">>MARKER:UNSAFE<<"
const ProjectReferenceMarker = ">>MARKER:PROJECT_REFERENCE<<"
const DynamicCastArgMarker = ">>MARKER:DYNAMIC_CAST_ARG<<"
const PackageInfoFileName = "package_info.cs"
const MaxSupportedGoVersion = 23

// Extended Unicode characters are being used to help avoid conflicts with Go identifiers for
// symbols, markers, intermediate and temporary variables. These characters have to be valid
// C# identifiers, i.e., Unicode letter characters, decimal digit characters, connecting
// characters, combining characters, or formatting characters. Some character variants will
// be better suited to different fonts or display environments. Defaults have been chosen
// based on better appearance with common Visual Studio code fonts, e.g., "Cascadia Mono".
// Note: keep constants in sync with go2cs-gen source code generator and golib core.
const PointerPrefix = "\u0436"                // Variants: ж Ж ǂ
const AddressPrefix = "\u13D1"                // Variants: Ꮡ ꝸ
const ShadowVarMarker = "\u0394"              // Variants: Δ Ʌ ꞥ
const CapturedVarMarker = "\u0297"            // Variants: ʗ ɔ ᴄ
const TempVarMarker = "\u1D1B"                // Variants: ᴛ Ŧ ᵀ
const TrueMarker = "\u1427"                   // Variants: ᐧ true
const OverloadDiscriminator = "\uA7F7"        // Variants: ꟷ false
const EllipsisOperator = "\uA4F8\uA4F8\uA4F8" // Variants: ꓸꓸꓸ ᐧᐧᐧ
const TypeAliasDot = "\uA4F8"                 // Variants: ꓸ
const ChannelLeftOp = "\u1438\uA7F7"          // Example: `ch.ᐸꟷ(val)` for `ch <- val`
const ChannelRightOp = "\uA7F7\u1433"         // Example: `ch.ꟷᐳ(out var val)` for `val := <-ch`
const PointerDerefOp = "~"                    // Example: `~ptr` for dereferencing a pointer

var keywords = NewHashSet([]string{
	// The following are all valid C# keywords and types, when encountered in Go code they should be
	// escaped with an `@` prefix which allows them to be used as identifiers in C#:
	"abstract", "as", "base", "catch", "char", "checked", "class", "const", "decimal", "delegate", "do", "double",
	"enum", "event", "explicit", "extern", "finally", "fixed", "foreach", "float", "implicit", "in", "internal",
	"is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private",
	"protected", "public", "readonly", "ref", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "this",
	"throw", "try", "typeof", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile",
	"while", "__argslist", "__makeref", "__reftype", "__refvalue",

	// The following C# types overlap with Go types, however, Go unnamed fields in structs will use type
	// name as the field name, so these should also be escaped with an `@` when encountered:
	"bool", "byte", "int", "string", "uint",

	// The remaining C# keywords overlap with Go keywords, so they do not need detection:
	// "break", "case", "const", "continue", "default", "else", "false", "for" "goto", "if",
	// "interface", "return", "select", "struct", "switch", "true", "var"
})

// The following names are reserved by go2cs or C#, if encountered in Go code, prefix with `Δ`:
// Note that "_" is used for type assertion functions in go2cs converted C# code, but it is not
// a valid method name in Go, so it is not included in the reserved list.
var reserved = NewHashSet([]string{
	"AreEqual", "array", "channel", "defer\u01C3", "Equals", "Finalize", "GetGoTypeName", "GetHashCode",
	"GetType", "GoFunc", "GoFuncRoot", "GoImplement", "GoImplementAttribute", "GoImplicitConv",
	"GoImplicitConvAttribute", "GoPackage", "GoPackageAttribute", "GoRecv", "GoRecvAttribute",
	"GoTestMatchingConsoleOutput", "GoTestMatchingConsoleOutputAttribute", "GoTag", "GoTagAttribute",
	"GoTypeAlias", "GoTypeAliasAttribute", "GoType", "GoTypeAttribute", "GoUntyped", "go\u01C3",
	"IArray", "IChannel", "IMap", "ISlice", "ISupportMake", "make\u01C3", "MemberwiseClone", "NilType",
	"PanicException", "PrintPointer", "slice", "ToString", "ToUTF8Bytes", "TryCastAsInteger", "type",
	"UntypedInt", "UntypedFloat", "UntypedComplex",
	PointerPrefix, TrueMarker, OverloadDiscriminator, EllipsisOperator,
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
var packageNamespace string
var projectImports HashSet[string]
var exportedTypeAliases map[string]string
var importedTypeAliases map[string]string
var constImportedTypeAliases HashSet[string]
var parsedPackageInfoFiles HashSet[string]
var interfaceImplementations map[string]HashSet[string]
var promotedInterfaceImplementations map[string]HashSet[string]
var interfaceInheritances map[string]HashSet[string]
var implicitConversions map[string]HashSet[string]
var invertedImplicitConversions map[string]HashSet[string]
var indirectImplicitConversions map[string]HashSet[string]
var numericConversions map[string]map[string]string
var indirectNumericConversions map[string]map[string]string
var nameCollisions map[string]bool
var initFuncCounter int
var usesUnsafeCode bool
var packageLock = sync.Mutex{}

func main() {
	var goRoot, goPath, go2csPath string
	var err error

	// Resolve GOROOT and GOPATH variables, any defined environment
	// variables will take precedence over derived values and command
	// line flags will override all
	if goRoot = os.Getenv("GOROOT"); len(goRoot) == 0 {
		if goRoot, err = getGoEnv("GOROOT"); err != nil {
			goRoot = runtime.GOROOT()
		}

		if len(goRoot) == 0 {
			log.Fatalln("Failed to resolve GOROOT path")
		}

		os.Setenv("GOROOT", goRoot)
	}

	if goPath = os.Getenv("GOPATH"); len(goPath) == 0 {
		if goPath, err = getGoEnv("GOPATH"); err != nil {
			goPath = build.Default.GOPATH
		}

		if len(goPath) == 0 {
			log.Fatalln("Failed to resolve GOPATH path")
		}

		os.Setenv("GOPATH", goPath)
	}

	// Resolve GO2CSPATH environment variable
	if go2csPath = os.Getenv("GO2CSPATH"); len(go2csPath) == 0 {
		homeDir, err := os.UserHomeDir()

		if err != nil {
			homeDir = strings.TrimSuffix(strings.TrimSuffix(goPath, "go"), string(os.PathSeparator))
		}

		go2csPath = filepath.Join(homeDir, "go2cs")

		os.Setenv("GO2CSPATH", go2csPath)
	}

	// Define command line flags for options
	commandLine := flag.NewFlagSet(os.Args[0], flag.ContinueOnError)
	commandLine.SetOutput(io.Discard)

	goRootCmd := commandLine.String("goroot", goRoot, "Path to Go root directory")
	goPathCmd := commandLine.String("gopath", goPath, "Path to Go path directory")
	go2csPathCmd := commandLine.String("go2cspath", go2csPath, "Path to C# converted code")
	convertStdLibCmd := commandLine.Bool("stdlib", false, "Convert Go standard library")
	parallelProcCmd := commandLine.Int("parallel", 1, "Number of packages to convert in parallel (1-4)")
	targetPlatformCmd := commandLine.String("platforms", fmt.Sprintf("%s/%s", runtime.GOOS, runtime.GOARCH), "Target platform for conversion, format: os/arch")
	indentSpacesCmd := commandLine.Int("indent", 4, "Number of spaces for indentation")
	preferVarDeclCmd := commandLine.Bool("var", true, "Prefer \"var\" declarations")
	useChannelOperatorsCmd := commandLine.Bool("uco", true, fmt.Sprintf("Use channel operators: %s / %s", ChannelLeftOp, ChannelRightOp))
	includeCommentsCmd := commandLine.Bool("comments", false, "Include comments in output")
	parseCgoTargetsCmd := commandLine.Bool("cgo", false, "Parse cgo targets")
	showParseTreeCmd := commandLine.Bool("tree", false, "Show parse tree")
	csprojFileCmd := commandLine.String("csproj", "", "Path to custom .csproj template file")
	debugModeCmd := commandLine.Bool("debug", false, "Enable debug mode")

	err = commandLine.Parse(os.Args[1:])

	var inputFilePath string
	var convertStdLib bool

	if err == nil {
		convertStdLib = *convertStdLibCmd
	}

	if !convertStdLib {
		inputFilePath = strings.TrimSpace(commandLine.Arg(0))
	}

	if err != nil || (!convertStdLib && len(inputFilePath) == 0) {
		if err != nil {
			fmt.Fprintf(os.Stderr, "Error: %s\n", err)
		}

		fmt.Fprintln(os.Stderr, `
 Usage: go2cs [options] <input_dir> [output_dir]
 
 Options:`)

		commandLine.SetOutput(nil)
		commandLine.PrintDefaults()

		fmt.Fprintln(os.Stderr, `
Examples:
  go2cs -indent 2 -var=false example.go conv/example.cs
  go2cs example.go
  go2cs -cgo=true input_dir output_dir
  go2cs package_dir
  go2cs -stdlib                           # Convert the entire Go standard library
  go2cs -stdlib fmt io/ioutil strings     # Convert specific standard library packages
 `)
		os.Exit(1)
	}

	options := Options{
		goRoot:              *goRootCmd,
		goPath:              *goPathCmd,
		go2csPath:           *go2csPathCmd,
		convertStdLib:       convertStdLib,
		targetPlatform:      *targetPlatformCmd,
		indentSpaces:        *indentSpacesCmd,
		preferVarDecl:       *preferVarDeclCmd,
		useChannelOperators: *useChannelOperatorsCmd,
		includeComments:     *includeCommentsCmd,
		parseCgoTargets:     *parseCgoTargetsCmd,
		showParseTree:       *showParseTreeCmd,
		debugMode:           *debugModeCmd,
	}

	// Load custom .csproj template if specified
	if *csprojFileCmd != "" {
		var err error
		csprojTemplate, err = os.ReadFile(*csprojFileCmd)

		if err != nil {
			log.Fatalf("Failed to read custom .csproj template file \"%s\": %s\n", *csprojFileCmd, err)
		}
	}

	// Set parallel processing environment variable if specified
	if *parallelProcCmd > 1 {
		// Limit to 4 parallel processes
		parallelCount := *parallelProcCmd

		if parallelCount > 4 {
			parallelCount = 4
		}

		os.Setenv("GO2CS_PARALLEL", strconv.Itoa(parallelCount))
	}

	if convertStdLib {
		// Initialize standard library converter
		converter := NewStdLibConverter(options)

		// Check if specific packages are specified
		var packageFilter []string

		if commandLine.NArg() > 0 {
			packageFilter = make([]string, commandLine.NArg())

			for i := range commandLine.NArg() {
				packageFilter[i] = strings.TrimSpace(commandLine.Arg(i))
			}

			fmt.Printf("Only converting specified packages: %s\n", strings.Join(packageFilter, ", "))
		}

		// Run the conversion process
		if err := converter.ScanAndConvertFiltered(packageFilter); err != nil {
			log.Fatalf("Standard library conversion failed: %v", err)
		}
	} else {
		// Check if the input is a file or a directory
		fileInfo, err := os.Stat(inputFilePath)

		if err != nil {
			log.Fatalf("Failed to access input file path \"%s\": %s\n", inputFilePath, err)
		}

		isDir := fileInfo.IsDir()

		if !isDir {
			inputFilePath = filepath.Dir(inputFilePath)
		}

		var outputFilePath string

		// If the user has provided a second argument, we will use it as the output directory or file
		if commandLine.NArg() > 1 {
			outputFilePath = strings.TrimSpace(commandLine.Arg(1))
		} else {
			outputFilePath = inputFilePath
		}

		inputFilePath, err = filepath.Abs(inputFilePath)

		if err != nil {
			log.Fatalf("Failed to get absolute file path \"%s\": %s\n", inputFilePath, err)
			return
		}

		processConversion(inputFilePath, isDir, outputFilePath, options)
	}
}

func processConversion(inputFilePath string, isDir bool, outputFilePath string, options Options) {
	var err error

	cfg := &packages.Config{
		Mode: packages.LoadAllSyntax,
		Dir:  inputFilePath,
	}

	targetParts := strings.Split(options.targetPlatform, "/")

	if len(targetParts) != 2 {
		log.Fatalf("Invalid target platform format: %s\n", options.targetPlatform)
	}

	cfg.Env = append(os.Environ(), fmt.Sprintf(`"GOOS=%s", "GOARCH=%s"`, targetParts[0], targetParts[1]))

	var pkgs []*packages.Package

	if strings.HasPrefix(strings.ToLower(inputFilePath), strings.ToLower(options.goPath)) {
		pkgs, err = packages.Load(cfg, "./...")
	} else {
		pkgs, err = packages.Load(cfg, inputFilePath)
	}

	for _, pkg := range pkgs {
		if len(pkg.Errors) > 0 {
			log.Printf("Errors: %v", pkg.Errors)
		}
	}

	if err != nil {
		log.Fatalf("Failed to parse files in directory \"%s\": %s\n", inputFilePath, err)
	}

	for _, pkg := range pkgs {
		// Reset package level variables for each package
		packageName = ""
		packageNamespace = ""
		projectImports = NewHashSet([]string{})
		exportedTypeAliases = make(map[string]string)
		importedTypeAliases = make(map[string]string)
		constImportedTypeAliases = NewHashSet([]string{})
		parsedPackageInfoFiles = NewHashSet([]string{})
		interfaceImplementations = make(map[string]HashSet[string])
		promotedInterfaceImplementations = make(map[string]HashSet[string])
		interfaceInheritances = make(map[string]HashSet[string])
		implicitConversions = make(map[string]HashSet[string])
		invertedImplicitConversions = make(map[string]HashSet[string])
		indirectImplicitConversions = make(map[string]HashSet[string])
		numericConversions = make(map[string]map[string]string)
		indirectNumericConversions = make(map[string]map[string]string)
		nameCollisions = make(map[string]bool)
		initFuncCounter = 0
		usesUnsafeCode = false

		files := []FileEntry{}
		fset := pkg.Fset
		packageTypes := pkg.Types
		info := pkg.TypesInfo

		packageInputPath := inputFilePath
		packageOutputPath := outputFilePath

		if len(pkg.Dir) > 0 && pkg.Dir != packageInputPath {
			// Adjust output path if the input is a subdirectory of the package directory
			subPath := strings.Replace(pkg.Dir, packageInputPath, "", 1)
			packageOutputPath = filepath.Join(packageOutputPath, subPath)
			packageInputPath = pkg.Dir
		}

		var projectName, projectFileName, projectFileContents string
		projectName, packageNamespace = getProjectName(packageInputPath, options)

		if projectFileName, projectFileContents, err = prepareProjectFiles(projectName, packageNamespace, packageOutputPath); err != nil {
			log.Fatalf("Failed to write project files for directory \"%s\": %s\n", packageOutputPath, err)
		} else {
			for i, file := range pkg.Syntax {
				path := pkg.GoFiles[i]

				if match, err := CheckBuildConstraints(path, options.targetPlatform); err != nil {
					showWarning("Failed to evaluate build constraints for file \"%s\": %s", path, err)
				} else if !match {
					// Skipping file due to non-matching build constraints
					continue
				}

				// See if output already exists and has been marked as manually converted
				outputFileName := filepath.Join(packageOutputPath, strings.TrimSuffix(filepath.Base(path), ".go")+".cs")
				manualConv, err := containsManualConversionMarker(outputFileName)

				if err != nil {
					log.Fatalf("Failed to check for manual conversion in file \"%s\": %s\n", outputFileName, err)
				}

				if !manualConv {
					files = append(files, FileEntry{file, path, map[types.Object]bool{}})
				}
			}
		}

		if len(files) == 0 {
			showMessage("Skipping conversion: no target Go source files found for conversion in input path \"%s\"", packageInputPath)
			continue
		}

		globalIdentNames := make(map[*ast.Ident]string)
		globalScope := map[string]*types.Var{}

		// Perform name collision analysis
		performNameCollisionAnalysis(pkg)

		// Pre-process all global variables in package
		for _, fileEntry := range files {
			performGlobalVariableAnalysis(fileEntry.file.Decls, info, globalIdentNames, globalScope)

			if options.showParseTree {
				ast.Fprint(os.Stdout, fset, fileEntry.file, nil)
			}
		}

		// Perform escape analysis for each file
		performEscapeAnalysis(files, fset, packageTypes, info)

		var concurrentTasks sync.WaitGroup

		for _, fileEntry := range files {
			concurrentTasks.Add(1)

			go func(fileEntry FileEntry) {
				defer func() {
					if !options.debugMode {
						if r := recover(); r != nil {
							showWarning("visit file error: %v in \"%s\"", r, filepath.Base(fileEntry.filePath))
						}
					}

					concurrentTasks.Done()
				}()

				visitor := &Visitor{
					fset:                  fset,
					pkg:                   packageTypes,
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

				if isDir {
					outputFileName = filepath.Join(packageOutputPath, strings.TrimSuffix(filepath.Base(fileEntry.filePath), ".go")+".cs")
				} else {
					outputFileName = strings.TrimSuffix(packageOutputPath, ".go") + ".cs"
				}

				if err := visitor.writeOutputFile(outputFileName); err != nil {
					log.Printf("%s\n", err)
				}

				packageLock.Lock()
				projectImports.UnionWithSet(visitor.importQueue)
				packageLock.Unlock()
			}(fileEntry)
		}

		concurrentTasks.Wait()

		// Write project file with correct output type and unsafe code settings
		err = writeProjectFile(projectFileName, projectFileContents, packageOutputPath, packageTypes, options)

		if err != nil {
			log.Fatalf("Error while writing project file \"%s\": %s\n", projectFileName, err)
		}

		var packageInfoFileName string

		// Handle package information file
		if isDir {
			packageInfoFileName = filepath.Join(packageOutputPath, PackageInfoFileName)
		} else {
			packageInfoFileName = filepath.Join(filepath.Dir(packageOutputPath), PackageInfoFileName)
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
			packageClassName := getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))
			templateFile := fmt.Sprintf(string(packageInfoTemplate), packageNamespace+"."+packageClassName, packageNamespace, packageName, packageClassName)
			packageInfoLines = strings.Split(templateFile, "\r\n")
		}

		// Handle imported type aliases
		startLineIndex := -1
		endLineIndex := -1

		for i, line := range packageInfoLines {
			if strings.Contains(line, "<ImportedTypeAliases>") {
				startLineIndex = i
				continue
			}

			if strings.Contains(line, "</ImportedTypeAliases>") {
				endLineIndex = i
				break
			}
		}

		if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
			// Read existing type aliases from package info file
			lines := HashSet[string]{}

			// If processing a single file, instead of all package files, merge type aliases
			if !isDir {
				for i := startLineIndex + 1; i < endLineIndex; i++ {
					line := packageInfoLines[i]
					lines.Add(strings.TrimSpace(line))
				}
			}

			// Add new type aliases to package info file (hashset ensures uniqueness)
			for alias, typeName := range importedTypeAliases {
				if !constImportedTypeAliases.Contains(alias) {
					lines.Add(fmt.Sprintf("global using %s = %s;", strings.ReplaceAll(alias, ".", TypeAliasDot), typeName))
				}
			}

			// Sort lines
			sortedLines := lines.Keys()
			sort.Strings(sortedLines)

			// Insert imported type aliases into package info file
			packageInfoLines = append(packageInfoLines[:startLineIndex+1],
				append(sortedLines, packageInfoLines[endLineIndex:]...)...)
		} else {
			log.Fatalf("Failed to find '<ImportedTypeAliases>...</ImportedTypeAliases>' section for inserting exported type aliases into package info file \"%s\"\n", packageInfoFileName)
		}

		// Handle exported type aliases
		startLineIndex = -1
		endLineIndex = -1

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
			if !isDir {
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
			if !isDir {
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
			if !isDir {
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

			// Add new inverted implicit conversions to package info file (hashset ensures uniqueness)
			for sourceType, targetTypes := range invertedImplicitConversions {
				for targetType := range targetTypes {
					lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Inverted = true)]", targetType, sourceType))
				}
			}

			// Add new indirect implicit conversions to package info file (hashset ensures uniqueness)
			for sourceType, targetTypes := range indirectImplicitConversions {
				for targetType := range targetTypes {
					lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Indirect = true)]", sourceType, targetType))
				}
			}

			// Add new numeric conversions to package info file (maps ensure uniqueness)
			for sourceType, targetTypes := range numericConversions {
				for targetType, valueType := range targetTypes {
					var inverted bool

					if strings.HasPrefix(valueType, "imported:") {
						valueType = strings.TrimPrefix(valueType, "imported:")
						inverted = false
					} else {
						inverted = true
					}

					lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Inverted = %t, ValueType = \"%s\")]", sourceType, targetType, inverted, valueType))
				}
			}

			// Add new indirect numeric conversions to package info file (maps ensure uniqueness)
			for sourceType, targetTypes := range indirectNumericConversions {
				for targetType, valueType := range targetTypes {
					var inverted bool

					if strings.HasPrefix(valueType, "imported:") {
						valueType = strings.TrimPrefix(valueType, "imported:")
						inverted = false
					} else {
						inverted = true
					}

					lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Inverted = %t, Indirect = true, ValueType = \"%s\")]", sourceType, targetType, inverted, valueType))
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
}

func getGoEnv(name string) (string, error) {
	cmd := exec.Command("go", "env", name)
	var out bytes.Buffer

	cmd.Stdout = &out
	err := cmd.Run()

	if err != nil {
		return "", fmt.Errorf("failed to get Go environment %s: %w", name, err)
	}

	return strings.TrimSpace(out.String()), nil
}

// prepareProjectFiles writes the project related files for the given project name and path,
// and returns project file contents with template parameters to be written to a file later.
func prepareProjectFiles(projectName string, packageNamespace string, projectPath string) (string, string, error) {
	// Make sure project path ends with a directory separator
	projectPath = strings.TrimRight(projectPath, string(filepath.Separator)) + string(filepath.Separator)

	// Ensure project directory exists
	if err := os.MkdirAll(projectPath, 0755); err != nil {
		return "", "", fmt.Errorf("failed to create project directory \"%s\": %s", projectPath, err)
	}

	iconFileName := projectPath + "go2cs.ico"

	// Check if icon file needs to be written
	if needToWriteFile(iconFileName, iconFileBytes) {
		iconFile, err := os.Create(iconFileName)

		if err != nil {
			return "", "", fmt.Errorf("failed to create icon file \"%s\": %s", iconFileName, err)
		}

		defer iconFile.Close()

		_, err = iconFile.Write(iconFileBytes)

		if err != nil {
			return "", "", fmt.Errorf("failed to write to icon file \"%s\": %s", iconFileName, err)
		}
	}

	// Generate project file contents
	projectFileContents := fmt.Sprintf(string(csprojTemplate),
		OutputTypeMarker,
		packageNamespace,
		projectName,
		time.Now().Year(),
		UnsafeMarker,
		ProjectReferenceMarker,
	)

	projectFileName := projectPath + projectName + ".csproj"

	return projectFileName, projectFileContents, nil
}

func writeProjectFile(projectFileName string, projectFileContents string, outputFilePath string, pkg *types.Package, options Options) error {
	// Get assembly output type from the package details
	outputType := getAssemblyOutputType(pkg)

	// Replace the output type marker with the actual output type
	newContents := []byte(strings.ReplaceAll(string(projectFileContents), OutputTypeMarker, outputType))

	// Replace the unsafe code marker with the actual unsafe code setting
	newContents = []byte(strings.ReplaceAll(string(newContents), UnsafeMarker, strconv.FormatBool(usesUnsafeCode)))

	// Extract project references from imports
	packageInfoMap := getImportPackageInfo(projectImports.Keys(), options)
	projectReferences := &strings.Builder{}

	// Ensure project references are sorted so that the project file output is deterministic
	references := make([]string, 0, len(packageInfoMap))

	for _, info := range packageInfoMap {
		reference := info.ProjectReference

		if len(reference) == 0 {
			continue
		}

		// Track project references
		references = append(references, reference)

		// Load imported type aliases for the current package, if not already loaded
		loadImportedTypeAliases(info)
	}

	sort.Strings(references)

	// Build project references XML
	for _, reference := range references {
		projectReferences.WriteString(fmt.Sprintf("\r\n    <ProjectReference Include=\"%s\" />", reference))
	}

	// Replace the project reference marker with the actual project references
	newContents = []byte(strings.ReplaceAll(string(newContents), ProjectReferenceMarker, projectReferences.String()))

	// Check if project file needs to be written
	if needToWriteFile(projectFileName, newContents) {
		// Write project file atomically
		err := os.WriteFile(projectFileName, newContents, 0644)

		if err != nil {
			return fmt.Errorf("failed to write project file: %s", err)
		}
	}

	// For executable projects, write OS-specific publish profiles
	if outputType == "Exe" {
		err := writePublishProfiles(outputFilePath)

		if err != nil {
			return fmt.Errorf("failed to write publish profiles for project \"%s\": %s", outputFilePath, err)
		}
	}

	// For library projects, write package files, like icon
	if outputType == "Library" {
		err := writePackageFiles(outputFilePath)

		if err != nil {
			return fmt.Errorf("failed to write package files for project \"%s\": %s", outputFilePath, err)
		}
	}

	return nil
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
	if pkg == nil {
		return false
	}

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
	if node == nil {
		return ""
	}

	result := &strings.Builder{}
	printer.Fprint(result, v.fset, node)
	return result.String()
}

func showMessage(format string, a ...interface{}) {
	message := fmt.Sprintf(format, a...)
	os.Stdout.WriteString(fmt.Sprintf("INFO: %s\n", message))
}

func (v *Visitor) showMessage(format string, a ...interface{}) {
	message := fmt.Sprintf(format, a...)
	showMessage("%s in \"%s\"", message, getShortFileName(v.file))
}

func showWarning(format string, a ...interface{}) {
	message := fmt.Sprintf(format, a...)
	os.Stderr.WriteString(fmt.Sprintf("WARNING: %s\n", message))
}

func (v *Visitor) showWarning(format string, a ...interface{}) {
	message := fmt.Sprintf(format, a...)
	showWarning("%s in \"%s\"", message, getShortFileName(v.file))
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

	// Get the type and value information
	tv, ok := v.info.Types[expr]

	if !ok {
		return false
	}

	return tv.IsValue() && !isStringLiteral(tv) && !isCallExpr
}

// isStringType determines if an expression is either a string literal or a string variable
func (v *Visitor) isStringType(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.BasicLit:
		// Direct string literal
		return e.Kind == token.STRING

	case *ast.BinaryExpr:
		// Handle string concatenation
		if e.Op != token.ADD {
			return false
		}

		// Both sides must be string types for the result to be a string
		return v.isStringType(e.X) && v.isStringType(e.Y)

	case *ast.Ident, *ast.SelectorExpr:
		// Variable or field access - check type info
		tv, ok := v.info.Types[expr]

		if !ok {
			return false
		}

		return isStringType(tv.Type)

	case *ast.IndexExpr, *ast.SliceExpr:
		// Slice expressions are not string literals or variables
		return false

	case *ast.CallExpr:
		// For function calls, check the return type
		tv, ok := v.info.Types[expr]

		if !ok {
			return false
		}

		return isStringType(tv.Type)

	case *ast.ParenExpr:
		// Handle parenthesized expressions
		return v.isStringType(e.X)
	}

	// For any other expression type, use type information
	tv, ok := v.info.Types[expr]

	if !ok {
		return false
	}

	return isStringType(tv.Type)
}

// isStringType checks if a type is a string type
func isStringType(t types.Type) bool {
	if t == nil {
		return false
	}

	// Handle basic types
	if basic, ok := t.Underlying().(*types.Basic); ok {
		return basic.Kind() == types.String
	}

	return false
}

// isStringLiteral specifically checks if the expression is a string literal (not a variable)
func isStringLiteral(tv types.TypeAndValue) bool {
	// Must be a constant value
	if !tv.IsValue() || tv.Value == nil {
		return false
	}

	// Must be a string constant
	if tv.Value.Kind() != constant.String {
		return false
	}

	// Type must be string
	return isStringType(tv.Type)
}

func getSanitizedImport(identifier string) string {
	if strings.HasPrefix(identifier, "@") {
		return identifier // Already sanitized
	}

	if keywords.Contains(identifier) {
		return "@" + identifier
	}

	return identifier
}

func getSanitizedIdentifier(identifier string) string {
	if nameCollisions[identifier] {
		return getCollisionAvoidanceIdentifier(identifier)
	}

	return getCoreSanitizedIdentifier(identifier)
}

func getCollisionAvoidanceIdentifier(identifier string) string {
	return ShadowVarMarker + identifier
}

func getCoreSanitizedIdentifier(identifier string) string {
	if strings.Contains(identifier, ".") {
		// Split identifiers based on dot separator and sanitize each part
		parts := strings.Split(identifier, ".")

		if len(parts) > 1 {
			for i, part := range parts {
				if i == len(parts)-1 {
					parts[i] = getCoreSanitizedIdentifier(part)
				} else {
					parts[i] = getSanitizedImport(part)
				}
			}

			return strings.Join(parts, ".")
		}
	}

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

func removeSanitizationMarker(identifier string) string {
	if strings.HasPrefix(identifier, "@") {
		return identifier[1:] // Remove "@" prefix
	}

	return identifier
}

func getSanitizedFunctionName(funcName string) string {
	funcName = getCoreSanitizedIdentifier(funcName)

	// Handle special exceptions
	if funcName == "Main" {
		// C# "Main" method name is reserved, so we need to
		// shadow it if Go code has a function named "Main"
		return ShadowVarMarker + "Main"
	}

	return funcName
}

func getAccess(name string) string {
	name = strings.TrimPrefix(name, "ref ")

	// If name starts with a lowercase letter, scope is "internal"
	ch, _ := utf8.DecodeRuneInString(name)

	if unicode.IsLower(ch) || ch == '_' {
		return "internal"
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

func isEmptyInterface(interfaceType *ast.InterfaceType) bool {
	if interfaceType == nil {
		return false
	}

	// Empty interface has no methods
	return len(interfaceType.Methods.List) == 0
}

func (v *Visitor) isDynamicInterface(expr ast.Expr) bool {
	return isDynamicInterface(v.getType(expr, false))
}

func isDynamicInterface(t types.Type) bool {
	if t == nil {
		return false
	}

	// If it's a pointer, get its element.
	if ptr, ok := t.(*types.Pointer); ok {
		t = ptr.Elem()
	}

	// If it's a named type, then it’s not dynamic.
	if _, ok := t.(*types.Named); ok {
		return false
	}

	// Finally, check if it is a direct interface.
	_, ok := t.(*types.Interface)

	return ok
}

func (v *Visitor) extractInterfaceType(expr ast.Expr) (*ast.InterfaceType, types.Type) {
	var interfaceType *ast.InterfaceType

	if starExpr, ok := expr.(*ast.StarExpr); ok {
		interfaceType, _ = starExpr.X.(*ast.InterfaceType)
	} else if compositeLit, ok := expr.(*ast.CompositeLit); ok {
		interfaceType, _ = compositeLit.Type.(*ast.InterfaceType)
	} else if arrayType, ok := expr.(*ast.ArrayType); ok {
		interfaceType, _ = arrayType.Elt.(*ast.InterfaceType)
	} else if indexExpr, ok := expr.(*ast.IndexExpr); ok {
		interfaceType, _ = indexExpr.X.(*ast.InterfaceType)
	} else if sliceExpr, ok := expr.(*ast.SliceExpr); ok {
		interfaceType, _ = sliceExpr.X.(*ast.InterfaceType)
	} else if callExpr, ok := expr.(*ast.CallExpr); ok {
		interfaceType, _ = callExpr.Fun.(*ast.InterfaceType)
	} else if typeAssertExpr, ok := expr.(*ast.TypeAssertExpr); ok {
		interfaceType, _ = typeAssertExpr.Type.(*ast.InterfaceType)
	} else if selectorExpr, ok := expr.(*ast.SelectorExpr); ok {
		interfaceType, _ = selectorExpr.X.(*ast.InterfaceType)
	} else if interfaceType, ok = expr.(*ast.InterfaceType); !ok {
		return nil, nil
	}

	if interfaceType == nil {
		return nil, nil
	}

	if isEmptyInterface(interfaceType) {
		return nil, nil
	}

	return interfaceType, v.getType(expr, false)
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

	// Also check for an unsafe.Pointer
	if !isPointer {
		if basic, ok := t.(*types.Basic); ok {
			isPointer = basic.Kind() == types.UnsafePointer
		}
	}

	return isPointer
}

func (v *Visitor) isPointerReceiver() (bool, string) {
	// First check if we're in a function with a receiver
	if !v.inFunction || v.currentFuncSignature.Recv() == nil {
		return false, ""
	}

	// Check if receiver is a pointer type
	recvType := v.currentFuncSignature.Recv().Type()
	isRecvPointer := false

	if _, ok := recvType.(*types.Pointer); ok {
		isRecvPointer = true
	}

	if !isRecvPointer {
		return false, ""
	}

	// Get the name of the receiver variable from the AST
	var recvName string

	if v.currentFuncDecl.Recv != nil && len(v.currentFuncDecl.Recv.List) > 0 {
		// The field might have multiple names for the same type,
		// but for a receiver there should be just one
		if len(v.currentFuncDecl.Recv.List[0].Names) > 0 {
			recvName = v.currentFuncDecl.Recv.List[0].Names[0].Name
		}
	}

	return true, recvName
}

func (v *Visitor) getCapturedReceiverName(recvName string) string {
	if !v.inFunction {
		return ""
	}

	return fmt.Sprintf("%s%s%s%s", v.currentFuncName, TypeAliasDot, AddressPrefix, recvName)
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

	if targetTypeName == "" || targetTypeName == "nil" || targetTypeName == "any" {
		return exprResult
	}

	var prefix string

	if strings.HasPrefix(targetTypeName, PointerPrefix+"<") {
		targetTypeName = targetTypeName[3 : len(targetTypeName)-1]
		prefix = PointerDerefOp
	}

	if interfaceTypeName != "" && interfaceTypeName != "nil" &&
		interfaceTypeName != targetTypeName &&
		interfaceTypeName != "any" &&
		!strings.Contains(targetTypeName, "interface{") {

		packageLock.Lock()

		if implementations, exists := interfaceImplementations[interfaceTypeName]; exists {
			implementations.Add(targetTypeName)
		} else {
			interfaceImplementations[interfaceTypeName] = NewHashSet([]string{targetTypeName})
		}

		packageLock.Unlock()
	}

	if derivedInterfaceType, ok := interfaceType.Underlying().(*types.Interface); ok {
		if targetStructType, ok := targetType.(*types.Named); ok {
			// Iterate over methods of the derived interface looking for struct parameters
			for i := 0; i < derivedInterfaceType.NumMethods(); i++ {
				interfaceMethod := derivedInterfaceType.Method(i)
				interfaceMethodSignature, ok := interfaceMethod.Type().(*types.Signature)

				if !ok {
					continue
				}

				// Lookup matching receiver method for target struct by name
				methodInfo, _, _ := types.LookupFieldOrMethod(types.NewPointer(targetStructType), true, v.pkg, interfaceMethod.Name())

				if methodInfo == nil {
					methodInfo, _, _ = types.LookupFieldOrMethod(targetStructType, true, v.pkg, interfaceMethod.Name())
				}

				if methodInfo == nil {
					continue
				}

				targetMethodSignature, ok := methodInfo.Type().(*types.Signature)

				if !ok {
					continue
				}

				// Iterate over parameters of the interface method
				totalParameters := interfaceMethodSignature.Params().Len()

				for j := 0; j < totalParameters; j++ {
					interfaceParamType := interfaceMethodSignature.Params().At(j).Type().Underlying()
					targetParameterType := targetMethodSignature.Params().At(j).Type().Underlying()

					// Check if targetParamType is a struct or a pointer to a struct
					if ptrType, ok := targetParameterType.(*types.Pointer); ok {
						targetParameterType = ptrType.Elem().Underlying()
					}

					if _, ok := targetParameterType.(*types.Struct); ok {
						// Check if interfaceParamType is a struct or a pointer to a struct
						if ptrType, ok := interfaceParamType.(*types.Pointer); ok {
							interfaceParamType = ptrType.Elem().Underlying()
						}

						if _, ok := interfaceParamType.(*types.Struct); ok {
							// Both interfaceParamType and targetParamType are structs, track implicit conversions
							interfaceParamTypeName := v.getCSTypeName(interfaceParamType)
							targetParamTypeName := v.getCSTypeName(targetParameterType)
							var conversions HashSet[string]
							var exists bool

							packageLock.Lock()

							// For interface methods that have struct parameters, tracked implicit conversions
							// are inverted to allow for implicit conversions from struct to interface
							if conversions, exists = invertedImplicitConversions[interfaceParamTypeName]; exists {
								conversions.Add(targetParamTypeName)
							} else {
								conversions = NewHashSet([]string{targetParamTypeName})
								invertedImplicitConversions[interfaceParamTypeName] = conversions
							}

							packageLock.Unlock()
						}
					}
				}
			}
		}
	}

	// Handle special case for pointer dereference of immediate address of operation, this
	// is an unnecessary operation as it creates a pointer to an object and then immediately
	// dereferences the pointer value, so we can just return the expression result instead
	if prefix == PointerDerefOp {
		if strings.HasPrefix(exprResult, AddressPrefix+"(") {
			return strings.TrimSuffix(strings.TrimPrefix(exprResult, AddressPrefix+"("), ")")
		} else if strings.HasPrefix(exprResult, "@new<") {
			return fmt.Sprintf("new %s()", strings.TrimSuffix(strings.TrimPrefix(exprResult, "@new<"), ">()"))
		}
	}

	return prefix + exprResult
}

func isDynamicStruct(t types.Type) bool {
	if t == nil {
		return false
	}

	// If it's a pointer, get its element.
	if ptr, ok := t.(*types.Pointer); ok {
		t = ptr.Elem()
	}

	// If it's a named type, then it’s not dynamic.
	if _, ok := t.(*types.Named); ok {
		return false
	}

	// Finally, check if it is a direct struct.
	_, ok := t.(*types.Struct)

	return ok
}

func (v *Visitor) checkForDynamicStructs(argType types.Type, targetType types.Type) string {
	if argType == nil || targetType == nil {
		return ""
	}

	// Only proceed if the target type is a dynamic (anonymous) struct
	if !isDynamicStruct(targetType) {
		return ""
	}

	// If targetType is a pointer, get its element and underlying type
	if ptrType, ok := targetType.(*types.Pointer); ok {
		targetType = ptrType.Elem().Underlying()
	}

	if _, ok := targetType.(*types.Struct); ok {
		// Likewise for argType.
		if ptrType, ok := argType.(*types.Pointer); ok {
			argType = ptrType.Elem().Underlying()
		}

		var argTypeName, targetTypeName string

		if _, ok := argType.(*types.Struct); ok {
			// Argument is a dynamic struct and target is a dynamic struct, track implicit conversions
			argTypeName = v.getCSTypeName(argType)
			targetTypeName = v.getCSTypeName(targetType)
		} else if _, ok := argType.(*types.Named); ok {
			// Argument is a named type and target is a dynamic struct, track implicit conversions
			argTypeName = v.getCSTypeName(argType)
			targetTypeName = v.getCSTypeName(targetType)
		}

		if len(argTypeName) > 0 && len(targetTypeName) > 0 {
			// In C#, operators are only allowed to be public, so if target type is
			// private and argument type is public, we need to manually apply conversions
			// instead of relying on implicit conversions
			argScope := getAccess(argTypeName)
			targetScope := getAccess(targetTypeName)

			if argScope == "public" && targetScope == "internal" {
				return v.dynamicCast(argType, targetType, targetTypeName)
			} else {
				// Track implicit conversions
				packageLock.Lock()

				var conversions HashSet[string]
				var exists bool

				if conversions, exists = implicitConversions[argTypeName]; exists {
					conversions.Add(targetTypeName)
				} else {
					conversions = NewHashSet([]string{targetTypeName})
					implicitConversions[argTypeName] = conversions
				}

				packageLock.Unlock()

				v.addImplicitSubStructConversions(argType, targetTypeName, false)
			}
		}
	}

	return ""
}

// dynamicCast generates a C# expression to cast a value of sourceType to targetType
// where both are structs that match "structurally" but are different types. This is
// used only as a fallback operation when no implicit conversion is allowed, e.g.,
// when the source type is public and the anonymous target type is internal. This is
// required since C# does not allow implicit operator conversions between structs
// with differnet access scopes, i.e., all operators in C# must be public, hence the
// types used with an operator must also be public :-p
func (v *Visitor) dynamicCast(sourceType types.Type, targetType types.Type, targetTypeName string) string {
	// Unwrap pointer types if needed
	if sourcePtr, ok := sourceType.(*types.Pointer); ok {
		sourceType = sourcePtr.Elem()
	}

	if targetPtr, ok := targetType.(*types.Pointer); ok {
		targetType = targetPtr.Elem()
	}

	// Get the underlying struct types
	sourceStruct, ok := sourceType.Underlying().(*types.Struct)

	if !ok {
		v.showWarning("Source type '%s' used with 'dynamicCast' is not a struct", sourceType.String())
		return ""
	}

	targetStruct, ok := targetType.Underlying().(*types.Struct)

	if !ok {
		v.showWarning("Target type '%s' used with 'dynamicCast' is not a struct", targetType.String())
		return ""
	}

	// Track all fields we need to include in the constructor
	params := make([]string, 0, targetStruct.NumFields())

	// Process target struct fields -- note that we are ignoring unexported
	// fields here since the target use case is to create a new instance of
	// an internal struct that is not accessible outside the package
	for i := range targetStruct.NumFields() {
		targetField := targetStruct.Field(i)
		targetFieldName := targetField.Name()

		// Sanitize the field name to avoid C# keyword conflicts
		sanitizedFieldName := getSanitizedIdentifier(targetFieldName)
		found := false

		// First try to find field directly in source struct
		for j := range sourceStruct.NumFields() {
			sourceField := sourceStruct.Field(j)
			if sourceField.Name() == targetFieldName {
				params = append(params, fmt.Sprintf("%s.%s", DynamicCastArgMarker, sanitizedFieldName))
				found = true
				break
			}
		}

		// If not found directly, check for promoted fields in embedded structs
		if !found {
			accessPath := v.findPromotedFieldPath(sourceStruct, targetFieldName, "")

			if len(accessPath) > 0 {
				params = append(params, fmt.Sprintf("%s.%s", DynamicCastArgMarker, accessPath))
				found = true
			}
		}

		// If field not found in source at all, leave a comment
		if !found {
			// This is an unexpected error so long as this function is called in context of checking
			// for needed dynamic struct casts, as the source and target types should be structurally
			// equivalent in order to get to this point
			v.showWarning("Field '%s' not found in source struct '%s' for dynamic cast", targetFieldName, sourceType.String())
			return ""
		}
	}

	// Construct the expression using object initializer syntax
	return fmt.Sprintf("new %s(%s)", targetTypeName, strings.Join(params, ", "))
}

// findPromotedFieldPath recursively searches for a promoted field in a struct
// and returns the access path to that field or an empty string if not found
func (v *Visitor) findPromotedFieldPath(sourceStruct *types.Struct, targetFieldName string, pathPrefix string) string {
	for i := range sourceStruct.NumFields() {
		field := sourceStruct.Field(i)

		// Check if this is an embedded field (anonymous struct field)
		if field.Anonymous() {
			var currentPath string

			if field.Name() == "" {
				currentPath = pathPrefix // Unnamed embedded field
			} else {
				// Named embedded field
				if pathPrefix == "" {
					currentPath = getSanitizedIdentifier(field.Name())
				} else {
					currentPath = pathPrefix + "." + getSanitizedIdentifier(field.Name())
				}
			}

			// Check if the field itself is what we're looking for
			if field.Name() == targetFieldName {
				return currentPath
			}

			// Check the embedded field's type for further embedding
			if fieldStruct, ok := field.Type().Underlying().(*types.Struct); ok {
				// Search within the embedded struct
				if result := v.findPromotedFieldPath(fieldStruct, targetFieldName, currentPath); result != "" {
					return result
				}
			}
		}
	}

	return "" // Field not found in any embedded struct
}

func (v *Visitor) extractStructType(expr ast.Expr) (*ast.StructType, types.Type) {
	if starExpr, ok := expr.(*ast.StarExpr); ok {
		if structType, ok := starExpr.X.(*ast.StructType); ok {
			return structType, v.getType(starExpr.X, false)
		}
	} else if compositeLit, ok := expr.(*ast.CompositeLit); ok {
		if structType, ok := compositeLit.Type.(*ast.StructType); ok {
			return structType, v.getType(compositeLit.Type, false)
		}
	} else if arrayType, ok := expr.(*ast.ArrayType); ok {
		if structType, ok := arrayType.Elt.(*ast.StructType); ok {
			return structType, v.getType(arrayType.Elt, false)
		}
	} else if indexExpr, ok := expr.(*ast.IndexExpr); ok {
		if structType, ok := indexExpr.X.(*ast.StructType); ok {
			return structType, v.getType(indexExpr.X, false)
		}
	} else if sliceExpr, ok := expr.(*ast.SliceExpr); ok {
		if structType, ok := sliceExpr.X.(*ast.StructType); ok {
			return structType, v.getType(sliceExpr.X, false)
		}
	} else if callExpr, ok := expr.(*ast.CallExpr); ok {
		if structType, ok := callExpr.Fun.(*ast.StructType); ok {
			return structType, v.getType(callExpr.Fun, false)
		}
	} else if typeAssertExpr, ok := expr.(*ast.TypeAssertExpr); ok {
		if structType, ok := typeAssertExpr.Type.(*ast.StructType); ok {
			return structType, v.getType(typeAssertExpr.Type, false)
		}
	} else if selectorExpr, ok := expr.(*ast.SelectorExpr); ok {
		if structType, ok := selectorExpr.X.(*ast.StructType); ok {
			return structType, v.getType(selectorExpr.X, false)
		}
	} else if structType, ok := expr.(*ast.StructType); ok {
		return structType, v.getType(expr, false)
	}

	return nil, nil
}

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
	} else if selExpr, ok := node.(*ast.SelectorExpr); ok {
		ident = getIdentifier(selExpr.X)
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
	// First check the Types map (for expressions)
	if tv, ok := v.info.Types[ident]; ok {
		return tv.Type
	}

	// Then check the Defs map (for declarations)
	if obj := v.info.Defs[ident]; obj != nil {
		return obj.Type()
	}

	// Finally, check the Uses map (for identifier usages)
	if obj := v.info.Uses[ident]; obj != nil {
		return obj.Type()
	}

	return nil
}

func (v *Visitor) getGenericDefinition(srcType types.Type) (string, string) {
	var named *types.Named
	var signature *types.Signature
	var ok bool

	if named, ok = srcType.(*types.Named); !ok {
		if signature, ok = srcType.(*types.Signature); !ok {
			return "", ""
		}
	}

	var typeParams *types.TypeParamList

	if named != nil {
		typeParams = named.TypeParams()
	} else {
		typeParams = signature.TypeParams()

		if typeParams == nil {
			typeParams = signature.RecvTypeParams()
		}
	}

	if typeParams == nil || typeParams.Len() == 0 {
		return "", ""
	}

	typeParamNames := make([]string, typeParams.Len())
	constraintNames := []string{}

	for i := range typeParams.Len() {
		typeParam := typeParams.At(i)
		typeParamNames[i] = typeParam.Obj().Name()

		constraint := typeParam.Constraint()
		var constraintName string

		// Check if the constraint type is an anonymous interface
		if _, ok := constraint.(*types.Interface); ok {
			constraintName = constraint.String()
		} else {
			constraintName = v.getTypeName(constraint, false)
		}

		if len(constraintName) == 0 || constraintName == "any" || constraintName == "interface{}" {
			// At a minimum, generic type must implement 'ISupportMake' to be constructable, e.g., with `make`
			constraintName = "new()"
		} else {
			var iface *types.Interface

			switch typ := constraint.(type) {
			case *types.Interface:
				iface = typ
			case *types.Named:
				iface = typ.Underlying().(*types.Interface)
			case *types.Signature:
				iface = typ.Recv().Type().Underlying().(*types.Interface)
			default:
				iface = nil
			}

			if iface != nil {
				originalConstraint := fmt.Sprintf("/* %s */", constraintName)
				constraintName = strings.TrimPrefix(strings.TrimSpace(constraintName), "~")
				constraintExpr := strings.ReplaceAll(constraintName, " ", "")
				var typeConstraint string

				// Check for common Go types, e.g., slice, map, channel, etc.
				if strings.HasPrefix(constraintExpr, "[]") {
					// Handle slice via ISlice interface
					typeConstraint = fmt.Sprintf("ISlice<%s>, ISupportMake<%s>", convertToCSTypeName(constraintName[2:]), typeParamNames[i])
				} else if strings.HasPrefix(constraintExpr, "map[") {
					// Handle map via IMap interface
					keyValue := strings.Split(constraintName[4:], "]")
					typeConstraint = fmt.Sprintf("IMap<%s, %s>, ISupportMake<%s>", convertToCSTypeName(keyValue[0]), convertToCSTypeName(keyValue[1]), typeParamNames[i])
				} else if strings.HasPrefix(constraintExpr, "chan ") {
					// Handle channel via IChannel interface
					typeConstraint = fmt.Sprintf("IChannel<%s>, ISupportMake<%s>", convertToCSTypeName(constraintName[5:]), typeParamNames[i])
				} else if strings.HasPrefix(constraintExpr, "chan<- ") {
					// Handle send-only channel via IChannel interface
					typeConstraint = fmt.Sprintf("IChannel<%s>, ISupportMake<%s>", convertToCSTypeName(constraintName[7:]), typeParamNames[i])
				} else if strings.HasPrefix(constraintExpr, "<-chan ") {
					// Handle receive-only channel via IChannel interface
					typeConstraint = fmt.Sprintf("IChannel<%s>, ISupportMake<%s>", convertToCSTypeName(constraintName[7:]), typeParamNames[i])
				} else if strings.HasPrefix(constraintExpr, "func") {
					// TODO: Handle function
					v.showWarning("@getGenericDefinition - unhandled function constraint `%s` on `%s`", constraintName, srcType.String())
					typeConstraint = originalConstraint
				} else if strings.HasPrefix(constraintExpr, "struct") {
					// TODO: Handle struct - will need to lift struct type defintion
					v.showWarning("@getGenericDefinition - unhandled struct constraint `%s` on `%s`", constraintName, srcType.String())
					typeConstraint = originalConstraint
				} else {
					// Handle special case for string and []byte types
					if constraintExpr == "string" || constraintExpr == "[]byte" || constraintExpr == "string|[]byte" || constraintExpr == "[]byte|string" {
						typeConstraint = "ISlice<byte>"
					}
				}

				if iface.NumMethods() == 0 {
					// For type-constraint only interfaces, C# native types cannot directly implement
					// interface, so all base-type operator constraints must be lifted to generic type
					// constraint defintion. This can get very noisy and C# does not have a mechanism
					// to hide these constraints in partial method declarations in generated code like
					// it does for structs. For partial methods, all constraint defintions are forced
					// to match, so there is no current benefit to declaring a partial method here.
					liftedConstraints := v.getLiftedConstraints(constraint, typeParamNames[i])

					if len(liftedConstraints) > 0 {
						if len(typeConstraint) == 0 {
							constraintName = fmt.Sprintf("%s %s", originalConstraint, liftedConstraints)
						} else {
							constraintName = fmt.Sprintf("%s %s, %s", originalConstraint, typeConstraint, liftedConstraints)
						}
					} else {
						if len(typeConstraint) == 0 {
							constraintName = fmt.Sprintf("%s %s", originalConstraint, constraintName)
						} else {
							constraintName = fmt.Sprintf("%s %s", originalConstraint, typeConstraint)
						}
					}
				} else {
					// If interface has methods, can safely assume generic type must implement it directly
					constraintName = fmt.Sprintf("%s<%s>", constraintName, typeParamNames[i])
				}

				constraintName = fmt.Sprintf("%s, new()", constraintName)
			} else {
				v.showWarning("@getGenericDefinition - constraint `%s` on `%s` is not an interface", constraintName, srcType.String())
			}
		}

		constraintNames = append(constraintNames, fmt.Sprintf("%s%s    where %s : %s", v.newline, v.indent(v.indentLevel), typeParamNames[i], constraintName))
	}

	return fmt.Sprintf("<%s>", strings.Join(typeParamNames, ", ")), strings.Join(constraintNames, "")
}

func (v *Visitor) typeExists(name string) bool {
	// Look in the package scope
	obj := v.pkg.Scope().Lookup(name)

	if obj != nil && (obj.Type() != nil || obj.Type().Underlying() != nil) {
		return true
	}

	// Or search through all definitions
	for _, obj := range v.info.Defs {
		if obj != nil && obj.Name() == name && (obj.Type() != nil || obj.Type().Underlying() != nil) {
			return true
		}
	}

	return false
}

func (v *Visitor) getUniqueLiftedTypeName(typeName string) string {
	typeName = getSanitizedIdentifier(removeSanitizationMarker(typeName))
	uniqueTypeName := typeName
	count := 0

	for v.liftedTypeNames.Contains(uniqueTypeName) || v.typeExists(uniqueTypeName) {
		count++
		uniqueTypeName = fmt.Sprintf("%s%s%d", typeName, TempVarMarker, count)
	}

	v.liftedTypeNames.Add(uniqueTypeName)

	return uniqueTypeName
}

func (v *Visitor) liftedTypeExists(expr ast.Expr) bool {
	if expr == nil {
		return false
	}

	exprType := v.getType(expr, false)

	if exprType == nil {
		return false
	}

	if _, ok := v.liftedTypeMap[exprType]; ok {
		return true
	}

	if named, ok := exprType.(*types.Named); ok {
		if _, ok := v.liftedTypeMap[named.Underlying()]; ok {
			return true
		}
	}

	return false
}

func (v *Visitor) getType(expr ast.Expr, underlying bool) types.Type {
	if expr == nil {
		return nil
	}

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
	if t == nil {
		return ""
	}

	if pointer, ok := t.(*types.Pointer); ok {
		return "*" + v.getTypeName(pointer.Elem(), isUnderlying)
	}

	if name, ok := v.liftedTypeMap[t]; ok {
		return name
	}

	var pkgPrefix string

	if named, ok := t.(*types.Named); ok {
		obj := named.Obj()
		pkg := obj.Pkg()

		// Handle builtin types with no package
		if pkg != nil && pkg != v.pkg {
			pkgPrefix = pkg.Name() + "."
		}
	}

	if !isUnderlying {
		if _, ok := t.(*types.Struct); ok {
			v.showWarning("Unresolved dynamic struct type: %s", t.String())
		} else if iface, ok := t.(*types.Interface); ok && !iface.Empty() {
			v.showWarning("Unresolved dynamic interface type: %s", t.String())
		}
	}

	typeName := strings.ReplaceAll(t.String(), "..", "")
	packagePathPrefix := v.pkg.Path() + "."

	var prefix string

	// Check for slice or array prefix
	if strings.HasPrefix(typeName, "[") {
		endBracket := strings.Index(typeName, "]")

		if endBracket != -1 {
			prefix = typeName[:endBracket+1]
			typeName = typeName[endBracket+1:]
		}
	}

	// Remove package path, if any, from the type name
	typeName = strings.Replace(typeName, packagePathPrefix, "", 1)
	slashIndex := strings.LastIndex(typeName, "/")

	if slashIndex != -1 {
		typeName = typeName[slashIndex+1:]
	}

	if len(pkgPrefix) > 0 && !strings.HasPrefix(typeName, pkgPrefix) {
		return prefix + pkgPrefix + typeName
	}

	return prefix + typeName
}

func (v *Visitor) getFullTypeName(t types.Type, isUnderlying bool) string {
	if t == nil {
		return ""
	}

	if pointer, ok := t.(*types.Pointer); ok {
		return "*" + v.getFullTypeName(pointer.Elem(), isUnderlying)
	}

	if name, ok := v.liftedTypeMap[t]; ok {
		return name
	}

	if named, ok := t.(*types.Named); ok {
		obj := named.Obj()
		pkg := obj.Pkg()

		// Handle builtin types with no package
		if pkg != nil && pkg.Name() != packageName {
			return getSanitizedImport(pkg.Path()+PackageSuffix) + "." + getSanitizedImport(obj.Name())
		}
	}

	if !isUnderlying {
		if _, ok := t.(*types.Struct); ok {
			v.showWarning("Unresolved dynamic struct type: %s", t.String())
		} else if iface, ok := t.(*types.Interface); ok && !iface.Empty() {
			v.showWarning("Unresolved dynamic interface type: %s", t.String())
		}
	}

	typeName := strings.ReplaceAll(t.String(), "..", "")
	packagePathPrefix := v.pkg.Path() + "."

	// Remove package path, if any, from the type name
	typeName = strings.Replace(typeName, packagePathPrefix, "", 1)
	slashIndex := strings.LastIndex(typeName, "/")

	if slashIndex != -1 {
		typeName = typeName[slashIndex+1:]
	}

	return typeName
}

func getAliasedTypeName(typeName string) string {
	packageLock.Lock()
	alias, exists := importedTypeAliases[typeName]
	isConst := constImportedTypeAliases.Contains(typeName)
	packageLock.Unlock()

	if exists {
		if isConst {
			parts := strings.Split(typeName, ".")

			if len(parts) == 1 {
				return alias
			}

			return fmt.Sprintf("%s.%s", strings.Join(parts[:len(parts)-1], "."), alias)
		}

		return strings.ReplaceAll(typeName, ".", TypeAliasDot)
	}

	return typeName
}

func (v *Visitor) getRefParamTypeName(t types.Type) string {
	typeName := v.getTypeName(t, false)

	if strings.HasPrefix(typeName, "*") {
		return fmt.Sprintf("ref %s", convertToCSTypeName(typeName[1:]))
	}

	return convertToCSTypeName(typeName)
}

func (v *Visitor) getCSTypeName(t types.Type) string {
	return convertToCSTypeName(v.getTypeName(t, false))
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
	typeName = strings.TrimPrefix(typeName, "~")

	if strings.HasPrefix(typeName, "untyped ") {
		typeName = strings.TrimPrefix(typeName, "untyped ")

		if strings.HasPrefix(typeName, "int") || strings.HasPrefix(typeName, "uint") || typeName == "rune" || typeName == "byte" {
			return "UntypedInt"
		}

		if strings.HasPrefix(typeName, "float") {
			return "UntypedFloat"
		}

		if strings.HasPrefix(typeName, "complex") {
			return "UntypedComplex"
		}
	}

	if strings.Contains(typeName, "/") {
		typeName = convertImportPathToNamespace(typeName, "")
	}

	// Replace all `[` and `]` with `<` and `>` to handle generic types
	typeName = strings.ReplaceAll(typeName, "[", "<")
	typeName = strings.ReplaceAll(typeName, "]", ">")

	if strings.HasPrefix(typeName, "<>") {
		return fmt.Sprintf("%s.slice<%s>", RootNamespace, convertToCSTypeName(typeName[2:]))
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

	// Handle array types
	if strings.HasPrefix(typeName, "<") {
		return fmt.Sprintf("%s.array<%s>", RootNamespace, convertToCSTypeName(typeName[strings.Index(typeName, ">")+1:]))
	}

	if strings.HasPrefix(typeName, "map<") {
		innerType := typeName[4:]
		keyType, valueType := splitMapKeyValue(innerType)
		return fmt.Sprintf("%s.map<%s, %s>", RootNamespace, convertToCSTypeName(keyType), convertToCSTypeName(valueType))
	}

	// Find all types inside '<T1, T2>' type expressions and recurse into them for conversion
	if strings.Contains(typeName, "<") {
		start := strings.Index(typeName, "<")
		end := strings.Index(typeName[start:], ">") + start

		if end != -1 {
			subTypes := strings.Split(typeName[start+1:end], ",")

			for i := range subTypes {
				subTypes[i] = strings.TrimSpace(convertToCSTypeName(subTypes[i]))
			}

			typeName = fmt.Sprintf("%s<%s>%s", typeName[:start], strings.Join(subTypes, ", "), typeName[end+1:])
		}
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
		return "any"
	default:
		if strings.Contains(typeName, PackageSuffix) {
			parts := strings.Split(typeName, ".")
			count := len(parts)

			if count > 1 {
				sourcePkg := strings.TrimSuffix(parts[count-2], PackageSuffix)
				targetType := parts[count-1]
				alias := fmt.Sprintf("%s.%s", sourcePkg, targetType)

				packageLock.Lock()
				aliasType, exists := importedTypeAliases[alias]
				packageLock.Unlock()

				if exists {
					return aliasType
				}
			}
		}

		return fmt.Sprintf("%s.%s", RootNamespace, getSanitizedIdentifier(getAliasedTypeName(typeName)))
	}
}

func splitMapKeyValue(typeStr string) (string, string) {
	depth := 0
	for i, char := range typeStr {
		if char == '<' {
			depth++
		} else if char == '>' {
			depth--
			if depth < 0 {
				// Found the first top-level closing bracket
				// This is the boundary between key and value
				if i+1 < len(typeStr) {
					return typeStr[:i], typeStr[i+1:]
				}
				return typeStr[:i], ""
			}
		}
	}

	// If we didn't find a proper split, return original and empty
	return typeStr, ""
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

	// If identifier is discarded, return empty string
	if csIDName == "_" {
		return ""
	}

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

// isInherentlyHeapAllocatedType checks if the type is inherently heap allocated,
// i.e., a reference type that is not a stack allocated value type, e.g., maps,
// slices, channels, interfaces, functions, and pointers.
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
