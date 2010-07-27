using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gibbed.Atlus.Test
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //var input = File.OpenRead(@"T:\Atlus\p3p\usa\data\init_free\field\script\field.bf");
            //var input = File.OpenRead(@"T:\Atlus\p3p\usa\data\event\e850\e850_004.bf");
            //var input = File.OpenRead(@"T:\Atlus\p3p\usa\data\event\e130\e131_002.bf");
            //var input = File.OpenRead(@"T:\Atlus\p3p\usa\data\field2d\bg\b06_01\n06_01.bf");
            //var input = File.OpenRead(@"T:\Atlus\p3p\usa\data\event\e130\e138_302.bf");
            //var bf = new FileFormats.BfFile();
            //bf.Deserialize(input);
            //input.Close();

            /*
            var input = File.OpenRead(@"T:\Atlus\p3p\usa\data\bustup\b78_162_0\b78_162_0.tmx");
            var txd = new FileFormats.TxdFile();
            txd.Deserialize(input);
            input.Close();
            */

            /*
            var names = Directory.GetFiles(@"T:\Atlus\p3p\usa\data", "*.tmx", SearchOption.AllDirectories);
            foreach (var name in names)
            {
                var input = File.OpenRead(name);
                var txd = new FileFormats.TxdFile();
                txd.Deserialize(input);
                input.Close();
            }
            */

            var e = new FileFormats.GameEncoding();
            //var s = e.GetString(new byte[] { 0x80, 0x9E, 0x81, 0xD8, 0x81, 0xE1, 0x81, 0xD4, 0x81, 0xC1, 0x81, 0xF3, 0x82, 0x99, 0x81, 0xDA });
            var s = e.GetString(new byte[] { 0xF2, 0x05 });
            //var s = e.GetString(new byte[] { 0xF5, 0x86, 0x4F, 0x01, 0xA2, 0x01, 0x01, 0x01, 0x01, 0x01 });
        }
    }
}
