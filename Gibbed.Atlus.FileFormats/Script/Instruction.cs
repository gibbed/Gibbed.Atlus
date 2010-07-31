namespace Gibbed.Atlus.FileFormats.Script
{
    public enum Instruction : ushort
    {
        PushInt = 0,
        PushFloat = 1,
        PushVariable = 2,
        PushResult = 4,
        PopVariable = 5,
        BeginProcedure = 7,
        CallNative = 8,
        Return = 9,
        CallProcedure = 11,
        Jump = 13,
        Add = 14,
        Subtract = 15,
        Not = 22,
        JumpFalse = 28,
        PushShort = 29,
        SetVariable = 32, // wrong (somehow...)
    }
}
