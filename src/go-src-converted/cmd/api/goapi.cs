// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Binary api computes the exported API of a set of Go packages.
// package main -- go2cs converted at 2022 March 06 22:41:08 UTC
// Original source: C:\Program Files\Go\src\cmd\api\goapi.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using types = go.go.types_package;
using exec = go.@internal.execabs_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

private static @string goCmd() {
    @string exeSuffix = default;
    if (runtime.GOOS == "windows") {
        exeSuffix = ".exe";
    }
    var path = filepath.Join(runtime.GOROOT(), "bin", "go" + exeSuffix);
    {
        var (_, err) = os.Stat(path);

        if (err == null) {
            return path;
        }
    }

    return "go";

}

// Flags
private static var checkFile = flag.String("c", "", "optional comma-separated filename(s) to check API against");private static var allowNew = flag.Bool("allow_new", true, "allow API additions");private static var exceptFile = flag.String("except", "", "optional filename of packages that are allowed to change without triggering a failure in the tool");private static var nextFile = flag.String("next", "", "optional filename of tentative upcoming API features for the next release. This file can be lazily maintained. It only affects the delta warnings from the -c file printed on success.");private static var verbose = flag.Bool("v", false, "verbose debugging");private static var forceCtx = flag.String("contexts", "", "optional comma-separated list of <goos>-<goarch>[-cgo] to override default contexts.");

// contexts are the default contexts which are scanned, unless
// overridden by the -contexts flag.
private static ptr<build.Context> contexts = new slice<ptr<build.Context>>(new ptr<build.Context>[] { {GOOS:"linux",GOARCH:"386",CgoEnabled:true}, {GOOS:"linux",GOARCH:"386"}, {GOOS:"linux",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"linux",GOARCH:"amd64"}, {GOOS:"linux",GOARCH:"arm",CgoEnabled:true}, {GOOS:"linux",GOARCH:"arm"}, {GOOS:"darwin",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"darwin",GOARCH:"amd64"}, {GOOS:"windows",GOARCH:"amd64"}, {GOOS:"windows",GOARCH:"386"}, {GOOS:"freebsd",GOARCH:"386",CgoEnabled:true}, {GOOS:"freebsd",GOARCH:"386"}, {GOOS:"freebsd",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"freebsd",GOARCH:"amd64"}, {GOOS:"freebsd",GOARCH:"arm",CgoEnabled:true}, {GOOS:"freebsd",GOARCH:"arm"}, {GOOS:"netbsd",GOARCH:"386",CgoEnabled:true}, {GOOS:"netbsd",GOARCH:"386"}, {GOOS:"netbsd",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"netbsd",GOARCH:"amd64"}, {GOOS:"netbsd",GOARCH:"arm",CgoEnabled:true}, {GOOS:"netbsd",GOARCH:"arm"}, {GOOS:"netbsd",GOARCH:"arm64",CgoEnabled:true}, {GOOS:"netbsd",GOARCH:"arm64"}, {GOOS:"openbsd",GOARCH:"386",CgoEnabled:true}, {GOOS:"openbsd",GOARCH:"386"}, {GOOS:"openbsd",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"openbsd",GOARCH:"amd64"} });

private static @string contextName(ptr<build.Context> _addr_c) {
    ref build.Context c = ref _addr_c.val;

    var s = c.GOOS + "-" + c.GOARCH;
    if (c.CgoEnabled) {
        s += "-cgo";
    }
    if (c.Dir != "") {
        s += fmt.Sprintf(" [%s]", c.Dir);
    }
    return s;

}

private static ptr<build.Context> parseContext(@string c) {
    var parts = strings.Split(c, "-");
    if (len(parts) < 2) {
        log.Fatalf("bad context: %q", c);
    }
    ptr<build.Context> bc = addr(new build.Context(GOOS:parts[0],GOARCH:parts[1],));
    if (len(parts) == 3) {
        if (parts[2] == "cgo") {
            bc.CgoEnabled = true;
        }
        else
 {
            log.Fatalf("bad context: %q", c);
        }
    }
    return _addr_bc!;

}

private static void setContexts() {
    contexts = new slice<ptr<build.Context>>(new ptr<build.Context>[] {  });
    foreach (var (_, c) in strings.Split(forceCtx.val, ",")) {
        contexts = append(contexts, parseContext(c));
    }
}

private static var internalPkg = regexp.MustCompile("(^|/)internal($|/)");

