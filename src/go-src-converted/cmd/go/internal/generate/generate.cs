// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package generate implements the ``go generate'' command.
// package generate -- go2cs converted at 2020 August 29 10:00:37 UTC
// import "cmd/go/internal/generate" ==> using generate = go.cmd.go.@internal.generate_package
// Original source: C:\Go\src\cmd\go\internal\generate\generate.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
using log = go.log_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

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
    public static partial class generate_package
    {
        public static base.Command CmdGenerate = ref new base.Command(Run:runGenerate,UsageLine:"generate [-run regexp] [-n] [-v] [-x] [build flags] [file.go... | packages]",Short:"generate Go files by processing source",Long:`
Generate runs commands described by directives within existing
files. Those commands can run any process but the intent is to
create or update Go source files.

Go generate is never run automatically by go build, go get, go test,
and so on. It must be run explicitly.

Go generate scans the file for directives, which are lines of
the form,

	//go:generate command argument...

(note: no leading spaces and no space in "//go") where command
is the generator to be run, corresponding to an executable file
that can be run locally. It must either be in the shell path
(gofmt), a fully qualified path (/usr/you/bin/mytool), or a
command alias, described below.

Note that go generate does not parse the file, so lines that look
like directives in comments or multiline strings will be treated
as directives.

The arguments to the directive are space-separated tokens or
double-quoted strings passed to the generator as individual
arguments when it is run.

Quoted strings use Go syntax and are evaluated before execution; a
quoted string appears as a single argument to the generator.

Go generate sets several variables when it runs the generator:

	$GOARCH
		The execution architecture (arm, amd64, etc.)
	$GOOS
		The execution operating system (linux, windows, etc.)
	$GOFILE
		The base name of the file.
	$GOLINE
		The line number of the directive in the source file.
	$GOPACKAGE
		The name of the package of the file containing the directive.
	$DOLLAR
		A dollar sign.

Other than variable substitution and quoted-string evaluation, no
special processing such as "globbing" is performed on the command
line.

As a last step before running the command, any invocations of any
environment variables with alphanumeric names, such as $GOFILE or
$HOME, are expanded throughout the command line. The syntax for
variable expansion is $NAME on all operating systems. Due to the
order of evaluation, variables are expanded even inside quoted
strings. If the variable NAME is not set, $NAME expands to the
empty string.

A directive of the form,

	//go:generate -command xxx args...

specifies, for the remainder of this source file only, that the
string xxx represents the command identified by the arguments. This
can be used to create aliases or to handle multiword generators.
For example,

	//go:generate -command foo go tool foo

specifies that the command "foo" represents the generator
"go tool foo".

Generate processes packages in the order given on the command line,
one at a time. If the command line lists .go files, they are treated
as a single package. Within a package, generate processes the
source files in a package in file name order, one at a time. Within
a source file, generate runs generators in the order they appear
in the file, one at a time.

If any generator returns an error exit status, "go generate" skips
all further processing for that package.

The generator is run in the package's source directory.

Go generate accepts one specific flag:

	-run=""
		if non-empty, specifies a regular expression to select
		directives whose full original source text (excluding
		any trailing spaces and final newline) matches the
		expression.

It also accepts the standard build flags including -v, -n, and -x.
The -v flag prints the names of packages and files as they are
processed.
The -n flag prints commands that would be executed.
The -x flag prints commands as they are executed.

For more about build flags, see 'go help build'.

For more about specifying packages, see 'go help packages'.
	`,);

        private static @string generateRunFlag = default;        private static ref regexp.Regexp generateRunRE = default;

        private static void init()
        {
            work.AddBuildFlags(CmdGenerate);
            CmdGenerate.Flag.StringVar(ref generateRunFlag, "run", "", "");
        }

        private static void runGenerate(ref base.Command cmd, slice<@string> args)
        {
            load.IgnoreImports = true;

            if (generateRunFlag != "")
            {
                error err = default;
                generateRunRE, err = regexp.Compile(generateRunFlag);
                if (err != null)
                {
                    log.Fatalf("generate: %s", err);
                }
            } 
            // Even if the arguments are .go files, this loop suffices.
            foreach (var (_, pkg) in load.Packages(args))
            {
                foreach (var (_, file) in pkg.InternalGoFiles())
                {
                    if (!generate(pkg.Name, file))
                    {
                        break;
                    }
                }
            }
        }

        // generate runs the generation directives for a single file.
        private static bool generate(@string pkg, @string absFile) => func((defer, _, __) =>
        {
            var (fd, err) = os.Open(absFile);
            if (err != null)
            {
                log.Fatalf("generate: %s", err);
            }
            defer(fd.Close());
            Generator g = ref new Generator(r:fd,path:absFile,pkg:pkg,commands:make(map[string][]string),);
            return g.run();
        });

        // A Generator represents the state of a single Go source file
        // being scanned for generator commands.
        public partial struct Generator
        {
            public io.Reader r;
            public @string path; // full rooted path name.
            public @string dir; // full rooted directory of file.
            public @string file; // base name of file.
            public @string pkg;
            public map<@string, slice<@string>> commands;
            public long lineNum; // current line number.
            public slice<@string> env;
        }

        // run runs the generators in the current file.
        private static bool run(this ref Generator _g) => func(_g, (ref Generator g, Defer defer, Panic panic, Recover _) =>
        { 
            // Processing below here calls g.errorf on failure, which does panic(stop).
            // If we encounter an error, we abort the package.
            defer(() =>
            {
                var e = recover();
                if (e != null)
                {
                    ok = false;
                    if (e != stop)
                    {
                        panic(e);
                    }
                    @base.SetExitStatus(1L);
                }
            }());
            g.dir, g.file = filepath.Split(g.path);
            g.dir = filepath.Clean(g.dir); // No final separator please.
            if (cfg.BuildV)
            {
                fmt.Fprintf(os.Stderr, "%s\n", @base.ShortPath(g.path));
            } 

            // Scan for lines that start "//go:generate".
            // Can't use bufio.Scanner because it can't handle long lines,
            // which are likely to appear when using generate.
            var input = bufio.NewReader(g.r);
            error err = default; 
            // One line per loop.
            while (true)
            {
                g.lineNum++; // 1-indexed.
                slice<byte> buf = default;
                buf, err = input.ReadSlice('\n');
                if (err == bufio.ErrBufferFull)
                { 
                    // Line too long - consume and ignore.
                    if (isGoGenerate(buf))
                    {
                        g.errorf("directive too long");
                    }
                    while (err == bufio.ErrBufferFull)
                    {
                        _, err = input.ReadSlice('\n');
                    }

                    if (err != null)
                    {
                        break;
                    }
                    continue;
                }
                if (err != null)
                { 
                    // Check for marker at EOF without final \n.
                    if (err == io.EOF && isGoGenerate(buf))
                    {
                        err = error.As(io.ErrUnexpectedEOF);
                    }
                    break;
                }
                if (!isGoGenerate(buf))
                {
                    continue;
                }
                if (generateRunFlag != "")
                {
                    if (!generateRunRE.Match(bytes.TrimSpace(buf)))
                    {
                        continue;
                    }
                }
                g.setEnv();
                var words = g.split(string(buf));
                if (len(words) == 0L)
                {
                    g.errorf("no arguments to directive");
                }
                if (words[0L] == "-command")
                {
                    g.setShorthand(words);
                    continue;
                } 
                // Run the command line.
                if (cfg.BuildN || cfg.BuildX)
                {
                    fmt.Fprintf(os.Stderr, "%s\n", strings.Join(words, " "));
                }
                if (cfg.BuildN)
                {
                    continue;
                }
                g.exec(words);
            }

            if (err != null && err != io.EOF)
            {
                g.errorf("error reading %s: %s", @base.ShortPath(g.path), err);
            }
            return true;
        });

        private static bool isGoGenerate(slice<byte> buf)
        {
            return bytes.HasPrefix(buf, (slice<byte>)"//go:generate ") || bytes.HasPrefix(buf, (slice<byte>)"//go:generate\t");
        }

        // setEnv sets the extra environment variables used when executing a
        // single go:generate command.
        private static void setEnv(this ref Generator g)
        {
            g.env = new slice<@string>(new @string[] { "GOARCH="+cfg.BuildContext.GOARCH, "GOOS="+cfg.BuildContext.GOOS, "GOFILE="+g.file, "GOLINE="+strconv.Itoa(g.lineNum), "GOPACKAGE="+g.pkg, "DOLLAR="+"$" });
        }

        // split breaks the line into words, evaluating quoted
        // strings and evaluating environment variables.
        // The initial //go:generate element is present in line.
        private static slice<@string> split(this ref Generator g, @string line)
        { 
            // Parse line, obeying quoted strings.
            slice<@string> words = default;
            line = line[len("//go:generate ")..len(line) - 1L]; // Drop preamble and final newline.
            // There may still be a carriage return.
            if (len(line) > 0L && line[len(line) - 1L] == '\r')
            {
                line = line[..len(line) - 1L];
            } 
            // One (possibly quoted) word per iteration.
Words: 
            // Substitute command if required.
            while (true)
            {
                line = strings.TrimLeft(line, " \t");
                if (len(line) == 0L)
                {
                    break;
                }
                if (line[0L] == '"')
                {
                    {
                        long i__prev2 = i;

                        for (long i = 1L; i < len(line); i++)
                        {
                            var c = line[i]; // Only looking for ASCII so this is OK.
                            switch (c)
                            {
                                case '\\': 
                                    if (i + 1L == len(line))
                                    {
                                        g.errorf("bad backslash");
                                    }
                                    i++; // Absorb next byte (If it's a multibyte we'll get an error in Unquote).
                                    break;
                                case '"': 
                                    var (word, err) = strconv.Unquote(line[0L..i + 1L]);
                                    if (err != null)
                                    {
                                        g.errorf("bad quoted string");
                                    }
                                    words = append(words, word);
                                    line = line[i + 1L..]; 
                                    // Check the next character is space or end of line.
                                    if (len(line) > 0L && line[0L] != ' ' && line[0L] != '\t')
                                    {
                                        g.errorf("expect space after quoted argument");
                                    }
                                    _continueWords = true;
                                    break;
                                    break;
                            }
                        }


                        i = i__prev2;
                    }
                    g.errorf("mismatched quoted string");
                }
                i = strings.IndexAny(line, " \t");
                if (i < 0L)
                {
                    i = len(line);
                }
                words = append(words, line[0L..i]);
                line = line[i..];
            } 
            // Substitute command if required.
 
            // Substitute command if required.
            if (len(words) > 0L && g.commands[words[0L]] != null)
            { 
                // Replace 0th word by command substitution.
                words = append(g.commands[words[0L]], words[1L..]);
            } 
            // Substitute environment variables.
            {
                long i__prev1 = i;
                var word__prev1 = word;

                foreach (var (__i, __word) in words)
                {
                    i = __i;
                    word = __word;
                    words[i] = os.Expand(word, g.expandVar);
                }

                i = i__prev1;
                word = word__prev1;
            }

            return words;
        }

        private static var stop = fmt.Errorf("error in generation");

        // errorf logs an error message prefixed with the file and line number.
        // It then exits the program (with exit status 1) because generation stops
        // at the first error.
        private static void errorf(this ref Generator _g, @string format, params object[] args) => func(_g, (ref Generator g, Defer _, Panic panic, Recover __) =>
        {
            fmt.Fprintf(os.Stderr, "%s:%d: %s\n", @base.ShortPath(g.path), g.lineNum, fmt.Sprintf(format, args));
            panic(stop);
        });

        // expandVar expands the $XXX invocation in word. It is called
        // by os.Expand.
        private static @string expandVar(this ref Generator g, @string word)
        {
            var w = word + "=";
            foreach (var (_, e) in g.env)
            {
                if (strings.HasPrefix(e, w))
                {
                    return e[len(w)..];
                }
            }
            return os.Getenv(word);
        }

        // setShorthand installs a new shorthand as defined by a -command directive.
        private static void setShorthand(this ref Generator g, slice<@string> words)
        { 
            // Create command shorthand.
            if (len(words) == 1L)
            {
                g.errorf("no command specified for -command");
            }
            var command = words[1L];
            if (g.commands[command] != null)
            {
                g.errorf("command %q multiply defined", command);
            }
            g.commands[command] = words.slice(2L, len(words), len(words)); // force later append to make copy
        }

        // exec runs the command specified by the argument. The first word is
        // the command name itself.
        private static void exec(this ref Generator g, slice<@string> words)
        {
            var cmd = exec.Command(words[0L], words[1L..]); 
            // Standard in and out of generator should be the usual.
            cmd.Stdout = os.Stdout;
            cmd.Stderr = os.Stderr; 
            // Run the command in the package directory.
            cmd.Dir = g.dir;
            cmd.Env = @base.MergeEnvLists(g.env, cfg.OrigEnv);
            var err = cmd.Run();
            if (err != null)
            {
                g.errorf("running %q: %s", words[0L], err);
            }
        }
    }
}}}}
