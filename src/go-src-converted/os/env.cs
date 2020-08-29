// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// General environment variables.

// package os -- go2cs converted at 2020 August 29 08:43:34 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\env.go
using testlog = go.@internal.testlog_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class os_package
    {
        // Expand replaces ${var} or $var in the string based on the mapping function.
        // For example, os.ExpandEnv(s) is equivalent to os.Expand(s, os.Getenv).
        public static @string Expand(@string s, Func<@string, @string> mapping)
        {
            var buf = make_slice<byte>(0L, 2L * len(s)); 
            // ${} is all ASCII, so bytes are fine for this operation.
            long i = 0L;
            for (long j = 0L; j < len(s); j++)
            {
                if (s[j] == '$' && j + 1L < len(s))
                {
                    buf = append(buf, s[i..j]);
                    var (name, w) = getShellName(s[j + 1L..]);
                    buf = append(buf, mapping(name));
                    j += w;
                    i = j + 1L;
                }
            }
            return string(buf) + s[i..];
        }

        // ExpandEnv replaces ${var} or $var in the string according to the values
        // of the current environment variables. References to undefined
        // variables are replaced by the empty string.
        public static @string ExpandEnv(@string s)
        {
            return Expand(s, Getenv);
        }

        // isShellSpecialVar reports whether the character identifies a special
        // shell variable such as $*.
        private static bool isShellSpecialVar(byte c)
        {
            switch (c)
            {
                case '*': 

                case '#': 

                case '$': 

                case '@': 

                case '!': 

                case '?': 

                case '-': 

                case '0': 

                case '1': 

                case '2': 

                case '3': 

                case '4': 

                case '5': 

                case '6': 

                case '7': 

                case '8': 

                case '9': 
                    return true;
                    break;
            }
            return false;
        }

        // isAlphaNum reports whether the byte is an ASCII letter, number, or underscore
        private static bool isAlphaNum(byte c)
        {
            return c == '_' || '0' <= c && c <= '9' || 'a' <= c && c <= 'z' || 'A' <= c && c <= 'Z';
        }

        // getShellName returns the name that begins the string and the number of bytes
        // consumed to extract it. If the name is enclosed in {}, it's part of a ${}
        // expansion and two more bytes are needed than the length of the name.
        private static (@string, long) getShellName(@string s)
        {

            if (s[0L] == '{') 
                if (len(s) > 2L && isShellSpecialVar(s[1L]) && s[2L] == '}')
                {
                    return (s[1L..2L], 3L);
                } 
                // Scan to closing brace
                {
                    long i__prev1 = i;

                    for (long i = 1L; i < len(s); i++)
                    {
                        if (s[i] == '}')
                        {
                            return (s[1L..i], i + 1L);
                        }
                    }


                    i = i__prev1;
                }
                return ("", 1L); // Bad syntax; just eat the brace.
            else if (isShellSpecialVar(s[0L])) 
                return (s[0L..1L], 1L);
            // Scan alphanumerics.
            i = default;
            for (i = 0L; i < len(s) && isAlphaNum(s[i]); i++)
            {
            }

            return (s[..i], i);
        }

        // Getenv retrieves the value of the environment variable named by the key.
        // It returns the value, which will be empty if the variable is not present.
        // To distinguish between an empty value and an unset value, use LookupEnv.
        public static @string Getenv(@string key)
        {
            testlog.Getenv(key);
            var (v, _) = syscall.Getenv(key);
            return v;
        }

        // LookupEnv retrieves the value of the environment variable named
        // by the key. If the variable is present in the environment the
        // value (which may be empty) is returned and the boolean is true.
        // Otherwise the returned value will be empty and the boolean will
        // be false.
        public static (@string, bool) LookupEnv(@string key)
        {
            testlog.Getenv(key);
            return syscall.Getenv(key);
        }

        // Setenv sets the value of the environment variable named by the key.
        // It returns an error, if any.
        public static error Setenv(@string key, @string value)
        {
            var err = syscall.Setenv(key, value);
            if (err != null)
            {
                return error.As(NewSyscallError("setenv", err));
            }
            return error.As(null);
        }

        // Unsetenv unsets a single environment variable.
        public static error Unsetenv(@string key)
        {
            return error.As(syscall.Unsetenv(key));
        }

        // Clearenv deletes all environment variables.
        public static void Clearenv()
        {
            syscall.Clearenv();
        }

        // Environ returns a copy of strings representing the environment,
        // in the form "key=value".
        public static slice<@string> Environ()
        {
            return syscall.Environ();
        }
    }
}
