// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 13 06:31:51 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\query.go
namespace go.cmd.go.@internal;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using fs = io.fs_package;
using os = os_package;
using pathpkg = path_package;
using sort = sort_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;

using cfg = cmd.go.@internal.cfg_package;
using imports = cmd.go.@internal.imports_package;
using modfetch = cmd.go.@internal.modfetch_package;
using search = cmd.go.@internal.search_package;
using str = cmd.go.@internal.str_package;
using trace = cmd.go.@internal.trace_package;

using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;


// Query looks up a revision of a given module given a version query string.
// The module must be a complete module path.
// The version must take one of the following forms:
//
// - the literal string "latest", denoting the latest available, allowed
//   tagged version, with non-prereleases preferred over prereleases.
//   If there are no tagged versions in the repo, latest returns the most
//   recent commit.
// - the literal string "upgrade", equivalent to "latest" except that if
//   current is a newer version, current will be returned (see below).
// - the literal string "patch", denoting the latest available tagged version
//   with the same major and minor number as current (see below).
// - v1, denoting the latest available tagged version v1.x.x.
// - v1.2, denoting the latest available tagged version v1.2.x.
// - v1.2.3, a semantic version string denoting that tagged version.
// - <v1.2.3, <=v1.2.3, >v1.2.3, >=v1.2.3,
//   denoting the version closest to the target and satisfying the given operator,
//   with non-prereleases preferred over prereleases.
// - a repository commit identifier or tag, denoting that commit.
//
// current denotes the currently-selected version of the module; it may be
// "none" if no version is currently selected, or "" if the currently-selected
// version is unknown or should not be considered. If query is
// "upgrade" or "patch", current will be returned if it is a newer
// semantic version or a chronologically later pseudo-version than the
// version that would otherwise be chosen. This prevents accidental downgrades
// from newer pre-release or development versions.
//
// The allowed function (which may be nil) is used to filter out unsuitable
// versions (see AllowedFunc documentation for details). If the query refers to
// a specific revision (for example, "master"; see IsRevisionQuery), and the
// revision is disallowed by allowed, Query returns the error. If the query
// does not refer to a specific revision (for example, "latest"), Query
// acts as if versions disallowed by allowed do not exist.
//
// If path is the path of the main module and the query is "latest",
// Query returns Target.Version as the version.

