// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows system calls.

// package syscall -- go2cs converted at 2022 March 06 22:27:17 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_windows.go
using errorspkg = go.errors_package;
using itoa = go.@internal.itoa_package;
using oserror = go.@internal.oserror_package;
using race = go.@internal.race_package;
using unsafeheader = go.@internal.unsafeheader_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class syscall_package {

public partial struct Handle { // : System.UIntPtr
}

public static readonly var InvalidHandle = ~Handle(0);

// StringToUTF16 returns the UTF-16 encoding of the UTF-8 string s,
// with a terminating NUL added. If s contains a NUL byte this
// function panics instead of returning an error.
//
// Deprecated: Use UTF16FromString instead.


// StringToUTF16 returns the UTF-16 encoding of the UTF-8 string s,
// with a terminating NUL added. If s contains a NUL byte this
// function panics instead of returning an error.
//
// Deprecated: Use UTF16FromString instead.
public static slice<ushort> StringToUTF16(@string s) => func((_, panic, _) => {
    var (a, err) = UTF16FromString(s);
    if (err != null) {
        panic("syscall: string with NUL passed to StringToUTF16");
    }
    return a;

});

// UTF16FromString returns the UTF-16 encoding of the UTF-8 string
// s, with a terminating NUL added. If s contains a NUL byte at any
// location, it returns (nil, EINVAL).
public static (slice<ushort>, error) UTF16FromString(@string s) {
    slice<ushort> _p0 = default;
    error _p0 = default!;

    for (nint i = 0; i < len(s); i++) {
        if (s[i] == 0) {
            return (null, error.As(EINVAL)!);
        }
    }
    return (utf16.Encode((slice<int>)s + "\x00"), error.As(null!)!);

}

// UTF16ToString returns the UTF-8 encoding of the UTF-16 sequence s,
// with a terminating NUL removed.
public static @string UTF16ToString(slice<ushort> s) {
    foreach (var (i, v) in s) {
        if (v == 0) {
            s = s[(int)0..(int)i];
            break;
        }
    }    return string(utf16.Decode(s));

}

// utf16PtrToString is like UTF16ToString, but takes *uint16
// as a parameter instead of []uint16.
private static @string utf16PtrToString(ptr<ushort> _addr_p) {
    ref ushort p = ref _addr_p.val;

    if (p == null) {
        return "";
    }
    var end = @unsafe.Pointer(p);
    nint n = 0;
    while (new ptr<ptr<ptr<ushort>>>(end) != 0) {
        end = @unsafe.Pointer(uintptr(end) + @unsafe.Sizeof(p));
        n++;
    } 
    // Turn *uint16 into []uint16.
    ref slice<ushort> s = ref heap(out ptr<slice<ushort>> _addr_s);
    var hdr = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_s));
    hdr.Data = @unsafe.Pointer(p);
    hdr.Cap = n;
    hdr.Len = n; 
    // Decode []uint16 into string.
    return string(utf16.Decode(s));

}

// StringToUTF16Ptr returns pointer to the UTF-16 encoding of
// the UTF-8 string s, with a terminating NUL added. If s
// contains a NUL byte this function panics instead of
// returning an error.
//
// Deprecated: Use UTF16PtrFromString instead.
public static ptr<ushort> StringToUTF16Ptr(@string s) {
    return _addr__addr_StringToUTF16(s)[0]!;
}

// UTF16PtrFromString returns pointer to the UTF-16 encoding of
// the UTF-8 string s, with a terminating NUL added. If s
// contains a NUL byte at any location, it returns (nil, EINVAL).
public static (ptr<ushort>, error) UTF16PtrFromString(@string s) {
    ptr<ushort> _p0 = default!;
    error _p0 = default!;

    var (a, err) = UTF16FromString(s);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr__addr_a[0]!, error.As(null!)!);

}

// Errno is the Windows error number.
//
// Errno values can be tested against error values from the os package
// using errors.Is. For example:
//
//    _, _, err := syscall.Syscall(...)
//    if errors.Is(err, fs.ErrNotExist) ...
public partial struct Errno { // : System.UIntPtr
}

private static uint langid(ushort pri, ushort sub) {
    return uint32(sub) << 10 | uint32(pri);
}

// FormatMessage is deprecated (msgsrc should be uintptr, not uint32, but can
// not be changed due to the Go 1 compatibility guarantee).
//
// Deprecated: Use FormatMessage from golang.org/x/sys/windows instead.
public static (uint, error) FormatMessage(uint flags, uint msgsrc, uint msgid, uint langid, slice<ushort> buf, ptr<byte> _addr_args) {
    uint n = default;
    error err = default!;
    ref byte args = ref _addr_args.val;

    return formatMessage(flags, uintptr(msgsrc), msgid, langid, buf, args);
}

public static @string Error(this Errno e) { 
    // deal with special go errors
    var idx = int(e - APPLICATION_ERROR);
    if (0 <= idx && idx < len(errors)) {
        return errors[idx];
    }
    uint flags = FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ARGUMENT_ARRAY | FORMAT_MESSAGE_IGNORE_INSERTS;
    var b = make_slice<ushort>(300);
    var (n, err) = formatMessage(flags, 0, uint32(e), langid(LANG_ENGLISH, SUBLANG_ENGLISH_US), b, null);
    if (err != null) {
        n, err = formatMessage(flags, 0, uint32(e), 0, b, null);
        if (err != null) {
            return "winapi error #" + itoa.Itoa(int(e));
        }
    }
    while (n > 0 && (b[n - 1] == '\n' || b[n - 1] == '\r')) {
        n--;
    }
    return string(utf16.Decode(b[..(int)n]));

}

private static readonly var _ERROR_BAD_NETPATH = Errno(53);



public static bool Is(this Errno e, error target) {

    if (target == oserror.ErrPermission) 
        return e == ERROR_ACCESS_DENIED;
    else if (target == oserror.ErrExist) 
        return e == ERROR_ALREADY_EXISTS || e == ERROR_DIR_NOT_EMPTY || e == ERROR_FILE_EXISTS;
    else if (target == oserror.ErrNotExist) 
        return e == ERROR_FILE_NOT_FOUND || e == _ERROR_BAD_NETPATH || e == ERROR_PATH_NOT_FOUND;
        return false;

}

public static bool Temporary(this Errno e) {
    return e == EINTR || e == EMFILE || e.Timeout();
}

public static bool Timeout(this Errno e) {
    return e == EAGAIN || e == EWOULDBLOCK || e == ETIMEDOUT;
}

// Implemented in runtime/syscall_windows.go.
private static System.UIntPtr compileCallback(object fn, bool cleanstack);

// NewCallback converts a Go function to a function pointer conforming to the stdcall calling convention.
// This is useful when interoperating with Windows code requiring callbacks.
// The argument is expected to be a function with one uintptr-sized result. The function must not have arguments with size larger than the size of uintptr.
// Only a limited number of callbacks may be created in a single Go process, and any memory allocated
// for these callbacks is never released.
// Between NewCallback and NewCallbackCDecl, at least 1024 callbacks can always be created.
public static System.UIntPtr NewCallback(object fn) {
    return compileCallback(fn, true);
}

// NewCallbackCDecl converts a Go function to a function pointer conforming to the cdecl calling convention.
// This is useful when interoperating with Windows code requiring callbacks.
// The argument is expected to be a function with one uintptr-sized result. The function must not have arguments with size larger than the size of uintptr.
// Only a limited number of callbacks may be created in a single Go process, and any memory allocated
// for these callbacks is never released.
// Between NewCallback and NewCallbackCDecl, at least 1024 callbacks can always be created.
public static System.UIntPtr NewCallbackCDecl(object fn) {
    return compileCallback(fn, false);
}

// windows api calls

