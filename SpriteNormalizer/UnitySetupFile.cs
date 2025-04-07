using System.IO;
using System;
public class UnitySetupFile
{
    public void CreateNewMetaFilesUIIcon(string templatePath, string uiIconPath)
    {
        // 1) Xác định vị trí file meta template (ví dụ "template.meta")
        //    Nếu bạn dùng tên khác, hãy chỉnh lại.
        string templateMetaFile = templatePath;
        if (!File.Exists(templateMetaFile))
        {
            return;
        }

        // 2) Lấy tất cả file trong thư mục UIIcon
        //    (Ở đây không nhất thiết giới hạn .png, vì đề không nói rõ;
        //     nếu chỉ file .png thì dùng "*.png")
        string[] filesInUIIcon = Directory.GetFiles(uiIconPath);
        if (filesInUIIcon.Length == 0)
        {
            return;
        }

        // 3) Duyệt từng file và tạo meta
        foreach (string filePath in filesInUIIcon)
        {
            // Lấy tên file (bao gồm phần mở rộng)
            string fileName = Path.GetFileName(filePath);

            // Tạo tên meta: ví dụ "Xyz.png.meta"
            string metaFileName = fileName + ".meta";
            string metaFilePath = Path.Combine(uiIconPath, metaFileName);

            // 4) Sao chép file meta mẫu sang file meta mới
            File.Copy(templateMetaFile, metaFilePath, overwrite: true);

            // 5) Sinh GUID mới và thay thế trong meta vừa copy
            ReplaceGuidInMeta(metaFilePath, Guid.NewGuid().ToString("N"));
        }
    }

