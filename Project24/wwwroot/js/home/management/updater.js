/*  home/management/updater.js
    Version: v1.1 (2023.09.02)

    Author
        Arime-chan
 */

const UPDATER_HEADER_BUFFER = 5 * 1024 * 1024; // max header buffer size is 5 KiB;

// ==================================================
// Upload File Status Codes;

const UPDATER_FILE_STATUS_NO_CHANGE = P24_TEXT_COLOR_GREEN;
const UPDATER_FILE_STATUS_CREATE = P24_TEXT_COLOR_NORMAL;
const UPDATER_FILE_STATUS_UPDATE = P24_TEXT_COLOR_YELLOW;
const UPDATER_FILE_STATUS_ERROR = P24_TEXT_COLOR_RED;

// ==================================================
// Batch Status Codes;

const UPDATER_BATCH_STATUS_NOT_STARTED = 0;
const UPDATER_BATCH_STATUS_IN_PROGRESS = 1;
const UPDATER_BATCH_STATUS_SUCCESS = 3;
const UPDATER_BATCH_STATUS_ERROR = 4;

// ==================================================
// Updater Status Codes;

const UPDATER_STATUS_NONE = 0;

const UPDATER_STATUS_PREV_PURGE_QUEUED = -1;
const UPDATER_STATUS_PREV_PURGE_RUNNING = -2;
const UPDATER_STATUS_PREV_APPLY_QUEUED = -3;
const UPDATER_STATUS_PREV_APPLY_RUNNING = -4;

const UPDATER_STATUS_NEXT_PURGE_QUEUED = 1;
const UPDATER_STATUS_NEXT_PURGE_RUNNING = 2;
const UPDATER_STATUS_NEXT_APPLY_QUEUED = 3;
const UPDATER_STATUS_NEXT_APPLY_RUNNING = 4;

// ==================================================
// Misc;

const UPDATER_BLOCK_NAME_PREV = "Prev";
const UPDATER_BLOCK_NAME_NEXT = "Next";


// ==================================================
window.Updater = {};

Updater.m_AwaitingData = false;

Updater.m_PageData = null;

Updater.m_TotalFilesSize = 0;
Updater.m_Metadata = null;
Updater.m_ProcessedFilesList = null;
Updater.m_Batches = null;

Updater.Elements = {};
Updater.Elements.m_DivLblMain = null;
Updater.Elements.m_DivLblPrev = null;
Updater.Elements.m_DivLblNext = null;
Updater.Elements.m_DivLstFilesSvr = null;
Updater.Elements.m_DivLstFilesUpl = null;
Updater.Elements.m_LblInputFiles = null;
Updater.Elements.m_InputFiles = null;
Updater.Elements.m_BtnClear = null;
Updater.Elements.m_BtnUpload = null;

Updater.Elements.m_DivUploadStatPanel = null;
Updater.Elements.m_DivUploadStatMetadata = null;
Updater.Elements.m_DivUploadStatBatches = null;

Updater.Elements.m_EventSet = false;
Updater.Elements.m_DebugHashCode = false;


$(document).ready(function () {
    Updater.Elements.m_DivLblMain = $("#div-lbl-main");
    Updater.Elements.m_DivLblPrev = $("#div-lbl-prev");
    Updater.Elements.m_DivLblNext = $("#div-lbl-next");
    Updater.Elements.m_DivLstFilesSvr = $("#div-lst-files-svr");
    Updater.Elements.m_DivLstFilesUpl = $("#div-lst-files-upl");
    Updater.Elements.m_LblInputFiles = $("#lbl-input-files");
    Updater.Elements.m_InputFiles = $("#input-files");
    Updater.Elements.m_BtnClear = $("#btn-clear");
    Updater.Elements.m_BtnUpload = $("#btn-upload");

    Updater.Elements.m_DivUploadStatPanel = $("#div-upload-stat-panel");

    Updater.ajax_fetchPageData();

    $("#div-files-table-head").css("padding-right", P24Utils.getScrollbarWidth());

});


// ==================================================
// ajax request sender

Updater.ajax_fetchPageData = function () {
    if (Updater.m_AwaitingData)
        return;

    Updater.m_AwaitingData = true;

    $.ajax({
        type: "GET",
        url: "Updater?handler=FetchPageData",
        success: Updater.ajax_fetchPageData_success,
        error: Updater.ajax_error
    });
}

Updater.ajax_clearInternalError = function () {
    if (Updater.m_AwaitingData || Updater.m_PageData.Message == "")
        return;

    Updater.m_AwaitingData = true;
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "Updater?handler=ClearInternalError",
        headers: { RequestVerificationToken: token },
        cache: false,
        success: Updater.ajax_clearInternalError_success,
        error: Updater.ajax_error,
    });
}

Updater.ajax_uploadMetadata = function () {
    if (Updater.m_AwaitingData)
        return;

    Updater.m_AwaitingData = true;
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "Updater?handler=UploadMetadata",
        headers: { RequestVerificationToken: token },
        data: JSON.stringify(Updater.m_Metadata),
        cache: false,
        contentType: "application/json; charset=utf-8",
        success: Updater.ajax_uploadMetadata_success,
        error: Updater.ajax_uploadMetadata_error
    });
}

Updater.ajax_uploadBatch = function (_header, _formData) {
    if (Updater.m_AwaitingData)
        return;

    let batchId = _header.Id;
    _header.RequestVerificationToken = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "Updater?handler=UploadBatch",
        headers: _header,
        data: _formData,
        cache: false,
        contentType: false,
        processData: false,
        success: Updater.ajax_uploadBatch_success,
        error: function (_xhr, _textStatus, _errorThrown) {
            Updater.m_Metadata.BatchesMetadata[batchId].Status = UPDATER_BATCH_STATUS_ERROR;
            Updater.refreshUploadBlock(Updater.Elements.m_DivUploadStatBatches[batchId], "Batch " + batchId, Updater.m_Metadata.BatchesMetadata[batchId].Status);
            P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
        }
    });
}

