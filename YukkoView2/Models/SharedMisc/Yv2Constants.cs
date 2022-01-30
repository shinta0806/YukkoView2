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
	}

	// --------------------------------------------------------------------
	// コメントサーバー指定方法
	// --------------------------------------------------------------------
	public enum CommentServerType
	{
		Auto,   // 自動（ゆかり設定ファイルから取得）
		Manual, // 手動
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
		public const String APP_VER = "Ver 0.01 α";
		public const String COPYRIGHT_J = "Copyright (C) 2022 by SHINTA";
#if DISTRIB_STORE
		public const String APP_DISTRIB = "ストア版";
#else
		public const String APP_DISTRIB = "zip 版";
#endif

		// --------------------------------------------------------------------
		// MessageKey
		// --------------------------------------------------------------------

		// メインウィンドウを開く
		public const String MESSAGE_KEY_OPEN_MAIN_WINDOW = "OpenMainWindow";

		// コメント表示ウィンドウを開く
		public const String MESSAGE_KEY_OPEN_DISPLAY_WINDOW = "OpenDisplayWindow";

		// ウィンドウをアクティブ化する
		public const String MESSAGE_KEY_WINDOW_ACTIVATE = "Activate";

		// ウィンドウを閉じる
		public const String MESSAGE_KEY_WINDOW_CLOSE = "Close";

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------

		// ゆかり設定ファイル
		public const String FILE_NAME_YUKARI_CONFIG = "config" + Common.FILE_EXT_INI;

		// --------------------------------------------------------------------
		// 状態色
		// --------------------------------------------------------------------

		// 待機中
		//public static readonly Color COLOR_STATUS_QUEUED = Color.FromRgb(0xFA, 0xFA, 0xFA);

		// 動作中
		public static readonly Color COLOR_STATUS_RUNNING = Color.FromRgb(0xE1, 0xFF, 0xE1);

		// 完了
		public static readonly Color COLOR_STATUS_DONE = Color.FromRgb(0xE1, 0xE1, 0xFF);

		// エラー
		public static readonly Color COLOR_STATUS_ERROR = Color.FromRgb(0xFF, 0xE1, 0xE1);

		// --------------------------------------------------------------------
		// 状態ブラシ
		// --------------------------------------------------------------------

		// 待機中
		//public static readonly SolidColorBrush BRUSH_STATUS_QUEUED = new(COLOR_STATUS_QUEUED);

		// 動作中
		public static readonly SolidColorBrush BRUSH_STATUS_RUNNING = new(COLOR_STATUS_RUNNING);

		// 完了
		public static readonly SolidColorBrush BRUSH_STATUS_DONE = new(COLOR_STATUS_DONE);

		// エラー
		public static readonly SolidColorBrush BRUSH_STATUS_ERROR = new(COLOR_STATUS_ERROR);

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// DPI
		public const Single DPI = 96.0f;
	}
}
