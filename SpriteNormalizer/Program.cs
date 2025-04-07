using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SpriteNormalizer
{
    class Program
    {
        static void Main()
        {
            Logger.Announce("Input Event Folder Link: ");
            string folderLink = Console.ReadLine();
            Logger.Announce("Input mmo_mft/code/asset Link: ");
            string assetLink = Console.ReadLine();
            CheckFolder(folderLink);
            RenameFile(folderLink);
            MovingFolder(folderLink);
            UpdateMetaFile(folderLink);
            MovingFile(folderLink);
            #region Function
            void CheckFolder(string path)
            {
                string txtFolderChecker = ConfigController.LoadTxT(AppDomain.CurrentDomain.BaseDirectory, "FolderChecker_Config");
                List<string> folderCheckerList = ConfigController.FolderCheckerConverter(txtFolderChecker);
                FolderChecker folderChecker = new FolderChecker(folderCheckerList);

                folderChecker.CheckFolder(path);
            }
            void RenameFile(string path)
            {
                string txtFileNameChanger = ConfigController.LoadTxT(AppDomain.CurrentDomain.BaseDirectory, "FileNameChanger_Config");
                Dictionary<string, string> fileNameChangerDict = ConfigController.FileNameChangerConverter(txtFileNameChanger);
                FileNameChanger fileNameChanger = new FileNameChanger(fileNameChangerDict);

                fileNameChanger.CheckFile(path, out List<string> correctFiles);

                Logger.Announce("Import Ignore All Warning?(y/n)");
                if (Console.ReadLine() == "y")
                {
                    Logger.Announce("Starting Basic Rename...");
                    fileNameChanger.RenameFiles(correctFiles, false);
                    Logger.Success("Done~");
                    Logger.Announce("Starting Advance Rename...");
                    fileNameChanger.ReIDFiles(path);

                    fileNameChangerDict.TryGetValue("eventName", out string eventName);
                    fileNameChanger.RenameIngredientPNGFiles(Path.Combine(path, "Ingredient"), eventName);
                    fileNameChanger.RenameNPCPngFiles(Path.Combine(path, "Npc"), eventName);
                    fileNameChanger.RenameBossPngFiles(Path.Combine(path, "Boss"), eventName);
                }
            }
            void MovingFolder(string path)
            {

                FolderMover folderMover = new FolderMover();
                string eventName = ConfigController.GetLastFolderName(path);
                string folderEventPath = Path.Combine(assetLink, $"Game\\Event\\{eventName}");
                bool isEventFolderExsist = folderMover.CheckEventFolderExists(folderEventPath);
                if (isEventFolderExsist)
                {
                    Logger.Warning($"Event folder exsist: {eventName}");
                    return;
                }
                folderMover.CopyFolder(path, folderEventPath);
                folderMover.CreateUIIconAndCopyFiles(folderEventPath);
                folderMover.MoveIngredientFiles(Path.Combine(folderEventPath, "Ingredient"), Path.Combine(folderEventPath, "UIIcon"));
                Logger.Success($"Done...");
            }
            void UpdateMetaFile(string path)
            {
                string eventName = ConfigController.GetLastFolderName(path);
                string templatePath = ConfigController.GetTemplatePath();
                string folderEventPath = Path.Combine(assetLink, $"Game\\Event\\{eventName}");
                UnitySetupFile unitySetup = new UnitySetupFile();
                unitySetup.CreateNewMetaFilesUIIcon(Path.Combine(templatePath, "IconTemplate.png.meta"), Path.Combine(folderEventPath, "UIIcon"));
                unitySetup.CreateNewMetaFilesBoss(templatePath, Path.Combine(folderEventPath, "Boss"));
                unitySetup.CreateNewMetaFilesEquipment(templatePath, Path.Combine(folderEventPath, "Equipment"));
                unitySetup.CreateNewMetaFilesNPC(templatePath, Path.Combine(folderEventPath, "Npc"));
                unitySetup.CreateNewMetaFilesPet(templatePath, Path.Combine(folderEventPath, "Pet"));
                unitySetup.CreateNewMetaFilesPet(templatePath, Path.Combine(folderEventPath, "Pet\\Evo"));
                unitySetup.CreateNewMetaFilesSkin(templatePath, Path.Combine(folderEventPath, "Skin"));
                unitySetup.CreateNewMetaFilesSkin(templatePath, Path.Combine(folderEventPath, "Skin\\Evo"));
                unitySetup.CreateNewMetaFilesElement(templatePath, Path.Combine(folderEventPath, "Element"));
            }
            void MovingFile(string path)
            {
                string txtFileNameChanger = ConfigController.LoadTxT(AppDomain.CurrentDomain.BaseDirectory, "FileNameChanger_Config");
                string eventName = ConfigController.GetLastFolderName(path);
                UnitySetupFile unitySetup = new UnitySetupFile();
                string folderEventPath = Path.Combine(assetLink, $"Game\\Event\\{eventName}");
                string folderUIIconPath = Path.Combine(assetLink, "Game\\GameAssets\\UIIcons");
                string folderItemPath = Path.Combine(assetLink, "Resources\\Icons\\Items");
                string folderPetpath = Path.Combine(assetLink, "Packages\\PixelFantasy\\PixelMonsters\\Event");

                unitySetup.MoveAllFilesUIICon(Path.Combine(folderEventPath, "UIIcon"), folderUIIconPath);
                unitySetup.MoveAllFiles(Path.Combine(folderEventPath, "Equipment"), folderItemPath);
                unitySetup.MoveAllFiles(Path.Combine(folderEventPath, "Skin\\Evo"), folderItemPath);
                unitySetup.MoveAllFiles(Path.Combine(folderEventPath, "Skin"), folderItemPath);
                unitySetup.MoveAllFiles(Path.Combine(folderEventPath, "Pet\\Evo"), folderPetpath, eventName);
                unitySetup.MoveAllFiles(Path.Combine(folderEventPath, "Pet"), folderPetpath, eventName);

                unitySetup.ReAlignBossFolder(Path.Combine(folderEventPath, "Boss"), Path.Combine(folderEventPath, "Boss\\Graphic"));
            }
            #endregion
        }
    }
}
