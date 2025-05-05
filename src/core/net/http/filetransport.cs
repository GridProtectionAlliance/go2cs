// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using fmt = fmt_package;
using io = io_package;
using fs = io.fs_package;
using io;

partial class http_package {

// fileTransport implements RoundTripper for the 'file' protocol.
[GoType] partial struct fileTransport {
    internal fileHandler fh;
}

// NewFileTransport returns a new [RoundTripper], serving the provided
// [FileSystem]. The returned RoundTripper ignores the URL host in its
// incoming requests, as well as most other properties of the
// request.
//
// The typical use case for NewFileTransport is to register the "file"
// protocol with a [Transport], as in:
//
//	t := &http.Transport{}
//	t.RegisterProtocol("file", http.NewFileTransport(http.Dir("/")))
//	c := &http.Client{Transport: t}
//	res, err := c.Get("file:///etc/passwd")
//	...
public static RoundTripper NewFileTransport(FileSystem fs) {
    return new fileTransport(new fileHandler(fs));
}

// NewFileTransportFS returns a new [RoundTripper], serving the provided
// file system fsys. The returned RoundTripper ignores the URL host in its
// incoming requests, as well as most other properties of the
// request. The files provided by fsys must implement [io.Seeker].
//
// The typical use case for NewFileTransportFS is to register the "file"
// protocol with a [Transport], as in:
//
//	fsys := os.DirFS("/")
//	t := &http.Transport{}
//	t.RegisterProtocol("file", http.NewFileTransportFS(fsys))
//	c := &http.Client{Transport: t}
//	res, err := c.Get("file:///etc/passwd")
//	...
public static RoundTripper NewFileTransportFS(fs.FS fsys) {
    return NewFileTransport(FS(fsys));
}

internal static (ж<Response> resp, error err) RoundTrip(this fileTransport t, ж<Request> Ꮡreq) {
    ж<Response> resp = default!;
    error err = default!;

    ref var req = ref Ꮡreq.val;
    // We start ServeHTTP in a goroutine, which may take a long
    // time if the file is large. The newPopulateResponseWriter
    // call returns a channel which either ServeHTTP or finish()
    // sends our *Response on, once the *Response itself has been
    // populated (even if the body itself is still being
    // written to the res.Body, a pipe)
    (rw, resc) = newPopulateResponseWriter();
    var rwʗ1 = rw;
    var tʗ1 = t;
    goǃ(() => {
        tʗ1.fh.ServeHTTP(~rwʗ1, Ꮡreq);
        rwʗ1.finish();
    });
    return (ᐸꟷ(resc), default!);
}

internal static (ж<populateResponse>, /*<-*/channel<ж<Response>>) newPopulateResponseWriter() {
    (pr, pw) = io.Pipe();
    var rw = Ꮡ(new populateResponse(
        ch: new channel<ж<Response>>(1),
        pw: pw,
        res: Ꮡ(new Response(
            Proto: "HTTP/1.0"u8,
            ProtoMajor: 1,
            ΔHeader: new ΔHeader(),
            Close: true,
            Body: pr
        ))
    ));
    return (rw, (~rw).ch);
}

// populateResponse is a ResponseWriter that populates the *Response
// in res, and writes its body to a pipe connected to the response
// body. Once writes begin or finish() is called, the response is sent
// on ch.
[GoType] partial struct populateResponse {
    internal ж<Response> res;
    internal channel<ж<Response>> ch;
    internal bool wroteHeader;
    internal bool hasContent;
    internal bool sentResponse;
    internal ж<io_package.PipeWriter> pw;
}

[GoRecv] internal static void finish(this ref populateResponse pr) {
    if (!pr.wroteHeader) {
        pr.WriteHeader(500);
    }
    if (!pr.sentResponse) {
        pr.sendResponse();
    }
    pr.pw.Close();
}

[GoRecv] internal static void sendResponse(this ref populateResponse pr) {
    if (pr.sentResponse) {
        return;
    }
    pr.sentResponse = true;
    if (pr.hasContent) {
        pr.res.ContentLength = -1;
    }
    pr.ch.ᐸꟷ(pr.res);
}

[GoRecv] internal static ΔHeader Header(this ref populateResponse pr) {
    return pr.res.Header;
}

[GoRecv] internal static void WriteHeader(this ref populateResponse pr, nint code) {
    if (pr.wroteHeader) {
        return;
    }
    pr.wroteHeader = true;
    pr.res.StatusCode = code;
    pr.res.Status = fmt.Sprintf("%d %s"u8, code, StatusText(code));
}

[GoRecv] internal static (nint n, error err) Write(this ref populateResponse pr, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (!pr.wroteHeader) {
        pr.WriteHeader(StatusOK);
    }
    pr.hasContent = true;
    if (!pr.sentResponse) {
        pr.sendResponse();
    }
    return pr.pw.Write(p);
}

} // end http_package
