// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:17:48 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mem_plan9.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static readonly var memDebug = false;



        private static System.UIntPtr bloc = default;
        private static mutex memlock = default;

        private partial struct memHdr
        {
            public memHdrPtr next;
            public System.UIntPtr size;
        }

        private static memHdrPtr memFreelist = default; // sorted in ascending order

        private partial struct memHdrPtr // : System.UIntPtr
        {
        }

        private static ref memHdr ptr(this memHdrPtr p)
        {
            return (memHdr.Value)(@unsafe.Pointer(p));
        }
        private static void set(this ref memHdrPtr p, ref memHdr x)
        {
            p.Value = memHdrPtr(@unsafe.Pointer(x));

        }

        private static unsafe.Pointer memAlloc(System.UIntPtr n)
        {
            n = memRound(n);
            ref memHdr prevp = default;
            {
                var p = memFreelist.ptr();

                while (p != null)
                {
                    if (p.size >= n)
                    {
                        if (p.size == n)
                        {
                            if (prevp != null)
                            {
                                prevp.next = p.next;
                    p = p.next.ptr();
                            }
                            else
                            {
                                memFreelist = p.next;
                            }
                        }
                        else
                        {
                            p.size -= n;
                            p = (memHdr.Value)(add(@unsafe.Pointer(p), p.size));
                        }
                        p.Value = new memHdr();
                        return @unsafe.Pointer(p);
                    }
                    prevp = p;
                }

            }
            return sbrk(n);
        }

        private static void memFree(unsafe.Pointer ap, System.UIntPtr n)
        {
            n = memRound(n);
            memclrNoHeapPointers(ap, n);
            var bp = (memHdr.Value)(ap);
            bp.size = n;
            var bpn = uintptr(ap);
            if (memFreelist == 0L)
            {
                bp.next = 0L;
                memFreelist.set(bp);
                return;
            }
            var p = memFreelist.ptr();
            if (bpn < uintptr(@unsafe.Pointer(p)))
            {
                memFreelist.set(bp);
                if (bpn + bp.size == uintptr(@unsafe.Pointer(p)))
                {
                    bp.size += p.size;
                    bp.next = p.next;
                    p.Value = new memHdr();
                }
                else
                {
                    bp.next.set(p);
                }
                return;
            }
            while (p.next != 0L)
            {
                if (bpn > uintptr(@unsafe.Pointer(p)) && bpn < uintptr(@unsafe.Pointer(p.next)))
                {
                    break;
                p = p.next.ptr();
                }
            }

            if (bpn + bp.size == uintptr(@unsafe.Pointer(p.next)))
            {
                bp.size += p.next.ptr().size;
                bp.next = p.next.ptr().next;
                p.next.ptr().Value = new memHdr();
            }
            else
            {
                bp.next = p.next;
            }
            if (uintptr(@unsafe.Pointer(p)) + p.size == bpn)
            {
                p.size += bp.size;
                p.next = bp.next;
                bp.Value = new memHdr();
            }
            else
            {
                p.next.set(bp);
            }
        }

        private static void memCheck()
        {
            if (memDebug == false)
            {
                return;
            }
            {
                var p = memFreelist.ptr();

                while (p != null && p.next != 0L)
                {
                    if (uintptr(@unsafe.Pointer(p)) == uintptr(@unsafe.Pointer(p.next)))
                    {
                        print("runtime: ", @unsafe.Pointer(p), " == ", @unsafe.Pointer(p.next), "\n");
                        throw("mem: infinite loop");
                    p = p.next.ptr();
                    }
                    if (uintptr(@unsafe.Pointer(p)) > uintptr(@unsafe.Pointer(p.next)))
                    {
                        print("runtime: ", @unsafe.Pointer(p), " > ", @unsafe.Pointer(p.next), "\n");
                        throw("mem: unordered list");
                    }
                    if (uintptr(@unsafe.Pointer(p)) + p.size > uintptr(@unsafe.Pointer(p.next)))
                    {
                        print("runtime: ", @unsafe.Pointer(p), "+", p.size, " > ", @unsafe.Pointer(p.next), "\n");
                        throw("mem: overlapping blocks");
                    }
                    {
                        var b = add(@unsafe.Pointer(p), @unsafe.Sizeof(new memHdr()));

                        while (uintptr(b) < uintptr(@unsafe.Pointer(p)) + p.size)
                        {
                            if (b.Value != 0L)
                            {
                                print("runtime: value at addr ", b, " with offset ", uintptr(b) - uintptr(@unsafe.Pointer(p)), " in block ", p, " of size ", p.size, " is not zero\n");
                                throw("mem: uninitialised memory");
                            b = add(b, 1L);
                            }
                        }

                    }
                }

            }
        }

        private static System.UIntPtr memRound(System.UIntPtr p)
        {
            return (p + _PAGESIZE - 1L) & ~(_PAGESIZE - 1L);
        }

        private static void initBloc()
        {
            bloc = memRound(firstmoduledata.end);
        }

        private static unsafe.Pointer sbrk(System.UIntPtr n)
        { 
            // Plan 9 sbrk from /sys/src/libc/9sys/sbrk.c
            var bl = bloc;
            n = memRound(n);
            if (brk_(@unsafe.Pointer(bl + n)) < 0L)
            {
                return null;
            }
            bloc += n;
            return @unsafe.Pointer(bl);
        }

        private static unsafe.Pointer sysAlloc(System.UIntPtr n, ref ulong sysStat)
        {
            lock(ref memlock);
            var p = memAlloc(n);
            memCheck();
            unlock(ref memlock);
            if (p != null)
            {
                mSysStatInc(sysStat, n);
            }
            return p;
        }

        private static void sysFree(unsafe.Pointer v, System.UIntPtr n, ref ulong sysStat)
        {
            mSysStatDec(sysStat, n);
            lock(ref memlock);
            memFree(v, n);
            memCheck();
            unlock(ref memlock);
        }

        private static void sysUnused(unsafe.Pointer v, System.UIntPtr n)
        {
        }

        private static void sysUsed(unsafe.Pointer v, System.UIntPtr n)
        {
        }

        private static void sysMap(unsafe.Pointer v, System.UIntPtr n, bool reserved, ref ulong sysStat)
        { 
            // sysReserve has already allocated all heap memory,
            // but has not adjusted stats.
            mSysStatInc(sysStat, n);
        }

        private static void sysFault(unsafe.Pointer v, System.UIntPtr n)
        {
        }

        private static unsafe.Pointer sysReserve(unsafe.Pointer v, System.UIntPtr n, ref bool reserved)
        {
            reserved.Value = true;
            lock(ref memlock);
            var p = memAlloc(n);
            memCheck();
            unlock(ref memlock);
            return p;
        }
    }
}
