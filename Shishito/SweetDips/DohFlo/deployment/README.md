# Deploy DohFlo to SmarterASP.NET

The repository-root `.github/workflows/dohflo.yml` builds the Angular dashboard,
copies its generated assets and manifest into the MVC app, builds .NET 10,
runs the existing xUnit tests, and publishes a self-contained Windows x64 app.
Out-of-process IIS hosting avoids depending on the application pool's bitness.

Pushes and pull requests affecting DohFlo run build/test only. Deployment runs
only from `master` when manually requested. No database migrations or restores
run during deployment; the existing hosted tables and data must already exist.

## Repository secrets

Under Settings > Secrets and variables > Actions, add:

| Name | Value |
| --- | --- |
| `DOHFLO_DEPLOY_URL` | Complete HTTPS Service URL from the site's VSDeploy panel |
| `DOHFLO_DEPLOY_SITE` | `booliekets-001-site1` |
| `DOHFLO_DEPLOY_USERNAME` | Hosting Web Deploy username |
| `DOHFLO_DEPLOY_PASSWORD` | Hosting Web Deploy password |
| `DOHFLO_DB_CONNECTION` | SQL Server connection string for `sql5111.site4now.net`, database `db_ad104c_dohflo`, with SQL login credentials |

Keep passwords out of commits, screenshots, and issue comments. A password with
a semicolon must be quoted correctly within a SQL connection string. Use the
host's connection-string template when available. The SQL connection check
requires encryption and certificate validation; it never bypasses TLS errors.

## Run

1. Merge the workflow into `master`.
2. Open Actions > DohFlo build and deploy > Run workflow.
3. Select `master` and check the deploy option to deploy. Leave it unchecked
   for a build/test run only.
4. Open the workflow run to see its progress.
5. After success, visit `https://booliekets-001-site1.etempurl.com` and test login,
   finance pages, and `/dashboard/` with the existing imported app accounts.

The deployment step first verifies database login and required imported tables.
SQL error 18456 means the login credentials need correcting. Update the database
secret rather than guessing repeatedly. Network and TLS failures can also stop
this check; do not assume every connection error means the password is wrong.

Production configuration is generated only in the temporary publish folder,
deployed over HTTPS, then removed from the runner. It is never uploaded as an
Actions artifact. Bootstrap stays disabled because app users were imported.
Only the named site's published files are synchronized. Extra destination files
are retained, and the AppOffline rule releases application file locks.

When troubleshooting a Web Deploy failure, check the endpoint and credentials
in the hosting panel. The script intentionally suppresses raw exception text to
avoid leaking secrets. If files remain locked, use Recycle in the website panel
and retry. A failed deployment can leave the site partially updated; rerun the
same tested revision after resolving the error. This is not an atomic release
or automatic rollback system.
