// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package jsonrpc implements a JSON-RPC 1.0 ClientCodec and ServerCodec
// for the rpc package.
// For JSON-RPC 2.0 support, see https://godoc.org/?q=json-rpc+2.0
namespace go.net.rpc;

using json = encoding.json_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using rpc = net.rpc_package;
using sync = sync_package;
using encoding;
using net;

partial class jsonrpc_package {

[GoType] partial struct clientCodec {
    internal ж<encoding.json_package.Decoder> dec; // for reading JSON values
    internal ж<encoding.json_package.Encoder> enc; // for writing JSON values
    internal io_package.Closer c;
    // temporary work space
    internal clientRequest req;
    internal clientResponse resp;
    // JSON-RPC responses include the request id but not the request method.
    // Package rpc expects both.
    // We save the request method in pending when sending a request
    // and then look it up by request ID when filling out the rpc Response.
    internal sync_package.Mutex mutex;        // protects pending
    internal map<uint64, @string> pending; // map request id to method name
}

// NewClientCodec returns a new [rpc.ClientCodec] using JSON-RPC on conn.
public static rpc.ClientCodec NewClientCodec(io.ReadWriteCloser conn) {
    return new clientCodec(
        dec: json.NewDecoder(conn),
        enc: json.NewEncoder(conn),
        c: conn,
        pending: new map<uint64, @string>()
    );
}

[GoType] partial struct clientRequest {
    [GoTag(@"json:""method""")]
    public @string Method;
    [GoTag(@"json:""params""")]
    public array<any> Params = new(1);
    [GoTag(@"json:""id""")]
    public uint64 Id;
}

[GoRecv] internal static error WriteRequest(this ref clientCodec c, ж<rpc.Request> Ꮡr, any param) {
    ref var r = ref Ꮡr.val;

    c.mutex.Lock();
    c.pending[r.Seq] = r.ServiceMethod;
    c.mutex.Unlock();
    c.req.Method = r.ServiceMethod;
    c.req.Params[0] = param;
    c.req.Id = r.Seq;
    return c.enc.Encode(Ꮡ(c.req));
}

[GoType] partial struct clientResponse {
    [GoTag(@"json:""id""")]
    public uint64 Id;
    [GoTag(@"json:""result""")]
    public ж<encoding.json_package.RawMessage> Result;
    [GoTag(@"json:""error""")]
    public any Error;
}

[GoRecv] internal static void reset(this ref clientResponse r) {
    r.Id = 0;
    r.Result = default!;
    r.Error = default!;
}

[GoRecv] internal static error ReadResponseHeader(this ref clientCodec c, ж<rpc.Response> Ꮡr) {
    ref var r = ref Ꮡr.val;

    c.resp.reset();
    {
        var err = c.dec.Decode(Ꮡ(c.resp)); if (err != default!) {
            return err;
        }
    }
    c.mutex.Lock();
    r.ServiceMethod = c.pending[c.resp.Id];
    delete(c.pending, c.resp.Id);
    c.mutex.Unlock();
    r.Error = ""u8;
    r.Seq = c.resp.Id;
    if (c.resp.Error != default! || c.resp.Result == nil) {
        var (x, ok) = c.resp.Error._<@string>(ᐧ);
        if (!ok) {
            return fmt.Errorf("invalid error %v"u8, c.resp.Error);
        }
        if (x == ""u8) {
            x = "unspecified error"u8;
        }
        r.Error = x;
    }
    return default!;
}

[GoRecv] internal static error ReadResponseBody(this ref clientCodec c, any x) {
    if (x == default!) {
        return default!;
    }
    return json.Unmarshal(c.resp.Result.val, x);
}

[GoRecv] internal static error Close(this ref clientCodec c) {
    return c.c.Close();
}

// NewClient returns a new [rpc.Client] to handle requests to the
// set of services at the other end of the connection.
public static ж<rpc.Client> NewClient(io.ReadWriteCloser conn) {
    return rpc.NewClientWithCodec(NewClientCodec(conn));
}

// Dial connects to a JSON-RPC server at the specified network address.
public static (ж<rpc.Client>, error) Dial(@string network, @string address) {
    (conn, err) = net.Dial(network, address);
    if (err != default!) {
        return (default!, err);
    }
    return (NewClient(conn), err);
}

} // end jsonrpc_package
