namespace go.NamedSliceChildPkg;

using IoLike = IoLike_package;
using FsLike = go.IoLike.FsLike_package;
using go.IoLike;

partial class netlike_package {

[GoType("[]IoLike.FsLike_package.Info")] partial struct InfoList;

public static @string Describe() {
    return IoLike.Version();
}

public static InfoList Build() {
    return new InfoList(new FsLike.Info[]{
        FsLike.NewInfo("alpha"u8, 3),
        FsLike.NewInfo("beta"u8, 5)
    }.slice());
}

public static @string ElementName(InfoList infos, nint i) {
    return infos[i].Name;
}

public static nint TotalSize(InfoList infos) {
    nint sum = 0;
    foreach (var (_, info) in infos) {
        sum += info.Size;
    }
    return sum;
}

} // end netlike_package
