/*  ~/clinic-manager/inventory/report-in-out.js
 *  Version: 1.0 (2023.02.13)
 *
 *  Contributor
 *      Arime-chan
 */

window.ReportListPage = {};

ReportListPage.Backend = {};
ReportListPage.Backend.m_DrugListing = null;

ReportListPage.Elements = {};
ReportListPage.Elements.m_SelectYear = null;
ReportListPage.Elements.m_SelectMonth = null;
ReportListPage.Elements.m_InputName = null;
ReportListPage.Elements.m_TbodyListing = null;

$(document).ready(function () {

    ReportListPage.Elements.m_SelectYear = $("#select-year");
    ReportListPage.Elements.m_SelectMonth = $("#select-month");
    ReportListPage.Elements.m_InputName = $("#input-name");
    ReportListPage.Elements.m_TbodyListing = $("#tbody-listing");

    let year = ReportListPage.Elements.m_SelectYear.find(":selected").text();
    ReportListPage.ajax_fetchSelectableMonths(year);
});


// ==================================================
// ajax request sender

ReportListPage.ajax_fetchSelectableMonths = function (_year) {
    $.ajax({
        type: "GET",
        url: "/ClinicManager/Inventory/Report/Monthly/List?handler=FetchSelectableMonths&_year=" + _year,
        success: ReportListPage.ajax_fetchSelectableMonths_success,
        error: Modals.ajax_error
    });
}

ReportListPage.ajax_fetchReportData = function (_year, _month) {
    $.ajax({
        type: "GET",
        url: "/ClinicManager/Inventory/Report/Monthly/List?handler=FetchReportData&_year=" + _year + "&_month=" + _month,
        success: ReportListPage.ajax_fetchReportData_success,
        error: Modals.ajax_error
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// event

function selectMonth_onChange() {
    let month = ReportListPage.Elements.m_SelectMonth.find(":selected").text();
    let year = ReportListPage.Elements.m_SelectYear.find(":selected").text();

    ReportListPage.ajax_fetchReportData(year, month);
}

function btnFilter_onClick() {
    let name = ReportListPage.Elements.m_InputName.val();
    ReportListPage.populateTable(name);
}

function btnClearFilter_onClick() {
    ReportListPage.Elements.m_InputName.val("");
    ReportListPage.populateTable();
}

ReportListPage.ajax_fetchSelectableMonths_success = function (_content, _textStatus, _xhr) {
    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        let months = JSON.parse(_content.substring(6));
        let thisMonth = new Date().getMonth() + 1;
        let html = "";

        for (let i = 0; i < months.length; ++i) {
            html += "<option";
            if (months[i] == thisMonth) {
                html += " selected";
            }

            html += ">" + months[i] + "</option>"
        }

        ReportListPage.Elements.m_SelectMonth.html(html);

        selectMonth_onChange();
    }
}

ReportListPage.ajax_fetchReportData_success = function (_content, _textStatus, _xhr) {
    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        let data = JSON.parse(_content.substring(6));
        ReportListPage.Backend.m_DrugListing = data;

        ReportListPage.populateTable();
    }
}

// END: event
// ==================================================

// ==================================================
// helper

ReportListPage.constructRow = function (_data) {
    let html = "";
    html += "<tr>";
    //html += "<td>" + _data.Id + "</td>";
    html += "<td>" + _data.Name + "</td>";
    //html += "<td>";
    //if (_data.Note != null) {
    //    html += _data.Note;
    //}
    //html += "</td>";
    html += "<td><div class=\"d-flex\">"
    html += "<span class=\"text-success col-6\">+" + _data.AmountIn + "</span>";
    html += "<span class=\"text-danger col-6\">-" + _data.AmountOut + "</span>";
    html += "</div></td>"
    html += "<td>" + _data.Unit + "</td>";
    html += "</tr>";

    return html;
}

ReportListPage.populateTable = function (_filterName = "") {
    let data = ReportListPage.Backend.m_DrugListing;
    let html = "";

    for (let i = 0; i < data.length; ++i) {
        if (data[i].Name.includes(_filterName))
            html += ReportListPage.constructRow(data[i]);
    }

    ReportListPage.Elements.m_TbodyListing.html(html);
}

// END: helper
// ==================================================
