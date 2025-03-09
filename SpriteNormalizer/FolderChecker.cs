using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class FolderChecker
{
    private List<string> folderCheckerList;
    public FolderChecker(List<string> folderCheckerList)
    {
        this.folderCheckerList = folderCheckerList;
    }
    public List<string> GetFolderCheckerList()
    {
        return folderCheckerList;
    }
    public void CheckFolder(string path)
    {
        if (folderCheckerList == null)
        {
            Logger.Error("folderCheckerList is null!");
            return;
        }

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Couldnt found path {path}");
            return;
        }

        Logger.Announce("Checking Folder...");

        List<string> actualFolders = Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
            .Select(folder => Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar).ToLower()) 
            .ToList();

        List<string> normalizedFolderCheckerList = folderCheckerList
            .Select(folder => Path.GetFullPath(Path.Combine(path, folder.TrimStart('\\'))).ToLower())
            .ToList();

        List<string> missingFolders = normalizedFolderCheckerList
            .Where(folder => !actualFolders.Contains(folder, StringComparer.OrdinalIgnoreCase))
            .ToList();

        List<string> extraFolders = actualFolders
            .Where(folder => !normalizedFolderCheckerList.Contains(folder, StringComparer.OrdinalIgnoreCase))
            .ToList();


        List<string> filteredExtraFolders = extraFolders
   .Where(file => !file.Contains(@"\element"))
   .ToList();

        if (missingFolders.Count > 0)
        {
            Logger.Warning("Missing Folder:");
            missingFolders.ForEach(Logger.Log);
        }
        else
        {
            Logger.Success("None Missing Folder");
        }

        if (filteredExtraFolders.Count > 0)
        {
            Logger.Warning("Extra Folder:");
            filteredExtraFolders.ForEach(Logger.Log);
        }
        else
        {
            Logger.Success("None Extra Folder");
        }
    }
}