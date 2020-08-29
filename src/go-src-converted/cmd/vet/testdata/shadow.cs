// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the shadowed variable checker.
// Some of these errors are caught by the compiler (shadowed return parameters for example)
// but are nonetheless useful tests.

// package testdata -- go2cs converted at 2020 August 29 10:10:38 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\shadow.go
using os = go.os_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static error ShadowRead(ref os.File f, slice<byte> buf)
        {
            long x = default;
            if (f != null)
            {
                long err = 3L; // OK - different type.
                _ = err;
            }
            if (f != null)
            {
                var (_, err) = f.Read(buf); // ERROR "declaration of .err. shadows declaration at testdata/shadow.go:13"
                if (err != null)
                {
                    return error.As(err);
                }
                long i = 3L; // OK
                _ = i;
            }
            if (f != null)
            {
                x = one(); // ERROR "declaration of .x. shadows declaration at testdata/shadow.go:14"
 // ERROR "declaration of .err. shadows declaration at testdata/shadow.go:13"
                if (x == 1L && err != null)
                {
                    return error.As(err);
                }
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < 10L; i++)
                {
                    i = i; // OK: obviously intentional idiomatic redeclaration
                    go_(() => () =>
                    {
                        println(i);
                    }());
                }

                i = i__prev1;
            }
            var shadowTemp = default;
            switch (shadowTemp.type())
            {
                case long shadowTemp:
                    println("OK");
                    _ = shadowTemp;
                    break;
            }
            {
                var shadowTemp__prev1 = shadowTemp;

                shadowTemp = shadowTemp;

                if (true)
                { // OK: obviously intentional idiomatic redeclaration
                    ref os.File f = default; // OK because f is not mentioned later in the function.
                    // The declaration of x is a shadow because x is mentioned below.
                    x = default; // ERROR "declaration of .x. shadows declaration at testdata/shadow.go:14"
                    _ = x;
                    _ = f;
                    _ = shadowTemp;
                }
                shadowTemp = shadowTemp__prev1;

            } 
            // Use a couple of variables to trigger shadowing errors.
            _ = err;
            _ = x;
            return;
        }

        private static long one()
        {
            return 1L;
        }
    }
}}}
