// ============================================================================
// 
// コメント表示ディスプレイ選択ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Shinta;
using Shinta.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.ViewModels.MiscWindowViewModels
{
	internal class SelectMonitorWindowViewModel : BasicWindowViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public SelectMonitorWindowViewModel()
				: base(Yv2Model.Instance.EnvModel.LogWriter)
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

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
				Title = "コメント表示ディスプレイ選択";
			}
			catch (Exception ex)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "コメント表示ディスプレイ選択ウィンドウ初期化時エラー：\n" + ex.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}
	}
}
