// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Fork, exec, wait, etc.
namespace go;

using bytealg = @internal.bytealg_package;
using runtime = runtime_package;
using sync = sync_package;
using utf16 = unicode.utf16_package;
using @unsafe = unsafe_package;
using @internal;
using unicode;

partial class syscall_package {

// ForkLock is not used on Windows.
public static sync.RWMutex ForkLock;

// EscapeArg rewrites command line argument s as prescribed
// in https://msdn.microsoft.com/en-us/library/ms880421.
// This function returns "" (2 double quotes) if s is empty.
// Alternatively, these transformations are done:
//   - every back slash (\) is doubled, but only if immediately
//     followed by double quote (");
//   - every double quote (") is escaped by back slash (\);
//   - finally, s is wrapped with double quotes (arg -> "arg"),
//     but only if there is space or tab inside s.
public static @string EscapeArg(@string s) {
    if (len(s) == 0) {
        return @""""""u8;
    }
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (rune)'"' or (rune)'\\' or (rune)' ' or (rune)'\t': {
            var b = new slice<byte>(0, // Some escaping required.
 len(s) + 2);
            b = appendEscapeArg(b, s);
            return ((@string)b);
        }}

    }
    return s;
}

// appendEscapeArg escapes the string s, as per escapeArg,
// appends the result to b, and returns the updated slice.
internal static slice<byte> appendEscapeArg(slice<byte> b, @string s) {
    if (len(s) == 0) {
        return append(b, @""""""u8.ꓸꓸꓸ);
    }
    var needsBackslash = false;
    var hasSpace = false;
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (rune)'"' or (rune)'\\': {
            needsBackslash = true;
            break;
        }
        case (rune)' ' or (rune)'\t': {
            hasSpace = true;
            break;
        }}

    }
    if (!needsBackslash && !hasSpace) {
        // No special handling required; normal case.
        return append(b, s.ꓸꓸꓸ);
    }
    if (!needsBackslash) {
        // hasSpace is true, so we need to quote the string.
        b = append(b, (rune)'"');
        b = append(b, s.ꓸꓸꓸ);
        return append(b, (rune)'"');
    }
    if (hasSpace) {
        b = append(b, (rune)'"');
    }
    nint slashes = 0;
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        switch (c) {
        default: {
            slashes = 0;
            break;
        }
        case (rune)'\\': {
            slashes++;
            break;
        }
        case (rune)'"': {
            for (; slashes > 0; slashes--) {
                b = append(b, (rune)'\\');
            }
            b = append(b, (rune)'\\');
            break;
        }}

        b = append(b, c);
    }
    if (hasSpace) {
        for (; slashes > 0; slashes--) {
            b = append(b, (rune)'\\');
        }
        b = append(b, (rune)'"');
    }
    return b;
}

// makeCmdLine builds a command line out of args by escaping "special"
// characters and joining the arguments with spaces.
internal static @string makeCmdLine(slice<@string> args) {
    slice<byte> b = default!;
    foreach (var (_, v) in args) {
        if (len(b) > 0) {
            b = append(b, (rune)' ');
        }
        b = appendEscapeArg(b, v);
    }
    return ((@string)b);
}

// createEnvBlock converts an array of environment strings into
// the representation required by CreateProcess: a sequence of NUL
// terminated strings followed by a nil.
// Last bytes are two UCS-2 NULs, or four NUL bytes.
// If any string contains a NUL, it returns (nil, EINVAL).
internal static (slice<uint16>, error) createEnvBlock(slice<@string> envv) {
    if (len(envv) == 0) {
        return (utf16.Encode(slice<rune>((@string)"\x00\x00")), default!);
    }
    nint length = default!;
    foreach (var (_, s) in envv) {
        if (bytealg.IndexByteString(s, 0) != -1) {
            return (default!, EINVAL);
        }
        length += len(s) + 1;
    }
    length += 1;
    var b = new slice<uint16>(0, length);
    foreach (var (_, s) in envv) {
        foreach (var (_, c) in s) {
            b = utf16.AppendRune(b, c);
        }
        b = utf16.AppendRune(b, 0);
    }
    b = utf16.AppendRune(b, 0);
    return (b, default!);
}

public static void CloseOnExec(ΔHandle fd) {
    SetHandleInformation(((ΔHandle)fd), HANDLE_FLAG_INHERIT, 0);
}

public static error /*err*/ SetNonblock(ΔHandle fd, bool nonblocking) {
    error err = default!;

    return default!;
}

// FullPath retrieves the full path of the specified file.
public static (@string path, error err) FullPath(@string name) {
    @string path = default!;
    error err = default!;

    (p, err) = UTF16PtrFromString(name);
    if (err != default!) {
        return ("", err);
    }
    var n = ((uint32)100);
    while (ᐧ) {
        var buf = new slice<uint16>(n);
        (n, err) = GetFullPathName(p, ((uint32)len(buf)), Ꮡ(buf, 0), nil);
        if (err != default!) {
            return ("", err);
        }
        if (n <= ((uint32)len(buf))) {
            return (UTF16ToString(buf[..(int)(n)]), default!);
        }
    }
}