Updater.ajax_performPurge = function (_blockName) {
    if (Updater.m_AwaitingData)
        return;

    if (_blockName != UPDATER_BLOCK_NAME_PREV && _blockName != UPDATER_BLOCK_NAME_NEXT) {
        Updater.m_AwaitingData = false;
        return;
    }

    Updater.m_AwaitingData = true;
    let token = $("input[name='__RequestVerificationToken']").val();

    let fn = null;
    if (_blockName == UPDATER_BLOCK_NAME_PREV)
        fn = Updater.ajax_purgePrev_success;
    else if (_blockName == UPDATER_BLOCK_NAME_NEXT)
        fn = Updater.ajax_purgeNext_success;

    $.ajax({
        type: "POST",
        url: "Updater?handler=Purge" + _blockName,
        headers: { RequestVerificationToken: token },
        cache: false,
        success: fn,
        error: Updater.ajax_error,
    });
}

Updater.ajax_performAbort = function (_blockName) {
    if (Updater.m_AwaitingData)
        return;

    if (_blockName != UPDATER_BLOCK_NAME_PREV && _blockName != UPDATER_BLOCK_NAME_NEXT) {
        Updater.m_AwaitingData = false;
        return;
    }

    Updater.m_AwaitingData = true;
    let token = $("input[name='__RequestVerificationToken']").val();

    let fn = null;
    if (_blockName == UPDATER_BLOCK_NAME_PREV)
        fn = Updater.ajax_abortPrev_success;
    else if (_blockName == UPDATER_BLOCK_NAME_NEXT)
        fn = Updater.ajax_abortNext_success;

    $.ajax({
        type: "POST",
        url: "Updater?handler=Abort" + _blockName,
        headers: { RequestVerificationToken: token },
        cache: false,
        success: fn,
        error: Updater.ajax_error,
    });
}

Updater.ajax_performApply = function (_blockName) {
    if (Updater.m_AwaitingData)
        return;

    if (_blockName != UPDATER_BLOCK_NAME_PREV && _blockName != UPDATER_BLOCK_NAME_NEXT) {
        Updater.m_AwaitingData = false;
        return;
    }

    Updater.m_AwaitingData = true;
    let token = $("input[name='__RequestVerificationToken']").val();

    let fn = null;
    if (_blockName == UPDATER_BLOCK_NAME_PREV)
        fn = Updater.ajax_applyPrev_success;
    else if (_blockName == UPDATER_BLOCK_NAME_NEXT)
        fn = Updater.ajax_applyNext_success;

    $.ajax({
        type: "POST",
        url: "Updater?handler=Apply" + _blockName,
        headers: { RequestVerificationToken: token },
        cache: false,
        success: fn,
        error: Updater.ajax_error,
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// event

Updater.ajax_error = function (_xhr, _textStatus, _errorThrown) {
    Updater.m_AwaitingData = false;
    P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
}

Updater.ajax_fetchPageData_success = function (_content, _textStatus, _xhr) {
    Updater.m_AwaitingData = false;

    let body = _content.substring(6);

    if (Updater.ajaxSuccessContentCheckCommon(_content, body)) {
        Updater.processPageData(body);
        Updater.refreshPage();
        return;
    }
}

Updater.ajax_clearInternalError_success = function (_content, _textStatus, _xhr) {
    Updater.m_AwaitingData = false;

    if (_content.startsWith(P24_MSG_TAG_SUCCESS))
        console.log("Internal Error cleared.");
    else
        console.error("Error clearing Internal Error :))");
}

Updater.ajax_uploadMetadata_success = function (_content, _textStatus, _xhr) {
    Updater.m_AwaitingData = false;

    let body = _content.substring(6);

    if (Updater.ajaxSuccessContentCheckCommon(_content, body)) {
        Updater.m_Metadata.Status = UPDATER_BATCH_STATUS_SUCCESS;
        Updater.refreshUploadBlock(Updater.Elements.m_DivUploadStatMetadata, "Metadata", Updater.m_Metadata.Status);
        Updater.startUploadBatches();
        return;
    }

    Updater.m_Metadata.Status = UPDATER_BATCH_STATUS_ERROR;
    Updater.refreshUploadBlock(Updater.Elements.m_DivUploadStatMetadata, "Metadata", Updater.m_Metadata.Status);
}

Updater.ajax_uploadMetadata_error = function (_xhr, _textStatus, _errorThrown) {
    Updater.m_AwaitingData = false;

    Updater.m_Metadata.Status = UPDATER_BATCH_STATUS_ERROR;
    Updater.refreshUploadBlock(Updater.Elements.m_DivUploadStatMetadata, "Metadata", Updater.m_Metadata.Status);
    P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
}

Updater.ajax_uploadBatch_success = function (_content, _textStatus, _xhr) {
    let body = _content.substring(6);

    if (_content.startsWith(P24_MSG_TAG_SUCCESS)) {
        Updater.m_Metadata.BatchesMetadata[body].Status = UPDATER_BATCH_STATUS_SUCCESS;
        Updater.refreshUploadBlock(Updater.Elements.m_DivUploadStatBatches[body], "Batch " + body, Updater.m_Metadata.BatchesMetadata[body].Status);

        Updater.tryFinalizeUpload();
        return;
    }

    let obj = JSON.parse(body);
    let id = obj[0];
    if (_content.startsWith(P24_MSG_TAG_ERROR)) {
        let msg = "<div>Có lỗi xảy ra khi upload batch " + id + ". Vui lòng upload lại.</div><div><b>Msg:</b> " + obj[1] + "</div>";
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_FAIL], msg, "error");
    }
    else if (_content.startsWith(P24_MSG_TAG_EXCEPTION)) {
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_EXCEPTION], "<pre>" + body + "</pre>");
    }
    else {
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_UNKNOWN_ERR], "<pre>" + _content + "</pre>", "error");
    }

    Updater.m_Metadata.BatchesMetadata[id].Status = UPDATER_BATCH_STATUS_ERROR;
    Updater.refreshUploadBlock(Updater.Elements.m_DivUploadStatBatches[id], "Batch " + id, Updater.m_Metadata.BatchesMetadata[id].Status);
}

