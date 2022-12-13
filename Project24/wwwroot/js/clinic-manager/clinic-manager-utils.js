/*  clinic-manager-utils.js
 *  Version: 1.1 (2022.12.13)
 *
 *  Contributor
 *      Arime-chan
 */

function customer_commonListImage_DeleteCustomerImage(_imageId) {
    var token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: 'POST',
        url: '/ClinicManager/Customer/Delete?handler=DeleteImage',
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(_imageId),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: function (_content) {
            $("#list-image").html(_content);
            console.log("Deleted image " + _imageId + ".");
            //location.reload();
        },
        error: function () {
            console.error("Error deleting image " + _imageId + ".");
            window.alert("Error deleting image " + _imageId + ".");
        }
    });
}

function customer_commonListImage_DeleteTicketImage(_imageId) {
    var token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: 'POST',
        url: '/ClinicManager/Ticket/Delete?handler=DeleteImage',
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify(_imageId),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: function (_content) {
            $("#list-image").html(_content);
            console.log("Deleted image " + _imageId + ".");
            //location.reload();
        },
        error: function () {
            console.error("Error deleting image " + _imageId + ".");
            window.alert("Error deleting image " + _imageId + ".");
        }
    });
}
