﻿// -------------------------------------------------------------------------------------------------
// <copyright file="Compile.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// <summary>
//   This file has been generated using a csproj file with this tool to convert Xml structure into Csharp: https://xmltocsharp.azurewebsites.net/
// <link>
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4PluginPackager.Models.AutoGen
{
    using System.Xml.Serialization;

    /// <summary>
    /// Autogenerated class matching a "Compile" tag in a csproj file 
    /// </summary>
    [XmlRoot(ElementName = "Compile", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class Compile
    {
        [XmlAttribute(AttributeName = "Include")]
        public string Include { get; set; }

        [XmlElement(ElementName = "DependentUpon", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string DependentUpon { get; set; }
    }
}