// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file provides an internal debug logging facility. The debug
// log is a lightweight, in-memory, per-M ring buffer. By default, the
// runtime prints the debug log on panic.
//
// To print something to the debug log, call dlog to obtain a dlogger
// and use the methods on that to add values. The values will be
// space-separated in the output (much like println).
//
// This facility can be enabled by passing -tags debuglog when
// building. Without this tag, dlog calls compile to nothing.

// package runtime -- go2cs converted at 2022 March 06 22:08:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\debuglog.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // debugLogBytes is the size of each per-M ring buffer. This is
    // allocated off-heap to avoid blowing up the M and hence the GC'd
    // heap size.
private static readonly nint debugLogBytes = 16 << 10;

// debugLogStringLimit is the maximum number of bytes in a string.
// Above this, the string will be truncated with "..(n more bytes).."


// debugLogStringLimit is the maximum number of bytes in a string.
// Above this, the string will be truncated with "..(n more bytes).."
private static readonly var debugLogStringLimit = debugLogBytes / 8;

// dlog returns a debug logger. The caller can use methods on the
// returned logger to add values, which will be space-separated in the
// final output, much like println. The caller must call end() to
// finish the message.
//
// dlog can be used from highly-constrained corners of the runtime: it
// is safe to use in the signal handler, from within the write
// barrier, from within the stack implementation, and in places that
// must be recursively nosplit.
//
// This will be compiled away if built without the debuglog build tag.
// However, argument construction may not be. If any of the arguments
// are not literals or trivial expressions, consider protecting the
// call with "if dlogEnabled".
//
//go:nosplit
//go:nowritebarrierrec


// dlog returns a debug logger. The caller can use methods on the
// returned logger to add values, which will be space-separated in the
// final output, much like println. The caller must call end() to
// finish the message.
//
// dlog can be used from highly-constrained corners of the runtime: it
// is safe to use in the signal handler, from within the write
// barrier, from within the stack implementation, and in places that
// must be recursively nosplit.
//
// This will be compiled away if built without the debuglog build tag.
// However, argument construction may not be. If any of the arguments
// are not literals or trivial expressions, consider protecting the
// call with "if dlogEnabled".
//
//go:nosplit
//go:nowritebarrierrec
private static ptr<dlogger> dlog() {
    if (!dlogEnabled) {
        return _addr_null!;
    }
    var tick = uint64(cputicks());
    var nano = uint64(nanotime()); 

    // Try to get a cached logger.
    var l = getCachedDlogger(); 

    // If we couldn't get a cached logger, try to get one from the
    // global pool.
    if (l == null) {
        var allp = (uintptr.val)(@unsafe.Pointer(_addr_allDloggers));
        var all = (dlogger.val)(@unsafe.Pointer(atomic.Loaduintptr(allp)));
        {
            var l1 = all;

            while (l1 != null) {
                if (atomic.Load(_addr_l1.owned) == 0 && atomic.Cas(_addr_l1.owned, 0, 1)) {
                    l = l1;
                    break;
                l1 = l1.allLink;
                }

            }

        }

    }
    if (l == null) {
        l = (dlogger.val)(sysAlloc(@unsafe.Sizeof(new dlogger()), null));
        if (l == null) {
            throw("failed to allocate debug log");
        }
        l.w.r.data = _addr_l.w.data;
        l.owned = 1; 

        // Prepend to allDloggers list.
        var headp = (uintptr.val)(@unsafe.Pointer(_addr_allDloggers));
        while (true) {
            var head = atomic.Loaduintptr(headp);
            l.allLink = (dlogger.val)(@unsafe.Pointer(head));
            if (atomic.Casuintptr(headp, head, uintptr(@unsafe.Pointer(l)))) {
                break;
            }
        }

    }
    const nint deltaLimit = 1 << (int)((3 * 7)) - 1; // ~2ms between sync packets
 // ~2ms between sync packets
    if (tick - l.w.tick > deltaLimit || nano - l.w.nano > deltaLimit) {
        l.w.writeSync(tick, nano);
    }
    l.w.ensure(debugLogHeaderSize);
    l.w.write += debugLogHeaderSize; 

    // Write record header.
    l.w.uvarint(tick - l.w.tick);
    l.w.uvarint(nano - l.w.nano);
    var gp = getg();
    if (gp != null && gp.m != null && gp.m.p != 0) {
        l.w.varint(int64(gp.m.p.ptr().id));
    }
    else
 {
        l.w.varint(-1);
    }
    return _addr_l!;

}

