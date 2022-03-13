// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Action graph creation (planning).

// package work -- go2cs converted at 2022 March 13 06:29:59 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\work\action.go
namespace go.cmd.go.@internal;

using bufio = bufio_package;
using bytes = bytes_package;
using heap = container.heap_package;
using context = context_package;
using elf = debug.elf_package;
using json = encoding.json_package;
using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;

using @base = cmd.go.@internal.@base_package;
using cache = cmd.go.@internal.cache_package;
using cfg = cmd.go.@internal.cfg_package;
using load = cmd.go.@internal.load_package;
using trace = cmd.go.@internal.trace_package;
using buildid = cmd.@internal.buildid_package;


// A Builder holds global state about a build.
// It does not hold per-package state, because we
// build packages in parallel, and the builder is shared.

using System;
using System.ComponentModel;
public static partial class work_package {

public partial struct Builder {
    public @string WorkDir; // the temporary work directory (ends in filepath.Separator)
    public map<cacheKey, ptr<Action>> actionCache; // a cache of already-constructed actions
    public map<@string, bool> mkdirCache; // a cache of created directories
    public map<array<@string>, bool> flagCache; // a cache of supported compiler flags
    public Func<object[], (nint, error)> Print;
    public bool IsCmdList; // running as part of go list; set p.Stale and additional fields below
    public bool NeedError; // list needs p.Error
    public bool NeedExport; // list needs p.Export
    public bool NeedCompiledGoFiles; // list needs p.CompiledGoFiles

    public nint objdirSeq; // counter for NewObjdir
    public nint pkgSeq;
    public sync.Mutex output;
    public @string scriptDir; // current directory in printed script

    public sync.Mutex exec;
    public channel<bool> readySema;
    public actionQueue ready;
    public sync.Mutex id;
    public map<@string, @string> toolIDCache; // tool name -> tool ID
    public map<@string, @string> buildIDCache; // file name -> build ID
}

// NOTE: Much of Action would not need to be exported if not for test.
// Maybe test functionality should move into this package too?

// An Action represents a single action in the action graph.
public partial struct Action {
    public @string Mode; // description of action operation
    public ptr<load.Package> Package; // the package this action works on
    public slice<ptr<Action>> Deps; // actions that must happen before this one
    public Func<ptr<Builder>, context.Context, ptr<Action>, error> Func; // the action itself (nil = no-op)
    public bool IgnoreFail; // whether to run f even if dependencies fail
    public ptr<bytes.Buffer> TestOutput; // test output buffer
    public slice<@string> Args; // additional args for runProgram

    public slice<ptr<Action>> triggers; // inverse of deps

    public bool buggyInstall; // is this a buggy install (see -linkshared)?

    public Func<ptr<Builder>, ptr<Action>, bool> TryCache; // callback for cache bypass

// Generated files, directories.
    public @string Objdir; // directory for intermediate objects
    public @string Target; // goal of the action: the created package or executable
    public @string built; // the actual created package or executable
    public cache.ActionID actionID; // cache ID of action input
    public @string buildID; // build ID of action output

