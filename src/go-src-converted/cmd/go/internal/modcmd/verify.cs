// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modcmd -- go2cs converted at 2022 March 06 23:19:39 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modcmd\verify.go
using bytes = go.bytes_package;
using context = go.context_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using fs = go.io.fs_package;
using os = go.os_package;
using runtime = go.runtime_package;

using @base = go.cmd.go.@internal.@base_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using modload = go.cmd.go.@internal.modload_package;

using module = go.golang.org.x.mod.module_package;
using dirhash = go.golang.org.x.mod.sumdb.dirhash_package;
using System;
using System.Threading;


namespace go.cmd.go.@internal;

public static partial class modcmd_package {

private static ptr<base.Command> cmdVerify = addr(new base.Command(UsageLine:"go mod verify",Short:"verify dependencies have expected content",Long:`
Verify checks that the dependencies of the current module,
which are stored in a local downloaded source cache, have not been
modified since being downloaded. If all the modules are unmodified,
verify prints "all modules verified." Otherwise it reports which
modules have been changed and causes 'go mod' to exit with a
non-zero status.

See https://golang.org/ref/mod#go-mod-verify for more about 'go mod verify'.
	`,Run:runVerify,));

private static void init() {
    @base.AddModCommonFlags(_addr_cmdVerify.Flag);
}

private static void runVerify(context.Context ctx, ptr<base.Command> _addr_cmd, slice<@string> args) {
    ref base.Command cmd = ref _addr_cmd.val;

    if (len(args) != 0) { 
        // NOTE(rsc): Could take a module pattern.
        @base.Fatalf("go mod verify: verify takes no arguments");

    }
    modload.ForceUseModules = true;
    modload.RootMode = modload.NeedRoot; 

    // Only verify up to GOMAXPROCS zips at once.
    private partial struct token {
    }
    var sem = make_channel<token>(runtime.GOMAXPROCS(0)); 

    // Use a slice of result channels, so that the output is deterministic.
    const @string defaultGoVersion = "";

    var mods = modload.LoadModGraph(ctx, defaultGoVersion).BuildList()[(int)1..];
    var errsChans = make_slice<channel<slice<error>>>(len(mods));

    {
        var mod__prev1 = mod;

        foreach (var (__i, __mod) in mods) {
            i = __i;
            mod = __mod;
            sem.Send(new token());
            var errsc = make_channel<slice<error>>(1);
            errsChans[i] = errsc;
            var mod = mod; // use a copy to avoid data races
            go_(() => () => {
                errsc.Send(verifyMod(mod));
                sem.Receive();
            }());

        }
        mod = mod__prev1;
    }

    var ok = true;
    {
        var errsc__prev1 = errsc;

        foreach (var (_, __errsc) in errsChans) {
            errsc = __errsc;
            var errs = errsc.Receive();
            foreach (var (_, err) in errs) {
                @base.Errorf("%s", err);
                ok = false;
            }
        }
        errsc = errsc__prev1;
    }

    if (ok) {
        fmt.Printf("all modules verified\n");
    }
}

private static slice<error> verifyMod(module.Version mod) {
    slice<error> errs = default;
    var (zip, zipErr) = modfetch.CachePath(mod, "zip");
    if (zipErr == null) {
        _, zipErr = os.Stat(zip);
    }
    var (dir, dirErr) = modfetch.DownloadDir(mod);
    var (data, err) = os.ReadFile(zip + "hash");
    if (err != null) {
        if (zipErr != null && errors.Is(zipErr, fs.ErrNotExist) && dirErr != null && errors.Is(dirErr, fs.ErrNotExist)) { 
            // Nothing downloaded yet. Nothing to verify.
            return null;

        }
        errs = append(errs, fmt.Errorf("%s %s: missing ziphash: %v", mod.Path, mod.Version, err));
        return errs;

    }
    var h = string(bytes.TrimSpace(data));

    if (zipErr != null && errors.Is(zipErr, fs.ErrNotExist)) { 
        // ok
    }
    else
 {
        var (hZ, err) = dirhash.HashZip(zip, dirhash.DefaultHash);
        if (err != null) {
            errs = append(errs, fmt.Errorf("%s %s: %v", mod.Path, mod.Version, err));
            return errs;
        }
        else if (hZ != h) {
            errs = append(errs, fmt.Errorf("%s %s: zip has been modified (%v)", mod.Path, mod.Version, zip));
        }
    }
    if (dirErr != null && errors.Is(dirErr, fs.ErrNotExist)) { 
        // ok
    }
    else
 {
        var (hD, err) = dirhash.HashDir(dir, mod.Path + "@" + mod.Version, dirhash.DefaultHash);
        if (err != null) {
            errs = append(errs, fmt.Errorf("%s %s: %v", mod.Path, mod.Version, err));
            return errs;
        }
        if (hD != h) {
            errs = append(errs, fmt.Errorf("%s %s: dir has been modified (%v)", mod.Path, mod.Version, dir));
        }
    }
    return errs;

}

} // end modcmd_package
