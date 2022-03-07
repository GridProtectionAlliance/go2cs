// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:39 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_windows.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

    // TODO(brainman): should not need those
private static readonly nint _NSIG = 65;


//go:cgo_import_dynamic runtime._AddVectoredExceptionHandler AddVectoredExceptionHandler%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._CloseHandle CloseHandle%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateEventA CreateEventA%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateFileA CreateFileA%7 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateIoCompletionPort CreateIoCompletionPort%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateThread CreateThread%6 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateWaitableTimerA CreateWaitableTimerA%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._CreateWaitableTimerExW CreateWaitableTimerExW%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._DuplicateHandle DuplicateHandle%7 "kernel32.dll"
//go:cgo_import_dynamic runtime._ExitProcess ExitProcess%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._FreeEnvironmentStringsW FreeEnvironmentStringsW%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetConsoleMode GetConsoleMode%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetEnvironmentStringsW GetEnvironmentStringsW%0 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetProcAddress GetProcAddress%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetProcessAffinityMask GetProcessAffinityMask%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetQueuedCompletionStatusEx GetQueuedCompletionStatusEx%6 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetStdHandle GetStdHandle%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetSystemDirectoryA GetSystemDirectoryA%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetSystemInfo GetSystemInfo%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._GetThreadContext GetThreadContext%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetThreadContext SetThreadContext%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._LoadLibraryW LoadLibraryW%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._LoadLibraryA LoadLibraryA%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._PostQueuedCompletionStatus PostQueuedCompletionStatus%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._ResumeThread ResumeThread%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetConsoleCtrlHandler SetConsoleCtrlHandler%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetErrorMode SetErrorMode%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetEvent SetEvent%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetProcessPriorityBoost SetProcessPriorityBoost%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetThreadPriority SetThreadPriority%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetUnhandledExceptionFilter SetUnhandledExceptionFilter%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SetWaitableTimer SetWaitableTimer%6 "kernel32.dll"
//go:cgo_import_dynamic runtime._Sleep Sleep%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SuspendThread SuspendThread%1 "kernel32.dll"
//go:cgo_import_dynamic runtime._SwitchToThread SwitchToThread%0 "kernel32.dll"
//go:cgo_import_dynamic runtime._TlsAlloc TlsAlloc%0 "kernel32.dll"
//go:cgo_import_dynamic runtime._VirtualAlloc VirtualAlloc%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._VirtualFree VirtualFree%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._VirtualQuery VirtualQuery%3 "kernel32.dll"
//go:cgo_import_dynamic runtime._WaitForSingleObject WaitForSingleObject%2 "kernel32.dll"
//go:cgo_import_dynamic runtime._WaitForMultipleObjects WaitForMultipleObjects%4 "kernel32.dll"
//go:cgo_import_dynamic runtime._WriteConsoleW WriteConsoleW%5 "kernel32.dll"
//go:cgo_import_dynamic runtime._WriteFile WriteFile%5 "kernel32.dll"

private partial struct stdFunction { // : unsafe.Pointer
}

 
// Following syscalls are available on every Windows PC.
// All these variables are set by the Windows executable
// loader before the Go program starts.
private static stdFunction _AddVectoredExceptionHandler = default;private static stdFunction _CloseHandle = default;private static stdFunction _CreateEventA = default;private static stdFunction _CreateFileA = default;private static stdFunction _CreateIoCompletionPort = default;private static stdFunction _CreateThread = default;private static stdFunction _CreateWaitableTimerA = default;private static stdFunction _CreateWaitableTimerExW = default;private static stdFunction _DuplicateHandle = default;private static stdFunction _ExitProcess = default;private static stdFunction _FreeEnvironmentStringsW = default;private static stdFunction _GetConsoleMode = default;private static stdFunction _GetEnvironmentStringsW = default;private static stdFunction _GetProcAddress = default;private static stdFunction _GetProcessAffinityMask = default;private static stdFunction _GetQueuedCompletionStatusEx = default;private static stdFunction _GetStdHandle = default;private static stdFunction _GetSystemDirectoryA = default;private static stdFunction _GetSystemInfo = default;private static stdFunction _GetSystemTimeAsFileTime = default;private static stdFunction _GetThreadContext = default;private static stdFunction _SetThreadContext = default;private static stdFunction _LoadLibraryW = default;private static stdFunction _LoadLibraryA = default;private static stdFunction _PostQueuedCompletionStatus = default;private static stdFunction _QueryPerformanceCounter = default;private static stdFunction _QueryPerformanceFrequency = default;private static stdFunction _ResumeThread = default;private static stdFunction _SetConsoleCtrlHandler = default;private static stdFunction _SetErrorMode = default;private static stdFunction _SetEvent = default;private static stdFunction _SetProcessPriorityBoost = default;private static stdFunction _SetThreadPriority = default;private static stdFunction _SetUnhandledExceptionFilter = default;private static stdFunction _SetWaitableTimer = default;private static stdFunction _Sleep = default;private static stdFunction _SuspendThread = default;private static stdFunction _SwitchToThread = default;private static stdFunction _TlsAlloc = default;private static stdFunction _VirtualAlloc = default;private static stdFunction _VirtualFree = default;private static stdFunction _VirtualQuery = default;private static stdFunction _WaitForSingleObject = default;private static stdFunction _WaitForMultipleObjects = default;private static stdFunction _WriteConsoleW = default;private static stdFunction _WriteFile = default;private static stdFunction _ = default; 

// Following syscalls are only available on some Windows PCs.
// We will load syscalls, if available, before using them.
private static stdFunction _AddDllDirectory = default;private static stdFunction _AddVectoredContinueHandler = default;private static stdFunction _LoadLibraryExA = default;private static stdFunction _LoadLibraryExW = default;private static stdFunction _ = default; 

// Use RtlGenRandom to generate cryptographically random data.
// This approach has been recommended by Microsoft (see issue
// 15589 for details).
// The RtlGenRandom is not listed in advapi32.dll, instead
// RtlGenRandom function can be found by searching for SystemFunction036.
// Also some versions of Mingw cannot link to SystemFunction036
// when building executable as Cgo. So load SystemFunction036
// manually during runtime startup.
private static stdFunction _RtlGenRandom = default;private static stdFunction _NtWaitForSingleObject = default;private static stdFunction _RtlGetCurrentPeb = default;private static stdFunction _RtlGetNtVersionNumbers = default;private static stdFunction _timeBeginPeriod = default;private static stdFunction _timeEndPeriod = default;private static stdFunction _WSAGetOverlappedResult = default;private static stdFunction _ = default;


// Function to be called by windows CreateThread
// to start new os thread.
private static void tstart_stdcall(ptr<m> newm);

// Init-time helper
private static void wintls();

private partial struct mOS {
    public mutex threadLock; // protects "thread" and prevents closing
    public System.UIntPtr thread; // thread handle

    public System.UIntPtr waitsema; // semaphore for parking on locks
    public System.UIntPtr resumesema; // semaphore to indicate suspend/resume

