﻿// -------------------------------------------------------------------------------------------------
// <copyright file="BoundsRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;    
    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="Bounds"/>
    /// </summary>
    public partial class BoundsRowViewModel : DiagramThingBaseRowViewModel<Bounds>
    {

        /// <summary>
        /// Backing field for <see cref="X"/>
        /// </summary>
        private float x;

        /// <summary>
        /// Backing field for <see cref="Y"/>
        /// </summary>
        private float y;

        /// <summary>
        /// Backing field for <see cref="Width"/>
        /// </summary>
        private float width;

        /// <summary>
        /// Backing field for <see cref="Height"/>
        /// </summary>
        private float height;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundsRowViewModel"/> class
        /// </summary>
        /// <param name="bounds">The <see cref="Bounds"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public BoundsRowViewModel(Bounds bounds, ISession session, IViewModelBase<Thing> containerViewModel) : base(bounds, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the X
        /// </summary>
        public float X
        {
            get { return this.x; }
            set { this.RaiseAndSetIfChanged(ref this.x, value); }
        }

        /// <summary>
        /// Gets or sets the Y
        /// </summary>
        public float Y
        {
            get { return this.y; }
            set { this.RaiseAndSetIfChanged(ref this.y, value); }
        }

        /// <summary>
        /// Gets or sets the Width
        /// </summary>
        public float Width
        {
            get { return this.width; }
            set { this.RaiseAndSetIfChanged(ref this.width, value); }
        }

        /// <summary>
        /// Gets or sets the Height
        /// </summary>
        public float Height
        {
            get { return this.height; }
            set { this.RaiseAndSetIfChanged(ref this.height, value); }
        }

	
        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.X = this.Thing.X;
            this.Y = this.Thing.Y;
            this.Width = this.Thing.Width;
            this.Height = this.Thing.Height;
        }
    }
}
