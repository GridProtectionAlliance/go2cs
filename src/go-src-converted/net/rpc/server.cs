// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
    Package rpc provides access to the exported methods of an object across a
    network or other I/O connection.  A server registers an object, making it visible
    as a service with the name of the type of the object.  After registration, exported
    methods of the object will be accessible remotely.  A server may register multiple
    objects (services) of different types but it is an error to register multiple
    objects of the same type.

    Only methods that satisfy these criteria will be made available for remote access;
    other methods will be ignored:

        - the method's type is exported.
        - the method is exported.
        - the method has two arguments, both exported (or builtin) types.
        - the method's second argument is a pointer.
        - the method has return type error.

    In effect, the method must look schematically like

        func (t *T) MethodName(argType T1, replyType *T2) error

    where T1 and T2 can be marshaled by encoding/gob.
    These requirements apply even if a different codec is used.
    (In the future, these requirements may soften for custom codecs.)

    The method's first argument represents the arguments provided by the caller; the
    second argument represents the result parameters to be returned to the caller.
    The method's return value, if non-nil, is passed back as a string that the client
    sees as if created by errors.New.  If an error is returned, the reply parameter
    will not be sent back to the client.

    The server may handle requests on a single connection by calling ServeConn.  More
    typically it will create a network listener and call Accept or, for an HTTP
    listener, HandleHTTP and http.Serve.

    A client wishing to use the service establishes a connection and then invokes
    NewClient on the connection.  The convenience function Dial (DialHTTP) performs
    both steps for a raw network connection (an HTTP connection).  The resulting
    Client object has two methods, Call and Go, that specify the service and method to
    call, a pointer containing the arguments, and a pointer to receive the result
    parameters.

    The Call method waits for the remote call to complete while the Go method
    launches the call asynchronously and signals completion using the Call
    structure's Done channel.

    Unless an explicit codec is set up, package encoding/gob is used to
    transport the data.

    Here is a simple example.  A server wishes to export an object of type Arith:

        package server

        import "errors"

        type Args struct {
            A, B int
        }

        type Quotient struct {
            Quo, Rem int
        }

        type Arith int

        func (t *Arith) Multiply(args *Args, reply *int) error {
            *reply = args.A * args.B
            return nil
        }

        func (t *Arith) Divide(args *Args, quo *Quotient) error {
            if args.B == 0 {
                return errors.New("divide by zero")
            }
            quo.Quo = args.A / args.B
            quo.Rem = args.A % args.B
            return nil
        }

    The server calls (for HTTP service):

        arith := new(Arith)
        rpc.Register(arith)
        rpc.HandleHTTP()
        l, e := net.Listen("tcp", ":1234")
        if e != nil {
            log.Fatal("listen error:", e)
        }
        go http.Serve(l, nil)

    At this point, clients can see a service "Arith" with methods "Arith.Multiply" and
    "Arith.Divide".  To invoke one, a client first dials the server:

        client, err := rpc.DialHTTP("tcp", serverAddress + ":1234")
        if err != nil {
            log.Fatal("dialing:", err)
        }

    Then it can make a remote call:

        // Synchronous call
        args := &server.Args{7,8}
        var reply int
        err = client.Call("Arith.Multiply", args, &reply)
        if err != nil {
            log.Fatal("arith error:", err)
        }
        fmt.Printf("Arith: %d*%d=%d", args.A, args.B, reply)

    or

        // Asynchronous call
        quotient := new(Quotient)
        divCall := client.Go("Arith.Divide", args, quotient, nil)
        replyCall := <-divCall.Done    // will be equal to divCall
        // check errors, print, etc.

    A server implementation will often provide a simple, type-safe wrapper for the
    client.

    The net/rpc package is frozen and is not accepting new features.
*/
// package rpc -- go2cs converted at 2020 October 09 05:00:36 UTC
// import "net/rpc" ==> using rpc = go.net.rpc_package
// Original source: C:\Go\src\net\rpc\server.go
using bufio = go.bufio_package;
using gob = go.encoding.gob_package;
using errors = go.errors_package;
using token = go.go.token_package;
using io = go.io_package;
using log = go.log_package;
using net = go.net_package;
using http = go.net.http_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using sync = go.sync_package;
using static go.builtin;
using System.Threading;

