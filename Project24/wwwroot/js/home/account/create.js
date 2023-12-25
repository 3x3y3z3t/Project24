/*  home/account/create.js
    Version: v1.0 (2023.12.24)
    Spec:    v0.1

    Contributor
        Arime-chan (Author)
 */

//class Record {
//    AddedDate = null;
//    Category = null;
//    Amount = 0;
//    Details = null;


//    constructor(_addedDate, _category, _amount, _details = null) {
//        this.AddedDate = _addedDate;
//        this.Category = _category;
//        this.Amount = _amount;
//        this.Details = _details;
//    }


//    getAddedDateAsString() {
//        return DotNetString.formatCustomDateTime("yyyy/MM/dd HH:mm", this.AddedDate);
//    }
//}


window.AccCreatePage = {
    Data: null,
    UI: null,

    m_AwaitingData: false,


    init: function () {
        this.Data.init();
        this.UI.init();
    },

    // ==================================================

    confirmCreate: function (_username) {
        this.ajax_create(_username);
    },

    backToList: function () {
        window.location.href = "List";
    },

    // ==================================================

    ajax_checkUsername: function (_proposedUsername) {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;

        $.ajax({
            type: "GET",
            url: "Create?handler=suggestUsername&_proposedUsername=" + _proposedUsername,
            success: function (_content, _textStatus, _xhr) { AccCreatePage.ajax_checkUsername_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { AccCreatePage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_create: function (_username) {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            type: "POST",
            url: "Create",
            headers: { RequestVerificationToken: token },
            data: JSON.stringify(_username),
            //data: _username,
            cache: false,
            contentType: "application/json; charset=utf-8",
            //contentType: "text/plain; charset=utf-8",
            success: function (_content, _textStatus, _xhr) { AccCreatePage.ajax_create_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { AccCreatePage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    // ==================================================

    ajax_error: function (_xhr, _textStatus, _errorThrown) {
        this.m_AwaitingData = false;
        P24Utils.Ajax.error(_xhr, _textStatus, _errorThrown);
    },
    
    ajax_checkUsername_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        let body = _content.substring(6);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body))
            return;

        if (_content.startsWith(P24_MSG_TAG_SUCCESS)) {
            this.UI.m_InputUsername.val(body);
            let msg = "Do you want to create account <code>" + body + "</code>?";
            Modal.Common.openTwoBtnModal(P24Localization[LOCL_STR_CONFIRM], msg, MODAL_ICON_QUESTION, "Yes", "AccCreatePage.confirmCreate('" + body + "')");
        }
    },

    ajax_create_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        let body = _content.substring(6);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body))
            return;

        if (_content.startsWith(P24_MSG_TAG_SUCCESS)) {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_SUCCESS], body, MODAL_ICON_SUCCESS, "Back to List", "AccCreatePage.backToList()", false);
        }
    },

    // ==================================================









};

AccCreatePage.Data = {


    init: function () {
    }
};

