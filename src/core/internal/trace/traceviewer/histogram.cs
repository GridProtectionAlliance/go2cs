// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using fmt = fmt_package;
using template = html.template_package;
using math = math_package;
using strings = strings_package;
using time = time_package;
using html;

partial class traceviewer_package {

// TimeHistogram is an high-dynamic-range histogram for durations.
[GoType] partial struct TimeHistogram {
    public nint Count;
    public slice<nint> Buckets;
    public nint MinBucket;
    public nint MaxBucket;
}

// Five buckets for every power of 10.
internal static float64 logDiv = math.Log(math.Pow(10, 1.0F / 5));

// Add adds a single sample to the histogram.
[GoRecv] public static void Add(this ref TimeHistogram h, time.Duration d) {
    nint bucket = default!;
    if (d > 0) {
        bucket = ((nint)(math.Log(((float64)d)) / logDiv));
    }
    if (len(h.Buckets) <= bucket) {
        h.Buckets = append(h.Buckets, new slice<nint>(bucket - len(h.Buckets) + 1).ꓸꓸꓸ);
        h.Buckets = h.Buckets[..(int)(cap(h.Buckets))];
    }
    h.Buckets[bucket]++;
    if (bucket < h.MinBucket || h.MaxBucket == 0) {
        h.MinBucket = bucket;
    }
    if (bucket > h.MaxBucket) {
        h.MaxBucket = bucket;
    }
    h.Count++;
}

// BucketMin returns the minimum duration value for a provided bucket.
[GoRecv] public static time.Duration BucketMin(this ref TimeHistogram h, nint bucket) {
    return ((time.Duration)math.Exp(((float64)bucket) * logDiv));
}

// ToHTML renders the histogram as HTML.
[GoRecv] public static template.HTML ToHTML(this ref TimeHistogram h, Func<time.Duration, time.Duration, @string> urlmaker) {
    if (h == nil || h.Count == 0) {
        return ((template.HTML)""u8);
    }
    static readonly UntypedInt barWidth = 400;
    nint maxCount = 0;
    foreach (var (_, count) in h.Buckets) {
        if (count > maxCount) {
            maxCount = count;
        }
    }
    var w = @new<strings.Builder>();
    fmt.Fprintf(~w, @"<table>"u8);
    for (nint i = h.MinBucket; i <= h.MaxBucket; i++) {
        // Tick label.
        if (h.Buckets[i] > 0){
            fmt.Fprintf(~w, @"<tr><td class=""histoTime"" align=""right""><a href=%s>%s</a></td>"u8, urlmaker(h.BucketMin(i), h.BucketMin(i + 1)), h.BucketMin(i));
        } else {
            fmt.Fprintf(~w, @"<tr><td class=""histoTime"" align=""right"">%s</td>"u8, h.BucketMin(i));
        }
        // Bucket bar.
        nint width = h.Buckets[i] * barWidth / maxCount;
        fmt.Fprintf(~w, @"<td><div style=""width:%dpx;background:blue;position:relative"">&nbsp;</div></td>"u8, width);
        // Bucket count.
        fmt.Fprintf(~w, @"<td align=""right""><div style=""position:relative"">%d</div></td>"u8, h.Buckets[i]);
        fmt.Fprintf(~w, "</tr>\n"u8);
    }
    // Final tick label.
    fmt.Fprintf(~w, @"<tr><td align=""right"">%s</td></tr>"u8, h.BucketMin(h.MaxBucket + 1));
    fmt.Fprintf(~w, @"</table>"u8);
    return ((template.HTML)w.String());
}

} // end traceviewer_package
