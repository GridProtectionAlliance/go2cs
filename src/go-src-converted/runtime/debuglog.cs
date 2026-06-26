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
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// debugLogBytes is the size of each per-M ring buffer. This is
// allocated off-heap to avoid blowing up the M and hence the GC'd
// heap size.
internal static readonly UntypedInt debugLogBytes = /* 16 << 10 */ 16384;

// debugLogStringLimit is the maximum number of bytes in a string.
// Above this, the string will be truncated with "..(n more bytes).."
internal static readonly UntypedInt debugLogStringLimit = /* debugLogBytes / 8 */ 2048;

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
internal static ж<dlogger> dlog() {
    if (!dlogEnabled) {
        return default!;
    }
    // Get the time.
    var (tick, nano) = (((uint64)cputicks()), ((uint64)nanotime()));
    // Try to get a cached logger.
    var l = getCachedDlogger();
    // If we couldn't get a cached logger, try to get one from the
    // global pool.
    if (l == nil) {
        var allp = ((ж<uintptr>)((@unsafe.Pointer)(Ꮡ(allDloggers))));
        var all = (ж<dlogger>)(uintptr)(((@unsafe.Pointer)atomic.Loaduintptr(allp)));
        for (var l1 = all; l1 != nil; l1 = l1.val.allLink) {
            if ((~l1).owned.Load() == 0 && (~l1).owned.CompareAndSwap(0, 1)) {
                l = l1;
                break;
            }
        }
    }
    // If that failed, allocate a new logger.
    if (l == nil) {
        // Use sysAllocOS instead of sysAlloc because we want to interfere
        // with the runtime as little as possible, and sysAlloc updates accounting.
        l = (ж<dlogger>)(uintptr)(sysAllocOS(@unsafe.Sizeof(new dlogger(nil))));
        if (l == nil) {
            @throw("failed to allocate debug log"u8);
        }
        (~l).w.r.data = Ꮡ(~l).w.of(debugLogWriter.Ꮡdata);
        (~l).owned.Store(1);
        // Prepend to allDloggers list.
        var headp = ((ж<uintptr>)((@unsafe.Pointer)(Ꮡ(allDloggers))));
        while (ᐧ) {
            var head = atomic.Loaduintptr(headp);
            l.val.allLink = (ж<dlogger>)(uintptr)(((@unsafe.Pointer)head));
            if (atomic.Casuintptr(headp, head, ((uintptr)new @unsafe.Pointer(l)))) {
                break;
            }
        }
    }
    // If the time delta is getting too high, write a new sync
    // packet. We set the limit so we don't write more than 6
    // bytes of delta in the record header.
    static readonly UntypedInt deltaLimit = /* 1<<(3*7) - 1 */ 2097151; // ~2ms between sync packets
    if (tick - (~l).w.tick > deltaLimit || nano - (~l).w.nano > deltaLimit) {
        (~l).w.writeSync(tick, nano);
    }
    // Reserve space for framing header.
    (~l).w.ensure(debugLogHeaderSize);
    (~l).w.write += debugLogHeaderSize;
    // Write record header.
    (~l).w.uvarint(tick - (~l).w.tick);
    (~l).w.uvarint(nano - (~l).w.nano);
    var gp = getg();
    if (gp != nil && (~gp).m != nil && (~(~gp).m).p != 0){
        (~l).w.varint(((int64)(~(~(~gp).m).p.ptr()).id));
    } else {
        (~l).w.varint(-1);
    }
    return l;
}

// A dlogger writes to the debug log.
//
// To obtain a dlogger, call dlog(). When done with the dlogger, call
// end().
[GoType] partial struct dlogger {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal debugLogWriter w;
    // allLink is the next dlogger in the allDloggers list.
    internal ж<dlogger> allLink;
    // owned indicates that this dlogger is owned by an M. This is
    // accessed atomically.
    internal @internal.runtime.atomic_package.Uint32 owned;
}

