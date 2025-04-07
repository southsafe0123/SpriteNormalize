using System;
using System.Collections.Generic;
using System.IO;

public class ConfigController
{
    public static string LoadTxT(string folderPath, string txtName)
    {
        try
        {
            string[] txtFiles = Directory.GetFiles(folderPath, $"{txtName}.txt");
            string fileContent;
            fileContent = File.ReadAllText(txtFiles[0]);

            return fileContent;
        }
        catch
        {
            Logger.Error("Cant read txt, file created? file need character to read");
            return "?";
        }
    }
    public static string GetTemplatePath()
    {
        // Lấy thư mục hiện tại nơi file .exe đang chạy (bin\Debug hoặc bin\Release)
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Ghép thêm "Template"
        string templatePath = Path.Combine(baseDirectory, "Template");

        return templatePath;
    }
    public static string GetLastFolderName(string link)
    {
        link = link.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.GetFileName(link);
    }
    public static Dictionary<string,string> FileNameChangerConverter(string txtConfig)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        string[] entries = txtConfig.Split('*');

        foreach (string entry in entries)
        {
            string[] pair = entry.Split('-');

            if (pair.Length == 2)
            {
                string key = pair[0].Trim();   
                string value = pair[1].Trim(); 

                if (!result.ContainsKey(key)) 
                {
                    result[key] = value;
                }
                else
                {
                    Logger.Error($"Duplicate key/value: {key}/{value}, ignore this key/value");
                }
            }
        }
        return result;
    }
    public static List<string> FolderCheckerConverter(string txtConfig)
    {
        if (txtConfig == null || txtConfig.Length == 0) return null;
        List<string> result = new List<string>();
        string[] parts = txtConfig.Split('*');

        foreach (string part in parts)
        {
            result.Add(part.Trim());
        }
        return result;
    }
}
