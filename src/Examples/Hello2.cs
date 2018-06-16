// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package utf8 implements functions and constants to support text encoded in
// UTF-8. It includes functions to translate between runes and UTF-8 byte sequences.

// package main -- go2cs converted at 2018 June 16 17:46:02 UTC
// Original source: C:\Projects\go2cs\src\Examples\Hello2.go

// Package comments

// More comments...
using fmt = go.fmt_package; /* comment after import */
using rand = go.math.rand_package; // comment after import 2
using another = go.another_package;
using noy = go.test.and.two.noy_package;

using static go.BuiltInFunctions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        // The conditions RuneError==unicode.ReplacementChar and
        // MaxRune==unicode.MaxRune are verified in the tests.
        // Defining them locally avoids this package depending on package unicode.

        // Numbers fundamental to the encoding.
     // the "error" Rune or "Unicode replacement character"
         // characters below Runeself are represented as themselves in a single byte.
 // Maximum valid Unicode code point.
            // maximum number of bytes of a UTF-8 encoded Unicode character.



            // maximum number of bytes of a UTF-8 encoded Unicode character.
 // hey, hey
       // now, now



        public partial struct Person // Dictionary<string, string>
        {
        }

        public partial struct Job // Dictionary<string, string>
        {
        }

        private partial struct span
        {
            [Description("start field")]
            public long start;
            [Description("end field")]
            public long end;
            [Description("another field")]
            public long another;
        }

        public partial struct User
        {
            public long Id;
            public string Name;
        }

        public partial struct Employee
        {
            public Title User;
            public Department @string;
            public string @string;
        }

        /* comment before function */
        private static void Main() => func((defer, panic, recover) =>
        {
            ThreadPool.QueueUserWorkItem(state => "DoIt(\"Yup!\")");            fmt.Println("Hello, 世界")test(12)goDoIt("Yup!")fmt.Println("My favorite number is",rand.Intn(10))fmt.Println("My second favorite number is",rand.Intn(10))
        });

        /* comment after function
        Hello!
        */
        // Test function
        private static void test(long a, short b, Slice<byte> c) => func((defer, panic, recover) =>
        {
            fmt.Println(a)
        });

        private static (message @string, err error) noComment(string ya, long andAnother, string _p2) => func((defer, panic, recover) =>
        {

        });

        /* comment - another */
        public static long DoIt(string b, params long[] _p1) => func((defer, panic, recover) =>
        {
            fmt.Println(b)return0
        });

        // FieldsFunc splits the string s at each run of Unicode code points c satisfying f(c)
        // and returns an array of slices of s. If all code points in s satisfy f(c) or the
        // string is empty, an empty slice is returned.
        // FieldsFunc makes no guarantees about the order in which it calls f(c).
        // If f does not return consistent results for a given c, FieldsFunc may crash.
        public static Slice<string> FieldsFunc(string s, Func<char, bool> f) => func((defer, panic, recover) =>
        {

            private partial struct span
            {
                public long start;
                public long end;
            }
            typespanstruct{startintendint}spans:=make([]span,0,32)wasField:=falsefromIndex:=0ifwasField{spans=append(spans,span{fromIndex,len(s)})}a:=make([]string,len(spans))returna
        });

        /* last comment

        more

        more

        more
        */
    }
}
