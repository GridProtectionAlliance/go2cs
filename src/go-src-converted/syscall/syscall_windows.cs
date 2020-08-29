// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows system calls.

// package syscall -- go2cs converted at 2020 August 29 08:38:28 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_windows.go
using errorspkg = go.errors_package;
using race = go.@internal.race_package;
using sync = go.sync_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class syscall_package
    {
        public partial struct Handle // : System.UIntPtr
        {
        }

        public static readonly var InvalidHandle = ~Handle(0L);

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
        public static slice<ushort> StringToUTF16(@string s) => func((_, panic, __) =>
        {
            var (a, err) = UTF16FromString(s);
            if (err != null)
            {
                panic("syscall: string with NUL passed to StringToUTF16");
            }
            return a;
        });

        // UTF16FromString returns the UTF-16 encoding of the UTF-8 string
        // s, with a terminating NUL added. If s contains a NUL byte at any
        // location, it returns (nil, EINVAL).
        public static (slice<ushort>, error) UTF16FromString(@string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] == 0L)
                {
                    return (null, EINVAL);
                }
            }

            return (utf16.Encode((slice<int>)s + "\x00"), null);
        }

        // UTF16ToString returns the UTF-8 encoding of the UTF-16 sequence s,
        // with a terminating NUL removed.
        public static @string UTF16ToString(slice<ushort> s)
        {
            foreach (var (i, v) in s)
            {
                if (v == 0L)
                {
                    s = s[0L..i];
                    break;
                }
            }
            return string(utf16.Decode(s));
        }

        // StringToUTF16Ptr returns pointer to the UTF-16 encoding of
        // the UTF-8 string s, with a terminating NUL added. If s
        // contains a NUL byte this function panics instead of
        // returning an error.
        //
        // Deprecated: Use UTF16PtrFromString instead.
        public static ref ushort StringToUTF16Ptr(@string s)
        {
            return ref StringToUTF16(s)[0L];
        }

        // UTF16PtrFromString returns pointer to the UTF-16 encoding of
        // the UTF-8 string s, with a terminating NUL added. If s
        // contains a NUL byte at any location, it returns (nil, EINVAL).
        public static (ref ushort, error) UTF16PtrFromString(@string s)
        {
            var (a, err) = UTF16FromString(s);
            if (err != null)
            {
                return (null, err);
            }
            return (ref a[0L], null);
        }

        // Errno is the Windows error number.
        public partial struct Errno // : System.UIntPtr
        {
        }

        private static uint langid(ushort pri, ushort sub)
        {
            return uint32(sub) << (int)(10L) | uint32(pri);
        }

        // FormatMessage is deprecated (msgsrc should be uintptr, not uint32, but can
        // not be changed due to the Go 1 compatibility guarantee).
        //
        // Deprecated: Use FormatMessage from golang.org/x/sys/windows instead.
        public static (uint, error) FormatMessage(uint flags, uint msgsrc, uint msgid, uint langid, slice<ushort> buf, ref byte args)
        {
            return formatMessage(flags, uintptr(msgsrc), msgid, langid, buf, args);
        }

        public static @string Error(this Errno e)
        { 
            // deal with special go errors
            var idx = int(e - APPLICATION_ERROR);
            if (0L <= idx && idx < len(errors))
            {
                return errors[idx];
            } 
            // ask windows for the remaining errors
            uint flags = FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ARGUMENT_ARRAY | FORMAT_MESSAGE_IGNORE_INSERTS;
            var b = make_slice<ushort>(300L);
            var (n, err) = formatMessage(flags, 0L, uint32(e), langid(LANG_ENGLISH, SUBLANG_ENGLISH_US), b, null);
            if (err != null)
            {
                n, err = formatMessage(flags, 0L, uint32(e), 0L, b, null);
                if (err != null)
                {
                    return "winapi error #" + itoa(int(e));
                }
            } 
            // trim terminating \r and \n
            while (n > 0L && (b[n - 1L] == '\n' || b[n - 1L] == '\r'))
            {
                n--;
            }

            return string(utf16.Decode(b[..n]));
        }

        public static bool Temporary(this Errno e)
        {
            return e == EINTR || e == EMFILE || e == WSAECONNABORTED || e == WSAECONNRESET || e.Timeout();
        }

        public static bool Timeout(this Errno e)
        {
            return e == EAGAIN || e == EWOULDBLOCK || e == ETIMEDOUT;
        }

        // Implemented in runtime/syscall_windows.go.
        private static System.UIntPtr compileCallback(object fn, bool cleanstack)