namespace go {
namespace net
{
    public static partial class rpc_package
    {
 
        // Defaults used by HandleHTTP
        public static readonly @string DefaultRPCPath = (@string)"/_goRPC_";
        public static readonly @string DefaultDebugPath = (@string)"/debug/rpc";


        // Precompute the reflect type for error. Can't use error directly
        // because Typeof takes an empty interface value. This is annoying.
        private static var typeOfError = reflect.TypeOf((error.val)(null)).Elem();

        private partial struct methodType
        {
            public ref sync.Mutex Mutex => ref Mutex_val; // protects counters
            public reflect.Method method;
            public reflect.Type ArgType;
            public reflect.Type ReplyType;
            public ulong numCalls;
        }

        private partial struct service
        {
            public @string name; // name of service
            public reflect.Value rcvr; // receiver of methods for the service
            public reflect.Type typ; // type of the receiver
            public map<@string, ptr<methodType>> method; // registered methods
        }

        // Request is a header written before every RPC call. It is used internally
        // but documented here as an aid to debugging, such as when analyzing
        // network traffic.
        public partial struct Request
        {
            public @string ServiceMethod; // format: "Service.Method"
            public ulong Seq; // sequence number chosen by client
            public ptr<Request> next; // for free list in Server
        }

        // Response is a header written before every RPC return. It is used internally
        // but documented here as an aid to debugging, such as when analyzing
        // network traffic.
        public partial struct Response
        {
            public @string ServiceMethod; // echoes that of the Request
            public ulong Seq; // echoes that of the request
            public @string Error; // error, if any.
            public ptr<Response> next; // for free list in Server
        }

        // Server represents an RPC Server.
        public partial struct Server
        {
            public sync.Map serviceMap; // map[string]*service
            public sync.Mutex reqLock; // protects freeReq
            public ptr<Request> freeReq;
            public sync.Mutex respLock; // protects freeResp
            public ptr<Response> freeResp;
        }

        // NewServer returns a new Server.
        public static ptr<Server> NewServer()
        {
            return addr(new Server());
        }

        // DefaultServer is the default instance of *Server.
        public static var DefaultServer = NewServer();

        // Is this type exported or a builtin?
        private static bool isExportedOrBuiltinType(reflect.Type t)
        {
            while (t.Kind() == reflect.Ptr)
            {
                t = t.Elem();
            } 
            // PkgPath will be non-empty even for an exported type,
            // so we need to check the type name as well.
 
            // PkgPath will be non-empty even for an exported type,
            // so we need to check the type name as well.
            return token.IsExported(t.Name()) || t.PkgPath() == "";

        }

        // Register publishes in the server the set of methods of the
        // receiver value that satisfy the following conditions:
        //    - exported method of exported type
        //    - two arguments, both of exported type
        //    - the second argument is a pointer
        //    - one return value, of type error
        // It returns an error if the receiver is not an exported type or has
        // no suitable methods. It also logs the error using package log.
        // The client accesses each method using a string of the form "Type.Method",
        // where Type is the receiver's concrete type.
        private static error Register(this ptr<Server> _addr_server, object rcvr)
        {
            ref Server server = ref _addr_server.val;

            return error.As(server.register(rcvr, "", false))!;
        }

        // RegisterName is like Register but uses the provided name for the type
        // instead of the receiver's concrete type.
        private static error RegisterName(this ptr<Server> _addr_server, @string name, object rcvr)
        {
            ref Server server = ref _addr_server.val;

            return error.As(server.register(rcvr, name, true))!;
        }

