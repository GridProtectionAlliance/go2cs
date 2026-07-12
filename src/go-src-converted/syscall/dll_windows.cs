// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted dll_windows.go output).
// Two parts of this file cannot work as literally converted:
//
//  1. The Syscall/SyscallN trampolines and loadlibrary/getprocaddress are runtime-provided
//     assembly (../runtime/syscall_windows.go), emitted as bodyless partials that the
//     PartialStubGenerator turns into throwing stubs. Here they are real: LoadLibraryExW /
//     GetProcAddress via P/Invoke, and an unmanaged function-pointer calli dispatch for the
//     system-call trampolines (Windows x64 has a single calling convention, so a switch on
//     argument count covers every arity).
//  2. LazyDLL.Load / LazyProc.Find use Go's non-racy pattern of atomically reading a pointer
//     FIELD through a raw address — atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&d.dll)))
//     — a managed-referent uintptr round-trip that cannot alias a C# object-reference field
//     (see docs/Baseline-vs-FullConversion.md, the S1 fork). Here the lazy init is a plain
//     double-checked lock; CLR reference assignment is atomic, so readers never see a torn value.
//
// Soundness note (inherited, documented): argument uintptrs captured by the converted zsyscall
// wrappers (e.g. uintptr(unsafe.Pointer(&buf[0]))) are TRANSIENT addresses — golib's ж→uintptr
// conversion cannot pin across the call. The window between capture and the calli below is
// short and allocation-free, but a compacting GC move in that window would invalidate them;
// the long-term fix is pinning at the capture seam.

using System;
using System.Runtime.InteropServices;

// Hand-owned native replacement of the converted dll_windows.go output — the converter skips
// regenerating a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

