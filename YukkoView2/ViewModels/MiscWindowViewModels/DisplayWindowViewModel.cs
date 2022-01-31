// ============================================================================
// 
// コメント表示ウィンドウの ViewModel
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YukkoView2.Models.Receiver;
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

		}

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public override async void Initialize()
		{
			base.Initialize();

			try
			{
				await StartAsync();
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
		public async Task StartAsync()
		{
			Receiver receiver = new();
			await receiver.ReceiveLoopAsync();
		}
	}
}
