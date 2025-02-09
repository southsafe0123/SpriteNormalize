using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SpriteNormalizer
{
    internal static class SpriteRename
    {
        private static readonly Dictionary<string, string[]> ValidFolders = new Dictionary<string, string[]>
        {
            { "equipment", new string[] { "weapon", "back", "boot", "cloth", "helmet" } },
            { "pet", new string[] { "pet" } }
        };

        private static readonly string[] ValidSkinNames = { "body", "eye", "hair", "facehair" };

        /// <summary>
        /// Đổi tên các file PNG trong equipment, pet, skin và ingredient.
        /// </summary>
        public static void RenameSprites(string rootPath, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                Logger.LogError("Event name is missing or invalid.");
                return;
            }

            RenameEquipmentAndPet(rootPath, eventName);
            RenameSkinFiles(rootPath, eventName);
            RenameIngredientFiles(rootPath, eventName);
        }

        /// <summary>
        /// ✅ Đổi tên file trong ingredient theo thứ tự tăng dần.
        /// </summary>
        private static void RenameIngredientFiles(string rootPath, string eventName)
        {
            string ingredientPath = Path.Combine(rootPath, "ingredient");

            if (!Directory.Exists(ingredientPath))
            {
                Logger.LogError($"Missing directory: {ingredientPath}");
                return;
            }

            // ✅ Lấy danh sách file PNG và sắp xếp theo số thứ tự nếu có
            var ingredientFiles = Directory.GetFiles(ingredientPath, "*.png")
                                           .OrderBy(f => ExtractNumber(f))
                                           .ToList();

            // ✅ Đổi tên toàn bộ file trong ingredient
            for (int i = 0; i < ingredientFiles.Count; i++)
            {
                RenameFile(ingredientFiles[i], ingredientPath, eventName, "Item", i, "ingredient");
            }
        }

        /// <summary>
        /// Đổi tên Equipment và Pet.
        /// </summary>
        private static void RenameEquipmentAndPet(string rootPath, string eventName)
        {
            foreach (var folder in ValidFolders.Keys)
            {
                string mainPath = Path.Combine(rootPath, folder);
                string iconPath = Path.Combine(mainPath, "icon");

                if (!Directory.Exists(mainPath) || !Directory.Exists(iconPath))
                {
                    Logger.LogError($"Missing directory: {mainPath} or {iconPath}");
                    continue;
                }

                var mainFiles = GetGroupedFileNames(mainPath);
                var iconFiles = GetGroupedFileNames(iconPath);
                var validFiles = GetValidFilesForRename(mainFiles, iconFiles, ValidFolders[folder]);

                RenameFiles(validFiles, mainFiles, mainPath, eventName, folder);
                RenameFiles(validFiles, iconFiles, iconPath, eventName, $"{folder}/icon");
            }
        }

        /// <summary>
        /// Đổi tên Skin và Skin/Evo.
        /// </summary>
        private static void RenameSkinFiles(string rootPath, string eventName)
        {
            string skinPath = Path.Combine(rootPath, "skin");
            string skinIconPath = Path.Combine(skinPath, "icon");
            string evoPath = Path.Combine(skinPath, "evo");
            string evoIconPath = Path.Combine(evoPath, "icon");

            if (!Directory.Exists(skinPath) || !Directory.Exists(skinIconPath) || !Directory.Exists(evoPath) || !Directory.Exists(evoIconPath))
            {
                Logger.LogError($"Missing skin directories: {skinPath}, {skinIconPath}, {evoPath}, {evoIconPath}");
                return;
            }

            var skinFiles = GetGroupedFileNames(skinPath);
            var skinIconFiles = GetGroupedFileNames(skinIconPath);
            var evoFiles = GetGroupedFileNames(evoPath);
            var evoIconFiles = GetGroupedFileNames(evoIconPath);

            var validFiles = GetValidFilesForRename(skinFiles, skinIconFiles, ValidSkinNames);

            RenameSkinGroup(validFiles, skinFiles, skinPath, eventName, "skin", 0);
            RenameSkinGroup(validFiles, skinIconFiles, skinIconPath, eventName, "skin/icon", 0);

            int maxIndex = skinFiles.Values.SelectMany(f => f).Select(f => ExtractNumber(f)).DefaultIfEmpty(0).Max() + 1;

            RenameSkinGroup(validFiles, evoFiles, evoPath, eventName, "skin/evo", maxIndex);
            RenameSkinGroup(validFiles, evoIconFiles, evoIconPath, eventName, "skin/evo/icon", maxIndex);
        }

        /// <summary>
        /// ✅ Đổi tên file trong Equipment & Pet
        /// </summary>
        private static void RenameFiles(HashSet<string> validFiles, Dictionary<string, List<string>> fileGroups, string directory, string eventName, string folderName)
        {
            foreach (var fileGroup in fileGroups)
            {
                if (!validFiles.Contains(fileGroup.Key))
                {
                    Logger.LogWarning($"Skipping rename in {folderName}: {fileGroup.Key} (Missing or Invalid)");
                    continue;
                }

                string baseName = CapitalizeFirstLetter(fileGroup.Key);
                var sortedFiles = fileGroup.Value.OrderBy(f => ExtractNumber(f)).ToList();

                for (int i = 0; i < sortedFiles.Count; i++)
                {
                    RenameFile(sortedFiles[i], directory, eventName, baseName, i, folderName);
                }
            }
        }

        /// <summary>
        /// ✅ Đổi tên file trong Skin & Skin/Evo
        /// </summary>
        private static void RenameSkinGroup(HashSet<string> validFiles, Dictionary<string, List<string>> fileGroups, string directory, string eventName, string folderName, int startIndex)
        {
            foreach (var fileGroup in fileGroups)
            {
                if (!validFiles.Contains(fileGroup.Key))
                {
                    Logger.LogWarning($"Skipping rename in {folderName}: {fileGroup.Key} (Missing or Invalid)");
                    continue;
                }

                string baseName = CapitalizeFirstLetter(fileGroup.Key);
                var sortedFiles = fileGroup.Value.OrderBy(f => ExtractNumber(f)).ToList();

                for (int i = 0; i < sortedFiles.Count; i++)
                {
                    RenameFile(sortedFiles[i], directory, eventName, baseName, startIndex + i, folderName);
                }
            }
        }
        private static Dictionary<string, List<string>> GetGroupedFileNames(string directory)
        {
            var files = Directory.GetFiles(directory, "*.png");
            var groupedFiles = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string baseName = ExtractBaseName(fileName);

                if (!groupedFiles.ContainsKey(baseName))
                {
                    groupedFiles[baseName] = new List<string>();
                }
                groupedFiles[baseName].Add(file);
            }

            return groupedFiles;
        }

        private static HashSet<string> GetValidFilesForRename(Dictionary<string, List<string>> mainFiles, Dictionary<string, List<string>> iconFiles, string[] validNames)
        {
            return new HashSet<string>(validNames.Where(name => mainFiles.ContainsKey(name) && iconFiles.ContainsKey(name)));
        }

        private static string ExtractBaseName(string fileName) => Regex.Match(fileName, @"^(.*?)[\s_\-]*\(?\d*\)?$").Groups[1].Value.ToLower();

        private static int ExtractNumber(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            var match = Regex.Match(fileName, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }


        /// <summary>
        /// ✅ Đổi tên file theo đúng định dạng [EventName] Item_[Index].png
        /// </summary>
        private static void RenameFile(string oldPath, string directory, string eventName, string baseName, int index, string folderName)
        {
            string newFileName = $"{eventName} Item_{index}.png";
            string newPath = Path.Combine(directory, newFileName);

            if (!oldPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
            {
                File.Move(oldPath, newPath);
                Logger.LogInfo($"Renamed in {folderName}: {Path.GetFileName(oldPath)} → {newFileName}");
            }
        }


        private static string CapitalizeFirstLetter(string input) => char.ToUpper(input[0], CultureInfo.InvariantCulture) + input.Substring(1).ToLower();
    }
}
