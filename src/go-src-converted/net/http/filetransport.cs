// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 August 29 08:32:18 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\filetransport.go
using fmt = go.fmt_package;
using io = go.io_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // fileTransport implements RoundTripper for the 'file' protocol.
        private partial struct fileTransport
        {
            public fileHandler fh;
        }

        // NewFileTransport returns a new RoundTripper, serving the provided
        // FileSystem. The returned RoundTripper ignores the URL host in its
        // incoming requests, as well as most other properties of the
        // request.
        //
        // The typical use case for NewFileTransport is to register the "file"
        // protocol with a Transport, as in:
        //
        //   t := &http.Transport{}
        //   t.RegisterProtocol("file", http.NewFileTransport(http.Dir("/")))
        //   c := &http.Client{Transport: t}
        //   res, err := c.Get("file:///etc/passwd")
        //   ...
        public static RoundTripper NewFileTransport(FileSystem fs)
        {
            return new fileTransport(fileHandler{fs});
        }

        private static (ref Response, error) RoundTrip(this fileTransport t, ref Request req)
        { 
            // We start ServeHTTP in a goroutine, which may take a long
            // time if the file is large. The newPopulateResponseWriter
            // call returns a channel which either ServeHTTP or finish()
            // sends our *Response on, once the *Response itself has been
            // populated (even if the body itself is still being
            // written to the res.Body, a pipe)
            var (rw, resc) = newPopulateResponseWriter();
            go_(() => () =>
            {
                t.fh.ServeHTTP(rw, req);
                rw.finish();
            }());
            return (resc.Receive(), null);
        }

        private static (ref populateResponse, channel<ref Response>) newPopulateResponseWriter()
        {
            var (pr, pw) = io.Pipe();
            populateResponse rw = ref new populateResponse(ch:make(chan*Response),pw:pw,res:&Response{Proto:"HTTP/1.0",ProtoMajor:1,Header:make(Header),Close:true,Body:pr,},);
            return (rw, rw.ch);
        }

        // populateResponse is a ResponseWriter that populates the *Response
        // in res, and writes its body to a pipe connected to the response
        // body. Once writes begin or finish() is called, the response is sent
        // on ch.
        private partial struct populateResponse
        {
            public ptr<Response> res;
            public channel<ref Response> ch;
            public bool wroteHeader;
            public bool hasContent;
            public bool sentResponse;
            public ptr<io.PipeWriter> pw;
        }

        private static void finish(this ref populateResponse pr)
        {
            if (!pr.wroteHeader)
            {
                pr.WriteHeader(500L);
            }
            if (!pr.sentResponse)
            {
                pr.sendResponse();
            }
            pr.pw.Close();
        }

        private static void sendResponse(this ref populateResponse pr)
        {
            if (pr.sentResponse)
            {
                return;
            }
            pr.sentResponse = true;

            if (pr.hasContent)
            {
                pr.res.ContentLength = -1L;
            }
            pr.ch.Send(pr.res);
        }

        private static Header Header(this ref populateResponse pr)
        {
            return pr.res.Header;
        }

        private static void WriteHeader(this ref populateResponse pr, long code)
        {
            if (pr.wroteHeader)
            {
                return;
            }
            pr.wroteHeader = true;

            pr.res.StatusCode = code;
            pr.res.Status = fmt.Sprintf("%d %s", code, StatusText(code));
        }

        private static (long, error) Write(this ref populateResponse pr, slice<byte> p)
        {
            if (!pr.wroteHeader)
            {
                pr.WriteHeader(StatusOK);
            }
            pr.hasContent = true;
            if (!pr.sentResponse)
            {
                pr.sendResponse();
            }
            return pr.pw.Write(p);
        }
    }
}}
