// ============================================================================
// 
// ゆっこビュー 2 共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;

namespace YukkoView2.Models.SharedMisc
{
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
		// その他
		// --------------------------------------------------------------------

		// DPI
		public const Single DPI = 96.0f;
	}
}
