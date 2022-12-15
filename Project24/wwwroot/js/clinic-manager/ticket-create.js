/*  ticket-create.js
 *  Version: 1.2 (2022.12.12)
 *
 *  Contributor
 *      Arime-chan
 */

TicketCreateForm = {};
TicketCreateForm.m_Form = null;

TicketCreateForm.m_CustomerCodeStatus = null;

//TicketCreateForm.m_TicketCodeInput = null;
TicketCreateForm.m_TicketDiagnoseInput = null;
TicketCreateForm.m_TicketTreatmentInput = null;
TicketCreateForm.m_TicketNotesInput = null;
TicketCreateForm.m_TicketFilesInput = null;

TicketCreateForm.m_CustomerCodeInput = null;
TicketCreateForm.m_CustomerNameInput = null;
TicketCreateForm.m_CustomerGenderMInput = null;
TicketCreateForm.m_CustomerGenderFInput = null;
TicketCreateForm.m_CustomerDobInput = null;
TicketCreateForm.m_CustomerPhoneInput = null;
TicketCreateForm.m_CustomerAddrInput = null;
TicketCreateForm.m_CustomerNotesInput = null;

$(document).ready(function () {
    TicketCreateForm.m_Form = $("#create-ticket-form");

    TicketCreateForm.m_CustomerCodeStatus = $("#customer-code-status");

    //TicketCreateForm.m_TicketCodeInput = $("#TicketFormData_Code");
    TicketCreateForm.m_TicketDiagnoseInput = $("#TicketFormData_Diagnose");
    TicketCreateForm.m_TicketTreatmentInput = $("#TicketFormData_Treatment");
    TicketCreateForm.m_TicketNotesInput = $("#TicketFormData_Notes");
    TicketCreateForm.m_TicketFilesInput = $("#TicketFormData_UploadedFiles");

    TicketCreateForm.m_CustomerCodeInput = $("#TicketFormData_CustomerFormData_Code");
    TicketCreateForm.m_CustomerNameInput = $("#TicketFormData_CustomerFormData_FullName");
    TicketCreateForm.m_CustomerGenderMInput = $("#radio-gender-m");
    TicketCreateForm.m_CustomerGenderFInput = $("#radio-gender-f");
    TicketCreateForm.m_CustomerDobInput = $("#TicketFormData_CustomerFormData_DateOfBirth");
    TicketCreateForm.m_CustomerPhoneInput = $("#TicketFormData_CustomerFormData_PhoneNumber");
    TicketCreateForm.m_CustomerAddrInput = $("#TicketFormData_CustomerFormData_Address");
    TicketCreateForm.m_CustomerNotesInput = $("#TicketFormData_CustomerFormData_Notes");

    let customerCode = TicketCreateForm.m_CustomerCodeInput.val();
    if (customerCode != null && customerCode != "")
        ticket_create_verifyCustomerCode();
});

function ticket_create_verifyCustomerCode() {
    let code = TicketCreateForm.m_CustomerCodeInput.val();
    let phone = TicketCreateForm.m_CustomerPhoneInput.val()

    $.ajax({
        type: 'GET',
        url: "/ClinicManager/Customer/Details?handler=Fetch&_code=" + code + "&_phone=" + phone,
        success: function (_content) {
            if (_content == null) {
                console.error("Error fetching customer info (" + code + ").");
                window.alert("Không tìm thấy bệnh nhân " + code + ".");
                return;
            }

            if (typeof _content === "string") {
                ticket_create_applyNewCustomerInfo(_content);
                console.info("New customer (" + _content + ").");
                return;
            }

            ticket_create_applyExistedCustomerInfo(_content);
            console.info("Customer " + code + " validated.");

        },
        error: function () {
            console.error("Error fetching customer info (" + code + ").");
        }
    });
}

function ticket_create_applyNewCustomerInfo(_customerCode) {
    ticket_create_removeDisabledAttribs();

    TicketCreateForm.m_Form.attr("action", "/ClinicManager/Ticket/Create?handler=NewCustomer");

    TicketCreateForm.m_CustomerCodeStatus.html(" (bệnh nhân mới)");

    TicketCreateForm.m_CustomerCodeInput.val(_customerCode);
    TicketCreateForm.m_CustomerCodeInput.attr("readonly", true);
}

