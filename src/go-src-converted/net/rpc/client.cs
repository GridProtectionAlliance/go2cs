// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using bufio = bufio_package;
using gob = encoding.gob_package;
using errors = errors_package;
using io = io_package;
using log = log_package;
using net = net_package;
using http = net.http_package;
using sync = sync_package;
using encoding;

partial class rpc_package {

[GoType("@string")] partial struct ServerError;

public static @string Error(this ServerError e) {
    return ((@string)e);
}

public static error ErrShutdown = errors.New("connection is shut down"u8);

// Call represents an active RPC.
[GoType] partial struct ΔCall {
    public @string ServiceMethod;    // The name of the service and method to call.
    public any Args;        // The argument to the function (*struct).
    public any Reply;        // The reply from the function (*struct).
    public error Error;      // After completion, the error status.
    public channel<ж<ΔCall>> Done; // Receives *Call when Go is complete.
}

// Client represents an RPC Client.
// There may be multiple outstanding Calls associated
// with a single Client, and a Client may be used by
// multiple goroutines simultaneously.
[GoType] partial struct Client {
    internal ClientCodec codec;
    internal sync_package.Mutex reqMutex; // protects following
    internal Request request;
    internal sync_package.Mutex mutex; // protects following
    internal uint64 seq;
    internal map<uint64, ж<ΔCall>> pending;
    internal bool closing; // user has called Close
    internal bool shutdown; // server has told us to stop
}

// A ClientCodec implements writing of RPC requests and
// reading of RPC responses for the client side of an RPC session.
// The client calls [ClientCodec.WriteRequest] to write a request to the connection
// and calls [ClientCodec.ReadResponseHeader] and [ClientCodec.ReadResponseBody] in pairs
// to read responses. The client calls [ClientCodec.Close] when finished with the
// connection. ReadResponseBody may be called with a nil
// argument to force the body of the response to be read and then
// discarded.
// See [NewClient]'s comment for information about concurrent access.
[GoType] partial interface ClientCodec {
    error WriteRequest(ж<Request> _, any _);
    error ReadResponseHeader(ж<Response> _);
    error ReadResponseBody(any _);
    error Close();
}

[GoRecv] public static void send(this ref Client client, ж<ΔCall> Ꮡcall) => func((defer, _) => {
    ref var call = ref Ꮡcall.val;

    client.reqMutex.Lock();
    defer(client.reqMutex.Unlock);
    // Register this call.
    client.mutex.Lock();
    if (client.shutdown || client.closing) {
        client.mutex.Unlock();
        call.Error = ErrShutdown;
        call.done();
        return;
    }
    var seq = client.seq;
    client.seq++;
    client.pending[seq] = call;
    client.mutex.Unlock();
    // Encode and send the request.
    client.request.Seq = seq;
    client.request.ServiceMethod = call.ServiceMethod;
    var err = client.codec.WriteRequest(Ꮡ(client.request), call.Args);
    if (err != default!) {
        client.mutex.Lock();
        call = client.pending[seq];
        delete(client.pending, seq);
        client.mutex.Unlock();
        if (call != nil) {
            call.Error = err;
            call.done();
        }
    }
});

[GoRecv] internal static void input(this ref Client client) {
    error err = default!;
    ref var response = ref heap(new Response(), out var Ꮡresponse);
    while (err == default!) {
        response = new Response(nil);
        err = client.codec.ReadResponseHeader(Ꮡresponse);
        if (err != default!) {
            break;
        }
        var seq = response.Seq;
        client.mutex.Lock();
        var call = client.pending[seq];
        delete(client.pending, seq);
        client.mutex.Unlock();
        switch (ᐧ) {
        case {} when call == nil: {
            err = client.codec.ReadResponseBody(default!);
            if (err != default!) {
                // We've got no pending call. That usually means that
                // WriteRequest partially failed, and call was already
                // removed; response is a server telling us about an
                // error reading request body. We should still attempt
                // to read error body, but there's no one to give it to.
                err = errors.New("reading error body: "u8 + err.Error());
            }
            break;
        }
        case {} when response.Error != ""u8: {
            call.val.Error = ((ServerError)response.Error);
            err = client.codec.ReadResponseBody(default!);
            if (err != default!) {
                // We've got an error response. Give this to the request;
                // any subsequent requests will get the ReadResponseBody
                // error if there is one.
                err = errors.New("reading error body: "u8 + err.Error());
            }
            call.done();
            break;
        }
        default: {
            err = client.codec.ReadResponseBody((~call).Reply);
            if (err != default!) {
                call.val.Error = errors.New("reading body "u8 + err.Error());
            }
            call.done();
            break;
        }}

    }
    // Terminate pending calls.
    client.reqMutex.Lock();
    client.mutex.Lock();
    client.shutdown = true;
    var closing = client.closing;
    if (AreEqual(err, io.EOF)) {
        if (closing){
            err = ErrShutdown;
        } else {
            err = io.ErrUnexpectedEOF;
        }
    }
    foreach (var (_, call) in client.pending) {
        call.val.Error = err;
        call.done();
    }
    client.mutex.Unlock();
    client.reqMutex.Unlock();
    if (debugLog && !AreEqual(err, io.EOF) && !closing) {
        log.Println("rpc: client protocol error:", err);
    }
}

[GoRecv] internal static void done(this ref ΔCall call) {
    switch (ᐧ) {
    case ᐧ: {
        break;
    }
    default: {
        if (debugLog) {
            // ok
            // We don't want to block here. It is the caller's responsibility to make
            // sure the channel has enough buffer space. See comment in Go().
            log.Println("rpc: discarding Call reply due to insufficient Done chan capacity");
        }
        break;
    }}
}

// NewClient returns a new [Client] to handle requests to the
// set of services at the other end of the connection.
// It adds a buffer to the write side of the connection so
// the header and payload are sent as a unit.
//
// The read and write halves of the connection are serialized independently,
// so no interlocking is required. However each half may be accessed
// concurrently so the implementation of conn should protect against
// concurrent reads or concurrent writes.
public static ж<Client> NewClient(io.ReadWriteCloser conn) {
    var encBuf = bufio.NewWriter(conn);
    var client = Ꮡ(new gobClientCodec(conn, gob.NewDecoder(conn), gob.NewEncoder(~encBuf), encBuf));
    return NewClientWithCodec(~client);
}

// NewClientWithCodec is like [NewClient] but uses the specified
// codec to encode requests and decode responses.
public static ж<Client> NewClientWithCodec(ClientCodec codec) {
    var client = Ꮡ(new Client(
        codec: codec,
        pending: new map<uint64, ж<ΔCall>>()
    ));
    var clientʗ1 = client;
    goǃ(clientʗ1.input);
    return client;
}

[GoType] partial struct gobClientCodec {
    internal io_package.ReadWriteCloser rwc;
    internal ж<encoding.gob_package.Decoder> dec;
    internal ж<encoding.gob_package.Encoder> enc;
    internal ж<bufio_package.Writer> encBuf;
}

[GoRecv] internal static error /*err*/ WriteRequest(this ref gobClientCodec c, ж<Request> Ꮡr, any body) {
    error err = default!;

    ref var r = ref Ꮡr.val;
    {
        err = c.enc.Encode(r); if (err != default!) {
            return err;
        }
    }
    {
        err = c.enc.Encode(body); if (err != default!) {
            return err;
        }
    }
    return c.encBuf.Flush();
}

[GoRecv] internal static error ReadResponseHeader(this ref gobClientCodec c, ж<Response> Ꮡr) {
    ref var r = ref Ꮡr.val;

    return c.dec.Decode(r);
}

[GoRecv] internal static error ReadResponseBody(this ref gobClientCodec c, any body) {
    return c.dec.Decode(body);
}

[GoRecv] internal static error Close(this ref gobClientCodec c) {
    return c.rwc.Close();
}

// DialHTTP connects to an HTTP RPC server at the specified network address
// listening on the default HTTP RPC path.
public static (ж<Client>, error) DialHTTP(@string network, @string address) {
    return DialHTTPPath(network, address, DefaultRPCPath);
}

// DialHTTPPath connects to an HTTP RPC server
// at the specified network address and path.
public static (ж<Client>, error) DialHTTPPath(@string network, @string address, @string path) {
    (conn, err) = net.Dial(network, address);
    if (err != default!) {
        return (default!, err);
    }
    io.WriteString(conn, "CONNECT "u8 + path + " HTTP/1.0\n\n"u8);
    // Require successful HTTP response
    // before switching to RPC protocol.
    (resp, err) = http.ReadResponse(bufio.NewReader(conn), Ꮡ(new http.Request(Method: "CONNECT"u8)));
    if (err == default! && (~resp).Status == connected) {
        return (NewClient(conn), default!);
    }
    if (err == default!) {
        err = errors.New("unexpected HTTP response: "u8 + (~resp).Status);
    }
    conn.Close();
    return (default!, new net.OpError(
        Op: "dial-http"u8,
        Net: network + " "u8 + address,
        Addr: default!,
        Err: err
    ));
}

// Dial connects to an RPC server at the specified network address.
public static (ж<Client>, error) Dial(@string network, @string address) {
    (conn, err) = net.Dial(network, address);
    if (err != default!) {
        return (default!, err);
    }
    return (NewClient(conn), default!);
}

// Close calls the underlying codec's Close method. If the connection is already
// shutting down, [ErrShutdown] is returned.
[GoRecv] public static error Close(this ref Client client) {
    client.mutex.Lock();
    if (client.closing) {
        client.mutex.Unlock();
        return ErrShutdown;
    }
    client.closing = true;
    client.mutex.Unlock();
    return client.codec.Close();
}

// Go invokes the function asynchronously. It returns the [Call] structure representing
// the invocation. The done channel will signal when the call is complete by returning
// the same Call object. If done is nil, Go will allocate a new channel.
// If non-nil, done must be buffered or Go will deliberately crash.
[GoRecv] public static ж<ΔCall> Go(this ref Client client, @string serviceMethod, any args, any reply, channel<ж<ΔCall>> done) {
    var call = @new<ΔCall>();
    call.val.ServiceMethod = serviceMethod;
    call.val.Args = args;
    call.val.Reply = reply;
    if (done == default!){
        done = new channel<ж<ΔCall>>(10);
    } else {
        // buffered.
        // If caller passes done != nil, it must arrange that
        // done has enough buffer for the number of simultaneous
        // RPCs that will be using that channel. If the channel
        // is totally unbuffered, it's best not to run at all.
        if (cap(done) == 0) {
            log.Panic("rpc: done channel is unbuffered");
        }
    }
    call.val.Done = done;
    client.send(call);
    return call;
}

// Call invokes the named function, waits for it to complete, and returns its error status.
[GoRecv] public static error Call(this ref Client client, @string serviceMethod, any args, any reply) {
    var call = ᐸꟷ(client.Go(serviceMethod, args, reply, new channel<ж<ΔCall>>(1)).Done);
    return (~call).Error;
}

} // end rpc_package
