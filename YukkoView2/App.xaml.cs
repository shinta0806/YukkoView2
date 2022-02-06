// ============================================================================
// 
// ゆっこビュー 2 アプリケーション
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Livet;

using Shinta;

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2
{
	public partial class App : Application
	{
		// ====================================================================
		// private 関数
		// ====================================================================

		// 多重起動防止用
		// アプリケーション終了までガベージコレクションされないようにメンバー変数で持つ
		private Mutex? _mutex;

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// スタートアップ
		// --------------------------------------------------------------------
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			// Livet コード
			DispatcherHelper.UIDispatcher = Dispatcher;

			// 集約エラーハンドラー設定
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			// 多重起動チェック
			_mutex = CommonWindows.ActivateAnotherProcessWindowIfNeeded(Common.SHINTA + '_' + Yv2Constants.APP_ID);
			if (_mutex == null)
			{
				throw new MultiInstanceException();
			}
		}

		// --------------------------------------------------------------------
		// 集約エラーハンドラー
		// --------------------------------------------------------------------
		private void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
			Boolean onProcessExit = false;

			if (unhandledExceptionEventArgs.ExceptionObject is MultiInstanceException)
			{
				// 多重起動の場合は何もしない
			}
			else
			{
				if (unhandledExceptionEventArgs.ExceptionObject is Exception excep)
				{
					onProcessExit = excep.StackTrace?.Contains("OnProcessExit") ?? false;

					// Yv2Model 未生成の可能性があるためまずはメッセージ表示のみ、ただし onProcessExit の場合は表示しない
					if (!onProcessExit)
					{
						MessageBox.Show("不明なエラーが発生しました。アプリケーションを終了します。\n" + excep.Message + "\n" + excep.InnerException?.Message + "\n" + excep.StackTrace,
								"エラー", MessageBoxButton.OK, MessageBoxImage.Error);
					}

					try
					{
						// 可能であればログする。Yv2Model 生成中に例外が発生する可能性がある
						Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "集約エラーハンドラー：\n" + excep.Message + "\n" + excep.InnerException?.Message);
						Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
					}
					catch (Exception)
					{
						MessageBox.Show("エラーの記録ができませんでした。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}

			if (onProcessExit)
			{
				// アプリが終了シーケンスに入っている場合、Exit() するとゾンビプロセスになるようなので、Exit() しない
				// ゆっこビュー 2 では該当ケースが確認されていないが、ゆかりすたー 4 NEBULA で事例がある
				return;
			}

			Environment.Exit(1);
		}
	}
}
