// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.rpc;

using json = encoding.json_package;
using errors = errors_package;
using io = io_package;
using rpc = net.rpc_package;
using sync = sync_package;
using encoding;
using net;

partial class jsonrpc_package {

internal static error errMissingParams = errors.New("jsonrpc: request body missing params"u8);

[GoType] partial struct serverCodec {
    internal ж<encoding.json_package.Decoder> dec; // for reading JSON values
    internal ж<encoding.json_package.Encoder> enc; // for writing JSON values
    internal io_package.Closer c;
    // temporary work space
    internal serverRequest req;
    // JSON-RPC clients can use arbitrary json values as request IDs.
    // Package rpc expects uint64 request IDs.
    // We assign uint64 sequence numbers to incoming requests
    // but save the original request ID in the pending map.
    // When rpc responds, we use the sequence number in
    // the response to find the original request ID.
    internal sync_package.Mutex mutex; // protects seq, pending
    internal uint64 seq;
    internal json.RawMessage pending;
}

// NewServerCodec returns a new [rpc.ServerCodec] using JSON-RPC on conn.
public static rpc.ServerCodec NewServerCodec(io.ReadWriteCloser conn) {
    return new serverCodec(
        dec: json.NewDecoder(conn),
        enc: json.NewEncoder(conn),
        c: conn,
        pending: new json.RawMessage()
    );
}

[GoType] partial struct serverRequest {
    [GoTag(@"json:""method""")]
    public @string Method;
    [GoTag(@"json:""params""")]
    public ж<encoding.json_package.RawMessage> Params;
    [GoTag(@"json:""id""")]
    public ж<encoding.json_package.RawMessage> Id;
}

[GoRecv] internal static void reset(this ref serverRequest r) {
    r.Method = ""u8;
    r.Params = default!;
    r.Id = default!;
}

[GoType] partial struct serverResponse {
    [GoTag(@"json:""id""")]
    public ж<encoding.json_package.RawMessage> Id;
    [GoTag(@"json:""result""")]
    public any Result;
    [GoTag(@"json:""error""")]
    public any Error;
}

[GoRecv] internal static error ReadRequestHeader(this ref serverCodec c, ж<rpc.Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    c.req.reset();
    {
        var err = c.dec.Decode(Ꮡ(c.req)); if (err != default!) {
            return err;
        }
    }
    r.ServiceMethod = c.req.Method;
    // JSON request id can be any JSON value;
    // RPC package expects uint64.  Translate to
    // internal uint64 and save JSON on the side.
    c.mutex.Lock();
    c.seq++;
    c.pending[c.seq] = c.req.Id;
    c.req.Id = default!;
    r.Seq = c.seq;
    c.mutex.Unlock();
    return default!;
}

[GoRecv] internal static error ReadRequestBody(this ref serverCodec c, any x) {
    if (x == default!) {
        return default!;
    }
    if (c.req.Params == nil) {
        return errMissingParams;
    }
    // JSON params is array value.
    // RPC params is struct.
    // Unmarshal into array containing struct for now.
    // Should think about making RPC more general.
    ref var params = ref heap(new array<any>(1), out var Ꮡparams);
    @params[0] = x;
    return json.Unmarshal(c.req.Params.val, Ꮡ@params);
}

public static json.RawMessage @null = ((json.RawMessage)slice<byte>("null"));

[GoRecv] internal static error WriteResponse(this ref serverCodec c, ж<rpc.Response> Ꮡr, any x) {
    ref var r = ref Ꮡr.val;

    c.mutex.Lock();
    var b = c.pending[r.Seq];
    var ok = c.pending[r.Seq];
    if (!ok) {
        c.mutex.Unlock();
        return errors.New("invalid sequence number in response"u8);
    }
    delete(c.pending, r.Seq);
    c.mutex.Unlock();
    if (b == nil) {
        // Invalid request so no id. Use JSON null.
        b = Ꮡ(@null);
    }
    var resp = new serverResponse(Id: b);
    if (r.Error == ""u8){
        resp.Result = x;
    } else {
        resp.Error = r.Error;
    }
    return c.enc.Encode(resp);
}

[GoRecv] internal static error Close(this ref serverCodec c) {
    return c.c.Close();
}

// ServeConn runs the JSON-RPC server on a single connection.
// ServeConn blocks, serving the connection until the client hangs up.
// The caller typically invokes ServeConn in a go statement.
public static void ServeConn(io.ReadWriteCloser conn) {
    rpc.ServeCodec(NewServerCodec(conn));
}

} // end jsonrpc_package
