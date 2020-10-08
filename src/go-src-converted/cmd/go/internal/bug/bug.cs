// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bug implements the ``go bug'' command.
// package bug -- go2cs converted at 2020 October 08 04:33:30 UTC
// import "cmd/go/internal/bug" ==> using bug = go.cmd.go.@internal.bug_package
// Original source: C:\Go\src\cmd\go\internal\bug\bug.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using urlpkg = go.net.url_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using runtime = go.runtime_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using web = go.cmd.go.@internal.web_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class bug_package
    {
        public static ptr<base.Command> CmdBug = addr(new base.Command(Run:runBug,UsageLine:"go bug",Short:"start a bug report",Long:`
Bug opens the default browser and starts a new bug report.
The report includes useful system information.
	`,));

        private static void init()
        {
            CmdBug.Flag.BoolVar(_addr_cfg.BuildV, "v", false, "");
        }

        private static void runBug(ptr<base.Command> _addr_cmd, slice<@string> args)
        {
            ref base.Command cmd = ref _addr_cmd.val;

            if (len(args) > 0L)
            {
                @base.Fatalf("go bug: bug takes no arguments");
            }

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            buf.WriteString(bugHeader);
            printGoVersion(_addr_buf);
            buf.WriteString("### Does this issue reproduce with the latest release?\n\n\n");
            printEnvDetails(_addr_buf);
            buf.WriteString(bugFooter);

            var body = buf.String();
            @string url = "https://github.com/golang/go/issues/new?body=" + urlpkg.QueryEscape(body);
            if (!web.OpenBrowser(url))
            {
                fmt.Print("Please file a new issue at golang.org/issue/new using this template:\n\n");
                fmt.Print(body);
            }

        }

        private static readonly @string bugHeader = (@string)"<!-- Please answer these questions before submitting your issue. Thanks! -->\n\n";

        private static readonly @string bugFooter = (@string)"### What did you do?\n\n<!--\nIf possible, provide a recipe for reproducing the erro" +
    "r.\nA complete runnable program is good.\nA link on play.golang.org is best.\n-->\n\n" +
    "\n\n### What did you expect to see?\n\n\n\n### What did you see instead?\n\n";



        private static void printGoVersion(io.Writer w)
        {
            fmt.Fprintf(w, "### What version of Go are you using (`go version`)?\n\n");
            fmt.Fprintf(w, "<pre>\n");
            fmt.Fprintf(w, "$ go version\n");
            printCmdOut(w, "", "go", "version");
            fmt.Fprintf(w, "</pre>\n");
            fmt.Fprintf(w, "\n");
        }

        private static void printEnvDetails(io.Writer w)
        {
            fmt.Fprintf(w, "### What operating system and processor architecture are you using (`go env`)?\n\n");
            fmt.Fprintf(w, "<details><summary><code>go env</code> Output</summary><br><pre>\n");
            fmt.Fprintf(w, "$ go env\n");
            printCmdOut(w, "", "go", "env");
            printGoDetails(w);
            printOSDetails(w);
            printCDetails(w);
            fmt.Fprintf(w, "</pre></details>\n\n");
        }

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
                case "illumos": 
                    // Be sure to use the OS-supplied uname, in "/usr/bin":

                case "solaris": 
                    // Be sure to use the OS-supplied uname, in "/usr/bin":
                    printCmdOut(w, "uname -srv: ", "/usr/bin/uname", "-srv");
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

                return ;

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
                return ;
            }

            slice<byte> src = (slice<byte>)"int main() {}";
            var srcfile = filepath.Join(tempdir, "go-bug.c");
            var outfile = filepath.Join(tempdir, "go-bug");
            var err = ioutil.WriteFile(srcfile, src, 0644L);
            if (err != null)
            {
                return ;
            }

            defer(os.Remove(srcfile));
            var cmd = exec.Command("gcc", "-o", outfile, srcfile);
            _, err = cmd.CombinedOutput();

            if (err != null)
            {
                return ;
            }

            defer(os.Remove(outfile));

            cmd = exec.Command("ldd", outfile);
            var (out, err) = cmd.CombinedOutput();
            if (err != null)
            {
                return ;
            }

            var re = regexp.MustCompile("libc\\.so[^ ]* => ([^ ]+)");
            var m = re.FindStringSubmatch(string(out));
            if (m == null)
            {
                return ;
            }

            cmd = exec.Command(m[1L]);
            out, err = cmd.Output();
            if (err != null)
            {
                return ;
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
