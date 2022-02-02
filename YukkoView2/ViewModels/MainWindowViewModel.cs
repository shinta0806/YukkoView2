// ============================================================================
// 
// メインウィンドウ（コントロールウィンドウ）の ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;

using Shinta;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
			set
			{
				if (RaisePropertyChangedIfSet(ref _comment, value))
				{
					ButtonCommentClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// --------------------------------------------------------------------
		// 一般プロパティー
		// --------------------------------------------------------------------

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region 開始ボタンの制御
		private ViewModelCommand? _buttonPlayClickedCommand;

		public ViewModelCommand ButtonPlayClickedCommand
		{
			get
			{
				if (_buttonPlayClickedCommand == null)
				{
					_buttonPlayClickedCommand = new ViewModelCommand(ButtonPlayClicked, CanButtonPlayClicked);
				}
				return _buttonPlayClickedCommand;
			}
		}

		public Boolean CanButtonPlayClicked()
		{
			return !_isPlaying;
		}

		public void ButtonPlayClicked()
		{
			try
			{
				_ = PlayAsync();
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "開始ボタンクリック時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}
		#endregion

		#region 停止ボタンの制御
		private ViewModelCommand? _buttonStopClickedCommand;

		public ViewModelCommand ButtonStopClickedCommand
		{
			get
			{
				if (_buttonStopClickedCommand == null)
				{
					_buttonStopClickedCommand = new ViewModelCommand(ButtonStopClicked, CanButtonStopClicked);
				}
				return _buttonStopClickedCommand;
			}
		}

		public Boolean CanButtonStopClicked()
		{
			return _isPlaying;
		}

		public void ButtonStopClicked()
		{
			try
			{
				_ = StopAsync();
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "停止ボタンクリック時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}
		#endregion

		#region 投稿ボタンの制御
		private ViewModelCommand? _buttonCommentClickedCommand;

		public ViewModelCommand ButtonCommentClickedCommand
		{
			get
			{
				if (_buttonCommentClickedCommand == null)
				{
					_buttonCommentClickedCommand = new ViewModelCommand(ButtonCommentClicked, CanButtonCommentClicked);
				}
				return _buttonCommentClickedCommand;
			}
		}

		public Boolean CanButtonCommentClicked()
		{
			return !String.IsNullOrEmpty(Comment);
		}

		public async void ButtonCommentClicked()
		{
			try
			{
				if (String.IsNullOrEmpty(Comment))
				{
					return;
				}

#if DEBUG
				await Task.Delay(3000);
#endif

				CommentInfo commentInfo = new();
				commentInfo.Message = Comment;
				commentInfo.YukariSize = Yv2Constants.DEFAULT_YUKARI_FONT_SIZE;
				commentInfo.Color = COMMENT_COLORS[_commentColorIndex];
				commentInfo.InitialTick = Environment.TickCount;

				// 色番号調整
				_commentColorIndex++;
				if (_commentColorIndex >= COMMENT_COLORS.Length)
				{
					_commentColorIndex = 0;
				}

				// コメント追加
				_displayWindowViewModel.AddComment(commentInfo);
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "投稿ボタンクリック時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}
		#endregion

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
					_ = PlayAsync();
				}

#if DEBUGz
				List<Rect> rects = new();
				Int32 tick = Environment.TickCount;
				for (Int32 i = 0; i < 1000; i++)
				{
					rects = CommonWindows.GetMonitorRects();
				}
				Debug.WriteLine("Initialize() " + rects.Count + " time: " + (Environment.TickCount - tick).ToString());
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
		// private 定数
		// ====================================================================

		// テストコメント投稿用の色
		private readonly Color[] COMMENT_COLORS = { Colors.White, Colors.Gray, Colors.Pink, Colors.Red, Colors.Orange, Colors.Yellow,
				Colors.Lime, Colors.Cyan, Colors.Blue, Colors.Purple, Color.FromRgb(0x11, 0x11, 0x11) };

		// ====================================================================
		// private 変数
		// ====================================================================

		// スプラッシュウィンドウ
		private readonly SplashWindowViewModel _splashWindowViewModel;

		// コメント表示ウィンドウ
		private DisplayWindowViewModel _displayWindowViewModel;

		// Yv2Status 変更監視
		private PropertyChangedEventListener _yv2StatusListener;

		// コメント表示中
		private Boolean _isPlaying;

		// コメント色番号
		private Int32 _commentColorIndex;

		// Dispose フラグ
		private Boolean _isDisposed;

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コメント表示開始
		// --------------------------------------------------------------------
		private Task PlayAsync()
		{
			// ウィンドウを前面に出すなど
			_displayWindowViewModel.Messenger.Raise(new InteractionMessage(Yv2Constants.MESSAGE_KEY_WINDOW_ACTIVATE));

			// 開始
			Task task = _displayWindowViewModel.StartAsync();
			SetIsPlaying(true);
			return task;
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
		// _isPlaying の設定
		// --------------------------------------------------------------------
		private void SetIsPlaying(Boolean isPlaying)
		{
			_isPlaying = isPlaying;
			ButtonPlayClickedCommand.RaiseCanExecuteChanged();
			ButtonStopClickedCommand.RaiseCanExecuteChanged();
		}

		// --------------------------------------------------------------------
		// コメント表示停止
		// --------------------------------------------------------------------
		private Task StopAsync()
		{
			Task task = _displayWindowViewModel.StopAsync();
			SetIsPlaying(false);
			return task;
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
