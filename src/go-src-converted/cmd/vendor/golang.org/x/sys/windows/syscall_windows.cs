// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows system calls.

// package windows -- go2cs converted at 2022 March 06 23:30:41 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\syscall_windows.go
using errorspkg = go.errors_package;
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using time = go.time_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;

using unsafeheader = go.golang.org.x.sys.@internal.unsafeheader_package;
using System;


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

public partial struct Handle { // : System.UIntPtr
}
public partial struct HWND { // : System.UIntPtr
}

public static readonly var InvalidHandle = ~Handle(0);
public static readonly var InvalidHWND = ~HWND(0); 

// Flags for DefineDosDevice.
public static readonly nuint DDD_EXACT_MATCH_ON_REMOVE = 0x00000004;
public static readonly nuint DDD_NO_BROADCAST_SYSTEM = 0x00000008;
public static readonly nuint DDD_RAW_TARGET_PATH = 0x00000001;
public static readonly nuint DDD_REMOVE_DEFINITION = 0x00000002; 

// Return values for GetDriveType.
public static readonly nint DRIVE_UNKNOWN = 0;
public static readonly nint DRIVE_NO_ROOT_DIR = 1;
public static readonly nint DRIVE_REMOVABLE = 2;
public static readonly nint DRIVE_FIXED = 3;
public static readonly nint DRIVE_REMOTE = 4;
public static readonly nint DRIVE_CDROM = 5;
public static readonly nint DRIVE_RAMDISK = 6; 

// File system flags from GetVolumeInformation and GetVolumeInformationByHandle.
public static readonly nuint FILE_CASE_SENSITIVE_SEARCH = 0x00000001;
public static readonly nuint FILE_CASE_PRESERVED_NAMES = 0x00000002;
public static readonly nuint FILE_FILE_COMPRESSION = 0x00000010;
public static readonly nuint FILE_DAX_VOLUME = 0x20000000;
public static readonly nuint FILE_NAMED_STREAMS = 0x00040000;
public static readonly nuint FILE_PERSISTENT_ACLS = 0x00000008;
public static readonly nuint FILE_READ_ONLY_VOLUME = 0x00080000;
public static readonly nuint FILE_SEQUENTIAL_WRITE_ONCE = 0x00100000;
public static readonly nuint FILE_SUPPORTS_ENCRYPTION = 0x00020000;
public static readonly nuint FILE_SUPPORTS_EXTENDED_ATTRIBUTES = 0x00800000;
public static readonly nuint FILE_SUPPORTS_HARD_LINKS = 0x00400000;
public static readonly nuint FILE_SUPPORTS_OBJECT_IDS = 0x00010000;
public static readonly nuint FILE_SUPPORTS_OPEN_BY_FILE_ID = 0x01000000;
public static readonly nuint FILE_SUPPORTS_REPARSE_POINTS = 0x00000080;
public static readonly nuint FILE_SUPPORTS_SPARSE_FILES = 0x00000040;
public static readonly nuint FILE_SUPPORTS_TRANSACTIONS = 0x00200000;
public static readonly nuint FILE_SUPPORTS_USN_JOURNAL = 0x02000000;
public static readonly nuint FILE_UNICODE_ON_DISK = 0x00000004;
public static readonly nuint FILE_VOLUME_IS_COMPRESSED = 0x00008000;
public static readonly nuint FILE_VOLUME_QUOTAS = 0x00000020; 

// Flags for LockFileEx.
public static readonly nuint LOCKFILE_FAIL_IMMEDIATELY = 0x00000001;
public static readonly nuint LOCKFILE_EXCLUSIVE_LOCK = 0x00000002; 

// Return value of SleepEx and other APC functions
public static readonly nuint WAIT_IO_COMPLETION = 0x000000C0;


// StringToUTF16 is deprecated. Use UTF16FromString instead.
// If s contains a NUL byte this function panics instead of
// returning an error.
public static slice<ushort> StringToUTF16(@string s) => func((_, panic, _) => {
    var (a, err) = UTF16FromString(s);
    if (err != null) {
        panic("windows: string with NUL passed to StringToUTF16");
    }
    return a;

});

// UTF16FromString returns the UTF-16 encoding of the UTF-8 string
// s, with a terminating NUL added. If s contains a NUL byte at any
// location, it returns (nil, syscall.EINVAL).
public static (slice<ushort>, error) UTF16FromString(@string s) {
    slice<ushort> _p0 = default;
    error _p0 = default!;

    for (nint i = 0; i < len(s); i++) {
        if (s[i] == 0) {
            return (null, error.As(syscall.EINVAL)!);
        }
    }
    return (utf16.Encode((slice<int>)s + "\x00"), error.As(null!)!);

}

// UTF16ToString returns the UTF-8 encoding of the UTF-16 sequence s,
// with a terminating NUL and any bytes after the NUL removed.
public static @string UTF16ToString(slice<ushort> s) {
    foreach (var (i, v) in s) {
        if (v == 0) {
            s = s[..(int)i];
            break;
        }
    }    return string(utf16.Decode(s));

}

// StringToUTF16Ptr is deprecated. Use UTF16PtrFromString instead.
// If s contains a NUL byte this function panics instead of
// returning an error.
public static ptr<ushort> StringToUTF16Ptr(@string s) {
    return _addr__addr_StringToUTF16(s)[0]!;
}

// UTF16PtrFromString returns pointer to the UTF-16 encoding of
// the UTF-8 string s, with a terminating NUL added. If s
// contains a NUL byte at any location, it returns (nil, syscall.EINVAL).
public static (ptr<ushort>, error) UTF16PtrFromString(@string s) {
    ptr<ushort> _p0 = default!;
    error _p0 = default!;

    var (a, err) = UTF16FromString(s);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return (_addr__addr_a[0]!, error.As(null!)!);

}

// UTF16PtrToString takes a pointer to a UTF-16 sequence and returns the corresponding UTF-8 encoded string.
// If the pointer is nil, it returns the empty string. It assumes that the UTF-16 sequence is terminated
// at a zero word; if the zero word is not present, the program may crash.
public static @string UTF16PtrToString(ptr<ushort> _addr_p) {
    ref ushort p = ref _addr_p.val;

    if (p == null) {
        return "";
    }
    if (p == 0.val) {
        return "";
    }
    nint n = 0;
    for (var ptr = @unsafe.Pointer(p); new ptr<ptr<ptr<ushort>>>(ptr) != 0; n++) {
        ptr = @unsafe.Pointer(uintptr(ptr) + @unsafe.Sizeof(p));
    }

    ref slice<ushort> s = ref heap(out ptr<slice<ushort>> _addr_s);
    var h = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_s));
    h.Data = @unsafe.Pointer(p);
    h.Len = n;
    h.Cap = n;

    return string(utf16.Decode(s));

}

public static nint Getpagesize() {
    return 4096;
}

// NewCallback converts a Go function to a function pointer conforming to the stdcall calling convention.
// This is useful when interoperating with Windows code requiring callbacks.
// The argument is expected to be a function with with one uintptr-sized result. The function must not have arguments with size larger than the size of uintptr.
public static System.UIntPtr NewCallback(object fn) {
    return syscall.NewCallback(fn);
}

// NewCallbackCDecl converts a Go function to a function pointer conforming to the cdecl calling convention.
// This is useful when interoperating with Windows code requiring callbacks.
// The argument is expected to be a function with with one uintptr-sized result. The function must not have arguments with size larger than the size of uintptr.
public static System.UIntPtr NewCallbackCDecl(object fn) {
    return syscall.NewCallbackCDecl(fn);
}

// windows api calls

