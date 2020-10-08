// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package trace -- go2cs converted at 2020 October 08 04:42:26 UTC
// import "internal/trace" ==> using trace = go.@internal.trace_package
// Original source: C:\Go\src\internal\trace\mud.go
using math = go.math_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace @internal
{
    public static partial class trace_package
    {
        // mud is an updatable mutator utilization distribution.
        //
        // This is a continuous distribution of duration over mutator
        // utilization. For example, the integral from mutator utilization a
        // to b is the total duration during which the mutator utilization was
        // in the range [a, b].
        //
        // This distribution is *not* normalized (it is not a probability
        // distribution). This makes it easier to work with as it's being
        // updated.
        //
        // It is represented as the sum of scaled uniform distribution
        // functions and Dirac delta functions (which are treated as
        // degenerate uniform distributions).
        private partial struct mud
        {
            public slice<edge> sorted; // trackMass is the inverse cumulative sum to track as the
// distribution is updated.
            public slice<edge> unsorted; // trackMass is the inverse cumulative sum to track as the
// distribution is updated.
            public double trackMass; // trackBucket is the bucket in which trackMass falls. If the
// total mass of the distribution is < trackMass, this is
// len(hist).
            public long trackBucket; // trackSum is the cumulative sum of hist[:trackBucket]. Once
// trackSum >= trackMass, trackBucket must be recomputed.
            public double trackSum; // hist is a hierarchical histogram of distribution mass.
            public array<double> hist;
        }

 
        // mudDegree is the number of buckets in the MUD summary
        // histogram.
        private static readonly long mudDegree = (long)1024L;


        private partial struct edge
        {
            public double x; // Additionally at x is a Dirac delta function with area dirac.
            public double delta; // Additionally at x is a Dirac delta function with area dirac.
            public double dirac;
        }

        // add adds a uniform function over [l, r] scaled so the total weight
        // of the uniform is area. If l==r, this adds a Dirac delta function.
        private static void add(this ptr<mud> _addr_d, double l, double r, double area)
        {
            ref mud d = ref _addr_d.val;

            if (area == 0L)
            {
                return ;
            }

            if (r < l)
            {
                l = r;
                r = l;

            } 

            // Add the edges.
            if (l == r)
            {
                d.unsorted = append(d.unsorted, new edge(l,0,area));
            }
            else
            {
                var delta = area / (r - l);
                d.unsorted = append(d.unsorted, new edge(l,delta,0), new edge(r,-delta,0));
            } 

            // Update the histogram.
            var h = _addr_d.hist;
            var (lbFloat, lf) = math.Modf(l * mudDegree);
            var lb = int(lbFloat);
            if (lb >= mudDegree)
            {
                lb = mudDegree - 1L;
                lf = 1L;

            }

            if (l == r)
            {
                h[lb] += area;
            }
            else
            {
                var (rbFloat, rf) = math.Modf(r * mudDegree);
                var rb = int(rbFloat);
                if (rb >= mudDegree)
                {
                    rb = mudDegree - 1L;
                    rf = 1L;

                }

                if (lb == rb)
                {
                    h[lb] += area;
                }
                else
                {
                    var perBucket = area / (r - l) / mudDegree;
                    h[lb] += perBucket * (1L - lf);
                    h[rb] += perBucket * rf;
                    for (var i = lb + 1L; i < rb; i++)
                    {
                        h[i] += perBucket;
                    }


                }

            } 

            // Update mass tracking.
            {
                var thresh = float64(d.trackBucket) / mudDegree;

                if (l < thresh)
                {
                    if (r < thresh)
                    {
                        d.trackSum += area;
                    }
                    else
                    {
                        d.trackSum += area * (thresh - l) / (r - l);
                    }

                    if (d.trackSum >= d.trackMass)
                    { 
                        // The tracked mass now falls in a different
                        // bucket. Recompute the inverse cumulative sum.
                        d.setTrackMass(d.trackMass);

                    }

                }

            }

        }

        // setTrackMass sets the mass to track the inverse cumulative sum for.
        //
        // Specifically, mass is a cumulative duration, and the mutator
        // utilization bounds for this duration can be queried using
        // approxInvCumulativeSum.
        private static void setTrackMass(this ptr<mud> _addr_d, double mass)
        {
            ref mud d = ref _addr_d.val;

            d.trackMass = mass; 

            // Find the bucket currently containing trackMass by computing
            // the cumulative sum.
            float sum = 0.0F;
            foreach (var (i, val) in d.hist[..])
            {
                var newSum = sum + val;
                if (newSum > mass)
                { 
                    // mass falls in bucket i.
                    d.trackBucket = i;
                    d.trackSum = sum;
                    return ;

                }

                sum = newSum;

            }
            d.trackBucket = len(d.hist);
            d.trackSum = sum;

        }

        // approxInvCumulativeSum is like invCumulativeSum, but specifically
        // operates on the tracked mass and returns an upper and lower bound
        // approximation of the inverse cumulative sum.
        //
        // The true inverse cumulative sum will be in the range [lower, upper).
        private static (double, double, bool) approxInvCumulativeSum(this ptr<mud> _addr_d)
        {
            double _p0 = default;
            double _p0 = default;
            bool _p0 = default;
            ref mud d = ref _addr_d.val;

            if (d.trackBucket == len(d.hist))
            {
                return (math.NaN(), math.NaN(), false);
            }

            return (float64(d.trackBucket) / mudDegree, float64(d.trackBucket + 1L) / mudDegree, true);

        }

        // invCumulativeSum returns x such that the integral of d from -∞ to x
        // is y. If the total weight of d is less than y, it returns the
        // maximum of the distribution and false.
        //
        // Specifically, y is a cumulative duration, and invCumulativeSum
        // returns the mutator utilization x such that at least y time has
        // been spent with mutator utilization <= x.
        private static (double, bool) invCumulativeSum(this ptr<mud> _addr_d, double y)
        {
            double _p0 = default;
            bool _p0 = default;
            ref mud d = ref _addr_d.val;

            if (len(d.sorted) == 0L && len(d.unsorted) == 0L)
            {
                return (math.NaN(), false);
            } 

            // Sort edges.
            var edges = d.unsorted;
            sort.Slice(edges, (i, j) =>
            {
                return edges[i].x < edges[j].x;
            }); 
            // Merge with sorted edges.
            d.unsorted = null;
            if (d.sorted == null)
            {
                d.sorted = edges;
            }
            else
            {
                var oldSorted = d.sorted;
                var newSorted = make_slice<edge>(len(oldSorted) + len(edges));
                long i = 0L;
                long j = 0L;
                foreach (var (o) in newSorted)
                {
                    if (i >= len(oldSorted))
                    {
                        copy(newSorted[o..], edges[j..]);
                        break;
                    }
                    else if (j >= len(edges))
                    {
                        copy(newSorted[o..], oldSorted[i..]);
                        break;
                    }
                    else if (oldSorted[i].x < edges[j].x)
                    {
                        newSorted[o] = oldSorted[i];
                        i++;
                    }
                    else
                    {
                        newSorted[o] = edges[j];
                        j++;
                    }

                }
                d.sorted = newSorted;

            } 

            // Traverse edges in order computing a cumulative sum.
            float csum = 0.0F;
            float rate = 0.0F;
            float prevX = 0.0F;
            foreach (var (_, e) in d.sorted)
            {
                var newCsum = csum + (e.x - prevX) * rate;
                if (newCsum >= y)
                { 
                    // y was exceeded between the previous edge
                    // and this one.
                    if (rate == 0L)
                    { 
                        // Anywhere between prevX and
                        // e.x will do. We return e.x
                        // because that takes care of
                        // the y==0 case naturally.
                        return (e.x, true);

                    }

                    return ((y - csum) / rate + prevX, true);

                }

                newCsum += e.dirac;
                if (newCsum >= y)
                { 
                    // y was exceeded by the Dirac delta at e.x.
                    return (e.x, true);

                }

                csum = newCsum;
                prevX = e.x;
                rate += e.delta;

            }
            return (prevX, false);

        }
    }
}}
