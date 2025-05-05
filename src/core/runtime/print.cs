// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

[GoType("num:uint64")] partial struct Δhex;

internal static slice<byte> /*ret*/ bytes(@string s) {
    slice<byte> ret = default!;

    var rp = (ж<Δslice>)(uintptr)(new @unsafe.Pointer(Ꮡ(ret)));
    var sp = stringStructOf(Ꮡ(s));
    rp.val.Δarray = sp.val.str;
    rp.val.len = sp.val.len;
    rp.val.cap = sp.val.len;
    return ret;
}

internal static array<byte> printBacklog;
internal static nint printBacklogIndex;

// recordForPanic maintains a circular buffer of messages written by the
// runtime leading up to a process crash, allowing the messages to be
// extracted from a core dump.
//
// The text written during a process crash (following "panic" or "fatal
// error") is not saved, since the goroutine stacks will generally be readable
// from the runtime data structures in the core file.
internal static void recordForPanic(slice<byte> b) {
    printlock();
    if (panicking.Load() == 0) {
        // Not actively crashing: maintain circular buffer of print output.
        for (nint i = 0; i < len(b); ) {
            nint n = copy(printBacklog[(int)(printBacklogIndex)..], b[(int)(i)..]);
            i += n;
            printBacklogIndex += n;
            printBacklogIndex %= len(printBacklog);
        }
    }
    printunlock();
}

internal static mutex debuglock;

// The compiler emits calls to printlock and printunlock around
// the multiple calls that implement a single Go print or println
// statement. Some of the print helpers (printslice, for example)
// call print recursively. There is also the problem of a crash
// happening during the print routines and needing to acquire
// the print lock to print information about the crash.
// For both these reasons, let a thread acquire the printlock 'recursively'.
internal static void printlock() {
    var mp = getg().val.m;
    (~mp).locks++;
    // do not reschedule between printlock++ and lock(&debuglock).
    (~mp).printlock++;
    if ((~mp).printlock == 1) {
        @lock(Ꮡ(debuglock));
    }
    (~mp).locks--;
}

// now we know debuglock is held and holding up mp.locks for us.
internal static void printunlock() {
    var mp = getg().val.m;
    (~mp).printlock--;
    if ((~mp).printlock == 0) {
        unlock(Ꮡ(debuglock));
    }
}

// write to goroutine-local buffer if diverting output,
// or else standard error.
internal static void gwrite(slice<byte> b) {
    if (len(b) == 0) {
        return;
    }
    recordForPanic(b);
    var gp = getg();
    // Don't use the writebuf if gp.m is dying. We want anything
    // written through gwrite to appear in the terminal rather
    // than be written to in some buffer, if we're in a panicking state.
    // Note that we can't just clear writebuf in the gp.m.dying case
    // because a panic isn't allowed to have any write barriers.
    if (gp == nil || (~gp).writebuf == default! || (~(~gp).m).dying > 0) {
        writeErr(b);
        return;
    }
    nint n = copy((~gp).writebuf[(int)(len((~gp).writebuf))..(int)(cap((~gp).writebuf))], b);
    gp.val.writebuf = (~gp).writebuf[..(int)(len((~gp).writebuf) + n)];
}

internal static void printsp() {
    printstring(" "u8);
}

internal static void printnl() {
    printstring("\n"u8);
}

internal static void printbool(bool v) {
    if (v){
        printstring("true"u8);
    } else {
        printstring("false"u8);
    }
}

