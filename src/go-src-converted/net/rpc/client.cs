// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package rpc -- go2cs converted at 2020 October 08 03:43:22 UTC
// import "net/rpc" ==> using rpc = go.net.rpc_package
// Original source: C:\Go\src\net\rpc\client.go
using bufio = go.bufio_package;
using gob = go.encoding.gob_package;
using errors = go.errors_package;
using io = go.io_package;
using log = go.log_package;
using net = go.net_package;
using http = go.net.http_package;
using sync = go.sync_package;
using static go.builtin;
using System.Threading;

namespace go {
namespace net
{
    public static partial class rpc_package
    {
        // ServerError represents an error that has been returned from
        // the remote side of the RPC connection.
        public partial struct ServerError // : @string
        {
        }

        public static @string Error(this ServerError e)
        {
            return string(e);
        }

        public static var ErrShutdown = errors.New("connection is shut down");

        // Call represents an active RPC.
        public partial struct Call
        {
            public @string ServiceMethod; // The name of the service and method to call.
            public error Error; // After completion, the error status.
            public channel<ptr<Call>> Done; // Receives *Call when Go is complete.
        }

        // Client represents an RPC Client.
        // There may be multiple outstanding Calls associated
        // with a single Client, and a Client may be used by
        // multiple goroutines simultaneously.
        public partial struct Client
        {
            public ClientCodec codec;
            public sync.Mutex reqMutex; // protects following
            public Request request;
            public sync.Mutex mutex; // protects following
            public ulong seq;
            public map<ulong, ptr<Call>> pending;
            public bool closing; // user has called Close
            public bool shutdown; // server has told us to stop
        }

        // A ClientCodec implements writing of RPC requests and
        // reading of RPC responses for the client side of an RPC session.
        // The client calls WriteRequest to write a request to the connection
        // and calls ReadResponseHeader and ReadResponseBody in pairs
        // to read responses. The client calls Close when finished with the
        // connection. ReadResponseBody may be called with a nil
        // argument to force the body of the response to be read and then
        // discarded.
        // See NewClient's comment for information about concurrent access.
        public partial interface ClientCodec
        {
            error WriteRequest(ptr<Request> _p0, object _p0);
            error ReadResponseHeader(ptr<Response> _p0);
            error ReadResponseBody(object _p0);
            error Close();
        }

        private static void send(this ptr<Client> _addr_client, ptr<Call> _addr_call) => func((defer, _, __) =>
        {
            ref Client client = ref _addr_client.val;
            ref Call call = ref _addr_call.val;

            client.reqMutex.Lock();
            defer(client.reqMutex.Unlock()); 

            // Register this call.
            client.mutex.Lock();
            if (client.shutdown || client.closing)
            {
                client.mutex.Unlock();
                call.Error = ErrShutdown;
                call.done();
                return ;
            }

            var seq = client.seq;
            client.seq++;
            client.pending[seq] = call;
            client.mutex.Unlock(); 

            // Encode and send the request.
            client.request.Seq = seq;
            client.request.ServiceMethod = call.ServiceMethod;
            var err = client.codec.WriteRequest(_addr_client.request, call.Args);
            if (err != null)
            {
                client.mutex.Lock();
                call = client.pending[seq];
                delete(client.pending, seq);
                client.mutex.Unlock();
                if (call != null)
                {
                    call.Error = err;
                    call.done();
                }

            }

        });

