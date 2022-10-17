/*  nas-upload.js
 *  Version: 1.1 (2022.10.18)
 *
 *  Contributor
 *      Arime-chan
 */

var g_FilesToUpload = new Array();

var g_ActiveTusUpload = null;
var g_UploadFilePath = ""; //TODO: fetch this;
var g_IsTransfering = false;
var g_UploadInProgress = -1;

var g_TotalBytesToUpload = 0;
var g_TotalBytesUploaded = 0;

$(document).ready(function () {
    $("#input-files").change(updateFilesList);

    $("#button-upload").on("click", uploadButton_onClick);
    $("#button-pause").on("click", pauseUpload);
    $("#button-resume").on("click", resumeUpload);

    $("#current-progress-info-status").hide();
    $("#total-progress-info-status").hide();

    g_UploadFilePath = $("#upload-location").html();

    enableFunctionButtons(false);
})

function uploadButton_onClick() {
    $("#input-files").attr("disabled", true);
    $("#button-upload").attr("disabled", true);

    $("#button-pause").removeAttr("disabled");
    $("#button-resume").removeAttr("disabled");

    tryStartNextUpload();
}

function tusUpload_OnError(_error) {
    console.log("tusUpload error: " + _error);
    window.alert("Upload error, please see Console for more information (QoL coming soon).");
    g_ActiveTusUpload = null;
}

function tusUpload_OnProgress(_bytesUploaded, _bytesTotal) {
    markFileAsInProgress(g_UploadInProgress);
    g_IsTransfering = true;

    $("#current-file-name").html(g_ActiveTusUpload.file.name);

    {
        // "current" scope;
        var percentage = (_bytesUploaded / _bytesTotal * 100.0).toFixed(2);
        var uploaded = formatDataLength(_bytesUploaded);
        var total = formatDataLength(_bytesTotal);

        $("#current-progress-bar-wrapper").attr("aria-valuenow", percentage);

        $("#current-progress-bar").width(percentage + '%');

        $("#current-progress-info").html("Current: " + uploaded + " / " + total + " (" + percentage + "%)");
        $("#current-progress-info-status").hide();
    }

    {
        // "total" scope;
        var percentage = ((_bytesUploaded + g_TotalBytesUploaded) / g_TotalBytesToUpload * 100.0).toFixed(2);
        var uploaded = formatDataLength(_bytesUploaded + g_TotalBytesUploaded);
        var total = formatDataLength(g_TotalBytesToUpload);

        $("#total-progress-bar-wrapper").attr("aria-valuenow", percentage);

        $("#total-progress-bar").width(percentage + '%');

        $("#total-progress-info").html("Total: " + uploaded + " / " + total + " (" + percentage + "%)");
        $("#total-progress-info-status").hide();
    }
}

function tusUpload_onSuccess() {
    markFileAsCompleted(g_ActiveTusUpload.file.name);

    $("#current-file-name").html("None");
    $("#current-progress-bar").width("0%");
    $("#current-progress-info").html("Current: None");

    g_ActiveTusUpload = null;
    g_IsTransfering = false;

    let file = g_FilesToUpload[g_UploadInProgress];
    g_TotalBytesUploaded += file.size;

    tryStartNextUpload();
}

function updateFilesList() {
    g_TotalBytesToUpload = 0;

    g_FilesToUpload = $("#input-files")[0].files;

    if (g_FilesToUpload.length > 0) {
        $("#button-upload").removeAttr("disabled");
    } else {
        $("#button-upload").attr("disabled", true);
    }

    let htmlFileDetails = "";

    for (let i = 0; i < g_FilesToUpload.length; ++i) {
        let file = g_FilesToUpload[i];

        g_TotalBytesToUpload += file.size;

        htmlFileDetails += "<div id=\"file-" + i + "\" class=\"d-flex\">";

        htmlFileDetails += "<div style=\"min-width: 2ch; font-weight:bold\"></div>";
        htmlFileDetails += "<div class=\"text-break\">" + file.name + "</div>";
        htmlFileDetails += "<div class=\"ml-auto pl-2 text-right text-nowrap\">" + formatDataLength(file.size) + "</div>";

        htmlFileDetails += "</div>\n";
    }

    let htmlFileOverview = "Total: " + g_FilesToUpload.length + " file";
    if (g_FilesToUpload.length > 1)
        htmlFileOverview += "s"

    htmlFileOverview += " (" + formatDataLength(g_TotalBytesToUpload) + ")";

    $("#files-list").html(htmlFileDetails);
    $("#files-list-overview").html(htmlFileOverview);
}

