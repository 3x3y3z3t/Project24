
#### v-core6-0.4.0 (2023.08.27)
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

#### v-core6-0.3.0 (2023.06.28)
- Added Updater page.
- Added Localization.
- Added FileSystem service.
- Added Updater service.
- Added Bootstrap icon library (used icons only).
- Added Two Buttons Common modal.

#### v-core6-0.2.1 (2023.05.16)
- Added Modal icons.
- Fixed unhandled exception in Changelog page.

#### v-core6-0.2.0 (2023.05.16)
- Added License.
- Added `Markdig.Signed` package for Markdown parser (unused).
- Added Changelog page.
- Added Common Modal.
- Renamed "Release Note.md" file to "Changelog.md".
- Updated Layout.

#### v-core6-0.1.0 (2023.04.09)
- Initialize .NET Core 6 project.
