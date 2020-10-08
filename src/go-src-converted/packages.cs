// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package packages -- go2cs converted at 2020 October 08 04:55:12 UTC
// import "golang.org/x/tools/go/packages" ==> using packages = go.golang.org.x.tools.go.packages_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\packages.go
// See doc.go for package documentation and implementation notes.

using context = go.context_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using types = go.go.types_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using gcexportdata = go.golang.org.x.tools.go.gcexportdata_package;
using gocommand = go.golang.org.x.tools.@internal.gocommand_package;
using packagesinternal = go.golang.org.x.tools.@internal.packagesinternal_package;
using typesinternal = go.golang.org.x.tools.@internal.typesinternal_package;
using static go.builtin;
using System;
using System.ComponentModel;
using System.Threading;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class packages_package
    {
        // A LoadMode controls the amount of detail to return when loading.
        // The bits below can be combined to specify which fields should be
        // filled in the result packages.
        // The zero value is a special case, equivalent to combining
        // the NeedName, NeedFiles, and NeedCompiledGoFiles bits.
        // ID and Errors (if present) will always be filled.
        // Load may return more information than requested.
        public partial struct LoadMode // : long
        {
        }

        // TODO(matloob): When a V2 of go/packages is released, rename NeedExportsFile to
        // NeedExportFile to make it consistent with the Package field it's adding.

 
        // NeedName adds Name and PkgPath.
        public static readonly LoadMode NeedName = (LoadMode)1L << (int)(iota); 

        // NeedFiles adds GoFiles and OtherFiles.
        public static readonly var NeedFiles = (var)0; 

        // NeedCompiledGoFiles adds CompiledGoFiles.
        public static readonly var NeedCompiledGoFiles = (var)1; 

        // NeedImports adds Imports. If NeedDeps is not set, the Imports field will contain
        // "placeholder" Packages with only the ID set.
        public static readonly var NeedImports = (var)2; 

        // NeedDeps adds the fields requested by the LoadMode in the packages in Imports.
        public static readonly var NeedDeps = (var)3; 

        // NeedExportsFile adds ExportFile.
        public static readonly var NeedExportsFile = (var)4; 

        // NeedTypes adds Types, Fset, and IllTyped.
        public static readonly var NeedTypes = (var)5; 

        // NeedSyntax adds Syntax.
        public static readonly var NeedSyntax = (var)6; 

        // NeedTypesInfo adds TypesInfo.
        public static readonly var NeedTypesInfo = (var)7; 

        // NeedTypesSizes adds TypesSizes.
        public static readonly var NeedTypesSizes = (var)8; 

        // typecheckCgo enables full support for type checking cgo. Requires Go 1.15+.
        // Modifies CompiledGoFiles and Types, and has no effect on its own.
        private static readonly var typecheckCgo = (var)9; 

        // NeedModule adds Module.
        public static readonly var NeedModule = (var)10;


 
        // Deprecated: LoadFiles exists for historical compatibility
        // and should not be used. Please directly specify the needed fields using the Need values.
        public static readonly var LoadFiles = (var)NeedName | NeedFiles | NeedCompiledGoFiles; 

        // Deprecated: LoadImports exists for historical compatibility
        // and should not be used. Please directly specify the needed fields using the Need values.
        public static readonly var LoadImports = (var)LoadFiles | NeedImports; 

        // Deprecated: LoadTypes exists for historical compatibility
        // and should not be used. Please directly specify the needed fields using the Need values.
        public static readonly var LoadTypes = (var)LoadImports | NeedTypes | NeedTypesSizes; 

        // Deprecated: LoadSyntax exists for historical compatibility
        // and should not be used. Please directly specify the needed fields using the Need values.
        public static readonly var LoadSyntax = (var)LoadTypes | NeedSyntax | NeedTypesInfo; 

        // Deprecated: LoadAllSyntax exists for historical compatibility
        // and should not be used. Please directly specify the needed fields using the Need values.
        public static readonly var LoadAllSyntax = (var)LoadSyntax | NeedDeps;


        // A Config specifies details about how packages should be loaded.
        // The zero value is a valid configuration.
        // Calls to Load do not modify this struct.
        public partial struct Config
        {
            public LoadMode Mode; // Context specifies the context for the load operation.
// If the context is cancelled, the loader may stop early
// and return an ErrCancelled error.
// If Context is nil, the load cannot be cancelled.
            public context.Context Context; // Logf is the logger for the config.
// If the user provides a logger, debug logging is enabled.
// If the GOPACKAGESDEBUG environment variable is set to true,
// but the logger is nil, default to log.Printf.
            public Action<@string, object[]> Logf; // Dir is the directory in which to run the build system's query tool
// that provides information about the packages.
// If Dir is empty, the tool is run in the current directory.
            public @string Dir; // Env is the environment to use when invoking the build system's query tool.
// If Env is nil, the current environment is used.
// As in os/exec's Cmd, only the last value in the slice for
// each environment key is used. To specify the setting of only
// a few variables, append to the current environment, as in:
//
//    opt.Env = append(os.Environ(), "GOOS=plan9", "GOARCH=386")
//
            public slice<@string> Env; // gocmdRunner guards go command calls from concurrency errors.
            public ptr<gocommand.Runner> gocmdRunner; // BuildFlags is a list of command-line flags to be passed through to
// the build system's query tool.
            public slice<@string> BuildFlags; // Fset provides source position information for syntax trees and types.
// If Fset is nil, Load will use a new fileset, but preserve Fset's value.
            public ptr<token.FileSet> Fset; // ParseFile is called to read and parse each file
// when preparing a package's type-checked syntax tree.
// It must be safe to call ParseFile simultaneously from multiple goroutines.
// If ParseFile is nil, the loader will uses parser.ParseFile.
//
// ParseFile should parse the source from src and use filename only for
// recording position information.
//
// An application may supply a custom implementation of ParseFile
// to change the effective file contents or the behavior of the parser,
// or to modify the syntax tree. For example, selectively eliminating
// unwanted function bodies can significantly accelerate type checking.
            public Func<ptr<token.FileSet>, @string, slice<byte>, (ptr<ast.File>, error)> ParseFile; // If Tests is set, the loader includes not just the packages
// matching a particular pattern but also any related test packages,
// including test-only variants of the package and the test executable.
//
// For example, when using the go command, loading "fmt" with Tests=true
// returns four packages, with IDs "fmt" (the standard package),
// "fmt [fmt.test]" (the package as compiled for the test),
// "fmt_test" (the test functions from source files in package fmt_test),
// and "fmt.test" (the test binary).
//
// In build systems with explicit names for tests,
// setting Tests may have no effect.
            public bool Tests; // Overlay provides a mapping of absolute file paths to file contents.
// If the file with the given path already exists, the parser will use the
// alternative file contents provided by the map.
//
// Overlays provide incomplete support for when a given file doesn't
// already exist on disk. See the package doc above for more details.
            public map<@string, slice<byte>> Overlay;
        }

        // driver is the type for functions that query the build system for the
        // packages named by the patterns.
        public delegate  error) driver(ptr<Config>,  @string[],  (ptr<driverResponse>);

        // driverResponse contains the results for a driver query.
        private partial struct driverResponse
        {
            public bool NotHandled; // Sizes, if not nil, is the types.Sizes to use when type checking.
            public ptr<types.StdSizes> Sizes; // Roots is the set of package IDs that make up the root packages.
// We have to encode this separately because when we encode a single package
// we cannot know if it is one of the roots as that requires knowledge of the
// graph it is part of.
            [Description("json:\",omitempty\"")]
            public slice<@string> Roots; // Packages is the full set of packages in the graph.
// The packages are not connected into a graph.
// The Imports if populated will be stubs that only have their ID set.
// Imports will be connected and then type and syntax information added in a
// later pass (see refine).
            public slice<ptr<Package>> Packages;
        }

        // Load loads and returns the Go packages named by the given patterns.
        //
        // Config specifies loading options;
        // nil behaves the same as an empty Config.
        //
        // Load returns an error if any of the patterns was invalid
        // as defined by the underlying build system.
        // It may return an empty list of packages without an error,
        // for instance for an empty expansion of a valid wildcard.
        // Errors associated with a particular package are recorded in the
        // corresponding Package's Errors list, and do not cause Load to
        // return an error. Clients may need to handle such errors before
        // proceeding with further analysis. The PrintErrors function is
        // provided for convenient display of all errors.
        public static (slice<ptr<Package>>, error) Load(ptr<Config> _addr_cfg, params @string[] patterns)
        {
            slice<ptr<Package>> _p0 = default;
            error _p0 = default!;
            patterns = patterns.Clone();
            ref Config cfg = ref _addr_cfg.val;

            var l = newLoader(_addr_cfg);
            var (response, err) = defaultDriver(_addr_l.Config, patterns);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            l.sizes = response.Sizes;
            return l.refine(response.Roots, response.Packages);

        }

        // defaultDriver is a driver that implements go/packages' fallback behavior.
        // It will try to request to an external driver, if one exists. If there's
        // no external driver, or the driver returns a response with NotHandled set,
        // defaultDriver will fall back to the go list driver.
        private static (ptr<driverResponse>, error) defaultDriver(ptr<Config> _addr_cfg, params @string[] patterns)
        {
            ptr<driverResponse> _p0 = default!;
            error _p0 = default!;
            patterns = patterns.Clone();
            ref Config cfg = ref _addr_cfg.val;

            var driver = findExternalDriver(cfg);
            if (driver == null)
            {
                driver = goListDriver;
            }

            var (response, err) = driver(cfg, patterns);
            if (err != null)
            {
                return (_addr_response!, error.As(err)!);
            }
            else if (response.NotHandled)
            {
                return _addr_goListDriver(cfg, patterns)!;
            }

            return (_addr_response!, error.As(null!)!);

        }

        // A Package describes a loaded Go package.
        public partial struct Package
        {
            public @string ID; // Name is the package name as it appears in the package source code.
            public @string Name; // PkgPath is the package path as used by the go/types package.
            public @string PkgPath; // Errors contains any errors encountered querying the metadata
// of the package, or while parsing or type-checking its files.
            public slice<Error> Errors; // GoFiles lists the absolute file paths of the package's Go source files.
            public slice<@string> GoFiles; // CompiledGoFiles lists the absolute file paths of the package's source
// files that are suitable for type checking.
// This may differ from GoFiles if files are processed before compilation.
            public slice<@string> CompiledGoFiles; // OtherFiles lists the absolute file paths of the package's non-Go source files,
// including assembly, C, C++, Fortran, Objective-C, SWIG, and so on.
            public slice<@string> OtherFiles; // ExportFile is the absolute path to a file containing type
// information for the package as provided by the build system.
            public @string ExportFile; // Imports maps import paths appearing in the package's Go source files
// to corresponding loaded Packages.
            public map<@string, ptr<Package>> Imports; // Types provides type information for the package.
// The NeedTypes LoadMode bit sets this field for packages matching the
// patterns; type information for dependencies may be missing or incomplete,
// unless NeedDeps and NeedImports are also set.
            public ptr<types.Package> Types; // Fset provides position information for Types, TypesInfo, and Syntax.
// It is set only when Types is set.
            public ptr<token.FileSet> Fset; // IllTyped indicates whether the package or any dependency contains errors.
// It is set only when Types is set.
            public bool IllTyped; // Syntax is the package's syntax trees, for the files listed in CompiledGoFiles.
//
// The NeedSyntax LoadMode bit populates this field for packages matching the patterns.
// If NeedDeps and NeedImports are also set, this field will also be populated
// for dependencies.
            public slice<ptr<ast.File>> Syntax; // TypesInfo provides type information about the package's syntax trees.
// It is set only when Syntax is set.
            public ptr<types.Info> TypesInfo; // TypesSizes provides the effective size function for types in TypesInfo.
            public types.Sizes TypesSizes; // forTest is the package under test, if any.
            public @string forTest; // module is the module information for the package if it exists.
            public ptr<Module> Module;
        }

        // Module provides module information for a package.
        public partial struct Module
        {
            public @string Path; // module path
            public @string Version; // module version
            public ptr<Module> Replace; // replaced by this module
            public ptr<time.Time> Time; // time version was created
            public bool Main; // is this the main module?
            public bool Indirect; // is this module only an indirect dependency of main module?
            public @string Dir; // directory holding files for this module, if any
            public @string GoMod; // path to go.mod file used when loading this module, if any
            public @string GoVersion; // go version used in module
            public ptr<ModuleError> Error; // error loading module
        }

        // ModuleError holds errors loading a module.
        public partial struct ModuleError
        {
            public @string Err; // the error itself
        }

        private static void init()
        {
            packagesinternal.GetForTest = p =>
            {
                return p._<ptr<Package>>().forTest;
            }
;
            packagesinternal.GetGoCmdRunner = config =>
            {
                return config._<ptr<Config>>().gocmdRunner;
            }
;
            packagesinternal.SetGoCmdRunner = (config, runner) =>
            {
                config._<ptr<Config>>().gocmdRunner = runner;
            }
;
            packagesinternal.TypecheckCgo = int(typecheckCgo);

        }

        // An Error describes a problem with a package's metadata, syntax, or types.
        public partial struct Error
        {
            public @string Pos; // "file:line:col" or "file:line" or "" or "-"
            public @string Msg;
            public ErrorKind Kind;
        }

        // ErrorKind describes the source of the error, allowing the user to
        // differentiate between errors generated by the driver, the parser, or the
        // type-checker.
        public partial struct ErrorKind // : long
        {
        }

        public static readonly ErrorKind UnknownError = (ErrorKind)iota;
        public static readonly var ListError = (var)0;
        public static readonly var ParseError = (var)1;
        public static readonly var TypeError = (var)2;


        public static @string Error(this Error err)
        {
            var pos = err.Pos;
            if (pos == "")
            {
                pos = "-"; // like token.Position{}.String()
            }

            return pos + ": " + err.Msg;

        }

        // flatPackage is the JSON form of Package
        // It drops all the type and syntax fields, and transforms the Imports
        //
        // TODO(adonovan): identify this struct with Package, effectively
        // publishing the JSON protocol.
        private partial struct flatPackage
        {
            public @string ID;
            [Description("json:\",omitempty\"")]
            public @string Name;
            [Description("json:\",omitempty\"")]
            public @string PkgPath;
            [Description("json:\",omitempty\"")]
            public slice<Error> Errors;
            [Description("json:\",omitempty\"")]
            public slice<@string> GoFiles;
            [Description("json:\",omitempty\"")]
            public slice<@string> CompiledGoFiles;
            [Description("json:\",omitempty\"")]
            public slice<@string> OtherFiles;
            [Description("json:\",omitempty\"")]
            public @string ExportFile;
            [Description("json:\",omitempty\"")]
            public map<@string, @string> Imports;
        }

        // MarshalJSON returns the Package in its JSON form.
        // For the most part, the structure fields are written out unmodified, and
        // the type and syntax fields are skipped.
        // The imports are written out as just a map of path to package id.
        // The errors are written using a custom type that tries to preserve the
        // structure of error types we know about.
        //
        // This method exists to enable support for additional build systems.  It is
        // not intended for use by clients of the API and we may change the format.
        private static (slice<byte>, error) MarshalJSON(this ptr<Package> _addr_p)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref Package p = ref _addr_p.val;

            ptr<flatPackage> flat = addr(new flatPackage(ID:p.ID,Name:p.Name,PkgPath:p.PkgPath,Errors:p.Errors,GoFiles:p.GoFiles,CompiledGoFiles:p.CompiledGoFiles,OtherFiles:p.OtherFiles,ExportFile:p.ExportFile,));
            if (len(p.Imports) > 0L)
            {
                flat.Imports = make_map<@string, @string>(len(p.Imports));
                foreach (var (path, ipkg) in p.Imports)
                {
                    flat.Imports[path] = ipkg.ID;
                }

            }

            return json.Marshal(flat);

        }

        // UnmarshalJSON reads in a Package from its JSON format.
        // See MarshalJSON for details about the format accepted.
        private static error UnmarshalJSON(this ptr<Package> _addr_p, slice<byte> b)
        {
            ref Package p = ref _addr_p.val;

            ptr<flatPackage> flat = addr(new flatPackage());
            {
                var err = json.Unmarshal(b, _addr_flat);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            p.val = new Package(ID:flat.ID,Name:flat.Name,PkgPath:flat.PkgPath,Errors:flat.Errors,GoFiles:flat.GoFiles,CompiledGoFiles:flat.CompiledGoFiles,OtherFiles:flat.OtherFiles,ExportFile:flat.ExportFile,);
            if (len(flat.Imports) > 0L)
            {
                p.Imports = make_map<@string, ptr<Package>>(len(flat.Imports));
                foreach (var (path, id) in flat.Imports)
                {
                    p.Imports[path] = addr(new Package(ID:id));
                }

            }

            return error.As(null!)!;

        }

        private static @string String(this ptr<Package> _addr_p)
        {
            ref Package p = ref _addr_p.val;

            return p.ID;
        }

        // loaderPackage augments Package with state used during the loading phase
        private partial struct loaderPackage
        {
            public ref ptr<Package> ptr<Package> => ref ptr<Package>_ptr;
            public map<@string, error> importErrors; // maps each bad import to its error
            public sync.Once loadOnce;
            public byte color; // for cycle detection
            public bool needsrc; // load from source (Mode >= LoadTypes)
            public bool needtypes; // type information is either requested or depended on
            public bool initial; // package was matched by a pattern
        }

        // loader holds the working state of a single call to load.
        private partial struct loader
        {
            public map<@string, ptr<loaderPackage>> pkgs;
            public ref Config Config => ref Config_val;
            public types.Sizes sizes;
            public map<@string, ptr<parseValue>> parseCache;
            public sync.Mutex parseCacheMu;
            public sync.Mutex exportMu; // enforces mutual exclusion of exportdata operations

// Config.Mode contains the implied mode (see impliedLoadMode).
// Implied mode contains all the fields we need the data for.
// In requestedMode there are the actually requested fields.
// We'll zero them out before returning packages to the user.
// This makes it easier for us to get the conditions where
// we need certain modes right.
            public LoadMode requestedMode;
        }

        private partial struct parseValue
        {
            public ptr<ast.File> f;
            public error err;
            public channel<object> ready;
        }

        private static ptr<loader> newLoader(ptr<Config> _addr_cfg)
        {
            ref Config cfg = ref _addr_cfg.val;

            ptr<loader> ld = addr(new loader(parseCache:map[string]*parseValue{},));
            if (cfg != null)
            {
                ld.Config = cfg; 
                // If the user has provided a logger, use it.
                ld.Config.Logf = cfg.Logf;

            }

            if (ld.Config.Logf == null)
            { 
                // If the GOPACKAGESDEBUG environment variable is set to true,
                // but the user has not provided a logger, default to log.Printf.
                if (debug)
                {
                    ld.Config.Logf = log.Printf;
                }
                else
                {
                    ld.Config.Logf = (format, args) =>
                    {
                    }
;

                }

            }

            if (ld.Config.Mode == 0L)
            {
                ld.Config.Mode = NeedName | NeedFiles | NeedCompiledGoFiles; // Preserve zero behavior of Mode for backwards compatibility.
            }

            if (ld.Config.Env == null)
            {
                ld.Config.Env = os.Environ();
            }

            if (ld.Config.gocmdRunner == null)
            {
                ld.Config.gocmdRunner = addr(new gocommand.Runner());
            }

            if (ld.Context == null)
            {
                ld.Context = context.Background();
            }

            if (ld.Dir == "")
            {
                {
                    var (dir, err) = os.Getwd();

                    if (err == null)
                    {
                        ld.Dir = dir;
                    }

                }

            } 

            // Save the actually requested fields. We'll zero them out before returning packages to the user.
            ld.requestedMode = ld.Mode;
            ld.Mode = impliedLoadMode(ld.Mode);

            if (ld.Mode & NeedTypes != 0L || ld.Mode & NeedSyntax != 0L)
            {
                if (ld.Fset == null)
                {
                    ld.Fset = token.NewFileSet();
                } 

                // ParseFile is required even in LoadTypes mode
                // because we load source if export data is missing.
                if (ld.ParseFile == null)
                {
                    ld.ParseFile = (fset, filename, src) =>
                    {
                        const var mode = (var)parser.AllErrors | parser.ParseComments;

                        return _addr_parser.ParseFile(fset, filename, src, mode)!;
                    }
;

                }

            }

            return _addr_ld!;

        }

        // refine connects the supplied packages into a graph and then adds type and
        // and syntax information as requested by the LoadMode.
        private static (slice<ptr<Package>>, error) refine(this ptr<loader> _addr_ld, slice<@string> roots, params ptr<ptr<Package>>[] _addr_list) => func((_, panic, __) =>
        {
            slice<ptr<Package>> _p0 = default;
            error _p0 = default!;
            list = list.Clone();
            ref loader ld = ref _addr_ld.val;
            ref Package list = ref _addr_list.val;

            var rootMap = make_map<@string, long>(len(roots));
            {
                var i__prev1 = i;
                var root__prev1 = root;

                foreach (var (__i, __root) in roots)
                {
                    i = __i;
                    root = __root;
                    rootMap[root] = i;
                }

                i = i__prev1;
                root = root__prev1;
            }

            ld.pkgs = make_map<@string, ptr<loaderPackage>>(); 
            // first pass, fixup and build the map and roots
            var initial = make_slice<ptr<loaderPackage>>(len(roots));
            foreach (var (_, pkg) in list)
            {
                long rootIndex = -1L;
                {
                    var i__prev1 = i;

                    var (i, found) = rootMap[pkg.ID];

                    if (found)
                    {
                        rootIndex = i;
                    } 

                    // Overlays can invalidate export data.
                    // TODO(matloob): make this check fine-grained based on dependencies on overlaid files

                    i = i__prev1;

                } 

                // Overlays can invalidate export data.
                // TODO(matloob): make this check fine-grained based on dependencies on overlaid files
                var exportDataInvalid = len(ld.Overlay) > 0L || pkg.ExportFile == "" && pkg.PkgPath != "unsafe"; 
                // This package needs type information if the caller requested types and the package is
                // either a root, or it's a non-root and the user requested dependencies ...
                var needtypes = (ld.Mode & NeedTypes | NeedTypesInfo != 0L && (rootIndex >= 0L || ld.Mode & NeedDeps != 0L)); 
                // This package needs source if the call requested source (or types info, which implies source)
                // and the package is either a root, or itas a non- root and the user requested dependencies...
                var needsrc = ((ld.Mode & (NeedSyntax | NeedTypesInfo) != 0L && (rootIndex >= 0L || ld.Mode & NeedDeps != 0L)) || (ld.Mode & NeedTypes | NeedTypesInfo != 0L && exportDataInvalid)) && pkg.PkgPath != "unsafe";
                ptr<loaderPackage> lpkg = addr(new loaderPackage(Package:pkg,needtypes:needtypes,needsrc:needsrc,));
                ld.pkgs[lpkg.ID] = lpkg;
                if (rootIndex >= 0L)
                {
                    initial[rootIndex] = lpkg;
                    lpkg.initial = true;
                }

            }
            {
                var i__prev1 = i;
                var root__prev1 = root;

                foreach (var (__i, __root) in roots)
                {
                    i = __i;
                    root = __root;
                    if (initial[i] == null)
                    {
                        return (null, error.As(fmt.Errorf("root package %v is missing", root))!);
                    }

                } 

                // Materialize the import graph.

                i = i__prev1;
                root = root__prev1;
            }

            const long white = (long)0L; // new
            const long grey = (long)1L; // in progress
            const long black = (long)2L; // complete 

            // visit traverses the import graph, depth-first,
            // and materializes the graph as Packages.Imports.
            //
            // Valid imports are saved in the Packages.Import map.
            // Invalid imports (cycles and missing nodes) are saved in the importErrors map.
            // Thus, even in the presence of both kinds of errors, the Import graph remains a DAG.
            //
            // visit returns whether the package needs src or has a transitive
            // dependency on a package that does. These are the only packages
            // for which we load source code.
            slice<ptr<loaderPackage>> stack = default;
            Func<ptr<loaderPackage>, bool> visit = default;
            slice<ptr<loaderPackage>> srcPkgs = default;
            visit = lpkg =>
            {

                if (lpkg.color == black) 
                    return lpkg.needsrc;
                else if (lpkg.color == grey) 
                    panic("internal error: grey node");
                                lpkg.color = grey;
                stack = append(stack, lpkg); // push
                var stubs = lpkg.Imports; // the structure form has only stubs with the ID in the Imports
                // If NeedImports isn't set, the imports fields will all be zeroed out.
                if (ld.Mode & NeedImports != 0L)
                {
                    lpkg.Imports = make_map<@string, ptr<Package>>(len(stubs));
                    {
                        var ipkg__prev1 = ipkg;

                        foreach (var (__importPath, __ipkg) in stubs)
                        {
                            importPath = __importPath;
                            ipkg = __ipkg;
                            error importErr = default!;
                            var imp = ld.pkgs[ipkg.ID];
                            if (imp == null)
                            { 
                                // (includes package "C" when DisableCgo)
                                importErr = error.As(fmt.Errorf("missing package: %q", ipkg.ID))!;

                            }
                            else if (imp.color == grey)
                            {
                                importErr = error.As(fmt.Errorf("import cycle: %s", stack))!;
                            }

                            if (importErr != null)
                            {
                                if (lpkg.importErrors == null)
                                {
                                    lpkg.importErrors = make_map<@string, error>();
                                }

                                lpkg.importErrors[importPath] = importErr;
                                continue;

                            }

                            if (visit(imp))
                            {
                                lpkg.needsrc = true;
                            }

                            lpkg.Imports[importPath] = imp.Package;

                        }

                        ipkg = ipkg__prev1;
                    }
                }

                if (lpkg.needsrc)
                {
                    srcPkgs = append(srcPkgs, lpkg);
                }

                if (ld.Mode & NeedTypesSizes != 0L)
                {
                    lpkg.TypesSizes = ld.sizes;
                }

                stack = stack[..len(stack) - 1L]; // pop
                lpkg.color = black;

                return lpkg.needsrc;

            }
;

            if (ld.Mode & NeedImports == 0L)
            { 
                // We do this to drop the stub import packages that we are not even going to try to resolve.
                {
                    loaderPackage lpkg__prev1 = lpkg;

                    foreach (var (_, __lpkg) in initial)
                    {
                        lpkg = __lpkg;
                        lpkg.Imports = null;
                    }
            else

                    lpkg = lpkg__prev1;
                }
            }            { 
                // For each initial package, create its import DAG.
                {
                    loaderPackage lpkg__prev1 = lpkg;

                    foreach (var (_, __lpkg) in initial)
                    {
                        lpkg = __lpkg;
                        visit(lpkg);
                    }

                    lpkg = lpkg__prev1;
                }
            }

            if (ld.Mode & NeedImports != 0L && ld.Mode & NeedTypes != 0L)
            {
                {
                    loaderPackage lpkg__prev1 = lpkg;

                    foreach (var (_, __lpkg) in srcPkgs)
                    {
                        lpkg = __lpkg; 
                        // Complete type information is required for the
                        // immediate dependencies of each source package.
                        {
                            var ipkg__prev2 = ipkg;

                            foreach (var (_, __ipkg) in lpkg.Imports)
                            {
                                ipkg = __ipkg;
                                imp = ld.pkgs[ipkg.ID];
                                imp.needtypes = true;
                            }

                            ipkg = ipkg__prev2;
                        }
                    }

                    lpkg = lpkg__prev1;
                }
            } 
            // Load type data and syntax if needed, starting at
            // the initial packages (roots of the import DAG).
            if (ld.Mode & NeedTypes != 0L || ld.Mode & NeedSyntax != 0L)
            {
                sync.WaitGroup wg = default;
                {
                    loaderPackage lpkg__prev1 = lpkg;

                    foreach (var (_, __lpkg) in initial)
                    {
                        lpkg = __lpkg;
                        wg.Add(1L);
                        go_(() => lpkg =>
                        {
                            ld.loadRecursive(lpkg);
                            wg.Done();
                        }(lpkg));

                    }

                    lpkg = lpkg__prev1;
                }

                wg.Wait();

            }

            var result = make_slice<ptr<Package>>(len(initial));
            {
                var i__prev1 = i;
                loaderPackage lpkg__prev1 = lpkg;

                foreach (var (__i, __lpkg) in initial)
                {
                    i = __i;
                    lpkg = __lpkg;
                    result[i] = lpkg.Package;
                }

                i = i__prev1;
                lpkg = lpkg__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in ld.pkgs)
                {
                    i = __i; 
                    // Clear all unrequested fields, for extra de-Hyrum-ization.
                    if (ld.requestedMode & NeedName == 0L)
                    {
                        ld.pkgs[i].Name = "";
                        ld.pkgs[i].PkgPath = "";
                    }

                    if (ld.requestedMode & NeedFiles == 0L)
                    {
                        ld.pkgs[i].GoFiles = null;
                        ld.pkgs[i].OtherFiles = null;
                    }

                    if (ld.requestedMode & NeedCompiledGoFiles == 0L)
                    {
                        ld.pkgs[i].CompiledGoFiles = null;
                    }

                    if (ld.requestedMode & NeedImports == 0L)
                    {
                        ld.pkgs[i].Imports = null;
                    }

                    if (ld.requestedMode & NeedExportsFile == 0L)
                    {
                        ld.pkgs[i].ExportFile = "";
                    }

                    if (ld.requestedMode & NeedTypes == 0L)
                    {
                        ld.pkgs[i].Types = null;
                        ld.pkgs[i].Fset = null;
                        ld.pkgs[i].IllTyped = false;
                    }

                    if (ld.requestedMode & NeedSyntax == 0L)
                    {
                        ld.pkgs[i].Syntax = null;
                    }

                    if (ld.requestedMode & NeedTypesInfo == 0L)
                    {
                        ld.pkgs[i].TypesInfo = null;
                    }

                    if (ld.requestedMode & NeedTypesSizes == 0L)
                    {
                        ld.pkgs[i].TypesSizes = null;
                    }

                    if (ld.requestedMode & NeedModule == 0L)
                    {
                        ld.pkgs[i].Module = null;
                    }

                }

                i = i__prev1;
            }

            return (result, error.As(null!)!);

        });

        // loadRecursive loads the specified package and its dependencies,
        // recursively, in parallel, in topological order.
        // It is atomic and idempotent.
        // Precondition: ld.Mode&NeedTypes.
        private static void loadRecursive(this ptr<loader> _addr_ld, ptr<loaderPackage> _addr_lpkg)
        {
            ref loader ld = ref _addr_ld.val;
            ref loaderPackage lpkg = ref _addr_lpkg.val;

            lpkg.loadOnce.Do(() =>
            { 
                // Load the direct dependencies, in parallel.
                sync.WaitGroup wg = default;
                foreach (var (_, ipkg) in lpkg.Imports)
                {
                    var imp = ld.pkgs[ipkg.ID];
                    wg.Add(1L);
                    go_(() => imp =>
                    {
                        ld.loadRecursive(imp);
                        wg.Done();
                    }(imp));

                }
                wg.Wait();
                ld.loadPackage(lpkg);

            });

        }

        // loadPackage loads the specified package.
        // It must be called only once per Package,
        // after immediate dependencies are loaded.
        // Precondition: ld.Mode & NeedTypes.
        private static void loadPackage(this ptr<loader> _addr_ld, ptr<loaderPackage> _addr_lpkg) => func((_, panic, __) =>
        {
            ref loader ld = ref _addr_ld.val;
            ref loaderPackage lpkg = ref _addr_lpkg.val;

            if (lpkg.PkgPath == "unsafe")
            { 
                // Fill in the blanks to avoid surprises.
                lpkg.Types = types.Unsafe;
                lpkg.Fset = ld.Fset;
                lpkg.Syntax = new slice<ptr<ast.File>>(new ptr<ast.File>[] {  });
                lpkg.TypesInfo = @new<types.Info>();
                lpkg.TypesSizes = ld.sizes;
                return ;

            } 

            // Call NewPackage directly with explicit name.
            // This avoids skew between golist and go/types when the files'
            // package declarations are inconsistent.
            lpkg.Types = types.NewPackage(lpkg.PkgPath, lpkg.Name);
            lpkg.Fset = ld.Fset; 

            // Subtle: we populate all Types fields with an empty Package
            // before loading export data so that export data processing
            // never has to create a types.Package for an indirect dependency,
            // which would then require that such created packages be explicitly
            // inserted back into the Import graph as a final step after export data loading.
            // The Diamond test exercises this case.
            if (!lpkg.needtypes && !lpkg.needsrc)
            {
                return ;
            }

            if (!lpkg.needsrc)
            {
                ld.loadFromExportData(lpkg);
                return ; // not a source package, don't get syntax trees
            }

            Action<error> appendError = err =>
            { 
                // Convert various error types into the one true Error.
                slice<Error> errs = default;
                switch (err.type())
                {
                    case Error err:
                        errs = append(errs, err);
                        break;
                    case ptr<os.PathError> err:
                        errs = append(errs, new Error(Pos:err.Path+":1",Msg:err.Err.Error(),Kind:ParseError,));
                        break;
                    case scanner.ErrorList err:
                        {
                            var err__prev1 = err;

                            foreach (var (_, __err) in err)
                            {
                                err = __err;
                                errs = append(errs, new Error(Pos:err.Pos.String(),Msg:err.Msg,Kind:ParseError,));
                            }

                            err = err__prev1;
                        }
                        break;
                    case types.Error err:
                        errs = append(errs, new Error(Pos:err.Fset.Position(err.Pos).String(),Msg:err.Msg,Kind:TypeError,));
                        break;
                    default:
                    {
                        var err = err.type();
                        errs = append(errs, new Error(Pos:"-",Msg:err.Error(),Kind:UnknownError,)); 

                        // If you see this error message, please file a bug.
                        log.Printf("internal error: error %q (%T) without position", err, err);
                        break;
                    }

                }

                lpkg.Errors = append(lpkg.Errors, errs);

            }
;

            if (ld.Config.Mode & NeedTypes != 0L && len(lpkg.CompiledGoFiles) == 0L && lpkg.ExportFile != "")
            { 
                // The config requested loading sources and types, but sources are missing.
                // Add an error to the package and fall back to loading from export data.
                appendError(new Error("-",fmt.Sprintf("sources missing for package %s",lpkg.ID),ParseError));
                ld.loadFromExportData(lpkg);
                return ; // can't get syntax trees for this package
            }

            var (files, errs) = ld.parseFiles(lpkg.CompiledGoFiles);
            {
                var err__prev1 = err;

                foreach (var (_, __err) in errs)
                {
                    err = __err;
                    appendError(err);
                }

                err = err__prev1;
            }

            lpkg.Syntax = files;
            if (ld.Config.Mode & NeedTypes == 0L)
            {
                return ;
            }

            lpkg.TypesInfo = addr(new types.Info(Types:make(map[ast.Expr]types.TypeAndValue),Defs:make(map[*ast.Ident]types.Object),Uses:make(map[*ast.Ident]types.Object),Implicits:make(map[ast.Node]types.Object),Scopes:make(map[ast.Node]*types.Scope),Selections:make(map[*ast.SelectorExpr]*types.Selection),));
            lpkg.TypesSizes = ld.sizes;

            var importer = importerFunc(path =>
            {
                if (path == "unsafe")
                {
                    return (types.Unsafe, null);
                } 

                // The imports map is keyed by import path.
                var ipkg = lpkg.Imports[path];
                if (ipkg == null)
                {
                    {
                        var err__prev2 = err;

                        var err = lpkg.importErrors[path];

                        if (err != null)
                        {
                            return (null, err);
                        } 
                        // There was skew between the metadata and the
                        // import declarations, likely due to an edit
                        // race, or because the ParseFile feature was
                        // used to supply alternative file contents.

                        err = err__prev2;

                    } 
                    // There was skew between the metadata and the
                    // import declarations, likely due to an edit
                    // race, or because the ParseFile feature was
                    // used to supply alternative file contents.
                    return (null, fmt.Errorf("no metadata for %s", path));

                }

                if (ipkg.Types != null && ipkg.Types.Complete())
                {
                    return (ipkg.Types, null);
                }

                log.Fatalf("internal error: package %q without types was imported from %q", path, lpkg);
                panic("unreachable");

            }); 

            // type-check
            ptr<types.Config> tc = addr(new types.Config(Importer:importer,IgnoreFuncBodies:ld.Mode&NeedDeps==0&&!lpkg.initial,Error:appendError,Sizes:ld.sizes,));
            if ((ld.Mode & typecheckCgo) != 0L)
            {
                if (!typesinternal.SetUsesCgo(tc))
                {
                    appendError(new Error(Msg:"typecheckCgo requires Go 1.15+",Kind:ListError,));
                    return ;
                }

            }

            types.NewChecker(tc, ld.Fset, lpkg.Types, lpkg.TypesInfo).Files(lpkg.Syntax);

            lpkg.importErrors = null; // no longer needed

            // If !Cgo, the type-checker uses FakeImportC mode, so
            // it doesn't invoke the importer for import "C",
            // nor report an error for the import,
            // or for any undefined C.f reference.
            // We must detect this explicitly and correctly
            // mark the package as IllTyped (by reporting an error).
            // TODO(adonovan): if these errors are annoying,
            // we could just set IllTyped quietly.
            if (tc.FakeImportC)
            {
outer:
                foreach (var (_, f) in lpkg.Syntax)
                {
                    {
                        var imp__prev2 = imp;

                        foreach (var (_, __imp) in f.Imports)
                        {
                            imp = __imp;
                            if (imp.Path.Value == "\"C\"")
                            {
                                err = new types.Error(Fset:ld.Fset,Pos:imp.Pos(),Msg:`import "C" ignored`);
                                appendError(err);
                                _breakouter = true;
                                break;
                            }

                        }

                        imp = imp__prev2;
                    }
                }

            } 

            // Record accumulated errors.
            var illTyped = len(lpkg.Errors) > 0L;
            if (!illTyped)
            {
                {
                    var imp__prev1 = imp;

                    foreach (var (_, __imp) in lpkg.Imports)
                    {
                        imp = __imp;
                        if (imp.IllTyped)
                        {
                            illTyped = true;
                            break;
                        }

                    }

                    imp = imp__prev1;
                }
            }

            lpkg.IllTyped = illTyped;

        });

        // An importFunc is an implementation of the single-method
        // types.Importer interface based on a function value.
        public delegate  error) importerFunc(@string,  (ptr<types.Package>);

        private static (ptr<types.Package>, error) Import(this importerFunc f, @string path)
        {
            ptr<types.Package> _p0 = default!;
            error _p0 = default!;

            return _addr_f(path)!;
        }

        // We use a counting semaphore to limit
        // the number of parallel I/O calls per process.
        private static var ioLimit = make_channel<bool>(20L);

        private static (ptr<ast.File>, error) parseFile(this ptr<loader> _addr_ld, @string filename)
        {
            ptr<ast.File> _p0 = default!;
            error _p0 = default!;
            ref loader ld = ref _addr_ld.val;

            ld.parseCacheMu.Lock();
            var (v, ok) = ld.parseCache[filename];
            if (ok)
            { 
                // cache hit
                ld.parseCacheMu.Unlock().Send(v.ready);

            }
            else
            { 
                // cache miss
                v = addr(new parseValue(ready:make(chanstruct{})));
                ld.parseCache[filename] = v;
                ld.parseCacheMu.Unlock();

                slice<byte> src = default;
                foreach (var (f, contents) in ld.Config.Overlay)
                {
                    if (sameFile(f, filename))
                    {
                        src = contents;
                    }

                }
                error err = default!;
                if (src == null)
                {
                    ioLimit.Send(true); // wait
                    src, err = ioutil.ReadFile(filename);
                    ioLimit.Receive(); // signal
                }

                if (err != null)
                {
                    v.err = err;
                }
                else
                {
                    v.f, v.err = ld.ParseFile(ld.Fset, filename, src);
                }

                close(v.ready);

            }

            return (_addr_v.f!, error.As(v.err)!);

        }

        // parseFiles reads and parses the Go source files and returns the ASTs
        // of the ones that could be at least partially parsed, along with a
        // list of I/O and parse errors encountered.
        //
        // Because files are scanned in parallel, the token.Pos
        // positions of the resulting ast.Files are not ordered.
        //
        private static (slice<ptr<ast.File>>, slice<error>) parseFiles(this ptr<loader> _addr_ld, slice<@string> filenames)
        {
            slice<ptr<ast.File>> _p0 = default;
            slice<error> _p0 = default;
            ref loader ld = ref _addr_ld.val;

            sync.WaitGroup wg = default;
            var n = len(filenames);
            var parsed = make_slice<ptr<ast.File>>(n);
            var errors = make_slice<error>(n);
            foreach (var (i, file) in filenames)
            {
                if (ld.Config.Context.Err() != null)
                {
                    parsed[i] = null;
                    errors[i] = ld.Config.Context.Err();
                    continue;
                }

                wg.Add(1L);
                go_(() => (i, filename) =>
                {
                    parsed[i], errors[i] = ld.parseFile(filename);
                    wg.Done();
                }(i, file));

            }
            wg.Wait(); 

            // Eliminate nils, preserving order.
            long o = default;
            foreach (var (_, f) in parsed)
            {
                if (f != null)
                {
                    parsed[o] = f;
                    o++;
                }

            }
            parsed = parsed[..o];

            o = 0L;
            foreach (var (_, err) in errors)
            {
                if (err != null)
                {
                    errors[o] = err;
                    o++;
                }

            }
            errors = errors[..o];

            return (parsed, errors);

        }

        // sameFile returns true if x and y have the same basename and denote
        // the same file.
        //
        private static bool sameFile(@string x, @string y)
        {
            if (x == y)
            { 
                // It could be the case that y doesn't exist.
                // For instance, it may be an overlay file that
                // hasn't been written to disk. To handle that case
                // let x == y through. (We added the exact absolute path
                // string to the CompiledGoFiles list, so the unwritten
                // overlay case implies x==y.)
                return true;

            }

            if (strings.EqualFold(filepath.Base(x), filepath.Base(y)))
            { // (optimisation)
                {
                    var (xi, err) = os.Stat(x);

                    if (err == null)
                    {
                        {
                            var (yi, err) = os.Stat(y);

                            if (err == null)
                            {
                                return os.SameFile(xi, yi);
                            }

                        }

                    }

                }

            }

            return false;

        }

        // loadFromExportData returns type information for the specified
        // package, loading it from an export data file on the first request.
        private static (ptr<types.Package>, error) loadFromExportData(this ptr<loader> _addr_ld, ptr<loaderPackage> _addr_lpkg) => func((defer, _, __) =>
        {
            ptr<types.Package> _p0 = default!;
            error _p0 = default!;
            ref loader ld = ref _addr_ld.val;
            ref loaderPackage lpkg = ref _addr_lpkg.val;

            if (lpkg.PkgPath == "")
            {
                log.Fatalf("internal error: Package %s has no PkgPath", lpkg);
            } 

            // Because gcexportdata.Read has the potential to create or
            // modify the types.Package for each node in the transitive
            // closure of dependencies of lpkg, all exportdata operations
            // must be sequential. (Finer-grained locking would require
            // changes to the gcexportdata API.)
            //
            // The exportMu lock guards the Package.Pkg field and the
            // types.Package it points to, for each Package in the graph.
            //
            // Not all accesses to Package.Pkg need to be protected by exportMu:
            // graph ordering ensures that direct dependencies of source
            // packages are fully loaded before the importer reads their Pkg field.
            ld.exportMu.Lock();
            defer(ld.exportMu.Unlock());

            {
                var tpkg__prev1 = tpkg;

                var tpkg = lpkg.Types;

                if (tpkg != null && tpkg.Complete())
                {
                    return (_addr_tpkg!, error.As(null!)!); // cache hit
                }

                tpkg = tpkg__prev1;

            }


            lpkg.IllTyped = true; // fail safe

            if (lpkg.ExportFile == "")
            { 
                // Errors while building export data will have been printed to stderr.
                return (_addr_null!, error.As(fmt.Errorf("no export data file"))!);

            }

            var (f, err) = os.Open(lpkg.ExportFile);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            defer(f.Close()); 

            // Read gc export data.
            //
            // We don't currently support gccgo export data because all
            // underlying workspaces use the gc toolchain. (Even build
            // systems that support gccgo don't use it for workspace
            // queries.)
            var (r, err) = gcexportdata.NewReader(f);
            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("reading %s: %v", lpkg.ExportFile, err))!);
            } 

            // Build the view.
            //
            // The gcexportdata machinery has no concept of package ID.
            // It identifies packages by their PkgPath, which although not
            // globally unique is unique within the scope of one invocation
            // of the linker, type-checker, or gcexportdata.
            //
            // So, we must build a PkgPath-keyed view of the global
            // (conceptually ID-keyed) cache of packages and pass it to
            // gcexportdata. The view must contain every existing
            // package that might possibly be mentioned by the
            // current package---its transitive closure.
            //
            // In loadPackage, we unconditionally create a types.Package for
            // each dependency so that export data loading does not
            // create new ones.
            //
            // TODO(adonovan): it would be simpler and more efficient
            // if the export data machinery invoked a callback to
            // get-or-create a package instead of a map.
            //
            var view = make_map<@string, ptr<types.Package>>(); // view seen by gcexportdata
            var seen = make_map<ptr<loaderPackage>, bool>(); // all visited packages
            Action<map<@string, ptr<Package>>> visit = default;
            visit = pkgs =>
            {
                foreach (var (_, p) in pkgs)
                {
                    var lpkg = ld.pkgs[p.ID];
                    if (!seen[lpkg])
                    {
                        seen[lpkg] = true;
                        view[lpkg.PkgPath] = lpkg.Types;
                        visit(lpkg.Imports);
                    }

                }

            }
