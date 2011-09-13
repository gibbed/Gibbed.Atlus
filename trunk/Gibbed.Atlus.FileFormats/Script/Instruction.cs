/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

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