internal static void printfloat(float64 v) {
    switch (ᐧ) {
    case {} when v is != v: {
        printstring("NaN"u8);
        return;
    }
    case {} when v + v == v && v > 0: {
        printstring("+Inf"u8);
        return;
    }
    case {} when v + v == v && v < 0: {
        printstring("-Inf"u8);
        return;
    }}

    static readonly UntypedInt n = 7; // digits printed
    array<byte> buf = new(14); /* n + 7 */
    buf[0] = (rune)'+';
    nint e = 0;
    // exp
    if (v == 0){
        if (1 / v < 0) {
            buf[0] = (rune)'-';
        }
    } else {
        if (v < 0) {
            v = -v;
            buf[0] = (rune)'-';
        }
        // normalize
        while (v >= 10) {
            e++;
            v /= 10;
        }
        while (v < 1) {
            e--;
            v *= 10;
        }
        // round
        var h = 5.0F;
        for (nint iΔ1 = 0; iΔ1 < n; iΔ1++) {
            h /= 10;
        }
        v += h;
        if (v >= 10) {
            e++;
            v /= 10;
        }
    }
    // format +d.dddd+edd
    for (nint i = 0; i < n; i++) {
        nint s = ((nint)v);
        buf[i + 2] = ((byte)(s + (rune)'0'));
        v -= ((float64)s);
        v *= 10;
    }
    buf[1] = buf[2];
    buf[2] = (rune)'.';
    buf[n + 2] = (rune)'e';
    buf[n + 3] = (rune)'+';
    if (e < 0) {
        e = -e;
        buf[n + 3] = (rune)'-';
    }
    buf[n + 4] = ((byte)(e / 100)) + (rune)'0';
    buf[n + 5] = ((byte)(e / 10)) % 10 + (rune)'0';
    buf[n + 6] = ((byte)(e % 10)) + (rune)'0';
    gwrite(buf[..]);
}

internal static void printcomplex(complex128 c) {
    print("(", real(c), imag(c), "i)");
}

internal static void printuint(uint64 v) {
    array<byte> buf = new(100);
    nint i = len(buf);
    for (i--; i > 0; i--) {
        buf[i] = ((byte)(v % 10 + (rune)'0'));
        if (v < 10) {
            break;
        }
        v /= 10;
    }
    gwrite(buf[(int)(i)..]);
}

internal static void printint(int64 v) {
    if (v < 0) {
        printstring("-"u8);
        v = -v;
    }
    printuint(((uint64)v));
}

internal static nint minhexdigits = 0; // protected by printlock

internal static void printhex(uint64 v) {
    @string dig = "0123456789abcdef"u8;
    array<byte> buf = new(100);
    nint i = len(buf);
    for (i--; i > 0; i--) {
        buf[i] = dig[v % 16];
        if (v < 16 && len(buf) - i >= minhexdigits) {
            break;
        }
        v /= 16;
    }
    i--;
    buf[i] = (rune)'x';
    i--;
    buf[i] = (rune)'0';
    gwrite(buf[(int)(i)..]);
}

internal static void printpointer(@unsafe.Pointer Δp) {
    printhex(((uint64)((uintptr)Δp)));
}

internal static void printuintptr(uintptr Δp) {
    printhex(((uint64)Δp));
}

internal static void printstring(@string s) {
    gwrite(bytes(s));
}

internal static void printslice(slice<byte> s) {
    var sp = (ж<Δslice>)(uintptr)(new @unsafe.Pointer(Ꮡ(s)));
    print("[", len(s), "/", cap(s), "]");
    printpointer((~sp).Δarray);
}

internal static void printeface(eface e) {
    print("(", e._type, ",", e.data, ")");
}

internal static void printiface(iface i) {
    print("(", i.tab, ",", i.data, ")");
}

// hexdumpWords prints a word-oriented hex dump of [p, end).
//
// If mark != nil, it will be called with each printed word's address
// and should return a character mark to appear just before that
// word's value. It can return 0 to indicate no mark.
internal static void hexdumpWords(uintptr Δp, uintptr end, Func<uintptr, byte> mark) {
    printlock();
    array<byte> markbuf = new(1);
    markbuf[0] = (rune)' ';
    minhexdigits = ((nint)(@unsafe.Sizeof(((uintptr)0)) * 2));
    for (var i = ((uintptr)0); Δp + i < end; i += goarch.PtrSize) {
        if (i % 16 == 0) {
            if (i != 0) {
                println();
            }
            print(((Δhex)(Δp + i)), ": ");
        }
        if (mark != default!) {
            markbuf[0] = mark(Δp + i);
            if (markbuf[0] == 0) {
                markbuf[0] = (rune)' ';
            }
        }
        gwrite(markbuf[..]);
        var val = ~(ж<uintptr>)(uintptr)(((@unsafe.Pointer)(Δp + i)));
        print(((Δhex)val));
        print(" ");
        // Can we symbolize val?
        var fn = findfunc(val);
        if (fn.valid()) {
            print("<", funcname(fn), "+", ((Δhex)(val - fn.entry())), "> ");
        }
    }
    minhexdigits = 0;
    println();
    printunlock();
}

} // end runtime_package