;

        // Converts a Go function to a function pointer conforming
        // to the stdcall calling convention. This is useful when
        // interoperating with Windows code requiring callbacks.
        public static System.UIntPtr NewCallback(object fn)
        {
            return compileCallback(fn, true);
        }

        // Converts a Go function to a function pointer conforming
        // to the cdecl calling convention. This is useful when
        // interoperating with Windows code requiring callbacks.
        public static System.UIntPtr NewCallbackCDecl(object fn)
        {
            return compileCallback(fn, false);
        }

        // windows api calls

        //sys    GetLastError() (lasterr error)
        //sys    LoadLibrary(libname string) (handle Handle, err error) = LoadLibraryW
        //sys    FreeLibrary(handle Handle) (err error)
        //sys    GetProcAddress(module Handle, procname string) (proc uintptr, err error)
        //sys    GetVersion() (ver uint32, err error)
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
        //sys    CreateIoCompletionPort(filehandle Handle, cphandle Handle, key uint32, threadcnt uint32) (handle Handle, err error)
        //sys    GetQueuedCompletionStatus(cphandle Handle, qty *uint32, key *uint32, overlapped **Overlapped, timeout uint32) (err error)
        //sys    PostQueuedCompletionStatus(cphandle Handle, qty uint32, key uint32, overlapped *Overlapped) (err error)
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
        //sys   CertOpenStore(storeProvider uintptr, msgAndCertEncodingType uint32, cryptProv uintptr, flags uint32, para uintptr) (handle Handle, err error) [failretval==InvalidHandle] = crypt32.CertOpenStore
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

        // syscall interface implementation for other packages

        private static ref SecurityAttributes makeInheritSa()
        {
            SecurityAttributes sa = default;
            sa.Length = uint32(@unsafe.Sizeof(sa));
            sa.InheritHandle = 1L;
            return ref sa;
        }

        public static (Handle, error) Open(@string path, long mode, uint perm)
        {
            if (len(path) == 0L)
            {>>MARKER:FUNCTION_compileCallback_BLOCK_PREFIX<<
                return (InvalidHandle, ERROR_FILE_NOT_FOUND);
            }
            var (pathp, err) = UTF16PtrFromString(path);
            if (err != null)
            {
                return (InvalidHandle, err);
            }
            uint access = default;

            if (mode & (O_RDONLY | O_WRONLY | O_RDWR) == O_RDONLY) 
                access = GENERIC_READ;
            else if (mode & (O_RDONLY | O_WRONLY | O_RDWR) == O_WRONLY) 
                access = GENERIC_WRITE;
            else if (mode & (O_RDONLY | O_WRONLY | O_RDWR) == O_RDWR) 
                access = GENERIC_READ | GENERIC_WRITE;
                        if (mode & O_CREAT != 0L)
            {
                access |= GENERIC_WRITE;
            }
            if (mode & O_APPEND != 0L)
            {
                access &= GENERIC_WRITE;
                access |= FILE_APPEND_DATA;
            }
            var sharemode = uint32(FILE_SHARE_READ | FILE_SHARE_WRITE);
            ref SecurityAttributes sa = default;
            if (mode & O_CLOEXEC == 0L)
            {
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
                        var (h, e) = CreateFile(pathp, access, sharemode, sa, createmode, FILE_ATTRIBUTE_NORMAL, 0L);
            return (h, e);
        }

        public static (long, error) Read(Handle fd, slice<byte> p)
        {
            uint done = default;
            var e = ReadFile(fd, p, ref done, null);
            if (e != null)
            {
                if (e == ERROR_BROKEN_PIPE)
                { 
                    // NOTE(brainman): work around ERROR_BROKEN_PIPE is returned on reading EOF from stdin
                    return (0L, null);
                }
                return (0L, e);
            }
            if (race.Enabled)
            {
                if (done > 0L)
                {
                    race.WriteRange(@unsafe.Pointer(ref p[0L]), int(done));
                }
                race.Acquire(@unsafe.Pointer(ref ioSync));
            }
            if (msanenabled && done > 0L)
            {
                msanWrite(@unsafe.Pointer(ref p[0L]), int(done));
            }
            return (int(done), null);
        }

        public static (long, error) Write(Handle fd, slice<byte> p)
        {
            if (race.Enabled)
            {
                race.ReleaseMerge(@unsafe.Pointer(ref ioSync));
            }
            uint done = default;
            var e = WriteFile(fd, p, ref done, null);
            if (e != null)
            {
                return (0L, e);
            }
            if (race.Enabled && done > 0L)
            {
                race.ReadRange(@unsafe.Pointer(ref p[0L]), int(done));
            }
            if (msanenabled && done > 0L)
            {
                msanRead(@unsafe.Pointer(ref p[0L]), int(done));
            }
            return (int(done), null);
        }

        private static long ioSync = default;

        private static var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx");

        private static readonly var ptrSize = @unsafe.Sizeof(uintptr(0L));

        // setFilePointerEx calls SetFilePointerEx.
        // See https://msdn.microsoft.com/en-us/library/windows/desktop/aa365542(v=vs.85).aspx


        // setFilePointerEx calls SetFilePointerEx.
        // See https://msdn.microsoft.com/en-us/library/windows/desktop/aa365542(v=vs.85).aspx
        private static error setFilePointerEx(Handle handle, long distToMove, ref long newFilePointer, uint whence)
        {
            Errno e1 = default;
            if (ptrSize == 8L)
            {
                _, _, e1 = Syscall6(procSetFilePointerEx.Addr(), 4L, uintptr(handle), uintptr(distToMove), uintptr(@unsafe.Pointer(newFilePointer)), uintptr(whence), 0L, 0L);
            }
            else
            { 
                // distToMove is a LARGE_INTEGER:
                // https://msdn.microsoft.com/en-us/library/windows/desktop/aa383713(v=vs.85).aspx
                _, _, e1 = Syscall6(procSetFilePointerEx.Addr(), 5L, uintptr(handle), uintptr(distToMove), uintptr(distToMove >> (int)(32L)), uintptr(@unsafe.Pointer(newFilePointer)), uintptr(whence), 0L);
            }
            if (e1 != 0L)
            {
                return error.As(errnoErr(e1));
            }
            return error.As(null);
        }

        public static (long, error) Seek(Handle fd, long offset, long whence)
        {
            uint w = default;
            switch (whence)
            {
                case 0L: 
                    w = FILE_BEGIN;
                    break;
                case 1L: 
                    w = FILE_CURRENT;
                    break;
                case 2L: 
                    w = FILE_END;
                    break;
            } 
            // use GetFileType to check pipe, pipe can't do seek
            var (ft, _) = GetFileType(fd);
            if (ft == FILE_TYPE_PIPE)
            {
                return (0L, ESPIPE);
            }
            err = setFilePointerEx(fd, offset, ref newoffset, w);
            return;
        }

        public static error Close(Handle fd)
        {
            return error.As(CloseHandle(fd));
        }

        public static var Stdin = getStdHandle(STD_INPUT_HANDLE);        public static var Stdout = getStdHandle(STD_OUTPUT_HANDLE);        public static var Stderr = getStdHandle(STD_ERROR_HANDLE);

        private static Handle getStdHandle(long h)
        {
            var (r, _) = GetStdHandle(h);
            CloseOnExec(r);
            return r;
        }

        public static readonly var ImplementsGetwd = true;



        public static (@string, error) Getwd()
        {
            var b = make_slice<ushort>(300L);
            var (n, e) = GetCurrentDirectory(uint32(len(b)), ref b[0L]);
            if (e != null)
            {
                return ("", e);
            }
            return (string(utf16.Decode(b[0L..n])), null);
        }

        public static error Chdir(@string path)
        {
            var (pathp, err) = UTF16PtrFromString(path);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(SetCurrentDirectory(pathp));
        }

        public static error Mkdir(@string path, uint mode)
        {
            var (pathp, err) = UTF16PtrFromString(path);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(CreateDirectory(pathp, null));
        }

        public static error Rmdir(@string path)
        {
            var (pathp, err) = UTF16PtrFromString(path);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(RemoveDirectory(pathp));
        }

        public static error Unlink(@string path)
        {
            var (pathp, err) = UTF16PtrFromString(path);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(DeleteFile(pathp));
        }

        public static error Rename(@string oldpath, @string newpath)
        {
            var (from, err) = UTF16PtrFromString(oldpath);
            if (err != null)
            {
                return error.As(err);
            }
            var (to, err) = UTF16PtrFromString(newpath);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(MoveFile(from, to));
        }

        public static (@string, error) ComputerName()
        {
            uint n = MAX_COMPUTERNAME_LENGTH + 1L;
            var b = make_slice<ushort>(n);
            var e = GetComputerName(ref b[0L], ref n);
            if (e != null)
            {
                return ("", e);
            }
            return (string(utf16.Decode(b[0L..n])), null);
        }

        public static error Ftruncate(Handle fd, long length) => func((defer, _, __) =>
        {
            var (curoffset, e) = Seek(fd, 0L, 1L);
            if (e != null)
            {
                return error.As(e);
            }
            defer(Seek(fd, curoffset, 0L));
            _, e = Seek(fd, length, 0L);
            if (e != null)
            {
                return error.As(e);
            }
            e = SetEndOfFile(fd);
            if (e != null)
            {
                return error.As(e);
            }
            return error.As(null);
        });

        public static error Gettimeofday(ref Timeval tv)
        {
            Filetime ft = default;
            GetSystemTimeAsFileTime(ref ft);
            tv.Value = NsecToTimeval(ft.Nanoseconds());
            return error.As(null);
        }

        public static error Pipe(slice<Handle> p)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL);
            }
            Handle r = default;            Handle w = default;

            var e = CreatePipe(ref r, ref w, makeInheritSa(), 0L);
            if (e != null)
            {
                return error.As(e);
            }
            p[0L] = r;
            p[1L] = w;
            return error.As(null);
        }

        public static error Utimes(@string path, slice<Timeval> tv) => func((defer, _, __) =>
        {
            if (len(tv) != 2L)
            {
                return error.As(EINVAL);
            }
            var (pathp, e) = UTF16PtrFromString(path);
            if (e != null)
            {
                return error.As(e);
            }
            var (h, e) = CreateFile(pathp, FILE_WRITE_ATTRIBUTES, FILE_SHARE_WRITE, null, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, 0L);
            if (e != null)
            {
                return error.As(e);
            }
            defer(Close(h));
            var a = NsecToFiletime(tv[0L].Nanoseconds());
            var w = NsecToFiletime(tv[1L].Nanoseconds());
            return error.As(SetFileTime(h, null, ref a, ref w));
        });

        public static error UtimesNano(@string path, slice<Timespec> ts) => func((defer, _, __) =>
        {
            if (len(ts) != 2L)
            {
                return error.As(EINVAL);
            }
            var (pathp, e) = UTF16PtrFromString(path);
            if (e != null)
            {
                return error.As(e);
            }
            var (h, e) = CreateFile(pathp, FILE_WRITE_ATTRIBUTES, FILE_SHARE_WRITE, null, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, 0L);
            if (e != null)
            {
                return error.As(e);
            }
            defer(Close(h));
            var a = NsecToFiletime(TimespecToNsec(ts[0L]));
            var w = NsecToFiletime(TimespecToNsec(ts[1L]));
            return error.As(SetFileTime(h, null, ref a, ref w));
        });

        public static error Fsync(Handle fd)
        {
            return error.As(FlushFileBuffers(fd));
        }

        public static error Chmod(@string path, uint mode)
        {
            if (mode == 0L)
            {
                return error.As(EINVAL);
            }
            var (p, e) = UTF16PtrFromString(path);
            if (e != null)
            {
                return error.As(e);
            }
            var (attrs, e) = GetFileAttributes(p);
            if (e != null)
            {
                return error.As(e);
            }
            if (mode & S_IWRITE != 0L)
            {
                attrs &= FILE_ATTRIBUTE_READONLY;
            }
            else
            {
                attrs |= FILE_ATTRIBUTE_READONLY;
            }
            return error.As(SetFileAttributes(p, attrs));
        }

        public static error LoadCancelIoEx()
        {
            return error.As(procCancelIoEx.Find());
        }

        public static error LoadSetFileCompletionNotificationModes()
        {
            return error.As(procSetFileCompletionNotificationModes.Find());
        }

        // net api calls

        private static readonly var socket_error = uintptr(~uint32(0L));

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

        public partial struct RawSockaddrInet4
        {
            public ushort Family;
            public ushort Port;
            public array<byte> Addr; /* in_addr */
            public array<byte> Zero;
        }

        public partial struct RawSockaddrInet6
        {
            public ushort Family;
            public ushort Port;
            public uint Flowinfo;
            public array<byte> Addr; /* in6_addr */
            public uint Scope_id;
        }

        public partial struct RawSockaddr
        {
            public ushort Family;
            public array<sbyte> Data;
        }

        public partial struct RawSockaddrAny
        {
            public RawSockaddr Addr;
            public array<sbyte> Pad;
        }

        public partial interface Sockaddr
        {
            (unsafe.Pointer, int, error) sockaddr(); // lowercase; only we can define Sockaddrs
        }

        public partial struct SockaddrInet4
        {
            public long Port;
            public array<byte> Addr;
            public RawSockaddrInet4 raw;
        }

        private static (unsafe.Pointer, int, error) sockaddr(this ref SockaddrInet4 sa)
        {
            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Family = AF_INET;
            ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(ref sa.raw), int32(@unsafe.Sizeof(sa.raw)), null);
        }

        public partial struct SockaddrInet6
        {
            public long Port;
            public uint ZoneId;
            public array<byte> Addr;
            public RawSockaddrInet6 raw;
        }

        private static (unsafe.Pointer, int, error) sockaddr(this ref SockaddrInet6 sa)
        {
            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Family = AF_INET6;
            ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            sa.raw.Scope_id = sa.ZoneId;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(ref sa.raw), int32(@unsafe.Sizeof(sa.raw)), null);
        }

        public partial struct SockaddrUnix
        {
            public @string Name;
        }

        private static (unsafe.Pointer, int, error) sockaddr(this ref SockaddrUnix sa)
        { 
            // TODO(brainman): implement SockaddrUnix.sockaddr()
            return (null, 0L, EWINDOWS);
        }

        private static (Sockaddr, error) Sockaddr(this ref RawSockaddrAny rsa)
        {

            if (rsa.Addr.Family == AF_UNIX) 
                return (null, EWINDOWS);
            else if (rsa.Addr.Family == AF_INET) 
                var pp = (RawSockaddrInet4.Value)(@unsafe.Pointer(rsa));
                ptr<SockaddrInet4> sa = @new<SockaddrInet4>();
                ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref pp.Port));
                sa.Port = int(p[0L]) << (int)(8L) + int(p[1L]);
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < len(sa.Addr); i++)
                    {
                        sa.Addr[i] = pp.Addr[i];
                    }


                    i = i__prev1;
                }
                return (sa, null);
            else if (rsa.Addr.Family == AF_INET6) 
                pp = (RawSockaddrInet6.Value)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrInet6>();
                p = new ptr<ref array<byte>>(@unsafe.Pointer(ref pp.Port));
                sa.Port = int(p[0L]) << (int)(8L) + int(p[1L]);
                sa.ZoneId = pp.Scope_id;
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(sa.Addr); i++)
                    {
                        sa.Addr[i] = pp.Addr[i];
                    }


                    i = i__prev1;
                }
                return (sa, null);
                        return (null, EAFNOSUPPORT);
        }

        public static (Handle, error) Socket(long domain, long typ, long proto)
        {
            if (domain == AF_INET6 && SocketDisableIPv6)
            {
                return (InvalidHandle, EAFNOSUPPORT);
            }
            return socket(int32(domain), int32(typ), int32(proto));
        }

        public static error SetsockoptInt(Handle fd, long level, long opt, long value)
        {
            var v = int32(value);
            return error.As(Setsockopt(fd, int32(level), int32(opt), (byte.Value)(@unsafe.Pointer(ref v)), int32(@unsafe.Sizeof(v))));
        }

        public static error Bind(Handle fd, Sockaddr sa)
        {
            var (ptr, n, err) = sa.sockaddr();
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(bind(fd, ptr, n));
        }

        public static error Connect(Handle fd, Sockaddr sa)
        {
            var (ptr, n, err) = sa.sockaddr();
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(connect(fd, ptr, n));
        }

        public static (Sockaddr, error) Getsockname(Handle fd)
        {
            RawSockaddrAny rsa = default;
            var l = int32(@unsafe.Sizeof(rsa));
            err = getsockname(fd, ref rsa, ref l);

            if (err != null)
            {
                return;
            }
            return rsa.Sockaddr();
        }

        public static (Sockaddr, error) Getpeername(Handle fd)
        {
            RawSockaddrAny rsa = default;
            var l = int32(@unsafe.Sizeof(rsa));
            err = getpeername(fd, ref rsa, ref l);

            if (err != null)
            {
                return;
            }
            return rsa.Sockaddr();
        }

        public static error Listen(Handle s, long n)
        {
            return error.As(listen(s, int32(n)));
        }

        public static error Shutdown(Handle fd, long how)
        {
            return error.As(shutdown(fd, int32(how)));
        }

        public static error WSASendto(Handle s, ref WSABuf bufs, uint bufcnt, ref uint sent, uint flags, Sockaddr to, ref Overlapped overlapped, ref byte croutine)
        {
            var (rsa, l, err) = to.sockaddr();
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(WSASendTo(s, bufs, bufcnt, sent, flags, (RawSockaddrAny.Value)(@unsafe.Pointer(rsa)), l, overlapped, croutine));
        }

        public static error LoadGetAddrInfo()
        {
            return error.As(procGetAddrInfoW.Find());
        }

        private static var connectExFunc = default;

        public static error LoadConnectEx() => func((defer, _, __) =>
        {
            connectExFunc.once.Do(() =>
            {
                Handle s = default;
                s, connectExFunc.err = Socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
                if (connectExFunc.err != null)
                {
                    return;
                }
                defer(CloseHandle(s));
                uint n = default;
                connectExFunc.err = WSAIoctl(s, SIO_GET_EXTENSION_FUNCTION_POINTER, (byte.Value)(@unsafe.Pointer(ref WSAID_CONNECTEX)), uint32(@unsafe.Sizeof(WSAID_CONNECTEX)), (byte.Value)(@unsafe.Pointer(ref connectExFunc.addr)), uint32(@unsafe.Sizeof(connectExFunc.addr)), ref n, null, 0L);
            });
            return error.As(connectExFunc.err);
        });

        private static error connectEx(Handle s, unsafe.Pointer name, int namelen, ref byte sendBuf, uint sendDataLen, ref uint bytesSent, ref Overlapped overlapped)
        {
            var (r1, _, e1) = Syscall9(connectExFunc.addr, 7L, uintptr(s), uintptr(name), uintptr(namelen), uintptr(@unsafe.Pointer(sendBuf)), uintptr(sendDataLen), uintptr(@unsafe.Pointer(bytesSent)), uintptr(@unsafe.Pointer(overlapped)), 0L, 0L);
            if (r1 == 0L)
            {
                if (e1 != 0L)
                {
                    err = error(e1);
                }
                else
                {
                    err = EINVAL;
                }
            }
            return;
        }

        public static error ConnectEx(Handle fd, Sockaddr sa, ref byte sendBuf, uint sendDataLen, ref uint bytesSent, ref Overlapped overlapped)
        {
            var err = LoadConnectEx();
            if (err != null)
            {
                return error.As(errorspkg.New("failed to find ConnectEx: " + err.Error()));
            }
            var (ptr, n, err) = sa.sockaddr();
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(connectEx(fd, ptr, n, sendBuf, sendDataLen, bytesSent, overlapped));
        }

        // Invented structures to support what package os expects.
        public partial struct Rusage
        {
            public Filetime CreationTime;
            public Filetime ExitTime;
            public Filetime KernelTime;
            public Filetime UserTime;
        }

        public partial struct WaitStatus
        {
            public uint ExitCode;
        }

        public static bool Exited(this WaitStatus w)
        {
            return true;
        }

        public static long ExitStatus(this WaitStatus w)
        {
            return int(w.ExitCode);
        }

        public static Signal Signal(this WaitStatus w)
        {
            return -1L;
        }

        public static bool CoreDump(this WaitStatus w)
        {
            return false;
        }

        public static bool Stopped(this WaitStatus w)
        {
            return false;
        }

        public static bool Continued(this WaitStatus w)
        {
            return false;
        }

        public static Signal StopSignal(this WaitStatus w)
        {
            return -1L;
        }

        public static bool Signaled(this WaitStatus w)
        {
            return false;
        }

        public static long TrapCause(this WaitStatus w)
        {
            return -1L;
        }

        // Timespec is an invented structure on Windows, but here for
        // consistency with the syscall package for other operating systems.
        public partial struct Timespec
        {
            public long Sec;
            public long Nsec;
        }

        public static long TimespecToNsec(Timespec ts)
        {
            return int64(ts.Sec) * 1e9F + int64(ts.Nsec);
        }

        public static Timespec NsecToTimespec(long nsec)
        {
            ts.Sec = nsec / 1e9F;
            ts.Nsec = nsec % 1e9F;
            return;
        }

        // TODO(brainman): fix all needed for net

        public static (Handle, Sockaddr, error) Accept(Handle fd)
        {
            return (0L, null, EWINDOWS);
        }
        public static (long, Sockaddr, error) Recvfrom(Handle fd, slice<byte> p, long flags)
        {
            return (0L, null, EWINDOWS);
        }
        public static error Sendto(Handle fd, slice<byte> p, long flags, Sockaddr to)
        {
            return error.As(EWINDOWS);
        }
        public static error SetsockoptTimeval(Handle fd, long level, long opt, ref Timeval tv)
        {
            return error.As(EWINDOWS);
        }

        // The Linger struct is wrong but we only noticed after Go 1.
        // sysLinger is the real system call structure.

        // BUG(brainman): The definition of Linger is not appropriate for direct use
        // with Setsockopt and Getsockopt.
        // Use SetsockoptLinger instead.

        public partial struct Linger
        {
            public int Onoff;
            public int Linger;
        }

        private partial struct sysLinger
        {
            public ushort Onoff;
            public ushort Linger;
        }

        public partial struct IPMreq
        {
            public array<byte> Multiaddr; /* in_addr */
            public array<byte> Interface; /* in_addr */
        }

        public partial struct IPv6Mreq
        {
            public array<byte> Multiaddr; /* in6_addr */
            public uint Interface;
        }

        public static (long, error) GetsockoptInt(Handle fd, long level, long opt)
        {
            return (-1L, EWINDOWS);
        }

        public static error SetsockoptLinger(Handle fd, long level, long opt, ref Linger l)
        {
            sysLinger sys = new sysLinger(Onoff:uint16(l.Onoff),Linger:uint16(l.Linger));
            return error.As(Setsockopt(fd, int32(level), int32(opt), (byte.Value)(@unsafe.Pointer(ref sys)), int32(@unsafe.Sizeof(sys))));
        }

        public static error SetsockoptInet4Addr(Handle fd, long level, long opt, array<byte> value)
        {
            value = value.Clone();

            return error.As(Setsockopt(fd, int32(level), int32(opt), (byte.Value)(@unsafe.Pointer(ref value[0L])), 4L));
        }
        public static error SetsockoptIPMreq(Handle fd, long level, long opt, ref IPMreq mreq)
        {
            return error.As(Setsockopt(fd, int32(level), int32(opt), (byte.Value)(@unsafe.Pointer(mreq)), int32(@unsafe.Sizeof(mreq.Value))));
        }
        public static error SetsockoptIPv6Mreq(Handle fd, long level, long opt, ref IPv6Mreq mreq)
        {
            return error.As(EWINDOWS);
        }

        public static long Getpid()
        {
            return int(getCurrentProcessId());
        }

        public static (Handle, error) FindFirstFile(ref ushort name, ref Win32finddata data)
        { 
            // NOTE(rsc): The Win32finddata struct is wrong for the system call:
            // the two paths are each one uint16 short. Use the correct struct,
            // a win32finddata1, and then copy the results out.
            // There is no loss of expressivity here, because the final
            // uint16, if it is used, is supposed to be a NUL, and Go doesn't need that.
            // For Go 1.1, we might avoid the allocation of win32finddata1 here
            // by adding a final Bug [2]uint16 field to the struct and then
            // adjusting the fields in the result directly.
            win32finddata1 data1 = default;
            handle, err = findFirstFile1(name, ref data1);
            if (err == null)
            {
                copyFindData(data, ref data1);
            }
            return;
        }

        public static error FindNextFile(Handle handle, ref Win32finddata data)
        {
            win32finddata1 data1 = default;
            err = findNextFile1(handle, ref data1);
            if (err == null)
            {
                copyFindData(data, ref data1);
            }
            return;
        }

        private static (ref ProcessEntry32, error) getProcessEntry(long pid) => func((defer, _, __) =>
        {
            var (snapshot, err) = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0L);
            if (err != null)
            {
                return (null, err);
            }
            defer(CloseHandle(snapshot));
            ProcessEntry32 procEntry = default;
            procEntry.Size = uint32(@unsafe.Sizeof(procEntry));
            err = Process32First(snapshot, ref procEntry);

            if (err != null)
            {
                return (null, err);
            }
            while (true)
            {
                if (procEntry.ProcessID == uint32(pid))
                {
                    return (ref procEntry, null);
                }
                err = Process32Next(snapshot, ref procEntry);
                if (err != null)
                {
                    return (null, err);
                }
            }

        });

        public static long Getppid()
        {
            var (pe, err) = getProcessEntry(Getpid());
            if (err != null)
            {
                return -1L;
            }
            return int(pe.ParentProcessID);
        }

        // TODO(brainman): fix all needed for os
        public static error Fchdir(Handle fd)
        {
            return error.As(EWINDOWS);
        }
        public static error Link(@string oldpath, @string newpath)
        {
            return error.As(EWINDOWS);
        }
        public static error Symlink(@string path, @string link)
        {
            return error.As(EWINDOWS);
        }

        public static error Fchmod(Handle fd, uint mode)
        {
            return error.As(EWINDOWS);
        }
        public static error Chown(@string path, long uid, long gid)
        {
            return error.As(EWINDOWS);
        }
        public static error Lchown(@string path, long uid, long gid)
        {
            return error.As(EWINDOWS);
        }
        public static error Fchown(Handle fd, long uid, long gid)
        {
            return error.As(EWINDOWS);
        }

        public static long Getuid()
        {
            return -1L;
        }
        public static long Geteuid()
        {
            return -1L;
        }
        public static long Getgid()
        {
            return -1L;
        }
        public static long Getegid()
        {
            return -1L;
        }
        public static (slice<long>, error) Getgroups()
        {
            return (null, EWINDOWS);
        }

        public partial struct Signal // : long
        {
        }

        public static void Signal(this Signal s)
        {
        }

        public static @string String(this Signal s)
        {
            if (0L <= s && int(s) < len(signals))
            {
                var str = signals[s];
                if (str != "")
                {
                    return str;
                }
            }
            return "signal " + itoa(int(s));
        }

        public static error LoadCreateSymbolicLink()
        {
            return error.As(procCreateSymbolicLinkW.Find());
        }

        // Readlink returns the destination of the named symbolic link.
        public static (long, error) Readlink(@string path, slice<byte> buf) => func((defer, _, __) =>
        {
            var (fd, err) = CreateFile(StringToUTF16Ptr(path), GENERIC_READ, 0L, null, OPEN_EXISTING, FILE_FLAG_OPEN_REPARSE_POINT | FILE_FLAG_BACKUP_SEMANTICS, 0L);
            if (err != null)
            {
                return (-1L, err);
            }
            defer(CloseHandle(fd));

            var rdbbuf = make_slice<byte>(MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
            uint bytesReturned = default;
            err = DeviceIoControl(fd, FSCTL_GET_REPARSE_POINT, null, 0L, ref rdbbuf[0L], uint32(len(rdbbuf)), ref bytesReturned, null);
            if (err != null)
            {
                return (-1L, err);
            }
            var rdb = (reparseDataBuffer.Value)(@unsafe.Pointer(ref rdbbuf[0L]));
            @string s = default;

            if (rdb.ReparseTag == IO_REPARSE_TAG_SYMLINK) 
                var data = (symbolicLinkReparseBuffer.Value)(@unsafe.Pointer(ref rdb.reparseBuffer));
                ref array<ushort> p = new ptr<ref array<ushort>>(@unsafe.Pointer(ref data.PathBuffer[0L]));
                s = UTF16ToString(p[data.SubstituteNameOffset / 2L..(data.SubstituteNameOffset + data.SubstituteNameLength) / 2L]);
                if (data.Flags & _SYMLINK_FLAG_RELATIVE == 0L)
                {
                    if (len(s) >= 4L && s[..4L] == "\\??\\")
                    {
                        s = s[4L..];

                        if (len(s) >= 2L && s[1L] == ':')                         else if (len(s) >= 4L && s[..4L] == "UNC\\") // \??\UNC\foo\bar
                            s = "\\\\" + s[4L..];
                        else                         
                    }
                    else
                    { 
                        // unexpected; do nothing
                    }
                }
            else if (rdb.ReparseTag == _IO_REPARSE_TAG_MOUNT_POINT) 
                data = (mountPointReparseBuffer.Value)(@unsafe.Pointer(ref rdb.reparseBuffer));
                p = new ptr<ref array<ushort>>(@unsafe.Pointer(ref data.PathBuffer[0L]));
                s = UTF16ToString(p[data.SubstituteNameOffset / 2L..(data.SubstituteNameOffset + data.SubstituteNameLength) / 2L]);
                if (len(s) >= 4L && s[..4L] == "\\??\\")
                { // \??\C:\foo\bar
                    s = s[4L..];
                }
                else
                { 
                    // unexpected; do nothing
                }
            else 
                // the path is not a symlink or junction but another type of reparse
                // point
                return (-1L, ENOENT);
                        n = copy(buf, (slice<byte>)s);

            return (n, null);
        });
    }
}
