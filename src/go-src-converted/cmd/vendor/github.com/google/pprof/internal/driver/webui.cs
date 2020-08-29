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

// package driver -- go2cs converted at 2020 August 29 10:05:31 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\webui.go
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
using static go.builtin;
using System.Threading;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class driver_package
    {
        // webInterface holds the state needed for serving a browser based interface.
        private partial struct webInterface
        {
            public ptr<profile.Profile> prof;
            public ptr<plugin.Options> options;
            public map<@string, @string> help;
            public ptr<template.Template> templates;
        }

        private static ref webInterface makeWebInterface(ref profile.Profile p, ref plugin.Options opt)
        {
            var templates = template.New("templategroup");
            addTemplates(templates);
            report.AddSourceTemplates(templates);
            return ref new webInterface(prof:p,options:opt,help:make(map[string]string),templates:templates,);
        }

        // maxEntries is the maximum number of entries to print for text interfaces.
        private static readonly long maxEntries = 50L;

        // errorCatcher is a UI that captures errors for reporting to the browser.


        // errorCatcher is a UI that captures errors for reporting to the browser.
        private partial struct errorCatcher
        {
            public ref plugin.UI UI => ref UI_val;
            public slice<@string> errors;
        }

        private static void PrintErr(this ref errorCatcher ec, params object[] args)
        {
            ec.errors = append(ec.errors, strings.TrimSuffix(fmt.Sprintln(args), "\n"));
            ec.UI.PrintErr(args);
        }

        // webArgs contains arguments passed to templates in webhtml.go.
        private partial struct webArgs
        {
            public @string BaseURL;
            public @string Title;
            public slice<@string> Errors;
            public long Total;
            public slice<@string> Legend;
            public map<@string, @string> Help;
            public slice<@string> Nodes;
            public template.HTML HTMLBody;
            public @string TextBody;
            public slice<report.TextItem> Top;
        }

        private static error serveWebInterface(@string hostport, ref profile.Profile p, ref plugin.Options o)
        {
            var (host, portStr, err) = net.SplitHostPort(hostport);
            if (err != null)
            {
                return error.As(fmt.Errorf("could not split http address: %v", err));
            }
            var (port, err) = strconv.Atoi(portStr);
            if (err != null)
            {
                return error.As(fmt.Errorf("invalid port number: %v", err));
            }
            if (host == "")
            {
                host = "localhost";
            }
            interactiveMode = true;
            var ui = makeWebInterface(p, o);
            {
                var n__prev1 = n;

                foreach (var (__n, __c) in pprofCommands)
                {
                    n = __n;
                    c = __c;
                    ui.help[n] = c.description;
                }

                n = n__prev1;
            }

            {
                var n__prev1 = n;

                foreach (var (__n, __v) in pprofVariables)
                {
                    n = __n;
                    v = __v;
                    ui.help[n] = v.help;
                }

                n = n__prev1;
            }

            ui.help["details"] = "Show information about the profile and this view";
            ui.help["graph"] = "Display profile as a directed graph";
            ui.help["reset"] = "Show the entire profile";

            var server = o.HTTPServer;
            if (server == null)
            {
                server = defaultWebServer;
            }
            plugin.HTTPServerArgs args = ref new plugin.HTTPServerArgs(Hostport:net.JoinHostPort(host,portStr),Host:host,Port:port,Handlers:map[string]http.Handler{"/":http.HandlerFunc(ui.dot),"/top":http.HandlerFunc(ui.top),"/disasm":http.HandlerFunc(ui.disasm),"/source":http.HandlerFunc(ui.source),"/peek":http.HandlerFunc(ui.peek),},);

            go_(() => openBrowser("http://" + args.Hostport, o));
            return error.As(server(args));
        }

        private static error defaultWebServer(ref plugin.HTTPServerArgs args)
        {
            var (ln, err) = net.Listen("tcp", args.Hostport);
            if (err != null)
            {
                return error.As(err);
            }
            var isLocal = isLocalhost(args.Host);
            var handler = http.HandlerFunc((w, req) =>
            {
                if (isLocal)
                { 
                    // Only allow local clients
                    var (host, _, err) = net.SplitHostPort(req.RemoteAddr);
                    if (err != null || !isLocalhost(host))
                    {
                        http.Error(w, "permission denied", http.StatusForbidden);
                        return;
                    }
                }
                var h = args.Handlers[req.URL.Path];
                if (h == null)
                { 
                    // Fall back to default behavior
                    h = http.DefaultServeMux;
                }
                h.ServeHTTP(w, req);
            });
            http.Server s = ref new http.Server(Handler:handler);
            return error.As(s.Serve(ln));
        }

        private static bool isLocalhost(@string host)
        {
            foreach (var (_, v) in new slice<@string>(new @string[] { "localhost", "127.0.0.1", "[::1]", "::1" }))
            {
                if (host == v)
                {
                    return true;
                }
            }
            return false;
        }

        private static void openBrowser(@string url, ref plugin.Options o)
        { 
            // Construct URL.
            var (u, _) = gourl.Parse(url);
            var q = u.Query();
            foreach (var (_, p) in true)
            {
                {
                    var v = pprofVariables[p.key].value;

                    if (v != "")
                    {
                        q.Set(p.param, v);
                    }

                }
            }
            u.RawQuery = q.Encode(); 

            // Give server a little time to get ready.
            time.Sleep(time.Millisecond * 500L);

            foreach (var (_, b) in browsers())
            {
                var args = strings.Split(b, " ");
                if (len(args) == 0L)
                {
                    continue;
                }
                var viewer = exec.Command(args[0L], append(args[1L..], u.String()));
                viewer.Stderr = os.Stderr;
                {
                    var err = viewer.Start();

                    if (err == null)
                    {
                        return;
                    }

                }
            } 
            // No visualizer succeeded, so just print URL.
            o.UI.PrintErr(u.String());
        }

        private static variables varsFromURL(ref gourl.URL u)
        {
            var vars = pprofVariables.makeCopy();
            vars["focus"].value = u.Query().Get("f");
            vars["show"].value = u.Query().Get("s");
            vars["ignore"].value = u.Query().Get("i");
            vars["hide"].value = u.Query().Get("h");
            return vars;
        }

        // makeReport generates a report for the specified command.
        private static (ref report.Report, slice<@string>) makeReport(this ref webInterface ui, http.ResponseWriter w, ref http.Request req, slice<@string> cmd, params @string[] vars)
        {
            var v = varsFromURL(req.URL);
            {
                long i = 0L;

                while (i + 1L < len(vars))
                {
                    v[vars[i]].value = vars[i + 1L];
                    i += 2L;
                }

            }
            errorCatcher catcher = ref new errorCatcher(UI:ui.options.UI);
            var options = ui.options.Value;
            options.UI = catcher;
            var (_, rpt, err) = generateRawReport(ui.prof, cmd, v, ref options);
            if (err != null)
            {
                http.Error(w, err.Error(), http.StatusBadRequest);
                ui.options.UI.PrintErr(err);
                return (null, null);
            }
            return (rpt, catcher.errors);
        }

        // render generates html using the named template based on the contents of data.
        private static void render(this ref webInterface ui, http.ResponseWriter w, @string baseURL, @string tmpl, ref report.Report rpt, slice<@string> errList, slice<@string> legend, webArgs data)
        {
            var file = getFromLegend(legend, "File: ", "unknown");
            var profile = getFromLegend(legend, "Type: ", "unknown");
            data.BaseURL = baseURL;
            data.Title = file + " " + profile;
            data.Errors = errList;
            data.Total = rpt.Total();
            data.Legend = legend;
            data.Help = ui.help;
            bytes.Buffer html = ref new bytes.Buffer();
            {
                var err = ui.templates.ExecuteTemplate(html, tmpl, data);

                if (err != null)
                {
                    http.Error(w, "internal template error", http.StatusInternalServerError);
                    ui.options.UI.PrintErr(err);
                    return;
                }

            }
            w.Header().Set("Content-Type", "text/html");
            w.Write(html.Bytes());
        }

        // dot generates a web page containing an svg diagram.
        private static void dot(this ref webInterface ui, http.ResponseWriter w, ref http.Request req)
        {
            var (rpt, errList) = ui.makeReport(w, req, new slice<@string>(new @string[] { "svg" }));
            if (rpt == null)
            {
                return; // error already reported
            } 

            // Generate dot graph.
            var (g, config) = report.GetDOT(rpt);
            var legend = config.Labels;
            config.Labels = null;
            bytes.Buffer dot = ref new bytes.Buffer();
            graph.ComposeDot(dot, g, ref new graph.DotAttributes(), config); 

            // Convert to svg.
            var (svg, err) = dotToSvg(dot.Bytes());
            if (err != null)
            {
                http.Error(w, "Could not execute dot; may need to install graphviz.", http.StatusNotImplemented);
                ui.options.UI.PrintErr("Failed to execute dot. Is Graphviz installed?\n", err);
                return;
            } 

            // Get all node names into an array.
            @string nodes = new slice<@string>(new @string[] { "" }); // dot starts with node numbered 1
            foreach (var (_, n) in g.Nodes)
            {
                nodes = append(nodes, n.Info.Name);
            }
            ui.render(w, "/", "graph", rpt, errList, legend, new webArgs(HTMLBody:template.HTML(string(svg)),Nodes:nodes,));
        }

        private static (slice<byte>, error) dotToSvg(slice<byte> dot)
        {
            var cmd = exec.Command("dot", "-Tsvg");
            bytes.Buffer @out = ref new bytes.Buffer();
            cmd.Stdin = bytes.NewBuffer(dot);
            cmd.Stdout = out;
            cmd.Stderr = os.Stderr;
            {
                var err = cmd.Run();

                if (err != null)
                {
                    return (null, err);
                } 

                // Fix dot bug related to unquoted amperands.

            } 

            // Fix dot bug related to unquoted amperands.
            var svg = bytes.Replace(@out.Bytes(), (slice<byte>)"&;", (slice<byte>)"&amp;;", -1L); 

            // Cleanup for embedding by dropping stuff before the <svg> start.
            {
                var pos = bytes.Index(svg, (slice<byte>)"<svg");

                if (pos >= 0L)
                {
                    svg = svg[pos..];
                }

            }
            return (svg, null);
        }

        private static void top(this ref webInterface ui, http.ResponseWriter w, ref http.Request req)
        {
            var (rpt, errList) = ui.makeReport(w, req, new slice<@string>(new @string[] { "top" }), "nodecount", "500");
            if (rpt == null)
            {
                return; // error already reported
            }
            var (top, legend) = report.TextItems(rpt);
            slice<@string> nodes = default;
            foreach (var (_, item) in top)
            {
                nodes = append(nodes, item.Name);
            }
            ui.render(w, "/top", "top", rpt, errList, legend, new webArgs(Top:top,Nodes:nodes,));
        }

        // disasm generates a web page containing disassembly.
        private static void disasm(this ref webInterface ui, http.ResponseWriter w, ref http.Request req)
        {
            @string args = new slice<@string>(new @string[] { "disasm", req.URL.Query().Get("f") });
            var (rpt, errList) = ui.makeReport(w, req, args);
            if (rpt == null)
            {
                return; // error already reported
            }
            bytes.Buffer @out = ref new bytes.Buffer();
            {
                var err = report.PrintAssembly(out, rpt, ui.options.Obj, maxEntries);

                if (err != null)
                {
                    http.Error(w, err.Error(), http.StatusBadRequest);
                    ui.options.UI.PrintErr(err);
                    return;
                }

            }

            var legend = report.ProfileLabels(rpt);
            ui.render(w, "/disasm", "plaintext", rpt, errList, legend, new webArgs(TextBody:out.String(),));

        }

        // source generates a web page containing source code annotated with profile
        // data.
        private static void source(this ref webInterface ui, http.ResponseWriter w, ref http.Request req)
        {
            @string args = new slice<@string>(new @string[] { "weblist", req.URL.Query().Get("f") });
            var (rpt, errList) = ui.makeReport(w, req, args);
            if (rpt == null)
            {
                return; // error already reported
            } 

            // Generate source listing.
            bytes.Buffer body = default;
            {
                var err = report.PrintWebList(ref body, rpt, ui.options.Obj, maxEntries);

                if (err != null)
                {
                    http.Error(w, err.Error(), http.StatusBadRequest);
                    ui.options.UI.PrintErr(err);
                    return;
                }

            }

            var legend = report.ProfileLabels(rpt);
            ui.render(w, "/source", "sourcelisting", rpt, errList, legend, new webArgs(HTMLBody:template.HTML(body.String()),));
        }

        // peek generates a web page listing callers/callers.
        private static void peek(this ref webInterface ui, http.ResponseWriter w, ref http.Request req)
        {
            @string args = new slice<@string>(new @string[] { "peek", req.URL.Query().Get("f") });
            var (rpt, errList) = ui.makeReport(w, req, args, "lines", "t");
            if (rpt == null)
            {
                return; // error already reported
            }
            bytes.Buffer @out = ref new bytes.Buffer();
            {
                var err = report.Generate(out, rpt, ui.options.Obj);

                if (err != null)
                {
                    http.Error(w, err.Error(), http.StatusBadRequest);
                    ui.options.UI.PrintErr(err);
                    return;
                }

            }

            var legend = report.ProfileLabels(rpt);
            ui.render(w, "/peek", "plaintext", rpt, errList, legend, new webArgs(TextBody:out.String(),));
        }

        // getFromLegend returns the suffix of an entry in legend that starts
        // with param.  It returns def if no such entry is found.
        private static @string getFromLegend(slice<@string> legend, @string param, @string def)
        {
            foreach (var (_, s) in legend)
            {
                if (strings.HasPrefix(s, param))
                {
                    return s[len(param)..];
                }
            }
            return def;
        }
    }
}}}}}}}
