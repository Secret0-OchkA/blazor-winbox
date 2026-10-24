using System.Reflection;

namespace BlazorWinbox;

/// <summary>
/// All window options that contains only data properties (does not contains Actions or Funcs) so it can be serialized to javascript object by the framework
/// </summary>
public class BasicWindowOptions : GlobalWindowOptions
{
    // configuration:
    /// <summary>
    /// query elements by context or just to identify the corresponding window instance. If no ID was set it will automatically create one for you.
    /// </summary>
    public string Id { get; set; }

    // appearance:
    /// <summary>
    /// The window title.
    /// <para>This is overridden by Title in Window component</para>
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Title bar height
    /// </summary>
    public double? Header { get; set; }
    /// <summary>
    /// Make the titlebar icon visible and set the image source url.
    /// </summary>
    public string Icon { get; set; }

    // initial state:
    /// <summary>
    /// Automatically toggles the window into maximized state when created.
    /// <para>Default is false</para>
    /// </summary>
    public bool? Max { get; set; } = false;
    /// <summary>
    /// Automatically toggles the window into minimized state when created.
    /// <para>Default is false</para>
    /// </summary>
    public bool? Min { get; set; } = false;
    /// <summary>
    /// Automatically toggles the window into hidden state when created.
    /// <para>Default is false</para>
    /// </summary>
    public bool? Hidden { get; set; } = false;

    public string OnMoveHandlerName { get; set; }
    public string OnShowHandlerName { get; set; }
    public string OnHideHandlerName { get; set; }
    public string OnFocusHandlerName { get; set; }
    public string OnBlurHandlerName { get; set; }
    public string OnResizeHandlerName { get; set; }
    public string OnFullScreenHandlerName { get; set; }
    public string OnMaximizeHandlerName { get; set; }
    public string OnMinimizeHandlerName { get; set; }
    public string OnRestoreHandlerName { get; set; }

    internal static BasicWindowOptions CreateCopy(BasicWindowOptions basicWindowOptions)
    {
        ArgumentNullException.ThrowIfNull(basicWindowOptions, nameof(basicWindowOptions));

        BasicWindowOptions xReVal = new();
        foreach (PropertyInfo iProp in typeof(BasicWindowOptions).GetProperties())
        {
            if (iProp.CanWrite)
            {
                iProp.SetValue(xReVal, iProp.GetValue(basicWindowOptions, null), null);
            }
        }

        return xReVal;
    }
}
