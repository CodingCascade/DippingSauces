# DippingList
A local Razor Pages practice app built with C# and Copilot help. It's a learning app for keeping a dipping-list shopping list. It supports duplicate names, purchased status, and removing individual items.
 
## Requirements
Require Windows, compatible Visual Studio, and the.NET 10 SDK.
 
## Run locally
Open the solution in Visual Studio. Restore packages, build, and launch the web project with HTTP.
The configured HTTP launch profile uses `http://localhost:5084`.
 
## Test
Open Test Explorer and run all tests. The test project references the DippingList web project. The latest Visual Studio Test Explorer results show **17 tests passed**. Tests cover item validation and duplicates, purchased-state updates, removal, missing files, restoring saved data, invalid JSON, and failed saves.
 
## Data
The app stores one shared list in App_Data/shopping-list.json. Keep it local. There are no separate user accounts. Back up data before experiments.

## Shared list and storage
The app uses a singleton service, so requests to the same running app instance use the same list. It is a shared list, not a separate list for each user. List data is saved as JSON at `DippingList\App_Data\shopping-list.json`, relative to the app’s content root. The app loads it on startup and saves changes. If the file does not exist, the list starts empty. The JSON file is local to that app instance; this setup does not provide shared storage across multiple app instances.

## Limits
One application process, local JSON, no public deployment. Full-page updates do not restore focus to the changed row. This project is for local learning; it makes no claim of public hosting or production readiness.
