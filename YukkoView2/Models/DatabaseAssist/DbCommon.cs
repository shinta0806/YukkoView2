// ============================================================================
// 
// データベース共通で使用する関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Microsoft.Data.Sqlite;

using Shinta;

using System;
using System.IO;

using YukkoView2.Models.Settings;
using YukkoView2.Models.SharedMisc;

namespace YukkoView2.Models.DatabaseAssist
{
	internal class DbCommon
	{
		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// データベース接続
		// --------------------------------------------------------------------
		public static SqliteConnection Connect(String path)
		{
			SqliteConnectionStringBuilder stringBuilder = new()
			{
				DataSource = path,
			};
			return new SqliteConnection(stringBuilder.ToString());
		}

		// --------------------------------------------------------------------
		// リストデータベース（ゆかり用：ディスク）のフルパス
		// --------------------------------------------------------------------
		public static String ListDatabasePath(Yv2Settings yv2Settings)
		{
			return YukariDatabaseFullFolder(yv2Settings) + FILE_NAME_LIST_DATABASE_IN_DISK;
		}

		// --------------------------------------------------------------------
		// ゆかり用データベースを保存するフォルダーのフルパス（末尾 '\\'）
		// --------------------------------------------------------------------
		public static String YukariDatabaseFullFolder(Yv2Settings yv2Settings)
		{
			return Path.GetDirectoryName(yv2Settings.YukariConfigPath()) + "\\" + Yv2Constants.FOLDER_NAME_LIST;
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// データベースファイル名
		private const String FILE_NAME_LIST_DATABASE_IN_DISK = "List" + Common.FILE_EXT_SQLITE3;
	}
}
