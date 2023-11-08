using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Discord;
using Exiled.Loader.Features.Configs;
using FoundationFortune.API.Models.Classes.Player;
using FoundationFortune.API.Models.Interfaces;
using FoundationFortune.API.YamlConverters;
using FoundationFortune.Configs;
using LiteDB;
using MEC;
using YamlDotNet.Serialization;

namespace FoundationFortune;

/// <summary>
/// five pebbles hellloooooooooooo five peeeblesssssssssssssssssss
/// </summary>
public static class DirectoryIterator
{
    private static string tempZipFile;
    private const string zipFileUrl = "https://github.com/DuoNineXcore/Foundation-Fortune-Assets/releases/latest/download/AudioFiles.zip";
    private static readonly Dictionary<string, IFoundationFortuneConfig> _configs = new Dictionary<string, IFoundationFortuneConfig>();
    private static readonly IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
    private static readonly ISerializer _serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();

    public static void SetupDirectories()
    {
        if (!FoundationFortune.Singleton.Config.DirectoryIterator) return;

        List<string> directoriesToCreate = new List<string>
        {
            FoundationFortune.commonDirectoryPath,
            FoundationFortune.databaseDirectoryPath,
            FoundationFortune.audioFilesPath,
            FoundationFortune.npcAudioFilesPath,
            FoundationFortune.playerAudioFilesPath
        };

        foreach (var directoryPath in directoriesToCreate.Where(directoryPath => !Directory.Exists(directoryPath)))
        {
            Directory.CreateDirectory(directoryPath);
            FoundationFortune.Log($"Missing Directory detected. Directory created: {directoryPath}", LogLevel.Warn);
        }

        if (FoundationFortune.Singleton.Config.DirectoryIteratorCheckAudio) InitializeAudioFileDirectories();
        if (FoundationFortune.Singleton.Config.DirectoryIteratorCheckDatabase) InitializeDatabase();
    }

    #region Github Auto-Downloader
    private static void InitializeAudioFileDirectories()
    {
        tempZipFile = Path.Combine(FoundationFortune.audioFilesPath, "AudioFiles.zip");

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

        ExtractMissingAudioFiles(tempZipFile, FoundationFortune.commonDirectoryPath, ".ogg");
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
    public static void LoadConfig<T>() where T : IFoundationFortuneConfig, new()
    {
        var fileName = typeof(T).Name;
        if (_configs.ContainsKey(fileName)) return;
        var configFilePath = Path.Combine(FoundationFortune.commonDirectoryPath, fileName + ".yml");

        if (!File.Exists(configFilePath))
        {
            var newConfigInstance = new T();
            SaveConfig(newConfigInstance);
            _configs[fileName] = newConfigInstance;
            FoundationFortune.Log($"Configuration file not found. Created new configuration: {fileName}.yml", LogLevel.Info);
            return;
        }

        var ymlContent = File.ReadAllText(configFilePath);
        var configInstance = _deserializer.Deserialize<T>(ymlContent);

        var defaultInstance = new T();
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var ymlPropertyValue = property.GetValue(configInstance);
            var defaultValue = property.GetValue(defaultInstance);
            if (ymlPropertyValue == null || ymlPropertyValue.Equals(defaultValue)) property.SetValue(configInstance, defaultValue);
        }

        _configs[fileName] = configInstance;
        FoundationFortune.Log($"Loaded configuration: {fileName}.yml", LogLevel.Info);
    }
    
    private static void SaveConfig<T>(T configInstance) where T : IFoundationFortuneConfig
    {
        var fileName = typeof(T).Name;
        var configFilePath = Path.Combine(FoundationFortune.commonDirectoryPath, fileName + ".yml");

        var builder = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .DisableAliases()
            .WithMaximumRecursion(10000) 
            .WithTypeConverter(new VectorsConverter());
        
        var serializer = builder.Build();

        var ymlContent = serializer.Serialize(configInstance);
        File.WriteAllText(configFilePath, ymlContent);
        FoundationFortune.Log($"Saved configuration: {fileName}.yml", LogLevel.Info);
    }


    public static T GetConfig<T>() where T : IFoundationFortuneConfig, new()
    {
        var fileName = typeof(T).Name;
        if (!_configs.TryGetValue(fileName, out var config)) throw new KeyNotFoundException($"Configuration for class '{typeof(T).Name}' not found.");
        FoundationFortune.Log($"Retrieved configuration: {fileName}.yml", LogLevel.Debug);
        return (T)config;
    }
    #endregion
    
    #region Database Detection
    private static void InitializeDatabase()
    {
        try
        {
            string databaseFilePath = Path.Combine(FoundationFortune.databaseDirectoryPath, "Foundation Fortune.db");

            if (!File.Exists(databaseFilePath))
            {
                FoundationFortune.Singleton.db = new LiteDatabase(databaseFilePath);
                var collection = FoundationFortune.Singleton.db.GetCollection<PlayerData>();
                collection.EnsureIndex(x => x.UserId);
                FoundationFortune.Log($"Database created successfully at {databaseFilePath}", LogLevel.Info);
            }
            else
            {
                FoundationFortune.Singleton.db = new LiteDatabase(databaseFilePath);
                FoundationFortune.Log($"Database loaded successfully at {databaseFilePath}.", LogLevel.Info);
            }
        }
        catch (Exception ex)
        {
            FoundationFortune.Log($" Failed to create/open database: {ex}", LogLevel.Error);
            Timing.CallDelayed(1f, FoundationFortune.Singleton.OnDisabled);
        }
    }
    #endregion
}