// allDloggers is a list of all dloggers, linked through
// dlogger.allLink. This is accessed atomically. This is prepend only,
// so it doesn't need to protect against ABA races.
internal static ж<dlogger> allDloggers;

//go:nosplit
[GoRecv] internal static void end(this ref dlogger l) {
    if (!dlogEnabled) {
        return;
    }
    // Fill in framing header.
    var size = l.w.write - l.w.r.end;
    if (!l.w.writeFrameAt(l.w.r.end, size)) {
        @throw("record too large"u8);
    }
    // Commit the record.
    l.w.r.end = l.w.write;
    // Attempt to return this logger to the cache.
    if (putCachedDlogger(l)) {
        return;
    }
    // Return the logger to the global pool.
    l.owned.Store(0);
}

internal static readonly UntypedInt debugLogUnknown = /* 1 + iota */ 1;
internal static readonly UntypedInt debugLogBoolTrue = 2;
internal static readonly UntypedInt debugLogBoolFalse = 3;
internal static readonly UntypedInt debugLogInt = 4;
internal static readonly UntypedInt debugLogUint = 5;
internal static readonly UntypedInt debugLogHex = 6;
internal static readonly UntypedInt debugLogPtr = 7;
internal static readonly UntypedInt debugLogString = 8;
internal static readonly UntypedInt debugLogConstString = 9;
internal static readonly UntypedInt debugLogStringOverflow = 10;
internal static readonly UntypedInt debugLogPC = 11;
internal static readonly UntypedInt debugLogTraceback = 12;

//go:nosplit
[GoRecv("capture")] internal static ж<dlogger> b(this ref dlogger l, bool x) {
    if (!dlogEnabled) {
        return bꓸᏑl;
    }
    if (x){
        l.w.@byte(debugLogBoolTrue);
    } else {
        l.w.@byte(debugLogBoolFalse);
    }
    return bꓸᏑl;
}

//go:nosplit
[GoRecv] internal static ж<dlogger> i(this ref dlogger l, nint x) {
    return l.i64(((int64)x));
}

//go:nosplit
[GoRecv] internal static ж<dlogger> i8(this ref dlogger l, int8 x) {
    return l.i64(((int64)x));
}

//go:nosplit
[GoRecv] internal static ж<dlogger> i16(this ref dlogger l, int16 x) {
    return l.i64(((int64)x));
}

//go:nosplit
[GoRecv] internal static ж<dlogger> i32(this ref dlogger l, int32 x) {
    return l.i64(((int64)x));
}

//go:nosplit
[GoRecv("capture")] internal static ж<dlogger> i64(this ref dlogger l, int64 x) {
    if (!dlogEnabled) {
        return i64ꓸᏑl;
    }
    l.w.@byte(debugLogInt);
    l.w.varint(x);
    return i64ꓸᏑl;
}

//go:nosplit
[GoRecv] internal static ж<dlogger> u(this ref dlogger l, nuint x) {
    return l.u64(((uint64)x));
}

//go:nosplit
[GoRecv] internal static ж<dlogger> uptr(this ref dlogger l, uintptr x) {
    return l.u64(((uint64)x));
}

//go:nosplit
[GoRecv] internal static ж<dlogger> u8(this ref dlogger l, uint8 x) {
    return l.u64(((uint64)x));
}

//go:nosplit
[GoRecv] internal static ж<dlogger> u16(this ref dlogger l, uint16 x) {
    return l.u64(((uint64)x));
}

//go:nosplit
[GoRecv] internal static ж<dlogger> u32(this ref dlogger l, uint32 x) {
    return l.u64(((uint64)x));
}

//go:nosplit
[GoRecv("capture")] internal static ж<dlogger> u64(this ref dlogger l, uint64 x) {
    if (!dlogEnabled) {
        return u64ꓸᏑl;
    }
    l.w.@byte(debugLogUint);
    l.w.uvarint(x);
    return u64ꓸᏑl;
}