internal static bool isSlash(uint8 c) {
    return c == (rune)'\\' || c == (rune)'/';
}

internal static (@string name, error err) normalizeDir(@string dir) {
    @string name = default!;
    error err = default!;

    var (ndir, err) = FullPath(dir);
    if (err != default!) {
        return ("", err);
    }
    if (len(ndir) > 2 && isSlash(ndir[0]) && isSlash(ndir[1])) {
        // dir cannot have \\server\share\path form
        return ("", EINVAL);
    }
    return (ndir, default!);
}

internal static nint volToUpper(nint ch) {
    if ((rune)'a' <= ch && ch <= (rune)'z') {
        ch += (rune)'A' - (rune)'a';
    }
    return ch;
}

internal static (@string name, error err) joinExeDirAndFName(@string dir, @string p) {
    @string name = default!;
    error err = default!;

    if (len(p) == 0) {
        return ("", EINVAL);
    }
    if (len(p) > 2 && isSlash(p[0]) && isSlash(p[1])) {
        // \\server\share\path form
        return (p, default!);
    }
    if (len(p) > 1 && p[1] == (rune)':'){
        // has drive letter
        if (len(p) == 2) {
            return ("", EINVAL);
        }
        if (isSlash(p[2])){
            return (p, default!);
        } else {
            var (d, errΔ1) = normalizeDir(dir);
            if (errΔ1 != default!) {
                return ("", errΔ1);
            }
            if (volToUpper(((nint)p[0])) == volToUpper(((nint)d[0]))){
                return FullPath(d + "\\"u8 + p[2..]);
            } else {
                return FullPath(p);
            }
        }
    } else {
        // no drive letter
        var (d, errΔ2) = normalizeDir(dir);
        if (errΔ2 != default!) {
            return ("", errΔ2);
        }
        if (isSlash(p[0])){
            return FullPath(d[..2] + p);
        } else {
            return FullPath(d + "\\"u8 + p);
        }
    }
}

[GoType] partial struct ProcAttr {
    public @string Dir;
    public slice<@string> Env;
    public slice<uintptr> Files;
    public ж<SysProcAttr> Sys;
}

[GoType] partial struct SysProcAttr {
    public bool HideWindow;
    public @string CmdLine; // used if non-empty, else the windows command line is built by escaping the arguments passed to StartProcess
    public uint32 CreationFlags;
    public Token Token;               // if set, runs new process in the security context represented by the token
    public ж<SecurityAttributes> ProcessAttributes; // if set, applies these security attributes as the descriptor for the new process
    public ж<SecurityAttributes> ThreadAttributes; // if set, applies these security attributes as the descriptor for the main thread of the new process
    public bool NoInheritHandles;                // if set, no handles are inherited by the new process, not even the standard handles, contained in ProcAttr.Files, nor the ones contained in AdditionalInheritedHandles
    public slice<ΔHandle> AdditionalInheritedHandles;     // a list of additional handles, already marked as inheritable, that will be inherited by the new process
    public ΔHandle ParentProcess;            // if non-zero, the new process regards the process given by this handle as its parent process, and AdditionalInheritedHandles, if set, should exist in this parent process
}

internal static ProcAttr zeroProcAttr;

internal static SysProcAttr zeroSysProcAttr;

