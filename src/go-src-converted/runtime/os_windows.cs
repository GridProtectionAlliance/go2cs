// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:10 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_windows.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        // TODO(brainman): should not need those
        private static readonly long _NSIG = 65L;

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
        //go:cgo_import_dynamic runtime._GetQueuedCompletionStatus GetQueuedCompletionStatus%5 "kernel32.dll"
        //go:cgo_import_dynamic runtime._GetStdHandle GetStdHandle%1 "kernel32.dll"
        //go:cgo_import_dynamic runtime._GetSystemInfo GetSystemInfo%1 "kernel32.dll"
        //go:cgo_import_dynamic runtime._GetThreadContext GetThreadContext%2 "kernel32.dll"
        //go:cgo_import_dynamic runtime._LoadLibraryW LoadLibraryW%1 "kernel32.dll"
        //go:cgo_import_dynamic runtime._LoadLibraryA LoadLibraryA%1 "kernel32.dll"
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
        //go:cgo_import_dynamic runtime._VirtualAlloc VirtualAlloc%4 "kernel32.dll"
        //go:cgo_import_dynamic runtime._VirtualFree VirtualFree%3 "kernel32.dll"
        //go:cgo_import_dynamic runtime._WSAGetOverlappedResult WSAGetOverlappedResult%5 "ws2_32.dll"
        //go:cgo_import_dynamic runtime._WaitForSingleObject WaitForSingleObject%2 "kernel32.dll"
        //go:cgo_import_dynamic runtime._WriteConsoleW WriteConsoleW%5 "kernel32.dll"
        //go:cgo_import_dynamic runtime._WriteFile WriteFile%5 "kernel32.dll"
        //go:cgo_import_dynamic runtime._timeBeginPeriod timeBeginPeriod%1 "winmm.dll"
        //go:cgo_import_dynamic runtime._timeEndPeriod timeEndPeriod%1 "winmm.dll"

        private partial struct stdFunction // : unsafe.Pointer
        {
        }

 
        // Following syscalls are available on every Windows PC.
        // All these variables are set by the Windows executable
        // loader before the Go program starts.
        private static stdFunction _AddVectoredExceptionHandler = default;        private static stdFunction _CloseHandle = default;        private static stdFunction _CreateEventA = default;        private static stdFunction _CreateIoCompletionPort = default;        private static stdFunction _CreateThread = default;        private static stdFunction _CreateWaitableTimerA = default;        private static stdFunction _DuplicateHandle = default;        private static stdFunction _ExitProcess = default;        private static stdFunction _FreeEnvironmentStringsW = default;        private static stdFunction _GetConsoleMode = default;        private static stdFunction _GetEnvironmentStringsW = default;        private static stdFunction _GetProcAddress = default;        private static stdFunction _GetProcessAffinityMask = default;        private static stdFunction _GetQueuedCompletionStatus = default;        private static stdFunction _GetStdHandle = default;        private static stdFunction _GetSystemInfo = default;        private static stdFunction _GetSystemTimeAsFileTime = default;        private static stdFunction _GetThreadContext = default;        private static stdFunction _LoadLibraryW = default;        private static stdFunction _LoadLibraryA = default;        private static stdFunction _QueryPerformanceCounter = default;        private static stdFunction _QueryPerformanceFrequency = default;        private static stdFunction _ResumeThread = default;        private static stdFunction _SetConsoleCtrlHandler = default;        private static stdFunction _SetErrorMode = default;        private static stdFunction _SetEvent = default;        private static stdFunction _SetProcessPriorityBoost = default;        private static stdFunction _SetThreadPriority = default;        private static stdFunction _SetUnhandledExceptionFilter = default;        private static stdFunction _SetWaitableTimer = default;        private static stdFunction _SuspendThread = default;        private static stdFunction _SwitchToThread = default;        private static stdFunction _VirtualAlloc = default;        private static stdFunction _VirtualFree = default;        private static stdFunction _WSAGetOverlappedResult = default;        private static stdFunction _WaitForSingleObject = default;        private static stdFunction _WriteConsoleW = default;        private static stdFunction _WriteFile = default;        private static stdFunction _timeBeginPeriod = default;        private static stdFunction _timeEndPeriod = default;        private static stdFunction _ = default; 

        // Following syscalls are only available on some Windows PCs.
        // We will load syscalls, if available, before using them.
        private static stdFunction _AddDllDirectory = default;        private static stdFunction _AddVectoredContinueHandler = default;        private static stdFunction _GetQueuedCompletionStatusEx = default;        private static stdFunction _LoadLibraryExW = default;        private static stdFunction _ = default; 

        // Use RtlGenRandom to generate cryptographically random data.
        // This approach has been recommended by Microsoft (see issue
        // 15589 for details).
        // The RtlGenRandom is not listed in advapi32.dll, instead
        // RtlGenRandom function can be found by searching for SystemFunction036.
        // Also some versions of Mingw cannot link to SystemFunction036
        // when building executable as Cgo. So load SystemFunction036
        // manually during runtime startup.
        private static stdFunction _RtlGenRandom = default;        private static stdFunction _NtWaitForSingleObject = default;

        // Function to be called by windows CreateThread
        // to start new os thread.
        private static uint tstart_stdcall(ref m newm)
