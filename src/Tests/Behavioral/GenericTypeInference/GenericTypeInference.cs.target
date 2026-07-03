namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Signed<ΔT> {
    //  Type constraints: ~int | ~int8 | ~int16 | ~int32 | ~int64
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Unsigned<ΔT> {
    //  Type constraints: ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType("operators = Sum, Arithmetic, Integer, Comparable, Ordered")]
partial interface Integer<ΔT> {
    //  Type constraints: Signed | Unsigned
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

[GoType("[]int32")] partial struct Point;

public static @string String(this Point p) {
    return fmt.Sprintf("%d"u8, p);
}

public static S Scale<S, E>(S s, E c)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    var r = make<S>(len(s));
    foreach (var (i, v) in s) {
        r[i] = v * c;
    }
    return r;
}

public static void ScaleAndPrint(Point p) {
    var r = Scale(p, 2);
    fmt.Println(r.String());
}

public static S Twice<S, E>(S s, E c)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    return Scale(s, c);
}

public static (E, E) CopyClearMinMax<S, E>(S dst, S src)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    copy(dst, src);
    var lo = min(dst[0], src[1]);
    var hi = max(dst[0], src[1]);
    clear(src);
    return (lo, hi);
}

public static E SumHalves<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    if (len(s) == 1) {
        s[0] += s[0];
        return s[0];
    }
    nint mid = len(s) / 2;
    return SumHalves<S, E>(subslice<S, E>(s, -1, mid)) + SumHalves<S, E>(subslice<S, E>(s, mid, -1));
}

public static S AppendKeep<S, E>(S s, E v)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    var @out = append(s, v);
    return @out;
}

internal static void setFirst<E>(slice<E> s, E v)
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    if (len(s) > 0) {
        s[0] = v;
    }
}

public static void PassSlice<S, E>(S s, E v)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* Integer */ IAdditionOperators<E, E, E>, ISubtractionOperators<E, E, E>, IMultiplyOperators<E, E, E>, IDivisionOperators<E, E, E>, IModulusOperators<E, E, E>, IBitwiseOperators<E, E, E>, IShiftOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    setFirst(new slice<E>(s), v);
}

[GoType("map[@string, nint]")] partial struct Grades;

public static bool EqualMaps<M, K, V>(M m1, M m2)
    where M : /* ~map[K]V */ IMap<K, V>, ISupportMake<M>, new()
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    if (len(m1) != len(m2)) {
        return false;
    }
    foreach (var (k, v1) in m1) {
        {
            var (v2, ok) = m2[k, ꟷ]; if (!ok || !AreEqual(v1, v2)) {
                return false;
            }
        }
    }
    return true;
}

public delegate void Seq<V>(Func<V, bool> yield);

public delegate void KVSeq<K, V>(Func<K, V, bool> yield);

internal static Seq<nint> countdown(nint n) {
    return (Func<nint, bool> yield) => {
        for (nint i = n; i > 0; i--) {
            if (!yield(i)) {
                return;
            }
        }
    };
}

internal static KVSeq<@string, nint> letters() {
    return (Func<@string, nint, bool> yield) => {
        _ = yield("a"u8, 1) && yield("b"u8, 2);
    };
}

internal static void Main() {
    Point p = default!;
    p = new int32[]{1, 2, 3}.slice();
    ScaleAndPrint(p);
    fmt.Println(Twice(new Point(new int32[]{3, 4}.slice()), 2).String());
    var (dst2, src2) = (new Point(new int32[]{0, 0, 0}.slice()), new Point(new int32[]{7, 8, 9}.slice()));
    var (lo, hi) = CopyClearMinMax<Point, int32>(dst2, src2);
    fmt.Println(dst2.String(), src2.String(), lo, hi);
    var q = new Point(new int32[]{1, 2, 3, 4}.slice());
    var total = SumHalves<Point, int32>(q);
    fmt.Println(q.String(), total);
    var grown = AppendKeep(new Point(new int32[]{5, 6}.slice()), 7);
    fmt.Println(grown.String());
    var pt = new Point(new int32[]{1, 2}.slice());
    PassSlice(pt, 42);
    fmt.Println(pt.String());
    var g1 = new Grades(new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2});
    var g2 = new Grades(new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2});
    var g3 = new Grades(new map<@string, nint>{["a"u8] = 9});
    fmt.Println(EqualMaps<Grades, @string, nint>(g1, g2), EqualMaps<Grades, @string, nint>(g1, g3));
    nint sum = 0;
    foreach (var v in range<nint>(countdown(5).Invoke)) {
        if (v == 2) {
            break;
        }
        sum += v;
    }
    fmt.Println(sum);
    foreach (var (k, v) in range<@string, nint>(letters().Invoke)) {
        fmt.Println(k, v);
    }
}

} // end main_package
