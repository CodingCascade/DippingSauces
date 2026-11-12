# Run with Windows PowerShell 5.1 on the Windows GitHub runner.
# Credentials are passed as environment variables, never command arguments.
param([Parameter(Mandatory = $true)][string]$PublishDirectory)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$stage = 'validate deployment settings'
$connection = $null
$command = $null
$source = $null
$productionConfig = Join-Path $PublishDirectory 'appsettings.Production.json'

try {
    foreach ($name in @('DOHFLO_DEPLOY_URL', 'DOHFLO_DEPLOY_SITE', 'DOHFLO_DEPLOY_USERNAME', 'DOHFLO_DEPLOY_PASSWORD', 'DOHFLO_DB_CONNECTION')) {
        if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
            throw "Missing repository secret: $name"
        }
    }
    $endpoint = [Uri]$env:DOHFLO_DEPLOY_URL
    if ($endpoint.Scheme -ne 'https' -or $endpoint.DnsSafeHost -ne 'win8129.site4now.net' -or $endpoint.Port -ne 8172) {
        throw 'Unexpected deployment endpoint. Check the VSDeploy service URL.'
    }
    if ($env:DOHFLO_DEPLOY_SITE -ne 'booliekets-001-site1') {
        throw 'Unexpected deployment site.'
    }
    foreach ($file in @('DohFlo.exe', 'web.config', 'DashboardAssets/dashboard-assets.json')) {
        if (-not (Test-Path (Join-Path $PublishDirectory $file))) {
            throw "Published file is missing: $file"
        }
    }

    # Windows PowerShell includes the .NET Framework SQL client. This is a
    # read-only login/schema check, not a schema migration or database restore.
    $stage = 'connect to the hosted database'
    Add-Type -AssemblyName System.Data
    $settings = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $settings.ConnectionString = $env:DOHFLO_DB_CONNECTION
    if ($settings.DataSource -ne 'sql5111.site4now.net' -or $settings.InitialCatalog -ne 'db_ad104c_dohflo') {
        throw 'The database secret must target the hosted DohFlo database.'
    }
    $settings.Encrypt = $true
    $settings.TrustServerCertificate = $false
    $settings.ConnectTimeout = 20
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $settings.ConnectionString
    $connection.Open()
    $stage = 'verify imported database tables'
    $command = $connection.CreateCommand()
    $command.CommandTimeout = 20
    $command.CommandText = @'
SELECT COUNT(*) FROM sys.tables
WHERE schema_id = SCHEMA_ID(N'dbo')
AND name IN (N'Accounts', N'Categories', N'Payees', N'Transactions', N'Users', N'__EFMigrationsHistory');
'@
    if ([int]$command.ExecuteScalar() -ne 6) {
        throw 'Required imported tables are missing.'
    }
    $connection.Close()
    Write-Host 'Database login and imported table check passed.'

    # Only the deployment step creates this file. It is never committed,
    # cached, or uploaded as a workflow artifact.
    $stage = 'write production configuration'
    $config = @{
        ConnectionStrings = @{ DohFloDb = $settings.ConnectionString }
        Bootstrap = @{ Enabled = $false }
    } | ConvertTo-Json -Depth 4
    [IO.File]::WriteAllText($productionConfig, $config, (New-Object Text.UTF8Encoding($false)))

    $stage = 'load Microsoft Web Deploy'
    $assembly = @(
        (Join-Path $env:ProgramFiles 'IIS/Microsoft Web Deploy V3/Microsoft.Web.Deployment.dll'),
        (Join-Path ${env:ProgramFiles(x86)} 'IIS/Microsoft Web Deploy V3/Microsoft.Web.Deployment.dll')
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1
    if ($null -eq $assembly) {
        throw 'Microsoft Web Deploy is not installed on this runner.'
    }
    Add-Type -Path $assembly
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    $destination = New-Object Microsoft.Web.Deployment.DeploymentBaseOptions
    $destination.ComputerName = $env:DOHFLO_DEPLOY_URL
    $destination.UserName = $env:DOHFLO_DEPLOY_USERNAME
    $destination.Password = $env:DOHFLO_DEPLOY_PASSWORD
    $destination.AuthenticationType = 'Basic'
    $destination.IncludeAcls = $false
    $sync = New-Object Microsoft.Web.Deployment.DeploymentSyncOptions
    $sync.DoNotDelete = $true
    $offlineFound = $false
    foreach ($rule in [Microsoft.Web.Deployment.DeploymentSyncOptions]::GetAvailableRules()) {
        if ($rule.Name -eq 'AppOffline') {
            $sync.Rules.Add($rule)
            $offlineFound = $true
        }
    }
    if (-not $offlineFound) { throw 'The AppOffline deployment rule is missing.' }
    $stage = 'upload the published application with Web Deploy'
    $source = [Microsoft.Web.Deployment.DeploymentManager]::CreateObject('contentPath', $PublishDirectory)
    $null = $source.SyncTo([Microsoft.Web.Deployment.DeploymentWellKnownProvider]::ContentPath, $env:DOHFLO_DEPLOY_SITE, $destination, $sync)
    Write-Host 'Web Deploy completed. Test the temporary HTTPS site and sign in with your existing app account.'
}
catch {
    # Exception messages may include connection strings or endpoint credentials.
    # Report the stage and SQL error number, never the complete exception.
    $sqlError = $_.Exception
    while ($null -ne $sqlError -and -not ($sqlError -is [System.Data.SqlClient.SqlException])) {
        $sqlError = $sqlError.InnerException
    }
    Write-Host "::error::DohFlo could not $stage. Deployment stopped."
    if ($null -ne $sqlError) {
        Write-Host "SQL error number: $($sqlError.Number)."
        if ($sqlError.Number -eq 18456) {
            Write-Host 'Database login failed. Verify/reset the database password and update DOHFLO_DB_CONNECTION.'
        }
    }
    exit 1
}
finally {
    if ($null -ne $source) { $source.Dispose() }
    if ($null -ne $command) { $command.Dispose() }
    if ($null -ne $connection) { $connection.Dispose() }
    if (Test-Path $productionConfig) { Remove-Item $productionConfig -Force }
}
