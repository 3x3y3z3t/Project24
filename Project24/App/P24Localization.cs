/*  App/Utils/P24Localization.cs
 *  Version: v1.6 (2023.10.29)
 *  
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;

namespace Project24.App
{
    public class P24Localization
    {
        public const string EN_US = "en-US";
        public const string JA_JP = "ja-JP";
        public const string VI_VN = "vi-VN";

        public const string Page_Changelog = "Changelog";

        public const string String_Changelog = "Changelog";
        public const string String_UpdateNotes = "Update Notes";

        public const string Description_Changelog = "\"Changelog\" ghi lại tất cả thay đổi trong project, chi tiết đến từng commit.";
        public const string Description_UpdateNotes = "\"Update Notes\" ghi lại thông tin các bản cập nhật (release).";


        public static Dictionary<string, string> Active { get { Instance ??= new P24Localization(); return Instance.m_Active; } }

        public static P24Localization Instance { get; private set; }

        private P24Localization()
        {
            LoadVie();
            LoadEng();
            LoadJap();

            m_Active = m_Vie;
        }


        private void LoadVie()
        {
            var dict = m_Vie;

            dict[LOCL.PAGE_UPDATER] = "Cập nhật";
            dict[LOCL.PAGE_CONFIG_PANEL] = "Cài đặt";

            dict[LOCL.STR_UPDATER_MAIN_VER] = "Phiên bản hiện tại";
            dict[LOCL.STR_UPDATER_PREV_VER] = "Phiên bản trước";
            dict[LOCL.STR_UPDATER_NEXT_VER] = "Phiên bản mới";
            dict[LOCL.STR_UPDATER_FILES_ON_SVR] = "File trên server";
            dict[LOCL.STR_UPDATER_FILES_TO_UPL] = "File chuẩn bị tải lên";

            dict[LOCL.DESC_UPDATE_NOTE_TAG_NOT_FOUND] = "Không tìm thấy thông tin bản cập nhật <code>{0}</code>.";

            //dict[LOCL.DESC_UPDATER_NO_PREV_VER] = "Không tìm thấy phiên bản trước";
            //dict[LOCL.DESC_UPDATER_NO_NEXT_VER] = "Không tìm thấy phiên bản mới";
            dict[LOCL.DESC_UPDATER_ERR_BATCH_OVERSIZE] = "Kích thước batch vượt quá {0}. Vui lòng báo lỗi với mã lỗi " + ErrCode.Updater_BatchOversize + ".";
            dict[LOCL.DESC_UPDATER_ERR_BATCH_COUNT_MISMATCH] = "Số lượng file trong batch không khớp ({0}/{1}). Vui lòng báo lỗi với mã lỗi " + ErrCode.Updater_BatchCountMismatch + ".";
            dict[LOCL.DESC_UPDATER_ERR_BATCH_SIZE_MISMATCH] = "Kích thước batch không khớp ({0}/{1}). Vui lòng báo lỗi với mã lỗi " + ErrCode.Updater_BatchSizeMismatch + ".";







        }

        private void LoadEng()
        {
            var dict = m_Eng;

            dict[LOCL.PAGE_UPDATER] = "Update";
            dict[LOCL.PAGE_CONFIG_PANEL] = "Config"; 

            dict[LOCL.STR_UPDATER_FILES_ON_SVR] = "File on server";
        }

        private void LoadJap()
        {
            var dict = m_Jap;

            dict[LOCL.PAGE_UPDATER] = "アップデート";
            dict[LOCL.PAGE_CONFIG_PANEL] = "設定";

            dict[LOCL.STR_UPDATER_MAIN_VER] = "現在のバージョン";
            dict[LOCL.STR_UPDATER_PREV_VER] = "前のバージョン";
            dict[LOCL.STR_UPDATER_NEXT_VER] = "新バージョン";
            dict[LOCL.STR_UPDATER_FILES_ON_SVR] = "サーバーのファイル";
            dict[LOCL.STR_UPDATER_FILES_TO_UPL] = "アップロードするファイル";

        }


        private readonly Dictionary<string, string> m_Active = null;


        private readonly Dictionary<string, string> m_Vie = new();
        private readonly Dictionary<string, string> m_Eng = new();
        private readonly Dictionary<string, string> m_Jap = new();
    }

    public static class LOCL
    {
        #region Button Labels
        public const string BTN_RELOAD = nameof(BTN_RELOAD);
        public const string BTN_SAVE = nameof(BTN_SAVE);

        public const string BTN_LOGIN = nameof(BTN_LOGIN);
        public const string BTN_LOGOUT = nameof(BTN_LOGOUT);
        #endregion

        #region Labels
        public const string LBL_HELLO = nameof(LBL_HELLO);

        public const string LBL_ID = nameof(LBL_ID);
        public const string LBL_ACCESS_COUNT = nameof(LBL_ACCESS_COUNT);
        public const string LBL_ADDED_DATE = nameof(LBL_ADDED_DATE);
        public const string LBL_REMOVED_DATE = nameof(LBL_REMOVED_DATE);

        public const string LBL_USERNAME = nameof(LBL_USERNAME);
        public const string LBL_PASSWORD = nameof(LBL_PASSWORD);
        public const string LBL_LOGIN_REMEMBER = nameof(LBL_LOGIN_REMEMBER);

        public const string LBL_BACK_TO_HOME = nameof(LBL_BACK_TO_HOME);

        public const string LBL_INDEX_CLINIC_MANAGER = nameof(LBL_INDEX_CLINIC_MANAGER);
        #endregion

        #region Page Titles
        //public const string PAGE_NOT_IMPLEMENTED = nameof(PAGE_NOT_IMPLEMENTED);

        public const string PAGE_IDENTITY_ERROR = nameof(PAGE_IDENTITY_ERROR);

        public const string PAGE_IDENTITY_ACC_ACCESS_DENIED = nameof(PAGE_IDENTITY_ACC_ACCESS_DENIED);
        public const string PAGE_IDENTITY_ACC_CONFIRM_EMAIL = nameof(PAGE_IDENTITY_ACC_CONFIRM_EMAIL);
        public const string PAGE_IDENTITY_ACC_CONFIRM_EMAIL_CHANGE = nameof(PAGE_IDENTITY_ACC_CONFIRM_EMAIL_CHANGE);
        public const string PAGE_IDENTITY_ACC_EXTERNAL_LOGIN = nameof(PAGE_IDENTITY_ACC_EXTERNAL_LOGIN);
        public const string PAGE_IDENTITY_ACC_FORGOT_PASS = nameof(PAGE_IDENTITY_ACC_FORGOT_PASS);
        public const string PAGE_IDENTITY_ACC_FORGOT_PASS_CONFIRM = nameof(PAGE_IDENTITY_ACC_FORGOT_PASS_CONFIRM);
        public const string PAGE_IDENTITY_ACC_LOCKOUT = nameof(PAGE_IDENTITY_ACC_LOCKOUT);
        public const string PAGE_IDENTITY_ACC_LOGIN = nameof(PAGE_IDENTITY_ACC_LOGIN);
        public const string PAGE_IDENTITY_ACC_LOGIN_2FA = nameof(PAGE_IDENTITY_ACC_LOGIN_2FA);
        public const string PAGE_IDENTITY_ACC_LOGIN_RECOVERY_CODE = nameof(PAGE_IDENTITY_ACC_LOGIN_RECOVERY_CODE);
        public const string PAGE_IDENTITY_ACC_LOGOUT = nameof(PAGE_IDENTITY_ACC_LOGOUT);
        public const string PAGE_IDENTITY_ACC_REGISTER = nameof(PAGE_IDENTITY_ACC_REGISTER);
        public const string PAGE_IDENTITY_ACC_REGISTER_CONFIRM = nameof(PAGE_IDENTITY_ACC_REGISTER_CONFIRM);
        public const string PAGE_IDENTITY_ACC_RESEND_EMAIL_CONFIRM = nameof(PAGE_IDENTITY_ACC_RESEND_EMAIL_CONFIRM);
        public const string PAGE_IDENTITY_ACC_RESET_PASS = nameof(PAGE_IDENTITY_ACC_RESET_PASS);
        public const string PAGE_IDENTITY_ACC_RESET_PASS_CONFIRM = nameof(PAGE_IDENTITY_ACC_RESET_PASS_CONFIRM);

        public const string PAGE_IDENTITY_ACC_MANAGE_CHANGE_PASS = nameof(PAGE_IDENTITY_ACC_MANAGE_CHANGE_PASS);
        public const string PAGE_IDENTITY_ACC_MANAGE_DELETE_PERSONAL_DATA = nameof(PAGE_IDENTITY_ACC_MANAGE_DELETE_PERSONAL_DATA);
        public const string PAGE_IDENTITY_ACC_MANAGE_DISABLE_2FA = nameof(PAGE_IDENTITY_ACC_MANAGE_DISABLE_2FA);
        public const string PAGE_IDENTITY_ACC_MANAGE_DOWNLOAD_PERSONAL_DATA = nameof(PAGE_IDENTITY_ACC_MANAGE_DOWNLOAD_PERSONAL_DATA);
        public const string PAGE_IDENTITY_ACC_MANAGE_EMAIL = nameof(PAGE_IDENTITY_ACC_MANAGE_EMAIL);
        public const string PAGE_IDENTITY_ACC_MANAGE_ENABLE_AUTHENTICATOR = nameof(PAGE_IDENTITY_ACC_MANAGE_ENABLE_AUTHENTICATOR);
        public const string PAGE_IDENTITY_ACC_MANAGE_EXTERNAL_LOGIN = nameof(PAGE_IDENTITY_ACC_MANAGE_EXTERNAL_LOGIN);
        public const string PAGE_IDENTITY_ACC_MANAGE_GENERATE_RECOVERY_CODE = nameof(PAGE_IDENTITY_ACC_MANAGE_GENERATE_RECOVERY_CODE);
        public const string PAGE_IDENTITY_ACC_MANAGE_INDEX = nameof(PAGE_IDENTITY_ACC_MANAGE_INDEX);
        public const string PAGE_IDENTITY_ACC_MANAGE_PERSONAL_DATA = nameof(PAGE_IDENTITY_ACC_MANAGE_PERSONAL_DATA);
        public const string PAGE_IDENTITY_ACC_MANAGE_RESET_AUTHENTICATOR = nameof(PAGE_IDENTITY_ACC_MANAGE_RESET_AUTHENTICATOR);
        public const string PAGE_IDENTITY_ACC_MANAGE_SET_PASS = nameof(PAGE_IDENTITY_ACC_MANAGE_SET_PASS);
        public const string PAGE_IDENTITY_ACC_MANAGE_SHOW_RECOVERY_CODE = nameof(PAGE_IDENTITY_ACC_MANAGE_SHOW_RECOVERY_CODE);
        public const string PAGE_IDENTITY_ACC_MANAGE_TWO_FACTOR_AUTH = nameof(PAGE_IDENTITY_ACC_MANAGE_TWO_FACTOR_AUTH);

        public const string PAGE_INDEX = nameof(PAGE_INDEX);
        public const string PAGE_ERROR = nameof(PAGE_ERROR);

        public const string PAGE_HOME_INDEX = nameof(PAGE_HOME_INDEX);

        public const string PAGE_HOME_ACCOUNT_LIST = nameof(PAGE_HOME_ACCOUNT_LIST);
        public const string PAGE_HOME_ACCOUNT_MANAGE = nameof(PAGE_HOME_ACCOUNT_MANAGE);



        public const string PAGE_UPDATER = nameof(PAGE_UPDATER);
        public const string PAGE_CONFIG_PANEL = nameof(PAGE_CONFIG_PANEL);
        #endregion

        #region Short Strings
        public const string STR_UPDATER_MAIN_VER = nameof(STR_UPDATER_MAIN_VER);
        public const string STR_UPDATER_PREV_VER = nameof(STR_UPDATER_PREV_VER);
        public const string STR_UPDATER_NEXT_VER = nameof(STR_UPDATER_NEXT_VER);
        public const string STR_UPDATER_FILES_ON_SVR = nameof(STR_UPDATER_FILES_ON_SVR);
        public const string STR_UPDATER_FILES_TO_UPL = nameof(STR_UPDATER_FILES_TO_UPL);
        #endregion

        #region Long Strings (Descriptions)
        public const string DESC_IDENTITY_ACCOUNT_ACCESS_DENIED = nameof(DESC_IDENTITY_ACCOUNT_ACCESS_DENIED);
        public const string DESC_IDENTITY_ACCOUNT_LOGIN_FAILED = nameof(DESC_IDENTITY_ACCOUNT_LOGIN_FAILED);
        public const string DESC_IDENTITY_ACCOUNT_LOGOUT_INCOMPLETE = nameof(DESC_IDENTITY_ACCOUNT_LOGOUT_INCOMPLETE);
        public const string DESC_IDENTITY_ACCOUNT_LOGOUT_COMPLETE = nameof(DESC_IDENTITY_ACCOUNT_LOGOUT_COMPLETE);

        public const string DESC_UPDATE_NOTE_TAG_NOT_FOUND = nameof(DESC_UPDATE_NOTE_TAG_NOT_FOUND);

        //public const string DESC_UPDATER_NO_PREV_VER = nameof(DESC_UPDATER_NO_PREV_VER);
        //public const string DESC_UPDATER_NO_NEXT_VER = nameof(DESC_UPDATER_NO_NEXT_VER);
        public const string DESC_UPDATER_ERR_BATCH_OVERSIZE = nameof(DESC_UPDATER_ERR_BATCH_OVERSIZE);
        public const string DESC_UPDATER_ERR_BATCH_COUNT_MISMATCH = nameof(DESC_UPDATER_ERR_BATCH_COUNT_MISMATCH);
        public const string DESC_UPDATER_ERR_BATCH_SIZE_MISMATCH = nameof(DESC_UPDATER_ERR_BATCH_SIZE_MISMATCH);

        public const string DESC_INDEX_WELCOME = nameof(DESC_INDEX_WELCOME);
        public const string DESC_INDEX_WHATS_NEW = nameof(DESC_INDEX_WHATS_NEW);
        public const string DESC_INDEX_WHATS_NEW_LONG = nameof(DESC_INDEX_WHATS_NEW_LONG);

        public const string DESC_CURRENT_PROGRESS = nameof(DESC_CURRENT_PROGRESS);
        public const string DESC_TRANSLATION_PROGRESS = nameof(DESC_TRANSLATION_PROGRESS);
        #endregion

        public const string SVRMSG_UPDATER_COUNTDOWN = nameof(SVRMSG_UPDATER_COUNTDOWN);
        public const string SVRMSG_UPDATER_IN_PROGRESS = nameof(SVRMSG_UPDATER_IN_PROGRESS);
    }

}








namespace Project24.App.Localization
{
    public static class Message
    {
        public static string UpdateNotes_Failed_Tagged = P24Localization.Active[LOCL.DESC_UPDATE_NOTE_TAG_NOT_FOUND];
    }
}
