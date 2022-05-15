﻿// ============================================================================
// 
// ゆかり予約テーブル
// 
// ============================================================================

// ----------------------------------------------------------------------------
// ゆっこビュー 2 で使用するカラムのみ定義
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukkoView2.Models.Database
{
	[Table(TABLE_NAME_YUKARI_REQUEST)]
	internal class TYukariRequest
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// request.db 由来
		// --------------------------------------------------------------------

		// 予約 ID
		[Column(FIELD_NAME_YUKARI_REQUEST_ID)]
		public Int32 Id { get; set; }

		// 予約動画フルパス
		[Column(FIELD_NAME_YUKARI_REQUEST_PATH)]
		public String Path { get; set; } = String.Empty;

		// 予約者
		[Column(FIELD_NAME_YUKARI_REQUEST_SINGER)]
		public String? Singer { get; set; }

		// 予約コメント
		[Column(FIELD_NAME_YUKARI_REQUEST_COMMENT)]
		public String? Comment { get; set; }

		// 予約順
		[Column(FIELD_NAME_YUKARI_REQUEST_ORDER)]
		public Int32 Order { get; set; }

		// 再生状況
		[Column(FIELD_NAME_YUKARI_REQUEST_NOW_PLAYING)]
		public String? NowPlaying { get; set; }

		// シークレット予約
		[Column(FIELD_NAME_YUKARI_REQUEST_SECRET)]
		public Int32 Secret { get; set; }

		// キー
		[Column(FIELD_NAME_YUKARI_REQUEST_KEY_CHANGE)]
		public Int32 KeyChange { get; set; }

		// ====================================================================
		// public 定数
		// ====================================================================

		public const String TABLE_NAME_YUKARI_REQUEST = "requesttable";
		public const String FIELD_NAME_YUKARI_REQUEST_ID = "id";
		public const String FIELD_NAME_YUKARI_REQUEST_PATH = "fullpath";
		public const String FIELD_NAME_YUKARI_REQUEST_SINGER = "singer";
		public const String FIELD_NAME_YUKARI_REQUEST_COMMENT = "comment";
		public const String FIELD_NAME_YUKARI_REQUEST_ORDER = "reqorder";
		public const String FIELD_NAME_YUKARI_REQUEST_NOW_PLAYING = "nowplaying";
		public const String FIELD_NAME_YUKARI_REQUEST_SECRET = "secret";
		public const String FIELD_NAME_YUKARI_REQUEST_KEY_CHANGE = "keychange";
	}
}
