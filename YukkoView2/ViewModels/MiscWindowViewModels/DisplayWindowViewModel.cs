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
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Shinta;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YukkoView2.Models.Receiver;
using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.ViewModels.MiscWindowViewModels
{
	internal class DisplayWindowViewModel : Yv2ViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public DisplayWindowViewModel()
		{
			_receiver = new(this);
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

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

			//TopMostIfNeeded();

			// 描画情報設定
			//SetCommentMessagePath(commentInfo);
			//SetCommentBrush(commentInfo);
			//CalcSpeed(commentInfo);

			// 位置設定（描画情報設定後に実行）
			// 文字を描画する際、X 位置ぴったりよりも少し右に描画されるので、少し左目に初期位置を設定する
#if false
			Int32 aX = CalcCommentLeft(commentInfo);
			Int32 aY = CalcCommentTop(commentInfo);
			MoveComment(commentInfo, aX, aY);
			mLogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "コメントを表示します。初期位置：" + aX + ", " + aY + ", 幅：" + commentInfo.Width
					+ ", 速度：" + commentInfo.Speed);
#endif

			// 追加
			lock (_commentInfoSet)
			{
				_commentInfoSet.Add(commentInfo);
				Debug.WriteLine("AddCommentInfo() 追加: 残: " + _commentInfoSet.Count);
			}
		}

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public override void Initialize()
		{
			base.Initialize();

			try
			{
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "コメント表示ウィンドウ初期化時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// コメント表示開始
		// --------------------------------------------------------------------
		public Task StartAsync()
		{
			IsPlaying = true;
			return _receiver.StartAsync();
		}

		// --------------------------------------------------------------------
		// コメント表示停止
		// --------------------------------------------------------------------
		public void Stop()
		{
			IsPlaying = false;
			_receiver.Stop();
		}

		// ====================================================================
		// private 変数
		// ====================================================================

		// 表示中のコメント群
		// 複数スレッドからアクセスされる想定のため、アクセス時はロックが必要
		private HashSet<CommentInfo> _commentInfoSet = new();

		// 前回追加されたコメント
		private CommentInfo? _prevCommentInfo;

		// コメント受信用
		private Receiver _receiver;


	}
}
