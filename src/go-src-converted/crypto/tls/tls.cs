// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tls partially implements TLS 1.2, as specified in RFC 5246,
// and TLS 1.3, as specified in RFC 8446.
// package tls -- go2cs converted at 2020 October 08 03:38:25 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\tls.go
// BUG(agl): The crypto/tls package only implements some countermeasures
// against Lucky13 attacks on CBC-mode encryption, and only on SHA1
// variants. See http://www.isg.rhul.ac.uk/tls/TLStiming.pdf and
// https://www.imperialviolet.org/2013/02/04/luckythirteen.html.

using bytes = go.bytes_package;
using context = go.context_package;
using crypto = go.crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using ed25519 = go.crypto.ed25519_package;
using rsa = go.crypto.rsa_package;
using x509 = go.crypto.x509_package;
using pem = go.encoding.pem_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using net = go.net_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace crypto
{
    public static partial class tls_package
    {
        // Server returns a new TLS server side connection
        // using conn as the underlying transport.
        // The configuration config must be non-nil and must include
        // at least one certificate or else set GetCertificate.
        public static ptr<Conn> Server(net.Conn conn, ptr<Config> _addr_config)
        {
            ref Config config = ref _addr_config.val;

            ptr<Conn> c = addr(new Conn(conn:conn,config:config,));
            c.handshakeFn = c.serverHandshake;
            return _addr_c!;
        }

        // Client returns a new TLS client side connection
        // using conn as the underlying transport.
        // The config cannot be nil: users must set either ServerName or
        // InsecureSkipVerify in the config.
        public static ptr<Conn> Client(net.Conn conn, ptr<Config> _addr_config)
        {
            ref Config config = ref _addr_config.val;

            ptr<Conn> c = addr(new Conn(conn:conn,config:config,isClient:true,));
            c.handshakeFn = c.clientHandshake;
            return _addr_c!;
        }

        // A listener implements a network listener (net.Listener) for TLS connections.
        private partial struct listener : net.Listener
        {
            public ref net.Listener Listener => ref Listener_val;
            public ptr<Config> config;
        }

        // Accept waits for and returns the next incoming TLS connection.
        // The returned connection is of type *Conn.
        private static (net.Conn, error) Accept(this ptr<listener> _addr_l)
        {
            net.Conn _p0 = default;
            error _p0 = default!;
            ref listener l = ref _addr_l.val;

            var (c, err) = l.Listener.Accept();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (Server(c, _addr_l.config), error.As(null!)!);

        }

        // NewListener creates a Listener which accepts connections from an inner
        // Listener and wraps each connection with Server.
        // The configuration config must be non-nil and must include
        // at least one certificate or else set GetCertificate.
        public static net.Listener NewListener(net.Listener inner, ptr<Config> _addr_config)
        {
            ref Config config = ref _addr_config.val;

            ptr<listener> l = @new<listener>();
            l.Listener = inner;
            l.config = config;
            return l;
        }

        // Listen creates a TLS listener accepting connections on the
        // given network address using net.Listen.
        // The configuration config must be non-nil and must include
        // at least one certificate or else set GetCertificate.
        public static (net.Listener, error) Listen(@string network, @string laddr, ptr<Config> _addr_config)
        {
            net.Listener _p0 = default;
            error _p0 = default!;
            ref Config config = ref _addr_config.val;

            if (config == null || len(config.Certificates) == 0L && config.GetCertificate == null && config.GetConfigForClient == null)
            {
                return (null, error.As(errors.New("tls: neither Certificates, GetCertificate, nor GetConfigForClient set in Config"))!);
            }

            var (l, err) = net.Listen(network, laddr);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (NewListener(l, _addr_config), error.As(null!)!);

        }

        private partial struct timeoutError
        {
        }

        private static @string Error(this timeoutError _p0)
        {
            return "tls: DialWithDialer timed out";
        }
        private static bool Timeout(this timeoutError _p0)
        {
            return true;
        }
        private static bool Temporary(this timeoutError _p0)
        {
            return true;
        }

        // DialWithDialer connects to the given network address using dialer.Dial and
        // then initiates a TLS handshake, returning the resulting TLS connection. Any
        // timeout or deadline given in the dialer apply to connection and TLS
        // handshake as a whole.
        //
        // DialWithDialer interprets a nil configuration as equivalent to the zero
        // configuration; see the documentation of Config for the defaults.
        public static (ptr<Conn>, error) DialWithDialer(ptr<net.Dialer> _addr_dialer, @string network, @string addr, ptr<Config> _addr_config)
        {
            ptr<Conn> _p0 = default!;
            error _p0 = default!;
            ref net.Dialer dialer = ref _addr_dialer.val;
            ref Config config = ref _addr_config.val;

            return _addr_dial(context.Background(), _addr_dialer, network, addr, _addr_config)!;
        }

        private static (ptr<Conn>, error) dial(context.Context ctx, ptr<net.Dialer> _addr_netDialer, @string network, @string addr, ptr<Config> _addr_config) => func((defer, _, __) =>
        {
            ptr<Conn> _p0 = default!;
            error _p0 = default!;
            ref net.Dialer netDialer = ref _addr_netDialer.val;
            ref Config config = ref _addr_config.val;
 
            // We want the Timeout and Deadline values from dialer to cover the
            // whole process: TCP connection and TLS handshake. This means that we
            // also need to start our own timers now.
            var timeout = netDialer.Timeout;

            if (!netDialer.Deadline.IsZero())
            {
                var deadlineTimeout = time.Until(netDialer.Deadline);
                if (timeout == 0L || deadlineTimeout < timeout)
                {
                    timeout = deadlineTimeout;
                }

            } 

            // hsErrCh is non-nil if we might not wait for Handshake to complete.
            channel<error> hsErrCh = default;
            if (timeout != 0L || ctx.Done() != null)
            {
                hsErrCh = make_channel<error>(2L);
            }

            if (timeout != 0L)
            {
                var timer = time.AfterFunc(timeout, () =>
                {
                    hsErrCh.Send(new timeoutError());
                });
                defer(timer.Stop());

            }

            var (rawConn, err) = netDialer.DialContext(ctx, network, addr);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var colonPos = strings.LastIndex(addr, ":");
            if (colonPos == -1L)
            {
                colonPos = len(addr);
            }

            var hostname = addr[..colonPos];

            if (config == null)
            {
                config = defaultConfig();
            } 
            // If no ServerName is set, infer the ServerName
            // from the hostname we're connecting to.
            if (config.ServerName == "")
            { 
                // Make a copy to avoid polluting argument or default.
                var c = config.Clone();
                c.ServerName = hostname;
                config = c;

            }

            var conn = Client(rawConn, _addr_config);

            if (hsErrCh == null)
            {
                err = conn.Handshake();
            }
            else
            {
                go_(() => () =>
                {
                    hsErrCh.Send(conn.Handshake());
                }());

                err = ctx.Err();
                if (err != null)
                { 
                    // If the error was due to the context
                    // closing, prefer the context's error, rather
                    // than some random network teardown error.
                    {
                        var e = ctx.Err();

                        if (e != null)
                        {
                            err = e;
                        }

                    }

                }

            }

            if (err != null)
            {
                rawConn.Close();
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_conn!, error.As(null!)!);

        });

        // Dial connects to the given network address using net.Dial
        // and then initiates a TLS handshake, returning the resulting
        // TLS connection.
        // Dial interprets a nil configuration as equivalent to
        // the zero configuration; see the documentation of Config
        // for the defaults.
        public static (ptr<Conn>, error) Dial(@string network, @string addr, ptr<Config> _addr_config)
        {
            ptr<Conn> _p0 = default!;
            error _p0 = default!;
            ref Config config = ref _addr_config.val;

            return _addr_DialWithDialer(@new<net.Dialer>(), network, addr, _addr_config)!;
        }

        // Dialer dials TLS connections given a configuration and a Dialer for the
        // underlying connection.
        public partial struct Dialer
        {
            public ptr<net.Dialer> NetDialer; // Config is the TLS configuration to use for new connections.
// A nil configuration is equivalent to the zero
// configuration; see the documentation of Config for the
// defaults.
            public ptr<Config> Config;
        }

        // Dial connects to the given network address and initiates a TLS
        // handshake, returning the resulting TLS connection.
        //
        // The returned Conn, if any, will always be of type *Conn.
        private static (net.Conn, error) Dial(this ptr<Dialer> _addr_d, @string network, @string addr)
        {
            net.Conn _p0 = default;
            error _p0 = default!;
            ref Dialer d = ref _addr_d.val;

            return d.DialContext(context.Background(), network, addr);
        }

        private static ptr<net.Dialer> netDialer(this ptr<Dialer> _addr_d)
        {
            ref Dialer d = ref _addr_d.val;

            if (d.NetDialer != null)
            {
                return _addr_d.NetDialer!;
            }

            return @new<net.Dialer>();

        }

        // DialContext connects to the given network address and initiates a TLS
        // handshake, returning the resulting TLS connection.
        //
        // The provided Context must be non-nil. If the context expires before
        // the connection is complete, an error is returned. Once successfully
        // connected, any expiration of the context will not affect the
        // connection.
        //
        // The returned Conn, if any, will always be of type *Conn.
        private static (net.Conn, error) DialContext(this ptr<Dialer> _addr_d, context.Context ctx, @string network, @string addr)
        {
            net.Conn _p0 = default;
            error _p0 = default!;
            ref Dialer d = ref _addr_d.val;

            var (c, err) = dial(ctx, _addr_d.netDialer(), network, addr, _addr_d.Config);
            if (err != null)
            { 
                // Don't return c (a typed nil) in an interface.
                return (null, error.As(err)!);

            }

            return (c, error.As(null!)!);

        }

        // LoadX509KeyPair reads and parses a public/private key pair from a pair
        // of files. The files must contain PEM encoded data. The certificate file
        // may contain intermediate certificates following the leaf certificate to
        // form a certificate chain. On successful return, Certificate.Leaf will
        // be nil because the parsed form of the certificate is not retained.
        public static (Certificate, error) LoadX509KeyPair(@string certFile, @string keyFile)
        {
            Certificate _p0 = default;
            error _p0 = default!;

            var (certPEMBlock, err) = ioutil.ReadFile(certFile);
            if (err != null)
            {
                return (new Certificate(), error.As(err)!);
            }

            var (keyPEMBlock, err) = ioutil.ReadFile(keyFile);
            if (err != null)
            {
                return (new Certificate(), error.As(err)!);
            }

            return X509KeyPair(certPEMBlock, keyPEMBlock);

        }

        // X509KeyPair parses a public/private key pair from a pair of
        // PEM encoded data. On successful return, Certificate.Leaf will be nil because
        // the parsed form of the certificate is not retained.
        public static (Certificate, error) X509KeyPair(slice<byte> certPEMBlock, slice<byte> keyPEMBlock)
        {
            Certificate _p0 = default;
            error _p0 = default!;

            Func<error, (Certificate, error)> fail = err => (new Certificate(), error.As(err)!);

            Certificate cert = default;
            slice<@string> skippedBlockTypes = default;
            while (true)
            {
                ptr<pem.Block> certDERBlock;
                certDERBlock, certPEMBlock = pem.Decode(certPEMBlock);
                if (certDERBlock == null)
                {
                    break;
                }

                if (certDERBlock.Type == "CERTIFICATE")
                {
                    cert.Certificate = append(cert.Certificate, certDERBlock.Bytes);
                }
                else
                {
                    skippedBlockTypes = append(skippedBlockTypes, certDERBlock.Type);
                }

            }


            if (len(cert.Certificate) == 0L)
            {
                if (len(skippedBlockTypes) == 0L)
                {
                    return fail(errors.New("tls: failed to find any PEM data in certificate input"));
                }

                if (len(skippedBlockTypes) == 1L && strings.HasSuffix(skippedBlockTypes[0L], "PRIVATE KEY"))
                {
                    return fail(errors.New("tls: failed to find certificate PEM data in certificate input, but did find a private key; PEM inputs may have been switched"));
                }

                return fail(fmt.Errorf("tls: failed to find \"CERTIFICATE\" PEM block in certificate input after skipping PEM blocks of the following types: %v", skippedBlockTypes));

            }

            skippedBlockTypes = skippedBlockTypes[..0L];
            ptr<pem.Block> keyDERBlock;
            while (true)
            {
                keyDERBlock, keyPEMBlock = pem.Decode(keyPEMBlock);
                if (keyDERBlock == null)
                {
                    if (len(skippedBlockTypes) == 0L)
                    {
                        return fail(errors.New("tls: failed to find any PEM data in key input"));
                    }

                    if (len(skippedBlockTypes) == 1L && skippedBlockTypes[0L] == "CERTIFICATE")
                    {
                        return fail(errors.New("tls: found a certificate rather than a key in the PEM for the private key"));
                    }

                    return fail(fmt.Errorf("tls: failed to find PEM block with type ending in \"PRIVATE KEY\" in key input after skipping PEM blocks of the following types: %v", skippedBlockTypes));

                }

                if (keyDERBlock.Type == "PRIVATE KEY" || strings.HasSuffix(keyDERBlock.Type, " PRIVATE KEY"))
                {
                    break;
                }

                skippedBlockTypes = append(skippedBlockTypes, keyDERBlock.Type);

            } 

            // We don't need to parse the public key for TLS, but we so do anyway
            // to check that it looks sane and matches the private key.
 

            // We don't need to parse the public key for TLS, but we so do anyway
            // to check that it looks sane and matches the private key.
            var (x509Cert, err) = x509.ParseCertificate(cert.Certificate[0L]);
            if (err != null)
            {
                return fail(err);
            }

            cert.PrivateKey, err = parsePrivateKey(keyDERBlock.Bytes);
            if (err != null)
            {
                return fail(err);
            }

            switch (x509Cert.PublicKey.type())
            {
                case ptr<rsa.PublicKey> pub:
                    ptr<rsa.PrivateKey> (priv, ok) = cert.PrivateKey._<ptr<rsa.PrivateKey>>();
                    if (!ok)
                    {
                        return fail(errors.New("tls: private key type does not match public key type"));
                    }

                    if (pub.N.Cmp(priv.N) != 0L)
                    {
                        return fail(errors.New("tls: private key does not match public key"));
                    }

                    break;
                case ptr<ecdsa.PublicKey> pub:
                    (priv, ok) = cert.PrivateKey._<ptr<ecdsa.PrivateKey>>();
                    if (!ok)
                    {
                        return fail(errors.New("tls: private key type does not match public key type"));
                    }

                    if (pub.X.Cmp(priv.X) != 0L || pub.Y.Cmp(priv.Y) != 0L)
                    {
                        return fail(errors.New("tls: private key does not match public key"));
                    }

                    break;
                case ed25519.PublicKey pub:
                    (priv, ok) = cert.PrivateKey._<ed25519.PrivateKey>();
                    if (!ok)
                    {
                        return fail(errors.New("tls: private key type does not match public key type"));
                    }

                    if (!bytes.Equal(priv.Public()._<ed25519.PublicKey>(), pub))
                    {
                        return fail(errors.New("tls: private key does not match public key"));
                    }

                    break;
                default:
                {
                    var pub = x509Cert.PublicKey.type();
                    return fail(errors.New("tls: unknown public key algorithm"));
                    break;
                }

            }

            return (cert, error.As(null!)!);

        }

        // Attempt to parse the given private key DER block. OpenSSL 0.9.8 generates
        // PKCS #1 private keys by default, while OpenSSL 1.0.0 generates PKCS #8 keys.
        // OpenSSL ecparam generates SEC1 EC private keys for ECDSA. We try all three.
        private static (crypto.PrivateKey, error) parsePrivateKey(slice<byte> der)
        {
            crypto.PrivateKey _p0 = default;
            error _p0 = default!;

            {
                var key__prev1 = key;

                var (key, err) = x509.ParsePKCS1PrivateKey(der);

                if (err == null)
                {
                    return (key, error.As(null!)!);
                }

                key = key__prev1;

            }

            {
                var key__prev1 = key;

                (key, err) = x509.ParsePKCS8PrivateKey(der);

                if (err == null)
                {
                    switch (key.type())
                    {
                        case ptr<rsa.PrivateKey> key:
                            return (key, error.As(null!)!);
                            break;
                        case ptr<ecdsa.PrivateKey> key:
                            return (key, error.As(null!)!);
                            break;
                        case ed25519.PrivateKey key:
                            return (key, error.As(null!)!);
                            break;
                        default:
                        {
                            var key = key.type();
                            return (null, error.As(errors.New("tls: found unknown private key type in PKCS#8 wrapping"))!);
                            break;
                        }
                    }

                }

                key = key__prev1;

            }

            {
                var key__prev1 = key;

                (key, err) = x509.ParseECPrivateKey(der);

                if (err == null)
                {
                    return (key, error.As(null!)!);
                }

                key = key__prev1;

            }


            return (null, error.As(errors.New("tls: failed to parse private key"))!);

        }
    }
}}
