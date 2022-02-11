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

using Livet;

using Shinta;
using Shinta.ViewModels;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
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

		// 常に手前に表示
		private Boolean _topMost = true;
		public Boolean TopMost
		{
			get => _topMost;
			set => RaisePropertyChangedIfSet(ref _topMost, value);
		}

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

		// メインウィンドウがアクティブかどうか
		private Boolean _isMainWindowActive;
		public Boolean IsMainWindowActive
		{
			get => _isMainWindowActive;
			set => RaisePropertyChangedIfSet(ref _isMainWindowActive, value);
		}

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コメントを追加
		// --------------------------------------------------------------------
		public void AddComment(CommentInfo commentInfo)
		{
			// UI スレッドで追加しないと、再生中にウィンドウを移動する際に、ディスプレイ拡大率が等倍で無い場合にウィンドウサイズがおかしくなる（原因は不明）
			DispatcherHelper.UIDispatcher.Invoke(() =>
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
			});
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
			if (IsPlaying)
			{
				return Task.CompletedTask;
			}

			MoveWindowIfNeeded();
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
			if (!IsPlaying)
			{
				return Task.CompletedTask;
			}

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
		private readonly Receiver _receiver;

		// コメント表示を停止した時刻
		private Int32 _stopTick;

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
			MonitorManager monitorManager = new();
			List<Rect> scaledMonitorRects = monitorManager.GetScaledMonitorRects();
			Int32 newTargetMonitorIndex = TargetMonitorIndex(scaledMonitorRects);
			if (_currentTargetMonitorIndex == newTargetMonitorIndex)
			{
				return;
			}

			_currentTargetMonitorIndex = newTargetMonitorIndex;
			Rect rect = scaledMonitorRects[_currentTargetMonitorIndex];

			// タスクバーが表示されるように上下左右 1 ピクセルずつ縮める
			Left = rect.Left + 1;
			Top = rect.Top + 1;
			Width = rect.Width - 2;
			Height = rect.Height - 2;
			_logWriter?.LogMessage(TraceEventType.Verbose, "MoveWindowIfNeeded() " + Left + ", " + Top + ", " + Width + ", " + Height);
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
		private static Int32 TargetMonitorIndexByManual()
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
			if (IsMainWindowActive)
			{
				return;
			}
			if (CommentInfos.IsEmpty)
			{
				return;
			}

			// いったん TopMost をオフにしてから再度オンにする（チケット #15 対策）
			TopMost = false;
			TopMost = true;

			// MPC-BE のフルスクリーンは通常の最大化とは異なり、このウィンドウの TopMost が効かない
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
