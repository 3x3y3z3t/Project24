/*  nas-upload.js
 *  Version: 1.2 (2023.01.30)
 *
 *  Contributor
 *      Arime-chan
 */

// =====
window.NasUploadPage = {};

NasUploadPage.m_CurrentLocation = "";

NasUploadPage.Elements = {};
NasUploadPage.Elements.m_InputNewFolder = null;
NasUploadPage.Elements.m_BtnNewFolderReset = null;
NasUploadPage.Elements.m_BtnNewFolderSubmit = null;

NasUploadPage.ManagePanel = {};
NasUploadPage.ManagePanel.m_Path = null;
NasUploadPage.ManagePanel.m_FileType = null;
NasUploadPage.ManagePanel.m_OrgFileName = null;
NasUploadPage.ManagePanel.m_OrgFileExtension = null;

NasUploadPage.ManagePanel.Elements = {};
NasUploadPage.ManagePanel.Elements.m_Modal = null;
NasUploadPage.ManagePanel.Elements.m_Header = null;
NasUploadPage.ManagePanel.Elements.m_InputRename = null;
NasUploadPage.ManagePanel.Elements.m_BtnRenameReset = null;
NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit = null;
NasUploadPage.ManagePanel.Elements.m_InputDst = null;
NasUploadPage.ManagePanel.Elements.m_BtnCopyTo = null;
NasUploadPage.ManagePanel.Elements.m_BtnMoveTo = null;
NasUploadPage.ManagePanel.Elements.m_BtnDelete = null;


$(document).ready(function () {
    NasUploadPage.updateBrowserPanel();
    NasUploadPage.updateUploadLocation();

    NasUploadPage.ManagePanel.Elements.m_Modal = $("#modal-manage");
    NasUploadPage.ManagePanel.Elements.m_Header = $("#modal-manage-header");

    NasUploadPage.ManagePanel.Elements.m_InputRename = $("#modal-manage-input-rename");
    NasUploadPage.ManagePanel.Elements.m_BtnRenameReset = $("#modal-manage-btn-rename-reset");
    NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit = $("#modal-manage-btn-rename-submit");

    NasUploadPage.ManagePanel.Elements.m_InputDst = $("#modal-manage-input-dst");
    NasUploadPage.ManagePanel.Elements.m_BtnCopyTo = $("#modal-manage-btn-copy");
    NasUploadPage.ManagePanel.Elements.m_BtnMoveTo = $("#modal-manage-btn-move");
    NasUploadPage.ManagePanel.Elements.m_BtnDelete = $("#modal-manage-btn-delete");

    NasUploadPage.ManagePanel.Elements.m_Modal.on("hide.bs.modal", function (_e) {
        NasUploadPage.ManagePanel.m_Path = null;
        NasUploadPage.ManagePanel.m_FileType = null;
        NasUploadPage.ManagePanel.m_OrgFileName = null;
        NasUploadPage.ManagePanel.m_OrgFileExtension = null;

        NasUploadPage.resetRenameForm();
    });

    let tooltipTemplate =
        "<div class=\"tooltip\" role=\"tooltip\">" +
        "<div class=\"arrow\"></div>" +
        "<div class=\"tooltip-inner text-left text-danger border border-dark tooltip-custom-nas\"></div>" +
        "</div>";

    NasUploadPage.Elements.m_InputNewFolder.tooltip({
        animation: true,
        container: "body",
        html: true,
        placement: "bottom",
        trigger: "manual",
        template: tooltipTemplate
    });

    NasUploadPage.ManagePanel.Elements.m_InputRename.tooltip({
        animation: true,
        container: "body",
        html: true,
        placement: "bottom",
        trigger: "manual",
        template: tooltipTemplate
    });

    $(document).on("click", ".a-tooltip-naming", function () {
        $("#modal-naming-rules").modal();
    });

});


function browseNas(_path) {
    NasUploadPage.ajax_browseNas(_path);
}