Updater.ajax_purgePrev_success = function (_content, _textStatus, _xhr) {
    Updater.ajax_purgeCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_PREV);
}

Updater.ajax_purgeNext_success = function (_content, _textStatus, _xhr) {
    Updater.ajax_purgeCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_NEXT);
}

Updater.ajax_abortPrev_success = function (_content, _textStatus, _xhr) {
    Updater.ajax_abortCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_PREV);
}

Updater.ajax_abortNext_success = function (_content, _textStatus, _xhr) {
    Updater.ajax_abortCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_NEXT);
}

Updater.ajax_applyPrev_success = function (_content, _textStatus, _xhr) {
    Updater.ajax_applyCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_PREV);
}

Updater.ajax_applyNext_success = function (_content, _textStatus, _xhr) {
    Updater.ajax_applyCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_NEXT);
}

Updater.ajax_purgeCommon_success = function (_content, _textStatus, _xhr, _blockName) {
    Updater.m_AwaitingData = false;

    let fname = "ajax_purge";
    let statusCode = 0;
    let descKey = "";

    if (_blockName == UPDATER_BLOCK_NAME_PREV) {
        fname += UPDATER_BLOCK_NAME_PREV;
        statusCode = UPDATER_STATUS_PREV_PURGE_QUEUED;
        descKey = LOCL_DESC_UPDATER_PREV_PURGE_QUEUED;
    }
    else if (_blockName == UPDATER_BLOCK_NAME_NEXT) {
        fname += UPDATER_BLOCK_NAME_NEXT;
        statusCode = UPDATER_STATUS_NEXT_PURGE_QUEUED;
        descKey = LOCL_DESC_UPDATER_NEXT_PURGE_QUEUED;
    }
    else {
        let html = "<div><code>" + ERRCODE_UPDATER_INVALID_BLOCK_NAME + "</code>.</div>"
            + "<div>fn: <code>" + fname + "Common</code></div>";
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], html, MODAL_ICON_WARNING);
        return;
    }

    let body = _content.substring(6);

    if (Updater.ajaxSuccessContentCheckCommon(_content, body)) {
        let err = true;
        let status = UPDATER_STATUS_NONE;

        if (body[1] == ':') {
            let num = parseInt(body.substring(2));

            if (num != NaN) {
                status = num;
                err = false;
            }
        }

        if (err) {
            let html = "<div>" + P24Localization[LOCL_DESC_MALFORMED] + ".</div><div>fn: <code>" + fname + "</code></div><div>body: <code>" + body + "</code></div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_ERR], html, MODAL_ICON_WARNING);
            return;
        }

        if (status == statusCode && body[0] != '0') {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_SUCCESS], P24Localization[descKey] + ".", MODAL_ICON_SUCCESS, "OK", "location.reload()");
            return;
        }

        {
            let html = "<div><code>" + ERRCODE_UNEXPECTED_CONTROL + "</code>.</div>"
                + "<div>fn: <code>" + fname + "</code></div>"
                + "<div>body: <code>" + body + "</code></div>"
                + "<div>pageData.Status: <code>" + Updater.m_PageData.Status + "</code></div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], html, MODAL_ICON_WARNING);
        }
    }
}

Updater.ajax_applyCommon_success = function (_content, _textStatus, _xhr, _blockName) {
    Updater.m_AwaitingData = false;

    let fname = "ajax_apply";
    let statusCode = 0;
    let descKey = "";

    if (_blockName == UPDATER_BLOCK_NAME_PREV) {
        fname += UPDATER_BLOCK_NAME_PREV;
        statusCode = UPDATER_STATUS_PREV_APPLY_QUEUED;
        descKey = LOCL_DESC_UPDATER_PREV_APPLY_QUEUED;
    }
    else if (_blockName == UPDATER_BLOCK_NAME_NEXT) {
        fname += UPDATER_BLOCK_NAME_NEXT;
        statusCode = UPDATER_STATUS_NEXT_APPLY_QUEUED;
        descKey = LOCL_DESC_UPDATER_NEXT_APPLY_QUEUED;
    }
    else {
        let html = "<div><code>" + ERRCODE_UPDATER_INVALID_BLOCK_NAME + "</code>.</div>"
            + "<div>fn: <code>" + fname + "Common</code></div>";
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], html, MODAL_ICON_WARNING);
        return;
    }

    let body = _content.substring(6);

    if (Updater.ajaxSuccessContentCheckCommon(_content, body)) {
        let err = true;
        let status = UPDATER_STATUS_NONE;

        if (body[1] == ':') {
            let num = parseInt(body.substring(2));

            if (num != NaN) {
                status = num;
                err = false;
            }
        }

        if (err) {
            let html = "<div>" + P24Localization[LOCL_DESC_MALFORMED] + ".</div><div>fn: <code>" + fname + "</code></div><div>body: <code>" + body + "</code></div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_ERR], html, MODAL_ICON_WARNING);
            return;
        }

        if (status == statusCode && body[0] != '0') {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_SUCCESS], P24Localization[descKey] + ".", MODAL_ICON_SUCCESS, "OK", "location.reload()");
            return;
        }

        {
            let html = "<div><code>" + ERRCODE_UNEXPECTED_CONTROL + "</code>.</div>"
                + "<div>fn: <code>" + fname + "</code></div>"
                + "<div>body: <code>" + body + "</code></div>"
                + "<div>pageData.Status: <code>" + Updater.m_PageData.Status + "</code></div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], html, MODAL_ICON_WARNING);
        }
    }
}

