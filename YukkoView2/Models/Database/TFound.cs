﻿// ============================================================================
// 
// 検出ファイルリストテーブル
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using YukkoView2.Models.SharedMisc;

namespace YukkoView2.Models.Database
{
	[Table(TABLE_NAME_FOUND)]
	internal class TFound
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// TFound 独自
		// --------------------------------------------------------------------

		// ユニーク ID
		[Key]
		[Column(FIELD_NAME_FOUND_UID)]
		public Int64 Uid { get; set; }

		// フルパス
		[Column(FIELD_NAME_FOUND_PATH)]
		public String Path { get; set; } = String.Empty;

		// フォルダー
		[Column(FIELD_NAME_FOUND_FOLDER)]
		public String Folder { get; set; } = String.Empty;

		// 親フォルダー（追加ボタンをクリックした時のフォルダー）（項目削除用）
		[Column(FIELD_NAME_FOUND_PARENT_FOLDER)]
		public String ParentFolder { get; set; } = String.Empty;

		// 頭文字（通常は番組名の頭文字、通常はひらがな（濁点なし））
		[Column(FIELD_NAME_FOUND_HEAD)]
		public String? Head { get; set; }

		// カラオケ動画制作者
		[Column(FIELD_NAME_FOUND_WORKER)]
		public String? Worker { get; set; }

		// トラック情報
		[Column(FIELD_NAME_FOUND_TRACK)]
		public String? Track { get; set; }

		// スマートトラック：オンボーカルがあるか
		[Column(FIELD_NAME_FOUND_SMART_TRACK_ON)]
		public Boolean SmartTrackOnVocal { get; set; }

		// スマートトラック：オフボーカルがあるか
		[Column(FIELD_NAME_FOUND_SMART_TRACK_OFF)]
		public Boolean SmartTrackOffVocal { get; set; }

		// 備考
		[Column(FIELD_NAME_FOUND_COMMENT)]
		public String? Comment { get; set; }

		// ファイル最終更新日時（修正ユリウス日）
		[Column(FIELD_NAME_FOUND_LAST_WRITE_TIME)]
		public Double LastWriteTime { get; set; }

		// ファイルサイズ
		[Column(FIELD_NAME_FOUND_FILE_SIZE)]
		public Int64 FileSize { get; set; }

		// --------------------------------------------------------------------
		// TSong
		// --------------------------------------------------------------------

		// 楽曲 ID
		[Column(FIELD_NAME_SONG_ID)]
		public String? SongId { get; set; }

		// 楽曲名
		[Column(FIELD_NAME_SONG_NAME)]
		public String? SongName { get; set; }

		// 楽曲フリガナ
		[Column(FIELD_NAME_SONG_RUBY)]
		public String? SongRuby { get; set; }

		// 摘要
		[Column(FIELD_NAME_SONG_OP_ED)]
		public String? SongOpEd { get; set; }

		// --------------------------------------------------------------------
		// TSong + TTieUp
		// --------------------------------------------------------------------

		// リリース日（修正ユリウス日）：TTieUp の値を優先
		[Column(FIELD_NAME_SONG_RELEASE_DATE)]
		public Double SongReleaseDate { get; set; }

		// カテゴリー名：TTieUp の値を優先
		[Column(FIELD_NAME_FOUND_CATEGORY_NAME)]
		public String? Category { get; set; }

		// --------------------------------------------------------------------
		// TPerson 由来
		// --------------------------------------------------------------------

		// 歌手名
		[Column(FIELD_NAME_FOUND_ARTIST_NAME)]
		public String? ArtistName { get; set; }

		// 歌手フリガナ
		[Column(FIELD_NAME_FOUND_ARTIST_RUBY)]
		public String? ArtistRuby { get; set; }

		// 作詞者名
		[Column(FIELD_NAME_FOUND_LYRIST_NAME)]
		public String? LyristName { get; set; }

		// 作詞者フリガナ
		[Column(FIELD_NAME_FOUND_LYRIST_RUBY)]
		public String? LyristRuby { get; set; }

