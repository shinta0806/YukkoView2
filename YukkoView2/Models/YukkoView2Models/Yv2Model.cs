// ============================================================================
// 
// ゆっこビュー 2 のロジック本体
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet;

namespace YukkoView2.Models.YukkoView2Models
{
	internal class Yv2Model : NotificationObject
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Yv2Model()
		{
		}

		// ====================================================================
		// static プロパティー
		// ====================================================================

		// 唯一のインスタンス
		public static Yv2Model Instance { get; } = new();

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 環境設定
		public EnvironmentModel EnvModel { get; } = new();
	}
}
