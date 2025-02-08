using System;
using System.Collections.Generic;

namespace SpriteNormalizer
{
    internal static class DisplayManager
    {
        /// <summary>
        /// Hiển thị kết quả kiểm tra thư mục
        /// </summary>
        public static void ShowFolderCheckResults(FileCheckerResult result)
        {
            Logger.LogInfo("\nFolder check results:");

            ShowMissingFolders(result.MissingFolders);
            ShowExtraFolders(result.ExtraFolders);

            Logger.LogSuccess("\nFolder check complete.");
        }

        /// <summary>
        /// Hiển thị danh sách thư mục bị thiếu
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
        /// Hiển thị danh sách thư mục dư thừa
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
    }
}