    public System.UIntPtr highResTimer; // high resolution timer handle used in usleep

// preemptExtLock synchronizes preemptM with entry/exit from
// external C code.
//
// This protects against races between preemptM calling
// SuspendThread and external code on this thread calling
// ExitProcess. If these happen concurrently, it's possible to
// exit the suspending thread and suspend the exiting thread,
// leading to deadlock.
//
// 0 indicates this M is not being preempted or in external
// code. Entering external code CASes this from 0 to 1. If
// this fails, a preemption is in progress, so the thread must
// wait for the preemption. preemptM also CASes this from 0 to
// 1. If this fails, the preemption fails (as it would if the
// PC weren't in Go code). The value is reset to 0 when
// returning from external code or after a preemption is
// complete.
//
// TODO(austin): We may not need this if preemption were more
// tightly synchronized on the G/P status and preemption
// blocked transition into _Gsyscall/_Psyscall.
    public uint preemptExtLock;
}

//go:linkname os_sigpipe os.sigpipe
private static void os_sigpipe() {
    throw("too many writes on closed pipe");
}

// Stubs so tests can link correctly. These should never be called.
private static int open(ptr<byte> _addr_name, int mode, int perm) {
    ref byte name = ref _addr_name.val;

    throw("unimplemented");
    return -1;
}
private static int closefd(int fd) {
    throw("unimplemented");
    return -1;
}
private static int read(int fd, unsafe.Pointer p, int n) {
    throw("unimplemented");
    return -1;
}

private partial struct sigset {
}

// Call a Windows function with stdcall conventions,
// and switch to os stack during the call.
private static void asmstdcall(unsafe.Pointer fn);

private static unsafe.Pointer asmstdcallAddr = default;

private static stdFunction windowsFindfunc(System.UIntPtr lib, slice<byte> name) {
    if (name[len(name) - 1] != 0) {>>MARKER:FUNCTION_asmstdcall_BLOCK_PREFIX<<
        throw("usage");
    }
    var f = stdcall2(_GetProcAddress, lib, uintptr(@unsafe.Pointer(_addr_name[0])));
    return stdFunction(@unsafe.Pointer(f));

}

private static readonly nint _MAX_PATH = 260; // https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation
 // https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation
private static array<byte> sysDirectory = new array<byte>(_MAX_PATH + 1);
private static System.UIntPtr sysDirectoryLen = default;

private static System.UIntPtr windowsLoadSystemLib(slice<byte> name) {
    if (sysDirectoryLen == 0) {>>MARKER:FUNCTION_wintls_BLOCK_PREFIX<<
        var l = stdcall2(_GetSystemDirectoryA, uintptr(@unsafe.Pointer(_addr_sysDirectory[0])), uintptr(len(sysDirectory) - 1));
        if (l == 0 || l > uintptr(len(sysDirectory) - 1)) {>>MARKER:FUNCTION_tstart_stdcall_BLOCK_PREFIX<<
            throw("Unable to determine system directory");
        }
        sysDirectory[l] = '\\';
        sysDirectoryLen = l + 1;

    }
    if (useLoadLibraryEx) {
        return stdcall3(_LoadLibraryExA, uintptr(@unsafe.Pointer(_addr_name[0])), 0, _LOAD_LIBRARY_SEARCH_SYSTEM32);
    }
    else
 {
        var absName = append(sysDirectory[..(int)sysDirectoryLen], name);
        return stdcall1(_LoadLibraryA, uintptr(@unsafe.Pointer(_addr_absName[0])));
    }
}

private static readonly var haveCputicksAsm = GOARCH == "386" || GOARCH == "amd64";



private static void loadOptionalSyscalls() {
    slice<byte> kernel32dll = (slice<byte>)"kernel32.dll ";
    var k32 = stdcall1(_LoadLibraryA, uintptr(@unsafe.Pointer(_addr_kernel32dll[0])));
    if (k32 == 0) {
        throw("kernel32.dll not found");
    }
    _AddDllDirectory = windowsFindfunc(k32, (slice<byte>)"AddDllDirectory ");
    _AddVectoredContinueHandler = windowsFindfunc(k32, (slice<byte>)"AddVectoredContinueHandler ");
    _LoadLibraryExA = windowsFindfunc(k32, (slice<byte>)"LoadLibraryExA ");
    _LoadLibraryExW = windowsFindfunc(k32, (slice<byte>)"LoadLibraryExW ");
    useLoadLibraryEx = (_LoadLibraryExW != null && _LoadLibraryExA != null && _AddDllDirectory != null);

    slice<byte> advapi32dll = (slice<byte>)"advapi32.dll ";
    var a32 = windowsLoadSystemLib(advapi32dll);
    if (a32 == 0) {
        throw("advapi32.dll not found");
    }
    _RtlGenRandom = windowsFindfunc(a32, (slice<byte>)"SystemFunction036 ");

    slice<byte> ntdll = (slice<byte>)"ntdll.dll ";
    var n32 = windowsLoadSystemLib(ntdll);
    if (n32 == 0) {
        throw("ntdll.dll not found");
    }
    _NtWaitForSingleObject = windowsFindfunc(n32, (slice<byte>)"NtWaitForSingleObject ");
    _RtlGetCurrentPeb = windowsFindfunc(n32, (slice<byte>)"RtlGetCurrentPeb ");
    _RtlGetNtVersionNumbers = windowsFindfunc(n32, (slice<byte>)"RtlGetNtVersionNumbers ");

    if (!haveCputicksAsm) {
        _QueryPerformanceCounter = windowsFindfunc(k32, (slice<byte>)"QueryPerformanceCounter ");
        if (_QueryPerformanceCounter == null) {
            throw("could not find QPC syscalls");
        }
    }
    slice<byte> winmmdll = (slice<byte>)"winmm.dll ";
    var m32 = windowsLoadSystemLib(winmmdll);
    if (m32 == 0) {
        throw("winmm.dll not found");
    }
    _timeBeginPeriod = windowsFindfunc(m32, (slice<byte>)"timeBeginPeriod ");
    _timeEndPeriod = windowsFindfunc(m32, (slice<byte>)"timeEndPeriod ");
    if (_timeBeginPeriod == null || _timeEndPeriod == null) {
        throw("timeBegin/EndPeriod not found");
    }
    slice<byte> ws232dll = (slice<byte>)"ws2_32.dll ";
    var ws232 = windowsLoadSystemLib(ws232dll);
    if (ws232 == 0) {
        throw("ws2_32.dll not found");
    }
    _WSAGetOverlappedResult = windowsFindfunc(ws232, (slice<byte>)"WSAGetOverlappedResult ");
    if (_WSAGetOverlappedResult == null) {
        throw("WSAGetOverlappedResult not found");
    }
    if (windowsFindfunc(n32, (slice<byte>)"wine_get_version ") != null) { 
        // running on Wine
        initWine(k32);

    }
}

