// ============================================================================
// 
// ゆっこビュー 2 の設定を管理
// 
// ============================================================================

// ----------------------------------------------------------------------------
// シリアライズされるため public class である必要がある
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Windows;
using YukkoView2.Models.SharedMisc;

namespace YukkoView2.Models.Settings
{
	public class Yv2Settings : SerializableSettings
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// EnvironmentModel 構築時に呼びだされるため、LogWriter は指定できない
		// --------------------------------------------------------------------
		public Yv2Settings()
				: base(null, Yv2SettingsPath())
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// コメント受信
		// --------------------------------------------------------------------

		// コメントサーバー指定方法
		public CommentServerType CommentServerType { get; set; }

		// コメントサーバー指定方法：自動
		// ゆかり設定ファイルのパス（相対または絶対）
		public String YukariConfigPathSeed { get; set; } = @"C:\xampp\htdocs\" + Yv2Constants.FILE_NAME_YUKARI_CONFIG;

		// コメントサーバー指定方法：手動
		// コメントサーバーの URL
		public String ServerUrl { get; set; } = String.Empty;

		// コメントサーバー指定方法：手動
		// ルーム名
		public String RoomName { get; set; } = String.Empty;

		// コメント受信方法
		public CommentReceiveType CommentReceiveType { get; set; }

		// コメント受信方法：プッシュ通知
		// 待ち受けポート
		public Int32 PushPort { get; set; } = 13581;

		// コメント受信方法：ダウンロード
		// 受信間隔 [ms]
		public Int32 DownloadInterval { get; set; } = 1000;

		// --------------------------------------------------------------------
		// 終了時の状態（一般）
		// --------------------------------------------------------------------

		// 前回起動時のバージョン
		public String PrevLaunchVer { get; set; } = String.Empty;

		// 前回起動時のパス
		public String PrevLaunchPath { get; set; } = String.Empty;

		// ウィンドウ位置
		public Rect DesktopBounds { get; set; }

		// RSS 確認日
		public DateTime RssCheckDate { get; set; }

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 保存パス
		// --------------------------------------------------------------------
		public static String Yv2SettingsPath()
		{
			return Common.UserAppDataFolderPath() + nameof(Yv2Settings) + Common.FILE_EXT_CONFIG;
		}
	}
}