        private static error register(this ptr<Server> _addr_server, object rcvr, @string name, bool useName)
        {
            ref Server server = ref _addr_server.val;

            ptr<service> s = @new<service>();
            s.typ = reflect.TypeOf(rcvr);
            s.rcvr = reflect.ValueOf(rcvr);
            var sname = reflect.Indirect(s.rcvr).Type().Name();
            if (useName)
            {
                sname = name;
            }

            if (sname == "")
            {
                s = "rpc.Register: no service name for type " + s.typ.String();
                log.Print(s);
                return error.As(errors.New(s))!;
            }

            if (!token.IsExported(sname) && !useName)
            {
                s = "rpc.Register: type " + sname + " is not exported";
                log.Print(s);
                return error.As(errors.New(s))!;
            }

            s.name = sname; 

            // Install the methods
            s.method = suitableMethods(s.typ, true);

            if (len(s.method) == 0L)
            {
                @string str = ""; 

                // To help the user, see if a pointer receiver would work.
                var method = suitableMethods(reflect.PtrTo(s.typ), false);
                if (len(method) != 0L)
                {
                    str = "rpc.Register: type " + sname + " has no exported methods of suitable type (hint: pass a pointer to value of that type)";
                }
                else
                {
                    str = "rpc.Register: type " + sname + " has no exported methods of suitable type";
                }

                log.Print(str);
                return error.As(errors.New(str))!;

            }

            {
                var (_, dup) = server.serviceMap.LoadOrStore(sname, s);

                if (dup)
                {
                    return error.As(errors.New("rpc: service already defined: " + sname))!;
                }

            }

            return error.As(null!)!;

        }

        // suitableMethods returns suitable Rpc methods of typ, it will report
        // error using log if reportErr is true.
        private static map<@string, ptr<methodType>> suitableMethods(reflect.Type typ, bool reportErr)
        {
            var methods = make_map<@string, ptr<methodType>>();
            for (long m = 0L; m < typ.NumMethod(); m++)
            {
                var method = typ.Method(m);
                var mtype = method.Type;
                var mname = method.Name; 
                // Method must be exported.
                if (method.PkgPath != "")
                {
                    continue;
                } 
                // Method needs three ins: receiver, *args, *reply.
                if (mtype.NumIn() != 3L)
                {
                    if (reportErr)
                    {
                        log.Printf("rpc.Register: method %q has %d input parameters; needs exactly three\n", mname, mtype.NumIn());
                    }

                    continue;

                } 
                // First arg need not be a pointer.
                var argType = mtype.In(1L);
                if (!isExportedOrBuiltinType(argType))
                {
                    if (reportErr)
                    {
                        log.Printf("rpc.Register: argument type of method %q is not exported: %q\n", mname, argType);
                    }

                    continue;

                } 
                // Second arg must be a pointer.
                var replyType = mtype.In(2L);
                if (replyType.Kind() != reflect.Ptr)
                {
                    if (reportErr)
                    {
                        log.Printf("rpc.Register: reply type of method %q is not a pointer: %q\n", mname, replyType);
                    }

                    continue;

                } 
                // Reply type must be exported.
                if (!isExportedOrBuiltinType(replyType))
                {
                    if (reportErr)
                    {
                        log.Printf("rpc.Register: reply type of method %q is not exported: %q\n", mname, replyType);
                    }

                    continue;

                } 
                // Method needs one out.
                if (mtype.NumOut() != 1L)
                {
                    if (reportErr)
                    {
                        log.Printf("rpc.Register: method %q has %d output parameters; needs exactly one\n", mname, mtype.NumOut());
                    }

                    continue;

                } 
                // The return type of the method must be error.
                {
                    var returnType = mtype.Out(0L);

                    if (returnType != typeOfError)
                    {
                        if (reportErr)
                        {
                            log.Printf("rpc.Register: return type of method %q is %q, must be error\n", mname, returnType);
                        }

                        continue;

                    }

                }

                methods[mname] = addr(new methodType(method:method,ArgType:argType,ReplyType:replyType));

            }

            return methods;

        }

        // A value sent as a placeholder for the server's response value when the server
        // receives an invalid request. It is never decoded by the client since the Response
        // contains an error when it is used.
        private static struct{} invalidRequest = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

