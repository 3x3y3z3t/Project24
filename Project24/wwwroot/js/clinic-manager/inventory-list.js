/*  inventory-list.js
 *  Version: 1.1 (2023.02.13)
 *
 *  Contributor
 *      Arime-chan
 */

window.InventoryListPage = {};


// ==================================================
// ajax request sender

InventoryListPage.ajax_validateStock = function () {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/ClinicManager/Inventory/List?handler=ValidateStorage",
        headers: { "RequestVerificationToken": token },
        success: InventoryListPage.ajax_validateStock_success,
        error: Modals.ajax_error,
    });
}

InventoryListPage.ajax_hideDrug = function (_drugId, _hide = true) {
    let token = $("input[name='__RequestVerificationToken']").val();

    let url = "/ClinicManager/Inventory/List?handler=HideDrug&_drugId=" + _drugId + "&_hidden=true";
    if (!_hide)
        url = "/ClinicManager/Inventory/List?handler=HideDrug&_drugId=" + _drugId + "&_hidden=false";;

    $.ajax({
        type: "POST",
        url: url,
        headers: { "RequestVerificationToken": token },
        success: InventoryListPage.ajax_hideDrug_success,
        error: Modals.ajax_error,
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// event

function btn_validateStock() {
    $("#btn-validate-stock").attr("disabled", true);
    $("#btn-validate-stock").html(".. (đang kiểm kho) ..");

    InventoryListPage.openInfoModal();

    InventoryListPage.ajax_validateStock();
}

function anchorHideListing_onClick(_drugId) {
    if (_drugId == null)
        return;

    InventoryListPage.ajax_hideDrug(_drugId);
}

function anchorUnHideListing_onClick(_drugId) {
    if (_drugId == null)
        return;

    InventoryListPage.ajax_hideDrug(_drugId, false);
}

InventoryListPage.ajax_validateStock_success = function (_content, _textStatus, _xhr) {
    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
        return;
    }

    if (_content.startsWith("<info>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openInfoModal("", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        let footer = "<button type=\"button\" class=\"btn btn-primary\" onclick=\"location.reload()\">Tải lại trang</button>";
        Modals.CommonInfoModal.openSuccessModal("Thành công", "Kiểm kho thành công. Vui lòng tải lại trang.", footer);
    }
}

InventoryListPage.ajax_hideDrug_success = function (_content, _textStatus, _xhr) {
    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
        return;
    }

    if (_content.startsWith("<info>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openInfoModal("", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        location.reload();
    }
}

// END: event
// ==================================================

// ==================================================
// helper

// END: helper
// ==================================================

InventoryListPage.openInfoModal = function () {
    let body = "Đang kiểm kho. Quá trình này có thể mất vài phút.";
    Modals.CommonInfoModal.openInfoModal("Kiểm kho", body, null);
}
