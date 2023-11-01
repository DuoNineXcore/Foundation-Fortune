using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using Exiled.API.Features;
using FoundationFortune.API.Models;
using LiteDB;
using MEC;

namespace FoundationFortune
{
    /// <summary>
    /// FIVE PEBBLES!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// </summary>
    public static class DirectoryIterator
    {
        public static string tempZipFile;
        public static void SetupDirectories()
        {
            if (!FoundationFortune.Singleton.Config.DirectoryIterator) return;
            
            List<string> directoriesToCreate = new List<string>
            {
                FoundationFortune.commonDirectoryPath,
                FoundationFortune.audioFilesPath,
                FoundationFortune.npcAudioFilesPath,
                FoundationFortune.playerAudioFilesPath
            };

            foreach (string directoryPath in directoriesToCreate)
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Log.SendRaw($"[Foundation Fortune's DirectoryIterator] Missing Directory detected. Directory created: {directoryPath}", ConsoleColor.DarkCyan);
                }
            }
            
            InitializeAudioFileDirectories();
            InitializeDatabase();
        }

        private static void InitializeAudioFileDirectories()
        {
            if (!FoundationFortune.Singleton.Config.DirectoryIteratorCheckAudio) return;

            string zipFileUrl = "https://github.com/DuoNineXcore/Foundation-Fortune-Assets/releases/latest/download/AudioFiles.zip";
            tempZipFile = Path.Combine(FoundationFortune.commonDirectoryPath, "AudioFiles.zip");

            using (WebClient client = new WebClient())
            {
                Log.SendRaw("[Foundation Fortune's DirectoryIterator] Checking Foundation Fortune Audio Files...", ConsoleColor.DarkCyan);

                client.DownloadFileCompleted += DownloadCompletedHandler;
                client.DownloadFileAsync(new Uri(zipFileUrl), tempZipFile);
            }
        }

        private static void DownloadCompletedHandler(object sender, AsyncCompletedEventArgs ev)
        {
            if (ev.Error != null)
            {
                Log.SendRaw($"[Foundation Fortune's DirectoryIterator] Failed to download AudioFiles.zip: {ev.Error.Message}", ConsoleColor.DarkBlue);
                return;
            }

            Log.SendRaw("[Foundation Fortune's DirectoryIterator] Foundation Fortune Audio Files downloaded. Beginning Extract...", ConsoleColor.DarkCyan);

            ExtractMissingAudioFiles(tempZipFile, FoundationFortune.audioFilesPath, ".ogg");
            File.Delete(tempZipFile);
        }

        private static void ExtractMissingAudioFiles(string zipFilePath, string destinationDirectory, string extension)
        {
            List<string> existingFiles = new List<string>();
            List<string> missingFiles = new List<string>();
            bool filesReplaced = false;

            using (ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (var entry in zipArchive.Entries)
                {
                    if (entry.FullName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        string fileName = Path.GetFileName(entry.FullName);
                        string destinationPath = Path.Combine(destinationDirectory, fileName);

                        if (!File.Exists(destinationPath))
                        {
                            entry.ExtractToFile(destinationPath, true);
                            filesReplaced = true;
                        }
                        else existingFiles.Add(fileName);
                    }
                    else missingFiles.Add(entry.FullName);
                }
            }

            if (existingFiles.Count > 0)
            {
                string existingFilesMessage = string.Join(", ", existingFiles);
                Log.SendRaw($"[Foundation Fortune's DirectoryIterator] The following files: {existingFilesMessage} already exist, not replacing those.", ConsoleColor.DarkCyan);
            }

            if (missingFiles.Count > 0)
            {
                string missingFilesMessage = string.Join(", ", missingFiles);
                Log.SendRaw($"[Foundation Fortune's DirectoryIterator] The following files are missing: {missingFilesMessage}.", ConsoleColor.DarkBlue);
            }

            if (filesReplaced) Log.SendRaw("[Foundation Fortune's DirectoryIterator] Successfully extracted audio files and replaced missing files!", ConsoleColor.DarkCyan);
            else Log.SendRaw("[Foundation Fortune's DirectoryIterator] All files already exist, no files need to be replaced.", ConsoleColor.DarkCyan);
        }

        private static void InitializeDatabase()
        {
            if (!FoundationFortune.Singleton.Config.DirectoryIteratorCheckDatabase) return;
             try
             {
                 string databaseFilePath = Path.Combine(FoundationFortune.commonDirectoryPath, "Foundation Fortune.db");

                 if (!File.Exists(databaseFilePath))
                 {
                     FoundationFortune.Singleton.db = new LiteDatabase(databaseFilePath);

                     var collection = FoundationFortune.Singleton.db.GetCollection<PlayerData>();
                     collection.EnsureIndex(x => x.UserId);

                     Log.SendRaw($"[Foundation Fortune's DirectoryIterator] Database created successfully at {databaseFilePath}", ConsoleColor.Magenta);
                 }
                 else
                 {
                     FoundationFortune.Singleton.db = new LiteDatabase(databaseFilePath);
                     Log.SendRaw($"[Foundation Fortune's DirectoryIterator] Database loaded successfully at {databaseFilePath}.", ConsoleColor.Magenta);
                 }
             }
             catch (Exception ex)
             {
                 Log.SendRaw($"[Foundation Fortune's DirectoryIterator] Failed to create/open database: {ex}", ConsoleColor.DarkMagenta);
                 Timing.CallDelayed(1f, FoundationFortune.Singleton.OnDisabled);
             }
         }
    }
}