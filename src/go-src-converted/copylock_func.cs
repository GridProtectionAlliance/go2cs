// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the copylock checker's
// function declaration analysis.

// package a -- go2cs converted at 2020 October 09 06:03:57 UTC
// import "golang.org/x/tools/go/analysis/passes/copylock/testdata/src/a" ==> using a = go.golang.org.x.tools.go.analysis.passes.copylock.testdata.src.a_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\copylock\testdata\src\a\copylock_func.go
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes {
namespace copylock {
namespace testdata {
namespace src
{
    public static partial class a_package
    {
        public static void OkFunc(ptr<sync.Mutex> _addr__p0)
        {
            ref sync.Mutex _p0 = ref _addr__p0.val;

        }
        public static void BadFunc(sync.Mutex _p0)
        {
        } // want "BadFunc passes lock by value: sync.Mutex"
        public static void BadFunc2(sync.Map _p0)
        {
        } // want "BadFunc2 passes lock by value: sync.Map contains sync.Mutex"
        public static ptr<sync.Mutex> OkRet()
        {
        }
        public static sync.Mutex BadRet()
        {
        } // Don't warn about results

        public static Action<ptr<sync.Mutex>> OkClosure = _p0 =>
        {
        };        public static Action<sync.Mutex> BadClosure = _p0 =>
        {
        };        public static Action<sync.Map> BadClosure2 = _p0 =>
        {
        };

        public partial struct EmbeddedRWMutex
        {
            public ref sync.RWMutex RWMutex => ref RWMutex_val;
        }

        private static void OkMeth(this ptr<EmbeddedRWMutex> _addr__p0)
        {
            ref EmbeddedRWMutex _p0 = ref _addr__p0.val;

        }
        public static void BadMeth(this EmbeddedRWMutex _p0)
        {
        } // want "BadMeth passes lock by value: a.EmbeddedRWMutex"
        public static void OkFunc(ptr<sync.Mutex> _addr__p0)
        {
            ref sync.Mutex _p0 = ref _addr__p0.val;

        }
        public static void BadFunc(sync.Mutex _p0)
        {
        } // want "BadFunc passes lock by value: a.EmbeddedRWMutex"
        public static ptr<sync.Mutex> OkRet()
        {
        }
        public static sync.Mutex BadRet()
        {
        } // Don't warn about results

        public partial struct FieldMutex
        {
            public sync.Mutex s;
        }

        private static void OkMeth(this ptr<FieldMutex> _addr__p0)
        {
            ref FieldMutex _p0 = ref _addr__p0.val;

        }
        public static void BadMeth(this FieldMutex _p0)
        {
        } // want "BadMeth passes lock by value: a.FieldMutex contains sync.Mutex"
        public static void OkFunc(ptr<sync.Mutex> _addr__p0)
        {
            ref sync.Mutex _p0 = ref _addr__p0.val;

        }
        public static void BadFunc(sync.Mutex _p0)
        {
        } // want "BadFunc passes lock by value: a.FieldMutex contains sync.Mutex"

        public partial struct L0
        {
            public ref L1 L1 => ref L1_val;
        }

        public partial struct L1
        {
            public L2 l;
        }

        public partial struct L2
        {
            public ref sync.Mutex Mutex => ref Mutex_val;
        }

        private static void Ok(this ptr<L0> _addr__p0)
        {
            ref L0 _p0 = ref _addr__p0.val;

        }
        public static void Bad(this L0 _p0)
        {
        } // want "Bad passes lock by value: a.L0 contains a.L1 contains a.L2"

        public partial struct EmbeddedMutexPointer
        {
            public ptr<sync.Mutex> s; // safe to copy this pointer
        }

        private static void Ok(this ptr<EmbeddedMutexPointer> _addr__p0)
        {
            ref EmbeddedMutexPointer _p0 = ref _addr__p0.val;

        }
        public static void AlsoOk(this EmbeddedMutexPointer _p0)
        {
        }
        public static void StillOk(EmbeddedMutexPointer _p0)
        {
        }
        public static EmbeddedMutexPointer LookinGood()
        {
        }

        public partial struct EmbeddedLocker : sync.Locker
        {
            public ref sync.Locker Locker => ref Locker_val; // safe to copy interface values
        }

        private static void Ok(this ptr<EmbeddedLocker> _addr__p0)
        {
            ref EmbeddedLocker _p0 = ref _addr__p0.val;

        }
        public static void AlsoOk(this EmbeddedLocker _p0)
        {
        }

        public partial struct CustomLock
        {
        }

        private static void Lock(this ptr<CustomLock> _addr__p0)
        {
            ref CustomLock _p0 = ref _addr__p0.val;

        }
        private static void Unlock(this ptr<CustomLock> _addr__p0)
        {
            ref CustomLock _p0 = ref _addr__p0.val;

        }

        public static void Ok(ptr<CustomLock> _addr__p0)
        {
            ref CustomLock _p0 = ref _addr__p0.val;

        }
        public static void Bad(CustomLock _p0)
        {
        } // want "Bad passes lock by value: a.CustomLock"

        // Passing lock values into interface function arguments
        public static void FuncCallInterfaceArg(Action<long, object> f)
        {
            sync.Mutex m = default;
            ref var t = ref heap(out ptr<var> _addr_t);

            f(1L, "foo");
            f(2L, _addr_t);
            f(3L, addr(new sync.Mutex()));
            f(4L, m); // want "call of f copies lock value: sync.Mutex"
            f(5L, t); // want "call of f copies lock value: struct.lock sync.Mutex. contains sync.Mutex"
            slice<Action<t>> fntab = default;
            fntab[0L](t); // want "call of fntab.0. copies lock value: struct.lock sync.Mutex. contains sync.Mutex"
        }

        // Returning lock via interface value
        public static (long, object) ReturnViaInterface(long x)
        {
            long _p0 = default;
            object _p0 = default;

            sync.Mutex m = default;
            var t = default;

            switch (x % 4L)
            {
                case 0L: 
                    return (0L, "qwe");
                    break;
                case 1L: 
                    return (1L, addr(new sync.Mutex()));
                    break;
                case 2L: 
                    return (2L, m); // want "return copies lock value: sync.Mutex"
                    break;
                default: 
                    return (3L, t); // want "return copies lock value: struct.lock sync.Mutex. contains sync.Mutex"
                    break;
            }

        }

        // Some cases that we don't warn about.

        public static void AcceptedCases()
        {
            EmbeddedRwMutex x = new EmbeddedRwMutex(); // composite literal on RHS is OK (#16227)
            x = BadRet(); // function call on RHS is OK (#16227)
            x = OKRet().val; // indirection of function call on RHS is OK (#16227)
        }

        // TODO: Unfortunate cases

        // Non-ideal error message:
        // Since we're looking for Lock methods, sync.Once's underlying
        // sync.Mutex gets called out, but without any reference to the sync.Once.
        public partial struct LocalOnce // : sync.Once
        {
        }

        public static void Bad(this LocalOnce _p0)
        {
        } // want "Bad passes lock by value: a.LocalOnce contains sync.Mutex"

        // False negative:
        // LocalMutex doesn't have a Lock method.
        // Nevertheless, it is probably a bad idea to pass it by value.
        public partial struct LocalMutex // : sync.Mutex
        {
        }

        public static void Bad(this LocalMutex _p0)
        {
        } // WANTED: An error here :(
    }
}}}}}}}}}}
