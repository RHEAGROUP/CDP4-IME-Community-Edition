﻿// -------------------------------------------------------------------------------------------------
// <copyright file="BrowserViewModelBaseTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm
{
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Navigation.Events;
    
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    
    using Microsoft.Practices.ServiceLocation;
    
    using Moq;
    
    using NUnit.Framework;
    
    using System;

    using ReactiveUI;
    
    [TestFixture]
    public class BrowserViewModelBaseTestFixture
    {
        private SiteDirectory siteDir;
        private Person person;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IPermissionService> permmissionService; 
        private Mock<ISession> session;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.person = new Person(Guid.NewGuid(), this.cache, null);

            this.siteDir.Person.Add(this.person);

            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigation.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.dialogNavigation.Object);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.permmissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permmissionService.Object);
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
        }

        [Test]
        public void AssertThatSelectingARowCallsCDPMessageBus()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);

            var selectedThingChangedRaised = false;
            CDPMessageBus.Current.Listen<SelectedThingChangedEvent>().Subscribe(_ => selectedThingChangedRaised = true);

            browser.SelectedThing = new RowTestClass(this.person, this.session.Object, this.dialogNavigation.Object);

            Assert.IsTrue(selectedThingChangedRaised);
        }

        [Test]
        public void AssertThatCreateCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);
            browser.CreateCommand.Execute(null);

            
            this.dialogNavigation.Verify(x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.dialogNavigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void AssertThatDeleteCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);
            Assert.DoesNotThrow(() => browser.DeleteCommand.Execute(null));
        }

        [Test]
        public void AssertThatEditCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);
            Assert.IsFalse(browser.UpdateCommand.CanExecute(null));

            browser.SelectedThing = new RowTestClass(this.person, this.session.Object, this.dialogNavigation.Object);
            browser.UpdateCommand.Execute(null);

            this.dialogNavigation.Verify(x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Update, this.dialogNavigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void AssertThatInspectCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);
            Assert.IsFalse(browser.UpdateCommand.CanExecute(null));

            browser.SelectedThing = new RowTestClass(this.person, this.session.Object, this.dialogNavigation.Object);
            browser.InspectCommand.Execute(null);
            this.dialogNavigation.Verify(x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Inspect, this.dialogNavigation.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void AssertThatRefreshCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);
            Assert.DoesNotThrow(() => browser.RefreshCommand.Execute(null));
            session.Verify(session => session.Refresh());
        }

        [Test]
        public void AssertThatExportCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);
            Assert.DoesNotThrow(() => browser.ExportCommand.Execute(null));
        }

        [Test]
        public void AssertThatHelpCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);
            Assert.DoesNotThrow(() => browser.HelpCommand.Execute(null));
        }

        [Test]
        public void VerifyThatDeprecateCommandWorks()
        {
            this.permmissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);
            Assert.IsFalse(browser.DeprecateCommand.CanExecute(null));
            browser.SelectedThing = new RowTestClass(this.person, this.session.Object, this.dialogNavigation.Object);
            
            browser.ComputePermission();

            Assert.IsTrue(browser.DeprecateCommand.CanExecute(null));
            browser.DeprecateCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyDomainSwitchEventIsCaught()
        {
            var iteration = new Iteration(Guid.NewGuid(), null, null);
            var browser = new IterationBrowserTestClass(iteration, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);
           

            var option = new Option(Guid.NewGuid(), null, null);
            iteration.Option.Add(option);

            var optionbrowser = new OptionBrowserTestClass(option, this.session.Object, this.dialogNavigation.Object, this.navigation.Object, null, null);

            CDPMessageBus.Current.SendMessage(new DomainChangedEvent(iteration, new DomainOfExpertise { Name = "changed", ShortName = "ch" }));
            Assert.AreEqual("changed [ch]", browser.DomainOfExpertise);
            Assert.AreEqual("changed [ch]", optionbrowser.DomainOfExpertise);
        }
    }

    internal class BrowserTestClass : BrowserViewModelBase<SiteDirectory>
    {
        internal BrowserTestClass(SiteDirectory siteDir, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService)
        {
            this.CreateCommand = ReactiveCommand.Create();
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<Person>(siteDir));
        }
    }

    internal class IterationBrowserTestClass : BrowserViewModelBase<Iteration>
    {
        internal IterationBrowserTestClass(Iteration it, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(it, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService)
        {
            this.CreateCommand = ReactiveCommand.Create();
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<Person>(it));
        }
    }

    internal class OptionBrowserTestClass : BrowserViewModelBase<Option>
    {
        internal OptionBrowserTestClass(Option op, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(op, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService)
        {
            this.CreateCommand = ReactiveCommand.Create();
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<Person>(op));
        }
    }

    internal class RowTestClass : RowViewModelBase<Person>
    {
        internal RowTestClass(Person person, ISession session, IThingDialogNavigationService dialogNav)
            : base(person, session, null)
        {
        }
    }
}