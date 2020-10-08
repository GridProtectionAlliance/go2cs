// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 October 08 04:53:43 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\windows\dll_windows.go
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        // We need to use LoadLibrary and GetProcAddress from the Go runtime, because
        // the these symbols are loaded by the system linker and are required to
        // dynamically load additional symbols. Note that in the Go runtime, these
        // return syscall.Handle and syscall.Errno, but these are the same, in fact,
        // as windows.Handle and windows.Errno, and we intend to keep these the same.

        //go:linkname syscall_loadlibrary syscall.loadlibrary
        private static (Handle, Errno) syscall_loadlibrary(ptr<ushort> filename)
;

        //go:linkname syscall_getprocaddress syscall.getprocaddress
        private static (System.UIntPtr, Errno) syscall_getprocaddress(Handle handle, ptr<byte> procname)
;

        // DLLError describes reasons for DLL load failures.
        public partial struct DLLError
        {
            public error Err;
            public @string ObjName;
            public @string Msg;
        }

        private static @string Error(this ptr<DLLError> _addr_e)
        {
            ref DLLError e = ref _addr_e.val;

            return e.Msg;
        }

        // A DLL implements access to a single DLL.
        public partial struct DLL
        {
            public @string Name;
            public Handle Handle;
        }

        // LoadDLL loads DLL file into memory.
        //
        // Warning: using LoadDLL without an absolute path name is subject to
        // DLL preloading attacks. To safely load a system DLL, use LazyDLL
        // with System set to true, or use LoadLibraryEx directly.
        public static (ptr<DLL>, error) LoadDLL(@string name)
        {
            ptr<DLL> dll = default!;
            error err = default!;

            var (namep, err) = UTF16PtrFromString(name);
            if (err != null)
            {>>MARKER:FUNCTION_syscall_getprocaddress_BLOCK_PREFIX<<
                return (_addr_null!, error.As(err)!);
            }

            var (h, e) = syscall_loadlibrary(_addr_namep);
            if (e != 0L)
            {>>MARKER:FUNCTION_syscall_loadlibrary_BLOCK_PREFIX<<
                return (_addr_null!, error.As(addr(new DLLError(Err:e,ObjName:name,Msg:"Failed to load "+name+": "+e.Error(),))!)!);
            }

            ptr<DLL> d = addr(new DLL(Name:name,Handle:h,));
            return (_addr_d!, error.As(null!)!);

        }

        // MustLoadDLL is like LoadDLL but panics if load operation failes.
        public static ptr<DLL> MustLoadDLL(@string name) => func((_, panic, __) =>
        {
            var (d, e) = LoadDLL(name);
            if (e != null)
            {
                panic(e);
            }

            return _addr_d!;

        });

        // FindProc searches DLL d for procedure named name and returns *Proc
        // if found. It returns an error if search fails.
        private static (ptr<Proc>, error) FindProc(this ptr<DLL> _addr_d, @string name)
        {
            ptr<Proc> proc = default!;
            error err = default!;
            ref DLL d = ref _addr_d.val;

            var (namep, err) = BytePtrFromString(name);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var (a, e) = syscall_getprocaddress(d.Handle, _addr_namep);
            if (e != 0L)
            {
                return (_addr_null!, error.As(addr(new DLLError(Err:e,ObjName:name,Msg:"Failed to find "+name+" procedure in "+d.Name+": "+e.Error(),))!)!);
            }

            ptr<Proc> p = addr(new Proc(Dll:d,Name:name,addr:a,));
            return (_addr_p!, error.As(null!)!);

        }

        // MustFindProc is like FindProc but panics if search fails.
        private static ptr<Proc> MustFindProc(this ptr<DLL> _addr_d, @string name) => func((_, panic, __) =>
        {
            ref DLL d = ref _addr_d.val;

            var (p, e) = d.FindProc(name);
            if (e != null)
            {
                panic(e);
            }

            return _addr_p!;

        });

        // Release unloads DLL d from memory.
        private static error Release(this ptr<DLL> _addr_d)
        {
            error err = default!;
            ref DLL d = ref _addr_d.val;

            return error.As(FreeLibrary(d.Handle))!;
        }

        // A Proc implements access to a procedure inside a DLL.
        public partial struct Proc
        {
            public ptr<DLL> Dll;
            public @string Name;
            public System.UIntPtr addr;
        }

        // Addr returns the address of the procedure represented by p.
        // The return value can be passed to Syscall to run the procedure.
        private static System.UIntPtr Addr(this ptr<Proc> _addr_p)
        {
            ref Proc p = ref _addr_p.val;

            return p.addr;
        }

        //go:uintptrescapes

        // Call executes procedure p with arguments a. It will panic, if more than 15 arguments
        // are supplied.
        //
        // The returned error is always non-nil, constructed from the result of GetLastError.
        // Callers must inspect the primary return value to decide whether an error occurred
        // (according to the semantics of the specific function being called) before consulting
        // the error. The error will be guaranteed to contain windows.Errno.
        private static (System.UIntPtr, System.UIntPtr, error) Call(this ptr<Proc> _addr_p, params System.UIntPtr[] a) => func((_, panic, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            error lastErr = default!;
            a = a.Clone();
            ref Proc p = ref _addr_p.val;

            switch (len(a))
            {
                case 0L: 
                    return syscall.Syscall(p.Addr(), uintptr(len(a)), 0L, 0L, 0L);
                    break;
                case 1L: 
                    return syscall.Syscall(p.Addr(), uintptr(len(a)), a[0L], 0L, 0L);
                    break;
                case 2L: 
                    return syscall.Syscall(p.Addr(), uintptr(len(a)), a[0L], a[1L], 0L);
                    break;
                case 3L: 
                    return syscall.Syscall(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L]);
                    break;
                case 4L: 
                    return syscall.Syscall6(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], 0L, 0L);
                    break;
                case 5L: 
                    return syscall.Syscall6(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], 0L);
                    break;
                case 6L: 
                    return syscall.Syscall6(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L]);
                    break;
                case 7L: 
                    return syscall.Syscall9(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], 0L, 0L);
                    break;
                case 8L: 
                    return syscall.Syscall9(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], 0L);
                    break;
                case 9L: 
                    return syscall.Syscall9(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L]);
                    break;
                case 10L: 
                    return syscall.Syscall12(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], 0L, 0L);
                    break;
                case 11L: 
                    return syscall.Syscall12(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], 0L);
                    break;
                case 12L: 
                    return syscall.Syscall12(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], a[11L]);
                    break;
                case 13L: 
                    return syscall.Syscall15(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], a[11L], a[12L], 0L, 0L);
                    break;
                case 14L: 
                    return syscall.Syscall15(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], a[11L], a[12L], a[13L], 0L);
                    break;
                case 15L: 
                    return syscall.Syscall15(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], a[11L], a[12L], a[13L], a[14L]);
                    break;
                default: 
                    panic("Call " + p.Name + " with too many arguments " + itoa(len(a)) + ".");
                    break;
            }

        });

        // A LazyDLL implements access to a single DLL.
        // It will delay the load of the DLL until the first
        // call to its Handle method or to one of its
        // LazyProc's Addr method.
        public partial struct LazyDLL
        {
            public @string Name; // System determines whether the DLL must be loaded from the
// Windows System directory, bypassing the normal DLL search
// path.
            public bool System;
            public sync.Mutex mu;
            public ptr<DLL> dll; // non nil once DLL is loaded
        }

        // Load loads DLL file d.Name into memory. It returns an error if fails.
        // Load will not try to load DLL, if it is already loaded into memory.
        private static error Load(this ptr<LazyDLL> _addr_d) => func((defer, _, __) =>
        {
            ref LazyDLL d = ref _addr_d.val;
 
            // Non-racy version of:
            // if d.dll != nil {
            if (atomic.LoadPointer((@unsafe.Pointer.val)(@unsafe.Pointer(_addr_d.dll))) != null)
            {
                return error.As(null!)!;
            }

            d.mu.Lock();
            defer(d.mu.Unlock());
            if (d.dll != null)
            {
                return error.As(null!)!;
            } 

            // kernel32.dll is special, since it's where LoadLibraryEx comes from.
            // The kernel already special-cases its name, so it's always
            // loaded from system32.
            ptr<DLL> dll;
            error err = default!;
            if (d.Name == "kernel32.dll")
            {
                dll, err = LoadDLL(d.Name);
            }
            else
            {
                dll, err = loadLibraryEx(d.Name, d.System);
            }

            if (err != null)
            {
                return error.As(err)!;
            } 

            // Non-racy version of:
            // d.dll = dll
            atomic.StorePointer((@unsafe.Pointer.val)(@unsafe.Pointer(_addr_d.dll)), @unsafe.Pointer(dll));
            return error.As(null!)!;

        });

        // mustLoad is like Load but panics if search fails.
        private static void mustLoad(this ptr<LazyDLL> _addr_d) => func((_, panic, __) =>
        {
            ref LazyDLL d = ref _addr_d.val;

            var e = d.Load();
            if (e != null)
            {
                panic(e);
            }

        });

        // Handle returns d's module handle.
        private static System.UIntPtr Handle(this ptr<LazyDLL> _addr_d)
        {
            ref LazyDLL d = ref _addr_d.val;

            d.mustLoad();
            return uintptr(d.dll.Handle);
        }

        // NewProc returns a LazyProc for accessing the named procedure in the DLL d.
        private static ptr<LazyProc> NewProc(this ptr<LazyDLL> _addr_d, @string name)
        {
            ref LazyDLL d = ref _addr_d.val;

            return addr(new LazyProc(l:d,Name:name));
        }

        // NewLazyDLL creates new LazyDLL associated with DLL file.
        public static ptr<LazyDLL> NewLazyDLL(@string name)
        {
            return addr(new LazyDLL(Name:name));
        }

        // NewLazySystemDLL is like NewLazyDLL, but will only
        // search Windows System directory for the DLL if name is
        // a base name (like "advapi32.dll").
        public static ptr<LazyDLL> NewLazySystemDLL(@string name)
        {
            return addr(new LazyDLL(Name:name,System:true));
        }

        // A LazyProc implements access to a procedure inside a LazyDLL.
        // It delays the lookup until the Addr method is called.
        public partial struct LazyProc
        {
            public @string Name;
            public sync.Mutex mu;
            public ptr<LazyDLL> l;
            public ptr<Proc> proc;
        }

        // Find searches DLL for procedure named p.Name. It returns
        // an error if search fails. Find will not search procedure,
        // if it is already found and loaded into memory.
        private static error Find(this ptr<LazyProc> _addr_p) => func((defer, _, __) =>
        {
            ref LazyProc p = ref _addr_p.val;
 
            // Non-racy version of:
            // if p.proc == nil {
            if (atomic.LoadPointer((@unsafe.Pointer.val)(@unsafe.Pointer(_addr_p.proc))) == null)
            {
                p.mu.Lock();
                defer(p.mu.Unlock());
                if (p.proc == null)
                {
                    var e = p.l.Load();
                    if (e != null)
                    {
                        return error.As(e)!;
                    }

                    var (proc, e) = p.l.dll.FindProc(p.Name);
                    if (e != null)
                    {
                        return error.As(e)!;
                    } 
                    // Non-racy version of:
                    // p.proc = proc
                    atomic.StorePointer((@unsafe.Pointer.val)(@unsafe.Pointer(_addr_p.proc)), @unsafe.Pointer(proc));

                }

            }

            return error.As(null!)!;

        });

        // mustFind is like Find but panics if search fails.
        private static void mustFind(this ptr<LazyProc> _addr_p) => func((_, panic, __) =>
        {
            ref LazyProc p = ref _addr_p.val;

            var e = p.Find();
            if (e != null)
            {
                panic(e);
            }

        });

        // Addr returns the address of the procedure represented by p.
        // The return value can be passed to Syscall to run the procedure.
        // It will panic if the procedure cannot be found.
        private static System.UIntPtr Addr(this ptr<LazyProc> _addr_p)
        {
            ref LazyProc p = ref _addr_p.val;

            p.mustFind();
            return p.proc.Addr();
        }

        //go:uintptrescapes

        // Call executes procedure p with arguments a. It will panic, if more than 15 arguments
        // are supplied. It will also panic if the procedure cannot be found.
        //
        // The returned error is always non-nil, constructed from the result of GetLastError.
        // Callers must inspect the primary return value to decide whether an error occurred
        // (according to the semantics of the specific function being called) before consulting
        // the error. The error will be guaranteed to contain windows.Errno.
        private static (System.UIntPtr, System.UIntPtr, error) Call(this ptr<LazyProc> _addr_p, params System.UIntPtr[] a)
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            error lastErr = default!;
            a = a.Clone();
            ref LazyProc p = ref _addr_p.val;

            p.mustFind();
            return p.proc.Call(a);
        }

        private static var canDoSearchSystem32Once = default;

        private static void initCanDoSearchSystem32()
        { 
            // https://msdn.microsoft.com/en-us/library/ms684179(v=vs.85).aspx says:
            // "Windows 7, Windows Server 2008 R2, Windows Vista, and Windows
            // Server 2008: The LOAD_LIBRARY_SEARCH_* flags are available on
            // systems that have KB2533623 installed. To determine whether the
            // flags are available, use GetProcAddress to get the address of the
            // AddDllDirectory, RemoveDllDirectory, or SetDefaultDllDirectories
            // function. If GetProcAddress succeeds, the LOAD_LIBRARY_SEARCH_*
            // flags can be used with LoadLibraryEx."
            canDoSearchSystem32Once.v = (modkernel32.NewProc("AddDllDirectory").Find() == null);

        }

        private static bool canDoSearchSystem32()
        {
            canDoSearchSystem32Once.Do(initCanDoSearchSystem32);
            return canDoSearchSystem32Once.v;
        }

        private static bool isBaseName(@string name)
        {
            foreach (var (_, c) in name)
            {
                if (c == ':' || c == '/' || c == '\\')
                {
                    return false;
                }

            }
            return true;

        }

        // loadLibraryEx wraps the Windows LoadLibraryEx function.
        //
        // See https://msdn.microsoft.com/en-us/library/windows/desktop/ms684179(v=vs.85).aspx
        //
        // If name is not an absolute path, LoadLibraryEx searches for the DLL
        // in a variety of automatic locations unless constrained by flags.
        // See: https://msdn.microsoft.com/en-us/library/ff919712%28VS.85%29.aspx
        private static (ptr<DLL>, error) loadLibraryEx(@string name, bool system)
        {
            ptr<DLL> _p0 = default!;
            error _p0 = default!;

            var loadDLL = name;
            System.UIntPtr flags = default;
            if (system)
            {
                if (canDoSearchSystem32())
                {
                    const ulong LOAD_LIBRARY_SEARCH_SYSTEM32 = (ulong)0x00000800UL;

                    flags = LOAD_LIBRARY_SEARCH_SYSTEM32;
                }
                else if (isBaseName(name))
                { 
                    // WindowsXP or unpatched Windows machine
                    // trying to load "foo.dll" out of the system
                    // folder, but LoadLibraryEx doesn't support
                    // that yet on their system, so emulate it.
                    var (systemdir, err) = GetSystemDirectory();
                    if (err != null)
                    {
                        return (_addr_null!, error.As(err)!);
                    }

                    loadDLL = systemdir + "\\" + name;

                }

            }

            var (h, err) = LoadLibraryEx(loadDLL, 0L, flags);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (addr(new DLL(Name:name,Handle:h)), error.As(null!)!);

        }

        private partial struct errString // : @string
        {
        }

        private static @string Error(this errString s)
        {
            return string(s);
        }
    }
}}}}}}
