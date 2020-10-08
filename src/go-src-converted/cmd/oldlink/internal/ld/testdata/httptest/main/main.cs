// A small test program that uses the net/http package. There is
// nothing special about net/http here, this is just a convenient way
// to pull in a lot of code.

// package main -- go2cs converted at 2020 October 08 04:42:06 UTC
// Original source: C:\Go\src\cmd\oldlink\internal\ld\testdata\httptest\main\main.go
using http = go.net.http_package;
using httptest = go.net.http.httptest_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private partial struct statusHandler // : long
        {
        }

        private static void ServeHTTP(this ptr<statusHandler> _addr_h, http.ResponseWriter w, ptr<http.Request> _addr_r)
        {
            ref statusHandler h = ref _addr_h.val;
            ref http.Request r = ref _addr_r.val;

            w.WriteHeader(int(h.val));
        }

        private static void Main() => func((defer, _, __) =>
        {
            ref var status = ref heap(statusHandler(http.StatusNotFound), out ptr<var> _addr_status);
            var s = httptest.NewServer(_addr_status);
            defer(s.Close());
        });
    }
}
