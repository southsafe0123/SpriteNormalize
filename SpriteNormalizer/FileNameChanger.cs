using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            missingIcon.ForEach(Logger.Warning);
        }

        if (missingBaseImage.Count > 0)
        {
            Logger.Warning("These Icons Are Missing Their Own Files:");
            missingBaseImage.ForEach(Logger.Warning);
        }

        CheckMissingPart(tempFiles);
    }
    public void CheckFile(string path)
    {
        if (fileNameChangerDict == null)
        {
            Logger.Error("folderCheckerList is null!");
            return;
        }

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Couldnt found path {path}");
            return;
        }

        Logger.Log("Checking File...");
        CheckWrongTypeAndName(path, out List<string> correctFiles);
        CheckMissingPart(correctFiles);
    }
    void CheckWrongTypeAndName(string path, out List<string> callBackCorrectFiles)
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

        if (incorrectFiles.Count > 0)
        {
            Logger.Warning("Incorrect File Name: ");
            incorrectFiles.ForEach(Logger.Warning);
        }
        else
        {
            Logger.Success("None Incorrect File Name");
        }

        if (incorrectFormats.Count > 0)
        {
            Logger.Warning("Incorrect File Type: ");
            incorrectFormats.ForEach(Logger.Warning);
        }
        else
        {
            Logger.Success("None Incorrect File Type");
        }
    }
    public void RenameFile(string path)
    {

    }
}