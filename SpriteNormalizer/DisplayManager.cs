using System;
using System.Collections.Generic;

namespace SpriteNormalizer
{
    internal static class DisplayManager
    {
        /// <summary>
        /// Hiển thị thông báo nếu file cấu hình không tìm thấy.
        /// </summary>
        public static void ShowConfigFileNotFound(string filePath)
        {
            Logger.LogError($"Configuration file not found: {filePath}");
        }

        /// <summary>
        /// Hiển thị thông báo nếu không có thư mục hợp lệ.
        /// </summary>
        public static void ShowNoValidFolders()
        {
            Logger.LogWarning("No valid folders found. Exiting.");
        }

        /// <summary>
        /// Lấy input từ người dùng với thông báo tùy chỉnh.
        /// </summary>
        public static string GetUserInput(string message)
        {
            Logger.LogInfo(message);
            return Console.ReadLine();
        }

        /// <summary>
        /// Hiển thị thông báo nếu đường dẫn không hợp lệ.
        /// </summary>
        public static void ShowInvalidDirectory(string folderPath)
        {
            Logger.LogError($"Error: The specified directory does not exist: {folderPath}");
        }

        /// <summary>
        /// Hiển thị kết quả kiểm tra thư mục.
        /// </summary>
        public static void ShowFolderCheckResults(FileCheckerResult result)
        {
            Logger.LogInfo("\nFolder check results:");

            ShowMissingFolders(result.MissingFolders);
            ShowExtraFolders(result.ExtraFolders);

            Logger.LogSuccess("\nFolder check complete.");
        }

        /// <summary>
        /// Hiển thị danh sách thư mục bị thiếu.
        /// </summary>
        private static void ShowMissingFolders(List<string> missingFolders)
        {
            if (missingFolders.Count > 0)
            {
                Logger.LogWarning("\nMissing folders:");
                foreach (var folder in missingFolders)
                {
                    Logger.LogInfo($"  - {folder}");
                }
            }
            else
            {
                Logger.LogSuccess("\nNo missing folders.");
            }
        }

        /// <summary>
        /// Hiển thị danh sách thư mục dư thừa.
        /// </summary>
        private static void ShowExtraFolders(List<string> extraFolders)
        {
            if (extraFolders.Count > 0)
            {
                Logger.LogWarning("\nExtra folders:");
                foreach (var folder in extraFolders)
                {
                    Logger.LogInfo($"  - {folder}");
                }
            }
            else
            {
                Logger.LogSuccess("\nNo extra folders.");
            }
        }

        /// <summary>
        /// Hỏi người dùng có muốn xoá folder dư thừa không.
        /// </summary>
        public static bool ConfirmFolderDeletion()
        {
            Logger.LogInfo("\nDo you want to delete extra folders? (yes/no): ");
            string input = Console.ReadLine().Trim().ToLower();
            return input == "yes";
        }

        /// <summary>
        /// Hiển thị thông báo nếu người dùng chọn không xoá folder.
        /// </summary>
        public static void ShowSkippedFolderDeletion()
        {
            Logger.LogInfo("Skipped folder deletion.");
        }
        public static void ShowSpriteRenameResults(SpriteCheckResult result)
        {
            Logger.LogInfo("\nSprite rename results:");

            if (result.MissingFiles.Count > 0)
            {
                Logger.LogWarning("\nMissing files:");
                foreach (var file in result.MissingFiles)
                {
                    Logger.LogInfo($"  - {file}");
                }
            }
            else
            {
                Logger.LogSuccess("\nNo missing files.");
            }

            if (result.InvalidFiles.Count > 0)
            {
                Logger.LogWarning("\nInvalid files:");
                foreach (var file in result.InvalidFiles)
                {
                    Logger.LogInfo($"  - {file}");
                }
            }
            else
            {
                Logger.LogSuccess("\nNo invalid files.");
            }
        }

        /// <summary>
        /// Hiển thị danh sách file bị thiếu.
        /// </summary>
        private static void ShowMissingFiles(List<string> missingFiles)
        {
            if (missingFiles.Count > 0)
            {
                Logger.LogWarning("\nMissing files:");
                foreach (var file in missingFiles)
                {
                    Logger.LogInfo($"  - {file}");
                }
            }
            else
            {
                Logger.LogSuccess("\nNo missing files.");
            }
        }

        /// <summary>
        /// Hiển thị danh sách file sai định dạng.
        /// </summary>
        private static void ShowInvalidFiles(List<string> invalidFiles)
        {
            if (invalidFiles.Count > 0)
            {
                Logger.LogWarning("\nInvalid files:");
                foreach (var file in invalidFiles)
                {
                    Logger.LogInfo($"  - {file}");
                }
            }
            else
            {
                Logger.LogSuccess("\nNo invalid files.");
            }
        }

        /// <summary>
        /// Hỏi người dùng có muốn đổi tên file không.
        /// </summary>
        public static bool ConfirmRenameFiles()
        {
            Logger.LogInfo("\nDo you want to rename the files? (yes/no): ");
            string input = Console.ReadLine().Trim().ToLower();
            return input == "yes";
        }

        /// <summary>
        /// Hiển thị thông báo nếu người dùng chọn không đổi tên file.
        /// </summary>
        public static void ShowSkippedRename()
        {
            Logger.LogInfo("Skipped renaming files.");
        }
    }
}