private static void monitorSuspendResume() {
    const nint _DEVICE_NOTIFY_CALLBACK = 2;
    private partial struct _DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS {
        public System.UIntPtr callback;
        public System.UIntPtr context;
    }

    var powrprof = windowsLoadSystemLib((slice<byte>)"powrprof.dll ");
    if (powrprof == 0) {
        return ; // Running on Windows 7, where we don't need it anyway.
    }
    var powerRegisterSuspendResumeNotification = windowsFindfunc(powrprof, (slice<byte>)"PowerRegisterSuspendResumeNotification ");
    if (powerRegisterSuspendResumeNotification == null) {
        return ; // Running on Windows 7, where we don't need it anyway.
    }
    ref Func<System.UIntPtr, uint, System.UIntPtr, System.UIntPtr> fn = ref heap((context, changeType, setting) => {
        {
            var mp = (m.val)(atomic.Loadp(@unsafe.Pointer(_addr_allm)));

            while (mp != null) {
                if (mp.resumesema != 0) {
                    stdcall1(_SetEvent, mp.resumesema);
                mp = mp.alllink;
                }

            }

        }
        return 0;

    }, out ptr<Func<System.UIntPtr, uint, System.UIntPtr, System.UIntPtr>> _addr_fn);
    ref _DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS @params = ref heap(new _DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS(callback:compileCallback(*efaceOf(&fn),true),), out ptr<_DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS> _addr_@params);
    ref var handle = ref heap(uintptr(0), out ptr<var> _addr_handle);
    stdcall3(powerRegisterSuspendResumeNotification, _DEVICE_NOTIFY_CALLBACK, uintptr(@unsafe.Pointer(_addr_params)), uintptr(@unsafe.Pointer(_addr_handle)));

}

//go:nosplit
private static System.UIntPtr getLoadLibrary() {
    return uintptr(@unsafe.Pointer(_LoadLibraryW));
}

//go:nosplit
private static System.UIntPtr getLoadLibraryEx() {
    return uintptr(@unsafe.Pointer(_LoadLibraryExW));
}

//go:nosplit
private static System.UIntPtr getGetProcAddress() {
    return uintptr(@unsafe.Pointer(_GetProcAddress));
}

private static int getproccount() {
    ref System.UIntPtr mask = ref heap(out ptr<System.UIntPtr> _addr_mask);    ref System.UIntPtr sysmask = ref heap(out ptr<System.UIntPtr> _addr_sysmask);

    var ret = stdcall3(_GetProcessAffinityMask, currentProcess, uintptr(@unsafe.Pointer(_addr_mask)), uintptr(@unsafe.Pointer(_addr_sysmask)));
    if (ret != 0) {
        nint n = 0;
        var maskbits = int(@unsafe.Sizeof(mask) * 8);
        for (nint i = 0; i < maskbits; i++) {
            if (mask & (1 << (int)(uint(i))) != 0) {
                n++;
            }
        }
        if (n != 0) {
            return int32(n);
        }
    }
    ref systeminfo info = ref heap(out ptr<systeminfo> _addr_info);
    stdcall1(_GetSystemInfo, uintptr(@unsafe.Pointer(_addr_info)));
    return int32(info.dwnumberofprocessors);

}

private static System.UIntPtr getPageSize() {
    ref systeminfo info = ref heap(out ptr<systeminfo> _addr_info);
    stdcall1(_GetSystemInfo, uintptr(@unsafe.Pointer(_addr_info)));
    return uintptr(info.dwpagesize);
}

private static readonly var currentProcess = ~uintptr(0); // -1 = current process
private static readonly var currentThread = ~uintptr(1); // -2 = current thread

// in sys_windows_386.s and sys_windows_amd64.s:
private static uint getlasterror();

// When loading DLLs, we prefer to use LoadLibraryEx with
// LOAD_LIBRARY_SEARCH_* flags, if available. LoadLibraryEx is not
// available on old Windows, though, and the LOAD_LIBRARY_SEARCH_*
// flags are not available on some versions of Windows without a
// security patch.
//
// https://msdn.microsoft.com/en-us/library/ms684179(v=vs.85).aspx says:
// "Windows 7, Windows Server 2008 R2, Windows Vista, and Windows
// Server 2008: The LOAD_LIBRARY_SEARCH_* flags are available on
// systems that have KB2533623 installed. To determine whether the
// flags are available, use GetProcAddress to get the address of the
// AddDllDirectory, RemoveDllDirectory, or SetDefaultDllDirectories
// function. If GetProcAddress succeeds, the LOAD_LIBRARY_SEARCH_*
// flags can be used with LoadLibraryEx."
private static bool useLoadLibraryEx = default;

private static uint timeBeginPeriodRetValue = default;

// osRelaxMinNS indicates that sysmon shouldn't osRelax if the next
// timer is less than 60 ms from now. Since osRelaxing may reduce
// timer resolution to 15.6 ms, this keeps timer error under roughly 1
// part in 4.
private static readonly nint osRelaxMinNS = 60 * 1e6F;

// osRelax is called by the scheduler when transitioning to and from
// all Ps being idle.
//
// Some versions of Windows have high resolution timer. For those
// versions osRelax is noop.
// For Windows versions without high resolution timer, osRelax
// adjusts the system-wide timer resolution. Go needs a
// high resolution timer while running and there's little extra cost
// if we're already using the CPU, but if all Ps are idle there's no
// need to consume extra power to drive the high-res timer.


// osRelax is called by the scheduler when transitioning to and from
// all Ps being idle.
//
// Some versions of Windows have high resolution timer. For those
// versions osRelax is noop.
// For Windows versions without high resolution timer, osRelax
// adjusts the system-wide timer resolution. Go needs a
// high resolution timer while running and there's little extra cost
// if we're already using the CPU, but if all Ps are idle there's no
// need to consume extra power to drive the high-res timer.
private static uint osRelax(bool relax) {
    if (haveHighResTimer) {>>MARKER:FUNCTION_getlasterror_BLOCK_PREFIX<< 
        // If the high resolution timer is available, the runtime uses the timer
        // to sleep for short durations. This means there's no need to adjust
        // the global clock frequency.
        return 0;

    }
    if (relax) {
        return uint32(stdcall1(_timeEndPeriod, 1));
    }
    else
 {
        return uint32(stdcall1(_timeBeginPeriod, 1));
    }
}

// haveHighResTimer indicates that the CreateWaitableTimerEx
// CREATE_WAITABLE_TIMER_HIGH_RESOLUTION flag is available.
private static var haveHighResTimer = false;

// createHighResTimer calls CreateWaitableTimerEx with
// CREATE_WAITABLE_TIMER_HIGH_RESOLUTION flag to create high
// resolution timer. createHighResTimer returns new timer
// handle or 0, if CreateWaitableTimerEx failed.
private static System.UIntPtr createHighResTimer() {
 
    // As per @jstarks, see
    // https://github.com/golang/go/issues/8687#issuecomment-656259353
    const nuint _CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x00000002;

    const nuint _SYNCHRONIZE = 0x00100000;
    const nuint _TIMER_QUERY_STATE = 0x0001;
    const nuint _TIMER_MODIFY_STATE = 0x0002;

    return stdcall4(_CreateWaitableTimerExW, 0, 0, _CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, _SYNCHRONIZE | _TIMER_QUERY_STATE | _TIMER_MODIFY_STATE);

}

private static readonly var highResTimerSupported = GOARCH == "386" || GOARCH == "amd64";



private static void initHighResTimer() {
    if (!highResTimerSupported) { 
        // TODO: Not yet implemented.
        return ;

    }
    var h = createHighResTimer();
    if (h != 0) {
        haveHighResTimer = true;
        stdcall1(_CloseHandle, h);
    }
}