//sys    GetLastError() (lasterr error)
//sys    LoadLibrary(libname string) (handle Handle, err error) = LoadLibraryW
//sys    LoadLibraryEx(libname string, zero Handle, flags uintptr) (handle Handle, err error) = LoadLibraryExW
//sys    FreeLibrary(handle Handle) (err error)
//sys    GetProcAddress(module Handle, procname string) (proc uintptr, err error)
//sys    GetModuleFileName(module Handle, filename *uint16, size uint32) (n uint32, err error) = kernel32.GetModuleFileNameW
//sys    GetModuleHandleEx(flags uint32, moduleName *uint16, module *Handle) (err error) = kernel32.GetModuleHandleExW
//sys    SetDefaultDllDirectories(directoryFlags uint32) (err error)
//sys    SetDllDirectory(path string) (err error) = kernel32.SetDllDirectoryW
//sys    GetVersion() (ver uint32, err error)
//sys    FormatMessage(flags uint32, msgsrc uintptr, msgid uint32, langid uint32, buf []uint16, args *byte) (n uint32, err error) = FormatMessageW
//sys    ExitProcess(exitcode uint32)
//sys    IsWow64Process(handle Handle, isWow64 *bool) (err error) = IsWow64Process
//sys    IsWow64Process2(handle Handle, processMachine *uint16, nativeMachine *uint16) (err error) = IsWow64Process2?
//sys    CreateFile(name *uint16, access uint32, mode uint32, sa *SecurityAttributes, createmode uint32, attrs uint32, templatefile Handle) (handle Handle, err error) [failretval==InvalidHandle] = CreateFileW
//sys    CreateNamedPipe(name *uint16, flags uint32, pipeMode uint32, maxInstances uint32, outSize uint32, inSize uint32, defaultTimeout uint32, sa *SecurityAttributes) (handle Handle, err error)  [failretval==InvalidHandle] = CreateNamedPipeW
//sys    ConnectNamedPipe(pipe Handle, overlapped *Overlapped) (err error)
//sys    GetNamedPipeInfo(pipe Handle, flags *uint32, outSize *uint32, inSize *uint32, maxInstances *uint32) (err error)
//sys    GetNamedPipeHandleState(pipe Handle, state *uint32, curInstances *uint32, maxCollectionCount *uint32, collectDataTimeout *uint32, userName *uint16, maxUserNameSize uint32) (err error) = GetNamedPipeHandleStateW
//sys    SetNamedPipeHandleState(pipe Handle, state *uint32, maxCollectionCount *uint32, collectDataTimeout *uint32) (err error) = SetNamedPipeHandleState
//sys    ReadFile(handle Handle, buf []byte, done *uint32, overlapped *Overlapped) (err error)
//sys    WriteFile(handle Handle, buf []byte, done *uint32, overlapped *Overlapped) (err error)
//sys    GetOverlappedResult(handle Handle, overlapped *Overlapped, done *uint32, wait bool) (err error)
//sys    SetFilePointer(handle Handle, lowoffset int32, highoffsetptr *int32, whence uint32) (newlowoffset uint32, err error) [failretval==0xffffffff]
//sys    CloseHandle(handle Handle) (err error)
//sys    GetStdHandle(stdhandle uint32) (handle Handle, err error) [failretval==InvalidHandle]
//sys    SetStdHandle(stdhandle uint32, handle Handle) (err error)
//sys    findFirstFile1(name *uint16, data *win32finddata1) (handle Handle, err error) [failretval==InvalidHandle] = FindFirstFileW
//sys    findNextFile1(handle Handle, data *win32finddata1) (err error) = FindNextFileW
//sys    FindClose(handle Handle) (err error)
//sys    GetFileInformationByHandle(handle Handle, data *ByHandleFileInformation) (err error)
//sys    GetFileInformationByHandleEx(handle Handle, class uint32, outBuffer *byte, outBufferLen uint32) (err error)
//sys    SetFileInformationByHandle(handle Handle, class uint32, inBuffer *byte, inBufferLen uint32) (err error)
//sys    GetCurrentDirectory(buflen uint32, buf *uint16) (n uint32, err error) = GetCurrentDirectoryW
//sys    SetCurrentDirectory(path *uint16) (err error) = SetCurrentDirectoryW
//sys    CreateDirectory(path *uint16, sa *SecurityAttributes) (err error) = CreateDirectoryW
//sys    RemoveDirectory(path *uint16) (err error) = RemoveDirectoryW
//sys    DeleteFile(path *uint16) (err error) = DeleteFileW
//sys    MoveFile(from *uint16, to *uint16) (err error) = MoveFileW
//sys    MoveFileEx(from *uint16, to *uint16, flags uint32) (err error) = MoveFileExW
//sys    LockFileEx(file Handle, flags uint32, reserved uint32, bytesLow uint32, bytesHigh uint32, overlapped *Overlapped) (err error)
//sys    UnlockFileEx(file Handle, reserved uint32, bytesLow uint32, bytesHigh uint32, overlapped *Overlapped) (err error)
//sys    GetComputerName(buf *uint16, n *uint32) (err error) = GetComputerNameW
//sys    GetComputerNameEx(nametype uint32, buf *uint16, n *uint32) (err error) = GetComputerNameExW
//sys    SetEndOfFile(handle Handle) (err error)
//sys    GetSystemTimeAsFileTime(time *Filetime)
//sys    GetSystemTimePreciseAsFileTime(time *Filetime)
//sys    GetTimeZoneInformation(tzi *Timezoneinformation) (rc uint32, err error) [failretval==0xffffffff]
//sys    CreateIoCompletionPort(filehandle Handle, cphandle Handle, key uintptr, threadcnt uint32) (handle Handle, err error)
//sys    GetQueuedCompletionStatus(cphandle Handle, qty *uint32, key *uintptr, overlapped **Overlapped, timeout uint32) (err error)
//sys    PostQueuedCompletionStatus(cphandle Handle, qty uint32, key uintptr, overlapped *Overlapped) (err error)
//sys    CancelIo(s Handle) (err error)
//sys    CancelIoEx(s Handle, o *Overlapped) (err error)
//sys    CreateProcess(appName *uint16, commandLine *uint16, procSecurity *SecurityAttributes, threadSecurity *SecurityAttributes, inheritHandles bool, creationFlags uint32, env *uint16, currentDir *uint16, startupInfo *StartupInfo, outProcInfo *ProcessInformation) (err error) = CreateProcessW
//sys   initializeProcThreadAttributeList(attrlist *ProcThreadAttributeList, attrcount uint32, flags uint32, size *uintptr) (err error) = InitializeProcThreadAttributeList
//sys   deleteProcThreadAttributeList(attrlist *ProcThreadAttributeList) = DeleteProcThreadAttributeList
//sys   updateProcThreadAttribute(attrlist *ProcThreadAttributeList, flags uint32, attr uintptr, value unsafe.Pointer, size uintptr, prevvalue unsafe.Pointer, returnedsize *uintptr) (err error) = UpdateProcThreadAttribute
//sys    OpenProcess(desiredAccess uint32, inheritHandle bool, processId uint32) (handle Handle, err error)
//sys    ShellExecute(hwnd Handle, verb *uint16, file *uint16, args *uint16, cwd *uint16, showCmd int32) (err error) [failretval<=32] = shell32.ShellExecuteW
//sys    GetWindowThreadProcessId(hwnd HWND, pid *uint32) (tid uint32, err error) = user32.GetWindowThreadProcessId
//sys    GetShellWindow() (shellWindow HWND) = user32.GetShellWindow
//sys    MessageBox(hwnd HWND, text *uint16, caption *uint16, boxtype uint32) (ret int32, err error) [failretval==0] = user32.MessageBoxW
//sys    ExitWindowsEx(flags uint32, reason uint32) (err error) = user32.ExitWindowsEx
//sys    shGetKnownFolderPath(id *KNOWNFOLDERID, flags uint32, token Token, path **uint16) (ret error) = shell32.SHGetKnownFolderPath
//sys    TerminateProcess(handle Handle, exitcode uint32) (err error)
//sys    GetExitCodeProcess(handle Handle, exitcode *uint32) (err error)
//sys    GetStartupInfo(startupInfo *StartupInfo) (err error) = GetStartupInfoW
//sys    GetProcessTimes(handle Handle, creationTime *Filetime, exitTime *Filetime, kernelTime *Filetime, userTime *Filetime) (err error)
//sys    DuplicateHandle(hSourceProcessHandle Handle, hSourceHandle Handle, hTargetProcessHandle Handle, lpTargetHandle *Handle, dwDesiredAccess uint32, bInheritHandle bool, dwOptions uint32) (err error)
//sys    WaitForSingleObject(handle Handle, waitMilliseconds uint32) (event uint32, err error) [failretval==0xffffffff]
//sys    waitForMultipleObjects(count uint32, handles uintptr, waitAll bool, waitMilliseconds uint32) (event uint32, err error) [failretval==0xffffffff] = WaitForMultipleObjects
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
//sys    CreateEnvironmentBlock(block **uint16, token Token, inheritExisting bool) (err error) = userenv.CreateEnvironmentBlock
//sys    DestroyEnvironmentBlock(block *uint16) (err error) = userenv.DestroyEnvironmentBlock
//sys    getTickCount64() (ms uint64) = kernel32.GetTickCount64
//sys    SetFileTime(handle Handle, ctime *Filetime, atime *Filetime, wtime *Filetime) (err error)
//sys    GetFileAttributes(name *uint16) (attrs uint32, err error) [failretval==INVALID_FILE_ATTRIBUTES] = kernel32.GetFileAttributesW
//sys    SetFileAttributes(name *uint16, attrs uint32) (err error) = kernel32.SetFileAttributesW
//sys    GetFileAttributesEx(name *uint16, level uint32, info *byte) (err error) = kernel32.GetFileAttributesExW
//sys    GetCommandLine() (cmd *uint16) = kernel32.GetCommandLineW
//sys    CommandLineToArgv(cmd *uint16, argc *int32) (argv *[8192]*[8192]uint16, err error) [failretval==nil] = shell32.CommandLineToArgvW
//sys    LocalFree(hmem Handle) (handle Handle, err error) [failretval!=0]
//sys    LocalAlloc(flags uint32, length uint32) (ptr uintptr, err error)
//sys    SetHandleInformation(handle Handle, mask uint32, flags uint32) (err error)
//sys    FlushFileBuffers(handle Handle) (err error)
//sys    GetFullPathName(path *uint16, buflen uint32, buf *uint16, fname **uint16) (n uint32, err error) = kernel32.GetFullPathNameW
//sys    GetLongPathName(path *uint16, buf *uint16, buflen uint32) (n uint32, err error) = kernel32.GetLongPathNameW
//sys    GetShortPathName(longpath *uint16, shortpath *uint16, buflen uint32) (n uint32, err error) = kernel32.GetShortPathNameW
//sys    GetFinalPathNameByHandle(file Handle, filePath *uint16, filePathSize uint32, flags uint32) (n uint32, err error) = kernel32.GetFinalPathNameByHandleW
//sys    CreateFileMapping(fhandle Handle, sa *SecurityAttributes, prot uint32, maxSizeHigh uint32, maxSizeLow uint32, name *uint16) (handle Handle, err error) [failretval == 0 || e1 == ERROR_ALREADY_EXISTS] = kernel32.CreateFileMappingW
//sys    MapViewOfFile(handle Handle, access uint32, offsetHigh uint32, offsetLow uint32, length uintptr) (addr uintptr, err error)
//sys    UnmapViewOfFile(addr uintptr) (err error)
//sys    FlushViewOfFile(addr uintptr, length uintptr) (err error)
//sys    VirtualLock(addr uintptr, length uintptr) (err error)
//sys    VirtualUnlock(addr uintptr, length uintptr) (err error)
//sys    VirtualAlloc(address uintptr, size uintptr, alloctype uint32, protect uint32) (value uintptr, err error) = kernel32.VirtualAlloc
//sys    VirtualFree(address uintptr, size uintptr, freetype uint32) (err error) = kernel32.VirtualFree
//sys    VirtualProtect(address uintptr, size uintptr, newprotect uint32, oldprotect *uint32) (err error) = kernel32.VirtualProtect
//sys    TransmitFile(s Handle, handle Handle, bytesToWrite uint32, bytsPerSend uint32, overlapped *Overlapped, transmitFileBuf *TransmitFileBuffers, flags uint32) (err error) = mswsock.TransmitFile
//sys    ReadDirectoryChanges(handle Handle, buf *byte, buflen uint32, watchSubTree bool, mask uint32, retlen *uint32, overlapped *Overlapped, completionRoutine uintptr) (err error) = kernel32.ReadDirectoryChangesW
//sys    FindFirstChangeNotification(path string, watchSubtree bool, notifyFilter uint32) (handle Handle, err error) [failretval==InvalidHandle] = kernel32.FindFirstChangeNotificationW
//sys    FindNextChangeNotification(handle Handle) (err error)
//sys    FindCloseChangeNotification(handle Handle) (err error)
//sys    CertOpenSystemStore(hprov Handle, name *uint16) (store Handle, err error) = crypt32.CertOpenSystemStoreW
//sys    CertOpenStore(storeProvider uintptr, msgAndCertEncodingType uint32, cryptProv uintptr, flags uint32, para uintptr) (handle Handle, err error) = crypt32.CertOpenStore
//sys    CertEnumCertificatesInStore(store Handle, prevContext *CertContext) (context *CertContext, err error) [failretval==nil] = crypt32.CertEnumCertificatesInStore
//sys    CertAddCertificateContextToStore(store Handle, certContext *CertContext, addDisposition uint32, storeContext **CertContext) (err error) = crypt32.CertAddCertificateContextToStore
//sys    CertCloseStore(store Handle, flags uint32) (err error) = crypt32.CertCloseStore
//sys    CertDeleteCertificateFromStore(certContext *CertContext) (err error) = crypt32.CertDeleteCertificateFromStore
//sys    CertDuplicateCertificateContext(certContext *CertContext) (dupContext *CertContext) = crypt32.CertDuplicateCertificateContext
//sys    PFXImportCertStore(pfx *CryptDataBlob, password *uint16, flags uint32) (store Handle, err error) = crypt32.PFXImportCertStore
//sys    CertGetCertificateChain(engine Handle, leaf *CertContext, time *Filetime, additionalStore Handle, para *CertChainPara, flags uint32, reserved uintptr, chainCtx **CertChainContext) (err error) = crypt32.CertGetCertificateChain
//sys    CertFreeCertificateChain(ctx *CertChainContext) = crypt32.CertFreeCertificateChain
//sys    CertCreateCertificateContext(certEncodingType uint32, certEncoded *byte, encodedLen uint32) (context *CertContext, err error) [failretval==nil] = crypt32.CertCreateCertificateContext
//sys    CertFreeCertificateContext(ctx *CertContext) (err error) = crypt32.CertFreeCertificateContext
//sys    CertVerifyCertificateChainPolicy(policyOID uintptr, chain *CertChainContext, para *CertChainPolicyPara, status *CertChainPolicyStatus) (err error) = crypt32.CertVerifyCertificateChainPolicy
//sys    CertGetNameString(certContext *CertContext, nameType uint32, flags uint32, typePara unsafe.Pointer, name *uint16, size uint32) (chars uint32) = crypt32.CertGetNameStringW
//sys    CertFindExtension(objId *byte, countExtensions uint32, extensions *CertExtension) (ret *CertExtension) = crypt32.CertFindExtension
//sys   CertFindCertificateInStore(store Handle, certEncodingType uint32, findFlags uint32, findType uint32, findPara unsafe.Pointer, prevCertContext *CertContext) (cert *CertContext, err error) [failretval==nil] = crypt32.CertFindCertificateInStore
//sys   CertFindChainInStore(store Handle, certEncodingType uint32, findFlags uint32, findType uint32, findPara unsafe.Pointer, prevChainContext *CertChainContext) (certchain *CertChainContext, err error) [failretval==nil] = crypt32.CertFindChainInStore
//sys   CryptAcquireCertificatePrivateKey(cert *CertContext, flags uint32, parameters unsafe.Pointer, cryptProvOrNCryptKey *Handle, keySpec *uint32, callerFreeProvOrNCryptKey *bool) (err error) = crypt32.CryptAcquireCertificatePrivateKey
//sys    CryptQueryObject(objectType uint32, object unsafe.Pointer, expectedContentTypeFlags uint32, expectedFormatTypeFlags uint32, flags uint32, msgAndCertEncodingType *uint32, contentType *uint32, formatType *uint32, certStore *Handle, msg *Handle, context *unsafe.Pointer) (err error) = crypt32.CryptQueryObject
//sys    CryptDecodeObject(encodingType uint32, structType *byte, encodedBytes *byte, lenEncodedBytes uint32, flags uint32, decoded unsafe.Pointer, decodedLen *uint32) (err error) = crypt32.CryptDecodeObject
//sys    CryptProtectData(dataIn *DataBlob, name *uint16, optionalEntropy *DataBlob, reserved uintptr, promptStruct *CryptProtectPromptStruct, flags uint32, dataOut *DataBlob) (err error) = crypt32.CryptProtectData
//sys    CryptUnprotectData(dataIn *DataBlob, name **uint16, optionalEntropy *DataBlob, reserved uintptr, promptStruct *CryptProtectPromptStruct, flags uint32, dataOut *DataBlob) (err error) = crypt32.CryptUnprotectData
//sys    WinVerifyTrustEx(hwnd HWND, actionId *GUID, data *WinTrustData) (ret error) = wintrust.WinVerifyTrustEx
//sys    RegOpenKeyEx(key Handle, subkey *uint16, options uint32, desiredAccess uint32, result *Handle) (regerrno error) = advapi32.RegOpenKeyExW
//sys    RegCloseKey(key Handle) (regerrno error) = advapi32.RegCloseKey
//sys    RegQueryInfoKey(key Handle, class *uint16, classLen *uint32, reserved *uint32, subkeysLen *uint32, maxSubkeyLen *uint32, maxClassLen *uint32, valuesLen *uint32, maxValueNameLen *uint32, maxValueLen *uint32, saLen *uint32, lastWriteTime *Filetime) (regerrno error) = advapi32.RegQueryInfoKeyW
//sys    RegEnumKeyEx(key Handle, index uint32, name *uint16, nameLen *uint32, reserved *uint32, class *uint16, classLen *uint32, lastWriteTime *Filetime) (regerrno error) = advapi32.RegEnumKeyExW
//sys    RegQueryValueEx(key Handle, name *uint16, reserved *uint32, valtype *uint32, buf *byte, buflen *uint32) (regerrno error) = advapi32.RegQueryValueExW
//sys    RegNotifyChangeKeyValue(key Handle, watchSubtree bool, notifyFilter uint32, event Handle, asynchronous bool) (regerrno error) = advapi32.RegNotifyChangeKeyValue
//sys    GetCurrentProcessId() (pid uint32) = kernel32.GetCurrentProcessId
//sys    ProcessIdToSessionId(pid uint32, sessionid *uint32) (err error) = kernel32.ProcessIdToSessionId
//sys    GetConsoleMode(console Handle, mode *uint32) (err error) = kernel32.GetConsoleMode
//sys    SetConsoleMode(console Handle, mode uint32) (err error) = kernel32.SetConsoleMode
//sys    GetConsoleScreenBufferInfo(console Handle, info *ConsoleScreenBufferInfo) (err error) = kernel32.GetConsoleScreenBufferInfo
//sys    setConsoleCursorPosition(console Handle, position uint32) (err error) = kernel32.SetConsoleCursorPosition
//sys    WriteConsole(console Handle, buf *uint16, towrite uint32, written *uint32, reserved *byte) (err error) = kernel32.WriteConsoleW
//sys    ReadConsole(console Handle, buf *uint16, toread uint32, read *uint32, inputControl *byte) (err error) = kernel32.ReadConsoleW
//sys    CreateToolhelp32Snapshot(flags uint32, processId uint32) (handle Handle, err error) [failretval==InvalidHandle] = kernel32.CreateToolhelp32Snapshot
//sys    Process32First(snapshot Handle, procEntry *ProcessEntry32) (err error) = kernel32.Process32FirstW
//sys    Process32Next(snapshot Handle, procEntry *ProcessEntry32) (err error) = kernel32.Process32NextW
//sys    Thread32First(snapshot Handle, threadEntry *ThreadEntry32) (err error)
//sys    Thread32Next(snapshot Handle, threadEntry *ThreadEntry32) (err error)
//sys    DeviceIoControl(handle Handle, ioControlCode uint32, inBuffer *byte, inBufferSize uint32, outBuffer *byte, outBufferSize uint32, bytesReturned *uint32, overlapped *Overlapped) (err error)
// This function returns 1 byte BOOLEAN rather than the 4 byte BOOL.
//sys    CreateSymbolicLink(symlinkfilename *uint16, targetfilename *uint16, flags uint32) (err error) [failretval&0xff==0] = CreateSymbolicLinkW
//sys    CreateHardLink(filename *uint16, existingfilename *uint16, reserved uintptr) (err error) [failretval&0xff==0] = CreateHardLinkW
//sys    GetCurrentThreadId() (id uint32)
//sys    CreateEvent(eventAttrs *SecurityAttributes, manualReset uint32, initialState uint32, name *uint16) (handle Handle, err error) [failretval == 0 || e1 == ERROR_ALREADY_EXISTS] = kernel32.CreateEventW
//sys    CreateEventEx(eventAttrs *SecurityAttributes, name *uint16, flags uint32, desiredAccess uint32) (handle Handle, err error) [failretval == 0 || e1 == ERROR_ALREADY_EXISTS] = kernel32.CreateEventExW
//sys    OpenEvent(desiredAccess uint32, inheritHandle bool, name *uint16) (handle Handle, err error) = kernel32.OpenEventW
//sys    SetEvent(event Handle) (err error) = kernel32.SetEvent
//sys    ResetEvent(event Handle) (err error) = kernel32.ResetEvent
//sys    PulseEvent(event Handle) (err error) = kernel32.PulseEvent
//sys    CreateMutex(mutexAttrs *SecurityAttributes, initialOwner bool, name *uint16) (handle Handle, err error) [failretval == 0 || e1 == ERROR_ALREADY_EXISTS] = kernel32.CreateMutexW
//sys    CreateMutexEx(mutexAttrs *SecurityAttributes, name *uint16, flags uint32, desiredAccess uint32) (handle Handle, err error) [failretval == 0 || e1 == ERROR_ALREADY_EXISTS] = kernel32.CreateMutexExW
//sys    OpenMutex(desiredAccess uint32, inheritHandle bool, name *uint16) (handle Handle, err error) = kernel32.OpenMutexW
//sys    ReleaseMutex(mutex Handle) (err error) = kernel32.ReleaseMutex
//sys    SleepEx(milliseconds uint32, alertable bool) (ret uint32) = kernel32.SleepEx
//sys    CreateJobObject(jobAttr *SecurityAttributes, name *uint16) (handle Handle, err error) = kernel32.CreateJobObjectW
//sys    AssignProcessToJobObject(job Handle, process Handle) (err error) = kernel32.AssignProcessToJobObject
//sys    TerminateJobObject(job Handle, exitCode uint32) (err error) = kernel32.TerminateJobObject
//sys    SetErrorMode(mode uint32) (ret uint32) = kernel32.SetErrorMode
//sys    ResumeThread(thread Handle) (ret uint32, err error) [failretval==0xffffffff] = kernel32.ResumeThread
//sys    SetPriorityClass(process Handle, priorityClass uint32) (err error) = kernel32.SetPriorityClass
//sys    GetPriorityClass(process Handle) (ret uint32, err error) = kernel32.GetPriorityClass
//sys    QueryInformationJobObject(job Handle, JobObjectInformationClass int32, JobObjectInformation uintptr, JobObjectInformationLength uint32, retlen *uint32) (err error) = kernel32.QueryInformationJobObject
//sys    SetInformationJobObject(job Handle, JobObjectInformationClass uint32, JobObjectInformation uintptr, JobObjectInformationLength uint32) (ret int, err error)
//sys    GenerateConsoleCtrlEvent(ctrlEvent uint32, processGroupID uint32) (err error)
//sys    GetProcessId(process Handle) (id uint32, err error)
//sys    QueryFullProcessImageName(proc Handle, flags uint32, exeName *uint16, size *uint32) (err error) = kernel32.QueryFullProcessImageNameW
//sys    OpenThread(desiredAccess uint32, inheritHandle bool, threadId uint32) (handle Handle, err error)
//sys    SetProcessPriorityBoost(process Handle, disable bool) (err error) = kernel32.SetProcessPriorityBoost
//sys    GetProcessWorkingSetSizeEx(hProcess Handle, lpMinimumWorkingSetSize *uintptr, lpMaximumWorkingSetSize *uintptr, flags *uint32)
//sys    SetProcessWorkingSetSizeEx(hProcess Handle, dwMinimumWorkingSetSize uintptr, dwMaximumWorkingSetSize uintptr, flags uint32) (err error)
//sys    GetCommTimeouts(handle Handle, timeouts *CommTimeouts) (err error)
//sys    SetCommTimeouts(handle Handle, timeouts *CommTimeouts) (err error)

