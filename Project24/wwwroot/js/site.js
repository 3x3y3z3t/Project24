/*  site.js
 *  Version: 1.0 (2022.09.06)
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
