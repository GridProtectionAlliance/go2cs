// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package smtp -- go2cs converted at 2020 August 29 08:36:38 UTC
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
            (slice<byte>, error) Start(ref ServerInfo server); // Next continues the authentication. The server has just sent
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
            return ref new plainAuth(identity,username,password,host);
        }

        private static bool isLocalhost(@string name)
        {
            return name == "localhost" || name == "127.0.0.1" || name == "::1";
        }

        private static (@string, slice<byte>, error) Start(this ref plainAuth a, ref ServerInfo server)
        { 
            // Must have TLS, or else localhost server.
            // Note: If TLS is not true, then we can't trust ANYTHING in ServerInfo.
            // In particular, it doesn't matter if the server advertises PLAIN auth.
            // That might just be the attacker saying
            // "it's ok, you can trust me with your password."
            if (!server.TLS && !isLocalhost(server.Name))
            {
                return ("", null, errors.New("unencrypted connection"));
            }
            if (server.Name != a.host)
            {
                return ("", null, errors.New("wrong host name"));
            }
            slice<byte> resp = (slice<byte>)a.identity + "\x00" + a.username + "\x00" + a.password;
            return ("PLAIN", resp, null);
        }

        private static (slice<byte>, error) Next(this ref plainAuth a, slice<byte> fromServer, bool more)
        {
            if (more)
            { 
                // We've already sent everything.
                return (null, errors.New("unexpected server challenge"));
            }
            return (null, null);
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
            return ref new cramMD5Auth(username,secret);
        }

        private static (@string, slice<byte>, error) Start(this ref cramMD5Auth a, ref ServerInfo server)
        {
            return ("CRAM-MD5", null, null);
        }

        private static (slice<byte>, error) Next(this ref cramMD5Auth a, slice<byte> fromServer, bool more)
        {
            if (more)
            {
                var d = hmac.New(md5.New, (slice<byte>)a.secret);
                d.Write(fromServer);
                var s = make_slice<byte>(0L, d.Size());
                return ((slice<byte>)fmt.Sprintf("%s %x", a.username, d.Sum(s)), null);
            }
            return (null, null);
        }
    }
}}
