Function New-CsSourceCodeProject {
    [OutputType([Microsoft.Build.Evaluation.Project])]
    Param(
        [Parameter(Mandatory = $true)]
        [ValidatePattern('^[a-zA-Z_][a-zA-Z\d_]*(\.[a-zA-Z_][a-zA-Z\d_]*)*$')]
        [string]$AssemblyName,

        [string]$ToolsVersion,

        [string]$DefaultTargets = 'Build',
        
        [string]$Configuration = 'Debug',
        
        [string]$Platform = 'AnyCPU',
        
        [string]$OutputType = 'Library',
        
        [ValidatePattern('^[a-zA-Z_][a-zA-Z\d_]*(\.[a-zA-Z_][a-zA-Z\d_]*)*$')]
        [string]$RootNamespace,

        [Guid]$ProjectGuid,

        [Microsoft.Build.Utilities.TargetDotNetFrameworkVersion]$TargetFrameworkVersion = [Microsoft.Build.Utilities.TargetDotNetFrameworkVersion]::VersionLatest,
        
        [string]$FileAlignment = '512'
    )
    $ProjectRootElement = [Microsoft.Build.Construction.ProjectRootElement]::Create();
    $ProjectRootElement.DefaultTargets = 'Build';
    $ProjectRootElement.AddImport('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props').Condition = 'Exists(''$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props'')';
    [Microsoft.Build.Construction.ProjectPropertyGroupElement]$ProjectPropertyGroupElement = $ProjectRootElement.AddPropertyGroup();
    $ProjectPropertyGroupElement.Condition = '';
    $ProjectPropertyGroupElement.AddProperty('Configuration', $Configuration).Condition = ' ''$(Configuration)'' == '''' ';
    $ProjectPropertyGroupElement.AddProperty('Platform', $Platform).Condition = ' ''$(Configuration)'' == '''' ';
    if ($PSBoundParameters.ContainsKey('ProjectGuid')) {
        $ProjectPropertyGroupElement.AddProperty('ProjectGuid', $ProjectGuid.ToString().ToUpper()) | Out-Null;
    } else {
        $ProjectPropertyGroupElement.AddProperty('ProjectGuid', [Guid]::NewGuid().ToString().ToUpper()) | Out-Null;
    }
    $ProjectPropertyGroupElement.AddProperty('OutputType', $OutputType) | Out-Null;
    if ($PSBoundParameters.ContainsKey('RootNamespace')) {
        $ProjectPropertyGroupElement.AddProperty('RootNamespace', $RootNamespace) | Out-Null;
    } else {
        $ProjectPropertyGroupElement.AddProperty('RootNamespace', $AssemblyName) | Out-Null;
    }
    $ProjectPropertyGroupElement.AddProperty('AssemblyName', $AssemblyName) | Out-Null;
    $ProjectPropertyGroupElement.AddProperty('TargetFrameworkVersion', [Microsoft.Build.Utilities.ToolLocationHelper]::GetDotNetFrameworkVersionFolderPrefix($TargetFrameworkVersion)) | Out-Null;
    $ProjectRootElement.AddImport('$(MSBuildToolsPath)\Microsoft.CSharp.targets') | Out-Null;
    [Microsoft.Build.Evaluation.Project]::new($ProjectRootElement);
}