// Volume Management Functions
//sys    DefineDosDevice(flags uint32, deviceName *uint16, targetPath *uint16) (err error) = DefineDosDeviceW
//sys    DeleteVolumeMountPoint(volumeMountPoint *uint16) (err error) = DeleteVolumeMountPointW
//sys    FindFirstVolume(volumeName *uint16, bufferLength uint32) (handle Handle, err error) [failretval==InvalidHandle] = FindFirstVolumeW
//sys    FindFirstVolumeMountPoint(rootPathName *uint16, volumeMountPoint *uint16, bufferLength uint32) (handle Handle, err error) [failretval==InvalidHandle] = FindFirstVolumeMountPointW
//sys    FindNextVolume(findVolume Handle, volumeName *uint16, bufferLength uint32) (err error) = FindNextVolumeW
//sys    FindNextVolumeMountPoint(findVolumeMountPoint Handle, volumeMountPoint *uint16, bufferLength uint32) (err error) = FindNextVolumeMountPointW
//sys    FindVolumeClose(findVolume Handle) (err error)
//sys    FindVolumeMountPointClose(findVolumeMountPoint Handle) (err error)
//sys    GetDiskFreeSpaceEx(directoryName *uint16, freeBytesAvailableToCaller *uint64, totalNumberOfBytes *uint64, totalNumberOfFreeBytes *uint64) (err error) = GetDiskFreeSpaceExW
//sys    GetDriveType(rootPathName *uint16) (driveType uint32) = GetDriveTypeW
//sys    GetLogicalDrives() (drivesBitMask uint32, err error) [failretval==0]
//sys    GetLogicalDriveStrings(bufferLength uint32, buffer *uint16) (n uint32, err error) [failretval==0] = GetLogicalDriveStringsW
//sys    GetVolumeInformation(rootPathName *uint16, volumeNameBuffer *uint16, volumeNameSize uint32, volumeNameSerialNumber *uint32, maximumComponentLength *uint32, fileSystemFlags *uint32, fileSystemNameBuffer *uint16, fileSystemNameSize uint32) (err error) = GetVolumeInformationW
//sys    GetVolumeInformationByHandle(file Handle, volumeNameBuffer *uint16, volumeNameSize uint32, volumeNameSerialNumber *uint32, maximumComponentLength *uint32, fileSystemFlags *uint32, fileSystemNameBuffer *uint16, fileSystemNameSize uint32) (err error) = GetVolumeInformationByHandleW
//sys    GetVolumeNameForVolumeMountPoint(volumeMountPoint *uint16, volumeName *uint16, bufferlength uint32) (err error) = GetVolumeNameForVolumeMountPointW
//sys    GetVolumePathName(fileName *uint16, volumePathName *uint16, bufferLength uint32) (err error) = GetVolumePathNameW
//sys    GetVolumePathNamesForVolumeName(volumeName *uint16, volumePathNames *uint16, bufferLength uint32, returnLength *uint32) (err error) = GetVolumePathNamesForVolumeNameW
//sys    QueryDosDevice(deviceName *uint16, targetPath *uint16, max uint32) (n uint32, err error) [failretval==0] = QueryDosDeviceW
//sys    SetVolumeLabel(rootPathName *uint16, volumeName *uint16) (err error) = SetVolumeLabelW
//sys    SetVolumeMountPoint(volumeMountPoint *uint16, volumeName *uint16) (err error) = SetVolumeMountPointW
//sys    InitiateSystemShutdownEx(machineName *uint16, message *uint16, timeout uint32, forceAppsClosed bool, rebootAfterShutdown bool, reason uint32) (err error) = advapi32.InitiateSystemShutdownExW
//sys    SetProcessShutdownParameters(level uint32, flags uint32) (err error) = kernel32.SetProcessShutdownParameters
//sys    GetProcessShutdownParameters(level *uint32, flags *uint32) (err error) = kernel32.GetProcessShutdownParameters
//sys    clsidFromString(lpsz *uint16, pclsid *GUID) (ret error) = ole32.CLSIDFromString
//sys    stringFromGUID2(rguid *GUID, lpsz *uint16, cchMax int32) (chars int32) = ole32.StringFromGUID2
//sys    coCreateGuid(pguid *GUID) (ret error) = ole32.CoCreateGuid
//sys    CoTaskMemFree(address unsafe.Pointer) = ole32.CoTaskMemFree
//sys    CoInitializeEx(reserved uintptr, coInit uint32) (ret error) = ole32.CoInitializeEx
//sys    CoUninitialize() = ole32.CoUninitialize
//sys    CoGetObject(name *uint16, bindOpts *BIND_OPTS3, guid *GUID, functionTable **uintptr) (ret error) = ole32.CoGetObject
//sys    getProcessPreferredUILanguages(flags uint32, numLanguages *uint32, buf *uint16, bufSize *uint32) (err error) = kernel32.GetProcessPreferredUILanguages
//sys    getThreadPreferredUILanguages(flags uint32, numLanguages *uint32, buf *uint16, bufSize *uint32) (err error) = kernel32.GetThreadPreferredUILanguages
//sys    getUserPreferredUILanguages(flags uint32, numLanguages *uint32, buf *uint16, bufSize *uint32) (err error) = kernel32.GetUserPreferredUILanguages
//sys    getSystemPreferredUILanguages(flags uint32, numLanguages *uint32, buf *uint16, bufSize *uint32) (err error) = kernel32.GetSystemPreferredUILanguages
//sys    findResource(module Handle, name uintptr, resType uintptr) (resInfo Handle, err error) = kernel32.FindResourceW
//sys    SizeofResource(module Handle, resInfo Handle) (size uint32, err error) = kernel32.SizeofResource
//sys    LoadResource(module Handle, resInfo Handle) (resData Handle, err error) = kernel32.LoadResource
//sys    LockResource(resData Handle) (addr uintptr, err error) = kernel32.LockResource

