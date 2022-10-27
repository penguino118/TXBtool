using System;
using System.IO;

namespace TXBtool
{
    class Program
    {
        public static int texcount { get; private set; }


        static void Main(string[] args)
        {
            Console.WriteLine("TXBtool v1.2 by Penguino");
            bool ClutFix = true;

            if (args.Length > 0 && args[0] == "-u" && File.Exists(args[1]))
            {
                Console.WriteLine("Unpacking " + Path.GetFileName(args[1]));
                File.OpenRead(args[1]);
                string TXBpath = args[1];
                byte[] inputTXB = File.ReadAllBytes(args[1]);
                if (args.Length > 2)
                    if (args[2] == "-k") ClutFix = false;
                if (ClutFix == false) Console.WriteLine("CLUT size will be kept as is.");
                TXBex(TXBpath, inputTXB, ClutFix);
            }
            if (args.Length > 0 && args[0] == "-p" && File.Exists(args[1]))
            {
                Console.WriteLine("Repacking " + Path.GetFileName(args[1]));
                File.OpenRead(args[1]);
                string TXBpath = args[1];
                byte[] inputTXB = File.ReadAllBytes(args[1]);
                if (args.Length > 2)
                    if (args[2] == "-k") ClutFix = false;
                if (ClutFix == false) Console.WriteLine("CLUT size will be kept as is.");
                TXBre(TXBpath, inputTXB, ClutFix);
            }
            if (args.Length > 0 && !File.Exists(args[1])) Console.WriteLine("Error: File not found.");
            if (args.Length == 1) Console.WriteLine("Error: No file input.");
            if (args.Length == 0) Console.WriteLine("Usage: program.exe (-u / -p) INPUT (-k)\n|| -u: unpack || -p: repack || -k: keep original CLUT size ||\n\nExample: TXBtool.exe -u pl01.txb\n         TXBtool.exe -p pl2f.txb");
        }


