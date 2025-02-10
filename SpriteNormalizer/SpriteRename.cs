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
            RenameNpcFiles(rootPath, eventName);
        }

        /// ✅ **Đổi tên `skin`, `skin/icon`, `skin/evo`, `skin/evo/icon`**
        private static void RenameSkinFiles(string rootPath, string eventName)
        {
            string skinPath = Path.Combine(rootPath, "skin");
            string skinIconPath = Path.Combine(skinPath, "icon");
            string evoPath = Path.Combine(skinPath, "evo");
            string evoIconPath = Path.Combine(evoPath, "icon");

            if (!Directory.Exists(skinPath) || !Directory.Exists(skinIconPath) ||
                !Directory.Exists(evoPath) || !Directory.Exists(evoIconPath))
            {
                Logger.LogError($"Missing skin directories: {skinPath}, {skinIconPath}, {evoPath}, {evoIconPath}");
                return;
            }

            var skinFiles = GetGroupedFileNames(skinPath);
            var skinIconFiles = GetGroupedFileNames(skinIconPath);
            var evoFiles = GetGroupedFileNames(evoPath);
            var evoIconFiles = GetGroupedFileNames(evoIconPath);

            var validFiles = new HashSet<string>(ValidSkinNames);

            foreach (var skinType in ValidSkinNames)
            {
                int maxIndex = 0;

                // ✅ Đổi tên `skin` & `skin/icon`
                if (skinFiles.ContainsKey(skinType))
                {
                    maxIndex = RenameSkinGroup(skinFiles[skinType], skinPath, eventName, skinType, 0, "skin");
                }
                if (skinIconFiles.ContainsKey(skinType))
                {
                    RenameSkinGroup(skinIconFiles[skinType], skinIconPath, eventName, skinType, 0, "skin/icon");
                }

                // ✅ Đổi tên `skin/evo` trước và **lưu mapping**
                Dictionary<string, string> evoRenameMap = new Dictionary<string, string>();

                if (evoFiles.ContainsKey(skinType))
                {
                    maxIndex = RenameSkinGroupWithMapping(evoFiles[skinType], evoPath, eventName, skinType, maxIndex, "skin/evo", evoRenameMap);
                }

                // ✅ Đồng bộ `skin/evo/icon` theo mapping của `skin/evo`
                if (evoIconFiles.ContainsKey(skinType))
                {
                    RenameEvoIconFiles(evoIconFiles[skinType], evoIconPath, evoRenameMap, "skin/evo/icon");
                }
            }
        }

        /// ✅ **Đổi tên nhóm file trong thư mục skin và các sub-folder**
        private static int RenameSkinGroup(List<string> files, string directory, string eventName, string baseName, int startIndex, string folderName)
        {
            files = files.OrderBy(f => ExtractNumber(f)).ToList();

            int currentIndex = startIndex;

            foreach (var file in files)
            {
                string oldFileName = Path.GetFileName(file);
                string newFileName = $"{eventName}_{CapitalizeFirstLetter(baseName)}_{currentIndex}.png";
                string newPath = Path.Combine(directory, newFileName);

                if (!file.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(file, newPath);
                    Logger.LogInfo($"Renamed in {folderName}: {oldFileName} → {newFileName}");
                }

                currentIndex++;
            }

            return currentIndex; // ✅ Trả về số thứ tự lớn nhất sau khi đổi tên
        }

        /// ✅ **Đổi tên `skin/evo` và lưu mapping (tên cũ → tên mới)**
        private static int RenameSkinGroupWithMapping(List<string> files, string directory, string eventName, string baseName, int startIndex, string folderName, Dictionary<string, string> renameMap)
        {
            files = files.OrderBy(f => ExtractNumber(f)).ToList();

            int currentIndex = startIndex;

            foreach (var file in files)
            {
                string oldFileName = Path.GetFileName(file);
                string newFileName = $"{eventName}_{CapitalizeFirstLetter(baseName)}_{currentIndex}.png";
                string newPath = Path.Combine(directory, newFileName);

                // 🔹 **Lưu mapping để đồng bộ `skin/evo/icon`**
                renameMap[oldFileName] = newFileName;

                if (!file.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(file, newPath);
                    Logger.LogInfo($"Renamed in {folderName}: {oldFileName} → {newFileName}");
                }

                currentIndex++;
            }

            return currentIndex; // ✅ Trả về số thứ tự lớn nhất sau khi đổi tên
        }

        /// ✅ **Đổi tên `skin/evo/icon` theo mapping của `skin/evo`**
        private static void RenameEvoIconFiles(List<string> files, string directory, Dictionary<string, string> renameMap, string folderName)
        {
            foreach (var file in files)
            {
                string oldFileName = Path.GetFileName(file);

                // 🔍 **Tìm tên mới trong mapping**
                if (renameMap.TryGetValue(oldFileName, out string newFileName))
                {
                    string newPath = Path.Combine(directory, newFileName);

                    if (!file.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(file, newPath);
                        Logger.LogInfo($"Renamed in {folderName}: {oldFileName} → {newFileName}");
                    }
                }
                else
                {
                    Logger.LogWarning($"No matching evo file found for {oldFileName} in {folderName}. Skipping...");
                }
            }
        }

        /// ✅ **Lấy danh sách file theo nhóm body, eye, hair,...**
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

        /// ✅ **Lấy tên file gốc (loại bỏ số thứ tự)**
        private static string ExtractBaseName(string fileName) =>
            Regex.Match(fileName, @"^(.*?)[\s_\-]*\(?\d*\)?$").Groups[1].Value.ToLower();

        /// ✅ **Lấy số thứ tự từ file (mặc định là 0 nếu không có số)**
        private static int ExtractNumber(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            var match = Regex.Match(fileName, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        /// ✅ **Viết hoa chữ cái đầu tiên**
        private static string CapitalizeFirstLetter(string input) =>
            char.ToUpper(input[0], CultureInfo.InvariantCulture) + input.Substring(1).ToLower();

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
        /// ✅ Đổi tên file trong Skin/Evo và đồng thời đổi tên file tương ứng trong Skin/Evo/Icon.
        /// </summary>
        private static void RenameEvoAndEvoIconFiles(
            HashSet<string> validFiles,
            Dictionary<string, List<string>> evoFiles,
            Dictionary<string, List<string>> evoIconFiles,
            string evoPath,
            string evoIconPath,
            string eventName,
            Dictionary<string, int> evoIndices)
        {
            Dictionary<string, string> evoRenameMap = new Dictionary<string, string>();

            foreach (var fileGroup in evoFiles)
            {
                if (!validFiles.Contains(fileGroup.Key))
                {
                    Logger.LogWarning($"Skipping rename in skin/evo: {fileGroup.Key} (Missing or Invalid)");
                    continue;
                }

                string baseName = CapitalizeFirstLetter(fileGroup.Key);
                var sortedEvoFiles = fileGroup.Value.OrderBy(f => ExtractNumber(Path.GetFileNameWithoutExtension(f))).ToList();

                int currentIndex = evoIndices[fileGroup.Key];

                for (int i = 0; i < sortedEvoFiles.Count; i++)
                {
                    string oldEvoPath = sortedEvoFiles[i];
                    string newEvoFileName = $"{eventName}_{baseName}_{currentIndex}.png";
                    string newEvoPath = Path.Combine(evoPath, newEvoFileName);

                    evoRenameMap[Path.GetFileName(oldEvoPath)] = newEvoFileName;

                    if (!oldEvoPath.Equals(newEvoPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(oldEvoPath, newEvoPath);
                        Logger.LogInfo($"Renamed in skin/evo: {Path.GetFileName(oldEvoPath)} → {newEvoFileName}");
                    }

                    currentIndex++;
                }

                evoIndices[fileGroup.Key] = currentIndex;
            }

            RenameEvoIconFiles(evoIconFiles, evoIconPath, eventName, "skin/evo/icon", evoRenameMap);
        }
        /// <summary>
        /// ✅ Đổi tên file trong Skin/Evo/Icon, đảm bảo ánh xạ chính xác với Skin/Evo.
        /// </summary>
        private static void RenameEvoIconFiles(
            Dictionary<string, List<string>> evoIconFiles,
            string evoIconPath,
            string eventName,
            string folderName,
            Dictionary<string, string> evoRenameMap)
        {
            foreach (var fileGroup in evoIconFiles)
            {
                foreach (var file in fileGroup.Value)
                {
                    string originalFileName = Path.GetFileName(file);

                    if (evoRenameMap.TryGetValue(originalFileName, out string newFileName))
                    {
                        string newIconPath = Path.Combine(evoIconPath, newFileName);

                        if (!file.Equals(newIconPath, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Move(file, newIconPath);
                            Logger.LogInfo($"Renamed in {folderName}: {originalFileName} → {newFileName}");
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"No matching evo file found for {originalFileName} in skin/evo/icon. Skipping...");
                    }
                }
            }
        }

        /// <summary>
        /// Đổi tên file trong Skin & Skin/Evo, **đảm bảo số thứ tự đồng bộ**.
        /// </summary>
        private static Dictionary<string, int> RenameSkinGroup(
            HashSet<string> validFiles,
            Dictionary<string, List<string>> fileGroups,
            string directory,
            string eventName,
            string folderName,
            Dictionary<string, int> startIndices)
        {
            Dictionary<string, int> updatedIndices = new Dictionary<string, int>(startIndices);

            foreach (var fileGroup in fileGroups)
            {
                if (!validFiles.Contains(fileGroup.Key))
                {
                    Logger.LogWarning($"Skipping rename in {folderName}: {fileGroup.Key} (Missing or Invalid)");
                    continue;
                }

                string baseName = CapitalizeFirstLetter(fileGroup.Key);
                var sortedFiles = fileGroup.Value.OrderBy(f => ExtractNumber(Path.GetFileNameWithoutExtension(f))).ToList();

                if (!updatedIndices.ContainsKey(fileGroup.Key))
                    updatedIndices[fileGroup.Key] = 0;

                int currentIndex = updatedIndices[fileGroup.Key];

                for (int i = 0; i < sortedFiles.Count; i++)
                {
                    RenameFile(sortedFiles[i], directory, eventName, baseName, currentIndex, folderName);
                    currentIndex++;
                }

                updatedIndices[fileGroup.Key] = currentIndex;
            }

            return updatedIndices;
        }

        /// <summary>
        /// ✅ Đổi tên file trong Skin/Evo/Icon **ĐẢM BẢO KHỚP VỚI Skin/Evo**.
        /// </summary>
        private static void RenameSkinIconGroup(
            HashSet<string> validFiles,
            Dictionary<string, List<string>> fileGroups,
            string directory,
            string eventName,
            string folderName,
            Dictionary<string, int> evoIndices)
        {
            foreach (var fileGroup in fileGroups)
            {
                if (!validFiles.Contains(fileGroup.Key))
                {
                    Logger.LogWarning($"Skipping rename in {folderName}: {fileGroup.Key} (Missing or Invalid)");
                    continue;
                }

                string baseName = CapitalizeFirstLetter(fileGroup.Key);
                var sortedFiles = fileGroup.Value.OrderBy(f => ExtractNumber(Path.GetFileNameWithoutExtension(f))).ToList();

                if (!evoIndices.ContainsKey(fileGroup.Key))
                {
                    Logger.LogError($"Evo file {fileGroup.Key} is missing reference index! Skipping...");
                    continue;
                }

                int currentIndex = evoIndices[fileGroup.Key];

                for (int i = 0; i < sortedFiles.Count; i++)
                {
                    RenameFile(sortedFiles[i], directory, eventName, baseName, currentIndex, folderName);
                    currentIndex++;
                }
            }
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

      
        
        private static HashSet<string> GetValidFilesForRename(Dictionary<string, List<string>> mainFiles, Dictionary<string, List<string>> iconFiles, string[] validNames)
        {
            return new HashSet<string>(validNames.Where(name => mainFiles.ContainsKey(name) && iconFiles.ContainsKey(name)));
        }

      

        /// <summary>
        /// ✅ Đổi tên file theo đúng định dạng [EventName] Item_[Index].png
        /// </summary>
        private static void RenameFile(string oldPath, string directory, string eventName, string baseName, int index, string folderName)
        {
            string newFileName;

            // ✅ Nếu là ingredient thì bỏ qua baseName
            if (folderName == "ingredient")
                newFileName = $"{eventName} Item_{index}.png";
            else
                newFileName = $"{eventName}_{baseName}_{index}.png";

            string newPath = Path.Combine(directory, newFileName);

            if (!oldPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
            {
                File.Move(oldPath, newPath);
                Logger.LogInfo($"Renamed in {folderName}: {Path.GetFileName(oldPath)} → {newFileName}");
            }
        }
        /// <summary>
        /// Đổi tên file trong thư mục npc theo định dạng [tên gốc]_[eventName].png
        /// </summary>
        private static void RenameNpcFiles(string rootPath, string eventName)
        {
            string npcPath = Path.Combine(rootPath, "npc");

            if (!Directory.Exists(npcPath))
            {
                Logger.LogError($"Missing directory: {npcPath}");
                return;
            }

            var npcFiles = Directory.GetFiles(npcPath, "*.png").ToList();

            foreach (var file in npcFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);
                string newFileName = $"{fileName}_{eventName}{extension}";
                string newPath = Path.Combine(npcPath, newFileName);

                if (!file.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(file, newPath);
                    Logger.LogInfo($"Renamed in npc: {Path.GetFileName(file)} → {newFileName}");
                }
            }
        }

       
    }
}
