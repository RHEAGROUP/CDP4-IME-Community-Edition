﻿// -------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.PluginSettingService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Reflection;
    using CDP4Composition.Exceptions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog;

    /// <summary>
    /// The service used to read and write the plugin configuration file
    /// </summary>
    [Export(typeof(IPluginSettingsService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PluginSettingsService : IPluginSettingsService
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Path to special windows "AppData" folder 
        /// </summary>
        public static string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// Application configuration folder path.
        /// </summary>
        public const string ConfigurationDirectoryFolder = @"RHEA\CDP4\";

        /// <summary>
        /// The setting file extension
        /// </summary>
        public const string SETTING_FILE_EXTENSION = ".settings.json";

        /// <summary>
        /// A dictionary used to store the user plugin-setting of the application
        /// </summary>
        private readonly Dictionary<string, PluginSettings> applicationUserPluginSettings;

        /// <summary>
        /// Initializes a new instance of <see cref="PluginSettingsService"/>
        /// </summary>
        public PluginSettingsService()
        {
            this.applicationUserPluginSettings = new Dictionary<string, PluginSettings>();
        }
        
        /// <summary>
        /// Configuration file Directory
        /// </summary>
        public string ApplicationConfigurationDirectory
        {
            get { return Path.Combine(AppDataFolder, ConfigurationDirectoryFolder); }
        }

        /// <summary>
        /// Reads the <see cref="T"/> plug in settings
        /// </summary>
        /// <typeparam name="T">A type of <see cref="PluginSettings"/></typeparam>
        /// <returns>
        /// An instance of <see cref="PluginSettings"/>
        /// </returns>
        public T Read<T>() where T : PluginSettings
        {
            var assemblyName = this.QueryAssemblyTitle(typeof(T));

            if (this.applicationUserPluginSettings.TryGetValue(assemblyName, out var result))
            {
                return result as T;
            }

            if (!Directory.Exists(this.ApplicationConfigurationDirectory))
            {
                logger.Debug("The CDP4 settings folder {0} does not yet exist", this.ApplicationConfigurationDirectory);
                Directory.CreateDirectory(this.ApplicationConfigurationDirectory);
                logger.Debug("The CDP4 settings folder {0} has been created", this.ApplicationConfigurationDirectory);
            }

            var path = Path.Combine(this.ApplicationConfigurationDirectory, assemblyName);

            logger.Debug("Read pluggin settings for {0} from {1}", assemblyName, path);

            try
            {
                using (var file = File.OpenText($"{path}{SETTING_FILE_EXTENSION}"))
                {
                    var serializer = new JsonSerializer();
                    result = (T)serializer.Deserialize(file, typeof(T));

                    // once the settings have been read from disk, add them to the cache for fast access
                    this.applicationUserPluginSettings.Add(assemblyName, result);

                    return (T)result;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The PluginSettings could not be read");

                throw new PluginSettingsException("The PluginSettings could not be read", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="PluginSettings"/> to disk
        /// </summary>
        /// <param name="pluginSettings">
        /// The <see cref="PluginSettings"/> that will be persisted
        /// </param>
        public void Write<T>(T pluginSettings) where T : PluginSettings
        {
            if (pluginSettings == null)
            {
                throw new ArgumentNullException(nameof(pluginSettings), "The pluginSettings may not be null");
            }

            var assemblyName = this.QueryAssemblyTitle(pluginSettings.GetType());

            if (!Directory.Exists(this.ApplicationConfigurationDirectory))
            {
                logger.Debug("The CDP4 settings folder {0} does not yet exist", this.ApplicationConfigurationDirectory);
                Directory.CreateDirectory(this.ApplicationConfigurationDirectory);
                logger.Debug("The CDP4 settings folder {0} has been created", this.ApplicationConfigurationDirectory);
            }

            var path = Path.Combine(this.ApplicationConfigurationDirectory, $"{assemblyName}{SETTING_FILE_EXTENSION}");

            logger.Debug("write settings to for {0} to {1}", assemblyName, path);

            using (var streamWriter = File.CreateText(path))
            {
                var serializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented
                };

                serializer.Serialize(streamWriter, pluginSettings);
            }

            if (this.applicationUserPluginSettings.ContainsKey(assemblyName))
            {
                this.applicationUserPluginSettings[assemblyName] = pluginSettings;
            }
            else
            {
                this.applicationUserPluginSettings.Add(assemblyName, pluginSettings);
            }
        }

        /// <summary>
        /// Queries the name of the assembly that contains the <see cref="Type"/>
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/> that is contained in the assembly for which the name is queried.
        /// </param>
        /// <returns>
        /// A string that contains the name of the assembly
        /// </returns>
        private string QueryAssemblyTitle(Type type)
        {
            return ((AssemblyTitleAttribute)type.Assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)))
                .Title;
        }
    }
}