//sys    GetLastError() (lasterr error)
//sys    LoadLibrary(libname string) (handle Handle, err error) = LoadLibraryW
//sys    FreeLibrary(handle Handle) (err error)
//sys    GetProcAddress(module Handle, procname string) (proc uintptr, err error)
//sys    GetVersion() (ver uint32, err error)
//sys    rtlGetNtVersionNumbers(majorVersion *uint32, minorVersion *uint32, buildNumber *uint32) = ntdll.RtlGetNtVersionNumbers
//sys    formatMessage(flags uint32, msgsrc uintptr, msgid uint32, langid uint32, buf []uint16, args *byte) (n uint32, err error) = FormatMessageW
//sys    ExitProcess(exitcode uint32)
//sys    CreateFile(name *uint16, access uint32, mode uint32, sa *SecurityAttributes, createmode uint32, attrs uint32, templatefile int32) (handle Handle, err error) [failretval==InvalidHandle] = CreateFileW
//sys    ReadFile(handle Handle, buf []byte, done *uint32, overlapped *Overlapped) (err error)
//sys    WriteFile(handle Handle, buf []byte, done *uint32, overlapped *Overlapped) (err error)
//sys    SetFilePointer(handle Handle, lowoffset int32, highoffsetptr *int32, whence uint32) (newlowoffset uint32, err error) [failretval==0xffffffff]
//sys    CloseHandle(handle Handle) (err error)
//sys    GetStdHandle(stdhandle int) (handle Handle, err error) [failretval==InvalidHandle]
//sys    findFirstFile1(name *uint16, data *win32finddata1) (handle Handle, err error) [failretval==InvalidHandle] = FindFirstFileW
//sys    findNextFile1(handle Handle, data *win32finddata1) (err error) = FindNextFileW
//sys    FindClose(handle Handle) (err error)
//sys    GetFileInformationByHandle(handle Handle, data *ByHandleFileInformation) (err error)
//sys    GetCurrentDirectory(buflen uint32, buf *uint16) (n uint32, err error) = GetCurrentDirectoryW
//sys    SetCurrentDirectory(path *uint16) (err error) = SetCurrentDirectoryW
//sys    CreateDirectory(path *uint16, sa *SecurityAttributes) (err error) = CreateDirectoryW
//sys    RemoveDirectory(path *uint16) (err error) = RemoveDirectoryW
//sys    DeleteFile(path *uint16) (err error) = DeleteFileW
//sys    MoveFile(from *uint16, to *uint16) (err error) = MoveFileW
//sys    GetComputerName(buf *uint16, n *uint32) (err error) = GetComputerNameW
//sys    SetEndOfFile(handle Handle) (err error)
//sys    GetSystemTimeAsFileTime(time *Filetime)
//sys    GetTimeZoneInformation(tzi *Timezoneinformation) (rc uint32, err error) [failretval==0xffffffff]
//sys    createIoCompletionPort(filehandle Handle, cphandle Handle, key uintptr, threadcnt uint32) (handle Handle, err error) = CreateIoCompletionPort
//sys    getQueuedCompletionStatus(cphandle Handle, qty *uint32, key *uintptr, overlapped **Overlapped, timeout uint32) (err error) = GetQueuedCompletionStatus
//sys    postQueuedCompletionStatus(cphandle Handle, qty uint32, key uintptr, overlapped *Overlapped) (err error) = PostQueuedCompletionStatus
//sys    CancelIo(s Handle) (err error)
//sys    CancelIoEx(s Handle, o *Overlapped) (err error)
//sys    CreateProcess(appName *uint16, commandLine *uint16, procSecurity *SecurityAttributes, threadSecurity *SecurityAttributes, inheritHandles bool, creationFlags uint32, env *uint16, currentDir *uint16, startupInfo *StartupInfo, outProcInfo *ProcessInformation) (err error) = CreateProcessW
//sys    CreateProcessAsUser(token Token, appName *uint16, commandLine *uint16, procSecurity *SecurityAttributes, threadSecurity *SecurityAttributes, inheritHandles bool, creationFlags uint32, env *uint16, currentDir *uint16, startupInfo *StartupInfo, outProcInfo *ProcessInformation) (err error) = advapi32.CreateProcessAsUserW
//sys    OpenProcess(da uint32, inheritHandle bool, pid uint32) (handle Handle, err error)
//sys    TerminateProcess(handle Handle, exitcode uint32) (err error)
//sys    GetExitCodeProcess(handle Handle, exitcode *uint32) (err error)
//sys    GetStartupInfo(startupInfo *StartupInfo) (err error) = GetStartupInfoW
//sys    GetCurrentProcess() (pseudoHandle Handle, err error)
//sys    GetProcessTimes(handle Handle, creationTime *Filetime, exitTime *Filetime, kernelTime *Filetime, userTime *Filetime) (err error)
//sys    DuplicateHandle(hSourceProcessHandle Handle, hSourceHandle Handle, hTargetProcessHandle Handle, lpTargetHandle *Handle, dwDesiredAccess uint32, bInheritHandle bool, dwOptions uint32) (err error)
//sys    WaitForSingleObject(handle Handle, waitMilliseconds uint32) (event uint32, err error) [failretval==0xffffffff]
//sys    GetTempPath(buflen uint32, buf *uint16) (n uint32, err error) = GetTempPathW
//sys    CreatePipe(readhandle *Handle, writehandle *Handle, sa *SecurityAttributes, size uint32) (err error)
//sys    GetFileType(filehandle Handle) (n uint32, err error)
//sys    CryptAcquireContext(provhandle *Handle, container *uint16, provider *uint16, provtype uint32, flags uint32) (err error) = advapi32.CryptAcquireContextW
//sys    CryptReleaseContext(provhandle Handle, flags uint32) (err error) = advapi32.CryptReleaseContext
//sys    CryptGenRandom(provhandle Handle, buflen uint32, buf *byte) (err error) = advapi32.CryptGenRandom
//sys    GetEnvironmentStrings() (envs *uint16, err error) [failretval==nil] = kernel32.GetEnvironmentStringsW
//sys    FreeEnvironmentStrings(envs *uint16) (err error) = kernel32.FreeEnvironmentStringsW
//sys    GetEnvironmentVariable(name *uint16, buffer *uint16, size uint32) (n uint32, err error) = kernel32.GetEnvironmentVariableW
//sys    SetEnvironmentVariable(name *uint16, value *uint16) (err error) = kernel32.SetEnvironmentVariableW
//sys    SetFileTime(handle Handle, ctime *Filetime, atime *Filetime, wtime *Filetime) (err error)
//sys    GetFileAttributes(name *uint16) (attrs uint32, err error) [failretval==INVALID_FILE_ATTRIBUTES] = kernel32.GetFileAttributesW
//sys    SetFileAttributes(name *uint16, attrs uint32) (err error) = kernel32.SetFileAttributesW
//sys    GetFileAttributesEx(name *uint16, level uint32, info *byte) (err error) = kernel32.GetFileAttributesExW
//sys    GetCommandLine() (cmd *uint16) = kernel32.GetCommandLineW
//sys    CommandLineToArgv(cmd *uint16, argc *int32) (argv *[8192]*[8192]uint16, err error) [failretval==nil] = shell32.CommandLineToArgvW
//sys    LocalFree(hmem Handle) (handle Handle, err error) [failretval!=0]
//sys    SetHandleInformation(handle Handle, mask uint32, flags uint32) (err error)
//sys    FlushFileBuffers(handle Handle) (err error)
//sys    GetFullPathName(path *uint16, buflen uint32, buf *uint16, fname **uint16) (n uint32, err error) = kernel32.GetFullPathNameW
//sys    GetLongPathName(path *uint16, buf *uint16, buflen uint32) (n uint32, err error) = kernel32.GetLongPathNameW
//sys    GetShortPathName(longpath *uint16, shortpath *uint16, buflen uint32) (n uint32, err error) = kernel32.GetShortPathNameW
//sys    CreateFileMapping(fhandle Handle, sa *SecurityAttributes, prot uint32, maxSizeHigh uint32, maxSizeLow uint32, name *uint16) (handle Handle, err error) = kernel32.CreateFileMappingW
//sys    MapViewOfFile(handle Handle, access uint32, offsetHigh uint32, offsetLow uint32, length uintptr) (addr uintptr, err error)
//sys    UnmapViewOfFile(addr uintptr) (err error)
//sys    FlushViewOfFile(addr uintptr, length uintptr) (err error)
//sys    VirtualLock(addr uintptr, length uintptr) (err error)
//sys    VirtualUnlock(addr uintptr, length uintptr) (err error)
//sys    TransmitFile(s Handle, handle Handle, bytesToWrite uint32, bytsPerSend uint32, overlapped *Overlapped, transmitFileBuf *TransmitFileBuffers, flags uint32) (err error) = mswsock.TransmitFile
//sys    ReadDirectoryChanges(handle Handle, buf *byte, buflen uint32, watchSubTree bool, mask uint32, retlen *uint32, overlapped *Overlapped, completionRoutine uintptr) (err error) = kernel32.ReadDirectoryChangesW
//sys    CertOpenSystemStore(hprov Handle, name *uint16) (store Handle, err error) = crypt32.CertOpenSystemStoreW
//sys   CertOpenStore(storeProvider uintptr, msgAndCertEncodingType uint32, cryptProv uintptr, flags uint32, para uintptr) (handle Handle, err error) = crypt32.CertOpenStore
//sys    CertEnumCertificatesInStore(store Handle, prevContext *CertContext) (context *CertContext, err error) [failretval==nil] = crypt32.CertEnumCertificatesInStore
//sys   CertAddCertificateContextToStore(store Handle, certContext *CertContext, addDisposition uint32, storeContext **CertContext) (err error) = crypt32.CertAddCertificateContextToStore
//sys    CertCloseStore(store Handle, flags uint32) (err error) = crypt32.CertCloseStore
//sys   CertGetCertificateChain(engine Handle, leaf *CertContext, time *Filetime, additionalStore Handle, para *CertChainPara, flags uint32, reserved uintptr, chainCtx **CertChainContext) (err error) = crypt32.CertGetCertificateChain
//sys   CertFreeCertificateChain(ctx *CertChainContext) = crypt32.CertFreeCertificateChain
//sys   CertCreateCertificateContext(certEncodingType uint32, certEncoded *byte, encodedLen uint32) (context *CertContext, err error) [failretval==nil] = crypt32.CertCreateCertificateContext
//sys   CertFreeCertificateContext(ctx *CertContext) (err error) = crypt32.CertFreeCertificateContext
//sys   CertVerifyCertificateChainPolicy(policyOID uintptr, chain *CertChainContext, para *CertChainPolicyPara, status *CertChainPolicyStatus) (err error) = crypt32.CertVerifyCertificateChainPolicy
//sys    RegOpenKeyEx(key Handle, subkey *uint16, options uint32, desiredAccess uint32, result *Handle) (regerrno error) = advapi32.RegOpenKeyExW
//sys    RegCloseKey(key Handle) (regerrno error) = advapi32.RegCloseKey
//sys    RegQueryInfoKey(key Handle, class *uint16, classLen *uint32, reserved *uint32, subkeysLen *uint32, maxSubkeyLen *uint32, maxClassLen *uint32, valuesLen *uint32, maxValueNameLen *uint32, maxValueLen *uint32, saLen *uint32, lastWriteTime *Filetime) (regerrno error) = advapi32.RegQueryInfoKeyW
//sys    RegEnumKeyEx(key Handle, index uint32, name *uint16, nameLen *uint32, reserved *uint32, class *uint16, classLen *uint32, lastWriteTime *Filetime) (regerrno error) = advapi32.RegEnumKeyExW
//sys    RegQueryValueEx(key Handle, name *uint16, reserved *uint32, valtype *uint32, buf *byte, buflen *uint32) (regerrno error) = advapi32.RegQueryValueExW
//sys    getCurrentProcessId() (pid uint32) = kernel32.GetCurrentProcessId
//sys    GetConsoleMode(console Handle, mode *uint32) (err error) = kernel32.GetConsoleMode
//sys    WriteConsole(console Handle, buf *uint16, towrite uint32, written *uint32, reserved *byte) (err error) = kernel32.WriteConsoleW
//sys    ReadConsole(console Handle, buf *uint16, toread uint32, read *uint32, inputControl *byte) (err error) = kernel32.ReadConsoleW
//sys    CreateToolhelp32Snapshot(flags uint32, processId uint32) (handle Handle, err error) [failretval==InvalidHandle] = kernel32.CreateToolhelp32Snapshot
//sys    Process32First(snapshot Handle, procEntry *ProcessEntry32) (err error) = kernel32.Process32FirstW
//sys    Process32Next(snapshot Handle, procEntry *ProcessEntry32) (err error) = kernel32.Process32NextW
//sys    DeviceIoControl(handle Handle, ioControlCode uint32, inBuffer *byte, inBufferSize uint32, outBuffer *byte, outBufferSize uint32, bytesReturned *uint32, overlapped *Overlapped) (err error)
// This function returns 1 byte BOOLEAN rather than the 4 byte BOOL.
//sys    CreateSymbolicLink(symlinkfilename *uint16, targetfilename *uint16, flags uint32) (err error) [failretval&0xff==0] = CreateSymbolicLinkW
//sys    CreateHardLink(filename *uint16, existingfilename *uint16, reserved uintptr) (err error) [failretval&0xff==0] = CreateHardLinkW
//sys    initializeProcThreadAttributeList(attrlist *_PROC_THREAD_ATTRIBUTE_LIST, attrcount uint32, flags uint32, size *uintptr) (err error) = InitializeProcThreadAttributeList
//sys    deleteProcThreadAttributeList(attrlist *_PROC_THREAD_ATTRIBUTE_LIST) = DeleteProcThreadAttributeList
//sys    updateProcThreadAttribute(attrlist *_PROC_THREAD_ATTRIBUTE_LIST, flags uint32, attr uintptr, value unsafe.Pointer, size uintptr, prevvalue unsafe.Pointer, returnedsize *uintptr) (err error) = UpdateProcThreadAttribute

