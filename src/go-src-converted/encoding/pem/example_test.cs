// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using x509 = crypto.x509_package;
using pem = go.encoding.pem_package;
using fmt = fmt_package;
using log = log_package;
using os = os_package;
using crypto;
using go.encoding;
using io = io_package;

partial class pem_test_package {

public static void ExampleDecode() {
    slice<byte> pubPEMData = slice<byte>("""

-----BEGIN PUBLIC KEY-----
MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAlRuRnThUjU8/prwYxbty
WPT9pURI3lbsKMiB6Fn/VHOKE13p4D8xgOCADpdRagdT6n4etr9atzDKUSvpMtR3
CP5noNc97WiNCggBjVWhs7szEe8ugyqF23XwpHQ6uV1LKH50m92MbOWfCtjU9p/x
qhNpQQ1AZhqNy5Gevap5k8XzRmjSldNAFZMY7Yv3Gi+nyCwGwpVtBUwhuLzgNFK/
yDtw2WcWmUU7NuC8Q6MWvPebxVtCfVp/iQU6q60yyt6aGOBkhAX0LpKAEhKidixY
nP9PNVBvxgu3XZ4P36gZV6+ummKdBVnc3NqwBLu5+CcdRdusmHPHd5pHf4/38Z3/
6qU2a/fPvWzceVTEgZ47QjFMTCTmCwNt29cvi7zZeQzjtwQgn4ipN9NibRH/Ax/q
TbIzHfrJ1xa2RteWSdFjwtxi9C20HUkjXSeI4YlzQMH0fPX6KCE7aVePTOnB69I/
a9/q96DiXZajwlpq3wFctrs1oXqBp5DVrCIj8hU2wNgB7LtQ1mCtsYz//heai0K9
PhE4X6hiE0YmeAZjR0uHl8M/5aW9xCoJ72+12kKpWAa0SFRWLy6FejNYCYpkupVJ
yecLk/4L1W0l6jQQZnWErXZYe0PNFcmwGXy1Rep83kfBRNKRy5tvocalLlwXLdUk
AIU+2GKjyT3iMuzZxxFxPFMCAwEAAQ==
-----END PUBLIC KEY-----
and some more
"""u8);
    var (block, rest) = pem.Decode(pubPEMData);
    if (block == nil || (~block).Type != "PUBLIC KEY"u8) {
        log.Fatal("failed to decode PEM block containing public key");
    }
    var (pub, err) = x509.ParsePKIXPublicKey((~block).Bytes);
    if (err != default!) {
        log.Fatal(err);
    }
    fmt.Printf("Got a %T, with remaining data: %q"u8, pub, rest);
}

// Output: Got a *rsa.PublicKey, with remaining data: "and some more"
public static void ExampleEncode() {
    var block = Ꮡ(new pem.Block(
        Type: "MESSAGE"u8,
        Headers: new map<@string, @string>{
            ["Animal"u8] = "Gopher"u8
        },
        Bytes: slice<byte>("test"u8)
    ));
    {
        var err = pem.Encode(new os.FileжWriter(os.Stdout), block); if (err != default!) {
            log.Fatal(err);
        }
    }
}

// Output:
// -----BEGIN MESSAGE-----
// Animal: Gopher
//
// dGVzdA==
// -----END MESSAGE-----

} // end pem_test_package
