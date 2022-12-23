/*  nas-upload-uploader.js
 *  Version: 1.0 (2022.12.24)
 *  
 *  This script define functions for functionality of the Upload portion of NAS Upload
 *  (right-side panel). Note that this script does not define functions for the uploader,
 *  which is handled by nas-upload-tus-uploader script (backend: tus-js-client).
 *
 *  Contributor
 *      Arime-chan
 */

// ==================================================
window.NasUploader = {};

NasUploader.m_IsTransfering = false;
NasUploader.m_UploadLocation = "";
NasUploader.m_Files = null;

NasUploader.m_ActiveTusUpload = null;

// ==================================================
// Nas Uploader backing objects

NasUploader.Backend = {};
NasUploader.Backend.m_FilesState = [];
NasUploader.Backend.m_DirectoryTree = null;
NasUploader.Backend.m_StartTime = null;
NasUploader.Backend.m_UploadInProgress = -1;

// ==================================================

// ==================================================
// Nas Uploader statistics

NasUploader.Stats = {};

//NasUploader.Stats.m_CurrentBytesToUpload = 0;
NasUploader.Stats.m_CurrentBytesUploaded = 0;
//NasUploader.Stats.m_CurrentBytesAccepted = 0;
NasUploader.Stats.m_CurrentBytesUploadedPercent = 0;
NasUploader.Stats.m_CurrentBytesAcceptedPercent = 0;

NasUploader.Stats.m_TotalBytesToUpload = 0;
NasUploader.Stats.m_TotalBytesUploaded = 0;
NasUploader.Stats.m_TotalBytesAccepted = 0;
NasUploader.Stats.m_TotalBytesUploadedPercent = 0;
NasUploader.Stats.m_TotalBytesAcceptedPercent = 0;

//NasUploader.m_TotalBytesUploaded_Realtime = 0;

// ==================================================

$(document).ready(function () {
    //$.jstree.defaults.core.data = true;

    window.setInterval(function () {
        NasUploader.updateStatistic();
    }, 200);
});

function resetNasUploader() {
    NasUploader.m_IsTransfering = false;
    NasUploader.m_UploadLocation = "";
    NasUploader.m_Files = null;

    NasUploader.m_ActiveTusUpload = null;

    NasUploader.Backend.m_FilesState = [];
    NasUploader.Backend.m_DirectoryTree = null;
    NasUploader.Backend.m_StartTime = null;
    NasUploader.Backend.m_UploadInProgress = -1;
    NasUploader.Backend.m_IsFolderUploadMode = false;

    NasUploader.Stats.m_CurrentBytesUploaded = 0;
    NasUploader.Stats.m_CurrentBytesUploadedPercent = 0;
    NasUploader.Stats.m_CurrentBytesAcceptedPercent = 0;

    NasUploader.Stats.m_TotalBytesToUpload = 0;
    NasUploader.Stats.m_TotalBytesUploaded = 0;
    NasUploader.Stats.m_TotalBytesAccepted = 0;
    NasUploader.Stats.m_TotalBytesUploadedPercent = 0;
    NasUploader.Stats.m_TotalBytesAcceptedPercent = 0;

    $("#checkbox-folder-mode").removeAttr("disabled");

    $("#btn-upload").attr("disabled", true);
    $("#btn-pause").attr("disabled", true);
    $("#btn-resume").attr("disabled", true);

    $("#current-file-name").html("None");

    $("#current-progress-info").html("Current: None");
    $("#current-progress-info-status").attr("hidden", true);
    $("#current-uploaded-progress-bar").width("0%");
    $("#current-accepted-progress-bar").width("0%");

    $("#total-progress-info").html("Total: None");
    $("#total-progress-info-status").attr("hidden", true);
    $("#total-uploaded-progress-bar").width("0%");
    $("#total-accepted-progress-bar").width("0%");

    $("#upload-elapsed-time").html("Elapsed time: 00:00:00");
    $("#upload-remaining-time").html("Remaining time: 00:00:00 (0 B/s)");

    $("#files-list-overview").html("");
    $("#files-list").html("");
}

