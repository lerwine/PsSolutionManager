using Microsoft.Build;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Linq;
using Microsoft.Build.Utilities;
using System.Reflection;

namespace Erwine.Leonard.T.PsSolutionManager
{
    [Cmdlet(VerbsCommon.New, "MSBuildProject", DefaultParameterSetName = ParameterSetName_New)]
    [OutputType(typeof(ProjectRootElement))]
    public class New_MSBuildProject : Cmdlet
    {
        public const string DefaultValue_DefaultTargets = "Build";
        public const string DefaultValue_Configuration = "Release";
        public const string DefaultValue_Platform = "AnyCPU";
        public const string DefaultValue_OutputType = "Library";
        private string _defaultTargets = null;
        private string _configuration = DefaultValue_Configuration;
        private string _platform = DefaultValue_Platform;
        private string _outputType = DefaultValue_OutputType;
        private Guid? _projectGuid = null;
        private string _fileAlignment = null;
        private TargetDotNetFrameworkVersion? _targetFrameworkVersion = null;

        public const string ParameterSetName_New = "New";

        [Parameter(Mandatory = true, ParameterSetName = ParameterSetName_New)]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern(@"^[a-zA-Z_][a-zA-Z\d_]*(\.[a-zA-Z_][a-zA-Z\d_]*)*$")]
        public string RootNamespace { get; set; }

        [Parameter(ParameterSetName = ParameterSetName_New)]
        [ValidateNotNullOrEmpty()]
        [ValidatePattern(@"^[a-zA-Z_][a-zA-Z\d_]*(\.[a-zA-Z_][a-zA-Z\d_]*)*$")]
        public string AssemblyName { get; set; }

        [Parameter(ParameterSetName = ParameterSetName_New)]
        [ValidateNotNullOrEmpty()]
        public string ToolsVersion { get; set; }

        [Parameter(ParameterSetName = ParameterSetName_New)]
        [ValidateNotNullOrEmpty()]
        public string DefaultTargets
        {
            get { return _defaultTargets; }
            set
            {
                string s;
                if (value == null || (s = value.Trim()).Length == 0)
                    _defaultTargets = DefaultValue_DefaultTargets;
                else
                    _defaultTargets = s;
            }
        }

        [Parameter(ParameterSetName = ParameterSetName_New)]
        [ValidateNotNullOrEmpty()]
        public string Configuration
        {
            get { return _configuration; }
            set
            {
                string s;
                if (value == null || (s = value.Trim()).Length == 0)
                    _configuration = DefaultValue_Configuration;
                else
                    _configuration = s;
            }
        }

        [Parameter(ParameterSetName = ParameterSetName_New)]
        [ValidateNotNullOrEmpty()]
        public string Platform
        {
            get { return _platform; }
            set
            {
                string s;
                if (value == null || (s = value.Trim()).Length == 0)
                    _platform = DefaultValue_Platform;
                else
                    _platform = s;
            }
        }

        [Parameter(ParameterSetName = ParameterSetName_New)]
        [ValidateNotNullOrEmpty()]
        public string OutputType
        {
            get { return _outputType; }
            set
            {
                string s;
                if (value == null || (s = value.Trim()).Length == 0)
                    _outputType = DefaultValue_OutputType;
                else
                    _outputType = s;
            }
        }

        [Parameter(ParameterSetName = ParameterSetName_New)]
        [ValidateNotNullOrEmpty()]
        public Guid ProjectGuid
        {
            get
            {
                if (!_projectGuid.HasValue)
                    _projectGuid = Guid.NewGuid();
                return _projectGuid.Value;
            }
            set { _projectGuid = value; }
        }

        [Parameter(ParameterSetName = ParameterSetName_New)]
        [ValidateNotNullOrEmpty()]
        public TargetDotNetFrameworkVersion TargetFrameworkVersion
        {
            get
            {
                if (!_targetFrameworkVersion.HasValue)
                    _targetFrameworkVersion = TargetDotNetFrameworkVersion.VersionLatest;
                return _targetFrameworkVersion.Value;
            }
            set { _targetFrameworkVersion = value; }
        }

        [Parameter(ParameterSetName = ParameterSetName_New)]
        [AllowEmptyString()]
        [AllowNull()]
        public string FileAlignment
        {
            get
            {
                if (_fileAlignment == null)
                    _fileAlignment = "512";
                return _fileAlignment;
            }
            set { _fileAlignment = value ?? ""; }
        }

