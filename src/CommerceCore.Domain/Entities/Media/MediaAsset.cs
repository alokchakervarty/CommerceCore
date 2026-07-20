using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Media;

/// <summary>
/// A single uploaded asset (image, document, video) tracked centrally, regardless
/// of where it's used (product image, banner, blog featured image, ...). Entities
/// that reference media store a plain Url string for read simplicity; this table
/// exists so uploads can be managed, replaced, and cleaned up as a first-class
/// admin feature rather than scattered orphan files in blob storage.
/// </summary>
public class MediaAsset : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string ContentType { get; set; } = string.Empty;   // "image/jpeg", "application/pdf"
    public long FileSizeBytes { get; set; }
    public int? WidthPx { get; set; }
    public int? HeightPx { get; set; }

    public string? AltText { get; set; }
    public string? Folder { get; set; }   // logical grouping, e.g. "products", "banners"

    public Guid UploadedByUserId { get; set; }
}
