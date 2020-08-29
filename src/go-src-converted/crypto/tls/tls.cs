// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tls partially implements TLS 1.2, as specified in RFC 5246.
// package tls -- go2cs converted at 2020 August 29 08:31:39 UTC
// import "crypto/tls" ==> using tls = go.crypto.tls_package
// Original source: C:\Go\src\crypto\tls\tls.go
// BUG(agl): The crypto/tls package only implements some countermeasures
// against Lucky13 attacks on CBC-mode encryption, and only on SHA1
// variants. See http://www.isg.rhul.ac.uk/tls/TLStiming.pdf and
// https://www.imperialviolet.org/2013/02/04/luckythirteen.html.

using crypto = go.crypto_package;
using ecdsa = go.crypto.ecdsa_package;
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
        public static ref Conn Server(net.Conn conn, ref Config config)
        {
            return ref new Conn(conn:conn,config:config);
        }

        // Client returns a new TLS client side connection
        // using conn as the underlying transport.
        // The config cannot be nil: users must set either ServerName or
        // InsecureSkipVerify in the config.
        public static ref Conn Client(net.Conn conn, ref Config config)
        {
            return ref new Conn(conn:conn,config:config,isClient:true);
        }

        // A listener implements a network listener (net.Listener) for TLS connections.
        private partial struct listener : net.Listener
        {
            public ref net.Listener Listener => ref Listener_val;
            public ptr<Config> config;
        }

        // Accept waits for and returns the next incoming TLS connection.
        // The returned connection is of type *Conn.
        private static (net.Conn, error) Accept(this ref listener l)
        {
            var (c, err) = l.Listener.Accept();
            if (err != null)
            {
                return (null, err);
            }
            return (Server(c, l.config), null);
        }

        // NewListener creates a Listener which accepts connections from an inner
        // Listener and wraps each connection with Server.
        // The configuration config must be non-nil and must include
        // at least one certificate or else set GetCertificate.
        public static net.Listener NewListener(net.Listener inner, ref Config config)
        {
            ptr<listener> l = @new<listener>();
            l.Listener = inner;
            l.config = config;
            return l;
        }

        // Listen creates a TLS listener accepting connections on the
        // given network address using net.Listen.
        // The configuration config must be non-nil and must include
        // at least one certificate or else set GetCertificate.
        public static (net.Listener, error) Listen(@string network, @string laddr, ref Config config)
        {
            if (config == null || (len(config.Certificates) == 0L && config.GetCertificate == null))
            {
                return (null, errors.New("tls: neither Certificates nor GetCertificate set in Config"));
            }
            var (l, err) = net.Listen(network, laddr);
            if (err != null)
            {
                return (null, err);
            }
            return (NewListener(l, config), null);
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
        public static (ref Conn, error) DialWithDialer(ref net.Dialer dialer, @string network, @string addr, ref Config config)
        { 
            // We want the Timeout and Deadline values from dialer to cover the
            // whole process: TCP connection and TLS handshake. This means that we
            // also need to start our own timers now.
            var timeout = dialer.Timeout;

            if (!dialer.Deadline.IsZero())
            {
                var deadlineTimeout = time.Until(dialer.Deadline);
                if (timeout == 0L || deadlineTimeout < timeout)
                {
                    timeout = deadlineTimeout;
                }
            }
            channel<error> errChannel = default;

            if (timeout != 0L)
            {
                errChannel = make_channel<error>(2L);
                time.AfterFunc(timeout, () =>
                {
                    errChannel.Send(new timeoutError());
                });
            }
            var (rawConn, err) = dialer.Dial(network, addr);
            if (err != null)
            {
                return (null, err);
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
            var conn = Client(rawConn, config);

            if (timeout == 0L)
            {
                err = conn.Handshake();
            }
            else
            {
                go_(() => () =>
                {
                    errChannel.Send(conn.Handshake());
                }());

                err = errChannel.Receive();
            }
            if (err != null)
            {
                rawConn.Close();
                return (null, err);
            }
            return (conn, null);
        }

        // Dial connects to the given network address using net.Dial
        // and then initiates a TLS handshake, returning the resulting
        // TLS connection.
        // Dial interprets a nil configuration as equivalent to
        // the zero configuration; see the documentation of Config
        // for the defaults.
        public static (ref Conn, error) Dial(@string network, @string addr, ref Config config)
        {
            return DialWithDialer(@new<net.Dialer>(), network, addr, config);
        }

        // LoadX509KeyPair reads and parses a public/private key pair from a pair
        // of files. The files must contain PEM encoded data. The certificate file
        // may contain intermediate certificates following the leaf certificate to
        // form a certificate chain. On successful return, Certificate.Leaf will
        // be nil because the parsed form of the certificate is not retained.
        public static (Certificate, error) LoadX509KeyPair(@string certFile, @string keyFile)
        {
            var (certPEMBlock, err) = ioutil.ReadFile(certFile);
            if (err != null)
            {
                return (new Certificate(), err);
            }
            var (keyPEMBlock, err) = ioutil.ReadFile(keyFile);
            if (err != null)
            {
                return (new Certificate(), err);
            }
            return X509KeyPair(certPEMBlock, keyPEMBlock);
        }

        // X509KeyPair parses a public/private key pair from a pair of
        // PEM encoded data. On successful return, Certificate.Leaf will be nil because
        // the parsed form of the certificate is not retained.
        public static (Certificate, error) X509KeyPair(slice<byte> certPEMBlock, slice<byte> keyPEMBlock)
        {
            Func<error, (Certificate, error)> fail = err => (new Certificate(), err);

            Certificate cert = default;
            slice<@string> skippedBlockTypes = default;
            while (true)
            {
                ref pem.Block certDERBlock = default;
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
            ref pem.Block keyDERBlock = default;
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


            error err = default;
            cert.PrivateKey, err = parsePrivateKey(keyDERBlock.Bytes);
            if (err != null)
            {
                return fail(err);
            } 

            // We don't need to parse the public key for TLS, but we so do anyway
            // to check that it looks sane and matches the private key.
            var (x509Cert, err) = x509.ParseCertificate(cert.Certificate[0L]);
            if (err != null)
            {
                return fail(err);
            }
            switch (x509Cert.PublicKey.type())
            {
                case ref rsa.PublicKey pub:
                    ref rsa.PrivateKey (priv, ok) = cert.PrivateKey._<ref rsa.PrivateKey>();
                    if (!ok)
                    {
                        return fail(errors.New("tls: private key type does not match public key type"));
                    }
                    if (pub.N.Cmp(priv.N) != 0L)
                    {
                        return fail(errors.New("tls: private key does not match public key"));
                    }
                    break;
                case ref ecdsa.PublicKey pub:
                    (priv, ok) = cert.PrivateKey._<ref ecdsa.PrivateKey>();
                    if (!ok)
                    {
                        return fail(errors.New("tls: private key type does not match public key type"));
                    }
                    if (pub.X.Cmp(priv.X) != 0L || pub.Y.Cmp(priv.Y) != 0L)
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

            return (cert, null);
        }

        // Attempt to parse the given private key DER block. OpenSSL 0.9.8 generates
        // PKCS#1 private keys by default, while OpenSSL 1.0.0 generates PKCS#8 keys.
        // OpenSSL ecparam generates SEC1 EC private keys for ECDSA. We try all three.
        private static (crypto.PrivateKey, error) parsePrivateKey(slice<byte> der)
        {
            {
                var key__prev1 = key;

                var (key, err) = x509.ParsePKCS1PrivateKey(der);

                if (err == null)
                {
                    return (key, null);
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
                        case ref rsa.PrivateKey key:
                            return (key, null);
                            break;
                        case ref ecdsa.PrivateKey key:
                            return (key, null);
                            break;
                        default:
                        {
                            var key = key.type();
                            return (null, errors.New("tls: found unknown private key type in PKCS#8 wrapping"));
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
                    return (key, null);
                }

                key = key__prev1;

            }

            return (null, errors.New("tls: failed to parse private key"));
        }
    }
}}