        private static void input(this ptr<Client> _addr_client)
        {
            ref Client client = ref _addr_client.val;

            error err = default!;
            ref Response response = ref heap(out ptr<Response> _addr_response);
            while (err == null)
            {
                response = new Response();
                err = error.As(client.codec.ReadResponseHeader(_addr_response))!;
                if (err != null)
                {
                    break;
                }

                var seq = response.Seq;
                client.mutex.Lock();
                var call = client.pending[seq];
                delete(client.pending, seq);
                client.mutex.Unlock();


                if (call == null) 
                    // We've got no pending call. That usually means that
                    // WriteRequest partially failed, and call was already
                    // removed; response is a server telling us about an
                    // error reading request body. We should still attempt
                    // to read error body, but there's no one to give it to.
                    err = error.As(client.codec.ReadResponseBody(null))!;
                    if (err != null)
                    {
                        err = error.As(errors.New("reading error body: " + err.Error()))!;
                    }

                else if (response.Error != "") 
                    // We've got an error response. Give this to the request;
                    // any subsequent requests will get the ReadResponseBody
                    // error if there is one.
                    call.Error = ServerError(response.Error);
                    err = error.As(client.codec.ReadResponseBody(null))!;
                    if (err != null)
                    {
                        err = error.As(errors.New("reading error body: " + err.Error()))!;
                    }

                    call.done();
                else 
                    err = error.As(client.codec.ReadResponseBody(call.Reply))!;
                    if (err != null)
                    {
                        call.Error = errors.New("reading body " + err.Error());
                    }

                    call.done();
                
            } 
            // Terminate pending calls.
 
            // Terminate pending calls.
            client.reqMutex.Lock();
            client.mutex.Lock();
            client.shutdown = true;
            var closing = client.closing;
            if (err == io.EOF)
            {
                if (closing)
                {
                    err = error.As(ErrShutdown)!;
                }
                else
                {
                    err = error.As(io.ErrUnexpectedEOF)!;
                }

            }

            {
                var call__prev1 = call;

                foreach (var (_, __call) in client.pending)
                {
                    call = __call;
                    call.Error = err;
                    call.done();
                }

                call = call__prev1;
            }

            client.mutex.Unlock();
            client.reqMutex.Unlock();
            if (debugLog && err != io.EOF && !closing)
            {
                log.Println("rpc: client protocol error:", err);
            }

        }

        private static void done(this ptr<Call> _addr_call)
        {
            ref Call call = ref _addr_call.val;

            if (debugLog)
            {
                log.Println("rpc: discarding Call reply due to insufficient Done chan capacity");
            }

        }

        // NewClient returns a new Client to handle requests to the
        // set of services at the other end of the connection.
        // It adds a buffer to the write side of the connection so
        // the header and payload are sent as a unit.
        //
        // The read and write halves of the connection are serialized independently,
        // so no interlocking is required. However each half may be accessed
        // concurrently so the implementation of conn should protect against
        // concurrent reads or concurrent writes.
        public static ptr<Client> NewClient(io.ReadWriteCloser conn)
        {
            var encBuf = bufio.NewWriter(conn);
            ptr<gobClientCodec> client = addr(new gobClientCodec(conn,gob.NewDecoder(conn),gob.NewEncoder(encBuf),encBuf));
            return _addr_NewClientWithCodec(client)!;
        }

        // NewClientWithCodec is like NewClient but uses the specified
        // codec to encode requests and decode responses.
        public static ptr<Client> NewClientWithCodec(ClientCodec codec)
        {
            ptr<Client> client = addr(new Client(codec:codec,pending:make(map[uint64]*Call),));
            go_(() => client.input());
            return _addr_client!;
        }

        private partial struct gobClientCodec
        {
            public io.ReadWriteCloser rwc;
            public ptr<gob.Decoder> dec;
            public ptr<gob.Encoder> enc;
            public ptr<bufio.Writer> encBuf;
        }

        private static error WriteRequest(this ptr<gobClientCodec> _addr_c, ptr<Request> _addr_r, object body)
        {
            error err = default!;
            ref gobClientCodec c = ref _addr_c.val;
            ref Request r = ref _addr_r.val;

            err = c.enc.Encode(r);

            if (err != null)
            {
                return ;
            }

            err = c.enc.Encode(body);

            if (err != null)
            {
                return ;
            }

            return error.As(c.encBuf.Flush())!;

        }

        private static error ReadResponseHeader(this ptr<gobClientCodec> _addr_c, ptr<Response> _addr_r)
        {
            ref gobClientCodec c = ref _addr_c.val;
            ref Response r = ref _addr_r.val;

            return error.As(c.dec.Decode(r))!;
        }

