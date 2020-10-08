// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gocommand -- go2cs converted at 2020 October 08 04:55:06 UTC
// import "golang.org/x/tools/internal/gocommand" ==> using gocommand = go.golang.org.x.tools.@internal.gocommand_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\gocommand\vendor.go
using bytes = go.bytes_package;
using context = go.context_package;
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using strings = go.strings_package;

using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal
{
    public static partial class gocommand_package
    {
        // ModuleJSON holds information about a module.
        public partial struct ModuleJSON
        {
            public @string Path; // module path
            public ptr<ModuleJSON> Replace; // replaced by this module
            public bool Main; // is this the main module?
            public bool Indirect; // is this module only an indirect dependency of main module?
            public @string Dir; // directory holding files for this module, if any
            public @string GoMod; // path to go.mod file for this module, if any
            public @string GoVersion; // go version used in module
        }

        private static var modFlagRegexp = regexp.MustCompile("-mod[ =](\\w+)");

        // VendorEnabled reports whether vendoring is enabled. It takes a *Runner to execute Go commands
        // with the supplied context.Context and Invocation. The Invocation can contain pre-defined fields,
        // of which only Verb and Args are modified to run the appropriate Go command.
        // Inspired by setDefaultBuildMod in modload/init.go
        public static (ptr<ModuleJSON>, bool, error) VendorEnabled(context.Context ctx, Invocation inv, ptr<Runner> _addr_r)
        {
            ptr<ModuleJSON> _p0 = default!;
            bool _p0 = default;
            error _p0 = default!;
            ref Runner r = ref _addr_r.val;

            var (mainMod, go114, err) = getMainModuleAnd114(ctx, inv, _addr_r);
            if (err != null)
            {
                return (_addr_null!, false, error.As(err)!);
            } 

            // We check the GOFLAGS to see if there is anything overridden or not.
            inv.Verb = "env";
            inv.Args = new slice<@string>(new @string[] { "GOFLAGS" });
            var (stdout, err) = r.Run(ctx, inv);
            if (err != null)
            {
                return (_addr_null!, false, error.As(err)!);
            }

            var goflags = string(bytes.TrimSpace(stdout.Bytes()));
            var matches = modFlagRegexp.FindStringSubmatch(goflags);
            @string modFlag = default;
            if (len(matches) != 0L)
            {
                modFlag = matches[1L];
            }

            if (modFlag != "")
            { 
                // Don't override an explicit '-mod=' argument.
                return (_addr_mainMod!, modFlag == "vendor", error.As(null!)!);

            }

            if (mainMod == null || !go114)
            {
                return (_addr_mainMod!, false, error.As(null!)!);
            } 
            // Check 1.14's automatic vendor mode.
            {
                var (fi, err) = os.Stat(filepath.Join(mainMod.Dir, "vendor"));

                if (err == null && fi.IsDir())
                {
                    if (mainMod.GoVersion != "" && semver.Compare("v" + mainMod.GoVersion, "v1.14") >= 0L)
                    { 
                        // The Go version is at least 1.14, and a vendor directory exists.
                        // Set -mod=vendor by default.
                        return (_addr_mainMod!, true, error.As(null!)!);

                    }

                }

            }

            return (_addr_mainMod!, false, error.As(null!)!);

        }

        // getMainModuleAnd114 gets the main module's information and whether the
        // go command in use is 1.14+. This is the information needed to figure out
        // if vendoring should be enabled.
        private static (ptr<ModuleJSON>, bool, error) getMainModuleAnd114(context.Context ctx, Invocation inv, ptr<Runner> _addr_r)
        {
            ptr<ModuleJSON> _p0 = default!;
            bool _p0 = default;
            error _p0 = default!;
            ref Runner r = ref _addr_r.val;

            const @string format = (@string)"{{.Path}}\n{{.Dir}}\n{{.GoMod}}\n{{.GoVersion}}\n{{range context.ReleaseTags}}{{if eq" +
    " . \"go1.14\"}}{{.}}{{end}}{{end}}\n";

            inv.Verb = "list";
            inv.Args = new slice<@string>(new @string[] { "-m", "-f", format });
            var (stdout, err) = r.Run(ctx, inv);
            if (err != null)
            {
                return (_addr_null!, false, error.As(err)!);
            }

            var lines = strings.Split(stdout.String(), "\n");
            if (len(lines) < 5L)
            {
                return (_addr_null!, false, error.As(fmt.Errorf("unexpected stdout: %q", stdout.String()))!);
            }

            ptr<ModuleJSON> mod = addr(new ModuleJSON(Path:lines[0],Dir:lines[1],GoMod:lines[2],GoVersion:lines[3],Main:true,));
            return (_addr_mod!, lines[4L] == "go1.14", error.As(null!)!);

        }
    }
}}}}}
