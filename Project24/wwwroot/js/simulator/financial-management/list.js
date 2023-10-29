/*  simulator/financial-management/list.js
    Version: v1.3 (2023.10.29)

    Author
        Arime-chan
 */

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

        for (const item of this.Data.PageData.Transactions) {
            if (item.Id == _id) {
                record = item;
                break;
            }
        }

        if (record == null) {
            console.warn("No record with id '" + _id + "'.");
            return;
        }

        let addedDate = DotNetString.formatCustomDateTime("yyyy/MM/dd HH:mm", record.AddedDate);

        let html = "<div>Are you sure you want to remove record #" + _id + "?</div>"
            + "<div class=\"mt-2 ms-4\">"
            + "<div>Added Date: <code>" + addedDate + "</code></div>"
            + "<div>Category: <code>" + record.Category + "</code></div>"
            + "<div>Amount: <code>" + record.Amount + "</code></div>"
            + "<div>Details: <code>" + record.Details + "</code></div>"
            + "</div>";

        Modal.Common.openTwoBtnModal("Remove Record", html, MODAL_ICON_QUESTION, "Yes", "FinManListPage.removeRecord(" + _id + ")");
    },

    removeRecord: function (_id) {
        this.ajax_removeRecord(_id);
    },

    // ==================================================

    openSyncInProgressModal: function () {
        Modal.Common.openOneBtnModal("Sync In Progress", "Sync in progress. Please wait until data is done being sync.", MODAL_ICON_INFO);
    },

    openImportInProgressModal: function () {
        Modal.Common.openOneBtnModal("Import In Progress", "Import in progress. Please wait until data is done being imported.", MODAL_ICON_INFO);
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
            data: JSON.stringify(_id),
            cache: false,
            contentType: "application/json",
            processData: false,
            success: function (_content, _textStatus, _xhr) { FinManListPage.ajax_removeRecord_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { FinManListPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_import: function (_file) {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        let formData = new FormData();
        formData.append("_file", _file);

        $.ajax({
            type: "POST",
            url: "List?handler=Import",
            headers: { RequestVerificationToken: token },
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (_content, _textStatus, _xhr) { FinManListPage.ajax_import_success(_content, _textStatus, _xhr); },
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

        this.UI.clearPage();
        this.UI.refreshPageButtons(false);

        let body = _content.substring(6);

        //console.log(body);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body)) {
            this.UI.refreshMonthNavBar();
            this.UI.refreshPageButtons(false);
            return;
        }

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
        if (processedData == null) {
            return;
        }

        this.UI.refreshPage(processedData);
        this.UI.refreshPageButtons(true);
    },

    ajax_removeRecord_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        let body = _content.substring(6);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body)) {
            return;
        }

        let arr = body.split(",");

        Modal.Common.openOneBtnModal("Success", arr[1], MODAL_ICON_SUCCESS);

        let id = +arr[0];
        let transaction = null;
        let newArray = [];
        
        for (const item of this.Data.PageData.Transactions) {
            if (item.Id != id) {
                newArray.push(item);
            }
            else {
                transaction = item;
            }
        }

        if (transaction == null) {
            console.log("Transaction '" + id + "' not found (this should not happen).");
            return;
        }

        this.Data.PageData.BalanceOut -= transaction.Amount;
        this.Data.PageData.Transactions = newArray;

        this.UI.refreshPage(this.Data.PageData);
    },

    ajax_import_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        let body = _content.substring(6);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body)) {
            return;
        }

        this.UI.refreshPageButtons(false);

        if (body == "Import") {
            Modal.Common.openOneBtnModal("Success", "Import success. Please wait while data is being imported.", MODAL_ICON_SUCCESS);
            return;
        }

        if (body == "ImportInProgress") {
            Modal.Common.openOneBtnModal("Import In Progress", "Another import is in progress. Please wait until data is done being imported. This import will be discarded.", MODAL_ICON_SUCCESS);
            return;
        }

        if (body == "SyncInProgress") {
            this.openSyncInProgressModal();
            return;
        }

        console.warn("Server returned success but message is invalid >> " + body);
    },
};

FinManListPage.Data = {
    PageData: null,


    init: function () {
    },

    // ==================================================

    processPageData: function (_json) {
        let parsedData = JSON.parse(_json);

        for (record of parsedData.Transactions) {
            record.AddedDate = new Date(record.AddedDate);
        }

        //console.log(_json);
        //console.log(parsedData);

        this.PageData = parsedData;
        return parsedData;
    },

};

