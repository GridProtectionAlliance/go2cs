// Copyright 2014 Google Inc. All Rights Reserved.
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

// Package graph collects a set of samples into a directed graph.
// package graph -- go2cs converted at 2020 August 29 10:05:40 UTC
// import "cmd/vendor/github.com/google/pprof/internal/graph" ==> using graph = go.cmd.vendor.github.com.google.pprof.@internal.graph_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\graph\graph.go
using fmt = go.fmt_package;
using math = go.math_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using profile = go.github.com.google.pprof.profile_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class graph_package
    {
        // Graph summarizes a performance profile into a format that is
        // suitable for visualization.
        public partial struct Graph
        {
            public Nodes Nodes;
        }

        // Options encodes the options for constructing a graph
        public partial struct Options
        {
            public Func<slice<long>, long> SampleValue; // Function to compute the value of a sample
            public Func<slice<long>, long> SampleMeanDivisor; // Function to compute the divisor for mean graphs, or nil
            public Func<long, @string, @string> FormatTag; // Function to format a sample tag value into a string
            public bool ObjNames; // Always preserve obj filename
            public bool OrigFnNames; // Preserve original (eg mangled) function names

            public bool CallTree; // Build a tree instead of a graph
            public bool DropNegative; // Drop nodes with overall negative values

            public NodeSet KeptNodes; // If non-nil, only use nodes in this set
        }

        // Nodes is an ordered collection of graph nodes.
        public partial struct Nodes // : slice<ref Node>
        {
        }

        // Node is an entry on a profiling report. It represents a unique
        // program location.
        public partial struct Node
        {
            public NodeInfo Info; // Function represents the function that this node belongs to. On
// graphs with sub-function resolution (eg line number or
// addresses), two nodes in a NodeMap that are part of the same
// function have the same value of Node.Function. If the Node
// represents the whole function, it points back to itself.
            public ptr<Node> Function; // Values associated to this node. Flat is exclusive to this node,
// Cum includes all descendents.
            public long Flat; // In and out Contains the nodes immediately reaching or reached by
// this node.
            public long FlatDiv; // In and out Contains the nodes immediately reaching or reached by
// this node.
            public long Cum; // In and out Contains the nodes immediately reaching or reached by
// this node.
            public long CumDiv; // In and out Contains the nodes immediately reaching or reached by
// this node.
            public EdgeMap In; // LabelTags provide additional information about subsets of a sample.
            public EdgeMap Out; // LabelTags provide additional information about subsets of a sample.
            public TagMap LabelTags; // NumericTags provide additional values for subsets of a sample.
// Numeric tags are optionally associated to a label tag. The key
// for NumericTags is the name of the LabelTag they are associated
// to, or "" for numeric tags not associated to a label tag.
            public map<@string, TagMap> NumericTags;
        }

        // FlatValue returns the exclusive value for this node, computing the
        // mean if a divisor is available.
        private static long FlatValue(this ref Node n)
        {
            if (n.FlatDiv == 0L)
            {
                return n.Flat;
            }
            return n.Flat / n.FlatDiv;
        }

        // CumValue returns the inclusive value for this node, computing the
        // mean if a divisor is available.
        private static long CumValue(this ref Node n)
        {
            if (n.CumDiv == 0L)
            {
                return n.Cum;
            }
            return n.Cum / n.CumDiv;
        }

        // AddToEdge increases the weight of an edge between two nodes. If
        // there isn't such an edge one is created.
        private static void AddToEdge(this ref Node n, ref Node to, long v, bool residual, bool inline)
        {
            n.AddToEdgeDiv(to, 0L, v, residual, inline);
        }

        // AddToEdgeDiv increases the weight of an edge between two nodes. If
        // there isn't such an edge one is created.
        private static void AddToEdgeDiv(this ref Node _n, ref Node _to, long dv, long v, bool residual, bool inline) => func(_n, _to, (ref Node n, ref Node to, Defer _, Panic panic, Recover __) =>
        {
            if (n.Out[to] != to.In[n])
            {
                panic(fmt.Errorf("asymmetric edges %v %v", n.Value, to.Value));
            }
            {
                var e = n.Out[to];

                if (e != null)
                {
                    e.WeightDiv += dv;
                    e.Weight += v;
                    if (residual)
                    {
                        e.Residual = true;
                    }
                    if (!inline)
                    {
                        e.Inline = false;
                    }
                    return;
                }

            }

            Edge info = ref new Edge(Src:n,Dest:to,WeightDiv:dv,Weight:v,Residual:residual,Inline:inline);
            n.Out[to] = info;
            to.In[n] = info;
        });

        // NodeInfo contains the attributes for a node.
        public partial struct NodeInfo
        {
            public @string Name;
            public @string OrigName;
            public ulong Address;
            public @string File;
            public long StartLine;
            public long Lineno;
            public @string Objfile;
        }

        // PrintableName calls the Node's Formatter function with a single space separator.
        private static @string PrintableName(this ref NodeInfo i)
        {
            return strings.Join(i.NameComponents(), " ");
        }

        // NameComponents returns the components of the printable name to be used for a node.
        private static slice<@string> NameComponents(this ref NodeInfo i)
        {
            slice<@string> name = default;
            if (i.Address != 0L)
            {
                name = append(name, fmt.Sprintf("%016x", i.Address));
            }
            {
                var fun = i.Name;

                if (fun != "")
                {
                    name = append(name, fun);
                }

            }


            if (i.Lineno != 0L) 
                // User requested line numbers, provide what we have.
                name = append(name, fmt.Sprintf("%s:%d", i.File, i.Lineno));
            else if (i.File != "") 
                // User requested file name, provide it.
                name = append(name, i.File);
            else if (i.Name != "")             else if (i.Objfile != "") 
                // Only binary name is available
                name = append(name, "[" + filepath.Base(i.Objfile) + "]");
            else 
                // Do not leave it empty if there is no information at all.
                name = append(name, "<unknown>");
                        return name;
        }

        // NodeMap maps from a node info struct to a node. It is used to merge
        // report entries with the same info.
        public partial struct NodeMap // : map<NodeInfo, ref Node>
        {
        }

        // NodeSet is a collection of node info structs.
        public partial struct NodeSet // : map<NodeInfo, bool>
        {
        }

        // NodePtrSet is a collection of nodes. Trimming a graph or tree requires a set
        // of objects which uniquely identify the nodes to keep. In a graph, NodeInfo
        // works as a unique identifier; however, in a tree multiple nodes may share
        // identical NodeInfos. A *Node does uniquely identify a node so we can use that
        // instead. Though a *Node also uniquely identifies a node in a graph,
        // currently, during trimming, graphs are rebult from scratch using only the
        // NodeSet, so there would not be the required context of the initial graph to
        // allow for the use of *Node.
        public partial struct NodePtrSet // : map<ref Node, bool>
        {
        }

        // FindOrInsertNode takes the info for a node and either returns a matching node
        // from the node map if one exists, or adds one to the map if one does not.
        // If kept is non-nil, nodes are only added if they can be located on it.
        public static ref Node FindOrInsertNode(this NodeMap nm, NodeInfo info, NodeSet kept)
        {
            if (kept != null)
            {
                {
                    var (_, ok) = kept[info];

                    if (!ok)
                    {
                        return null;
                    }

                }
            }
            {
                var n__prev1 = n;

                var (n, ok) = nm[info];

                if (ok)
                {
                    return n;
                }

                n = n__prev1;

            }

            Node n = ref new Node(Info:info,In:make(EdgeMap),Out:make(EdgeMap),LabelTags:make(TagMap),NumericTags:make(map[string]TagMap),);
            nm[info] = n;
            if (info.Address == 0L && info.Lineno == 0L)
            { 
                // This node represents the whole function, so point Function
                // back to itself.
                n.Function = n;
                return n;
            } 
            // Find a node that represents the whole function.
            info.Address = 0L;
            info.Lineno = 0L;
            n.Function = nm.FindOrInsertNode(info, null);
            return n;
        }

        // EdgeMap is used to represent the incoming/outgoing edges from a node.
        public partial struct EdgeMap // : map<ref Node, ref Edge>
        {
        }

        // Edge contains any attributes to be represented about edges in a graph.
        public partial struct Edge
        {
            public ptr<Node> Src; // The summary weight of the edge
            public ptr<Node> Dest; // The summary weight of the edge
            public long Weight; // residual edges connect nodes that were connected through a
// separate node, which has been removed from the report.
            public long WeightDiv; // residual edges connect nodes that were connected through a
// separate node, which has been removed from the report.
            public bool Residual; // An inline edge represents a call that was inlined into the caller.
            public bool Inline;
        }

        // WeightValue returns the weight value for this edge, normalizing if a
        // divisor is available.
        private static long WeightValue(this ref Edge e)
        {
            if (e.WeightDiv == 0L)
            {
                return e.Weight;
            }
            return e.Weight / e.WeightDiv;
        }

        // Tag represent sample annotations
        public partial struct Tag
        {
            public @string Name;
            public @string Unit; // Describe the value, "" for non-numeric tags
            public long Value;
            public long Flat;
            public long FlatDiv;
            public long Cum;
            public long CumDiv;
        }

        // FlatValue returns the exclusive value for this tag, computing the
        // mean if a divisor is available.
        private static long FlatValue(this ref Tag t)
        {
            if (t.FlatDiv == 0L)
            {
                return t.Flat;
            }
            return t.Flat / t.FlatDiv;
        }

        // CumValue returns the inclusive value for this tag, computing the
        // mean if a divisor is available.
        private static long CumValue(this ref Tag t)
        {
            if (t.CumDiv == 0L)
            {
                return t.Cum;
            }
            return t.Cum / t.CumDiv;
        }

        // TagMap is a collection of tags, classified by their name.
        public partial struct TagMap // : map<@string, ref Tag>
        {
        }

        // SortTags sorts a slice of tags based on their weight.
        public static slice<ref Tag> SortTags(slice<ref Tag> t, bool flat)
        {
            tags ts = new tags(t,flat);
            sort.Sort(ts);
            return ts.t;
        }

        // New summarizes performance data from a profile into a graph.
        public static ref Graph New(ref profile.Profile prof, ref Options o)
        {
            if (o.CallTree)
            {
                return newTree(prof, o);
            }
            var (g, _) = newGraph(prof, o);
            return g;
        }

        // newGraph computes a graph from a profile. It returns the graph, and
        // a map from the profile location indices to the corresponding graph
        // nodes.
        private static (ref Graph, map<ulong, Nodes>) newGraph(ref profile.Profile prof, ref Options o)
        {
            var (nodes, locationMap) = CreateNodes(prof, o);
            foreach (var (_, sample) in prof.Sample)
            {
                long w = default;                long dw = default;

                w = o.SampleValue(sample.Value);
                if (o.SampleMeanDivisor != null)
                {
                    dw = o.SampleMeanDivisor(sample.Value);
                }
                if (dw == 0L && w == 0L)
                {
                    continue;
                }
                var seenNode = make_map<ref Node, bool>(len(sample.Location));
                var seenEdge = make_map<nodePair, bool>(len(sample.Location));
                ref Node parent = default; 
                // A residual edge goes over one or more nodes that were not kept.
                var residual = false;

                var labels = joinLabels(sample); 
                // Group the sample frames, based on a global map.
                for (var i = len(sample.Location) - 1L; i >= 0L; i--)
                {
                    var l = sample.Location[i];
                    var locNodes = locationMap[l.ID];
                    for (var ni = len(locNodes) - 1L; ni >= 0L; ni--)
                    {
                        var n = locNodes[ni];
                        if (n == null)
                        {
                            residual = true;
                            continue;
                        } 
                        // Add cum weight to all nodes in stack, avoiding double counting.
                        {
                            var (_, ok) = seenNode[n];

                            if (!ok)
                            {
                                seenNode[n] = true;
                                n.addSample(dw, w, labels, sample.NumLabel, sample.NumUnit, o.FormatTag, false);
                            } 
                            // Update edge weights for all edges in stack, avoiding double counting.

                        } 
                        // Update edge weights for all edges in stack, avoiding double counting.
                        {
                            (_, ok) = seenEdge[new nodePair(n,parent)];

                            if (!ok && parent != null && n != parent)
                            {
                                seenEdge[new nodePair(n,parent)] = true;
                                parent.AddToEdgeDiv(n, dw, w, residual, ni != len(locNodes) - 1L);
                            }

                        }
                        parent = n;
                        residual = false;
                    }

                }

                if (parent != null && !residual)
                { 
                    // Add flat weight to leaf node.
                    parent.addSample(dw, w, labels, sample.NumLabel, sample.NumUnit, o.FormatTag, true);
                }
            }
            return (selectNodesForGraph(nodes, o.DropNegative), locationMap);
        }

        private static ref Graph selectNodesForGraph(Nodes nodes, bool dropNegative)
        { 
            // Collect nodes into a graph.
            var gNodes = make(Nodes, 0L, len(nodes));
            foreach (var (_, n) in nodes)
            {
                if (n == null)
                {
                    continue;
                }
                if (n.Cum == 0L && n.Flat == 0L)
                {
                    continue;
                }
                if (dropNegative && isNegative(n))
                {
                    continue;
                }
                gNodes = append(gNodes, n);
            }
            return ref new Graph(gNodes);
        }

        private partial struct nodePair
        {
            public ptr<Node> src;
            public ptr<Node> dest;
        }

        private static ref Graph newTree(ref profile.Profile prof, ref Options o)
        {
            var parentNodeMap = make_map<ref Node, NodeMap>(len(prof.Sample));
            foreach (var (_, sample) in prof.Sample)
            {
                long w = default;                long dw = default;

                w = o.SampleValue(sample.Value);
                if (o.SampleMeanDivisor != null)
                {
                    dw = o.SampleMeanDivisor(sample.Value);
                }
                if (dw == 0L && w == 0L)
                {
                    continue;
                }
                ref Node parent = default;
                var labels = joinLabels(sample); 
                // Group the sample frames, based on a per-node map.
                for (var i = len(sample.Location) - 1L; i >= 0L; i--)
                {
                    var l = sample.Location[i];
                    var lines = l.Line;
                    if (len(lines) == 0L)
                    {
                        lines = new slice<profile.Line>(new profile.Line[] { {} }); // Create empty line to include location info.
                    }
                    for (var lidx = len(lines) - 1L; lidx >= 0L; lidx--)
                    {
                        var nodeMap = parentNodeMap[parent];
                        if (nodeMap == null)
                        {
                            nodeMap = make(NodeMap);
                            parentNodeMap[parent] = nodeMap;
                        }
                        var n = nodeMap.findOrInsertLine(l, lines[lidx], o);
                        if (n == null)
                        {
                            continue;
                        }
                        n.addSample(dw, w, labels, sample.NumLabel, sample.NumUnit, o.FormatTag, false);
                        if (parent != null)
                        {
                            parent.AddToEdgeDiv(n, dw, w, false, lidx != len(lines) - 1L);
                        }
                        parent = n;
                    }

                }

                if (parent != null)
                {
                    parent.addSample(dw, w, labels, sample.NumLabel, sample.NumUnit, o.FormatTag, true);
                }
            }
            var nodes = make(Nodes, len(prof.Location));
            foreach (var (_, nm) in parentNodeMap)
            {
                nodes = append(nodes, nm.nodes());
            }
            return selectNodesForGraph(nodes, o.DropNegative);
        }

        // TrimTree trims a Graph in forest form, keeping only the nodes in kept. This
        // will not work correctly if even a single node has multiple parents.
        private static void TrimTree(this ref Graph _g, NodePtrSet kept) => func(_g, (ref Graph g, Defer _, Panic panic, Recover __) =>
        { 
            // Creates a new list of nodes
            var oldNodes = g.Nodes;
            g.Nodes = make(Nodes, 0L, len(kept));

            foreach (var (_, cur) in oldNodes)
            { 
                // A node may not have multiple parents
                if (len(cur.In) > 1L)
                {
                    panic("TrimTree only works on trees");
                } 

                // If a node should be kept, add it to the new list of nodes
                {
                    var (_, ok) = kept[cur];

                    if (ok)
                    {
                        g.Nodes = append(g.Nodes, cur);
                        continue;
                    } 

                    // If a node has no parents, then delete all of the in edges of its
                    // children to make them each roots of their own trees.

                } 

                // If a node has no parents, then delete all of the in edges of its
                // children to make them each roots of their own trees.
                if (len(cur.In) == 0L)
                {
                    {
                        var outEdge__prev2 = outEdge;

                        foreach (var (_, __outEdge) in cur.Out)
                        {
                            outEdge = __outEdge;
                            delete(outEdge.Dest.In, cur);
                        }

                        outEdge = outEdge__prev2;
                    }

                    continue;
                } 

                // Get the parent. This works since at this point cur.In must contain only
                // one element.
                if (len(cur.In) != 1L)
                {
                    panic("Get parent assertion failed. cur.In expected to be of length 1.");
                }
                ref Node parent = default;
                foreach (var (_, edge) in cur.In)
                {
                    parent = edge.Src;
                }
                var parentEdgeInline = parent.Out[cur].Inline; 

                // Remove the edge from the parent to this node
                delete(parent.Out, cur); 

                // Reconfigure every edge from the current node to now begin at the parent.
                {
                    var outEdge__prev2 = outEdge;

                    foreach (var (_, __outEdge) in cur.Out)
                    {
                        outEdge = __outEdge;
                        var child = outEdge.Dest;

                        delete(child.In, cur);
                        child.In[parent] = outEdge;
                        parent.Out[child] = outEdge;

                        outEdge.Src = parent;
                        outEdge.Residual = true; 
                        // If the edge from the parent to the current node and the edge from the
                        // current node to the child are both inline, then this resulting residual
                        // edge should also be inline
                        outEdge.Inline = parentEdgeInline && outEdge.Inline;
                    }

                    outEdge = outEdge__prev2;
                }

            }
            g.RemoveRedundantEdges();
        });

        private static @string joinLabels(ref profile.Sample s)
        {
            if (len(s.Label) == 0L)
            {
                return "";
            }
            slice<@string> labels = default;
            foreach (var (key, vals) in s.Label)
            {
                foreach (var (_, v) in vals)
                {
                    labels = append(labels, key + ":" + v);
                }
            }
            sort.Strings(labels);
            return strings.Join(labels, "\\n");
        }

        // isNegative returns true if the node is considered as "negative" for the
        // purposes of drop_negative.
        private static bool isNegative(ref Node n)
        {

            if (n.Flat < 0L) 
                return true;
            else if (n.Flat == 0L && n.Cum < 0L) 
                return true;
            else 
                return false;
                    }

        // CreateNodes creates graph nodes for all locations in a profile. It
        // returns set of all nodes, plus a mapping of each location to the
        // set of corresponding nodes (one per location.Line). If kept is
        // non-nil, only nodes in that set are included; nodes that do not
        // match are represented as a nil.
        public static (Nodes, map<ulong, Nodes>) CreateNodes(ref profile.Profile prof, ref Options o)
        {
            var locations = make_map<ulong, Nodes>(len(prof.Location));
            var nm = make(NodeMap, len(prof.Location));
            foreach (var (_, l) in prof.Location)
            {
                var lines = l.Line;
                if (len(lines) == 0L)
                {
                    lines = new slice<profile.Line>(new profile.Line[] { {} }); // Create empty line to include location info.
                }
                var nodes = make(Nodes, len(lines));
                foreach (var (ln) in lines)
                {
                    nodes[ln] = nm.findOrInsertLine(l, lines[ln], o);
                }
                locations[l.ID] = nodes;
            }
            return (nm.nodes(), locations);
        }

        public static Nodes nodes(this NodeMap nm)
        {
            var nodes = make(Nodes, 0L, len(nm));
            foreach (var (_, n) in nm)
            {
                nodes = append(nodes, n);
            }
            return nodes;
        }

        public static ref Node findOrInsertLine(this NodeMap nm, ref profile.Location l, profile.Line li, ref Options o)
        {
            @string objfile = default;
            {
                var m = l.Mapping;

                if (m != null && m.File != "")
                {
                    objfile = m.File;
                }

            }

            {
                var ni = nodeInfo(l, li, objfile, o);

                if (ni != null)
                {
                    return nm.FindOrInsertNode(ni.Value, o.KeptNodes);
                }

            }
            return null;
        }

        private static ref NodeInfo nodeInfo(ref profile.Location l, profile.Line line, @string objfile, ref Options o)
        {
            if (line.Function == null)
            {
                return ref new NodeInfo(Address:l.Address,Objfile:objfile);
            }
            NodeInfo ni = ref new NodeInfo(Address:l.Address,Lineno:int(line.Line),Name:line.Function.Name,);
            {
                var fname = line.Function.Filename;

                if (fname != "")
                {
                    ni.File = filepath.Clean(fname);
                }

            }
            if (o.ObjNames)
            {
                ni.Objfile = objfile;
                ni.StartLine = int(line.Function.StartLine);
            }
            if (o.OrigFnNames)
            {
                ni.OrigName = line.Function.SystemName;
            }
            return ni;
        }

        private partial struct tags
        {
            public slice<ref Tag> t;
            public bool flat;
        }

        private static long Len(this tags t)
        {
            return len(t.t);
        }
        private static void Swap(this tags t, long i, long j)
        {
            t.t[i] = t.t[j];
            t.t[j] = t.t[i];

        }
        private static bool Less(this tags t, long i, long j)
        {
            if (!t.flat)
            {
                if (t.t[i].Cum != t.t[j].Cum)
                {
                    return abs64(t.t[i].Cum) > abs64(t.t[j].Cum);
                }
            }
            if (t.t[i].Flat != t.t[j].Flat)
            {
                return abs64(t.t[i].Flat) > abs64(t.t[j].Flat);
            }
            return t.t[i].Name < t.t[j].Name;
        }

        // Sum adds the flat and cum values of a set of nodes.
        public static (long, long) Sum(this Nodes ns)
        {
            foreach (var (_, n) in ns)
            {
                flat += n.Flat;
                cum += n.Cum;
            }
            return;
        }

        private static @string addSample(this ref Node n, long dw, long w, @string labels, map<@string, slice<long>> numLabel, map<@string, slice<@string>> numUnit, Func<long, @string, @string> format, bool flat)
        { 
            // Update sample value
            if (flat)
            {
                n.FlatDiv += dw;
                n.Flat += w;
            }
            else
            {
                n.CumDiv += dw;
                n.Cum += w;
            } 

            // Add string tags
            if (labels != "")
            {
                var t = n.LabelTags.findOrAddTag(labels, "", 0L);
                if (flat)
                {
                    t.FlatDiv += dw;
                    t.Flat += w;
                }
                else
                {
                    t.CumDiv += dw;
                    t.Cum += w;
                }
            }
            var numericTags = n.NumericTags[labels];
            if (numericTags == null)
            {
                numericTags = new TagMap();
                n.NumericTags[labels] = numericTags;
            } 
            // Add numeric tags
            if (format == null)
            {
                format = defaultLabelFormat;
            }
            foreach (var (k, nvals) in numLabel)
            {
                var units = numUnit[k];
                foreach (var (i, v) in nvals)
                {
                    t = default;
                    if (len(units) > 0L)
                    {
                        t = numericTags.findOrAddTag(format(v, units[i]), units[i], v);
                    }
                    else
                    {
                        t = numericTags.findOrAddTag(format(v, k), k, v);
                    }
                    if (flat)
                    {
                        t.FlatDiv += dw;
                        t.Flat += w;
                    }
                    else
                    {
                        t.CumDiv += dw;
                        t.Cum += w;
                    }
                }
            }
        }

        private static @string defaultLabelFormat(long v, @string key)
        {
            return strconv.FormatInt(v, 10L);
        }

        public static ref Tag findOrAddTag(this TagMap m, @string label, @string unit, long value)
        {
            var l = m[label];
            if (l == null)
            {
                l = ref new Tag(Name:label,Unit:unit,Value:value,);
                m[label] = l;
            }
            return l;
        }

        // String returns a text representation of a graph, for debugging purposes.
        private static @string String(this ref Graph g)
        {
            slice<@string> s = default;

            var nodeIndex = make_map<ref Node, long>(len(g.Nodes));

            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in g.Nodes)
                {
                    i = __i;
                    n = __n;
                    nodeIndex[n] = i + 1L;
                }

                i = i__prev1;
                n = n__prev1;
            }

            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in g.Nodes)
                {
                    i = __i;
                    n = __n;
                    var name = n.Info.PrintableName();
                    slice<long> @in = default;                    slice<long> @out = default;



                    foreach (var (_, from) in n.In)
                    {
                        in = append(in, nodeIndex[from.Src]);
                    }
                    foreach (var (_, to) in n.Out)
                    {
                        out = append(out, nodeIndex[to.Dest]);
                    }
                    s = append(s, fmt.Sprintf("%d: %s[flat=%d cum=%d] %x -> %v ", i + 1L, name, n.Flat, n.Cum, in, out));
                }

                i = i__prev1;
                n = n__prev1;
            }

            return strings.Join(s, "\n");
        }

        // DiscardLowFrequencyNodes returns a set of the nodes at or over a
        // specific cum value cutoff.
        private static NodeSet DiscardLowFrequencyNodes(this ref Graph g, long nodeCutoff)
        {
            return makeNodeSet(g.Nodes, nodeCutoff);
        }

        // DiscardLowFrequencyNodePtrs returns a NodePtrSet of nodes at or over a
        // specific cum value cutoff.
        private static NodePtrSet DiscardLowFrequencyNodePtrs(this ref Graph g, long nodeCutoff)
        {
            var cutNodes = getNodesAboveCumCutoff(g.Nodes, nodeCutoff);
            var kept = make(NodePtrSet, len(cutNodes));
            foreach (var (_, n) in cutNodes)
            {
                kept[n] = true;
            }
            return kept;
        }

        private static NodeSet makeNodeSet(Nodes nodes, long nodeCutoff)
        {
            var cutNodes = getNodesAboveCumCutoff(nodes, nodeCutoff);
            var kept = make(NodeSet, len(cutNodes));
            foreach (var (_, n) in cutNodes)
            {
                kept[n.Info] = true;
            }
            return kept;
        }

        // getNodesAboveCumCutoff returns all the nodes which have a Cum value greater
        // than or equal to cutoff.
        private static Nodes getNodesAboveCumCutoff(Nodes nodes, long nodeCutoff)
        {
            var cutoffNodes = make(Nodes, 0L, len(nodes));
            foreach (var (_, n) in nodes)
            {
                if (abs64(n.Cum) < nodeCutoff)
                {
                    continue;
                }
                cutoffNodes = append(cutoffNodes, n);
            }
            return cutoffNodes;
        }

        // TrimLowFrequencyTags removes tags that have less than
        // the specified weight.
        private static void TrimLowFrequencyTags(this ref Graph g, long tagCutoff)
        { 
            // Remove nodes with value <= total*nodeFraction
            foreach (var (_, n) in g.Nodes)
            {
                n.LabelTags = trimLowFreqTags(n.LabelTags, tagCutoff);
                foreach (var (s, nt) in n.NumericTags)
                {
                    n.NumericTags[s] = trimLowFreqTags(nt, tagCutoff);
                }
            }
        }

        private static TagMap trimLowFreqTags(TagMap tags, long minValue)
        {
            TagMap kept = new TagMap();
            foreach (var (s, t) in tags)
            {
                if (abs64(t.Flat) >= minValue || abs64(t.Cum) >= minValue)
                {
                    kept[s] = t;
                }
            }
            return kept;
        }

        // TrimLowFrequencyEdges removes edges that have less than
        // the specified weight. Returns the number of edges removed
        private static long TrimLowFrequencyEdges(this ref Graph g, long edgeCutoff)
        {
            long droppedEdges = default;
            foreach (var (_, n) in g.Nodes)
            {
                foreach (var (src, e) in n.In)
                {
                    if (abs64(e.Weight) < edgeCutoff)
                    {
                        delete(n.In, src);
                        delete(src.Out, n);
                        droppedEdges++;
                    }
                }
            }
            return droppedEdges;
        }

        // SortNodes sorts the nodes in a graph based on a specific heuristic.
        private static void SortNodes(this ref Graph g, bool cum, bool visualMode)
        { 
            // Sort nodes based on requested mode

            if (visualMode) 
                // Specialized sort to produce a more visually-interesting graph
                g.Nodes.Sort(EntropyOrder);
            else if (cum) 
                g.Nodes.Sort(CumNameOrder);
            else 
                g.Nodes.Sort(FlatNameOrder);
                    }

        // SelectTopNodePtrs returns a set of the top maxNodes *Node in a graph.
        private static NodePtrSet SelectTopNodePtrs(this ref Graph g, long maxNodes, bool visualMode)
        {
            var set = make(NodePtrSet);
            foreach (var (_, node) in g.selectTopNodes(maxNodes, visualMode))
            {
                set[node] = true;
            }
            return set;
        }

        // SelectTopNodes returns a set of the top maxNodes nodes in a graph.
        private static NodeSet SelectTopNodes(this ref Graph g, long maxNodes, bool visualMode)
        {
            return makeNodeSet(g.selectTopNodes(maxNodes, visualMode), 0L);
        }

        // selectTopNodes returns a slice of the top maxNodes nodes in a graph.
        private static Nodes selectTopNodes(this ref Graph g, long maxNodes, bool visualMode)
        {
            if (maxNodes > 0L)
            {
                if (visualMode)
                {
                    long count = default; 
                    // If generating a visual graph, count tags as nodes. Update
                    // maxNodes to account for them.
                    foreach (var (i, n) in g.Nodes)
                    {
                        var tags = countTags(n);
                        if (tags > maxNodelets)
                        {
                            tags = maxNodelets;
                        }
                        count += tags + 1L;

                        if (count >= maxNodes)
                        {
                            maxNodes = i + 1L;
                            break;
                        }
                    }
                }
            }
            if (maxNodes > len(g.Nodes))
            {
                maxNodes = len(g.Nodes);
            }
            return g.Nodes[..maxNodes];
        }

        // countTags counts the tags with flat count. This underestimates the
        // number of tags being displayed, but in practice is close enough.
        private static long countTags(ref Node n)
        {
            long count = 0L;
            {
                var e__prev1 = e;

                foreach (var (_, __e) in n.LabelTags)
                {
                    e = __e;
                    if (e.Flat != 0L)
                    {
                        count++;
                    }
                }

                e = e__prev1;
            }

            foreach (var (_, t) in n.NumericTags)
            {
                {
                    var e__prev2 = e;

                    foreach (var (_, __e) in t)
                    {
                        e = __e;
                        if (e.Flat != 0L)
                        {
                            count++;
                        }
                    }

                    e = e__prev2;
                }

            }
            return count;
        }

        // RemoveRedundantEdges removes residual edges if the destination can
        // be reached through another path. This is done to simplify the graph
        // while preserving connectivity.
        private static void RemoveRedundantEdges(this ref Graph g)
        { 
            // Walk the nodes and outgoing edges in reverse order to prefer
            // removing edges with the lowest weight.
            for (var i = len(g.Nodes); i > 0L; i--)
            {
                var n = g.Nodes[i - 1L];
                var @in = n.In.Sort();
                for (var j = len(in); j > 0L; j--)
                {
                    var e = in[j - 1L];
                    if (!e.Residual)
                    { 
                        // Do not remove edges heavier than a non-residual edge, to
                        // avoid potential confusion.
                        break;
                    }
                    if (isRedundantEdge(e))
                    {
                        delete(e.Src.Out, e.Dest);
                        delete(e.Dest.In, e.Src);
                    }
                }

            }

        }

        // isRedundantEdge determines if there is a path that allows e.Src
        // to reach e.Dest after removing e.
        private static bool isRedundantEdge(ref Edge e)
        {
            var src = e.Src;
            var n = e.Dest;
            map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Node, bool>{n:true};
            Nodes queue = new Nodes(n);
            while (len(queue) > 0L)
            {
                n = queue[0L];
                queue = queue[1L..];
                foreach (var (_, ie) in n.In)
                {
                    if (e == ie || seen[ie.Src])
                    {
                        continue;
                    }
                    if (ie.Src == src)
                    {
                        return true;
                    }
                    seen[ie.Src] = true;
                    queue = append(queue, ie.Src);
                }
            }

            return false;
        }

        // nodeSorter is a mechanism used to allow a report to be sorted
        // in different ways.
        private partial struct nodeSorter
        {
            public Nodes rs;
            public Func<ref Node, ref Node, bool> less;
        }

        private static long Len(this nodeSorter s)
        {
            return len(s.rs);
        }
        private static void Swap(this nodeSorter s, long i, long j)
        {
            s.rs[i] = s.rs[j];
            s.rs[j] = s.rs[i];

        }
        private static bool Less(this nodeSorter s, long i, long j)
        {
            return s.less(s.rs[i], s.rs[j]);
        }

        // Sort reorders a slice of nodes based on the specified ordering
        // criteria. The result is sorted in decreasing order for (absolute)
        // numeric quantities, alphabetically for text, and increasing for
        // addresses.
        public static error Sort(this Nodes ns, NodeOrder o)
        {
            nodeSorter s = default;


            if (o == FlatNameOrder) 
                s = new nodeSorter(ns,func(l,r*Node)bool{ifiv,jv:=abs64(l.Flat),abs64(r.Flat);iv!=jv{returniv>jv}ifiv,jv:=l.Info.PrintableName(),r.Info.PrintableName();iv!=jv{returniv<jv}ifiv,jv:=abs64(l.Cum),abs64(r.Cum);iv!=jv{returniv>jv}returncompareNodes(l,r)},);
            else if (o == FlatCumNameOrder) 
                s = new nodeSorter(ns,func(l,r*Node)bool{ifiv,jv:=abs64(l.Flat),abs64(r.Flat);iv!=jv{returniv>jv}ifiv,jv:=abs64(l.Cum),abs64(r.Cum);iv!=jv{returniv>jv}ifiv,jv:=l.Info.PrintableName(),r.Info.PrintableName();iv!=jv{returniv<jv}returncompareNodes(l,r)},);
            else if (o == NameOrder) 
                s = new nodeSorter(ns,func(l,r*Node)bool{ifiv,jv:=l.Info.Name,r.Info.Name;iv!=jv{returniv<jv}returncompareNodes(l,r)},);
            else if (o == FileOrder) 
                s = new nodeSorter(ns,func(l,r*Node)bool{ifiv,jv:=l.Info.File,r.Info.File;iv!=jv{returniv<jv}ifiv,jv:=l.Info.StartLine,r.Info.StartLine;iv!=jv{returniv<jv}returncompareNodes(l,r)},);
            else if (o == AddressOrder) 
                s = new nodeSorter(ns,func(l,r*Node)bool{ifiv,jv:=l.Info.Address,r.Info.Address;iv!=jv{returniv<jv}returncompareNodes(l,r)},);
            else if (o == CumNameOrder || o == EntropyOrder) 
                // Hold scoring for score-based ordering
                map<ref Node, long> score = default;
                Func<ref Node, ref Node, bool> scoreOrder = (l, r) =>
                {
                    {
                        var iv__prev1 = iv;
                        var jv__prev1 = jv;

                        iv = abs64(score[l]);
                        jv = abs64(score[r]);

                        if (iv != jv)
                        {
                            return error.As(iv > jv);
                        }

                        iv = iv__prev1;
                        jv = jv__prev1;

                    }
                    {
                        var iv__prev1 = iv;
                        var jv__prev1 = jv;

                        iv = l.Info.PrintableName();
                        jv = r.Info.PrintableName();

                        if (iv != jv)
                        {
                            return error.As(iv < jv);
                        }

                        iv = iv__prev1;
                        jv = jv__prev1;

                    }
                    {
                        var iv__prev1 = iv;
                        var jv__prev1 = jv;

                        iv = abs64(l.Flat);
                        jv = abs64(r.Flat);

                        if (iv != jv)
                        {
                            return error.As(iv > jv);
                        }

                        iv = iv__prev1;
                        jv = jv__prev1;

                    }
                    return error.As(compareNodes(l, r));
                }
;


                if (o == CumNameOrder) 
                    score = make_map<ref Node, long>(len(ns));
                    {
                        var n__prev1 = n;

                        foreach (var (_, __n) in ns)
                        {
                            n = __n;
                            score[n] = n.Cum;
                        }

                        n = n__prev1;
                    }

                    s = new nodeSorter(ns,scoreOrder);
                else if (o == EntropyOrder) 
                    score = make_map<ref Node, long>(len(ns));
                    {
                        var n__prev1 = n;

                        foreach (var (_, __n) in ns)
                        {
                            n = __n;
                            score[n] = entropyScore(n);
                        }

                        n = n__prev1;
                    }

                    s = new nodeSorter(ns,scoreOrder);
                            else 
                return error.As(fmt.Errorf("report: unrecognized sort ordering: %d", o));
                        sort.Sort(s);
            return error.As(null);
        }

        // compareNodes compares two nodes to provide a deterministic ordering
        // between them. Two nodes cannot have the same Node.Info value.
        private static bool compareNodes(ref Node l, ref Node r)
        {
            return fmt.Sprint(l.Info) < fmt.Sprint(r.Info);
        }

        // entropyScore computes a score for a node representing how important
        // it is to include this node on a graph visualization. It is used to
        // sort the nodes and select which ones to display if we have more
        // nodes than desired in the graph. This number is computed by looking
        // at the flat and cum weights of the node and the incoming/outgoing
        // edges. The fundamental idea is to penalize nodes that have a simple
        // fallthrough from their incoming to the outgoing edge.
        private static long entropyScore(ref Node n)
        {
            var score = float64(0L);

            if (len(n.In) == 0L)
            {
                score++; // Favor entry nodes
            }
            else
            {
                score += edgeEntropyScore(n, n.In, 0L);
            }
            if (len(n.Out) == 0L)
            {
                score++; // Favor leaf nodes
            }
            else
            {
                score += edgeEntropyScore(n, n.Out, n.Flat);
            }
            return int64(score * float64(n.Cum)) + n.Flat;
        }

        // edgeEntropyScore computes the entropy value for a set of edges
        // coming in or out of a node. Entropy (as defined in information
        // theory) refers to the amount of information encoded by the set of
        // edges. A set of edges that have a more interesting distribution of
        // samples gets a higher score.
        private static double edgeEntropyScore(ref Node n, EdgeMap edges, long self)
        {
            var score = float64(0L);
            var total = self;
            {
                var e__prev1 = e;

                foreach (var (_, __e) in edges)
                {
                    e = __e;
                    if (e.Weight > 0L)
                    {
                        total += abs64(e.Weight);
                    }
                }

                e = e__prev1;
            }

            if (total != 0L)
            {
                {
                    var e__prev1 = e;

                    foreach (var (_, __e) in edges)
                    {
                        e = __e;
                        var frac = float64(abs64(e.Weight)) / float64(total);
                        score += -frac * math.Log2(frac);
                    }

                    e = e__prev1;
                }

                if (self > 0L)
                {
                    frac = float64(abs64(self)) / float64(total);
                    score += -frac * math.Log2(frac);
                }
            }
            return score;
        }

        // NodeOrder sets the ordering for a Sort operation
        public partial struct NodeOrder // : long
        {
        }

        // Sorting options for node sort.
        public static readonly NodeOrder FlatNameOrder = iota;
        public static readonly var FlatCumNameOrder = 0;
        public static readonly var CumNameOrder = 1;
        public static readonly var NameOrder = 2;
        public static readonly var FileOrder = 3;
        public static readonly var AddressOrder = 4;
        public static readonly var EntropyOrder = 5;

        // Sort returns a slice of the edges in the map, in a consistent
        // order. The sort order is first based on the edge weight
        // (higher-to-lower) and then by the node names to avoid flakiness.
        public static slice<ref Edge> Sort(this EdgeMap e)
        {
            var el = make(edgeList, 0L, len(e));
            foreach (var (_, w) in e)
            {
                el = append(el, w);
            }
            sort.Sort(el);
            return el;
        }

        // Sum returns the total weight for a set of nodes.
        public static long Sum(this EdgeMap e)
        {
            long ret = default;
            foreach (var (_, edge) in e)
            {
                ret += edge.Weight;
            }
            return ret;
        }

        private partial struct edgeList // : slice<ref Edge>
        {
        }

        private static long Len(this edgeList el)
        {
            return len(el);
        }

        private static bool Less(this edgeList el, long i, long j)
        {
            if (el[i].Weight != el[j].Weight)
            {
                return abs64(el[i].Weight) > abs64(el[j].Weight);
            }
            var from1 = el[i].Src.Info.PrintableName();
            var from2 = el[j].Src.Info.PrintableName();
            if (from1 != from2)
            {
                return from1 < from2;
            }
            var to1 = el[i].Dest.Info.PrintableName();
            var to2 = el[j].Dest.Info.PrintableName();

            return to1 < to2;
        }

        private static void Swap(this edgeList el, long i, long j)
        {
            el[i] = el[j];
            el[j] = el[i];
        }

        private static long abs64(long i)
        {
            if (i < 0L)
            {
                return -i;
            }
            return i;
        }
    }
}}}}}}}
