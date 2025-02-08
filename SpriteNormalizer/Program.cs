using System;
using System.Collections.Generic;
using System.IO;

namespace SpriteNormalizer
{
    class Program
    {
        static void Main()
        {
            // ✅ Lấy dữ liệu folder cần ignore
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FolderCheckerConfig.txt");
            if (!File.Exists(configFilePath))
            {
                DisplayManager.ShowConfigFileNotFound(configFilePath);
                return;
            }
            // ✅ Tự động lấy đường dẫn file EventName.txt
            string eventFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventName.txt");

            

            // ✅ Load valid folders từ file TXT
            HashSet<string> validTopFolders;
            Dictionary<string, HashSet<string>> validSubFolders;
            HashSet<string> ignoredFolders;

            FolderConfigReader.LoadValidFolders(configFilePath, out validTopFolders, out validSubFolders, out ignoredFolders);

            if (validTopFolders.Count == 0)
            {
                DisplayManager.ShowNoValidFolders();
                return;
            }

            // ✅ Initialize FileChecker với danh sách từ file TXT
            FileChecker.InitializeValidFolders(validTopFolders, validSubFolders, ignoredFolders);

            // ✅ Nhập đường dẫn cần kiểm tra
            //string folderPath = DisplayManager.GetUserInput("Enter the directory path to check:");
            string folderPath = @"E:\Anntest\Birthday";

            if (!Directory.Exists(folderPath))
            {
                DisplayManager.ShowInvalidDirectory(folderPath);
                return;
            }

            // ✅ Chạy kiểm tra thư mục
            FileCheckerResult result = FileChecker.CheckFolders(folderPath);

            // ✅ Hiển thị kết quả kiểm tra thư mục
            DisplayManager.ShowFolderCheckResults(result);

            // ✅ Hỏi người dùng có muốn xoá folder dư thừa không
            //if (result.ExtraFolders.Count > 0)
            //{
            //    if (DisplayManager.ConfirmFolderDeletion())
            //    {
            //        FileCleaner.DeleteExtraFolders(folderPath, result.ExtraFolders);
            //    }
            //    else
            //    {
            //        DisplayManager.ShowSkippedFolderDeletion();
            //    }
            //}

            string eventName;
            FolderConfigReader.LoadEventName(eventFilePath, out eventName);

            // ✅ Chạy kiểm tra sprite (tên file trong equipment & icon)
            SpriteCheckResult spriteCheckResult = SpriteNameChecker.CheckSpriteNames(folderPath);

            // ✅ Hiển thị kết quả kiểm tra tên sprite
            DisplayManager.ShowSpriteRenameResults(spriteCheckResult);
        }
    }
}
