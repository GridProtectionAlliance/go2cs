// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Build toolchain using Go 1.4.
//
// The general strategy is to copy the source files we need into
// a new GOPATH workspace, adjust import paths appropriately,
// invoke the Go 1.4 go command to build those sources,
// and then copy the binaries back.

// package main -- go2cs converted at 2022 March 06 23:15:20 UTC
// Original source: C:\Program Files\Go\src\cmd\dist\buildtool.go
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using System;


namespace go;

public static partial class main_package {

    // bootstrapDirs is a list of directories holding code that must be
    // compiled with a Go 1.4 toolchain to produce the bootstrapTargets.
    // All directories in this list are relative to and must be below $GOROOT/src.
    //
    // The list has two kinds of entries: names beginning with cmd/ with
    // no other slashes, which are commands, and other paths, which are packages
    // supporting the commands. Packages in the standard library can be listed
    // if a newer copy needs to be substituted for the Go 1.4 copy when used
    // by the command packages. Paths ending with /... automatically
    // include all packages within subdirectories as well.
    // These will be imported during bootstrap as bootstrap/name, like bootstrap/math/big.
private static @string bootstrapDirs = new slice<@string>(new @string[] { "cmd/asm", "cmd/asm/internal/...", "cmd/cgo", "cmd/compile", "cmd/compile/internal/...", "cmd/internal/archive", "cmd/internal/bio", "cmd/internal/codesign", "cmd/internal/dwarf", "cmd/internal/edit", "cmd/internal/gcprog", "cmd/internal/goobj", "cmd/internal/obj/...", "cmd/internal/objabi", "cmd/internal/pkgpath", "cmd/internal/src", "cmd/internal/sys", "cmd/link", "cmd/link/internal/...", "compress/flate", "compress/zlib", "container/heap", "debug/dwarf", "debug/elf", "debug/macho", "debug/pe", "go/constant", "internal/buildcfg", "internal/goexperiment", "internal/goversion", "internal/race", "internal/unsafeheader", "internal/xcoff", "math/big", "math/bits", "sort", "strconv" });

// File prefixes that are ignored by go/build anyway, and cause
// problems with editor generated temporary files (#18931).
private static @string ignorePrefixes = new slice<@string>(new @string[] { ".", "_", "#" });

// File suffixes that use build tags introduced since Go 1.4.
// These must not be copied into the bootstrap build directory.
// Also ignore test files.
private static @string ignoreSuffixes = new slice<@string>(new @string[] { "_arm64.s", "_arm64.go", "_riscv64.s", "_riscv64.go", "_wasm.s", "_wasm.go", "_test.s", "_test.go" });

private static void bootstrapBuildTools() => func((defer, _, _) => {
    var goroot_bootstrap = os.Getenv("GOROOT_BOOTSTRAP");
    if (goroot_bootstrap == "") {
        goroot_bootstrap = pathf("%s/go1.4", os.Getenv("HOME"));
    }
    xprintf("Building Go toolchain1 using %s.\n", goroot_bootstrap);

    mkbuildcfg(pathf("%s/src/internal/buildcfg/zbootstrap.go", goroot));
    mkobjabi(pathf("%s/src/cmd/internal/objabi/zbootstrap.go", goroot)); 

    // Use $GOROOT/pkg/bootstrap as the bootstrap workspace root.
    // We use a subdirectory of $GOROOT/pkg because that's the
    // space within $GOROOT where we store all generated objects.
    // We could use a temporary directory outside $GOROOT instead,
    // but it is easier to debug on failure if the files are in a known location.
    var workspace = pathf("%s/pkg/bootstrap", goroot);
    xremoveall(workspace);
    xatexit(() => {
        xremoveall(workspace);
    });
    var @base = pathf("%s/src/bootstrap", workspace);
    xmkdirall(base); 

    // Copy source code into $GOROOT/pkg/bootstrap and rewrite import paths.
    writefile("module bootstrap\n", pathf("%s/%s", base, "go.mod"), 0);
    foreach (var (_, dir) in bootstrapDirs) {
        var recurse = strings.HasSuffix(dir, "/...");
        dir = strings.TrimSuffix(dir, "/...");
        filepath.Walk(dir, (path, info, err) => {
            if (err != null) {
                fatalf("walking bootstrap dirs failed: %v: %v", path, err);
            }
            var name = filepath.Base(path);
            var src = pathf("%s/src/%s", goroot, path);
            var dst = pathf("%s/%s", base, path);

            if (info.IsDir()) {
                if (!recurse && path != dir || name == "testdata") {
                    return filepath.SkipDir;
                }
                xmkdirall(dst);
                if (path == "cmd/cgo") { 
                    // Write to src because we need the file both for bootstrap
                    // and for later in the main build.
                    mkzdefaultcc("", pathf("%s/zdefaultcc.go", src));
                    mkzdefaultcc("", pathf("%s/zdefaultcc.go", dst));

                }

                return null;

            }

            foreach (var (_, pre) in ignorePrefixes) {
                if (strings.HasPrefix(name, pre)) {
                    return null;
                }
            }
            foreach (var (_, suf) in ignoreSuffixes) {
                if (strings.HasSuffix(name, suf)) {
                    return null;
                }
            }
            var text = bootstrapRewriteFile(src);
            writefile(text, dst, 0);
            return null;

        });

    }    defer(os.Setenv("GOROOT", os.Getenv("GOROOT")));
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
    if (vflag > 0) {
        cmd = append(cmd, "-v");
    }
    {
        var tool = os.Getenv("GOBOOTSTRAP_TOOLEXEC");

        if (tool != "") {
            cmd = append(cmd, "-toolexec=" + tool);
        }
    }

    cmd = append(cmd, "bootstrap/cmd/...");
    run(base, ShowOutput | CheckExit, cmd); 

    // Copy binaries into tool binary directory.
    {
        var name__prev1 = name;

        foreach (var (_, __name) in bootstrapDirs) {
            name = __name;
            if (!strings.HasPrefix(name, "cmd/")) {
                continue;
            }
            name = name[(int)len("cmd/")..];
            if (!strings.Contains(name, "/")) {
                copyfile(pathf("%s/%s%s", tooldir, name, exe), pathf("%s/bin/%s%s", workspace, name, exe), writeExec);
            }
        }
        name = name__prev1;
    }

    if (vflag > 0) {
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
private static (@string, bool) isUnneededSSARewriteFile(@string srcFile) {
    @string archCaps = default;
    bool unneeded = default;

    if (!strings.Contains(srcFile, ssaRewriteFileSubstring)) {
        return ("", false);
    }
    var fileArch = strings.TrimSuffix(strings.TrimPrefix(filepath.Base(srcFile), "rewrite"), ".go");
    if (fileArch == "") {
        return ("", false);
    }
    var b = fileArch[0];
    if (b == '_' || ('a' <= b && b <= 'z')) {
        return ("", false);
    }
    archCaps = fileArch;
    fileArch = strings.ToLower(fileArch);
    fileArch = strings.TrimSuffix(fileArch, "splitload");
    if (fileArch == os.Getenv("GOHOSTARCH")) {
        return ("", false);
    }
    if (fileArch == strings.TrimSuffix(runtime.GOARCH, "le")) {
        return ("", false);
    }
    if (fileArch == strings.TrimSuffix(os.Getenv("GOARCH"), "le")) {
        return ("", false);
    }
    return (archCaps, true);

}

private static @string bootstrapRewriteFile(@string srcFile) { 
    // During bootstrap, generate dummy rewrite files for
    // irrelevant architectures. We only need to build a bootstrap
    // binary that works for the current runtime.GOARCH.
    // This saves 6+ seconds of bootstrap.
    {
        var (archCaps, ok) = isUnneededSSARewriteFile(srcFile);

        if (ok) {
            return fmt.Sprintf("// Code generated by go tool dist; DO NOT EDIT.\n\npackage ssa\n\nfunc rewriteValue%s" +
    "(v *Value) bool { panic(\"unused during bootstrap\") }\nfunc rewriteBlock%s(b *Bloc" +
    "k) bool { panic(\"unused during bootstrap\") }\n", archCaps, archCaps);

        }
    }


    return bootstrapFixImports(srcFile);

}

private static @string bootstrapFixImports(@string srcFile) {
    var lines = strings.SplitAfter(readfile(srcFile), "\n");
    var inBlock = false;
    foreach (var (i, line) in lines) {
        if (strings.HasPrefix(line, "import (")) {
            inBlock = true;
            continue;
        }
        if (inBlock && strings.HasPrefix(line, ")")) {
            inBlock = false;
            continue;
        }
        if (strings.HasPrefix(line, "import \"") || strings.HasPrefix(line, "import . \"") || inBlock && (strings.HasPrefix(line, "\t\"") || strings.HasPrefix(line, "\t. \"") || strings.HasPrefix(line, "\texec \""))) {
            line = strings.Replace(line, "\"cmd/", "\"bootstrap/cmd/", -1); 
            // During bootstrap, must use plain os/exec.
            line = strings.Replace(line, "exec \"internal/execabs\"", "\"os/exec\"", -1);
            foreach (var (_, dir) in bootstrapDirs) {
                if (strings.HasPrefix(dir, "cmd/")) {
                    continue;
                }
                line = strings.Replace(line, "\"" + dir + "\"", "\"bootstrap/" + dir + "\"", -1);
            }
            lines[i] = line;

        }
    }    lines[0] = "// Code generated by go tool dist; DO NOT EDIT.\n// This is a bootstrap copy of " + srcFile + "\n\n//line " + srcFile + ":1\n" + lines[0];

    return strings.Join(lines, "");

}

} // end main_package
