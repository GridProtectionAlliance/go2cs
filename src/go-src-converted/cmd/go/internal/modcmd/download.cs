// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modcmd -- go2cs converted at 2020 October 08 04:33:41 UTC
// import "cmd/go/internal/modcmd" ==> using modcmd = go.cmd.go.@internal.modcmd_package
// Original source: C:\Go\src\cmd\go\internal\modcmd\download.go
using json = go.encoding.json_package;
using os = go.os_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using modload = go.cmd.go.@internal.modload_package;
using par = go.cmd.go.@internal.par_package;
using work = go.cmd.go.@internal.work_package;

using module = go.golang.org.x.mod.module_package;
using static go.builtin;
using System.ComponentModel;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modcmd_package
    {
        private static ptr<base.Command> cmdDownload = addr(new base.Command(UsageLine:"go mod download [-x] [-json] [modules]",Short:"download modules to local cache",Long:`
Download downloads the named modules, which can be module patterns selecting
dependencies of the main module or module queries of the form path@version.
With no arguments, download applies to all dependencies of the main module
(equivalent to 'go mod download all').

The go command will automatically download modules as needed during ordinary
execution. The "go mod download" command is useful mainly for pre-filling
the local cache or to compute the answers for a Go module proxy.

By default, download writes nothing to standard output. It may print progress
messages and errors to standard error.

The -json flag causes download to print a sequence of JSON objects
to standard output, describing each downloaded module (or failure),
corresponding to this Go struct:

    type Module struct {
        Path     string // module path
        Version  string // module version
        Error    string // error loading module
        Info     string // absolute path to cached .info file
        GoMod    string // absolute path to cached .mod file
        Zip      string // absolute path to cached .zip file
        Dir      string // absolute path to cached source root directory
        Sum      string // checksum for path, version (as in go.sum)
        GoModSum string // checksum for go.mod (as in go.sum)
    }

The -x flag causes download to print the commands download executes.

See 'go help modules' for more about module queries.
	`,));

        private static var downloadJSON = cmdDownload.Flag.Bool("json", false, "");

        private static void init()
        {
            cmdDownload.Run = runDownload; // break init cycle

            // TODO(jayconrod): https://golang.org/issue/35849 Apply -x to other 'go mod' commands.
            cmdDownload.Flag.BoolVar(_addr_cfg.BuildX, "x", false, "");
            work.AddModCommonFlags(cmdDownload);

        }

        private partial struct moduleJSON
        {
            [Description("json:\",omitempty\"")]
            public @string Path;
            [Description("json:\",omitempty\"")]
            public @string Version;
            [Description("json:\",omitempty\"")]
            public @string Error;
            [Description("json:\",omitempty\"")]
            public @string Info;
            [Description("json:\",omitempty\"")]
            public @string GoMod;
            [Description("json:\",omitempty\"")]
            public @string Zip;
            [Description("json:\",omitempty\"")]
            public @string Dir;
            [Description("json:\",omitempty\"")]
            public @string Sum;
            [Description("json:\",omitempty\"")]
            public @string GoModSum;
        }

        private static void runDownload(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;
 
            // Check whether modules are enabled and whether we're in a module.
            if (cfg.Getenv("GO111MODULE") == "off")
            {
                @base.Fatalf("go: modules disabled by GO111MODULE=off; see 'go help modules'");
            }

            if (!modload.HasModRoot() && len(args) == 0L)
            {
                @base.Fatalf("go mod download: no modules specified (see 'go help mod download')");
            }

            if (len(args) == 0L)
            {
                args = new slice<@string>(new @string[] { "all" });
            }
            else if (modload.HasModRoot())
            {
                modload.InitMod(); // to fill Target
                var targetAtLatest = modload.Target.Path + "@latest";
                var targetAtUpgrade = modload.Target.Path + "@upgrade";
                var targetAtPatch = modload.Target.Path + "@patch";
                foreach (var (_, arg) in args)
                {

                    if (arg == modload.Target.Path || arg == targetAtLatest || arg == targetAtUpgrade || arg == targetAtPatch) 
                        os.Stderr.WriteString("go mod download: skipping argument " + arg + " that resolves to the main module\n");
                    
                }

            }

            slice<ptr<moduleJSON>> mods = default;
            par.Work work = default;
            var listU = false;
            var listVersions = false;
            foreach (var (_, info) in modload.ListModules(args, listU, listVersions))
            {
                if (info.Replace != null)
                {
                    info = info.Replace;
                }

                if (info.Version == "" && info.Error == null)
                { 
                    // main module or module replaced with file path.
                    // Nothing to download.
                    continue;

                }

                ptr<moduleJSON> m = addr(new moduleJSON(Path:info.Path,Version:info.Version,));
                mods = append(mods, m);
                if (info.Error != null)
                {
                    m.Error = info.Error.Err;
                    continue;
                }

                work.Add(m);

            }
            work.Do(10L, item =>
            {
                m = item._<ptr<moduleJSON>>();
                error err = default!;
                m.Info, err = modfetch.InfoFile(m.Path, m.Version);
                if (err != null)
                {
                    m.Error = err.Error();
                    return ;
                }

                m.GoMod, err = modfetch.GoModFile(m.Path, m.Version);
                if (err != null)
                {
                    m.Error = err.Error();
                    return ;
                }

                m.GoModSum, err = modfetch.GoModSum(m.Path, m.Version);
                if (err != null)
                {
                    m.Error = err.Error();
                    return ;
                }

                module.Version mod = new module.Version(Path:m.Path,Version:m.Version);
                m.Zip, err = modfetch.DownloadZip(mod);
                if (err != null)
                {
                    m.Error = err.Error();
                    return ;
                }

                m.Sum = modfetch.Sum(mod);
                m.Dir, err = modfetch.Download(mod);
                if (err != null)
                {
                    m.Error = err.Error();
                    return ;
                }

            });

            if (downloadJSON.val)
            {
                {
                    moduleJSON m__prev1 = m;

                    foreach (var (_, __m) in mods)
                    {
                        m = __m;
                        var (b, err) = json.MarshalIndent(m, "", "\t");
                        if (err != null)
                        {
                            @base.Fatalf("%v", err);
                        }

                        os.Stdout.Write(append(b, '\n'));
                        if (m.Error != "")
                        {
                            @base.SetExitStatus(1L);
                        }

                    }
            else

                    m = m__prev1;
                }
            }            {
                {
                    moduleJSON m__prev1 = m;

                    foreach (var (_, __m) in mods)
                    {
                        m = __m;
                        if (m.Error != "")
                        {
                            @base.Errorf("%s", m.Error);
                        }

                    }

                    m = m__prev1;
                }

                @base.ExitIfErrors();

            }

        }
    }
}}}}