// A dlogger writes to the debug log.
//
// To obtain a dlogger, call dlog(). When done with the dlogger, call
// end().
//
//go:notinheap
private partial struct dlogger {
    public debugLogWriter w; // allLink is the next dlogger in the allDloggers list.
    public ptr<dlogger> allLink; // owned indicates that this dlogger is owned by an M. This is
// accessed atomically.
    public uint owned;
}

// allDloggers is a list of all dloggers, linked through
// dlogger.allLink. This is accessed atomically. This is prepend only,
// so it doesn't need to protect against ABA races.
private static ptr<dlogger> allDloggers;

//go:nosplit
private static void end(this ptr<dlogger> _addr_l) {
    ref dlogger l = ref _addr_l.val;

    if (!dlogEnabled) {
        return ;
    }
    var size = l.w.write - l.w.r.end;
    if (!l.w.writeFrameAt(l.w.r.end, size)) {
        throw("record too large");
    }
    l.w.r.end = l.w.write; 

    // Attempt to return this logger to the cache.
    if (putCachedDlogger(l)) {
        return ;
    }
    atomic.Store(_addr_l.owned, 0);

}

private static readonly nint debugLogUnknown = 1 + iota;
private static readonly var debugLogBoolTrue = 0;
private static readonly var debugLogBoolFalse = 1;
private static readonly var debugLogInt = 2;
private static readonly var debugLogUint = 3;
private static readonly var debugLogHex = 4;
private static readonly var debugLogPtr = 5;
private static readonly var debugLogString = 6;
private static readonly var debugLogConstString = 7;
private static readonly var debugLogStringOverflow = 8;

private static readonly var debugLogPC = 9;
private static readonly var debugLogTraceback = 10;


//go:nosplit
private static ptr<dlogger> b(this ptr<dlogger> _addr_l, bool x) {
    ref dlogger l = ref _addr_l.val;

    if (!dlogEnabled) {
        return _addr_l!;
    }
    if (x) {
        l.w.@byte(debugLogBoolTrue);
    }
    else
 {
        l.w.@byte(debugLogBoolFalse);
    }
    return _addr_l!;

}

//go:nosplit
private static ptr<dlogger> i(this ptr<dlogger> _addr_l, nint x) {
    ref dlogger l = ref _addr_l.val;

    return _addr_l.i64(int64(x))!;
}

//go:nosplit
private static ptr<dlogger> i8(this ptr<dlogger> _addr_l, sbyte x) {
    ref dlogger l = ref _addr_l.val;

    return _addr_l.i64(int64(x))!;
}

//go:nosplit
private static ptr<dlogger> i16(this ptr<dlogger> _addr_l, short x) {
    ref dlogger l = ref _addr_l.val;

    return _addr_l.i64(int64(x))!;
}

//go:nosplit
private static ptr<dlogger> i32(this ptr<dlogger> _addr_l, int x) {
    ref dlogger l = ref _addr_l.val;

    return _addr_l.i64(int64(x))!;
}

//go:nosplit
private static ptr<dlogger> i64(this ptr<dlogger> _addr_l, long x) {
    ref dlogger l = ref _addr_l.val;

    if (!dlogEnabled) {
        return _addr_l!;
    }
    l.w.@byte(debugLogInt);
    l.w.varint(x);
    return _addr_l!;

}

//go:nosplit
private static ptr<dlogger> u(this ptr<dlogger> _addr_l, nuint x) {
    ref dlogger l = ref _addr_l.val;

    return _addr_l.u64(uint64(x))!;
}

//go:nosplit
private static ptr<dlogger> uptr(this ptr<dlogger> _addr_l, System.UIntPtr x) {
    ref dlogger l = ref _addr_l.val;

    return _addr_l.u64(uint64(x))!;
}

