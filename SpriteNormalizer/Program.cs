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
            Logger.Log(AppDomain.CurrentDomain.BaseDirectory);
            string txtFolderChecker = ConfigController.LoadTxT(AppDomain.CurrentDomain.BaseDirectory,"FolderChecker_Config");
            Logger.Log(txtFolderChecker);
        }
    }
}
