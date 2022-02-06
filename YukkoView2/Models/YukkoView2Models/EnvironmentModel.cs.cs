// ============================================================================
// 
// 環境設定類を管理する
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Shinta;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using YukkoView2.Models.Settings;
using YukkoView2.Models.SharedMisc;

namespace YukkoView2.Models.YukkoView2Models
{
	internal class EnvironmentModel : NotificationObject
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public EnvironmentModel()
		{
			// 最初にログの設定をする
			SetLogWriter();

			// Yv2StatusErrorFactors 要素アロケート
			for (Int32 i = 0; i < (Int32)Yv2StatusErrorFactor.__End__; i++)
			{
				Yv2StatusErrorFactors.Add(false);
			}

			// 環境設定の Load() はしない（YlModel.Instance 生成途中で EnvironmentModel が生成され、エラー発生時に YukaListerModel.Instance 経由でのログ記録ができないため）
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// 一般プロパティー
		// --------------------------------------------------------------------

		// 環境設定
		public Yv2Settings Yv2Settings { get; } = new();

		// エラーが発生しているかどうか（要因ごと）
		public ObservableSynchronizedCollection<Boolean> Yv2StatusErrorFactors { get; } = new();

		// ログ
		public LogWriter LogWriter { get; } = new(Yv2Constants.APP_ID);

		// EXE フルパス
		private String? _exeFullPath;
		public String ExeFullPath
		{
			get
			{
				if (_exeFullPath == null)
				{
					// 単一ファイル時にも内容が格納される GetCommandLineArgs を用いる（Assembly 系の Location は不可）
					_exeFullPath = Environment.GetCommandLineArgs()[0];
					if (Path.GetExtension(_exeFullPath).ToLower() != Common.FILE_EXT_EXE)
					{
						_exeFullPath = Path.ChangeExtension(_exeFullPath, Common.FILE_EXT_EXE);
					}
				}
				return _exeFullPath;
			}
		}

		// EXE があるフォルダーのフルパス（末尾 '\\'）
		private String? _exeFullFolder;
		public String ExeFullFolder
		{
			get
			{
				if (_exeFullFolder == null)
				{
					_exeFullFolder = Path.GetDirectoryName(ExeFullPath) + '\\';
				}
				return _exeFullFolder;
			}
		}

		// アプリケーション終了時タスク安全中断用
		public CancellationTokenSource AppCancellationTokenSource { get; } = new();

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region ヘルプリンクの制御
		private ListenerCommand<String>? _helpClickedCommand;

		public ListenerCommand<String> HelpClickedCommand
		{
			get
			{
				if (_helpClickedCommand == null)
				{
					_helpClickedCommand = new ListenerCommand<String>(HelpClicked);
				}
				return _helpClickedCommand;
			}
		}

		public void HelpClicked(String parameter)
		{
			try
			{
				ShowHelp(parameter);
			}
			catch (Exception excep)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "ヘルプ表示時エラー：\n" + excep.Message);
				LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
		}
		#endregion

		// ====================================================================
		// private 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------
		private const String FILE_NAME_HELP_PREFIX = Yv2Constants.APP_ID + "_JPN";

		// --------------------------------------------------------------------
		// フォルダー名
		// --------------------------------------------------------------------
		private const String FOLDER_NAME_HELP_PARTS = "HelpParts\\";

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// LogWriter の設定
		// --------------------------------------------------------------------
		private void SetLogWriter()
		{
			LogWriter.ApplicationQuitToken = AppCancellationTokenSource.Token;
			LogWriter.SimpleTraceListener.MaxSize = 10 * 1024 * 1024;
			LogWriter.SimpleTraceListener.MaxOldGenerations = 5;
			LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "起動しました：" + Yv2Constants.APP_NAME_J + " "
					+ Yv2Constants.APP_VER + "  " + Yv2Constants.APP_DISTRIB + " ====================");
#if DEBUG
			LogWriter.ShowLogMessage(TraceEventType.Verbose, "デバッグモード：" + Common.DEBUG_ENABLED_MARK);
#endif
			LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "プロセス動作モード：" + (Environment.Is64BitProcess ? "64" : "32"));
			LogWriter.ShowLogMessage(TraceEventType.Verbose, "Path: " + ExeFullPath);
		}

		// --------------------------------------------------------------------
		// ヘルプの表示
		// --------------------------------------------------------------------
		private void ShowHelp(String? anchor = null)
		{
			String? helpPath = null;

			try
			{
				// アンカーが指定されている場合は状況依存型ヘルプを表示
				if (!String.IsNullOrEmpty(anchor))
				{
					helpPath = ExeFullFolder + Yv2Constants.FOLDER_NAME_DOCUMENTS + FOLDER_NAME_HELP_PARTS + FILE_NAME_HELP_PREFIX + "_" + anchor + Common.FILE_EXT_HTML;
					try
					{
						Common.ShellExecute(helpPath);
						return;
					}
					catch (Exception ex)
					{
						LogWriter.ShowLogMessage(TraceEventType.Error, "状況に応じたヘルプを表示できませんでした：\n" + ex.Message + "\n" + helpPath
								+ "\n通常のヘルプを表示します。");
					}
				}

				// アンカーが指定されていない場合・状況依存型ヘルプを表示できなかった場合は通常のヘルプを表示
				helpPath = ExeFullFolder + Yv2Constants.FOLDER_NAME_DOCUMENTS + FILE_NAME_HELP_PREFIX + Common.FILE_EXT_HTML;
				Common.ShellExecute(helpPath);
			}
			catch (Exception ex)
			{
				LogWriter.ShowLogMessage(TraceEventType.Error, "ヘルプを表示できませんでした。\n" + ex.Message + "\n" + helpPath);
			}
		}
	}
}
