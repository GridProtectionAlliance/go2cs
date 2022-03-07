// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:51 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\print.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

    // The compiler knows that a print of a value of this type
    // should use printhex instead of printuint (decimal).
private partial struct hex { // : ulong
}

private static slice<byte> bytes(@string s) {
    slice<byte> ret = default;

    var rp = (slice.val)(@unsafe.Pointer(_addr_ret));
    var sp = stringStructOf(_addr_s);
    rp.array = sp.str;
    rp.len = sp.len;
    rp.cap = sp.len;
    return ;
}

 
// printBacklog is a circular buffer of messages written with the builtin
// print* functions, for use in postmortem analysis of core dumps.
private static array<byte> printBacklog = new array<byte>(512);private static nint printBacklogIndex = default;

// recordForPanic maintains a circular buffer of messages written by the
// runtime leading up to a process crash, allowing the messages to be
// extracted from a core dump.
//
// The text written during a process crash (following "panic" or "fatal
// error") is not saved, since the goroutine stacks will generally be readable
// from the runtime datastructures in the core file.
private static void recordForPanic(slice<byte> b) {
    printlock();

    if (atomic.Load(_addr_panicking) == 0) { 
        // Not actively crashing: maintain circular buffer of print output.
        {
            nint i = 0;

            while (i < len(b)) {
                var n = copy(printBacklog[(int)printBacklogIndex..], b[(int)i..]);
                i += n;
                printBacklogIndex += n;
                printBacklogIndex %= len(printBacklog);
            }

        }

    }
    printunlock();

}

private static mutex debuglock = default;

// The compiler emits calls to printlock and printunlock around
// the multiple calls that implement a single Go print or println
// statement. Some of the print helpers (printslice, for example)
// call print recursively. There is also the problem of a crash
// happening during the print routines and needing to acquire
// the print lock to print information about the crash.
// For both these reasons, let a thread acquire the printlock 'recursively'.

private static void printlock() {
    var mp = getg().m;
    mp.locks++; // do not reschedule between printlock++ and lock(&debuglock).
    mp.printlock++;
    if (mp.printlock == 1) {
        lock(_addr_debuglock);
    }
    mp.locks--; // now we know debuglock is held and holding up mp.locks for us.
}

private static void printunlock() {
    var mp = getg().m;
    mp.printlock--;
    if (mp.printlock == 0) {
        unlock(_addr_debuglock);
    }
}

// write to goroutine-local buffer if diverting output,
// or else standard error.
private static void gwrite(slice<byte> b) {
    if (len(b) == 0) {
        return ;
    }
    recordForPanic(b);
    var gp = getg(); 
    // Don't use the writebuf if gp.m is dying. We want anything
    // written through gwrite to appear in the terminal rather
    // than be written to in some buffer, if we're in a panicking state.
    // Note that we can't just clear writebuf in the gp.m.dying case
    // because a panic isn't allowed to have any write barriers.
    if (gp == null || gp.writebuf == null || gp.m.dying > 0) {
        writeErr(b);
        return ;
    }
    var n = copy(gp.writebuf[(int)len(gp.writebuf)..(int)cap(gp.writebuf)], b);
    gp.writebuf = gp.writebuf[..(int)len(gp.writebuf) + n];

}

private static void printsp() {
    printstring(" ");
}

private static void printnl() {
    printstring("\n");
}

private static void printbool(bool v) {
    if (v) {
        printstring("true");
    }
    else
 {
        printstring("false");
    }
}