using System;
using System.Threading;
public static partial class modload_package {

public static (ptr<modfetch.RevInfo>, error) Query(context.Context ctx, @string path, @string query, @string current, AllowedFunc allowed) {
    ptr<modfetch.RevInfo> _p0 = default!;
    error _p0 = default!;

    ptr<modfetch.RevInfo> info;
    var err = modfetch.TryProxies(proxy => {
        info, err = queryProxy(ctx, proxy, path, query, current, allowed);
        return _addr_err!;
    });
    return (_addr_info!, error.As(err)!);
}

// AllowedFunc is used by Query and other functions to filter out unsuitable
// versions, for example, those listed in exclude directives in the main
// module's go.mod file.
//
// An AllowedFunc returns an error equivalent to ErrDisallowed for an unsuitable
// version. Any other error indicates the function was unable to determine
// whether the version should be allowed, for example, the function was unable
// to fetch or parse a go.mod file containing retractions. Typically, errors
// other than ErrDisallowd may be ignored.
public delegate  error AllowedFunc(context.Context,  module.Version);

private static error errQueryDisabled = error.As(new queryDisabledError())!;

private partial struct queryDisabledError {
}

private static @string Error(this queryDisabledError _p0) {
    if (cfg.BuildModReason == "") {
        return fmt.Sprintf("cannot query module due to -mod=%s", cfg.BuildMod);
    }
    return fmt.Sprintf("cannot query module due to -mod=%s\n\t(%s)", cfg.BuildMod, cfg.BuildModReason);
}

private static (ptr<modfetch.RevInfo>, error) queryProxy(context.Context ctx, @string proxy, @string path, @string query, @string current, AllowedFunc allowed) => func((defer, _, _) => {
    ptr<modfetch.RevInfo> _p0 = default!;
    error _p0 = default!;

    var (ctx, span) = trace.StartSpan(ctx, "modload.queryProxy " + path + " " + query);
    defer(span.Done());

    if (current != "" && current != "none" && !semver.IsValid(current)) {
        return (_addr_null!, error.As(fmt.Errorf("invalid previous version %q", current))!);
    }
    if (cfg.BuildMod == "vendor") {
        return (_addr_null!, error.As(errQueryDisabled)!);
    }
    if (allowed == null) {
        allowed = (_p0, _p0) => _addr_null!;
    }
    if (path == Target.Path && (query == "upgrade" || query == "patch")) {
        {
            var err__prev2 = err;

            var err = allowed(ctx, Target);

            if (err != null) {
                return (_addr_null!, error.As(fmt.Errorf("internal error: main module version is not allowed: %w", err))!);
            }

            err = err__prev2;

        }
        return (addr(new modfetch.RevInfo(Version:Target.Version)), error.As(null!)!);
    }
    if (path == "std" || path == "cmd") {
        return (_addr_null!, error.As(fmt.Errorf("can't query specific version (%q) of standard-library module %q", query, path))!);
    }
    var (repo, err) = lookupRepo(proxy, path);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (qm, err) = newQueryMatcher(path, query, current, allowed);
    if ((err == null && qm.canStat) || err == errRevQuery) { 
        // Direct lookup of a commit identifier or complete (non-prefix) semantic
        // version.

        // If the identifier is not a canonical semver tag — including if it's a
        // semver tag with a +metadata suffix — then modfetch.Stat will populate
        // info.Version with a suitable pseudo-version.
        var (info, err) = repo.Stat(query);
        if (err != null) {
            var queryErr = err; 
            // The full query doesn't correspond to a tag. If it is a semantic version
            // with a +metadata suffix, see if there is a tag without that suffix:
            // semantic versioning defines them to be equivalent.
            var canonicalQuery = module.CanonicalVersion(query);
            if (canonicalQuery != "" && query != canonicalQuery) {
                info, err = repo.Stat(canonicalQuery);
                if (err != null && !errors.Is(err, fs.ErrNotExist)) {
                    return (_addr_info!, error.As(err)!);
                }
            }
            if (err != null) {
                return (_addr_null!, error.As(queryErr)!);
            }
        }
        {
            var err__prev2 = err;

            err = allowed(ctx, new module.Version(Path:path,Version:info.Version));

            if (errors.Is(err, ErrDisallowed)) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }
        return (_addr_info!, error.As(null!)!);
    }
    else if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (versions, err) = repo.Versions(qm.prefix);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (releases, prereleases, err) = qm.filterVersions(ctx, versions);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    Func<@string, (ptr<modfetch.RevInfo>, error)> lookup = v => {
        var (rev, err) = repo.Stat(v);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        if ((query == "upgrade" || query == "patch") && module.IsPseudoVersion(current) && !rev.Time.IsZero()) { 
            // Don't allow "upgrade" or "patch" to move from a pseudo-version
            // to a chronologically older version or pseudo-version.
            //
            // If the current version is a pseudo-version from an untagged branch, it
            // may be semantically lower than the "latest" release or the latest
            // pseudo-version on the main branch. A user on such a version is unlikely
            // to intend to “upgrade” to a version that already existed at that point
            // in time.
            //
            // We do this only if the current version is a pseudo-version: if the
            // version is tagged, the author of the dependency module has given us
            // explicit information about their intended precedence of this version
            // relative to other versions, and we shouldn't contradict that
            // information. (For example, v1.0.1 might be a backport of a fix already
            // incorporated into v1.1.0, in which case v1.0.1 would be chronologically
            // newer but v1.1.0 is still an “upgrade”; or v1.0.2 might be a revert of
            // an unsuccessful fix in v1.0.1, in which case the v1.0.2 commit may be
            // older than the v1.0.1 commit despite the tag itself being newer.)
            var (currentTime, err) = module.PseudoVersionTime(current);
            if (err == null && rev.Time.Before(currentTime)) {
                {
                    var err__prev3 = err;

                    err = allowed(ctx, new module.Version(Path:path,Version:current));

                    if (errors.Is(err, ErrDisallowed)) {
                        return (_addr_null!, error.As(err)!);
                    }

                    err = err__prev3;

                }
                return _addr_repo.Stat(current)!;
            }
        }
        return (_addr_rev!, error.As(null!)!);
    };

    if (qm.preferLower) {
        if (len(releases) > 0) {
            return _addr_lookup(releases[0])!;
        }
        if (len(prereleases) > 0) {
            return _addr_lookup(prereleases[0])!;
        }
    }
    else
 {
        if (len(releases) > 0) {
            return _addr_lookup(releases[len(releases) - 1])!;
        }
        if (len(prereleases) > 0) {
            return _addr_lookup(prereleases[len(prereleases) - 1])!;
        }
    }
    if (qm.mayUseLatest) {
        var (latest, err) = repo.Latest();
        if (err == null) {
            if (qm.allowsVersion(ctx, latest.Version)) {
                return _addr_lookup(latest.Version)!;
            }
        }
        else if (!errors.Is(err, fs.ErrNotExist)) {
            return (_addr_null!, error.As(err)!);
        }
    }
    if ((query == "upgrade" || query == "patch") && current != "" && current != "none") { 
        // "upgrade" and "patch" may stay on the current version if allowed.
        {
            var err__prev2 = err;

            err = allowed(ctx, new module.Version(Path:path,Version:current));

            if (errors.Is(err, ErrDisallowed)) {
                return (_addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }
        return _addr_lookup(current)!;
    }
    return (_addr_null!, error.As(addr(new NoMatchingVersionError(query:query,current:current))!)!);
});

// IsRevisionQuery returns true if vers is a version query that may refer to
// a particular version or revision in a repository like "v1.0.0", "master",
// or "0123abcd". IsRevisionQuery returns false if vers is a query that
// chooses from among available versions like "latest" or ">v1.0.0".
public static bool IsRevisionQuery(@string vers) {
    if (vers == "latest" || vers == "upgrade" || vers == "patch" || strings.HasPrefix(vers, "<") || strings.HasPrefix(vers, ">") || (semver.IsValid(vers) && isSemverPrefix(vers))) {
        return false;
    }
    return true;
}

// isSemverPrefix reports whether v is a semantic version prefix: v1 or v1.2 (not v1.2.3).
// The caller is assumed to have checked that semver.IsValid(v) is true.
private static bool isSemverPrefix(@string v) {
    nint dots = 0;
    for (nint i = 0; i < len(v); i++) {
        switch (v[i]) {
            case '-': 

            case '+': 
                return false;
                break;
            case '.': 
                dots++;
                if (dots >= 2) {
                    return false;
                }
                break;
        }
    }
    return true;
}

private partial struct queryMatcher {
    public @string path;
    public @string prefix;
    public Func<@string, bool> filter;
    public AllowedFunc allowed;
    public bool canStat; // if true, the query can be resolved by repo.Stat
    public bool preferLower; // if true, choose the lowest matching version
    public bool mayUseLatest;
    public bool preferIncompatible;
}

private static var errRevQuery = errors.New("query refers to a non-semver revision");

// newQueryMatcher returns a new queryMatcher that matches the versions
// specified by the given query on the module with the given path.
//
// If the query can only be resolved by statting a non-SemVer revision,
// newQueryMatcher returns errRevQuery.
private static (ptr<queryMatcher>, error) newQueryMatcher(@string path, @string query, @string current, AllowedFunc allowed) {
    ptr<queryMatcher> _p0 = default!;
    error _p0 = default!;

    Func<@string, (ptr<queryMatcher>, error)> badVersion = v => (_addr_null!, error.As(fmt.Errorf("invalid semantic version %q in range %q", v, query))!);

    Func<@string, bool> matchesMajor = v => {
        var (_, pathMajor, ok) = module.SplitPathVersion(path);
        if (!ok) {
            return _addr_false!;
        }
        return _addr_module.CheckPathMajor(v, pathMajor) == null!;
    };

    ptr<queryMatcher> qm = addr(new queryMatcher(path:path,allowed:allowed,preferIncompatible:strings.HasSuffix(current,"+incompatible"),));


    if (query == "latest") 
        qm.mayUseLatest = true;
    else if (query == "upgrade") 
        if (current == "" || current == "none") {
            qm.mayUseLatest = true;
        }
        else
 {
            qm.mayUseLatest = module.IsPseudoVersion(current);
            qm.filter = mv => _addr_semver.Compare(mv, current) >= 0!;
        }
    else if (query == "patch") 
        if (current == "" || current == "none") {
            return (_addr_null!, error.As(addr(new NoPatchBaseError(path))!)!);
        }
        if (current == "") {
            qm.mayUseLatest = true;
        }
        else
 {
            qm.mayUseLatest = module.IsPseudoVersion(current);
            qm.prefix = semver.MajorMinor(current) + ".";
            qm.filter = mv => _addr_semver.Compare(mv, current) >= 0!;
        }
    else if (strings.HasPrefix(query, "<=")) 
        var v = query[(int)len("<=")..];
        if (!semver.IsValid(v)) {
            return _addr_badVersion(v)!;
        }
        if (isSemverPrefix(v)) { 
            // Refuse to say whether <=v1.2 allows v1.2.3 (remember, @v1.2 might mean v1.2.3).
            return (_addr_null!, error.As(fmt.Errorf("ambiguous semantic version %q in range %q", v, query))!);
        }
        qm.filter = mv => _addr_semver.Compare(mv, v) <= 0!;
        if (!matchesMajor(v)) {
            qm.preferIncompatible = true;
        }
    else if (strings.HasPrefix(query, "<")) 
        v = query[(int)len("<")..];
        if (!semver.IsValid(v)) {
            return _addr_badVersion(v)!;
        }
        qm.filter = mv => _addr_semver.Compare(mv, v) < 0!;
        if (!matchesMajor(v)) {
            qm.preferIncompatible = true;
        }
    else if (strings.HasPrefix(query, ">=")) 
        v = query[(int)len(">=")..];
        if (!semver.IsValid(v)) {
            return _addr_badVersion(v)!;
        }
        qm.filter = mv => _addr_semver.Compare(mv, v) >= 0!;
        qm.preferLower = true;
        if (!matchesMajor(v)) {
            qm.preferIncompatible = true;
        }
    else if (strings.HasPrefix(query, ">")) 
        v = query[(int)len(">")..];
        if (!semver.IsValid(v)) {
            return _addr_badVersion(v)!;
        }
        if (isSemverPrefix(v)) { 
            // Refuse to say whether >v1.2 allows v1.2.3 (remember, @v1.2 might mean v1.2.3).
            return (_addr_null!, error.As(fmt.Errorf("ambiguous semantic version %q in range %q", v, query))!);
        }
        qm.filter = mv => _addr_semver.Compare(mv, v) > 0!;
        qm.preferLower = true;
        if (!matchesMajor(v)) {
            qm.preferIncompatible = true;
        }
    else if (semver.IsValid(query)) 
        if (isSemverPrefix(query)) {
            qm.prefix = query + "."; 
            // Do not allow the query "v1.2" to match versions lower than "v1.2.0",
            // such as prereleases for that version. (https://golang.org/issue/31972)
            qm.filter = mv => _addr_semver.Compare(mv, query) >= 0!;
            }
        else;
        } {
            qm.canStat = true;
            qm.filter = mv => _addr_semver.Compare(mv, query) == 0!;
            qm.prefix = semver.Canonical(query);
        }
        if (!matchesMajor(query)) {
            qm.preferIncompatible = true;
        }
    else 
        return (_addr_null!, error.As(errRevQuery)!);
        return (_addr_qm!, error.As(null!)!);
}

// allowsVersion reports whether version v is allowed by the prefix, filter, and
// AllowedFunc of qm.
private static bool allowsVersion(this ptr<queryMatcher> _addr_qm, context.Context ctx, @string v) {
    ref queryMatcher qm = ref _addr_qm.val;

    if (qm.prefix != "" && !strings.HasPrefix(v, qm.prefix)) {
        return false;
    }
    if (qm.filter != null && !qm.filter(v)) {
        return false;
    }
    if (qm.allowed != null) {
        {
            var err = qm.allowed(ctx, new module.Version(Path:qm.path,Version:v));

            if (errors.Is(err, ErrDisallowed)) {
                return false;
            }

        }
    }
    return true;
}

// filterVersions classifies versions into releases and pre-releases, filtering
// out:
//     1. versions that do not satisfy the 'allowed' predicate, and
//     2. "+incompatible" versions, if a compatible one satisfies the predicate
//        and the incompatible version is not preferred.
//
// If the allowed predicate returns an error not equivalent to ErrDisallowed,
// filterVersions returns that error.
private static (slice<@string>, slice<@string>, error) filterVersions(this ptr<queryMatcher> _addr_qm, context.Context ctx, slice<@string> versions) {
    slice<@string> releases = default;
    slice<@string> prereleases = default;
    error err = default!;
    ref queryMatcher qm = ref _addr_qm.val;

    var needIncompatible = qm.preferIncompatible;

    @string lastCompatible = default;
    foreach (var (_, v) in versions) {
        if (!qm.allowsVersion(ctx, v)) {
            continue;
        }
        if (!needIncompatible) { 
            // We're not yet sure whether we need to include +incomptaible versions.
            // Keep track of the last compatible version we've seen, and use the
            // presence (or absence) of a go.mod file in that version to decide: a
            // go.mod file implies that the module author is supporting modules at a
            // compatible version (and we should ignore +incompatible versions unless
            // requested explicitly), while a lack of go.mod file implies the
            // potential for legacy (pre-modules) versioning without semantic import
            // paths (and thus *with* +incompatible versions).
            //
            // This isn't strictly accurate if the latest compatible version has been
            // replaced by a local file path, because we do not allow file-path
            // replacements without a go.mod file: the user would have needed to add
            // one. However, replacing the last compatible version while
            // simultaneously expecting to upgrade implicitly to a +incompatible
            // version seems like an extreme enough corner case to ignore for now.

            if (!strings.HasSuffix(v, "+incompatible")) {
                lastCompatible = v;
            }
            else if (lastCompatible != "") { 
                // If the latest compatible version is allowed and has a go.mod file,
                // ignore any version with a higher (+incompatible) major version. (See
                // https://golang.org/issue/34165.) Note that we even prefer a
                // compatible pre-release over an incompatible release.
                var (ok, err) = versionHasGoMod(ctx, new module.Version(Path:qm.path,Version:lastCompatible));
                if (err != null) {
                    return (null, null, error.As(err)!);
                }
                if (ok) { 
                    // The last compatible version has a go.mod file, so that's the
                    // highest version we're willing to consider. Don't bother even
                    // looking at higher versions, because they're all +incompatible from
                    // here onward.
                    break;
                } 

                // No acceptable compatible release has a go.mod file, so the versioning
                // for the module might not be module-aware, and we should respect
                // legacy major-version tags.
                needIncompatible = true;
            }
        }
        if (semver.Prerelease(v) != "") {
            prereleases = append(prereleases, v);
        }
        else
 {
            releases = append(releases, v);
        }
    }    return (releases, prereleases, error.As(null!)!);
}

public partial struct QueryResult {
    public module.Version Mod;
    public ptr<modfetch.RevInfo> Rev;
    public slice<@string> Packages;
}

// QueryPackages is like QueryPattern, but requires that the pattern match at
// least one package and omits the non-package result (if any).
public static (slice<QueryResult>, error) QueryPackages(context.Context ctx, @string pattern, @string query, Func<@string, @string> current, AllowedFunc allowed) {
    slice<QueryResult> _p0 = default;
    error _p0 = default!;

    var (pkgMods, modOnly, err) = QueryPattern(ctx, pattern, query, current, allowed);

    if (len(pkgMods) == 0 && err == null) {
        return (null, error.As(addr(new PackageNotInModuleError(Mod:modOnly.Mod,Replacement:Replacement(modOnly.Mod),Query:query,Pattern:pattern,))!)!);
    }
    return (pkgMods, error.As(err)!);
}

// QueryPattern looks up the module(s) containing at least one package matching
// the given pattern at the given version. The results are sorted by module path
// length in descending order. If any proxy provides a non-empty set of candidate
// modules, no further proxies are tried.
//
// For wildcard patterns, QueryPattern looks in modules with package paths up to
// the first "..." in the pattern. For the pattern "example.com/a/b.../c",
// QueryPattern would consider prefixes of "example.com/a".
//
// If any matching package is in the main module, QueryPattern considers only
// the main module and only the version "latest", without checking for other
// possible modules.
//
// QueryPattern always returns at least one QueryResult (which may be only
// modOnly) or a non-nil error.
public static (slice<QueryResult>, ptr<QueryResult>, error) QueryPattern(context.Context ctx, @string pattern, @string query, Func<@string, @string> current, AllowedFunc allowed) => func((defer, _, _) => {
    slice<QueryResult> pkgMods = default;
    ptr<QueryResult> modOnly = default!;
    error err = default!;

    var (ctx, span) = trace.StartSpan(ctx, "modload.QueryPattern " + pattern + " " + query);
    defer(span.Done());

    var @base = pattern;

    Func<ptr<search.Match>, error> firstError = m => {
        if (len(m.Errs) == 0) {
            return null;
        }
        return m.Errs[0];
    };

    Func<module.Version, @string, bool, ptr<search.Match>> match = default;
    var matchPattern = search.MatchPattern(pattern);

    {
        var i = strings.Index(pattern, "...");

        if (i >= 0) {
            base = pathpkg.Dir(pattern[..(int)i + 3]);
            if (base == ".") {
                return (null, _addr_null!, error.As(addr(new WildcardInFirstElementError(Pattern:pattern,Query:query))!)!);
            }
            match = (mod, root, isLocal) => {
                var m = search.NewMatch(pattern);
                matchPackages(ctx, m, imports.AnyTags(), omitStd, new slice<module.Version>(new module.Version[] { mod }));
                return m;
            }
        else
;
        } {
            match = (mod, root, isLocal) => {
                m = search.NewMatch(pattern);
                var prefix = mod.Path;
                if (mod == Target) {
                    prefix = targetPrefix;
                }
                {
                    var err__prev2 = err;

                    var (_, ok, err) = dirInModule(pattern, prefix, root, isLocal);

                    if (err != null) {
                        m.AddError(err);
                    }
                    else if (ok) {
                        m.Pkgs = new slice<@string>(new @string[] { pattern });
                    }

                    err = err__prev2;

                }
                return m;
            }
;
        }
    }

    bool queryMatchesMainModule = default;
    if (HasModRoot()) {
        m = match(Target, modRoot, true);
        if (len(m.Pkgs) > 0) {
            if (query != "upgrade" && query != "patch") {
                return (null, _addr_null!, error.As(addr(new QueryMatchesPackagesInMainModuleError(Pattern:pattern,Query:query,Packages:m.Pkgs,))!)!);
            }
            {
                var err__prev3 = err;

                var err = allowed(ctx, Target);

                if (err != null) {
                    return (null, _addr_null!, error.As(fmt.Errorf("internal error: package %s is in the main module (%s), but version is not allowed: %w", pattern, Target.Path, err))!);
                }

                err = err__prev3;

            }
            return (new slice<QueryResult>(new QueryResult[] { {Mod:Target,Rev:&modfetch.RevInfo{Version:Target.Version},Packages:m.Pkgs,} }), _addr_null!, error.As(null!)!);
        }
        {
            var err__prev2 = err;

            err = firstError(m);

            if (err != null) {
                return (null, _addr_null!, error.As(err)!);
            }

            err = err__prev2;

        }

        if (matchPattern(Target.Path)) {
            queryMatchesMainModule = true;
        }
        if ((query == "upgrade" || query == "patch") && queryMatchesMainModule) {
            {
                var err__prev3 = err;

                err = allowed(ctx, Target);

                if (err == null) {
                    modOnly = addr(new QueryResult(Mod:Target,Rev:&modfetch.RevInfo{Version:Target.Version},));
                }

                err = err__prev3;

            }
        }
    }
    slice<QueryResult> results = default;    var candidateModules = modulePrefixesExcludingTarget(base);
    if (len(candidateModules) == 0) {
        if (modOnly != null) {
            return (null, _addr_modOnly!, error.As(null!)!);
        }
        else if (queryMatchesMainModule) {
            return (null, _addr_null!, error.As(addr(new QueryMatchesMainModuleError(Pattern:pattern,Query:query,))!)!);
        }
        else
 {
            return (null, _addr_null!, error.As(addr(new PackageNotInModuleError(Mod:Target,Query:query,Pattern:pattern,))!)!);
        }
    }
    err = modfetch.TryProxies(proxy => {
        Func<context.Context, @string, (QueryResult, error)> queryModule = (ctx, path) => {
            (ctx, span) = trace.StartSpan(ctx, "modload.QueryPattern.queryModule [" + proxy + "] " + path);
            defer(span.Done());

            var pathCurrent = current(path);
            r.Mod.Path = path;
            r.Rev, err = queryProxy(ctx, proxy, path, query, pathCurrent, allowed);
            if (err != null) {
                return (r, _addr_err!);
            }
            r.Mod.Version = r.Rev.Version;
            var needSum = true;
            var (root, isLocal, err) = fetch(ctx, r.Mod, needSum);
            if (err != null) {
                return (r, _addr_err!);
            }
            m = match(r.Mod, root, isLocal);
            r.Packages = m.Pkgs;
            if (len(r.Packages) == 0 && !matchPattern(path)) {
                {
                    var err__prev2 = err;

                    err = firstError(m);

                    if (err != null) {
                        return (r, _addr_err!);
                    }

                    err = err__prev2;

                }
                return (r, addr(new PackageNotInModuleError(Mod:r.Mod,Replacement:Replacement(r.Mod),Query:query,Pattern:pattern,)));
            }
            return (r, _addr_null!);
        };

        var (allResults, err) = queryPrefixModules(ctx, candidateModules, queryModule);
        results = allResults[..(int)0];
        foreach (var (_, r) in allResults) {
            if (len(r.Packages) == 0) {
                modOnly = _addr_r;
            }
            else
 {
                results = append(results, r);
            }
        }        return err;
    });

    if (queryMatchesMainModule && len(results) == 0 && modOnly == null && errors.Is(err, fs.ErrNotExist)) {
        return (null, _addr_null!, error.As(addr(new QueryMatchesMainModuleError(Pattern:pattern,Query:query,))!)!);
    }
    return (results.slice(-1, len(results), len(results)), _addr_modOnly!, error.As(err)!);
});

// modulePrefixesExcludingTarget returns all prefixes of path that may plausibly
// exist as a module, excluding targetPrefix but otherwise including path
// itself, sorted by descending length. Prefixes that are not valid module paths
// but are valid package paths (like "m" or "example.com/.gen") are included,
// since they might be replaced.
private static slice<@string> modulePrefixesExcludingTarget(@string path) {
    var prefixes = make_slice<@string>(0, strings.Count(path, "/") + 1);

    while (true) {
        if (path != targetPrefix) {
            {
                var (_, _, ok) = module.SplitPathVersion(path);

                if (ok) {
                    prefixes = append(prefixes, path);
                }

            }
        }
        var j = strings.LastIndexByte(path, '/');
        if (j < 0) {
            break;
        }
        path = path[..(int)j];
    }

    return prefixes;
}

private static (slice<QueryResult>, error) queryPrefixModules(context.Context ctx, slice<@string> candidateModules, Func<context.Context, @string, (QueryResult, error)> queryModule) => func((defer, panic, _) => {
    slice<QueryResult> found = default;
    error err = default!;

    var (ctx, span) = trace.StartSpan(ctx, "modload.queryPrefixModules");
    defer(span.Done()); 

    // If the path we're attempting is not in the module cache and we don't have a
    // fetch result cached either, we'll end up making a (potentially slow)
    // request to the proxy or (often even slower) the origin server.
    // To minimize latency, execute all of those requests in parallel.
    private partial struct result {
        public ref QueryResult QueryResult => ref QueryResult_val;
        public error err;
    }
    var results = make_slice<result>(len(candidateModules));
    sync.WaitGroup wg = default;
    wg.Add(len(candidateModules));
    foreach (var (i, p) in candidateModules) {
        var ctx = trace.StartGoroutine(ctx);
        go_(() => (p, r) => {
            r.QueryResult, r.err = queryModule(ctx, p);
            wg.Done();
        }(p, _addr_results[i]));
    }    wg.Wait(); 

    // Classify the results. In case of failure, identify the error that the user
    // is most likely to find helpful: the most useful class of error at the
    // longest matching path.
    ptr<PackageNotInModuleError> noPackage;    ptr<NoMatchingVersionError> noVersion;    ptr<NoPatchBaseError> noPatchBase;    ptr<module.InvalidPathError> invalidPath;    error notExistErr = default!;
    foreach (var (_, r) in results) {
        switch (r.err.type()) {
            case 
                found = append(found, r.QueryResult);
                break;
            case ptr<PackageNotInModuleError> rErr:
                if (noPackage == null || noPackage.Mod == Target) {
                    noPackage = rErr;
                }
                break;
            case ptr<NoMatchingVersionError> rErr:
                if (noVersion == null) {
                    noVersion = rErr;
                }
                break;
            case ptr<NoPatchBaseError> rErr:
                if (noPatchBase == null) {
                    noPatchBase = rErr;
                }
                break;
            case ptr<module.InvalidPathError> rErr:
                if (invalidPath == null) {
                    invalidPath = rErr;
                }
                break;
            default:
            {
                var rErr = r.err.type();
                if (errors.Is(rErr, fs.ErrNotExist)) {
                    if (notExistErr == null) {
                        notExistErr = error.As(rErr)!;
                    }
                }
                else if (err == null) {
                    if (len(found) > 0 || noPackage != null) { 
                        // golang.org/issue/34094: If we have already found a module that
                        // could potentially contain the target package, ignore unclassified
                        // errors for modules with shorter paths.

                        // golang.org/issue/34383 is a special case of this: if we have
                        // already found example.com/foo/v2@v2.0.0 with a matching go.mod
                        // file, ignore the error from example.com/foo@v2.0.0.
                    }
                    else
 {
                        err = r.err;
                    }
                }
                break;
            }
        }
    }    if (len(found) == 0 && err == null) {

        if (noPackage != null) 
            err = noPackage;
        else if (noVersion != null) 
            err = noVersion;
        else if (noPatchBase != null) 
            err = noPatchBase;
        else if (invalidPath != null) 
            err = invalidPath;
        else if (notExistErr != null) 
            err = notExistErr;
        else 
            panic("queryPrefixModules: no modules found, but no error detected");
            }
    return (found, error.As(err)!);
});

// A NoMatchingVersionError indicates that Query found a module at the requested
// path, but not at any versions satisfying the query string and allow-function.
//
// NOTE: NoMatchingVersionError MUST NOT implement Is(fs.ErrNotExist).
//
// If the module came from a proxy, that proxy had to return a successful status
// code for the versions it knows about, and thus did not have the opportunity
// to return a non-400 status code to suppress fallback.
public partial struct NoMatchingVersionError {
    public @string query;
    public @string current;
}

private static @string Error(this ptr<NoMatchingVersionError> _addr_e) {
    ref NoMatchingVersionError e = ref _addr_e.val;

    @string currentSuffix = "";
    if ((e.query == "upgrade" || e.query == "patch") && e.current != "" && e.current != "none") {
        currentSuffix = fmt.Sprintf(" (current version is %s)", e.current);
    }
    return fmt.Sprintf("no matching versions for query %q", e.query) + currentSuffix;
}

// A NoPatchBaseError indicates that Query was called with the query "patch"
// but with a current version of "" or "none".
public partial struct NoPatchBaseError {
    public @string path;
}

private static @string Error(this ptr<NoPatchBaseError> _addr_e) {
    ref NoPatchBaseError e = ref _addr_e.val;

    return fmt.Sprintf("can\'t query version \"patch\" of module %s: no existing version is required", e.path);
}

// A WildcardInFirstElementError indicates that a pattern passed to QueryPattern
// had a wildcard in its first path element, and therefore had no pattern-prefix
// modules to search in.
public partial struct WildcardInFirstElementError {
    public @string Pattern;
    public @string Query;
}

private static @string Error(this ptr<WildcardInFirstElementError> _addr_e) {
    ref WildcardInFirstElementError e = ref _addr_e.val;

    return fmt.Sprintf("no modules to query for %s@%s because first path element contains a wildcard", e.Pattern, e.Query);
}

// A PackageNotInModuleError indicates that QueryPattern found a candidate
// module at the requested version, but that module did not contain any packages
// matching the requested pattern.
//
// NOTE: PackageNotInModuleError MUST NOT implement Is(fs.ErrNotExist).
//
// If the module came from a proxy, that proxy had to return a successful status
// code for the versions it knows about, and thus did not have the opportunity
// to return a non-400 status code to suppress fallback.
public partial struct PackageNotInModuleError {
    public module.Version Mod;
    public module.Version Replacement;
    public @string Query;
    public @string Pattern;
}

private static @string Error(this ptr<PackageNotInModuleError> _addr_e) {
    ref PackageNotInModuleError e = ref _addr_e.val;

    if (e.Mod == Target) {
        if (strings.Contains(e.Pattern, "...")) {
            return fmt.Sprintf("main module (%s) does not contain packages matching %s", Target.Path, e.Pattern);
        }
        return fmt.Sprintf("main module (%s) does not contain package %s", Target.Path, e.Pattern);
    }
    @string found = "";
    {
        var r = e.Replacement;

        if (r.Path != "") {
            var replacement = r.Path;
            if (r.Version != "") {
                replacement = fmt.Sprintf("%s@%s", r.Path, r.Version);
            }
            if (e.Query == e.Mod.Version) {
                found = fmt.Sprintf(" (replaced by %s)", replacement);
            }
            else
 {
                found = fmt.Sprintf(" (%s, replaced by %s)", e.Mod.Version, replacement);
            }
        }
        else if (e.Query != e.Mod.Version) {
            found = fmt.Sprintf(" (%s)", e.Mod.Version);
        }

    }

    if (strings.Contains(e.Pattern, "...")) {
        return fmt.Sprintf("module %s@%s found%s, but does not contain packages matching %s", e.Mod.Path, e.Query, found, e.Pattern);
    }
    return fmt.Sprintf("module %s@%s found%s, but does not contain package %s", e.Mod.Path, e.Query, found, e.Pattern);
}

private static @string ImportPath(this ptr<PackageNotInModuleError> _addr_e) {
    ref PackageNotInModuleError e = ref _addr_e.val;

    if (!strings.Contains(e.Pattern, "...")) {
        return e.Pattern;
    }
    return "";
}

// moduleHasRootPackage returns whether module m contains a package m.Path.
private static (bool, error) moduleHasRootPackage(context.Context ctx, module.Version m) {
    bool _p0 = default;
    error _p0 = default!;

    var needSum = false;
    var (root, isLocal, err) = fetch(ctx, m, needSum);
    if (err != null) {
        return (false, error.As(err)!);
    }
    var (_, ok, err) = dirInModule(m.Path, m.Path, root, isLocal);
    return (ok, error.As(err)!);
}

// versionHasGoMod returns whether a version has a go.mod file.
//
// versionHasGoMod fetches the go.mod file (possibly a fake) and true if it
// contains anything other than a module directive with the same path. When a
// module does not have a real go.mod file, the go command acts as if it had one
// that only contained a module directive. Normal go.mod files created after
// 1.12 at least have a go directive.
//
// This function is a heuristic, since it's possible to commit a file that would
// pass this test. However, we only need a heurstic for determining whether
// +incompatible versions may be "latest", which is what this function is used
// for.
//
// This heuristic is useful for two reasons: first, when using a proxy,
// this lets us fetch from the .mod endpoint which is much faster than the .zip
// endpoint. The .mod file is used anyway, even if the .zip file contains a
// go.mod with different content. Second, if we don't fetch the .zip, then
// we don't need to verify it in go.sum. This makes 'go list -m -u' faster
// and simpler.
private static (bool, error) versionHasGoMod(context.Context _, module.Version m) {
    bool _p0 = default;
    error _p0 = default!;

    var (_, data, err) = rawGoModData(m);
    if (err != null) {
        return (false, error.As(err)!);
    }
    var isFake = bytes.Equal(data, modfetch.LegacyGoMod(m.Path));
    return (!isFake, error.As(null!)!);
}

// A versionRepo is a subset of modfetch.Repo that can report information about
// available versions, but cannot fetch specific source files.
private partial interface versionRepo {
    (ptr<modfetch.RevInfo>, error) ModulePath();
    (ptr<modfetch.RevInfo>, error) Versions(@string prefix);
    (ptr<modfetch.RevInfo>, error) Stat(@string rev);
    (ptr<modfetch.RevInfo>, error) Latest();
}

private static versionRepo _ = versionRepo.As(modfetch.Repo(null))!;

private static (versionRepo, error) lookupRepo(@string proxy, @string path) {
    versionRepo repo = default;
    error err = default!;

    err = module.CheckPath(path);
    if (err == null) {
        repo = modfetch.Lookup(proxy, path);
    }
    else
 {
        repo = new emptyRepo(path:path,err:err);
    }
    if (index == null) {
        return (repo, error.As(err)!);
    }
    {
        var (_, ok) = index.highestReplaced[path];

        if (!ok) {
            return (repo, error.As(err)!);
        }
    }

    return (addr(new replacementRepo(repo:repo)), error.As(null!)!);
}

// An emptyRepo is a versionRepo that contains no versions.
private partial struct emptyRepo {
    public @string path;
    public error err;
}

private static versionRepo _ = versionRepo.As(new emptyRepo())!;

private static @string ModulePath(this emptyRepo er) {
    return er.path;
}
private static (slice<@string>, error) Versions(this emptyRepo er, @string prefix) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    return (null, error.As(null!)!);
}
private static (ptr<modfetch.RevInfo>, error) Stat(this emptyRepo er, @string rev) {
    ptr<modfetch.RevInfo> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(er.err)!);
}
private static (ptr<modfetch.RevInfo>, error) Latest(this emptyRepo er) {
    ptr<modfetch.RevInfo> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(er.err)!);
}