// Process Status API (PSAPI)
//sys    EnumProcesses(processIds []uint32, bytesReturned *uint32) (err error) = psapi.EnumProcesses

// NT Native APIs
//sys    rtlNtStatusToDosErrorNoTeb(ntstatus NTStatus) (ret syscall.Errno) = ntdll.RtlNtStatusToDosErrorNoTeb
//sys    rtlGetVersion(info *OsVersionInfoEx) (ntstatus error) = ntdll.RtlGetVersion
//sys    rtlGetNtVersionNumbers(majorVersion *uint32, minorVersion *uint32, buildNumber *uint32) = ntdll.RtlGetNtVersionNumbers
//sys    RtlGetCurrentPeb() (peb *PEB) = ntdll.RtlGetCurrentPeb
//sys    RtlInitUnicodeString(destinationString *NTUnicodeString, sourceString *uint16) = ntdll.RtlInitUnicodeString
//sys    RtlInitString(destinationString *NTString, sourceString *byte) = ntdll.RtlInitString
//sys    NtCreateFile(handle *Handle, access uint32, oa *OBJECT_ATTRIBUTES, iosb *IO_STATUS_BLOCK, allocationSize *int64, attributes uint32, share uint32, disposition uint32, options uint32, eabuffer uintptr, ealength uint32) (ntstatus error) = ntdll.NtCreateFile
//sys    NtCreateNamedPipeFile(pipe *Handle, access uint32, oa *OBJECT_ATTRIBUTES, iosb *IO_STATUS_BLOCK, share uint32, disposition uint32, options uint32, typ uint32, readMode uint32, completionMode uint32, maxInstances uint32, inboundQuota uint32, outputQuota uint32, timeout *int64) (ntstatus error) = ntdll.NtCreateNamedPipeFile
//sys    RtlDosPathNameToNtPathName(dosName *uint16, ntName *NTUnicodeString, ntFileNamePart *uint16, relativeName *RTL_RELATIVE_NAME) (ntstatus error) = ntdll.RtlDosPathNameToNtPathName_U_WithStatus
//sys    RtlDosPathNameToRelativeNtPathName(dosName *uint16, ntName *NTUnicodeString, ntFileNamePart *uint16, relativeName *RTL_RELATIVE_NAME) (ntstatus error) = ntdll.RtlDosPathNameToRelativeNtPathName_U_WithStatus
//sys    RtlDefaultNpAcl(acl **ACL) (ntstatus error) = ntdll.RtlDefaultNpAcl
//sys    NtQueryInformationProcess(proc Handle, procInfoClass int32, procInfo unsafe.Pointer, procInfoLen uint32, retLen *uint32) (ntstatus error) = ntdll.NtQueryInformationProcess
//sys    NtSetInformationProcess(proc Handle, procInfoClass int32, procInfo unsafe.Pointer, procInfoLen uint32) (ntstatus error) = ntdll.NtSetInformationProcess

