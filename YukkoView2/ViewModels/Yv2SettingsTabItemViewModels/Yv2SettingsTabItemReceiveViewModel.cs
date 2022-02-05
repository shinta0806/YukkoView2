﻿// ============================================================================
// 
// コメント受信タブアイテムの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta.ViewModels;

using System;

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
		public Yv2SettingsTabItemReceiveViewModel(TabControlWindowViewModel tabControlWindowViewModel)
				: base(tabControlWindowViewModel, Yv2Model.Instance.EnvModel.LogWriter)
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

		// コメントサーバー指定方法：自動
		private Boolean _commentServerAuto;
		public Boolean CommentServerAuto
		{
			get => _commentServerAuto;
			set => RaisePropertyChangedIfSet(ref _commentServerAuto, value);
		}

		// コメントサーバー指定方法：自動
		// ゆかり設定ファイルのパス（相対または絶対）
		private String _yukariConfigPathSeed = String.Empty;
		public String YukariConfigPathSeed
		{
			get => _yukariConfigPathSeed;
			set => RaisePropertyChangedIfSet(ref _yukariConfigPathSeed, value);
		}

		// コメントサーバー指定方法：手動
		private Boolean _commentServerManual;
		public Boolean CommentServerManual
		{
			get => _commentServerManual;
			set => RaisePropertyChangedIfSet(ref _commentServerManual, value);
		}

		// コメントサーバー指定方法：手動
		// コメントサーバーの URL
		private String _serverUrlSeed = String.Empty;
		public String ServerUrlSeed
		{
			get => _serverUrlSeed;
			set => RaisePropertyChangedIfSet(ref _serverUrlSeed, value);
		}

		// コメントサーバー指定方法：手動
		// ルーム名
		private String _roomNameSeed = String.Empty;
		public String RoomNameSeed
		{
			get => _roomNameSeed;
			set => RaisePropertyChangedIfSet(ref _roomNameSeed, value);
		}

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
			// コメントサーバー指定方法
			Yv2Model.Instance.EnvModel.Yv2Settings.CommentServerType = CommentServerAuto ? CommentServerType.Auto : CommentServerType.Manual;
			Yv2Model.Instance.EnvModel.Yv2Settings.YukariConfigPathSeed = YukariConfigPathSeed;
			Yv2Model.Instance.EnvModel.Yv2Settings.ServerUrlSeed = ServerUrlSeed;
			Yv2Model.Instance.EnvModel.Yv2Settings.RoomNameSeed = RoomNameSeed;

			// コメント受信方法
			Yv2Model.Instance.EnvModel.Yv2Settings.CommentReceiveType = CommentReceivePush ? CommentReceiveType.Push : CommentReceiveType.Download;
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		public override void SettingsToProperties()
		{
			// コメントサーバー指定方法
			switch (Yv2Model.Instance.EnvModel.Yv2Settings.CommentServerType)
			{
				case CommentServerType.Auto:
					CommentServerAuto = true;
					break;
				case CommentServerType.Manual:
				default:
					CommentServerManual = true;
					break;
			}
			YukariConfigPathSeed = Yv2Model.Instance.EnvModel.Yv2Settings.YukariConfigPathSeed;
			ServerUrlSeed = Yv2Model.Instance.EnvModel.Yv2Settings.ServerUrlSeed;
			RoomNameSeed = Yv2Model.Instance.EnvModel.Yv2Settings.RoomNameSeed;

			// コメント受信方法
			switch (Yv2Model.Instance.EnvModel.Yv2Settings.CommentReceiveType)
			{
				case CommentReceiveType.Push:
					CommentReceivePush = true;
					break;
				case CommentReceiveType.Download:
				default:
					CommentReceiveDownload = true;
					break;
			}
		}
	}
}