//go:linkname canUseLongPaths os.canUseLongPaths
private static bool canUseLongPaths = default;

// We want this to be large enough to hold the contents of sysDirectory, *plus*
// a slash and another component that itself is greater than MAX_PATH.
private static array<byte> longFileName = new array<byte>((_MAX_PATH + 1) * 2 + 1);

// initLongPathSupport initializes the canUseLongPaths variable, which is
// linked into os.canUseLongPaths for determining whether or not long paths
// need to be fixed up. In the best case, this function is running on newer
// Windows 10 builds, which have a bit field member of the PEB called
// "IsLongPathAwareProcess." When this is set, we don't need to go through the
// error-prone fixup function in order to access long paths. So this init
// function first checks the Windows build number, sets the flag, and then
// tests to see if it's actually working. If everything checks out, then
// canUseLongPaths is set to true, and later when called, os.fixLongPath
// returns early without doing work.
private static void initLongPathSupport() {
    const nuint IsLongPathAwareProcess = 0x80;
    const nint PebBitFieldOffset = 3;
    const nint OPEN_EXISTING = 3;
    const nint ERROR_PATH_NOT_FOUND = 3; 

    // Check that we're ≥ 10.0.15063.
    ref uint maj = ref heap(out ptr<uint> _addr_maj);    ref uint min = ref heap(out ptr<uint> _addr_min);    ref uint build = ref heap(out ptr<uint> _addr_build);

    stdcall3(_RtlGetNtVersionNumbers, uintptr(@unsafe.Pointer(_addr_maj)), uintptr(@unsafe.Pointer(_addr_min)), uintptr(@unsafe.Pointer(_addr_build)));
    if (maj < 10 || (maj == 10 && min == 0 && build & 0xffff < 15063)) {
        return ;
    }
    var bitField = (byte.val)(@unsafe.Pointer(stdcall0(_RtlGetCurrentPeb) + PebBitFieldOffset));
    var originalBitField = bitField.val;
    bitField.val |= IsLongPathAwareProcess; 

    // Check that this actually has an effect, by constructing a large file
    // path and seeing whether we get ERROR_PATH_NOT_FOUND, rather than
    // some other error, which would indicate the path is too long, and
    // hence long path support is not successful. This whole section is NOT
    // strictly necessary, but is a nice validity check for the near to
    // medium term, when this functionality is still relatively new in
    // Windows.
    getRandomData(longFileName[(int)len(longFileName) - 33..(int)len(longFileName) - 1]);
    var start = copy(longFileName[..], sysDirectory[..(int)sysDirectoryLen]);
    const @string dig = "0123456789abcdef";

    {
        nint i__prev1 = i;

        for (nint i = 0; i < 32; i++) {
            longFileName[start + i * 2] = dig[longFileName[len(longFileName) - 33 + i] >> 4];
            longFileName[start + i * 2 + 1] = dig[longFileName[len(longFileName) - 33 + i] & 0xf];
        }

        i = i__prev1;
    }
    start += 64;
    {
        nint i__prev1 = i;

        for (i = start; i < len(longFileName) - 1; i++) {
            longFileName[i] = 'A';
        }

        i = i__prev1;
    }
    stdcall7(_CreateFileA, uintptr(@unsafe.Pointer(_addr_longFileName[0])), 0, 0, 0, OPEN_EXISTING, 0, 0); 
    // The ERROR_PATH_NOT_FOUND error value is distinct from
    // ERROR_FILE_NOT_FOUND or ERROR_INVALID_NAME, the latter of which we
    // expect here due to the final component being too long.
    if (getlasterror() == ERROR_PATH_NOT_FOUND) {
        bitField.val = originalBitField;
        println("runtime: warning: IsLongPathAwareProcess failed to enable long paths; proceeding in fixup mode");
        return ;
    }
    canUseLongPaths = true;

}

private static void osinit() {
    asmstdcallAddr = @unsafe.Pointer(funcPC(asmstdcall));

    setBadSignalMsg();

    loadOptionalSyscalls();

    disableWER();

    initExceptionHandler();

    initHighResTimer();
    timeBeginPeriodRetValue = osRelax(false);

    initLongPathSupport();

    ncpu = getproccount();

    physPageSize = getPageSize(); 

    // Windows dynamic priority boosting assumes that a process has different types
    // of dedicated threads -- GUI, IO, computational, etc. Go processes use
    // equivalent threads that all do a mix of GUI, IO, computations, etc.
    // In such context dynamic priority boosting does nothing but harm, so we turn it off.
    stdcall2(_SetProcessPriorityBoost, currentProcess, 1);

}

// useQPCTime controls whether time.now and nanotime use QueryPerformanceCounter.
// This is only set to 1 when running under Wine.
private static byte useQPCTime = default;

private static long qpcStartCounter = default;
private static long qpcMultiplier = default;

//go:nosplit
private static long nanotimeQPC() {
    ref long counter = ref heap(0, out ptr<long> _addr_counter);
    stdcall1(_QueryPerformanceCounter, uintptr(@unsafe.Pointer(_addr_counter))); 

    // returns number of nanoseconds
    return (counter - qpcStartCounter) * qpcMultiplier;

}

//go:nosplit
private static (long, int, long) nowQPC() {
    long sec = default;
    int nsec = default;
    long mono = default;

    ref long ft = ref heap(out ptr<long> _addr_ft);
    stdcall1(_GetSystemTimeAsFileTime, uintptr(@unsafe.Pointer(_addr_ft)));

    var t = (ft - (nint)116444736000000000L) * 100;

    sec = t / 1000000000;
    nsec = int32(t - sec * 1000000000);

    mono = nanotimeQPC();
    return ;
}

private static void initWine(System.UIntPtr k32) {
    _GetSystemTimeAsFileTime = windowsFindfunc(k32, (slice<byte>)"GetSystemTimeAsFileTime ");
    if (_GetSystemTimeAsFileTime == null) {
        throw("could not find GetSystemTimeAsFileTime() syscall");
    }
    _QueryPerformanceCounter = windowsFindfunc(k32, (slice<byte>)"QueryPerformanceCounter ");
    _QueryPerformanceFrequency = windowsFindfunc(k32, (slice<byte>)"QueryPerformanceFrequency ");
    if (_QueryPerformanceCounter == null || _QueryPerformanceFrequency == null) {
        throw("could not find QPC syscalls");
    }
    ref long tmp = ref heap(out ptr<long> _addr_tmp);
    stdcall1(_QueryPerformanceFrequency, uintptr(@unsafe.Pointer(_addr_tmp)));
    if (tmp == 0) {
        throw("QueryPerformanceFrequency syscall returned zero, running on unsupported hardware");
    }
    if (tmp > (1 << 31 - 1)) {
        throw("QueryPerformanceFrequency overflow 32 bit divider, check nosplit discussion to proceed");
    }
    var qpcFrequency = int32(tmp);
    stdcall1(_QueryPerformanceCounter, uintptr(@unsafe.Pointer(_addr_qpcStartCounter))); 

    // Since we are supposed to run this time calls only on Wine, it does not lose precision,
    // since Wine's timer is kind of emulated at 10 Mhz, so it will be a nice round multiplier of 100
    // but for general purpose system (like 3.3 Mhz timer on i7) it will not be very precise.
    // We have to do it this way (or similar), since multiplying QPC counter by 100 millions overflows
    // int64 and resulted time will always be invalid.
    qpcMultiplier = int64(timediv(1000000000, qpcFrequency, null));

    useQPCTime = 1;

}