private static void Main() => func((defer, _, _) => {
    flag.Parse();

    if (!strings.Contains(runtime.Version(), "weekly") && !strings.Contains(runtime.Version(), "devel")) {
        if (nextFile != "".val) {
            fmt.Printf("Go version is %q, ignoring -next %s\n", runtime.Version(), nextFile.val);
            nextFile.val = "";
        }
    }
    if (forceCtx != "".val) {
        setContexts();
    }
    foreach (var (_, c) in contexts) {
        c.Compiler = build.Default.Compiler;
    }    var walkers = make_slice<ptr<Walker>>(len(contexts));
    sync.WaitGroup wg = default;
    {
        var i__prev1 = i;
        var context__prev1 = context;

        foreach (var (__i, __context) in contexts) {
            i = __i;
            context = __context;
            var i = i;
            var context = context;
            wg.Add(1);
            go_(() => () => {
                defer(wg.Done());
                walkers[i] = NewWalker(_addr_context, filepath.Join(build.Default.GOROOT, "src"));
            }());

        }
        i = i__prev1;
        context = context__prev1;
    }

    wg.Wait();

    var featureCtx = make_map<@string, map<@string, bool>>(); // feature -> context name -> true
    foreach (var (_, w) in walkers) {
        var pkgNames = w.stdPackages;
        if (flag.NArg() > 0) {
            pkgNames = flag.Args();
        }
        foreach (var (_, name) in pkgNames) {
            var (pkg, err) = w.Import(name);
            {
                ptr<build.NoGoError> (_, nogo) = err._<ptr<build.NoGoError>>();

                if (nogo) {
                    continue;
                }

            }

            if (err != null) {
                log.Fatalf("Import(%q): %v", name, err);
            }

            w.export(pkg);

        }        var ctxName = contextName(_addr_w.context);
        {
            var f__prev2 = f;

            foreach (var (_, __f) in w.Features()) {
                f = __f;
                if (featureCtx[f] == null) {
                    featureCtx[f] = make_map<@string, bool>();
                }
                featureCtx[f][ctxName] = true;
            }

            f = f__prev2;
        }
    }    slice<@string> features = default;
    {
        var f__prev1 = f;

        foreach (var (__f, __cmap) in featureCtx) {
            f = __f;
            cmap = __cmap;
            if (len(cmap) == len(contexts)) {
                features = append(features, f);
                continue;
            }
            var comma = strings.Index(f, ",");
            foreach (var (cname) in cmap) {
                var f2 = fmt.Sprintf("%s (%s)%s", f[..(int)comma], cname, f[(int)comma..]);
                features = append(features, f2);
            }
        }
        f = f__prev1;
    }

    var fail = false;
    defer(() => {
        if (fail) {
            os.Exit(1);
        }
    }());

    var bw = bufio.NewWriter(os.Stdout);
    defer(bw.Flush());

    if (checkFile == "".val) {
        sort.Strings(features);
        {
            var f__prev1 = f;

            foreach (var (_, __f) in features) {
                f = __f;
                fmt.Fprintln(bw, f);
            }

            f = f__prev1;
        }

        return ;

    }
    slice<@string> required = default;
    foreach (var (_, file) in strings.Split(checkFile.val, ",")) {
        required = append(required, fileFeatures(file));
    }    var optional = fileFeatures(nextFile.val);
    var exception = fileFeatures(exceptFile.val);
    fail = !compareAPI(bw, features, required, optional, exception, allowNew.val);

});

// export emits the exported package features.
private static void export(this ptr<Walker> _addr_w, ptr<types.Package> _addr_pkg) {
    ref Walker w = ref _addr_w.val;
    ref types.Package pkg = ref _addr_pkg.val;

    if (verbose.val) {
        log.Println(pkg);
    }
    var pop = w.pushScope("pkg " + pkg.Path());
    w.current = pkg;
    var scope = pkg.Scope();
    foreach (var (_, name) in scope.Names()) {
        if (token.IsExported(name)) {
            w.emitObj(scope.Lookup(name));
        }
    }    pop();

}

private static map<@string, bool> set(slice<@string> items) {
    var s = make_map<@string, bool>();
    foreach (var (_, v) in items) {
        s[v] = true;
    }    return s;
}

private static var spaceParensRx = regexp.MustCompile(" \\(\\S+?\\)");

private static @string featureWithoutContext(@string f) {
    if (!strings.Contains(f, "(")) {
        return f;
    }
    return spaceParensRx.ReplaceAllString(f, "");

}

// portRemoved reports whether the given port-specific API feature is
// okay to no longer exist because its port was removed.
private static bool portRemoved(@string feature) {
    return strings.Contains(feature, "(darwin-386)") || strings.Contains(feature, "(darwin-386-cgo)");
}

