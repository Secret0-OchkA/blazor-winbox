namespace BlazorWinbox;

/// <summary>
/// Customizes one of the built-in window title bar controls (minimize, maximize, fullscreen and close).
/// <para>Assign it to <see cref="GlobalWindowOptions.Minimize"/>, <see cref="GlobalWindowOptions.Maximize"/>,
/// <see cref="GlobalWindowOptions.Fullscreen"/> or <see cref="GlobalWindowOptions.Close"/> either on a single window
/// or on the global options of <see cref="WindowProvider"/>.</para>
/// <para>The control keeps its built-in behavior, only its look and availability are changed.</para>
/// </summary>
public class WindowControlButton
{
    /// <summary>
    /// Tooltip text shown when the control is hovered. The built-in tooltip is kept when not set.
    /// </summary>
    public string Tooltip { get; set; }

    /// <summary>
    /// Replaces the built-in icon with an image url (svg, png, data uri, etc.).
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Css classes of an icon font element rendered instead of the built-in icon, e.g. "oi oi-minus" or "fa-solid fa-xmark".
    /// </summary>
    public string IconClass { get; set; }

    /// <summary>
    /// Css size of <see cref="Icon"/>, e.g. "18px 18px".
    /// </summary>
    public string IconSize { get; set; }

    /// <summary>
    /// Extra css classes of the control element.
    /// </summary>
    public string CssClass { get; set; }

    /// <summary>
    /// Background color of the control (any css color).
    /// </summary>
    public string Background { get; set; }

    /// <summary>
    /// Foreground (icon) color of the control (any css color). Useful for icon fonts.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Raw css declarations applied to the control element, e.g. "opacity: .8; border-radius: 4px;".
    /// </summary>
    public string Style { get; set; }

    /// <summary>
    /// Hides the control when false.
    /// <para>Default is true</para>
    /// </summary>
    public bool? Visible { get; set; }

    /// <summary>
    /// Ignores clicks on the control when true, so the built-in action is not performed.
    /// <para>Default is false</para>
    /// </summary>
    public bool? Disabled { get; set; }
}
