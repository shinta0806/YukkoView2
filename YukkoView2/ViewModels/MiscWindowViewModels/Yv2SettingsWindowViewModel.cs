// ============================================================================
// 
// 環境設定ウィンドウの ViewModel
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
using System.Windows;
using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;
using YukkoView2.ViewModels.Yv2SettingsTabItemViewModels;

namespace YukkoView2.ViewModels.MiscWindowViewModels
{
	internal class Yv2SettingsWindowViewModel : Yv2ViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Yv2SettingsWindowViewModel()
		{
			// タブアイテムの ViewModel 初期化
			_yv2SettingsTabItemSettingsViewModel = new Yv2SettingsTabItemSettingsViewModel(this);
			_yv2SettingsTabItemViewModels = new TabItemViewModel[]
			{
				_yv2SettingsTabItemSettingsViewModel,
			};
			Debug.Assert(_yv2SettingsTabItemViewModels.Length == (Int32)Yv2SettingsTabItem.__End__, "Yv2SettingsWindowViewModel() bad tab vm nums");
			for (Int32 i = 0; i < _yv2SettingsTabItemViewModels.Length; i++)
			{
				CompositeDisposable.Add(_yv2SettingsTabItemViewModels[i]);
			}
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// タブアイテム：設定
		private Yv2SettingsTabItemSettingsViewModel _yv2SettingsTabItemSettingsViewModel;
		public Yv2SettingsTabItemSettingsViewModel Yv2SettingsTabItemSettingsViewModel
		{
			get => _yv2SettingsTabItemSettingsViewModel;
			set => RaisePropertyChangedIfSet(ref _yv2SettingsTabItemSettingsViewModel, value);
		}

		// OK ボタンフォーカス
		private Boolean _isButtonOkFocused;
		public Boolean IsButtonOkFocused
		{
			get => _isButtonOkFocused;
			set
			{
				// 再度フォーカスを当てられるように強制伝播
				_isButtonOkFocused = value;
				RaisePropertyChanged(nameof(IsButtonOkFocused));
			}
		}

		#region OK ボタンの制御
		private ViewModelCommand? _buttonOkClickedCommand;

		public ViewModelCommand ButtonOkClickedCommand
		{
			get
			{
				if (_buttonOkClickedCommand == null)
				{
					_buttonOkClickedCommand = new ViewModelCommand(ButtonOkClicked);
				}
				return _buttonOkClickedCommand;
			}
		}

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		public void ButtonOkClicked()
		{
			try
			{
				// Enter キーでボタンが押された場合はテキストボックスからフォーカスが移らずプロパティーが更新されないため強制フォーカス
				IsButtonOkFocused = true;

				CheckInput();
				PropertiesToSettings();
				Yv2Model.Instance.EnvModel.Yv2Settings.Save();
				Result = MessageBoxResult.OK;
				Messenger.Raise(new WindowActionMessage(Yv2Constants.MESSAGE_KEY_WINDOW_CLOSE));
			}
			catch (Exception excep)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "OK ボタンクリック時エラー：\n" + excep.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
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
				// タイトルバー
				Title = "環境設定";

				for (Int32 i = 0; i < _yv2SettingsTabItemViewModels.Length; i++)
				{
					_yv2SettingsTabItemViewModels[i].Initialize();
				}

				SettingsToProperties();
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ウィンドウ初期化時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// ====================================================================
		// private 変数
		// ====================================================================

		// タブアイテムの ViewModel
		private readonly TabItemViewModel[] _yv2SettingsTabItemViewModels;

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 入力された値が適正か確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckInput()
		{
			for (Int32 i = 0; i < _yv2SettingsTabItemViewModels.Length; i++)
			{
				_yv2SettingsTabItemViewModels[i].CheckInput();
			}
		}

		// --------------------------------------------------------------------
		// プロパティーから設定に反映
		// --------------------------------------------------------------------
		private void PropertiesToSettings()
		{
			for (Int32 i = 0; i < _yv2SettingsTabItemViewModels.Length; i++)
			{
				_yv2SettingsTabItemViewModels[i].PropertiesToSettings();
			}
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		private void SettingsToProperties()
		{
			for (Int32 i = 0; i < _yv2SettingsTabItemViewModels.Length; i++)
			{
				_yv2SettingsTabItemViewModels[i].SettingsToProperties();
			}
		}
	}
}
