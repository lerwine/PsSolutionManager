if ($BuildTypes -eq $null) {
    $BuildTypes = @([System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory() | Get-ChildItem -Filter '*.dll' | ForEach-Object {
        $n = $null;
        try { $n = [System.Reflection.AssemblyName]::GetAssemblyName($_.FullName) } catch { }
        if ($n -ne $null -and $n.Name.StartsWith('Microsoft.Build')) {
		    New-Object -TypeName 'System.Management.Automation.PSObject' -Property @{ Name = $n; Types = @(Add-Type -LiteralPath $_.FullName -PassThru) }
	    }
    });
}


@($BuildTypes | ForEach-Object { $_.Types | ForEach-Object { if ($_.IsPublic) { $_.GetMethods() | Where-Object { $_.Name.Contains('Eval') } } } }) | Out-GridView
@($BuildTypes | ForEach-Object {
    $n = $_.Name.ToString();
    $_.Types | ForEach-Object {
        if ($_.IsPublic) {
            $BaseType = '?';
            if ($_.IsEnum) {
                $BaseType = 'enum';
            } else {
                if ($_.IsValueType) {
                    $BaseType = 'struct';
                } else {
                    if ($_.IsAbstract) {
                        $BaseType = "abstract $($_.BaseType.FullName)";
                    } else {
                        if ($_.IsInterface) {
                            $BaseType = "interface";
                        } else {
                            if ($_.IsClass) { $BaseType = $_.BaseType.FullName }
                        }
                    }
                }
            }
            
            New-Object -TypeName 'System.Management.Automation.PSObject' -Property @{ Assembly = $n; FullName = $_.FullName; BaseType = $BaseType; Interfaces = ($_.GetInterfaces() | ForEach-Object { $_.FullName } | Out-String).Trim() }
        }
    }
}) | Sort-Object 'Assembly', 'FullName' | Out-GridView -Title 'MSBuild Types';

#v4.0
$FrameworkPath = [Microsoft.Build.Utilities.ToolLocationHelper]::GetPathToDotNetFramework([Microsoft.Build.Utilities.TargetDotNetFrameworkVersion]::Version45);
#C:\Windows\Microsoft.NET\Framework64\v4.0.30319
$ProcessorArchitecture = [Microsoft.Build.Utilities.ProcessorArchitecture]::CurrentProcessArchitecture;
#AMD64

$NewProject.Save($TargetProjectPath);