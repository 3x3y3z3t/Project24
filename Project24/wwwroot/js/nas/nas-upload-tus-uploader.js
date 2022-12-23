/*  nas-upload-tus-uploader.js
 *  Version: 1.9 (2022.12.24)
 *
 *  Contributor
 *      Arime-chan
 */

NasUploader.TusUploader = {};

// ==================================================
// events

NasUploader.TusUploader.onError = function (_error) {
    NasUploader.markFileAsErrorByIndex(NasUploader.Backend.m_UploadInProgress);
    NasUploader.m_ActiveTusUpload = null;

    console.error("tus upload error: " + _error);
    window.alert("Upload error, please see Console for more information (QoL coming soon).");

    NasUploader.tryStartNextUpload();
}

NasUploader.TusUploader.onProgress = function (_bytesSent, _bytesTotal) {
    NasUploader.markFileAsInProgressByIndex(NasUploader.Backend.m_UploadInProgress);
    NasUploader.m_IsTransfering = true;

    $("#current-file-name").html(NasUploader.m_ActiveTusUpload.file.name);

    {
        // "current" scope;
        let percent = (_bytesSent / _bytesTotal * 100.0).toFixed(2);
        let uploaded = formatDataLength(_bytesSent);
        let total = formatDataLength(_bytesTotal);
        let diffPercent = percent - NasUploader.Stats.m_CurrentBytesAcceptedPercent;

        //if (diffPercent < 0) {
        //    console.log("_bytesSent = " + _bytesSent);
        //    console.log("_bytesTotal = " + _bytesTotal);
        //    console.log("percent = " + percent);
        //    console.log("bigPercent = " + NasUploader.m_CurrentBytesAcceptedPercent);
        //}

        NasUploader.Stats.m_CurrentBytesUploaded = _bytesSent;
        NasUploader.Stats.m_CurrentBytesUploadedPercent = percent;

        $("#current-uploaded-progress-bar").width(diffPercent + '%');
        $("#current-progress-info").html("Current: " + uploaded + " / " + total + " (" + percent + "%)");
        $("#current-progress-info-status").attr("hidden", true);
    }

    {
        // "total" scope;
        let percent = ((_bytesSent + NasUploader.Stats.m_TotalBytesUploaded) / NasUploader.Stats.m_TotalBytesToUpload * 100.0).toFixed(2);
        let uploaded = formatDataLength(_bytesSent + NasUploader.Stats.m_TotalBytesUploaded);
        let total = formatDataLength(NasUploader.Stats.m_TotalBytesToUpload);
        let diffPercent = percent - NasUploader.Stats.m_TotalBytesAcceptedPercent;

        NasUploader.Stats.m_TotalBytesUploadedPercent = percent;

        $("#total-uploaded-progress-bar").width(diffPercent + '%');
        $("#total-progress-info").html("Total: " + uploaded + " / " + total + " (" + percent + "%)");
        $("#total-progress-info-status").attr("hidden", true);
    }

    //NasUploader.Stats.m_TotalBytesUploaded_Realtime = NasUploader.Stats.m_TotalBytesUploaded + _bytesSent;
}

NasUploader.TusUploader.onSuccess = function () {
    NasUploader.markFileAsCompletedByIndex(NasUploader.Backend.m_UploadInProgress);
    NasUploader.m_ActiveTusUpload = null;
    NasUploader.m_IsTransfering = false;

    let file = NasUploader.m_Files[NasUploader.Backend.m_UploadInProgress];

    NasUploader.Stats.m_CurrentBytesAcceptedPercent = 0.0;

    NasUploader.Stats.m_TotalBytesUploaded += file.size;
    NasUploader.Stats.m_TotalBytesAccepted += file.size;
    NasUploader.Stats.m_TotalBytesAcceptedPercent = (NasUploader.Stats.m_TotalBytesAccepted / NasUploader.Stats.m_TotalBytesToUpload * 100.0).toFixed(2);

    $("#current-file-name").html("None");
    $("#current-progress-info").html("Current: None");
    $("#current-uploaded-progress-bar").width("0%");
    $("#current-accepted-progress-bar").width("0%");

    NasUploader.tryStartNextUpload();
}

