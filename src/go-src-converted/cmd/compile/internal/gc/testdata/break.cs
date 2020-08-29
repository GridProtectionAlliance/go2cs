// run

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Tests continue and break.

// package main -- go2cs converted at 2020 August 29 09:57:38 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\break.go

using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static long continuePlain_ssa()
        {
            long n = default;
            for (long i = 0L; i < 10L; i++)
            {
                if (i == 6L)
                {
                    continue;
                }
                n = i;
            }
            return n;
        }

        private static long continueLabeled_ssa()
        {
            long n = default;
Next:
            for (long i = 0L; i < 10L; i++)
            {
                if (i == 6L)
                {
                    _continueNext = true;
                    break;
                }
                n = i;
            }
            return n;
        }

        private static long continuePlainInner_ssa()
        {
            long n = default;
            {
                long j = 0L;

                while (j < 30L)
                {
                    for (long i = 0L; i < 10L; i++)
                    {
                        if (i == 6L)
                        {
                            continue;
                        }
                        n = i;
                    j += 10L;
                    }

                    n += j;
                }

            }
            return n;
        }

        private static long continueLabeledInner_ssa()
        {
            long n = default;
            {
                long j = 0L;

                while (j < 30L)
                {
Next:
                    for (long i = 0L; i < 10L; i++)
                    {
                        if (i == 6L)
                        {
                            _continueNext = true;
                            break;
                        }
                        n = i;
                    j += 10L;
                    }
                    n += j;
                }

            }
            return n;
        }

        private static long continueLabeledOuter_ssa()
        {
            long n = default;
Next:
            {
                long j = 0L;

                while (j < 30L)
                {
                    for (long i = 0L; i < 10L; i++)
                    {
                        if (i == 6L)
                        {
                            _continueNext = true;
                            break;
                        }
                        n = i;
                    j += 10L;
                    }

                    n += j;
                }

            }
            return n;
        }

        private static long breakPlain_ssa()
        {
            long n = default;
            for (long i = 0L; i < 10L; i++)
            {
                if (i == 6L)
                {
                    break;
                }
                n = i;
            }

            return n;
        }

        private static long breakLabeled_ssa()
        {
            long n = default;
Next:
            for (long i = 0L; i < 10L; i++)
            {
                if (i == 6L)
                {
                    _breakNext = true;
                    break;
                }
                n = i;
            }
            return n;
        }

        private static long breakPlainInner_ssa()
        {
            long n = default;
            {
                long j = 0L;

                while (j < 30L)
                {
                    for (long i = 0L; i < 10L; i++)
                    {
                        if (i == 6L)
                        {
                            break;
                        }
                        n = i;
                    j += 10L;
                    }

                    n += j;
                }

            }
            return n;
        }

        private static long breakLabeledInner_ssa()
        {
            long n = default;
            {
                long j = 0L;

                while (j < 30L)
                {
Next:
                    for (long i = 0L; i < 10L; i++)
                    {
                        if (i == 6L)
                        {
                            _breakNext = true;
                            break;
                        }
                        n = i;
                    j += 10L;
                    }
                    n += j;
                }

            }
            return n;
        }

        private static long breakLabeledOuter_ssa()
        {
            long n = default;
Next:
            {
                long j = 0L;

                while (j < 30L)
                {
                    for (long i = 0L; i < 10L; i++)
                    {
                        if (i == 6L)
                        {
                            _breakNext = true;
                            break;
                        }
                        n = i;
                    j += 10L;
                    }

                    n += j;
                }

            }
            return n;
        }

        private static long g = default;        private static long h = default; // globals to ensure optimizations don't collapse our switch statements

 // globals to ensure optimizations don't collapse our switch statements

        private static long switchPlain_ssa()
        {
            long n = default;
            switch (g)
            {
                case 0L: 
                    n = 1L;
                    break;
                    n = 2L;
                    break;
            }
            return n;
        }

        private static long switchLabeled_ssa()
        {
            long n = default;
Done:
            switch (g)
            {
                case 0L: 
                    n = 1L;
                    _breakDone = true;
                    break;
                    n = 2L;
                    break;
            }
            return n;
        }

        private static long switchPlainInner_ssa()
        {
            long n = default;
            switch (g)
            {
                case 0L: 
                    n = 1L;
                    switch (h)
                    {
                        case 0L: 
                            n += 10L;
                            break;
                            break;
                    }
                    n = 2L;
                    break;
            }
            return n;
        }

        private static long switchLabeledInner_ssa()
        {
            long n = default;
            switch (g)
            {
                case 0L: 
                                    n = 1L;
                    Done:
                                    switch (h)
                                    {
                                        case 0L: 
                                            n += 10L;
                                            _breakDone = true;
                                            break;
                                            break;
                                    }
                                    n = 2L;
                    break;
            }
            return n;
        }

        private static long switchLabeledOuter_ssa()
        {
            long n = default;
Done:
            switch (g)
            {
                case 0L: 
                    n = 1L;
                    switch (h)
                    {
                        case 0L: 
                            n += 10L;
                            _breakDone = true;
                            break;
                            break;
                    }
                    n = 2L;
                    break;
            }
            return n;
        }

        private static void Main() => func((_, panic, __) =>
        {
            bool failed = default;
            foreach (var (_, test) in tests)
            {
                {
                    var got = test.fn();

                    if (test.fn() != test.want)
                    {
                        print(test.name, "()=", got, ", want ", test.want, "\n");
                        failed = true;
                    }

                }
            }
            if (failed)
            {
                panic("failed");
            }
        });
    }
}
