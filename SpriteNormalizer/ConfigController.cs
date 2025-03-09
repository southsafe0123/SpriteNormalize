using System;
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
}
