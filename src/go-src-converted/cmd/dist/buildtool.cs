// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Build toolchain using Go 1.4.
//
// The general strategy is to copy the source files we need into
// a new GOPATH workspace, adjust import paths appropriately,
// invoke the Go 1.4 go command to build those sources,
// and then copy the binaries back.

// package main -- go2cs converted at 2020 August 29 09:59:42 UTC
// Original source: C:\Go\src\cmd\dist\buildtool.go
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // bootstrapDirs is a list of directories holding code that must be
        // compiled with a Go 1.4 toolchain to produce the bootstrapTargets.
        // All directories in this list are relative to and must be below $GOROOT/src.
        //
        // The list has have two kinds of entries: names beginning with cmd/ with
        // no other slashes, which are commands, and other paths, which are packages
        // supporting the commands. Packages in the standard library can be listed
        // if a newer copy needs to be substituted for the Go 1.4 copy when used
        // by the command packages.
        // These will be imported during bootstrap as bootstrap/name, like bootstrap/math/big.
        private static @string bootstrapDirs = new slice<@string>(new @string[] { "cmd/asm", "cmd/asm/internal/arch", "cmd/asm/internal/asm", "cmd/asm/internal/flags", "cmd/asm/internal/lex", "cmd/cgo", "cmd/compile", "cmd/compile/internal/amd64", "cmd/compile/internal/arm", "cmd/compile/internal/arm64", "cmd/compile/internal/gc", "cmd/compile/internal/mips", "cmd/compile/internal/mips64", "cmd/compile/internal/ppc64", "cmd/compile/internal/types", "cmd/compile/internal/s390x", "cmd/compile/internal/ssa", "cmd/compile/internal/syntax", "cmd/compile/internal/x86", "cmd/internal/bio", "cmd/internal/gcprog", "cmd/internal/dwarf", "cmd/internal/edit", "cmd/internal/objabi", "cmd/internal/obj", "cmd/internal/obj/arm", "cmd/internal/obj/arm64", "cmd/internal/obj/mips", "cmd/internal/obj/ppc64", "cmd/internal/obj/s390x", "cmd/internal/obj/x86", "cmd/internal/src", "cmd/internal/sys", "cmd/link", "cmd/link/internal/amd64", "cmd/link/internal/arm", "cmd/link/internal/arm64", "cmd/link/internal/ld", "cmd/link/internal/loadelf", "cmd/link/internal/loadmacho", "cmd/link/internal/loadpe", "cmd/link/internal/mips", "cmd/link/internal/mips64", "cmd/link/internal/objfile", "cmd/link/internal/ppc64", "cmd/link/internal/s390x", "cmd/link/internal/sym", "cmd/link/internal/x86", "container/heap", "debug/dwarf", "debug/elf", "debug/macho", "debug/pe", "math/big", "math/bits", "sort" });

        // File prefixes that are ignored by go/build anyway, and cause
        // problems with editor generated temporary files (#18931).
        private static @string ignorePrefixes = new slice<@string>(new @string[] { ".", "_" });

        // File suffixes that use build tags introduced since Go 1.4.
        // These must not be copied into the bootstrap build directory.
        private static @string ignoreSuffixes = new slice<@string>(new @string[] { "_arm64.s", "_arm64.go" });

        private static void bootstrapBuildTools() => func((defer, _, __) =>
        {
            var goroot_bootstrap = os.Getenv("GOROOT_BOOTSTRAP");
            if (goroot_bootstrap == "")
            {
                goroot_bootstrap = pathf("%s/go1.4", os.Getenv("HOME"));
            }
            xprintf("Building Go toolchain1 using %s.\n", goroot_bootstrap);

            mkzbootstrap(pathf("%s/src/cmd/internal/objabi/zbootstrap.go", goroot)); 

            // Use $GOROOT/pkg/bootstrap as the bootstrap workspace root.
            // We use a subdirectory of $GOROOT/pkg because that's the
            // space within $GOROOT where we store all generated objects.
            // We could use a temporary directory outside $GOROOT instead,
            // but it is easier to debug on failure if the files are in a known location.
            var workspace = pathf("%s/pkg/bootstrap", goroot);
            xremoveall(workspace);
            var @base = pathf("%s/src/bootstrap", workspace);
            xmkdirall(base); 

            // Copy source code into $GOROOT/pkg/bootstrap and rewrite import paths.
            foreach (var (_, dir) in bootstrapDirs)
            {
                var src = pathf("%s/src/%s", goroot, dir);
                var dst = pathf("%s/%s", base, dir);
                xmkdirall(dst);
                if (dir == "cmd/cgo")
                { 
                    // Write to src because we need the file both for bootstrap
                    // and for later in the main build.
                    mkzdefaultcc("", pathf("%s/zdefaultcc.go", src));
                }
Dir:
                {
                    var name__prev2 = name;

                    foreach (var (_, __name) in xreaddirfiles(src))
                    {
                        name = __name;
                        foreach (var (_, pre) in ignorePrefixes)
                        {
                            if (strings.HasPrefix(name, pre))
                            {
                                _continueDir = true;
                                break;
                            }
                        }
                        foreach (var (_, suf) in ignoreSuffixes)
                        {
                            if (strings.HasSuffix(name, suf))
                            {
                                _continueDir = true;
                                break;
                            }
                        }
                        var srcFile = pathf("%s/%s", src, name);
                        var dstFile = pathf("%s/%s", dst, name);
                        var text = bootstrapRewriteFile(srcFile);
                        writefile(text, dstFile, 0L);
                    }

                    name = name__prev2;
                }
            } 

            // Set up environment for invoking Go 1.4 go command.
            // GOROOT points at Go 1.4 GOROOT,
            // GOPATH points at our bootstrap workspace,
            // GOBIN is empty, so that binaries are installed to GOPATH/bin,
            // and GOOS, GOHOSTOS, GOARCH, and GOHOSTOS are empty,
            // so that Go 1.4 builds whatever kind of binary it knows how to build.
            // Restore GOROOT, GOPATH, and GOBIN when done.
            // Don't bother with GOOS, GOHOSTOS, GOARCH, and GOHOSTARCH,
            // because setup will take care of those when bootstrapBuildTools returns.
            defer(os.Setenv("GOROOT", os.Getenv("GOROOT")));
            os.Setenv("GOROOT", goroot_bootstrap);

            defer(os.Setenv("GOPATH", os.Getenv("GOPATH")));
            os.Setenv("GOPATH", workspace);

            defer(os.Setenv("GOBIN", os.Getenv("GOBIN")));
            os.Setenv("GOBIN", "");

            os.Setenv("GOOS", "");
            os.Setenv("GOHOSTOS", "");
            os.Setenv("GOARCH", "");
            os.Setenv("GOHOSTARCH", ""); 

            // Run Go 1.4 to build binaries. Use -gcflags=-l to disable inlining to
            // workaround bugs in Go 1.4's compiler. See discussion thread:
            // https://groups.google.com/d/msg/golang-dev/Ss7mCKsvk8w/Gsq7VYI0AwAJ
            // Use the math_big_pure_go build tag to disable the assembly in math/big
            // which may contain unsupported instructions.
            // Note that if we are using Go 1.10 or later as bootstrap, the -gcflags=-l
            // only applies to the final cmd/go binary, but that's OK: if this is Go 1.10
            // or later we don't need to disable inlining to work around bugs in the Go 1.4 compiler.
            @string cmd = new slice<@string>(new @string[] { pathf("%s/bin/go",goroot_bootstrap), "install", "-gcflags=-l", "-tags=math_big_pure_go compiler_bootstrap" });
            if (vflag > 0L)
            {
                cmd = append(cmd, "-v");
            }
            {
                var tool = os.Getenv("GOBOOTSTRAP_TOOLEXEC");

                if (tool != "")
                {
                    cmd = append(cmd, "-toolexec=" + tool);
                }

            }
            cmd = append(cmd, "bootstrap/cmd/...");
            run(workspace, ShowOutput | CheckExit, cmd); 

            // Copy binaries into tool binary directory.
            {
                var name__prev1 = name;

                foreach (var (_, __name) in bootstrapDirs)
                {
                    name = __name;
                    if (!strings.HasPrefix(name, "cmd/"))
                    {
                        continue;
                    }
                    name = name[len("cmd/")..];
                    if (!strings.Contains(name, "/"))
                    {
                        copyfile(pathf("%s/%s%s", tooldir, name, exe), pathf("%s/bin/%s%s", workspace, name, exe), writeExec);
                    }
                }

                name = name__prev1;
            }

            if (vflag > 0L)
            {
                xprintf("\n");
            }
        });

        private static var ssaRewriteFileSubstring = filepath.FromSlash("src/cmd/compile/internal/ssa/rewrite");

        // isUnneededSSARewriteFile reports whether srcFile is a
        // src/cmd/compile/internal/ssa/rewriteARCHNAME.go file for an
        // architecture that isn't for the current runtime.GOARCH.
        //
        // When unneeded is true archCaps is the rewrite base filename without
        // the "rewrite" prefix or ".go" suffix: AMD64, 386, ARM, ARM64, etc.
        private static (@string, bool) isUnneededSSARewriteFile(@string srcFile)
        {
            if (!strings.Contains(srcFile, ssaRewriteFileSubstring))
            {
                return ("", false);
            }
            var fileArch = strings.TrimSuffix(strings.TrimPrefix(filepath.Base(srcFile), "rewrite"), ".go");
            if (fileArch == "")
            {
                return ("", false);
            }
            var b = fileArch[0L];
            if (b == '_' || ('a' <= b && b <= 'z'))
            {
                return ("", false);
            }
            archCaps = fileArch;
            fileArch = strings.ToLower(fileArch);
            if (fileArch == strings.TrimSuffix(runtime.GOARCH, "le"))
            {
                return ("", false);
            }
            if (fileArch == strings.TrimSuffix(os.Getenv("GOARCH"), "le"))
            {
                return ("", false);
            }
            return (archCaps, true);
        }

        private static @string bootstrapRewriteFile(@string srcFile)
        { 
            // During bootstrap, generate dummy rewrite files for
            // irrelevant architectures. We only need to build a bootstrap
            // binary that works for the current runtime.GOARCH.
            // This saves 6+ seconds of bootstrap.
            {
                var (archCaps, ok) = isUnneededSSARewriteFile(srcFile);

                if (ok)
                {
                    return fmt.Sprintf("// Code generated by go tool dist; DO NOT EDIT.\n\npackage ssa\n\nfunc rewriteValue%s" +
    "(v *Value) bool { panic(\"unused during bootstrap\") }\nfunc rewriteBlock%s(b *Bloc" +
    "k) bool { panic(\"unused during bootstrap\") }\n", archCaps, archCaps);
                }

            }

            return bootstrapFixImports(srcFile);
        }

        private static @string bootstrapFixImports(@string srcFile)
        {
            var lines = strings.SplitAfter(readfile(srcFile), "\n");
            var inBlock = false;
            foreach (var (i, line) in lines)
            {
                if (strings.HasPrefix(line, "import ("))
                {
                    inBlock = true;
                    continue;
                }
                if (inBlock && strings.HasPrefix(line, ")"))
                {
                    inBlock = false;
                    continue;
                }
                if (strings.HasPrefix(line, "import \"") || strings.HasPrefix(line, "import . \"") || inBlock && (strings.HasPrefix(line, "\t\"") || strings.HasPrefix(line, "\t. \"")))
                {
                    line = strings.Replace(line, "\"cmd/", "\"bootstrap/cmd/", -1L);
                    foreach (var (_, dir) in bootstrapDirs)
                    {
                        if (strings.HasPrefix(dir, "cmd/"))
                        {
                            continue;
                        }
                        line = strings.Replace(line, "\"" + dir + "\"", "\"bootstrap/" + dir + "\"", -1L);
                    }
                    lines[i] = line;
                }
            }
            lines[0L] = "// Code generated by go tool dist; DO NOT EDIT.\n// This is a bootstrap copy of " + srcFile + "\n\n//line " + srcFile + ":1\n" + lines[0L];

            return strings.Join(lines, "");
        }
    }
}