FinManListPage.UI = {
    m_DivReportInfo: null,
    m_DivTransactionList: null,

    m_InputSelectYear: null,
    m_InputFile: null,

    m_RowStyles: [],


    init: function () {
        this.m_DivReportInfo = $("#div-report-info");
        this.m_DivTransactionList = $("#div-transaction-list");

        this.m_InputSelectYear = $("#select-year");
        this.m_InputFile = $("#input-file");

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

        this.refreshMonthNavBar();
        this.refreshPageButtons();
    },

    // ==================================================

    selectYear_onchange: function () {
        this.refreshMonthNavBar();
    },

    btnMonth_onclick: function (_month) {
        let year = +this.m_InputSelectYear.val();

        FinManListPage.loadMonth(year, _month + 1);
    },

    inputFile_onchange: function () {
        let file = this.m_InputFile[0].files[0];
        if (file == null)
            return;

        let strSize = P24Utils.formatDataLength(file.size);

        let promptMsg = "Are you sure you want to upload this file?\n"
            + "    File name: " + file.name + "\n"
            + "    Size: " + strSize + "\n"
            + "WARNING: This is the last confirmation. Database will be overwritten with data in this file!";

        let confirm = window.confirm(promptMsg);

        if (!confirm) {
            this.m_InputFile.val(null);
            return;
        }

        FinManListPage.ajax_import(file);
        this.m_InputFile.val(null);
    },

    // ==================================================

    clearPage: function () {
        this.m_DivReportInfo.html("");
        this.m_DivTransactionList.html("");

        this.refreshMonthNavBar(true);
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
            html += "<b class=\"text-success\">+" + P24Utils.formatNumberWithSeparator(offset) + "</b>";
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

        this.refreshMonthNavBar();
    },

    refreshMonthNavBar: function (_clear = false) {
        let date = new Date();
        let thisYear = date.getFullYear();
        let thisMonth = date.getMonth();

        let selectedYear = +this.m_InputSelectYear.val();

        for (let i = 0; i < 12; ++i) {
            let element = $("#btn-month-" + i);

            if (_clear || (selectedYear == 2023 && i < 3) || (selectedYear == thisYear && i > thisMonth)) {
                element.removeAttr("data-bs-toggle");
                element.removeAttr("data-bs-target");
                element.removeAttr("onclick");
                element.addClass("disabled");
            }
            else {
                element.attr("data-bs-toggle", "tab");
                element.attr("data-bs-target", "#div-tab-content");
                element.attr("onclick", "FinManListPage.UI.btnMonth_onclick(" + i + ")");
                element.removeClass("disabled");
            }
        }
    },

    refreshPageButtons: function (_enabled) {
        let btnAdd = $("#link-add");
        let btnExport = $("#link-export");
        let btnImport = $("#link-import");

        if (_enabled) {
            this.m_InputFile.removeAttr("disabled");
            btnAdd.removeClass("disabled");
            btnExport.removeClass("disabled");
            btnImport.removeClass("disabled");
        } else {
            this.m_InputFile.attr("disabled", true);
            btnAdd.addClass("disabled");
            btnExport.addClass("disabled");
            btnImport.addClass("disabled");
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

        let html = "<div id=\"r" + _record.Id + "\" class=\"d-flex flex-nowrap border-bottom py-1\">"
            + "<div class=\"px-2\" style=\"width:" + this.m_RowStyles[0] + "\">" + dateString + "</div>"
            + "<div class=\"px-2\" style=\"width:" + this.m_RowStyles[1] + "\">" + _record.Category + "</div>"
            + "<div class=\"px-2 text-end text-" + classAmount + "\" style=\"width:" + this.m_RowStyles[2] + "\">" + amountString + "</div>"
            + "<div class=\"px-2\" style=\"width:" + this.m_RowStyles[3] + "\">" + details + "</div>"
            + "<div style=\"width:" + this.m_RowStyles[4] + "\">"
            + "<a href=\"#\" class=\"text-danger\" onclick=\"FinManListPage.confirmRemoveRecord('" + _record.Id + "')\">" + P24Utils.svg("x-lg") + "</a>"
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