private static bool compareAPI(io.Writer w, slice<@string> features, slice<@string> required, slice<@string> optional, slice<@string> exception, bool allowAdd) {
    bool ok = default;

    ok = true;

    var optionalSet = set(optional);
    var exceptionSet = set(exception);
    var featureSet = set(features);

    sort.Strings(features);
    sort.Strings(required);

    Func<ptr<slice<@string>>, @string> take = sl => {
        var s = (sl.val)[0];
        sl.val = (sl.val)[(int)1..];
        return s;
    };

    while (len(required) > 0 || len(features) > 0) {

        if (len(features) == 0 || (len(required) > 0 && required[0] < features[0])) 
            var feature = take(_addr_required);
            if (exceptionSet[feature]) { 
                // An "unfortunate" case: the feature was once
                // included in the API (e.g. go1.txt), but was
                // subsequently removed. These are already
                // acknowledged by being in the file
                // "api/except.txt". No need to print them out
                // here.
            }
            else if (portRemoved(feature)) { 
                // okay.
            }
            else if (featureSet[featureWithoutContext(feature)]) { 
                // okay.
            }
            else
 {
                fmt.Fprintf(w, "-%s\n", feature);
                ok = false; // broke compatibility
            }

        else if (len(required) == 0 || (len(features) > 0 && required[0] > features[0])) 
            var newFeature = take(_addr_features);
            if (optionalSet[newFeature]) { 
                // Known added feature to the upcoming release.
                // Delete it from the map so we can detect any upcoming features
                // which were never seen.  (so we can clean up the nextFile)
                delete(optionalSet, newFeature);

            }
            else
 {
                fmt.Fprintf(w, "+%s\n", newFeature);
                if (!allowAdd) {
                    ok = false; // we're in lock-down mode for next release
                }

            }

        else 
            take(_addr_required);
            take(_addr_features);
        
    } 

    // In next file, but not in API.
    slice<@string> missing = default;
    {
        var feature__prev1 = feature;

        foreach (var (__feature) in optionalSet) {
            feature = __feature;
            missing = append(missing, feature);
        }
        feature = feature__prev1;
    }

    sort.Strings(missing);
    {
        var feature__prev1 = feature;

        foreach (var (_, __feature) in missing) {
            feature = __feature;
            fmt.Fprintf(w, "±%s\n", feature);
        }
        feature = feature__prev1;
    }

    return ;

}

// aliasReplacer applies type aliases to earlier API files,
// to avoid misleading negative results.
// This makes all the references to os.FileInfo in go1.txt
// be read as if they said fs.FileInfo, since os.FileInfo is now an alias.
// If there are many of these, we could do a more general solution,
// but for now the replacer is fine.
private static var aliasReplacer = strings.NewReplacer("os.FileInfo", "fs.FileInfo", "os.FileMode", "fs.FileMode", "os.PathError", "fs.PathError");

private static slice<@string> fileFeatures(@string filename) {
    if (filename == "") {
        return null;
    }
    var (bs, err) = os.ReadFile(filename);
    if (err != null) {
        log.Fatalf("Error reading file %s: %v", filename, err);
    }
    var s = string(bs);
    s = aliasReplacer.Replace(s);
    var lines = strings.Split(s, "\n");
    slice<@string> nonblank = default;
    foreach (var (_, line) in lines) {
        line = strings.TrimSpace(line);
        if (line != "" && !strings.HasPrefix(line, "#")) {
            nonblank = append(nonblank, line);
        }
    }    return nonblank;

}

private static var fset = token.NewFileSet();

public partial struct Walker {
    public ptr<build.Context> context;
    public @string root;
    public slice<@string> scope;
    public ptr<types.Package> current;
    public map<@string, bool> features; // set
    public map<@string, ptr<types.Package>> imported; // packages already imported
    public slice<@string> stdPackages; // names, omitting "unsafe", internal, and vendored packages
    public map<@string, map<@string, @string>> importMap; // importer dir -> import path -> canonical path
    public map<@string, @string> importDir; // canonical import path -> dir

}

public static ptr<Walker> NewWalker(ptr<build.Context> _addr_context, @string root) {
    ref build.Context context = ref _addr_context.val;

    ptr<Walker> w = addr(new Walker(context:context,root:root,features:map[string]bool{},imported:map[string]*types.Package{"unsafe":types.Unsafe},));
    w.loadImports();
    return _addr_w!;
}

private static slice<@string> Features(this ptr<Walker> _addr_w) {
    slice<@string> fs = default;
    ref Walker w = ref _addr_w.val;

    foreach (var (f) in w.features) {
        fs = append(fs, f);
    }    sort.Strings(fs);
    return ;
}

private static var parsedFileCache = make_map<@string, ptr<ast.File>>();

private static (ptr<ast.File>, error) parseFile(this ptr<Walker> _addr_w, @string dir, @string file) {
    ptr<ast.File> _p0 = default!;
    error _p0 = default!;
    ref Walker w = ref _addr_w.val;

    var filename = filepath.Join(dir, file);
    {
        var f__prev1 = f;

        var f = parsedFileCache[filename];

        if (f != null) {
            return (_addr_f!, error.As(null!)!);
        }
        f = f__prev1;

    }


    var (f, err) = parser.ParseFile(fset, filename, null, 0);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    parsedFileCache[filename] = f;

    return (_addr_f!, error.As(null!)!);

}

