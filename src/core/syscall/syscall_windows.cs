// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Windows system calls.
namespace go;

using errorspkg = errors_package;
using asan = @internal.asan_package;
using bytealg = @internal.bytealg_package;
using itoa = @internal.itoa_package;
using msan = @internal.msan_package;
using oserror = @internal.oserror_package;
using race = @internal.race_package;
using runtime = runtime_package;
using sync = sync_package;
using @unsafe = unsafe_package;
using @internal;

partial class syscall_package {

[GoType("num:uintptr")] partial struct ΔHandle;

public static readonly ΔHandle InvalidHandle = /* ^Handle(0) */ 18446744073709551615;

// StringToUTF16 returns the UTF-16 encoding of the UTF-8 string s,
// with a terminating NUL added. If s contains a NUL byte this
// function panics instead of returning an error.
//
// Deprecated: Use [UTF16FromString] instead.
public static slice<uint16> StringToUTF16(@string s) {
    (a, err) = UTF16FromString(s);
    if (err != default!) {
        throw panic("syscall: string with NUL passed to StringToUTF16");
    }
    return a;
}

// UTF16FromString returns the UTF-16 encoding of the UTF-8 string
// s, with a terminating NUL added. If s contains a NUL byte at any
// location, it returns (nil, [EINVAL]). Unpaired surrogates
// are encoded using WTF-8.
public static (slice<uint16>, error) UTF16FromString(@string s) {
    if (bytealg.IndexByteString(s, 0) != -1) {
        return (default!, EINVAL);
    }
    // Valid UTF-8 characters between 1 and 3 bytes require one uint16.
    // Valid UTF-8 characters of 4 bytes require two uint16.
    // Bytes with invalid UTF-8 encoding require maximum one uint16 per byte.
    // So the number of UTF-8 code units (len(s)) is always greater or
    // equal than the number of UTF-16 code units.
    // Also account for the terminating NUL character.
    var buf = new slice<uint16>(0, len(s) + 1);
    buf = encodeWTF16(s, buf);
    return (append(buf, 0), default!);
}

// UTF16ToString returns the UTF-8 encoding of the UTF-16 sequence s,
// with a terminating NUL removed. Unpaired surrogates are decoded
// using WTF-8 instead of UTF-8 encoding.
public static @string UTF16ToString(slice<uint16> s) {
    nint maxLen = 0;
    foreach (var (i, v) in s) {
        if (v == 0) {
            s = s[0..(int)(i)];
            break;
        }
        switch (ᐧ) {
        case {} when v <= rune1Max: {
            maxLen += 1;
            break;
        }
        case {} when v <= rune2Max: {
            maxLen += 2;
            break;
        }
        default: {
            maxLen += 3;
            break;
        }}

    }
    // r is a non-surrogate that decodes to 3 bytes,
    // or is an unpaired surrogate (also 3 bytes in WTF-8),
    // or is one half of a valid surrogate pair.
    // If it is half of a pair, we will add 3 for the second surrogate
    // (total of 6) and overestimate by 2 bytes for the pair,
    // since the resulting rune only requires 4 bytes.
    var buf = decodeWTF16(s, new slice<byte>(0, maxLen));
    return @unsafe.String(@unsafe.SliceData(buf), len(buf));
}

// utf16PtrToString is like UTF16ToString, but takes *uint16
// as a parameter instead of []uint16.
internal static @string utf16PtrToString(ж<uint16> Ꮡp) {
    ref var p = ref Ꮡp.val;

    if (p == nil) {
        return ""u8;
    }
    @unsafe.Pointer end = new @unsafe.Pointer(Ꮡp);
    nint n = 0;
    while (~(ж<uint16>)(uintptr)(end) != 0) {
        end = ((@unsafe.Pointer)(((uintptr)end) + @unsafe.Sizeof(p)));
        n++;
    }
    return UTF16ToString(@unsafe.Slice(Ꮡp, n));
}

// StringToUTF16Ptr returns pointer to the UTF-16 encoding of
// the UTF-8 string s, with a terminating NUL added. If s
// contains a NUL byte this function panics instead of
// returning an error.
//
// Deprecated: Use [UTF16PtrFromString] instead.
public static ж<uint16> StringToUTF16Ptr(@string s) {
    return ᏑStringToUTF16(s).at<uint16>(0);
}

// UTF16PtrFromString returns pointer to the UTF-16 encoding of
// the UTF-8 string s, with a terminating NUL added. If s
// contains a NUL byte at any location, it returns (nil, EINVAL).
// Unpaired surrogates are encoded using WTF-8.
public static (ж<uint16>, error) UTF16PtrFromString(@string s) {
    (a, err) = UTF16FromString(s);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(a, 0), default!);
}

[GoType("num:uintptr")] partial struct Errno;

internal static uint32 langid(uint16 pri, uint16 sub) {
    return (uint32)(((uint32)sub) << (int)(10) | ((uint32)pri));
}

// FormatMessage is deprecated (msgsrc should be uintptr, not uint32, but can
// not be changed due to the Go 1 compatibility guarantee).
//
// Deprecated: Use FormatMessage from golang.org/x/sys/windows instead.
public static (uint32 n, error err) FormatMessage(uint32 flags, uint32 msgsrc, uint32 msgid, uint32 langid, slice<uint16> buf, ж<byte> Ꮡargs) {
    uint32 n = default!;
    error err = default!;

    ref var args = ref Ꮡargs.val;
    return formatMessage(flags, ((uintptr)msgsrc), msgid, langid, buf, Ꮡargs);
}

public static @string Error(this Errno e) {
    // deal with special go errors
    nint idx = ((nint)(e - APPLICATION_ERROR));
    if (0 <= idx && idx < len(errors)) {
        return errors[idx];
    }
    // ask windows for the remaining errors
    uint32 flags = (uint32)((UntypedInt)(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ARGUMENT_ARRAY) | FORMAT_MESSAGE_IGNORE_INSERTS);
    var b = new slice<uint16>(300);
    var (n, err) = formatMessage(flags, 0, ((uint32)e), langid(LANG_ENGLISH, SUBLANG_ENGLISH_US), b, nil);
    if (err != default!) {
        (n, err) = formatMessage(flags, 0, ((uint32)e), 0, b, nil);
        if (err != default!) {
            return "winapi error #"u8 + itoa.Itoa(((nint)e));
        }
    }
    // trim terminating \r and \n
    for (; n > 0 && (b[n - 1] == (rune)'\n' || b[n - 1] == (rune)'\r'); n--) {
    }
    return UTF16ToString(b[..(int)(n)]);
}

internal static readonly Errno _ERROR_NOT_ENOUGH_MEMORY = /* Errno(8) */ 8;
internal static readonly Errno _ERROR_NOT_SUPPORTED = /* Errno(50) */ 50;
internal static readonly Errno _ERROR_BAD_NETPATH = /* Errno(53) */ 53;
internal static readonly Errno _ERROR_CALL_NOT_IMPLEMENTED = /* Errno(120) */ 120;

public static bool Is(this Errno e, error target) {
    var exprᴛ1 = target;
    if (exprᴛ1 == oserror.ErrPermission) {
        return e == ERROR_ACCESS_DENIED || e == EACCES || e == EPERM;
    }
    if (exprᴛ1 == oserror.ErrExist) {
        return e == ERROR_ALREADY_EXISTS || e == ERROR_DIR_NOT_EMPTY || e == ERROR_FILE_EXISTS || e == EEXIST || e == ENOTEMPTY;
    }
    if (exprᴛ1 == oserror.ErrNotExist) {
        return e == ERROR_FILE_NOT_FOUND || e == _ERROR_BAD_NETPATH || e == ERROR_PATH_NOT_FOUND || e == ENOENT;
    }
    if (exprᴛ1 == errorspkg.ErrUnsupported) {
        return e == _ERROR_NOT_SUPPORTED || e == _ERROR_CALL_NOT_IMPLEMENTED || e == ENOSYS || e == ENOTSUP || e == EOPNOTSUPP || e == EWINDOWS;
    }

    return false;
}

public static bool Temporary(this Errno e) {
    return e == EINTR || e == EMFILE || e.Timeout();
}

public static bool Timeout(this Errno e) {
    return e == EAGAIN || e == EWOULDBLOCK || e == ETIMEDOUT;
}

// Implemented in runtime/syscall_windows.go.
internal static partial uintptr compileCallback(any fn, bool cleanstack);

// NewCallback converts a Go function to a function pointer conforming to the stdcall calling convention.
// This is useful when interoperating with Windows code requiring callbacks.
// The argument is expected to be a function with one uintptr-sized result. The function must not have arguments with size larger than the size of uintptr.
// Only a limited number of callbacks may be created in a single Go process, and any memory allocated
// for these callbacks is never released.
// Between NewCallback and NewCallbackCDecl, at least 1024 callbacks can always be created.
public static uintptr NewCallback(any fn) {
    return compileCallback(fn, true);
}

// NewCallbackCDecl converts a Go function to a function pointer conforming to the cdecl calling convention.
// This is useful when interoperating with Windows code requiring callbacks.
// The argument is expected to be a function with one uintptr-sized result. The function must not have arguments with size larger than the size of uintptr.
// Only a limited number of callbacks may be created in a single Go process, and any memory allocated
// for these callbacks is never released.
// Between NewCallback and NewCallbackCDecl, at least 1024 callbacks can always be created.
public static uintptr NewCallbackCDecl(any fn) {
    return compileCallback(fn, false);
}

