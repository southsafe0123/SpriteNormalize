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
            Logger.Log("Input Folder Link: ");
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

                fileNameChanger.CheckFile(path);

                Logger.Log("Rename Ignore All Warning?(y/n)");
                if (Console.ReadLine() == "y")
                {
                    fileNameChanger.RenameFile(path);
                }
            }
            #endregion
        }


    }
}
