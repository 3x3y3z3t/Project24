
### v-core6-0.9.1 (2023.10.29)
- Updated Index page. Index page is now a proper landing page.
- Updated Request Pipeline to force log user out after their role got changed.
- Updated some pages to enforce authorization.
- Updated Access Denied page with link to previous page.
- Updated Locale File Validator in AppHelper to handle .js files.
- Updated Locale File reader utility in AppHelper to handle .js files.

### v-core6-0.9.0 (2023.10.15)
- Added user authentication (powered by Identity Framework).
- Added some role names for specific pages.
- Added landing page (`Index` pages).
- Added `PageCollection` that stores all pages' path.
- Added `UserAction` to record user's action.
- Updated some pages to enforce authorization.
- Updated Localization functionality: Project24 now display localized contents based on User preference > Local settings > App default.
- Updated `LocalizationSvc` to take advantage of AppHelper's Locale File reader.
- Added Locale File Validator in AppHelper.
- Added Locale File reader utility in AppHelper.

### v-core6-0.8.0 (2023.10.02)
- Added Simulator > FinancialManagement Remove page.
- Added Simulator > FinancialManagement Import functionality.
- Added `P24Stopwatch` which has extra functionality over `System.Diagnostics.Stopwatch`.
- Added `Syncable` which define object that can be synchronized (import from other source).
- Added helper methods for Syncable in `ApplicationDbContext` (basically overrides of DBContext's Add and Update methods).
- Added `DBMaintenanceSvc` to perform various db maintenance task, include locking db during import.

### v-core6-0.7.0 (2023.09.21)
- Added file logger.
- Added Simulator > FinancialManagement simulator.
- Updated Updater's front-end script and back-end services.
- Removed `ServerAnnouncementIncludePageModel` due to obsolete (replaced with Server Announcement hosted service).
- Removed `svgs.js` due to obsolete (replaced with `P24Utils.svg()` in `site.js`.
- Updated `modal.js` to make use of new svg construction.
- Fixed crashes on `FileSystemSvc`'s `DeleteFiles()` and `CopyFiles()` when supplied File path instead of Directory path.
- Fixed racing issue on `UpdaterSvc`'s `Update()`.
- Fixed various parsing and formatting bugs in `dotnet.js`.

### v-core6-0.6.0 (2023.09.18)
- Added Server Announcement.
- Added various .NET object clone in Javascript.
- Added `IProject24HostedService` interface.
- Updated Updater Service to make used of Server Announcement.
- Moved cyrb53 hashing function to `site.js` to reduce the amount of script files sent to client.

### v-core6-0.5.0 (2023.09.02)
- Added Config Panel.
- Added User upload data logger.
- Added Localization support (both front-end and back-end), currently supports Global (app-wide) localization only.
- Added `Trackable` (to be used in place of `InternalState`).
- Added `TrackableMetadata`.
- Updated Updater function to use User upload data logger.
- Updated `InternalTrackerSvc`: Added init function.
- Removed `InternalState`.
- Fixed Updater front-end bug in which batches info tag should not display when there is no file to be upload.

### v-core6-0.4.0 (2023.08.27)
- Added MySQL Database.
- Added Updater function (for updating without manually copying files to server).
- Added Rollback function (for rolling back *one* previous version).
- Added Setup funtion in AppHelper.
- Added `DBMaintenanceSvc` for cleaning up duplication in database.
- Added `InternalTrackerSvc` for tracking app's internal states.
- Added `TrackableSvc` which is used for services with states require tracking.
- Added `InternalState`.
- Added `ServerAnnouncementIncludedPageModel` for Server Announcement (barebone).
- Added `P24JsonSerializerContext` in AppHelper.
- Added `SystemCaller` in AppHelper.
- Added `DeleteFiles()` in `FileSystemSvc`.
- Updated Bootstrap icon library (now use full library);
- Minor fixes for `FileSystemSvc`.
- Minor fixes for Updater front-end script (js).

### v-core6-0.3.0 (2023.06.28)
- Added Updater page.
- Added Localization.
- Added FileSystem service.
- Added Updater service.
- Added Bootstrap icon library (used icons only).
- Added Two Buttons Common modal.

#### v-core6-0.2.1 (2023.05.16)
- Added Modal icons.
- Fixed unhandled exception in Changelog page.

### v-core6-0.2.0 (2023.05.16)
- Added License.
- Added `Markdig.Signed` package for Markdown parser (unused).
- Added Changelog page.
- Added Common Modal.
- Renamed "Release Note.md" file to "Changelog.md".
- Updated Layout.

### v-core6-0.1.0 (2023.04.09)
- Initialize .NET Core 6 project.
