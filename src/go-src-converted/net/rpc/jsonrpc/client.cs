// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package jsonrpc implements a JSON-RPC 1.0 ClientCodec and ServerCodec
// for the rpc package.
// For JSON-RPC 2.0 support, see https://godoc.org/?q=json-rpc+2.0
// package jsonrpc -- go2cs converted at 2020 October 08 03:43:28 UTC
// import "net/rpc/jsonrpc" ==> using jsonrpc = go.net.rpc.jsonrpc_package
// Original source: C:\Go\src\net\rpc\jsonrpc\client.go
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using io = go.io_package;
using net = go.net_package;
using rpc = go.net.rpc_package;
using sync = go.sync_package;
using static go.builtin;
using System.ComponentModel;

namespace go {
namespace net {
namespace rpc
{
    public static partial class jsonrpc_package
    {
        private partial struct clientCodec
        {
            public ptr<json.Decoder> dec; // for reading JSON values
            public ptr<json.Encoder> enc; // for writing JSON values
            public io.Closer c; // temporary work space
            public clientRequest req;
            public clientResponse resp; // JSON-RPC responses include the request id but not the request method.
// Package rpc expects both.
// We save the request method in pending when sending a request
// and then look it up by request ID when filling out the rpc Response.
            public sync.Mutex mutex; // protects pending
            public map<ulong, @string> pending; // map request id to method name
        }

        // NewClientCodec returns a new rpc.ClientCodec using JSON-RPC on conn.
        public static rpc.ClientCodec NewClientCodec(io.ReadWriteCloser conn)
        {
            return addr(new clientCodec(dec:json.NewDecoder(conn),enc:json.NewEncoder(conn),c:conn,pending:make(map[uint64]string),));
        }

        private partial struct clientRequest
        {
            [Description("json:\"method\"")]
            public @string Method;
            [Description("json:\"id\"")]
            public ulong Id;
        }

        private static error WriteRequest(this ptr<clientCodec> _addr_c, ptr<rpc.Request> _addr_r, object param)
        {
            ref clientCodec c = ref _addr_c.val;
            ref rpc.Request r = ref _addr_r.val;

            c.mutex.Lock();
            c.pending[r.Seq] = r.ServiceMethod;
            c.mutex.Unlock();
            c.req.Method = r.ServiceMethod;
            c.req.Params[0L] = param;
            c.req.Id = r.Seq;
            return error.As(c.enc.Encode(_addr_c.req))!;
        }

        private partial struct clientResponse
        {
            [Description("json:\"id\"")]
            public ulong Id;
            [Description("json:\"result\"")]
            public ptr<json.RawMessage> Result;
        }

        private static void reset(this ptr<clientResponse> _addr_r)
        {
            ref clientResponse r = ref _addr_r.val;

            r.Id = 0L;
            r.Result = null;
            r.Error = null;
        }

        private static error ReadResponseHeader(this ptr<clientCodec> _addr_c, ptr<rpc.Response> _addr_r)
        {
            ref clientCodec c = ref _addr_c.val;
            ref rpc.Response r = ref _addr_r.val;

            c.resp.reset();
            {
                var err = c.dec.Decode(_addr_c.resp);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            c.mutex.Lock();
            r.ServiceMethod = c.pending[c.resp.Id];
            delete(c.pending, c.resp.Id);
            c.mutex.Unlock();

            r.Error = "";
            r.Seq = c.resp.Id;
            if (c.resp.Error != null || c.resp.Result == null)
            {
                @string (x, ok) = c.resp.Error._<@string>();
                if (!ok)
                {
                    return error.As(fmt.Errorf("invalid error %v", c.resp.Error))!;
                }

                if (x == "")
                {
                    x = "unspecified error";
                }

                r.Error = x;

            }

            return error.As(null!)!;

        }

        private static error ReadResponseBody(this ptr<clientCodec> _addr_c, object x)
        {
            ref clientCodec c = ref _addr_c.val;

            if (x == null)
            {
                return error.As(null!)!;
            }

            return error.As(json.Unmarshal(c.resp.Result.val, x))!;

        }

        private static error Close(this ptr<clientCodec> _addr_c)
        {
            ref clientCodec c = ref _addr_c.val;

            return error.As(c.c.Close())!;
        }

        // NewClient returns a new rpc.Client to handle requests to the
        // set of services at the other end of the connection.
        public static ptr<rpc.Client> NewClient(io.ReadWriteCloser conn)
        {
            return _addr_rpc.NewClientWithCodec(NewClientCodec(conn))!;
        }

        // Dial connects to a JSON-RPC server at the specified network address.
        public static (ptr<rpc.Client>, error) Dial(@string network, @string address)
        {
            ptr<rpc.Client> _p0 = default!;
            error _p0 = default!;

            var (conn, err) = net.Dial(network, address);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_NewClient(conn)!, error.As(err)!);

        }
    }
}}}
