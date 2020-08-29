// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cache -- go2cs converted at 2020 August 29 10:01:05 UTC
// import "cmd/go/internal/cache" ==> using cache = go.cmd.go.@internal.cache_package
// Original source: C:\Go\src\cmd\go\internal\cache\default.go
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class cache_package
    {
        // Default returns the default cache to use, or nil if no cache should be used.
        public static ref Cache Default()
        {
            defaultOnce.Do(initDefaultCache);
            return defaultCache;
        }

        private static sync.Once defaultOnce = default;        private static ref Cache defaultCache = default;

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
        private static void initDefaultCache()
        {
            var dir = DefaultDir();
            if (dir == "off")
            {
                return;
            }
            {
                var err = os.MkdirAll(dir, 0777L);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "go: disabling cache (%s) due to initialization failure: %s\n", dir, err);
                    return;
                }

            }
            {
                var (_, err) = os.Stat(filepath.Join(dir, "README"));

                if (err != null)
                { 
                    // Best effort.
                    ioutil.WriteFile(filepath.Join(dir, "README"), (slice<byte>)cacheREADME, 0666L);
                }

            }

            var (c, err) = Open(dir);
            if (err != null)
            {
                fmt.Fprintf(os.Stderr, "go: disabling cache (%s) due to initialization failure: %s\n", dir, err);
                return;
            }
            defaultCache = c;
        }

        // DefaultDir returns the effective GOCACHE setting.
        // It returns "off" if the cache is disabled.
        public static @string DefaultDir()
        {
            var dir = os.Getenv("GOCACHE");
            if (dir != "")
            {
                return dir;
            } 

            // Compute default location.
            // TODO(rsc): This code belongs somewhere else,
            // like maybe ioutil.CacheDir or os.CacheDir.
            switch (runtime.GOOS)
            {
                case "windows": 
                    dir = os.Getenv("LocalAppData");
                    if (dir == "")
                    { 
                        // Fall back to %AppData%, the old name of
                        // %LocalAppData% on Windows XP.
                        dir = os.Getenv("AppData");
                    }
                    if (dir == "")
                    {
                        return "off";
                    }
                    break;
                case "darwin": 
                    dir = os.Getenv("HOME");
                    if (dir == "")
                    {
                        return "off";
                    }
                    dir += "/Library/Caches";
                    break;
                case "plan9": 
                    dir = os.Getenv("home");
                    if (dir == "")
                    {
                        return "off";
                    } 
                    // Plan 9 has no established per-user cache directory,
                    // but $home/lib/xyz is the usual equivalent of $HOME/.xyz on Unix.
                    dir += "/lib/cache";
                    break;
                default: // Unix
                    // https://standards.freedesktop.org/basedir-spec/basedir-spec-latest.html
                    dir = os.Getenv("XDG_CACHE_HOME");
                    if (dir == "")
                    {
                        dir = os.Getenv("HOME");
                        if (dir == "")
                        {
                            return "off";
                        }
                        dir += "/.cache";
                    }
                    break;
            }
            return filepath.Join(dir, "go-build");
        }
    }
}}}}