        private static void sendResponse(this ptr<Server> _addr_server, ptr<sync.Mutex> _addr_sending, ptr<Request> _addr_req, object reply, ServerCodec codec, @string errmsg)
        {
            ref Server server = ref _addr_server.val;
            ref sync.Mutex sending = ref _addr_sending.val;
            ref Request req = ref _addr_req.val;

            var resp = server.getResponse(); 
            // Encode the response header
            resp.ServiceMethod = req.ServiceMethod;
            if (errmsg != "")
            {
                resp.Error = errmsg;
                reply = invalidRequest;
            }

            resp.Seq = req.Seq;
            sending.Lock();
            var err = codec.WriteResponse(resp, reply);
            if (debugLog && err != null)
            {
                log.Println("rpc: writing response:", err);
            }

            sending.Unlock();
            server.freeResponse(resp);

        }

        private static ulong NumCalls(this ptr<methodType> _addr_m)
        {
            ulong n = default;
            ref methodType m = ref _addr_m.val;

            m.Lock();
            n = m.numCalls;
            m.Unlock();
            return n;
        }

        private static void call(this ptr<service> _addr_s, ptr<Server> _addr_server, ptr<sync.Mutex> _addr_sending, ptr<sync.WaitGroup> _addr_wg, ptr<methodType> _addr_mtype, ptr<Request> _addr_req, reflect.Value argv, reflect.Value replyv, ServerCodec codec) => func((defer, _, __) =>
        {
            ref service s = ref _addr_s.val;
            ref Server server = ref _addr_server.val;
            ref sync.Mutex sending = ref _addr_sending.val;
            ref sync.WaitGroup wg = ref _addr_wg.val;
            ref methodType mtype = ref _addr_mtype.val;
            ref Request req = ref _addr_req.val;

            if (wg != null)
            {
                defer(wg.Done());
            }

            mtype.Lock();
            mtype.numCalls++;
            mtype.Unlock();
            var function = mtype.method.Func; 
            // Invoke the method, providing a new value for the reply.
            var returnValues = function.Call(new slice<reflect.Value>(new reflect.Value[] { s.rcvr, argv, replyv })); 
            // The return value for the method is an error.
            var errInter = returnValues[0L].Interface();
            @string errmsg = "";
            if (errInter != null)
            {
                errmsg = errInter._<error>().Error();
            }

            server.sendResponse(sending, req, replyv.Interface(), codec, errmsg);
            server.freeRequest(req);

        });

        private partial struct gobServerCodec
        {
            public io.ReadWriteCloser rwc;
            public ptr<gob.Decoder> dec;
            public ptr<gob.Encoder> enc;
            public ptr<bufio.Writer> encBuf;
            public bool closed;
        }

        private static error ReadRequestHeader(this ptr<gobServerCodec> _addr_c, ptr<Request> _addr_r)
        {
            ref gobServerCodec c = ref _addr_c.val;
            ref Request r = ref _addr_r.val;

            return error.As(c.dec.Decode(r))!;
        }

        private static error ReadRequestBody(this ptr<gobServerCodec> _addr_c, object body)
        {
            ref gobServerCodec c = ref _addr_c.val;

            return error.As(c.dec.Decode(body))!;
        }

        private static error WriteResponse(this ptr<gobServerCodec> _addr_c, ptr<Response> _addr_r, object body)
        {
            error err = default!;
            ref gobServerCodec c = ref _addr_c.val;
            ref Response r = ref _addr_r.val;

            err = c.enc.Encode(r);

            if (err != null)
            {
                if (c.encBuf.Flush() == null)
                { 
                    // Gob couldn't encode the header. Should not happen, so if it does,
                    // shut down the connection to signal that the connection is broken.
                    log.Println("rpc: gob error encoding response:", err);
                    c.Close();

                }

                return ;

            }

            err = c.enc.Encode(body);

            if (err != null)
            {
                if (c.encBuf.Flush() == null)
                { 
                    // Was a gob problem encoding the body but the header has been written.
                    // Shut down the connection to signal that the connection is broken.
                    log.Println("rpc: gob error encoding body:", err);
                    c.Close();

                }

                return ;

            }

            return error.As(c.encBuf.Flush())!;

        }

        private static error Close(this ptr<gobServerCodec> _addr_c)
        {
            ref gobServerCodec c = ref _addr_c.val;

            if (c.closed)
            { 
                // Only call c.rwc.Close once; otherwise the semantics are undefined.
                return error.As(null!)!;

            }

            c.closed = true;
            return error.As(c.rwc.Close())!;

        }

