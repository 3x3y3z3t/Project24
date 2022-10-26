/*  nas-utils.js
 *  Version: 1.0 (2022.10.26)
 *
 *  Contributor
 *      Arime-chan
 */

function browseNas(_path) {
    //window.alert(_path);
    $("#io-operation-status").html("Requesting NAS Browser..");

    if ($("#upload-mode").html() != null) {
        $.ajax({
            type: 'GET',
            url: _path,
            success: function (_content) {
                $("#nas-browser").html(_content);

                let newLocation = "root/" + $("#current-location").html();
                //window.alert(newLocation);
                $("#upload-location").html(newLocation);
            },
            error: function () {
                window.alert("Could not fetch browser.");
            }
        });
    } else {
        window.location.href = _path;
    }
}

function createFolder() {
    let location = $("#current-location").html();
    let folderName = $("#input-new-folder-name").val();

    let text = "Location: root/" + location + "\nNew Folder: '" + folderName + "'\n\nAre you sure you want to create this folder?"

    if (confirm(text) == true) {
        sendCreateFolderRequest(location, folderName);
    } else {

    }
}

function copyFile(_path) {

}

function deleteFile(_path, _fileType) {
    let pos = _path.lastIndexOf("/");
    let location = _path.substring(0, pos);
    let folderName = _path.substring(pos + 1);

    let text = "Location: root/" + location + "\n" + _fileType + ": '" + folderName + "'\n\nAre you sure you want to delete this ";
    if (_fileType == "Folder")
        text += "folder?";
    else if (_fileType == "File")
        text += "file?";

    if (confirm(text) == true) {
        sendDeleteFileRequest(_path)
    } else {

    }
}

function sendCreateFolderRequest(_path, _folderName) {
    var token = $("input[name='__RequestVerificationToken']").val();

    $("#io-operation-status").html("Creating folder..");

    $.ajax({
        type: 'POST',
        url: '/Nas/CreateDirectory',
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify({
            ParentDir: _path,
            FolderName: _folderName
        }),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: function (_content) {
            $("#nas-browser").html(_content);

            console.log("CreateDirectory returns " + _returnMsg)
            //location.reload();
        },
        error: function () {
            window.alert("Could not fetch browser.");
        }
    });
}

function sendCopyFileToRequest(_src, _dst) {

}

function sendDeleteFileRequest(_path) {
    var token = $("input[name='__RequestVerificationToken']").val();

    $("#io-operation-status").html("Deleting file/folder..");

    $.ajax({
        type: 'POST',
        url: '/Nas/Delete',
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify({
            File: _path,
        }),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: function (_content) {
            $("#nas-browser").html(_content);

            console.log("Delete returns " + _returnMsg)
            //location.reload();
        },
        error: function () {
            window.alert("Could not fetch browser.");
        }
    });
}

function updateUploadLocation() {
    let newLocation = "root/" + $("#current-location").html();
    $("#upload-location").html(newLocation);
    //window.alert(newLocation);
}
