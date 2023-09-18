# Project24
This project is an "app bundle" consist of a Clinic Manager app (Project24), a NAS (Project24b) and a personal blog (Project24c).

## This branch `core-6`
This is the new version of Project24 written in ASP .NET Core 6.

The project is rewritten from scratch with slightly different design, and is not compatible with the stable version (.NET Core 3.1).
Program will crash and data will be lose when using interchangeably.

When this branch is stable enough, it will replace replace the stable branch.

## Version
- Project24: v.core6-0.6.0
- AppHelper: v.core6-1.1.0


## Installation (Linux - ARM x64)
### Prerequisite
You can create an user with root privilege dedicated for running Project24.

### Nginx
You can install nginx using any tutorial available.

### MySQL
You can install MySQL using any tutorial available.

### Build Project24
1. Build Project24 in Visual Studio.
2. Create a publish in Package Manager Console in Visual Studio.
    ```
    dotnet publish -r linux-arm64
    ```
3. Copy the whole "publish" folder to the target directory. This is where the application will run from.
4. Set executable's attribute.
    ```
    > chmod 777 ./AppHelper
    ```
    (If you want to run Project24 right now, you have to set `Project24` executable's attribute to `777` as well.)

### Project24 setup and configuration
1. Run AppHelper with argument `--setup` under sudo privilege.
    ```
    > sudo ./AppHelper --setup
    ```
    Alternatively, run with argument `--setup quiet` to run quietly (there will be no prompt for file overwriting).
    ```
    > sudo ./AppHelper --setup quiet
    ```
    AppHelper will write the following file to system directory:
    - Nginx's configuration file and symbolic link
    - systemd service files

    so running without `sudo` will cause permission error.
2. Change database root user and app's Power user credentials.
    ```
    > sudo nano appsettings.Production.json
    ```
    The relevant part looks like this. These are default credentials, which you need to change.
    ```json
    "Credentials": {
      "DBCredential": {
        "Username": "root",
        "Password": "12345@Aa"
      },
      "PowerUser": {
        "Username": "power",
        "Password": "12345@Aa"
      }
    },
    ```
3. Restart Project24 service (if needed, see details in "Running Project24" and "Troubleshoot" section below).
    ```
    > sudo system restart kestrel-p24-core6-main
    ```

### Running Project24
After generating the required files, AppHelper will start the service for Project24, so it should start automatically.

However Project24 will try to connect to database using default credential, and should fail (because your db credential should be different from our default one), which cause Project24 to crash.

Systemd will try to restart the program after 10s, the program start, tries connecting to db then fail, then crash again.

When you are done with `appsettings` configuration, Project24 should be able to connect to db, succesfullt starting up and listening to request. **Setup Done**

### Troubleshoot
If Project24 still can not start up in more than 10s after you have done with `appsettings`, first make sure that your configured database credential is correct.
Then, restart the systemd service using the command above.

## Known Issues
#### Issues (intentional, won't fix)
These issues are in internal logic, so on normal working condition there is no way fo them to go wrong.
- `Updater` accepts all files uploaded without verify their purpose.
- `ServerAnnouncementSvc` will crash if json string or ini string loading from database is malformatted.

#### Issues (will fix)
These issues are being investigated and will be fixed in a future patch.
- `UpdaterSvc` doesn't kill `prev` app instance after finishing updating (Linux).