function markFileAsInProgress(_index) {
    if (_index < 0 || _index > g_FilesToUpload.length)
        return;

    for (let i = 0; i < g_FilesToUpload.length; ++i) {
        if (i == _index) {
            let divElement = $("#file-" + i);
            if (!divElement.hasClass("text-warning"))
                divElement.addClass("text-warning");

            $("#file-" + i + " div:first-child").html(">");
        }
    }
}

function markFileAsCompleted(_filename) {
    if (_filename == null)
        return;

    for (let i = 0; i < g_FilesToUpload.length; ++i) {
        if (g_FilesToUpload[i].name == _filename) {
            let divElement = $("#file-" + i);
            divElement.removeClass("text-warning");
            divElement.addClass("text-success");

            $("#file-" + i + " div:first-child").html("");
        }
    }
}

function enableFunctionButtons(_enable = true) {
    if (_enable) {
        $("#button-upload").removeAttr("disabled");
        $("#button-pause").removeAttr("disabled");
        $("#button-resume").removeAttr("disabled");
    } else {
        $("#button-upload").attr("disabled", true);
        $("#button-pause").attr("disabled", true);
        $("#button-resume").attr("disabled", true);
    }
}

function beginUploadProcess() {
    if (g_FilesToUpload.length <= 0)
        return;

    if (g_UploadInProgress > 0 || g_ActiveTusUpload != null)
        return;

    tryStartUpload(0);
}

function tryStartUpload(_index) {
    if (_index >= g_FilesToUpload.length)
        return;

    if (g_ActiveTusUpload != null)
        return;

    let file = g_FilesToUpload[_index];
    g_ActiveTusUpload = new tus.Upload(g_FilesToUpload[_index], {
        endpoint: "/Nas/Upload0",                           // Endpoint is the upload creation URL from your tus server
        retryDelays: [0, 3000, 5000, 10000, 20000],         // Retry delays will enable tus-js-client to automatically retry on errors
        chunkSize: 10 * 1024 * 1024,                        // Set chunk size to 10MiB;

        metadata: {
            fileName: file.name,
            contentType: file.type,
            filePath: g_UploadFilePath,
        },                                                  // Attach additional meta data about the file for the server

        onError: tusUpload_OnError,                         // Callback for errors which cannot be fixed using retries
        onProgress: tusUpload_OnProgress,                   // Callback for reporting upload progress
        onSuccess: tusUpload_onSuccess,                     // Callback for once the upload is completed
    });

    // Check if there are any previous uploads to continue.
    //upload.findPreviousUploads().then(function (previousUploads) {
    //    // Found previous uploads so we select the first one. 
    //    if (previousUploads.length) {
    //        upload.resumeFromPreviousUpload(previousUploads[0]);
    //    }

    //    // Restart the upload
    //    g_ActiveTusUpload.start();
    //});

    g_ActiveTusUpload.start();

    g_UploadInProgress = _index;
}

function tryStartNextUpload() {
    if (g_UploadInProgress + 1 < g_FilesToUpload.length) {
        tryStartUpload(g_UploadInProgress + 1);
    } else {
        g_UploadInProgress = -1;

        $("#input-files").removeAttr("disabled");
        enableFunctionButtons(false);
    }
}

function pauseUpload() {
    if (!g_IsTransfering)
        return;

    g_ActiveTusUpload.abort();
    g_IsTransfering = false;

    $("#current-progress-info-status").show();
    $("#total-progress-info-status").show();
}

function resumeUpload() {
    if (g_IsTransfering)
        return;

    g_ActiveTusUpload.start();
    g_IsTransfering = true;
}

function abortCurrentFile() {
    g_ActiveTusUpload.abort(true);
    g_ActiveTusUpload = null;
    g_IsTransfering = false;

    tryStartNextUpload();
}

function abortAllFiles() {
    g_ActiveTusUpload.abort(true);
    g_ActiveTusUpload = null;
    g_IsTransfering = false;

    g_UploadInProgress = -1;
    g_FilesToUpload = new Array();
}