// syscall interface implementation for other packages

private static ptr<SecurityAttributes> makeInheritSa() {
    ref SecurityAttributes sa = ref heap(out ptr<SecurityAttributes> _addr_sa);
    sa.Length = uint32(@unsafe.Sizeof(sa));
    sa.InheritHandle = 1;
    return _addr__addr_sa!;
}

public static (Handle, error) Open(@string path, nint mode, uint perm) {
    Handle fd = default;
    error err = default!;

    if (len(path) == 0) {>>MARKER:FUNCTION_compileCallback_BLOCK_PREFIX<<
        return (InvalidHandle, error.As(ERROR_FILE_NOT_FOUND)!);
    }
    var (pathp, err) = UTF16PtrFromString(path);
    if (err != null) {
        return (InvalidHandle, error.As(err)!);
    }
    uint access = default;

    if (mode & (O_RDONLY | O_WRONLY | O_RDWR) == O_RDONLY) 
        access = GENERIC_READ;
    else if (mode & (O_RDONLY | O_WRONLY | O_RDWR) == O_WRONLY) 
        access = GENERIC_WRITE;
    else if (mode & (O_RDONLY | O_WRONLY | O_RDWR) == O_RDWR) 
        access = GENERIC_READ | GENERIC_WRITE;
        if (mode & O_CREAT != 0) {
        access |= GENERIC_WRITE;
    }
    if (mode & O_APPEND != 0) {
        access &= GENERIC_WRITE;
        access |= FILE_APPEND_DATA;
    }
    var sharemode = uint32(FILE_SHARE_READ | FILE_SHARE_WRITE);
    ptr<SecurityAttributes> sa;
    if (mode & O_CLOEXEC == 0) {
        sa = makeInheritSa();
    }
    uint createmode = default;

    if (mode & (O_CREAT | O_EXCL) == (O_CREAT | O_EXCL)) 
        createmode = CREATE_NEW;
    else if (mode & (O_CREAT | O_TRUNC) == (O_CREAT | O_TRUNC)) 
        createmode = CREATE_ALWAYS;
    else if (mode & O_CREAT == O_CREAT) 
        createmode = OPEN_ALWAYS;
    else if (mode & O_TRUNC == O_TRUNC) 
        createmode = TRUNCATE_EXISTING;
    else 
        createmode = OPEN_EXISTING;
        uint attrs = FILE_ATTRIBUTE_NORMAL;
    if (perm & S_IWRITE == 0) {
        attrs = FILE_ATTRIBUTE_READONLY;
        if (createmode == CREATE_ALWAYS) { 
            // We have been asked to create a read-only file.
            // If the file already exists, the semantics of
            // the Unix open system call is to preserve the
            // existing permissions. If we pass CREATE_ALWAYS
            // and FILE_ATTRIBUTE_READONLY to CreateFile,
            // and the file already exists, CreateFile will
            // change the file permissions.
            // Avoid that to preserve the Unix semantics.
            var (h, e) = CreateFile(pathp, access, sharemode, sa, TRUNCATE_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);

            if (e == ERROR_FILE_NOT_FOUND || e == _ERROR_BAD_NETPATH || e == ERROR_PATH_NOT_FOUND)             else 
                // Success or some different error.
                return (h, error.As(e)!);
            
        }
    }
    (h, e) = CreateFile(pathp, access, sharemode, sa, createmode, attrs, 0);
    return (h, error.As(e)!);

}

public static (nint, error) Read(Handle fd, slice<byte> p) {
    nint n = default;
    error err = default!;

    ref uint done = ref heap(out ptr<uint> _addr_done);
    var e = ReadFile(fd, p, _addr_done, null);
    if (e != null) {
        if (e == ERROR_BROKEN_PIPE) { 
            // NOTE(brainman): work around ERROR_BROKEN_PIPE is returned on reading EOF from stdin
            return (0, error.As(null!)!);

        }
        return (0, error.As(e)!);

    }
    if (race.Enabled) {
        if (done > 0) {
            race.WriteRange(@unsafe.Pointer(_addr_p[0]), int(done));
        }
        race.Acquire(@unsafe.Pointer(_addr_ioSync));

    }
    if (msanenabled && done > 0) {
        msanWrite(@unsafe.Pointer(_addr_p[0]), int(done));
    }
    return (int(done), error.As(null!)!);

}

public static (nint, error) Write(Handle fd, slice<byte> p) {
    nint n = default;
    error err = default!;

    if (race.Enabled) {
        race.ReleaseMerge(@unsafe.Pointer(_addr_ioSync));
    }
    ref uint done = ref heap(out ptr<uint> _addr_done);
    var e = WriteFile(fd, p, _addr_done, null);
    if (e != null) {
        return (0, error.As(e)!);
    }
    if (race.Enabled && done > 0) {
        race.ReadRange(@unsafe.Pointer(_addr_p[0]), int(done));
    }
    if (msanenabled && done > 0) {
        msanRead(@unsafe.Pointer(_addr_p[0]), int(done));
    }
    return (int(done), error.As(null!)!);

}

private static long ioSync = default;

private static var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx");

private static readonly var ptrSize = @unsafe.Sizeof(uintptr(0));

// setFilePointerEx calls SetFilePointerEx.
// See https://msdn.microsoft.com/en-us/library/windows/desktop/aa365542(v=vs.85).aspx


