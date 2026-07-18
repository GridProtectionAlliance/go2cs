// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class strings_package {

public static any ΔReplacer(this ж<Replacer> Ꮡr) {
    ref var r = ref Ꮡr.Value;

    Ꮡr.of(Replacer.Ꮡonce).Do(Ꮡr.buildOnce);
    return r.r;
}

public static @string PrintTrie(this ж<Replacer> Ꮡr) {
    ref var r = ref Ꮡr.Value;

    Ꮡr.of(Replacer.Ꮡonce).Do(Ꮡr.buildOnce);
    var gen = r.r._<ж<genericReplacer>>();
    return gen.printNode(gen.of(genericReplacer.Ꮡroot), 0);
}

[GoRecv] internal static @string /*s*/ printNode(this ref genericReplacer r, ж<trieNode> Ꮡt, nint depth) {
    @string s = default!;

    ref var t = ref Ꮡt.Value;
    if (t.priority > 0){
        s += "+"u8;
    } else {
        s += "-"u8;
    }
    s += "\n"u8;
    if (t.prefix != ""u8){
        s += Repeat("."u8, depth) + t.prefix;
        s += r.printNode(t.next, depth + len(t.prefix));
    } else 
    if (t.table != default!) {
        foreach (var (b, m) in r.mapping) {
            if ((nint)m != r.tableSize && t.table[m] != nil) {
                s += Repeat("."u8, depth) + ((@string)new byte[]{(byte)b}.slice());
                s += r.printNode(t.table[m], depth + 1);
            }
        }
    }
    return s;
}

public static nint StringFind(@string pattern, @string text) {
    return makeStringFinder(pattern).next(text);
}

public static (slice<nint>, slice<nint>) DumpTables(@string pattern) {
    var finder = makeStringFinder(pattern);
    return ((~finder).badCharSkip[..], (~finder).goodSuffixSkip);
}

} // end strings_package
