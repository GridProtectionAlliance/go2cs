// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2022 March 06 23:18:13 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modload\list.go
using context = go.context_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using modinfo = go.cmd.go.@internal.modinfo_package;
using search = go.cmd.go.@internal.search_package;

using module = go.golang.org.x.mod.module_package;
using System;
using System.Threading;


namespace go.cmd.go.@internal;

public static partial class modload_package {

public partial struct ListMode { // : nint
}

public static readonly ListMode ListU = 1 << (int)(iota);
public static readonly var ListRetracted = 0;
public static readonly var ListDeprecated = 1;
public static readonly var ListVersions = 2;
public static readonly var ListRetractedVersions = 3;


// ListModules returns a description of the modules matching args, if known,
// along with any error preventing additional matches from being identified.
//
// The returned slice can be nonempty even if the error is non-nil.
public static (slice<ptr<modinfo.ModulePublic>>, error) ListModules(context.Context ctx, slice<@string> args, ListMode mode) {
    slice<ptr<modinfo.ModulePublic>> _p0 = default;
    error _p0 = default!;

    var (rs, mods, err) = listModules(ctx, _addr_LoadModFile(ctx), args, mode);

    private partial struct token {
    }
    var sem = make_channel<token>(runtime.GOMAXPROCS(0));
    if (mode != 0) {
        foreach (var (_, m) in mods) {
            Action<ptr<modinfo.ModulePublic>> add = m => {
                sem.Send(new token());
                go_(() => () => {
                    if (mode & ListU != 0) {
                        addUpdate(ctx, m);
                    }
                    if (mode & ListVersions != 0) {
                        addVersions(ctx, m, mode & ListRetractedVersions != 0);
                    }
                    if (mode & ListRetracted != 0) {
                        addRetraction(ctx, m);
                    }
                    if (mode & ListDeprecated != 0) {
                        addDeprecation(ctx, m);
                    }
                    sem.Receive();
                }());
            }
;

            add(m);
            if (m.Replace != null) {
                add(m.Replace);
            }
        }
    }
    for (var n = cap(sem); n > 0; n--) {
        sem.Send(new token());
    }

    if (err == null) {
        commitRequirements(ctx, modFileGoVersion(), rs);
    }
    return (mods, error.As(err)!);

}

private static (ptr<Requirements>, slice<ptr<modinfo.ModulePublic>>, error) listModules(context.Context ctx, ptr<Requirements> _addr_rs, slice<@string> args, ListMode mode) => func((_, panic, _) => {
    ptr<Requirements> _ = default!;
    slice<ptr<modinfo.ModulePublic>> mods = default;
    error mgErr = default!;
    ref Requirements rs = ref _addr_rs.val;

    if (len(args) == 0) {
        return (_addr_rs!, new slice<ptr<modinfo.ModulePublic>>(new ptr<modinfo.ModulePublic>[] { moduleInfo(ctx,rs,Target,mode) }), error.As(null!)!);
    }
    var needFullGraph = false;
    {
        var arg__prev1 = arg;

        foreach (var (_, __arg) in args) {
            arg = __arg;
            if (strings.Contains(arg, "\\")) {
                @base.Fatalf("go: module paths never use backslash");
            }
            if (search.IsRelativePath(arg)) {
                @base.Fatalf("go: cannot use relative path %s to specify module", arg);
            }
            if (arg == "all" || strings.Contains(arg, "...")) {
                needFullGraph = true;
                if (!HasModRoot()) {
                    @base.Fatalf("go: cannot match %q: %v", arg, ErrNoModRoot);
                }
                continue;
            }
            {
                var i__prev1 = i;

                var i = strings.Index(arg, "@");

                if (i >= 0) {
                    var path = arg[..(int)i];
                    var vers = arg[(int)i + 1..];
                    if (vers == "upgrade" || vers == "patch") {
                        {
                            var (_, ok) = rs.rootSelected(path);

                            if (!ok || rs.depth == eager) {
                                needFullGraph = true;
                                if (!HasModRoot()) {
                                    @base.Fatalf("go: cannot match %q: %v", arg, ErrNoModRoot);
                                }
                            }

                        }

                    }

                    continue;

                }

                i = i__prev1;

            }

            {
                (_, ok) = rs.rootSelected(arg);

                if (!ok || rs.depth == eager) {
                    needFullGraph = true;
                    if (mode & ListVersions == 0 && !HasModRoot()) {
                        @base.Fatalf("go: cannot match %q without -versions or an explicit version: %v", arg, ErrNoModRoot);
                    }
                }

            }

        }
        arg = arg__prev1;
    }

    ptr<ModuleGraph> mg;
    if (needFullGraph) {
        rs, mg, mgErr = expandGraph(ctx, rs);
    }
    map matchedModule = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<module.Version, bool>{};
    {
        var arg__prev1 = arg;

        foreach (var (_, __arg) in args) {
            arg = __arg;
            {
                var i__prev1 = i;

                i = strings.Index(arg, "@");

                if (i >= 0) {
                    path = arg[..(int)i];
                    vers = arg[(int)i + 1..];

                    @string current = default;
                    if (mg == null) {
                        current, _ = rs.rootSelected(path);
                    }
                    else
 {
                        current = mg.Selected(path);
                    }

                    if (current == "none" && mgErr != null) {
                        if (vers == "upgrade" || vers == "patch") { 
                            // The module graph is incomplete, so we don't know what version we're
                            // actually upgrading from.
                            // mgErr is already set, so just skip this module.
                            continue;

                        }

                    }

                    var allowed = CheckAllowed;
                    if (IsRevisionQuery(vers) || mode & ListRetracted != 0) { 
                        // Allow excluded and retracted versions if the user asked for a
                        // specific revision or used 'go list -retracted'.
                        allowed = null;

                    }

                    var (info, err) = Query(ctx, path, vers, current, allowed);
                    if (err != null) {
                        mods = append(mods, addr(new modinfo.ModulePublic(Path:path,Version:vers,Error:modinfoError(path,vers,err),)));
                        continue;
                    } 

                    // Indicate that m was resolved from outside of rs by passing a nil
                    // *Requirements instead.
                    ptr<Requirements> noRS;

                    var mod = moduleInfo(ctx, noRS, new module.Version(Path:path,Version:info.Version), mode);
                    mods = append(mods, mod);
                    continue;

                } 

                // Module path or pattern.

                i = i__prev1;

            } 

            // Module path or pattern.
            Func<@string, bool> match = default;
            if (arg == "all") {
                match = _p0 => _addr_true!;
            }
            else if (strings.Contains(arg, "...")) {
                match = search.MatchPattern(arg);
            }
            else
 {
                @string v = default;
                if (mg == null) {
                    bool ok = default;
                    v, ok = rs.rootSelected(arg);
                    if (!ok) { 
                        // We checked rootSelected(arg) in the earlier args loop, so if there
                        // is no such root we should have loaded a non-nil mg.
                        panic(fmt.Sprintf("internal error: root requirement expected but not found for %v", arg));

                    }

                }
                else
 {
                    v = mg.Selected(arg);
                }

                if (v == "none" && mgErr != null) { 
                    // mgErr is already set, so just skip this module.
                    continue;

                }

                if (v != "none") {
                    mods = append(mods, moduleInfo(ctx, rs, new module.Version(Path:arg,Version:v), mode));
                }
                else if (cfg.BuildMod == "vendor") { 
                    // In vendor mode, we can't determine whether a missing module is “a
                    // known dependency” because the module graph is incomplete.
                    // Give a more explicit error message.
                    mods = append(mods, addr(new modinfo.ModulePublic(Path:arg,Error:modinfoError(arg,"",errors.New("can't resolve module using the vendor directory\n\t(Use -mod=mod or -mod=readonly to bypass.)")),)));

                }
                else if (mode & ListVersions != 0) { 
                    // Don't make the user provide an explicit '@latest' when they're
                    // explicitly asking what the available versions are. Instead, return a
                    // module with version "none", to which we can add the requested list.
                    mods = append(mods, addr(new modinfo.ModulePublic(Path:arg)));

                }
                else
 {
                    mods = append(mods, addr(new modinfo.ModulePublic(Path:arg,Error:modinfoError(arg,"",errors.New("not a known dependency")),)));
                }

                continue;

            }

            var matched = false;
            foreach (var (_, m) in mg.BuildList()) {
                if (match(m.Path)) {
                    matched = true;
                    if (!matchedModule[m]) {
                        matchedModule[m] = true;
                        mods = append(mods, moduleInfo(ctx, rs, m, mode));
                    }
                }
            }
            if (!matched) {
                fmt.Fprintf(os.Stderr, "warning: pattern %q matched no module dependencies\n", arg);
            }

        }
        arg = arg__prev1;
    }

    return (_addr_rs!, mods, error.As(mgErr)!);

});

// modinfoError wraps an error to create an error message in
// modinfo.ModuleError with minimal redundancy.
private static ptr<modinfo.ModuleError> modinfoError(@string path, @string vers, error err) {
    ptr<NoMatchingVersionError> nerr;
    ptr<module.ModuleError> merr;
    if (errors.As(err, _addr_nerr)) { 
        // NoMatchingVersionError contains the query, so we don't mention the
        // query again in ModuleError.
        err = addr(new module.ModuleError(Path:path,Err:err));

    }
    else if (!errors.As(err, _addr_merr)) { 
        // If the error does not contain path and version, wrap it in a
        // module.ModuleError.
        err = addr(new module.ModuleError(Path:path,Version:vers,Err:err));

    }
    return addr(new modinfo.ModuleError(Err:err.Error()));

}

} // end modload_package
