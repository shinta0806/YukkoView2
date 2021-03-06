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
		MpcBe,  // MPC-BE が表示されているディスプレイ
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
		Maintenance,
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
		public const String APP_VER = "Ver 2.10";
		public const String COPYRIGHT_J = "Copyright (C) 2022 by SHINTA";
#if DISTRIB_STORE
		public const String APP_DISTRIB = "ストア版";
#else
		public const String APP_DISTRIB = "zip 版";
#endif

		// --------------------------------------------------------------------
		// MessageKey
		// --------------------------------------------------------------------

		// バージョン情報ウィンドウを開く
		public const String MESSAGE_KEY_OPEN_ABOUT_WINDOW = "OpenAboutWindow";

		// コメント表示ウィンドウを開く
		public const String MESSAGE_KEY_OPEN_DISPLAY_WINDOW = "OpenDisplayWindow";

		// メインウィンドウを開く
		public const String MESSAGE_KEY_OPEN_MAIN_WINDOW = "OpenMainWindow";

		// ディスプレイ選択ウィンドウを開く
		public const String MESSAGE_KEY_OPEN_SELECT_MONITOR_WINDOW = "OpenSelectMonitorWindow";

		// 環境設定ウィンドウを開く
		public const String MESSAGE_KEY_OPEN_YV2_SETTINGS_WINDOW = "OpenYv2SettingsWindow";

		// --------------------------------------------------------------------
		// フォルダー名
		// --------------------------------------------------------------------

		// ドキュメントフォルダー名
		public const String FOLDER_NAME_DOCUMENTS = "Documents\\";

		// ゆかり用データベースを保存するフォルダー名
		public const String FOLDER_NAME_LIST = "list\\";

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------

		// ゆかり設定ファイル
		public const String FILE_NAME_YUKARI_CONFIG = "config" + Common.FILE_EXT_INI;

		// --------------------------------------------------------------------
		// ダイアログのフィルター
		// --------------------------------------------------------------------

		// 設定ファイル
		public const String DIALOG_FILTER_SETTINGS_ARCHIVE = "設定ファイル|*" + Common.FILE_EXT_SETTINGS_ARCHIVE;

		// --------------------------------------------------------------------
		// URL
		// --------------------------------------------------------------------

		// メールアドレス
		public const String EMAIL_ADDRESS = "shinta.0806@gmail.com";

		// ホームページ
		public const String URL_AUTHOR_WEB = "https://shinta.coresv.com";

		// Twitter
		public const String URL_TWITTER = "https://twitter.com/shinta0806";

		// Fantia
		public const String URL_FANTIA = "https://fantia.jp/fanclubs/65509";

		// アプリ配布ページ
		public const String URL_APP_WEB = "https://shinta.coresv.com/software/yukkoview2-jpn/";

		// アプリサポートページ
		public const String URL_APP_WEB_SUPPORT = URL_APP_WEB + "#Support";

		// よくある質問
		public const String URL_FAQ = "https://github.com/shinta0806/YukkoView2/issues?q=label%3Aquestion+sort%3Aupdated-desc";

		// --------------------------------------------------------------------
		// ゆかり関連
		// --------------------------------------------------------------------

		// デフォルトコメントフォントサイズ（ゆかりから送られてくるサイズの中）
		public const Int32 DEFAULT_YUKARI_FONT_SIZE = 3;

		// 未再生
		public const String YUKARI_REQUEST_NOW_PLAYING_QUEUED = "未再生";

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
			"ゆかり設定ファイルにコメント設定がありません。",
			"コメントサーバーと通信できません。",
		};

		// エラー要因に対するヒントの URL
		public static readonly String?[] ERROR_FACTOR_URL =
		{
			"https://github.com/shinta0806/YukkoView2/issues/7",
			"https://github.com/shinta0806/YukkoView2/issues/17",
			"https://github.com/shinta0806/YukkoView2/issues/18",
		};

		// --------------------------------------------------------------------
		// コメントコマンド（必ず 5 文字）
		// --------------------------------------------------------------------

		// 予約一覧表示
		public const String COMMENT_COMMAND_REQUEST_LIST = "rqlst";

		// --------------------------------------------------------------------
		// 楽曲情報データベースカラム名
		// --------------------------------------------------------------------

		// IRcBase
		public const String FIELD_SUFFIX_ID = "id";
		public const String FIELD_SUFFIX_IMPORT = "import";
		public const String FIELD_SUFFIX_INVALID = "invalid";
		public const String FIELD_SUFFIX_UPDATE_TIME = "update_time";
		public const String FIELD_SUFFIX_DIRTY = "dirty";

		// IRcMaster
		public const String FIELD_SUFFIX_NAME = "name";
		public const String FIELD_SUFFIX_RUBY = "ruby";
		public const String FIELD_SUFFIX_RUBY_FOR_SEARCH = "ruby_for_search";
		public const String FIELD_SUFFIX_KEYWORD = "keyword";
		public const String FIELD_SUFFIX_KEYWORD_RUBY_FOR_SEARCH = "keyword_ruby_for_search";

		// IRcCategorizable
		public const String FIELD_SUFFIX_CATEGORY_ID = "category_id";
		public const String FIELD_SUFFIX_RELEASE_DATE = "release_date";

		// IRcAlias
		public const String FIELD_SUFFIX_ALIAS = "alias";
		public const String FIELD_SUFFIX_ORIGINAL_ID = "original_id";

		// IRcSequence
		public const String FIELD_SUFFIX_SEQUENCE = "sequence";
		public const String FIELD_SUFFIX_LINK_ID = "link_id";

		// TSong 独自項目
		public const String FIELD_SUFFIX_TIE_UP_ID = "tie_up_id";
		public const String FIELD_SUFFIX_OP_ED = "op_ed";

		// TTieUp 独自項目
		public const String FIELD_SUFFIX_MAKER_ID = "maker_id";
		public const String FIELD_SUFFIX_AGE_LIMIT = "age_limit";

		// --------------------------------------------------------------------
		// その他
		// --------------------------------------------------------------------

		// Microsoft Store での製品 ID
		public const String STORE_PRODUCT_ID = "9NLZX1SFJV98";

		// 同時に表示するコメントの最大数
		// サーバーからの取得時の指標であり、UI からの投稿時は無制限
		public const Int32 NUM_DISPLAY_COMMENTS_MAX = 7;

		// ツールチップを長く表示する場合の時間 [ms]
		public const Int32 TOOL_TIP_LONG_DURATION = 20 * 1000;

		// 連続投稿防止間隔 [ms]
		public const Int32 CONTINUOUS_PREVENT_TIME = 5000;

		// 通信確認間隔 [ms]
		public const Int32 CHECK_CONNECTION_INTERVAL = 10 * 1000;

		// 枠描画時の比率
		public const Int32 FRAME_DIVIDER = 20;

		// 年齢制限のデフォルト値
		public const Int32 AGE_LIMIT_DEFAULT = -1;

		// ラベル（XAML 側で ContentStringFormat で結合するとアクセラレータキーが効かないためここで結合）
		public const String LABEL_CONTENT_CHECK_RSS = APP_NAME_J + "の最新情報を自動的に確認する (_L)";
	}
}