        [Parameter()]
        [AllowEmptyCollection()]
        public ProjectCollection Collection { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [ValidateNotNullOrEmpty()]
        public string Path { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Xml")]
        [ValidateXmlSource()]
        [ValidateNotNullOrEmpty()]
        public object Xml { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (Path != null)
                    WriteObject((Collection == null) ? ProjectRootElement.Create(Path) : ProjectRootElement.Create(Path, Collection), false);
                else if (Xml != null)
                    WriteObject((Collection == null) ? ProjectRootElement.Create(ValidateXmlSourceAttribute.AsXmlReader(Xml)) : ProjectRootElement.Create(ValidateXmlSourceAttribute.AsXmlReader(Xml), Collection), false);
                else
                {
                    ProjectRootElement project = (Collection == null) ? ProjectRootElement.Create() : ProjectRootElement.Create(Collection);
                    project.DefaultTargets = DefaultTargets;
                    project.AddImport("$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props").Condition = "Exists('$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props')";
                    ProjectPropertyGroupElement propertyGroup = project.AddPropertyGroup();
                    propertyGroup.Condition = "";
                    propertyGroup.AddProperty("Configuration", Configuration).Condition = " $(Configuration)' == '' ";
                    propertyGroup.AddProperty("Platform", Platform).Condition = " $(Configuration)' == '' ";
                    propertyGroup.AddProperty("ProjectGuid", ProjectGuid.ToString("B").ToUpper());
                    propertyGroup.AddProperty("OutputType", OutputType);
                    propertyGroup.AddProperty("RootNamespace", RootNamespace);
                    propertyGroup.AddProperty("AssemblyName", (String.IsNullOrWhiteSpace(AssemblyName)) ? RootNamespace : AssemblyName);
                    propertyGroup.AddProperty("TargetFrameworkVersion", ToolLocationHelper.GetDotNetFrameworkVersionFolderPrefix(TargetFrameworkVersion));
                    project.AddImport("$(MSBuildToolsPath)\\Microsoft.CSharp.targets");
                    WriteObject(project);
                    propertyGroup = project.AddPropertyGroup();
                    if (Configuration == "Debug" || Configuration == "Release")
                        propertyGroup.Condition = " '$(Configuration)|$(Platform)' == 'Debug|" + Platform + "' ";
                    else
                        propertyGroup.Condition = " '$(Configuration)|$(Platform)' == '" + Configuration + "|" + Platform + "' ";
                    propertyGroup.AddProperty("DebugSymbols", "true");
                    propertyGroup.AddProperty("DebugType", "full");
                    propertyGroup.AddProperty("Optimize", "false");
                    if (Configuration == "Debug" || Configuration == "Release")
                        propertyGroup.AddProperty("OutputPath", "bin\\" + Platform);
                    else
                        propertyGroup.AddProperty("OutputPath", "bin\\Debug");
                    propertyGroup.AddProperty("DefineConstants", "DEBUG;TRACE");
                    propertyGroup.AddProperty("ErrorReport", "prompt");
                    propertyGroup.AddProperty("WarningLevel", "4");
                    
                    propertyGroup = project.AddPropertyGroup();
                    propertyGroup.Condition = " '$(Configuration)|$(Platform)' == 'Release|" + Platform + "' ";
                    propertyGroup.AddProperty("DebugType", "pdbonly");
                    propertyGroup.AddProperty("Optimize", "true");
                    propertyGroup.AddProperty("OutputPath", "bin\\Release");
                    propertyGroup.AddProperty("DefineConstants", "TRACE");
                    propertyGroup.AddProperty("ErrorReport", "prompt");
                    propertyGroup.AddProperty("WarningLevel", "4");

                    ProjectItemGroupElement itemGroup = project.AddItemGroup();
                    itemGroup.AddItem("Reference", "System");
                    itemGroup.AddItem("Reference", "System.Core");
                    itemGroup.AddItem("Reference", "Microsoft.CSharp");
                    itemGroup.AddItem("Reference", "System.Xml");
                    Assembly assembly = (typeof(PSObject)).Assembly;
                    itemGroup.AddItem("Reference", assembly.FullName + ", processorArchitecture=MSIL",
                        new KeyValuePair<string, string>[]
                        {
                            new KeyValuePair<string, string>("SpecificVersion", "false"),
                            new KeyValuePair<string, string>("HintPath", assembly.Location)
                        });
                    assembly = (typeof(Microsoft.PowerShell.Commands.OutStringCommand)).Assembly;
                    itemGroup.AddItem("Reference", assembly.FullName + ", processorArchitecture=MSIL",
                        new KeyValuePair<string, string>[]
                        {
                            new KeyValuePair<string, string>("SpecificVersion", "false"),
                            new KeyValuePair<string, string>("HintPath", assembly.Location)
                        });
                    assembly = (typeof(Microsoft.PowerShell.Commands.GetItemCommand)).Assembly;
                    itemGroup.AddItem("Reference", assembly.FullName + ", processorArchitecture=MSIL",
                        new KeyValuePair<string, string>[]
                        {
                            new KeyValuePair<string, string>("SpecificVersion", "false"),
                            new KeyValuePair<string, string>("HintPath", assembly.Location)
                        });
                    WriteObject(project);
                }
            }
            catch (Exception exception)
            {
                WriteError(new ErrorRecord(exception, "New_MSBuildProject", ErrorCategory.OpenError, null));
            }
        }
    }
}