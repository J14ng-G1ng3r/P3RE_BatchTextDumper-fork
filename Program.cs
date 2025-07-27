using CUE4Parse.Compression;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using System.Diagnostics;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        ZlibHelper.DownloadDll();
        ZlibHelper.Initialize(ZlibHelper.DLL_NAME);
        const string _aesKey = "92BADFE2921B376069D3DE8541696D230BA06B5E4320084DD34A26D117D2FFEE";
        var dumpBuilder = new StringBuilder();
        dumpBuilder.AppendLine("P3R Message DUMP:\n");
        string? compilerPath = null;
        while (true)
        {
            Console.Write("Please enter the full path to AtlusScriptCompiler.exe: ");
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && File.Exists(input))
            {
                compilerPath = input;
                break;
            }
            Console.WriteLine("Invalid path or file does not exist. Please try again.");
        }
        string? folderPath = null;
        string? outputPath = null;

        while (true)
        {
            Console.Write("Please enter the full folder path to the output folder: ");
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input) && Directory.Exists(input))
            {
                outputPath = input;
                break;
            }
            Console.WriteLine("Invalid folder path. Please try again.");
        }

        while (true)
        {
            Console.Write("Please enter the full folder path to the P3R Paks folder: ");
            string? input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input) && Directory.Exists(input))
            {
                folderPath = input;
                break;
            }
            Console.WriteLine("Invalid folder path. Please try again.");
        }

        Console.WriteLine($"Using folder: {folderPath}");
        var provider = new DefaultFileProvider(
            directory: folderPath,
            searchOption: SearchOption.TopDirectoryOnly,
            isCaseInsensitive: true,
            versions: new VersionContainer(EGame.GAME_UE4_28));
        provider.Initialize();
        provider.SubmitKey(new CUE4Parse.UE4.Objects.Core.Misc.FGuid(), new CUE4Parse.Encryption.Aes.FAesKey(_aesKey));
        provider.PostMount();
        var files = provider.Files.Values;
        Console.Write("Enter language code (leave blank for JP/Xrd777): ");
        string? lang = Console.ReadLine();
        bool isJapanese = string.IsNullOrWhiteSpace(lang);
        string? l10nPath = isJapanese ? null : $"L10N/{lang.Trim().ToLower()}";

        var filteredFiles = files.Where(file =>
        {
            var path = file.Path;
            bool matchesLanguage = isJapanese
                ? !path.Contains("L10N") && path.Contains("Xrd777")
                : path.Contains(l10nPath!);

            return matchesLanguage && path.Split('/').Last().StartsWith("BMD");
        }).ToList();

        Console.WriteLine($"Found {filteredFiles.Count} matching files.");

        foreach (var file in filteredFiles)
        {
            try
            {
                Console.WriteLine($"Processing: {file.Path}");
                var data = provider.SaveAsset(file);
                string savePath = Path.Combine(outputPath!, file.Path.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                File.WriteAllBytes(savePath, data);
                string escapedCompiler = compilerPath.Replace("'", "''");
                string escapedUasset = savePath.Replace("'", "''");
                string psCommand = $"& '{escapedCompiler}' '{escapedUasset}' -Decompile -InFormat MessageScriptBinary -Library P3RE -Encoding UTF-8 -OutFormat V1RE";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"{psCommand}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                // Read decompiled output
                string msgPath = Path.Combine(
                    Path.GetDirectoryName(savePath)!,
                    Path.GetFileNameWithoutExtension(savePath) + "_unwrapped.bmd.msg"
                );
                string bmdPath = Path.Combine(
                    Path.GetDirectoryName(savePath)!,
                    Path.GetFileNameWithoutExtension(savePath) + "_unwrapped.bmd"
                );
                string hPath = Path.Combine(
                    Path.GetDirectoryName(savePath)!,
                    Path.GetFileNameWithoutExtension(savePath) + "_unwrapped.bmd.msg.h"
                );

                if (File.Exists(msgPath))
                {
                    string relativePath = Path.GetRelativePath(outputPath!, savePath);
                    string decompiledText = File.ReadAllText(msgPath);

                    dumpBuilder.AppendLine(relativePath);
                    dumpBuilder.AppendLine(decompiledText);
                    dumpBuilder.AppendLine();
                }
                else
                {
                    Console.WriteLine($"Decompiled file not found: {msgPath}");
                }

                File.Delete(savePath);
                File.Delete(msgPath);
                File.Delete(bmdPath);
                File.Delete(hPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to extract {file.Path}: {ex.Message}");
            }
        }

        string dumpPath = Path.Combine(outputPath!, $"P3RMessageDump.txt");
        File.WriteAllText(dumpPath, dumpBuilder.ToString());
        Console.WriteLine($"Dump saved to: {dumpPath}");
    }
}
