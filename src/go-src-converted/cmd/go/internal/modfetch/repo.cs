// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfetch -- go2cs converted at 2022 March 13 06:32:19 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modfetch\repo.go
namespace go.cmd.go.@internal;

using fmt = fmt_package;
using io = io_package;
using fs = io.fs_package;
using os = os_package;
using strconv = strconv_package;
using time = time_package;

using cfg = cmd.go.@internal.cfg_package;
using codehost = cmd.go.@internal.modfetch.codehost_package;
using par = cmd.go.@internal.par_package;
using vcs = cmd.go.@internal.vcs_package;
using web = cmd.go.@internal.web_package;

using module = golang.org.x.mod.module_package;
using System.ComponentModel;
using System;

public static partial class modfetch_package {

private static readonly var traceRepo = false; // trace all repo actions, for debugging

// A Repo represents a repository storing all versions of a single module.
// It must be safe for simultaneous use by multiple goroutines.
 // trace all repo actions, for debugging

// A Repo represents a repository storing all versions of a single module.
// It must be safe for simultaneous use by multiple goroutines.
public partial interface Repo {
    error ModulePath(); // Versions lists all known versions with the given prefix.
// Pseudo-versions are not included.
//
// Versions should be returned sorted in semver order
// (implementations can use semver.Sort).
//
// Versions returns a non-nil error only if there was a problem
// fetching the list of versions: it may return an empty list
// along with a nil error if the list of matching versions
// is known to be empty.
//
// If the underlying repository does not exist,
// Versions returns an error matching errors.Is(_, os.NotExist).
    error Versions(@string prefix); // Stat returns information about the revision rev.
// A revision can be any identifier known to the underlying service:
// commit hash, branch, tag, and so on.
    error Stat(@string rev); // Latest returns the latest revision on the default branch,
// whatever that means in the underlying source code repository.
// It is only used when there are no tagged versions.
    error Latest(); // GoMod returns the go.mod file for the given version.
    error GoMod(@string version); // Zip writes a zip file for the given version to dst.
    error Zip(io.Writer dst, @string version);
}

// A Rev describes a single revision in a module repository.
public partial struct RevInfo {
    public @string Version; // suggested version string for this revision
    public time.Time Time; // commit time

// These fields are used for Stat of arbitrary rev,
// but they are not recorded when talking about module versions.
    [Description("json:\"-\"")]
    public @string Name; // complete ID in underlying repository
    [Description("json:\"-\"")]
    public @string Short; // shortened ID, for use in pseudo-version
}

// Re: module paths, import paths, repository roots, and lookups
//
// A module is a collection of Go packages stored in a file tree
// with a go.mod file at the root of the tree.
// The go.mod defines the module path, which is the import path
// corresponding to the root of the file tree.
// The import path of a directory within that file tree is the module path
// joined with the name of the subdirectory relative to the root.
//
// For example, the module with path rsc.io/qr corresponds to the
// file tree in the repository https://github.com/rsc/qr.
// That file tree has a go.mod that says "module rsc.io/qr".
// The package in the root directory has import path "rsc.io/qr".
// The package in the gf256 subdirectory has import path "rsc.io/qr/gf256".
// In this example, "rsc.io/qr" is both a module path and an import path.
// But "rsc.io/qr/gf256" is only an import path, not a module path:
// it names an importable package, but not a module.
//
// As a special case to incorporate code written before modules were
// introduced, if a path p resolves using the pre-module "go get" lookup
// to the root of a source code repository without a go.mod file,
// that repository is treated as if it had a go.mod in its root directory
// declaring module path p. (The go.mod is further considered to
// contain requirements corresponding to any legacy version
// tracking format such as Gopkg.lock, vendor/vendor.conf, and so on.)
//
// The presentation so far ignores the fact that a source code repository
// has many different versions of a file tree, and those versions may
// differ in whether a particular go.mod exists and what it contains.
// In fact there is a well-defined mapping only from a module path, version
// pair - often written path@version - to a particular file tree.
// For example rsc.io/qr@v0.1.0 depends on the "implicit go.mod at root of
// repository" rule, while rsc.io/qr@v0.2.0 has an explicit go.mod.
// Because the "go get" import paths rsc.io/qr and github.com/rsc/qr
// both redirect to the Git repository https://github.com/rsc/qr,
// github.com/rsc/qr@v0.1.0 is the same file tree as rsc.io/qr@v0.1.0
// but a different module (a different name). In contrast, since v0.2.0
// of that repository has an explicit go.mod that declares path rsc.io/qr,
// github.com/rsc/qr@v0.2.0 is an invalid module path, version pair.
// Before modules, import comments would have had the same effect.
//
// The set of import paths associated with a given module path is
// clearly not fixed: at the least, new directories with new import paths
// can always be added. But another potential operation is to split a
// subtree out of a module into its own module. If done carefully,
// this operation can be done while preserving compatibility for clients.
// For example, suppose that we want to split rsc.io/qr/gf256 into its
// own module, so that there would be two modules rsc.io/qr and rsc.io/qr/gf256.
// Then we can simultaneously issue rsc.io/qr v0.3.0 (dropping the gf256 subdirectory)
// and rsc.io/qr/gf256 v0.1.0, including in their respective go.mod
// cyclic requirements pointing at each other: rsc.io/qr v0.3.0 requires
// rsc.io/qr/gf256 v0.1.0 and vice versa. Then a build can be
// using an older rsc.io/qr module that includes the gf256 package, but if
// it adds a requirement on either the newer rsc.io/qr or the newer
// rsc.io/qr/gf256 module, it will automatically add the requirement
// on the complementary half, ensuring both that rsc.io/qr/gf256 is
// available for importing by the build and also that it is only defined
// by a single module. The gf256 package could move back into the
// original by another simultaneous release of rsc.io/qr v0.4.0 including
// the gf256 subdirectory and an rsc.io/qr/gf256 v0.2.0 with no code
// in its root directory, along with a new requirement cycle.
// The ability to shift module boundaries in this way is expected to be
// important in large-scale program refactorings, similar to the ones
// described in https://talks.golang.org/2016/refactor.article.
//
// The possibility of shifting module boundaries reemphasizes
// that you must know both the module path and its version
// to determine the set of packages provided directly by that module.
//
// On top of all this, it is possible for a single code repository
// to contain multiple modules, either in branches or subdirectories,
// as a limited kind of monorepo. For example rsc.io/qr/v2,
// the v2.x.x continuation of rsc.io/qr, is expected to be found
// in v2-tagged commits in https://github.com/rsc/qr, either
// in the root or in a v2 subdirectory, disambiguated by go.mod.
// Again the precise file tree corresponding to a module
// depends on which version we are considering.
//
// It is also possible for the underlying repository to change over time,
// without changing the module path. If I copy the github repo over
// to https://bitbucket.org/rsc/qr and update https://rsc.io/qr?go-get=1,
// then clients of all versions should start fetching from bitbucket
// instead of github. That is, in contrast to the exact file tree,
// the location of the source code repository associated with a module path
// does not depend on the module version. (This is by design, as the whole
// point of these redirects is to allow package authors to establish a stable
// name that can be updated as code moves from one service to another.)
//
// All of this is important background for the lookup APIs defined in this
// file.
//
// The Lookup function takes a module path and returns a Repo representing
// that module path. Lookup can do only a little with the path alone.
// It can check that the path is well-formed (see semver.CheckPath)
// and it can check that the path can be resolved to a target repository.
// To avoid version control access except when absolutely necessary,
// Lookup does not attempt to connect to the repository itself.

private static par.Cache lookupCache = default;

private partial struct lookupCacheKey {
    public @string proxy;
    public @string path;
}

// Lookup returns the module with the given module path,
// fetched through the given proxy.
//
// The distinguished proxy "direct" indicates that the path should be fetched
// from its origin, and "noproxy" indicates that the patch should be fetched
// directly only if GONOPROXY matches the given path.
//
// For the distinguished proxy "off", Lookup always returns a Repo that returns
// a non-nil error for every method call.
//
// A successful return does not guarantee that the module
// has any defined versions.
public static Repo Lookup(@string proxy, @string path) => func((defer, _, _) => {
    if (traceRepo) {
        defer(logCall("Lookup(%q, %q)", proxy, path)());
    }
    private partial struct cached {
        public Repo r;
    }
    cached c = lookupCache.Do(new lookupCacheKey(proxy,path), () => {
        var r = newCachingRepo(path, () => {
            var (r, err) = lookup(proxy, path);
            if (err == null && traceRepo) {
                r = newLoggingRepo(r);
            }
            return (r, err);
        });
        return new cached(r);
    })._<cached>();

    return c.r;
});

// lookup returns the module with the given module path.
private static (Repo, error) lookup(@string proxy, @string path) {
    Repo r = default;
    error err = default!;

    if (cfg.BuildMod == "vendor") {
        return (null, error.As(errLookupDisabled)!);
    }
    if (module.MatchPrefixPatterns(cfg.GONOPROXY, path)) {
        switch (proxy) {
            case "noproxy": 

            case "direct": 
                return lookupDirect(path);
                break;
            default: 
                return (null, error.As(errNoproxy)!);
                break;
        }
    }
    switch (proxy) {
        case "off": 
            return (new errRepo(path,errProxyOff), error.As(null!)!);
            break;
        case "direct": 
            return lookupDirect(path);
            break;
        case "noproxy": 
            return (null, error.As(errUseProxy)!);
            break;
        default: 
            return newProxyRepo(proxy, path);
            break;
    }
}

private partial struct lookupDisabledError {
}

private static @string Error(this lookupDisabledError _p0) {
    if (cfg.BuildModReason == "") {
        return fmt.Sprintf("module lookup disabled by -mod=%s", cfg.BuildMod);
    }
    return fmt.Sprintf("module lookup disabled by -mod=%s\n\t(%s)", cfg.BuildMod, cfg.BuildModReason);
}

private static error errLookupDisabled = error.As(new lookupDisabledError())!;

private static var errProxyOff = notExistErrorf("module lookup disabled by GOPROXY=off");private static error errNoproxy = error.As(notExistErrorf("disabled by GOPRIVATE/GONOPROXY"))!;private static error errUseProxy = error.As(notExistErrorf("path does not match GOPRIVATE/GONOPROXY"))!;

private static (Repo, error) lookupDirect(@string path) {
    Repo _p0 = default;
    error _p0 = default!;

    var security = web.SecureOnly;

    if (module.MatchPrefixPatterns(cfg.GOINSECURE, path)) {
        security = web.Insecure;
    }
    var (rr, err) = vcs.RepoRootForImportPath(path, vcs.PreferMod, security);
    if (err != null) { 
        // We don't know where to find code for a module with this path.
        return (null, error.As(new notExistError(err:err))!);
    }
    if (rr.VCS.Name == "mod") { 
        // Fetch module from proxy with base URL rr.Repo.
        return newProxyRepo(rr.Repo, path);
    }
    var (code, err) = lookupCodeRepo(_addr_rr);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return newCodeRepo(code, rr.Root, path);
}

private static (codehost.Repo, error) lookupCodeRepo(ptr<vcs.RepoRoot> _addr_rr) {
    codehost.Repo _p0 = default;
    error _p0 = default!;
    ref vcs.RepoRoot rr = ref _addr_rr.val;

    var (code, err) = codehost.NewRepo(rr.VCS.Cmd, rr.Repo);
    if (err != null) {
        {
            ptr<codehost.VCSError> (_, ok) = err._<ptr<codehost.VCSError>>();

            if (ok) {
                return (null, error.As(err)!);
            }

        }
        return (null, error.As(fmt.Errorf("lookup %s: %v", rr.Root, err))!);
    }
    return (code, error.As(null!)!);
}

// A loggingRepo is a wrapper around an underlying Repo
// that prints a log message at the start and end of each call.
// It can be inserted when debugging.
private partial struct loggingRepo {
    public Repo r;
}

private static ptr<loggingRepo> newLoggingRepo(Repo r) {
    return addr(new loggingRepo(r));
}

// logCall prints a log message using format and args and then
// also returns a function that will print the same message again,
// along with the elapsed time.
// Typical usage is:
//
//    defer logCall("hello %s", arg)()
//
// Note the final ().
private static Action logCall(@string format, params object[] args) {
    args = args.Clone();

    var start = time.Now();
    fmt.Fprintf(os.Stderr, "+++ %s\n", fmt.Sprintf(format, args));
    return () => {
        fmt.Fprintf(os.Stderr, "%.3fs %s\n", time.Since(start).Seconds(), fmt.Sprintf(format, args));
    };
}

private static @string ModulePath(this ptr<loggingRepo> _addr_l) {
    ref loggingRepo l = ref _addr_l.val;

    return l.r.ModulePath();
}

private static (slice<@string>, error) Versions(this ptr<loggingRepo> _addr_l, @string prefix) => func((defer, _, _) => {
    slice<@string> tags = default;
    error err = default!;
    ref loggingRepo l = ref _addr_l.val;

    defer(logCall("Repo[%s]: Versions(%q)", l.r.ModulePath(), prefix)());
    return l.r.Versions(prefix);
});

private static (ptr<RevInfo>, error) Stat(this ptr<loggingRepo> _addr_l, @string rev) => func((defer, _, _) => {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref loggingRepo l = ref _addr_l.val;

    defer(logCall("Repo[%s]: Stat(%q)", l.r.ModulePath(), rev)());
    return _addr_l.r.Stat(rev)!;
});

private static (ptr<RevInfo>, error) Latest(this ptr<loggingRepo> _addr_l) => func((defer, _, _) => {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref loggingRepo l = ref _addr_l.val;

    defer(logCall("Repo[%s]: Latest()", l.r.ModulePath())());
    return _addr_l.r.Latest()!;
});

private static (slice<byte>, error) GoMod(this ptr<loggingRepo> _addr_l, @string version) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref loggingRepo l = ref _addr_l.val;

