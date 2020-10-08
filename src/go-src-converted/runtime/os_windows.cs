// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:22:12 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_windows.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // TODO(brainman): should not need those
        private static readonly long _NSIG = (long)65L;


        //go:cgo_import_dynamic runtime._AddVectoredExceptionHandler AddVectoredExceptionHandler%2 "kernel32.dll"
        //go:cgo_import_dynamic runtime._CloseHandle CloseHandle%1 "kernel32.dll"
        //go:cgo_import_dynamic runtime._CreateEventA CreateEventA%4 "kernel32.dll"
        //go:cgo_import_dynamic runtime._CreateIoCompletionPort CreateIoCompletionPort%4 "kernel32.dll"
        //go:cgo_import_dynamic runtime._CreateThread CreateThread%6 "kernel32.dll"
        //go:cgo_import_dynamic runtime._CreateWaitableTimerA CreateWaitableTimerA%3 "kernel32.dll"
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
        //go:cgo_import_dynamic runtime._OpenProcess OpenProcess%3 "kernel32.dll"
        //go:cgo_import_dynamic runtime._PostQueuedCompletionStatus PostQueuedCompletionStatus%4 "kernel32.dll"
        //go:cgo_import_dynamic runtime._ProcessIdToSessionId ProcessIdToSessionId%2 "kernel32.dll"
        //go:cgo_import_dynamic runtime._QueryFullProcessImageNameA QueryFullProcessImageNameA%4 "kernel32.dll"
        //go:cgo_import_dynamic runtime._ResumeThread ResumeThread%1 "kernel32.dll"
        //go:cgo_import_dynamic runtime._SetConsoleCtrlHandler SetConsoleCtrlHandler%2 "kernel32.dll"
        //go:cgo_import_dynamic runtime._SetErrorMode SetErrorMode%1 "kernel32.dll"
        //go:cgo_import_dynamic runtime._SetEvent SetEvent%1 "kernel32.dll"
        //go:cgo_import_dynamic runtime._SetProcessPriorityBoost SetProcessPriorityBoost%2 "kernel32.dll"
        //go:cgo_import_dynamic runtime._SetThreadPriority SetThreadPriority%2 "kernel32.dll"
        //go:cgo_import_dynamic runtime._SetUnhandledExceptionFilter SetUnhandledExceptionFilter%1 "kernel32.dll"
        //go:cgo_import_dynamic runtime._SetWaitableTimer SetWaitableTimer%6 "kernel32.dll"
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

        private partial struct stdFunction // : unsafe.Pointer
        {
        }

 
        // Following syscalls are available on every Windows PC.
        // All these variables are set by the Windows executable
        // loader before the Go program starts.
        private static stdFunction _AddVectoredExceptionHandler = default;        private static stdFunction _CloseHandle = default;        private static stdFunction _CreateEventA = default;        private static stdFunction _CreateIoCompletionPort = default;        private static stdFunction _CreateThread = default;        private static stdFunction _CreateWaitableTimerA = default;        private static stdFunction _DuplicateHandle = default;        private static stdFunction _ExitProcess = default;        private static stdFunction _FreeEnvironmentStringsW = default;        private static stdFunction _GetConsoleMode = default;        private static stdFunction _GetEnvironmentStringsW = default;        private static stdFunction _GetProcAddress = default;        private static stdFunction _GetProcessAffinityMask = default;        private static stdFunction _GetQueuedCompletionStatusEx = default;        private static stdFunction _GetStdHandle = default;        private static stdFunction _GetSystemDirectoryA = default;        private static stdFunction _GetSystemInfo = default;        private static stdFunction _GetSystemTimeAsFileTime = default;        private static stdFunction _GetThreadContext = default;        private static stdFunction _SetThreadContext = default;        private static stdFunction _LoadLibraryW = default;        private static stdFunction _LoadLibraryA = default;        private static stdFunction _OpenProcess = default;        private static stdFunction _PostQueuedCompletionStatus = default;        private static stdFunction _ProcessIdToSessionId = default;        private static stdFunction _QueryFullProcessImageNameA = default;        private static stdFunction _QueryPerformanceCounter = default;        private static stdFunction _QueryPerformanceFrequency = default;        private static stdFunction _ResumeThread = default;        private static stdFunction _SetConsoleCtrlHandler = default;        private static stdFunction _SetErrorMode = default;        private static stdFunction _SetEvent = default;        private static stdFunction _SetProcessPriorityBoost = default;        private static stdFunction _SetThreadPriority = default;        private static stdFunction _SetUnhandledExceptionFilter = default;        private static stdFunction _SetWaitableTimer = default;        private static stdFunction _SuspendThread = default;        private static stdFunction _SwitchToThread = default;        private static stdFunction _TlsAlloc = default;        private static stdFunction _VirtualAlloc = default;        private static stdFunction _VirtualFree = default;        private static stdFunction _VirtualQuery = default;        private static stdFunction _WaitForSingleObject = default;        private static stdFunction _WaitForMultipleObjects = default;        private static stdFunction _WriteConsoleW = default;        private static stdFunction _WriteFile = default;        private static stdFunction _ = default; 

        // Following syscalls are only available on some Windows PCs.
        // We will load syscalls, if available, before using them.
        private static stdFunction _AddDllDirectory = default;        private static stdFunction _AddVectoredContinueHandler = default;        private static stdFunction _LoadLibraryExA = default;        private static stdFunction _LoadLibraryExW = default;        private static stdFunction _ = default; 

        // Use RtlGenRandom to generate cryptographically random data.
        // This approach has been recommended by Microsoft (see issue
        // 15589 for details).
        // The RtlGenRandom is not listed in advapi32.dll, instead
        // RtlGenRandom function can be found by searching for SystemFunction036.
        // Also some versions of Mingw cannot link to SystemFunction036
        // when building executable as Cgo. So load SystemFunction036
        // manually during runtime startup.
        private static stdFunction _RtlGenRandom = default;        private static stdFunction _NtWaitForSingleObject = default;        private static stdFunction _NtQueryInformationProcess = default;        private static stdFunction _timeBeginPeriod = default;        private static stdFunction _timeEndPeriod = default;        private static stdFunction _WSAGetOverlappedResult = default;        private static stdFunction _ = default;


        // Function to be called by windows CreateThread
        // to start new os thread.
        private static void tstart_stdcall(ptr<m> newm)
;

        // Called by OS using stdcall ABI.
        private static void ctrlhandler()
