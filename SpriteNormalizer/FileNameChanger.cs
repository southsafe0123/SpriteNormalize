using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class FileNameChanger
{
    private Dictionary<string, string> fileNameChangerDict;
    public FileNameChanger(Dictionary<string, string> fileNameChangerDict)
    {
        this.fileNameChangerDict = fileNameChangerDict;
    }
    public Dictionary<string, string> GetFolderCheckerList()
    {
        return this.fileNameChangerDict;
    }
    public void CheckMissingPart(List<string> correctFiles)
    {
        if (correctFiles == null || correctFiles.Count == 0) return;

        List<string> tempFiles = new List<string>(correctFiles);
        string firstDirectory = Path.GetDirectoryName(tempFiles[0]);
        string iconDirectory = Path.Combine(firstDirectory, "Icon");

        List<string> compareADirectory = tempFiles
            .Where(file => string.Equals(Path.GetDirectoryName(file), firstDirectory, StringComparison.OrdinalIgnoreCase))
            .ToList();

        List<string> compareBDirectory = new List<string>();

        if (Directory.Exists(iconDirectory))
        {
            List<string> iconFiles = tempFiles
                .Where(file => string.Equals(Path.GetDirectoryName(file), iconDirectory, StringComparison.OrdinalIgnoreCase))
                .ToList();

            compareBDirectory.AddRange(iconFiles);
        }

        tempFiles.RemoveAll(file => compareADirectory.Contains(file));
        tempFiles.RemoveAll(file => compareBDirectory.Contains(file));

        List<string> missingIcon = new List<string>();
        List<string> missingBaseImage = new List<string>();

        foreach (var fileA in compareADirectory)
        {
            string fileNameA = Path.GetFileName(fileA);
            bool existsInB = compareBDirectory.Any(fileB =>
                string.Equals(Path.GetFileName(fileB), fileNameA, StringComparison.OrdinalIgnoreCase));

            if (!existsInB)
            {
                missingIcon.Add(fileA);
            }
        }

        foreach (var fileB in compareBDirectory)
        {
            string fileNameB = Path.GetFileName(fileB);
            bool existsInA = compareADirectory.Any(fileA =>
                string.Equals(Path.GetFileName(fileA), fileNameB, StringComparison.OrdinalIgnoreCase));

            if (!existsInA)
            {
                missingBaseImage.Add(fileB);
            }
        }

        if (missingIcon.Count > 0)
        {
            Logger.Warning("These File Is Missing Its Own Icon:");
            missingIcon.ForEach(Logger.Log);
        }

        if (missingBaseImage.Count > 0)
        {
            Logger.Warning("These Icons Are Missing Their Own Files:");
            missingBaseImage.ForEach(Logger.Log);
        }

        CheckMissingPart(tempFiles);
    }
    public void CheckFile(string path, out List<string> correctFilesCallBack)
    {
        if (fileNameChangerDict == null)
        {
            Logger.Error("folderCheckerList is null!");
            correctFilesCallBack = null;
            return;
        }

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Couldnt found path {path}");
            correctFilesCallBack = null;
            return;
        }

        Logger.Announce("Checking File...");
        CheckWrongTypeAndName(path, out List<string> correctFiles);
        Logger.Announce("Compare Icon And Base Image...");
        CheckMissingPart(correctFiles);

        correctFilesCallBack = correctFiles;
    }
    void CheckWrongTypeAndName(string path, out List<string> callBackCorrectFiles, bool enableAnnouce = true)
    {

        List<string> allFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .ToList();

        List<string> incorrectFiles = new List<string>();
        List<string> incorrectFormats = new List<string>();
        List<string> correctFiles = new List<string>();

        foreach (string filePath in allFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
            string fileExtension = Path.GetExtension(filePath).ToLower();

            bool containsKey = fileNameChangerDict.Keys
                .Any(key => fileName.Contains(key.ToLower()));

            if (!containsKey)
            {
                incorrectFiles.Add(filePath);
            }
            else
            {
                correctFiles.Add(filePath);
            }

            if (fileExtension != ".png")
            {
                incorrectFormats.Add(filePath);
            }
        }

        callBackCorrectFiles = correctFiles;

        if (!enableAnnouce) return;

        List<string> filteredIncorrectFiles = incorrectFiles
    .Where(file => !file.Contains(@"\Element") && !file.Contains(@"\Ingredient"))
    .ToList();

        if (filteredIncorrectFiles.Count > 0)
        {
            Logger.Warning("Incorrect File Name: ");
            filteredIncorrectFiles.ForEach(Logger.Log);
        }
        else
        {
            Logger.Success("None Incorrect File Name");
        }

        if (incorrectFormats.Count > 0)
        {
            Logger.Warning("Incorrect File Type: ");
            incorrectFormats.ForEach(Logger.Log);
        }
        else
        {
            Logger.Success("None Incorrect File Type");
        }
    }
    public void RenameFiles(List<string> correctFiles, bool enableLog = true)
    {
        if (correctFiles == null || correctFiles.Count == 0) return;

        List<string> tempFiles = new List<string>(correctFiles);
        string firstDirectory = Path.GetDirectoryName(tempFiles[0]);
        string iconDirectory = Path.Combine(firstDirectory, "Icon");

        List<string> currentDirectory = tempFiles
            .Where(file => string.Equals(Path.GetDirectoryName(file), firstDirectory, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (Directory.Exists(iconDirectory))
        {
            List<string> iconFiles = tempFiles
                .Where(file => string.Equals(Path.GetDirectoryName(file), iconDirectory, StringComparison.OrdinalIgnoreCase))
                .ToList();

            currentDirectory.AddRange(iconFiles);
        }

        tempFiles.RemoveAll(file => currentDirectory.Contains(file));

        List<int> idList = new List<int>();
        foreach (string filePath in currentDirectory)
        {
            string directory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath).ToLower(); // Chuyển về chữ thường
            string fileExtension = Path.GetExtension(filePath);

            string baseFileName = fileNameChangerDict.Keys
                .FirstOrDefault(key => fileNameWithoutExtension.Contains(key.ToLower()));

            MatchCollection matches = Regex.Matches(fileNameWithoutExtension, @"\d+");
            List<string> matchList = matches.Cast<Match>().Select(m => m.Value).ToList();
            string stringId = string.Join("", matchList);
            int.TryParse(stringId, out int id);
            idList.Add(id);

            if (baseFileName != null && fileNameChangerDict.TryGetValue(baseFileName, out string newName))
            {
                string newFilePath = Path.Combine(directory, $"{newName}{fileExtension}");

                if (fileNameChangerDict.ContainsKey("eventName"))
                {
                    newFilePath = newFilePath.Replace("E_", $"{fileNameChangerDict["eventName"]}_");
                }

                newFilePath = newFilePath.Replace("_ID", $"_{id}");

                try
                {
                    File.Move(filePath, newFilePath);
                    if (enableLog)
                    {
                        Logger.Log($"Renamed: {filePath} -> {newFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    if (enableLog)
                    {
                        Logger.Log($"Error renaming {filePath}: {ex.Message}");
                    }
                }
            }

        }

        RenameFiles(tempFiles,enableLog);
    }

    public List<string> GetEvoFiles(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            Logger.Error($"Root directory does not exist: {rootPath}");
            return new List<string>();
        }

        List<string> evoFilesList = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(file => Path.GetDirectoryName(file).Split(Path.DirectorySeparatorChar)
                .Any(folder => folder.Equals("Evo", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return evoFilesList;
    }

    public void ReIDEvo(string path)
    {
        List<string> evoFiles = GetEvoFiles(path);

        foreach (string evoFile in evoFiles)
        {
            string evoDirectory = Path.GetDirectoryName(evoFile);
            string evoIconDirectory = Path.Combine(evoDirectory, "Icon");
            string parentDirectory = Directory.GetParent(evoDirectory)?.FullName;

            if (parentDirectory == null) continue;

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(evoFile);
            string fileExtension = Path.GetExtension(evoFile);

            // ✅ Tìm ID trong tên file
            Match match = Regex.Match(fileNameWithoutExt, @"\d+");
            int id = match.Success ? int.Parse(match.Value) : 0;
            string baseName = Regex.Replace(fileNameWithoutExt, @"\d+", "").Trim('_', '-', ' ');

            string newEvoFilePath;
            string newIconFilePath = null;

            do
            {
                // ✅ Tạo tên mới với ID mới
                newEvoFilePath = Path.Combine(evoDirectory, $"{baseName}_{id}{fileExtension}");

                if (Directory.Exists(evoIconDirectory))
                {
                    string iconFilePath = Path.Combine(evoIconDirectory, $"{fileNameWithoutExt}{fileExtension}");
                    if (File.Exists(iconFilePath))
                    {
                        newIconFilePath = Path.Combine(evoIconDirectory, $"{baseName}_{id}{fileExtension}");
                    }
                }

                // ✅ Kiểm tra xem file Evo có trùng với file trong Parent không
                string parentFilePath = Path.Combine(parentDirectory, $"{baseName}_{id}{fileExtension}");
                id++;
            }
            while (File.Exists(newEvoFilePath) || File.Exists(Path.Combine(parentDirectory, $"{baseName}_{id}{fileExtension}")));

            // ✅ Đổi tên file Evo
            try
            {
                File.Move(evoFile, newEvoFilePath);
                Logger.Log($"Renamed: {evoFile} -> {newEvoFilePath}");
            }
            catch (Exception ex)
            {
                //nothing wrong here... maybe?
            }

            // ✅ Đổi tên file Icon nếu có
            if (newIconFilePath != null)
            {
                string oldIconFilePath = Path.Combine(evoIconDirectory, $"{fileNameWithoutExt}{fileExtension}");
                if (File.Exists(oldIconFilePath))
                {
                    try
                    {
                        File.Move(oldIconFilePath, newIconFilePath);
                        Logger.Log($"Renamed Icon: {oldIconFilePath} -> {newIconFilePath}");
                    }
                    catch (Exception ex)
                    {
                        //nothing wrong here... maybe?
                    }
                }
            }
        }
    }

    public void ReIDFiles(string path)
    {
        CheckWrongTypeAndName(path, out List<string> correctFiles, false);
        Logger.Announce("Calculate ID...");
        ReID(correctFiles);
        Logger.Announce("Done~");
        CheckWrongTypeAndName(path, out List<string> correctFiles2, false);
        Logger.Announce("Re ID Evo File...");
        ReIDEvo(path);
    }
    void ReID(List<string> correctFiles)
    {
        if (correctFiles == null || correctFiles.Count == 0) return;

        List<string> tempFiles = new List<string>(correctFiles);
        string firstDirectory = Path.GetDirectoryName(tempFiles[0]);
        string iconDirectory = Path.Combine(firstDirectory, "Icon");

        List<string> currentDirectory = tempFiles
            .Where(file => string.Equals(Path.GetDirectoryName(file), firstDirectory, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (Directory.Exists(iconDirectory))
        {
            List<string> iconFiles = tempFiles
                .Where(file => string.Equals(Path.GetDirectoryName(file), iconDirectory, StringComparison.OrdinalIgnoreCase))
                .ToList();

            currentDirectory.AddRange(iconFiles);
        }

        tempFiles.RemoveAll(file => currentDirectory.Contains(file));

        // 1️⃣ Lấy danh sách ID từ các file PNG
        Dictionary<string, int> fileIDMap = new Dictionary<string, int>();
        List<int> idList = new List<int>();

        foreach (var file in currentDirectory)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            Match match = Regex.Match(fileNameWithoutExt, @"\d+");
            int id = match.Success ? int.Parse(match.Value) : 0;

            fileIDMap[file] = id;
            idList.Add(id);
        }

        // 2️⃣ Sắp xếp danh sách ID và tìm số thiếu
        idList.Sort();
        int newID = 0;
        Dictionary<int, int> idMapping = new Dictionary<int, int>();

        foreach (var id in idList)
        {
            if (!idMapping.ContainsKey(id))
            {
                idMapping[id] = newID;
                newID++;
            }
        }

        // 3️⃣ Đổi tên file dựa vào ID mới
        foreach (var file in fileIDMap.Keys)
        {
            string directory = Path.GetDirectoryName(file);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
            string fileExtension = Path.GetExtension(file);

            int oldID = fileIDMap[file];
            int newIDValue = idMapping[oldID];

            string newFileName = Regex.Replace(fileNameWithoutExt, @"\d+", newIDValue.ToString());
            string newFilePath = Path.Combine(directory, $"{newFileName}{fileExtension}");

            try
            {
                File.Move(file, newFilePath);
                Logger.Log($"Renamed: {file} -> {newFilePath}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error renaming {file}: {ex.Message}");
            }
        }

        ReID(tempFiles);
    }
}