using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminPanel.Domain.Entities;

[Table("Game")]
public class Game
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    public ICollection<GameConfig> GameConfigs { get; set; } = new List<GameConfig>();

    [Column("title")]
    [MaxLength(256)]
    public string? Title { get; set; }

    [Column("path")]
    [MaxLength(256)]
    public string? Path { get; set; }

    [Column("identifier")]
    [MaxLength(128)]
    public string? Identifier { get; set; }

    [Column("provider")]
    [MaxLength(128)]
    public string? Provider { get; set; }

    [Column("producer")]
    [MaxLength(128)]
    public string? Producer { get; set; }

    [Column("category")]
    [MaxLength(128)]
    public string? Category { get; set; }

    [Column("tags")]
    [MaxLength(128)]
    public string? Tags { get; set; }

    [Column("source")]
    public int? Source { get; set; }

    [Column("table_id")]
    [MaxLength(64)]
    public string? TableId { get; set; }

    [Column("tid")]
    [MaxLength(64)]
    public string? Tid { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("has_freespins")]
    public bool HasFreespins { get; set; }

    [Column("rtp")]
    public double? Rtp { get; set; }

    [Column("desktop")]
    public bool Desktop { get; set; }

    [Column("mobile")]
    public bool Mobile { get; set; }

    [Column("is_new")]
    public bool IsNew { get; set; }

    [Column("is_featured")]
    public bool IsFeatured { get; set; }

    [Column("feature_group")]
    [MaxLength(200)]
    public string? FeatureGroup { get; set; }

    [Column("volatility_rating")]
    [MaxLength(200)]
    public string? VolatilityRating { get; set; }

    [Column("line")]
    public int? Line { get; set; }

    [Column("hit_rate")]
    public double? HitRate { get; set; }

    [Column("ways")]
    public int? Ways { get; set; }

    [Column("bg_img_url")]
    [MaxLength(300)]
    public string? BackgroundImageUrl { get; set; }

    [Column("img_url")]
    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    [Column("cover_img_url")]
    [MaxLength(300)]
    public string? CoverImageUrl { get; set; }

    [Column("thumbnail_url")]
    [MaxLength(300)]
    public string? ThumbnailUrl { get; set; }

    [Column("recomended")]
    public bool Recomended { get; set; }

    [Column("fresh")]
    public bool Fresh { get; set; }

    [Column("popular")]
    public bool Popular { get; set; }

    [Column("weight")]
    public int? Weight { get; set; }

    [Column("house_edge", TypeName = "decimal(10,5)")]
    public decimal? HouseEdge { get; set; }

    [Column("display")]
    public bool Display { get; set; }

    [Column("status")]
    public int? Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("huge_profit_at")]
    public int? HugeProfitAt { get; set; }

    [Column("demo")]
    public long? Demo { get; set; }

    [Column("show_rtp")]
    public bool ShowRtp { get; set; }

    [Column("bonus_buy")]
    public bool BonusBuy { get; set; }

    [Column("tab")]
    [MaxLength(45)]
    public string? Tab { get; set; }

    [Column("released_at")]
    public DateTime? ReleasedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("ertp")]
    public double? Ertp { get; set; }
}

