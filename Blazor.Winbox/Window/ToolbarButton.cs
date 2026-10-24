using System.Text.Json.Serialization;

namespace BlazorWinbox;

/// <summary>
/// A custom button displayed in the window title bar toolbar next to the built-in window controls.
/// <para>Assign buttons to <see cref="GlobalWindowOptions.Toolbar"/> from <see cref="WindowOptions"/> to add them to a single window,
/// or to the global options of <see cref="WindowProvider"/> to add them to every window.</para>
/// </summary>
public class ToolbarButton
{
    /// <summary>
    /// Unique identifier of the button. Generated automatically when not set.
    /// <para>Used to update or remove the button later with <see cref="WindowInstance.UpdateToolbarButton"/> and <see cref="WindowInstance.RemoveToolbarButton"/>.</para>
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Tooltip text shown when the button is hovered.
    /// </summary>
    public string Tooltip { get; set; }

    /// <summary>
    /// Icon image url (svg, png, data uri, etc.) rendered as the button background.
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Css classes of an icon font/glyph element rendered inside the button, e.g. "oi oi-plus" or "fa-solid fa-xmark".
    /// </summary>
    public string IconClass { get; set; }

    /// <summary>
    /// Css size of <see cref="Icon"/>, e.g. "20px" or "18px 18px".
    /// <para>Default is 16px 16px</para>
    /// </summary>
    public string IconSize { get; set; }

    /// <summary>
    /// Extra css classes of the button element.
    /// </summary>
    public string CssClass { get; set; }

    /// <summary>
    /// Background color of the button (any css color).
    /// </summary>
    public string Background { get; set; }

    /// <summary>
    /// Foreground (icon) color of the button (any css color). Useful for icon fonts.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Raw css declarations applied to the button element, e.g. "opacity: .8; border-radius: 4px;".
    /// </summary>
    public string Style { get; set; }

    /// <summary>
    /// Hides the button when false.
    /// <para>Default is true</para>
    /// </summary>
    public bool? Visible { get; set; }

    /// <summary>
    /// Disables the button when true, so clicks are ignored.
    /// <para>Default is false</para>
    /// </summary>
    public bool? Disabled { get; set; }

    /// <summary>
    /// Zero-based position inside the toolbar controls container (0 is the leftmost position next to the window title).
    /// <para>When not set, buttons are placed in declaration order starting from the leftmost position.</para>
    /// </summary>
    public int? Index { get; set; }

    /// <summary>
    /// Action invoked when the button is clicked.
    /// </summary>
    [JsonIgnore] public Func<Task> OnClick { get; set; }
}
