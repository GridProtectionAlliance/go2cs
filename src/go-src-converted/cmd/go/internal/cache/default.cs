// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cache -- go2cs converted at 2022 March 13 06:30:34 UTC
// import "cmd/go/internal/cache" ==> using cache = go.cmd.go.@internal.cache_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\cache\default.go
namespace go.cmd.go.@internal;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using sync = sync_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;


// Default returns the default cache to use, or nil if no cache should be used.

using System;
public static partial class cache_package {

public static ptr<Cache> Default() {
    defaultOnce.Do(initDefaultCache);
    return _addr_defaultCache!;
}

private static sync.Once defaultOnce = default;private static ptr<Cache> defaultCache;

// cacheREADME is a message stored in a README in the cache directory.
// Because the cache lives outside the normal Go trees, we leave the
// README as a courtesy to explain where it came from.
private static readonly @string cacheREADME = "This directory holds cached build artifacts from the Go build system.\nRun \"go cle" +
    "an -cache\" if the directory is getting too large.\nSee golang.org to learn more a" +
    "bout Go.\n";

// initDefaultCache does the work of finding the default cache
// the first time Default is called.


// initDefaultCache does the work of finding the default cache
// the first time Default is called.
private static void initDefaultCache() {
    var dir = DefaultDir();
    if (dir == "off") {
        if (defaultDirErr != null) {
            @base.Fatalf("build cache is required, but could not be located: %v", defaultDirErr);
        }
        @base.Fatalf("build cache is disabled by GOCACHE=off, but required as of Go 1.12");
    }
    {
        var err = os.MkdirAll(dir, 0777);

        if (err != null) {
            @base.Fatalf("failed to initialize build cache at %s: %s\n", dir, err);
        }
    }
    {
        var (_, err) = os.Stat(filepath.Join(dir, "README"));

        if (err != null) { 
            // Best effort.
            os.WriteFile(filepath.Join(dir, "README"), (slice<byte>)cacheREADME, 0666);
        }
    }

    var (c, err) = Open(dir);
    if (err != null) {
        @base.Fatalf("failed to initialize build cache at %s: %s\n", dir, err);
    }
    defaultCache = c;
}

private static sync.Once defaultDirOnce = default;private static @string defaultDir = default;private static error defaultDirErr = default!;

// DefaultDir returns the effective GOCACHE setting.
// It returns "off" if the cache is disabled.
public static @string DefaultDir() { 
    // Save the result of the first call to DefaultDir for later use in
    // initDefaultCache. cmd/go/main.go explicitly sets GOCACHE so that
    // subprocesses will inherit it, but that means initDefaultCache can't
    // otherwise distinguish between an explicit "off" and a UserCacheDir error.

    defaultDirOnce.Do(() => {
        defaultDir = cfg.Getenv("GOCACHE");
        if (filepath.IsAbs(defaultDir) || defaultDir == "off") {
            return ;
        }
        if (defaultDir != "") {
            defaultDir = "off";
            defaultDirErr = fmt.Errorf("GOCACHE is not an absolute path");
            return ;
        }
        var (dir, err) = os.UserCacheDir();
        if (err != null) {
            defaultDir = "off";
            defaultDirErr = fmt.Errorf("GOCACHE is not defined and %v", err);
            return ;
        }
        defaultDir = filepath.Join(dir, "go-build");
    });

    return defaultDir;
}

} // end cache_package
