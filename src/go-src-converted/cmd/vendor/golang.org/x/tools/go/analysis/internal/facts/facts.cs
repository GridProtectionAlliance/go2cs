// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package facts defines a serializable set of analysis.Fact.
//
// It provides a partial implementation of the Fact-related parts of the
// analysis.Pass interface for use in analysis drivers such as "go vet"
// and other build systems.
//
// The serial format is unspecified and may change, so the same version
// of this package must be used for reading and writing serialized facts.
//
// The handling of facts in the analysis system parallels the handling
// of type information in the compiler: during compilation of package P,
// the compiler emits an export data file that describes the type of
// every object (named thing) defined in package P, plus every object
// indirectly reachable from one of those objects. Thus the downstream
// compiler of package Q need only load one export data file per direct
// import of Q, and it will learn everything about the API of package P
// and everything it needs to know about the API of P's dependencies.
//
// Similarly, analysis of package P emits a fact set containing facts
// about all objects exported from P, plus additional facts about only
// those objects of P's dependencies that are reachable from the API of
// package P; the downstream analysis of Q need only load one fact set
// per direct import of Q.
//
// The notion of "exportedness" that matters here is that of the
// compiler. According to the language spec, a method pkg.T.f is
// unexported simply because its name starts with lowercase. But the
// compiler must nonetheless export f so that downstream compilations can
// accurately ascertain whether pkg.T implements an interface pkg.I
// defined as interface{f()}. Exported thus means "described in export
// data".
//

// package facts -- go2cs converted at 2022 March 13 06:41:40 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/internal/facts" ==> using facts = go.cmd.vendor.golang.org.x.tools.go.analysis.@internal.facts_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\internal\facts\facts.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.@internal;

using bytes = bytes_package;
using gob = encoding.gob_package;
using fmt = fmt_package;
using types = go.types_package;
using ioutil = io.ioutil_package;
using log = log_package;
using reflect = reflect_package;
using sort = sort_package;
using sync = sync_package;

using analysis = golang.org.x.tools.go.analysis_package;
using objectpath = golang.org.x.tools.go.types.objectpath_package;
using System;