NasUploadPage.openManagePanel = function (_filename, _filetype) {
    NasUploadPage.ManagePanel.m_Path = NasUploadPage.m_CurrentLocation
    NasUploadPage.ManagePanel.m_FileType = _filetype;
    NasUploadPage.ManagePanel.m_OrgFileName = _filename;
    if (_filetype == "File") {
        NasUploadPage.ManagePanel.m_OrgFileExtension = P24Utils.getFileExtension(_filename);
    }

    NasUploadPage.ManagePanel.Elements.m_Header.html(_filename);
    NasUploadPage.ManagePanel.Elements.m_InputRename.val(_filename);
    NasUploadPage.ManagePanel.Elements.m_InputDst.val(NasUploadPage.ManagePanel.m_Path);

    modalManage_inputDst_onInput();

    NasUploadPage.ManagePanel.Elements.m_Modal.modal();
}

// ==================================================
// ajax request sender

NasUploadPage.ajax_browseNas = function (_path) {
    $.ajax({
        type: "GET",
        url: "/Nas/Upload/?handler=Browse&_path=" + _path,
        success: function (_content, _textStatus, _xhr) {
            NasUploadPage.updateBrowserPanel(_content);
            NasUploadPage.updateUploadLocation();
        },
        error: P24Utils.ajax_error
    });
}

NasUploadPage.ajax_createNewFolder = function (_path, _folderName) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/Nas/Manage?handler=CreateFolder",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify({
            Item1: _path,
            Item2: _folderName,
        }),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: NasUploadPage.ajax_createNewFolder_success,
        error: function (_xhr, _textStatus, _errorThrown) {
            NasUploadPage.releaseBtnNewFolderSubmit();
            P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
        }
    });
}

NasUploadPage.ajax_rename = function (_path, _newName, _fileType) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/Nas/Manage?handler=Rename",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify({
            Item1: _path,
            Item2: _newName,
        }),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: function (_content, _textStatus, _xhr) {
            if (!NasUploadPage.ajax_rename_success(_content, _textStatus, _xhr)) {
                return;
            }

            NasUploadPage.updateBrowserPanel(_content);
            NasUploadPage.updateUploadLocation();

            NasUploadPage.ManagePanel.Elements.m_Modal.modal("hide");
            NasUploadPage.openManagePanel(_newName, _fileType);
        },
        error: function (_xhr, _textStatus, _errorThrown) {
            NasUploadPage.releaseBtnRenameSubmit();
            P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
        }
    });
}

NasUploadPage.ajax_copyTo = function (_srcDir, _dstDir) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/Nas/Manage?handler=CopyTo",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify({
            Item1: _srcDir,
            Item2: _dstDir,
        }),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: NasUploadPage.ajax_copyTo_success,
        error: function (_xhr, _textStatus, _errorThrown) {
            NasUploadPage.releaseBtnCopyTo();
            P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
        }
    });
}

NasUploadPage.ajax_moveTo = function (_srcDir, _dstDir) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/Nas/Manage?handler=MoveTo",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify({
            Item1: _srcDir,
            Item2: _dstDir,
        }),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: NasUploadPage.ajax_moveTo_success,
        error: function (_xhr, _textStatus, _errorThrown) {
            NasUploadPage.releaseBtnMoveTo();
            P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
        }
    });
}

NasUploadPage.ajax_delete = function (_path) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/Nas/Manage?handler=Delete",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify({
            Item1: _path,
            Item2: "",
        }),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: NasUploadPage.ajax_delete_success,
        error: function (_xhr, _textStatus, _errorThrown) {
            NasUploadPage.releaseBtnDelete();
            P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
        }
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// events

