// ============================================================================
// 
// バージョン情報ウィンドウの ViewModel
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

using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.ViewModels.MiscWindowViewModels
{
	internal class AboutWindowViewModel : BasicWindowViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public AboutWindowViewModel()
				: base(Yv2Model.Instance.EnvModel.LogWriter)
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region リンククリックの制御
		private ListenerCommand<String>? _linkClickedCommand;

		public ListenerCommand<String> LinkClickedCommand
		{
			get
			{
				if (_linkClickedCommand == null)
				{
					_linkClickedCommand = new ListenerCommand<String>(LinkClicked);
				}
				return _linkClickedCommand;
			}
		}

		public void LinkClicked(String parameter)
		{
			try
			{
				Common.ShellExecute(parameter);
			}
			catch (Exception excep)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "リンククリック時エラー：\n" + excep.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
		}
		#endregion

		#region 更新プログラムの確認ボタンの制御
		private ViewModelCommand? _buttonCheckUpdateClickedCommand;

		public ViewModelCommand ButtonCheckUpdateClickedCommand
		{
			get
			{
				if (_buttonCheckUpdateClickedCommand == null)
				{
					_buttonCheckUpdateClickedCommand = new ViewModelCommand(ButtonCheckUpdateClicked);
				}
				return _buttonCheckUpdateClickedCommand;
			}
		}

		public void ButtonCheckUpdateClicked()
		{
			try
			{
				Common.OpenMicrosoftStore(Yv2Constants.STORE_PRODUCT_ID);
			}
			catch (Exception ex)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "更新プログラムの確認ボタンクリック時エラー：\n" + ex.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}
		#endregion

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
				// 表示
				Title = Yv2Constants.APP_NAME_J + " のバージョン情報";
			}
			catch (Exception excep)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "バージョン情報ウィンドウ初期化時エラー：\n" + excep.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
		}
	}
}
