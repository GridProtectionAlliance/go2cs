// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bug implements the ``go bug'' command.
// package bug -- go2cs converted at 2020 August 29 10:00:29 UTC
// import "cmd/go/internal/bug" ==> using bug = go.cmd.go.@internal.bug_package
// Original source: C:\Go\src\cmd\go\internal\bug\bug.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using envcmd = go.cmd.go.@internal.envcmd_package;
using web = go.cmd.go.@internal.web_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class bug_package
    {
        public static base.Command CmdBug = ref new base.Command(Run:runBug,UsageLine:"bug",Short:"start a bug report",Long:`
Bug opens the default browser and starts a new bug report.
The report includes useful system information.
	`,);

        private static void init()
        {
            CmdBug.Flag.BoolVar(ref cfg.BuildV, "v", false, "");
        }

        private static void runBug(ref base.Command cmd, slice<@string> args)
        {
            bytes.Buffer buf = default;
            buf.WriteString(bugHeader);
            inspectGoVersion(ref buf);
            fmt.Fprint(ref buf, "#### System details\n\n");
            fmt.Fprintln(ref buf, "```");
            fmt.Fprintf(ref buf, "go version %s %s/%s\n", runtime.Version(), runtime.GOOS, runtime.GOARCH);
            var env = cfg.CmdEnv;
            env = append(env, envcmd.ExtraEnvVars());
            foreach (var (_, e) in env)
            { 
                // Hide the TERM environment variable from "go bug".
                // See issue #18128
                if (e.Name != "TERM")
                {
                    fmt.Fprintf(ref buf, "%s=\"%s\"\n", e.Name, e.Value);
                }
            }
            printGoDetails(ref buf);
            printOSDetails(ref buf);
            printCDetails(ref buf);
            fmt.Fprintln(ref buf, "```");

            var body = buf.String();
            @string url = "https://github.com/golang/go/issues/new?body=" + web.QueryEscape(body);
            if (!web.OpenBrowser(url))
            {
                fmt.Print("Please file a new issue at golang.org/issue/new using this template:\n\n");
                fmt.Print(body);
            }
        }

        private static readonly @string bugHeader = @"Please answer these questions before submitting your issue. Thanks!

#### What did you do?
If possible, provide a recipe for reproducing the error.
A complete runnable program is good.
A link on play.golang.org is best.


#### What did you expect to see?


#### What did you see instead?


";



        private static void printGoDetails(io.Writer w)
        {
            printCmdOut(w, "GOROOT/bin/go version: ", filepath.Join(runtime.GOROOT(), "bin/go"), "version");
            printCmdOut(w, "GOROOT/bin/go tool compile -V: ", filepath.Join(runtime.GOROOT(), "bin/go"), "tool", "compile", "-V");
        }

        private static void printOSDetails(io.Writer w)
        {
            switch (runtime.GOOS)
            {
                case "darwin": 
                    printCmdOut(w, "uname -v: ", "uname", "-v");
                    printCmdOut(w, "", "sw_vers");
                    break;
                case "linux": 
                    printCmdOut(w, "uname -sr: ", "uname", "-sr");
                    printCmdOut(w, "", "lsb_release", "-a");
                    printGlibcVersion(w);
                    break;
                case "openbsd": 

                case "netbsd": 

                case "freebsd": 

                case "dragonfly": 
                    printCmdOut(w, "uname -v: ", "uname", "-v");
                    break;
                case "solaris": 
                    var (out, err) = ioutil.ReadFile("/etc/release");
                    if (err == null)
                    {
                        fmt.Fprintf(w, "/etc/release: %s\n", out);
                    }
                    else
                    {
                        if (cfg.BuildV)
                        {
                            fmt.Printf("failed to read /etc/release: %v\n", err);
                        }
                    }
                    break;
            }
        }

        private static void printCDetails(io.Writer w)
        {
            printCmdOut(w, "lldb --version: ", "lldb", "--version");
            var cmd = exec.Command("gdb", "--version");
            var (out, err) = cmd.Output();
            if (err == null)
            { 
                // There's apparently no combination of command line flags
                // to get gdb to spit out its version without the license and warranty.
                // Print up to the first newline.
                fmt.Fprintf(w, "gdb --version: %s\n", firstLine(out));
            }
            else
            {
                if (cfg.BuildV)
                {
                    fmt.Printf("failed to run gdb --version: %v\n", err);
                }
            }
        }

        private static void inspectGoVersion(io.Writer w)
        {
            var (data, err) = web.Get("https://golang.org/VERSION?m=text");
            if (err != null)
            {
                if (cfg.BuildV)
                {
                    fmt.Printf("failed to read from golang.org/VERSION: %v\n", err);
                }
                return;
            } 

            // golang.org/VERSION currently returns a whitespace-free string,
            // but just in case, protect against that changing.
            // Similarly so for runtime.Version.
            var release = string(bytes.TrimSpace(data));
            var vers = strings.TrimSpace(runtime.Version());

            if (vers == release)
            { 
                // Up to date
                return;
            } 

            // Devel version or outdated release. Either way, this request is apropos.
            fmt.Fprintf(w, "#### Does this issue reproduce with the latest release (%s)?\n\n\n", release);
        }

        // printCmdOut prints the output of running the given command.
        // It ignores failures; 'go bug' is best effort.
        private static void printCmdOut(io.Writer w, @string prefix, @string path, params @string[] args)
        {
            args = args.Clone();

            var cmd = exec.Command(path, args);
            var (out, err) = cmd.Output();
            if (err != null)
            {
                if (cfg.BuildV)
                {
                    fmt.Printf("%s %s: %v\n", path, strings.Join(args, " "), err);
                }
                return;
            }
            fmt.Fprintf(w, "%s%s\n", prefix, bytes.TrimSpace(out));
        }

        // firstLine returns the first line of a given byte slice.
        private static slice<byte> firstLine(slice<byte> buf)
        {
            var idx = bytes.IndexByte(buf, '\n');
            if (idx > 0L)
            {
                buf = buf[..idx];
            }
            return bytes.TrimSpace(buf);
        }

        // printGlibcVersion prints information about the glibc version.
        // It ignores failures.
        private static void printGlibcVersion(io.Writer w) => func((defer, _, __) =>
        {
            var tempdir = os.TempDir();
            if (tempdir == "")
            {
                return;
            }
            slice<byte> src = (slice<byte>)"int main() {}";
            var srcfile = filepath.Join(tempdir, "go-bug.c");
            var outfile = filepath.Join(tempdir, "go-bug");
            var err = ioutil.WriteFile(srcfile, src, 0644L);
            if (err != null)
            {
                return;
            }
            defer(os.Remove(srcfile));
            var cmd = exec.Command("gcc", "-o", outfile, srcfile);
            _, err = cmd.CombinedOutput();

            if (err != null)
            {
                return;
            }
            defer(os.Remove(outfile));

            cmd = exec.Command("ldd", outfile);
            var (out, err) = cmd.CombinedOutput();
            if (err != null)
            {
                return;
            }
            var re = regexp.MustCompile("libc\\.so[^ ]* => ([^ ]+)");
            var m = re.FindStringSubmatch(string(out));
            if (m == null)
            {
                return;
            }
            cmd = exec.Command(m[1L]);
            out, err = cmd.Output();
            if (err != null)
            {
                return;
            }
            fmt.Fprintf(w, "%s: %s\n", m[1L], firstLine(out)); 

            // print another line (the one containing version string) in case of musl libc
            {
                var idx = bytes.IndexByte(out, '\n');

                if (bytes.Index(out, (slice<byte>)"musl") != -1L && idx > -1L)
                {
                    fmt.Fprintf(w, "%s\n", firstLine(out[idx + 1L..]));
                }

            }
        });
    }
}}}}