//go:nosplit
private static ptr<dlogger> u8(this ptr<dlogger> _addr_l, byte x) {
    ref dlogger l = ref _addr_l.val;

    return _addr_l.u64(uint64(x))!;
}

//go:nosplit
private static ptr<dlogger> u16(this ptr<dlogger> _addr_l, ushort x) {
    ref dlogger l = ref _addr_l.val;

    return _addr_l.u64(uint64(x))!;
}

//go:nosplit
private static ptr<dlogger> u32(this ptr<dlogger> _addr_l, uint x) {
    ref dlogger l = ref _addr_l.val;

    return _addr_l.u64(uint64(x))!;
}

//go:nosplit
private static ptr<dlogger> u64(this ptr<dlogger> _addr_l, ulong x) {
    ref dlogger l = ref _addr_l.val;

    if (!dlogEnabled) {
        return _addr_l!;
    }
    l.w.@byte(debugLogUint);
    l.w.uvarint(x);
    return _addr_l!;

}

//go:nosplit
private static ptr<dlogger> hex(this ptr<dlogger> _addr_l, ulong x) {
    ref dlogger l = ref _addr_l.val;

    if (!dlogEnabled) {
        return _addr_l!;
    }
    l.w.@byte(debugLogHex);
    l.w.uvarint(x);
    return _addr_l!;

}

//go:nosplit
private static ptr<dlogger> p(this ptr<dlogger> _addr_l, object x) {
    ref dlogger l = ref _addr_l.val;

    if (!dlogEnabled) {
        return _addr_l!;
    }
    l.w.@byte(debugLogPtr);
    if (x == null) {
        l.w.uvarint(0);
    }
    else
 {
        var v = efaceOf(_addr_x);

        if (v._type.kind & kindMask == kindChan || v._type.kind & kindMask == kindFunc || v._type.kind & kindMask == kindMap || v._type.kind & kindMask == kindPtr || v._type.kind & kindMask == kindUnsafePointer) 
            l.w.uvarint(uint64(uintptr(v.data)));
        else 
            throw("not a pointer type");
        
    }
    return _addr_l!;

}

//go:nosplit
private static ptr<dlogger> s(this ptr<dlogger> _addr_l, @string x) {
    ref dlogger l = ref _addr_l.val;

    if (!dlogEnabled) {
        return _addr_l!;
    }
    var str = stringStructOf(_addr_x);
    var datap = _addr_firstmoduledata;
    if (len(x) > 4 && datap.etext <= uintptr(str.str) && uintptr(str.str) < datap.end) { 
        // String constants are in the rodata section, which
        // isn't recorded in moduledata. But it has to be
        // somewhere between etext and end.
        l.w.@byte(debugLogConstString);
        l.w.uvarint(uint64(str.len));
        l.w.uvarint(uint64(uintptr(str.str) - datap.etext));

    }
    else
 {
        l.w.@byte(debugLogString);
        ref slice<byte> b = ref heap(out ptr<slice<byte>> _addr_b);
        var bb = (slice.val)(@unsafe.Pointer(_addr_b));
        bb.array = str.str;
        (bb.len, bb.cap) = (str.len, str.len);        if (len(b) > debugLogStringLimit) {
            b = b[..(int)debugLogStringLimit];
        }
        l.w.uvarint(uint64(len(b)));
        l.w.bytes(b);
        if (len(b) != len(x)) {
            l.w.@byte(debugLogStringOverflow);
            l.w.uvarint(uint64(len(x) - len(b)));
        }
    }
    return _addr_l!;

}

//go:nosplit
private static ptr<dlogger> pc(this ptr<dlogger> _addr_l, System.UIntPtr x) {
    ref dlogger l = ref _addr_l.val;

    if (!dlogEnabled) {
        return _addr_l!;
    }
    l.w.@byte(debugLogPC);
    l.w.uvarint(uint64(x));
    return _addr_l!;

}

