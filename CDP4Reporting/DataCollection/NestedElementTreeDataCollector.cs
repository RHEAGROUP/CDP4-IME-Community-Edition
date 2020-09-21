﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedElementTreeDataCollector.cs" company="RHEA System S.A.">
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

namespace CDP4Reporting.DataCollection
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;

    /// <summary>
    /// Class that can be used to collect data from the NestedElementTree / ProductTree.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="DataCollectorRow"/> representing the data collector rows.
    /// </typeparam>
    public class NestedElementTreeDataCollector<T> where T : DataCollectorRow, new()
    {
        /// <summary>
        /// The <see cref="DataCollectorNode{T}"/>'s which are the root elements of the hierarhical tree.
        /// </summary>
        internal readonly IEnumerable<DataCollectorNode<T>> TopNodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedElementTreeDataCollector{T}"/> class.
        /// </summary>
        /// <param name="categoryHierarchy">
        /// The <see cref="CategoryHierarchy"/> used for filtering the considered <see cref="ElementBase"/> items.
        /// </param>
        /// <param name="option">
        /// The <see cref="Option"/> for which the data object is built.
        /// </param>
        public NestedElementTreeDataCollector(
            CategoryHierarchy categoryHierarchy,
            Option option)
        {
            var nestedElements = new NestedElementTreeGenerator()
                .Generate(option)
                .ToList();

            this.TopNodes = new CategoryHierarchyDataCollectorNodesGenerator<T>().Generate(categoryHierarchy, nestedElements);
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> representation of the hierarhical tree upon which the data object is based.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public DataTable GetTable()
        {
            var dataTables = this.TopNodes.Select(x => x.GetTable()).ToList();

            if (!dataTables.Any())
            {
                return null;
            }

            var dataTable = dataTables.First();

            if (dataTables.Count > 1)
            {
                for (var dt = 1; dt < dataTables.Count; dt++)
                {
                    var mergeTable = dataTables[dt];
                    dataTable.Merge(mergeTable, false, MissingSchemaAction.Add);
                }
            }

            return dataTable;
        }
    }
}