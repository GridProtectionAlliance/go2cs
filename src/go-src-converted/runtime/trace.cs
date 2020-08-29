// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Go execution tracer.
// The tracer captures a wide range of execution events like goroutine
// creation/blocking/unblocking, syscall enter/exit/block, GC-related events,
// changes of heap size, processor start/stop, etc and writes them to a buffer
// in a compact form. A precise nanosecond-precision timestamp and a stack
// trace is captured for most events.
// See https://golang.org/s/go15trace for more info.

// package runtime -- go2cs converted at 2020 August 29 08:21:22 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\trace.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Event types in the trace, args are given in square brackets.
        private static readonly long traceEvNone = 0L; // unused
        private static readonly long traceEvBatch = 1L; // start of per-P batch of events [pid, timestamp]
        private static readonly long traceEvFrequency = 2L; // contains tracer timer frequency [frequency (ticks per second)]
        private static readonly long traceEvStack = 3L; // stack [stack id, number of PCs, array of {PC, func string ID, file string ID, line}]
        private static readonly long traceEvGomaxprocs = 4L; // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack id]
        private static readonly long traceEvProcStart = 5L; // start of P [timestamp, thread id]
        private static readonly long traceEvProcStop = 6L; // stop of P [timestamp]
        private static readonly long traceEvGCStart = 7L; // GC start [timestamp, seq, stack id]
        private static readonly long traceEvGCDone = 8L; // GC done [timestamp]
        private static readonly long traceEvGCSTWStart = 9L; // GC STW start [timestamp, kind]
        private static readonly long traceEvGCSTWDone = 10L; // GC STW done [timestamp]
        private static readonly long traceEvGCSweepStart = 11L; // GC sweep start [timestamp, stack id]
        private static readonly long traceEvGCSweepDone = 12L; // GC sweep done [timestamp, swept, reclaimed]
        private static readonly long traceEvGoCreate = 13L; // goroutine creation [timestamp, new goroutine id, new stack id, stack id]
        private static readonly long traceEvGoStart = 14L; // goroutine starts running [timestamp, goroutine id, seq]
        private static readonly long traceEvGoEnd = 15L; // goroutine ends [timestamp]
        private static readonly long traceEvGoStop = 16L; // goroutine stops (like in select{}) [timestamp, stack]
        private static readonly long traceEvGoSched = 17L; // goroutine calls Gosched [timestamp, stack]
        private static readonly long traceEvGoPreempt = 18L; // goroutine is preempted [timestamp, stack]
        private static readonly long traceEvGoSleep = 19L; // goroutine calls Sleep [timestamp, stack]
        private static readonly long traceEvGoBlock = 20L; // goroutine blocks [timestamp, stack]
        private static readonly long traceEvGoUnblock = 21L; // goroutine is unblocked [timestamp, goroutine id, seq, stack]
        private static readonly long traceEvGoBlockSend = 22L; // goroutine blocks on chan send [timestamp, stack]
        private static readonly long traceEvGoBlockRecv = 23L; // goroutine blocks on chan recv [timestamp, stack]
        private static readonly long traceEvGoBlockSelect = 24L; // goroutine blocks on select [timestamp, stack]
        private static readonly long traceEvGoBlockSync = 25L; // goroutine blocks on Mutex/RWMutex [timestamp, stack]
        private static readonly long traceEvGoBlockCond = 26L; // goroutine blocks on Cond [timestamp, stack]
        private static readonly long traceEvGoBlockNet = 27L; // goroutine blocks on network [timestamp, stack]
        private static readonly long traceEvGoSysCall = 28L; // syscall enter [timestamp, stack]
        private static readonly long traceEvGoSysExit = 29L; // syscall exit [timestamp, goroutine id, seq, real timestamp]
        private static readonly long traceEvGoSysBlock = 30L; // syscall blocks [timestamp]
        private static readonly long traceEvGoWaiting = 31L; // denotes that goroutine is blocked when tracing starts [timestamp, goroutine id]
        private static readonly long traceEvGoInSyscall = 32L; // denotes that goroutine is in syscall when tracing starts [timestamp, goroutine id]
        private static readonly long traceEvHeapAlloc = 33L; // memstats.heap_live change [timestamp, heap_alloc]
        private static readonly long traceEvNextGC = 34L; // memstats.next_gc change [timestamp, next_gc]
        private static readonly long traceEvTimerGoroutine = 35L; // denotes timer goroutine [timer goroutine id]
        private static readonly long traceEvFutileWakeup = 36L; // denotes that the previous wakeup of this goroutine was futile [timestamp]
        private static readonly long traceEvString = 37L; // string dictionary entry [ID, length, string]
        private static readonly long traceEvGoStartLocal = 38L; // goroutine starts running on the same P as the last event [timestamp, goroutine id]
        private static readonly long traceEvGoUnblockLocal = 39L; // goroutine is unblocked on the same P as the last event [timestamp, goroutine id, stack]
        private static readonly long traceEvGoSysExitLocal = 40L; // syscall exit on the same P as the last event [timestamp, goroutine id, real timestamp]
        private static readonly long traceEvGoStartLabel = 41L; // goroutine starts running with label [timestamp, goroutine id, seq, label string id]
        private static readonly long traceEvGoBlockGC = 42L; // goroutine blocks on GC assist [timestamp, stack]
        private static readonly long traceEvGCMarkAssistStart = 43L; // GC mark assist start [timestamp, stack]
        private static readonly long traceEvGCMarkAssistDone = 44L; // GC mark assist done [timestamp]
        private static readonly long traceEvCount = 45L;

 
        // Timestamps in trace are cputicks/traceTickDiv.
        // This makes absolute values of timestamp diffs smaller,
        // and so they are encoded in less number of bytes.
        // 64 on x86 is somewhat arbitrary (one tick is ~20ns on a 3GHz machine).
        // The suggested increment frequency for PowerPC's time base register is
        // 512 MHz according to Power ISA v2.07 section 6.2, so we use 16 on ppc64
        // and ppc64le.
        // Tracing won't work reliably for architectures where cputicks is emulated
        // by nanotime, so the value doesn't matter for those architectures.
        private static readonly long traceTickDiv = 16L + 48L * (sys.Goarch386 | sys.GoarchAmd64 | sys.GoarchAmd64p32); 
        // Maximum number of PCs in a single stack trace.
        // Since events contain only stack id rather than whole stack trace,
        // we can allow quite large values here.
        private static readonly long traceStackSize = 128L; 
        // Identifier of a fake P that is used when we trace without a real P.
        private static readonly long traceGlobProc = -1L; 
        // Maximum number of bytes to encode uint64 in base-128.
        private static readonly long traceBytesPerNumber = 10L; 
        // Shift of the number of arguments in the first event byte.
        private static readonly long traceArgCountShift = 6L; 
        // Flag passed to traceGoPark to denote that the previous wakeup of this
        // goroutine was futile. For example, a goroutine was unblocked on a mutex,
        // but another goroutine got ahead and acquired the mutex before the first
        // goroutine is scheduled, so the first goroutine has to block again.
        // Such wakeups happen on buffered channels and sync.Mutex,
        // but are generally not interesting for end user.
        private static readonly byte traceFutileWakeup = 128L;

        // trace is global tracing context.
        private static var trace = default;

        // traceBufHeader is per-P tracing buffer.
        private partial struct traceBufHeader
        {
            public traceBufPtr link; // in trace.empty/full
            public ulong lastTicks; // when we wrote the last event
            public long pos; // next write offset in arr
            public array<System.UIntPtr> stk; // scratch buffer for traceback
        }

        // traceBuf is per-P tracing buffer.
        //
        //go:notinheap
        private partial struct traceBuf
        {
            public ref traceBufHeader traceBufHeader => ref traceBufHeader_val;
            public array<byte> arr; // underlying buffer for traceBufHeader.buf
        }

        // traceBufPtr is a *traceBuf that is not traced by the garbage
        // collector and doesn't have write barriers. traceBufs are not
        // allocated from the GC'd heap, so this is safe, and are often
        // manipulated in contexts where write barriers are not allowed, so
        // this is necessary.
        //
        // TODO: Since traceBuf is now go:notinheap, this isn't necessary.
        private partial struct traceBufPtr // : System.UIntPtr
        {
        }

        private static ref traceBuf ptr(this traceBufPtr tp)
        {
            return (traceBuf.Value)(@unsafe.Pointer(tp));
        }
        private static void set(this ref traceBufPtr tp, ref traceBuf b)
        {
            tp.Value = traceBufPtr(@unsafe.Pointer(b));

        }
        private static traceBufPtr traceBufPtrOf(ref traceBuf b)
        {
            return traceBufPtr(@unsafe.Pointer(b));
        }

        // StartTrace enables tracing for the current process.
        // While tracing, the data will be buffered and available via ReadTrace.
        // StartTrace returns an error if tracing is already enabled.
        // Most clients should use the runtime/trace package or the testing package's
        // -test.trace flag instead of calling StartTrace directly.
        public static error StartTrace()
        { 
            // Stop the world, so that we can take a consistent snapshot
            // of all goroutines at the beginning of the trace.
            stopTheWorld("start tracing"); 

            // We are in stop-the-world, but syscalls can finish and write to trace concurrently.
            // Exitsyscall could check trace.enabled long before and then suddenly wake up
            // and decide to write to trace at a random point in time.
            // However, such syscall will use the global trace.buf buffer, because we've
            // acquired all p's by doing stop-the-world. So this protects us from such races.
            lock(ref trace.bufLock);

            if (trace.enabled || trace.shutdown)
            {
                unlock(ref trace.bufLock);
                startTheWorld();
                return error.As(errorString("tracing is already enabled"));
            } 

            // Can't set trace.enabled yet. While the world is stopped, exitsyscall could
            // already emit a delayed event (see exitTicks in exitsyscall) if we set trace.enabled here.
            // That would lead to an inconsistent trace:
            // - either GoSysExit appears before EvGoInSyscall,
            // - or GoSysExit appears for a goroutine for which we don't emit EvGoInSyscall below.
            // To instruct traceEvent that it must not ignore events below, we set startingtrace.
            // trace.enabled is set afterwards once we have emitted all preliminary events.
            var _g_ = getg();
            _g_.m.startingtrace = true; 

            // Obtain current stack ID to use in all traceEvGoCreate events below.
            var mp = acquirem();
            var stkBuf = make_slice<System.UIntPtr>(traceStackSize);
            var stackID = traceStackID(mp, stkBuf, 2L);
            releasem(mp);

            foreach (var (_, gp) in allgs)
            {
                var status = readgstatus(gp);
                if (status != _Gdead)
                {
                    gp.traceseq = 0L;
                    gp.tracelastp = getg().m.p; 
                    // +PCQuantum because traceFrameForPC expects return PCs and subtracts PCQuantum.
                    var id = trace.stackTab.put(new slice<System.UIntPtr>(new System.UIntPtr[] { gp.startpc+sys.PCQuantum }));
                    traceEvent(traceEvGoCreate, -1L, uint64(gp.goid), uint64(id), stackID);
                }
                if (status == _Gwaiting)
                { 
                    // traceEvGoWaiting is implied to have seq=1.
                    gp.traceseq++;
                    traceEvent(traceEvGoWaiting, -1L, uint64(gp.goid));
                }
                if (status == _Gsyscall)
                {
                    gp.traceseq++;
                    traceEvent(traceEvGoInSyscall, -1L, uint64(gp.goid));
                }
                else
                {
                    gp.sysblocktraced = false;
                }
            }
            traceProcStart();
            traceGoStart(); 
            // Note: ticksStart needs to be set after we emit traceEvGoInSyscall events.
            // If we do it the other way around, it is possible that exitsyscall will
            // query sysexitticks after ticksStart but before traceEvGoInSyscall timestamp.
            // It will lead to a false conclusion that cputicks is broken.
            trace.ticksStart = cputicks();
            trace.timeStart = nanotime();
            trace.headerWritten = false;
            trace.footerWritten = false; 

            // string to id mapping
            //  0 : reserved for an empty string
            //  remaining: other strings registered by traceString
            trace.stringSeq = 0L;
            trace.strings = make_map<@string, ulong>();

            trace.seqGC = 0L;
            _g_.m.startingtrace = false;
            trace.enabled = true; 

            // Register runtime goroutine labels.
            var (_, pid, bufp) = traceAcquireBuffer();
            foreach (var (i, label) in gcMarkWorkerModeStrings[..])
            {
                trace.markWorkerLabels[i], bufp = traceString(bufp, pid, label);
            }
            traceReleaseBuffer(pid);

            unlock(ref trace.bufLock);

            startTheWorld();
            return error.As(null);
        }

        // StopTrace stops tracing, if it was previously enabled.
        // StopTrace only returns after all the reads for the trace have completed.
        public static void StopTrace()
        { 
            // Stop the world so that we can collect the trace buffers from all p's below,
            // and also to avoid races with traceEvent.
            stopTheWorld("stop tracing"); 

            // See the comment in StartTrace.
            lock(ref trace.bufLock);

            if (!trace.enabled)
            {
                unlock(ref trace.bufLock);
                startTheWorld();
                return;
            }
            traceGoSched(); 

            // Loop over all allocated Ps because dead Ps may still have
            // trace buffers.
            {
                var p__prev1 = p;

                foreach (var (_, __p) in allp[..cap(allp)])
                {
                    p = __p;
                    var buf = p.tracebuf;
                    if (buf != 0L)
                    {
                        traceFullQueue(buf);
                        p.tracebuf = 0L;
                    }
                }

                p = p__prev1;
            }

            if (trace.buf != 0L)
            {
                buf = trace.buf;
                trace.buf = 0L;
                if (buf.ptr().pos != 0L)
                {
                    traceFullQueue(buf);
                }
            }
            while (true)
            {
                trace.ticksEnd = cputicks();
                trace.timeEnd = nanotime(); 
                // Windows time can tick only every 15ms, wait for at least one tick.
                if (trace.timeEnd != trace.timeStart)
                {
                    break;
                }
                osyield();
            }


            trace.enabled = false;
            trace.shutdown = true;
            unlock(ref trace.bufLock);

            startTheWorld(); 

            // The world is started but we've set trace.shutdown, so new tracing can't start.
            // Wait for the trace reader to flush pending buffers and stop.
            semacquire(ref trace.shutdownSema);
            if (raceenabled)
            {
                raceacquire(@unsafe.Pointer(ref trace.shutdownSema));
            } 

            // The lock protects us from races with StartTrace/StopTrace because they do stop-the-world.
            lock(ref trace.@lock);
            {
                var p__prev1 = p;

                foreach (var (_, __p) in allp[..cap(allp)])
                {
                    p = __p;
                    if (p.tracebuf != 0L)
                    {
                        throw("trace: non-empty trace buffer in proc");
                    }
                }

                p = p__prev1;
            }

            if (trace.buf != 0L)
            {
                throw("trace: non-empty global trace buffer");
            }
            if (trace.fullHead != 0L || trace.fullTail != 0L)
            {
                throw("trace: non-empty full trace buffer");
            }
            if (trace.reading != 0L || trace.reader != 0L)
            {
                throw("trace: reading after shutdown");
            }
            while (trace.empty != 0L)
            {
                buf = trace.empty;
                trace.empty = buf.ptr().link;
                sysFree(@unsafe.Pointer(buf), @unsafe.Sizeof(buf.ptr().Value), ref memstats.other_sys);
            }

            trace.strings = null;
            trace.shutdown = false;
            unlock(ref trace.@lock);
        }

        // ReadTrace returns the next chunk of binary tracing data, blocking until data
        // is available. If tracing is turned off and all the data accumulated while it
        // was on has been returned, ReadTrace returns nil. The caller must copy the
        // returned data before calling ReadTrace again.
        // ReadTrace must be called from one goroutine at a time.
        public static slice<byte> ReadTrace()
        { 
            // This function may need to lock trace.lock recursively
            // (goparkunlock -> traceGoPark -> traceEvent -> traceFlush).
            // To allow this we use trace.lockOwner.
            // Also this function must not allocate while holding trace.lock:
            // allocation can call heap allocate, which will try to emit a trace
            // event while holding heap lock.
            lock(ref trace.@lock);
            trace.lockOwner = getg();

            if (trace.reader != 0L)
            { 
                // More than one goroutine reads trace. This is bad.
                // But we rather do not crash the program because of tracing,
                // because tracing can be enabled at runtime on prod servers.
                trace.lockOwner = null;
                unlock(ref trace.@lock);
                println("runtime: ReadTrace called from multiple goroutines simultaneously");
                return null;
            } 
            // Recycle the old buffer.
            {
                var buf__prev1 = buf;

                var buf = trace.reading;

                if (buf != 0L)
                {
                    buf.ptr().link = trace.empty;
                    trace.empty = buf;
                    trace.reading = 0L;
                } 
                // Write trace header.

                buf = buf__prev1;

            } 
            // Write trace header.
            if (!trace.headerWritten)
            {
                trace.headerWritten = true;
                trace.lockOwner = null;
                unlock(ref trace.@lock);
                return (slice<byte>)"go 1.10 trace\x00\x00\x00";
            } 
            // Wait for new data.
            if (trace.fullHead == 0L && !trace.shutdown)
            {
                trace.reader.set(getg());
                goparkunlock(ref trace.@lock, "trace reader (blocked)", traceEvGoBlock, 2L);
                lock(ref trace.@lock);
            } 
            // Write a buffer.
            if (trace.fullHead != 0L)
            {
                buf = traceFullDequeue();
                trace.reading = buf;
                trace.lockOwner = null;
                unlock(ref trace.@lock);
                return buf.ptr().arr[..buf.ptr().pos];
            } 
            // Write footer with timer frequency.
            if (!trace.footerWritten)
            {
                trace.footerWritten = true; 
                // Use float64 because (trace.ticksEnd - trace.ticksStart) * 1e9 can overflow int64.
                var freq = float64(trace.ticksEnd - trace.ticksStart) * 1e9F / float64(trace.timeEnd - trace.timeStart) / traceTickDiv;
                trace.lockOwner = null;
                unlock(ref trace.@lock);
                slice<byte> data = default;
                data = append(data, traceEvFrequency | 0L << (int)(traceArgCountShift));
                data = traceAppend(data, uint64(freq));
                foreach (var (i) in timers)
                {
                    var tb = ref timers[i];
                    if (tb.gp != null)
                    {
                        data = append(data, traceEvTimerGoroutine | 0L << (int)(traceArgCountShift));
                        data = traceAppend(data, uint64(tb.gp.goid));
                    }
                } 
                // This will emit a bunch of full buffers, we will pick them up
                // on the next iteration.
                trace.stackTab.dump();
                return data;
            } 
            // Done.
            if (trace.shutdown)
            {
                trace.lockOwner = null;
                unlock(ref trace.@lock);
                if (raceenabled)
                { 
                    // Model synchronization on trace.shutdownSema, which race
                    // detector does not see. This is required to avoid false
                    // race reports on writer passed to trace.Start.
                    racerelease(@unsafe.Pointer(ref trace.shutdownSema));
                } 
                // trace.enabled is already reset, so can call traceable functions.
                semrelease(ref trace.shutdownSema);
                return null;
            } 
            // Also bad, but see the comment above.
            trace.lockOwner = null;
            unlock(ref trace.@lock);
            println("runtime: spurious wakeup of trace reader");
            return null;
        }

        // traceReader returns the trace reader that should be woken up, if any.
        private static ref g traceReader()
        {
            if (trace.reader == 0L || (trace.fullHead == 0L && !trace.shutdown))
            {
                return null;
            }
            lock(ref trace.@lock);
            if (trace.reader == 0L || (trace.fullHead == 0L && !trace.shutdown))
            {
                unlock(ref trace.@lock);
                return null;
            }
            var gp = trace.reader.ptr();
            trace.reader.set(null);
            unlock(ref trace.@lock);
            return gp;
        }

        // traceProcFree frees trace buffer associated with pp.
        private static void traceProcFree(ref p pp)
        {
            var buf = pp.tracebuf;
            pp.tracebuf = 0L;
            if (buf == 0L)
            {
                return;
            }
            lock(ref trace.@lock);
            traceFullQueue(buf);
            unlock(ref trace.@lock);
        }

        // traceFullQueue queues buf into queue of full buffers.
        private static void traceFullQueue(traceBufPtr buf)
        {
            buf.ptr().link = 0L;
            if (trace.fullHead == 0L)
            {
                trace.fullHead = buf;
            }
            else
            {
                trace.fullTail.ptr().link = buf;
            }
            trace.fullTail = buf;
        }

        // traceFullDequeue dequeues from queue of full buffers.
        private static traceBufPtr traceFullDequeue()
        {
            var buf = trace.fullHead;
            if (buf == 0L)
            {
                return 0L;
            }
            trace.fullHead = buf.ptr().link;
            if (trace.fullHead == 0L)
            {
                trace.fullTail = 0L;
            }
            buf.ptr().link = 0L;
            return buf;
        }

        // traceEvent writes a single event to trace buffer, flushing the buffer if necessary.
        // ev is event type.
        // If skip > 0, write current stack id as the last argument (skipping skip top frames).
        // If skip = 0, this event type should contain a stack, but we don't want
        // to collect and remember it for this particular call.
        private static void traceEvent(byte ev, long skip, params ulong[] args)
        {
            args = args.Clone();

            var (mp, pid, bufp) = traceAcquireBuffer(); 
            // Double-check trace.enabled now that we've done m.locks++ and acquired bufLock.
            // This protects from races between traceEvent and StartTrace/StopTrace.

            // The caller checked that trace.enabled == true, but trace.enabled might have been
            // turned off between the check and now. Check again. traceLockBuffer did mp.locks++,
            // StopTrace does stopTheWorld, and stopTheWorld waits for mp.locks to go back to zero,
            // so if we see trace.enabled == true now, we know it's true for the rest of the function.
            // Exitsyscall can run even during stopTheWorld. The race with StartTrace/StopTrace
            // during tracing in exitsyscall is resolved by locking trace.bufLock in traceLockBuffer.
            if (!trace.enabled && !mp.startingtrace)
            {
                traceReleaseBuffer(pid);
                return;
            }
            object buf = ref bufp();
            const long maxSize = 2L + 5L * traceBytesPerNumber; // event type, length, sequence, timestamp, stack id and two add params
 // event type, length, sequence, timestamp, stack id and two add params
            if (buf == null || len(buf.arr) - buf.pos < maxSize)
            {
                buf = traceFlush(traceBufPtrOf(buf), pid).ptr()(bufp.Value).set(buf);
            }
            var ticks = uint64(cputicks()) / traceTickDiv;
            var tickDiff = ticks - buf.lastTicks;
            buf.lastTicks = ticks;
            var narg = byte(len(args));
            if (skip >= 0L)
            {
                narg++;
            } 
            // We have only 2 bits for number of arguments.
            // If number is >= 3, then the event type is followed by event length in bytes.
            if (narg > 3L)
            {
                narg = 3L;
            }
            var startPos = buf.pos;
            buf.@byte(ev | narg << (int)(traceArgCountShift));
            ref byte lenp = default;
            if (narg == 3L)
            { 
                // Reserve the byte for length assuming that length < 128.
                buf.varint(0L);
                lenp = ref buf.arr[buf.pos - 1L];
            }
            buf.varint(tickDiff);
            foreach (var (_, a) in args)
            {
                buf.varint(a);
            }
            if (skip == 0L)
            {
                buf.varint(0L);
            }
            else if (skip > 0L)
            {
                buf.varint(traceStackID(mp, buf.stk[..], skip));
            }
            var evSize = buf.pos - startPos;
            if (evSize > maxSize)
            {
                throw("invalid length of trace event");
            }
            if (lenp != null)
            { 
                // Fill in actual length.
                lenp.Value = byte(evSize - 2L);
            }
            traceReleaseBuffer(pid);
        }

        private static ulong traceStackID(ref m mp, slice<System.UIntPtr> buf, long skip)
        {
            var _g_ = getg();
            var gp = mp.curg;
            long nstk = default;
            if (gp == _g_)
            {
                nstk = callers(skip + 1L, buf[..]);
            }
            else if (gp != null)
            {
                gp = mp.curg;
                nstk = gcallers(gp, skip, buf[..]);
            }
            if (nstk > 0L)
            {
                nstk--; // skip runtime.goexit
            }
            if (nstk > 0L && gp.goid == 1L)
            {
                nstk--; // skip runtime.main
            }
            var id = trace.stackTab.put(buf[..nstk]);
            return uint64(id);
        }

        // traceAcquireBuffer returns trace buffer to use and, if necessary, locks it.
        private static (ref m, int, ref traceBufPtr) traceAcquireBuffer()
        {
            mp = acquirem();
            {
                var p = mp.p.ptr();

                if (p != null)
                {
                    return (mp, p.id, ref p.tracebuf);
                }

            }
            lock(ref trace.bufLock);
            return (mp, traceGlobProc, ref trace.buf);
        }

        // traceReleaseBuffer releases a buffer previously acquired with traceAcquireBuffer.
        private static void traceReleaseBuffer(int pid)
        {
            if (pid == traceGlobProc)
            {
                unlock(ref trace.bufLock);
            }
            releasem(getg().m);
        }

        // traceFlush puts buf onto stack of full buffers and returns an empty buffer.
        private static traceBufPtr traceFlush(traceBufPtr buf, int pid)
        {
            var owner = trace.lockOwner;
            var dolock = owner == null || owner != getg().m.curg;
            if (dolock)
            {
                lock(ref trace.@lock);
            }
            if (buf != 0L)
            {
                traceFullQueue(buf);
            }
            if (trace.empty != 0L)
            {
                buf = trace.empty;
                trace.empty = buf.ptr().link;
            }
            else
            {
                buf = traceBufPtr(sysAlloc(@unsafe.Sizeof(new traceBuf()), ref memstats.other_sys));
                if (buf == 0L)
                {
                    throw("trace: out of memory");
                }
            }
            var bufp = buf.ptr();
            bufp.link.set(null);
            bufp.pos = 0L; 

            // initialize the buffer for a new batch
            var ticks = uint64(cputicks()) / traceTickDiv;
            bufp.lastTicks = ticks;
            bufp.@byte(traceEvBatch | 1L << (int)(traceArgCountShift));
            bufp.varint(uint64(pid));
            bufp.varint(ticks);

            if (dolock)
            {
                unlock(ref trace.@lock);
            }
            return buf;
        }

        // traceString adds a string to the trace.strings and returns the id.
        private static (ulong, ref traceBufPtr) traceString(ref traceBufPtr bufp, int pid, @string s)
        {
            if (s == "")
            {
                return (0L, bufp);
            }
            {
                var id__prev1 = id;

                var (id, ok) = trace.strings[s];

                if (ok)
                {
                    return (id, bufp);
                }

                id = id__prev1;

            }

            trace.stringSeq++;
            var id = trace.stringSeq;
            trace.strings[s] = id; 

            // memory allocation in above may trigger tracing and
            // cause *bufp changes. Following code now works with *bufp,
            // so there must be no memory allocation or any activities
            // that causes tracing after this point.

            object buf = ref bufp();
            long size = 1L + 2L * traceBytesPerNumber + len(s);
            if (buf == null || len(buf.arr) - buf.pos < size)
            {
                buf = traceFlush(traceBufPtrOf(buf), pid).ptr()(bufp.Value).set(buf);
            }
            buf.@byte(traceEvString);
            buf.varint(id);
            buf.varint(uint64(len(s)));
            buf.pos += copy(buf.arr[buf.pos..], s)(bufp.Value).set(buf);
            return (id, bufp);
        }

        // traceAppend appends v to buf in little-endian-base-128 encoding.
        private static slice<byte> traceAppend(slice<byte> buf, ulong v)
        {
            while (v >= 0x80UL)
            {
                buf = append(buf, 0x80UL | byte(v));
                v >>= 7L;
            }

            buf = append(buf, byte(v));
            return buf;
        }

        // varint appends v to buf in little-endian-base-128 encoding.
        private static void varint(this ref traceBuf buf, ulong v)
        {
            var pos = buf.pos;
            while (v >= 0x80UL)
            {
                buf.arr[pos] = 0x80UL | byte(v);
                pos++;
                v >>= 7L;
            }

            buf.arr[pos] = byte(v);
            pos++;
            buf.pos = pos;
        }

        // byte appends v to buf.
        private static void @byte(this ref traceBuf buf, byte v)
        {
            buf.arr[buf.pos] = v;
            buf.pos++;
        }

        // traceStackTable maps stack traces (arrays of PC's) to unique uint32 ids.
        // It is lock-free for reading.
        private partial struct traceStackTable
        {
            public mutex @lock;
            public uint seq;
            public traceAlloc mem;
            public array<traceStackPtr> tab;
        }

        // traceStack is a single stack in traceStackTable.
        private partial struct traceStack
        {
            public traceStackPtr link;
            public System.UIntPtr hash;
            public uint id;
            public long n;
            public array<System.UIntPtr> stk; // real type [n]uintptr
        }

        private partial struct traceStackPtr // : System.UIntPtr
        {
        }

        private static ref traceStack ptr(this traceStackPtr tp)
        {
            return (traceStack.Value)(@unsafe.Pointer(tp));
        }

        // stack returns slice of PCs.
        private static slice<System.UIntPtr> stack(this ref traceStack ts)
        {
            return new ptr<ref array<System.UIntPtr>>(@unsafe.Pointer(ref ts.stk))[..ts.n];
        }

        // put returns a unique id for the stack trace pcs and caches it in the table,
        // if it sees the trace for the first time.
        private static uint put(this ref traceStackTable tab, slice<System.UIntPtr> pcs)
        {
            if (len(pcs) == 0L)
            {
                return 0L;
            }
            var hash = memhash(@unsafe.Pointer(ref pcs[0L]), 0L, uintptr(len(pcs)) * @unsafe.Sizeof(pcs[0L])); 
            // First, search the hashtable w/o the mutex.
            {
                var id__prev1 = id;

                var id = tab.find(pcs, hash);

                if (id != 0L)
                {
                    return id;
                } 
                // Now, double check under the mutex.

                id = id__prev1;

            } 
            // Now, double check under the mutex.
            lock(ref tab.@lock);
            {
                var id__prev1 = id;

                id = tab.find(pcs, hash);

                if (id != 0L)
                {
                    unlock(ref tab.@lock);
                    return id;
                } 
                // Create new record.

                id = id__prev1;

            } 
            // Create new record.
            tab.seq++;
            var stk = tab.newStack(len(pcs));
            stk.hash = hash;
            stk.id = tab.seq;
            stk.n = len(pcs);
            var stkpc = stk.stack();
            foreach (var (i, pc) in pcs)
            {
                stkpc[i] = pc;
            }
            var part = int(hash % uintptr(len(tab.tab)));
            stk.link = tab.tab[part];
            atomicstorep(@unsafe.Pointer(ref tab.tab[part]), @unsafe.Pointer(stk));
            unlock(ref tab.@lock);
            return stk.id;
        }

        // find checks if the stack trace pcs is already present in the table.
        private static uint find(this ref traceStackTable tab, slice<System.UIntPtr> pcs, System.UIntPtr hash)
        {
            var part = int(hash % uintptr(len(tab.tab)));
Search:
            {
                var stk = tab.tab[part].ptr();

                while (stk != null)
                {
                    if (stk.hash == hash && stk.n == len(pcs))
                    {
                        foreach (var (i, stkpc) in stk.stack())
                        {
                            if (stkpc != pcs[i])
                            {
                                _continueSearch = true;
                                break;
                            }
                    stk = stk.link.ptr();
                        }
                        return stk.id;
                    }
                }

            }
            return 0L;
        }

        // newStack allocates a new stack of size n.
        private static ref traceStack newStack(this ref traceStackTable tab, long n)
        {
            return (traceStack.Value)(tab.mem.alloc(@unsafe.Sizeof(new traceStack()) + uintptr(n) * sys.PtrSize));
        }

        // allFrames returns all of the Frames corresponding to pcs.
        private static slice<Frame> allFrames(slice<System.UIntPtr> pcs)
        {
            var frames = make_slice<Frame>(0L, len(pcs));
            var ci = CallersFrames(pcs);
            while (true)
            {
                var (f, more) = ci.Next();
                frames = append(frames, f);
                if (!more)
                {
                    return frames;
                }
            }

        }

        // dump writes all previously cached stacks to trace buffers,
        // releases all memory and resets state.
        private static void dump(this ref traceStackTable tab)
        {
            array<byte> tmp = new array<byte>((2L + 4L * traceStackSize) * traceBytesPerNumber);
            var bufp = traceFlush(0L, 0L);
            {
                var stk__prev1 = stk;

                foreach (var (_, __stk) in tab.tab)
                {
                    stk = __stk;
                    var stk = stk.ptr();
                    while (stk != null)
                    {
                        var tmpbuf = tmp[..0L];
                        tmpbuf = traceAppend(tmpbuf, uint64(stk.id));
                        var frames = allFrames(stk.stack());
                        tmpbuf = traceAppend(tmpbuf, uint64(len(frames)));
                        foreach (var (_, f) in frames)
                        {
                            traceFrame frame = default;
                            frame, bufp = traceFrameForPC(bufp, 0L, f);
                            tmpbuf = traceAppend(tmpbuf, uint64(f.PC));
                            tmpbuf = traceAppend(tmpbuf, uint64(frame.funcID));
                            tmpbuf = traceAppend(tmpbuf, uint64(frame.fileID));
                            tmpbuf = traceAppend(tmpbuf, uint64(frame.line));
                        } 
                        // Now copy to the buffer.
                        long size = 1L + traceBytesPerNumber + len(tmpbuf);
                        {
                            var buf__prev1 = buf;

                            var buf = bufp.ptr();

                            if (len(buf.arr) - buf.pos < size)
                            {
                                bufp = traceFlush(bufp, 0L);
                        stk = stk.link.ptr();
                            }

                            buf = buf__prev1;

                        }
                        buf = bufp.ptr();
                        buf.@byte(traceEvStack | 3L << (int)(traceArgCountShift));
                        buf.varint(uint64(len(tmpbuf)));
                        buf.pos += copy(buf.arr[buf.pos..], tmpbuf);
                    }

                }

                stk = stk__prev1;
            }

            lock(ref trace.@lock);
            traceFullQueue(bufp);
            unlock(ref trace.@lock);

            tab.mem.drop();
            tab.Value = new traceStackTable();
        }

        private partial struct traceFrame
        {
            public ulong funcID;
            public ulong fileID;
            public ulong line;
        }

        // traceFrameForPC records the frame information.
        // It may allocate memory.
        private static (traceFrame, traceBufPtr) traceFrameForPC(traceBufPtr buf, int pid, Frame f)
        {
            var bufp = ref buf;
            traceFrame frame = default;

            var fn = f.Function;
            const long maxLen = 1L << (int)(10L);

            if (len(fn) > maxLen)
            {
                fn = fn[len(fn) - maxLen..];
            }
            frame.funcID, bufp = traceString(bufp, pid, fn);
            frame.line = uint64(f.Line);
            var file = f.File;
            if (len(file) > maxLen)
            {
                file = file[len(file) - maxLen..];
            }
            frame.fileID, bufp = traceString(bufp, pid, file);
            return (frame, (bufp.Value));
        }

        // traceAlloc is a non-thread-safe region allocator.
        // It holds a linked list of traceAllocBlock.
        private partial struct traceAlloc
        {
            public traceAllocBlockPtr head;
            public System.UIntPtr off;
        }

        // traceAllocBlock is a block in traceAlloc.
        //
        // traceAllocBlock is allocated from non-GC'd memory, so it must not
        // contain heap pointers. Writes to pointers to traceAllocBlocks do
        // not need write barriers.
        //
        //go:notinheap
        private partial struct traceAllocBlock
        {
            public traceAllocBlockPtr next;
            public array<byte> data;
        }

        // TODO: Since traceAllocBlock is now go:notinheap, this isn't necessary.
        private partial struct traceAllocBlockPtr // : System.UIntPtr
        {
        }

        private static ref traceAllocBlock ptr(this traceAllocBlockPtr p)
        {
            return (traceAllocBlock.Value)(@unsafe.Pointer(p));
        }
        private static void set(this ref traceAllocBlockPtr p, ref traceAllocBlock x)
        {
            p.Value = traceAllocBlockPtr(@unsafe.Pointer(x));

        }

        // alloc allocates n-byte block.
        private static unsafe.Pointer alloc(this ref traceAlloc a, System.UIntPtr n)
        {
            n = round(n, sys.PtrSize);
            if (a.head == 0L || a.off + n > uintptr(len(a.head.ptr().data)))
            {
                if (n > uintptr(len(a.head.ptr().data)))
                {
                    throw("trace: alloc too large");
                }
                var block = (traceAllocBlock.Value)(sysAlloc(@unsafe.Sizeof(new traceAllocBlock()), ref memstats.other_sys));
                if (block == null)
                {
                    throw("trace: out of memory");
                }
                block.next.set(a.head.ptr());
                a.head.set(block);
                a.off = 0L;
            }
            var p = ref a.head.ptr().data[a.off];
            a.off += n;
            return @unsafe.Pointer(p);
        }

        // drop frees all previously allocated memory and resets the allocator.
        private static void drop(this ref traceAlloc a)
        {
            while (a.head != 0L)
            {
                var block = a.head.ptr();
                a.head.set(block.next.ptr());
                sysFree(@unsafe.Pointer(block), @unsafe.Sizeof(new traceAllocBlock()), ref memstats.other_sys);
            }

        }

        // The following functions write specific events to trace.

        private static void traceGomaxprocs(int procs)
        {
            traceEvent(traceEvGomaxprocs, 1L, uint64(procs));
        }

        private static void traceProcStart()
        {
            traceEvent(traceEvProcStart, -1L, uint64(getg().m.id));
        }

        private static void traceProcStop(ref p pp)
        { 
            // Sysmon and stopTheWorld can stop Ps blocked in syscalls,
            // to handle this we temporary employ the P.
            var mp = acquirem();
            var oldp = mp.p;
            mp.p.set(pp);
            traceEvent(traceEvProcStop, -1L);
            mp.p = oldp;
            releasem(mp);
        }

        private static void traceGCStart()
        {
            traceEvent(traceEvGCStart, 3L, trace.seqGC);
            trace.seqGC++;
        }

        private static void traceGCDone()
        {
            traceEvent(traceEvGCDone, -1L);
        }

        private static void traceGCSTWStart(long kind)
        {
            traceEvent(traceEvGCSTWStart, -1L, uint64(kind));
        }

        private static void traceGCSTWDone()
        {
            traceEvent(traceEvGCSTWDone, -1L);
        }

        // traceGCSweepStart prepares to trace a sweep loop. This does not
        // emit any events until traceGCSweepSpan is called.
        //
        // traceGCSweepStart must be paired with traceGCSweepDone and there
        // must be no preemption points between these two calls.
        private static void traceGCSweepStart()
        { 
            // Delay the actual GCSweepStart event until the first span
            // sweep. If we don't sweep anything, don't emit any events.
            var _p_ = getg().m.p.ptr();
            if (_p_.traceSweep)
            {
                throw("double traceGCSweepStart");
            }
            _p_.traceSweep = true;
            _p_.traceSwept = 0L;
            _p_.traceReclaimed = 0L;
        }

        // traceGCSweepSpan traces the sweep of a single page.
        //
        // This may be called outside a traceGCSweepStart/traceGCSweepDone
        // pair; however, it will not emit any trace events in this case.
        private static void traceGCSweepSpan(System.UIntPtr bytesSwept)
        {
            var _p_ = getg().m.p.ptr();
            if (_p_.traceSweep)
            {
                if (_p_.traceSwept == 0L)
                {
                    traceEvent(traceEvGCSweepStart, 1L);
                }
                _p_.traceSwept += bytesSwept;
            }
        }

        private static void traceGCSweepDone()
        {
            var _p_ = getg().m.p.ptr();
            if (!_p_.traceSweep)
            {
                throw("missing traceGCSweepStart");
            }
            if (_p_.traceSwept != 0L)
            {
                traceEvent(traceEvGCSweepDone, -1L, uint64(_p_.traceSwept), uint64(_p_.traceReclaimed));
            }
            _p_.traceSweep = false;
        }

        private static void traceGCMarkAssistStart()
        {
            traceEvent(traceEvGCMarkAssistStart, 1L);
        }

        private static void traceGCMarkAssistDone()
        {
            traceEvent(traceEvGCMarkAssistDone, -1L);
        }

        private static void traceGoCreate(ref g newg, System.UIntPtr pc)
        {
            newg.traceseq = 0L;
            newg.tracelastp = getg().m.p; 
            // +PCQuantum because traceFrameForPC expects return PCs and subtracts PCQuantum.
            var id = trace.stackTab.put(new slice<System.UIntPtr>(new System.UIntPtr[] { pc+sys.PCQuantum }));
            traceEvent(traceEvGoCreate, 2L, uint64(newg.goid), uint64(id));
        }

        private static void traceGoStart()
        {
            var _g_ = getg().m.curg;
            var _p_ = _g_.m.p;
            _g_.traceseq++;
            if (_g_ == _p_.ptr().gcBgMarkWorker.ptr())
            {
                traceEvent(traceEvGoStartLabel, -1L, uint64(_g_.goid), _g_.traceseq, trace.markWorkerLabels[_p_.ptr().gcMarkWorkerMode]);
            }
            else if (_g_.tracelastp == _p_)
            {
                traceEvent(traceEvGoStartLocal, -1L, uint64(_g_.goid));
            }
            else
            {
                _g_.tracelastp = _p_;
                traceEvent(traceEvGoStart, -1L, uint64(_g_.goid), _g_.traceseq);
            }
        }

        private static void traceGoEnd()
        {
            traceEvent(traceEvGoEnd, -1L);
        }

        private static void traceGoSched()
        {
            var _g_ = getg();
            _g_.tracelastp = _g_.m.p;
            traceEvent(traceEvGoSched, 1L);
        }

        private static void traceGoPreempt()
        {
            var _g_ = getg();
            _g_.tracelastp = _g_.m.p;
            traceEvent(traceEvGoPreempt, 1L);
        }

        private static void traceGoPark(byte traceEv, long skip)
        {
            if (traceEv & traceFutileWakeup != 0L)
            {
                traceEvent(traceEvFutileWakeup, -1L);
            }
            traceEvent(traceEv & ~traceFutileWakeup, skip);
        }

        private static void traceGoUnpark(ref g gp, long skip)
        {
            var _p_ = getg().m.p;
            gp.traceseq++;
            if (gp.tracelastp == _p_)
            {
                traceEvent(traceEvGoUnblockLocal, skip, uint64(gp.goid));
            }
            else
            {
                gp.tracelastp = _p_;
                traceEvent(traceEvGoUnblock, skip, uint64(gp.goid), gp.traceseq);
            }
        }

        private static void traceGoSysCall()
        {
            traceEvent(traceEvGoSysCall, 1L);
        }

        private static void traceGoSysExit(long ts)
        {
            if (ts != 0L && ts < trace.ticksStart)
            { 
                // There is a race between the code that initializes sysexitticks
                // (in exitsyscall, which runs without a P, and therefore is not
                // stopped with the rest of the world) and the code that initializes
                // a new trace. The recorded sysexitticks must therefore be treated
                // as "best effort". If they are valid for this trace, then great,
                // use them for greater accuracy. But if they're not valid for this
                // trace, assume that the trace was started after the actual syscall
                // exit (but before we actually managed to start the goroutine,
                // aka right now), and assign a fresh time stamp to keep the log consistent.
                ts = 0L;
            }
            var _g_ = getg().m.curg;
            _g_.traceseq++;
            _g_.tracelastp = _g_.m.p;
            traceEvent(traceEvGoSysExit, -1L, uint64(_g_.goid), _g_.traceseq, uint64(ts) / traceTickDiv);
        }

        private static void traceGoSysBlock(ref p pp)
        { 
            // Sysmon and stopTheWorld can declare syscalls running on remote Ps as blocked,
            // to handle this we temporary employ the P.
            var mp = acquirem();
            var oldp = mp.p;
            mp.p.set(pp);
            traceEvent(traceEvGoSysBlock, -1L);
            mp.p = oldp;
            releasem(mp);
        }

        private static void traceHeapAlloc()
        {
            traceEvent(traceEvHeapAlloc, -1L, memstats.heap_live);
        }

        private static void traceNextGC()
        {
            if (memstats.next_gc == ~uint64(0L))
            { 
                // Heap-based triggering is disabled.
                traceEvent(traceEvNextGC, -1L, 0L);
            }
            else
            {
                traceEvent(traceEvNextGC, -1L, memstats.next_gc);
            }
        }
    }
}
