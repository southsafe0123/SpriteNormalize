using System;
using System.Collections.Generic;
using System.IO;

namespace SpriteNormalizer
{
    class Program
    {
        static void Main()
        {
            // ✅ Tự động lấy đường dẫn file TXT từ bin\Debug
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FolderCheckerConfig.txt");
            if (!File.Exists(configFilePath))
            {
                Logger.LogError($"Configuration file not found: {configFilePath}");
                return;
            }

            // ✅ Load valid folders từ file TXT
            HashSet<string> validTopFolders;
            Dictionary<string, HashSet<string>> validSubFolders;
            HashSet<string> ignoredFolders; // Các thư mục hợp lệ nhưng không kiểm tra

            FolderConfigReader.LoadValidFolders(configFilePath, out validTopFolders, out validSubFolders, out ignoredFolders);

            if (validTopFolders.Count == 0)
            {
                Logger.LogWarning("No valid folders found. Exiting.");
                return;
            }

            // ✅ Initialize FileChecker với danh sách từ file TXT
            FileChecker.InitializeValidFolders(validTopFolders, validSubFolders, ignoredFolders);

            Logger.LogInfo("Enter the directory path to check:");
            string folderPath = Console.ReadLine();

            if (!Directory.Exists(folderPath))
            {
                Logger.LogError("Error: The specified directory does not exist.");
                return;
            }

            // ✅ Chạy kiểm tra thư mục
            FileCheckerResult result = FileChecker.CheckFolders(folderPath);

            // ✅ Hiển thị kết quả kiểm tra thư mục
            DisplayManager.ShowFolderCheckResults(result);

            // ✅ Hỏi người dùng có muốn xoá folder dư thừa không
            if (result.ExtraFolders.Count > 0)
            {
                Logger.LogInfo("\nDo you want to delete extra folders? (yes/no): ");
                string input = Console.ReadLine().Trim().ToLower();

                if (input == "yes")
                {
                    FileCleaner.DeleteExtraFolders(folderPath, result.ExtraFolders);
                }
                else
                {
                    Logger.LogInfo("Skipped folder deletion.");
                }
            }
        }
    }
}
