// ============================================================================
// 
// ゆっこビュー 2 共通で使用する関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Threading.Tasks;

using YukkoView2.Models.Settings;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.Models.SharedMisc
{
	internal class Yv2Common
	{
		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 最新情報の確認
		// --------------------------------------------------------------------
		public static async Task CheckLatestInfoAsync(Boolean forceShow)
		{
			LatestInfoManager latestInfoManager = Yv2Common.CreateLatestInfoManager(forceShow);
			if (await latestInfoManager.CheckAsync())
			{
				ExitSettings exitSettings = new();
				exitSettings.Load();
				exitSettings.RssCheckDate = DateTime.Now.Date;
				exitSettings.Save();
			}
		}

		// --------------------------------------------------------------------
		// 最新情報管理者を作成
		// --------------------------------------------------------------------
		public static LatestInfoManager CreateLatestInfoManager(Boolean forceShow)
		{
			return new LatestInfoManager("http://shinta.coresv.com/soft/YukkoView2_JPN.xml", forceShow, 3, Yv2Constants.APP_VER,
					Yv2Model.Instance.EnvModel.AppCancellationTokenSource.Token, Yv2Model.Instance.EnvModel.LogWriter);
		}

		// --------------------------------------------------------------------
		// 環境情報をログする
		// --------------------------------------------------------------------
		public static void LogEnvironmentInfo()
		{
			SystemEnvironment se = new();
			se.LogEnvironment(Yv2Model.Instance.EnvModel.LogWriter);
		}
	}
}
