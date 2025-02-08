using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpriteNormalizer
{
    internal static class SpriteChecker
    {
        private static readonly Dictionary<string, string[]> ValidFileNames = new Dictionary<string, string[]>
        {
            { "equipment", new string[] { "weapon", "back", "boot", "cloth", "helmet" } },
            { "pet", new string[] { "pet" } },
            { "skin", new string[] { "body", "eye", "hair", "facehair" } },
            { "skin/evo", new string[] { "body", "eye", "hair", "facehair" } }
        };

        private static readonly string[] ValidNpcNames = { "back", "cloth", "helmet", "boot" }; // Weapon là tùy chọn

        /// <summary>
        /// Kiểm tra file PNG trong các thư mục equipment, pet, skin, skin/evo và npc.
        /// </summary>
        public static SpriteCheckResult CheckSpriteNames(string rootPath)
        {
            var missingFiles = new HashSet<string>();
            var invalidFiles = new List<string>();

            // ✅ Kiểm tra Equipment, Pet, Skin, Skin/Evo
            foreach (var folder in ValidFileNames.Keys)
            {
                CheckFolderPair(rootPath, folder, $"{folder}/icon", ValidFileNames[folder], missingFiles, invalidFiles);
            }

            // ✅ Kiểm tra NPC riêng biệt
            CheckNpcFolder(rootPath, missingFiles, invalidFiles);

            return new SpriteCheckResult(missingFiles.ToList(), invalidFiles);
        }

        /// <summary>
        /// Kiểm tra file giữa thư mục chính và thư mục icon tương ứng.
        /// </summary>
        private static void CheckFolderPair(string rootPath, string mainFolder, string iconFolder, string[] validNames, HashSet<string> missingFiles, List<string> invalidFiles)
        {
            string mainPath = Path.Combine(rootPath, mainFolder);
            string iconPath = Path.Combine(rootPath, iconFolder);

            if (!Directory.Exists(mainPath) || !Directory.Exists(iconPath))
            {
                Logger.LogError($"Missing directory: {mainPath} or {iconPath}");
                return;
            }

            var mainFiles = GetFileNames(mainPath);
            var iconFiles = GetFileNames(iconPath);

            // ✅ Kiểm tra file bị thiếu
            CheckMissingFiles(mainFiles, iconFiles, mainFolder, iconFolder, validNames, missingFiles);
            CheckMissingFiles(iconFiles, mainFiles, iconFolder, mainFolder, validNames, missingFiles);

            // ✅ Kiểm tra file sai tên
            CheckInvalidFileNames(mainFiles, mainFolder, validNames, invalidFiles);
            CheckInvalidFileNames(iconFiles, iconFolder, validNames, invalidFiles);

            // ✅ Kiểm tra file gốc phải tồn tại
            CheckEssentialFiles(mainFiles, mainFolder, validNames, missingFiles);
            CheckEssentialFiles(iconFiles, iconFolder, validNames, missingFiles);
        }

        /// <summary>
        /// Kiểm tra thư mục NPC.
        /// </summary>
        private static void CheckNpcFolder(string rootPath, HashSet<string> missingFiles, List<string> invalidFiles)
        {
            string npcPath = Path.Combine(rootPath, "npc");

            if (!Directory.Exists(npcPath))
            {
                Logger.LogError($"Missing directory: {npcPath}");
                return;
            }

            var npcFiles = GetFileNames(npcPath);

            // ✅ Kiểm tra file bị thiếu (ngoại trừ weapon)
            CheckEssentialFiles(npcFiles, "npc", ValidNpcNames, missingFiles);

            // ✅ Kiểm tra file sai tên (cho phép weapon)
            CheckInvalidFileNames(npcFiles, "npc", ValidNpcNames.Concat(new[] { "weapon" }).ToArray(), invalidFiles);
        }

        /// <summary>
        /// Lấy danh sách file PNG trong một thư mục.
        /// </summary>
        private static HashSet<string> GetFileNames(string directory)
        {
            return Directory.GetFiles(directory, "*.png")
                .Select(f => Path.GetFileName(f).ToLower()) // Không phân biệt chữ hoa/thường
                .ToHashSet();
        }

        /// <summary>
        /// Kiểm tra file bị thiếu giữa hai thư mục.
        /// </summary>
        private static void CheckMissingFiles(HashSet<string> source, HashSet<string> target, string sourceFolder, string targetFolder, string[] validNames, HashSet<string> missingFiles)
        {
            foreach (var file in source)
            {
                if (!target.Contains(file) && !IsValidFileName(file, validNames))
                {
                    missingFiles.Add($"Missing in {targetFolder}: {file}.png");
                }
            }
        }

        /// <summary>
        /// Kiểm tra file có tên không hợp lệ.
        /// </summary>
        private static void CheckInvalidFileNames(HashSet<string> files, string folder, string[] validNames, List<string> invalidFiles)
        {
            foreach (var file in files)
            {
                if (!IsValidFileName(file, validNames))
                {
                    invalidFiles.Add($"Invalid file in {folder}: {file}");
                }
            }
        }

        /// <summary>
        /// Kiểm tra xem có đủ file gốc không.
        /// </summary>
        private static void CheckEssentialFiles(HashSet<string> files, string folder, string[] validNames, HashSet<string> missingFiles)
        {
            foreach (var validName in validNames)
            {
                if (!files.Any(f => f.StartsWith(validName)))
                {
                    missingFiles.Add($"Missing in {folder}: {validName}.png");
                }
            }
        }

        /// <summary>
        /// Xác định xem file có tên hợp lệ hay không.
        /// </summary>
        private static bool IsValidFileName(string fileName, string[] validNames)
        {
            string baseName = NormalizeFileName(fileName);
            return validNames.Any(valid => baseName.StartsWith(valid));
        }

        /// <summary>
        /// Chuẩn hóa tên file để nhận diện chính xác.
        /// </summary>
        private static string NormalizeFileName(string fileName)
        {
            string baseName = Path.GetFileNameWithoutExtension(fileName).ToLower();
            var match = Regex.Match(baseName, @"^(.*?)[\s_\-]*\(?(\d*)\)?$");
            string namePart = match.Groups[1].Value.Trim();
            string numberPart = match.Groups[2].Success && match.Groups[2].Value != "" ? $"({match.Groups[2].Value})" : "";

            return $"{namePart}{numberPart}";
        }
    }

    /// <summary>
    /// Kết quả kiểm tra file PNG.
    /// </summary>
    internal class SpriteCheckResult
    {
        public List<string> MissingFiles { get; }
        public List<string> InvalidFiles { get; }

        public SpriteCheckResult(List<string> missingFiles, List<string> invalidFiles)
        {
            MissingFiles = missingFiles;
            InvalidFiles = invalidFiles;
        }
    }
}
