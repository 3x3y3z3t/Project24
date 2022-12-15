/*  nas-index.js
 *  Version: 1.0 (2022.12.14)
 *
 *  Contributor
 *      Arime-chan
 */

function browseNas(_path) {
    //$("#modal-browsing-path").html(_path);
    //$("#modal-browsing").modal();

    if (_path == "")
        window.location.href = "/Nas";
    else
        window.location.href = "/Nas/" + _path;
}
