﻿// ============================================================================
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

		// 上下の指定パーセントにコメントを表示しない
		public Boolean EnableMargin { get; set; }

		// パーセント
		public Int32 MarginPercent { get; set; } = 10;

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
		// ディスプレイ選択
		// --------------------------------------------------------------------

		// ディスプレイ選択方法
		public SelectMonitorType SelectMonitorType { get; set; }

		// ディスプレイ選択方法：MPC-BE
		// MPC-BE 64 ビット版のプロセス名
		public String MpcBe64ProcessName { get; set; } = "mpc-be64";

		// ディスプレイ選択方法：MPC-BE
		// MPC-BE 32 ビット版のプロセス名
		public String MpcBe32ProcessName { get; set; } = "mpc-be";

		// ディスプレイ選択方法：手動
		// ディスプレイ番号（0 オリジン）
		public Int32 MonitorIndex { get; set; }

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
				return Path.GetFullPath(YukariConfigPathSeed, Yv2Model.Instance.EnvModel.ExeFullFolder);
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
			SelectMonitorType = (SelectMonitorType)Math.Clamp((Int32)SelectMonitorType, 0, (Int32)SelectMonitorType.__End__ - 1);

			// コメントサーバー指定方法が自動の場合は、手動のデフォルト値を設定（未設定の場合のみ）
			if (CommentServerType == CommentServerType.Auto)
			{
				(String serverUrlSeed, String roomNameSeed) = AnalyzeYukariConfig();
				if (String.IsNullOrEmpty(ServerUrlSeed))
				{
					if (String.IsNullOrEmpty(serverUrlSeed))
					{
						ServerUrlSeed = "http://localhost/cms/" + FILE_NAME_COMMENT_SERVER_DEFAULT;
					}
					else
					{
						ServerUrlSeed = serverUrlSeed;
					}
				}
				if (String.IsNullOrEmpty(RoomNameSeed))
				{
					RoomNameSeed = roomNameSeed;
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
				Yv2Model.Instance.EnvModel.Yv2StatusErrorFactors[(Int32)Yv2StatusErrorFactor.YukariConfigNotFound] = !IsYukariConfigPathValid();
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