;

        private partial struct mOS
        {
            public mutex threadLock; // protects "thread" and prevents closing
            public System.UIntPtr thread; // thread handle

            public System.UIntPtr waitsema; // semaphore for parking on locks
            public System.UIntPtr resumesema; // semaphore to indicate suspend/resume

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
        private static void os_sigpipe()
        {
            throw("too many writes on closed pipe");
        }

        // Stubs so tests can link correctly. These should never be called.
        private static int open(ptr<byte> _addr_name, int mode, int perm)
        {
            ref byte name = ref _addr_name.val;

            throw("unimplemented");
            return -1L;
        }
        private static int closefd(int fd)
        {
            throw("unimplemented");
            return -1L;
        }
        private static int read(int fd, unsafe.Pointer p, int n)
        {
            throw("unimplemented");
            return -1L;
        }

        private partial struct sigset
        {
        }

        // Call a Windows function with stdcall conventions,
        // and switch to os stack during the call.
        private static void asmstdcall(unsafe.Pointer fn)
;

        private static unsafe.Pointer asmstdcallAddr = default;

        private static stdFunction windowsFindfunc(System.UIntPtr lib, slice<byte> name)
        {
            if (name[len(name) - 1L] != 0L)
            {>>MARKER:FUNCTION_asmstdcall_BLOCK_PREFIX<<
                throw("usage");
            }

            var f = stdcall2(_GetProcAddress, lib, uintptr(@unsafe.Pointer(_addr_name[0L])));
            return stdFunction(@unsafe.Pointer(f));

        }

        private static array<byte> sysDirectory = new array<byte>(521L);
        private static System.UIntPtr sysDirectoryLen = default;

        private static System.UIntPtr windowsLoadSystemLib(slice<byte> name)
        {
            if (useLoadLibraryEx)
            {>>MARKER:FUNCTION_ctrlhandler_BLOCK_PREFIX<<
                return stdcall3(_LoadLibraryExA, uintptr(@unsafe.Pointer(_addr_name[0L])), 0L, _LOAD_LIBRARY_SEARCH_SYSTEM32);
            }
            else
            {>>MARKER:FUNCTION_tstart_stdcall_BLOCK_PREFIX<<
                if (sysDirectoryLen == 0L)
                {
                    var l = stdcall2(_GetSystemDirectoryA, uintptr(@unsafe.Pointer(_addr_sysDirectory[0L])), uintptr(len(sysDirectory) - 1L));
                    if (l == 0L || l > uintptr(len(sysDirectory) - 1L))
                    {
                        throw("Unable to determine system directory");
                    }

                    sysDirectory[l] = '\\';
                    sysDirectoryLen = l + 1L;

                }

                var absName = append(sysDirectory[..sysDirectoryLen], name);
                return stdcall1(_LoadLibraryA, uintptr(@unsafe.Pointer(_addr_absName[0L])));

            }

        }

        private static void loadOptionalSyscalls()
        {
            slice<byte> kernel32dll = (slice<byte>)"kernel32.dll ";
            var k32 = stdcall1(_LoadLibraryA, uintptr(@unsafe.Pointer(_addr_kernel32dll[0L])));
            if (k32 == 0L)
            {
                throw("kernel32.dll not found");
            }

            _AddDllDirectory = windowsFindfunc(k32, (slice<byte>)"AddDllDirectory ");
            _AddVectoredContinueHandler = windowsFindfunc(k32, (slice<byte>)"AddVectoredContinueHandler ");
            _LoadLibraryExA = windowsFindfunc(k32, (slice<byte>)"LoadLibraryExA ");
            _LoadLibraryExW = windowsFindfunc(k32, (slice<byte>)"LoadLibraryExW ");
            useLoadLibraryEx = (_LoadLibraryExW != null && _LoadLibraryExA != null && _AddDllDirectory != null);

            slice<byte> advapi32dll = (slice<byte>)"advapi32.dll ";
            var a32 = windowsLoadSystemLib(advapi32dll);
            if (a32 == 0L)
            {
                throw("advapi32.dll not found");
            }

            _RtlGenRandom = windowsFindfunc(a32, (slice<byte>)"SystemFunction036 ");

            slice<byte> ntdll = (slice<byte>)"ntdll.dll ";
            var n32 = windowsLoadSystemLib(ntdll);
            if (n32 == 0L)
            {
                throw("ntdll.dll not found");
            }

            _NtWaitForSingleObject = windowsFindfunc(n32, (slice<byte>)"NtWaitForSingleObject ");
            _NtQueryInformationProcess = windowsFindfunc(n32, (slice<byte>)"NtQueryInformationProcess ");

            if (GOARCH == "arm")
            {
                _QueryPerformanceCounter = windowsFindfunc(k32, (slice<byte>)"QueryPerformanceCounter ");
                if (_QueryPerformanceCounter == null)
                {
                    throw("could not find QPC syscalls");
                }

            }

            slice<byte> winmmdll = (slice<byte>)"winmm.dll ";
            var m32 = windowsLoadSystemLib(winmmdll);
            if (m32 == 0L)
            {
                throw("winmm.dll not found");
            }

            _timeBeginPeriod = windowsFindfunc(m32, (slice<byte>)"timeBeginPeriod ");
            _timeEndPeriod = windowsFindfunc(m32, (slice<byte>)"timeEndPeriod ");
            if (_timeBeginPeriod == null || _timeEndPeriod == null)
            {
                throw("timeBegin/EndPeriod not found");
            }

            slice<byte> ws232dll = (slice<byte>)"ws2_32.dll ";
            var ws232 = windowsLoadSystemLib(ws232dll);
            if (ws232 == 0L)
            {
                throw("ws2_32.dll not found");
            }

            _WSAGetOverlappedResult = windowsFindfunc(ws232, (slice<byte>)"WSAGetOverlappedResult ");
            if (_WSAGetOverlappedResult == null)
            {
                throw("WSAGetOverlappedResult not found");
            }

            if (windowsFindfunc(n32, (slice<byte>)"wine_get_version ") != null)
            { 
                // running on Wine
                initWine(k32);

            }

        }

        private static void monitorSuspendResume()
        {
            const long _DEVICE_NOTIFY_CALLBACK = (long)2L;
            private partial struct _DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS
            {
                public System.UIntPtr callback;
                public System.UIntPtr context;
            }

            var powrprof = windowsLoadSystemLib((slice<byte>)"powrprof.dll ");
            if (powrprof == 0L)
            {
                return ; // Running on Windows 7, where we don't need it anyway.
            }

            var powerRegisterSuspendResumeNotification = windowsFindfunc(powrprof, (slice<byte>)"PowerRegisterSuspendResumeNotification ");
            if (powerRegisterSuspendResumeNotification == null)
            {
                return ; // Running on Windows 7, where we don't need it anyway.
            }

            ref Func<System.UIntPtr, uint, System.UIntPtr, System.UIntPtr> fn = ref heap((context, changeType, setting) =>
            {
                {
                    var mp = (m.val)(atomic.Loadp(@unsafe.Pointer(_addr_allm)));

                    while (mp != null)
                    {
                        if (mp.resumesema != 0L)
                        {
                            stdcall1(_SetEvent, mp.resumesema);
                        mp = mp.alllink;
                        }

                    }

                }
                return 0L;

            }
, out ptr<Func<System.UIntPtr, uint, System.UIntPtr, System.UIntPtr>> _addr_fn);
            ref _DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS @params = ref heap(new _DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS(callback:compileCallback(*efaceOf(&fn),true),), out ptr<_DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS> _addr_@params);
            ref var handle = ref heap(uintptr(0L), out ptr<var> _addr_handle);
            stdcall3(powerRegisterSuspendResumeNotification, _DEVICE_NOTIFY_CALLBACK, uintptr(@unsafe.Pointer(_addr_params)), uintptr(@unsafe.Pointer(_addr_handle)));

        }

        //go:nosplit
        private static System.UIntPtr getLoadLibrary()
        {
            return uintptr(@unsafe.Pointer(_LoadLibraryW));
        }

        //go:nosplit
        private static System.UIntPtr getLoadLibraryEx()
        {
            return uintptr(@unsafe.Pointer(_LoadLibraryExW));
        }

        //go:nosplit
        private static System.UIntPtr getGetProcAddress()
        {
            return uintptr(@unsafe.Pointer(_GetProcAddress));
        }

        private static int getproccount()
        {
            ref System.UIntPtr mask = ref heap(out ptr<System.UIntPtr> _addr_mask);            ref System.UIntPtr sysmask = ref heap(out ptr<System.UIntPtr> _addr_sysmask);

            var ret = stdcall3(_GetProcessAffinityMask, currentProcess, uintptr(@unsafe.Pointer(_addr_mask)), uintptr(@unsafe.Pointer(_addr_sysmask)));
            if (ret != 0L)
            {
                long n = 0L;
                var maskbits = int(@unsafe.Sizeof(mask) * 8L);
                for (long i = 0L; i < maskbits; i++)
                {
                    if (mask & (1L << (int)(uint(i))) != 0L)
                    {
                        n++;
                    }

                }

                if (n != 0L)
                {
                    return int32(n);
                }

            } 
            // use GetSystemInfo if GetProcessAffinityMask fails
            ref systeminfo info = ref heap(out ptr<systeminfo> _addr_info);
            stdcall1(_GetSystemInfo, uintptr(@unsafe.Pointer(_addr_info)));
            return int32(info.dwnumberofprocessors);

        }

        private static System.UIntPtr getPageSize()
        {
            ref systeminfo info = ref heap(out ptr<systeminfo> _addr_info);
            stdcall1(_GetSystemInfo, uintptr(@unsafe.Pointer(_addr_info)));
            return uintptr(info.dwpagesize);
        }

        private static readonly var currentProcess = (var)~uintptr(0L); // -1 = current process
        private static readonly var currentThread = (var)~uintptr(1L); // -2 = current thread

        // in sys_windows_386.s and sys_windows_amd64.s:
        private static void externalthreadhandler()
;
        private static uint getlasterror()
;
        private static void setlasterror(uint err)
;

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
        private static readonly long osRelaxMinNS = (long)60L * 1e6F;

        // osRelax is called by the scheduler when transitioning to and from
        // all Ps being idle.
        //
        // On Windows, it adjusts the system-wide timer resolution. Go needs a
        // high resolution timer while running and there's little extra cost
        // if we're already using the CPU, but if all Ps are idle there's no
        // need to consume extra power to drive the high-res timer.


        // osRelax is called by the scheduler when transitioning to and from
        // all Ps being idle.
        //
        // On Windows, it adjusts the system-wide timer resolution. Go needs a
        // high resolution timer while running and there's little extra cost
        // if we're already using the CPU, but if all Ps are idle there's no
        // need to consume extra power to drive the high-res timer.
        private static uint osRelax(bool relax)
        {
            if (relax)
            {>>MARKER:FUNCTION_setlasterror_BLOCK_PREFIX<<
                return uint32(stdcall1(_timeEndPeriod, 1L));
            }
            else
            {>>MARKER:FUNCTION_getlasterror_BLOCK_PREFIX<<
                return uint32(stdcall1(_timeBeginPeriod, 1L));
            }

        }

        private static void osinit()
        {
            asmstdcallAddr = @unsafe.Pointer(funcPC(asmstdcall));
            usleep2Addr = @unsafe.Pointer(funcPC(usleep2));
            switchtothreadAddr = @unsafe.Pointer(funcPC(switchtothread));

            setBadSignalMsg();

            loadOptionalSyscalls();

            disableWER();

            initExceptionHandler();

            stdcall2(_SetConsoleCtrlHandler, funcPC(ctrlhandler), 1L);

            timeBeginPeriodRetValue = osRelax(false);

            ncpu = getproccount();

            physPageSize = getPageSize(); 

            // Windows dynamic priority boosting assumes that a process has different types
            // of dedicated threads -- GUI, IO, computational, etc. Go processes use
            // equivalent threads that all do a mix of GUI, IO, computations, etc.
            // In such context dynamic priority boosting does nothing but harm, so we turn it off.
            stdcall2(_SetProcessPriorityBoost, currentProcess, 1L);

        }

        // useQPCTime controls whether time.now and nanotime use QueryPerformanceCounter.
        // This is only set to 1 when running under Wine.
        private static byte useQPCTime = default;

        private static long qpcStartCounter = default;
        private static long qpcMultiplier = default;

        //go:nosplit
        private static long nanotimeQPC()
        {
            ref long counter = ref heap(0L, out ptr<long> _addr_counter);
            stdcall1(_QueryPerformanceCounter, uintptr(@unsafe.Pointer(_addr_counter))); 

            // returns number of nanoseconds
            return (counter - qpcStartCounter) * qpcMultiplier;

        }

        //go:nosplit
        private static (long, int, long) nowQPC()
        {
            long sec = default;
            int nsec = default;
            long mono = default;

            ref long ft = ref heap(out ptr<long> _addr_ft);
            stdcall1(_GetSystemTimeAsFileTime, uintptr(@unsafe.Pointer(_addr_ft)));

            var t = (ft - 116444736000000000L) * 100L;

            sec = t / 1000000000L;
            nsec = int32(t - sec * 1000000000L);

            mono = nanotimeQPC();
            return ;
        }

        private static void initWine(System.UIntPtr k32)
        {
            _GetSystemTimeAsFileTime = windowsFindfunc(k32, (slice<byte>)"GetSystemTimeAsFileTime ");
            if (_GetSystemTimeAsFileTime == null)
            {>>MARKER:FUNCTION_externalthreadhandler_BLOCK_PREFIX<<
                throw("could not find GetSystemTimeAsFileTime() syscall");
            }

            _QueryPerformanceCounter = windowsFindfunc(k32, (slice<byte>)"QueryPerformanceCounter ");
            _QueryPerformanceFrequency = windowsFindfunc(k32, (slice<byte>)"QueryPerformanceFrequency ");
            if (_QueryPerformanceCounter == null || _QueryPerformanceFrequency == null)
            {
                throw("could not find QPC syscalls");
            } 

            // We can not simply fallback to GetSystemTimeAsFileTime() syscall, since its time is not monotonic,
            // instead we use QueryPerformanceCounter family of syscalls to implement monotonic timer
            // https://msdn.microsoft.com/en-us/library/windows/desktop/dn553408(v=vs.85).aspx
            ref long tmp = ref heap(out ptr<long> _addr_tmp);
            stdcall1(_QueryPerformanceFrequency, uintptr(@unsafe.Pointer(_addr_tmp)));
            if (tmp == 0L)
            {
                throw("QueryPerformanceFrequency syscall returned zero, running on unsupported hardware");
            } 

            // This should not overflow, it is a number of ticks of the performance counter per second,
            // its resolution is at most 10 per usecond (on Wine, even smaller on real hardware), so it will be at most 10 millions here,
            // panic if overflows.
            if (tmp > (1L << (int)(31L) - 1L))
            {
                throw("QueryPerformanceFrequency overflow 32 bit divider, check nosplit discussion to proceed");
            }

            var qpcFrequency = int32(tmp);
            stdcall1(_QueryPerformanceCounter, uintptr(@unsafe.Pointer(_addr_qpcStartCounter))); 

            // Since we are supposed to run this time calls only on Wine, it does not lose precision,
            // since Wine's timer is kind of emulated at 10 Mhz, so it will be a nice round multiplier of 100
            // but for general purpose system (like 3.3 Mhz timer on i7) it will not be very precise.
            // We have to do it this way (or similar), since multiplying QPC counter by 100 millions overflows
            // int64 and resulted time will always be invalid.
            qpcMultiplier = int64(timediv(1000000000L, qpcFrequency, null));

            useQPCTime = 1L;

        }

        //go:nosplit
        private static void getRandomData(slice<byte> r)
        {
            long n = 0L;
            if (stdcall2(_RtlGenRandom, uintptr(@unsafe.Pointer(_addr_r[0L])), uintptr(len(r))) & 0xffUL != 0L)
            {
                n = len(r);
            }

            extendRandom(r, n);

        }

        private static void goenvs()
        { 
            // strings is a pointer to environment variable pairs in the form:
            //     "envA=valA\x00envB=valB\x00\x00" (in UTF-16)
            // Two consecutive zero bytes end the list.
            var strings = @unsafe.Pointer(stdcall0(_GetEnvironmentStringsW));
            ptr<array<ushort>> p = new ptr<ptr<array<ushort>>>(strings)[..];

            long n = 0L;
            {
                long i__prev1 = i;

                for (long from = 0L;
                long i = 0L; true; i++)
                {
                    if (p[i] == 0L)
                    { 
                        // empty string marks the end
                        if (i == from)
                        {
                            break;
                        }

                        from = i + 1L;
                        n++;

                    }

                }


                i = i__prev1;
            }
            envs = make_slice<@string>(n);

            {
                long i__prev1 = i;

                foreach (var (__i) in envs)
                {
                    i = __i;
                    envs[i] = gostringw(_addr_p[0L]);
                    while (p[0L] != 0L)
                    {
                        p = p[1L..];
                    }

                    p = p[1L..]; // skip nil byte
                }

                i = i__prev1;
            }

            stdcall1(_FreeEnvironmentStringsW, uintptr(strings)); 

            // We call this all the way here, late in init, so that malloc works
            // for the callback function this generates.
            monitorSuspendResume();

        }

        // exiting is set to non-zero when the process is exiting.
        private static uint exiting = default;

        //go:nosplit
        private static void exit(int code)
        { 
            // Disallow thread suspension for preemption. Otherwise,
            // ExitProcess and SuspendThread can race: SuspendThread
            // queues a suspension request for this thread, ExitProcess
            // kills the suspending thread, and then this thread suspends.
            lock(_addr_suspendLock);
            atomic.Store(_addr_exiting, 1L);
            stdcall1(_ExitProcess, uintptr(code));

        }

        // write1 must be nosplit because it's used as a last resort in
        // functions like badmorestackg0. In such cases, we'll always take the
        // ASCII path.
        //
        //go:nosplit
        private static int write1(System.UIntPtr fd, unsafe.Pointer buf, int n)
        {
            const var _STD_OUTPUT_HANDLE = (var)~uintptr(10L); // -11
            const var _STD_ERROR_HANDLE = (var)~uintptr(11L); // -12
            System.UIntPtr handle = default;
            switch (fd)
            {
                case 1L: 
                    handle = stdcall1(_GetStdHandle, _STD_OUTPUT_HANDLE);
                    break;
                case 2L: 
                    handle = stdcall1(_GetStdHandle, _STD_ERROR_HANDLE);
                    break;
                default: 
                    // assume fd is real windows handle.
                    handle = fd;
                    break;
            }
            var isASCII = true;
            ptr<array<byte>> b = new ptr<ptr<array<byte>>>(buf)[..n];
            foreach (var (_, x) in b)
            {
                if (x >= 0x80UL)
                {
                    isASCII = false;
                    break;
                }

            }
            if (!isASCII)
            {
                ref uint m = ref heap(out ptr<uint> _addr_m);
                var isConsole = stdcall2(_GetConsoleMode, handle, uintptr(@unsafe.Pointer(_addr_m))) != 0L; 
                // If this is a console output, various non-unicode code pages can be in use.
                // Use the dedicated WriteConsole call to ensure unicode is printed correctly.
                if (isConsole)
                {
                    return int32(writeConsole(handle, buf, n));
                }

            }

            ref uint written = ref heap(out ptr<uint> _addr_written);
            stdcall5(_WriteFile, handle, uintptr(buf), uintptr(n), uintptr(@unsafe.Pointer(_addr_written)), 0L);
            return int32(written);

        }

        private static array<ushort> utf16ConsoleBack = new array<ushort>(1000L);        private static mutex utf16ConsoleBackLock = default;

        // writeConsole writes bufLen bytes from buf to the console File.
        // It returns the number of bytes written.
        private static long writeConsole(System.UIntPtr handle, unsafe.Pointer buf, int bufLen)
        {
            const var surr2 = (var)(surrogateMin + surrogateMax + 1L) / 2L; 

            // Do not use defer for unlock. May cause issues when printing a panic.
 

            // Do not use defer for unlock. May cause issues when printing a panic.
            lock(_addr_utf16ConsoleBackLock);

            ptr<array<byte>> b = new ptr<ptr<array<byte>>>(buf)[..bufLen];
            ptr<ptr<@string>> s = new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(_addr_b));

            var utf16tmp = utf16ConsoleBack[..];

            var total = len(s);
            long w = 0L;
            foreach (var (_, r) in s)
            {
                if (w >= len(utf16tmp) - 2L)
                {
                    writeConsoleUTF16(handle, utf16tmp[..w]);
                    w = 0L;
                }

                if (r < 0x10000UL)
                {
                    utf16tmp[w] = uint16(r);
                    w++;
                }
                else
                {
                    r -= 0x10000UL;
                    utf16tmp[w] = surrogateMin + uint16(r >> (int)(10L)) & 0x3ffUL;
                    utf16tmp[w + 1L] = surr2 + uint16(r) & 0x3ffUL;
                    w += 2L;
                }

            }
            writeConsoleUTF16(handle, utf16tmp[..w]);
            unlock(_addr_utf16ConsoleBackLock);
            return total;

        }

        // writeConsoleUTF16 is the dedicated windows calls that correctly prints
        // to the console regardless of the current code page. Input is utf-16 code points.
        // The handle must be a console handle.
        private static void writeConsoleUTF16(System.UIntPtr handle, slice<ushort> b)
        {
            var l = uint32(len(b));
            if (l == 0L)
            {
                return ;
            }

            ref uint written = ref heap(out ptr<uint> _addr_written);
            stdcall5(_WriteConsoleW, handle, uintptr(@unsafe.Pointer(_addr_b[0L])), uintptr(l), uintptr(@unsafe.Pointer(_addr_written)), 0L);
            return ;

        }

        // walltime1 isn't implemented on Windows, but will never be called.
        private static (long, int) walltime1()