// syscall interface implementation for other packages

// GetCurrentProcess returns the handle for the current process.
// It is a pseudo handle that does not need to be closed.
// The returned error is always nil.
//
// Deprecated: use CurrentProcess for the same Handle without the nil
// error.
public static (Handle, error) GetCurrentProcess() {
    Handle _p0 = default;
    error _p0 = default!;

    return (CurrentProcess(), error.As(null!)!);
}

// CurrentProcess returns the handle for the current process.
// It is a pseudo handle that does not need to be closed.
public static Handle CurrentProcess() {
    return Handle(~uintptr(1 - 1));
}

// GetCurrentThread returns the handle for the current thread.
// It is a pseudo handle that does not need to be closed.
// The returned error is always nil.
//
// Deprecated: use CurrentThread for the same Handle without the nil
// error.
public static (Handle, error) GetCurrentThread() {
    Handle _p0 = default;
    error _p0 = default!;

    return (CurrentThread(), error.As(null!)!);
}

// CurrentThread returns the handle for the current thread.
// It is a pseudo handle that does not need to be closed.
public static Handle CurrentThread() {
    return Handle(~uintptr(2 - 1));
}

// GetProcAddressByOrdinal retrieves the address of the exported
// function from module by ordinal.
public static (System.UIntPtr, error) GetProcAddressByOrdinal(Handle module, System.UIntPtr ordinal) {
    System.UIntPtr proc = default;
    error err = default!;

    var (r0, _, e1) = syscall.Syscall(procGetProcAddress.Addr(), 2, uintptr(module), ordinal, 0);
    proc = uintptr(r0);
    if (proc == 0) {
        err = errnoErr(e1);
    }
    return ;

}

public static void Exit(nint code) {
    ExitProcess(uint32(code));
}

private static ptr<SecurityAttributes> makeInheritSa() {
    ref SecurityAttributes sa = ref heap(out ptr<SecurityAttributes> _addr_sa);
    sa.Length = uint32(@unsafe.Sizeof(sa));
    sa.InheritHandle = 1;
    return _addr__addr_sa!;
}

public static (Handle, error) Open(@string path, nint mode, uint perm) {
    Handle fd = default;
    error err = default!;

    if (len(path) == 0) {
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
    }
    var (h, e) = CreateFile(pathp, access, sharemode, sa, createmode, attrs, 0);
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
    if (raceenabled) {
        if (done > 0) {
            raceWriteRange(@unsafe.Pointer(_addr_p[0]), int(done));
        }
        raceAcquire(@unsafe.Pointer(_addr_ioSync));

    }
    return (int(done), error.As(null!)!);

}

