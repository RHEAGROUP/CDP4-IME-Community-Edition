﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRDL.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ReferenceSourceRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class ReferenceSourceRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private Uri uri;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTheConstructorSetsTheProperties()
        {
            var shortname = "referenceSourceshortname";
            var name = "referenceSource name";
            var testSiteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri) { ShortName = "testRDL"};
            var organization = new Organization(Guid.NewGuid(), null, this.uri) { ShortName = "Rhea" };
            var versionDate = new DateTime(2016, 8, 6);
            var referenceSource = new ReferenceSource(Guid.NewGuid(), null, this.uri)
                                      {
                                          Name = name,
                                          ShortName = shortname,
                                          Container = testSiteRdl,
                                          Author = "John Doe",
                                          Language = "EU",
                                          PublicationYear = 2016,
                                          Publisher = organization,
                                          VersionIdentifier = "v1",
                                          VersionDate = new DateTime(2016, 8, 6)
                                      };


            testSiteRdl.ReferenceSource.Add(referenceSource);
            var referenceSourceRowViewModel = new ReferenceSourceRowViewModel(referenceSource, this.session.Object, null);

            Assert.AreEqual(shortname, referenceSourceRowViewModel.ShortName);
            Assert.AreEqual(name, referenceSourceRowViewModel.Name);
            Assert.AreEqual("John Doe", referenceSourceRowViewModel.Author);
            Assert.AreEqual("EU", referenceSourceRowViewModel.Language);
            Assert.AreEqual(2016, referenceSourceRowViewModel.PublicationYear);
            Assert.AreEqual("v1", referenceSourceRowViewModel.VersionIdentifier);
            Assert.AreEqual(versionDate, referenceSourceRowViewModel.VersionDate);
            Assert.AreEqual(organization, referenceSourceRowViewModel.Publisher);
            Assert.AreEqual(testSiteRdl.ShortName, referenceSourceRowViewModel.ContainerRdl);
            Assert.AreEqual(0, referenceSourceRowViewModel.ContainedRows.Count);
        }
    }
}