;
            visit(lpkg.Imports);

            var viewLen = len(view) + 1L; // adding the self package
            // Parse the export data.
            // (May modify incomplete packages in view but not create new ones.)
            var (tpkg, err) = gcexportdata.Read(r, ld.Fset, view, lpkg.PkgPath);
            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("reading %s: %v", lpkg.ExportFile, err))!);
            }

            if (viewLen != len(view))
            {
                log.Fatalf("Unexpected package creation during export data loading");
            }

            lpkg.Types = tpkg;
            lpkg.IllTyped = false;

            return (_addr_tpkg!, error.As(null!)!);

        });

        // impliedLoadMode returns loadMode with its dependencies.
        private static LoadMode impliedLoadMode(LoadMode loadMode)
        {
            if (loadMode & NeedTypesInfo != 0L && loadMode & NeedImports == 0L)
            { 
                // If NeedTypesInfo, go/packages needs to do typechecking itself so it can
                // associate type info with the AST. To do so, we need the export data
                // for dependencies, which means we need to ask for the direct dependencies.
                // NeedImports is used to ask for the direct dependencies.
                loadMode |= NeedImports;

            }

            if (loadMode & NeedDeps != 0L && loadMode & NeedImports == 0L)
            { 
                // With NeedDeps we need to load at least direct dependencies.
                // NeedImports is used to ask for the direct dependencies.
                loadMode |= NeedImports;

            }

            return loadMode;

        }

        private static bool usesExportData(ptr<Config> _addr_cfg)
        {
            ref Config cfg = ref _addr_cfg.val;

            return cfg.Mode & NeedExportsFile != 0L || cfg.Mode & NeedTypes != 0L && cfg.Mode & NeedDeps == 0L;
        }
    }
}}}}}