Updater.ajax_abortCommon_success = function (_content, _textStatus, _xhr, _blockName) {
    Updater.m_AwaitingData = false;

    let fname = "ajax_abort";

    if (_blockName == UPDATER_BLOCK_NAME_PREV) {
        fname += UPDATER_BLOCK_NAME_PREV;
    }
    else if (_blockName == UPDATER_BLOCK_NAME_NEXT) {
        fname += UPDATER_BLOCK_NAME_NEXT;
    }
    else {
        let html = "<div><code>" + ERRCODE_UPDATER_INVALID_BLOCK_NAME + "</code>.</div>"
            + "<div>fn: <code>" + fname + "Common</code></div>";
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], html, MODAL_ICON_WARNING);
        return;
    }

    let body = _content.substring(6);

    if (Updater.ajaxSuccessContentCheckCommon(_content, body)) {
        let err = true;
        let status = UPDATER_STATUS_NONE;

        if (body[1] == ':') {
            let num = parseInt(body.substring(2));

            if (num != NaN) {
                status = num;
                err = false;
            }
        }

        if (err) {
            let html = "<div>" + P24Localization[LOCL_DESC_MALFORMED] + ".</div><div>fn: <code>" + fname + "</code></div><div>body: <code>" + body + "</code></div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_ERR], html, MODAL_ICON_WARNING);
            return;
        }

        if (status == UPDATER_STATUS_PREV_PURGE_RUNNING || status == UPDATER_STATUS_NEXT_PURGE_RUNNING) {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], P24Localization[LOCL_DESC_UPDATER_PURGE_CANNOT_ABORT], MODAL_ICON_WARNING);
            return;
        }

        if (status == UPDATER_STATUS_NONE && body[0] != '0') {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_SUCCESS], P24Localization[LOCL_DESC_UPDATER_ABORT_SUCCESS] + ".", MODAL_ICON_SUCCESS, "OK", "location.reload()");
            return;
        }

        {
            let html = "<div><code>" + ERRCODE_UNEXPECTED_CONTROL + "</code>.</div>"
                + "<div>fn: <code>" + fname + "</code></div>"
                + "<div>body: <code>" + body + "</code></div>"
                + "<div>pageData.Status: <code>" + Updater.m_PageData.Status + "</code></div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], html, MODAL_ICON_WARNING);
        }
    }
}

Updater.ajaxSuccessContentCheckCommon = function (_content, _body) {
    if (_content.startsWith(P24_MSG_TAG_ERROR)) {
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_FAIL], _body, MODAL_ICON_ERROR);
        return false;
    }

    if (_content.startsWith(P24_MSG_TAG_EXCEPTION)) {
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_EXCEPTION], "<pre>" + HtmlUtils.escape(_body) + "</pre>");
        return false;
    }

    if (!_content.startsWith(P24_MSG_TAG_SUCCESS)) {
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_UNKNOWN_ERR], "<pre>" + HtmlUtils.escape(_content) + "</pre>", MODAL_ICON_ERROR);
        return false;
    }

    return true;
}

Updater.cancelModal = function () {
    Updater.m_AwaitingData = false;
}

function inputFiles_onChange() {
    Updater.processFilesList();
    Updater.refreshUploadFilesPanel();
}

function btnClear_onClick() {
    Updater.Elements.m_InputFiles.val(null);
    Updater.clearUploadPanel();
}

function btnUpload_onClick() {
    Updater.startUploadMetadata();
}

function btnPurge_onClick(_blockName) {
    let content = "";

    if (_blockName == UPDATER_BLOCK_NAME_PREV)
        content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_PURGE_PREV];
    else if (_blockName == UPDATER_BLOCK_NAME_NEXT)
        content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_PURGE_NEXT];
    else
        return;

    Updater.openConfirmationModal(content, "Updater.ajax_performPurge('" + _blockName + "')", "Updater.cancelModal()");
}

function btnAbort_onClick(_blockName) {
    let content = "";

    if (_blockName == UPDATER_BLOCK_NAME_PREV) {
        if (Updater.m_PageData.Status == UPDATER_STATUS_PREV_PURGE_QUEUED)
            content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_PURGE];
        else if (Updater.m_PageData.Status == UPDATER_STATUS_PREV_APPLY_QUEUED)
            content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_APPLY];
    }
    else if (_blockName == UPDATER_BLOCK_NAME_NEXT) {
        if (Updater.m_PageData.Status == UPDATER_STATUS_NEXT_PURGE_QUEUED)
            content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_PURGE];
        else if (Updater.m_PageData.Status == UPDATER_STATUS_NEXT_APPLY_QUEUED)
            content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_APPLY];
    }
    else
        return;

    Updater.openConfirmationModal(content, "Updater.ajax_performAbort('" + _blockName + "')", "Updater.cancelModal()");
}

function btnApply_onClick(_blockName) {
    let content = "";

    if (_blockName == UPDATER_BLOCK_NAME_PREV)
        content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_APPLY_PREV];
    else if (_blockName == UPDATER_BLOCK_NAME_NEXT)
        content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_APPLY_NEXT];
    else
        return;

    Updater.openConfirmationModal(content, "Updater.ajax_performApply('" + _blockName + "')", "Updater.cancelModal()");
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

// END: event
// ==================================================

// ==================================================
// helper

