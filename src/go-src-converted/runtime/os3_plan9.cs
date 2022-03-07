// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:20 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os3_plan9.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // May run during STW, so write barriers are not allowed.
    //
    //go:nowritebarrierrec
private static nint sighandler(ptr<ureg> _addr__ureg, ptr<byte> _addr_note, ptr<g> _addr_gp) {
    ref ureg _ureg = ref _addr__ureg.val;
    ref byte note = ref _addr_note.val;
    ref g gp = ref _addr_gp.val;

    var _g_ = getg();
    sigTabT t = default;
    bool docrash = default;
    nint sig = default;
    nint flags = default;
    int level = default;

    ptr<sigctxt> c = addr(new sigctxt(_ureg));
    var notestr = gostringnocopy(note); 

    // The kernel will never pass us a nil note or ureg so we probably
    // made a mistake somewhere in sigtramp.
    if (_ureg == null || note == null) {
        print("sighandler: ureg ", _ureg, " note ", note, "\n");
        goto Throw;
    }
    if (len(notestr) > _ERRMAX - 1) {
        print("sighandler: note is longer than ERRMAX\n");
        goto Throw;
    }
    if (isAbortPC(c.pc())) { 
        // Never turn abort into a panic.
        goto Throw;

    }
    flags = _SigNotify;
    foreach (var (__sig, __t) in sigtable) {
        sig = __sig;
        t = __t;
        if (hasPrefix(notestr, t.name)) {
            flags = t.flags;
            break;
        }
    }
    if (flags & _SigPanic != 0 && gp.throwsplit) { 
        // We can't safely sigpanic because it may grow the
        // stack. Abort in the signal handler instead.
        flags = (flags & ~_SigPanic) | _SigThrow;

    }
    if (flags & _SigGoExit != 0) {
        exits((byte.val)(add(@unsafe.Pointer(note), 9))); // Strip "go: exit " prefix.
    }
    if (flags & _SigPanic != 0) { 
        // Copy the error string from sigtramp's stack into m->notesig so
        // we can reliably access it from the panic routines.
        memmove(@unsafe.Pointer(_g_.m.notesig), @unsafe.Pointer(note), uintptr(len(notestr) + 1));
        gp.sig = uint32(sig);
        gp.sigpc = c.pc();

        var pc = c.pc();
        var sp = c.sp(); 

        // If we don't recognize the PC as code
        // but we do recognize the top pointer on the stack as code,
        // then assume this was a call to non-code and treat like
        // pc == 0, to make unwinding show the context.
        if (pc != 0 && !findfunc(pc).valid() && findfunc(new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(sp))).valid()) {
            pc = 0;
        }
        if (usesLR) {
            c.savelr(c.lr());
        }
        if (pc != 0) {
            if (usesLR) {
                c.setlr(pc);
            }
            else
 {
                sp -= sys.PtrSize * (uintptr.val)(@unsafe.Pointer(sp));

                pc;
                c.setsp(sp);

            }
        }
        if (usesLR) {
            c.setpc(funcPC(sigpanictramp));
        }
        else
 {
            c.setpc(funcPC(sigpanic0));
        }
        return _NCONT;

    }
    if (flags & _SigNotify != 0) {
        if (ignoredNote(note)) {
            return _NCONT;
        }
        if (sendNote(note)) {
            return _NCONT;
        }
    }
    if (flags & _SigKill != 0) {
        goto Exit;
    }
    if (flags & _SigThrow == 0) {
        return _NCONT;
    }
Throw:
    _g_.m.throwing = 1;
    _g_.m.caughtsig.set(gp);
    startpanic_m();
    print(notestr, "\n");
    print("PC=", hex(c.pc()), "\n");
    print("\n");
    level, _, docrash = gotraceback();
    if (level > 0) {
        goroutineheader(gp);
        tracebacktrap(c.pc(), c.sp(), c.lr(), gp);
        tracebackothers(gp);
        print("\n");
        dumpregs(_ureg);
    }
    if (docrash) {
        crash();
    }
Exit:
    goexitsall(note);
    exits(note);
    return _NDFLT; // not reached
}

private static void sigenable(uint sig) {
}

private static void sigdisable(uint sig) {
}

private static void sigignore(uint sig) {
}

private static void setProcessCPUProfiler(int hz) {
}

private static void setThreadCPUProfiler(int hz) { 
    // TODO: Enable profiling interrupts.
    getg().m.profilehz = hz;

}

// gsignalStack is unused on Plan 9.
private partial struct gsignalStack {
}

} // end runtime_package