AccCreatePage.UI = {
    m_InputUsername: null,
    m_InputFirstName: null,
    m_InputLastName: null,

    m_BtnSubmit: null,
    m_BtnClear: null,


    init: function () {
        this.m_InputUsername = $('#input-username');
        this.m_InputFirstName = $('#input-firstname');
        this.m_InputLastName = $('#input-lastname');

        this.m_BtnSubmit = $('#btn-submit');
        this.m_BtnClear = $('#btn-clear');
    },

    // ==================================================

    input_onchange: function () {

    },

    btnSubmit_onclick: function () {
        let formElement = $(".needs-validation");
        formElement.addClass("was-validated");
        if (!formElement[0].checkValidity()) {
            return;
        }

        // TODO: validate username;
        //

        let username = this.m_InputUsername.val();
        AccCreatePage.ajax_checkUsername(username);

        //let date = new Date(this.m_InputAddDate.val());
        //let category = this.m_InputCategory.val();
        //let amount = this.m_InputAmount.val();
        //let details = this.m_InputDetails.val();

        //let formElement = $(".needs-validation");
        //formElement.addClass("was-validated");
        //if (!formElement[0].checkValidity()) {
        //    return;
        //}

        //let divElement = $("#div-invalid-date");
        //if (isNaN(date)) {
        //    divElement.removeAttr("hidden");
        //    return;
        //}

        //if (!FinManCreatePage.Data.containsCategory(category)) {
        //    this.m_DataListCategories.append("<option value=\"" + category + "\"></option>");
        //}

        //if (this.m_CheckCard.prop("checked")) {
        //    if (!FinManCreatePage.Data.containsCategory("Card Pay")) {
        //        this.m_DataListCategories.append("<option value=\"Card Pay\"></option>");
        //    }

        //    // add a dummy as card withdrawal;
        //    let cardDate = new Date(date.getTime() - 1);
        //    FinManCreatePage.addRecord(cardDate, "Card Pay", -amount);
        //}

        //FinManCreatePage.addRecord(date, category, amount, details);

        //this.clearInputs();
        //this.m_BtnSubmit.removeAttr("disabled");
    },
    
    btnClear_onclick: function () {
        this.clearInputs();
    },

    // ==================================================
};


// ==================================================
$(function () {
    AccCreatePage.init();
    
});




