// Copyright 2017 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:26:42 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\profbuf.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;


// A profBuf is a lock-free buffer for profiling events,
// safe for concurrent use by one reader and one writer.
// The writer may be a signal handler running without a user g.
// The reader is assumed to be a user g.
//
// Each logged event corresponds to a fixed size header, a list of
// uintptrs (typically a stack), and exactly one unsafe.Pointer tag.
// The header and uintptrs are stored in the circular buffer data and the
// tag is stored in a circular buffer tags, running in parallel.
// In the circular buffer data, each event takes 2+hdrsize+len(stk)
// words: the value 2+hdrsize+len(stk), then the time of the event, then
// hdrsize words giving the fixed-size header, and then len(stk) words
// for the stack.
//
// The current effective offsets into the tags and data circular buffers
// for reading and writing are stored in the high 30 and low 32 bits of r and w.
// The bottom bits of the high 32 are additional flag bits in w, unused in r.
// "Effective" offsets means the total number of reads or writes, mod 2^length.
// The offset in the buffer is the effective offset mod the length of the buffer.
// To make wraparound mod 2^length match wraparound mod length of the buffer,
// the length of the buffer must be a power of two.
//
// If the reader catches up to the writer, a flag passed to read controls
// whether the read blocks until more data is available. A read returns a
// pointer to the buffer data itself; the caller is assumed to be done with
// that data at the next read. The read offset rNext tracks the next offset to
// be returned by read. By definition, r ≤ rNext ≤ w (before wraparound),
// and rNext is only used by the reader, so it can be accessed without atomics.
//
// If the writer gets ahead of the reader, so that the buffer fills,
// future writes are discarded and replaced in the output stream by an
// overflow entry, which has size 2+hdrsize+1, time set to the time of
// the first discarded write, a header of all zeroed words, and a "stack"
// containing one word, the number of discarded writes.
//
// Between the time the buffer fills and the buffer becomes empty enough
// to hold more data, the overflow entry is stored as a pending overflow
// entry in the fields overflow and overflowTime. The pending overflow
// entry can be turned into a real record by either the writer or the
// reader. If the writer is called to write a new record and finds that
// the output buffer has room for both the pending overflow entry and the
// new record, the writer emits the pending overflow entry and the new
// record into the buffer. If the reader is called to read data and finds
// that the output buffer is empty but that there is a pending overflow
// entry, the reader will return a synthesized record for the pending
// overflow entry.
//
// Only the writer can create or add to a pending overflow entry, but
// either the reader or the writer can clear the pending overflow entry.
// A pending overflow entry is indicated by the low 32 bits of 'overflow'
// holding the number of discarded writes, and overflowTime holding the
// time of the first discarded write. The high 32 bits of 'overflow'
// increment each time the low 32 bits transition from zero to non-zero
// or vice versa. This sequence number avoids ABA problems in the use of
// compare-and-swap to coordinate between reader and writer.
// The overflowTime is only written when the low 32 bits of overflow are
// zero, that is, only when there is no pending overflow entry, in
// preparation for creating a new one. The reader can therefore fetch and
// clear the entry atomically using
//
//    for {
//        overflow = load(&b.overflow)
//        if uint32(overflow) == 0 {
//            // no pending entry
//            break
//        }
//        time = load(&b.overflowTime)
//        if cas(&b.overflow, overflow, ((overflow>>32)+1)<<32) {
//            // pending entry cleared
//            break
//        }
//    }
//    if uint32(overflow) > 0 {
//        emit entry for uint32(overflow), time
//    }
//

