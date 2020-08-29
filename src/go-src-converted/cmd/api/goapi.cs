// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Binary api computes the exported API of a set of Go packages.
// package main -- go2cs converted at 2020 August 29 08:46:38 UTC
// Original source: C:\Go\src\cmd\api\goapi.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static @string goCmd()
        {
            @string exeSuffix = default;
            if (runtime.GOOS == "windows")
            {
                exeSuffix = ".exe";
            }
            var path = filepath.Join(runtime.GOROOT(), "bin", "go" + exeSuffix);
            {
                var (_, err) = os.Stat(path);

                if (err == null)
                {
                    return path;
                }
            }
            return "go";
        }

        // Flags
        private static var checkFile = flag.String("c", "", "optional comma-separated filename(s) to check API against");        private static var allowNew = flag.Bool("allow_new", true, "allow API additions");        private static var exceptFile = flag.String("except", "", "optional filename of packages that are allowed to change without triggering a failure in the tool");        private static var nextFile = flag.String("next", "", "optional filename of tentative upcoming API features for the next release. This file can be lazily maintained. It only affects the delta warnings from the -c file printed on success.");        private static var verbose = flag.Bool("v", false, "verbose debugging");        private static var forceCtx = flag.String("contexts", "", "optional comma-separated list of <goos>-<goarch>[-cgo] to override default contexts.");

        // contexts are the default contexts which are scanned, unless
        // overridden by the -contexts flag.
        private static ref build.Context contexts = new slice<ref build.Context>(new ref build.Context[] { {GOOS:"linux",GOARCH:"386",CgoEnabled:true}, {GOOS:"linux",GOARCH:"386"}, {GOOS:"linux",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"linux",GOARCH:"amd64"}, {GOOS:"linux",GOARCH:"arm",CgoEnabled:true}, {GOOS:"linux",GOARCH:"arm"}, {GOOS:"darwin",GOARCH:"386",CgoEnabled:true}, {GOOS:"darwin",GOARCH:"386"}, {GOOS:"darwin",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"darwin",GOARCH:"amd64"}, {GOOS:"windows",GOARCH:"amd64"}, {GOOS:"windows",GOARCH:"386"}, {GOOS:"freebsd",GOARCH:"386",CgoEnabled:true}, {GOOS:"freebsd",GOARCH:"386"}, {GOOS:"freebsd",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"freebsd",GOARCH:"amd64"}, {GOOS:"freebsd",GOARCH:"arm",CgoEnabled:true}, {GOOS:"freebsd",GOARCH:"arm"}, {GOOS:"netbsd",GOARCH:"386",CgoEnabled:true}, {GOOS:"netbsd",GOARCH:"386"}, {GOOS:"netbsd",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"netbsd",GOARCH:"amd64"}, {GOOS:"netbsd",GOARCH:"arm",CgoEnabled:true}, {GOOS:"netbsd",GOARCH:"arm"}, {GOOS:"openbsd",GOARCH:"386",CgoEnabled:true}, {GOOS:"openbsd",GOARCH:"386"}, {GOOS:"openbsd",GOARCH:"amd64",CgoEnabled:true}, {GOOS:"openbsd",GOARCH:"amd64"} });

        private static @string contextName(ref build.Context c)
        {
            var s = c.GOOS + "-" + c.GOARCH;
            if (c.CgoEnabled)
            {
                return s + "-cgo";
            }
            return s;
        }

        private static ref build.Context parseContext(@string c)
        {
            var parts = strings.Split(c, "-");
            if (len(parts) < 2L)
            {
                log.Fatalf("bad context: %q", c);
            }
            build.Context bc = ref new build.Context(GOOS:parts[0],GOARCH:parts[1],);
            if (len(parts) == 3L)
            {
                if (parts[2L] == "cgo")
                {
                    bc.CgoEnabled = true;
                }
                else
                {
                    log.Fatalf("bad context: %q", c);
                }
            }
            return bc;
        }

        private static void setContexts()
        {
            contexts = new slice<ref build.Context>(new ref build.Context[] {  });
            foreach (var (_, c) in strings.Split(forceCtx.Value, ","))
            {
                contexts = append(contexts, parseContext(c));
            }
        }

        private static var internalPkg = regexp.MustCompile("(^|/)internal($|/)");

        private static void Main() => func((defer, _, __) =>
        {
            flag.Parse();

            if (!strings.Contains(runtime.Version(), "weekly") && !strings.Contains(runtime.Version(), "devel"))
            {
                if (nextFile != "".Value)
                {
                    fmt.Printf("Go version is %q, ignoring -next %s\n", runtime.Version(), nextFile.Value);
                    nextFile.Value = "";
                }
            }
            if (forceCtx != "".Value)
            {
                setContexts();
            }
            foreach (var (_, c) in contexts)
            {
                c.Compiler = build.Default.Compiler;
            }
            slice<@string> pkgNames = default;
            if (flag.NArg() > 0L)
            {
                pkgNames = flag.Args();
            }
            else
            {
                var (stds, err) = exec.Command(goCmd(), "list", "std").Output();
                if (err != null)
                {
                    log.Fatal(err);
                }
                {
                    var pkg__prev1 = pkg;

                    foreach (var (_, __pkg) in strings.Fields(string(stds)))
                    {
                        pkg = __pkg;
                        if (!internalPkg.MatchString(pkg))
                        {
                            pkgNames = append(pkgNames, pkg);
                        }
                    }

                    pkg = pkg__prev1;
                }

            }
            var featureCtx = make_map<@string, map<@string, bool>>(); // feature -> context name -> true
            foreach (var (_, context) in contexts)
            {
                var w = NewWalker(context, filepath.Join(build.Default.GOROOT, "src"));

                foreach (var (_, name) in pkgNames)
                { 
                    // Vendored packages do not contribute to our
                    // public API surface.
                    if (strings.HasPrefix(name, "vendor/"))
                    {
                        continue;
                    } 
                    // - Package "unsafe" contains special signatures requiring
                    //   extra care when printing them - ignore since it is not
                    //   going to change w/o a language change.
                    // - We don't care about the API of commands.
                    if (name != "unsafe" && !strings.HasPrefix(name, "cmd/"))
                    {
                        if (name == "runtime/cgo" && !context.CgoEnabled)
                        { 
                            // w.Import(name) will return nil
                            continue;
                        }
                        var (pkg, _) = w.Import(name);
                        w.export(pkg);
                    }
                }
                var ctxName = contextName(context);
                {
                    var f__prev2 = f;

                    foreach (var (_, __f) in w.Features())
                    {
                        f = __f;
                        if (featureCtx[f] == null)
                        {
                            featureCtx[f] = make_map<@string, bool>();
                        }
                        featureCtx[f][ctxName] = true;
                    }

                    f = f__prev2;
                }

            }
            slice<@string> features = default;
            {
                var f__prev1 = f;

                foreach (var (__f, __cmap) in featureCtx)
                {
                    f = __f;
                    cmap = __cmap;
                    if (len(cmap) == len(contexts))
                    {
                        features = append(features, f);
                        continue;
                    }
                    var comma = strings.Index(f, ",");
                    foreach (var (cname) in cmap)
                    {
                        var f2 = fmt.Sprintf("%s (%s)%s", f[..comma], cname, f[comma..]);
                        features = append(features, f2);
                    }
                }

                f = f__prev1;
            }

            var fail = false;
            defer(() =>
            {
                if (fail)
                {
                    os.Exit(1L);
                }
            }());

            var bw = bufio.NewWriter(os.Stdout);
            defer(bw.Flush());

            if (checkFile == "".Value)
            {
                sort.Strings(features);
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in features)
                    {
                        f = __f;
                        fmt.Fprintln(bw, f);
                    }

                    f = f__prev1;
                }

                return;
            }
            slice<@string> required = default;
            foreach (var (_, file) in strings.Split(checkFile.Value, ","))
            {
                required = append(required, fileFeatures(file));
            }
            var optional = fileFeatures(nextFile.Value);
            var exception = fileFeatures(exceptFile.Value);
            fail = !compareAPI(bw, features, required, optional, exception, allowNew && strings.Contains(runtime.Version(), "devel").Value);
        });

        // export emits the exported package features.
        private static void export(this ref Walker w, ref types.Package pkg)
        {
            if (verbose.Value)
            {
                log.Println(pkg);
            }
            var pop = w.pushScope("pkg " + pkg.Path());
            w.current = pkg;
            var scope = pkg.Scope();
            foreach (var (_, name) in scope.Names())
            {
                if (ast.IsExported(name))
                {
                    w.emitObj(scope.Lookup(name));
                }
            }
            pop();
        }

        private static map<@string, bool> set(slice<@string> items)
        {
            var s = make_map<@string, bool>();
            foreach (var (_, v) in items)
            {
                s[v] = true;
            }
            return s;
        }

        private static var spaceParensRx = regexp.MustCompile(" \\(\\S+?\\)");

        private static @string featureWithoutContext(@string f)
        {
            if (!strings.Contains(f, "("))
            {
                return f;
            }
            return spaceParensRx.ReplaceAllString(f, "");
        }

        private static bool compareAPI(io.Writer w, slice<@string> features, slice<@string> required, slice<@string> optional, slice<@string> exception, bool allowAdd)
        {
            ok = true;

            var optionalSet = set(optional);
            var exceptionSet = set(exception);
            var featureSet = set(features);

            sort.Strings(features);
            sort.Strings(required);

            Func<ref slice<@string>, @string> take = sl =>
            {
                var s = (sl.Value)[0L];
                sl.Value = (sl.Value)[1L..];
                return s;
            }
;

            while (len(required) > 0L || len(features) > 0L)
            {

                if (len(features) == 0L || (len(required) > 0L && required[0L] < features[0L])) 
                    var feature = take(ref required);
                    if (exceptionSet[feature])
                    { 
                        // An "unfortunate" case: the feature was once
                        // included in the API (e.g. go1.txt), but was
                        // subsequently removed. These are already
                        // acknowledged by being in the file
                        // "api/except.txt". No need to print them out
                        // here.
                    }
                    else if (featureSet[featureWithoutContext(feature)])
                    { 
                        // okay.
                    }
                    else
                    {
                        fmt.Fprintf(w, "-%s\n", feature);
                        ok = false; // broke compatibility
                    }
                else if (len(required) == 0L || (len(features) > 0L && required[0L] > features[0L])) 
                    var newFeature = take(ref features);
                    if (optionalSet[newFeature])
                    { 
                        // Known added feature to the upcoming release.
                        // Delete it from the map so we can detect any upcoming features
                        // which were never seen.  (so we can clean up the nextFile)
                        delete(optionalSet, newFeature);
                    }
                    else
                    {
                        fmt.Fprintf(w, "+%s\n", newFeature);
                        if (!allowAdd)
                        {
                            ok = false; // we're in lock-down mode for next release
                        }
                    }
                else 
                    take(ref required);
                    take(ref features);
                            } 

            // In next file, but not in API.
 

            // In next file, but not in API.
            slice<@string> missing = default;
            {
                var feature__prev1 = feature;

                foreach (var (__feature) in optionalSet)
                {
                    feature = __feature;
                    missing = append(missing, feature);
                }

                feature = feature__prev1;
            }

            sort.Strings(missing);
            {
                var feature__prev1 = feature;

                foreach (var (_, __feature) in missing)
                {
                    feature = __feature;
                    fmt.Fprintf(w, "±%s\n", feature);
                }

                feature = feature__prev1;
            }

            return;
        }

        private static slice<@string> fileFeatures(@string filename)
        {
            if (filename == "")
            {
                return null;
            }
            var (bs, err) = ioutil.ReadFile(filename);
            if (err != null)
            {
                log.Fatalf("Error reading file %s: %v", filename, err);
            }
            var lines = strings.Split(string(bs), "\n");
            slice<@string> nonblank = default;
            foreach (var (_, line) in lines)
            {
                line = strings.TrimSpace(line);
                if (line != "" && !strings.HasPrefix(line, "#"))
                {
                    nonblank = append(nonblank, line);
                }
            }
            return nonblank;
        }

        private static var fset = token.NewFileSet();

        public partial struct Walker
        {
            public ptr<build.Context> context;
            public @string root;
            public slice<@string> scope;
            public ptr<types.Package> current;
            public map<@string, bool> features; // set
            public map<@string, ref types.Package> imported; // packages already imported
        }

        public static ref Walker NewWalker(ref build.Context context, @string root)
        {
            return ref new Walker(context:context,root:root,features:map[string]bool{},imported:map[string]*types.Package{"unsafe":types.Unsafe},);
        }

        private static slice<@string> Features(this ref Walker w)
        {
            foreach (var (f) in w.features)
            {
                fs = append(fs, f);
            }
            sort.Strings(fs);
            return;
        }

        private static var parsedFileCache = make_map<@string, ref ast.File>();

        private static (ref ast.File, error) parseFile(this ref Walker w, @string dir, @string file)
        {
            var filename = filepath.Join(dir, file);
            {
                var f__prev1 = f;

                var f = parsedFileCache[filename];

                if (f != null)
                {
                    return (f, null);
                }

                f = f__prev1;

            }

            var (f, err) = parser.ParseFile(fset, filename, null, 0L);
            if (err != null)
            {
                return (null, err);
            }
            parsedFileCache[filename] = f;

            return (f, null);
        }

        // The package cache doesn't operate correctly in rare (so far artificial)
        // circumstances (issue 8425). Disable before debugging non-obvious errors
        // from the type-checker.
        private static readonly var usePkgCache = true;



        private static map pkgCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ref types.Package>{};        private static map pkgTags = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<@string>>{};

        // tagKey returns the tag-based key to use in the pkgCache.
        // It is a comma-separated string; the first part is dir, the rest tags.
        // The satisfied tags are derived from context but only those that
        // matter (the ones listed in the tags argument) are used.
        // The tags list, which came from go/build's Package.AllTags,
        // is known to be sorted.
        private static @string tagKey(@string dir, ref build.Context context, slice<@string> tags)
        {
            map ctags = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{context.GOOS:true,context.GOARCH:true,};
            if (context.CgoEnabled)
            {
                ctags["cgo"] = true;
            }
            {
                var tag__prev1 = tag;

                foreach (var (_, __tag) in context.BuildTags)
                {
                    tag = __tag;
                    ctags[tag] = true;
                } 
                // TODO: ReleaseTags (need to load default)

                tag = tag__prev1;
            }

            var key = dir;
            {
                var tag__prev1 = tag;

                foreach (var (_, __tag) in tags)
                {
                    tag = __tag;
                    if (ctags[tag])
                    {
                        key += "," + tag;
                    }
                }

                tag = tag__prev1;
            }

            return key;
        }

        // Importing is a sentinel taking the place in Walker.imported
        // for a package that is in the process of being imported.
        private static types.Package importing = default;

        private static (ref types.Package, error) Import(this ref Walker w, @string name)
        {
            var pkg = w.imported[name];
            if (pkg != null)
            {
                if (pkg == ref importing)
                {
                    log.Fatalf("cycle importing package %q", name);
                }
                return (pkg, null);
            }
            w.imported[name] = ref importing;

            var root = w.root;
            if (strings.HasPrefix(name, "golang_org/x/"))
            {
                root = filepath.Join(root, "vendor");
            } 

            // Determine package files.
            var dir = filepath.Join(root, filepath.FromSlash(name));
            {
                var (fi, err) = os.Stat(dir);

                if (err != null || !fi.IsDir())
                {
                    log.Fatalf("no source in tree for import %q: %v", name, err);
                }

            }

            var context = w.context;
            if (context == null)
            {
                context = ref build.Default;
            } 

            // Look in cache.
            // If we've already done an import with the same set
            // of relevant tags, reuse the result.
            @string key = default;
            if (usePkgCache)
            {
                {
                    var (tags, ok) = pkgTags[dir];

                    if (ok)
                    {
                        key = tagKey(dir, context, tags);
                        {
                            var pkg__prev3 = pkg;

                            pkg = pkgCache[key];

                            if (pkg != null)
                            {
                                w.imported[name] = pkg;
                                return (pkg, null);
                            }

                            pkg = pkg__prev3;

                        }
                    }

                }
            }
            var (info, err) = context.ImportDir(dir, 0L);
            if (err != null)
            {
                {
                    ref build.NoGoError (_, nogo) = err._<ref build.NoGoError>();

                    if (nogo)
                    {
                        return (null, null);
                    }

                }
                log.Fatalf("pkg %q, dir %q: ScanDir: %v", name, dir, err);
            } 

            // Save tags list first time we see a directory.
            if (usePkgCache)
            {
                {
                    var (_, ok) = pkgTags[dir];

                    if (!ok)
                    {
                        pkgTags[dir] = info.AllTags;
                        key = tagKey(dir, context, info.AllTags);
                    }

                }
            }
            var filenames = append(append(new slice<@string>(new @string[] {  }), info.GoFiles), info.CgoFiles); 

            // Parse package files.
            slice<ref ast.File> files = default;
            foreach (var (_, file) in filenames)
            {
                var (f, err) = w.parseFile(dir, file);
                if (err != null)
                {
                    log.Fatalf("error parsing package %s: %s", name, err);
                }
                files = append(files, f);
            } 

            // Type-check package files.
            types.Config conf = new types.Config(IgnoreFuncBodies:true,FakeImportC:true,Importer:w,);
            pkg, err = conf.Check(name, fset, files, null);
            if (err != null)
            {
                @string ctxt = "<no context>";
                if (w.context != null)
                {
                    ctxt = fmt.Sprintf("%s-%s", w.context.GOOS, w.context.GOARCH);
                }
                log.Fatalf("error typechecking package %s: %s (%s)", name, err, ctxt);
            }
            if (usePkgCache)
            {
                pkgCache[key] = pkg;
            }
            w.imported[name] = pkg;
            return (pkg, null);
        }

        // pushScope enters a new scope (walking a package, type, node, etc)
        // and returns a function that will leave the scope (with sanity checking
        // for mismatched pushes & pops)
        private static Action pushScope(this ref Walker w, @string name)
        {
            w.scope = append(w.scope, name);
            return () =>
            {
                if (len(w.scope) == 0L)
                {
                    log.Fatalf("attempt to leave scope %q with empty scope list", name);
                }
                if (w.scope[len(w.scope) - 1L] != name)
                {
                    log.Fatalf("attempt to leave scope %q, but scope is currently %#v", name, w.scope);
                }
                w.scope = w.scope[..len(w.scope) - 1L];
            }
;
        }

        private static slice<@string> sortedMethodNames(ref types.Interface typ)
        {
            var n = typ.NumMethods();
            var list = make_slice<@string>(n);
            foreach (var (i) in list)
            {
                list[i] = typ.Method(i).Name();
            }
            sort.Strings(list);
            return list;
        }

        private static void writeType(this ref Walker _w, ref bytes.Buffer _buf, types.Type typ) => func(_w, _buf, (ref Walker w, ref bytes.Buffer buf, Defer _, Panic panic, Recover __) =>
        {
            switch (typ.type())
            {
                case ref types.Basic typ:
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
                        switch (s)
                        {
                            case "byte": 
                                s = "uint8";
                                break;
                            case "rune": 
                                s = "int32";
                                break;
                        }
                                        buf.WriteString(s);
                    break;
                case ref types.Array typ:
                    fmt.Fprintf(buf, "[%d]", typ.Len());
                    w.writeType(buf, typ.Elem());
                    break;
                case ref types.Slice typ:
                    buf.WriteString("[]");
                    w.writeType(buf, typ.Elem());
                    break;
                case ref types.Struct typ:
                    buf.WriteString("struct");
                    break;
                case ref types.Pointer typ:
                    buf.WriteByte('*');
                    w.writeType(buf, typ.Elem());
                    break;
                case ref types.Tuple typ:
                    panic("should never see a tuple type");
                    break;
                case ref types.Signature typ:
                    buf.WriteString("func");
                    w.writeSignature(buf, typ);
                    break;
                case ref types.Interface typ:
                    buf.WriteString("interface{");
                    if (typ.NumMethods() > 0L)
                    {
                        buf.WriteByte(' ');
                        buf.WriteString(strings.Join(sortedMethodNames(typ), ", "));
                        buf.WriteByte(' ');
                    }
                    buf.WriteString("}");
                    break;
                case ref types.Map typ:
                    buf.WriteString("map[");
                    w.writeType(buf, typ.Key());
                    buf.WriteByte(']');
                    w.writeType(buf, typ.Elem());
                    break;
                case ref types.Chan typ:
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
                case ref types.Named typ:
                    var obj = typ.Obj();
                    var pkg = obj.Pkg();
                    if (pkg != null && pkg != w.current)
                    {
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

        private static void writeSignature(this ref Walker w, ref bytes.Buffer buf, ref types.Signature sig)
        {
            w.writeParams(buf, sig.Params(), sig.Variadic());
            {
                var res = sig.Results();

                switch (res.Len())
                {
                    case 0L: 
                        break;
                    case 1L: 
                        buf.WriteByte(' ');
                        w.writeType(buf, res.At(0L).Type());
                        break;
                    default: 
                        buf.WriteByte(' ');
                        w.writeParams(buf, res, false);
                        break;
                }
            }
        }

        private static void writeParams(this ref Walker w, ref bytes.Buffer buf, ref types.Tuple t, bool variadic)
        {
            buf.WriteByte('(');
            for (long i = 0L;
            var n = t.Len(); i < n; i++)
            {
                if (i > 0L)
                {
                    buf.WriteString(", ");
                }
                var typ = t.At(i).Type();
                if (variadic && i + 1L == n)
                {
                    buf.WriteString("...");
                    typ = typ._<ref types.Slice>().Elem();
                }
                w.writeType(buf, typ);
            }

            buf.WriteByte(')');
        }

        private static @string typeString(this ref Walker w, types.Type typ)
        {
            bytes.Buffer buf = default;
            w.writeType(ref buf, typ);
            return buf.String();
        }

        private static @string signatureString(this ref Walker w, ref types.Signature sig)
        {
            bytes.Buffer buf = default;
            w.writeSignature(ref buf, sig);
            return buf.String();
        }

        private static void emitObj(this ref Walker _w, types.Object obj) => func(_w, (ref Walker w, Defer _, Panic panic, Recover __) =>
        {
            switch (obj.type())
            {
                case ref types.Const obj:
                    w.emitf("const %s %s", obj.Name(), w.typeString(obj.Type()));
                    var x = obj.Val();
                    var @short = x.String();
                    var exact = x.ExactString();
                    if (short == exact)
                    {
                        w.emitf("const %s = %s", obj.Name(), short);
                    }
                    else
                    {
                        w.emitf("const %s = %s  // %s", obj.Name(), short, exact);
                    }
                    break;
                case ref types.Var obj:
                    w.emitf("var %s %s", obj.Name(), w.typeString(obj.Type()));
                    break;
                case ref types.TypeName obj:
                    w.emitType(obj);
                    break;
                case ref types.Func obj:
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

        private static void emitType(this ref Walker w, ref types.TypeName obj)
        {
            var name = obj.Name();
            var typ = obj.Type();
            switch (typ.Underlying().type())
            {
                case ref types.Struct typ:
                    w.emitStructType(name, typ);
                    break;
                case ref types.Interface typ:
                    w.emitIfaceType(name, typ);
                    return; // methods are handled by emitIfaceType
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
                long i__prev1 = i;
                var n__prev1 = n;

                for (long i = 0L;
                var n = vset.Len(); i < n; i++)
                {
                    var m = vset.At(i);
                    if (m.Obj().Exported())
                    {
                        w.emitMethod(m);
                        if (methodNames == null)
                        {
                            methodNames = make_map<@string, bool>();
                        }
                        methodNames[m.Obj().Name()] = true;
                    }
                } 

                // emit methods with pointer receiver; exclude
                // methods that we have emitted already
                // (the method set of *T includes the methods of T)


                i = i__prev1;
                n = n__prev1;
            } 

            // emit methods with pointer receiver; exclude
            // methods that we have emitted already
            // (the method set of *T includes the methods of T)
            var pset = types.NewMethodSet(types.NewPointer(typ));
            {
                long i__prev1 = i;
                var n__prev1 = n;

                for (i = 0L;
                n = pset.Len(); i < n; i++)
                {
                    m = pset.At(i);
                    if (m.Obj().Exported() && !methodNames[m.Obj().Name()])
                    {
                        w.emitMethod(m);
                    }
                }


                i = i__prev1;
                n = n__prev1;
            }
        }

        private static void emitStructType(this ref Walker _w, @string name, ref types.Struct _typ) => func(_w, _typ, (ref Walker w, ref types.Struct typ, Defer defer, Panic _, Recover __) =>
        {
            var typeStruct = fmt.Sprintf("type %s struct", name);
            w.emitf(typeStruct);
            defer(w.pushScope(typeStruct)());

            for (long i = 0L; i < typ.NumFields(); i++)
            {
                var f = typ.Field(i);
                if (!f.Exported())
                {
                    continue;
                }
                var typ = f.Type();
                if (f.Anonymous())
                {
                    w.emitf("embedded %s", w.typeString(typ));
                    continue;
                }
                w.emitf("%s %s", f.Name(), w.typeString(typ));
            }

        });

        private static void emitIfaceType(this ref Walker w, @string name, ref types.Interface typ)
        {
            var pop = w.pushScope("type " + name + " interface");

            slice<@string> methodNames = default;
            var complete = true;
            var mset = types.NewMethodSet(typ);
            for (long i = 0L;
            var n = mset.Len(); i < n; i++)
            {
                ref types.Func m = mset.At(i).Obj()._<ref types.Func>();
                if (!m.Exported())
                {
                    complete = false;
                    continue;
                }
                methodNames = append(methodNames, m.Name());
                w.emitf("%s%s", m.Name(), w.signatureString(m.Type()._<ref types.Signature>()));
            }


            if (!complete)
            { 
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

            if (!complete)
            {
                return;
            }
            if (len(methodNames) == 0L)
            {
                w.emitf("type %s interface {}", name);
                return;
            }
            sort.Strings(methodNames);
            w.emitf("type %s interface { %s }", name, strings.Join(methodNames, ", "));
        }

        private static void emitFunc(this ref Walker _w, ref types.Func _f) => func(_w, _f, (ref Walker w, ref types.Func f, Defer _, Panic panic, Recover __) =>
        {
            ref types.Signature sig = f.Type()._<ref types.Signature>();
            if (sig.Recv() != null)
            {
                panic("method considered a regular function: " + f.String());
            }
            w.emitf("func %s%s", f.Name(), w.signatureString(sig));
        });

        private static void emitMethod(this ref Walker w, ref types.Selection m)
        {
            ref types.Signature sig = m.Type()._<ref types.Signature>();
            var recv = sig.Recv().Type(); 
            // report exported methods with unexported receiver base type
            if (true)
            {
                var @base = recv;
                {
                    ref types.Pointer (p, _) = recv._<ref types.Pointer>();

                    if (p != null)
                    {
                        base = p.Elem();
                    }

                }
                {
                    ref types.Named obj = base._<ref types.Named>().Obj();

                    if (!obj.Exported())
                    {
                        log.Fatalf("exported method with unexported receiver base type: %s", m);
                    }

                }
            }
            w.emitf("method (%s) %s%s", w.typeString(recv), m.Obj().Name(), w.signatureString(sig));
        }

        private static void emitf(this ref Walker _w, @string format, params object[] args) => func(_w, (ref Walker w, Defer _, Panic panic, Recover __) =>
        {
            var f = strings.Join(w.scope, ", ") + ", " + fmt.Sprintf(format, args);
            if (strings.Contains(f, "\n"))
            {
                panic("feature contains newlines: " + f);
            }
            {
                var (_, dup) = w.features[f];

                if (dup)
                {
                    panic("duplicate feature inserted: " + f);
                }

            }
            w.features[f] = true;

            if (verbose.Value)
            {
                log.Printf("feature: %s", f);
            }
        });
    }
}