// setFilePointerEx calls SetFilePointerEx.
// See https://msdn.microsoft.com/en-us/library/windows/desktop/aa365542(v=vs.85).aspx
private static error setFilePointerEx(Handle handle, long distToMove, ptr<long> _addr_newFilePointer, uint whence) => func((_, panic, _) => {
    ref long newFilePointer = ref _addr_newFilePointer.val;

    Errno e1 = default;
    if (@unsafe.Sizeof(uintptr(0)) == 8) {
        _, _, e1 = Syscall6(procSetFilePointerEx.Addr(), 4, uintptr(handle), uintptr(distToMove), uintptr(@unsafe.Pointer(newFilePointer)), uintptr(whence), 0, 0);
    }
    else
 { 
        // Different 32-bit systems disgaree about whether distToMove starts 8-byte aligned.
        switch (runtime.GOARCH) {
            case "386": 
                // distToMove is a LARGE_INTEGER:
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa383713(v=vs.85).aspx
                _, _, e1 = Syscall6(procSetFilePointerEx.Addr(), 5, uintptr(handle), uintptr(distToMove), uintptr(distToMove >> 32), uintptr(@unsafe.Pointer(newFilePointer)), uintptr(whence), 0);
                break;
            case "arm": 
                // distToMove must be 8-byte aligned per ARM calling convention
                // https://msdn.microsoft.com/en-us/library/dn736986.aspx#Anchor_7
                _, _, e1 = Syscall6(procSetFilePointerEx.Addr(), 6, uintptr(handle), 0, uintptr(distToMove), uintptr(distToMove >> 32), uintptr(@unsafe.Pointer(newFilePointer)), uintptr(whence));
                break;
            default: 
                panic("unsupported 32-bit architecture");
                break;
        }

    }
    if (e1 != 0) {
        return error.As(errnoErr(e1))!;
    }
    return error.As(null!)!;

});

public static (long, error) Seek(Handle fd, long offset, nint whence) {
    long newoffset = default;
    error err = default!;

    uint w = default;
    switch (whence) {
        case 0: 
            w = FILE_BEGIN;
            break;
        case 1: 
            w = FILE_CURRENT;
            break;
        case 2: 
            w = FILE_END;
            break;
    } 
    // use GetFileType to check pipe, pipe can't do seek
    var (ft, _) = GetFileType(fd);
    if (ft == FILE_TYPE_PIPE) {
        return (0, error.As(ESPIPE)!);
    }
    err = setFilePointerEx(fd, offset, _addr_newoffset, w);
    return ;

}

public static error Close(Handle fd) {
    error err = default!;

    return error.As(CloseHandle(fd))!;
}

public static var Stdin = getStdHandle(STD_INPUT_HANDLE);public static var Stdout = getStdHandle(STD_OUTPUT_HANDLE);public static var Stderr = getStdHandle(STD_ERROR_HANDLE);

private static Handle getStdHandle(nint h) {
    Handle fd = default;

    var (r, _) = GetStdHandle(h);
    return r;
}

public static readonly var ImplementsGetwd = true;



public static (@string, error) Getwd() {
    @string wd = default;
    error err = default!;

    var b = make_slice<ushort>(300);
    var (n, e) = GetCurrentDirectory(uint32(len(b)), _addr_b[0]);
    if (e != null) {
        return ("", error.As(e)!);
    }
    return (string(utf16.Decode(b[(int)0..(int)n])), error.As(null!)!);

}

public static error Chdir(@string path) {
    error err = default!;

    var (pathp, err) = UTF16PtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(SetCurrentDirectory(pathp))!;

}

public static error Mkdir(@string path, uint mode) {
    error err = default!;

    var (pathp, err) = UTF16PtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(CreateDirectory(pathp, null))!;

}

public static error Rmdir(@string path) {
    error err = default!;

    var (pathp, err) = UTF16PtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(RemoveDirectory(pathp))!;

}

public static error Unlink(@string path) {
    error err = default!;

    var (pathp, err) = UTF16PtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(DeleteFile(pathp))!;

}

public static error Rename(@string oldpath, @string newpath) {
    error err = default!;

    var (from, err) = UTF16PtrFromString(oldpath);
    if (err != null) {
        return error.As(err)!;
    }
    var (to, err) = UTF16PtrFromString(newpath);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(MoveFile(from, to))!;

}

public static (@string, error) ComputerName() {
    @string name = default;
    error err = default!;

    ref uint n = ref heap(MAX_COMPUTERNAME_LENGTH + 1, out ptr<uint> _addr_n);
    var b = make_slice<ushort>(n);
    var e = GetComputerName(_addr_b[0], _addr_n);
    if (e != null) {
        return ("", error.As(e)!);
    }
    return (string(utf16.Decode(b[(int)0..(int)n])), error.As(null!)!);

}

public static error Ftruncate(Handle fd, long length) => func((defer, _, _) => {
    error err = default!;

    var (curoffset, e) = Seek(fd, 0, 1);
    if (e != null) {
        return error.As(e)!;
    }
    defer(Seek(fd, curoffset, 0));
    _, e = Seek(fd, length, 0);
    if (e != null) {
        return error.As(e)!;
    }
    e = SetEndOfFile(fd);
    if (e != null) {
        return error.As(e)!;
    }
    return error.As(null!)!;

});

public static error Gettimeofday(ptr<Timeval> _addr_tv) {
    error err = default!;
    ref Timeval tv = ref _addr_tv.val;

    ref Filetime ft = ref heap(out ptr<Filetime> _addr_ft);
    GetSystemTimeAsFileTime(_addr_ft);
    tv = NsecToTimeval(ft.Nanoseconds());
    return error.As(null!)!;
}

public static error Pipe(slice<Handle> p) {
    error err = default!;

    if (len(p) != 2) {
        return error.As(EINVAL)!;
    }
    ref Handle r = ref heap(out ptr<Handle> _addr_r);    ref Handle w = ref heap(out ptr<Handle> _addr_w);

    var e = CreatePipe(_addr_r, _addr_w, makeInheritSa(), 0);
    if (e != null) {
        return error.As(e)!;
    }
    p[0] = r;
    p[1] = w;
    return error.As(null!)!;

}

public static error Utimes(@string path, slice<Timeval> tv) => func((defer, _, _) => {
    error err = default!;

    if (len(tv) != 2) {
        return error.As(EINVAL)!;
    }
    var (pathp, e) = UTF16PtrFromString(path);
    if (e != null) {
        return error.As(e)!;
    }
    var (h, e) = CreateFile(pathp, FILE_WRITE_ATTRIBUTES, FILE_SHARE_WRITE, null, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, 0);
    if (e != null) {
        return error.As(e)!;
    }
    defer(Close(h));
    ref var a = ref heap(NsecToFiletime(tv[0].Nanoseconds()), out ptr<var> _addr_a);
    ref var w = ref heap(NsecToFiletime(tv[1].Nanoseconds()), out ptr<var> _addr_w);
    return error.As(SetFileTime(h, null, _addr_a, _addr_w))!;

});

public static error UtimesNano(@string path, slice<Timespec> ts) => func((defer, _, _) => {
    error err = default!;

    if (len(ts) != 2) {
        return error.As(EINVAL)!;
    }
    var (pathp, e) = UTF16PtrFromString(path);
    if (e != null) {
        return error.As(e)!;
    }
    var (h, e) = CreateFile(pathp, FILE_WRITE_ATTRIBUTES, FILE_SHARE_WRITE, null, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, 0);
    if (e != null) {
        return error.As(e)!;
    }
    defer(Close(h));
    ref var a = ref heap(NsecToFiletime(TimespecToNsec(ts[0])), out ptr<var> _addr_a);
    ref var w = ref heap(NsecToFiletime(TimespecToNsec(ts[1])), out ptr<var> _addr_w);
    return error.As(SetFileTime(h, null, _addr_a, _addr_w))!;

});

public static error Fsync(Handle fd) {
    error err = default!;

    return error.As(FlushFileBuffers(fd))!;
}

public static error Chmod(@string path, uint mode) {
    error err = default!;

    var (p, e) = UTF16PtrFromString(path);
    if (e != null) {
        return error.As(e)!;
    }
    var (attrs, e) = GetFileAttributes(p);
    if (e != null) {
        return error.As(e)!;
    }
    if (mode & S_IWRITE != 0) {
        attrs &= FILE_ATTRIBUTE_READONLY;
    }
    else
 {
        attrs |= FILE_ATTRIBUTE_READONLY;
    }
    return error.As(SetFileAttributes(p, attrs))!;

}

public static error LoadCancelIoEx() {
    return error.As(procCancelIoEx.Find())!;
}

public static error LoadSetFileCompletionNotificationModes() {
    return error.As(procSetFileCompletionNotificationModes.Find())!;
}

// net api calls

private static readonly var socket_error = uintptr(~uint32(0));