public static partial class runtime_package {

private partial struct profBuf {
    public profAtomic r;
    public profAtomic w;
    public ulong overflow;
    public ulong overflowTime;
    public uint eof; // immutable (excluding slice content)
    public System.UIntPtr hdrsize;
    public slice<ulong> data;
    public slice<unsafe.Pointer> tags; // owned by reader
    public profIndex rNext;
    public slice<ulong> overflowBuf; // for use by reader to return overflow record
    public note wait;
}

// A profAtomic is the atomically-accessed word holding a profIndex.
private partial struct profAtomic { // : ulong
}

// A profIndex is the packet tag and data counts and flags bits, described above.
private partial struct profIndex { // : ulong
}

private static readonly profIndex profReaderSleeping = 1 << 32; // reader is sleeping and must be woken up
private static readonly profIndex profWriteExtra = 1 << 33; // overflow or eof waiting

private static profIndex load(this ptr<profAtomic> _addr_x) {
    ref profAtomic x = ref _addr_x.val;

    return profIndex(atomic.Load64((uint64.val)(x)));
}

private static void store(this ptr<profAtomic> _addr_x, profIndex @new) {
    ref profAtomic x = ref _addr_x.val;

    atomic.Store64((uint64.val)(x), uint64(new));
}

private static bool cas(this ptr<profAtomic> _addr_x, profIndex old, profIndex @new) {
    ref profAtomic x = ref _addr_x.val;

    return atomic.Cas64((uint64.val)(x), uint64(old), uint64(new));
}

private static uint dataCount(this profIndex x) {
    return uint32(x);
}

private static uint tagCount(this profIndex x) {
    return uint32(x >> 34);
}

// countSub subtracts two counts obtained from profIndex.dataCount or profIndex.tagCount,
// assuming that they are no more than 2^29 apart (guaranteed since they are never more than
// len(data) or len(tags) apart, respectively).
// tagCount wraps at 2^30, while dataCount wraps at 2^32.
// This function works for both.
private static nint countSub(uint x, uint y) { 
    // x-y is 32-bit signed or 30-bit signed; sign-extend to 32 bits and convert to int.
    return int(int32(x - y) << 2 >> 2);
}

// addCountsAndClearFlags returns the packed form of "x + (data, tag) - all flags".
private static profIndex addCountsAndClearFlags(this profIndex x, nint data, nint tag) {
    return profIndex((uint64(x) >> 34 + uint64(uint32(tag) << 2 >> 2)) << 34 | uint64(uint32(x) + uint32(data)));
}

// hasOverflow reports whether b has any overflow records pending.
private static bool hasOverflow(this ptr<profBuf> _addr_b) {
    ref profBuf b = ref _addr_b.val;

    return uint32(atomic.Load64(_addr_b.overflow)) > 0;
}

// takeOverflow consumes the pending overflow records, returning the overflow count
// and the time of the first overflow.
// When called by the reader, it is racing against incrementOverflow.
private static (uint, ulong) takeOverflow(this ptr<profBuf> _addr_b) {
    uint count = default;
    ulong time = default;
    ref profBuf b = ref _addr_b.val;

    var overflow = atomic.Load64(_addr_b.overflow);
    time = atomic.Load64(_addr_b.overflowTime);
    while (true) {
        count = uint32(overflow);
        if (count == 0) {
            time = 0;
            break;
        }
        if (atomic.Cas64(_addr_b.overflow, overflow, ((overflow >> 32) + 1) << 32)) {
            break;
        }
        overflow = atomic.Load64(_addr_b.overflow);
        time = atomic.Load64(_addr_b.overflowTime);
    }
    return (uint32(overflow), time);
}

// incrementOverflow records a single overflow at time now.
// It is racing against a possible takeOverflow in the reader.
private static void incrementOverflow(this ptr<profBuf> _addr_b, long now) {
    ref profBuf b = ref _addr_b.val;

    while (true) {
        var overflow = atomic.Load64(_addr_b.overflow); 

        // Once we see b.overflow reach 0, it's stable: no one else is changing it underfoot.
        // We need to set overflowTime if we're incrementing b.overflow from 0.
        if (uint32(overflow) == 0) { 
            // Store overflowTime first so it's always available when overflow != 0.
            atomic.Store64(_addr_b.overflowTime, uint64(now));
            atomic.Store64(_addr_b.overflow, (((overflow >> 32) + 1) << 32) + 1);
            break;
        }
        if (int32(overflow) == -1) {
            break;
        }
        if (atomic.Cas64(_addr_b.overflow, overflow, overflow + 1)) {
            break;
        }
    }
}

// newProfBuf returns a new profiling buffer with room for
// a header of hdrsize words and a buffer of at least bufwords words.
private static ptr<profBuf> newProfBuf(nint hdrsize, nint bufwords, nint tags) {
    {
        nint min = 2 + hdrsize + 1;

        if (bufwords < min) {
            bufwords = min;
        }
    } 

    // Buffer sizes must be power of two, so that we don't have to
    // worry about uint32 wraparound changing the effective position
    // within the buffers. We store 30 bits of count; limiting to 28
    // gives us some room for intermediate calculations.
    if (bufwords >= 1 << 28 || tags >= 1 << 28) {
        throw("newProfBuf: buffer too large");
    }
    nint i = default;
    i = 1;

    while (i < bufwords) {
        i<<=1;
    }
    bufwords = i;
    i = 1;

    while (i < tags) {
        i<<=1;
    }
    tags = i;

    ptr<profBuf> b = @new<profBuf>();
    b.hdrsize = uintptr(hdrsize);
    b.data = make_slice<ulong>(bufwords);
    b.tags = make_slice<unsafe.Pointer>(tags);
    b.overflowBuf = make_slice<ulong>(2 + b.hdrsize + 1);
    return _addr_b!;
}

// canWriteRecord reports whether the buffer has room
// for a single contiguous record with a stack of length nstk.
private static bool canWriteRecord(this ptr<profBuf> _addr_b, nint nstk) {
    ref profBuf b = ref _addr_b.val;

    var br = b.r.load();
    var bw = b.w.load(); 

    // room for tag?
    if (countSub(br.tagCount(), bw.tagCount()) + len(b.tags) < 1) {
        return false;
    }
    var nd = countSub(br.dataCount(), bw.dataCount()) + len(b.data);
    nint want = 2 + int(b.hdrsize) + nstk;
    var i = int(bw.dataCount() % uint32(len(b.data)));
    if (i + want > len(b.data)) { 
        // Can't fit in trailing fragment of slice.
        // Skip over that and start over at beginning of slice.
        nd -= len(b.data) - i;
    }
    return nd >= want;
}

// canWriteTwoRecords reports whether the buffer has room
// for two records with stack lengths nstk1, nstk2, in that order.
// Each record must be contiguous on its own, but the two
// records need not be contiguous (one can be at the end of the buffer
// and the other can wrap around and start at the beginning of the buffer).
private static bool canWriteTwoRecords(this ptr<profBuf> _addr_b, nint nstk1, nint nstk2) {
    ref profBuf b = ref _addr_b.val;

    var br = b.r.load();
    var bw = b.w.load(); 

    // room for tag?
    if (countSub(br.tagCount(), bw.tagCount()) + len(b.tags) < 2) {
        return false;
    }
    var nd = countSub(br.dataCount(), bw.dataCount()) + len(b.data); 

    // first record
    nint want = 2 + int(b.hdrsize) + nstk1;
    var i = int(bw.dataCount() % uint32(len(b.data)));
    if (i + want > len(b.data)) { 
        // Can't fit in trailing fragment of slice.
        // Skip over that and start over at beginning of slice.
        nd -= len(b.data) - i;
        i = 0;
    }
    i += want;
    nd -= want; 

    // second record
    want = 2 + int(b.hdrsize) + nstk2;
    if (i + want > len(b.data)) { 
        // Can't fit in trailing fragment of slice.
        // Skip over that and start over at beginning of slice.
        nd -= len(b.data) - i;
        i = 0;
    }
    return nd >= want;
}

// write writes an entry to the profiling buffer b.
// The entry begins with a fixed hdr, which must have
// length b.hdrsize, followed by a variable-sized stack
// and a single tag pointer *tagPtr (or nil if tagPtr is nil).
// No write barriers allowed because this might be called from a signal handler.
private static void write(this ptr<profBuf> _addr_b, ptr<unsafe.Pointer> _addr_tagPtr, long now, slice<ulong> hdr, slice<System.UIntPtr> stk) {
    ref profBuf b = ref _addr_b.val;
    ref unsafe.Pointer tagPtr = ref _addr_tagPtr.val;

    if (b == null) {
        return ;
    }
    if (len(hdr) > int(b.hdrsize)) {
        throw("misuse of profBuf.write");
    }
    {
        var hasOverflow = b.hasOverflow();

        if (hasOverflow && b.canWriteTwoRecords(1, len(stk))) { 
            // Room for both an overflow record and the one being written.
            // Write the overflow record if the reader hasn't gotten to it yet.
            // Only racing against reader, not other writers.
            var (count, time) = b.takeOverflow();
            if (count > 0) {
                array<System.UIntPtr> stk = new array<System.UIntPtr>(1);
                stk[0] = uintptr(count);
                b.write(null, int64(time), null, stk[..]);
            }
        }
        else if (hasOverflow || !b.canWriteRecord(len(stk))) { 
            // Pending overflow without room to write overflow and new records
            // or no overflow but also no room for new record.
            b.incrementOverflow(now);
            b.wakeupExtra();
            return ;
        }

    } 

    // There's room: write the record.
    var br = b.r.load();
    var bw = b.w.load(); 

    // Profiling tag
    //
    // The tag is a pointer, but we can't run a write barrier here.
    // We have interrupted the OS-level execution of gp, but the
    // runtime still sees gp as executing. In effect, we are running
    // in place of the real gp. Since gp is the only goroutine that
    // can overwrite gp.labels, the value of gp.labels is stable during
    // this signal handler: it will still be reachable from gp when
    // we finish executing. If a GC is in progress right now, it must
    // keep gp.labels alive, because gp.labels is reachable from gp.
    // If gp were to overwrite gp.labels, the deletion barrier would
    // still shade that pointer, which would preserve it for the
    // in-progress GC, so all is well. Any future GC will see the
    // value we copied when scanning b.tags (heap-allocated).
    // We arrange that the store here is always overwriting a nil,
    // so there is no need for a deletion barrier on b.tags[wt].
    var wt = int(bw.tagCount() % uint32(len(b.tags)));
    if (tagPtr != null) {
        (uintptr.val).val;

        (@unsafe.Pointer(_addr_b.tags[wt])) = uintptr(@unsafe.Pointer(tagPtr));
    }
    var wd = int(bw.dataCount() % uint32(len(b.data)));
    var nd = countSub(br.dataCount(), bw.dataCount()) + len(b.data);
    nint skip = 0;
    if (wd + 2 + int(b.hdrsize) + len(stk) > len(b.data)) {
        b.data[wd] = 0;
        skip = len(b.data) - wd;
        nd -= skip;
        wd = 0;
    }
    var data = b.data[(int)wd..];
    data[0] = uint64(2 + b.hdrsize + uintptr(len(stk))); // length
    data[1] = uint64(now); // time stamp
    // header, zero-padded
    var i = uintptr(copy(data[(int)2..(int)2 + b.hdrsize], hdr));
    while (i < b.hdrsize) {
        data[2 + i] = 0;
        i++;
    }
    {
        var i__prev1 = i;

        foreach (var (__i, __pc) in stk) {
            i = __i;
            pc = __pc;
            data[2 + b.hdrsize + uintptr(i)] = uint64(pc);
        }
        i = i__prev1;
    }

    while (true) { 
        // Commit write.
        // Racing with reader setting flag bits in b.w, to avoid lost wakeups.
        var old = b.w.load();
        var @new = old.addCountsAndClearFlags(skip + 2 + len(stk) + int(b.hdrsize), 1);
        if (!b.w.cas(old, new)) {
            continue;
        }
        if (old & profReaderSleeping != 0) {
            notewakeup(_addr_b.wait);
        }
        break;
    }
}

// close signals that there will be no more writes on the buffer.
// Once all the data has been read from the buffer, reads will return eof=true.
private static void close(this ptr<profBuf> _addr_b) {
    ref profBuf b = ref _addr_b.val;

    if (atomic.Load(_addr_b.eof) > 0) {
        throw("runtime: profBuf already closed");
    }
    atomic.Store(_addr_b.eof, 1);
    b.wakeupExtra();
}

// wakeupExtra must be called after setting one of the "extra"
// atomic fields b.overflow or b.eof.
// It records the change in b.w and wakes up the reader if needed.
private static void wakeupExtra(this ptr<profBuf> _addr_b) {
    ref profBuf b = ref _addr_b.val;

    while (true) {
        var old = b.w.load();
        var @new = old | profWriteExtra;
        if (!b.w.cas(old, new)) {
            continue;
        }
        if (old & profReaderSleeping != 0) {
            notewakeup(_addr_b.wait);
        }
        break;
    }
}

// profBufReadMode specifies whether to block when no data is available to read.
private partial struct profBufReadMode { // : nint
}

private static readonly profBufReadMode profBufBlocking = iota;
private static readonly var profBufNonBlocking = 0;

private static array<unsafe.Pointer> overflowTag = new array<unsafe.Pointer>(1); // always nil

private static (slice<ulong>, slice<unsafe.Pointer>, bool) read(this ptr<profBuf> _addr_b, profBufReadMode mode) {
    slice<ulong> data = default;
    slice<unsafe.Pointer> tags = default;
    bool eof = default;
    ref profBuf b = ref _addr_b.val;

    if (b == null) {
        return (null, null, true);
    }
    var br = b.rNext; 

    // Commit previous read, returning that part of the ring to the writer.
    // First clear tags that have now been read, both to avoid holding
    // up the memory they point at for longer than necessary
    // and so that b.write can assume it is always overwriting
    // nil tag entries (see comment in b.write).
    var rPrev = b.r.load();
    if (rPrev != br) {
        var ntag = countSub(br.tagCount(), rPrev.tagCount());
        var ti = int(rPrev.tagCount() % uint32(len(b.tags)));
        {
            nint i__prev1 = i;

            for (nint i = 0; i < ntag; i++) {
                b.tags[ti] = null;
                ti++;

                if (ti == len(b.tags)) {
                    ti = 0;
                }
            }


            i = i__prev1;
        }
        b.r.store(br);
    }
Read:
    var bw = b.w.load();
    var numData = countSub(bw.dataCount(), br.dataCount());
    if (numData == 0) {
        if (b.hasOverflow()) { 
            // No data to read, but there is overflow to report.
            // Racing with writer flushing b.overflow into a real record.
            var (count, time) = b.takeOverflow();
            if (count == 0) { 
                // Lost the race, go around again.
                goto Read;
            } 
            // Won the race, report overflow.
            var dst = b.overflowBuf;
            dst[0] = uint64(2 + b.hdrsize + 1);
            dst[1] = uint64(time);
            {
                nint i__prev1 = i;

                for (i = uintptr(0); i < b.hdrsize; i++) {
                    dst[2 + i] = 0;
                }


                i = i__prev1;
            }
            dst[2 + b.hdrsize] = uint64(count);
            return (dst[..(int)2 + b.hdrsize + 1], overflowTag[..(int)1], false);
        }
        if (atomic.Load(_addr_b.eof) > 0) { 
            // No data, no overflow, EOF set: done.
            return (null, null, true);
        }
        if (bw & profWriteExtra != 0) { 
            // Writer claims to have published extra information (overflow or eof).
            // Attempt to clear notification and then check again.
            // If we fail to clear the notification it means b.w changed,
            // so we still need to check again.
            b.w.cas(bw, bw & ~profWriteExtra);
            goto Read;
        }
        if (mode == profBufNonBlocking) {
            return (null, null, false);
        }
        if (!b.w.cas(bw, bw | profReaderSleeping)) {
            goto Read;
        }
        notetsleepg(_addr_b.wait, -1);
        noteclear(_addr_b.wait);
        goto Read;
    }
    data = b.data[(int)br.dataCount() % uint32(len(b.data))..];
    if (len(data) > numData) {
        data = data[..(int)numData];
    }
    else
 {
        numData -= len(data); // available in case of wraparound
    }
    nint skip = 0;
    if (data[0] == 0) { 
        // Wraparound record. Go back to the beginning of the ring.
        skip = len(data);
        data = b.data;
        if (len(data) > numData) {
            data = data[..(int)numData];
        }
    }
    ntag = countSub(bw.tagCount(), br.tagCount());
    if (ntag == 0) {
        throw("runtime: malformed profBuf buffer - tag and data out of sync");
    }
    tags = b.tags[(int)br.tagCount() % uint32(len(b.tags))..];
    if (len(tags) > ntag) {
        tags = tags[..(int)ntag];
    }
    nint di = 0;
    ti = 0;
    while (di < len(data) && data[di] != 0 && ti < len(tags)) {
        if (uintptr(di) + uintptr(data[di]) > uintptr(len(data))) {
            throw("runtime: malformed profBuf buffer - invalid size");
        }
        di += int(data[di]);
        ti++;
    } 

    // Remember how much we returned, to commit read on next call.
    b.rNext = br.addCountsAndClearFlags(skip + di, ti);

    if (raceenabled) { 
        // Match racereleasemerge in runtime_setProfLabel,
        // so that the setting of the labels in runtime_setProfLabel
        // is treated as happening before any use of the labels
        // by our caller. The synchronization on labelSync itself is a fiction
        // for the race detector. The actual synchronization is handled
        // by the fact that the signal handler only reads from the current
        // goroutine and uses atomics to write the updated queue indices,
        // and then the read-out from the signal handler buffer uses
        // atomics to read those queue indices.
        raceacquire(@unsafe.Pointer(_addr_labelSync));
    }
    return (data[..(int)di], tags[..(int)ti], false);
}

} // end runtime_package
