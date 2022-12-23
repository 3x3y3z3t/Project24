/*  nas-upload.js
 *  Version: 1.1 (2022.12.23)
 *
 *  Contributor
 *      Arime-chan
 */

// =====
window.ConfigPanel = {};

ConfigPanel.m_Path = null;
ConfigPanel.m_FileType = null;
ConfigPanel.m_OrgFileName = null;
ConfigPanel.m_OrgFileExtension = null;

ConfigPanel.m_InputRename = null;
ConfigPanel.m_BtnRenameReset = null;
ConfigPanel.m_BtnRenameSubmit = null;

ConfigPanel.m_BtnCopyTo = null;
ConfigPanel.m_BtnMoveTo = null;
ConfigPanel.m_BtnDelete = null;

// =====
window.NewFolderInput = {};

NewFolderInput.m_Input = null;
NewFolderInput.m_BtnReset = null;
NewFolderInput.m_BtnSubmit = null;

// =====
window.ModalConfirm = {}

ModalConfirm.m_ModalConfirmTitle = null;
ModalConfirm.m_ModalConfirmMsg = null;
ModalConfirm.m_BtnConfirmSubmit = null;


$(document).ready(function () {
    updateBrowserPanel();
    updateUploadLocation();

    ConfigPanel.m_InputRename = $("#modal-config-input-rename");
    ConfigPanel.m_BtnRenameReset = $("#modal-config-btn-rename-reset");
    ConfigPanel.m_BtnRenameSubmit = $("#modal-config-btn-rename-submit");

    ConfigPanel.m_BtnCopyTo = $("#modal-config-btn-copy");
    ConfigPanel.m_BtnMoveTo = $("#modal-config-btn-move");
    ConfigPanel.m_BtnDelete = $("#modal-config-btn-delete");

    ModalConfirm.m_ModalConfirmTitle = $("#modal-confirm-title");
    ModalConfirm.m_ModalConfirmMsg = $("#modal-confirm-msg");
    ModalConfirm.m_BtnConfirmSubmit = $("#modal-confirm-btn-submit");

    $("#modal-config").on('hide.bs.modal', function (_e) {
        ConfigPanel.m_Path = null;
        ConfigPanel.m_FileType = null;
        ConfigPanel.m_OrgFileName = null;
        ConfigPanel.m_OrgFileExtension = null;

        modalConfig_btnRenameReset();

        ModalConfirm.m_ModalConfirmTitle.html("");
        ModalConfirm.m_ModalConfirmMsg.html("");
        ModalConfirm.m_BtnConfirmSubmit.off(".modal");
    });

});

function openCfgPanel(_fileName, _fileType) {
    ConfigPanel.m_Path = $("#current-location").text();
    ConfigPanel.m_FileType = _fileType;
    ConfigPanel.m_OrgFileName = _fileName;
    if (_fileType == "File") {
        ConfigPanel.m_OrgFileExtension = getFileExtension(_fileName);
    }

    ConfigPanel.m_InputRename.val(_fileName);

    $("#modal-config-title").html(_fileName);

    $("#modal-config").modal();
}

function browseNas(_path) {
    //$("#modal-browsing-path").html(_path);
    //$("#modal-browsing").modal();

    $.ajax({
        type: 'GET',
        url: "/Nas/Upload/?handler=Browse&_path=" + _path,
        success: function (_content) {
            $("#nas-browser").html(_content);
            updateBrowserPanel();
            updateUploadLocation();
        },
        error: function () {
            window.alert("Could not fetch browser.");
        }
    });
}

// ==================================================
// ajax request sender

function ajax_createNewFolder(_path, _folderName) {
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
        success: function (_content) {
            $("#nas-browser").html(_content);
            updateBrowserPanel();
            updateUploadLocation();
        },
        error: function () {
            window.alert("Could not fetch browser.");
        }
    });
}

function ajax_rename(_path, _newName, _fileType) {
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
        success: function (_content) {
            $("#nas-browser").html(_content);
            updateBrowserPanel();
            updateUploadLocation();

            $("#modal-config").modal("hide");
            openCfgPanel(_newName, _fileType);
        },
        error: function () {
            window.alert("Could not fetch browser.");
        }
    });
}

function ajax_moveTo(_src, _dst) {
    let token = $("input[name='__RequestVerificationToken']").val();

    //TODO: ajax
}

function ajax_copyTo(_src, _dst) {
    let token = $("input[name='__RequestVerificationToken']").val();

    //TODO: ajax
}

