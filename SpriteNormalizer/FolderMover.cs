using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

class FolderMover
{
    public bool CheckEventFolderExists(string link)
    {
        return Directory.Exists(link);
    }
    public void CopyFolder(string sourcePath, string destinationPath)
    {
        // Tạo thư mục đích nếu chưa tồn tại
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        // Sao chép tất cả các tệp trong thư mục gốc
        string[] files = Directory.GetFiles(sourcePath);
        foreach (string file in files)
        {
            // Lấy tên tệp (không bao gồm đường dẫn)
            string fileName = Path.GetFileName(file);

            // Tạo đường dẫn mới cho tệp trong thư mục đích
            string destFile = Path.Combine(destinationPath, fileName);

            // Sao chép tệp
            File.Copy(file, destFile, true);  // true = ghi đè nếu đã tồn tại
        }

        // Sao chép toàn bộ các thư mục con (đệ quy)
        string[] directories = Directory.GetDirectories(sourcePath);
        foreach (string directory in directories)
        {
            // Lấy tên thư mục con (không bao gồm đường dẫn)
            string folderName = Path.GetFileName(directory);

            // Tạo đường dẫn cho thư mục con trong thư mục đích
            string destFolder = Path.Combine(destinationPath, folderName);

            // Gọi đệ quy để sao chép thư mục con
            CopyFolder(directory, destFolder);
        }
    }
    public void CreateUIIconAndCopyFiles(string eventFolder)
    {
        // 1. Tạo thư mục “UIIcon” trong đường dẫn chỉ định
        string uiIconPath = Path.Combine(eventFolder, "UIIcon");
        Directory.CreateDirectory(uiIconPath);

        // 2. Tìm các thư mục có tên "icon" hoặc "Icon" (nếu bạn chỉ cần 2 trường hợp này).
        //    Nếu muốn bắt mọi dạng chữ hoa/thường (Icon, ICON...), hãy dùng cách lọc thủ công (OrdinalIgnoreCase).
        string[] iconFolders = Directory.GetDirectories(eventFolder, "icon", SearchOption.AllDirectories);
        
        // 3. Sao chép tất cả file trong mỗi folder tìm được sang UIIcon
        if (iconFolders.Length > 0)
        {
            foreach (string iconFolder in iconFolders)
            {
                string[] files = Directory.GetFiles(iconFolder);
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string destFilePath = Path.Combine(uiIconPath, fileName);
                    File.Copy(file, destFilePath, overwrite: true);
                }
            }

            // Xóa các folder icon sau khi sao chép
            foreach (string iconFolder in iconFolders)
            {
                // Đệ quy xóa toàn bộ nội dung
                Directory.Delete(iconFolder, recursive: true);
            }
        }
    }
    public void MoveIngredientFiles(string ingredientPath, string uiiconPath)
    {
        // Tạo thư mục đích (uiiconPath) nếu chưa tồn tại
        Directory.CreateDirectory(uiiconPath);

        // Lấy tất cả file trong ingredientPath
        string[] files = Directory.GetFiles(ingredientPath);

        // Di chuyển từng file sang thư mục uiiconPath
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string destFilePath = Path.Combine(uiiconPath, fileName);

            // Move sẽ di chuyển file sang vị trí mới, tương đương copy rồi xóa file cũ
            File.Move(file, destFilePath);
        }

        // Xóa thư mục ingredientPath, tham số true cho phép xóa đệ quy (các thư mục con)
        Directory.Delete(ingredientPath, recursive: true);
    }
}