/*  inventory-import-list.js
 *  Version: 1.1 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

window.ImportListPage = {};

ImportListPage.Element = {};
ImportListPage.Element.m_DivBatchDetailsView = null;


$(document).ready(function () {
    ImportListPage.Element.m_DivBatchDetailsView = $("#div-batch-details-view");
});

// ==================================================
// ajax request sender

ImportListPage.ajax_getImportBatchDetails = function (_batchId) {
    $.ajax({
        type: "GET",
        url: "/ClinicManager/Inventory/Import/Details?handler=PartialOnly&_id=" + _batchId,
        success: ImportListPage.ajax_getBatchDetails_success,
        error: ImportListPage.ajax_getBatchDetails_error
    });
}

ImportListPage.ajax_deleteImportBatch = function (_batchId) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/ClinicManager/Inventory/Import/Delete?handler=DeleteBatch",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(_batchId),
        contentType: "application/json; charset=utf-8",
        success: ImportListPage.ajax_getBatchDetails_success,
        error: ImportListPage.ajax_getBatchDetails_error
    });
}

ImportListPage.ajax_deleteImportSingle = function (_importId) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/ClinicManager/Inventory/Import/Delete?handler=DeleteSingle",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(_importId),
        contentType: "application/json; charset=utf-8",
        success: ImportListPage.ajax_getBatchDetails_success,
        error: ImportListPage.ajax_getBatchDetails_error
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// event

function a_detailsBatchImport(_batchId) {
    ImportListPage.ajax_getImportBatchDetails(_batchId);
}

function a_deleteBatchImport(_batchId) {
    ImportListPage.ajax_deleteImportBatch(_batchId);
}

function a_deleteImport(_importId) {
    ImportListPage.ajax_deleteImportSingle(_importId);
}

ImportListPage.ajax_getBatchDetails_success = function (_content, _textStatus, _xhr) {
    ImportListPage.Element.m_DivBatchDetailsView.removeAttr("hidden");
    ImportListPage.Element.m_DivBatchDetailsView.html(_content);

    return;

    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        let json = _content.substring(6);

        ImportListPage.updateDetailsView(json);
        return;
    }
}

ImportListPage.ajax_getBatchDetails_error = function (_xhr, _textStatus, _errorThrown) {
    let body = "<div>Status Code: <code>" + _xhr.status + " - " + HttpStatusCodeName[_xhr.status] + "</code></div><div>jq Status: <code>" + _textStatus + "</code></div>";
    Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
}

// END: event
// ==================================================

// ==================================================
// helper

// END: helper
// ==================================================