// Disable before debugging non-obvious errors from the type-checker.
private static readonly var usePkgCache = true;



private static map pkgCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<types.Package>>{};private static map pkgTags = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<@string>>{};

// tagKey returns the tag-based key to use in the pkgCache.
// It is a comma-separated string; the first part is dir, the rest tags.
// The satisfied tags are derived from context but only those that
// matter (the ones listed in the tags argument plus GOOS and GOARCH) are used.
// The tags list, which came from go/build's Package.AllTags,
// is known to be sorted.
private static @string tagKey(@string dir, ptr<build.Context> _addr_context, slice<@string> tags) {
    ref build.Context context = ref _addr_context.val;

    map ctags = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{context.GOOS:true,context.GOARCH:true,};
    if (context.CgoEnabled) {
        ctags["cgo"] = true;
    }
    {
        var tag__prev1 = tag;

        foreach (var (_, __tag) in context.BuildTags) {
            tag = __tag;
            ctags[tag] = true;
        }
        tag = tag__prev1;
    }

    var key = dir; 

    // explicit on GOOS and GOARCH as global cache will use "all" cached packages for
    // an indirect imported package. See https://github.com/golang/go/issues/21181
    // for more detail.
    tags = append(tags, context.GOOS, context.GOARCH);
    sort.Strings(tags);

    {
        var tag__prev1 = tag;

        foreach (var (_, __tag) in tags) {
            tag = __tag;
            if (ctags[tag]) {
                key += "," + tag;
                ctags[tag] = false;
            }
        }
        tag = tag__prev1;
    }

    return key;

}

private partial struct listImports {
    public slice<@string> stdPackages; // names, omitting "unsafe", internal, and vendored packages
    public map<@string, @string> importDir; // canonical import path → directory
    public map<@string, map<@string, @string>> importMap; // import path → canonical import path
}

private static sync.Map listCache = default; // map[string]listImports, keyed by contextName

// listSem is a semaphore restricting concurrent invocations of 'go list'.
private static var listSem = make_channel<semToken>(runtime.GOMAXPROCS(0));

private partial struct semToken {
}

// loadImports populates w with information about the packages in the standard
// library and the packages they themselves import in w's build context.
//
// The source import path and expanded import path are identical except for vendored packages.
// For example, on return:
//
//    w.importMap["math"] = "math"
//    w.importDir["math"] = "<goroot>/src/math"
//
//    w.importMap["golang.org/x/net/route"] = "vendor/golang.org/x/net/route"
//    w.importDir["vendor/golang.org/x/net/route"] = "<goroot>/src/vendor/golang.org/x/net/route"
//
// Since the set of packages that exist depends on context, the result of
// loadImports also depends on context. However, to improve test running time
// the configuration for each environment is cached across runs.
private static void loadImports(this ptr<Walker> _addr_w) => func((defer, _, _) => {
    ref Walker w = ref _addr_w.val;

    if (w.context == null) {
        return ; // test-only Walker; does not use the import map
    }
    var name = contextName(_addr_w.context);

    var (imports, ok) = listCache.Load(name);
    if (!ok) {
        listSem.Send(new semToken());
        defer(() => {
            listSem.Receive();
        }());

        var cmd = exec.Command(goCmd(), "list", "-e", "-deps", "-json", "std");
        cmd.Env = listEnv(_addr_w.context);
        if (w.context.Dir != "") {
            cmd.Dir = w.context.Dir;
        }
        var (out, err) = cmd.CombinedOutput();
        if (err != null) {
            log.Fatalf("loading imports: %v\n%s", err, out);
        }
        slice<@string> stdPackages = default;
        var importMap = make_map<@string, map<@string, @string>>();
        var importDir = make_map<@string, @string>();
        var dec = json.NewDecoder(bytes.NewReader(out));
        while (true) {
            ref var pkg = ref heap(out ptr<var> _addr_pkg);
            var err = dec.Decode(_addr_pkg);
            if (err == io.EOF) {
                break;
            }
            if (err != null) {
                log.Fatalf("go list: invalid output: %v", err);
            } 

            // - Package "unsafe" contains special signatures requiring
            //   extra care when printing them - ignore since it is not
            //   going to change w/o a language change.
            // - Internal and vendored packages do not contribute to our
            //   API surface. (If we are running within the "std" module,
            //   vendored dependencies appear as themselves instead of
            //   their "vendor/" standard-library copies.)
            // - 'go list std' does not include commands, which cannot be
            //   imported anyway.
            {
                var ip = pkg.ImportPath;

                if (pkg.Standard && ip != "unsafe" && !strings.HasPrefix(ip, "vendor/") && !internalPkg.MatchString(ip)) {
                    stdPackages = append(stdPackages, ip);
                }

            }

            importDir[pkg.ImportPath] = pkg.Dir;
            if (len(pkg.ImportMap) > 0) {
                importMap[pkg.Dir] = make_map<@string, @string>(len(pkg.ImportMap));
            }

            foreach (var (k, v) in pkg.ImportMap) {
                importMap[pkg.Dir][k] = v;
            }

        }

        sort.Strings(stdPackages);
        imports = new listImports(stdPackages:stdPackages,importMap:importMap,importDir:importDir,);
        imports, _ = listCache.LoadOrStore(name, imports);

    }
    listImports li = imports._<listImports>();
    w.stdPackages = li.stdPackages;
    w.importDir = li.importDir;
    w.importMap = li.importMap;

});

