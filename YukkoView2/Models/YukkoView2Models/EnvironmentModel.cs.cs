// ============================================================================
// 
// 環境設定類を管理する
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Livet;

using Shinta;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

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

#if false
		// マルチディスプレイ領域
		// メインスレッドのみからアクセスするものとする
		private List<Rect> _monitorRects = new();
		public List<Rect> MonitorRects
		{
			get
			{
				Debug.Assert(Environment.CurrentManagedThreadId == DispatcherHelper.UIDispatcher.Thread.ManagedThreadId, "MonitorRects.get() not UI thread");
				return _monitorRects;
			}
			set
			{
				Debug.Assert(Environment.CurrentManagedThreadId == DispatcherHelper.UIDispatcher.Thread.ManagedThreadId, "MonitorRects.set() not UI thread");
				_monitorRects = value;
			}
		}
#endif

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
	}
}
