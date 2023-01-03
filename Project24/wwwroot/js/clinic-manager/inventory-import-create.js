/*  inventory-import-create.js
 *  Version: 1.1 (2023.01.03)
 *
 *  Contributor
 *      Arime-chan
 */

window.ImportCreatePage = {};

ImportCreatePage.Backend = {};
ImportCreatePage.Backend.m_AddedList = {};
ImportCreatePage.Backend.m_AddIndex = 0;

ImportCreatePage.Backend.m_AvailableDrugsInfo = {};
ImportCreatePage.Backend.m_AddedNames = [];

ImportCreatePage.Backend.m_Timer = null;

ImportCreatePage.Elements = {};
ImportCreatePage.Elements.m_InputName = null;
ImportCreatePage.Elements.m_InputAmount = null;
ImportCreatePage.Elements.m_InputUnit = null;
ImportCreatePage.Elements.m_UlValidationMsg = null;
ImportCreatePage.Elements.m_TbodyAddedList = null;

ImportCreatePage.Elements.m_BtnAdd = null;
ImportCreatePage.Elements.m_BtnFinish = null;


$(document).ready(function () {

    ImportCreatePage.Elements.m_InputName = $("#input-name");
    ImportCreatePage.Elements.m_InputAmount = $("#input-amount");
    ImportCreatePage.Elements.m_InputUnit = $("#input-unit");
    ImportCreatePage.Elements.m_UlValidationMsg = $("#ul-validation-msg");
    ImportCreatePage.Elements.m_TbodyAddedList = $("#tbody-added-list");

    ImportCreatePage.Elements.m_BtnAdd = $("#btn-add");
    ImportCreatePage.Elements.m_BtnFinish = $("#btn-finish");


    ImportCreatePage.ajax_fetchAvailDrugsInfo();

});

// ==================================================
// ajax request sender

ImportCreatePage.ajax_fetchAvailDrugsInfo = function () {
    $.ajax({
        type: "GET",
        url: "/ClinicManager/Inventory/List?handler=FetchAvailDrugsInfo",
        success: ImportCreatePage.fetchAvailDrugsInfo_success,
        error: ImportCreatePage.submit_error
    });
}

