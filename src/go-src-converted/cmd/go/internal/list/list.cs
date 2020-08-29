// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package list implements the ``go list'' command.
// package list -- go2cs converted at 2020 August 29 10:01:50 UTC
// import "cmd/go/internal/list" ==> using list = go.cmd.go.@internal.list_package
// Original source: C:\Go\src\cmd\go\internal\list\list.go
using bufio = go.bufio_package;
using json = go.encoding.json_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using template = go.text.template_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using work = go.cmd.go.@internal.work_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class list_package
    {
        public static base.Command CmdList = ref new base.Command(UsageLine:"list [-e] [-f format] [-json] [build flags] [packages]",Short:"list packages",Long:`
List lists the packages named by the import paths, one per line.

The default output shows the package import path:

    bytes
    encoding/json
    github.com/gorilla/mux
    golang.org/x/net/html

The -f flag specifies an alternate format for the list, using the
syntax of package template. The default output is equivalent to -f
'{{.ImportPath}}'. The struct being passed to the template is:

    type Package struct {
        Dir           string // directory containing package sources
        ImportPath    string // import path of package in dir
        ImportComment string // path in import comment on package statement
        Name          string // package name
        Doc           string // package documentation string
        Target        string // install path
        Shlib         string // the shared library that contains this package (only set when -linkshared)
        Goroot        bool   // is this package in the Go root?
        Standard      bool   // is this package part of the standard Go library?
        Stale         bool   // would 'go install' do anything for this package?
        StaleReason   string // explanation for Stale==true
        Root          string // Go root or Go path dir containing this package
        ConflictDir   string // this directory shadows Dir in $GOPATH
        BinaryOnly    bool   // binary-only package: cannot be recompiled from sources

        // Source files
        GoFiles        []string // .go source files (excluding CgoFiles, TestGoFiles, XTestGoFiles)
        CgoFiles       []string // .go sources files that import "C"
        IgnoredGoFiles []string // .go sources ignored due to build constraints
        CFiles         []string // .c source files
        CXXFiles       []string // .cc, .cxx and .cpp source files
        MFiles         []string // .m source files
        HFiles         []string // .h, .hh, .hpp and .hxx source files
        FFiles         []string // .f, .F, .for and .f90 Fortran source files
        SFiles         []string // .s source files
        SwigFiles      []string // .swig files
        SwigCXXFiles   []string // .swigcxx files
        SysoFiles      []string // .syso object files to add to archive
        TestGoFiles    []string // _test.go files in package
        XTestGoFiles   []string // _test.go files outside package

        // Cgo directives
        CgoCFLAGS    []string // cgo: flags for C compiler
        CgoCPPFLAGS  []string // cgo: flags for C preprocessor
        CgoCXXFLAGS  []string // cgo: flags for C++ compiler
        CgoFFLAGS    []string // cgo: flags for Fortran compiler
        CgoLDFLAGS   []string // cgo: flags for linker
        CgoPkgConfig []string // cgo: pkg-config names

        // Dependency information
        Imports      []string // import paths used by this package
        Deps         []string // all (recursively) imported dependencies
        TestImports  []string // imports from TestGoFiles
        XTestImports []string // imports from XTestGoFiles

        // Error information
        Incomplete bool            // this package or a dependency has an error
        Error      *PackageError   // error loading package
        DepsErrors []*PackageError // errors loading dependencies
    }

Packages stored in vendor directories report an ImportPath that includes the
path to the vendor directory (for example, "d/vendor/p" instead of "p"),
so that the ImportPath uniquely identifies a given copy of a package.
The Imports, Deps, TestImports, and XTestImports lists also contain these
expanded imports paths. See golang.org/s/go15vendor for more about vendoring.

The error information, if any, is

    type PackageError struct {
        ImportStack   []string // shortest path from package named on command line to this one
        Pos           string   // position of error (if present, file:line:col)
        Err           string   // the error itself
    }

The template function "join" calls strings.Join.

The template function "context" returns the build context, defined as:

	type Context struct {
		GOARCH        string   // target architecture
		GOOS          string   // target operating system
		GOROOT        string   // Go root
		GOPATH        string   // Go path
		CgoEnabled    bool     // whether cgo can be used
		UseAllFiles   bool     // use files regardless of +build lines, file names
		Compiler      string   // compiler to assume when computing target paths
		BuildTags     []string // build constraints to match in +build lines
		ReleaseTags   []string // releases the current release is compatible with
		InstallSuffix string   // suffix to use in the name of the install dir
	}

For more information about the meaning of these fields see the documentation
for the go/build package's Context type.

The -json flag causes the package data to be printed in JSON format
instead of using the template format.

The -e flag changes the handling of erroneous packages, those that
cannot be found or are malformed. By default, the list command
prints an error to standard error for each erroneous package and
omits the packages from consideration during the usual printing.
With the -e flag, the list command never prints errors to standard
error and instead processes the erroneous packages with the usual
printing. Erroneous packages will have a non-empty ImportPath and
a non-nil Error field; other information may or may not be missing
(zeroed).

For more about build flags, see 'go help build'.

For more about specifying packages, see 'go help packages'.
	`,);

        private static void init()
        {
            CmdList.Run = runList; // break init cycle
            work.AddBuildFlags(CmdList);
        }

        private static var listE = CmdList.Flag.Bool("e", false, "");
        private static var listFmt = CmdList.Flag.String("f", "{{.ImportPath}}", "");
        private static var listJson = CmdList.Flag.Bool("json", false, "");
        private static byte nl = new slice<byte>(new byte[] { '\n' });

        private static void runList(ref base.Command _cmd, slice<@string> args) => func(_cmd, (ref base.Command cmd, Defer defer, Panic _, Recover __) =>
        {
            work.BuildInit();
            var @out = newTrackingWriter(os.Stdout);
            defer(@out.w.Flush());

            Action<ref load.PackagePublic> @do = default;
            if (listJson.Value)
            {
                do = p =>
                {
                    var (b, err) = json.MarshalIndent(p, "", "\t");
                    if (err != null)
                    {
                        @out.Flush();
                        @base.Fatalf("%s", err);
                    }
                    @out.Write(b);
                    @out.Write(nl);
                }
            else
;
            }            {
                ref Context cachedCtxt = default;
                Func<ref Context> context = () =>
                {
                    if (cachedCtxt == null)
                    {
                        cachedCtxt = newContext(ref cfg.BuildContext);
                    }
                    return cachedCtxt;
                }
;
                template.FuncMap fm = new template.FuncMap("join":strings.Join,"context":context,);
                var (tmpl, err) = template.New("main").Funcs(fm).Parse(listFmt.Value);
                if (err != null)
                {
                    @base.Fatalf("%s", err);
                }
                do = p =>
                {
                    {
                        var err = tmpl.Execute(out, p);

                        if (err != null)
                        {
                            @out.Flush();
                            @base.Fatalf("%s", err);
                        }

                    }
                    if (@out.NeedNL())
                    {
                        @out.Write(nl);
                    }
                }
;
            }
            slice<ref load.Package> pkgs = default;
            if (listE.Value)
            {
                pkgs = load.PackagesAndErrors(args);
            }
            else
            {
                pkgs = load.Packages(args);
            } 

            // Estimate whether staleness information is needed,
            // since it's a little bit of work to compute.
            var needStale = listJson || strings.Contains(listFmt.Value, ".Stale").Value;
            if (needStale)
            {
                work.Builder b = default;
                b.Init();
                b.ComputeStaleOnly = true;
                work.Action a = ref new work.Action(); 
                // TODO: Use pkgsFilter?
                foreach (var (_, p) in pkgs)
                {
                    a.Deps = append(a.Deps, b.AutoAction(work.ModeInstall, work.ModeInstall, p));
                }
                b.Do(a);
            }
            foreach (var (_, pkg) in pkgs)
            { 
                // Show vendor-expanded paths in listing
                pkg.TestImports = pkg.Vendored(pkg.TestImports);
                pkg.XTestImports = pkg.Vendored(pkg.XTestImports);

                do(ref pkg.PackagePublic);
            }
        });

        // TrackingWriter tracks the last byte written on every write so
        // we can avoid printing a newline if one was already written or
        // if there is no output at all.
        public partial struct TrackingWriter
        {
            public ptr<bufio.Writer> w;
            public byte last;
        }

        private static ref TrackingWriter newTrackingWriter(io.Writer w)
        {
            return ref new TrackingWriter(w:bufio.NewWriter(w),last:'\n',);
        }

        private static (long, error) Write(this ref TrackingWriter t, slice<byte> p)
        {
            n, err = t.w.Write(p);
            if (n > 0L)
            {
                t.last = p[n - 1L];
            }
            return;
        }

        private static void Flush(this ref TrackingWriter t)
        {
            t.w.Flush();
        }

        private static bool NeedNL(this ref TrackingWriter t)
        {
            return t.last != '\n';
        }
    }
}}}}
