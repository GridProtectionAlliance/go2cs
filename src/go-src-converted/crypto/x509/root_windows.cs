// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x509 -- go2cs converted at 2020 August 29 08:31:56 UTC
// import "crypto/x509" ==> using x509 = go.crypto.x509_package
// Original source: C:\Go\src\crypto\x509\root_windows.go
using errors = go.errors_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class x509_package
    {
        // Creates a new *syscall.CertContext representing the leaf certificate in an in-memory
        // certificate store containing itself and all of the intermediate certificates specified
        // in the opts.Intermediates CertPool.
        //
        // A pointer to the in-memory store is available in the returned CertContext's Store field.
        // The store is automatically freed when the CertContext is freed using
        // syscall.CertFreeCertificateContext.
        private static (ref syscall.CertContext, error) createStoreContext(ref Certificate _leaf, ref VerifyOptions _opts) => func(_leaf, _opts, (ref Certificate leaf, ref VerifyOptions opts, Defer defer, Panic _, Recover __) =>
        {
            ref syscall.CertContext storeCtx = default;

            var (leafCtx, err) = syscall.CertCreateCertificateContext(syscall.X509_ASN_ENCODING | syscall.PKCS_7_ASN_ENCODING, ref leaf.Raw[0L], uint32(len(leaf.Raw)));
            if (err != null)
            {
                return (null, err);
            }
            defer(syscall.CertFreeCertificateContext(leafCtx));

            var (handle, err) = syscall.CertOpenStore(syscall.CERT_STORE_PROV_MEMORY, 0L, 0L, syscall.CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG, 0L);
            if (err != null)
            {
                return (null, err);
            }
            defer(syscall.CertCloseStore(handle, 0L));

            err = syscall.CertAddCertificateContextToStore(handle, leafCtx, syscall.CERT_STORE_ADD_ALWAYS, ref storeCtx);
            if (err != null)
            {
                return (null, err);
            }
            if (opts.Intermediates != null)
            {
                foreach (var (_, intermediate) in opts.Intermediates.certs)
                {
                    var (ctx, err) = syscall.CertCreateCertificateContext(syscall.X509_ASN_ENCODING | syscall.PKCS_7_ASN_ENCODING, ref intermediate.Raw[0L], uint32(len(intermediate.Raw)));
                    if (err != null)
                    {
                        return (null, err);
                    }
                    err = syscall.CertAddCertificateContextToStore(handle, ctx, syscall.CERT_STORE_ADD_ALWAYS, null);
                    syscall.CertFreeCertificateContext(ctx);
                    if (err != null)
                    {
                        return (null, err);
                    }
                }
            }
            return (storeCtx, null);
        });

        // extractSimpleChain extracts the final certificate chain from a CertSimpleChain.
        private static (slice<ref Certificate>, error) extractSimpleChain(ptr<ptr<syscall.CertSimpleChain>> simpleChain, long count)
        {
            if (simpleChain == null || count == 0L)
            {
                return (null, errors.New("x509: invalid simple chain"));
            }
            ref array<ref syscall.CertSimpleChain> simpleChains = new ptr<ref array<ref syscall.CertSimpleChain>>(@unsafe.Pointer(simpleChain))[..];
            var lastChain = simpleChains[count - 1L];
            ref array<ref syscall.CertChainElement> elements = new ptr<ref array<ref syscall.CertChainElement>>(@unsafe.Pointer(lastChain.Elements))[..];
            for (long i = 0L; i < int(lastChain.NumElements); i++)
            { 
                // Copy the buf, since ParseCertificate does not create its own copy.
                var cert = elements[i].CertContext;
                ref array<byte> encodedCert = new ptr<ref array<byte>>(@unsafe.Pointer(cert.EncodedCert))[..];
                var buf = make_slice<byte>(cert.Length);
                copy(buf, encodedCert[..]);
                var (parsedCert, err) = ParseCertificate(buf);
                if (err != null)
                {
                    return (null, err);
                }
                chain = append(chain, parsedCert);
            }


            return (chain, null);
        }

        // checkChainTrustStatus checks the trust status of the certificate chain, translating
        // any errors it finds into Go errors in the process.
        private static error checkChainTrustStatus(ref Certificate c, ref syscall.CertChainContext chainCtx)
        {
            if (chainCtx.TrustStatus.ErrorStatus != syscall.CERT_TRUST_NO_ERROR)
            {
                var status = chainCtx.TrustStatus.ErrorStatus;

                if (status == syscall.CERT_TRUST_IS_NOT_TIME_VALID) 
                    return error.As(new CertificateInvalidError(c,Expired,""));
                else 
                    return error.As(new UnknownAuthorityError(c,nil,nil));
                            }
            return error.As(null);
        }

        // checkChainSSLServerPolicy checks that the certificate chain in chainCtx is valid for
        // use as a certificate chain for a SSL/TLS server.
        private static error checkChainSSLServerPolicy(ref Certificate c, ref syscall.CertChainContext chainCtx, ref VerifyOptions opts)
        {
            var (servernamep, err) = syscall.UTF16PtrFromString(opts.DNSName);
            if (err != null)
            {
                return error.As(err);
            }
            syscall.SSLExtraCertChainPolicyPara sslPara = ref new syscall.SSLExtraCertChainPolicyPara(AuthType:syscall.AUTHTYPE_SERVER,ServerName:servernamep,);
            sslPara.Size = uint32(@unsafe.Sizeof(sslPara.Value));

            syscall.CertChainPolicyPara para = ref new syscall.CertChainPolicyPara(ExtraPolicyPara:uintptr(unsafe.Pointer(sslPara)),);
            para.Size = uint32(@unsafe.Sizeof(para.Value));

            syscall.CertChainPolicyStatus status = new syscall.CertChainPolicyStatus();
            err = syscall.CertVerifyCertificateChainPolicy(syscall.CERT_CHAIN_POLICY_SSL, chainCtx, para, ref status);
            if (err != null)
            {
                return error.As(err);
            } 

            // TODO(mkrautz): use the lChainIndex and lElementIndex fields
            // of the CertChainPolicyStatus to provide proper context, instead
            // using c.
            if (status.Error != 0L)
            {

                if (status.Error == syscall.CERT_E_EXPIRED) 
                    return error.As(new CertificateInvalidError(c,Expired,""));
                else if (status.Error == syscall.CERT_E_CN_NO_MATCH) 
                    return error.As(new HostnameError(c,opts.DNSName));
                else if (status.Error == syscall.CERT_E_UNTRUSTEDROOT) 
                    return error.As(new UnknownAuthorityError(c,nil,nil));
                else 
                    return error.As(new UnknownAuthorityError(c,nil,nil));
                            }
            return error.As(null);
        }

        // systemVerify is like Verify, except that it uses CryptoAPI calls
        // to build certificate chains and verify them.
        private static (slice<slice<ref Certificate>>, error) systemVerify(this ref Certificate _c, ref VerifyOptions _opts) => func(_c, _opts, (ref Certificate c, ref VerifyOptions opts, Defer defer, Panic _, Recover __) =>
        {
            var hasDNSName = opts != null && len(opts.DNSName) > 0L;

            var (storeCtx, err) = createStoreContext(c, opts);
            if (err != null)
            {
                return (null, err);
            }
            defer(syscall.CertFreeCertificateContext(storeCtx));

            ptr<object> para = @new<syscall.CertChainPara>();
            para.Size = uint32(@unsafe.Sizeof(para.Value)); 

            // If there's a DNSName set in opts, assume we're verifying
            // a certificate from a TLS server.
            if (hasDNSName)
            {
                ref byte oids = new slice<ref byte>(new ref byte[] { &syscall.OID_PKIX_KP_SERVER_AUTH[0], &syscall.OID_SERVER_GATED_CRYPTO[0], &syscall.OID_SGC_NETSCAPE[0] });
                para.RequestedUsage.Type = syscall.USAGE_MATCH_TYPE_OR;
                para.RequestedUsage.Usage.Length = uint32(len(oids));
                para.RequestedUsage.Usage.UsageIdentifiers = ref oids[0L];
            }
            else
            {
                para.RequestedUsage.Type = syscall.USAGE_MATCH_TYPE_AND;
                para.RequestedUsage.Usage.Length = 0L;
                para.RequestedUsage.Usage.UsageIdentifiers = null;
            }
            ref syscall.Filetime verifyTime = default;
            if (opts != null && !opts.CurrentTime.IsZero())
            {
                var ft = syscall.NsecToFiletime(opts.CurrentTime.UnixNano());
                verifyTime = ref ft;
            } 

            // CertGetCertificateChain will traverse Windows's root stores
            // in an attempt to build a verified certificate chain. Once
            // it has found a verified chain, it stops. MSDN docs on
            // CERT_CHAIN_CONTEXT:
            //
            //   When a CERT_CHAIN_CONTEXT is built, the first simple chain
            //   begins with an end certificate and ends with a self-signed
            //   certificate. If that self-signed certificate is not a root
            //   or otherwise trusted certificate, an attempt is made to
            //   build a new chain. CTLs are used to create the new chain
            //   beginning with the self-signed certificate from the original
            //   chain as the end certificate of the new chain. This process
            //   continues building additional simple chains until the first
            //   self-signed certificate is a trusted certificate or until
            //   an additional simple chain cannot be built.
            //
            // The result is that we'll only get a single trusted chain to
            // return to our caller.
            ref syscall.CertChainContext chainCtx = default;
            err = syscall.CertGetCertificateChain(syscall.Handle(0L), storeCtx, verifyTime, storeCtx.Store, para, 0L, 0L, ref chainCtx);
            if (err != null)
            {
                return (null, err);
            }
            defer(syscall.CertFreeCertificateChain(chainCtx));

            err = checkChainTrustStatus(c, chainCtx);
            if (err != null)
            {
                return (null, err);
            }
            if (hasDNSName)
            {
                err = checkChainSSLServerPolicy(c, chainCtx, opts);
                if (err != null)
                {
                    return (null, err);
                }
            }
            var (chain, err) = extractSimpleChain(chainCtx.Chains, int(chainCtx.ChainCount));
            if (err != null)
            {
                return (null, err);
            }
            chains = append(chains, chain);

            return (chains, null);
        });

        private static (ref CertPool, error) loadSystemRoots() => func((defer, _, __) =>
        { 
            // TODO: restore this functionality on Windows. We tried to do
            // it in Go 1.8 but had to revert it. See Issue 18609.
            // Returning (nil, nil) was the old behavior, prior to CL 30578.
            return (null, null);

            const ulong CRYPT_E_NOT_FOUND = 0x80092004UL;



            var (store, err) = syscall.CertOpenSystemStore(0L, syscall.StringToUTF16Ptr("ROOT"));
            if (err != null)
            {
                return (null, err);
            }
            defer(syscall.CertCloseStore(store, 0L));

            var roots = NewCertPool();
            ref syscall.CertContext cert = default;
            while (true)
            {
                cert, err = syscall.CertEnumCertificatesInStore(store, cert);
                if (err != null)
                {
                    {
                        syscall.Errno (errno, ok) = err._<syscall.Errno>();

                        if (ok)
                        {
                            if (errno == CRYPT_E_NOT_FOUND)
                            {
                                break;
                            }
                        }

                    }
                    return (null, err);
                }
                if (cert == null)
                {
                    break;
                } 
                // Copy the buf, since ParseCertificate does not create its own copy.
                ref array<byte> buf = new ptr<ref array<byte>>(@unsafe.Pointer(cert.EncodedCert))[..];
                var buf2 = make_slice<byte>(cert.Length);
                copy(buf2, buf);
                {
                    var (c, err) = ParseCertificate(buf2);

                    if (err == null)
                    {
                        roots.AddCert(c);
                    }

                }
            }

            return (roots, null);
        });
    }
}}
