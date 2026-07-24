// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using fs = io.fs_package;
using os = os_package;
using time = time_package;
using io;

partial class errors_test_package {

// MyError is an error implementation that includes a time and message.
[GoType] partial struct MyError {
    public time.Time When;
    public @string What;
}

public static @string Error(this MyError e) {
    return fmt.Sprintf("%v: %v"u8, e.When, e.What);
}

internal static error oops() {
    return new MyError(
        time.Date(1989, 3, 15, 22, 30, 0, 0, time.ΔUTC),
        "the file system has gone away"
    );
}

public static void Example() {
    {
        var err = oops(); if (err != default!) {
            fmt.Println(err);
        }
    }
}

// Output: 1989-03-15 22:30:00 +0000 UTC: the file system has gone away
public static void ExampleNew() {
    var err = errors.New("emit macho dwarf: elf header corrupted"u8);
    if (err != default!) {
        fmt.Print(err);
    }
}

// Output: emit macho dwarf: elf header corrupted

// The fmt package's Errorf function lets us use the package's formatting
// features to create descriptive error messages.
public static void ExampleNew_errorf() {
    @string name = "bimmler"u8;
    const nint id = 17;
    var err = fmt.Errorf("user %q (id %d) not found"u8, name, id);
    if (err != default!) {
        fmt.Print(err);
    }
}

// Output: user "bimmler" (id 17) not found
public static void ExampleJoin() {
    var err1 = errors.New("err1"u8);
    var err2 = errors.New("err2"u8);
    var err = errors.Join(err1, err2);
    fmt.Println(err);
    if (errors.Is(err, err1)) {
        fmt.Println("err is err1");
    }
    if (errors.Is(err, err2)) {
        fmt.Println("err is err2");
    }
}

// Output:
// err1
// err2
// err is err1
// err is err2
public static void ExampleIs() {
    {
        var (_, err) = os.Open("non-existing"u8); if (err != default!) {
            if (errors.Is(err, fs.ErrNotExist)){
                fmt.Println("file does not exist");
            } else {
                fmt.Println(err);
            }
        }
    }
}

// Output:
// file does not exist
public static void ExampleAs() {
    {
        var (_, err) = os.Open("non-existing"u8); if (err != default!) {
            ref var pathError = ref heap<ж<fs.PathError>>(out var ᏑpathError);
            if (errors.As(err, ᏑpathError)){
                fmt.Println("Failed at path:", (~pathError).Path);
            } else {
                fmt.Println(err);
            }
        }
    }
}

// Output:
// Failed at path: non-existing
public static void ExampleUnwrap() {
    var err1 = errors.New("error1"u8);
    var err2 = fmt.Errorf("error2: [%w]"u8, err1);
    fmt.Println(err2);
    fmt.Println(errors.Unwrap(err2));
}

// Output:
// error2: [error1]
// error1

} // end errors_test_package
