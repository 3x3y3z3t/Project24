/*  nas-upload-tus-uploader.js
 *  Version: 1.8 (2022.12.16)
 *
 *  Contributor
 *      Arime-chan
 */

window.NasUploader = {};

NasUploader.m_FilesToUpload = new Array();

NasUploader.m_ActiveTusUpload = null;
NasUploader.m_UploadFilePath = "";
NasUploader.m_IsTransfering = false;
NasUploader.m_UploadInProgress = -1;

NasUploader.m_TotalBytesToUpload = 0;
NasUploader.m_TotalBytesSent = 0;

NasUploader.m_TotalBytesSent_Realtime = 0;

NasUploader.m_CurrentBytesAccepted = 0;
NasUploader.m_CurrentBytesSentPercent = 0;
NasUploader.m_CurrentBytesAcceptedPercent = 0;

NasUploader.m_TotalBytesAccepted = 0;
NasUploader.m_TotalBytesSentPercent = 0;
NasUploader.m_TotalBytesAcceptedPercent = 0;

NasUploader.m_StartDate = null;


$(document).ready(function () {
    $("#input-files").change(updateFilesList);

    $("#button-upload").on("click", uploadButton_onClick);
    $("#button-pause").on("click", pauseUpload);
    $("#button-resume").on("click", resumeUpload);

    $("#current-progress-info-status").hide();
    $("#total-progress-info-status").hide();

    enableFunctionButtons(false);

    window.setInterval(function () {
        NasUploader.m_UploadFilePath = $("#upload-location").text();
        updateStatistic();
    }, 500);
});


function uploadButton_onClick() {
    $("#input-files").attr("disabled", true);
    $("#button-upload").attr("disabled", true);

    $("#button-pause").removeAttr("disabled");
    $("#button-resume").removeAttr("disabled");

    $("#total-progress-bar-wrapper").attr("aria-valuenow", 0);
    $("#total-sent-progress-bar").width("0%");

    tryStartNextUpload();
    NasUploader.m_StartDate = new Date();
    NasUploader.m_TotalBytesSent_Realtime = 0;
}

function tusUpload_OnError(_error) {
    markFileAsError(NasUploader.m_ActiveTusUpload.file.name);

    console.log("tusUpload error: " + _error);
    window.alert("Upload error, please see Console for more information (QoL coming soon).");
    NasUploader.m_ActiveTusUpload = null;

    tryStartNextUpload();
}

function tusUpload_OnProgress(_bytesSent, _bytesTotal) {
    markFileAsInProgress(NasUploader.m_UploadInProgress);
    NasUploader.m_IsTransfering = true;

    $("#current-file-name").html(NasUploader.m_ActiveTusUpload.file.name);

    {
        // "current" scope;
        let percent = (_bytesSent / _bytesTotal * 100.0).toFixed(2);
        let uploaded = formatDataLength(_bytesSent);
        let total = formatDataLength(_bytesTotal);
        let diffPercent = percent - NasUploader.m_CurrentBytesAcceptedPercent;

        //if (diffPercent < 0) {
        //    console.log("_bytesSent = " + _bytesSent);
        //    console.log("_bytesTotal = " + _bytesTotal);
        //    console.log("percent = " + percent);
        //    console.log("bigPercent = " + NasUploader.m_CurrentBytesAcceptedPercent);
        //}

        NasUploader.m_CurrentBytesSentPercent = percent;

        $("#current-progress-bar-wrapper").attr("aria-valuenow", percent);
        $("#current-sent-progress-bar").width(diffPercent + '%');
        $("#current-progress-info").html("Current: " + uploaded + " / " + total + " (" + percent + "%)");
        $("#current-progress-info-status").hide();
    }

    {
        // "total" scope;
        let percent = ((_bytesSent + NasUploader.m_TotalBytesSent) / NasUploader.m_TotalBytesToUpload * 100.0).toFixed(2);
        let uploaded = formatDataLength(_bytesSent + NasUploader.m_TotalBytesSent);
        let total = formatDataLength(NasUploader.m_TotalBytesToUpload);
        let diffPercent = percent - NasUploader.m_TotalBytesAcceptedPercent;

        NasUploader.m_TotalBytesSentPercent = percent;

        $("#total-progress-bar-wrapper").attr("aria-valuenow", percent);
        $("#total-sent-progress-bar").width(diffPercent + '%');
        $("#total-progress-info").html("Total: " + uploaded + " / " + total + " (" + percent + "%)");
        $("#total-progress-info-status").hide();
    }

    NasUploader.m_TotalBytesSent_Realtime = NasUploader.m_TotalBytesSent + _bytesSent;
}

