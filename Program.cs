using CUE4Parse.Compression;  // 引入CUE4Parse库的压缩相关功能
using CUE4Parse.FileProvider;  // 引入CUE4Parse库的文件提供功能
using CUE4Parse.UE4.Versions;  // 引入CUE4Parse库的UE4版本管理功能
using System.Diagnostics;      // 引入系统诊断功能，用于启动外部进程
using System.Text;             // 引入系统文本功能，用于字符串构建
using P3RE_BatchTextDumper_UI; // 引入CLI UI项目命名空间

// 定义程序的主类
internal class Program
{
    // 程序入口点，Main方法是C#控制台应用程序的入口
    private static void Main(string[] args)
    {
        // 检查是否有命令行参数
        if (args.Length >= 3)
        {
            // 从命令行参数获取路径
            string compilerPath = args[0];
            string folderPath = args[1];
            string outputPath = args[2];
            string languageCode = args.Length > 3 ? args[3] : "";
            
            // 验证路径
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
            
            // 使用命令行参数运行程序
            RunExtraction(compilerPath, folderPath, outputPath, languageCode);
        }
        else
        {
            // 运行CLI UI
            RunInteractiveMode();
        }
    }
    
    // 交互式模式
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
    
    // 使用命令行参数运行提取过程
    private static void RunExtraction(string compilerPath, string folderPath, string outputPath, string languageCode)
    {
        // 下载并初始化Zlib压缩库，用于处理压缩文件
        ZlibHelper.DownloadDll();
        ZlibHelper.Initialize(ZlibHelper.DLL_NAME);
        
        // 定义游戏资源的AES解密密钥，用于解密游戏文件
        const string _aesKey = "92BADFE2921B376069D3DE8541696D230BA06B5E4320084DD34A26D117D2FFEE";
        
        // 创建一个字符串构建器，用于存储最终的文本输出
        var dumpBuilder = new StringBuilder();
        dumpBuilder.AppendLine("P3R Message DUMP:\n");
        
        // 显示正在使用的文件夹路径
        Console.WriteLine($"Using folder: {folderPath}");
        
        // 执行提取过程
        ProcessExtraction(_aesKey, dumpBuilder, compilerPath, folderPath, outputPath, languageCode);
    }
    
    // 处理提取过程的核心逻辑
    private static void ProcessExtraction(string _aesKey, StringBuilder dumpBuilder, string? compilerPath, string? folderPath, string? outputPath, string? lang)
    {
        // 创建文件提供器实例，用于访问游戏资源文件
        var provider = new DefaultFileProvider(
            directory: folderPath,           // 指定要扫描的目录
            searchOption: SearchOption.TopDirectoryOnly,  // 只搜索顶层目录
            isCaseInsensitive: true,         // 不区分大小写
            versions: new VersionContainer(EGame.GAME_UE4_28));  // 指定游戏版本
            
        // 初始化文件提供器
        provider.Initialize();
        
        // 提交AES密钥用于解密游戏资源
        provider.SubmitKey(new CUE4Parse.UE4.Objects.Core.Misc.FGuid(), new CUE4Parse.Encryption.Aes.FAesKey(_aesKey));
        
        // 完成挂载操作
        provider.PostMount();
        
        // 获取所有文件列表
        var files = provider.Files.Values;
        
        // 判断是否为日文（默认）
        bool isJapanese = string.IsNullOrWhiteSpace(lang);
        
        // 根据语言代码构建本地化路径
        string? l10nPath = isJapanese ? null : $"L10N/{lang.Trim().ToLower()}";

        // 筛选出符合语言要求和文件名要求的BMD文件
        var filteredFiles = files.Where(file =>
        {
            // 获取文件路径
            var path = file.Path;
            // 根据是否为日文版本判断匹配条件
            bool matchesLanguage = isJapanese
                ? !path.Contains("L10N") && path.Contains("Xrd777")  // 日文版本：不包含L10N且包含Xrd777
                : path.Contains(l10nPath!);  // 其他语言：包含指定的本地化路径

            // 返回同时满足语言条件和文件名以BMD开头的文件
            return matchesLanguage && path.Split('/').Last().StartsWith("BMD");
        }).ToList();

        // 显示找到的符合条件的文件数量
        Console.WriteLine($"Found {filteredFiles.Count} matching files.");

        // 遍历处理每个筛选出的文件
        foreach (var file in filteredFiles)
        {
            try
            {
                // 显示正在处理的文件路径
                Console.WriteLine($"Processing: {file.Path}");
                
                // 保存资源文件数据
                var data = provider.SaveAsset(file);
                
                // 构建保存路径
                string savePath = Path.Combine(outputPath!, file.Path.Replace('/', Path.DirectorySeparatorChar));
                
                // 创建目录（如果不存在）
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                
                // 将数据写入文件
                File.WriteAllBytes(savePath, data);
                
                // 转义路径中的单引号
                string escapedCompiler = compilerPath.Replace("'", "''");
                string escapedUasset = savePath.Replace("'", "''");
                
                // 构建PowerShell命令
                string psCommand = $"& '{escapedCompiler}' '{escapedUasset}' -Decompile -InFormat MessageScriptBinary -Library P3RE -Encoding UTF-8 -OutFormat V1RE";

                // 创建进程对象
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",        // 指定要启动的程序
                        Arguments = $"-Command \"{psCommand}\"",  // 指定程序参数
                        UseShellExecute = false,            // 不使用操作系统shell启动
                        CreateNoWindow = true               // 不创建新窗口
                    }
                };

                // 启动进程并等待完成
                process.Start();
                process.WaitForExit();

                // 构建反编译后文件的路径
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

                // 检查反编译后的文件是否存在
                if (File.Exists(msgPath))
                {
                    // 获取相对路径
                    string relativePath = Path.GetRelativePath(outputPath!, savePath);
                    
                    // 读取反编译后的文本内容
                    string decompiledText = File.ReadAllText(msgPath);

                    // 将路径和文本内容添加到输出构建器
                    dumpBuilder.AppendLine(relativePath);
                    dumpBuilder.AppendLine(decompiledText);
                    dumpBuilder.AppendLine();
                }
                else
                {
                    // 如果文件不存在，输出提示信息
                    Console.WriteLine($"Decompiled file not found: {msgPath}");
                }

                // 删除临时文件
                File.Delete(savePath);
                File.Delete(msgPath);
                File.Delete(bmdPath);
                File.Delete(hPath);
            }
            catch (Exception ex)
            {
                // 处理异常情况
                Console.WriteLine($"Failed to extract {file.Path}: {ex.Message}");
            }
        }

        // 构建最终输出文件的路径
        string dumpPath = Path.Combine(outputPath!, $"P3RMessageDump.txt");
        
        // 将收集到的所有文本写入输出文件
        File.WriteAllText(dumpPath, dumpBuilder.ToString());
        
        // 显示输出文件保存路径
        Console.WriteLine($"Dump saved to: {dumpPath}");
    }
}