    private static void ReplaceGuidInMeta(string metaFilePath, string newGuid)
    {
        string[] lines = File.ReadAllLines(metaFilePath);
        for (int i = 0; i < lines.Length; i++)
        {
            // Loại bỏ khoảng trắng đầu, so sánh không phân biệt hoa/thường
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("guid: ", StringComparison.OrdinalIgnoreCase))
            {
                // Giữ lại phần thụt lề (nếu có) ở đầu dòng
                int leadingSpaceCount = lines[i].Length - trimmed.Length;
                string leadingSpaces = leadingSpaceCount > 0
                    ? lines[i].Substring(0, leadingSpaceCount)
                    : "";

                // Ghi đè dòng GUID
                lines[i] = $"{leadingSpaces}guid: {newGuid}";
                break; // chỉ thay dòng "guid:" đầu tiên
            }
        }
        File.WriteAllLines(metaFilePath, lines);
    }

    public void CreateNewMetaFilesBoss(string templatePath, string bossPath)
    {
        if (!Directory.Exists(templatePath))
        {
            Logger.Error("Template not found: " + templatePath);
            return;
        }

        if (!Directory.Exists(bossPath))
        {
            Logger.Error("Boss path not found: " + bossPath);
            return;
        }

        // Lấy danh sách file (ví dụ: .png) trong thư mục boss
        // Nếu bạn chỉ cần xử lý .png, thay thế "*" bằng "*.png"
        string[] bossFiles = Directory.GetFiles(bossPath);
        if (bossFiles.Length == 0)
        {
            Logger.Error("Not found any file in BossPath: " + bossPath);
            return;
        }

        foreach (var filePath in bossFiles)
        {
            // Lấy tên file, bỏ extension
            // Ví dụ: "ArmorBoss_Winter_01.png" -> fileNameWithoutExt = "ArmorBoss_Winter_01"
            string fileName = Path.GetFileName(filePath);
            string fileExtension = Path.GetExtension(filePath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

            // Tách theo '_': [0]=Key, [1]=eventName, [2]=Value (giả sử luôn đủ 3 phần)
            // Nếu có file không đúng định dạng, ta bỏ qua
            string[] parts = fileNameWithoutExt.Split('_');
            if (parts.Length < 3)
            {
                Logger.Warning($"File '{fileName}' wrong type Key_eventName_Value. Skiped!");
                continue;
            }

            // Key (phần đầu) => ví dụ: "ArmorBoss"
            string key = parts[0];

            // Tên file meta trong template (ví dụ: "ArmorBossTemplate.meta")
            // Tùy vào bạn đặt tên thật ở folder template, ở đây ví dụ: Key + "Template.meta"
            //   ArmorBoss -> "ArmorBossTemplate.meta"
            //   BackBoss  -> "BackBossTemplate.meta"
            // ...
            string templateMetaFileName = $"{key}Template.png.meta";
            string templateMetaFilePath = Path.Combine(templatePath, templateMetaFileName);

            if (!File.Exists(templateMetaFilePath))
            {
                Logger.Warning($"Not found template meta for key '{key}': {templateMetaFilePath}");
                continue;
            }

            // Đường dẫn file meta mới (VD: "ArmorBoss_Winter_01.png.meta")
            string newMetaFilePath = filePath + ".meta";

            // Copy file template -> meta mới
            File.Copy(templateMetaFilePath, newMetaFilePath, overwrite: true);

            // Tạo GUID mới
            string newGuid = Guid.NewGuid().ToString("N");

            // Ghi GUID mới vào meta
            ReplaceGuidInMeta(newMetaFilePath, newGuid);

            Logger.Success($"Created metafile '{fileName}'");
        }
    }

    /// <summary>
    /// Thực hiện quá trình cập nhật file meta cho các file trong Equipment.
    /// </summary>
    public void CreateNewMetaFilesEquipment(string templatePath, string equipmentPath)
    {
        if (!Directory.Exists(templatePath))
        {
            Logger.Error("Template not found: " + templatePath);
            return;
        }
        if (!Directory.Exists(equipmentPath))
        {

            Logger.Error("equipment path not found: " + equipmentPath);
            return;
        }

        // Lấy các file trong thư mục Equipment. 
        // Nếu chỉ xử lý PNG, dùng "*.png"
        string[] equipmentFiles = Directory.GetFiles(equipmentPath, "*.png");
        if (equipmentFiles.Length == 0)
        {
            Logger.Error("Not found any file in Equipment Path: " + equipmentPath);
            return;
        }

        foreach (var filePath in equipmentFiles)
        {
            // Tên file, ví dụ "Halloween_Back_01.png"
            string fileName = Path.GetFileName(filePath);               // "Halloween_Back_01.png"
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath); // "Halloween_Back_01"
            string fileExtension = Path.GetExtension(filePath);               // ".png"

            // Tách theo '_' => [0]=eventName, [1]=Key, [2]=Value (giả sử luôn có 3 phần)
            string[] parts = fileNameWithoutExt.Split('_');
            if (parts.Length < 3)
            {
                Logger.Warning($"File '{fileName}' wrong type Key_eventName_Value. Skiped!");
                continue;
            }

            // Key = phần thứ hai
            string key = parts[1]; // "Back", "Helmet", v.v.

            // Tạo tên file meta template, ví dụ: "BackEquipTemplate.meta"
            string templateMetaFileName = $"{key}EquipTemplate.png.meta";
            string templateMetaFilePath = Path.Combine(templatePath, templateMetaFileName);

            if (!File.Exists(templateMetaFilePath))
            {
                Logger.Warning($"Not found template meta for key '{key}': {templateMetaFilePath}");
                continue;
            }

            // Đường dẫn file meta mới (VD: "Halloween_Back_01.png.meta")
            string newMetaFilePath = filePath + ".meta";

            // Sao chép file template => meta mới
            File.Copy(templateMetaFilePath, newMetaFilePath, overwrite: true);

            // Sinh GUID mới
            string newGuid = Guid.NewGuid().ToString("N");

            // Thay GUID trong file meta
            ReplaceGuidInMeta(newMetaFilePath, newGuid);

            Logger.Success($"Created metafile '{fileName}'");
        }
    }
    public void CreateNewMetaFilesNPC(string templatePath, string npcPath)
    {
        if (!Directory.Exists(templatePath))
        {
            Logger.Error("Template not found: " + templatePath);
            return;
        }
        if (!Directory.Exists(npcPath))
        {
            Logger.Error("Npc path not found: " + npcPath);
            return;
        }

        // Lấy danh sách file .png
        string[] npcFiles = Directory.GetFiles(npcPath, "*.png");
        if (npcFiles.Length == 0)
        {
            Logger.Error("Not found any file in NPC Path: " + npcPath);
            return;
        }

        foreach (string filePath in npcFiles)
        {
            // Ví dụ: "Armor_Winter.png"
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            string fileName = Path.GetFileName(filePath);

            // Tách theo dấu '_': [0] = Key, [1] = eventName
            string[] parts = fileNameWithoutExt.Split('_');
            if (parts.Length < 2)
            {
                Logger.Warning($"File '{fileName}' wrong type Key_eventName_Value. Skiped!");
                continue;
            }

            // Key = phần đầu
            string key = parts[0]; // Armor, Back, Helmet, v.v.
            // eventName = parts[1] (nếu cần dùng đến)

            // Tìm file meta template, ví dụ: "ArmorNPCTemplate.meta"
            // Bạn có thể tuỳ ý thay đổi quy tắc đặt tên template
            string templateMetaFileName = $"{key}NPCTemplate.png.meta";
            string templateMetaFilePath = Path.Combine(templatePath, templateMetaFileName);

            if (!File.Exists(templateMetaFilePath))
            {
                Logger.Warning($"Not found template meta for key '{key}': {templateMetaFilePath}");
                continue;
            }

            // Đường dẫn file meta mới: "Armor_Winter.png.meta"
            string newMetaFilePath = filePath + ".meta";

            // Sao chép template -> meta mới
            File.Copy(templateMetaFilePath, newMetaFilePath, overwrite: true);

            // Tạo GUID mới
            string newGuid = Guid.NewGuid().ToString("N");

            // Ghi GUID vào meta
            ReplaceGuidInMeta(newMetaFilePath, newGuid);

            Logger.Success($"Created metafile '{fileName}'");
        }

    }

    public void CreateNewMetaFilesPet(string templatePath, string petPath)
    {
        if (!Directory.Exists(templatePath))
        {
            Logger.Error("Template not found: " + templatePath);
            return;
        }

        if (!Directory.Exists(petPath))
        {
            Logger.Error("Pet path not found: " + petPath);
            return;
        }

        string[] petFiles = Directory.GetFiles(petPath, "*.png");
        if (petFiles.Length == 0)
        {
            Logger.Error("Not found any file in PetPath: " + petPath);
            return;
        }

        foreach (var filePath in petFiles)
        {
            string fileName = Path.GetFileName(filePath);                     // ví dụ: Dragon_Pet_01.png
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath); // Dragon_Pet_01

            string[] parts = fileNameWithoutExt.Split('_');
            if (parts.Length < 3)
            {
                Logger.Warning($"File '{fileName}' wrong type Key_eventName_Value. Skiped!");
                continue;
            }

            string key = parts[1]; // phần thứ hai là Key

            string templateFileName = $"{key}Template.png.meta"; // ví dụ: PetTemplate.meta
            string templateFilePath = Path.Combine(templatePath, templateFileName);

            if (!File.Exists(templateFilePath))
            {
                Logger.Warning($"Not found template meta for key '{key}': {templateFilePath}");
                continue;
            }

            string newMetaFilePath = filePath + ".meta";

            File.Copy(templateFilePath, newMetaFilePath, overwrite: true);

            string newGuid = Guid.NewGuid().ToString("N");
            ReplaceGuidInMeta(newMetaFilePath, newGuid);
            Logger.Success($"Created metafile '{fileName}'");
        }
    }

    public void CreateNewMetaFilesSkin(string templatePath, string skinPath)
    {
        if (!Directory.Exists(templatePath))
        {
            Logger.Error("Template not found: " + templatePath);
            return;
        }

        if (!Directory.Exists(skinPath))
        {
            Logger.Error("Skin path not found: " + skinPath);
            return;
        }

        string[] skinFiles = Directory.GetFiles(skinPath, "*.png");
        if (skinFiles.Length == 0)
        {
            Logger.Error("Not found any file in Skin path: " + skinPath);

            return;
        }

        foreach (string filePath in skinFiles)
        {
            string fileName = Path.GetFileName(filePath);                       // ví dụ: Summer_Body_01.png
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath); // Summer_Body_01
            string[] parts = fileNameWithoutExt.Split('_');

            if (parts.Length < 3)
            {
                Logger.Warning($"File '{fileName}' wrong type Key_eventName_Value. Skiped!");
                continue;
            }

            string key = parts[1]; // phần thứ hai là Key, ví dụ: "Body"

            // Ví dụ: Body -> BodySkinTemplate.meta
            string templateMetaFileName = $"{key}SkinTemplate.png.meta";
            string templateMetaFilePath = Path.Combine(templatePath, templateMetaFileName);

            if (!File.Exists(templateMetaFilePath))
            {
                Logger.Warning($"Not found template meta for key '{key}': {templateMetaFilePath}");
                continue;
            }

            string newMetaFilePath = filePath + ".meta";

            File.Copy(templateMetaFilePath, newMetaFilePath, overwrite: true);

            string newGuid = Guid.NewGuid().ToString("N");
            ReplaceGuidInMeta(newMetaFilePath, newGuid);
            Logger.Success($"Created metafile '{fileName}'");
        }

    }
    public void CreateNewMetaFilesElement(string templatePath, string elementPath)
    {
        string templateMetaFile = Path.Combine(templatePath, "ElementTemplate.png.meta");
        if (!File.Exists(templateMetaFile))
        {
            Logger.Error("Template not found: " + elementPath);
            return;
        }

        if (!Directory.Exists(elementPath))
        {
            Logger.Error("Element path not found: " + elementPath);
            return;
        }

        // Duyệt tất cả file trong elementPath và các thư mục con
        string[] allFiles = Directory.GetFiles(elementPath, "*.*", SearchOption.AllDirectories);
        if (allFiles.Length == 0)
        {
            Logger.Error("Not found any file in element path: " + elementPath);
            return;
        }

        foreach (string filePath in allFiles)
        {
            // Bỏ qua file meta (chỉ xử lý file thật)
            if (filePath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                continue;

            string metaFilePath = filePath + ".meta";

            File.Copy(templateMetaFile, metaFilePath, overwrite: true);

            string newGuid = Guid.NewGuid().ToString("N");
            ReplaceGuidInMeta(metaFilePath, newGuid);

        }
        Logger.Success($"Created meta for elemnt folder too...");
    }

    public void MoveAllFilesUIICon(string from, string to)
    {
        // Create destination directory if it doesn't exist
        if (!Directory.Exists(to))
        {
            Directory.CreateDirectory(to);
        }

        // Get all files in the source directory
        string[] files = Directory.GetFiles(from);

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string destPath = Path.Combine(to, fileName);

            File.Copy(file, destPath, true); // Overwrite if exists
        }

        // Delete the source directory after copying
        Directory.Delete(from, true); // true to delete subdirectories and files

        Console.WriteLine("Move completed.");
    }
    public void MoveAllFiles(string from, string to, string customFolderName = "")
    {
        if (!Directory.Exists(from))
        {
            Console.WriteLine("Source folder does not exist.");
            return;
        }

        // Get all files in source folder
        string[] files = Directory.GetFiles(from);

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

            // Expected format: eventName_Key_Value
            string[] parts = fileNameWithoutExt.Split('_');

            if (parts.Length < 3)
            {
                Console.WriteLine($"Skipped invalid format: {fileName}");
                continue;
            }

            string key = parts[1];
            string targetFolder = customFolderName == "" ? Path.Combine(to, key) : Path.Combine(to, customFolderName);

            if (!Directory.Exists(targetFolder))
            {

                Directory.CreateDirectory(targetFolder);
            }

            string destPath = Path.Combine(targetFolder, fileName);

            File.Copy(filePath, destPath, true); // Overwrite if exists
        }

        // Delete source folder after copying
        Directory.Delete(from, true);

        Console.WriteLine("All files moved and source folder deleted.");
    }

    public void ReAlignBossFolder(string from, string to)
    {
        if (!Directory.Exists(from))
        {
            Console.WriteLine("Source folder does not exist.");
            return;
        }

        if (!Directory.Exists(to))
        {
            Directory.CreateDirectory(to);
        }

        string[] files = Directory.GetFiles(from);

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
            string destPath = Path.Combine(to, fileName);

            File.Move(filePath, destPath); // Move file
        }

        Console.WriteLine("All files moved and source folder deleted.");
    }
}