function tusUpload_OnChunkComplete(_chunkSize, _bytesAccepted, _bytesTotal) {
    {
        NasUploader.m_CurrentBytesAcceptedPercent = (_bytesAccepted / _bytesTotal * 100.0).toFixed(2);
        let diffPercent = NasUploader.m_CurrentBytesSentPercent - NasUploader.m_CurrentBytesAcceptedPercent;

        $("#current-sent-progress-bar").width(diffPercent + '%');
        $("#current-accepted-progress-bar").width(NasUploader.m_CurrentBytesAcceptedPercent + '%');
    }

    {
        NasUploader.m_TotalBytesAcceptedPercent = ((_bytesAccepted + NasUploader.m_TotalBytesAccepted) / NasUploader.m_TotalBytesToUpload * 100.0).toFixed(2);
        let diffPercent = NasUploader.m_TotalBytesSentPercent - NasUploader.m_TotalBytesAcceptedPercent;

        $("#total-sent-progress-bar").width(diffPercent + '%');
        $("#total-accepted-progress-bar").width(NasUploader.m_TotalBytesAcceptedPercent + '%');
    }
}

function tusUpload_OnSuccess() {
    markFileAsCompleted(NasUploader.m_ActiveTusUpload.file.name);

    $("#current-file-name").html("None");
    $("#current-sent-progress-bar").width("0%");
    $("#current-accepted-progress-bar").width("0%");
    $("#current-progress-info").html("Current: None");

    NasUploader.m_ActiveTusUpload = null;
    NasUploader.m_IsTransfering = false;

    let file = NasUploader.m_FilesToUpload[NasUploader.m_UploadInProgress];
    NasUploader.m_TotalBytesSent += file.size;
    NasUploader.m_TotalBytesAccepted += file.size;

    NasUploader.m_TotalBytesAcceptedPercent = (NasUploader.m_TotalBytesAccepted / NasUploader.m_TotalBytesToUpload * 100.0).toFixed(2);
    NasUploader.m_CurrentBytesAcceptedPercent = 0.0;

    tryStartNextUpload();
}

function beginUploadProcess() {
    if (NasUploader.m_FilesToUpload.length <= 0)
        return;

    if (NasUploader.m_UploadInProgress > 0 || NasUploader.m_ActiveTusUpload != null)
        return;

    tryStartUpload(0);
}

function tryStartUpload(_index) {
    if (_index >= NasUploader.m_FilesToUpload.length)
        return;

    if (NasUploader.m_ActiveTusUpload != null)
        return;

    let file = NasUploader.m_FilesToUpload[_index];
    NasUploader.m_ActiveTusUpload = new tus.Upload(NasUploader.m_FilesToUpload[_index], {
        endpoint: "/Nas/Upload0",                           // Endpoint is the upload creation URL from your tus server
        retryDelays: [0, 500, 1000, 3000, 5000, 10000],     // Retry delays will enable tus-js-client to automatically retry on errors
        chunkSize: 64 * 1024 * 1024,                        // Set chunk size to 64MiB;

        metadata: {
            fileName: file.name,
            fileSize: file.size,
            filePath: NasUploader.m_UploadFilePath,
            contentType: file.type,
        },                                                  // Attach additional meta data about the file for the server

        onError: tusUpload_OnError,                         // Callback for errors which cannot be fixed using retries
        onProgress: tusUpload_OnProgress,                   // Callback for reporting upload progress
        onChunkComplete: tusUpload_OnChunkComplete,         // Callback for reporting accepted chunks
        onSuccess: tusUpload_OnSuccess,                     // Callback for once the upload is completed
    });

    // Check if there are any previous uploads to continue.
    NasUploader.m_ActiveTusUpload.findPreviousUploads().then(function (_previousUploads) {
        // Found previous uploads so we select the first one. 
        if (_previousUploads.length) {
            NasUploader.m_ActiveTusUpload.resumeFromPreviousUpload(previousUploads[0]);
        }

        // Restart the upload
        NasUploader.m_ActiveTusUpload.start();
    });

    NasUploader.m_ActiveTusUpload.start();

    NasUploader.m_UploadInProgress = _index;
}

function tryStartNextUpload() {
    if (NasUploader.m_UploadInProgress + 1 < NasUploader.m_FilesToUpload.length) {
        tryStartUpload(NasUploader.m_UploadInProgress + 1);
    } else {
        NasUploader.m_UploadInProgress = -1;
        NasUploader.m_StartDate = null;

        $("#input-files").removeAttr("disabled");
        enableFunctionButtons(false);
    }
}

