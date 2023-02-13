/*  modal/modal.js
 *  Version: 1.3 (2023.02.14)
 *
 *  Contributor
 *      Arime-chan
 */

window.Modals = {};

Modals.CommonInfoModal = {};
Modals.CommonInfoModal.Modal = null;
Modals.CommonInfoModal.Header = null;
Modals.CommonInfoModal.Icon = null;
Modals.CommonInfoModal.Body = null;
Modals.CommonInfoModal.Footer = null;

Modals.Common = {};

Modals.Common.Info = {};
Modals.Common.Info.m_Modal = null;
Modals.Common.Info.m_Header = null;
Modals.Common.Info.m_Icon = null;
Modals.Common.Info.m_Body = null;

Modals.Common.Confirm = {};
Modals.Common.Confirm.m_Modal = null;
Modals.Common.Confirm.m_Header = null;
Modals.Common.Confirm.m_Icon = null;
Modals.Common.Confirm.m_Body = null;
Modals.Common.Confirm.m_BtnYes = null;
Modals.Common.Confirm.m_BtnNo = null;


$(document).ready(function () {

    Modals.CommonInfoModal.Modal = $("#modal-common-info");
    Modals.CommonInfoModal.Header = $("#modal-common-info-header");
    Modals.CommonInfoModal.Icon= $("#modal-common-info-icon");
    Modals.CommonInfoModal.Body = $("#modal-common-info-body");
    Modals.CommonInfoModal.Footer = $("#modal-common-info-footer");

    Modals.Common.Info.m_Modal = $("#modal-common-info");
    Modals.Common.Info.m_Header = $("#modal-common-info-header");
    Modals.Common.Info.m_Icon = $("#modal-common-info-icon");
    Modals.Common.Info.m_Body = $("#modal-common-info-body");

    Modals.Common.Confirm.m_Modal = $("#modal-common-confirm");
    Modals.Common.Confirm.m_Header = $("#modal-common-confirm-header");
    Modals.Common.Confirm.m_Icon = $("#modal-common-confirm-icon");
    Modals.Common.Confirm.m_Body = $("#modal-common-confirm-body");
    Modals.Common.Confirm.m_BtnYes = $("#modal-common-confirm-btn-yes");
    //Modals.Common.Confirm.m_BtnNo = $("#modal-common-confirm-btn-no");

    if (Modals.Common.Info.m_Modal != null) {
        Modals.Common.Info.m_Modal.on("hide.bs.modal", function (_e) {
            Modals.Common.Info.m_Header.html("");
            Modals.Common.Info.m_Icon.html("");
            Modals.Common.Info.m_Body.html("");
        });
    }

    if (Modals.Common.Confirm.m_Modal != null) {
        Modals.Common.Confirm.m_Modal.on("hide.bs.modal", function (_e) {
            Modals.Common.Confirm.m_Header.html("");
            Modals.Common.Confirm.m_Icon.html("");
            Modals.Common.Confirm.m_Body.html("");
            //Modals.Common.Confirm.m_BtnYes.html("");
            //Modals.Common.Confirm.m_BtnNo.html("");

            Modals.Common.Confirm.m_BtnYes.attr("onclick", "");
            //Modals.Common.Confirm.m_BtnNo.attr("onclick", "");
        });
    }

});


Modals.Common.openModal = function (_modalModule, _header, _body, _fnNameYes) {
    if (_header != null)
        _modalModule.m_Header.html(_header);

    if (_body != null)
        _modalModule.m_Body.html(_body);

    _modalModule.m_BtnYes.attr("onclick", _fnNameYes);

    _modalModule.m_Icon.html(Modals.Common.buildSvgIcon("info"));

    _modalModule.m_Modal.modal();
}

Modals.Common.Info.openModal = function (_header, _body, _icon = null) {
    if (_header != null)
        Modals.Common.Info.m_Header.html(_header);

    if (_body != null)
        Modals.Common.Info.m_Body.html(_body);

    if (_icon != null) {
        Modals.Common.Info.m_Icon.html(_icon);
    } else {
        Modals.Common.Info.m_Icon.html(Modals.Common.buildSvgIcon("info"));
    }

    Modals.Common.Info.m_Modal.modal();
}

Modals.Common.Confirm.openModal = function (_header, _body, _fnNameYes) {
    if (_header != null)
        Modals.Common.Confirm.m_Header.html(_header);

    if (_body != null)
        Modals.Common.Confirm.m_Body.html(_body);

    Modals.Common.Confirm.m_BtnYes.attr("onclick", _fnNameYes);

    Modals.Common.Confirm.m_Icon.html(Modals.Common.buildSvgIcon("info"));

    Modals.Common.Confirm.m_Modal.modal();
}

// ==================================================
// event

Modals.ajax_error = function (_xhr, _textStatus, _errorThrown) {
    let body = "<div>Status Code: <code>" + _xhr.status + " - " + HttpStatusCodeName[_xhr.status] + "</code></div><div>jq Status: <code>" + _textStatus + "</code></div>";
    Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
}

// END: event
// ==================================================

// ==================================================
// helper

Modals.Common.Info.openInfoModal = function (_header, _body) {
    Modals.Common.Info.openModal(_header, _body);
}

Modals.Common.Info.openErrorModal = function (_header, _body) {
    Modals.Common.Info.openModal(_header, _body, Modals.Common.buildSvgIcon("error"));
}

Modals.CommonInfoModal.openModal = function (_header, _body, _footer) {

    if (_header != null)
        Modals.CommonInfoModal.Header.html(_header);

    if (_body != null)
        Modals.CommonInfoModal.Body.html(_body);

    if (_footer != null)
        Modals.CommonInfoModal.Footer.html(_footer);

    Modals.CommonInfoModal.Modal.modal();
}

