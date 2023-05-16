/*  modals.js
    Version: v1.0 (2023.05.16)

    Contributor
        Arime-chan
 */

window.Modal = {};


// ==================================================
// Common Modals

Modal.Common = {};

Modal.Common = {};
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

});


Modal.Common.openCustomized = function (_header, _body, _footer) {
    Modal.Common.Element.m_Header.html(_header);
    Modal.Common.Element.m_Body.html(_body);
    Modal.Common.Element.m_Footer.html(_footer);

    let modal = new bootstrap.Modal(Modal.Common.m_Modal[0], { focus: true });
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
    let html = "";

    if (_iconHtml == null) {

    } else {

    }

    html += _contentHtml;

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

Modal.Common.openOneBtnModal = function (_titleHtml, _content, _iconName = null, _btnLabel = "OK", _addCloseBtn = true) {
    let btnData = [
        { label: _btnLabel, actionName: "", dismiss: true, className: "btn-secondary" }
    ];

    let header = Modal.Common.constructHeader(_titleHtml, _addCloseBtn);
    let body = Modal.Common.constructBody(null, _content);
    let footer = Modal.Common.constructFooter(1, btnData);

    Modal.Common.openCustomized(header, body, footer);
}

// END: Common Modals
// ==================================================
