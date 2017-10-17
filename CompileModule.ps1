$MSBuild = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory() | Join-Path -ChildPath 'MSBuild.exe';
if (-not ($MSBuild | Test-Path -PathType Leaf)) {
    Write-Warning -Message "Unable to build project: MSBuild.exe not found in $([System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()).";
    return;
}

$ErrorsPath = [Guid]::NewGuid().ToString('N');
$WarningsPath = $PSScriptRoot | Join-Path -ChildPath "BuildWarnings_$ErrorsPath.txt";
$ErrorsPath = $PSScriptRoot | Join-Path -ChildPath "BuildErrors_$ErrorsPath.txt";

$BuildSwitches = ("/flp1:errorsonly;logfile=$ErrorsPath", "/flp2:warningsonly;logfile=$WarningsPath");
$ProjFile = $PSScriptRoot | Join-Path -ChildPath 'PSDev.csproj'

# C:\
# http://user:pw@hostname/
# [^:\\/]+\:(//([^@:]*(:[^@]*)?@)?[^:\\/]+(:\d+)?)?
# \\machine\share\
# [\\/]{2}[^:\\/]+[\\/][^:\\/]+
# C:
# (?<f>[^()]+(\((?=\d*[^\d,]|\d+,\d*[^\d,)])[^(]*)*)\((?<l>\d+),(?<c>\d+)\):\s*(?<t>error|warning)\s+(?<n>[a-z]+\d+):\s+(?<m>.*)$
# XmlHelper.cs(5,26): error CS0246: The type or namespace name 'XmlWriterSettings' could not be found (are you missing a using directive or an assembly reference?) [C:\LennyTemp\SourceCode\PSDev\PSDev.csproj]
#C:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\Microsoft.Common.targets(983,5): warning MSB3644: The reference assemblies for framework ".NETFramework,Version=v4.5" were not found. To resolve this, install the SDK or Targeting Pack for 
#this framework version or retarget your application to a version of the framework for which you have the SDK or Targeting Pack installed. Note that assemblies will be resolved from the Global Assembly Cache (GAC) and will be used in plac
#e of reference assemblies. Therefore your assembly may not be correctly targeted for the framework you intend. [C:\LennyTemp\SourceCode\PSDev\PSDev.csproj]
#

$MessageRegex = [System.Text.RegularExpressions.Regex]::new('(?<f>[^()]+(\((?=\d*[^\d,]|\d+,\d*[^\d,)])[^(]*)*)\((?<l>\d+),(?<c>\d+)\):\s*(?<t>error|warning)\s+(?<n>[a-z]+\d+):\s+(?<m>.*)$',
    ([System.Text.RegularExpressions.RegexOptions]::Compiled -bor [System.Text.RegularExpressions.RegexOptions]::IgnoreCase));

$ErrorLines = @();
$WarningLines = @();
$Success = $false;
$Old_VerbosePreference = $VerbosePreference;
$Old_WarningPreference = $WarningPreference;
$Old_ErrorActionPreference = $ErrorActionPreference;
$VerbosePreference = [System.Management.Automation.ActionPreference]::Continue;
$WarningPreference = [System.Management.Automation.ActionPreference]::Continue;
$ErrorActionPreference = [System.Management.Automation.ActionPreference]::Stop;
$VerboseLines = @();
try {
    $VerboseLines = @(& $MSBuild $BuildSwitches $ProjFile | ForEach-Object {
        Write-Verbose -Message $_;
        $_ | Write-Output;
    });
    if ($ErrorsPath | Test-Path -PathType Leaf) {
        $ErrorLines = @(Get-Content -LiteralPath $ErrorsPath);
        Remove-Item -LiteralPath $ErrorsPath;
    }
    if ($WarningsPath | Test-Path -PathType Leaf) {
        $WarningLines = @(Get-Content -LiteralPath $WarningsPath);
        Remove-Item -LiteralPath $WarningsPath;
    }

    $AllErrors = @();
    if ($ErrorLines.Count -gt 0) {
        $AllErrors = @($ErrorLines | ForEach-Object {
            $m = $MessageRegex.Match($_);
            if ($m.Success) {
                New-Object -TypeName 'System.Management.Automation.PSObject' -Property @{
                    File = $m.Groups['f'].Value;
                    Line = [int]::Parse($m.Groups['l'].Value);
                    Column = [int]::Parse($m.Groups['c'].Value);
                    Type = $m.Groups['t'].Value;
                    Number = $m.Groups['n'].Value;
                    Message = $m.Groups['m'].Value;
                };
            } else {
                New-Object -TypeName 'System.Management.Automation.PSObject' -Property @{
                    File = '';
                    Line = -1;
                    Column = -1;
                    Type = '';
                    Number = '';
                    Message = $_;
                };
            }
            $Host.UI.WriteErrorLine("$($_.ToString().Trim())`r`n");
        });
    }

    if ($WarningLines.Count -gt 0) {
        $AllErrors = $AllErrors + @($WarningLines | ForEach-Object {
            $m = $MessageRegex.Match($_);
            if ($m.Success) {
                New-Object -TypeName 'System.Management.Automation.PSObject' -Property @{
                    File = $m.Groups['f'].Value;
                    Line = [int]::Parse($m.Groups['l'].Value);
                    Column = [int]::Parse($m.Groups['c'].Value);
                    Type = $m.Groups['t'].Value;
                    Number = $m.Groups['n'].Value;
                    Message = $m.Groups['m'].Value;
                };
            } else {
                New-Object -TypeName 'System.Management.Automation.PSObject' -Property @{
                    File = '';
                    Line = -1;
                    Column = -1;
                    Type = '';
                    Number = '';
                    Message = $_;
                };
            }
            $Host.UI.WriteWarningLine("$($_.ToString().Trim())`r`n");
        });
    } else {
        if ($ErrorLines.Count -eq 0) {
            Write-Verbose -Message "Completed with no errors or warnings";
        }
    }
    if ($AllErrors.Count -gt 0) {
        if ($ErrorLines.Count -gt 0) {
            if ($WarningLines.Count -gt 0) {
                $AllErrors | Out-GridView -Title 'Errors and Warnings' -Wait;
            } else {
                $AllErrors | Out-GridView -Title 'Errors' -Wait;
            }
        } else {
            if ($WarningLines.Count -gt 0) {
                $AllErrors | Out-GridView -Title 'Warnings' -Wait;
            }
        }
    }
} finally {
    $VerbosePreference = $Old_VerbosePreference;
    $WarningPreference = $Old_WarningPreference;
    $ErrorActionPreference = $Old_ErrorActionPreference;
}