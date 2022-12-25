/*  nas-upload-uploader.js
 *  Version: 1.3 (2022.12.25)
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
NasUploader.Backend.m_IsFolderUploadMode = false;

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

    $("#div-input-files").on("dragenter", lblInputFiles_dragIn);
    $("#div-input-files").on("dragend drop dragexit dragleave", lblInputFiles_dragOut);

    NasUploader.updateUploadModeButton();

    window.setInterval(function () {
        NasUploader.updateStatistic();
    }, 200);
});

function resetNasUploader() {
    NasUploader.m_IsTransfering = false;
    NasUploader.m_UploadLocation = $("#current-location").text();
    NasUploader.m_Files = null;

    NasUploader.m_ActiveTusUpload = null;

    NasUploader.Backend.m_FilesState = [];
    NasUploader.Backend.m_DirectoryTree = null;
    NasUploader.Backend.m_StartTime = null;
    NasUploader.Backend.m_UploadInProgress = -1;

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

function btn_uploadMode() {
    resetNasUploader();
    $("#input-files").val(null);
    $("#input-files-folder").val(null);

    NasUploader.Backend.m_IsFolderUploadMode = ~NasUploader.Backend.m_IsFolderUploadMode;

    NasUploader.updateUploadModeButton();
}

function lblInputFiles_dragIn() {
    let lbl = $("#lbl-input-files");

    lbl.addClass("border");
    lbl.addClass("border-success");

    //lbl.attr("style", "border-width:2px!important");
}

function lblInputFiles_dragOut(_element) {
    let lbl = $("#lbl-input-files");

    lbl.removeClass("border");
    lbl.removeClass("border-success");

    //lbl.removeAttr("style");
}

function input_files_changed(_element) {
    if (_element == null)
        return;

    resetNasUploader();
    NasUploader.updateLabelInputFiles(null);

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
    NasUploader.updateLabelInputFiles(htmlOverview);

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
    let colorClass = NasUploader.getColorClassBasedOnState(_json.state);

    if (_json.isLeaf) {
        // leaf node;
        _html = NasUploader.buildSingleFileInfo(_html, _json);
        return _html;
    }

    // not leaf node;
    _html += "<div class=\"text-truncate " + colorClass + "\"><b>" + _json.text + "</b></div>";
    _html += "<div class=\"ml-3\">";

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
    let colorClass = NasUploader.getColorClassBasedOnState(_json.state);

    _html += "<div class=\"d-flex\">";
    _html += "<div class=\"text-break " + colorClass + "\">" + _json.text + "</div>";
    _html += "<div class=\"text-nowrap text-right ml-auto pl-2\">" + formatDataLength(_json.length) + "</div>";
    _html += "</div>";
    return _html;
}

NasUploader.getColorClassBasedOnState = function (_state) {
    if (_state == "transfering")
        return "text-warning ";

    if (_state == "error")
        return "text-danger ";

    if (_state == "success")
        return "text-success ";

    return "";
}

NasUploader.extractFilePath = function (_file) {
    let path = _file.webkitRelativePath.replace(_file.name, "");
    if (path.endsWith("/"))
        path = path.substring(0, path.length - 1);

    return path;
}

NasUploader.updateUploadModeButton = function () {
    let svgPath = "";
    let biClass = "";

    if (NasUploader.Backend.m_IsFolderUploadMode) {
        svgPath = "<path d=\"M.54 3.87.5 3a2 2 0 0 1 2-2h3.672a2 2 0 0 1 1.414.586l.828.828A2 2 0 0 0 9.828 3h3.982a2 2 0 0 1 1.992 2.181l-.637 7A2 2 0 0 1 13.174 14H2.826a2 2 0 0 1-1.991-1.819l-.637-7a1.99 1.99 0 0 1 .342-1.31zM2.19 4a1 1 0 0 0-.996 1.09l.637 7a1 1 0 0 0 .995.91h10.348a1 1 0 0 0 .995-.91l.637-7A1 1 0 0 0 13.81 4H2.19zm4.69-1.707A1 1 0 0 0 6.172 2H2.5a1 1 0 0 0-1 .981l.006.139C1.72 3.042 1.95 3 2.19 3h5.396l-.707-.707z\" />"
        biClass = "folder";

        $("#input-files").attr("hidden", true);
        $("#input-files-folder").removeAttr("hidden");
    } else {
        svgPath = "<path d=\"M13 0H6a2 2 0 0 0-2 2 2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h7a2 2 0 0 0 2-2 2 2 0 0 0 2-2V2a2 2 0 0 0-2-2zm0 13V4a2 2 0 0 0-2-2H5a1 1 0 0 1 1-1h7a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1zM3 4a1 1 0 0 1 1-1h7a1 1 0 0 1 1 1v10a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V4z\" />"
        biClass = "files";

        $("#input-files-folder").attr("hidden", true);
        $("#input-files").removeAttr("hidden");
    }

    let html = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-" + biClass + "\" viewBox=\"0 0 16 16\">"
    html += svgPath;
    html += "</svg>";
    $("#btn-upload-mode").html(html);

    NasUploader.updateLabelInputFiles(null);
}

NasUploader.updateLabelInputFiles = function (_text) {
    if (_text != null && _text != "") {
        $("#lbl-input-files").html(_text);
        return;
    }

    if (NasUploader.Backend.m_IsFolderUploadMode) {
        $("#lbl-input-files").attr("for", "input-files-folder");
        $("#lbl-input-files").html("Select or drop folder here..");
    }
    else {
        $("#lbl-input-files").attr("for", "input-files");
        $("#lbl-input-files").html("Select or drop files here..");
    }
}

// END: helpers
// ==================================================