;

        private static uint ctrlhandler(uint _type)
;

        private partial struct mOS
        {
            public System.UIntPtr waitsema; // semaphore for parking on locks
        }

        //go:linkname os_sigpipe os.sigpipe
        private static void os_sigpipe()
        {
            throw("too many writes on closed pipe");
        }

        // Stubs so tests can link correctly. These should never be called.
        private static int open(ref byte name, int mode, int perm)
        {
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
            var f = stdcall2(_GetProcAddress, lib, uintptr(@unsafe.Pointer(ref name[0L])));
            return stdFunction(@unsafe.Pointer(f));
        }

        private static void loadOptionalSyscalls()
        {
            slice<byte> kernel32dll = (slice<byte>)"kernel32.dll ";
            var k32 = stdcall1(_LoadLibraryA, uintptr(@unsafe.Pointer(ref kernel32dll[0L])));
            if (k32 == 0L)
            {>>MARKER:FUNCTION_ctrlhandler_BLOCK_PREFIX<<
                throw("kernel32.dll not found");
            }
            _AddDllDirectory = windowsFindfunc(k32, (slice<byte>)"AddDllDirectory ");
            _AddVectoredContinueHandler = windowsFindfunc(k32, (slice<byte>)"AddVectoredContinueHandler ");
            _GetQueuedCompletionStatusEx = windowsFindfunc(k32, (slice<byte>)"GetQueuedCompletionStatusEx ");
            _LoadLibraryExW = windowsFindfunc(k32, (slice<byte>)"LoadLibraryExW ");

            slice<byte> advapi32dll = (slice<byte>)"advapi32.dll ";
            var a32 = stdcall1(_LoadLibraryA, uintptr(@unsafe.Pointer(ref advapi32dll[0L])));
            if (a32 == 0L)
            {>>MARKER:FUNCTION_tstart_stdcall_BLOCK_PREFIX<<
                throw("advapi32.dll not found");
            }
            _RtlGenRandom = windowsFindfunc(a32, (slice<byte>)"SystemFunction036 ");

            slice<byte> ntdll = (slice<byte>)"ntdll.dll ";
            var n32 = stdcall1(_LoadLibraryA, uintptr(@unsafe.Pointer(ref ntdll[0L])));
            if (n32 == 0L)
            {
                throw("ntdll.dll not found");
            }
            _NtWaitForSingleObject = windowsFindfunc(n32, (slice<byte>)"NtWaitForSingleObject ");

            if (windowsFindfunc(n32, (slice<byte>)"wine_get_version ") != null)
            { 
                // running on Wine
                initWine(k32);
            }
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
            System.UIntPtr mask = default;            System.UIntPtr sysmask = default;

            var ret = stdcall3(_GetProcessAffinityMask, currentProcess, uintptr(@unsafe.Pointer(ref mask)), uintptr(@unsafe.Pointer(ref sysmask)));
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
            systeminfo info = default;
            stdcall1(_GetSystemInfo, uintptr(@unsafe.Pointer(ref info)));
            return int32(info.dwnumberofprocessors);
        }

        private static System.UIntPtr getPageSize()
        {
            systeminfo info = default;
            stdcall1(_GetSystemInfo, uintptr(@unsafe.Pointer(ref info)));
            return uintptr(info.dwpagesize);
        }

        private static readonly var currentProcess = ~uintptr(0L); // -1 = current process
        private static readonly var currentThread = ~uintptr(1L); // -2 = current thread

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
        private static readonly long osRelaxMinNS = 60L * 1e6F;

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

            useLoadLibraryEx = (_LoadLibraryExW != null && _AddDllDirectory != null);

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

        private static long nanotime()
