// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class math_package {

// Export internal functions for testing.
public static Func<float64, float64> ExpGo = exp;

public static Func<float64, float64> Exp2Go = exp2;

public static Func<float64, float64, float64> HypotGo = hypot;

public static Func<float64, float64> SqrtGo = sqrt;

public static Func<float64, (uint64, float64)> TrigReduce = trigReduce;

public static readonly UntypedInt ReduceThreshold = /* reduceThreshold */ 536870912;

} // end math_package
