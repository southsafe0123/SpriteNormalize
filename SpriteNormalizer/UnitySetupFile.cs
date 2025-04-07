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
            Console.WriteLine("Không tìm thấy thư mục template: " + templatePath);
            return;
        }

        if (!Directory.Exists(bossPath))
        {
            Console.WriteLine("Không tìm thấy thư mục boss: " + bossPath);
            return;
        }

        // Lấy danh sách file (ví dụ: .png) trong thư mục boss
        // Nếu bạn chỉ cần xử lý .png, thay thế "*" bằng "*.png"
        string[] bossFiles = Directory.GetFiles(bossPath);
        if (bossFiles.Length == 0)
        {
            Console.WriteLine("Không có file nào trong bossPath: " + bossPath);
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
                Console.WriteLine($"File '{fileName}' không đúng dạng Key_eventName_Value. Bỏ qua.");
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
                Console.WriteLine($"Không tìm thấy template meta cho key '{key}': {templateMetaFilePath}");
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

            Console.WriteLine($"Đã tạo meta cho file '{fileName}' với key '{key}', GUID {newGuid}.");
        }

        Console.WriteLine("Hoàn tất cập nhật meta cho tất cả file trong boss.");
    }

    /// <summary>
    /// Thực hiện quá trình cập nhật file meta cho các file trong Equipment.
    /// </summary>
    public void CreateNewMetaFilesEquipment(string templatePath, string equipmentPath)
    {
        if (!Directory.Exists(templatePath))
        {
            Console.WriteLine("Không tìm thấy thư mục template: " + templatePath);
            return;
        }
        if (!Directory.Exists(equipmentPath))
        {
            Console.WriteLine("Không tìm thấy thư mục equipment: " + equipmentPath);
            return;
        }

        // Lấy các file trong thư mục Equipment. 
        // Nếu chỉ xử lý PNG, dùng "*.png"
        string[] equipmentFiles = Directory.GetFiles(equipmentPath, "*.png");
        if (equipmentFiles.Length == 0)
        {
            Console.WriteLine("Không có file .png nào trong: " + equipmentPath);
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
                Console.WriteLine($"File '{fileName}' không đúng dạng eventName_Key_Value. Bỏ qua.");
                continue;
            }

            // Key = phần thứ hai
            string key = parts[1]; // "Back", "Helmet", v.v.

            // Tạo tên file meta template, ví dụ: "BackEquipTemplate.meta"
            string templateMetaFileName = $"{key}EquipTemplate.png.meta";
            string templateMetaFilePath = Path.Combine(templatePath, templateMetaFileName);

            if (!File.Exists(templateMetaFilePath))
            {
                Console.WriteLine($"Không tìm thấy template meta cho key '{key}': {templateMetaFilePath}");
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

            Console.WriteLine($"Đã tạo meta cho file '{fileName}' (Key='{key}'), GUID={newGuid}.");
        }

        Console.WriteLine("Hoàn tất cập nhật meta cho các file trong Equipment.");
    }
    public  void CreateNewMetaFilesNPC(string templatePath, string npcPath)
    {
        if (!Directory.Exists(templatePath))
        {
            Console.WriteLine("Không tìm thấy thư mục template: " + templatePath);
            return;
        }
        if (!Directory.Exists(npcPath))
        {
            Console.WriteLine("Không tìm thấy thư mục npcPath: " + npcPath);
            return;
        }

        // Lấy danh sách file .png
        string[] npcFiles = Directory.GetFiles(npcPath, "*.png");
        if (npcFiles.Length == 0)
        {
            Console.WriteLine("Không có file .png nào trong npcPath: " + npcPath);
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
                Console.WriteLine($"File '{fileName}' không đúng định dạng Key_eventName. Bỏ qua.");
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
                Console.WriteLine($"Không tìm thấy template meta cho key '{key}': {templateMetaFilePath}");
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

            Console.WriteLine($"Đã tạo meta cho file '{fileName}' (Key='{key}'), GUID={newGuid}.");
        }

        Console.WriteLine("Hoàn tất cập nhật meta cho các file NPC.");
    }

    public void CreateNewMetaFilesPet(string templatePath, string petPath)
    {
        if (!Directory.Exists(templatePath))
        {
            Console.WriteLine("❌ Không tìm thấy thư mục template: " + templatePath);
            return;
        }

        if (!Directory.Exists(petPath))
        {
            Console.WriteLine("❌ Không tìm thấy thư mục pet: " + petPath);
            return;
        }

        string[] petFiles = Directory.GetFiles(petPath, "*.png");
        if (petFiles.Length == 0)
        {
            Console.WriteLine("⚠️ Không có file .png nào trong thư mục pet.");
            return;
        }

        foreach (var filePath in petFiles)
        {
            string fileName = Path.GetFileName(filePath);                     // ví dụ: Dragon_Pet_01.png
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath); // Dragon_Pet_01

            string[] parts = fileNameWithoutExt.Split('_');
            if (parts.Length < 3)
            {
                Console.WriteLine($"⚠️ File '{fileName}' không đúng định dạng eventName_Key_Value. Bỏ qua.");
                continue;
            }

            string key = parts[1]; // phần thứ hai là Key

            string templateFileName = $"{key}Template.png.meta"; // ví dụ: PetTemplate.meta
            string templateFilePath = Path.Combine(templatePath, templateFileName);

            if (!File.Exists(templateFilePath))
            {
                Console.WriteLine($"⚠️ Không tìm thấy template tương ứng với Key '{key}': {templateFilePath}");
                continue;
            }

            string newMetaFilePath = filePath + ".meta";

            File.Copy(templateFilePath, newMetaFilePath, overwrite: true);

            string newGuid = Guid.NewGuid().ToString("N");
            ReplaceGuidInMeta(newMetaFilePath, newGuid);

            Console.WriteLine($"✅ Đã tạo meta cho '{fileName}' với GUID mới: {newGuid}");
        }

        Console.WriteLine("🎉 Hoàn tất cập nhật file meta cho thư mục Pet.");
    }

    public void CreateNewMetaFilesSkin(string templatePath, string skinPath)
    {
        if (!Directory.Exists(templatePath))
        {
            Console.WriteLine("❌ Không tìm thấy thư mục template: " + templatePath);
            return;
        }

        if (!Directory.Exists(skinPath))
        {
            Console.WriteLine("❌ Không tìm thấy thư mục skin: " + skinPath);
            return;
        }

        string[] skinFiles = Directory.GetFiles(skinPath, "*.png");
        if (skinFiles.Length == 0)
        {
            Console.WriteLine("⚠️ Không có file .png nào trong thư mục skin.");
            return;
        }

        foreach (string filePath in skinFiles)
        {
            string fileName = Path.GetFileName(filePath);                       // ví dụ: Summer_Body_01.png
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath); // Summer_Body_01
            string[] parts = fileNameWithoutExt.Split('_');

            if (parts.Length < 3)
            {
                Console.WriteLine($"⚠️ File '{fileName}' không đúng định dạng eventName_Key_Value. Bỏ qua.");
                continue;
            }

            string key = parts[1]; // phần thứ hai là Key, ví dụ: "Body"

            // Ví dụ: Body -> BodySkinTemplate.meta
            string templateMetaFileName = $"{key}SkinTemplate.png.meta";
            string templateMetaFilePath = Path.Combine(templatePath, templateMetaFileName);

            if (!File.Exists(templateMetaFilePath))
            {
                Console.WriteLine($"⚠️ Không tìm thấy template tương ứng với Key '{key}': {templateMetaFilePath}");
                continue;
            }

            string newMetaFilePath = filePath + ".meta";

            File.Copy(templateMetaFilePath, newMetaFilePath, overwrite: true);

            string newGuid = Guid.NewGuid().ToString("N");
            ReplaceGuidInMeta(newMetaFilePath, newGuid);

            Console.WriteLine($"✅ Đã tạo meta cho '{fileName}' với template '{templateMetaFileName}', GUID: {newGuid}");
        }

        Console.WriteLine("🎉 Hoàn tất cập nhật meta cho tất cả file trong Skin.");
    }
    public void CreateNewMetaFilesElement(string templatePath, string elementPath)
    {
        string templateMetaFile = Path.Combine(templatePath, "ElementTemplate.png.meta");
        if (!File.Exists(templateMetaFile))
        {
            Console.WriteLine("❌ Không tìm thấy template meta: " + templateMetaFile);
            return;
        }

        if (!Directory.Exists(elementPath))
        {
            Console.WriteLine("❌ Không tìm thấy thư mục element: " + elementPath);
            return;
        }

        // Duyệt tất cả file trong elementPath và các thư mục con
        string[] allFiles = Directory.GetFiles(elementPath, "*.*", SearchOption.AllDirectories);
        if (allFiles.Length == 0)
        {
            Console.WriteLine("⚠️ Không tìm thấy file nào trong Element.");
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

            Console.WriteLine($"✅ Meta created for: {filePath}, GUID: {newGuid}");
        }

        Console.WriteLine("🎉 Hoàn tất tạo file meta cho toàn bộ file trong Element (bao gồm thư mục con).");
    }
}
