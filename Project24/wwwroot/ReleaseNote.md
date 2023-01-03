
#### v0.13.2 (2023.01.03)
- Added Inventory Drug Import note listing (Clinic Manager side).
- Fool-proof'd Inventory Drug Import note creation (Clinic Manager side).

#### v0.13.1 (2023.01.02)
- Added Symptom in Ticket Profile (Clinic Manager side).
- Various (minor) fixed for Clinic Manager.

#### v0.13.0 (2022.12.31)
- Added Inventory tracking for Clinic Manager.
- Added object version tracker (for future data restoration function).

#### v0.12.7 (2022.12.25)
- Minor QoL update for NAS.

#### v0.12.6 (2022.12.24)
- Added Quick Update feature (update static files only, no downtime).
- Added NAS Upload Folder upload capability.
- Fixed NAS nav bar.
- Fixed NAS Upload not writing files from cache to disk (2).
- Hotfix .5: Fixed NAS Upload not displaying file list correctly (visual only).
- Hotfix .6: Fixed NAS Upload not saving file in correct location.

#### v0.12.3 (2022.12.18)
- Fixed some URL issues with NAS.
- Hotfix .3: Fixed NAS Upload not writing file from cache to disk.

#### v0.12.1 (2022.12.17)
- Reworked NAS.
- Fixed error with special characters in upload file name.
- Added user upload tracking. Now every bytes you have uploaded will be counted.
- Hotfix .1: Disabled NAS resume upload.

---

#### v0.11.1 (2022.12.13)
- Added Profile Changelog (Customer Profile, Ticket Profile) (Clinic Manager side).

#### v0.11.0 (2022.12.12)
- Updated User Account management.

---

#### v0.10.3 (2022.12.10)
- Reworked Updater (Home side).

#### v0.10.2-beta (2022.12.04)
- Reworked Clinic Manager (Customer).
- Added Visiting Ticcket (Clinic Manager side).

---

#### v0.9.1 (2022.11.15)
- Added self-contained update feature.
- Updated Index page.
- Fixed issue with Search function in Clinic Manager index page.

---

#### v0.8.6 (2022.10.29)
- Added Search feature in Clinic Manager index page.

#### v0.8.5 (2022.10.29)
- Added logging to file.
- Fixed some crash when moving uploaded file from cache to disk.

#### v0.8.4 (2022.10.27)
- Added statistic in NAS Upload (Elapsed time, Avg speed).
- Update server file upload location to (hopefully) improve upload speed (previously 907KB/s, now 1.11MB/s, increased by 22%).

#### v0.8.3 (2022.10.26)
- Added Delete File/Folder in NAS Upload.

#### v0.8.2 (2022.10.24)
- Added Create Folder in NAS Upload.

#### v0.8.1 (2022.10.21)
- Restructured data directories.
- Minor fixes on Clinic Manager side.

---

#### v0.7.5-alpha (2022.10.19)
- Reworked NAS Browser function.
- Reworked NAS Download function.
- Minor refactor.

#### v0.7.3 (2022.10.18)
- Update NAS Upload function using [tus protocol](https://tus.io/). Currently every file is uploaded to root directory.
- Added NAS Tester user for NAS upload function testing. Default username is `nas-tester`, password is `nas-tester1`.

---

#### v0.6.13 (2022.10.16)
- Updated Release Notes fetching logic on About page.

#### v0.6.11 (2022.10.16)
- Updated static content on About page.

#### v0.6.10 (2022.10.15)
- Minor cleanup.

#### v0.6.9 (2022.10.15)
- Added **Clinic Manager** (basic).
- Added About page.