//go:nosplit
private static ptr<dlogger> traceback(this ptr<dlogger> _addr_l, slice<System.UIntPtr> x) {
    ref dlogger l = ref _addr_l.val;

    if (!dlogEnabled) {
        return _addr_l!;
    }
    l.w.@byte(debugLogTraceback);
    l.w.uvarint(uint64(len(x)));
    foreach (var (_, pc) in x) {
        l.w.uvarint(uint64(pc));
    }    return _addr_l!;

}

// A debugLogWriter is a ring buffer of binary debug log records.
//
// A log record consists of a 2-byte framing header and a sequence of
// fields. The framing header gives the size of the record as a little
// endian 16-bit value. Each field starts with a byte indicating its
// type, followed by type-specific data. If the size in the framing
// header is 0, it's a sync record consisting of two little endian
// 64-bit values giving a new time base.
//
// Because this is a ring buffer, new records will eventually
// overwrite old records. Hence, it maintains a reader that consumes
// the log as it gets overwritten. That reader state is where an
// actual log reader would start.
//
//go:notinheap
private partial struct debugLogWriter {
    public ulong write;
    public debugLogBuf data; // tick and nano are the time bases from the most recently
// written sync record.
    public ulong tick; // r is a reader that consumes records as they get overwritten
// by the writer. It also acts as the initial reader state
// when printing the log.
    public ulong nano; // r is a reader that consumes records as they get overwritten
// by the writer. It also acts as the initial reader state
// when printing the log.
    public debugLogReader r; // buf is a scratch buffer for encoding. This is here to
// reduce stack usage.
    public array<byte> buf;
}

//go:notinheap
private partial struct debugLogBuf { // : array<byte>
}

 
// debugLogHeaderSize is the number of bytes in the framing
// header of every dlog record.
private static readonly nint debugLogHeaderSize = 2; 

// debugLogSyncSize is the number of bytes in a sync record.
private static readonly var debugLogSyncSize = debugLogHeaderSize + 2 * 8;


//go:nosplit
private static void ensure(this ptr<debugLogWriter> _addr_l, ulong n) {
    ref debugLogWriter l = ref _addr_l.val;

    while (l.write + n >= l.r.begin + uint64(len(l.data))) { 
        // Consume record at begin.
        if (l.r.skip() == ~uint64(0)) { 
            // Wrapped around within a record.
            //
            // TODO(austin): It would be better to just
            // eat the whole buffer at this point, but we
            // have to communicate that to the reader
            // somehow.
            throw("record wrapped around");

        }
    }

}

//go:nosplit
private static bool writeFrameAt(this ptr<debugLogWriter> _addr_l, ulong pos, ulong size) {
    ref debugLogWriter l = ref _addr_l.val;

    l.data[pos % uint64(len(l.data))] = uint8(size);
    l.data[(pos + 1) % uint64(len(l.data))] = uint8(size >> 8);
    return size <= 0xFFFF;
}

//go:nosplit
private static void writeSync(this ptr<debugLogWriter> _addr_l, ulong tick, ulong nano) {
    ref debugLogWriter l = ref _addr_l.val;

    (l.tick, l.nano) = (tick, nano);    l.ensure(debugLogHeaderSize);
    l.writeFrameAt(l.write, 0);
    l.write += debugLogHeaderSize;
    l.writeUint64LE(tick);
    l.writeUint64LE(nano);
    l.r.end = l.write;
}

//go:nosplit
private static void writeUint64LE(this ptr<debugLogWriter> _addr_l, ulong x) {
    ref debugLogWriter l = ref _addr_l.val;

    array<byte> b = new array<byte>(8);
    b[0] = byte(x);
    b[1] = byte(x >> 8);
    b[2] = byte(x >> 16);
    b[3] = byte(x >> 24);
    b[4] = byte(x >> 32);
    b[5] = byte(x >> 40);
    b[6] = byte(x >> 48);
    b[7] = byte(x >> 56);
    l.bytes(b[..]);
}

//go:nosplit
private static void @byte(this ptr<debugLogWriter> _addr_l, byte x) {
    ref debugLogWriter l = ref _addr_l.val;

    l.ensure(1);
    var pos = l.write;
    l.write++;
    l.data[pos % uint64(len(l.data))] = x;
}

