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
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "連続投稿のため表示しません：" + commentInfo.Message);
				return;
			}
			_prevCommentInfo = commentInfo;

			// 追加
			CommentInfos[commentInfo] = 0;
			Debug.WriteLine("AddCommentInfo() 追加: 残: " + CommentInfos.Count);

			// 描画環境調整
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
				MoveWindowIfNeeded();

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
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "コメント表示ウィンドウ初期化時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
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
			return _receiver.StopAsync();
		}

		// ====================================================================
		// private 変数
		// ====================================================================

		// ウィンドウを表示するディスプレイ
		private Int32 _currentTargetMonitor = -1;

		// 前回追加されたコメント
		private CommentInfo? _prevCommentInfo;

		// コメント受信用
		private Receiver _receiver;

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
			Int32 target = TargetMonitor();
			if (_currentTargetMonitor == target)
			{
				return;
			}

			_currentTargetMonitor = target;
			Rect rect = Yv2Model.Instance.EnvModel.MonitorRects[_currentTargetMonitor];
			Left = rect.Left;
			Top = rect.Top;
			Width = rect.Width;
			Height = rect.Height;
		}

		// --------------------------------------------------------------------
		// 設定と現実を考慮して表示対象のディスプレイ番号を返す
		// --------------------------------------------------------------------
		private Int32 TargetMonitor()
		{
			return 0;
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
