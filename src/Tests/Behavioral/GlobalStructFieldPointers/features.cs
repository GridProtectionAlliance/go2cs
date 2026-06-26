namespace go;

partial class main_package {


[GoType("dyn")] partial struct Featuresᴛ1 {
    public bool HasFast;
    public bool HasWide;
    public nint Level;
}
public static ж<Featuresᴛ1> ᏑFeatures = new(default(Featuresᴛ1));
public static ref Featuresᴛ1 Features => ref ᏑFeatures.val;

[GoType] partial struct option {
    internal @string name;
    internal ж<bool> flag;
}

} // end main_package
