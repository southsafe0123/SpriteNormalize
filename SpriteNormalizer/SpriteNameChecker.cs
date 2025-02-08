using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpriteNormalizer
{
    internal static class SpriteNameChecker
    {
        private static readonly string[] ValidEquipmentNames = { "weapon", "back", "boot", "cloth", "helmet" };
        private static readonly string[] ValidPetNames = { "pet" };
        private static readonly string[] ValidSkinNames = { "body", "eye", "hair", "facehair" };

        /// <summary>
        /// Kiểm tra file PNG trong các thư mục equipment, pet, skin và skin/evo.
        /// </summary>
        public static SpriteCheckResult CheckSpriteNames(string rootPath)
        {
            var missingFiles = new HashSet<string>(); // ✅ Tránh lặp lại thông báo thiếu file
            var invalidFiles = new List<string>();

            // ✅ Kiểm tra Equipment
            CheckFolderPair(rootPath, "equipment", "equipment/icon", ValidEquipmentNames, missingFiles, invalidFiles);

            // ✅ Kiểm tra Pet
            CheckFolderPair(rootPath, "pet", "pet/icon", ValidPetNames, missingFiles, invalidFiles);

            // ✅ Kiểm tra Skin
            CheckFolderPair(rootPath, "skin", "skin/icon", ValidSkinNames, missingFiles, invalidFiles);

            // ✅ Kiểm tra Skin/Evo
            CheckFolderPair(rootPath, "skin/evo", "skin/evo/icon", ValidSkinNames, missingFiles, invalidFiles);

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

            var mainFiles = GetNormalizedFileNames(mainPath);
            var iconFiles = GetNormalizedFileNames(iconPath);

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
        /// Lấy danh sách file PNG trong một thư mục, chuẩn hóa tên file để nhận diện chính xác.
        /// </summary>
        private static Dictionary<string, string> GetNormalizedFileNames(string directory)
        {
            return Directory.GetFiles(directory, "*.png")
                .Select(f => new { Original = f, Normalized = NormalizeFileName(Path.GetFileNameWithoutExtension(f)) })
                .ToDictionary(x => x.Normalized, x => x.Original, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra file bị thiếu giữa hai thư mục.
        /// </summary>
        private static void CheckMissingFiles(Dictionary<string, string> source, Dictionary<string, string> target, string sourceFolder, string targetFolder, string[] validNames, HashSet<string> missingFiles)
        {
            foreach (var file in source.Keys)
            {
                if (!target.ContainsKey(file) && !IsInvalidFileName(file, validNames))
                {
                    missingFiles.Add($"Missing in {targetFolder}: {file}.png");
                }
            }
        }

        /// <summary>
        /// Kiểm tra file có tên không hợp lệ.
        /// </summary>
        private static void CheckInvalidFileNames(Dictionary<string, string> files, string folder, string[] validNames, List<string> invalidFiles)
        {
            foreach (var file in files.Keys)
            {
                if (IsInvalidFileName(file, validNames))
                {
                    invalidFiles.Add($"Invalid file in {folder}: {file}.png");
                }
            }
        }

        /// <summary>
        /// Kiểm tra xem có đủ file gốc không.
        /// </summary>
        private static void CheckEssentialFiles(Dictionary<string, string> files, string folder, string[] validNames, HashSet<string> missingFiles)
        {
            foreach (var validName in validNames)
            {
                if (!files.Keys.Any(f => f.StartsWith(validName)))
                {
                    missingFiles.Add($"Missing in {folder}: {validName}.png");
                }
            }
        }

        /// <summary>
        /// Xác định xem file có tên hợp lệ hay không.
        /// </summary>
        private static bool IsInvalidFileName(string fileName, string[] validNames)
        {
            string baseName = NormalizeFileName(fileName);

            if (!validNames.Any(valid => baseName.StartsWith(valid)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Chuẩn hóa tên file để nhận diện chính xác.
        /// </summary>
        private static string NormalizeFileName(string fileName)
        {
            string baseName = Path.GetFileNameWithoutExtension(fileName).ToLower();

            // Chuẩn hóa tên file để nhận diện các số đánh dấu giống nhau
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
