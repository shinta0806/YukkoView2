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
using Shinta.ViewModels;

using System;
using System.Collections.Specialized;
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
	internal class MainWindowViewModel : BasicWindowViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public MainWindowViewModel(SplashWindowViewModel splashWindowViewModel)
				: base(Yv2Model.Instance.EnvModel.LogWriter)
		{
			// スプラッシュウィンドウ
			_splashWindowViewModel = splashWindowViewModel;

			// コメント表示ウィンドウ
			_displayWindowViewModel = new();
			CompositeDisposable.Add(_displayWindowViewModel);

			// Yv2StatusErrorFactors 変更監視
			_yv2StatusErrorFactorsListener = new(Yv2Model.Instance.EnvModel.Yv2StatusErrorFactors, Yv2StatusErrorFactorsChanged);
			CompositeDisposable.Add(_yv2StatusErrorFactorsListener);
		}

		// --------------------------------------------------------------------
		// ダミーコンストラクター（Visual Studio・TransitionMessage 用）
		// --------------------------------------------------------------------
		public MainWindowViewModel()
		{
			// 警告抑止用にメンバーを null! で初期化
			_splashWindowViewModel = null!;
			_displayWindowViewModel = null!;
			_yv2StatusErrorFactorsListener = null!;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// ゆっこビュー 2 の動作状況
		private String _yv2StatusMessage = String.Empty;
		public String Yv2StatusMessage
		{
			get => _yv2StatusMessage;
			set => RaisePropertyChangedIfSet(ref _yv2StatusMessage, value);
		}

		// ゆっこビュー 2 の動作状況の背景
		private Brush _yv2StatusBackground = Yv2Constants.BRUSH_STATUS_RUNNING;
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

		#region 環境設定ボタンの制御
		private ViewModelCommand? _buttonYv2SettingsClickedCommand;

		public ViewModelCommand ButtonYv2SettingsClickedCommand
		{
			get
			{
				if (_buttonYv2SettingsClickedCommand == null)
				{
					_buttonYv2SettingsClickedCommand = new ViewModelCommand(ButtonYv2SettingsClicked);
				}
				return _buttonYv2SettingsClickedCommand;
			}
		}

		public void ButtonYv2SettingsClicked()
		{
			try
			{
				// ViewModel 経由でウィンドウを開く
				using Yv2SettingsWindowViewModel yv2SettingsWindowViewModel = new();
				Messenger.Raise(new TransitionMessage(yv2SettingsWindowViewModel, Yv2Constants.MESSAGE_KEY_OPEN_YV2_SETTINGS_WINDOW));
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ボタンクリック時エラー：\n" + ex.Message);
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

		public void ButtonCommentClicked()
		{
			try
			{
				if (String.IsNullOrEmpty(Comment))
				{
					return;
				}

				CommentInfo commentInfo = new(Comment, Yv2Constants.DEFAULT_YUKARI_FONT_SIZE, COMMENT_COLORS[_commentColorIndex]);

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

				// プログラムエラーチェック
				Debug.Assert(Yv2Constants.ERROR_FACTOR_MESSAGE.Length == (Int32)Yv2StatusErrorFactor.__End__, "MainWindow.Initialize() bad ERROR_FACTOR_MESSAGE length");

				// 環境の変化に対応
				DoVerChangedIfNeeded();

				// コメント表示ウィンドウを開く
				Messenger.Raise(new TransitionMessage(_displayWindowViewModel, Yv2Constants.MESSAGE_KEY_OPEN_DISPLAY_WINDOW));

				// コメント表示開始
				if (Yv2Model.Instance.EnvModel.Yv2Settings.PlayOnStart)
				{
					_ = PlayAsync();
				}
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

		// Yv2StatusErrorFactors 変更監視
		private CollectionChangedEventListener _yv2StatusErrorFactorsListener;

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
		// バージョン更新時の処理
		// --------------------------------------------------------------------
		private void DoVerChangedIfNeeded()
		{
			// 更新起動時とパス変更時の記録
			// 新規起動時は、両フラグが立つのでダブらないように注意
			String prevLaunchVer = Yv2Model.Instance.EnvModel.Yv2Settings.PrevLaunchVer;
			Boolean verChanged = prevLaunchVer != Yv2Constants.APP_VER;
			if (verChanged)
			{
				// ユーザーにメッセージ表示する前にログしておく
				if (String.IsNullOrEmpty(prevLaunchVer))
				{
					Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Information, "新規起動：" + Yv2Constants.APP_VER);
				}
				else
				{
					Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Information, "更新起動：" + prevLaunchVer + "→" + Yv2Constants.APP_VER);
				}
			}
			String prevLaunchPath = Yv2Model.Instance.EnvModel.Yv2Settings.PrevLaunchPath;
			Boolean pathChanged = (String.Compare(prevLaunchPath, Yv2Model.Instance.EnvModel.ExeFullPath, true) != 0);
			if (pathChanged && !String.IsNullOrEmpty(prevLaunchPath))
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Information, "パス変更起動：" + prevLaunchPath + "→" + Yv2Model.Instance.EnvModel.ExeFullPath);
			}

			// 更新起動時とパス変更時の処理
			if (verChanged || pathChanged)
			{
				Yv2Common.LogEnvironmentInfo();
			}
			if (verChanged)
			{
				NewVersionLaunched();
			}
		}

		// --------------------------------------------------------------------
		// 新バージョンで初回起動された時の処理を行う
		// --------------------------------------------------------------------
		private void NewVersionLaunched()
		{
			String newVerMsg;
			TraceEventType type = TraceEventType.Information;

			// α・β警告、ならびに、更新時のメッセージ（2022/01/23）
			// 新規・更新のご挨拶
			if (String.IsNullOrEmpty(Yv2Model.Instance.EnvModel.Yv2Settings.PrevLaunchVer))
			{
				// 新規
				newVerMsg = "【初回起動】\n\n";
				newVerMsg += Yv2Constants.APP_NAME_J + "をダウンロードしていただき、ありがとうございます。";
			}
			else
			{
				// 更新
				newVerMsg = "【更新起動】\n\n";
				newVerMsg += Yv2Constants.APP_NAME_J + "が更新されました。\n";
				newVerMsg += "更新内容については［ヘルプ→改訂履歴］メニューをご参照ください。";
			}

			// α・βの注意
			if (Yv2Constants.APP_VER.Contains('α'))
			{
				newVerMsg += "\n\nこのバージョンは開発途上のアルファバージョンです。\n"
						+ "使用前にヘルプをよく読み、注意してお使い下さい。";
				type = TraceEventType.Warning;
			}
			else if (Yv2Constants.APP_VER.Contains('β'))
			{
				newVerMsg += "\n\nこのバージョンは開発途上のベータバージョンです。\n"
						+ "使用前にヘルプをよく読み、注意してお使い下さい。";
				type = TraceEventType.Warning;
			}

			// 表示
			Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(type, newVerMsg);
			SaveExitStatus();

#if !DISTRIB_STORE
			// Zone ID 削除
			CommonWindows.DeleteZoneID(YlModel.Instance.EnvModel.ExeFullFolder, SearchOption.AllDirectories);

			// パスの注意
			String? installMsg = InstallWarningMessage();
			if (!String.IsNullOrEmpty(installMsg))
			{
				YlModel.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Warning, installMsg);
			}
#endif
		}

		// --------------------------------------------------------------------
		// コメント表示開始
		// --------------------------------------------------------------------
		private Task PlayAsync()
		{
			// 開始
			Task task = _displayWindowViewModel.StartAsync();
			SetIsPlaying(true);

			// 開始コメント投稿
			CommentInfo commentInfo = new("コメント表示を開始します", Yv2Constants.DEFAULT_YUKARI_FONT_SIZE, Colors.White);
			_displayWindowViewModel.AddComment(commentInfo);

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
			UpdateYv2Status();
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
		// ゆっこビュー 2 の動作状況表示を更新
		// --------------------------------------------------------------------
		private void UpdateYv2Status()
		{
			Int32 errorIndex = Yv2Model.Instance.EnvModel.Yv2StatusErrorFactors.IndexOf(true);
			if (errorIndex >= 0)
			{
				// エラーがある場合はエラー表示
				Yv2StatusMessage = Yv2Constants.ERROR_FACTOR_MESSAGE[errorIndex];
				Yv2StatusBackground = Yv2Constants.BRUSH_STATUS_ERROR;
			}
			else
			{
				if (_isPlaying)
				{
					Yv2StatusMessage = "コメント表示中";
					Yv2StatusBackground = Yv2Constants.BRUSH_STATUS_RUNNING;
				}
				else
				{
					Yv2StatusMessage = "コメント表示停止中";
					Yv2StatusBackground = Yv2Constants.BRUSH_STATUS_DONE;
				}
			}
		}

		// --------------------------------------------------------------------
		// Yv2Model.Instance.EnvModel.Yv2StatusErrorFactors が変更された
		// --------------------------------------------------------------------
		private void Yv2StatusErrorFactorsChanged(Object? sender, NotifyCollectionChangedEventArgs e)
		{
			Debug.WriteLine("Yv2StatusChanged()");
			UpdateYv2Status();
		}
	}
}
