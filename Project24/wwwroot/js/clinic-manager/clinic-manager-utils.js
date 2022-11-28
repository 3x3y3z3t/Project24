/*  clinic-manager-utils.js
 *  Version: 1.0 (2022.11.28)
 *
 *  Contributor
 *      Arime-chan
 */

function customer_commonListImage_SubmitDeleteImageForm(_imageId, _customerCode) {
    var token = $("input[name='__RequestVerificationToken']").val();

    $.ajax({
        type: 'POST',
        url: '/ClinicManager/Customer/Delete?handler=DeleteImage',
        headers: { "RequestVerificationToken": token },
        data: JSON.stringify({
            ImageId: _imageId,
            CustomerCode: _customerCode
        }),
        contentType: "application/json; charset=utf-8",
        processData: false,
        success: function (_content) {
            $("#list-image").html(_content);
            console.log("Deleted image " + _imageId + " (" + _customerCode + ").");
            //location.reload();
        },
        error: function () {
            console.error("Error deleting image " + _imageId + " (" + _customerCode + ").");
            window.alert("Error deleting image " + _imageId + " (" + _customerCode + ").");
        }
    });
}
