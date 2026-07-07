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

	// currentLambdaVarObjs records, per captured NAME, the types.Object of the OUTER variable that was
	// captured. currentLambdaVars maps by name, so a same-named variable DECLARED inside the lambda (an
	// `s := f(s)` self-shadow, where the inner `s` shadows the captured outer `s`) would otherwise be
	// renamed to the capture name too — conflating the two (`var sʗ3 = …(~sʗ3)…`, CS0841). The capture
	// name is applied only when an ident resolves to this exact captured object; a distinct inner binding
	// falls through to its own name.
	currentLambdaVarObjs map[string]types.Object

	// boxRefVars holds heap-boxed local variables whose address is taken inside a lambda. Such a
	// variable must NOT be snapshot-captured (the value copy loses the box, so writes through the
	// captured `&m` are lost — and the copy declaration is invalid in expression position, e.g. a
	// func literal passed as a call argument). Instead the lambda references the box directly: `&m`
	// emits `Ꮡm` (a capturable reference) and value uses emit `Ꮡm.Value` — the ref-local alias
	// `ref var m = ref Ꮡm.Value` itself can't be captured (CS8175). Keyed by the var's types.Object.
	boxRefVars map[types.Object]bool

	// Analysis phase tracking
	analysisInLambda  bool     // Currently analyzing a lambda
	currentLambda     ast.Node // Current lambda being analyzed
	detectingCaptures bool

	// Conversion phase tracking
	conversionInLambda bool     // Currently converting a lambda
	currentConversion  ast.Node // Current node being converted

	// conversionStack saves the conversion-phase fields above (plus the per-lambda var maps) on each
	// enterLambdaConversion so a NESTED lambda restores the ENCLOSING lambda's state on exit rather
	// than clobbering it. Without it, a nested func literal's exit reset conversionInLambda to false
	// (and nil'd the var maps), so every receiver/box reference in the enclosing lambda's body AFTER
	// the nested lambda rendered as the un-boxed ref-local — uncapturable inside a closure (CS8175;
	// database/sql (*Stmt).QueryContext read `s.cg` after the inner releaseConn closure).
	conversionStack []lambdaConversionState
}

// lambdaConversionState snapshots the conversion-phase LambdaCapture fields that
// enter/exitLambdaConversion mutate, so nested lambdas nest correctly (LIFO save/restore).
type lambdaConversionState struct {
	conversionInLambda   bool
	currentConversion    ast.Node
	currentLambdaVars    map[string]string
	currentLambdaVarObjs map[string]types.Object
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

	// hoistedDecls, when non-nil, collects func-literal capture declarations that would otherwise
	// be emitted inline (a `var mʗ1 = m;` statement) at the func literal's position — invalid C#
	// when the literal sits in an expression slot (a call argument, an assignment RHS, a composite-
	// literal element). The enclosing statement emitter (visitAssignStmt, …) sets this to a buffer,
	// converts its expressions, then writes the collected decls before the statement. convFuncLit
	// consults it (after context.deferredDecls, which go/defer/return thread explicitly). Save and
	// restore around nested statements so an inner statement's decls don't leak to the outer buffer.
	hoistedDecls *strings.Builder

	// ImportSpec variables
	currentImportPath     string
	packageImports        *strings.Builder
	importQueue           HashSet[string]
	requiredUsings        HashSet[string]
	typeAliasDeclarations *strings.Builder
	// A cross-package type reference emits a short-alias form (`pkg.Type`, `@unsafe.Pointer`) that
	// resolves only through a file-local alias `using <alias> = <namespace>;`. That alias is emitted
	// when the file imports the package under its canonical (unaliased) name; a file can reference the
	// type WITHOUT such an import — via type INFERENCE (a same-package function returns a foreign type,
	// so the caller need not import it — e.g. `fd := funcdata(...)`, funcdata returns unsafe.Pointer),
	// a BLANK import (`_ "pkg"`, whose C# alias is `_`), or an alias that differs from the canonical
	// name — and then the reference fails to resolve (CS0246). referencedForeignPackages collects the
	// import paths whose types getTypeName emits; canonicalAliasImported records the paths whose
	// canonical alias a file import already emitted. visitFile supplies the alias for the difference.
	referencedForeignPackages HashSet[string]
	canonicalAliasImported    HashSet[string]
	// importAliasesEmitted holds the C# alias NAMES a file's real imports already bound (`asn1`,
	// `encoding_asn1`, `time`). visitFile's synthesized canonical-alias `using` is skipped when its
	// alias collides with one of these — a same-named subpackage plus an aliased parent import both
	// resolving to alias `asn1` (cryptobyte's `encoding/asn1` + `.../cryptobyte/asn1`, CS1537).
	importAliasesEmitted HashSet[string]

	// importPathAliases maps a Go import PATH to the C# alias THIS FILE bound for it, for the
	// EXPLICITLY-ALIASED imports only. getTypeName consults it so a foreign type renders via the
	// file's ACTUAL alias, not the canonical package name: cryptobyte's asn1.go imports
	// `encoding/asn1` under the NON-canonical alias `encoding_asn1` (the vendored
	// `.../cryptobyte/asn1` subpackage claims the canonical `asn1`), so a `*asn1.BitString` type
	// reference must render `encoding_asn1.BitString`, not `asn1.BitString` (which resolves to the
	// subpackage — CS0426). Unaliased / blank / dot / Δ-collision-renamed imports are absent and fall
	// back to importQualifier(pkg.Name()) (the prior behavior), so this only changes explicit-alias
	// renders — no churn elsewhere. types.Type carries no source alias, so this map supplies it.
	importPathAliases map[string]string

	// FuncDecl variables
	inFunction           bool
	currentFuncDecl      *ast.FuncDecl
	currentFuncSignature *types.Signature
	// currentReturnSignature is the signature whose RESULTS a `return` currently emits against — the
	// enclosing function's, or a nested function literal's own (set with save/restore in convFuncLit).
	// Distinct from currentFuncSignature (which stays the enclosing func for receiver/param detection).
	currentReturnSignature *types.Signature
	currentFuncName        string
	currentFuncPrefix    *strings.Builder
	paramNames           HashSet[string]
	paramObjects         map[types.Object]bool
	// identAddressTakenCache memoizes per-object `&ident` scans of the current function
	// (see identAddressTaken); lazily initialized, keyed by the *types.Object so entries
	// from prior functions are simply never consulted again.
	identAddressTakenCache map[types.Object]bool
	// nilSafePtrParamNames holds the raw names of pointer PARAMETERS that are compared with `==`/
	// `!=` (against nil or another pointer) anywhere in the current function body — i.e. params
	// walked to a nil terminator (`for p != nil { …; p = p.next }`). For these, the deref-alias and
	// any pointer-reassignment re-alias use the nil-safe `Ꮡp.DerefOrNil()` accessor instead of
	// `Ꮡp.Value`, so re-aliasing to a nil box yields a ref to default(T) (never read while p is nil)
	// rather than throwing a nil-pointer dereference. Populated per function in visitFuncDecl;
	// other (non-nil-compared) pointer params keep the plain `.Value` form (zero golden churn).
	nilSafePtrParamNames HashSet[string]
	varNames             map[*types.Var]string
	hasDefer             bool
	hasRecover           bool
	// pendingTypeAccess carries an explicit C# access modifier ("public ") for the type
	// declaration currently being emitted — set by visitTypeSpec for an unexported type that
	// must be publicized (used as an exported struct field; see packagePublicizedTypes), and
	// consumed (read and cleared) by the type-kind emitter (visitArrayType/visitStructType/…).
	pendingTypeAccess string
	// namedReturnDeferMode is set when the current function has named return values AND uses
	// defer/recover. Such a function is emitted as a block body that declares the named returns
	// *outside* the `func((defer, recover) => …)` wrapper (so deferred code, including recover,
	// mutates them by closure) and returns them *after* the wrapper runs — matching Go, where a
	// `return` assigns the result params, runs the defers, then returns the (possibly-mutated)
	// result params. namedReturnNames holds those result identifiers in order.
	namedReturnDeferMode bool
	namedReturnNames     []string
	useUnsafeFunc        bool
	capturedVarCount     map[string]int
	tempVarCount         map[string]int

	// BlockStmt variables
	blocks                 Stack[*strings.Builder]
	firstStatementIsReturn bool
	// tupleTempIndex numbers the multi-value-call expansion temp markers monotonically per
	// file (see convExprList's tuple-arg expansion).
	tupleTempIndex int
	// inForPost is set while emitting a for-loop's POST statement. A deref-aliased pointer
	// param/box repointed in the post (`for ; scope != nil; scope = scope.Outer`) expands to a
	// box-repoint PLUS a value re-alias (`Ꮡscope = scope.Outer; scope = ref Ꮡscope…`); the
	// second statement cannot share the single for-post slot, so the re-alias is stashed in
	// forPostReAlias and visitForStmt injects it at the TOP of the loop body instead.
	inForPost      bool
	forPostReAlias string
	lastStatementWasReturn bool
	lastReturnIndentLevel  int
	identEscapesHeap       map[types.Object]bool
	identNames             map[*ast.Ident]string   // Local identifiers to adjusted names map
	isReassigned           map[*ast.Ident]bool     // Local identifiers to reassignment status map
	funcLevelDecls         map[string]*types.Var   // Function-level local declarations of the current function (for global-shadow qualification)
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
const OpaqueTrueMarker = TrueMarker + TrueMarker // golib static readonly true - NOT compiler-foldable (leading constant-true case, CS8120)
const ValueAdapterInfix = "ᴠ"          // ᴠ - value-form foreign adapter infix (keep in sync with Symbols.ValueAdapterInfix)
const OverloadDiscriminator = "\uA7F7"        // Variants: ꟷ false
const EllipsisOperator = "\uA4F8\uA4F8\uA4F8" // Variants: ꓸꓸꓸ ᐧᐧᐧ
const TypeAliasDot = "\uA4F8"                 // Variants: ꓸ
const ChannelLeftOp = "\u1438\uA7F7"          // Example: `ch.ᐸꟷ(val)` for `ch <- val`
const ChannelRightOp = "\uA7F7\u1433"         // Example: `ch.ꟷᐳ(out var val)` for `val := <-ch`
const PointerDerefOp = "~"                    // Example: `~ptr` for dereferencing a pointer

// NilSafeDerefAccessor is the golib ж<T> extension method used in place of `.Value` to re-alias a
// deref'd pointer parameter that is walked to a nil terminator (see nilSafePtrParamNames). Unlike
// `.Value` (which throws on a nil box), it returns a ref to a shared default(T) slot when the box is
// nil — the ref is never read while the box is nil, so the standard `*p` panic semantics are
// preserved (a genuine nil deref still uses `~`/`.Value`). Includes `()` as it is a method call.
const NilSafeDerefAccessor = "DerefOrNil()"

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

	// `file` is a C# 11 contextual keyword reserved as a TYPE-name modifier: a type declared
	// `partial struct file` is CS9056 ("Types and aliases cannot be named 'file'") — os's
	// `type file struct{…}` cascaded ~30 errors. The '@' escape is valid in every position,
	// so it is escaped like a full keyword.
	"file",

	// `true`/`false` are C# KEYWORDS but Go PREDECLARED IDENTIFIERS (not keywords) — a Go
	// parameter/variable may shadow them (`func (t *Tree) newBool(pos Pos, true bool)`,
	// text/template/parse), so the raw `bool true` is a C# syntax error (CS1001/CS1003). Escape.
	"true", "false",

	// The remaining C# keywords overlap with Go keywords (true keywords in Go too), so they
	// do not need detection: "break", "case", "const", "continue", "default", "else", "for",
	// "goto", "if", "interface", "return", "select", "struct", "switch", "var"
})

