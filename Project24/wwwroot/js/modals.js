/*  modals.js
    Version: v1.2 (2023.06.28)

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
        header = Modal.Common.constructHeader("Nhiều thông báo (" + Modal.Common.m_StacksCount + ")", true);

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
    let iconClass = "";
    let colorClass = null;

    if (_name == MODAL_ICON_QUESTION) {
        html = SVG_BI_QUESTION_CIRCLE;
        colorClass = "text-info";

        html = html.replace("width=\"16\"", "width=\"" + size + "\"");
        html = html.replace("height=\"16\"", "height=\"" + size + "\"");
        html = html.replace("class=\"", "class=\"" + colorClass + " ");

        return html;
    } else if (_name == MODAL_ICON_INFO) {
        html = SVG_BI_INFO_CIRCLE;
        colorClass = "text-info";

        html = html.replace("width=\"16\"", "width=\"" + size + "\"");
        html = html.replace("height=\"16\"", "height=\"" + size + "\"");
        html = html.replace("class=\"", "class=\"" + colorClass + " ");

        return html;
    }

    //if (html == null)
    //    return null;

    //html = html.replace("width=\"16\"", "width=\"" + size + "\"");
    //html = html.replace("height=\"16\"", "height=\"" + size + "\"");
    //html = html.replace("class=\"", "class=\"" + colorClass + " ");

    //return html;

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
    else return null;

    html = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"" + size + "\" height=\"" + size + "\" fill=\"currentColor\" class=\"bi " + iconClass + "\" viewBox=\"0 0 16 16\">"
        + path + "</svg>";
    return html;
}

// END: Common Modals
// ==================================================
