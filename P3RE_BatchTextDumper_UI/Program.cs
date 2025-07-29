using System;  // 引入系统基础功能库
using System.Diagnostics;  // 引入系统诊断功能，用于启动外部进程
using System.IO;  // 引入输入输出功能，用于文件和目录操作
using System.Runtime.InteropServices;  // 引入运行时互操作服务，用于与操作系统交互

// 定义命名空间，将相关的类组织在一起
namespace P3RE_BatchTextDumper_UI
{
    // 定义公共类P3REBatchTextDumperUI，包含所有CLI用户界面相关的方法
    public class P3REBatchTextDumperUI
    {
        /// <summary>
        /// 显示带有方向键选择功能的菜单
        /// </summary>
        /// <param name="title">菜单标题</param>
        /// <param name="menuOptions">菜单选项数组</param>
        /// <returns>选中的菜单项索引（从1开始）</returns>
        public static int ShowMenuWithArrowSelection(string title, string[] menuOptions)
        {
            int selectedIndex = 0;  // 当前选中的菜单项索引（从0开始）
            ConsoleKey key;  // 存储用户按下的键
            
            // 循环显示菜单，直到用户按下Enter键确认选择
            do
            {
                Console.Clear();  // 清除控制台屏幕
                Console.WriteLine(title);  // 显示菜单标题
                Console.WriteLine(new string('=', title.Length));  // 显示与标题长度相同的分隔线
                Console.WriteLine();  // 输出一个空行
                
                // 遍历所有菜单选项
                for (int i = 0; i < menuOptions.Length; i++)
                {
                    // 检查当前选项是否为选中项
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;  // 设置前景色为绿色
                        Console.WriteLine($"> {menuOptions[i]}");  // 以绿色高亮显示选中项，并在前面加上>符号
                        Console.ResetColor();  // 重置控制台颜色为默认值
                    }
                    else
                    {
                        Console.WriteLine($"  {menuOptions[i]}");  // 显示未选中项，前面有空格用于对齐
                    }
                }
                
                Console.WriteLine();  // 输出一个空行
                Console.WriteLine("Use ↑/↓ arrow keys to select, Enter to confirm");  // 显示操作提示
                
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);  // 读取用户按键，true表示不显示按键字符
                key = keyInfo.Key;  // 获取按键信息
                
                // 根据用户按下的键执行相应操作
                switch (key)
                {
                    case ConsoleKey.UpArrow:  // 如果按下上箭头键
                        // 如果当前不是第一项，则选择上一项；否则选择最后一项
                        selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : menuOptions.Length - 1;
                        break;
                    case ConsoleKey.DownArrow:  // 如果按下下箭头键
                        // 如果当前不是最后一项，则选择下一项；否则选择第一项
                        selectedIndex = (selectedIndex < menuOptions.Length - 1) ? selectedIndex + 1 : 0;
                        break;
                    case ConsoleKey.Enter:  // 如果按下回车键
                        Console.WriteLine($"> {menuOptions[selectedIndex]}");  // 显示选中的选项
                        Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
                        Console.ReadKey();  // 等待用户按键
                        return selectedIndex + 1;  // 返回选中项的索引（从1开始）
                }
            } while (key != ConsoleKey.Enter);  // 当用户未按下回车键时继续循环
            
