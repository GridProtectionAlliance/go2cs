// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Fork, exec, wait, etc.

// package syscall -- go2cs converted at 2022 March 06 22:26:34 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\exec_windows.go
using runtime = go.runtime_package;
using sync = go.sync_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class syscall_package {

public static sync.RWMutex ForkLock = default;

// EscapeArg rewrites command line argument s as prescribed
// in https://msdn.microsoft.com/en-us/library/ms880421.
// This function returns "" (2 double quotes) if s is empty.
// Alternatively, these transformations are done:
// - every back slash (\) is doubled, but only if immediately
//   followed by double quote (");
// - every double quote (") is escaped by back slash (\);
// - finally, s is wrapped with double quotes (arg -> "arg"),
//   but only if there is space or tab inside s.
public static @string EscapeArg(@string s) {
    if (len(s) == 0) {
        return "\"\"";
    }
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
            case '"': 
                // Some escaping required.

            case '\\': 
                // Some escaping required.

            case ' ': 
                // Some escaping required.

            case '\t': 
                // Some escaping required.
                var b = make_slice<byte>(0, len(s) + 2);
                b = appendEscapeArg(b, s);
                return string(b);
                break;
        }

    }
    return s;

}

// appendEscapeArg escapes the string s, as per escapeArg,
// appends the result to b, and returns the updated slice.
private static slice<byte> appendEscapeArg(slice<byte> b, @string s) {
    if (len(s) == 0) {
        return append(b, "\"\"");
    }
    var needsBackslash = false;
    var hasSpace = false;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(s); i++) {
            switch (s[i]) {
                case '"': 

                case '\\': 
                    needsBackslash = true;
                    break;
                case ' ': 

                case '\t': 
                    hasSpace = true;
                    break;
            }

        }

        i = i__prev1;
    }

    if (!needsBackslash && !hasSpace) { 
        // No special handling required; normal case.
        return append(b, s);

    }
    if (!needsBackslash) { 
        // hasSpace is true, so we need to quote the string.
        b = append(b, '"');
        b = append(b, s);
        return append(b, '"');

    }
    if (hasSpace) {
        b = append(b, '"');
    }
    nint slashes = 0;
    {
        nint i__prev1 = i;

        for (i = 0; i < len(s); i++) {
            var c = s[i];
            switch (c) {
                case '\\': 
                    slashes++;
                    break;
                case '"': 
                    while (slashes > 0) {
                        b = append(b, '\\');
                        slashes--;
                    }

                    b = append(b, '\\');

                    break;
                default: 
                    slashes = 0;
                    break;
            }
            b = append(b, c);

        }

        i = i__prev1;
    }
    if (hasSpace) {
        while (slashes > 0) {
            b = append(b, '\\');
            slashes--;
        }
        b = append(b, '"');

    }
    return b;

}

// makeCmdLine builds a command line out of args by escaping "special"
// characters and joining the arguments with spaces.
private static @string makeCmdLine(slice<@string> args) {
    slice<byte> b = default;
    foreach (var (_, v) in args) {
        if (len(b) > 0) {
            b = append(b, ' ');
        }
        b = appendEscapeArg(b, v);

    }    return string(b);

}

// createEnvBlock converts an array of environment strings into
// the representation required by CreateProcess: a sequence of NUL
// terminated strings followed by a nil.
// Last bytes are two UCS-2 NULs, or four NUL bytes.
private static ptr<ushort> createEnvBlock(slice<@string> envv) {
    if (len(envv) == 0) {
        return _addr__addr_utf16.Encode((slice<int>)"\x00\x00")[0]!;
    }
    nint length = 0;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in envv) {
            s = __s;
            length += len(s) + 1;
        }
        s = s__prev1;
    }

    length += 1;

    var b = make_slice<byte>(length);
    nint i = 0;
    {
        var s__prev1 = s;

        foreach (var (_, __s) in envv) {
            s = __s;
            var l = len(s);
            copy(b[(int)i..(int)i + l], (slice<byte>)s);
            copy(b[(int)i + l..(int)i + l + 1], new slice<byte>(new byte[] { 0 }));
            i = i + l + 1;
        }
        s = s__prev1;
    }

    copy(b[(int)i..(int)i + 1], new slice<byte>(new byte[] { 0 }));

    return _addr__addr_utf16.Encode((slice<int>)string(b))[0]!;

}

