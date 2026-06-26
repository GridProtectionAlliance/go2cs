// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// TODO: This file contains the special casing rules for Turkish and Azeri only.
// It should encompass all the languages with special casing rules
// and be generated automatically, but that requires some API
// development first.
namespace go;

partial class unicode_package {

public static SpecialCase TurkishCase = _TurkishCase;

internal static SpecialCase _TurkishCase = new SpecialCase{
    new CaseRange(73, 73, new d{0, 305 - 73, 0}),
    new CaseRange(105, 105, new d{304 - 105, 0, 304 - 105}),
    new CaseRange(304, 304, new d{0, 105 - 304, 0}),
    new CaseRange(305, 305, new d{73 - 305, 0, 73 - 305})
};

public static SpecialCase AzeriCase = _TurkishCase;

} // end unicode_package
