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

// package smtp -- go2cs converted at 2022 March 13 05:40:27 UTC
// import "net/smtp" ==> using smtp = go.net.smtp_package
// Original source: C:\Program Files\Go\src\net\smtp\smtp.go
namespace go.net;

using tls = crypto.tls_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using textproto = net.textproto_package;
using strings = strings_package;


// A Client represents a client connection to an SMTP server.

using System;
public static partial class smtp_package {

public partial struct Client {
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
public static (ptr<Client>, error) Dial(@string addr) {
    ptr<Client> _p0 = default!;
    error _p0 = default!;

    var (conn, err) = net.Dial("tcp", addr);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (host, _, _) = net.SplitHostPort(addr);
    return _addr_NewClient(conn, host)!;
}

// NewClient returns a new Client using an existing connection and host as a
// server name to be used when authenticating.
public static (ptr<Client>, error) NewClient(net.Conn conn, @string host) {
    ptr<Client> _p0 = default!;
    error _p0 = default!;

    var text = textproto.NewConn(conn);
    var (_, _, err) = text.ReadResponse(220);
    if (err != null) {
        text.Close();
        return (_addr_null!, error.As(err)!);
    }
    ptr<Client> c = addr(new Client(Text:text,conn:conn,serverName:host,localName:"localhost"));
    _, c.tls = conn._<ptr<tls.Conn>>();
    return (_addr_c!, error.As(null!)!);
}

// Close closes the connection.
private static error Close(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    return error.As(c.Text.Close())!;
}

// hello runs a hello exchange if needed.
private static error hello(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    if (!c.didHello) {
        c.didHello = true;
        var err = c.ehlo();
        if (err != null) {
            c.helloError = c.helo();
        }
    }
    return error.As(c.helloError)!;
}

// Hello sends a HELO or EHLO to the server as the given host name.
// Calling this method is only necessary if the client needs control
// over the host name used. The client will introduce itself as "localhost"
// automatically otherwise. If Hello is called, it must be called before
// any of the other methods.
private static error Hello(this ptr<Client> _addr_c, @string localName) {
    ref Client c = ref _addr_c.val;

    {
        var err = validateLine(localName);

        if (err != null) {
            return error.As(err)!;
        }
    }
    if (c.didHello) {
        return error.As(errors.New("smtp: Hello called after other methods"))!;
    }
    c.localName = localName;
    return error.As(c.hello())!;
}

// cmd is a convenience function that sends a command and returns the response
private static (nint, @string, error) cmd(this ptr<Client> _addr_c, nint expectCode, @string format, params object[] args) => func((defer, _, _) => {
    nint _p0 = default;
    @string _p0 = default;
    error _p0 = default!;
    args = args.Clone();
    ref Client c = ref _addr_c.val;

    var (id, err) = c.Text.Cmd(format, args);
    if (err != null) {
        return (0, "", error.As(err)!);
    }
    c.Text.StartResponse(id);
    defer(c.Text.EndResponse(id));
    var (code, msg, err) = c.Text.ReadResponse(expectCode);
    return (code, msg, error.As(err)!);
});

// helo sends the HELO greeting to the server. It should be used only when the
// server does not support ehlo.
private static error helo(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    c.ext = null;
    var (_, _, err) = c.cmd(250, "HELO %s", c.localName);
    return error.As(err)!;
}

// ehlo sends the EHLO (extended hello) greeting to the server. It
// should be the preferred greeting for servers that support it.
private static error ehlo(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    var (_, msg, err) = c.cmd(250, "EHLO %s", c.localName);
    if (err != null) {
        return error.As(err)!;
    }
    var ext = make_map<@string, @string>();
    var extList = strings.Split(msg, "\n");
    if (len(extList) > 1) {
        extList = extList[(int)1..];
        foreach (var (_, line) in extList) {
            var args = strings.SplitN(line, " ", 2);
            if (len(args) > 1) {
                ext[args[0]] = args[1];
            }
            else
 {
                ext[args[0]] = "";
            }
        }
    }
    {
        var (mechs, ok) = ext["AUTH"];

        if (ok) {
            c.auth = strings.Split(mechs, " ");
        }
    }
    c.ext = ext;
    return error.As(err)!;
}

// StartTLS sends the STARTTLS command and encrypts all further communication.
// Only servers that advertise the STARTTLS extension support this function.
private static error StartTLS(this ptr<Client> _addr_c, ptr<tls.Config> _addr_config) {
    ref Client c = ref _addr_c.val;
    ref tls.Config config = ref _addr_config.val;

    {
        var err = c.hello();

        if (err != null) {
            return error.As(err)!;
        }
    }
    var (_, _, err) = c.cmd(220, "STARTTLS");
    if (err != null) {
        return error.As(err)!;
    }
    c.conn = tls.Client(c.conn, config);
    c.Text = textproto.NewConn(c.conn);
    c.tls = true;
    return error.As(c.ehlo())!;
}

// TLSConnectionState returns the client's TLS connection state.
// The return values are their zero values if StartTLS did
// not succeed.
private static (tls.ConnectionState, bool) TLSConnectionState(this ptr<Client> _addr_c) {
    tls.ConnectionState state = default;
    bool ok = default;
    ref Client c = ref _addr_c.val;

    ptr<tls.Conn> (tc, ok) = c.conn._<ptr<tls.Conn>>();
    if (!ok) {
        return ;
    }
    return (tc.ConnectionState(), true);
}

// Verify checks the validity of an email address on the server.
// If Verify returns nil, the address is valid. A non-nil return
// does not necessarily indicate an invalid address. Many servers
// will not verify addresses for security reasons.
private static error Verify(this ptr<Client> _addr_c, @string addr) {
    ref Client c = ref _addr_c.val;

    {
        var err__prev1 = err;

        var err = validateLine(addr);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    {
        var err__prev1 = err;

        err = c.hello();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    var (_, _, err) = c.cmd(250, "VRFY %s", addr);
    return error.As(err)!;
}

// Auth authenticates a client using the provided authentication mechanism.
// A failed authentication closes the connection.
// Only servers that advertise the AUTH extension support this function.
private static error Auth(this ptr<Client> _addr_c, Auth a) {
    ref Client c = ref _addr_c.val;

    {
        var err = c.hello();

        if (err != null) {
            return error.As(err)!;
        }
    }
    var encoding = base64.StdEncoding;
    var (mech, resp, err) = a.Start(addr(new ServerInfo(c.serverName,c.tls,c.auth)));
    if (err != null) {
        c.Quit();
        return error.As(err)!;
    }
    var resp64 = make_slice<byte>(encoding.EncodedLen(len(resp)));
    encoding.Encode(resp64, resp);
    var (code, msg64, err) = c.cmd(0, strings.TrimSpace(fmt.Sprintf("AUTH %s %s", mech, resp64)));
    while (err == null) {
        slice<byte> msg = default;
        switch (code) {
            case 334: 
                msg, err = encoding.DecodeString(msg64);
                break;
            case 235: 
                // the last message isn't base64 because it isn't a challenge
                msg = (slice<byte>)msg64;
                break;
            default: 
                err = addr(new textproto.Error(Code:code,Msg:msg64));
                break;
        }
        if (err == null) {
            resp, err = a.Next(msg, code == 334);
        }
        if (err != null) { 
            // abort the AUTH
            c.cmd(501, "*");
            c.Quit();
            break;
        }
        if (resp == null) {
            break;
        }
        resp64 = make_slice<byte>(encoding.EncodedLen(len(resp)));
        encoding.Encode(resp64, resp);
        code, msg64, err = c.cmd(0, string(resp64));
    }
    return error.As(err)!;
}

// Mail issues a MAIL command to the server using the provided email address.
// If the server supports the 8BITMIME extension, Mail adds the BODY=8BITMIME
// parameter. If the server supports the SMTPUTF8 extension, Mail adds the
// SMTPUTF8 parameter.
// This initiates a mail transaction and is followed by one or more Rcpt calls.
private static error Mail(this ptr<Client> _addr_c, @string from) {
    ref Client c = ref _addr_c.val;

    {
        var err__prev1 = err;

        var err = validateLine(from);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    {
        var err__prev1 = err;

        err = c.hello();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    @string cmdStr = "MAIL FROM:<%s>";
    if (c.ext != null) {
        {
            var (_, ok) = c.ext["8BITMIME"];

            if (ok) {
                cmdStr += " BODY=8BITMIME";
            }

        }
        {
            (_, ok) = c.ext["SMTPUTF8"];

            if (ok) {
                cmdStr += " SMTPUTF8";
            }

        }
    }
    var (_, _, err) = c.cmd(250, cmdStr, from);
    return error.As(err)!;
}

// Rcpt issues a RCPT command to the server using the provided email address.
// A call to Rcpt must be preceded by a call to Mail and may be followed by
// a Data call or another Rcpt call.
private static error Rcpt(this ptr<Client> _addr_c, @string to) {
    ref Client c = ref _addr_c.val;

    {
        var err = validateLine(to);

        if (err != null) {
            return error.As(err)!;
        }
    }
    var (_, _, err) = c.cmd(25, "RCPT TO:<%s>", to);
    return error.As(err)!;
}

private partial struct dataCloser : io.WriteCloser {
    public ptr<Client> c;
    public ref io.WriteCloser WriteCloser => ref WriteCloser_val;
}

private static error Close(this ptr<dataCloser> _addr_d) {
    ref dataCloser d = ref _addr_d.val;

    d.WriteCloser.Close();
    var (_, _, err) = d.c.Text.ReadResponse(250);
    return error.As(err)!;
}

// Data issues a DATA command to the server and returns a writer that
// can be used to write the mail headers and body. The caller should
// close the writer before calling any more methods on c. A call to
// Data must be preceded by one or more calls to Rcpt.
private static (io.WriteCloser, error) Data(this ptr<Client> _addr_c) {
    io.WriteCloser _p0 = default;
    error _p0 = default!;
    ref Client c = ref _addr_c.val;

    var (_, _, err) = c.cmd(354, "DATA");
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (addr(new dataCloser(c,c.Text.DotWriter())), error.As(null!)!);
}

private static Action<ptr<tls.Config>> testHookStartTLS = default; // nil, except for tests

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
public static error SendMail(@string addr, Auth a, @string from, slice<@string> to, slice<byte> msg) => func((defer, _, _) => {
    {
        var err__prev1 = err;

        var err = validateLine(from);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }
    foreach (var (_, recp) in to) {
        {
            var err__prev1 = err;

            err = validateLine(recp);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }
    }    var (c, err) = Dial(addr);
    if (err != null) {
        return error.As(err)!;
    }
    defer(c.Close());
    err = c.hello();

    if (err != null) {
        return error.As(err)!;
    }
    {
        var (ok, _) = c.Extension("STARTTLS");

        if (ok) {
            ptr<tls.Config> config = addr(new tls.Config(ServerName:c.serverName));
            if (testHookStartTLS != null) {
                testHookStartTLS(config);
            }
            err = c.StartTLS(config);

            if (err != null) {
                return error.As(err)!;
            }
        }
    }
    if (a != null && c.ext != null) {
        {
            var (_, ok) = c.ext["AUTH"];

            if (!ok) {
                return error.As(errors.New("smtp: server doesn't support AUTH"))!;
            }

        }
        err = c.Auth(a);

        if (err != null) {
            return error.As(err)!;
        }
    }
    err = c.Mail(from);

    if (err != null) {
        return error.As(err)!;
    }
    foreach (var (_, addr) in to) {
        err = c.Rcpt(addr);

        if (err != null) {
            return error.As(err)!;
        }
    }    var (w, err) = c.Data();
    if (err != null) {
        return error.As(err)!;
    }
    _, err = w.Write(msg);
    if (err != null) {
        return error.As(err)!;
    }
    err = w.Close();
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(c.Quit())!;
});

// Extension reports whether an extension is support by the server.
// The extension name is case-insensitive. If the extension is supported,
// Extension also returns a string that contains any parameters the
// server specifies for the extension.
private static (bool, @string) Extension(this ptr<Client> _addr_c, @string ext) {
    bool _p0 = default;
    @string _p0 = default;
    ref Client c = ref _addr_c.val;

    {
        var err = c.hello();

        if (err != null) {
            return (false, "");
        }
    }
    if (c.ext == null) {
        return (false, "");
    }
    ext = strings.ToUpper(ext);
    var (param, ok) = c.ext[ext];
    return (ok, param);
}

// Reset sends the RSET command to the server, aborting the current mail
// transaction.
private static error Reset(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    {
        var err = c.hello();

        if (err != null) {
            return error.As(err)!;
        }
    }
    var (_, _, err) = c.cmd(250, "RSET");
    return error.As(err)!;
}

// Noop sends the NOOP command to the server. It does nothing but check
// that the connection to the server is okay.
private static error Noop(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    {
        var err = c.hello();

        if (err != null) {
            return error.As(err)!;
        }
    }
    var (_, _, err) = c.cmd(250, "NOOP");
    return error.As(err)!;
}

// Quit sends the QUIT command and closes the connection to the server.
private static error Quit(this ptr<Client> _addr_c) {
    ref Client c = ref _addr_c.val;

    {
        var err = c.hello();

        if (err != null) {
            return error.As(err)!;
        }
    }
    var (_, _, err) = c.cmd(221, "QUIT");
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(c.Text.Close())!;
}

// validateLine checks to see if a line has CR or LF as per RFC 5321
private static error validateLine(@string line) {
    if (strings.ContainsAny(line, "\n\r")) {
        return error.As(errors.New("smtp: A line must not contain CR or LF"))!;
    }
    return error.As(null!)!;
}

} // end smtp_package
