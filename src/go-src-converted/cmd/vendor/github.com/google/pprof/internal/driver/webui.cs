// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// package driver -- go2cs converted at 2022 March 06 23:23:33 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\webui.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using template = go.html.template_package;
using net = go.net_package;
using http = go.net.http_package;
using gourl = go.net.url_package;
using os = go.os_package;
using exec = go.os.exec_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;

using graph = go.github.com.google.pprof.@internal.graph_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using report = go.github.com.google.pprof.@internal.report_package;
using profile = go.github.com.google.pprof.profile_package;
using System.Threading;
using System;


namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class driver_package {

    // webInterface holds the state needed for serving a browser based interface.
private partial struct webInterface {
    public ptr<profile.Profile> prof;
    public ptr<plugin.Options> options;
    public map<@string, @string> help;
    public ptr<template.Template> templates;
    public @string settingsFile;
}

private static (ptr<webInterface>, error) makeWebInterface(ptr<profile.Profile> _addr_p, ptr<plugin.Options> _addr_opt) {
    ptr<webInterface> _p0 = default!;
    error _p0 = default!;
    ref profile.Profile p = ref _addr_p.val;
    ref plugin.Options opt = ref _addr_opt.val;

    var (settingsFile, err) = settingsFileName();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var templates = template.New("templategroup");
    addTemplates(templates);
    report.AddSourceTemplates(templates);
    return (addr(new webInterface(prof:p,options:opt,help:make(map[string]string),templates:templates,settingsFile:settingsFile,)), error.As(null!)!);

}

// maxEntries is the maximum number of entries to print for text interfaces.
private static readonly nint maxEntries = 50;

// errorCatcher is a UI that captures errors for reporting to the browser.


// errorCatcher is a UI that captures errors for reporting to the browser.
private partial struct errorCatcher {
    public ref plugin.UI UI => ref UI_val;
    public slice<@string> errors;
}

private static void PrintErr(this ptr<errorCatcher> _addr_ec, params object[] args) {
    args = args.Clone();
    ref errorCatcher ec = ref _addr_ec.val;

    ec.errors = append(ec.errors, strings.TrimSuffix(fmt.Sprintln(args), "\n"));
    ec.UI.PrintErr(args);
}

// webArgs contains arguments passed to templates in webhtml.go.
private partial struct webArgs {
    public @string Title;
    public slice<@string> Errors;
    public long Total;
    public slice<@string> SampleTypes;
    public slice<@string> Legend;
    public map<@string, @string> Help;
    public slice<@string> Nodes;
    public template.HTML HTMLBody;
    public @string TextBody;
    public slice<report.TextItem> Top;
    public template.JS FlameGraph;
    public slice<configMenuEntry> Configs;
}

private static error serveWebInterface(@string hostport, ptr<profile.Profile> _addr_p, ptr<plugin.Options> _addr_o, bool disableBrowser) {
    ref profile.Profile p = ref _addr_p.val;
    ref plugin.Options o = ref _addr_o.val;

    var (host, port, err) = getHostAndPort(hostport);
    if (err != null) {
        return error.As(err)!;
    }
    interactiveMode = true;
    var (ui, err) = makeWebInterface(_addr_p, _addr_o);
    if (err != null) {
        return error.As(err)!;
    }
    {
        var n__prev1 = n;

        foreach (var (__n, __c) in pprofCommands) {
            n = __n;
            c = __c;
            ui.help[n] = c.description;
        }
        n = n__prev1;
    }

    {
        var n__prev1 = n;

        foreach (var (__n, __help) in configHelp) {
            n = __n;
            help = __help;
            ui.help[n] = help;
        }
        n = n__prev1;
    }

    ui.help["details"] = "Show information about the profile and this view";
    ui.help["graph"] = "Display profile as a directed graph";
    ui.help["reset"] = "Show the entire profile";
    ui.help["save_config"] = "Save current settings";

    var server = o.HTTPServer;
    if (server == null) {
        server = defaultWebServer;
    }
    ptr<plugin.HTTPServerArgs> args = addr(new plugin.HTTPServerArgs(Hostport:net.JoinHostPort(host,strconv.Itoa(port)),Host:host,Port:port,Handlers:map[string]http.Handler{"/":http.HandlerFunc(ui.dot),"/top":http.HandlerFunc(ui.top),"/disasm":http.HandlerFunc(ui.disasm),"/source":http.HandlerFunc(ui.source),"/peek":http.HandlerFunc(ui.peek),"/flamegraph":http.HandlerFunc(ui.flamegraph),"/saveconfig":http.HandlerFunc(ui.saveConfig),"/deleteconfig":http.HandlerFunc(ui.deleteConfig),},));

    @string url = "http://" + args.Hostport;

    o.UI.Print("Serving web UI on ", url);

    if (o.UI.WantBrowser() && !disableBrowser) {
        go_(() => openBrowser(url, _addr_o));
    }
    return error.As(server(args))!;

}

private static (@string, nint, error) getHostAndPort(@string hostport) {
    @string _p0 = default;
    nint _p0 = default;
    error _p0 = default!;

    var (host, portStr, err) = net.SplitHostPort(hostport);
    if (err != null) {
        return ("", 0, error.As(fmt.Errorf("could not split http address: %v", err))!);
    }
    if (host == "") {
        host = "localhost";
    }
    nint port = default;
    if (portStr == "") {
        var (ln, err) = net.Listen("tcp", net.JoinHostPort(host, "0"));
        if (err != null) {
            return ("", 0, error.As(fmt.Errorf("could not generate random port: %v", err))!);
        }
        port = ln.Addr()._<ptr<net.TCPAddr>>().Port;
        err = ln.Close();
        if (err != null) {
            return ("", 0, error.As(fmt.Errorf("could not generate random port: %v", err))!);
        }
    }
    else
 {
        port, err = strconv.Atoi(portStr);
        if (err != null) {
            return ("", 0, error.As(fmt.Errorf("invalid port number: %v", err))!);
        }
    }
    return (host, port, error.As(null!)!);

}
private static error defaultWebServer(ptr<plugin.HTTPServerArgs> _addr_args) {
    ref plugin.HTTPServerArgs args = ref _addr_args.val;

    var (ln, err) = net.Listen("tcp", args.Hostport);
    if (err != null) {
        return error.As(err)!;
    }
    var isLocal = isLocalhost(args.Host);
    var handler = http.HandlerFunc((w, req) => {
        if (isLocal) { 
            // Only allow local clients
            var (host, _, err) = net.SplitHostPort(req.RemoteAddr);
            if (err != null || !isLocalhost(host)) {
                http.Error(w, "permission denied", http.StatusForbidden);
                return ;
            }

        }
        var h = args.Handlers[req.URL.Path];
        if (h == null) { 
            // Fall back to default behavior
            h = http.DefaultServeMux;

        }
        h.ServeHTTP(w, req);

    }); 

    // We serve the ui at /ui/ and redirect there from the root. This is done
    // to surface any problems with serving the ui at a non-root early. See:
    //
    // https://github.com/google/pprof/pull/348
    var mux = http.NewServeMux();
    mux.Handle("/ui/", http.StripPrefix("/ui", handler));
    mux.Handle("/", redirectWithQuery("/ui"));
    ptr<http.Server> s = addr(new http.Server(Handler:mux));
    return error.As(s.Serve(ln))!;

}

private static http.HandlerFunc redirectWithQuery(@string path) {
    return (w, r) => {
        ptr<gourl.URL> pathWithQuery = addr(new gourl.URL(Path:path,RawQuery:r.URL.RawQuery));
        http.Redirect(w, r, pathWithQuery.String(), http.StatusTemporaryRedirect);
    };
}

private static bool isLocalhost(@string host) {
    foreach (var (_, v) in new slice<@string>(new @string[] { "localhost", "127.0.0.1", "[::1]", "::1" })) {
        if (host == v) {
            return true;
        }
    }    return false;

}

private static void openBrowser(@string url, ptr<plugin.Options> _addr_o) {
    ref plugin.Options o = ref _addr_o.val;
 
    // Construct URL.
    var (baseURL, _) = gourl.Parse(url);
    var current = currentConfig();
    var (u, _) = current.makeURL(baseURL.val); 

    // Give server a little time to get ready.
    time.Sleep(time.Millisecond * 500);

    foreach (var (_, b) in browsers()) {
        var args = strings.Split(b, " ");
        if (len(args) == 0) {
            continue;
        }
        var viewer = exec.Command(args[0], append(args[(int)1..], u.String()));
        viewer.Stderr = os.Stderr;
        {
            var err = viewer.Start();

            if (err == null) {
                return ;
            }

        }

    }    o.UI.PrintErr(u.String());

}

// makeReport generates a report for the specified command.
// If configEditor is not null, it is used to edit the config used for the report.
private static (ptr<report.Report>, slice<@string>) makeReport(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req, slice<@string> cmd, Action<ptr<config>> configEditor) {
    ptr<report.Report> _p0 = default!;
    slice<@string> _p0 = default;
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;

    ref var cfg = ref heap(currentConfig(), out ptr<var> _addr_cfg);
    {
        var err = cfg.applyURL(req.URL.Query());

        if (err != null) {
            http.Error(w, err.Error(), http.StatusBadRequest);
            ui.options.UI.PrintErr(err);
            return (_addr_null!, null);
        }
    }

    if (configEditor != null) {
        configEditor(_addr_cfg);
    }
    ptr<errorCatcher> catcher = addr(new errorCatcher(UI:ui.options.UI));
    ref var options = ref heap(ui.options.val, out ptr<var> _addr_options);
    options.UI = catcher;
    var (_, rpt, err) = generateRawReport(ui.prof, cmd, cfg, _addr_options);
    if (err != null) {
        http.Error(w, err.Error(), http.StatusBadRequest);
        ui.options.UI.PrintErr(err);
        return (_addr_null!, null);
    }
    return (_addr_rpt!, catcher.errors);

}

// render generates html using the named template based on the contents of data.
private static void render(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req, @string tmpl, ptr<report.Report> _addr_rpt, slice<@string> errList, slice<@string> legend, webArgs data) {
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;
    ref report.Report rpt = ref _addr_rpt.val;

    var file = getFromLegend(legend, "File: ", "unknown");
    var profile = getFromLegend(legend, "Type: ", "unknown");
    data.Title = file + " " + profile;
    data.Errors = errList;
    data.Total = rpt.Total();
    data.SampleTypes = sampleTypes(ui.prof);
    data.Legend = legend;
    data.Help = ui.help;
    data.Configs = configMenu(ui.settingsFile, req.URL.val);

    ptr<bytes.Buffer> html = addr(new bytes.Buffer());
    {
        var err = ui.templates.ExecuteTemplate(html, tmpl, data);

        if (err != null) {
            http.Error(w, "internal template error", http.StatusInternalServerError);
            ui.options.UI.PrintErr(err);
            return ;
        }
    }

    w.Header().Set("Content-Type", "text/html");
    w.Write(html.Bytes());

}

// dot generates a web page containing an svg diagram.
private static void dot(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req) {
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;

    var (rpt, errList) = ui.makeReport(w, req, new slice<@string>(new @string[] { "svg" }), null);
    if (rpt == null) {
        return ; // error already reported
    }
    var (g, config) = report.GetDOT(rpt);
    var legend = config.Labels;
    config.Labels = null;
    ptr<bytes.Buffer> dot = addr(new bytes.Buffer());
    graph.ComposeDot(dot, g, addr(new graph.DotAttributes()), config); 

    // Convert to svg.
    var (svg, err) = dotToSvg(dot.Bytes());
    if (err != null) {
        http.Error(w, "Could not execute dot; may need to install graphviz.", http.StatusNotImplemented);
        ui.options.UI.PrintErr("Failed to execute dot. Is Graphviz installed?\n", err);
        return ;
    }
    @string nodes = new slice<@string>(new @string[] { "" }); // dot starts with node numbered 1
    foreach (var (_, n) in g.Nodes) {
        nodes = append(nodes, n.Info.Name);
    }    ui.render(w, req, "graph", rpt, errList, legend, new webArgs(HTMLBody:template.HTML(string(svg)),Nodes:nodes,));

}

private static (slice<byte>, error) dotToSvg(slice<byte> dot) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    var cmd = exec.Command("dot", "-Tsvg");
    ptr<bytes.Buffer> @out = addr(new bytes.Buffer());
    (cmd.Stdin, cmd.Stdout, cmd.Stderr) = (bytes.NewBuffer(dot), out, os.Stderr);    {
        var err = cmd.Run();

        if (err != null) {
            return (null, error.As(err)!);
        }
    } 

    // Fix dot bug related to unquoted ampersands.
    var svg = bytes.Replace(@out.Bytes(), (slice<byte>)"&;", (slice<byte>)"&amp;;", -1); 

    // Cleanup for embedding by dropping stuff before the <svg> start.
    {
        var pos = bytes.Index(svg, (slice<byte>)"<svg");

        if (pos >= 0) {
            svg = svg[(int)pos..];
        }
    }

    return (svg, error.As(null!)!);

}

