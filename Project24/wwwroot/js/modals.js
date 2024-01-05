/*  modals.js
 *  Version: v1.4 (2023.12.28)
 *  Spec:    v0.1
 *
 *  Contributor
 *      Arime-chan (Author)
 */

// ==================================================

window.Modal = {
    IconData: null,

    m_IsOpen: false,

    m_Modal: null,
    m_Header: null,
    m_Body: null,
    m_Footer: null,


    init: function () {
        this.m_Modal = $("#modal-common");
        this.m_Header = $("#modal-common-header");
        this.m_Body = $("#modal-common-body");
        this.m_Footer = $("#modal-common-footer");

        this.m_Modal.on("shown.bs.modal", function () { Modal.m_IsOpen = true; });
        this.m_Modal.on("hidden.bs.modal", function () {
            Modal.m_IsOpen = false;
            //Modal.Common.m_StacksCount = 1;
            //Modal.Common.m_InitialTitleHtml = "";
        });



        //let modal2 = bootstrap.Modal.getOrCreateInstance($("#modal-2")[0], { focus: true });
        //modal2.show();

        //let modal3 = bootstrap.Modal.getOrCreateInstance($("#modal-3")[0], { focus: true });
        //modal3.show();



    },

    // ==================================================

    /**
     * Open a customized modal.
     * @param {any} _modalData should have the following definition:
     *  {
     *      AddCloseBtn:            {boolean}   (true) whether a close button should be displayed on the header
     *      ButtonsData: [
     *          {
     *              Class:          {string}    (BTN_CLASS_SECONDARY) use `BTN_CLASS_*`
     *              DismissModal:   {boolean}   (true) whether this button should close the modal
     *              Id:             {string}    (null) the button's id
     *              Label:          {string}    ("OK") the label of the button
     *              OnClickText:    {string}    (null) the function name to be invoked in onclick event
     *          }
     *      ]
     *      Content:                {html}      the content of the modal
     *      IconData:               {string}    use `Modal.IconData.*`
     *      TitleHtml:              {html}      the title of the modal
     *  }
     */
    openModal: function (_modalData) {
        if (_modalData == null) {
            console.warn("Modal.openModal(): _modalData is null.");
            return;
        }

        let header = this.constructHeader(_modalData.TitleHtml, _modalData.AddCloseBtn);
        this.m_Header.html(header);

        let body = this.constructBody(_modalData.Content, _modalData.IconData);
        this.m_Body.html(body);

        let footer = this.constructFooter(_modalData.ButtonsData);
        this.m_Footer.html(footer);


        let modal = bootstrap.Modal.getOrCreateInstance(this.m_Modal[0], { focus: true });
        modal.show();
    },


    /**
     * Constructs the html header of the modal.
     * @param {any} _titleHtml The title of this modal in html.
     * @param {any} _addCloseBtn Specifies whether to display a close button on the header.
     * @returns The header of the modal in html.
     */
    constructHeader: function (_titleHtml, _addCloseBtn) {
        let html = "<h5 class=\"modal-title\">" + _titleHtml + "</h5>";

        if (_addCloseBtn == null || _addCloseBtn)
            html += "<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\"></button>";

        return html;
    },

    /**
     * Constructs the html body of the modal.
     * @param {any} _iconData The icon name to be displayed in the modal.
     *      If this parameter is omitted, the modal will be display without an icon.
     * @param {any} _contentHtml The html content to be displayed in the modal.
     * @returns The body of the modal in html.
     */
    constructBody: function (_contentHtml, _iconData = null) {
        if (_iconData == null)
            return "<div>" + _contentHtml + "</div>";

        let iconHtml = P24Utils.svg(_iconData.name, _iconData.class, 48);

        let html = "<div class=\"col-2 text-center align-self-center\">" + iconHtml + "</div>"
            + "<div class=\"col-10\">" + _contentHtml + "</div>";

        return html;
    },

    constructFooter: function (_buttonsData) {
        if (_buttonsData == null || _buttonsData.length <= 0) {
            return "<button class=\"btn btn-secondary\" data-bs-dismiss=\"modal\">OK</button>";
        }

        let html = "";
        for (const btnData of _buttonsData) {
            html += this.constructFooterButton(btnData);
        }

        return html;
    },

    constructFooterButton: function (_buttonData) {
        if (_buttonData.Label == null)
            _buttonData.Label = "OK";
        if (_buttonData.Class == null)
            _buttonData.Class = BTN_CLASS_SECONDARY;

        let html = "<button";
        if (_buttonData.Id != null && _buttonData.Id.trim() != "")
            html += " id=\"" + _buttonData.Id + "\"";

        html += " class=\"btn ";

        if (_buttonData.Class != null)
            html += _buttonData.Class;

        html += "\"";

        if (_buttonData.OnClickText != null)
            html += " onclick=\"" + _buttonData.OnClickText + "\"";

        if (_buttonData.DismissModal == null || _buttonData.DismissModal)
            html += " data-bs-dismiss=\"modal\"";

        html += " type=\"button\">" + _buttonData.Label + "</button>";

        return html;
    },

};

