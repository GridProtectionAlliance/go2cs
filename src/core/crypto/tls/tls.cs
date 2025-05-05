// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tls partially implements TLS 1.2, as specified in RFC 5246,
// and TLS 1.3, as specified in RFC 8446.
namespace go.crypto;

// BUG(agl): The crypto/tls package only implements some countermeasures
// against Lucky13 attacks on CBC-mode encryption, and only on SHA1
// variants. See http://www.isg.rhul.ac.uk/tls/TLStiming.pdf and
// https://www.imperialviolet.org/2013/02/04/luckythirteen.html.
using bytes = bytes_package;
using context = context_package;
using crypto = crypto_package;
using ecdsa = crypto.ecdsa_package;
using ed25519 = crypto.ed25519_package;
using rsa = crypto.rsa_package;
using x509 = crypto.x509_package;
using pem = encoding.pem_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = @internal.godebug_package;
using net = net_package;
using os = os_package;
using strings = strings_package;
using @internal;
using encoding;

partial class tls_package {

// Server returns a new TLS server side connection
// using conn as the underlying transport.
// The configuration config must be non-nil and must include
// at least one certificate or else set GetCertificate.
public static ж<Conn> Server(net.Conn conn, ж<Config> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    var c = Ꮡ(new Conn(
        conn: conn,
        config: config
    ));
    c.val.handshakeFn = 
    var cʗ1 = c;
    () => cʗ1.serverHandshake();
    return c;
}

// Client returns a new TLS client side connection
// using conn as the underlying transport.
// The config cannot be nil: users must set either ServerName or
// InsecureSkipVerify in the config.
public static ж<Conn> Client(net.Conn conn, ж<Config> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    var c = Ꮡ(new Conn(
        conn: conn,
        config: config,
        isClient: true
    ));
    c.val.handshakeFn = 
    var cʗ1 = c;
    () => cʗ1.clientHandshake();
    return c;
}

// A listener implements a network listener (net.Listener) for TLS connections.
[GoType] partial struct listener {
    public partial ref net_package.Listener Listener { get; }
    internal ж<Config> config;
}

// Accept waits for and returns the next incoming TLS connection.
// The returned connection is of type *Conn.
[GoRecv] internal static (net.Conn, error) Accept(this ref listener l) {
    (c, err) = l.Listener.Accept();
    if (err != default!) {
        return (default!, err);
    }
    return (~Server(c, l.config), default!);
}

// NewListener creates a Listener which accepts connections from an inner
// Listener and wraps each connection with [Server].
// The configuration config must be non-nil and must include
// at least one certificate or else set GetCertificate.
public static net.Listener NewListener(net.Listener inner, ж<Config> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    var l = @new<listener>();
    l.val.Listener = inner;
    l.val.config = config;
    return ~l;
}

// Listen creates a TLS listener accepting connections on the
// given network address using net.Listen.
// The configuration config must be non-nil and must include
// at least one certificate or else set GetCertificate.
public static (net.Listener, error) Listen(@string network, @string laddr, ж<Config> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    // If this condition changes, consider updating http.Server.ServeTLS too.
    if (config == nil || len(config.Certificates) == 0 && config.GetCertificate == default! && config.GetConfigForClient == default!) {
        return (default!, errors.New("tls: neither Certificates, GetCertificate, nor GetConfigForClient set in Config"u8));
    }
    (l, err) = net.Listen(network, laddr);
    if (err != default!) {
        return (default!, err);
    }
    return (NewListener(l, Ꮡconfig), default!);
}

[GoType] partial struct timeoutError {
}

internal static @string Error(this timeoutError _) {
    return "tls: DialWithDialer timed out"u8;
}

internal static bool Timeout(this timeoutError _) {
    return true;
}

internal static bool Temporary(this timeoutError _) {
    return true;
}

// DialWithDialer connects to the given network address using dialer.Dial and
// then initiates a TLS handshake, returning the resulting TLS connection. Any
// timeout or deadline given in the dialer apply to connection and TLS
// handshake as a whole.
//
// DialWithDialer interprets a nil configuration as equivalent to the zero
// configuration; see the documentation of [Config] for the defaults.
//
// DialWithDialer uses context.Background internally; to specify the context,
// use [Dialer.DialContext] with NetDialer set to the desired dialer.
public static (ж<Conn>, error) DialWithDialer(ж<net.Dialer> Ꮡdialer, @string network, @string addr, ж<Config> Ꮡconfig) {
    ref var dialer = ref Ꮡdialer.val;
    ref var config = ref Ꮡconfig.val;

    return dial(context.Background(), Ꮡdialer, network, addr, Ꮡconfig);
}

internal static (ж<Conn>, error) dial(context.Context ctx, ж<net.Dialer> ᏑnetDialer, @string network, @string addr, ж<Config> Ꮡconfig) => func((defer, _) => {
    ref var netDialer = ref ᏑnetDialer.val;
    ref var config = ref Ꮡconfig.val;

    if (netDialer.Timeout != 0) {
        context.CancelFunc cancelΔ1 = default!;
        (ctx, ) = context.WithTimeout(ctx, netDialer.Timeout);
        var cancelʗ1 = cancelΔ1;
        defer(cancelʗ1);
    }
    if (!netDialer.Deadline.IsZero()) {
        context.CancelFunc cancel = default!;
        (ctx, cancel) = context.WithDeadline(ctx, netDialer.Deadline);
        var cancelʗ2 = cancel;
        defer(cancelʗ2);
    }
    (rawConn, err) = netDialer.DialContext(ctx, network, addr);
    if (err != default!) {
        return (default!, err);
    }
    nint colonPos = strings.LastIndex(addr, ":"u8);
    if (colonPos == -1) {
        colonPos = len(addr);
    }
    @string hostname = addr[..(int)(colonPos)];
    if (config == nil) {
        config = defaultConfig();
    }
    // If no ServerName is set, infer the ServerName
    // from the hostname we're connecting to.
    if (config.ServerName == ""u8) {
        // Make a copy to avoid polluting argument or default.
        var c = config.Clone();
        c.val.ServerName = hostname;
        config = c;
    }
    var conn = Client(rawConn, Ꮡconfig);
    {
        var errΔ1 = conn.HandshakeContext(ctx); if (errΔ1 != default!) {
            rawConn.Close();
            return (default!, errΔ1);
        }
    }
    return (conn, default!);
});

// Dial connects to the given network address using net.Dial
// and then initiates a TLS handshake, returning the resulting
// TLS connection.
// Dial interprets a nil configuration as equivalent to
// the zero configuration; see the documentation of Config
// for the defaults.
public static (ж<Conn>, error) Dial(@string network, @string addr, ж<Config> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    return DialWithDialer(@new<net.Dialer>(), network, addr, Ꮡconfig);
}

// Dialer dials TLS connections given a configuration and a Dialer for the
// underlying connection.
[GoType] partial struct Dialer {
    // NetDialer is the optional dialer to use for the TLS connections'
    // underlying TCP connections.
    // A nil NetDialer is equivalent to the net.Dialer zero value.
    public ж<net_package.Dialer> NetDialer;
    // Config is the TLS configuration to use for new connections.
    // A nil configuration is equivalent to the zero
    // configuration; see the documentation of Config for the
    // defaults.
    public ж<Config> Config;
}

// Dial connects to the given network address and initiates a TLS
// handshake, returning the resulting TLS connection.
//
// The returned [Conn], if any, will always be of type *[Conn].
//
// Dial uses context.Background internally; to specify the context,
// use [Dialer.DialContext].
[GoRecv] public static (net.Conn, error) Dial(this ref Dialer d, @string network, @string addr) {
    return d.DialContext(context.Background(), network, addr);
}

[GoRecv] internal static ж<net.Dialer> netDialer(this ref Dialer d) {
    if (d.NetDialer != nil) {
        return d.NetDialer;
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
// The returned [Conn], if any, will always be of type *[Conn].
[GoRecv] public static (net.Conn, error) DialContext(this ref Dialer d, context.Context ctx, @string network, @string addr) {
    (c, err) = dial(ctx, d.netDialer(), network, addr, d.Config);
    if (err != default!) {
        // Don't return c (a typed nil) in an interface.
        return (default!, err);
    }
    return (~c, default!);
}

// LoadX509KeyPair reads and parses a public/private key pair from a pair of
// files. The files must contain PEM encoded data. The certificate file may
// contain intermediate certificates following the leaf certificate to form a
// certificate chain. On successful return, Certificate.Leaf will be populated.
//
// Before Go 1.23 Certificate.Leaf was left nil, and the parsed certificate was
// discarded. This behavior can be re-enabled by setting "x509keypairleaf=0"
// in the GODEBUG environment variable.
public static (Certificate, error) LoadX509KeyPair(@string certFile, @string keyFile) {
    (certPEMBlock, err) = os.ReadFile(certFile);
    if (err != default!) {
        return (new Certificate(nil), err);
    }
    (keyPEMBlock, err) = os.ReadFile(keyFile);
    if (err != default!) {
        return (new Certificate(nil), err);
    }
    return X509KeyPair(certPEMBlock, keyPEMBlock);
}

internal static ж<godebug.Setting> x509keypairleaf = godebug.New("x509keypairleaf"u8);

// X509KeyPair parses a public/private key pair from a pair of
// PEM encoded data. On successful return, Certificate.Leaf will be populated.
//
// Before Go 1.23 Certificate.Leaf was left nil, and the parsed certificate was
// discarded. This behavior can be re-enabled by setting "x509keypairleaf=0"
// in the GODEBUG environment variable.
public static (Certificate, error) X509KeyPair(slice<byte> certPEMBlock, slice<byte> keyPEMBlock) {
    var fail = (error err) => (new Certificate(nil), errΔ1);
    Certificate cert = default!;
    slice<@string> skippedBlockTypes = default!;
    while (ᐧ) {
        ж<pem.Block> certDERBlock = default!;
        (certDERBlock, certPEMBlock) = pem.Decode(certPEMBlock);
        if (certDERBlock == nil) {
            break;
        }
        if ((~certDERBlock).Type == "CERTIFICATE"u8){
            cert.Certificate = append(cert.Certificate, (~certDERBlock).Bytes);
        } else {
            skippedBlockTypes = append(skippedBlockTypes, (~certDERBlock).Type);
        }
    }
    if (len(cert.Certificate) == 0) {
        if (len(skippedBlockTypes) == 0) {
            return fail(errors.New("tls: failed to find any PEM data in certificate input"u8));
        }
        if (len(skippedBlockTypes) == 1 && strings.HasSuffix(skippedBlockTypes[0], "PRIVATE KEY"u8)) {
            return fail(errors.New("tls: failed to find certificate PEM data in certificate input, but did find a private key; PEM inputs may have been switched"u8));
        }
        return fail(fmt.Errorf("tls: failed to find \"CERTIFICATE\" PEM block in certificate input after skipping PEM blocks of the following types: %v"u8, skippedBlockTypes));
    }
    skippedBlockTypes = skippedBlockTypes[..0];
    ж<pem.Block> keyDERBlock = default!;
    while (ᐧ) {
        (keyDERBlock, keyPEMBlock) = pem.Decode(keyPEMBlock);
        if (keyDERBlock == nil) {
            if (len(skippedBlockTypes) == 0) {
                return fail(errors.New("tls: failed to find any PEM data in key input"u8));
            }
            if (len(skippedBlockTypes) == 1 && skippedBlockTypes[0] == "CERTIFICATE") {
                return fail(errors.New("tls: found a certificate rather than a key in the PEM for the private key"u8));
            }
            return fail(fmt.Errorf("tls: failed to find PEM block with type ending in \"PRIVATE KEY\" in key input after skipping PEM blocks of the following types: %v"u8, skippedBlockTypes));
        }
        if ((~keyDERBlock).Type == "PRIVATE KEY"u8 || strings.HasSuffix((~keyDERBlock).Type, " PRIVATE KEY"u8)) {
            break;
        }
        skippedBlockTypes = append(skippedBlockTypes, (~keyDERBlock).Type);
    }
    // We don't need to parse the public key for TLS, but we so do anyway
    // to check that it looks sane and matches the private key.
    (x509Cert, err) = x509.ParseCertificate(cert.Certificate[0]);
    if (err != default!) {
        return fail(err);
    }
    if (x509keypairleaf.Value() != "0"u8){
        cert.Leaf = x509Cert;
    } else {
        x509keypairleaf.IncNonDefault();
    }
    (cert.PrivateKey, err) = parsePrivateKey((~keyDERBlock).Bytes);
    if (err != default!) {
        return fail(err);
    }
    switch ((~x509Cert).PublicKey.type()) {
    case ж<rsa.PublicKey> pub: {
        var (priv, ok) = cert.PrivateKey._<ж<rsa.PrivateKey>>(ᐧ);
        if (!ok) {
            return fail(errors.New("tls: private key type does not match public key type"u8));
        }
        if ((~pub).N.Cmp(priv.N) != 0) {
            return fail(errors.New("tls: private key does not match public key"u8));
        }
        break;
    }
    case ж<ecdsa.PublicKey> pub: {
        (priv, ok) = cert.PrivateKey._<ж<ecdsa.PrivateKey>>(ᐧ);
        if (!ok) {
            return fail(errors.New("tls: private key type does not match public key type"u8));
        }
        if ((~pub).X.Cmp(priv.X) != 0 || (~pub).Y.Cmp(priv.Y) != 0) {
            return fail(errors.New("tls: private key does not match public key"u8));
        }
        break;
    }
    case ed25519.PublicKey pub: {
        (priv, ok) = cert.PrivateKey._<ed25519.PrivateKey>(ᐧ);
        if (!ok) {
            return fail(errors.New("tls: private key type does not match public key type"u8));
        }
        if (!bytes.Equal(priv.Public()._<ed25519.PublicKey>(), pub)) {
            return fail(errors.New("tls: private key does not match public key"u8));
        }
        break;
    }
    default: {
        var pub = (~x509Cert).PublicKey.type();
        return fail(errors.New("tls: unknown public key algorithm"u8));
    }}
    return (cert, default!);
}

// Attempt to parse the given private key DER block. OpenSSL 0.9.8 generates
// PKCS #1 private keys by default, while OpenSSL 1.0.0 generates PKCS #8 keys.
// OpenSSL ecparam generates SEC1 EC private keys for ECDSA. We try all three.
internal static (crypto.PrivateKey, error) parsePrivateKey(slice<byte> der) {
    {
        (key, err) = x509.ParsePKCS1PrivateKey(der); if (err == default!) {
            return (key, default!);
        }
    }
    {
        (key, err) = x509.ParsePKCS8PrivateKey(der); if (err == default!) {
            switch (key.type()) {
            case ж<rsa.PrivateKey> key: {
                return (key, default!);
            }
            case ж<ecdsa.PrivateKey> key: {
                return (key, default!);
            }
            case ed25519.PrivateKey key: {
                return (key, default!);
            }
            default: {
                var key = key.type();
                return (default!, errors.New("tls: found unknown private key type in PKCS#8 wrapping"u8));
            }}
        }
    }
    {
        (key, err) = x509.ParseECPrivateKey(der); if (err == default!) {
            return (key, default!);
        }
    }
    return (default!, errors.New("tls: failed to parse private key"u8));
}

} // end tls_package