//go:nosplit
private static void bytes(this ptr<debugLogWriter> _addr_l, slice<byte> x) {
    ref debugLogWriter l = ref _addr_l.val;

    l.ensure(uint64(len(x)));
    var pos = l.write;
    l.write += uint64(len(x));
    while (len(x) > 0) {
        var n = copy(l.data[(int)pos % uint64(len(l.data))..], x);
        pos += uint64(n);
        x = x[(int)n..];
    }
}

//go:nosplit
private static void varint(this ptr<debugLogWriter> _addr_l, long x) {
    ref debugLogWriter l = ref _addr_l.val;

    ulong u = default;
    if (x < 0) {
        u = (~uint64(x) << 1) | 1; // complement i, bit 0 is 1
    }
    else
 {
        u = (uint64(x) << 1); // do not complement i, bit 0 is 0
    }
    l.uvarint(u);

}

//go:nosplit
private static void uvarint(this ptr<debugLogWriter> _addr_l, ulong u) {
    ref debugLogWriter l = ref _addr_l.val;

    nint i = 0;
    while (u >= 0x80) {
        l.buf[i] = byte(u) | 0x80;
        u>>=7;
        i++;
    }
    l.buf[i] = byte(u);
    i++;
    l.bytes(l.buf[..(int)i]);
}

private partial struct debugLogReader {
    public ptr<debugLogBuf> data; // begin and end are the positions in the log of the beginning
// and end of the log data, modulo len(data).
    public ulong begin; // tick and nano are the current time base at begin.
    public ulong end; // tick and nano are the current time base at begin.
    public ulong tick;
    public ulong nano;
}

//go:nosplit
private static ulong skip(this ptr<debugLogReader> _addr_r) {
    ref debugLogReader r = ref _addr_r.val;
 
    // Read size at pos.
    if (r.begin + debugLogHeaderSize > r.end) {
        return ~uint64(0);
    }
    var size = uint64(r.readUint16LEAt(r.begin));
    if (size == 0) { 
        // Sync packet.
        r.tick = r.readUint64LEAt(r.begin + debugLogHeaderSize);
        r.nano = r.readUint64LEAt(r.begin + debugLogHeaderSize + 8);
        size = debugLogSyncSize;

    }
    if (r.begin + size > r.end) {
        return ~uint64(0);
    }
    r.begin += size;
    return size;

}

//go:nosplit
private static ushort readUint16LEAt(this ptr<debugLogReader> _addr_r, ulong pos) {
    ref debugLogReader r = ref _addr_r.val;

    return uint16(r.data[pos % uint64(len(r.data))]) | uint16(r.data[(pos + 1) % uint64(len(r.data))]) << 8;
}

//go:nosplit
private static ulong readUint64LEAt(this ptr<debugLogReader> _addr_r, ulong pos) {
    ref debugLogReader r = ref _addr_r.val;

    array<byte> b = new array<byte>(8);
    foreach (var (i) in b) {
        b[i] = r.data[pos % uint64(len(r.data))];
        pos++;
    }    return uint64(b[0]) | uint64(b[1]) << 8 | uint64(b[2]) << 16 | uint64(b[3]) << 24 | uint64(b[4]) << 32 | uint64(b[5]) << 40 | uint64(b[6]) << 48 | uint64(b[7]) << 56;
}

private static ulong peek(this ptr<debugLogReader> _addr_r) {
    ulong tick = default;
    ref debugLogReader r = ref _addr_r.val;
 
    // Consume any sync records.
    var size = uint64(0);
    while (size == 0) {
        if (r.begin + debugLogHeaderSize > r.end) {
            return ~uint64(0);
        }
        size = uint64(r.readUint16LEAt(r.begin));
        if (size != 0) {
            break;
        }
        if (r.begin + debugLogSyncSize > r.end) {
            return ~uint64(0);
        }
        r.tick = r.readUint64LEAt(r.begin + debugLogHeaderSize);
        r.nano = r.readUint64LEAt(r.begin + debugLogHeaderSize + 8);
        r.begin += debugLogSyncSize;

    } 

    // Peek tick delta.
    if (r.begin + size > r.end) {
        return ~uint64(0);
    }
    var pos = r.begin + debugLogHeaderSize;
    ulong u = default;
    {
        var i = uint(0);

        while (>>MARKER:FOREXPRESSION_LEVEL_1<<) {
            var b = r.data[pos % uint64(len(r.data))];
            pos++;
            u |= uint64(b & ~0x80) << (int)(i);
            if (b & 0x80 == 0) {
                break;
            i += 7;
            }

        }
    }
    if (pos > r.begin + size) {
        return ~uint64(0);
    }
    return r.tick + u;

}

