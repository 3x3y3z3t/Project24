/*  nas-upload.js
 *  Version: 1.0 (2022.10.17)
 *
 *  Contributor
 *      Arime-chan
 */

$(document).ready(function () {
    $("#progress-status-2").hide();

    $("#button-pause").click(function () {
        $("#progress-status-2").show();
    });

    $("#button-resume").click(function () {
        $("#progress-status-2").hide();
    });
})

function startUpload() {
    var files = $("#input-file")[0].files;

    $("#button-pause").click(function () {
        $("#progress-status-2").show();
    });
    return;

    if (files.length == 0) {
        return;
    }

    var file = files[0];

    var upload = new tus.Upload(file, {
        // Endpoint is the upload creation URL from your tus server
        endpoint: "/Nas/Upload0",
        // Retry delays will enable tus-js-client to automatically retry on errors
        retryDelays: [0, 3000, 5000, 10000, 20000],
        // Attach additional meta data about the file for the server
        metadata: {
            fileName: file.name,
            contentType: file.type
        },
        // Set chunk size to 10MiB;
        chunkSize: 10 * 1024 * 1024,

        // Callback for errors which cannot be fixed using retries
        onError: function (_error) {
            console.log("tusUpload error: " + _error);
            window.alert("Upload error, please see Console for more information (QoL coming soon).");
        },

        // Callback for reporting upload progress
        onProgress: onProgress,

        // Callback for once the upload is completed
        onSuccess: function () {
            window.alert("Upload success");
        }
    });

    // Check if there are any previous uploads to continue.
    //upload.findPreviousUploads().then(function (previousUploads) {
    //    // Found previous uploads so we select the first one. 
    //    if (previousUploads.length) {
    //        upload.resumeFromPreviousUpload(previousUploads[0]);
    //    }

    //    // Start the upload
    //    upload.start();
    //});

    $("#button-pause").click(function () {
        upload.abort();
    });

    $("#button-resume").click(function () {
        upload.start();
    });

    upload.start();
}

function onProgress(_bytesUploaded, _bytesTotal) {

    var percentage = (_bytesUploaded / _bytesTotal * 100.0).toFixed(2);

    var uploaded = formatDataLength(_bytesUploaded);
    var total = formatDataLength(_bytesTotal);

    $("#progress-bar-wrapper").show();
    $("#progress-bar-wrapper").attr("aria-valuenow", percentage);

    $("#progress-bar").width(percentage + '%');

    $("#progress-status").html(uploaded + " / " + total + " (" + percentage + "%)");
}