public static void CloseOnExec(Handle fd) {
    SetHandleInformation(Handle(fd), HANDLE_FLAG_INHERIT, 0);
}

public static error SetNonblock(Handle fd, bool nonblocking) {
    error err = default!;

    return error.As(null!)!;
}

// FullPath retrieves the full path of the specified file.
public static (@string, error) FullPath(@string name) {
    @string path = default;
    error err = default!;

    var (p, err) = UTF16PtrFromString(name);
    if (err != null) {
        return ("", error.As(err)!);
    }
    var n = uint32(100);
    while (true) {
        var buf = make_slice<ushort>(n);
        n, err = GetFullPathName(p, uint32(len(buf)), _addr_buf[0], null);
        if (err != null) {
            return ("", error.As(err)!);
        }
        if (n <= uint32(len(buf))) {
            return (UTF16ToString(buf[..(int)n]), error.As(null!)!);
        }
    }

}

private static bool isSlash(byte c) {
    return c == '\\' || c == '/';
}

private static (@string, error) normalizeDir(@string dir) {
    @string name = default;
    error err = default!;

    var (ndir, err) = FullPath(dir);
    if (err != null) {
        return ("", error.As(err)!);
    }
    if (len(ndir) > 2 && isSlash(ndir[0]) && isSlash(ndir[1])) { 
        // dir cannot have \\server\share\path form
        return ("", error.As(EINVAL)!);

    }
    return (ndir, error.As(null!)!);

}

private static nint volToUpper(nint ch) {
    if ('a' <= ch && ch <= 'z') {
        ch += 'A' - 'a';
    }
    return ch;

}

