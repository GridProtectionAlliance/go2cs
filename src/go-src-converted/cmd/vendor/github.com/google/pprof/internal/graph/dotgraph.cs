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

// package graph -- go2cs converted at 2020 October 09 05:53:36 UTC
// import "cmd/vendor/github.com/google/pprof/internal/graph" ==> using graph = go.cmd.vendor.github.com.google.pprof.@internal.graph_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\graph\dotgraph.go
using fmt = go.fmt_package;
using io = go.io_package;
using math = go.math_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;

using measurement = go.github.com.google.pprof.@internal.measurement_package;
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
        // DotAttributes contains details about the graph itself, giving
        // insight into how its elements should be rendered.
        public partial struct DotAttributes
        {
            public map<ptr<Node>, ptr<DotNodeAttributes>> Nodes; // A map allowing each Node to have its own visualization option
        }

        // DotNodeAttributes contains Node specific visualization options.
        public partial struct DotNodeAttributes
        {
            public @string Shape; // The optional shape of the node when rendered visually
            public bool Bold; // If the node should be bold or not
            public long Peripheries; // An optional number of borders to place around a node
            public @string URL; // An optional url link to add to a node
            public Func<ptr<NodeInfo>, @string> Formatter; // An optional formatter for the node's label
        }

        // DotConfig contains attributes about how a graph should be
        // constructed and how it should look.
        public partial struct DotConfig
        {
            public @string Title; // The title of the DOT graph
            public @string LegendURL; // The URL to link to from the legend.
            public slice<@string> Labels; // The labels for the DOT's legend

            public Func<long, @string> FormatValue; // A formatting function for values
            public long Total; // The total weight of the graph, used to compute percentages
        }

        private static readonly long maxNodelets = (long)4L; // Number of nodelets for labels (both numeric and non)

        // ComposeDot creates and writes a in the DOT format to the writer, using
        // the configurations given.
 // Number of nodelets for labels (both numeric and non)

        // ComposeDot creates and writes a in the DOT format to the writer, using
        // the configurations given.
        public static void ComposeDot(io.Writer w, ptr<Graph> _addr_g, ptr<DotAttributes> _addr_a, ptr<DotConfig> _addr_c) => func((defer, _, __) =>
        {
            ref Graph g = ref _addr_g.val;
            ref DotAttributes a = ref _addr_a.val;
            ref DotConfig c = ref _addr_c.val;

            ptr<builder> builder = addr(new builder(w,a,c)); 

            // Begin constructing DOT by adding a title and legend.
            builder.start();
            defer(builder.finish());
            builder.addLegend();

            if (len(g.Nodes) == 0L)
            {
                return ;
            } 

            // Preprocess graph to get id map and find max flat.
            var nodeIDMap = make_map<ptr<Node>, long>();
            var hasNodelets = make_map<ptr<Node>, bool>();

            var maxFlat = float64(abs64(g.Nodes[0L].FlatValue()));
            {
                var n__prev1 = n;

                foreach (var (__i, __n) in g.Nodes)
                {
                    i = __i;
                    n = __n;
                    nodeIDMap[n] = i + 1L;
                    if (float64(abs64(n.FlatValue())) > maxFlat)
                    {
                        maxFlat = float64(abs64(n.FlatValue()));
                    }

                }

                n = n__prev1;
            }

            EdgeMap edges = new EdgeMap(); 

            // Add nodes and nodelets to DOT builder.
            {
                var n__prev1 = n;

                foreach (var (_, __n) in g.Nodes)
                {
                    n = __n;
                    builder.addNode(n, nodeIDMap[n], maxFlat);
                    hasNodelets[n] = builder.addNodelets(n, nodeIDMap[n]); 

                    // Collect all edges. Use a fake node to support multiple incoming edges.
                    {
                        var e__prev2 = e;

                        foreach (var (_, __e) in n.Out)
                        {
                            e = __e;
                            edges[addr(new Node())] = e;
                        }

                        e = e__prev2;
                    }
                } 

                // Add edges to DOT builder. Sort edges by frequency as a hint to the graph layout engine.

                n = n__prev1;
            }

            {
                var e__prev1 = e;

                foreach (var (_, __e) in edges.Sort())
                {
                    e = __e;
                    builder.addEdge(e, nodeIDMap[e.Src], nodeIDMap[e.Dest], hasNodelets[e.Src]);
                }

                e = e__prev1;
            }
        });

        // builder wraps an io.Writer and understands how to compose DOT formatted elements.
        private partial struct builder : io.Writer
        {
            public ref io.Writer Writer => ref Writer_val;
            public ptr<DotAttributes> attributes;
            public ptr<DotConfig> config;
        }

        // start generates a title and initial node in DOT format.
        private static void start(this ptr<builder> _addr_b)
        {
            ref builder b = ref _addr_b.val;

            @string graphname = "unnamed";
            if (b.config.Title != "")
            {
                graphname = b.config.Title;
            }

            fmt.Fprintln(b, "digraph \"" + graphname + "\" {");
            fmt.Fprintln(b, "node [style=filled fillcolor=\"#f8f8f8\"]");

        }

        // finish closes the opening curly bracket in the constructed DOT buffer.
        private static void finish(this ptr<builder> _addr_b)
        {
            ref builder b = ref _addr_b.val;

            fmt.Fprintln(b, "}");
        }

        // addLegend generates a legend in DOT format.
        private static void addLegend(this ptr<builder> _addr_b)
        {
            ref builder b = ref _addr_b.val;

            var labels = b.config.Labels;
            if (len(labels) == 0L)
            {
                return ;
            }

            var title = labels[0L];
            fmt.Fprintf(b, "subgraph cluster_L { \"%s\" [shape=box fontsize=16", title);
            fmt.Fprintf(b, " label=\"%s\\l\"", strings.Join(labels, "\\l"));
            if (b.config.LegendURL != "")
            {
                fmt.Fprintf(b, " URL=\"%s\" target=\"_blank\"", b.config.LegendURL);
            }

            if (b.config.Title != "")
            {
                fmt.Fprintf(b, " tooltip=\"%s\"", b.config.Title);
            }

            fmt.Fprintf(b, "] }\n");

        }

        // addNode generates a graph node in DOT format.
        private static void addNode(this ptr<builder> _addr_b, ptr<Node> _addr_node, long nodeID, double maxFlat)
        {
            ref builder b = ref _addr_b.val;
            ref Node node = ref _addr_node.val;

            var flat = node.FlatValue();
            var cum = node.CumValue();
            var attrs = b.attributes.Nodes[node]; 

            // Populate label for node.
            @string label = default;
            if (attrs != null && attrs.Formatter != null)
            {
                label = attrs.Formatter(_addr_node.Info);
            }
            else
            {
                label = multilinePrintableName(_addr_node.Info);
            }

            var flatValue = b.config.FormatValue(flat);
            if (flat != 0L)
            {
                label = label + fmt.Sprintf("%s (%s)", flatValue, strings.TrimSpace(measurement.Percentage(flat, b.config.Total)));
            }
            else
            {
                label = label + "0";
            }

            var cumValue = flatValue;
            if (cum != flat)
            {
                if (flat != 0L)
                {
                    label = label + "\\n";
                }
                else
                {
                    label = label + " ";
                }

                cumValue = b.config.FormatValue(cum);
                label = label + fmt.Sprintf("of %s (%s)", cumValue, strings.TrimSpace(measurement.Percentage(cum, b.config.Total)));

            } 

            // Scale font sizes from 8 to 24 based on percentage of flat frequency.
            // Use non linear growth to emphasize the size difference.
            long baseFontSize = 8L;
            float maxFontGrowth = 16.0F;
            var fontSize = baseFontSize;
            if (maxFlat != 0L && flat != 0L && float64(abs64(flat)) <= maxFlat)
            {
                fontSize += int(math.Ceil(maxFontGrowth * math.Sqrt(float64(abs64(flat)) / maxFlat)));
            } 

            // Determine node shape.
            @string shape = "box";
            if (attrs != null && attrs.Shape != "")
            {
                shape = attrs.Shape;
            } 

            // Create DOT attribute for node.
            var attr = fmt.Sprintf("label=\"%s\" id=\"node%d\" fontsize=%d shape=%s tooltip=\"%s (%s)\" color=\"%s\" fillcolo" +
    "r=\"%s\"", label, nodeID, fontSize, shape, node.Info.PrintableName(), cumValue, dotColor(float64(node.CumValue()) / float64(abs64(b.config.Total)), false), dotColor(float64(node.CumValue()) / float64(abs64(b.config.Total)), true)); 

            // Add on extra attributes if provided.
            if (attrs != null)
            { 
                // Make bold if specified.
                if (attrs.Bold)
                {
                    attr += " style=\"bold,filled\"";
                } 

                // Add peripheries if specified.
                if (attrs.Peripheries != 0L)
                {
                    attr += fmt.Sprintf(" peripheries=%d", attrs.Peripheries);
                } 

                // Add URL if specified. target="_blank" forces the link to open in a new tab.
                if (attrs.URL != "")
                {
                    attr += fmt.Sprintf(" URL=\"%s\" target=\"_blank\"", attrs.URL);
                }

            }

            fmt.Fprintf(b, "N%d [%s]\n", nodeID, attr);

        }

        // addNodelets generates the DOT boxes for the node tags if they exist.
        private static bool addNodelets(this ptr<builder> _addr_b, ptr<Node> _addr_node, long nodeID)
        {
            ref builder b = ref _addr_b.val;
            ref Node node = ref _addr_node.val;

            @string nodelets = default; 

            // Populate two Tag slices, one for LabelTags and one for NumericTags.
            slice<ptr<Tag>> ts = default;
            var lnts = make_map<@string, slice<ptr<Tag>>>();
            {
                var t__prev1 = t;

                foreach (var (_, __t) in node.LabelTags)
                {
                    t = __t;
                    ts = append(ts, t);
                }

                t = t__prev1;
            }

            foreach (var (l, tm) in node.NumericTags)
            {
                {
                    var t__prev2 = t;

                    foreach (var (_, __t) in tm)
                    {
                        t = __t;
                        lnts[l] = append(lnts[l], t);
                    }

                    t = t__prev2;
                }
            } 

            // For leaf nodes, print cumulative tags (includes weight from
            // children that have been deleted).
            // For internal nodes, print only flat tags.
            var flatTags = len(node.Out) > 0L; 

            // Select the top maxNodelets alphanumeric labels by weight.
            SortTags(ts, flatTags);
            if (len(ts) > maxNodelets)
            {
                ts = ts[..maxNodelets];
            }

            {
                var t__prev1 = t;

                foreach (var (__i, __t) in ts)
                {
                    i = __i;
                    t = __t;
                    var w = t.CumValue();
                    if (flatTags)
                    {
                        w = t.FlatValue();
                    }

                    if (w == 0L)
                    {
                        continue;
                    }

                    var weight = b.config.FormatValue(w);
                    nodelets += fmt.Sprintf("N%d_%d [label = \"%s\" id=\"N%d_%d\" fontsize=8 shape=box3d tooltip=\"%s\"]" + "\n", nodeID, i, t.Name, nodeID, i, weight);
                    nodelets += fmt.Sprintf("N%d -> N%d_%d [label=\" %s\" weight=100 tooltip=\"%s\" labeltooltip=\"%s\"]" + "\n", nodeID, nodeID, i, weight, weight, weight);
                    {
                        var nts__prev1 = nts;

                        var nts = lnts[t.Name];

                        if (nts != null)
                        {
                            nodelets += b.numericNodelets(nts, maxNodelets, flatTags, fmt.Sprintf("N%d_%d", nodeID, i));
                        }

                        nts = nts__prev1;

                    }

                }

                t = t__prev1;
            }

            {
                var nts__prev1 = nts;

                nts = lnts[""];

                if (nts != null)
                {
                    nodelets += b.numericNodelets(nts, maxNodelets, flatTags, fmt.Sprintf("N%d", nodeID));
                }

                nts = nts__prev1;

            }


            fmt.Fprint(b, nodelets);
            return nodelets != "";

        }

        private static @string numericNodelets(this ptr<builder> _addr_b, slice<ptr<Tag>> nts, long maxNumNodelets, bool flatTags, @string source)
        {
            ref builder b = ref _addr_b.val;

            @string nodelets = ""; 

            // Collapse numeric labels into maxNumNodelets buckets, of the form:
            // 1MB..2MB, 3MB..5MB, ...
            foreach (var (j, t) in b.collapsedTags(nts, maxNumNodelets, flatTags))
            {
                var w = t.CumValue();
                @string attr = " style=\"dotted\"";
                if (flatTags || t.FlatValue() == t.CumValue())
                {
                    w = t.FlatValue();
                    attr = "";

                }

                if (w != 0L)
                {
                    var weight = b.config.FormatValue(w);
                    nodelets += fmt.Sprintf("N%s_%d [label = \"%s\" id=\"N%s_%d\" fontsize=8 shape=box3d tooltip=\"%s\"]" + "\n", source, j, t.Name, source, j, weight);
                    nodelets += fmt.Sprintf("%s -> N%s_%d [label=\" %s\" weight=100 tooltip=\"%s\" labeltooltip=\"%s\"%s]" + "\n", source, source, j, weight, weight, weight, attr);
                }

            }
            return nodelets;

        }

        // addEdge generates a graph edge in DOT format.
        private static void addEdge(this ptr<builder> _addr_b, ptr<Edge> _addr_edge, long from, long to, bool hasNodelets)
        {
            ref builder b = ref _addr_b.val;
            ref Edge edge = ref _addr_edge.val;

            @string inline = default;
            if (edge.Inline)
            {
                inline = "\\n (inline)";
            }

            var w = b.config.FormatValue(edge.WeightValue());
            var attr = fmt.Sprintf("label=\" %s%s\"", w, inline);
            if (b.config.Total != 0L)
            { 
                // Note: edge.weight > b.config.Total is possible for profile diffs.
                {
                    long weight = 1L + int(min64(abs64(edge.WeightValue() * 100L / b.config.Total), 100L));

                    if (weight > 1L)
                    {
                        attr = fmt.Sprintf("%s weight=%d", attr, weight);
                    }

                }

                {
                    long width = 1L + int(min64(abs64(edge.WeightValue() * 5L / b.config.Total), 5L));

                    if (width > 1L)
                    {
                        attr = fmt.Sprintf("%s penwidth=%d", attr, width);
                    }

                }

                attr = fmt.Sprintf("%s color=\"%s\"", attr, dotColor(float64(edge.WeightValue()) / float64(abs64(b.config.Total)), false));

            }

            @string arrow = "->";
            if (edge.Residual)
            {
                arrow = "...";
            }

            var tooltip = fmt.Sprintf("\"%s %s %s (%s)\"", edge.Src.Info.PrintableName(), arrow, edge.Dest.Info.PrintableName(), w);
            attr = fmt.Sprintf("%s tooltip=%s labeltooltip=%s", attr, tooltip, tooltip);

            if (edge.Residual)
            {
                attr = attr + " style=\"dotted\"";
            }

            if (hasNodelets)
            { 
                // Separate children further if source has tags.
                attr = attr + " minlen=2";

            }

            fmt.Fprintf(b, "N%d -> N%d [%s]\n", from, to, attr);

        }

        // dotColor returns a color for the given score (between -1.0 and
        // 1.0), with -1.0 colored red, 0.0 colored grey, and 1.0 colored
        // green. If isBackground is true, then a light (low-saturation)
        // color is returned (suitable for use as a background color);
        // otherwise, a darker color is returned (suitable for use as a
        // foreground color).
        private static @string dotColor(double score, bool isBackground)
        { 
            // A float between 0.0 and 1.0, indicating the extent to which
            // colors should be shifted away from grey (to make positive and
            // negative values easier to distinguish, and to make more use of
            // the color range.)
            const float shift = (float)0.7F; 

            // Saturation and value (in hsv colorspace) for background colors.
 

            // Saturation and value (in hsv colorspace) for background colors.
            const float bgSaturation = (float)0.1F;

            const float bgValue = (float)0.93F; 

            // Saturation and value (in hsv colorspace) for foreground colors.
 

            // Saturation and value (in hsv colorspace) for foreground colors.
            const float fgSaturation = (float)1.0F;

            const float fgValue = (float)0.7F; 

            // Choose saturation and value based on isBackground.
 

            // Choose saturation and value based on isBackground.
            double saturation = default;
            double value = default;
            if (isBackground)
            {
                saturation = bgSaturation;
                value = bgValue;
            }
            else
            {
                saturation = fgSaturation;
                value = fgValue;
            } 

            // Limit the score values to the range [-1.0, 1.0].
            score = math.Max(-1.0F, math.Min(1.0F, score)); 

            // Reduce saturation near score=0 (so it is colored grey, rather than yellow).
            if (math.Abs(score) < 0.2F)
            {
                saturation *= math.Abs(score) / 0.2F;
            } 

            // Apply 'shift' to move scores away from 0.0 (grey).
            if (score > 0.0F)
            {
                score = math.Pow(score, (1.0F - shift));
            }

            if (score < 0.0F)
            {
                score = -math.Pow(-score, (1.0F - shift));
            }

            double r = default;            double g = default;            double b = default; // red, green, blue
 // red, green, blue
            if (score < 0.0F)
            {
                g = value;
                r = value * (1L + saturation * score);
            }
            else
            {
                r = value;
                g = value * (1L - saturation * score);
            }

            b = value * (1L - saturation);
            return fmt.Sprintf("#%02x%02x%02x", uint8(r * 255.0F), uint8(g * 255.0F), uint8(b * 255.0F));

        }

        private static @string multilinePrintableName(ptr<NodeInfo> _addr_info)
        {
            ref NodeInfo info = ref _addr_info.val;

            NodeInfo infoCopy = info;
            infoCopy.Name = ShortenFunctionName(infoCopy.Name);
            infoCopy.Name = strings.Replace(infoCopy.Name, "::", "\\n", -1L);
            infoCopy.Name = strings.Replace(infoCopy.Name, ".", "\\n", -1L);
            if (infoCopy.File != "")
            {
                infoCopy.File = filepath.Base(infoCopy.File);
            }

            return strings.Join(infoCopy.NameComponents(), "\\n") + "\\n";

        }

        // collapsedTags trims and sorts a slice of tags.
        private static slice<ptr<Tag>> collapsedTags(this ptr<builder> _addr_b, slice<ptr<Tag>> ts, long count, bool flatTags)
        {
            ref builder b = ref _addr_b.val;

            ts = SortTags(ts, flatTags);
            if (len(ts) <= count)
            {
                return ts;
            }

            var tagGroups = make_slice<slice<ptr<Tag>>>(count);
            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in (ts)[..count])
                {
                    i = __i;
                    t = __t;
                    tagGroups[i] = new slice<ptr<Tag>>(new ptr<Tag>[] { t });
                }

                i = i__prev1;
                t = t__prev1;
            }

            {
                var t__prev1 = t;

                foreach (var (_, __t) in (ts)[count..])
                {
                    t = __t;
                    long g = 0L;
                    var d = tagDistance(_addr_t, _addr_tagGroups[0L][0L]);
                    {
                        var i__prev2 = i;

                        for (long i = 1L; i < count; i++)
                        {
                            {
                                var nd = tagDistance(_addr_t, _addr_tagGroups[i][0L]);

                                if (nd < d)
                                {
                                    g = i;
                                    d = nd;

                                }

                            }

                        }


                        i = i__prev2;
                    }
                    tagGroups[g] = append(tagGroups[g], t);

                }

                t = t__prev1;
            }

            slice<ptr<Tag>> nts = default;
            {
                long g__prev1 = g;

                foreach (var (_, __g) in tagGroups)
                {
                    g = __g;
                    var (l, w, c) = b.tagGroupLabel(g);
                    nts = append(nts, addr(new Tag(Name:l,Flat:w,Cum:c,)));
                }

                g = g__prev1;
            }

            return SortTags(nts, flatTags);

        }

        private static double tagDistance(ptr<Tag> _addr_t, ptr<Tag> _addr_u)
        {
            ref Tag t = ref _addr_t.val;
            ref Tag u = ref _addr_u.val;

            var (v, _) = measurement.Scale(u.Value, u.Unit, t.Unit);
            if (v < float64(t.Value))
            {
                return float64(t.Value) - v;
            }

            return v - float64(t.Value);

        }

        private static (@string, long, long) tagGroupLabel(this ptr<builder> _addr_b, slice<ptr<Tag>> g)
        {
            @string label = default;
            long flat = default;
            long cum = default;
            ref builder b = ref _addr_b.val;

            if (len(g) == 1L)
            {
                var t = g[0L];
                return (measurement.Label(t.Value, t.Unit), t.FlatValue(), t.CumValue());
            }

            var min = g[0L];
            var max = g[0L];
            var df = min.FlatDiv;
            var f = min.Flat;
            var dc = min.CumDiv;
            var c = min.Cum;
            {
                var t__prev1 = t;

                foreach (var (_, __t) in g[1L..])
                {
                    t = __t;
                    {
                        var v__prev1 = v;

                        var (v, _) = measurement.Scale(t.Value, t.Unit, min.Unit);

                        if (int64(v) < min.Value)
                        {
                            min = t;
                        }

                        v = v__prev1;

                    }

                    {
                        var v__prev1 = v;

                        (v, _) = measurement.Scale(t.Value, t.Unit, max.Unit);

                        if (int64(v) > max.Value)
                        {
                            max = t;
                        }

                        v = v__prev1;

                    }

                    f += t.Flat;
                    df += t.FlatDiv;
                    c += t.Cum;
                    dc += t.CumDiv;

                }

                t = t__prev1;
            }

            if (df != 0L)
            {
                f = f / df;
            }

            if (dc != 0L)
            {
                c = c / dc;
            } 

            // Tags are not scaled with the selected output unit because tags are often
            // much smaller than other values which appear, so the range of tag sizes
            // sometimes would appear to be "0..0" when scaled to the selected output unit.
            return (measurement.Label(min.Value, min.Unit) + ".." + measurement.Label(max.Value, max.Unit), f, c);

        }

        private static long min64(long a, long b)
        {
            if (a < b)
            {
                return a;
            }

            return b;

        }
    }
}}}}}}}