using sysdll = @internal.syscall.windows.sysdll_package;
using Δsync = sync_package;
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class syscall_package {

// DLLError describes reasons for DLL load failures.
[GoType] partial struct DLLError {
    public error Err;
    public @string ObjName;
    public @string Msg;
}

[GoRecv] public static @string Error(this ref DLLError e) {
    return e.Msg;
}

[GoRecv] public static error Unwrap(this ref DLLError e) {
    return e.Err;
}

// ---- native Win32 entry points (the runtime-provided assembly in Go) ----

private const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;

[DllImport("kernel32.dll", EntryPoint = "LoadLibraryExW", CharSet = CharSet.Unicode, SetLastError = true)]
private static extern IntPtr win32LoadLibraryEx(string lpLibFileName, IntPtr hFile, uint dwFlags);

[DllImport("kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
private static extern IntPtr win32GetProcAddress(IntPtr hModule, string lpProcName);

// The system-call trampoline: invoke the native function at trap with the given uintptr
// arguments. Mirrors runtime.syscall_syscalln: r2 is only meaningful for floating-point
// returns (always 0 here) and err is GetLastError after the call (cleared first, so a
// succeeding API that does not touch last-error reports 0, matching Go).
private static unsafe (uintptr r1, uintptr r2, Errno err) syscalln(nuint fn, ReadOnlySpan<uintptr> a) {
    nuint r;

    Marshal.SetLastSystemError(0);

    switch (a.Length) {
        case 0: r = ((delegate* unmanaged<nuint>)fn)(); break;
        case 1: r = ((delegate* unmanaged<nuint, nuint>)fn)(a[0]); break;
        case 2: r = ((delegate* unmanaged<nuint, nuint, nuint>)fn)(a[0], a[1]); break;
        case 3: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2]); break;
        case 4: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3]); break;
        case 5: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4]); break;
        case 6: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5]); break;
        case 7: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6]); break;
        case 8: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7]); break;
        case 9: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8]); break;
        case 10: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9]); break;
        case 11: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10]); break;
        case 12: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11]); break;
        case 13: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12]); break;
        case 14: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13]); break;
        case 15: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14]); break;
        case 16: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15]); break;
        case 17: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15], a[16]); break;
        case 18: r = ((delegate* unmanaged<nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint, nuint>)fn)(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15], a[16], a[17]); break;
        default: throw new ArgumentOutOfRangeException(nameof(a), "too many syscall arguments");
    }

    return (r, 0, (Errno)(uint32)Marshal.GetLastSystemError());
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3) {
    return SyscallN(trap, a1, a2, a3);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall6(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall9(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6, a7, a8, a9);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall12(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall15(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12, uintptr a13, uintptr a14, uintptr a15) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
}

// Deprecated: Use [SyscallN] instead.
public static (uintptr r1, uintptr r2, Errno err) Syscall18(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12, uintptr a13, uintptr a14, uintptr a15, uintptr a16, uintptr a17, uintptr a18) {
    return SyscallN(trap, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17, a18);
}

public static (uintptr r1, uintptr r2, Errno err) SyscallN(uintptr trap, params ꓸꓸꓸuintptr argsʗp) {
    return syscalln(trap, argsʗp);
}

// A DLL implements access to a single DLL.
[GoType] partial struct DLL {
    public @string Name;
    public ΔHandle Handle;
}

// LoadDLL loads the named DLL file into memory.
//
// If name is not an absolute path and is not a known system DLL used by
// Go, Windows will search for the named DLL in many locations, causing
// potential DLL preloading attacks.
//
// Use [LazyDLL] in golang.org/x/sys/windows for a secure way to
// load system DLLs.
public static (ж<DLL>, error) LoadDLL(@string name) {
    // A known system DLL is loaded from System32 only (LOAD_LIBRARY_SEARCH_SYSTEM32),
    // mirroring Go's loadsystemlibrary; anything else takes the default search order,
    // mirroring loadlibrary.
    uint flags = sysdll.IsSystemDLL[name] ? LOAD_LIBRARY_SEARCH_SYSTEM32 : 0u;
    IntPtr h = win32LoadLibraryEx(name.ToString(), IntPtr.Zero, flags);

    if (h == IntPtr.Zero) {
        ref var e = ref heap<Errno>(out var _);
        e = (Errno)(uint32)Marshal.GetLastSystemError();
        return (default!, new DLLErrorжerror(Ꮡ(new DLLError(
            Err: e,
            ObjName: name,
            Msg: "Failed to load "u8 + name + ": "u8 + e.Error()
        ))));
    }

    var d = Ꮡ(new DLL(
        Name: name,
        Handle: (ΔHandle)(uintptr)(nuint)h
    ));
    return (d, default!);
}

// MustLoadDLL is like [LoadDLL] but panics if load operation fails.
public static ж<DLL> MustLoadDLL(@string name) {
    var (d, e) = LoadDLL(name);
    if (e != default!) {
        throw panic(e);
    }
    return d;
}

// FindProc searches [DLL] d for procedure named name and returns [*Proc]
// if found. It returns an error if search fails.
public static (ж<Proc> proc, error err) FindProc(this ж<DLL> Ꮡd, @string name) {
    ref var d = ref Ꮡd.Value;

    IntPtr addr = win32GetProcAddress((IntPtr)(nint)(nuint)(uintptr)d.Handle, name.ToString());

    if (addr == IntPtr.Zero) {
        ref var e = ref heap<Errno>(out var _);
        e = (Errno)(uint32)Marshal.GetLastSystemError();
        return (default!, new DLLErrorжerror(Ꮡ(new DLLError(
            Err: e,
            ObjName: name,
            Msg: "Failed to find "u8 + name + " procedure in "u8 + d.Name + ": "u8 + e.Error()
        ))));
    }

    var p = Ꮡ(new Proc(
        Dll: Ꮡd,
        Name: name,
        addr: (uintptr)(nuint)addr
    ));
    return (p, default!);
}

// MustFindProc is like [DLL.FindProc] but panics if search fails.
public static ж<Proc> MustFindProc(this ж<DLL> Ꮡd, @string name) {
    var (p, e) = Ꮡd.FindProc(name);
    if (e != default!) {
        throw panic(e);
    }
    return p;
}

// Release unloads [DLL] d from memory.
[GoRecv] public static error /*err*/ Release(this ref DLL d) {
    return FreeLibrary(d.Handle);
}

// A Proc implements access to a procedure inside a [DLL].
[GoType] partial struct Proc {
    public ж<DLL> Dll;
    public @string Name;
    internal uintptr addr;
}

// Addr returns the address of the procedure represented by p.
// The return value can be passed to Syscall to run the procedure.
[GoRecv] public static uintptr Addr(this ref Proc p) {
    return p.addr;
}

// Call executes procedure p with arguments a.
//
// The returned error is always non-nil, constructed from the result of GetLastError.
// Callers must inspect the primary return value to decide whether an error occurred
// (according to the semantics of the specific function being called) before consulting
// the error. The error always has type [Errno].
[GoRecv] public static (uintptr, uintptr, error) Call(this ref Proc p, params ꓸꓸꓸuintptr aʗp) {
    var (r1, r2, err) = SyscallN(p.Addr(), aʗp);
    return (r1, r2, err);
}

// A LazyDLL implements access to a single [DLL].
// It will delay the load of the DLL until the first
// call to its [LazyDLL.Handle] method or to one of its
// [LazyProc]'s Addr method.
//
// LazyDLL is subject to the same DLL preloading attacks as documented
// on [LoadDLL].
//
// Use LazyDLL in golang.org/x/sys/windows for a secure way to
// load system DLLs.
[GoType] partial struct LazyDLL {
    internal Δsync.Mutex mu;
    internal ж<DLL> dll; // non nil once DLL is loaded
    public @string Name;
}

// Load loads DLL file d.Name into memory. It returns an error if fails.
// Load will not try to load DLL, if it is already loaded into memory.
public static error Load(this ж<LazyDLL> Ꮡd) {
    ref var d = ref Ꮡd.Value;

    // CLR object-reference reads/writes are atomic, so the unlocked fast path never sees a
    // torn value (Go's version reads the field via atomic.LoadPointer for the same reason).
    if (d.dll != nil) {
        return default!;
    }

    Ꮡd.of(LazyDLL.Ꮡmu).Lock();
    try {
        if (d.dll == nil) {
            var (dll, e) = LoadDLL(d.Name);
            if (e != default!) {
                return e;
            }
            d.dll = dll;
        }
    }
    finally {
        Ꮡd.of(LazyDLL.Ꮡmu).Unlock();
    }

    return default!;
}

// mustLoad is like Load but panics if search fails.
internal static void mustLoad(this ж<LazyDLL> Ꮡd) {
    var e = Ꮡd.Load();
    if (e != default!) {
        throw panic(e);
    }
}

// Handle returns d's module handle.
public static uintptr Handle(this ж<LazyDLL> Ꮡd) {
    ref var d = ref Ꮡd.Value;

    Ꮡd.mustLoad();
    return (uintptr)(~d.dll).Handle;
}

// NewProc returns a [LazyProc] for accessing the named procedure in the [DLL] d.
public static ж<LazyProc> NewProc(this ж<LazyDLL> Ꮡd, @string name) {
    return Ꮡ(new LazyProc(l: Ꮡd, Name: name));
}

// NewLazyDLL creates new [LazyDLL] associated with [DLL] file.
public static ж<LazyDLL> NewLazyDLL(@string name) {
    return Ꮡ(new LazyDLL(Name: name));
}

// A LazyProc implements access to a procedure inside a [LazyDLL].
// It delays the lookup until the [LazyProc.Addr], [LazyProc.Call], or [LazyProc.Find] method is called.
[GoType] partial struct LazyProc {
    internal Δsync.Mutex mu;
    public @string Name;
    internal ж<LazyDLL> l;
    internal ж<Proc> proc;
}

// Find searches [DLL] for procedure named p.Name. It returns
// an error if search fails. Find will not search procedure,
// if it is already found and loaded into memory.
public static error Find(this ж<LazyProc> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    // Same atomic-reference fast path as LazyDLL.Load above.
    if (p.proc != nil) {
        return default!;
    }

    Ꮡp.of(LazyProc.Ꮡmu).Lock();
    try {
        if (p.proc == nil) {
            var e = p.l.Load();
            if (e != default!) {
                return e;
            }
            var (proc, eΔ1) = (~p.l).dll.FindProc(p.Name);
            if (eΔ1 != default!) {
                return eΔ1;
            }
            p.proc = proc;
        }
    }
    finally {
        Ꮡp.of(LazyProc.Ꮡmu).Unlock();
    }

    return default!;
}

// mustFind is like Find but panics if search fails.
internal static void mustFind(this ж<LazyProc> Ꮡp) {
    var e = Ꮡp.Find();
    if (e != default!) {
        throw panic(e);
    }
}

// Addr returns the address of the procedure represented by p.
// The return value can be passed to Syscall to run the procedure.
public static uintptr Addr(this ж<LazyProc> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    Ꮡp.mustFind();
    return p.proc.Addr();
}

// Call executes procedure p with arguments a. See the documentation of
// Proc.Call for more information.
public static (uintptr r1, uintptr r2, error lastErr) Call(this ж<LazyProc> Ꮡp, params ꓸꓸꓸuintptr aʗp) {
    ref var p = ref Ꮡp.Value;

    Ꮡp.mustFind();
    return p.proc.Call(aʗp);
}

} // end syscall_package