NasUploader.tryStartNextUpload = function () {
    if (NasUploader.Backend.m_UploadInProgress + 1 < NasUploader.m_Files.length) {
        NasUploader.TusUploader.tryStartUpload(NasUploader.Backend.m_UploadInProgress + 1);
    } else {
        NasUploader.Backend.m_UploadInProgress = -1;
        NasUploader.Stats.m_StartTime = null;

        $("#checkbox-folder-mode").removeAttr("disabled");

        $("#input-files").removeAttr("disabled");
        $("#input-files-folder").removeAttr("disabled");

        $("#button-pause").attr("disabled", true);
        $("#button-resume").attr("disabled", true);
        $("#btn-upload").attr("disabled", true);
    }
}

// ==================================================
// events

function checkbox_folderMode_changed(_element) {
    if (_element == null)
        return;

    resetNasUploader();
    $("#input-files").val(null);
    $("#input-files-folder").val(null);

    NasUploader.Backend.m_IsFolderUploadMode = _element.checked;
    if (_element.checked) {
        $("#input-files").attr("hidden", true);
        $("#input-files-folder").removeAttr("hidden");

        //m_Input = $("#input-files-folder");
    } else {
        $("#input-files-folder").attr("hidden", true);
        $("#input-files").removeAttr("hidden");

        //m_Input = $("#input-files");
    }
}

function input_files_changed(_element) {
    if (_element == null)
        return;

    resetNasUploader();

    NasUploader.m_Files = _element.files;

    if (NasUploader.m_Files == null || NasUploader.m_Files.length <= 0) {
        return;
    }

    if (NasUploader.Backend.m_IsFolderUploadMode) {
        NasUploader.processFolderUploadInfo(NasUploader.m_Files);
    } else {
        NasUploader.processFileUploadInfo(NasUploader.m_Files);
    }

    NasUploader.refreshFileInfoPanel();

    let htmlOverview = "Total :" + NasUploader.m_Files.length + " file"
    if (NasUploader.m_Files.length > 1)
        htmlOverview += "s";
    htmlOverview += " (" + formatDataLength(NasUploader.Stats.m_TotalBytesToUpload) + ")";
    $("#files-list-overview").html(htmlOverview);

    $("#btn-upload").removeAttr("disabled");
}

function btn_pause(_element) {
    if (!NasUploader.m_IsTransfering)
        return;

    NasUploader.m_ActiveTusUpload.abort();
    NasUploader.m_IsTransfering = false;

    $("#current-progress-info-status").removeAttr("hidden");
    $("#total-progress-info-status").removeAttr("hidden");

    $("#checkbox-folder-mode").removeAttr("disabled");

    $("#input-files").removeAttr("disabled");
    $("#input-files-folder").removeAttr("disabled");
}

function btn_resume(_element) {
    if (NasUploader.m_IsTransfering)
        return;

    // TODO: find resumable uploads;
    NasUploader.m_ActiveTusUpload.start();
    NasUploader.m_IsTransfering = true;

    $("#current-progress-info-status").attr("hidden", true);
    $("#total-progress-info-status").attr("hidden", true);

    $("#checkbox-folder-mode").attr("disabled", true);

    $("#input-files").attr("disabled", true);
    $("#input-files-folder").attr("disabled", true);
}

function btn_upload(_element) {
    NasUploader.tryStartNextUpload();

    NasUploader.Backend.m_StartTime = new Date();

    $(_element).attr("disabled", true);

    $("#checkbox-folder-mode").attr("disabled", true);

    $("#input-files").attr("disabled", true);
    $("#input-files-folder").attr("disabled", true);

    $("#btn-pause").removeAttr("disabled");
    $("#btn-resume").removeAttr("disabled");

    $("#total-sent-progress-bar").width("0%");
    $("#total-accepted-progress-bar").width("0%");
}

