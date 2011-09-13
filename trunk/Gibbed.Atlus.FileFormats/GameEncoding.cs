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

using System;
using System.Text;

namespace Gibbed.Atlus.FileFormats
{
    public class GameEncoding : Encoding
    {
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return 1;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            throw new NotImplementedException();
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            if (index < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException(index < 0 ? "index" : "count");
            }
            if (bytes.Length - index < count)
            {
                throw new ArgumentOutOfRangeException("bytes");
            }

            if (bytes.Length == 0)
            {
                return 0;
            }

            int length = 0;
            for (int i = index; i < index + count; )
            {
                ushort c = bytes[i];
                i++;

                if ((c & 0x80) == 0x80)
                {
                    c <<= 8;
                    c |= bytes[i];
                    i++;
                }

                if ((c & 0x80) == 0x80)
                {
                    ushort s1 = 0;
                    s1 |= (ushort)((c & 0x7F00) >> 1);
                    s1 |= (ushort)((c & 0x007F));

                    length++;
                }
                else
                {
                    length++;
                }
            }

            return length;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (bytes == null || chars == null)
            {
                throw new ArgumentNullException(bytes == null ? "bytes" : "chars");
            }

            if (byteIndex < 0 || byteCount < 0)
            {
                throw new ArgumentOutOfRangeException(byteIndex < 0 ? "byteIndex" : "byteCount");
            }

            if (bytes.Length - byteIndex < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes");
            }

            if (charIndex < 0 || charIndex > chars.Length)
            {
                throw new ArgumentOutOfRangeException("charIndex");
            }

            if (bytes.Length == 0)
            {
                return 0;
            }

            int j = charIndex;
            for (
                int i = byteIndex; i < byteIndex + byteCount; )
            {
                ushort b = bytes[i];
                i++;
                
                if ((b & 0x80) == 0x80)
                {
                    b <<= 8;
                    b |= bytes[i];
                    i++;
                }

                if (b >= 0x80)
                {
                    ushort s1 = 0;
                    s1 |= (ushort)((b & 0x7F00) >> 1);
                    s1 |= (ushort)((b & 0x007F));

                    if (s1 >= Lookup.Length)
                    {
                        chars[j] = '?';
                    }
                    else
                    {
                        chars[j] = Lookup[(int)s1];
                    }
                    j++;
                }
                else
                {
                    chars[j] = (char)b;
                    j++;
                }
            }

            return j;
        }

        public override int GetMaxByteCount(int charCount)
        {
            throw new NotImplementedException();
        }

        public override int GetMaxCharCount(int byteCount)
        {
            throw new NotImplementedException();
        }

