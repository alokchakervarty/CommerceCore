namespace CommerceCore.Domain.Enums;

/// <summary>
/// Controls how a dynamic Attribute is captured and rendered on the frontend,
/// and whether it participates in variant generation (e.g. Color/Size) or is
/// purely informational (e.g. Fragrance Notes, Material).
/// </summary>
public enum AttributeInputType
{
    Text = 0,
    Number = 1,
    Boolean = 2,
    Select = 3,       // single choice from AttributeValue list
    MultiSelect = 4,   // multiple choices from AttributeValue list
    Color = 5,         // choice from AttributeValue list, rendered as swatches
    Date = 6
}
