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
        RenamePetEvoFiles(Path.Combine(path,"Pet\\Evo"));
        RenameSkinEvoFiles(Path.Combine(path, "Skin\\Evo"));
        //ReIDEvo(path);
    }
    public void RenameSkinEvoFiles(string pathSkinEvo)
    {
        if (!Directory.Exists(pathSkinEvo))
        {
            Console.WriteLine("Skin Evo path does not exist.");
            return;
        }

        string pathSkinParent = Directory.GetParent(pathSkinEvo).FullName;

        // Step 3: Get all PNGs in parent folder (no subfolders)
        string[] parentPngFiles = Directory.GetFiles(pathSkinParent, "*.png", SearchOption.TopDirectoryOnly);

        // Step 4: Parse Value and Key to find max Key per Value
        Dictionary<string, int> currentIds = new Dictionary<string, int>();

        foreach (string filePath in parentPngFiles)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string[] parts = name.Split('_');
            if (parts.Length < 3) continue;

            string value = parts[1];
            if (int.TryParse(parts[2], out int key))
            {
                if (!currentIds.ContainsKey(value) || currentIds[value] < key)
                {
                    currentIds[value] = key;
                }
            }
        }

        // Step 5: Get all PNGs in evo (include subfolders)
        string[] evoPngFiles = Directory.GetFiles(pathSkinEvo, "*.png", SearchOption.AllDirectories);

        // Step 6 & 7: Group by file name (no extension) to keep consistent renaming
        Dictionary<string, List<string>> nameGroups = new Dictionary<string, List<string>>();

        foreach (string filePath in evoPngFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (!nameGroups.ContainsKey(fileName))
            {
                nameGroups[fileName] = new List<string>();
            }
            nameGroups[fileName].Add(filePath);
        }

        // Map for temp renaming
        Dictionary<string, string> renameMap = new Dictionary<string, string>();

        foreach (var group in nameGroups)
        {
            string name = group.Key;
            string[] parts = name.Split('_');
            if (parts.Length < 3) continue;

            string value = parts[1];

            if (!currentIds.ContainsKey(value))
            {
                currentIds[value] = 0;
            }

            int newId = ++currentIds[value];
            parts[2] = newId.ToString();
            string newName = string.Join("_", parts) + "_temp.png";

            foreach (string originalPath in group.Value)
            {
                string newFullPath = Path.Combine(Path.GetDirectoryName(originalPath), newName);
                File.Move(originalPath, newFullPath);
                renameMap[originalPath] = newFullPath;
            }
        }

        // Step 9: Remove "_temp" to finalize names
        foreach (var entry in renameMap)
        {
            string tempPath = entry.Value;
            string finalName = Path.GetFileName(tempPath).Replace("_temp", "");
            string finalPath = Path.Combine(Path.GetDirectoryName(tempPath), finalName);

            File.Move(tempPath, finalPath);
        }

        Console.WriteLine("Renaming completed.");
    }
    public void RenamePetEvoFiles(string pathEvo)
    {
        if (!Directory.Exists(pathEvo))
        {
            Console.WriteLine("Evo path does not exist.");
            return;
        }

        string pathParent = Directory.GetParent(pathEvo).FullName;

        // Step 3: Get all PNGs in parent folder (not subfolders)
        string[] parentPngFiles = Directory.GetFiles(pathParent, "*.png", SearchOption.TopDirectoryOnly);
        int currentId = parentPngFiles.Length;

        // Step 5: Get all PNGs in evo folder (include subfolders)
        string[] evoPngFiles = Directory.GetFiles(pathEvo, "*.png", SearchOption.AllDirectories);

        // Map original name without extension to list of file paths
        Dictionary<string, List<string>> nameGroups = new Dictionary<string, List<string>>();

        foreach (string filePath in evoPngFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (!nameGroups.ContainsKey(fileName))
            {
                nameGroups[fileName] = new List<string>();
            }
            nameGroups[fileName].Add(filePath);
        }

        // Store mapping from original path to temp renamed path
        Dictionary<string, string> renameMap = new Dictionary<string, string>();

        foreach (var group in nameGroups)
        {
            string originalName = group.Key;
            string[] parts = originalName.Split('_');

            if (parts.Length < 3)
            {
                Console.WriteLine($"Skipped invalid name: {originalName}");
                continue;
            }

            // Replace Key (last part) with currentId and add "_temp"
            parts[2] = currentId.ToString();
            string newName = string.Join("_", parts) + "_temp.png";

            foreach (string originalPath in group.Value)
            {
                string newFullPath = Path.Combine(Path.GetDirectoryName(originalPath), newName);
                File.Move(originalPath, newFullPath);
                renameMap[originalPath] = newFullPath;
            }

            currentId++;
        }

        // Step 9: Remove "_temp" from renamed files
        foreach (var entry in renameMap)
        {
            string tempPath = entry.Value;
            string finalName = Path.GetFileName(tempPath).Replace("_temp", "");
            string finalPath = Path.Combine(Path.GetDirectoryName(tempPath), finalName);

            File.Move(tempPath, finalPath);
        }

        Console.WriteLine("Renaming completed.");
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
    public void RenameIngredientPNGFiles(string pathIngredient, string eventName)
    {
        // Lấy danh sách file .png trong thư mục (chỉ cấp hiện tại)
        string[] pngFiles = Directory.GetFiles(pathIngredient, "*.png");

        // Chạy vòng lặp để đổi tên từng file
        for (int i = 0; i < pngFiles.Length; i++)
        {
            // Lấy đường dẫn cũ
            string oldFilePath = pngFiles[i];

            // Tạo tên mới cho file
            // Ví dụ: "MyEvent Item_1.png", "MyEvent Item_2.png"...
            string newFileName = $"{eventName} Item_{i}.png";

            // Kết hợp path + tên file mới để ra đường dẫn đầy đủ
            string newFilePath = Path.Combine(pathIngredient, newFileName);

            // Đổi tên file
            File.Move(oldFilePath, newFilePath);
        }
    }

    public void RenameNPCPngFiles(string path, string eventName)
    {
        // 1) Lấy tất cả file .png trong thư mục
        string[] allPngFiles = Directory.GetFiles(path, "*.png");
        if (allPngFiles.Length == 0)
        {
            Console.WriteLine("Không tìm thấy file .png nào trong thư mục: " + path);
            return;
        }

        // 2) Lấy 1 file bất kỳ để phân tích eventName (giả sử file format: eventName_Key_Value.png)
        //    Nếu bạn luôn truyền eventName từ bên ngoài, có thể bỏ qua bước này.
        string sampleFile = allPngFiles[0];
        string sampleNameWithoutExt = Path.GetFileNameWithoutExtension(sampleFile);
        // Tách tên file theo dấu '_'
        string[] parts = sampleNameWithoutExt.Split('_');
        if (parts.Length < 2)
        {
            Console.WriteLine("File mẫu không đúng định dạng eventName_Key_Value. " +
                              "Không thể xác định eventName từ file: " + sampleFile);
            return;
        }

        // 3) Duyệt qua tất cả file .png, phân tích Key và đổi tên
        for (int i = 0; i < allPngFiles.Length; i++)
        {
            string oldFilePath = allPngFiles[i];
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(oldFilePath);
            string fileExtension = Path.GetExtension(oldFilePath);

            // Tách tên file để lấy ra Key (phần thứ hai)
            string[] fileParts = fileNameWithoutExt.Split('_');
            if (fileParts.Length < 2)
            {
                // Nếu file không có định dạng eventName_Key_..., bỏ qua hoặc xử lý tùy ý
                continue;
            }
            string key = fileParts[1];  // Giả sử file dạng eventName_Key_...

            // Xác định tên file mới dựa trên Key
            string newKey = key;
            switch (key)
            {
                case "Back":
                    newKey = "Back";
                    break;
                case "Boot":
                    newKey = "Pant";
                    break;
                case "Cloth":
                    newKey = "Armor";
                    break;
                case "Helmet":
                    newKey = "Helmet";
                    break;
                case "Weapon":
                    newKey = "Weapon";
                    break;
                default:
                    // Nếu file không nằm trong nhóm cần đổi
                    // có thể bỏ qua hoặc xử lý riêng
                    continue;
            }

            // Định dạng tên mới: newKey_eventName + .png
            string newFileName = $"{newKey}_{eventName}{fileExtension}";
            string newFilePath = Path.Combine(path, newFileName);

            // Thực hiện đổi tên (move)
            File.Move(oldFilePath, newFilePath);
        }
    }
    public void RenameBossPngFiles(string path, string eventName)
    {
        // Bước 1: Đổi tên file trong thư mục gốc (path)
        // Lấy danh sách file .png có dạng eventName_Key_Value
        string[] pngFiles = Directory.GetFiles(path, "*.png");

        foreach (string oldFilePath in pngFiles)
        {
            // Tách tên file (bỏ phần mở rộng)
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(oldFilePath);
            string fileExtension = Path.GetExtension(oldFilePath); // .png

            // Tách theo '_' => [0] = eventName, [1] = Key, [2] = Value (giả sử đủ 3 phần)
            string[] parts = fileNameWithoutExt.Split('_');

            // Đảm bảo file đúng định dạng, ít nhất có 3 phần
            if (parts.Length < 3)
            {
                // Không đúng format => bỏ qua hoặc xử lý tùy ý
                continue;
            }

            // Bóc tách eventName, key, value
            string oldEventName = parts[0]; // Thường = eventName
            string key = parts[1];
            string value = parts[2];

            // Xác định tiền tố mới dựa trên Key
            string newPrefix;
            switch (key)
            {
                case "Back":
                    newPrefix = "BackBoss";
                    break;
                case "Boot":
                    newPrefix = "PantBoss";
                    break;
                case "Cloth":
                    newPrefix = "ArmorBoss";
                    break;
                case "Helmet":
                    newPrefix = "HelmetBoss";
                    break;
                case "Weapon":
                    newPrefix = "Weapons";
                    break;
                default:
                    // Nếu không nằm trong nhóm cần đổi thì bỏ qua
                    continue;
            }

            // Tạo tên file mới: newPrefix_eventName_Value + đuôi .png
            // Ví dụ: BackBoss_EventABC_01.png
            string newFileName = $"{newPrefix}_{eventName}_{value}{fileExtension}";

            // Ghép đường dẫn đầy đủ
            string newFilePath = Path.Combine(path, newFileName);

            // Đổi tên file
            File.Move(oldFilePath, newFilePath);
        }

        // Bước 2: Đổi tên file trong thư mục con icon (path\icon)
        string iconFolder = Path.Combine(path, "icon");
        if (Directory.Exists(iconFolder))
        {
            // Lấy tất cả file .png
            string[] iconPngFiles = Directory.GetFiles(iconFolder, "*.png");
            foreach (string oldIconFilePath in iconPngFiles)
            {
                string iconNameWithoutExt = Path.GetFileNameWithoutExtension(oldIconFilePath);
                string iconExtension = Path.GetExtension(oldIconFilePath); // .png

                // Có thể file icon cũ cũng có cấu trúc Key/Value, 
                // hoặc chỉ đơn giản tách ra Value. 
                // Ở đây ta giả định format cũ = eventName_Key_Value hoặc 
                // <bất kỳ>_Value => ta chỉ cần Value từ phần cuối.
                string[] iconParts = iconNameWithoutExt.Split('_');
                if (iconParts.Length < 2)
                {
                    // Nếu không đủ phần => bỏ qua hoặc xử lý tùy ý
                    continue;
                }

                // Giả sử value là phần cuối
                string iconValue = iconParts[iconParts.Length - 1];

                // Tạo tên mới: "IconBoss{eventName}_Value.png"
                // Ví dụ: IconBossEventABC_03.png
                string newIconFileName = $"IconBoss{eventName}_{iconValue}{iconExtension}";
                string newIconFilePath = Path.Combine(iconFolder, newIconFileName);

                // Đổi tên
                File.Move(oldIconFilePath, newIconFilePath);
            }
        }
        else
        {
            Console.WriteLine("Không tìm thấy thư mục icon tại: " + iconFolder);
        }
    }
}