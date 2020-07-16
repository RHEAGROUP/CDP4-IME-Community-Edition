﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceClass.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;

    using System.Collections.Generic;

    /// <summary>
    /// Class representing a reporting data source.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="ReportingDataSourceRowRepresentation"/> representing the data source rows.
    /// </typeparam>
    public class ReportingDataSourceClass<T> where T : ReportingDataSourceRowRepresentation, new()
    {
        /// <summary>
        /// The <see cref="ReportingDataSourceRow{T}"/> which is the root of the hierarhical tree.
        /// </summary>
        private readonly ReportingDataSourceRow<T> topRow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDataSourceClass{T}"/> class.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> upon which the data source is based.
        /// </param>
        /// <param name="categoryHierarchy">
        /// The <see cref="CategoryHierarchy"/> used for filtering the considered <see cref="ElementBase"/> items.
        /// </param>
        public ReportingDataSourceClass(Iteration iteration, CategoryHierarchy categoryHierarchy)
        {
            this.topRow = new ReportingDataSourceRow<T>(iteration.TopElement, categoryHierarchy);
        }

        /// <summary>
        /// Gets a tabular representation of the hierarhical tree upon which the data source is based.
        /// </summary>
        /// <returns>
        /// The tabular representation.
        /// </returns>
        public List<T> GetTabularRepresentation()
        {
            return this.topRow.GetTabularRepresentation();
        }
    }
}