// END: events
// ==================================================

// ==================================================
// helpers

NasUploader.refreshFileInfoPanel = function () {
    if (NasUploader.Backend.m_DirectoryTree == null)
        return;

    NasUploader.updateDirectoryTree(NasUploader.Backend.m_DirectoryTree);

    let html = NasUploader.buildFileInfoFromTree("", NasUploader.Backend.m_DirectoryTree);

    $("#files-list").html(html);
}

NasUploader.processFileUploadInfo = function (_files) {
    let json = {
        id: null,
        text: "",
        length: 0,
        isLeaf: false,
        state: "idle",
        children: []
    };

    for (let i = 0; i < _files.length; ++i) {
        let file = _files[i];
        let id = "file-" + i;

        json.children.push({
            id: id,
            text: file.name,
            length: file.size,
            isLeaf: true,
            state: "idle",
            children: []
        });

        NasUploader.Backend.m_FilesState[id] = "idle";

        NasUploader.Stats.m_TotalBytesToUpload += _files[i].size;
    }

    NasUploader.Backend.m_DirectoryTree = json;
}

NasUploader.processFolderUploadInfo = function (_files) {
    let pos = _files[0].webkitRelativePath.indexOf("/");
    let parent = _files[0].webkitRelativePath.substring(0, pos);
    let json = {
        id: null,
        text: parent,
        length: 0,
        isLeaf: false,
        state: "idle",
        children: []
    };

    for (let i = 0; i < _files.length; ++i) {
        let path = _files[i].webkitRelativePath;
        pos = path.indexOf("/");
        let name = path.substring(pos + 1);
        let id = "file-" + i;

        json = NasUploader.constructDirectoryTree(json, id, name, _files[i].size);
        NasUploader.Backend.m_FilesState[id] = "idle";

        NasUploader.Stats.m_TotalBytesToUpload += _files[i].size;
    }

    NasUploader.Backend.m_DirectoryTree = json;
}

NasUploader.constructDirectoryTree = function (_json, _id, _name, _length) {
    let pos = _name.indexOf("/");
    if (pos < 0) {
        _json.children.push({
            isLeaf: true,
            id: _id,
            text: _name,
            length: _length,
            state: "idle",
            children: null
        });
        return _json;
    }

    let parent = _name.substring(0, pos);
    let child = _name.substring(pos + 1);
    let jsonChild = null;

    for (let i = 0; i < _json.children.length; ++i) {
        jsonChild = _json.children[i];

        if (jsonChild.isLeaf)
            continue;

        if (parent == jsonChild.text) {
            // same directory;
            jsonChild = NasUploader.constructDirectoryTree(jsonChild, _id, child, _length);
            return _json;
        }
    }

    jsonChild = {
        isLeaf: false,
        id: null,
        text: parent,
        length: 0,
        state: "idle",
        children: []
    };

    jsonChild = NasUploader.constructDirectoryTree(jsonChild, _id, child, _length);
    _json.children.push(jsonChild);

    return _json;
}

NasUploader.updateDirectoryTree = function (_json) {
    if (_json.isLeaf) {
        _json.state = NasUploader.Backend.m_FilesState[_json.id];
        return;
    }

    for (let i = 0; i < _json.children.length; ++i) {
        NasUploader.updateDirectoryTree(_json.children[i]);
    }

    let hasIdle = false;
    let hasError = false;
    let hasTransfering = false;
    let hasSuccess = false;

    for (let i = 0; i < _json.children.length; ++i) {
        let childState = _json.children[i].state;
        if (childState == "error") {
            hasError = true;
            break;
        }

        if (childState == "transfering") {
            hasTransfering = true;
            continue;
        }

        if (childState == "idle") {
            hasIdle = true;
            continue;
        }

        if (childState == "success") {
            hasSuccess = true;
            continue;
        }
    }

    if (hasError)
        _json.state = "error";
    else if (hasTransfering)
        _json.state = "transfering";
    else if (hasSuccess) {
        if (hasIdle)
            _json.state = "transfering";
        else
            _json.state = "success";
    }
}

