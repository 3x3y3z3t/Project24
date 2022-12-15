/*  site.js
 *  Version: 1.3 (2022.12.13)
 *
 *  Contributor
 *      Arime-chan
 */
// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function formatDataLength(_length) {
    const oneKiB = 1024;
    const oneMiB = 1024 * oneKiB;
    const oneGiB = 1024 * oneMiB;
    const oneTiB = 1024 * oneGiB;

    if (_length > oneTiB) {
        return (_length / oneTiB).toFixed(2) + " TB";
    }

    if (_length > oneGiB) {
        return (_length / oneGiB).toFixed(2) + " GB";
    }

    if (_length > oneMiB) {
        return (_length / oneMiB).toFixed(2) + " MB";
    }

    if (_length > oneKiB) {
        return (_length / oneKiB).toFixed(2) + " KB";
    }

    return (_length).toFixed(0) + " B";
}

function formatTimeSpan_Hour(_millis) {
    _millis /= 1000.0;

    let seconds = Math.round(_millis % 60);
    _millis = Math.floor(_millis / 60);

    let minutes = Math.round(_millis % 60);
    _millis = Math.floor(_millis / 60);

    let hours = Math.round(_millis % 24);
    _millis = Math.floor(_millis / 24);

    return hours.toLocaleString("en-US", { minimumIntegerDigits: 2 }) + ":"
        + minutes.toLocaleString("en-US", { minimumIntegerDigits: 2 }) + ":"
        + seconds.toLocaleString("en-US", { minimumIntegerDigits: 2 });
}

function formatDateString(_date) {
    let year = _date.getFullYear();
    let month = String(_date.getMonth() + 1).padStart(2, "0");
    let date = String(_date.getDate()).padStart(2, "0");
    let hour = String(_date.getHours()).padStart(2, "0");
    let minute = String(_date.getMinutes()).padStart(2, "0");

    return year + "/" + month + "/" + date + " " + hour + ":" + minute;
}

function getFileExtension(_fileName) {
    let pos = _fileName.lastIndexOf('.');
    if (pos <= 0)
        return "";
    else
        return _fileName.substring(pos + 1);
}

function underlineMe(_x) {
    _x.style.textDecoration = "underline";
}

function normalizeMe(_x) {
    _x.style.textDecoration = "initial";
}
