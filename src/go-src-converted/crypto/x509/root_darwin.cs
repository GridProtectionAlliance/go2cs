// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run root_darwin_arm_gen.go -output root_darwin_armx.go

// package x509 -- go2cs converted at 2020 August 29 08:31:46 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root_darwin.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using sha1 = go.crypto.sha1_package;
using pem = go.encoding.pem_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using user = go.os.user_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using sync = go.sync_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        private static var debugExecDarwinRoots = strings.Contains(os.Getenv("GODEBUG"), "x509roots=1");

        private static (slice<slice<ref Certificate>>, error) systemVerify(this ref Certificate c, ref VerifyOptions opts)
        {
            return (null, null);
        }

        // This code is only used when compiling without cgo.
        // It is here, instead of root_nocgo_darwin.go, so that tests can check it
        // even if the tests are run with cgo enabled.
        // The linker will not include these unused functions in binaries built with cgo enabled.

        // execSecurityRoots finds the macOS list of trusted root certificates
        // using only command-line tools. This is our fallback path when cgo isn't available.
        //
        // The strategy is as follows:
        //
        // 1. Run "security trust-settings-export" and "security
        //    trust-settings-export -d" to discover the set of certs with some
        //    user-tweaked trust policy. We're too lazy to parse the XML (at
        //    least at this stage of Go 1.8) to understand what the trust
        //    policy actually is. We just learn that there is _some_ policy.
        //
        // 2. Run "security find-certificate" to dump the list of system root
        //    CAs in PEM format.
        //
        // 3. For each dumped cert, conditionally verify it with "security
        //    verify-cert" if that cert was in the set discovered in Step 1.
        //    Without the Step 1 optimization, running "security verify-cert"
        //    150-200 times takes 3.5 seconds. With the optimization, the
        //    whole process takes about 180 milliseconds with 1 untrusted root
        //    CA. (Compared to 110ms in the cgo path)
        private static (ref CertPool, error) execSecurityRoots() => func((defer, _, __) =>
        {
            var (hasPolicy, err) = getCertsWithTrustPolicy();
            if (err != null)
            {
                return (null, err);
            }
            if (debugExecDarwinRoots)
            {
                println(fmt.Sprintf("crypto/x509: %d certs have a trust policy", len(hasPolicy)));
            }
            @string args = new slice<@string>(new @string[] { "find-certificate", "-a", "-p", "/System/Library/Keychains/SystemRootCertificates.keychain", "/Library/Keychains/System.keychain" });

            var (u, err) = user.Current();
            if (err != null)
            {
                if (debugExecDarwinRoots)
                {
                    println(fmt.Sprintf("crypto/x509: get current user: %v", err));
                }
            }
            else
            {
                args = append(args, filepath.Join(u.HomeDir, "/Library/Keychains/login.keychain"), filepath.Join(u.HomeDir, "/Library/Keychains/login.keychain-db"));
            }
            var cmd = exec.Command("/usr/bin/security", args);
            var (data, err) = cmd.Output();
            if (err != null)
            {
                return (null, err);
            }
            sync.Mutex mu = default;            var roots = NewCertPool();            long numVerified = default;

            var blockCh = make_channel<ref pem.Block>();
            sync.WaitGroup wg = default; 

            // Using 4 goroutines to pipe into verify-cert seems to be
            // about the best we can do. The verify-cert binary seems to
            // just RPC to another server with coarse locking anyway, so
            // running 16 at a time for instance doesn't help at all. Due
            // to the "if hasPolicy" check below, though, we will rarely
            // (or never) call verify-cert on stock macOS systems, though.
            // The hope is that we only call verify-cert when the user has
            // tweaked their trust policy. These 4 goroutines are only
            // defensive in the pathological case of many trust edits.
            for (long i = 0L; i < 4L; i++)
            {
                wg.Add(1L);
                go_(() => () =>
                {
                    defer(wg.Done());
                    {
                        var block__prev2 = block;

                        foreach (var (__block) in blockCh)
                        {
                            block = __block;
                            var (cert, err) = ParseCertificate(block.Bytes);
                            if (err != null)
                            {
                                continue;
                            }
                            var sha1CapHex = fmt.Sprintf("%X", sha1.Sum(block.Bytes));

                            var valid = true;
                            long verifyChecks = 0L;
                            if (hasPolicy[sha1CapHex])
                            {
                                verifyChecks++;
                                if (!verifyCertWithSystem(block, cert))
                                {
                                    valid = false;
                                }
                            }
                            mu.Lock();
                            numVerified += verifyChecks;
                            if (valid)
                            {
                                roots.AddCert(cert);
                            }
                            mu.Unlock();
                        }

                        block = block__prev2;
                    }

                }());
            }

            while (len(data) > 0L)
            {
                ref pem.Block block = default;
                block, data = pem.Decode(data);
                if (block == null)
                {
                    break;
                }
                if (block.Type != "CERTIFICATE" || len(block.Headers) != 0L)
                {
                    continue;
                }
                blockCh.Send(block);
            }

            close(blockCh);
            wg.Wait();

            if (debugExecDarwinRoots)
            {
                mu.Lock();
                defer(mu.Unlock());
                println(fmt.Sprintf("crypto/x509: ran security verify-cert %d times", numVerified));
            }
            return (roots, null);
        });

        private static bool verifyCertWithSystem(ref pem.Block _block, ref Certificate _cert) => func(_block, _cert, (ref pem.Block block, ref Certificate cert, Defer defer, Panic _, Recover __) =>
        {
            var data = pem.EncodeToMemory(block);

            var (f, err) = ioutil.TempFile("", "cert");
            if (err != null)
            {
                fmt.Fprintf(os.Stderr, "can't create temporary file for cert: %v", err);
                return false;
            }
            defer(os.Remove(f.Name()));
            {
                var err__prev1 = err;

                var (_, err) = f.Write(data);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "can't write temporary file for cert: %v", err);
                    return false;
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                var err = f.Close();

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "can't write temporary file for cert: %v", err);
                    return false;
                }

                err = err__prev1;

            }
            var cmd = exec.Command("/usr/bin/security", "verify-cert", "-c", f.Name(), "-l", "-L");
            bytes.Buffer stderr = default;
            if (debugExecDarwinRoots)
            {
                cmd.Stderr = ref stderr;
            }
            {
                var err__prev1 = err;

                err = cmd.Run();

                if (err != null)
                {
                    if (debugExecDarwinRoots)
                    {
                        println(fmt.Sprintf("crypto/x509: verify-cert rejected %s: %q", cert.Subject.CommonName, bytes.TrimSpace(stderr.Bytes())));
                    }
                    return false;
                }

                err = err__prev1;

            }
            if (debugExecDarwinRoots)
            {
                println(fmt.Sprintf("crypto/x509: verify-cert approved %s", cert.Subject.CommonName));
            }
            return true;
        });

        // getCertsWithTrustPolicy returns the set of certs that have a
        // possibly-altered trust policy. The keys of the map are capitalized
        // sha1 hex of the raw cert.
        // They are the certs that should be checked against `security
        // verify-cert` to see whether the user altered the default trust
        // settings. This code is only used for cgo-disabled builds.
        private static (map<@string, bool>, error) getCertsWithTrustPolicy() => func((defer, _, __) =>
        {
            map set = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
            var (td, err) = ioutil.TempDir("", "x509trustpolicy");
            if (err != null)
            {
                return (null, err);
            }
            defer(os.RemoveAll(td));
            Func<@string, @string[], error> run = (file, args) =>
            {
                file = filepath.Join(td, file);
                args = append(args, file);
                var cmd = exec.Command("/usr/bin/security", args);
                bytes.Buffer stderr = default;
                cmd.Stderr = ref stderr;
                {
                    var err__prev1 = err;

                    var err = cmd.Run();

                    if (err != null)
                    { 
                        // If there are no trust settings, the
                        // `security trust-settings-export` command
                        // fails with:
                        //    exit status 1, SecTrustSettingsCreateExternalRepresentation: No Trust Settings were found.
                        // Rather than match on English substrings that are probably
                        // localized on macOS, just interpret any failure to mean that
                        // there are no trust settings.
                        if (debugExecDarwinRoots)
                        {
                            println(fmt.Sprintf("crypto/x509: exec %q: %v, %s", cmd.Args, err, stderr.Bytes()));
                        }
                        return null;
                    }

                    err = err__prev1;

                }

                var (f, err) = os.Open(file);
                if (err != null)
                {
                    return err;
                }
                defer(f.Close()); 

                // Gather all the runs of 40 capitalized hex characters.
                var br = bufio.NewReader(f);
                bytes.Buffer hexBuf = default;
                while (true)
                {
                    var (b, err) = br.ReadByte();
                    char isHex = ('A' <= b && b <= 'F') || ('0' <= b && b <= '9');
                    if (isHex)
                    {
                        hexBuf.WriteByte(b);
                    }
                    else
                    {
                        if (hexBuf.Len() == 40L)
                        {
                            set[hexBuf.String()] = true;
                        }
                        hexBuf.Reset();
                    }
                    if (err == io.EOF)
                    {
                        break;
                    }
                    if (err != null)
                    {
                        return err;
                    }
                }


                return null;
            }
;
            {
                var err__prev1 = err;

                err = run("user", "trust-settings-export");

                if (err != null)
                {
                    return (null, fmt.Errorf("dump-trust-settings (user): %v", err));
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = run("admin", "trust-settings-export", "-d");

                if (err != null)
                {
                    return (null, fmt.Errorf("dump-trust-settings (admin): %v", err));
                }

                err = err__prev1;

            }
            return (set, null);
        });
    }
}}
