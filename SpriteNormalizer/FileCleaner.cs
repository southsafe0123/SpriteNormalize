using System;
using System.IO;
using System.Collections.Generic;

namespace SpriteNormalizer
{
    internal static class FileCleaner
    {
        /// <summary>
        /// Deletes extra folders except 'element' at the top level.
        /// </summary>
        public static void DeleteExtraFolders(string rootPath, List<string> extraFolders)
        {
            if (extraFolders.Count == 0)
            {
                Logger.LogInfo("No extra folders to delete.");
                return;
            }

            foreach (var folder in extraFolders)
            {
                string folderPath = Path.Combine(rootPath, folder);

                // Skip 'element' if it's at the top level
                if (folder.Equals("element", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogWarning($"Skipping: {folderPath} (Protected)");
                    continue;
                }

                try
                {
                    Directory.Delete(folderPath, true); // Xoá folder và toàn bộ nội dung bên trong
                    Logger.LogSuccess($"Deleted: {folderPath}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error deleting {folderPath}: {ex.Message}");
                }
            }
        }
    }
}
