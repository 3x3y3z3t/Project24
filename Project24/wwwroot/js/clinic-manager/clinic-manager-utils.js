/*  clinic-manager-utils.js
 *  Version: 1.3 (2023.02.13)
 *
 *  Contributor
 *      Arime-chan
 */

function customer_commonListImage_DeleteCustomerImage(_imageId) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: 'POST',
        url: '/ClinicManager/Customer/Delete?handler=DeleteImage',
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(_imageId),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: function (_content) {
            $("#list-image").html(_content);
            console.log("Deleted image " + _imageId + ".");
            //location.reload();
        },
        error: function () {
            console.error("Error deleting image " + _imageId + ".");
            window.alert("Error deleting image " + _imageId + ".");
        }
    });
}

function customer_commonListImage_DeleteTicketImage(_imageId) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: 'POST',
        url: '/ClinicManager/Ticket/Delete?handler=DeleteImage',
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(_imageId),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: function (_content) {
            $("#list-image").html(_content);
            console.log("Deleted image " + _imageId + ".");
            //location.reload();
        },
        error: function () {
            console.error("Error deleting image " + _imageId + ".");
            window.alert("Error deleting image " + _imageId + ".");
        }
    });
}



window.CommonListImage = {};

CommonListImage.Backend = {};
CommonListImage.Backend.m_Side = null;
CommonListImage.Backend.m_ImageId = null;
CommonListImage.Backend.m_OrgName = null;
CommonListImage.Backend.m_OrgExtension = null;

CommonListImage.Elements = {};
CommonListImage.Elements.Modal = {};
CommonListImage.Elements.Modal.m_Modal = null;
CommonListImage.Elements.Modal.m_Img = null;
CommonListImage.Elements.Modal.m_InputRename = null;
CommonListImage.Elements.Modal.m_BtnRenameReset = null;
CommonListImage.Elements.Modal.m_BtnRenameSubmit = null;
CommonListImage.Elements.Modal.m_BtnDelete = null;

$(document).ready(function () {

    CommonListImage.Elements.Modal.m_Modal = $("#modal-image");
    CommonListImage.Elements.Modal.m_Img = $("#modal-image-img");
    CommonListImage.Elements.Modal.m_InputRename = $("#modal-image-input-rename");
    CommonListImage.Elements.Modal.m_BtnRenameReset = $("#modal-image-btn-rename-reset");
    CommonListImage.Elements.Modal.m_BtnRenameSubmit = $("#modal-image-btn-rename-submit");
    CommonListImage.Elements.Modal.m_BtnDelete = $("#modal-image-btn-delete");

    CommonListImage.Elements.Modal.m_Modal.on("hide.bs.modal", function (_e) {
        CommonListImage.Backend.m_Side = null;
        CommonListImage.Backend.m_ImageId = null;
        CommonListImage.Backend.m_OrgName = null;
        CommonListImage.Backend.m_OrgExtension = null;

        CommonListImage.Elements.Modal.m_Img.attr("src", "");
        CommonListImage.Elements.Modal.m_InputRename.val("");
        CommonListImage.Elements.Modal.m_InputRename.removeClass("text-success");
        CommonListImage.Elements.Modal.m_BtnRenameReset.attr("disabled", true);
        CommonListImage.Elements.Modal.m_BtnRenameSubmit.attr("disabled", true);
    });

    let tooltipTemplate =
        "<div class=\"tooltip\" role=\"tooltip\">" +
        "<div class=\"arrow\"></div>" +
        "<div class=\"tooltip-inner text-left text-danger border border-dark tooltip-custom-nas\"></div>" +
        "</div>";

    CommonListImage.Elements.Modal.m_InputRename.tooltip({
        animation: true,
        container: "body",
        html: true,
        placement: "top",
        trigger: "manual",
        template: tooltipTemplate
    });
});


// ==================================================
// ajax request sender

CommonListImage.ajax_renameImage = function (_side, _imageId, _newName) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/ClinicManager/ImageManager?handler=Rename" + _side + "Image",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify({
            ImageId: _imageId,
            NewName: _newName,
        }),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: CommonListImage.ajax_renameImage_success,
        error: CommonListImage.ajax_renameImage_error
    });

}