Updater.constructFileItemHtml = function (_fileItem, _noColor = false) {
    let colorClass = "";
    if (!_noColor) {
        if (_fileItem.Status == UPDATER_FILE_STATUS_NO_CHANGE)
            colorClass = "text-success ";
        else if (_fileItem.Status == UPDATER_FILE_STATUS_UPDATE)
            colorClass = "text-warning ";
        else if (_fileItem.Status == UPDATER_FILE_STATUS_ERROR)
            colorClass = "text-danger ";
    }

    let hiddenAttr = " hidden";
    if (Updater.Elements.m_DebugHashCode)
        hiddenAttr = "";

    let path = _fileItem.Path;
    if (path != "" && !path.endsWith("/"))
        path += "/";

    let html = "<div class=\"d-flex " + colorClass + "me-2\">"
        + "<div class=\"text-end me-1\" style=\"width:18ch\"" + hiddenAttr + ">" + _fileItem.HashCode + "</div>"
        + "<div class=\"text-break me-1\"><span class=\"fw-light\">" + path + "</span>" + _fileItem.File.name + "</div>"
        + "<div class=\"text-break text-end fs-6 ms-auto\">" + _fileItem.LastMod + "</div>"
        + "<div class=\"text-end text-nowrap ms-1\" style=\"width:11ch\">" + P24Utils.formatDataLength(_fileItem.File.size) + "</div>"
        + "</div>";

    return html;
}

Updater.constructUploadBlockHtml = function (_blockId, _blockText, _status, _tooltipText = null) {
    let blockClass = "alert ";
    if (_status == UPDATER_BATCH_STATUS_NOT_STARTED) {
        blockClass += "alert-secondary";
    }
    else if (_status == UPDATER_BATCH_STATUS_IN_PROGRESS) {
        blockClass += "alert-warning";
        svgClass = "bi-box-arrow-up";
        svgContent = "<path fill-rule=\"evenodd\" d=\"M3.5 6a.5.5 0 0 0-.5.5v8a.5.5 0 0 0 .5.5h9a.5.5 0 0 0 .5-.5v-8a.5.5 0 0 0-.5-.5h-2a.5.5 0 0 1 0-1h2A1.5 1.5 0 0 1 14 6.5v8a1.5 1.5 0 0 1-1.5 1.5h-9A1.5 1.5 0 0 1 2 14.5v-8A1.5 1.5 0 0 1 3.5 5h2a.5.5 0 0 1 0 1h-2z\" />"
            + "<path fill-rule=\"evenodd\" d=\"M7.646.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1-.708.708L8.5 1.707V10.5a.5.5 0 0 1-1 0V1.707L5.354 3.854a.5.5 0 1 1-.708-.708l3-3z\" />";
    }
    else if (_status == UPDATER_BATCH_STATUS_SUCCESS) {
        blockClass += "alert-success";
        svgClass = "bi-check-lg";
        svgContent = "<path d=\"M12.736 3.97a.733.733 0 0 1 1.047 0c.286.289.29.756.01 1.05L7.88 12.01a.733.733 0 0 1-1.065.02L3.217 8.384a.757.757 0 0 1 0-1.06.733.733 0 0 1 1.047 0l3.052 3.093 5.4-6.425a.247.247 0 0 1 .02-.022Z\" />";
    }
    else if (_status == UPDATER_BATCH_STATUS_ERROR) {
        blockClass += "alert-danger";
        svgClass = "bi-x-lg";
        svgContent = "<path d=\"M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z\" /> ";
    }
    else {
        blockClass += "alert-info";
    }
    blockClass += " m-1 py-1 px-3";

    let tooltipHtml = "";
    if (_tooltipText != null)
        tooltipHtml = " data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"" + _tooltipText + "\"";

    let html = "<div id=\"" + _blockId + "\" class=\"" + blockClass + "\"" + tooltipHtml + ">"
        + Updater.constructUploadBlockSvg(_status)
        + _blockText
        + "</div>";

    return html;
}

Updater.constructUploadBlockSvg = function (_status) {
    let svgClass = "";
    let svgContent = "";
    if (_status == UPDATER_BATCH_STATUS_IN_PROGRESS) {
        svgClass = "bi-box-arrow-up";
        svgContent = "<path fill-rule=\"evenodd\" d=\"M3.5 6a.5.5 0 0 0-.5.5v8a.5.5 0 0 0 .5.5h9a.5.5 0 0 0 .5-.5v-8a.5.5 0 0 0-.5-.5h-2a.5.5 0 0 1 0-1h2A1.5 1.5 0 0 1 14 6.5v8a1.5 1.5 0 0 1-1.5 1.5h-9A1.5 1.5 0 0 1 2 14.5v-8A1.5 1.5 0 0 1 3.5 5h2a.5.5 0 0 1 0 1h-2z\" />"
            + "<path fill-rule=\"evenodd\" d=\"M7.646.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1-.708.708L8.5 1.707V10.5a.5.5 0 0 1-1 0V1.707L5.354 3.854a.5.5 0 1 1-.708-.708l3-3z\" />";
    }
    else if (_status == UPDATER_BATCH_STATUS_SUCCESS) {
        svgClass = "bi-check-lg";
        svgContent = "<path d=\"M12.736 3.97a.733.733 0 0 1 1.047 0c.286.289.29.756.01 1.05L7.88 12.01a.733.733 0 0 1-1.065.02L3.217 8.384a.757.757 0 0 1 0-1.06.733.733 0 0 1 1.047 0l3.052 3.093 5.4-6.425a.247.247 0 0 1 .02-.022Z\" />";
    }
    else if (_status == UPDATER_BATCH_STATUS_ERROR) {
        svgClass = "bi-x-lg";
        svgContent = "<path d=\"M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z\" /> ";
    }

    let html = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi " + svgClass + " me-1\" viewBox=\"0 0 16 16\">"
        + svgContent
        + "</svg>";

    return html;
}

Updater.constructVersionPanelButton = function (_svcIcon, _subClass = "", _onClick = "", _isEnabled = true) {
    if (_subClass != "")
        _subClass = " btn-outline-" + _subClass;

    let attr = "";
    if (!_isEnabled)
        attr = " disabled";

    let html = "<button type=\"button\" class=\"btn" + _subClass + " mx-1 px-2 py-1\" onclick=\"" + _onClick + "\"" + attr + ">" + _svcIcon + "</button>";
    return html;
}