function updateStatistic() {
    if (!NasUploader.m_IsTransfering)
        return;
    if (NasUploader.m_StartDate == null)
        return;

    let elapsed = new Date() - NasUploader.m_StartDate;
    let avgSpeed = (NasUploader.m_TotalBytesSent_Realtime) / elapsed * 1000.0;

    $("#upload-elapsed-time").html("Elapsed time: " + formatTimeSpan_Hour(elapsed));
    $("#upload-avg-speed").html("Average speed: " + formatDataLength(avgSpeed) + "/s");

    //console.log(formatTimeSpan_Hour(elapsed) + ", " + formatDataLength(avgSpeed) + "/");
}

function pauseUpload() {
    if (!NasUploader.m_IsTransfering)
        return;

    NasUploader.m_ActiveTusUpload.abort();
    NasUploader.m_IsTransfering = false;

    $("#current-progress-info-status").show();
    $("#total-progress-info-status").show();
}

function resumeUpload() {
    if (NasUploader.m_IsTransfering)
        return;

    NasUploader.m_ActiveTusUpload.start();
    NasUploader.m_IsTransfering = true;
}

function abortCurrentFile() {
    NasUploader.m_ActiveTusUpload.abort(true);
    NasUploader.m_ActiveTusUpload = null;
    NasUploader.m_IsTransfering = false;

    tryStartNextUpload();
}

function abortAllFiles() {
    NasUploader.m_ActiveTusUpload.abort(true);
    NasUploader.m_ActiveTusUpload = null;
    NasUploader.m_IsTransfering = false;

    NasUploader.m_UploadInProgress = -1;
    NasUploader.m_FilesToUpload = new Array();
}

function updateFilesList() {
    NasUploader.m_TotalBytesToUpload = 0;
    NasUploader.m_TotalBytesSent = 0;
    NasUploader.m_TotalBytesAccepted = 0;

    NasUploader.m_FilesToUpload = $("#input-files")[0].files;

    if (NasUploader.m_FilesToUpload.length > 0) {
        $("#button-upload").removeAttr("disabled");
    } else {
        $("#button-upload").attr("disabled", true);
    }

    let htmlFileDetails = "";

    for (let i = 0; i < NasUploader.m_FilesToUpload.length; ++i) {
        let file = NasUploader.m_FilesToUpload[i];

        NasUploader.m_TotalBytesToUpload += file.size;

        htmlFileDetails += "<div id=\"file-" + i + "\" class=\"d-flex\">";

        htmlFileDetails += "<div style=\"min-width: 2ch; font-weight:bold\"></div>";
        htmlFileDetails += "<div class=\"text-break\">" + file.name + "</div>";
        htmlFileDetails += "<div class=\"ml-auto pl-2 text-right text-nowrap\">" + formatDataLength(file.size) + "</div>";

        htmlFileDetails += "</div>\n";
    }

    let htmlFileOverview = "Total: " + NasUploader.m_FilesToUpload.length + " file";
    if (NasUploader.m_FilesToUpload.length > 1)
        htmlFileOverview += "s"

    htmlFileOverview += " (" + formatDataLength(NasUploader.m_TotalBytesToUpload) + ")";

    $("#files-list").html(htmlFileDetails);
    $("#files-list-overview").html(htmlFileOverview);
}

function markFileAsInProgress(_index) {
    if (_index < 0 || _index > NasUploader.m_FilesToUpload.length)
        return;

    for (let i = 0; i < NasUploader.m_FilesToUpload.length; ++i) {
        if (i == _index) {
            let divElement = $("#file-" + i);
            divElement.removeClass("text-danger");
            divElement.removeClass("text-success");

            if (!divElement.hasClass("text-warning"))
                divElement.addClass("text-warning");

            $("#file-" + i + " div:first-child").html(">");
        }
    }
}

function markFileAsCompleted(_filename) {
    if (_filename == null)
        return;

    for (let i = 0; i < NasUploader.m_FilesToUpload.length; ++i) {
        if (NasUploader.m_FilesToUpload[i].name == _filename) {
            let divElement = $("#file-" + i);
            divElement.removeClass("text-warning");
            divElement.removeClass("text-danger");
            divElement.addClass("text-success");

            $("#file-" + i + " div:first-child").html("");
        }
    }
}

function markFileAsError(_filename) {
    if (_filename == null)
        return;

    for (let i = 0; i < NasUploader.m_FilesToUpload.length; ++i) {
        if (NasUploader.m_FilesToUpload[i].name == _filename) {
            let divElement = $("#file-" + i);
            divElement.removeClass("text-warning");
            divElement.removeClass("text-success");
            divElement.addClass("text-danger");

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

function updateProgressbars() {
    let currentSentPercent = 0;
    let currentAcceptedPercent = 0;
    let totalSentPercent = 0;
    let totalAcceptedPercent = 0;



}
