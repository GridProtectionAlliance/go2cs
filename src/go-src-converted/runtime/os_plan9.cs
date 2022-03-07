// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:35 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_plan9.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private partial struct mOS {
    public uint waitsemacount;
    public ptr<sbyte> notesig;
    public ptr<byte> errstr;
    public bool ignoreHangup;
}

private static int closefd(int fd);

//go:noescape
private static int open(ptr<byte> name, int mode, int perm);

//go:noescape
private static int pread(int fd, unsafe.Pointer buf, int nbytes, long offset);

//go:noescape
private static int pwrite(int fd, unsafe.Pointer buf, int nbytes, long offset);

private static long seek(int fd, long offset, int whence);

//go:noescape
private static void exits(ptr<byte> msg);

//go:noescape
private static int brk_(unsafe.Pointer addr);

private static int sleep(int ms);

private static int rfork(int flags);

//go:noescape
private static int plan9_semacquire(ptr<uint> addr, int block);

//go:noescape
private static int plan9_tsemacquire(ptr<uint> addr, int ms);

//go:noescape
private static int plan9_semrelease(ptr<uint> addr, int count);

//go:noescape
private static int notify(unsafe.Pointer fn);

private static int noted(int mode);

//go:noescape
private static long nsec(ptr<long> _p0);

//go:noescape
private static void sigtramp(unsafe.Pointer ureg, unsafe.Pointer note);

private static void setfpmasks();

//go:noescape
private static void tstart_plan9(ptr<m> newm);

private static @string errstr();

private partial struct _Plink { // : System.UIntPtr
}

//go:linkname os_sigpipe os.sigpipe
private static void os_sigpipe() {
    throw("too many writes on closed pipe");
}

private static void sigpanic() => func((_, panic, _) => {
    var g = getg();
    if (!canpanic(g)) {>>MARKER:FUNCTION_errstr_BLOCK_PREFIX<<
        throw("unexpected signal during runtime execution");
    }
    var note = gostringnocopy((byte.val)(@unsafe.Pointer(g.m.notesig)));

    if (g.sig == _SIGRFAULT || g.sig == _SIGWFAULT) 
        var i = indexNoFloat(note, "addr=");
        if (i >= 0) {>>MARKER:FUNCTION_tstart_plan9_BLOCK_PREFIX<<
            i += 5;
        }        i = indexNoFloat(note, "va=");


        else if (i >= 0) {>>MARKER:FUNCTION_setfpmasks_BLOCK_PREFIX<<
            i += 3;
        }
        else
 {>>MARKER:FUNCTION_sigtramp_BLOCK_PREFIX<<
            panicmem();
        }
        var addr = note[(int)i..];
        g.sigcode1 = uintptr(atolwhex(addr));
        if (g.sigcode1 < 0x1000) {>>MARKER:FUNCTION_nsec_BLOCK_PREFIX<<
            panicmem();
        }
        if (g.paniconfault) {>>MARKER:FUNCTION_noted_BLOCK_PREFIX<<
            panicmemAddr(g.sigcode1);
        }
        print("unexpected fault address ", hex(g.sigcode1), "\n");
        throw("fault");
    else if (g.sig == _SIGTRAP) 
        if (g.paniconfault) {>>MARKER:FUNCTION_notify_BLOCK_PREFIX<<
            panicmem();
        }
        throw(note);
    else if (g.sig == _SIGINTDIV) 
        panicdivide();
    else if (g.sig == _SIGFLOAT) 
        panicfloat();
    else 
        panic(errorString(note));
    
});

// indexNoFloat is bytealg.IndexString but safe to use in a note
// handler.
private static nint indexNoFloat(@string s, @string t) {
    if (len(t) == 0) {>>MARKER:FUNCTION_plan9_semrelease_BLOCK_PREFIX<<
        return 0;
    }
    for (nint i = 0; i < len(s); i++) {>>MARKER:FUNCTION_plan9_tsemacquire_BLOCK_PREFIX<<
        if (s[i] == t[0] && hasPrefix(s[(int)i..], t)) {>>MARKER:FUNCTION_plan9_semacquire_BLOCK_PREFIX<<
            return i;
        }
    }
    return -1;

}