function ticket_create_applyExistedCustomerInfo(_json) {
    ticket_create_removeDisabledAttribs();

    TicketCreateForm.m_Form.attr("action", "/ClinicManager/Ticket/Create?handler=OldCustomer");

    TicketCreateForm.m_CustomerCodeStatus.html(" (bệnh nhân cũ)");

    TicketCreateForm.m_CustomerCodeInput.val(_json.code);
    TicketCreateForm.m_CustomerCodeInput.attr("readonly", true);

    TicketCreateForm.m_CustomerNameInput.val(_json.fullName);

    if (_json.gender == "M")
        TicketCreateForm.m_CustomerGenderMInput.attr("checked", true);
    else if (_json.gender == "F")
        TicketCreateForm.m_CustomerGenderFInput.attr("checked", true);

    TicketCreateForm.m_CustomerDobInput.val(_json.dateOfBirth);

    TicketCreateForm.m_CustomerPhoneInput.val(_json.phoneNumber);

    TicketCreateForm.m_CustomerAddrInput.val(_json.address);

    TicketCreateForm.m_CustomerNotesInput.val(_json.notes);

    $("#btn-submit").removeAttr("disabled");
}

function ticket_create_resetCustomerInfo() {
    TicketCreateForm.m_CustomerCodeStatus.html("");

    TicketCreateForm.m_TicketDiagnoseInput.attr("disabled", true);
    TicketCreateForm.m_TicketTreatmentInput.attr("disabled", true);
    TicketCreateForm.m_TicketNotesInput.attr("disabled", true);

    TicketCreateForm.m_TicketFilesInput.val(null);
    TicketCreateForm.m_TicketFilesInput.attr("disabled", true);

    TicketCreateForm.m_CustomerCodeInput.val(null);
    TicketCreateForm.m_CustomerCodeInput.removeAttr("readonly");

    TicketCreateForm.m_CustomerNameInput.val(null);
    TicketCreateForm.m_CustomerNameInput.attr("disabled", true);

    TicketCreateForm.m_CustomerGenderMInput.removeAttr("checked");
    TicketCreateForm.m_CustomerGenderMInput.attr("disabled", true);

    TicketCreateForm.m_CustomerGenderFInput.removeAttr("checked");
    TicketCreateForm.m_CustomerGenderFInput.attr("disabled", true);

    TicketCreateForm.m_CustomerDobInput.val(null);
    TicketCreateForm.m_CustomerDobInput.attr("disabled", true);

    TicketCreateForm.m_CustomerPhoneInput.val(null);
    //TicketCreateForm.m_CustomerPhoneInput.attr("disabled", true);

    TicketCreateForm.m_CustomerAddrInput.val(null);
    TicketCreateForm.m_CustomerAddrInput.attr("disabled", true);

    TicketCreateForm.m_CustomerNotesInput.val(null);
    TicketCreateForm.m_CustomerNotesInput.attr("disabled", true);

    $("#btn-submit").attr("disabled", true);
}

function ticket_create_removeDisabledAttribs() {
    TicketCreateForm.m_TicketDiagnoseInput.removeAttr("disabled");
    TicketCreateForm.m_TicketTreatmentInput.removeAttr("disabled");
    TicketCreateForm.m_TicketNotesInput.removeAttr("disabled");
    TicketCreateForm.m_TicketFilesInput.removeAttr("disabled");

    TicketCreateForm.m_CustomerNameInput.removeAttr("disabled");
    TicketCreateForm.m_CustomerGenderMInput.removeAttr("disabled");
    TicketCreateForm.m_CustomerGenderFInput.removeAttr("disabled");
    TicketCreateForm.m_CustomerDobInput.removeAttr("disabled");
    TicketCreateForm.m_CustomerPhoneInput.removeAttr("disabled");
    TicketCreateForm.m_CustomerAddrInput.removeAttr("disabled");
    TicketCreateForm.m_CustomerNotesInput.removeAttr("disabled");

    $("#btn-submit").removeAttr("disabled");
}
