namespace go;

using @unsafe = unsafe_package;

partial class main_package {

[GoType] partial struct ITab {
    public ж<ΔInterfaceType> Inter;
    public ж<Type> Type;
    public uint32 Hash;
    public array<uintptr> Fun = new(1);
}

[GoType] partial struct EmptyInterface {
    public ж<Type> Type;
    public @unsafe.Pointer Data;
}

} // end main_package
