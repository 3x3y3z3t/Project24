/*  site.js
 *  Version: 1.4 (2022.12.29)
 *
 *  Contributor
 *      Arime-chan
 */
// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var HttpStatusCodeName = {
    '200': 'OK',
    '201': 'Created',
    '202': 'Accepted',
    '203': 'Non-Authoritative Information',
    '204': 'No Content',
    '205': 'Reset Content',
    '206': 'Partial Content',
    '300': 'Multiple Choices',
    '301': 'Moved Permanently',
    '302': 'Found',
    '303': 'See Other',
    '304': 'Not Modified',
    '305': 'Use Proxy',
    '306': 'Unused',
    '307': 'Temporary Redirect',
    '400': 'Bad Request',
    '401': 'Unauthorized',
    '402': 'Payment Required',
    '403': 'Forbidden',
    '404': 'Not Found',
    '405': 'Method Not Allowed',
    '406': 'Not Acceptable',
    '407': 'Proxy Authentication Required',
    '408': 'Request Timeout',
    '409': 'Conflict',
    '410': 'Gone',
    '411': 'Length Required',
    '412': 'Precondition Required',
    '413': 'Request Entry Too Large',
    '414': 'Request-URI Too Long',
    '415': 'Unsupported Media Type',
    '416': 'Requested Range Not Satisfiable',
    '417': 'Expectation Failed',
    '418': 'I\'m a teapot',
    '429': 'Too Many Requests',
    '500': 'Internal Server Error',
    '501': 'Not Implemented',
    '502': 'Bad Gateway',
    '503': 'Service Unavailable',
    '504': 'Gateway Timeout',
    '505': 'HTTP Version Not Supported',
};

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
