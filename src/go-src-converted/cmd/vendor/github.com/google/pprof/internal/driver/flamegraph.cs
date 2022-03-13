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

// package driver -- go2cs converted at 2022 March 13 06:36:32 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\flamegraph.go
namespace go.cmd.vendor.github.com.google.pprof.@internal;

using json = encoding.json_package;
using template = html.template_package;
using http = net.http_package;
using strings = strings_package;

using graph = github.com.google.pprof.@internal.graph_package;
using measurement = github.com.google.pprof.@internal.measurement_package;
using report = github.com.google.pprof.@internal.report_package;
using System.ComponentModel;
using System;

public static partial class driver_package {

private partial struct treeNode {
    [Description("json:\"n\"")]
    public @string Name;
    [Description("json:\"f\"")]
    public @string FullName;
    [Description("json:\"v\"")]
    public long Cum;
    [Description("json:\"l\"")]
    public @string CumFormat;
    [Description("json:\"p\"")]
    public @string Percent;
    [Description("json:\"c\"")]
    public slice<ptr<treeNode>> Children;
}

// flamegraph generates a web page containing a flamegraph.
private static void flamegraph(this ptr<webInterface> _addr_ui, http.ResponseWriter w, ptr<http.Request> _addr_req) {
    ref webInterface ui = ref _addr_ui.val;
    ref http.Request req = ref _addr_req.val;
 
    // Force the call tree so that the graph is a tree.
    // Also do not trim the tree so that the flame graph contains all functions.
    var (rpt, errList) = ui.makeReport(w, req, new slice<@string>(new @string[] { "svg" }), cfg => {
        cfg.CallTree = true;
        cfg.Trim = false;
    });
    if (rpt == null) {
        return ; // error already reported
    }
    var (g, config) = report.GetDOT(rpt);
    slice<ptr<treeNode>> nodes = default;
    nint nroots = 0;
    var rootValue = int64(0);
    @string nodeArr = new slice<@string>(new @string[] {  });
    map nodeMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<graph.Node>, ptr<treeNode>>{}; 
    // Make all nodes and the map, collect the roots.
    {
        var n__prev1 = n;

        foreach (var (_, __n) in g.Nodes) {
            n = __n;
            var v = n.CumValue();
            var fullName = n.Info.PrintableName();
            ptr<treeNode> node = addr(new treeNode(Name:graph.ShortenFunctionName(fullName),FullName:fullName,Cum:v,CumFormat:config.FormatValue(v),Percent:strings.TrimSpace(measurement.Percentage(v,config.Total)),));
            nodes = append(nodes, node);
            if (len(n.In) == 0) {
                (nodes[nroots], nodes[len(nodes) - 1]) = (nodes[len(nodes) - 1], nodes[nroots]);                nroots++;
                rootValue += v;
            }
            nodeMap[n] = node; 
            // Get all node names into an array.
            nodeArr = append(nodeArr, n.Info.Name);
        }
        n = n__prev1;
    }

    {
        var n__prev1 = n;

        foreach (var (_, __n) in g.Nodes) {
            n = __n;
            node = nodeMap[n];
            foreach (var (child) in n.Out) {
                node.Children = append(node.Children, nodeMap[child]);
            }
        }
        n = n__prev1;
    }

    ptr<treeNode> rootNode = addr(new treeNode(Name:"root",FullName:"root",Cum:rootValue,CumFormat:config.FormatValue(rootValue),Percent:strings.TrimSpace(measurement.Percentage(rootValue,config.Total)),Children:nodes[0:nroots],)); 

    // JSON marshalling flame graph
    var (b, err) = json.Marshal(rootNode);
    if (err != null) {
        http.Error(w, "error serializing flame graph", http.StatusInternalServerError);
        ui.options.UI.PrintErr(err);
        return ;
    }
    ui.render(w, req, "flamegraph", rpt, errList, config.Labels, new webArgs(FlameGraph:template.JS(b),Nodes:nodeArr,));
}

} // end driver_package
