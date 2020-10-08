// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Code to check that pointer writes follow the cgo rules.
// These functions are invoked via the write barrier when debug.cgocheck > 1.

// package runtime -- go2cs converted at 2020 October 08 03:19:12 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\cgocheck.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly @string cgoWriteBarrierFail = (@string)"Go pointer stored into non-Go memory";

        // cgoCheckWriteBarrier is called whenever a pointer is stored into memory.
        // It throws if the program is storing a Go pointer into non-Go memory.
        //
        // This is called from the write barrier, so its entire call tree must
        // be nosplit.
        //
        //go:nosplit
        //go:nowritebarrier


        // cgoCheckWriteBarrier is called whenever a pointer is stored into memory.
        // It throws if the program is storing a Go pointer into non-Go memory.
        //
        // This is called from the write barrier, so its entire call tree must
        // be nosplit.
        //
        //go:nosplit
        //go:nowritebarrier
        private static void cgoCheckWriteBarrier(ptr<System.UIntPtr> _addr_dst, System.UIntPtr src)
        {
            ref System.UIntPtr dst = ref _addr_dst.val;

            if (!cgoIsGoPointer(@unsafe.Pointer(src)))
            {
                return ;
            }

            if (cgoIsGoPointer(@unsafe.Pointer(dst)))
            {
                return ;
            } 

            // If we are running on the system stack then dst might be an
            // address on the stack, which is OK.
            var g = getg();
            if (g == g.m.g0 || g == g.m.gsignal)
            {
                return ;
            } 

            // Allocating memory can write to various mfixalloc structs
            // that look like they are non-Go memory.
            if (g.m.mallocing != 0L)
            {
                return ;
            } 

            // It's OK if writing to memory allocated by persistentalloc.
            // Do this check last because it is more expensive and rarely true.
            // If it is false the expense doesn't matter since we are crashing.
            if (inPersistentAlloc(uintptr(@unsafe.Pointer(dst))))
            {
                return ;
            }

            systemstack(() =>
            {
                println("write of Go pointer", hex(src), "to non-Go memory", hex(uintptr(@unsafe.Pointer(dst))));
                throw(cgoWriteBarrierFail);
            });

        }

        // cgoCheckMemmove is called when moving a block of memory.
        // dst and src point off bytes into the value to copy.
        // size is the number of bytes to copy.
        // It throws if the program is copying a block that contains a Go pointer
        // into non-Go memory.
        //go:nosplit
        //go:nowritebarrier
        private static void cgoCheckMemmove(ptr<_type> _addr_typ, unsafe.Pointer dst, unsafe.Pointer src, System.UIntPtr off, System.UIntPtr size)
        {
            ref _type typ = ref _addr_typ.val;

            if (typ.ptrdata == 0L)
            {
                return ;
            }

            if (!cgoIsGoPointer(src))
            {
                return ;
            }

            if (cgoIsGoPointer(dst))
            {
                return ;
            }

            cgoCheckTypedBlock(_addr_typ, src, off, size);

        }

        // cgoCheckSliceCopy is called when copying n elements of a slice.
        // src and dst are pointers to the first element of the slice.
        // typ is the element type of the slice.
        // It throws if the program is copying slice elements that contain Go pointers
        // into non-Go memory.
        //go:nosplit
        //go:nowritebarrier
        private static void cgoCheckSliceCopy(ptr<_type> _addr_typ, unsafe.Pointer dst, unsafe.Pointer src, long n)
        {
            ref _type typ = ref _addr_typ.val;

            if (typ.ptrdata == 0L)
            {
                return ;
            }

            if (!cgoIsGoPointer(src))
            {
                return ;
            }

            if (cgoIsGoPointer(dst))
            {
                return ;
            }

            var p = src;
            for (long i = 0L; i < n; i++)
            {
                cgoCheckTypedBlock(_addr_typ, p, 0L, typ.size);
                p = add(p, typ.size);
            }


        }

        // cgoCheckTypedBlock checks the block of memory at src, for up to size bytes,
        // and throws if it finds a Go pointer. The type of the memory is typ,
        // and src is off bytes into that type.
        //go:nosplit
        //go:nowritebarrier
        private static void cgoCheckTypedBlock(ptr<_type> _addr_typ, unsafe.Pointer src, System.UIntPtr off, System.UIntPtr size)
        {
            ref _type typ = ref _addr_typ.val;
 
            // Anything past typ.ptrdata is not a pointer.
            if (typ.ptrdata <= off)
            {
                return ;
            }

            {
                var ptrdataSize = typ.ptrdata - off;

                if (size > ptrdataSize)
                {
                    size = ptrdataSize;
                }

            }


            if (typ.kind & kindGCProg == 0L)
            {
                cgoCheckBits(src, _addr_typ.gcdata, off, size);
                return ;
            } 

            // The type has a GC program. Try to find GC bits somewhere else.
            foreach (var (_, datap) in activeModules())
            {
                if (cgoInRange(src, datap.data, datap.edata))
                {
                    var doff = uintptr(src) - datap.data;
                    cgoCheckBits(add(src, -doff), _addr_datap.gcdatamask.bytedata, off + doff, size);
                    return ;
                }

                if (cgoInRange(src, datap.bss, datap.ebss))
                {
                    var boff = uintptr(src) - datap.bss;
                    cgoCheckBits(add(src, -boff), _addr_datap.gcbssmask.bytedata, off + boff, size);
                    return ;
                }

            }
            var s = spanOfUnchecked(uintptr(src));
            if (s.state.get() == mSpanManual)
            { 
                // There are no heap bits for value stored on the stack.
                // For a channel receive src might be on the stack of some
                // other goroutine, so we can't unwind the stack even if
                // we wanted to.
                // We can't expand the GC program without extra storage
                // space we can't easily get.
                // Fortunately we have the type information.
                systemstack(() =>
                {
                    cgoCheckUsingType(_addr_typ, src, off, size);
                });
                return ;

            } 

            // src must be in the regular heap.
            var hbits = heapBitsForAddr(uintptr(src));
            {
                var i = uintptr(0L);

                while (i < off + size)
                {
                    var bits = hbits.bits();
                    if (i >= off && bits & bitPointer != 0L)
                    {
                        ptr<ptr<unsafe.Pointer>> v = new ptr<ptr<ptr<unsafe.Pointer>>>(add(src, i));
                        if (cgoIsGoPointer(v))
                        {
                            throw(cgoWriteBarrierFail);
                    i += sys.PtrSize;
                        }

                    }

                    hbits = hbits.next();

                }

            }

        }

        // cgoCheckBits checks the block of memory at src, for up to size
        // bytes, and throws if it finds a Go pointer. The gcbits mark each
        // pointer value. The src pointer is off bytes into the gcbits.
        //go:nosplit
        //go:nowritebarrier
        private static void cgoCheckBits(unsafe.Pointer src, ptr<byte> _addr_gcbits, System.UIntPtr off, System.UIntPtr size)
        {
            ref byte gcbits = ref _addr_gcbits.val;

            var skipMask = off / sys.PtrSize / 8L;
            var skipBytes = skipMask * sys.PtrSize * 8L;
            var ptrmask = addb(gcbits, skipMask);
            src = add(src, skipBytes);
            off -= skipBytes;
            size += off;
            uint bits = default;
            {
                var i = uintptr(0L);

                while (i < size)
                {
                    if (i & (sys.PtrSize * 8L - 1L) == 0L)
                    {
                        bits = uint32(ptrmask.val);
                        ptrmask = addb(ptrmask, 1L);
                    i += sys.PtrSize;
                    }
                    else
                    {
                        bits >>= 1L;
                    }

                    if (off > 0L)
                    {
                        off -= sys.PtrSize;
                    }
                    else
                    {
                        if (bits & 1L != 0L)
                        {
                            ptr<ptr<unsafe.Pointer>> v = new ptr<ptr<ptr<unsafe.Pointer>>>(add(src, i));
                            if (cgoIsGoPointer(v))
                            {
                                throw(cgoWriteBarrierFail);
                            }

                        }

                    }

                }

            }

        }

        // cgoCheckUsingType is like cgoCheckTypedBlock, but is a last ditch
        // fall back to look for pointers in src using the type information.
        // We only use this when looking at a value on the stack when the type
        // uses a GC program, because otherwise it's more efficient to use the
        // GC bits. This is called on the system stack.
        //go:nowritebarrier
        //go:systemstack
        private static void cgoCheckUsingType(ptr<_type> _addr_typ, unsafe.Pointer src, System.UIntPtr off, System.UIntPtr size)
        {
            ref _type typ = ref _addr_typ.val;

            if (typ.ptrdata == 0L)
            {
                return ;
            } 

            // Anything past typ.ptrdata is not a pointer.
            if (typ.ptrdata <= off)
            {
                return ;
            }

            {
                var ptrdataSize = typ.ptrdata - off;

                if (size > ptrdataSize)
                {
                    size = ptrdataSize;
                }

            }


            if (typ.kind & kindGCProg == 0L)
            {
                cgoCheckBits(src, _addr_typ.gcdata, off, size);
                return ;
            }


            if (typ.kind & kindMask == kindArray) 
                var at = (arraytype.val)(@unsafe.Pointer(typ));
                for (var i = uintptr(0L); i < at.len; i++)
                {
                    if (off < at.elem.size)
                    {
                        cgoCheckUsingType(_addr_at.elem, src, off, size);
                    }

                    src = add(src, at.elem.size);
                    var skipped = off;
                    if (skipped > at.elem.size)
                    {
                        skipped = at.elem.size;
                    }

                    var @checked = at.elem.size - skipped;
                    off -= skipped;
                    if (size <= checked)
                    {
                        return ;
                    }

                    size -= checked;

                }
            else if (typ.kind & kindMask == kindStruct) 
                var st = (structtype.val)(@unsafe.Pointer(typ));
                foreach (var (_, f) in st.fields)
                {
                    if (off < f.typ.size)
                    {
                        cgoCheckUsingType(_addr_f.typ, src, off, size);
                    }

                    src = add(src, f.typ.size);
                    skipped = off;
                    if (skipped > f.typ.size)
                    {
                        skipped = f.typ.size;
                    }

                    @checked = f.typ.size - skipped;
                    off -= skipped;
                    if (size <= checked)
                    {
                        return ;
                    }

                    size -= checked;

                }
            else 
                throw("can't happen");
            
        }
    }
}