public static partial class facts_package {

private static readonly var debug = false;

// A Set is a set of analysis.Facts.
//
// Decode creates a Set of facts by reading from the imports of a given
// package, and Encode writes out the set. Between these operation,
// the Import and Export methods will query and update the set.
//
// All of Set's methods except String are safe to call concurrently.


// A Set is a set of analysis.Facts.
//
// Decode creates a Set of facts by reading from the imports of a given
// package, and Encode writes out the set. Between these operation,
// the Import and Export methods will query and update the set.
//
// All of Set's methods except String are safe to call concurrently.
public partial struct Set {
    public ptr<types.Package> pkg;
    public sync.Mutex mu;
    public map<key, analysis.Fact> m;
}

private partial struct key {
    public ptr<types.Package> pkg;
    public types.Object obj; // (object facts only)
    public reflect.Type t;
}

// ImportObjectFact implements analysis.Pass.ImportObjectFact.
private static bool ImportObjectFact(this ptr<Set> _addr_s, types.Object obj, analysis.Fact ptr) => func((defer, panic, _) => {
    ref Set s = ref _addr_s.val;

    if (obj == null) {
        panic("nil object");
    }
    key key = new key(pkg:obj.Pkg(),obj:obj,t:reflect.TypeOf(ptr));
    s.mu.Lock();
    defer(s.mu.Unlock());
    {
        var (v, ok) = s.m[key];

        if (ok) {
            reflect.ValueOf(ptr).Elem().Set(reflect.ValueOf(v).Elem());
            return true;
        }
    }
    return false;
});

// ExportObjectFact implements analysis.Pass.ExportObjectFact.
private static void ExportObjectFact(this ptr<Set> _addr_s, types.Object obj, analysis.Fact fact) {
    ref Set s = ref _addr_s.val;

    if (obj.Pkg() != s.pkg) {
        log.Panicf("in package %s: ExportObjectFact(%s, %T): can't set fact on object belonging another package", s.pkg, obj, fact);
    }
    key key = new key(pkg:obj.Pkg(),obj:obj,t:reflect.TypeOf(fact));
    s.mu.Lock();
    s.m[key] = fact; // clobber any existing entry
    s.mu.Unlock();
}

private static slice<analysis.ObjectFact> AllObjectFacts(this ptr<Set> _addr_s, map<reflect.Type, bool> filter) {
    ref Set s = ref _addr_s.val;

    slice<analysis.ObjectFact> facts = default;
    s.mu.Lock();
    foreach (var (k, v) in s.m) {
        if (k.obj != null && filter[k.t]) {
            facts = append(facts, new analysis.ObjectFact(Object:k.obj,Fact:v));
        }
    }    s.mu.Unlock();
    return facts;
}

// ImportPackageFact implements analysis.Pass.ImportPackageFact.
private static bool ImportPackageFact(this ptr<Set> _addr_s, ptr<types.Package> _addr_pkg, analysis.Fact ptr) => func((defer, panic, _) => {
    ref Set s = ref _addr_s.val;
    ref types.Package pkg = ref _addr_pkg.val;

    if (pkg == null) {
        panic("nil package");
    }
    key key = new key(pkg:pkg,t:reflect.TypeOf(ptr));
    s.mu.Lock();
    defer(s.mu.Unlock());
    {
        var (v, ok) = s.m[key];

        if (ok) {
            reflect.ValueOf(ptr).Elem().Set(reflect.ValueOf(v).Elem());
            return true;
        }
    }
    return false;
});

// ExportPackageFact implements analysis.Pass.ExportPackageFact.
private static void ExportPackageFact(this ptr<Set> _addr_s, analysis.Fact fact) {
    ref Set s = ref _addr_s.val;

    key key = new key(pkg:s.pkg,t:reflect.TypeOf(fact));
    s.mu.Lock();
    s.m[key] = fact; // clobber any existing entry
    s.mu.Unlock();
}

private static slice<analysis.PackageFact> AllPackageFacts(this ptr<Set> _addr_s, map<reflect.Type, bool> filter) {
    ref Set s = ref _addr_s.val;

    slice<analysis.PackageFact> facts = default;
    s.mu.Lock();
    foreach (var (k, v) in s.m) {
        if (k.obj == null && filter[k.t]) {
            facts = append(facts, new analysis.PackageFact(Package:k.pkg,Fact:v));
        }
    }    s.mu.Unlock();
    return facts;
}

// gobFact is the Gob declaration of a serialized fact.
private partial struct gobFact {
    public @string PkgPath; // path of package
    public objectpath.Path Object; // optional path of object relative to package itself
    public analysis.Fact Fact; // type and value of user-defined Fact
}

// Decode decodes all the facts relevant to the analysis of package pkg.
// The read function reads serialized fact data from an external source
// for one of of pkg's direct imports. The empty file is a valid
// encoding of an empty fact set.
//
// It is the caller's responsibility to call gob.Register on all
// necessary fact types.
public static (ptr<Set>, error) Decode(ptr<types.Package> _addr_pkg, Func<@string, (slice<byte>, error)> read) {
    ptr<Set> _p0 = default!;
    error _p0 = default!;
    ref types.Package pkg = ref _addr_pkg.val;
 
    // Compute the import map for this package.
    // See the package doc comment.
    var packages = importMap(pkg.Imports()); 

    // Read facts from imported packages.
    // Facts may describe indirectly imported packages, or their objects.
    var m = make_map<key, analysis.Fact>(); // one big bucket
    foreach (var (_, imp) in pkg.Imports()) {
        Action<@string, object[]> logf = (format, args) => {
            if (debug) {
                var prefix = fmt.Sprintf("in %s, importing %s: ", pkg.Path(), imp.Path());
                log.Print(prefix, fmt.Sprintf(format, args));
            }
        }; 

        // Read the gob-encoded facts.
        var (data, err) = read(imp.Path());
        if (err != null) {
            return (_addr_null!, error.As(fmt.Errorf("in %s, can't import facts for package %q: %v", pkg.Path(), imp.Path(), err))!);
        }
        if (len(data) == 0) {
            continue; // no facts
        }
        ref slice<gobFact> gobFacts = ref heap(out ptr<slice<gobFact>> _addr_gobFacts);
        {
            var err = gob.NewDecoder(bytes.NewReader(data)).Decode(_addr_gobFacts);

            if (err != null) {
                return (_addr_null!, error.As(fmt.Errorf("decoding facts for %q: %v", imp.Path(), err))!);
            }

        }
        if (debug) {
            logf("decoded %d facts: %v", len(gobFacts), gobFacts);
        }
        foreach (var (_, f) in gobFacts) {
            var factPkg = packages[f.PkgPath];
            if (factPkg == null) { 
                // Fact relates to a dependency that was
                // unused in this translation unit. Skip.
                logf("no package %q; discarding %v", f.PkgPath, f.Fact);
                continue;
            }
            key key = new key(pkg:factPkg,t:reflect.TypeOf(f.Fact));
            if (f.Object != "") { 
                // object fact
                var (obj, err) = objectpath.Object(factPkg, f.Object);
                if (err != null) { 
                    // (most likely due to unexported object)
                    // TODO(adonovan): audit for other possibilities.
                    logf("no object for path: %v; discarding %s", err, f.Fact);
                    continue;
                }
                key.obj = obj;
                logf("read %T fact %s for %v", f.Fact, f.Fact, key.obj);
            }
            else
 { 
                // package fact
                logf("read %T fact %s for %v", f.Fact, f.Fact, factPkg);
            }
            m[key] = f.Fact;
        }
    }    return (addr(new Set(pkg:pkg,m:m)), error.As(null!)!);
}

// Encode encodes a set of facts to a memory buffer.
//
// It may fail if one of the Facts could not be gob-encoded, but this is
// a sign of a bug in an Analyzer.
private static slice<byte> Encode(this ptr<Set> _addr_s) {
    ref Set s = ref _addr_s.val;

    // TODO(adonovan): opt: use a more efficient encoding
    // that avoids repeating PkgPath for each fact.

    // Gather all facts, including those from imported packages.
    slice<gobFact> gobFacts = default;

    s.mu.Lock();
    {
        var fact__prev1 = fact;

        foreach (var (__k, __fact) in s.m) {
            k = __k;
            fact = __fact;
            if (debug) {
                log.Printf("%v => %s\n", k, fact);
            }
            objectpath.Path @object = default;
            if (k.obj != null) {
                var (path, err) = objectpath.For(k.obj);
                if (err != null) {
                    if (debug) {
                        log.Printf("discarding fact %s about %s\n", fact, k.obj);
                    }
                    continue; // object not accessible from package API; discard fact
                }
                object = path;
            }
            gobFacts = append(gobFacts, new gobFact(PkgPath:k.pkg.Path(),Object:object,Fact:fact,));
        }
        fact = fact__prev1;
    }

    s.mu.Unlock(); 

    // Sort facts by (package, object, type) for determinism.
    sort.Slice(gobFacts, (i, j) => {
        var x = gobFacts[i];
        var y = gobFacts[j];
        if (x.PkgPath != y.PkgPath) {
            return x.PkgPath < y.PkgPath;
        }
        if (x.Object != y.Object) {
            return x.Object < y.Object;
        }
        var tx = reflect.TypeOf(x.Fact);
        var ty = reflect.TypeOf(y.Fact);
        if (tx != ty) {
            return tx.String() < ty.String();
        }
        return false; // equal
    });

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    if (len(gobFacts) > 0) {
        {
            var err__prev2 = err;

            var err = gob.NewEncoder(_addr_buf).Encode(gobFacts);

            if (err != null) { 
                // Fact encoding should never fail. Identify the culprit.
                foreach (var (_, gf) in gobFacts) {
                    {
                        var err__prev3 = err;

                        err = gob.NewEncoder(ioutil.Discard).Encode(gf);

                        if (err != null) {
                            var fact = gf.Fact;
                            var pkgpath = reflect.TypeOf(fact).Elem().PkgPath();
                            log.Panicf("internal error: gob encoding of analysis fact %s failed: %v; please report a bug against fact %T in package %q", fact, err, fact, pkgpath);
                        }

                        err = err__prev3;

                    }
                }
            }

            err = err__prev2;

        }
    }
    if (debug) {
        log.Printf("package %q: encode %d facts, %d bytes\n", s.pkg.Path(), len(gobFacts), buf.Len());
    }
    return buf.Bytes();
}

// String is provided only for debugging, and must not be called
// concurrent with any Import/Export method.
private static @string String(this ptr<Set> _addr_s) {
    ref Set s = ref _addr_s.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    buf.WriteString("{");
    foreach (var (k, f) in s.m) {
        if (buf.Len() > 1) {
            buf.WriteString(", ");
        }
        if (k.obj != null) {
            buf.WriteString(k.obj.String());
        }
        else
 {
            buf.WriteString(k.pkg.Path());
        }
        fmt.Fprintf(_addr_buf, ": %v", f);
    }    buf.WriteString("}");
    return buf.String();
}

} // end facts_package
