// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The unitchecker package defines the main function for an analysis
// driver that analyzes a single compilation unit during a build.
// It is invoked by a build system such as "go vet":
//
//   $ go vet -vettool=$(which vet)
//
// It supports the following command-line protocol:
//
//      -V=full         describe executable               (to the build tool)
//      -flags          describe flags                    (to the build tool)
//      foo.cfg         description of compilation unit (from the build tool)
//
// This package does not depend on go/packages.
// If you need a standalone tool, use multichecker,
// which supports this mode but can also load packages
// from source using go/packages.
// package unitchecker -- go2cs converted at 2020 October 09 06:04:53 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/unitchecker" ==> using unitchecker = go.cmd.vendor.golang.org.x.tools.go.analysis.unitchecker_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\unitchecker\unitchecker.go
// TODO(adonovan):
// - with gccgo, go build does not build standard library,
//   so we will not get to analyze it. Yet we must in order
//   to create base facts for, say, the fmt package for the
//   printf checker.

using gob = go.encoding.gob_package;
using json = go.encoding.json_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using importer = go.go.importer_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using analysisflags = go.golang.org.x.tools.go.analysis.@internal.analysisflags_package;
using facts = go.golang.org.x.tools.go.analysis.@internal.facts_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis
{
    public static partial class unitchecker_package
    {
        // A Config describes a compilation unit to be analyzed.
        // It is provided to the tool in a JSON-encoded file
        // whose name ends with ".cfg".
        public partial struct Config
        {
            public @string ID; // e.g. "fmt [fmt.test]"
            public @string Compiler;
            public @string Dir;
            public @string ImportPath;
            public slice<@string> GoFiles;
            public slice<@string> NonGoFiles;
            public map<@string, @string> ImportMap;
            public map<@string, @string> PackageFile;
            public map<@string, bool> Standard;
            public map<@string, @string> PackageVetx;
            public bool VetxOnly;
            public @string VetxOutput;
            public bool SucceedOnTypecheckFailure;
        }

        // Main is the main function of a vet-like analysis tool that must be
        // invoked by a build system to analyze a single package.
        //
        // The protocol required by 'go vet -vettool=...' is that the tool must support:
        //
        //      -flags          describe flags in JSON
        //      -V=full         describe executable for build caching
        //      foo.cfg         perform separate modular analyze on the single
        //                      unit described by a JSON config file foo.cfg.
        //
        public static void Main(params ptr<ptr<analysis.Analyzer>>[] _addr_analyzers)
        {
            analyzers = analyzers.Clone();
            ref analysis.Analyzer analyzers = ref _addr_analyzers.val;

            var progname = filepath.Base(os.Args[0L]);
            log.SetFlags(0L);
            log.SetPrefix(progname + ": ");

            {
                var err = analysis.Validate(analyzers);

                if (err != null)
                {
                    log.Fatal(err);
                }

            }


            flag.Usage = () =>
            {
                fmt.Fprintf(os.Stderr, "%[1]s is a tool for static analysis of Go programs.\n\nUsage of %[1]s:\n\t%.16[1]s un" +
    "it.cfg\t# execute analysis specified by config file\n\t%.16[1]s help    \t# general " +
    "help\n\t%.16[1]s help name\t# help on specific analyzer and its flags\n", progname);
                os.Exit(1L);

            }
;

            analyzers = analysisflags.Parse(analyzers, true);

            var args = flag.Args();
            if (len(args) == 0L)
            {
                flag.Usage();
            }

            if (args[0L] == "help")
            {
                analysisflags.Help(progname, analyzers, args[1L..]);
                os.Exit(0L);
            }

            if (len(args) != 1L || !strings.HasSuffix(args[0L], ".cfg"))
            {
                log.Fatalf("invoking \"go tool vet\" directly is unsupported; use \"go vet\"");
            }

            Run(args[0L], analyzers);

        }

        // Run reads the *.cfg file, runs the analysis,
        // and calls os.Exit with an appropriate error code.
        // It assumes flags have already been set.
        public static void Run(@string configFile, slice<ptr<analysis.Analyzer>> analyzers)
        {
            var (cfg, err) = readConfig(configFile);
            if (err != null)
            {
                log.Fatal(err);
            }

            var fset = token.NewFileSet();
            var (results, err) = run(_addr_fset, _addr_cfg, analyzers);
            if (err != null)
            {
                log.Fatal(err);
            } 

            // In VetxOnly mode, the analysis is run only for facts.
            if (!cfg.VetxOnly)
            {
                if (analysisflags.JSON)
                { 
                    // JSON output
                    var tree = make(analysisflags.JSONTree);
                    {
                        var res__prev1 = res;

                        foreach (var (_, __res) in results)
                        {
                            res = __res;
                            tree.Add(fset, cfg.ID, res.a.Name, res.diagnostics, res.err);
                        }
                else

                        res = res__prev1;
                    }

                    tree.Print();

                }                { 
                    // plain text
                    long exit = 0L;
                    {
                        var res__prev1 = res;

                        foreach (var (_, __res) in results)
                        {
                            res = __res;
                            if (res.err != null)
                            {
                                log.Println(res.err);
                                exit = 1L;
                            }

                        }

                        res = res__prev1;
                    }

                    {
                        var res__prev1 = res;

                        foreach (var (_, __res) in results)
                        {
                            res = __res;
                            foreach (var (_, diag) in res.diagnostics)
                            {
                                analysisflags.PrintPlain(fset, diag);
                                exit = 1L;
                            }

                        }

                        res = res__prev1;
                    }

                    os.Exit(exit);

                }

            }

            os.Exit(0L);

        }

        private static (ptr<Config>, error) readConfig(@string filename)
        {
            ptr<Config> _p0 = default!;
            error _p0 = default!;

            var (data, err) = ioutil.ReadFile(filename);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            ptr<Config> cfg = @new<Config>();
            {
                var err = json.Unmarshal(data, cfg);

                if (err != null)
                {
                    return (_addr_null!, error.As(fmt.Errorf("cannot decode JSON config file %s: %v", filename, err))!);
                }

            }

            if (len(cfg.GoFiles) == 0L)
            { 
                // The go command disallows packages with no files.
                // The only exception is unsafe, but the go command
                // doesn't call vet on it.
                return (_addr_null!, error.As(fmt.Errorf("package has no files: %s", cfg.ImportPath))!);

            }

            return (_addr_cfg!, error.As(null!)!);

        }

        private static Func<ptr<token.FileSet>, @string, importer.Lookup, types.Importer> importerForCompiler = (_, compiler, lookup) =>
        { 
            // broken legacy implementation (https://golang.org/issue/28995)
            return importer.For(compiler, lookup);

        };

        private static (slice<result>, error) run(ptr<token.FileSet> _addr_fset, ptr<Config> _addr_cfg, slice<ptr<analysis.Analyzer>> analyzers)
        {
            slice<result> _p0 = default;
            error _p0 = default!;
            ref token.FileSet fset = ref _addr_fset.val;
            ref Config cfg = ref _addr_cfg.val;
 
            // Load, parse, typecheck.
            slice<ptr<ast.File>> files = default;
            foreach (var (_, name) in cfg.GoFiles)
            {
                var (f, err) = parser.ParseFile(fset, name, null, parser.ParseComments);
                if (err != null)
                {
                    if (cfg.SucceedOnTypecheckFailure)
                    { 
                        // Silently succeed; let the compiler
                        // report parse errors.
                        err = null;

                    }

                    return (null, error.As(err)!);

                }

                files = append(files, f);

            }
            var compilerImporter = importerForCompiler(fset, cfg.Compiler, path =>
            { 
                // path is a resolved package path, not an import path.
                var (file, ok) = cfg.PackageFile[path];
                if (!ok)
                {
                    if (cfg.Compiler == "gccgo" && cfg.Standard[path])
                    {
                        return (null, error.As(null!)!); // fall back to default gccgo lookup
                    }

                    return (null, error.As(fmt.Errorf("no package file for %q", path))!);

                }

                return os.Open(file);

            });
            var importer = importerFunc(importPath =>
            {
                var (path, ok) = cfg.ImportMap[importPath]; // resolve vendoring, etc
                if (!ok)
                {
                    return (null, error.As(fmt.Errorf("can't resolve import %q", path))!);
                }

                return compilerImporter.Import(path);

            });
            ptr<types.Config> tc = addr(new types.Config(Importer:importer,Sizes:types.SizesFor("gc",build.Default.GOARCH),));
            ptr<types.Info> info = addr(new types.Info(Types:make(map[ast.Expr]types.TypeAndValue),Defs:make(map[*ast.Ident]types.Object),Uses:make(map[*ast.Ident]types.Object),Implicits:make(map[ast.Node]types.Object),Scopes:make(map[ast.Node]*types.Scope),Selections:make(map[*ast.SelectorExpr]*types.Selection),));
            var (pkg, err) = tc.Check(cfg.ImportPath, fset, files, info);
            if (err != null)
            {
                if (cfg.SucceedOnTypecheckFailure)
                { 
                    // Silently succeed; let the compiler
                    // report type errors.
                    err = null;

                }

                return (null, error.As(err)!);

            } 

            // Register fact types with gob.
            // In VetxOnly mode, analyzers are only for their facts,
            // so we can skip any analysis that neither produces facts
            // nor depends on any analysis that produces facts.
            // Also build a map to hold working state and result.
            private partial struct action
            {
                public sync.Once once;
                public error err;
                public bool usesFacts; // (transitively uses)
                public slice<analysis.Diagnostic> diagnostics;
            }
            var actions = make_map<ptr<analysis.Analyzer>, ptr<action>>();
            Func<ptr<analysis.Analyzer>, bool> registerFacts = default;
            registerFacts = a =>
            {
                var (act, ok) = actions[a];
                if (!ok)
                {
                    act = @new<action>();
                    bool usesFacts = default;
                    {
                        var f__prev1 = f;

                        foreach (var (_, __f) in a.FactTypes)
                        {
                            f = __f;
                            usesFacts = true;
                            gob.Register(f);
                        }

                        f = f__prev1;
                    }

                    {
                        var req__prev1 = req;

                        foreach (var (_, __req) in a.Requires)
                        {
                            req = __req;
                            if (registerFacts(req))
                            {
                                usesFacts = true;
                            }

                        }

                        req = req__prev1;
                    }

                    act.usesFacts = usesFacts;
                    actions[a] = act;

                }

                return act.usesFacts;

            }
;
            slice<ptr<analysis.Analyzer>> filtered = default;
            {
                var a__prev1 = a;

                foreach (var (_, __a) in analyzers)
                {
                    a = __a;
                    if (registerFacts(a) || !cfg.VetxOnly)
                    {
                        filtered = append(filtered, a);
                    }

                }

                a = a__prev1;
            }

            analyzers = filtered; 

            // Read facts from imported packages.
            Func<@string, (slice<byte>, error)> read = path =>
            {
                {
                    var (vetx, ok) = cfg.PackageVetx[path];

                    if (ok)
                    {
                        return ioutil.ReadFile(vetx);
                    }

                }

                return (null, error.As(null!)!); // no .vetx file, no facts
            }
;
            var (facts, err) = facts.Decode(pkg, read);
            if (err != null)
            {
                return (null, error.As(err)!);
            } 

            // In parallel, execute the DAG of analyzers.
            Func<ptr<analysis.Analyzer>, ptr<action>> exec = default;
            Action<slice<ptr<analysis.Analyzer>>> execAll = default;
            exec = a =>
            {
                var act = actions[a];
                act.once.Do(() =>
                {
                    execAll(a.Requires); // prefetch dependencies in parallel

                    // The inputs to this analysis are the
                    // results of its prerequisites.
                    var inputs = make();
                    slice<@string> failed = default;
                    {
                        var req__prev1 = req;

                        foreach (var (_, __req) in a.Requires)
                        {
                            req = __req;
                            var reqact = exec(req);
                            if (reqact.err != null)
                            {
                                failed = append(failed, req.String());
                                continue;
                            }

                            inputs[req] = reqact.result;

                        } 

                        // Report an error if any dependency failed.

                        req = req__prev1;
                    }

                    if (failed != null)
                    {
                        sort.Strings(failed);
                        act.err = fmt.Errorf("failed prerequisites: %s", strings.Join(failed, ", "));
                        return ;
                    }

                    var factFilter = make_map<reflect.Type, bool>();
                    {
                        var f__prev1 = f;

                        foreach (var (_, __f) in a.FactTypes)
                        {
                            f = __f;
                            factFilter[reflect.TypeOf(f)] = true;
                        }

                        f = f__prev1;
                    }

                    ptr<analysis.Pass> pass = addr(new analysis.Pass(Analyzer:a,Fset:fset,Files:files,OtherFiles:cfg.NonGoFiles,Pkg:pkg,TypesInfo:info,TypesSizes:tc.Sizes,ResultOf:inputs,Report:func(danalysis.Diagnostic){act.diagnostics=append(act.diagnostics,d)},ImportObjectFact:facts.ImportObjectFact,ExportObjectFact:facts.ExportObjectFact,AllObjectFacts:func()[]analysis.ObjectFact{returnfacts.AllObjectFacts(factFilter)},ImportPackageFact:facts.ImportPackageFact,ExportPackageFact:facts.ExportPackageFact,AllPackageFacts:func()[]analysis.PackageFact{returnfacts.AllPackageFacts(factFilter)},));

                    var t0 = time.Now();
                    act.result, act.err = a.Run(pass);
                    if (false)
                    {
                        log.Printf("analysis %s = %s", pass, time.Since(t0));
                    }

                });
                return act;

            }
;
            execAll = analyzers =>
            {
                sync.WaitGroup wg = default;
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in analyzers)
                    {
                        a = __a;
                        wg.Add(1L);
                        go_(() => a =>
                        {
                            _ = exec(a);
                            wg.Done();
                        }(a));

                    }

                    a = a__prev1;
                }

                wg.Wait();

            }
;

            execAll(analyzers); 

            // Return diagnostics and errors from root analyzers.
            var results = make_slice<result>(len(analyzers));
            {
                var a__prev1 = a;

                foreach (var (__i, __a) in analyzers)
                {
                    i = __i;
                    a = __a;
                    act = actions[a];
                    results[i].a = a;
                    results[i].err = act.err;
                    results[i].diagnostics = act.diagnostics;
                }

                a = a__prev1;
            }

            var data = facts.Encode();
            {
                var err = ioutil.WriteFile(cfg.VetxOutput, data, 0666L);

                if (err != null)
                {
                    return (null, error.As(fmt.Errorf("failed to write analysis facts: %v", err))!);
                }

            }


            return (results, error.As(null!)!);

        }

        private partial struct result
        {
            public ptr<analysis.Analyzer> a;
            public slice<analysis.Diagnostic> diagnostics;
            public error err;
        }

        public delegate  error) importerFunc(@string,  (ptr<types.Package>);

        private static (ptr<types.Package>, error) Import(this importerFunc f, @string path)
        {
            ptr<types.Package> _p0 = default!;
            error _p0 = default!;

            return _addr_f(path)!;
        }
    }
}}}}}}}}
