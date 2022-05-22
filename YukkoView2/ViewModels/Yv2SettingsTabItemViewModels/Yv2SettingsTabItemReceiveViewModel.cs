// ============================================================================
// 
// コメント受信タブアイテムの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet.Commands;

using Shinta;
using Shinta.ViewModels;

using System;
using System.Diagnostics;

using YukkoView2.Models.Settings;
using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;
using YukkoView2.ViewModels.MiscWindowViewModels;

namespace YukkoView2.ViewModels.Yv2SettingsTabItemViewModels
{
	internal class Yv2SettingsTabItemReceiveViewModel : TabItemViewModel<Yv2Settings>
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Yv2SettingsTabItemReceiveViewModel(Yv2SettingsWindowViewModel yv2SettingsWindowViewModel)
				: base(yv2SettingsWindowViewModel, Yv2Model.Instance.EnvModel.LogWriter)
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

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region ヘルプリンクの制御
		public static ListenerCommand<String>? HelpClickedCommand
		{
			get => Yv2Model.Instance.EnvModel.HelpClickedCommand;
		}
		#endregion

		#region ゆかり設定ファイル参照ボタンの制御
		private ViewModelCommand? _buttonBrowseYukariConfigPathSeedClickedCommand;

		public ViewModelCommand ButtonBrowseYukariConfigPathSeedClickedCommand
		{
			get
			{
				if (_buttonBrowseYukariConfigPathSeedClickedCommand == null)
				{
					_buttonBrowseYukariConfigPathSeedClickedCommand = new ViewModelCommand(ButtonBrowseYukariConfigPathSeedClicked);
				}
				return _buttonBrowseYukariConfigPathSeedClickedCommand;
			}
		}

		public void ButtonBrowseYukariConfigPathSeedClicked()
		{
			try
			{
				String? path = _tabControlWindowViewModel.PathByOpeningDialog("ゆかり設定ファイル", "ゆかり設定ファイル|" + Yv2Constants.FILE_NAME_YUKARI_CONFIG, Yv2Constants.FILE_NAME_YUKARI_CONFIG);
				if (path != null)
				{
					YukariConfigPathSeed = path;
				}
			}
			catch (Exception excep)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "ゆかり設定ファイル参照ボタンクリック時エラー：\n" + excep.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
		}
		#endregion

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// プロパティーから設定に反映
		// --------------------------------------------------------------------
		public override void PropertiesToSettings(Yv2Settings destSettings)
		{
			// コメントサーバー指定方法
			destSettings.CommentServerType = CommentServerAuto ? CommentServerType.Auto : CommentServerType.Manual;
			destSettings.YukariConfigPathSeed = YukariConfigPathSeed;
			destSettings.ServerUrlSeed = ServerUrlSeed;
			destSettings.RoomNameSeed = RoomNameSeed;

			// コメント受信方法
			destSettings.CommentReceiveType = CommentReceivePush ? CommentReceiveType.Push : CommentReceiveType.Download;
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		public override void SettingsToProperties(Yv2Settings srcSettings)
		{
			// コメントサーバー指定方法
			switch (srcSettings.CommentServerType)
			{
				case CommentServerType.Auto:
					CommentServerAuto = true;
					break;
				case CommentServerType.Manual:
				default:
					CommentServerManual = true;
					break;
			}
			YukariConfigPathSeed = srcSettings.YukariConfigPathSeed;
			ServerUrlSeed = srcSettings.ServerUrlSeed;
			RoomNameSeed = srcSettings.RoomNameSeed;

			// コメント受信方法
			switch (srcSettings.CommentReceiveType)
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