Updater.computePanelVerBtnStatus = function (_blockName) {
    let samePQ = 0;
    let sameAQ = 0;
    let crosAQ = 0;

    // If any Purge task or Apply task is in progress: No action is available.
    if (Updater.m_PageData.Status == UPDATER_STATUS_PREV_PURGE_RUNNING || Updater.m_PageData.Status == UPDATER_STATUS_NEXT_PURGE_RUNNING
        || Updater.m_PageData.Status == UPDATER_STATUS_PREV_APPLY_RUNNING || Updater.m_PageData.Status == UPDATER_STATUS_NEXT_APPLY_RUNNING) {
        return [false, false, false];
    }

    if (_blockName == UPDATER_BLOCK_NAME_PREV) {
        samePQ = UPDATER_STATUS_PREV_PURGE_QUEUED;
        sameAQ = UPDATER_STATUS_PREV_APPLY_QUEUED;
        crosAQ = UPDATER_STATUS_NEXT_APPLY_QUEUED;
    }

    if (_blockName == UPDATER_BLOCK_NAME_NEXT) {
        samePQ = UPDATER_STATUS_NEXT_PURGE_QUEUED;
        sameAQ = UPDATER_STATUS_NEXT_APPLY_QUEUED;
        crosAQ = UPDATER_STATUS_PREV_APPLY_QUEUED;
    }

    // If this side's Purge task is queued: Abort is available.
    if (Updater.m_PageData.Status == samePQ)
        return [false, true, false];

    // If this side's Apply task is queued: Abort and Purge is available.
    if (Updater.m_PageData.Status == sameAQ)
        return [true, true, false];

    // If opposite side's Apply task is queued: No action on this side is available.
    if (Updater.m_PageData.Status == crosAQ)
        return [false, false, false];

    // Else: Purge and Apply is available.
    return [true, false, true];
}

Updater.openConfirmationModal = function (_content, _actionName0, _actionName1) {
    Modal.Common.openTwoBtnModal(
        P24Localization[LOCL_STR_CONFIRM],
        _content,
        MODAL_ICON_QUESTION,
        P24Localization[LOCL_STR_CONFIRM], _actionName0,
        P24Localization[LOCL_STR_CANCEL], _actionName1,
        false);
}

function computeCyrb53HashCode(_string, _seed = 0) {
    return cyrb53(_string, _seed);
}

// END: helper
// ==================================================

Updater.refreshPage = function () {
    Updater.refreshPanelVer("main");
    Updater.refreshPanelVer("Prev");
    Updater.refreshPanelVer("Next");

    {
        let html = "";
        for (let pair of Updater.m_PageData.Files) {
            html += Updater.constructFileItemHtml(pair[1], true);
        }
        Updater.Elements.m_DivLstFilesSvr.html(html);
    }

    {
        let elm = $("#div-svr-title");
        let html = elm.html();

        let pos = html.indexOf("(");
        if (pos > 0)
            html = html.substring(0, pos - 1);

        html += " (" + Updater.m_PageData.Files.size + ")";
        elm.html(html);
    }

    Updater.refreshUploadFilesPanel();

    Updater.Elements.m_InputFiles.removeAttr("disabled");

    if (!Updater.Elements.m_EventSet) {
        Updater.Elements.m_EventSet = true;
        $("#div-input-files").on("mouseenter dragenter", lblInputFiles_dragIn);
        $("#div-input-files").on("mouseleave dragend drop dragexit dragleave", lblInputFiles_dragOut);
    }

    // open modal if there is error message;
    Updater.tryOpenErrMsgDialog();
}

Updater.refreshUploadFilesPanel = function () {
    if (Updater.m_Metadata == null) {
        Updater.clearUploadPanel();
        return;
    }

    {
        let uploadSizeString = P24Utils.formatDataLength(Updater.m_Metadata.FilesSize);
        let totalSizeString = P24Utils.formatDataLength(Updater.m_TotalFilesSize);

        let html = Updater.m_Metadata.FilesCount + "/" + Updater.m_ProcessedFilesList.size + " file";
        if (Updater.m_ProcessedFilesList.size > 1)
            html += "s";
        html += " (" + uploadSizeString + "/" + totalSizeString + ")";
        Updater.Elements.m_LblInputFiles.html(html);
    }

    {
        let html = "";
        for (let pair of Updater.m_ProcessedFilesList) {
            html += Updater.constructFileItemHtml(pair[1]);
        }

        Updater.Elements.m_DivLstFilesUpl.html(html);
    }

    {
        let elm = $("#div-upl-title");
        let html = elm.html();

        let pos = html.indexOf("(");
        if (pos > 0)
            html = html.substring(0, pos - 1);

        html += " (" + Updater.m_Metadata.FilesCount + ")";
        elm.html(html);
    }

    if (Updater.m_Metadata.BatchesCount > 0) {
        Updater.refreshUploadStatusPanel();

        Updater.Elements.m_BtnClear.removeAttr("disabled");
        Updater.Elements.m_BtnUpload.removeAttr("disabled");
    } else {
        Updater.Elements.m_BtnClear.attr("disabled", true);
        Updater.Elements.m_BtnUpload.attr("disabled", true);
    }

}

