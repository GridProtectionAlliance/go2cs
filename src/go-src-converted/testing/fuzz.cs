// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using corpusEntry = go.testing_package.corpusEntryᴛ1;

namespace go;

using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using time = time_package;
using path;
using ꓸꓸꓸany = Span<any>;

partial class testing_package {

internal static void initFuzzFlags() {
    matchFuzz = flag.String("test.fuzz"u8, ""u8, "run the fuzz test matching `regexp`"u8);
    flag.Var(fuzzDuration, "test.fuzztime"u8, "time to spend fuzzing; default is to run indefinitely"u8);
    flag.Var(minimizeDuration, "test.fuzzminimizetime"u8, "time to spend minimizing a value after finding a failing input"u8);
    fuzzCacheDir = flag.String("test.fuzzcachedir"u8, ""u8, "directory where interesting fuzzing inputs are stored (for use only by cmd/go)"u8);
    isFuzzWorker = flag.Bool("test.fuzzworker"u8, false, "coordinate with the parent process to fuzz random values (for use only by cmd/go)"u8);
}

internal static ж<@string> matchFuzz;
internal static durationOrCountFlag fuzzDuration;
internal static durationOrCountFlag minimizeDuration = new durationOrCountFlag(d: 60 * time.ΔSecond, allowZero: true);
internal static ж<@string> fuzzCacheDir;
internal static ж<bool> isFuzzWorker;
internal static @string corpusDir = "testdata/fuzz"u8;

// fuzzWorkerExitCode is used as an exit code by fuzz worker processes after an
// internal error. This distinguishes internal errors from uncontrolled panics
// and other failures. Keep in sync with internal/fuzz.workerExitCode.
internal static readonly UntypedInt fuzzWorkerExitCode = 70;

// InternalFuzzTarget is an internal type but exported because it is
// cross-package; it is part of the implementation of the "go test" command.
[GoType] partial struct InternalFuzzTarget {
    public @string Name;
    public Action<ж<F>> Fn;
}

// F is a type passed to fuzz tests.
//
// Fuzz tests run generated inputs against a provided fuzz target, which can
// find and report potential bugs in the code being tested.
//
// A fuzz test runs the seed corpus by default, which includes entries provided
// by (*F).Add and entries in the testdata/fuzz/<FuzzTestName> directory. After
// any necessary setup and calls to (*F).Add, the fuzz test must then call
// (*F).Fuzz to provide the fuzz target. See the testing package documentation
// for an example, and see the [F.Fuzz] and [F.Add] method documentation for
// details.
//
// *F methods can only be called before (*F).Fuzz. Once the test is
// executing the fuzz target, only (*T) methods can be used. The only *F methods
// that are allowed in the (*F).Fuzz function are (*F).Failed and (*F).Name.
[GoType] partial struct F {
    internal partial ref common common { get; }
    internal ж<fuzzContext> fuzzContext;
    internal ж<testContext> testContext;
    // inFuzzFn is true when the fuzz function is running. Most F methods cannot
    // be called when inFuzzFn is true.
    internal bool inFuzzFn;
    // corpus is a set of seed corpus entries, added with F.Add and loaded
    // from testdata.
    internal slice<corpusEntry> corpus;
    internal fuzzResult result;
    internal bool fuzzCalled;
}

internal static TB _ᴛ1ʗ = (ж<F>)(default!);

// corpusEntry is an alias to the same type as internal/fuzz.CorpusEntry.
// We use a type alias because we don't want to export this type, and we can't
// import internal/fuzz from testing.
[GoType("dyn")] partial struct corpusEntryᴛ1 {
    public @string Parent;
    public @string Path;
    public slice<byte> Data;
    public slice<any> Values;
    public nint Generation;
    public bool IsSeed;
}

[GoType("dyn")] partial struct Helper_f {
}

// Helper marks the calling function as a test helper function.
// When printing file and line information, that function will be skipped.
// Helper may be called simultaneously from multiple goroutines.
[GoRecv] public static void Helper(this ref F f) => func((defer, _) => {
    if (f.inFuzzFn) {
        throw panic("testing: f.Helper was called inside the fuzz target, use t.Helper instead");
    }
    // common.Helper is inlined here.
    // If we called it, it would mark F.Helper as the helper
    // instead of the caller.
    f.mu.Lock();
    defer(f.mu.Unlock);
    if (f.helperPCs == default!) {
        f.helperPCs = new map<uintptr, EmptyStruct>();
    }
    // repeating code from callerName here to save walking a stack frame
    array<uintptr> pc = new(1);
    nint n = runtime.Callers(2, pc[..]);
    // skip runtime.Callers + Helper
    if (n == 0) {
        throw panic("testing: zero callers found");
    }
    {
        var (_, found) = f.helperPCs[pc[0]]; if (!found) {
            f.helperPCs[pc[0]] = new Helper_f();
            f.helperNames = default!;
        }
    }
});

// map will be recreated next time it is needed

// Fail marks the function as having failed but continues execution.
[GoRecv] public static void Fail(this ref F f) {
    // (*F).Fail may be called by (*T).Fail, which we should allow. However, we
    // shouldn't allow direct (*F).Fail calls from inside the (*F).Fuzz function.
    if (f.inFuzzFn) {
        throw panic("testing: f.Fail was called inside the fuzz target, use t.Fail instead");
    }
    f.common.Helper();
    f.common.Fail();
}

// Skipped reports whether the test was skipped.
[GoRecv] public static bool Skipped(this ref F f) {
    // (*F).Skipped may be called by tRunner, which we should allow. However, we
    // shouldn't allow direct (*F).Skipped calls from inside the (*F).Fuzz function.
    if (f.inFuzzFn) {
        throw panic("testing: f.Skipped was called inside the fuzz target, use t.Skipped instead");
    }
    f.common.Helper();
    return f.common.Skipped();
}

// Add will add the arguments to the seed corpus for the fuzz test. This will be
// a no-op if called after or within the fuzz target, and args must match the
// arguments for the fuzz target.
[GoRecv] public static void Add(this ref F f, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    slice<any> values = default!;
    foreach (var (i, _) in args) {
        {
            var t = reflect.TypeOf(args[i]); if (!supportedTypes[t]) {
                throw panic(fmt.Sprintf("testing: unsupported type to Add %v"u8, t));
            }
        }
        values = append(values, args[i]);
    }
    f.corpus = append(f.corpus, new corpusEntry{Values: values, IsSeed: true, Path: fmt.Sprintf("seed#%d"u8, len(f.corpus))});
}

// supportedTypes represents all of the supported types which can be fuzzed.
internal static map<reflectꓸType, bool> supportedTypes = new map<reflectꓸType, bool>{
    [reflect.TypeOf((slice<byte>)(""))] = true,
    [reflect.TypeOf(((@string)""u8))] = true,
    [reflect.TypeOf(((bool)false))] = true,
    [reflect.TypeOf(((byte)0))] = true,
    [reflect.TypeOf(((rune)0))] = true,
    [reflect.TypeOf(((float32)0))] = true,
    [reflect.TypeOf(((float64)0))] = true,
    [reflect.TypeOf(((nint)0))] = true,
    [reflect.TypeOf(((int8)0))] = true,
    [reflect.TypeOf(((int16)0))] = true,
    [reflect.TypeOf(((int32)0))] = true,
    [reflect.TypeOf(((int64)0))] = true,
    [reflect.TypeOf(((nuint)0))] = true,
    [reflect.TypeOf(((uint8)0))] = true,
    [reflect.TypeOf(((uint16)0))] = true,
    [reflect.TypeOf(((uint32)0))] = true,
    [reflect.TypeOf(((uint64)0))] = true
};

// Fuzz runs the fuzz function, ff, for fuzz testing. If ff fails for a set of
// arguments, those arguments will be added to the seed corpus.
//
// ff must be a function with no return value whose first argument is *T and
// whose remaining arguments are the types to be fuzzed.
// For example:
//
//	f.Fuzz(func(t *testing.T, b []byte, i int) { ... })
//
// The following types are allowed: []byte, string, bool, byte, rune, float32,
// float64, int, int8, int16, int32, int64, uint, uint8, uint16, uint32, uint64.
// More types may be supported in the future.
//
// ff must not call any *F methods, e.g. (*F).Log, (*F).Error, (*F).Skip. Use
// the corresponding *T method instead. The only *F methods that are allowed in
// the (*F).Fuzz function are (*F).Failed and (*F).Name.
//
// This function should be fast and deterministic, and its behavior should not
// depend on shared state. No mutable input arguments, or pointers to them,
// should be retained between executions of the fuzz function, as the memory
// backing them may be mutated during a subsequent invocation. ff must not
// modify the underlying data of the arguments provided by the fuzzing engine.
//
// When fuzzing, F.Fuzz does not return until a problem is found, time runs out
// (set with -fuzztime), or the test process is interrupted by a signal. F.Fuzz
// should be called exactly once, unless F.Skip or [F.Fail] is called beforehand.
[GoRecv] public static void Fuzz(this ref F f, any ff) => func((defer, _) => {
    if (f.fuzzCalled) {
        throw panic("testing: F.Fuzz called more than once");
    }
    f.fuzzCalled = true;
    if (f.failed) {
        return;
    }
    f.Helper();
    // ff should be in the form func(*testing.T, ...interface{})
    ref var fn = ref heap<reflect_package.ΔValue>(out var Ꮡfn);
    fn = reflect.ValueOf(ff);
    var fnType = fn.Type();
    if (fnType.Kind() != reflect.Func) {
        throw panic("testing: F.Fuzz must receive a function");
    }
    if (fnType.NumIn() < 2 || !AreEqual(fnType.In(0), reflect.TypeOf((ж<T>)(default!)))) {
        throw panic("testing: fuzz target must receive at least two arguments, where the first argument is a *T");
    }
    if (fnType.NumOut() != 0) {
        throw panic("testing: fuzz target must not return a value");
    }
    // Save the types of the function to compare against the corpus.
    slice<reflectꓸType> types = default!;
    for (nint i = 1; i < fnType.NumIn(); i++) {
        var t = fnType.In(i);
        if (!supportedTypes[t]) {
            throw panic(fmt.Sprintf("testing: unsupported type for fuzzing %v"u8, t));
        }
        types = append(types, t);
    }
    // Load the testdata seed corpus. Check types of entries in the testdata
    // corpus and entries declared with F.Add.
    //
    // Don't load the seed corpus if this is a worker process; we won't use it.
    if (f.fuzzContext.mode != fuzzWorker) {
        foreach (var (_, c) in f.corpus) {
            {
                var err = f.fuzzContext.deps.CheckCorpus(c.Values, types); if (err != default!) {
                    // TODO(#48302): Report the source location of the F.Add call.
                    f.Fatal(err);
                }
            }
        }
        // Load seed corpus
        (c, err) = f.fuzzContext.deps.ReadCorpus(filepath.Join(corpusDir, f.name), types);
        if (err != default!) {
            f.Fatal(err);
        }
        foreach (var (i, _) in c) {
            c[i].IsSeed = true;
            // these are all seed corpus values
            if (f.fuzzContext.mode == fuzzCoordinator) {
                // If this is the coordinator process, zero the values, since we don't need
                // to hold onto them.
                c[i].Values = default!;
            }
        }
        f.corpus = append(f.corpus, c.ꓸꓸꓸ);
    }
    // run calls fn on a given input, as a subtest with its own T.
    // run is analogous to T.Run. The test filtering and cleanup works similarly.
    // fn is called in its own goroutine.
    var run = 
    var fnʗ1 = fn;
    (io.Writer captureOut, corpusEntry e) => {
        if (e.Values == default!) {
            // The corpusEntry must have non-nil Values in order to run the
            // test. If Values is nil, it is a bug in our code.
            throw panic(fmt.Sprintf("corpus file %q was not unmarshaled"u8, e.Path));
        }
        if (shouldFailFast()) {
            return true;
        }
        @string testName = f.name;
        if (e.Path != ""u8) {
            testName = fmt.Sprintf("%s/%s"u8, testName, filepath.Base(e.Path));
        }
        if (f.testContext.isFuzzing) {
            // Don't preserve subtest names while fuzzing. If fn calls T.Run,
            // there will be a very large number of subtests with duplicate names,
            // which will use a large amount of memory. The subtest names aren't
            // useful since there's no way to re-run them deterministically.
            f.testContext.match.clearSubNames();
        }
        // Record the stack trace at the point of this call so that if the subtest
        // function - which runs in a separate stack - is marked as a helper, we can
        // continue walking the stack into the parent test.
        ref var pc = ref heap(new array<uintptr>(50), out var Ꮡpc);
        nint n = runtime.Callers(2, pc[..]);
        var t = Ꮡ(new T(
            common: new common(
                barrier: new channel<bool>(1),
                signal: new channel<bool>(1),
                name: testName,
                parent: Ꮡ(f.common),
                level: f.level + 1,
                creator: pc[..(int)(n)],
                chatty: f.chatty
            ),
            context: f.testContext
        ));
        if (captureOut != default!) {
            // t.parent aliases f.common.
            t.parent.val.w = captureOut;
        }
        t.w = new indenter(Ꮡ((~t).common));
        if (t.chatty != nil) {
            t.chatty.Updatef(t.name, "=== RUN   %s\n"u8, t.name);
        }
        (f.common.inFuzzFn, f.inFuzzFn) = (true, true);
        goǃ(tRunner, t, 
        var eʗ1 = e;
        var fnʗ2 = fn;
        (ж<T> t) => {
            var args = new reflectꓸValue[]{reflect.ValueOf(tΔ1)}.slice();
            foreach (var (_, v) in eʗ1.Values) {
                args = append(args, reflect.ValueOf(v));
            }
            // Before resetting the current coverage, defer the snapshot so that
            // we make sure it is called right before the tRunner function
            // exits, regardless of whether it was executed cleanly, panicked,
            // or if the fuzzFn called t.Fatal.
            if (f.testContext.isFuzzing) {
                defer(f.fuzzContext.deps.SnapshotCoverage);
                f.fuzzContext.deps.ResetCoverage();
            }
            fn.Call(args);
        });
        ᐸꟷ(t.signal);
        if (t.chatty != nil && (~t.chatty).json) {
            t.chatty.Updatef((~t.parent).name, "=== NAME  %s\n"u8, (~t.parent).name);
        }
        (f.common.inFuzzFn, f.inFuzzFn) = (false, false);
        return !t.Failed();
    };
    var exprᴛ1 = f.fuzzContext.mode;
    if (exprᴛ1 == fuzzCoordinator) {
        @string corpusTargetDir = filepath.Join(corpusDir, // Fuzzing is enabled, and this is the test process started by 'go test'.
 // Act as the coordinator process, and coordinate workers to perform the
 // actual fuzzing.
 f.name);
        @string cacheTargetDir = filepath.Join(fuzzCacheDir.val, f.name);
        var err = f.fuzzContext.deps.CoordinateFuzzing(
            fuzzDuration.d,
            ((int64)fuzzDuration.n),
            minimizeDuration.d,
            ((int64)minimizeDuration.n),
            parallel.val,
            f.corpus,
            types,
            corpusTargetDir,
            cacheTargetDir);
        if (err != default!) {
            f.result = new fuzzResult(Error: err);
            f.Fail();
            fmt.Fprintf(f.w, "%v\n"u8, err);
            {
                var (crashErr, ok) = err._<fuzzCrashError>(ᐧ); if (ok) {
                    @string crashPath = crashErr.CrashPath();
                    fmt.Fprintf(f.w, "Failing input written to %s\n"u8, crashPath);
                    @string testName = filepath.Base(crashPath);
                    fmt.Fprintf(f.w, "To re-run:\ngo test -run=%s/%s\n"u8, f.name, testName);
                }
            }
        }
    }
    else if (exprᴛ1 == fuzzWorker) {
        {
            var err = f.fuzzContext.deps.RunFuzzWorker(
            var runʗ2 = run;
            (corpusEntry e) => {
                
                ref var buf = ref heap(new strings_package.Builder(), out var Ꮡbuf);
                {
                    var ok = runʗ2(~Ꮡbuf, e); if (!ok) {
                        return errors.New(buf.String());
                    }
                }
                return default!;
            }); if (err != default!) {
                // Internal errors are marked with f.Fail; user code may call this too, before F.Fuzz.
                // The worker will exit with fuzzWorkerExitCode, indicating this is a failure
                // (and 'go test' should exit non-zero) but a failing input should not be recorded.
                f.Errorf("communicating with fuzzing coordinator: %v"u8, err);
            }
        }
    }
    else { /* default: */
        foreach (var (_, e) in f.corpus) {
            // Fuzzing is not enabled, or will be done later. Only run the seed
            // corpus now.
            @string name = fmt.Sprintf("%s/%s"u8, f.name, filepath.Base(e.Path));
            {
                var (_, ok, _) = f.testContext.match.fullName(nil, name); if (ok) {
                    run(f.w, e);
                }
            }
        }
    }

});

[GoRecv] internal static void report(this ref F f) {
    if (isFuzzWorker.val || f.parent == nil) {
        return;
    }
    @string dstr = fmtDuration(f.duration);
    @string format = "--- %s: %s (%s)\n"u8;
    if (f.Failed()){
        f.flushToParent(f.name, format, "FAIL", f.name, dstr);
    } else 
    if (f.chatty != nil) {
        if (f.Skipped()){
            f.flushToParent(f.name, format, "SKIP", f.name, dstr);
        } else {
            f.flushToParent(f.name, format, "PASS", f.name, dstr);
        }
    }
}

// fuzzResult contains the results of a fuzz run.
[GoType] partial struct fuzzResult {
    public nint N;          // The number of iterations.
    public time_package.Duration T; // The total time taken.
    public error Error;         // Error is the error from the failing input
}

internal static @string String(this fuzzResult r) {
    if (r.Error == default!) {
        return ""u8;
    }
    return r.Error.Error();
}

// fuzzCrashError is satisfied by a failing input detected while fuzzing.
// These errors are written to the seed corpus and can be re-run with 'go test'.
// Errors within the fuzzing framework (like I/O errors between coordinator
// and worker processes) don't satisfy this interface.
[GoType] partial interface fuzzCrashError :
    error
{
    error Unwrap();
    // CrashPath returns the path of the subtest that corresponds to the saved
    // crash input file in the seed corpus. The test can be re-run with go test
    // -run=$test/$name $test is the fuzz test name, and $name is the
    // filepath.Base of the string returned here.
    @string CrashPath();
}

// fuzzContext holds fields common to all fuzz tests.
[GoType] partial struct fuzzContext {
    internal testDeps deps;
    internal fuzzMode mode;
}

[GoType("num:uint8")] partial struct fuzzMode;

internal static readonly fuzzMode seedCorpusOnly = /* iota */ 0;
internal static readonly fuzzMode fuzzCoordinator = 1;
internal static readonly fuzzMode fuzzWorker = 2;

// runFuzzTests runs the fuzz tests matching the pattern for -run. This will
// only run the (*F).Fuzz function for each seed corpus without using the
// fuzzing engine to generate or mutate inputs.
internal static (bool ran, bool ok) runFuzzTests(testDeps deps, slice<InternalFuzzTarget> fuzzTests, time.Time deadline) {
    bool ran = default!;
    bool ok = default!;

    ok = true;
    if (len(fuzzTests) == 0 || isFuzzWorker.val) {
        return (ran, ok);
    }
    var m = newMatcher(deps.MatchString, match.val, "-test.run"u8, skip.val);
    ж<matcher> mFuzz = default!;
    if (matchFuzz.val != ""u8) {
        mFuzz = newMatcher(deps.MatchString, matchFuzz.val, "-test.fuzz"u8, skip.val);
    }
    foreach (var (_, procs) in cpuList) {
        runtime.GOMAXPROCS(procs);
        for (nuint i = ((nuint)0); i < count.val; i++) {
            if (shouldFailFast()) {
                break;
            }
            var tctx = newTestContext(parallel.val, m);
            tctx.val.deadline = deadline;
            var fctx = Ꮡ(new fuzzContext(deps: deps, mode: seedCorpusOnly));
            ref var root = ref heap<common>(out var Ꮡroot);
            root = new common(w: os.Stdout);
            // gather output in one place
            if (Verbose()) {
                root.chatty = newChattyPrinter(root.w);
            }
            ref var ft = ref heap(new InternalFuzzTarget(), out var Ꮡft);

            foreach (var (_, ft) in fuzzTests) {
                if (shouldFailFast()) {
                    break;
                }
                var (testName, matched, _) = (~tctx).match.fullName(nil, ft.Name);
                if (!matched) {
                    continue;
                }
                if (mFuzz != nil) {
                    {
                        var (_, fuzzMatched, _) = mFuzz.fullName(nil, ft.Name); if (fuzzMatched) {
                            // If this will be fuzzed, then don't run the seed corpus
                            // right now. That will happen later.
                            continue;
                        }
                    }
                }
                var f = Ꮡ(new F(
                    common: new common(
                        signal: new channel<bool>(1),
                        barrier: new channel<bool>(1),
                        name: testName,
                        parent: Ꮡroot,
                        level: root.level + 1,
                        chatty: root.chatty
                    ),
                    testContext: tctx,
                    fuzzContext: fctx
                ));
                f.w = new indenter(Ꮡ((~f).common));
                if (f.chatty != nil) {
                    f.chatty.Updatef(f.name, "=== RUN   %s\n"u8, f.name);
                }
                goǃ(fRunner, f, ft.Fn);
                ᐸꟷ(f.signal);
                if (f.chatty != nil && (~f.chatty).json) {
                    f.chatty.Updatef((~f.parent).name, "=== NAME  %s\n"u8, (~f.parent).name);
                }
                ok = ok && !f.Failed();
                ran = ran || f.ran;
            }
            if (!ran) {
                // There were no tests to run on this iteration.
                // This won't change, so no reason to keep trying.
                break;
            }
        }
    }
    return (ran, ok);
}

// runFuzzing runs the fuzz test matching the pattern for -fuzz. Only one such
// fuzz test must match. This will run the fuzzing engine to generate and
// mutate new inputs against the fuzz target.
//
// If fuzzing is disabled (-test.fuzz is not set), runFuzzing
// returns immediately.
internal static bool /*ok*/ runFuzzing(testDeps deps, slice<InternalFuzzTarget> fuzzTests) {
    bool ok = default!;

    if (len(fuzzTests) == 0 || matchFuzz.val == ""u8) {
        return true;
    }
    var m = newMatcher(deps.MatchString, matchFuzz.val, "-test.fuzz"u8, skip.val);
    var tctx = newTestContext(1, m);
    tctx.val.isFuzzing = true;
    var fctx = Ꮡ(new fuzzContext(
        deps: deps
    ));
    ref var root = ref heap<common>(out var Ꮡroot);
    root = new common(w: os.Stdout);
    if (isFuzzWorker.val){
        root.w = io.Discard;
        fctx.val.mode = fuzzWorker;
    } else {
        fctx.val.mode = fuzzCoordinator;
    }
    if (Verbose() && !isFuzzWorker.val) {
        root.chatty = newChattyPrinter(root.w);
    }
    ж<InternalFuzzTarget> fuzzTest = default!;
    @string testName = default!;
    slice<@string> matched = default!;
    foreach (var (i, _) in fuzzTests) {
        var (name, okΔ1, _) = (~tctx).match.fullName(nil, fuzzTests[i].Name);
        if (!okΔ1) {
            continue;
        }
        matched = append(matched, name);
        fuzzTest = Ꮡ(fuzzTests, i);
        testName = name;
    }
    if (len(matched) == 0) {
        fmt.Fprintln(~os.Stderr, "testing: warning: no fuzz tests to fuzz");
        return true;
    }
    if (len(matched) > 1) {
        fmt.Fprintf(~os.Stderr, "testing: will not fuzz, -fuzz matches more than one fuzz test: %v\n"u8, matched);
        return false;
    }
    var f = Ꮡ(new F(
        common: new common(
            signal: new channel<bool>(1),
            barrier: default!, // T.Parallel has no effect when fuzzing.

            name: testName,
            parent: Ꮡroot,
            level: root.level + 1,
            chatty: root.chatty
        ),
        fuzzContext: fctx,
        testContext: tctx
    ));
    f.w = new indenter(Ꮡ((~f).common));
    if (f.chatty != nil) {
        f.chatty.Updatef(f.name, "=== RUN   %s\n"u8, f.name);
    }
    goǃ(fRunner, f, (~fuzzTest).Fn);
    ᐸꟷ(f.signal);
    if (f.chatty != nil) {
        f.chatty.Updatef((~f.parent).name, "=== NAME  %s\n"u8, (~f.parent).name);
    }
    return !f.failed;
}

// fRunner wraps a call to a fuzz test and ensures that cleanup functions are
// called and status flags are set. fRunner should be called in its own
// goroutine. To wait for its completion, receive from f.signal.
//
// fRunner is analogous to tRunner, which wraps subtests started with T.Run.
// Unit tests and fuzz tests work a little differently, so for now, these
// functions aren't consolidated. In particular, because there are no F.Run and
// F.Parallel methods, i.e., no fuzz sub-tests or parallel fuzz tests, a few
// simplifications are made. We also require that F.Fuzz, F.Skip, or F.Fail is
// called.
internal static void fRunner(ж<F> Ꮡf, Action<ж<F>> fn) => func((defer, recover) => {
    ref var f = ref Ꮡf.val;

    // When this goroutine is done, either because runtime.Goexit was called, a
    // panic started, or fn returned normally, record the duration and send
    // t.signal, indicating the fuzz test is done.
    var numFailedʗ1 = numFailed;
    defer(() => {
        // Detect whether the fuzz test panicked or called runtime.Goexit
        // without calling F.Fuzz, F.Fail, or F.Skip. If it did, panic (possibly
        // replacing a nil panic value). Nothing should recover after fRunner
        // unwinds, so this should crash the process and print stack.
        // Unfortunately, recovering here adds stack frames, but the location of
        // the original panic should still be
        // clear.
        f.checkRaces();
        if (f.Failed()) {
            numFailedʗ1.Add(1);
        }
        var err = recover();
        if (err == default!) {
            f.mu.RLock();
            var fuzzNotCalled = !f.fuzzCalled && !f.skipped && !f.failed;
            if (!f.finished && !f.skipped && !f.failed) {
                err = errNilPanicOrGoexit;
            }
            f.mu.RUnlock();
            if (fuzzNotCalled && err == default!) {
                f.Error("returned without calling F.Fuzz, F.Fail, or F.Skip");
            }
        }
        // Use a deferred call to ensure that we report that the test is
        // complete even if a cleanup function calls F.FailNow. See issue 41355.
        var didPanic = false;
        defer(() => {
            if (!didPanic) {
                // Only report that the test is complete if it doesn't panic,
                // as otherwise the test binary can exit before the panic is
                // reported to the user. See issue 41479.
                f.signal.ᐸꟷ(true);
            }
        });
        // If we recovered a panic or inappropriate runtime.Goexit, fail the test,
        // flush the output log up to the root, then panic.
        var doPanic = 
        (any err) => {
            f.Fail();
            {
                var r = f.runCleanup(recoverAndReturnPanic); if (r != default!) {
                    f.Logf("cleanup panicked with %v"u8, r);
                }
            }
            for (var root = Ꮡ(f.common); (~root).parent != nil; root = root.val.parent) {
                (~root).mu.Lock();
                root.val.duration += highPrecisionTimeSince((~root).start);
                var d = root.val.duration;
                (~root).mu.Unlock();
                root.flushToParent((~root).name, "--- FAIL: %s (%s)\n"u8, (~root).name, fmtDuration(d));
            }
            didPanic = true;
            throw panic(errΔ1);
        };
        if (err != default!) {
            doPanic(err);
        }
        // No panic or inappropriate Goexit.
        f.duration += highPrecisionTimeSince(f.start);
        if (len(f.sub) > 0) {
            // Unblock inputs that called T.Parallel while running the seed corpus.
            // This only affects fuzz tests run as normal tests.
            // While fuzzing, T.Parallel has no effect, so f.sub is empty, and this
            // branch is not taken. f.barrier is nil in that case.
            f.testContext.release();
            close(f.barrier);
            // Wait for the subtests to complete.
            foreach (var (_, sub) in f.sub) {
                ᐸꟷ(sub.signal);
            }
            ref var cleanupStart = ref heap<highPrecisionTime>(out var ᏑcleanupStart);
            cleanupStart = highPrecisionTimeNow();
            var errΔ2 = f.runCleanup(recoverAndReturnPanic);
            f.duration += highPrecisionTimeSince(cleanupStart);
            if (errΔ2 != default!) {
                doPanic(errΔ2);
            }
        }
        // Report after all subtests have finished.
        f.report();
        f.done = true;
        f.setRan();
    });
    defer(() => {
        if (len(f.sub) == 0) {
            f.runCleanup(normalPanic);
        }
    });
    f.start = highPrecisionTimeNow();
    f.resetRaces();
    fn(Ꮡf);
    // Code beyond this point will not be executed when FailNow or SkipNow
    // is invoked.
    f.mu.Lock();
    f.finished = true;
    f.mu.Unlock();
});

} // end testing_package