        // ServeConn runs the server on a single connection.
        // ServeConn blocks, serving the connection until the client hangs up.
        // The caller typically invokes ServeConn in a go statement.
        // ServeConn uses the gob wire format (see package gob) on the
        // connection. To use an alternate codec, use ServeCodec.
        // See NewClient's comment for information about concurrent access.
        private static void ServeConn(this ptr<Server> _addr_server, io.ReadWriteCloser conn)
        {
            ref Server server = ref _addr_server.val;

            var buf = bufio.NewWriter(conn);
            ptr<gobServerCodec> srv = addr(new gobServerCodec(rwc:conn,dec:gob.NewDecoder(conn),enc:gob.NewEncoder(buf),encBuf:buf,));
            server.ServeCodec(srv);
        }

        // ServeCodec is like ServeConn but uses the specified codec to
        // decode requests and encode responses.
        private static void ServeCodec(this ptr<Server> _addr_server, ServerCodec codec)
        {
            ref Server server = ref _addr_server.val;

            ptr<sync.Mutex> sending = @new<sync.Mutex>();
            ptr<sync.WaitGroup> wg = @new<sync.WaitGroup>();
            while (true)
            {
                var (service, mtype, req, argv, replyv, keepReading, err) = server.readRequest(codec);
                if (err != null)
                {
                    if (debugLog && err != io.EOF)
                    {
                        log.Println("rpc:", err);
                    }

                    if (!keepReading)
                    {
                        break;
                    } 
                    // send a response if we actually managed to read a header.
                    if (req != null)
                    {
                        server.sendResponse(sending, req, invalidRequest, codec, err.Error());
                        server.freeRequest(req);
                    }

                    continue;

                }

                wg.Add(1L);
                go_(() => service.call(server, sending, wg, mtype, req, argv, replyv, codec));

            } 
            // We've seen that there are no more requests.
            // Wait for responses to be sent before closing codec.
 
            // We've seen that there are no more requests.
            // Wait for responses to be sent before closing codec.
            wg.Wait();
            codec.Close();

        }

        // ServeRequest is like ServeCodec but synchronously serves a single request.
        // It does not close the codec upon completion.
        private static error ServeRequest(this ptr<Server> _addr_server, ServerCodec codec)
        {
            ref Server server = ref _addr_server.val;

            ptr<sync.Mutex> sending = @new<sync.Mutex>();
            var (service, mtype, req, argv, replyv, keepReading, err) = server.readRequest(codec);
            if (err != null)
            {
                if (!keepReading)
                {
                    return error.As(err)!;
                } 
                // send a response if we actually managed to read a header.
                if (req != null)
                {
                    server.sendResponse(sending, req, invalidRequest, codec, err.Error());
                    server.freeRequest(req);
                }

                return error.As(err)!;

            }

            service.call(server, sending, null, mtype, req, argv, replyv, codec);
            return error.As(null!)!;

        }

        private static ptr<Request> getRequest(this ptr<Server> _addr_server)
        {
            ref Server server = ref _addr_server.val;

            server.reqLock.Lock();
            var req = server.freeReq;
            if (req == null)
            {
                req = @new<Request>();
            }
            else
            {
                server.freeReq = req.next;
                req.val = new Request();
            }

            server.reqLock.Unlock();
            return _addr_req!;

        }

        private static void freeRequest(this ptr<Server> _addr_server, ptr<Request> _addr_req)
        {
            ref Server server = ref _addr_server.val;
            ref Request req = ref _addr_req.val;

            server.reqLock.Lock();
            req.next = server.freeReq;
            server.freeReq = req;
            server.reqLock.Unlock();
        }

        private static ptr<Response> getResponse(this ptr<Server> _addr_server)
        {
            ref Server server = ref _addr_server.val;

            server.respLock.Lock();
            var resp = server.freeResp;
            if (resp == null)
            {
                resp = @new<Response>();
            }
            else
            {
                server.freeResp = resp.next;
                resp.val = new Response();
            }

            server.respLock.Unlock();
            return _addr_resp!;

        }