// listEnv returns the process environment to use when invoking 'go list' for
// the given context.
private static slice<@string> listEnv(ptr<build.Context> _addr_c) {
    ref build.Context c = ref _addr_c.val;

    if (c == null) {
        return os.Environ();
    }
    var environ = append(os.Environ(), "GOOS=" + c.GOOS, "GOARCH=" + c.GOARCH);
    if (c.CgoEnabled) {
        environ = append(environ, "CGO_ENABLED=1");
    }
    else
 {
        environ = append(environ, "CGO_ENABLED=0");
    }
    return environ;

}

// Importing is a sentinel taking the place in Walker.imported
// for a package that is in the process of being imported.
private static types.Package importing = default;

private static (ptr<types.Package>, error) Import(this ptr<Walker> _addr_w, @string name) {
    ptr<types.Package> _p0 = default!;
    error _p0 = default!;
    ref Walker w = ref _addr_w.val;

    return _addr_w.ImportFrom(name, "", 0)!;
}

private static (ptr<types.Package>, error) ImportFrom(this ptr<Walker> _addr_w, @string fromPath, @string fromDir, types.ImportMode mode) {
    ptr<types.Package> _p0 = default!;
    error _p0 = default!;
    ref Walker w = ref _addr_w.val;

    var name = fromPath;
    {
        var (canonical, ok) = w.importMap[fromDir][fromPath];

        if (ok) {
            name = canonical;
        }
    }


    var pkg = w.imported[name];
    if (pkg != null) {
        if (pkg == _addr_importing) {
            log.Fatalf("cycle importing package %q", name);
        }
        return (_addr_pkg!, error.As(null!)!);

    }
    w.imported[name] = _addr_importing; 

    // Determine package files.
    var dir = w.importDir[name];
    if (dir == "") {
        dir = filepath.Join(w.root, filepath.FromSlash(name));
    }
    {
        var (fi, err) = os.Stat(dir);

        if (err != null || !fi.IsDir()) {
            log.Panicf("no source in tree for import %q (from import %s in %s): %v", name, fromPath, fromDir, err);
        }
    }


    var context = w.context;
    if (context == null) {
        context = _addr_build.Default;
    }
    @string key = default;
    if (usePkgCache) {
        {
            var (tags, ok) = pkgTags[dir];

            if (ok) {
                key = tagKey(dir, _addr_context, tags);
                {
                    var pkg__prev3 = pkg;

                    pkg = pkgCache[key];

                    if (pkg != null) {
                        w.imported[name] = pkg;
                        return (_addr_pkg!, error.As(null!)!);
                    }

                    pkg = pkg__prev3;

                }

            }

        }

    }
    var (info, err) = context.ImportDir(dir, 0);
    if (err != null) {
        {
            ptr<build.NoGoError> (_, nogo) = err._<ptr<build.NoGoError>>();

            if (nogo) {
                return (_addr_null!, error.As(err)!);
            }

        }

        log.Fatalf("pkg %q, dir %q: ScanDir: %v", name, dir, err);

    }
    if (usePkgCache) {
        {
            var (_, ok) = pkgTags[dir];

            if (!ok) {
                pkgTags[dir] = info.AllTags;
                key = tagKey(dir, _addr_context, info.AllTags);
            }

        }

    }
    var filenames = append(append(new slice<@string>(new @string[] {  }), info.GoFiles), info.CgoFiles); 

    // Parse package files.
    slice<ptr<ast.File>> files = default;
    foreach (var (_, file) in filenames) {
        var (f, err) = w.parseFile(dir, file);
        if (err != null) {
            log.Fatalf("error parsing package %s: %s", name, err);
        }
        files = append(files, f);

    }    types.Config conf = new types.Config(IgnoreFuncBodies:true,FakeImportC:true,Importer:w,);
    pkg, err = conf.Check(name, fset, files, null);
    if (err != null) {
        @string ctxt = "<no context>";
        if (w.context != null) {
            ctxt = fmt.Sprintf("%s-%s", w.context.GOOS, w.context.GOARCH);
        }
        log.Fatalf("error typechecking package %s: %s (%s)", name, err, ctxt);

    }
    if (usePkgCache) {
        pkgCache[key] = pkg;
    }
    w.imported[name] = pkg;
    return (_addr_pkg!, error.As(null!)!);

}