// windows api calls
//sys	GetLastError() (lasterr error)
//sys	LoadLibrary(libname string) (handle Handle, err error) = LoadLibraryW
//sys	FreeLibrary(handle Handle) (err error)
//sys	GetProcAddress(module Handle, procname string) (proc uintptr, err error)
//sys	GetVersion() (ver uint32, err error)
//sys	formatMessage(flags uint32, msgsrc uintptr, msgid uint32, langid uint32, buf []uint16, args *byte) (n uint32, err error) = FormatMessageW
//sys	ExitProcess(exitcode uint32)
//sys	CreateFile(name *uint16, access uint32, mode uint32, sa *SecurityAttributes, createmode uint32, attrs uint32, templatefile int32) (handle Handle, err error) [failretval==InvalidHandle] = CreateFileW
//sys	readFile(handle Handle, buf []byte, done *uint32, overlapped *Overlapped) (err error) = ReadFile
//sys	writeFile(handle Handle, buf []byte, done *uint32, overlapped *Overlapped) (err error) = WriteFile
//sys	SetFilePointer(handle Handle, lowoffset int32, highoffsetptr *int32, whence uint32) (newlowoffset uint32, err error) [failretval==0xffffffff]
//sys	CloseHandle(handle Handle) (err error)
//sys	GetStdHandle(stdhandle int) (handle Handle, err error) [failretval==InvalidHandle]
//sys	findFirstFile1(name *uint16, data *win32finddata1) (handle Handle, err error) [failretval==InvalidHandle] = FindFirstFileW
//sys	findNextFile1(handle Handle, data *win32finddata1) (err error) = FindNextFileW
//sys	FindClose(handle Handle) (err error)
//sys	GetFileInformationByHandle(handle Handle, data *ByHandleFileInformation) (err error)
//sys	GetCurrentDirectory(buflen uint32, buf *uint16) (n uint32, err error) = GetCurrentDirectoryW
//sys	SetCurrentDirectory(path *uint16) (err error) = SetCurrentDirectoryW
//sys	CreateDirectory(path *uint16, sa *SecurityAttributes) (err error) = CreateDirectoryW
//sys	RemoveDirectory(path *uint16) (err error) = RemoveDirectoryW
//sys	DeleteFile(path *uint16) (err error) = DeleteFileW
//sys	MoveFile(from *uint16, to *uint16) (err error) = MoveFileW
//sys	GetComputerName(buf *uint16, n *uint32) (err error) = GetComputerNameW
//sys	SetEndOfFile(handle Handle) (err error)
//sys	GetSystemTimeAsFileTime(time *Filetime)
//sys	GetTimeZoneInformation(tzi *Timezoneinformation) (rc uint32, err error) [failretval==0xffffffff]
//sys	createIoCompletionPort(filehandle Handle, cphandle Handle, key uintptr, threadcnt uint32) (handle Handle, err error) = CreateIoCompletionPort
//sys	getQueuedCompletionStatus(cphandle Handle, qty *uint32, key *uintptr, overlapped **Overlapped, timeout uint32) (err error) = GetQueuedCompletionStatus
//sys	postQueuedCompletionStatus(cphandle Handle, qty uint32, key uintptr, overlapped *Overlapped) (err error) = PostQueuedCompletionStatus
//sys	CancelIo(s Handle) (err error)
//sys	CancelIoEx(s Handle, o *Overlapped) (err error)
//sys	CreateProcess(appName *uint16, commandLine *uint16, procSecurity *SecurityAttributes, threadSecurity *SecurityAttributes, inheritHandles bool, creationFlags uint32, env *uint16, currentDir *uint16, startupInfo *StartupInfo, outProcInfo *ProcessInformation) (err error) = CreateProcessW
//sys	CreateProcessAsUser(token Token, appName *uint16, commandLine *uint16, procSecurity *SecurityAttributes, threadSecurity *SecurityAttributes, inheritHandles bool, creationFlags uint32, env *uint16, currentDir *uint16, startupInfo *StartupInfo, outProcInfo *ProcessInformation) (err error) = advapi32.CreateProcessAsUserW
//sys	OpenProcess(da uint32, inheritHandle bool, pid uint32) (handle Handle, err error)
//sys	TerminateProcess(handle Handle, exitcode uint32) (err error)
//sys	GetExitCodeProcess(handle Handle, exitcode *uint32) (err error)
//sys	getStartupInfo(startupInfo *StartupInfo) = GetStartupInfoW
//sys	GetCurrentProcess() (pseudoHandle Handle, err error)
//sys	GetProcessTimes(handle Handle, creationTime *Filetime, exitTime *Filetime, kernelTime *Filetime, userTime *Filetime) (err error)
//sys	DuplicateHandle(hSourceProcessHandle Handle, hSourceHandle Handle, hTargetProcessHandle Handle, lpTargetHandle *Handle, dwDesiredAccess uint32, bInheritHandle bool, dwOptions uint32) (err error)
//sys	WaitForSingleObject(handle Handle, waitMilliseconds uint32) (event uint32, err error) [failretval==0xffffffff]
//sys	GetTempPath(buflen uint32, buf *uint16) (n uint32, err error) = GetTempPathW
//sys	CreatePipe(readhandle *Handle, writehandle *Handle, sa *SecurityAttributes, size uint32) (err error)
//sys	GetFileType(filehandle Handle) (n uint32, err error)
//sys	CryptAcquireContext(provhandle *Handle, container *uint16, provider *uint16, provtype uint32, flags uint32) (err error) = advapi32.CryptAcquireContextW
//sys	CryptReleaseContext(provhandle Handle, flags uint32) (err error) = advapi32.CryptReleaseContext
//sys	CryptGenRandom(provhandle Handle, buflen uint32, buf *byte) (err error) = advapi32.CryptGenRandom
//sys	GetEnvironmentStrings() (envs *uint16, err error) [failretval==nil] = kernel32.GetEnvironmentStringsW
//sys	FreeEnvironmentStrings(envs *uint16) (err error) = kernel32.FreeEnvironmentStringsW
//sys	GetEnvironmentVariable(name *uint16, buffer *uint16, size uint32) (n uint32, err error) = kernel32.GetEnvironmentVariableW
//sys	SetEnvironmentVariable(name *uint16, value *uint16) (err error) = kernel32.SetEnvironmentVariableW
//sys	SetFileTime(handle Handle, ctime *Filetime, atime *Filetime, wtime *Filetime) (err error)
//sys	GetFileAttributes(name *uint16) (attrs uint32, err error) [failretval==INVALID_FILE_ATTRIBUTES] = kernel32.GetFileAttributesW
//sys	SetFileAttributes(name *uint16, attrs uint32) (err error) = kernel32.SetFileAttributesW
//sys	GetFileAttributesEx(name *uint16, level uint32, info *byte) (err error) = kernel32.GetFileAttributesExW
//sys	GetCommandLine() (cmd *uint16) = kernel32.GetCommandLineW
//sys	CommandLineToArgv(cmd *uint16, argc *int32) (argv *[8192]*[8192]uint16, err error) [failretval==nil] = shell32.CommandLineToArgvW
//sys	LocalFree(hmem Handle) (handle Handle, err error) [failretval!=0]
//sys	SetHandleInformation(handle Handle, mask uint32, flags uint32) (err error)
//sys	FlushFileBuffers(handle Handle) (err error)
//sys	GetFullPathName(path *uint16, buflen uint32, buf *uint16, fname **uint16) (n uint32, err error) = kernel32.GetFullPathNameW
//sys	GetLongPathName(path *uint16, buf *uint16, buflen uint32) (n uint32, err error) = kernel32.GetLongPathNameW
//sys	GetShortPathName(longpath *uint16, shortpath *uint16, buflen uint32) (n uint32, err error) = kernel32.GetShortPathNameW
//sys	CreateFileMapping(fhandle Handle, sa *SecurityAttributes, prot uint32, maxSizeHigh uint32, maxSizeLow uint32, name *uint16) (handle Handle, err error) = kernel32.CreateFileMappingW
//sys	MapViewOfFile(handle Handle, access uint32, offsetHigh uint32, offsetLow uint32, length uintptr) (addr uintptr, err error)
//sys	UnmapViewOfFile(addr uintptr) (err error)
//sys	FlushViewOfFile(addr uintptr, length uintptr) (err error)
//sys	VirtualLock(addr uintptr, length uintptr) (err error)
//sys	VirtualUnlock(addr uintptr, length uintptr) (err error)
//sys	TransmitFile(s Handle, handle Handle, bytesToWrite uint32, bytsPerSend uint32, overlapped *Overlapped, transmitFileBuf *TransmitFileBuffers, flags uint32) (err error) = mswsock.TransmitFile
//sys	ReadDirectoryChanges(handle Handle, buf *byte, buflen uint32, watchSubTree bool, mask uint32, retlen *uint32, overlapped *Overlapped, completionRoutine uintptr) (err error) = kernel32.ReadDirectoryChangesW
//sys	CertOpenSystemStore(hprov Handle, name *uint16) (store Handle, err error) = crypt32.CertOpenSystemStoreW
//sys   CertOpenStore(storeProvider uintptr, msgAndCertEncodingType uint32, cryptProv uintptr, flags uint32, para uintptr) (handle Handle, err error) = crypt32.CertOpenStore
//sys	CertEnumCertificatesInStore(store Handle, prevContext *CertContext) (context *CertContext, err error) [failretval==nil] = crypt32.CertEnumCertificatesInStore
//sys   CertAddCertificateContextToStore(store Handle, certContext *CertContext, addDisposition uint32, storeContext **CertContext) (err error) = crypt32.CertAddCertificateContextToStore
//sys	CertCloseStore(store Handle, flags uint32) (err error) = crypt32.CertCloseStore
//sys   CertGetCertificateChain(engine Handle, leaf *CertContext, time *Filetime, additionalStore Handle, para *CertChainPara, flags uint32, reserved uintptr, chainCtx **CertChainContext) (err error) = crypt32.CertGetCertificateChain
//sys   CertFreeCertificateChain(ctx *CertChainContext) = crypt32.CertFreeCertificateChain
//sys   CertCreateCertificateContext(certEncodingType uint32, certEncoded *byte, encodedLen uint32) (context *CertContext, err error) [failretval==nil] = crypt32.CertCreateCertificateContext
//sys   CertFreeCertificateContext(ctx *CertContext) (err error) = crypt32.CertFreeCertificateContext
//sys   CertVerifyCertificateChainPolicy(policyOID uintptr, chain *CertChainContext, para *CertChainPolicyPara, status *CertChainPolicyStatus) (err error) = crypt32.CertVerifyCertificateChainPolicy
//sys	RegOpenKeyEx(key Handle, subkey *uint16, options uint32, desiredAccess uint32, result *Handle) (regerrno error) = advapi32.RegOpenKeyExW
//sys	RegCloseKey(key Handle) (regerrno error) = advapi32.RegCloseKey
//sys	RegQueryInfoKey(key Handle, class *uint16, classLen *uint32, reserved *uint32, subkeysLen *uint32, maxSubkeyLen *uint32, maxClassLen *uint32, valuesLen *uint32, maxValueNameLen *uint32, maxValueLen *uint32, saLen *uint32, lastWriteTime *Filetime) (regerrno error) = advapi32.RegQueryInfoKeyW
//sys	regEnumKeyEx(key Handle, index uint32, name *uint16, nameLen *uint32, reserved *uint32, class *uint16, classLen *uint32, lastWriteTime *Filetime) (regerrno error) = advapi32.RegEnumKeyExW
//sys	RegQueryValueEx(key Handle, name *uint16, reserved *uint32, valtype *uint32, buf *byte, buflen *uint32) (regerrno error) = advapi32.RegQueryValueExW
//sys	getCurrentProcessId() (pid uint32) = kernel32.GetCurrentProcessId
//sys	GetConsoleMode(console Handle, mode *uint32) (err error) = kernel32.GetConsoleMode
//sys	WriteConsole(console Handle, buf *uint16, towrite uint32, written *uint32, reserved *byte) (err error) = kernel32.WriteConsoleW
//sys	ReadConsole(console Handle, buf *uint16, toread uint32, read *uint32, inputControl *byte) (err error) = kernel32.ReadConsoleW
//sys	CreateToolhelp32Snapshot(flags uint32, processId uint32) (handle Handle, err error) [failretval==InvalidHandle] = kernel32.CreateToolhelp32Snapshot
//sys	Process32First(snapshot Handle, procEntry *ProcessEntry32) (err error) = kernel32.Process32FirstW
//sys	Process32Next(snapshot Handle, procEntry *ProcessEntry32) (err error) = kernel32.Process32NextW
//sys	DeviceIoControl(handle Handle, ioControlCode uint32, inBuffer *byte, inBufferSize uint32, outBuffer *byte, outBufferSize uint32, bytesReturned *uint32, overlapped *Overlapped) (err error)
// This function returns 1 byte BOOLEAN rather than the 4 byte BOOL.
//sys	CreateSymbolicLink(symlinkfilename *uint16, targetfilename *uint16, flags uint32) (err error) [failretval&0xff==0] = CreateSymbolicLinkW
//sys	CreateHardLink(filename *uint16, existingfilename *uint16, reserved uintptr) (err error) [failretval&0xff==0] = CreateHardLinkW
//sys	initializeProcThreadAttributeList(attrlist *_PROC_THREAD_ATTRIBUTE_LIST, attrcount uint32, flags uint32, size *uintptr) (err error) = InitializeProcThreadAttributeList
//sys	deleteProcThreadAttributeList(attrlist *_PROC_THREAD_ATTRIBUTE_LIST) = DeleteProcThreadAttributeList
//sys	updateProcThreadAttribute(attrlist *_PROC_THREAD_ATTRIBUTE_LIST, flags uint32, attr uintptr, value unsafe.Pointer, size uintptr, prevvalue unsafe.Pointer, returnedsize *uintptr) (err error) = UpdateProcThreadAttribute
//sys	getFinalPathNameByHandle(file Handle, filePath *uint16, filePathSize uint32, flags uint32) (n uint32, err error) [n == 0 || n >= filePathSize] = kernel32.GetFinalPathNameByHandleW
// syscall interface implementation for other packages
internal static ж<SecurityAttributes> makeInheritSa() {
    ref var sa = ref heap(new SecurityAttributes(), out var Ꮡsa);
    sa.Length = ((uint32)@unsafe.Sizeof(sa));
    sa.InheritHandle = 1;
    return Ꮡsa;
}