function ajax_delete(_path) {
    let token = $("input[name='__RequestVerificationToken']").val();

    //TODO: ajax
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
        success: function (_content) {
            $("#nas-browser").html(_content);
            updateBrowserPanel();
            updateUploadLocation();

            $("#modal-config").modal("hide");
        },
        error: function () {
            window.alert("Could not fetch browser.");
        }
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// events

function inputNewFolder_typing() {
    let name = NewFolderInput.m_Input.val();
    if (name == "") {
        NewFolderInput.m_BtnReset.attr("disabled", true);
        NewFolderInput.m_BtnSubmit.attr("disabled", true);
    } else {
        NewFolderInput.m_BtnReset.removeAttr("disabled");
        NewFolderInput.m_BtnSubmit.removeAttr("disabled");
    }
}

function btnNewFolder_reset() {
    NewFolderInput.m_Input.val("");
    NewFolderInput.m_BtnReset.attr("disabled", true);
    NewFolderInput.m_BtnSubmit.attr("disabled", true);
}

function btnNewFolder_submit() {
    let path = $("#current-location").text();
    let folderName = NewFolderInput.m_Input.val();

    ajax_createNewFolder(path, folderName);
}

function modalConfig_inputRename_typing() {
    if (ConfigPanel.m_InputRename.val() == ConfigPanel.m_OrgFileName) {
        ConfigPanel.m_InputRename.removeClass("text-success");
        ConfigPanel.m_BtnRenameReset.attr("disabled", true);
        ConfigPanel.m_BtnRenameSubmit.attr("disabled", true);
    } else {
        ConfigPanel.m_InputRename.addClass("text-success");
        ConfigPanel.m_BtnRenameReset.removeAttr("disabled");
        ConfigPanel.m_BtnRenameSubmit.removeAttr("disabled");
    }
}

function modalConfig_btnRenameReset() {
    ConfigPanel.m_InputRename.val(ConfigPanel.m_OrgFileName);
    ConfigPanel.m_InputRename.removeClass("text-success");
    ConfigPanel.m_BtnRenameReset.attr("disabled", true);
    ConfigPanel.m_BtnRenameSubmit.attr("disabled", true);
}

function modalConfig_btnRenameBeforeSubmit() {
    if (ConfigPanel.m_FileType != "File") {
        modalConfig_btnRenameSubmit();
        return;
    }

    let ext = getFileExtension(ConfigPanel.m_InputRename.val());
    if (ext != ConfigPanel.m_OrgFileExtension) {
        ModalConfirm.m_ModalConfirmTitle.html("Rename");
        ModalConfirm.m_ModalConfirmMsg.html(
            "<div>If you change a file name extension, the file might become unusable.</div><div>Are you sure you want to change it?</div>"
        );
        ModalConfirm.m_BtnConfirmSubmit.on("click.modal", modalConfig_btnRenameSubmit);

        $("#modal-confirm").modal();
    }
}

function modalConfig_btnRenameSubmit() {
    let path = ConfigPanel.m_OrgFileName;
    if (ConfigPanel.m_Path != "")
        path = ConfigPanel.m_Path + "/" + path;
    let newName = ConfigPanel.m_InputRename.val();
    let fileType = ConfigPanel.m_FileType;

    ajax_rename(path, newName, fileType);
}



//TODO: Copy, Move;




function modalConfig_btnDeleteBeforeSubmit() {
    let msg = "<div>Are you sure you want to delete this " + ConfigPanel.m_FileType.toLowerCase() + "?</div>";
    msg += "<div>" + ConfigPanel.m_OrgFileName + "</div>";
    // TODO: add more metadata;

    if (ConfigPanel.m_FileType == "Directory") {
        msg += "<br/><div class=\"text-danger\">All subdirectories will be deleted as well!</div>";
    }

    ModalConfirm.m_ModalConfirmTitle.html("Delete");
    ModalConfirm.m_ModalConfirmMsg.html(msg);
    ModalConfirm.m_BtnConfirmSubmit.on("click.modal", modalConfig_btnDeleteSubmit);

    $("#modal-confirm").modal();
}

function modalConfig_btnDeleteSubmit() {
    let path = ConfigPanel.m_OrgFileName;
    if (ConfigPanel.m_Path != "")
        path = ConfigPanel.m_Path + "/" + path;
    let fileType = ConfigPanel.m_FileType;

    ajax_delete(path, fileType);
}

// END: events
// ==================================================

// ==================================================
// helper

function updateUploadLocation() {
    if (window.NasUploader.m_ActiveTusUpload != null)
        return;

    NasUploader.m_UploadLocation = $("#current-location").text();
    $("#upload-location").html("root/" + NasUploader.m_UploadLocation);
    //window.alert(newLocation);
}

function updateBrowserPanel() {
    NewFolderInput.m_Input = $("#input-new-folder");
    NewFolderInput.m_BtnReset = $("#btn-new-folder-reset");
    NewFolderInput.m_BtnSubmit = $("#btn-new-folder-submit");

}

// END: helpers
// ==================================================