function inputNewFolder_onInput() {
    let name = NasUploadPage.Elements.m_InputNewFolder.val();

    if (name == "") {
        NasUploadPage.Elements.m_InputNewFolder.tooltip("hide");
        NasUploadPage.Elements.m_BtnNewFolderReset.attr("disabled", true);
        NasUploadPage.Elements.m_BtnNewFolderSubmit.attr("disabled", true);
    } else if (NasUploadPage.isFileNameValid(name)) {
        NasUploadPage.Elements.m_InputNewFolder.tooltip("hide");
        NasUploadPage.Elements.m_BtnNewFolderReset.removeAttr("disabled");
        NasUploadPage.Elements.m_BtnNewFolderSubmit.removeAttr("disabled");
    } else {
        NasUploadPage.Elements.m_InputNewFolder.tooltip("show");
        NasUploadPage.Elements.m_BtnNewFolderReset.attr("disabled", true);
        NasUploadPage.Elements.m_BtnNewFolderSubmit.attr("disabled", true);
    }
}

function btnNewFolderReset_onClick() {
    NasUploadPage.Elements.m_InputNewFolder.val("");
    NasUploadPage.Elements.m_BtnNewFolderReset.attr("disabled", true);
    NasUploadPage.Elements.m_BtnNewFolderSubmit.attr("disabled", true);
}

function btnNewFolderSubmit_onClick() {
    let folderName = NasUploadPage.Elements.m_InputNewFolder.val();

    NasUploadPage.Elements.m_BtnNewFolderSubmit.attr("disabled", true);
    NasUploadPage.ajax_createNewFolder(NasUploadPage.m_CurrentLocation, folderName);
}

function modalManage_inputRename_onInput() {
    let name = NasUploadPage.ManagePanel.Elements.m_InputRename.val();

    if (name == NasUploadPage.ManagePanel.m_OrgFileName) {
        NasUploadPage.ManagePanel.Elements.m_InputRename.removeClass("text-success");
        NasUploadPage.ManagePanel.Elements.m_BtnRenameReset.attr("disabled", true);
        NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit.attr("disabled", true);
    } else {
        NasUploadPage.ManagePanel.Elements.m_InputRename.addClass("text-success");
        NasUploadPage.ManagePanel.Elements.m_BtnRenameReset.removeAttr("disabled");
        NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit.removeAttr("disabled");

        if (name == "") {
            NasUploadPage.ManagePanel.Elements.m_InputRename.tooltip("hide");
            NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit.attr("disabled", true);
        }
        else if (NasUploadPage.isFileNameValid(name)) {
            NasUploadPage.ManagePanel.Elements.m_InputRename.tooltip("hide");
            NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit.removeAttr("disabled");
        } else {
            NasUploadPage.ManagePanel.Elements.m_InputRename.tooltip("show");
            NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit.attr("disabled", true);
        }
    }
}

function modalManage_btnRenameReset_onClick() {
    NasUploadPage.resetRenameForm();
}

function modalManage_btnRenameSubmit_onClick() {
    if (NasUploadPage.ManagePanel.m_FileType != "File") {
        NasUploadPage.submitRename();
        return;
    }

    let ext = getFileExtension(NasUploadPage.ManagePanel.Elements.m_InputRename.val());
    if (ext != NasUploadPage.ManagePanel.m_OrgFileExtension) {
        Modals.Common.Confirm.openModal(
            "Rename",
            "<div>If you change a file name extension, the file might become unusable.</div><div>Are you sure you want to change it?</div>",
            "NasUploadPage.submitRename()"
        );
    }
}

function modalManage_inputDst_onInput() {
    let dstDir = NasUploadPage.ManagePanel.Elements.m_InputDst.val();

    if (dstDir == NasUploadPage.ManagePanel.m_Path) {
        NasUploadPage.ManagePanel.Elements.m_BtnCopyTo.children("span").html("Make Copy");

        NasUploadPage.ManagePanel.Elements.m_BtnMoveTo.attr("disabled", true);
        NasUploadPage.ManagePanel.Elements.m_BtnMoveTo.children("span").html("Same Directory");
    } else {
        NasUploadPage.ManagePanel.Elements.m_BtnCopyTo.children("span").html("Copy To");

        NasUploadPage.ManagePanel.Elements.m_BtnMoveTo.removeAttr("disabled");
        NasUploadPage.ManagePanel.Elements.m_BtnMoveTo.children("span").html("Move To");
    }
}

