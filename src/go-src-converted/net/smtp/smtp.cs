// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package smtp implements the Simple Mail Transfer Protocol as defined in RFC 5321.
// It also implements the following extensions:
//
//	8BITMIME  RFC 1652
//	AUTH      RFC 2554
//	STARTTLS  RFC 3207
//
// Additional extensions may be handled by clients.
//
// The smtp package is frozen and is not accepting new features.
// Some external packages provide more functionality. See:
//
//	https://godoc.org/?q=smtp
namespace go.net;

using tls = crypto.tls_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using textproto = net.textproto_package;
using strings = strings_package;
using crypto;
using encoding;
using ꓸꓸꓸany = Span<any>;

partial class smtp_package {

// A Client represents a client connection to an SMTP server.
[GoType] partial struct Client {
    // Text is the textproto.Conn used by the Client. It is exported to allow for
    // clients to add extensions.
    public ж<net.textproto_package.Conn> Text;
    // keep a reference to the connection so it can be used to create a TLS
    // connection later
    internal net_package.Conn conn;
    // whether the Client is using TLS
    internal bool tls;
    internal @string serverName;
    // map of supported extensions
    internal map<@string, @string> ext;
    // supported auth mechanisms
    internal slice<@string> auth;
    internal @string localName; // the name to use in HELO/EHLO
    internal bool didHello;   // whether we've said HELO/EHLO
    internal error helloError;  // the error from the hello
}

// Dial returns a new [Client] connected to an SMTP server at addr.
// The addr must include a port, as in "mail.example.com:smtp".
public static (ж<Client>, error) Dial(@string addr) {
    (conn, err) = net.Dial("tcp"u8, addr);
    if (err != default!) {
        return (default!, err);
    }
    var (host, _, _) = net.SplitHostPort(addr);
    return NewClient(conn, host);
}

// NewClient returns a new [Client] using an existing connection and host as a
// server name to be used when authenticating.
public static (ж<Client>, error) NewClient(net.Conn conn, @string host) {
    var text = textproto.NewConn(conn);
    var (_, _, err) = text.ReadResponse(220);
    if (err != default!) {
        text.Close();
        return (default!, err);
    }
    var c = Ꮡ(new Client(Text: text, conn: conn, serverName: host, localName: "localhost"u8));
    (_, c.val.tls) = conn._<ж<tls.Conn>>(ᐧ);
    return (c, default!);
}

// Close closes the connection.
[GoRecv] public static error Close(this ref Client c) {
    return c.Text.Close();
}

// hello runs a hello exchange if needed.
[GoRecv] internal static error hello(this ref Client c) {
    if (!c.didHello) {
        c.didHello = true;
        var err = c.ehlo();
        if (err != default!) {
            c.helloError = c.helo();
        }
    }
    return c.helloError;
}

// Hello sends a HELO or EHLO to the server as the given host name.
// Calling this method is only necessary if the client needs control
// over the host name used. The client will introduce itself as "localhost"
// automatically otherwise. If Hello is called, it must be called before
// any of the other methods.
[GoRecv] public static error Hello(this ref Client c, @string localName) {
    {
        var err = validateLine(localName); if (err != default!) {
            return err;
        }
    }
    if (c.didHello) {
        return errors.New("smtp: Hello called after other methods"u8);
    }
    c.localName = localName;
    return c.hello();
}

// cmd is a convenience function that sends a command and returns the response
[GoRecv] internal static (nint, @string, error) cmd(this ref Client c, nint expectCode, @string format, params ꓸꓸꓸany argsʗp) => func((defer, _) => {
    var args = argsʗp.slice();

    var (id, err) = c.Text.Cmd(format, args.ꓸꓸꓸ);
    if (err != default!) {
        return (0, "", err);
    }
    c.Text.StartResponse(id);
    deferǃ(c.Text.EndResponse, id, defer);
    var (code, msg, err) = c.Text.ReadResponse(expectCode);
    return (code, msg, err);
});

// helo sends the HELO greeting to the server. It should be used only when the
// server does not support ehlo.
[GoRecv] internal static error helo(this ref Client c) {
    c.ext = default!;
    var (_, _, err) = c.cmd(250, "HELO %s"u8, c.localName);
    return err;
}

// ehlo sends the EHLO (extended hello) greeting to the server. It
// should be the preferred greeting for servers that support it.
[GoRecv] internal static error ehlo(this ref Client c) {
    var (_, msg, err) = c.cmd(250, "EHLO %s"u8, c.localName);
    if (err != default!) {
        return err;
    }
    var ext = new map<@string, @string>();
    var extList = strings.Split(msg, "\n"u8);
    if (len(extList) > 1) {
        extList = extList[1..];
        foreach (var (_, line) in extList) {
            var (k, v, _) = strings.Cut(line, " "u8);
            ext[k] = v;
        }
    }
    {
        @string mechs = ext["AUTH"u8];
        var ok = ext["AUTH"u8]; if (ok) {
            c.auth = strings.Split(mechs, " "u8);
        }
    }
    c.ext = ext;
    return err;
}

// StartTLS sends the STARTTLS command and encrypts all further communication.
// Only servers that advertise the STARTTLS extension support this function.
[GoRecv] public static error StartTLS(this ref Client c, ж<tls.Config> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    {
        var errΔ1 = c.hello(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, _, err) = c.cmd(220, "STARTTLS"u8);
    if (err != default!) {
        return err;
    }
    c.conn = tls.Client(c.conn, Ꮡconfig);
    c.Text = textproto.NewConn(c.conn);
    c.tls = true;
    return c.ehlo();
}

// TLSConnectionState returns the client's TLS connection state.
// The return values are their zero values if [Client.StartTLS] did
// not succeed.
[GoRecv] public static (tlsꓸConnectionState state, bool ok) TLSConnectionState(this ref Client c) {
    tlsꓸConnectionState state = default!;
    bool ok = default!;

    (tc, ok) = c.conn._<ж<tls.Conn>>(ᐧ);
    if (!ok) {
        return (state, ok);
    }
    return (tc.ConnectionState(), true);
}

// Verify checks the validity of an email address on the server.
// If Verify returns nil, the address is valid. A non-nil return
// does not necessarily indicate an invalid address. Many servers
// will not verify addresses for security reasons.
[GoRecv] public static error Verify(this ref Client c, @string addr) {
    {
        var errΔ1 = validateLine(addr); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var errΔ2 = c.hello(); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    var (_, _, err) = c.cmd(250, "VRFY %s"u8, addr);
    return err;
}

// Auth authenticates a client using the provided authentication mechanism.
// A failed authentication closes the connection.
// Only servers that advertise the AUTH extension support this function.
[GoRecv] public static error Auth(this ref Client c, ΔAuth a) {
    {
        var errΔ1 = c.hello(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var encoding = base64.StdEncoding;
    var (mech, resp, err) = a.Start(Ꮡ(new ServerInfo(c.serverName, c.tls, c.auth)));
    if (err != default!) {
        c.Quit();
        return err;
    }
    var resp64 = new slice<byte>(encoding.EncodedLen(len(resp)));
    encoding.Encode(resp64, resp);
    (code, msg64, err) = c.cmd(0, "%s"u8, strings.TrimSpace(fmt.Sprintf("AUTH %s %s"u8, mech, resp64)));
    while (err == default!) {
        slice<byte> msg = default!;
        switch (code) {
        case 334: {
            (msg, err) = encoding.DecodeString(msg64);
            break;
        }
        case 235: {
            msg = slice<byte>(msg64);
            break;
        }
        default: {
            Ꮡerr = new textprotoꓸError( // the last message isn't base64 because it isn't a challenge
Code: code, Msg: msg64); err = ref Ꮡerr.val;
            break;
        }}

        if (err == default!) {
            (resp, err) = a.Next(msg, code == 334);
        }
        if (err != default!) {
            // abort the AUTH
            c.cmd(501, "*"u8);
            c.Quit();
            break;
        }
        if (resp == default!) {
            break;
        }
        resp64 = new slice<byte>(encoding.EncodedLen(len(resp)));
        encoding.Encode(resp64, resp);
        (code, msg64, err) = c.cmd(0, "%s"u8, resp64);
    }
    return err;
}

// Mail issues a MAIL command to the server using the provided email address.
// If the server supports the 8BITMIME extension, Mail adds the BODY=8BITMIME
// parameter. If the server supports the SMTPUTF8 extension, Mail adds the
// SMTPUTF8 parameter.
// This initiates a mail transaction and is followed by one or more [Client.Rcpt] calls.
[GoRecv] public static error Mail(this ref Client c, @string from) {
    {
        var errΔ1 = validateLine(from); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var errΔ2 = c.hello(); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    @string cmdStr = "MAIL FROM:<%s>"u8;
    if (c.ext != default!) {
        {
            @string _ = c.ext["8BITMIME"u8];
            var ok = c.ext["8BITMIME"u8]; if (ok) {
                cmdStr += " BODY=8BITMIME"u8;
            }
        }
        {
            @string _ = c.ext["SMTPUTF8"u8];
            var ok = c.ext["SMTPUTF8"u8]; if (ok) {
                cmdStr += " SMTPUTF8"u8;
            }
        }
    }
    var (_, _, err) = c.cmd(250, cmdStr, from);
    return err;
}

// Rcpt issues a RCPT command to the server using the provided email address.
// A call to Rcpt must be preceded by a call to [Client.Mail] and may be followed by
// a [Client.Data] call or another Rcpt call.
[GoRecv] public static error Rcpt(this ref Client c, @string to) {
    {
        var errΔ1 = validateLine(to); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, _, err) = c.cmd(25, "RCPT TO:<%s>"u8, to);
    return err;
}

[GoType] partial struct dataCloser {
    internal ж<Client> c;
    public partial ref io_package.WriteCloser WriteCloser { get; }
}

[GoRecv] internal static error Close(this ref dataCloser d) {
    d.WriteCloser.Close();
    var (_, _, err) = d.c.Text.ReadResponse(250);
    return err;
}

// Data issues a DATA command to the server and returns a writer that
// can be used to write the mail headers and body. The caller should
// close the writer before calling any more methods on c. A call to
// Data must be preceded by one or more calls to [Client.Rcpt].
[GoRecv] public static (io.WriteCloser, error) Data(this ref Client c) {
    var (_, _, err) = c.cmd(354, "DATA"u8);
    if (err != default!) {
        return (default!, err);
    }
    return (new dataCloser(c, c.Text.DotWriter()), default!);
}

internal static tls.Config) testHookStartTLS; // nil, except for tests

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
public static error SendMail(@string addr, ΔAuth a, @string from, slice<@string> to, slice<byte> msg) => func((defer, _) => {
    {
        var errΔ1 = validateLine(from); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    foreach (var (_, recp) in to) {
        {
            var errΔ2 = validateLine(recp); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
    }
    (c, err) = Dial(addr);
    if (err != default!) {
        return err;
    }
    var cʗ1 = c;
    defer(cʗ1.Close);
    {
        err = c.hello(); if (err != default!) {
            return err;
        }
    }
    {
        var (ok, _) = c.Extension("STARTTLS"u8); if (ok) {
            var config = Ꮡ(new tls.Config(ServerName: (~c).serverName));
            if (testHookStartTLS != default!) {
                testHookStartTLS(config);
            }
            {
                err = c.StartTLS(config); if (err != default!) {
                    return err;
                }
            }
        }
    }
    if (a != default! && (~c).ext != default!) {
        {
            @string _ = (~c).ext["AUTH"u8];
            var ok = (~c).ext["AUTH"u8]; if (!ok) {
                return errors.New("smtp: server doesn't support AUTH"u8);
            }
        }
        {
            err = c.Auth(a); if (err != default!) {
                return err;
            }
        }
    }
    {
        err = c.Mail(from); if (err != default!) {
            return err;
        }
    }
    foreach (var (_, addrΔ1) in to) {
        {
            err = c.Rcpt(addrΔ1); if (err != default!) {
                return err;
            }
        }
    }
    (w, err) = c.Data();
    if (err != default!) {
        return err;
    }
    (_, err) = w.Write(msg);
    if (err != default!) {
        return err;
    }
    err = w.Close();
    if (err != default!) {
        return err;
    }
    return c.Quit();
});

// Extension reports whether an extension is support by the server.
// The extension name is case-insensitive. If the extension is supported,
// Extension also returns a string that contains any parameters the
// server specifies for the extension.
[GoRecv] public static (bool, @string) Extension(this ref Client c, @string ext) {
    {
        var err = c.hello(); if (err != default!) {
            return (false, "");
        }
    }
    if (c.ext == default!) {
        return (false, "");
    }
    ext = strings.ToUpper(ext);
    @string param = c.ext[ext];
    var ok = c.ext[ext];
    return (ok, param);
}

// Reset sends the RSET command to the server, aborting the current mail
// transaction.
[GoRecv] public static error Reset(this ref Client c) {
    {
        var errΔ1 = c.hello(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, _, err) = c.cmd(250, "RSET"u8);
    return err;
}

// Noop sends the NOOP command to the server. It does nothing but check
// that the connection to the server is okay.
[GoRecv] public static error Noop(this ref Client c) {
    {
        var errΔ1 = c.hello(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, _, err) = c.cmd(250, "NOOP"u8);
    return err;
}

// Quit sends the QUIT command and closes the connection to the server.
[GoRecv] public static error Quit(this ref Client c) {
    {
        var errΔ1 = c.hello(); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (_, _, err) = c.cmd(221, "QUIT"u8);
    if (err != default!) {
        return err;
    }
    return c.Text.Close();
}

// validateLine checks to see if a line has CR or LF as per RFC 5321.
internal static error validateLine(@string line) {
    if (strings.ContainsAny(line, "\n\r"u8)) {
        return errors.New("smtp: A line must not contain CR or LF"u8);
    }
    return default!;
}

} // end smtp_package
