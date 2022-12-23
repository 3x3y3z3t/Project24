/*  updater.js
 *  Version: 1.2 (2022.12.18)
 *
 *  Contributor
 *      Arime-chan
 */

const c_MaxUploadSize = 64 * 1024 * 1024;

window.Updater = {};

Updater.m_HasError = false;

Updater.m_UploadFileList = null;
Updater.m_BatchCount = 0;
Updater.m_BatchStatus = {};

Updater.m_UploadFileHashCodes = {};
Updater.m_UploadFileLastModifiedDates = {};

Updater.m_LocalFileModDates = {};
Updater.m_ShouldUploadList = {};


$(document).ready(function () {
    $("#input-files").change(updateFilesList);

    $("#button-upload").on("click", uploadButton_onClick);

    localFileList_OnChanged();
    //window.setInterval(function () {
    //    NasUploader.m_UploadFilePath = $("#upload-location").html();
    //    updateStatistic();
    //}, 500);
});

function btn_UpdateStaticFiles() {
    $("#btn-upload-static-files").attr("disabled", true);
    $("#btn-purge").attr("disabled", true);

    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: 'POST',
        url: 'Updater/UpdateStaticFiles',
        headers: { "RequestVerificationToken": token },
        cache: false,
        contentType: false,
        processData: false,
        success: function (_content) {
            $("#local-file-panel").html(_content);
        },
        error: function (_xhr, _status, _errorThrow) {
            console.error("Update Static Files Request error (" + _status + ")\nServer msg: " + _xhr.responseText);
        }
    });
}

function uploadButton_onClick() {
    $("#button-upload").attr("disabled", true);

    if (Updater.m_UploadFileList == null || Updater.m_UploadFileList.length <= 0) {
        console.error("There is no file to upload!");
        $("#button-upload").removeAttr("disabled");
        return;
    }

    for (let i = 0; i < Updater.m_BatchCount; ++i) {
        Updater.m_BatchStatus[i] = "idle";
    }

    initUpload();
}

function purgeButton_OnClick() {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: 'POST',
        url: 'Updater/PurgeNext',
        headers: { "RequestVerificationToken": token },
        cache: false,
        contentType: false,
        processData: false,
        success: function (_content) {
            $("#local-file-panel").html(_content);
            localFileList_OnChanged();
        },
        error: function (_xhr, _status, _errorThrow) {
            console.error("Purge Request error (" + _status + ")\nServer msg: " + _xhr.responseText);
        }
    });
}

function verUpButton_OnClick() {
    let token = $("input[name='__RequestVerificationToken']").val();

    $("#btn-ver-up").attr("disabled", true);
    $("#btn-purge").attr("disabled", true);

    $.ajax({
        type: 'POST',
        url: 'Updater/VersionUp',
        headers: { "RequestVerificationToken": token },
        cache: false,
        contentType: false,
        processData: false,
        success: function (_content) {
            $("#local-file-panel").html(_content);
        },
        error: function (_xhr, _status, _errorThrow) {
            console.error("Ver Up Request error (" + _status + ")\nServer msg: " + _xhr.responseText);
        }
    });
}

function abortVerUpButton_OnClick() {
    let token = $("input[name='__RequestVerificationToken']").val();

    $("#btn-abort").attr("disabled", true);
    $("#btn-purge").attr("disabled", true);

    $.ajax({
        type: 'POST',
        url: 'Updater/AbortVersionUp',
        headers: { "RequestVerificationToken": token },
        cache: false,
        contentType: false,
        processData: false,
        success: function (_content) {
            $("#local-file-panel").html(_content);
        },
        error: function (_xhr, _status, _errorThrow) {
            console.error("Abort Ver Up Request error (" + _status + ")\nServer msg: " + _xhr.responseText);
        }
    });
}

function localFileList_OnChanged() {
    Updater.m_LocalFileModDates = {};

    let fileCount = $("#local-file-count").html();

    for (let i = 0; i < fileCount; ++i) {

        let html = $("#local-file-" + i + " div:first-child").html();

        let pos = html.indexOf(":");
        let hashCode = html.substring(0, pos);
        let date = html.substring(pos + 1);

        Updater.m_LocalFileModDates[hashCode] = new Date(date);
    }
}

function ajaxPost_Success(_batch) {
    Updater.m_BatchStatus[_batch - 1] = "success";

    console.info("Batch " + _batch + " success.");

    let isFinished = true;

    for (let i = 0; i < Updater.m_BatchCount; ++i) {
        if (Updater.m_BatchStatus[i] == "idle") {
            isFinished = false;
        }
    }

    if (!isFinished)
        return;

    if (Updater.m_HasError) {
        window.alert("Some upload batches failed. See console for more info.");
    }
    else {
        //location.reload();
    }

    localFileList_OnChanged();
    $("#button-upload").removeAttr("disabled");
}

function initUpload() {
    let token = $("input[name='__RequestVerificationToken']").val();

    let count = 0;
    for (let i = 0; i < Updater.m_UploadFileList.length; ++i) {
        if (Updater.m_ShouldUploadList[i])
            ++count;
    }

    $.ajax({
        type: 'POST',
        url: 'Updater/InitUpload',
        headers: { "RequestVerificationToken": token, "TotalFiles": count },
        success: function (_data, _status, _xhr) {
            if (_xhr.status != 200) {
                console.warn("initUpload success, but server returned '" + _status + "' (" + _xhr.status + ") instead.");
                return;
            }

            startUpload();
        },
        error: function (_xhr, _status, _errorThrow) {
            console.error("Upload init failed: " + _status + "\nServer msg: " + _xhr.responseText);
        }
    });
}

