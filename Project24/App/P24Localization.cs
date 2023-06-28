/*  App/Utils/P24Localization.cs
 *  Version: v1.1 (2023.06.28)
 *  
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;

namespace Project24.App
{
    public class P24Localization
    {
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

            dict[LOCL.STR_UPDATER_FILES_ON_SVR] = "File on server";
        }

        private void LoadJap()
        {
            var dict = m_Jap;

            dict[LOCL.PAGE_UPDATER] = "アップデート";

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
        public const string PAGE_UPDATER = nameof(PAGE_UPDATER);

        public const string STR_UPDATER_MAIN_VER = nameof(STR_UPDATER_MAIN_VER);
        public const string STR_UPDATER_PREV_VER = nameof(STR_UPDATER_PREV_VER);
        public const string STR_UPDATER_NEXT_VER = nameof(STR_UPDATER_NEXT_VER);
        public const string STR_UPDATER_FILES_ON_SVR = nameof(STR_UPDATER_FILES_ON_SVR);
        public const string STR_UPDATER_FILES_TO_UPL = nameof(STR_UPDATER_FILES_TO_UPL);

        public const string DESC_UPDATE_NOTE_TAG_NOT_FOUND = nameof(DESC_UPDATE_NOTE_TAG_NOT_FOUND);

        //public const string DESC_UPDATER_NO_PREV_VER = nameof(DESC_UPDATER_NO_PREV_VER);
        //public const string DESC_UPDATER_NO_NEXT_VER = nameof(DESC_UPDATER_NO_NEXT_VER);
        public const string DESC_UPDATER_ERR_BATCH_OVERSIZE = nameof(DESC_UPDATER_ERR_BATCH_OVERSIZE);
        public const string DESC_UPDATER_ERR_BATCH_COUNT_MISMATCH = nameof(DESC_UPDATER_ERR_BATCH_COUNT_MISMATCH);
        public const string DESC_UPDATER_ERR_BATCH_SIZE_MISMATCH = nameof(DESC_UPDATER_ERR_BATCH_SIZE_MISMATCH);
    }

}








namespace Project24.App.Localization
{
    public static class Message
    {
        public static string UpdateNotes_Failed_Tagged = P24Localization.Active[LOCL.DESC_UPDATE_NOTE_TAG_NOT_FOUND];
    }
}
