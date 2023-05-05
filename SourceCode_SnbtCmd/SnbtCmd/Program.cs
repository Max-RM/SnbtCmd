using System;
using System.Collections.Generic;
using System.IO;
using TryashtarUtils.Nbt;

namespace SnbtCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Use:");
                Console.WriteLine("First type mode 'path' or 'raw'");
                Console.WriteLine("If using 'path', next type the file path");
                Console.WriteLine("Next type 'to-snbt' or 'to-nbt'");
                Console.WriteLine("If using 'to-snbt', add 'expanded' for pretty-print)");
                Console.WriteLine("If using 'to-nbt', add 'gzip' for g-zip compression)");
                return;
            }
            var consumable_args = new List<string>(args);
            Stream input_stream;
            if (consumable_args[0] == "path")
            {
                string path = consumable_args[1];
                consumable_args.RemoveRange(0, 2);
                input_stream = File.OpenRead(path);
            }
            else if (consumable_args[0] == "raw")
            {
                input_stream = Console.OpenStandardInput();
            }
            else
                throw new ArgumentException();
            using Stream stdout = Console.OpenStandardOutput();
            using var writer = new StreamWriter(stdout);
            if (consumable_args[0] == "to-snbt")
            {
                var nbt_file = new fNbt.NbtFile();
                nbt_file.LoadFromStream(input_stream, fNbt.NbtCompression.AutoDetect);
                var options = SnbtOptions.Default;
                if (consumable_args.Count >= 2 && consumable_args[1] == "expanded")
                    options = SnbtOptions.DefaultExpanded;
                writer.Write(Snbt.ToSnbt(nbt_file.RootTag, options));
            }
            else if (consumable_args[0] == "to-nbt")
            {
                using var stream = new StreamReader(input_stream);
                var snbt = stream.ReadToEnd();
                var data = SnbtParser.Parse(snbt, false);
                data.Name = "";
                var file = new fNbt.NbtFile(data);
                var compression = fNbt.NbtCompression.None;
                if (consumable_args.Count >= 2 && consumable_args[1] == "gzip")
                    compression = fNbt.NbtCompression.GZip;
                file.SaveToStream(stdout, compression);
            }
        }
    }
}

