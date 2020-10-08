// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 03:43:42 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\panicprint.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        public partial struct MyBool // : bool
        {
        }
        public partial struct MyComplex128 // : System.Numerics.Complex128
        {
        }
        public partial struct MyComplex64 // : complex64
        {
        }
        public partial struct MyFloat32 // : float
        {
        }
        public partial struct MyFloat64 // : double
        {
        }
        public partial struct MyInt // : long
        {
        }
        public partial struct MyInt8 // : sbyte
        {
        }
        public partial struct MyInt16 // : short
        {
        }
        public partial struct MyInt32 // : int
        {
        }
        public partial struct MyInt64 // : long
        {
        }
        public partial struct MyString // : @string
        {
        }
        public partial struct MyUint // : ulong
        {
        }
        public partial struct MyUint8 // : byte
        {
        }
        public partial struct MyUint16 // : ushort
        {
        }
        public partial struct MyUint32 // : uint
        {
        }
        public partial struct MyUint64 // : ulong
        {
        }
        public partial struct MyUintptr // : System.UIntPtr
        {
        }

        private static void panicCustomComplex64() => func((_, panic, __) =>
        {
            panic(MyComplex64(0.11F + 3iUL));
        });

        private static void panicCustomComplex128() => func((_, panic, __) =>
        {
            panic(MyComplex128(32.1F + 10iUL));
        });

        private static void panicCustomString() => func((_, panic, __) =>
        {
            panic(MyString("Panic"));
        });

        private static void panicCustomBool() => func((_, panic, __) =>
        {
            panic(MyBool(true));
        });

        private static void panicCustomInt() => func((_, panic, __) =>
        {
            panic(MyInt(93L));
        });

        private static void panicCustomInt8() => func((_, panic, __) =>
        {
            panic(MyInt8(93L));
        });

        private static void panicCustomInt16() => func((_, panic, __) =>
        {
            panic(MyInt16(93L));
        });

        private static void panicCustomInt32() => func((_, panic, __) =>
        {
            panic(MyInt32(93L));
        });

        private static void panicCustomInt64() => func((_, panic, __) =>
        {
            panic(MyInt64(93L));
        });

        private static void panicCustomUint() => func((_, panic, __) =>
        {
            panic(MyUint(93L));
        });

        private static void panicCustomUint8() => func((_, panic, __) =>
        {
            panic(MyUint8(93L));
        });

        private static void panicCustomUint16() => func((_, panic, __) =>
        {
            panic(MyUint16(93L));
        });

        private static void panicCustomUint32() => func((_, panic, __) =>
        {
            panic(MyUint32(93L));
        });

        private static void panicCustomUint64() => func((_, panic, __) =>
        {
            panic(MyUint64(93L));
        });

        private static void panicCustomUintptr() => func((_, panic, __) =>
        {
            panic(MyUintptr(93L));
        });

        private static void panicCustomFloat64() => func((_, panic, __) =>
        {
            panic(MyFloat64(-93.70F));
        });

        private static void panicCustomFloat32() => func((_, panic, __) =>
        {
            panic(MyFloat32(-93.70F));
        });

        private static void init()
        {
            register("panicCustomComplex64", panicCustomComplex64);
            register("panicCustomComplex128", panicCustomComplex128);
            register("panicCustomBool", panicCustomBool);
            register("panicCustomFloat32", panicCustomFloat32);
            register("panicCustomFloat64", panicCustomFloat64);
            register("panicCustomInt", panicCustomInt);
            register("panicCustomInt8", panicCustomInt8);
            register("panicCustomInt16", panicCustomInt16);
            register("panicCustomInt32", panicCustomInt32);
            register("panicCustomInt64", panicCustomInt64);
            register("panicCustomString", panicCustomString);
            register("panicCustomUint", panicCustomUint);
            register("panicCustomUint8", panicCustomUint8);
            register("panicCustomUint16", panicCustomUint16);
            register("panicCustomUint32", panicCustomUint32);
            register("panicCustomUint64", panicCustomUint64);
            register("panicCustomUintptr", panicCustomUintptr);
        }
    }
}