private static void printfloat(double v) {

    if (v != v) 
        printstring("NaN");
        return ;
    else if (v + v == v && v > 0) 
        printstring("+Inf");
        return ;
    else if (v + v == v && v < 0) 
        printstring("-Inf");
        return ;
        const nint n = 7; // digits printed
 // digits printed
    array<byte> buf = new array<byte>(n + 7);
    buf[0] = '+';
    nint e = 0; // exp
    if (v == 0) {
        if (1 / v < 0) {
            buf[0] = '-';
        }
    }
    else
 {
        if (v < 0) {
            v = -v;
            buf[0] = '-';
        }
        while (v >= 10) {
            e++;
            v /= 10;
        }
        while (v < 1) {
            e--;
            v *= 10;
        } 

        // round
        float h = 5.0F;
        {
            nint i__prev1 = i;

            for (nint i = 0; i < n; i++) {
                h /= 10;
            }


            i = i__prev1;
        }
        v += h;
        if (v >= 10) {
            e++;
            v /= 10;
        }
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < n; i++) {
            var s = int(v);
            buf[i + 2] = byte(s + '0');
            v -= float64(s);
            v *= 10;
        }

        i = i__prev1;
    }
    buf[1] = buf[2];
    buf[2] = '.';

    buf[n + 2] = 'e';
    buf[n + 3] = '+';
    if (e < 0) {
        e = -e;
        buf[n + 3] = '-';
    }
    buf[n + 4] = byte(e / 100) + '0';
    buf[n + 5] = byte(e / 10) % 10 + '0';
    buf[n + 6] = byte(e % 10) + '0';
    gwrite(buf[..]);

}

private static void printcomplex(System.Numerics.Complex128 c) {
    print("(", real(c), imag(c), "i)");
}

private static void printuint(ulong v) {
    array<byte> buf = new array<byte>(100);
    var i = len(buf);
    i--;

    while (i > 0) {
        buf[i] = byte(v % 10 + '0');
        if (v < 10) {
            break;
        i--;
        }
        v /= 10;

    }
    gwrite(buf[(int)i..]);

}

private static void printint(long v) {
    if (v < 0) {
        printstring("-");
        v = -v;
    }
    printuint(uint64(v));

}

private static nint minhexdigits = 0; // protected by printlock

private static void printhex(ulong v) {
    const @string dig = "0123456789abcdef";

    array<byte> buf = new array<byte>(100);
    var i = len(buf);
    i--;

    while (i > 0) {
        buf[i] = dig[v % 16];
        if (v < 16 && len(buf) - i >= minhexdigits) {
            break;
        i--;
        }
        v /= 16;

    }
    i--;
    buf[i] = 'x';
    i--;
    buf[i] = '0';
    gwrite(buf[(int)i..]);

}

private static void printpointer(unsafe.Pointer p) {
    printhex(uint64(uintptr(p)));
}
private static void printuintptr(System.UIntPtr p) {
    printhex(uint64(p));
}

private static void printstring(@string s) {
    gwrite(bytes(s));
}

private static void printslice(slice<byte> s) {
    var sp = (slice.val)(@unsafe.Pointer(_addr_s));
    print("[", len(s), "/", cap(s), "]");
    printpointer(sp.array);
}

private static void printeface(eface e) {
    print("(", e._type, ",", e.data, ")");
}

private static void printiface(iface i) {
    print("(", i.tab, ",", i.data, ")");
}

// hexdumpWords prints a word-oriented hex dump of [p, end).
//
// If mark != nil, it will be called with each printed word's address
// and should return a character mark to appear just before that
// word's value. It can return 0 to indicate no mark.
private static byte hexdumpWords(System.UIntPtr p, System.UIntPtr end, Func<System.UIntPtr, byte> mark) {
    printlock();
    array<byte> markbuf = new array<byte>(1);
    markbuf[0] = ' ';
    minhexdigits = int(@unsafe.Sizeof(uintptr(0)) * 2);
    {
        var i = uintptr(0);

        while (p + i < end) {
            if (i % 16 == 0) {
                if (i != 0) {
                    println();
            i += sys.PtrSize;
                }

                print(hex(p + i), ": ");

            }

            if (mark != null) {
                markbuf[0] = mark(p + i);
                if (markbuf[0] == 0) {
                    markbuf[0] = ' ';
                }
            }

            gwrite(markbuf[..]);
            ptr<ptr<System.UIntPtr>> val = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(p + i));
            print(hex(val));
            print(" "); 

            // Can we symbolize val?
            var fn = findfunc(val);
            if (fn.valid()) {
                print("<", funcname(fn), "+", hex(val - fn.entry), "> ");
            }

        }
    }
    minhexdigits = 0;
    println();
    printunlock();

}

} // end runtime_package
