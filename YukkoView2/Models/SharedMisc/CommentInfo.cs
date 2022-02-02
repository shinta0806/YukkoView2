// ============================================================================
// 
// コメントの内容
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace YukkoView2.Models.SharedMisc
{
	internal class CommentInfo
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public CommentInfo(String message, Int32 yukariSize, Color color)
		{
			Message = message;
			YukariSize = yukariSize;
			Color = color;

			InitialTick = Environment.TickCount;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// 基本情報
		// --------------------------------------------------------------------

		// コメント内容
		public String Message { get; set; }

		// サイズ（ゆかり指定サイズ）
		// ゆかりでは「小=0、中=3、大=6、特大=9」
		// プログラムの都合上、小=1 とする
		private Int32 _yukariSize;
		public Int32 YukariSize
		{
			get => _yukariSize;
			set
			{
				_yukariSize = Math.Clamp(value, 1, 9);
			}
		}

		// 色
		public Color Color { get; set; }

		// コメントを取得した時刻
		public Int32 InitialTick { get; set; }

		// --------------------------------------------------------------------
		// ビューア側で計算する情報
		// --------------------------------------------------------------------

		// 移動速度 [px/s]
		public Int32 Speed { get; set; }

		// 描画指定位置
		public Int32 SpecifyLeft { get; set; }

		// --------------------------------------------------------------------
		// 描画情報
		// --------------------------------------------------------------------

		// 描画用データ設定済かどうか
		public Boolean IsDrawDataSet { get; set; }

		// 描画用データ
		public FormattedText? FormattedText { get; set; }

#if false
		// コメント描画用グラフィックスパス
		public GraphicsPath MessagePath
		{
			get
			{
				return mMessagePath;
			}
			set
			{
				if (mMessagePath != null)
				{
					mMessagePath.Dispose();
				}
				mMessagePath = value;
			}
		}
#endif

		// ブラシ
		//public SolidColorBrush Brush { get; set; }

#if false
		// 表示される位置（縁取り込み）
		// ※位置指定は MessagePath の Transform で行う
		public Int32 Top
		{
			get
			{
				return (Int32)(mMessagePath.GetBounds().Top - YukkoViewCommon.EDGE_WIDTH / 2);
			}
		}

		public Int32 Right
		{
			get
			{
				return (Int32)(mMessagePath.GetBounds().Right + YukkoViewCommon.EDGE_WIDTH / 2);
			}
		}

		public Int32 Bottom
		{
			get
			{
				return (Int32)(mMessagePath.GetBounds().Bottom + YukkoViewCommon.EDGE_WIDTH / 2);
			}
		}

		// 表示されるサイズ（縁取り込み）
		public Int32 Width
		{
			get
			{
				return (Int32)(mMessagePath.GetBounds().Width + YukkoViewCommon.EDGE_WIDTH);
			}
		}

		public Int32 Height
		{
			get
			{
				return (Int32)(mMessagePath.GetBounds().Height + YukkoViewCommon.EDGE_WIDTH);
			}
		}
#endif

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 基本情報の比較
		// ＜返値＞ 基本情報が全て等しければ true
		// --------------------------------------------------------------------
		public Boolean CompareBasic(CommentInfo comp)
		{
			return Message == comp.Message
					&& YukariSize == comp.YukariSize
					&& Color == comp.Color;
		}

		// --------------------------------------------------------------------
		// 後始末
		// --------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 後始末
		// --------------------------------------------------------------------
		protected virtual void Dispose(bool oDisposing)
		{
		}

		// ====================================================================
		// private メンバー変数
		// ====================================================================

		// MessagePath
		//private GraphicsPath mMessagePath;

	}
}
