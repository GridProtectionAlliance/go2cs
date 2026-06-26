// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

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
//	for {
//		overflow = load(&b.overflow)
//		if uint32(overflow) == 0 {
//			// no pending entry
//			break
//		}
//		time = load(&b.overflowTime)
//		if cas(&b.overflow, overflow, ((overflow>>32)+1)<<32) {
//			// pending entry cleared
//			break
//		}
//	}
//	if uint32(overflow) > 0 {
//		emit entry for uint32(overflow), time
//	}
[GoType] partial struct profBuf {
    // accessed atomically
    internal profAtomic r;
    internal profAtomic w;
    internal @internal.runtime.atomic_package.Uint64 overflow;
    internal @internal.runtime.atomic_package.Uint64 overflowTime;
    internal @internal.runtime.atomic_package.Uint32 eof;
    // immutable (excluding slice content)
    internal uintptr hdrsize;
    internal slice<uint64> data;
    internal slice<@unsafe.Pointer> tags;
    // owned by reader
    internal profIndex rNext;
    internal slice<uint64> overflowBuf; // for use by reader to return overflow record
    internal note wait;
}

[GoType("num:uint64")] partial struct profAtomic;

[GoType("num:uint64")] partial struct profIndex;

internal static readonly profIndex profReaderSleeping = /* 1 << 32 */ 4294967296;   // reader is sleeping and must be woken up
internal static readonly profIndex profWriteExtra = /* 1 << 33 */ 8589934592;       // overflow or eof waiting

[GoRecv] internal static profIndex load(this ref profAtomic x) {
    return ((profIndex)atomic.Load64(((ж<uint64>)x)));
}

[GoRecv] internal static void store(this ref profAtomic x, profIndex @new) {
    atomic.Store64(((ж<uint64>)x), ((uint64)@new));
}

[GoRecv] internal static bool cas(this ref profAtomic x, profIndex old, profIndex @new) {
    return atomic.Cas64(((ж<uint64>)x), ((uint64)old), ((uint64)@new));
}

internal static uint32 dataCount(this profIndex x) {
    return ((uint32)x);
}

internal static uint32 tagCount(this profIndex x) {
    return ((uint32)(x >> (int)(34)));
}

// countSub subtracts two counts obtained from profIndex.dataCount or profIndex.tagCount,
// assuming that they are no more than 2^29 apart (guaranteed since they are never more than
// len(data) or len(tags) apart, respectively).
// tagCount wraps at 2^30, while dataCount wraps at 2^32.
// This function works for both.
internal static nint countSub(uint32 x, uint32 y) {
    // x-y is 32-bit signed or 30-bit signed; sign-extend to 32 bits and convert to int.
    return ((nint)(((int32)(x - y)) << (int)(2) >> (int)(2)));
}

// addCountsAndClearFlags returns the packed form of "x + (data, tag) - all flags".
internal static profIndex addCountsAndClearFlags(this profIndex x, nint data, nint tag) {
    return ((profIndex)((uint64)((((uint64)x) >> (int)(34) + ((uint64)(((uint32)tag) << (int)(2) >> (int)(2)))) << (int)(34) | ((uint64)(((uint32)x) + ((uint32)data))))));
}

// hasOverflow reports whether b has any overflow records pending.
[GoRecv] internal static bool hasOverflow(this ref profBuf b) {
    return ((uint32)b.overflow.Load()) > 0;
}

// takeOverflow consumes the pending overflow records, returning the overflow count
// and the time of the first overflow.
// When called by the reader, it is racing against incrementOverflow.
[GoRecv] internal static (uint32 count, uint64 time) takeOverflow(this ref profBuf b) {
    uint32 count = default!;
    uint64 time = default!;

    var overflow = b.overflow.Load();
    time = b.overflowTime.Load();
    while (ᐧ) {
        count = ((uint32)overflow);
        if (count == 0) {
            time = 0;
            break;
        }
        // Increment generation, clear overflow count in low bits.
        if (b.overflow.CompareAndSwap(overflow, ((overflow >> (int)(32)) + 1) << (int)(32))) {
            break;
        }
        overflow = b.overflow.Load();
        time = b.overflowTime.Load();
    }
    return (((uint32)overflow), time);
}

