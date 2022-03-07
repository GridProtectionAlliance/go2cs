// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package jsonrpc -- go2cs converted at 2022 March 06 22:25:55 UTC
// import "net/rpc/jsonrpc" ==> using jsonrpc = go.net.rpc.jsonrpc_package
// Original source: C:\Program Files\Go\src\net\rpc\jsonrpc\server.go
using json = go.encoding.json_package;
using errors = go.errors_package;
using io = go.io_package;
using rpc = go.net.rpc_package;
using sync = go.sync_package;
using System.ComponentModel;


namespace go.net.rpc;

public static partial class jsonrpc_package {

private static var errMissingParams = errors.New("jsonrpc: request body missing params");

private partial struct serverCodec {
    public ptr<json.Decoder> dec; // for reading JSON values
    public ptr<json.Encoder> enc; // for writing JSON values
    public io.Closer c; // temporary work space
    public serverRequest req; // JSON-RPC clients can use arbitrary json values as request IDs.
// Package rpc expects uint64 request IDs.
// We assign uint64 sequence numbers to incoming requests
// but save the original request ID in the pending map.
// When rpc responds, we use the sequence number in
// the response to find the original request ID.
    public sync.Mutex mutex; // protects seq, pending
    public ulong seq;
    public map<ulong, ptr<json.RawMessage>> pending;
}

// NewServerCodec returns a new rpc.ServerCodec using JSON-RPC on conn.
public static rpc.ServerCodec NewServerCodec(io.ReadWriteCloser conn) {
    return addr(new serverCodec(dec:json.NewDecoder(conn),enc:json.NewEncoder(conn),c:conn,pending:make(map[uint64]*json.RawMessage),));
}

private partial struct serverRequest {
    [Description("json:\"method\"")]
    public @string Method;
    [Description("json:\"params\"")]
    public ptr<json.RawMessage> Params;
    [Description("json:\"id\"")]
    public ptr<json.RawMessage> Id;
}

private static void reset(this ptr<serverRequest> _addr_r) {
    ref serverRequest r = ref _addr_r.val;

    r.Method = "";
    r.Params = null;
    r.Id = null;
}

private partial struct serverResponse {
    [Description("json:\"id\"")]
    public ptr<json.RawMessage> Id;
}

private static error ReadRequestHeader(this ptr<serverCodec> _addr_c, ptr<rpc.Request> _addr_r) {
    ref serverCodec c = ref _addr_c.val;
    ref rpc.Request r = ref _addr_r.val;

    c.req.reset();
    {
        var err = c.dec.Decode(_addr_c.req);

        if (err != null) {
            return error.As(err)!;
        }
    }

    r.ServiceMethod = c.req.Method; 

    // JSON request id can be any JSON value;
    // RPC package expects uint64.  Translate to
    // internal uint64 and save JSON on the side.
    c.mutex.Lock();
    c.seq++;
    c.pending[c.seq] = c.req.Id;
    c.req.Id = null;
    r.Seq = c.seq;
    c.mutex.Unlock();

    return error.As(null!)!;

}

private static error ReadRequestBody(this ptr<serverCodec> _addr_c, object x) {
    ref serverCodec c = ref _addr_c.val;

    if (x == null) {
        return error.As(null!)!;
    }
    if (c.req.Params == null) {
        return error.As(errMissingParams)!;
    }
    ref var @params = ref heap(out ptr<var> _addr_@params);
    params[0] = x;
    return error.As(json.Unmarshal(c.req.Params.val, _addr_params))!;

}

private static var @null = json.RawMessage((slice<byte>)"null");

private static error WriteResponse(this ptr<serverCodec> _addr_c, ptr<rpc.Response> _addr_r, object x) {
    ref serverCodec c = ref _addr_c.val;
    ref rpc.Response r = ref _addr_r.val;

    c.mutex.Lock();
    var (b, ok) = c.pending[r.Seq];
    if (!ok) {
        c.mutex.Unlock();
        return error.As(errors.New("invalid sequence number in response"))!;
    }
    delete(c.pending, r.Seq);
    c.mutex.Unlock();

    if (b == null) { 
        // Invalid request so no id. Use JSON null.
        b = _addr_null;

    }
    serverResponse resp = new serverResponse(Id:b);
    if (r.Error == "") {
        resp.Result = x;
    }
    else
 {
        resp.Error = r.Error;
    }
    return error.As(c.enc.Encode(resp))!;

}

private static error Close(this ptr<serverCodec> _addr_c) {
    ref serverCodec c = ref _addr_c.val;

    return error.As(c.c.Close())!;
}

// ServeConn runs the JSON-RPC server on a single connection.
// ServeConn blocks, serving the connection until the client hangs up.
// The caller typically invokes ServeConn in a go statement.
public static void ServeConn(io.ReadWriteCloser conn) {
    rpc.ServeCodec(NewServerCodec(conn));
}

} // end jsonrpc_package
