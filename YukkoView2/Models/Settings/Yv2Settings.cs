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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Windows;

using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

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
		// 設定
		// --------------------------------------------------------------------

		// 起動と同時にコメント表示を開始する
		public Boolean PlayOnStart { get; set; } = true;

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
		public String ServerUrlSeed { get; set; } = String.Empty;

		// コメントサーバー指定方法：手動
		// ルーム名
		public String RoomNameSeed { get; set; } = String.Empty;

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
		// ゆかり設定ファイルが正しく指定されているかどうか
		// --------------------------------------------------------------------
		public Boolean IsYukariConfigPathValid()
		{
			return File.Exists(YukariConfigPath());
		}

		// --------------------------------------------------------------------
		// コメントサーバー指定方法を考慮したサーバー URL と RoomName
		// --------------------------------------------------------------------
		public (String serverUrl, String roomName) ServerUrlAndRoomName()
		{
			String serverUrl;
			String roomName;
			switch (CommentServerType)
			{
				case CommentServerType.Auto:
					(serverUrl, roomName) = AnalyzeYukariConfig();
					break;
				case CommentServerType.Manual:
					serverUrl = ServerUrlSeed;
					roomName = RoomNameSeed;
					break;
				default:
					Debug.Assert(false, "ServerUrl() bad CommentServerType");
					serverUrl = String.Empty;
					roomName = String.Empty;
					break;
			}
			return (serverUrl, roomName);
		}

		// --------------------------------------------------------------------
		// ゆかり設定ファイルのフルパス
		// --------------------------------------------------------------------
		public String YukariConfigPath()
		{
			if (Path.IsPathRooted(YukariConfigPathSeed))
			{
				return YukariConfigPathSeed;
			}
			else
			{
				return Common.MakeAbsolutePath(Yv2Model.Instance.EnvModel.ExeFullFolder, YukariConfigPathSeed);
			}
		}

		// --------------------------------------------------------------------
		// 保存パス
		// --------------------------------------------------------------------
		public static String Yv2SettingsPath()
		{
			return Common.UserAppDataFolderPath() + nameof(Yv2Settings) + Common.FILE_EXT_CONFIG;
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 読み込み後の調整
		// --------------------------------------------------------------------
		protected override void AdjustAfterLoad()
		{
			// 範囲調整
			CommentServerType = (CommentServerType)Math.Clamp((Int32)CommentServerType, 0, (Int32)CommentServerType.__End__ - 1);
			CommentReceiveType = (CommentReceiveType)Math.Clamp((Int32)CommentReceiveType, 0, (Int32)CommentReceiveType.__End__ - 1);

			// コメントサーバー指定方法が自動の場合は、手動のデフォルト値を設定
			if (CommentServerType == CommentServerType.Auto)
			{
				(ServerUrlSeed, RoomNameSeed) = AnalyzeYukariConfig();
				if (String.IsNullOrEmpty(ServerUrlSeed))
				{
					ServerUrlSeed = "http://localhost/cms/" + FILE_NAME_COMMENT_SERVER_DEFAULT;
				}
			}
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// ゆかりの config.ini の項目
		private const String YUKARI_CONFIG_KEY_NAME_ROOM_NAME = "commentroom";
		private const String YUKARI_CONFIG_KEY_NAME_SERVER_URL = "commenturl_base";

		// デフォルトのコメントサーバーファイル名
		private const String FILE_NAME_COMMENT_SERVER_DEFAULT = "c.php";

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ゆかり設定ファイルを解析してゆかりの設定を取得
		// --------------------------------------------------------------------
		private (String serverUrl, String roomName) AnalyzeYukariConfig()
		{
			String serverUrl = String.Empty;
			String roomName = String.Empty;

			try
			{
				if (!IsYukariConfigPathValid())
				{
					throw new Exception("ゆかり設定ファイルが正しく指定されていません。");
				}

				String[] config = File.ReadAllLines(YukariConfigPath(), Encoding.UTF8);

				// コメントサーバー URL
				String serverBase = HttpUtility.UrlDecode(YukariConfigValue(config, YUKARI_CONFIG_KEY_NAME_SERVER_URL));
				if (!String.IsNullOrEmpty(serverBase))
				{
					Int32 slash = serverBase.LastIndexOf("/");
					if (slash >= 0)
					{
						serverUrl = serverBase[..slash] + "/" + FILE_NAME_COMMENT_SERVER_DEFAULT;
					}
				}

				// ルーム名
				roomName = YukariConfigValue(config, YUKARI_CONFIG_KEY_NAME_ROOM_NAME);
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, ex.Message);
			}
			return (serverUrl, roomName);
		}

		// --------------------------------------------------------------------
		// ゆかり設定を config.ini の内容から取得
		// --------------------------------------------------------------------
		private static String YukariConfigValue(String[] config, String keyName)
		{
			// キーを検索
			Int32 line = -1;
			for (Int32 i = 0; i < config.Length; i++)
			{
				if (config[i].StartsWith(keyName + "="))
				{
					line = i;
					break;
				}
			}
			if (line < 0)
			{
				// キーがない
				return String.Empty;
			}

			// 値を検索
			Int32 pos = config[line].IndexOf('=');
			if (pos == config[line].Length - 1)
			{
				// 値がない
				return String.Empty;
			}

			return config[line][(pos + 1)..];
		}
	}
}