// incrementOverflow records a single overflow at time now.
// It is racing against a possible takeOverflow in the reader.
[GoRecv] internal static void incrementOverflow(this ref profBuf b, int64 now) {
    while (ᐧ) {
        var overflow = b.overflow.Load();
        // Once we see b.overflow reach 0, it's stable: no one else is changing it underfoot.
        // We need to set overflowTime if we're incrementing b.overflow from 0.
        if (((uint32)overflow) == 0) {
            // Store overflowTime first so it's always available when overflow != 0.
            b.overflowTime.Store(((uint64)now));
            b.overflow.Store((((overflow >> (int)(32)) + 1) << (int)(32)) + 1);
            break;
        }
        // Otherwise we're racing to increment against reader
        // who wants to set b.overflow to 0.
        // Out of paranoia, leave 2³²-1 a sticky overflow value,
        // to avoid wrapping around. Extremely unlikely.
        if (((int32)overflow) == -1) {
            break;
        }
        if (b.overflow.CompareAndSwap(overflow, overflow + 1)) {
            break;
        }
    }
}

// newProfBuf returns a new profiling buffer with room for
// a header of hdrsize words and a buffer of at least bufwords words.
internal static ж<profBuf> newProfBuf(nint hdrsize, nint bufwords, nint tags) {
    {
        nint min = 2 + hdrsize + 1; if (bufwords < min) {
            bufwords = min;
        }
    }
    // Buffer sizes must be power of two, so that we don't have to
    // worry about uint32 wraparound changing the effective position
    // within the buffers. We store 30 bits of count; limiting to 28
    // gives us some room for intermediate calculations.
    if (bufwords >= 1 << (int)(28) || tags >= 1 << (int)(28)) {
        @throw("newProfBuf: buffer too large"u8);
    }
    nint i = default!;
    for (i = 1; i < bufwords; i <<= (UntypedInt)(1)) {
    }
    bufwords = i;
    for (i = 1; i < tags; i <<= (UntypedInt)(1)) {
    }
    tags = i;
    var b = @new<profBuf>();
    b.val.hdrsize = ((uintptr)hdrsize);
    b.val.data = new slice<uint64>(bufwords);
    b.val.tags = new slice<@unsafe.Pointer>(tags);
    b.val.overflowBuf = new slice<uint64>(2 + (~b).hdrsize + 1);
    return b;
}

