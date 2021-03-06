﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypeBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition;

    using NLog;

    /// <summary>
    /// Interaction logic for FileTypeBrowser
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class FileTypeBrowser : IPanelView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTypeBrowser"/> class.
        /// </summary>
        public FileTypeBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTypeBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public FileTypeBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