    public bool VetxOnly; // Mode=="vet": only being called to supply info about dependencies
    public bool needVet; // Mode=="build": need to fill in vet config
    public bool needBuild; // Mode=="build": need to do actual build (can be false if needVet is true)
    public ptr<vetConfig> vetCfg; // vet config
    public slice<byte> output; // output redirect buffer (nil means use b.Print)

// Execution state.
    public nint pending; // number of deps yet to complete
    public nint priority; // relative execution priority
    public bool Failed; // whether the action failed
    public ptr<actionJSON> json; // action graph information
    public map<@string, @string> nonGoOverlay; // map from non-.go source files to copied files in objdir. Nil if no overlay is used.
    public ptr<trace.Span> traceSpan;
}

// BuildActionID returns the action ID section of a's build ID.
private static @string BuildActionID(this ptr<Action> _addr_a) {
    ref Action a = ref _addr_a.val;

    return actionID(a.buildID);
}

// BuildContentID returns the content ID section of a's build ID.
private static @string BuildContentID(this ptr<Action> _addr_a) {
    ref Action a = ref _addr_a.val;

    return contentID(a.buildID);
}

// BuildID returns a's build ID.
private static @string BuildID(this ptr<Action> _addr_a) {
    ref Action a = ref _addr_a.val;

    return a.buildID;
}

// BuiltTarget returns the actual file that was built. This differs
// from Target when the result was cached.
private static @string BuiltTarget(this ptr<Action> _addr_a) {
    ref Action a = ref _addr_a.val;

    return a.built;
}

// An actionQueue is a priority queue of actions.
private partial struct actionQueue { // : slice<ptr<Action>>
}

// Implement heap.Interface
private static nint Len(this ptr<actionQueue> _addr_q) {
    ref actionQueue q = ref _addr_q.val;

    return len(q.val);
}
private static void Swap(this ptr<actionQueue> _addr_q, nint i, nint j) {
    ref actionQueue q = ref _addr_q.val;

    ((q.val)[i], (q.val)[j]) = ((q.val)[j], (q.val)[i]);
}
private static bool Less(this ptr<actionQueue> _addr_q, nint i, nint j) {
    ref actionQueue q = ref _addr_q.val;

    return (q.val)[i].priority < (q.val)[j].priority;
}
private static void Push(this ptr<actionQueue> _addr_q, object x) {
    ref actionQueue q = ref _addr_q.val;

    q.val = append(q.val, x._<ptr<Action>>());
}
private static void Pop(this ptr<actionQueue> _addr_q) {
    ref actionQueue q = ref _addr_q.val;

    var n = len(q.val) - 1;
    var x = (q.val)[n];
    q.val = (q.val)[..(int)n];
    return x;
}

private static void push(this ptr<actionQueue> _addr_q, ptr<Action> _addr_a) {
    ref actionQueue q = ref _addr_q.val;
    ref Action a = ref _addr_a.val;

    if (a.json != null) {
        a.json.TimeReady = time.Now();
    }
    heap.Push(q, a);
}

private static ptr<Action> pop(this ptr<actionQueue> _addr_q) {
    ref actionQueue q = ref _addr_q.val;

    return heap.Pop(q)._<ptr<Action>>();
}

private partial struct actionJSON {
    public nint ID;
    public @string Mode;
    public @string Package;
    [Description("json:\",omitempty\"")]
    public slice<nint> Deps;
    [Description("json:\",omitempty\"")]
    public bool IgnoreFail;
    [Description("json:\",omitempty\"")]
    public slice<@string> Args;
    [Description("json:\",omitempty\"")]
    public bool Link;
    [Description("json:\",omitempty\"")]
    public @string Objdir;
    [Description("json:\",omitempty\"")]
    public @string Target;
    [Description("json:\",omitempty\"")]
    public nint Priority;
    [Description("json:\",omitempty\"")]
    public bool Failed;
    [Description("json:\",omitempty\"")]
    public @string Built;
    [Description("json:\",omitempty\"")]
    public bool VetxOnly;
    [Description("json:\",omitempty\"")]
    public bool NeedVet;
    [Description("json:\",omitempty\"")]
    public bool NeedBuild;
    [Description("json:\",omitempty\"")]
    public @string ActionID;
    [Description("json:\",omitempty\"")]
    public @string BuildID;
    [Description("json:\",omitempty\"")]
    public time.Time TimeReady;
    [Description("json:\",omitempty\"")]
    public time.Time TimeStart;
    [Description("json:\",omitempty\"")]
    public time.Time TimeDone;
    public slice<@string> Cmd; // `json:",omitempty"`
    [Description("json:\",omitempty\"")]
    public time.Duration CmdReal;
    [Description("json:\",omitempty\"")]
    public time.Duration CmdUser;
    [Description("json:\",omitempty\"")]
    public time.Duration CmdSys;
}

// cacheKey is the key for the action cache.
private partial struct cacheKey {
    public @string mode;
    public ptr<load.Package> p;
}

private static @string actionGraphJSON(ptr<Action> _addr_a) {
    ref Action a = ref _addr_a.val;

    slice<ptr<Action>> workq = default;
    var inWorkq = make_map<ptr<Action>, nint>();

    Action<ptr<Action>> add = a => {
        {
            var (_, ok) = inWorkq[a];

            if (ok) {
                return ;
            }

        }
        inWorkq[a] = len(workq);
        workq = append(workq, a);
    };
    add(a);

    for (nint i = 0; i < len(workq); i++) {
        foreach (var (_, dep) in workq[i].Deps) {
            add(dep);
        }
    }

    slice<ptr<actionJSON>> list = default;
    foreach (var (id, a) in workq) {
        if (a.json == null) {
            a.json = addr(new actionJSON(Mode:a.Mode,ID:id,IgnoreFail:a.IgnoreFail,Args:a.Args,Objdir:a.Objdir,Target:a.Target,Failed:a.Failed,Priority:a.priority,Built:a.built,VetxOnly:a.VetxOnly,NeedBuild:a.needBuild,NeedVet:a.needVet,));
            if (a.Package != null) { 
                // TODO(rsc): Make this a unique key for a.Package somehow.
                a.json.Package = a.Package.ImportPath;
            }
            foreach (var (_, a1) in a.Deps) {
                a.json.Deps = append(a.json.Deps, inWorkq[a1]);
            }
        }
        list = append(list, a.json);
    }    var (js, err) = json.MarshalIndent(list, "", "\t");
    if (err != null) {
        fmt.Fprintf(os.Stderr, "go: writing debug action graph: %v\n", err);
        return "";
    }
    return string(js);
}

// BuildMode specifies the build mode:
// are we just building things or also installing the results?
public partial struct BuildMode { // : nint
}

public static readonly BuildMode ModeBuild = iota;
public static readonly var ModeInstall = 0;
public static readonly ModeVetOnly ModeBuggyInstall = 1 << 8;

private static void Init(this ptr<Builder> _addr_b) {
    ref Builder b = ref _addr_b.val;

    b.Print = a => fmt.Fprint(os.Stderr, a);
    b.actionCache = make_map<cacheKey, ptr<Action>>();
    b.mkdirCache = make_map<@string, bool>();
    b.toolIDCache = make_map<@string, @string>();
    b.buildIDCache = make_map<@string, @string>();

    if (cfg.BuildN) {
        b.WorkDir = "$WORK";
    }
    else
 {
        var (tmp, err) = os.MkdirTemp(cfg.Getenv("GOTMPDIR"), "go-build");
        if (err != null) {
            @base.Fatalf("go: creating work dir: %v", err);
        }
        if (!filepath.IsAbs(tmp)) {
            var (abs, err) = filepath.Abs(tmp);
            if (err != null) {
                os.RemoveAll(tmp);
                @base.Fatalf("go: creating work dir: %v", err);
            }
            tmp = abs;
        }
        b.WorkDir = tmp;
        if (cfg.BuildX || cfg.BuildWork) {
            fmt.Fprintf(os.Stderr, "WORK=%s\n", b.WorkDir);
        }
        if (!cfg.BuildWork) {
            var workdir = b.WorkDir;
            @base.AtExit(() => {
                var start = time.Now();
                while (true) {
                    var err = os.RemoveAll(workdir);
                    if (err == null) {
                        return ;
                    } 

                    // On some configurations of Windows, directories containing executable
                    // files may be locked for a while after the executable exits (perhaps
                    // due to antivirus scans?). It's probably worth a little extra latency
                    // on exit to avoid filling up the user's temporary directory with leaked
                    // files. (See golang.org/issue/30789.)
                    if (runtime.GOOS != "windows" || time.Since(start) >= 500 * time.Millisecond) {
                        fmt.Fprintf(os.Stderr, "go: failed to remove work dir: %s\n", err);
                        return ;
                    }
                    time.Sleep(5 * time.Millisecond);
                }
            });
        }
    }
    {
        var err__prev1 = err;

        err = CheckGOOSARCHPair(cfg.Goos, cfg.Goarch);

        if (err != null) {
            fmt.Fprintf(os.Stderr, "cmd/go: %v\n", err);
            @base.SetExitStatus(2);
            @base.Exit();
        }
        err = err__prev1;

    }

    foreach (var (_, tag) in cfg.BuildContext.BuildTags) {
        if (strings.Contains(tag, ",")) {
            fmt.Fprintf(os.Stderr, "cmd/go: -tags space-separated list contains comma\n");
            @base.SetExitStatus(2);
            @base.Exit();
        }
    }
}

public static error CheckGOOSARCHPair(@string goos, @string goarch) {
    {
        var (_, ok) = cfg.OSArchSupportsCgo[goos + "/" + goarch];

        if (!ok && cfg.BuildContext.Compiler == "gc") {
            return error.As(fmt.Errorf("unsupported GOOS/GOARCH pair %s/%s", goos, goarch))!;
        }
    }
    return error.As(null!)!;
}

// NewObjdir returns the name of a fresh object directory under b.WorkDir.
// It is up to the caller to call b.Mkdir on the result at an appropriate time.
// The result ends in a slash, so that file names in that directory
// can be constructed with direct string addition.
//
// NewObjdir must be called only from a single goroutine at a time,
// so it is safe to call during action graph construction, but it must not
// be called during action graph execution.
private static @string NewObjdir(this ptr<Builder> _addr_b) {
    ref Builder b = ref _addr_b.val;

    b.objdirSeq++;
    return filepath.Join(b.WorkDir, fmt.Sprintf("b%03d", b.objdirSeq)) + string(filepath.Separator);
}

// readpkglist returns the list of packages that were built into the shared library
// at shlibpath. For the native toolchain this list is stored, newline separated, in
// an ELF note with name "Go\x00\x00" and type 1. For GCCGO it is extracted from the
// .go_export section.
private static slice<ptr<load.Package>> readpkglist(@string shlibpath) {
    slice<ptr<load.Package>> pkgs = default;

    ref load.ImportStack stk = ref heap(out ptr<load.ImportStack> _addr_stk);
    if (cfg.BuildToolchainName == "gccgo") {
        var (f, _) = elf.Open(shlibpath);
        var sect = f.Section(".go_export");
        var (data, _) = sect.Data();
        var scanner = bufio.NewScanner(bytes.NewBuffer(data));
        while (scanner.Scan()) {
            var t = scanner.Text();
            if (strings.HasPrefix(t, "pkgpath ")) {
                t = strings.TrimPrefix(t, "pkgpath ");
                t = strings.TrimSuffix(t, ";");
                pkgs = append(pkgs, load.LoadImportWithFlags(t, @base.Cwd(), null, _addr_stk, null, 0));
            }
        }
    else
    } {
        var (pkglistbytes, err) = buildid.ReadELFNote(shlibpath, "Go\x00\x00", 1);
        if (err != null) {
            @base.Fatalf("readELFNote failed: %v", err);
        }
        scanner = bufio.NewScanner(bytes.NewBuffer(pkglistbytes));
        while (scanner.Scan()) {
            t = scanner.Text();
            pkgs = append(pkgs, load.LoadImportWithFlags(t, @base.Cwd(), null, _addr_stk, null, 0));
        }
    }
    return ;
}

// cacheAction looks up {mode, p} in the cache and returns the resulting action.
// If the cache has no such action, f() is recorded and returned.
// TODO(rsc): Change the second key from *load.Package to interface{},
// to make the caching in linkShared less awkward?
private static ptr<Action> cacheAction(this ptr<Builder> _addr_b, @string mode, ptr<load.Package> _addr_p, Func<ptr<Action>> f) {
    ref Builder b = ref _addr_b.val;
    ref load.Package p = ref _addr_p.val;

    var a = b.actionCache[new cacheKey(mode,p)];
    if (a == null) {
        a = f();
        b.actionCache[new cacheKey(mode,p)] = a;
    }
    return _addr_a!;
}

// AutoAction returns the "right" action for go build or go install of p.
private static ptr<Action> AutoAction(this ptr<Builder> _addr_b, BuildMode mode, BuildMode depMode, ptr<load.Package> _addr_p) {
    ref Builder b = ref _addr_b.val;
    ref load.Package p = ref _addr_p.val;

    if (p.Name == "main") {
        return _addr_b.LinkAction(mode, depMode, p)!;
    }
    return _addr_b.CompileAction(mode, depMode, p)!;
}

// CompileAction returns the action for compiling and possibly installing
// (according to mode) the given package. The resulting action is only
// for building packages (archives), never for linking executables.
// depMode is the action (build or install) to use when building dependencies.
// To turn package main into an executable, call b.Link instead.
private static ptr<Action> CompileAction(this ptr<Builder> _addr_b, BuildMode mode, BuildMode depMode, ptr<load.Package> _addr_p) => func((_, panic, _) => {
    ref Builder b = ref _addr_b.val;
    ref load.Package p = ref _addr_p.val;

    var vetOnly = mode & ModeVetOnly != 0;
    mode &= ModeVetOnly;

    if (mode != ModeBuild && (p.Internal.Local || p.Module != null) && p.Target == "") { 
        // Imported via local path or using modules. No permanent target.
        mode = ModeBuild;
    }
    if (mode != ModeBuild && p.Name == "main") { 
        // We never install the .a file for a main package.
        mode = ModeBuild;
    }
    a = b.cacheAction("build", p, () => {
        ptr<Action> a = addr(new Action(Mode:"build",Package:p,Func:(*Builder).build,Objdir:b.NewObjdir(),));

        if (p.Error == null || !p.Error.IsImportCycle) {
            foreach (var (_, p1) in p.Internal.Imports) {
                a.Deps = append(a.Deps, b.CompileAction(depMode, depMode, p1));
            }
        }
        if (p.Standard) {
            switch (p.ImportPath) {
                case "builtin": 
                    // Fake packages - nothing to build.

                case "unsafe": 
                    // Fake packages - nothing to build.
                    a.Mode = "built-in package";
                    a.Func = null;
                    return _addr_a!;
                    break;
            } 

            // gccgo standard library is "fake" too.
            if (cfg.BuildToolchainName == "gccgo") { 
                // the target name is needed for cgo.
                a.Mode = "gccgo stdlib";
                a.Target = p.Target;
                a.Func = null;
                return _addr_a!;
            }
        }
        return _addr_a!;
    }); 

    // Find the build action; the cache entry may have been replaced
    // by the install action during (*Builder).installAction.
    var buildAction = a;
    switch (buildAction.Mode) {
        case "build": 

        case "built-in package": 

        case "gccgo stdlib": 

            break;
        case "build-install": 
            buildAction = a.Deps[0];
            break;
        default: 
            panic("lost build action: " + buildAction.Mode);
            break;
    }
    buildAction.needBuild = buildAction.needBuild || !vetOnly; 

    // Construct install action.
    if (mode == ModeInstall || mode == ModeBuggyInstall) {
        a = b.installAction(a, mode);
    }
    return _addr_a!;
});

// VetAction returns the action for running go vet on package p.
// It depends on the action for compiling p.
// If the caller may be causing p to be installed, it is up to the caller
// to make sure that the install depends on (runs after) vet.
private static ptr<Action> VetAction(this ptr<Builder> _addr_b, BuildMode mode, BuildMode depMode, ptr<load.Package> _addr_p) {
    ref Builder b = ref _addr_b.val;
    ref load.Package p = ref _addr_p.val;

    var a = b.vetAction(mode, depMode, p);
    a.VetxOnly = false;
    return _addr_a!;
}

private static ptr<Action> vetAction(this ptr<Builder> _addr_b, BuildMode mode, BuildMode depMode, ptr<load.Package> _addr_p) {
    ref Builder b = ref _addr_b.val;
    ref load.Package p = ref _addr_p.val;
 
    // Construct vet action.
    a = b.cacheAction("vet", p, () => {
        var a1 = b.CompileAction(mode | ModeVetOnly, depMode, p); 

        // vet expects to be able to import "fmt".
        ref load.ImportStack stk = ref heap(out ptr<load.ImportStack> _addr_stk);
        stk.Push("vet");
        var p1 = load.LoadImportWithFlags("fmt", p.Dir, p, _addr_stk, null, 0);
        stk.Pop();
        var aFmt = b.CompileAction(ModeBuild, depMode, p1);

        slice<ptr<Action>> deps = default;
        if (a1.buggyInstall) { 
            // (*Builder).vet expects deps[0] to be the package
            // and deps[1] to be "fmt". If we see buggyInstall
            // here then a1 is an install of a shared library,
            // and the real package is a1.Deps[0].
            deps = new slice<ptr<Action>>(new ptr<Action>[] { a1.Deps[0], aFmt, a1 });
        }
        else
 {
            deps = new slice<ptr<Action>>(new ptr<Action>[] { a1, aFmt });
        }
        {
            var p1__prev1 = p1;

            foreach (var (_, __p1) in p.Internal.Imports) {
                p1 = __p1;
                deps = append(deps, b.vetAction(mode, depMode, p1));
            }

            p1 = p1__prev1;
        }

        ptr<Action> a = addr(new Action(Mode:"vet",Package:p,Deps:deps,Objdir:a1.Objdir,VetxOnly:true,IgnoreFail:true,));
        if (a1.Func == null) { 
            // Built-in packages like unsafe.
            return _addr_a!;
        }
        deps[0].needVet = true;
        a.Func = (Builder.val).vet;
        return _addr_a!;
    });
    return _addr_a!;
}

// LinkAction returns the action for linking p into an executable
// and possibly installing the result (according to mode).
// depMode is the action (build or install) to use when compiling dependencies.
private static ptr<Action> LinkAction(this ptr<Builder> _addr_b, BuildMode mode, BuildMode depMode, ptr<load.Package> _addr_p) {
    ref Builder b = ref _addr_b.val;
    ref load.Package p = ref _addr_p.val;
 
    // Construct link action.
    a = b.cacheAction("link", p, () => {
        ptr<Action> a = addr(new Action(Mode:"link",Package:p,));

        var a1 = b.CompileAction(ModeBuild, depMode, p);
        a.Func = (Builder.val).link;
        a.Deps = new slice<ptr<Action>>(new ptr<Action>[] { a1 });
        a.Objdir = a1.Objdir; 

        // An executable file. (This is the name of a temporary file.)
        // Because we run the temporary file in 'go run' and 'go test',
        // the name will show up in ps listings. If the caller has specified
        // a name, use that instead of a.out. The binary is generated
        // in an otherwise empty subdirectory named exe to avoid
        // naming conflicts. The only possible conflict is if we were
        // to create a top-level package named exe.
        @string name = "a.out";
        if (p.Internal.ExeName != "") {
            name = p.Internal.ExeName;
        }
        else if ((cfg.Goos == "darwin" || cfg.Goos == "windows") && cfg.BuildBuildmode == "c-shared" && p.Target != "") { 
            // On OS X, the linker output name gets recorded in the
            // shared library's LC_ID_DYLIB load command.
            // The code invoking the linker knows to pass only the final
            // path element. Arrange that the path element matches what
            // we'll install it as; otherwise the library is only loadable as "a.out".
            // On Windows, DLL file name is recorded in PE file
            // export section, so do like on OS X.
            _, name = filepath.Split(p.Target);
        }
        a.Target = a.Objdir + filepath.Join("exe", name) + cfg.ExeSuffix;
        a.built = a.Target;
        b.addTransitiveLinkDeps(a, a1, ""); 

        // Sequence the build of the main package (a1) strictly after the build
        // of all other dependencies that go into the link. It is likely to be after
        // them anyway, but just make sure. This is required by the build ID-based
        // shortcut in (*Builder).useCache(a1), which will call b.linkActionID(a).
        // In order for that linkActionID call to compute the right action ID, all the
        // dependencies of a (except a1) must have completed building and have
        // recorded their build IDs.
        a1.Deps = append(a1.Deps, addr(new Action(Mode:"nop",Deps:a.Deps[1:])));
        return _addr_a!;
    });

    if (mode == ModeInstall || mode == ModeBuggyInstall) {
        a = b.installAction(a, mode);
    }
    return _addr_a!;
}

// installAction returns the action for installing the result of a1.
private static ptr<Action> installAction(this ptr<Builder> _addr_b, ptr<Action> _addr_a1, BuildMode mode) {
    ref Builder b = ref _addr_b.val;
    ref Action a1 = ref _addr_a1.val;
 
    // Because we overwrite the build action with the install action below,
    // a1 may already be an install action fetched from the "build" cache key,
    // and the caller just doesn't realize.
    if (strings.HasSuffix(a1.Mode, "-install")) {
        if (a1.buggyInstall && mode == ModeInstall) { 
            //  Congratulations! The buggy install is now a proper install.
            a1.buggyInstall = false;
        }
        return _addr_a1!;
    }
    if (a1.Func == null) {
        return _addr_a1!;
    }
    var p = a1.Package;
    return _addr_b.cacheAction(a1.Mode + "-install", p, () => { 
        // The install deletes the temporary build result,
        // so we need all other actions, both past and future,
        // that attempt to depend on the build to depend instead
        // on the install.

        // Make a private copy of a1 (the build action),
        // no longer accessible to any other rules.
        ptr<Action> buildAction = @new<Action>();
        buildAction.val = a1; 

        // Overwrite a1 with the install action.
        // This takes care of updating past actions that
        // point at a1 for the build action; now they will
        // point at a1 and get the install action.
        // We also leave a1 in the action cache as the result
        // for "build", so that actions not yet created that
        // try to depend on the build will instead depend
        // on the install.
        a1 = new Action(Mode:buildAction.Mode+"-install",Func:BuildInstallFunc,Package:p,Objdir:buildAction.Objdir,Deps:[]*Action{buildAction},Target:p.Target,built:p.Target,buggyInstall:mode==ModeBuggyInstall,);

        b.addInstallHeaderAction(a1);
        return _addr_a1!;
    })!;
}

// addTransitiveLinkDeps adds to the link action a all packages
// that are transitive dependencies of a1.Deps.
// That is, if a is a link of package main, a1 is the compile of package main
// and a1.Deps is the actions for building packages directly imported by
// package main (what the compiler needs). The linker needs all packages
// transitively imported by the whole program; addTransitiveLinkDeps
// makes sure those are present in a.Deps.
// If shlib is non-empty, then a corresponds to the build and installation of shlib,
// so any rebuild of shlib should not be added as a dependency.
private static void addTransitiveLinkDeps(this ptr<Builder> _addr_b, ptr<Action> _addr_a, ptr<Action> _addr_a1, @string shlib) {
    ref Builder b = ref _addr_b.val;
    ref Action a = ref _addr_a.val;
    ref Action a1 = ref _addr_a1.val;
 
    // Expand Deps to include all built packages, for the linker.
    // Use breadth-first search to find rebuilt-for-test packages
    // before the standard ones.
    // TODO(rsc): Eliminate the standard ones from the action graph,
    // which will require doing a little bit more rebuilding.
    ptr<Action> workq = new slice<ptr<Action>>(new ptr<Action>[] { a1 });
    map haveDep = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    if (a1.Package != null) {
        haveDep[a1.Package.ImportPath] = true;
    }
    for (nint i = 0; i < len(workq); i++) {
        var a1 = workq[i];
        foreach (var (_, a2) in a1.Deps) { 
            // TODO(rsc): Find a better discriminator than the Mode strings, once the dust settles.
            if (a2.Package == null || (a2.Mode != "build-install" && a2.Mode != "build") || haveDep[a2.Package.ImportPath]) {
                continue;
            }
            haveDep[a2.Package.ImportPath] = true;
            a.Deps = append(a.Deps, a2);
            if (a2.Mode == "build-install") {
                a2 = a2.Deps[0]; // walk children of "build" action
            }
            workq = append(workq, a2);
        }
    } 

    // If this is go build -linkshared, then the link depends on the shared libraries
    // in addition to the packages themselves. (The compile steps do not.)
    if (cfg.BuildLinkshared) {
        map haveShlib = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{shlib:true};
        {
            var a1__prev1 = a1;

            foreach (var (_, __a1) in a.Deps) {
                a1 = __a1;
                var p1 = a1.Package;
                if (p1 == null || p1.Shlib == "" || haveShlib[filepath.Base(p1.Shlib)]) {
                    continue;
                }
                haveShlib[filepath.Base(p1.Shlib)] = true; 
                // TODO(rsc): The use of ModeInstall here is suspect, but if we only do ModeBuild,
                // we'll end up building an overall library or executable that depends at runtime
                // on other libraries that are out-of-date, which is clearly not good either.
                // We call it ModeBuggyInstall to make clear that this is not right.
                a.Deps = append(a.Deps, b.linkSharedAction(ModeBuggyInstall, ModeBuggyInstall, p1.Shlib, null));
            }

            a1 = a1__prev1;
        }
    }
}

// addInstallHeaderAction adds an install header action to a, if needed.
// The action a should be an install action as generated by either
// b.CompileAction or b.LinkAction with mode=ModeInstall,
// and so a.Deps[0] is the corresponding build action.
private static void addInstallHeaderAction(this ptr<Builder> _addr_b, ptr<Action> _addr_a) {
    ref Builder b = ref _addr_b.val;
    ref Action a = ref _addr_a.val;
 
    // Install header for cgo in c-archive and c-shared modes.
    var p = a.Package;
    if (p.UsesCgo() && (cfg.BuildBuildmode == "c-archive" || cfg.BuildBuildmode == "c-shared")) {
        var hdrTarget = a.Target[..(int)len(a.Target) - len(filepath.Ext(a.Target))] + ".h";
        if (cfg.BuildContext.Compiler == "gccgo" && cfg.BuildO == "") { 
            // For the header file, remove the "lib"
            // added by go/build, so we generate pkg.h
            // rather than libpkg.h.
            var (dir, file) = filepath.Split(hdrTarget);
            file = strings.TrimPrefix(file, "lib");
            hdrTarget = filepath.Join(dir, file);
        }
        ptr<Action> ah = addr(new Action(Mode:"install header",Package:a.Package,Deps:[]*Action{a.Deps[0]},Func:(*Builder).installHeader,Objdir:a.Deps[0].Objdir,Target:hdrTarget,));
        a.Deps = append(a.Deps, ah);
    }
}

// buildmodeShared takes the "go build" action a1 into the building of a shared library of a1.Deps.
// That is, the input a1 represents "go build pkgs" and the result represents "go build -buildmode=shared pkgs".
private static ptr<Action> buildmodeShared(this ptr<Builder> _addr_b, BuildMode mode, BuildMode depMode, slice<@string> args, slice<ptr<load.Package>> pkgs, ptr<Action> _addr_a1) {
    ref Builder b = ref _addr_b.val;
    ref Action a1 = ref _addr_a1.val;

    var (name, err) = libname(args, pkgs);
    if (err != null) {
        @base.Fatalf("%v", err);
    }
    return _addr_b.linkSharedAction(mode, depMode, name, a1)!;
}

// linkSharedAction takes a grouping action a1 corresponding to a list of built packages
// and returns an action that links them together into a shared library with the name shlib.
// If a1 is nil, shlib should be an absolute path to an existing shared library,
// and then linkSharedAction reads that library to find out the package list.
private static ptr<Action> linkSharedAction(this ptr<Builder> _addr_b, BuildMode mode, BuildMode depMode, @string shlib, ptr<Action> _addr_a1) {
    ref Builder b = ref _addr_b.val;
    ref Action a1 = ref _addr_a1.val;

    var fullShlib = shlib;
    shlib = filepath.Base(shlib);
    a = b.cacheAction("build-shlib " + shlib, null, () => {
        if (a1 == null) { 
            // TODO(rsc): Need to find some other place to store config,
            // not in pkg directory. See golang.org/issue/22196.
            var pkgs = readpkglist(fullShlib);
            a1 = addr(new Action(Mode:"shlib packages",));
            {
                var p__prev1 = p;

                foreach (var (_, __p) in pkgs) {
                    p = __p;
                    a1.Deps = append(a1.Deps, b.CompileAction(mode, depMode, p));
                }

                p = p__prev1;
            }
        }
        ptr<load.Package> p = addr(new load.Package());
        p.Internal.CmdlinePkg = true;
        p.Internal.Ldflags = load.BuildLdflags.For(p);
        p.Internal.Gccgoflags = load.BuildGccgoflags.For(p); 

        // Add implicit dependencies to pkgs list.
        // Currently buildmode=shared forces external linking mode, and
        // external linking mode forces an import of runtime/cgo (and
        // math on arm). So if it was not passed on the command line and
        // it is not present in another shared library, add it here.
        // TODO(rsc): Maybe this should only happen if "runtime" is in the original package set.
        // TODO(rsc): This should probably be changed to use load.LinkerDeps(p).
        // TODO(rsc): We don't add standard library imports for gccgo
        // because they are all always linked in anyhow.
        // Maybe load.LinkerDeps should be used and updated.
        ptr<Action> a = addr(new Action(Mode:"go build -buildmode=shared",Package:p,Objdir:b.NewObjdir(),Func:(*Builder).linkShared,Deps:[]*Action{a1},));
        a.Target = filepath.Join(a.Objdir, shlib);
        if (cfg.BuildToolchainName != "gccgo") {
            Action<ptr<Action>, @string, bool> add = (a1, pkg, force) => {
                {
                    var a2__prev1 = a2;

                    foreach (var (_, __a2) in a1.Deps) {
                        a2 = __a2;
                        if (a2.Package != null && a2.Package.ImportPath == pkg) {
                            return ;
                        }
                    }

                    a2 = a2__prev1;
                }

                ref load.ImportStack stk = ref heap(out ptr<load.ImportStack> _addr_stk);
                p = load.LoadImportWithFlags(pkg, @base.Cwd(), null, _addr_stk, null, 0);
                if (p.Error != null) {
                    @base.Fatalf("load %s: %v", pkg, p.Error);
                } 
                // Assume that if pkg (runtime/cgo or math)
                // is already accounted for in a different shared library,
                // then that shared library also contains runtime,
                // so that anything we do will depend on that library,
                // so we don't need to include pkg in our shared library.
                if (force || p.Shlib == "" || filepath.Base(p.Shlib) == pkg) {
                    a1.Deps = append(a1.Deps, b.CompileAction(depMode, depMode, p));
                }
            }
;
            add(a1, "runtime/cgo", false);
            if (cfg.Goarch == "arm") {
                add(a1, "math", false);
            } 

            // The linker step still needs all the usual linker deps.
            // (For example, the linker always opens runtime.a.)
            foreach (var (_, dep) in load.LinkerDeps(null)) {
                add(a, dep, true);
            }
        }
        b.addTransitiveLinkDeps(a, a1, shlib);
        return _addr_a!;
    }); 

    // Install result.
    if ((mode == ModeInstall || mode == ModeBuggyInstall) && a.Func != null) {
        var buildAction = a;

        a = b.cacheAction("install-shlib " + shlib, null, () => { 
            // Determine the eventual install target.
            // The install target is root/pkg/shlib, where root is the source root
            // in which all the packages lie.
            // TODO(rsc): Perhaps this cross-root check should apply to the full
            // transitive package dependency list, not just the ones named
            // on the command line?
            var pkgDir = a1.Deps[0].Package.Internal.Build.PkgTargetRoot;
            {
                var a2__prev1 = a2;

                foreach (var (_, __a2) in a1.Deps) {
                    a2 = __a2;
                    {
                        var dir = a2.Package.Internal.Build.PkgTargetRoot;

                        if (dir != pkgDir) {
                            @base.Fatalf("installing shared library: cannot use packages %s and %s from different roots %s and %s", a1.Deps[0].Package.ImportPath, a2.Package.ImportPath, pkgDir, dir);
                        }

                    }
                } 
                // TODO(rsc): Find out and explain here why gccgo is different.

                a2 = a2__prev1;
            }

            if (cfg.BuildToolchainName == "gccgo") {
                pkgDir = filepath.Join(pkgDir, "shlibs");
            }
            var target = filepath.Join(pkgDir, shlib);

            a = addr(new Action(Mode:"go install -buildmode=shared",Objdir:buildAction.Objdir,Func:BuildInstallFunc,Deps:[]*Action{buildAction},Target:target,));
            {
                var a2__prev1 = a2;

                foreach (var (_, __a2) in buildAction.Deps[0].Deps) {
                    a2 = __a2;
                    p = a2.Package;
                    if (p.Target == "") {
                        continue;
                    }
                    a.Deps = append(a.Deps, addr(new Action(Mode:"shlibname",Package:p,Func:(*Builder).installShlibname,Target:strings.TrimSuffix(p.Target,".a")+".shlibname",Deps:[]*Action{a.Deps[0]},)));
                }

                a2 = a2__prev1;
            }

            return _addr_a!;
        });
    }
    return _addr_a!;
}

} // end work_package