NasUploader.buildFileInfoFromTree = function (_html, _json) {
    if (!NasUploader.Backend.m_IsFolderUploadMode) {
        for (let i = 0; i < _json.children.length; ++i) {
            _html = NasUploader.buildSingleFileInfo(_html, _json.children[i]);
        }
        return _html;
    }

    // folder upload mode;
    if (_json.isLeaf) {
        // leaf node;
        NasUploader.buildSingleFileInfo(_html, _json);
        return _html;
    }

    // not leaf node;
    _html += "<div class=\"ml-3\">";
    _html += "<div class=\"" + colorClass + "\">" + _json.text + "</div>";

    for (let i = 0; i < _json.children.length; ++i) {
        _html += NasUploader.buildFileInfoFromTree("", _json.children[i]);
    }

    _html += "</div>";

    return _html;
}

NasUploader.updateStatistic = function () {
    if (!NasUploader.m_IsTransfering || NasUploader.Backend.m_StartTime == null)
        return;

    let bytesUploaded = NasUploader.Stats.m_TotalBytesUploaded + NasUploader.Stats.m_CurrentBytesUploaded;
    let elapsed = new Date() - NasUploader.Backend.m_StartTime;
    let avgSpeed = bytesUploaded / elapsed * 1000.0;

    let bytesRemaining = NasUploader.Stats.m_TotalBytesToUpload - bytesUploaded;
    let timeRemaining = bytesRemaining * 1000.0 / avgSpeed;

    $("#upload-elapsed-time").html("Elapsed time: " + formatTimeSpan_Hour(elapsed));
    $("#upload-remaining-time").html("Remaining time: " + formatTimeSpan_Hour(timeRemaining) + " (" + formatDataLength(avgSpeed) + "/s)");

    //console.log(formatTimeSpan_Hour(elapsed) + ", " + formatDataLength(avgSpeed) + "/");
}

NasUploader.markFileAsErrorByIndex = function (_index) {
    if (_index < 0 || _index > NasUploader.m_Files.length) {
        console.warn("Upload file index out of range: " + _index + "/" + NasUploader.m_Files.length + ".");
        return;
    }

    NasUploader.Backend.m_FilesState["file-" + _index] = "error";
    NasUploader.refreshFileInfoPanel();
}

NasUploader.markFileAsCompletedByIndex = function (_index) {
    if (_index < 0 || _index > NasUploader.m_Files.length) {
        console.warn("Upload file index out of range: " + _index + "/" + NasUploader.m_Files.length + ".");
        return;
    }

    NasUploader.Backend.m_FilesState["file-" + _index] = "success";
    NasUploader.refreshFileInfoPanel();
}

NasUploader.markFileAsInProgressByIndex = function (_index) {
    if (_index < 0 || _index > NasUploader.m_Files.length) {
        console.warn("Upload file index out of range: " + _index + "/" + NasUploader.m_Files.length + ".");
        return;
    }

    NasUploader.Backend.m_FilesState["file-" + _index] = "transfering";
    NasUploader.refreshFileInfoPanel();
}

NasUploader.buildSingleFileInfo = function (_html, _json) {
    let colorClass = "";
    if (_json.state == "transfering")
        colorClass = "text-warning ";
    else if (_json.state == "error")
        colorClass = "text-danger ";
    else if (_json.state == "success")
        colorClass = "text-success ";

    _html += "<div class=\"d-flex\">";
    _html += "<div class=\"text-break " + colorClass + "ml-3\">" + _json.text + "</div>";
    _html += "<div class=\"text-nowrap text-right ml-auto pl-2\">" + formatDataLength(_json.length) + "</div>";
    _html += "</div>";
    return _html;
}

// END: helpers
// ==================================================