//sys    WSAStartup(verreq uint32, data *WSAData) (sockerr error) = ws2_32.WSAStartup
//sys    WSACleanup() (err error) [failretval==socket_error] = ws2_32.WSACleanup
//sys    WSAIoctl(s Handle, iocc uint32, inbuf *byte, cbif uint32, outbuf *byte, cbob uint32, cbbr *uint32, overlapped *Overlapped, completionRoutine uintptr) (err error) [failretval==socket_error] = ws2_32.WSAIoctl
//sys    socket(af int32, typ int32, protocol int32) (handle Handle, err error) [failretval==InvalidHandle] = ws2_32.socket
//sys    Setsockopt(s Handle, level int32, optname int32, optval *byte, optlen int32) (err error) [failretval==socket_error] = ws2_32.setsockopt
//sys    Getsockopt(s Handle, level int32, optname int32, optval *byte, optlen *int32) (err error) [failretval==socket_error] = ws2_32.getsockopt
//sys    bind(s Handle, name unsafe.Pointer, namelen int32) (err error) [failretval==socket_error] = ws2_32.bind
//sys    connect(s Handle, name unsafe.Pointer, namelen int32) (err error) [failretval==socket_error] = ws2_32.connect
//sys    getsockname(s Handle, rsa *RawSockaddrAny, addrlen *int32) (err error) [failretval==socket_error] = ws2_32.getsockname
//sys    getpeername(s Handle, rsa *RawSockaddrAny, addrlen *int32) (err error) [failretval==socket_error] = ws2_32.getpeername
//sys    listen(s Handle, backlog int32) (err error) [failretval==socket_error] = ws2_32.listen
//sys    shutdown(s Handle, how int32) (err error) [failretval==socket_error] = ws2_32.shutdown
//sys    Closesocket(s Handle) (err error) [failretval==socket_error] = ws2_32.closesocket
//sys    AcceptEx(ls Handle, as Handle, buf *byte, rxdatalen uint32, laddrlen uint32, raddrlen uint32, recvd *uint32, overlapped *Overlapped) (err error) = mswsock.AcceptEx
//sys    GetAcceptExSockaddrs(buf *byte, rxdatalen uint32, laddrlen uint32, raddrlen uint32, lrsa **RawSockaddrAny, lrsalen *int32, rrsa **RawSockaddrAny, rrsalen *int32) = mswsock.GetAcceptExSockaddrs
//sys    WSARecv(s Handle, bufs *WSABuf, bufcnt uint32, recvd *uint32, flags *uint32, overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSARecv
//sys    WSASend(s Handle, bufs *WSABuf, bufcnt uint32, sent *uint32, flags uint32, overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSASend
//sys    WSARecvFrom(s Handle, bufs *WSABuf, bufcnt uint32, recvd *uint32, flags *uint32,  from *RawSockaddrAny, fromlen *int32, overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSARecvFrom
//sys    WSASendTo(s Handle, bufs *WSABuf, bufcnt uint32, sent *uint32, flags uint32, to *RawSockaddrAny, tolen int32,  overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSASendTo
//sys    GetHostByName(name string) (h *Hostent, err error) [failretval==nil] = ws2_32.gethostbyname
//sys    GetServByName(name string, proto string) (s *Servent, err error) [failretval==nil] = ws2_32.getservbyname
//sys    Ntohs(netshort uint16) (u uint16) = ws2_32.ntohs
//sys    GetProtoByName(name string) (p *Protoent, err error) [failretval==nil] = ws2_32.getprotobyname
//sys    DnsQuery(name string, qtype uint16, options uint32, extra *byte, qrs **DNSRecord, pr *byte) (status error) = dnsapi.DnsQuery_W
//sys    DnsRecordListFree(rl *DNSRecord, freetype uint32) = dnsapi.DnsRecordListFree
//sys    DnsNameCompare(name1 *uint16, name2 *uint16) (same bool) = dnsapi.DnsNameCompare_W
//sys    GetAddrInfoW(nodename *uint16, servicename *uint16, hints *AddrinfoW, result **AddrinfoW) (sockerr error) = ws2_32.GetAddrInfoW
//sys    FreeAddrInfoW(addrinfo *AddrinfoW) = ws2_32.FreeAddrInfoW
//sys    GetIfEntry(pIfRow *MibIfRow) (errcode error) = iphlpapi.GetIfEntry
//sys    GetAdaptersInfo(ai *IpAdapterInfo, ol *uint32) (errcode error) = iphlpapi.GetAdaptersInfo
//sys    SetFileCompletionNotificationModes(handle Handle, flags uint8) (err error) = kernel32.SetFileCompletionNotificationModes
//sys    WSAEnumProtocols(protocols *int32, protocolBuffer *WSAProtocolInfo, bufferLength *uint32) (n int32, err error) [failretval==-1] = ws2_32.WSAEnumProtocolsW

// For testing: clients can set this flag to force
// creation of IPv6 sockets to return EAFNOSUPPORT.


//sys    WSAStartup(verreq uint32, data *WSAData) (sockerr error) = ws2_32.WSAStartup
//sys    WSACleanup() (err error) [failretval==socket_error] = ws2_32.WSACleanup
//sys    WSAIoctl(s Handle, iocc uint32, inbuf *byte, cbif uint32, outbuf *byte, cbob uint32, cbbr *uint32, overlapped *Overlapped, completionRoutine uintptr) (err error) [failretval==socket_error] = ws2_32.WSAIoctl
//sys    socket(af int32, typ int32, protocol int32) (handle Handle, err error) [failretval==InvalidHandle] = ws2_32.socket
//sys    Setsockopt(s Handle, level int32, optname int32, optval *byte, optlen int32) (err error) [failretval==socket_error] = ws2_32.setsockopt
//sys    Getsockopt(s Handle, level int32, optname int32, optval *byte, optlen *int32) (err error) [failretval==socket_error] = ws2_32.getsockopt
//sys    bind(s Handle, name unsafe.Pointer, namelen int32) (err error) [failretval==socket_error] = ws2_32.bind
//sys    connect(s Handle, name unsafe.Pointer, namelen int32) (err error) [failretval==socket_error] = ws2_32.connect
//sys    getsockname(s Handle, rsa *RawSockaddrAny, addrlen *int32) (err error) [failretval==socket_error] = ws2_32.getsockname
//sys    getpeername(s Handle, rsa *RawSockaddrAny, addrlen *int32) (err error) [failretval==socket_error] = ws2_32.getpeername
//sys    listen(s Handle, backlog int32) (err error) [failretval==socket_error] = ws2_32.listen
//sys    shutdown(s Handle, how int32) (err error) [failretval==socket_error] = ws2_32.shutdown
//sys    Closesocket(s Handle) (err error) [failretval==socket_error] = ws2_32.closesocket
//sys    AcceptEx(ls Handle, as Handle, buf *byte, rxdatalen uint32, laddrlen uint32, raddrlen uint32, recvd *uint32, overlapped *Overlapped) (err error) = mswsock.AcceptEx
//sys    GetAcceptExSockaddrs(buf *byte, rxdatalen uint32, laddrlen uint32, raddrlen uint32, lrsa **RawSockaddrAny, lrsalen *int32, rrsa **RawSockaddrAny, rrsalen *int32) = mswsock.GetAcceptExSockaddrs
//sys    WSARecv(s Handle, bufs *WSABuf, bufcnt uint32, recvd *uint32, flags *uint32, overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSARecv
//sys    WSASend(s Handle, bufs *WSABuf, bufcnt uint32, sent *uint32, flags uint32, overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSASend
//sys    WSARecvFrom(s Handle, bufs *WSABuf, bufcnt uint32, recvd *uint32, flags *uint32,  from *RawSockaddrAny, fromlen *int32, overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSARecvFrom
//sys    WSASendTo(s Handle, bufs *WSABuf, bufcnt uint32, sent *uint32, flags uint32, to *RawSockaddrAny, tolen int32,  overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSASendTo
//sys    GetHostByName(name string) (h *Hostent, err error) [failretval==nil] = ws2_32.gethostbyname
//sys    GetServByName(name string, proto string) (s *Servent, err error) [failretval==nil] = ws2_32.getservbyname
//sys    Ntohs(netshort uint16) (u uint16) = ws2_32.ntohs
//sys    GetProtoByName(name string) (p *Protoent, err error) [failretval==nil] = ws2_32.getprotobyname
//sys    DnsQuery(name string, qtype uint16, options uint32, extra *byte, qrs **DNSRecord, pr *byte) (status error) = dnsapi.DnsQuery_W
//sys    DnsRecordListFree(rl *DNSRecord, freetype uint32) = dnsapi.DnsRecordListFree
//sys    DnsNameCompare(name1 *uint16, name2 *uint16) (same bool) = dnsapi.DnsNameCompare_W
//sys    GetAddrInfoW(nodename *uint16, servicename *uint16, hints *AddrinfoW, result **AddrinfoW) (sockerr error) = ws2_32.GetAddrInfoW
//sys    FreeAddrInfoW(addrinfo *AddrinfoW) = ws2_32.FreeAddrInfoW
//sys    GetIfEntry(pIfRow *MibIfRow) (errcode error) = iphlpapi.GetIfEntry
//sys    GetAdaptersInfo(ai *IpAdapterInfo, ol *uint32) (errcode error) = iphlpapi.GetAdaptersInfo
//sys    SetFileCompletionNotificationModes(handle Handle, flags uint8) (err error) = kernel32.SetFileCompletionNotificationModes
//sys    WSAEnumProtocols(protocols *int32, protocolBuffer *WSAProtocolInfo, bufferLength *uint32) (n int32, err error) [failretval==-1] = ws2_32.WSAEnumProtocolsW