function modalManage_btnCopy_onClick() {
    let dstDir = NasUploadPage.ManagePanel.Elements.m_InputDst.val();

    if (dstDir == NasUploadPage.ManagePanel.m_Path) {
        Modals.Common.Confirm.openModal(
            "Make Copy",
            "<div>Destination direntory is the same as current directory.</div><div class=\"text-primary\">Are you sure you want to make copy of selected item?</div>",
            "NasUploadPage.submitCopy()"
        );

        return;
    }

    NasUploadPage.submitCopy();
}

function modalManage_btnMove_onClick() {
    let dstDir = NasUploadPage.ManagePanel.Elements.m_InputDst.val();
    if (dstDir == NasUploadPage.ManagePanel.m_Path) {
        return;
    }

    NasUploadPage.submitMove();
}

function modalManage_btnDelete_onClick() {
    let msg = "<div>Are you sure you want to delete this " + NasUploadPage.ManagePanel.m_FileType.toLowerCase() + "?</div>";
    msg += "<div class=\"ml-3\">Name: <code>" + NasUploadPage.ManagePanel.m_OrgFileName + "</code></div>";
    // TODO: add more metadata;

    if (NasUploadPage.ManagePanel.m_FileType == "Directory") {
        msg += "<br/><div class=\"text-danger\">All subdirectories will be deleted as well!</div>";
    }

    Modals.Common.Confirm.openModal(
        "Delete",
        msg,
        "NasUploadPage.submitDelete()"
    );
}