window.FinManCreatePage = {
    Data: null,
    UI: null,

    m_AwaitingData: false,

    
    reload: function () {
        this.ajax_fetchPageData();
    },

    // ==================================================

    addRecord: function (_addedDate, _category, _amount, _details = "") {
        this.Data.addRecord(_addedDate, _category, +_amount, _details);
        this.UI.addRecord(_addedDate, _category, +_amount, _details);
    },

    confirmRemoveRecord: function (_index) {
        let record = this.Data.AddedData[_index];

        let html = "<div>Are you sure you want to remove record #" + _index + "?</div>"
            + "<div class=\"mt-2 ms-4\">"
            + "<div>Added Date: <code>" + record.getAddedDateAsString() + "</code></div>"
            + "<div>Category: <code>" + record.Category + "</code></div>"
            + "<div>Amount: <code>" + record.Amount + "</code></div>"
            + "<div>Details: <code>" + record.Details + "</code></div>"
            + "</div>";

        Modal.Common.openTwoBtnModal("Remove Record", html, MODAL_ICON_QUESTION, "Yes", "FinManCreatePage.removeRecord(" + _index + ")");
    },

    removeRecord: function (_index) {
        if (_index < 0 || _index > this.Data.AddedData.length || this.Data.AddedData[_index] == null)
            return;

        this.Data.removeRecord(_index);
        this.UI.removeRecord(_index);
    },

    submitRecords: function () {
        this.ajax_submit();
    },

    // ==================================================

    openSyncInProgressModal: function () {
        Modal.Common.openOneBtnModal("Sync In Progress", "Sync in progress. Please wait until data is done being sync.", MODAL_ICON_INFO);
    },

    openImportInProgressModal: function () {
        Modal.Common.openOneBtnModal("Import In Progress", "Import in progress. Please wait until data is done being imported.", MODAL_ICON_INFO);
    },

    // ==================================================

    ajax_fetchPageData: function () {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;

        $.ajax({
            type: "GET",
            url: "Create?handler=FetchPageData",
            success: function (_content, _textStatus, _xhr) { FinManCreatePage.ajax_fetchPageData_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { FinManCreatePage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_submit: function () {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            type: "POST",
            url: "Create",
            headers: { RequestVerificationToken: token },
            data: JSON.stringify(this.Data.AddedData),
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (_content, _textStatus, _xhr) { FinManCreatePage.ajax_submit_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { FinManCreatePage.ajax_submit_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    // ==================================================

    ajax_error: function (_xhr, _textStatus, _errorThrown) {
        this.m_AwaitingData = false;
        P24Utils.Ajax.error(_xhr, _textStatus, _errorThrown);
    },

    ajax_fetchPageData_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        let body = _content.substring(6);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body))
            return;

        if (body == "ImportInProgress") {
            this.Data.PageData = body;
            this.openImportInProgressModal();
            return;
        }

        if (body == "SyncInProgress") {
            this.Data.PageData = body;
            this.openSyncInProgressModal();
            return;
        }

        let processedData = this.Data.processPageData(body);
        if (processedData == null)
            return;

        this.UI.refreshPage(processedData);
    },

    ajax_submit_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        this.UI.m_BtnSubmit.removeAttr("disabled");
        let body = _content.substring(6);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body))
            return;

        this.Data.AddedData = [];

        let processData = this.Data.processPageData(body);
        if (processData != null)
            this.UI.refreshPage(processData);

        Modal.Common.openOneBtnModal("Success", "Data has been added.", MODAL_ICON_SUCCESS);

    },

    ajax_submit_error: function (_xhr, _textStatus, _errorThrow) {
        this.m_AwaitingData = false;
        this.UI.m_BtnSubmit.removeAttr("disabled");

        P24Utils.Ajax.error(_xhr, _textStatus, _errorThrown);
    },
};

FinManCreatePage.Data = {
    PageData: null,
    AddedData: null,


    init: function () {
        this.AddedData = [];
    },

    // ==================================================

    addRecord: function (_date, _category, _amount, _details = "") {
        this.AddedData.push(new Record(_date, _category, _amount, _details));
    },

    removeRecord: function (_index) {
        this.AddedData[_index] = null;
    },

    containsCategory: function (_category) {
        for (const category of this.PageData) {
            if (_category == category)
                return true;
        }

        for (const record of this.AddedData) {
            if (_category == record.Category)
                return true;
        }

        return false;
    },

    // ==================================================

    processPageData: function (_json) {
        let parsedData = JSON.parse(_json);

        //console.log(_json);
        //console.log(parsedData);
        this.PageData = parsedData;

        return parsedData;
    },

};

FinManCreatePage.UI = {
    m_InputAddDate: null,
    m_InputCategory: null,
    m_InputAmount: null,
    m_InputDetails: null,
    m_CheckCard: null,

    m_BtnAdd: null,
    m_BtnClear: null,
    m_BtnSubmit: null,

    m_DataListCategories: null,
    m_DivAddList: null,

    m_RowStyles: [],


    init: function () {
        this.m_InputAddDate = $("#input-add-date");
        this.m_InputCategory = $("#input-category");
        this.m_InputAmount = $("#input-amount");
        this.m_InputDetails = $("#input-details");
        this.m_CheckCard = $("#check-card");

        this.m_BtnAdd = $("#btn-add");
        this.m_BtnClear = $("#btn-clear");
        this.m_BtnSubmit = $("#btn-submit");

        this.m_DataListCategories = $("#datalist-categories");
        this.m_DivAddList = $("#div-add-list");

        this.m_RowStyles = ["20em", "25em", "15em", "100%", "16px"];
        for (let i = 0; i < this.m_RowStyles.length; ++i) {
            $("#tr" + i).css("width", this.m_RowStyles[i]);
        }

        //let dateTimeNow = DotNetString.formatCustomDateTime("yyyy-MM-ddTHH:mm", new Date());
        //this.m_InputAddDate.val(dateTimeNow);
    },

    // ==================================================

    addRecord: function (_date, _category, _amount, _details = "") {
        let dateString = DotNetString.formatCustomDateTime("yyyy/MM/dd HH:mm", _date);

        let classAmount = "success";
        let amountString = "";

        if (_amount > 0) {
            amountString = "+";
        } else if (_amount < 0) {
            classAmount = "danger";
        }

        amountString += P24Utils.formatNumberWithSeparator(_amount);

        if (_details == null)
            _details = "";

        if (_details != "") {
            let lines = _details.split("\n");
            let details = "";

            for (let i = 0; i < lines.length; ++i) {
                details += "<div>" + lines[i] + "</div>";
            }
            _details = details;
        }

        index = FinManCreatePage.Data.AddedData.length - 1;

        let html = "<div id=\"r" + index + "\" class=\"d-flex flex-nowrap border-bottom py-1\">"
            + "<div class=\"px-2\" style=\"width:" + this.m_RowStyles[0] + "\">" + dateString + "</div>"
            + "<div class=\"px-2\" style=\"width:" + this.m_RowStyles[1] + "\">" + _category + "</div>"
            + "<div class=\"px-2 text-end text-" + classAmount + "\" style=\"width:" + this.m_RowStyles[2] + "\">" + amountString + "</div>"
            + "<div class=\"px-2\" style=\"width:" + this.m_RowStyles[3] + "\">" + _details + "</div>"
            + "<div style=\"width:" + this.m_RowStyles[4] + "\">"
            + "<a href=\"#\" class=\"text-danger\" onclick=\"FinManCreatePage.confirmRemoveRecord('" + index + "')\">" + P24Utils.svg("x-lg") + "</a>"
            + "</div></div>";

        this.m_DivAddList.append(html);
    },

    removeRecord: function (_index) {
        $("#r" + _index).remove();
    },

    // ==================================================

    btnAdd_onclick: function () {
        let date = new Date(this.m_InputAddDate.val());
        let category = this.m_InputCategory.val();
        let amount = this.m_InputAmount.val();
        let details = this.m_InputDetails.val();

        let formElement = $(".needs-validation");
        formElement.addClass("was-validated");
        if (!formElement[0].checkValidity()) {
            return;
        }

        let divElement = $("#div-invalid-date");
        if (isNaN(date)) {
            divElement.removeAttr("hidden");
            return;
        }

        if (!FinManCreatePage.Data.containsCategory(category)) {
            this.m_DataListCategories.append("<option value=\"" + category + "\"></option>");
        }

        if (this.m_CheckCard.prop("checked")) {
            if (!FinManCreatePage.Data.containsCategory("Card Pay")) {
                this.m_DataListCategories.append("<option value=\"Card Pay\"></option>");
            }

            // add a dummy as card withdrawal;
            let cardDate = new Date(date.getTime() - 1);
            FinManCreatePage.addRecord(cardDate, "Card Pay", -amount);
        }

        FinManCreatePage.addRecord(date, category, amount, details);

        this.clearInputs();
        this.m_BtnSubmit.removeAttr("disabled");
    },

    btnClear_onclick: function () {
        this.clearInputs();
    },

    btnSubmit_onclick: function () {
        if (FinManCreatePage.Data.PageData == "ImportInProgress") {
            FinManCreatePage.openImportInProgressModal();
            return;
        }

        if (FinManCreatePage.Data.PageData == "SyncInProgress") {
            FinManCreatePage.openSyncInProgressModal();
            return;
        }

        FinManCreatePage.submitRecords();

        this.m_BtnSubmit.attr("disabled", true);
    },

    // ==================================================

    refreshPage: function (_data) {
        let html = "";
        for (const category of _data) {
            html += "<option value=\"" + category + "\"></option>";
        }

        this.m_DataListCategories.html(html);

        this.m_DivAddList.html("");
        this.m_BtnSubmit.attr("disabled", true);









        return;

        for (let i = 0; i < 10; ++i) {
            let num = Math.floor(Math.random() * 1000000001) - 500000000;

            let linesCount = Math.floor(Math.random() * 10) % 4;
            let details = "";
            for (let j = 0; j < linesCount; ++j) {
                details += "Line " + j + ": asdsadhasjkdhsa\n";
            }

            let date = new Date();
            FinManCreatePage.addRecord(date, "Category", num, details);
        }
        this.m_BtnSubmit.removeAttr("disabled");
    },

    clearInputs: function () {
        this.m_InputAddDate.val("");
        this.m_InputCategory.val("");
        this.m_InputAmount.val("");
        this.m_InputDetails.val("");
        this.m_CheckCard.prop("checked", false);

        let formElement = $(".needs-validation");
        formElement.removeClass("was-validated");

        let divElement = $("#div-invalid-date");
        divElement.attr("hidden", true);
    },

    clearTable: function () {
        this.m_DivAddList.html("");
    },
};
