// ============================================================================
// 
// ゆかり request.db のコンテキスト
// 
// ============================================================================

// ----------------------------------------------------------------------------
// データベースプロパティーテーブルが存在しないため YukaListerContext の派生にはしない
// ----------------------------------------------------------------------------

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using System.Diagnostics;

using YukkoView2.Models.Database;
using YukkoView2.Models.DatabaseAssist;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.Models.DatabaseContexts
{
	internal class YukariRequestContext : DbContext
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public YukariRequestContext()
		{
			Debug.Assert(YukariRequests != null, "YukariRequests table not init");
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ゆかり予約テーブル
		public DbSet<TYukariRequest> YukariRequests { get; set; }

		// ====================================================================
		// protected 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// データベース設定
		// --------------------------------------------------------------------
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			using SqliteConnection sqliteConnection = DbCommon.Connect(Yv2Model.Instance.EnvModel.Yv2Settings.YukariRequestDatabasePath());
			optionsBuilder.UseSqlite(sqliteConnection);
		}

		// --------------------------------------------------------------------
		// データベースモデル作成
		// --------------------------------------------------------------------
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
		}
	}
}