// pushScope enters a new scope (walking a package, type, node, etc)
// and returns a function that will leave the scope (with sanity checking
// for mismatched pushes & pops)
private static Action pushScope(this ptr<Walker> _addr_w, @string name) {
    Action popFunc = default;
    ref Walker w = ref _addr_w.val;

    w.scope = append(w.scope, name);
    return () => {
        if (len(w.scope) == 0) {
            log.Fatalf("attempt to leave scope %q with empty scope list", name);
        }
        if (w.scope[len(w.scope) - 1] != name) {
            log.Fatalf("attempt to leave scope %q, but scope is currently %#v", name, w.scope);
        }
        w.scope = w.scope[..(int)len(w.scope) - 1];

    };

}

private static slice<@string> sortedMethodNames(ptr<types.Interface> _addr_typ) {
    ref types.Interface typ = ref _addr_typ.val;

    var n = typ.NumMethods();
    var list = make_slice<@string>(n);
    foreach (var (i) in list) {
        list[i] = typ.Method(i).Name();
    }    sort.Strings(list);
    return list;
}

private static void writeType(this ptr<Walker> _addr_w, ptr<bytes.Buffer> _addr_buf, types.Type typ) => func((_, panic, _) => {
    ref Walker w = ref _addr_w.val;
    ref bytes.Buffer buf = ref _addr_buf.val;

    switch (typ.type()) {
        case ptr<types.Basic> typ:
            var s = typ.Name();

            if (typ.Kind() == types.UnsafePointer) 
                s = "unsafe.Pointer";
            else if (typ.Kind() == types.UntypedBool) 
                s = "ideal-bool";
            else if (typ.Kind() == types.UntypedInt) 
                s = "ideal-int";
            else if (typ.Kind() == types.UntypedRune) 
                // "ideal-char" for compatibility with old tool
                // TODO(gri) change to "ideal-rune"
                s = "ideal-char";
            else if (typ.Kind() == types.UntypedFloat) 
                s = "ideal-float";
            else if (typ.Kind() == types.UntypedComplex) 
                s = "ideal-complex";
            else if (typ.Kind() == types.UntypedString) 
                s = "ideal-string";
            else if (typ.Kind() == types.UntypedNil) 
                panic("should never see untyped nil type");
            else 
                switch (s) {
                    case "byte": 
                        s = "uint8";
                        break;
                    case "rune": 
                        s = "int32";
                        break;
                }
                        buf.WriteString(s);
            break;
        case ptr<types.Array> typ:
            fmt.Fprintf(buf, "[%d]", typ.Len());
            w.writeType(buf, typ.Elem());
            break;
        case ptr<types.Slice> typ:
            buf.WriteString("[]");
            w.writeType(buf, typ.Elem());
            break;
        case ptr<types.Struct> typ:
            buf.WriteString("struct");
            break;
        case ptr<types.Pointer> typ:
            buf.WriteByte('*');
            w.writeType(buf, typ.Elem());
            break;
        case ptr<types.Tuple> typ:
            panic("should never see a tuple type");
            break;
        case ptr<types.Signature> typ:
            buf.WriteString("func");
            w.writeSignature(buf, typ);
            break;
        case ptr<types.Interface> typ:
            buf.WriteString("interface{");
            if (typ.NumMethods() > 0) {
                buf.WriteByte(' ');
                buf.WriteString(strings.Join(sortedMethodNames(_addr_typ), ", "));
                buf.WriteByte(' ');
            }
            buf.WriteString("}");
            break;
        case ptr<types.Map> typ:
            buf.WriteString("map[");
            w.writeType(buf, typ.Key());
            buf.WriteByte(']');
            w.writeType(buf, typ.Elem());
            break;
        case ptr<types.Chan> typ:
            s = default;

            if (typ.Dir() == types.SendOnly) 
                s = "chan<- ";
            else if (typ.Dir() == types.RecvOnly) 
                s = "<-chan ";
            else if (typ.Dir() == types.SendRecv) 
                s = "chan ";
            else 
                panic("unreachable");
                        buf.WriteString(s);
            w.writeType(buf, typ.Elem());
            break;
        case ptr<types.Named> typ:
            var obj = typ.Obj();
            var pkg = obj.Pkg();
            if (pkg != null && pkg != w.current) {
                buf.WriteString(pkg.Name());
                buf.WriteByte('.');
            }
            buf.WriteString(typ.Obj().Name());
            break;
        default:
        {
            var typ = typ.type();
            panic(fmt.Sprintf("unknown type %T", typ));
            break;
        }
    }

});

