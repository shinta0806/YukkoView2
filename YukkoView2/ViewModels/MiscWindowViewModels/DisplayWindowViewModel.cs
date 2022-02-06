// ============================================================================
// 
// コメント表示ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// コメントコンテナを兼ねる
// コメント表示ウィンドウは常に表示されている仕様とする
// コメント停止中は全透明にする
// ----------------------------------------------------------------------------

using Shinta;
using Shinta.ViewModels;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

using YukkoView2.Models.Receiver;
using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.ViewModels.MiscWindowViewModels
{
	internal class DisplayWindowViewModel : BasicWindowViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public DisplayWindowViewModel()
				: base(Yv2Model.Instance.EnvModel.LogWriter)
		{
			_receiver = new(this);
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// 表示中のコメント群（Int32 はダミー）
		private ConcurrentDictionary<CommentInfo, Int32> _commentInfos = new();
		public ConcurrentDictionary<CommentInfo, Int32> CommentInfos
		{
			get => _commentInfos;
			set => RaisePropertyChangedIfSet(ref _commentInfos, value);
		}

		// コメント表示中
		private Boolean _isPlaying;
		public Boolean IsPlaying
		{
			get => _isPlaying;
			set => RaisePropertyChangedIfSet(ref _isPlaying, value);
		}

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コメントを追加
		// --------------------------------------------------------------------
		public void AddComment(CommentInfo commentInfo)
		{
			// 連続投稿防止
			if (_prevCommentInfo != null && commentInfo.CompareBasic(_prevCommentInfo) && commentInfo.InitialTick - _prevCommentInfo.InitialTick <= Yv2Constants.CONTINUOUS_PREVENT_TIME)
			{
				_logWriter?.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "連続投稿のため表示しません：" + commentInfo.Message);
				return;
			}
			_prevCommentInfo = commentInfo;

			// 追加
			CommentInfos[commentInfo] = 0;
			Debug.WriteLine("AddCommentInfo() 追加: 残: " + CommentInfos.Count);

			// 描画環境調整
			MoveWindowIfNeeded();
			TopMostIfNeeded();
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public override void Initialize()
		{
			base.Initialize();

			try
			{
				// 最前面表示用タイマー
				_timerTopMost.Interval = TimeSpan.FromSeconds(5);
				_timerTopMost.Tick += new EventHandler((s, e) =>
				{
					TopMostIfNeeded();
				});
				_timerTopMost.Start();
			}
			catch (Exception ex)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "コメント表示ウィンドウ初期化時エラー：\n" + ex.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 初期化 2
		// --------------------------------------------------------------------
		public void Initialize2(object sender)
		{
			try
			{
				Window window = (Window)sender;
				_windowHandle = new WindowInteropHelper(window).Handle;
				MoveWindowIfNeeded();
			}
			catch (Exception excep)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "メインウィンドウ初期化 (2) 時エラー：\n" + excep.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// コメントの数
		// --------------------------------------------------------------------
		public Int32 NumComments()
		{
			return CommentInfos.Count;
		}

		// --------------------------------------------------------------------
		// コメント表示開始
		// --------------------------------------------------------------------
		public Task StartAsync()
		{
#if DEBUGz
			Left += 20;
			Top += 20;
			Width -= 40;
			Height -= 40;
#endif
			SlideInitialTicks();
			IsActive = true;
			IsPlaying = true;
			return _receiver.StartAsync();
		}

		// --------------------------------------------------------------------
		// コメント表示停止
		// --------------------------------------------------------------------
		public Task StopAsync()
		{
			IsPlaying = false;
			_stopTick = Environment.TickCount;
			return _receiver.StopAsync();
		}

		// ====================================================================
		// private 変数
		// ====================================================================

		// ウィンドウを表示するディスプレイ
		private Int32 _currentTargetMonitorIndex = -1;

		// 前回追加されたコメント
		private CommentInfo? _prevCommentInfo;

		// コメント受信用
		private Receiver _receiver;

		// コメント表示を停止した時刻
		private Int32 _stopTick;

		// ウィンドウハンドル
		private IntPtr _windowHandle;

		// 最前面表示用タイマー
		private readonly DispatcherTimer _timerTopMost = new();

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 現在の表示対象ディスプレイが適切でなければ移動
		// --------------------------------------------------------------------
		private void MoveWindowIfNeeded()
		{
			if (_windowHandle == IntPtr.Zero)
			{
				return;
			}

			MonitorManager monitorManager = new();
			List<Rect> monitorRects = monitorManager.GetScaledMonitorRects();
			Int32 newTargetMonitorIndex = TargetMonitorIndex(monitorRects);
			if (_currentTargetMonitorIndex == newTargetMonitorIndex)
			{
				return;
			}

			_currentTargetMonitorIndex = newTargetMonitorIndex;
			Rect rect = monitorRects[_currentTargetMonitorIndex];
#if DEBUG
			Debug.WriteLine("MoveWindowIfNeeded() new target: " + newTargetMonitorIndex);
			for (Int32 i = 0; i < monitorRects.Count; i++)
			{
				Rect dr = monitorRects[i];
				_logWriter?.LogMessage(TraceEventType.Verbose, "Scaled ディスプレイ " + i.ToString() + ": " + dr.Left + ", " + dr.Top + ", " + dr.Right + ", " + dr.Bottom);
			}
			List<Rect> rawRects = monitorManager.GetRawMonitorRects();
			for (Int32 i = 0; i < rawRects.Count; i++)
			{
				Rect dr = rawRects[i];
				_logWriter?.LogMessage(TraceEventType.Verbose, "Raw ディスプレイ " + i.ToString() + ": " + dr.Left + ", " + dr.Top + ", " + dr.Right + ", " + dr.Bottom);
			}
#endif

			//WindowsApi.MoveWindow(_windowHandle, (Int32)rect.Left + 1, (Int32)rect.Top + 1, (Int32)rect.Width - 2, (Int32)rect.Height - 2, false);
#if true
			// タスクバーが表示されるように上下左右 1 ピクセルずつ縮める
			Left = rect.Left + 1;
			Top = rect.Top + 1;
			Width = rect.Width - 2;
			Height = rect.Height - 2;
#endif
		}

		// --------------------------------------------------------------------
		// 再開始時にコメントがワープするのを防ぐために InitialTick を調整する
		// --------------------------------------------------------------------
		private void SlideInitialTicks()
		{
			Int32 delta = Environment.TickCount - _stopTick;
			foreach (CommentInfo commentInfo in CommentInfos.Keys)
			{
				commentInfo.InitialTick += delta;
			}
		}

		// --------------------------------------------------------------------
		// 設定と現実を考慮して表示対象のディスプレイ番号を返す
		// --------------------------------------------------------------------
		private Int32 TargetMonitorIndex(List<Rect> monitorRects)
		{
			Int32 index = Yv2Model.Instance.EnvModel.Yv2Settings.SelectMonitorType switch
			{
				SelectMonitorType.MpcBe => TargetMonitorIndexByMpcBe(monitorRects),
				_ => TargetMonitorIndexByManual(),
			};
			index = Math.Clamp(index, 0, monitorRects.Count - 1);
			return index;
		}

		// --------------------------------------------------------------------
		// 指定されたディスプレイの番号
		// --------------------------------------------------------------------
		private Int32 TargetMonitorIndexByManual()
		{
			return Yv2Model.Instance.EnvModel.Yv2Settings.MonitorIndex;
		}

		// --------------------------------------------------------------------
		// MPC-BE が表示されているディスプレイの番号
		// --------------------------------------------------------------------
		private Int32 TargetMonitorIndexByMpcBe(List<Rect> monitorRects)
		{
			// MPC-BE 64 ビット版のプロセス
			Process[] processes = Process.GetProcessesByName(Yv2Model.Instance.EnvModel.Yv2Settings.MpcBe64ProcessName);
			if (processes.Length == 0)
			{
				// 64 ビット版が見つからない場合は 32 ビット版で探す
				processes = Process.GetProcessesByName(Yv2Model.Instance.EnvModel.Yv2Settings.MpcBe32ProcessName);
			}
			if (processes.Length == 0)
			{
				// MPC-BE が見つからない場合は現在のディスプレイ番号を返す
				return _currentTargetMonitorIndex;
			}

			// MPC-BE のウィンドウ領域を取得
			Boolean rectGot = false;
			WindowsApi.RECT mpcRect = new();
			foreach (Process process in processes)
			{
				if (WindowsApi.GetWindowRect(process.MainWindowHandle, out mpcRect))
				{
					rectGot = true;
					break;
				}
			}
			if (!rectGot)
			{
				// MPC-BE のウィンドウ領域を取得できない場合は現在のディスプレイ番号を返す
				return _currentTargetMonitorIndex;
			}
			Double mpcCenterX = (mpcRect.left + mpcRect.right) / 2;
			Double mpcCenterY = (mpcRect.top + mpcRect.bottom) / 2;

			// MPC-BE の中央を含むディスプレイを探す
			for (Int32 i = 0; i < monitorRects.Count; i++)
			{
				if (monitorRects[i].Contains(mpcCenterX, mpcCenterY))
				{
					Debug.WriteLine("TargetMonitorIndexByMpcBe() MPC 発見: " + i);
					return i;
				}
			}

			// MPC-BE の中央を含むディスプレイが無い場合は現在のディスプレイ番号を返す
			return _currentTargetMonitorIndex;
		}

		// --------------------------------------------------------------------
		// ウィンドウを最前面に出す（必要に応じて）
		// --------------------------------------------------------------------
		private void TopMostIfNeeded()
		{
			if (!IsPlaying)
			{
				return;
			}
			if (CommentInfos.Count == 0)
			{
				return;
			}

			// MPC-BE のフルスクリーンは通常の最大化とは異なり、このフォームの TopMost が効かない
			// MPC-BE が最前面に来ている時は、このフォームを最前面に持って行く必要がある
			// GetForegroundWindow() はマルチディスプレイに関係なく最前面を報告するので、
			// ディスプレイ 2 の最前面が MPC-BE でも、ディスプレイ 1 に他のアプリがあってそれが最前面ならそれを報告する
			// 従って、MPC-BE が当該ディスプレイで最前面かどうか判定不能
			// そこで、このフォームが最前面でない場合は常に最前面に出すことにする
			// タスクスイッチなどシステム系が最前面になっている場合にこのフォームを最前面にしても、今のところ問題ない模様
			IsActive = true;
		}
	}
}
