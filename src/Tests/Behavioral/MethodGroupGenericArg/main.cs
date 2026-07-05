namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint addInt(nint a, nint b) {
    return a + b;
}

internal static E foldSlice<S, E>(S s, Func<E, E, E> combine)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    E acc = default!;
    foreach (var (_, v) in s) {
        acc = combine(acc, v);
    }
    return acc;
}

internal static void Main() {
    var nums = new nint[]{1, 2, 3, 4}.slice();
    nint sum = foldSlice<slice<nint>, nint>(nums, addInt);
    fmt.Println(sum);
}

} // end main_package