public static (nint, error) Write(Handle fd, slice<byte> p) {
    nint n = default;
    error err = default!;

    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
    }
    ref uint done = ref heap(out ptr<uint> _addr_done);
    var e = WriteFile(fd, p, _addr_done, null);
    if (e != null) {
        return (0, error.As(e)!);
    }
    if (raceenabled && done > 0) {
        raceReadRange(@unsafe.Pointer(_addr_p[0]), int(done));
    }
    return (int(done), error.As(null!)!);

}

private static long ioSync = default;

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
    ref var hi = ref heap(int32(offset >> 32), out ptr<var> _addr_hi);
    var lo = int32(offset); 
    // use GetFileType to check pipe, pipe can't do seek
    var (ft, _) = GetFileType(fd);
    if (ft == FILE_TYPE_PIPE) {
        return (0, error.As(syscall.EPIPE)!);
    }
    var (rlo, e) = SetFilePointer(fd, lo, _addr_hi, w);
    if (e != null) {
        return (0, error.As(e)!);
    }
    return (int64(hi) << 32 + int64(rlo), error.As(null!)!);

}

public static error Close(Handle fd) {
    error err = default!;

    return error.As(CloseHandle(fd))!;
}

public static var Stdin = getStdHandle(STD_INPUT_HANDLE);public static var Stdout = getStdHandle(STD_OUTPUT_HANDLE);public static var Stderr = getStdHandle(STD_ERROR_HANDLE);

private static Handle getStdHandle(uint stdhandle) {
    Handle fd = default;

    var (r, _) = GetStdHandle(stdhandle);
    CloseOnExec(r);
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
    return error.As(MoveFileEx(from, to, MOVEFILE_REPLACE_EXISTING))!;

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

public static time.Duration DurationSinceBoot() {
    return time.Duration(getTickCount64()) * time.Millisecond;
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
        return error.As(syscall.EINVAL)!;
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
        return error.As(syscall.EINVAL)!;
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
        return error.As(syscall.EINVAL)!;
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

public static error LoadGetSystemTimePreciseAsFileTime() {
    return error.As(procGetSystemTimePreciseAsFileTime.Find())!;
}

public static error LoadCancelIoEx() {
    return error.As(procCancelIoEx.Find())!;
}

public static error LoadSetFileCompletionNotificationModes() {
    return error.As(procSetFileCompletionNotificationModes.Find())!;
}

public static (uint, error) WaitForMultipleObjects(slice<Handle> handles, bool waitAll, uint waitMilliseconds) {
    uint @event = default;
    error err = default!;
 
    // Every other win32 array API takes arguments as "pointer, count", except for this function. So we
    // can't declare it as a usual [] type, because mksyscall will use the opposite order. We therefore
    // trivially stub this ourselves.

    ptr<Handle> handlePtr;
    if (len(handles) > 0) {
        handlePtr = _addr_handles[0];
    }
    return waitForMultipleObjects(uint32(len(handles)), uintptr(@unsafe.Pointer(handlePtr)), waitAll, waitMilliseconds);

}

// net api calls

private static readonly var socket_error = uintptr(~uint32(0));

//sys    WSAStartup(verreq uint32, data *WSAData) (sockerr error) = ws2_32.WSAStartup
//sys    WSACleanup() (err error) [failretval==socket_error] = ws2_32.WSACleanup
//sys    WSAIoctl(s Handle, iocc uint32, inbuf *byte, cbif uint32, outbuf *byte, cbob uint32, cbbr *uint32, overlapped *Overlapped, completionRoutine uintptr) (err error) [failretval==socket_error] = ws2_32.WSAIoctl
//sys    socket(af int32, typ int32, protocol int32) (handle Handle, err error) [failretval==InvalidHandle] = ws2_32.socket
//sys    sendto(s Handle, buf []byte, flags int32, to unsafe.Pointer, tolen int32) (err error) [failretval==socket_error] = ws2_32.sendto
//sys    recvfrom(s Handle, buf []byte, flags int32, from *RawSockaddrAny, fromlen *int32) (n int32, err error) [failretval==-1] = ws2_32.recvfrom
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
//sys    WSASocket(af int32, typ int32, protocol int32, protoInfo *WSAProtocolInfo, group uint32, flags uint32) (handle Handle, err error) [failretval==InvalidHandle] = ws2_32.WSASocketW
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
//sys    WSAGetOverlappedResult(h Handle, o *Overlapped, bytes *uint32, wait bool, flags *uint32) (err error) = ws2_32.WSAGetOverlappedResult
//sys    GetAdaptersAddresses(family uint32, flags uint32, reserved uintptr, adapterAddresses *IpAdapterAddresses, sizePointer *uint32) (errcode error) = iphlpapi.GetAdaptersAddresses
//sys    GetACP() (acp uint32) = kernel32.GetACP
//sys    MultiByteToWideChar(codePage uint32, dwFlags uint32, str *byte, nstr int32, wchar *uint16, nwchar int32) (nwrite int32, err error) = kernel32.MultiByteToWideChar

// For testing: clients can set this flag to force
// creation of IPv6 sockets to return EAFNOSUPPORT.


//sys    WSAStartup(verreq uint32, data *WSAData) (sockerr error) = ws2_32.WSAStartup
//sys    WSACleanup() (err error) [failretval==socket_error] = ws2_32.WSACleanup
//sys    WSAIoctl(s Handle, iocc uint32, inbuf *byte, cbif uint32, outbuf *byte, cbob uint32, cbbr *uint32, overlapped *Overlapped, completionRoutine uintptr) (err error) [failretval==socket_error] = ws2_32.WSAIoctl
//sys    socket(af int32, typ int32, protocol int32) (handle Handle, err error) [failretval==InvalidHandle] = ws2_32.socket
//sys    sendto(s Handle, buf []byte, flags int32, to unsafe.Pointer, tolen int32) (err error) [failretval==socket_error] = ws2_32.sendto
//sys    recvfrom(s Handle, buf []byte, flags int32, from *RawSockaddrAny, fromlen *int32) (n int32, err error) [failretval==-1] = ws2_32.recvfrom
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
//sys    WSASocket(af int32, typ int32, protocol int32, protoInfo *WSAProtocolInfo, group uint32, flags uint32) (handle Handle, err error) [failretval==InvalidHandle] = ws2_32.WSASocketW
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
//sys    WSAGetOverlappedResult(h Handle, o *Overlapped, bytes *uint32, wait bool, flags *uint32) (err error) = ws2_32.WSAGetOverlappedResult
//sys    GetAdaptersAddresses(family uint32, flags uint32, reserved uintptr, adapterAddresses *IpAdapterAddresses, sizePointer *uint32) (errcode error) = iphlpapi.GetAdaptersAddresses
//sys    GetACP() (acp uint32) = kernel32.GetACP
//sys    MultiByteToWideChar(codePage uint32, dwFlags uint32, str *byte, nstr int32, wchar *uint16, nwchar int32) (nwrite int32, err error) = kernel32.MultiByteToWideChar

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
        return (null, 0, error.As(syscall.EINVAL)!);
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
        return (null, 0, error.As(syscall.EINVAL)!);
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
        return (null, 0, error.As(syscall.EINVAL)!);
    }
    if (n == len(sa.raw.Path) && name[0] != '@') {
        return (null, 0, error.As(syscall.EINVAL)!);
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
        ptr<array<byte>> bytes = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Path[0]))[(int)0..(int)n];
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
        return (null, error.As(syscall.EAFNOSUPPORT)!);

}

