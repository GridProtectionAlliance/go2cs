// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modget -- go2cs converted at 2022 March 13 06:31:56 UTC
// import "cmd/go/internal/modget" ==> using modget = go.cmd.go.@internal.modget_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modget\query.go
namespace go.cmd.go.@internal;

using fmt = fmt_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using strings = strings_package;
using sync = sync_package;

using @base = cmd.go.@internal.@base_package;
using modload = cmd.go.@internal.modload_package;
using search = cmd.go.@internal.search_package;
using str = cmd.go.@internal.str_package;

using module = golang.org.x.mod.module_package;


// A query describes a command-line argument and the modules and/or packages
// to which that argument may resolve..

using System;
public static partial class modget_package {

private partial struct query {
    public @string raw; // rawVersion is the portion of raw corresponding to version, if any
    public @string rawVersion; // pattern is the part of the argument before "@" (or the whole argument
// if there is no "@"), which may match either packages (preferred) or
// modules (if no matching packages).
//
// The pattern may also be "-u", for the synthetic query representing the -u
// (“upgrade”)flag.
    public @string pattern; // patternIsLocal indicates whether pattern is restricted to match only paths
// local to the main module, such as absolute filesystem paths or paths
// beginning with './'.
//
// A local pattern must resolve to one or more packages in the main module.
    public bool patternIsLocal; // version is the part of the argument after "@", or an implied
// "upgrade" or "patch" if there is no "@". version specifies the
// module version to get.
    public @string version; // matchWildcard, if non-nil, reports whether pattern, which must be a
// wildcard (with the substring "..."), matches the given package or module
// path.
    public Func<@string, bool> matchWildcard; // canMatchWildcard, if non-nil, reports whether the module with the given
// path could lexically contain a package matching pattern, which must be a
// wildcard.
    public Func<@string, bool> canMatchWildcardInModule; // conflict is the first query identified as incompatible with this one.
// conflict forces one or more of the modules matching this query to a
// version that does not match version.
    public ptr<query> conflict; // candidates is a list of sets of alternatives for a path that matches (or
// contains packages that match) the pattern. The query can be resolved by
// choosing exactly one alternative from each set in the list.
//
// A path-literal query results in only one set: the path itself, which
// may resolve to either a package path or a module path.
//
// A wildcard query results in one set for each matching module path, each
// module for which the matching version contains at least one matching
// package, and (if no other modules match) one candidate set for the pattern
// overall if no existing match is identified in the build list.
//
// A query for pattern "all" results in one set for each package transitively
// imported by the main module.
//
// The special query for the "-u" flag results in one set for each
// otherwise-unconstrained package that has available upgrades.
    public slice<pathSet> candidates;
    public sync.Mutex candidatesMu; // pathSeen ensures that only one pathSet is added to the query per
// unique path.
    public sync.Map pathSeen; // resolved contains the set of modules whose versions have been determined by
// this query, in the order in which they were determined.
//
// The resolver examines the candidate sets for each query, resolving one
// module per candidate set in a way that attempts to avoid obvious conflicts
// between the versions resolved by different queries.
    public slice<module.Version> resolved; // matchesPackages is true if the resolved modules provide at least one
// package mathcing q.pattern.
    public bool matchesPackages;
}

// A pathSet describes the possible options for resolving a specific path
// to a package and/or module.
private partial struct pathSet {
    public @string path; // pkgMods is a set of zero or more modules, each of which contains the
// package with the indicated path. Due to the requirement that imports be
// unambiguous, only one such module can be in the build list, and all others
// must be excluded.
    public slice<module.Version> pkgMods; // mod is either the zero Version, or a module that does not contain any
// packages matching the query but for which the module path itself
// matches the query pattern.
//
// We track this module separately from pkgMods because, all else equal, we
// prefer to match a query to a package rather than just a module. Also,
// unlike the modules in pkgMods, this module does not inherently exclude
// any other module in pkgMods.
    public module.Version mod;
    public error err;
}

// errSet returns a pathSet containing the given error.
private static pathSet errSet(error err) {
    return new pathSet(err:err);
}

// newQuery returns a new query parsed from the raw argument,
// which must be either path or path@version.
private static (ptr<query>, error) newQuery(@string raw) {
    ptr<query> _p0 = default!;
    error _p0 = default!;

    var pattern = raw;
    @string rawVers = "";
    {
        var i = strings.Index(raw, "@");

        if (i >= 0) {
            (pattern, rawVers) = (raw[..(int)i], raw[(int)i + 1..]);            if (strings.Contains(rawVers, "@") || rawVers == "") {
                return (_addr_null!, error.As(fmt.Errorf("invalid module version syntax %q", raw))!);
            }
        }
    } 

    // If no version suffix is specified, assume @upgrade.
    // If -u=patch was specified, assume @patch instead.
    var version = rawVers;
    if (version == "") {
        if (getU.version == "") {
            version = "upgrade";
        }
        else
 {
            version = getU.version;
        }
    }
    ptr<query> q = addr(new query(raw:raw,rawVersion:rawVers,pattern:pattern,patternIsLocal:filepath.IsAbs(pattern)||search.IsRelativePath(pattern),version:version,));
    if (strings.Contains(q.pattern, "...")) {
        q.matchWildcard = search.MatchPattern(q.pattern);
        q.canMatchWildcardInModule = search.TreeCanMatchPattern(q.pattern);
    }
    {
        var err = q.validate();

        if (err != null) {
            return (_addr_q!, error.As(err)!);
        }
    }
    return (_addr_q!, error.As(null!)!);
}

// validate reports a non-nil error if q is not sensible and well-formed.
private static error validate(this ptr<query> _addr_q) {
    ref query q = ref _addr_q.val;

    if (q.patternIsLocal) {
        if (q.rawVersion != "") {
            return error.As(fmt.Errorf("can't request explicit version %q of path %q in main module", q.rawVersion, q.pattern))!;
        }
        return error.As(null!)!;
    }
    if (q.pattern == "all") { 
        // If there is no main module, "all" is not meaningful.
        if (!modload.HasModRoot()) {
            return error.As(fmt.Errorf("cannot match \"all\": %v", modload.ErrNoModRoot))!;
        }
        if (!versionOkForMainModule(q.version)) { 
            // TODO(bcmills): "all@none" seems like a totally reasonable way to
            // request that we remove all module requirements, leaving only the main
            // module and standard library. Perhaps we should implement that someday.
            return error.As(addr(new modload.QueryMatchesMainModuleError(Pattern:q.pattern,Query:q.version,))!)!;
        }
    }
    if (search.IsMetaPackage(q.pattern) && q.pattern != "all") {
        if (q.pattern != q.raw) {
            return error.As(fmt.Errorf("can't request explicit version of standard-library pattern %q", q.pattern))!;
        }
    }
    return error.As(null!)!;
}

// String returns the original argument from which q was parsed.
private static @string String(this ptr<query> _addr_q) {
    ref query q = ref _addr_q.val;

    return q.raw;
}

// ResolvedString returns a string describing m as a resolved match for q.
private static @string ResolvedString(this ptr<query> _addr_q, module.Version m) {
    ref query q = ref _addr_q.val;

    if (m.Path != q.pattern) {
        if (m.Version != q.version) {
            return fmt.Sprintf("%v (matching %s@%s)", m, q.pattern, q.version);
        }
        return fmt.Sprintf("%v (matching %v)", m, q);
    }
    if (m.Version != q.version) {
        return fmt.Sprintf("%s@%s (%s)", q.pattern, q.version, m.Version);
    }
    return q.String();
}

// isWildcard reports whether q is a pattern that can match multiple paths.
private static bool isWildcard(this ptr<query> _addr_q) {
    ref query q = ref _addr_q.val;

    return q.matchWildcard != null || (q.patternIsLocal && strings.Contains(q.pattern, "..."));
}

// matchesPath reports whether the given path matches q.pattern.
private static bool matchesPath(this ptr<query> _addr_q, @string path) {
    ref query q = ref _addr_q.val;

    if (q.matchWildcard != null) {
        return q.matchWildcard(path);
    }
    return path == q.pattern;
}

// canMatchInModule reports whether the given module path can potentially
// contain q.pattern.
private static bool canMatchInModule(this ptr<query> _addr_q, @string mPath) {
    ref query q = ref _addr_q.val;

    if (q.canMatchWildcardInModule != null) {
        return q.canMatchWildcardInModule(mPath);
    }
    return str.HasPathPrefix(q.pattern, mPath);
}

// pathOnce invokes f to generate the pathSet for the given path,
// if one is still needed.
//
// Note that, unlike sync.Once, pathOnce does not guarantee that a concurrent
// call to f for the given path has completed on return.
//
// pathOnce is safe for concurrent use by multiple goroutines, but note that
// multiple concurrent calls will result in the sets being added in
// nondeterministic order.
private static pathSet pathOnce(this ptr<query> _addr_q, @string path, Func<pathSet> f) {
    ref query q = ref _addr_q.val;

    {
        var (_, dup) = q.pathSeen.LoadOrStore(path, null);

        if (dup) {
            return ;
        }
    }

    var cs = f();

    if (len(cs.pkgMods) > 0 || cs.mod != (new module.Version()) || cs.err != null) {
        cs.path = path;
        q.candidatesMu.Lock();
        q.candidates = append(q.candidates, cs);
        q.candidatesMu.Unlock();
    }
}

// reportError logs err concisely using base.Errorf.
private static void reportError(ptr<query> _addr_q, error err) {
    ref query q = ref _addr_q.val;

    var errStr = err.Error(); 

    // If err already mentions all of the relevant parts of q, just log err to
    // reduce stutter. Otherwise, log both q and err.
    //
    // TODO(bcmills): Use errors.As to unpack these errors instead of parsing
    // strings with regular expressions.

    var patternRE = regexp.MustCompile("(?m)(?:[ \t(\"`]|^)" + regexp.QuoteMeta(q.pattern) + "(?:[ @:;)\"`]|$)");
    if (patternRE.MatchString(errStr)) {
        if (q.rawVersion == "") {
            @base.Errorf("go get: %s", errStr);
            return ;
        }
        var versionRE = regexp.MustCompile("(?m)(?:[ @(\"`]|^)" + regexp.QuoteMeta(q.version) + "(?:[ :;)\"`]|$)");
        if (versionRE.MatchString(errStr)) {
            @base.Errorf("go get: %s", errStr);
            return ;
        }
    }
    {
        var qs = q.String();

        if (qs != "") {
            @base.Errorf("go get %s: %s", qs, errStr);
        }
        else
 {
            @base.Errorf("go get: %s", errStr);
        }
    }
}

private static void reportConflict(ptr<query> _addr_pq, module.Version m, versionReason conflict) {
    ref query pq = ref _addr_pq.val;

    if (pq.conflict != null) { 
        // We've already reported a conflict for the proposed query.
        // Don't report it again, even if it has other conflicts.
        return ;
    }
    pq.conflict = conflict.reason;

    versionReason proposed = new versionReason(version:m.Version,reason:pq,);
    if (pq.isWildcard() && !conflict.reason.isWildcard()) { 
        // Prefer to report the specific path first and the wildcard second.
        (proposed, conflict) = (conflict, proposed);
    }
    reportError(_addr_pq, addr(new conflictError(mPath:m.Path,proposed:proposed,conflict:conflict,)));
}

private partial struct conflictError {
    public @string mPath;
    public versionReason proposed;
    public versionReason conflict;
}

private static @string Error(this ptr<conflictError> _addr_e) {
    ref conflictError e = ref _addr_e.val;

    Func<ptr<query>, @string, @string> argStr = (q, v) => {
        if (v != q.version) {
            return fmt.Sprintf("%s@%s (%s)", q.pattern, q.version, v);
        }
        return q.String();
    };

    var pq = e.proposed.reason;
    var rq = e.conflict.reason;
    @string modDetail = "";
    if (e.mPath != pq.pattern) {
        modDetail = fmt.Sprintf("for module %s, ", e.mPath);
    }
    return fmt.Sprintf("%s%s conflicts with %s", modDetail, argStr(pq, e.proposed.version), argStr(rq, e.conflict.version));
}

private static bool versionOkForMainModule(@string version) {
    return version == "upgrade" || version == "patch";
}

} // end modget_package