            // 这个返回语句理论上不会被执行，但为了满足编译器要求而存在
            return 1;
        }
        
        /// <summary>
        /// 获取AtlusScriptCompiler.exe的路径
        /// </summary>
        /// <returns>有效的编译器路径</returns>
        public static string GetCompilerPath()
        {
            // 无限循环，直到用户提供有效的路径
            while (true)
            {
                Console.Write("Enter the full path to AtlusScriptCompiler.exe: ");  // 提示用户输入路径
                string? input = Console.ReadLine();  // 读取用户输入
                
                // 检查输入是否不为空且文件存在
                if (!string.IsNullOrWhiteSpace(input) && File.Exists(input))
                {
                    Console.WriteLine("Compiler path set successfully!");  // 显示成功消息
                    Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
                    Console.ReadKey();  // 等待用户按键
                    return input;  // 返回有效的路径
                }
                else
                {
                    Console.WriteLine("Invalid path or file does not exist. Please try again.");  // 显示错误消息
                    Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
                    Console.ReadKey();  // 等待用户按键
                }
            }
        }
        
        /// <summary>
        /// 获取P3R游戏资源包文件夹路径
        /// </summary>
        /// <returns>有效的游戏资源包文件夹路径</returns>
        public static string GetPaksPath()
        {
            // 无限循环，直到用户提供有效的路径
            while (true)
            {
                Console.Write("Enter the full folder path to the P3R Paks folder: ");  // 提示用户输入路径
                string? input = Console.ReadLine();  // 读取用户输入
                
                // 检查输入是否不为空且目录存在
                if (!string.IsNullOrWhiteSpace(input) && Directory.Exists(input))
                {
                    Console.WriteLine("Paks folder path set successfully!");  // 显示成功消息
                    Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
                    Console.ReadKey();  // 等待用户按键
                    return input;  // 返回有效的路径
                }
                else
                {
                    Console.WriteLine("Invalid folder path. Please try again.");  // 显示错误消息
                    Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
                    Console.ReadKey();  // 等待用户按键
                }
            }
        }
        
        /// <summary>
        /// 获取输出文件夹路径
        /// </summary>
        /// <returns>有效的输出文件夹路径</returns>
        public static string GetOutputPath()
        {
            // 无限循环，直到用户提供有效的路径
            while (true)
            {
                Console.Write("Enter the full folder path to the output folder: ");  // 提示用户输入路径
                string? input = Console.ReadLine();  // 读取用户输入
                
                // 检查输入是否不为空且目录存在
                if (!string.IsNullOrWhiteSpace(input) && Directory.Exists(input))
                {
                    Console.WriteLine("Output folder path set successfully!");  // 显示成功消息
                    Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
                    Console.ReadKey();  // 等待用户按键
                    return input;  // 返回有效的路径
                }
                else
                {
                    Console.WriteLine("Invalid folder path. Please try again.");  // 显示错误消息
                    Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
                    Console.ReadKey();  // 等待用户按键
                }
            }
        }
        
        /// <summary>
        /// 获取语言代码
        /// </summary>
        /// <returns>语言代码，如果用户未输入则返回空字符串</returns>
        public static string GetLanguageCode()
        {
            Console.Write("Enter language code (leave blank for JP/Xrd777): ");  // 提示用户输入语言代码
            string? input = Console.ReadLine();  // 读取用户输入
            
            // 检查用户是否未输入任何内容
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Language set to default (JP/Xrd777)");  // 显示默认语言设置消息
                return string.Empty;  // 返回空字符串
            }
            else
            {
                Console.WriteLine($"Language set to: {input}");  // 显示用户设置的语言代码
            }
            
            Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
            Console.ReadKey();  // 等待用户按键
            return input;  // 返回用户输入的语言代码
        }
        
        /// <summary>
        /// 验证所有必需的路径是否有效
        /// </summary>
        /// <param name="compilerPath">编译器路径</param>
        /// <param name="paksPath">游戏资源包文件夹路径</param>
        /// <param name="outputPath">输出文件夹路径</param>
        /// <returns>如果所有路径都有效则返回true，否则返回false</returns>
        public static bool ValidatePaths(string compilerPath, string paksPath, string outputPath)
        {
            // 检查所有路径都不为空，并且文件/目录存在
            return !string.IsNullOrWhiteSpace(compilerPath) && 
                   !string.IsNullOrWhiteSpace(paksPath) && 
                   !string.IsNullOrWhiteSpace(outputPath) &&
                   File.Exists(compilerPath) && 
                   Directory.Exists(paksPath) && 
                   Directory.Exists(outputPath);
        }
        
        /// <summary>
        /// 启动文本提取过程
        /// </summary>
        /// <param name="compilerPath">编译器路径</param>
        /// <param name="paksPath">游戏资源包文件夹路径</param>
        /// <param name="outputPath">输出文件夹路径</param>
        /// <param name="languageCode">语言代码</param>
        public static void StartExtraction(string compilerPath, string paksPath, string outputPath, string languageCode)
        {
            Console.WriteLine("Starting extraction process...");  // 显示开始提取消息
            
            try
            {
                // 构建传递给主程序的参数字符串
                string arguments = $"\"{compilerPath}\" \"{paksPath}\" \"{outputPath}\"";
                // 如果提供了语言代码，则将其添加到参数中
                if (!string.IsNullOrWhiteSpace(languageCode))
                {
                    arguments += $" \"{languageCode}\"";
                }
                
                // 获取主程序的路径
                string mainProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "P3RE_BatchTextDumper.exe");
                
                // 检查主程序是否存在
                if (!File.Exists(mainProgramPath))
                {
                    // 如果在上级目录未找到，则尝试在当前目录查找
                    mainProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "P3RE_BatchTextDumper.exe");
                    if (!File.Exists(mainProgramPath))
                    {
                        Console.WriteLine("Error: Main program not found.");  // 显示错误消息
                        Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
                        Console.ReadKey();  // 等待用户按键
                        return;  // 退出方法
                    }
                }
                
                // 创建启动主程序的进程信息
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = mainProgramPath,  // 设置要启动的程序文件名
                    Arguments = arguments,  // 设置传递给程序的参数
                    UseShellExecute = false,  // 不使用操作系统shell启动
                    RedirectStandardOutput = true,  // 重定向标准输出
                    RedirectStandardError = true,  // 重定向标准错误输出
                    CreateNoWindow = false  // 创建窗口
                };
                
                // 启动进程并获取输出
                using (Process? process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        // 读取程序的标准输出和错误输出
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        
                        process.WaitForExit();  // 等待程序执行完成
                        
                        // 显示程序输出
                        Console.WriteLine(output);
                        // 如果有错误输出，则显示错误信息
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            Console.WriteLine($"Error: {error}");
                        }
                        
                        Console.WriteLine("Extraction completed!");  // 显示提取完成消息
                    }
                    else
                    {
                        Console.WriteLine("Error: Failed to start the extraction process.");  // 显示启动失败消息
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting extraction: {ex.Message}");  // 显示异常信息
            }
            
            Console.WriteLine("Press any key to continue...");  // 提示用户按任意键继续
            Console.ReadKey();  // 等待用户按键
        }
    }
}