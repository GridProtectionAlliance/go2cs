// A small test program that uses the net/http package. There is
// nothing special about net/http here, this is just a convenient way
// to pull in a lot of code.

// package main -- go2cs converted at 2022 March 06 23:22:34 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\httptest\main\main.go
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;

namespace go;

public static partial class main_package {

private partial struct statusHandler { // : nint
}

private static void ServeHTTP(this ptr<statusHandler> _addr_h, http.ResponseWriter w, ptr<http.Request> _addr_r) {
    ref statusHandler h = ref _addr_h.val;
    ref http.Request r = ref _addr_r.val;

    w.WriteHeader(int(h.val));
}

private static void Main() => func((defer, _, _) => {
    ref var status = ref heap(statusHandler(http.StatusNotFound), out ptr<var> _addr_status);
    var s = httptest.NewServer(_addr_status);
    defer(s.Close());
});

} // end main_package
