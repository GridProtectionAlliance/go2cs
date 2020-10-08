// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 04:32:47 UTC
// Original source: C:\Go\src\cmd\dist\main.go
using flag = go.flag_package;
using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void usage()
        {
            xprintf(@"usage: go tool dist [command]
Commands are:

banner         print installation banner
bootstrap      rebuild everything
clean          deletes all built files
env [-p]       print environment (-p: include $PATH)
install [dir]  install individual directory
list [-json]   list all supported platforms
test [-h]      run Go test(s)
version        print Go version

All commands take -v flags to emit extra information.
");
            xexit(2L);
        }

        // commands records the available commands.
        private static map commands = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, Action>{"banner":cmdbanner,"bootstrap":cmdbootstrap,"clean":cmdclean,"env":cmdenv,"install":cmdinstall,"list":cmdlist,"test":cmdtest,"version":cmdversion,};

        // main takes care of OS-specific startup and dispatches to xmain.
        private static void Main()
        {
            os.Setenv("TERM", "dumb"); // disable escape codes in clang errors

            // provide -check-armv6k first, before checking for $GOROOT so that
            // it is possible to run this check without having $GOROOT available.
            if (len(os.Args) > 1L && os.Args[1L] == "-check-armv6k")
            {
                useARMv6K(); // might fail with SIGILL
                println("ARMv6K supported.");
                os.Exit(0L);

            }

            gohostos = runtime.GOOS;
            switch (gohostos)
            {
                case "aix": 
                    // uname -m doesn't work under AIX
                    gohostarch = "ppc64";
                    break;
                case "darwin": 
                    // macOS 10.9 and later require clang
                    defaultclang = true;
                    break;
                case "freebsd": 
                    // Since FreeBSD 10 gcc is no longer part of the base system.
                    defaultclang = true;
                    break;
                case "openbsd": 
                    // OpenBSD ships with GCC 4.2, which is now quite old.
                    defaultclang = true;
                    break;
                case "plan9": 
                    gohostarch = os.Getenv("objtype");
                    if (gohostarch == "")
                    {
                        fatalf("$objtype is unset");
                    }

                    break;
                case "solaris": 
                    // Solaris and illumos systems have multi-arch userlands, and
                    // "uname -m" reports the machine hardware name; e.g.,
                    // "i86pc" on both 32- and 64-bit x86 systems.  Check for the
                    // native (widest) instruction set on the running kernel:

                case "illumos": 
                    // Solaris and illumos systems have multi-arch userlands, and
                    // "uname -m" reports the machine hardware name; e.g.,
                    // "i86pc" on both 32- and 64-bit x86 systems.  Check for the
                    // native (widest) instruction set on the running kernel:
                    var @out = run("", CheckExit, "isainfo", "-n");
                    if (strings.Contains(out, "amd64"))
                    {
                        gohostarch = "amd64";
                    }

                    if (strings.Contains(out, "i386"))
                    {
                        gohostarch = "386";
                    }

                    break;
                case "windows": 
                    exe = ".exe";
                    break;
            }

            sysinit();

            if (gohostarch == "")
            { 
                // Default Unix system.
                @out = run("", CheckExit, "uname", "-m");

                if (strings.Contains(out, "x86_64") || strings.Contains(out, "amd64")) 
                    gohostarch = "amd64";
                else if (strings.Contains(out, "86")) 
                    gohostarch = "386";
                    if (gohostos == "darwin")
                    { 
                        // Even on 64-bit platform, some versions of macOS uname -m prints i386.
                        // We don't support any of the OS X versions that run on 32-bit-only hardware anymore.
                        gohostarch = "amd64";

                    }

                else if (strings.Contains(out, "aarch64") || strings.Contains(out, "arm64")) 
                    gohostarch = "arm64";
                else if (strings.Contains(out, "arm")) 
                    gohostarch = "arm";
                else if (strings.Contains(out, "ppc64le")) 
                    gohostarch = "ppc64le";
                else if (strings.Contains(out, "ppc64")) 
                    gohostarch = "ppc64";
                else if (strings.Contains(out, "mips64")) 
                    gohostarch = "mips64";
                    if (elfIsLittleEndian(os.Args[0L]))
                    {
                        gohostarch = "mips64le";
                    }

                else if (strings.Contains(out, "mips")) 
                    gohostarch = "mips";
                    if (elfIsLittleEndian(os.Args[0L]))
                    {
                        gohostarch = "mipsle";
                    }

                else if (strings.Contains(out, "riscv64")) 
                    gohostarch = "riscv64";
                else if (strings.Contains(out, "s390x")) 
                    gohostarch = "s390x";
                else if (gohostos == "darwin") 
                    if (strings.Contains(run("", CheckExit, "uname", "-v"), "RELEASE_ARM64_"))
                    {
                        gohostarch = "arm64";
                    }

                else 
                    fatalf("unknown architecture: %s", out);
                
            }

            if (gohostarch == "arm" || gohostarch == "mips64" || gohostarch == "mips64le")
            {
                maxbg = min(maxbg, runtime.NumCPU());
            }

            bginit();

            if (len(os.Args) > 1L && os.Args[1L] == "-check-goarm")
            {
                useVFPv1(); // might fail with SIGILL
                println("VFPv1 OK.");
                useVFPv3(); // might fail with SIGILL
                println("VFPv3 OK.");
                os.Exit(0L);

            }

            xinit();
            xmain();
            xexit(0L);

        }

        // The OS-specific main calls into the portable code here.
        private static void xmain()
        {
            if (len(os.Args) < 2L)
            {
                usage();
            }

            var cmd = os.Args[1L];
            os.Args = os.Args[1L..]; // for flag parsing during cmd
            flag.Usage = () =>
            {
                fmt.Fprintf(os.Stderr, "usage: go tool dist %s [options]\n", cmd);
                flag.PrintDefaults();
                os.Exit(2L);
            }
;
            {
                var (f, ok) = commands[cmd];

                if (ok)
                {
                    f();
                }
                else
                {
                    xprintf("unknown command %s\n", cmd);
                    usage();
                }

            }

        }
    }
}
