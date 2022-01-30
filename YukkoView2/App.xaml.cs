// ============================================================================
// 
// ゆっこビュー 2 アプリケーション
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Livet;

using System.Windows;

namespace YukkoView2
{
	public partial class App : Application
	{
		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// スタートアップ
		// --------------------------------------------------------------------
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			// Livet コード
			DispatcherHelper.UIDispatcher = Dispatcher;
			//AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
		}

		// Application level error handling
		//private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		//{
		//    //TODO: Logging
		//    MessageBox.Show(
		//        "Something errors were occurred.",
		//        "Error",
		//        MessageBoxButton.OK,
		//        MessageBoxImage.Error);
		//
		//    Environment.Exit(1);
		//}
	}
}