private static void writeSignature(this ptr<Walker> _addr_w, ptr<bytes.Buffer> _addr_buf, ptr<types.Signature> _addr_sig) {
    ref Walker w = ref _addr_w.val;
    ref bytes.Buffer buf = ref _addr_buf.val;
    ref types.Signature sig = ref _addr_sig.val;

    w.writeParams(buf, sig.Params(), sig.Variadic());
    {
        var res = sig.Results();

        switch (res.Len()) {
            case 0: 

                break;
            case 1: 
                buf.WriteByte(' ');
                w.writeType(buf, res.At(0).Type());
                break;
            default: 
                buf.WriteByte(' ');
                w.writeParams(buf, res, false);
                break;
        }
    }

}

private static void writeParams(this ptr<Walker> _addr_w, ptr<bytes.Buffer> _addr_buf, ptr<types.Tuple> _addr_t, bool variadic) {
    ref Walker w = ref _addr_w.val;
    ref bytes.Buffer buf = ref _addr_buf.val;
    ref types.Tuple t = ref _addr_t.val;

    buf.WriteByte('(');
    for (nint i = 0;
    var n = t.Len(); i < n; i++) {
        if (i > 0) {
            buf.WriteString(", ");
        }
        var typ = t.At(i).Type();
        if (variadic && i + 1 == n) {
            buf.WriteString("...");
            typ = typ._<ptr<types.Slice>>().Elem();
        }
        w.writeType(buf, typ);

    }
    buf.WriteByte(')');

}

private static @string typeString(this ptr<Walker> _addr_w, types.Type typ) {
    ref Walker w = ref _addr_w.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    w.writeType(_addr_buf, typ);
    return buf.String();
}

private static @string signatureString(this ptr<Walker> _addr_w, ptr<types.Signature> _addr_sig) {
    ref Walker w = ref _addr_w.val;
    ref types.Signature sig = ref _addr_sig.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    w.writeSignature(_addr_buf, sig);
    return buf.String();
}

private static void emitObj(this ptr<Walker> _addr_w, types.Object obj) => func((_, panic, _) => {
    ref Walker w = ref _addr_w.val;

    switch (obj.type()) {
        case ptr<types.Const> obj:
            w.emitf("const %s %s", obj.Name(), w.typeString(obj.Type()));
            var x = obj.Val();
            var @short = x.String();
            var exact = x.ExactString();
            if (short == exact) {
                w.emitf("const %s = %s", obj.Name(), short);
            }
            else
 {
                w.emitf("const %s = %s  // %s", obj.Name(), short, exact);
            }

            break;
        case ptr<types.Var> obj:
            w.emitf("var %s %s", obj.Name(), w.typeString(obj.Type()));
            break;
        case ptr<types.TypeName> obj:
            w.emitType(obj);
            break;
        case ptr<types.Func> obj:
            w.emitFunc(obj);
            break;
        default:
        {
            var obj = obj.type();
            panic("unknown object: " + obj.String());
            break;
        }
    }

});

private static void emitType(this ptr<Walker> _addr_w, ptr<types.TypeName> _addr_obj) {
    ref Walker w = ref _addr_w.val;
    ref types.TypeName obj = ref _addr_obj.val;

    var name = obj.Name();
    var typ = obj.Type();
    if (obj.IsAlias()) {
        w.emitf("type %s = %s", name, w.typeString(typ));
        return ;
    }
    switch (typ.Underlying().type()) {
        case ptr<types.Struct> typ:
            w.emitStructType(name, typ);
            break;
        case ptr<types.Interface> typ:
            w.emitIfaceType(name, typ);
            return ; // methods are handled by emitIfaceType
            break;
        default:
        {
            var typ = typ.Underlying().type();
            w.emitf("type %s %s", name, w.typeString(typ.Underlying()));
            break;
        } 

        // emit methods with value receiver
    } 

    // emit methods with value receiver
    map<@string, bool> methodNames = default;
    var vset = types.NewMethodSet(typ);
    {
        nint i__prev1 = i;
        var n__prev1 = n;

        for (nint i = 0;
        var n = vset.Len(); i < n; i++) {
            var m = vset.At(i);
            if (m.Obj().Exported()) {
                w.emitMethod(m);
                if (methodNames == null) {
                    methodNames = make_map<@string, bool>();
                }
                methodNames[m.Obj().Name()] = true;
            }
        }

        i = i__prev1;
        n = n__prev1;
    } 

    // emit methods with pointer receiver; exclude
    // methods that we have emitted already
    // (the method set of *T includes the methods of T)
    var pset = types.NewMethodSet(types.NewPointer(typ));
    {
        nint i__prev1 = i;
        var n__prev1 = n;

        for (i = 0;
        n = pset.Len(); i < n; i++) {
            m = pset.At(i);
            if (m.Obj().Exported() && !methodNames[m.Obj().Name()]) {
                w.emitMethod(m);
            }
        }

        i = i__prev1;
        n = n__prev1;
    }

}

