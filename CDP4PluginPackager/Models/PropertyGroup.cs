﻿namespace CDP4PluginPackager
{
    using System;
    using System.Xml.Serialization;

    using CDP4PluginPackager.Models;

    [XmlRoot(ElementName = "PropertyGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	public class PropertyGroup
	{
		[XmlElement(ElementName = "Configuration", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public Configuration Configuration { get; set; }

		[XmlElement(ElementName = "Platform", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public Platform Platform { get; set; }

		[XmlElement(ElementName = "ProjectGuid", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public Guid ProjectGuid { get; set; }

		[XmlElement(ElementName = "OutputType", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string OutputType { get; set; }

		[XmlElement(ElementName = "AppDesignerFolder", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string AppDesignerFolder { get; set; }

		[XmlElement(ElementName = "RootNamespace", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string RootNamespace { get; set; }

		[XmlElement(ElementName = "AssemblyName", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string AssemblyName { get; set; }

		[XmlElement(ElementName = "TargetFrameworkVersion", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string TargetFrameworkVersion { get; set; }

		[XmlElement(ElementName = "FileAlignment", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string FileAlignment { get; set; }

		[XmlElement(ElementName = "Deterministic", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string Deterministic { get; set; }

		[XmlElement(ElementName = "DebugSymbols", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string DebugSymbols { get; set; }

		[XmlElement(ElementName = "DebugType", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string DebugType { get; set; }

		[XmlElement(ElementName = "Optimize", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string Optimize { get; set; }

		[XmlElement(ElementName = "OutputPath", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string OutputPath { get; set; }

		[XmlElement(ElementName = "DefineConstants", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string DefineConstants { get; set; }

		[XmlElement(ElementName = "ErrorReport", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string ErrorReport { get; set; }

		[XmlElement(ElementName = "WarningLevel", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
		public string WarningLevel { get; set; }

		[XmlAttribute(AttributeName = "Condition")]
        public string Condition { get; set; }
	}
}

