// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2020 October 09 05:46:56 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Go\src\cmd\go\internal\modload\query.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using pathpkg = go.path_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using sync = go.sync_package;

using cfg = go.cmd.go.@internal.cfg_package;
using imports = go.cmd.go.@internal.imports_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using search = go.cmd.go.@internal.search_package;
using str = go.cmd.go.@internal.str_package;

using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modload_package
    {
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
        // current denotes the current version of the module; it may be "" if the
        // current version is unknown or should not be considered. If query is
        // "upgrade" or "patch", current will be returned if it is a newer
        // semantic version or a chronologically later pseudo-version than the
        // version that would otherwise be chosen. This prevents accidental downgrades
        // from newer pre-release or development versions.
        //
        // If the allowed function is non-nil, Query excludes any versions for which
        // allowed returns false.
        //
        // If path is the path of the main module and the query is "latest",
        // Query returns Target.Version as the version.
        public static (ptr<modfetch.RevInfo>, error) Query(@string path, @string query, @string current, Func<module.Version, bool> allowed)
        {
            ptr<modfetch.RevInfo> _p0 = default!;
            error _p0 = default!;

            ptr<modfetch.RevInfo> info;
            var err = modfetch.TryProxies(proxy =>
            {
                info, err = queryProxy(proxy, path, query, current, allowed);
                return _addr_err!;
            });
            return (_addr_info!, error.As(err)!);

        }

        private static error errQueryDisabled = error.As(new queryDisabledError())!;

        private partial struct queryDisabledError
        {
        }

        private static @string Error(this queryDisabledError _p0)
        {
            if (cfg.BuildModReason == "")
            {
                return fmt.Sprintf("cannot query module due to -mod=%s", cfg.BuildMod);
            }

            return fmt.Sprintf("cannot query module due to -mod=%s\n\t(%s)", cfg.BuildMod, cfg.BuildModReason);

        }

        private static (ptr<modfetch.RevInfo>, error) queryProxy(@string proxy, @string path, @string query, @string current, Func<module.Version, bool> allowed)
        {
            ptr<modfetch.RevInfo> _p0 = default!;
            error _p0 = default!;

            if (current != "" && !semver.IsValid(current))
            {
                return (_addr_null!, error.As(fmt.Errorf("invalid previous version %q", current))!);
            }

            if (cfg.BuildMod == "vendor")
            {
                return (_addr_null!, error.As(errQueryDisabled)!);
            }

            if (allowed == null)
            {
                allowed = _p0 => _addr_true!;
            } 

            // Parse query to detect parse errors (and possibly handle query)
            // before any network I/O.
            Func<@string, (ptr<modfetch.RevInfo>, error)> badVersion = v =>
            {
                return (_addr_null!, error.As(fmt.Errorf("invalid semantic version %q in range %q", v, query))!);
            }
;
            Func<@string, bool> matchesMajor = v =>
            {
                var (_, pathMajor, ok) = module.SplitPathVersion(path);
                if (!ok)
                {
                    return _addr_false!;
                }

                return _addr_module.CheckPathMajor(v, pathMajor) == null!;

            }
;
            Func<module.Version, bool> ok = default;            @string prefix = default;            bool preferOlder = default;            bool mayUseLatest = default;            bool preferIncompatible = strings.HasSuffix(current, "+incompatible");

            if (query == "latest") 
                ok = allowed;
                mayUseLatest = true;
            else if (query == "upgrade") 
                ok = allowed;
                mayUseLatest = true;
            else if (query == "patch") 
                if (current == "")
                {
                    ok = allowed;
                    mayUseLatest = true;
                }
                else
                {
                    prefix = semver.MajorMinor(current);
                    ok = m =>
                    {
                        return _addr_matchSemverPrefix(prefix, m.Version) && allowed(m)!;
                    }
;

                }

            else if (strings.HasPrefix(query, "<=")) 
                var v = query[len("<=")..];
                if (!semver.IsValid(v))
                {
                    return _addr_badVersion(v)!;
                }

                if (isSemverPrefix(v))
                { 
                    // Refuse to say whether <=v1.2 allows v1.2.3 (remember, @v1.2 might mean v1.2.3).
                    return (_addr_null!, error.As(fmt.Errorf("ambiguous semantic version %q in range %q", v, query))!);

                }

                ok = m =>
                {
                    return _addr_semver.Compare(m.Version, v) <= 0L && allowed(m)!;
                }
;
                if (!matchesMajor(v))
                {
                    preferIncompatible = true;
                }

            else if (strings.HasPrefix(query, "<")) 
                v = query[len("<")..];
                if (!semver.IsValid(v))
                {
                    return _addr_badVersion(v)!;
                }

                ok = m =>
                {
                    return _addr_semver.Compare(m.Version, v) < 0L && allowed(m)!;
                }
;
                if (!matchesMajor(v))
                {
                    preferIncompatible = true;
                }

            else if (strings.HasPrefix(query, ">=")) 
                v = query[len(">=")..];
                if (!semver.IsValid(v))
                {
                    return _addr_badVersion(v)!;
                }

                ok = m =>
                {
                    return _addr_semver.Compare(m.Version, v) >= 0L && allowed(m)!;
                }
;
                preferOlder = true;
                if (!matchesMajor(v))
                {
                    preferIncompatible = true;
                }

            else if (strings.HasPrefix(query, ">")) 
                v = query[len(">")..];
                if (!semver.IsValid(v))
                {
                    return _addr_badVersion(v)!;
                }

                if (isSemverPrefix(v))
                { 
                    // Refuse to say whether >v1.2 allows v1.2.3 (remember, @v1.2 might mean v1.2.3).
                    return (_addr_null!, error.As(fmt.Errorf("ambiguous semantic version %q in range %q", v, query))!);

                }

                ok = m =>
                {
                    return _addr_semver.Compare(m.Version, v) > 0L && allowed(m)!;
                }
;
                preferOlder = true;
                if (!matchesMajor(v))
                {
                    preferIncompatible = true;
                }

            else if (semver.IsValid(query) && isSemverPrefix(query)) 
                ok = m =>
                {
                    return _addr_matchSemverPrefix(query, m.Version) && allowed(m)!;
                }
;
                prefix = query + ".";
                if (!matchesMajor(query))
                {
                    preferIncompatible = true;
                }

            else 
                // Direct lookup of semantic version or commit identifier.
                //
                // If the identifier is not a canonical semver tag — including if it's a
                // semver tag with a +metadata suffix — then modfetch.Stat will populate
                // info.Version with a suitable pseudo-version.
                var (info, err) = modfetch.Stat(proxy, path, query);
                if (err != null)
                {
                    var queryErr = err; 
                    // The full query doesn't correspond to a tag. If it is a semantic version
                    // with a +metadata suffix, see if there is a tag without that suffix:
                    // semantic versioning defines them to be equivalent.
                    {
                        var vers = module.CanonicalVersion(query);

                        if (vers != "" && vers != query)
                        {
                            info, err = modfetch.Stat(proxy, path, vers);
                            if (!errors.Is(err, os.ErrNotExist))
                            {
                                return (_addr_info!, error.As(err)!);
                            }

                        }

                    }

                    if (err != null)
                    {
                        return (_addr_null!, error.As(queryErr)!);
                    }

                }

                if (!allowed(new module.Version(Path:path,Version:info.Version)))
                {
                    return (_addr_null!, error.As(fmt.Errorf("%s@%s excluded", path, info.Version))!);
                }

                return (_addr_info!, error.As(null!)!);
                        if (path == Target.Path)
            {
                if (query != "latest")
                {
                    return (_addr_null!, error.As(fmt.Errorf("can't query specific version (%q) for the main module (%s)", query, path))!);
                }

                if (!allowed(Target))
                {
                    return (_addr_null!, error.As(fmt.Errorf("internal error: main module version is not allowed"))!);
                }

                return (addr(new modfetch.RevInfo(Version:Target.Version)), error.As(null!)!);

            }

            if (str.HasPathPrefix(path, "std") || str.HasPathPrefix(path, "cmd"))
            {
                return (_addr_null!, error.As(fmt.Errorf("explicit requirement on standard-library module %s not allowed", path))!);
            } 

            // Load versions and execute query.
            var (repo, err) = modfetch.Lookup(proxy, path);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var (versions, err) = repo.Versions(prefix);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var (releases, prereleases, err) = filterVersions(path, versions, ok, preferIncompatible);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            Func<@string, (ptr<modfetch.RevInfo>, error)> lookup = v =>
            {
                var (rev, err) = repo.Stat(v);
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                } 

                // For "upgrade" and "patch", make sure we don't accidentally downgrade
                // from a newer prerelease or from a chronologically newer pseudoversion.
                if (current != "" && (query == "upgrade" || query == "patch"))
                {
                    var (currentTime, err) = modfetch.PseudoVersionTime(current);
                    if (semver.Compare(rev.Version, current) < 0L || (err == null && rev.Time.Before(currentTime)))
                    {
                        return _addr_repo.Stat(current)!;
                    }

                }

                return (_addr_rev!, error.As(null!)!);

            }
;

            if (preferOlder)
            {
                if (len(releases) > 0L)
                {
                    return _addr_lookup(releases[0L])!;
                }

                if (len(prereleases) > 0L)
                {
                    return _addr_lookup(prereleases[0L])!;
                }

            }
            else
            {
                if (len(releases) > 0L)
                {
                    return _addr_lookup(releases[len(releases) - 1L])!;
                }

                if (len(prereleases) > 0L)
                {
                    return _addr_lookup(prereleases[len(prereleases) - 1L])!;
                }

            }

            if (mayUseLatest)
            { 
                // Special case for "latest": if no tags match, use latest commit in repo,
                // provided it is not excluded.
                var (latest, err) = repo.Latest();
                if (err == null)
                {
                    if (allowed(new module.Version(Path:path,Version:latest.Version)))
                    {
                        return _addr_lookup(latest.Version)!;
                    }

                }
                else if (!errors.Is(err, os.ErrNotExist))
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            return (_addr_null!, error.As(addr(new NoMatchingVersionError(query:query,current:current))!)!);

        }

        // isSemverPrefix reports whether v is a semantic version prefix: v1 or v1.2 (not v1.2.3).
        // The caller is assumed to have checked that semver.IsValid(v) is true.
        private static bool isSemverPrefix(@string v)
        {
            long dots = 0L;
            for (long i = 0L; i < len(v); i++)
            {
                switch (v[i])
                {
                    case '-': 

                    case '+': 
                        return false;
                        break;
                    case '.': 
                        dots++;
                        if (dots >= 2L)
                        {
                            return false;
                        }

                        break;
                }

            }

            return true;

        }

        // matchSemverPrefix reports whether the shortened semantic version p
        // matches the full-width (non-shortened) semantic version v.
        private static bool matchSemverPrefix(@string p, @string v)
        {
            return len(v) > len(p) && v[len(p)] == '.' && v[..len(p)] == p && semver.Prerelease(v) == "";
        }

        // filterVersions classifies versions into releases and pre-releases, filtering
        // out:
        //     1. versions that do not satisfy the 'ok' predicate, and
        //     2. "+incompatible" versions, if a compatible one satisfies the predicate
        //        and the incompatible version is not preferred.
        private static (slice<@string>, slice<@string>, error) filterVersions(@string path, slice<@string> versions, Func<module.Version, bool> ok, bool preferIncompatible)
        {
            slice<@string> releases = default;
            slice<@string> prereleases = default;
            error err = default!;

            @string lastCompatible = default;
            foreach (var (_, v) in versions)
            {
                if (!ok(new module.Version(Path:path,Version:v)))
                {
                    continue;
                }

                if (!preferIncompatible)
                {
                    if (!strings.HasSuffix(v, "+incompatible"))
                    {
                        lastCompatible = v;
                    }
                    else if (lastCompatible != "")
                    { 
                        // If the latest compatible version is allowed and has a go.mod file,
                        // ignore any version with a higher (+incompatible) major version. (See
                        // https://golang.org/issue/34165.) Note that we even prefer a
                        // compatible pre-release over an incompatible release.

                        var (ok, err) = versionHasGoMod(new module.Version(Path:path,Version:lastCompatible));
                        if (err != null)
                        {
                            return (null, null, error.As(err)!);
                        }

                        if (ok)
                        {
                            break;
                        } 

                        // No acceptable compatible release has a go.mod file, so the versioning
                        // for the module might not be module-aware, and we should respect
                        // legacy major-version tags.
                        preferIncompatible = true;

                    }

                }

                if (semver.Prerelease(v) != "")
                {
                    prereleases = append(prereleases, v);
                }
                else
                {
                    releases = append(releases, v);
                }

            }
            return (releases, prereleases, error.As(null!)!);

        }

        public partial struct QueryResult
        {
            public module.Version Mod;
            public ptr<modfetch.RevInfo> Rev;
            public slice<@string> Packages;
        }

        // QueryPackage looks up the module(s) containing path at a revision matching
        // query. The results are sorted by module path length in descending order.
        //
        // If the package is in the main module, QueryPackage considers only the main
        // module and only the version "latest", without checking for other possible
        // modules.
        public static (slice<QueryResult>, error) QueryPackage(@string path, @string query, Func<module.Version, bool> allowed)
        {
            slice<QueryResult> _p0 = default;
            error _p0 = default!;

            var m = search.NewMatch(path);
            if (m.IsLocal() || !m.IsLiteral())
            {
                return (null, error.As(fmt.Errorf("pattern %s is not an importable package", path))!);
            }

            return QueryPattern(path, query, allowed);

        }

        // QueryPattern looks up the module(s) containing at least one package matching
        // the given pattern at the given version. The results are sorted by module path
        // length in descending order.
        //
        // QueryPattern queries modules with package paths up to the first "..."
        // in the pattern. For the pattern "example.com/a/b.../c", QueryPattern would
        // consider prefixes of "example.com/a". If multiple modules have versions
        // that match the query and packages that match the pattern, QueryPattern
        // picks the one with the longest module path.
        //
        // If any matching package is in the main module, QueryPattern considers only
        // the main module and only the version "latest", without checking for other
        // possible modules.
        public static (slice<QueryResult>, error) QueryPattern(@string pattern, @string query, Func<module.Version, bool> allowed)
        {
            slice<QueryResult> _p0 = default;
            error _p0 = default!;

            var @base = pattern;

            Func<ptr<search.Match>, error> firstError = m =>
            {
                if (len(m.Errs) == 0L)
                {
                    return null;
                }

                return m.Errs[0L];

            }
;

            Func<module.Version, @string, bool, ptr<search.Match>> match = default;

            {
                var i = strings.Index(pattern, "...");

                if (i >= 0L)
                {
                    base = pathpkg.Dir(pattern[..i + 3L]);
                    match = (mod, root, isLocal) =>
                    {
                        var m = search.NewMatch(pattern);
                        matchPackages(m, imports.AnyTags(), omitStd, new slice<module.Version>(new module.Version[] { mod }));
                        return m;
                    }
                else
;

                }                {
                    match = (mod, root, isLocal) =>
                    {
                        m = search.NewMatch(pattern);
                        var prefix = mod.Path;
                        if (mod == Target)
                        {
                            prefix = targetPrefix;
                        }

                        {
                            var err__prev2 = err;

                            var (_, ok, err) = dirInModule(pattern, prefix, root, isLocal);

                            if (err != null)
                            {
                                m.AddError(err);
                            }
                            else if (ok)
                            {
                                m.Pkgs = new slice<@string>(new @string[] { pattern });
                            }


                            err = err__prev2;

                        }

                        return m;

                    }
;

                }

            }


            if (HasModRoot())
            {
                m = match(Target, modRoot, true);
                if (len(m.Pkgs) > 0L)
                {
                    if (query != "latest")
                    {
                        return (null, error.As(fmt.Errorf("can't query specific version for package %s in the main module (%s)", pattern, Target.Path))!);
                    }

                    if (!allowed(Target))
                    {
                        return (null, error.As(fmt.Errorf("internal error: package %s is in the main module (%s), but version is not allowed", pattern, Target.Path))!);
                    }

                    return (new slice<QueryResult>(new QueryResult[] { {Mod:Target,Rev:&modfetch.RevInfo{Version:Target.Version},Packages:m.Pkgs,} }), error.As(null!)!);

                }

                {
                    var err__prev2 = err;

                    var err = firstError(m);

                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                    err = err__prev2;

                }

            }

            slice<QueryResult> results = default;            var candidateModules = modulePrefixesExcludingTarget(base);
            if (len(candidateModules) == 0L)
            {
                return (null, error.As(addr(new PackageNotInModuleError(Mod:Target,Query:query,Pattern:pattern,))!)!);
            }

            err = modfetch.TryProxies(proxy =>
            {
                Func<@string, (QueryResult, error)> queryModule = path =>
                {
                    var current = findCurrentVersion(path);
                    r.Mod.Path = path;
                    r.Rev, err = queryProxy(proxy, path, query, current, allowed);
                    if (err != null)
                    {
                        return (r, error.As(err)!);
                    }

                    r.Mod.Version = r.Rev.Version;
                    var (root, isLocal, err) = fetch(r.Mod);
                    if (err != null)
                    {
                        return (r, error.As(err)!);
                    }

                    m = match(r.Mod, root, isLocal);
                    r.Packages = m.Pkgs;
                    if (len(r.Packages) == 0L)
                    {
                        {
                            var err__prev2 = err;

                            err = firstError(m);

                            if (err != null)
                            {
                                return (r, error.As(err)!);
                            }

                            err = err__prev2;

                        }

                        return (r, error.As(addr(new PackageNotInModuleError(Mod:r.Mod,Replacement:Replacement(r.Mod),Query:query,Pattern:pattern,))!)!);

                    }

                    return (r, error.As(null!)!);

                }
;

                err = default!;
                results, err = queryPrefixModules(candidateModules, queryModule);
                return err;

            });

            return (results, error.As(err)!);

        }

        // modulePrefixesExcludingTarget returns all prefixes of path that may plausibly
        // exist as a module, excluding targetPrefix but otherwise including path
        // itself, sorted by descending length.
        private static slice<@string> modulePrefixesExcludingTarget(@string path)
        {
            var prefixes = make_slice<@string>(0L, strings.Count(path, "/") + 1L);

            while (true)
            {
                if (path != targetPrefix)
                {
                    {
                        var (_, _, ok) = module.SplitPathVersion(path);

                        if (ok)
                        {
                            prefixes = append(prefixes, path);
                        }

                    }

                }

                var j = strings.LastIndexByte(path, '/');
                if (j < 0L)
                {
                    break;
                }

                path = path[..j];

            }


            return prefixes;

        }

        private static @string findCurrentVersion(@string path)
        {
            foreach (var (_, m) in buildList)
            {
                if (m.Path == path)
                {
                    return m.Version;
                }

            }
            return "";

        }

        private partial struct prefixResult
        {
            public ref QueryResult QueryResult => ref QueryResult_val;
            public error err;
        }

        private static (slice<QueryResult>, error) queryPrefixModules(slice<@string> candidateModules, Func<@string, (QueryResult, error)> queryModule) => func((_, panic, __) =>
        {
            slice<QueryResult> found = default;
            error err = default!;
 
            // If the path we're attempting is not in the module cache and we don't have a
            // fetch result cached either, we'll end up making a (potentially slow)
            // request to the proxy or (often even slower) the origin server.
            // To minimize latency, execute all of those requests in parallel.
            private partial struct result
            {
                public ref QueryResult QueryResult => ref QueryResult_val;
                public error err;
            }
            var results = make_slice<result>(len(candidateModules));
            sync.WaitGroup wg = default;
            wg.Add(len(candidateModules));
            foreach (var (i, p) in candidateModules)
            {
                go_(() => (p, r) =>
                {
                    r.QueryResult, r.err = queryModule(p);
                    wg.Done();
                }(p, _addr_results[i]));

            }
            wg.Wait(); 

            // Classify the results. In case of failure, identify the error that the user
            // is most likely to find helpful: the most useful class of error at the
            // longest matching path.
            ptr<PackageNotInModuleError> noPackage;            ptr<NoMatchingVersionError> noVersion;            error notExistErr = default!;
            foreach (var (_, r) in results)
            {
                switch (r.err.type())
                {
                    case 
                        found = append(found, r.QueryResult);
                        break;
                    case ptr<PackageNotInModuleError> rErr:
                        if (noPackage == null || noPackage.Mod == Target)
                        {
                            noPackage = rErr;
                        }

                        break;
                    case ptr<NoMatchingVersionError> rErr:
                        if (noVersion == null)
                        {
                            noVersion = rErr;
                        }

                        break;
                    default:
                    {
                        var rErr = r.err.type();
                        if (errors.Is(rErr, os.ErrNotExist))
                        {
                            if (notExistErr == null)
                            {
                                notExistErr = error.As(rErr)!;
                            }

                        }
                        else if (err == null)
                        {
                            if (len(found) > 0L || noPackage != null)
                            { 
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

            } 

            // TODO(#26232): If len(found) == 0 and some of the errors are 4xx HTTP
            // codes, have the auth package recheck the failed paths.
            // If we obtain new credentials for any of them, re-run the above loop.
            if (len(found) == 0L && err == null)
            {

                if (noPackage != null) 
                    err = noPackage;
                else if (noVersion != null) 
                    err = noVersion;
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
        // NOTE: NoMatchingVersionError MUST NOT implement Is(os.ErrNotExist).
        //
        // If the module came from a proxy, that proxy had to return a successful status
        // code for the versions it knows about, and thus did not have the opportunity
        // to return a non-400 status code to suppress fallback.
        public partial struct NoMatchingVersionError
        {
            public @string query;
            public @string current;
        }

        private static @string Error(this ptr<NoMatchingVersionError> _addr_e)
        {
            ref NoMatchingVersionError e = ref _addr_e.val;

            @string currentSuffix = "";
            if ((e.query == "upgrade" || e.query == "patch") && e.current != "")
            {
                currentSuffix = fmt.Sprintf(" (current version is %s)", e.current);
            }

            return fmt.Sprintf("no matching versions for query %q", e.query) + currentSuffix;

        }

        // A PackageNotInModuleError indicates that QueryPattern found a candidate
        // module at the requested version, but that module did not contain any packages
        // matching the requested pattern.
        //
        // NOTE: PackageNotInModuleError MUST NOT implement Is(os.ErrNotExist).
        //
        // If the module came from a proxy, that proxy had to return a successful status
        // code for the versions it knows about, and thus did not have the opportunity
        // to return a non-400 status code to suppress fallback.
        public partial struct PackageNotInModuleError
        {
            public module.Version Mod;
            public module.Version Replacement;
            public @string Query;
            public @string Pattern;
        }

        private static @string Error(this ptr<PackageNotInModuleError> _addr_e)
        {
            ref PackageNotInModuleError e = ref _addr_e.val;

            if (e.Mod == Target)
            {
                if (strings.Contains(e.Pattern, "..."))
                {
                    return fmt.Sprintf("main module (%s) does not contain packages matching %s", Target.Path, e.Pattern);
                }

                return fmt.Sprintf("main module (%s) does not contain package %s", Target.Path, e.Pattern);

            }

            @string found = "";
            {
                var r = e.Replacement;

                if (r.Path != "")
                {
                    var replacement = r.Path;
                    if (r.Version != "")
                    {
                        replacement = fmt.Sprintf("%s@%s", r.Path, r.Version);
                    }

                    if (e.Query == e.Mod.Version)
                    {
                        found = fmt.Sprintf(" (replaced by %s)", replacement);
                    }
                    else
                    {
                        found = fmt.Sprintf(" (%s, replaced by %s)", e.Mod.Version, replacement);
                    }

                }
                else if (e.Query != e.Mod.Version)
                {
                    found = fmt.Sprintf(" (%s)", e.Mod.Version);
                }


            }


            if (strings.Contains(e.Pattern, "..."))
            {
                return fmt.Sprintf("module %s@%s found%s, but does not contain packages matching %s", e.Mod.Path, e.Query, found, e.Pattern);
            }

            return fmt.Sprintf("module %s@%s found%s, but does not contain package %s", e.Mod.Path, e.Query, found, e.Pattern);

        }

        private static @string ImportPath(this ptr<PackageNotInModuleError> _addr_e)
        {
            ref PackageNotInModuleError e = ref _addr_e.val;

            if (!strings.Contains(e.Pattern, "..."))
            {
                return e.Pattern;
            }

            return "";

        }

        // ModuleHasRootPackage returns whether module m contains a package m.Path.
        public static (bool, error) ModuleHasRootPackage(module.Version m)
        {
            bool _p0 = default;
            error _p0 = default!;

            var (root, isLocal, err) = fetch(m);
            if (err != null)
            {
                return (false, error.As(err)!);
            }

            var (_, ok, err) = dirInModule(m.Path, m.Path, root, isLocal);
            return (ok, error.As(err)!);

        }

        private static (bool, error) versionHasGoMod(module.Version m)
        {
            bool _p0 = default;
            error _p0 = default!;

            var (root, _, err) = fetch(m);
            if (err != null)
            {
                return (false, error.As(err)!);
            }

            var (fi, err) = os.Stat(filepath.Join(root, "go.mod"));
            return (err == null && !fi.IsDir(), error.As(null!)!);

        }
    }
}}}}