private static void emitStructType(this ptr<Walker> _addr_w, @string name, ptr<types.Struct> _addr_typ) => func((defer, _, _) => {
    ref Walker w = ref _addr_w.val;
    ref types.Struct typ = ref _addr_typ.val;

    var typeStruct = fmt.Sprintf("type %s struct", name);
    w.emitf(typeStruct);
    defer(w.pushScope(typeStruct)());

    for (nint i = 0; i < typ.NumFields(); i++) {
        var f = typ.Field(i);
        if (!f.Exported()) {
            continue;
        }
        var typ = f.Type();
        if (f.Anonymous()) {
            w.emitf("embedded %s", w.typeString(typ));
            continue;
        }
        w.emitf("%s %s", f.Name(), w.typeString(typ));

    }

});

private static void emitIfaceType(this ptr<Walker> _addr_w, @string name, ptr<types.Interface> _addr_typ) {
    ref Walker w = ref _addr_w.val;
    ref types.Interface typ = ref _addr_typ.val;

    var pop = w.pushScope("type " + name + " interface");

    slice<@string> methodNames = default;
    var complete = true;
    var mset = types.NewMethodSet(typ);
    for (nint i = 0;
    var n = mset.Len(); i < n; i++) {
        ptr<types.Func> m = mset.At(i).Obj()._<ptr<types.Func>>();
        if (!m.Exported()) {
            complete = false;
            continue;
        }
        methodNames = append(methodNames, m.Name());
        w.emitf("%s%s", m.Name(), w.signatureString(m.Type()._<ptr<types.Signature>>()));

    }

    if (!complete) { 
        // The method set has unexported methods, so all the
        // implementations are provided by the same package,
        // so the method set can be extended. Instead of recording
        // the full set of names (below), record only that there were
        // unexported methods. (If the interface shrinks, we will notice
        // because a method signature emitted during the last loop
        // will disappear.)
        w.emitf("unexported methods");

    }
    pop();

    if (!complete) {
        return ;
    }
    if (len(methodNames) == 0) {
        w.emitf("type %s interface {}", name);
        return ;
    }
    sort.Strings(methodNames);
    w.emitf("type %s interface { %s }", name, strings.Join(methodNames, ", "));

}

private static void emitFunc(this ptr<Walker> _addr_w, ptr<types.Func> _addr_f) => func((_, panic, _) => {
    ref Walker w = ref _addr_w.val;
    ref types.Func f = ref _addr_f.val;

    ptr<types.Signature> sig = f.Type()._<ptr<types.Signature>>();
    if (sig.Recv() != null) {
        panic("method considered a regular function: " + f.String());
    }
    w.emitf("func %s%s", f.Name(), w.signatureString(sig));

});

private static void emitMethod(this ptr<Walker> _addr_w, ptr<types.Selection> _addr_m) {
    ref Walker w = ref _addr_w.val;
    ref types.Selection m = ref _addr_m.val;

    ptr<types.Signature> sig = m.Type()._<ptr<types.Signature>>();
    var recv = sig.Recv().Type(); 
    // report exported methods with unexported receiver base type
    if (true) {
        var @base = recv;
        {
            ptr<types.Pointer> (p, _) = recv._<ptr<types.Pointer>>();

            if (p != null) {
                base = p.Elem();
            }

        }

        {
            ptr<types.Named> obj = base._<ptr<types.Named>>().Obj();

            if (!obj.Exported()) {
                log.Fatalf("exported method with unexported receiver base type: %s", m);
            }

        }

    }
    w.emitf("method (%s) %s%s", w.typeString(recv), m.Obj().Name(), w.signatureString(sig));

}

private static void emitf(this ptr<Walker> _addr_w, @string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();
    ref Walker w = ref _addr_w.val;

    var f = strings.Join(w.scope, ", ") + ", " + fmt.Sprintf(format, args);
    if (strings.Contains(f, "\n")) {
        panic("feature contains newlines: " + f);
    }
    {
        var (_, dup) = w.features[f];

        if (dup) {
            panic("duplicate feature inserted: " + f);
        }
    }

    w.features[f] = true;

    if (verbose.val) {
        log.Printf("feature: %s", f);
    }
});

} // end main_package