public static (Handle, error) Socket(nint domain, nint typ, nint proto) {
    Handle fd = default;
    error err = default!;

    if (domain == AF_INET6 && SocketDisableIPv6) {
        return (InvalidHandle, error.As(syscall.EAFNOSUPPORT)!);
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

    var (rsa, l, err) = to.sockaddr();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(WSASendTo(s, bufs, bufcnt, sent, flags, (RawSockaddrAny.val)(@unsafe.Pointer(rsa)), l, overlapped, croutine))!;

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

    var (r1, _, e1) = syscall.Syscall9(connectExFunc.addr, 7, uintptr(s), uintptr(name), uintptr(namelen), uintptr(@unsafe.Pointer(sendBuf)), uintptr(sendDataLen), uintptr(@unsafe.Pointer(bytesSent)), uintptr(@unsafe.Pointer(overlapped)), 0, 0);
    if (r1 == 0) {
        if (e1 != 0) {
            err = error(e1);
        }
        else
 {
            err = syscall.EINVAL;
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

private static var sendRecvMsgFunc = default;

private static error loadWSASendRecvMsg() => func((defer, _, _) => {
    sendRecvMsgFunc.once.Do(() => {
        Handle s = default;
        s, sendRecvMsgFunc.err = Socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
        if (sendRecvMsgFunc.err != null) {
            return ;
        }
        defer(CloseHandle(s));
        ref uint n = ref heap(out ptr<uint> _addr_n);
        sendRecvMsgFunc.err = WSAIoctl(s, SIO_GET_EXTENSION_FUNCTION_POINTER, (byte.val)(@unsafe.Pointer(_addr_WSAID_WSARECVMSG)), uint32(@unsafe.Sizeof(WSAID_WSARECVMSG)), (byte.val)(@unsafe.Pointer(_addr_sendRecvMsgFunc.recvAddr)), uint32(@unsafe.Sizeof(sendRecvMsgFunc.recvAddr)), _addr_n, null, 0);
        if (sendRecvMsgFunc.err != null) {
            return ;
        }
        sendRecvMsgFunc.err = WSAIoctl(s, SIO_GET_EXTENSION_FUNCTION_POINTER, (byte.val)(@unsafe.Pointer(_addr_WSAID_WSASENDMSG)), uint32(@unsafe.Sizeof(WSAID_WSASENDMSG)), (byte.val)(@unsafe.Pointer(_addr_sendRecvMsgFunc.sendAddr)), uint32(@unsafe.Sizeof(sendRecvMsgFunc.sendAddr)), _addr_n, null, 0);

    });
    return error.As(sendRecvMsgFunc.err)!;

});

public static error WSASendMsg(Handle fd, ptr<WSAMsg> _addr_msg, uint flags, ptr<uint> _addr_bytesSent, ptr<Overlapped> _addr_overlapped, ptr<byte> _addr_croutine) {
    ref WSAMsg msg = ref _addr_msg.val;
    ref uint bytesSent = ref _addr_bytesSent.val;
    ref Overlapped overlapped = ref _addr_overlapped.val;
    ref byte croutine = ref _addr_croutine.val;

    var err = loadWSASendRecvMsg();
    if (err != null) {
        return error.As(err)!;
    }
    var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.sendAddr, 6, uintptr(fd), uintptr(@unsafe.Pointer(msg)), uintptr(flags), uintptr(@unsafe.Pointer(bytesSent)), uintptr(@unsafe.Pointer(overlapped)), uintptr(@unsafe.Pointer(croutine)));
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return error.As(err)!;

}

public static error WSARecvMsg(Handle fd, ptr<WSAMsg> _addr_msg, ptr<uint> _addr_bytesReceived, ptr<Overlapped> _addr_overlapped, ptr<byte> _addr_croutine) {
    ref WSAMsg msg = ref _addr_msg.val;
    ref uint bytesReceived = ref _addr_bytesReceived.val;
    ref Overlapped overlapped = ref _addr_overlapped.val;
    ref byte croutine = ref _addr_croutine.val;

    var err = loadWSASendRecvMsg();
    if (err != null) {
        return error.As(err)!;
    }
    var (r1, _, e1) = syscall.Syscall6(sendRecvMsgFunc.recvAddr, 5, uintptr(fd), uintptr(@unsafe.Pointer(msg)), uintptr(@unsafe.Pointer(bytesReceived)), uintptr(@unsafe.Pointer(overlapped)), uintptr(@unsafe.Pointer(croutine)), 0);
    if (r1 == socket_error) {
        err = errnoErr(e1);
    }
    return error.As(err)!;

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
// consistency with the corresponding package for other operating systems.
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

    return (0, null, error.As(syscall.EWINDOWS)!);
}

public static (nint, Sockaddr, error) Recvfrom(Handle fd, slice<byte> p, nint flags) {
    nint n = default;
    Sockaddr from = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref var l = ref heap(int32(@unsafe.Sizeof(rsa)), out ptr<var> _addr_l);
    var (n32, err) = recvfrom(fd, p, int32(flags), _addr_rsa, _addr_l);
    n = int(n32);
    if (err != null) {
        return ;
    }
    from, err = rsa.Sockaddr();
    return ;

}

public static error Sendto(Handle fd, slice<byte> p, nint flags, Sockaddr to) {
    error err = default!;

    var (ptr, l, err) = to.sockaddr();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(sendto(fd, p, int32(flags), ptr, l))!;

}

public static error SetsockoptTimeval(Handle fd, nint level, nint opt, ptr<Timeval> _addr_tv) {
    error err = default!;
    ref Timeval tv = ref _addr_tv.val;

    return error.As(syscall.EWINDOWS)!;
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

    ref var v = ref heap(int32(0), out ptr<var> _addr_v);
    ref var l = ref heap(int32(@unsafe.Sizeof(v)), out ptr<var> _addr_l);
    var err = Getsockopt(fd, int32(level), int32(opt), (byte.val)(@unsafe.Pointer(_addr_v)), _addr_l);
    return (int(v), error.As(err)!);
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

    return error.As(syscall.EWINDOWS)!;
}

public static nint Getpid() {
    nint pid = default;

    return int(GetCurrentProcessId());
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

    return error.As(syscall.EWINDOWS)!;
}
public static error Link(@string oldpath, @string newpath) {
    error err = default!;

    return error.As(syscall.EWINDOWS)!;
}
public static error Symlink(@string path, @string link) {
    error err = default!;

    return error.As(syscall.EWINDOWS)!;
}

public static error Fchmod(Handle fd, uint mode) {
    error err = default!;

    return error.As(syscall.EWINDOWS)!;
}
public static error Chown(@string path, nint uid, nint gid) {
    error err = default!;

    return error.As(syscall.EWINDOWS)!;
}
public static error Lchown(@string path, nint uid, nint gid) {
    error err = default!;

    return error.As(syscall.EWINDOWS)!;
}
public static error Fchown(Handle fd, nint uid, nint gid) {
    error err = default!;

    return error.As(syscall.EWINDOWS)!;
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

    return (null, error.As(syscall.EWINDOWS)!);
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
    return "signal " + itoa(int(s));

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
        s = UTF16ToString(p[(int)data.PrintNameOffset / 2..(int)(data.PrintNameLength - data.PrintNameOffset) / 2]);
    else if (rdb.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT) 
        data = (mountPointReparseBuffer.val)(@unsafe.Pointer(_addr_rdb.reparseBuffer));
        p = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(_addr_data.PathBuffer[0]));
        s = UTF16ToString(p[(int)data.PrintNameOffset / 2..(int)(data.PrintNameLength - data.PrintNameOffset) / 2]);
    else 
        // the path is not a symlink or junction but another type of reparse
        // point
        return (-1, error.As(syscall.ENOENT)!);
        n = copy(buf, (slice<byte>)s);

    return (n, error.As(null!)!);

});

// GUIDFromString parses a string in the form of
// "{XXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}" into a GUID.
public static (GUID, error) GUIDFromString(@string str) {
    GUID _p0 = default;
    error _p0 = default!;

    ref GUID guid = ref heap(new GUID(), out ptr<GUID> _addr_guid);
    var (str16, err) = syscall.UTF16PtrFromString(str);
    if (err != null) {
        return (guid, error.As(err)!);
    }
    err = clsidFromString(str16, _addr_guid);
    if (err != null) {
        return (guid, error.As(err)!);
    }
    return (guid, error.As(null!)!);

}

// GenerateGUID creates a new random GUID.
public static (GUID, error) GenerateGUID() {
    GUID _p0 = default;
    error _p0 = default!;

    ref GUID guid = ref heap(new GUID(), out ptr<GUID> _addr_guid);
    var err = coCreateGuid(_addr_guid);
    if (err != null) {
        return (guid, error.As(err)!);
    }
    return (guid, error.As(null!)!);

}

// String returns the canonical string form of the GUID,
// in the form of "{XXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}".
public static @string String(this GUID guid) {
    array<ushort> str = new array<ushort>(100);
    var chars = stringFromGUID2(_addr_guid, _addr_str[0], int32(len(str)));
    if (chars <= 1) {
        return "";
    }
    return string(utf16.Decode(str[..(int)chars - 1]));

}

// KnownFolderPath returns a well-known folder path for the current user, specified by one of
// the FOLDERID_ constants, and chosen and optionally created based on a KF_ flag.
public static (@string, error) KnownFolderPath(ptr<KNOWNFOLDERID> _addr_folderID, uint flags) {
    @string _p0 = default;
    error _p0 = default!;
    ref KNOWNFOLDERID folderID = ref _addr_folderID.val;

    return Token(0).KnownFolderPath(folderID, flags);
}

// KnownFolderPath returns a well-known folder path for the user token, specified by one of
// the FOLDERID_ constants, and chosen and optionally created based on a KF_ flag.
public static (@string, error) KnownFolderPath(this Token t, ptr<KNOWNFOLDERID> _addr_folderID, uint flags) => func((defer, _, _) => {
    @string _p0 = default;
    error _p0 = default!;
    ref KNOWNFOLDERID folderID = ref _addr_folderID.val;

    ptr<ushort> p;
    var err = shGetKnownFolderPath(folderID, flags, t, _addr_p);
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(CoTaskMemFree(@unsafe.Pointer(p)));
    return (UTF16PtrToString(p), error.As(null!)!);

});

// RtlGetVersion returns the version of the underlying operating system, ignoring
// manifest semantics but is affected by the application compatibility layer.
public static ptr<OsVersionInfoEx> RtlGetVersion() {
    ptr<OsVersionInfoEx> info = addr(new OsVersionInfoEx());
    info.osVersionInfoSize = uint32(@unsafe.Sizeof(info.val)); 
    // According to documentation, this function always succeeds.
    // The function doesn't even check the validity of the
    // osVersionInfoSize member. Disassembling ntdll.dll indicates
    // that the documentation is indeed correct about that.
    _ = rtlGetVersion(info);
    return _addr_info!;

}

// RtlGetNtVersionNumbers returns the version of the underlying operating system,
// ignoring manifest semantics and the application compatibility layer.
public static (uint, uint, uint) RtlGetNtVersionNumbers() {
    uint majorVersion = default;
    uint minorVersion = default;
    uint buildNumber = default;

    rtlGetNtVersionNumbers(_addr_majorVersion, _addr_minorVersion, _addr_buildNumber);
    buildNumber &= 0xffff;
    return ;
}

// GetProcessPreferredUILanguages retrieves the process preferred UI languages.
public static (slice<@string>, error) GetProcessPreferredUILanguages(uint flags) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    return getUILanguages(flags, getProcessPreferredUILanguages);
}