//go:nosplit
[GoRecv("capture")] internal static ж<dlogger> hex(this ref dlogger l, uint64 x) {
    if (!dlogEnabled) {
        return hexꓸᏑl;
    }
    l.w.@byte(debugLogHex);
    l.w.uvarint(x);
    return hexꓸᏑl;
}

//go:nosplit
[GoRecv("capture")] internal static ж<dlogger> p(this ref dlogger l, any x) {
    if (!dlogEnabled) {
        return pꓸᏑl;
    }
    l.w.@byte(debugLogPtr);
    if (x == default!){
        l.w.uvarint(0);
    } else {
        var v = efaceOf(Ꮡ(x));
        var exprᴛ1 = (abiꓸKind)((~(~v)._type).Kind_ & abi.KindMask);
        if (exprᴛ1 == abi.Chan || exprᴛ1 == abi.Func || exprᴛ1 == abi.Map || exprᴛ1 == abi.Pointer || exprᴛ1 == abi.UnsafePointer) {
            l.w.uvarint(((uint64)((uintptr)(~v).data)));
        }
        else { /* default: */
            @throw("not a pointer type"u8);
        }

    }
    return pꓸᏑl;
}

//go:nosplit
[GoRecv("capture")] internal static ж<dlogger> s(this ref dlogger l, @string x) {
    if (!dlogEnabled) {
        return sꓸᏑl;
    }
    var strData = @unsafe.StringData(x);
    var datap = Ꮡ(firstmoduledata);
    if (len(x) > 4 && (~datap).etext <= ((uintptr)new @unsafe.Pointer(strData)) && ((uintptr)new @unsafe.Pointer(strData)) < (~datap).end){
        // String constants are in the rodata section, which
        // isn't recorded in moduledata. But it has to be
        // somewhere between etext and end.
        l.w.@byte(debugLogConstString);
        l.w.uvarint(((uint64)len(x)));
        l.w.uvarint(((uint64)(((uintptr)new @unsafe.Pointer(strData)) - (~datap).etext)));
    } else {
        l.w.@byte(debugLogString);
        // We can't use unsafe.Slice as it may panic, which isn't safe
        // in this (potentially) nowritebarrier context.
        slice<byte> b = default!;
        var bb = (ж<Δslice>)(uintptr)(new @unsafe.Pointer(Ꮡ(b)));
        bb.val.Δarray = new @unsafe.Pointer(strData);
        (bb.val.len, bb.val.cap) = (len(x), len(x));
        if (len(b) > debugLogStringLimit) {
            b = b[..(int)(debugLogStringLimit)];
        }
        l.w.uvarint(((uint64)len(b)));
        l.w.bytes(b);
        if (len(b) != len(x)) {
            l.w.@byte(debugLogStringOverflow);
            l.w.uvarint(((uint64)(len(x) - len(b))));
        }
    }
    return sꓸᏑl;
}

//go:nosplit
[GoRecv("capture")] internal static ж<dlogger> pc(this ref dlogger l, uintptr x) {
    if (!dlogEnabled) {
        return pcꓸᏑl;
    }
    l.w.@byte(debugLogPC);
    l.w.uvarint(((uint64)x));
    return pcꓸᏑl;
}

