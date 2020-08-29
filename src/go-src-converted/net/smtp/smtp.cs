// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package smtp implements the Simple Mail Transfer Protocol as defined in RFC 5321.
// It also implements the following extensions:
//    8BITMIME  RFC 1652
//    AUTH      RFC 2554
//    STARTTLS  RFC 3207
// Additional extensions may be handled by clients.
//
// The smtp package is frozen and is not accepting new features.
// Some external packages provide more functionality. See:
//
//   https://godoc.org/?q=smtp
// package smtp -- go2cs converted at 2020 August 29 08:36:39 UTC
// import "net/smtp" ==> using smtp = go.net.smtp_package
// Original source: C:\Go\src\net\smtp\smtp.go
using tls = go.crypto.tls_package;
using base64 = go.encoding.base64_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using net = go.net_package;
using textproto = go.net.textproto_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace net
{
    public static partial class smtp_package
    {
        // A Client represents a client connection to an SMTP server.
        public partial struct Client
        {
            public ptr<textproto.Conn> Text; // keep a reference to the connection so it can be used to create a TLS
// connection later
            public net.Conn conn; // whether the Client is using TLS
            public bool tls;
            public @string serverName; // map of supported extensions
            public map<@string, @string> ext; // supported auth mechanisms
            public slice<@string> auth;
            public @string localName; // the name to use in HELO/EHLO
            public bool didHello; // whether we've said HELO/EHLO
            public error helloError; // the error from the hello
        }

        // Dial returns a new Client connected to an SMTP server at addr.
        // The addr must include a port, as in "mail.example.com:smtp".
        public static (ref Client, error) Dial(@string addr)
        {
            var (conn, err) = net.Dial("tcp", addr);
            if (err != null)
            {
                return (null, err);
            }
            var (host, _, _) = net.SplitHostPort(addr);
            return NewClient(conn, host);
        }

        // NewClient returns a new Client using an existing connection and host as a
        // server name to be used when authenticating.
        public static (ref Client, error) NewClient(net.Conn conn, @string host)
        {
            var text = textproto.NewConn(conn);
            var (_, _, err) = text.ReadResponse(220L);
            if (err != null)
            {
                text.Close();
                return (null, err);
            }
            Client c = ref new Client(Text:text,conn:conn,serverName:host,localName:"localhost");
            _, c.tls = conn._<ref tls.Conn>();
            return (c, null);
        }

        // Close closes the connection.
        private static error Close(this ref Client c)
        {
            return error.As(c.Text.Close());
        }

        // hello runs a hello exchange if needed.
        private static error hello(this ref Client c)
        {
            if (!c.didHello)
            {
                c.didHello = true;
                var err = c.ehlo();
                if (err != null)
                {
                    c.helloError = c.helo();
                }
            }
            return error.As(c.helloError);
        }

        // Hello sends a HELO or EHLO to the server as the given host name.
        // Calling this method is only necessary if the client needs control
        // over the host name used. The client will introduce itself as "localhost"
        // automatically otherwise. If Hello is called, it must be called before
        // any of the other methods.
        private static error Hello(this ref Client c, @string localName)
        {
            {
                var err = validateLine(localName);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            if (c.didHello)
            {
                return error.As(errors.New("smtp: Hello called after other methods"));
            }
            c.localName = localName;
            return error.As(c.hello());
        }

        // cmd is a convenience function that sends a command and returns the response
        private static (long, @string, error) cmd(this ref Client _c, long expectCode, @string format, params object[] args) => func(_c, (ref Client c, Defer defer, Panic _, Recover __) =>
        {
            var (id, err) = c.Text.Cmd(format, args);
            if (err != null)
            {
                return (0L, "", err);
            }
            c.Text.StartResponse(id);
            defer(c.Text.EndResponse(id));
            var (code, msg, err) = c.Text.ReadResponse(expectCode);
            return (code, msg, err);
        });

        // helo sends the HELO greeting to the server. It should be used only when the
        // server does not support ehlo.
        private static error helo(this ref Client c)
        {
            c.ext = null;
            var (_, _, err) = c.cmd(250L, "HELO %s", c.localName);
            return error.As(err);
        }

        // ehlo sends the EHLO (extended hello) greeting to the server. It
        // should be the preferred greeting for servers that support it.
        private static error ehlo(this ref Client c)
        {
            var (_, msg, err) = c.cmd(250L, "EHLO %s", c.localName);
            if (err != null)
            {
                return error.As(err);
            }
            var ext = make_map<@string, @string>();
            var extList = strings.Split(msg, "\n");
            if (len(extList) > 1L)
            {
                extList = extList[1L..];
                foreach (var (_, line) in extList)
                {
                    var args = strings.SplitN(line, " ", 2L);
                    if (len(args) > 1L)
                    {
                        ext[args[0L]] = args[1L];
                    }
                    else
                    {
                        ext[args[0L]] = "";
                    }
                }
            }
            {
                var (mechs, ok) = ext["AUTH"];

                if (ok)
                {
                    c.auth = strings.Split(mechs, " ");
                }

            }
            c.ext = ext;
            return error.As(err);
        }

        // StartTLS sends the STARTTLS command and encrypts all further communication.
        // Only servers that advertise the STARTTLS extension support this function.
        private static error StartTLS(this ref Client c, ref tls.Config config)
        {
            {
                var err = c.hello();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            var (_, _, err) = c.cmd(220L, "STARTTLS");
            if (err != null)
            {
                return error.As(err);
            }
            c.conn = tls.Client(c.conn, config);
            c.Text = textproto.NewConn(c.conn);
            c.tls = true;
            return error.As(c.ehlo());
        }

        // TLSConnectionState returns the client's TLS connection state.
        // The return values are their zero values if StartTLS did
        // not succeed.
        private static (tls.ConnectionState, bool) TLSConnectionState(this ref Client c)
        {
            ref tls.Conn (tc, ok) = c.conn._<ref tls.Conn>();
            if (!ok)
            {
                return;
            }
            return (tc.ConnectionState(), true);
        }

        // Verify checks the validity of an email address on the server.
        // If Verify returns nil, the address is valid. A non-nil return
        // does not necessarily indicate an invalid address. Many servers
        // will not verify addresses for security reasons.
        private static error Verify(this ref Client c, @string addr)
        {
            {
                var err__prev1 = err;

                var err = validateLine(addr);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = c.hello();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            var (_, _, err) = c.cmd(250L, "VRFY %s", addr);
            return error.As(err);
        }

        // Auth authenticates a client using the provided authentication mechanism.
        // A failed authentication closes the connection.
        // Only servers that advertise the AUTH extension support this function.
        private static error Auth(this ref Client c, Auth a)
        {
            {
                var err = c.hello();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            var encoding = base64.StdEncoding;
            var (mech, resp, err) = a.Start(ref new ServerInfo(c.serverName,c.tls,c.auth));
            if (err != null)
            {
                c.Quit();
                return error.As(err);
            }
            var resp64 = make_slice<byte>(encoding.EncodedLen(len(resp)));
            encoding.Encode(resp64, resp);
            var (code, msg64, err) = c.cmd(0L, strings.TrimSpace(fmt.Sprintf("AUTH %s %s", mech, resp64)));
            while (err == null)
            {
                slice<byte> msg = default;
                switch (code)
                {
                    case 334L: 
                        msg, err = encoding.DecodeString(msg64);
                        break;
                    case 235L: 
                        // the last message isn't base64 because it isn't a challenge
                        msg = (slice<byte>)msg64;
                        break;
                    default: 
                        err = ref new textproto.Error(Code:code,Msg:msg64);
                        break;
                }
                if (err == null)
                {
                    resp, err = a.Next(msg, code == 334L);
                }
                if (err != null)
                { 
                    // abort the AUTH
                    c.cmd(501L, "*");
                    c.Quit();
                    break;
                }
                if (resp == null)
                {
                    break;
                }
                resp64 = make_slice<byte>(encoding.EncodedLen(len(resp)));
                encoding.Encode(resp64, resp);
                code, msg64, err = c.cmd(0L, string(resp64));
            }

            return error.As(err);
        }

        // Mail issues a MAIL command to the server using the provided email address.
        // If the server supports the 8BITMIME extension, Mail adds the BODY=8BITMIME
        // parameter.
        // This initiates a mail transaction and is followed by one or more Rcpt calls.
        private static error Mail(this ref Client c, @string from)
        {
            {
                var err__prev1 = err;

                var err = validateLine(from);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = c.hello();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            @string cmdStr = "MAIL FROM:<%s>";
            if (c.ext != null)
            {
                {
                    var (_, ok) = c.ext["8BITMIME"];

                    if (ok)
                    {
                        cmdStr += " BODY=8BITMIME";
                    }

                }
            }
            var (_, _, err) = c.cmd(250L, cmdStr, from);
            return error.As(err);
        }

        // Rcpt issues a RCPT command to the server using the provided email address.
        // A call to Rcpt must be preceded by a call to Mail and may be followed by
        // a Data call or another Rcpt call.
        private static error Rcpt(this ref Client c, @string to)
        {
            {
                var err = validateLine(to);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            var (_, _, err) = c.cmd(25L, "RCPT TO:<%s>", to);
            return error.As(err);
        }

        private partial struct dataCloser : io.WriteCloser
        {
            public ptr<Client> c;
            public ref io.WriteCloser WriteCloser => ref WriteCloser_val;
        }

        private static error Close(this ref dataCloser d)
        {
            d.WriteCloser.Close();
            var (_, _, err) = d.c.Text.ReadResponse(250L);
            return error.As(err);
        }

        // Data issues a DATA command to the server and returns a writer that
        // can be used to write the mail headers and body. The caller should
        // close the writer before calling any more methods on c. A call to
        // Data must be preceded by one or more calls to Rcpt.
        private static (io.WriteCloser, error) Data(this ref Client c)
        {
            var (_, _, err) = c.cmd(354L, "DATA");
            if (err != null)
            {
                return (null, err);
            }
            return (ref new dataCloser(c,c.Text.DotWriter()), null);
        }

        private static Action<ref tls.Config> testHookStartTLS = default; // nil, except for tests

        // SendMail connects to the server at addr, switches to TLS if
        // possible, authenticates with the optional mechanism a if possible,
        // and then sends an email from address from, to addresses to, with
        // message msg.
        // The addr must include a port, as in "mail.example.com:smtp".
        //
        // The addresses in the to parameter are the SMTP RCPT addresses.
        //
        // The msg parameter should be an RFC 822-style email with headers
        // first, a blank line, and then the message body. The lines of msg
        // should be CRLF terminated. The msg headers should usually include
        // fields such as "From", "To", "Subject", and "Cc".  Sending "Bcc"
        // messages is accomplished by including an email address in the to
        // parameter but not including it in the msg headers.
        //
        // The SendMail function and the net/smtp package are low-level
        // mechanisms and provide no support for DKIM signing, MIME
        // attachments (see the mime/multipart package), or other mail
        // functionality. Higher-level packages exist outside of the standard
        // library.
        public static error SendMail(@string addr, Auth a, @string from, slice<@string> to, slice<byte> msg) => func((defer, _, __) =>
        {
            {
                var err__prev1 = err;

                var err = validateLine(from);

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            foreach (var (_, recp) in to)
            {
                {
                    var err__prev1 = err;

                    err = validateLine(recp);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev1;

                }
            }
            var (c, err) = Dial(addr);
            if (err != null)
            {
                return error.As(err);
            }
            defer(c.Close());
            err = c.hello();

            if (err != null)
            {
                return error.As(err);
            }
            {
                var (ok, _) = c.Extension("STARTTLS");

                if (ok)
                {
                    tls.Config config = ref new tls.Config(ServerName:c.serverName);
                    if (testHookStartTLS != null)
                    {
                        testHookStartTLS(config);
                    }
                    err = c.StartTLS(config);

                    if (err != null)
                    {
                        return error.As(err);
                    }
                }

            }
            if (a != null && c.ext != null)
            {
                {
                    var (_, ok) = c.ext["AUTH"];

                    if (ok)
                    {
                        err = c.Auth(a);

                        if (err != null)
                        {
                            return error.As(err);
                        }
                    }

                }
            }
            err = c.Mail(from);

            if (err != null)
            {
                return error.As(err);
            }
            foreach (var (_, addr) in to)
            {
                err = c.Rcpt(addr);

                if (err != null)
                {
                    return error.As(err);
                }
            }
            var (w, err) = c.Data();
            if (err != null)
            {
                return error.As(err);
            }
            _, err = w.Write(msg);
            if (err != null)
            {
                return error.As(err);
            }
            err = w.Close();
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(c.Quit());
        });

        // Extension reports whether an extension is support by the server.
        // The extension name is case-insensitive. If the extension is supported,
        // Extension also returns a string that contains any parameters the
        // server specifies for the extension.
        private static (bool, @string) Extension(this ref Client c, @string ext)
        {
            {
                var err = c.hello();

                if (err != null)
                {
                    return (false, "");
                }

            }
            if (c.ext == null)
            {
                return (false, "");
            }
            ext = strings.ToUpper(ext);
            var (param, ok) = c.ext[ext];
            return (ok, param);
        }

        // Reset sends the RSET command to the server, aborting the current mail
        // transaction.
        private static error Reset(this ref Client c)
        {
            {
                var err = c.hello();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            var (_, _, err) = c.cmd(250L, "RSET");
            return error.As(err);
        }

        // Noop sends the NOOP command to the server. It does nothing but check
        // that the connection to the server is okay.
        private static error Noop(this ref Client c)
        {
            {
                var err = c.hello();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            var (_, _, err) = c.cmd(250L, "NOOP");
            return error.As(err);
        }

        // Quit sends the QUIT command and closes the connection to the server.
        private static error Quit(this ref Client c)
        {
            {
                var err = c.hello();

                if (err != null)
                {
                    return error.As(err);
                }

            }
            var (_, _, err) = c.cmd(221L, "QUIT");
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(c.Text.Close());
        }

        // validateLine checks to see if a line has CR or LF as per RFC 5321
        private static error validateLine(@string line)
        {
            if (strings.ContainsAny(line, "\n\r"))
            {
                return error.As(errors.New("smtp: A line must not contain CR or LF"));
            }
            return error.As(null);
        }
    }
}}