;

        //go:nosplit
        private static int semasleep(long ns)
        {
            const ulong _WAIT_ABANDONED = (ulong)0x00000080UL;
            const ulong _WAIT_OBJECT_0 = (ulong)0x00000000UL;
            const ulong _WAIT_TIMEOUT = (ulong)0x00000102UL;
            const ulong _WAIT_FAILED = (ulong)0xFFFFFFFFUL;

            System.UIntPtr result = default;
            if (ns < 0L)
            {>>MARKER:FUNCTION_walltime1_BLOCK_PREFIX<<
                result = stdcall2(_WaitForSingleObject, getg().m.waitsema, uintptr(_INFINITE));
            }
            else
            {
                var start = nanotime();
                var elapsed = int64(0L);
                while (true)
                {
                    var ms = int64(timediv(ns - elapsed, 1000000L, null));
                    if (ms == 0L)
                    {
                        ms = 1L;
                    }

                    result = stdcall4(_WaitForMultipleObjects, 2L, uintptr(@unsafe.Pointer(addr(new array<System.UIntPtr>(new System.UIntPtr[] { getg().m.waitsema, getg().m.resumesema })))), 0L, uintptr(ms));
                    if (result != _WAIT_OBJECT_0 + 1L)
                    { 
                        // Not a suspend/resume event
                        break;

                    }

                    elapsed = nanotime() - start;
                    if (elapsed >= ns)
                    {
                        return -1L;
                    }

                }


            }


            if (result == _WAIT_OBJECT_0) // Signaled
                return 0L;
            else if (result == _WAIT_TIMEOUT) 
                return -1L;
            else if (result == _WAIT_ABANDONED) 
                systemstack(() =>
                {
                    throw("runtime.semasleep wait_abandoned");
                });
            else if (result == _WAIT_FAILED) 
                systemstack(() =>
                {
                    print("runtime: waitforsingleobject wait_failed; errno=", getlasterror(), "\n");
                    throw("runtime.semasleep wait_failed");
                });
            else 
                systemstack(() =>
                {
                    print("runtime: waitforsingleobject unexpected; result=", result, "\n");
                    throw("runtime.semasleep unexpected");
                });
                        return -1L; // unreachable
        }

        //go:nosplit
        private static void semawakeup(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            if (stdcall1(_SetEvent, mp.waitsema) == 0L)
            {
                systemstack(() =>
                {
                    print("runtime: setevent failed; errno=", getlasterror(), "\n");
                    throw("runtime.semawakeup");
                });

            }

        }

        //go:nosplit
        private static void semacreate(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            if (mp.waitsema != 0L)
            {
                return ;
            }

            mp.waitsema = stdcall4(_CreateEventA, 0L, 0L, 0L, 0L);
            if (mp.waitsema == 0L)
            {
                systemstack(() =>
                {
                    print("runtime: createevent failed; errno=", getlasterror(), "\n");
                    throw("runtime.semacreate");
                });

            }

            mp.resumesema = stdcall4(_CreateEventA, 0L, 0L, 0L, 0L);
            if (mp.resumesema == 0L)
            {
                systemstack(() =>
                {
                    print("runtime: createevent failed; errno=", getlasterror(), "\n");
                    throw("runtime.semacreate");
                });
                stdcall1(_CloseHandle, mp.waitsema);
                mp.waitsema = 0L;

            }

        }

        // May run with m.p==nil, so write barriers are not allowed. This
        // function is called by newosproc0, so it is also required to
        // operate without stack guards.
        //go:nowritebarrierrec
        //go:nosplit
        private static void newosproc(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;
 
            // We pass 0 for the stack size to use the default for this binary.
            var thandle = stdcall6(_CreateThread, 0L, 0L, funcPC(tstart_stdcall), uintptr(@unsafe.Pointer(mp)), 0L, 0L);

            if (thandle == 0L)
            {
                if (atomic.Load(_addr_exiting) != 0L)
                { 
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

            // Close thandle to avoid leaking the thread object if it exits.
            stdcall1(_CloseHandle, thandle);

        }

        // Used by the C library build mode. On Linux this function would allocate a
        // stack, but that's not necessary for Windows. No stack guards are present
        // and the GC has not been initialized, so write barriers will fail.
        //go:nowritebarrierrec
        //go:nosplit
        private static void newosproc0(ptr<m> _addr_mp, unsafe.Pointer stk)
        {
            ref m mp = ref _addr_mp.val;
 
            // TODO: this is completely broken. The args passed to newosproc0 (in asm_amd64.s)
            // are stacksize and function, not *m and stack.
            // Check os_linux.go for an implementation that might actually work.
            throw("bad newosproc0");

        }

        private static void exitThread(ptr<uint> _addr_wait)
        {
            ref uint wait = ref _addr_wait.val;
 
            // We should never reach exitThread on Windows because we let
            // the OS clean up threads.
            throw("exitThread");

        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the parent thread (main thread in case of bootstrap), can allocate memory.
        private static void mpreinit(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

        }

        //go:nosplit
        private static void msigsave(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

        }

        //go:nosplit
        private static void msigrestore(sigset sigmask)
        {
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void clearSignalHandlers()
        {
        }

        //go:nosplit
        private static void sigblock()
        {
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the new thread, cannot allocate memory.
        private static void minit()
        {
            ref System.UIntPtr thandle = ref heap(out ptr<System.UIntPtr> _addr_thandle);
            stdcall7(_DuplicateHandle, currentProcess, currentThread, currentProcess, uintptr(@unsafe.Pointer(_addr_thandle)), 0L, 0L, _DUPLICATE_SAME_ACCESS);

            var mp = getg().m;
            lock(_addr_mp.threadLock);
            mp.thread = thandle;
            unlock(_addr_mp.threadLock); 

            // Query the true stack base from the OS. Currently we're
            // running on a small assumed stack.
            ref memoryBasicInformation mbi = ref heap(out ptr<memoryBasicInformation> _addr_mbi);
            var res = stdcall3(_VirtualQuery, uintptr(@unsafe.Pointer(_addr_mbi)), uintptr(@unsafe.Pointer(_addr_mbi)), @unsafe.Sizeof(mbi));
            if (res == 0L)
            {
                print("runtime: VirtualQuery failed; errno=", getlasterror(), "\n");
                throw("VirtualQuery for stack base failed");
            } 
            // The system leaves an 8K PAGE_GUARD region at the bottom of
            // the stack (in theory VirtualQuery isn't supposed to include
            // that, but it does). Add an additional 8K of slop for
            // calling C functions that don't have stack checks and for
            // lastcontinuehandler. We shouldn't be anywhere near this
            // bound anyway.
            var @base = mbi.allocationBase + 16L << (int)(10L); 
            // Sanity check the stack bounds.
            var g0 = getg();
            if (base > g0.stack.hi || g0.stack.hi - base > 64L << (int)(20L))
            {
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
        private static void unminit()
        {
            var mp = getg().m;
            lock(_addr_mp.threadLock);
            stdcall1(_CloseHandle, mp.thread);
            mp.thread = 0L;
            unlock(_addr_mp.threadLock);
        }

        // Calling stdcall on os stack.
        // May run during STW, so write barriers are not allowed.
        //go:nowritebarrier
        //go:nosplit
        private static System.UIntPtr stdcall(stdFunction fn)
        {
            var gp = getg();
            var mp = gp.m;
            mp.libcall.fn = uintptr(@unsafe.Pointer(fn));
            var resetLibcall = false;
            if (mp.profilehz != 0L && mp.libcallsp == 0L)
            { 
                // leave pc/sp for cpu profiler
                mp.libcallg.set(gp);
                mp.libcallpc = getcallerpc(); 
                // sp must be the last, because once async cpu profiler finds
                // all three values to be non-zero, it will use them
                mp.libcallsp = getcallersp();
                resetLibcall = true; // See comment in sys_darwin.go:libcCall
            }

            asmcgocall(asmstdcallAddr, @unsafe.Pointer(_addr_mp.libcall));
            if (resetLibcall)
            {
                mp.libcallsp = 0L;
            }

            return mp.libcall.r1;

        }

        //go:nosplit
        private static System.UIntPtr stdcall0(stdFunction fn)
        {
            var mp = getg().m;
            mp.libcall.n = 0L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_fn))); // it's unused but must be non-nil, otherwise crashes
            return stdcall(fn);

        }

        //go:nosplit
        private static System.UIntPtr stdcall1(stdFunction fn, System.UIntPtr a0)
        {
            var mp = getg().m;
            mp.libcall.n = 1L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall2(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1)
        {
            var mp = getg().m;
            mp.libcall.n = 2L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall3(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2)
        {
            var mp = getg().m;
            mp.libcall.n = 3L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall4(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            var mp = getg().m;
            mp.libcall.n = 4L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall5(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4)
        {
            var mp = getg().m;
            mp.libcall.n = 5L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall6(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
        {
            var mp = getg().m;
            mp.libcall.n = 6L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall7(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            var mp = getg().m;
            mp.libcall.n = 7L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a0)));
            return stdcall(fn);
        }

        // in sys_windows_386.s and sys_windows_amd64.s
        private static void onosstack(unsafe.Pointer fn, uint arg)
;
        private static void usleep2(uint usec)
;
        private static void switchtothread()
;

        private static unsafe.Pointer usleep2Addr = default;
        private static unsafe.Pointer switchtothreadAddr = default;

        //go:nosplit
        private static void osyield()
        {
            onosstack(switchtothreadAddr, 0L);
        }

        //go:nosplit
        private static void usleep(uint us)
        { 
            // Have 1us units; want 100ns units.
            onosstack(usleep2Addr, 10L * us);

        }

        // isWindowsService returns whether the process is currently executing as a
        // Windows service. The below technique looks a bit hairy, but it's actually
        // exactly what the .NET framework does for the similarly named function:
        // https://github.com/dotnet/extensions/blob/f4066026ca06984b07e90e61a6390ac38152ba93/src/Hosting/WindowsServices/src/WindowsServiceHelpers.cs#L26-L31
        // Specifically, it looks up whether the parent process has session ID zero
        // and is called "services".
        private static bool isWindowsService() => func((defer, _, __) =>
        {
            const var _CURRENT_PROCESS = (var)~uintptr(0L);
            const ulong _PROCESS_QUERY_LIMITED_INFORMATION = (ulong)0x1000UL; 
            // pbi is a PROCESS_BASIC_INFORMATION struct, where we just care about
            // the 6th pointer inside of it, which contains the pid of the process
            // parent:
            // https://github.com/wine-mirror/wine/blob/42cb7d2ad1caba08de235e6319b9967296b5d554/include/winternl.h#L1294
            array<System.UIntPtr> pbi = new array<System.UIntPtr>(6L);
            ref uint pbiLen = ref heap(out ptr<uint> _addr_pbiLen);
            var err = stdcall5(_NtQueryInformationProcess, _CURRENT_PROCESS, 0L, uintptr(@unsafe.Pointer(_addr_pbi[0L])), uintptr(@unsafe.Sizeof(pbi)), uintptr(@unsafe.Pointer(_addr_pbiLen)));
            if (err != 0L)
            {>>MARKER:FUNCTION_switchtothread_BLOCK_PREFIX<<
                return false;
            }

            ref uint psid = ref heap(out ptr<uint> _addr_psid);
            err = stdcall2(_ProcessIdToSessionId, pbi[5L], uintptr(@unsafe.Pointer(_addr_psid)));
            if (err == 0L || psid != 0L)
            {>>MARKER:FUNCTION_usleep2_BLOCK_PREFIX<<
                return false;
            }

            var pproc = stdcall3(_OpenProcess, _PROCESS_QUERY_LIMITED_INFORMATION, 0L, pbi[5L]);
            if (pproc == 0L)
            {>>MARKER:FUNCTION_onosstack_BLOCK_PREFIX<<
                return false;
            }

            defer(stdcall1(_CloseHandle, pproc)); 
            // exeName gets the path to the executable image of the parent process
            array<byte> exeName = new array<byte>(261L);
            ref var exeNameLen = ref heap(uint32(len(exeName) - 1L), out ptr<var> _addr_exeNameLen);
            err = stdcall4(_QueryFullProcessImageNameA, pproc, 0L, uintptr(@unsafe.Pointer(_addr_exeName[0L])), uintptr(@unsafe.Pointer(_addr_exeNameLen)));
            if (err == 0L || exeNameLen == 0L)
            {
                return false;
            }

            @string servicesLower = "services.exe";
            @string servicesUpper = "SERVICES.EXE";
            var i = int(exeNameLen) - 1L;
            var j = len(servicesLower) - 1L;
            if (i < j)
            {
                return false;
            }

            while (true)
            {
                if (j == -1L)
                {
                    return i == -1L || exeName[i] == '\\';
                }

                if (exeName[i] != servicesLower[j] && exeName[i] != servicesUpper[j])
                {
                    return false;
                }

                i--;
                j--;

            }


        });

        private static uint ctrlhandler1(uint _type)
        {
            uint s = default;


            if (_type == _CTRL_C_EVENT || _type == _CTRL_BREAK_EVENT) 
                s = _SIGINT;
            else if (_type == _CTRL_CLOSE_EVENT || _type == _CTRL_LOGOFF_EVENT || _type == _CTRL_SHUTDOWN_EVENT) 
                s = _SIGTERM;
            else 
                return 0L;
                        if (sigsend(s))
            {
                return 1L;
            }

            if (!islibrary && !isarchive && !isWindowsService())
            { 
                // Only exit the program if we don't have a DLL or service.
                // See https://golang.org/issues/35965 and https://golang.org/issues/40167
                exit(2L); // SIGINT, SIGTERM, etc
            }

            return 0L;

        }

        // in sys_windows_386.s and sys_windows_amd64.s
        private static void profileloop()
;

        // called from zcallback_windows_*.s to sys_windows_*.s
        private static void callbackasm1()
;

        private static System.UIntPtr profiletimer = default;

        private static void profilem(ptr<m> _addr_mp, System.UIntPtr thread)
        {
            ref m mp = ref _addr_mp.val;
 
            // Align Context to 16 bytes.
            ptr<context> c;
            array<byte> cbuf = new array<byte>(@unsafe.Sizeof(c.val) + 15L);
            c = (context.val)(@unsafe.Pointer((uintptr(@unsafe.Pointer(_addr_cbuf[15L]))) & ~15L));

            c.contextflags = _CONTEXT_CONTROL;
            stdcall2(_GetThreadContext, thread, uintptr(@unsafe.Pointer(c)));

            var gp = gFromTLS(_addr_mp);

            sigprof(c.ip(), c.sp(), c.lr(), gp, mp);

        }

        private static ptr<g> gFromTLS(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            switch (GOARCH)
            {
                case "arm": 
                    var tls = _addr_mp.tls[0L];
                    return _addr_((g.val)(@unsafe.Pointer(tls))).val!;
                    break;
                case "386": 

                case "amd64": 
                    tls = _addr_mp.tls[0L];
                    return _addr_((g.val)(@unsafe.Pointer(tls))).val!;
                    break;
            }
            throw("unsupported architecture");
            return _addr_null!;

        }

        private static uint profileloop1(System.UIntPtr param)
        {
            stdcall2(_SetThreadPriority, currentThread, _THREAD_PRIORITY_HIGHEST);

            while (true)
            {>>MARKER:FUNCTION_callbackasm1_BLOCK_PREFIX<<
                stdcall2(_WaitForSingleObject, profiletimer, _INFINITE);
                var first = (m.val)(atomic.Loadp(@unsafe.Pointer(_addr_allm)));
                {
                    var mp = first;

                    while (mp != null)
                    {>>MARKER:FUNCTION_profileloop_BLOCK_PREFIX<<
                        lock(_addr_mp.threadLock); 
                        // Do not profile threads blocked on Notes,
                        // this includes idle worker threads,
                        // idle timer thread, idle heap scavenger, etc.
                        if (mp.thread == 0L || mp.profilehz == 0L || mp.blocked)
                        {
                            unlock(_addr_mp.threadLock);
                            continue;
                        mp = mp.alllink;
                        } 
                        // Acquire our own handle to the thread.
                        ref System.UIntPtr thread = ref heap(out ptr<System.UIntPtr> _addr_thread);
                        stdcall7(_DuplicateHandle, currentProcess, mp.thread, currentProcess, uintptr(@unsafe.Pointer(_addr_thread)), 0L, 0L, _DUPLICATE_SAME_ACCESS);
                        unlock(_addr_mp.threadLock); 
                        // mp may exit between the DuplicateHandle
                        // above and the SuspendThread. The handle
                        // will remain valid, but SuspendThread may
                        // fail.
                        if (int32(stdcall1(_SuspendThread, thread)) == -1L)
                        { 
                            // The thread no longer exists.
                            stdcall1(_CloseHandle, thread);
                            continue;

                        }

                        if (mp.profilehz != 0L && !mp.blocked)
                        { 
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

        private static void setProcessCPUProfiler(int hz)
        {
            if (profiletimer == 0L)
            {
                var timer = stdcall3(_CreateWaitableTimerA, 0L, 0L, 0L);
                atomic.Storeuintptr(_addr_profiletimer, timer);
                var thread = stdcall6(_CreateThread, 0L, 0L, funcPC(profileloop), 0L, 0L, 0L);
                stdcall2(_SetThreadPriority, thread, _THREAD_PRIORITY_HIGHEST);
                stdcall1(_CloseHandle, thread);
            }

        }

        private static void setThreadCPUProfiler(int hz)
        {
            var ms = int32(0L);
            ref var due = ref heap(~int64(~uint64(1L << (int)(63L))), out ptr<var> _addr_due);
            if (hz > 0L)
            {
                ms = 1000L / hz;
                if (ms == 0L)
                {
                    ms = 1L;
                }

                due = int64(ms) * -10000L;

            }

            stdcall6(_SetWaitableTimer, profiletimer, uintptr(@unsafe.Pointer(_addr_due)), uintptr(ms), 0L, 0L, 0L);
            atomic.Store((uint32.val)(@unsafe.Pointer(_addr_getg().m.profilehz)), uint32(hz));

        }

        private static readonly var preemptMSupported = (var)GOARCH != "arm";

        // suspendLock protects simultaneous SuspendThread operations from
        // suspending each other.


        // suspendLock protects simultaneous SuspendThread operations from
        // suspending each other.
        private static mutex suspendLock = default;

        private static void preemptM(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            if (GOARCH == "arm")
            { 
                // TODO: Implement call injection
                return ;

            }

            if (mp == getg().m)
            {
                throw("self-preempt");
            } 

            // Synchronize with external code that may try to ExitProcess.
            if (!atomic.Cas(_addr_mp.preemptExtLock, 0L, 1L))
            { 
                // External code is running. Fail the preemption
                // attempt.
                atomic.Xadd(_addr_mp.preemptGen, 1L);
                return ;

            } 

            // Acquire our own handle to mp's thread.
            lock(_addr_mp.threadLock);
            if (mp.thread == 0L)
            { 
                // The M hasn't been minit'd yet (or was just unminit'd).
                unlock(_addr_mp.threadLock);
                atomic.Store(_addr_mp.preemptExtLock, 0L);
                atomic.Xadd(_addr_mp.preemptGen, 1L);
                return ;

            }

            ref System.UIntPtr thread = ref heap(out ptr<System.UIntPtr> _addr_thread);
            stdcall7(_DuplicateHandle, currentProcess, mp.thread, currentProcess, uintptr(@unsafe.Pointer(_addr_thread)), 0L, 0L, _DUPLICATE_SAME_ACCESS);
            unlock(_addr_mp.threadLock); 

            // Prepare thread context buffer. This must be aligned to 16 bytes.
            ptr<context> c;
            array<byte> cbuf = new array<byte>(@unsafe.Sizeof(c.val) + 15L);
            c = (context.val)(@unsafe.Pointer((uintptr(@unsafe.Pointer(_addr_cbuf[15L]))) & ~15L));
            c.contextflags = _CONTEXT_CONTROL; 

            // Serialize thread suspension. SuspendThread is asynchronous,
            // so it's otherwise possible for two threads to suspend each
            // other and deadlock. We must hold this lock until after
            // GetThreadContext, since that blocks until the thread is
            // actually suspended.
            lock(_addr_suspendLock); 

            // Suspend the thread.
            if (int32(stdcall1(_SuspendThread, thread)) == -1L)
            {
                unlock(_addr_suspendLock);
                stdcall1(_CloseHandle, thread);
                atomic.Store(_addr_mp.preemptExtLock, 0L); 
                // The thread no longer exists. This shouldn't be
                // possible, but just acknowledge the request.
                atomic.Xadd(_addr_mp.preemptGen, 1L);
                return ;

            } 

            // We have to be very careful between this point and once
            // we've shown mp is at an async safe-point. This is like a
            // signal handler in the sense that mp could have been doing
            // anything when we stopped it, including holding arbitrary
            // locks.

            // We have to get the thread context before inspecting the M
            // because SuspendThread only requests a suspend.
            // GetThreadContext actually blocks until it's suspended.
            stdcall2(_GetThreadContext, thread, uintptr(@unsafe.Pointer(c)));

            unlock(_addr_suspendLock); 

            // Does it want a preemption and is it safe to preempt?
            var gp = gFromTLS(_addr_mp);
            if (wantAsyncPreempt(gp))
            {
                {
                    var (ok, newpc) = isAsyncSafePoint(gp, c.ip(), c.sp(), c.lr());

                    if (ok)
                    { 
                        // Inject call to asyncPreempt
                        var targetPC = funcPC(asyncPreempt);
                        switch (GOARCH)
                        {
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

            atomic.Store(_addr_mp.preemptExtLock, 0L); 

            // Acknowledge the preemption.
            atomic.Xadd(_addr_mp.preemptGen, 1L);

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
        private static void osPreemptExtEnter(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            while (!atomic.Cas(_addr_mp.preemptExtLock, 0L, 1L))
            { 
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
 
            // Asynchronous preemption is now blocked.
        }

        // osPreemptExtExit is called after returning from external code that
        // may call ExitProcess.
        //
        // See osPreemptExtEnter for why this is nosplit.
        //
        //go:nosplit
        private static void osPreemptExtExit(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            atomic.Store(_addr_mp.preemptExtLock, 0L);
        }
    }
}