//go:nosplit
[GoRecv("capture")] internal static ж<dlogger> traceback(this ref dlogger l, slice<uintptr> x) {
    if (!dlogEnabled) {
        return tracebackꓸᏑl;
    }
    l.w.@byte(debugLogTraceback);
    l.w.uvarint(((uint64)len(x)));
    foreach (var (_, pc) in x) {
        l.w.uvarint(((uint64)pc));
    }
    return tracebackꓸᏑl;
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
[GoType] partial struct debugLogWriter {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal uint64 write;
    internal debugLogBuf data;
    // tick and nano are the time bases from the most recently
    // written sync record.
    internal uint64 tick;
    internal uint64 nano;
    // r is a reader that consumes records as they get overwritten
    // by the writer. It also acts as the initial reader state
    // when printing the log.
    internal debugLogReader r;
    // buf is a scratch buffer for encoding. This is here to
    // reduce stack usage.
    internal array<byte> buf = new(10);
}

[GoType] partial struct debugLogBuf {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal array<byte> b = new(debugLogBytes);
}

internal static readonly UntypedInt debugLogHeaderSize = 2;
internal static readonly UntypedInt debugLogSyncSize = /* debugLogHeaderSize + 2*8 */ 18;

//go:nosplit
[GoRecv] internal static void ensure(this ref debugLogWriter l, uint64 n) {
    while (l.write + n >= l.r.begin + ((uint64)len(l.data.b))) {
        // Consume record at begin.
        if (l.r.skip() == ~((uint64)0)) {
            // Wrapped around within a record.
            //
            // TODO(austin): It would be better to just
            // eat the whole buffer at this point, but we
            // have to communicate that to the reader
            // somehow.
            @throw("record wrapped around"u8);
        }
    }
}

//go:nosplit
[GoRecv] internal static bool writeFrameAt(this ref debugLogWriter l, uint64 pos, uint64 size) {
    l.data.b[pos % ((uint64)len(l.data.b))] = ((uint8)size);
    l.data.b[(pos + 1) % ((uint64)len(l.data.b))] = ((uint8)(size >> (int)(8)));
    return size <= 65535;
}

//go:nosplit
[GoRecv] internal static void writeSync(this ref debugLogWriter l, uint64 tick, uint64 nano) {
    (l.tick, l.nano) = (tick, nano);
    l.ensure(debugLogHeaderSize);
    l.writeFrameAt(l.write, 0);
    l.write += debugLogHeaderSize;
    l.writeUint64LE(tick);
    l.writeUint64LE(nano);
    l.r.end = l.write;
}

//go:nosplit
[GoRecv] internal static void writeUint64LE(this ref debugLogWriter l, uint64 x) {
    array<byte> b = new(8);
    b[0] = ((byte)x);
    b[1] = ((byte)(x >> (int)(8)));
    b[2] = ((byte)(x >> (int)(16)));
    b[3] = ((byte)(x >> (int)(24)));
    b[4] = ((byte)(x >> (int)(32)));
    b[5] = ((byte)(x >> (int)(40)));
    b[6] = ((byte)(x >> (int)(48)));
    b[7] = ((byte)(x >> (int)(56)));
    l.bytes(b[..]);
}

//go:nosplit
[GoRecv] internal static void @byte(this ref debugLogWriter l, byte x) {
    l.ensure(1);
    var pos = l.write;
    l.write++;
    l.data.b[pos % ((uint64)len(l.data.b))] = x;
}

//go:nosplit
[GoRecv] internal static void bytes(this ref debugLogWriter l, slice<byte> x) {
    l.ensure(((uint64)len(x)));
    var pos = l.write;
    l.write += ((uint64)len(x));
    while (len(x) > 0) {
        nint n = copy(l.data.b[(int)(pos % ((uint64)len(l.data.b)))..], x);
        pos += ((uint64)n);
        x = x[(int)(n)..];
    }
}

//go:nosplit
[GoRecv] internal static void varint(this ref debugLogWriter l, int64 x) {
    uint64 u = default!;
    if (x < 0){
        u = (uint64)((~((uint64)x) << (int)(1)) | 1);
    } else {
        // complement i, bit 0 is 1
        u = (((uint64)x) << (int)(1));
    }
    // do not complement i, bit 0 is 0
    l.uvarint(u);
}

//go:nosplit
[GoRecv] internal static void uvarint(this ref debugLogWriter l, uint64 u) {
    nint i = 0;
    while (u >= 128) {
        l.buf[i] = (byte)(((byte)u) | 128);
        u >>= (UntypedInt)(7);
        i++;
    }
    l.buf[i] = ((byte)u);
    i++;
    l.bytes(l.buf[..(int)(i)]);
}

[GoType] partial struct debugLogReader {
    internal ж<debugLogBuf> data;
    // begin and end are the positions in the log of the beginning
    // and end of the log data, modulo len(data).
    internal uint64 begin;
    internal uint64 end;
    // tick and nano are the current time base at begin.
    internal uint64 tick;
    internal uint64 nano;
}

//go:nosplit
[GoRecv] internal static uint64 skip(this ref debugLogReader r) {
    // Read size at pos.
    if (r.begin + debugLogHeaderSize > r.end) {
        return ~((uint64)0);
    }
    var size = ((uint64)r.readUint16LEAt(r.begin));
    if (size == 0) {
        // Sync packet.
        r.tick = r.readUint64LEAt(r.begin + debugLogHeaderSize);
        r.nano = r.readUint64LEAt(r.begin + debugLogHeaderSize + 8);
        size = debugLogSyncSize;
    }
    if (r.begin + size > r.end) {
        return ~((uint64)0);
    }
    r.begin += size;
    return size;
}

//go:nosplit
[GoRecv] internal static uint16 readUint16LEAt(this ref debugLogReader r, uint64 pos) {
    return (uint16)(((uint16)r.data.b[pos % ((uint64)len(r.data.b))]) | ((uint16)r.data.b[(pos + 1) % ((uint64)len(r.data.b))]) << (int)(8));
}

//go:nosplit
[GoRecv] internal static uint64 readUint64LEAt(this ref debugLogReader r, uint64 pos) {
    array<byte> b = new(8);
    foreach (var (i, _) in b) {
        b[i] = r.data.b[pos % ((uint64)len(r.data.b))];
        pos++;
    }
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(((uint64)b[0]) | ((uint64)b[1]) << (int)(8)) | ((uint64)b[2]) << (int)(16)) | ((uint64)b[3]) << (int)(24)) | ((uint64)b[4]) << (int)(32)) | ((uint64)b[5]) << (int)(40)) | ((uint64)b[6]) << (int)(48)) | ((uint64)b[7]) << (int)(56));
}