// GetThreadPreferredUILanguages retrieves the thread preferred UI languages for the current thread.
public static (slice<@string>, error) GetThreadPreferredUILanguages(uint flags) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    return getUILanguages(flags, getThreadPreferredUILanguages);
}

// GetUserPreferredUILanguages retrieves information about the user preferred UI languages.
public static (slice<@string>, error) GetUserPreferredUILanguages(uint flags) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    return getUILanguages(flags, getUserPreferredUILanguages);
}

// GetSystemPreferredUILanguages retrieves the system preferred UI languages.
public static (slice<@string>, error) GetSystemPreferredUILanguages(uint flags) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    return getUILanguages(flags, getSystemPreferredUILanguages);
}

private static (slice<@string>, error) getUILanguages(uint flags, Func<uint, ptr<uint>, ptr<ushort>, ptr<uint>, error> f) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    ref var size = ref heap(uint32(128), out ptr<var> _addr_size);
    while (true) {
        ref uint numLanguages = ref heap(out ptr<uint> _addr_numLanguages);
        var buf = make_slice<ushort>(size);
        var err = f(flags, _addr_numLanguages, _addr_buf[0], _addr_size);
        if (err == ERROR_INSUFFICIENT_BUFFER) {
            continue;
        }
        if (err != null) {
            return (null, error.As(err)!);
        }
        buf = buf[..(int)size];
        if (numLanguages == 0 || len(buf) == 0) { // GetProcessPreferredUILanguages may return numLanguages==0 with "\0\0"
            return (new slice<@string>(new @string[] {  }), error.As(null!)!);

        }
        if (buf[len(buf) - 1] == 0) {
            buf = buf[..(int)len(buf) - 1]; // remove terminating null
        }
        var languages = make_slice<@string>(0, numLanguages);
        nint from = 0;
        foreach (var (i, c) in buf) {
            if (c == 0) {
                languages = append(languages, string(utf16.Decode(buf[(int)from..(int)i])));
                from = i + 1;
            }
        }        return (languages, error.As(null!)!);

    }

}

public static error SetConsoleCursorPosition(Handle console, Coord position) {
    return error.As(setConsoleCursorPosition(console, ((uint32.val)(@unsafe.Pointer(_addr_position))).val))!;
}

public static syscall.Errno Errno(this NTStatus s) {
    return rtlNtStatusToDosErrorNoTeb(s);
}

private static uint langID(ushort pri, ushort sub) {
    return uint32(sub) << 10 | uint32(pri);
}

public static @string Error(this NTStatus s) {
    var b = make_slice<ushort>(300);
    var (n, err) = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_FROM_HMODULE | FORMAT_MESSAGE_ARGUMENT_ARRAY, modntdll.Handle(), uint32(s), langID(LANG_ENGLISH, SUBLANG_ENGLISH_US), b, null);
    if (err != null) {
        return fmt.Sprintf("NTSTATUS 0x%08x", uint32(s));
    }
    while (n > 0 && (b[n - 1] == '\n' || b[n - 1] == '\r')) {
        n--;
    }
    return string(utf16.Decode(b[..(int)n]));

}

// NewNTUnicodeString returns a new NTUnicodeString structure for use with native
// NT APIs that work over the NTUnicodeString type. Note that most Windows APIs
// do not use NTUnicodeString, and instead UTF16PtrFromString should be used for
// the more common *uint16 string type.
public static (ptr<NTUnicodeString>, error) NewNTUnicodeString(@string s) {
    ptr<NTUnicodeString> _p0 = default!;
    error _p0 = default!;

    ref NTUnicodeString u = ref heap(out ptr<NTUnicodeString> _addr_u);
    var (s16, err) = UTF16PtrFromString(s);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    RtlInitUnicodeString(_addr_u, s16);
    return (_addr__addr_u!, error.As(null!)!);

}

// Slice returns a uint16 slice that aliases the data in the NTUnicodeString.
private static slice<ushort> Slice(this ptr<NTUnicodeString> _addr_s) {
    ref NTUnicodeString s = ref _addr_s.val;

    ref slice<ushort> slice = ref heap(out ptr<slice<ushort>> _addr_slice);
    var hdr = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_slice));
    hdr.Data = @unsafe.Pointer(s.Buffer);
    hdr.Len = int(s.Length);
    hdr.Cap = int(s.MaximumLength);
    return slice;
}

private static @string String(this ptr<NTUnicodeString> _addr_s) {
    ref NTUnicodeString s = ref _addr_s.val;

    return UTF16ToString(s.Slice());
}

// NewNTString returns a new NTString structure for use with native
// NT APIs that work over the NTString type. Note that most Windows APIs
// do not use NTString, and instead UTF16PtrFromString should be used for
// the more common *uint16 string type.
public static (ptr<NTString>, error) NewNTString(@string s) {
    ptr<NTString> _p0 = default!;
    error _p0 = default!;

    ref NTString nts = ref heap(out ptr<NTString> _addr_nts);
    var (s8, err) = BytePtrFromString(s);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    RtlInitString(_addr_nts, s8);
    return (_addr__addr_nts!, error.As(null!)!);

}

// Slice returns a byte slice that aliases the data in the NTString.
private static slice<byte> Slice(this ptr<NTString> _addr_s) {
    ref NTString s = ref _addr_s.val;

    ref slice<byte> slice = ref heap(out ptr<slice<byte>> _addr_slice);
    var hdr = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_slice));
    hdr.Data = @unsafe.Pointer(s.Buffer);
    hdr.Len = int(s.Length);
    hdr.Cap = int(s.MaximumLength);
    return slice;
}

private static @string String(this ptr<NTString> _addr_s) {
    ref NTString s = ref _addr_s.val;

    return ByteSliceToString(s.Slice());
}

// FindResource resolves a resource of the given name and resource type.
public static (Handle, error) FindResource(Handle module, ResourceIDOrString name, ResourceIDOrString resType) {
    Handle _p0 = default;
    error _p0 = default!;

    System.UIntPtr namePtr = default;    System.UIntPtr resTypePtr = default;

    ptr<ushort> name16;    ptr<ushort> resType16;

    error err = default!;
    Func<object, ptr<ptr<ushort>>, (System.UIntPtr, error)> resolvePtr = (i, keep) => {
        switch (i.type()) {
            case @string v:
                keep.val, err = UTF16PtrFromString(v);
                if (err != null) {
                    return (0, error.As(err)!);
                }
                return (uintptr(@unsafe.Pointer(keep.val)), error.As(null!)!);
                break;
            case ResourceID v:
                return (uintptr(v), error.As(null!)!);
                break;
        }
        return (0, error.As(errorspkg.New("parameter must be a ResourceID or a string"))!);

    };
    namePtr, err = resolvePtr(name, _addr_name16);
    if (err != null) {
        return (0, error.As(err)!);
    }
    resTypePtr, err = resolvePtr(resType, _addr_resType16);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var (resInfo, err) = findResource(module, namePtr, resTypePtr);
    runtime.KeepAlive(name16);
    runtime.KeepAlive(resType16);
    return (resInfo, error.As(err)!);

}

public static (slice<byte>, error) LoadResourceData(Handle module, Handle resInfo) {
    slice<byte> data = default;
    error err = default!;

    var (size, err) = SizeofResource(module, resInfo);
    if (err != null) {
        return ;
    }
    var (resData, err) = LoadResource(module, resInfo);
    if (err != null) {
        return ;
    }
    var (ptr, err) = LockResource(resData);
    if (err != null) {
        return ;
    }
    var h = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_data));
    h.Data = @unsafe.Pointer(ptr);
    h.Len = int(size);
    h.Cap = int(size);
    return ;

}

} // end windows_package
