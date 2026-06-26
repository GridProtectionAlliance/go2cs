// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

/*
	Some HTML presented at http://machine:port/debug/rpc
	Lists services, their methods, and some statistics, still rudimentary.
*/
using fmt = fmt_package;
using template = html.template_package;
using http = net.http_package;
using slices = slices_package;
using strings = strings_package;
using html;

partial class rpc_package {

internal static readonly @string debugText = """
<html>
	<body>
	<title>Services</title>
	{{range .}}
	<hr>
	Service {{.Name}}
	<hr>
		<table>
		<th align=center>Method</th><th align=center>Calls</th>
		{{range .Method}}
			<tr>
			<td align=left font=fixed>{{.Name}}({{.Type.ArgType}}, {{.Type.ReplyType}}) error</td>
			<td align=center>{{.Type.NumCalls}}</td>
			</tr>
		{{end}}
		</table>
	{{end}}
	</body>
	</html>
"""u8;

internal static ж<template.Template> debug = template.Must(template.New("RPC debug"u8).Parse(debugText));

// If set, print log statements for internal and I/O errors.
internal static bool debugLog = false;

[GoType] partial struct debugMethod {
    public ж<methodType> Type;
    public @string Name;
}

[GoType("[]debugMethod")] partial struct methodArray;

[GoType] partial struct debugService {
    public ж<service> Service;
    public @string Name;
    public slice<debugMethod> Method;
}

[GoType("[]debugService")] partial struct serviceArray;

internal static nint Len(this serviceArray s) {
    return len(s);
}

internal static bool Less(this serviceArray s, nint i, nint j) {
    return s[i].Name < s[j].Name;
}

internal static void Swap(this serviceArray s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

internal static nint Len(this methodArray m) {
    return len(m);
}

internal static bool Less(this methodArray m, nint i, nint j) {
    return m[i].Name < m[j].Name;
}

internal static void Swap(this methodArray m, nint i, nint j) {
    (m[i], m[j]) = (m[j], m[i]);
}

[GoType] partial struct debugHTTP {
    public partial ref ж<Server> Server { get; }
}

// Runs at /debug/rpc
internal static void ServeHTTP(this debugHTTP server, http.ResponseWriter w, ж<http.Request> Ꮡreq) {
    ref var req = ref Ꮡreq.val;

    // Build a sorted version of the data.
    serviceArray services = default!;
    server.serviceMap.Range(
    var servicesʗ2 = services;
    (any snamei, any svci) => {
        var svc = svci._<service.val>();
        ref var ds = ref heap<debugService>(out var Ꮡds);
        ds = new debugService(svc, snamei._<@string>(), new slice<debugMethod>(0, len((~svc).method)));
        foreach (var (mname, method) in (~svc).method) {
            ds.Method = append(ds.Method, new debugMethod(method, mname));
        }
        slices.SortFunc(ds.Method, 
        (debugMethod a, debugMethod b) => strings.Compare(a.Name, b.Name));
        services = append(services, ds);
        return true;
    });
    slices.SortFunc(services, 
    (debugService a, debugService b) => strings.Compare(a.Name, b.Name));
    var err = debug.Execute(w, services);
    if (err != default!) {
        fmt.Fprintln(w, "rpc: error executing template:", err.Error());
    }
}

} // end rpc_package