[GoRecv] internal static uint64 /*tick*/ peek(this ref debugLogReader r) {
    uint64 tick = default!;

    // Consume any sync records.
    var size = ((uint64)0);
    while (size == 0) {
        if (r.begin + debugLogHeaderSize > r.end) {
            return ~((uint64)0);
        }
        size = ((uint64)r.readUint16LEAt(r.begin));
        if (size != 0) {
            break;
        }
        if (r.begin + debugLogSyncSize > r.end) {
            return ~((uint64)0);
        }
        // Sync packet.
        r.tick = r.readUint64LEAt(r.begin + debugLogHeaderSize);
        r.nano = r.readUint64LEAt(r.begin + debugLogHeaderSize + 8);
        r.begin += debugLogSyncSize;
    }
    // Peek tick delta.
    if (r.begin + size > r.end) {
        return ~((uint64)0);
    }
    var pos = r.begin + debugLogHeaderSize;
    uint64 u = default!;
    for (nuint i = ((nuint)0); ᐧ ; i += 7) {
        var b = r.data.b[pos % ((uint64)len(r.data.b))];
        pos++;
        u |= (uint64)(((uint64)((byte)(b & ~128))) << (int)(i));
        if ((byte)(b & 128) == 0) {
            break;
        }
    }
    if (pos > r.begin + size) {
        return ~((uint64)0);
    }
    return r.tick + u;
}

[GoRecv] internal static (uint64 end, uint64 tick, uint64 nano, nint Δp) header(this ref debugLogReader r) {
    uint64 end = default!;
    uint64 tick = default!;
    uint64 nano = default!;
    nint Δp = default!;

    // Read size. We've already skipped sync packets and checked
    // bounds in peek.
    var size = ((uint64)r.readUint16LEAt(r.begin));
    end = r.begin + size;
    r.begin += debugLogHeaderSize;
    // Read tick, nano, and p.
    tick = r.uvarint() + r.tick;
    nano = r.uvarint() + r.nano;
    Δp = ((nint)r.varint());
    return (end, tick, nano, Δp);
}