Modal.IconData = {
    Success: { name: "check-circle", class: "text-success" },
    Warning: { name: "exclamation-triangle", class: "text-warning" },
    Error: { name: "x-circle", class: "text-danger" },
    Info: { name: "info-circle", class: "text-info" },
    Question: { name: "question-circle", class: "text-info" },
}


// ==================================================
$(function () {
    Modal.init();
});


























//// ==================================================
//// Common Modals

//Modal.Common = {
//    //m_IsOpen = false,
//    //m_StacksCount = 1,
//    //m_InitialTitleHtml = "",

//    //m_Modal = null,






//};









//Modal.Common.m_IsOpen = false;
//Modal.Common.m_StacksCount = 1;
//Modal.Common.m_InitialTitleHtml = "";
//Modal.Common.m_Modal = null;

//Modal.Common.Element = {};
//Modal.Common.Element.m_Header = null;
//Modal.Common.Element.m_Body = null;
//Modal.Common.Element.m_Footer = null;


//$(document).ready(function () {
//    Modal.Common.m_Modal = $("#modal-common");
//    Modal.Common.Element.m_Header = $("#modal-common-header");
//    Modal.Common.Element.m_Body = $("#modal-common-body");

//    Modal.Common.Element.m_Footer = $("#modal-common-footer");



//    Modal.Common.m_Modal.on("shown.bs.modal", function () { Modal.Common.m_IsOpen = true; });
//    Modal.Common.m_Modal.on("hidden.bs.modal", function () {
//        Modal.Common.m_IsOpen = false;
//        Modal.Common.m_StacksCount = 1;
//        Modal.Common.m_InitialTitleHtml = "";
//    });
//});


//Modal.Common.openOneBtnModal = function (_titleHtml, _content, _iconName = null, _btnLabel = "OK", _actionName = "", _addCloseBtn = true) {
//    let header = "";
//    let body = "";

//    let btnData = [
//        { label: _btnLabel, actionName: _actionName, dismiss: true, className: "btn-secondary" }
//    ];

//    if (Modal.Common.m_IsOpen) {
//        ++Modal.Common.m_StacksCount;
//        header = Modal.Common.constructHeader(P24Localization[LOCL_STR_MULTIPLE_MSG] + " (" + Modal.Common.m_StacksCount + ")", true);

//        if (Modal.Common.m_StacksCount == 2) {
//            body = Modal.Common.constructHeader(Modal.Common.m_InitialTitleHtml, false);
//        }

//        body += Modal.Common.Element.m_Body.html()
//            + "<hr />"
//            + "<div class=\"mb-2\">" + Modal.Common.constructHeader(_titleHtml, false) + "</div>"
//            + Modal.Common.constructBody(Modal.Common.constructSvgIcon(_iconName), _content);

//        btnData.label = "OK";
//    } else {
//        Modal.Common.m_InitialTitleHtml = _titleHtml;

//        header = Modal.Common.constructHeader(_titleHtml, _addCloseBtn);
//        body = Modal.Common.constructBody(Modal.Common.constructSvgIcon(_iconName), _content);
//    }

//    let footer = Modal.Common.constructFooter(1, btnData);

//    Modal.Common.openCustomized(header, body, footer);
//}

