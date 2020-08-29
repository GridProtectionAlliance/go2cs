// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package exec runs external commands. It wraps os.StartProcess to make it
// easier to remap stdin and stdout, connect I/O with pipes, and do other
// adjustments.
//
// Unlike the "system" library call from C and other languages, the
// os/exec package intentionally does not invoke the system shell and
// does not expand any glob patterns or handle other expansions,
// pipelines, or redirections typically done by shells. The package
// behaves more like C's "exec" family of functions. To expand glob
// patterns, either call the shell directly, taking care to escape any
// dangerous input, or use the path/filepath package's Glob function.
// To expand environment variables, use package os's ExpandEnv.
//
// Note that the examples in this package assume a Unix system.
// They may not run on Windows, and they do not run in the Go Playground
// used by golang.org and godoc.org.
// package exec -- go2cs converted at 2020 August 29 08:24:36 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Go\src\os\exec\exec.go
using bytes = go.bytes_package;
using context = go.context_package;
using errors = go.errors_package;
using io = go.io_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace os
{
    public static partial class exec_package
    {
        // Error records the name of a binary that failed to be executed
        // and the reason it failed.
        public partial struct Error
        {
            public @string Name;
            public error Err;
        }

        private static @string Error(this ref Error e)
        {
            return "exec: " + strconv.Quote(e.Name) + ": " + e.Err.Error();
        }

        // Cmd represents an external command being prepared or run.
        //
        // A Cmd cannot be reused after calling its Run, Output or CombinedOutput
        // methods.
        public partial struct Cmd
        {
            public @string Path; // Args holds command line arguments, including the command as Args[0].
// If the Args field is empty or nil, Run uses {Path}.
//
// In typical use, both Path and Args are set by calling Command.
            public slice<@string> Args; // Env specifies the environment of the process.
// Each entry is of the form "key=value".
// If Env is nil, the new process uses the current process's
// environment.
// If Env contains duplicate environment keys, only the last
// value in the slice for each duplicate key is used.
            public slice<@string> Env; // Dir specifies the working directory of the command.
// If Dir is the empty string, Run runs the command in the
// calling process's current directory.
            public @string Dir; // Stdin specifies the process's standard input.
//
// If Stdin is nil, the process reads from the null device (os.DevNull).
//
// If Stdin is an *os.File, the process's standard input is connected
// directly to that file.
//
// Otherwise, during the execution of the command a separate
// goroutine reads from Stdin and delivers that data to the command
// over a pipe. In this case, Wait does not complete until the goroutine
// stops copying, either because it has reached the end of Stdin
// (EOF or a read error) or because writing to the pipe returned an error.
            public io.Reader Stdin; // Stdout and Stderr specify the process's standard output and error.
//
// If either is nil, Run connects the corresponding file descriptor
// to the null device (os.DevNull).
//
// If either is an *os.File, the corresponding output from the process
// is connected directly to that file.
//
// Otherwise, during the execution of the command a separate goroutine
// reads from the process over a pipe and delivers that data to the
// corresponding Writer. In this case, Wait does not complete until the
// goroutine reaches EOF or encounters an error.
//
// If Stdout and Stderr are the same writer, and have a type that can
// be compared with ==, at most one goroutine at a time will call Write.
            public io.Writer Stdout;
            public io.Writer Stderr; // ExtraFiles specifies additional open files to be inherited by the
// new process. It does not include standard input, standard output, or
// standard error. If non-nil, entry i becomes file descriptor 3+i.
            public slice<ref os.File> ExtraFiles; // SysProcAttr holds optional, operating system-specific attributes.
// Run passes it to os.StartProcess as the os.ProcAttr's Sys field.
            public ptr<syscall.SysProcAttr> SysProcAttr; // Process is the underlying process, once started.
            public ptr<os.Process> Process; // ProcessState contains information about an exited process,
// available after a call to Wait or Run.
            public ptr<os.ProcessState> ProcessState;
            public context.Context ctx; // nil means none
            public error lookPathErr; // LookPath error, if any.
            public bool finished; // when Wait was called
            public slice<ref os.File> childFiles;
            public slice<io.Closer> closeAfterStart;
            public slice<io.Closer> closeAfterWait;
            public slice<Func<error>> goroutine;
            public channel<error> errch; // one send per goroutine
            public channel<object> waitDone;
        }

        // Command returns the Cmd struct to execute the named program with
        // the given arguments.
        //
        // It sets only the Path and Args in the returned structure.
        //
        // If name contains no path separators, Command uses LookPath to
        // resolve name to a complete path if possible. Otherwise it uses name
        // directly as Path.
        //
        // The returned Cmd's Args field is constructed from the command name
        // followed by the elements of arg, so arg should not include the
        // command name itself. For example, Command("echo", "hello").
        // Args[0] is always name, not the possibly resolved Path.
        public static ref Cmd Command(@string name, params @string[] arg)
        {
            arg = arg.Clone();

            Cmd cmd = ref new Cmd(Path:name,Args:append([]string{name},arg...),);
            if (filepath.Base(name) == name)
            {
                {
                    var (lp, err) = LookPath(name);

                    if (err != null)
                    {
                        cmd.lookPathErr = err;
                    }
                    else
                    {
                        cmd.Path = lp;
                    }

                }
            }
            return cmd;
        }

        // CommandContext is like Command but includes a context.
        //
        // The provided context is used to kill the process (by calling
        // os.Process.Kill) if the context becomes done before the command
        // completes on its own.
        public static ref Cmd CommandContext(context.Context ctx, @string name, params @string[] arg) => func((_, panic, __) =>
        {
            arg = arg.Clone();

            if (ctx == null)
            {
                panic("nil Context");
            }
            var cmd = Command(name, arg);
            cmd.ctx = ctx;
            return cmd;
        });

        // interfaceEqual protects against panics from doing equality tests on
        // two interfaces with non-comparable underlying types.
        private static bool interfaceEqual(object a, object b) => func((defer, _, recover) =>
        {
            defer(() =>
            {
                recover();
            }());
            return a == b;
        });

        private static slice<@string> envv(this ref Cmd c)
        {
            if (c.Env != null)
            {
                return c.Env;
            }
            return os.Environ();
        }

        private static slice<@string> argv(this ref Cmd c)
        {
            if (len(c.Args) > 0L)
            {
                return c.Args;
            }
            return new slice<@string>(new @string[] { c.Path });
        }

        // skipStdinCopyError optionally specifies a function which reports
        // whether the provided stdin copy error should be ignored.
        // It is non-nil everywhere but Plan 9, which lacks EPIPE. See exec_posix.go.
        private static Func<error, bool> skipStdinCopyError = default;

        private static (ref os.File, error) stdin(this ref Cmd c)
        {
            if (c.Stdin == null)
            {
                f, err = os.Open(os.DevNull);
                if (err != null)
                {
                    return;
                }
                c.closeAfterStart = append(c.closeAfterStart, f);
                return;
            }
            {
                ref os.File (f, ok) = c.Stdin._<ref os.File>();

                if (ok)
                {
                    return (f, null);
                }

            }

            var (pr, pw, err) = os.Pipe();
            if (err != null)
            {
                return;
            }
            c.closeAfterStart = append(c.closeAfterStart, pr);
            c.closeAfterWait = append(c.closeAfterWait, pw);
            c.goroutine = append(c.goroutine, () =>
            {
                var (_, err) = io.Copy(pw, c.Stdin);
                {
                    var skip = skipStdinCopyError;

                    if (skip != null && skip(err))
                    {
                        err = null;
                    }

                }
                {
                    var err1 = pw.Close();

                    if (err == null)
                    {
                        err = err1;
                    }

                }
                return err;
            });
            return (pr, null);
        }

        private static (ref os.File, error) stdout(this ref Cmd c)
        {
            return c.writerDescriptor(c.Stdout);
        }

        private static (ref os.File, error) stderr(this ref Cmd c)
        {
            if (c.Stderr != null && interfaceEqual(c.Stderr, c.Stdout))
            {
                return (c.childFiles[1L], null);
            }
            return c.writerDescriptor(c.Stderr);
        }

        private static (ref os.File, error) writerDescriptor(this ref Cmd c, io.Writer w)
        {
            if (w == null)
            {
                f, err = os.OpenFile(os.DevNull, os.O_WRONLY, 0L);
                if (err != null)
                {
                    return;
                }
                c.closeAfterStart = append(c.closeAfterStart, f);
                return;
            }
            {
                ref os.File (f, ok) = w._<ref os.File>();

                if (ok)
                {
                    return (f, null);
                }

            }

            var (pr, pw, err) = os.Pipe();
            if (err != null)
            {
                return;
            }
            c.closeAfterStart = append(c.closeAfterStart, pw);
            c.closeAfterWait = append(c.closeAfterWait, pr);
            c.goroutine = append(c.goroutine, () =>
            {
                var (_, err) = io.Copy(w, pr);
                pr.Close(); // in case io.Copy stopped due to write error
                return err;
            });
            return (pw, null);
        }

        private static void closeDescriptors(this ref Cmd c, slice<io.Closer> closers)
        {
            foreach (var (_, fd) in closers)
            {
                fd.Close();
            }
        }

        // Run starts the specified command and waits for it to complete.
        //
        // The returned error is nil if the command runs, has no problems
        // copying stdin, stdout, and stderr, and exits with a zero exit
        // status.
        //
        // If the command starts but does not complete successfully, the error is of
        // type *ExitError. Other error types may be returned for other situations.
        //
        // If the calling goroutine has locked the operating system thread
        // with runtime.LockOSThread and modified any inheritable OS-level
        // thread state (for example, Linux or Plan 9 name spaces), the new
        // process will inherit the caller's thread state.
        private static error Run(this ref Cmd c)
        {
            {
                var err = c.Start();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            return error.As(c.Wait());
        }

        // lookExtensions finds windows executable by its dir and path.
        // It uses LookPath to try appropriate extensions.
        // lookExtensions does not search PATH, instead it converts `prog` into `.\prog`.
        private static (@string, error) lookExtensions(@string path, @string dir)
        {
            if (filepath.Base(path) == path)
            {
                path = filepath.Join(".", path);
            }
            if (dir == "")
            {
                return LookPath(path);
            }
            if (filepath.VolumeName(path) != "")
            {
                return LookPath(path);
            }
            if (len(path) > 1L && os.IsPathSeparator(path[0L]))
            {
                return LookPath(path);
            }
            var dirandpath = filepath.Join(dir, path); 
            // We assume that LookPath will only add file extension.
            var (lp, err) = LookPath(dirandpath);
            if (err != null)
            {
                return ("", err);
            }
            var ext = strings.TrimPrefix(lp, dirandpath);
            return (path + ext, null);
        }

        // Start starts the specified command but does not wait for it to complete.
        //
        // The Wait method will return the exit code and release associated resources
        // once the command exits.
        private static error Start(this ref Cmd c)
        {
            if (c.lookPathErr != null)
            {
                c.closeDescriptors(c.closeAfterStart);
                c.closeDescriptors(c.closeAfterWait);
                return error.As(c.lookPathErr);
            }
            if (runtime.GOOS == "windows")
            {
                var (lp, err) = lookExtensions(c.Path, c.Dir);
                if (err != null)
                {
                    c.closeDescriptors(c.closeAfterStart);
                    c.closeDescriptors(c.closeAfterWait);
                    return error.As(err);
                }
                c.Path = lp;
            }
            if (c.Process != null)
            {
                return error.As(errors.New("exec: already started"));
            }
            if (c.ctx != null)
            {
                c.closeDescriptors(c.closeAfterStart);
                c.closeDescriptors(c.closeAfterWait);
                return error.As(c.ctx.Err());
            }
            public delegate  error) F(ref Cmd,  (ref os.File);
            foreach (var (_, setupFd) in new slice<F>(new F[] { (*Cmd).stdin, (*Cmd).stdout, (*Cmd).stderr }))
            {
                var (fd, err) = setupFd(c);
                if (err != null)
                {
                    c.closeDescriptors(c.closeAfterStart);
                    c.closeDescriptors(c.closeAfterWait);
                    return error.As(err);
                }
                c.childFiles = append(c.childFiles, fd);
            }
            c.childFiles = append(c.childFiles, c.ExtraFiles);

            error err = default;
            c.Process, err = os.StartProcess(c.Path, c.argv(), ref new os.ProcAttr(Dir:c.Dir,Files:c.childFiles,Env:dedupEnv(c.envv()),Sys:c.SysProcAttr,));
            if (err != null)
            {
                c.closeDescriptors(c.closeAfterStart);
                c.closeDescriptors(c.closeAfterWait);
                return error.As(err);
            }
            c.closeDescriptors(c.closeAfterStart);

            c.errch = make_channel<error>(len(c.goroutine));
            foreach (var (_, fn) in c.goroutine)
            {
                go_(() => fn =>
                {
                    c.errch.Send(fn());
                }(fn));
            }
            if (c.ctx != null)
            {
                c.waitDone = make_channel<object>();
                go_(() => () =>
                {
                    c.Process.Kill();
                }());
            }
            return error.As(null);
        }

        // An ExitError reports an unsuccessful exit by a command.
        public partial struct ExitError
        {
            public ref os.ProcessState ProcessState => ref ProcessState_ptr; // Stderr holds a subset of the standard error output from the
// Cmd.Output method if standard error was not otherwise being
// collected.
//
// If the error output is long, Stderr may contain only a prefix
// and suffix of the output, with the middle replaced with
// text about the number of omitted bytes.
//
// Stderr is provided for debugging, for inclusion in error messages.
// Users with other needs should redirect Cmd.Stderr as needed.
            public slice<byte> Stderr;
        }

        private static @string Error(this ref ExitError e)
        {
            return e.ProcessState.String();
        }

        // Wait waits for the command to exit and waits for any copying to
        // stdin or copying from stdout or stderr to complete.
        //
        // The command must have been started by Start.
        //
        // The returned error is nil if the command runs, has no problems
        // copying stdin, stdout, and stderr, and exits with a zero exit
        // status.
        //
        // If the command fails to run or doesn't complete successfully, the
        // error is of type *ExitError. Other error types may be
        // returned for I/O problems.
        //
        // If any of c.Stdin, c.Stdout or c.Stderr are not an *os.File, Wait also waits
        // for the respective I/O loop copying to or from the process to complete.
        //
        // Wait releases any resources associated with the Cmd.
        private static error Wait(this ref Cmd c)
        {
            if (c.Process == null)
            {
                return error.As(errors.New("exec: not started"));
            }
            if (c.finished)
            {
                return error.As(errors.New("exec: Wait was already called"));
            }
            c.finished = true;

            var (state, err) = c.Process.Wait();
            if (c.waitDone != null)
            {
                close(c.waitDone);
            }
            c.ProcessState = state;

            error copyError = default;
            foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in c.goroutine)
            {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
                {
                    var err = c.errch.Receive();

                    if (err != null && copyError == null)
                    {
                        copyError = error.As(err);
                    }

                }
            }
            c.closeDescriptors(c.closeAfterWait);

            if (err != null)
            {
                return error.As(err);
            }
            else if (!state.Success())
            {
                return error.As(ref new ExitError(ProcessState:state));
            }
            return error.As(copyError);
        }

        // Output runs the command and returns its standard output.
        // Any returned error will usually be of type *ExitError.
        // If c.Stderr was nil, Output populates ExitError.Stderr.
        private static (slice<byte>, error) Output(this ref Cmd c)
        {
            if (c.Stdout != null)
            {
                return (null, errors.New("exec: Stdout already set"));
            }
            bytes.Buffer stdout = default;
            c.Stdout = ref stdout;

            var captureErr = c.Stderr == null;
            if (captureErr)
            {
                c.Stderr = ref new prefixSuffixSaver(N:32<<10);
            }
            var err = c.Run();
            if (err != null && captureErr)
            {
                {
                    ref ExitError (ee, ok) = err._<ref ExitError>();

                    if (ok)
                    {
                        ee.Stderr = c.Stderr._<ref prefixSuffixSaver>().Bytes();
                    }

                }
            }
            return (stdout.Bytes(), err);
        }

        // CombinedOutput runs the command and returns its combined standard
        // output and standard error.
        private static (slice<byte>, error) CombinedOutput(this ref Cmd c)
        {
            if (c.Stdout != null)
            {
                return (null, errors.New("exec: Stdout already set"));
            }
            if (c.Stderr != null)
            {
                return (null, errors.New("exec: Stderr already set"));
            }
            bytes.Buffer b = default;
            c.Stdout = ref b;
            c.Stderr = ref b;
            var err = c.Run();
            return (b.Bytes(), err);
        }

        // StdinPipe returns a pipe that will be connected to the command's
        // standard input when the command starts.
        // The pipe will be closed automatically after Wait sees the command exit.
        // A caller need only call Close to force the pipe to close sooner.
        // For example, if the command being run will not exit until standard input
        // is closed, the caller must close the pipe.
        private static (io.WriteCloser, error) StdinPipe(this ref Cmd c)
        {
            if (c.Stdin != null)
            {
                return (null, errors.New("exec: Stdin already set"));
            }
            if (c.Process != null)
            {
                return (null, errors.New("exec: StdinPipe after process started"));
            }
            var (pr, pw, err) = os.Pipe();
            if (err != null)
            {
                return (null, err);
            }
            c.Stdin = pr;
            c.closeAfterStart = append(c.closeAfterStart, pr);
            closeOnce wc = ref new closeOnce(File:pw);
            c.closeAfterWait = append(c.closeAfterWait, wc);
            return (wc, null);
        }

        private partial struct closeOnce
        {
            public ref os.File File => ref File_ptr;
            public sync.Once once;
            public error err;
        }

        private static error Close(this ref closeOnce c)
        {
            c.once.Do(c.close);
            return error.As(c.err);
        }

        private static void close(this ref closeOnce c)
        {
            c.err = c.File.Close();
        }

        // StdoutPipe returns a pipe that will be connected to the command's
        // standard output when the command starts.
        //
        // Wait will close the pipe after seeing the command exit, so most callers
        // need not close the pipe themselves; however, an implication is that
        // it is incorrect to call Wait before all reads from the pipe have completed.
        // For the same reason, it is incorrect to call Run when using StdoutPipe.
        // See the example for idiomatic usage.
        private static (io.ReadCloser, error) StdoutPipe(this ref Cmd c)
        {
            if (c.Stdout != null)
            {
                return (null, errors.New("exec: Stdout already set"));
            }
            if (c.Process != null)
            {
                return (null, errors.New("exec: StdoutPipe after process started"));
            }
            var (pr, pw, err) = os.Pipe();
            if (err != null)
            {
                return (null, err);
            }
            c.Stdout = pw;
            c.closeAfterStart = append(c.closeAfterStart, pw);
            c.closeAfterWait = append(c.closeAfterWait, pr);
            return (pr, null);
        }

        // StderrPipe returns a pipe that will be connected to the command's
        // standard error when the command starts.
        //
        // Wait will close the pipe after seeing the command exit, so most callers
        // need not close the pipe themselves; however, an implication is that
        // it is incorrect to call Wait before all reads from the pipe have completed.
        // For the same reason, it is incorrect to use Run when using StderrPipe.
        // See the StdoutPipe example for idiomatic usage.
        private static (io.ReadCloser, error) StderrPipe(this ref Cmd c)
        {
            if (c.Stderr != null)
            {
                return (null, errors.New("exec: Stderr already set"));
            }
            if (c.Process != null)
            {
                return (null, errors.New("exec: StderrPipe after process started"));
            }
            var (pr, pw, err) = os.Pipe();
            if (err != null)
            {
                return (null, err);
            }
            c.Stderr = pw;
            c.closeAfterStart = append(c.closeAfterStart, pw);
            c.closeAfterWait = append(c.closeAfterWait, pr);
            return (pr, null);
        }

        // prefixSuffixSaver is an io.Writer which retains the first N bytes
        // and the last N bytes written to it. The Bytes() methods reconstructs
        // it with a pretty error message.
        private partial struct prefixSuffixSaver
        {
            public long N; // max size of prefix or suffix
            public slice<byte> prefix;
            public slice<byte> suffix; // ring buffer once len(suffix) == N
            public long suffixOff; // offset to write into suffix
            public long skipped; // TODO(bradfitz): we could keep one large []byte and use part of it for
// the prefix, reserve space for the '... Omitting N bytes ...' message,
// then the ring buffer suffix, and just rearrange the ring buffer
// suffix when Bytes() is called, but it doesn't seem worth it for
// now just for error messages. It's only ~64KB anyway.
        }

        private static (long, error) Write(this ref prefixSuffixSaver w, slice<byte> p)
        {
            var lenp = len(p);
            p = w.fill(ref w.prefix, p); 

            // Only keep the last w.N bytes of suffix data.
            {
                var overage = len(p) - w.N;

                if (overage > 0L)
                {
                    p = p[overage..];
                    w.skipped += int64(overage);
                }

            }
            p = w.fill(ref w.suffix, p); 

            // w.suffix is full now if p is non-empty. Overwrite it in a circle.
            while (len(p) > 0L)
            { // 0, 1, or 2 iterations.
                var n = copy(w.suffix[w.suffixOff..], p);
                p = p[n..];
                w.skipped += int64(n);
                w.suffixOff += n;
                if (w.suffixOff == w.N)
                {
                    w.suffixOff = 0L;
                }
            }

            return (lenp, null);
        }

        // fill appends up to len(p) bytes of p to *dst, such that *dst does not
        // grow larger than w.N. It returns the un-appended suffix of p.
        private static slice<byte> fill(this ref prefixSuffixSaver w, ref slice<byte> dst, slice<byte> p)
        {
            {
                var remain = w.N - len(dst.Value);

                if (remain > 0L)
                {
                    var add = minInt(len(p), remain);
                    dst.Value = append(dst.Value, p[..add]);
                    p = p[add..];
                }

            }
            return p;
        }

        private static slice<byte> Bytes(this ref prefixSuffixSaver w)
        {
            if (w.suffix == null)
            {
                return w.prefix;
            }
            if (w.skipped == 0L)
            {
                return append(w.prefix, w.suffix);
            }
            bytes.Buffer buf = default;
            buf.Grow(len(w.prefix) + len(w.suffix) + 50L);
            buf.Write(w.prefix);
            buf.WriteString("\n... omitting ");
            buf.WriteString(strconv.FormatInt(w.skipped, 10L));
            buf.WriteString(" bytes ...\n");
            buf.Write(w.suffix[w.suffixOff..]);
            buf.Write(w.suffix[..w.suffixOff]);
            return buf.Bytes();
        }

        private static long minInt(long a, long b)
        {
            if (a < b)
            {
                return a;
            }
            return b;
        }

        // dedupEnv returns a copy of env with any duplicates removed, in favor of
        // later values.
        // Items not of the normal environment "key=value" form are preserved unchanged.
        private static slice<@string> dedupEnv(slice<@string> env)
        {
            return dedupEnvCase(runtime.GOOS == "windows", env);
        }

        // dedupEnvCase is dedupEnv with a case option for testing.
        // If caseInsensitive is true, the case of keys is ignored.
        private static slice<@string> dedupEnvCase(bool caseInsensitive, slice<@string> env)
        {
            var @out = make_slice<@string>(0L, len(env));
            map saw = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{}; // key => index into out
            foreach (var (_, kv) in env)
            {
                var eq = strings.Index(kv, "=");
                if (eq < 0L)
                {
                    out = append(out, kv);
                    continue;
                }
                var k = kv[..eq];
                if (caseInsensitive)
                {
                    k = strings.ToLower(k);
                }
                {
                    var (dupIdx, isDup) = saw[k];

                    if (isDup)
                    {
                        out[dupIdx] = kv;
                        continue;
                    }

                }
                saw[k] = len(out);
                out = append(out, kv);
            }
            return out;
        }
    }
}}
