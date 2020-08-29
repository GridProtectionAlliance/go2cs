// package testdata -- go2cs converted at 2020 August 29 10:10:32 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\httpresponse.go
using log = go.log_package;
using http = go.net.http_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
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

        private static void badHTTPHead() => func((defer, _, __) =>
        {
            var (res, err) = http.Head("http://foo.com");
            defer(res.Body.Close()); // ERROR "using res before checking for errors"
            if (err != null)
            {
                log.Fatal(err);
            }
        });

        private static void goodClientGet() => func((defer, _, __) =>
        {
            var client = http.DefaultClient;
            var (res, err) = client.Get("http://foo.com");
            if (err != null)
            {
                log.Fatal(err);
            }
            defer(res.Body.Close());
        });

        private static void badClientPtrGet() => func((defer, _, __) =>
        {
            var client = http.DefaultClient;
            var (resp, err) = client.Get("http://foo.com");
            defer(resp.Body.Close()); // ERROR "using resp before checking for errors"
            if (err != null)
            {
                log.Fatal(err);
            }
        });

        private static void badClientGet() => func((defer, _, __) =>
        {
            http.Client client = new http.Client();
            var (resp, err) = client.Get("http://foo.com");
            defer(resp.Body.Close()); // ERROR "using resp before checking for errors"
            if (err != null)
            {
                log.Fatal(err);
            }
        });

        private static void badClientPtrDo() => func((defer, _, __) =>
        {
            var client = http.DefaultClient;
            var (req, err) = http.NewRequest("GET", "http://foo.com", null);
            if (err != null)
            {
                log.Fatal(err);
            }
            var (resp, err) = client.Do(req);
            defer(resp.Body.Close()); // ERROR "using resp before checking for errors"
            if (err != null)
            {
                log.Fatal(err);
            }
        });

        private static void badClientDo() => func((defer, _, __) =>
        {
            http.Client client = default;
            var (req, err) = http.NewRequest("GET", "http://foo.com", null);
            if (err != null)
            {
                log.Fatal(err);
            }
            var (resp, err) = client.Do(req);
            defer(resp.Body.Close()); // ERROR "using resp before checking for errors"
            if (err != null)
            {
                log.Fatal(err);
            }
        });
    }
}}}