        private static char[] Lookup = new char[]
        {
			'\u3000', '\uFF01', '\u201D', '\uFF03', '\uFF04', '\uFF05', '\uFF06', '\u2019',
			'\uFF08', '\uFF09', '\uFF0A', '\uFF0B', '\uFF0C', '\u2015', '\uFF0E', '\uFF0F',
			'\uFF10', '\uFF11', '\uFF12', '\uFF13', '\uFF14', '\uFF15', '\uFF16', '\uFF17',
			'\uFF18', '\uFF19', '\uFF1A', '\uFF1B', '\uFF1C', '\uFF1D', '\uFF1E', '\uFF1F',
			'\uFF20', '\uFF21', '\uFF22', '\uFF23', '\uFF24', '\uFF25', '\uFF26', '\uFF27',
			'\uFF28', '\uFF29', '\uFF2A', '\uFF2B', '\uFF2C', '\uFF2D', '\uFF2E', '\uFF2F',
			'\uFF30', '\uFF31', '\uFF32', '\uFF33', '\uFF34', '\uFF35', '\uFF36', '\uFF37',
			'\uFF38', '\uFF39', '\uFF3A', '\uFF3B', '\uFF3C', '\uFF3D', '\uFF3E', '\uFF3F',
			'\u2018', '\uFF41', '\uFF42', '\uFF43', '\uFF44', '\uFF45', '\uFF46', '\uFF47',
			'\uFF48', '\uFF49', '\uFF4A', '\uFF4B', '\uFF4C', '\uFF4D', '\uFF4E', '\uFF4F',
			'\uFF50', '\uFF51', '\uFF52', '\uFF53', '\uFF54', '\uFF55', '\uFF56', '\uFF57',
			'\uFF58', '\uFF59', '\uFF5A', '\uFF5B', '\uFF5C', '\uFF5D', '\uFFE3', '\u3041',
			'\u3042', '\u3043', '\u3044', '\u3045', '\u3046', '\u3047', '\u3048', '\u3049',
			'\u304A', '\u304B', '\u304C', '\u304D', '\u304E', '\u304F', '\u3050', '\u3051',
			'\u3052', '\u3053', '\u3054', '\u3055', '\u3056', '\u3057', '\u3058', '\u3059',
			'\u305A', '\u305B', '\u305C', '\u305D', '\u305E', '\u305F', '\u3060', '\u3061',
			'\u3062', '\u3063', '\u3064', '\u3065', '\u3066', '\u3067', '\u3068', '\u3069',
			'\u306A', '\u306B', '\u306C', '\u306D', '\u306E', '\u306F', '\u3070', '\u3071',
			'\u3072', '\u3073', '\u3074', '\u3075', '\u3076', '\u3077', '\u3078', '\u3079',
			'\u307A', '\u307B', '\u307C', '\u307D', '\u307E', '\u307F', '\u3080', '\u3081',
			'\u3082', '\u3083', '\u3084', '\u3085', '\u3086', '\u3087', '\u3088', '\u3089',
			'\u308A', '\u308B', '\u308C', '\u308D', '\u308E', '\u308F', '\u3090', '\u3091',
			'\u3092', '\u3093', '\u30A1', '\u30A2', '\u30A3', '\u30A4', '\u30A5', '\u30A6',
			'\u30A7', '\u30A8', '\u30A9', '\u30AA', '\u30AB', '\u30AC', '\u30AD', '\u30AE',
			'\u30AF', '\u30B0', '\u30B1', '\u30B2', '\u30B3', '\u30B4', '\u30B5', '\u30B6',
			'\u30B7', '\u30B8', '\u30B9', '\u30BA', '\u30BB', '\u30BC', '\u30BD', '\u30BE',
			'\u30BF', '\u30C0', '\u30C1', '\u30C2', '\u30C3', '\u30C4', '\u30C5', '\u30C6',
			'\u30C7', '\u30C8', '\u30C9', '\u30CA', '\u30CB', '\u30CC', '\u30CD', '\u30CE',
			'\u30CF', '\u30D0', '\u30D1', '\u30D2', '\u30D3', '\u30D4', '\u30D5', '\u30D6',
			'\u30D7', '\u30D8', '\u30D9', '\u30DA', '\u30DB', '\u30DC', '\u30DD', '\u30DE',
			'\u30DF', '\u30E0', '\u30E1', '\u30E2', '\u30E3', '\u30E4', '\u30E5', '\u30E6',
			'\u30E7', '\u30E8', '\u30E9', '\u30EA', '\u30EB', '\u30EC', '\u30ED', '\u30EE',
			'\u30EF', '\u30F0', '\u30F1', '\u30F2', '\u30F3', '\u30F4', '\u30F5', '\u30F6',
			'\u3001', '\u3002', '\u30FB', '\u309B', '\u309C', '\u00B4', '\uFF40', '\u00A8',
			'\u30FD', '\u30FE', '\u309D', '\u309E', '\u3003', '\u4EDD', '\u3005', '\u3006',
			'\u3007', '\u30FC', '\u2010', '\uFF5E', '\u2225', '\u2026', '\u2025', '\u201C',
			'\u3014', '\u3015', '\u3008', '\u3009', '\u300A', '\u300B', '\u300C', '\u300D',
			'\u300E', '\u300F', '\u3010', '\u3011', '\uFF0D', '\u00B1', '\u00D7', '\u30FB',
			'\u00F7', '\u2260', '\u2266', '\u2267', '\u221E', '\u2234', '\u2642', '\u2640',
			'\u00B0', '\u2032', '\u2033', '\u2103', '\uFFE5', '\uFFE0', '\uFFE1', '\u00A7',
			'\u2606', '\u2605', '\u25CB', '\u25CF', '\u25CE', '\u25C7', '\u25C6', '\u25A1',
			'\u25A0', '\u25B3', '\u25B2', '\u25BD', '\u25BC', '\u203B', '\u3012', '\u2192',
			'\u2190', '\u2191', '\u2193', '\u3013', '\u0391', '\u0392', '\u0393', '\u0394',
			'\u0395', '\u0396', '\u0397', '\u0398', '\u0399', '\u039A', '\u039B', '\u039C',
			'\u039D', '\u039E', '\u039F', '\u03A0', '\u03A1', '\u03A3', '\u03A4', '\u03A5',
			'\u03A6', '\u03A7', '\u03A8', '\u03A9', '\u03B1', '\u03B2', '\u03B3', '\u03B4',
			'\u03B5', '\u03B6', '\u03B7', '\u03B8', '\u03B9', '\u03BA', '\u03BB', '\u03BC',
			'\u03BD', '\u03BE', '\u03BF', '\u03C0', '\u03C1', '\u03C3', '\u03C4', '\u03C5',
			'\u03C6', '\u03C7', '\u03C8', '\u03C9', '\u0410', '\u0411', '\u0412', '\u0413',
			'\u0414', '\u0415', '\u0401', '\u0416', '\u0417', '\u0418', '\u0419', '\u041A',
			'\u041B', '\u041C', '\u041D', '\u041E', '\u041F', '\u0420', '\u0421', '\u0422',
			'\u0423', '\u0424', '\u0425', '\u0426', '\u0427', '\u0428', '\u0429', '\u042A',
			'\u042B', '\u042C', '\u042D', '\u042E', '\u042F', '\u0430', '\u0431', '\u0432',
			'\u0433', '\u0434', '\u0435', '\u0451', '\u0436', '\u0437', '\u0438', '\u0439',
			'\u043A', '\u043B', '\u043C', '\u043D', '\u043E', '\u043F', '\u0440', '\u0441',
			'\u0442', '\u0443', '\u0444', '\u0445', '\u0446', '\u0447', '\u0448', '\u0449',
        };
    }
}
