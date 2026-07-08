namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

internal static (slice<nint> evens, slice<nint> odds, error err) parse(slice<nint> items) {
    slice<nint> evens = default!;
    slice<nint> odds = default!;
    error err = default!;

    var classify = (slice<nint> e, slice<nint> o, error err) (slice<nint> vals) => {
        slice<nint> e = default!;
        slice<nint> o = default!;
        error errΔ1 = default!;
        foreach (var (_, v) in vals) {
            if (v < 0) {
                return (default!, default!, errors.New("negative value"u8));
            }
            if (v % 2 == 0){
                e = append(e, v);
            } else {
                o = append(o, v);
            }
        }
        return (e, o, default!);
    };
    return classify(items);
}

internal static void Main() {
    var (e, o, err) = parse(new nint[]{1, 2, 3, 4, 5, 6}.slice());
    fmt.Println(e, o, err);
}

} // end main_package