private static long atolwhex(@string p) {
    while (hasPrefix(p, " ") || hasPrefix(p, "\t")) {>>MARKER:FUNCTION_rfork_BLOCK_PREFIX<<
        p = p[(int)1..];
    }
    var neg = false;
    if (hasPrefix(p, "-") || hasPrefix(p, "+")) {>>MARKER:FUNCTION_sleep_BLOCK_PREFIX<<
        neg = p[0] == '-';
        p = p[(int)1..];
        while (hasPrefix(p, " ") || hasPrefix(p, "\t")) {>>MARKER:FUNCTION_brk__BLOCK_PREFIX<<
            p = p[(int)1..];
        }
    }
    long n = default;

    if (hasPrefix(p, "0x") || hasPrefix(p, "0X")) 
        p = p[(int)2..];
        while (len(p) > 0) {>>MARKER:FUNCTION_exits_BLOCK_PREFIX<<
            if ('0' <= p[0] && p[0] <= '9') {>>MARKER:FUNCTION_seek_BLOCK_PREFIX<<
                n = n * 16 + int64(p[0] - '0');
            p = p[(int)1..];
            }
            else if ('a' <= p[0] && p[0] <= 'f') {>>MARKER:FUNCTION_pwrite_BLOCK_PREFIX<<
                n = n * 16 + int64(p[0] - 'a' + 10);
            }
            else if ('A' <= p[0] && p[0] <= 'F') {>>MARKER:FUNCTION_pread_BLOCK_PREFIX<<
                n = n * 16 + int64(p[0] - 'A' + 10);
            }
            else
 {>>MARKER:FUNCTION_open_BLOCK_PREFIX<<
                break;
            }

        }
    else if (hasPrefix(p, "0")) 
        while (len(p) > 0 && '0' <= p[0] && p[0] <= '7') {>>MARKER:FUNCTION_closefd_BLOCK_PREFIX<<
            n = n * 8 + int64(p[0] - '0');
            p = p[(int)1..];
        }
    else 
        while (len(p) > 0 && '0' <= p[0] && p[0] <= '9') {
            n = n * 10 + int64(p[0] - '0');
            p = p[(int)1..];
        }
        if (neg) {
        n = -n;
    }
    return n;

}

private partial struct sigset {
}

// Called to initialize a new m (including the bootstrap m).
// Called on the parent thread (main thread in case of bootstrap), can allocate memory.
private static void mpreinit(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;
 
    // Initialize stack and goroutine for note handling.
    mp.gsignal = malg(32 * 1024);
    mp.gsignal.m = mp;
    mp.notesig = (int8.val)(mallocgc(_ERRMAX, null, true)); 
    // Initialize stack for handling strings from the
    // errstr system call, as used in package syscall.
    mp.errstr = (byte.val)(mallocgc(_ERRMAX, null, true));

}

private static void sigsave(ptr<sigset> _addr_p) {
    ref sigset p = ref _addr_p.val;

}

private static void msigrestore(sigset sigmask) {
}

//go:nosplit
//go:nowritebarrierrec
private static void clearSignalHandlers() {
}

private static void sigblock(bool exiting) {
}

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate memory.
private static void minit() {
    if (atomic.Load(_addr_exiting) != 0) {
        exits(_addr_emptystatus[0]);
    }
    setfpmasks();

}

// Called from dropm to undo the effect of an minit.
private static void unminit() {
}

