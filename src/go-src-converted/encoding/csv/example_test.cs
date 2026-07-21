// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using csv = go.encoding.csv_package;
using fmt = fmt_package;
using io = io_package;
using log = log_package;
using os = os_package;
using strings = strings_package;
using go.encoding;

partial class csv_test_package {

public static void ExampleReader() {
    @string @in = """
first_name,last_name,username
"Rob","Pike",rob
Ken,Thompson,ken
"Robert","Griesemer","gri"

"""u8;
    var r = csv.NewReader(new strings_ReaderжReader(strings.NewReader(@in)));
    while (ᐧ) {
        var (record, err) = r.Read();
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != default!) {
            log.Fatal(err);
        }
        fmt.Println(record);
    }
}

// Output:
// [first_name last_name username]
// [Rob Pike rob]
// [Ken Thompson ken]
// [Robert Griesemer gri]

// This example shows how csv.Reader can be configured to handle other
// types of CSV files.
public static void ExampleReader_options() {
    @string @in = """
first_name;last_name;username
"Rob";"Pike";rob
# lines beginning with a # character are ignored
Ken;Thompson;ken
"Robert";"Griesemer";"gri"

"""u8;
    var r = csv.NewReader(new strings_ReaderжReader(strings.NewReader(@in)));
    r.Value.Comma = (rune)';';
    r.Value.Comment = (rune)'#';
    var (records, err) = r.ReadAll();
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Print(records);
}

// Output:
// [[first_name last_name username] [Rob Pike rob] [Ken Thompson ken] [Robert Griesemer gri]]
public static void ExampleReader_ReadAll() {
    @string @in = """
first_name,last_name,username
"Rob","Pike",rob
Ken,Thompson,ken
"Robert","Griesemer","gri"

"""u8;
    var r = csv.NewReader(new strings_ReaderжReader(strings.NewReader(@in)));
    var (records, err) = r.ReadAll();
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Print(records);
}

// Output:
// [[first_name last_name username] [Rob Pike rob] [Ken Thompson ken] [Robert Griesemer gri]]
public static void ExampleWriter() {
    var records = new slice<@string>[]{
        new @string[]{"first_name"u8, "last_name"u8, "username"u8}.slice(),
        new @string[]{"Rob"u8, "Pike"u8, "rob"u8}.slice(),
        new @string[]{"Ken"u8, "Thompson"u8, "ken"u8}.slice(),
        new @string[]{"Robert"u8, "Griesemer"u8, "gri"u8}.slice()
    }.slice();
    var w = csv.NewWriter(new os.FileжWriter(os.Stdout));
    foreach (var (_, record) in records) {
        {
            var err = w.Write(record); if (err != default!) {
                log.Fatalln("error writing record to csv:", err);
            }
        }
    }
    // Write any buffered data to the underlying writer (standard output).
    w.Flush();
    {
        var err = w.Error(); if (err != default!) {
            log.Fatal(err);
        }
    }
}

// Output:
// first_name,last_name,username
// Rob,Pike,rob
// Ken,Thompson,ken
// Robert,Griesemer,gri
public static void ExampleWriter_WriteAll() {
    var records = new slice<@string>[]{
        new @string[]{"first_name"u8, "last_name"u8, "username"u8}.slice(),
        new @string[]{"Rob"u8, "Pike"u8, "rob"u8}.slice(),
        new @string[]{"Ken"u8, "Thompson"u8, "ken"u8}.slice(),
        new @string[]{"Robert"u8, "Griesemer"u8, "gri"u8}.slice()
    }.slice();
    var w = csv.NewWriter(new os.FileжWriter(os.Stdout));
    w.WriteAll(records);
    // calls Flush internally
    {
        var err = w.Error(); if (err != default!) {
            log.Fatalln("error writing csv:", err);
        }
    }
}

// Output:
// first_name,last_name,username
// Rob,Pike,rob
// Ken,Thompson,ken
// Robert,Griesemer,gri

} // end csv_test_package
