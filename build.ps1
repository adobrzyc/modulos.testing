<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

 
 if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

exec { & dotnet restore }

exec { & dotnet build  .\src\Modulos.Testing\Modulos.Testing.csproj -c Release}
exec { & dotnet build  .\src\Modulos.Testing.Db\Modulos.Testing.Db.csproj -c Release}
exec { & dotnet build  .\src\Modulos.Testing.EF\Modulos.Testing.EF.csproj -c Release}

exec { & dotnet pack .\src\Modulos.Testing\Modulos.Testing.csproj -c Release -o .\artifacts }
exec { & dotnet pack .\src\Modulos.Testing.Db\Modulos.Testing.Db.csproj -c Release -o .\artifacts }
exec { & dotnet pack .\src\Modulos.Testing.EF\Modulos.Testing.EF.csproj -c Release -o .\artifacts }