private static (ulong, ulong, ulong, nint) header(this ptr<debugLogReader> _addr_r) {
    ulong end = default;
    ulong tick = default;
    ulong nano = default;
    nint p = default;
    ref debugLogReader r = ref _addr_r.val;
 
    // Read size. We've already skipped sync packets and checked
    // bounds in peek.
    var size = uint64(r.readUint16LEAt(r.begin));
    end = r.begin + size;
    r.begin += debugLogHeaderSize; 

    // Read tick, nano, and p.
    tick = r.uvarint() + r.tick;
    nano = r.uvarint() + r.nano;
    p = int(r.varint());

    return ;

}

private static ulong uvarint(this ptr<debugLogReader> _addr_r) {
    ref debugLogReader r = ref _addr_r.val;

    ulong u = default;
    {
        var i = uint(0);

        while (>>MARKER:FOREXPRESSION_LEVEL_1<<) {
            var b = r.data[r.begin % uint64(len(r.data))];
            r.begin++;
            u |= uint64(b & ~0x80) << (int)(i);
            if (b & 0x80 == 0) {
                break;
            i += 7;
            }

        }
    }
    return u;

}

private static long varint(this ptr<debugLogReader> _addr_r) {
    ref debugLogReader r = ref _addr_r.val;

    var u = r.uvarint();
    long v = default;
    if (u & 1 == 0) {
        v = int64(u >> 1);
    }
    else
 {
        v = ~int64(u >> 1);
    }
    return v;

}

private static bool printVal(this ptr<debugLogReader> _addr_r) {
    ref debugLogReader r = ref _addr_r.val;

    var typ = r.data[r.begin % uint64(len(r.data))];
    r.begin++;


    if (typ == debugLogUnknown) 
        print("<unknown kind>");
    else if (typ == debugLogBoolTrue) 
        print(true);
    else if (typ == debugLogBoolFalse) 
        print(false);
    else if (typ == debugLogInt) 
        print(r.varint());
    else if (typ == debugLogUint) 
        print(r.uvarint());
    else if (typ == debugLogHex || typ == debugLogPtr) 
        print(hex(r.uvarint()));
    else if (typ == debugLogString) 
        var sl = r.uvarint();
        if (r.begin + sl > r.end) {
            r.begin = r.end;
            print("<string length corrupted>");
            break;
        }
        while (sl > 0) {
            var b = r.data[(int)r.begin % uint64(len(r.data))..];
            if (uint64(len(b)) > sl) {
                b = b[..(int)sl];
            }
            r.begin += uint64(len(b));
            sl -= uint64(len(b));
            gwrite(b);
        }
    else if (typ == debugLogConstString) 
        var len = int(r.uvarint());
        var ptr = uintptr(r.uvarint());
        ptr += firstmoduledata.etext;
        ref stringStruct str = ref heap(new stringStruct(str:unsafe.Pointer(ptr),len:len,), out ptr<stringStruct> _addr_str);
        ptr<ptr<@string>> s = new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(_addr_str));
        print(s);
    else if (typ == debugLogStringOverflow) 
        print("..(", r.uvarint(), " more bytes)..");
    else if (typ == debugLogPC) 
        printDebugLogPC(uintptr(r.uvarint()), false);
    else if (typ == debugLogTraceback) 
        var n = int(r.uvarint());
        for (nint i = 0; i < n; i++) {
            print("\n\t"); 
            // gentraceback PCs are always return PCs.
            // Convert them to call PCs.
            //
            // TODO(austin): Expand inlined frames.
            printDebugLogPC(uintptr(r.uvarint()), true);

        }
    else 
        print("<unknown field type ", hex(typ), " pos ", r.begin - 1, " end ", r.end, ">\n");
        return false;
        return true;

}

