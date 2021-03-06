// ============================================================================
// 
// コメントの内容
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Windows.Media;

namespace YukkoView2.Models.SharedMisc
{
	internal class CommentInfo
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コメント用コンストラクター
		// --------------------------------------------------------------------
		public CommentInfo(String message, Int32 yukariSize, Color color)
		{
			Command = String.Empty;
			Message = message.Trim();
			YukariSize = yukariSize;
			Color = color;

			InitialTick = Environment.TickCount;
		}

		// --------------------------------------------------------------------
		// コマンド用コンストラクター
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		public CommentInfo(String command, String message)
		{
			Command = command;
			Message = message;

			InitialTick = Environment.TickCount;

			// 有効なコマンドか確認
			if (Command != Yv2Constants.COMMENT_COMMAND_REQUEST_LIST)
			{
				throw new Exception("未対応のコメントコマンドです：" + Command);
			}
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// 基本情報
		// --------------------------------------------------------------------

		// コマンド
		public String Command { get; }

		// コメント内容 or コマンドパラメーター
		public String Message { get; }

		// サイズ（ゆかり指定サイズ）
		// ゆかりでは「小=0、中=3、大=6、特大=9」
		// プログラムの都合上、小=1 とする
		private Int32 _yukariSize;
		public Int32 YukariSize
		{
			get => _yukariSize;
			private set
			{
				_yukariSize = Math.Clamp(value, 1, 9);
			}
		}

		// 色
		public Color Color { get; }

		// コメントを取得した時刻
		// 停止→開始で変更される
		public Int32 InitialTick { get; set; }

		// --------------------------------------------------------------------
		// 描画情報
		// --------------------------------------------------------------------

		// 描画用データ準備済かどうか
		public Boolean IsDrawDataPrepared { get; set; }

		// 描画用ジオメトリ
		public Geometry? MessageGeometry { get; set; }

		// 文字の中身のブラシ
		public Brush? Brush { get; set; }

		// 文字の縁の幅
		public Int32 EdgeWidth { get; set; }

		// 文字の縁
		public Pen? Pen { get; set; }

		// 移動速度 [px/s]
		public Int32 Speed { get; set; }

		// 描画位置
		public Int32 Left { get; set; }

		// 描画位置
		public Int32 Top { get; set; }

		// 表示される位置（縁取り込み）
		public Int32 Right
		{
			get => Left + Width;
		}

		// 表示される位置（縁取り込み）
		public Int32 Bottom
		{
			get => Top + Height;
		}

		// 表示されるサイズ（縁取り込み）
		public Int32 Width
		{
			get => (Int32)(MessageGeometry?.Bounds.Width ?? 0) + EdgeWidth;
		}

		// 表示されるサイズ（縁取り込み）
		public Int32 Height
		{
			get => (Int32)(MessageGeometry?.Bounds.Height ?? 0) + EdgeWidth;
		}

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
		// コマンドかどうか
		// --------------------------------------------------------------------
		public Boolean IsCommand()
		{
			return Command != String.Empty;
		}

		// --------------------------------------------------------------------
		// コマンドのオプションとしてのメッセージをフラグとして扱った場合の値を返す
		// --------------------------------------------------------------------
		public Boolean MessageAsFlag()
		{
			Int32.TryParse(Message, out Int32 flag);
			return flag != 0;
		}
	}
}
