// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 09:58:29 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\unsafe.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // global pointer slot
        private static ref array<ulong> a = default;

        // unfoldable true
        private static var b = true;

        // Test to make sure that a pointer value which is alive
        // across a call is retained, even when there are matching
        // conversions to/from uintptr around the call.
        // We arrange things very carefully to have to/from
        // conversions on either side of the call which cannot be
        // combined with any other conversions.
        private static ref array<ulong> f_ssa()
        { 
            // Make x a uintptr pointing to where a points.
            System.UIntPtr x = default;
            if (b)
            {
                x = uintptr(@unsafe.Pointer(a));
            }
            else
            {
                x = 0L;
            } 
            // Clobber the global pointer. The only live ref
            // to the allocated object is now x.
            a = null; 

            // Convert to pointer so it should hold
            // the object live across GC call.
            var p = @unsafe.Pointer(x); 

            // Call gc.
            runtime.GC(); 

            // Convert back to uintptr.
            var y = uintptr(p); 

            // Mess with y so that the subsequent cast
            // to unsafe.Pointer can't be combined with the
            // uintptr cast above.
            System.UIntPtr z = default;
            if (b)
            {
                z = y;
            }
            else
            {
                z = 0L;
            }
            return new ptr<ref array<ulong>>(@unsafe.Pointer(z));
        }

        // g_ssa is the same as f_ssa, but with a bit of pointer
        // arithmetic for added insanity.
        private static ref array<ulong> g_ssa()
        { 
            // Make x a uintptr pointing to where a points.
            System.UIntPtr x = default;
            if (b)
            {
                x = uintptr(@unsafe.Pointer(a));
            }
            else
            {
                x = 0L;
            } 
            // Clobber the global pointer. The only live ref
            // to the allocated object is now x.
            a = null; 

            // Offset x by one int.
            x += @unsafe.Sizeof(int(0L)); 

            // Convert to pointer so it should hold
            // the object live across GC call.
            var p = @unsafe.Pointer(x); 

            // Call gc.
            runtime.GC(); 

            // Convert back to uintptr.
            var y = uintptr(p); 

            // Mess with y so that the subsequent cast
            // to unsafe.Pointer can't be combined with the
            // uintptr cast above.
            System.UIntPtr z = default;
            if (b)
            {
                z = y;
            }
            else
            {
                z = 0L;
            }
            return new ptr<ref array<ulong>>(@unsafe.Pointer(z));
        }

        private static void testf() => func((_, panic, __) =>
        {
            a = @new<array<ulong>>();
            {
                long i__prev1 = i;

                for (long i = 0L; i < 8L; i++)
                {
                    a[i] = 0xabcdUL;
                }


                i = i__prev1;
            }
            var c = f_ssa();
            {
                long i__prev1 = i;

                for (i = 0L; i < 8L; i++)
                {
                    if (c[i] != 0xabcdUL)
                    {
                        fmt.Printf("%d:%x\n", i, c[i]);
                        panic("bad c");
                    }
                }


                i = i__prev1;
            }
        });

        private static void testg() => func((_, panic, __) =>
        {
            a = @new<array<ulong>>();
            {
                long i__prev1 = i;

                for (long i = 0L; i < 8L; i++)
                {
                    a[i] = 0xabcdUL;
                }


                i = i__prev1;
            }
            var c = g_ssa();
            {
                long i__prev1 = i;

                for (i = 0L; i < 7L; i++)
                {
                    if (c[i] != 0xabcdUL)
                    {
                        fmt.Printf("%d:%x\n", i, c[i]);
                        panic("bad c");
                    }
                }


                i = i__prev1;
            }
        });

        private static uint alias_ssa(ref ulong ui64, ref uint ui32)
        {
            ui32.Value = 0xffffffffUL;
            ui64.Value = 0L; // store
            var ret = ui32.Value; // load from same address, should be zero
            ui64.Value = 0xffffffffffffffffUL; // store
            return ret;
        }
        private static void testdse() => func((_, panic, __) =>
        {
            var x = int64(-1L); 
            // construct two pointers that alias one another
            var ui64 = (uint64.Value)(@unsafe.Pointer(ref x));
            var ui32 = (uint32.Value)(@unsafe.Pointer(ref x));
            {
                var want = uint32(0L);
                var got = alias_ssa(ui64, ui32);

                if (got != want)
                {
                    fmt.Printf("alias_ssa: wanted %d, got %d\n", want, got);
                    panic("alias_ssa");
                }

            }
        });

        private static void Main()
        {
            testf();
            testg();
            testdse();
        }
    }
}
