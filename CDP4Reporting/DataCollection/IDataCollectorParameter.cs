﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataCollectorParameter.cs" company="RHEA System S.A.">
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

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The interface that defines members of implementing classes of <see cref="IDataCollectorParameter"/>
    /// </summary>
    internal interface IDataCollectorParameter
    {
        /// <summary>
        /// Gets a flag that indicates whether this instance has <see cref="IValueSet"/>s.
        /// </summary>
        bool HasValueSets { get; }

        /// <summary>
        /// Gets a flag that indicates that a parameter also collects parent values up a tree of <see cref="CategoryDecompositionHierarchy"/>s
        /// </summary>
        bool CollectParentValues { get; }

        /// <summary>
        /// Gets or sets the associated field name prefix in the result Data Object.
        /// </summary>
        string ParentValuePrefix { get; set; }

        /// <summary>
        /// The ValueSets of the associated object.
        /// The <see cref="IEnumerable{T}"/>s of the associated object/>.
        /// </summary>
        IEnumerable<IValueSet> ValueSets { get; set; }
    }
}