private static (@string, error) joinExeDirAndFName(@string dir, @string p) {
    @string name = default;
    error err = default!;

    if (len(p) == 0) {
        return ("", error.As(EINVAL)!);
    }
    if (len(p) > 2 && isSlash(p[0]) && isSlash(p[1])) { 
        // \\server\share\path form
        return (p, error.As(null!)!);

    }
    if (len(p) > 1 && p[1] == ':') { 
        // has drive letter
        if (len(p) == 2) {
            return ("", error.As(EINVAL)!);
        }
        if (isSlash(p[2])) {
            return (p, error.As(null!)!);
        }
        else
 {
            var (d, err) = normalizeDir(dir);
            if (err != null) {
                return ("", error.As(err)!);
            }
            if (volToUpper(int(p[0])) == volToUpper(int(d[0]))) {
                return FullPath(d + "\\" + p[(int)2..]);
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
        if (err != null) {
            return ("", error.As(err)!);
        }
        if (isSlash(p[0])) {
            return FullPath(d[..(int)2] + p);
        }
        else
 {
            return FullPath(d + "\\" + p);
        }
    }
}

public partial struct ProcAttr {
    public @string Dir;
    public slice<@string> Env;
    public slice<System.UIntPtr> Files;
    public ptr<SysProcAttr> Sys;
}

public partial struct SysProcAttr {
    public bool HideWindow;
    public @string CmdLine; // used if non-empty, else the windows command line is built by escaping the arguments passed to StartProcess
    public uint CreationFlags;
    public Token Token; // if set, runs new process in the security context represented by the token
    public ptr<SecurityAttributes> ProcessAttributes; // if set, applies these security attributes as the descriptor for the new process
    public ptr<SecurityAttributes> ThreadAttributes; // if set, applies these security attributes as the descriptor for the main thread of the new process
    public bool NoInheritHandles; // if set, each inheritable handle in the calling process is not inherited by the new process
    public slice<Handle> AdditionalInheritedHandles; // a list of additional handles, already marked as inheritable, that will be inherited by the new process
    public Handle ParentProcess; // if non-zero, the new process regards the process given by this handle as its parent process, and AdditionalInheritedHandles, if set, should exist in this parent process
}

private static ProcAttr zeroProcAttr = default;
private static SysProcAttr zeroSysProcAttr = default;

public static (nint, System.UIntPtr, error) StartProcess(@string argv0, slice<@string> argv, ptr<ProcAttr> _addr_attr) => func((defer, _, _) => {
    nint pid = default;
    System.UIntPtr handle = default;
    error err = default!;
    ref ProcAttr attr = ref _addr_attr.val;

    if (len(argv0) == 0) {
        return (0, 0, error.As(EWINDOWS)!);
    }
    if (attr == null) {
        attr = _addr_zeroProcAttr;
    }
    var sys = attr.Sys;
    if (sys == null) {
        sys = _addr_zeroSysProcAttr;
    }
    if (len(attr.Files) > 3) {
        return (0, 0, error.As(EWINDOWS)!);
    }
    if (len(attr.Files) < 3) {
        return (0, 0, error.As(EINVAL)!);
    }
    if (len(attr.Dir) != 0) { 
        // StartProcess assumes that argv0 is relative to attr.Dir,
        // because it implies Chdir(attr.Dir) before executing argv0.
        // Windows CreateProcess assumes the opposite: it looks for
        // argv0 relative to the current directory, and, only once the new
        // process is started, it does Chdir(attr.Dir). We are adjusting
        // for that difference here by making argv0 absolute.
        error err = default!;
        argv0, err = joinExeDirAndFName(attr.Dir, argv0);
        if (err != null) {
            return (0, 0, error.As(err)!);
        }
    }
    var (argv0p, err) = UTF16PtrFromString(argv0);
    if (err != null) {
        return (0, 0, error.As(err)!);
    }
    @string cmdline = default; 
    // Windows CreateProcess takes the command line as a single string:
    // use attr.CmdLine if set, else build the command line by escaping
    // and joining each argument with spaces
    if (sys.CmdLine != "") {
        cmdline = sys.CmdLine;
    }
    else
 {
        cmdline = makeCmdLine(argv);
    }
    ptr<ushort> argvp;
    if (len(cmdline) != 0) {
        argvp, err = UTF16PtrFromString(cmdline);
        if (err != null) {
            return (0, 0, error.As(err)!);
        }
    }
    ptr<ushort> dirp;
    if (len(attr.Dir) != 0) {
        dirp, err = UTF16PtrFromString(attr.Dir);
        if (err != null) {
            return (0, 0, error.As(err)!);
        }
    }
    ref uint maj = ref heap(out ptr<uint> _addr_maj);    ref uint min = ref heap(out ptr<uint> _addr_min);    ref uint build = ref heap(out ptr<uint> _addr_build);

    rtlGetNtVersionNumbers(_addr_maj, _addr_min, _addr_build);
    var isWin7 = maj < 6 || (maj == 6 && min <= 1); 
    // NT kernel handles are divisible by 4, with the bottom 3 bits left as
    // a tag. The fully set tag correlates with the types of handles we're
    // concerned about here.  Except, the kernel will interpret some
    // special handle values, like -1, -2, and so forth, so kernelbase.dll
    // checks to see that those bottom three bits are checked, but that top
    // bit is not checked.
    Func<Handle, bool> isLegacyWin7ConsoleHandle = handle => isWin7 && handle & 0x10000003 == 3;

    var (p, _) = GetCurrentProcess();
    var parentProcess = p;
    if (sys.ParentProcess != 0) {
        parentProcess = sys.ParentProcess;
    }
    var fd = make_slice<Handle>(len(attr.Files));
    {
        var i__prev1 = i;

        foreach (var (__i) in attr.Files) {
            i = __i;
            if (attr.Files[i] > 0) {
                var destinationProcessHandle = parentProcess; 

                // On Windows 7, console handles aren't real handles, and can only be duplicated
                // into the current process, not a parent one, which amounts to the same thing.
                if (parentProcess != p && isLegacyWin7ConsoleHandle(Handle(attr.Files[i]))) {
                    destinationProcessHandle = p;
                }

                err = DuplicateHandle(p, Handle(attr.Files[i]), destinationProcessHandle, _addr_fd[i], 0, true, DUPLICATE_SAME_ACCESS);
                if (err != null) {
                    return (0, 0, error.As(err)!);
                }

                defer(DuplicateHandle(parentProcess, fd[i], 0, null, 0, false, DUPLICATE_CLOSE_SOURCE));

            }

        }
        i = i__prev1;
    }

    ptr<object> si = @new<_STARTUPINFOEXW>();
    si.ProcThreadAttributeList, err = newProcThreadAttributeList(2);
    if (err != null) {
        return (0, 0, error.As(err)!);
    }
    defer(deleteProcThreadAttributeList(si.ProcThreadAttributeList));
    si.Cb = uint32(@unsafe.Sizeof(si.val));
    si.Flags = STARTF_USESTDHANDLES;
    if (sys.HideWindow) {
        si.Flags |= STARTF_USESHOWWINDOW;
        si.ShowWindow = SW_HIDE;
    }
    if (sys.ParentProcess != 0) {
        err = error.As(updateProcThreadAttribute(si.ProcThreadAttributeList, 0, _PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, @unsafe.Pointer(_addr_sys.ParentProcess), @unsafe.Sizeof(sys.ParentProcess), null, null))!;
        if (err != null) {
            return (0, 0, error.As(err)!);
        }
    }
    si.StdInput = fd[0];
    si.StdOutput = fd[1];
    si.StdErr = fd[2];

    fd = append(fd, sys.AdditionalInheritedHandles); 

    // On Windows 7, console handles aren't real handles, so don't pass them
    // through to PROC_THREAD_ATTRIBUTE_HANDLE_LIST.
    {
        var i__prev1 = i;

        foreach (var (__i) in fd) {
            i = __i;
            if (isLegacyWin7ConsoleHandle(fd[i])) {
                fd[i] = 0;
            }
        }
        i = i__prev1;
    }

    nint j = 0;
    {
        var i__prev1 = i;

        foreach (var (__i) in fd) {
            i = __i;
            if (fd[i] != 0) {
                fd[j] = fd[i];
                j++;
            }
        }
        i = i__prev1;
    }

    fd = fd[..(int)j];

    var willInheritHandles = len(fd) > 0 && !sys.NoInheritHandles; 

    // Do not accidentally inherit more than these handles.
    if (willInheritHandles) {
        err = error.As(updateProcThreadAttribute(si.ProcThreadAttributeList, 0, _PROC_THREAD_ATTRIBUTE_HANDLE_LIST, @unsafe.Pointer(_addr_fd[0]), uintptr(len(fd)) * @unsafe.Sizeof(fd[0]), null, null))!;
        if (err != null) {
            return (0, 0, error.As(err)!);
        }
    }
    ptr<object> pi = @new<ProcessInformation>();
    var flags = sys.CreationFlags | CREATE_UNICODE_ENVIRONMENT | _EXTENDED_STARTUPINFO_PRESENT;
    if (sys.Token != 0) {
        err = error.As(CreateProcessAsUser(sys.Token, argv0p, argvp, sys.ProcessAttributes, sys.ThreadAttributes, willInheritHandles, flags, createEnvBlock(attr.Env), dirp, _addr_si.StartupInfo, pi))!;
    }
    else
 {
        err = error.As(CreateProcess(argv0p, argvp, sys.ProcessAttributes, sys.ThreadAttributes, willInheritHandles, flags, createEnvBlock(attr.Env), dirp, _addr_si.StartupInfo, pi))!;
    }
    if (err != null) {
        return (0, 0, error.As(err)!);
    }
    defer(CloseHandle(Handle(pi.Thread)));
    runtime.KeepAlive(fd);
    runtime.KeepAlive(sys);

    return (int(pi.ProcessId), uintptr(pi.Process), error.As(null!)!);

});

public static error Exec(@string argv0, slice<@string> argv, slice<@string> envv) {
    error err = default!;

    return error.As(EWINDOWS)!;
}

} // end syscall_package