public static (ΔHandle fd, error err) Open(@string path, nint mode, uint32 perm) {
    ΔHandle fd = default!;
    error err = default!;

    if (len(path) == 0) {
        return (InvalidHandle, ERROR_FILE_NOT_FOUND);
    }
    (pathp, err) = UTF16PtrFromString(path);
    if (err != default!) {
        return (InvalidHandle, err);
    }
    uint32 access = default!;
    var exprᴛ1 = (nint)(mode & ((nint)((UntypedInt)(O_RDONLY | O_WRONLY) | O_RDWR)));
    if (exprᴛ1 == O_RDONLY) {
        access = GENERIC_READ;
    }
    else if (exprᴛ1 == O_WRONLY) {
        access = GENERIC_WRITE;
    }
    else if (exprᴛ1 == O_RDWR) {
        access = (uint32)(GENERIC_READ | GENERIC_WRITE);
    }

    if ((nint)(mode & O_CREAT) != 0) {
        access |= (uint32)(GENERIC_WRITE);
    }
    if ((nint)(mode & O_APPEND) != 0) {
        access &= ~(uint32)(GENERIC_WRITE);
        access |= (uint32)(FILE_APPEND_DATA);
    }
    var sharemode = ((uint32)((uint32)(FILE_SHARE_READ | FILE_SHARE_WRITE)));
    ж<SecurityAttributes> sa = default!;
    if ((nint)(mode & O_CLOEXEC) == 0) {
        sa = makeInheritSa();
    }
    uint32 createmode = default!;
    switch (ᐧ) {
    case {} when (nint)(mode & ((nint)(O_CREAT | O_EXCL))) == ((nint)(O_CREAT | O_EXCL)): {
        createmode = CREATE_NEW;
        break;
    }
    case {} when (nint)(mode & ((nint)(O_CREAT | O_TRUNC))) == ((nint)(O_CREAT | O_TRUNC)): {
        createmode = CREATE_ALWAYS;
        break;
    }
    case {} when (nint)(mode & O_CREAT) == O_CREAT: {
        createmode = OPEN_ALWAYS;
        break;
    }
    case {} when (nint)(mode & O_TRUNC) == O_TRUNC: {
        createmode = TRUNCATE_EXISTING;
        break;
    }
    default: {
        createmode = OPEN_EXISTING;
        break;
    }}

    uint32 attrs = FILE_ATTRIBUTE_NORMAL;
    if ((uint32)(perm & S_IWRITE) == 0) {
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
            var exprᴛ2 = e;
            if (exprᴛ2 == ERROR_FILE_NOT_FOUND || exprᴛ2 == _ERROR_BAD_NETPATH || exprᴛ2 == ERROR_PATH_NOT_FOUND) {
            }
            else { /* default: */
                return (h, e);
            }

        }
    }
    // File does not exist. These are the same
    // errors as Errno.Is checks for ErrNotExist.
    // Carry on to create the file.
    // Success or some different error.
    if (createmode == OPEN_EXISTING && access == GENERIC_READ) {
        // Necessary for opening directory handles.
        attrs |= (uint32)(FILE_FLAG_BACKUP_SEMANTICS);
    }
    if ((nint)(mode & O_SYNC) != 0) {
        static readonly UntypedInt _FILE_FLAG_WRITE_THROUGH = /* 0x80000000 */ 2147483648;
        attrs |= (uint32)(_FILE_FLAG_WRITE_THROUGH);
    }
    return CreateFile(pathp, access, sharemode, sa, createmode, attrs, 0);
}

public static (nint n, error err) Read(ΔHandle fd, slice<byte> p) {
    nint n = default!;
    error err = default!;

    ref var done = ref heap(new uint32(), out var Ꮡdone);
    var e = ReadFile(fd, p, Ꮡdone, nil);
    if (e != default!) {
        if (e == ERROR_BROKEN_PIPE) {
            // NOTE(brainman): work around ERROR_BROKEN_PIPE is returned on reading EOF from stdin
            return (0, default!);
        }
        return (0, e);
    }
    return (((nint)done), default!);
}

public static (nint n, error err) Write(ΔHandle fd, slice<byte> p) {
    nint n = default!;
    error err = default!;

    ref var done = ref heap(new uint32(), out var Ꮡdone);
    var e = WriteFile(fd, p, Ꮡdone, nil);
    if (e != default!) {
        return (0, e);
    }
    return (((nint)done), default!);
}

public static error ReadFile(ΔHandle fd, slice<byte> p, ж<uint32> Ꮡdone, ж<Overlapped> Ꮡoverlapped) {
    ref var done = ref Ꮡdone.val;
    ref var overlapped = ref Ꮡoverlapped.val;

    var err = readFile(fd, p, Ꮡdone, Ꮡoverlapped);
    if (race.Enabled) {
        if (done > 0) {
            race.WriteRange(new @unsafe.Pointer(Ꮡ(p, 0)), ((nint)(done)));
        }
        race.Acquire(new @unsafe.Pointer(Ꮡ(ioSync)));
    }
    if (msan.Enabled && done > 0) {
        msan.Write(new @unsafe.Pointer(Ꮡ(p, 0)), ((uintptr)(done)));
    }
    if (asan.Enabled && done > 0) {
        asan.Write(new @unsafe.Pointer(Ꮡ(p, 0)), ((uintptr)(done)));
    }
    return err;
}

public static error WriteFile(ΔHandle fd, slice<byte> p, ж<uint32> Ꮡdone, ж<Overlapped> Ꮡoverlapped) {
    ref var done = ref Ꮡdone.val;
    ref var overlapped = ref Ꮡoverlapped.val;

    if (race.Enabled) {
        race.ReleaseMerge(new @unsafe.Pointer(Ꮡ(ioSync)));
    }
    var err = writeFile(fd, p, Ꮡdone, Ꮡoverlapped);
    if (race.Enabled && done > 0) {
        race.ReadRange(new @unsafe.Pointer(Ꮡ(p, 0)), ((nint)(done)));
    }
    if (msan.Enabled && done > 0) {
        msan.Read(new @unsafe.Pointer(Ꮡ(p, 0)), ((uintptr)(done)));
    }
    if (asan.Enabled && done > 0) {
        asan.Read(new @unsafe.Pointer(Ꮡ(p, 0)), ((uintptr)(done)));
    }
    return err;
}

internal static int64 ioSync;

internal static ж<LazyProc> procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx"u8);

internal const uintptr ptrSize = /* unsafe.Sizeof(uintptr(0)) */ 8;

