// ============================================================================
// 
// メインウィンドウ（コントロールウィンドウ）の ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet.EventListeners;
using Livet.Messaging;

using Shinta;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;
using YukkoView2.ViewModels.MiscWindowViewModels;

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
			// スプラッシュウィンドウ
			_splashWindowViewModel = splashWindowViewModel;

			// コメント表示ウィンドウ
			_displayWindowViewModel = new();
			CompositeDisposable.Add(_displayWindowViewModel);

			// Yv2Status 変更監視
			_yv2StatusListener = new(Yv2Model.Instance.EnvModel)
			{
				{
					() => Yv2Model.Instance.EnvModel.Yv2Status,
					Yv2StatusChanged
				},
			};
			CompositeDisposable.Add(_yv2StatusListener);
		}

		// --------------------------------------------------------------------
		// ダミーコンストラクター（Visual Studio・TransitionMessage 用）
		// --------------------------------------------------------------------
		public MainWindowViewModel()
		{
			// 警告抑止用にメンバーを null! で初期化
			_splashWindowViewModel = null!;
			_displayWindowViewModel = null!;
			_yv2StatusListener = null!;
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

		// ゆっこビュー 2 の動作状況
		private String _yv2StatusMessage = String.Empty;
		public String Yv2StatusMessage
		{
			get => _yv2StatusMessage;
			set => RaisePropertyChangedIfSet(ref _yv2StatusMessage, value);
		}

		// ゆっこビュー 2 の動作状況の背景
		private Brush _yv2StatusBackground = Yv2Constants.BRUSH_STATUS_DONE;
		public Brush Yv2StatusBackground
		{
			get => _yv2StatusBackground;
			set => RaisePropertyChangedIfSet(ref _yv2StatusBackground, value);
		}

		// ゆっこビュー 2 の動作状況のカーソル
		private Cursor? _yv2StatusCursor;
		public Cursor? Yv2StatusCursor
		{
			get => _yv2StatusCursor;
			set => RaisePropertyChangedIfSet(ref _yv2StatusCursor, value);
		}

		// コメント
		private String? _comment = "こんにちは";
		public String? Comment
		{
			get => _comment;
			set => RaisePropertyChangedIfSet(ref _comment, value);
		}

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

				// コメント表示ウィンドウを開く
				Messenger.Raise(new TransitionMessage(_displayWindowViewModel, Yv2Constants.MESSAGE_KEY_OPEN_DISPLAY_WINDOW));

				// コメント表示開始
				if (Yv2Model.Instance.EnvModel.Yv2Settings.PlayOnStart)
				{
					Play();
				}

#if DEBUGz
				Yv2Model.Instance.EnvModel.Yv2Status = Yv2Status.Error;
				Yv2Model.Instance.EnvModel.Yv2Status = Yv2Status.Ready;
#endif
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "メインウィンドウ初期化時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
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
				SaveExitStatus();
				//Common.DeleteTempFolder();

				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "終了しました：" + Yv2Constants.APP_NAME_J + " "
						+ Yv2Constants.APP_VER + " --------------------");

				_isDisposed = true;
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "メインウィンドウ破棄時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// ====================================================================
		// private 変数
		// ====================================================================

		// スプラッシュウィンドウ
		private readonly SplashWindowViewModel _splashWindowViewModel;

		// コメント表示ウィンドウ
		private DisplayWindowViewModel _displayWindowViewModel;

		// Yv2Status 変更監視
		private PropertyChangedEventListener _yv2StatusListener;

		// Dispose フラグ
		private Boolean _isDisposed;

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コメント表示開始
		// --------------------------------------------------------------------
		private void Play()
		{
			// ウィンドウを前面に出すなど
			_displayWindowViewModel.Messenger.Raise(new InteractionMessage(Yv2Constants.MESSAGE_KEY_WINDOW_ACTIVATE));

			// 開始
			_displayWindowViewModel.StartAsync();
		}

		// --------------------------------------------------------------------
		// 終了時の状態を保存
		// --------------------------------------------------------------------
		private void SaveExitStatus()
		{
			Yv2Model.Instance.EnvModel.Yv2Settings.PrevLaunchPath = Yv2Model.Instance.EnvModel.ExeFullPath;
			Yv2Model.Instance.EnvModel.Yv2Settings.PrevLaunchVer = Yv2Constants.APP_VER;
			Yv2Model.Instance.EnvModel.Yv2Settings.DesktopBounds = new Rect(Left, Top, Int32.MaxValue, Int32.MaxValue);
			Yv2Model.Instance.EnvModel.Yv2Settings.Save();
		}

		// --------------------------------------------------------------------
		// Yv2Model.Instance.EnvModel.Yv2Status が変更された
		// --------------------------------------------------------------------
		private void Yv2StatusChanged(Object? sender, PropertyChangedEventArgs e)
		{
			switch (Yv2Model.Instance.EnvModel.Yv2Status)
			{
				case Yv2Status.Ready:
					Yv2StatusMessage = "コメント表示停止中";
					Yv2StatusBackground = Yv2Constants.BRUSH_STATUS_DONE;
					break;
				case Yv2Status.Running:
					Yv2StatusMessage = "コメント表示中";
					Yv2StatusBackground = Yv2Constants.BRUSH_STATUS_RUNNING;
					break;
				default:
					Yv2StatusMessage = "エラー";
					Yv2StatusBackground = Yv2Constants.BRUSH_STATUS_ERROR;
					break;
			}
		}
	}
}