		// 作曲者名
		[Column(FIELD_NAME_FOUND_COMPOSER_NAME)]
		public String? ComposerName { get; set; }

		// 作曲者フリガナ
		[Column(FIELD_NAME_FOUND_COMPOSER_RUBY)]
		public String? ComposerRuby { get; set; }

		// 編曲者名
		[Column(FIELD_NAME_FOUND_ARRANGER_NAME)]
		public String? ArrangerName { get; set; }

		// 編曲者フリガナ
		[Column(FIELD_NAME_FOUND_ARRANGER_RUBY)]
		public String? ArrangerRuby { get; set; }

		// --------------------------------------------------------------------
		// TTieUp
		// --------------------------------------------------------------------

		// タイアップ ID
		// NEBULA で追加された項目
		[Column(FIELD_NAME_TIE_UP_ID)]
		public String? TieUpId { get; set; }

		// タイアップ名
		// ファイル名から供給される場合もあるため、TieUpId == null でも TieUpName が設定されていることはあり得る
		[Column(FIELD_NAME_FOUND_TIE_UP_NAME)]
		public String? TieUpName { get; set; }

		// タイアップフリガナ
		[Column(FIELD_NAME_TIE_UP_RUBY)]
		public String? TieUpRuby { get; set; }

		// 年齢制限（○歳以上対象）
		[Column(FIELD_NAME_TIE_UP_AGE_LIMIT)]
		public Int32 TieUpAgeLimit { get; set; } = Yv2Constants.AGE_LIMIT_DEFAULT;

		// --------------------------------------------------------------------
		// TTieUpGroup
		// --------------------------------------------------------------------

		// タイアップグループ名
		[Column(FIELD_NAME_TIE_UP_GROUP_NAME)]
		public String? TieUpGroupName { get; set; }

		// タイアップグループフリガナ
		[Column(FIELD_NAME_TIE_UP_GROUP_RUBY)]
		public String? TieUpGroupRuby { get; set; }

		// --------------------------------------------------------------------
		// TMaker
		// --------------------------------------------------------------------

		// 制作会社名
		[Column(FIELD_NAME_MAKER_NAME)]
		public String? MakerName { get; set; }

		// 制作会社フリガナ
		[Column(FIELD_NAME_MAKER_RUBY)]
		public String? MakerRuby { get; set; }

		// --------------------------------------------------------------------
		// TTag
		// --------------------------------------------------------------------

		// タグ名
		[Column(FIELD_NAME_TAG_NAME)]
		public String? TagName { get; set; }

		// タグフリガナ
		[Column(FIELD_NAME_TAG_RUBY)]
		public String? TagRuby { get; set; }

		// ====================================================================
		// public 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// TFound
		// --------------------------------------------------------------------

		public const String TABLE_NAME_FOUND = "t_found";
		public const String FIELD_NAME_FOUND_UID = "found_uid";
		public const String FIELD_NAME_FOUND_PATH = "found_path";
		public const String FIELD_NAME_FOUND_FOLDER = "found_folder";
		public const String FIELD_NAME_FOUND_PARENT_FOLDER = "found_parent_folder";
		public const String FIELD_NAME_FOUND_HEAD = "found_head";
		public const String FIELD_NAME_FOUND_TITLE_RUBY = "found_title_ruby";
		public const String FIELD_NAME_FOUND_WORKER = "found_worker";
		public const String FIELD_NAME_FOUND_TRACK = "found_track";
		public const String FIELD_NAME_FOUND_SMART_TRACK_ON = "found_smart_track_on";
		public const String FIELD_NAME_FOUND_SMART_TRACK_OFF = "found_smart_track_off";
		public const String FIELD_NAME_FOUND_COMMENT = "found_comment";
		public const String FIELD_NAME_FOUND_LAST_WRITE_TIME = "found_last_write_time";
		public const String FIELD_NAME_FOUND_FILE_SIZE = "found_file_size";
		public const String FIELD_NAME_FOUND_ARTIST_NAME = "song_artist";   // ニコカラりすたーとの互換性維持
		public const String FIELD_NAME_FOUND_ARTIST_RUBY = "found_artist_ruby";
		public const String FIELD_NAME_FOUND_LYRIST_NAME = "found_lyrist_name";
		public const String FIELD_NAME_FOUND_LYRIST_RUBY = "found_lyrist_ruby";
		public const String FIELD_NAME_FOUND_COMPOSER_NAME = "found_composer_name";
		public const String FIELD_NAME_FOUND_COMPOSER_RUBY = "found_composer_ruby";
		public const String FIELD_NAME_FOUND_ARRANGER_NAME = "found_arranger_name";
		public const String FIELD_NAME_FOUND_ARRANGER_RUBY = "found_arranger_ruby";
		public const String FIELD_NAME_FOUND_TIE_UP_NAME = "program_name";  // ニコカラりすたーとの互換性維持
		public const String FIELD_NAME_FOUND_CATEGORY_NAME = "program_category";    // ニコカラりすたーとの互換性維持

