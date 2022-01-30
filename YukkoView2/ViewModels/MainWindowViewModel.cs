// ============================================================================
// 
// メインウィンドウ（コントロールウィンドウ）の ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Diagnostics;

using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.ViewModels
{
	internal class MainWindowViewModel : Yv2ViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// プログラム中で使うべき引数付きコンストラクター
		// --------------------------------------------------------------------
		public MainWindowViewModel(SplashWindowViewModel splashWindowViewModel)
		{
			_splashWindowViewModel = splashWindowViewModel;
		}

		// --------------------------------------------------------------------
		// ダミーコンストラクター（Visual Studio・TransitionMessage 用）
		// --------------------------------------------------------------------
		public MainWindowViewModel()
		{
			// 警告抑止用にメンバーを null! で初期化
			_splashWindowViewModel = null!;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// ウィンドウ左端
		private Double _left;
		public Double Left
		{
			get => _left;
			set => RaisePropertyChangedIfSet(ref _left, value);
		}

		// ウィンドウ上端
		private Double _top;
		public Double Top
		{
			get => _top;
			set => RaisePropertyChangedIfSet(ref _top, value);
		}

#if false
		// ウィンドウ幅
		private Double _width;
		public Double Width
		{
			get => _width;
			set => RaisePropertyChangedIfSet(ref _width, value);
		}

		// ウィンドウ高さ
		private Double _height;
		public Double Height
		{
			get => _height;
			set => RaisePropertyChangedIfSet(ref _height, value);
		}
#endif

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public override void Initialize()
		{
			base.Initialize();

			try
			{
				// タイトルバー
				Title = Yv2Constants.APP_NAME_J;

				// スプラッシュウィンドウを閉じる
				_splashWindowViewModel.Close();
			}
			catch (Exception excep)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "メインウィンドウ初期化時エラー：\n" + excep.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
		}

		// ====================================================================
		// protected 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リソース解放
		// --------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_isDisposed)
			{
				return;
			}

			try
			{
				// アプリケーションの終了を通知
				Yv2Model.Instance.EnvModel.AppCancellationTokenSource.Cancel();

				// 終了処理
				// await するとその間に強制終了されてしまうようなので、await しない
				//SaveExitStatus();
				//Common.DeleteTempFolder();

				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "終了しました：" + Yv2Constants.APP_NAME_J + " "
						+ Yv2Constants.APP_VER + " --------------------");

				_isDisposed = true;
			}
			catch (Exception excep)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "メインウィンドウ破棄時エラー：\n" + excep.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
		}

		// ====================================================================
		// private 変数
		// ====================================================================

		// スプラッシュウィンドウ
		private readonly SplashWindowViewModel _splashWindowViewModel;

		// Dispose フラグ
		private Boolean _isDisposed;
	}
}
