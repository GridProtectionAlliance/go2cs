// package html -- go2cs converted at 2022 March 06 23:15:13 UTC
// import "cmd/cover/testdata/html" ==> using html = go.cmd.cover.testdata.html_package
// Original source: C:\Program Files\Go\src\cmd\cover\testdata\html\html.go
using fmt = go.fmt_package;

namespace go.cmd.cover.testdata;

public static partial class html_package {

    // This file is tested by html_test.go.
    // The comments below are markers for extracting the annotated source
    // from the HTML output.

    // This is a regression test for incorrect sorting of boundaries
    // that coincide, specifically for empty select clauses.
    // START f
private static void f() {
    var ch = make_channel<nint>();
}

// END f

// https://golang.org/issue/25767
// START g
private static void g() {
    if (false) {
        fmt.Printf("Hello");
    }
}

// END g

} // end html_package
