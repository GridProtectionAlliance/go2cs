// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package smtp -- go2cs converted at 2020 October 08 03:43:29 UTC
// import "net/smtp" ==> using smtp = go.net.smtp_package
// Original source: C:\Go\src\net\smtp\auth.go
using hmac = go.crypto.hmac_package;
using md5 = go.crypto.md5_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace net
{
    public static partial class smtp_package
    {
        // Auth is implemented by an SMTP authentication mechanism.
        public partial interface Auth
        {
            (slice<byte>, error) Start(ptr<ServerInfo> server); // Next continues the authentication. The server has just sent
// the fromServer data. If more is true, the server expects a
// response, which Next should return as toServer; otherwise
// Next should return toServer == nil.
// If Next returns a non-nil error, the SMTP client aborts
// the authentication attempt and closes the connection.
            (slice<byte>, error) Next(slice<byte> fromServer, bool more);
        }

        // ServerInfo records information about an SMTP server.
        public partial struct ServerInfo
        {
            public @string Name; // SMTP server name
            public bool TLS; // using TLS, with valid certificate for Name
            public slice<@string> Auth; // advertised authentication mechanisms
        }

        private partial struct plainAuth
        {
            public @string identity;
            public @string username;
            public @string password;
            public @string host;
        }

        // PlainAuth returns an Auth that implements the PLAIN authentication
        // mechanism as defined in RFC 4616. The returned Auth uses the given
        // username and password to authenticate to host and act as identity.
        // Usually identity should be the empty string, to act as username.
        //
        // PlainAuth will only send the credentials if the connection is using TLS
        // or is connected to localhost. Otherwise authentication will fail with an
        // error, without sending the credentials.
        public static Auth PlainAuth(@string identity, @string username, @string password, @string host)
        {
            return addr(new plainAuth(identity,username,password,host));
        }

        private static bool isLocalhost(@string name)
        {
            return name == "localhost" || name == "127.0.0.1" || name == "::1";
        }

        private static (@string, slice<byte>, error) Start(this ptr<plainAuth> _addr_a, ptr<ServerInfo> _addr_server)
        {
            @string _p0 = default;
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref plainAuth a = ref _addr_a.val;
            ref ServerInfo server = ref _addr_server.val;
 
            // Must have TLS, or else localhost server.
            // Note: If TLS is not true, then we can't trust ANYTHING in ServerInfo.
            // In particular, it doesn't matter if the server advertises PLAIN auth.
            // That might just be the attacker saying
            // "it's ok, you can trust me with your password."
            if (!server.TLS && !isLocalhost(server.Name))
            {
                return ("", null, error.As(errors.New("unencrypted connection"))!);
            }

            if (server.Name != a.host)
            {
                return ("", null, error.As(errors.New("wrong host name"))!);
            }

            slice<byte> resp = (slice<byte>)a.identity + "\x00" + a.username + "\x00" + a.password;
            return ("PLAIN", resp, error.As(null!)!);

        }

        private static (slice<byte>, error) Next(this ptr<plainAuth> _addr_a, slice<byte> fromServer, bool more)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref plainAuth a = ref _addr_a.val;

            if (more)
            { 
                // We've already sent everything.
                return (null, error.As(errors.New("unexpected server challenge"))!);

            }

            return (null, error.As(null!)!);

        }

        private partial struct cramMD5Auth
        {
            public @string username;
            public @string secret;
        }

        // CRAMMD5Auth returns an Auth that implements the CRAM-MD5 authentication
        // mechanism as defined in RFC 2195.
        // The returned Auth uses the given username and secret to authenticate
        // to the server using the challenge-response mechanism.
        public static Auth CRAMMD5Auth(@string username, @string secret)
        {
            return addr(new cramMD5Auth(username,secret));
        }

        private static (@string, slice<byte>, error) Start(this ptr<cramMD5Auth> _addr_a, ptr<ServerInfo> _addr_server)
        {
            @string _p0 = default;
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref cramMD5Auth a = ref _addr_a.val;
            ref ServerInfo server = ref _addr_server.val;

            return ("CRAM-MD5", null, error.As(null!)!);
        }

        private static (slice<byte>, error) Next(this ptr<cramMD5Auth> _addr_a, slice<byte> fromServer, bool more)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref cramMD5Auth a = ref _addr_a.val;

            if (more)
            {
                var d = hmac.New(md5.New, (slice<byte>)a.secret);
                d.Write(fromServer);
                var s = make_slice<byte>(0L, d.Size());
                return ((slice<byte>)fmt.Sprintf("%s %x", a.username, d.Sum(s)), error.As(null!)!);
            }

            return (null, error.As(null!)!);

        }
    }
}}
