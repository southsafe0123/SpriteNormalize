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
            Logger.Announce("Input Folder Link: ");
            string folderLink = Console.ReadLine();
            CheckFolder(folderLink);
            RenameFile(folderLink);

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

                Logger.Announce("Rename Ignore All Warning?(y/n)");
                if (Console.ReadLine() == "y")
                {
                    Logger.Announce("Starting Basic Rename...");
                    fileNameChanger.RenameFiles(correctFiles,false);
                    Logger.Success("Done~");
                    Logger.Announce("Starting Advance Rename...");
                    fileNameChanger.ReIDFiles(path);
                    Logger.Success("Renamed Success... Check your files again :D");
                }
            }
            #endregion
        }


    }
}