        private static void freeResponse(this ptr<Server> _addr_server, ptr<Response> _addr_resp)
        {
            ref Server server = ref _addr_server.val;
            ref Response resp = ref _addr_resp.val;

            server.respLock.Lock();
            resp.next = server.freeResp;
            server.freeResp = resp;
            server.respLock.Unlock();
        }

        private static (ptr<service>, ptr<methodType>, ptr<Request>, reflect.Value, reflect.Value, bool, error) readRequest(this ptr<Server> _addr_server, ServerCodec codec)
        {
            ptr<service> service = default!;
            ptr<methodType> mtype = default!;
            ptr<Request> req = default!;
            reflect.Value argv = default;
            reflect.Value replyv = default;
            bool keepReading = default;
            error err = default!;
            ref Server server = ref _addr_server.val;

            service, mtype, req, keepReading, err = server.readRequestHeader(codec);
            if (err != null)
            {
                if (!keepReading)
                {
                    return ;
                } 
                // discard body
                codec.ReadRequestBody(null);
                return ;

            } 

            // Decode the argument value.
            var argIsValue = false; // if true, need to indirect before calling.
            if (mtype.ArgType.Kind() == reflect.Ptr)
            {
                argv = reflect.New(mtype.ArgType.Elem());
            }
            else
            {
                argv = reflect.New(mtype.ArgType);
                argIsValue = true;
            } 
            // argv guaranteed to be a pointer now.
            err = codec.ReadRequestBody(argv.Interface());

            if (err != null)
            {
                return ;
            }

            if (argIsValue)
            {
                argv = argv.Elem();
            }

            replyv = reflect.New(mtype.ReplyType.Elem());


            if (mtype.ReplyType.Elem().Kind() == reflect.Map) 
                replyv.Elem().Set(reflect.MakeMap(mtype.ReplyType.Elem()));
            else if (mtype.ReplyType.Elem().Kind() == reflect.Slice) 
                replyv.Elem().Set(reflect.MakeSlice(mtype.ReplyType.Elem(), 0L, 0L));
                        return ;

        }

        private static (ptr<service>, ptr<methodType>, ptr<Request>, bool, error) readRequestHeader(this ptr<Server> _addr_server, ServerCodec codec)
        {
            ptr<service> svc = default!;
            ptr<methodType> mtype = default!;
            ptr<Request> req = default!;
            bool keepReading = default;
            error err = default!;
            ref Server server = ref _addr_server.val;
 
            // Grab the request header.
            req = server.getRequest();
            err = codec.ReadRequestHeader(req);
            if (err != null)
            {
                req = null;
                if (err == io.EOF || err == io.ErrUnexpectedEOF)
                {
                    return ;
                }

                err = errors.New("rpc: server cannot decode request: " + err.Error());
                return ;

            } 

            // We read the header successfully. If we see an error now,
            // we can still recover and move on to the next request.
            keepReading = true;

            var dot = strings.LastIndex(req.ServiceMethod, ".");
            if (dot < 0L)
            {
                err = errors.New("rpc: service/method request ill-formed: " + req.ServiceMethod);
                return ;
            }

            var serviceName = req.ServiceMethod[..dot];
            var methodName = req.ServiceMethod[dot + 1L..]; 

            // Look up the request.
            var (svci, ok) = server.serviceMap.Load(serviceName);
            if (!ok)
            {
                err = errors.New("rpc: can't find service " + req.ServiceMethod);
                return ;
            }

            svc = svci._<ptr<service>>();
            mtype = svc.method[methodName];
            if (mtype == null)
            {
                err = errors.New("rpc: can't find method " + req.ServiceMethod);
            }

            return ;

        }

        // Accept accepts connections on the listener and serves requests
        // for each incoming connection. Accept blocks until the listener
        // returns a non-nil error. The caller typically invokes Accept in a
        // go statement.
        private static void Accept(this ptr<Server> _addr_server, net.Listener lis)
        {
            ref Server server = ref _addr_server.val;

            while (true)
            {
                var (conn, err) = lis.Accept();
                if (err != null)
                {
                    log.Print("rpc.Serve: accept:", err.Error());
                    return ;
                }

                go_(() => server.ServeConn(conn));

            }


        }