    defer(logCall("Repo[%s]: GoMod(%q)", l.r.ModulePath(), version)());
    return l.r.GoMod(version);
});

private static error Zip(this ptr<loggingRepo> _addr_l, io.Writer dst, @string version) => func((defer, _, _) => {
    ref loggingRepo l = ref _addr_l.val;

    @string dstName = "_";
    {

        if (ok) {
            dstName = strconv.Quote(dst.Name());
        }
    }
    defer(logCall("Repo[%s]: Zip(%s, %q)", l.r.ModulePath(), dstName, version)());
    return error.As(l.r.Zip(dst, version))!;
});

// errRepo is a Repo that returns the same error for all operations.
//
// It is useful in conjunction with caching, since cache hits will not attempt
// the prohibited operations.
private partial struct errRepo {
    public @string modulePath;
    public error err;
}

private static @string ModulePath(this errRepo r) {
    return r.modulePath;
}

private static (slice<@string>, error) Versions(this errRepo r, @string prefix) {
    slice<@string> tags = default;
    error err = default!;

    return (null, error.As(r.err)!);
}
private static (ptr<RevInfo>, error) Stat(this errRepo r, @string rev) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(r.err)!);
}
private static (ptr<RevInfo>, error) Latest(this errRepo r) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(r.err)!);
}
private static (slice<byte>, error) GoMod(this errRepo r, @string version) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return (null, error.As(r.err)!);
}
private static error Zip(this errRepo r, io.Writer dst, @string version) {
    return error.As(r.err)!;
}

// A notExistError is like fs.ErrNotExist, but with a custom message
private partial struct notExistError {
    public error err;
}

private static error notExistErrorf(@string format, params object[] args) {
    args = args.Clone();

    return error.As(new notExistError(fmt.Errorf(format,args...)))!;
}

private static @string Error(this notExistError e) {
    return e.err.Error();
}

private static bool Is(this notExistError _p0, error target) {
    return target == fs.ErrNotExist;
}

private static error Unwrap(this notExistError e) {
    return error.As(e.err)!;
}

} // end modfetch_package