// For testing: clients can set this flag to force
// creation of IPv6 sockets to return EAFNOSUPPORT.
public static bool SocketDisableIPv6 = default;

public partial struct RawSockaddrInet4 {
    public ushort Family;
    public ushort Port;
    public array<byte> Addr; /* in_addr */
    public array<byte> Zero;
}

public partial struct RawSockaddrInet6 {
    public ushort Family;
    public ushort Port;
    public uint Flowinfo;
    public array<byte> Addr; /* in6_addr */
    public uint Scope_id;
}

public partial struct RawSockaddr {
    public ushort Family;
    public array<sbyte> Data;
}

public partial struct RawSockaddrAny {
    public RawSockaddr Addr;
    public array<sbyte> Pad;
}

public partial interface Sockaddr {
    (unsafe.Pointer, int, error) sockaddr(); // lowercase; only we can define Sockaddrs
}

public partial struct SockaddrInet4 {
    public nint Port;
    public array<byte> Addr;
    public RawSockaddrInet4 raw;
}

private static (unsafe.Pointer, int, error) sockaddr(this ptr<SockaddrInet4> _addr_sa) {
    unsafe.Pointer _p0 = default;
    int _p0 = default;
    error _p0 = default!;
    ref SockaddrInet4 sa = ref _addr_sa.val;

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Family = AF_INET;
    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
    p[0] = byte(sa.Port >> 8);
    p[1] = byte(sa.Port);
    for (nint i = 0; i < len(sa.Addr); i++) {
        sa.raw.Addr[i] = sa.Addr[i];
    }
    return (@unsafe.Pointer(_addr_sa.raw), int32(@unsafe.Sizeof(sa.raw)), error.As(null!)!);

}

public partial struct SockaddrInet6 {
    public nint Port;
    public uint ZoneId;
    public array<byte> Addr;
    public RawSockaddrInet6 raw;
}

private static (unsafe.Pointer, int, error) sockaddr(this ptr<SockaddrInet6> _addr_sa) {
    unsafe.Pointer _p0 = default;
    int _p0 = default;
    error _p0 = default!;
    ref SockaddrInet6 sa = ref _addr_sa.val;

    if (sa.Port < 0 || sa.Port > 0xFFFF) {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Family = AF_INET6;
    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
    p[0] = byte(sa.Port >> 8);
    p[1] = byte(sa.Port);
    sa.raw.Scope_id = sa.ZoneId;
    for (nint i = 0; i < len(sa.Addr); i++) {
        sa.raw.Addr[i] = sa.Addr[i];
    }
    return (@unsafe.Pointer(_addr_sa.raw), int32(@unsafe.Sizeof(sa.raw)), error.As(null!)!);

}

public partial struct RawSockaddrUnix {
    public ushort Family;
    public array<sbyte> Path;
}

public partial struct SockaddrUnix {
    public @string Name;
    public RawSockaddrUnix raw;
}

private static (unsafe.Pointer, int, error) sockaddr(this ptr<SockaddrUnix> _addr_sa) {
    unsafe.Pointer _p0 = default;
    int _p0 = default;
    error _p0 = default!;
    ref SockaddrUnix sa = ref _addr_sa.val;

    var name = sa.Name;
    var n = len(name);
    if (n > len(sa.raw.Path)) {
        return (null, 0, error.As(EINVAL)!);
    }
    if (n == len(sa.raw.Path) && name[0] != '@') {
        return (null, 0, error.As(EINVAL)!);
    }
    sa.raw.Family = AF_UNIX;
    for (nint i = 0; i < n; i++) {
        sa.raw.Path[i] = int8(name[i]);
    } 
    // length is family (uint16), name, NUL.
    var sl = int32(2);
    if (n > 0) {
        sl += int32(n) + 1;
    }
    if (sa.raw.Path[0] == '@') {
        sa.raw.Path[0] = 0; 
        // Don't count trailing NUL for abstract address.
        sl--;

    }
    return (@unsafe.Pointer(_addr_sa.raw), sl, error.As(null!)!);

}

private static (Sockaddr, error) Sockaddr(this ptr<RawSockaddrAny> _addr_rsa) {
    Sockaddr _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;


    if (rsa.Addr.Family == AF_UNIX) 
        var pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
        ptr<SockaddrUnix> sa = @new<SockaddrUnix>();
        if (pp.Path[0] == 0) { 
            // "Abstract" Unix domain socket.
            // Rewrite leading NUL as @ for textual display.
            // (This is the standard convention.)
            // Not friendly to overwrite in place,
            // but the callers below don't care.
            pp.Path[0] = '@';

        }
        nint n = 0;
        while (n < len(pp.Path) && pp.Path[n] != 0) {
            n++;
        }
        ptr<array<byte>> bytes = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Path[0])).slice(0, n, n);
        sa.Name = string(bytes);
        return (sa, error.As(null!)!);
    else if (rsa.Addr.Family == AF_INET) 
        pp = (RawSockaddrInet4.val)(@unsafe.Pointer(rsa));
        sa = @new<SockaddrInet4>();
        ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Port));
        sa.Port = int(p[0]) << 8 + int(p[1]);
        {
            nint i__prev1 = i;

            for (nint i = 0; i < len(sa.Addr); i++) {
                sa.Addr[i] = pp.Addr[i];
            }


            i = i__prev1;
        }
        return (sa, error.As(null!)!);
    else if (rsa.Addr.Family == AF_INET6) 
        pp = (RawSockaddrInet6.val)(@unsafe.Pointer(rsa));
        sa = @new<SockaddrInet6>();
        p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Port));
        sa.Port = int(p[0]) << 8 + int(p[1]);
        sa.ZoneId = pp.Scope_id;
        {
            nint i__prev1 = i;

            for (i = 0; i < len(sa.Addr); i++) {
                sa.Addr[i] = pp.Addr[i];
            }


            i = i__prev1;
        }
        return (sa, error.As(null!)!);
        return (null, error.As(EAFNOSUPPORT)!);

}

public static (Handle, error) Socket(nint domain, nint typ, nint proto) {
    Handle fd = default;
    error err = default!;

    if (domain == AF_INET6 && SocketDisableIPv6) {
        return (InvalidHandle, error.As(EAFNOSUPPORT)!);
    }
    return socket(int32(domain), int32(typ), int32(proto));

}

public static error SetsockoptInt(Handle fd, nint level, nint opt, nint value) {
    error err = default!;

    ref var v = ref heap(int32(value), out ptr<var> _addr_v);
    return error.As(Setsockopt(fd, int32(level), int32(opt), (byte.val)(@unsafe.Pointer(_addr_v)), int32(@unsafe.Sizeof(v))))!;
}

public static error Bind(Handle fd, Sockaddr sa) {
    error err = default!;

    var (ptr, n, err) = sa.sockaddr();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(bind(fd, ptr, n))!;

}

public static error Connect(Handle fd, Sockaddr sa) {
    error err = default!;

    var (ptr, n, err) = sa.sockaddr();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(connect(fd, ptr, n))!;

}

public static (Sockaddr, error) Getsockname(Handle fd) {
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref var l = ref heap(int32(@unsafe.Sizeof(rsa)), out ptr<var> _addr_l);
    err = getsockname(fd, _addr_rsa, _addr_l);

    if (err != null) {
        return ;
    }
    return rsa.Sockaddr();

}

public static (Sockaddr, error) Getpeername(Handle fd) {
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref var l = ref heap(int32(@unsafe.Sizeof(rsa)), out ptr<var> _addr_l);
    err = getpeername(fd, _addr_rsa, _addr_l);

    if (err != null) {
        return ;
    }
    return rsa.Sockaddr();

}

public static error Listen(Handle s, nint n) {
    error err = default!;

    return error.As(listen(s, int32(n)))!;
}

public static error Shutdown(Handle fd, nint how) {
    error err = default!;

    return error.As(shutdown(fd, int32(how)))!;
}

public static error WSASendto(Handle s, ptr<WSABuf> _addr_bufs, uint bufcnt, ptr<uint> _addr_sent, uint flags, Sockaddr to, ptr<Overlapped> _addr_overlapped, ptr<byte> _addr_croutine) {
    error err = default!;
    ref WSABuf bufs = ref _addr_bufs.val;
    ref uint sent = ref _addr_sent.val;
    ref Overlapped overlapped = ref _addr_overlapped.val;
    ref byte croutine = ref _addr_croutine.val;

    var (rsa, len, err) = to.sockaddr();
    if (err != null) {
        return error.As(err)!;
    }
    var (r1, _, e1) = Syscall9(procWSASendTo.Addr(), 9, uintptr(s), uintptr(@unsafe.Pointer(bufs)), uintptr(bufcnt), uintptr(@unsafe.Pointer(sent)), uintptr(flags), uintptr(@unsafe.Pointer(rsa)), uintptr(len), uintptr(@unsafe.Pointer(overlapped)), uintptr(@unsafe.Pointer(croutine)));
    if (r1 == socket_error) {
        if (e1 != 0) {
            err = errnoErr(e1);
        }
        else
 {
            err = EINVAL;
        }
    }
    return error.As(err)!;

}