		// --------------------------------------------------------------------
		// TSong
		// --------------------------------------------------------------------

		public const String FIELD_PREFIX_SONG = "song_";
		public const String FIELD_NAME_SONG_ID = FIELD_PREFIX_SONG + Yv2Constants.FIELD_SUFFIX_ID;
		public const String FIELD_NAME_SONG_NAME = FIELD_PREFIX_SONG + Yv2Constants.FIELD_SUFFIX_NAME;
		public const String FIELD_NAME_SONG_RUBY = FIELD_PREFIX_SONG + Yv2Constants.FIELD_SUFFIX_RUBY;
		public const String FIELD_NAME_SONG_RELEASE_DATE = FIELD_PREFIX_SONG + Yv2Constants.FIELD_SUFFIX_RELEASE_DATE;
		public const String FIELD_NAME_SONG_OP_ED = FIELD_PREFIX_SONG + Yv2Constants.FIELD_SUFFIX_OP_ED;

		// --------------------------------------------------------------------
		// TTieUp
		// --------------------------------------------------------------------

		public const String FIELD_PREFIX_TIE_UP = "tie_up_";
		public const String FIELD_NAME_TIE_UP_ID = FIELD_PREFIX_TIE_UP + Yv2Constants.FIELD_SUFFIX_ID;
		public const String FIELD_NAME_TIE_UP_RUBY = FIELD_PREFIX_TIE_UP + Yv2Constants.FIELD_SUFFIX_RUBY;
		public const String FIELD_NAME_TIE_UP_AGE_LIMIT = FIELD_PREFIX_TIE_UP + Yv2Constants.FIELD_SUFFIX_AGE_LIMIT;

		// --------------------------------------------------------------------
		// TTieUpGroup
		// --------------------------------------------------------------------

		public const String FIELD_PREFIX_TIE_UP_GROUP = "tie_up_group_";
		public const String FIELD_NAME_TIE_UP_GROUP_NAME = FIELD_PREFIX_TIE_UP_GROUP + Yv2Constants.FIELD_SUFFIX_NAME;
		public const String FIELD_NAME_TIE_UP_GROUP_RUBY = FIELD_PREFIX_TIE_UP_GROUP + Yv2Constants.FIELD_SUFFIX_RUBY;

		// --------------------------------------------------------------------
		// TMaker
		// --------------------------------------------------------------------

		public const String FIELD_PREFIX_MAKER = "maker_";
		public const String FIELD_NAME_MAKER_NAME = FIELD_PREFIX_MAKER + Yv2Constants.FIELD_SUFFIX_NAME;
		public const String FIELD_NAME_MAKER_RUBY = FIELD_PREFIX_MAKER + Yv2Constants.FIELD_SUFFIX_RUBY;

		// --------------------------------------------------------------------
		// TTag
		// --------------------------------------------------------------------

		public const String FIELD_PREFIX_TAG = "tag_";
		public const String FIELD_NAME_TAG_NAME = FIELD_PREFIX_TAG + Yv2Constants.FIELD_SUFFIX_NAME;
		public const String FIELD_NAME_TAG_RUBY = FIELD_PREFIX_TAG + Yv2Constants.FIELD_SUFFIX_RUBY;
	}
}
