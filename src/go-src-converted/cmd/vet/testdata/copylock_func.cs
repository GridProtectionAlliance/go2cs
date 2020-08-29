// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the copylock checker's
// function declaration analysis.

// package testdata -- go2cs converted at 2020 August 29 10:10:31 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\copylock_func.go
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static void OkFunc(ref sync.Mutex _p0)
        {
        }
        public static void BadFunc(sync.Mutex _p0)
        {
        } // ERROR "BadFunc passes lock by value: sync.Mutex"
        public static void BadFunc2(sync.Map _p0)
        {
        } // ERROR "BadFunc2 passes lock by value: sync.Map contains sync.Mutex"
        public static ref sync.Mutex OkRet()
        {
        }
        public static sync.Mutex BadRet()
        {
        } // Don't warn about results

        public static Action<ref sync.Mutex> OkClosure = _p0 =>
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

        private static void OkMeth(this ref EmbeddedRWMutex _p0)
        {
        }
        public static void BadMeth(this EmbeddedRWMutex _p0)
        {
        } // ERROR "BadMeth passes lock by value: testdata.EmbeddedRWMutex"
        public static void OkFunc(ref sync.Mutex _p0)
        {
        }
        public static void BadFunc(sync.Mutex _p0)
        {
        } // ERROR "BadFunc passes lock by value: testdata.EmbeddedRWMutex"
        public static ref sync.Mutex OkRet()
        {
        }
        public static sync.Mutex BadRet()
        {
        } // Don't warn about results

        public partial struct FieldMutex
        {
            public sync.Mutex s;
        }

        private static void OkMeth(this ref FieldMutex _p0)
        {
        }
        public static void BadMeth(this FieldMutex _p0)
        {
        } // ERROR "BadMeth passes lock by value: testdata.FieldMutex contains sync.Mutex"
        public static void OkFunc(ref sync.Mutex _p0)
        {
        }
        public static void BadFunc(sync.Mutex _p0)
        {
        } // ERROR "BadFunc passes lock by value: testdata.FieldMutex contains sync.Mutex"

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

        private static void Ok(this ref L0 _p0)
        {
        }
        public static void Bad(this L0 _p0)
        {
        } // ERROR "Bad passes lock by value: testdata.L0 contains testdata.L1 contains testdata.L2"

        public partial struct EmbeddedMutexPointer
        {
            public ptr<sync.Mutex> s; // safe to copy this pointer
        }

        private static void Ok(this ref EmbeddedMutexPointer _p0)
        {
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

        private static void Ok(this ref EmbeddedLocker _p0)
        {
        }
        public static void AlsoOk(this EmbeddedLocker _p0)
        {
        }

        public partial struct CustomLock
        {
        }

        private static void Lock(this ref CustomLock _p0)
        {
        }
        private static void Unlock(this ref CustomLock _p0)
        {
        }

        public static void Ok(ref CustomLock _p0)
        {
        }
        public static void Bad(CustomLock _p0)
        {
        } // ERROR "Bad passes lock by value: testdata.CustomLock"

        // Passing lock values into interface function arguments
        public static void FuncCallInterfaceArg(Action<long, object> f)
        {
            sync.Mutex m = default;
            var t = default;

            f(1L, "foo");
            f(2L, ref t);
            f(3L, ref new sync.Mutex());
            f(4L, m); // ERROR "call of f copies lock value: sync.Mutex"
            f(5L, t); // ERROR "call of f copies lock value: struct.lock sync.Mutex. contains sync.Mutex"
            slice<Action<t>> fntab = default;
            fntab[0L](t); // ERROR "call of fntab.0. copies lock value: struct.lock sync.Mutex. contains sync.Mutex"
        }

        // Returning lock via interface value
        public static (long, object) ReturnViaInterface(long x)
        {
            sync.Mutex m = default;
            var t = default;

            switch (x % 4L)
            {
                case 0L: 
                    return (0L, "qwe");
                    break;
                case 1L: 
                    return (1L, ref new sync.Mutex());
                    break;
                case 2L: 
                    return (2L, m); // ERROR "return copies lock value: sync.Mutex"
                    break;
                default: 
                    return (3L, t); // ERROR "return copies lock value: struct.lock sync.Mutex. contains sync.Mutex"
                    break;
            }
        }

        // Some cases that we don't warn about.

        public static void AcceptedCases()
        {
            EmbeddedRwMutex x = new EmbeddedRwMutex(); // composite literal on RHS is OK (#16227)
            x = BadRet(); // function call on RHS is OK (#16227)
            x = OKRet().Value; // indirection of function call on RHS is OK (#16227)
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
        } // ERROR "Bad passes lock by value: testdata.LocalOnce contains sync.Mutex"

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
}}}
