using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Discord;
using FoundationFortune.API.Core.Models.Classes.Player;
using FoundationFortune.API.Core.Models.Interfaces;
using FoundationFortune.API.YamlConverters;
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
    private static string tempZipFile;
    private const string zipFileUrl = "https://github.com/DuoNineXcore/Foundation-Fortune-Assets/releases/latest/download/AudioFiles.zip";
    private static readonly Dictionary<string, IFoundationFortuneConfig> _configs = new Dictionary<string, IFoundationFortuneConfig>();
    private static readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .WithTypeConverter(new VectorsConverter())
        .Build();
    
    private static List<string> directoriesToCreate = new List<string>
    { 
        FoundationFortune.CommonDirectoryPath,
        FoundationFortune.DatabaseDirectoryPath,
        FoundationFortune.AudioFilesPath,
        FoundationFortune.NpcAudioFilesPath,
        FoundationFortune.PlayerAudioFilesPath
    };
    
    public static void SetupDirectories()
    {
        if (!FoundationFortune.Singleton.Config.DirectoryIterator) return;
        FoundationFortune.Log($"Directory Iterator Enabled.", LogLevel.Info);

        foreach (var directoryPath in directoriesToCreate.Where(directoryPath => !Directory.Exists(directoryPath)))
        {
            Directory.CreateDirectory(directoryPath);
            FoundationFortune.Log($"Missing Directory detected. Directory created: {directoryPath}", LogLevel.Warn);
        }

        if (FoundationFortune.Singleton.Config.DirectoryIteratorCheckAudio) InitializeAudioFileDirectories();
        if (FoundationFortune.Singleton.Config.DirectoryIteratorCheckDatabase) InitializeDatabase();
    }

    #region File Creation
    public static void CreateFile(string filePath, string content)
    {
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory)) if (directory != null) Directory.CreateDirectory(directory);
        File.WriteAllText(filePath, content);
        FoundationFortune.Log($"File {filePath} created.", LogLevel.Debug);
    }
    
    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath)) File.Delete(filePath);
        else FoundationFortune.Log($"File '{filePath}' not found.", LogLevel.Error);
    }
    #endregion

    #region Github Auto-Downloader
    private static void InitializeAudioFileDirectories()
    {
        tempZipFile = Path.Combine(FoundationFortune.AudioFilesPath, "AudioFiles.zip");

        using WebClient client = new WebClient();
        FoundationFortune.Log("Checking Foundation Fortune Audio Files...", LogLevel.Info);

        client.DownloadFileCompleted += DownloadCompletedHandler;
        client.DownloadFileAsync(new Uri(zipFileUrl), tempZipFile);
    }

    private static void DownloadCompletedHandler(object sender, AsyncCompletedEventArgs ev)
    {
        if (ev.Error != null)
        {
            FoundationFortune.Log($"Failed to download AudioFiles.zip: {ev.Error.Message}", LogLevel.Error);
            return;
        }

        FoundationFortune.Log("Foundation Fortune Audio Files downloaded. Beginning Extract...", LogLevel.Info);

        ExtractMissingAudioFiles(tempZipFile, FoundationFortune.CommonDirectoryPath, ".ogg");
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
                    string relativeEntryPath = entry.FullName;
                    string destinationPath = Path.Combine(destinationDirectory, relativeEntryPath);

                    if (relativeEntryPath.EndsWith("/"))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    string entryDirectory = Path.GetDirectoryName(destinationPath);
                    if (entryDirectory != null) Directory.CreateDirectory(entryDirectory);

                    if (!File.Exists(destinationPath))
                    {
                        entry.ExtractToFile(destinationPath, true);
                        filesReplaced = true;
                    }
                    else existingFiles.Add(relativeEntryPath);
                }
                else missingFiles.Add(entry.FullName);
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

        FoundationFortune.Log(logMessage, LogLevel.Info);
    }
    #endregion

    #region Multiple Configs Deserializer
    private static void LoadConfig<T>() where T : IFoundationFortuneConfig, new()
    {
        var fileName = typeof(T).Name;
        if (_configs.ContainsKey(fileName)) return;
        var configFilePath = Path.Combine(FoundationFortune.CommonDirectoryPath, fileName + ".yml");

        T configInstance;

        if (!File.Exists(configFilePath))
        {
            configInstance = new T();
            SaveConfig(configInstance);
            _configs[fileName] = configInstance;
            FoundationFortune.Log($"Configuration file not found. Created new configuration: {fileName}.yml", LogLevel.Info);
        }
        else
        {
            var ymlContent = File.ReadAllText(configFilePath);
            configInstance = _deserializer.Deserialize<T>(ymlContent);
            var defaultInstance = new T();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var ymlPropertyValue = property.GetValue(configInstance);
                var defaultValue = property.GetValue(defaultInstance);
                if (ymlPropertyValue == null || ymlPropertyValue.Equals(defaultValue)) property.SetValue(configInstance, defaultValue);
            }

            SaveConfig(configInstance);
            _configs[fileName] = configInstance;
            FoundationFortune.Log($"Loaded configuration: {fileName}.yml", LogLevel.Info);
        }
    }
    
    private static void SaveConfig<T>(T configInstance) where T : IFoundationFortuneConfig
    {
        var fileName = typeof(T).Name;
        var configFilePath = Path.Combine(FoundationFortune.CommonDirectoryPath, fileName + ".yml");

        var builder = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .DisableAliases()
            .WithMaximumRecursion(10000)
            .WithTypeConverter(new VectorsConverter());

        var serializer = builder.Build();
        var ymlContent = serializer.Serialize(configInstance);
        
        File.WriteAllText(configFilePath, ymlContent);
        FoundationFortune.Log($"Saved configuration: {fileName}.yml", LogLevel.Info);
    }

    public static T LoadAndAssignConfig<T>() where T : IFoundationFortuneConfig, new()
    {
        var fileName = typeof(T).Name;
        LoadConfig<T>();
        if (!_configs.TryGetValue(fileName, out var config))
            throw new KeyNotFoundException($"Configuration for class '{typeof(T).Name}' not found.");
        FoundationFortune.Log($"Retrieved configuration: {fileName}.yml", LogLevel.Debug);
        return (T)config;
    }
    #endregion
    
    #region check if the database is still alive
    private static void InitializeDatabase()
    {
        try
        {
            string databaseFilePath = Path.Combine(FoundationFortune.DatabaseDirectoryPath, "Foundation Fortune.db");

            FoundationFortune.Singleton.db = new LiteDatabase(databaseFilePath);
            var collection = FoundationFortune.Singleton.db.GetCollection<PlayerData>();
            collection.EnsureIndex(x => x.UserId);

            FoundationFortune.Log(!File.Exists(databaseFilePath) ? $"Database created successfully at {databaseFilePath}" : $"Database loaded successfully at {databaseFilePath}.", LogLevel.Info);
        }
        catch (Exception ex)
        {
            FoundationFortune.Log($"Failed to create/open database: {ex}", LogLevel.Error);
            Timing.CallDelayed(1f, FoundationFortune.Singleton.OnDisabled);
        }
    }
    #endregion
}