[GoRecv] internal static uint64 uvarint(this ref debugLogReader r) {
    uint64 u = default!;
    for (nuint i = ((nuint)0); ᐧ ; i += 7) {
        var b = r.data.b[r.begin % ((uint64)len(r.data.b))];
        r.begin++;
        u |= (uint64)(((uint64)((byte)(b & ~128))) << (int)(i));
        if ((byte)(b & 128) == 0) {
            break;
        }
    }
    return u;
}

[GoRecv] internal static int64 varint(this ref debugLogReader r) {
    var u = r.uvarint();
    int64 v = default!;
    if ((uint64)(u & 1) == 0){
        v = ((int64)(u >> (int)(1)));
    } else {
        v = ~((int64)(u >> (int)(1)));
    }
    return v;
}

[GoRecv] internal static bool printVal(this ref debugLogReader r) {
    var typ = r.data.b[r.begin % ((uint64)len(r.data.b))];
    r.begin++;
    var exprᴛ1 = typ;
    { /* default: */
        print("<unknown field type ", ((Δhex)typ), " pos ", r.begin - 1, " end ", r.end, ">\n");
        return false;
    }
    if (exprᴛ1 == debugLogUnknown) {
        print("<unknown kind>");
    }
    else if (exprᴛ1 == debugLogBoolTrue) {
        print(true);
    }
    else if (exprᴛ1 == debugLogBoolFalse) {
        print(false);
    }
    else if (exprᴛ1 == debugLogInt) {
        print(r.varint());
    }
    else if (exprᴛ1 == debugLogUint) {
        print(r.uvarint());
    }
    else if (exprᴛ1 == debugLogHex || exprᴛ1 == debugLogPtr) {
        print(((Δhex)r.uvarint()));
    }
    else if (exprᴛ1 == debugLogString) {
        var sl = r.uvarint();
        if (r.begin + sl > r.end) {
            r.begin = r.end;
            print("<string length corrupted>");
            break;
        }
        while (sl > 0) {
            var b = r.data.b[(int)(r.begin % ((uint64)len(r.data.b)))..];
            if (((uint64)len(b)) > sl) {
                b = b[..(int)(sl)];
            }
            r.begin += ((uint64)len(b));
            sl -= ((uint64)len(b));
            gwrite(b);
        }
    }
    else if (exprᴛ1 == debugLogConstString) {
        nint len = ((nint)r.uvarint());
        var ptr = ((uintptr)r.uvarint());
        ptr += firstmoduledata.etext;
        ref var str = ref heap<stringStruct>(out var Ꮡstr);
        str = new stringStruct( // We can't use unsafe.String as it may panic, which isn't safe
 // in this (potentially) nowritebarrier context.

            str: ((@unsafe.Pointer)ptr),
            len: len
        );
        @string s = ~(ж<@string>)(uintptr)(new @unsafe.Pointer(Ꮡstr));
        print(s);
    }
    else if (exprᴛ1 == debugLogStringOverflow) {
        print("..(", r.uvarint(), " more bytes)..");
    }
    else if (exprᴛ1 == debugLogPC) {
        printDebugLogPC(((uintptr)r.uvarint()), false);
    }
    else if (exprᴛ1 == debugLogTraceback) {
        nint n = ((nint)r.uvarint());
        for (nint i = 0; i < n; i++) {
            print("\n\t");
            // gentraceback PCs are always return PCs.
            // Convert them to call PCs.
            //
            // TODO(austin): Expand inlined frames.
            printDebugLogPC(((uintptr)r.uvarint()), true);
        }
    }

    return true;
}

// Prepare read state for all logs.
[GoType("dyn")] partial struct printDebugLog_readState {
    internal partial ref debugLogReader debugLogReader { get; }
    internal bool first;
    internal uint64 lost;
    internal uint64 nextTick;
}

