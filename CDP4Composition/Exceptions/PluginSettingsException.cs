﻿// -------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsException.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using CDP4Composition.PluginSettingService;

    /// <summary>
    /// An <see cref="PluginSettingsException"/> is thrown when <see cref="PluginSettings"/> cannot be loaded or written
    /// </summary>
    public class PluginSettingsException : SettingsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSettingsException"/> class.
        /// </summary>
        public PluginSettingsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsException"/> class.
        /// </summary>
        /// <param name="message">
        /// The exception message
        /// </param>
        /// <param name="innerException">
        /// A reference to the inner <see cref="Exception"/>
        /// </param>
        public PluginSettingsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}