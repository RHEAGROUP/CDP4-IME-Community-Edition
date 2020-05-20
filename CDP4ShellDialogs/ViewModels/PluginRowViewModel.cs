﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.ViewModels
{
    using System;

    using CDP4Composition.Modularity;
    using CDP4Composition.Services.AppSettingService;
    using Microsoft.Practices.Prism.Modularity;
    using ReactiveUI;

    /// <summary>
    /// The plugin row view model exposes some literal information on a module.
    /// </summary>
    public class PluginRowViewModel : ReactiveObject
    {
        /// <summary>
        /// The backing field for the <see cref="ProjectGuid"/> property
        /// </summary>
        private Guid projectGuid;

        /// <summary>
        /// The backing field for the <see cref="AssemblyName"/> property
        /// </summary>
        private string assemblyName;

        /// <summary>
        /// The backing field for the <see cref="Name"/> property
        /// </summary>
        private string name;

        /// <summary>
        /// The backing field for the <see cref="Description"/> property
        /// </summary>
        private string description;

        /// <summary>
        /// The backing field for the <see cref="Version"/> property
        /// </summary>
        private string version;

        /// <summary>
        /// The backing field for the <see cref="Company"/> property
        /// </summary>
        private string company;

        /// <summary>
        /// The backing field for the <see cref="IsPluginEnabled"/> property
        /// </summary>
        private bool isPluginEnabled;

        /// <summary>
        /// The backing field for the <see cref="IsRowEnabled"/> property
        /// </summary>
        private bool isRowEnabled;

        /// <summary>
        /// The backing field for the <see cref="IsMandatory"/> property
        /// </summary>
        private bool isMandatory;

        /// <summary>
        /// The backing field for the <see cref="IsRowDirty"/> property
        /// </summary>
        private bool isRowDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginRowViewModel"/> class.
        /// </summary>
        /// <param name="module">
        /// The <see cref="IModule"/> to extract information from.
        /// </param>
        public PluginRowViewModel(Manifest module, bool isPluginEnabled)
        {
            this.projectGuid = module.ProjectGuid;
            this.assemblyName = module.Name;
            this.name = module.Name;
            this.description = module.Description;
            this.version = module.Version.ToString();
            this.company = module.Author;
            this.isPluginEnabled = isPluginEnabled;
            this.isRowEnabled = true; //!module.IsMandatory;

            //this.isMandatory = module.IsMandatory;
        }

        /// <summary>
        /// Gets or sets the key of the Plugin
        /// </summary>
        public Guid ProjectGuid
        {
            get => this.projectGuid;
            set => this.RaiseAndSetIfChanged(ref this.projectGuid, value);
        }

        /// <summary>
        /// Gets or sets the assembly name of the Plugin
        /// </summary>
        public string AssemblyName
        {
            get => this.assemblyName;
            set => this.RaiseAndSetIfChanged(ref this.assemblyName, value);
        }

        /// <summary>
        /// Gets or sets the name of the Plugin
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the description of the Plugin
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.RaiseAndSetIfChanged(ref this.description, value);
        }

        /// <summary>
        /// Gets or sets the version of the Plugin
        /// </summary>
        public string Version
        {
            get => this.version;
            set => this.RaiseAndSetIfChanged(ref this.version, value);
        }

        /// <summary>
        /// Gets or sets the company
        /// </summary>
        public string Company
        {
            get => this.company;
            set => this.RaiseAndSetIfChanged(ref this.company, value);
        }

        /// <summary>
        /// Gets or sets the IsPluginEnabled
        /// </summary>
        public bool IsPluginEnabled
        {
            get => this.isPluginEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isPluginEnabled, value);
        }

        /// <summary>
        /// Gets or sets the IsRowEnabled (able to edit)
        /// </summary>
        public bool IsRowEnabled
        {
            get => this.isRowEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isRowEnabled, value);
        }

        /// <summary>
        /// Gets or sets the IsMandatory
        /// </summary>
        public bool IsMandatory
        {
            get => this.isMandatory;
            set => this.RaiseAndSetIfChanged(ref this.isMandatory, value);
        }

        /// <summary>
        /// Gets or sets the IsRowDirty
        /// </summary>
        public bool IsRowDirty
        {
            get => this.isRowDirty;
            set => this.RaiseAndSetIfChanged(ref this.isRowDirty, value);
        }
    }
}