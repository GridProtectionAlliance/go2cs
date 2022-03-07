// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 06 23:18:23 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\modfile.go
using context = go.context_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using sync = go.sync_package;
using unicode = go.unicode_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using fsys = go.cmd.go.@internal.fsys_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using par = go.cmd.go.@internal.par_package;
using trace = go.cmd.go.@internal.trace_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using System;


namespace go.cmd.go.@internal;

public static partial class modload_package {

 
// narrowAllVersionV is the Go version (plus leading "v") at which the
// module-module "all" pattern no longer closes over the dependencies of
// tests outside of the main module.
private static readonly @string narrowAllVersionV = "v1.16"; 

// lazyLoadingVersionV is the Go version (plus leading "v") at which a
// module's go.mod file is expected to list explicit requirements on every
// module that provides any package transitively imported by that module.
private static readonly @string lazyLoadingVersionV = "v1.17"; 

// separateIndirectVersionV is the Go version (plus leading "v") at which
// "// indirect" dependencies are added in a block separate from the direct
// ones. See https://golang.org/issue/45965.
private static readonly @string separateIndirectVersionV = "v1.17";


 
// go117EnableLazyLoading toggles whether lazy-loading code paths should be
// active. It will be removed once the lazy loading implementation is stable
// and well-tested.
private static readonly var go117EnableLazyLoading = true; 

// go1117LazyTODO is a constant that exists only until lazy loading is
// implemented. Its use indicates a condition that will need to change if the
// main module is lazy.
private static readonly var go117LazyTODO = false;


private static ptr<modfile.File> modFile;

// modFileGoVersion returns the (non-empty) Go version at which the requirements
// in modFile are intepreted, or the latest Go version if modFile is nil.
private static @string modFileGoVersion() {
    if (modFile == null) {
        return LatestGoVersion();
    }
    if (modFile.Go == null || modFile.Go.Version == "") { 
        // The main module necessarily has a go.mod file, and that file lacks a
        // 'go' directive. The 'go' command has been adding that directive
        // automatically since Go 1.12, so this module either dates to Go 1.11 or
        // has been erroneously hand-edited.
        //
        // The semantics of the go.mod file are more-or-less the same from Go 1.11
        // through Go 1.16, changing at 1.17 for lazy loading. So even though a
        // go.mod file without a 'go' directive is theoretically a Go 1.11 file,
        // scripts may assume that it ends up as a Go 1.16 module.
        return "1.16";

    }
    return modFile.Go.Version;

}

// A modFileIndex is an index of data corresponding to a modFile
// at a specific point in time.
private partial struct modFileIndex {
    public slice<byte> data;
    public bool dataNeedsFix; // true if fixVersion applied a change while parsing data
    public module.Version module;
    public @string goVersionV; // GoVersion with "v" prefix
    public map<module.Version, requireMeta> require;
    public map<module.Version, module.Version> replace;
    public map<@string, @string> highestReplaced; // highest replaced version of each module path; empty string for wildcard-only replacements
    public map<module.Version, bool> exclude;
}

// index is the index of the go.mod file as of when it was last read or written.
private static ptr<modFileIndex> index;

private partial struct requireMeta {
    public bool indirect;
}

// A modDepth indicates which dependencies should be loaded for a go.mod file.
private partial struct modDepth { // : byte
}

private static readonly modDepth lazy = iota; // load dependencies only as needed
private static readonly var eager = 0; // load all transitive dependencies eagerly

private static modDepth modDepthFromGoVersion(@string goVersion) {
    if (!go117EnableLazyLoading) {
        return eager;
    }
    if (semver.Compare("v" + goVersion, lazyLoadingVersionV) < 0) {
        return eager;
    }
    return lazy;

}

// CheckAllowed returns an error equivalent to ErrDisallowed if m is excluded by
// the main module's go.mod or retracted by its author. Most version queries use
// this to filter out versions that should not be used.
public static error CheckAllowed(context.Context ctx, module.Version m) {
    {
        var err__prev1 = err;

        var err = CheckExclusions(ctx, m);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = CheckRetractions(ctx, m);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    return error.As(null!)!;

}

// ErrDisallowed is returned by version predicates passed to Query and similar
// functions to indicate that a version should not be considered.
public static var ErrDisallowed = errors.New("disallowed module version");

// CheckExclusions returns an error equivalent to ErrDisallowed if module m is
// excluded by the main module's go.mod file.
public static error CheckExclusions(context.Context ctx, module.Version m) {
    if (index != null && index.exclude[m]) {
        return error.As(module.VersionError(m, errExcluded))!;
    }
    return error.As(null!)!;

}

private static ptr<excludedError> errExcluded = addr(new excludedError());

private partial struct excludedError {
}

private static @string Error(this ptr<excludedError> _addr_e) {
    ref excludedError e = ref _addr_e.val;

    return "excluded by go.mod";
}
private static bool Is(this ptr<excludedError> _addr_e, error err) {
    ref excludedError e = ref _addr_e.val;

    return err == ErrDisallowed;
}

// CheckRetractions returns an error if module m has been retracted by
// its author.
public static error CheckRetractions(context.Context ctx, module.Version m) => func((defer, _, _) => {
    error err = default!;

    defer(() => {
        {
            ref var retractErr = ref heap((ModuleRetractedError.val)(null), out ptr<var> _addr_retractErr);

            if (err == null || errors.As(err, _addr_retractErr)) {
                return ;
            } 
            // Attribute the error to the version being checked, not the version from
            // which the retractions were to be loaded.

        } 
        // Attribute the error to the version being checked, not the version from
        // which the retractions were to be loaded.
        {
            ref var mErr = ref heap((module.ModuleError.val)(null), out ptr<var> _addr_mErr);

            if (errors.As(err, _addr_mErr)) {
                err = mErr.Err;
            }

        }

        err = addr(new retractionLoadingError(m:m,err:err));

    }());

    if (m.Version == "") { 
        // Main module, standard library, or file replacement module.
        // Cannot be retracted.
        return error.As(null!)!;

    }
    {
        var repl = Replacement(new module.Version(Path:m.Path));

        if (repl.Path != "") { 
            // All versions of the module were replaced.
            // Don't load retractions, since we'd just load the replacement.
            return error.As(null!)!;

        }
    } 

    // Find the latest available version of the module, and load its go.mod. If
    // the latest version is replaced, we'll load the replacement.
    //
    // If there's an error loading the go.mod, we'll return it here. These errors
    // should generally be ignored by callers since they happen frequently when
    // we're offline. These errors are not equivalent to ErrDisallowed, so they
    // may be distinguished from retraction errors.
    //
    // We load the raw file here: the go.mod file may have a different module
    // path that we expect if the module or its repository was renamed.
    // We still want to apply retractions to other aliases of the module.
    var (rm, err) = queryLatestVersionIgnoringRetractions(ctx, m.Path);
    if (err != null) {
        return error.As(err)!;
    }
    var (summary, err) = rawGoModSummary(rm);
    if (err != null) {
        return error.As(err)!;
    }
    slice<@string> rationale = default;
    var isRetracted = false;
    foreach (var (_, r) in summary.retract) {
        if (semver.Compare(r.Low, m.Version) <= 0 && semver.Compare(m.Version, r.High) <= 0) {
            isRetracted = true;
            if (r.Rationale != "") {
                rationale = append(rationale, r.Rationale);
            }
        }
    }    if (isRetracted) {
        return error.As(module.VersionError(m, addr(new ModuleRetractedError(Rationale:rationale))))!;
    }
    return error.As(null!)!;

});

public partial struct ModuleRetractedError {
    public slice<@string> Rationale;
}

private static @string Error(this ptr<ModuleRetractedError> _addr_e) {
    ref ModuleRetractedError e = ref _addr_e.val;

    @string msg = "retracted by module author";
    if (len(e.Rationale) > 0) { 
        // This is meant to be a short error printed on a terminal, so just
        // print the first rationale.
        msg += ": " + ShortMessage(e.Rationale[0], "retracted by module author");

    }
    return msg;

}

private static bool Is(this ptr<ModuleRetractedError> _addr_e, error err) {
    ref ModuleRetractedError e = ref _addr_e.val;

    return err == ErrDisallowed;
}

private partial struct retractionLoadingError {
    public module.Version m;
    public error err;
}

private static @string Error(this ptr<retractionLoadingError> _addr_e) {
    ref retractionLoadingError e = ref _addr_e.val;

    return fmt.Sprintf("loading module retractions for %v: %v", e.m, e.err);
}

private static error Unwrap(this ptr<retractionLoadingError> _addr_e) {
    ref retractionLoadingError e = ref _addr_e.val;

    return error.As(e.err)!;
}

// ShortMessage returns a string from go.mod (for example, a retraction
// rationale or deprecation message) that is safe to print in a terminal.
//
// If the given string is empty, ShortMessage returns the given default. If the
// given string is too long or contains non-printable characters, ShortMessage
// returns a hard-coded string.
public static @string ShortMessage(@string message, @string emptyDefault) {
    const nint maxLen = 500;

    {
        var i = strings.Index(message, "\n");

        if (i >= 0) {
            message = message[..(int)i];
        }
    }

    message = strings.TrimSpace(message);
    if (message == "") {
        return emptyDefault;
    }
    if (len(message) > maxLen) {
        return "(message omitted: too long)";
    }
    foreach (var (_, r) in message) {
        if (!unicode.IsGraphic(r) && !unicode.IsSpace(r)) {
            return "(message omitted: contains non-printable characters)";
        }
    }    return message;

}

// CheckDeprecation returns a deprecation message from the go.mod file of the
// latest version of the given module. Deprecation messages are comments
// before or on the same line as the module directives that start with
// "Deprecated:" and run until the end of the paragraph.
//
// CheckDeprecation returns an error if the message can't be loaded.
// CheckDeprecation returns "", nil if there is no deprecation message.
public static (@string, error) CheckDeprecation(context.Context ctx, module.Version m) => func((defer, _, _) => {
    @string deprecation = default;
    error err = default!;

    defer(() => {
        if (err != null) {
            err = fmt.Errorf("loading deprecation for %s: %w", m.Path, err);
        }
    }());

    if (m.Version == "") { 
        // Main module, standard library, or file replacement module.
        // Don't look up deprecation.
        return ("", error.As(null!)!);

    }
    {
        var repl = Replacement(new module.Version(Path:m.Path));

        if (repl.Path != "") { 
            // All versions of the module were replaced.
            // We'll look up deprecation separately for the replacement.
            return ("", error.As(null!)!);

        }
    }


    var (latest, err) = queryLatestVersionIgnoringRetractions(ctx, m.Path);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var (summary, err) = rawGoModSummary(latest);
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (summary.deprecated, error.As(null!)!);

});

// Replacement returns the replacement for mod, if any, from go.mod.
// If there is no replacement for mod, Replacement returns
// a module.Version with Path == "".
public static module.Version Replacement(module.Version mod) {
    if (index != null) {
        {
            var r__prev2 = r;

            var (r, ok) = index.replace[mod];

            if (ok) {
                return r;
            }

            r = r__prev2;

        }

        {
            var r__prev2 = r;

            (r, ok) = index.replace[new module.Version(Path:mod.Path)];

            if (ok) {
                return r;
            }

            r = r__prev2;

        }

    }
    return new module.Version();

}

// resolveReplacement returns the module actually used to load the source code
// for m: either m itself, or the replacement for m (iff m is replaced).
private static module.Version resolveReplacement(module.Version m) {
    {
        var r = Replacement(m);

        if (r.Path != "") {
            return r;
        }
    }

    return m;

}

// indexModFile rebuilds the index of modFile.
// If modFile has been changed since it was first read,
// modFile.Cleanup must be called before indexModFile.
private static ptr<modFileIndex> indexModFile(slice<byte> data, ptr<modfile.File> _addr_modFile, bool needsFix) {
    ref modfile.File modFile = ref _addr_modFile.val;

    ptr<modFileIndex> i = @new<modFileIndex>();
    i.data = data;
    i.dataNeedsFix = needsFix;

    i.module = new module.Version();
    if (modFile.Module != null) {
        i.module = modFile.Module.Mod;
    }
    i.goVersionV = "";
    if (modFile.Go == null) {
        rawGoVersion.Store(Target, "");
    }
    else
 { 
        // We're going to use the semver package to compare Go versions, so go ahead
        // and add the "v" prefix it expects once instead of every time.
        i.goVersionV = "v" + modFile.Go.Version;
        rawGoVersion.Store(Target, modFile.Go.Version);

    }
    i.require = make_map<module.Version, requireMeta>(len(modFile.Require));
    {
        var r__prev1 = r;

        foreach (var (_, __r) in modFile.Require) {
            r = __r;
            i.require[r.Mod] = new requireMeta(indirect:r.Indirect);
        }
        r = r__prev1;
    }

    i.replace = make_map<module.Version, module.Version>(len(modFile.Replace));
    {
        var r__prev1 = r;

        foreach (var (_, __r) in modFile.Replace) {
            r = __r;
            {
                var (prev, dup) = i.replace[r.Old];

                if (dup && prev != r.New) {
                    @base.Fatalf("go: conflicting replacements for %v:\n\t%v\n\t%v", r.Old, prev, r.New);
                }

            }

            i.replace[r.Old] = r.New;

        }
        r = r__prev1;
    }

    i.highestReplaced = make_map<@string, @string>();
    {
        var r__prev1 = r;

        foreach (var (_, __r) in modFile.Replace) {
            r = __r;
            var (v, ok) = i.highestReplaced[r.Old.Path];
            if (!ok || semver.Compare(r.Old.Version, v) > 0) {
                i.highestReplaced[r.Old.Path] = r.Old.Version;
            }
        }
        r = r__prev1;
    }

    i.exclude = make_map<module.Version, bool>(len(modFile.Exclude));
    foreach (var (_, x) in modFile.Exclude) {
        i.exclude[x.Mod] = true;
    }    return _addr_i!;

}

// modFileIsDirty reports whether the go.mod file differs meaningfully
// from what was indexed.
// If modFile has been changed (even cosmetically) since it was first read,
// modFile.Cleanup must be called before modFileIsDirty.
private static bool modFileIsDirty(this ptr<modFileIndex> _addr_i, ptr<modfile.File> _addr_modFile) {
    ref modFileIndex i = ref _addr_i.val;
    ref modfile.File modFile = ref _addr_modFile.val;

    if (i == null) {
        return modFile != null;
    }
    if (i.dataNeedsFix) {
        return true;
    }
    if (modFile.Module == null) {
        if (i.module != (new module.Version())) {
            return true;
        }
    }
    else if (modFile.Module.Mod != i.module) {
        return true;
    }
    if (modFile.Go == null) {
        if (i.goVersionV != "") {
            return true;
        }
    }
    else if ("v" + modFile.Go.Version != i.goVersionV) {
        if (i.goVersionV == "" && cfg.BuildMod != "mod") { 
            // go.mod files did not always require a 'go' version, so do not error out
            // if one is missing — we may be inside an older module in the module
            // cache, and should bias toward providing useful behavior.
        }
        else
 {
            return true;
        }
    }
    if (len(modFile.Require) != len(i.require) || len(modFile.Replace) != len(i.replace) || len(modFile.Exclude) != len(i.exclude)) {
        return true;
    }
    {
        var r__prev1 = r;

        foreach (var (_, __r) in modFile.Require) {
            r = __r;
            {
                var (meta, ok) = i.require[r.Mod];

                if (!ok) {
                    return true;
                }
                else if (r.Indirect != meta.indirect) {
                    if (cfg.BuildMod == "readonly") { 
                        // The module's requirements are consistent; only the "// indirect"
                        // comments that are wrong. But those are only guaranteed to be accurate
                        // after a "go mod tidy" — it's a good idea to run those before
                        // committing a change, but it's certainly not mandatory.
                    }
                    else
 {
                        return true;
                    }

                }


            }

        }
        r = r__prev1;
    }

    {
        var r__prev1 = r;

        foreach (var (_, __r) in modFile.Replace) {
            r = __r;
            if (r.New != i.replace[r.Old]) {
                return true;
            }
        }
        r = r__prev1;
    }

    foreach (var (_, x) in modFile.Exclude) {
        if (!i.exclude[x.Mod]) {
            return true;
        }
    }    return false;

}

// rawGoVersion records the Go version parsed from each module's go.mod file.
//
// If a module is replaced, the version of the replacement is keyed by the
// replacement module.Version, not the version being replaced.
private static sync.Map rawGoVersion = default; // map[module.Version]string

// A modFileSummary is a summary of a go.mod file for which we do not need to
// retain complete information — for example, the go.mod file of a dependency
// module.
private partial struct modFileSummary {
    public module.Version module;
    public @string goVersion;
    public modDepth depth;
    public slice<module.Version> require;
    public slice<retraction> retract;
    public @string deprecated;
}

// A retraction consists of a retracted version interval and rationale.
// retraction is like modfile.Retract, but it doesn't point to the syntax tree.
private partial struct retraction {
    public ref modfile.VersionInterval VersionInterval => ref VersionInterval_val;
    public @string Rationale;
}

// goModSummary returns a summary of the go.mod file for module m,
// taking into account any replacements for m, exclusions of its dependencies,
// and/or vendoring.
//
// m must be a version in the module graph, reachable from the Target module.
// In readonly mode, the go.sum file must contain an entry for m's go.mod file
// (or its replacement). goModSummary must not be called for the Target module
// itself, as its requirements may change. Use rawGoModSummary for other
// module versions.
//
// The caller must not modify the returned summary.
private static (ptr<modFileSummary>, error) goModSummary(module.Version m) => func((_, panic, _) => {
    ptr<modFileSummary> _p0 = default!;
    error _p0 = default!;

    if (m == Target) {
        panic("internal error: goModSummary called on the Target module");
    }
    if (cfg.BuildMod == "vendor") {
        ptr<modFileSummary> summary = addr(new modFileSummary(module:module.Version{Path:m.Path},));
        if (vendorVersion[m.Path] != m.Version) { 
            // This module is not vendored, so packages cannot be loaded from it and
            // it cannot be relevant to the build.
            return (_addr_summary!, error.As(null!)!);

        }
        readVendorList(); 

        // We don't know what versions the vendored module actually relies on,
        // so assume that it requires everything.
        summary.require = vendorList;
        return (_addr_summary!, error.As(null!)!);

    }
    var actual = resolveReplacement(m);
    if (HasModRoot() && cfg.BuildMod == "readonly" && actual.Version != "") {
        module.Version key = new module.Version(Path:actual.Path,Version:actual.Version+"/go.mod");
        if (!modfetch.HaveSum(key)) {
            var suggestion = fmt.Sprintf("; to add it:\n\tgo mod download %s", m.Path);
            return (_addr_null!, error.As(module.VersionError(actual, addr(new sumMissingError(suggestion:suggestion))))!);
        }
    }
    var (summary, err) = rawGoModSummary(actual);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (actual.Version == "") { 
        // The actual module is a filesystem-local replacement, for which we have
        // unfortunately not enforced any sort of invariants about module lines or
        // matching module paths. Anything goes.
        //
        // TODO(bcmills): Remove this special-case, update tests, and add a
        // release note.
    }
    else
 {
        if (summary.module.Path == "") {
            return (_addr_null!, error.As(module.VersionError(actual, errors.New("parsing go.mod: missing module line")))!);
        }
        {
            var mpath = summary.module.Path;

            if (mpath != m.Path && mpath != actual.Path) {
                return (_addr_null!, error.As(module.VersionError(actual, fmt.Errorf("parsing go.mod:\n\tmodule declares its path as: %s\n\t        but was required as: %s" +
    "", mpath, m.Path)))!);

            }

        }

    }
    if (index != null && len(index.exclude) > 0) { 
        // Drop any requirements on excluded versions.
        // Don't modify the cached summary though, since we might need the raw
        // summary separately.
        var haveExcludedReqs = false;
        {
            var r__prev1 = r;

            foreach (var (_, __r) in summary.require) {
                r = __r;
                if (index.exclude[r]) {
                    haveExcludedReqs = true;
                    break;
                }
            }

            r = r__prev1;
        }

        if (haveExcludedReqs) {
            ptr<modFileSummary> s = @new<modFileSummary>();
            s.val = summary.val;
            s.require = make_slice<module.Version>(0, len(summary.require));
            {
                var r__prev1 = r;

                foreach (var (_, __r) in summary.require) {
                    r = __r;
                    if (!index.exclude[r]) {
                        s.require = append(s.require, r);
                    }
                }

                r = r__prev1;
            }

            summary = addr(s);

        }
    }
    return (_addr_summary!, error.As(null!)!);

});

// rawGoModSummary returns a new summary of the go.mod file for module m,
// ignoring all replacements that may apply to m and excludes that may apply to
// its dependencies.
//
// rawGoModSummary cannot be used on the Target module.
private static (ptr<modFileSummary>, error) rawGoModSummary(module.Version m) => func((_, panic, _) => {
    ptr<modFileSummary> _p0 = default!;
    error _p0 = default!;

    if (m == Target) {
        panic("internal error: rawGoModSummary called on the Target module");
    }
    private partial struct cached {
        public ptr<modFileSummary> summary;
        public error err;
    }
    cached c = rawGoModSummaryCache.Do(m, () => {
        ptr<modFileSummary> summary = @new<modFileSummary>();
        var (name, data, err) = rawGoModData(m);
        if (err != null) {
            return _addr_new cached(nil,err)!;
        }
        var (f, err) = modfile.ParseLax(name, data, null);
        if (err != null) {
            return _addr_new cached(nil,module.VersionError(m,fmt.Errorf("parsing %s: %v",base.ShortPath(name),err)))!;
        }
        if (f.Module != null) {
            summary.module = f.Module.Mod;
            summary.deprecated = f.Module.Deprecated;
        }
        if (f.Go != null && f.Go.Version != "") {
            rawGoVersion.LoadOrStore(m, f.Go.Version);
            summary.goVersion = f.Go.Version;
            summary.depth = modDepthFromGoVersion(f.Go.Version);
        }
        else
 {
            summary.depth = eager;
        }
        if (len(f.Require) > 0) {
            summary.require = make_slice<module.Version>(0, len(f.Require));
            foreach (var (_, req) in f.Require) {
                summary.require = append(summary.require, req.Mod);
            }
        }
        if (len(f.Retract) > 0) {
            summary.retract = make_slice<retraction>(0, len(f.Retract));
            foreach (var (_, ret) in f.Retract) {
                summary.retract = append(summary.retract, new retraction(VersionInterval:ret.VersionInterval,Rationale:ret.Rationale,));
            }
        }
        return _addr_new cached(summary,nil)!;

    })._<cached>();

    return (_addr_c.summary!, error.As(c.err)!);

});

private static par.Cache rawGoModSummaryCache = default; // module.Version → rawGoModSummary result

// rawGoModData returns the content of the go.mod file for module m, ignoring
// all replacements that may apply to m.
//
// rawGoModData cannot be used on the Target module.
//
// Unlike rawGoModSummary, rawGoModData does not cache its results in memory.
// Use rawGoModSummary instead unless you specifically need these bytes.
private static (@string, slice<byte>, error) rawGoModData(module.Version m) {
    @string name = default;
    slice<byte> data = default;
    error err = default!;

    if (m.Version == "") { 
        // m is a replacement module with only a file path.
        var dir = m.Path;
        if (!filepath.IsAbs(dir)) {
            dir = filepath.Join(ModRoot(), dir);
        }
        name = filepath.Join(dir, "go.mod");
        {
            var (gomodActual, ok) = fsys.OverlayPath(name);

            if (ok) { 
                // Don't lock go.mod if it's part of the overlay.
                // On Plan 9, locking requires chmod, and we don't want to modify any file
                // in the overlay. See #44700.
                data, err = os.ReadFile(gomodActual);

            }
            else
 {
                data, err = lockedfile.Read(gomodActual);
            }

        }

        if (err != null) {
            return ("", null, error.As(module.VersionError(m, fmt.Errorf("reading %s: %v", @base.ShortPath(name), err)))!);
        }
    }
    else
 {
        if (!semver.IsValid(m.Version)) { 
            // Disallow the broader queries supported by fetch.Lookup.
            @base.Fatalf("go: internal error: %s@%s: unexpected invalid semantic version", m.Path, m.Version);

        }
        name = "go.mod";
        data, err = modfetch.GoMod(m.Path, m.Version);

    }
    return (name, data, error.As(err)!);

}

// queryLatestVersionIgnoringRetractions looks up the latest version of the
// module with the given path without considering retracted or excluded
// versions.
//
// If all versions of the module are replaced,
// queryLatestVersionIgnoringRetractions returns the replacement without making
// a query.
//
// If the queried latest version is replaced,
// queryLatestVersionIgnoringRetractions returns the replacement.
private static (module.Version, error) queryLatestVersionIgnoringRetractions(context.Context ctx, @string path) => func((defer, _, _) => {
    module.Version latest = default;
    error err = default!;

    private partial struct entry {
        public module.Version latest;
        public error err;
    }
    ptr<entry> e = latestVersionIgnoringRetractionsCache.Do(path, () => {
        var (ctx, span) = trace.StartSpan(ctx, "queryLatestVersionIgnoringRetractions " + path);
        defer(span.Done());

        {
            var repl__prev1 = repl;

            var repl = Replacement(new module.Version(Path:path));

            if (repl.Path != "") { 
                // All versions of the module were replaced.
                // No need to query.
                return addr(new entry(latest:repl));

            } 

            // Find the latest version of the module.
            // Ignore exclusions from the main module's go.mod.

            repl = repl__prev1;

        } 

        // Find the latest version of the module.
        // Ignore exclusions from the main module's go.mod.
        const @string ignoreSelected = "";

        AllowedFunc allowAll = default;
        var (rev, err) = Query(ctx, path, "latest", ignoreSelected, allowAll);
        if (err != null) {
            return addr(new entry(err:err));
        }
        module.Version latest = new module.Version(Path:path,Version:rev.Version);
        {
            var repl__prev1 = repl;

            repl = resolveReplacement(latest);

            if (repl.Path != "") {
                latest = repl;
            }

            repl = repl__prev1;

        }

        return addr(new entry(latest:latest));

    })._<ptr<entry>>();
    return (e.latest, error.As(e.err)!);

});

private static par.Cache latestVersionIgnoringRetractionsCache = default; // path → queryLatestVersionIgnoringRetractions result

} // end modload_package