//go:nosplit
private static void getRandomData(slice<byte> r) {
    nint n = 0;
    if (stdcall2(_RtlGenRandom, uintptr(@unsafe.Pointer(_addr_r[0])), uintptr(len(r))) & 0xff != 0) {
        n = len(r);
    }
    extendRandom(r, n);

}

private static void goenvs() { 
    // strings is a pointer to environment variable pairs in the form:
    //     "envA=valA\x00envB=valB\x00\x00" (in UTF-16)
    // Two consecutive zero bytes end the list.
    var strings = @unsafe.Pointer(stdcall0(_GetEnvironmentStringsW));
    ptr<array<ushort>> p = new ptr<ptr<array<ushort>>>(strings)[..];

    nint n = 0;
    {
        nint i__prev1 = i;

        for (nint from = 0;
        nint i = 0; true; i++) {
            if (p[i] == 0) { 
                // empty string marks the end
                if (i == from) {
                    break;
                }

                from = i + 1;
                n++;

            }

        }

        i = i__prev1;
    }
    envs = make_slice<@string>(n);

    {
        nint i__prev1 = i;

        foreach (var (__i) in envs) {
            i = __i;
            envs[i] = gostringw(_addr_p[0]);
            while (p[0] != 0) {
                p = p[(int)1..];
            }

            p = p[(int)1..]; // skip nil byte
        }
        i = i__prev1;
    }

    stdcall1(_FreeEnvironmentStringsW, uintptr(strings)); 

    // We call these all the way here, late in init, so that malloc works
    // for the callback functions these generate.
    ref var fn = ref heap(ctrlHandler, out ptr<var> _addr_fn);
    var ctrlHandlerPC = compileCallback(new ptr<ptr<efaceOf>>(_addr_fn), true);
    stdcall2(_SetConsoleCtrlHandler, ctrlHandlerPC, 1);

    monitorSuspendResume();

}

// exiting is set to non-zero when the process is exiting.
private static uint exiting = default;

//go:nosplit
private static void exit(int code) { 
    // Disallow thread suspension for preemption. Otherwise,
    // ExitProcess and SuspendThread can race: SuspendThread
    // queues a suspension request for this thread, ExitProcess
    // kills the suspending thread, and then this thread suspends.
    lock(_addr_suspendLock);
    atomic.Store(_addr_exiting, 1);
    stdcall1(_ExitProcess, uintptr(code));

}

// write1 must be nosplit because it's used as a last resort in
// functions like badmorestackg0. In such cases, we'll always take the
// ASCII path.
//
//go:nosplit
private static int write1(System.UIntPtr fd, unsafe.Pointer buf, int n) {
    const var _STD_OUTPUT_HANDLE = ~uintptr(10); // -11
    const var _STD_ERROR_HANDLE = ~uintptr(11); // -12
    System.UIntPtr handle = default;
    switch (fd) {
        case 1: 
            handle = stdcall1(_GetStdHandle, _STD_OUTPUT_HANDLE);
            break;
        case 2: 
            handle = stdcall1(_GetStdHandle, _STD_ERROR_HANDLE);
            break;
        default: 
            // assume fd is real windows handle.
            handle = fd;
            break;
    }
    var isASCII = true;
    ptr<array<byte>> b = new ptr<ptr<array<byte>>>(buf)[..(int)n];
    foreach (var (_, x) in b) {
        if (x >= 0x80) {
            isASCII = false;
            break;
        }
    }    if (!isASCII) {
        ref uint m = ref heap(out ptr<uint> _addr_m);
        var isConsole = stdcall2(_GetConsoleMode, handle, uintptr(@unsafe.Pointer(_addr_m))) != 0; 
        // If this is a console output, various non-unicode code pages can be in use.
        // Use the dedicated WriteConsole call to ensure unicode is printed correctly.
        if (isConsole) {
            return int32(writeConsole(handle, buf, n));
        }
    }
    ref uint written = ref heap(out ptr<uint> _addr_written);
    stdcall5(_WriteFile, handle, uintptr(buf), uintptr(n), uintptr(@unsafe.Pointer(_addr_written)), 0);
    return int32(written);

}

private static array<ushort> utf16ConsoleBack = new array<ushort>(1000);private static mutex utf16ConsoleBackLock = default;

// writeConsole writes bufLen bytes from buf to the console File.
// It returns the number of bytes written.
private static nint writeConsole(System.UIntPtr handle, unsafe.Pointer buf, int bufLen) {
    const var surr2 = (surrogateMin + surrogateMax + 1) / 2; 

    // Do not use defer for unlock. May cause issues when printing a panic.
 

    // Do not use defer for unlock. May cause issues when printing a panic.
    lock(_addr_utf16ConsoleBackLock);

    ptr<array<byte>> b = new ptr<ptr<array<byte>>>(buf)[..(int)bufLen];
    ptr<ptr<@string>> s = new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(_addr_b));

    var utf16tmp = utf16ConsoleBack[..];

    var total = len(s);
    nint w = 0;
    foreach (var (_, r) in s) {
        if (w >= len(utf16tmp) - 2) {
            writeConsoleUTF16(handle, utf16tmp[..(int)w]);
            w = 0;
        }
        if (r < 0x10000) {
            utf16tmp[w] = uint16(r);
            w++;
        }
        else
 {
            r -= 0x10000;
            utf16tmp[w] = surrogateMin + uint16(r >> 10) & 0x3ff;
            utf16tmp[w + 1] = surr2 + uint16(r) & 0x3ff;
            w += 2;
        }
    }    writeConsoleUTF16(handle, utf16tmp[..(int)w]);
    unlock(_addr_utf16ConsoleBackLock);
    return total;

}

// writeConsoleUTF16 is the dedicated windows calls that correctly prints
// to the console regardless of the current code page. Input is utf-16 code points.
// The handle must be a console handle.
private static void writeConsoleUTF16(System.UIntPtr handle, slice<ushort> b) {
    var l = uint32(len(b));
    if (l == 0) {
        return ;
    }
    ref uint written = ref heap(out ptr<uint> _addr_written);
    stdcall5(_WriteConsoleW, handle, uintptr(@unsafe.Pointer(_addr_b[0])), uintptr(l), uintptr(@unsafe.Pointer(_addr_written)), 0);
    return ;

}