// setFilePointerEx calls SetFilePointerEx.
// See https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-setfilepointerex
internal static error setFilePointerEx(ΔHandle handle, int64 distToMove, ж<int64> ᏑnewFilePointer, uint32 whence) {
    ref var newFilePointer = ref ᏑnewFilePointer.val;

    Errno e1 = default!;
    if (@unsafe.Sizeof(((uintptr)0)) == 8){
        (_, _, e1) = Syscall6(procSetFilePointerEx.Addr(), 4, ((uintptr)handle), ((uintptr)distToMove), ((uintptr)new @unsafe.Pointer(ᏑnewFilePointer)), ((uintptr)whence), 0, 0);
    } else {
        // Different 32-bit systems disgaree about whether distToMove starts 8-byte aligned.
        var exprᴛ1 = runtime.GOARCH;
        { /* default: */
            throw panic("unsupported 32-bit architecture");
        }
        else if (exprᴛ1 == "386"u8) {
            (_, _, e1) = Syscall6(procSetFilePointerEx.Addr(), // distToMove is a LARGE_INTEGER, which is 64 bits.
 5, ((uintptr)handle), ((uintptr)distToMove), ((uintptr)(distToMove >> (int)(32))), ((uintptr)new @unsafe.Pointer(ᏑnewFilePointer)), ((uintptr)whence), 0);
        }
        else if (exprᴛ1 == "arm"u8) {
            (_, _, e1) = Syscall6(procSetFilePointerEx.Addr(), // distToMove must be 8-byte aligned per ARM calling convention
 // https://docs.microsoft.com/en-us/cpp/build/overview-of-arm-abi-conventions#stage-c-assignment-of-arguments-to-registers-and-stack
 6, ((uintptr)handle), 0, ((uintptr)distToMove), ((uintptr)(distToMove >> (int)(32))), ((uintptr)new @unsafe.Pointer(ᏑnewFilePointer)), ((uintptr)whence));
        }

    }
    if (e1 != 0) {
        return errnoErr(e1);
    }
    return default!;
}

public static (int64 newoffset, error err) Seek(ΔHandle fd, int64 offset, nint whence) {
    int64 newoffset = default!;
    error err = default!;

    uint32 w = default!;
    switch (whence) {
    case 0: {
        w = FILE_BEGIN;
        break;
    }
    case 1: {
        w = FILE_CURRENT;
        break;
    }
    case 2: {
        w = FILE_END;
        break;
    }}

    err = setFilePointerEx(fd, offset, Ꮡ(newoffset), w);
    return (newoffset, err);
}

public static error /*err*/ Close(ΔHandle fd) {
    error err = default!;

    return CloseHandle(fd);
}

public static ΔHandle Stdin = getStdHandle(STD_INPUT_HANDLE);
public static ΔHandle Stdout = getStdHandle(STD_OUTPUT_HANDLE);
public static ΔHandle Stderr = getStdHandle(STD_ERROR_HANDLE);

internal static ΔHandle /*fd*/ getStdHandle(nint h) {
    ΔHandle fd = default!;

    var (r, _) = GetStdHandle(h);
    return r;
}

public const bool ImplementsGetwd = true;

public static (@string wd, error err) Getwd() {
    @string wd = default!;
    error err = default!;

    var b = new slice<uint16>(300);
    // The path of the current directory may not fit in the initial 300-word
    // buffer when long path support is enabled. The current directory may also
    // change between subsequent calls of GetCurrentDirectory. As a result, we
    // need to retry the call in a loop until the current directory fits, each
    // time with a bigger buffer.
    while (ᐧ) {
        var (n, e) = GetCurrentDirectory(((uint32)len(b)), Ꮡ(b, 0));
        if (e != default!) {
            return ("", e);
        }
        if (((nint)n) <= len(b)) {
            return (UTF16ToString(b[..(int)(n)]), default!);
        }
        b = new slice<uint16>(n);
    }
}

public static error /*err*/ Chdir(@string path) {
    error err = default!;

    (pathp, err) = UTF16PtrFromString(path);
    if (err != default!) {
        return err;
    }
    return SetCurrentDirectory(pathp);
}

public static error /*err*/ Mkdir(@string path, uint32 mode) {
    error err = default!;

    (pathp, err) = UTF16PtrFromString(path);
    if (err != default!) {
        return err;
    }
    return CreateDirectory(pathp, nil);
}

public static error /*err*/ Rmdir(@string path) {
    error err = default!;

    (pathp, err) = UTF16PtrFromString(path);
    if (err != default!) {
        return err;
    }
    return RemoveDirectory(pathp);
}

public static error /*err*/ Unlink(@string path) {
    error err = default!;

    (pathp, err) = UTF16PtrFromString(path);
    if (err != default!) {
        return err;
    }
    return DeleteFile(pathp);
}

public static error /*err*/ Rename(@string oldpath, @string newpath) {
    error err = default!;

    (from, err) = UTF16PtrFromString(oldpath);
    if (err != default!) {
        return err;
    }
    (to, err) = UTF16PtrFromString(newpath);
    if (err != default!) {
        return err;
    }
    return MoveFile(from, to);
}

public static (@string name, error err) ComputerName() {
    @string name = default!;
    error err = default!;

    ref var n = ref heap(new uint32(), out var Ꮡn);
    n = MAX_COMPUTERNAME_LENGTH + 1;
    var b = new slice<uint16>(n);
    var e = GetComputerName(Ꮡ(b, 0), Ꮡn);
    if (e != default!) {
        return ("", e);
    }
    return (UTF16ToString(b[..(int)(n)]), default!);
}

public static error /*err*/ Ftruncate(ΔHandle fd, int64 length) => func((defer, _) => {
    error err = default!;

    var (curoffset, e) = Seek(fd, 0, 1);
    if (e != default!) {
        return e;
    }
    deferǃ(Seek, fd, curoffset, 0, defer);
    (_, e) = Seek(fd, length, 0);
    if (e != default!) {
        return e;
    }
    e = SetEndOfFile(fd);
    if (e != default!) {
        return e;
    }
    return default!;
});

public static error /*err*/ Gettimeofday(ж<Timeval> Ꮡtv) {
    error err = default!;

    ref var tv = ref Ꮡtv.val;
    ref var ft = ref heap(new Filetime(), out var Ꮡft);
    GetSystemTimeAsFileTime(Ꮡft);
    tv = NsecToTimeval(ft.Nanoseconds());
    return default!;
}

public static error /*err*/ Pipe(slice<ΔHandle> p) {
    error err = default!;

    if (len(p) != 2) {
        return EINVAL;
    }
    ref var r = ref heap(new ΔHandle(), out var Ꮡr);
    ref var w = ref heap(new ΔHandle(), out var Ꮡw);
    var e = CreatePipe(Ꮡr, Ꮡw, makeInheritSa(), 0);
    if (e != default!) {
        return e;
    }
    p[0] = r;
    p[1] = w;
    return default!;
}

public static error /*err*/ Utimes(@string path, slice<Timeval> tv) => func((defer, _) => {
    error err = default!;

    if (len(tv) != 2) {
        return EINVAL;
    }
    (pathp, e) = UTF16PtrFromString(path);
    if (e != default!) {
        return e;
    }
    var (h, e) = CreateFile(pathp,
        FILE_WRITE_ATTRIBUTES, FILE_SHARE_WRITE, nil,
        OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, 0);
    if (e != default!) {
        return e;
    }
    deferǃ(Close, h, defer);
    ref var a = ref heap<Filetime>(out var Ꮡa);
    a = new Filetime(nil);
    ref var w = ref heap<Filetime>(out var Ꮡw);
    w = new Filetime(nil);
    if (tv[0].Nanoseconds() != 0) {
        a = NsecToFiletime(tv[0].Nanoseconds());
    }
    if (tv[0].Nanoseconds() != 0) {
        w = NsecToFiletime(tv[1].Nanoseconds());
    }
    return SetFileTime(h, nil, Ꮡa, Ꮡw);
});

// This matches the value in os/file_windows.go.
internal static readonly GoUntyped _UTIME_OMIT = /* -1 */
    GoUntyped.Parse("-1");

public static error /*err*/ UtimesNano(@string path, slice<Timespec> ts) => func((defer, _) => {
    error err = default!;

    if (len(ts) != 2) {
        return EINVAL;
    }
    (pathp, e) = UTF16PtrFromString(path);
    if (e != default!) {
        return e;
    }
    var (h, e) = CreateFile(pathp,
        FILE_WRITE_ATTRIBUTES, FILE_SHARE_WRITE, nil,
        OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, 0);
    if (e != default!) {
        return e;
    }
    deferǃ(Close, h, defer);
    ref var a = ref heap<Filetime>(out var Ꮡa);
    a = new Filetime(nil);
    ref var w = ref heap<Filetime>(out var Ꮡw);
    w = new Filetime(nil);
    if (ts[0].Nsec != _UTIME_OMIT) {
        a = NsecToFiletime(TimespecToNsec(ts[0]));
    }
    if (ts[1].Nsec != _UTIME_OMIT) {
        w = NsecToFiletime(TimespecToNsec(ts[1]));
    }
    return SetFileTime(h, nil, Ꮡa, Ꮡw);
});

public static error /*err*/ Fsync(ΔHandle fd) {
    error err = default!;

    return FlushFileBuffers(fd);
}

public static error /*err*/ Chmod(@string path, uint32 mode) {
    error err = default!;

    (p, e) = UTF16PtrFromString(path);
    if (e != default!) {
        return e;
    }
    var (attrs, e) = GetFileAttributes(p);
    if (e != default!) {
        return e;
    }
    if ((uint32)(mode & S_IWRITE) != 0){
        attrs &= ~(uint32)(FILE_ATTRIBUTE_READONLY);
    } else {
        attrs |= (uint32)(FILE_ATTRIBUTE_READONLY);
    }
    return SetFileAttributes(p, attrs);
}

public static error LoadCancelIoEx() {
    return procCancelIoEx.Find();
}

public static error LoadSetFileCompletionNotificationModes() {
    return procSetFileCompletionNotificationModes.Find();
}

// net api calls
internal const uintptr socket_error = /* uintptr(^uint32(0)) */ 4294967295;