// A replacementRepo augments a versionRepo to include the replacement versions
// (if any) found in the main module's go.mod file.
//
// A replacementRepo suppresses "not found" errors for otherwise-nonexistent
// modules, so a replacementRepo should only be constructed for a module that
// actually has one or more valid replacements.
private partial struct replacementRepo {
    public versionRepo repo;
}

private static versionRepo _ = versionRepo.As((replacementRepo.val)(null))!;

private static @string ModulePath(this ptr<replacementRepo> _addr_rr) {
    ref replacementRepo rr = ref _addr_rr.val;

    return rr.repo.ModulePath();
}

// Versions returns the versions from rr.repo augmented with any matching
// replacement versions.
private static (slice<@string>, error) Versions(this ptr<replacementRepo> _addr_rr, @string prefix) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref replacementRepo rr = ref _addr_rr.val;

    var (repoVersions, err) = rr.repo.Versions(prefix);
    if (err != null && !errors.Is(err, os.ErrNotExist)) {
        return (null, error.As(err)!);
    }
    ref var versions = ref heap(repoVersions, out ptr<var> _addr_versions);
    if (index != null && len(index.replace) > 0) {
        var path = rr.ModulePath();
        foreach (var (m, _) in index.replace) {
            if (m.Path == path && strings.HasPrefix(m.Version, prefix) && m.Version != "" && !module.IsPseudoVersion(m.Version)) {
                versions = append(versions, m.Version);
            }
        }
    }
    if (len(versions) == len(repoVersions)) { // No replacement versions added.
        return (versions, error.As(null!)!);
    }
    sort.Slice(versions, (i, j) => semver.Compare(versions[i], versions[j]) < 0);
    str.Uniq(_addr_versions);
    return (versions, error.As(null!)!);
}