        static void TXBex(string TXBpath, byte[] TXBin, bool clutfix)
        {
            int texcount = Buffer.GetByte(TXBin, 0x00);
            Console.WriteLine("Texture Count: " + texcount);
            for (int k = 0; k < texcount; k++)
            {
                byte[] IDArray = { Buffer.GetByte(TXBin, 0x08 + k * 8), Buffer.GetByte(TXBin, 0x09 + k * 8), Buffer.GetByte(TXBin, 0x0A + k * 8), Buffer.GetByte(TXBin, 0x0B + k * 8) };
                byte[] OffArray = { Buffer.GetByte(TXBin, 0x0C + k * 8), Buffer.GetByte(TXBin, 0x0D + k * 8), Buffer.GetByte(TXBin, 0x0E + k * 8), Buffer.GetByte(TXBin, 0x0F + k * 8) };
                int texID = BitConverter.ToInt32(IDArray, 0);                 //internal image ID
                int texOffset = BitConverter.ToInt32(OffArray, 0);            //where the image is in the TXB
                int alignment = Buffer.GetByte(TXBin, 0x05 + texOffset);      //what byte alignment the image is using 
                int shortclutcount = Buffer.GetByte(TXBin, 0x14 + texOffset); //the color count on a 16 byte aligned image
                int longclutcount = Buffer.GetByte(TXBin, 0x8E + texOffset);  //the color count on a 128 byte aligned image
                byte[] shortsize = { Buffer.GetByte(TXBin, 0x10 + texOffset), Buffer.GetByte(TXBin, 0x11 + texOffset), Buffer.GetByte(TXBin, 0x12 + texOffset), Buffer.GetByte(TXBin, 0x13 + texOffset) };//16  byte clut size
                byte[] longsize = { Buffer.GetByte(TXBin, 0x80 + texOffset), Buffer.GetByte(TXBin, 0x81 + texOffset), Buffer.GetByte(TXBin, 0x82 + texOffset), Buffer.GetByte(TXBin, 0x83 + texOffset) }; //128 byte clut size

                //Console.WriteLine("Texture " + k + " ID: " + texID + "\nTexture " + k + " Offset: " + texOffset);

                using (var stream = File.Create(Path.ChangeExtension(TXBpath, null) + "_img" + texID + ".tm2"))
                {
                    stream.Write(TXBin, texOffset, TXBin.Length - texOffset);
                    if (alignment == 0)
                    {
                        stream.SetLength(BitConverter.ToInt32(shortsize, 0) + 16);
                        if (clutfix == true && shortclutcount == 16) //fixes clut size on 16 color 16 byte images
                        {
                            stream.Seek(0x14, 0x0);
                            stream.WriteByte(0x40);
                        }
                    }
                    if (alignment == 1)
                    {
                        stream.SetLength(BitConverter.ToInt32(longsize, 0) + 128);
                        if (clutfix == true && longclutcount == 16) //fixes clut size on 16 color 128 byte images
                        {
                            stream.Seek(0x84, 0x0);
                            stream.WriteByte(0x40);
                        }
                    }
                }

                Console.WriteLine("Extracted: " + Path.GetFileName(Path.ChangeExtension(TXBpath, null)) + "_img" + texID + ".tm2");
            }
        }
        static void TXBre(string TXBpath, byte[] TXBin, bool clutfix)
        {
            int texcount = Buffer.GetByte(TXBin, 0x00);
            //Console.WriteLine("Texture Count:" + texcount);

            var newTXB = File.Create(Path.ChangeExtension(TXBpath, null) + "_repack.txb");
            newTXB.Close();

            for (int k = 0; k < texcount; k++)
            {
                byte[] IDArray = { Buffer.GetByte(TXBin, 0x08 + k * 8), Buffer.GetByte(TXBin, 0x09 + k * 8), Buffer.GetByte(TXBin, 0x0A + k * 8), Buffer.GetByte(TXBin, 0x0B + k * 8) };
                int texID = BitConverter.ToInt32(IDArray, 0);		//internal image ID
                byte[] OffArray = { Buffer.GetByte(TXBin, 0x0C + k * 8), Buffer.GetByte(TXBin, 0x0D + k * 8), Buffer.GetByte(TXBin, 0x0E + k * 8), Buffer.GetByte(TXBin, 0x0F + k * 8) };
                int texOffset = BitConverter.ToInt32(OffArray, 0);	//where the image is in the TXB

                byte[] TM2in = File.ReadAllBytes(Path.ChangeExtension(TXBpath, null) + "_img" + texID + ".tm2");

                int TM2alignment = Buffer.GetByte(TM2in, 0x05);		//what byte alignment the image is using 
                int TM2sclutcount = Buffer.GetByte(TM2in, 0x14);	//the color count is on a 16 byte aligned image
                int TM2lclutcount = Buffer.GetByte(TM2in, 0x8E);	//the color count is on a 128 byte aligned image

                if (TM2alignment == 0 && clutfix == true && TM2sclutcount == 16) Buffer.SetByte(TM2in, 0x14, 0x80);
                //reverts clut size on 16 color 16 byte images
                if (TM2alignment == 1 && clutfix == true && TM2lclutcount == 16) Buffer.SetByte(TM2in, 0x84, 0x80);
                //reverts clut size on 16 color 128 byte images

                Buffer.BlockCopy(TM2in, 0x0, TXBin, texOffset, TM2in.Length);
                Console.WriteLine(Path.GetFileName(Path.ChangeExtension(TXBpath, null)) + "_img" + texID + ".tm2" + " has been inserted at offset " + texOffset);
            }

            File.WriteAllBytes(Path.ChangeExtension(TXBpath, null) + "_repack.txb", TXBin);
            Console.WriteLine("Saved as: " + Path.ChangeExtension(TXBpath, null) + "_repack.txb");
        }
    }
}