private static void top(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req) {
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;

    var (rpt, errList) = ui.makeReport(w, req, new slice<@string>(new @string[] { "top" }), cfg => {
        cfg.NodeCount = 500;
    });
    if (rpt == null) {
        return ; // error already reported
    }
    var (top, legend) = report.TextItems(rpt);
    slice<@string> nodes = default;
    foreach (var (_, item) in top) {
        nodes = append(nodes, item.Name);
    }    ui.render(w, req, "top", rpt, errList, legend, new webArgs(Top:top,Nodes:nodes,));

}

// disasm generates a web page containing disassembly.
private static void disasm(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req) {
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;

    @string args = new slice<@string>(new @string[] { "disasm", req.URL.Query().Get("f") });
    var (rpt, errList) = ui.makeReport(w, req, args, null);
    if (rpt == null) {
        return ; // error already reported
    }
    ptr<bytes.Buffer> @out = addr(new bytes.Buffer());
    {
        var err = report.PrintAssembly(out, rpt, ui.options.Obj, maxEntries);

        if (err != null) {
            http.Error(w, err.Error(), http.StatusBadRequest);
            ui.options.UI.PrintErr(err);
            return ;
        }
    }


    var legend = report.ProfileLabels(rpt);
    ui.render(w, req, "plaintext", rpt, errList, legend, new webArgs(TextBody:out.String(),));


}

