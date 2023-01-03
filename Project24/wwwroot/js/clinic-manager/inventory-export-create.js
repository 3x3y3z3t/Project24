/*  inventory-export-create.js
 *  Version: 1.1 (2023.01.03)
 *
 *  Contributor
 *      Arime-chan
 */

window.ExportCreatePage = {};

ExportCreatePage.Backend = {};
ExportCreatePage.Backend.m_AddedList = {};
ExportCreatePage.Backend.m_AddIndex = 0;

ExportCreatePage.Backend.m_AvailableDrugsInfo = {};
ExportCreatePage.Backend.m_AddedNames = [];

ExportCreatePage.Backend.m_Timer = null;

ExportCreatePage.Elements = {};
ExportCreatePage.Elements.m_InputName = null;
ExportCreatePage.Elements.m_InputAmount = null;
ExportCreatePage.Elements.m_InputUnit = null;
ExportCreatePage.Elements.m_UlValidationMsg = null;
ExportCreatePage.Elements.m_DivStockInfo = null;
ExportCreatePage.Elements.m_TbodyAddedList = null;

ExportCreatePage.Elements.m_BtnAdd = null;
ExportCreatePage.Elements.m_BtnFinish = null;


$(document).ready(function () {

    ExportCreatePage.Elements.m_InputName = $("#input-name");
    ExportCreatePage.Elements.m_InputAmount = $("#input-amount");
    ExportCreatePage.Elements.m_InputUnit = $("#input-unit");
    ExportCreatePage.Elements.m_UlValidationMsg = $("#ul-validation-msg");
    ExportCreatePage.Elements.m_DivStockInfo = $("#div-stock-info");
    ExportCreatePage.Elements.m_TbodyAddedList = $("#tbody-added-list");

    ExportCreatePage.Elements.m_BtnAdd = $("#btn-add");
    ExportCreatePage.Elements.m_BtnFinish = $("#btn-finish");


    ExportCreatePage.ajax_fetchAvailDrugsInfo();
});

// ==================================================
// ajax request sender

ExportCreatePage.ajax_fetchAvailDrugsInfo = function () {
    $.ajax({
        type: "GET",
        url: "/ClinicManager/Inventory/List?handler=FetchAvailDrugsInfoWithAmount",
        success: ExportCreatePage.fetchAvailDrugsInfo_success,
        error: ExportCreatePage.submit_error
    });
}