        private static error ReadResponseBody(this ptr<gobClientCodec> _addr_c, object body)
        {
            ref gobClientCodec c = ref _addr_c.val;

            return error.As(c.dec.Decode(body))!;
        }

        private static error Close(this ptr<gobClientCodec> _addr_c)
        {
            ref gobClientCodec c = ref _addr_c.val;

            return error.As(c.rwc.Close())!;
        }

        // DialHTTP connects to an HTTP RPC server at the specified network address
        // listening on the default HTTP RPC path.
        public static (ptr<Client>, error) DialHTTP(@string network, @string address)
        {
            ptr<Client> _p0 = default!;
            error _p0 = default!;

            return _addr_DialHTTPPath(network, address, DefaultRPCPath)!;
        }

        // DialHTTPPath connects to an HTTP RPC server
        // at the specified network address and path.
        public static (ptr<Client>, error) DialHTTPPath(@string network, @string address, @string path)
        {
            ptr<Client> _p0 = default!;
            error _p0 = default!;

            error err = default!;
            var (conn, err) = net.Dial(network, address);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            io.WriteString(conn, "CONNECT " + path + " HTTP/1.0\n\n"); 

            // Require successful HTTP response
            // before switching to RPC protocol.
            var (resp, err) = http.ReadResponse(bufio.NewReader(conn), addr(new http.Request(Method:"CONNECT")));
            if (err == null && resp.Status == connected)
            {
                return (_addr_NewClient(conn)!, error.As(null!)!);
            }

            if (err == null)
            {
                err = error.As(errors.New("unexpected HTTP response: " + resp.Status))!;
            }

            conn.Close();
            return (_addr_null!, error.As(addr(new net.OpError(Op:"dial-http",Net:network+" "+address,Addr:nil,Err:err,))!)!);

        }

        // Dial connects to an RPC server at the specified network address.
        public static (ptr<Client>, error) Dial(@string network, @string address)
        {
            ptr<Client> _p0 = default!;
            error _p0 = default!;

            var (conn, err) = net.Dial(network, address);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_NewClient(conn)!, error.As(null!)!);

        }

        // Close calls the underlying codec's Close method. If the connection is already
        // shutting down, ErrShutdown is returned.
        private static error Close(this ptr<Client> _addr_client)
        {
            ref Client client = ref _addr_client.val;

            client.mutex.Lock();
            if (client.closing)
            {
                client.mutex.Unlock();
                return error.As(ErrShutdown)!;
            }

            client.closing = true;
            client.mutex.Unlock();
            return error.As(client.codec.Close())!;

        }

        // Go invokes the function asynchronously. It returns the Call structure representing
        // the invocation. The done channel will signal when the call is complete by returning
        // the same Call object. If done is nil, Go will allocate a new channel.
        // If non-nil, done must be buffered or Go will deliberately crash.
        private static ptr<Call> Go(this ptr<Client> _addr_client, @string serviceMethod, object args, object reply, channel<ptr<Call>> done)
        {
            ref Client client = ref _addr_client.val;

            ptr<Call> call = @new<Call>();
            call.ServiceMethod = serviceMethod;
            call.Args = args;
            call.Reply = reply;
            if (done == null)
            {
                done = make_channel<ptr<Call>>(10L); // buffered.
            }
            else
            { 
                // If caller passes done != nil, it must arrange that
                // done has enough buffer for the number of simultaneous
                // RPCs that will be using that channel. If the channel
                // is totally unbuffered, it's best not to run at all.
                if (cap(done) == 0L)
                {
                    log.Panic("rpc: done channel is unbuffered");
                }

            }

            call.Done = done;
            client.send(call);
            return _addr_call!;

        }

        // Call invokes the named function, waits for it to complete, and returns its error status.
        private static error Call(this ptr<Client> _addr_client, @string serviceMethod, object args, object reply)
        {
            ref Client client = ref _addr_client.val;

            var call = client.Go(serviceMethod, args, reply, make_channel<ptr<Call>>(1L)).Done.Receive();
            return error.As(call.Error)!;
        }
    }
}}
