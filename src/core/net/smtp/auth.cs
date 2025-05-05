// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using hmac = crypto.hmac_package;
using md5 = crypto.md5_package;
using errors = errors_package;
using fmt = fmt_package;
using crypto;

partial class smtp_package {

// Auth is implemented by an SMTP authentication mechanism.
[GoType] partial interface ΔAuth {
    // Start begins an authentication with a server.
    // It returns the name of the authentication protocol
    // and optionally data to include in the initial AUTH message
    // sent to the server.
    // If it returns a non-nil error, the SMTP client aborts
    // the authentication attempt and closes the connection.
    (@string proto, slice<byte> toServer, error err) Start(ж<ServerInfo> server);
    // Next continues the authentication. The server has just sent
    // the fromServer data. If more is true, the server expects a
    // response, which Next should return as toServer; otherwise
    // Next should return toServer == nil.
    // If Next returns a non-nil error, the SMTP client aborts
    // the authentication attempt and closes the connection.
    (slice<byte> toServer, error err) Next(slice<byte> fromServer, bool more);
}

// ServerInfo records information about an SMTP server.
[GoType] partial struct ServerInfo {
    public @string Name;  // SMTP server name
    public bool TLS;     // using TLS, with valid certificate for Name
    public slice<@string> Auth; // advertised authentication mechanisms
}

[GoType] partial struct plainAuth {
    internal @string identity;
    internal @string username;
    internal @string password;
    internal @string host;
}

// PlainAuth returns an [Auth] that implements the PLAIN authentication
// mechanism as defined in RFC 4616. The returned Auth uses the given
// username and password to authenticate to host and act as identity.
// Usually identity should be the empty string, to act as username.
//
// PlainAuth will only send the credentials if the connection is using TLS
// or is connected to localhost. Otherwise authentication will fail with an
// error, without sending the credentials.
public static ΔAuth PlainAuth(@string identity, @string username, @string password, @string host) {
    return new plainAuth(identity, username, password, host);
}

internal static bool isLocalhost(@string name) {
    return name == "localhost"u8 || name == "127.0.0.1"u8 || name == "::1"u8;
}

[GoRecv] internal static (@string, slice<byte>, error) Start(this ref plainAuth a, ж<ServerInfo> Ꮡserver) {
    ref var server = ref Ꮡserver.val;

    // Must have TLS, or else localhost server.
    // Note: If TLS is not true, then we can't trust ANYTHING in ServerInfo.
    // In particular, it doesn't matter if the server advertises PLAIN auth.
    // That might just be the attacker saying
    // "it's ok, you can trust me with your password."
    if (!server.TLS && !isLocalhost(server.Name)) {
        return ("", default!, errors.New("unencrypted connection"u8));
    }
    if (server.Name != a.host) {
        return ("", default!, errors.New("wrong host name"u8));
    }
    var resp = slice<byte>(a.identity + "\x00"u8 + a.username + "\x00"u8 + a.password);
    return ("PLAIN", resp, default!);
}

[GoRecv] internal static (slice<byte>, error) Next(this ref plainAuth a, slice<byte> fromServer, bool more) {
    if (more) {
        // We've already sent everything.
        return (default!, errors.New("unexpected server challenge"u8));
    }
    return (default!, default!);
}

[GoType] partial struct cramMD5Auth {
    internal @string username;
    internal @string secret;
}

// CRAMMD5Auth returns an [Auth] that implements the CRAM-MD5 authentication
// mechanism as defined in RFC 2195.
// The returned Auth uses the given username and secret to authenticate
// to the server using the challenge-response mechanism.
public static ΔAuth CRAMMD5Auth(@string username, @string secret) {
    return new cramMD5Auth(username, secret);
}

[GoRecv] internal static (@string, slice<byte>, error) Start(this ref cramMD5Auth a, ж<ServerInfo> Ꮡserver) {
    ref var server = ref Ꮡserver.val;

    return ("CRAM-MD5", default!, default!);
}

[GoRecv] internal static (slice<byte>, error) Next(this ref cramMD5Auth a, slice<byte> fromServer, bool more) {
    if (more) {
        var d = hmac.New(md5.New, slice<byte>(a.secret));
        d.Write(fromServer);
        var s = new slice<byte>(0, d.Size());
        return (fmt.Appendf(default!, "%s %x"u8, a.username, d.Sum(s)), default!);
    }
    return (default!, default!);
}

} // end smtp_package
