// package httpresponse -- go2cs converted at 2020 October 08 04:58:36 UTC
// import "cmd/vet/testdata/httpresponse" ==> using httpresponse = go.cmd.vet.testdata.httpresponse_package
// Original source: C:\Go\src\cmd\vet\testdata\httpresponse\httpresponse.go
using log = go.log_package;
using http = go.net.http_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet {
namespace testdata
{
    public static partial class httpresponse_package
    {
        private static void goodHTTPGet() => func((defer, _, __) =>
        {
            var (res, err) = http.Get("http://foo.com");
            if (err != null)
            {
                log.Fatal(err);
            }
            defer(res.Body.Close());

        });

        private static void badHTTPGet() => func((defer, _, __) =>
        {
            var (res, err) = http.Get("http://foo.com");
            defer(res.Body.Close()); // ERROR "using res before checking for errors"
            if (err != null)
            {
                log.Fatal(err);
            }

        });
    }
}}}}
