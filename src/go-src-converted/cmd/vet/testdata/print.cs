// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the printf checker.

// TODO(rsc): The user-defined wrapper tests are commented out
// because they produced too many false positives when vet was
// enabled during go test. See the TODO in ../print.go for a plan
// to fix that; when it's fixed, uncomment the user-defined wrapper tests.

// package testdata -- go2cs converted at 2020 August 29 10:10:36 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\print.go
using fmt = go.fmt_package;
using fmt = go.fmt_package;
using math = go.math_package;
using os = go.os_package;
using testing = go.testing_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static void UnsafePointerPrintfTest()
        {
            unsafe.Pointer up = default;
            fmt.Printf("%p, %x %X", up, up, up);
        }

        // Error methods that do not satisfy the Error interface and should be checked.
        private partial struct errorTest1 // : long
        {
        }

        private static @string Error(this errorTest1 _p0, params object _p0)
        {
            return "hi";
        }

        private partial struct errorTest2 // : long
        {
        } // Analogous to testing's *T type.
        private static void Error(this errorTest2 _p0, params object _p0)
        {
        }

        private partial struct errorTest3 // : long
        {
        }

        private static void Error(this errorTest3 _p0)
        { // No return value.
        }

        private partial struct errorTest4 // : long
        {
        }

        private static long Error(this errorTest4 _p0)
        { // Different return type.
            return 3L;
        }

        private partial struct errorTest5 // : long
        {
        }

        private static void error(this errorTest5 _p0)
        { // niladic; don't complain if no args (was bug)
        }

        // This function never executes, but it serves as a simple test for the program.
        // Test with make test.
        public static void PrintfTests()
        {
            bool b = default;
            long i = default;
            int r = default;
            @string s = default;
            double x = default;
            ref long p = default;
            map<long, long> imap = default;
            slice<double> fslice = default;
            complex64 c = default; 
            // Some good format/argtypes
            fmt.Printf("");
            fmt.Printf("%b %b %b", 3L, i, x);
            fmt.Printf("%c %c %c %c", 3L, i, 'x', r);
            fmt.Printf("%d %d %d", 3L, i, imap);
            fmt.Printf("%e %e %e %e", 3e9F, x, fslice, c);
            fmt.Printf("%E %E %E %E", 3e9F, x, fslice, c);
            fmt.Printf("%f %f %f %f", 3e9F, x, fslice, c);
            fmt.Printf("%F %F %F %F", 3e9F, x, fslice, c);
            fmt.Printf("%g %g %g %g", 3e9F, x, fslice, c);
            fmt.Printf("%G %G %G %G", 3e9F, x, fslice, c);
            fmt.Printf("%b %b %b %b", 3e9F, x, fslice, c);
            fmt.Printf("%o %o", 3L, i);
            fmt.Printf("%p %p", p, null);
            fmt.Printf("%q %q %q %q", 3L, i, 'x', r);
            fmt.Printf("%s %s %s", "hi", s, new slice<byte>(new byte[] { 65 }));
            fmt.Printf("%t %t", true, b);
            fmt.Printf("%T %T", 3L, i);
            fmt.Printf("%U %U", 3L, i);
            fmt.Printf("%v %v", 3L, i);
            fmt.Printf("%x %x %x %x", 3L, i, "hi", s);
            fmt.Printf("%X %X %X %X", 3L, i, "hi", s);
            fmt.Printf("%.*s %d %g", 3L, "hi", 23L, 2.3F);
            fmt.Printf("%s", ref stringerv);
            fmt.Printf("%v", ref stringerv);
            fmt.Printf("%T", ref stringerv);
            fmt.Printf("%s", ref embeddedStringerv);
            fmt.Printf("%v", ref embeddedStringerv);
            fmt.Printf("%T", ref embeddedStringerv);
            fmt.Printf("%v", notstringerv);
            fmt.Printf("%T", notstringerv);
            fmt.Printf("%q", stringerarrayv);
            fmt.Printf("%v", stringerarrayv);
            fmt.Printf("%s", stringerarrayv);
            fmt.Printf("%v", notstringerarrayv);
            fmt.Printf("%T", notstringerarrayv);
            fmt.Printf("%d", @new<Formatter>());
            fmt.Printf("%*%", 2L); // Ridiculous but allowed.
            fmt.Printf("%s"); // Nothing useful we can say.

            fmt.Printf("%g", 1L + 2iUL);
            fmt.Printf("%#e %#E %#f %#F %#g %#G", 1.2F, 1.2F, 1.2F, 1.2F, 1.2F, 1.2F); // OK since Go 1.9
            // Some bad format/argTypes
            fmt.Printf("%b", "hi"); // ERROR "Printf format %b has arg \x22hi\x22 of wrong type string"
            fmt.Printf("%t", c); // ERROR "Printf format %t has arg c of wrong type complex64"
            fmt.Printf("%t", 1L + 2iUL); // ERROR "Printf format %t has arg 1 \+ 2i of wrong type complex128"
            fmt.Printf("%c", 2.3F); // ERROR "Printf format %c has arg 2.3 of wrong type float64"
            fmt.Printf("%d", 2.3F); // ERROR "Printf format %d has arg 2.3 of wrong type float64"
            fmt.Printf("%e", "hi"); // ERROR "Printf format %e has arg \x22hi\x22 of wrong type string"
            fmt.Printf("%E", true); // ERROR "Printf format %E has arg true of wrong type bool"
            fmt.Printf("%f", "hi"); // ERROR "Printf format %f has arg \x22hi\x22 of wrong type string"
            fmt.Printf("%F", 'x'); // ERROR "Printf format %F has arg 'x' of wrong type rune"
            fmt.Printf("%g", "hi"); // ERROR "Printf format %g has arg \x22hi\x22 of wrong type string"
            fmt.Printf("%g", imap); // ERROR "Printf format %g has arg imap of wrong type map\[int\]int"
            fmt.Printf("%G", i); // ERROR "Printf format %G has arg i of wrong type int"
            fmt.Printf("%o", x); // ERROR "Printf format %o has arg x of wrong type float64"
            fmt.Printf("%p", 23L); // ERROR "Printf format %p has arg 23 of wrong type int"
            fmt.Printf("%q", x); // ERROR "Printf format %q has arg x of wrong type float64"
            fmt.Printf("%s", b); // ERROR "Printf format %s has arg b of wrong type bool"
            fmt.Printf("%s", byte(65L)); // ERROR "Printf format %s has arg byte\(65\) of wrong type byte"
            fmt.Printf("%t", 23L); // ERROR "Printf format %t has arg 23 of wrong type int"
            fmt.Printf("%U", x); // ERROR "Printf format %U has arg x of wrong type float64"
            fmt.Printf("%x", null); // ERROR "Printf format %x has arg nil of wrong type untyped nil"
            fmt.Printf("%X", 2.3F); // ERROR "Printf format %X has arg 2.3 of wrong type float64"
            fmt.Printf("%s", stringerv); // ERROR "Printf format %s has arg stringerv of wrong type testdata.ptrStringer"
            fmt.Printf("%t", stringerv); // ERROR "Printf format %t has arg stringerv of wrong type testdata.ptrStringer"
            fmt.Printf("%s", embeddedStringerv); // ERROR "Printf format %s has arg embeddedStringerv of wrong type testdata.embeddedStringer"
            fmt.Printf("%t", embeddedStringerv); // ERROR "Printf format %t has arg embeddedStringerv of wrong type testdata.embeddedStringer"
            fmt.Printf("%q", notstringerv); // ERROR "Printf format %q has arg notstringerv of wrong type testdata.notstringer"
            fmt.Printf("%t", notstringerv); // ERROR "Printf format %t has arg notstringerv of wrong type testdata.notstringer"
            fmt.Printf("%t", stringerarrayv); // ERROR "Printf format %t has arg stringerarrayv of wrong type testdata.stringerarray"
            fmt.Printf("%t", notstringerarrayv); // ERROR "Printf format %t has arg notstringerarrayv of wrong type testdata.notstringerarray"
            fmt.Printf("%q", notstringerarrayv); // ERROR "Printf format %q has arg notstringerarrayv of wrong type testdata.notstringerarray"
            fmt.Printf("%d", BoolFormatter(true)); // ERROR "Printf format %d has arg BoolFormatter\(true\) of wrong type testdata.BoolFormatter"
            fmt.Printf("%z", FormatterVal(true)); // correct (the type is responsible for formatting)
            fmt.Printf("%d", FormatterVal(true)); // correct (the type is responsible for formatting)
            fmt.Printf("%s", nonemptyinterface); // correct (the type is responsible for formatting)
            fmt.Printf("%.*s %d %6g", 3L, "hi", 23L, 'x'); // ERROR "Printf format %6g has arg 'x' of wrong type rune"
            fmt.Println(); // not an error
            fmt.Println("%s", "hi"); // ERROR "Println call has possible formatting directive %s"
            fmt.Println("%v", "hi"); // ERROR "Println call has possible formatting directive %v"
            fmt.Println("0.0%"); // correct (trailing % couldn't be a formatting directive)
            fmt.Printf("%s", "hi", 3L); // ERROR "Printf call needs 1 arg but has 2 args"
            _ = fmt.Sprintf("%" + ("s"), "hi", 3L); // ERROR "Sprintf call needs 1 arg but has 2 args"
            fmt.Printf("%s%%%d", "hi", 3L); // correct
            fmt.Printf("%08s", "woo"); // correct
            fmt.Printf("% 8s", "woo"); // correct
            fmt.Printf("%.*d", 3L, 3L); // correct
            fmt.Printf("%.*d x", 3L, 3L, 3L, 3L); // ERROR "Printf call needs 2 args but has 4 args"
            fmt.Printf("%.*d x", "hi", 3L); // ERROR "Printf format %.*d uses non-int \x22hi\x22 as argument of \*"
            fmt.Printf("%.*d x", i, 3L); // correct
            fmt.Printf("%.*d x", s, 3L); // ERROR "Printf format %.\*d uses non-int s as argument of \*"
            fmt.Printf("%*% x", 0.22F); // ERROR "Printf format %\*% uses non-int 0.22 as argument of \*"
            fmt.Printf("%q %q", multi()); // ok
            fmt.Printf("%#q", "blah"); // ok
            // printf("now is the time", "buddy")          // no error "printf call has arguments but no formatting directives"
            Printf("now is the time", "buddy"); // ERROR "Printf call has arguments but no formatting directives"
            Printf("hi"); // ok
            const @string format = "%s %s\n";

            Printf(format, "hi", "there");
            Printf(format, "hi"); // ERROR "Printf format %s reads arg #2, but call has only 1 arg$"
            Printf("%s %d %.3v %q", "str", 4L); // ERROR "Printf format %.3v reads arg #3, but call has only 2 args"
            ptr<object> f = @new<ptrStringer>();
            f.Warn(0L, "%s", "hello", 3L); // ERROR "Warn call has possible formatting directive %s"
            f.Warnf(0L, "%s", "hello", 3L); // ERROR "Warnf call needs 1 arg but has 2 args"
            f.Warnf(0L, "%r", "hello"); // ERROR "Warnf format %r has unknown verb r"
            f.Warnf(0L, "%#s", "hello"); // ERROR "Warnf format %#s has unrecognized flag #"
            fmt.Printf("%#s", FormatterVal(true)); // correct (the type is responsible for formatting)
            Printf("d%", 2L); // ERROR "Printf format % is missing verb at end of string"
            Printf("%d", percentDV);
            Printf("%d", ref percentDV);
            Printf("%d", notPercentDV); // ERROR "Printf format %d has arg notPercentDV of wrong type testdata.notPercentDStruct"
            Printf("%d", ref notPercentDV); // ERROR "Printf format %d has arg &notPercentDV of wrong type \*testdata.notPercentDStruct"
            Printf("%p", ref notPercentDV); // Works regardless: we print it as a pointer.
            Printf("%s", percentSV);
            Printf("%s", ref percentSV); 
            // Good argument reorderings.
            Printf("%[1]d", 3L);
            Printf("%[1]*d", 3L, 1L);
            Printf("%[2]*[1]d", 1L, 3L);
            Printf("%[2]*.[1]*[3]d", 2L, 3L, 4L);
            fmt.Fprintf(os.Stderr, "%[2]*.[1]*[3]d", 2L, 3L, 4L); // Use Fprintf to make sure we count arguments correctly.
            // Bad argument reorderings.
            Printf("%[xd", 3L); // ERROR "Printf format %\[xd is missing closing \]"
            Printf("%[x]d x", 3L); // ERROR "Printf format has invalid argument index \[x\]"
            Printf("%[3]*s x", "hi", 2L); // ERROR "Printf format has invalid argument index \[3\]"
            _ = fmt.Sprintf("%[3]d x", 2L); // ERROR "Sprintf format has invalid argument index \[3\]"
            Printf("%[2]*.[1]*[3]d x", 2L, "hi", 4L); // ERROR "Printf format %\[2]\*\.\[1\]\*\[3\]d uses non-int \x22hi\x22 as argument of \*"
            Printf("%[0]s x", "arg1"); // ERROR "Printf format has invalid argument index \[0\]"
            Printf("%[0]d x", 1L); // ERROR "Printf format has invalid argument index \[0\]"
            // Something that satisfies the error interface.
            error e = default;
            fmt.Println(e.Error()); // ok
            // Something that looks like an error interface but isn't, such as the (*T).Error method
            // in the testing package.
            ref testing.T et1 = default;
            et1.Error(); // ok
            et1.Error("hi"); // ok
            et1.Error("%d", 3L); // ERROR "Error call has possible formatting directive %d"
            errorTest3 et3 = default;
            et3.Error(); // ok, not an error method.
            errorTest4 et4 = default;
            et4.Error(); // ok, not an error method.
            errorTest5 et5 = default;
            et5.error(); // ok, not an error method.
            // Interfaces can be used with any verb.
            var iface = default;
            fmt.Printf("%f", iface); // ok: fmt treats interfaces as transparent and iface may well have a float concrete type
            // Can't print a function.
            Printf("%d", someFunction); // ERROR "Printf format %d arg someFunction is a func value, not called"
            Printf("%v", someFunction); // ERROR "Printf format %v arg someFunction is a func value, not called"
            Println(someFunction); // ERROR "Println arg someFunction is a func value, not called"
            Printf("%p", someFunction); // ok: maybe someone wants to see the pointer
            Printf("%T", someFunction); // ok: maybe someone wants to see the type
            // Bug: used to recur forever.
            Printf("%p %x", recursiveStructV, recursiveStructV.next);
            Printf("%p %x", recursiveStruct1V, recursiveStruct1V.next);
            Printf("%p %x", recursiveSliceV, recursiveSliceV);
            Printf("%p %x", recursiveMapV, recursiveMapV); 
            // Special handling for Log.
            math.Log(3L); // OK
            ref testing.T t = default;
            t.Log("%d", 3L); // ERROR "Log call has possible formatting directive %d"
            t.Logf("%d", 3L);
            t.Logf("%d", "hi"); // ERROR "Logf format %d has arg \x22hi\x22 of wrong type string"

            // Errorf(1, "%d", 3)    // OK
            // Errorf(1, "%d", "hi") // no error "Errorf format %d has arg \x22hi\x22 of wrong type string"

            // Multiple string arguments before variadic args
            // errorf("WARNING", "foobar")            // OK
            // errorf("INFO", "s=%s, n=%d", "foo", 1) // OK
            // errorf("ERROR", "%d")                  // no error "errorf format %d reads arg #1, but call has only 0 args"

            // Printf from external package
            // externalprintf.Printf("%d", 42) // OK
            // externalprintf.Printf("foobar") // OK
            // level := 123
            // externalprintf.Logf(level, "%d", 42)                        // OK
            // externalprintf.Errorf(level, level, "foo %q bar", "foobar") // OK
            // externalprintf.Logf(level, "%d")                            // no error "Logf format %d reads arg #1, but call has only 0 args"
            // var formatStr = "%s %s"
            // externalprintf.Sprintf(formatStr, "a", "b")     // OK
            // externalprintf.Logf(level, formatStr, "a", "b") // OK

            // user-defined Println-like functions
            someStruct ss = ref new someStruct();
            ss.Log(someFunction, "foo"); // OK
            ss.Error(someFunction, someFunction); // OK
            ss.Println(); // OK
            ss.Println(1.234F, "foo"); // OK
            ss.Println(1L, someFunction); // no error "Println arg someFunction is a func value, not called"
            ss.log(someFunction); // OK
            ss.log(someFunction, "bar", 1.33F); // OK
            ss.log(someFunction, someFunction); // no error "log arg someFunction is a func value, not called"

            // indexed arguments
            Printf("%d %[3]d %d %[2]d x", 1L, 2L, 3L, 4L); // OK
            Printf("%d %[0]d %d %[2]d x", 1L, 2L, 3L, 4L); // ERROR "Printf format has invalid argument index \[0\]"
            Printf("%d %[3]d %d %[-2]d x", 1L, 2L, 3L, 4L); // ERROR "Printf format has invalid argument index \[-2\]"
            Printf("%d %[3]d %d %[2234234234234]d x", 1L, 2L, 3L, 4L); // ERROR "Printf format has invalid argument index \[2234234234234\]"
            Printf("%d %[3]d %-10d %[2]d x", 1L, 2L, 3L); // ERROR "Printf format %-10d reads arg #4, but call has only 3 args"
            Printf("%[1][3]d x", 1L, 2L); // ERROR "Printf format %\[1\]\[ has unknown verb \["
            Printf("%[1]d x", 1L, 2L); // OK
            Printf("%d %[3]d %d %[2]d x", 1L, 2L, 3L, 4L, 5L); // OK

            // wrote Println but meant Fprintln
            Printf("%p\n", os.Stdout); // OK
            Println(os.Stdout, "hello"); // ERROR "Println does not take io.Writer but has first arg os.Stdout"

            Printf(someString(), "hello"); // OK

        }

        private static @string someString()
        {
            return "X";
        }

        private partial struct someStruct
        {
        }

        // Log is non-variadic user-define Println-like function.
        // Calls to this func must be skipped when checking
        // for Println-like arguments.
        private static void Log(this ref someStruct ss, Action f, @string s)
        {
        }

        // Error is variadic user-define Println-like function.
        // Calls to this func mustn't be checked for Println-like arguments,
        // since variadic arguments type isn't interface{}.
        private static void Error(this ref someStruct ss, params Action[] args)
        {
        }

        // Println is variadic user-defined Println-like function.
        // Calls to this func must be checked for Println-like arguments.
        private static void Println(this ref someStruct ss, params object[] args)
        {
        }

        // log is variadic user-defined Println-like function.
        // Calls to this func must be checked for Println-like arguments.
        private static void log(this ref someStruct ss, Action f, params object[] args)
        {
        }

        // A function we use as a function value; it has no other purpose.
        private static void someFunction()
        {
        }

        /*
        // Printf is used by the test so we must declare it.
        func Printf(format string, args ...interface{}) {
            panic("don't call - testing only")
        }

        // Println is used by the test so we must declare it.
        func Println(args ...interface{}) {
            panic("don't call - testing only")
        }

        // Logf is used by the test so we must declare it.
        func Logf(format string, args ...interface{}) {
            panic("don't call - testing only")
        }

        // Log is used by the test so we must declare it.
        func Log(args ...interface{}) {
            panic("don't call - testing only")
        }
        */

        // printf is used by the test so we must declare it.
        private static void printf(@string format, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            panic("don't call - testing only");
        });

        /*
        // Errorf is used by the test for a case in which the first parameter
        // is not a format string.
        func Errorf(i int, format string, args ...interface{}) {
            panic("don't call - testing only")
        }

        // errorf is used by the test for a case in which the function accepts multiple
        // string parameters before variadic arguments
        func errorf(level, format string, args ...interface{}) {
            panic("don't call - testing only")
        }
        */

        // multi is used by the test.
        private static slice<object> multi() => func((_, panic, __) =>
        {
            panic("don't call - testing only");
        });

        private partial struct stringer // : long
        {
        }

        private static @string String(this stringer _p0)
        {
            return "string";
        }

        private partial struct ptrStringer // : double
        {
        }

        private static ptrStringer stringerv = default;

        private static @string String(this ref ptrStringer _p0)
        {
            return "string";
        }

        private static @string Warn(this ref ptrStringer _p0, long _p0, params object _p0)
        {
            return "warn";
        }

        private static @string Warnf(this ref ptrStringer _p0, long _p0, @string _p0, params object _p0)
        {
            return "warnf";
        }

        private partial struct embeddedStringer
        {
            public @string foo;
            public ref ptrStringer ptrStringer => ref ptrStringer_val;
            public long bar;
        }

        private static embeddedStringer embeddedStringerv = default;

        private partial struct notstringer
        {
            public double f;
        }

        private static notstringer notstringerv = default;

        private partial struct stringerarray // : array<double>
        {
        }

        private static @string String(this stringerarray _p0)
        {
            return "string";
        }

        private static stringerarray stringerarrayv = default;

        private partial struct notstringerarray // : array<double>
        {
        }

        private static notstringerarray notstringerarrayv = default;



        // A data type we can print with "%d".
        private partial struct percentDStruct
        {
            public long a;
            public slice<byte> b;
            public ptr<double> c;
        }

        private static percentDStruct percentDV = default;

        // A data type we cannot print correctly with "%d".
        private partial struct notPercentDStruct
        {
            public long a;
            public slice<byte> b;
            public bool c;
        }

        private static notPercentDStruct notPercentDV = default;

        // A data type we can print with "%s".
        private partial struct percentSStruct
        {
            public @string a;
            public slice<byte> b;
            public stringerarray C;
        }

        private static percentSStruct percentSV = default;

        private partial struct recursiveStringer // : long
        {
        }

        private static @string String(this recursiveStringer s)
        {
            _ = fmt.Sprintf("%d", s);
            _ = fmt.Sprintf("%#v", s);
            _ = fmt.Sprintf("%v", s); // ERROR "Sprintf format %v with arg s causes recursive String method call"
            _ = fmt.Sprintf("%v", ref s); // ERROR "Sprintf format %v with arg &s causes recursive String method call"
            _ = fmt.Sprintf("%T", s); // ok; does not recursively call String
            return fmt.Sprintln(s); // ERROR "Sprintln arg s causes recursive call to String method"
        }

        private partial struct recursivePtrStringer // : long
        {
        }

        private static @string String(this ref recursivePtrStringer p)
        {
            _ = fmt.Sprintf("%v", p.Value);
            _ = fmt.Sprint(ref p); // ok; prints address
            return fmt.Sprintln(p); // ERROR "Sprintln arg p causes recursive call to String method"
        }

        public partial struct BoolFormatter // : bool
        {
        }

        private static void Format(this ref BoolFormatter _p0, fmt.State _p0, int _p0)
        {
        }

        // Formatter with value receiver
        public partial struct FormatterVal // : bool
        {
        }

        public static void Format(this FormatterVal _p0, fmt.State _p0, int _p0)
        {
        }

        public partial struct RecursiveSlice // : slice<RecursiveSlice>
        {
        }

        private static RecursiveSlice recursiveSliceV = ref new RecursiveSlice();

        public partial struct RecursiveMap // : map<long, RecursiveMap>
        {
        }

        private static var recursiveMapV = make(RecursiveMap);

        public partial struct RecursiveStruct
        {
            public ptr<RecursiveStruct> next;
        }

        private static RecursiveStruct recursiveStructV = ref new RecursiveStruct();

        public partial struct RecursiveStruct1
        {
            public ptr<RecursiveStruct2> next;
        }

        public partial struct RecursiveStruct2
        {
            public ptr<RecursiveStruct1> next;
        }

        private static RecursiveStruct1 recursiveStruct1V = ref new RecursiveStruct1();

        private partial struct unexportedInterface
        {
        }

        // Issue 17798: unexported ptrStringer cannot be formatted.
        private partial struct unexportedStringer
        {
            public ptrStringer t;
        }
        private partial struct unexportedStringerOtherFields
        {
            public @string s;
            public ptrStringer t;
            public @string S;
        }

        // Issue 17798: unexported error cannot be formatted.
        private partial struct unexportedError
        {
            public error e;
        }
        private partial struct unexportedErrorOtherFields
        {
            public @string s;
            public error e;
            public @string S;
        }

        private partial struct errorer
        {
        }

        private static @string Error(this errorer e)
        {
            return "errorer";
        }

        private partial struct unexportedCustomError
        {
            public errorer e;
        }

        private partial interface errorInterface : error
        {
            void ExtraMethod();
        }

        private partial struct unexportedErrorInterface
        {
            public errorInterface e;
        }

        public static void UnexportedStringerOrError()
        {
            fmt.Printf("%s", new unexportedInterface("foo")); // ok; prints {foo}
            fmt.Printf("%s", new unexportedInterface(3)); // ok; we can't see the problem

            unexportedStringer us = new unexportedStringer();
            fmt.Printf("%s", us); // ERROR "Printf format %s has arg us of wrong type testdata.unexportedStringer"
            fmt.Printf("%s", ref us); // ERROR "Printf format %s has arg &us of wrong type [*]testdata.unexportedStringer"

            unexportedStringerOtherFields usf = new unexportedStringerOtherFields(s:"foo",S:"bar",);
            fmt.Printf("%s", usf); // ERROR "Printf format %s has arg usf of wrong type testdata.unexportedStringerOtherFields"
            fmt.Printf("%s", ref usf); // ERROR "Printf format %s has arg &usf of wrong type [*]testdata.unexportedStringerOtherFields"

            unexportedError ue = new unexportedError(e:&errorer{},);
            fmt.Printf("%s", ue); // ERROR "Printf format %s has arg ue of wrong type testdata.unexportedError"
            fmt.Printf("%s", ref ue); // ERROR "Printf format %s has arg &ue of wrong type [*]testdata.unexportedError"

            unexportedErrorOtherFields uef = new unexportedErrorOtherFields(s:"foo",e:&errorer{},S:"bar",);
            fmt.Printf("%s", uef); // ERROR "Printf format %s has arg uef of wrong type testdata.unexportedErrorOtherFields"
            fmt.Printf("%s", ref uef); // ERROR "Printf format %s has arg &uef of wrong type [*]testdata.unexportedErrorOtherFields"

            unexportedCustomError uce = new unexportedCustomError(e:errorer{},);
            fmt.Printf("%s", uce); // ERROR "Printf format %s has arg uce of wrong type testdata.unexportedCustomError"

            unexportedErrorInterface uei = new unexportedErrorInterface();
            fmt.Printf("%s", uei); // ERROR "Printf format %s has arg uei of wrong type testdata.unexportedErrorInterface"
            fmt.Println("foo\n", "bar"); // not an error

            fmt.Println("foo\n"); // ERROR "Println arg list ends with redundant newline"
            fmt.Println("foo\\n"); // not an error
            fmt.Println("foo\\n"); // not an error

            long intSlice = new slice<long>(new long[] { 3, 4 });
            fmt.Printf("%s", intSlice); // ERROR "Printf format %s has arg intSlice of wrong type \[\]int"
            array<unexportedStringer> nonStringerArray = new array<unexportedStringer>(new unexportedStringer[] { {} });
            fmt.Printf("%s", nonStringerArray); // ERROR "Printf format %s has arg nonStringerArray of wrong type \[1\]testdata.unexportedStringer"
            fmt.Printf("%s", new slice<stringer>(new stringer[] { 3, 4 })); // not an error
            fmt.Printf("%s", new array<stringer>(new stringer[] { 3, 4 })); // not an error
        }

        // TODO: Disable complaint about '0' for Go 1.10. To be fixed properly in 1.11.
        // See issues 23598 and 23605.
        public static void DisableErrorForFlag0()
        {
            fmt.Printf("%0t", true);
        }
    }
}}}
