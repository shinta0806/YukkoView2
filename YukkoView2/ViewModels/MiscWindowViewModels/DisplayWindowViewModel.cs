﻿// ============================================================================
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
using System.Windows;
using System.Windows.Interop;
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
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "コメント表示ウィンドウ初期化時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// 初期化 2
		// Handle 取得用
		// --------------------------------------------------------------------
		public void Initialize2(object sender)
		{
			try
			{
				Window window = (Window)sender;
				_windowHandle = new WindowInteropHelper(window).Handle;
			}
			catch (Exception excep)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "メインウィンドウ初期化 (2) 時エラー：\n" + excep.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
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

		// ウィンドウハンドル
		IntPtr _windowHandle;

		// ウィンドウを表示するディスプレイ
		private Int32 _currentTargetMonitor = -1;

		// 表示中のコメント群
		// 複数スレッドからアクセスされる想定のため、アクセス時はロックが必要
		private HashSet<CommentInfo> _commentInfoSet = new();

		// 前回追加されたコメント
		private CommentInfo? _prevCommentInfo;

		// コメント受信用
		private Receiver _receiver;

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
			lock (_commentInfoSet)
			{
				if (_commentInfoSet.Count == 0)
				{
					return;
				}
			}

			// MPC-BE のフルスクリーンは通常の最大化とは異なり、このフォームの TopMost が効かない
			// MPC-BE が最前面に来ている時は、このフォームを最前面に持って行く必要がある
			// GetForegroundWindow() はマルチディスプレイに関係なく最前面を報告するので、
			// ディスプレイ 2 の最前面が MPC-BE でも、ディスプレイ 1 に他のアプリがあってそれが最前面ならそれを報告する
			// 従って、MPC-BE が当該ディスプレイで最前面かどうか判定不能
			// そこで、このフォームが最前面でない場合は常に最前面に出すことにする
			// タスクスイッチなどシステム系が最前面になっている場合にこのフォームを最前面にしても、今のところ問題ない模様
			IntPtr fgHandle = WindowsApi.GetForegroundWindow();
			if (fgHandle != _windowHandle)
			{
				var a = WindowsApi.BringWindowToTop(_windowHandle);
				Debug.WriteLine("TopMostIfNeeded() " + a);
			}
		}


	}
}
