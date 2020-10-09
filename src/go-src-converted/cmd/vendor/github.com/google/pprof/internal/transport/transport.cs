// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Package transport provides a mechanism to send requests with https cert,
// key, and CA.
// package transport -- go2cs converted at 2020 October 09 05:53:49 UTC
// import "cmd/vendor/github.com/google/pprof/internal/transport" ==> using transport = go.cmd.vendor.github.com.google.pprof.@internal.transport_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\transport\transport.go
using tls = go.crypto.tls_package;
using x509 = go.crypto.x509_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using http = go.net.http_package;
using sync = go.sync_package;

using plugin = go.github.com.google.pprof.@internal.plugin_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class transport_package
    {
        private partial struct transport
        {
            public ptr<@string> cert;
            public ptr<@string> key;
            public ptr<@string> ca;
            public ptr<x509.CertPool> caCertPool;
            public slice<tls.Certificate> certs;
            public sync.Once initOnce;
            public error initErr;
        }

        private static readonly @string extraUsage = (@string)"    -tls_cert             TLS client certificate file for fetching profile and sy" +
    "mbols\n    -tls_key              TLS private key file for fetching profile and sy" +
    "mbols\n    -tls_ca               TLS CA certs file for fetching profile and symbo" +
    "ls";

        // New returns a round tripper for making requests with the
        // specified cert, key, and ca. The flags tls_cert, tls_key, and tls_ca are
        // added to the flagset to allow a user to specify the cert, key, and ca. If
        // the flagset is nil, no flags will be added, and users will not be able to
        // use these flags.


        // New returns a round tripper for making requests with the
        // specified cert, key, and ca. The flags tls_cert, tls_key, and tls_ca are
        // added to the flagset to allow a user to specify the cert, key, and ca. If
        // the flagset is nil, no flags will be added, and users will not be able to
        // use these flags.
        public static http.RoundTripper New(plugin.FlagSet flagset)
        {
            if (flagset == null)
            {
                return addr(new transport());
            }

            flagset.AddExtraUsage(extraUsage);
            return addr(new transport(cert:flagset.String("tls_cert","","TLS client certificate file for fetching profile and symbols"),key:flagset.String("tls_key","","TLS private key file for fetching profile and symbols"),ca:flagset.String("tls_ca","","TLS CA certs file for fetching profile and symbols"),));

        }

        // initialize uses the cert, key, and ca to initialize the certs
        // to use these when making requests.
        private static error initialize(this ptr<transport> _addr_tr)
        {
            ref transport tr = ref _addr_tr.val;

            @string cert = default;            @string key = default;            @string ca = default;

            if (tr.cert != null)
            {
                cert = tr.cert.val;
            }

            if (tr.key != null)
            {
                key = tr.key.val;
            }

            if (tr.ca != null)
            {
                ca = tr.ca.val;
            }

            if (cert != "" && key != "")
            {
                var (tlsCert, err) = tls.LoadX509KeyPair(cert, key);
                if (err != null)
                {
                    return error.As(fmt.Errorf("could not load certificate/key pair specified by -tls_cert and -tls_key: %v", err))!;
                }

                tr.certs = new slice<tls.Certificate>(new tls.Certificate[] { tlsCert });

            }
            else if (cert == "" && key != "")
            {
                return error.As(fmt.Errorf("-tls_key is specified, so -tls_cert must also be specified"))!;
            }
            else if (cert != "" && key == "")
            {
                return error.As(fmt.Errorf("-tls_cert is specified, so -tls_key must also be specified"))!;
            }

            if (ca != "")
            {
                var caCertPool = x509.NewCertPool();
                var (caCert, err) = ioutil.ReadFile(ca);
                if (err != null)
                {
                    return error.As(fmt.Errorf("could not load CA specified by -tls_ca: %v", err))!;
                }

                caCertPool.AppendCertsFromPEM(caCert);
                tr.caCertPool = caCertPool;

            }

            return error.As(null!)!;

        }

        // RoundTrip executes a single HTTP transaction, returning
        // a Response for the provided Request.
        private static (ptr<http.Response>, error) RoundTrip(this ptr<transport> _addr_tr, ptr<http.Request> _addr_req)
        {
            ptr<http.Response> _p0 = default!;
            error _p0 = default!;
            ref transport tr = ref _addr_tr.val;
            ref http.Request req = ref _addr_req.val;

            tr.initOnce.Do(() =>
            {
                tr.initErr = tr.initialize();
            });
            if (tr.initErr != null)
            {
                return (_addr_null!, error.As(tr.initErr)!);
            }

            ptr<tls.Config> tlsConfig = addr(new tls.Config(RootCAs:tr.caCertPool,Certificates:tr.certs,));

            if (req.URL.Scheme == "https+insecure")
            { 
                // Make shallow copy of request, and req.URL, so the request's URL can be
                // modified.
                ref http.Request r = ref heap(req, out ptr<http.Request> _addr_r);
                r.URL.val = req.URL.val;
                _addr_req = _addr_r;
                req = ref _addr_req.val;
                tlsConfig.InsecureSkipVerify = true;
                req.URL.Scheme = "https";

            }

            http.Transport transport = new http.Transport(Proxy:http.ProxyFromEnvironment,TLSClientConfig:tlsConfig,);

            return _addr_transport.RoundTrip(req)!;

        }
    }
}}}}}}}
