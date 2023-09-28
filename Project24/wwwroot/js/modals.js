/*  modals.js
    Version: v1.3 (2023.09.23)

    Contributor
        Arime-chan
 */

const MODAL_ICON_ERROR = "error";
const MODAL_ICON_WARNING = "warning";
const MODAL_ICON_SUCCESS = "success";
const MODAL_ICON_INFO = "info";
const MODAL_ICON_QUESTION = "ask";


window.Modal = {};

// ==================================================
// Common Modals

Modal.Common = {};

Modal.Common = {};
Modal.Common.m_IsOpen = false;
Modal.Common.m_StacksCount = 1;
Modal.Common.m_InitialTitleHtml = "";
Modal.Common.m_Modal = null;

Modal.Common.Element = {};
Modal.Common.Element.m_Header = null;
Modal.Common.Element.m_Body = null;
Modal.Common.Element.m_Footer = null;


$(document).ready(function () {
    Modal.Common.m_Modal = $("#modal-common");
    Modal.Common.Element.m_Header = $("#modal-common-header");
    Modal.Common.Element.m_Body = $("#modal-common-body");
    Modal.Common.Element.m_Footer = $("#modal-common-footer");


    Modal.Common.m_Modal.on("shown.bs.modal", function () { Modal.Common.m_IsOpen = true; });
    Modal.Common.m_Modal.on("hidden.bs.modal", function () {
        Modal.Common.m_IsOpen = false;
        Modal.Common.m_StacksCount = 1;
        Modal.Common.m_InitialTitleHtml = "";
    });
});


Modal.Common.openOneBtnModal = function (_titleHtml, _content, _iconName = null, _btnLabel = "OK", _actionName = "", _addCloseBtn = true) {
    let header = "";
    let body = "";

    let btnData = [
        { label: _btnLabel, actionName: _actionName, dismiss: true, className: "btn-secondary" }
    ];

    if (Modal.Common.m_IsOpen) {
        ++Modal.Common.m_StacksCount;
        header = Modal.Common.constructHeader(P24Localization[LOCL_STR_MULTIPLE_MSG] + " (" + Modal.Common.m_StacksCount + ")", true);

        if (Modal.Common.m_StacksCount == 2) {
            body = Modal.Common.constructHeader(Modal.Common.m_InitialTitleHtml, false);
        }

        body += Modal.Common.Element.m_Body.html()
            + "<hr />"
            + "<div class=\"mb-2\">" + Modal.Common.constructHeader(_titleHtml, false) + "</div>"
            + Modal.Common.constructBody(Modal.Common.constructSvgIcon(_iconName), _content);

        btnData.label = "OK";
    } else {
        Modal.Common.m_InitialTitleHtml = _titleHtml;

        header = Modal.Common.constructHeader(_titleHtml, _addCloseBtn);
        body = Modal.Common.constructBody(Modal.Common.constructSvgIcon(_iconName), _content);
    }

    let footer = Modal.Common.constructFooter(1, btnData);

    Modal.Common.openCustomized(header, body, footer);
}

Modal.Common.openTwoBtnModal = function (_titleHtml, _content, _iconName = null, _btnLabel0 = "Yes", _actionName0 = "", _btnLabel1 = "No", _actionName1 = "", _addCloseBtn = true) {
    let btnData = [
        { label: _btnLabel0, actionName: _actionName0, dismiss: true, className: "btn-primary" },
        { label: _btnLabel1, actionName: _actionName1, dismiss: true, className: "btn-secondary" }
    ];

    let header = Modal.Common.constructHeader(_titleHtml, _addCloseBtn);
    let body = Modal.Common.constructBody(Modal.Common.constructSvgIcon(_iconName), _content);
    let footer = Modal.Common.constructFooter(2, btnData);

    Modal.Common.openCustomized(header, body, footer);
}

Modal.Common.openCustomized = function (_header, _body, _footer) {
    Modal.Common.Element.m_Header.html(_header);
    Modal.Common.Element.m_Body.html(_body);
    Modal.Common.Element.m_Footer.html(_footer);

    let modal = bootstrap.Modal.getOrCreateInstance(Modal.Common.m_Modal[0], { focus: true });
    //if (Modal.Common.m_IsOpen)
    //    modal.hide();

    modal.show();
}

/*  
 *
 */
Modal.Common.constructHeader = function (_titleHtml, _addCloseBtn) {
    let html = "<h5 class=\"modal-title\">" + _titleHtml + "</h5>";

    if (_addCloseBtn) {
        html += "\n<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\"></button>";
    }

    return html;
}

/*  
 *
 */
Modal.Common.constructBody = function (_iconHtml, _contentHtml) {
    if (_iconHtml == null)
        return "<div>" + _contentHtml + "</div>";

    let html = "<div class=\"d-flex\">"
        + "<div class=\"col-2 text-center align-self-center\">" + _iconHtml + "</div>"
        + "<div class=\"col-10\">" + _contentHtml + "</div>"
        + "</div>";

    return html;
}

/*  
 *
 */
Modal.Common.constructFooter = function (_btnCount, _btnData) {
    if (_btnCount > _btnData.length) {
        console.warn("Modal.Common.constructFooter(" + _btnCount + ", " + _btnData.length + "): not enough button data supported.");
        _btnCount = _btnData.length;
    } else if (_btnCount < _btnData.length) {
        console.log("Modal.Common.constructFooter(" + _btnCount + ", " + _btnData.length + "): exceed button data will be ignored.");
    }

    let html = "";
    for (let i = 0; i < _btnCount; ++i) {
        let btnLabel = _btnData[i].label;
        let btnAction = _btnData[i].actionName;
        let btnClass = _btnData[i].className;

        html += "<button type=\"button\" class=\"btn " + btnClass + "\"";

        if (_btnData[i].dismiss) {
            html += "data-bs-dismiss=\"modal\"";
        }

        html += " onclick=\"" + btnAction + "\">" + btnLabel + "</button>\n";
    }

    return html;
}

Modal.Common.constructSvgIcon = function (_name) {
    const size = 48;

    let html = null;

    switch (_name) {
        case MODAL_ICON_SUCCESS:
            html = P24Utils.svg("check-circle", "text-success");
            break;

        case MODAL_ICON_WARNING:
            html = P24Utils.svg("exclamation-triangle", "text-warning");
            break;

        case MODAL_ICON_ERROR:
            html = P24Utils.svg("x-circle", "text-danger");
            break;

        case MODAL_ICON_INFO:
            html = P24Utils.svg("info-circle", "text-info");
            break;

        case MODAL_ICON_QUESTION:
            html = P24Utils.svg("question-circle", "text-info");
            break;

        default:
            return "";
    }

    html = html.replace("width=\"16\"", "width=\"" + size + "\"");
    html = html.replace("height=\"16\"", "height=\"" + size + "\"");

    return html;
}

// END: Common Modals
// ==================================================