public static (nint pid, uintptr handle, error err) StartProcess(@string argv0, slice<@string> argv, ж<ProcAttr> Ꮡattr) => func((defer, _) => {
    nint pid = default!;
    uintptr handle = default!;
    error errΔ1 = default!;

    ref var attr = ref Ꮡattr.val;
    if (len(argv0) == 0) {
        return (0, 0, EWINDOWS);
    }
    if (attr == nil) {
        Ꮡattr = Ꮡ(zeroProcAttr); attr = ref Ꮡattr.val;
    }
    var sys = attr.Sys;
    if (sys == nil) {
        sys = Ꮡ(zeroSysProcAttr);
    }
    if (len(attr.Files) > 3) {
        return (0, 0, EWINDOWS);
    }
    if (len(attr.Files) < 3) {
        return (0, 0, EINVAL);
    }
    if (len(attr.Dir) != 0) {
        // StartProcess assumes that argv0 is relative to attr.Dir,
        // because it implies Chdir(attr.Dir) before executing argv0.
        // Windows CreateProcess assumes the opposite: it looks for
        // argv0 relative to the current directory, and, only once the new
        // process is started, it does Chdir(attr.Dir). We are adjusting
        // for that difference here by making argv0 absolute.
        error errΔ2 = default!;
        (argv0, errΔ2) = joinExeDirAndFName(attr.Dir, argv0);
        if (errΔ2 != default!) {
            return (0, 0, errΔ2);
        }
    }
    (argv0p, errΔ2) = UTF16PtrFromString(argv0);
    if (errΔ1 != default!) {
        return (0, 0, errΔ1);
    }
    @string cmdline = default!;
    // Windows CreateProcess takes the command line as a single string:
    // use attr.CmdLine if set, else build the command line by escaping
    // and joining each argument with spaces
    if ((~sys).CmdLine != ""u8){
        cmdline = sys.val.CmdLine;
    } else {
        cmdline = makeCmdLine(argv);
    }
    ж<uint16> argvp = default!;
    if (len(cmdline) != 0) {
        (argvp, errΔ2) = UTF16PtrFromString(cmdline);
        if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    ж<uint16> dirp = default!;
    if (len(attr.Dir) != 0) {
        (dirp, errΔ2) = UTF16PtrFromString(attr.Dir);
        if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    var (p, _) = GetCurrentProcess();
    var parentProcess = p;
    if ((~sys).ParentProcess != 0) {
        parentProcess = sys.val.ParentProcess;
    }
    var fd = new slice<ΔHandle>(len(attr.Files));
    ref var i = ref heap(new nint(), out var Ꮡi);

    foreach (var (i, _) in attr.Files) {
        if (attr.Files[i] > 0) {
            var errΔ3 = DuplicateHandle(p, ((ΔHandle)attr.Files[i]), parentProcess, Ꮡ(fd, i), 0, true, DUPLICATE_SAME_ACCESS);
            if (errΔ3 != default!) {
                return (0, 0, errΔ3);
            }
            deferǃ(DuplicateHandle, parentProcess, fd[i], 0, nil, 0, false, DUPLICATE_CLOSE_SOURCE, defer);
        }
    }
    var si = @new<_STARTUPINFOEXW>();
    (si.val.ProcThreadAttributeList, errΔ2) = newProcThreadAttributeList(2);
    if (errΔ1 != default!) {
        return (0, 0, errΔ1);
    }
    deferǃ(deleteProcThreadAttributeList, (~si).ProcThreadAttributeList, defer);
    si.Cb = ((uint32)@unsafe.Sizeof(si.val));
    si.Flags = STARTF_USESTDHANDLES;
    if ((~sys).HideWindow) {
        si.Flags |= (uint32)(STARTF_USESHOWWINDOW);
        si.ShowWindow = SW_HIDE;
    }
    if ((~sys).ParentProcess != 0) {
        errΔ2 = updateProcThreadAttribute((~si).ProcThreadAttributeList, 0, _PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, ((@unsafe.Pointer)(Ꮡ((~sys).ParentProcess))), @unsafe.Sizeof((~sys).ParentProcess), nil, nil);
        if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    si.StdInput = fd[0];
    si.StdOutput = fd[1];
    si.StdErr = fd[2];
    fd = append(fd, (~sys).AdditionalInheritedHandles.ꓸꓸꓸ);
    // The presence of a NULL handle in the list is enough to cause PROC_THREAD_ATTRIBUTE_HANDLE_LIST
    // to treat the entire list as empty, so remove NULL handles.
    nint j = 0;
    foreach (var (i, _) in fd) {
        if (fd[i] != 0) {
            fd[j] = fd[i];
            j++;
        }
    }
    fd = fd[..(int)(j)];
    var willInheritHandles = len(fd) > 0 && !(~sys).NoInheritHandles;
    // Do not accidentally inherit more than these handles.
    if (willInheritHandles) {
        errΔ2 = updateProcThreadAttribute((~si).ProcThreadAttributeList, 0, _PROC_THREAD_ATTRIBUTE_HANDLE_LIST, ((@unsafe.Pointer)(Ꮡ(fd, 0))), ((uintptr)len(fd)) * @unsafe.Sizeof(fd[0]), nil, nil);
        if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    (envBlock, errΔ2) = createEnvBlock(attr.Env);
    if (errΔ1 != default!) {
        return (0, 0, errΔ1);
    }
    var pi = @new<ProcessInformation>();
    var flags = (uint32)((uint32)((~sys).CreationFlags | CREATE_UNICODE_ENVIRONMENT) | _EXTENDED_STARTUPINFO_PRESENT);
    if ((~sys).Token != 0){
        errΔ2 = CreateProcessAsUser((~sys).Token, argv0p, argvp, (~sys).ProcessAttributes, (~sys).ThreadAttributes, willInheritHandles, flags, Ꮡ(envBlock, 0), dirp, Ꮡ((~si).StartupInfo), pi);
    } else {
        errΔ2 = CreateProcess(argv0p, argvp, (~sys).ProcessAttributes, (~sys).ThreadAttributes, willInheritHandles, flags, Ꮡ(envBlock, 0), dirp, Ꮡ((~si).StartupInfo), pi);
    }
    if (errΔ1 != default!) {
        return (0, 0, errΔ1);
    }
    deferǃ(CloseHandle, ((ΔHandle)(~pi).Thread), defer);
    runtime.KeepAlive(fd);
    runtime.KeepAlive(sys);
    return (((nint)(~pi).ProcessId), ((uintptr)(~pi).Process), default!);
});

public static error /*err*/ Exec(@string argv0, slice<@string> argv, slice<@string> envv) {
    error err = default!;

    return EWINDOWS;
}

} // end syscall_package