ImportCreatePage.ajax_Submit = function (_data) {
    let token = $("input[name='__RequestVerificationToken']").val();

    let formData = JSON.stringify(_data);

    $.ajax({
        type: "POST",
        url: "/ClinicManager/Inventory/Import/Create",
        headers: { "RequestVerificationToken": token },
        data: formData,
        contentType: "application/json; charset=utf-8",
        success: ImportCreatePage.submit_success,
        error: ImportCreatePage.submit_error
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// event

function inputName_onInput() {
    clearTimeout(ImportCreatePage.Backend.m_Timer);
    ImportCreatePage.Backend.m_Timer = setTimeout(ImportCreatePage.displayDrugInfo, 500);
}

function btnAdd() {
    ImportCreatePage.Elements.m_UlValidationMsg.html("");
    if (!ImportCreatePage.validateName() || !ImportCreatePage.validateAmount() || !ImportCreatePage.validateUnit())
        return;

    let name = ImportCreatePage.Elements.m_InputName.val();
    let itemIndex = ImportCreatePage.Backend.m_AddedNames.indexOf(name);
    if (itemIndex > -1) {
        ImportCreatePage.openDuplicateAdditionModal(name);
        return;
    }

    let index = ImportCreatePage.Backend.m_AddIndex;
    let amount = ImportCreatePage.Elements.m_InputAmount.val();
    let unit = ImportCreatePage.Elements.m_InputUnit.val();

    ImportCreatePage.Backend.m_AddedList[index] = { name, amount, unit };
    ++ImportCreatePage.Backend.m_AddIndex;

    if (ImportCreatePage.Backend.m_AvailableDrugsInfo[name] != null)
        ImportCreatePage.Backend.m_AddedNames.push(name);

    let html = "<tr id=\"row-" + index + "\">"; // open tag;
    html += "<td>" + (index + 1) + "</td>" // index column;
    html += "<td>" + name + "</td>" // name column;
    html += "<td>" + amount + "</td>" // amount column;
    html += "<td>" + unit + "</td>" // unit column;
    html += "<td><button class=\"btn btn-sm btn-outline-danger py-0 px-1\" onclick=\"btnRowRemove('" + index + "')\">&times;</button></td>" // close button column;
    html += "</tr>" // close tag;

    ImportCreatePage.Elements.m_TbodyAddedList.append(html);

    ImportCreatePage.Elements.m_InputName.val("");
    ImportCreatePage.Elements.m_InputAmount.val("");
    ImportCreatePage.Elements.m_InputUnit.val("");
    ImportCreatePage.Elements.m_InputUnit.removeAttr("readonly");

    ImportCreatePage.updateFinishButton();
    ImportCreatePage.updateDataLists();
}

function btnRowRemove(_index) {
    if (ImportCreatePage.Backend.m_AddedList[_index] == null)
        return;

    ImportCreatePage.untrackItemWithName(ImportCreatePage.Backend.m_AddedList[_index].name);

    delete ImportCreatePage.Backend.m_AddedList[_index];
    $("#row-" + _index).remove();

    ImportCreatePage.updateFinishButton();
    ImportCreatePage.updateDataLists();
}

function btnFinish() {
    if (ImportCreatePage.getAddedCount() <= 0)
        return;

    let dataList = [];

    for (let i = 0; i < ImportCreatePage.Backend.m_AddIndex; ++i) {
        let item = ImportCreatePage.Backend.m_AddedList[i];
        if (item == null)
            continue;

        dataList.push({ "Name": item.name, "Amount": item.amount, "Unit": item.unit });
    }

    ImportCreatePage.ajax_Submit(dataList);
}

function btnUpdate_modal() {

}

ImportCreatePage.fetchAvailDrugsInfo_success = function (_content, _textStatus, _xhr) {
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
            ImportCreatePage.Backend.m_AvailableDrugsInfo[drug.Name] = drug.Unit;
        }

        ImportCreatePage.populateDatalists();

        ImportCreatePage.Elements.m_BtnAdd.removeAttr("disabled");
        return;
    }
}

ImportCreatePage.submit_success = function (_content, _textStatus, _xhr) {
    if (_content.startsWith("<fail>")) {
        let body = "<div>" + _content.substring(6) + "</div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
        return;
    }

    if (_content.startsWith("<done>")) {
        let body = "<div>";
        body += "Nhập kho thành công.";
        body += "</div>";

        Modals.CommonInfoModal.openSuccessModal("Thành công", body, null);

        ImportCreatePage.Backend.m_AddedList = {};
        ImportCreatePage.Backend.m_AddIndex = 0;
        ImportCreatePage.Backend.m_AddedNames = [];

        ImportCreatePage.Elements.m_TbodyAddedList.html("");
        ImportCreatePage.updateFinishButton();

        return;
    }
}

ImportCreatePage.submit_error = function (_xhr, _textStatus, _errorThrown) {
    let body = "<div>Status Code: <code>" + _xhr.status + " - " + HttpStatusCodeName[_xhr.status] + "</code></div><div>jq Status: <code>" + _textStatus + "</code></div>";
        Modals.CommonInfoModal.openErrorModal("Có lỗi xảy ra", body, null);
}

// END: event
// ==================================================

// ==================================================
// helper

ImportCreatePage.validateName = function () {
    let html = ImportCreatePage.Elements.m_UlValidationMsg.html();

    if (ImportCreatePage.Elements.m_InputName.val() == "") {
        html += "<li>Tên thuốc không được để trống</li>";
        ImportCreatePage.Elements.m_UlValidationMsg.html(html);
        return false;
    }

    return true;
}

ImportCreatePage.validateAmount = function () {
    let html = ImportCreatePage.Elements.m_UlValidationMsg.html();

    if (ImportCreatePage.Elements.m_InputAmount.val() == "") {
        html += "<li>Số lượng không được để trống</li>";
        ImportCreatePage.Elements.m_UlValidationMsg.html(html);
        return false;
    }

    if (ImportCreatePage.Elements.m_InputAmount.val() <= 0) {
        html += "<li>Số lượng phải lớn hơn 0</li>";
        ImportCreatePage.Elements.m_UlValidationMsg.html(html);
    }

    return true;
}

ImportCreatePage.validateUnit = function () {
    let html = ImportCreatePage.Elements.m_UlValidationMsg.html();

    if (ImportCreatePage.Elements.m_InputUnit.val() == "") {
        html += "<li>Đơn vị không được để trống</li>";
        ImportCreatePage.Elements.m_UlValidationMsg.html(html);
        return false;
    }

    return true;
}

ImportCreatePage.displayDrugInfo = function () {
    let name = ImportCreatePage.Elements.m_InputName.val();

    let unit = ImportCreatePage.Backend.m_AvailableDrugsInfo[name];
    if (unit != null) {
        ImportCreatePage.Elements.m_InputUnit.val(unit);
        ImportCreatePage.Elements.m_InputUnit.attr("readonly", true);
    } else {
        ImportCreatePage.Elements.m_InputUnit.val("");
        ImportCreatePage.Elements.m_InputUnit.removeAttr("readonly");
    }
}

ImportCreatePage.updateFinishButton = function () {
    if (ImportCreatePage.getAddedCount() <= 0)
        ImportCreatePage.Elements.m_BtnFinish.attr("disabled", true);
    else
        ImportCreatePage.Elements.m_BtnFinish.removeAttr("disabled");
}

ImportCreatePage.getAddedCount = function () {
    return Object.keys(ImportCreatePage.Backend.m_AddedList).length;
}

// END: helper
// ==================================================

ImportCreatePage.populateDatalists = function () {
    let length = Object.keys(ImportCreatePage.Backend.m_AvailableDrugsInfo).length;

    let datalistName = $("#datalist-all-names");
    let datalistUnit = $("#datalist-all-units");

    for (const [name, unit] of Object.entries(ImportCreatePage.Backend.m_AvailableDrugsInfo)) {
        datalistName.append("<option value=\"" + name + "\">");
        datalistUnit.append("<option value=\"" + unit + "\">");
    }
}

ImportCreatePage.updateDataLists = function () {
    let datalistName = $("#datalist-all-names");
    datalistName.html("");

    for (const [name, unit] of Object.entries(ImportCreatePage.Backend.m_AvailableDrugsInfo)) {
        let index = ImportCreatePage.Backend.m_AddedNames.indexOf(name);
        if (index < 0) {
            datalistName.append("<option value=\"" + name + "\">");
        }
    }
}

ImportCreatePage.untrackItemWithName = function (_name) {
    let index = ImportCreatePage.Backend.m_AddedNames.indexOf(_name);
    if (index < 0)
        return;

    ImportCreatePage.Backend.m_AddedNames.splice(index, 1);
}

ImportCreatePage.openDuplicateAdditionModal = function (_name) {
    let index = -1;
    let item = null;
    for (let i = 0; i < ImportCreatePage.Backend.m_AddIndex; ++i) {
        item = ImportCreatePage.Backend.m_AddedList[i];
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
