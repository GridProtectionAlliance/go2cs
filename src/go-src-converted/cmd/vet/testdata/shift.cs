// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the suspicious shift checker.

// package testdata -- go2cs converted at 2020 August 29 10:10:38 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\shift.go
using fmt = go.fmt_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static void ShiftTest()
        {
            sbyte i8 = default;
            _ = i8 << (int)(7L);
            _ = (i8 + 1L) << (int)(8L); // ERROR ".i8 . 1. .8 bits. too small for shift of 8"
            _ = i8 << (int)((7L + 1L)); // ERROR "i8 .8 bits. too small for shift of 8"
            _ = i8 >> (int)(8L); // ERROR "i8 .8 bits. too small for shift of 8"
            i8 <<= 8L; // ERROR "i8 .8 bits. too small for shift of 8"
            i8 >>= 8L; // ERROR "i8 .8 bits. too small for shift of 8"
            short i16 = default;
            _ = i16 << (int)(15L);
            _ = i16 << (int)(16L); // ERROR "i16 .16 bits. too small for shift of 16"
            _ = i16 >> (int)(16L); // ERROR "i16 .16 bits. too small for shift of 16"
            i16 <<= 16L; // ERROR "i16 .16 bits. too small for shift of 16"
            i16 >>= 16L; // ERROR "i16 .16 bits. too small for shift of 16"
            int i32 = default;
            _ = i32 << (int)(31L);
            _ = i32 << (int)(32L); // ERROR "i32 .32 bits. too small for shift of 32"
            _ = i32 >> (int)(32L); // ERROR "i32 .32 bits. too small for shift of 32"
            i32 <<= 32L; // ERROR "i32 .32 bits. too small for shift of 32"
            i32 >>= 32L; // ERROR "i32 .32 bits. too small for shift of 32"
            long i64 = default;
            _ = i64 << (int)(63L);
            _ = i64 << (int)(64L); // ERROR "i64 .64 bits. too small for shift of 64"
            _ = i64 >> (int)(64L); // ERROR "i64 .64 bits. too small for shift of 64"
            i64 <<= 64L; // ERROR "i64 .64 bits. too small for shift of 64"
            i64 >>= 64L; // ERROR "i64 .64 bits. too small for shift of 64"
            byte u8 = default;
            _ = u8 << (int)(7L);
            _ = u8 << (int)(8L); // ERROR "u8 .8 bits. too small for shift of 8"
            _ = u8 >> (int)(8L); // ERROR "u8 .8 bits. too small for shift of 8"
            u8 <<= 8L; // ERROR "u8 .8 bits. too small for shift of 8"
            u8 >>= 8L; // ERROR "u8 .8 bits. too small for shift of 8"
            ushort u16 = default;
            _ = u16 << (int)(15L);
            _ = u16 << (int)(16L); // ERROR "u16 .16 bits. too small for shift of 16"
            _ = u16 >> (int)(16L); // ERROR "u16 .16 bits. too small for shift of 16"
            u16 <<= 16L; // ERROR "u16 .16 bits. too small for shift of 16"
            u16 >>= 16L; // ERROR "u16 .16 bits. too small for shift of 16"
            uint u32 = default;
            _ = u32 << (int)(31L);
            _ = u32 << (int)(32L); // ERROR "u32 .32 bits. too small for shift of 32"
            _ = u32 >> (int)(32L); // ERROR "u32 .32 bits. too small for shift of 32"
            u32 <<= 32L; // ERROR "u32 .32 bits. too small for shift of 32"
            u32 >>= 32L; // ERROR "u32 .32 bits. too small for shift of 32"
            ulong u64 = default;
            _ = u64 << (int)(63L);
            _ = u64 << (int)(64L); // ERROR "u64 .64 bits. too small for shift of 64"
            _ = u64 >> (int)(64L); // ERROR "u64 .64 bits. too small for shift of 64"
            u64 <<= 64L; // ERROR "u64 .64 bits. too small for shift of 64"
            u64 >>= 64L; // ERROR "u64 .64 bits. too small for shift of 64"
            _ = u64 << (int)(u64); // Non-constant shifts should succeed.

            long i = default;
            _ = i << (int)(31L);
            const long in = 8L * @unsafe.Sizeof(i);

            _ = i << (int)(in); // ERROR "too small for shift"
            _ = i >> (int)(in); // ERROR "too small for shift"
            i <<= in; // ERROR "too small for shift"
            i >>= in; // ERROR "too small for shift"
            const long ix = 8L * @unsafe.Sizeof(i) - 1L;

            _ = i << (int)(ix);
            _ = i >> (int)(ix);
            i <<= ix;
            i >>= ix;

            ulong u = default;
            _ = u << (int)(31L);
            const long un = 8L * @unsafe.Sizeof(u);

            _ = u << (int)(un); // ERROR "too small for shift"
            _ = u >> (int)(un); // ERROR "too small for shift"
            u <<= un; // ERROR "too small for shift"
            u >>= un; // ERROR "too small for shift"
            const long ux = 8L * @unsafe.Sizeof(u) - 1L;

            _ = u << (int)(ux);
            _ = u >> (int)(ux);
            u <<= ux;
            u >>= ux;

            System.UIntPtr p = default;
            _ = p << (int)(31L);
            const long pn = 8L * @unsafe.Sizeof(p);

            _ = p << (int)(pn); // ERROR "too small for shift"
            _ = p >> (int)(pn); // ERROR "too small for shift"
            p <<= pn; // ERROR "too small for shift"
            p >>= pn; // ERROR "too small for shift"
            const long px = 8L * @unsafe.Sizeof(p) - 1L;

            _ = p << (int)(px);
            _ = p >> (int)(px);
            p <<= px;
            p >>= px;

            const var oneIf64Bit = ~uint(0L) >> (int)(63L); // allow large shifts of constants; they are used for 32/64 bit compatibility tricks

 // allow large shifts of constants; they are used for 32/64 bit compatibility tricks

            System.UIntPtr h = default;
            h = h << (int)(8L) | (h >> (int)((8L * (@unsafe.Sizeof(h) - 1L))));
            h <<= 8L * @unsafe.Sizeof(h); // ERROR "too small for shift"
            h >>= 7L * @unsafe.Alignof(h);
            h >>= 8L * @unsafe.Alignof(h); // ERROR "too small for shift"
        }

        public static void ShiftDeadCode()
        {
            long i = default;
            const long iBits = 8L * @unsafe.Sizeof(i);



            if (iBits <= 32L)
            {
                if (iBits == 16L)
                {
                    _ = i >> (int)(8L);
                }
                else
                {
                    _ = i >> (int)(16L);
                }
            }
            else
            {
                _ = i >> (int)(32L);
            }
            if (iBits >= 64L)
            {
                _ = i << (int)(32L);
                if (iBits == 128L)
                {
                    _ = i << (int)(64L);
                }
            }
            else
            {
                _ = i << (int)(16L);
            }
            if (iBits == 64L)
            {
                _ = i << (int)(32L);
            }
            switch (iBits)
            {
                case 128L: 

                case 64L: 
                    _ = i << (int)(32L);
                    break;
                default: 
                    _ = i << (int)(16L);
                    break;
            }


            if (iBits < 32L) 
                _ = i << (int)(16L);
            else if (iBits > 64L) 
                _ = i << (int)(64L);
            else 
                _ = i << (int)(64L); // ERROR "too small for shift"
            // Make sure other vet checks work in dead code.
            if (iBits == 1024L)
            {
                _ = i << (int)(512L); // OK
                fmt.Printf("foo %s bar", 123L); // ERROR "Printf"
            }
        }
    }
}}}