//sys	WSAStartup(verreq uint32, data *WSAData) (sockerr error) = ws2_32.WSAStartup
//sys	WSACleanup() (err error) [failretval==socket_error] = ws2_32.WSACleanup
//sys	WSAIoctl(s Handle, iocc uint32, inbuf *byte, cbif uint32, outbuf *byte, cbob uint32, cbbr *uint32, overlapped *Overlapped, completionRoutine uintptr) (err error) [failretval==socket_error] = ws2_32.WSAIoctl
//sys	socket(af int32, typ int32, protocol int32) (handle Handle, err error) [failretval==InvalidHandle] = ws2_32.socket
//sys	Setsockopt(s Handle, level int32, optname int32, optval *byte, optlen int32) (err error) [failretval==socket_error] = ws2_32.setsockopt
//sys	Getsockopt(s Handle, level int32, optname int32, optval *byte, optlen *int32) (err error) [failretval==socket_error] = ws2_32.getsockopt
//sys	bind(s Handle, name unsafe.Pointer, namelen int32) (err error) [failretval==socket_error] = ws2_32.bind
//sys	connect(s Handle, name unsafe.Pointer, namelen int32) (err error) [failretval==socket_error] = ws2_32.connect
//sys	getsockname(s Handle, rsa *RawSockaddrAny, addrlen *int32) (err error) [failretval==socket_error] = ws2_32.getsockname
//sys	getpeername(s Handle, rsa *RawSockaddrAny, addrlen *int32) (err error) [failretval==socket_error] = ws2_32.getpeername
//sys	listen(s Handle, backlog int32) (err error) [failretval==socket_error] = ws2_32.listen
//sys	shutdown(s Handle, how int32) (err error) [failretval==socket_error] = ws2_32.shutdown
//sys	Closesocket(s Handle) (err error) [failretval==socket_error] = ws2_32.closesocket
//sys	AcceptEx(ls Handle, as Handle, buf *byte, rxdatalen uint32, laddrlen uint32, raddrlen uint32, recvd *uint32, overlapped *Overlapped) (err error) = mswsock.AcceptEx
//sys	GetAcceptExSockaddrs(buf *byte, rxdatalen uint32, laddrlen uint32, raddrlen uint32, lrsa **RawSockaddrAny, lrsalen *int32, rrsa **RawSockaddrAny, rrsalen *int32) = mswsock.GetAcceptExSockaddrs
//sys	WSARecv(s Handle, bufs *WSABuf, bufcnt uint32, recvd *uint32, flags *uint32, overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSARecv
//sys	WSASend(s Handle, bufs *WSABuf, bufcnt uint32, sent *uint32, flags uint32, overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSASend
//sys	WSARecvFrom(s Handle, bufs *WSABuf, bufcnt uint32, recvd *uint32, flags *uint32,  from *RawSockaddrAny, fromlen *int32, overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSARecvFrom
//sys	WSASendTo(s Handle, bufs *WSABuf, bufcnt uint32, sent *uint32, flags uint32, to *RawSockaddrAny, tolen int32,  overlapped *Overlapped, croutine *byte) (err error) [failretval==socket_error] = ws2_32.WSASendTo
//sys	GetHostByName(name string) (h *Hostent, err error) [failretval==nil] = ws2_32.gethostbyname
//sys	GetServByName(name string, proto string) (s *Servent, err error) [failretval==nil] = ws2_32.getservbyname
//sys	Ntohs(netshort uint16) (u uint16) = ws2_32.ntohs
//sys	GetProtoByName(name string) (p *Protoent, err error) [failretval==nil] = ws2_32.getprotobyname
//sys	DnsQuery(name string, qtype uint16, options uint32, extra *byte, qrs **DNSRecord, pr *byte) (status error) = dnsapi.DnsQuery_W
//sys	DnsRecordListFree(rl *DNSRecord, freetype uint32) = dnsapi.DnsRecordListFree
//sys	DnsNameCompare(name1 *uint16, name2 *uint16) (same bool) = dnsapi.DnsNameCompare_W
//sys	GetAddrInfoW(nodename *uint16, servicename *uint16, hints *AddrinfoW, result **AddrinfoW) (sockerr error) = ws2_32.GetAddrInfoW
//sys	FreeAddrInfoW(addrinfo *AddrinfoW) = ws2_32.FreeAddrInfoW
//sys	GetIfEntry(pIfRow *MibIfRow) (errcode error) = iphlpapi.GetIfEntry
//sys	GetAdaptersInfo(ai *IpAdapterInfo, ol *uint32) (errcode error) = iphlpapi.GetAdaptersInfo
//sys	SetFileCompletionNotificationModes(handle Handle, flags uint8) (err error) = kernel32.SetFileCompletionNotificationModes
//sys	WSAEnumProtocols(protocols *int32, protocolBuffer *WSAProtocolInfo, bufferLength *uint32) (n int32, err error) [failretval==-1] = ws2_32.WSAEnumProtocolsW

// For testing: clients can set this flag to force
// creation of IPv6 sockets to return [EAFNOSUPPORT].
public static bool SocketDisableIPv6;

[GoType] partial struct RawSockaddrInet4 {
    public uint16 Family;
    public uint16 Port;
    public array<byte> Addr = new(4); /* in_addr */
    public array<uint8> Zero = new(8);
}

[GoType] partial struct RawSockaddrInet6 {
    public uint16 Family;
    public uint16 Port;
    public uint32 Flowinfo;
    public array<byte> Addr = new(16); /* in6_addr */
    public uint32 Scope_id;
}

[GoType] partial struct RawSockaddr {
    public uint16 Family;
    public array<int8> Data = new(14);
}

[GoType] partial struct RawSockaddrAny {
    public RawSockaddr Addr;
    public array<int8> Pad = new(100);
}

[GoType] partial interface ΔSockaddr {
    (@unsafe.Pointer ptr, int32 len, error err) sockaddr(); // lowercase; only we can define Sockaddrs
}

[GoType] partial struct SockaddrInet4 {
    public nint Port;
    public array<byte> Addr = new(4);
    internal RawSockaddrInet4 raw;
}

[GoRecv] internal static (@unsafe.Pointer, int32, error) sockaddr(this ref SockaddrInet4 sa) {
    if (sa.Port < 0 || sa.Port > 65535) {
        return (default!, 0, EINVAL);
    }
    sa.raw.Family = AF_INET;
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡsa.raw.of(RawSockaddrInet4.ᏑPort)));
    p.val[0] = ((byte)(sa.Port >> (int)(8)));
    p.val[1] = ((byte)sa.Port);
    sa.raw.Addr = sa.Addr;
    return (new @unsafe.Pointer(Ꮡ(sa.raw)), ((int32)@unsafe.Sizeof(sa.raw)), default!);
}

[GoType] partial struct SockaddrInet6 {
    public nint Port;
    public uint32 ZoneId;
    public array<byte> Addr = new(16);
    internal RawSockaddrInet6 raw;
}

[GoRecv] internal static (@unsafe.Pointer, int32, error) sockaddr(this ref SockaddrInet6 sa) {
    if (sa.Port < 0 || sa.Port > 65535) {
        return (default!, 0, EINVAL);
    }
    sa.raw.Family = AF_INET6;
    var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡsa.raw.of(RawSockaddrInet6.ᏑPort)));
    p.val[0] = ((byte)(sa.Port >> (int)(8)));
    p.val[1] = ((byte)sa.Port);
    sa.raw.Scope_id = sa.ZoneId;
    sa.raw.Addr = sa.Addr;
    return (new @unsafe.Pointer(Ꮡ(sa.raw)), ((int32)@unsafe.Sizeof(sa.raw)), default!);
}

[GoType] partial struct RawSockaddrUnix {
    public uint16 Family;
    public array<int8> Path = new(UNIX_PATH_MAX);
}

[GoType] partial struct SockaddrUnix {
    public @string Name;
    internal RawSockaddrUnix raw;
}

[GoRecv] internal static (@unsafe.Pointer, int32, error) sockaddr(this ref SockaddrUnix sa) {
    @string name = sa.Name;
    nint n = len(name);
    if (n > len(sa.raw.Path)) {
        return (default!, 0, EINVAL);
    }
    if (n == len(sa.raw.Path) && name[0] != (rune)'@') {
        return (default!, 0, EINVAL);
    }
    sa.raw.Family = AF_UNIX;
    for (nint i = 0; i < n; i++) {
        sa.raw.Path[i] = ((int8)name[i]);
    }
    // length is family (uint16), name, NUL.
    var sl = ((int32)2);
    if (n > 0) {
        sl += ((int32)n) + 1;
    }
    if (sa.raw.Path[0] == (rune)'@' || (sa.raw.Path[0] == 0 && sl > 3)) {
        // Check sl > 3 so we don't change unnamed socket behavior.
        sa.raw.Path[0] = 0;
        // Don't count trailing NUL for abstract address.
        sl--;
    }
    return (new @unsafe.Pointer(Ꮡ(sa.raw)), sl, default!);
}

[GoRecv] public static (ΔSockaddr, error) Sockaddr(this ref RawSockaddrAny rsa) {
    var exprᴛ1 = rsa.Addr.Family;
    if (exprᴛ1 == AF_UNIX) {
        var pp = (ж<RawSockaddrUnix>)(uintptr)(@unsafe.Pointer.FromRef(ref rsa));
        var sa = @new<SockaddrUnix>();
        if ((~pp).Path[0] == 0) {
            // "Abstract" Unix domain socket.
            // Rewrite leading NUL as @ for textual display.
            // (This is the standard convention.)
            // Not friendly to overwrite in place,
            // but the callers below don't care.
            (~pp).Path[0] = (rune)'@';
        }
        nint n = 0;
        while (n < len((~pp).Path) && (~pp).Path[n] != 0) {
            // Assume path ends at NUL.
            // This is not technically the Linux semantics for
            // abstract Unix domain sockets--they are supposed
            // to be uninterpreted fixed-size binary blobs--but
            // everyone uses this convention.
            n++;
        }
        sa.val.Name = ((@string)@unsafe.Slice((ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡ(~pp).Path.at<int8>(0))), n));
        return (~sa, default!);
    }
    if (exprᴛ1 == AF_INET) {
        var pp = (ж<RawSockaddrInet4>)(uintptr)(@unsafe.Pointer.FromRef(ref rsa));
        var sa = @new<SockaddrInet4>();
        var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡ((~pp).Port)));
        sa.val.Port = ((nint)p.val[0]) << (int)(8) + ((nint)p.val[1]);
        sa.val.Addr = pp.val.Addr;
        return (~sa, default!);
    }
    if (exprᴛ1 == AF_INET6) {
        var pp = (ж<RawSockaddrInet6>)(uintptr)(@unsafe.Pointer.FromRef(ref rsa));
        var sa = @new<SockaddrInet6>();
        var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡ((~pp).Port)));
        sa.val.Port = ((nint)p.val[0]) << (int)(8) + ((nint)p.val[1]);
        sa.val.ZoneId = pp.val.Scope_id;
        sa.val.Addr = pp.val.Addr;
        return (~sa, default!);
    }

    return (default!, EAFNOSUPPORT);
}

