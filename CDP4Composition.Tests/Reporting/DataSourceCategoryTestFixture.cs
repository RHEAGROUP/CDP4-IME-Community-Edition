﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceCategoryTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Reporting
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Reporting;

    using NUnit.Framework;

    [TestFixture]
    public class DataSourceCategoryTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Iteration iteration;

        private Category rootCat;
        private Category cat1;
        private Category cat2;
        private Category cat3;

        private ElementDefinition ed;

        [DefinedThingShortName("cat1")]
        private class TestCategory1 : ReportingDataSourceCategory<Row>
        {
            public bool GetValue() => this.Value;
        }

        [DefinedThingShortName("cat2")]
        private class TestCategory2 : ReportingDataSourceCategory<Row>
        {
            public bool GetValue() => this.Value;
        }

        [DefinedThingShortName("cat3")]
        private class TestCategory3 : ReportingDataSourceCategory<Row>
        {
        }

        private class Row : ReportingDataSourceRow
        {
            public TestCategory1 category1;
            public TestCategory2 category2;
        }

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, null);

            // Categories

            this.rootCat = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "rootCat",
                Name = "rootCat"
            };

            this.cache.TryAdd(
                new CacheKey(this.rootCat.Iid, null),
                new Lazy<Thing>(() => this.rootCat));

            this.cat1 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat1",
                Name = "cat1"
            };

            this.cache.TryAdd(
                new CacheKey(this.cat1.Iid, null),
                new Lazy<Thing>(() => this.cat1));

            this.cat2 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat2",
                Name = "cat2"
            };

            this.cache.TryAdd(
                new CacheKey(this.cat2.Iid, null),
                new Lazy<Thing>(() => this.cat2));

            this.cat3 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat3",
                Name = "cat3"
            };

            this.cache.TryAdd(
                new CacheKey(this.cat3.Iid, null),
                new Lazy<Thing>(() => this.cat3));

            // Element Definitions

            this.ed = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed",
                Name = "element definition"
            };

            // Structure

            this.iteration.TopElement = this.ed;
            this.ed.Category.Add(this.rootCat);
            this.ed.Category.Add(this.cat1);
        }

        [Test]
        public void VerifyThatNodeIdentifiesCategories()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.rootCat.ShortName)
                .Build();

            var node = new ReportingDataSourceNode<Row>(
                this.ed,
                hierarchy);

            Assert.IsNotNull(node.GetColumn<TestCategory1>());
            Assert.IsNotNull(node.GetColumn<TestCategory2>());
            Assert.Throws<KeyNotFoundException>(() => node.GetColumn<TestCategory3>());
        }

        [Test]
        public void VerifyCategoryShortNameInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.rootCat.ShortName)
                .Build();

            var node = new ReportingDataSourceNode<Row>(
                this.ed,
                hierarchy);

            var parameter1 = node.GetColumn<TestCategory1>();
            Assert.AreEqual("cat1", parameter1.ShortName);

            var parameter2 = node.GetColumn<TestCategory2>();
            Assert.AreEqual("cat2", parameter2.ShortName);
        }

        [Test]
        public void VerifyCategoryValueInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.rootCat.ShortName)
                .Build();

            var node = new ReportingDataSourceNode<Row>(
                this.ed,
                hierarchy);

            var parameter1 = node.GetColumn<TestCategory1>();
            Assert.AreEqual(true, parameter1.GetValue());

            var parameter2 = node.GetColumn<TestCategory2>();
            Assert.AreEqual(false, parameter2.GetValue());
        }
    }
}