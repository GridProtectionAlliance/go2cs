// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd linux nacl netbsd openbsd solaris

// package x509 -- go2cs converted at 2020 August 29 08:31:55 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root_unix.go
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        // Possible directories with certificate files; stop after successfully
        // reading at least one file from a directory.
        private static @string certDirectories = new slice<@string>(new @string[] { "/etc/ssl/certs", "/system/etc/security/cacerts", "/usr/local/share/certs", "/etc/pki/tls/certs", "/etc/openssl/certs" });

 
        // certFileEnv is the environment variable which identifies where to locate
        // the SSL certificate file. If set this overrides the system default.
        private static readonly @string certFileEnv = "SSL_CERT_FILE"; 

        // certDirEnv is the environment variable which identifies which directory
        // to check for SSL certificate files. If set this overrides the system default.
        private static readonly @string certDirEnv = "SSL_CERT_DIR";

        private static (slice<slice<ref Certificate>>, error) systemVerify(this ref Certificate c, ref VerifyOptions opts)
        {
            return (null, null);
        }

        private static (ref CertPool, error) loadSystemRoots()
        {
            var roots = NewCertPool();

            var files = certFiles;
            {
                var f = os.Getenv(certFileEnv);

                if (f != "")
                {
                    files = new slice<@string>(new @string[] { f });
                }

            }

            error firstErr = default;
            foreach (var (_, file) in files)
            {
                var (data, err) = ioutil.ReadFile(file);
                if (err == null)
                {
                    roots.AppendCertsFromPEM(data);
                    break;
                }
                if (firstErr == null && !os.IsNotExist(err))
                {
                    firstErr = error.As(err);
                }
            }
            var dirs = certDirectories;
            {
                var d = os.Getenv(certDirEnv);

                if (d != "")
                {
                    dirs = new slice<@string>(new @string[] { d });
                }

            }

            foreach (var (_, directory) in dirs)
            {
                var (fis, err) = ioutil.ReadDir(directory);
                if (err != null)
                {
                    if (firstErr == null && !os.IsNotExist(err))
                    {
                        firstErr = error.As(err);
                    }
                    continue;
                }
                var rootsAdded = false;
                foreach (var (_, fi) in fis)
                {
                    (data, err) = ioutil.ReadFile(directory + "/" + fi.Name());
                    if (err == null && roots.AppendCertsFromPEM(data))
                    {
                        rootsAdded = true;
                    }
                }
                if (rootsAdded)
                {
                    return (roots, null);
                }
            }
            if (len(roots.certs) > 0L)
            {
                return (roots, null);
            }
            return (null, firstErr);
        }
    }
}}