//go:nosplit
private static int semasleep(long ns) {
    const nuint _WAIT_ABANDONED = 0x00000080;
    const nuint _WAIT_OBJECT_0 = 0x00000000;
    const nuint _WAIT_TIMEOUT = 0x00000102;
    const nuint _WAIT_FAILED = 0xFFFFFFFF;

    System.UIntPtr result = default;
    if (ns < 0) {
        result = stdcall2(_WaitForSingleObject, getg().m.waitsema, uintptr(_INFINITE));
    }
    else
 {
        var start = nanotime();
        var elapsed = int64(0);
        while (true) {
            var ms = int64(timediv(ns - elapsed, 1000000, null));
            if (ms == 0) {
                ms = 1;
            }
            result = stdcall4(_WaitForMultipleObjects, 2, uintptr(@unsafe.Pointer(addr(new array<System.UIntPtr>(new System.UIntPtr[] { getg().m.waitsema, getg().m.resumesema })))), 0, uintptr(ms));
            if (result != _WAIT_OBJECT_0 + 1) { 
                // Not a suspend/resume event
                break;

            }

            elapsed = nanotime() - start;
            if (elapsed >= ns) {
                return -1;
            }

        }

    }

    if (result == _WAIT_OBJECT_0) // Signaled
        return 0;
    else if (result == _WAIT_TIMEOUT) 
        return -1;
    else if (result == _WAIT_ABANDONED) 
        systemstack(() => {
            throw("runtime.semasleep wait_abandoned");
        });
    else if (result == _WAIT_FAILED) 
        systemstack(() => {
            print("runtime: waitforsingleobject wait_failed; errno=", getlasterror(), "\n");
            throw("runtime.semasleep wait_failed");
        });
    else 
        systemstack(() => {
            print("runtime: waitforsingleobject unexpected; result=", result, "\n");
            throw("runtime.semasleep unexpected");
        });
        return -1; // unreachable
}

//go:nosplit
private static void semawakeup(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (stdcall1(_SetEvent, mp.waitsema) == 0) {
        systemstack(() => {
            print("runtime: setevent failed; errno=", getlasterror(), "\n");
            throw("runtime.semawakeup");
        });
    }
}

//go:nosplit
private static void semacreate(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (mp.waitsema != 0) {
        return ;
    }
    mp.waitsema = stdcall4(_CreateEventA, 0, 0, 0, 0);
    if (mp.waitsema == 0) {
        systemstack(() => {
            print("runtime: createevent failed; errno=", getlasterror(), "\n");
            throw("runtime.semacreate");
        });
    }
    mp.resumesema = stdcall4(_CreateEventA, 0, 0, 0, 0);
    if (mp.resumesema == 0) {
        systemstack(() => {
            print("runtime: createevent failed; errno=", getlasterror(), "\n");
            throw("runtime.semacreate");
        });
        stdcall1(_CloseHandle, mp.waitsema);
        mp.waitsema = 0;
    }
}

// May run with m.p==nil, so write barriers are not allowed. This
// function is called by newosproc0, so it is also required to
// operate without stack guards.
//go:nowritebarrierrec
//go:nosplit
private static void newosproc(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;
 
    // We pass 0 for the stack size to use the default for this binary.
    var thandle = stdcall6(_CreateThread, 0, 0, funcPC(tstart_stdcall), uintptr(@unsafe.Pointer(mp)), 0, 0);

    if (thandle == 0) {
        if (atomic.Load(_addr_exiting) != 0) { 
            // CreateThread may fail if called
            // concurrently with ExitProcess. If this
            // happens, just freeze this thread and let
            // the process exit. See issue #18253.
            lock(_addr_deadlock);
            lock(_addr_deadlock);

        }
        print("runtime: failed to create new OS thread (have ", mcount(), " already; errno=", getlasterror(), ")\n");
        throw("runtime.newosproc");

    }
    stdcall1(_CloseHandle, thandle);

}

// Used by the C library build mode. On Linux this function would allocate a
// stack, but that's not necessary for Windows. No stack guards are present
// and the GC has not been initialized, so write barriers will fail.
//go:nowritebarrierrec
//go:nosplit
private static void newosproc0(ptr<m> _addr_mp, unsafe.Pointer stk) {
    ref m mp = ref _addr_mp.val;
 
    // TODO: this is completely broken. The args passed to newosproc0 (in asm_amd64.s)
    // are stacksize and function, not *m and stack.
    // Check os_linux.go for an implementation that might actually work.
    throw("bad newosproc0");

}

private static void exitThread(ptr<uint> _addr_wait) {
    ref uint wait = ref _addr_wait.val;
 
    // We should never reach exitThread on Windows because we let
    // the OS clean up threads.
    throw("exitThread");

}

// Called to initialize a new m (including the bootstrap m).
// Called on the parent thread (main thread in case of bootstrap), can allocate memory.
private static void mpreinit(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

//go:nosplit
private static void sigsave(ptr<sigset> _addr_p) {
    ref sigset p = ref _addr_p.val;

}

//go:nosplit
private static void msigrestore(sigset sigmask) {
}

//go:nosplit
//go:nowritebarrierrec
private static void clearSignalHandlers() {
}

//go:nosplit
private static void sigblock(bool exiting) {
}

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate memory.
private static void minit() {
    ref System.UIntPtr thandle = ref heap(out ptr<System.UIntPtr> _addr_thandle);
    if (stdcall7(_DuplicateHandle, currentProcess, currentThread, currentProcess, uintptr(@unsafe.Pointer(_addr_thandle)), 0, 0, _DUPLICATE_SAME_ACCESS) == 0) {
        print("runtime.minit: duplicatehandle failed; errno=", getlasterror(), "\n");
        throw("runtime.minit: duplicatehandle failed");
    }
    var mp = getg().m;
    lock(_addr_mp.threadLock);
    mp.thread = thandle; 

    // Configure usleep timer, if possible.
    if (mp.highResTimer == 0 && haveHighResTimer) {
        mp.highResTimer = createHighResTimer();
        if (mp.highResTimer == 0) {
            print("runtime: CreateWaitableTimerEx failed; errno=", getlasterror(), "\n");
            throw("CreateWaitableTimerEx when creating timer failed");
        }
    }
    unlock(_addr_mp.threadLock); 

    // Query the true stack base from the OS. Currently we're
    // running on a small assumed stack.
    ref memoryBasicInformation mbi = ref heap(out ptr<memoryBasicInformation> _addr_mbi);
    var res = stdcall3(_VirtualQuery, uintptr(@unsafe.Pointer(_addr_mbi)), uintptr(@unsafe.Pointer(_addr_mbi)), @unsafe.Sizeof(mbi));
    if (res == 0) {
        print("runtime: VirtualQuery failed; errno=", getlasterror(), "\n");
        throw("VirtualQuery for stack base failed");
    }
    var @base = mbi.allocationBase + 16 << 10; 
    // Sanity check the stack bounds.
    var g0 = getg();
    if (base > g0.stack.hi || g0.stack.hi - base > 64 << 20) {
        print("runtime: g0 stack [", hex(base), ",", hex(g0.stack.hi), ")\n");
        throw("bad g0 stack");
    }
    g0.stack.lo = base;
    g0.stackguard0 = g0.stack.lo + _StackGuard;
    g0.stackguard1 = g0.stackguard0; 
    // Sanity check the SP.
    stackcheck();

}

// Called from dropm to undo the effect of an minit.
//go:nosplit
private static void unminit() {
    var mp = getg().m;
    lock(_addr_mp.threadLock);
    if (mp.thread != 0) {
        stdcall1(_CloseHandle, mp.thread);
        mp.thread = 0;
    }
    unlock(_addr_mp.threadLock);

}

// Called from exitm, but not from drop, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
//go:nosplit
private static void mdestroy(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (mp.highResTimer != 0) {
        stdcall1(_CloseHandle, mp.highResTimer);
        mp.highResTimer = 0;
    }
    if (mp.waitsema != 0) {
        stdcall1(_CloseHandle, mp.waitsema);
        mp.waitsema = 0;
    }
    if (mp.resumesema != 0) {
        stdcall1(_CloseHandle, mp.resumesema);
        mp.resumesema = 0;
    }
}

// Calling stdcall on os stack.
// May run during STW, so write barriers are not allowed.
//go:nowritebarrier
//go:nosplit
private static System.UIntPtr stdcall(stdFunction fn) {
    var gp = getg();
    var mp = gp.m;
    mp.libcall.fn = uintptr(@unsafe.Pointer(fn));
    var resetLibcall = false;
    if (mp.profilehz != 0 && mp.libcallsp == 0) { 
        // leave pc/sp for cpu profiler
        mp.libcallg.set(gp);
        mp.libcallpc = getcallerpc(); 
        // sp must be the last, because once async cpu profiler finds
        // all three values to be non-zero, it will use them
        mp.libcallsp = getcallersp();
        resetLibcall = true; // See comment in sys_darwin.go:libcCall
    }
    asmcgocall(asmstdcallAddr, @unsafe.Pointer(_addr_mp.libcall));
    if (resetLibcall) {
        mp.libcallsp = 0;
    }
    return mp.libcall.r1;

}

//go:nosplit
private static System.UIntPtr stdcall0(stdFunction fn) {
    var mp = getg().m;
    mp.libcall.n = 0;
    mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_fn))); // it's unused but must be non-nil, otherwise crashes
    return stdcall(fn);

}

