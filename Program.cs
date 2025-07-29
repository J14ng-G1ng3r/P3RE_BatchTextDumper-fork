using CUE4Parse.Compression;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using System.Diagnostics;
using System.Text;
using P3RE_BatchTextDumper_UI;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length >= 3)
        {
            string compilerPath = args[0];
            string folderPath = args[1];
            string outputPath = args[2];
            string languageCode = args.Length > 3 ? args[3] : "";
            
            if (!File.Exists(compilerPath))
            {
                Console.WriteLine($"Error: Compiler not found at {compilerPath}");
                return;
            }
            
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"Error: Paks folder not found at {folderPath}");
                return;
            }
            
            if (!Directory.Exists(outputPath))
            {
                Console.WriteLine($"Error: Output folder not found at {outputPath}");
                return;
            }
            
            RunExtraction(compilerPath, folderPath, outputPath, languageCode);
        }
        else
        {
            RunInteractiveMode();
        }
    }
    
    private static void RunInteractiveMode()
    {
        string compilerPath = string.Empty;
        string paksPath = string.Empty;
        string outputPath = string.Empty;
        string languageCode = string.Empty;
        
        while (true)
        {
            string[] menuOptions = { "Set AtlusScriptCompiler Path", "Set P3R Paks Folder Path", "Set Output Folder Path", "Set Language Code", "Start Extraction", "Exit" };
            int choice = P3REBatchTextDumperUI.ShowMenuWithArrowSelection("P3RE Batch Text Dumper - Interactive CLI UI", menuOptions);
            
            switch (choice)
            {
                case 1:
                    compilerPath = P3REBatchTextDumperUI.GetCompilerPath();
                    break;
                case 2:
                    paksPath = P3REBatchTextDumperUI.GetPaksPath();
                    break;
                case 3:
                    outputPath = P3REBatchTextDumperUI.GetOutputPath();
                    break;
                case 4:
                    languageCode = P3REBatchTextDumperUI.GetLanguageCode();
                    break;
                case 5:
                    if (P3REBatchTextDumperUI.ValidatePaths(compilerPath, paksPath, outputPath))
                    {
                        P3REBatchTextDumperUI.StartExtraction(compilerPath, paksPath, outputPath, languageCode);
                    }
                    else
                    {
                        Console.WriteLine("Please set all required paths first.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    break;
                case 6:
                    Console.WriteLine("Goodbye!");
                    return;
            }
        }
    }
    
    private static void RunExtraction(string compilerPath, string folderPath, string outputPath, string languageCode)
    {
        ZlibHelper.DownloadDll();
        ZlibHelper.Initialize(ZlibHelper.DLL_NAME);
        
        const string _aesKey = "92BADFE2921B376069D3DE8541696D230BA06B5E4320084DD34A26D117D2FFEE";
        
        var dumpBuilder = new StringBuilder();
        dumpBuilder.AppendLine("P3R Message DUMP:\n");
        
        Console.WriteLine($"Using folder: {folderPath}");
        
        ProcessExtraction(_aesKey, dumpBuilder, compilerPath, folderPath, outputPath, languageCode);
    }
    
    private static void ProcessExtraction(string _aesKey, StringBuilder dumpBuilder, string? compilerPath, string? folderPath, string? outputPath, string? lang)
    {
        var provider = new DefaultFileProvider(
            directory: folderPath,
            searchOption: SearchOption.TopDirectoryOnly,
            isCaseInsensitive: true,
            versions: new VersionContainer(EGame.GAME_UE4_28));
            
        provider.Initialize();
        
        provider.SubmitKey(new CUE4Parse.UE4.Objects.Core.Misc.FGuid(), new CUE4Parse.Encryption.Aes.FAesKey(_aesKey));
        
        provider.PostMount();
        
        var files = provider.Files.Values;
        
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
