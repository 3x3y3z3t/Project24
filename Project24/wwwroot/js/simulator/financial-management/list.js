/*  simulator/financial-management/list.js
    Version: v1.0 (2023.09.24)

    Author
        Arime-chan
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


window.FinManListPage = {
    Data: null,
    UI: null,

    m_AwaitingData: false,


    init: function () {
        this.Data.init();
        this.UI.init();

    },

    reload: function () {
        let date = new Date();
        this.loadMonth(date.getFullYear(), date.getMonth() + 1);
    },

    // ==================================================

    loadMonth: function (_year, _month) {
        this.ajax_fetchPageData(_year, _month);
    },

    confirmRemoveRecord: function (_id) {
        let record = null;

        for (const item of this.Data.Transactions) {
            if (item.Id == _id) {
                record = item;
                return;
            }
        }

        if (record == null) {
            console.warn("No record with id '" + _id + "'.");
            return;
        }

        let html = "<div>Are you sure you want to remove record #" + _id + "?</div>"
            + "<div class=\"mt-2 ms-4\">"
            + "<div>Added Date: <code>" + record.getAddedDateAsString() + "</code></div>"
            + "<div>Category: <code>" + record.Category + "</code></div>"
            + "<div>Amount: <code>" + record.Amount + "</code></div>"
            + "<div>Details: <code>" + record.Details + "</code></div>"
            + "</div>";

        Modal.Common.openTwoBtnModal("Remove Record", html, MODAL_ICON_QUESTION, "Yes", "FinManCreatePage.removeRecord(" + _id + ")");
    },

    removeRecord: function (_id) {
        this.ajax_removeRecord(_id);
    },

    // ==================================================

    ajax_fetchPageData: function (_year, _month) {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;

        $.ajax({
            type: "GET",
            url: "List?handler=FetchPageData&_year=" + _year + "&_month=" + _month,
            success: function (_content, _textStatus, _xhr) { FinManListPage.ajax_fetchPageData_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { FinManListPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_removeRecord: function (_id) {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            type: "POST",
            url: "Remove",
            headers: { RequestVerificationToken: token },
            data: JSON.stringify({ _id: _id }),
            cache: false,
            contenttype: "application/json; charset=utf-8",
            success: function (_content, _textStatus, _xhr) { FinManListPage.ajax_removeRecord_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { FinManListPage.ajax_error(_xhr, _textStatus, _errorThrow); }
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

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body)) {
            this.UI.clearPage();
            return;
        }

        let processedData = this.Data.processPageData(body);
        if (processedData == null) {
            this.UI.clearPage();
            return;
        }

        this.UI.refreshPage(processedData);
    },

    //ajax_submit_success: function (_content, _textStatus, _xhr) {
    //    this.m_AwaitingData = false;

    //    this.UI.m_BtnSubmit.removeAttr("disabled");
    //    let body = _content.substring(6);

    //    if (_content.startsWith(P24_MSG_TAG_EXCEPTION)) {
    //        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_EXCEPTION], "<pre>" + _body + "</pre>");
    //        return;
    //    }

    //    if (_content.startsWith(P24_MSG_TAG_ERROR)) {
    //        Modal.Common.openOneBtnModal("Error", _body, MODAL_ICON_ERROR);
    //        return;
    //    }

    //    if (!_content.startsWith(P24_MSG_TAG_SUCCESS)) {
    //        console.error("ajax_submit_success(): Unknown error: \n" + _content);
    //        return;
    //    }

    //    this.Data.AddedData = [];

    //    let processData = this.Data.processPageData(body);
    //    if (processData != null)
    //        this.UI.refreshPage(processData);

    //    Modal.Common.openOneBtnModal("Success", "", MODAL_ICON_SUCCESS);

    //},

    //ajax_submit_error: function (_xhr, _textStatus, _errorThrow) {
    //    this.m_AwaitingData = false;
    //    this.UI.m_BtnSubmit.removeAttr("disabled");

    //    P24Utils.Ajax.error(_xhr, _textStatus, _errorThrown);
    //},
};

FinManListPage.Data = {
    PageData: null,
    //AddedData: null,


    init: function () {
        //this.AddedData = [];
    },

    // ==================================================

    //addRecord: function (_date, _category, _amount, _details) {
    //    this.AddedData.push(new Record(_date, _category, _amount, _details));
    //},

    //removeRecord: function (_index) {
    //    this.AddedData[_index] = null;
    //},

    // ==================================================

    processPageData: function (_json) {
        let parsedData = JSON.parse(_json);

        for (record of parsedData.Transactions) {
            record.AddedDate = new Date(record.AddedDate);
        }

        //console.log(_json);
        console.log(parsedData);

        return parsedData;
    },

};

FinManListPage.UI = {
    m_DivReportInfo: null,
    m_DivTransactionList: null,

    m_InputSelectYear: null,

    m_RowStyles: [],


    init: function () {
        this.m_DivReportInfo = $("#div-report-info");
        this.m_DivTransactionList = $("#div-transaction-list");

        this.m_InputSelectYear = $("#select-year");

        this.m_RowStyles = ["20em", "25em", "15em", "100%", "16px"];
        for (let i = 0; i < this.m_RowStyles.length; ++i) {
            $("#tr" + i).css("width", this.m_RowStyles[i]);
        }

       // init Year select box;
        let html = "";
        let thisYear = (new Date()).getFullYear();
        for (let i = 2023; i <= thisYear; ++i) {
            html += "<option value=\"" + i + "\"";
            if (i == thisYear)
                html += " selected";
            html += ">" + i + "</option>";
        }
        this.m_InputSelectYear.html(html);

        // init Month nav bar;
        this.refreshMonthNavBar();
    },

    // ==================================================

    //addRecord: function (_date, _category, _amount, _details) {
    //},

    //removeRecord: function (_index) {
    //    $("#r" + _index).remove();
    //},

    // ==================================================

    selectYear_onchange: function () {

    },

    btnMonth_onclick: function (_month) {
        let year = 2023;

        // TODO: get yyear;

        FinManListPage.loadMonth(year, _month + 1);
    },

    //btnAdd_onclick: function () {
    //    let date = new Date(this.m_InputAddDate.val());
    //    let category = this.m_InputCategory.val();
    //    let amount = this.m_InputAmount.val();
    //    let details = this.m_InputDetails.val();

    //    let formElement = $(".needs-validation");
    //    formElement.addClass("was-validated");
    //    if (!formElement[0].checkValidity()) {
    //        return;
    //    }

    //    let divElement = $("#div-invalid-date");
    //    if (isNaN(date)) {
    //        divElement.removeAttr("hidden");
    //        return;
    //    }

    //    FinManListPage.addRecord(date, category, amount, details);

    //    this.clearInputs();

    //    this.m_BtnSubmit.removeAttr("disabled");
    //},

    //btnClear_onclick: function () {
    //    this.clearInputs();
    //},

    //btnSubmit_onclick: function () {
    //    FinManListPage.submitRecords();

    //    this.m_BtnSubmit.attr("disabled", true);
    //},

    // ==================================================

    clearPage: function () {
        this.m_DivReportInfo.html("");
        this.m_DivTransactionList.html("");
    },

    refreshPage: function (_data) {

        let balanceIn = P24Utils.formatNumberWithSeparator(_data.BalanceIn);
        let balanceOut = P24Utils.formatNumberWithSeparator(_data.BalanceOut);
        let offset = _data.BalanceOut - _data.BalanceIn;

        let html = "<div>Monthly report for <b class=\"text-primary\">" + _data.Year + "/" + String(_data.Month).padStart(2, '0') + "</b>:</div>"
            + "<div class=\"ms-2\">"

            + "<div class=\"text-break\">Balance: <b>" + balanceIn + "</b> → <b>" + balanceOut + "</b> (";
        if (offset == 0) {
            html += "unchanged";
        } else if (offset > 0) {
            html += "<b class=\"text-success\">+" + P24Utils.formatNumberWithSeparator(offset)+ "</b>";
        } else if (offset < 0) {
            html += "<b class=\"text-danger\">" + P24Utils.formatNumberWithSeparator(offset) + "</b>";
        }
        html += ")</div>"
        html += "<div>Transaction Count: " + _data.Transactions.length + "</div>"

            + "</div>";

        this.m_DivReportInfo.html(html);


        html = "";
        for (const record of _data.Transactions) {
            html += this.construcSingleRecordHtml(record);
        }

        this.m_DivTransactionList.html(html);
    },

    refreshMonthNavBar: function () {
        let date = new Date();
        let thisYear = date.getFullYear();
        let thisMonth = date.getMonth();

        let selectedYear = +this.m_InputSelectYear.val();

        for (let i = 0; i < 12; ++i) {
            let element = $("#btn-month-" + i);

            if ((selectedYear == 2023 && i < 3) || (selectedYear == thisYear && i > thisMonth)) {
                element.removeAttr("data-bs-toggle");
                element.removeAttr("data-bs-target");
                element.removeAttr("onclick");
                element.addClass("disabled");
            }
        }
    },

    // ==================================================

    construcSingleRecordHtml: function (_record) {
        // ==========;
        let dateString = DotNetString.formatCustomDateTime("yyyy/MM/dd HH:mm", _record.AddedDate);

        // ==========;
        let classAmount = "success";
        let amountString = "";

        if (_record.Amount > 0) {
            amountString = "+";
        } else if (_record.Amount < 0) {
            classAmount = "danger";
        }
        amountString += P24Utils.formatNumberWithSeparator(_record.Amount);

        // ==========;
        let details = "";
        if (_record.Details != null && _record.Details != "") {
            let lines = _record.Details.split("\n");

            for (let i = 0; i < lines.length; ++i) {
                details += "<div>" + lines[i] + "</div>";
            }
        }

    //    index = FinManListPage.Data.AddedData.length - 1;

        let html = "<div id=\"r" + _record.Id + "\" class=\"d-flex flex-nowrap border-bottom py-1\">"
            + "<div class=\"px-2\" style=\"width:" + this.m_RowStyles[0] + "\">" + dateString + "</div>"
            + "<div class=\"px-2\" style=\"width:" + this.m_RowStyles[1] + "\">" + _record.Category + "</div>"
            + "<div class=\"px-2 text-end text-" + classAmount + "\" style=\"width:" + this.m_RowStyles[2] + "\">" + amountString + "</div>"
            + "<div class=\"px-2\" style=\"width:" + this.m_RowStyles[3] + "\">" + details + "</div>"
            + "<div style=\"width:" + this.m_RowStyles[4] + "\">"
            + "<a href=\"#\" class=\"text-danger\" onclick=\"FinManCreatePage.confirmRemoveRecord('" + _record.Id + "')\">" + P24Utils.svg("x-lg") + "</a>"
            + "</div></div>";

        return html;
    },



    //clearInputs: function () {
    //    this.m_InputAddDate.val("");
    //    this.m_InputCategory.val("");
    //    this.m_InputAmount.val("");
    //    this.m_InputDetails.val("");

    //    let formElement = $(".needs-validation");
    //    formElement.removeClass("was-validated");

    //    let divElement = $("#div-invalid-date");
    //    divElement.attr("hidden", true);
    //},

    //clearTable: function () {
    //    this.m_DivAddList.html();
    //},
};


// ==================================================
$(function () {
    FinManListPage.init();
    FinManListPage.reload();

});