// The following names are reserved by go2cs or C#, if encountered in Go code, prefix with `Δ`:
// Note that "_" is used for type assertion functions in go2cs converted C# code, but it is not
// a valid method name in Go, so it is not included in the reserved list.
var reserved = NewHashSet([]string{
	"AreEqual", "array", "channel", "defer\u01C3", "EmptyStruct", "Equals", "Finalize", "GetGoTypeName",
	"GetHashCode", "GetType", "GoFunc", "GoFuncRoot", "GoImplement", "GoImplementAttribute", "GoImplicitConv",
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

// packageInlineFuncTypeNames records the names of this package's NON-GENERIC METHODLESS named func
// types — the ones visitFuncType renders inline as their base delegate and whose named declaration
// is skipped (there is no `<name>_package.<Δname>` type). Their exported-type-alias must NOT be
// emitted: a `[GoTypeAlias("Filter", "ΔFilter")]` makes consumers reference a nonexistent
// `go.go.ast_package.ΔFilter` (go/doc's `ast.Filter`, CS0426). Keyed by both the raw Go name and its
// core-sanitized form to match either exportedTypeAliases population site.
var packageInlineFuncTypeNames map[string]bool

// importedPointerImplements records `[assembly: GoImplement<T, Iface>(Pointer = true)]` lines
// parsed from IMPORTED packages' package_info files, keyed "pkgName|T|ifaceSimple" - the
// existence proof that the foreign assembly generated the public TжIface adapter class
// (io/fs's PathErrorжerror), so a cross-package pointer-to-interface conversion can
// reference it (os's `err = &PathError{...}`, CS0029 x38).
var importedPointerImplements HashSet[string]

// importedValueImplements records VALUE-form `[assembly: GoImplement<T, Iface>]` lines (plain or
// Promoted) parsed from IMPORTED packages' package_info files, keyed "pkgName|T|ifaceSimple" -
// the existence proof that the foreign assembly itself implements the interface on the value
// type, so a both-foreign value cast here converts implicitly and skips the local adapter.
var importedValueImplements HashSet[string]

// importPackageDirs maps a directly-imported package's import path to its on-disk source directory
// and Go package name, captured from the MODULE-AWARE go/packages graph at load time. It is the
// fallback resolver for cross-package references to a LOCAL/USER module (a `replace`d or co-located
// module), which the legacy go/build (GOPATH-only) resolver in getImportPackageInfo cannot find.
// Reset and repopulated per package.
var importPackageDirs map[string]importedPackageMeta

type importedPackageMeta struct {
	Dir  string // package source directory (also the in-place converted-output directory)
	Name string // Go package name (the identifier used to qualify references in code)
}
var constImportedTypeAliases HashSet[string]
var parsedPackageInfoFiles HashSet[string]
var interfaceImplementations map[string]HashSet[string]
var promotedInterfaceImplementations map[string]HashSet[string]

// constraintProxies collects the SELF-REFERENTIAL constraint proxies this package needs — a
// generic type instantiated with a pointer type whose type parameter carries a self-referential
// generic method-set interface constraint (nistCurve's `Point nistPoint[Point]` at `*P224Point`).
// The box ж<P224Point> can't nominally implement nistPoint<…>, so a proxy stands in as the type
// argument. Keyed "elementFullName|interfaceFullName" → {elementFullName, interfaceFullName}; each
// becomes a `[assembly: GoImplement<element, iface<element>>(ConstraintProxy = true)]` record that
// drives ImplementGenerator's EmitConstraintProxy. See constraintProxyArg.
var constraintProxies map[string][2]string
var interfaceInheritances map[string]HashSet[string]
var implicitConversions map[string]HashSet[string]
var invertedImplicitConversions map[string]HashSet[string]
var indirectImplicitConversions map[string]HashSet[string]

// conversionPackageUsings maps a cross-package import alias (e.g. "abi") to its C# namespace (e.g.
// "@internal.abi_package") for every package referenced by a recorded implicit conversion. The
// `[assembly: GoImplicitConv<abi.Type, ж<abi.Type>>]` lines in package_info.cs use these aliases, but
// that file has no file-local `using abi = …`, so the aliases are emitted there as `global using`
// directives. Keyed by alias; guarded by packageLock. See recordConversionPackageUsing.
var conversionPackageUsings map[string]string
var numericConversions map[string]map[string]string
var indirectNumericConversions map[string]map[string]string
var nameCollisions map[string]bool

// packageBuiltinShadows holds Go built-in names (`clear`, `len`, …) that the current package ALSO
// declares as a method or function. In Go a method `func (x T) clear()` and the universe `clear`
// built-in coexist (the method is only reached as `x.clear()`), but in C# the method is emitted as a
// `clear(this ref T)` extension on the package's static class, which SHADOWS the using-static
// `go.builtin.clear` for an unqualified free `clear(s)` call (C# member lookup stops at the class).
// A built-in call whose name is in this set is therefore emitted qualified as `builtin.<name>(…)`.
var packageBuiltinShadows map[string]bool

// packageFuncMethodNames holds every method/function name declared in the current package —
// a name matching an imported package's using-alias shadows it inside the package class
// (compress/flate's byLiteral.sort vs `import "sort"`, CS0119); the package-ident emission
// qualifies through the _package class instead.
var packageFuncMethodNames map[string]bool

// goBuiltinNames is the set of Go universe built-in function names that golib implements as static
// methods on `go.builtin`. Used to detect a package method/function that shadows one of them.
var goBuiltinNames = map[string]bool{
	"append": true, "cap": true, "clear": true, "close": true, "complex": true,
	"copy": true, "delete": true, "imag": true, "len": true, "make": true,
	"max": true, "min": true, "new": true, "panic": true, "print": true,
	"println": true, "real": true, "recover": true,
}
var globalTempVarCount map[string]int
var initFuncCounter int
var usesUnsafeCode bool
var packageLock = sync.Mutex{}

// packageDynamicTypeNames maps a lifted (anonymous struct/interface) type's
// structural signature (`types.Type.String()`) to its generated C# type name,
// shared across all files in a package. Per-file visitors lift these types into
// their own `liftedTypeMap`, but cross-file references (e.g. taking the address
// of a field of a package-global anonymous-struct var declared in another file)
// can't see that per-file map. This package-level registry, resolved after the
// concurrent file-visit barrier, bridges that gap. Guarded by packageLock.
var packageDynamicTypeNames map[string]string

// packageManualTypeNames records the CONVERTED names of this package's manually-converted
// types (see manualTypeOperations.go), collected as visitTypeSpec skips their declarations.
// Consumed by the GoImplicitConv attribute emission, which must not reference the skipped
// auto forms (the *_impl.cs declares any conversion operators the call sites need). Guarded
// by packageLock.
var packageManualTypeNames map[string]bool

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

	// Pin go/build's resolver to the converter's robustly-resolved GOROOT/GOPATH. build.Default is
	// initialized at package-init from the start-up environment, which can be empty or stale in a
	// child process (e.g. the behavioral runner Execs go2cs.exe with a sparse env) — leaving
	// build.Import unable to find even stdlib packages like "fmt". Without this, getImportPackageInfo
	// falls through to the local-module path and emits a machine-specific absolute GOROOT reference.
	build.Default.GOROOT = *goRootCmd
	build.Default.GOPATH = *goPathCmd

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
		packageInlineFuncTypeNames = make(map[string]bool)
		importedPointerImplements = HashSet[string]{}
		importedValueImplements = HashSet[string]{}
		constImportedTypeAliases = NewHashSet([]string{})
		parsedPackageInfoFiles = NewHashSet([]string{})
		interfaceImplementations = make(map[string]HashSet[string])
		promotedInterfaceImplementations = make(map[string]HashSet[string])
		constraintProxies = make(map[string][2]string)
		interfaceInheritances = make(map[string]HashSet[string])
		implicitConversions = make(map[string]HashSet[string])
		invertedImplicitConversions = make(map[string]HashSet[string])
		indirectImplicitConversions = make(map[string]HashSet[string])
		conversionPackageUsings = make(map[string]string)
		numericConversions = make(map[string]map[string]string)
		indirectNumericConversions = make(map[string]map[string]string)
		nameCollisions = make(map[string]bool)
		globalTempVarCount = make(map[string]int)
		packageDynamicTypeNames = make(map[string]string)
		packageManualTypeNames = make(map[string]bool)
		packageAddressedGlobals = make(map[types.Object]bool)
		packageImportAliasRenames = make(map[string]string)
		packageChildNamespaces = make(map[string]bool)
		packageImportLeadingSegments = make(map[string]bool)
		packagePublicizedTypes = make(map[types.Object]bool)
		packagePublicizedLiftedTypes = make(map[types.Type]bool)
		packageCaptureModeMethods = make(map[*types.Func]bool)
		packageCaptureModeBoxIdents = make(map[types.Object]bool)
		packageDirectBoxReceiverMethods = make(map[*types.Func]bool)
		initFuncCounter = 0
		usesUnsafeCode = false

		files := []FileEntry{}
		fset := pkg.Fset
		packageTypes := pkg.Types
		info := pkg.TypesInfo

		// Capture the module-aware source dir + package name of every directly-imported package, so
		// cross-package references to LOCAL/USER modules (which go/build cannot resolve) can be wired
		// up — both their ProjectReference and their exported-type-alias package_info.cs.
		importPackageDirs = make(map[string]importedPackageMeta)
		for importPath, importedPkg := range pkg.Imports {
			importPackageDirs[importPath] = importedPackageMeta{Dir: importedPkg.Dir, Name: importedPkg.Name}
		}

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
		// Identify capture-mode methods (those taking &recv.field) — across the package
		// and its imports — before escape analysis, so a value var on which one is
		// called can be marked as escaping (and the call routed through the ж overload).
		collectCaptureModeMethods(pkg)

		// Record each defined type's WRITTEN right-hand side (lost by Named.Underlying()'s
		// full resolution) — the array-reinterpret emission in convCallExpr consults it.
		collectTypeSpecRHS(pkg)

		performEscapeAnalysis(files, fset, packageTypes, info)

		// Find package-level vars whose address is taken (cross-file) so their
		// declarations can be emitted as heap boxes that &global references directly.
		collectAddressedGlobals(files, packageTypes, info)

		// Find import aliases whose name collides with a child namespace visible from the
		// transitive import closure (CS0576) so alias emission and every package-qualifier
		// render Δ-renames them consistently.
		computeImportAliasRenames(files, packageTypes, packageNamespace)

		// Find unexported types used as exported struct fields so they can be emitted as public
		// (an exported field's type must be at least as accessible — CS0051/CS0052).
		collectPublicizedTypes(packageTypes)

		var outputFileNames []string

		// Convert files SEQUENTIALLY, in the deterministic pkg.Syntax (sorted filename) order. Files
		// were previously converted in concurrent goroutines, but the per-file visitors share package-
		// level state claimed at visit time — initFuncCounter (initΔN indices), getGlobalTempVarName
		// (blank `_` func/var numbering, an unsynchronized map), and the loadImportedTypeAliases
		// check-then-act (a file marked an imported package_info "parsed" BEFORE the parse finished, so
		// a concurrently-converting file saw the marker, skipped the wait, and emitted an imported
		// const collision-rename bare — e.g. `abi.String` instead of `abi.ΔString`, a compile error
		// that came and went with goroutine scheduling). Claim order = schedule order made the emitted
		// bytes nondeterministic across otherwise-identical runs. Per-file emission is a small fraction
		// of conversion cost (dominated by go/packages type-graph loading), so sequential conversion
		// buys byte-reproducible output for free: a full-stdlib conversion (305 packages, -parallel 4)
		// measured 3m42s with the concurrent per-file goroutines and 3m39s sequential — within noise.
		for _, fileEntry := range files {
			func(fileEntry FileEntry) {
				defer func() {
					if !options.debugMode {
						if r := recover(); r != nil {
							showWarning("visit file error: %v in \"%s\"", r, filepath.Base(fileEntry.filePath))
						}
					}
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
					referencedForeignPackages: HashSet[string]{},
					canonicalAliasImported:    HashSet[string]{},
					importAliasesEmitted:      HashSet[string]{},
					importPathAliases:         map[string]string{},
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
				outputFileNames = append(outputFileNames, outputFileName)
				packageLock.Unlock()
			}(fileEntry)
		}

		// Resolve any deferred cross-file dynamic (anonymous struct) type references
		// now that every file's lifted names are registered in the shared registry.
		resolveDynamicTypeMarkers(outputFileNames)

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

			// Add package-qualifier aliases used by recorded GoImplicitConv attributes (e.g.
			// `abi.Type`). package_info.cs has no file-local `using abi = …`; emit a FILE-LOCAL `using`
			// (not `global` — that would clash with the per-file `using abi = …` other source files
			// already declare, CS1537) resolving each alias to its `go`-rooted namespace.
			for alias, namespace := range conversionPackageUsings {
				lines.Add(fmt.Sprintf("using %s = %s.%s;", alias, RootNamespace, namespace))
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

			// Add new type aliases to package info file (hashset ensures uniqueness). A NON-GENERIC
			// METHODLESS named func type is rendered inline as its base delegate with no named
			// declaration (visitFuncType), so it has no `<pkg>_package.<Δname>` type — skip its alias,
			// or a consumer's generated `global using` names a nonexistent type (go/doc's `ast.Filter`
			// → `go.go.ast_package.ΔFilter`, CS0426).
			for alias, typeName := range exportedTypeAliases {
				// visitFuncType records the RENAMED name (a collision-renamed `Filter` is stored as
				// `ΔFilter`, which is the alias VALUE), while a non-collision methodless func type is
				// stored under its plain name (the alias KEY) — check both.
				if packageInlineFuncTypeNames[alias] || packageInlineFuncTypeNames[typeName] {
					continue
				}

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

		// Fully-qualified prefix (e.g. `go.@internal.profile_package`) for this package's own types.
		// Used to root a BARE local type reference in the GoImplement/GoImplicitConv assembly
		// attributes when that name collides with a `using System;`-imported type (CS0104) — see
		// qualifySystemCollidingLocalTypeRefs.
		localTypePrefix := packageNamespace + "." + getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))

		qualifyLocalTypeRef := func(name string) string {
			return qualifySystemCollidingLocalTypeRefs(rootQualifySubNamespaceTypeRefs(name), localTypePrefix)
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

			// Drop lower level interface implementations where interface inheritances are already covered.
			// POINTER-form (ж<T>-wrapped) pairs are exempt: each generates a DISTINCT IжAdapter class,
			// and cast sites reference the adapter for the EXACT interface they target — a Source-
			// targeted cast needs runtimeSourceᴵSource even though runtimeSourceᴵSource64 also
			// implements Source through interface inheritance (math/rand CS0246). Only the value-boxing
			// partial-struct form (one type, one interface list) is redundant under inheritance.
			for interfaceName, inheritedInterfaces := range interfaceInheritances {
				for _, inheritedInterfaceName := range inheritedInterfaces.Keys() {
					// Check if the same type implements both interfaces
					if inheritedImplementations, ok := interfaceImplementations[inheritedInterfaceName]; ok {
						if baseImplementations, ok := interfaceImplementations[interfaceName]; ok {
							// Intersect on a COPY — IntersectWithSet mutates its receiver, and the receiver
							// here is the DERIVED interface's LIVE implementation set. The old in-place
							// intersect deleted every derived-only implementation (io: nopCloser →
							// ReadCloser shares nothing with Reader's set, so both ReadCloser pairs
							// vanished and the returns failed CS0029) and made the surviving set depend
							// on map iteration order. Only the COMMON implementations are dropped, and
							// only from the LOWER (inherited) interface — C# interface inheritance
							// already covers them via the derived implementation.
							commonImplementations := NewHashSet(baseImplementations.Keys())
							commonImplementations.IntersectWithSet(inheritedImplementations)

							for _, implementation := range commonImplementations.Keys() {
								if strings.HasPrefix(implementation, PointerPrefix+"<") {
									continue
								}

								inheritedImplementations.Remove(implementation)
							}
						}
					}
				}
			}

			// Add new interface implementations to package info file (hashset ensures uniqueness).
			// A ж<T>-wrapped implementation records a POINTER-sourced cast (`var s Iface = &t`) —
			// unwrap it to `GoImplement<T, Iface>(Pointer = true)`, which generates the IжAdapter
			// wrapper (interface aliases the receiver box) instead of the value-boxing partial.
			//
			// De-duplicate implementations recorded under BOTH a package type ALIAS and the
			// aliased type's qualified name (os converts dirEntry to fs.DirEntry through its own
			// `type DirEntry = fs.DirEntry` AND through the io/fs name): two GoImplement
			// attributes for ONE interface make the generator emit the explicit interface
			// implementation twice (CS8646 ×4 + CS0111 ×4, os dirEntry). The ALIASED record wins
			// — its simple name resolves via the package usings and keeps the generator's
			// last-dot-segment naming; the QUALIFIED duplicate is skipped. (Normalizing the
			// RECORDS to the qualified form instead regressed os 8→77: the qualified interface
			// name broke generator resolution and flipped the alias-locality gate.)
			aliasCoveredImplementations := HashSet[string]{}

			for alias, typeName := range exportedTypeAliases {
				if implementations, ok := interfaceImplementations[alias]; ok {
					canonIface := strings.TrimPrefix(typeName, RootNamespace+".")

					for implementation := range implementations {
						aliasCoveredImplementations.Add(canonIface + "|" + implementation)
					}
				}
			}

			for interfaceName, implementations := range interfaceImplementations {
				// A marker-form key (an anonymous-interface record made from a file visited
				// before the declaring file registered its lift) resolves against the
				// now-complete registry; an unresolvable one is dropped — mirroring the
				// implicit-conversion writer's resolveImplicitConvTypeName skip below.
				interfaceName, okIface := resolveImplicitConvTypeName(interfaceName)

				if !okIface {
					continue
				}

				for implementation := range implementations {
					canonKey := strings.TrimPrefix(interfaceName, RootNamespace+".") + "|" + implementation

					if aliasCoveredImplementations.Contains(canonKey) {
						continue
					}
					if inner, ok := strings.CutPrefix(implementation, PointerPrefix+"<"); ok {
						lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s>(Pointer = true)]", qualifyLocalTypeRef(strings.TrimSuffix(inner, ">")), qualifyLocalTypeRef(interfaceName)))
						continue
					}

					lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s>]", qualifyLocalTypeRef(implementation), qualifyLocalTypeRef(interfaceName)))
				}
			}

			// Add new promoted interface implementations to package info file (hashset ensures uniqueness)
			for interfaceName, implementations := range promotedInterfaceImplementations {
				for implementation := range implementations {
					lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s>(Promoted = true)]", qualifyLocalTypeRef(implementation), qualifyLocalTypeRef(interfaceName)))
				}
			}

			// Add self-referential constraint proxies (nistCurve[*P224Point]'s Point). Each is a
			// `GoImplement<element, iface<element>>(ConstraintProxy = true)` — the interface's own
			// type argument is a PLACEHOLDER (the generator closes it over the emitted proxy itself),
			// so the element doubles as the dummy. See constraintProxyArg / EmitConstraintProxy.
			for _, proxy := range constraintProxies {
				elementRef := qualifyLocalTypeRef(proxy[0])
				interfaceRef := qualifyLocalTypeRef(proxy[1])
				lines.Add(fmt.Sprintf("[assembly: GoImplement<%s, %s<%s>>(ConstraintProxy = true)]", elementRef, interfaceRef, elementRef))
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

			// A conversion referencing a manually-converted type must not emit — the generated
			// operator would read the skipped auto form's numeric backing; the *_impl.cs declares
			// any conversion operators its call sites need (see manualTypeOperations.go).
			referencesManualType := func(typeNames ...string) bool {
				for _, typeName := range typeNames {
					if packageManualTypeNames[typeName] {
						return true
					}
				}

				return false
			}

			// Add new implicit conversions to package info file (hashset ensures uniqueness)
			for sourceType, targetTypes := range implicitConversions {
				for targetType := range targetTypes {
					if referencesManualType(sourceType, targetType) {
						continue
					}

					source, okSource := resolveImplicitConvTypeName(sourceType)
					target, okTarget := resolveImplicitConvTypeName(targetType)

					if !okSource || !okTarget || source == target {
						continue
					}

					lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>]", qualifyLocalTypeRef(source), qualifyLocalTypeRef(target)))
				}
			}

			// Add new inverted implicit conversions to package info file (hashset ensures uniqueness)
			for sourceType, targetTypes := range invertedImplicitConversions {
				for targetType := range targetTypes {
					if referencesManualType(sourceType, targetType) {
						continue
					}

					source, okSource := resolveImplicitConvTypeName(sourceType)
					target, okTarget := resolveImplicitConvTypeName(targetType)

					if !okSource || !okTarget || source == target {
						continue
					}

					lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Inverted = true)]", qualifyLocalTypeRef(target), qualifyLocalTypeRef(source)))
				}
			}

			// Add new indirect implicit conversions to package info file (hashset ensures uniqueness)
			for sourceType, targetTypes := range indirectImplicitConversions {
				for targetType := range targetTypes {
					if referencesManualType(sourceType, targetType) {
						continue
					}

					source, okSource := resolveImplicitConvTypeName(sourceType)
					target, okTarget := resolveImplicitConvTypeName(targetType)

					if !okSource || !okTarget {
						continue
					}

					lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Indirect = true)]", qualifyLocalTypeRef(source), qualifyLocalTypeRef(target)))
				}
			}

			// Add new numeric conversions to package info file (maps ensure uniqueness)
			for sourceType, targetTypes := range numericConversions {
				for targetType, valueType := range targetTypes {
					if referencesManualType(sourceType, targetType) {
						continue
					}

					var inverted bool

					if strings.HasPrefix(valueType, "imported:") {
						valueType = strings.TrimPrefix(valueType, "imported:")
						inverted = false
					} else {
						inverted = true
					}

					lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Inverted = %t, ValueType = \"%s\")]", qualifyLocalTypeRef(sourceType), qualifyLocalTypeRef(targetType), inverted, valueType))
				}
			}

			// Add new indirect numeric conversions to package info file (maps ensure uniqueness)
			for sourceType, targetTypes := range indirectNumericConversions {
				for targetType, valueType := range targetTypes {
					if referencesManualType(sourceType, targetType) {
						continue
					}

					var inverted bool

					if strings.HasPrefix(valueType, "imported:") {
						valueType = strings.TrimPrefix(valueType, "imported:")
						inverted = false
					} else {
						inverted = true
					}

					lines.Add(fmt.Sprintf("[assembly: GoImplicitConv<%s, %s>(Inverted = %t, Indirect = true, ValueType = \"%s\")]", qualifyLocalTypeRef(sourceType), qualifyLocalTypeRef(targetType), inverted, valueType))
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

	// References to converted stdlib packages are emitted as `$(go2csPath)core\...`; a LOCAL/USER
	// module reference (getLocalModulePackageInfo) is an ABSOLUTE path, which is rewritten here
	// relative to THIS project's directory so the generated .csproj is portable. projectDir is made
	// absolute first — projectFileName can be relative (a relative output path), which would make
	// filepath.Rel fail against the absolute reference and leave a machine-specific path behind.
	projectDir := filepath.Dir(projectFileName)

	if absDir, absErr := filepath.Abs(projectDir); absErr == nil {
		projectDir = absDir
	}

	for _, info := range packageInfoMap {
		reference := info.ProjectReference

		if len(reference) == 0 {
			continue
		}

		if filepath.IsAbs(reference) {
			if rel, relErr := filepath.Rel(projectDir, reference); relErr == nil {
				reference = rel
			}
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

// addMethodPackageNamespaceUsing ensures the file-local `using <namespace>;` for a cross-package
// method's defining package is emitted, so its C# extension method (`Method(this ж<T>, …)`) is in
// scope at the call site even when the file does not explicitly import that package. Mirrors the
// namespace-using derivation in visitImportSpec's unaliased-import branch; a no-op for the current
// package and for root-namespace (`go`) packages, whose extensions are already visible.
func (v *Visitor) addMethodPackageNamespaceUsing(pkg *types.Package) {
	if pkg == nil || pkg == v.pkg {
		return
	}

	importPath := rootQualifyIfAmbiguous(convertImportPathToNamespace(pkg.Path(), PackageSuffix))

	lastDot := strings.LastIndex(importPath, ".")

	if lastDot == -1 {
		return
	}

	namespace := importPath[:lastDot]

	if len(namespace) > 0 && packageNamespace != fmt.Sprintf("%s.%s", RootNamespace, namespace) {
		v.addRequiredUsing(namespace)
	}
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

// isCSharpConstantExpr reports whether the expression renders as a C# compile-time constant, and
// so may be used as the operand of a relational/constant pattern (`x is <op> Y`). Literals always
// qualify; a const reference qualifies only when it is emitted as a C# `const` — i.e. a concrete
// (non-untyped) basic type. A variable, or a const emitted as `static readonly` (untyped/named,
// see visitValueSpec), does not, and the caller must use a `when` guard instead (avoids CS9135).
func (v *Visitor) isCSharpConstantExpr(expr ast.Expr) bool {
	// A constant expression whose CONTEXTUAL type is a wrapper STRUCT — the golib uintptr
	// (golib/uintptr.cs) or ANY named numeric (`[GoType("num:…")]`, time's Duration) — can
	// NEVER be a C# constant: wrapper structs have no constant form, so no constant/relational
	// pattern can compare against them (CS9135). Even a plain literal adopts the tag's or the
	// comparand's type in context (`case 4:` under a uintptr tag; `d is >= 0` typing 0 as
	// Duration — time Abs/round ×2). Force the when-guard/`==` fallback for the whole class.
	if tv, ok := v.info.Types[expr]; ok && tv.Type != nil {
		if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Kind() == types.Uintptr {
			return false
		}

		if named, ok := types.Unalias(tv.Type).(*types.Named); ok {
			if _, isBasic := named.Underlying().(*types.Basic); isBasic {
				return false
			}
		}
	}

	switch e := expr.(type) {
	case *ast.BasicLit:
		return true
	case *ast.ParenExpr:
		return v.isCSharpConstantExpr(e.X)
	case *ast.Ident:
		return v.isCSharpConstObject(v.info.ObjectOf(e))
	case *ast.SelectorExpr:
		return v.isCSharpConstObject(v.info.ObjectOf(e.Sel))
	}

	return false
}

func (v *Visitor) isCSharpConstObject(obj types.Object) bool {
	constObj, ok := obj.(*types.Const)

	if !ok {
		return false
	}

	basic, ok := constObj.Type().(*types.Basic)

	// A named-type const, or an untyped const (emitted as an Untyped* wrapper / GoUntyped), is
	// `static readonly`, not a C# `const`.
	return ok && basic.Info()&types.IsUntyped == 0
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
	// A type-vs-method name collision (a package-level type and a method sharing a name) is normally
	// avoided by Δ-prefixing the TYPE here, while the METHOD keeps its core-sanitized name — so the
	// nested type `Δfoo` and the extension method `foo` no longer collide. But when the colliding name
	// is also a golib reserved word, the METHOD's core-sanitized name is ALSO Δ-prefixed
	// (getCoreSanitizedIdentifier's reserved branch), so the plain Δ-prefix stops separating them: the
	// nested type and the extension method would BOTH be `Δ<name>` and collide in C# (CS0102). Append a
	// type marker so the TYPE gets a name distinct from the method. Example: runtime's `type slice` vs
	// `func (*userArena) slice` — both reserved-renamed: TYPE → `Δsliceᴛ`, METHOD → `Δslice`. Only the
	// type side is renamed; the method (and every method call site / go2cs-gen generated overload) is
	// left as `Δslice`, which keeps the converter and the go2cs-gen generators — they compute method
	// names independently — in sync. (Renaming the method instead desyncs them and cascades.)
	if reserved.Contains(identifier) {
		return ShadowVarMarker + identifier + TempVarMarker
	}

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

	// Strip a pointer marker so accessibility is judged by the pointed-to type's name, not the '*'
	// (which is neither lowercase nor '_', so an embedded `*unexported` field would wrongly read as
	// exported → a `public` accessor for an internal type, causing CS0053/CS8799).
	name = strings.TrimPrefix(name, "*")

	// Strip the Δ collision/reserved-word rename prefix so accessibility tracks the ORIGINAL Go
	// name's exported-ness. Δ (a Greek capital) is not lowercase, so a Δ-renamed unexported type
	// (e.g. `p_gFree` → `Δp_gFree`) would otherwise read as exported → a `public` promoted method
	// whose unexported parameter types are less accessible (CS0051).
	name = strings.TrimPrefix(name, ShadowVarMarker)

	// Strip the C# keyword escape so accessibility tracks the Go name's case: an @-escaped
	// unexported type (`decimal` -> `@decimal`, strconv) otherwise reads as exported ('@' is
	// neither lowercase nor '_'), defeating the receiver clamp - a public method with an
	// internal receiver parameter type (CS0051 x7). Only C# keywords are escaped and all are
	// lowercase, so the stripped name is always judged internal, never wrongly public.
	name = strings.TrimPrefix(name, "@")

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
		// A container-element index (`mr.readers[0]`, `m[k]`) already types as its ELEMENT — the
		// interface itself — so keep the whole expression. Redirect to X only when the indexed
		// expression is not interface-typed (e.g. a generic instantiation F[T]); redirecting a
		// container gave the SLICE/MAP type and recorded/keyed the conversion on the wrong type.
		exprType := v.getType(interfaceExpr, false)
		exprIsInterface := false

		if exprType != nil {
			exprIsInterface, _ = isInterface(exprType)
		}

		if !exprIsInterface {
			interfaceExpr = indexExpr.X
		}
	}

	return v.convertToInterfaceType(v.getType(interfaceExpr, false), v.getType(targetExpr, false), exprResult)
}

// isLocalImplType reports whether the impl type for an interface implementation (GoImplement) is
// declared in the package currently being converted. A GoImplement attribute is realized by the
// partial-struct generator, which can only add an interface to a type defined in the SAME assembly;
// an impl type imported from another package must therefore NOT be recorded here. That relationship
// is already established in the impl type's own package (e.g. color.RGBA's `Color` implementation
// lives in the image/color assembly), so re-emitting it in a consumer such as image/color/palette
// only generates a broken cross-assembly partial (CS1929/CS0034). Pointer types are unwrapped, and
// lifted/anonymous types (always local) are treated as local.
func (v *Visitor) isLocalImplType(t types.Type) bool {
	if _, ok := v.liftedTypeMap[t]; ok {
		return true
	}

	if pointer, ok := t.(*types.Pointer); ok {
		return v.isLocalImplType(pointer.Elem())
	}

	if named, ok := t.(*types.Named); ok {
		pkg := named.Obj().Pkg()
		return pkg != nil && pkg == v.pkg
	}

	return false
}

func (v *Visitor) convertToInterfaceType(interfaceType types.Type, targetType types.Type, exprResult string) string {
	// Track interface types that need to an implementation mapping
	// to properly handle duck typed Go interface implementations
	var interfaceTypeName string

	if iface, ok := interfaceType.(*types.Interface); ok && !iface.Empty() {
		// An ANONYMOUS non-empty interface (a bare *types.Interface, not through a Named)
		// resolves to its LIFTED name — the raw Go literal is not valid C# (its `}` breaks
		// the GoImplement assembly attribute and the adapter class name; internal/trace's
		// `readBatch(r interface{io.Reader; io.ByteReader})` cast cross-file from
		// generation.go, CS1730 cascade). Prefer this file's lift, then the shared package
		// registry, then a deferred marker resolved after the file-visit barrier — the same
		// three-step resolution dynamicStructTypeName performs for anonymous structs.
		if name, ok := v.liftedTypeMap[interfaceType]; ok {
			interfaceTypeName = name
		} else if name := lookupDynamicTypeName(interfaceType.String()); name != "" {
			interfaceTypeName = name
		} else {
			interfaceTypeName = dynamicTypeMarkerPrefix + interfaceType.String() + dynamicTypeMarkerSuffix
		}
	} else {
		interfaceTypeName = convertToCSTypeName(v.getFullTypeName(interfaceType, false))
	}

	targetTypeName := convertToCSTypeName(v.getFullTypeName(targetType, false))

	// Register the interface's DECLARED embeds so the inheritance PRUNE sees CROSS-ASSEMBLY
	// relations too — elf's errorReader records both io.ReadSeeker and io.Reader; C#'s
	// ReadSeeker : Reader (structural inheritance emitted at io's own declaration) makes the
	// two value-form partials implement Reader.Read twice (CS0111 + CS8646) unless the
	// subsumed record prunes. interfaceInheritances was only populated at LOCAL interface
	// declarations (visitInterfaceType), so a foreign base was invisible here.
	if named, ok := types.Unalias(interfaceType).(*types.Named); ok {
		if iface, ok := named.Underlying().(*types.Interface); ok && iface.NumEmbeddeds() > 0 {
			var embeds []string

			for ei := range iface.NumEmbeddeds() {
				if embNamed, ok := types.Unalias(iface.EmbeddedType(ei)).(*types.Named); ok {
					if _, isIface := embNamed.Underlying().(*types.Interface); isIface {
						embeds = append(embeds, convertToCSTypeName(v.getFullTypeName(embNamed, false)))
					}
				}
			}

			if len(embeds) > 0 {
				packageLock.Lock()

				if existing, ok := interfaceInheritances[interfaceTypeName]; ok {
					for _, embed := range embeds {
						existing.Add(embed)
					}
				} else {
					interfaceInheritances[interfaceTypeName] = NewHashSet(embeds)
				}

				packageLock.Unlock()
			}
		}
	}

	if targetTypeName == "" || targetTypeName == "nil" || targetTypeName == "any" {
		return exprResult
	}

	var prefix string
	pointerTarget := false

	if strings.HasPrefix(targetTypeName, PointerPrefix+"<") {
		targetTypeName = targetTypeName[3 : len(targetTypeName)-1]
		pointerTarget = true
		prefix = PointerDerefOp
	}

	// An interface-to-interface conversion is never recorded: it is satisfied by the C#
	// inheritance emitted at the interface DECLARATION (structural bases — see
	// getStructuralInterfaceBases), not by a generated impl partial. The generator's impl
	// types are structs; an interface-typed record kills its whole run (every other
	// GoImplement partial in the package vanishes — CrossPkgUser counter CS0029).
	targetIsIface, _ := isInterface(targetType)

	recordableBase := !targetIsIface &&
		interfaceTypeName != "" && interfaceTypeName != "nil" &&
		interfaceTypeName != targetTypeName &&
		interfaceTypeName != "any" &&
		!strings.Contains(targetTypeName, "interface{")

	recordable := recordableBase && v.isLocalImplType(targetType)

	// A VALUE conversion of a FOREIGN type to a LOCAL interface (os.Signal is DOWNSTREAM
	// of syscall.Signal - neither assembly can partial the other) records too: the
	// generator emits a local VALUE ADAPTER class wrapping a COPY (Go value semantics)
	// instead of the impossible foreign partial (exec_posix p.Signal(Kill), CS1503).
	// Gate to a NAMED foreign type - tuples/other shapes must not record (the first cut
	// wrapped a destructured tuple result in a phantom adapter).
	targetIsForeignNamed := false

	if named, ok := types.Unalias(targetType).(*types.Named); ok {
		if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg {
			targetIsForeignNamed = true
		}
	}

	recordableValueForeign := recordableBase && !pointerTarget && targetIsForeignNamed && !v.isLocalImplType(targetType) && v.isLocalImplType(interfaceType)

	// A VALUE conversion where BOTH sides are FOREIGN (encoding/binary's `BigEndian` passed as
	// binary.ByteOrder from debug/plan9obj): when the defining assembly already implements the
	// pair (its package_info carries the value-form GoImplement record), the bare value converts
	// implicitly — otherwise record the pair locally so the generator emits the LOCAL value
	// adapter for the foreign struct (same route as the local-interface case above).
	if recordableBase && !pointerTarget && targetIsForeignNamed && !v.isLocalImplType(interfaceType) {
		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg {
				// The interface side of the key is the CANONICAL QUALIFIED name — the simple
				// name collides across same-named interfaces (image's Paletted→image.Image
				// record must not satisfy a Paletted→draw.Image cast; see
				// canonicalRecordIfaceName). A dotless render qualifies with the INTERFACE's
				// own package (not the target struct's).
				ifacePkgName := pkg.Name()

				if ifaceNamed, ok := types.Unalias(interfaceType).(*types.Named); ok {
					if ifacePkg := ifaceNamed.Obj().Pkg(); ifacePkg != nil {
						ifacePkgName = ifacePkg.Name()
					}
				}

				key := fmt.Sprintf("%s|%s|%s", getSanitizedIdentifier(pkg.Name()),
					removeSanitizationMarker(getCoreSanitizedIdentifier(named.Obj().Name())),
					canonicalRecordIfaceName(interfaceTypeName, ifacePkgName))

				packageLock.Lock()
				foreignValueImplExists := importedValueImplements.Contains(key)
				packageLock.Unlock()

				if !foreignValueImplExists {
					recordableValueForeign = true
				}
			}
		}
	}

	if recordable || recordableValueForeign {
		// A POINTER-sourced cast records the ж<T>-wrapped name; the attribute emission unwraps
		// it to `GoImplement<T, Iface>(Pointer = true)`, which generates the IжAdapter wrapper
		// instead of the value-boxing partial struct (see convert-to-interface emission below).
		recordName := targetTypeName

		if pointerTarget {
			recordName = PointerPrefix + "<" + targetTypeName + ">"
		}

		packageLock.Lock()

		if implementations, exists := interfaceImplementations[interfaceTypeName]; exists {
			implementations.Add(recordName)
		} else {
			interfaceImplementations[interfaceTypeName] = NewHashSet([]string{recordName})
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
					// Underlying() is used ONLY for the struct-KIND checks below — recording the
					// underlying's name stringifies a raw *types.Struct into Go-ish text
					// (`GoImplicitConv<struct{p printer}, …>` and a mangled anonymous decoder-state
					// monster in encoding/xml's package_info.cs — not valid C# attribute args).
					interfaceParamType := interfaceMethodSignature.Params().At(j).Type()
					targetParameterType := targetMethodSignature.Params().At(j).Type()

					// Check if targetParamType is a struct or a pointer to a struct
					if ptrType, ok := targetParameterType.Underlying().(*types.Pointer); ok {
						targetParameterType = ptrType.Elem()
					}

					if _, ok := targetParameterType.Underlying().(*types.Struct); ok {
						// Check if interfaceParamType is a struct or a pointer to a struct
						if ptrType, ok := interfaceParamType.Underlying().(*types.Pointer); ok {
							interfaceParamType = ptrType.Elem()
						}

						if _, ok := interfaceParamType.Underlying().(*types.Struct); ok {
							// Both interfaceParamType and targetParamType are structs, track implicit conversions
							interfaceParamTypeName := v.implicitConvStructTypeName(interfaceParamType)
							targetParamTypeName := v.implicitConvStructTypeName(targetParameterType)

							// An IDENTICAL pair (the interface and target methods share the
							// parameter type) is a self-conversion — meaningless, and a
							// user-defined operator cannot convert a type to itself (CS0555 from
							// the generator). Marker-form names compare by signature, so identical
							// anonymous structs are also skipped; differing markers resolve after
							// the barrier (see resolveImplicitConvTypeName).
							if interfaceParamTypeName == targetParamTypeName {
								continue
							}
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

	// A POINTER-sourced cast to a locally-implemented interface routes through the generated
	// IжAdapter wrapper: Go's interface value holds the *T, so the adapter aliases the receiver
	// box exactly — every call through the interface mutates the original object, direct-ж
	// receiver methods bind on the box, and a type assert back to *T unwraps to the same box.
	// The old `~box` deref boxed a COPY into the C# interface (aliasing divergence) and could
	// not serve direct-ж members at all (math/rand lockedSource CS1929/CS1503). Non-local impl
	// types keep the deref-copy form below (their adapter is not generated in this assembly).
	// A recorded VALUE-form foreign conversion references the LOCAL value adapter. Its class
	// name is PACKAGE-QUALIFIED (`new syscall_ΔSignalᴠΔSignal(sig)`) for the same reason as
	// the local pointer adapters: two same-named foreign types adapting to one interface
	// otherwise compose a single colliding class (math/big's bytes.Reader + strings.Reader).
	if recordableValueForeign && exprResult != "" {
		qualifiedTarget := targetTypeName

		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg {
				simpleTarget := targetTypeName

				if idx := strings.LastIndex(simpleTarget, "."); idx >= 0 {
					simpleTarget = simpleTarget[idx+1:]
				}

				qualifiedTarget = getSanitizedIdentifier(pkg.Name()) + "_" + simpleTarget
			}
		}

		return fmt.Sprintf("new %s(%s)", valueAdapterTypeRef(qualifiedTarget, interfaceTypeName), exprResult)
	}

	// A LOCAL NAMED FUNC type with methods (flag's funcValue implementing Value): a C#
	// delegate cannot be a partial struct, so the generator emits a VALUE adapter class —
	// reference it at the conversion site (the record fires via the recordable arm above).
	if recordable && !pointerTarget && exprResult != "" {
		if named, ok := types.Unalias(targetType).(*types.Named); ok {
			if _, isSig := named.Underlying().(*types.Signature); isSig {
				return fmt.Sprintf("new %s(%s)", valueAdapterTypeRef(targetTypeName, interfaceTypeName), exprResult)
			}
		}
	}

	if pointerTarget && recordable && exprResult != "" {
		return fmt.Sprintf("new %s(%s)", adapterTypeRef(targetTypeName, interfaceTypeName), exprResult)
	}

	// A POINTER-sourced cast to an interface implemented by a FOREIGN type routes through the
	// foreign assembly's PUBLIC adapter when that package recorded the same pointer-implement
	// pair (parsed from its package_info) - os's `err = &PathError{...}` references io/fs's
	// generated `io.fs_package.PathErrorжerror` (the bare value emission was CS0029 x38: the
	// foreign VALUE struct does not implement the interface; only its pointer adapter does).
	if pointerTarget && !recordable && exprResult != "" {
		// The pointer-sourced target arrives as the *types.Pointer - unwrap to its named elem.
		namedTarget := targetType

		if ptr, ok := namedTarget.(*types.Pointer); ok {
			namedTarget = ptr.Elem()
		}

		if named, ok := types.Unalias(namedTarget).(*types.Named); ok {
			if pkg := named.Obj().Pkg(); pkg != nil && pkg != v.pkg {
				// The interface side of the key is CANONICAL QUALIFIED — the simple name
				// collides across same-named interfaces (image's Paletted→image.Image record
				// satisfied a Paletted→draw.Image cast, referencing the foreign adapter that
				// implements the WRONG interface; image/draw CS1503).
				ifacePkgName := pkg.Name()

				if ifaceNamed, ok := types.Unalias(interfaceType).(*types.Named); ok {
					if ifacePkg := ifaceNamed.Obj().Pkg(); ifacePkg != nil {
						ifacePkgName = ifacePkg.Name()
					}
				}

				key := fmt.Sprintf("%s|%s|%s", getSanitizedIdentifier(pkg.Name()),
					removeSanitizationMarker(getCoreSanitizedIdentifier(named.Obj().Name())),
					canonicalRecordIfaceName(interfaceTypeName, ifacePkgName))

				packageLock.Lock()
				foreignAdapterExists := importedPointerImplements.Contains(key)
				packageLock.Unlock()

				if foreignAdapterExists {
					// Reference through the file-local package ALIAS (`CrossPkgLib.Meter` +
					// adapter suffix), not the raw package-class qualifier
					// (`CrossPkgLib_package.…`) - user-ruled style; getTypeName both yields
					// the aliased form and registers the file-local using for it.
					adapterBase := convertToCSTypeName(v.getTypeName(named, false))

					return fmt.Sprintf("new %s(%s)", adapterTypeRef(adapterBase, interfaceTypeName), exprResult)
				}

				// NO exported adapter — the defining package never converts this pair itself
				// (os never casts *File to io.Reader). Record the pairing LOCALLY: the
				// generator emits a LOCAL adapter class for the foreign struct, binding its
				// methods from metadata (fmt's Fscan(os.Stdin, …), CS1503 ×3). Aliasing is
				// faithful — the adapter wraps the ж<T> box itself, unlike the deref-COPY
				// fallback this replaces. The class name is PACKAGE-QUALIFIED
				// (`os_FileжReader`): two same-named foreign structs adapting to the same
				// interface otherwise compose ONE colliding class (math/big records both
				// bytes.Reader and strings.Reader against io.ByteScanner — CS0102/CS0111).
				if recordableBase && exprResult != "" {
					recordName := PointerPrefix + "<" + targetTypeName + ">"

					packageLock.Lock()

					if implementations, exists := interfaceImplementations[interfaceTypeName]; exists {
						implementations.Add(recordName)
					} else {
						interfaceImplementations[interfaceTypeName] = NewHashSet([]string{recordName})
					}

					packageLock.Unlock()

					simpleTarget := targetTypeName

					if idx := strings.LastIndex(simpleTarget, "."); idx >= 0 {
						simpleTarget = simpleTarget[idx+1:]
					}

					qualifiedTarget := getSanitizedIdentifier(pkg.Name()) + "_" + simpleTarget

					return fmt.Sprintf("new %s(%s)", adapterTypeRef(qualifiedTarget, interfaceTypeName), exprResult)
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

// adapterTypeRef renders the reference to the generated pointer-interface adapter class for a
// *T → iface cast: `<struct>ж<ifaceSimple>` (PointerPrefix - user-ruled style; keep in
// sync with the generator, which composes the same name via Symbols.PointerPrefix), nested in the struct's package class like
// the struct itself, so a same-package reference is the bare name. The interface side uses its
// SIMPLE name — the generator derives the same identifier via GetSimpleName, so both sides must
// agree on last-dot-segment naming.
// valueAdapterTypeRef renders the reference to the generated VALUE-form foreign adapter
// class: `<structSimple>ᴠ<ifaceSimple>` (ValueAdapterInfix), emitted in the INTERFACE's
// package (the converting package), so the reference is the bare composed name.
func valueAdapterTypeRef(structTypeName string, interfaceTypeName string) string {
	structSimple := structTypeName

	if idx := strings.LastIndex(structSimple, "."); idx >= 0 {
		structSimple = structSimple[idx+1:]
	}

	ifaceSimple := interfaceTypeName

	// A deferred dynamic-type marker (an anonymous interface lifted in a not-yet-visited
	// file) must survive INTACT to the post-barrier resolution — its embedded signature
	// contains dots (`interface{io.Reader; …}`), so the simple-name strip would mangle it.
	if !strings.Contains(ifaceSimple, dynamicTypeMarkerPrefix) {
		if idx := strings.LastIndex(ifaceSimple, "."); idx >= 0 {
			ifaceSimple = ifaceSimple[idx+1:]
		}
	}

	return structSimple + ValueAdapterInfix + ifaceSimple
}

func adapterTypeRef(structTypeName string, interfaceTypeName string) string {
	ifaceSimple := interfaceTypeName

	// Keep a deferred dynamic-type marker intact (see valueAdapterTypeRef).
	if !strings.Contains(ifaceSimple, dynamicTypeMarkerPrefix) {
		if idx := strings.LastIndex(ifaceSimple, "."); idx >= 0 {
			ifaceSimple = ifaceSimple[idx+1:]
		}
	}

	// A GENERIC struct's closed type-argument list must TRAIL the adapter name, not sit inside
	// it. `nistCurve<ж<P224Point>>` + PointerPrefix + `Curve` otherwise composes the malformed
	// `nistCurve<ж<P224Point>>жCurve` (an identifier with `<…>` mid-name — CS1526). Split the
	// base name from its `<…>` args so the reference reads `nistCurveжCurve<ж<P224Point>>`:
	// base+PointerPrefix+iface NAMES the ONE generic adapter class the generator emits (from the
	// open `nistCurve<Point>` form), and the closed args instantiate it. Non-generic names have
	// no `<`, so this is a no-op for them. Keep in sync with ImplementGenerator's generic-adapter
	// emission (which composes the class name identically via Symbols.PointerPrefix).
	structBase := structTypeName
	typeArgs := ""

	if idx := strings.Index(structTypeName, "<"); idx >= 0 {
		structBase = structTypeName[:idx]
		typeArgs = structTypeName[idx:]
	}

	return structBase + PointerPrefix + ifaceSimple + typeArgs
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

func isEmptyStruct(structType *ast.StructType) bool {
	if structType == nil {
		return false
	}

	// Empty struct has no fields
	return len(structType.Fields.List) == 0
}

func isEmptyStructType(structType *types.Struct) bool {
	if structType == nil {
		return false
	}

	// Empty struct has no fields
	return structType.NumFields() == 0
}

func (v *Visitor) extractStructType(expr ast.Expr) (*ast.StructType, types.Type) {
	if starExpr, ok := expr.(*ast.StarExpr); ok {
		if structType, ok := starExpr.X.(*ast.StructType); ok && !isEmptyStruct(structType) {
			return structType, v.getType(starExpr.X, false)
		}
	} else if compositeLit, ok := expr.(*ast.CompositeLit); ok {
		if structType, ok := compositeLit.Type.(*ast.StructType); ok && !isEmptyStruct(structType) {
			return structType, v.getType(compositeLit.Type, false)
		}
	} else if arrayType, ok := expr.(*ast.ArrayType); ok {
		if structType, ok := arrayType.Elt.(*ast.StructType); ok && !isEmptyStruct(structType) {
			return structType, v.getType(arrayType.Elt, false)
		}
	} else if indexExpr, ok := expr.(*ast.IndexExpr); ok {
		if structType, ok := indexExpr.X.(*ast.StructType); ok && !isEmptyStruct(structType) {
			return structType, v.getType(indexExpr.X, false)
		}
	} else if sliceExpr, ok := expr.(*ast.SliceExpr); ok {
		if structType, ok := sliceExpr.X.(*ast.StructType); ok && !isEmptyStruct(structType) {
			return structType, v.getType(sliceExpr.X, false)
		}
	} else if callExpr, ok := expr.(*ast.CallExpr); ok {
		if structType, ok := callExpr.Fun.(*ast.StructType); ok && !isEmptyStruct(structType) {
			return structType, v.getType(callExpr.Fun, false)
		}
	} else if typeAssertExpr, ok := expr.(*ast.TypeAssertExpr); ok {
		if structType, ok := typeAssertExpr.Type.(*ast.StructType); ok && !isEmptyStruct(structType) {
			return structType, v.getType(typeAssertExpr.Type, false)
		}
	} else if selectorExpr, ok := expr.(*ast.SelectorExpr); ok {
		if structType, ok := selectorExpr.X.(*ast.StructType); ok && !isEmptyStruct(structType) {
			return structType, v.getType(selectorExpr.X, false)
		}
	} else if structType, ok := expr.(*ast.StructType); ok && !isEmptyStruct(structType) {
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
			// An unconstrained (`any`) type parameter gets NO C# constraint. Previously `new()` was
			// added (so `@new<T>`/`make` could construct it, and to force `@string` over `System.String`
			// for generic string args). But `new()` rejects a delegate/func type argument — Go's
			// `atomic.Pointer[func()]` is valid yet `Pointer<Action>` failed CS0310 — and it is no
			// longer required: golib `@new<T>` constructs via the runtime (no new() bound), and string
			// literals are cast to `@string` at generic call sites. Leave it unconstrained.
			constraintName = ""
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
				// `string | []byte` union members share no operators, so suppress the spurious lifted
				// operator constraints (IAddition/IComparison/...) for it; set in the union branch below.
				suppressLiftedConstraints := false

				// Check for common Go types, e.g., slice, map, channel, etc. The `string | []byte`
				// UNION is checked FIRST: its `[]byte | string` ordering starts with "[]" and would
				// otherwise take the ISlice branch with the raw union as the element type
				// (`ISlice<byte | string>` — CS1003 cascade, time/format.go's appendNano family).
				if constraintExpr == "string|[]byte" || constraintExpr == "[]byte|string" {
					// Go's `string | []byte` union — emit the read-only byte-sequence interface
					// both @string and slice<byte> implement (IByteSeq<byte>); C# cannot express
					// the "or" directly. See golib IByteSeq.
					typeConstraint = "IByteSeq<byte>"
					suppressLiftedConstraints = true
				} else if strings.HasPrefix(constraintExpr, "[]") {
					// Handle slice via ISlice interface. ISliceWrap supplies the S-preserving factory:
					// a sub-slice or append of a constrained S must yield S again (Go's named-slice
					// semantics), which golib's subslice<S, T>/append<S, T> realize through S.Wrap.
					elemType := convertToCSTypeName(constraintName[2:])
					typeConstraint = fmt.Sprintf("ISlice<%s>, ISupportMake<%s>, ISliceWrap<%s, %s>", elemType, typeParamNames[i], typeParamNames[i], elemType)
				} else if arrayElem, isArrayCore := v.getArrayConstraintElem(iface); isArrayCore {
					// Handle an array-core constraint `~[N]E` (ML-KEM's `~[256]fieldElement`) via the
					// IArray interface. The named-array [GoType] wrapper (ringElement, nttElement)
					// implements IArray<E>, so this exposes the array surface — indexing, length,
					// `(nint, E)` ranging/deconstruction — that the generic body binds against, and
					// the wrapper type arguments satisfy the constraint. The Array member of the
					// comparable operator set would otherwise lift IEqualityOperators<T, T, bool>,
					// which the wrapper cannot satisfy and which exposes no array surface (CS0315,
					// plus CS0021/CS1579/CS8130 on the body's `t[i]`/`range t`/deconstruction).
					elemType := convertToCSTypeName(v.getTypeName(arrayElem, false))
					typeConstraint = fmt.Sprintf("IArray<%s>", elemType)
					suppressLiftedConstraints = true
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
					// Handle special case for string and []byte types (the union form is hoisted
					// to the head of this chain - see above)
					if constraintExpr == "string" || constraintExpr == "[]byte" {
						typeConstraint = "ISlice<byte>"
					} else if constraintExpr == "comparable" {
						// Go's built-in `comparable` admits every ==-able Go type — numerics,
						// strings, pointers, channels, comparable structs/arrays/interfaces. No C#
						// constraint can express that set: golib's comparable<T> CRTP is
						// implemented by NOTHING (every real instantiation failed — blocking
						// maps.Keys), and lifting IEqualityOperators would reject structs, which
						// Go admits. Emit NO C# constraint beyond new(): Go's checker already
						// validated every instantiation, and emitted equality on type parameters
						// routes through AreEqual (object equality), not operator ==.
						constraintNames = append(constraintNames, fmt.Sprintf("%s%s    where %s : %s new()", v.newline, v.indent(v.indentLevel), typeParamNames[i], originalConstraint))
						continue
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

					// The `string | []byte` union has no common operators; drop the spurious lifted set.
					if suppressLiftedConstraints {
						liftedConstraints = ""
					}

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
				} else if iface.IsMethodSet() {
					// A REGULAR method-set interface (a pure method set, no type-term unions —
					// go/ast's `Node` in `walkList[N Node]`) is emitted arity-0 by
					// visitInterfaceType, NOT as the generic CRTP form that union+method
					// constraint interfaces take below (`ConstraintTest1<ΔT>`), so the type
					// parameter constrains against the interface itself (`where N : Node` —
					// the phantom `Node<N>` was CS0308). NO `new()` either: the instantiation
					// may itself be an INTERFACE (walkList takes N=Stmt/Expr/Spec/Decl), which
					// cannot satisfy a constructor constraint.
					constraintNames = append(constraintNames, fmt.Sprintf("%s%s    where %s : %s", v.newline, v.indent(v.indentLevel), typeParamNames[i], convertToCSTypeName(constraintName)))
					continue
				} else {
					// If interface has methods, can safely assume generic type must implement it directly
					constraintName = fmt.Sprintf("%s<%s>", constraintName, typeParamNames[i])
				}

				constraintName = fmt.Sprintf("%s, new()", constraintName)
			} else {
				v.showWarning("@getGenericDefinition - constraint `%s` on `%s` is not an interface", constraintName, srcType.String())
			}
		}

		// An unconstrained type parameter emits no `where` clause at all (the type-param name still
		// appears in the `<…>` list above).
		if len(constraintName) == 0 {
			continue
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

func getGlobalTempVarName(varPrefix string) string {
	if globalTempVarCount == nil {
		globalTempVarCount = make(map[string]int)
	}

	count := globalTempVarCount[varPrefix]
	count++
	globalTempVarCount[varPrefix] = count

	return fmt.Sprintf("%s%s%d", varPrefix, TempVarMarker, count)
}

func (v *Visitor) getUniqueLiftedTypeName(typeName string) string {
	// Recover the original Go name by stripping BOTH sanitization markers ('@' and the Δ collision
	// rename) so the typeExists check below hits the real package scope (which holds the unsanitized
	// name). The lift is often called with the already-sanitized name (e.g. `Δtrace`).
	originalName := strings.TrimPrefix(removeSanitizationMarker(typeName), ShadowVarMarker)
	typeName = getSanitizedIdentifier(originalName)
	uniqueTypeName := typeName
	count := 0

	// typeExists looks names up in the Go package scope, which holds UNSANITIZED names. A lifted type
	// named after a global var that is Δ-renamed (e.g. a `var trace struct{…}` whose anon type lifts
	// to `trace`→`Δtrace`) would check the sanitized `Δtrace`, miss the `trace` var, and collide with
	// it (a nested type + a property both named `Δtrace`, CS0102). Also test the original name so the
	// first iteration forces a `ᴛ1` suffix in that case.
	for v.liftedTypeNames.Contains(uniqueTypeName) || v.typeExists(uniqueTypeName) || (count == 0 && v.typeExists(originalName)) {
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

// isUnsignedType reports whether the expression's contextual type is an unsigned
// integer. An untyped constant adopts its target type (e.g. a uint32 argument), so
// this drives correct C# literal suffixing for values outside the int32 range.
func (v *Visitor) isUnsignedType(expr ast.Expr) bool {
	if tv, ok := v.info.Types[expr]; ok && tv.Type != nil {
		if basic, ok := tv.Type.Underlying().(*types.Basic); ok {
			return basic.Info()&types.IsUnsigned != 0
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

	if chanType, ok := expr.(*ast.ChanType); ok {
		// Check if the channel value is an anonymous struct
		if structType, exprType := v.extractStructType(chanType.Value); structType != nil && !v.liftedTypeExists(structType) {
			v.indentLevel++
			v.visitStructType(structType, exprType, "channel", nil, true, nil)
			v.indentLevel--
		}

		// Check if the channel value is an anonymous interface
		if interfaceType, exprType := v.extractInterfaceType(chanType.Value); interfaceType != nil && !v.liftedTypeExists(interfaceType) {
			v.indentLevel++
			v.visitInterfaceType(interfaceType, exprType, "channel", nil, true, nil)
			v.indentLevel--
		}
	}

	return v.getTypeName(v.getType(expr, underlying), underlying)
}

// collectTypePackages records the import paths of every foreign (non-current-package) named type
// reachable in t — directly, or as a pointer/slice/array/map/chan element, a generic type argument,
// or a func-signature parameter/result — into referencedForeignPackages, plus the pseudo-path
// "unsafe" for an unsafe.Pointer basic. These are the packages whose short-alias type names
// (`pkg.Type`, `@unsafe.Pointer`) the converter emits, so visitFile can supply the matching
// `using <alias> = <namespace>;`. A named type is recorded by its own package only — its underlying
// fields belong to its own declaration and are not emitted inline, so the walk does not recurse into
// Underlying (which also avoids cycles on recursive struct types).
func (v *Visitor) collectTypePackages(t types.Type, seen map[types.Type]bool) {
	if t == nil {
		return
	}

	if seen == nil {
		seen = map[types.Type]bool{}
	}

	if seen[t] {
		return
	}

	seen[t] = true

	switch u := t.(type) {
	case *types.Basic:
		if u.Kind() == types.UnsafePointer {
			v.referencedForeignPackages.Add("unsafe")
		}
	case *types.Named:
		if obj := u.Obj(); obj != nil {
			if pkg := obj.Pkg(); pkg != nil && pkg != v.pkg {
				v.referencedForeignPackages.Add(pkg.Path())
			}
		}

		if typeArgs := u.TypeArgs(); typeArgs != nil {
			for i := range typeArgs.Len() {
				v.collectTypePackages(typeArgs.At(i), seen)
			}
		}
	case *types.Pointer:
		v.collectTypePackages(u.Elem(), seen)
	case *types.Slice:
		v.collectTypePackages(u.Elem(), seen)
	case *types.Array:
		v.collectTypePackages(u.Elem(), seen)
	case *types.Map:
		v.collectTypePackages(u.Key(), seen)
		v.collectTypePackages(u.Elem(), seen)
	case *types.Chan:
		v.collectTypePackages(u.Elem(), seen)
	case *types.Signature:
		if params := u.Params(); params != nil {
			for i := range params.Len() {
				v.collectTypePackages(params.At(i).Type(), seen)
			}
		}

		if results := u.Results(); results != nil {
			for i := range results.Len() {
				v.collectTypePackages(results.At(i).Type(), seen)
			}
		}
	}
}

// methodlessNamedFuncSignature reports the underlying *types.Signature when t is a NON-GENERIC
// named func type with NO methods — `type releaseConn func(error)`, `context.CancelFunc`. Go treats
// such a type as freely interconvertible with its underlying `func(...)` (the name is purely
// documentary when there are no methods), but the converter would otherwise emit a distinct C#
// delegate (`ΔreleaseConn`) incompatible with the base `Action<error>` its underlying renders to —
// so a value flowing between the two (database/sql's grabConn returns `releaseConn`, queryDC takes
// `func(error)`) fails (CS1503/CS0029, and the mismatch blocks the ж-receiver overload → CS1929).
// Such a type is rendered AS its base delegate everywhere and its declaration is skipped
// (visitFuncType), making the conversions identity, exactly as Go models them. A named func type
// WITH methods keeps its distinct delegate (its method set is meaningful); a GENERIC one keeps its
// name (it is referenced as `Seq<V>`, and the type parameter must stay in scope).
func methodlessNamedFuncSignature(t types.Type) (*types.Signature, bool) {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok {
		return nil, false
	}

	if named.NumMethods() != 0 || named.TypeParams() != nil {
		return nil, false
	}

	sig, ok := named.Underlying().(*types.Signature)

	if !ok {
		return nil, false
	}

	// Don't collapse when the signature references ANY named func type (its own or another) in
	// its params/results. A SELF-referential func type — `type stateFn func(*machine) stateFn`
	// (a Go state machine) — has no finite base-delegate form (`Func<M, Func<M, …>>` is infinite),
	// and a reference to ANOTHER named func type would leave that name undefined after collapse
	// (FirstClassFunctions' `strategy func(score) action`). Keeping such a type as a named
	// delegate is correct and self-consistent; only the leaves of the func-type reference graph
	// (whose signatures name no func types — database/sql's `releaseConn func(error)`, context's
	// `CancelFunc func()`) collapse.
	if signatureReferencesNamedFuncType(sig) {
		return nil, false
	}

	return sig, true
}

// signatureReferencesNamedFuncType reports whether any param/result of sig is (or, through
// pointer/slice/array/map/chan wrappers, contains) a NAMED type whose underlying is a func
// signature — i.e. a named func type. Used to keep self- or mutually-referential func types as
// named delegates (see methodlessNamedFuncSignature). Struct fields are not descended into (a
// struct-typed param does not make the delegate recursive).
func signatureReferencesNamedFuncType(sig *types.Signature) bool {
	var referencesFunc func(t types.Type) bool

	referencesFunc = func(t types.Type) bool {
		switch typ := t.(type) {
		case *types.Named:
			if _, ok := typ.Underlying().(*types.Signature); ok {
				return true
			}
		case *types.Pointer:
			return referencesFunc(typ.Elem())
		case *types.Slice:
			return referencesFunc(typ.Elem())
		case *types.Array:
			return referencesFunc(typ.Elem())
		case *types.Map:
			return referencesFunc(typ.Key()) || referencesFunc(typ.Elem())
		case *types.Chan:
			return referencesFunc(typ.Elem())
		}

		return false
	}

	tuples := []*types.Tuple{sig.Params(), sig.Results()}

	for _, tuple := range tuples {
		if tuple == nil {
			continue
		}

		for i := range tuple.Len() {
			if referencesFunc(tuple.At(i).Type()) {
				return true
			}
		}
	}

	return false
}

// constraintProxyArg reports the C# constraint-proxy type name to render for type argument i of an
// instantiated generic `named`, when that argument is a POINTER to a named type AND the matching
// type parameter carries a SELF-REFERENTIAL generic method-set interface constraint — Go's
// `nistCurve[Point nistPoint[Point]]` instantiated with `*P224Point`. The golib box ж<P224Point>
// cannot NOMINALLY implement nistPoint<…> (it is a sealed golib type in another assembly, and Go's
// structural satisfaction has no C# analog), and the interface is self-referential so the value
// can't widen to the interface either. So the argument renders as the generated proxy
// `P224PointжnistPoint : nistPoint<itself>` (ImplementGenerator's EmitConstraintProxy), and this
// also registers the (element, interface) pair so package_info emits its ConstraintProxy record.
// Returns ("", false) for every other argument, leaving normal rendering untouched.
func (v *Visitor) constraintProxyArg(named *types.Named, i int) (string, bool) {
	origin := named.Origin()

	if origin == nil {
		return "", false
	}

	typeParams := origin.TypeParams()

	if typeParams == nil || i >= typeParams.Len() {
		return "", false
	}

	typeParam := typeParams.At(i)

	// The argument must be a pointer to a named type — a value type arg satisfies its constraint
	// nominally (or widens), only the boxed pointer needs the proxy.
	ptr, ok := named.TypeArgs().At(i).(*types.Pointer)

	if !ok {
		return "", false
	}

	elemNamed, ok := types.Unalias(ptr.Elem()).(*types.Named)

	if !ok {
		return "", false
	}

	// The constraint must be an INSTANTIATED generic method-set interface (nistPoint[Point]);
	// a plain non-generic method-set interface (go/ast's Node) widens to itself instead.
	constraintNamed, ok := typeParam.Constraint().(*types.Named)

	if !ok || constraintNamed.TypeArgs() == nil || constraintNamed.TypeArgs().Len() == 0 {
		return "", false
	}

	iface, ok := constraintNamed.Underlying().(*types.Interface)

	if !ok || iface.NumMethods() == 0 || !iface.IsMethodSet() {
		return "", false
	}

	// Self-referential: one of the constraint's type arguments IS the type parameter itself.
	selfReferential := false

	for j := 0; j < constraintNamed.TypeArgs().Len(); j++ {
		if tp, ok := constraintNamed.TypeArgs().At(j).(*types.TypeParam); ok && tp == typeParam {
			selfReferential = true
			break
		}
	}

	if !selfReferential {
		return "", false
	}

	interfaceOrigin := constraintNamed.Origin()

	// Proxy name element-simple + PointerPrefix + interface-simple — MUST match
	// ImplementGenerator's `elementType.Name + PointerPrefix + interfaceDef.Name`.
	proxyName := elemNamed.Obj().Name() + PointerPrefix + interfaceOrigin.Obj().Name()

	// Register the (element, interface) pair so package_info emits the ConstraintProxy record.
	// The interface name drops its type-parameter DECLARATION (`point[T any]` → `point`): the
	// record's `GoImplement<element, point<element>>` closes it over the element placeholder.
	// A CROSS-PACKAGE element renders to its C# full type name (nistec.P224Point →
	// `crypto.@internal.nistec_package.P224Point`, resolving the slash path); a SAME-PACKAGE
	// element stays BARE (convertToCSFullTypeName would root-qualify it to the wrong `go.p224`,
	// exactly as it would the local interface name below).
	elementFullName := v.getFullTypeName(elemNamed, false)

	if elemNamed.Obj().Pkg() != v.pkg {
		elementFullName = convertToCSFullTypeName(elementFullName)
	}

	interfaceFullName := v.getFullTypeName(interfaceOrigin, false)

	// Strip the type-parameter DECLARATION only — getFullTypeName already yields the interface's
	// C# reference form (bare `nistPoint` for a local interface, `pkg_package.Iface` cross-package),
	// so it must NOT go through convertToCSFullTypeName (which would root-qualify the bare local name
	// to the wrong `go.nistPoint`). qualifyLocalTypeRef handles final qualification at emission.
	if idx := strings.Index(interfaceFullName, "["); idx >= 0 {
		interfaceFullName = interfaceFullName[:idx]
	}

	packageLock.Lock()
	constraintProxies[elementFullName+"|"+interfaceFullName] = [2]string{elementFullName, interfaceFullName}
	packageLock.Unlock()

	return proxyName, true
}

// namedHasConstraintProxy reports whether any type argument of the instantiated generic `named`
// resolves to a self-referential constraint proxy (see constraintProxyArg) — used to re-render a
// composite-literal type through the resolved type so its type arguments match the proxy the
// pointer adapter wraps, rather than the box that convExpr's AST walk would emit.
func (v *Visitor) namedHasConstraintProxy(named *types.Named) bool {
	if named == nil || named.TypeArgs() == nil {
		return false
	}

	for i := 0; i < named.TypeArgs().Len(); i++ {
		if _, ok := v.constraintProxyArg(named, i); ok {
			return true
		}
	}

	return false
}

func (v *Visitor) getTypeName(t types.Type, isUnderlying bool) string {
	if t == nil {
		return ""
	}

	// A non-generic methodless named func type renders as its base C# delegate (its underlying
	// signature), matching Go's free named↔underlying func interconversion (see
	// methodlessNamedFuncSignature). Guarded so a pointer/composite wrapper still recurses here.
	if sig, ok := methodlessNamedFuncSignature(t); ok {
		v.collectTypePackages(t, nil)
		return v.getTypeName(sig, isUnderlying)
	}

	// Register any foreign package whose type is emitted here so visitFile can supply the file-local
	// `using <alias> = <namespace>;` even when the file did not import the package under its canonical
	// name (inferred type, blank/non-canonical alias import) — see the field comment. Walks composites
	// and generics so an element/argument type (`[]time.Duration`, `map[K]abi.Kind`, unsafe.Pointer
	// inside a slice) registers too, since those are emitted through the string path below without
	// recursing into getTypeName.
	v.collectTypePackages(t, nil)

	if pointer, ok := t.(*types.Pointer); ok {
		return "*" + v.getTypeName(pointer.Elem(), isUnderlying)
	}

	// A FOREIGN type ALIAS whose TARGET lives in yet ANOTHER package — `os.FileInfo = fs.FileInfo`
	// (os/types.go, target in io/fs) — is emitted as an assembly-scoped `global using FileInfo =
	// go.io.fs_package.FileInfo;` in ITS OWN package's conversion, NOT as a member of that package's C#
	// class, so a cross-package reference `os_package.FileInfo` does not resolve (CS0426, path/filepath's
	// `os.Lstat` func value). Render the alias's TARGET instead. Gated to a DIFFERENT-package target: an
	// alias to a SAME-package type (`CrossPkgLib.Temperature = Celsius`) already resolves through the
	// existing `ꓸ` global-using alias, so it is left untouched (no churn on that mechanism).
	if alias, ok := t.(*types.Alias); ok {
		if aliasObj := alias.Obj(); aliasObj != nil && aliasObj.Pkg() != nil && aliasObj.Pkg() != v.pkg {
			if targetNamed, ok := types.Unalias(t).(*types.Named); ok {
				if targetObj := targetNamed.Obj(); targetObj != nil && targetObj.Pkg() != nil && targetObj.Pkg() != aliasObj.Pkg() {
					return v.getTypeName(targetNamed, isUnderlying)
				}
			}
		}
	}

	if name, ok := v.liftedTypeMap[t]; ok {
		return name
	}

	// An array/slice type is rendered structurally — the `[N]`/`[]` marker plus the recursively
	// resolved element — rather than via t.String() below (mirrors getFullTypeName). t.String()
	// yields a path-qualified string (`[]*internal/abi.Type`) whose cross-package last-segment
	// slash-strip eats everything before the slash INCLUDING a pointer marker, so the element's
	// `*` is silently dropped (`slice<abi.Type>` instead of `slice<ж<abi.Type>>` — reflect
	// CS1503 ×16, plus SILENTLY WRONG type asserts that compiled). Recursing on the element also
	// resolves a lifted element and a cross-package generic element through their own arms.
	switch composite := t.(type) {
	case *types.Array:
		return fmt.Sprintf("[%d]%s", composite.Len(), v.getTypeName(composite.Elem(), isUnderlying))
	case *types.Slice:
		return "[]" + v.getTypeName(composite.Elem(), isUnderlying)
	case *types.Chan:
		// Structural like slices/arrays so a LIFTED element resolves - `make(chan dialResult)`
		// where dialResult is a FUNCTION-LOCAL type (net dialParallel) rendered
		// `channel<dialResult>` while every composite used the lifted name (CS0246).
		elem := v.getTypeName(composite.Elem(), isUnderlying)

		switch composite.Dir() {
		case types.RecvOnly:
			return "<-chan " + elem
		case types.SendOnly:
			return "chan<- " + elem
		default:
			return "chan " + elem
		}
	case *types.Map:
		// Structural like slices/chans — the t.String() path renders a PACKAGE-LOCAL value
		// type path-qualified (`map[chan<- os.Signal]*os/signal.handler`), whose cross-package
		// slash-strip eats everything before the slash including the map header (os/signal's
		// handlers.m emitted `map<channel/*<-*/<os.Signal>*handler>, >` — CS1003 cascade ×8).
		return fmt.Sprintf("map[%s]%s", v.getTypeName(composite.Key(), isUnderlying), v.getTypeName(composite.Elem(), isUnderlying))
	}

	// A cross-package INSTANTIATED generic (e.g. `internal/runtime/atomic.Pointer[func(string,
	// string)]`) must be rendered structurally — `pkg.Name() + "." + Name[args…]` with each arg
	// recursively named — rather than from t.String(). The string form keeps the full import path,
	// and the slash-strip that would reduce it is skipped whenever the string contains '(' (to
	// protect func types), so a func-type type-argument leaves the full path AND the pkg.Name()
	// alias gets prepended → a doubled `atomic.@internal.runtime.atomic.Pointer` (CS0426).
	if named, ok := t.(*types.Named); ok {
		obj := named.Obj()

		if typeArgs := named.TypeArgs(); typeArgs != nil && typeArgs.Len() > 0 {
			args := make([]string, typeArgs.Len())

			for i := 0; i < typeArgs.Len(); i++ {
				if proxyName, ok := v.constraintProxyArg(named, i); ok {
					args[i] = proxyName
				} else {
					args[i] = v.getTypeName(typeArgs.At(i), false)
				}
			}

			if pkg := obj.Pkg(); pkg != nil && pkg != v.pkg {
				return fmt.Sprintf("%s.%s[%s]", importQualifier(pkg.Name()), obj.Name(), strings.Join(args, ", "))
			}

			// A SAME-PACKAGE instantiated generic must ALSO render structurally — each type ARGUMENT
			// recursively named — rather than falling through to the t.String() path. When an argument
			// is itself cross-package, t.String() path-qualifies it (`curve[*repro/sub.Item]`), and the
			// cross-package slash-strip then eats everything before the slash INCLUDING the `curve[`
			// header, dropping the wrapper (crypto/elliptic's `*nistCurve[*nistec.P224Point]` →
			// `ж<nistec.P224Point>>`, a CS1519 cascade). Rendering args via getTypeName yields their
			// short, slash-free package-qualified names, so the header survives.
			return fmt.Sprintf("%s[%s]", obj.Name(), strings.Join(args, ", "))
		}
	}

	var pkgPrefix string
	var plainPkgPrefix string

	if named, ok := t.(*types.Named); ok {
		obj := named.Obj()
		pkg := obj.Pkg()

		// Handle builtin types with no package
		if pkg != nil && pkg != v.pkg {
			// Prefer THIS FILE's actual import alias for the type's package over the canonical
			// package name — cryptobyte's asn1.go imports `encoding/asn1` as `encoding_asn1`
			// (the vendored `.../cryptobyte/asn1` subpackage took the canonical `asn1`), so a
			// `*asn1.BitString` must render `encoding_asn1.BitString`, not `asn1.BitString` (which
			// resolves to the subpackage — CS0426). Only EXPLICITLY-aliased imports populate the map;
			// unaliased/Δ-renamed imports are absent and keep the importQualifier fallback — no churn.
			aliasQualifier := importQualifier(pkg.Name())

			if fileAlias, ok := v.importPathAliases[pkg.Path()]; ok && fileAlias != "" {
				aliasQualifier = fileAlias
			}

			pkgPrefix = aliasQualifier + "."
			plainPkgPrefix = pkg.Name() + "."
		}
	}

	if !isUnderlying {
		if structType, ok := t.(*types.Struct); ok && !isEmptyStructType(structType) {
			v.showWarning("Unresolved dynamic struct type: %s", t.String())
		} else if interfaceType, ok := t.(*types.Interface); ok && !interfaceType.Empty() {
			v.showWarning("Unresolved dynamic interface type: %s", t.String())
		}
	}

	typeName := strings.ReplaceAll(t.String(), "..", "")
	packagePathPrefix := v.pkg.Path() + "."

	// Remove the current package's path prefix from the type name. Use ReplaceAll, not a
	// single replace: a composite type (e.g. map[K]V) can name two current-package types, and
	// stripping only the first leaves a self-qualified one (which then also trips the slash
	// handling below for slash-bearing package paths like internal/platform → CS0246).
	typeName = strings.ReplaceAll(typeName, packagePathPrefix, "")

	// The slash-strip below reduces a remaining cross-package import path to its last segment
	// (`internal/platform.Foo` → `platform.Foo`). It must NOT touch a composite type string whose
	// slashes are inside it — e.g. a func type `func(go2cs/x/sub.Record)` would be butchered to
	// `sub.Record)`. Such strings are converted structurally downstream (convertToCSFunc... +
	// convertImportPathToNamespace handle their inner package paths), so skip the strip for them.
	if !strings.HasPrefix(typeName, "func") && !strings.Contains(typeName, "(") {
		slashIndex := strings.LastIndex(typeName, "/")

		if slashIndex != -1 {
			typeName = typeName[slashIndex+1:]
		}
	}

	if len(pkgPrefix) > 0 && !strings.HasPrefix(typeName, pkgPrefix) {
		// A Δ-renamed import alias (importQualifier) diverges from the PLAIN package name that
		// t.String() carries (`sync.Pool` under alias `Δsync`): strip the plain qualifier before
		// prepending the alias, or the name doubles — `Δsync.sync.Pool`, `'sync' does not exist
		// in the type 'sync_package'` (io/syscall CS0426 ×22). When alias == plain name the
		// HasPrefix guard above already short-circuits, so this strip only fires on renames.
		if len(plainPkgPrefix) > 0 && strings.HasPrefix(typeName, plainPkgPrefix) {
			typeName = typeName[len(plainPkgPrefix):]
		}

		return pkgPrefix + typeName
	}

	return typeName
}

func (v *Visitor) getFullTypeName(t types.Type, isUnderlying bool) string {
	if t == nil {
		return ""
	}

	if pointer, ok := t.(*types.Pointer); ok {
		return "*" + v.getFullTypeName(pointer.Elem(), isUnderlying)
	}

	// A non-generic methodless named func type renders as its base C# delegate (see
	// methodlessNamedFuncSignature / the getTypeName twin).
	if sig, ok := methodlessNamedFuncSignature(t); ok {
		return v.getFullTypeName(sig, isUnderlying)
	}

	if name, ok := v.liftedTypeMap[t]; ok {
		return name
	}

	// An array/slice type is rendered structurally — the `[N]`/`[]` marker plus the recursively
	// resolved element — rather than via t.String() below. t.String() yields a path-qualified string
	// (`[2]internal/runtime/atomic.Pointer[…]`) whose cross-package slash-strip would also strip the
	// leading `[N]` marker, dropping the array wrapper (`atomic.Pointer<…>` instead of
	// `array<atomic.Pointer<…>>`). Recursing on the element also resolves a lifted anonymous
	// struct/interface element (liftedTypeMap is keyed by the element) and a cross-package generic.
	switch composite := t.(type) {
	case *types.Array:
		return fmt.Sprintf("[%d]%s", composite.Len(), v.getFullTypeName(composite.Elem(), isUnderlying))
	case *types.Slice:
		return "[]" + v.getFullTypeName(composite.Elem(), isUnderlying)
	case *types.Chan:
		// Mirrors getTypeName's Chan arm (lifted channel elements; net dialParallel).
		elem := v.getFullTypeName(composite.Elem(), isUnderlying)

		switch composite.Dir() {
		case types.RecvOnly:
			return "<-chan " + elem
		case types.SendOnly:
			return "chan<- " + elem
		default:
			return "chan " + elem
		}
	}

	if named, ok := t.(*types.Named); ok {
		obj := named.Obj()
		pkg := obj.Pkg()

		// Handle builtin types with no package
		if pkg != nil && pkg.Name() != packageName {
			baseName := getSanitizedImport(pkg.Path()+PackageSuffix) + "." + getSanitizedImport(obj.Name())

			// Append type arguments for an instantiated cross-package generic type (e.g.
			// atomic.Pointer[Config]). The qualified-name form above omits them, whereas the
			// local fall-through path keeps them via t.String(); without this, a boxed value
			// of such a type emits `new sync.atomic_package.Pointer()` (missing <Config>).
			if typeArgs := named.TypeArgs(); typeArgs != nil && typeArgs.Len() > 0 {
				args := make([]string, typeArgs.Len())

				for i := 0; i < typeArgs.Len(); i++ {
					if proxyName, ok := v.constraintProxyArg(named, i); ok {
						args[i] = proxyName
					} else {
						args[i] = v.getFullTypeName(typeArgs.At(i), isUnderlying)
					}
				}

				return baseName + "[" + strings.Join(args, ", ") + "]"
			}

			return baseName
		}

		// A SAME-PACKAGE instantiated generic renders structurally too — each type ARGUMENT recursively
		// named — otherwise the t.String() fall-through below path-qualifies a cross-package argument and
		// the slash-strip eats the `Name[` header (crypto/elliptic's embedded `nistCurve[*nistec.P256Point]`
		// → `nistec.P256Point>`, a CS1519 cascade). The current package is elided, so the bare name stands.
		if typeArgs := named.TypeArgs(); typeArgs != nil && typeArgs.Len() > 0 {
			args := make([]string, typeArgs.Len())

			for i := 0; i < typeArgs.Len(); i++ {
				if proxyName, ok := v.constraintProxyArg(named, i); ok {
					args[i] = proxyName
				} else {
					args[i] = v.getFullTypeName(typeArgs.At(i), isUnderlying)
				}
			}

			return obj.Name() + "[" + strings.Join(args, ", ") + "]"
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

	// Remove the current package's path prefix from the type name (ReplaceAll so a composite
	// type naming two current-package types doesn't keep a self-qualified one — see getTypeName).
	typeName = strings.ReplaceAll(typeName, packagePathPrefix, "")

	// Skip the cross-package last-segment strip for composite/func type strings whose slashes are
	// internal (e.g. `func(go2cs/x/sub.Record)`); they are converted structurally downstream.
	if !strings.HasPrefix(typeName, "func") && !strings.Contains(typeName, "(") {
		slashIndex := strings.LastIndex(typeName, "/")

		if slashIndex != -1 {
			typeName = typeName[slashIndex+1:]
		}
	}

	return typeName
}

// collectCrossPackagePaths gathers the import paths of every cross-package named type referenced
// (recursively) by t — through pointers, arrays/slices, maps, channels and generic type arguments.
// Used by getDisplayTypeName to decide whether the file-local package aliases for those packages
// are all in scope.
func (v *Visitor) collectCrossPackagePaths(t types.Type, paths HashSet[string]) {
	switch tt := t.(type) {
	case *types.Pointer:
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Array:
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Slice:
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Chan:
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Map:
		v.collectCrossPackagePaths(tt.Key(), paths)
		v.collectCrossPackagePaths(tt.Elem(), paths)
	case *types.Named:
		if pkg := tt.Obj().Pkg(); pkg != nil && pkg != v.pkg {
			paths.Add(pkg.Path())
		}

		if typeArgs := tt.TypeArgs(); typeArgs != nil {
			for i := 0; i < typeArgs.Len(); i++ {
				v.collectCrossPackagePaths(typeArgs.At(i), paths)
			}
		}
	}
}

// getDisplayTypeName resolves a type name for emission into the CURRENT source file's body, preferring
// the readable file-local package alias (`atomic.Int32`, via getTypeName) over the fully-qualified
// form (`sync.atomic_package.Int32`, via getFullTypeName) — but ONLY when every cross-package type it
// references is imported in this file, so the alias is guaranteed in scope. When a referenced package
// is not imported here (e.g. a file indexing an atomic-typed array field without ever naming the
// element type → no `using atomic`), it falls back to the fully-qualified form, which resolves inside
// `namespace go;` without an alias. This keeps the converted C# visually close to the Go source while
// staying compilable. NOT for GoType attribute strings or other generator-consumed strings, which live
// in alias-less generated files and must always use getFullTypeName.
func (v *Visitor) getDisplayTypeName(t types.Type, isUnderlying bool) string {
	// Foreign renamed types display as the recorded imported-type alias (see
	// foreignAliasedTypeName) - the display layer, so promoted-member naming is untouched.
	if aliased, ok := v.foreignAliasedTypeName(t); ok {
		return aliased
	}

	paths := HashSet[string]{}
	v.collectCrossPackagePaths(t, paths)

	for _, path := range paths.Keys() {
		if !v.importQueue.Contains(path) {
			return v.getFullTypeName(t, isUnderlying)
		}
	}

	return v.getTypeName(t, isUnderlying)
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

	// The file-local using for this qualifier may be COLLISION-RENAMED (`using Δio =
	// io_package;` — `io` collides with the go.io CHILD NAMESPACE once io/fs is in the
	// reference closure): rewrite the qualifier so the type reference binds the renamed
	// alias (mime's `CharsetReader Func<@string, io.Reader, …>` field, CS0234 ×6).
	if qualifier, rest, found := strings.Cut(typeName, "."); found {
		if renamed, ok := packageImportAliasRenames[qualifier]; ok {
			// No recursion: a foreign type WITH its own rename already hit the alias map
			// above under the raw key; this qualifier rewrite is the final form.
			return renamed + "." + rest
		}
	}

	// A Δ-renamed IMPORT qualifier (gif's `color` import arrives shadow-renamed `Δcolor`)
	// must consult the alias map by the RAW package name — the global-using alias
	// (`colorꓸRGBA = go.image.color_package.ΔRGBA`) resolves namespace-wide regardless of
	// the file-local rename; the composed `Δcolor.RGBA` missed the map and kept the RAW
	// foreign type name, which is Δ-renamed in image/color (CS0426 ×4, image/gif). A type
	// the map does NOT rename (Δcolor.Palette) keeps the renamed-import qualifier.
	if strings.Contains(typeName, ".") {
		parts := strings.SplitN(typeName, ".", 2)

		if raw, wasShadow := strings.CutPrefix(parts[0], ShadowVarMarker); wasShadow {
			if resolved := getAliasedTypeName(raw + "." + parts[1]); resolved != raw+"."+parts[1] {
				return resolved
			}
		}
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
	// Render a func type structurally as an Action/Func delegate. The string-based path mangles
	// func types whose parameter/result types carry slash-bearing package paths (the slash-strip in
	// getTypeName chops `func(*math/rand.Rand)` to `*math/rand.Rand)`), and emits Go field order for
	// a named multi-result tuple. iifeDelegateType builds it from the signature using getCSTypeName
	// per element (correct qualification; nameless tuple results). Simple func types render
	// identically to the old path, so this is zero-churn for them.
	// Only an ANONYMOUS func type (t itself is the signature) is expanded; a NAMED func type
	// (`type Stringy func() string`, a *types.Named) keeps its delegate name via the normal path.
	if sig, ok := t.(*types.Signature); ok {
		return v.iifeDelegateType(sig)
	}

	if aliased, ok := v.foreignAliasedTypeName(t); ok {
		return aliased
	}

	return convertToCSTypeName(v.getTypeName(t, false))
}

// foreignAliasedTypeName resolves a cross-package type that is RENAMED (or Go-aliased) inside
// its own package — syscall declares `ΔHandle` for its type-vs-method-colliding `Handle` — to
// the recorded imported-type alias (`syscallꓸHandle` = `go.syscall_package.ΔHandle`): the raw
// qualified render (`Δsyscall.Handle`) names a type that does not exist (CS0426 ×21,
// internal/poll's signatures, fields, conversion targets, and local declarations). This lives
// at the C#-NAME layers ONLY (getCSTypeName / getDisplayTypeName / conversion targets), never
// in getTypeName: the Go-shaped name also feeds promoted-embed MEMBER naming, where the alias
// substitution renamed and rescoped the generated accessors (reflect CS8799 ×3 regression on
// the first cut). A type without a registered alias, and every generic instantiation, keeps
// the plain render (no churn).
func (v *Visitor) foreignAliasedTypeName(t types.Type) (string, bool) {
	named, ok := types.Unalias(t).(*types.Named)

	if !ok || (named.TypeArgs() != nil && named.TypeArgs().Len() > 0) {
		return "", false
	}

	pkg := named.Obj().Pkg()

	if pkg == nil || pkg == v.pkg {
		return "", false
	}

	plainKey := fmt.Sprintf("%s.%s", getSanitizedIdentifier(pkg.Name()), getCoreSanitizedIdentifier(named.Obj().Name()))

	packageLock.Lock()
	_, aliasExists := importedTypeAliases[plainKey]
	packageLock.Unlock()

	if !aliasExists {
		return "", false
	}

	return getAliasedTypeName(plainKey), true
}

// namedFuncTypeNameForSignature returns the C# delegate name of a package-level named func type whose
// underlying signature is identical to sig, or "" if none exists. Go's `:=` on a bare function value
// (`state := lexText`, where `lexText` returns the named func type `stateFn`) infers the variable's
// type as the UNNAMED signature `func(*lexer) stateFn`, not the named `stateFn` — so naming the local
// structurally emits a `Func<…>` delegate that is a DISTINCT C# type from the `stateFn` delegate the
// function group actually produces and that later `state = state(l)` assignments yield (CS0029, no
// implicit conversion between two delegate types). Recovering the named delegate lets the local take
// the single interconvertible delegate type (the classic self-referential state-machine func type).
func (v *Visitor) namedFuncTypeNameForSignature(sig *types.Signature) string {
	if sig == nil || v.pkg == nil {
		return ""
	}

	// A generic signature (type params or receiver) is never a plain named func type match.
	if sig.TypeParams() != nil || sig.RecvTypeParams() != nil || sig.Recv() != nil {
		return ""
	}

	scope := v.pkg.Scope()

	if scope == nil {
		return ""
	}

	for _, name := range scope.Names() {
		obj := scope.Lookup(name)

		typeName, ok := obj.(*types.TypeName)

		if !ok {
			continue
		}

		named, ok := typeName.Type().(*types.Named)

		if !ok || named.TypeParams() != nil {
			continue
		}

		underlyingSig, ok := named.Underlying().(*types.Signature)

		if !ok {
			continue
		}

		if types.Identical(underlyingSig, sig) {
			return getSanitizedIdentifier(named.Obj().Name())
		}
	}

	return ""
}

// exprIsMethodGroup reports whether expr is a bare reference to a function or method value (a C#
// method group), i.e. an identifier or selector whose resolved object is a *types.Func and which is
// not itself the call in a call expression. Such a value has no inferable delegate type under `var`.
func (v *Visitor) exprIsMethodGroup(expr ast.Expr) bool {
	var ident *ast.Ident

	switch e := expr.(type) {
	case *ast.Ident:
		ident = e
	case *ast.SelectorExpr:
		ident = e.Sel
	default:
		return false
	}

	if ident == nil {
		return false
	}

	_, isFunc := v.info.ObjectOf(ident).(*types.Func)

	return isFunc
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
		// A package-qualified TYPE string carries a subpackage PATH plus a trailing `.TypeName` —
		// `io/fs.DirEntry`, `internal/abi.Type` (a func-type param rendered from t.String(), whose
		// import alias was lost). Converting the WHOLE thing as one import path drops the package CLASS
		// suffix and dots the type straight into the namespace (`io.fs.DirEntry`, CS0234 — `fs` is not a
		// namespace of `go.io`; the type lives in class `fs_package`). Split the trailing type off: the
		// package path ends at the first `.` AFTER the last path `/`, so `io/fs` → `io.fs_package` and
		// the `.DirEntry` (plus any `[…]` generic args) re-appends. Falls back to the whole-path form
		// when there is no trailing type (a bare import path).
		genericStart := strings.IndexByte(typeName, '[')
		scanEnd := len(typeName)

		if genericStart != -1 {
			scanEnd = genericStart
		}

		lastSlash := strings.LastIndex(typeName[:scanEnd], "/")
		dotAfterSlash := -1

		if lastSlash != -1 {
			dotAfterSlash = strings.IndexByte(typeName[lastSlash:scanEnd], '.')
		}

		if dotAfterSlash != -1 {
			splitAt := lastSlash + dotAfterSlash
			pkgPath := typeName[:splitAt]

			// Some callers hand a path whose last segment ALREADY carries the class suffix
			// (`sync/atomic_package.Uint32`, from a recorded `[GoType]` underlying); others hand the
			// raw path (`io/fs.DirEntry`, from a signature's t.String()). Only append the suffix when
			// it is not already present, or it doubles (`atomic_package_package`).
			suffix := PackageSuffix

			if strings.HasSuffix(pkgPath[lastSlash+1:], PackageSuffix) {
				suffix = ""
			}

			typeName = convertImportPathToNamespace(pkgPath, suffix) + typeName[splitAt:]
		} else {
			typeName = convertImportPathToNamespace(typeName, "")
		}
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
	if start := strings.Index(typeName, "<"); start != -1 {
		// Locate the matching closing '>' by bracket depth rather than the first '>' — the latter
		// mis-handles nested generics (e.g. Pointer<node<K, V>> would stop at the inner '>',
		// extract the unbalanced "node<K, V", and recurse into "node<K", slicing out of range).
		depth := 0
		end := -1

		for i := start; i < len(typeName); i++ {
			if typeName[i] == '<' {
				depth++
			} else if typeName[i] == '>' {
				depth--

				if depth == 0 {
					end = i
					break
				}
			}
		}

		// Only split when a matching '>' exists; otherwise the '<' is not a generic bracket (e.g.
		// the '<-' of a directional channel inside a func type) and is handled by a later branch.
		if end != -1 {
			subTypes := splitTopLevelTypes(typeName[start+1 : end])

			for i := range subTypes {
				// Trim BEFORE converting: splitTopLevelTypes keeps the ", " separator's space, so
				// later args arrive as " string". The conversion switch matches the type name
				// exactly (`case "string"`), so a leading space would miss it and fall through to
				// the default named-type path — emitting C# `string` (System.String) instead of
				// golib `@string`. That violates the generic `new()` constraint the converter adds
				// (CS0310) and breaks string-literal assignment (CS0029).
				subTypes[i] = convertToCSTypeName(strings.TrimSpace(subTypes[i]))
			}

			base := typeName[:start]

			// The type-vs-method collision Δ-rename keys on the BARE type name; a generic
			// instantiation reaches the default sanitize with its `<args>` attached
			// (`indirect<K, V>`), missing the map — internal/concurrent's `type indirect[K, V]`
			// vs `func (n *node[K, V]) indirect()` renamed the DECLARATION `Δindirect<K, V>`
			// while every use kept the raw name (CS0246 ×33, leaking into net/netip). Rename
			// the bare base at reassembly; a dotted (package-qualified) base never matches the
			// per-package map and keeps its exported-alias route.
			if nameCollisions[base] {
				base = getCollisionAvoidanceIdentifier(base)
			}

			typeName = fmt.Sprintf("%s<%s>%s", base, strings.Join(subTypes, ", "), typeName[end+1:])
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

		// extractTypes already renders each parameter in C# form (a NAMED param has its type
		// converted after the name is stripped; the bare-type case is converted in place), so use
		// its output directly. Re-running convertToCSTypeName here DOUBLE-converts an already-C#
		// param — an emitted `map<@string, ж<Object>>` re-fed through the `map<` arm's
		// splitMapKeyValue mis-parses to `map<@string, ж<Object>, >` (spurious trailing empty type
		// arg → CS1031), as in go/ast's `type Importer func(imports map[string]*Object, …)`.
		csTypeNames := paramTypes

		// Check for return type after the closing parenthesis
		remainingType := strings.TrimSpace(typeName[closingParenIndex+1:])

		if len(remainingType) > 0 {
			// Has explicit return type. A PARENTHESIZED result list may carry Go NAMED
			// results (`(importPath string, ok bool)` — go/doc/comment's LookupPackage
			// func field): split the elements, strip the Go-ordered names, and rebuild —
			// ONE result unwraps to its bare type (a C# 1-tuple is CS8124), several yield
			// the C#-ordered named tuple `(@string importPath, bool ok)`.
			csReturnType := convertToCSResultList(remainingType)

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
	case "struct{}":
		return RootNamespace + ".EmptyStruct"
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
			// A channel ARROW survives the bracket replace — the '<' of `chan<-` /
			// `<-chan` (immediately followed by '-') is not a bracket; counting it
			// unbalanced the walk so the key/value boundary was never found
			// (os/signal's `map[chan<- os.Signal]*handler`, CS1003 syntax cascade ×8).
			if i+1 < len(typeStr) && typeStr[i+1] == '-' {
				continue
			}

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

// splitTopLevelTypes splits a comma-separated list of generic type arguments at the top bracket
// level only, so commas nested inside an inner generic (e.g. the comma in `node<K, V>` within
// `Pointer<node<K, V>>`) are not treated as argument separators.
func splitTopLevelTypes(typeArgs string) []string {
	var result []string

	depth := 0
	start := 0

	for i := 0; i < len(typeArgs); i++ {
		switch typeArgs[i] {
		case '<', '(', '[':
			depth++
		case '>', ')', ']':
			depth--
		case ',':
			// Only split on a comma at the outermost level. Besides nested generics (`<...>`), a
			// type arg can itself be a func type whose parameter list carries commas — e.g.
			// `Pointer[func(name, msg string)]`; track paren/bracket depth too so that inner comma
			// is not mistaken for an argument separator (which would shred the func type).
			if depth == 0 {
				result = append(result, typeArgs[start:i])
				start = i + 1
			}
		}
	}

	return append(result, typeArgs[start:])
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

		// If no space found, the entire param is a type (e.g., "string") — convert it in place so
		// this function ALWAYS returns C#-form types (the named branch below already does), letting
		// the sole caller trust the output without a second convertToCSTypeName pass (which would
		// double-convert an already-C# named param — see convertToCSFullTypeName's func-handler).
		if typeStart == 0 {
			types = append(types, convertToCSTypeName(param))
		} else {
			// Extract everything after the space
			paramType := convertToCSTypeName(strings.TrimSpace(param[typeStart:]))
			types = append(types, paramType)
		}
	}

	return types
}

// convertToCSResultList converts a Go func-type RESULT segment — a single bare type or a
// parenthesized (possibly NAMED) result list — to its C# rendering. ONE result unwraps to
// its bare type (a C# 1-tuple is CS8124); several yield the C#-ordered named tuple
// (`(@string importPath, bool ok)`). Go result lists are all-named or all-unnamed; a leading
// token is a NAME only when it is a plain identifier that is not a type-leading keyword
// (`chan int` stays a type).
func convertToCSResultList(resultType string) string {
	if !strings.HasPrefix(resultType, "(") || !strings.HasSuffix(resultType, ")") {
		return convertToCSTypeName(resultType)
	}

	inner := resultType[1 : len(resultType)-1]

	// Depth-aware split on top-level commas (nested func/map/generic types carry their own).
	var elements []string
	depth := 0
	start := 0

	for i, ch := range inner {
		switch ch {
		case '(', '[', '{':
			depth++
		case ')', ']', '}':
			depth--
		case ',':
			if depth == 0 {
				elements = append(elements, strings.TrimSpace(inner[start:i]))
				start = i + 1
			}
		}
	}

	elements = append(elements, strings.TrimSpace(inner[start:]))

	isPlainIdentifier := func(s string) bool {
		for i, ch := range s {
			if !(unicode.IsLetter(ch) || ch == '_' || (i > 0 && unicode.IsDigit(ch))) {
				return false
			}
		}

		return len(s) > 0
	}

	typeLeadingKeywords := map[string]bool{"chan": true, "func": true, "map": true, "struct": true, "interface": true}

	names := make([]string, len(elements))
	allNamed := true

	for i, element := range elements {
		spaceIndex := strings.IndexFunc(element, unicode.IsSpace)

		if spaceIndex <= 0 {
			allNamed = false
			break
		}

		name := element[:spaceIndex]

		if !isPlainIdentifier(name) || typeLeadingKeywords[name] {
			allNamed = false
			break
		}

		names[i] = name
	}

	if len(elements) == 1 {
		if allNamed {
			return convertToCSTypeName(strings.TrimSpace(elements[0][len(names[0]):]))
		}

		return convertToCSTypeName(elements[0])
	}

	parts := make([]string, len(elements))

	for i, element := range elements {
		if allNamed {
			elemType := convertToCSTypeName(strings.TrimSpace(element[len(names[i]):]))

			// A BLANK Go result name (`func match(x, y Value) (_, _ Value)`, go/constant) must
			// NOT become a C# tuple element name — two `_` elements collide (CS8127). Emit the
			// type only; C# allows a mixed named/unnamed tuple, so real names are kept.
			if names[i] == "_" {
				parts[i] = elemType
			} else {
				parts[i] = elemType + " " + getSanitizedIdentifier(names[i])
			}
		} else {
			parts[i] = convertToCSTypeName(element)
		}
	}

	return "(" + strings.Join(parts, ", ") + ")"
}

// identHasHeapBox reports whether the local behind obj is backed by a `Ꮡname` heap box.
// An escaping VALUE-type local always boxes. An INHERENTLY heap-allocated local (pointer/
// slice/map/chan/interface/func — already a reference, and blanket-marked escaping by the
// escape analysis) normally needs no box; it boxes only when its address is genuinely
// taken — by a capturing closure (a box-ref var: the closure writes through `&name` must
// reach the outer storage) or ANYWHERE in the current function (`zeroArray(&typ)` with
// `typ Type` — the `Ꮡ(typ)` copy-box fallback silently loses the callee's write through
// the pointer; dwarf zeroArray / InterfaceCasting replaceAnimal).
func (v *Visitor) identHasHeapBox(obj types.Object, identType types.Type) bool {
	if !v.identEscapesHeap[obj] {
		return false
	}

	if !isInherentlyHeapAllocatedType(identType) {
		return true
	}

	// An inherently-heap type (named slice/map/chan) is already a reference, so it is boxed only
	// when its address is genuinely needed: `&ident` taken, captured by-box in a closure, OR a
	// capture-mode pointer-receiver method is called on it (`frontier.Push(…)` with Push on
	// `*orderEventList`), which needs the ж overload's receiver box (CS1929 without it).
	return v.isLambdaBoxRefVar(obj) || v.identAddressTaken(obj) || packageCaptureModeBoxIdents[obj]
}

// identAddressTaken reports whether `&ident` occurs for obj anywhere in the current
// function (including nested function literals). Memoized per object.
func (v *Visitor) identAddressTaken(obj types.Object) bool {
	if v.currentFuncDecl == nil || obj == nil {
		return false
	}

	if taken, found := v.identAddressTakenCache[obj]; found {
		return taken
	}

	taken := false

	ast.Inspect(v.currentFuncDecl, func(n ast.Node) bool {
		if taken {
			return false
		}

		if unaryExpr, ok := n.(*ast.UnaryExpr); ok && unaryExpr.Op == token.AND {
			if id, ok := unaryExpr.X.(*ast.Ident); ok && v.info.ObjectOf(id) == obj {
				taken = true
				return false
			}
		}

		return true
	})

	if v.identAddressTakenCache == nil {
		v.identAddressTakenCache = make(map[types.Object]bool)
	}

	v.identAddressTakenCache[obj] = taken
	return taken
}

func (v *Visitor) convertToHeapTypeDecl(ident *ast.Ident, createNew bool) string {
	identType := v.info.TypeOf(ident)

	// Check both Defs and Uses maps
	obj := v.info.Defs[ident]

	if obj == nil {
		obj = v.info.Uses[ident]
	}

	if obj != nil && !v.identHasHeapBox(obj, identType) {
		return ""
	}

	goTypeName := v.getDisplayTypeName(identType, false)
	csIDName := v.getIdentName(ident)

	// If identifier is discarded, return empty string
	if csIDName == "_" {
		return ""
	}

	// The local's name is sanitized (a C# keyword such as `base`/`as`/`event` becomes `@base`…),
	// matching how it is referenced elsewhere. The box keeps the raw name with the Ꮡ prefix
	// (`Ꮡbase` is already a valid identifier and is how its address is emitted everywhere).
	varName := getSanitizedIdentifier(csIDName)

	// Handle array types. A SLICE (`[]T` — empty length) is NOT an array: it must fall
	// through to the generic path (`heap<slice<T>>`), or the boxed ref-local's type
	// mismatches every use (a `[]nint` local boxed as `heap<array<nint>>`, CS0029).
	if arrayLen := strings.Split(strings.TrimPrefix(goTypeName, "["), "]")[0]; strings.HasPrefix(goTypeName, "[") && arrayLen != "" {

		// Get array element type
		arrayType := convertToCSTypeName(goTypeName[strings.Index(goTypeName, "]")+1:])

		if v.options.preferVarDecl {
			if createNew {
				return fmt.Sprintf("ref var %s = ref heap(new array<%s>(%s), out var %s%s);", varName, arrayType, arrayLen, AddressPrefix, csIDName)
			}

			return fmt.Sprintf("ref var %s = ref heap<array<%s>>(out var %s%s);", varName, arrayType, AddressPrefix, csIDName)
		}

		if createNew {
			return fmt.Sprintf("ref array<%s> %s = ref heap(new array<%s>(%s), out %s<array<%s>> %s%s);", arrayType, varName, arrayType, arrayLen, PointerPrefix, arrayType, AddressPrefix, csIDName)
		}

		return fmt.Sprintf("ref array<%s> %s = ref heap<array<%s>>(out %s%s);", arrayType, varName, arrayType, AddressPrefix, csIDName)
	}

	csTypeName := convertToCSTypeName(goTypeName)

	// An inherently heap-allocated type (interface/pointer/slice/map/chan/func) takes the
	// parameterless box form: `new Animal()` is invalid for an interface (CS0144), and the
	// reference-like zero value is exactly what `heap<T>(out …)` provides.
	if isInherentlyHeapAllocatedType(identType) {
		createNew = false
	}

	if v.options.preferVarDecl {
		if createNew {
			return fmt.Sprintf("ref var %s = ref heap(new %s(), out var %s%s);", varName, csTypeName, AddressPrefix, csIDName)
		}

		return fmt.Sprintf("ref var %s = ref heap<%s>(out var %s%s);", varName, csTypeName, AddressPrefix, csIDName)
	}

	if createNew {
		return fmt.Sprintf("ref %s %s = ref heap(out %s<%s> %s%s);", csTypeName, varName, PointerPrefix, csTypeName, AddressPrefix, csIDName)
	}

	return fmt.Sprintf("ref %s %s = ref heap<%s>(out %s%s);", csTypeName, varName, csTypeName, AddressPrefix, csIDName)
}

// isBoxedPointerLocal reports whether ident is a box-ref LOCAL of an inherently heap-allocated type
// (pointer/slice/map/chan/interface/func) — exactly the case convertToHeapTypeDecl heap-boxes as a
// `ж<ж<T>>` because its address is taken inside a capturing closure. For such a box, `Ꮡm.Value` reads the
// HELD reference value (which may legitimately be nil), so emission must use `.ValueSlot` (no nil-deref
// panic) rather than the strict `.Value`. A deref'd pointer PARAMETER is excluded: its box wraps the
// pointed-to value, so `Ꮡp.Value` is a genuine dereference that must keep the strict nil check.
func (v *Visitor) isBoxedPointerLocal(ident *ast.Ident) bool {
	obj := v.info.ObjectOf(ident)

	if obj == nil || !v.isLambdaBoxRefVar(obj) {
		return false
	}

	// A deref'd pointer PARAMETER or RECEIVER is excluded: its box `Ꮡp` wraps the pointed-to value
	// (a `ж<T>`), so `Ꮡp.Value` is a genuine dereference that must keep the strict nil check. Only a
	// pointer/slice/map/... LOCAL gets a box that wraps the pointer value itself (a `ж<ж<T>>`), where
	// `.Value` is a non-dereferencing read of the held value. (identIsParameter misses the receiver,
	// which is not in the parameter list — varIsDerefdPointerParam covers both.)
	if v.varIsDerefdPointerParam(obj) {
		return false
	}

	return isInherentlyHeapAllocatedType(v.getIdentType(ident))
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
			// The map is keyed by NAME. Apply the capture name only when this ident resolves to the exact
			// captured OUTER variable — a same-named variable declared inside the lambda (an `s := f(s)`
			// self-shadow, where the inner `s` shadows the captured outer `s`) is a distinct binding and
			// must keep its own name (mapping it to the capture name emits `var sʗ3 = …(~sʗ3)…`, the inner
			// decl's RHS binding to itself → CS0841). A nil/untracked captured object keeps prior behavior.
			capturedObj, tracked := v.lambdaCapture.currentLambdaVarObjs[ident.Name]

			if !tracked || capturedObj == nil || v.info.ObjectOf(ident) == capturedObj {
				return captureName
			}
		}

		// Then check if it needs to be captured
		if captureInfo, ok := v.lambdaCapture.capturedVars[ident]; ok {
			captureInfo.used = true

			// Store the mapping for this lambda
			v.lambdaCapture.currentLambdaVars[ident.Name] = captureInfo.copyIdent.Name
			v.lambdaCapture.currentLambdaVarObjs[ident.Name] = v.info.ObjectOf(ident)

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

// implicitConvStructTypeName renders the C# name a GoImplicitConv attribute can carry for a
// struct-underlying type: a NAMED type's converted name, or the lifted dynamic-type name for a
// package-level anonymous struct. A lifted name whose declaring file has not been visited yet
// records the DEFERRED MARKER, resolved after the file-visit barrier when the package_info
// lines are emitted (raw Go `struct{…}` text is never attribute-safe C#).
func (v *Visitor) implicitConvStructTypeName(t types.Type) string {
	if named, ok := types.Unalias(t).(*types.Named); ok {
		return v.getCSTypeName(named)
	}

	// This visitor's lifted name is type-identity-keyed — precise even when two anonymous
	// structs share a structural signature (Process_data vs main_data); the shared registry
	// and the deferred marker are the cross-file fallbacks.
	if name, ok := v.liftedTypeMap[t]; ok {
		return name
	}

	signature := t.String()

	if name := lookupDynamicTypeName(signature); name != "" {
		return name
	}

	return dynamicTypeMarkerPrefix + signature + dynamicTypeMarkerSuffix
}

// resolveImplicitConvTypeName resolves a possibly-deferred implicit-conversion type name after
// the file-visit barrier (the dynamic-type registry is complete). Returns ok=false when the
// marker cannot resolve — a genuinely unlifted anonymous struct has no attribute-safe name and
// its record is dropped.
func resolveImplicitConvTypeName(name string) (string, bool) {
	if strings.HasPrefix(name, dynamicTypeMarkerPrefix) && strings.HasSuffix(name, dynamicTypeMarkerSuffix) {
		signature := name[len(dynamicTypeMarkerPrefix) : len(name)-len(dynamicTypeMarkerSuffix)]

		if resolved := lookupDynamicTypeName(signature); resolved != "" {
			return resolved, true
		}

		return "", false
	}

	return name, true
}
