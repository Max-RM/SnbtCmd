using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TryashtarUtils.Nbt;

namespace SnbtCmd
{
    class Program
    {
        private const int STD_OUTPUT_HANDLE = -11;
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint GetFinalPathNameByHandle(IntPtr hFile, StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

        private static bool TryGetRedirectedStdoutFilePath(out string path)
        {
            path = null;
            try
            {
                IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
                if (handle == IntPtr.Zero || handle == new IntPtr(-1))
                    return false;
                var sb = new StringBuilder(32768);
                uint len = GetFinalPathNameByHandle(handle, sb, (uint)sb.Capacity, 0);
                if (len == 0)
                    return false;
                string p = sb.ToString();
                if (p.StartsWith(@"\\?\"))
                    p = p.Substring(4);
                path = p;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool LooksLikeFileName(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            if (token.StartsWith("--")) return false;
            try
            {
                var ext = Path.GetExtension(token);
                if (!string.IsNullOrEmpty(ext)) return true;
            }
            catch { }
            return token.Contains(".");
        }

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
                Console.WriteLine("Optional: add '--output-dir <directory>' to specify output directory");
                Console.WriteLine("  --output-dir source: save to source file directory");
                Console.WriteLine("  --output-dir exe: save to executable directory");
                Console.WriteLine("  --output-dir current: save to current working directory");
                Console.WriteLine("Optional: add '--output-file <filename>' to specify output filename (default: out.txt)");
                return;
            }
            
            var consumable_args = new List<string>(args);
            string outputDirectory = null; // decide default based on operation
            string outputFileName = "out.txt"; // default file name
            string inputFilePath = null;
            bool outputFileExplicit = false;
            
            // Parse --output-dir and --output-file parameters
            for (int i = 0; i < consumable_args.Count; i++)
            {
                if (consumable_args[i] == "--output-dir" && i + 1 < consumable_args.Count)
                {
                    outputDirectory = consumable_args[i + 1];
                    consumable_args.RemoveRange(i, 2);
                    i -= 1;
                    continue;
                }
                if (consumable_args[i] == "--output-file" && i + 1 < consumable_args.Count)
                {
                    outputFileName = consumable_args[i + 1];
                    outputFileExplicit = true;
                    consumable_args.RemoveRange(i, 2);
                    i -= 1;
                    continue;
                }
            }
            
            // Positional output filename support (e.g., ... to-snbt myname.txt --output-dir exe)
            // Detect a filename token after operation modifiers, not starting with '--'
            
            Stream input_stream;
            if (consumable_args[0] == "path")
            {
                inputFilePath = consumable_args[1];
                consumable_args.RemoveRange(0, 2);
                // Resolve relative path: prefer CWD, then exe directory
                if (!Path.IsPathRooted(inputFilePath))
                {
                    string cwdCandidate = Path.Combine(Directory.GetCurrentDirectory(), inputFilePath);
                    if (File.Exists(cwdCandidate))
                    {
                        inputFilePath = cwdCandidate;
                    }
                    else
                    {
                        string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        string exeCandidate = Path.Combine(exeDir, inputFilePath);
                        if (File.Exists(exeCandidate))
                            inputFilePath = exeCandidate;
                    }
                }
                input_stream = File.OpenRead(inputFilePath);
            }
            else if (consumable_args[0] == "raw")
            {
                input_stream = Console.OpenStandardInput();
            }
            else
                throw new ArgumentException();
            
            // Determine default output behavior based on operation and redirection if not explicitly set
            string operation = consumable_args[0];
            // Extract optional modifier and positional filename
            if (operation == "to-snbt")
            {
                // Allowed next token: "expanded" or a filename
                if (consumable_args.Count >= 2 && consumable_args[1] != null && consumable_args[1].Equals("expanded", StringComparison.OrdinalIgnoreCase))
                {
                    // keep as is
                }
                else if (consumable_args.Count >= 2 && LooksLikeFileName(consumable_args[1]))
                {
                    outputFileName = consumable_args[1];
                    outputFileExplicit = true;
                    consumable_args.RemoveAt(1);
                }
            }
            else if (operation == "to-nbt")
            {
                // Allowed next token: "gzip" or a filename
                if (consumable_args.Count >= 2 && consumable_args[1] != null && consumable_args[1].Equals("gzip", StringComparison.OrdinalIgnoreCase))
                {
                    // keep as is
                }
                else if (consumable_args.Count >= 2 && LooksLikeFileName(consumable_args[1]))
                {
                    outputFileName = consumable_args[1];
                    outputFileExplicit = true;
                    consumable_args.RemoveAt(1);
                }
            }
            bool outputDirExplicit = outputDirectory != null;
            bool redirected = Console.IsOutputRedirected;
            if (!outputDirExplicit)
            {
                if (redirected && !outputFileExplicit)
                {
                    // If stdout is redirected and the user didn't explicitly request a file/dir,
                    // try to capture the redirected filename and write into the exe directory
                    // unless it would collide with the same path; in that case, honor stdout.
                    string redPath;
                    if (TryGetRedirectedStdoutFilePath(out redPath) && !string.IsNullOrEmpty(redPath))
                    {
                        string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        string intended = Path.GetFullPath(Path.Combine(exeDir, Path.GetFileName(redPath)));
                        string redFull = Path.GetFullPath(redPath);
                        if (string.Equals(intended, redFull, StringComparison.OrdinalIgnoreCase))
                        {
                            // would collide with the same path, so write to stdout
                            outputDirectory = "stdout";
                        }
                        else
                        {
                            outputDirectory = "exe";
                            outputFileName = Path.GetFileName(redPath);
                        }
                    }
                    else
                    {
                        // Fall back to stdout when filename cannot be determined.
                        outputDirectory = "stdout";
                    }
                }
                else
                {
                    if (operation == "to-snbt")
                        outputDirectory = "current";
                    else if (operation == "to-nbt")
                        outputDirectory = "exe";
                    else
                        outputDirectory = "current";
                }
            }

            // Determine output stream based on outputDirectory
            Stream outputStream;
            if (outputDirectory == "source" && !string.IsNullOrEmpty(inputFilePath))
            {
                string sourceDir = Path.GetDirectoryName(Path.GetFullPath(inputFilePath));
                string outputPath = Path.Combine(sourceDir, outputFileName);
                outputStream = File.Create(outputPath);
            }
            else if (outputDirectory == "exe")
            {
                string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string outputPath = Path.Combine(exeDir, outputFileName);
                outputStream = File.Create(outputPath);
            }
            else if (outputDirectory == "current")
            {
                string cwd = Directory.GetCurrentDirectory();
                string outputPath = Path.Combine(cwd, outputFileName);
                outputStream = File.Create(outputPath);
            }
            else // stdout
            {
                outputStream = Console.OpenStandardOutput();
            }
            
            using (outputStream)
            using (var writer = new StreamWriter(outputStream))
            {
                if (operation == "to-snbt")
                {
                    var nbt_file = new fNbt.NbtFile();
                    nbt_file.LoadFromStream(input_stream, fNbt.NbtCompression.AutoDetect);
                    var options = SnbtOptions.Default;
                    if (consumable_args.Count >= 2 && consumable_args[1] == "expanded")
                        options = SnbtOptions.DefaultExpanded;
                    writer.Write(Snbt.ToSnbt(nbt_file.RootTag, options));
                }
                else if (operation == "to-nbt")
                {
                    using (var stream = new StreamReader(input_stream))
                    {
                        var snbt = stream.ReadToEnd();
                        var data = SnbtParser.Parse(snbt, false);
                        data.Name = "";
                        var file = new fNbt.NbtFile(data);
                        var compression = fNbt.NbtCompression.None;
                        if (consumable_args.Count >= 2 && consumable_args[1] == "gzip")
                            compression = fNbt.NbtCompression.GZip;
                        file.SaveToStream(outputStream, compression);
                    }
                }
            }
        }
    }
}