        // Register publishes the receiver's methods in the DefaultServer.
        public static error Register(object rcvr)
        {
            return error.As(DefaultServer.Register(rcvr))!;
        }

        // RegisterName is like Register but uses the provided name for the type
        // instead of the receiver's concrete type.
        public static error RegisterName(@string name, object rcvr)
        {
            return error.As(DefaultServer.RegisterName(name, rcvr))!;
        }

        // A ServerCodec implements reading of RPC requests and writing of
        // RPC responses for the server side of an RPC session.
        // The server calls ReadRequestHeader and ReadRequestBody in pairs
        // to read requests from the connection, and it calls WriteResponse to
        // write a response back. The server calls Close when finished with the
        // connection. ReadRequestBody may be called with a nil
        // argument to force the body of the request to be read and discarded.
        // See NewClient's comment for information about concurrent access.
        public partial interface ServerCodec
        {
            error ReadRequestHeader(ptr<Request> _p0);
            error ReadRequestBody(object _p0);
            error WriteResponse(ptr<Response> _p0, object _p0); // Close can be called multiple times and must be idempotent.
            error Close();
        }

        // ServeConn runs the DefaultServer on a single connection.
        // ServeConn blocks, serving the connection until the client hangs up.
        // The caller typically invokes ServeConn in a go statement.
        // ServeConn uses the gob wire format (see package gob) on the
        // connection. To use an alternate codec, use ServeCodec.
        // See NewClient's comment for information about concurrent access.
        public static void ServeConn(io.ReadWriteCloser conn)
        {
            DefaultServer.ServeConn(conn);
        }

        // ServeCodec is like ServeConn but uses the specified codec to
        // decode requests and encode responses.
        public static void ServeCodec(ServerCodec codec)
        {
            DefaultServer.ServeCodec(codec);
        }

        // ServeRequest is like ServeCodec but synchronously serves a single request.
        // It does not close the codec upon completion.
        public static error ServeRequest(ServerCodec codec)
        {
            return error.As(DefaultServer.ServeRequest(codec))!;
        }

        // Accept accepts connections on the listener and serves requests
        // to DefaultServer for each incoming connection.
        // Accept blocks; the caller typically invokes it in a go statement.
        public static void Accept(net.Listener lis)
        {
            DefaultServer.Accept(lis);
        }

        // Can connect to RPC service using HTTP CONNECT to rpcPath.
        private static @string connected = "200 Connected to Go RPC";

        // ServeHTTP implements an http.Handler that answers RPC requests.
        private static void ServeHTTP(this ptr<Server> _addr_server, http.ResponseWriter w, ptr<http.Request> _addr_req)
        {
            ref Server server = ref _addr_server.val;
            ref http.Request req = ref _addr_req.val;

            if (req.Method != "CONNECT")
            {
                w.Header().Set("Content-Type", "text/plain; charset=utf-8");
                w.WriteHeader(http.StatusMethodNotAllowed);
                io.WriteString(w, "405 must CONNECT\n");
                return ;
            }

            http.Hijacker (conn, _, err) = w._<http.Hijacker>().Hijack();
            if (err != null)
            {
                log.Print("rpc hijacking ", req.RemoteAddr, ": ", err.Error());
                return ;
            }

            io.WriteString(conn, "HTTP/1.0 " + connected + "\n\n");
            server.ServeConn(conn);

        }

        // HandleHTTP registers an HTTP handler for RPC messages on rpcPath,
        // and a debugging handler on debugPath.
        // It is still necessary to invoke http.Serve(), typically in a go statement.
        private static void HandleHTTP(this ptr<Server> _addr_server, @string rpcPath, @string debugPath)
        {
            ref Server server = ref _addr_server.val;

            http.Handle(rpcPath, server);
            http.Handle(debugPath, new debugHTTP(server));
        }

        // HandleHTTP registers an HTTP handler for RPC messages to DefaultServer
        // on DefaultRPCPath and a debugging handler on DefaultDebugPath.
        // It is still necessary to invoke http.Serve(), typically in a go statement.
        public static void HandleHTTP()
        {
            DefaultServer.HandleHTTP(DefaultRPCPath, DefaultDebugPath);
        }
    }
}}
