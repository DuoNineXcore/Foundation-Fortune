using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Discord;
using Exiled.API.Features;
using FoundationFortune.API.Core.Models.Classes.Player;
using FoundationFortune.API.Core.Models.Interfaces;
using FoundationFortune.API.Core.Models.Interfaces.Configs;
using FoundationFortune.API.Core.YamlConverters;
using LiteDB;
using MEC;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FoundationFortune
{
    /// <summary>
    /// the leg (if this collapses we're all dead)
    /// </summary>
    public static class DirectoryIterator
    {
        private const string ZipFileUrl = "https://github.com/DuoNineXcore/Foundation-Fortune-Assets/releases/latest/download/AudioFiles.zip";
        private static string _logFilePath;

        //paths but i call them directories because it sounds cool
        public static readonly string CommonDirectoryPath = Path.Combine(Paths.IndividualConfigs, "this plugin is a performance issue", "Foundation Fortune Assets");
        public static readonly string DatabaseDirectoryPath = Path.Combine(CommonDirectoryPath, "Database");
        public static readonly string LogsDirectoryPath = Path.Combine(CommonDirectoryPath, "Logs");
        public static readonly string AudioFilesPath = Path.Combine(CommonDirectoryPath, "Sound Files");
        public static readonly string PlayerAudioFilesPath = Path.Combine(AudioFilesPath, "PlayerVoiceChatUsageType");
        public static readonly string NpcAudioFilesPath = Path.Combine(AudioFilesPath, "NPCVoiceChatUsageType");
        private static readonly string TempZipFile = Path.Combine(AudioFilesPath, "AudioFiles.zip");
        private static readonly List<string> DirectoriesToCreate = new List<string>
        {
            CommonDirectoryPath, DatabaseDirectoryPath, AudioFilesPath, NpcAudioFilesPath, PlayerAudioFilesPath, LogsDirectoryPath
        };
        
        //config
        private static readonly Dictionary<string, IFoundationFortuneConfig> Configs = new Dictionary<string, IFoundationFortuneConfig>();
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithTypeConverter(new VectorsConverter())
            .Build();

        public static void SetupDirectories()
        {
            if (!FoundationFortune.Instance.Config.DirectoryIterator) return;
            DirectoryIterator.Log($"Directory Iterator Enabled.", LogLevel.Info);

            foreach (var directoryPath in DirectoriesToCreate) CreateFile(directoryPath);

            if (FoundationFortune.Instance.Config.DirectoryIteratorCheckAudio) InitializeAudioFileDirectories();
            if (FoundationFortune.Instance.Config.DirectoryIteratorCheckDatabase) InitializeDatabase();
        }
        
        public static void CreateFile(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                if (directory != null) Directory.CreateDirectory(directory);
                DirectoryIterator.Log($"File {filePath} created.", LogLevel.Debug);
            }
            else DirectoryIterator.Log($"File {filePath} already exists.", LogLevel.Info);
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            else DirectoryIterator.Log($"File '{filePath}' not found.", LogLevel.Error);
        }

        #region logs
        public static void InitializeLogFile()
        {
            if (!FoundationFortune.Instance.Config.DirectoryIteratorCreateLogFiles) return;
            if (!Directory.Exists(LogsDirectoryPath)) Directory.CreateDirectory(LogsDirectoryPath);
            string logFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            _logFilePath = Path.Combine(LogsDirectoryPath, logFileName);
        }

        public static void Log(string message, LogLevel severity)
        {
            var declaringType = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType;
            if (declaringType == null) return;

            string className = declaringType.Name;
            string methodName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;

            string logEntry = $"[Foundation Fortune 1.0 - {severity.ToString().ToUpper()}] {className}::{methodName} -> {message}";
            string consoleLogEntry = $"[{severity.ToString().ToUpper()}] {className}::{methodName} -> {message}";
            if (FoundationFortune.Instance.Config.DirectoryIteratorCreateLogFiles) LogToFile(consoleLogEntry);

            switch (severity)
            {
                case LogLevel.Debug: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.DarkCyan); break;
                case LogLevel.Error: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.DarkBlue); break;
                case LogLevel.Warn: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.Blue); break;
                case LogLevel.Info: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.Cyan); break;
                default: throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
            }
        }

        public static void Log(string message, ConsoleColor color)
        {
            var declaringType = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType;
            if (declaringType == null) return;

            string className = declaringType.Name;
            string methodName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;

            string logEntry = $"[Foundation Fortune 1.0] {className}::{methodName} -> {message}";
            string consoleLogEntry = $"{className}::{methodName} -> {message}";
            if (FoundationFortune.Instance.Config.DirectoryIteratorCreateLogFiles) LogToFile(consoleLogEntry);
            Exiled.API.Features.Log.SendRaw(logEntry, color);
        }

        private static void LogToFile(string message)
        {
            if (DateTime.Now.Date != DateTime.ParseExact(Path.GetFileNameWithoutExtension(_logFilePath), "yyyy-MM-dd_HH-mm-ss", null).Date) 
                InitializeLogFile();

            string logEntry = $"{DateTime.Now:HH:mm:ss} - {message}";
            if (_logFilePath != null) File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
        #endregion

        #region database stuff
        private static void InitializeDatabase()
        {
            try
            {
                string databaseFilePath = Path.Combine(DatabaseDirectoryPath, "Foundation Fortune.db");
                FoundationFortune.Instance.DB = new LiteDatabase(databaseFilePath);
                var collection = FoundationFortune.Instance.DB.GetCollection<PlayerData>();
                collection.EnsureIndex(x => x.UserId);

                DirectoryIterator.Log(!File.Exists(databaseFilePath)
                        ? $"Database created successfully at {databaseFilePath}"
                        : $"Database loaded successfully at {databaseFilePath}.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                DirectoryIterator.Log($"Failed to create/open database: {ex}", LogLevel.Error);
                Timing.CallDelayed(1f, FoundationFortune.Instance.OnDisabled);
            }
        }
        #endregion

        #region github file auto downloader
        private static void InitializeAudioFileDirectories()
        {
            DirectoryIterator.Log("Checking Foundation Fortune Audio Files...", LogLevel.Info);

            string destinationDirectory = CommonDirectoryPath;
            string[] files = GetFilesInDirectory(destinationDirectory, ".ogg");

            if (files.Length > 0)
            {
                DirectoryIterator.Log("Foundation Fortune Audio Files already exist. No need to download.",
                    LogLevel.Info);
                return;
            }

            using WebClient client = new WebClient();
            client.DownloadFileCompleted += DownloadCompletedHandler;
            client.DownloadFileAsync(new Uri(ZipFileUrl), TempZipFile);
        }

        private static void DownloadCompletedHandler(object sender, AsyncCompletedEventArgs ev)
        {
            if (ev.Error != null)
            {
                DirectoryIterator.Log($"Failed to download AudioFiles.zip: {ev.Error.Message}", LogLevel.Error);
                return;
            }

            DirectoryIterator.Log("Foundation Fortune Audio Files downloaded. Beginning Extract...", LogLevel.Info);

            ExtractMissingAudioFiles(TempZipFile, CommonDirectoryPath, ".ogg");
            File.Delete(TempZipFile);
        }

        private static void ExtractMissingAudioFiles(string zipFilePath, string destinationDirectory, string extension)
        {
            try
            {
                List<string> existingFiles = new List<string>();
                List<string> missingFiles = new List<string>();
                bool filesReplaced = false;

                using (ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath))
                {
                    foreach (var entry in zipArchive.Entries)
                    {
                        try
                        {
                            if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\")) continue;
                            if (entry.FullName == ".") continue;

                            if (entry.FullName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                            {
                                string relativeEntryPath = entry.FullName;
                                string destinationPath = Path.Combine(destinationDirectory, relativeEntryPath);

                                string entryDirectory = Path.GetDirectoryName(destinationPath);
                                if (entryDirectory != null && !Directory.Exists(entryDirectory))
                                    Directory.CreateDirectory(entryDirectory);
                                if (!File.Exists(destinationPath))
                                {
                                    entry.ExtractToFile(destinationPath, true);
                                    filesReplaced = true;
                                }
                                else existingFiles.Add(relativeEntryPath);
                            }
                            else missingFiles.Add(entry.FullName);
                        }
                        catch (Exception ex)
                        {
                            DirectoryIterator.Log($"Error extracting file: {ex.Message}", LogLevel.Error);
                        }
                    }
                }

                string logMessage;
                if (filesReplaced)
                {
                    if (existingFiles.Count > 0)
                    {
                        string existingFilesMessage = string.Join(", ", existingFiles);
                        logMessage = $"Successfully extracted audio files and replaced missing files: {existingFilesMessage}!";
                    }
                    else logMessage = "Successfully extracted audio files and replaced missing files!";
                }
                else
                {
                    string missingFilesMessage = string.Join(", ", missingFiles);
                    logMessage = $"The following files are missing: {missingFilesMessage}.";
                }

                DirectoryIterator.Log(logMessage, LogLevel.Info);
            }
            catch (Exception ex)
            {
                DirectoryIterator.Log($"An unexpected error occurred: {ex.Message}", LogLevel.Error);
            }
        }

        private static string[] GetFilesInDirectory(string directory, string extension) => !Directory.Exists(directory)
                ? Array.Empty<string>()
                : Directory.GetFiles(directory, "*" + extension)
                    .Where(file => file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)).ToArray();

        #endregion

        #region configs deserializer
        private static void LoadConfig<T>() where T : IFoundationFortuneConfig, new()
        {
            var fileName = typeof(T).Name;
            if (Configs.ContainsKey(fileName)) return;
            var configFilePath = Path.Combine(CommonDirectoryPath, fileName + ".yml");

            T configInstance;

            if (!File.Exists(configFilePath))
            {
                configInstance = new T();
                SaveConfig(configInstance);
                Configs[fileName] = configInstance;
                DirectoryIterator.Log($"Configuration file not found. Created new configuration: {fileName}.yml",
                    LogLevel.Info);
            }
            else
            {
                var ymlContent = File.ReadAllText(configFilePath);
                configInstance = Deserializer.Deserialize<T>(ymlContent);
                var defaultInstance = new T();
                var properties = typeof(T).GetProperties();

                foreach (var property in properties)
                {
                    var ymlPropertyValue = property.GetValue(configInstance);
                    var defaultValue = property.GetValue(defaultInstance);
                    if (ymlPropertyValue == null) property.SetValue(configInstance, defaultValue);
                }

                SaveConfig(configInstance);
                Configs[fileName] = configInstance;
                DirectoryIterator.Log($"Loaded configuration: {fileName}.yml", LogLevel.Info);
            }
        }

        private static void SaveConfig<T>(T configInstance) where T : IFoundationFortuneConfig
        {
            var fileName = typeof(T).Name;
            var configFilePath = Path.Combine(CommonDirectoryPath, fileName + ".yml");

            var builder = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .DisableAliases()
                .WithMaximumRecursion(10000)
                .WithTypeConverter(new VectorsConverter());

            var serializer = builder.Build();
            var ymlContent = serializer.Serialize(configInstance);
            File.WriteAllText(configFilePath, ymlContent);
        }

        public static T LoadAndAssignConfig<T>() where T : IFoundationFortuneConfig, new()
        {
            var fileName = typeof(T).Name;
            LoadConfig<T>();
            if (!Configs.TryGetValue(fileName, out var config))
                throw new KeyNotFoundException($"Configuration for class '{typeof(T).Name}' not found.");
            return (T)config;
        }
        #endregion
    }
}