Modals.CommonInfoModal.openErrorModal = function (_header, _body, _footer) {
    let html = Modals.CommonInfoModal.buildSvgIcon("error");
    Modals.CommonInfoModal.Icon.html(html);

    Modals.CommonInfoModal.openModal(_header, _body, _footer);
}

Modals.CommonInfoModal.openWarningModal = function (_header, _body, _footer) {
    let html = Modals.CommonInfoModal.buildSvgIcon("warning");
    Modals.CommonInfoModal.Icon.html(html);

    Modals.CommonInfoModal.openModal(_header, _body, _footer);
}

Modals.CommonInfoModal.openSuccessModal = function (_header, _body, _footer) {
    let html = Modals.CommonInfoModal.buildSvgIcon("success");
    Modals.CommonInfoModal.Icon.html(html);

    Modals.CommonInfoModal.openModal(_header, _body, _footer);
}

Modals.CommonInfoModal.openInfoModal = function (_header, _body, _footer) {
    let html = Modals.CommonInfoModal.buildSvgIcon("info");
    Modals.CommonInfoModal.Icon.html(html);

    Modals.CommonInfoModal.openModal(_header, _body, _footer);
}

Modals.CommonInfoModal.buildSvgIcon = function (_name) {
    const size = 48;
    let iconClass = "";
    let path = "";

    if (_name == "success") {
        iconClass = "bi-check-circle text-success";
        path = "<path d=\"M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z\" />"
            + "<path d=\"M10.97 4.97a.235.235 0 0 0-.02.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-1.071-1.05z\" />";
    }
    else if (_name == "error") {
        iconClass = "bi-x-circle text-danger";
        path = "<path d=\"M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z\" />"
            + "<path d=\"M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z\" />";
    }
    else if (_name == "warning") {
        iconClass = "bi-exclamation-triangle text-warning";
        path = "<path d=\"M7.938 2.016A.13.13 0 0 1 8.002 2a.13.13 0 0 1 .063.016.146.146 0 0 1 .054.057l6.857 11.667c.036.06.035.124.002.183a.163.163 0 0 1-.054.06.116.116 0 0 1-.066.017H1.146a.115.115 0 0 1-.066-.017.163.163 0 0 1-.054-.06.176.176 0 0 1 .002-.183L7.884 2.073a.147.147 0 0 1 .054-.057zm1.044-.45a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566z\" />"
            + "<path d=\"M7.002 12a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 5.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995z\" />";
    }
    else if (_name == "info") {
        iconClass = "bi-info-circle text-info";
        path = "<path d=\"M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z\" />"
            + "<path d=\"m8.93 6.588-2.29.287-.082.38.45.083c.294.07.352.176.288.469l-.738 3.468c-.194.897.105 1.319.808 1.319.545 0 1.178-.252 1.465-.598l.088-.416c-.2.176-.492.246-.686.246-.275 0-.375-.193-.304-.533L8.93 6.588zM9 4.5a1 1 0 1 1-2 0 1 1 0 0 1 2 0z\" />";
    }
    else return "";

    let html = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"" + size + "\" height=\"" + size + "\" fill=\"currentColor\" class=\"bi " + iconClass + "\" viewBox=\"0 0 16 16\">"
        + path + "</svg>";
    return html;
}

Modals.Common.buildSvgIcon = function (_name) {
    const size = 48;
    let iconClass = "";
    let path = "";

    if (_name == "success") {
        iconClass = "bi-check-circle text-success";
        path = "<path d=\"M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z\" />"
            + "<path d=\"M10.97 4.97a.235.235 0 0 0-.02.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-1.071-1.05z\" />";
    }
    else if (_name == "error") {
        iconClass = "bi-x-circle text-danger";
        path = "<path d=\"M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z\" />"
            + "<path d=\"M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z\" />";
    }
    else if (_name == "warning") {
        iconClass = "bi-exclamation-triangle text-warning";
        path = "<path d=\"M7.938 2.016A.13.13 0 0 1 8.002 2a.13.13 0 0 1 .063.016.146.146 0 0 1 .054.057l6.857 11.667c.036.06.035.124.002.183a.163.163 0 0 1-.054.06.116.116 0 0 1-.066.017H1.146a.115.115 0 0 1-.066-.017.163.163 0 0 1-.054-.06.176.176 0 0 1 .002-.183L7.884 2.073a.147.147 0 0 1 .054-.057zm1.044-.45a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566z\" />"
            + "<path d=\"M7.002 12a1 1 0 1 1 2 0 1 1 0 0 1-2 0zM7.1 5.995a.905.905 0 1 1 1.8 0l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995z\" />";
    }
    else if (_name == "info") {
        iconClass = "bi-info-circle text-info";
        path = "<path d=\"M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z\" />"
            + "<path d=\"m8.93 6.588-2.29.287-.082.38.45.083c.294.07.352.176.288.469l-.738 3.468c-.194.897.105 1.319.808 1.319.545 0 1.178-.252 1.465-.598l.088-.416c-.2.176-.492.246-.686.246-.275 0-.375-.193-.304-.533L8.93 6.588zM9 4.5a1 1 0 1 1-2 0 1 1 0 0 1 2 0z\" />";
    }
    else return "";

    let html = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"" + size + "\" height=\"" + size + "\" fill=\"currentColor\" class=\"bi " + iconClass + "\" viewBox=\"0 0 16 16\">"
        + path + "</svg>";
    return html;
}

// END: helper
// ==================================================
