// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package utf8 implements functions and constants to support text encoded in
// UTF-8. It includes functions to translate between runes and UTF-8 byte sequences.
// package main -- go2cs converted at 2018 August 06 15:38:06 UTC
// Original source: D:\Projects\go2cs\src\Examples\Hello2.go
// Package comments

// More comments...
using fmt = go.fmt_package; /* comment after import */
using rand = go.math.rand_package; // comment after import 2
using another = go.another_package;
using noy = go.test.and.two.noy_package;
using static go.builtin;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static readonly dynamic RuneError = '\uFFFD';// the "error" Rune or "Unicode replacement character"
        public static readonly dynamic RuneSelf = 0x80;// characters below Runeself are represented as themselves in a single byte.
        public static readonly dynamic MaxRune = '\U0010FFFF';// Maximum valid Unicode code point.
        public static readonly dynamic UTFMax = 4;// maximum number of bytes of a UTF-8 encoded Unicode character.
        public static readonly dynamic RuneError2 = '\uFFFD';
        public static readonly dynamic RuneSelf2 = 0x80;
        public static readonly dynamic MaxRune2 = '\U0010FFFF';
        public static readonly dynamic UTFMax2 = 4;// maximum number of bytes of a UTF-8 encoded Unicode character.
        private static readonly dynamic test = iota;// hey, hey
        private static readonly dynamic test2 = 0;// now, now
        private static readonly dynamic testi = 1 + iota * i(1);
        private static readonly dynamic testi2 = 0;
        public partial struct Person // : Dictionary<@string, @string>
        {
        }

        public partial struct Job // : Dictionary<@string, @string>
        {
        }

        private partial struct span
        {
            [Description("start field")]
            public @int start;
            [Description("end field")]
            public @int end;
            [Description("another field")]
            public @int another;
        }

        public partial struct User
        {
            public @int Id;
            public @string Name;
        }

        public partial struct Employee
        {
            public ref User User => ref User_val;       //annonymous field
            public @string Title;
            public @string Department;
        }


        /* comment before function */
        private static void Main()
        {
            fmt.Println("Hello, 世界");            /* eol comment */
            test(12);

            // Before call comment
            ThreadPool.QueueUserWorkItem(state => DoIt("Yup!"));// After call comment
            fmt.Println("My favorite number is", rand.Intn(10));
            fmt.Println("My second favorite number is", rand.Intn(10));
        }
        /* comment after function
        Hello!
        */
        // Test function
        private static void test(@int a, int16 b, slice<@byte> c)
        {
            fmt.Println(a);
        }

        private static (message, err) noComment(@string ya, @int andAnother, @string _p0)
        {
        }

        /* comment - another */
        public static @int DoIt(@string b, @int _p0)
        {            // here
            fmt.Println(b);
            return 0;
        }

        // FieldsFunc splits the string s at each run of Unicode code points c satisfying f(c)
        // and returns an array of slices of s. If all code points in s satisfy f(c) or the
        // string is empty, an empty slice is returned.
        // FieldsFunc makes no guarantees about the order in which it calls f(c).
        // If f does not return consistent results for a given c, FieldsFunc may crash.
        public static slice<@string> FieldsFunc(@string s, Func<rune, @bool> f)
        {
            // A span is used to record a slice of s of the form s[start:end].
            // The start index is inclusive and the end index is exclusive.
            private partial struct span
            {
                [Description("start field")]
                public @int start;
                [Description("end field")]
                public @int end;
                [Description("another field")]
                public @int another;
            }
            var spans = make(typeof(slice<span>), 0, 32);

            // Find the field start and end indices.
            var wasField = false;
            var fromIndex = 0;

            //for i, rune := range s {
            if (f(rune))
            {
                if (wasField)
                {
                    spans = append(spans, span{start:fromIndex,end:i});
                    wasField = false;

                    if (len(spans) < 3)
                    {
                        return (0, syntaxError(fnParseUint, s0));
                    }
                    else if (len(spans) < 2)
                    {
                        return (0, syntaxError(fnParseUint, s0));
                    }
                    else if (len(spans) < 1)
                    {
                        return (0, syntaxError(fnParseUint, s0));
                    }
                    else
                    {
                        wasField = true;
                    }
                }
            }
            else
            {
                if (!wasField)
                {
                    fromIndex = i;
                    wasField = true;
                }
                else
                {
                    var s0 = s;
                    Switch()
                    .Case(2 <= base && base <= 36)(() =>
                    {
                        // valid base; nothing to do

                    })
                    .Case(base == 0)(() =>
                    {
                        // Look for octal, hex prefix.
                        Switch()
                        .Case(s[0] == '0' && len(s) > 1 && (s[1] == 'x' || s[1] == 'X'))(() =>
                        {
                            if (len(s) < 3)
                            {
                                return (0, syntaxError(fnParseUint, s0));
                            }
                            else if (len(s) < 2)
                            {
                                return (0, syntaxError(fnParseUint, s0));
                            }
                            else if (len(s) < 1)
                            {
                                return (0, syntaxError(fnParseUint, s0));
                            }
                            else
                            {
                                base = 16;
                                s = s.slice(2);
                            }
                        })
                        .Case(s[0] == '0')(() =>
                        {
                            base = 8;
                            s = s.slice(1);
                        })
                        .Default(() =>
                        {
                            base = 10;
                        });

                    })
                    .Default(() =>
                    {
                        return (0, baseError(fnParseUint, s0, base));
                    });
                }
            }
            //}

            // Last field might end at EOF.
            if (wasField)
            {
                spans = append(spans, span{fromIndex,len(s)});
            }

            // Create strings from recorded field indices.
            var a = make(typeof(slice<@string>), len(spans));

            /*
            for i, span := range spans {
            a[i] = s[span.start:span.end]
            }
            */

            return a;
        }
        /* last comment

        more

        more

        more
        */
    }
}