// source generates a web page containing source code annotated with profile
// data.
private static void source(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req) {
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;

    @string args = new slice<@string>(new @string[] { "weblist", req.URL.Query().Get("f") });
    var (rpt, errList) = ui.makeReport(w, req, args, null);
    if (rpt == null) {
        return ; // error already reported
    }
    ref bytes.Buffer body = ref heap(out ptr<bytes.Buffer> _addr_body);
    {
        var err = report.PrintWebList(_addr_body, rpt, ui.options.Obj, maxEntries);

        if (err != null) {
            http.Error(w, err.Error(), http.StatusBadRequest);
            ui.options.UI.PrintErr(err);
            return ;
        }
    }


    var legend = report.ProfileLabels(rpt);
    ui.render(w, req, "sourcelisting", rpt, errList, legend, new webArgs(HTMLBody:template.HTML(body.String()),));

}

// peek generates a web page listing callers/callers.
private static void peek(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req) {
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;

    @string args = new slice<@string>(new @string[] { "peek", req.URL.Query().Get("f") });
    var (rpt, errList) = ui.makeReport(w, req, args, cfg => {
        cfg.Granularity = "lines";
    });
    if (rpt == null) {
        return ; // error already reported
    }
    ptr<bytes.Buffer> @out = addr(new bytes.Buffer());
    {
        var err = report.Generate(out, rpt, ui.options.Obj);

        if (err != null) {
            http.Error(w, err.Error(), http.StatusBadRequest);
            ui.options.UI.PrintErr(err);
            return ;
        }
    }


    var legend = report.ProfileLabels(rpt);
    ui.render(w, req, "plaintext", rpt, errList, legend, new webArgs(TextBody:out.String(),));

}

// saveConfig saves URL configuration.
private static void saveConfig(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req) {
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;

    {
        var err = setConfig(ui.settingsFile, req.URL.val);

        if (err != null) {
            http.Error(w, err.Error(), http.StatusBadRequest);
            ui.options.UI.PrintErr(err);
            return ;
        }
    }

}

// deleteConfig deletes a configuration.
private static void deleteConfig(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req) {
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;

    var name = req.URL.Query().Get("config");
    {
        var err = removeConfig(ui.settingsFile, name);

        if (err != null) {
            http.Error(w, err.Error(), http.StatusBadRequest);
            ui.options.UI.PrintErr(err);
            return ;
        }
    }

}

// getFromLegend returns the suffix of an entry in legend that starts
// with param.  It returns def if no such entry is found.
private static @string getFromLegend(slice<@string> legend, @string param, @string def) {
    foreach (var (_, s) in legend) {
        if (strings.HasPrefix(s, param)) {
            return s[(int)len(param)..];
        }
    }    return def;

}

} // end driver_package
