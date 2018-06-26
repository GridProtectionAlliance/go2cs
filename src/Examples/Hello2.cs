// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package utf8 implements functions and constants to support text encoded in
// UTF-8. It includes functions to translate between runes and UTF-8 byte sequences.

// package main -- go2cs converted at 2018 June 26 19:23:09 UTC
// Original source: D:\Projects\go2cs\src\Examples\Hello2.go

// Package comments

// More comments...
using fmt = go.fmt_package; /* comment after import */
using rand = go.math.rand_package; // comment after import 2
using another = go.another_package;
using noy = go.test.and.two.noy_package;
using static go.BuiltInFunctions;
using System.Collections.Generic;
using System.Threading;
using System;

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








        /* comment before function */
        private static void Main()
        {
            ThreadPool.QueueUserWorkItem(state => DoIt("Yup!"));            fmt.Println("Hello, 世界")test(12)goDoIt("Yup!")fmt.Println("My favorite number is",rand.Intn(10))fmt.Println("My second favorite number is",rand.Intn(10))
        }

        /* comment after function
        Hello!
        */
        // Test function
        private static void test(long a, short b, Slice<byte> c)
        {
            fmt.Println(a)
        }

        private static (message, err) noComment(string ya, long andAnother, string _p0)
        {

        }

        /* comment - another */
        public static long DoIt(string b, long _p0)
        {
            fmt.Println(b)return0
        }

        // FieldsFunc splits the string s at each run of Unicode code points c satisfying f(c)
        // and returns an array of slices of s. If all code points in s satisfy f(c) or the
        // string is empty, an empty slice is returned.
        // FieldsFunc makes no guarantees about the order in which it calls f(c).
        // If f does not return consistent results for a given c, FieldsFunc may crash.
        public static Slice<string> FieldsFunc(string s, Func<char, bool> f)
        {

            typespanstruct{startintendint}spans:=make([]span,0,32)wasField:=falsefromIndex:=0ifwasField{spans=append(spans,span{fromIndex,len(s)})}a:=make([]string,len(spans))returna
        }

        /* last comment

        more

        more

        more
        */
    }
}