// Called from exitm, but not from drop, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
private static void mdestroy(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

private static slice<byte> sysstat = (slice<byte>)"/dev/sysstat\x00";

private static int getproccount() {
    ref array<byte> buf = ref heap(new array<byte>(2048), out ptr<array<byte>> _addr_buf);
    var fd = open(_addr_sysstat[0], _OREAD, 0);
    if (fd < 0) {
        return 1;
    }
    var ncpu = int32(0);
    while (true) {
        var n = read(fd, @unsafe.Pointer(_addr_buf), int32(len(buf)));
        if (n <= 0) {
            break;
        }
        for (var i = int32(0); i < n; i++) {
            if (buf[i] == '\n') {
                ncpu++;
            }
        }

    }
    closefd(fd);
    if (ncpu == 0) {
        ncpu = 1;
    }
    return ncpu;

}

private static slice<byte> devswap = (slice<byte>)"/dev/swap\x00";
private static slice<byte> pagesize = (slice<byte>)" pagesize\n";

private static System.UIntPtr getPageSize() {
    array<byte> buf = new array<byte>(2048);
    nint pos = default;
    var fd = open(_addr_devswap[0], _OREAD, 0);
    if (fd < 0) { 
        // There's not much we can do if /dev/swap doesn't
        // exist. However, nothing in the memory manager uses
        // this on Plan 9, so it also doesn't really matter.
        return minPhysPageSize;

    }
    while (pos < len(buf)) {
        var n = read(fd, @unsafe.Pointer(_addr_buf[pos]), int32(len(buf) - pos));
        if (n <= 0) {
            break;
        }
        pos += int(n);

    }
    closefd(fd);
    var text = buf[..(int)pos]; 
    // Find "<n> pagesize" line.
    nint bol = 0;
    foreach (var (i, c) in text) {
        if (c == '\n') {
            bol = i + 1;
        }
        if (bytesHasPrefix(text[(int)i..], pagesize)) { 
            // Parse number at the beginning of this line.
            return uintptr(_atoi(text[(int)bol..]));

        }
    }    return minPhysPageSize;

}

private static bool bytesHasPrefix(slice<byte> s, slice<byte> prefix) {
    if (len(s) < len(prefix)) {
        return false;
    }
    foreach (var (i, p) in prefix) {
        if (s[i] != p) {
            return false;
        }
    }    return true;

}

private static slice<byte> pid = (slice<byte>)"#c/pid\x00";

private static ulong getpid() {
    ref array<byte> b = ref heap(new array<byte>(20), out ptr<array<byte>> _addr_b);
    var fd = open(_addr_pid[0], 0, 0);
    if (fd >= 0) {
        read(fd, @unsafe.Pointer(_addr_b), int32(len(b)));
        closefd(fd);
    }
    var c = b[..];
    while (c[0] == ' ' || c[0] == '\t') {
        c = c[(int)1..];
    }
    return uint64(_atoi(c));

}

private static void osinit() {
    initBloc();
    ncpu = getproccount();
    physPageSize = getPageSize();
    getg().m.procid = getpid();
}

//go:nosplit
private static void crash() {
    notify(null) * (int.val)(null);

    0;

}

//go:nosplit
private static void getRandomData(slice<byte> r) { 
    // inspired by wyrand see hash32.go for detail
    var t = nanotime();
    var v = getg().m.procid ^ uint64(t);

    while (len(r) > 0) {
        v ^= 0xa0761d6478bd642f;
        v *= 0xe7037ed1a0b428db;
        nint size = 8;
        if (len(r) < 8) {
            size = len(r);
        }
        for (nint i = 0; i < size; i++) {
            r[i] = byte(v >> (int)((8 * i)));
        }
        r = r[(int)size..];
        v = v >> 32 | v << 32;

    }

}

private static void initsig(bool preinit) {
    if (!preinit) {
        notify(@unsafe.Pointer(funcPC(sigtramp)));
    }
}

//go:nosplit
private static void osyield() {
    sleep(0);
}

//go:nosplit
private static void osyield_no_g() {
    osyield();
}

//go:nosplit
private static void usleep(uint µs) {
    var ms = int32(µs / 1000);
    if (ms == 0) {
        ms = 1;
    }
    sleep(ms);

}

//go:nosplit
private static void usleep_no_g(uint usec) {
    usleep(usec);
}

//go:nosplit
private static long nanotime1() {
    ref long scratch = ref heap(out ptr<long> _addr_scratch);
    var ns = nsec(_addr_scratch); 
    // TODO(aram): remove hack after I fix _nsec in the pc64 kernel.
    if (ns == 0) {
        return scratch;
    }
    return ns;

}

private static slice<byte> goexits = (slice<byte>)"go: exit ";
private static slice<byte> emptystatus = (slice<byte>)"\x00";
private static uint exiting = default;

private static void goexitsall(ptr<byte> _addr_status) {
    ref byte status = ref _addr_status.val;

    array<byte> buf = new array<byte>(_ERRMAX);
    if (!atomic.Cas(_addr_exiting, 0, 1)) {
        return ;
    }
    getg().m.locks++;
    var n = copy(buf[..], goexits);
    n = copy(buf[(int)n..], gostringnocopy(status));
    var pid = getpid();
    {
        var mp = (m.val)(atomic.Loadp(@unsafe.Pointer(_addr_allm)));

        while (mp != null) {
            if (mp.procid != 0 && mp.procid != pid) {
                postnote(mp.procid, buf[..]);
            mp = mp.alllink;
            }

        }
    }
    getg().m.locks--;

}

private static slice<byte> procdir = (slice<byte>)"/proc/";
private static slice<byte> notefile = (slice<byte>)"/note\x00";

private static nint postnote(ulong pid, slice<byte> msg) {
    array<byte> buf = new array<byte>(128);
    array<byte> tmp = new array<byte>(32);
    var n = copy(buf[..], procdir);
    n += copy(buf[(int)n..], itoa(tmp[..], pid));
    copy(buf[(int)n..], notefile);
    var fd = open(_addr_buf[0], _OWRITE, 0);
    if (fd < 0) {
        return -1;
    }
    var len = findnull(_addr_msg[0]);
    if (write1(uintptr(fd), @unsafe.Pointer(_addr_msg[0]), int32(len)) != int32(len)) {
        closefd(fd);
        return -1;
    }
    closefd(fd);
    return 0;

}

//go:nosplit
private static void exit(int e) {
    slice<byte> status = default;
    if (e == 0) {
        status = emptystatus;
    }
    else
 { 
        // build error string
        array<byte> tmp = new array<byte>(32);
        status = append(itoa(tmp[..(int)len(tmp) - 1], uint64(e)), 0);

    }
    goexitsall(_addr_status[0]);
    exits(_addr_status[0]);

}

// May run with m.p==nil, so write barriers are not allowed.
//go:nowritebarrier
private static void newosproc(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (false) {
        print("newosproc mp=", mp, " ostk=", _addr_mp, "\n");
    }
    var pid = rfork(_RFPROC | _RFMEM | _RFNOWAIT);
    if (pid < 0) {
        throw("newosproc: rfork failed");
    }
    if (pid == 0) {
        tstart_plan9(_addr_mp);
    }
}

private static void exitThread(ptr<uint> _addr_wait) {
    ref uint wait = ref _addr_wait.val;
 
    // We should never reach exitThread on Plan 9 because we let
    // the OS clean up threads.
    throw("exitThread");

}

//go:nosplit
private static void semacreate(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

//go:nosplit
private static nint semasleep(long ns) {
    var _g_ = getg();
    if (ns >= 0) {
        var ms = timediv(ns, 1000000, null);
        if (ms == 0) {
            ms = 1;
        }
        var ret = plan9_tsemacquire(_addr__g_.m.waitsemacount, ms);
        if (ret == 1) {
            return 0; // success
        }
        return -1; // timeout or interrupted
    }
    while (plan9_semacquire(_addr__g_.m.waitsemacount, 1) < 0) { 
        // interrupted; try again (c.f. lock_sema.go)
    }
    return 0; // success
}

//go:nosplit
private static void semawakeup(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    plan9_semrelease(_addr_mp.waitsemacount, 1);
}

//go:nosplit
private static int read(int fd, unsafe.Pointer buf, int n) {
    return pread(fd, buf, n, -1);
}

//go:nosplit
private static int write1(System.UIntPtr fd, unsafe.Pointer buf, int n) {
    return pwrite(int32(fd), buf, n, -1);
}

private static slice<byte> _badsignal = (slice<byte>)"runtime: signal received on thread not created by Go.\n";

// This runs on a foreign stack, without an m or a g. No stack split.
//go:nosplit
private static void badsignal2() {
    pwrite(2, @unsafe.Pointer(_addr__badsignal[0]), int32(len(_badsignal)), -1);
    exits(_addr__badsignal[0]);
}

private static void raisebadsignal(uint sig) {
    badsignal2();
}

private static nint _atoi(slice<byte> b) {
    nint n = 0;
    while (len(b) > 0 && '0' <= b[0] && b[0] <= '9') {
        n = n * 10 + int(b[0]) - '0';
        b = b[(int)1..];
    }
    return n;
}

private static @string signame(uint sig) {
    if (sig >= uint32(len(sigtable))) {
        return "";
    }
    return sigtable[sig].name;

}

private static readonly var preemptMSupported = false;



private static void preemptM(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;
 
    // Not currently supported.
    //
    // TODO: Use a note like we use signals on POSIX OSes
}

} // end runtime_package
