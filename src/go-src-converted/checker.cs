// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package checker defines the implementation of the checker commands.
// The same code drives the multi-analysis driver, the single-analysis
// driver that is conventionally provided for convenience along with
// each analysis package, and the test driver.
// package checker -- go2cs converted at 2022 March 06 23:31:16 UTC
// import "golang.org/x/tools/go/analysis/internal/checker" ==> using checker = go.golang.org.x.tools.go.analysis.@internal.checker_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\internal\checker\checker.go
using bytes = go.bytes_package;
using gob = go.encoding.gob_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using format = go.go.format_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using types = go.go.types_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using pprof = go.runtime.pprof_package;
using trace = go.runtime.trace_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using analysisflags = go.golang.org.x.tools.go.analysis.@internal.analysisflags_package;
using packages = go.golang.org.x.tools.go.packages_package;
using analysisinternal = go.golang.org.x.tools.@internal.analysisinternal_package;
using span = go.golang.org.x.tools.@internal.span_package;
using System;
using System.Threading;


namespace go.golang.org.x.tools.go.analysis.@internal;

public static partial class checker_package {

 
// Debug is a set of single-letter flags:
//
//    f    show [f]acts as they are created
//     p    disable [p]arallel execution of analyzers
//    s    do additional [s]anity checks on fact types and serialization
//    t    show [t]iming info (NB: use 'p' flag to avoid GC/scheduler noise)
//    v    show [v]erbose logging
//
public static @string Debug = "";public static @string CPUProfile = default;public static @string MemProfile = default;public static @string Trace = default; 

// Fix determines whether to apply all suggested fixes.
public static bool Fix = default;

// RegisterFlags registers command-line flags used by the analysis driver.
public static void RegisterFlags() { 
    // When adding flags here, remember to update
    // the list of suppressed flags in analysisflags.

    flag.StringVar(_addr_Debug, "debug", Debug, "debug flags, any subset of \"fpstv\"");

    flag.StringVar(_addr_CPUProfile, "cpuprofile", "", "write CPU profile to this file");
    flag.StringVar(_addr_MemProfile, "memprofile", "", "write memory profile to this file");
    flag.StringVar(_addr_Trace, "trace", "", "write trace log to this file");

    flag.BoolVar(_addr_Fix, "fix", false, "apply all suggested fixes");

}

// Run loads the packages specified by args using go/packages,
// then applies the specified analyzers to them.
// Analysis flags must already have been set.
// It provides most of the logic for the main functions of both the
// singlechecker and the multi-analysis commands.
// It returns the appropriate exit code.
public static nint Run(slice<@string> args, slice<ptr<analysis.Analyzer>> analyzers) => func((defer, _, _) => {
    nint exitcode = default;

    if (CPUProfile != "") {
        var (f, err) = os.Create(CPUProfile);
        if (err != null) {
            log.Fatal(err);
        }
        {
            var err__prev2 = err;

            var err = pprof.StartCPUProfile(f);

            if (err != null) {
                log.Fatal(err);
            } 
            // NB: profile won't be written in case of error.

            err = err__prev2;

        } 
        // NB: profile won't be written in case of error.
        defer(pprof.StopCPUProfile());

    }
    if (Trace != "") {
        (f, err) = os.Create(Trace);
        if (err != null) {
            log.Fatal(err);
        }
        {
            var err__prev2 = err;

            err = trace.Start(f);

            if (err != null) {
                log.Fatal(err);
            } 
            // NB: trace log won't be written in case of error.

            err = err__prev2;

        } 
        // NB: trace log won't be written in case of error.
        defer(() => {
            trace.Stop();
            log.Printf("To view the trace, run:\n$ go tool trace view %s", Trace);
        }());

    }
    if (MemProfile != "") {
        (f, err) = os.Create(MemProfile);
        if (err != null) {
            log.Fatal(err);
        }
        defer(() => {
            runtime.GC(); // get up-to-date statistics
            {
                var err__prev2 = err;

                err = pprof.WriteHeapProfile(f);

                if (err != null) {
                    log.Fatalf("Writing memory profile: %v", err);
                }

                err = err__prev2;

            }

            f.Close();

        }());

    }
    if (dbg('v')) {
        log.SetPrefix("");
        log.SetFlags(log.Lmicroseconds); // display timing
        log.Printf("load %s", args);

    }
    var allSyntax = needFacts(analyzers);
    var (initial, err) = load(args, allSyntax);
    if (err != null) {
        log.Print(err);
        return 1; // load errors
    }
    var roots = analyze(initial, analyzers);

    if (Fix) {
        applyFixes(roots);
    }
    return printDiagnostics(roots);

});

// load loads the initial packages.
private static (slice<ptr<packages.Package>>, error) load(slice<@string> patterns, bool allSyntax) {
    slice<ptr<packages.Package>> _p0 = default;
    error _p0 = default!;

    var mode = packages.LoadSyntax;
    if (allSyntax) {
        mode = packages.LoadAllSyntax;
    }
    ref packages.Config conf = ref heap(new packages.Config(Mode:mode,Tests:true,), out ptr<packages.Config> _addr_conf);
    var (initial, err) = packages.Load(_addr_conf, patterns);
    if (err == null) {
        {
            var n = packages.PrintErrors(initial);

            if (n > 1) {
                err = fmt.Errorf("%d errors during loading", n);
            }
            else if (n == 1) {
                err = fmt.Errorf("error during loading");
            }
            else if (len(initial) == 0) {
                err = fmt.Errorf("%s matched no packages", strings.Join(patterns, " "));
            }


        }

    }
    return (initial, error.As(err)!);

}

// TestAnalyzer applies an analysis to a set of packages (and their
// dependencies if necessary) and returns the results.
//
// Facts about pkg are returned in a map keyed by object; package facts
// have a nil key.
//
// This entry point is used only by analysistest.
public static slice<ptr<TestAnalyzerResult>> TestAnalyzer(ptr<analysis.Analyzer> _addr_a, slice<ptr<packages.Package>> pkgs) {
    ref analysis.Analyzer a = ref _addr_a.val;

    slice<ptr<TestAnalyzerResult>> results = default;
    foreach (var (_, act) in analyze(pkgs, new slice<ptr<analysis.Analyzer>>(new ptr<analysis.Analyzer>[] { a }))) {
        var facts = make_map<types.Object, slice<analysis.Fact>>();
        {
            var key__prev2 = key;
            var fact__prev2 = fact;

            foreach (var (__key, __fact) in act.objectFacts) {
                key = __key;
                fact = __fact;
                if (key.obj.Pkg() == act.pass.Pkg) {
                    facts[key.obj] = append(facts[key.obj], fact);
                }
            }

            key = key__prev2;
            fact = fact__prev2;
        }

        {
            var key__prev2 = key;
            var fact__prev2 = fact;

            foreach (var (__key, __fact) in act.packageFacts) {
                key = __key;
                fact = __fact;
                if (key.pkg == act.pass.Pkg) {
                    facts[null] = append(facts[null], fact);
                }
            }

            key = key__prev2;
            fact = fact__prev2;
        }

        results = append(results, addr(new TestAnalyzerResult(act.pass,act.diagnostics,facts,act.result,act.err)));

    }    return results;

}

public partial struct TestAnalyzerResult {
    public ptr<analysis.Pass> Pass;
    public slice<analysis.Diagnostic> Diagnostics;
    public map<types.Object, slice<analysis.Fact>> Facts;
    public error Err;
}

private static slice<ptr<action>> analyze(slice<ptr<packages.Package>> pkgs, slice<ptr<analysis.Analyzer>> analyzers) { 
    // Construct the action graph.
    if (dbg('v')) {
        log.Printf("building graph of analysis passes");
    }
    private partial struct key {
        public ref ptr<analysis.Analyzer> Analyzer> => ref Analyzer>_ptr;
        public ref ptr<packages.Package> Package> => ref Package>_ptr;
    }
    var actions = make_map<key, ptr<action>>();

    Func<ptr<analysis.Analyzer>, ptr<packages.Package>, ptr<action>> mkAction = default;
    mkAction = (a, pkg) => {
        key k = new key(a,pkg);
        var (act, ok) = actions[k];
        if (!ok) {
            act = addr(new action(a:a,pkg:pkg)); 

            // Add a dependency on each required analyzers.
            foreach (var (_, req) in a.Requires) {
                act.deps = append(act.deps, mkAction(req, pkg));
            } 

            // An analysis that consumes/produces facts
            // must run on the package's dependencies too.
            if (len(a.FactTypes) > 0) {
                var paths = make_slice<@string>(0, len(pkg.Imports));
                {
                    var path__prev1 = path;

                    foreach (var (__path) in pkg.Imports) {
                        path = __path;
                        paths = append(paths, path);
                    }

                    path = path__prev1;
                }

                sort.Strings(paths); // for determinism
                {
                    var path__prev1 = path;

                    foreach (var (_, __path) in paths) {
                        path = __path;
                        var dep = mkAction(a, pkg.Imports[path]);
                        act.deps = append(act.deps, dep);
                    }

                    path = path__prev1;
                }
            }

            actions[k] = act;

        }
        return act;

    }; 

    // Build nodes for initial packages.
    slice<ptr<action>> roots = default;
    foreach (var (_, a) in analyzers) {
        foreach (var (_, pkg) in pkgs) {
            var root = mkAction(a, pkg);
            root.isroot = true;
            roots = append(roots, root);
        }
    }    execAll(roots);

    return roots;

}

private static void applyFixes(slice<ptr<action>> roots) {
    var visited = make_map<ptr<action>, bool>();
    Func<ptr<action>, error> apply = default;
    Func<slice<ptr<action>>, error> visitAll = default;
    visitAll = actions => {
        foreach (var (_, act) in actions) {
            if (!visited[act]) {
                visited[act] = true;
                visitAll(act.deps);
                {
                    var err__prev2 = err;

                    var err = apply(act);

                    if (err != null) {
                        return err;
                    }

                    err = err__prev2;

                }

            }

        }        return null;

    }; 

    // TODO(matloob): Is this tree business too complicated? (After all this is Go!)
    // Just create a set (map) of edits, sort by pos and call it a day?
    private partial struct offsetedit {
        public nint start;
        public nint end;
        public slice<byte> newText;
    } // TextEdit using byteOffsets instead of pos
    private partial struct node {
        public offsetedit edit;
        public ptr<node> left;
        public ptr<node> right;
    }

    Func<ptr<ptr<node>>, offsetedit, error> insert = default;
    insert = (treeptr, edit) => {
        if (treeptr == null.val) {
            treeptr.val = addr(new node(edit,nil,nil));
            return null;
        }
        var tree = treeptr.val;
        if (edit.end <= tree.edit.start) {
            return insert(_addr_tree.left, edit);
        }
        else if (edit.start >= tree.edit.end) {
            return insert(_addr_tree.right, edit);
        }
        return fmt.Errorf("analyses applying overlapping text edits affecting pos range (%v, %v) and (%v, %v)", edit.start, edit.end, tree.edit.start, tree.edit.end);


    };

    var editsForFile = make_map<ptr<token.File>, ptr<node>>();

    apply = act => {
        foreach (var (_, diag) in act.diagnostics) {
            foreach (var (_, sf) in diag.SuggestedFixes) {
                {
                    var edit__prev3 = edit;

                    foreach (var (_, __edit) in sf.TextEdits) {
                        edit = __edit; 
                        // Validate the edit.
                        if (edit.Pos > edit.End) {
                            return fmt.Errorf("diagnostic for analysis %v contains Suggested Fix with malformed edit: pos (%v) > end (%v)", act.a.Name, edit.Pos, edit.End);
                        }

                        var file = act.pkg.Fset.File(edit.Pos);
                        var endfile = act.pkg.Fset.File(edit.End);
                        if (file == null || endfile == null || file != endfile) {
                            return (fmt.Errorf("diagnostic for analysis %v contains Suggested Fix with malformed spanning files %v and %v", act.a.Name, file.Name(), endfile.Name()));
                        }

                        var start = file.Offset(edit.Pos);
                        var end = file.Offset(edit.End); 

                        // TODO(matloob): Validate that edits do not affect other packages.
                        ref var root = ref heap(editsForFile[file], out ptr<var> _addr_root);
                        {
                            var err__prev1 = err;

                            err = insert(_addr_root, new offsetedit(start,end,edit.NewText));

                            if (err != null) {
                                return err;
                            }

                            err = err__prev1;

                        }

                        editsForFile[file] = root; // In case the root changed
                    }

                    edit = edit__prev3;
                }
            }

        }        return null;

    };

    visitAll(roots);

    var fset = token.NewFileSet(); // Shared by parse calls below
    // Now we've got a set of valid edits for each file. Get the new file contents.
    {
        var tree__prev1 = tree;

        foreach (var (__f, __tree) in editsForFile) {
            f = __f;
            tree = __tree;
            var (contents, err) = ioutil.ReadFile(f.Name());
            if (err != null) {
                log.Fatal(err);
            }
            nint cur = 0; // current position in the file

            bytes.Buffer @out = default;

            Action<ptr<node>> recurse = default;
            recurse = node => {
                if (node.left != null) {
                    recurse(node.left);
                }
                var edit = node.edit;
                if (edit.start > cur) {
                    @out.Write(contents[(int)cur..(int)edit.start]);
                    @out.Write(edit.newText);
                }
                cur = edit.end;

                if (node.right != null) {
                    recurse(node.right);
                }
            }
;
            recurse(tree); 
            // Write out the rest of the file.
            if (cur < len(contents)) {
                @out.Write(contents[(int)cur..]);
            } 

            // Try to format the file.
            var (ff, err) = parser.ParseFile(fset, f.Name(), @out.Bytes(), parser.ParseComments);
            if (err == null) {
                ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
                err = format.Node(_addr_buf, fset, ff);

                if (err == null) {
                    out = buf;
                }

            }

            ioutil.WriteFile(f.Name(), @out.Bytes(), 0644);

        }
        tree = tree__prev1;
    }
}

// printDiagnostics prints the diagnostics for the root packages in either
// plain text or JSON format. JSON format also includes errors for any
// dependencies.
//
// It returns the exitcode: in plain mode, 0 for success, 1 for analysis
// errors, and 3 for diagnostics. We avoid 2 since the flag package uses
// it. JSON mode always succeeds at printing errors and diagnostics in a
// structured form to stdout.
private static nint printDiagnostics(slice<ptr<action>> roots) {
    nint exitcode = default;
 
    // Print the output.
    //
    // Print diagnostics only for root packages,
    // but errors for all packages.
    var printed = make_map<ptr<action>, bool>();
    Action<ptr<action>> print = default;
    Action<slice<ptr<action>>> visitAll = default;
    visitAll = actions => {
        {
            var act__prev1 = act;

            foreach (var (_, __act) in actions) {
                act = __act;
                if (!printed[act]) {
                    printed[act] = true;
                    visitAll(act.deps);
                    print(act);
                }
            }

            act = act__prev1;
        }
    };

    if (analysisflags.JSON) { 
        // JSON output
        var tree = make(analysisflags.JSONTree);
        print = act => {
            slice<analysis.Diagnostic> diags = default;
            if (act.isroot) {
                diags = act.diagnostics;
            }
            tree.Add(act.pkg.Fset, act.pkg.ID, act.a.Name, diags, act.err);
        }
    else
;
        visitAll(roots);
        tree.Print();

    } { 
        // plain text output

        // De-duplicate diagnostics by position (not token.Pos) to
        // avoid double-reporting in source files that belong to
        // multiple packages, such as foo and foo.test.
        private partial struct key {
            public ref ptr<analysis.Analyzer> Analyzer> => ref Analyzer>_ptr;
            public ref ptr<packages.Package> Package> => ref Package>_ptr;
        }
        var seen = make_map<key, bool>();

        print = act => {
            if (act.err != null) {
                fmt.Fprintf(os.Stderr, "%s: %v\n", act.a.Name, act.err);
                exitcode = 1; // analysis failed, at least partially
                return ;

            }

            if (act.isroot) {
                foreach (var (_, diag) in act.diagnostics) { 
                    // We don't display a.Name/f.Category
                    // as most users don't care.

                    var posn = act.pkg.Fset.Position(diag.Pos);
                    var end = act.pkg.Fset.Position(diag.End);
                    key k = new key(posn,end,act.a,diag.Message);
                    if (seen[k]) {
                        continue; // duplicate
                    }

                    seen[k] = true;

                    analysisflags.PrintPlain(act.pkg.Fset, diag);

                }

            }

        };
        visitAll(roots);

        if (exitcode == 0 && len(seen) > 0) {
            exitcode = 3; // successfully produced diagnostics
        }
    }
    if (dbg('t')) {
        if (!dbg('p')) {
            log.Println("Warning: times are mostly GC/scheduler noise; use -debug=tp to disable parallelism");
        }
        slice<ptr<action>> all = default;
        time.Duration total = default;
        {
            var act__prev1 = act;

            foreach (var (__act) in printed) {
                act = __act;
                all = append(all, act);
                total += act.duration;
            }

            act = act__prev1;
        }

        sort.Slice(all, (i, j) => {
            return all[i].duration > all[j].duration;
        }); 

        // Print actions accounting for 90% of the total.
        time.Duration sum = default;
        {
            var act__prev1 = act;

            foreach (var (_, __act) in all) {
                act = __act;
                fmt.Fprintf(os.Stderr, "%s\t%s\n", act.duration, act);
                sum += act.duration;
                if (sum >= total * 9 / 10) {
                    break;
                }
            }

            act = act__prev1;
        }
    }
    return exitcode;

}

// needFacts reports whether any analysis required by the specified set
// needs facts.  If so, we must load the entire program from source.
private static bool needFacts(slice<ptr<analysis.Analyzer>> analyzers) {
    var seen = make_map<ptr<analysis.Analyzer>, bool>();
    slice<ptr<analysis.Analyzer>> q = default; // for BFS
    q = append(q, analyzers);
    while (len(q) > 0) {
        var a = q[0];
        q = q[(int)1..];
        if (!seen[a]) {
            seen[a] = true;
            if (len(a.FactTypes) > 0) {
                return true;
            }
            q = append(q, a.Requires);
        }
    }
    return false;

}

// An action represents one unit of analysis work: the application of
// one analysis to one package. Actions form a DAG, both within a
// package (as different analyzers are applied, either in sequence or
// parallel), and across packages (as dependencies are analyzed).
private partial struct action {
    public sync.Once once;
    public ptr<analysis.Analyzer> a;
    public ptr<packages.Package> pkg;
    public ptr<analysis.Pass> pass;
    public bool isroot;
    public slice<ptr<action>> deps;
    public map<objectFactKey, analysis.Fact> objectFacts;
    public map<packageFactKey, analysis.Fact> packageFacts;
    public slice<analysis.Diagnostic> diagnostics;
    public error err;
    public time.Duration duration;
}

private partial struct objectFactKey {
    public types.Object obj;
    public reflect.Type typ;
}

private partial struct packageFactKey {
    public ptr<types.Package> pkg;
    public reflect.Type typ;
}

private static @string String(this ptr<action> _addr_act) {
    ref action act = ref _addr_act.val;

    return fmt.Sprintf("%s@%s", act.a, act.pkg);
}

private static void execAll(slice<ptr<action>> actions) {
    var sequential = dbg('p');
    sync.WaitGroup wg = default;
    foreach (var (_, act) in actions) {
        wg.Add(1);
        Action<ptr<action>> work = act => {
            act.exec();
            wg.Done();
        };
        if (sequential) {
            work(act);
        }
        else
 {
            go_(() => work(act));
        }
    }    wg.Wait();

}

private static void exec(this ptr<action> _addr_act) {
    ref action act = ref _addr_act.val;

    act.once.Do(act.execOnce);
}

private static void execOnce(this ptr<action> _addr_act) => func((defer, _, _) => {
    ref action act = ref _addr_act.val;
 
    // Analyze dependencies.
    execAll(act.deps); 

    // TODO(adonovan): uncomment this during profiling.
    // It won't build pre-go1.11 but conditional compilation
    // using build tags isn't warranted.
    //
    // ctx, task := trace.NewTask(context.Background(), "exec")
    // trace.Log(ctx, "pass", act.String())
    // defer task.End()

    // Record time spent in this node but not its dependencies.
    // In parallel mode, due to GC/scheduler contention, the
    // time is 5x higher than in sequential mode, even with a
    // semaphore limiting the number of threads here.
    // So use -debug=tp.
    if (dbg('t')) {
        var t0 = time.Now();
        defer(() => {
            act.duration = time.Since(t0);
        }());

    }
    slice<@string> failed = default;
    {
        var dep__prev1 = dep;

        foreach (var (_, __dep) in act.deps) {
            dep = __dep;
            if (dep.err != null) {
                failed = append(failed, dep.String());
            }
        }
        dep = dep__prev1;
    }

    if (failed != null) {
        sort.Strings(failed);
        act.err = fmt.Errorf("failed prerequisites: %s", strings.Join(failed, ", "));
        return ;
    }
    var inputs = make();
    act.objectFacts = make_map<objectFactKey, analysis.Fact>();
    act.packageFacts = make_map<packageFactKey, analysis.Fact>();
    {
        var dep__prev1 = dep;

        foreach (var (_, __dep) in act.deps) {
            dep = __dep;
            if (dep.pkg == act.pkg) { 
                // Same package, different analysis (horizontal edge):
                // in-memory outputs of prerequisite analyzers
                // become inputs to this analysis pass.
                inputs[dep.a] = dep.result;


            }
            else if (dep.a == act.a) { // (always true)
                // Same analysis, different package (vertical edge):
                // serialized facts produced by prerequisite analysis
                // become available to this analysis pass.
                inheritFacts(_addr_act, _addr_dep);

            }

        }
        dep = dep__prev1;
    }

    ptr<analysis.Pass> pass = addr(new analysis.Pass(Analyzer:act.a,Fset:act.pkg.Fset,Files:act.pkg.Syntax,OtherFiles:act.pkg.OtherFiles,Pkg:act.pkg.Types,TypesInfo:act.pkg.TypesInfo,TypesSizes:act.pkg.TypesSizes,ResultOf:inputs,Report:func(danalysis.Diagnostic){act.diagnostics=append(act.diagnostics,d)},ImportObjectFact:act.importObjectFact,ExportObjectFact:act.exportObjectFact,ImportPackageFact:act.importPackageFact,ExportPackageFact:act.exportPackageFact,AllObjectFacts:act.allObjectFacts,AllPackageFacts:act.allPackageFacts,));
    act.pass = pass;

    slice<types.Error> errors = default; 
    // Get any type errors that are attributed to the pkg.
    // This is necessary to test analyzers that provide
    // suggested fixes for compiler/type errors.
    {
        var err__prev1 = err;

        foreach (var (_, __err) in act.pkg.Errors) {
            err = __err;
            if (err.Kind != packages.TypeError) {
                continue;
            } 
            // err.Pos is a string of form: "file:line:col" or "file:line" or "" or "-"
            var spn = span.Parse(err.Pos); 
            // Extract the token positions from the error string.
            var line = spn.Start().Line();
            var col = spn.Start().Column();
            nint offset = -1;
            act.pkg.Fset.Iterate(f => {
                if (f.Name() != spn.URI().Filename()) {
                    return true;
                }
                offset = int(f.LineStart(line)) + col - 1;
                return false;
            });
            if (offset == -1) {
                continue;
            }

            errors = append(errors, new types.Error(Fset:act.pkg.Fset,Msg:err.Msg,Pos:token.Pos(offset),));

        }
        err = err__prev1;
    }

    analysisinternal.SetTypeErrors(pass, errors);

    error err = default!;
    if (act.pkg.IllTyped && !pass.Analyzer.RunDespiteErrors) {
        err = error.As(fmt.Errorf("analysis skipped due to errors in package"))!;
    }
    else
 {
        act.result, err = pass.Analyzer.Run(pass);
        if (err == null) {
            {
                var got = reflect.TypeOf(act.result);
                var want = pass.Analyzer.ResultType;

                if (got != want) {
                    err = error.As(fmt.Errorf("internal error: on package %s, analyzer %s returned a result of type %v, but declared ResultType %v", pass.Pkg.Path(), pass.Analyzer, got, want))!;
                }

            }

        }
    }
    act.err = err; 

    // disallow calls after Run
    pass.ExportObjectFact = null;
    pass.ExportPackageFact = null;

});

// inheritFacts populates act.facts with
// those it obtains from its dependency, dep.
private static void inheritFacts(ptr<action> _addr_act, ptr<action> _addr_dep) {
    ref action act = ref _addr_act.val;
    ref action dep = ref _addr_dep.val;

    var serialize = dbg('s');

    {
        var key__prev1 = key;
        var fact__prev1 = fact;

        foreach (var (__key, __fact) in dep.objectFacts) {
            key = __key;
            fact = __fact; 
            // Filter out facts related to objects
            // that are irrelevant downstream
            // (equivalently: not in the compiler export data).
            if (!exportedFrom(key.obj, _addr_dep.pkg.Types)) {
                if (false) {
                    log.Printf("%v: discarding %T fact from %s for %s: %s", act, fact, dep, key.obj, fact);
                }
                continue;
            } 

            // Optionally serialize/deserialize fact
            // to verify that it works across address spaces.
            if (serialize) {
                var (encodedFact, err) = codeFact(fact);
                if (err != null) {
                    log.Panicf("internal error: encoding of %T fact failed in %v", fact, act);
                }
                fact = encodedFact;
            }

            if (false) {
                log.Printf("%v: inherited %T fact for %s: %s", act, fact, key.obj, fact);
            }

            act.objectFacts[key] = fact;

        }
        key = key__prev1;
        fact = fact__prev1;
    }

    {
        var key__prev1 = key;
        var fact__prev1 = fact;

        foreach (var (__key, __fact) in dep.packageFacts) {
            key = __key;
            fact = __fact; 
            // TODO: filter out facts that belong to
            // packages not mentioned in the export data
            // to prevent side channels.

            // Optionally serialize/deserialize fact
            // to verify that it works across address spaces
            // and is deterministic.
            if (serialize) {
                (encodedFact, err) = codeFact(fact);
                if (err != null) {
                    log.Panicf("internal error: encoding of %T fact failed in %v", fact, act);
                }
                fact = encodedFact;
            }

            if (false) {
                log.Printf("%v: inherited %T fact for %s: %s", act, fact, key.pkg.Path(), fact);
            }

            act.packageFacts[key] = fact;

        }
        key = key__prev1;
        fact = fact__prev1;
    }
}

// codeFact encodes then decodes a fact,
// just to exercise that logic.
private static (analysis.Fact, error) codeFact(analysis.Fact fact) {
    analysis.Fact _p0 = default;
    error _p0 = default!;
 
    // We encode facts one at a time.
    // A real modular driver would emit all facts
    // into one encoder to improve gob efficiency.
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    {
        var err__prev1 = err;

        var err = gob.NewEncoder(_addr_buf).Encode(fact);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    } 

    // Encode it twice and assert that we get the same bits.
    // This helps detect nondeterministic Gob encoding (e.g. of maps).
    ref bytes.Buffer buf2 = ref heap(out ptr<bytes.Buffer> _addr_buf2);
    {
        var err__prev1 = err;

        err = gob.NewEncoder(_addr_buf2).Encode(fact);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    if (!bytes.Equal(buf.Bytes(), buf2.Bytes())) {
        return (null, error.As(fmt.Errorf("encoding of %T fact is nondeterministic", fact))!);
    }
    analysis.Fact @new = reflect.New(reflect.TypeOf(fact).Elem()).Interface()._<analysis.Fact>();
    {
        var err__prev1 = err;

        err = gob.NewDecoder(_addr_buf).Decode(new);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    return (new, error.As(null!)!);

}

// exportedFrom reports whether obj may be visible to a package that imports pkg.
// This includes not just the exported members of pkg, but also unexported
// constants, types, fields, and methods, perhaps belonging to oether packages,
// that find there way into the API.
// This is an overapproximation of the more accurate approach used by
// gc export data, which walks the type graph, but it's much simpler.
//
// TODO(adonovan): do more accurate filtering by walking the type graph.
private static bool exportedFrom(types.Object obj, ptr<types.Package> _addr_pkg) {
    ref types.Package pkg = ref _addr_pkg.val;

    switch (obj.type()) {
        case ptr<types.Func> obj:
            return obj.Exported() && obj.Pkg() == pkg || obj.Type()._<ptr<types.Signature>>().Recv() != null;
            break;
        case ptr<types.Var> obj:
            if (obj.IsField()) {
                return true;
            } 
            // we can't filter more aggressively than this because we need
            // to consider function parameters exported, but have no way
            // of telling apart function parameters from local variables.
            return obj.Pkg() == pkg;
            break;
        case ptr<types.TypeName> obj:
            return true;
            break;
        case ptr<types.Const> obj:
            return true;
            break;
    }
    return false; // Nil, Builtin, Label, or PkgName
}

// importObjectFact implements Pass.ImportObjectFact.
// Given a non-nil pointer ptr of type *T, where *T satisfies Fact,
// importObjectFact copies the fact value to *ptr.
private static bool importObjectFact(this ptr<action> _addr_act, types.Object obj, analysis.Fact ptr) => func((_, panic, _) => {
    ref action act = ref _addr_act.val;

    if (obj == null) {
        panic("nil object");
    }
    objectFactKey key = new objectFactKey(obj,factType(ptr));
    {
        var (v, ok) = act.objectFacts[key];

        if (ok) {
            reflect.ValueOf(ptr).Elem().Set(reflect.ValueOf(v).Elem());
            return true;
        }
    }

    return false;

});

// exportObjectFact implements Pass.ExportObjectFact.
private static void exportObjectFact(this ptr<action> _addr_act, types.Object obj, analysis.Fact fact) {
    ref action act = ref _addr_act.val;

    if (act.pass.ExportObjectFact == null) {
        log.Panicf("%s: Pass.ExportObjectFact(%s, %T) called after Run", act, obj, fact);
    }
    if (obj.Pkg() != act.pkg.Types) {
        log.Panicf("internal error: in analysis %s of package %s: Fact.Set(%s, %T): can't set facts on objects belonging another package", act.a, act.pkg, obj, fact);
    }
    objectFactKey key = new objectFactKey(obj,factType(fact));
    act.objectFacts[key] = fact; // clobber any existing entry
    if (dbg('f')) {
        var objstr = types.ObjectString(obj, (types.Package.val).Name);
        fmt.Fprintf(os.Stderr, "%s: object %s has fact %s\n", act.pkg.Fset.Position(obj.Pos()), objstr, fact);
    }
}

// allObjectFacts implements Pass.AllObjectFacts.
private static slice<analysis.ObjectFact> allObjectFacts(this ptr<action> _addr_act) {
    ref action act = ref _addr_act.val;

    var facts = make_slice<analysis.ObjectFact>(0, len(act.objectFacts));
    foreach (var (k) in act.objectFacts) {
        facts = append(facts, new analysis.ObjectFact(k.obj,act.objectFacts[k]));
    }    return facts;
}

// importPackageFact implements Pass.ImportPackageFact.
// Given a non-nil pointer ptr of type *T, where *T satisfies Fact,
// fact copies the fact value to *ptr.
private static bool importPackageFact(this ptr<action> _addr_act, ptr<types.Package> _addr_pkg, analysis.Fact ptr) => func((_, panic, _) => {
    ref action act = ref _addr_act.val;
    ref types.Package pkg = ref _addr_pkg.val;

    if (pkg == null) {
        panic("nil package");
    }
    packageFactKey key = new packageFactKey(pkg,factType(ptr));
    {
        var (v, ok) = act.packageFacts[key];

        if (ok) {
            reflect.ValueOf(ptr).Elem().Set(reflect.ValueOf(v).Elem());
            return true;
        }
    }

    return false;

});

// exportPackageFact implements Pass.ExportPackageFact.
private static void exportPackageFact(this ptr<action> _addr_act, analysis.Fact fact) {
    ref action act = ref _addr_act.val;

    if (act.pass.ExportPackageFact == null) {
        log.Panicf("%s: Pass.ExportPackageFact(%T) called after Run", act, fact);
    }
    packageFactKey key = new packageFactKey(act.pass.Pkg,factType(fact));
    act.packageFacts[key] = fact; // clobber any existing entry
    if (dbg('f')) {
        fmt.Fprintf(os.Stderr, "%s: package %s has fact %s\n", act.pkg.Fset.Position(act.pass.Files[0].Pos()), act.pass.Pkg.Path(), fact);
    }
}

private static reflect.Type factType(analysis.Fact fact) {
    var t = reflect.TypeOf(fact);
    if (t.Kind() != reflect.Ptr) {
        log.Fatalf("invalid Fact type: got %T, want pointer", t);
    }
    return t;

}

// allObjectFacts implements Pass.AllObjectFacts.
private static slice<analysis.PackageFact> allPackageFacts(this ptr<action> _addr_act) {
    ref action act = ref _addr_act.val;

    var facts = make_slice<analysis.PackageFact>(0, len(act.packageFacts));
    foreach (var (k) in act.packageFacts) {
        facts = append(facts, new analysis.PackageFact(k.pkg,act.packageFacts[k]));
    }    return facts;
}

private static bool dbg(byte b) {
    return strings.IndexByte(Debug, b) >= 0;
}

} // end checker_package