// printDebugLog prints the debug log.
private static void printDebugLog() {
    if (!dlogEnabled) {
        return ;
    }
    printlock(); 

    // Get the list of all debug logs.
    var allp = (uintptr.val)(@unsafe.Pointer(_addr_allDloggers));
    var all = (dlogger.val)(@unsafe.Pointer(atomic.Loaduintptr(allp))); 

    // Count the logs.
    nint n = 0;
    {
        var l__prev1 = l;

        var l = all;

        while (l != null) {
            n++;
            l = l.allLink;
        }

        l = l__prev1;
    }
    if (n == 0) {
        printunlock();
        return ;
    }
    private partial struct readState {
        public ref debugLogReader debugLogReader => ref debugLogReader_val;
        public bool first;
        public ulong lost;
        public ulong nextTick;
    }
    var state1 = sysAlloc(@unsafe.Sizeof(new readState()) * uintptr(n), null);
    if (state1 == null) {
        println("failed to allocate read state for", n, "logs");
        printunlock();
        return ;
    }
    ptr<array<readState>> state = new ptr<ptr<array<readState>>>(state1)[..(int)n];
 {
        l = all;
        {
            var i__prev1 = i;

            foreach (var (__i) in state) {
                i = __i;
                var s = _addr_state[i];
                s.debugLogReader = l.w.r;
                s.first = true;
                s.lost = l.w.r.begin;
                s.nextTick = s.peek();
                l = l.allLink;
            }

            i = i__prev1;
        }
    }    while (true) { 
        // Find the next record.
        var best = default;
        best.tick = ~uint64(0);
        {
            var i__prev2 = i;

            foreach (var (__i) in state) {
                i = __i;
                if (state[i].nextTick < best.tick) {
                    best.tick = state[i].nextTick;
                    best.i = i;
                }
            }

            i = i__prev2;
        }

        if (best.tick == ~uint64(0)) {
            break;
        }
        s = _addr_state[best.i];
        if (s.first) {
            print(">> begin log ", best.i);
            if (s.lost != 0) {
                print("; lost first ", s.lost >> 10, "KB");
            }
            print(" <<\n");
            s.first = false;
        }
        var (end, _, nano, p) = s.header();
        var oldEnd = s.end;
        s.end = end;

        print("[");
        array<byte> tmpbuf = new array<byte>(21);
        var pnano = int64(nano) - runtimeInitTime;
        if (pnano < 0) { 
            // Logged before runtimeInitTime was set.
            pnano = 0;

        }
        print(string(itoaDiv(tmpbuf[..], uint64(pnano), 9)));
        print(" P ", p, "] ");

        {
            var i__prev2 = i;

            for (nint i = 0; s.begin < s.end; i++) {
                if (i > 0) {
                    print(" ");
                }
                if (!s.printVal()) { 
                    // Abort this P log.
                    print("<aborting P log>");
                    end = oldEnd;
                    break;

                }

            }


            i = i__prev2;
        }
        println(); 

        // Move on to the next record.
        s.begin = end;
        s.end = oldEnd;
        s.nextTick = s.peek();

    }

    printunlock();

}

// printDebugLogPC prints a single symbolized PC. If returnPC is true,
// pc is a return PC that must first be converted to a call PC.
private static void printDebugLogPC(System.UIntPtr pc, bool returnPC) {
    var fn = findfunc(pc);
    if (returnPC && (!fn.valid() || pc > fn.entry)) { 
        // TODO(austin): Don't back up if the previous frame
        // was a sigpanic.
        pc--;

    }
    print(hex(pc));
    if (!fn.valid()) {
        print(" [unknown PC]");
    }
    else
 {
        var name = funcname(fn);
        var (file, line) = funcline(fn, pc);
        print(" [", name, "+", hex(pc - fn.entry), " ", file, ":", line, "]");
    }
}

} // end runtime_package