;

        // useQPCTime controls whether time.now and nanotime use QueryPerformanceCounter.
        // This is only set to 1 when running under Wine.
        private static byte useQPCTime = default;

        private static long qpcStartCounter = default;
        private static long qpcMultiplier = default;

        //go:nosplit
        private static long nanotimeQPC()
        {
            long counter = 0L;
            stdcall1(_QueryPerformanceCounter, uintptr(@unsafe.Pointer(ref counter))); 

            // returns number of nanoseconds
            return (counter - qpcStartCounter) * qpcMultiplier;
        }

        //go:nosplit
        private static (long, int, long) nowQPC()
        {
            long ft = default;
            stdcall1(_GetSystemTimeAsFileTime, uintptr(@unsafe.Pointer(ref ft)));

            var t = (ft - 116444736000000000L) * 100L;

            sec = t / 1000000000L;
            nsec = int32(t - sec * 1000000000L);

            mono = nanotimeQPC();
            return;
        }

        private static void initWine(System.UIntPtr k32)
        {
            _GetSystemTimeAsFileTime = windowsFindfunc(k32, (slice<byte>)"GetSystemTimeAsFileTime ");
            if (_GetSystemTimeAsFileTime == null)
            {>>MARKER:FUNCTION_nanotime_BLOCK_PREFIX<<
                throw("could not find GetSystemTimeAsFileTime() syscall");
            }
            _QueryPerformanceCounter = windowsFindfunc(k32, (slice<byte>)"QueryPerformanceCounter ");
            _QueryPerformanceFrequency = windowsFindfunc(k32, (slice<byte>)"QueryPerformanceFrequency ");
            if (_QueryPerformanceCounter == null || _QueryPerformanceFrequency == null)
            {>>MARKER:FUNCTION_externalthreadhandler_BLOCK_PREFIX<<
                throw("could not find QPC syscalls");
            } 

            // We can not simply fallback to GetSystemTimeAsFileTime() syscall, since its time is not monotonic,
            // instead we use QueryPerformanceCounter family of syscalls to implement monotonic timer
            // https://msdn.microsoft.com/en-us/library/windows/desktop/dn553408(v=vs.85).aspx
            long tmp = default;
            stdcall1(_QueryPerformanceFrequency, uintptr(@unsafe.Pointer(ref tmp)));
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
            stdcall1(_QueryPerformanceCounter, uintptr(@unsafe.Pointer(ref qpcStartCounter))); 

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
            if (stdcall2(_RtlGenRandom, uintptr(@unsafe.Pointer(ref r[0L])), uintptr(len(r))) & 0xffUL != 0L)
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
            ref array<ushort> p = new ptr<ref array<ushort>>(strings)[..];

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
                    envs[i] = gostringw(ref p[0L]);
                    while (p[0L] != 0L)
                    {
                        p = p[1L..];
                    }

                    p = p[1L..]; // skip nil byte
                }

                i = i__prev1;
            }

            stdcall1(_FreeEnvironmentStringsW, uintptr(strings));
        }

        // exiting is set to non-zero when the process is exiting.
        private static uint exiting = default;

        //go:nosplit
        private static void exit(int code)
        {
            atomic.Store(ref exiting, 1L);
            stdcall1(_ExitProcess, uintptr(code));
        }

        //go:nosplit
        private static int write(System.UIntPtr fd, unsafe.Pointer buf, int n)
        {
            const var _STD_OUTPUT_HANDLE = ~uintptr(10L); // -11
            const var _STD_ERROR_HANDLE = ~uintptr(11L); // -12
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
            ref array<byte> b = new ptr<ref array<byte>>(buf)[..n];
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
                uint m = default;
                var isConsole = stdcall2(_GetConsoleMode, handle, uintptr(@unsafe.Pointer(ref m))) != 0L; 
                // If this is a console output, various non-unicode code pages can be in use.
                // Use the dedicated WriteConsole call to ensure unicode is printed correctly.
                if (isConsole)
                {
                    return int32(writeConsole(handle, buf, n));
                }
            }
            uint written = default;
            stdcall5(_WriteFile, handle, uintptr(buf), uintptr(n), uintptr(@unsafe.Pointer(ref written)), 0L);
            return int32(written);
        }

        private static array<ushort> utf16ConsoleBack = new array<ushort>(1000L);        private static mutex utf16ConsoleBackLock = default;

        // writeConsole writes bufLen bytes from buf to the console File.
        // It returns the number of bytes written.
        private static long writeConsole(System.UIntPtr handle, unsafe.Pointer buf, int bufLen)
        {
            const var surr2 = (surrogateMin + surrogateMax + 1L) / 2L; 

            // Do not use defer for unlock. May cause issues when printing a panic.
 

            // Do not use defer for unlock. May cause issues when printing a panic.
            lock(ref utf16ConsoleBackLock);

            ref array<byte> b = new ptr<ref array<byte>>(buf)[..bufLen];
            *(*@string) s = @unsafe.Pointer(ref b).Value;

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
            unlock(ref utf16ConsoleBackLock);
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
                return;
            }
            uint written = default;
            stdcall5(_WriteConsoleW, handle, uintptr(@unsafe.Pointer(ref b[0L])), uintptr(l), uintptr(@unsafe.Pointer(ref written)), 0L);
            return;
        }

        //go:nosplit
        private static int semasleep(long ns)
        {
            const ulong _WAIT_ABANDONED = 0x00000080UL;
            const ulong _WAIT_OBJECT_0 = 0x00000000UL;
            const ulong _WAIT_TIMEOUT = 0x00000102UL;
            const ulong _WAIT_FAILED = 0xFFFFFFFFUL; 

            // store ms in ns to save stack space
            if (ns < 0L)
            {
                ns = _INFINITE;
            }
            else
            {
                ns = int64(timediv(ns, 1000000L, null));
                if (ns == 0L)
                {
                    ns = 1L;
                }
            }
            var result = stdcall2(_WaitForSingleObject, getg().m.waitsema, uintptr(ns));

            if (result == _WAIT_OBJECT_0) //signaled
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
        private static void semawakeup(ref m mp)
        {
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
        private static void semacreate(ref m mp)
        {
            if (mp.waitsema != 0L)
            {
                return;
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
        }

        // May run with m.p==nil, so write barriers are not allowed. This
        // function is called by newosproc0, so it is also required to
        // operate without stack guards.
        //go:nowritebarrierrec
        //go:nosplit
        private static void newosproc(ref m mp, unsafe.Pointer stk)
        {
            const ulong _STACK_SIZE_PARAM_IS_A_RESERVATION = 0x00010000UL; 
            // stackSize must match SizeOfStackReserve in cmd/link/internal/ld/pe.go.
 
            // stackSize must match SizeOfStackReserve in cmd/link/internal/ld/pe.go.
            const ulong stackSize = 0x00200000UL * _64bit + 0x00100000UL * (1L - _64bit);

            var thandle = stdcall6(_CreateThread, 0L, stackSize, funcPC(tstart_stdcall), uintptr(@unsafe.Pointer(mp)), _STACK_SIZE_PARAM_IS_A_RESERVATION, 0L);

            if (thandle == 0L)
            {
                if (atomic.Load(ref exiting) != 0L)
                { 
                    // CreateThread may fail if called
                    // concurrently with ExitProcess. If this
                    // happens, just freeze this thread and let
                    // the process exit. See issue #18253.
                    lock(ref deadlock);
                    lock(ref deadlock);
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
        private static void newosproc0(ref m mp, unsafe.Pointer stk)
        {
            newosproc(mp, stk);
        }

        private static void exitThread(ref uint wait)
        { 
            // We should never reach exitThread on Windows because we let
            // the OS clean up threads.
            throw("exitThread");
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the parent thread (main thread in case of bootstrap), can allocate memory.
        private static void mpreinit(ref m mp)
        {
        }

        //go:nosplit
        private static void msigsave(ref m mp)
        {
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
            System.UIntPtr thandle = default;
            stdcall7(_DuplicateHandle, currentProcess, currentThread, currentProcess, uintptr(@unsafe.Pointer(ref thandle)), 0L, 0L, _DUPLICATE_SAME_ACCESS);
            atomic.Storeuintptr(ref getg().m.thread, thandle);
        }

        // Called from dropm to undo the effect of an minit.
        //go:nosplit
        private static void unminit()
        {
            var tp = ref getg().m.thread;
            stdcall1(_CloseHandle, tp.Value);
            tp.Value = 0L;
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

            if (mp.profilehz != 0L)
            { 
                // leave pc/sp for cpu profiler
                mp.libcallg.set(gp);
                mp.libcallpc = getcallerpc(); 
                // sp must be the last, because once async cpu profiler finds
                // all three values to be non-zero, it will use them
                mp.libcallsp = getcallersp(@unsafe.Pointer(ref fn));
            }
            asmcgocall(asmstdcallAddr, @unsafe.Pointer(ref mp.libcall));
            mp.libcallsp = 0L;
            return mp.libcall.r1;
        }

        //go:nosplit
        private static System.UIntPtr stdcall0(stdFunction fn)
        {
            var mp = getg().m;
            mp.libcall.n = 0L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(ref fn))); // it's unused but must be non-nil, otherwise crashes
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall1(stdFunction fn, System.UIntPtr a0)
        {
            var mp = getg().m;
            mp.libcall.n = 1L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(ref a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall2(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1)
        {
            var mp = getg().m;
            mp.libcall.n = 2L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(ref a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall3(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2)
        {
            var mp = getg().m;
            mp.libcall.n = 3L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(ref a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall4(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            var mp = getg().m;
            mp.libcall.n = 4L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(ref a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall5(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4)
        {
            var mp = getg().m;
            mp.libcall.n = 5L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(ref a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall6(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
        {
            var mp = getg().m;
            mp.libcall.n = 6L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(ref a0)));
            return stdcall(fn);
        }

        //go:nosplit
        private static System.UIntPtr stdcall7(stdFunction fn, System.UIntPtr a0, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            var mp = getg().m;
            mp.libcall.n = 7L;
            mp.libcall.args = uintptr(noescape(@unsafe.Pointer(ref a0)));
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

        private static uint ctrlhandler1(uint _type)
        {
            uint s = default;


            if (_type == _CTRL_C_EVENT || _type == _CTRL_BREAK_EVENT) 
                s = _SIGINT;
            else 
                return 0L;
                        if (sigsend(s))
            {>>MARKER:FUNCTION_switchtothread_BLOCK_PREFIX<<
                return 1L;
            }
            exit(2L); // SIGINT, SIGTERM, etc
            return 0L;
        }

        // in sys_windows_386.s and sys_windows_amd64.s
        private static void profileloop()
;

        private static System.UIntPtr profiletimer = default;

        private static void profilem(ref m mp)
        {
            ref context r = default;
            var rbuf = make_slice<byte>(@unsafe.Sizeof(r.Value) + 15L);

            var tls = ref mp.tls[0L];
            var gp = ((g.Value.Value)(@unsafe.Pointer(tls))).Value; 

            // align Context to 16 bytes
            r = (context.Value)(@unsafe.Pointer((uintptr(@unsafe.Pointer(ref rbuf[15L]))) & ~15L));
            r.contextflags = _CONTEXT_CONTROL;
            stdcall2(_GetThreadContext, mp.thread, uintptr(@unsafe.Pointer(r)));
            sigprof(r.ip(), r.sp(), 0L, gp, mp);
        }

        private static uint profileloop1(System.UIntPtr param)
        {
            stdcall2(_SetThreadPriority, currentThread, _THREAD_PRIORITY_HIGHEST);

            while (true)
            {>>MARKER:FUNCTION_profileloop_BLOCK_PREFIX<<
                stdcall2(_WaitForSingleObject, profiletimer, _INFINITE);
                var first = (m.Value)(atomic.Loadp(@unsafe.Pointer(ref allm)));
                {
                    var mp = first;

                    while (mp != null)
                    {>>MARKER:FUNCTION_usleep2_BLOCK_PREFIX<<
                        var thread = atomic.Loaduintptr(ref mp.thread); 
                        // Do not profile threads blocked on Notes,
                        // this includes idle worker threads,
                        // idle timer thread, idle heap scavenger, etc.
                        if (thread == 0L || mp.profilehz == 0L || mp.blocked)
                        {>>MARKER:FUNCTION_onosstack_BLOCK_PREFIX<<
                            continue;
                        mp = mp.alllink;
                        }
                        stdcall1(_SuspendThread, thread);
                        if (mp.profilehz != 0L && !mp.blocked)
                        {
                            profilem(mp);
                        }
                        stdcall1(_ResumeThread, thread);
                    }

                }
            }

        }

        private static void setProcessCPUProfiler(int hz)
        {
            if (profiletimer == 0L)
            {
                var timer = stdcall3(_CreateWaitableTimerA, 0L, 0L, 0L);
                atomic.Storeuintptr(ref profiletimer, timer);
                var thread = stdcall6(_CreateThread, 0L, 0L, funcPC(profileloop), 0L, 0L, 0L);
                stdcall2(_SetThreadPriority, thread, _THREAD_PRIORITY_HIGHEST);
                stdcall1(_CloseHandle, thread);
            }
        }

        private static void setThreadCPUProfiler(int hz)
        {
            var ms = int32(0L);
            var due = ~int64(~uint64(1L << (int)(63L)));
            if (hz > 0L)
            {
                ms = 1000L / hz;
                if (ms == 0L)
                {
                    ms = 1L;
                }
                due = int64(ms) * -10000L;
            }
            stdcall6(_SetWaitableTimer, profiletimer, uintptr(@unsafe.Pointer(ref due)), uintptr(ms), 0L, 0L, 0L);
            atomic.Store((uint32.Value)(@unsafe.Pointer(ref getg().m.profilehz)), uint32(hz));
        }

        private static System.UIntPtr memlimit()
        {
            return 0L;
        }
    }
}
