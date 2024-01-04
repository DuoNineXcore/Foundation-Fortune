using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Discord;
using Exiled.API.Features;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Common.Interfaces.Configs;
using FoundationFortune.API.Core.Common.Models.Databases;
using LiteDB;
using MEC;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FoundationFortune;

/// <summary>
/// underhang
/// </summary>
public static class DirectoryIterator
{
    private static string _logFilePath;
    private static Queue<Action> PostLoadActions = new();

    //paths but i call them directories because it sounds cool
    public static readonly string CommonDirectoryPath = Path.Combine(Paths.IndividualConfigs, "this plugin is a performance issue", "Foundation Fortune Assets");
    public static readonly string DatabaseDirectoryPath = Path.Combine(CommonDirectoryPath, "Database");
    public static readonly string LogsDirectoryPath = Path.Combine(CommonDirectoryPath, "Logs");
    public static readonly string AudioFilesPath = Path.Combine(CommonDirectoryPath, "Sound Files");
    public static readonly string PlayerAudioFilesPath = Path.Combine(AudioFilesPath, "PlayerVoiceChatUsageType");
    public static readonly string NpcAudioFilesPath = Path.Combine(AudioFilesPath, "NPCVoiceChatUsageType");
    private static readonly string TempZipFile = Path.Combine(AudioFilesPath, "AudioFiles.zip");
    private static readonly List<string> DirectoriesToCreate = new()
    {
        CommonDirectoryPath, DatabaseDirectoryPath, AudioFilesPath, NpcAudioFilesPath, PlayerAudioFilesPath, LogsDirectoryPath
    };

    //config
    private static readonly Dictionary<string, IFoundationFortuneConfig> Configs = new();
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .WithTypeConverter(new VectorsConverter())
        .WithTypeConverter(new EmojiConverter())
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .WithTypeConverter(new VectorsConverter())
        .WithTypeConverter(new EmojiConverter())
        .DisableAliases()
        .WithMaximumRecursion(10000)
        .Build();

    
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
    
    public static void SetupDirectories()
    {
        if (!FoundationFortune.Instance.Config.DirectoryIterator) return;
        Log("Directory Iterator Enabled.", LogLevel.Info);
        foreach (var directoryPath in DirectoriesToCreate) CreateFile(directoryPath);
        if (FoundationFortune.Instance.Config.DirectoryIteratorCheckAudio) InitializeFileDirectories(); 
    }

    public static void InitializeDatabases()
    {
        try
        {
            if (!Directory.Exists(DatabaseDirectoryPath))
            {
                Directory.CreateDirectory(DatabaseDirectoryPath);
                Log($"Created database directory at {DatabaseDirectoryPath}", LogLevel.Info);
            }

            string playerStatsDbPath = Path.Combine(DatabaseDirectoryPath, "PlayerStats.db");
            FoundationFortune.Instance.PlayerStatsDatabase = new(playerStatsDbPath);
            var playerstatscollection =FoundationFortune.Instance.PlayerStatsDatabase.GetCollection<PlayerStats>();
            playerstatscollection.EnsureIndex(p => p.UserId);

            string questRotationDbPath = Path.Combine(DatabaseDirectoryPath, "QuestRotation.db");
            FoundationFortune.Instance.QuestRotationDatabase = new(questRotationDbPath);
            var questrotationcollection = FoundationFortune.Instance.QuestRotationDatabase.GetCollection<QuestRotationData>();
            questrotationcollection.EnsureIndex(p => p.UserId);

            string playerSettingsDbPath = Path.Combine(DatabaseDirectoryPath, "PlayerSettings.db");
            FoundationFortune.Instance.PlayerSettingsDatabase = new(playerSettingsDbPath);
            var playersettingsdatabase = FoundationFortune.Instance.PlayerSettingsDatabase.GetCollection<PlayerSettings>();
            playersettingsdatabase.EnsureIndex(p => p.UserId);

            Log("Databases initialized successfully.", LogLevel.Info);
        }
        catch (Exception ex)
        {
            Log($"Failed to initialize databases: {ex}", LogLevel.Error);
            Timing.CallDelayed(1f, FoundationFortune.Instance.OnDisabled);
        }
    }


