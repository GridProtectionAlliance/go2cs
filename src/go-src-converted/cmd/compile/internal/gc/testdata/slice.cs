// run

// This test makes sure that t.s = t.s[0:x] doesn't write
// either the slice pointer or the capacity.
// See issue #14855.

// package main -- go2cs converted at 2020 August 29 09:58:27 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\slice.go
using fmt = go.fmt_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        public static readonly long N = 1000000L;



        public partial struct T
        {
            public slice<long> s;
        }

        private static void Main() => func((_, panic, __) =>
        {
            var done = make_channel<object>();
            var a = make_slice<long>(N + 10L);

            T t = ref new T(a);

            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < N; i++)
                    {
                        t.s = t.s[1L..9L];
                    }


                    i = i__prev1;
                }
                done.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
            }());
            go_(() => () =>
            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i < N; i++)
                    {
                        t.s = t.s[0L..8L]; // should only write len
                    }


                    i = i__prev1;
                }
                done.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
            }());
            done.Receive().Send(done);

            var ok = true;
            if (cap(t.s) != cap(a) - N)
            {
                fmt.Printf("wanted cap=%d, got %d\n", cap(a) - N, cap(t.s));
                ok = false;
            }
            if (ref t.s[0L] != ref a[N])
            {
                fmt.Printf("wanted ptr=%p, got %p\n", ref a[N], ref t.s[0L]);
                ok = false;
            }
            if (!ok)
            {
                panic("bad");
            }
        });
    }
}
