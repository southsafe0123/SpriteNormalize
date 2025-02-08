using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpriteNormalizer
{
    internal static class FolderConfigReader
    {
        public static void LoadValidFolders(
            string configFilePath,
            out HashSet<string> validTopFolders,
            out Dictionary<string, HashSet<string>> validSubFolders,
            out HashSet<string> ignoredFolders)
        {
            validTopFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            validSubFolders = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            ignoredFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(configFilePath))
            {
                Console.WriteLine($"Error: Configuration file not found - {configFilePath}");
                return;
            }

            try
            {
                foreach (var line in File.ReadAllLines(configFilePath))
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;

                    bool ignoreFolder = trimmedLine.StartsWith("-");
                    string folderPath = trimmedLine.Trim('*', '-');

                    string[] pathParts = folderPath.Split('\\');

                    if (pathParts.Length == 1)
                    {
                        if (ignoreFolder)
                        {
                            ignoredFolders.Add(pathParts[0]);
                        }
                        else
                        {
                            validTopFolders.Add(pathParts[0]);
                        }
                    }
                    else
                    {
                        string parentFolder = pathParts[0];
                        string subFolder = string.Join("/", pathParts.Skip(1));

                        if (!validSubFolders.ContainsKey(parentFolder))
                        {
                            validSubFolders[parentFolder] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }

                        validSubFolders[parentFolder].Add(subFolder);
                    }
                }

                Console.WriteLine($"Loaded {validTopFolders.Count} top-level folders, {validSubFolders.Count} sub-folder groups, and {ignoredFolders.Count} ignored folders.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading configuration file: {ex.Message}");
            }
        }

        /// <summary>
        /// Load event name from a text file and return it as a string.
        /// </summary>
        public static void LoadEventName(string eventFilePath, out string eventName)
        {
            if (!File.Exists(eventFilePath))
            {
                Console.WriteLine($"Error: Event file not found - {eventFilePath}");
                eventName = "Unknown Event";
            }

            try
            {
                string eventNameInText = File.ReadAllText(eventFilePath).Trim();
                eventName= string.IsNullOrEmpty(eventNameInText) ? "Unnamed Event" : eventNameInText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading event file: {ex.Message}");
                eventName = "Error Reading Event";
            }
        }
    }
}