//Modal.Common.openTwoBtnModal = function (_titleHtml, _content, _iconName = null, _btnLabel0 = "Yes", _actionName0 = "", _btnLabel1 = "No", _actionName1 = "", _addCloseBtn = true) {
//    let btnData = [
//        { label: _btnLabel0, actionName: _actionName0, dismiss: true, className: "btn-primary" },
//        { label: _btnLabel1, actionName: _actionName1, dismiss: true, className: "btn-secondary" }
//    ];

//    let header = Modal.Common.constructHeader(_titleHtml, _addCloseBtn);
//    let body = Modal.Common.constructBody(Modal.Common.constructSvgIcon(_iconName), _content);
//    let footer = Modal.Common.constructFooter(2, btnData);

//    Modal.Common.openCustomized(header, body, footer);
//}

//Modal.Common.openCustomized = function (_header, _body, _footer) {
//    Modal.Common.Element.m_Header.html(_header);
//    Modal.Common.Element.m_Body.html(_body);
//    Modal.Common.Element.m_Footer.html(_footer);

//    let modal = bootstrap.Modal.getOrCreateInstance(Modal.Common.m_Modal[0], { focus: true });
//    //if (Modal.Common.m_IsOpen)
//    //    modal.hide();

//    modal.show();
//}

///*
// *
// */
//Modal.Common.constructHeader = function (_titleHtml, _addCloseBtn) {
//    let html = "<h5 class=\"modal-title\">" + _titleHtml + "</h5>";

//    if (_addCloseBtn) {
//        html += "\n<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"modal\"></button>";
//    }

//    return html;
//}

///*
// *
// */
//Modal.Common.constructBody = function (_iconHtml, _contentHtml) {
//    if (_iconHtml == null)
//        return "<div>" + _contentHtml + "</div>";

//    let html = "<div class=\"d-flex\">"
//        + "<div class=\"col-2 text-center align-self-center\">" + _iconHtml + "</div>"
//        + "<div class=\"col-10\">" + _contentHtml + "</div>"
//        + "</div>";

//    return html;
//}

///*
// *
// */
//Modal.Common.constructFooter = function (_btnCount, _btnData) {
//    if (_btnCount > _btnData.length) {
//        console.warn("Modal.Common.constructFooter(" + _btnCount + ", " + _btnData.length + "): not enough button data supported.");
//        _btnCount = _btnData.length;
//    } else if (_btnCount < _btnData.length) {
//        console.log("Modal.Common.constructFooter(" + _btnCount + ", " + _btnData.length + "): exceed button data will be ignored.");
//    }

//    let html = "";
//    for (let i = 0; i < _btnCount; ++i) {
//        let btnLabel = _btnData[i].label;
//        let btnAction = _btnData[i].actionName;
//        let btnClass = _btnData[i].className;

//        html += "<button type=\"button\" class=\"btn " + btnClass + "\"";

//        if (_btnData[i].dismiss) {
//            html += "data-bs-dismiss=\"modal\"";
//        }

//        html += " onclick=\"" + btnAction + "\">" + btnLabel + "</button>\n";
//    }

//    return html;
//}

//Modal.Common.constructSvgIcon = function (_name) {
//    const size = 48;

//    let html = null;

//    switch (_name) {
//        case MODAL_ICON_SUCCESS:
//            html = P24Utils.svg("check-circle", "text-success");
//            break;

//        case MODAL_ICON_WARNING:
//            html = P24Utils.svg("exclamation-triangle", "text-warning");
//            break;

//        case MODAL_ICON_ERROR:
//            html = P24Utils.svg("x-circle", "text-danger");
//            break;

//        case MODAL_ICON_INFO:
//            html = P24Utils.svg("info-circle", "text-info");
//            break;

//        case MODAL_ICON_QUESTION:
//            html = P24Utils.svg("question-circle", "text-info");
//            break;

//        default:
//            return "";
//    }

//    html = html.replace("width=\"16\"", "width=\"" + size + "\"");
//    html = html.replace("height=\"16\"", "height=\"" + size + "\"");

//    return html;
//}

//// END: Common Modals
//// ==================================================