private static (ptr<modfetch.RevInfo>, error) Stat(this ptr<replacementRepo> _addr_rr, @string rev) {
    ptr<modfetch.RevInfo> _p0 = default!;
    error _p0 = default!;
    ref replacementRepo rr = ref _addr_rr.val;

    var (info, err) = rr.repo.Stat(rev);
    if (err == null || index == null || len(index.replace) == 0) {
        return (_addr_info!, error.As(err)!);
    }
    var v = module.CanonicalVersion(rev);
    if (v != rev) { 
        // The replacements in the go.mod file list only canonical semantic versions,
        // so a non-canonical version can't possibly have a replacement.
        return (_addr_info!, error.As(err)!);
    }
    var path = rr.ModulePath();
    var (_, pathMajor, ok) = module.SplitPathVersion(path);
    if (ok && pathMajor == "") {
        {
            var err = module.CheckPathMajor(v, pathMajor);

            if (err != null && semver.Build(v) == "") {
                v += "+incompatible";
            }

        }
    }
    {
        var r = Replacement(new module.Version(Path:path,Version:v));

        if (r.Path == "") {
            return (_addr_info!, error.As(err)!);
        }
    }
    return _addr_rr.replacementStat(v)!;
}

private static (ptr<modfetch.RevInfo>, error) Latest(this ptr<replacementRepo> _addr_rr) {
    ptr<modfetch.RevInfo> _p0 = default!;
    error _p0 = default!;
    ref replacementRepo rr = ref _addr_rr.val;

    var (info, err) = rr.repo.Latest();

    if (index != null) {
        var path = rr.ModulePath();
        {
            var (v, ok) = index.highestReplaced[path];

            if (ok) {
                if (v == "") { 
                    // The only replacement is a wildcard that doesn't specify a version, so
                    // synthesize a pseudo-version with an appropriate major version and a
                    // timestamp below any real timestamp. That way, if the main module is
                    // used from within some other module, the user will be able to upgrade
                    // the requirement to any real version they choose.
                    {
                        var (_, pathMajor, ok) = module.SplitPathVersion(path);

                        if (ok && len(pathMajor) > 0) {
                            v = module.PseudoVersion(pathMajor[(int)1..], "", new time.Time(), "000000000000");
                        }
                        else
 {
                            v = module.PseudoVersion("v0", "", new time.Time(), "000000000000");
                        }

                    }
                }
                if (err != null || semver.Compare(v, info.Version) > 0) {
                    return _addr_rr.replacementStat(v)!;
                }
            }

        }
    }
    return (_addr_info!, error.As(err)!);
}

