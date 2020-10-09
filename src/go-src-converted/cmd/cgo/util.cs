// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:23:56 UTC
// Original source: C:\Go\src\cmd\cgo\util.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // run runs the command argv, feeding in stdin on standard input.
        // It returns the output to standard output and standard error.
        // ok indicates whether the command exited successfully.
        private static (slice<byte>, slice<byte>, bool) run(slice<byte> stdin, slice<@string> argv) => func((defer, _, __) =>
        {
            slice<byte> stdout = default;
            slice<byte> stderr = default;
            bool ok = default;

            {
                var i = find(argv, "-xc");

                if (i >= 0L && argv[len(argv) - 1L] == "-")
                { 
                    // Some compilers have trouble with standard input.
                    // Others have trouble with -xc.
                    // Avoid both problems by writing a file with a .c extension.
                    var (f, err) = ioutil.TempFile("", "cgo-gcc-input-");
                    if (err != null)
                    {
                        fatalf("%s", err);
                    }
                    var name = f.Name();
                    f.Close();
                    {
                        var err__prev2 = err;

                        var err = ioutil.WriteFile(name + ".c", stdin, 0666L);

                        if (err != null)
                        {
                            os.Remove(name);
                            fatalf("%s", err);
                        }
                        err = err__prev2;

                    }

                    defer(os.Remove(name));
                    defer(os.Remove(name + ".c")); 

                    // Build new argument list without -xc and trailing -.
                    var @new = append(argv.slice(-1, i, i), argv[i + 1L..len(argv) - 1L]); 

                    // Since we are going to write the file to a temporary directory,
                    // we will need to add -I . explicitly to the command line:
                    // any #include "foo" before would have looked in the current
                    // directory as the directory "holding" standard input, but now
                    // the temporary directory holds the input.
                    // We've also run into compilers that reject "-I." but allow "-I", ".",
                    // so be sure to use two arguments.
                    // This matters mainly for people invoking cgo -godefs by hand.
                    new = append(new, "-I", "."); 

                    // Finish argument list with path to C file.
                    new = append(new, name + ".c");

                    argv = new;
                    stdin = null;

                }
            }


            var p = exec.Command(argv[0L], argv[1L..]);
            p.Stdin = bytes.NewReader(stdin);
            ref bytes.Buffer bout = ref heap(out ptr<bytes.Buffer> _addr_bout);            ref bytes.Buffer berr = ref heap(out ptr<bytes.Buffer> _addr_berr);

            _addr_p.Stdout = _addr_bout;
            p.Stdout = ref _addr_p.Stdout.val;
            _addr_p.Stderr = _addr_berr;
            p.Stderr = ref _addr_p.Stderr.val; 
            // Disable escape codes in clang error messages.
            p.Env = append(os.Environ(), "TERM=dumb");
            err = p.Run();
            {
                ptr<exec.ExitError> (_, ok) = err._<ptr<exec.ExitError>>();

                if (err != null && !ok)
                {
                    fatalf("%s", err);
                }
            }

            ok = p.ProcessState.Success();
            stdout = bout.Bytes();
            stderr = berr.Bytes();
            return ;

        });

        private static long find(slice<@string> argv, @string target)
        {
            foreach (var (i, arg) in argv)
            {
                if (arg == target)
                {
                    return i;
                }

            }
            return -1L;

        }

        private static @string lineno(token.Pos pos)
        {
            return fset.Position(pos).String();
        }

        // Die with an error message.
        private static void fatalf(@string msg, params object[] args)
        {
            args = args.Clone();
 
            // If we've already printed other errors, they might have
            // caused the fatal condition. Assume they're enough.
            if (nerrors == 0L)
            {
                fmt.Fprintf(os.Stderr, msg + "\n", args);
            }

            os.Exit(2L);

        }

        private static long nerrors = default;

        private static void error_(token.Pos pos, @string msg, params object[] args)
        {
            args = args.Clone();

            nerrors++;
            if (pos.IsValid())
            {
                fmt.Fprintf(os.Stderr, "%s: ", fset.Position(pos).String());
            }
            else
            {
                fmt.Fprintf(os.Stderr, "cgo: ");
            }

            fmt.Fprintf(os.Stderr, msg, args);
            fmt.Fprintf(os.Stderr, "\n");

        }

        // isName reports whether s is a valid C identifier
        private static bool isName(@string s)
        {
            foreach (var (i, v) in s)
            {
                if (v != '_' && (v < 'A' || v > 'Z') && (v < 'a' || v > 'z') && (v < '0' || v > '9'))
                {
                    return false;
                }

                if (i == 0L && '0' <= v && v <= '9')
                {
                    return false;
                }

            }
            return s != "";

        }

        private static ptr<os.File> creat(@string name)
        {
            var (f, err) = os.Create(name);
            if (err != null)
            {
                fatalf("%s", err);
            }

            return _addr_f!;

        }

        private static int slashToUnderscore(int c)
        {
            if (c == '/' || c == '\\' || c == ':')
            {
                c = '_';
            }

            return c;

        }
    }
}