public static error LoadGetAddrInfo() {
    return error.As(procGetAddrInfoW.Find())!;
}

private static var connectExFunc = default;

public static error LoadConnectEx() => func((defer, _, _) => {
    connectExFunc.once.Do(() => {
        Handle s = default;
        s, connectExFunc.err = Socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
        if (connectExFunc.err != null) {
            return ;
        }
        defer(CloseHandle(s));
        ref uint n = ref heap(out ptr<uint> _addr_n);
        connectExFunc.err = WSAIoctl(s, SIO_GET_EXTENSION_FUNCTION_POINTER, (byte.val)(@unsafe.Pointer(_addr_WSAID_CONNECTEX)), uint32(@unsafe.Sizeof(WSAID_CONNECTEX)), (byte.val)(@unsafe.Pointer(_addr_connectExFunc.addr)), uint32(@unsafe.Sizeof(connectExFunc.addr)), _addr_n, null, 0);

    });
    return error.As(connectExFunc.err)!;

});

private static error connectEx(Handle s, unsafe.Pointer name, int namelen, ptr<byte> _addr_sendBuf, uint sendDataLen, ptr<uint> _addr_bytesSent, ptr<Overlapped> _addr_overlapped) {
    error err = default!;
    ref byte sendBuf = ref _addr_sendBuf.val;
    ref uint bytesSent = ref _addr_bytesSent.val;
    ref Overlapped overlapped = ref _addr_overlapped.val;

    var (r1, _, e1) = Syscall9(connectExFunc.addr, 7, uintptr(s), uintptr(name), uintptr(namelen), uintptr(@unsafe.Pointer(sendBuf)), uintptr(sendDataLen), uintptr(@unsafe.Pointer(bytesSent)), uintptr(@unsafe.Pointer(overlapped)), 0, 0);
    if (r1 == 0) {
        if (e1 != 0) {
            err = error(e1);
        }
        else
 {
            err = EINVAL;
        }
    }
    return ;

}

public static error ConnectEx(Handle fd, Sockaddr sa, ptr<byte> _addr_sendBuf, uint sendDataLen, ptr<uint> _addr_bytesSent, ptr<Overlapped> _addr_overlapped) {
    ref byte sendBuf = ref _addr_sendBuf.val;
    ref uint bytesSent = ref _addr_bytesSent.val;
    ref Overlapped overlapped = ref _addr_overlapped.val;

    var err = LoadConnectEx();
    if (err != null) {
        return error.As(errorspkg.New("failed to find ConnectEx: " + err.Error()))!;
    }
    var (ptr, n, err) = sa.sockaddr();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(connectEx(fd, ptr, n, _addr_sendBuf, sendDataLen, _addr_bytesSent, _addr_overlapped))!;

}

// Invented structures to support what package os expects.
public partial struct Rusage {
    public Filetime CreationTime;
    public Filetime ExitTime;
    public Filetime KernelTime;
    public Filetime UserTime;
}

public partial struct WaitStatus {
    public uint ExitCode;
}

public static bool Exited(this WaitStatus w) {
    return true;
}

public static nint ExitStatus(this WaitStatus w) {
    return int(w.ExitCode);
}

public static Signal Signal(this WaitStatus w) {
    return -1;
}

public static bool CoreDump(this WaitStatus w) {
    return false;
}

public static bool Stopped(this WaitStatus w) {
    return false;
}

public static bool Continued(this WaitStatus w) {
    return false;
}

public static Signal StopSignal(this WaitStatus w) {
    return -1;
}

public static bool Signaled(this WaitStatus w) {
    return false;
}

public static nint TrapCause(this WaitStatus w) {
    return -1;
}

// Timespec is an invented structure on Windows, but here for
// consistency with the syscall package for other operating systems.
public partial struct Timespec {
    public long Sec;
    public long Nsec;
}

public static long TimespecToNsec(Timespec ts) {
    return int64(ts.Sec) * 1e9F + int64(ts.Nsec);
}

public static Timespec NsecToTimespec(long nsec) {
    Timespec ts = default;

    ts.Sec = nsec / 1e9F;
    ts.Nsec = nsec % 1e9F;
    return ;
}

// TODO(brainman): fix all needed for net

public static (Handle, Sockaddr, error) Accept(Handle fd) {
    Handle nfd = default;
    Sockaddr sa = default;
    error err = default!;

    return (0, null, error.As(EWINDOWS)!);
}
public static (nint, Sockaddr, error) Recvfrom(Handle fd, slice<byte> p, nint flags) {
    nint n = default;
    Sockaddr from = default;
    error err = default!;

    return (0, null, error.As(EWINDOWS)!);
}
public static error Sendto(Handle fd, slice<byte> p, nint flags, Sockaddr to) {
    error err = default!;

    return error.As(EWINDOWS)!;
}
public static error SetsockoptTimeval(Handle fd, nint level, nint opt, ptr<Timeval> _addr_tv) {
    error err = default!;
    ref Timeval tv = ref _addr_tv.val;

    return error.As(EWINDOWS)!;
}

// The Linger struct is wrong but we only noticed after Go 1.
// sysLinger is the real system call structure.

// BUG(brainman): The definition of Linger is not appropriate for direct use
// with Setsockopt and Getsockopt.
// Use SetsockoptLinger instead.

public partial struct Linger {
    public int Onoff;
    public int Linger;
}

private partial struct sysLinger {
    public ushort Onoff;
    public ushort Linger;
}

public partial struct IPMreq {
    public array<byte> Multiaddr; /* in_addr */
    public array<byte> Interface; /* in_addr */
}

public partial struct IPv6Mreq {
    public array<byte> Multiaddr; /* in6_addr */
    public uint Interface;
}

public static (nint, error) GetsockoptInt(Handle fd, nint level, nint opt) {
    nint _p0 = default;
    error _p0 = default!;

    return (-1, error.As(EWINDOWS)!);
}

public static error SetsockoptLinger(Handle fd, nint level, nint opt, ptr<Linger> _addr_l) {
    error err = default!;
    ref Linger l = ref _addr_l.val;

    ref sysLinger sys = ref heap(new sysLinger(Onoff:uint16(l.Onoff),Linger:uint16(l.Linger)), out ptr<sysLinger> _addr_sys);
    return error.As(Setsockopt(fd, int32(level), int32(opt), (byte.val)(@unsafe.Pointer(_addr_sys)), int32(@unsafe.Sizeof(sys))))!;
}

public static error SetsockoptInet4Addr(Handle fd, nint level, nint opt, array<byte> value) {
    error err = default!;
    value = value.Clone();

    return error.As(Setsockopt(fd, int32(level), int32(opt), (byte.val)(@unsafe.Pointer(_addr_value[0])), 4))!;
}
public static error SetsockoptIPMreq(Handle fd, nint level, nint opt, ptr<IPMreq> _addr_mreq) {
    error err = default!;
    ref IPMreq mreq = ref _addr_mreq.val;

    return error.As(Setsockopt(fd, int32(level), int32(opt), (byte.val)(@unsafe.Pointer(mreq)), int32(@unsafe.Sizeof(mreq))))!;
}
public static error SetsockoptIPv6Mreq(Handle fd, nint level, nint opt, ptr<IPv6Mreq> _addr_mreq) {
    error err = default!;
    ref IPv6Mreq mreq = ref _addr_mreq.val;

    return error.As(EWINDOWS)!;
}

public static nint Getpid() {
    nint pid = default;

    return int(getCurrentProcessId());
}

public static (Handle, error) FindFirstFile(ptr<ushort> _addr_name, ptr<Win32finddata> _addr_data) {
    Handle handle = default;
    error err = default!;
    ref ushort name = ref _addr_name.val;
    ref Win32finddata data = ref _addr_data.val;
 
    // NOTE(rsc): The Win32finddata struct is wrong for the system call:
    // the two paths are each one uint16 short. Use the correct struct,
    // a win32finddata1, and then copy the results out.
    // There is no loss of expressivity here, because the final
    // uint16, if it is used, is supposed to be a NUL, and Go doesn't need that.
    // For Go 1.1, we might avoid the allocation of win32finddata1 here
    // by adding a final Bug [2]uint16 field to the struct and then
    // adjusting the fields in the result directly.
    ref win32finddata1 data1 = ref heap(out ptr<win32finddata1> _addr_data1);
    handle, err = findFirstFile1(name, _addr_data1);
    if (err == null) {
        copyFindData(data, _addr_data1);
    }
    return ;

}

