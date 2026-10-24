namespace BlazorWinbox;

/// <summary>
/// Describes how a window docks (snaps) to the other windows while it is moved or resized.
/// <para>Docking is enabled per window by assigning this object to <see cref="GlobalWindowOptions.Docking"/>.
/// Assign it to the global options of <see cref="WindowProvider"/> to enable docking for every window.</para>
/// </summary>
public class WindowDockingOptions
{
    /// <summary>
    /// Enables the docking behavior.
    /// <para>Default is true</para>
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The maximum distance (in pixels) between window edges at which the window snaps to another window.
    /// <para>Default is 10</para>
    /// </summary>
    public double SnapDistance { get; set; } = 10;

    /// <summary>
    /// Shows dashed guide lines at the edges which the window docks to.
    /// <para>Default is true</para>
    /// </summary>
    public bool ShowGuides { get; set; } = true;

    /// <summary>
    /// Any css color of the guide lines (e.g. "#22c55e", "rgb(34, 197, 94)", "rgba(34, 197, 94, .8)").
    /// <para>Default is a light blue color (rgba(96, 165, 250, 0.95))</para>
    /// </summary>
    public string GuideColor { get; set; }

    /// <summary>
    /// Thickness of the guide lines in pixels.
    /// <para>Default is 1</para>
    /// </summary>
    public double GuideThickness { get; set; } = 1;

    /// <summary>
    /// Docking group. A window docks only to windows with the same group, so unrelated windows can be kept apart.
    /// <para>Default is null: windows without a group dock to other windows without a group</para>
    /// </summary>
    public string Group { get; set; }
}