ExportCreatePage.ajax_Submit = function (_data) {
    let token = $("input[name='__RequestVerificationToken']").val();

    let formData = JSON.stringify(_data);

    $.ajax({
        type: "POST",
        url: "/ClinicManager/Inventory/Export/Create",
        headers: { "RequestVerificationToken": token },
        data: formData,
        contentType: "application/json; charset=utf-8",
        success: ExportCreatePage.submit_success,
        error: ExportCreatePage.submit_error
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// event

function inputName_onInput() {
    clearTimeout(ExportCreatePage.Backend.m_Timer);
    ExportCreatePage.Backend.m_Timer = setTimeout(ExportCreatePage.displayDrugInfo, 500);
}

function btnAdd() {
    ExportCreatePage.Elements.m_UlValidationMsg.html("");
    if (!ExportCreatePage.validateName() || !ExportCreatePage.validateAmount() || !ExportCreatePage.validateUnit())
        return;

    let name = ExportCreatePage.Elements.m_InputName.val();
    let itemIndex = ExportCreatePage.Backend.m_AddedNames.indexOf(name);
    if (itemIndex > -1) {
        ExportCreatePage.openDuplicateAdditionModal(name);
        return;
    }

    let index = ExportCreatePage.Backend.m_AddIndex;
    let amount = ExportCreatePage.Elements.m_InputAmount.val();
    let unit = ExportCreatePage.Elements.m_InputUnit.val();

    ExportCreatePage.Backend.m_AddedList[index] = { name, amount, unit };
    ++ExportCreatePage.Backend.m_AddIndex;

    if (ExportCreatePage.Backend.m_AvailableDrugsInfo[name] != null)
        ExportCreatePage.Backend.m_AddedNames.push(name);

    let html = "<tr id=\"row-" + index + "\">"; // open tag;
    html += "<td>" + (index + 1) + "</td>" // index column;
    html += "<td>" + name + "</td>" // name column;
    html += "<td>" + amount + "</td>" // amount column;
    html += "<td>" + unit + "</td>" // unit column;
    html += "<td><button class=\"btn btn-sm btn-outline-danger py-0 px-1\" onclick=\"btnRowRemove('" + index + "')\">&times;</button></td>" // close button column;
    html += "</tr>" // close tag;

    ExportCreatePage.Elements.m_TbodyAddedList.append(html);

    ExportCreatePage.Elements.m_InputName.val("");
    ExportCreatePage.Elements.m_InputAmount.val("");
    ExportCreatePage.Elements.m_InputUnit.val("");
    ExportCreatePage.Elements.m_InputUnit.removeAttr("readonly");
    ExportCreatePage.Elements.m_DivStockInfo.html("");

    ExportCreatePage.updateFinishButton();
    ExportCreatePage.updateDataLists();
}

function btnRowRemove(_index) {
    if (ExportCreatePage.Backend.m_AddedList[_index] == null)
        return;

    ExportCreatePage.untrackItemWithName(ExportCreatePage.Backend.m_AddedList[_index].name);

    delete ExportCreatePage.Backend.m_AddedList[_index];
    $("#row-" + _index).remove();

    ExportCreatePage.updateFinishButton();
    ExportCreatePage.updateDataLists();
}

function btnFinish() {
    if (ExportCreatePage.getAddedCount() <= 0)
        return;

    let dataList = [];

    for (let i = 0; i < ExportCreatePage.Backend.m_AddIndex; ++i) {
        let item = ExportCreatePage.Backend.m_AddedList[i];
        if (item == null)
            continue;

        dataList.push({ "Name": item.name, "Amount": item.amount });
    }

    let ticketCode = $("#a-ticket-code").text();
    let formData = { "TicketCode": ticketCode, "Data": dataList };
    ExportCreatePage.ajax_Submit(formData);
}

function btnUpdate_modal() {

}

ExportCreatePage.fetchAvailDrugsInfo_success = function (_content, _textStatus, _xhr) {
    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        _content = _content.substring(6);

        let obj = JSON.parse(_content);
        for (let i = 0; i < obj.length; ++i) {
            let drug = obj[i];
            ExportCreatePage.Backend.m_AvailableDrugsInfo[drug.Name] = { "amount": drug.Amount, "unit": drug.Unit };
        }

        ExportCreatePage.populateDatalists();

        ExportCreatePage.Elements.m_BtnAdd.removeAttr("disabled");
        return;
    }
}

ExportCreatePage.submit_success = function (_content, _textStatus, _xhr) {
    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        let body = "<div>";
        body += "Xuất kho thành công.";
        body += "</div>";

        ExportCreatePage.Elements.m_TbodyAddedList.html("");
        ExportCreatePage.Elements.m_BtnFinish.attr("disabled", true);

        Modals.CommonInfoModal.openSuccessModal("Thành công", body, null);

        ExportCreatePage.Backend.m_AddedList = {};
        ExportCreatePage.Backend.m_AddIndex = 0;
        ExportCreatePage.Backend.m_AddedNames = [];

        ExportCreatePage.Elements.m_TbodyAddedList.html("");
        ExportCreatePage.updateFinishButton();

        return;
    }
}

ExportCreatePage.submit_error = function (_xhr, _textStatus, _errorThrown) {
    let body = "<div>Status Code: <code>" + _xhr.status + " - " + HttpStatusCodeName[_xhr.status] + "</code></div><div>jq Status: <code>" + _textStatus + "</code></div>";
    Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
}

// END: event
// ==================================================

// ==================================================
// helper

ExportCreatePage.validateName = function () {
    let html = ExportCreatePage.Elements.m_UlValidationMsg.html();

    if (ExportCreatePage.Elements.m_InputName.val() == "") {
        html += "<li>Tên thuốc không được để trống</li>";
        ExportCreatePage.Elements.m_UlValidationMsg.html(html);
        return false;
    }

    let name = ExportCreatePage.Elements.m_InputName.val();
    for (const key of Object.keys(ExportCreatePage.Backend.m_AvailableDrugsInfo)) {
        if (name == key)
            return true;
    }

    html += "<li>Tên thuốc không hợp lệ (không có trong kho)</li>";
    ExportCreatePage.Elements.m_UlValidationMsg.html(html);
    return false;
}

ExportCreatePage.validateAmount = function () {
    let html = ExportCreatePage.Elements.m_UlValidationMsg.html();

    if (ExportCreatePage.Elements.m_InputAmount.val() == "") {
        html += "<li>Số lượng không được để trống</li>";
        ExportCreatePage.Elements.m_UlValidationMsg.html(html);
        return false;
    }

    let amount = ExportCreatePage.Elements.m_InputAmount.val();
    if (amount <= 0) {
        html += "<li>Số lượng phải lớn hơn 0</li>";
        ExportCreatePage.Elements.m_UlValidationMsg.html(html);
    }

    let name = ExportCreatePage.Elements.m_InputName.val();
    let stock = ExportCreatePage.Backend.m_AvailableDrugsInfo[name].amount;
    if (amount > stock) {
        html += "<li>Số lượng không hợp lệ (không đủ số lượng trong kho)</li>";
        ExportCreatePage.Elements.m_UlValidationMsg.html(html);
        return false;
    }

    return true;
}

ExportCreatePage.validateUnit = function () {
    let html = ExportCreatePage.Elements.m_UlValidationMsg.html();

    if (ExportCreatePage.Elements.m_InputUnit.val() == "") {
        html += "<li>Đơn vị không được để trống</li>";
        ExportCreatePage.Elements.m_UlValidationMsg.html(html);
        return false;
    }

    return true;
}

ExportCreatePage.displayDrugInfo = function () {
    ExportCreatePage.Elements.m_UlValidationMsg.html("");

    let name = ExportCreatePage.Elements.m_InputName.val();

    let drug = ExportCreatePage.Backend.m_AvailableDrugsInfo[name];
    if (drug != null) {
        ExportCreatePage.Elements.m_InputUnit.val(drug.unit);
        ExportCreatePage.Elements.m_InputUnit.attr("readonly", true);

        let stockInfo = "Kho: <span class=text-success><b>" + drug.amount + " " + drug.unit + "</b></span>";
        ExportCreatePage.Elements.m_DivStockInfo.html(stockInfo);
    } else {
        ExportCreatePage.Elements.m_InputUnit.val("");
        ExportCreatePage.Elements.m_InputUnit.removeAttr("readonly");

        ExportCreatePage.Elements.m_DivStockInfo.html("");
    }
}

ExportCreatePage.updateFinishButton = function () {
    if (ExportCreatePage.getAddedCount() <= 0)
        ExportCreatePage.Elements.m_BtnFinish.attr("disabled", true);
    else
        ExportCreatePage.Elements.m_BtnFinish.removeAttr("disabled");
}

ExportCreatePage.getAddedCount = function () {
    return Object.keys(ExportCreatePage.Backend.m_AddedList).length;
}

// END: helper
// ==================================================

ExportCreatePage.populateDatalists = function () {
    let length = Object.keys(ExportCreatePage.Backend.m_AvailableDrugsInfo).length;

    let datalistName = $("#datalist-all-names");
    let datalistUnit = $("#datalist-all-units");

    for (const [name, drug] of Object.entries(ExportCreatePage.Backend.m_AvailableDrugsInfo)) {
        datalistName.append("<option value=\"" + name + "\">");
        datalistUnit.append("<option value=\"" + drug.unit + "\">");
    }
}

ExportCreatePage.updateDataLists = function () {
    let datalistName = $("#datalist-all-names");
    datalistName.html("");

    for (const [name, drug] of Object.entries(ExportCreatePage.Backend.m_AvailableDrugsInfo)) {
        let index = ExportCreatePage.Backend.m_AddedNames.indexOf(name);
        if (index < 0) {
            datalistName.append("<option value=\"" + name + "\">");
        }
    }
}

ExportCreatePage.untrackItemWithName = function (_name) {
    let index = ExportCreatePage.Backend.m_AddedNames.indexOf(_name);
    if (index < 0)
        return;

    ExportCreatePage.Backend.m_AddedNames.splice(index, 1);
}

ExportCreatePage.openDuplicateAdditionModal = function (_name) {
    let index = -1;
    let item = null;
    for (let i = 0; i < ExportCreatePage.Backend.m_AddIndex; ++i) {
        item = ExportCreatePage.Backend.m_AddedList[i];
        if (item == null)
            continue;

        if (item.name == _name) {
            index = i;
            break;
        }
    }

    if (index == -1)
        return;

    let body = "<div>Vật tư <b>" + _name + "</b> đã được thêm vào danh sách</div>"
        + "<div>Nếu bạn muốn thay đổi số lượng, vui lòng xóa vật tư khỏi danh sách và thêm lại.</div>";

    //body += "<div class=\"ml-2 mt-2\">Tên thuốc: <b>" + _name + "</b></div>";
    //body += "<div class=\"form-group col-4 ml-2\">"
    //    + "<label for=\"input-amount-m\" class=\"control-label\">Số lượng</label>"
    //    + "<div class=\"input-group\">"
    //    + "<input id=\"input-amount-m\" class=\"form-control\" type=\"number\" min=\"0\" value=\"" + item.amount + "\" />"
    //    + "<input class=\"form-control\" style=\"width:7ch\" value=\"" + item.unit + "\"/>"
    //    + "</div></div>";

    //let footer = "<button type=\"button\" class=\"btn btn-primary\" data-dismiss=\"modal\" onclick=\"btnUpdate_modal()\">Cập nhật</button>"
    //    + "<button type=\"button\" class=\"btn btn-secondary\" data-dismiss=\"modal\">Hủy bỏ</button>";

    Modals.CommonInfoModal.openWarningModal("Cảnh báo", body, null);
}