NasUploader.TusUploader.onChunkComplete = function (_chunkSize, _bytesAccepted, _bytesTotal) {
    {
        // "current" scope;
        NasUploader.Stats.m_CurrentBytesAcceptedPercent = (_bytesAccepted / _bytesTotal * 100.0).toFixed(2);
        let diffPercent = NasUploader.Stats.m_CurrentBytesUploadedPercent - NasUploader.Stats.m_CurrentBytesAcceptedPercent;

        $("#current-uploaded-progress-bar").width(diffPercent + '%');
        $("#current-accepted-progress-bar").width(NasUploader.Stats.m_CurrentBytesAcceptedPercent + '%');
    }

    {
        // "total" scope;
        NasUploader.Stats.m_TotalBytesAcceptedPercent = ((_bytesAccepted + NasUploader.Stats.m_TotalBytesAccepted) / NasUploader.Stats.m_TotalBytesToUpload * 100.0).toFixed(2);
        let diffPercent = NasUploader.Stats.m_TotalBytesUploadedPercent - NasUploader.Stats.m_TotalBytesAcceptedPercent;

        $("#total-uploaded-progress-bar").width(diffPercent + '%');
        $("#total-accepted-progress-bar").width(NasUploader.Stats.m_TotalBytesAcceptedPercent + '%');
    }
}

// END: events
// ==================================================

NasUploader.TusUploader.tryStartUpload = function (_index) {
    if (_index > NasUploader.m_Files.length) {
        console.warn("Upload file index out of range: " + _index + "/" + NasUploader.m_Files.length + ".");
        return;
    }

    if (NasUploader.m_ActiveTusUpload != null) {
        console.warn("Other upload is in progress.");
        return;
    }

    let file = NasUploader.m_Files[_index];
    let uploader = new tus.Upload(file, {
        endpoint: "/Nas/Upload0",                                   // Endpoint is the upload creation URL from your tus server
        retryDelays: [0, 500, 1000, 3000, 5000, 10000],             // Retry delays will enable tus-js-client to automatically retry on errors
        chunkSize: 64 * 1024 * 1024,                                // Set chunk size to 64MiB;

        metadata: {
            fileName: file.name,
            fileSize: file.size,
            filePath: NasUploader.m_UploadLocation,
            fileDate: file.lastModified,
            contentType: file.type,
        },                                                          // Attach additional meta data about the file for the server

        onError: NasUploader.TusUploader.onError,                   // Callback for errors which cannot be fixed using retries
        onProgress: NasUploader.TusUploader.onProgress,             // Callback for reporting upload progress
        onChunkComplete: NasUploader.TusUploader.onChunkComplete,   // Callback for reporting accepted chunks
        onSuccess: NasUploader.TusUploader.onSuccess,               // Callback for once the upload is completed
    });

    // Check if there are any previous uploads to continue.
    //NasUploader.m_ActiveTusUpload.findPreviousUploads().then(function (_previousUploads) {
    //    // Found previous uploads so we select the first one. 
    //    if (_previousUploads.length) {
    //        NasUploader.m_ActiveTusUpload.resumeFromPreviousUpload(previousUploads[0]);
    //    }

    //    // Restart the upload
    //    NasUploader.m_ActiveTusUpload.start();
    //});

    NasUploader.m_ActiveTusUpload = uploader;
    NasUploader.m_ActiveTusUpload.start();

    NasUploader.m_IsTransfering = true;
    NasUploader.Backend.m_UploadInProgress = _index;
    console.info("Uploading file " + _index + ": " + file.name + "(" + formatDataLength(file.size) + ")..");
}
