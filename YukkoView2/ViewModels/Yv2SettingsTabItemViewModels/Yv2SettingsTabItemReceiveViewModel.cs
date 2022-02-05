// ============================================================================
// 
// コメント受信タブアイテムの ViewModel
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.ViewModels.Yv2SettingsTabItemViewModels
{
	internal class Yv2SettingsTabItemReceiveViewModel : TabItemViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Yv2SettingsTabItemReceiveViewModel(Yv2ViewModel windowViewModel)
				: base(windowViewModel)
		{
		}

		// --------------------------------------------------------------------
		// ダミーコンストラクター（Visual Studio・TransitionMessage 用）
		// --------------------------------------------------------------------
		public Yv2SettingsTabItemReceiveViewModel()
				: base()
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// コメント受信方法：プッシュ通知
		private Boolean _commentReceivePush;
		public Boolean CommentReceivePush
		{
			get => _commentReceivePush;
			set => RaisePropertyChangedIfSet(ref _commentReceivePush, value);
		}

		// コメント受信方法：ダウンロード
		private Boolean _commentReceiveDownload;
		public Boolean CommentReceiveDownload
		{
			get => _commentReceiveDownload;
			set => RaisePropertyChangedIfSet(ref _commentReceiveDownload, value);
		}

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// プロパティーから設定に反映
		// --------------------------------------------------------------------
		public override void PropertiesToSettings()
		{
			Yv2Model.Instance.EnvModel.Yv2Settings.CommentReceiveType = CommentReceivePush ? CommentReceiveType.Push : CommentReceiveType.Download;
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		public override void SettingsToProperties()
		{
			switch (Yv2Model.Instance.EnvModel.Yv2Settings.CommentReceiveType)
			{
				case CommentReceiveType.Push:
					CommentReceivePush = true;
					break;
				case CommentReceiveType.Download:
					CommentReceiveDownload = true;
					break;
				default:
					break;
			}
		}
	}
}