public static (ΔHandle fd, error err) Socket(nint domain, nint typ, nint proto) {
    ΔHandle fd = default!;
    error err = default!;

    if (domain == AF_INET6 && SocketDisableIPv6) {
        return (InvalidHandle, EAFNOSUPPORT);
    }
    return socket(((int32)domain), ((int32)typ), ((int32)proto));
}

public static error /*err*/ SetsockoptInt(ΔHandle fd, nint level, nint opt, nint value) {
    error err = default!;

    ref var v = ref heap<int32>(out var Ꮡv);
    v = ((int32)value);
    return Setsockopt(fd, ((int32)level), ((int32)opt), (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡv)), ((int32)@unsafe.Sizeof(v)));
}

public static error /*err*/ Bind(ΔHandle fd, ΔSockaddr sa) {
    error err = default!;

    var (ptr, n, err) = sa.sockaddr();
    if (err != default!) {
        return err;
    }
    return bind(fd, ptr, n);
}

public static error /*err*/ Connect(ΔHandle fd, ΔSockaddr sa) {
    error err = default!;

    var (ptr, n, err) = sa.sockaddr();
    if (err != default!) {
        return err;
    }
    return connect(fd, ptr, n);
}

public static (ΔSockaddr sa, error err) Getsockname(ΔHandle fd) {
    ΔSockaddr sa = default!;
    error err = default!;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var l = ref heap<int32>(out var Ꮡl);
    l = ((int32)@unsafe.Sizeof(rsa));
    {
        err = getsockname(fd, Ꮡrsa, Ꮡl); if (err != default!) {
            return (sa, err);
        }
    }
    return rsa.Sockaddr();
}

public static (ΔSockaddr sa, error err) Getpeername(ΔHandle fd) {
    ΔSockaddr sa = default!;
    error err = default!;

    ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa);
    ref var l = ref heap<int32>(out var Ꮡl);
    l = ((int32)@unsafe.Sizeof(rsa));
    {
        err = getpeername(fd, Ꮡrsa, Ꮡl); if (err != default!) {
            return (sa, err);
        }
    }
    return rsa.Sockaddr();
}

public static error /*err*/ Listen(ΔHandle s, nint n) {
    error err = default!;

    return listen(s, ((int32)n));
}

public static error /*err*/ Shutdown(ΔHandle fd, nint how) {
    error err = default!;

    return shutdown(fd, ((int32)how));
}

public static error /*err*/ WSASendto(ΔHandle s, ж<WSABuf> Ꮡbufs, uint32 bufcnt, ж<uint32> Ꮡsent, uint32 flags, ΔSockaddr to, ж<Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
    error err = default!;

    ref var bufs = ref Ꮡbufs.val;
    ref var sent = ref Ꮡsent.val;
    ref var overlapped = ref Ꮡoverlapped.val;
    ref var croutine = ref Ꮡcroutine.val;
    @unsafe.Pointer rsa = default!;
    int32 len = default!;
    if (to != default!) {
        (rsa, len, err) = to.sockaddr();
        if (err != default!) {
            return err;
        }
    }
    var (r1, _, e1) = Syscall9(procWSASendTo.Addr(), 9, ((uintptr)s), ((uintptr)new @unsafe.Pointer(Ꮡbufs)), ((uintptr)bufcnt), ((uintptr)new @unsafe.Pointer(Ꮡsent)), ((uintptr)flags), ((uintptr)((@unsafe.Pointer)rsa)), ((uintptr)len), ((uintptr)new @unsafe.Pointer(Ꮡoverlapped)), ((uintptr)new @unsafe.Pointer(Ꮡcroutine)));
    if (r1 == socket_error) {
        if (e1 != 0){
            err = errnoErr(e1);
        } else {
            err = EINVAL;
        }
    }
    return err;
}

internal static error /*err*/ wsaSendtoInet4(ΔHandle s, ж<WSABuf> Ꮡbufs, uint32 bufcnt, ж<uint32> Ꮡsent, uint32 flags, ж<SockaddrInet4> Ꮡto, ж<Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
    error err = default!;

    ref var bufs = ref Ꮡbufs.val;
    ref var sent = ref Ꮡsent.val;
    ref var to = ref Ꮡto.val;
    ref var overlapped = ref Ꮡoverlapped.val;
    ref var croutine = ref Ꮡcroutine.val;
    var (rsa, len, err) = to.sockaddr();
    if (err != default!) {
        return err;
    }
    var (r1, _, e1) = Syscall9(procWSASendTo.Addr(), 9, ((uintptr)s), ((uintptr)new @unsafe.Pointer(Ꮡbufs)), ((uintptr)bufcnt), ((uintptr)new @unsafe.Pointer(Ꮡsent)), ((uintptr)flags), ((uintptr)((@unsafe.Pointer)rsa)), ((uintptr)len), ((uintptr)new @unsafe.Pointer(Ꮡoverlapped)), ((uintptr)new @unsafe.Pointer(Ꮡcroutine)));
    if (r1 == socket_error) {
        if (e1 != 0){
            err = errnoErr(e1);
        } else {
            err = EINVAL;
        }
    }
    return err;
}

internal static error /*err*/ wsaSendtoInet6(ΔHandle s, ж<WSABuf> Ꮡbufs, uint32 bufcnt, ж<uint32> Ꮡsent, uint32 flags, ж<SockaddrInet6> Ꮡto, ж<Overlapped> Ꮡoverlapped, ж<byte> Ꮡcroutine) {
    error err = default!;

    ref var bufs = ref Ꮡbufs.val;
    ref var sent = ref Ꮡsent.val;
    ref var to = ref Ꮡto.val;
    ref var overlapped = ref Ꮡoverlapped.val;
    ref var croutine = ref Ꮡcroutine.val;
    var (rsa, len, err) = to.sockaddr();
    if (err != default!) {
        return err;
    }
    var (r1, _, e1) = Syscall9(procWSASendTo.Addr(), 9, ((uintptr)s), ((uintptr)new @unsafe.Pointer(Ꮡbufs)), ((uintptr)bufcnt), ((uintptr)new @unsafe.Pointer(Ꮡsent)), ((uintptr)flags), ((uintptr)((@unsafe.Pointer)rsa)), ((uintptr)len), ((uintptr)new @unsafe.Pointer(Ꮡoverlapped)), ((uintptr)new @unsafe.Pointer(Ꮡcroutine)));
    if (r1 == socket_error) {
        if (e1 != 0){
            err = errnoErr(e1);
        } else {
            err = EINVAL;
        }
    }
    return err;
}

public static error LoadGetAddrInfo() {
    return procGetAddrInfoW.Find();
}


[GoType("dyn")] partial struct connectExFuncᴛ1 {
    internal sync_package.Once once;
    internal uintptr addr;
    internal error err;
}
internal static connectExFuncᴛ1 connectExFunc;

public static error LoadConnectEx() => func((defer, _) => {
    connectExFunc.once.Do(
    var WSAID_CONNECTEXʗ2 = WSAID_CONNECTEX;
    var connectExFuncʗ2 = connectExFunc;
    () => {
        ΔHandle s = default!;
        (s, connectExFuncʗ2.err) = Socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
        if (connectExFuncʗ2.err != default!) {
            return;
        }
        deferǃ(CloseHandle, s, defer);
        ref var n = ref heap(new uint32(), out var Ꮡn);
        connectExFunc.err = WSAIoctl(s,
            SIO_GET_EXTENSION_FUNCTION_POINTER,
            (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡ(WSAID_CONNECTEX))),
            ((uint32)@unsafe.Sizeof(WSAID_CONNECTEX)),
            (ж<byte>)(uintptr)(((@unsafe.Pointer)(ᏑconnectExFunc.of(connectExFuncᴛ1.Ꮡaddr)))),
            ((uint32)@unsafe.Sizeof(connectExFunc.addr)),
            Ꮡn, nil, 0);
    });
    return connectExFunc.err;
});

internal static error /*err*/ connectEx(ΔHandle s, @unsafe.Pointer name, int32 namelen, ж<byte> ᏑsendBuf, uint32 sendDataLen, ж<uint32> ᏑbytesSent, ж<Overlapped> Ꮡoverlapped) {
    error err = default!;

    ref var sendBuf = ref ᏑsendBuf.val;
    ref var bytesSent = ref ᏑbytesSent.val;
    ref var overlapped = ref Ꮡoverlapped.val;
    var (r1, _, e1) = Syscall9(connectExFunc.addr, 7, ((uintptr)s), ((uintptr)name), ((uintptr)namelen), ((uintptr)new @unsafe.Pointer(ᏑsendBuf)), ((uintptr)sendDataLen), ((uintptr)new @unsafe.Pointer(ᏑbytesSent)), ((uintptr)new @unsafe.Pointer(Ꮡoverlapped)), 0, 0);
    if (r1 == 0) {
        if (e1 != 0){
            err = ((error)e1);
        } else {
            err = EINVAL;
        }
    }
    return err;
}

