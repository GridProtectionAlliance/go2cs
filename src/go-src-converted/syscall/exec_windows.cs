// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Fork, exec, wait, etc.

// package syscall -- go2cs converted at 2020 August 29 08:37:04 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\exec_windows.go
using sync = go.sync_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public static sync.RWMutex ForkLock = default;

        // EscapeArg rewrites command line argument s as prescribed
        // in http://msdn.microsoft.com/en-us/library/ms880421.
        // This function returns "" (2 double quotes) if s is empty.
        // Alternatively, these transformations are done:
        // - every back slash (\) is doubled, but only if immediately
        //   followed by double quote (");
        // - every double quote (") is escaped by back slash (\);
        // - finally, s is wrapped with double quotes (arg -> "arg"),
        //   but only if there is space or tab inside s.
        public static @string EscapeArg(@string s)
        {
            if (len(s) == 0L)
            {
                return "\"\"";
            }
            var n = len(s);
            var hasSpace = false;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(s); i++)
                {
                    switch (s[i])
                    {
                        case '"': 

                        case '\\': 
                            n++;
                            break;
                        case ' ': 

                        case '\t': 
                            hasSpace = true;
                            break;
                    }
                }


                i = i__prev1;
            }
            if (hasSpace)
            {
                n += 2L;
            }
            if (n == len(s))
            {
                return s;
            }
            var qs = make_slice<byte>(n);
            long j = 0L;
            if (hasSpace)
            {
                qs[j] = '"';
                j++;
            }
            long slashes = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < len(s); i++)
                {
                    switch (s[i])
                    {
                        case '\\': 
                            slashes++;
                            qs[j] = s[i];
                            break;
                        case '"': 
                            while (slashes > 0L)
                            {
                                qs[j] = '\\';
                                j++;
                                slashes--;
                            }

                            qs[j] = '\\';
                            j++;
                            qs[j] = s[i];
                            break;
                        default: 
                            slashes = 0L;
                            qs[j] = s[i];
                            break;
                    }
                    j++;
                }


                i = i__prev1;
            }
            if (hasSpace)
            {
                while (slashes > 0L)
                {
                    qs[j] = '\\';
                    j++;
                    slashes--;
                }

                qs[j] = '"';
                j++;
            }
            return string(qs[..j]);
        }

        // makeCmdLine builds a command line out of args by escaping "special"
        // characters and joining the arguments with spaces.
        private static @string makeCmdLine(slice<@string> args)
        {
            @string s = default;
            foreach (var (_, v) in args)
            {
                if (s != "")
                {
                    s += " ";
                }
                s += EscapeArg(v);
            }
            return s;
        }

        // createEnvBlock converts an array of environment strings into
        // the representation required by CreateProcess: a sequence of NUL
        // terminated strings followed by a nil.
        // Last bytes are two UCS-2 NULs, or four NUL bytes.
        private static ref ushort createEnvBlock(slice<@string> envv)
        {
            if (len(envv) == 0L)
            {
                return ref utf16.Encode((slice<int>)"\x00\x00")[0L];
            }
            long length = 0L;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in envv)
                {
                    s = __s;
                    length += len(s) + 1L;
                }

                s = s__prev1;
            }

            length += 1L;

            var b = make_slice<byte>(length);
            long i = 0L;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in envv)
                {
                    s = __s;
                    var l = len(s);
                    copy(b[i..i + l], (slice<byte>)s);
                    copy(b[i + l..i + l + 1L], new slice<byte>(new byte[] { 0 }));
                    i = i + l + 1L;
                }

                s = s__prev1;
            }

            copy(b[i..i + 1L], new slice<byte>(new byte[] { 0 }));

            return ref utf16.Encode((slice<int>)string(b))[0L];
        }

        public static void CloseOnExec(Handle fd)
        {
            SetHandleInformation(Handle(fd), HANDLE_FLAG_INHERIT, 0L);
        }

        public static error SetNonblock(Handle fd, bool nonblocking)
        {
            return error.As(null);
        }

        // FullPath retrieves the full path of the specified file.
        public static (@string, error) FullPath(@string name)
        {
            var (p, err) = UTF16PtrFromString(name);
            if (err != null)
            {
                return ("", err);
            }
            var n = uint32(100L);
            while (true)
            {
                var buf = make_slice<ushort>(n);
                n, err = GetFullPathName(p, uint32(len(buf)), ref buf[0L], null);
                if (err != null)
                {
                    return ("", err);
                }
                if (n <= uint32(len(buf)))
                {
                    return (UTF16ToString(buf[..n]), null);
                }
            }

        }

        private static bool isSlash(byte c)
        {
            return c == '\\' || c == '/';
        }

        private static (@string, error) normalizeDir(@string dir)
        {
            var (ndir, err) = FullPath(dir);
            if (err != null)
            {
                return ("", err);
            }
            if (len(ndir) > 2L && isSlash(ndir[0L]) && isSlash(ndir[1L]))
            { 
                // dir cannot have \\server\share\path form
                return ("", EINVAL);
            }
            return (ndir, null);
        }

        private static long volToUpper(long ch)
        {
            if ('a' <= ch && ch <= 'z')
            {
                ch += 'A' - 'a';
            }
            return ch;
        }

        private static (@string, error) joinExeDirAndFName(@string dir, @string p)
        {
            if (len(p) == 0L)
            {
                return ("", EINVAL);
            }
            if (len(p) > 2L && isSlash(p[0L]) && isSlash(p[1L]))
            { 
                // \\server\share\path form
                return (p, null);
            }
            if (len(p) > 1L && p[1L] == ':')
            { 
                // has drive letter
                if (len(p) == 2L)
                {
                    return ("", EINVAL);
                }
                if (isSlash(p[2L]))
                {
                    return (p, null);
                }
                else
                {
                    var (d, err) = normalizeDir(dir);
                    if (err != null)
                    {
                        return ("", err);
                    }
                    if (volToUpper(int(p[0L])) == volToUpper(int(d[0L])))
                    {
                        return FullPath(d + "\\" + p[2L..]);
                    }
                    else
                    {
                        return FullPath(p);
                    }
                }
            }
            else
            { 
                // no drive letter
                (d, err) = normalizeDir(dir);
                if (err != null)
                {
                    return ("", err);
                }
                if (isSlash(p[0L]))
                {
                    return FullPath(d[..2L] + p);
                }
                else
                {
                    return FullPath(d + "\\" + p);
                }
            }
        }

        public partial struct ProcAttr
        {
            public @string Dir;
            public slice<@string> Env;
            public slice<System.UIntPtr> Files;
            public ptr<SysProcAttr> Sys;
        }

        public partial struct SysProcAttr
        {
            public bool HideWindow;
            public @string CmdLine; // used if non-empty, else the windows command line is built by escaping the arguments passed to StartProcess
            public uint CreationFlags;
            public Token Token; // if set, runs new process in the security context represented by the token
        }

        private static ProcAttr zeroProcAttr = default;
        private static SysProcAttr zeroSysProcAttr = default;

        public static (long, System.UIntPtr, error) StartProcess(@string argv0, slice<@string> argv, ref ProcAttr _attr) => func(_attr, (ref ProcAttr attr, Defer defer, Panic _, Recover __) =>
        {
            if (len(argv0) == 0L)
            {
                return (0L, 0L, EWINDOWS);
            }
            if (attr == null)
            {
                attr = ref zeroProcAttr;
            }
            var sys = attr.Sys;
            if (sys == null)
            {
                sys = ref zeroSysProcAttr;
            }
            if (len(attr.Files) > 3L)
            {
                return (0L, 0L, EWINDOWS);
            }
            if (len(attr.Files) < 3L)
            {
                return (0L, 0L, EINVAL);
            }
            if (len(attr.Dir) != 0L)
            { 
                // StartProcess assumes that argv0 is relative to attr.Dir,
                // because it implies Chdir(attr.Dir) before executing argv0.
                // Windows CreateProcess assumes the opposite: it looks for
                // argv0 relative to the current directory, and, only once the new
                // process is started, it does Chdir(attr.Dir). We are adjusting
                // for that difference here by making argv0 absolute.
                error err = default;
                argv0, err = joinExeDirAndFName(attr.Dir, argv0);
                if (err != null)
                {
                    return (0L, 0L, err);
                }
            }
            var (argv0p, err) = UTF16PtrFromString(argv0);
            if (err != null)
            {
                return (0L, 0L, err);
            }
            @string cmdline = default; 
            // Windows CreateProcess takes the command line as a single string:
            // use attr.CmdLine if set, else build the command line by escaping
            // and joining each argument with spaces
            if (sys.CmdLine != "")
            {
                cmdline = sys.CmdLine;
            }
            else
            {
                cmdline = makeCmdLine(argv);
            }
            ref ushort argvp = default;
            if (len(cmdline) != 0L)
            {
                argvp, err = UTF16PtrFromString(cmdline);
                if (err != null)
                {
                    return (0L, 0L, err);
                }
            }
            ref ushort dirp = default;
            if (len(attr.Dir) != 0L)
            {
                dirp, err = UTF16PtrFromString(attr.Dir);
                if (err != null)
                {
                    return (0L, 0L, err);
                }
            } 

            // Acquire the fork lock so that no other threads
            // create new fds that are not yet close-on-exec
            // before we fork.
            ForkLock.Lock();
            defer(ForkLock.Unlock());

            var (p, _) = GetCurrentProcess();
            var fd = make_slice<Handle>(len(attr.Files));
            foreach (var (i) in attr.Files)
            {
                if (attr.Files[i] > 0L)
                {
                    err = DuplicateHandle(p, Handle(attr.Files[i]), p, ref fd[i], 0L, true, DUPLICATE_SAME_ACCESS);
                    if (err != null)
                    {
                        return (0L, 0L, err);
                    }
                    defer(CloseHandle(Handle(fd[i])));
                }
            }
            ptr<object> si = @new<StartupInfo>();
            si.Cb = uint32(@unsafe.Sizeof(si.Value));
            si.Flags = STARTF_USESTDHANDLES;
            if (sys.HideWindow)
            {
                si.Flags |= STARTF_USESHOWWINDOW;
                si.ShowWindow = SW_HIDE;
            }
            si.StdInput = fd[0L];
            si.StdOutput = fd[1L];
            si.StdErr = fd[2L];

            ptr<object> pi = @new<ProcessInformation>();

            var flags = sys.CreationFlags | CREATE_UNICODE_ENVIRONMENT;
            if (sys.Token != 0L)
            {
                err = error.As(CreateProcessAsUser(sys.Token, argv0p, argvp, null, null, true, flags, createEnvBlock(attr.Env), dirp, si, pi));
            }
            else
            {
                err = error.As(CreateProcess(argv0p, argvp, null, null, true, flags, createEnvBlock(attr.Env), dirp, si, pi));
            }
            if (err != null)
            {
                return (0L, 0L, err);
            }
            defer(CloseHandle(Handle(pi.Thread)));

            return (int(pi.ProcessId), uintptr(pi.Process), null);
        });

        public static error Exec(@string argv0, slice<@string> argv, slice<@string> envv)
        {
            return error.As(EWINDOWS);
        }
    }
}