// canWriteRecord reports whether the buffer has room
// for a single contiguous record with a stack of length nstk.
[GoRecv] internal static bool canWriteRecord(this ref profBuf b, nint nstk) {
    var br = b.r.load();
    var bw = b.w.load();
    // room for tag?
    if (countSub(br.tagCount(), bw.tagCount()) + len(b.tags) < 1) {
        return false;
    }
    // room for data?
    nint nd = countSub(br.dataCount(), bw.dataCount()) + len(b.data);
    nint want = 2 + ((nint)b.hdrsize) + nstk;
    nint i = ((nint)(bw.dataCount() % ((uint32)len(b.data))));
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
[GoRecv] internal static bool canWriteTwoRecords(this ref profBuf b, nint nstk1, nint nstk2) {
    var br = b.r.load();
    var bw = b.w.load();
    // room for tag?
    if (countSub(br.tagCount(), bw.tagCount()) + len(b.tags) < 2) {
        return false;
    }
    // room for data?
    nint nd = countSub(br.dataCount(), bw.dataCount()) + len(b.data);
    // first record
    nint want = 2 + ((nint)b.hdrsize) + nstk1;
    nint i = ((nint)(bw.dataCount() % ((uint32)len(b.data))));
    if (i + want > len(b.data)) {
        // Can't fit in trailing fragment of slice.
        // Skip over that and start over at beginning of slice.
        nd -= len(b.data) - i;
        i = 0;
    }
    i += want;
    nd -= want;
    // second record
    want = 2 + ((nint)b.hdrsize) + nstk2;
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
[GoRecv] internal static void write(this ref profBuf b, ж<@unsafe.Pointer> ᏑtagPtr, int64 now, slice<uint64> hdr, slice<uintptr> stk) {
    ref var tagPtr = ref ᏑtagPtr.val;

    if (b == nil) {
        return;
    }
    if (len(hdr) > ((nint)b.hdrsize)) {
        @throw("misuse of profBuf.write"u8);
    }
    {
        var hasOverflow = b.hasOverflow(); if (hasOverflow && b.canWriteTwoRecords(1, len(stkΔ1))){
            // Room for both an overflow record and the one being written.
            // Write the overflow record if the reader hasn't gotten to it yet.
            // Only racing against reader, not other writers.
            var (count, time) = b.takeOverflow();
            if (count > 0) {
                array<uintptr> stkΔ2 = new(1);
                stkΔ2[0] = ((uintptr)count);
                b.write(nil, ((int64)time), default!, stkΔ2[..]);
            }
        } else 
        if (hasOverflow || !b.canWriteRecord(len(stkΔ1))) {
            // Pending overflow without room to write overflow and new records
            // or no overflow but also no room for new record.
            b.incrementOverflow(now);
            b.wakeupExtra();
            return;
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
    nint wt = ((nint)(bw.tagCount() % ((uint32)len(b.tags))));
    if (tagPtr != nil) {
        ((ж<uintptr>)(uintptr)(((@unsafe.Pointer)(Ꮡ(b.tags[wt]))))).val = ((uintptr)(tagPtr));
    }
    // Main record.
    // It has to fit in a contiguous section of the slice, so if it doesn't fit at the end,
    // leave a rewind marker (0) and start over at the beginning of the slice.
    nint wd = ((nint)(bw.dataCount() % ((uint32)len(b.data))));
    nint nd = countSub(br.dataCount(), bw.dataCount()) + len(b.data);
    nint skip = 0;
    if (wd + 2 + ((nint)b.hdrsize) + len(stkΔ1) > len(b.data)) {
        b.data[wd] = 0;
        skip = len(b.data) - wd;
        nd -= skip;
        wd = 0;
    }
    var data = b.data[(int)(wd)..];
    data[0] = ((uint64)(2 + b.hdrsize + ((uintptr)len(stkΔ1))));
    // length
    data[1] = ((uint64)now);
    // time stamp
    // header, zero-padded
    nint i = copy(data[2..(int)(2 + b.hdrsize)], hdr);
    clear(data[(int)(2 + i)..(int)(2 + b.hdrsize)]);
    foreach (var (iΔ1, pc) in stkΔ1) {
        data[2 + b.hdrsize + ((uintptr)iΔ1)] = ((uint64)pc);
    }
    while (ᐧ) {
        // Commit write.
        // Racing with reader setting flag bits in b.w, to avoid lost wakeups.
        var old = b.w.load();
        var @new = old.addCountsAndClearFlags(skip + 2 + len(stkΔ1) + ((nint)b.hdrsize), 1);
        if (!b.w.cas(old, @new)) {
            continue;
        }
        // If there was a reader, wake it up.
        if ((profIndex)(old & profReaderSleeping) != 0) {
            notewakeup(Ꮡ(b.wait));
        }
        break;
    }
}

// close signals that there will be no more writes on the buffer.
// Once all the data has been read from the buffer, reads will return eof=true.
[GoRecv] internal static void close(this ref profBuf b) {
    if (b.eof.Load() > 0) {
        @throw("runtime: profBuf already closed"u8);
    }
    b.eof.Store(1);
    b.wakeupExtra();
}

// wakeupExtra must be called after setting one of the "extra"
// atomic fields b.overflow or b.eof.
// It records the change in b.w and wakes up the reader if needed.
[GoRecv] internal static void wakeupExtra(this ref profBuf b) {
    while (ᐧ) {
        var old = b.w.load();
        var @new = (profIndex)(old | profWriteExtra);
        if (!b.w.cas(old, @new)) {
            continue;
        }
        if ((profIndex)(old & profReaderSleeping) != 0) {
            notewakeup(Ꮡ(b.wait));
        }
        break;
    }
}

[GoType("num:nint")] partial struct profBufReadMode;

internal static readonly profBufReadMode profBufBlocking = /* iota */ 0;
internal static readonly profBufReadMode profBufNonBlocking = 1;

internal static array<@unsafe.Pointer> overflowTag;           // always nil

[GoRecv] internal static (slice<uint64> data, slice<@unsafe.Pointer> tags, bool eof) read(this ref profBuf b, profBufReadMode mode) {
    slice<uint64> data = default!;
    slice<@unsafe.Pointer> tags = default!;
    bool eof = default!;

    if (b == nil) {
        return (default!, default!, true);
    }
    var br = b.rNext;
    // Commit previous read, returning that part of the ring to the writer.
    // First clear tags that have now been read, both to avoid holding
    // up the memory they point at for longer than necessary
    // and so that b.write can assume it is always overwriting
    // nil tag entries (see comment in b.write).
    var rPrev = b.r.load();
    if (rPrev != br) {
        nint ntagΔ1 = countSub(br.tagCount(), rPrev.tagCount());
        nint tiΔ1 = ((nint)(rPrev.tagCount() % ((uint32)len(b.tags))));
        for (nint i = 0; i < ntagΔ1; i++) {
            b.tags[ti] = default!;
            {
                tiΔ1++; if (tiΔ1 == len(b.tags)) {
                    tiΔ1 = 0;
                }
            }
        }
        b.r.store(br);
    }
Read:
    var bw = b.w.load();
    nint numData = countSub(bw.dataCount(), br.dataCount());
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
            dst[0] = ((uint64)(2 + b.hdrsize + 1));
            dst[1] = time;
            clear(dst[2..(int)(2 + b.hdrsize)]);
            dst[2 + b.hdrsize] = ((uint64)count);
            return (dst[..(int)(2 + b.hdrsize + 1)], overflowTag[..1], false);
        }
        if (b.eof.Load() > 0) {
            // No data, no overflow, EOF set: done.
            return (default!, default!, true);
        }
        if ((profIndex)(bw & profWriteExtra) != 0) {
            // Writer claims to have published extra information (overflow or eof).
            // Attempt to clear notification and then check again.
            // If we fail to clear the notification it means b.w changed,
            // so we still need to check again.
            b.w.cas(bw, (profIndex)(bw & ~profWriteExtra));
            goto Read;
        }
        // Nothing to read right now.
        // Return or sleep according to mode.
        if (mode == profBufNonBlocking) {
            // Necessary on Darwin, notetsleepg below does not work in signal handler, root cause of #61768.
            return (default!, default!, false);
        }
        if (!b.w.cas(bw, (profIndex)(bw | profReaderSleeping))) {
            goto Read;
        }
        // Committed to sleeping.
        notetsleepg(Ꮡ(b.wait), -1);
        noteclear(Ꮡ(b.wait));
        goto Read;
    }
    data = b.data[(int)(br.dataCount() % ((uint32)len(b.data)))..];
    if (len(data) > numData){
        data = data[..(int)(numData)];
    } else {
        numData -= len(data);
    }
    // available in case of wraparound
    nint skip = 0;
    if (data[0] == 0) {
        // Wraparound record. Go back to the beginning of the ring.
        skip = len(data);
        data = b.data;
        if (len(data) > numData) {
            data = data[..(int)(numData)];
        }
    }
    nint ntag = countSub(bw.tagCount(), br.tagCount());
    if (ntag == 0) {
        @throw("runtime: malformed profBuf buffer - tag and data out of sync"u8);
    }
    tags = b.tags[(int)(br.tagCount() % ((uint32)len(b.tags)))..];
    if (len(tags) > ntag) {
        tags = tags[..(int)(ntag)];
    }
    // Count out whole data records until either data or tags is done.
    // They are always in sync in the buffer, but due to an end-of-slice
    // wraparound we might need to stop early and return the rest
    // in the next call.
    nint di = 0;
    nint ti = 0;
    while (di < len(data) && data[di] != 0 && ti < len(tags)) {
        if (((uintptr)di) + ((uintptr)data[di]) > ((uintptr)len(data))) {
            @throw("runtime: malformed profBuf buffer - invalid size"u8);
        }
        di += ((nint)data[di]);
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
        raceacquire(((@unsafe.Pointer)(Ꮡ(labelSync))));
    }
    return (data[..(int)(di)], tags[..(int)(ti)], false);
}

} // end runtime_package