[GoType("dyn")] partial struct printDebugLog_best {
    internal uint64 tick;
    internal nint i;
}

// printDebugLog prints the debug log.
internal static unsafe void printDebugLog() {
    if (!dlogEnabled) {
        return;
    }
    // This function should not panic or throw since it is used in
    // the fatal panic path and this may deadlock.
    printlock();
    // Get the list of all debug logs.
    var allp = ((ж<uintptr>)((@unsafe.Pointer)(Ꮡ(allDloggers))));
    var all = (ж<dlogger>)(uintptr)(((@unsafe.Pointer)atomic.Loaduintptr(allp)));
    // Count the logs.
    nint n = 0;
    for (var l = all; l != nil; l = l.val.allLink) {
        n++;
    }
    if (n == 0) {
        printunlock();
        return;
    }
    // Use sysAllocOS instead of sysAlloc because we want to interfere
    // with the runtime as little as possible, and sysAlloc updates accounting.
    @unsafe.Pointer state1 = (uintptr)sysAllocOS(@unsafe.Sizeof(new readState(nil)) * ((uintptr)n));
    if (state1 == nil) {
        println("failed to allocate read state for", n, "logs");
        printunlock();
        return;
    }
    var state = new Span<readState>((readState*)(uintptr)(state1), n);
    {
        var l = all;
        foreach (var (i, _) in state) {
            var s = Ꮡ(state, i);
            s.val.debugLogReader = (~l).w.r;
            s.val.first = true;
            s.val.lost = (~l).w.r.begin;
            s.val.nextTick = s.peek();
            l = l.val.allLink;
        }
    }
    // Print records.
    while (ᐧ) {
        // Find the next record.
        printDebugLog_best best = default!;
        best.tick = ~((uint64)0);
        foreach (var (i, _) in state) {
            if (state[i].nextTick < best.tick) {
                best.tick = state[i].nextTick;
                best.i = i;
            }
        }
        if (best.tick == ~((uint64)0)) {
            break;
        }
        // Print record.
        var s = Ꮡ(state, best.i);
        if ((~s).first) {
            print(">> begin log ", best.i);
            if ((~s).lost != 0) {
                print("; lost first ", (~s).lost >> (int)(10), "KB");
            }
            print(" <<\n");
            s.val.first = false;
        }
        var (end, _, nano, Δp) = s.header();
        var oldEnd = s.end;
        s.end = end;
        print("[");
        array<byte> tmpbuf = new(21);
        var pnano = ((int64)nano) - runtimeInitTime;
        if (pnano < 0) {
            // Logged before runtimeInitTime was set.
            pnano = 0;
        }
        var pnanoBytes = itoaDiv(tmpbuf[..], ((uint64)pnano), 9);
        print(slicebytetostringtmp((ж<byte>)(uintptr)(noescape(new @unsafe.Pointer(Ꮡ(pnanoBytes, 0)))), len(pnanoBytes)));
        print(" P ", Δp, "] ");
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
        println();
        // Move on to the next record.
        s.begin = end;
        s.end = oldEnd;
        s.val.nextTick = s.peek();
    }
    printunlock();
}

// printDebugLogPC prints a single symbolized PC. If returnPC is true,
// pc is a return PC that must first be converted to a call PC.
internal static void printDebugLogPC(uintptr pc, bool returnPC) {
    var fn = findfunc(pc);
    if (returnPC && (!fn.valid() || pc > fn.entry())) {
        // TODO(austin): Don't back up if the previous frame
        // was a sigpanic.
        pc--;
    }
    print(((Δhex)pc));
    if (!fn.valid()){
        print(" [unknown PC]");
    } else {
        @string name = funcname(fn);
        var (file, line) = funcline(fn, pc);
        print(" [", name, "+", ((Δhex)(pc - fn.entry())),
            " ", file, ":", line, "]");
    }
}

} // end runtime_package