public static error FindNextFile(Handle handle, ptr<Win32finddata> _addr_data) {
    error err = default!;
    ref Win32finddata data = ref _addr_data.val;

    ref win32finddata1 data1 = ref heap(out ptr<win32finddata1> _addr_data1);
    err = findNextFile1(handle, _addr_data1);
    if (err == null) {
        copyFindData(data, _addr_data1);
    }
    return ;

}

private static (ptr<ProcessEntry32>, error) getProcessEntry(nint pid) => func((defer, _, _) => {
    ptr<ProcessEntry32> _p0 = default!;
    error _p0 = default!;

    var (snapshot, err) = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(CloseHandle(snapshot));
    ref ProcessEntry32 procEntry = ref heap(out ptr<ProcessEntry32> _addr_procEntry);
    procEntry.Size = uint32(@unsafe.Sizeof(procEntry));
    err = Process32First(snapshot, _addr_procEntry);

    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    while (true) {
        if (procEntry.ProcessID == uint32(pid)) {
            return (_addr__addr_procEntry!, error.As(null!)!);
        }
        err = Process32Next(snapshot, _addr_procEntry);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

});

public static nint Getppid() {
    nint ppid = default;

    var (pe, err) = getProcessEntry(Getpid());
    if (err != null) {
        return -1;
    }
    return int(pe.ParentProcessID);

}

// TODO(brainman): fix all needed for os
public static error Fchdir(Handle fd) {
    error err = default!;

    return error.As(EWINDOWS)!;
}
public static error Link(@string oldpath, @string newpath) {
    error err = default!;

    return error.As(EWINDOWS)!;
}
public static error Symlink(@string path, @string link) {
    error err = default!;

    return error.As(EWINDOWS)!;
}

public static error Fchmod(Handle fd, uint mode) {
    error err = default!;

    return error.As(EWINDOWS)!;
}
public static error Chown(@string path, nint uid, nint gid) {
    error err = default!;

    return error.As(EWINDOWS)!;
}
public static error Lchown(@string path, nint uid, nint gid) {
    error err = default!;

    return error.As(EWINDOWS)!;
}
public static error Fchown(Handle fd, nint uid, nint gid) {
    error err = default!;

    return error.As(EWINDOWS)!;
}

public static nint Getuid() {
    nint uid = default;

    return -1;
}
public static nint Geteuid() {
    nint euid = default;

    return -1;
}
public static nint Getgid() {
    nint gid = default;

    return -1;
}
public static nint Getegid() {
    nint egid = default;

    return -1;
}
public static (slice<nint>, error) Getgroups() {
    slice<nint> gids = default;
    error err = default!;

    return (null, error.As(EWINDOWS)!);
}

public partial struct Signal { // : nint
}

public static void Signal(this Signal s) {
}

public static @string String(this Signal s) {
    if (0 <= s && int(s) < len(signals)) {
        var str = signals[s];
        if (str != "") {
            return str;
        }
    }
    return "signal " + itoa.Itoa(int(s));

}

public static error LoadCreateSymbolicLink() {
    return error.As(procCreateSymbolicLinkW.Find())!;
}

// Readlink returns the destination of the named symbolic link.
public static (nint, error) Readlink(@string path, slice<byte> buf) => func((defer, _, _) => {
    nint n = default;
    error err = default!;

    var (fd, err) = CreateFile(StringToUTF16Ptr(path), GENERIC_READ, 0, null, OPEN_EXISTING, FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_BACKUP_SEMANTICS, 0);
    if (err != null) {
        return (-1, error.As(err)!);
    }
    defer(CloseHandle(fd));

    var rdbbuf = make_slice<byte>(MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
    ref uint bytesReturned = ref heap(out ptr<uint> _addr_bytesReturned);
    err = DeviceIoControl(fd, FSCTL_GET_REPARSE_POINT, null, 0, _addr_rdbbuf[0], uint32(len(rdbbuf)), _addr_bytesReturned, null);
    if (err != null) {
        return (-1, error.As(err)!);
    }
    var rdb = (reparseDataBuffer.val)(@unsafe.Pointer(_addr_rdbbuf[0]));
    @string s = default;

    if (rdb.ReparseTag == IO_REPARSE_TAG_SYMLINK) 
        var data = (symbolicLinkReparseBuffer.val)(@unsafe.Pointer(_addr_rdb.reparseBuffer));
        ptr<array<ushort>> p = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(_addr_data.PathBuffer[0]));
        s = UTF16ToString(p[(int)data.SubstituteNameOffset / 2..(int)(data.SubstituteNameOffset + data.SubstituteNameLength) / 2]);
        if (data.Flags & _SYMLINK_FLAG_RELATIVE == 0) {
            if (len(s) >= 4 && s[..(int)4] == "\\??\\") {
                s = s[(int)4..];

                if (len(s) >= 2 && s[1] == ':')                 else if (len(s) >= 4 && s[..(int)4] == "UNC\\") // \??\UNC\foo\bar
                    s = "\\\\" + s[(int)4..];
                else                 
            }
            else
 { 
                // unexpected; do nothing
            }

        }
    else if (rdb.ReparseTag == _IO_REPARSE_TAG_MOUNT_POINT) 
        data = (mountPointReparseBuffer.val)(@unsafe.Pointer(_addr_rdb.reparseBuffer));
        p = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(_addr_data.PathBuffer[0]));
        s = UTF16ToString(p[(int)data.SubstituteNameOffset / 2..(int)(data.SubstituteNameOffset + data.SubstituteNameLength) / 2]);
        if (len(s) >= 4 && s[..(int)4] == "\\??\\") { // \??\C:\foo\bar
            s = s[(int)4..];

        }
        else
 { 
            // unexpected; do nothing
        }
    else 
        // the path is not a symlink or junction but another type of reparse
        // point
        return (-1, error.As(ENOENT)!);
        n = copy(buf, (slice<byte>)s);

    return (n, error.As(null!)!);

});

// Deprecated: CreateIoCompletionPort has the wrong function signature. Use x/sys/windows.CreateIoCompletionPort.
public static (Handle, error) CreateIoCompletionPort(Handle filehandle, Handle cphandle, uint key, uint threadcnt) {
    Handle _p0 = default;
    error _p0 = default!;

    return createIoCompletionPort(filehandle, cphandle, uintptr(key), threadcnt);
}

// Deprecated: GetQueuedCompletionStatus has the wrong function signature. Use x/sys/windows.GetQueuedCompletionStatus.
public static error GetQueuedCompletionStatus(Handle cphandle, ptr<uint> _addr_qty, ptr<uint> _addr_key, ptr<ptr<Overlapped>> _addr_overlapped, uint timeout) {
    ref uint qty = ref _addr_qty.val;
    ref uint key = ref _addr_key.val;
    ref ptr<Overlapped> overlapped = ref _addr_overlapped.val;

    ref System.UIntPtr ukey = ref heap(out ptr<System.UIntPtr> _addr_ukey);
    ptr<System.UIntPtr> pukey;
    if (key != null) {
        ukey = uintptr(key);
        pukey = _addr_ukey;
    }
    var err = getQueuedCompletionStatus(cphandle, qty, pukey, overlapped, timeout);
    if (key != null) {
        key = uint32(ukey);
        if (uintptr(key) != ukey && err == null) {
            err = errorspkg.New("GetQueuedCompletionStatus returned key overflow");
        }
    }
    return error.As(err)!;

}

// Deprecated: PostQueuedCompletionStatus has the wrong function signature. Use x/sys/windows.PostQueuedCompletionStatus.
public static error PostQueuedCompletionStatus(Handle cphandle, uint qty, uint key, ptr<Overlapped> _addr_overlapped) {
    ref Overlapped overlapped = ref _addr_overlapped.val;

    return error.As(postQueuedCompletionStatus(cphandle, qty, uintptr(key), overlapped))!;
}

// newProcThreadAttributeList allocates new PROC_THREAD_ATTRIBUTE_LIST, with
// the requested maximum number of attributes, which must be cleaned up by
// deleteProcThreadAttributeList.
private static (ptr<_PROC_THREAD_ATTRIBUTE_LIST>, error) newProcThreadAttributeList(uint maxAttrCount) {
    ptr<_PROC_THREAD_ATTRIBUTE_LIST> _p0 = default!;
    error _p0 = default!;

    ref System.UIntPtr size = ref heap(out ptr<System.UIntPtr> _addr_size);
    var err = initializeProcThreadAttributeList(null, maxAttrCount, 0, _addr_size);
    if (err != ERROR_INSUFFICIENT_BUFFER) {
        if (err == null) {
            return (_addr_null!, error.As(errorspkg.New("unable to query buffer size from InitializeProcThreadAttributeList"))!);
        }
        return (_addr_null!, error.As(err)!);

    }
    var al = (_PROC_THREAD_ATTRIBUTE_LIST.val)(@unsafe.Pointer(_addr_make_slice<byte>(size)[0]));
    err = initializeProcThreadAttributeList(al, maxAttrCount, 0, _addr_size);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr_al!, error.As(null!)!);

}

} // end syscall_package