//go:nosplit
//go:cgo_unsafe_args
private static System.UIntPtr stdcall1(stdFunction fn, System.UIntPtr a0) {
    var mp = getg().m;
    mp.libcall.n = 1;
    mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
private static System.UIntPtr stdcall2(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1) {
    var mp = getg().m;
    mp.libcall.n = 2;
    mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
private static System.UIntPtr stdcall3(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2) {
    var mp = getg().m;
    mp.libcall.n = 3;
    mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
private static System.UIntPtr stdcall4(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    var mp = getg().m;
    mp.libcall.n = 4;
    mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
private static System.UIntPtr stdcall5(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4) {
    var mp = getg().m;
    mp.libcall.n = 5;
    mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
private static System.UIntPtr stdcall6(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5) {
    var mp = getg().m;
    mp.libcall.n = 6;
    mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
    return stdcall(fn);
}

//go:nosplit
//go:cgo_unsafe_args
private static System.UIntPtr stdcall7(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    var mp = getg().m;
    mp.libcall.n = 7;
    mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
    return stdcall(fn);
}

// These must run on the system stack only.
private static void usleep2(int dt);
private static void usleep2HighRes(int dt);
private static void switchtothread();

//go:nosplit
private static void osyield_no_g() {
    switchtothread();
}

//go:nosplit
private static void osyield() {
    systemstack(switchtothread);
}

//go:nosplit
private static void usleep_no_g(uint us) {
    nint dt = -10 * int32(us); // relative sleep (negative), 100ns units
    usleep2(dt);

}

//go:nosplit
private static void usleep(uint us) {
    systemstack(() => {>>MARKER:FUNCTION_switchtothread_BLOCK_PREFIX<<
        nint dt = -10 * int32(us); // relative sleep (negative), 100ns units
        // If the high-res timer is available and its handle has been allocated for this m, use it.
        // Otherwise fall back to the low-res one, which doesn't need a handle.
        if (haveHighResTimer && getg().m.highResTimer != 0) {>>MARKER:FUNCTION_usleep2HighRes_BLOCK_PREFIX<<
            usleep2HighRes(dt);
        }
        else
 {>>MARKER:FUNCTION_usleep2_BLOCK_PREFIX<<
            usleep2(dt);
        }
    });

}

private static System.UIntPtr ctrlHandler(uint _type) {
    uint s = default;


    if (_type == _CTRL_C_EVENT || _type == _CTRL_BREAK_EVENT) 
        s = _SIGINT;
    else if (_type == _CTRL_CLOSE_EVENT || _type == _CTRL_LOGOFF_EVENT || _type == _CTRL_SHUTDOWN_EVENT) 
        s = _SIGTERM;
    else 
        return 0;
        if (sigsend(s)) {
        if (s == _SIGTERM) { 
            // Windows terminates the process after this handler returns.
            // Block indefinitely to give signal handlers a chance to clean up.
            stdcall1(_Sleep, uintptr(_INFINITE));

        }
        return 1;

    }
    return 0;

}

// called from zcallback_windows_*.s to sys_windows_*.s
private static void callbackasm1();

private static System.UIntPtr profiletimer = default;

private static void profilem(ptr<m> _addr_mp, System.UIntPtr thread) {
    ref m mp = ref _addr_mp.val;
 
    // Align Context to 16 bytes.
    ptr<context> c;
    array<byte> cbuf = new array<byte>(@unsafe.Sizeof(c.val) + 15);
    c = (context.val)(@unsafe.Pointer((uintptr(@unsafe.Pointer(_addr_cbuf[15]))) & ~15));

    c.contextflags = _CONTEXT_CONTROL;
    stdcall2(_GetThreadContext, thread, uintptr(@unsafe.Pointer(c)));

    var gp = gFromSP(_addr_mp, c.sp());

    sigprof(c.ip(), c.sp(), c.lr(), gp, mp);

}

private static ptr<g> gFromSP(ptr<m> _addr_mp, System.UIntPtr sp) {
    ref m mp = ref _addr_mp.val;

    {
        var gp__prev1 = gp;

        var gp = mp.g0;

        if (gp != null && gp.stack.lo < sp && sp < gp.stack.hi) {>>MARKER:FUNCTION_callbackasm1_BLOCK_PREFIX<<
            return _addr_gp!;
        }
        gp = gp__prev1;

    }

    {
        var gp__prev1 = gp;

        gp = mp.gsignal;

        if (gp != null && gp.stack.lo < sp && sp < gp.stack.hi) {
            return _addr_gp!;
        }
        gp = gp__prev1;

    }

    {
        var gp__prev1 = gp;

        gp = mp.curg;

        if (gp != null && gp.stack.lo < sp && sp < gp.stack.hi) {
            return _addr_gp!;
        }
        gp = gp__prev1;

    }

    return _addr_null!;

}

private static void profileLoop() {
    stdcall2(_SetThreadPriority, currentThread, _THREAD_PRIORITY_HIGHEST);

    while (true) {
        stdcall2(_WaitForSingleObject, profiletimer, _INFINITE);
        var first = (m.val)(atomic.Loadp(@unsafe.Pointer(_addr_allm)));
        {
            var mp = first;

            while (mp != null) {
                if (mp == getg().m) { 
                    // Don't profile ourselves.
                    continue;
                mp = mp.alllink;
                }

                lock(_addr_mp.threadLock); 
                // Do not profile threads blocked on Notes,
                // this includes idle worker threads,
                // idle timer thread, idle heap scavenger, etc.
                if (mp.thread == 0 || mp.profilehz == 0 || mp.blocked) {
                    unlock(_addr_mp.threadLock);
                    continue;
                } 
                // Acquire our own handle to the thread.
                ref System.UIntPtr thread = ref heap(out ptr<System.UIntPtr> _addr_thread);
                if (stdcall7(_DuplicateHandle, currentProcess, mp.thread, currentProcess, uintptr(@unsafe.Pointer(_addr_thread)), 0, 0, _DUPLICATE_SAME_ACCESS) == 0) {
                    print("runtime: duplicatehandle failed; errno=", getlasterror(), "\n");
                    throw("duplicatehandle failed");
                }

                unlock(_addr_mp.threadLock); 

                // mp may exit between the DuplicateHandle
                // above and the SuspendThread. The handle
                // will remain valid, but SuspendThread may
                // fail.
                if (int32(stdcall1(_SuspendThread, thread)) == -1) { 
                    // The thread no longer exists.
                    stdcall1(_CloseHandle, thread);
                    continue;

                }

                if (mp.profilehz != 0 && !mp.blocked) { 
                    // Pass the thread handle in case mp
                    // was in the process of shutting down.
                    profilem(_addr_mp, thread);

                }

                stdcall1(_ResumeThread, thread);
                stdcall1(_CloseHandle, thread);

            }

        }

    }

}

private static void setProcessCPUProfiler(int hz) {
    if (profiletimer == 0) {
        var timer = stdcall3(_CreateWaitableTimerA, 0, 0, 0);
        atomic.Storeuintptr(_addr_profiletimer, timer);
        newm(profileLoop, null, -1);
    }
}

private static void setThreadCPUProfiler(int hz) {
    var ms = int32(0);
    ref var due = ref heap(~int64(~uint64(1 << 63)), out ptr<var> _addr_due);
    if (hz > 0) {
        ms = 1000 / hz;
        if (ms == 0) {
            ms = 1;
        }
        due = int64(ms) * -10000;

    }
    stdcall6(_SetWaitableTimer, profiletimer, uintptr(@unsafe.Pointer(_addr_due)), uintptr(ms), 0, 0, 0);
    atomic.Store((uint32.val)(@unsafe.Pointer(_addr_getg().m.profilehz)), uint32(hz));

}

private static readonly var preemptMSupported = GOARCH == "386" || GOARCH == "amd64";

// suspendLock protects simultaneous SuspendThread operations from
// suspending each other.


// suspendLock protects simultaneous SuspendThread operations from
// suspending each other.
private static mutex suspendLock = default;

private static void preemptM(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (!preemptMSupported) { 
        // TODO: Implement call injection
        return ;

    }
    if (mp == getg().m) {
        throw("self-preempt");
    }
    if (!atomic.Cas(_addr_mp.preemptExtLock, 0, 1)) { 
        // External code is running. Fail the preemption
        // attempt.
        atomic.Xadd(_addr_mp.preemptGen, 1);
        return ;

    }
    lock(_addr_mp.threadLock);
    if (mp.thread == 0) { 
        // The M hasn't been minit'd yet (or was just unminit'd).
        unlock(_addr_mp.threadLock);
        atomic.Store(_addr_mp.preemptExtLock, 0);
        atomic.Xadd(_addr_mp.preemptGen, 1);
        return ;

    }
    ref System.UIntPtr thread = ref heap(out ptr<System.UIntPtr> _addr_thread);
    if (stdcall7(_DuplicateHandle, currentProcess, mp.thread, currentProcess, uintptr(@unsafe.Pointer(_addr_thread)), 0, 0, _DUPLICATE_SAME_ACCESS) == 0) {
        print("runtime.preemptM: duplicatehandle failed; errno=", getlasterror(), "\n");
        throw("runtime.preemptM: duplicatehandle failed");
    }
    unlock(_addr_mp.threadLock); 

    // Prepare thread context buffer. This must be aligned to 16 bytes.
    ptr<context> c;
    array<byte> cbuf = new array<byte>(@unsafe.Sizeof(c.val) + 15);
    c = (context.val)(@unsafe.Pointer((uintptr(@unsafe.Pointer(_addr_cbuf[15]))) & ~15));
    c.contextflags = _CONTEXT_CONTROL; 

    // Serialize thread suspension. SuspendThread is asynchronous,
    // so it's otherwise possible for two threads to suspend each
    // other and deadlock. We must hold this lock until after
    // GetThreadContext, since that blocks until the thread is
    // actually suspended.
    lock(_addr_suspendLock); 

    // Suspend the thread.
    if (int32(stdcall1(_SuspendThread, thread)) == -1) {
        unlock(_addr_suspendLock);
        stdcall1(_CloseHandle, thread);
        atomic.Store(_addr_mp.preemptExtLock, 0); 
        // The thread no longer exists. This shouldn't be
        // possible, but just acknowledge the request.
        atomic.Xadd(_addr_mp.preemptGen, 1);
        return ;

    }
    stdcall2(_GetThreadContext, thread, uintptr(@unsafe.Pointer(c)));

    unlock(_addr_suspendLock); 

    // Does it want a preemption and is it safe to preempt?
    var gp = gFromSP(_addr_mp, c.sp());
    if (gp != null && wantAsyncPreempt(gp)) {
        {
            var (ok, newpc) = isAsyncSafePoint(gp, c.ip(), c.sp(), c.lr());

            if (ok) { 
                // Inject call to asyncPreempt
                var targetPC = funcPC(asyncPreempt);
                switch (GOARCH) {
                    case "386": 
                        // Make it look like the thread called targetPC.

                    case "amd64": 
                        // Make it look like the thread called targetPC.
                        var sp = c.sp();
                        sp -= sys.PtrSize * (uintptr.val)(@unsafe.Pointer(sp));

                        newpc;
                        c.set_sp(sp);
                        c.set_ip(targetPC);

                        break;
                    default: 
                        throw("unsupported architecture");
                        break;
                }

                stdcall2(_SetThreadContext, thread, uintptr(@unsafe.Pointer(c)));

            }

        }

    }
    atomic.Store(_addr_mp.preemptExtLock, 0); 

    // Acknowledge the preemption.
    atomic.Xadd(_addr_mp.preemptGen, 1);

    stdcall1(_ResumeThread, thread);
    stdcall1(_CloseHandle, thread);

}

// osPreemptExtEnter is called before entering external code that may
// call ExitProcess.
//
// This must be nosplit because it may be called from a syscall with
// untyped stack slots, so the stack must not be grown or scanned.
//
//go:nosplit
private static void osPreemptExtEnter(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    while (!atomic.Cas(_addr_mp.preemptExtLock, 0, 1)) { 
        // An asynchronous preemption is in progress. It's not
        // safe to enter external code because it may call
        // ExitProcess and deadlock with SuspendThread.
        // Ideally we would do the preemption ourselves, but
        // can't since there may be untyped syscall arguments
        // on the stack. Instead, just wait and encourage the
        // SuspendThread APC to run. The preemption should be
        // done shortly.
        osyield();

    } 
    // Asynchronous preemption is now blocked.
}

// osPreemptExtExit is called after returning from external code that
// may call ExitProcess.
//
// See osPreemptExtEnter for why this is nosplit.
//
//go:nosplit
private static void osPreemptExtExit(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    atomic.Store(_addr_mp.preemptExtLock, 0);
}

} // end runtime_package