NasUploadPage.ajax_createNewFolder_success = function (_content, _textStatus, _xhr) {
    NasUploadPage.releaseBtnNewFolderSubmit();

    if (_content.startsWith("<fail>")) {
        let body = _content.substring(6);
        if (body.startsWith("<excp>")) {
            body = "<pre>" + body.substring(6) + "<pre>";
        }

        body = "<div>" + body + "<div>";

        Modals.CommonInfoModal.openErrorModal("Create New Folder failed", body, null);
        return;
    }

    if (_content.startsWith("<warn>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openWarningModal("Create New Folder failed", body, null);
        return;
    }

    NasUploadPage.updateBrowserPanel(_content);
    NasUploadPage.updateUploadLocation();

    $("#new-folder-jump")[0].scrollIntoView();
}

NasUploadPage.ajax_rename_success = function (_content, _textStatus, _xhr) {
    NasUploadPage.releaseBtnRenameSubmit();

    if (_content.startsWith("<fail>")) {
        let body = _content.substring(6);
        if (body.startsWith("<excp>")) {
            body = "<pre>" + body.substring(6) + "<pre>";
        }

        body = "<div>" + body + "<div>";

        Modals.CommonInfoModal.openErrorModal("Rename failed", body, null);
        return false;
    }

    if (_content.startsWith("<warn>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openWarningModal("Rename failed", body, null);
        return false;
    }

    return true;
}

NasUploadPage.ajax_copyTo_success = function (_content, _textStatus, _xhr) {
    NasUploadPage.releaseBtnCopyTo();

    if (_content.startsWith("<fail>")) {
        let body = _content.substring(6);
        if (body.startsWith("<excp>")) {
            body = "<pre>" + body.substring(6) + "<pre>";
        }

        body = "<div>" + body + "<div>";

        Modals.CommonInfoModal.openErrorModal("Copy failed", body, null);
        return;
    }

    if (_content.startsWith("<warn>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openWarningModal("Copy failed", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        let body = "<div>Item has been copied.</div>" +
            "<div class=\"ml-3\">Name: <code>" + NasUploadPage.ManagePanel.m_OrgFileName + "</code></div> " +
            "<div class=\"ml-3\">Destination: <code>" + NasUploadPage.ManagePanel.Elements.m_InputDst.val() + "</code></div> ";
        Modals.CommonInfoModal.openInfoModal("Copy success", body, null);
        return;
    }

    NasUploadPage.updateBrowserPanel(_content);
    NasUploadPage.updateUploadLocation();

    $("#new-folder-jump")[0].scrollIntoView();

    return;
}

NasUploadPage.ajax_moveTo_success = function (_content, _textStatus, _xhr) {
    NasUploadPage.releaseBtnCopyTo();

    if (_content.startsWith("<fail>")) {
        let body = _content.substring(6);
        if (body.startsWith("<excp>")) {
            body = "<pre>" + body.substring(6) + "<pre>";
        }

        body = "<div>" + body + "<div>";

        Modals.CommonInfoModal.openErrorModal("Move failed", body, null);
        return;
    }

    if (_content.startsWith("<warn>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openWarningModal("Move failed", body, null);
        return;
    }

    NasUploadPage.updateBrowserPanel(_content);
    NasUploadPage.updateUploadLocation();

    let body = "<div>Item has been moved.</div>" +
        "<div class=\"ml-3\">Name: <code>" + NasUploadPage.ManagePanel.m_OrgFileName + "</code></div> " +
        "<div class=\"ml-3\">Destination: <code>" + NasUploadPage.ManagePanel.Elements.m_InputDst.val() + "</code></div> ";
    Modals.CommonInfoModal.openInfoModal("Move success", body, null);
    return;
}

NasUploadPage.ajax_delete_success = function (_content, _textStatus, _xhr) {
    NasUploadPage.releaseBtnDelete();

    if (_content.startsWith("<fail>")) {
        let body = _content.substring(6);
        if (body.startsWith("<excp>")) {
            body = "<pre>" + body.substring(6) + "<pre>";
        }

        body = "<div>" + body + "<div>";

        Modals.CommonInfoModal.openErrorModal("Delete failed", body, null);
        return;
    }

    if (_content.startsWith("<warn>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openWarningModal("Delete failed", body, null);
        return;
    }

    NasUploadPage.updateBrowserPanel(_content);
    NasUploadPage.updateUploadLocation();

    NasUploadPage.ManagePanel.Elements.m_Modal.modal("hide");
}


// END: events
// ==================================================

// ==================================================
// helper

NasUploadPage.releaseBtnNewFolderSubmit = function () {
    NasUploadPage.Elements.m_BtnNewFolderSubmit.removeAttr("disabled");
}

NasUploadPage.releaseBtnRenameSubmit = function () {
    NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit.removeAttr("disabled");
}

NasUploadPage.releaseBtnDelete = function () {
    NasUploadPage.ManagePanel.Elements.m_BtnDelete.removeAttr("disabled");
}

NasUploadPage.releaseBtnCopyTo = function () {
    NasUploadPage.ManagePanel.Elements.m_BtnCopyTo.removeAttr("disabled");
}

NasUploadPage.releaseBtnMoveTo = function () {
    NasUploadPage.ManagePanel.Elements.m_BtnMoveTo.removeAttr("disabled");
}

NasUploadPage.updateBrowserPanel = function (_content = null) {
    if (_content != null)
        $("#nas-browser").html(_content);

    NasUploadPage.Elements.m_InputNewFolder = $("#input-new-folder");
    NasUploadPage.Elements.m_BtnNewFolderReset = $("#btn-new-folder-reset");
    NasUploadPage.Elements.m_BtnNewFolderSubmit = $("#btn-new-folder-submit");
}

NasUploadPage.updateUploadLocation = function () {
    if (window.NasUploader.m_ActiveTusUpload != null)
        return;

    NasUploadPage.m_CurrentLocation = $("#current-location").text();
    window.NasUploader.m_UploadLocation = NasUploadPage.m_CurrentLocation;
    $("#upload-location").html("root/" + NasUploadPage.m_CurrentLocation);
}

NasUploadPage.resetRenameForm = function () {
    NasUploadPage.ManagePanel.Elements.m_InputRename.val(NasUploadPage.ManagePanel.m_OrgFileName);
    NasUploadPage.ManagePanel.Elements.m_InputRename.removeClass("text-success");
    NasUploadPage.ManagePanel.Elements.m_BtnRenameReset.attr("disabled", true);
    NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit.attr("disabled", true);
}

NasUploadPage.submitRename = function () {
    NasUploadPage.ManagePanel.Elements.m_BtnRenameSubmit.attr("disabled", true);

    let path = NasUploadPage.ManagePanel.m_OrgFileName;
    if (NasUploadPage.ManagePanel.m_Path != "")
        path = NasUploadPage.ManagePanel.m_Path + "/" + path;
    let newName = NasUploadPage.ManagePanel.Elements.m_InputRename.val();
    let fileType = NasUploadPage.ManagePanel.m_FileType;

    NasUploadPage.ajax_rename(path, newName, fileType);
}

NasUploadPage.submitCopy = function () {
    NasUploadPage.ManagePanel.Elements.m_BtnCopyTo.attr("disabled", true);

    let srcDir = NasUploadPage.ManagePanel.m_OrgFileName;
    if (NasUploadPage.ManagePanel.m_Path != "")
        srcDir = NasUploadPage.ManagePanel.m_Path + "/" + srcDir;

    let dstDir = NasUploadPage.ManagePanel.Elements.m_InputDst.val();

    NasUploadPage.ajax_copyTo(srcDir, dstDir);
}

NasUploadPage.submitMove = function () {
    NasUploadPage.ManagePanel.Elements.m_BtnMoveTo.attr("disabled", true);

    let srcDir = NasUploadPage.ManagePanel.m_OrgFileName;
    if (NasUploadPage.ManagePanel.m_Path != "")
        srcDir = NasUploadPage.ManagePanel.m_Path + "/" + srcDir;

    let dstDir = NasUploadPage.ManagePanel.Elements.m_InputDst.val();

    NasUploadPage.ajax_moveTo(srcDir, dstDir);
}

NasUploadPage.submitDelete = function () {
    NasUploadPage.ManagePanel.Elements.m_BtnDelete.attr("disabled", true);

    let path = NasUploadPage.ManagePanel.m_OrgFileName;
    if (NasUploadPage.ManagePanel.m_Path != "")
        path = NasUploadPage.ManagePanel.m_Path + "/" + path;
    let fileType = NasUploadPage.ManagePanel.m_FileType;

    NasUploadPage.ajax_delete(path, fileType);
}

NasUploadPage.isFileNameValid = function (_filename, _isFolder = false) {
    /* Reference:
     * https://learn.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions
     */

    // file name cannot be all white spaces;
    if (_filename.trim() == "")
        return false;

    // file name cannot be "." (Current Directory) or ".." (Parent Directory);
    if (_filename == "." || _filename == "..")
        return false;

    // file name cannot end with white space or period character;
    if (_filename.endsWith(' ') || _filename.endsWith('.'))
        return false;

    // file name cannot contains any of invalid characters;
    for (let i = 0; i < P24Utils.InvalidFilenameChars.length; ++i) {
        if (_filename.indexOf(P24Utils.InvalidFilenameChars[i]) >= 0)
            return false;
    }

    // file name cannot be any of reserved names;
    _filename = _filename.toUpperCase();
    for (let i = 0; i < P24Utils.InvalidFilenameString.length; ++i) {

        if (_filename == P24Utils.InvalidFilenameString[i] >= 0)
            return false;

        if (!_isFolder && _filename.startsWith(P24Utils.InvalidFilenameString[i] + "."))
            return false;
    }

    return true;
}

// END: helpers
// ==================================================
