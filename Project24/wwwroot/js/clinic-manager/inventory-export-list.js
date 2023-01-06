/*  inventory-export-list.js
 *  Version: 1.0 (2023.01.07)
 *
 *  Contributor
 *      Arime-chan
 */

window.ExportListPage = {};

ExportListPage.Element = {};
ExportListPage.Element.m_DivBatchDetailsView = null;


$(document).ready(function () {
    ExportListPage.Element.m_DivBatchDetailsView = $("#div-batch-details-view");
});

// ==================================================
// ajax request sender

ExportListPage.ajax_getExportBatchDetails = function (_batchId) {
    $.ajax({
        type: "GET",
        url: "/ClinicManager/Inventory/Export/Details?handler=PartialOnly&_id=" + _batchId,
        success: ExportListPage.ajax_getBatchDetails_success,
        error: ExportListPage.ajax_getBatchDetails_error
    });
}

ExportListPage.ajax_deleteExportBatch = function (_batchId) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/ClinicManager/Inventory/Export/Delete?handler=DeleteBatch",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(_batchId),
        contentType: "application/json; charset=utf-8",
        success: ExportListPage.ajax_getBatchDetails_success,
        error: ExportListPage.ajax_getBatchDetails_error
    });
}

ExportListPage.ajax_deleteExportSingle= function (_exportId) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/ClinicManager/Inventory/Export/Delete?handler=DeleteSingle",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(_exportId),
        contentType: "application/json; charset=utf-8",
        success: ExportListPage.ajax_getBatchDetails_success,
        error: ExportListPage.ajax_getBatchDetails_error
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// event

function a_detailsBatchExport(_batchId) {
    ExportListPage.ajax_getExportBatchDetails(_batchId);
}

function a_deleteBatchExport(_batchId) {
    ExportListPage.ajax_deleteExportBatch(_batchId);
}

function a_deleteExport(_importId) {
    ExportListPage.ajax_deleteExportSingle(_importId);
}

ExportListPage.ajax_getBatchDetails_success = function (_content, _textStatus, _xhr) {
    ExportListPage.Element.m_DivBatchDetailsView.removeAttr("hidden");
    ExportListPage.Element.m_DivBatchDetailsView.html(_content);

    return;

    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        let json = _content.substring(6);

        ExportListPage.updateDetailsView(json);
        return;
    }
}

ExportListPage.ajax_getBatchDetails_error = function (_xhr, _textStatus, _errorThrown) {
    let body = "<div>Status Code: <code>" + _xhr.status + " - " + HttpStatusCodeName[_xhr.status] + "</code></div><div>jq Status: <code>" + _textStatus + "</code></div>";
    Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
}

// END: event
// ==================================================

// ==================================================
// helper

// END: helper
// ==================================================