CommonListImage.ajax_deleteImage = function (_side, _imageId) {
    let token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: "POST",
        url: "/ClinicManager/" + _side + "/Delete?handler=DeleteImage",
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(_imageId),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: CommonListImage.ajax_deleteImage_success,
        error: Modals.ajax_error
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// event

function img_onClick(_side, _imageId, _path, _name) {
    CommonListImage.Backend.m_Side = _side;
    CommonListImage.Backend.m_ImageId = _imageId;
    CommonListImage.Backend.m_OrgExtension = P24Utils.getFileExtension(_name);
    CommonListImage.Backend.m_OrgName = _name.replace("." + CommonListImage.Backend.m_OrgExtension, "");

    CommonListImage.Elements.Modal.m_Img.attr("src", "/data/" + _path + "/" + _name);
    CommonListImage.Elements.Modal.m_InputRename.val(CommonListImage.Backend.m_OrgName);

    CommonListImage.Elements.Modal.m_Modal.modal();
}

function btnDelete_onClick(_side, _imageId, _name) {
    CommonListImage.Elements.Modal.m_BtnDelete.attr("disabled", true);

    let extension = P24Utils.getFileExtension(_name);
    let name = _name.replace(extension, "");

    Modals.Common.Confirm.openModal(
        "Đổi tên file",
        "<div>Bạn có muốn xóa file ảnh <code>" + name + extension + "</code>?</div>",
        "CommonListImage.submitDelete('" + _side + "', '" + _imageId + "')"
    );
}

function modalImage_inputRename_onInput() {
    let name = CommonListImage.Elements.Modal.m_InputRename.val();

    if (name == CommonListImage.Backend.m_OrgName) {
        CommonListImage.Elements.Modal.m_InputRename.removeClass("text-success");
        CommonListImage.Elements.Modal.m_BtnRenameReset.attr("disabled", true);
        CommonListImage.Elements.Modal.m_BtnRenameSubmit.attr("disabled", true);
    } else {
        CommonListImage.Elements.Modal.m_InputRename.addClass("text-success");
        CommonListImage.Elements.Modal.m_BtnRenameReset.removeAttr("disabled");
        CommonListImage.Elements.Modal.m_BtnRenameSubmit.removeAttr("disabled");

        if (name == "") {
            CommonListImage.Elements.Modal.m_InputRename.tooltip("hide");
            CommonListImage.Elements.Modal.m_BtnRenameSubmit.attr("disabled", true);
        }
        else if (P24Utils.isFileNameValid(name)) {
            CommonListImage.Elements.Modal.m_InputRename.tooltip("hide");
            CommonListImage.Elements.Modal.m_BtnRenameSubmit.removeAttr("disabled");
        } else {
            CommonListImage.Elements.Modal.m_InputRename.tooltip("show");
            CommonListImage.Elements.Modal.m_BtnRenameSubmit.attr("disabled", true);
        }
    }
}

function modalImage_btnRenameReset_onClick() {
    CommonListImage.Elements.Modal.m_InputRename.val(CommonListImage.Backend.m_OrgName);

    CommonListImage.Elements.Modal.m_InputRename.removeClass("text-success");
    CommonListImage.Elements.Modal.m_BtnRenameReset.attr("disabled", true);
    CommonListImage.Elements.Modal.m_BtnRenameSubmit.attr("disabled", true);
}

function modalImage_btnRenameSubmit_onClick() {
    let newName = CommonListImage.Elements.Modal.m_InputRename.val();

    Modals.Common.Confirm.openModal(
        "Đổi tên file",
        "<div>Bạn có muốn đổi tên file thành <code>" + newName + "." + CommonListImage.Backend.m_OrgExtension + "</code>?</div>",
        "CommonListImage.submitRename()"
    );
}

function modalImage_btnDelete_onClick() {
    let name = CommonListImage.Backend.m_OrgName + "." + CommonListImage.Backend.m_OrgExtension;
    btnDelete_onClick(CommonListImage.Backend.m_Side, CommonListImage.Backend.m_ImageId, name);
}

CommonListImage.ajax_renameImage_success = function (_content, _textStatus, _xhr) {
    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.Common.Info.openErrorModal("Có lỗi xảy ra", body);
        return;
    }

    $("#list-image").html(_content);

    Modals.Common.Info.openInfoModal("Đổi tên file", "Đổi tên file thành công.");
}

CommonListImage.ajax_renameImage_error = function (_xhr, _textStatus, _errorThrown) {
    CommonListImage.Elements.Modal.m_BtnRenameSubmit.removeAttr("disabled");
    P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
}

CommonListImage.ajax_deleteImage_success = function (_content, _textStatus, _xhr) {
    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.Common.Info.openErrorModal("Có lỗi xảy ra", body);
        return;
    }

    $("#list-image").html(_content);

    CommonListImage.Elements.Modal.m_Modal.modal("hide");

    Modals.Common.Info.openInfoModal("Xóa file", "Xóa file thành công.");
}

// END: event
// ==================================================

// ==================================================
// helper



// END: helper
// ==================================================

CommonListImage.submitRename = function () {
    CommonListImage.Elements.Modal.m_BtnRenameSubmit.attr("disabled", true);

    let newName = CommonListImage.Elements.Modal.m_InputRename.val() + "." + CommonListImage.Backend.m_OrgExtension;
    CommonListImage.ajax_renameImage(CommonListImage.Backend.m_Side, CommonListImage.Backend.m_ImageId, newName);
}

CommonListImage.submitDelete = function (_side, _imageId) {
    CommonListImage.Elements.Modal.m_BtnRenameSubmit.attr("disabled", true);

    CommonListImage.ajax_deleteImage(_side, _imageId);
}