public static error ConnectEx(ΔHandle fd, ΔSockaddr sa, ж<byte> ᏑsendBuf, uint32 sendDataLen, ж<uint32> ᏑbytesSent, ж<Overlapped> Ꮡoverlapped) {
    ref var sendBuf = ref ᏑsendBuf.val;
    ref var bytesSent = ref ᏑbytesSent.val;
    ref var overlapped = ref Ꮡoverlapped.val;

    var err = LoadConnectEx();
    if (err != default!) {
        return errorspkg.New("failed to find ConnectEx: "u8 + err.Error());
    }
    var (ptr, n, err) = sa.sockaddr();
    if (err != default!) {
        return err;
    }
    return connectEx(fd, ptr, n, ᏑsendBuf, sendDataLen, ᏑbytesSent, Ꮡoverlapped);
}

// Invented structures to support what package os expects.
[GoType] partial struct Rusage {
    public Filetime CreationTime;
    public Filetime ExitTime;
    public Filetime KernelTime;
    public Filetime UserTime;
}

[GoType] partial struct WaitStatus {
    public uint32 ExitCode;
}

public static bool Exited(this WaitStatus w) {
    return true;
}

public static nint ExitStatus(this WaitStatus w) {
    return ((nint)w.ExitCode);
}

public static ΔSignal Signal(this WaitStatus w) {
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

public static ΔSignal StopSignal(this WaitStatus w) {
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
[GoType] partial struct Timespec {
    public int64 Sec;
    public int64 Nsec;
}

public static int64 TimespecToNsec(Timespec ts) {
    return ((int64)ts.Sec) * 1e9F + ((int64)ts.Nsec);
}

public static Timespec /*ts*/ NsecToTimespec(int64 nsec) {
    Timespec ts = default!;

    ts.Sec = nsec / 1e9F;
    ts.Nsec = nsec % 1e9F;
    return ts;
}

// TODO(brainman): fix all needed for net
public static (ΔHandle nfd, ΔSockaddr sa, error err) Accept(ΔHandle fd) {
    ΔHandle nfd = default!;
    ΔSockaddr sa = default!;
    error err = default!;

    return (0, default!, EWINDOWS);
}

public static (nint n, ΔSockaddr from, error err) Recvfrom(ΔHandle fd, slice<byte> p, nint flags) {
    nint n = default!;
    ΔSockaddr from = default!;
    error err = default!;

    return (0, default!, EWINDOWS);
}

public static error /*err*/ Sendto(ΔHandle fd, slice<byte> p, nint flags, ΔSockaddr to) {
    error err = default!;

    return EWINDOWS;
}

public static error /*err*/ SetsockoptTimeval(ΔHandle fd, nint level, nint opt, ж<Timeval> Ꮡtv) {
    error err = default!;

    ref var tv = ref Ꮡtv.val;
    return EWINDOWS;
}

// The Linger struct is wrong but we only noticed after Go 1.
// sysLinger is the real system call structure.
// BUG(brainman): The definition of Linger is not appropriate for direct use
// with Setsockopt and Getsockopt.
// Use SetsockoptLinger instead.
[GoType] partial struct Linger {
    public int32 Onoff;
    public int32 Linger;
}

[GoType] partial struct sysLinger {
    public uint16 Onoff;
    public uint16 Linger;
}

[GoType] partial struct IPMreq {
    public array<byte> Multiaddr = new(4); /* in_addr */
    public array<byte> Interface = new(4); /* in_addr */
}

[GoType] partial struct IPv6Mreq {
    public array<byte> Multiaddr = new(16); /* in6_addr */
    public uint32 Interface;
}

public static (nint, error) GetsockoptInt(ΔHandle fd, nint level, nint opt) {
    ref var optval = ref heap<int32>(out var Ꮡoptval);
    optval = ((int32)0);
    ref var optlen = ref heap<int32>(out var Ꮡoptlen);
    optlen = ((int32)@unsafe.Sizeof(optval));
    var err = Getsockopt(fd, ((int32)level), ((int32)opt), (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡoptval)), Ꮡoptlen);
    return (((nint)optval), err);
}

public static error /*err*/ SetsockoptLinger(ΔHandle fd, nint level, nint opt, ж<Linger> Ꮡl) {
    error err = default!;

    ref var l = ref Ꮡl.val;
    ref var sys = ref heap<sysLinger>(out var Ꮡsys);
    sys = new sysLinger(Onoff: ((uint16)l.Onoff), Linger: ((uint16)l.Linger));
    return Setsockopt(fd, ((int32)level), ((int32)opt), (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡsys)), ((int32)@unsafe.Sizeof(sys)));
}

public static error /*err*/ SetsockoptInet4Addr(ΔHandle fd, nint level, nint opt, array<byte> value) {
    error err = default!;

    value = value.Clone();
    return Setsockopt(fd, ((int32)level), ((int32)opt), (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡvalue.at<byte>(0))), 4);
}

public static error /*err*/ SetsockoptIPMreq(ΔHandle fd, nint level, nint opt, ж<IPMreq> Ꮡmreq) {
    error err = default!;

    ref var mreq = ref Ꮡmreq.val;
    return Setsockopt(fd, ((int32)level), ((int32)opt), (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡmreq)), ((int32)@unsafe.Sizeof(mreq)));
}

public static error /*err*/ SetsockoptIPv6Mreq(ΔHandle fd, nint level, nint opt, ж<IPv6Mreq> Ꮡmreq) {
    error err = default!;

    ref var mreq = ref Ꮡmreq.val;
    return EWINDOWS;
}

public static nint /*pid*/ Getpid() {
    nint pid = default!;

    return ((nint)getCurrentProcessId());
}

public static (ΔHandle handle, error err) FindFirstFile(ж<uint16> Ꮡname, ж<Win32finddata> Ꮡdata) {
    ΔHandle handle = default!;
    error err = default!;

    ref var name = ref Ꮡname.val;
    ref var data = ref Ꮡdata.val;
    // NOTE(rsc): The Win32finddata struct is wrong for the system call:
    // the two paths are each one uint16 short. Use the correct struct,
    // a win32finddata1, and then copy the results out.
    // There is no loss of expressivity here, because the final
    // uint16, if it is used, is supposed to be a NUL, and Go doesn't need that.
    // For Go 1.1, we might avoid the allocation of win32finddata1 here
    // by adding a final Bug [2]uint16 field to the struct and then
    // adjusting the fields in the result directly.
    ref var data1 = ref heap(new win32finddata1(), out var Ꮡdata1);
    (handle, err) = findFirstFile1(Ꮡname, Ꮡdata1);
    if (err == default!) {
        copyFindData(Ꮡdata, Ꮡdata1);
    }
    return (handle, err);
}

public static error /*err*/ FindNextFile(ΔHandle handle, ж<Win32finddata> Ꮡdata) {
    error err = default!;

    ref var data = ref Ꮡdata.val;
    ref var data1 = ref heap(new win32finddata1(), out var Ꮡdata1);
    err = findNextFile1(handle, Ꮡdata1);
    if (err == default!) {
        copyFindData(Ꮡdata, Ꮡdata1);
    }
    return err;
}

internal static (ж<ProcessEntry32>, error) getProcessEntry(nint pid) => func((defer, _) => {
    var (snapshot, err) = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (err != default!) {
        return (default!, err);
    }
    deferǃ(CloseHandle, snapshot, defer);
    ref var procEntry = ref heap(new ProcessEntry32(), out var ᏑprocEntry);
    procEntry.Size = ((uint32)@unsafe.Sizeof(procEntry));
    {
        err = Process32First(snapshot, ᏑprocEntry); if (err != default!) {
            return (default!, err);
        }
    }
    while (ᐧ) {
        if (procEntry.ProcessID == ((uint32)pid)) {
            return (ᏑprocEntry, default!);
        }
        err = Process32Next(snapshot, ᏑprocEntry);
        if (err != default!) {
            return (default!, err);
        }
    }
});

public static nint /*ppid*/ Getppid() {
    nint ppid = default!;

    (pe, err) = getProcessEntry(Getpid());
    if (err != default!) {
        return -1;
    }
    return ((nint)(~pe).ParentProcessID);
}

internal static (slice<uint16>, error) fdpath(ΔHandle fd, slice<uint16> buf) {
    static readonly UntypedInt FILE_NAME_NORMALIZED = 0;
    static readonly UntypedInt VOLUME_NAME_DOS = 0;
    while (ᐧ) {
        var (n, err) = getFinalPathNameByHandle(fd, Ꮡ(buf, 0), ((uint32)len(buf)), (uint32)(FILE_NAME_NORMALIZED | VOLUME_NAME_DOS));
        if (err == default!) {
            buf = buf[..(int)(n)];
            break;
        }
        if (err != _ERROR_NOT_ENOUGH_MEMORY) {
            return (default!, err);
        }
        buf = append(buf, new slice<uint16>(n - ((uint32)len(buf))).ꓸꓸꓸ);
    }
    return (buf, default!);
}

public static error /*err*/ Fchdir(ΔHandle fd) {
    error err = default!;

    array<uint16> buf = new(261); /* MAX_PATH + 1 */
    (path, err) = fdpath(fd, buf[..]);
    if (err != default!) {
        return err;
    }
    // When using VOLUME_NAME_DOS, the path is always prefixed by "\\?\".
    // That prefix tells the Windows APIs to disable all string parsing and to send
    // the string that follows it straight to the file system.
    // Although SetCurrentDirectory and GetCurrentDirectory do support the "\\?\" prefix,
    // some other Windows APIs don't. If the prefix is not removed here, it will leak
    // to Getwd, and we don't want such a general-purpose function to always return a
    // path with the "\\?\" prefix after Fchdir is called.
    // The downside is that APIs that do support it will parse the path and try to normalize it,
    // when it's already normalized.
    if (len(path) >= 4 && path[0] == (rune)'\\' && path[1] == (rune)'\\' && path[2] == (rune)'?' && path[3] == (rune)'\\') {
        path = path[4..];
    }
    return SetCurrentDirectory(Ꮡ(path, 0));
}

// TODO(brainman): fix all needed for os
public static error /*err*/ Link(@string oldpath, @string newpath) {
    error err = default!;

    return EWINDOWS;
}

public static error /*err*/ Symlink(@string path, @string link) {
    error err = default!;

    return EWINDOWS;
}

