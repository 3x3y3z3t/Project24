/*  locale/p24-localization-vi-vn.js
    Version: v1.1 (2023.09.23)

    Author
        Arime-chan
 */

window.P24Localization = {
    get: function (_key) { if (this[_key] == null || this[_key].trim() == "") return "<code>{" + _key + "}</code>"; return this[_key]; },

    LOCL_LANG_VIE                                   : "", 
    LOCL_LANG_ENG                                   : "", 
    LOCL_LANG_JAP                                   : "", 

    ERRCODE_UNEXPECTED_CONTROL                      : "", 
    ERRCODE_UPDATER_INVALID_BLOCK_NAME              : "", 

    LOCL_BTN_CLOSE                                  : "", 
    LOCL_BTN_CHANGE_PASS                            : "Đổi mật khẩu", 

    LOCL_STR_CONFIRM                                : "Xác nhận", // comment
    LOCL_STR_YES                                    : "Có", 
    LOCL_STR_NO                                     : "Không", 
    LOCL_STR_ACCEPT                                 : "Đồng ý", 
    LOCL_STR_CANCEL                                 : "Hủy bỏ", 

    LOCL_STR_SUCCESS                                : "Thành công", 
    LOCL_STR_INFO                                   : "Thông báo", 
    LOCL_STR_WARN                                   : "Cảnh báo", 
    LOCL_STR_ERR                                    : "Lỗi", 
    LOCL_STR_FAIL                                   : "Thất bại", 
    LOCL_STR_CRIT                                   : "", 
    LOCL_STR_UNKNOWN_ERR                            : "Lỗi không xác định", 
    LOCL_STR_EXCEPTION                              : "Exception (lỗi ngoại lệ)", 

    LOCL_DESC_DEFAULT_PASSWORD_USAGE                : "<p><b>Bạn đang sử dụng mật khẩu mặc định.</b> Điều này rất nguy hiểm vì <b>mật khẩu mặc định được công khai</b> cùng với mã nguồn phần mềm.</p><p>Hãy click vào nút \"Đổi mật khẩu\" dưới đây để chuyển  đến trang đổi mật khẩu ngay.</p>", 

    LOCL_DESC_MALFORMED                             : "Dữ liệu không đúng định dạng.", 
    LOCL_DESC_UPDATER_NOMAIN                        : "Không có thông tin phiên bản hiện tại.", 
    LOCL_DESC_UPDATER_PURGE_CANNOT_ABORT            : "Tác vụ Xóa file đang chạy, không thể hủy tác vụ.", 
    LOCL_DESC_UPDATER_PREV_PURGE_QUEUED             : "Tác vụ Xóa file phiên bản trước đã được khởi chạy.", 
    LOCL_DESC_UPDATER_NEXT_PURGE_QUEUED             : "Tác vụ Xóa file phiên bản mới đã được khởi chạy.", 
    LOCL_DESC_UPDATER_PREV_APPLY_QUEUED             : "Tác vụ Hạ xuống phiên bản trước đã được khởi chạy.", 
    LOCL_DESC_UPDATER_NEXT_APPLY_QUEUED             : "Tác vụ Nâng cấp phiên bản mới đã được khởi chạy.", 
    LOCL_DESC_UPDATER_ABORT_SUCCESS                 : "Hủy tác vụ thành công.", 
    LOCL_DESC_UPDATER_CONFIRM_PURGE_PREV            : "<div>Xóa file phiên bản cũ cũng sẽ <b>hủy tác vụ Hạ xuống phiên bản cũ</b>.</div><div>Bạn có chắc muốn xóa file?</div>", 
    LOCL_DESC_UPDATER_CONFIRM_PURGE_NEXT            : "<div>Xóa file phiên bản mới cũng sẽ <b>hủy tác vụ Nâng cấp phiên bản mới</b>.</div><div>Bạn có chắc muốn xóa file?</div>", 
    LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_PURGE      : "Bạn có chắc muốn <b>hủy tác vụ Xóa file phiên bản cũ</b>?", 
    LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_PURGE      : "Bạn có chắc muốn <b>hủy tác vụ Xóa file phiên bản mới</b>?", 
    LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_APPLY      : "Bạn có chắc muốn <b>hủy tác vụ Hạ xuống phiên bản cũ</b>?", 
    LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_APPLY      : "Bạn có chắc muốn <b>hủy tác vụ Nâng cấp phiên bản mới</b>?", 
    LOCL_DESC_UPDATER_CONFIRM_APPLY_PREV            : "Bạn có chắc muốn <b>khởi chạy tác vụ Hạ xuống phiên bản cũ</b>?", 
    LOCL_DESC_UPDATER_CONFIRM_APPLY_NEXT            : "Bạn có chắc muốn <b>khởi chạy tác vụ Nâng cấp phiên bản mới</b>?", 

    LOCL_DESC_UPDATER_INPUTFILES_INITIAL            : "Click hoặc thả thư mục vào đây..", 
    LOCL_DESC_UPDATER_UPLOAD_SUCCESS                : "Upload thành công. Click OK để tải lại trang.", 
    LOCL_DESC_UPDATER_BATCH_ERROR                   : "", 

    LOCL_DESC_CFG_LOCALE_CHANGED                    : "<div>Ngôn ngữ hiển thị đã được thay đổi. Click OK để tải lại trang.</div><div>(Một số nội dung sẽ không thay đổi cho đến khi tải lại trang.)</div>", 

};