Updater.refreshUploadStatusPanel = function () {
    const id_divUploadStatMetadata = "div-upload-stat-metadata";
    const id_divUploadStatBatchX = "div-upload-stat-batch-";

    let html = Updater.constructUploadBlockHtml(id_divUploadStatMetadata, "Metadata", Updater.m_Metadata.Status);

    html += "<div class=\"vr mx-1\"></div>";

    for (let i = 0; i < Updater.m_Metadata.BatchesCount; ++i) {
        let tooltipText = Updater.m_Metadata.BatchesMetadata[i].FilesCount + " file";
        if (Updater.m_Metadata.BatchesMetadata[i].FilesCount > 1)
            tooltipText += "s";
        tooltipText += ", " + P24Utils.formatDataLength(Updater.m_Metadata.BatchesMetadata[i].FilesSize);
        html += Updater.constructUploadBlockHtml(id_divUploadStatBatchX + i, "Batch " + i, Updater.m_Metadata.BatchesMetadata[i].Status, tooltipText);
    }

    Updater.Elements.m_DivUploadStatPanel.html(html);
    P24Utils.reloadAllTooltips();

    Updater.Elements.m_DivUploadStatMetadata = $("#" + id_divUploadStatMetadata);
    Updater.Elements.m_DivUploadStatBatches = [];
    for (let i = 0; i < Updater.m_Metadata.BatchesCount; ++i) {
        Updater.Elements.m_DivUploadStatBatches[i] = $("#" + id_divUploadStatBatchX + i);
    }
}

Updater.refreshUploadBlock = function (_element, _blockText, _status) {
    let blockClass = "alert-secondary";

    if (_status == UPDATER_BATCH_STATUS_IN_PROGRESS)
        blockClass = "alert-warning";
    else if (_status == UPDATER_BATCH_STATUS_SUCCESS)
        blockClass = "alert-success";
    else if (_status == UPDATER_BATCH_STATUS_ERROR)
        blockClass = "alert-danger";

    let html = Updater.constructUploadBlockSvg(_status) + _blockText;
    _element.html(html);

    _element.removeClass("alert-secondary alert-warning alert-success alert-danger");
    _element.addClass(blockClass);
}

Updater.refreshPanelVer = function (_blockName = "main") {
    let data = null;
    let element = null;
    let btnStat = [];

    if (_blockName == "main") {
        data = Updater.m_PageData.Main;
        element = Updater.Elements.m_DivLblMain;
    }
    else if (_blockName == "Prev") {
        data = Updater.m_PageData.Prev;
        element = Updater.Elements.m_DivLblPrev;
        btnStat = Updater.computePanelVerBtnStatus(_blockName);
    }
    else if (_blockName == "Next") {
        data = Updater.m_PageData.Next;
        element = Updater.Elements.m_DivLblNext;
        btnStat = Updater.computePanelVerBtnStatus(_blockName);
    }
    else
        return;

    let colClass0 = "col-6 col-md-12";
    let colClass1 = "col-6 col-md-12";

    let html = "";

    if (data == null) {
        html += "<div class=\"" + colClass0 + "\">&times;</div><div class=\"" + colClass1 + "\">" + Updater.constructVersionPanelButton(SVG_BLANK, true) + "</div>";
    } else {
        html += "<div class=\"" + colClass0 + "\">" + data + "</div>";

        if (_blockName == "main") {
            html += "<div class=\"" + colClass1 + "\">" + Updater.constructVersionPanelButton(SVG_BI_ARROW_CLOCKWISE, "primary", "Updater.ajax_fetchPageData()") + "</div>";
        } else {
            html += "<div class=\"" + colClass1 + "\">"
                + Updater.constructVersionPanelButton(SVG_BI_TRASH3, "danger", "btnPurge_onClick('" + _blockName + "')", btnStat[0])
                + Updater.constructVersionPanelButton(SVG_BI_X_LG, "warning", "btnAbort_onClick('" + _blockName + "')", btnStat[1])
                + Updater.constructVersionPanelButton(SVG_BI_CHECK_LG, "success", "btnApply_onClick('" + _blockName + "')", btnStat[2])
                + "</div>";
        }
    }

    element.html(html);
}

Updater.tryOpenErrMsgDialog = function () {
    if (Updater.m_PageData.Message == "")
        return;

    let html = Updater.m_PageData.Message;
    Modal.Common.openTwoBtnModal("Internal Message", html, MODAL_ICON_ERROR, "Clear Message", "Updater.ajax_clearInternalError()", "Close");
}

Updater.clearUploadPanel = function () {
    Updater.Elements.m_LblInputFiles.html(P24Localization[LOCL_DESC_UPDATER_INPUTFILES_INITIAL]);
    Updater.Elements.m_DivLstFilesUpl.html("");
    Updater.Elements.m_DivUploadStatPanel.html("");

    Updater.Elements.m_DivUploadStatMetadata = null;
    Updater.Elements.m_DivUploadStatBatches = null;

    Updater.Elements.m_BtnClear.attr("disabled", true);
    Updater.Elements.m_BtnUpload.attr("disabled", true);
}

// ==================================================

Updater.processPageData = function (_jsonData) {
    let data = JSON.parse(_jsonData);

    if (data.Main == null) {
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_ERR], P24Localization[LOCL_DESC_UPDATER_NOMAIN] + ".", MODAL_ICON_ERROR);
        return;
    }

    let map = new Map();
    if (data.Files != null) {
        let arr = data.Files;
        for (let i = 0; i < arr.length; ++i) {
            let fi = arr[i];
            map.set(fi.HashCode, fi);
        }
    }
    data.Files = map;

    Updater.m_PageData = data;
}