public static error /*err*/ Fchmod(ΔHandle fd, uint32 mode) {
    error err = default!;

    return EWINDOWS;
}

public static error /*err*/ Chown(@string path, nint uid, nint gid) {
    error err = default!;

    return EWINDOWS;
}

public static error /*err*/ Lchown(@string path, nint uid, nint gid) {
    error err = default!;

    return EWINDOWS;
}

public static error /*err*/ Fchown(ΔHandle fd, nint uid, nint gid) {
    error err = default!;

    return EWINDOWS;
}

public static nint /*uid*/ Getuid() {
    nint uid = default!;

    return -1;
}

public static nint /*euid*/ Geteuid() {
    nint euid = default!;

    return -1;
}

public static nint /*gid*/ Getgid() {
    nint gid = default!;

    return -1;
}

public static nint /*egid*/ Getegid() {
    nint egid = default!;

    return -1;
}

public static (slice<nint> gids, error err) Getgroups() {
    slice<nint> gids = default!;
    error err = default!;

    return (default!, EWINDOWS);
}

[GoType("num:nint")] partial struct ΔSignal;

public static void Signal(this ΔSignal s) {
}

public static @string String(this ΔSignal s) {
    if (0 <= s && ((nint)s) < len(signals)) {
        @string str = signals[s];
        if (str != ""u8) {
            return str;
        }
    }
    return "signal "u8 + itoa.Itoa(((nint)s));
}

public static error LoadCreateSymbolicLink() {
    return procCreateSymbolicLinkW.Find();
}

// Readlink returns the destination of the named symbolic link.
public static (nint n, error err) Readlink(@string path, slice<byte> buf) => func((defer, _) => {
    nint n = default!;
    error err = default!;

    var (fd, err) = CreateFile(StringToUTF16Ptr(path), GENERIC_READ, 0, nil, OPEN_EXISTING,
        (uint32)(FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_BACKUP_SEMANTICS), 0);
    if (err != default!) {
        return (-1, err);
    }
    deferǃ(CloseHandle, fd, defer);
    var rdbbuf = new slice<byte>(MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
    ref var bytesReturned = ref heap(new uint32(), out var ᏑbytesReturned);
    err = DeviceIoControl(fd, FSCTL_GET_REPARSE_POINT, nil, 0, Ꮡ(rdbbuf, 0), ((uint32)len(rdbbuf)), ᏑbytesReturned, nil);
    if (err != default!) {
        return (-1, err);
    }
    var rdb = (ж<reparseDataBuffer>)(uintptr)(new @unsafe.Pointer(Ꮡ(rdbbuf, 0)));
    @string s = default!;
    var exprᴛ1 = (~rdb).ReparseTag;
    if (exprᴛ1 == IO_REPARSE_TAG_SYMLINK) {
        var data = (ж<symbolicLinkReparseBuffer>)(uintptr)(new @unsafe.Pointer(Ꮡ((~rdb).reparseBuffer)));
        var p = (ж<array<uint16>>)(uintptr)(new @unsafe.Pointer(Ꮡ(~data).PathBuffer.at<uint16>(0)));
        s = UTF16ToString(p[(int)((~data).SubstituteNameOffset / 2)..(int)(((~data).SubstituteNameOffset + (~data).SubstituteNameLength) / 2)]);
        if ((uint32)((~data).Flags & _SYMLINK_FLAG_RELATIVE) == 0) {
            if (len(s) >= 4 && s[..4] == @"\??\"){
                s = s[4..];
                switch (ᐧ) {
                case {} when len(s) >= 2 && s[1] == (rune)':': {
                    break;
                }
                case {} when len(s) >= 4 && s[..4] == @"UNC\": {
                    s = @"\\" + s[4..];
                    break;
                }
                default: {
                    break;
                }}

            } else {
            }
        }
    }
    else if (exprᴛ1 == _IO_REPARSE_TAG_MOUNT_POINT) {
        var data = (ж<mountPointReparseBuffer>)(uintptr)(new @unsafe.Pointer(Ꮡ((~rdb).reparseBuffer)));
        var p = (ж<array<uint16>>)(uintptr)(new @unsafe.Pointer(Ꮡ(~data).PathBuffer.at<uint16>(0)));
        s = UTF16ToString(p[(int)((~data).SubstituteNameOffset / 2)..(int)(((~data).SubstituteNameOffset + (~data).SubstituteNameLength) / 2)]);
        if (len(s) >= 4 && s[..4] == @"\??\"){
            // \??\C:\foo\bar
            // do nothing
            // \??\UNC\foo\bar
            // unexpected; do nothing
            // unexpected; do nothing
            // \??\C:\foo\bar
            s = s[4..];
        } else {
        }
    }
    else { /* default: */
        return (-1, ENOENT);
    }

    // unexpected; do nothing
    // the path is not a symlink or junction but another type of reparse
    // point
    n = copy(buf, slice<byte>(s));
    return (n, default!);
});

// Deprecated: CreateIoCompletionPort has the wrong function signature. Use x/sys/windows.CreateIoCompletionPort.
public static (ΔHandle, error) CreateIoCompletionPort(ΔHandle filehandle, ΔHandle cphandle, uint32 key, uint32 threadcnt) {
    return createIoCompletionPort(filehandle, cphandle, ((uintptr)key), threadcnt);
}

// Deprecated: GetQueuedCompletionStatus has the wrong function signature. Use x/sys/windows.GetQueuedCompletionStatus.
public static error GetQueuedCompletionStatus(ΔHandle cphandle, ж<uint32> Ꮡqty, ж<uint32> Ꮡkey, ж<ж<Overlapped>> Ꮡoverlapped, uint32 timeout) {
    ref var qty = ref Ꮡqty.val;
    ref var key = ref Ꮡkey.val;
    ref var overlapped = ref Ꮡoverlapped.val;

    ref var ukey = ref heap(new uintptr(), out var Ꮡukey);
    ж<uintptr> pukey = default!;
    if (key != nil) {
        ukey = ((uintptr)(key));
        pukey = Ꮡukey;
    }
    var err = getQueuedCompletionStatus(cphandle, Ꮡqty, pukey, Ꮡoverlapped, timeout);
    if (key != nil) {
        key = ((uint32)ukey);
        if (((uintptr)(key)) != ukey && err == default!) {
            err = errorspkg.New("GetQueuedCompletionStatus returned key overflow"u8);
        }
    }
    return err;
}

// Deprecated: PostQueuedCompletionStatus has the wrong function signature. Use x/sys/windows.PostQueuedCompletionStatus.
public static error PostQueuedCompletionStatus(ΔHandle cphandle, uint32 qty, uint32 key, ж<Overlapped> Ꮡoverlapped) {
    ref var overlapped = ref Ꮡoverlapped.val;

    return postQueuedCompletionStatus(cphandle, qty, ((uintptr)key), Ꮡoverlapped);
}

// newProcThreadAttributeList allocates new PROC_THREAD_ATTRIBUTE_LIST, with
// the requested maximum number of attributes, which must be cleaned up by
// deleteProcThreadAttributeList.
internal static (ж<_PROC_THREAD_ATTRIBUTE_LIST>, error) newProcThreadAttributeList(uint32 maxAttrCount) {
    ref var size = ref heap(new uintptr(), out var Ꮡsize);
    var err = initializeProcThreadAttributeList(nil, maxAttrCount, 0, Ꮡsize);
    if (err != ERROR_INSUFFICIENT_BUFFER) {
        if (err == default!) {
            return (default!, errorspkg.New("unable to query buffer size from InitializeProcThreadAttributeList"u8));
        }
        return (default!, err);
    }
    // size is guaranteed to be ≥1 by initializeProcThreadAttributeList.
    var al = (ж<_PROC_THREAD_ATTRIBUTE_LIST>)(uintptr)(new @unsafe.Pointer(Ꮡnew slice<byte>(size).at<byte>(0)));
    err = initializeProcThreadAttributeList(al, maxAttrCount, 0, Ꮡsize);
    if (err != default!) {
        return (default!, err);
    }
    return (al, default!);
}

// RegEnumKeyEx enumerates the subkeys of an open registry key.
// Each call retrieves information about one subkey. name is
// a buffer that should be large enough to hold the name of the
// subkey plus a null terminating character. nameLen is its
// length. On return, nameLen will contain the actual length of the
// subkey.
//
// Should name not be large enough to hold the subkey, this function
// will return ERROR_MORE_DATA, and must be called again with an
// appropriately sized buffer.
//
// reserved must be nil. class and classLen behave like name and nameLen
// but for the class of the subkey, except that they are optional.
// lastWriteTime, if not nil, will be populated with the time the subkey
// was last written.
//
// The caller must enumerate all subkeys in order. That is
// RegEnumKeyEx must be called with index starting at 0, incrementing
// the index until the function returns ERROR_NO_MORE_ITEMS, or with
// the index of the last subkey (obtainable from RegQueryInfoKey),
// decrementing until index 0 is enumerated.
//
// Successive calls to this API must happen on the same OS thread,
// so call [runtime.LockOSThread] before calling this function.
public static error /*regerrno*/ RegEnumKeyEx(ΔHandle key, uint32 index, ж<uint16> Ꮡname, ж<uint32> ᏑnameLen, ж<uint32> Ꮡreserved, ж<uint16> Ꮡclass, ж<uint32> ᏑclassLen, ж<Filetime> ᏑlastWriteTime) {
    error regerrno = default!;

    ref var name = ref Ꮡname.val;
    ref var nameLen = ref ᏑnameLen.val;
    ref var reserved = ref Ꮡreserved.val;
    ref var @class = ref Ꮡclass.val;
    ref var classLen = ref ᏑclassLen.val;
    ref var lastWriteTime = ref ᏑlastWriteTime.val;
    return regEnumKeyEx(key, index, Ꮡname, ᏑnameLen, Ꮡreserved, Ꮡclass, ᏑclassLen, ᏑlastWriteTime);
}

public static error GetStartupInfo(ж<StartupInfo> ᏑstartupInfo) {
    ref var startupInfo = ref ᏑstartupInfo.val;

    getStartupInfo(ᏑstartupInfo);
    return default!;
}

} // end syscall_package
