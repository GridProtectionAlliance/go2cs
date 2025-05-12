// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;

partial class tls_package {

[GoType("num:nint")] partial struct QUICEncryptionLevel;

public static readonly QUICEncryptionLevel QUICEncryptionLevelInitial = /* QUICEncryptionLevel(iota) */ 0;
public static readonly QUICEncryptionLevel QUICEncryptionLevelEarly = 1;
public static readonly QUICEncryptionLevel QUICEncryptionLevelHandshake = 2;
public static readonly QUICEncryptionLevel QUICEncryptionLevelApplication = 3;

public static @string String(this QUICEncryptionLevel l) {
    var exprᴛ1 = l;
    if (exprᴛ1 == QUICEncryptionLevelInitial) {
        return "Initial"u8;
    }
    if (exprᴛ1 == QUICEncryptionLevelEarly) {
        return "Early"u8;
    }
    if (exprᴛ1 == QUICEncryptionLevelHandshake) {
        return "Handshake"u8;
    }
    if (exprᴛ1 == QUICEncryptionLevelApplication) {
        return "Application"u8;
    }
    { /* default: */
        return fmt.Sprintf("QUICEncryptionLevel(%v)"u8, ((nint)l));
    }

}

// A QUICConn represents a connection which uses a QUIC implementation as the underlying
// transport as described in RFC 9001.
//
// Methods of QUICConn are not safe for concurrent use.
[GoType] partial struct QUICConn {
    internal ж<Conn> conn;
    internal bool sessionTicketSent;
}

// A QUICConfig configures a [QUICConn].
[GoType] partial struct QUICConfig {
    public ж<Config> TLSConfig;
    // EnableSessionEvents may be set to true to enable the
    // [QUICStoreSession] and [QUICResumeSession] events for client connections.
    // When this event is enabled, sessions are not automatically
    // stored in the client session cache.
    // The application should use [QUICConn.StoreSession] to store sessions.
    public bool EnableSessionEvents;
}

[GoType("num:nint")] partial struct QUICEventKind;

public static readonly QUICEventKind QUICNoEvent = /* iota */ 0;
public static readonly QUICEventKind QUICSetReadSecret = 1;
public static readonly QUICEventKind QUICSetWriteSecret = 2;
public static readonly QUICEventKind QUICWriteData = 3;
public static readonly QUICEventKind QUICTransportParameters = 4;
public static readonly QUICEventKind QUICTransportParametersRequired = 5;
public static readonly QUICEventKind QUICRejectedEarlyData = 6;
public static readonly QUICEventKind QUICHandshakeDone = 7;
public static readonly QUICEventKind QUICResumeSession = 8;
public static readonly QUICEventKind QUICStoreSession = 9;

// A QUICEvent is an event occurring on a QUIC connection.
//
// The type of event is specified by the Kind field.
// The contents of the other fields are kind-specific.
[GoType] partial struct QUICEvent {
    public QUICEventKind Kind;
    // Set for QUICSetReadSecret, QUICSetWriteSecret, and QUICWriteData.
    public QUICEncryptionLevel Level;
    // Set for QUICTransportParameters, QUICSetReadSecret, QUICSetWriteSecret, and QUICWriteData.
    // The contents are owned by crypto/tls, and are valid until the next NextEvent call.
    public slice<byte> Data;
    // Set for QUICSetReadSecret and QUICSetWriteSecret.
    public uint16 Suite;
    // Set for QUICResumeSession and QUICStoreSession.
    public ж<SessionState> SessionState;
}

[GoType] partial struct quicState {
    internal slice<QUICEvent> events;
    internal nint nextEvent;
    // eventArr is a statically allocated event array, large enough to handle
    // the usual maximum number of events resulting from a single call: transport
    // parameters, Initial data, Early read secret, Handshake write and read
    // secrets, Handshake data, Application write secret, Application data.
    internal array<QUICEvent> eventArr = new(8);
    internal bool started;
    internal channel<EmptyStruct> signalc; // handshake data is available to be read
    internal channel<EmptyStruct> blockedc; // handshake is waiting for data, closed when done
    internal /*<-*/channel<EmptyStruct> cancelc; // handshake has been canceled
    internal context_package.CancelFunc cancel;
    internal bool waitingForDrain;
    // readbuf is shared between HandleData and the handshake goroutine.
    // HandshakeCryptoData passes ownership to the handshake goroutine by
    // reading from signalc, and reclaims ownership by reading from blockedc.
    internal slice<byte> readbuf;
    internal slice<byte> transportParams; // to send to the peer
    internal bool enableSessionEvents;
}

// QUICClient returns a new TLS client side connection using QUICTransport as the
// underlying transport. The config cannot be nil.
//
// The config's MinVersion must be at least TLS 1.3.
public static ж<QUICConn> QUICClient(ж<QUICConfig> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    return newQUICConn(Client(default!, config.TLSConfig), Ꮡconfig);
}

// QUICServer returns a new TLS server side connection using QUICTransport as the
// underlying transport. The config cannot be nil.
//
// The config's MinVersion must be at least TLS 1.3.
public static ж<QUICConn> QUICServer(ж<QUICConfig> Ꮡconfig) {
    ref var config = ref Ꮡconfig.val;

    return newQUICConn(Server(default!, config.TLSConfig), Ꮡconfig);
}

internal static ж<QUICConn> newQUICConn(ж<Conn> Ꮡconn, ж<QUICConfig> Ꮡconfig) {
    ref var conn = ref Ꮡconn.val;
    ref var config = ref Ꮡconfig.val;

    conn.quic = Ꮡ(new quicState(
        signalc: new channel<EmptyStruct>(1),
        blockedc: new channel<EmptyStruct>(1),
        enableSessionEvents: config.EnableSessionEvents
    ));
    conn.quic.events = conn.quic.eventArr[..0];
    return Ꮡ(new QUICConn(
        conn: conn
    ));
}

// Start starts the client or server handshake protocol.
// It may produce connection events, which may be read with [QUICConn.NextEvent].
//
// Start must be called at most once.
[GoRecv] public static error Start(this ref QUICConn q, context.Context ctx) {
    if (q.conn.quic.started) {
        return quicError(errors.New("tls: Start called more than once"u8));
    }
    q.conn.quic.started = true;
    if (q.conn.config.MinVersion < VersionTLS13) {
        return quicError(errors.New("tls: Config MinVersion must be at least TLS 1.13"u8));
    }
    goǃ(q.conn.HandshakeContext, ctx);
    {
        var (_, ok) = ᐸꟷ(q.conn.quic.blockedc, ꟷ); if (!ok) {
            return q.conn.handshakeErr;
        }
    }
    return default!;
}

// NextEvent returns the next event occurring on the connection.
// It returns an event with a Kind of [QUICNoEvent] when no events are available.
[GoRecv] public static QUICEvent NextEvent(this ref QUICConn q) {
    var qs = q.conn.quic;
    {
        nint last = (~qs).nextEvent - 1; if (last >= 0 && len((~qs).events[last].Data) > 0) {
            // Write over some of the previous event's data,
            // to catch callers erroniously retaining it.
            (~qs).events[last].Data[0] = 0;
        }
    }
    if ((~qs).nextEvent >= len((~qs).events) && (~qs).waitingForDrain) {
        qs.val.waitingForDrain = false;
        ᐸꟷ((~qs).signalc);
        ᐸꟷ((~qs).blockedc);
    }
    if ((~qs).nextEvent >= len((~qs).events)) {
        qs.val.events = (~qs).events[..0];
        qs.val.nextEvent = 0;
        return new QUICEvent(Kind: QUICNoEvent);
    }
    var e = (~qs).events[(~qs).nextEvent];
    (~qs).events[(~qs).nextEvent] = new QUICEvent(nil);
    // zero out references to data
    (~qs).nextEvent++;
    return e;
}

// Close closes the connection and stops any in-progress handshake.
[GoRecv] public static error Close(this ref QUICConn q) {
    if (q.conn.quic.cancel == default!) {
        return default!;
    }
    // never started
    q.conn.quic.cancel();
    foreach (var _ in q.conn.quic.blockedc) {
    }
    // Wait for the handshake goroutine to return.
    return q.conn.handshakeErr;
}

// HandleData handles handshake bytes received from the peer.
// It may produce connection events, which may be read with [QUICConn.NextEvent].
[GoRecv] public static error HandleData(this ref QUICConn q, QUICEncryptionLevel level, slice<byte> data) => func((defer, _) => {
    var c = q.conn;
    if ((~c).@in.level != level) {
        return quicError((~c).@in.setErrorLocked(errors.New("tls: handshake data received at wrong level"u8)));
    }
    (~c).quic.val.readbuf = data;
    ᐸꟷ((~(~c).quic).signalc);
    var (_, ok) = ᐸꟷ((~(~c).quic).blockedc, ꟷ);
    if (ok) {
        // The handshake goroutine is waiting for more data.
        return default!;
    }
    // The handshake goroutine has exited.
    (~c).handshakeMutex.Lock();
    var cʗ1 = c;
    defer((~cʗ1).handshakeMutex.Unlock);
    (~c).hand.Write((~(~c).quic).readbuf);
    (~c).quic.val.readbuf = default!;
    while (q.conn.hand.Len() >= 4 && q.conn.handshakeErr == default!) {
        var b = q.conn.hand.Bytes();
        nint n = (nint)((nint)(((nint)b[1]) << (int)(16) | ((nint)b[2]) << (int)(8)) | ((nint)b[3]));
        if (n > maxHandshake) {
            q.conn.handshakeErr = fmt.Errorf("tls: handshake message of length %d bytes exceeds maximum of %d bytes"u8, n, maxHandshake);
            break;
        }
        if (len(b) < 4 + n) {
            return default!;
        }
        {
            var err = q.conn.handlePostHandshakeMessage(); if (err != default!) {
                q.conn.handshakeErr = err;
            }
        }
    }
    if (q.conn.handshakeErr != default!) {
        return quicError(q.conn.handshakeErr);
    }
    return default!;
});

[GoType] partial struct QUICSessionTicketOptions {
    // EarlyData specifies whether the ticket may be used for 0-RTT.
    public bool EarlyData;
    public slice<slice<byte>> Extra;
}

// SendSessionTicket sends a session ticket to the client.
// It produces connection events, which may be read with [QUICConn.NextEvent].
// Currently, it can only be called once.
[GoRecv] public static error SendSessionTicket(this ref QUICConn q, QUICSessionTicketOptions opts) {
    var c = q.conn;
    if (!(~c).isHandshakeComplete.Load()) {
        return quicError(errors.New("tls: SendSessionTicket called before handshake completed"u8));
    }
    if ((~c).isClient) {
        return quicError(errors.New("tls: SendSessionTicket called on the client"u8));
    }
    if (q.sessionTicketSent) {
        return quicError(errors.New("tls: SendSessionTicket called multiple times"u8));
    }
    q.sessionTicketSent = true;
    return quicError(c.sendSessionTicket(opts.EarlyData, opts.Extra));
}

// StoreSession stores a session previously received in a QUICStoreSession event
// in the ClientSessionCache.
// The application may process additional events or modify the SessionState
// before storing the session.
[GoRecv] public static error StoreSession(this ref QUICConn q, ж<SessionState> Ꮡsession) {
    ref var session = ref Ꮡsession.val;

    var c = q.conn;
    if (!(~c).isClient) {
        return quicError(errors.New("tls: StoreSessionTicket called on the server"u8));
    }
    @string cacheKey = c.clientSessionCacheKey();
    if (cacheKey == ""u8) {
        return default!;
    }
    var cs = Ꮡ(new ClientSessionState(session: session));
    (~(~c).config).ClientSessionCache.Put(cacheKey, cs);
    return default!;
}

// ConnectionState returns basic TLS details about the connection.
[GoRecv] public static ΔConnectionState ConnectionState(this ref QUICConn q) {
    return q.conn.ConnectionState();
}

// SetTransportParameters sets the transport parameters to send to the peer.
//
// Server connections may delay setting the transport parameters until after
// receiving the client's transport parameters. See [QUICTransportParametersRequired].
[GoRecv] public static void SetTransportParameters(this ref QUICConn q, slice<byte> @params) {
    if (@params == default!) {
        @params = new byte[]{}.slice();
    }
    q.conn.quic.transportParams = @params;
    if (q.conn.quic.started) {
        ᐸꟷ(q.conn.quic.signalc);
        ᐸꟷ(q.conn.quic.blockedc);
    }
}

// quicError ensures err is an AlertError.
// If err is not already, quicError wraps it with alertInternalError.
internal static error quicError(error err) {
    if (err == default!) {
        return default!;
    }
    ref var ae = ref heap(new AlertError(), out var Ꮡae);
    if (errors.As(err, Ꮡae)) {
        return err;
    }
    ref var a = ref heap(new alert(), out var Ꮡa);
    if (!errors.As(err, Ꮡa)) {
        a = alertInternalError;
    }
    // Return an error wrapping the original error and an AlertError.
    // Truncate the text of the alert to 0 characters.
    return fmt.Errorf("%w%.0w"u8, err, ((AlertError)a));
}

[GoRecv] internal static error quicReadHandshakeBytes(this ref Conn c, nint n) {
    while (c.hand.Len() < n) {
        {
            var err = c.quicWaitForSignal(); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

[GoRecv] internal static void quicSetReadSecret(this ref Conn c, QUICEncryptionLevel level, uint16 suite, slice<byte> secret) {
    c.quic.events = append(c.quic.events, new QUICEvent(
        Kind: QUICSetReadSecret,
        Level: level,
        Suite: suite,
        Data: secret
    ));
}

[GoRecv] internal static void quicSetWriteSecret(this ref Conn c, QUICEncryptionLevel level, uint16 suite, slice<byte> secret) {
    c.quic.events = append(c.quic.events, new QUICEvent(
        Kind: QUICSetWriteSecret,
        Level: level,
        Suite: suite,
        Data: secret
    ));
}

[GoRecv] internal static void quicWriteCryptoData(this ref Conn c, QUICEncryptionLevel level, slice<byte> data) {
    ж<QUICEvent> last = default!;
    if (len(c.quic.events) > 0) {
        last = Ꮡ(c.quic.events[len(c.quic.events) - 1]);
    }
    if (last == nil || (~last).Kind != QUICWriteData || (~last).Level != level) {
        c.quic.events = append(c.quic.events, new QUICEvent(
            Kind: QUICWriteData,
            Level: level
        ));
        last = Ꮡ(c.quic.events[len(c.quic.events) - 1]);
    }
    last.val.Data = append((~last).Data, data.ꓸꓸꓸ);
}

[GoRecv] public static error quicResumeSession(this ref Conn c, ж<SessionState> Ꮡsession) {
    ref var session = ref Ꮡsession.val;

    c.quic.events = append(c.quic.events, new QUICEvent(
        Kind: QUICResumeSession,
        SessionState: session
    ));
    c.quic.waitingForDrain = true;
    while (c.quic.waitingForDrain) {
        {
            var err = c.quicWaitForSignal(); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

[GoRecv] public static void quicStoreSession(this ref Conn c, ж<SessionState> Ꮡsession) {
    ref var session = ref Ꮡsession.val;

    c.quic.events = append(c.quic.events, new QUICEvent(
        Kind: QUICStoreSession,
        SessionState: session
    ));
}

[GoRecv] internal static void quicSetTransportParameters(this ref Conn c, slice<byte> @params) {
    c.quic.events = append(c.quic.events, new QUICEvent(
        Kind: QUICTransportParameters,
        Data: @params
    ));
}

[GoRecv] internal static (slice<byte>, error) quicGetTransportParameters(this ref Conn c) {
    if (c.quic.transportParams == default!) {
        c.quic.events = append(c.quic.events, new QUICEvent(
            Kind: QUICTransportParametersRequired
        ));
    }
    while (c.quic.transportParams == default!) {
        {
            var err = c.quicWaitForSignal(); if (err != default!) {
                return (default!, err);
            }
        }
    }
    return (c.quic.transportParams, default!);
}

[GoRecv] internal static void quicHandshakeComplete(this ref Conn c) {
    c.quic.events = append(c.quic.events, new QUICEvent(
        Kind: QUICHandshakeDone
    ));
}

[GoRecv] internal static void quicRejectedEarlyData(this ref Conn c) {
    c.quic.events = append(c.quic.events, new QUICEvent(
        Kind: QUICRejectedEarlyData
    ));
}

[GoType("dyn")] partial struct quicWaitForSignal_type {
}

[GoType("dyn")] partial struct quicWaitForSignal_typeᴛ1 {
}

// quicWaitForSignal notifies the QUICConn that handshake progress is blocked,
// and waits for a signal that the handshake should proceed.
//
// The handshake may become blocked waiting for handshake bytes
// or for the user to provide transport parameters.
[GoRecv] internal static error quicWaitForSignal(this ref Conn c) => func((defer, _) => {
    // Drop the handshake mutex while blocked to allow the user
    // to call ConnectionState before the handshake completes.
    c.handshakeMutex.Unlock();
    defer(c.handshakeMutex.Lock);
    // Send on blockedc to notify the QUICConn that the handshake is blocked.
    // Exported methods of QUICConn wait for the handshake to become blocked
    // before returning to the user.
    switch (select(c.quic.blockedc.ᐸꟷ(new quicWaitForSignal_type(), ꓸꓸꓸ), ᐸꟷ(c.quic.cancelc, ꓸꓸꓸ))) {
    case 0: {
        break;
    }
    case 1 when c.quic.cancelc.ꟷᐳ(out _): {
        return c.sendAlertLocked(alertCloseNotify);
    }}
    // The QUICConn reads from signalc to notify us that the handshake may
    // be able to proceed. (The QUICConn reads, because we close signalc to
    // indicate that the handshake has completed.)
    switch (select(c.quic.signalc.ᐸꟷ(new quicWaitForSignal_typeᴛ1(), ꓸꓸꓸ), ᐸꟷ(c.quic.cancelc, ꓸꓸꓸ))) {
    case 0: {
        c.hand.Write(c.quic.readbuf);
        c.quic.readbuf = default!;
        break;
    }
    case 1 when c.quic.cancelc.ꟷᐳ(out _): {
        return c.sendAlertLocked(alertCloseNotify);
    }}
    return default!;
});

} // end tls_package