private static (ptr<modfetch.RevInfo>, error) replacementStat(this ptr<replacementRepo> _addr_rr, @string v) {
    ptr<modfetch.RevInfo> _p0 = default!;
    error _p0 = default!;
    ref replacementRepo rr = ref _addr_rr.val;

    ptr<modfetch.RevInfo> rev = addr(new modfetch.RevInfo(Version:v));
    if (module.IsPseudoVersion(v)) {
        rev.Time, _ = module.PseudoVersionTime(v);
        rev.Short, _ = module.PseudoVersionRev(v);
    }
    return (_addr_rev!, error.As(null!)!);
}

// A QueryMatchesMainModuleError indicates that a query requests
// a version of the main module that cannot be satisfied.
// (The main module's version cannot be changed.)
public partial struct QueryMatchesMainModuleError {
    public @string Pattern;
    public @string Query;
}

private static @string Error(this ptr<QueryMatchesMainModuleError> _addr_e) {
    ref QueryMatchesMainModuleError e = ref _addr_e.val;

    if (e.Pattern == Target.Path) {
        return fmt.Sprintf("can't request version %q of the main module (%s)", e.Query, e.Pattern);
    }
    return fmt.Sprintf("can't request version %q of pattern %q that includes the main module (%s)", e.Query, e.Pattern, Target.Path);
}

// A QueryMatchesPackagesInMainModuleError indicates that a query cannot be
// satisfied because it matches one or more packages found in the main module.
public partial struct QueryMatchesPackagesInMainModuleError {
    public @string Pattern;
    public @string Query;
    public slice<@string> Packages;
}

private static @string Error(this ptr<QueryMatchesPackagesInMainModuleError> _addr_e) {
    ref QueryMatchesPackagesInMainModuleError e = ref _addr_e.val;

    if (len(e.Packages) > 1) {
        return fmt.Sprintf("pattern %s matches %d packages in the main module, so can't request version %s", e.Pattern, len(e.Packages), e.Query);
    }
    if (search.IsMetaPackage(e.Pattern) || strings.Contains(e.Pattern, "...")) {
        return fmt.Sprintf("pattern %s matches package %s in the main module, so can't request version %s", e.Pattern, e.Packages[0], e.Query);
    }
    return fmt.Sprintf("package %s is in the main module, so can't request version %s", e.Packages[0], e.Query);
}

} // end modload_package
