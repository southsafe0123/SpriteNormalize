using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SpriteNormalizer
{
    internal static class FileChecker
    {
        private static HashSet<string> validTopFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, HashSet<string>> validSubFolders = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        private static HashSet<string> ignoredFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static void InitializeValidFolders(
            HashSet<string> topFolders,
            Dictionary<string, HashSet<string>> subFolders,
            HashSet<string> ignored)
        {
            validTopFolders = topFolders;
            validSubFolders = subFolders;
            ignoredFolders = ignored;
        }

        public static FileCheckerResult CheckFolders(string rootPath)
        {
            var missingFolders = new List<string>();
            var extraFolders = new List<string>();

            if (!Directory.Exists(rootPath))
            {
                Console.WriteLine("Error: Directory does not exist.");
                return new FileCheckerResult(missingFolders, extraFolders);
            }

            var allValidFolders = new HashSet<string>(validTopFolders, StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in validSubFolders)
            {
                foreach (var sub in kvp.Value)
                {
                    allValidFolders.Add($"{kvp.Key}/{sub}");
                }
            }

            var existingFolders = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories)
                                           .Select(f => f.Substring(rootPath.Length).TrimStart(Path.DirectorySeparatorChar)
                                                         .Replace("\\", "/"))
                                           .ToList();

            // ✅ Kiểm tra folder bị thiếu, bỏ qua ignoredFolders
            foreach (var validFolder in allValidFolders)
            {
                if (!ignoredFolders.Contains(validFolder))
                {
                    string fullPath = Path.Combine(rootPath, validFolder.Replace("/", "\\"));
                    if (!Directory.Exists(fullPath))
                    {
                        missingFolders.Add(validFolder);
                    }
                }
            }

            HashSet<string> skippedPaths = new HashSet<string>();

            foreach (var folder in existingFolders)
            {
                // ✅ Nếu thư mục này nằm trong ignoredFolders, bỏ qua
                if (ignoredFolders.Any(ignored => folder.StartsWith(ignored + "/", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // ✅ Nếu thư mục đã bị đánh dấu là dư thừa, bỏ qua sub-folder của nó
                if (skippedPaths.Any(parent => folder.StartsWith(parent + "/")))
                {
                    continue;
                }

                if (!allValidFolders.Contains(folder) && !ignoredFolders.Contains(folder))
                {
                    extraFolders.Add(folder);
                    skippedPaths.Add(folder);
                }
            }

            return new FileCheckerResult(missingFolders, extraFolders);
        }
    }

    internal class FileCheckerResult
    {
        public List<string> MissingFolders { get; }
        public List<string> ExtraFolders { get; }

        public bool IsAllCorrect()
        {
            return MissingFolders.Count == 0 && ExtraFolders.Count == 0;
        }

        public FileCheckerResult(List<string> missingFolders, List<string> extraFolders)
        {
            MissingFolders = missingFolders;
            ExtraFolders = extraFolders;
        }
    }
}