function startUpload() {
    let formData = new FormData();

    let hasPendingUpload = false;
    let totalSize = 0;
    let currentBatch = 0;
    let uploadFileLastModDates = {};

    for (let i = 0; i < Updater.m_UploadFileList.length; ++i) {
        if (!Updater.m_ShouldUploadList[i])
            continue;

        let file = Updater.m_UploadFileList[i];
        if (totalSize + file.size < c_MaxUploadSize) {
            hasPendingUpload = true;

            formData.append("_files", file);
            totalSize += file.size;

            // =====
            let poss = file.webkitRelativePath.lastIndexOf("/");
            let paths = file.webkitRelativePath.substring(8, poss + 1);
            let hc = computeCyrb53HashCode(paths + file.name);
            // =====

            let hashCode = Updater.m_UploadFileHashCodes[i];
            uploadFileLastModDates[hashCode] = file.lastModified;

            continue;
        }

        uploadBatch(currentBatch + 1, formData, uploadFileLastModDates);

        hasPendingUpload = false;
        ++currentBatch;
        formData = new FormData();
        totalSize = 0;
        uploadFileLastModDates = {};

        --i;
    }

    if (hasPendingUpload) {
        uploadBatch(currentBatch + 1, formData, uploadFileLastModDates);
    }
}

function uploadBatch(_batch, _formData, _extraHeader) {
    let token = $("input[name='__RequestVerificationToken']").val();

    let jsonHeader = JSON.stringify(_extraHeader);

    console.info("Uploading batch " + _batch + "/" + Updater.m_BatchCount + "..");

    $.ajax({
        type: 'POST',
        url: 'Updater/Upload',
        headers: { "RequestVerificationToken": token, "LastModifiedDates": jsonHeader },
        data: _formData,
        contentType: false,
        processData: false,
        success: function (_content) {
            $("#local-file-panel").html(_content);

            ajaxPost_Success(_batch);
        },
        error: function (_xhr, _status, _errorThrow) {
            Updater.m_BatchStatus[_batch] = "failed";
            Updater.m_HasError = true;

            console.error("Batch " + _batch + " failed: " + _status + "\nServer msg: " + _xhr.responseText);
        }
    });
}


function updateFilesList() {
    Updater.m_HasError = false;
    Updater.m_UploadFileList = null;
    Updater.m_BatchCount = 0;
    Updater.m_BatchStatus = {};
    Updater.m_UploadFileHashCodes = {};
    Updater.m_UploadFileLastModifiedDates = {};
    Updater.m_ShouldUploadList = {};

    Updater.m_UploadFileList = $("#input-files")[0].files;
    if (Updater.m_UploadFileList.length > 0) {
        $("#button-upload").removeAttr("disabled");
    } else {
        $("#button-upload").attr("disabled", true);
    }

    let htmlFileDetails = "";
    let totalSize = 0;
    let totalUpload = 0;

    for (let i = 0; i < Updater.m_UploadFileList.length; ++i) {
        let file = Updater.m_UploadFileList[i];

        htmlFileDetails += processFileInfo(i, file);

        if (Updater.m_ShouldUploadList[i]) {
            totalSize += file.size;
            ++totalUpload;
        }
    }

    Updater.m_BatchCount = Math.ceil(totalSize / c_MaxUploadSize);

    let htmlFileOverview = totalUpload + "/" + Updater.m_UploadFileList.length + " file";
    if (totalUpload > 1)
        htmlFileOverview += "s";

    htmlFileOverview += ", " + formatDataLength(totalSize);

    $("#upload-file-list").html(htmlFileDetails);
    $("#upload-file-count").html(htmlFileOverview);
}

function processFileInfo(_index, _file) {
    let pos = _file.webkitRelativePath.lastIndexOf("/");
    let path = _file.webkitRelativePath.substring(8, pos + 1);

    let hashCode = computeCyrb53HashCode(path + _file.name);
    let dateString = formatDateString(_file.lastModifiedDate);
    let date = new Date(dateString);

    Updater.m_ShouldUploadList[_index] = true;
    Updater.m_UploadFileHashCodes[_index] = hashCode;

    let textProp = "";
    if (Updater.m_LocalFileModDates[hashCode] == null) {
        textProp = "";
    }
    else if (Updater.m_LocalFileModDates[hashCode] < date) {
        textProp = "text-warning";
    }
    else {
        textProp = "text-success";
        Updater.m_ShouldUploadList[_index] = false;
    }

    let html = "";

    html += "<div id=\"file-" + _index + "\" class=\"d-flex align-items-center " + textProp + " mr-2\">";
    {
        html += "<div hidden>" + hashCode + "</div>";
        html += "<div class=\"text-break\"><span class=\"font-weight-light\">" + path + "</span>" + _file.name + "</div>";
        html += "<div class=\"text-break text-right ml-auto\" style=\"font-size: small;\">" + dateString + "</div>";
        html += "<div class=\"text-right text-nowrap ml-2 pl-1\" style=\"width: 9%;\">" + formatDataLength(_file.size) + "</div>";
    }
    html += "</div>\n";

    return html
}

function computeCyrb53HashCode(_string, _seed = 0) {
    return cyrb53(_string, _seed);
}
