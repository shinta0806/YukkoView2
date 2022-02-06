// ============================================================================
// 
// ゆっこビュー 2 共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Windows.Media;

namespace YukkoView2.Models.SharedMisc
{
	// ====================================================================
	// public 列挙子
	// ====================================================================

	// --------------------------------------------------------------------
	// コメント受信方法
	// --------------------------------------------------------------------
	public enum CommentReceiveType
	{
		Push,       // プッシュ通知
		Download,   // ダウンロード
		__End__,
	}

	// --------------------------------------------------------------------
	// コメントサーバー指定方法
	// --------------------------------------------------------------------
	public enum CommentServerType
	{
		Auto,   // 自動（ゆかり設定ファイルから取得）
		Manual, // 手動
		__End__,
	}

	// --------------------------------------------------------------------
	// ディスプレイ選択方法
	// --------------------------------------------------------------------
	public enum SelectMonitorType
	{
		MpcBe,	// MPC-BE が表示されているディスプレイ
		Manual, // 手動
		__End__,
	}

	// --------------------------------------------------------------------
	// 環境設定ウィンドウのタブアイテム
	// --------------------------------------------------------------------
	public enum Yv2SettingsTabItem
	{
		Settings,
		Receive,
		__End__,
	}

	// --------------------------------------------------------------------
	// ゆっこビュー 2 の動作状況
	// --------------------------------------------------------------------
	public enum Yv2Status
	{
		Ready,      // 待機
		Running,    // 実行中
		Error,      // エラー
		__End__,
	}

	// --------------------------------------------------------------------
	// ゆっこビュー 2 のエラー要因
	// --------------------------------------------------------------------
	public enum Yv2StatusErrorFactor
	{
		YukariConfigNotFound,
		YukariConfigBadContents,
		ServerNotConnected,
		__End__,
	}

	internal class Yv2Constants
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// アプリの基本情報
		// --------------------------------------------------------------------
		public const String APP_ID = "YukkoView2";
		public const String APP_NAME_J = "ゆっこビュー 2 ";
		public const String APP_VER = "Ver 0.56 α3";
		public const String COPYRIGHT_J = "Copyright (C) 2022 by SHINTA";
#if DISTRIB_STORE
		public const String APP_DISTRIB = "ストア版";
#else
		public const String APP_DISTRIB = "zip 版";
#endif

		// --------------------------------------------------------------------
		// MessageKey
		// --------------------------------------------------------------------

		// コメント表示ウィンドウを開く
		public const String MESSAGE_KEY_OPEN_DISPLAY_WINDOW = "OpenDisplayWindow";

		// メインウィンドウを開く
		public const String MESSAGE_KEY_OPEN_MAIN_WINDOW = "OpenMainWindow";

		// ディスプレイ選択ウィンドウを開く
		public const String MESSAGE_KEY_OPEN_SELECT_MONITOR_WINDOW = "OpenSelectMonitorWindow";

		// 環境設定ウィンドウを開く
		public const String MESSAGE_KEY_OPEN_YV2_SETTINGS_WINDOW = "OpenYv2SettingsWindow";

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------

		// ゆかり設定ファイル
		public const String FILE_NAME_YUKARI_CONFIG = "config" + Common.FILE_EXT_INI;

		// --------------------------------------------------------------------
		// ゆかり関連
		// --------------------------------------------------------------------

		// デフォルトコメントフォントサイズ（ゆかりから送られてくるサイズの中）
		public const Int32 DEFAULT_YUKARI_FONT_SIZE = 3;

		// --------------------------------------------------------------------
		// 状態色
		// --------------------------------------------------------------------

		// 動作中
		public static readonly Color COLOR_STATUS_RUNNING = Color.FromRgb(0xE1, 0xFF, 0xE1);

		// 完了
		public static readonly Color COLOR_STATUS_DONE = Color.FromRgb(0xE1, 0xE1, 0xFF);

		// エラー
		public static readonly Color COLOR_STATUS_ERROR = Color.FromRgb(0xFF, 0xE1, 0xE1);

		// --------------------------------------------------------------------
		// 状態ブラシ
		// --------------------------------------------------------------------

		// 動作中
		public static readonly SolidColorBrush BRUSH_STATUS_RUNNING = new(COLOR_STATUS_RUNNING);

		// 完了
		public static readonly SolidColorBrush BRUSH_STATUS_DONE = new(COLOR_STATUS_DONE);

		// エラー
		public static readonly SolidColorBrush BRUSH_STATUS_ERROR = new(COLOR_STATUS_ERROR);

		// --------------------------------------------------------------------
		// エラーメッセージ
		// --------------------------------------------------------------------

		// エラー要因ごとのメッセージ
		public static readonly String[] ERROR_FACTOR_MESSAGE =
		{
			"ゆかり設定ファイルが見つかりませんでした。",
			"ゆかり設定を読み込めませんでした。",
			"コメントサーバーと通信できません。",
		};

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// 同時に表示するコメントの最大数
		// サーバーからの取得時の指標であり、UI からの投稿時は無制限
		public const Int32 NUM_DISPLAY_COMMENTS_MAX = 7;

		// ツールチップを長く表示する場合の時間 [ms]
		public const Int32 TOOL_TIP_LONG_DURATION = 20 * 1000;

		// 連続投稿防止間隔 [ms]
		public const Int32 CONTINUOUS_PREVENT_TIME = 5000;

		// 通信確認間隔 [ms]
		public const Int32 CHECK_CONNECTION_INTERVAL = 1000;

		// 枠描画時の比率
		public const Int32 FRAME_DIVIDER = 20;
	}
}