    public static void CreateFile(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            if (directory != null) Directory.CreateDirectory(directory);
            Log($"File {filePath} created.", LogLevel.Debug);
        }
        else Log($"File {filePath} already exists.", LogLevel.Info);
    }

    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath)) File.Delete(filePath);
        else Log($"File '{filePath}' not found.", LogLevel.Error);
    }

    #region logs
    public static void InitializeLogFile()
    {
        if (!FoundationFortune.Instance.Config.DirectoryIteratorCreateLogFiles) return;
        if (!Directory.Exists(LogsDirectoryPath)) Directory.CreateDirectory(LogsDirectoryPath);
        string logFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        _logFilePath = Path.Combine(LogsDirectoryPath, logFileName);
    }

    private static void LogToFile(string message)
    {
        if (DateTime.Now.Date != DateTime.ParseExact(Path.GetFileNameWithoutExtension(_logFilePath), "yyyy-MM-dd_HH-mm-ss", null).Date) 
            InitializeLogFile();

        string logEntry = $"{DateTime.Now:HH:mm:ss} - {message}";
        if (_logFilePath != null) File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
    }
    #endregion

    #region github file auto downloader
    private static void InitializeFileDirectories()
    {
        Log("Checking Foundation Fortune Files...", LogLevel.Info);

        var missingFolders = FoundationFortune.Instance.Config.RequiredFolders.Where(folder => !Directory.Exists(Path.Combine(CommonDirectoryPath, folder))).ToList();
        var missingFiles = FoundationFortune.Instance.Config.RequiredFiles.Where(file => !File.Exists(Path.Combine(CommonDirectoryPath, file))).ToList();

        if (!missingFolders.Any() && !missingFiles.Any())
        {
            Log("All required folders and files are present.", LogLevel.Info);
            return;
        }

        using WebClient client = new();
        client.DownloadFileCompleted += (_, ev) => DownloadCompletedHandler(ev, missingFiles);
        client.DownloadFileAsync(new(FoundationFortune.Instance.Config.DirectoryIteratorFileURL), TempZipFile);
    }

    private static void DownloadCompletedHandler(AsyncCompletedEventArgs ev, List<string> missingFiles)
    {
        if (ev.Error != null)
        {
            Log($"Failed to download AudioFiles.zip: {ev.Error.Message}", LogLevel.Error);
            return;
        }

        Log("Foundation Fortune Files downloaded. Beginning Extract...", LogLevel.Info);

        ExtractMissingFiles(TempZipFile, CommonDirectoryPath, missingFiles);
        VerifyDownloadedFiles(TempZipFile, missingFiles);
        File.Delete(TempZipFile);
    }

    private static void VerifyDownloadedFiles(string zipFilePath, List<string> missingFiles)
    {
        using ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in zipArchive.Entries)
        {
            if (!entry.FullName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)) continue;
            if (!missingFiles.Contains(entry.FullName))
            {
                Log($"File in config not found in zip file: {entry.FullName}", LogLevel.Warn);
            }
        }
    }
    
    private static void ExtractMissingFiles(string zipFilePath, string destinationDirectory, List<string> missingFiles)
    {
        try
        {
            bool filesExtracted = false;

            using (ZipArchive zipArchive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (var entry in zipArchive.Entries)
                {
                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\") || !entry.FullName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!missingFiles.Contains(entry.FullName)) continue;
                    
                    string destinationPath = Path.Combine(destinationDirectory, entry.FullName);
                    string entryDirectory = Path.GetDirectoryName(destinationPath);

                    if (entryDirectory != null && !Directory.Exists(entryDirectory)) Directory.CreateDirectory(entryDirectory);

                    entry.ExtractToFile(destinationPath, true);
                    filesExtracted = true;
                }
            }

            Log(filesExtracted ? "Successfully extracted missing files." : "No files were extracted.", LogLevel.Info);
        }
        catch (Exception ex)
        {
            Log($"An error occurred during file extraction: {ex.Message}", LogLevel.Error);
        }
    }
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
            configInstance = new();
            SaveConfig(configInstance);
            Configs[fileName] = configInstance;
            Log($"Configuration file not found. Created new configuration: {fileName}.yml", LogLevel.Warn);
        }
        else
        {
            var ymlContent = File.ReadAllText(configFilePath);
            configInstance = PleaseDontDeserializeANullObject<T>(ymlContent);
            SaveConfig(configInstance);
            Configs[fileName] = configInstance;
            Log($"Loaded configuration: {fileName}.yml", LogLevel.Info);
        }

        if (PostLoadActions.Count <= 0) return;
        while (PostLoadActions.Count > 0)
        {
            var action = PostLoadActions.Dequeue();
            action?.Invoke();
        }
    }

    private static void SaveConfig<T>(T configInstance) where T : IFoundationFortuneConfig
    {
        var fileName = typeof(T).Name;
        var configFilePath = Path.Combine(CommonDirectoryPath, fileName + ".yml");
        var ymlContent = Serializer.Serialize(configInstance);
        File.WriteAllText(configFilePath, ymlContent);
    }
    
    public static T LoadAndAssignConfig<T>() where T : IFoundationFortuneConfig, new()
    {
        var fileName = typeof(T).Name;
        LoadConfig<T>();
        if (Configs.TryGetValue(fileName, out var config)) return (T)config;
        Log($"Configuration for class '{fileName}' not found.", LogLevel.Error);
        throw new KeyNotFoundException($"Configuration for class '{fileName}' not found.");
    }
    
    private static T PleaseDontDeserializeANullObject<T>(string ymlContent) where T : IFoundationFortuneConfig, new()
    {
        T deserializedInstance;
        try
        {
            deserializedInstance = Deserializer.Deserialize<T>(ymlContent);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            Log($"Deserialization error: {ex.Message}", LogLevel.Error);
            deserializedInstance = new();
        }

        var defaultInstance = new T();
        bool missingFields = false;
        foreach (var property in typeof(T).GetProperties())
        {
            var deserializedValue = property.GetValue(deserializedInstance);
            if (deserializedValue != null && !IsDefaultValue(deserializedValue, property.PropertyType)) continue;
            missingFields = true;
            var defaultValue = property.GetValue(defaultInstance);
            property.SetValue(deserializedInstance, defaultValue);
            Log($"Missing field '{property.Name}' set to default value.", LogLevel.Warn);
        }

        if (missingFields) PostLoadActions.Enqueue(() => SaveConfig(deserializedInstance));
        return deserializedInstance;
    }
    
    private static bool IsDefaultValue(object value, Type type)
    {
        if (type.IsValueType) return value.Equals(Activator.CreateInstance(type));
        return value == null;
    }
    #endregion
}