Updater.processFilesList = function () {
    let files = Updater.Elements.m_InputFiles[0].files;
    if (files.length == 0) {
        Updater.m_TotalFilesSize = 0;
        Updater.m_Metadata = null;
        Updater.m_ProcessedFilesList = null;
        Updater.m_Batches = null;

        return;
    }

    Updater.m_TotalFilesSize = 0;
    Updater.m_ProcessedFilesList = new Map();
    Updater.m_Batches = new Array();

    let batchesMetadata = [];

    let lastModDates = new Array();
    let filesCount = 0;
    let filesSize = 0;

    let batch = [];
    let batchIndex = 0;
    let batchFilesSize = 0;
    let batchFilesCount = 0;
    let filesMetadata = {};

    for (let i = 0; i < files.length; ++i) {
        let file = files[i];
        let fileItem = Updater.processSingleFile(file);

        Updater.m_ProcessedFilesList.set(fileItem.HashCode, fileItem);
        Updater.m_TotalFilesSize += file.size;

        if (fileItem.Status == UPDATER_FILE_STATUS_NO_CHANGE)
            continue;

        if (batchFilesSize >= P24_MAX_UPLOAD_SIZE - UPDATER_HEADER_BUFFER || batchFilesCount >= P24_MAX_UPLOAD_COUNT) {
            Updater.m_Batches[batchIndex] = {
                Id: batchIndex,
                //Status: UPDATER_BATCH_STATUS_NOT_STARTED,
                Size: batchFilesSize,
                Count: batchFilesCount,
                Files: batch
            };

            batchesMetadata.push({
                Id: batchIndex,
                Status: UPDATER_BATCH_STATUS_NOT_STARTED,
                FilesCount: batchFilesCount,
                FilesSize: batchFilesSize,
                FilesMetadata: filesMetadata
            });

            batch = [];
            batchFilesSize = 0;
            batchFilesCount = 0;
            ++batchIndex;
            filesMetadata = {};
        }

        filesMetadata[fileItem.HashCode] = { HashCode: fileItem.HashCode, LastMod: file.lastModified };

        lastModDates.push({ K: fileItem.HashCode, V: file.lastModified });
        filesSize += file.size;
        ++filesCount;

        batch.push(fileItem);
        batchFilesSize += file.size;
        ++batchFilesCount;
    }

    if (batchFilesCount > 0)
    {
        Updater.m_Batches[batchIndex] = {
            Id: batchIndex,
            //Status: UPDATER_BATCH_STATUS_NOT_STARTED,
            Size: batchFilesSize,
            Count: batchFilesCount,
            Files: batch
        };

        batchesMetadata.push({
            Id: batchIndex,
            Status: UPDATER_BATCH_STATUS_NOT_STARTED,
            FilesCount: batchFilesCount,
            FilesSize: batchFilesSize,
            FilesMetadata: filesMetadata
        });
    }

    Updater.m_Metadata = {
        Status: UPDATER_BATCH_STATUS_NOT_STARTED,
        FilesCount: filesCount,
        FilesSize: filesSize,
        BatchesCount: Updater.m_Batches.length,
        BatchesMetadata: batchesMetadata,
    };
}

Updater.processSingleFile = function (_file) {
    let prePos = _file.webkitRelativePath.indexOf("/");
    if (prePos < 0) {
        console.error("Invalid file: " + _file.name);
        return null;
    }

    let path = "";

    let pos = _file.webkitRelativePath.lastIndexOf("/");
    if (pos > prePos) {
        path = _file.webkitRelativePath.substring(prePos + 1, pos);
    }

    if (path != "" && !path.endsWith("/"))
        path += "/";

    let hashCode = computeCyrb53HashCode(path + _file.name);
    let dateString = P24Utils.formatDateString_endsAtMinute(_file.lastModifiedDate);

    let status = UPDATER_FILE_STATUS_CREATE;

    let svrFile = Updater.m_PageData.Files.get(hashCode);
    if (svrFile != null) {
        if (svrFile.LastMod == dateString)
            status = UPDATER_FILE_STATUS_NO_CHANGE;
        else
            status = UPDATER_FILE_STATUS_UPDATE;
    }

    let fileItem = {
        Path: path,
        LastMod: dateString,
        HashCode: hashCode,
        Status: status,
        File: _file,
    };

    return fileItem;
}

Updater.tryFinalizeUpload = function () {
    for (let i = 0; i < Updater.m_Metadata.BatchesCount; ++i) {
        if (Updater.m_Metadata.BatchesMetadata[i].Status != UPDATER_BATCH_STATUS_SUCCESS)
            return;
    }

    let content = P24Localization[LOCL_DESC_UPDATER_UPLOAD_SUCCESS] + ".";
    Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_SUCCESS], content, MODAL_ICON_SUCCESS, "OK", "Updater.ajax_fetchPageData()");
}

// ==================================================

Updater.startUploadMetadata = function () {
    if (Updater.m_Metadata.Status == UPDATER_BATCH_STATUS_SUCCESS) {
        console.log("Metadata uploaded, no need to reupload.");
        Updater.startUploadBatches();
        return;
    }

    Updater.refreshUploadBlock(Updater.Elements.m_DivUploadStatMetadata, "Metadata", UPDATER_BATCH_STATUS_IN_PROGRESS);
    Updater.ajax_uploadMetadata();
}

Updater.startUploadBatches = function () {
    for (let i = 0; i < Updater.m_Batches.length; ++i) {
        let batchStatus = Updater.m_Metadata.BatchesMetadata[i].Status;
        if (batchStatus == UPDATER_BATCH_STATUS_SUCCESS || batchStatus == UPDATER_BATCH_STATUS_IN_PROGRESS) {
            console.log("Batch " + i + " uploaded, no need to reupload.");
            continue;
        }

        let batch = Updater.m_Batches[i];
        Updater.m_Metadata.BatchesMetadata[i].Status = UPDATER_BATCH_STATUS_IN_PROGRESS;
        Updater.refreshUploadBlock(Updater.Elements.m_DivUploadStatBatches[i], "Batch " + i, Updater.m_Metadata.BatchesMetadata[i].Status);
        Updater.uploadBatch(batch);
    }
}

Updater.uploadBatch = function (_batch) {
    let header = {
        Id: _batch.Id,
        Size: _batch.Size,
        Count: _batch.Count
    };

    let formData = new FormData();
    for (let i = 0; i < _batch.Files.length; ++i) {
        formData.append("_files", _batch.Files[i].File);
    }

    Updater.ajax_uploadBatch(header, formData);
}
