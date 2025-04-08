namespace go;

partial class main_package {

[GoType("num:nint")] partial struct ArchFamilyType;

public static readonly ArchFamilyType AMD64 = /* iota */ 0;
public static readonly ArchFamilyType ARM = 1;
public static readonly ArchFamilyType ARM64 = 2;
public static readonly ArchFamilyType I386 = 3;
public static readonly ArchFamilyType LOONG64 = 4;
public static readonly ArchFamilyType MIPS = 5;
public static readonly ArchFamilyType MIPS64 = 6;
public static readonly ArchFamilyType PPC64 = 7;
public static readonly ArchFamilyType RISCV64 = 8;
public static readonly ArchFamilyType S390X = 9;
public static readonly ArchFamilyType WASM = 10;

public static readonly UntypedInt PtrSize = /* 4 << (^uintptr(0) >> 63) */ 8;

public static readonly ArchFamilyType ArchFamily = /* _ArchFamily */ 0;

public const bool BigEndian = /* IsArmbe|IsArm64be|IsMips|IsMips64|IsPpc|IsPpc64|IsS390|IsS390x|IsSparc|IsSparc64 == 1 */ false;

public static readonly UntypedInt DefaultPhysPageSize = /* _DefaultPhysPageSize */ 4096;

public static readonly UntypedInt PCQuantum = /* _PCQuantum */ 1;

public static readonly UntypedInt Int64Align = /* PtrSize */ 8;

public static readonly UntypedInt MinFrameSize = /* _MinFrameSize */ 0;

public static readonly UntypedInt StackAlign = /* _StackAlign */ 8;

} // end main_package
